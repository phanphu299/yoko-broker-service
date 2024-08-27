using Broker.Application.Service.Abstraction;
using System.Collections.Generic;
using System.Linq;
using Broker.Application.Constant;

namespace Broker.Application.Service
{
    public class EventHubIntegrationVerificationHandler : IContentVerificationHandler
    {
        public bool Handle(IDictionary<string, object> content)
        {
            return content.Count == IntegrationContentKeys.EVENT_HUB_KEYS.Length && 
                   content.All(x => IntegrationContentKeys.EVENT_HUB_KEYS.Contains(x.Key));
        }
    }
}
