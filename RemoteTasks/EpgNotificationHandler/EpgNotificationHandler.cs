using EventBus.Abstraction;
using KLogMonitor;
using System;
using System.Reflection;
using System.Threading.Tasks;
using ApiLogic.Notification;
using ApiObjects.EventBus;
using Core.Notification;
using TVinciShared;
using Newtonsoft.Json;
using EpgNotificationHandler.DTO;
using ApiObjects.Notification;
using System.Linq;

namespace EpgNotificationHandler
{
    public class EpgNotificationHandler : IServiceEventHandler<EpgNotificationEvent>
    {
        private static readonly KLogger _Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private const int Max_Send_Duration = 30000; //30 seconds
        private readonly IIotManager _manager;
        private readonly INotificationCache _notificationCache;


        public EpgNotificationHandler(IIotManager manager, INotificationCache notificationCache)
        {
            _Logger.Debug("Starting 'EpgNotificationHandler'");
            _manager = manager;
            _notificationCache = notificationCache;
        }

        public async Task Handle(EpgNotificationEvent serviceEvent)
        {
            _Logger.Debug($"Received message RequestId:{serviceEvent.RequestId}, LiveAssetId:{serviceEvent.LiveAssetId}, " +
                $"EpgChannelId:{serviceEvent.EpgChannelId}, range:({serviceEvent.UpdatedRange.From},{serviceEvent.UpdatedRange.To})");
            using (AppMetrics.EventDuration()) // TODO add braces and move code inside try
            try
            {
                var ns = _notificationCache.GetPartnerNotificationSettings(serviceEvent.GroupId);

                if (ns?.settings == null)
                {
                    _Logger.Debug($"Group {serviceEvent.GroupId} No settings available, canceling");
                    AppMetrics.EventFiltered();
                    return;
                }

                if (!CheckIfGroupSupportEpgUpdate(ns) || !CheckIfEventDatesValid(ns, serviceEvent) || !LiveAssetIdsExists(ns, serviceEvent))
                {
                    _Logger.Debug($"Group {serviceEvent.GroupId} Not Supporting Epg Update or event is invalid, canceling");
                    AppMetrics.EventFiltered();
                    return;
                }

                var content = new EpgUpdateMessage()
                {
                    Header = new UpdateHeader
                    {
                        EventDate = DateUtils.GetUtcUnixTimestampNow(),
                        EventType = EventType.epg_update
                    },
                    StartDate = DateUtils.DateTimeToUtcUnixTimestampSeconds(serviceEvent.UpdatedRange.From),
                    EndDate = DateUtils.DateTimeToUtcUnixTimestampSeconds(serviceEvent.UpdatedRange.To),
                    EpgChannelId = serviceEvent.EpgChannelId,
                    LiveAssetId = serviceEvent.LiveAssetId
                };
                var message = JsonConvert.SerializeObject(content);

                await SendTopicUpdate(serviceEvent.GroupId, message, serviceEvent.RequestId);

                AppMetrics.EventSucceed();
                _Logger.Debug($"Handled message RequestId:{serviceEvent.RequestId}");
            }
            catch (Exception ex)
            {
                _Logger.Error($"An Exception occurred in EpgNotificationHandler requestId:[{serviceEvent.RequestId}]", ex);
                AppMetrics.EventFailed();
            }
        }

        private bool CheckIfGroupSupportEpgUpdate(NotificationPartnerSettingsResponse ns)
        {
            var iotEnabled = ns.settings.IsIotEnabled.HasValue && ns.settings.IsIotEnabled.Value;
            var epgUpdateEnabled = ns.settings.EpgNotification != null && ns.settings.EpgNotification.Enabled;
            return iotEnabled && epgUpdateEnabled;
        }

        private bool CheckIfEventDatesValid(NotificationPartnerSettingsResponse ns, EpgNotificationEvent serviceEvent)
        {
            var tr = ns.settings.EpgNotification.TimeRange;
            var now = SystemDateTime.UtcNow;
            var startDate = now.AddHours(-tr);
            var endDate = now.AddHours(tr);
            return serviceEvent.UpdatedRange.From >= startDate && serviceEvent.UpdatedRange.From <= endDate;
        }

        private bool LiveAssetIdsExists(NotificationPartnerSettingsResponse ns, EpgNotificationEvent serviceEvent)
        {
            return ns.settings.EpgNotification.LiveAssetIds.Count == 0 || ns.settings.EpgNotification.LiveAssetIds.Contains(serviceEvent.LiveAssetId);
        }

        private async Task SendTopicUpdate(int groupId, string message, string requestId)
        {
            var partitionsCount = _manager.GetTopicPartitionsCount();
            var delay = partitionsCount == 1 ? 0 : Max_Send_Duration / (partitionsCount - 1);
            var topicFormat = _manager.GetTopicFormat(groupId, EventType.epg_update);
            for (int partitionNumber = 0; partitionNumber < partitionsCount; partitionNumber++)
            {
                var topic = string.Format(topicFormat, partitionNumber);
                using (AppMetrics.IotRequestDuration())
                {
                    if (_manager.PublishIotMessage(groupId, message, topic))
                    {
                        AppMetrics.IotRequestSucceed();
                        _Logger.Debug($"Iot: Message sent to topic: {topic}, requestId:{requestId}");
                    }
                    else
                    {
                        _Logger.Error($"Iot: Failed to send message to topic: {topic}, requestId:{requestId}");
                        AppMetrics.IotRequestFailed();
                    }
                }
                await Task.Delay(delay);
            }
        }
    }
}