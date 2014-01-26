using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class FriendWatchedObject
    {
        public int siteGuid { get; set; }

        public int mediaID { get; set; }

        public System.DateTime updateDate { get; set; }
    }
}
