using System.Collections.Generic;
using ApiLogic.IndexManager.Models;
using ApiObjects;
using ApiObjects.SearchObjects;

namespace ApiLogic.IndexManager.Sorting
{
    public interface IStartDateAssociationTagsSortStrategy
    {
        IEnumerable<(long id, string sortValue)> SortAssetsByStartDate(
            IEnumerable<ExtendedUnifiedSearchResult> extendedUnifiedSearchResults,
            LanguageObj language,
            OrderDir orderDirection,
            Dictionary<int, string> associationTags,
            Dictionary<int, int> mediaTypeParent,
            int partnerId);

        IEnumerable<(long id, string sortValue)> SortAssetsByStartDate(
            IEnumerable<ExtendedUnifiedSearchResult> extendedUnifiedSearchResults,
            OrderDir orderDirection,
            UnifiedSearchDefinitions unifiedSearchDefinitions);
    }
}
