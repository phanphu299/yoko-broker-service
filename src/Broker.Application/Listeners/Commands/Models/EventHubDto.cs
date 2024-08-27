using System;

namespace Broker.Application.Handler.Command.Model
{
    public class EventHubDto
    {
        public Guid Id { get; set; }
        public string ConnectionString { get; set; }
        public string HubName { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
    }
}
