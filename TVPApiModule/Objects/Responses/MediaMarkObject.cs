using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class MediaMarkObject
    {
        public int groupID { get; set; }

        public string siteGUID { get; set; }

        public int mediaID { get; set; }

        public int locationSec { get; set; }

        public string deviceName { get; set; }

        public string deviceID { get; set; }

        public MediaMarkObjectStatus status { get; set; }
    }
}

