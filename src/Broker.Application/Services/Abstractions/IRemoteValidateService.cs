using System.Threading.Tasks;
using FluentValidation.Results;

namespace Broker.Application.Service.Abstractions
{
    public interface IRemoteValidateService
    {
        Task<ValidationFailure[]> ValidateByKeyAsync<T>(string propertyName, T value, string keyPrefix, bool useCache = true);
    }
}
