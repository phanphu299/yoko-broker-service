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
    public class AddBroker : UpsertTagCommand, IRequest<BrokerDto>
    {
        public Guid? Id { get; set; }
        [DynamicValidation(RemoteValidationKeys.name)]
        public string Name { get; set; }
        public string Type { get; set; }
        public string Status { get; set; } = "IN";
        public IDictionary<string, object> Details { get; set; }
        public AddBroker()
        {
            Details = new Dictionary<string, object>();
        }
        public readonly static Func<AddBroker, Domain.Entity.Broker> Converter = Projection.Compile();
        public static Expression<Func<AddBroker, Domain.Entity.Broker>> Projection
        {
            get
            {
                return entity => new Domain.Entity.Broker
                {
                    Id = entity.Id != null ? entity.Id.Value : Guid.NewGuid(),
                    Name = entity.Name,
                    Type = entity.Type,
                    Detail = entity.Type == "BROKER_REST_API" ? null : new Domain.Entity.BrokerDetail()
                    {
                        Content = JsonConvert.SerializeObject(entity.Details)
                    }
                };
            }
        }

        public static Domain.Entity.Broker Create(AddBroker entity)
        {
            if (entity == null)
                return null;
            return Converter(entity);
        }

    }
}
