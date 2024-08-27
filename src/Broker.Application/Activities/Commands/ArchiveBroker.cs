using System;
using System.Collections.Generic;
using Broker.Application.Handler.Command.Model;
using MediatR;

namespace Broker.Application.Handler.Command
{
    public class ArchiveBroker : IRequest<IEnumerable<ArchiveBrokerDto>>
    {
        public DateTime ArchiveTime { get; set; } = DateTime.UtcNow;
    }
}
