using System.Data;
using System.Threading.Tasks;
using AHI.Broker.Function.Model.ImportModel;

namespace AHI.Infrastructure.Repository.Abstraction
{
    public interface IBrokerPersistenceValidator
    {
        bool CanApply(string brokerType);
        Task<bool> ValidateAsync(BrokerModel broker, IDbConnection connection, IDbTransaction transaction);
    }
}