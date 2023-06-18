using WebAPI.Models.Catalog;

namespace WebAPI.ObjectsConvertor.Extensions
{
    public static class AssetInfoMapper
    {
        public static int getType(this KalturaBaseAssetInfo model)
        {
            return model.Type.HasValue ? (int)model.Type : 0;
        }
    }
}
