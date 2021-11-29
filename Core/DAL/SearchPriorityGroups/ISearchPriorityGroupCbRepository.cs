using System.Collections.Generic;

namespace DAL.SearchPriorityGroups
{
    public interface ISearchPriorityGroupCbRepository
    {
        string Save(long groupId, SearchPriorityGroupCb searchPriorityGroupCb);
        string Save(long groupId, string documentKey, SearchPriorityGroupCb searchPriorityGroupCb);
        SearchPriorityGroupCb Get(long groupId, string documentKey);
        IDictionary<string, SearchPriorityGroupCb> List(long groupId, IEnumerable<string> documentKeys);
        bool Delete(long groupId, string documentKey);
    }
}