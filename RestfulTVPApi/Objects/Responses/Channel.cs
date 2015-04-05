using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Tvinci.Data.Loaders.TvinciPlatform.Catalog;
using TVPPro.SiteManager.DataEntities;

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

        public Channel(dsCategory.ChannelsRow channelRow)
        {
            title = string.Empty;
            channel_id = 0;
            media_count = 0;
            if (!channelRow.IsTitleNull())
            {
                title = channelRow.Title;
            }
            channel_id = channelRow.ID;

            if (!channelRow.IsPicURLNull())
            {
                pic_url = channelRow.PicURL;
            }
            
            if (!channelRow.IsNumOfItemsNull())
            {
                media_count = channelRow.NumOfItems;
            }
        }

        public Channel(dsItemInfo.ChannelRow channelRow)
        {
            title = string.Empty;
            this.channel_id = 0;
            media_count = 0;

            if (!channelRow.IsTitleNull())
            {
                title = channelRow.Title;
            }

            long channelID;

            if (long.TryParse(channelRow.ChannelId, out channelID))
            {
                this.channel_id = channelID;
            }
        }

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
