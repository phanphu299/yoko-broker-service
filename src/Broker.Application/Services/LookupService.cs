using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AHI.Infrastructure.Exception;
using Broker.Application.Repository;
using Broker.Application.Service.Abstractions;

namespace Broker.Application.Service.Abstraction
{
    public class LookupService : ILookupService
    {
        private readonly IUnitOfWork _unitOfWork;
        public LookupService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Domain.Entity.Lookup> ProcessLookUpFromConfigurationServiceAsync(string code, CancellationToken token)
        {
            // Check the validity of lookup code.
            var lookupCode = code;
            var lookup = await _unitOfWork.Lookups.FindLookupByCodeAsync(lookupCode, token);
            if (lookup == null)
                throw new EntityNotFoundException($"Lookup with code {lookupCode} is not found.");

            // Lookup is not active.
            if (!lookup.Active)
                throw new EntityNotFoundException($"Lookup with code {lookupCode} is not found as ACTIVE");
            //save or update lookup
            return await SaveAsync(lookup);
        }

        public Task<Domain.Entity.Lookup> SaveAsync(Domain.Entity.Lookup entity)
        {
            return _unitOfWork.Lookups.SaveAsync(entity);
        }
    }

}
