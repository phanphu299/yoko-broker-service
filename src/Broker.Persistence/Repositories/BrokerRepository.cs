using Broker.Persistence.DbContexts;
using Broker.Application.Repository.Abstraction;
using AHI.Infrastructure.Repository.Generic;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using Microsoft.EntityFrameworkCore;
using AHI.Infrastructure.MultiTenancy.Abstraction;
using Microsoft.Extensions.Configuration;
using AHI.Infrastructure.Cache.Abstraction;
using HashidsNet;
using Newtonsoft.Json;
using Broker.Persistence.Extension;
using Broker.Application.Constant;

namespace Broker.Persistence.Repository
{
    public class BrokerRepository : GenericRepository<Domain.Entity.Broker, Guid>, IBrokerRepository
    {
        private const string REST_API_TYPE = "BROKER_REST_API";
        private BrokerDbContext _context;
        private readonly ITenantContext _tenantContext;
        private readonly IConfiguration _configuration;
        private readonly ICache _cache;
        public BrokerRepository(BrokerDbContext context, ITenantContext tenantContext, IConfiguration configuration, ICache cache)
            : base(context)
        {
            _context = context;
            _tenantContext = tenantContext;
            _configuration = configuration;
            _cache = cache;
        }
        public override IQueryable<Domain.Entity.Broker> AsQueryable()
        {
            return _context.Brokers.Include(x => x.Lookup)
                                .Include(x => x.Detail)
                                .Include(x => x.EntityTags)
                                .Where(x => !x.EntityTags.Any() || x.EntityTags.Any(a => a.EntityType == FileEntityConstants.BROKER));
        }

        public override IQueryable<Domain.Entity.Broker> AsFetchable()
        {
            return _context.Brokers.AsNoTracking().Select(x => new Domain.Entity.Broker { Id = x.Id, Name = x.Name, Type = x.Type });
        }

        public override Task<Domain.Entity.Broker> FindAsync(Guid id)
        {
            return AsQueryable().FirstOrDefaultAsync(x => x.Id == id);
        }

        public async override Task<bool> RemoveAsync(Guid id)
        {
            var entity = _context.Brokers.Where(x => x.Id == id).FirstOrDefault();
            if (entity == null)
            {
                return false;
            }

            entity.Deleted = true;
            entity.UpdatedUtc = DateTime.UtcNow;
            await DeleteCacheAsync(entity.Id);
            return true;
        }

        public async Task<bool> RemoveBrokersAsync(IEnumerable<Guid> ids)
        {
            var entities = await _context.Brokers.Where(x => ids.Contains(x.Id)).ToListAsync();

            if (!entities.Any())
            {
                return false;
            }

            foreach (var e in entities)
            {
                e.Deleted = true;
                e.UpdatedUtc = DateTime.UtcNow;
            }
            await Task.WhenAll(entities.Select(entity => DeleteCacheAsync(entity.Id)));

            return true;
        }

        protected override void Update(Domain.Entity.Broker requestObject, Domain.Entity.Broker targetObject)
        {
            // update the target detail resource
            requestObject.Detail.Id = targetObject.Detail.Id;
            targetObject.Status = requestObject.Status;
            targetObject.Name = requestObject.Name;
            targetObject.Detail = requestObject.Detail;
            targetObject.UpdatedUtc = DateTime.UtcNow;
            targetObject.IsShared = requestObject.IsShared;
        }

        public async Task<Domain.Entity.Broker> AddAsync(Domain.Entity.Broker e, IDictionary<string, object> details)
        {
            _context.Database.SetCommandTimeout(RetrieveConstants.TIME_OUT);
            await base.AddAsync(e);
            await BuildDetailAsync(e, details);
            return e;
        }

        public override async Task<Domain.Entity.Broker> UpdateAsync(Guid id, Domain.Entity.Broker e)
        {
            var trackingEntity = await AsQueryable().FirstOrDefaultAsync(x => x.Id == id);
            if (trackingEntity != null)
            {
                Update(e, trackingEntity);
            }
            return trackingEntity;
        }

        protected virtual Task BuildDetailAsync(Domain.Entity.Broker e, IDictionary<string, object> details)
        {
            if (e.Type == REST_API_TYPE)
            {
                e.Status = "AC";

                var id = GenerateApiId(e.Id);
                details["endpoint"] = $"{_configuration["Api:PublicEndpoint"].Trim('/')}/fnc/bkr/api?id={id}";
                // details["api_key"] = await GenerateApiKeyAsync(brokerId); // May implement later
                e.Detail = new Domain.Entity.BrokerDetail()
                {
                    Content = JsonConvert.SerializeObject(details)
                };
            }
            return Task.CompletedTask;
        }

        private string GenerateApiId(Guid brokerId)
        {
            var hasher = new Hashids();
            var key = hasher.EncodeGuid(_tenantContext.TenantId, _tenantContext.SubscriptionId, _tenantContext.ProjectId, brokerId.ToString());

            return key;
        }

        private Task DeleteCacheAsync(Guid brokerId)
        {
            return _cache.DeleteAllKeysAsync($"*_{brokerId}_*".ToLowerInvariant());
        }
    }
}
