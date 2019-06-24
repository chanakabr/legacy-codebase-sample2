using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

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