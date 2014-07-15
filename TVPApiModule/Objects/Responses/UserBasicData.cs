using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class UserBasicData
    {
        public string user_name { get; set; }

        public string first_name { get; set; }

        public string last_name { get; set; }

        public string email { get; set; }

        public string address { get; set; }

        public string city { get; set; }

        public State state { get; set; }

        public Country country { get; set; }

        public string zip { get; set; }

        public string phone { get; set; }

        public string facebook_id { get; set; }

        public string facebook_image { get; set; }

        public bool is_facebook_image_permitted { get; set; }

        public string affiliate_code { get; set; }

        public string co_guid { get; set; }

        public string external_token { get; set; }

        public string facebook_token { get; set; }

        public string twitter_token { get; set; }

        public UserType user_type { get; set; }
    }
}
