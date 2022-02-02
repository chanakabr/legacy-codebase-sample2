using System.Collections.Generic;
using ApiObjects.SearchObjects;
using ElasticSearch.Searcher;

namespace ApiLogic.IndexManager.Sorting
{
    public interface ISlidingWindowOrderStrategy
    {
        IEnumerable<(long id, string sortValue)> Sort(
            IEnumerable<long> assetIds,
            UnifiedSearchDefinitions unifiedSearchDefinitions,
            EsOrderBySlidingWindow esOrderByField);
    }
}