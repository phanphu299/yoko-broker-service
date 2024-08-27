using System.Collections.Generic;
using AHI.Broker.Function.Constant;
using AHI.Broker.Function.FileParser.ErrorTracking.Model;
using AHI.Broker.Function.FileParser.Model;

namespace AHI.Broker.Function.FileParser.Abstraction
{
    public interface IErrorService
    {
        bool HasError { get; }

        void RegisterError(string message, ErrorType errorType = ErrorType.UNDEFINED, IDictionary<string, object> validationInfo = null);
    }

    public interface IImportTrackingService : IErrorService
    {
        IDictionary<string, ICollection<TrackError>> FileErrors { get; }
        string File { get; set; }
        void Track(TrackModel model, int fileIndex);
        void RegisterError(string message, TrackModel model, string propName, ErrorType errorType = ErrorType.UNDEFINED, IDictionary<string, object> validationInfo = null);
    }

    public interface IExportTrackingService : IErrorService
    {
        ICollection<TrackError> GetErrors { get; }
    }
}
