using System;

namespace WebAPI.Models.Catalog
{
    [Serializable]
    public enum KalturaLinearChannelType
    {
        UNKNOWN = 0,
        DTT = 1,
        OTT = 2,
        DTT_AND_OTT = 3,
        VRM_EXPORT = 4
    }
}