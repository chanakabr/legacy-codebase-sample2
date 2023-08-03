using ApiObjects.Lineup;
using ApiObjects.Response;
using Core.Catalog;

namespace ApiLogic.Catalog.CatalogManagement.Services
{
    public interface ILineupService
    {
        GenericResponse<LineupChannelAssetResponse> GetLineupChannelAssets(long groupId, long regionId, UserSearchContext searchContext, int pageIndex, int pageSize);

        GenericListResponse<LineupChannelAsset> GetLineupChannelAssetsWithFilter(
            UserSearchContext userSearchContext,
            LineupRegionalChannelRequest request);
    }
}
