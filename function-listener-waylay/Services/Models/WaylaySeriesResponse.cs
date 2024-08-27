namespace Function.Service.Model
{
    public class WaylaySeriesResponse
    {
        public string Name { get; set; }
        public WaylaySeriesTimestamp Latest { get; set; }
    }
    public class WaylaySeriesTimestamp
    {
        public long Timestamp { get; set; }
        public object Value { get; set; }
    }
}