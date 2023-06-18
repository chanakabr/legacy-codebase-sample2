using System;

namespace WebAPI.Models.Catalog
{
    [Serializable]
    public enum KalturaMediaFileTypeQuality
    {
        ADAPTIVE = 1,
        SD = 2,
        HD_720 = 3,
        HD_1080 = 4,
        UHD_4K = 5
    }
}