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
using EpgCacheGrpcClientWrapper;
using Polly;

namespace EpgNotificationHandler
{
    public class EpgNotificationHandler : IServiceEventHandler<EpgNotificationEvent>
    {
        private static readonly KLogger Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private const int MAX_SEND_DURATION = 30000; //30 seconds
        private const int EpgCacheInMemoryCacheTTL = 10 * 1000; // 10 sec https://github.com/kaltura/ott-service-epgcache/blob/master/logic/clients/epg_cache.go#L81
        private readonly IIotManager _manager;
        private readonly INotificationCache _notificationCache;
        private readonly IEpgCacheClient _epgCacheClient;
        private readonly IAsyncPolicy _retryPolicy;

        public EpgNotificationHandler(IIotManager manager, INotificationCache notificationCache,
            IEpgCacheClient epgCacheClient, IAsyncPolicy retryPolicy)
        {
            Logger.Debug("Starting 'EpgNotificationHandler'");
            _manager = manager;
            _notificationCache = notificationCache;
            _epgCacheClient = epgCacheClient;
            _retryPolicy = retryPolicy;
        }

        public async Task Handle(EpgNotificationEvent serviceEvent)
        {
            Logger.Debug($"Received event LiveAssetId:[{serviceEvent.LiveAssetId}], EpgChannelId:[{serviceEvent.EpgChannelId}]," +
                         $" range:[{serviceEvent.UpdatedRange.From},{serviceEvent.UpdatedRange.To}]");
            using (AppMetrics.EventDuration())
            {
                try
                {
                    // WARNING!
                    // This call should be in a separate service EpgCacheInvalidator.
                    // If you need to make invalidation logic more complex (e.g. call several services, have some settings),
                    // please, make refactoring and move this logic to a separate service (look at better-epg-notification-flow.puml)
                    await InvalidateEpgCache(serviceEvent);
                    await Task.Delay(EpgCacheInMemoryCacheTTL); // need to be 100% sure that user will receive latest data from EpgCache
                    // END WARNING
                    
                    if (SkipNotification(serviceEvent))
                    {
                        AppMetrics.EventFiltered();
                    }
                    else
                    {
                        await SendNotification(serviceEvent.GroupId, Serialize(serviceEvent));
                        AppMetrics.EventSucceed();
                    }
                    
                    Logger.Debug($"Event is handled successfully");
                }
                catch (Exception ex)
                {
                    Logger.Error($"Failed to handle event", ex);
                    AppMetrics.EventFailed();
                }
            }
        }

        private async Task InvalidateEpgCache(EpgNotificationEvent serviceEvent)
        {
            Logger.Debug($"Invalidating EpgCache");
            try
            {
                using (AppMetrics.EpgCacheInvalidate.RequestDuration())
                {
                    await _retryPolicy.ExecuteAsync(
                        async () => // TODO https://anthonygiretti.com/2020/03/31/grpc-asp-net-core-3-1-resiliency-with-polly/
                            await _epgCacheClient.InvalidateEpgAsync(
                                serviceEvent.GroupId,
                                serviceEvent.LiveAssetId,
                                DateUtils.DateTimeToUtcUnixTimestampSeconds(serviceEvent.UpdatedRange.From),
                                DateUtils.DateTimeToUtcUnixTimestampSeconds(serviceEvent.UpdatedRange.To))
                    );
                }
                AppMetrics.EpgCacheInvalidate.RequestSucceed();
            }
            catch (Exception exception)
            {
                AppMetrics.EpgCacheInvalidate.RequestFailed();
                Logger.Error($"Failed to invalidate EpgCache", exception);
                throw;
            }
        }

        private bool SkipNotification(EpgNotificationEvent serviceEvent)
        {
            if (serviceEvent.DisableEpgNotification)
            {
                Logger.Debug($"EpgNotification is disabled");
                return true;
            }

            var ns = _notificationCache.GetPartnerNotificationSettings(serviceEvent.GroupId);
            if (ns?.settings == null)
            {
                Logger.Debug($"Group {serviceEvent.GroupId} No settings available, canceling");
                return true;
            }

            if (!CheckIfGroupSupportEpgUpdate(ns) || !CheckIfEventDatesValid(ns, serviceEvent) || !LiveAssetIdsExists(ns, serviceEvent))
            {
                Logger.Debug($"Group {serviceEvent.GroupId} Not Supporting Epg Update or event is invalid, canceling");
                return true;
            }
            return false;
        }

        private static bool CheckIfGroupSupportEpgUpdate(NotificationPartnerSettingsResponse ns)
        {
            var iotEnabled = ns.settings.IsIotEnabled.HasValue && ns.settings.IsIotEnabled.Value;
            var epgUpdateEnabled = ns.settings.EpgNotification != null && ns.settings.EpgNotification.Enabled;
            return iotEnabled && epgUpdateEnabled;
        }

        private static bool CheckIfEventDatesValid(NotificationPartnerSettingsResponse ns, EpgNotificationEvent serviceEvent)
        {
            var backwardTimeRange = ns.settings.EpgNotification.BackwardTimeRange;
            var forwardTimeRange = ns.settings.EpgNotification.ForwardTimeRange;
            if (backwardTimeRange == 0 && forwardTimeRange == 0)
            {
                return false;
            }
            
            var now = SystemDateTime.UtcNow;
            var startDate = now.AddHours(-backwardTimeRange);
            var endDate = now.AddHours(forwardTimeRange);
            return CheckIfDateWithinRange(serviceEvent.UpdatedRange.From, startDate, endDate) || CheckIfDateWithinRange(serviceEvent.UpdatedRange.To, startDate, endDate);
        }

        private static bool CheckIfDateWithinRange(DateTime date, DateTime startRange, DateTime endRange)
        {
            return date >= startRange && date <= endRange;
        }

        private static bool LiveAssetIdsExists(NotificationPartnerSettingsResponse ns, EpgNotificationEvent serviceEvent)
        {
            return ns.settings.EpgNotification.LiveAssetIds.Count == 0 || ns.settings.EpgNotification.LiveAssetIds.Contains(serviceEvent.LiveAssetId);
        }
        
        private static string Serialize(EpgNotificationEvent serviceEvent)
        {
            var content = new EpgUpdateMessage
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
            return message;
        }

        private async Task SendNotification(int groupId, string message)
        {
            var partitionsCount = _manager.GetTopicPartitionsCount();
            var delay = partitionsCount == 1 ? 0 : MAX_SEND_DURATION / (partitionsCount - 1);
            var topicFormat = _manager.GetTopicFormat(groupId, EventType.epg_update);
            for (int partitionNumber = 0; partitionNumber < partitionsCount; partitionNumber++)
            {
                var topic = string.Format(topicFormat, partitionNumber);
                using (AppMetrics.Iot.RequestDuration())
                {
                    try
                    {
                        if (_manager.PublishIotMessage(groupId, message, topic))
                        {
                            AppMetrics.Iot.RequestSucceed();
                            Logger.Debug($"Iot: Message sent to topic: {topic}");
                        }
                        else
                        {
                            IotSendFailed(topic);
                        }
                    }
                    catch (Exception exception)
                    {
                        IotSendFailed(topic, exception);
                        throw;
                    }
                }
                await Task.Delay(delay);
            }
        }

        private static void IotSendFailed(string topic, Exception exception = null)
        {
            Logger.Error($"Iot: Failed to send message to topic: {topic}", exception);
            AppMetrics.Iot.RequestFailed();
        }
    }
}