using System.Collections.Generic;
using System.Linq;

namespace ApiObjects.SearchPriorityGroups
{
    public class SearchPriorityGroupOrderedIdsSet
    {
        public IEnumerable<long> PriorityGroupIds { get; set; }

        public SearchPriorityGroupOrderedIdsSet()
            : this(Enumerable.Empty<long>())
        {
        }

        public SearchPriorityGroupOrderedIdsSet(IEnumerable<long> priorityGroupIds)
        {
            PriorityGroupIds = priorityGroupIds ?? Enumerable.Empty<long>();
        }
    }
}