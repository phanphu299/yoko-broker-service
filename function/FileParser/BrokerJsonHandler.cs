using System.Collections.Generic;
using AHI.Broker.Function.Model.ImportModel;
using Newtonsoft.Json;
using AHI.Broker.Function.Constant;
using FluentValidation;
using AHI.Broker.Function.FileParser.ErrorTracking.Abstraction;
using AHI.Infrastructure.Import.Handler;
using JsonConstant = AHI.Infrastructure.SharedKernel.Extension.Constant;
using AHI.Broker.Function.Extension;

namespace AHI.Broker.Function.FileParser
{
    public class BrokerJsonHandler : JsonFileHandler<BrokerModel>
    {
        private readonly IJsonTrackingService _errorService;
        private readonly IValidator<BrokerModel> _validator;
        public BrokerJsonHandler(IJsonTrackingService errorService, IValidator<BrokerModel> validator)
        {
            _errorService = errorService;
            _validator = validator;
        }

        protected override IEnumerable<BrokerModel> Parse(JsonTextReader reader)
        {
            // Read object
            var template = reader.ReadSingleObject<BrokerModel>(
                JsonConstant.JsonSerializer,
                e => _errorService.RegisterError($"Failed to parse json file. Detail: {e.Message}", ErrorType.PARSING)
            );

            if (template is null)
                yield break;

            // Validate object
            var validation = _validator.Validate(template);
            if (!validation.IsValid)
            {
                foreach (var error in validation.Errors)
                {
                    _errorService.RegisterError(error.ErrorMessage, ErrorType.VALIDATING, error.FormattedMessagePlaceholderValues);
                }
                yield break;
            }

            yield return template;
        }
    }
}
