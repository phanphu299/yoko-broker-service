namespace AHI.Broker.Function.Constant
{
    public class MessageConstants
    {
        public const string DEVICE_ID_UNMATCHED_BROKER_TYPE = "ERROR.ENTITY.VALIDATION.ID_UNMATCHED_BROKER_TYPE";

        public class FluentValidation
        {
            public const string REQUIRED = "AUDIT.LOG.IMPORT_ERROR.REQUIRED";
            public const string MAX_LENGTH = "AUDIT.LOG.IMPORT_ERROR.MAX_LENGTH";
            public const string GENERAL_INVALID = "AUDIT.LOG.IMPORT_ERROR.GENERAL_INVALID";
            public const string INVALID_VALUE_TYPE = "AUDIT.LOG.IMPORT_ERROR.INVALID_VALUE_TYPE";
            public const string INVALID_OPTION = "AUDIT.LOG.IMPORT_ERROR.INVALID_OPTION";
            public const string OUT_OF_RANGE = "AUDIT.LOG.IMPORT_ERROR.OUT_OF_RANGE";
            public const string NOT_EXIST_OR_ACTIVE = "AUDIT.LOG.IMPORT_ERROR.NOT_EXIST_OR_ACTIVE";

            public const string GET_FILE_FAILED = "AUDIT.LOG.IMPORT_ERROR.GET_FILE_FAILED";
            public const string EXPORT_NOT_SUPPORTED = "AUDIT.LOG.EXPORT_ERROR.NOT_SUPPORTED";
            public const string EXPORT_NOT_FOUND = "AUDIT.LOG.EXPORT_ERROR.NOT_FOUND";
        }
    }
}