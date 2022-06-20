using System.Collections.Generic;
using ApiLogic.IndexManager.Models;
using ApiObjects.SearchObjects;

namespace ApiLogic.IndexManager.Sorting
{
    public interface ISortingByStatsService
    {
        IEnumerable<(long id, string sortValue)> ListOrderedIdsWithSortValues(
            IEnumerable<ExtendedUnifiedSearchResult> extendedUnifiedSearchResults,
            UnifiedSearchDefinitions unifiedSearchDefinitions,
            IEsOrderByField orderByField);
    }
}
