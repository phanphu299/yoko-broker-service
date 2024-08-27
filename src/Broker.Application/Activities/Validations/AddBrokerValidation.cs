using AHI.Infrastructure.Exception;
using AHI.Infrastructure.Service.Tag.Model;
using Broker.Application.Constant;
using Broker.Application.Handler.Command;
using FluentValidation;

namespace Broker.Application.Broker.Validation
{
    public class AddBrokerValidation : AbstractValidator<AddBroker>
    {
        public AddBrokerValidation()
        {
            RuleFor(x => x.Type).NotNull().WithMessage(MessageConstants.ATTRIBUTE_TYPE_REQUIRED);
            RuleFor(x => x.Name).NotNull().WithMessage(MessageConstants.ATTRIBUTE_NAME_REQUIRED);
            RuleFor(x => x.Details).NotNull().WithMessage(MessageConstants.ATTRIBUTE_TYPE_REQUIRED);

            RuleForEach(x => x.Tags).SetValidator(
                new InlineValidator<UpsertTag> {
                    agValidator => agValidator.RuleFor(x => x.Key)
                                            .NotEmpty()
                                            .WithMessage(ExceptionErrorCode.DetailCode.ERROR_VALIDATION_REQUIRED)
                                            .MaximumLength(216)
                                            .WithMessage(ExceptionErrorCode.DetailCode.ERROR_VALIDATION_MAX_LENGTH)
                                            .Must(ContainsInvalidChar)
                                            .WithMessage(ExceptionErrorCode.DetailCode.ERROR_VALIDATION_INVALID)
                }
            );

            RuleForEach(x => x.Tags).SetValidator(
                new InlineValidator<UpsertTag> {
                    agValidator => agValidator.RuleFor(x => x.Value)
                                              .NotEmpty()
                                              .WithMessage(ExceptionErrorCode.DetailCode.ERROR_VALIDATION_REQUIRED)
                                              .MaximumLength(216)
                                              .WithMessage(ExceptionErrorCode.DetailCode.ERROR_VALIDATION_MAX_LENGTH)
                                              .Must(ContainsInvalidChar)
                                              .WithMessage(ExceptionErrorCode.DetailCode.ERROR_VALIDATION_INVALID)
                }
            );

        }
        private bool ContainsInvalidChar(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return true;
            return !input.Contains(':') && !input.Contains(';') && !input.Contains(',');
        }
    }
}
