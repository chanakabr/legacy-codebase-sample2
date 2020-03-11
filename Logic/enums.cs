using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Api
{
    public enum SocialAction
    {
        UNKNOWN = 0,
        LIKE = 1,
        UNLIKE = 2,
        SHARE = 3,
        POST = 4
    }

    public enum SocialPlatform
    {
        UNKNOWN = 0,
        FACEBOOK = 1,
        GOOGLE = 2
    }

    public enum OSSAdapterStatus
    {
        OK = 0,
        Error = 1,
        SignatureMismatch = 2,
        NoConfigurationFound = 3
    }
}
