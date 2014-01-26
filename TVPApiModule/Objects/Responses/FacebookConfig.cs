using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class FacebookConfig
    {
        public string fb_key { get; set; }

        public string fb_secret { get; set; }

        public string fb_callback { get; set; }

        public int fb_min_friends { get; set; }

        public string fb_permissions { get; set; }

        public string fb_redirect { get; set; }
    }
}
