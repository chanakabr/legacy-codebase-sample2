using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ApiLogic.IndexManager.QueryBuilders.ESV2QueryBuilders.SearchPriority.Models;
using ApiLogic.IndexManager.Sorting;
using ApiObjects.SearchObjects;
using ApiObjects.SearchPriorityGroups;
using Core.Catalog.Response;
using ElasticSearch.Common;
using ElasticSearch.Searcher;
using ElasticSearch.Utils;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace ApiLogic.Tests.IndexManager
{
    public class SortingServiceTests
    {
        private MockRepository _mockRepository;
        private Mock<IEsSortingService> _esSortingService;

        [SetUp]
        public void Setup()
        {
            _mockRepository = new MockRepository(MockBehavior.Strict);
            _esSortingService = _mockRepository.Create<IEsSortingService>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockRepository.VerifyAll();
        }

        [TestCaseSource(nameof(NoStatsData))]
        public void GetReorderedAssetIds_NoStats(IReadOnlyCollection<IEsOrderByField> ordering)
        {
            var definitions = new UnifiedSearchDefinitions { orderByFields = ordering };
            foreach (var orderByField in ordering)
            {
                _esSortingService
                    .Setup(x => x.ShouldSortByStatistics(orderByField))
                    .Returns(false);
            }

            var sortingService = new SortingService(
                _mockRepository.Create<ISortingByStatsService>().Object,
                _mockRepository.Create<ISortingByBasicFieldsService>().Object,
                GetSortingAdapterMock(definitions).Object,
                _esSortingService.Object);

            var result = sortingService.GetReorderedAssetIds(
                new List<UnifiedSearchResult>(),
                definitions,
                new Dictionary<string, ElasticSearchApi.ESAssetDocument>());

            result.Should().BeNull();
        }

        [Test]
        public void GetReorderedAssetIds_SingleStats()
        {
            var orderByStatsResult = new List<(long id, string sortValue)> { (2, "3"), (1, "2") };
            var expectedResult = new List<long> { 2, 1 };

            var orderByField = new EsOrderByField(OrderBy.VIEWS, OrderDir.DESC);
            var (sortingResults, assetIdToDocument) = GenerateData(2);
            var definitions = new UnifiedSearchDefinitions { orderByFields = new List<IEsOrderByField> { orderByField } };
            var assetIds = sortingResults.Select(x => long.Parse(x.AssetId)).ToArray();
            var esDocuments = assetIdToDocument.Values.ToArray();
            _esSortingService
                .Setup(x => x.ShouldSortByStatistics(orderByField))
                .Returns(true);

            var sortingService = new SortingService(
                GetSortingByStatsServiceMock(esDocuments, assetIds, definitions, orderByField, orderByStatsResult).Object,
                _mockRepository.Create<ISortingByBasicFieldsService>().Object,
                GetSortingAdapterMock(definitions).Object,
                _esSortingService.Object);

            var result = sortingService.GetReorderedAssetIds(
                sortingResults,
                definitions,
                assetIdToDocument);

            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(expectedResult, options => options.WithStrictOrdering());
        }

        [TestCaseSource(nameof(NotStatsBeforeStatsData))]
        public void GetReorderedAssetIds_NotStatsBeforeStats(IEsOrderByField orderByField)
        {
            var orderByStatsResult = new List<(long id, string sortValue)> { (2, "3"), (3, "2") };
            var expectedResult = new List<long> { 2, 3, 1 };

            var (sortingResults, assetIdToDocument) = GenerateData(3);

            var orderByLikes = new EsOrderByStatisticsField(OrderBy.LIKE_COUNTER, OrderDir.DESC, null);
            var definitions = new UnifiedSearchDefinitions { orderByFields = new[] { orderByField, orderByLikes } };
            var esDocuments = new[] { assetIdToDocument["2"], assetIdToDocument["3"] };
            var assetIds = new HashSet<long> { 2, 3 };
            _esSortingService
                .Setup(x => x.ShouldSortByStatistics(orderByLikes))
                .Returns(true);
            _esSortingService
                .Setup(x => x.ShouldSortByStatistics(orderByField))
                .Returns(false);

            var sortingService = new SortingService(
                GetSortingByStatsServiceMock(esDocuments, assetIds, definitions, orderByLikes, orderByStatsResult).Object,
                _mockRepository.Create<ISortingByBasicFieldsService>().Object,
                GetSortingAdapterMock(definitions).Object,
                _esSortingService.Object);

            var result = sortingService.GetReorderedAssetIds(
                sortingResults,
                definitions,
                assetIdToDocument);

            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(expectedResult, options => options.WithStrictOrdering());
        }

        [Test]
        public void GetReorderedAssetIds_StatsBeforeNotStats()
        {
            var orderByStatsResult = new List<(long id, string sortValue)> { (3, "10"), (5, "4"), (1, "4"), (4, "2"), (2, "2") };
            var orderByBasicResult = new List<(long id, string sortValue)>
                { (1, "name 4"), (2, "name 3"), (4, "name 2"), (5, "name 1") };
            var expectedResult = new List<long> { 3, 1, 5, 2, 4 };

            var (sortingResults, assetIdToDocument) = GenerateData(5);
            var orderByBasicInput = new[]
                { assetIdToDocument["5"], assetIdToDocument["1"], assetIdToDocument["4"], assetIdToDocument["2"] };

            IEsOrderByField orderByName = new EsOrderByField(OrderBy.NAME, OrderDir.DESC);
            IEsOrderByField orderByLikes = new EsOrderByStatisticsField(OrderBy.LIKE_COUNTER, OrderDir.DESC, null);
            var definitions = new UnifiedSearchDefinitions { orderByFields = new[] { orderByLikes, orderByName } };
            var assetIds = sortingResults.Select(x => long.Parse(x.AssetId)).ToArray();
            var esDocuments = assetIdToDocument.Values.ToArray();

            _esSortingService
                .Setup(x => x.ShouldSortByStatistics(orderByLikes))
                .Returns(true);
            _esSortingService
                .Setup(x => x.ShouldSortByStatistics(orderByName))
                .Returns(false);

            var sortingService = new SortingService(
                GetSortingByStatsServiceMock(esDocuments, assetIds, definitions, orderByLikes, orderByStatsResult).Object,
                GetSortingByBasicFieldsServiceMock(orderByBasicInput, orderByBasicResult, orderByName).Object,
                GetSortingAdapterMock(definitions).Object,
                _esSortingService.Object);

            var result = sortingService.GetReorderedAssetIds(
                sortingResults,
                definitions,
                assetIdToDocument);

            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(expectedResult, options => options.WithStrictOrdering());
        }

        [Test]
        public void GetReorderedAssetIds_TwoStats()
        {
            var orderByViewsResult = new List<(long id, string sortValue)>
                { (3, "10"), (5, "4"), (1, "4"), (4, "2"), (2, "2") };
            var orderByLikesResult = new List<(long id, string sortValue)>
                { (1, "10"), (2, "8"), (4, "5"), (5, "3") };
            var expectedResult = new List<long> { 3, 1, 5, 2, 4 };

            var (sortingResults, assetIdToDocument) = GenerateData(5);

            var orderByViews = new EsOrderByStatisticsField(OrderBy.VIEWS, OrderDir.DESC, null);
            var orderByLikes = new EsOrderByStatisticsField(OrderBy.LIKE_COUNTER, OrderDir.DESC, null);
            var definitions = new UnifiedSearchDefinitions { orderByFields = new[] { orderByViews, orderByLikes } };
            var assetIdsForViews = sortingResults.Select(x => long.Parse(x.AssetId)).ToArray();
            var assetIdsForLikes = new long[] { 4, 2, 5, 1 };
            var esDocumentsForViews = assetIdToDocument.Values.ToArray();
            var esDocumentsForLikes = new[]
            {
                assetIdToDocument["4"], assetIdToDocument["2"], assetIdToDocument["5"], assetIdToDocument["1"]
            };

            _esSortingService
                .Setup(x => x.ShouldSortByStatistics(orderByLikes))
                .Returns(true);
            _esSortingService
                .Setup(x => x.ShouldSortByStatistics(orderByViews))
                .Returns(true);

            var sortingByStatsServiceMock =
                GetSortingByStatsServiceMock(esDocumentsForViews, assetIdsForViews, definitions, orderByViews, orderByViewsResult);
            sortingByStatsServiceMock.Setup(x => x.ListOrderedIdsWithSortValues(
                    It.Is<IEnumerable<ElasticSearchApi.ESAssetDocument>>(
                        y => y.All(esDocumentsForLikes.Contains)),
                    It.Is<IEnumerable<long>>(ids => ids.All(assetIdsForLikes.Contains)),
                    definitions,
                    orderByLikes
                ))
                .Returns(orderByLikesResult);

            var sortingService = new SortingService(
                sortingByStatsServiceMock.Object,
                _mockRepository.Create<ISortingByBasicFieldsService>().Object,
                GetSortingAdapterMock(definitions).Object,
                _esSortingService.Object);

            var result = sortingService.GetReorderedAssetIds(
                sortingResults,
                definitions,
                assetIdToDocument);

            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(expectedResult, options => options.WithStrictOrdering());
        }

        [Test]
        public void GetReorderedAssetIds_SkipSecondarySorting()
        {
            var expectedResult = new List<long> { 3, 4, 1, 5, 2 };
            var orderByViewsResult = new List<(long id, string sortValue)>
                { (3, "10"), (4, "5"), (1, "4"), (5, "2"), (2, "1") };

            var (sortingResults, assetIdToDocument) = GenerateData(5);

            var orderByViews = new EsOrderByStatisticsField(OrderBy.VIEWS, OrderDir.DESC, null);
            var orderByLikes = new EsOrderByStatisticsField(OrderBy.LIKE_COUNTER, OrderDir.DESC, null);
            var definitions = new UnifiedSearchDefinitions { orderByFields = new[] { orderByViews, orderByLikes } };
            var assetIds = sortingResults.Select(x => long.Parse(x.AssetId)).ToArray();
            var esDocuments = assetIdToDocument.Values.ToArray();

            _esSortingService
                .Setup(x => x.ShouldSortByStatistics(orderByLikes))
                .Returns(true);
            _esSortingService
                .Setup(x => x.ShouldSortByStatistics(orderByViews))
                .Returns(true);

            var sortingService = new SortingService(
                GetSortingByStatsServiceMock(esDocuments, assetIds, definitions, orderByViews, orderByViewsResult).Object,
                _mockRepository.Create<ISortingByBasicFieldsService>().Object,
                GetSortingAdapterMock(definitions).Object,
                _esSortingService.Object);

            var result = sortingService.GetReorderedAssetIds(
                sortingResults,
                definitions,
                assetIdToDocument);

            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(expectedResult, options => options.WithStrictOrdering());
        }

        [Test]
        public void GetReorderedAssetIds_PriorityGroupsSet()
        {
            var expectedResult = new List<long> { 4, 5, 2, 3, 1 };
            var orderByStatsResult = new List<(long id, string sortValue)> { (4, "10"), (2, "9"), (3, "8"), (5, "4") };
            var (sortingResults, assetIdToDocument) = GenerateData(5);

            var orderByViews = new EsOrderByStatisticsField(OrderBy.VIEWS, OrderDir.DESC, null);
            var definitions = new UnifiedSearchDefinitions
            {
                orderByFields = new[] { orderByViews },
                PriorityGroupsMappings = new Dictionary<double, IEsPriorityGroup>
                {
                    { 1.0, new KSqlEsPriorityGroup(new BooleanLeaf()) }
                }
            };

            var assetIds = new long[] { 5, 4, 3, 2 };
            var esDocuments = new[]
            {
                assetIdToDocument["5"], assetIdToDocument["4"], assetIdToDocument["3"], assetIdToDocument["2"]
            };

            _esSortingService
                .Setup(x => x.ShouldSortByStatistics(orderByViews))
                .Returns(true);

            var sortingService = new SortingService(
                GetSortingByStatsServiceMock(esDocuments, assetIds, definitions, orderByViews, orderByStatsResult).Object,
                _mockRepository.Create<ISortingByBasicFieldsService>().Object,
                GetSortingAdapterMock(definitions).Object,
                _esSortingService.Object);

            var result = sortingService.GetReorderedAssetIds(
                sortingResults,
                definitions,
                assetIdToDocument);

            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(expectedResult, options => options.WithStrictOrdering());
        }

        private static (IEnumerable<UnifiedSearchResult> sortingResults, IDictionary<string, ElasticSearchApi.ESAssetDocument> assetIdToDocument)
            GenerateData(int count)
        {
            var sortResults = Enumerable.Range(1, count)
                .Select(x => new UnifiedSearchResult { AssetId = x.ToString(), Score = (int)(x / 2)})
                .Reverse()
                .ToArray();
            var esDocuments = sortResults
                .ToDictionary(
                    x => x.AssetId,
                    x => GenerateDocument(x.AssetId, count));

            return (sortResults, esDocuments);
        }

        private static ElasticSearchApi.ESAssetDocument GenerateDocument(string assetId, int totalCount)
        {
            var parsedId = int.Parse(assetId);
            var currentDateTime = DateTime.UtcNow;
            var groupCount = parsedId / 2;

            return new ElasticSearchApi.ESAssetDocument
            {
                name = $"document {groupCount}",
                id = assetId,
                asset_id = parsedId,
                start_date = currentDateTime.AddDays(totalCount - groupCount),
                update_date = currentDateTime.AddDays(totalCount - groupCount),
                score = groupCount,
                extraReturnFields = new Dictionary<string, string>
                {
                    {
                        "create_date",
                        (currentDateTime.AddDays(totalCount - groupCount)).ToString(CultureInfo.InvariantCulture)
                    },
                    {
                        "metas.meta",
                        $"meta {groupCount}"
                    },
                    {
                        "metas.padded_meta",
                        $"meta {groupCount}"
                    }
                }
            };
        }

        private Mock<ISortingByBasicFieldsService> GetSortingByBasicFieldsServiceMock(
            IEnumerable<ElasticSearchApi.ESAssetDocument> orderByBasicInput,
            IEnumerable<(long id, string sortValue)> expectedOrderByBasicResult,
            IEsOrderByField orderByField)
        {
            var sortingByBasicFieldsService = _mockRepository.Create<ISortingByBasicFieldsService>();
            sortingByBasicFieldsService
                .Setup(x => x.ListOrderedIdsWithSortValues(
                    It.Is<IEnumerable<ElasticSearchApi.ESAssetDocument>>(
                        y => y.All(orderByBasicInput.Contains)),
                    orderByField))
                .Returns(expectedOrderByBasicResult);

            return sortingByBasicFieldsService;
        }

        private Mock<ISortingByStatsService> GetSortingByStatsServiceMock(
            IEnumerable<ElasticSearchApi.ESAssetDocument> esDocuments,
            IEnumerable<long> assetIds,
            UnifiedSearchDefinitions definitions,
            IEsOrderByField orderByField,
            IEnumerable<(long id, string sortValue)> expectedOrderByStatsResult)
        {
            var sortingByStatsServiceMock = _mockRepository.Create<ISortingByStatsService>();
            sortingByStatsServiceMock.Setup(x => x.ListOrderedIdsWithSortValues(
                    It.Is<IEnumerable<ElasticSearchApi.ESAssetDocument>>(
                        y => y.All(esDocuments.Contains)),
                    It.Is<IEnumerable<long>>(ids => ids.All(assetIds.Contains)),
                    definitions,
                    orderByField
                ))
                .Returns(expectedOrderByStatsResult);

            return sortingByStatsServiceMock;
        }

        private Mock<ISortingAdapter> GetSortingAdapterMock(UnifiedSearchDefinitions definitions)
        {
            var sortingAdapterMock = _mockRepository.Create<ISortingAdapter>();
            sortingAdapterMock.Setup(x => x.ResolveOrdering(definitions)).Returns(definitions.orderByFields);

            return sortingAdapterMock;
        }

        private static IEnumerable<TestCaseData> NoStatsData()
        {
            yield return new TestCaseData(
                new List<IEsOrderByField> { new EsOrderByField(OrderBy.NAME, OrderDir.DESC) });
            yield return new TestCaseData(
                new List<IEsOrderByField>
                {
                    new EsOrderByField(OrderBy.NAME, OrderDir.DESC),
                    new EsOrderByMetaField("value", OrderDir.DESC, false, null)
                });
        }

        private static IEnumerable<TestCaseData> NotStatsBeforeStatsData()
        {
            yield return new TestCaseData(new EsOrderByField(OrderBy.NAME, OrderDir.DESC));
            yield return new TestCaseData(new EsOrderByField(OrderBy.CREATE_DATE, OrderDir.DESC));
            yield return new TestCaseData(new EsOrderByField(OrderBy.UPDATE_DATE, OrderDir.DESC));
            yield return new TestCaseData(new EsOrderByField(OrderBy.START_DATE, OrderDir.DESC));
            yield return new TestCaseData(new EsOrderByMetaField("meta", OrderDir.DESC, false, typeof(long)));
            yield return new TestCaseData(new EsOrderByMetaField("meta", OrderDir.DESC, true, typeof(long)));
        }
    }
}