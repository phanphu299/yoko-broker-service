using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using AHI.Infrastructure.SharedKernel.Model;
using MediatR;

namespace Broker.Application.Handler.Command
{
    public class DeleteIntegration : IRequest<BaseResponse>
    {
        public Guid Id { get; set; }
        public IEnumerable<DeleteIntegration> Integrations { get; set; } = new List<DeleteIntegration>();
        public static Expression<Func<DeleteIntegration, Domain.Entity.Integration>> Projection
        {
            get
            {
                return entity => new Domain.Entity.Integration
                {
                    Id = entity.Id
                };
            }
        }

        public static Domain.Entity.Integration Create(DeleteIntegration entity)
        {
            return Projection.Compile().Invoke(entity);
        }

    }
}
