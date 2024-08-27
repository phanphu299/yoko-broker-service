using System.Collections.Generic;
using System.Linq;
using Broker.Application.FileRequest.Command;
using Broker.Application.Constant;
using FluentValidation;

namespace Broker.Application.FileRequest.Validation
{
    public class ImportFileValidation : AbstractValidator<ImportFile>
    {
        private IEnumerable<string> _allowedObject;
        public ImportFileValidation()
        {
            _allowedObject = typeof(FileEntityConstants).GetFields().Select(f => (string)f.GetValue(null));

            RuleFor(x => x.ObjectType).Cascade(CascadeMode.StopOnFirstFailure)
                .NotNull().WithMessage("Object type is required")
                .Must(IsObjectValid).WithMessage(x => x.ObjectType + " not supported");

            RuleFor(x => x.FileNames).NotEmpty().WithMessage("Filenames required")
                .DependentRules(() =>
                {
                    RuleForEach(x => x.FileNames).NotEmpty().WithMessage("Filename null or empty");
                });
        }
        private bool IsObjectValid(string objectType)
        {
            return _allowedObject.Contains(objectType);
        }
    }
}
