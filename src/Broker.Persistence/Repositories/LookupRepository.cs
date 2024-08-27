using System.Linq;
using Broker.Persistence.DbContexts;
using Broker.Application.Repository.Abstraction;
using System.Threading.Tasks;
using Broker.Domain.Entity;
using AHI.Infrastructure.Repository.Generic;
using System.Threading;
using Newtonsoft.Json;
using Broker.Application.Constants;
using System.Net.Http;
using Microsoft.EntityFrameworkCore;
using Broker.Application.Handler.Command.Model;
using AHI.Infrastructure.MultiTenancy.Abstraction;
using AHI.Infrastructure.MultiTenancy.Extension;

namespace Broker.Persistence.Repository
{
    public class LookupRepository : GenericRepository<Lookup, string>, ILookupRepository
    {
        private readonly BrokerDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ITenantContext _tenantContext;
        public LookupRepository(BrokerDbContext context, IHttpClientFactory httpClientFactory, ITenantContext tenantContext) : base(context)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _tenantContext = tenantContext;
        }
        public override Task<Lookup> FindAsync(string id)
        {
            return _context.Lookups.Where(x => x.Id == id).FirstOrDefaultAsync();
        }

        public async Task<Lookup> FindLookupByCodeAsync(string code, CancellationToken cancellationToken = default)
        {
            using var httpClient = _httpClientFactory.CreateClient(HttpClientNames.CONFIGURATION, _tenantContext);
            var endpoint = $"cnm/lookups/{code}";

            var httpResponseMessage = await httpClient.GetAsync(endpoint, cancellationToken);
            httpResponseMessage.EnsureSuccessStatusCode();

            var szContent = await httpResponseMessage.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(szContent))
                return null;

            var lookup = JsonConvert.DeserializeObject<LookupResponse>(szContent);
            return new Lookup { Id = lookup.Id, Name = lookup.Name, Active = lookup.Active, LookupTypeCode = lookup.LookupType.Id };
        }

        public async Task<Lookup> SaveAsync(Lookup entity)
        {
            var tracking = await _context.Lookups.Where(x => x.Id == entity.Id)
                                                        .Where(x => x.LookupTypeCode == entity.LookupTypeCode)
                                                        .FirstOrDefaultAsync();

            if (tracking == null)
            {
                await _context.Lookups.AddAsync(entity);
                await _context.SaveChangesAsync();
                return entity;
            }

            await UpdateAsync(entity.Id, entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        protected override void Update(Lookup requestObject, Lookup targetObject)
        {
            targetObject.Name = requestObject.Name;
            targetObject.Active = requestObject.Active;
        }
    }
}
