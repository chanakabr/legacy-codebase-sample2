using System.Data;
using ApiObjects;

namespace DAL
{
    public interface IDeviceBrandDal
    {
        DataSet Add(long groupId, DeviceBrand deviceBrand, long updaterId);
        DataSet Update(long groupId, DeviceBrand deviceBrand, long updaterId);
        DataSet List(long groupId);
    }
}