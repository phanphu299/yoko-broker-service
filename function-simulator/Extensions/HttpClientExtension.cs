using System.Net.Http;
using AHI.Broker.Function.Models;

namespace AHI.Broker.Function.Extension
{
    public static class HttpClientExtension
    {
        public static void AddTenantContextHeader(this HttpClient httpClient, ProjectInfo projectInfo)
        {
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("x-tenant-id", projectInfo.TenantId.ToString());
            httpClient.DefaultRequestHeaders.Add("x-subscription-id", projectInfo.SubscriptionId.ToString());
            httpClient.DefaultRequestHeaders.Add("x-project-id", projectInfo.Id.ToString());
        }
    }
}