using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

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
        IMAGE_TYPE
    }
}