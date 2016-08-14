using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace WebAPI.Models.Catalog
{
    public enum KalturaAssetOrderBy
    {
        RELEVANCY_DESC,

        NAME_ASC,

        NAME_DESC,

        VIEWS_DESC,

        RATINGS_DESC,

        VOTES_DESC,

        START_DATE_DESC,

        START_DATE_ASC,

        LIKES_DESC
    }
}