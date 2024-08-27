namespace AHI.Broker.Function.Service.Model
{
    public class IoTHubDto
    {
        public IoTHubProperty Properties { get; set; }
    }
    public class IoTHubProperty
    {
        public EventHubEndpoint EventHubEndpoints { get; set; }
    }
    public class EventHubEndpoint
    {
        public HubEvent Events { get; set; }
    }
    public class HubEvent
    {
        public string Endpoint { get; set; }
        public string Path { get; set; }
    }
}