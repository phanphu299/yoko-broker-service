using System.Threading;
using System.Threading.Tasks;
using Broker.Application.Handler.Command;
using Broker.Application.Handler.Command.Model;
using Broker.Application.Service.Abstraction;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Broker.Application.Constant;
using System.Net.Http;
using Newtonsoft.Json;
using AHI.Infrastructure.SharedKernel.Abstraction;
using Broker.Application.Repository;

namespace Broker.Application.Service
{
    public class EmqxService : IEmqxService
    {
        private readonly IUnitOfWork _unitOfWork;

        public EmqxService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        
        public async Task<IEnumerable<MqttDto>> GetAllMqttAsync(GetValidMqtt request, CancellationToken cancellationToken)
        {
            var brokers = await _unitOfWork.Brokers
                                    .AsQueryable()
                                    .Include(x => x.Detail)
                                    .Where(x => x.Detail != null && x.Type == BrokerTypeConstants.EMQX_MQTT)
                                    .Select(x => x)
                                    .ToListAsync();

            var result = new List<MqttDto>();
            foreach (var broker in brokers)
            {
                var content = JsonConvert.DeserializeObject<MqttInformation>(broker.Detail.Content);
                result.Add(new MqttDto()
                {
                    UserName = content.UserName,
                    AccessToken = content.AccessToken,
                    QoS = content.QoS,
                    Id = broker.Id,
                    Name = broker.Name
                });
            }
            return result;
        }

        public async Task<IEnumerable<CoapDto>> GetAllCoapAsync(GetValidCoap request, CancellationToken cancellationToken)
        {
            var brokers = await _unitOfWork.Brokers
                                    .AsQueryable()
                                    .Include(x => x.Detail)
                                    .Where(x => x.Detail != null && x.Type == BrokerTypeConstants.EMQX_COAP)
                                    .Select(x => x)
                                    .ToListAsync();

            var result = new List<CoapDto>();
            foreach (var broker in brokers)
            {
                var content = JsonConvert.DeserializeObject<CoapInformation>(broker.Detail.Content);
                result.Add(new CoapDto()
                {
                    UserName = content.UserName,
                    AccessToken = content.AccessToken,
                    QoS = content.QoS,
                    Id = broker.Id,
                    Name = broker.Name
                });
            }
            return result;
        }
    }
}
