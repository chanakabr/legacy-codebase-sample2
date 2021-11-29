using System.Collections.Generic;
using System.Data;

namespace DAL.SearchPriorityGroups
{
    public interface ISearchPriorityGroupDal
    {
        DataSet Add(long groupId, string documentKey, long updaterId);
        DataSet Get(long groupId, long id);
        DataSet List(long groupId);
        DataSet List(long groupId, IEnumerable<long> ids);
        bool Delete(long groupId, long id, long updaterId);
    }
}