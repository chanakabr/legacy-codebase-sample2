using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiLogic.Api.Managers;
using ApiObjects.Cloudfront;
using ApiObjects.EventBus;
using ApiObjects.Notification;
using Core.Notification;
using EpgNotificationHandler.DTO;
using EventBus.Abstraction;
using EventBus.RabbitMQ;
using IotGrpcClientWrapper;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NotificationHandlers.Common;
using phoenix;
using TVinciShared;

namespace EpgNotificationHandler
{
    // Regenerate service was the main branch when added this nuget(0.1.33-regenerate-service), can be changed in the future when needed
    public class EpgNotificationHandler : IServiceEventHandler<EpgNotificationEvent>
    {
        public const int COUNT_OF_DAYS_FOR_FULL_INVALIDATION = 5;
        private readonly INotificationCache _notificationCache;
        private readonly IRegionManager _regionManager;
        private readonly IIotClient _iotClient;
        private readonly ICloudfrontInvalidator _cloudfrontInvalidator;
        private readonly ILogger<EpgNotificationHandler> _logger;
        
        public EpgNotificationHandler(
            INotificationCache notificationCache,
            IRegionManager regionManager,
            IIotClient iotClient,
            ICloudfrontInvalidator cloudfrontInvalidator,
            ILogger<EpgNotificationHandler> logger)
        {
            _notificationCache = notificationCache ?? throw new ArgumentNullException(nameof(notificationCache));
            _regionManager = regionManager ?? throw new ArgumentNullException(nameof(regionManager));
            _iotClient = iotClient ?? throw new ArgumentNullException(nameof(iotClient));
            _cloudfrontInvalidator = cloudfrontInvalidator ?? throw new ArgumentNullException(nameof(cloudfrontInvalidator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Handle(EpgNotificationEvent serviceEvent)
        {
            _logger.LogInformation(
                "Received event LiveAssetId:[{LiveAssetId}], EpgChannelId:[{EpgChannelId}], range:[{UpdatedRangeFrom},{UpdatedRangeTo}]",
                serviceEvent.LiveAssetId, serviceEvent.EpgChannelId, serviceEvent.UpdatedRange.From,
                serviceEvent.UpdatedRange.To);

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

                    var message = Serialize(serviceEvent);
                    await _iotClient.SendNotificationAsync(serviceEvent.GroupId, message,
                        EventNotificationType.EpgUpdate, GetRegions(serviceEvent.GroupId, serviceEvent.LiveAssetId));

                    AppMetrics.EventSucceed();

                    _logger.LogInformation("Event is handled successfully");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to handle event");
                    AppMetrics.EventFailed();
                    throw new RetryableErrorException(ex); // throw exception in order to process event once again
                }
            }
        }

        private List<int> GetRegions(int groupId, long linearAssetId)
        {
            var linearMediaRegions = _regionManager.GetLinearMediaRegions(groupId);
            if (!linearMediaRegions.TryGetValue(linearAssetId, out var regions))
            {
                _logger.LogInformation("Linear asset {LinearAssetId} doesn't belong to any region. Skip notification", linearAssetId);

                return Enumerable.Empty<int>().ToList();
            }

            return regions;
        }

        private bool CanHandleNotification(EpgNotificationEvent serviceEvent)
        {
            if (serviceEvent.DisableEpgNotification)
            {
                _logger.LogDebug("EpgNotification is disabled");

                return false;
            }
            
            var settingsResponse = _notificationCache.GetPartnerNotificationSettings(serviceEvent.GroupId);

            if (settingsResponse?.settings == null)
            {
                _logger.LogDebug("Group {GroupId} No settings available, canceling", serviceEvent.GroupId);

                return false;
            }

            if (CheckIfGroupSupportEpgUpdate(settingsResponse) &&
                CheckIfEventDatesValid(settingsResponse, serviceEvent) &&
                LiveAssetIdsExists(settingsResponse, serviceEvent))
            {
                return true;
            }

            _logger.LogDebug("Group {GroupId} Not Supporting Epg Update or event is invalid, canceling", serviceEvent.GroupId);

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
            var utcNow = DateUtils.GetUtcUnixTimestampNow();
            var content = new EpgUpdateMessage
            {
                Header = new UpdateHeader
                {
                    Properties = new UpdateEventProperties
                    {
                        EventDate = new UpdateEventDate
                        {
                            EventDate = utcNow,
                            Type = utcNow.GetType().Name
                        },
                        EventType = new UpdateEventType
                        {
                            Enum = new List<string> { EventType.epg_update.ToString() },
                            Type = typeof(string).Name
                        }
                    }
                },
                StartDate = DateUtils.DateTimeToUtcUnixTimestampSeconds(serviceEvent.UpdatedRange.From),
                EndDate = DateUtils.DateTimeToUtcUnixTimestampSeconds(serviceEvent.UpdatedRange.To),
                EpgChannelId = serviceEvent.EpgChannelId,
                LiveAssetId = serviceEvent.LiveAssetId
            };
            var message = JsonConvert.SerializeObject(content);
            return message;
        }

        private async Task InvalidateCloudfront(EpgNotificationEvent serviceEvent)
        {
            var pathsToInvalidate = GetPathsToInvalidate(serviceEvent).ToArray();
            var (success, failedInvalidations) =
                await _cloudfrontInvalidator.InvalidateAndWaitAsync(serviceEvent.GroupId, pathsToInvalidate,
                    WaitConfig.Default);
            if (!success)
            {

                throw new Exception(
                    $"Cloudfront invalidation failed. paths:[{string.Join(',', failedInvalidations ?? Enumerable.Empty<string>())}]");
            }
        }

        private static IEnumerable<string> GetPathsToInvalidate(EpgNotificationEvent serviceEvent)
        {
            var range = serviceEvent.UpdatedRange; 
            var updateDatesCount = range.To.Date.Subtract(range.From.Date).Days + 1;
            if (updateDatesCount >= COUNT_OF_DAYS_FOR_FULL_INVALIDATION)
            {
                yield return InvalidationPath.EpgPartner(serviceEvent.GroupId);
            }
            else
            {
                for (var i = 0; i < updateDatesCount; i++)
                {
                    var dayTimestamp = range.From.Date.AddDays(i).ToUtcUnixTimestampSeconds();
                    yield return InvalidationPath.EpgDay(serviceEvent.GroupId, dayTimestamp);
                }
            }
        }
    }
}
