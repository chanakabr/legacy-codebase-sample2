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
using IotGrpcClientWrapper;
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

        //private Mock<IEpgCacheClient> _epgCacheClient;
        private NotificationPartnerSettings _notificationSettings;
        private EpgNotificationHandler.EpgNotificationHandler _handler;
        private EpgNotificationEvent _epgEvent;
        
        [SetUp]
        public void Setup()
        {
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
            _handler = new EpgNotificationHandler.EpgNotificationHandler(
                configurationMock.Object,
                notificationSettingsCache.Object,
                catalogManagerMock.Object,
                regionManagerMock.Object,
                new Mock<IIotClient>().Object);
            
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
    }
}