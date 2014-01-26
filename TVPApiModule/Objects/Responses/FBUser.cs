using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class FBUser
    {
        public string site_guid { get; set; }

        public string birthday { get; set; }

        public FBLoaction location { get; set; }

        public FBInterest interests { get; set; }

        public string name { get; set; }

        public string id { get; set; }

        public string uid { get; set; }

        public string first_name { get; set; }

        public string last_name { get; set; }

        public string email { get; set; }

        public string gender { get; set; }
    }
}
