using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestfulTVPApi.Objects.Responses
{
    public class FacebookResponseObject
    {
        public string status { get; set; }

        public string site_guid { get; set; }

        public string tvinci_name { get; set; }

        public string facebook_name { get; set; }

        public string pic { get; set; }

        public string data { get; set; }

        public string min_friends { get; set; }

        public FBUser fb_user { get; set; }

        public string token { get; set; }
    }
}

