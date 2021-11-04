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
using EventBus.Abstraction;
using KLogMonitor;
using LineupNotificationHandler.Configuration;
using Newtonsoft.Json;
using NotificationHandlers.Common;
using NotificationHandlers.Common.DTO;
using TVinciShared;

namespace LineupNotificationHandler
{
    public class LineupNotificationRequestedHandler : IServiceEventHandler<LineupNotificationRequestedEvent>
    {
        private static readonly IKLogger Logger = new KLogger(nameof(LineupNotificationRequestedHandler));

        private readonly ILineupNotificationConfiguration _configuration;
        private readonly INotificationCache _notificationCache;
        private readonly ICatalogManager _catalogManager;
        private readonly IRegionManager _regionManager;
        private readonly IIotManager _iotManager;
        private readonly IIotNotificationService _iotNotificationService;

        public LineupNotificationRequestedHandler(
            ILineupNotificationConfiguration configuration,
            IIotManager iotManager,
            INotificationCache notificationCache,
            ICatalogManager catalogManager,
            IRegionManager regionManager,
            IIotNotificationService iotNotificationService)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _iotManager = iotManager ?? throw new ArgumentNullException(nameof(iotManager));
            _notificationCache = notificationCache ?? throw new ArgumentNullException(nameof(notificationCache));
            _catalogManager = catalogManager ?? throw new ArgumentNullException(nameof(catalogManager));
            _regionManager = regionManager ?? throw new ArgumentNullException(nameof(regionManager));
            _iotNotificationService = iotNotificationService ?? throw new ArgumentNullException(nameof(iotNotificationService));
        }

        public async Task Handle(LineupNotificationRequestedEvent serviceEvent)
        {
            Logger.Debug($"Received event {nameof(serviceEvent.RegionIds)}:[{string.Join(",", serviceEvent.RegionIds)}]");

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

                    var regionsToNotify = new HashSet<long>();
                    foreach (var regionId in serviceEvent.RegionIds)
                    {
                        regionsToNotify.Add(regionId);
                        var childRegionIds = _regionManager.GetChildRegionIds(serviceEvent.GroupId, regionId);

                        regionsToNotify.UnionWith(childRegionIds);
                    }

                    var message = GetSerializedMessage();
                    var notificationTasks = regionsToNotify
                        .Select(x => _iotManager.GetRegionTopicFormat(serviceEvent.GroupId, EventType.lineup_updated, x))
                        .Select(x => _iotNotificationService.SendNotificationAsync(serviceEvent.GroupId, message, x))
                        .ToList();
                    await Task.WhenAll(notificationTasks);

                    Logger.Debug("Event is handled successfully");
                    AppMetrics.EventSucceed();
                }
                catch (Exception ex)
                {
                    Logger.Error("Failed to handle event", ex);
                    AppMetrics.EventFailed();
                }
            }
        }

        private static string GetSerializedMessage()
        {
            var message = new LineupNotificationRequestedMessage
            {
                Header = new UpdateHeader
                {
                    EventDate = DateUtils.GetUtcUnixTimestampNow(),
                    EventType = EventType.lineup_updated
                }
            };

            return JsonConvert.SerializeObject(message);
        }

        private bool CanHandleNotification(LineupNotificationRequestedEvent serviceEvent, NotificationPartnerSettingsResponse settingsResponse)
        {
            if (!serviceEvent.RegionIds.Any())
            {
                Logger.Debug($"Group {serviceEvent.GroupId} Region ids are empty, canceling");

                return false;
            }

            if (!_catalogManager.IsRegionalizationEnabled(serviceEvent.GroupId))
            {
                Logger.Debug($"Group {serviceEvent.GroupId} Regionalization disabled, canceling");

                return false;
            }

            var groupRegionIds = _regionManager.GetRegionIds(serviceEvent.GroupId).ToHashSet();
            if (!serviceEvent.RegionIds.All(x => groupRegionIds.Contains((int)x)))
            {
                Logger.Debug($"Group {serviceEvent.GroupId} Regions [{string.Join(",", serviceEvent.RegionIds)}] are not valid, canceling.");

                return false;
            }

            if (settingsResponse?.settings == null)
            {
                Logger.Debug($"Group {serviceEvent.GroupId} Lineup notifications settings are not available, canceling");

                return false;
            }

            if (!IsLineupNotificationSupported(settingsResponse.settings))
            {
                Logger.Debug($"Group {serviceEvent.GroupId} Lineup notifications are disabled, canceling");

                return false;
            }

            return true;
        }

        private static bool IsLineupNotificationSupported(NotificationPartnerSettings settings)
        {
            var iotEnabled = settings.IsIotEnabled.HasValue && settings.IsIotEnabled.Value;
            var lineupUpdateEnabled = settings.LineupNotification?.Enabled == true;

            return iotEnabled && lineupUpdateEnabled;
        }
    }
}