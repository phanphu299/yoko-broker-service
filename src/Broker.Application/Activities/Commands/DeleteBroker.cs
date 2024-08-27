using System;
using System.Collections.Generic;
using AHI.Infrastructure.SharedKernel.Model;
using MediatR;

namespace Broker.Application.Handler.Command
{
    public class DeleteBroker : IRequest<BaseResponse>
    {
        public IEnumerable<Guid> Ids { get; set; }

        public DeleteBroker(IEnumerable<Guid> ids)
        {
            Ids = ids;
        }
    }
}