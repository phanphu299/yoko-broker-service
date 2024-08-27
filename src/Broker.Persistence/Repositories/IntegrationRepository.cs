using Broker.Persistence.DbContexts;
using Broker.Application.Repository.Abstraction;
using AHI.Infrastructure.Repository.Generic;
using System;
using Broker.Domain.Entity;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using AHI.Infrastructure.Exception;
using Broker.Application.Constant;
using Configuration.Application.Constant;
using Broker.Persistence.Extension;
using Microsoft.Extensions.Logging;
using AHI.Infrastructure.SharedKernel.Extension;

namespace Broker.Persistence.Repository
{
    public class IntegrationRepository : GenericRepository<Integration, Guid>, IIntegrationRepository
    {
        private BrokerDbContext _context;
        public IntegrationRepository(
            BrokerDbContext context) : base(context)
        {
            _context = context;
        }
        public override IQueryable<Integration> AsQueryable()
            => _context.Integrations.Include(x => x.Lookup)
                                    .Include(x => x.Detail)
                                    .Include(x => x.EntityTags)
                                    .Where(x => !x.EntityTags.Any() || x.EntityTags.Any(a => a.EntityType == EntityTypeConstants.INTEGRATION));
        public override IQueryable<Integration> AsFetchable()
        {
            return _context.Integrations.AsNoTracking().Select(x => new Integration { Id = x.Id, Name = x.Name, Type = x.Type });
        }
        public override Task<Integration> FindAsync(Guid id)
        {
            return _context.Integrations.Include(x => x.Detail).Include(x => x.Lookup).FirstAsync(x => x.Id == id);
        }

        public async Task<bool> RemoveListEntityWithRelationAsync(ICollection<Integration> entity)
        {
            foreach (Integration ig in entity)
            {
                var dbCheck = await _context.Integrations.Include(x => x.Detail).Where(x => x.Id == ig.Id).FirstOrDefaultAsync();
                if (dbCheck == null)
                    throw new EntityNotFoundException(detailCode: ExceptionErrorCode.DetailCode.ERROR_VALIDATION_SOME_ITEMS_DELETED);
                if (dbCheck.Detail != null)
                {
                    dbCheck.Detail.Deleted = true;
                    dbCheck.Detail.UpdatedUtc = DateTime.UtcNow;
                }

                dbCheck.Deleted = true;
                dbCheck.UpdatedUtc = DateTime.UtcNow;
                _context.Integrations.Update(dbCheck);
            }
            return true;
        }

        public override async Task<Integration> UpdateAsync(Guid id, Integration e)
        {
            var trackingEntity = await AsQueryable().FirstOrDefaultAsync(x => x.Id == id);
            if (trackingEntity != null)
            {
                Update(e, trackingEntity);
            }
            return trackingEntity;
        }

        protected override void Update(Integration requestObject, Integration targetObject)
        {
            // update the target detail resource
            requestObject.Detail.Id = targetObject.Detail.Id;
            targetObject.Name = requestObject.Name;
            targetObject.Detail = requestObject.Detail;
            targetObject.UpdatedUtc = DateTime.UtcNow;
        }

        public override Task<Integration> AddAsync(Integration e)
        {
            _context.Database.SetCommandTimeout(RetrieveConstants.TIME_OUT);
            return base.AddAsync(e);
        }
    }
}
