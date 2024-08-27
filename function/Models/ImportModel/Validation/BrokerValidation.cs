using FluentValidation;
using AHI.Broker.Function.Constant;
using AHI.Broker.Function.Service.Abstraction;
using ValidationMessage = AHI.Broker.Function.Constant.MessageConstants.FluentValidation;

namespace AHI.Broker.Function.Model.ImportModel.Validation
{
    public class BrokerValidation : AbstractValidator<BrokerModel>
    {
        public BrokerValidation(ISystemContext context)
        {
            RuleFor(x => x.Name).Cascade(CascadeMode.StopOnFirstFailure)
                                .NotEmpty().WithMessage(ValidationMessage.REQUIRED)
                                .MaximumLength(255).WithMessage(ValidationMessage.MAX_LENGTH)
                                .SetValidator(new MatchRegex(RegexConfig.GENERAL_RULE, RegexConfig.GENERAL_DESCRIPTION, context));

            RuleFor(x => x.Type).NotEmpty().WithMessage(ValidationMessage.REQUIRED);
        }
    }
}
