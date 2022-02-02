using System;
using System.Collections.Generic;
using ApiObjects.SearchObjects;
using ElasticSearch.Searcher;

namespace ApiLogic.IndexManager.Sorting
{
    public interface IStatisticsSortStrategy
    {
        IEnumerable<(long id, string sortValue)> SortAssetsByStatsWithSortValues(
            IEnumerable<long> assetIds,
            EsOrderByStatisticsField esOrderByField,
            int partnerId);

        IEnumerable<(long id, string sortValue)> SortAssetsByStatsWithSortValues(
            IEnumerable<long> assetIds,
            OrderBy orderBy,
            OrderDir orderDirection,
            int partnerId,
            DateTime? startDate = null,
            DateTime? endDate = null);

        IEnumerable<long> SortAssetsByStats(
            IEnumerable<long> assetIds,
            EsOrderByStatisticsField esOrderByField,
            int partnerId);

        IEnumerable<long> SortAssetsByStats(
            IEnumerable<long> assetIds,
            OrderBy orderBy,
            OrderDir orderDirection,
            int partnerId,
            DateTime? startDate = null,
            DateTime? endDate = null);
    }
}
