using System.Collections.Generic;
using AHI.Broker.Function.Constant;
using AHI.Broker.Function.FileParser.Abstraction;
using AHI.Broker.Function.FileParser.ErrorTracking.Model;

namespace AHI.Broker.Function.FileParser.ErrorTracking
{
    public abstract class BaseExportTrackingService : IExportTrackingService
    {
        protected ICollection<TrackError> _currentErrors { get; set; }
        public ICollection<TrackError> GetErrors => _currentErrors;

        public bool HasError => (_currentErrors?.Count ?? -1) > 0;

        public virtual void RegisterError(string message, ErrorType errorType = ErrorType.UNDEFINED, IDictionary<string, object> validationInfo = null)
        {
            _currentErrors.Add(new TrackError(message, errorType, validationInfo));
        }
    }
}
