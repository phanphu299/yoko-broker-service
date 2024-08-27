using System.Threading.Tasks;
using AHI.Infrastructure.Service.Tag.Service.Abstraction;
using Broker.Application.Repository.Abstraction;

namespace Broker.Application.Repository
{
    public interface IUnitOfWork
    {
        IBrokerRepository Brokers { get; }
        IIntegrationRepository Integrations { get; }
        ILookupRepository Lookups { get; }
        ISchemaRepository Schemas { get; }
        IEntityTagRepository<Domain.Entity.EntityTagDb> EntityTags { get; }
        Task CommitAsync();
        Task RollbackAsync();
        Task BeginTransactionAsync();
    }
}
