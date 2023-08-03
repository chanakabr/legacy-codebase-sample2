using System.Collections.Generic;
using System.Linq;
using ApiLogic.Api.Managers;
using ApiLogic.Api.Managers.Rule;
using ApiLogic.Catalog;
using ApiLogic.Catalog.CatalogManagement.Services;
using ApiObjects;
using ApiObjects.Catalog;
using ApiObjects.Lineup;
using ApiObjects.Response;
using ApiObjects.SearchObjects;
using Core.Catalog;
using Core.Catalog.CatalogManagement;
using Core.Catalog.Request;
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
        private Mock<IFilterAsset> _filterAssetMock;
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
            _filterAssetMock = _mockRepository.Create<IFilterAsset>();
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
            var service = new LineupService(_catalogManagerMock.Object, _regionManagerMock.Object, _assetManagerMock.Object, _searchProviderMock.Object, _publisher.Object, _filterAssetMock.Object, _loggerMock.Object);

            var result = service.GetLineupChannelAssets(10, 11, GetUserSearchContext(), 1, 3);

            result.Should().NotBeNull();
            result.Status.Should().Be(new Status(2, "Custom Region Error"));
            result.Object.Should().BeNull();
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
                .Setup(x => x.Error("Linear asset structs were not found. partnerId:10.", null, It.IsAny<string>()));
            var service = new LineupService(_catalogManagerMock.Object, _regionManagerMock.Object, _assetManagerMock.Object, _searchProviderMock.Object, _publisher.Object, _filterAssetMock.Object, _loggerMock.Object);

            var result = service.GetLineupChannelAssets(10, 11, searchContext, 1, 3);

            result.Should().NotBeNull();
            result.Status.Should().Be(Status.Error);
            result.Object.Should().BeNull();
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
            var filterQuery =
                "(and (or asset_type='1001' asset_type='1002') media_id:'106,102,107,101,108,104,103,105')";
            _filterAssetMock
                .Setup(x => x.UpdateKsql(filterQuery, 10, searchContext.SessionCharacteristicKey))
                .Returns(filterQuery);
            _searchProviderMock
                .Setup(x => x.SearchAssets(It.Is<UnifiedSearchRequest>(r => r.m_nGroupID == 10 && r.filterQuery == filterQuery)))
                .Returns(new UnifiedSearchResponse { status = new Status(3, "Custom Search Error") });
            var service = new LineupService(_catalogManagerMock.Object, _regionManagerMock.Object, _assetManagerMock.Object, _searchProviderMock.Object, _publisher.Object, _filterAssetMock.Object, _loggerMock.Object);

            var result = service.GetLineupChannelAssets(10, 11, searchContext, 1, 3);

            result.Should().NotBeNull();
            result.Status.Should().Be(new Status(3, "Custom Search Error"));
            result.Object.Should().BeNull();
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
            var filterQuery =
                "(and (or asset_type='1001' asset_type='1002') media_id:'106,102,107,101,108,104,103,105')";
            _filterAssetMock
                .Setup(x => x.UpdateKsql(filterQuery, 10, searchContext.SessionCharacteristicKey))
                .Returns(filterQuery);
            _searchProviderMock
                .Setup(x => x.SearchAssets(It.Is<UnifiedSearchRequest>(r => r.m_nGroupID == 10 && r.filterQuery == filterQuery)))
                .Returns(new UnifiedSearchResponse { status = Status.Ok, searchResults = FakeSearchResultsByRegion() });
            _assetManagerMock
                .Setup(x => x.GetAssets(
                    10,
                    It.Is<IEnumerable<KeyValuePair<eAssetTypes, long>>>(_ => _.SequenceEqual(new[] { new KeyValuePair<eAssetTypes, long>(eAssetTypes.MEDIA, 101), new KeyValuePair<eAssetTypes, long>(eAssetTypes.MEDIA, 103) })),
                    searchContext.IsAllowedToViewInactiveAssets))
                .Returns(GetFakeAssets());
            var service = new LineupService(_catalogManagerMock.Object, _regionManagerMock.Object, _assetManagerMock.Object, _searchProviderMock.Object, _publisher.Object, _filterAssetMock.Object, _loggerMock.Object);

            var result = service.GetLineupChannelAssets(10, 11, searchContext, 1, 3);

            result.Should().NotBeNull();
            result.Status.Should().Be(Status.Ok);
            result.Object.LineupChannelAssets.Should().NotBeEmpty();
            result.Object.LineupChannelAssets[0].Id.Should().Be(101);
            result.Object.LineupChannelAssets[0].LinearChannelNumber.Should().Be(4);
            result.Object.LineupChannelAssets[1].Id.Should().Be(103);
            result.Object.LineupChannelAssets[1].LinearChannelNumber.Should().Be(7);
            result.Object.LineupChannelAssets[2].Id.Should().Be(101);
            result.Object.LineupChannelAssets[2].LinearChannelNumber.Should().Be(8);
            result.Object.TotalCount.Should().Be(8);
        }

        [Test]
        public void GetLineupChannelAssetsWithFilter_OrderByLcnAndFilterByLcn_ReturnsExpectedResponse()
        {
            var searchContext = GetUserSearchContext();
            _regionManagerMock
                .Setup(x => x.GetRegions(10, It.Is<RegionFilter>(m => m.RegionIds.Contains(11)), 0, 0))
                .Returns(new GenericListResponse<Region>(Status.Ok, new List<Region> { GetFakeRegion() }));
            _catalogManagerMock
                .Setup(x => x.GetLinearMediaTypes(10))
                .Returns(new List<AssetStruct> { new AssetStruct { Id = 1001 }, new AssetStruct { Id = 1002 } });
            var filterQuery =
                "(and (or asset_type='1001' asset_type='1002') media_id:'107,101,103,105')";
            _filterAssetMock
                .Setup(x => x.UpdateKsql(filterQuery, 10, searchContext.SessionCharacteristicKey))
                .Returns(filterQuery);
            _searchProviderMock
                .Setup(x => x.SearchAssets(It.Is<UnifiedSearchRequest>(r => r.m_nGroupID == 10 && r.filterQuery == filterQuery)))
                .Returns(new UnifiedSearchResponse { status = Status.Ok, searchResults = FakeSearchResultsByRegionWithFilterByLcn() });
            _assetManagerMock
                .Setup(x => x.GetAssets(
                    10,
                    It.Is<IEnumerable<KeyValuePair<eAssetTypes, long>>>(_ => _.SequenceEqual(new[]
                    {
                        new KeyValuePair<eAssetTypes, long>(eAssetTypes.MEDIA, 101),
                        new KeyValuePair<eAssetTypes, long>(eAssetTypes.MEDIA, 105),
                        new KeyValuePair<eAssetTypes, long>(eAssetTypes.MEDIA, 107),
                        new KeyValuePair<eAssetTypes, long>(eAssetTypes.MEDIA, 103)
                    })),
                    searchContext.IsAllowedToViewInactiveAssets))
                .Returns(GetFakeAssetsWithFilterByLcn());
            var service = new LineupService(_catalogManagerMock.Object, _regionManagerMock.Object, _assetManagerMock.Object, _searchProviderMock.Object, _publisher.Object, _filterAssetMock.Object, _loggerMock.Object);

            var request = new LineupRegionalChannelRequest
            {
                LcnLessThanOrEqual = 8, LcnGreaterThanOrEqual = 4, RegionId = 11, PageSize = 10, PartnerId = 10
            };
            var result = service.GetLineupChannelAssetsWithFilter(searchContext, request);

            result.Should().NotBeNull();
            result.Status.Should().Be(Status.Ok);
            result.Objects.Should().NotBeEmpty();
            result.Objects[0].Id.Should().Be(101);
            result.Objects[0].LinearChannelNumber.Should().Be(4);
            result.Objects[1].Id.Should().Be(105);
            result.Objects[1].LinearChannelNumber.Should().Be(5);
            result.Objects[2].Id.Should().Be(107);
            result.Objects[2].LinearChannelNumber.Should().Be(6);
            result.Objects[3].Id.Should().Be(103);
            result.Objects[3].LinearChannelNumber.Should().Be(7);
            result.Objects[4].Id.Should().Be(101);
            result.Objects[4].LinearChannelNumber.Should().Be(8);
            result.TotalItems.Should().Be(5);
        }

        [Test]
        public void GetLineupChannelAssetsWithFilter_OrderByLcnAndFilterByKsqlAndPaging_ReturnsExpectedResponse()
        {
            var searchContext = GetUserSearchContext();
            _regionManagerMock
                .Setup(x => x.GetRegions(10, It.Is<RegionFilter>(m => m.RegionIds.Contains(11)), 0, 0))
                .Returns(new GenericListResponse<Region>(Status.Ok, new List<Region> { GetFakeRegion() }));
            _catalogManagerMock
                .Setup(x => x.GetLinearMediaTypes(10))
                .Returns(new List<AssetStruct> { new AssetStruct { Id = 1001 }, new AssetStruct { Id = 1002 } });
            var filterQuery =
                "(and (or asset_type='1001' asset_type='1002') media_id:'106,102,107,101,108,104,103,105' (and name~'top'))";
            _filterAssetMock
                .Setup(x => x.UpdateKsql(filterQuery, 10, searchContext.SessionCharacteristicKey))
                .Returns(filterQuery);
            _searchProviderMock
                .Setup(x => x.SearchAssets(It.Is<UnifiedSearchRequest>(r => r.m_nGroupID == 10 && r.filterQuery == filterQuery)))
                .Returns(new UnifiedSearchResponse { status = Status.Ok, searchResults = FakeSearchResultsWithFilterByKsqlByRegion() });
            _assetManagerMock
                .Setup(x => x.GetAssets(
                    10,
                    It.Is<IEnumerable<KeyValuePair<eAssetTypes, long>>>(_ => _.SequenceEqual(new[]
                    {
                        new KeyValuePair<eAssetTypes, long>(eAssetTypes.MEDIA, 108)
                    })),
                    searchContext.IsAllowedToViewInactiveAssets))
                .Returns(GetFakeAssetsWithFilterByKsql());
            var service = new LineupService(_catalogManagerMock.Object, _regionManagerMock.Object, _assetManagerMock.Object, _searchProviderMock.Object, _publisher.Object, _filterAssetMock.Object, _loggerMock.Object);

            var request = new LineupRegionalChannelRequest
            {
                RegionId = 11, PageSize = 3, PageIndex = 2, PartnerId = 10, Ksql = "(and name~'top')"
            };
            var result = service.GetLineupChannelAssetsWithFilter(searchContext, request);

            result.Should().NotBeNull();
            result.Status.Should().Be(Status.Ok);
            result.Objects.Should().NotBeEmpty();
            result.Objects[0].Id.Should().Be(108);
            result.Objects[0].LinearChannelNumber.Should().Be(9);
            result.TotalItems.Should().Be(7);
        }

        [Test]
        public void GetLineupChannelAssetsWithFilter_OrderByLinearAndFilterByLcn_ReturnsExpectedResponse()
        {
            var searchContext = GetUserSearchContext();
            _regionManagerMock
                .Setup(x => x.GetRegions(10, It.Is<RegionFilter>(m => m.RegionIds.Contains(11)), 0, 0))
                .Returns(new GenericListResponse<Region>(Status.Ok, new List<Region> { GetFakeRegion() }));
            _catalogManagerMock
                .Setup(x => x.GetLinearMediaTypes(10))
                .Returns(new List<AssetStruct> { new AssetStruct { Id = 1001 }, new AssetStruct { Id = 1002 } });
            var filterQuery =
                "(and (or asset_type='1001' asset_type='1002') media_id:'107,101,103,105')";
            _filterAssetMock
                .Setup(x => x.UpdateKsql(filterQuery, 10, searchContext.SessionCharacteristicKey))
                .Returns(filterQuery);
            _searchProviderMock
                .Setup(x => x.SearchAssets(It.Is<UnifiedSearchRequest>(r => r.m_nGroupID == 10
                    && r.filterQuery == filterQuery
                    && r.orderingParameters.Single().Field == OrderBy.NAME
                    && r.orderingParameters.Single().Direction == OrderDir.DESC
                    && r.m_nPageSize == 10000)))
                .Returns(new UnifiedSearchResponse { status = Status.Ok, searchResults = FakeSearchResultsByRegionWithFilterByLcn() });
            _assetManagerMock
                .Setup(x => x.GetAssets(
                    10,
                    It.Is<IEnumerable<KeyValuePair<eAssetTypes, long>>>(_ => _.SequenceEqual(new[]
                    {
                        new KeyValuePair<eAssetTypes, long>(eAssetTypes.MEDIA, 107),
                        new KeyValuePair<eAssetTypes, long>(eAssetTypes.MEDIA, 103),
                        new KeyValuePair<eAssetTypes, long>(eAssetTypes.MEDIA, 101),
                        new KeyValuePair<eAssetTypes, long>(eAssetTypes.MEDIA, 105)
                    })),
                    searchContext.IsAllowedToViewInactiveAssets))
                .Returns(GetFakeAssetsWithFilterByLcn());
            var service = new LineupService(_catalogManagerMock.Object, _regionManagerMock.Object, _assetManagerMock.Object, _searchProviderMock.Object, _publisher.Object, _filterAssetMock.Object, _loggerMock.Object);

            var request = new LineupRegionalChannelRequest
            {
                LcnLessThanOrEqual = 8,
                LcnGreaterThanOrEqual = 4,
                RegionId = 11,
                PageSize = 10,
                OrderBy = LineupRegionalChannelOrderBy.NAME_DESC,
                PartnerId = 10
            };
            var result = service.GetLineupChannelAssetsWithFilter(searchContext, request);

            result.Should().NotBeNull();
            result.Status.Should().Be(Status.Ok);
            result.Objects.Should().NotBeEmpty();
            result.Objects[0].Id.Should().Be(107);
            result.Objects[0].LinearChannelNumber.Should().Be(6);
            result.Objects[1].Id.Should().Be(103);
            result.Objects[1].LinearChannelNumber.Should().Be(7);
            result.Objects[2].Id.Should().Be(101);
            result.Objects[2].LinearChannelNumber.Should().Be(4);
            result.Objects[3].Id.Should().Be(101);
            result.Objects[3].LinearChannelNumber.Should().Be(8);
            result.Objects[4].Id.Should().Be(105);
            result.Objects[4].LinearChannelNumber.Should().Be(5);
            result.TotalItems.Should().Be(5);
        }

        [Test]
        public void GetLineupChannelAssetsWithFilter_OrderByLinearAndFilterByKsqlAndPaging_ReturnsExpectedResponse()
        {
            var searchContext = GetUserSearchContext();
            _regionManagerMock
                .Setup(x => x.GetRegions(10, It.Is<RegionFilter>(m => m.RegionIds.Contains(11)), 0, 0))
                .Returns(new GenericListResponse<Region>(Status.Ok, new List<Region> { GetFakeRegion() }));
            _catalogManagerMock
                .Setup(x => x.GetLinearMediaTypes(10))
                .Returns(new List<AssetStruct> { new AssetStruct { Id = 1001 }, new AssetStruct { Id = 1002 } });
            var filterQuery =
                "(and (or asset_type='1001' asset_type='1002') media_id:'106,102,107,101,108,104,103,105' (and name~'top'))";
            _filterAssetMock
                .Setup(x => x.UpdateKsql(filterQuery, 10, searchContext.SessionCharacteristicKey))
                .Returns(filterQuery);
            _searchProviderMock
                .Setup(x => x.SearchAssets(It.Is<UnifiedSearchRequest>(r => r.m_nGroupID == 10
                    && r.filterQuery == filterQuery
                    && r.orderingParameters.Single().Field == OrderBy.NAME
                    && r.orderingParameters.Single().Direction == OrderDir.ASC
                    && r.m_nPageSize == 10000)))
                .Returns(new UnifiedSearchResponse { status = Status.Ok, searchResults = FakeSearchResultsWithFilterByKsqlByRegion() });
            _assetManagerMock
                .Setup(x => x.GetAssets(
                    10,
                    It.Is<IEnumerable<KeyValuePair<eAssetTypes, long>>>(_ => _.SequenceEqual(new[]
                    {
                        new KeyValuePair<eAssetTypes, long>(eAssetTypes.MEDIA, 101)
                    })),
                    searchContext.IsAllowedToViewInactiveAssets))
                .Returns(new List<Asset> { new LiveAsset { Id = 101 }});
            var service = new LineupService(_catalogManagerMock.Object, _regionManagerMock.Object, _assetManagerMock.Object, _searchProviderMock.Object, _publisher.Object, _filterAssetMock.Object, _loggerMock.Object);

            var request = new LineupRegionalChannelRequest
            {
                RegionId = 11,
                PageSize = 3,
                PageIndex = 2,
                PartnerId = 10,
                Ksql = "(and name~'top')",
                OrderBy = LineupRegionalChannelOrderBy.NAME_ASC
            };
            var result = service.GetLineupChannelAssetsWithFilter(searchContext, request);

            result.Should().NotBeNull();
            result.Status.Should().Be(Status.Ok);
            result.Objects.Should().NotBeEmpty();
            result.Objects[0].Id.Should().Be(101);
            result.Objects[0].LinearChannelNumber.Should().Be(8);
            result.TotalItems.Should().Be(7);
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
                .Setup(x => x.Error("Linear asset structs were not found. partnerId:10.", null, It.IsAny<string>()));
            var service = new LineupService(_catalogManagerMock.Object, _regionManagerMock.Object, _assetManagerMock.Object, _searchProviderMock.Object, _publisher.Object, _filterAssetMock.Object, _loggerMock.Object);

            var result = service.GetLineupChannelAssets(10, regionId, searchContext, 1, 3);

            result.Should().NotBeNull();
            result.Status.Should().Be(Status.Error);
            result.Object.Should().BeNull();
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
            var filterQuery =
                "(and (or asset_type='1001' asset_type='1002'))";
            _filterAssetMock
                .Setup(x => x.UpdateKsql(filterQuery, 10, searchContext.SessionCharacteristicKey))
                .Returns(filterQuery);
            _searchProviderMock
                .Setup(x => x.SearchAssets(It.Is<UnifiedSearchRequest>(r => r.m_nGroupID == 10
                    && r.filterQuery == filterQuery
                    && r.m_nPageIndex == 1
                    && r.m_nPageSize == 3)))
                .Returns(new UnifiedSearchResponse { status = new Status(3, "Custom Search Error") });
            var service = new LineupService(_catalogManagerMock.Object, _regionManagerMock.Object, _assetManagerMock.Object, _searchProviderMock.Object, _publisher.Object, _filterAssetMock.Object, _loggerMock.Object);

            var result = service.GetLineupChannelAssets(10, regionId, searchContext, 1, 3);

            result.Should().NotBeNull();
            result.Status.Should().Be(new Status(3, "Custom Search Error"));
            result.Object.Should().BeNull();
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
            var filterQuery =
                "(and (or asset_type='1001' asset_type='1002'))";
            _filterAssetMock
                .Setup(x => x.UpdateKsql(filterQuery, 10, searchContext.SessionCharacteristicKey))
                .Returns(filterQuery);
            _searchProviderMock
                .Setup(x => x.SearchAssets(It.Is<UnifiedSearchRequest>(r => r.m_nGroupID == 10
                    && r.filterQuery == filterQuery
                    && r.m_nPageIndex == 1
                    && r.m_nPageSize == 3)))
                .Returns(new UnifiedSearchResponse { status = Status.Ok, searchResults = FakePagedSearchResults(), m_nTotalItems = 8 });
            _assetManagerMock
                .Setup(x => x.GetAssets(
                    10,
                    It.Is<IEnumerable<KeyValuePair<eAssetTypes, long>>>(_ => _.SequenceEqual(new[] { new KeyValuePair<eAssetTypes, long>(eAssetTypes.MEDIA, 103), new KeyValuePair<eAssetTypes, long>(eAssetTypes.MEDIA, 106), new KeyValuePair<eAssetTypes, long>(eAssetTypes.MEDIA, 108) })),
                    searchContext.IsAllowedToViewInactiveAssets))
                .Returns(GetFakePagedAssets());
            var service = new LineupService(_catalogManagerMock.Object, _regionManagerMock.Object, _assetManagerMock.Object, _searchProviderMock.Object, _publisher.Object, _filterAssetMock.Object, _loggerMock.Object);

            var result = service.GetLineupChannelAssets(10, regionId, searchContext, 1, 3);

            result.Should().NotBeNull();
            result.Status.Should().Be(Status.Ok);
            result.Object.LineupChannelAssets.Should().NotBeEmpty();
            result.Object.LineupChannelAssets[0].Id.Should().Be(103);
            result.Object.LineupChannelAssets[0].LinearChannelNumber.Should().BeNull();
            result.Object.LineupChannelAssets[1].Id.Should().Be(106);
            result.Object.LineupChannelAssets[1].LinearChannelNumber.Should().BeNull();
            result.Object.LineupChannelAssets[2].Id.Should().Be(108);
            result.Object.LineupChannelAssets[2].LinearChannelNumber.Should().BeNull();
            result.Object.TotalCount.Should().Be(8);
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
            var filterQuery =
                "(and (or asset_type='1001' asset_type='1002') media_id:'106,102,107,101,108,104,103,105')";
            _filterAssetMock
                .Setup(x => x.UpdateKsql(filterQuery, 10, searchContext.SessionCharacteristicKey))
                .Returns(filterQuery);
            _searchProviderMock
                .Setup(x => x.SearchAssets(It.Is<UnifiedSearchRequest>(r => r.m_nGroupID == 10
                    && r.filterQuery == filterQuery)))
                .Returns(new UnifiedSearchResponse { status = Status.Ok, searchResults = FakeSearchResultsByRegion(), m_nTotalItems = 8 });
            _assetManagerMock
                .Setup(x => x.GetAssets(
                    10,
                    It.Is<IEnumerable<KeyValuePair<eAssetTypes, long>>>(_ => _.SequenceEqual(new[] { new KeyValuePair<eAssetTypes, long>(eAssetTypes.MEDIA, 101), new KeyValuePair<eAssetTypes, long>(eAssetTypes.MEDIA, 103) })),
                    searchContext.IsAllowedToViewInactiveAssets))
                .Returns(GetFakeAssets());
            var service = new LineupService(_catalogManagerMock.Object, _regionManagerMock.Object, _assetManagerMock.Object, _searchProviderMock.Object, _publisher.Object, _filterAssetMock.Object, _loggerMock.Object);

            var result = service.GetLineupChannelAssets(10, regionId, searchContext, 1, 3);

            result.Should().NotBeNull();
            result.Status.Should().Be(Status.Ok);
            result.Object.LineupChannelAssets.Should().NotBeEmpty();
            result.Object.LineupChannelAssets[0].Id.Should().Be(101);
            result.Object.LineupChannelAssets[0].LinearChannelNumber.Should().Be(4);
            result.Object.LineupChannelAssets[1].Id.Should().Be(103);
            result.Object.LineupChannelAssets[1].LinearChannelNumber.Should().Be(7);
            result.Object.LineupChannelAssets[2].Id.Should().Be(101);
            result.Object.LineupChannelAssets[2].LinearChannelNumber.Should().Be(8);
            result.Object.TotalCount.Should().Be(8);
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

        private List<UnifiedSearchResult> FakeSearchResultsByRegionWithFilterByLcn()
        {
            return new List<UnifiedSearchResult>
            {
                new UnifiedSearchResult { AssetId = "107" },
                new UnifiedSearchResult { AssetId = "103" },
                new UnifiedSearchResult { AssetId = "101" },
                new UnifiedSearchResult { AssetId = "105" }
            };
        }

        private List<UnifiedSearchResult> FakeSearchResultsWithFilterByKsqlByRegion()
        {
            return new List<UnifiedSearchResult>
            {
                new UnifiedSearchResult { AssetId = "106" },
                new UnifiedSearchResult { AssetId = "105" },
                new UnifiedSearchResult { AssetId = "108" },
                new UnifiedSearchResult { AssetId = "104" },
                new UnifiedSearchResult { AssetId = "107" },
                new UnifiedSearchResult { AssetId = "101" }
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

        private List<Asset> GetFakeAssetsWithFilterByLcn()
        {
            return new List<Asset>
            {
                new LiveAsset { Id = 101 },
                new LiveAsset { Id = 105 },
                new LiveAsset { Id = 107 },
                new LiveAsset { Id = 103 }
            };
        }

        private List<Asset> GetFakeAssetsWithFilterByKsql()
        {
            return new List<Asset>
            {
                new LiveAsset { Id = 108 }
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
