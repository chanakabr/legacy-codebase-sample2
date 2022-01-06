using ApiObjects.EventBus;
using Core.Notification;
using EventBus.Abstraction;
using EventBus.RabbitMQ;
using Phx.Lib.Log;
using System;
using System.Reflection;
using System.Threading;

namespace ApiLogic.Notification.Managers
{
    public class EpgNotificationManager
    {
        private static readonly KLogger _logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private readonly IEventBusPublisher _publisher;
        private static readonly Lazy<EpgNotificationManager> _instance = new Lazy<EpgNotificationManager>(() => 
            new EpgNotificationManager(EventBusPublisherRabbitMQ.GetInstanceUsingTCMConfiguration()), LazyThreadSafetyMode.PublicationOnly);
        public static EpgNotificationManager Instance() => _instance.Value;

        public EpgNotificationManager(IEventBusPublisher publisher)
        {
            _publisher = publisher;
        }

        public void ChannelWasUpdated(string requestId, int groupId, long userId, long linearAssetId, long epgChannelId,
            DateTime startDate, DateTime endDate, bool disableEpgNotification)
        {
            _logger.Debug($"[Epg notification] Try send. requestId:{requestId}");

            var @event = new EpgNotificationEvent
            {
                RequestId = requestId,
                GroupId = groupId,
                UserId = userId,
                LiveAssetId = linearAssetId,
                EpgChannelId = epgChannelId,
                UpdatedRange = new Range<DateTime>(startDate, endDate),
                DisableEpgNotification = disableEpgNotification
            };
            _publisher.Publish(@event);
            _logger.Debug($"[Epg notification] Was sent successfully. requestId:{requestId}");
        }
    }
}
