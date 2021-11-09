using System.Collections.Generic;
using ApiObjects.SearchObjects;

namespace ApiLogic.IndexManager.Sorting
{
    public interface IRecommendationSortStrategy
    {
        IEnumerable<long> Sort(IEnumerable<long> assetIds, UnifiedSearchDefinitions unifiedSearchDefinitions);
    }
}
