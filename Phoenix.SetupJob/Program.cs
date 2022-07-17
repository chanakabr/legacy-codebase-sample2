using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using OTT.Service.Kronos;
using Phoenix.Generated.Tasks.Recurring.LiveToVodTearDown;
using Phoenix.SetupJob.Configuration;

namespace Phoenix.SetupJob
{
    class Program
    {
        private const string ClientId = "phoenix";
        
        static void Main(string[] args)
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .Build();
            var setupConfiguration = configuration.Get<PhoenixSetupJobConfiguration>();
            var kronosAsyncClient = new KronosAsyncClient(
                ClientId,
                setupConfiguration.KronosServiceId,
                setupConfiguration.KafkaConnectionString,
                NullLoggerFactory.Instance,
                new PhoenixSetupTracingProvider());
            var request = new RegisterServiceRecurringTasksRequest
            {
                ServiceName = ClientId,
                Tasks =
                {
                    {
                        LiveToVodTearDown.LiveToVodTearDownQualifiedName,
                        new RegisterServiceRecurrenceTaskItem
                        {
                            CronExpression = "@every 1h",
                            TimeoutSecs = 600
                        }
                    }
                }
            };
            
            kronosAsyncClient.RegisterServiceRecurringTasks(request);
            
            // temporary solution, because current implementation of kronos client doesn't allow to wait while kafka message is published.
            // TODO: Remove it and update client version after https://kaltura.atlassian.net/browse/BEO-12400 will be fixed.
            Task.WaitAll(Task.Delay(TimeSpan.FromSeconds(10)));
        }
    }
}