using System.Collections.Generic;
using ApiObjects.SearchObjects;

namespace ApiLogic.IndexManager.Sorting
{
    public interface IRecommendationSortStrategy
    {
        IEnumerable<(long id, string sortValue)> Sort(IEnumerable<long> assetIds, UnifiedSearchDefinitions unifiedSearchDefinitions);
    }
}
