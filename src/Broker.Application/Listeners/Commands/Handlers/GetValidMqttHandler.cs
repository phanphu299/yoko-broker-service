using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Broker.Application.Handler.Command.Model;
using Broker.Application.Service.Abstraction;
using MediatR;

namespace Broker.Application.Handler.Command.Handler
{
    public class GetValidMqttHandler : IRequestHandler<GetValidMqtt, IEnumerable<MqttDto>>
    {
        private readonly IEmqxService _service;
        public GetValidMqttHandler(IEmqxService service)
        {
            _service = service;
        }

        public Task<IEnumerable<MqttDto>> Handle(GetValidMqtt request, CancellationToken cancellationToken)
        {
            return _service.GetAllMqttAsync(request, cancellationToken);
        }
    }
}
