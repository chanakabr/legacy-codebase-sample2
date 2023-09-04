using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OTT.Service.Kronos;
using Phoenix.Generated.Tasks.Recurring.EpgV3Cleanup;
using Phoenix.Generated.Tasks.Recurring.LiveToVodTearDown;
using Phoenix.Generated.Tasks.Recurring.RecordingsCleanup;
using Phoenix.Generated.Tasks.Recurring.RecordingsLifetime;
using Phoenix.Generated.Tasks.Recurring.RecordingsScheduledTasks;
using Phoenix.Generated.Tasks.Recurring.ScheduleRecordingEvictions;
using Phoenix.SetupJob.Configuration;
using Phx.Lib.Log;

namespace Phoenix.SetupJob
{
    class Program
    {
        private const string ClientId = "phoenix";

        
        static void Main(string[] args)
        {
            KLogger.Configure("log4net.config", KLogEnums.AppType.WindowsService);
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.ClearProviders().AddProvider(new KLoggerProvider());
            });

            var logger = loggerFactory.CreateLogger<Program>();
            logger.LogInformation("starting phoenix setup job.");

            IConfiguration configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .Build();
            var setupConfiguration = configuration.Get<PhoenixSetupJobConfiguration>();
            var kronosAsyncClient = new KronosAsyncClient(
                ClientId,
                setupConfiguration.KronosServiceId,
                setupConfiguration.KafkaConnectionString,
                loggerFactory,
                new PhoenixSetupTracingProvider());


            var request = new RegisterServiceRecurringTasksRequest
            {
                ServiceName = ClientId,
                Tasks =
                {
                    {
                        // 1m, recording cleanup
                        RecordingsScheduledTasks.RecordingsScheduledTasksQualifiedName,
                        new RegisterServiceRecurrenceTaskItem
                        {
                            CronExpression = "@every 1m",
                            TimeoutSecs = 100
                        }
                    },
                    { 
                        LiveToVodTearDown.LiveToVodTearDownQualifiedName,
                        new RegisterServiceRecurrenceTaskItem
                        {
                            CronExpression = "@every 1h",
                            TimeoutSecs = 600
                        }
                    },
                    { //1d, recording cleanup
                        RecordingsCleanup.RecordingsCleanupQualifiedName,
                        new RegisterServiceRecurrenceTaskItem
                        {
                            CronExpression = "@every 1d",
                            TimeoutSecs = 200
                        }
                    },
                    { //1h, recording cleanup
                        RecordingsLifetime.RecordingsLifetimeQualifiedName,
                        new RegisterServiceRecurrenceTaskItem
                        {
                            CronExpression = "@every 1h",
                            TimeoutSecs = 200
                        }
                    },
                    {
                        EpgV3Cleanup.EpgV3CleanupQualifiedName,
                        new RegisterServiceRecurrenceTaskItem 
                        { 
                            CronExpression = "@every 4h",
                            TimeoutSecs=600
                        }
                    },
                    { 
                        ScheduleRecordingEvictions.ScheduleRecordingEvictionsQualifiedName,
                        new RegisterServiceRecurrenceTaskItem
                        {
                            CronExpression = "@every 1h",
                            TimeoutSecs = 200
                        }
                    },
                }
            };
            
            var reqId = kronosAsyncClient.RegisterServiceRecurringTasks(request);

            logger.LogInformation($"phoenix setup job completed, kronos requestid:[{reqId}]");
            
            // temporary solution, because current implementation of kronos client doesn't allow to wait while kafka message is published.
            // TODO: Remove it and update client version after https://kaltura.atlassian.net/browse/BEO-12400 will be fixed.
            Task.WaitAll(Task.Delay(TimeSpan.FromSeconds(10)));
        }
    }
}