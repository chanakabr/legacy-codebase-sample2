using ApiObjects;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.Catalog;

namespace WebAPI.Managers
{
    public static class AssetTypeMapper
    {
        public static eAssetTypes ToEAssetType(int? assetTypeId)
        {
            switch (assetTypeId)
            {
                case null: return eAssetTypes.UNKNOWN;
                case 0: return eAssetTypes.EPG;
                case 1: return eAssetTypes.NPVR;
                default: return eAssetTypes.MEDIA;
            }
        }

        public static eAssetTypes ToEAssetType(KalturaAssetType assetType)
        {
            switch (assetType)
            {
                case KalturaAssetType.media:
                    return eAssetTypes.MEDIA;
                case KalturaAssetType.recording:
                    return eAssetTypes.NPVR;
                case KalturaAssetType.epg:
                    return eAssetTypes.EPG;
                default:
                    throw new ClientException((int)StatusCode.UnknownEnumValue, $"Unknown Asset Type: {assetType}.");
            }
        }
    }
}