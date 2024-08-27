using System.Net.Http;
using Microsoft.Extensions.Configuration;

namespace Function.Extension
{
    public static class HttpClientExtension
    {

        public static HttpClient SetHeaders(this HttpClient client, IConfiguration configuration)
        {
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("x-tenant-id", configuration["TenantId"]);
            client.DefaultRequestHeaders.Add("x-subscription-id", configuration["SubscriptionId"]);
            client.DefaultRequestHeaders.Add("x-project-id", configuration["ProjectId"]);
            return client;
        }
    }
}