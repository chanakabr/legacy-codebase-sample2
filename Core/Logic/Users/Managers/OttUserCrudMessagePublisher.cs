using System;
using System.Threading;
using ApiLogic.Context;
using Confluent.Kafka;
using EventBus.Kafka;
using OTT.Lib.Kafka;
using Phoenix.Generated.Api.Events.Crud.OttUser;
using SchemaRegistryEvents;
using SchemaRegistryEvents.Catalog;

namespace ApiLogic.Users.Managers
{
    // TODO should be moved to ApiObjects after Domain class will be moved to ApiObject
    public interface IOttUserCrudMessagePublisher
    {
        void Delete(long id);
    }

    public class OttUserCrudMessagePublisher : IOttUserCrudMessagePublisher
    {
        private static readonly Lazy<IOttUserCrudMessagePublisher> LazyInstance =
            new Lazy<IOttUserCrudMessagePublisher>(
                () => new OttUserCrudMessagePublisher(
                    KafkaProducerFactoryInstance.Get().Get<string, OttUser>(WebKafkaContextProvider.Instance, Partitioner.Murmur2Random),
                    WebKafkaContextProvider.Instance),
                LazyThreadSafetyMode.PublicationOnly);
        public static readonly IOttUserCrudMessagePublisher Instance = LazyInstance.Value;

        private readonly IKafkaProducer<string, OttUser> _producer;
        private readonly IKafkaContextProvider _contextProvider;

        public OttUserCrudMessagePublisher(IKafkaProducer<string, OttUser> producer, IKafkaContextProvider contextProvider)
        {
            _producer = producer ?? throw new ArgumentNullException(nameof(producer));
            _contextProvider = contextProvider ?? throw new ArgumentNullException(nameof(contextProvider));
        }
        
        public void Delete(long id)
        {
            Produce(new OttUser
            {
                Id = id,
                Operation = CrudOperationType.DELETE_OPERATION
            });
        }

        private void Produce(OttUser user)
        {
            user.PartnerId = _contextProvider.GetPartnerId() ?? 0;
            _producer.Produce(OttUser.GetTopic(), user.GetPartitioningKey(), user, SourceService.PhoenixHeader);
        }
    }
}
