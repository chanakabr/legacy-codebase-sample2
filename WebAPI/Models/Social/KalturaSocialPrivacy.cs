using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI.Models.Social
{
    public enum KalturaSocialPrivacy
    {
        UNKNOWN = 0,
        EVERYONE = 2,
        ALL_FRIENDS = 4,
        FRIENDS_OF_FRIENDS = 8,
        SELF = 16,
        CUSTOM = 32
    }
}