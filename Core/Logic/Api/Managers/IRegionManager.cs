using ApiObjects;
using ApiObjects.Response;

namespace ApiLogic.Api.Managers
{
    public interface IRegionManager
    {
        GenericResponse<Region> GetRegion(long groupId, long regionId);
    }
}