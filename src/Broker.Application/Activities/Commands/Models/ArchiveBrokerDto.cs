using System;
using System.Linq.Expressions;

namespace Broker.Application.Handler.Command.Model
{
    public class ArchiveBrokerDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Content { get; set; }
        public string Type { get; set; }
        public DateTime CreatedUtc { get; set; }
        public DateTime UpdatedUtc { get; set; }
        public readonly static Func<BrokerDto, ArchiveBrokerDto> Converter = Projection.Compile();
        public static Expression<Func<BrokerDto, ArchiveBrokerDto>> Projection
        {
            get
            {
                return entity => new ArchiveBrokerDto
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

        public static ArchiveBrokerDto Create(BrokerDto brokerDto)
        {
            if (brokerDto == null)
                return null;
            return Converter(brokerDto);
        }
    }
}
