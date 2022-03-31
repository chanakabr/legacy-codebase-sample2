using ApiObjects;
using ApiObjects.Response;

namespace ApiLogic.Repositories
{
    public interface IDeviceFamilyRepository
    {
        GenericResponse<DeviceFamily> Add(long groupId, DeviceFamily deviceFamily, long updaterId);
        GenericResponse<DeviceFamily> Update(long groupId, DeviceFamily deviceFamily, long updaterId);
        GenericResponse<DeviceFamily> GetByDeviceBrandId(long groupId, long deviceBrandId);
        GenericListResponse<DeviceFamily> List(long groupId);
    }
}