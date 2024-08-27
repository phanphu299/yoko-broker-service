using System.Collections.Generic;
using AHI.Broker.Function.FileParser.ErrorTracking.Model;

namespace AHI.Broker.Function.FileParser.ErrorTracking
{
    public class ExportTrackingService : BaseExportTrackingService
    {
        public ExportTrackingService()
        {
            _currentErrors = new List<TrackError>();
        }
    }
}
