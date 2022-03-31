using ApiObjects;
using ApiObjects.Response;

namespace ApiLogic.Repositories
{
    public interface IDeviceBrandRepository
    {
        GenericResponse<DeviceBrand> Add(long groupId, DeviceBrand deviceBrand, long updaterId);
        GenericResponse<DeviceBrand> Update(long groupId, DeviceBrand deviceBrand, long updaterId);
        GenericListResponse<DeviceBrand> List(long groupId);
    }
}