using System.Collections.Generic;
using ApiObjects.SearchObjects;
using ElasticSearch.Common;
using ElasticSearch.Searcher;

namespace ApiLogic.IndexManager.Sorting
{
    public interface ISortingByStatsService
    {
        IEnumerable<(long id, string sortValue)> ListOrderedIdsWithSortValues(
            IEnumerable<ElasticSearchApi.ESAssetDocument> assetsDocumentsDecoded,
            IEnumerable<long> assetIds,
            UnifiedSearchDefinitions unifiedSearchDefinitions,
            IEsOrderByField orderByField);
    }
}
