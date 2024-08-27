using System;
using System.Collections.Generic;
namespace Broker.Application.Handler.Command.Model
{
    public class CoapDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public IEnumerable<BrokerTopicDto> Topics { get; set; }
        public string UserName { get; set; }
        public string AccessToken { get; set; }
        public int QoS { get; set; }
    }
}
