using Broker.Application.Service.Abstraction;
using System.Collections.Generic;

namespace Broker.Application.Service
{
    public class RestApiBrokerVerificationHandler : IContentVerificationHandler
    {
        public bool Handle(IDictionary<string, object> content)
        {
            return true;
        }
    }
}
