using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Broker.Application.Handler.Command;
using FluentValidation.Results;

namespace Broker.Application.Service.Abstraction
{
    public interface IIntegrationValidator
    {
        Task<IEnumerable<ValidationFailure>> ValidateAsync(AddIntegration integration, CancellationToken token);
        Task<IEnumerable<ValidationFailure>> ValidateAsync(UpdateIntegration integration, CancellationToken token);
    }
}
