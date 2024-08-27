using Broker.Application.Handler.Command.Model;
using AHI.Infrastructure.SharedKernel.Model;
using MediatR;
using System;

namespace Broker.Application.Handler.Command
{
    public class FetchIntegrationData : BaseCriteria, IRequest<BaseSearchResponse<FetchDataDto>>
    {
        public Guid Id { get; set; }
        public string Type { get; set; }
        public string Data { get; set; }
        public FetchIntegrationData(Guid id, string type, string data)
        {
            Id = id;
            Type = type;
            Data = data;
        }
    }
}
