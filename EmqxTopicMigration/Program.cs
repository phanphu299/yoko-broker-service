using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace EmqxTopicMigration
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            foreach (var s in args)
                Console.WriteLine("params: " + s);

            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            var configuration = configBuilder.Build();
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging();
            serviceCollection.AddSingleton<IConfiguration>(configuration);
            serviceCollection.AddScoped<EmqxTopicMigrationService>();
            serviceCollection.AddScoped<RedisMigrationService>();
            var serviceProvider = serviceCollection.BuildServiceProvider();

            var cmd = args[0];
            string? brokerConnectionStr;
            switch (cmd)
            {
                case "database":
                {
                    brokerConnectionStr = args[1].Replace("%20", " ");
                    var deviceConnectionStr = args[2].Replace("%20", " ");
                    var migrationService = serviceProvider.GetService<EmqxTopicMigrationService>();
                    var migrationCmd = new DatabaseMigrationCmd(brokerConnectionStr, deviceConnectionStr);
                    await migrationService.Migrate(migrationCmd);
                    break;
                }
                case "redis":
                {
                    brokerConnectionStr = args[1].Replace("%20", " ");
                    var redisConnectionStr = args[2].Replace("%20", " ");
                    var redisMigrationService = serviceProvider.GetRequiredService<RedisMigrationService>();
                    await redisMigrationService.Migrate(new RedisMigrationCmd(brokerConnectionStr, redisConnectionStr));
                    break;
                }
            }
        }
    }
}