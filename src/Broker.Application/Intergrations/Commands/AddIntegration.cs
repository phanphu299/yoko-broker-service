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
    public class AddIntegration : UpsertTagCommand, IRequest<IntegrationDto>
    {
        public Guid? Id { get; set; }
        public string Type { get; set; }
        [DynamicValidation(RemoteValidationKeys.name)]
        public string Name { get; set; }
        public string Status { get; set; } = "IN";
        public IDictionary<string, object> Details { get; set; }
        public AddIntegration()
        {
            Details = new Dictionary<string, object>();
        }
        public readonly static Func<AddIntegration, Domain.Entity.Integration> Converter = Projection.Compile();
        public static Expression<Func<AddIntegration, Domain.Entity.Integration>> Projection
        {
            get
            {
                return entity => new Domain.Entity.Integration
                {
                    Id = entity.Id != null ? entity.Id.Value : Guid.NewGuid(),
                    Name = entity.Name,
                    Type = entity.Type,
                    Status = entity.Status,
                    Detail = new Domain.Entity.IntegrationDetail()
                    {
                        Content = JsonConvert.SerializeObject(entity.Details)
                    }
                };
            }
        }

        public static Domain.Entity.Integration Create(AddIntegration entity)
        {
            if (entity == null)
                return null;
            return Converter(entity);
        }
    }
}
