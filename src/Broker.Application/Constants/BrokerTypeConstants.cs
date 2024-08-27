namespace Broker.Application.Constant
{
    public class BrokerTypeConstants
    {
        public const string EVENT_HUB = "BROKER_EVENT_HUB";
        public const string EMQX_MQTT = "BROKER_EMQX_MQTT";
        public const string EMQX_COAP = "BROKER_EMQX_COAP";
        public const string IOT_HUB = "BROKER_IOT_HUB";
        public const string REST_API = "BROKER_REST_API";
        public const string INTEGRATION_EVENT_HUB = "INTEGRATION_EVENT_HUB";
        public static readonly string[] EMQX_BROKERS = { EMQX_MQTT, EMQX_COAP };
    }

    public static class AzureEventHubTier
    {
        public const string Basic = "Basic";
        public const string Standard = "Standard";
        public const string Premium = "Premium";
    }
}
