using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using OTT.Lib.Kafka;
using Phoenix.Generated.Api.Events.Logical.HouseholdRecordingMigrationStatus;

namespace Phoenix.AsyncHandler.Kafka
{
    public class HouseholdRecordingMigrationPublisher : IHouseholdRecordingMigrationPublisher
    {
        private readonly IKafkaProducer<string, HouseholdRecordingMigrationStatus> _producer;
        private readonly ILogger<HouseholdRecordingMigrationPublisher> _logger;

        public HouseholdRecordingMigrationPublisher(
            IKafkaProducerFactory producerFactory,
            IKafkaContextProvider contextProvider,
            ILogger<HouseholdRecordingMigrationPublisher> logger)
        {
            _producer = producerFactory.Get<string, HouseholdRecordingMigrationStatus>(contextProvider, Partitioner.Murmur2Random);
            _logger = logger;
        }

        public void Publish(HouseholdRecordingMigrationStatus status)
        {
            if (_producer == null)
            {
                _logger.LogError($"{nameof(HouseholdRecordingMigrationStatus)} message with parameters {nameof(status.PartnerId)}={status.PartnerId}," +
                                 $"can not be published : {nameof(_producer)} is null.");
                return;
            }

            _producer.Produce(HouseholdRecordingMigrationStatus.GetTopic(), status.GetPartitioningKey(), status);
        }
    }
}
