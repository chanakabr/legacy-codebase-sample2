using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using ApiLogic.Notification;
using ApiObjects.EventBus;
using ApiObjects.Notification;
using ApiObjects.Response;
using Core.Notification;
using Moq;
using NUnit.Framework;

namespace EpgNotificationHandler.Tests
{
    public class Tests
    {
        private Mock<IIotManager> _iotManager;
        //private Mock<IEpgCacheClient> _epgCacheClient;
        private NotificationPartnerSettings _notificationSettings;
        private EpgNotificationHandler _handler;
        private EpgNotificationEvent _epgEvent;
        
        [SetUp]
        public void Setup()
        {
            _iotManager = GetMockIotManager(1);
            _notificationSettings = new NotificationPartnerSettings
            {
                IsIotEnabled = true,
                EpgNotification = new EpgNotificationSettings
                {
                    Enabled = true,
                    BackwardTimeRange = 2,
                    ForwardTimeRange = 2,
                    DeviceFamilyIds = new List<int> {1, 2, 3, 4, 5},
                    LiveAssetIds = new List<long> {10, 20, 30, 40}
                }
            };
            var notificationSettingsCache = GetMockSettings(_notificationSettings);
            //_epgCacheClient = new Mock<IEpgCacheClient>();
            _handler = new EpgNotificationHandler(_iotManager.Object, notificationSettingsCache.Object);
            
            _epgEvent = new EpgNotificationEvent
            {
                UpdatedRange = new Range<DateTime>(DateTime.UtcNow.AddSeconds(20), DateTime.UtcNow.AddHours(3)),
                LiveAssetId = 10,
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

        private static IEnumerable TestBackwardAndForwardTimeRangeForSendingNotificationsSource()
        {
            yield return new TestCaseData(6, 6, true, new EpgNotificationEvent
            {
                UpdatedRange = new Range<DateTime>(DateTime.UtcNow.AddHours(-7), DateTime.UtcNow.AddHours(3)),
                LiveAssetId = 10,
                DisableEpgNotification = false
            });
            
            yield return new TestCaseData(6, 6, true, new EpgNotificationEvent
            {
                UpdatedRange = new Range<DateTime>(DateTime.UtcNow, DateTime.UtcNow.AddHours(3)),
                LiveAssetId = 10,
                DisableEpgNotification = false
            });
            
            yield return new TestCaseData(6, 6, true, new EpgNotificationEvent
            {
                UpdatedRange = new Range<DateTime>(DateTime.UtcNow, DateTime.UtcNow.AddHours(10)),
                LiveAssetId = 10,
                DisableEpgNotification = false
            });
            
            yield return new TestCaseData(6, 6, false, new EpgNotificationEvent
            {
                UpdatedRange = new Range<DateTime>(DateTime.UtcNow.AddHours(7), DateTime.UtcNow.AddHours(8)),
                LiveAssetId = 10,
                DisableEpgNotification = false
            });
            
            yield return new TestCaseData(6, 6, false, new EpgNotificationEvent
            {
                UpdatedRange = new Range<DateTime>(DateTime.UtcNow.AddHours(-8), DateTime.UtcNow.AddHours(-7)),
                LiveAssetId = 10,
                DisableEpgNotification = false
            });
            
            yield return new TestCaseData(0, 0, false, new EpgNotificationEvent
            {
                UpdatedRange = new Range<DateTime>(DateTime.UtcNow.AddHours(-6.1), DateTime.UtcNow.AddHours(8)),
                LiveAssetId = 10,
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
            
            _iotManager.Verify(mock => mock.PublishIotMessage(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), times);
        }

        private static Mock<IIotManager> GetMockIotManager(int partitionsCount)
        {
            var mock = new Mock<IIotManager>();
            mock.Setup(m => m
                .GetTopicFormat(It.IsAny<int>(), It.IsAny<EventType>()))
                .Returns("MockTopic{0}");
            mock.Setup(m => m
               .GetTopicPartitionsCount())
               .Returns(partitionsCount);
            mock.Setup(m => m
               .PublishIotMessage(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
               .Returns(true);
            return mock;
        }

        private static Mock<INotificationCache> GetMockSettings(NotificationPartnerSettings settings)
        {
            var mock = new Mock<INotificationCache>();
            mock.Setup(m => m
                .GetPartnerNotificationSettings(It.IsAny<int>()))
                .Returns(new NotificationPartnerSettingsResponse()
                {
                    settings = settings,
                    Status = new Status(eResponseStatus.OK)
                });
            return mock;
        }
    }
}