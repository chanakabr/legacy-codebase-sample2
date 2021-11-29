using System.Collections.Generic;
using ApiLogic.Api.Managers;
using ApiLogic.Notification;
using ApiObjects;
using ApiObjects.Base;
using ApiObjects.Notification;
using CachingProvider.LayeredCache;
using Core.Catalog.CatalogManagement;
using Core.Domains;
using Core.Notification;
using Core.Users;
using DAL;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Status = ApiObjects.Response.Status;

namespace ApiLogic.Tests.IoTManagers
{
    public class IoTManagerTests
    {
        private const int GROUP_ID_NO_REGIONALIZATION = 1;
        private const int GROUP_ID_NO_DEFAULT_REGION = 2;
        private const int GROUP_ID_WITH_DEFAULT_REGION = 3;
        private const long DEFAULT_REGION_ID = 1;
        private const int ALLOWED_DEVICE_FAMILY_ID = 1;
        private const int NOT_ALLOWED_DEVICE_FAMILY_ID = 2;
        private const string ALLOWED_DEVICE_UDID = "22222222";
        private const string NOT_ALLOWED_DEVICE_UDID = "11111111";
        private const string DEVICE_UDID_NO_FAMILY_ID = "33333333";

        private static readonly IReadOnlyDictionary<string, DeviceResponseObject> UdidDevice =
            new Dictionary<string, DeviceResponseObject>
            {
                {
                    NOT_ALLOWED_DEVICE_UDID, new DeviceResponseObject
                    {
                        m_oDevice = new Device { m_deviceFamilyID = NOT_ALLOWED_DEVICE_FAMILY_ID }
                    }
                },
                {
                    DEVICE_UDID_NO_FAMILY_ID, new DeviceResponseObject
                    {
                        m_oDevice = new Device()
                    }
                },
                {
                    ALLOWED_DEVICE_UDID, new DeviceResponseObject
                    {
                        m_oDevice = new Device { m_deviceFamilyID = ALLOWED_DEVICE_FAMILY_ID }
                    }
                }
            };

        private static readonly IReadOnlyDictionary<int, long?> GroupIdDefaultRegionId = new Dictionary<int, long?>
            {
                { GROUP_ID_NO_REGIONALIZATION, null },
                { GROUP_ID_NO_DEFAULT_REGION, null },
                { GROUP_ID_WITH_DEFAULT_REGION, DEFAULT_REGION_ID }
            };

        private static readonly HashSet<int> GroupIdWithRegionalizationEnabled = new HashSet<int>
        {
            GROUP_ID_NO_DEFAULT_REGION,
            GROUP_ID_WITH_DEFAULT_REGION
        };

        private static readonly HashSet<int> GroupsWithRegionalizationEnabled = new HashSet<int>
            {
                GROUP_ID_NO_DEFAULT_REGION,
                GROUP_ID_WITH_DEFAULT_REGION
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

        [TestCaseSource(nameof(IoTDisabledClientConfigurationTestData))]
        public void GetClientConfiguration_IotDisabled_ReturnsError(NotificationPartnerSettingsResponse response)
        {
            var contextData = new ContextData(GROUP_ID_NO_REGIONALIZATION);
            var iotManager = new IotManager(
                _mockRepository.Create<ICatalogManager>().Object,
                _mockRepository.Create<IRegionManager>().Object,
                GetNotificationCacheMock(GROUP_ID_NO_REGIONALIZATION, response).Object,
                _mockRepository.Create<INotificationDal>().Object,
                _mockRepository.Create<ILayeredCache>().Object,
                _mockRepository.Create<IDomainModule>().Object);

            var result = iotManager.GetClientConfiguration(contextData);

            result.Should().NotBeNull();
            result.Status.Should().Be(Status.Error);
        }

        [Test]
        public void GetClientConfiguration_EpgAndLineupNotificationsDisabled_EmptyTopics()
        {
            var contextData = new ContextData(GROUP_ID_NO_REGIONALIZATION) { Udid = ALLOWED_DEVICE_UDID };
            var response = new NotificationPartnerSettingsResponse
            {
                settings = new NotificationPartnerSettings
                {
                    IsIotEnabled = true,
                    LineupNotification = new LineupNotificationSettings { Enabled = false },
                    EpgNotification = new EpgNotificationSettings { Enabled = false }
                }
            };

            var iotManager = new IotManager(
                _mockRepository.Create<ICatalogManager>().Object,
                _mockRepository.Create<IRegionManager>().Object,
                GetNotificationCacheMock(GROUP_ID_NO_REGIONALIZATION, response).Object,
                _mockRepository.Create<INotificationDal>().Object,
                _mockRepository.Create<ILayeredCache>().Object,
                _mockRepository.Create<IDomainModule>().Object);

            var result = iotManager.GetClientConfiguration(contextData);

            result.Should().NotBeNull();
            result.Status.Should().Be(Status.Ok);
            result.Object.Should().NotBeNull();
            result.Object.Topics.Should().BeEmpty();
        }

        [TestCase(GROUP_ID_NO_REGIONALIZATION, NOT_ALLOWED_DEVICE_UDID)]
        [TestCase(GROUP_ID_NO_DEFAULT_REGION, NOT_ALLOWED_DEVICE_UDID)]
        [TestCase(GROUP_ID_WITH_DEFAULT_REGION, NOT_ALLOWED_DEVICE_UDID)]
        public void GetClientConfiguration_EpgDeviceFamilyNotAllowed_EmptyTopics(int groupId, string udid)
        {
            var contextData = new ContextData(groupId) { Udid = udid };
            var settingsResponse = new NotificationPartnerSettingsResponse
            {
                settings = new NotificationPartnerSettings
                {
                    IsIotEnabled = true,
                    LineupNotification = new LineupNotificationSettings { Enabled = false },
                    EpgNotification = new EpgNotificationSettings
                    {
                        Enabled = true,
                        DeviceFamilyIds = new List<int> { ALLOWED_DEVICE_FAMILY_ID }
                    }
                }
            };

            var iotManager = new IotManager(
                _mockRepository.Create<ICatalogManager>().Object,
                _mockRepository.Create<IRegionManager>().Object,
                GetNotificationCacheMock(groupId, settingsResponse).Object,
                _mockRepository.Create<INotificationDal>().Object,
                GetLayeredCacheMock().Object,
                GetDomainModuleMock(NOT_ALLOWED_DEVICE_UDID, UdidDevice[NOT_ALLOWED_DEVICE_UDID]).Object);

            var result = iotManager.GetClientConfiguration(contextData);

            result.Should().NotBeNull();
            result.Status.Should().Be(Status.Ok);
            result.Object.Should().NotBeNull();
            result.Object.Topics.Should().BeEmpty();
        }

        [TestCaseSource(nameof(EpgIotEnabledTestData))]
        public void GetClientConfiguration_EpgIotEnabledRegionalizationDisabled_NotEmptyTopics(
            ContextData contextData,
            List<int> allowedDeviceFamilyIds,
            List<string> topics)
        {
            var settingsResponse = new NotificationPartnerSettingsResponse
            {
                settings = new NotificationPartnerSettings
                {
                    IsIotEnabled = true,
                    LineupNotification = new LineupNotificationSettings { Enabled = false },
                    EpgNotification = new EpgNotificationSettings
                    {
                        Enabled = true,
                        DeviceFamilyIds = allowedDeviceFamilyIds
                    }
                }
            };

            var iotManager = new IotManager(
                GetCatalogManagerMock(contextData.GroupId, GroupIdWithRegionalizationEnabled.Contains(contextData.GroupId)).Object,
                _mockRepository.Create<IRegionManager>().Object,
                GetNotificationCacheMock(contextData.GroupId, settingsResponse).Object,
                _mockRepository.Create<INotificationDal>().Object,
                GetLayeredCacheMock().Object,
                GetDomainModuleMock(contextData.Udid, UdidDevice[contextData.Udid]).Object);

            var result = iotManager.GetClientConfiguration(contextData);

            result.Should().NotBeNull();
            result.Status.Should().Be(Status.Ok);
            result.Object.Should().NotBeNull();
            result.Object.Topics.Should().BeEquivalentTo(topics);
        }

        [TestCaseSource(nameof(EpgIotDefaultRegionIdTestData))]
        public void GetClientConfiguration_EpgIotEnabledRegionalizationEnabled_NotEmptyTopics(
            ContextData contextData,
            List<int> allowedDeviceFamilyIds,
            List<string> topics)
        {
            var settingsResponse = new NotificationPartnerSettingsResponse
            {
                settings = new NotificationPartnerSettings
                {
                    IsIotEnabled = true,
                    LineupNotification = new LineupNotificationSettings { Enabled = false },
                    EpgNotification = new EpgNotificationSettings
                    {
                        Enabled = true,
                        DeviceFamilyIds = allowedDeviceFamilyIds
                    }
                }
            };

            var iotManager = new IotManager(
                GetCatalogManagerMock(contextData.GroupId, GroupIdWithRegionalizationEnabled.Contains(contextData.GroupId)).Object,
                GetRegionManagerMock(contextData.GroupId, GroupIdDefaultRegionId[contextData.GroupId]).Object,
                GetNotificationCacheMock(contextData.GroupId, settingsResponse).Object,
                _mockRepository.Create<INotificationDal>().Object,
                GetLayeredCacheMock().Object,
                GetDomainModuleMock(contextData.Udid, UdidDevice[contextData.Udid]).Object);

            var result = iotManager.GetClientConfiguration(contextData);

            result.Should().NotBeNull();
            result.Status.Should().Be(Status.Ok);
            result.Object.Should().NotBeNull();
            result.Object.Topics.Should().BeEquivalentTo(topics);
        }

        [TestCaseSource(nameof(LineupIotDefaultRegionIdTestData))]
        public void GetClientConfiguration_LineupIotEnabledRegionalizationEnabled_NotEmptyTopics(
            ContextData contextData,
            List<int> allowedDeviceFamilyIds,
            List<string> topics)
        {
            var settingsResponse = new NotificationPartnerSettingsResponse
            {
                settings = new NotificationPartnerSettings
                {
                    IsIotEnabled = true,
                    LineupNotification = new LineupNotificationSettings { Enabled = true },
                    EpgNotification = new EpgNotificationSettings { Enabled = false }
                }
            };

            var iotManager = new IotManager(
                GetCatalogManagerMock(contextData.GroupId, GroupIdWithRegionalizationEnabled.Contains(contextData.GroupId)).Object,
                GetRegionManagerMock(contextData.GroupId, GroupIdDefaultRegionId[contextData.GroupId]).Object,
                GetNotificationCacheMock(contextData.GroupId, settingsResponse).Object,
                _mockRepository.Create<INotificationDal>().Object,
                GetLayeredCacheMock().Object,
                GetDomainModuleMock(contextData.Udid, UdidDevice[contextData.Udid]).Object);

            var result = iotManager.GetClientConfiguration(contextData);

            result.Should().NotBeNull();
            result.Status.Should().Be(Status.Ok);
            result.Object.Should().NotBeNull();
            result.Object.Topics.Should().BeEquivalentTo(topics);
        }

        [Test]
        public void GetClientConfiguration_LineupNotificationsEnabledNoRegionalization_EmptyTopics()
        {
            var contextData = new ContextData(GROUP_ID_NO_REGIONALIZATION) { Udid = ALLOWED_DEVICE_UDID };
            var settingsResponse = new NotificationPartnerSettingsResponse
            {
                settings = new NotificationPartnerSettings
                {
                    IsIotEnabled = true,
                    LineupNotification = new LineupNotificationSettings { Enabled = true },
                    EpgNotification = new EpgNotificationSettings { Enabled = false }
                }
            };

            var iotManager = new IotManager(
                GetCatalogManagerMock(contextData.GroupId, GroupIdWithRegionalizationEnabled.Contains(contextData.GroupId)).Object,
                _mockRepository.Create<IRegionManager>().Object,
                GetNotificationCacheMock(contextData.GroupId, settingsResponse).Object,
                _mockRepository.Create<INotificationDal>().Object,
                _mockRepository.Create<ILayeredCache>().Object,
                _mockRepository.Create<IDomainModule>().Object);

            var result = iotManager.GetClientConfiguration(contextData);

            result.Should().NotBeNull();
            result.Status.Should().Be(Status.Ok);
            result.Object.Should().NotBeNull();
            result.Object.Topics.Should().BeEmpty();
        }

        [TestCase(GROUP_ID_WITH_DEFAULT_REGION)]
        [TestCase(GROUP_ID_NO_DEFAULT_REGION)]
        public void GetClientConfiguration_LineupNotificationsEnabledNotEmptyRegion_NotEmptyTopics(int groupId)
        {
            var contextData = new ContextData(groupId) { Udid = ALLOWED_DEVICE_UDID, RegionId = 3 };
            var expectedTopics = new List<string> { $"{groupId}/lineup_updated/3/4" };
            var settingsResponse = new NotificationPartnerSettingsResponse
            {
                settings = new NotificationPartnerSettings
                {
                    IsIotEnabled = true,
                    LineupNotification = new LineupNotificationSettings { Enabled = true },
                    EpgNotification = new EpgNotificationSettings { Enabled = false }
                }
            };

            var iotManager = new IotManager(
                GetCatalogManagerMock(groupId, GroupIdWithRegionalizationEnabled.Contains(groupId)).Object,
                _mockRepository.Create<IRegionManager>().Object,
                GetNotificationCacheMock(groupId, settingsResponse).Object,
                _mockRepository.Create<INotificationDal>().Object,
                _mockRepository.Create<ILayeredCache>().Object,
                _mockRepository.Create<IDomainModule>().Object);

            var result = iotManager.GetClientConfiguration(contextData);

            result.Should().NotBeNull();
            result.Status.Should().Be(Status.Ok);
            result.Object.Should().NotBeNull();
            result.Object.Topics.Should().BeEquivalentTo(expectedTopics);
        }

        [Test]
        public void GetClientConfiguration_EpgAndLineupNotificationsEnabled_NotEmptyTopics()
        {
            var contextData = new ContextData(GROUP_ID_WITH_DEFAULT_REGION) { Udid = ALLOWED_DEVICE_UDID, RegionId = 1 };
            var expectedTopics = new List<string>
            {
                $"{contextData.GroupId}/epg_update/1/4",
                $"{contextData.GroupId}/lineup_updated/1/4"
            };

            var settingsResponse = new NotificationPartnerSettingsResponse
            {
                settings = new NotificationPartnerSettings
                {
                    IsIotEnabled = true,
                    LineupNotification = new LineupNotificationSettings { Enabled = true },
                    EpgNotification = new EpgNotificationSettings { Enabled = true, DeviceFamilyIds = new List<int>() }
                }
            };

            var iotManager = new IotManager(
                GetCatalogManagerMock(contextData.GroupId, GroupIdWithRegionalizationEnabled.Contains(contextData.GroupId)).Object,
                _mockRepository.Create<IRegionManager>().Object,
                GetNotificationCacheMock(contextData.GroupId, settingsResponse).Object,
                _mockRepository.Create<INotificationDal>().Object,
                GetLayeredCacheMock().Object,
                GetDomainModuleMock(contextData.Udid, UdidDevice[contextData.Udid]).Object);

            var result = iotManager.GetClientConfiguration(contextData);

            result.Should().NotBeNull();
            result.Status.Should().Be(Status.Ok);
            result.Object.Should().NotBeNull();
            result.Object.Topics.Should().BeEquivalentTo(expectedTopics);
        }

        private static IEnumerable<TestCaseData> IoTDisabledClientConfigurationTestData()
        {
            yield return new TestCaseData(null);
            yield return new TestCaseData(new NotificationPartnerSettingsResponse());
            yield return new TestCaseData(new NotificationPartnerSettingsResponse
            {
                settings = new NotificationPartnerSettings { IsIotEnabled = false }
            });
        }

        private static IEnumerable<TestCaseData> EpgIotEnabledTestData()
        {
            yield return new TestCaseData(
                new ContextData(GROUP_ID_NO_REGIONALIZATION) { Udid = ALLOWED_DEVICE_UDID },
                new List<int>(),
                new List<string> { "1/epg_update/4" });
            yield return new TestCaseData(
                new ContextData(GROUP_ID_NO_REGIONALIZATION) { Udid = ALLOWED_DEVICE_UDID },
                new List<int> { ALLOWED_DEVICE_FAMILY_ID },
                new List<string> { "1/epg_update/4" });
            yield return new TestCaseData(
                new ContextData(GROUP_ID_NO_REGIONALIZATION) { Udid = DEVICE_UDID_NO_FAMILY_ID },
                new List<int>(),
                new List<string> { "1/epg_update/2" });
            yield return new TestCaseData(
                new ContextData(GROUP_ID_WITH_DEFAULT_REGION) { Udid = NOT_ALLOWED_DEVICE_UDID, RegionId = 2 },
                new List<int>(),
                new List<string> { "3/epg_update/2/7" });
        }

        private static IEnumerable<TestCaseData> EpgIotDefaultRegionIdTestData()
        {
            yield return new TestCaseData(
                new ContextData(GROUP_ID_NO_DEFAULT_REGION) { Udid = ALLOWED_DEVICE_UDID },
                new List<int>(),
                new List<string>());
            yield return new TestCaseData(
                new ContextData(GROUP_ID_WITH_DEFAULT_REGION) { Udid = ALLOWED_DEVICE_UDID },
                new List<int>(),
                new List<string> { "3/epg_update/1/4" });
        }

        private static IEnumerable<TestCaseData> LineupIotDefaultRegionIdTestData()
        {
            yield return new TestCaseData(
                new ContextData(GROUP_ID_NO_DEFAULT_REGION) { Udid = ALLOWED_DEVICE_UDID },
                new List<int>(),
                new List<string>());
            yield return new TestCaseData(
                new ContextData(GROUP_ID_WITH_DEFAULT_REGION) { Udid = ALLOWED_DEVICE_UDID },
                new List<int>(),
                new List<string> { "3/lineup_updated/1/4" });
        }

        private static Mock<IDomainModule> GetDomainModuleMock(string udid, DeviceResponseObject device)
        {
            var mock = new Mock<IDomainModule>();
            mock
                .Setup(x => x.GetDeviceInfo(
                    It.Is<int>(id => id == GROUP_ID_NO_REGIONALIZATION || id == GROUP_ID_NO_DEFAULT_REGION || id == GROUP_ID_WITH_DEFAULT_REGION),
                    udid,
                    true))
                .Returns(device);

            return mock;
        }

        private Mock<INotificationCache> GetNotificationCacheMock(int groupId, NotificationPartnerSettingsResponse response)
        {
            var mock = _mockRepository.Create<INotificationCache>();

            mock.Setup(x => x.GetPartnerNotificationSettings(groupId)).Returns(response);

            return mock;
        }

        private static Mock<ILayeredCache> GetLayeredCacheMock()
        {
            var mock = new Mock<ILayeredCache>();
            mock.Setup(new IotClientConfiguration(), true, false);

            return mock;
        }

        private Mock<ICatalogManager> GetCatalogManagerMock(int groupId, bool isRegionalizationEnabled)
        {
            var mock = _mockRepository.Create<ICatalogManager>();
            mock.Setup(x => x.IsRegionalizationEnabled(groupId)).Returns(isRegionalizationEnabled);

            return mock;
        }

        private Mock<IRegionManager> GetRegionManagerMock(int groupId, long? defaultRegionId)
        {
            var mock = _mockRepository.Create<IRegionManager>();
            mock.Setup(x => x.GetDefaultRegionId(groupId)).Returns(defaultRegionId);

            return mock;
        }
    }
}