using System;
using System.Linq.Expressions;
namespace Broker.Application.Handler.Command.Model
{
    public class SchemaOptionDto
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public static Expression<Func<Domain.Entity.SchemaDetailDataOption, SchemaOptionDto>> Projection
        {
            get
            {
                return entity => new SchemaOptionDto
                {
                    Id = entity.Id,
                    Name = entity.Name,
                };
            }
        }

        public static SchemaOptionDto Create(Domain.Entity.SchemaDetailDataOption entity)
        {
            if (entity == null)
                return null;
            return Projection.Compile().Invoke(entity);
        }
    }
}
