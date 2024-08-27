using AHI.Infrastructure.SharedKernel.Model;
using MediatR;

namespace Broker.Application.Handler.Command
{
    public class VerifyIntegration : IRequest<BaseResponse>
    {
        public string Data { get; set; }
    }
}
