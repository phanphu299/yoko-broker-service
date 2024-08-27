using MediatR;
using System.Collections.Generic;

namespace Broker.Application.Handler.Command
{
    public class RetrieveIntegration : IRequest<IDictionary<string, object>>
    {
        public string Data { get; set; }
        public string Upn { get; set; }
    }
}
