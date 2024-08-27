using System.Threading;
using System.Threading.Tasks;
using MediatR;
using AHI.Infrastructure.SharedKernel.Model;
using Broker.Application.Service.Abstraction;

namespace Broker.Application.FileRequest.Command.Handler
{
    public class ImportFileRequestHandler : IRequestHandler<ImportFile, BaseResponse>
    {
        private readonly IFileEventService _fileEventService;
        public ImportFileRequestHandler(IFileEventService fileEventService)
        {
            _fileEventService = fileEventService;
        }

        public async Task<BaseResponse> Handle(ImportFile request, CancellationToken cancellationToken)
        {
            await _fileEventService.SendImportEventAsync(request.ObjectType, request.FileNames);
            return new BaseResponse(true, "starting import");
        }

    }
}
