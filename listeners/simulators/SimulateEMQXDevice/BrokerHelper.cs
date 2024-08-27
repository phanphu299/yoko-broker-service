using System;

namespace SimulateEMQXDevice
{
    public class BrokerHelper
    {
        public static string GenerateCommandTopic(Guid projectId, string deviceId)
        {
            return $"{projectId}/devices/{deviceId}/commands";
        }
    }
}
