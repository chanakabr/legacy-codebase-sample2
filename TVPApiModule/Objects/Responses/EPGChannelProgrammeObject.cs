using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class EPGChannelProgrammeObject
    {
        public long epgID { get; set; }

        public string epgChannelID { get; set; }

        public string epgIdentifier { get; set; }

        public string name { get; set; }

        public string description { get; set; }

        public string startDate { get; set; }

        public string endDate { get; set; }

        public string picUrl { get; set; }

        public string status { get; set; }

        public string isActive { get; set; }

        public string groupID { get; set; }

        public string updaterID { get; set; }

        public string updateDate { get; set; }

        public string publishDate { get; set; }

        public string createDate { get; set; }

        public int likeCounter { get; set; }

        public EPGDictionary[] epgTags { get; set; }

        public EPGDictionary[] epgMeta { get; set; }

        public string mediaID { get; set; }
    }
}
