using NUnit.Framework;
using ApiObjects.Notification;
using ApiObjects.EventBus;
using System;
using System.Collections.Generic;
using ApiLogic.Notification;
using Moq;
using System.Threading.Tasks;
using Core.Notification;

namespace EpgNotificationHandler.Tests
{
    public class Tests
    {
        private EpgNotificationHandler _handler;
        private static NotificationPartnerSettingsResponse _settings;
        private static Mock<IIotManager> _mockManager;
        private static Mock<INotificationCache> _mockSettings;

        [SetUp]
        public void Setup()
        {
            _settings = new NotificationPartnerSettingsResponse() { settings = new NotificationPartnerSettings() };
            _settings.settings.IsIotEnabled = true;
            _settings.settings.EpgNotification = new EpgNotificationSettings
            {
                Enabled = true,
                TimeRange = 2,
                DeviceFamilyIds = new List<int>() { 1, 2, 3, 4, 5 },
                LiveAssetIds = new List<long>() { 10, 20, 30, 40 },
            };
            _mockManager = GetMockIotManager(1);
            _mockSettings = GetMockSettings();
            _handler = new EpgNotificationHandler(_mockManager.Object, _mockSettings.Object);
        }

        #region Tests
        [Test]
        public async Task TestHandle()
        {
            await _handler.Handle(GetEpgNotificationEvent());
            _mockManager.Verify(mock => mock.PublishIotMessage(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Test]
        public async Task TestHandleWrongDate()
        {
            await CheckValidation(ValidationType.DateRange);
        }

        [Test]
        public async Task TestHandleWrongSettings()
        {
            await CheckValidation(ValidationType.Settings);
        }

        [Test]
        public async Task TestHandleWrongLiveAssetId()
        {
            await CheckValidation(ValidationType.LiveAssetIds);
        }
        #endregion

        #region Moq
        private Mock<IIotManager> GetMockIotManager(int value)
        {
            var mock = new Mock<IIotManager>();
            mock.Setup(m => m
                .GetTopicFormat(It.IsAny<int>(), It.IsAny<EventType>()))
                .Returns("MockTopic{0}");
            mock.Setup(m => m
               .GetTopicPartitionsCount())
               .Returns(value);
            mock.Setup(m => m
               .PublishIotMessage(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
               .Returns(true);
            return mock;
        }

        private Mock<INotificationCache> GetMockSettings()
        {
            var mock = new Mock<INotificationCache>();
            mock.Setup(m => m
                .GetPartnerNotificationSettings(It.IsAny<int>()))
                .Returns(new NotificationPartnerSettingsResponse()
                {
                    settings = _settings.settings,
                    Status = new ApiObjects.Response.Status(ApiObjects.Response.eResponseStatus.OK)
                });
            return mock;
        }
        #endregion

        private async Task CheckValidation(ValidationType validation)
        {
            await _handler.Handle(GetEpgNotificationEvent(validation));
            _mockManager.Verify(mock => mock.PublishIotMessage(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        private static EpgNotificationEvent GetEpgNotificationEvent(ValidationType? type = null)
        {
            var response = new EpgNotificationEvent() { UpdatedRange = new Range<DateTime>(DateTime.UtcNow.AddSeconds(12), DateTime.UtcNow.AddHours(3)), LiveAssetId = 10 };
            if (!type.HasValue)
            {
                return response;
            }

            switch (type)
            {
                case ValidationType.Settings:
                    _settings.settings.IsIotEnabled = false;
                    break;
                case ValidationType.DateRange:
                    response.UpdatedRange = new Range<DateTime>(DateTime.UtcNow.AddHours(12), DateTime.UtcNow.AddHours(3));
                    break;
                case ValidationType.LiveAssetIds:
                    response.LiveAssetId = 11;
                    break;
                default:
                    break;
            }
            return response;
        }

        private enum ValidationType
        {
            Settings,
            DateRange,
            LiveAssetIds
        }
    }
}