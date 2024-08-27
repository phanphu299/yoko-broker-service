using System;
using System.Collections.Generic;

namespace AHI.Broker.Function.Model
{
    public class RemoveEmqxBrokersRequest
    {
        public IEnumerable<Guid> BrokerIds { get; set; }
    }
}
