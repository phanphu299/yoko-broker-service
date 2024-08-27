using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
namespace Broker.Application.Handler.Command.Model
{
    public class IntegrationSchemaDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public IEnumerable<SchemaDetailDto> Details { get; set; }
        public DateTime CreatedUtc { get; set; }
        public static Expression<Func<Domain.Entity.Schema, IntegrationSchemaDto>> Projection
        {
            get
            {
                return entity => new IntegrationSchemaDto
                {
                    Id = entity.Id,
                    Name = entity.Name,
                    Type = entity.Type,
                    CreatedUtc = entity.CreatedUtc,
                    Details = entity.Details.OrderBy(x => x.Order).Select(SchemaDetailDto.Create)
                };
            }
        }

        public static IntegrationSchemaDto Create(Domain.Entity.Schema entity)
        {
            if (entity == null)
                return null;
            return Projection.Compile().Invoke(entity);
        }
    }
}
