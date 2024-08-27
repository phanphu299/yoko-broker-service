using Broker.Application.Handler.Command.Model;
using MediatR;
using System.Collections.Generic;

namespace Broker.Application.Handler.Command
{
    public class GetValidGreenKoncept : IRequest<IEnumerable<GreenKonceptDto>>
    {
    }
}
