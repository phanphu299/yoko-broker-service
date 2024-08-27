using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AHI.Infrastructure.Exception;
using AHI.Infrastructure.Exception.Helper;
using Broker.Application.Handler.Command;
using Broker.Application.Service.Abstraction;

namespace Broker.Application.Service
{
    public class BrokerValidator : IBrokerValidator
    {
        private readonly ISchemaService _schemaService;
        public BrokerValidator(ISchemaService schemaService)
        {
            _schemaService = schemaService;
        }

        public Task ValidateAsync(AddBroker broker, CancellationToken token)
        {
            return ValidateAsync(broker.Type, broker.Details);
        }
        public async Task ValidateAsync(string type, IDictionary<string, object> payload)
        {
            var schema = await _schemaService.FindByTypeAsync(new GetSchemaByType(type), CancellationToken.None);
            if (schema == null)
            {
                throw EntityValidationExceptionHelper.GenerateException("Type", ExceptionErrorCode.DetailCode.ERROR_VALIDATION_NOT_FOUND);
            }
            var validationFailures = new List<FluentValidation.Results.ValidationFailure>();
            foreach (var item in schema.Details.Where(x => x.IsRequired))
            {
                var actualKey =
                    payload.Keys.FirstOrDefault(x => x.Equals(item.Key, StringComparison.InvariantCultureIgnoreCase));
                if (string.IsNullOrEmpty(actualKey))
                {
                    validationFailures.Add(new FluentValidation.Results.ValidationFailure(item.Name, "This property is required"));
                }
            }
            if (validationFailures.Any())
            {
                throw EntityValidationExceptionHelper.GenerateException(validationFailures);
            }
        }

        public Task ValidateAsync(UpdateBroker broker, CancellationToken token)
        {
            return ValidateAsync(broker.Type, broker.Details);
        }
    }
}
