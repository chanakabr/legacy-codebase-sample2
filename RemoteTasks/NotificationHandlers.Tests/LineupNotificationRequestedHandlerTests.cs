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
using LineupNotificationHandler;
using LineupNotificationHandler.Configuration;
using Moq;
using NotificationHandlers.Common;
using NUnit.Framework;

namespace NotificationHandlers.Tests
{
    public class LineupNotificationRequestedHandlerTests
    {
        private const int GROUP_ID_DISABLED_REGIONALIZATION = 1;
        private const int GROUP_ID_ENABLED_REGIONALIZATION = 1;
        private static readonly List<long> Regions = new List<long> { 1, 2 };
        private static readonly List<long> ChildRegions = new List<long> { 3, 4 };

        private static readonly NotificationPartnerSettings IotEnabledSettings = new NotificationPartnerSettings
        {
            IsIotEnabled = true,
            LineupNotification = new LineupNotificationSettings { Enabled = true }
        };

        private MockRepository _mockRepository;

        [SetUp]
        public void Setup()
        {
            _mockRepository = new MockRepository(MockBehavior.Strict);
        }

        [TearDown]
        public void TearDown()
        {
            _mockRepository.VerifyAll();
        }

        [Test]
        public async Task Handle_EmptyRegionIds_SkipNotification()
        {
            var @event = new LineupNotificationRequestedEvent { RegionIds = new List<long>() };

            var handler = new LineupNotificationRequestedHandler(
                _mockRepository.Create<ILineupNotificationConfiguration>().Object,
                _mockRepository.Create<IIotManager>().Object,
                _mockRepository.Create<INotificationCache>().Object,
                _mockRepository.Create<ICatalogManager>().Object,
                _mockRepository.Create<IRegionManager>().Object,
                _mockRepository.Create<IIotNotificationService>().Object);

            await handler.Handle(@event);
        }

        [Test]
        public async Task Handle_RegionalizationDisabled_SkipNotification()
        {
            var @event = new LineupNotificationRequestedEvent
            {
                GroupId = GROUP_ID_DISABLED_REGIONALIZATION,
                RegionIds = new List<long> { Regions.First() }
            };

            var handler = new LineupNotificationRequestedHandler(
                _mockRepository.Create<ILineupNotificationConfiguration>().Object,
                _mockRepository.Create<IIotManager>().Object,
                GetNotificationCacheMock(GROUP_ID_DISABLED_REGIONALIZATION, IotEnabledSettings).Object,
                GetCatalogManagerMock(GROUP_ID_DISABLED_REGIONALIZATION, false).Object,
                _mockRepository.Create<IRegionManager>().Object,
                _mockRepository.Create<IIotNotificationService>().Object);

            await handler.Handle(@event);
        }

        [Test]
        public async Task Handle_RegionsNotInGroup_SkipNotification()
        {
            var @event = new LineupNotificationRequestedEvent
            {
                GroupId = GROUP_ID_ENABLED_REGIONALIZATION,
                RegionIds = new List<long> { Regions.First(), 3 }
            };

            var handler = new LineupNotificationRequestedHandler(
                _mockRepository.Create<ILineupNotificationConfiguration>().Object,
                _mockRepository.Create<IIotManager>().Object,
                GetNotificationCacheMock(GROUP_ID_ENABLED_REGIONALIZATION, IotEnabledSettings).Object,
                GetCatalogManagerMock(GROUP_ID_ENABLED_REGIONALIZATION, true).Object,
                GetRegionManagerMock(GROUP_ID_ENABLED_REGIONALIZATION, Regions).Object,
                _mockRepository.Create<IIotNotificationService>().Object);

            await handler.Handle(@event);
        }

        [TestCaseSource(nameof(LineupNotificationDisabledSettings))]
        public async Task Handle_LineupNotificationNotAllowed_SkipNotification(NotificationPartnerSettings settings)
        {
            var @event = new LineupNotificationRequestedEvent
            {
                GroupId = GROUP_ID_ENABLED_REGIONALIZATION,
                RegionIds = new List<long> { Regions.First() }
            };

            var handler = new LineupNotificationRequestedHandler(
                _mockRepository.Create<ILineupNotificationConfiguration>().Object,
                _mockRepository.Create<IIotManager>().Object,
                GetNotificationCacheMock(GROUP_ID_ENABLED_REGIONALIZATION, settings).Object,
                GetCatalogManagerMock(GROUP_ID_ENABLED_REGIONALIZATION, true).Object,
                GetRegionManagerMock(GROUP_ID_ENABLED_REGIONALIZATION, Regions).Object,
                _mockRepository.Create<IIotNotificationService>().Object);

            await handler.Handle(@event);
        }

        [Test]
        public async Task Handle_DuplicatedRegions_NotificationPublishedOnce()
        {
            var @event = new LineupNotificationRequestedEvent
            {
                GroupId = GROUP_ID_ENABLED_REGIONALIZATION,
                RegionIds = new List<long> { Regions.First(), Regions.First() }
            };

            var regionManager = GetRegionManagerMock(GROUP_ID_ENABLED_REGIONALIZATION, Regions);
            regionManager.Setup(x => x.GetChildRegionIds(GROUP_ID_ENABLED_REGIONALIZATION, Regions.First()))
                .Returns(new List<long>());

            var handler = new LineupNotificationRequestedHandler(
                GetConfigurationMock().Object,
                GetIotManagerMock(GROUP_ID_ENABLED_REGIONALIZATION, new List<long> { Regions.First() }).Object,
                GetNotificationCacheMock(GROUP_ID_ENABLED_REGIONALIZATION, IotEnabledSettings).Object,
                GetCatalogManagerMock(GROUP_ID_ENABLED_REGIONALIZATION, true).Object,
                regionManager.Object,
                GetIotNotificationServiceMock(GROUP_ID_ENABLED_REGIONALIZATION, new List<long> { Regions.First() }).Object);

            await handler.Handle(@event);
        }

        [Test]
        public async Task Handle_ChildRegions_MultipleNotificationsPublished()
        {
            var @event = new LineupNotificationRequestedEvent
            {
                GroupId = GROUP_ID_ENABLED_REGIONALIZATION,
                RegionIds = new List<long> { Regions.Last() }
            };

            var regionManager = GetRegionManagerMock(GROUP_ID_ENABLED_REGIONALIZATION, Regions);
            regionManager.Setup(x => x.GetChildRegionIds(GROUP_ID_ENABLED_REGIONALIZATION, Regions.Last()))
                .Returns(ChildRegions);

            var regionsToNotify = new List<long> { Regions.Last() }.Union(ChildRegions).ToList();

            var handler = new LineupNotificationRequestedHandler(
                GetConfigurationMock().Object,
                GetIotManagerMock(GROUP_ID_ENABLED_REGIONALIZATION, regionsToNotify).Object,
                GetNotificationCacheMock(GROUP_ID_ENABLED_REGIONALIZATION, IotEnabledSettings).Object,
                GetCatalogManagerMock(GROUP_ID_ENABLED_REGIONALIZATION, true).Object,
                regionManager.Object,
                GetIotNotificationServiceMock(GROUP_ID_ENABLED_REGIONALIZATION, regionsToNotify).Object);

            await handler.Handle(@event);
        }

        private static IEnumerable<TestCaseData> LineupNotificationDisabledSettings()
        {
            yield return null;
            yield return new TestCaseData(new NotificationPartnerSettings());
            yield return new TestCaseData(new NotificationPartnerSettings { IsIotEnabled = true });
            yield return new TestCaseData(new NotificationPartnerSettings
            {
                IsIotEnabled = true,
                LineupNotification = new LineupNotificationSettings { Enabled = false }
            });
        }

        private Mock<INotificationCache> GetNotificationCacheMock(int groupId, NotificationPartnerSettings settings)
        {
            var mock = _mockRepository.Create<INotificationCache>();

            mock.Setup(x => x.GetPartnerNotificationSettings(groupId))
                .Returns(new NotificationPartnerSettingsResponse
                {
                    Status = Status.Ok,
                    settings = settings
                });

            return mock;
        }

        private Mock<ICatalogManager> GetCatalogManagerMock(int groupId, bool isRegionalizationEnabled)
        {
            var mock = _mockRepository.Create<ICatalogManager>();
            mock.Setup(x => x.IsRegionalizationEnabled(groupId)).Returns(isRegionalizationEnabled);

            return mock;
        }

        private Mock<IRegionManager> GetRegionManagerMock(int groupId, List<long> regionIds)
        {
            var mock = _mockRepository.Create<IRegionManager>();
            mock.Setup(x => x.GetRegionIds(groupId))
                .Returns(regionIds.Select(x => (int)x).ToList());

            return mock;
        }

        private static Mock<ILineupNotificationConfiguration> GetConfigurationMock()
        {
            var mock = new Mock<ILineupNotificationConfiguration>();
            mock
                .Setup(x => x.CloudFrontInvalidationTtlInMs).Returns(0);

            return mock;
        }

        private static Mock<IIotManager> GetIotManagerMock(int groupId, IEnumerable<long> regionIds)
        {
            var mock = new Mock<IIotManager>();
            mock.Setup(m => m.GetRegionTopicFormat(groupId, EventType.lineup_updated, It.Is<int>(x => regionIds.Contains(x))))
                .Returns((int g, EventType e, int r) => $"MockRegionTopic_{r}_{0}");

            mock.Setup(m => m.GetTopicPartitionsCount())
                .Returns(1);

            return mock;
        }

        private static Mock<IIotNotificationService> GetIotNotificationServiceMock(
            int groupId,
            IEnumerable<long> regionIds)
        {
            var mock = new Mock<IIotNotificationService>();
            foreach (var regionId in regionIds)
            {
                mock.Setup(x => x.SendNotificationAsync(
                        groupId,
                        It.Is<string>(m => m.StartsWith("{\"header\":{\"event_type\":2,\"event_date\":")),
                        $"MockRegionTopic_lineup_updated_{regionId}_{{0}}"))
                    .Returns(Task.CompletedTask);
            }

            return mock;
        }
    }
}