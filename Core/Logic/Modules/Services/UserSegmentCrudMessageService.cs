using System.Threading.Tasks;
using Confluent.Kafka;
using EventBus.Kafka;
using OTT.Lib.Kafka;
using SchemaRegistryEvents.Catalog;
using Phoenix.Generated.Api.Events.Crud.UserSegment;
using Microsoft.Extensions.Logging;

namespace ApiLogic.Modules.Services
{
    public class UserSegmentCrudMessageService : IUserSegmentCrudMessageService
    {
        private readonly IKafkaProducer<string, UserSegment> _userSegmentProducer;
        private readonly ILogger _logger;

        public UserSegmentCrudMessageService(IKafkaProducerFactory producerFactory, IKafkaContextProvider contextProvider, ILogger logger)
        {
            _userSegmentProducer = producerFactory.Get<string, UserSegment>(contextProvider, Partitioner.Murmur2Random);
            _logger = logger;
        }

        public Task PublishCreateEventAsync(long groupId, ApiObjects.Segmentation.UserSegment userSegment) => PublishKafkaEvent(groupId, CrudOperationType.CREATE_OPERATION, userSegment);
        
        public Task PublishDeleteEventAsync(long groupId, ApiObjects.Segmentation.UserSegment userSegment) => PublishKafkaEvent(groupId, CrudOperationType.DELETE_OPERATION, userSegment);

        private Task PublishKafkaEvent(long groupId, long operation, ApiObjects.Segmentation.UserSegment userSegment)
        {
            var userSegmentEvent = new UserSegment
            {
                PartnerId = groupId,
                Operation = operation,
                SegmentId = userSegment.SegmentId,
                UserId = userSegment.UserId
            };

            return _userSegmentProducer.ProduceAsync(UserSegment.GetTopic(), userSegmentEvent.GetPartitioningKey(), userSegmentEvent);
        }
    }
}