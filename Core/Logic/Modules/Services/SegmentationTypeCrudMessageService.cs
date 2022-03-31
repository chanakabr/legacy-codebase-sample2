using System.Linq;
using System.Threading.Tasks;
using Confluent.Kafka;
using OTT.Lib.Kafka;
using SchemaRegistryEvents.Catalog;
using Phoenix.Generated.Api.Events.Crud.SegmentationType;
using Microsoft.Extensions.Logging;

namespace ApiLogic.Modules.Services
{
    public class SegmentationTypeCrudMessageService : ISegmentationTypeCrudMessageService
    {
        private readonly IKafkaProducer<string, SegmentationType> _segProducer;
        private readonly ILogger _logger;

        public SegmentationTypeCrudMessageService(IKafkaProducerFactory producerFactory, IKafkaContextProvider contextProvider, ILogger logger)
        {
            _segProducer = producerFactory.Get<string, SegmentationType>(contextProvider, Partitioner.Murmur2Random);
            _logger = logger;
        }
        
        public Task PublishCreateEventAsync(int groupId, ApiObjects.Segmentation.SegmentationType segType) => PublishKafkaCreateOrUpdateEvent(groupId, CrudOperationType.CREATE_OPERATION, segType);

        public Task PublishUpdateEventAsync(int groupId, ApiObjects.Segmentation.SegmentationType segType) => PublishKafkaCreateOrUpdateEvent(groupId, CrudOperationType.UPDATE_OPERATION, segType);

        private Task PublishKafkaCreateOrUpdateEvent(long groupId, long operation, ApiObjects.Segmentation.SegmentationType segType)
        {
            var values = new SegmentsValue[] { };
            
            if (segType.Value is ApiObjects.Segmentation.SegmentDummyValue value)
            {
                values = new[] { new SegmentsValue{Id = value.Id} };
            } else if (segType.Value is ApiObjects.Segmentation.SegmentValues segmentValues)
            {
                values = segmentValues.Values.Select(curr => new SegmentsValue {Id = curr.Id, Name = curr.Name, Value = curr.Value}).ToArray();
            }
            
            var segmentationTypeEvent = new SegmentationType
            {
                Id = segType.Id,
                PartnerId = groupId,
                Operation = operation,
                CreateDate = segType.CreateDate,
                Name = segType.Name, 
                SegmentsValues = values,
                Description = segType.Description
            };

            return _segProducer.ProduceAsync(SegmentationType.GetTopic(), segmentationTypeEvent.GetPartitioningKey(), segmentationTypeEvent);
        }
        
        public Task PublishDeleteEventAsync(long groupId, long segTypeId)
        {
            var segmentationTypeEvent = new SegmentationType
            {
                Id = segTypeId,
                PartnerId = groupId,
                Operation = CrudOperationType.DELETE_OPERATION
            };

            return _segProducer.ProduceAsync(SegmentationType.GetTopic(), segmentationTypeEvent.GetPartitioningKey(), segmentationTypeEvent);
        }
    }
}