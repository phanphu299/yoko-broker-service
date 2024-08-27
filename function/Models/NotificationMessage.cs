using AHI.Infrastructure.Audit.Model;

namespace AHI.Broker.Function.Model.Notification
{
    public class BrokerNotificationMessage : NotificationMessage
    {
        public string BrokerId { get; set; }

        public BrokerNotificationMessage(string type, string brokerId, object payload) : base(type, payload)
        {
            BrokerId = brokerId;
        }
    }
}
