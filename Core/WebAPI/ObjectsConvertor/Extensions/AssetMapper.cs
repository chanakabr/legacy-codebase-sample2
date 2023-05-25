using WebAPI.Models.Catalog;

namespace WebAPI.ObjectsConvertor.Extensions
{
    public static class AssetMapper
    {
        public static int getType(this KalturaAsset model)
        {
            return model.Type.HasValue ? model.Type.Value : 0;
        }
    }
}
