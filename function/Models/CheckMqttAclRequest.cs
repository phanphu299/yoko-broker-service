namespace AHI.Broker.Function.Model
{
    public class CheckMqttAclRequest
    {
        public string Username { get; set; }
        public string Topic { get; set; }
        public string Action { get; set; }
    }
}
