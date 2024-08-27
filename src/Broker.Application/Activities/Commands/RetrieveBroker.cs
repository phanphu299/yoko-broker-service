using System.Collections.Generic;
using MediatR;

namespace Broker.Application.Handler.Command
{
    public class RetrieveBroker : IRequest<IDictionary<string, object>>
    {
        public string Data { get; set; }
        public string Upn { get; set; }
    }
}
