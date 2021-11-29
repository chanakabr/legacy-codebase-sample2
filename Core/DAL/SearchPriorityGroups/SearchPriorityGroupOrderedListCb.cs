using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace DAL.SearchPriorityGroups
{
    public class SearchPriorityGroupOrderedListCb
    {
        [JsonProperty("priorityGroupIds")]
        public long[] PriorityGroupIds { get; set; }

        public SearchPriorityGroupOrderedListCb(IEnumerable<long> priorityGroupIds)
        {
            PriorityGroupIds = priorityGroupIds.ToArray();
        }
    }
}