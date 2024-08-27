namespace Broker.Application.Constant
{
    public class BrokerContentKeys
    {
        public const string ENABLE_SHARING = "enable_sharing";
        public const string HOST = "host";
        public const string CONNECTION_STRING = "connection_string";
        public const string TELEMETRY_TOPIC = "telemetry_topic";
        public const string COMMAND_TOPIC = "command_topic";
        public const string URI_TELEMETRY = "uri_telemetry";
        public const string URI_COMMAND = "uri_command";
        public const string SAS_TOKEN = "sasToken";
        public const string SAS_TOKEN_DURATION = "sasTokenDuration";
        public const string EVENT_HUB_NAME = "event_hub_name";
        public const string CONNECTION_MODE = "connection_mode";
        public const string AUTHN_TYPE = "authentication_type";
        public const string PORT = "port";
        public const string TIER = "tier";
        public const string TIER_NAME = "tierName";
        public const string THROUGHPUT_UNITS = "throughput_units";
        public const string MAX_THROUGHPUT_UNITS = "max_throughput_units";
        public const string AUTO_INFLATE = "auto_inflate";
        public const string NUMBER_OF_HUB_UNITS = "number_of_hub_units";
        public const string DEFENDER_FOR_IOT = "defender_for_iot";
        public const string DEVICE_TO_CLOUD_PARTITIONS = "device_to_cloud_partitions";
        public const string EVENT_HUB_ID = "event_hub_id";
        public const string REST_API_NAME = "name";
        public const string PASSWORD_LENGTH = "password_length";

        public static readonly string[] KEYS =
        {
            CONNECTION_MODE,
            AUTHN_TYPE,
            PORT,
            TIER,
            EVENT_HUB_NAME,
            THROUGHPUT_UNITS,
            AUTO_INFLATE,
            MAX_THROUGHPUT_UNITS,
            SAS_TOKEN_DURATION,
            NUMBER_OF_HUB_UNITS,
            DEFENDER_FOR_IOT,
            DEVICE_TO_CLOUD_PARTITIONS,
            ENABLE_SHARING
        };

        public static readonly string[] COAP_KEYS =
        {
            CONNECTION_MODE,
            PASSWORD_LENGTH
        };

        public static readonly string[] VALIDATION_COAP_KEYS =
        {
            CONNECTION_MODE
        };

        public static readonly string[] MQTT_KEYS =
        {
            AUTHN_TYPE,
            PORT,
            PASSWORD_LENGTH
        };

        public static readonly string[] VALIDATION_MQTT_KEYS =
        {
            AUTHN_TYPE,
            PORT
        };

        public static readonly string[] EVENT_HUB_KEYS =
        {
            TIER,
            TIER_NAME,
            EVENT_HUB_NAME,
            THROUGHPUT_UNITS,
            AUTO_INFLATE,
            MAX_THROUGHPUT_UNITS,
            SAS_TOKEN_DURATION
        };

        public static readonly string[] IOT_HUB_KEYS =
        {
            TIER,
            TIER_NAME,
            NUMBER_OF_HUB_UNITS,
            DEFENDER_FOR_IOT,
            DEVICE_TO_CLOUD_PARTITIONS,
            ENABLE_SHARING
        };

        public static readonly string[] REST_API_KEYS =
        {
            REST_API_NAME
        };

        public static readonly string[] BROKER_SCHEMA_EXCEPT_DETAIL_KEYS =
        {
            TELEMETRY_TOPIC,
            COMMAND_TOPIC,
            URI_COMMAND,
            URI_TELEMETRY
        };
    }
}
