namespace AHI.Broker.Function.Model
{
    public class NotificationMessage
    {
        public string Type { get; set; }
        public string Upn { get; set; }
        public object Payload { get; set; }

        public NotificationMessage(string type, string upn, object payload)
        {
            Type = type;
            Payload = payload;
            Upn = upn;
        }
    }
}