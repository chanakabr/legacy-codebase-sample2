using System.Collections.Generic;
using System.Linq;
using ApiLogic.Api.Managers;
using ApiLogic.Catalog;
using ApiLogic.Catalog.CatalogManagement.Services;
using ApiObjects;
using ApiObjects.Catalog;
using ApiObjects.Response;
using Core.Catalog;
using Core.Catalog.CatalogManagement;
using Core.Catalog.Response;
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
        private Mock<ICatalogManager> _catalogManagerMock;
        private Mock<IRegionManager> _regionManagerMock;
        private Mock<IAssetManager> _assetManagerMock;
        private Mock<ISearchProvider> _searchProviderMock;
        private Mock<IEventBusPublisher> _publisher;
        private Mock<IKLogger> _loggerMock;

        [SetUp]
        public void SetUp()
        {
            _mockRepository = new MockRepository(MockBehavior.Strict);
            _catalogManagerMock = _mockRepository.Create<ICatalogManager>();
            _regionManagerMock = _mockRepository.Create<IRegionManager>();
            _assetManagerMock = _mockRepository.Create<IAssetManager>();
            _searchProviderMock = _mockRepository.Create<ISearchProvider>();
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
            var service = new LineupService(_catalogManagerMock.Object, _regionManagerMock.Object, _assetManagerMock.Object, _searchProviderMock.Object, _publisher.Object, _loggerMock.Object);

            var result = service.GetLineupChannelAssets(10, 11, GetUserSearchContext(), 1, 3);

            result.Should().NotBeNull();
            result.Status.Should().Be(new Status(2, "Custom Region Error"));
            result.Objects.Should().BeNull();
        }

        [Test]
        public void GetLineupChannelAssets_RegionDefinedAndLinearMediaTypesNotFound_ReturnsExpectedResponse()
        {
            var searchContext = GetUserSearchContext();
            _regionManagerMock
                .Setup(x => x.GetRegion(10, 11))
                .Returns(new GenericResponse<Region>(Status.Ok, GetFakeRegion()));
            _catalogManagerMock
                .Setup(x => x.GetLinearMediaTypes(10))
                .Returns(new List<AssetStruct>());
            _loggerMock
                .Setup(x => x.Error("Linear asset structs were not found. groupId:10.", null, It.IsAny<string>()));
            var service = new LineupService(_catalogManagerMock.Object, _regionManagerMock.Object, _assetManagerMock.Object, _searchProviderMock.Object, _publisher.Object, _loggerMock.Object);

            var result = service.GetLineupChannelAssets(10, 11, searchContext, 1, 3);

            result.Should().NotBeNull();
            result.Status.Should().Be(Status.Error);
            result.Objects.Should().BeNull();
        }

        [Test]
        public void GetLineupChannelAssets_RegionDefinedAndSearchAssetsError_ReturnsExpectedResponse()
        {
            var searchContext = GetUserSearchContext();
            _regionManagerMock
                .Setup(x => x.GetRegion(10, 11))
                .Returns(new GenericResponse<Region>(Status.Ok, GetFakeRegion()));
            _catalogManagerMock
                .Setup(x => x.GetLinearMediaTypes(10))
                .Returns(new List<AssetStruct> { new AssetStruct { Id = 1001 }, new AssetStruct { Id = 1002 } });
            _searchProviderMock
                .Setup(x => x.SearchAssets(10, searchContext, "(and (or asset_type='1001' asset_type='1002') (or media_id:'106,102,107,101,108,104,103,105'))"))
                .Returns(new UnifiedSearchResponse { status = new Status(3, "Custom Search Error") });
            var service = new LineupService(_catalogManagerMock.Object, _regionManagerMock.Object, _assetManagerMock.Object, _searchProviderMock.Object, _publisher.Object, _loggerMock.Object);

            var result = service.GetLineupChannelAssets(10, 11, searchContext, 1, 3);

            result.Should().NotBeNull();
            result.Status.Should().Be(new Status(3, "Custom Search Error"));
            result.Objects.Should().BeNull();
        }

        [Test]
        public void GetLineupChannelAssets_RegionDefinedAndValidParameters_ReturnsExpectedResponse()
        {
            var searchContext = GetUserSearchContext();
            _regionManagerMock
                .Setup(x => x.GetRegion(10, 11))
                .Returns(new GenericResponse<Region>(Status.Ok, GetFakeRegion()));
            _catalogManagerMock
                .Setup(x => x.GetLinearMediaTypes(10))
                .Returns(new List<AssetStruct> { new AssetStruct { Id = 1001 }, new AssetStruct { Id = 1002 } });
            _searchProviderMock
                .Setup(x => x.SearchAssets(10, searchContext, "(and (or asset_type='1001' asset_type='1002') (or media_id:'106,102,107,101,108,104,103,105'))"))
                .Returns(new UnifiedSearchResponse { status = Status.Ok, searchResults = FakeSearchResultsByRegion(), m_nTotalItems = 8 });
            _assetManagerMock
                .Setup(x => x.GetAssets(
                    10,
                    It.Is<IEnumerable<KeyValuePair<eAssetTypes, long>>>(_ => _.SequenceEqual(new[] { new KeyValuePair<eAssetTypes, long>(eAssetTypes.MEDIA, 101), new KeyValuePair<eAssetTypes, long>(eAssetTypes.MEDIA, 103) })),
                    searchContext.IsAllowedToViewInactiveAssets))
                .Returns(GetFakeAssets());
            var service = new LineupService(_catalogManagerMock.Object, _regionManagerMock.Object, _assetManagerMock.Object, _searchProviderMock.Object, _publisher.Object, _loggerMock.Object);

            var result = service.GetLineupChannelAssets(10, 11, searchContext, 1, 3);

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
        public void GetLineupChannelAssets_RegionNotDefinedAndLinearMediaTypesNotFound_ReturnsExpectedResponse(long regionId)
        {
            var searchContext = GetUserSearchContext();
            _regionManagerMock
                .Setup(x => x.GetDefaultRegionId(10))
                .Returns(new int?());
            _catalogManagerMock
                .Setup(x => x.GetLinearMediaTypes(10))
                .Returns(new List<AssetStruct>());
            _loggerMock
                .Setup(x => x.Error("Linear asset structs were not found. groupId:10.", null, It.IsAny<string>()));
            var service = new LineupService(_catalogManagerMock.Object, _regionManagerMock.Object, _assetManagerMock.Object, _searchProviderMock.Object, _publisher.Object, _loggerMock.Object);

            var result = service.GetLineupChannelAssets(10, regionId, searchContext, 1, 3);

            result.Should().NotBeNull();
            result.Status.Should().Be(Status.Error);
            result.Objects.Should().BeNull();
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void GetLineupChannelAssets_RegionNotDefinedAndSearchAssetsError_ReturnsExpectedResponse(int regionId)
        {
            var searchContext = GetUserSearchContext();
            _regionManagerMock
                .Setup(x => x.GetDefaultRegionId(10))
                .Returns(new int?());
            _catalogManagerMock
                .Setup(x => x.GetLinearMediaTypes(10))
                .Returns(new List<AssetStruct> { new AssetStruct { Id = 1001 }, new AssetStruct { Id = 1002 } });
            _searchProviderMock
                .Setup(x => x.SearchAssets(10, searchContext, "(and (or asset_type='1001' asset_type='1002'))", 1, 3))
                .Returns(new UnifiedSearchResponse { status = new Status(3, "Custom Search Error") });
            var service = new LineupService(_catalogManagerMock.Object, _regionManagerMock.Object, _assetManagerMock.Object, _searchProviderMock.Object, _publisher.Object, _loggerMock.Object);

            var result = service.GetLineupChannelAssets(10, regionId, searchContext, 1, 3);

            result.Should().NotBeNull();
            result.Status.Should().Be(new Status(3, "Custom Search Error"));
            result.Objects.Should().BeNull();
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void GetLineupChannelAssets_RegionNotDefinedAndValidParameters_ReturnsExpectedResponse(int regionId)
        {
            var searchContext = GetUserSearchContext();
            _regionManagerMock
                .Setup(x => x.GetDefaultRegionId(10))
                .Returns(new int?());
            _catalogManagerMock
                .Setup(x => x.GetLinearMediaTypes(10))
                .Returns(new List<AssetStruct> { new AssetStruct { Id = 1001 }, new AssetStruct { Id = 1002 } });
            _searchProviderMock
                .Setup(x => x.SearchAssets(10, searchContext, "(and (or asset_type='1001' asset_type='1002'))", 1, 3))
                .Returns(new UnifiedSearchResponse { status = Status.Ok, searchResults = FakePagedSearchResults(), m_nTotalItems = 8 });
            _assetManagerMock
                .Setup(x => x.GetAssets(
                    10,
                    It.Is<IEnumerable<KeyValuePair<eAssetTypes, long>>>(_ => _.SequenceEqual(new[] { new KeyValuePair<eAssetTypes, long>(eAssetTypes.MEDIA, 103), new KeyValuePair<eAssetTypes, long>(eAssetTypes.MEDIA, 106), new KeyValuePair<eAssetTypes, long>(eAssetTypes.MEDIA, 108) })),
                    searchContext.IsAllowedToViewInactiveAssets))
                .Returns(GetFakePagedAssets());
            var service = new LineupService(_catalogManagerMock.Object, _regionManagerMock.Object, _assetManagerMock.Object, _searchProviderMock.Object, _publisher.Object, _loggerMock.Object);

            var result = service.GetLineupChannelAssets(10, regionId, searchContext, 1, 3);

            result.Should().NotBeNull();
            result.Status.Should().Be(Status.Ok);
            result.Objects.Should().NotBeEmpty();
            result.Objects[0].Id.Should().Be(103);
            result.Objects[0].LinearChannelNumber.Should().BeNull();
            result.Objects[1].Id.Should().Be(106);
            result.Objects[1].LinearChannelNumber.Should().BeNull();
            result.Objects[2].Id.Should().Be(108);
            result.Objects[2].LinearChannelNumber.Should().BeNull();
            result.TotalItems.Should().Be(8);
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void GetLineupChannelAssets_RegionNotDefinedAndDefaultRegionDefinedAndValidParameters_ReturnsExpectedResponse(long regionId)
        {
            var searchContext = GetUserSearchContext();
            _regionManagerMock
                .Setup(x => x.GetDefaultRegionId(10))
                .Returns(11);
            _regionManagerMock
                .Setup(x => x.GetRegion(10, 11))
                .Returns(new GenericResponse<Region>(Status.Ok, GetFakeRegion()));
            _catalogManagerMock
                .Setup(x => x.GetLinearMediaTypes(10))
                .Returns(new List<AssetStruct> { new AssetStruct { Id = 1001 }, new AssetStruct { Id = 1002 } });
            _searchProviderMock
                .Setup(x => x.SearchAssets(10, searchContext, "(and (or asset_type='1001' asset_type='1002') (or media_id:'106,102,107,101,108,104,103,105'))"))
                .Returns(new UnifiedSearchResponse { status = Status.Ok, searchResults = FakeSearchResultsByRegion(), m_nTotalItems = 8 });
            _assetManagerMock
                .Setup(x => x.GetAssets(
                    10,
                    It.Is<IEnumerable<KeyValuePair<eAssetTypes, long>>>(_ => _.SequenceEqual(new[] { new KeyValuePair<eAssetTypes, long>(eAssetTypes.MEDIA, 101), new KeyValuePair<eAssetTypes, long>(eAssetTypes.MEDIA, 103) })),
                    searchContext.IsAllowedToViewInactiveAssets))
                .Returns(GetFakeAssets());
            var service = new LineupService(_catalogManagerMock.Object, _regionManagerMock.Object, _assetManagerMock.Object, _searchProviderMock.Object, _publisher.Object, _loggerMock.Object);

            var result = service.GetLineupChannelAssets(10, regionId, searchContext, 1, 3);

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
            return new UserSearchContext(12, 13, 14, "udid", "ip", true, true, true, true, true, "sessionCharacteristicKey");
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

        private List<UnifiedSearchResult> FakeSearchResultsByRegion()
        {
            return new List<UnifiedSearchResult>
            {
                new UnifiedSearchResult { AssetId = "106" },
                new UnifiedSearchResult { AssetId = "103" },
                new UnifiedSearchResult { AssetId = "108" },
                new UnifiedSearchResult { AssetId = "104" },
                new UnifiedSearchResult { AssetId = "101" },
                new UnifiedSearchResult { AssetId = "102" }
            };
        }

        private List<UnifiedSearchResult> FakePagedSearchResults()
        {
            return new List<UnifiedSearchResult>
            {
                new UnifiedSearchResult { AssetId = "103" },
                new UnifiedSearchResult { AssetId = "106" },
                new UnifiedSearchResult { AssetId = "108" }
            };
        }

        private List<Asset> GetFakeAssets()
        {
            return new List<Asset>
            {
                new LiveAsset { Id = 103 },
                new LiveAsset { Id = 101 }
            };
        }

        private List<Asset> GetFakePagedAssets()
        {
            return new List<Asset>
            {
                new LiveAsset { Id = 103 },
                new LiveAsset { Id = 106 },
                new LiveAsset { Id = 108 }
            };
        }
    }
}