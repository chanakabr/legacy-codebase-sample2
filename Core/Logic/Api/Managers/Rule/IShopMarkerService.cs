using ApiObjects;
using ApiObjects.Response;
using ApiObjects.Rules;
using Core.Catalog;

namespace ApiLogic.Api.Managers.Rule
{
    public interface IShopMarkerService
    {
        GenericResponse<Topic> GetShopMarkerTopic(long groupId);
        Status SetShopMarkerMeta(long groupId, Asset asset, AssetUserRule assetUserRule);
    }
}