namespace Function.Service.Message
{
    public class BrokerListenerChangedMessage : BrokerChangedMessage
    {
        public override string TopicName => "broker.function.event.broker.changed";
    }
}
