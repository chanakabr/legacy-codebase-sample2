using System;
using System.Collections.Generic;
using ApiObjects.SearchObjects;

namespace ApiLogic.IndexManager.Sorting
{
    public interface IStatisticsSortStrategy
    {
        IEnumerable<long> SortAssetsByStats(IEnumerable<long> assetIds, OrderBy orderBy, OrderDir orderDir, DateTime? trendingAssetWindow, int partnerId);

        IEnumerable<long> SortAssetsByStats(
            IEnumerable<long> assetIds,
            OrderBy orderBy,
            OrderDir orderDirection,
            int partnerId,
            DateTime? startDate = null,
            DateTime? endDate = null);
    }
}
