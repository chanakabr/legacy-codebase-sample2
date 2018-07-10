using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI.Models.Catalog
{
    [Serializable]
    public enum KalturaMediaFileTypeQuality
    {        
        ADAPTIVE,
        SD,
        HD_720,
        HD_1080,       
        UHD_4K
    }
}