using WebAPI.Models.Catalog;
using WebAPI.Models.General;

namespace WebAPI.ObjectsConvertor.Extensions
{
    public static class ChannelMapper
    {
        public static int[] GetAssetTypes(this KalturaChannel model)
        {
            if (model.AssetTypes == null && model.MediaTypes != null)
                model.AssetTypes = model.MediaTypes;

            if (model.AssetTypes == null)
                return new int[0];

            int[] assetTypes = new int[model.AssetTypes.Count];
            for (int i = 0; i < model.AssetTypes.Count; i++)
            {
                assetTypes[i] = model.AssetTypes[i].value;
            }

            return assetTypes;
        }
        
        public static void FillEmptyFieldsForUpdate(this KalturaChannel model)
        {
            if (model.NullableProperties != null && model.NullableProperties.Contains("metadata"))
            {
                model.MetaData = new SerializableDictionary<string, KalturaStringValue>();
            }
        }
    }

    public static class DynamicChannelMapper
    {
        public static int[] getAssetTypes(this KalturaDynamicChannel model)
        {
            if (model.AssetTypes == null && model.MediaTypes != null)
                model.AssetTypes = model.MediaTypes;

            if (model.AssetTypes == null)
                return new int[0];

            int[] assetTypes = new int[model.AssetTypes.Count];
            for (int i = 0; i < model.AssetTypes.Count; i++)
            {
                assetTypes[i] = model.AssetTypes[i].value;
            }

            return assetTypes;
        }
    }
}