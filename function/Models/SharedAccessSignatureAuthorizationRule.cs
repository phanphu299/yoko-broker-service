namespace Broker.Application.Handler.Command.Model
{
    public class SharedAccessSignatureAuthorizationRule
    {
        public string KeyName { get; set; }
        public string PrimaryKey { get; set; }
        public string SecondaryKey { get; set; }
    }
}