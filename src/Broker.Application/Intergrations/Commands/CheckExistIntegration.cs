using System;
using System.Collections.Generic;
using AHI.Infrastructure.SharedKernel.Model;
using MediatR;


namespace Broker.Application.Handler.Command
{
    public class CheckExistIntegration : IRequest<BaseResponse>
    {
        public IEnumerable<Guid> Ids { get; set; }

        public CheckExistIntegration(IEnumerable<Guid> ids)
        {
            Ids = ids;
        }
    }
}