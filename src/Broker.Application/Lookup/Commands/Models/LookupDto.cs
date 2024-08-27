using System;
using System.Linq.Expressions;

namespace Broker.Application.Handler.Command.Model
{
    public class LookupDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string LookupTypeCode { get; set; }
        public bool Active { get; set; }
        public static Expression<Func<Domain.Entity.Lookup, LookupDto>> Projection
        {
            get
            {
                return entity => new LookupDto
                {
                    Id = entity.Id,
                    Name = entity.Name,
                    Active = entity.Active,
                    LookupTypeCode = entity.LookupTypeCode
                };
            }
        }

        public static LookupDto Create(Domain.Entity.Lookup entity)
        {
            if (entity == null)
                return null;
            return Projection.Compile().Invoke(entity);
        }
    }
}
