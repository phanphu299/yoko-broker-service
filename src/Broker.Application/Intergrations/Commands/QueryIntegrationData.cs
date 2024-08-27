using Device.Domain.Entity;
using AHI.Infrastructure.SharedKernel.Model;
using MediatR;
using System;
using System.Collections.Generic;

namespace Broker.Application.Handler.Command
{
    public class QueryIntegrationData : BaseCriteria, IRequest<IEnumerable<TimeSeriesDto>>
    {
        public Guid Id { get; set; }
        public string EntityId { get; set; }
        public string MetricKey { get; set; }
        public long TimeStart { get; set; }
        public long TimeEnd { get; set; }
        public string Aggregate { get; set; }
        public string Grouping { get; set; } = "PT5S";
        public QueryIntegrationData(Guid id, string entityId, string metricKey, long? start, long? end, string aggregate, string grouping = null)
        {
            Id = id;
            EntityId = entityId;
            MetricKey = metricKey;
            TimeStart = start ?? DateTimeOffset.UtcNow.AddHours(-1).ToUnixTimeMilliseconds();
            TimeEnd = end ?? DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            Aggregate = aggregate ?? "median";
            Grouping = grouping ?? "PT5S";
        }
    }
}
