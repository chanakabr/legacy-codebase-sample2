using System.Collections.Generic;
using AdapterControllers.RecommendationEngineAdapter;
using WebAPI.Models.Catalog;
using WebAPI.Models.ConditionalAccess.FilterActions.Files;

namespace WebAPI.ObjectsConvertor.Extensions
{
    public static class FilterFileByFileTypeIdForAssetTypeActionMapper
    {
        public static List<eAssetTypes> GetAssetTypes(this KalturaFilterFileByFileTypeIdForAssetTypeAction model)
        {
            var types = model.GetItemsIn<List<KalturaAssetType>, KalturaAssetType>(model.AssetTypeIn, "assetTypeIn", true, true);
            var mapped = AutoMapper.Mapper.Map<List<eAssetTypes>>(types);
            return mapped;
        }
    }
}