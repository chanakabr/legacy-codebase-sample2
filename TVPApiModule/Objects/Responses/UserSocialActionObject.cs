using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class UserSocialActionObject
    {
        public string site_guid { get; set; }

        public eUserAction social_action { get; set; }

        public SocialPlatform social_platform { get; set; }

        public int media_id { get; set; }

        public int program_id { get; set; }

        public eAssetType asset_type { get; set; }

        public System.DateTime action_date { get; set; }
    }
}
