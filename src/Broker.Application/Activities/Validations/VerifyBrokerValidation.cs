using Broker.Application.Constant;
using Broker.Application.Handler.Command;
using FluentValidation;

namespace Broker.Application.Broker.Validation
{
    public class VerifyBrokerValidation : AbstractValidator<VerifyBroker>
    {
        public VerifyBrokerValidation()
        {
            RuleFor(x => x.Data).NotNull().WithMessage(MessageConstants.ATTRIBUTE_TYPE_REQUIRED);
        }
    }
}
