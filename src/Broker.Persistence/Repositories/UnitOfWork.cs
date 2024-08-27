using Broker.Persistence.DbContexts;
using Broker.Application.Repository;
using Microsoft.EntityFrameworkCore.Storage;
using System.Threading.Tasks;
using Broker.Application.Repository.Abstraction;
using AHI.Infrastructure.Service.Tag.Service.Abstraction;

namespace Broker.Persistence.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private BrokerDbContext _dbContext;
        private IDbContextTransaction _transaction;
        public IBrokerRepository Brokers { get; private set; }
        public IIntegrationRepository Integrations { get; private set; }
        public ILookupRepository Lookups { get; private set; }
        public ISchemaRepository Schemas { get; private set; }
        public IEntityTagRepository<Domain.Entity.EntityTagDb> EntityTags { get; private set; }
        public UnitOfWork(BrokerDbContext context, IBrokerRepository brokers, IIntegrationRepository integrations, ILookupRepository lookups, ISchemaRepository schemas, IEntityTagRepository<Domain.Entity.EntityTagDb> entityTags)
        {
            _dbContext = context;
            Brokers = brokers;
            Integrations = integrations;
            Lookups = lookups;
            Schemas = schemas;
            EntityTags = entityTags;
        }
        public async Task BeginTransactionAsync()
        {
            _transaction = await _dbContext.Database.BeginTransactionAsync();
        }
        public async Task RollbackAsync()
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
        }

        public async Task CommitAsync()
        {
            _dbContext.SaveChanges();
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
            }
        }
    }
}
