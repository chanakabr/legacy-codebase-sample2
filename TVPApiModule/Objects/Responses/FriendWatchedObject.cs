using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class FriendWatchedObject
    {
        public int SiteGuid { get; set; }

        public int MediaID { get; set; }

        public System.DateTime UpdateDate { get; set; }
    }
}
