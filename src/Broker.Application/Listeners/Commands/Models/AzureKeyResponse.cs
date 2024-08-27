namespace Broker.Application.Handler.Command.Model
{
    public class AzureKeyResponse
    {
        public string PrimaryConnectionString { get; set; }
        public string PrimaryKey { get; set; }
    }
}