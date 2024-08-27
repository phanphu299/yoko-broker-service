namespace Broker.Application.Constant
{
     public class BrokerConfig
    {
        public const string EMQX_COAP_CONNECTION_STRING = "CoapConnectionString";
        public const string EMQX_MQTT_CONNECTION_STRING = "MqttConnectionString";
        public const int MINIMUM_PASSWORD_LENGTH = 10;
        public const int MAXIMUM_PASSWORD_LENGTH = 64;
        public const int DEFAULT_PASSWORD_LENGTH = 30;
    }
}
