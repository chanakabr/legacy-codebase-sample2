using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiLogic.Api.Managers;
using ApiObjects.Cloudfront;
using ApiObjects.EventBus;
using ApiObjects.Notification;
using Core.Catalog.CatalogManagement;
using Core.Notification;
using EventBus.RabbitMQ;
using IotGrpcClientWrapper;
using LineupNotificationHandler;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using phoenix;

namespace NotificationHandlers.Tests
{
    public class LineupNotificationRequestedHandlerTests
    {
        private const int GROUP_ID = 1;
        private static readonly List<long> Regions = new List<long> { 1, 2 };
        private static readonly List<long> ChildRegions = new List<long> { 3, 4 };
        private static readonly NotificationPartnerSettings IotEnabledSettings = new NotificationPartnerSettings
        {
            IsIotEnabled = true,
            LineupNotification = new LineupNotificationSettings { Enabled = true }
        };
        private static readonly string[] CloudfrontInvalidationPaths = {
            $"/api_v3/service/epg/action/get/partnerid/{GROUP_ID}/*",
            $"/api_v3/service/lineup/action/get/partnerid/{GROUP_ID}/*"
        };
        private const string IOT_MESSAGE = @"{""header"":{""event_type"":2,""event_date"":";

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
                _mockRepository.Create<INotificationCache>().Object,
                _mockRepository.Create<ICatalogManager>().Object,
                _mockRepository.Create<IRegionManager>().Object,
                _mockRepository.Create<IIotClient>().Object,
                _mockRepository.Create<ICloudfrontInvalidator>().Object,
                Logger);

            await handler.Handle(@event);
        }

        [Test]
        public async Task Handle_RegionalizationDisabled_SkipNotification()
        {
            var @event = new LineupNotificationRequestedEvent
            {
                GroupId = GROUP_ID,
                RegionIds = new List<long> { Regions.First() }
            };

            var handler = new LineupNotificationRequestedHandler(
                _mockRepository.Create<INotificationCache>().Object,
                CatalogManager(GROUP_ID, false),
                _mockRepository.Create<IRegionManager>().Object,
                _mockRepository.Create<IIotClient>().Object,
                _mockRepository.Create<ICloudfrontInvalidator>().Object,
                Logger);

            await handler.Handle(@event);
        }

        [Test]
        public async Task Handle_RegionsNotInGroup_SkipNotification()
        {
            var @event = new LineupNotificationRequestedEvent
            {
                GroupId = GROUP_ID,
                RegionIds = new List<long> { Regions.First(), 3 }
            };

            var handler = new LineupNotificationRequestedHandler(
                _mockRepository.Create<INotificationCache>().Object,
                CatalogManager(GROUP_ID, true),
                RegionManagerMock(GROUP_ID, Regions).Object,
                _mockRepository.Create<IIotClient>().Object,
                _mockRepository.Create<ICloudfrontInvalidator>().Object,
                Logger);

            await handler.Handle(@event);
        }

        [TestCaseSource(nameof(LineupNotificationDisabledSettings))]
        public async Task Handle_LineupNotificationNotAllowed_SkipNotification(NotificationPartnerSettings settings)
        {
            var @event = new LineupNotificationRequestedEvent
            {
                GroupId = GROUP_ID,
                RegionIds = new List<long> { Regions.First() }
            };

            var handler = new LineupNotificationRequestedHandler(
                _mockRepository.NotificationCache(GROUP_ID, settings),
                CatalogManager(GROUP_ID, true),
                RegionManagerMock(GROUP_ID, Regions).Object,
                _mockRepository.Create<IIotClient>().Object,
                _mockRepository.Create<ICloudfrontInvalidator>().Object,
                Logger);

            await handler.Handle(@event);
        }

        [Test]
        public async Task Handle_DuplicatedRegions_NotificationPublishedOnce()
        {
            var @event = new LineupNotificationRequestedEvent
            {
                GroupId = GROUP_ID,
                RegionIds = new List<long> { Regions.First(), Regions.First() }
            };

            var regionManager = RegionManagerMock(GROUP_ID, Regions);
            regionManager.Setup(x => x.GetChildRegionIds(GROUP_ID, Regions.First()))
                .Returns(new List<long>());

            var handler = new LineupNotificationRequestedHandler(
                _mockRepository.NotificationCache(GROUP_ID, IotEnabledSettings),
                CatalogManager(GROUP_ID, true),
                regionManager.Object,
                _mockRepository.IotClient(GROUP_ID, IOT_MESSAGE, EventNotificationType.LineupUpdated, Regions.First()),
                _mockRepository.CloudfrontInvalidator(GROUP_ID, CloudfrontInvalidationPaths),
                Logger);

            await handler.Handle(@event);
        }

        [Test]
        public async Task Handle_ChildRegions_MultipleNotificationsPublished()
        {
            var @event = new LineupNotificationRequestedEvent
            {
                GroupId = GROUP_ID,
                RegionIds = new List<long> { Regions.Last() }
            };

            var regionManager = RegionManagerMock(GROUP_ID, Regions);
            regionManager.Setup(x => x.GetChildRegionIds(GROUP_ID, Regions.Last()))
                .Returns(ChildRegions);

            var regionsToNotify = ChildRegions.Prepend(Regions.Last()).ToArray();

            var handler = new LineupNotificationRequestedHandler(
                _mockRepository.NotificationCache(GROUP_ID, IotEnabledSettings),
                CatalogManager(GROUP_ID, true),
                regionManager.Object,
                _mockRepository.IotClient(GROUP_ID, IOT_MESSAGE, EventNotificationType.LineupUpdated, regionsToNotify),
                _mockRepository.CloudfrontInvalidator(GROUP_ID, CloudfrontInvalidationPaths),
                Logger);

            await handler.Handle(@event);
        }
        
        [Test]
        public void Handle_InvalidateCloudfrontFailure_ShouldThrowRetryException()
        {
            var @event = new LineupNotificationRequestedEvent
            {
                GroupId = GROUP_ID,
                RegionIds = new List<long> { Regions.First(), Regions.First() }
            };
            
            var handler = new LineupNotificationRequestedHandler(
                _mockRepository.NotificationCache(GROUP_ID, IotEnabledSettings),
                CatalogManager(GROUP_ID, true),
                RegionManagerMock(GROUP_ID, new List<long>{Regions.First()}).Object,
                _mockRepository.Create<IIotClient>().Object,
                _mockRepository.CloudfrontInvalidator(GROUP_ID, CloudfrontInvalidationPaths, returnSuccess: false),
                Logger);

            Assert.ThrowsAsync<RetryableErrorException>(() => handler.Handle(@event));
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

        private ICatalogManager CatalogManager(int groupId, bool isRegionalizationEnabled)
        {
            var mock = _mockRepository.Create<ICatalogManager>();
            mock.Setup(x => x.IsRegionalizationEnabled(groupId)).Returns(isRegionalizationEnabled);
            return mock.Object;
        }

        private Mock<IRegionManager> RegionManagerMock(int groupId, List<long> regionIds)
        {
            var mock = _mockRepository.Create<IRegionManager>();
            mock.Setup(x => x.GetRegionIds(groupId))
                .Returns(regionIds.Select(x => (int)x).ToList());
            return mock;
        }

        private static ILogger<LineupNotificationRequestedHandler> Logger =>
            Mock.Of<ILogger<LineupNotificationRequestedHandler>>();
    }
}