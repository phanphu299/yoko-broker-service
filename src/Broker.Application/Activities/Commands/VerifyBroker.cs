using AHI.Infrastructure.SharedKernel.Model;
using MediatR;

namespace Broker.Application.Handler.Command
{
    public class VerifyBroker : IRequest<BaseResponse>
    {
        public string Data { get; set; }
    }
}
