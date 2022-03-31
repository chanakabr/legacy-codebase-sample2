using System.Data;
using ApiObjects;

namespace DAL
{
    public interface IDeviceFamilyDal
    {
        DataSet Add(long groupId, DeviceFamily deviceFamily, long updaterId);
        DataSet Update(long groupId, DeviceFamily deviceFamily, long updaterId);
        DataSet GetByDeviceBrandId(long groupId, long deviceBrandId);
        DataSet List(long groupId);
    }
}