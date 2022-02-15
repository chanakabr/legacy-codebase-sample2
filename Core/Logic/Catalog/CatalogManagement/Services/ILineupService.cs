using ApiObjects.Response;
using Core.Catalog;

namespace ApiLogic.Catalog.CatalogManagement.Services
{
    public interface ILineupService
    {
        GenericListResponse<LineupChannelAsset> GetLineupChannelAssets(long groupId, long regionId, UserSearchContext searchContext, int pageIndex, int pageSize);
    }
}