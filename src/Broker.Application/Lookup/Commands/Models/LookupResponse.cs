namespace Broker.Application.Handler.Command.Model
{
    public class LookupResponse
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Color { get; set; }
        public bool Active { get; set; }
        public LookupTypeDto LookupType { get; set; }
    }
}
