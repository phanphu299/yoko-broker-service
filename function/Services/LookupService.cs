using System.Threading.Tasks;
using AHI.Infrastructure.MultiTenancy.Abstraction;
using AHI.Broker.Function.Service.Abstraction;
using AHI.Function.Model;
using Microsoft.Extensions.Logging;
using Function.Enum;
using Microsoft.Extensions.Configuration;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Threading;
using Newtonsoft.Json;
using System.Net.Http;
using AHI.Broker.Function.Constant;
using AHI.Infrastructure.SharedKernel.Extension;
using AHI.Infrastructure.MultiTenancy.Extension;

namespace AHI.Broker.Function.Service
{
    public class LookupService : ILookupService
    {
        private readonly ITenantContext _tenantContext;
        private readonly ILogger _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        public LookupService(ITenantContext tenantContext, IConfiguration configuration, IHttpClientFactory httpClientFactory, ILogger logger)
        {
            _tenantContext = tenantContext;
            _logger = logger;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<LookupDto> ProcessLookUpFromConfigurationServiceAsync(string code, CancellationToken token = default)
        {
            // Check the validity of lookup code.
            var lookupCode = code;
            var lookup = await FindLookupByCodeAsync(lookupCode, token);
            if (lookup == null)
                return null;

            // Lookup is not active.
            if (!lookup.Active)
                return null;
            //save or update lookup
            return await SaveAsync(lookup);
        }

        public async Task<LookupDto> FindLookupByCodeAsync(string code, CancellationToken cancellationToken = default)
        {
            try
            {
                using var httpClient = _httpClientFactory.CreateClient(HttpClientNames.CONFIGURATION, _tenantContext);
                var endpoint = $"cnm/lookups/{code}";

                var httpResponseMessage = await httpClient.GetAsync(endpoint, cancellationToken);
                httpResponseMessage.EnsureSuccessStatusCode();

                var szContent = await httpResponseMessage.Content.ReadAsStringAsync();
                if (string.IsNullOrEmpty(szContent))
                    return null;

                return JsonConvert.DeserializeObject<LookupDto>(szContent);
            }
            catch (System.Exception ex)
            {
                _logger.LogError($"Got exception {ex.Message}, system retry with http Client");
                return null;
            }

        }

        public async Task<LookupDto> SaveAsync(LookupDto entity)
        {
            var connectionString = _configuration["ConnectionStrings:Default"].BuildConnectionString(_configuration, _tenantContext.ProjectId);
            var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync($@" if not exists (select 1 from lookups where code = '{entity.Id}' and  lookup_type_code = '{entity.LookupType.Id}') 
                                                begin
                                                        insert into lookups(code, name, lookup_type_code) values ('{entity.Id}', '{entity.Name}', '{entity.LookupType.Name}')
                                                end
                                            else
                                                begin
                                                    update lookups set name = '{entity.Name}', active = '{entity.Active}' where code = '{entity.Id}' and  lookup_type_code = '{entity.LookupType.Id}' 
                                                end; ");
            connection.Close();
            return entity;
        }

        public async Task UpsertAsync(LookupInfo info)
        {
            if (info.ActionType != ActionTypeEnum.Updated)
                return;

            var connectionString = _configuration["ConnectionStrings:Default"].BuildConnectionString(_configuration, _tenantContext.ProjectId);
            var dbConnection = new SqlConnection(connectionString);

            await dbConnection.ExecuteAsync($" update lookups set name ='{info.Name}', active = '{info.Active}' where code = '{info.Id}' and  lookup_type_code = '{info.LookupTypeCode}' ;");
            dbConnection.Close();
        }
    }
    public class LookupDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Color { get; set; }
        public bool Active { get; set; }
        public LookupTypeDto LookupType { get; set; }
    }

    public class LookupTypeDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
}
