using System.Threading;
using System.Threading.Tasks;
using Broker.Application.Handler.Command;
using Broker.Application.Handler.Command.Model;
using Broker.Application.Service.Abstraction;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Text;
using Configuration.Application.Constant;
using Broker.Application.Repository;

namespace Broker.Application.Service
{
    public class WaylayService : IWaylayService
    {
        private readonly IUnitOfWork _unitOfWork;

        public WaylayService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }


        public async Task<IEnumerable<WaylayDto>> GetAllWaylayAsync(GetValidWaylay request, CancellationToken cancellationToken)
        {
            var integrations = await _unitOfWork.Integrations.AsQueryable().Include(x => x.Detail).Where(x => x.Detail != null && x.Type == IntegrationTypeConstants.WAY_LAY).Select(x => x).ToListAsync();
            var result = new List<WaylayDto>();
            foreach (var integration in integrations)
            {
                var content = JsonConvert.DeserializeObject<WaylayInformation>(integration.Detail.Content);
                var secret = GenerateWaylayToken(content);
                result.Add(new WaylayDto()
                {
                    Id = integration.Id,
                    BrokerEndpoint = content.BrokerEndpoint,
                    Token = secret,
                    TokenType = "Basic",
                    IntegrationId = integration.Id,
                    Interval = content.Interval,
                    Status = integration.Status
                });
            }
            return result;
        }


        private string GenerateWaylayToken(WaylayInformation content)
        {
            return System.Convert.ToBase64String(Encoding.UTF8.GetBytes($"{content.ApiKey}:{content.ApiSecret}"));
        }
    }
}
