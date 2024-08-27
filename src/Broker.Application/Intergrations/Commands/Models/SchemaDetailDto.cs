using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
namespace Broker.Application.Handler.Command.Model
{
    public class SchemaDetailDto
    {
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
        public string DependOn { get; set; }
        public bool IsAllowCopy { get; set; }
        public bool IsAllowEdit { get; set; }
        public string Endpoint { get; set; }
        public IEnumerable<SchemaOptionDto> Options { get; set; }
        public static Expression<Func<Domain.Entity.SchemaDetail, SchemaDetailDto>> Projection
        {
            get
            {
                return entity => new SchemaDetailDto
                {
                    Name = entity.Name,
                    Key = entity.Key,
                    IsRequired = entity.IsRequired,
                    PlaceHolder = entity.PlaceHolder,
                    DataType = entity.DataType,
                    IsReadonly = entity.IsReadonly,
                    Regex = entity.Regex,
                    MinValue = entity.MinValue,
                    MaxValue = entity.MaxValue,
                    DefaultValue = entity.DefaultValue,
                    Options = entity.Options.OrderBy(x => x.Order).Select(SchemaOptionDto.Create),
                    DependOn = entity.DependOn,
                    IsAllowCopy = entity.IsAllowCopy,
                    IsAllowEdit = entity.IsAllowEdit,
                    Endpoint = entity.Endpoint
                };
            }
        }

        public static SchemaDetailDto Create(Domain.Entity.SchemaDetail entity)
        {
            if (entity == null)
                return null;
            return Projection.Compile().Invoke(entity);
        }
    }
}
