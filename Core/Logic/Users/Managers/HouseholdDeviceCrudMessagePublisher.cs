using System;
using System.Threading;
using ApiLogic.Context;
using Confluent.Kafka;
using EventBus.Kafka;
using OTT.Lib.Kafka;
using Phoenix.Generated.Api.Events.Crud.HouseholdDevice;
using SchemaRegistryEvents;
using SchemaRegistryEvents.Catalog;

namespace ApiLogic.Users.Managers
{
    // TODO should be moved to ApiObjects after Domain class will be moved to ApiObject
    public interface IHouseholdDeviceCrudMessagePublisher
    {
        void Delete(long id, string udid);
    }

    public class HouseholdDeviceCrudMessagePublisher : IHouseholdDeviceCrudMessagePublisher
    {
        private static readonly Lazy<IHouseholdDeviceCrudMessagePublisher> LazyInstance =
            new Lazy<IHouseholdDeviceCrudMessagePublisher>(
                () => new HouseholdDeviceCrudMessagePublisher(
                    KafkaProducerFactoryInstance.Get().Get<string, HouseholdDevice>(WebKafkaContextProvider.Instance, Partitioner.Murmur2Random),
                    WebKafkaContextProvider.Instance),
                LazyThreadSafetyMode.PublicationOnly);
        public static readonly IHouseholdDeviceCrudMessagePublisher Instance = LazyInstance.Value;

        private readonly IKafkaProducer<string, HouseholdDevice> _producer;
        private readonly IKafkaContextProvider _contextProvider;

        public HouseholdDeviceCrudMessagePublisher(IKafkaProducer<string, HouseholdDevice> producer, IKafkaContextProvider contextProvider)
        {
            _producer = producer ?? throw new ArgumentNullException(nameof(producer));
            _contextProvider = contextProvider ?? throw new ArgumentNullException(nameof(contextProvider));
        }
        
        public void Delete(long id, string udid)
        {
            Produce(new HouseholdDevice
            {
                Id = id,
                Udid = udid,
                Operation = CrudOperationType.DELETE_OPERATION
            });
        }

        private void Produce(HouseholdDevice householdDevice)
        {
            householdDevice.PartnerId = _contextProvider.GetPartnerId() ?? 0;
            _producer.Produce(HouseholdDevice.GetTopic(), householdDevice.GetPartitioningKey(), householdDevice, SourceService.PhoenixHeader);
        }
    }
}
