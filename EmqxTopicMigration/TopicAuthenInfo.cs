namespace EmqxTopicMigration
{
    public class TopicAuthenInfo
    {
        public string? Username { get; set; }
        public string? Token { get; set; }
        public IEnumerable<string?>? Topic { get; set; }
    }
}