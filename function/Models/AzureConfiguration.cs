namespace AHI.Broker.Function.Model
{
    public class AzureConfiguration
    {
        public string Authority { get; set; } = "https://login.microsoftonline.com";
        public string Endpoint { get; set; } = "https://management.azure.com";
        public string TenantId { get; set; }
        public string SubscriptionId { get; set; }
        public string ResourceGroup { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string Environment { get; set; }
    }
}