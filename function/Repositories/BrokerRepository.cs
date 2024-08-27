using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using AHI.Infrastructure.Import.Abstraction;
using AHI.Infrastructure.MultiTenancy.Abstraction;
using Dapper;
using System.Data;
using AHI.Broker.Function.Constant;
using System;
using AHI.Broker.Function.FileParser.Abstraction;
using AHI.Broker.Function.Model.ImportModel;
using Microsoft.Data.SqlClient;
using System.Linq;
using AHI.Broker.Function.Model.SearchModel;
using System.Net.Http;
using Function.Service.Message;
using AHI.Infrastructure.Bus.ServiceBus.Abstraction;
using AHI.Broker.Function.Extension;
using AHI.Infrastructure.Repository.Abstraction;
using System.Data.Common;
using AHI.Infrastructure.SharedKernel.Extension;
using AHI.Infrastructure.MultiTenancy.Extension;
using ValidationMessage = AHI.Broker.Function.Constant.MessageConstants.FluentValidation;

namespace AHI.Infrastructure.Repository
{
    public class BrokerRepository : IBrokerRepository, IImportRepository<BrokerModel>
    {
        private readonly ErrorType _errorType = ErrorType.DATABASE;
        private readonly IConfiguration _configuration;
        private readonly ITenantContext _tenantContext;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IDomainEventDispatcher _domainEventDispatcher;
        private readonly IImportTrackingService _errorService;
        private readonly IBrokerPersistenceValidator _persistenceValidator;
        private readonly NameValidator _nameValidator;

        public BrokerRepository(IConfiguration configuration,
                                ITenantContext tenantContext,
                                IHttpClientFactory factory,
                                IDomainEventDispatcher domainEventDispatcher,
                                IBrokerPersistenceValidator validator,
                                IDictionary<string, IImportTrackingService> errorHandlers)
        {
            _configuration = configuration;
            _tenantContext = tenantContext;
            _httpClientFactory = factory;
            _domainEventDispatcher = domainEventDispatcher;
            _errorService = errorHandlers[MimeType.JSON];
            _persistenceValidator = validator;
            _nameValidator = new NameValidator("brokers", "name", new FilterCondition("deleted", "false"));
            _nameValidator.Seperator = ' ';
        }

        public async Task CommitAsync(IEnumerable<BrokerModel> source)
        {
            // if any error detected when parsing data in any sheet, discard all file
            if (_errorService.HasError)
                return;

            var broker = source.FirstOrDefault();
            if (broker == null)
                return;
                
            var connectionString = _configuration["ConnectionStrings:Default"].BuildConnectionString(_configuration, _tenantContext.ProjectId);

            var success = true;
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                using (var transaction = await connection.BeginTransactionAsync())
                {
                    try
                    {
                        success = await ValidateBrokerAsync(broker, connection, transaction);
                        if (success)
                            await UpsertBrokerAsync(broker, connection, transaction);
                    }
                    catch (DbException ex)
                    {
                        _errorService.RegisterError(ex.Message, _errorType);
                        success = false;
                    }
                    await (success ? transaction.CommitAsync() : transaction.RollbackAsync());
                }
                await connection.CloseAsync();
                if (success)
                {
                    await ProcessBrokerChangedAsync(broker.Id);
                }
            }
        }

        private async Task<bool> ValidateBrokerAsync(BrokerModel broker, IDbConnection connection, IDbTransaction transaction)
        {
            if (!await ValidateBrokerTypeAsync(broker))
                return false;

            broker.Name = await _nameValidator.ValidateDuplicateNameAsync(broker.Name, connection, transaction);
            await ValidateDeletedBrokerAsync(broker, connection, transaction);

            return await _persistenceValidator.ValidateAsync(broker, connection, transaction);
        }

        private Task UpsertBrokerAsync(BrokerModel broker, IDbConnection connection, IDbTransaction transaction)
        {

            var insertQuery = @"INSERT INTO brokers (id, name, type, status)
                                VALUES (@Id, @Name, @Type, @Status);
                                INSERT INTO broker_details (id, broker_id, content)
                                VALUES (@DetailId, @Id, @Content)";

            var updateQuery = @"UPDATE brokers SET type = @Type, status = @Status, deleted = 0
                                WHERE id = @Id;
                                UPDATE broker_details SET content = @Content
                                WHERE broker_id = @Id";

            return connection.ExecuteAsync(
                broker.ShouldReplace ? updateQuery : insertQuery,
                new
                {
                    Id = broker.Id,
                    Name = broker.Name,
                    Type = broker.Type,
                    Status = broker.Status,
                    DetailId = Guid.NewGuid(),
                    Content = broker.Settings.JsonSerialize()
                },
                transaction, commandTimeout: 600);
        }

        private Task ProcessBrokerChangedAsync(Guid brokerId)
        {
            BrokerChangedMessage message = new BrokerChangedMessage(brokerId, _tenantContext);
            return _domainEventDispatcher.SendAsync(message);
        }

        private async Task<bool> ValidateBrokerTypeAsync(BrokerModel broker)
        {
            var typeInfo = await GetLookupAsync(broker.Type);
            if (typeInfo is null)
            {
                _errorService.RegisterError(ValidationMessage.NOT_EXIST_OR_ACTIVE, validationInfo: new Dictionary<string, object>
                {
                    { "propertyName", "Type" },
                    { "propertyValue", broker.Type }
                });
                return false;
            }

            broker.Type = typeInfo.Id;
            return true;
        }

        private async Task<TypeInfo> GetLookupAsync(string lookupName)
        {
            var query = new FilteredSearchQuery(
                new SearchFilter("name.ToLower()", $"{lookupName}"),
                new SearchFilter("lookupTypeCode.ToLower()", "broker"),
                new SearchFilter("active", "true", queryType: "boolean")
            );
            try
            {
                var client = _httpClientFactory.CreateClient(HttpClientNames.CONFIGURATION, _tenantContext);
                var response = await client.SearchAsync<TypeInfo>($"cnm/lookups/search", query);

                return response.Data.FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }

        public async Task ValidateDeletedBrokerAsync(BrokerModel broker, IDbConnection connection, IDbTransaction transaction)
        {
            var query = "SELECT id FROM brokers WHERE name = @Name AND deleted = 1";
            var deletedId = await connection.QueryFirstOrDefaultAsync<Guid?>(query, new { Name = broker.Name }, transaction, commandTimeout: 600);

            broker.Id = deletedId ?? Guid.NewGuid();
            broker.ShouldReplace = deletedId.HasValue;
        }

        public Task CommitAsync(IEnumerable<BrokerModel> source, Guid correlationId)
        {
            throw new NotImplementedException();
        }

        class TypeInfo
        {
            public string Id { get; set; }
            public string Name { get; set; }
        }
    }
}
