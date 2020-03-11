using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects
{
    public class UserVote
    {
        public string MediaID { get; set; }
        public string SiteGUID { get; set; }
        public int Score { get; set; }
        public DateTime Time { get; set; }
        public string Platform { get; set; }
    }
}
