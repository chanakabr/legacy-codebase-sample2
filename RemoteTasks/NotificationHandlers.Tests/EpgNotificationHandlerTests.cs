using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiLogic.Api.Managers;
using ApiLogic.Notification;
using ApiObjects.EventBus;
using ApiObjects.Notification;
using ApiObjects.Response;
using Core.Catalog.CatalogManagement;
using Core.Notification;
using EpgNotificationHandler.Configuration;
using Moq;
using NotificationHandlers.Common;
using NUnit.Framework;

namespace NotificationHandlers.Tests
{
    public class EpgNotificationHandlerTests
    {
        private const int LIVE_ASSET_ID = 10;
        private const int GROUP_ID_DISABLED_REGIONALIZATION = 1;
        private const int GROUP_ID_ENABLED_REGIONALIZATION = 2;
        private static readonly List<int> Regions = new List<int> { 1, 2 };

        private Mock<IIotNotificationService> _iotNotificationServiceMock;
        //private Mock<IEpgCacheClient> _epgCacheClient;
        private NotificationPartnerSettings _notificationSettings;
        private EpgNotificationHandler.EpgNotificationHandler _handler;
        private EpgNotificationEvent _epgEvent;
        
        [SetUp]
        public void Setup()
        {
            var iotManager = GetMockIotManager();
            _notificationSettings = new NotificationPartnerSettings
            {
                IsIotEnabled = true,
                EpgNotification = new EpgNotificationSettings
                {
                    Enabled = true,
                    BackwardTimeRange = 2,
                    ForwardTimeRange = 2,
                    DeviceFamilyIds = new List<int> {1, 2, 3, 4, 5},
                    LiveAssetIds = new List<long> {LIVE_ASSET_ID, 20, 30, 40}
                }
            };
            var notificationSettingsCache = GetMockSettings(_notificationSettings);
            //_epgCacheClient = new Mock<IEpgCacheClient>();
            var regionManagerMock = GetRegionManagerMock();
            var catalogManagerMock = GetCatalogMangerMock();
            var configurationMock = GetConfigurationMock();
            _iotNotificationServiceMock = GetIotNotificationServiceMock();
            _handler = new EpgNotificationHandler.EpgNotificationHandler(
                configurationMock.Object,
                iotManager.Object,
                notificationSettingsCache.Object,
                catalogManagerMock.Object,
                regionManagerMock.Object,
                _iotNotificationServiceMock.Object);
            
            _epgEvent = new EpgNotificationEvent
            {
                GroupId = GROUP_ID_DISABLED_REGIONALIZATION,
                UpdatedRange = new Range<DateTime>(DateTime.UtcNow.AddSeconds(20), DateTime.UtcNow.AddHours(3)),
                LiveAssetId = LIVE_ASSET_ID,
                DisableEpgNotification = false
            };
        }
        
        // [Test]
        // public async Task TestEpgCacheInvalidationFailed()
        // {
        //     _epgCacheClient
        //         .Setup(m => m.InvalidateEpgAsync(It.IsAny<int>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long>()))
        //         .ThrowsAsync(new Exception());
        //     await _handler.Handle(_epgEvent);
        //     VerifyNotificationWasSent(Times.Never);
        // }

        [Test]
        [TestCaseSource(nameof(TestBackwardAndForwardTimeRangeForSendingNotificationsSource))]
        public async Task TestBackwardAndForwardTimeRangeForSendingNotifications(int backwardTimeRange, int forwardTimeRange, bool shouldNotify, EpgNotificationEvent epgEvent)
        {
            _notificationSettings.EpgNotification.BackwardTimeRange = backwardTimeRange;
            _notificationSettings.EpgNotification.ForwardTimeRange = forwardTimeRange;
            await _handler.Handle(epgEvent);
            if (shouldNotify)
            {
                VerifyNotificationWasSent(Times.Once);
            }
            else
            {
                VerifyNotificationWasSent(Times.Never);
            }
        }

        [Test]
        public async Task TestRegionalizationForLinearNotInRegion()
        {
            _epgEvent.GroupId = GROUP_ID_ENABLED_REGIONALIZATION;
            _epgEvent.LiveAssetId = -1;

            await _handler.Handle(_epgEvent);

            _iotNotificationServiceMock.Verify(
                x => x.SendNotificationAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Never);
        }

        [Test]
        public async Task TestRegionalizationForLinearInRegion()
        {
            _epgEvent.GroupId = GROUP_ID_ENABLED_REGIONALIZATION;

            await _handler.Handle(_epgEvent);

            foreach (var region in Regions)
            {
                _iotNotificationServiceMock.Verify(
                    x => x.SendNotificationAsync(GROUP_ID_ENABLED_REGIONALIZATION, It.IsAny<string>(), $"MockRegionTopic_{region}_{{0}}"),
                    Times.Once);
            }
        }

        private static IEnumerable TestBackwardAndForwardTimeRangeForSendingNotificationsSource()
        {
            yield return new TestCaseData(6, 6, true, new EpgNotificationEvent
            {
                GroupId = GROUP_ID_DISABLED_REGIONALIZATION,
                UpdatedRange = new Range<DateTime>(DateTime.UtcNow.AddHours(-7), DateTime.UtcNow.AddHours(3)),
                LiveAssetId = LIVE_ASSET_ID,
                DisableEpgNotification = false
            });
            
            yield return new TestCaseData(6, 6, true, new EpgNotificationEvent
            {
                GroupId = GROUP_ID_DISABLED_REGIONALIZATION,
                UpdatedRange = new Range<DateTime>(DateTime.UtcNow, DateTime.UtcNow.AddHours(3)),
                LiveAssetId = LIVE_ASSET_ID,
                DisableEpgNotification = false
            });
            
            yield return new TestCaseData(6, 6, true, new EpgNotificationEvent
            {
                GroupId = GROUP_ID_DISABLED_REGIONALIZATION,
                UpdatedRange = new Range<DateTime>(DateTime.UtcNow, DateTime.UtcNow.AddHours(10)),
                LiveAssetId = LIVE_ASSET_ID,
                DisableEpgNotification = false
            });
            
            yield return new TestCaseData(6, 6, false, new EpgNotificationEvent
            {
                GroupId = GROUP_ID_DISABLED_REGIONALIZATION,
                UpdatedRange = new Range<DateTime>(DateTime.UtcNow.AddHours(7), DateTime.UtcNow.AddHours(8)),
                LiveAssetId = LIVE_ASSET_ID,
                DisableEpgNotification = false
            });
            
            yield return new TestCaseData(6, 6, false, new EpgNotificationEvent
            {
                GroupId = GROUP_ID_DISABLED_REGIONALIZATION,
                UpdatedRange = new Range<DateTime>(DateTime.UtcNow.AddHours(-8), DateTime.UtcNow.AddHours(-7)),
                LiveAssetId = LIVE_ASSET_ID,
                DisableEpgNotification = false
            });
            
            yield return new TestCaseData(0, 0, false, new EpgNotificationEvent
            {
                GroupId = GROUP_ID_DISABLED_REGIONALIZATION,
                UpdatedRange = new Range<DateTime>(DateTime.UtcNow.AddHours(-6.1), DateTime.UtcNow.AddHours(8)),
                LiveAssetId = LIVE_ASSET_ID,
                DisableEpgNotification = false
            });
            
            // #region Backward - 6H, Forward - 6H
            //
            // yield return new TestCaseData(6, 6, true, new EpgNotificationEvent
            // {
            //     UpdatedRange = new Range<DateTime>(DateTime.UtcNow, DateTime.UtcNow.AddHours(3)),
            //     LiveAssetId = 10,
            //     DisableEpgNotification = false
            // });
            //
            // yield return new TestCaseData(6, 6, false, new EpgNotificationEvent
            // {
            //     UpdatedRange = new Range<DateTime>(DateTime.UtcNow.AddHours(6.1), DateTime.UtcNow.AddHours(8)),
            //     LiveAssetId = 10,
            //     DisableEpgNotification = false
            // });
            //
            // #endregion
            //
            // #region Backward - 0H, Forward - 6H
            //
            // yield return new TestCaseData(0, 6, true, new EpgNotificationEvent
            // {
            //     UpdatedRange = new Range<DateTime>(DateTime.UtcNow.AddHours(-1), DateTime.UtcNow.AddHours(3)),
            //     LiveAssetId = 10,
            //     DisableEpgNotification = false
            // });
            //
            // #endregion
            //
            // #region Backward - 6H, Forward - 0H
            //
            // yield return new TestCaseData(6, 0, false, new EpgNotificationEvent
            // {
            //     UpdatedRange = new Range<DateTime>(DateTime.UtcNow.AddHours(6.1), DateTime.UtcNow.AddHours(8)),
            //     LiveAssetId = 10,
            //     DisableEpgNotification = false
            // });
            //
            // yield return new TestCaseData(6, 0, false, new EpgNotificationEvent
            // {
            //     UpdatedRange = new Range<DateTime>(DateTime.UtcNow.AddHours(5.9), DateTime.UtcNow.AddHours(8)),
            //     LiveAssetId = 10,
            //     DisableEpgNotification = false
            // });
            //
            // #endregion
            //
            // #region Backward - 0H, Forward - 0H
            //
            // yield return new TestCaseData(0, 0, false, new EpgNotificationEvent
            // {
            //     UpdatedRange = new Range<DateTime>(DateTime.UtcNow.AddHours(-6.1), DateTime.UtcNow.AddHours(8)),
            //     LiveAssetId = 10,
            //     DisableEpgNotification = false
            // });
            //
            // #endregion
        }

        [Test]
        public async Task TestForwardBackwardTimeRanges()
        {
            await _handler.Handle(_epgEvent);
            VerifyNotificationWasSent(Times.Once);
        }
        
        [Test]
        public async Task TestIotNotificationDisabled()
        {
            _notificationSettings.IsIotEnabled = false;
            await _handler.Handle(_epgEvent);
            VerifyNotificationWasSent(Times.Never);
        }
        
        [Test]
        public async Task TestEpgNotificationDisabled()
        {
            _notificationSettings.EpgNotification.Enabled = false;
            await _handler.Handle(_epgEvent);
            VerifyNotificationWasSent(Times.Never);
        }

        [TestCaseSource(nameof(EventsTestCases))]
        public async Task TestEventIsFiltered(EpgNotificationEvent epgEvent)
        {
            await _handler.Handle(epgEvent);
            VerifyNotificationWasSent(Times.Never);
        }

        private static IEnumerable EventsTestCases()
        {
            yield return new TestCaseData(new EpgNotificationEvent
            {
                UpdatedRange = new Range<DateTime>(DateTime.UtcNow.AddHours(10), DateTime.UtcNow.AddHours(13)),
                LiveAssetId = 10
            }).SetName("EventNotInTimeRange");
            yield return new TestCaseData(new EpgNotificationEvent
            {
                UpdatedRange = new Range<DateTime>(DateTime.UtcNow.AddSeconds(12), DateTime.UtcNow.AddHours(3)),
                LiveAssetId = 11
            }).SetName("EventFromIgnoredLiveAsset");
            yield return new TestCaseData(new EpgNotificationEvent
            {
                UpdatedRange = new Range<DateTime>(DateTime.UtcNow.AddSeconds(12), DateTime.UtcNow.AddHours(3)),
                LiveAssetId = 10,
                DisableEpgNotification = true
            }).SetName("EventWithDisabledEpgNotification");
        }

        private void VerifyNotificationWasSent(Func<Times> times)
        {
            // should always call invalidation
            //_epgCacheClient.Verify(m => m.InvalidateEpgAsync(It.IsAny<int>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long>()), Times.Once);
            
            _iotNotificationServiceMock.Verify(mock => mock.SendNotificationAsync(GROUP_ID_DISABLED_REGIONALIZATION, It.IsAny<string>(), "MockTopic{0}"), times);
        }

        private static Mock<IIotManager> GetMockIotManager()
        {
            var mock = new Mock<IIotManager>();
            mock.Setup(m => m
                .GetTopicFormat(GROUP_ID_DISABLED_REGIONALIZATION, EventType.epg_update))
                .Returns("MockTopic{0}");
            mock.Setup(m => m.GetRegionTopicFormat(
                    GROUP_ID_ENABLED_REGIONALIZATION,
                    EventType.epg_update,
                    It.Is<long>(r => Regions.Any(id => r == id))))
                .Returns((int g, EventType e, long r) => $"MockRegionTopic_{r}_{{0}}");
            return mock;
        }

        private static Mock<INotificationCache> GetMockSettings(NotificationPartnerSettings settings)
        {
            var mock = new Mock<INotificationCache>();
            mock.Setup(m => m
                .GetPartnerNotificationSettings(
                    It.Is<int>(x => x == GROUP_ID_ENABLED_REGIONALIZATION || x == GROUP_ID_DISABLED_REGIONALIZATION)))
                .Returns(new NotificationPartnerSettingsResponse()
                {
                    settings = settings,
                    Status = new Status(eResponseStatus.OK)
                });
            return mock;
        }

        private static Mock<ICatalogManager> GetCatalogMangerMock()
        {
            var mock = new Mock<ICatalogManager>();
            mock.Setup(x => x.IsRegionalizationEnabled(GROUP_ID_DISABLED_REGIONALIZATION)).Returns(false);
            mock.Setup(x => x.IsRegionalizationEnabled(GROUP_ID_ENABLED_REGIONALIZATION)).Returns(true);

            return mock;
        }

        private static Mock<IRegionManager> GetRegionManagerMock()
        {
            var mock = new Mock<IRegionManager>();
            mock
                .Setup(x => x.GetLinearMediaRegions(GROUP_ID_DISABLED_REGIONALIZATION))
                .Returns(new Dictionary<long, List<int>>());
            mock
                .Setup(x => x.GetLinearMediaRegions(GROUP_ID_ENABLED_REGIONALIZATION))
                .Returns(new Dictionary<long, List<int>>
                {
                    { LIVE_ASSET_ID, Regions }
                });

            return mock;
        }

        private static IMock<IEpgNotificationConfiguration> GetConfigurationMock()
        {
            var mock = new Mock<IEpgNotificationConfiguration>();
            mock
                .Setup(x => x.CloudFrontInvalidationTtlInMs).Returns(0);

            return mock;
        }

        private static Mock<IIotNotificationService> GetIotNotificationServiceMock()
        {
            var mock = new Mock<IIotNotificationService>();
            mock
                .Setup(m => m.SendNotificationAsync(
                    It.Is<int>(x => x == GROUP_ID_ENABLED_REGIONALIZATION || x == GROUP_ID_DISABLED_REGIONALIZATION),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            return mock;
        }
    }
}