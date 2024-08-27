using System.Linq;
using Broker.Persistence.DbContexts;
using Broker.Application.Repository.Abstraction;

namespace Broker.Persistence.Repository
{
    public class SchemaRepository : ISchemaRepository
    {
        private BrokerDbContext _context;
        public SchemaRepository(BrokerDbContext context)
        {
            _context = context;
        }

        public IQueryable<Domain.Entity.Schema> AsQueryable()
        {
            return _context.IntegrationSchemas;
        }
    }
}
