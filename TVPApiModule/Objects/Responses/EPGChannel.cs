using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class EPGChannel
    {
        public string EPG_CHANNEL_ID { get; set; }
        public string NAME { get; set; }
        public string DESCRIPTION { get; set; }
        public string ORDER_NUM { get; set; }
        public string IS_ACTIVE { get; set; }
        public string PIC_URL { get; set; }
        public string GROUP_ID { get; set; }
        public string EDITOR_REMARKS { get; set; }
        public string STATUS { get; set; }
        public string UPDATER_ID { get; set; }
        public string CREATE_DATE { get; set; }
        public string PUBLISH_DATE { get; set; }
        public string CHANNEL_ID { get; set; }
        public string MEDIA_ID { get; set; }
    }
}
