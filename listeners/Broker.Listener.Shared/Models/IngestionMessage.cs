using System.Collections.Immutable;

namespace Broker.Listener.Shared.Models;
public class IngestionMessage
{
    public string TenantId { get; }
    public string SubscriptionId { get; }
    public string ProjectId { get; }
    public string DeviceId { get; set; }
    public string BrokerType { get; set; }
    public string TopicName { get; set; }
    public IDictionary<string, object> RawData { get; }
    public IngestionMessage(string tenantId, string subscriptionId, string projectId, IDictionary<string, object> payload)
    {
        TenantId = tenantId;
        SubscriptionId = subscriptionId;
        ProjectId = projectId;
        // IMPORTANT: To prevent its value to be changed outside of the class once assigned
        RawData = payload.ToImmutableDictionary();
    }
}
