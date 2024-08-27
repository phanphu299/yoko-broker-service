using Broker.Application.Service.Abstraction;
using System.Collections.Generic;
using System.Linq;
using Broker.Application.Constant;

namespace Broker.Application.Service
{
    public class GreenKonceptIntegrationVerificationHandler : IContentVerificationHandler
    {
        public bool Handle(IDictionary<string, object> content)
        {
            return content.Count == IntegrationContentKeys.GREEN_KONCEPT_KEYS.Length && 
                   content.All(x => IntegrationContentKeys.GREEN_KONCEPT_KEYS.Contains(x.Key));
        }
    }
}
