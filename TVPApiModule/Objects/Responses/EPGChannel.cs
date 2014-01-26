using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class EPGChannel
    {
        public string epg_channel_id { get; set; }

        public string name { get; set; }

        public string description { get; set; }

        public string order_num { get; set; }

        public string is_active { get; set; }

        public string pic_url { get; set; }

        public string group_id { get; set; }

        public string editor_remarks { get; set; }

        public string status { get; set; }

        public string updater_id { get; set; }

        public string create_date { get; set; }

        public string publish_date { get; set; }

        public string channel_id { get; set; }

        public string media_id { get; set; }
    }
}
