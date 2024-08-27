using System;

namespace AHI.Broker.Function.Model
{
    public class AssignClientRequest
    {
        public Guid BrokerId { get; set; }
        public string ClientId { get; set; }
        public string Topic { get; set; }
        public string AccessToken { get; set; }
        public string[] Topics { get; set; }
    }
}