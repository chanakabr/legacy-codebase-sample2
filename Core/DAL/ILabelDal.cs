using System.Data;

namespace DAL
{
    public interface ILabelDal
    {
        DataSet Add(long groupId, int attributeId, string value, long updaterId);
        DataSet Update(long groupId, long id, string value, long updaterId);
        bool Delete(long groupId, long id, long updaterId);
        DataSet Get(long groupId);
    }
}
