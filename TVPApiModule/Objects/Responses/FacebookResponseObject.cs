using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class FacebookResponseObject
    {
        public string status { get; set; }

        public string siteGuid { get; set; }

        public string tvinciName { get; set; }

        public string facebookName { get; set; }

        public string pic { get; set; }

        public string data { get; set; }

        public string minFriends { get; set; }

        public FBUser fbUser { get; set; }

        public string token { get; set; }
    }
}

