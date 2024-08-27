using Broker.Application.Handler.Command.Model;
using MediatR;
using System.Collections.Generic;

namespace Broker.Application.Handler.Command
{
    public class GetValidMqtt : IRequest<IEnumerable<MqttDto>>
    {
        public GetValidMqtt()
        {
        }
    }
}
