using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class FacebookConfig
    {
        public string sFBKey { get; set; }

        public string sFBSecret { get; set; }

        public string sFBCallback { get; set; }

        public int nFBMinFriends { get; set; }

        public string sFBPermissions { get; set; }

        public string sFBRedirect { get; set; }
    }
}
