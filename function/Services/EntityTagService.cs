using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AHI.Broker.Function.Constant;
using AHI.Broker.Function.Extension;
using AHI.Broker.Function.Service.Abstraction;
using AHI.Infrastructure.Cache.Abstraction;
using AHI.Infrastructure.MultiTenancy.Abstraction;
using AHI.Infrastructure.SharedKernel.Extension;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace AHI.Broker.Function.Service
{
    public class EntityTagService : IEntityTagService
    {
        private readonly IConfiguration _configuration;
        private readonly ITenantContext _tenantContext;
        private readonly ICache _cache;

        public EntityTagService(
            IConfiguration configuration,
            ITenantContext tenantContext,
            ICache cache)
        {
            _configuration = configuration;
            _tenantContext = tenantContext;
            _cache = cache;
        }
        
        public async Task<IEnumerable<Guid>> GetEntityIdsByTagIdsAsync(long[] tagIds)
        {
            var connectionString = _configuration["ConnectionStrings:Default"].BuildConnectionString(_configuration, _tenantContext.ProjectId);
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                var query = @"SELECT et.entity_id_uuid 
                            FROM [entity_tags] et WITH(NOLOCK) 
                            WHERE et.tag_id IN @TagIds";
                var result = await connection.QueryAsync<Guid>(query, new { TagIds = tagIds });
                await connection.CloseAsync();
                return result;
            }
        }

        public async Task RemoveBrokerDetailCacheAsync(IEnumerable<Guid> brokerIds)
        {
            var tasks = brokerIds.Select(id => _cache.DeleteAsync(CacheKey.BROKER_DETAIL_KEY.GetCacheKey(_tenantContext.TenantId, _tenantContext.SubscriptionId, _tenantContext.ProjectId, id)));
            await Task.WhenAll(tasks);
        }
    }
}