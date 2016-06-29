using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI.Models.Users
{
    [Serializable]
    public enum KalturaUserAssetsListType
    {
        ALL,
        WATCH,
        PURCHASE,
        LIBRARY,
    }
}