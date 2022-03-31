using ApiObjects;
using ApiObjects.Response;

namespace ApiLogic.Api.Managers
{
    public interface IDeviceFamilyManager
    {
        GenericResponse<DeviceFamily> Add(long groupId, DeviceFamily deviceFamily, long updaterId);
        GenericResponse<DeviceFamily> Update(long groupId, DeviceFamily deviceFamily, long updaterId);
        GenericListResponse<DeviceFamily> List(long groupId, long? id, string name, bool? isSystem, bool orderByIdAsc, int pageIndex, int pageSize);
    }
}