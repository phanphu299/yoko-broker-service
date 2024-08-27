using System.Collections.Generic;
using AHI.Broker.Function.Constant;
using AHI.Broker.Function.FileParser.ErrorTracking.Abstraction;
using AHI.Broker.Function.FileParser.ErrorTracking.Model;
using AHI.Broker.Function.FileParser.Model;

namespace AHI.Broker.Function.FileParser.ErrorTracking
{
    public class JsonTrackingService : BaseImportTrackingService, IJsonTrackingService
    {

        public override void RegisterError(string message, TrackModel model, string propName, ErrorType errorType = ErrorType.UNDEFINED, IDictionary<string, object> validationInfo = null)
        {
            RegisterError(message, errorType, validationInfo);
        }

        protected override FileTrackInfo InitTrackInfo(TrackModel model, int fileIndex)
        {
            return new JsonTrackInfo
            {
                Index = fileIndex
            };
        }
    }
}
