using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Broker.Application.Constants;
using Broker.Application.Handler.Command.Model;
using MediatR;
using Newtonsoft.Json;
using AHI.Infrastructure.Validation.CustomAttribute;
using AHI.Infrastructure.Service.Tag.Model;

namespace Broker.Application.Handler.Command
{
    public class UpdateIntegration : UpsertTagCommand, IRequest<IntegrationDto>
    {
        public Guid Id { get; set; }
        public string Type { get; set; }
        [DynamicValidation(RemoteValidationKeys.name)]
        public string Name { get; set; }
        public IDictionary<string, object> Details { get; set; }
        public UpdateIntegration()
        {
            Details = new Dictionary<string, object>();
        }
        public static Expression<Func<UpdateIntegration, Domain.Entity.Integration>> Projection
        {
            get
            {
                return entity => new Domain.Entity.Integration
                {
                    Id = entity.Id,
                    Name = entity.Name,
                    Type = entity.Type,
                    Detail = new Domain.Entity.IntegrationDetail()
                    {
                        Content = JsonConvert.SerializeObject(entity.Details)
                    }
                };
            }
        }

        public static Domain.Entity.Integration Create(UpdateIntegration entity)
        {
            return Projection.Compile().Invoke(entity);
        }
    }
}
