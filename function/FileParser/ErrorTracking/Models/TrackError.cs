using Newtonsoft.Json;
using AHI.Broker.Function.Constant;
using System.Collections.Generic;

namespace AHI.Broker.Function.FileParser.ErrorTracking.Model
{
    public class TrackError
    {
        [JsonProperty(Order = -2)]
        public string Type { get; set; }
        public string Message { get; set; }
        public IDictionary<string, object> ValidationInfo { get; set; }
        public TrackError(string message, ErrorType errorType = ErrorType.UNDEFINED, IDictionary<string, object> validationInfo = null)
        {
            this.Message = message;
            this.Type = errorType.ToString();
            this.ValidationInfo = validationInfo;
        }
    }

    public class JsonTrackError : TrackError
    {
        public JsonTrackError(string message, ErrorType errorType = ErrorType.UNDEFINED, IDictionary<string, object> validationInfo = null)
            : base(message, errorType, validationInfo)
        {
        }
    }
}
