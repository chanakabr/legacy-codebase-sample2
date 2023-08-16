using System.Collections.Generic;
using ApiObjects.Catalog;
using ApiObjects.Rules;

namespace ApiLogic.Catalog.CatalogManagement.Services
{
    public interface IShopAssetUserRuleResolver
    {
        AssetUserRule ResolveByMediaAsset(
            int groupId,
            string assetType,
            IEnumerable<Metas> metas,
            IEnumerable<Tags> tags);

        AssetUserRule ResolveByMediaAsset(int groupId, IEnumerable<Metas> metas, IEnumerable<Tags> tags);
    }
}