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
namespace TVPApi
{
    public class Channel
    {

        public string title { get; set; }
        public long channelID { get; set; }
        public int mediaCount { get; set; }
        public string picURL { get; set; }

        public Channel(dsCategory.ChannelsRow channelRow)
        {
            title = string.Empty;
            channelID = 0;
            mediaCount = 0;
            if (!channelRow.IsTitleNull())
            {
                title = channelRow.Title;
            }
            channelID = channelRow.ID;

            if (!channelRow.IsPicURLNull())
            {
                picURL = channelRow.PicURL;
            }
            
            if (!channelRow.IsNumOfItemsNull())
            {
                mediaCount = channelRow.NumOfItems;
            }
        }

        public Channel(dsItemInfo.ChannelRow channelRow)
        {
            title = string.Empty;
            this.channelID = 0;
            mediaCount = 0;

            if (!channelRow.IsTitleNull())
            {
                title = channelRow.Title;
            }

            long channelID;

            if (long.TryParse(channelRow.ChannelId, out channelID))
            {
                this.channelID = channelID;
            }
        }

        public Channel(channelObj channel, string picSize)
        {
            title = channel.m_sTitle;
            channelID = channel.m_nChannelID;
            mediaCount = 0;
            if (!string.IsNullOrEmpty(picSize) && channel.m_lPic != null)
            {
                var pic = channel.m_lPic.Where(p => p.m_sSize.ToLower() == picSize.ToLower()).FirstOrDefault();
                picURL = pic == null ? string.Empty : pic.m_sURL;
            }
        }

        public Channel()
        {
            channelID = 0;
            title = string.Empty;
            mediaCount = 0;
            picURL = string.Empty;
        }



    }
}
