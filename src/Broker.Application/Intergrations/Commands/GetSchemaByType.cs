using Broker.Application.Handler.Command.Model;
using AHI.Infrastructure.SharedKernel.Model;
using MediatR;

namespace Broker.Application.Handler.Command
{
    public class GetSchemaByType : BaseCriteria, IRequest<IntegrationSchemaDto>
    {
        public string Type { get; set; }
        public GetSchemaByType(string type)
        {
            Type = type;
        }
    }
}
