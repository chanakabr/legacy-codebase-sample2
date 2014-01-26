using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class User
    {
        public UserBasicData basicData { get; set; }

        public UserDynamicData dynamicData { get; set; }

        public string siteGUID { get; set; }

        public int domianID { get; set; }

        public bool domainMaster { get; set; }

        public UserState userState { get; set; }

        public int ssoOperatorID { get; set; }
    }
}
