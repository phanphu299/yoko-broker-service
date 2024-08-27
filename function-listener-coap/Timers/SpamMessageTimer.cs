using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AHI.Broker.Function.Trigger.ServiceBus;
using AHI.Infrastructure.Bus.ServiceBus.Abstraction;
using Microsoft.Azure.WebJobs;

namespace AHI.Broker.Function.Trigger.Timer
{
    public class SpamMessageTimer
    {
        private readonly IDomainEventDispatcher _domainEventDispatcher;
        // private readonly ILoggerAdapter<SpammerBackgroundService> _logger;

        public SpamMessageTimer(IDomainEventDispatcher domainEventDispatcher
                                        //, ILoggerAdapter<SpammerBackgroundService> logger
                                        )
        {
            _domainEventDispatcher = domainEventDispatcher;
            // _logger = logger;
        }

        [FunctionName("SpammerTrigger")]
        public async Task Run([TimerTrigger("* */1 * * * *")] TimerInfo timerInfo, CancellationToken stoppingToken)
        {
            var count = 1;
            while (!stoppingToken.IsCancellationRequested && count < 100)
            {
                var dict = new Dictionary<string, object>();
                var randomProjectId = Guid.NewGuid().ToString();
                var randomSubscriptionId = Guid.NewGuid().ToString();
                var randomTenantId = Guid.NewGuid().ToString();

                dict.Add("ProjectId", randomProjectId);
                dict.Add("SubscriptionId", randomSubscriptionId);
                dict.Add("TenantId", randomTenantId);
                dict.Add("value", $"{DateTime.UtcNow.ToString("G")} - {new Random().Next()}");

                var message = new IngestionMessage(dict, ProjectInfo.Create(randomProjectId, randomSubscriptionId, randomTenantId));
                await _domainEventDispatcher.SendAsync(message);
                count++;
            }
        }
    }
}
