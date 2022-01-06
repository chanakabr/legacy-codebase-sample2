using System.Collections.Generic;
using System.Linq;
using ApiLogic.Api.Managers;
using ApiLogic.Catalog;
using ApiLogic.Catalog.CatalogManagement.Services;
using ApiObjects;
using ApiObjects.Response;
using Core.Catalog;
using EventBus.Abstraction;
using FluentAssertions;
using Phx.Lib.Log;
using Moq;
using NUnit.Framework;

namespace ApiLogic.Tests.Catalog.CatalogManagement.Services
{
    [TestFixture]
    public class LineupServiceTests
    {
        private MockRepository _mockRepository;
        private Mock<IRegionManager> _regionManagerMock;
        private Mock<IAssetManager> _assetManagerMock;
        private Mock<IKLogger> _loggerMock;
        private Mock<IEventBusPublisher> _publisher;

        [SetUp]
        public void SetUp()
        {
            _mockRepository = new MockRepository(MockBehavior.Strict);
            _regionManagerMock = _mockRepository.Create<IRegionManager>();
            _assetManagerMock = _mockRepository.Create<IAssetManager>();
            _publisher = _mockRepository.Create<IEventBusPublisher>();
            _loggerMock = _mockRepository.Create<IKLogger>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockRepository.VerifyAll();
        }

        [Test]
        public void GetLineupChannelAssets_RegionDefinedAndGetRegionError_ReturnsExpectedResponse()
        {
            _regionManagerMock
                .Setup(x => x.GetRegion(10, 11))
                .Returns(new GenericResponse<Region>(new Status(2, "Custom Region Error")));
            _loggerMock
                .Setup(x => x.Error("GetRegion with parameters groupId:10, id:11 completed with status {2 - Custom Region Error}.", null, It.IsAny<string>()));
            var service = new LineupService(_regionManagerMock.Object, _assetManagerMock.Object, _loggerMock.Object, _publisher.Object);

            var result = service.GetLineupChannelAssets(10, 11, GetUserSearchContext(), 1, 3);

            result.Should().NotBeNull();
            result.Status.Should().Be(new Status(2, "Custom Region Error"));
            result.Objects.Should().BeNull();
        }

        [Test]
        public void GetLineupChannelAssets_RegionDefinedAndGetLinearChannelsError_ReturnsExpectedResponse()
        {
            _regionManagerMock
                .Setup(x => x.GetRegion(10, 11))
                .Returns(new GenericResponse<Region>(Status.Ok, GetFakeRegion()));
            _assetManagerMock
                .Setup(x => x.GetLinearChannels(
                    10,
                    It.Is<IEnumerable<long>>(_ => _.SequenceEqual(new List<long> { 106, 102, 107, 101, 108, 104, 103, 105 })),
                    It.Is<UserSearchContext>(_ => _.DomainId == 12 && _.UserId == 0 && _.LanguageId == 0 && _.Udid == null && _.UserIp == null && _.IgnoreEndDate && _.UseStartDate && _.UseFinal && _.GetOnlyActiveAssets && _.IsAllowedToViewInactiveAssets)))
                .Returns(new GenericListResponse<Asset>(new Status(3, "Custom Asset Error"), null));
            _loggerMock
                .Setup(x => x.Error("GetLinearChannels with parameters searchContext:{DomainId:12, UserId:0, LanguageId:0, Udid:, UserIp:, IgnoreEndDate:True, UseStartDate:True, UseFinal:True, GetOnlyActiveAssets:True, IsAllowedToViewInactiveAssets:True} completed with status {3 - Custom Asset Error}.", null, It.IsAny<string>()));
            var service = new LineupService(_regionManagerMock.Object, _assetManagerMock.Object, _loggerMock.Object, _publisher.Object);

            var result = service.GetLineupChannelAssets(10, 11, GetUserSearchContext(), 1, 3);

            result.Should().NotBeNull();
            result.Status.Should().Be(new Status(3, "Custom Asset Error"));
            result.Objects.Should().BeNull();
        }

        [Test]
        public void GetLineupChannelAssets_RegionDefinedAndValidParameters_ReturnsExpectedResponse()
        {
            _regionManagerMock
                .Setup(x => x.GetRegion(10, 11))
                .Returns(new GenericResponse<Region>(Status.Ok, GetFakeRegion()));
            _assetManagerMock
                .Setup(x => x.GetLinearChannels(
                    10,
                    It.Is<IEnumerable<long>>(_ => _.SequenceEqual(new List<long> { 106, 102, 107, 101, 108, 104, 103, 105 })),
                    It.Is<UserSearchContext>(_ => _.DomainId == 12 && _.UserId == 0 && _.LanguageId == 0 && _.Udid == null && _.UserIp == null && _.IgnoreEndDate && _.UseStartDate && _.UseFinal && _.GetOnlyActiveAssets && _.IsAllowedToViewInactiveAssets)))
                .Returns(new GenericListResponse<Asset>(Status.Ok, GetFakeAssets()));
            var service = new LineupService(_regionManagerMock.Object, _assetManagerMock.Object, _loggerMock.Object, _publisher.Object);

            var result = service.GetLineupChannelAssets(10, 11, GetUserSearchContext(), 1, 3);

            result.Should().NotBeNull();
            result.Status.Should().Be(Status.Ok);
            result.Objects.Should().NotBeEmpty();
            result.Objects[0].Id.Should().Be(101);
            result.Objects[0].LinearChannelNumber.Should().Be(4);
            result.Objects[1].Id.Should().Be(103);
            result.Objects[1].LinearChannelNumber.Should().Be(7);
            result.Objects[2].Id.Should().Be(101);
            result.Objects[2].LinearChannelNumber.Should().Be(8);
            result.TotalItems.Should().Be(8);
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void GetLineupChannelAssets_RegionNotDefinedAndDefaultRegionNotDefinedAndGetLinearChannelsError_ReturnsExpectedResponse(long regionId)
        {
            _regionManagerMock
                .Setup(x => x.GetDefaultRegionId(10))
                .Returns(new int?());
            _assetManagerMock
                .Setup(x => x.GetLinearChannels(
                    10,
                    It.Is<IEnumerable<long>>(_ => !_.Any()),
                    It.Is<UserSearchContext>(_ => _.DomainId == 12 && _.UserId == 0 && _.LanguageId == 0 && _.Udid == null && _.UserIp == null && _.IgnoreEndDate && _.UseStartDate && _.UseFinal && _.GetOnlyActiveAssets && _.IsAllowedToViewInactiveAssets)))
                .Returns(new GenericListResponse<Asset>(new Status(3, "Custom Asset Error"), null));
            _loggerMock
                .Setup(x => x.Error("GetLinearChannels with parameters searchContext:{DomainId:12, UserId:0, LanguageId:0, Udid:, UserIp:, IgnoreEndDate:True, UseStartDate:True, UseFinal:True, GetOnlyActiveAssets:True, IsAllowedToViewInactiveAssets:True} completed with status {3 - Custom Asset Error}.", null, It.IsAny<string>()));
            var service = new LineupService(_regionManagerMock.Object, _assetManagerMock.Object, _loggerMock.Object, _publisher.Object);

            var result = service.GetLineupChannelAssets(10, regionId, GetUserSearchContext(), 1, 3);

            result.Should().NotBeNull();
            result.Status.Should().Be(new Status(3, "Custom Asset Error"));
            result.Objects.Should().BeNull();
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void GetLineupChannelAssets_RegionNotDefinedAndDefaultRegionNotDefinedAndValidParameters_ReturnsExpectedResponse(long regionId)
        {
            _regionManagerMock
                .Setup(x => x.GetDefaultRegionId(10))
                .Returns(new int?());
            _assetManagerMock
                .Setup(x => x.GetLinearChannels(
                    10,
                    It.Is<IEnumerable<long>>(_ => !_.Any()),
                    It.Is<UserSearchContext>(_ => _.DomainId == 12 && _.UserId == 0 && _.LanguageId == 0 && _.Udid == null && _.UserIp == null && _.IgnoreEndDate && _.UseStartDate && _.UseFinal && _.GetOnlyActiveAssets && _.IsAllowedToViewInactiveAssets)))
                .Returns(new GenericListResponse<Asset>(Status.Ok, GetFakeAssets()));
            var service = new LineupService(_regionManagerMock.Object, _assetManagerMock.Object, _loggerMock.Object, _publisher.Object);

            var result = service.GetLineupChannelAssets(10, regionId, GetUserSearchContext(), 1, 3);

            result.Should().NotBeNull();
            result.Status.Should().Be(Status.Ok);
            result.Objects.Should().NotBeEmpty();
            result.Objects[0].Id.Should().Be(104);
            result.Objects[0].LinearChannelNumber.Should().BeNull();
            result.Objects[1].Id.Should().Be(106);
            result.Objects[1].LinearChannelNumber.Should().BeNull();
            result.Objects[2].Id.Should().Be(108);
            result.Objects[2].LinearChannelNumber.Should().BeNull();
            result.TotalItems.Should().Be(6);
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void GetLineupChannelAssets_RegionNotDefinedAndDefaultRegionDefinedAndValidParameters_ReturnsExpectedResponse(long regionId)
        {
            _regionManagerMock
                .Setup(x => x.GetDefaultRegionId(10))
                .Returns(11);
            _regionManagerMock
                .Setup(x => x.GetRegion(10, 11))
                .Returns(new GenericResponse<Region>(Status.Ok, GetFakeRegion()));
            _assetManagerMock
                .Setup(x => x.GetLinearChannels(
                    10,
                    It.Is<IEnumerable<long>>(_ => _.SequenceEqual(new List<long> { 106, 102, 107, 101, 108, 104, 103, 105 })),
                    It.Is<UserSearchContext>(_ => _.DomainId == 12 && _.UserId == 0 && _.LanguageId == 0 && _.Udid == null && _.UserIp == null && _.IgnoreEndDate && _.UseStartDate && _.UseFinal && _.GetOnlyActiveAssets && _.IsAllowedToViewInactiveAssets)))
                .Returns(new GenericListResponse<Asset>(Status.Ok, GetFakeAssets()));
            var service = new LineupService(_regionManagerMock.Object, _assetManagerMock.Object, _loggerMock.Object, _publisher.Object);

            var result = service.GetLineupChannelAssets(10, regionId, GetUserSearchContext(), 1, 3);

            result.Should().NotBeNull();
            result.Status.Should().Be(Status.Ok);
            result.Objects.Should().NotBeEmpty();
            result.Objects[0].Id.Should().Be(101);
            result.Objects[0].LinearChannelNumber.Should().Be(4);
            result.Objects[1].Id.Should().Be(103);
            result.Objects[1].LinearChannelNumber.Should().Be(7);
            result.Objects[2].Id.Should().Be(101);
            result.Objects[2].LinearChannelNumber.Should().Be(8);
            result.TotalItems.Should().Be(8);
        }

        private UserSearchContext GetUserSearchContext()
        {
            return new UserSearchContext(12, 13, 14, "udid", "ip", true, true, true, true, true);
        }

        private Region GetFakeRegion()
        {
            return new Region
            {
                linearChannels = new List<KeyValuePair<long, int>>
                {
                    new KeyValuePair<long, int>(106, 3),
                    new KeyValuePair<long, int>(102, 1),
                    new KeyValuePair<long, int>(107, 6),
                    new KeyValuePair<long, int>(101, 4),
                    new KeyValuePair<long, int>(108, 9),
                    new KeyValuePair<long, int>(104, 2),
                    new KeyValuePair<long, int>(103, 7),
                    new KeyValuePair<long, int>(101, 8),
                    new KeyValuePair<long, int>(105, 5),
                    new KeyValuePair<long, int>(102, 10)
                }
            };
        }

        private List<Asset> GetFakeAssets()
        {
            return new List<Asset>
            {
                new LiveAsset { Id = 106 },
                new LiveAsset { Id = 103 },
                new LiveAsset { Id = 108 },
                new LiveAsset { Id = 104 },
                new LiveAsset { Id = 101 },
                new Asset { Id = 107 },
                new LiveAsset { Id = 102 }
            };
        }
    }
}