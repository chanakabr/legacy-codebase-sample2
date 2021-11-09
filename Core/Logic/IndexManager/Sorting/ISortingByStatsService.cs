using System.Collections.Generic;
using ApiObjects.SearchObjects;
using ElasticSearch.Common;

namespace ApiLogic.IndexManager.Sorting
{
    public interface ISortingByStatsService
    {
        IEnumerable<long> ListOrderedIds(
            IEnumerable<ElasticSearchApi.ESAssetDocument> assetsDocumentsDecoded,
            IEnumerable<long> assetIds,
            bool shouldSortByStartDateOfAssociationTagsAndParentMedia,
            UnifiedSearchDefinitions unifiedSearchDefinitions,
            OrderDir orderDir,
            OrderBy orderBy,
            int partnerId);
    }
}
