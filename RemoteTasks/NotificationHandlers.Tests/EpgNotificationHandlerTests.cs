using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using ApiLogic.Api.Managers;
using ApiObjects.Cloudfront;
using ApiObjects.EventBus;
using ApiObjects.Notification;
using Core.Notification;
using Core.Tests;
using EventBus.RabbitMQ;
using IotGrpcClientWrapper;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using phoenix;
using TVinciShared;
using Range = ApiObjects.EventBus.Range;
using static EpgNotificationHandler.EpgNotificationHandler;

namespace NotificationHandlers.Tests
{
    public class EpgNotificationHandlerTests
    {
        private const int LIVE_ASSET_ID = 10;
        private const int GROUP_ID = 1;
        private static readonly List<int> Regions = new List<int> { 2, 3 };
        private static readonly DateTime Now = DateTime.UtcNow;
        private static readonly EpgNotificationEvent DefaultEvent = new EpgNotificationEvent
        {
            GroupId = GROUP_ID,
            UpdatedRange = new Range<DateTime>(Now.AddHours(3), Now.AddHours(5)),
            LiveAssetId = LIVE_ASSET_ID,
            DisableEpgNotification = false
        };
        private static readonly NotificationPartnerSettings DefaultSettings = new NotificationPartnerSettings
        {
            IsIotEnabled = true,
            EpgNotification = new EpgNotificationSettings
            {
                Enabled = true,
                BackwardTimeRange = 72, // hours
                ForwardTimeRange = 72, // hours
                LiveAssetIds = new long[]{LIVE_ASSET_ID}
            }
        };
        private const string IOT_MESSAGE = "{\"header";

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
        public async Task Event_DisableEpgNotification_Should_Skip_InvalidateAndNotify()
        {
            var handler = new EpgNotificationHandler.EpgNotificationHandler(
                _mockRepository.Create<INotificationCache>().Object,
                _mockRepository.Create<IRegionManager>().Object,
                _mockRepository.Create<IIotClient>().Object,
                _mockRepository.Create<ICloudfrontInvalidator>().Object,
                Logger);
            
            await handler.Handle(DefaultEvent.With(_ => _.DisableEpgNotification = true));
        }

        [TestCaseSource(nameof(PartnerNotificationSettings))]
        public async Task PartnerNotificationSettings_Should_Skip_InvalidateAndNotify(
            NotificationPartnerSettings s, EpgNotificationEvent e)
        {
            var handler = new EpgNotificationHandler.EpgNotificationHandler(
                _mockRepository.NotificationCache(GROUP_ID, s),
                _mockRepository.Create<IRegionManager>().Object,
                _mockRepository.Create<IIotClient>().Object,
                _mockRepository.Create<ICloudfrontInvalidator>().Object,
                Logger);

            await handler.Handle(e);
        }
        private static IEnumerable PartnerNotificationSettings()
        {
            var s = DefaultSettings;
            var e = DefaultEvent;
            yield return new TestCaseData(s.With(_ => _.IsIotEnabled = null), e).SetName("IotEnabled empty");
            yield return new TestCaseData(s.With(_ => _.IsIotEnabled = false), e).SetName("Iot disabled");
            yield return new TestCaseData(s.With(_ => _.EpgNotification = null), e).SetName("EpgNotification empty");
            yield return new TestCaseData(s.With(_ => _.EpgNotification.Enabled = false), e).SetName("EpgNotification disabled");
            yield return new TestCaseData(s.With(_ => _.EpgNotification.LiveAssetIds = new long[]{LIVE_ASSET_ID + 1}), e).SetName("LiveAssetId not in whitelist");
            yield return new TestCaseData(s, e.With(_ => _.UpdatedRange = Range.Create(Now.AddHours(73), Now.AddHours(75)))).SetName("Event out of ForwardTimeRange");
            yield return new TestCaseData(s, e.With(_ => _.UpdatedRange = Range.Create(Now.AddHours(-75), Now.AddHours(-73)))).SetName("Event out of BackwardTimeRange");
        }
        
        [Test]
        public async Task Event_Should_Invalidate_OneDay_And_Notify()
        {
            var day = Now.Date;
            string[] cloudfrontInvalidationPaths = {
                $"/api_v3/service/epg/action/get/partnerid/{GROUP_ID}/date/{day.ToUtcUnixTimestampSeconds()}/*"
            };
            var handler = new EpgNotificationHandler.EpgNotificationHandler(
                _mockRepository.NotificationCache(GROUP_ID, DefaultSettings),
                RegionManager(),
                _mockRepository.IotClient(GROUP_ID, IOT_MESSAGE, EventNotificationType.EpgUpdate, 2, 3),
                _mockRepository.CloudfrontInvalidator(GROUP_ID, cloudfrontInvalidationPaths),
                Logger);

            await handler.Handle(DefaultEvent.With(_ => _.UpdatedRange = Range.Create(day.AddHours(3), day.AddHours(5))));
        }
        
        [Test]
        public async Task Event_With_UpdateRangeTo_Higher_Than_Limit_Should_Invalidate_And_Notify()
        {
            var day = Now.Date;
            var forwardTimeRange = DefaultSettings.EpgNotification.ForwardTimeRange; // 72h == 3d
            string[] cloudfrontInvalidationPaths = {
                $"/api_v3/service/epg/action/get/partnerid/{GROUP_ID}/date/{day.ToUtcUnixTimestampSeconds()}/*",
                $"/api_v3/service/epg/action/get/partnerid/{GROUP_ID}/date/{day.AddDays(1).ToUtcUnixTimestampSeconds()}/*",
                $"/api_v3/service/epg/action/get/partnerid/{GROUP_ID}/date/{day.AddDays(2).ToUtcUnixTimestampSeconds()}/*",
                $"/api_v3/service/epg/action/get/partnerid/{GROUP_ID}/date/{day.AddDays(3).ToUtcUnixTimestampSeconds()}/*"
            };
            var handler = new EpgNotificationHandler.EpgNotificationHandler(
                _mockRepository.NotificationCache(GROUP_ID, DefaultSettings),
                RegionManager(),
                _mockRepository.IotClient(GROUP_ID, IOT_MESSAGE, EventNotificationType.EpgUpdate, 2, 3),
                _mockRepository.CloudfrontInvalidator(GROUP_ID, cloudfrontInvalidationPaths),
                Logger);
            
            await handler.Handle(DefaultEvent.With(_ => _.UpdatedRange = Range.Create(Now, Now.AddHours(forwardTimeRange + 0.1))));
        }
        
        [Test]
        public async Task Event_With_UpdateRangeFrom_Less_Than_Limit_Should_Invalidate_And_Notify()
        {
            var day = Now.Date;
            var backwardTimeRange = DefaultSettings.EpgNotification.BackwardTimeRange; // 72h == 3d
            string[] cloudfrontInvalidationPaths = {
                $"/api_v3/service/epg/action/get/partnerid/{GROUP_ID}/date/{day.AddDays(-3).ToUtcUnixTimestampSeconds()}/*",
                $"/api_v3/service/epg/action/get/partnerid/{GROUP_ID}/date/{day.AddDays(-2).ToUtcUnixTimestampSeconds()}/*",
                $"/api_v3/service/epg/action/get/partnerid/{GROUP_ID}/date/{day.AddDays(-1).ToUtcUnixTimestampSeconds()}/*",
                $"/api_v3/service/epg/action/get/partnerid/{GROUP_ID}/date/{day.ToUtcUnixTimestampSeconds()}/*"
            };
            var handler = new EpgNotificationHandler.EpgNotificationHandler(
                _mockRepository.NotificationCache(GROUP_ID, DefaultSettings),
                RegionManager(),
                _mockRepository.IotClient(GROUP_ID, IOT_MESSAGE, EventNotificationType.EpgUpdate, 2, 3),
                _mockRepository.CloudfrontInvalidator(GROUP_ID, cloudfrontInvalidationPaths),
                Logger);
            
            await handler.Handle(DefaultEvent.With(_ => _.UpdatedRange = Range.Create(Now.AddHours(-(backwardTimeRange + 0.1)), Now)));
        }
        
        [Test]
        public async Task Event_Should_InvalidateWholePartner_And_Notify()
        {
            var day = Now.Date;
            string[] cloudfrontInvalidationPaths = { $"/api_v3/service/epg/action/get/partnerid/{GROUP_ID}/*" };
            var handler = new EpgNotificationHandler.EpgNotificationHandler(
                _mockRepository.NotificationCache(GROUP_ID, DefaultSettings),
                RegionManager(),
                _mockRepository.IotClient(GROUP_ID, IOT_MESSAGE, EventNotificationType.EpgUpdate, 2, 3),
                _mockRepository.CloudfrontInvalidator(GROUP_ID, cloudfrontInvalidationPaths),
                Logger);

            await handler.Handle(DefaultEvent.With(_ => _.UpdatedRange = Range.Create(day, day.AddDays(COUNT_OF_DAYS_FOR_FULL_INVALIDATION))));
        }
        
        [Test]
        public void CloudfrontInvalidationFailed_ShouldThrowRetryException()
        {
            var day = Now.Date;
            string[] cloudfrontInvalidationPaths = {
                $"/api_v3/service/epg/action/get/partnerid/{GROUP_ID}/date/{day.ToUtcUnixTimestampSeconds()}/*"
            };
            var handler = new EpgNotificationHandler.EpgNotificationHandler(
                _mockRepository.NotificationCache(GROUP_ID, DefaultSettings),
                RegionManager(),
                _mockRepository.Create<IIotClient>().Object,
                _mockRepository.CloudfrontInvalidator(GROUP_ID, cloudfrontInvalidationPaths, returnSuccess: false),
                Logger);

            Assert.ThrowsAsync<RetryableErrorException>(() => handler.Handle(DefaultEvent.With(_ => _.UpdatedRange = Range.Create(day.AddHours(3), day.AddHours(5)))));
        }

        private static IRegionManager RegionManager()
        {
            var mock = new Mock<IRegionManager>();
            mock
                .Setup(x => x.GetLinearMediaRegions(GROUP_ID))
                .Returns(new Dictionary<long, List<int>>
                {
                    { LIVE_ASSET_ID, Regions },
                    { LIVE_ASSET_ID + 1, new List<int>() }
                });
        
            return mock.Object;
        }
        
        private static ILogger<EpgNotificationHandler.EpgNotificationHandler> Logger =>
            Mock.Of<ILogger<EpgNotificationHandler.EpgNotificationHandler>>();
    }
}