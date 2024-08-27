namespace EmqxTopicMigration
{
    public class DatabaseMigrationCmd
    {
        public  string BrokerConnectionString { get; set; }
        public  string DeviceConnectionString { get; set; }

        public DatabaseMigrationCmd(string brokerConnectionString, string deviceConnectionString)
        {
            BrokerConnectionString = brokerConnectionString;
            DeviceConnectionString = deviceConnectionString;
        }
    }
}