using System.Threading;
using System.Threading.Tasks;
using AHI.Infrastructure.Exception;
using Broker.Application.Handler.Command;
using Broker.Application.Handler.Command.Model;
using Broker.Application.Service.Abstraction;
using Microsoft.EntityFrameworkCore;
using Broker.Application.Repository;
using System.Linq;
using Broker.Application.Constant;
using System.Linq.Dynamic.Core;

namespace Broker.Application.Service
{
    public class SchemaService : ISchemaService
    {
        private readonly IUnitOfWork _unitOfWork;

        public SchemaService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IntegrationSchemaDto> FindByTypeAsync(GetSchemaByType command, CancellationToken token)
        {
            var schema = await _unitOfWork.Schemas.AsQueryable().Include(x => x.Details).ThenInclude(x => x.Options).FirstOrDefaultAsync(x => x.Type == command.Type);
            if (schema is null)
                throw new EntityNotFoundException("No schema found.");

            var dto = IntegrationSchemaDto.Create(schema);
            if (BrokerTypeConstants.EMQX_BROKERS.Contains(command.Type))
            {
                dto.Details = dto.Details.Where(x => !BrokerContentKeys.BROKER_SCHEMA_EXCEPT_DETAIL_KEYS.Any(a => a == x.Key)).ToArray();
            }
            return dto;
        }
    }
}
