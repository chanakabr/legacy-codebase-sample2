using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace WebAPI.Models.Catalog
{
    public enum KalturaAssetOrderBy
    {
        RELEVANCY,

        A_TO_Z,

        Z_TO_A,

        VIEWS,

        RATINGS,

        VOTES,

        NEWEST,

        OLDEST_FIRST
    }
}