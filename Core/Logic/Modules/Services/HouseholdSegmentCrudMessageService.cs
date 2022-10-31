using System.Threading.Tasks;
using Confluent.Kafka;
using EventBus.Kafka;
using Microsoft.Extensions.Logging;
using OTT.Lib.Kafka;
using SchemaRegistryEvents.Catalog;
using Phoenix.Generated.Api.Events.Crud.HouseholdSegment;

namespace ApiLogic.Modules.Services
{
    public class HouseholdSegmentCrudMessageService : IHouseholdSegmentCrudMessageService
    {
        private readonly IKafkaProducer<string, HouseholdSegment> _householdSegmentProducer;
        private readonly ILogger _logger;

        public HouseholdSegmentCrudMessageService(IKafkaProducerFactory producerFactory, IKafkaContextProvider contextProvider, ILogger logger)
        {
            _householdSegmentProducer = producerFactory.Get<string, HouseholdSegment>(contextProvider, Partitioner.Murmur2Random);
            _logger = logger;
        }
        
        public Task PublishCreateEventAsync(long groupId, ApiObjects.Segmentation.HouseholdSegment householdSegment) => PublishKafkaEvent(groupId, CrudOperationType.CREATE_OPERATION, householdSegment);

        public Task PublishDeleteEventAsync(long groupId, ApiObjects.Segmentation.HouseholdSegment householdSegment) =>
            PublishKafkaEvent(groupId, CrudOperationType.DELETE_OPERATION, householdSegment);

        private Task PublishKafkaEvent(long groupId, long operation, ApiObjects.Segmentation.HouseholdSegment householdSegment)
        {
            var householdSegmentEvent = new HouseholdSegment
            {
                PartnerId = groupId,
                Operation = operation,
                SegmentId = householdSegment.SegmentId,
                HouseholdId = householdSegment.HouseholdId.ToString()
            };

            return _householdSegmentProducer.ProduceAsync(HouseholdSegment.GetTopic(), householdSegmentEvent.GetPartitioningKey(), householdSegmentEvent);
        }
    }
}