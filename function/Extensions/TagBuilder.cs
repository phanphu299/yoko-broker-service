using System.Collections.Generic;
using AHI.Infrastructure.MultiTenancy.Abstraction;

namespace AHI.Broker.Function.Extension
{
    public static class TagBuilder
    {
        public static IDictionary<string, object> DEFAULT_TAGS = new Dictionary<string, object>()
        {
            {"Application","AHI"},
            {"Company","YEI"},
            {"Department","BD"},
            {"Division","DXP"},
            {"Environment","DEV"},
            {"Primary Owner", "Jati.Santoso@yokogawa.com"},
            {"Secondary Owner", "Thanhtrung.bui@yokogawa.com"},
            {"ApplicationOwner", "Santoso, Jati"},
            {"CostCenter", "2360"},
            {"UsageScope", "External"},
            {"IntraConnected", "false"}
        };
        public static IDictionary<string, object> BuidTags(ITenantContext tenantContext, string tenantName, string subscriptionName, string projectName, string environment, string rid)
        {
            var tags = new Dictionary<string, object>(DEFAULT_TAGS);
            tags["TenantId"] = tenantContext.TenantId;
            tags["TenantName"] = tenantName;
            if (!string.IsNullOrEmpty(tenantContext.SubscriptionId))
            {
                tags["SubscriptionId"] = tenantContext.SubscriptionId;
                tags["SubscriptionName"] = subscriptionName;
            }
            if (!string.IsNullOrEmpty(tenantContext.ProjectId))
            {
                tags["ProjectId"] = tenantContext.ProjectId;
                tags["ProjectName"] = projectName;
                tags["Project"] = projectName;
            }
            if (!string.IsNullOrEmpty(environment))
            {
                tags["Environment"] = environment;
            }
            tags["RID"] = rid;
            return tags;
        }
    }
}
