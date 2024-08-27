using System;
using System.Linq.Expressions;

namespace Broker.Application.Handler.Command.Model
{
    public class ArchiveIntegrationDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Content { get; set; }
        public string Type { get; set; }
        public DateTime CreatedUtc { get; set; }
        public DateTime UpdatedUtc { get; set; }
        public readonly static Func<IntegrationDto, ArchiveIntegrationDto> Converter = Projection.Compile();
        public static Expression<Func<IntegrationDto, ArchiveIntegrationDto>> Projection
        {
            get
            {
                return entity => new ArchiveIntegrationDto
                {
                    Id = entity.Id,
                    Name = entity.Name,
                    Content = entity.Content != null ? entity.Content : null,
                    CreatedUtc = entity.CreatedUtc,
                    UpdatedUtc = entity.UpdatedUtc,
                    Type = entity.Type
                };
            }
        }

        public static ArchiveIntegrationDto Create(IntegrationDto integrationDto)
        {
            if (integrationDto == null)
                return null;
            return Converter(integrationDto);
        }
    }
}
