using System.Collections.Generic;
using ApiObjects.SearchObjects;
using Core.Catalog.Response;
using ElasticSearch.Common;

namespace ApiLogic.IndexManager.Sorting
{
    public interface ISortingService
    {
        IReadOnlyCollection<long> GetReorderedAssetIds(
            IEnumerable<UnifiedSearchResult> searchResults,
            UnifiedSearchDefinitions definitions,
            IDictionary<string, ElasticSearchApi.ESAssetDocument> assetIdToDocument);
    }
}