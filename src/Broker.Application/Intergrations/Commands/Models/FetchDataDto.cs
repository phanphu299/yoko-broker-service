namespace Broker.Application.Handler.Command.Model
{
    public class FetchDataDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public object Items { get; set; }
    }
}
