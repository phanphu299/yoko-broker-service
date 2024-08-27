namespace AHI.Broker.Function.Model.ImportModel
{
    public class EvenHub
    {
        public string Tier { get; set; }
        public string BubName { get; set; }
        public int ThroughputUnits { get; set; }
        public bool AutoInplate { get; set; }
        public int MaximumThroughputUnits { get; set; }
    }
}
