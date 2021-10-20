using ApiLogic.Catalog;
using ApiObjects.Response;

namespace Core.Catalog.CatalogManagement
{
    public interface ILineupService
    {
        GenericListResponse<LineupChannelAsset> GetLineupChannelAssets(long groupId, long regionId, UserSearchContext searchContext, int pageIndex, int pageSize);
    }
}