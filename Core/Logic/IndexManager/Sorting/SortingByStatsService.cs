using System;
using System.Collections.Generic;
using System.Linq;
using ApiLogic.IndexManager.Models;
using ApiObjects.CanaryDeployment.Elasticsearch;
using ApiObjects.SearchObjects;
using ElasticSearch.Searcher;

namespace ApiLogic.IndexManager.Sorting
{
    public class SortingByStatsService : ISortingByStatsService
    {
        private readonly IStartDateAssociationTagsSortStrategy _startDateAssociationTagsSortStrategy;
        private readonly IRecommendationSortStrategy _recommendationSortStrategy;
        private readonly IStatisticsSortStrategy _statisticsSortStrategy;
        private readonly ISlidingWindowOrderStrategy _slidingWindowOrderStrategy;

        private static readonly Lazy<ISortingByStatsService> LazyInstanceV2 = new Lazy<ISortingByStatsService>(
            () => new SortingByStatsService(
                StartDateAssociationTagsSortStrategy.Instance,
                RecommendationSortStrategy.Instance,
                StatisticsSortStrategy.Instance,
                SlidingWindowOrderStrategy.Instance(ElasticsearchVersion.ES_2_3)));
        
        private static readonly Lazy<ISortingByStatsService> LazyInstanceV7 = new Lazy<ISortingByStatsService>(
            () => new SortingByStatsService(
                StartDateAssociationTagsSortStrategyV7.Instance,
                RecommendationSortStrategy.Instance,
                StatisticsSortStrategyV7.Instance,
                SlidingWindowOrderStrategy.Instance(ElasticsearchVersion.ES_7)));
        
        public static ISortingByStatsService Instance(ElasticsearchVersion version)
        {
            switch (version)
            {
                case ElasticsearchVersion.ES_2_3:
                    return LazyInstanceV2.Value;
                case ElasticsearchVersion.ES_7:
                    return LazyInstanceV7.Value;
                default:
                    throw new ArgumentOutOfRangeException(nameof(version), version, null);
            }
        }

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
            IEnumerable<ExtendedUnifiedSearchResult> extendedUnifiedSearchResults,
            UnifiedSearchDefinitions unifiedSearchDefinitions,
            IEsOrderByField orderByField)
        {
            var assetIds = extendedUnifiedSearchResults.Select(x => x.AssetId).ToArray();

            if (orderByField is EsOrderBySlidingWindow orderBySlidingWindowField)
            {
                return _slidingWindowOrderStrategy.Sort(assetIds, unifiedSearchDefinitions, orderBySlidingWindowField);
            }

            // Do special sort only when searching by media
            if (orderByField is EsOrderByStartDateAndAssociationTags)
            {
                return _startDateAssociationTagsSortStrategy.SortAssetsByStartDate(
                    extendedUnifiedSearchResults,
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
