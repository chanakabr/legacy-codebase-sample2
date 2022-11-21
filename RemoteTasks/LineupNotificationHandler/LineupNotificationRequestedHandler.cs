using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiLogic.Api.Managers;
using ApiObjects.Cloudfront;
using ApiObjects.EventBus;
using ApiObjects.Notification;
using Core.Catalog.CatalogManagement;
using Core.Notification;
using EventBus.Abstraction;
using EventBus.RabbitMQ;
using IotGrpcClientWrapper;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NotificationHandlers.Common;
using NotificationHandlers.Common.DTO;
using phoenix;
using TVinciShared;

namespace LineupNotificationHandler
{
    public class LineupNotificationRequestedHandler : IServiceEventHandler<LineupNotificationRequestedEvent>
    {
        private readonly INotificationCache _notificationCache;
        private readonly ICatalogManager _catalogManager;
        private readonly IRegionManager _regionManager;
        private readonly IIotClient _iotClient;
        private readonly ICloudfrontInvalidator _cloudfrontInvalidator;
        private readonly ILogger<LineupNotificationRequestedHandler> _logger;

        public LineupNotificationRequestedHandler(
            INotificationCache notificationCache,
            ICatalogManager catalogManager,
            IRegionManager regionManager,
            IIotClient iotClient,
            ICloudfrontInvalidator cloudfrontInvalidator,
            ILogger<LineupNotificationRequestedHandler> logger)
        {
            _notificationCache = notificationCache ?? throw new ArgumentNullException(nameof(notificationCache));
            _catalogManager = catalogManager ?? throw new ArgumentNullException(nameof(catalogManager));
            _regionManager = regionManager ?? throw new ArgumentNullException(nameof(regionManager));
            _iotClient = iotClient ?? throw new ArgumentNullException(nameof(iotClient));
            _cloudfrontInvalidator = cloudfrontInvalidator ?? throw new ArgumentNullException(nameof(cloudfrontInvalidator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Handle(LineupNotificationRequestedEvent serviceEvent)
        {
            _logger.LogInformation("Received event RegionIds:[{RegionIds}]", serviceEvent.RegionIds);

            using (AppMetrics.EventDuration())
            {
                try
                {
                    if (!CanHandleNotification(serviceEvent))
                    {
                        AppMetrics.EventFiltered();

                        return;
                    }

                    await InvalidateCloudfront(serviceEvent);

                    var regionsToNotify = new HashSet<long>();
                    foreach (var regionId in serviceEvent.RegionIds)
                    {
                        regionsToNotify.Add(regionId);
                        var childRegionIds = _regionManager.GetChildRegionIds(serviceEvent.GroupId, regionId);

                        regionsToNotify.UnionWith(childRegionIds);
                    }

                    var message = GetSerializedMessage();
                    await _iotClient.SendNotificationAsync(serviceEvent.GroupId, message,
                        EventNotificationType.LineupUpdated, regionsToNotify.Select(x => (int)x).ToList());

                    _logger.LogInformation("Event is handled successfully");
                    AppMetrics.EventSucceed();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to handle event");
                    AppMetrics.EventFailed();
                    throw new RetryableErrorException(ex); // throw exception in order to process event once again
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

        private bool CanHandleNotification(LineupNotificationRequestedEvent serviceEvent)
        {
            if (!serviceEvent.RegionIds.Any())
            {
                _logger.LogDebug("Group {GroupId} Region ids are empty, canceling", serviceEvent.GroupId);

                return false;
            }

            if (!_catalogManager.IsRegionalizationEnabled(serviceEvent.GroupId))
            {
                _logger.LogDebug("Group {GroupId} Regionalization disabled, canceling", serviceEvent.GroupId);

                return false;
            }

            var groupRegionIds = _regionManager.GetRegionIds(serviceEvent.GroupId).ToHashSet();
            if (!serviceEvent.RegionIds.All(x => groupRegionIds.Contains((int)x)))
            {
                _logger.LogDebug("Group {GroupId} Regions [{RegionIds}] are not valid, canceling", serviceEvent.GroupId, serviceEvent.RegionIds);

                return false;
            }

            var settingsResponse = _notificationCache.GetPartnerNotificationSettings(serviceEvent.GroupId);
            if (settingsResponse?.settings == null)
            {
                _logger.LogDebug("Group {GroupId} Lineup notifications settings are not available, canceling", serviceEvent.GroupId);

                return false;
            }

            if (!IsLineupNotificationSupported(settingsResponse.settings))
            {
                _logger.LogDebug("Group {GroupId} Lineup notifications are disabled, canceling", serviceEvent.GroupId);

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

        private async Task InvalidateCloudfront(LineupNotificationRequestedEvent serviceEvent)
        {
            var pathsToInvalidate = new[]
            {
                InvalidationPath.EpgPartner(serviceEvent.GroupId),
                InvalidationPath.Lineup(serviceEvent.GroupId)
            };
            var (success, failedInvalidations) =
                await _cloudfrontInvalidator.InvalidateAndWaitAsync(serviceEvent.GroupId, pathsToInvalidate, WaitConfig.Default);
            if (!success)
            {
                throw new Exception($"Cloudfront invalidation failed. paths:[{string.Join(',', failedInvalidations ?? Enumerable.Empty<string>())}]");
            }
        }
    }
}
