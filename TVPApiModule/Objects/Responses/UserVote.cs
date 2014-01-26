using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects
{
    public class UserVote
    {
        public string mediaID { get; set; }

        public string siteGUID { get; set; }

        public int score { get; set; }

        public DateTime time { get; set; }

        public string platform { get; set; }
    }
}
