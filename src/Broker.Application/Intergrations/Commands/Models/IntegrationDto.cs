using System;
using System.Linq.Expressions;
using AHI.Infrastructure.Service.Tag.Extension;
using AHI.Infrastructure.Service.Tag.Model;

namespace Broker.Application.Handler.Command.Model
{
    public class IntegrationDto : TagDtos
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Content { get; set; }
        public string Status { get; set; }
        public string Type { get; set; }
        public DateTime CreatedUtc { get; set; }
        public DateTime UpdatedUtc { get; set; }
        public LookupDto Lookup { get; set; }
        public static Expression<Func<Domain.Entity.Integration, IntegrationDto>> Projection
        {
            get
            {
                return entity => new IntegrationDto
                {
                    Id = entity.Id,
                    Name = entity.Name,
                    Type = entity.Type,
                    CreatedUtc = entity.CreatedUtc,
                    UpdatedUtc = entity.UpdatedUtc,
                    Content = entity.Detail != null ? entity.Detail.Content : null,
                    Status = entity.Status,
                    Lookup = LookupDto.Create(entity.Lookup),
                    Tags = entity.EntityTags.MappingTagDto()
                };
            }
        }

        public static IntegrationDto Create(Domain.Entity.Integration entity)
        {
            if (entity == null)
                return null;
            return Projection.Compile().Invoke(entity);
        }
    }
}
