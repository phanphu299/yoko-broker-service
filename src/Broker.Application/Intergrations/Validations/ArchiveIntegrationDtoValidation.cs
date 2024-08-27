using Broker.Application.Constant;
using Broker.Application.Handler.Command.Model;
using FluentValidation;

namespace Broker.Application.Intergration.Validation
{
    public class ArchiveIntegrationDtoValidation : AbstractValidator<ArchiveIntegrationDto>
    {
        public ArchiveIntegrationDtoValidation()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage(MessageConstants.ATTRIBUTE_TYPE_REQUIRED);

            RuleFor(x => x.Type)
                .NotEmpty()
                .WithMessage(MessageConstants.ATTRIBUTE_TYPE_REQUIRED);

            RuleFor(x => x.Content)
                .NotEmpty()
                .When(x => x.Type != BrokerTypeConstants.REST_API)
                .WithMessage(MessageConstants.ATTRIBUTE_TYPE_REQUIRED);

            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage(MessageConstants.ATTRIBUTE_TYPE_REQUIRED);
        }
    }
}
