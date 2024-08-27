using FluentValidation;

namespace AHI.Broker.Function.Model.ImportModel.Validation
{
    public class EventHubValidation : AbstractValidator<EvenHub>
    {
        public EventHubValidation()
        {
            RuleFor(x => x.Tier).Cascade(CascadeMode.StopOnFirstFailure)
                                .Must(name => !string.IsNullOrWhiteSpace(name)).WithMessage("Name required");
            RuleFor(x => x.ThroughputUnits).Must(ThroughputUnits => ThroughputUnits > 0);
            RuleFor(x => x.MaximumThroughputUnits).Must(MaximumThroughputUnits => MaximumThroughputUnits > 0);
        }
    }
}
