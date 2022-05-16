using ApiObjects;
using ApiObjects.Catalog;
using ApiObjects.Response;
using ApiObjects.Rules;
using Core.Catalog;

namespace ApiLogic.Api.Managers.Rule
{
    public interface IShopMarkerService
    {
        GenericResponse<Topic> GetShopMarkerTopic(long groupId);
        Status SetShopMarkerMeta(long groupId, AssetStruct assetStruct, Asset asset, AssetUserRule assetUserRule);
    }
}