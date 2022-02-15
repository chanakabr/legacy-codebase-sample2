using System.Collections.Generic;
using ApiObjects;
using ApiObjects.Response;

namespace ApiLogic.Api.Managers
{
    public interface IRegionManager
    {
        GenericResponse<Region> GetRegion(long groupId, long regionId);
        GenericListResponse<Region> GetRegions(int groupId, RegionFilter filter, int pageIndex = 0, int pageSize = 0);
        GenericResponse<Region> UpdateRegion(int groupId, Region regionToUpdate, long userId);
        IReadOnlyDictionary<long, List<int>> GetLinearMediaToRegionsMapWhenEnabled(int groupId);
        Dictionary<long, List<int>> GetLinearMediaRegions(int groupId);
        long? GetDefaultRegionId(int groupId);
        GenericResponse<Region> AddRegion(int groupId, Region region, long userId);
        Status BulkUpdateRegions(
            int groupId,
            long userId,
            long linearChannelId,
            IReadOnlyCollection<RegionChannelNumber> regionChannelNumbers);
        List<int> GetRegionIds(int groupId);
        List<long> GetChildRegionIds(int groupId, long parentRegionId);
    }
}