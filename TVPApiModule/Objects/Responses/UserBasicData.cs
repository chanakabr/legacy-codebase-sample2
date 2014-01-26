using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class UserBasicData
    {
        public string userName { get; set; }

        public string firstName { get; set; }

        public string lastName { get; set; }

        public string email { get; set; }

        public string address { get; set; }

        public string city { get; set; }

        public State state { get; set; }

        public Country country { get; set; }

        public string zip { get; set; }

        public string phone { get; set; }

        public string facebookID { get; set; }

        public string facebookImage { get; set; }

        public bool isFacebookImagePermitted { get; set; }

        public string affiliateCode { get; set; }

        public string coGuid { get; set; }

        public string externalToken { get; set; }

        public string facebookToken { get; set; }

        public UserType userType { get; set; }
    }
}
