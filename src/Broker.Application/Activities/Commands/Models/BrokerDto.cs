using System;
using System.Linq.Expressions;
using AHI.Infrastructure.Service.Tag.Extension;
using AHI.Infrastructure.Service.Tag.Model;

namespace Broker.Application.Handler.Command.Model
{
    public class BrokerDto : TagDtos
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Content { get; set; }
        public string Type { get; set; }
        public DateTime CreatedUtc { get; set; }
        public DateTime UpdatedUtc { get; set; }
        public string Status { get; set; }
        public LookupDto Lookup { get; set; }
        public string ResourcePath { get; set; }
        public string CreatedBy { get; set; }
        public static Expression<Func<Domain.Entity.Broker, BrokerDto>> Projection
        {
            get
            {
                return entity => new BrokerDto
                {
                    Id = entity.Id,
                    Name = entity.Name,
                    CreatedUtc = entity.CreatedUtc,
                    UpdatedUtc = entity.UpdatedUtc,
                    Content = entity.Detail != null ? entity.Detail.Content : null,
                    Type = entity.Type,
                    Status = entity.Status,
                    Lookup = LookupDto.Create(entity.Lookup),
                    ResourcePath = entity.ResourcePath,
                    CreatedBy = entity.CreatedBy,
                    Tags = entity.EntityTags.MappingTagDto()
                };
            }
        }

        public static BrokerDto Create(Domain.Entity.Broker entity)
        {
            if (entity == null)
                return null;
            return Projection.Compile().Invoke(entity);
        }
    }
}
