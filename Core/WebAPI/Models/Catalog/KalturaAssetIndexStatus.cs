using System;

namespace WebAPI.Models.Catalog
{
    [Serializable]
    public enum KalturaAssetIndexStatus
    {
        Ok = 0,
        Deleted = 1,
        NotUpdated = 2
    }
}