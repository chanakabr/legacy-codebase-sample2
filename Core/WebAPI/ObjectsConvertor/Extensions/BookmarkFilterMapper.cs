using System;
using System.Collections.Generic;
using System.Text;
using WebAPI.Models.Catalog;

namespace WebAPI.ObjectsConvertor.Extensions
{
    public static class BookmarkFilterMapper
    {
        internal static List<KalturaSlimAsset> getAssetIn(this KalturaBookmarkFilter model)
        {
            if (model.AssetIn != null && model.AssetIn.Count > 0)
                return model.AssetIn;

            if (string.IsNullOrEmpty(model.AssetIdIn))
                return null;

            List<KalturaSlimAsset> values = new List<KalturaSlimAsset>();
            string[] stringValues = model.AssetIdIn.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string value in stringValues)
            {
                KalturaSlimAsset asset = new KalturaSlimAsset();
                asset.Id = value;
                asset.Type = model.AssetTypeEqual.Value;
                values.Add(asset);
            }

            return values;
        }
    }
}
