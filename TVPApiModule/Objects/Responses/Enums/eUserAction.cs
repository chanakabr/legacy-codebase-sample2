using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects
{
    public enum eUserAction
    {

        /// <remarks/>
        UNKNOWN = 1,

        /// <remarks/>
        LIKE = 2,

        /// <remarks/>
        UNLIKE = 4,

        /// <remarks/>
        SHARE = 8,

        /// <remarks/>
        POST = 16,

        /// <remarks/>
        WATCHES = 32,

        /// <remarks/>
        WANT_TO_WATCH = 64,

        /// <remarks/>
        RATES = 128,

        /// <remarks/>
        FOLLOWS = 256,

        /// <remarks/>
        UNFOLLOW = 512,
    }
}
