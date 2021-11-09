using System.Collections.Generic;
using ApiObjects.SearchObjects;
using ElasticSearch.Common;

namespace ApiLogic.IndexManager.Sorting
{
    public interface IStartDateAssociationTagsSortStrategy
    {
        List<long> SortAssetsByStartDate(
            IEnumerable<ElasticSearchApi.ESAssetDocument> assets,
            OrderDir orderDirection,
            Dictionary<int, string> associationTags,
            Dictionary<int, int> mediaTypeParent,
            int partnerId);
    }
}
