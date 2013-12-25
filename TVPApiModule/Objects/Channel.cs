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

        public string Title { get; set; }
        public long ChannelID { get; set; }
        public int MediaCount { get; set; }
        public string PicURL { get; set; }

        public Channel(dsCategory.ChannelsRow channelRow)
        {
            Title = string.Empty;
            ChannelID = 0;
            MediaCount = 0;
            if (!channelRow.IsTitleNull())
            {
                Title = channelRow.Title;
            }
            ChannelID = channelRow.ID;

            if (!channelRow.IsPicURLNull())
            {
                PicURL = channelRow.PicURL;
            }
            
            if (!channelRow.IsNumOfItemsNull())
            {
                MediaCount = channelRow.NumOfItems;
            }
        }

        public Channel(dsItemInfo.ChannelRow channelRow)
        {
            Title = string.Empty;
            ChannelID = 0;
            MediaCount = 0;

            if (!channelRow.IsTitleNull())
            {
                Title = channelRow.Title;
            }

            long channelID;

            if (long.TryParse(channelRow.ChannelId, out channelID))
            {
                ChannelID = channelID;
            }
        }

        public Channel(channelObj channel, string picSize)
        {
            Title = channel.m_sTitle;
            ChannelID = channel.m_nChannelID;
            MediaCount = 0;
            if (!string.IsNullOrEmpty(picSize) && channel.m_lPic != null)
            {
                var pic = channel.m_lPic.Where(p => p.m_sSize.ToLower() == picSize.ToLower()).FirstOrDefault();
                PicURL = pic == null ? string.Empty : pic.m_sURL;
            }
        }

        public Channel()
        {
            ChannelID = 0;
            Title = string.Empty;
            MediaCount = 0;
            PicURL = string.Empty;
        }



    }
}
