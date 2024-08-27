using AHI.Broker.Function.Model.ImportModel;
using AHI.Broker.Function.Service.FileImport.Abstraction;
using AHI.Infrastructure.Import;
using AHI.Infrastructure.Import.Abstraction;

namespace AHI.Broker.Function.Service.FileImport
{
    public class BrokerImportService : BaseFileImport<BrokerModel>, IBrokerImportService
    {
        private readonly IFileHandler<BrokerModel> _fileHandler;
        private readonly IImportRepository<BrokerModel> _repository;
        public BrokerImportService(IFileHandler<BrokerModel> fileHandler, IImportRepository<BrokerModel> repository)
        {
            _fileHandler = fileHandler;
            _repository = repository;
        }
        protected override IFileHandler<BrokerModel> GetFileHandler()
        {
            return _fileHandler;
        }

        protected override IImportRepository<BrokerModel> GetRepository()
        {
            return _repository;
        }
    }
}
