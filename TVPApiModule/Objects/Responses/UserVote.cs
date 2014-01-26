using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects
{
    public class UserVote
    {
        public string media_id { get; set; }

        public string site_guid { get; set; }

        public int score { get; set; }

        public DateTime time { get; set; }

        public string platform { get; set; }
    }
}
