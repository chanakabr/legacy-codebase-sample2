using ApiObjects.EventBus;
using Core.Notification;
using EventBus.Abstraction;
using EventBus.RabbitMQ;
using System;
using System.Threading;

namespace ApiLogic.Notification.Managers
{
    public class EpgNotificationManager
    {
        private readonly IEventBusPublisher _publisher;
        private readonly NotificationCache _notificationCache;
        private static readonly Lazy<EpgNotificationManager> _instance = new Lazy<EpgNotificationManager>(() => 
            new EpgNotificationManager(EventBusPublisherRabbitMQ.GetInstanceUsingTCMConfiguration(), NotificationCache.Instance()), LazyThreadSafetyMode.PublicationOnly);
        public static EpgNotificationManager Instance() => _instance.Value;

        public EpgNotificationManager(IEventBusPublisher publisher, NotificationCache notificationCache)
        {
            _publisher = publisher;
            _notificationCache = notificationCache;
        }

        public void ChannelWasUpdated(string requestId, int groupId, long userId, long linearAssetId, long epgChannelId, DateTime startDate, DateTime endDate)
        {
            if (!ShouldSendEvent(groupId)) return;

            var @event = new EpgNotificationEvent
            {
                RequestId = requestId,
                GroupId = groupId,
                UserId = userId,
                LiveAssetId = linearAssetId,
                EpgChannelId = epgChannelId,
                UpdatedRange = new Range<DateTime>(startDate, endDate)
            };
            _publisher.Publish(@event);
        }

        private bool ShouldSendEvent(int groupId)
        {
            var response = _notificationCache.GetPartnerNotificationSettings(groupId);
            return response.Status.IsOkStatusCode() && response.settings.EpgNotification.Enabled;
        }
    }
}
