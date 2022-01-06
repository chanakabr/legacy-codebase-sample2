using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiLogic.Api.Managers;
using ApiLogic.Notification;
using ApiObjects.EventBus;
using ApiObjects.Notification;
using Core.Catalog.CatalogManagement;
using Core.Notification;
using EpgNotificationHandler.Configuration;
using EpgNotificationHandler.DTO;
using EventBus.Abstraction;
using Phx.Lib.Log;
using Newtonsoft.Json;
using NotificationHandlers.Common;
using TVinciShared;

namespace EpgNotificationHandler
{
    // Regenerate service was the main branch when added this nuget(0.1.33-regenerate-service), can be changed in the future when needed
    public class EpgNotificationHandler : IServiceEventHandler<EpgNotificationEvent>
    {
        private static readonly KLogger Logger = new KLogger(nameof(EpgNotificationHandler));
        private readonly IIotManager _iotManager;
        private readonly INotificationCache _notificationCache;
        private readonly ICatalogManager _catalogManager;
        private readonly IRegionManager _regionManager;
        private readonly IEpgNotificationConfiguration _configuration;
        private readonly IIotNotificationService _iotNotificationService;

        public EpgNotificationHandler(
            IEpgNotificationConfiguration configuration,
            IIotManager iotManager,
            INotificationCache notificationCache,
            ICatalogManager catalogManager,
            IRegionManager regionManager,
            IIotNotificationService iotNotificationService)
        {
            Logger.Debug("Starting 'EpgNotificationHandler'");
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _iotManager = iotManager ?? throw new ArgumentNullException(nameof(iotManager));
            _notificationCache = notificationCache ?? throw new ArgumentNullException(nameof(notificationCache));
            _catalogManager = catalogManager ?? throw new ArgumentNullException(nameof(catalogManager));
            _regionManager = regionManager ?? throw new ArgumentNullException(nameof(regionManager));
            _iotNotificationService = iotNotificationService ?? throw new ArgumentNullException(nameof(iotNotificationService));
        }

        public async Task Handle(EpgNotificationEvent serviceEvent)
        {
            Logger.Debug($"Received event LiveAssetId:[{serviceEvent.LiveAssetId}], EpgChannelId:[{serviceEvent.EpgChannelId}]," +
                         $" range:[{serviceEvent.UpdatedRange.From},{serviceEvent.UpdatedRange.To}]");

            using (AppMetrics.EventDuration())
            {
                try
                {
                    var settingsResponse = _notificationCache.GetPartnerNotificationSettings(serviceEvent.GroupId);
                    if (!CanHandleNotification(serviceEvent, settingsResponse))
                    {
                        AppMetrics.EventFiltered();

                        return;
                    }

                    // Don't need to invalidate CloudFront explicitly, for now CF will be invalidated by TTL.
                    await Task.Delay(_configuration.CloudFrontInvalidationTtlInMs);

                    var topics = BuildIotTopics(serviceEvent.GroupId, serviceEvent.LiveAssetId);
                    var message = Serialize(serviceEvent);
                    var notificationTasks = topics.Select(x => _iotNotificationService.SendNotificationAsync(serviceEvent.GroupId, message, x));
                    await Task.WhenAll(notificationTasks);

                    AppMetrics.EventSucceed();

                    Logger.Debug($"Event is handled successfully");
                }
                catch (Exception ex)
                {
                    Logger.Error($"Failed to handle event", ex);
                    AppMetrics.EventFailed();
                }
            }
        }

        private List<string> BuildIotTopics(int groupId, long linearAssetId)
        {
            if (!_catalogManager.IsRegionalizationEnabled(groupId))
            {
                return new List<string> { _iotManager.GetTopicFormat(groupId, EventType.epg_update) };
            }

            var linearMediaRegions = _regionManager.GetLinearMediaRegions(groupId);
            if (!linearMediaRegions.TryGetValue(linearAssetId, out var regions))
            {
                Logger.Info($"Linear asset {linearAssetId} doesn't belong to any region. Skip notification.");

                return Enumerable.Empty<string>().ToList();
            }

            return regions
                .Select(x => _iotManager.GetRegionTopicFormat(groupId, EventType.epg_update, x))
                .ToList();
        }

        // private async Task InvalidateEpgCache(EpgNotificationEvent serviceEvent)
        // {
        //     Logger.Debug($"Invalidating EpgCache");
        //     try
        //     {
        //         using (AppMetrics.EpgCacheInvalidate.RequestDuration())
        //         {
        //             await _retryPolicy.ExecuteAsync(
        //                 async () => // TODO https://anthonygiretti.com/2020/03/31/grpc-asp-net-core-3-1-resiliency-with-polly/
        //                     await _epgCacheClient.InvalidateEpgAsync(
        //                         serviceEvent.GroupId,
        //                         serviceEvent.LiveAssetId,
        //                         DateUtils.DateTimeToUtcUnixTimestampSeconds(serviceEvent.UpdatedRange.From),
        //                         DateUtils.DateTimeToUtcUnixTimestampSeconds(serviceEvent.UpdatedRange.To))
        //             );
        //         }
        //         AppMetrics.EpgCacheInvalidate.RequestSucceed();
        //     }
        //     catch (Exception exception)
        //     {
        //         AppMetrics.EpgCacheInvalidate.RequestFailed();
        //         Logger.Error($"Failed to invalidate EpgCache", exception);
        //         throw;
        //     }
        // }

        private static bool CanHandleNotification(EpgNotificationEvent serviceEvent, NotificationPartnerSettingsResponse settingsResponse)
        {
            if (serviceEvent.DisableEpgNotification)
            {
                Logger.Debug($"EpgNotification is disabled");

                return false;
            }

            if (settingsResponse?.settings == null)
            {
                Logger.Debug($"Group {serviceEvent.GroupId} No settings available, canceling");

                return false;
            }

            if (CheckIfGroupSupportEpgUpdate(settingsResponse) &&
                CheckIfEventDatesValid(settingsResponse, serviceEvent) &&
                LiveAssetIdsExists(settingsResponse, serviceEvent))
            {
                return true;
            }

            Logger.Debug($"Group {serviceEvent.GroupId} Not Supporting Epg Update or event is invalid, canceling");

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
    }
}