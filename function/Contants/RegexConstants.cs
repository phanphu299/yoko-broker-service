namespace AHI.Broker.Function.Constant
{
    public class RegexConstants
    {
        //it is not stated in document in https://learn.microsoft.com/en-us/azure/iot-hub/iot-hub-devguide-identity-registry#module-identity-properties
        //but if you create an iot hub device has id end with '.', it will fail.
        public const string IOT_HUB_DEVICE_ID = @"^[a-zA-Z0-9\-\.%_*?!(),:=@\$']{0,127}[a-zA-Z0-9\-%_*?!(),:=@\$']{1}$";
    }
}