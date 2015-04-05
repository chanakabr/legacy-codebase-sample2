using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestfulTVPApi.Objects.Responses
{
    public class EPGChannelProgrammeObject
    {
        public long epg_id { get; set; }

        public string epg_channel_id { get; set; }

        public string epg_identifier { get; set; }

        public string name { get; set; }

        public string description { get; set; }

        public string start_date { get; set; }

        public string end_date { get; set; }

        public string pic_url { get; set; }

        public string status { get; set; }

        public string is_active { get; set; }

        public string group_id { get; set; }

        public string updater_id { get; set; }

        public string update_date { get; set; }

        public string publish_date { get; set; }

        public string create_date { get; set; }

        public int like_counter { get; set; }

        public EPGDictionary[] epg_tags { get; set; }

        public EPGDictionary[] epg_meta { get; set; }

        public string media_id { get; set; }
    }
}
