using System.Collections.Generic;
using ApiObjects.SearchObjects;
using ElasticSearch.Common;

namespace ApiLogic.IndexManager.Sorting
{
    public interface IStartDateAssociationTagsSortStrategy
    {
        IEnumerable<(long id, string sortValue)> SortAssetsByStartDate(
            IEnumerable<ElasticSearchApi.ESAssetDocument> assets,
            OrderDir orderDirection,
            Dictionary<int, string> associationTags,
            Dictionary<int, int> mediaTypeParent,
            int partnerId);

        IEnumerable<(long id, string sortValue)> SortAssetsByStartDate(
            IEnumerable<ElasticSearchApi.ESAssetDocument> assets,
            OrderDir orderDirection,
            UnifiedSearchDefinitions unifiedSearchDefinitions);
    }
}
