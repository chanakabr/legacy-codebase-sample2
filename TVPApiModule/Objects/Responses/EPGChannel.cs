using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class EPGChannel
    {
        public string epgChannelID { get; set; }

        public string name { get; set; }

        public string description { get; set; }

        public string orderNum { get; set; }

        public string isActive { get; set; }

        public string picUrl { get; set; }

        public string groupID { get; set; }

        public string editorRemarks { get; set; }

        public string status { get; set; }

        public string updaterID { get; set; }

        public string createDate { get; set; }

        public string publishDate { get; set; }

        public string channelID { get; set; }

        public string mediaID { get; set; }
    }
}
