using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class FriendWatchedObject
    {
        public int site_guid { get; set; }

        public int media_id { get; set; }

        public System.DateTime update_date { get; set; }
    }
}
