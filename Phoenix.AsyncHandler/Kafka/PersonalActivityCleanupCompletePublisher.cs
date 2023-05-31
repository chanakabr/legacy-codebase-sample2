using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using OTT.Lib.Kafka;
using Phoenix.Generated.Api.Events.Logical.PersonalActivityCleanupComplete;
using SchemaRegistryEvents;

namespace Phoenix.AsyncHandler.Kafka
{
    public class PersonalActivityCleanupCompletePublisher :IPersonalActivityCleanupCompletePublisher
    {
        private readonly IKafkaProducer<string, PersonalActivityCleanupComplete> _personalActivityCleanupCompleteProducer;
        private readonly ILogger<PersonalActivityCleanupCompletePublisher> _logger;

        public PersonalActivityCleanupCompletePublisher(
            IKafkaProducerFactory producerFactory,
            IKafkaContextProvider contextProvider,
            ILogger<PersonalActivityCleanupCompletePublisher> logger)
        {
            _personalActivityCleanupCompleteProducer = producerFactory.Get<string, PersonalActivityCleanupComplete>(contextProvider, Partitioner.Murmur2Random);
            _logger = logger;
        }

        public void Publish(long partnerId, long key)
        {
            if (_personalActivityCleanupCompleteProducer == null)
            {
                _logger.LogError($"{nameof(PersonalActivityCleanupComplete)} message with parameters {nameof(partnerId)}={partnerId}," +
                                 $"can not be published : {nameof(_personalActivityCleanupCompleteProducer)} is null.");
                return;
            }

            var personalActivityCleanupComplete = new PersonalActivityCleanupComplete
            {
                ServiceName = SourceService.Phoenix,
                PartnerId = partnerId,
                Key = key
            };
            
            _personalActivityCleanupCompleteProducer.Produce(
                PersonalActivityCleanupComplete.GetTopic(),
                personalActivityCleanupComplete.GetPartitioningKey(),
                personalActivityCleanupComplete);
        }
    }
}