using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class UserSocialActionObject
    {
        public string siteGuid { get; set; }

        public eUserAction socialAction { get; set; }

        public SocialPlatform socialPlatform { get; set; }

        public int mediaID { get; set; }

        public int programID { get; set; }

        public eAssetType assetType { get; set; }

        public System.DateTime actionDate { get; set; }
    }
}
