using System;

namespace Broker.Application.Handler.Command.Model
{
    public class WaylayDto
    {
        public Guid Id { get; set; }
        public string TokenType { get; set; }
        public string Token { get; set; }
        public string BrokerEndpoint { get; set; }
        public string DeviceId { get; set; }
        public Guid IntegrationId { get; set; }
        public int Interval { get; set; }
        public string Status { get; set; }
    }
}
