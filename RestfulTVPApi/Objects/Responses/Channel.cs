using RestfulTVPApi.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for Channel
/// </summary>
/// 
namespace RestfulTVPApi.Objects.Responses
{
    public class Channel
    {

        public string title { get; set; }
        public long channel_id { get; set; }
        public int media_count { get; set; }
        public string pic_url { get; set; }

        public Channel(channelObj channel, string picSize)
        {
            title = channel.m_sTitle;
            channel_id = channel.m_nChannelID;
            media_count = 0;
            if (!string.IsNullOrEmpty(picSize) && channel.m_lPic != null)
            {
                var pic = channel.m_lPic.Where(p => p.m_sSize.ToLower() == picSize.ToLower()).FirstOrDefault();
                pic_url = pic == null ? string.Empty : pic.m_sURL;
            }
        }

        public Channel()
        {
            channel_id = 0;
            title = string.Empty;
            media_count = 0;
            pic_url = string.Empty;
        }



    }
}
