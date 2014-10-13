using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.Epg
{
    public static class EpgLinkConstants
    {
        public static readonly string BASIC_LINK = "basic_link";
        public static readonly string MEDIA_FILE_ID = "media_file_id";
        public static readonly string PROGRAM_ID = "program_id";
        public static readonly string START_TIME = "start_time";
        public static readonly string EPG_FORMAT_TYPE = "epg_format_type";

        public static readonly string CHANNEL_NAME = "channel_name";
        public static readonly string PROGRAM_START = "program_start";
        public static readonly string PROGRAM_END = "program_end";
        public static readonly string DEVICE_PROFILE = "device_profile";

        public static readonly string HOST = "host";        
        public static readonly string TIME_MULT_FACTOR = "time_mult_factor";
        public static readonly string RIGHT_MARGIN = "right_margin";
        public static readonly string LEFT_MARGIN = "left_margin";

    }


    [Serializable]
    public class EpgLinkItem
    {
        public string m_key { get; set; }
        public object m_value { get; set; }
        public EpgLinkItem()
        {
        }
        public EpgLinkItem(string key, object value)
        {
            m_key = key;
            m_value = value;
        }
    }
    [Serializable]
    public class EpgLink
    {
        public List<EpgLinkItem> m_lParams;

        public EpgLink()
        {
            m_lParams = new List<EpgLinkItem>();
        }

        public void AddPair(string key, object value)
        {
            EpgLinkItem epgLinkItem = new EpgLinkItem(key, value);
            this.m_lParams.Add(epgLinkItem);
        }
    }
}
