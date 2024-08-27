using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using AHI.Broker.Function.Constant;
using AHI.Broker.Function.FileParser.Abstraction;
using AHI.Broker.Function.Model.ImportModel;
using AHI.Infrastructure.Repository.Abstraction;

namespace AHI.Infrastructure.Repository
{
    public abstract class BaseBrokerPersistenceValidator : IBrokerPersistenceValidator
    {
        protected readonly IImportTrackingService _errorService;
        private IBrokerPersistenceValidator _next;

        public BaseBrokerPersistenceValidator(IDictionary<string, IImportTrackingService> errorHandlers)
        {
            _errorService = errorHandlers[MimeType.JSON];
        }

        public void SetNextValidator(IBrokerPersistenceValidator next)
        {
            _next = next;
        }

        public async Task<bool> ValidateAsync(BrokerModel broker, IDbConnection connection, IDbTransaction transaction)
        {
            if (CanApply(broker.Type))
            {
                // process data before validation (set default value, etc...)
                ProcessPreValidate(broker);

                // validate using broker schema
                var schema = await GetSchemaAsync(broker.Type, connection, transaction);
                if (!schema.Validate(broker, exception =>
                {
                    ProcessException(exception);
                }))
                    return false;

                // additional validation other than schema validation
                if (!ValidateAdditionalCondition(broker))
                    return false;

                // additional processing after validation
                ProcessPostValidate(broker, schema);

                return true;
            }
            else if (_next != null)
            {
                return await _next.ValidateAsync(broker, connection, transaction);
            }
            throw new ArgumentException($"Invalid type {broker.Type}");
        }

        private void ProcessException(System.Exception exception)
        {
            if (exception is FluentValidation.ValidationException fluentException)
            {
                foreach (var error in fluentException.Errors)
                {
                    _errorService.RegisterError(error.ErrorMessage, ErrorType.VALIDATING, error.FormattedMessagePlaceholderValues);
                }
            }
            else
            {
                _errorService.RegisterError(exception.Message, ErrorType.VALIDATING);
            }
        }

        private async Task<BrokerSchema> GetSchemaAsync(string brokerType, IDbConnection connection, IDbTransaction transaction)
        {
            var query = @"SELECT
                            [schema_details].[id]
                            ,[schema_details].[key]
                            ,[schema_details].[name]
                            ,[schema_details].[data_type] AS DataType
                            ,[schema_details].[regex]
                            ,[schema_details].[min_value] AS MinValue
                            ,[schema_details].[max_value] AS MaxValue
                            ,[schema_detail_options].[id]
                            ,[schema_detail_options].[name]
                        FROM [schema_details]
                        LEFT JOIN [schema_detail_options] ON [schema_detail_options].[schema_detail_id] = [schema_details].[id] AND [schema_details].[data_type] = 'select'
                        WHERE [schema_details].[deleted] = 0
                        AND [is_readonly] = 0
                        AND [schema_id] = (SELECT TOP 1 [id] FROM [schemas] WHERE [schemas].[type] = @Type AND [schemas].[deleted] = 0)";

            var queryResult = new Dictionary<Guid, SchemaDetail>();

            _ = await connection.QueryAsync<SchemaDetail, SchemaOption, SchemaDetail>(
                    query,
                    (detail, option) =>
                    {
                        if (queryResult.TryGetValue(detail.Id, out var existingDetail))
                        {
                            if (option != null)
                                existingDetail.Options.Add(option);
                        }
                        else
                        {
                            if (option != null)
                                detail.Options.Add(option);
                            queryResult[detail.Id] = detail;
                        }
                        return detail;
                    }
                    , new { Type = brokerType }
                    , transaction, commandTimeout: 600);
            return new BrokerSchema(queryResult.Values);
        }

        public abstract bool CanApply(string brokerType);
        public abstract void ProcessPreValidate(BrokerModel broker);
        public virtual bool ValidateAdditionalCondition(BrokerModel broker) => true;
        public abstract void ProcessPostValidate(BrokerModel broker, BrokerSchema schema);
    }
}