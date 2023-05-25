using System;
using WebAPI.Models.Catalog;

namespace WebAPI.ObjectsConvertor.Extensions
{
    public static class AssetGroupByMapper
    {
        public static string GetValue(this KalturaAssetGroupBy model)
        {
            switch (model)
            {
                case KalturaAssetFieldGroupBy m: return m.Value.ToString();
                case KalturaAssetMetaOrTagGroupBy m: return m.Value;
                default: throw new NotImplementedException($"GetValue for {model.objectType} is not implemented");
            }
        }
    }
}
