using System.Collections.Generic;
using ApiLogic.Catalog.IndexManager.GroupBy;
using ApiLogic.IndexManager.Models;
using ApiObjects.SearchObjects;
using Core.Catalog.Response;

namespace ApiLogic.IndexManager.Sorting
{
    public interface ISortingService
    {
        IReadOnlyCollection<long> GetReorderedAssetIds(
            UnifiedSearchDefinitions definitions,
            IEnumerable<ExtendedUnifiedSearchResult> extendedUnifiedSearchResults);
        
        IReadOnlyCollection<UnifiedSearchResult> GetReorderedAssets(
            UnifiedSearchDefinitions definitions,
            IEnumerable<ExtendedUnifiedSearchResult> extendedUnifiedSearchResults);

        bool IsSortingCompleted(UnifiedSearchDefinitions definitions);

        IGroupBySearch GetGroupBySortingStrategy(IReadOnlyCollection<IEsOrderByField> orderByFields);
        bool IsSortingCompatibleWithGroupBy(IReadOnlyCollection<IEsOrderByField> esOrderByFields);
    }
}
