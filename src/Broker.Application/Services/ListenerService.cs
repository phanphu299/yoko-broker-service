using System.Threading;
using System.Threading.Tasks;
using Broker.Application.Service.Abstraction;
using System;
using Microsoft.EntityFrameworkCore;
using Broker.Application.Repository;

namespace Broker.Application.Service
{
    public class ListenerService : IListenerService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ListenerService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> ActiveAsync(Guid id, CancellationToken cancellationToken)
        {
            var broker = await _unitOfWork.Brokers.AsQueryable().FirstOrDefaultAsync(x => x.Id == id);
            if (broker != null)
            {
                broker.Status = "AC";
                await _unitOfWork.Brokers.UpdateAsync(id, broker);
                return true;
            }
            else
            {
                var integration = await _unitOfWork.Integrations.AsQueryable().FirstOrDefaultAsync(x => x.Id == id);
                if (integration != null)
                {
                    integration.Status = "AC";
                    await _unitOfWork.Integrations.UpdateAsync(id, integration);
                    return true;
                }
            }
            return false;
        }

    }
}
