using System;

namespace Broker.Application.Handler.Command.Model
{
    public class GreenKonceptDto
    {
        public Guid Id { get; set; }
        public string TokenType { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string Endpoint { get; set; }
        public Guid IntegrationId { get; set; }
        public string Status { get; set; }
    }
}
