using ApiObjects;
using ApiObjects.Response;

namespace ApiLogic.Api.Managers
{
    public interface IDeviceBrandManager
    {
        GenericResponse<DeviceBrand> Add(long groupId, DeviceBrand deviceBrand, long updaterId);
        GenericResponse<DeviceBrand> Update(long groupId, DeviceBrand deviceBrand, long updaterId);
        GenericListResponse<DeviceBrand> List(long groupId, long? id, long? deviceFamilyId, string name, bool? isSystem, bool orderByIdAsc, int pageIndex, int pageSize);
    }
}