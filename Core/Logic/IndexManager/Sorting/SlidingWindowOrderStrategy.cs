using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ApiObjects.CanaryDeployment.Elasticsearch;
using ApiObjects.SearchObjects;
using ElasticSearch.Searcher;

namespace ApiLogic.IndexManager.Sorting
{
    public class SlidingWindowOrderStrategy : ISlidingWindowOrderStrategy
    {
        private static readonly Lazy<ISlidingWindowOrderStrategy> LazyInstanceV2 = new Lazy<ISlidingWindowOrderStrategy>(
            () => new SlidingWindowOrderStrategy(StatisticsSortStrategy.Instance),
            LazyThreadSafetyMode.PublicationOnly);
        
        private static readonly Lazy<ISlidingWindowOrderStrategy> LazyInstanceV7 = new Lazy<ISlidingWindowOrderStrategy>(
            () => new SlidingWindowOrderStrategy(StatisticsSortStrategyV7.Instance),
            LazyThreadSafetyMode.PublicationOnly);

        private readonly IStatisticsSortStrategy _statisticsSortStrategy;

        public static ISlidingWindowOrderStrategy Instance(ElasticsearchVersion version)
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
