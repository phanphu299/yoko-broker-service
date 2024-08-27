namespace Broker.Application.Constant
{
    public class IntegrationContentKeys
    {
        public static readonly string EVENT_HUB_NAME = "event_hub_name";
        public static readonly string CONNECTION_STRING = "connection_string";
        public static readonly string ENDPOINT = "endpoint";
        public static readonly string API_KEY = "api_key";
        public static readonly string API_SECRET = "api_secret";
        public static readonly string CLIENT_ID = "client_id";
        public static readonly string CLIENT_SECRET = "client_secret";
        public static readonly string POOLING_INTERVAL = "pooling_interval";
        public static readonly string BROKER_ENPOINT = "broker_endpoint";

        public static readonly string[] EVENT_HUB_KEYS =
        {
            EVENT_HUB_NAME,
            CONNECTION_STRING
        };

        public static readonly string[] WAYLAY_KEYS =
        {
            ENDPOINT,
            API_KEY,
            API_SECRET,
            POOLING_INTERVAL,
            BROKER_ENPOINT
        };

        public static readonly string[] GREEN_KONCEPT_KEYS =
        {
            ENDPOINT,
            CLIENT_ID,
            CLIENT_SECRET
        };
    }
}
