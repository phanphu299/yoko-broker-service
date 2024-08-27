using System;

namespace AHI.Broker.Function.Model
{
    public class RemoveDeviceRequest
    {
        public Guid BrokerId { get; set; }
        public string ClientId { get; set; }
    }
}
