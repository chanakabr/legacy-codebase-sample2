using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ApiLogic.IndexManager.Models;
using ApiLogic.IndexManager.Sorting;
using ApiObjects;
using ApiObjects.SearchObjects;
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
        
            var result = sortingService.GetReorderedAssetIds(definitions, new List<ExtendedUnifiedSearchResult>());
        
            result.Should().BeNull();
        }

        [Test]
        public void GetReorderedAssetIds_SingleStats()
        {
            var orderByStatsResult = new List<(long id, string sortValue)> { (2, "3"), (1, "2") };
            var expectedResult = new List<long> { 2, 1 };
        
            var orderByField = new EsOrderByField(OrderBy.VIEWS, OrderDir.DESC);
            var extendedUnifiedSearchResults = GenerateDataExtended(2);
            var definitions = new UnifiedSearchDefinitions { orderByFields = new List<IEsOrderByField> { orderByField } };
            _esSortingService
                .Setup(x => x.ShouldSortByStatistics(orderByField))
                .Returns(true);
        
            var sortingService = new SortingService(
                GetSortingByStatsServiceMock(extendedUnifiedSearchResults, definitions, orderByField, orderByStatsResult).Object,
                _mockRepository.Create<ISortingByBasicFieldsService>().Object,
                GetSortingAdapterMock(definitions).Object,
                _esSortingService.Object);
        
            var result = sortingService.GetReorderedAssetIds(definitions, extendedUnifiedSearchResults);
        
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(expectedResult, options => options.WithStrictOrdering());
        }
        
        [TestCaseSource(nameof(NotStatsBeforeStatsData))]
        public void GetReorderedAssetIds_NotStatsBeforeStats(IEsOrderByField orderByField)
        {
            var orderByStatsResult = new List<(long id, string sortValue)> { (2, "3"), (3, "2") };
            var expectedResult = new List<long> { 2, 3, 1 };
        
            var extendedUnifiedSearchResults = GenerateDataExtended(3);
        
            var orderByLikes = new EsOrderByStatisticsField(OrderBy.LIKE_COUNTER, OrderDir.DESC, null);
            var definitions = new UnifiedSearchDefinitions { orderByFields = new[] { orderByField, orderByLikes } };
            _esSortingService
                .Setup(x => x.ShouldSortByStatistics(orderByLikes))
                .Returns(true);
            _esSortingService
                .Setup(x => x.ShouldSortByStatistics(orderByField))
                .Returns(false);
        
            var sortingService = new SortingService(
                GetSortingByStatsServiceMock(extendedUnifiedSearchResults, definitions, orderByLikes, orderByStatsResult).Object,
                _mockRepository.Create<ISortingByBasicFieldsService>().Object,
                GetSortingAdapterMock(definitions).Object,
                _esSortingService.Object);
        
            var result = sortingService.GetReorderedAssetIds(definitions, extendedUnifiedSearchResults);
        
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
        
            var extendedUnifiedSearchResults = GenerateDataExtended(5);

            IEsOrderByField orderByName = new EsOrderByField(OrderBy.NAME, OrderDir.DESC);
            IEsOrderByField orderByLikes = new EsOrderByStatisticsField(OrderBy.LIKE_COUNTER, OrderDir.DESC, null);
            var definitions = new UnifiedSearchDefinitions { orderByFields = new[] { orderByLikes, orderByName } };

            _esSortingService
                .Setup(x => x.ShouldSortByStatistics(orderByLikes))
                .Returns(true);
            _esSortingService
                .Setup(x => x.ShouldSortByStatistics(orderByName))
                .Returns(false);
        
            var sortingService = new SortingService(
                GetSortingByStatsServiceMock(extendedUnifiedSearchResults, definitions, orderByLikes, orderByStatsResult).Object,
                GetSortingByBasicFieldsServiceMock(extendedUnifiedSearchResults, orderByBasicResult, orderByName).Object,
                GetSortingAdapterMock(definitions).Object,
                _esSortingService.Object);
        
            var result = sortingService.GetReorderedAssetIds(definitions, extendedUnifiedSearchResults);
        
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
        
            var extendedUnifiedSearchResults = GenerateDataExtended(5);
        
            var orderByViews = new EsOrderByStatisticsField(OrderBy.VIEWS, OrderDir.DESC, null);
            var orderByLikes = new EsOrderByStatisticsField(OrderBy.LIKE_COUNTER, OrderDir.DESC, null);
            var definitions = new UnifiedSearchDefinitions { orderByFields = new[] { orderByViews, orderByLikes } };

            _esSortingService
                .Setup(x => x.ShouldSortByStatistics(orderByLikes))
                .Returns(true);
            _esSortingService
                .Setup(x => x.ShouldSortByStatistics(orderByViews))
                .Returns(true);
        
            var sortingByStatsServiceMock =
                GetSortingByStatsServiceMock(extendedUnifiedSearchResults, definitions, orderByViews, orderByViewsResult);
            sortingByStatsServiceMock.Setup(x => x.ListOrderedIdsWithSortValues(
                    It.Is<IEnumerable<ExtendedUnifiedSearchResult>>(
                        y => y.All(extendedUnifiedSearchResults.Contains)),
                    definitions,
                    orderByLikes
                ))
                .Returns(orderByLikesResult);
        
            var sortingService = new SortingService(
                sortingByStatsServiceMock.Object,
                _mockRepository.Create<ISortingByBasicFieldsService>().Object,
                GetSortingAdapterMock(definitions).Object,
                _esSortingService.Object);
        
            var result = sortingService.GetReorderedAssetIds(definitions, extendedUnifiedSearchResults);
        
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(expectedResult, options => options.WithStrictOrdering());
        }
        
        [Test]
        public void GetReorderedAssetIds_SkipSecondarySorting()
        {
            var expectedResult = new List<long> { 3, 4, 1, 5, 2 };
            var orderByViewsResult = new List<(long id, string sortValue)>
                { (3, "10"), (4, "5"), (1, "4"), (5, "2"), (2, "1") };
        
            var extendedUnifiedSearchResults = GenerateDataExtended(5);
        
            var orderByViews = new EsOrderByStatisticsField(OrderBy.VIEWS, OrderDir.DESC, null);
            var orderByLikes = new EsOrderByStatisticsField(OrderBy.LIKE_COUNTER, OrderDir.DESC, null);
            var definitions = new UnifiedSearchDefinitions { orderByFields = new[] { orderByViews, orderByLikes } };

            _esSortingService
                .Setup(x => x.ShouldSortByStatistics(orderByLikes))
                .Returns(true);
            _esSortingService
                .Setup(x => x.ShouldSortByStatistics(orderByViews))
                .Returns(true);
        
            var sortingService = new SortingService(
                GetSortingByStatsServiceMock(extendedUnifiedSearchResults, definitions, orderByViews, orderByViewsResult).Object,
                _mockRepository.Create<ISortingByBasicFieldsService>().Object,
                GetSortingAdapterMock(definitions).Object,
                _esSortingService.Object);
        
            var result = sortingService.GetReorderedAssetIds(definitions, extendedUnifiedSearchResults);
        
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(expectedResult, options => options.WithStrictOrdering());
        }

        [Test]
        [TestCaseSource(nameof(IsSortingCompletedTestCases))]
        public void IsSortingCompleted_ShouldReturnTrue2((IEsOrderByField EsOrderByField, bool isStatistics)[] orderFieldsBySortFeature, bool expectedResult)
        {
            var definitions = new UnifiedSearchDefinitions { orderByFields = orderFieldsBySortFeature.Select(x => x.EsOrderByField).ToArray() };

            foreach (var item in orderFieldsBySortFeature)
            {
                _esSortingService
                    .Setup(x => x.ShouldSortByStatistics(item.EsOrderByField))
                    .Returns(item.isStatistics);    
            }

            var sortingService = new SortingService(
                _mockRepository.Create<ISortingByStatsService>().Object,
                _mockRepository.Create<ISortingByBasicFieldsService>().Object,
                GetSortingAdapterMock(definitions).Object,
                _esSortingService.Object);
        
            var result = sortingService.IsSortingCompleted(definitions);
        
            result.Should().Be(expectedResult);
        }

        private static IEnumerable<ExtendedUnifiedSearchResult> GenerateDataExtended(int count)
        {
            var sortResults = Enumerable.Range(1, count)
                .Select(x => new UnifiedSearchResult { AssetId = x.ToString(), Score = (int)(x / 2) })
                .Reverse()
                .ToArray();

            return sortResults.Select(x => new ExtendedUnifiedSearchResult(x, GenerateDocument(x.AssetId, count))).ToArray();
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
                    },
                    {
                        "metas.meta_eng",
                        $"meta eng {groupCount}"
                    },
                    {
                        "metas.padded_meta_eng",
                        $"padded meta eng {groupCount}"
                    }
                }
            };
        }

        private Mock<ISortingByBasicFieldsService> GetSortingByBasicFieldsServiceMock(
            IEnumerable<ExtendedUnifiedSearchResult> extendedUnifiedSearchResults,
            IEnumerable<(long id, string sortValue)> expectedOrderByBasicResult,
            IEsOrderByField orderByField)
        {
            var sortingByBasicFieldsService = _mockRepository.Create<ISortingByBasicFieldsService>();
            sortingByBasicFieldsService
                .Setup(x => x.ListOrderedIdsWithSortValues(
                    It.Is<IEnumerable<ExtendedUnifiedSearchResult>>(
                        y => y.All(extendedUnifiedSearchResults.Contains)),
                    orderByField))
                .Returns(expectedOrderByBasicResult);

            return sortingByBasicFieldsService;
        }

        private Mock<ISortingByStatsService> GetSortingByStatsServiceMock(
            IEnumerable<ExtendedUnifiedSearchResult> extendedUnifiedSearchResults,
            UnifiedSearchDefinitions definitions,
            IEsOrderByField orderByField,
            IEnumerable<(long id, string sortValue)> expectedOrderByStatsResult)
        {
            var sortingByStatsServiceMock = _mockRepository.Create<ISortingByStatsService>();
            sortingByStatsServiceMock.Setup(x => x.ListOrderedIdsWithSortValues(
                    It.Is<IEnumerable<ExtendedUnifiedSearchResult>>(y => y.All(extendedUnifiedSearchResults.Contains)),
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
                    new EsOrderByMetaField("value", OrderDir.DESC, false, null, null)
                });
        }
        
        private static IEnumerable<TestCaseData> IsSortingCompletedTestCases()
        {
            yield return new TestCaseData(new (IEsOrderByField EsOrderByField, bool isStatistics)[]
                {
                    (new EsOrderByStatisticsField(OrderBy.VIEWS, OrderDir.DESC, null), true),
                    (new EsOrderByStatisticsField(OrderBy.LIKE_COUNTER, OrderDir.DESC, null), true)
                },
                false);
            
            yield return new TestCaseData(new (IEsOrderByField EsOrderByField, bool isStatistics)[]
                {
                    (new EsOrderByStatisticsField(OrderBy.CREATE_DATE, OrderDir.DESC, null), false),
                    (new EsOrderByStatisticsField(OrderBy.LIKE_COUNTER, OrderDir.DESC, null), true)
                },
                false);
            
            yield return new TestCaseData(new (IEsOrderByField EsOrderByField, bool isStatistics)[]
                {
                    (new EsOrderByStatisticsField(OrderBy.CREATE_DATE, OrderDir.DESC, null), false),
                },
                true);
            
            yield return new TestCaseData(new (IEsOrderByField EsOrderByField, bool isStatistics)[]
                {
                    (new EsOrderByStatisticsField(OrderBy.LIKE_COUNTER, OrderDir.DESC, null), true),
                },
                false);
        }

        private static IEnumerable<TestCaseData> NotStatsBeforeStatsData()
        {
            yield return new TestCaseData(new EsOrderByField(OrderBy.NAME, OrderDir.DESC));
            yield return new TestCaseData(new EsOrderByField(OrderBy.CREATE_DATE, OrderDir.DESC));
            yield return new TestCaseData(new EsOrderByField(OrderBy.UPDATE_DATE, OrderDir.DESC));
            yield return new TestCaseData(new EsOrderByField(OrderBy.START_DATE, OrderDir.DESC));
            yield return new TestCaseData(new EsOrderByMetaField("meta", OrderDir.DESC, false, typeof(long), null));
            yield return new TestCaseData(new EsOrderByMetaField("meta", OrderDir.DESC, true, typeof(long), new LanguageObj { IsDefault = true }));
            yield return new TestCaseData(new EsOrderByMetaField("meta", OrderDir.DESC, false, typeof(long), new LanguageObj { Code = "eng"}));
            yield return new TestCaseData(new EsOrderByMetaField("meta", OrderDir.DESC, true, typeof(long), new LanguageObj { Code = "eng"}));
        }
    }
}
