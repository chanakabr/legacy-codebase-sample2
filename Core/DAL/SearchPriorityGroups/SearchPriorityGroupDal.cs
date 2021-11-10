using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;

namespace DAL.SearchPriorityGroups
{
    public class SearchPriorityGroupDal : ISearchPriorityGroupDal
    {
        private static readonly Lazy<SearchPriorityGroupDal> Lazy = new Lazy<SearchPriorityGroupDal>(() => new SearchPriorityGroupDal(), LazyThreadSafetyMode.PublicationOnly);
        public static SearchPriorityGroupDal Instance => Lazy.Value;

        public DataSet Add(long groupId, string documentKey, long updaterId)
        {
            var sp = new ODBCWrapper.StoredProcedure("Insert_SearchPriorityGroupEntry");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@GroupId", groupId);
            sp.AddParameter("@DocumentKey", documentKey);
            sp.AddParameter("@UpdaterId", updaterId);

            return sp.ExecuteDataSet();
        }

        public DataSet Get(long groupId, long id)
        {
            var sp = new ODBCWrapper.StoredProcedure("Get_SearchPriorityGroupEntriesByIds");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@GroupId", groupId);
            sp.AddIDListParameter("@Ids", new[] { id }, "Id");

            return sp.ExecuteDataSet();
        }

        public DataSet List(long groupId)
        {
            var sp = new ODBCWrapper.StoredProcedure("Get_SearchPriorityGroupEntries");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@GroupId", groupId);

            return sp.ExecuteDataSet();
        }

        public DataSet List(long groupId, IEnumerable<long> ids)
        {
            var sp = new ODBCWrapper.StoredProcedure("Get_SearchPriorityGroupEntriesByIds");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@GroupId", groupId);
            sp.AddIDListParameter("@Ids", ids.ToArray(), "Id");

            return sp.ExecuteDataSet();
        }

        public bool Delete(long groupId, long id, long updaterId)
        {
            var sp = new ODBCWrapper.StoredProcedure("Delete_SearchPriorityGroupEntry");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@GroupId", groupId);
            sp.AddParameter("@Id", id);
            sp.AddParameter("@UpdaterId", updaterId);

            return sp.ExecuteReturnValue<int>() > 0;
        }
    }
}