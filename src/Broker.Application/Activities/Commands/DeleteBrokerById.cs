using System;
using AHI.Infrastructure.SharedKernel.Model;
using MediatR;

namespace Broker.Application.Handler.Command
{
    public class DeleteBrokerById : IRequest<BaseResponse>
    {
        public Guid Id { get; set; }

        public DeleteBrokerById(Guid id)
        {
            Id = id;
        }
    }
}
