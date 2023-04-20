using System;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using OTT.Lib.Kafka;
using Phoenix.AsyncHandler.Pricing;
using Phoenix.Generated.Api.Events.Logical.PersonalActivityCleanupComplete;

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

        public void Publish(long partnerId, PersonalActivityCleanupStatus status, string description)
        {
            if (_personalActivityCleanupCompleteProducer == null)
            {
                _logger.LogError($"{nameof(PersonalActivityCleanupComplete)} message with parameters {nameof(partnerId)}={partnerId}," +
                                 $"can not be published : {nameof(_personalActivityCleanupCompleteProducer)} is null.");
                return;
            }

            var personalActivityCleanupComplete = new PersonalActivityCleanupComplete
            {
                ServiceName = "Phoenix",
                PartnerId = partnerId,
                ExecutionTimeEpoch = 0,
                Result = new Generated.Api.Events.Logical.PersonalActivityCleanupComplete.Result
                {
                    Description = description,
                    Status = Enum.GetName(typeof(PersonalActivityCleanupStatus), status)
                }
            };
            
            _personalActivityCleanupCompleteProducer.Produce(
                PersonalActivityCleanupComplete.GetTopic(),
                personalActivityCleanupComplete.GetPartitioningKey(),
                personalActivityCleanupComplete);
        }
    }
}