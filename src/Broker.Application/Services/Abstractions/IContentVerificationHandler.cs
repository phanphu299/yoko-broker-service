using System.Collections.Generic;

namespace Broker.Application.Service.Abstraction
{
    public interface IContentVerificationHandler
    {
        bool Handle(IDictionary<string, object> content);
    }
}
