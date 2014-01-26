using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class FacebookConfig
    {
        public string fbKey { get; set; }

        public string fbSecret { get; set; }

        public string fbCallback { get; set; }

        public int fbMinFriends { get; set; }

        public string fbPermissions { get; set; }

        public string fbRedirect { get; set; }
    }
}
