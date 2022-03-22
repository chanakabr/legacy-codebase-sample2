using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ApiObjects.SearchObjects;
using ElasticSearch.Searcher;

namespace ApiLogic.IndexManager.Sorting
{
    public class SlidingWindowOrderStrategy : ISlidingWindowOrderStrategy
    {
        private static readonly Lazy<ISlidingWindowOrderStrategy> LazyInstance = new Lazy<ISlidingWindowOrderStrategy>(
            () => new SlidingWindowOrderStrategy(StatisticsSortStrategy.Instance),
            LazyThreadSafetyMode.PublicationOnly);

        private readonly IStatisticsSortStrategy _statisticsSortStrategy;

        public static ISlidingWindowOrderStrategy Instance => LazyInstance.Value;

        public SlidingWindowOrderStrategy(IStatisticsSortStrategy statisticsSortStrategy)
        {
            _statisticsSortStrategy = statisticsSortStrategy;
        }

        public IEnumerable<(long id, string sortValue)> Sort(
            IEnumerable<long> assetIds,
            UnifiedSearchDefinitions unifiedSearchDefinitions,
            EsOrderBySlidingWindow esOrderByField)
        {
            var orderBy = GetMappedOrderBy(esOrderByField.OrderByField);
            if (!orderBy.HasValue)
            {
                var increment = 0;

                return assetIds.Select(x => (x, (++increment).ToString()));
            }

            DateTime? timeWindowEnd = null;
            DateTime? timeWindowStart = null;

            if (esOrderByField.SlidingWindowPeriod > 0)
            {
                timeWindowEnd = DateTime.UtcNow;
                timeWindowStart = timeWindowEnd.Value.AddMinutes(-esOrderByField.SlidingWindowPeriod);
            }

            return _statisticsSortStrategy.SortAssetsByStatsWithSortValues(
                assetIds,
                esOrderByField.OrderByField,
                OrderDir.DESC,
                unifiedSearchDefinitions.groupId,
                timeWindowStart,
                timeWindowEnd);
        }

        private static OrderBy? GetMappedOrderBy(OrderBy orderBy)
        {
            switch (orderBy)
            {
                case OrderBy.VIEWS:
                case OrderBy.LIKE_COUNTER:
                    return orderBy;
                case OrderBy.VOTES_COUNT:
                case OrderBy.RATING:
                    return OrderBy.RATING;
                default:
                    return null;
            }
        }
    }
}