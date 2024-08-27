using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AHI.Infrastructure.Exception;
using Broker.Application.Handler.Command;
using Broker.Application.Service.Abstraction;
using Configuration.Application.Constant;
using FluentValidation.Results;

namespace Broker.Application.Service
{
    public class IntegrationValidator : IIntegrationValidator
    {
        private readonly ISchemaService _schemaService;
        public IntegrationValidator(ISchemaService schemaService)
        {
            _schemaService = schemaService;
        }

        public virtual Task<IEnumerable<ValidationFailure>> ValidateAsync(AddIntegration integration, CancellationToken token)
        {
            return ValidateAsync(integration.Type, integration.Details);
        }
        public virtual async Task<IEnumerable<ValidationFailure>> ValidateAsync(string type, IDictionary<string, object> payload)
        {
            var validationFailures = new List<ValidationFailure>();
            var schema = await _schemaService.FindByTypeAsync(new GetSchemaByType(type), CancellationToken.None);
            if (schema == null)
            {
                validationFailures.Add(new ValidationFailure(IntegrationTypeConstants.TYPE_NAME, ExceptionErrorCode.DetailCode.ERROR_VALIDATION_NOT_FOUND));
            }
            else
            {
                foreach (var item in schema.Details.Where(x => x.IsRequired))
                {
                    if (!payload.ContainsKey(item.Key))
                    {
                        validationFailures.Add(new ValidationFailure(item.Name, ExceptionErrorCode.DetailCode.ERROR_VALIDATION_REQUIRED));
                    }
                }
            }

            return validationFailures;
        }

        public virtual Task<IEnumerable<ValidationFailure>> ValidateAsync(UpdateIntegration integration, CancellationToken token)
        {
            return ValidateAsync(integration.Type, integration.Details);
        }
    }
}
