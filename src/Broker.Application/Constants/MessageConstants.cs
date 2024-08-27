namespace Broker.Application.Constant
{
    public static class MessageConstants
    {
        public const string ATTRIBUTE_TYPE_REQUIRED = "MSG_ATTRIBUTE_TYPE_REQUIRED";
        public const string ATTRIBUTE_NAME_REQUIRED = "MSG_ATTRIBUTE_NAME_REQUIRED";
        public const string ENTITY_NOT_FOUND = "MSG_ENTITY_NOT_FOUND";
        public const string DUPLICATED_BROKER_NAME = "MSG_DUPLICATED_BROKER_NAME";
        public const string BROKER_NOT_FOUND = "BROKER.NOT_FOUND";
        public const string INTEGRATION_NOT_FOUND = "INTEGRATION.NOT_FOUND";
        public const string COMMON_ERROR_NO_HANDLER = "ERROR.MESSAGE.NO_HANDLER";
        public const string COMMON_ERROR_MISSED_CONFIG = "ERROR.MESSAGE.MISSED_CONFIG";

        public static class ApiCallMessage
        {
            public const string WAYLAY_API_CALL_ERROR = "BROKER.INTEGRATION.FETCH.WAYLAY_API_ERROR";
            public const string GREEN_KONCEPT_API_CALL_ERROR = "BROKER.INTEGRATION.FETCH.GREEN_KONCEPT_API_ERROR";
        }
    }
}
