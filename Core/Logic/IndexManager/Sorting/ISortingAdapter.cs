using System.Collections.Generic;
using ApiObjects.SearchObjects;

namespace ApiLogic.IndexManager.Sorting
{
    public interface ISortingAdapter
    {
        IReadOnlyCollection<IEsOrderByField> ResolveOrdering(UnifiedSearchDefinitions definitions);
    }
}