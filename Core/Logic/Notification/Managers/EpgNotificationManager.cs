using ApiObjects.EventBus;
using Core.Notification;
using EventBus.Abstraction;
using EventBus.RabbitMQ;
using KLogMonitor;
using System;
using System.Reflection;
using System.Threading;

namespace ApiLogic.Notification.Managers
{
    public class EpgNotificationManager
    {
        private static readonly KLogger _logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
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
            _logger.Debug($"[Epg notification] Try send. requestId:{requestId}");
            if (!ShouldSendEvent(groupId))
            {
                _logger.Debug($"[Epg notification] Skip sending. requestId:{requestId}");
                return;
            }

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
            _logger.Debug($"[Epg notification] Was sent successfully. requestId:{requestId}");
        }

        private bool ShouldSendEvent(int groupId)
        {
            var response = _notificationCache.GetPartnerNotificationSettings(groupId);
            return response?.Status != null && response.Status.IsOkStatusCode()
                && response?.settings?.EpgNotification.Enabled == true;
        }
    }
}
