using System.Threading.Tasks;
using ApiObjects.Segmentation;
using Confluent.Kafka;
using EventBus.Kafka;
using Microsoft.Extensions.Logging;
using OTT.Lib.Kafka;
using Phoenix.Generated.Api.Events.Logical.announcementMessage;

namespace ApiLogic.Notification.Managers
{
    public class IotManager
    {
        private readonly IKafkaProducer<string, AnnouncementMessage> _announcementProducer;

        public IotManager(IKafkaContextProvider contextProvider)
        {
            _announcementProducer = KafkaProducerFactoryInstance.Get().Get<string, AnnouncementMessage>(contextProvider, Partitioner.Consistent);
        }

        public Task PublishIotAnnouncementMessageKafkaEvent(long groupId, string message)
        {
            var announcementMessage = new AnnouncementMessage
            {
                PartnerId = groupId,
                Message = message
            };

            return _announcementProducer.ProduceAsync(AnnouncementMessage.GetTopic(), announcementMessage.GetPartitioningKey(), announcementMessage);
        }
    }
}