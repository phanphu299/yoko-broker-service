using System.Threading.Tasks;
using System.Collections.Generic;
using AHI.Broker.Function.Service.Abstraction;
using AHI.Infrastructure.MultiTenancy.Abstraction;
using Microsoft.Extensions.Configuration;
using Dapper;
using System.Linq;
using System;
using Microsoft.Azure.WebJobs;
using System.Data;
using Microsoft.Data.SqlClient;
using AHI.Broker.Function.Model.ExportModel;
using System.Net.Http;
using AHI.Broker.Function.Model.SearchModel;
using AHI.Broker.Function.FileParser.Abstraction;
using AHI.Broker.Function.FileParser.Constant;
using AHI.Broker.Function.Extension;
using AHI.Infrastructure.Export.Builder;
using AHI.Infrastructure.SharedKernel.Extension;
using AHI.Broker.Function.Constant;
using AHI.Infrastructure.MultiTenancy.Extension;
using ValidationMessage = AHI.Broker.Function.Constant.MessageConstants.FluentValidation;

namespace AHI.Broker.Function.Service
{
    public class BrokerExportHandler : IExportHandler
    {
        private const string MODEL_NAME = "Broker";
        private readonly IConfiguration _configuration;
        private readonly ITenantContext _tenantContext;
        private readonly IStorageService _storageService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IParserContext _context;
        private readonly JsonExportBuilder<BrokerModel> _builder;

        public BrokerExportHandler(IConfiguration configuration,
                                   ITenantContext tenantContext,
                                   IStorageService storageService,
                                   IHttpClientFactory factory,
                                   IParserContext context,
                                   JsonExportBuilder<BrokerModel> builder)
        {
            _tenantContext = tenantContext;
            _storageService = storageService;
            _configuration = configuration;
            _httpClientFactory = factory;
            _context = context;
            _builder = builder;
        }
        public async Task<string> HandleAsync(ExecutionContext context, IEnumerable<string> ids)
        {
            var brokers = await GetBrokersAsync(ids);

            return await HandleUploadContentAsync(brokers);
        }

        private async Task<IEnumerable<BrokerModel>> GetBrokersAsync(IEnumerable<string> ids)
        {
            var connectionString = _configuration["ConnectionStrings:Default"].BuildConnectionString(_configuration, _tenantContext.ProjectId);

            var typeInfos = await GetLookupAsync();

            using (var connection = new SqlConnection(connectionString))
            {
                var schemas = await GetSchemaAsync(typeInfos.Select(info => info.Id), connection);
                var result = await QueryBrokersAsync(connection, ids);
                await connection.CloseAsync();

                if (!result.Any())
                    throw new InvalidOperationException(ValidationMessage.EXPORT_NOT_FOUND);

                SetTypeName(result, typeInfos);
                StandardizeSettingBySchema(result, schemas);

                return result;
            }
        }

        private Task<IEnumerable<BrokerModel>> QueryBrokersAsync(IDbConnection connection, IEnumerable<string> ids)
        {
            var query = @$"SELECT 
                                b.id, b.name, b.type AS TypeId, d.content
                            FROM brokers b WITH(NOLOCK)
                            JOIN broker_details d WITH(NOLOCK) ON b.id = d.broker_id
                            WHERE b.id IN @Ids AND b.deleted = 0";

            return connection.QueryAsync<BrokerModel, string, BrokerModel>(
                query,
                (broker, content) =>
                {
                    broker.Settings = content.JsonDeserialize<IDictionary<string, object>>();
                    return broker;
                },
                new { Ids = ids },
                splitOn: "content", commandTimeout: 600);
        }

        private async Task<string> HandleUploadContentAsync(IEnumerable<BrokerModel> brokers)
        {
            var timezone_offset = GetTimeZoneInfo();

            _builder.SetZipEntryNameBuilder(x =>
            {
                var timestamp = DateTime.UtcNow.ToTimestamp(timezone_offset);
                return x.Name.CreateJsonFileName(null, timestamp, MODEL_NAME.Length);
            });

            byte[] data = _builder.BuildContent(brokers);

            string fileTimestamp = DateTime.UtcNow.ToTimestamp(timezone_offset);
            string fileName;
            if (brokers.Count() == 1)
            {
                var broker = brokers.First();
                fileName = broker.Name.CreateJsonFileName(null, fileTimestamp);
            }
            else
            {
                fileName = MODEL_NAME.CreateZipFileName(fileTimestamp);
            }

            return await _storageService.UploadAsync($"sta/files/temp/exports", fileName, data);
        }

        private async Task<IEnumerable<BrokerSchema>> GetSchemaAsync(IEnumerable<string> TypeIds, IDbConnection connection)
        {
            var query = @"SELECT
                            [schemas].[id]
                            ,[schemas].[type]
                            ,[schema_details].[id]
                            ,[schema_details].[key]
                            ,[schema_details].[data_type] AS DataType
                            ,[schema_detail_options].[id]
                            ,[schema_detail_options].[name]
                        FROM [schemas] WITH(NOLOCK)
                        LEFT JOIN [schema_details] WITH(NOLOCK) ON [schema_details].[schema_id] = [schemas].[id] AND [schema_details].[is_readonly] = 0
                        LEFT JOIN [schema_detail_options] WITH(NOLOCK) ON [schema_detail_options].[schema_detail_id] = [schema_details].[id]
                        WHERE [schemas].[deleted] = 0 AND [schemas].[type] IN @Types";

            var queryResult = new Dictionary<Guid, BrokerSchema>();

            _ = await connection.QueryAsync<Guid, string, SchemaDetail, SchemaOption, BrokerSchema>(
                    query,
                    (id, type, detail, option) =>
                    {
                        if (!queryResult.TryGetValue(id, out var existingSchema))
                            existingSchema = BrokerSchema.CreateSchema(id, type);

                        var existingDetail = existingSchema.Details.FirstOrDefault(d => d.Id == detail.Id) ?? detail;

                        existingDetail.AddOption(option);
                        existingSchema.AddDetail(existingDetail);
                        _ = queryResult.TryAdd(existingSchema.Id, existingSchema);
                        return existingSchema;
                    }
                    , new { Types = TypeIds }
                    , splitOn: "id,type,id,id", commandTimeout: 600);

            return queryResult.Values;
        }

        private async Task<IEnumerable<TypeInfo>> GetLookupAsync()
        {
            var query = new FilteredSearchQuery(
                new SearchFilter("lookupTypeCode.ToLower()", "broker"),
                new SearchFilter("active", "true", queryType: "boolean")
            );
            var client = _httpClientFactory.CreateClient(HttpClientNames.CONFIGURATION, _tenantContext);
            var response = await client.SearchAsync<TypeInfo>($"cnm/lookups/search", query);
            return response.Data;
        }

        private void SetTypeName(IEnumerable<BrokerModel> brokers, IEnumerable<TypeInfo> typeInfos)
        {
            foreach (var broker in brokers)
            {
                broker.Type = typeInfos.FirstOrDefault(info => info.Id == broker.TypeId)?.Name;
            }
        }

        private void StandardizeSettingBySchema(IEnumerable<BrokerModel> brokers, IEnumerable<BrokerSchema> schemas)
        {
            foreach (var broker in brokers)
            {
                var schema = schemas.FirstOrDefault(schema => schema.Type == broker.TypeId);
                if (schema != null)
                    schema.StandardizeSetting(broker);
            }
        }

        private string GetTimeZoneInfo()
        {
            var timezone_offset = _context.GetContextFormat(ContextFormatKey.DATETIMEOFFSET) ?? DateTimeExtensions.DEFAULT_DATETIME_OFFSET;
            return DateTimeExtensions.ToValidOffset(timezone_offset);
        }

        class TypeInfo
        {
            public string Id { get; set; }
            public string Name { get; set; }
        }
    }

    static class SchemaAddListExtension
    {
        public static void AddDetail(this BrokerSchema schema, SchemaDetail detail)
        {
            if (schema is null || detail is null)
                return;

            if (schema.Details.Any(d => d.Id == detail.Id))
                return;

            schema.Details.Add(detail);
        }

        public static void AddOption(this SchemaDetail detail, SchemaOption option)
        {
            if (detail is null || option is null)
                return;

            detail.Options.Add(option);
        }
    }
}
