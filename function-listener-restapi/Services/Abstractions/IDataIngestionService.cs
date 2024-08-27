using System;
using System.Threading.Tasks;

namespace AHI.Broker.Function.Service.Abstraction
{
    public interface IDataIngestionService
    {
        Task IngestDataAsync(Guid brokerId, string contentType, byte[] contentStream, string fileName);
        Task IngestBatchDataAsync(Guid brokerId, byte[] rawInput);
    }
}
