namespace AHI.Broker.Function.Constant
{
    public static class BrokerType
    {
        public const string BROKER_EVENT_HUB = "BROKER_EVENT_HUB";
        public const string BROKER_IOT_HUB = "BROKER_IOT_HUB";
        public const string BROKER_REST_API = "BROKER_REST_API";
        public const string EMQX_MQTT = "BROKER_EMQX_MQTT";
        public const string EMQX_COAP = "BROKER_EMQX_COAP";
        public static readonly string[] EMQX_BROKERS = { EMQX_MQTT, EMQX_COAP };
        public static readonly string[] IOT_HUB_STANDARD_TIER = { "S1", "S2", "S3" };
    }
}