using System;
using System.Collections.Generic;
using ApiObjects.SearchObjects;
using ElasticSearch.Common;

namespace ApiLogic.IndexManager.Sorting
{
    public class SortingByStatsService : ISortingByStatsService
    {
        private readonly IStartDateAssociationTagsSortStrategy _startDateAssociationTagsSortStrategy;
        private readonly IRecommendationSortStrategy _recommendationSortStrategy;
        private readonly IStatisticsSortStrategy _statisticsSortStrategy;

        private static readonly Lazy<ISortingByStatsService> LazyValue = new Lazy<ISortingByStatsService>(() =>
            new SortingByStatsService(StartDateAssociationTagsSortStrategy.Instance, RecommendationSortStrategy.Instance, StatisticsSortStrategy.Instance));

        public static ISortingByStatsService Instance => LazyValue.Value;

        public SortingByStatsService(IStartDateAssociationTagsSortStrategy startDateAssociationTagsSortStrategy, IRecommendationSortStrategy recommendationSortStrategy, IStatisticsSortStrategy statisticsSortStrategy)
        {
            _startDateAssociationTagsSortStrategy = startDateAssociationTagsSortStrategy;
            _recommendationSortStrategy = recommendationSortStrategy;
            _statisticsSortStrategy = statisticsSortStrategy;
        }
        
        public IEnumerable<long> ListOrderedIds(
            IEnumerable<ElasticSearchApi.ESAssetDocument> assetsDocumentsDecoded,
            IEnumerable<long> assetIds,
            bool shouldSortByStartDateOfAssociationTagsAndParentMedia,
            UnifiedSearchDefinitions unifiedSearchDefinitions,
            OrderDir orderDir,
            OrderBy orderBy,
            int partnerId)
        {
            // Do special sort only when searching by media
            if (shouldSortByStartDateOfAssociationTagsAndParentMedia)
            {
                return _startDateAssociationTagsSortStrategy.SortAssetsByStartDate(
                    assetsDocumentsDecoded,
                    orderDir,
                    unifiedSearchDefinitions.associationTags,
                    unifiedSearchDefinitions.parentMediaTypes,
                    partnerId);
            }
            
            // Recommendation - the order is predefined already. We will use the order that is given to us
            if (orderBy == OrderBy.RECOMMENDATION)
            {
                return _recommendationSortStrategy.Sort(assetIds, unifiedSearchDefinitions);
            }

            return _statisticsSortStrategy.SortAssetsByStats(assetIds, orderBy, orderDir, unifiedSearchDefinitions.trendingAssetWindow, partnerId);
        }
    }
}
