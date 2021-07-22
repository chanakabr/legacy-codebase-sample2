using System.Data;

namespace DAL
{
    public class LabelDal : ILabelDal
    {
        public DataSet Add(long groupId, int attributeId, string value, long updaterId)
        {
            var sp = new ODBCWrapper.StoredProcedure("Insert_Label");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@GroupId", groupId);
            sp.AddParameter("@EntityAttribute", attributeId);
            sp.AddParameter("@Value", value);
            sp.AddParameter("@UpdaterId", updaterId);

            return sp.ExecuteDataSet();
        }

        public DataSet Update(long groupId, long id, string value, long updaterId)
        {
            var sp = new ODBCWrapper.StoredProcedure("Update_Label");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@GroupId", groupId);
            sp.AddParameter("@Id", id);
            sp.AddParameter("@Value", value);
            sp.AddParameter("@UpdaterId", updaterId);

            return sp.ExecuteDataSet();
        }

        public bool Delete(long groupId, long id, long updaterId)
        {
            var sp = new ODBCWrapper.StoredProcedure("Delete_Label");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@GroupId", groupId);
            sp.AddParameter("@Id", id);
            sp.AddParameter("@UpdaterId", updaterId);

            return sp.ExecuteReturnValue<int>() > 0;
        }

        public DataSet Get(long groupId)
        {
            var sp = new ODBCWrapper.StoredProcedure("Get_Labels");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@GroupId", groupId);

            return sp.ExecuteDataSet();
        }
    }
}
