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
    public class UpdateBroker : UpsertTagCommand, IRequest<BrokerDto>
    {
        public Guid Id { get; set; }
        [DynamicValidation(RemoteValidationKeys.name)]
        public string Name { get; set; }
        public string Status { get; set; }
        public string Type { get; set; }
        public IDictionary<string, object> Details { get; set; }
        public UpdateBroker()
        {
            Details = new Dictionary<string, object>();
        }
        public static Expression<Func<UpdateBroker, Domain.Entity.Broker>> Projection
        {
            get
            {
                return entity => new Domain.Entity.Broker
                {
                    Id = entity.Id,
                    Name = entity.Name,
                    Type = entity.Type,
                    Status = entity.Status,
                    Detail = new Domain.Entity.BrokerDetail()
                    {
                        Content = JsonConvert.SerializeObject(entity.Details)
                    }
                };
            }
        }

        public static Domain.Entity.Broker Create(UpdateBroker entity)
        {
            return Projection.Compile().Invoke(entity);
        }
    }
}
