using System;

namespace WebAPI.Models.Catalog
{
    [Serializable]
    public enum KalturaImageObjectType
    {
        MEDIA_ASSET,
        PROGRAM_ASSET,
        CHANNEL,
        CATEGORY,
        PARTNER,
        IMAGE_TYPE,        
        PROGRAM_GROUP
    }
}