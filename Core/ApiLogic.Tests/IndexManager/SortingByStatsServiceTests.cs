using System;
using System.Collections.Generic;
using System.Linq;
using ApiLogic.IndexManager.Sorting;
using ApiObjects.SearchObjects;
using ElasticSearch.Common;
using ElasticSearch.Searcher;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace ApiLogic.Tests.IndexManager
{
    public class SortingByStatsServiceTests
    {
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

        [TestCaseSource(nameof(NotImplementedData))]
        public void ListOrderedIdsWithSortValues_ThrowsNotImplementedException(IEsOrderByField orderByField)
        {
            var sortingByStatsService = new SortingByStatsService(
                _mockRepository.Create<IStartDateAssociationTagsSortStrategy>().Object,
                _mockRepository.Create<IRecommendationSortStrategy>().Object,
                _mockRepository.Create<IStatisticsSortStrategy>().Object,
                _mockRepository.Create<ISlidingWindowOrderStrategy>().Object);

            Action act = () => sortingByStatsService.ListOrderedIdsWithSortValues(
                Enumerable.Empty<ElasticSearchApi.ESAssetDocument>(),
                Enumerable.Empty<long>(),
                new UnifiedSearchDefinitions(),
                orderByField);

            act.Should().Throw<NotImplementedException>();
        }

        [Test]
        public void ListOrderedIdsWithSortValues_SortByStartDate()
        {
            var esOrderByField = new EsOrderByStartDateAndAssociationTags(OrderDir.DESC);
            var definitions = new UnifiedSearchDefinitions();
            var documents = Enumerable.Empty<ElasticSearchApi.ESAssetDocument>();

            var expectedResult = Enumerable.Empty<(long id, string sortValue)>();

            var sortingByStatsService = new SortingByStatsService(
                GetStartDateStrategy(documents, esOrderByField.OrderByDirection, definitions, expectedResult).Object,
                _mockRepository.Create<IRecommendationSortStrategy>().Object,
                _mockRepository.Create<IStatisticsSortStrategy>().Object,
                _mockRepository.Create<ISlidingWindowOrderStrategy>().Object);

            var result = sortingByStatsService.ListOrderedIdsWithSortValues(
                documents,
                Enumerable.Empty<long>(),
                definitions,
                esOrderByField);

            result.Should().NotBeNull();
            result.Should().Equal(expectedResult);
        }

        [Test]
        public void ListOrderedIdsWithSortValues_SortWithSlidingWindow()
        {
            var esOrderByField = new EsOrderBySlidingWindow(OrderBy.VIEWS, OrderDir.DESC, default);
            var definitions = new UnifiedSearchDefinitions();
            var documents = Enumerable.Empty<ElasticSearchApi.ESAssetDocument>();
            var assetIds = Enumerable.Empty<long>();

            var expectedResult = Enumerable.Empty<(long id, string sortValue)>();

            var sortingByStatsService = new SortingByStatsService(
                _mockRepository.Create<IStartDateAssociationTagsSortStrategy>().Object,
                _mockRepository.Create<IRecommendationSortStrategy>().Object,
                _mockRepository.Create<IStatisticsSortStrategy>().Object,
                GetSlidingWindowStrategy(assetIds, esOrderByField, definitions, expectedResult).Object);

            var result = sortingByStatsService.ListOrderedIdsWithSortValues(
                documents,
                assetIds,
                definitions,
                esOrderByField);

            result.Should().NotBeNull();
            result.Should().Equal(expectedResult);
        }

        [Test]
        public void ListOrderedIdsWithSortValues_SortByStatistics()
        {
            var esOrderByField = new EsOrderByStatisticsField(OrderBy.VIEWS, OrderDir.DESC, default);
            var definitions = new UnifiedSearchDefinitions { groupId = 1483 };
            var documents = Enumerable.Empty<ElasticSearchApi.ESAssetDocument>();
            var assetIds = Enumerable.Empty<long>();

            var expectedResult = Enumerable.Empty<(long id, string sortValue)>();

            var sortingByStatsService = new SortingByStatsService(
                _mockRepository.Create<IStartDateAssociationTagsSortStrategy>().Object,
                _mockRepository.Create<IRecommendationSortStrategy>().Object,
                GetStatisticsStrategy(assetIds, esOrderByField, definitions.groupId, expectedResult).Object,
                _mockRepository.Create<ISlidingWindowOrderStrategy>().Object);

            var result = sortingByStatsService.ListOrderedIdsWithSortValues(
                documents,
                assetIds,
                definitions,
                esOrderByField);

            result.Should().NotBeNull();
            result.Should().Equal(expectedResult);
        }

        [Test]
        public void ListOrderedIdsWithSortValues_SortByRecommendations()
        {
            var esOrderByField = new EsOrderByField(OrderBy.RECOMMENDATION, OrderDir.DESC);
            var definitions = new UnifiedSearchDefinitions { groupId = 1483 };
            var documents = Enumerable.Empty<ElasticSearchApi.ESAssetDocument>();
            var assetIds = Enumerable.Empty<long>();

            var expectedResult = Enumerable.Empty<(long id, string sortValue)>();

            var sortingByStatsService = new SortingByStatsService(
                _mockRepository.Create<IStartDateAssociationTagsSortStrategy>().Object,
                GetRecommendationSortStrategy(assetIds, definitions, expectedResult).Object,
                _mockRepository.Create<IStatisticsSortStrategy>().Object,
                _mockRepository.Create<ISlidingWindowOrderStrategy>().Object);

            var result = sortingByStatsService.ListOrderedIdsWithSortValues(
                documents,
                assetIds,
                definitions,
                esOrderByField);

            result.Should().NotBeNull();
            result.Should().Equal(expectedResult);
        }

        private Mock<IStartDateAssociationTagsSortStrategy> GetStartDateStrategy(
            IEnumerable<ElasticSearchApi.ESAssetDocument> documents,
            OrderDir direction,
            UnifiedSearchDefinitions definitions,
            IEnumerable<(long id, string sortValue)> result)
        {
            var startSateStrategyMock = _mockRepository.Create<IStartDateAssociationTagsSortStrategy>();
            startSateStrategyMock
                .Setup(x => x.SortAssetsByStartDate(documents, direction, definitions))
                .Returns(result);

            return startSateStrategyMock;
        }

        private Mock<IRecommendationSortStrategy> GetRecommendationSortStrategy(
            IEnumerable<long> assetIds,
            UnifiedSearchDefinitions definitions,
            IEnumerable<(long id, string sortValue)> result)
        {
            var recommendationStrategyMock = _mockRepository.Create<IRecommendationSortStrategy>();
            recommendationStrategyMock
                .Setup(x => x.Sort(assetIds, definitions))
                .Returns(result);

            return recommendationStrategyMock;
        }

        private Mock<IStatisticsSortStrategy> GetStatisticsStrategy(
            IEnumerable<long> assetIds,
            EsOrderByStatisticsField esOrderByField,
            int partnerId,
            IEnumerable<(long id, string sortValue)> result)
        {
            var statisticsSortStrategy = _mockRepository.Create<IStatisticsSortStrategy>();
            statisticsSortStrategy
                .Setup(x => x.SortAssetsByStatsWithSortValues(assetIds, esOrderByField, partnerId))
                .Returns(result);

            return statisticsSortStrategy;
        }

        private Mock<ISlidingWindowOrderStrategy> GetSlidingWindowStrategy(
            IEnumerable<long> assetIds,
            EsOrderBySlidingWindow esOrderByField,
            UnifiedSearchDefinitions definitions,
            IEnumerable<(long id, string sortValue)> result)
        {
            var slidingWindowStrategyMock = _mockRepository.Create<ISlidingWindowOrderStrategy>();
            slidingWindowStrategyMock
                .Setup(x => x.Sort(assetIds, definitions, esOrderByField))
                .Returns(result);

            return slidingWindowStrategyMock;
        }

        private static IEnumerable<TestCaseData> NotImplementedData()
        {
            yield return new TestCaseData(new EsOrderByField(OrderBy.NAME, OrderDir.DESC));
            yield return new TestCaseData(new EsOrderByField(OrderBy.NAME, OrderDir.ASC));
            yield return new TestCaseData(new EsOrderByField(OrderBy.START_DATE, OrderDir.DESC));
            yield return new TestCaseData(new EsOrderByField(OrderBy.CREATE_DATE, OrderDir.DESC));
            yield return new TestCaseData(new EsOrderByField(OrderBy.EPG_ID, OrderDir.DESC));
            yield return new TestCaseData(new EsOrderByField(OrderBy.MEDIA_ID, OrderDir.DESC));
            yield return new TestCaseData(new EsOrderByField(OrderBy.ID, OrderDir.DESC));
            yield return new TestCaseData(new EsOrderByField(OrderBy.RELATED, OrderDir.DESC));
            yield return new TestCaseData(new EsOrderByMetaField("meta", OrderDir.DESC, default, null, null));
        }
    }
}