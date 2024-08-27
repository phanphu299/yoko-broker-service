using Broker.Application.Service.Abstraction;
using System.Collections.Generic;
using System.Linq;
using Broker.Application.Constant;

namespace Broker.Application.Service
{
    public class MqttBrokerVerificationHandler : IContentVerificationHandler
    {
        public bool Handle(IDictionary<string, object> content)
        {
            if (content.ContainsKey(BrokerContentKeys.PASSWORD_LENGTH))
            {
                return content.Count == BrokerContentKeys.VALIDATION_MQTT_KEYS.Length + 1 &&
                   BrokerContentKeys.VALIDATION_MQTT_KEYS.AsEnumerable().All(x => content.Keys.Contains(x));
            }
            else
            {
                return content.Count == BrokerContentKeys.VALIDATION_MQTT_KEYS.Length &&
                    BrokerContentKeys.VALIDATION_MQTT_KEYS.AsEnumerable().All(x => content.Keys.Contains(x));
            }
        }
    }
}
