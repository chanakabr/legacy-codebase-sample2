using System;
using System.Threading;
using ApiLogic.Context;
using Confluent.Kafka;
using EventBus.Kafka;
using OTT.Lib.Kafka;
using Phoenix.Generated.Api.Events.Crud.Household;
using SchemaRegistryEvents;
using SchemaRegistryEvents.Catalog;

namespace ApiLogic.Users.Managers
{
    // TODO should be moved to ApiObjects after Domain class will be moved to ApiObject
    public interface IHouseholdCrudMessagePublisher
    {
        void Delete(long id);
    }

    public class HouseholdCrudMessagePublisher : IHouseholdCrudMessagePublisher
    {
        private static readonly Lazy<IHouseholdCrudMessagePublisher> LazyInstance =
            new Lazy<IHouseholdCrudMessagePublisher>(
                () => new HouseholdCrudMessagePublisher(
                    KafkaProducerFactoryInstance.Get().Get<string, Household>(WebKafkaContextProvider.Instance, Partitioner.Murmur2Random),
                    WebKafkaContextProvider.Instance),
                LazyThreadSafetyMode.PublicationOnly);
        public static readonly IHouseholdCrudMessagePublisher Instance = LazyInstance.Value;

        private readonly IKafkaProducer<string, Household> _producer;
        private readonly IKafkaContextProvider _contextProvider;

        public HouseholdCrudMessagePublisher(IKafkaProducer<string, Household> producer, IKafkaContextProvider contextProvider)
        {
            _producer = producer ?? throw new ArgumentNullException(nameof(producer));
            _contextProvider = contextProvider ?? throw new ArgumentNullException(nameof(contextProvider));
        }
        
        public void Delete(long id)
        {
            Produce(new Household
            {
                Id = id,
                Operation = CrudOperationType.DELETE_OPERATION
            });
        }

        private void Produce(Household household)
        {
            household.PartnerId = _contextProvider.GetPartnerId() ?? 0;
            _producer.Produce(Household.GetTopic(), household.GetPartitioningKey(), household, SourceService.PhoenixHeader);
        }
    }
}
