using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using OTT.Lib.Kafka;
using OTT.Lib.Kafka.Extensions;
using Phoenix.AsyncHandler.Kafka;
using Phoenix.Generated.Api.Events.Logical.Gdpr.HouseholdRetentionPeriodExpired;
using Phoenix.Generated.Api.Events.Logical.Gdpr.ObjectCleanupComplete;

namespace Phoenix.AsyncHandler.Recording
{
    public class HouseholdRetentionPeriodExpiredHandler : IKafkaMessageHandler<HouseholdRetentionPeriodExpired>
    {
        private readonly IKafkaProducer<string, ObjectCleanupComplete> _producer;
        private readonly ILogger<HouseholdRetentionPeriodExpiredHandler> _logger;

        public HouseholdRetentionPeriodExpiredHandler(IKafkaProducerFactory producerFactory,
            IKafkaContextProvider kafkaContextProvider,
            ILogger<HouseholdRetentionPeriodExpiredHandler> logger)
        {
            _producer = producerFactory.Get<string, ObjectCleanupComplete>(kafkaContextProvider,
                Partitioner.Murmur2Random);
            _logger = logger;
        }

        public Task<HandleResult> Handle(OTT.Lib.Kafka.ConsumeResult<string, HouseholdRetentionPeriodExpired> consumeResult)
        {
            var r = consumeResult.GetValue();
            _logger.LogInformation("Performing GDPR cleanup. household [{id}]", r.HouseholdId);
            // TODO the cleanup

            var cleanupComplete = new ObjectCleanupComplete
            {
                PartnerId = r.PartnerId,
                ServiceName = "phoenix-recording",
                CleanedObjectId = r.HouseholdId,
                SourceMessageTypeId = HouseholdRetentionPeriodExpired.GetTopic()
            };
            _producer.Produce(ObjectCleanupComplete.GetTopic(), cleanupComplete.GetPartitioningKey(), cleanupComplete);
            return Task.FromResult(Result.Ok);
        }
    }
}
