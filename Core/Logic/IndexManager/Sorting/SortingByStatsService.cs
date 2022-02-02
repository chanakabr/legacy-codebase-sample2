using System;
using System.Collections.Generic;
using ApiObjects.SearchObjects;
using ElasticSearch.Common;
using ElasticSearch.Searcher;

namespace ApiLogic.IndexManager.Sorting
{
    public class SortingByStatsService : ISortingByStatsService
    {
        private readonly IStartDateAssociationTagsSortStrategy _startDateAssociationTagsSortStrategy;
        private readonly IRecommendationSortStrategy _recommendationSortStrategy;
        private readonly IStatisticsSortStrategy _statisticsSortStrategy;
        private readonly ISlidingWindowOrderStrategy _slidingWindowOrderStrategy;

        private static readonly Lazy<ISortingByStatsService> LazyValue = new Lazy<ISortingByStatsService>(
            () => new SortingByStatsService(
                StartDateAssociationTagsSortStrategy.Instance,
                RecommendationSortStrategy.Instance,
                StatisticsSortStrategy.Instance,
                SlidingWindowOrderStrategy.Instance));

        public static ISortingByStatsService Instance => LazyValue.Value;

        public SortingByStatsService(
            IStartDateAssociationTagsSortStrategy startDateAssociationTagsSortStrategy,
            IRecommendationSortStrategy recommendationSortStrategy,
            IStatisticsSortStrategy statisticsSortStrategy,
            ISlidingWindowOrderStrategy slidingWindowOrderStrategy)
        {
            _startDateAssociationTagsSortStrategy = startDateAssociationTagsSortStrategy;
            _recommendationSortStrategy = recommendationSortStrategy;
            _statisticsSortStrategy = statisticsSortStrategy;
            _slidingWindowOrderStrategy = slidingWindowOrderStrategy;
        }
        
        public IEnumerable<(long id, string sortValue)> ListOrderedIdsWithSortValues(
            IEnumerable<ElasticSearchApi.ESAssetDocument> assetsDocumentsDecoded,
            IEnumerable<long> assetIds,
            UnifiedSearchDefinitions unifiedSearchDefinitions,
            IEsOrderByField orderByField)
        {
            if (orderByField is EsOrderBySlidingWindow orderBySlidingWindowField)
            {
                return _slidingWindowOrderStrategy.Sort(assetIds, unifiedSearchDefinitions, orderBySlidingWindowField);
            }

            // Do special sort only when searching by media
            if (orderByField is EsOrderByStartDateAndAssociationTags)
            {
                return _startDateAssociationTagsSortStrategy.SortAssetsByStartDate(
                    assetsDocumentsDecoded,
                    orderByField.OrderByDirection,
                    unifiedSearchDefinitions);
            }

            if (orderByField is EsOrderByStatisticsField orderByStatisticsField)
            {
                return _statisticsSortStrategy.SortAssetsByStatsWithSortValues(
                    assetIds,
                    orderByStatisticsField,
                    unifiedSearchDefinitions.groupId);
            }

            if (orderByField is EsOrderByField field && field.OrderByField == OrderBy.RECOMMENDATION)
            {
                return _recommendationSortStrategy.Sort(assetIds, unifiedSearchDefinitions);
            }

            throw new NotImplementedException("Sorting strategy is not implemented");
        }
    }
}
