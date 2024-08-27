using System.Threading;
using System.Threading.Tasks;
using Broker.Application.Handler.Command;
using Broker.Application.Handler.Command.Model;
using Broker.Application.Service.Abstraction;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Configuration.Application.Constant;
using Broker.Application.Repository;

namespace Broker.Application.Service
{
    public class GreenKonceptService : IGreenKonceptService
    {
        private readonly IUnitOfWork _unitOfWork;

        public GreenKonceptService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<GreenKonceptDto>> GetAllGreenKonceptAsync(GetValidGreenKoncept request, CancellationToken cancellationToken)
        {
            var integrations = await _unitOfWork.Integrations.AsQueryable().Include(x => x.Detail).Where(x => x.Detail != null && x.Type == IntegrationTypeConstants.GREEN_KONCEPT).Select(x => x).ToListAsync();
            var result = new List<GreenKonceptDto>();
            foreach (var integration in integrations)
            {
                var content = JsonConvert.DeserializeObject<GreenKonceptInfomation>(integration.Detail.Content);
                result.Add(new GreenKonceptDto()
                {
                    Id = integration.Id,
                    Endpoint = content.Endpoint,
                    ClientId = content.ClientId,
                    ClientSecret = content.ClientSecret,
                    IntegrationId = integration.Id,
                    Status = integration.Status
                });
            }
            return result;
        }
    }
}
