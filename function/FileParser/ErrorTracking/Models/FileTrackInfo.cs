using System.Collections.Generic;

namespace AHI.Broker.Function.FileParser.ErrorTracking.Model
{
    public abstract class FileTrackInfo
    {
        public int Index { get; set; }
    }

    public class JsonTrackInfo : FileTrackInfo
    {

    }

    public class ModelTrackInfo
    {
        public IDictionary<int, FileTrackInfo> TrackObjectInfos = new Dictionary<int, FileTrackInfo>();
    }
}
