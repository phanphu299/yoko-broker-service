using Broker.Application.Service.Abstraction;
using System.Collections.Generic;
using System.Linq;
using Broker.Application.Constant;

namespace Broker.Application.Service
{
    public class WaylayIntegrationVerificationHandler : IContentVerificationHandler
    {
        public bool Handle(IDictionary<string, object> content)
        {
            return content.Count == IntegrationContentKeys.WAYLAY_KEYS.Length && 
                   content.All(x => IntegrationContentKeys.WAYLAY_KEYS.Contains(x.Key));
        }
    }
}
