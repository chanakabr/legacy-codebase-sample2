using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class EPGMultiChannelProgrammeObject
    {
        public string EPG_CHANNEL_ID { get; set; }
        public EPGChannelProgrammeObject[] EPGChannelProgrammeObject { get; set; }
    }

    public class EPGChannelProgrammeObject
    {
        public long EPG_ID { get; set; }
        public string EPG_CHANNEL_ID { get; set; }
        public string EPG_IDENTIFIER { get; set; }
        public string NAME { get; set; }
        public string DESCRIPTION { get; set; }
        public string START_DATE { get; set; }
        public string END_DATE { get; set; }
        public string PIC_URL { get; set; }
        public string STATUS { get; set; }
        public string IS_ACTIVE { get; set; }
        public string GROUP_ID { get; set; }
        public string UPDATER_ID { get; set; }
        public string UPDATE_DATE { get; set; }
        public string PUBLISH_DATE { get; set; }
        public string CREATE_DATE { get; set; }
        public int LIKE_COUNTER { get; set; }
        public EPGDictionary[] EPG_TAGS { get; set; }
        public EPGDictionary[] EPG_Meta { get; set; }
        public string media_id { get; set; }
    }

    public class EPGDictionary
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }
}
