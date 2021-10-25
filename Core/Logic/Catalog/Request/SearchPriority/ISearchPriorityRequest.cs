using System.Collections.Generic;
using ApiObjects.SearchPriority;

namespace Core.Catalog.Request.SearchPriority
{
    public interface ISearchPriorityRequest
    {
        IReadOnlyDictionary<double, SearchPriorityGroup> PriorityGroupsMappings { get; set; }
    }
}
