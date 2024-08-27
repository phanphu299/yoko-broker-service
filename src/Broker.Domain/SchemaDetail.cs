using System;
using System.Collections.Generic;
using AHI.Infrastructure.Repository.Model.Generic;

namespace Broker.Domain.Entity
{
    public class SchemaDetail : IEntity<Guid>
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Key { get; set; }
        public bool IsRequired { get; set; }
        public string PlaceHolder { get; set; }
        public string DataType { get; set; }
        public bool IsReadonly { get; set; }
        public string Regex { get; set; }
        public int? MinValue { get; set; }
        public int? MaxValue { get; set; }
        public string DefaultValue { get; set; }
        public DateTime CreatedUtc { get; set; }
        public DateTime UpdatedUtc { get; set; }
        public bool Deleted { set; get; }
        public Guid SchemaId { get; set; }
        public Schema Schema { get; set; }
        public int Order { get; set; }
        public string DependOn { get; set; }
        public bool IsAllowCopy { get; set; }
        public bool IsAllowEdit { get; set; }
        public string Endpoint { get; set; }
        public ICollection<SchemaDetailDataOption> Options { get; set; }
        public SchemaDetail()
        {
            CreatedUtc = DateTime.UtcNow;
            UpdatedUtc = DateTime.UtcNow;
            Options = new List<SchemaDetailDataOption>();
        }
    }
}
