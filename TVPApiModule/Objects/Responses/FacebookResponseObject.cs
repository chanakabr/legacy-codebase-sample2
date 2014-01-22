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

        public FBUser fbUser{ get; set; }

        public string token { get; set; }
    }

    public class FBUser
    {
        public string m_sSiteGuid { get; set; }

        public string Birthday { get; set; }

        public FBLoaction Location { get; set; }

        public FBInterest interests { get; set; }

        public string name { get; set; }

        public string id { get; set; }

        public string uid { get; set; }

        public string first_name { get; set; }

        public string last_name { get; set; }

        public string email { get; set; }

        public string gender { get; set; }
    }

    public class FBLoaction
    {
        public string name { get; set; }

        public string id { get; set; }
    }

    public class FBInterest
    {
        public FBInterestData[] data { get; set; }
    }

    public class FBInterestData
    {
        public string name { get; set; }

        public string category { get; set; }

        public string id { get; set; }

        public string created_time { get; set; }
    }

}

