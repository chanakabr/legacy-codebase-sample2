using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class UserSocialActionObject
    {
        public string m_sSiteGuid { get; set; }

        public eUserAction m_eSocialAction { get; set; }

        public SocialPlatform m_eSocialPlatform { get; set; }

        public int nMediaID { get; set; }

        public int nProgramID { get; set; }

        public eAssetType assetType { get; set; }

        public System.DateTime m_dActionDate { get; set; }
    }

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

    public enum SocialPlatform
    {

        /// <remarks/>
        UNKNOWN,

        /// <remarks/>
        FACEBOOK,

        /// <remarks/>
        GOOGLE,
    }

    public enum eAssetType
    {

        /// <remarks/>
        UNKNOWN = 1,

        /// <remarks/>
        MEDIA = 2,

        /// <remarks/>
        PROGRAM = 4,
    }
}
