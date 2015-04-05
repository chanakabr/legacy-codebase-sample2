using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestfulTVPApi.Objects.Responses.Enums
{
    public enum SocialActionResponseStatus
    {

        /// <remarks/>
        UNKNOWN,

        /// <remarks/>
        OK,

        /// <remarks/>
        ERROR,

        /// <remarks/>
        UNKNOWN_ACTION,

        /// <remarks/>
        INVALID_ACCESS_TOKEN,

        /// <remarks/>
        INVALID_PLATFORM_REQUEST,

        /// <remarks/>
        MEDIA_DOESNT_EXISTS,

        /// <remarks/>
        MEDIA_ALREADY_LIKED,

        /// <remarks/>
        INVALID_PARAMETERS,

        /// <remarks/>
        USER_DOES_NOT_EXIST,

        /// <remarks/>
        NO_FB_ACTION,

        /// <remarks/>
        EMPTY_FB_OBJECT_ID,

        /// <remarks/>
        MEDIA_ALEADY_FOLLOWED,

        /// <remarks/>
        CONFIG_ERROR,

        /// <remarks/>
        MEDIA_ALREADY_RATED,

        /// <remarks/>
        NOT_ALLOWED,
    }
    
}
