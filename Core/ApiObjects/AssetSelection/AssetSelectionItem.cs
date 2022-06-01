using System;

namespace ApiObjects.AssetSelection
{
    public class AssetSelectionItem
    {
        public long AssetId { get; }
        public eAssetTypes AssetType { get; }
        public DateTime UpdateDate { get; }

        public AssetSelectionItem(long assetId, eAssetTypes assetType, DateTime updateDate)
        {
            AssetId = assetId;
            AssetType = assetType;
            UpdateDate = updateDate;
        }
    }
}