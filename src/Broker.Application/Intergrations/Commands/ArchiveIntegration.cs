using Broker.Application.Handler.Command.Model;
using MediatR;
using System;
using System.Collections.Generic;

namespace Broker.Application.Handler.Command
{
    public class ArchiveIntegration : IRequest<IEnumerable<ArchiveIntegrationDto>>
    {
        public DateTime ArchiveTime { get; set; } = DateTime.UtcNow;
    }
}
