using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace EmqxTopicMigration
{
    public class EmqxTopicMigrationService
    {
        private readonly ILogger<EmqxTopicMigrationService> _logger;
        private readonly IConfiguration _configuration;

        public EmqxTopicMigrationService(ILogger<EmqxTopicMigrationService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public async Task Migrate(DatabaseMigrationCmd databaseMigrationCmd)
        {
            _logger.LogInformation($"Broker:{databaseMigrationCmd.BrokerConnectionString}");
            _logger.LogInformation($"Device:{databaseMigrationCmd.DeviceConnectionString}");
            const string generateInsertTopicSqlCommand = @"
       select 'IF NOT EXISTS (select id from dbo.emqx_topics where ' ||
       'broker_id=' || QUOTE_LITERAL(emqx.bId) || ' and ' ||
       'client_id=' || QUOTE_LITERAL(emqx.usr) || ' and ' ||
       'topic_name=' || QUOTE_LITERAL(emqx.tp) || ' and ' ||
       'deleted=0'
       ')'
       'begin ' ||
          'INSERT INTO dbo.emqx_topics (id, broker_id, client_id, access_token, topic_name, created_utc, updated_utc, deleted) VALUES (' ||
          'newid()' || ', ' ||
          QUOTE_LITERAL(emqx.bId) || ', ' ||
          QUOTE_LITERAL(emqx.usr) || ', ' ||
          QUOTE_LITERAL(emqx.pas) || ', ' ||
          QUOTE_LITERAL(emqx.tp) || ', ' ||
          'getutcdate()' || ', ' ||
          'getutcdate()' || ', 0)' ||
          'end;'

   from (select device_content::json ->> 'brokerId' bId,
                device_content::json ->> 'username' usr,
                device_content::json ->> 'password' pas,
                telemetry_topic                     tp
         from devices
         where device_content is not null
           and telemetry_topic is not null
           and device_content::json ->> 'brokerType' like 'BROKER_EMQX_%'
         union all

         select device_content::json ->> 'brokerId' bId,
                device_content::json ->> 'username' usr,
                device_content::json ->> 'password' pas,
                command_topic                       tp
         from devices
         where device_content is not null
           and command_topic is not null
           and device_content::json ->> 'brokerType' like 'BROKER_EMQX_%'
           and has_command) emqx;";

            Console.WriteLine("Pgl:---" + databaseMigrationCmd.DeviceConnectionString);
            Console.WriteLine("Sql:---" + databaseMigrationCmd.BrokerConnectionString);

            await using var deviceConnection = new NpgsqlConnection(databaseMigrationCmd.DeviceConnectionString);
            await using var brokerConnection = new SqlConnection(databaseMigrationCmd.BrokerConnectionString);
            var insertCmds = await deviceConnection.QueryAsync<string>(generateInsertTopicSqlCommand);
            Console.WriteLine("Total command for inserting: " + insertCmds.Count());
            foreach (var insertCmd in insertCmds)
            {
                _logger.LogInformation(insertCmd);
            }

            await brokerConnection.OpenAsync();
            await using var tras = await brokerConnection.BeginTransactionAsync();
            try
            {
                foreach (var insertCmd in insertCmds)
                {
                    await brokerConnection.ExecuteAsync(insertCmd, insertCmd, tras);
                }
            }
            catch (Exception)
            {
                await tras.RollbackAsync();
                throw;
            }

            await tras.CommitAsync();
        }
    }
}