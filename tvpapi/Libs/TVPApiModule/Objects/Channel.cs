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
        private int MediaCount { get; set; }
        public string PicURL { get; set; }
        public List<Picture> m_pictures;

        public Channel(channelObj channelObj, string picSize)
        {
            Title = string.Empty;
            ChannelID = 0;
            MediaCount = 0;

            Title = channelObj.m_sTitle;
            ChannelID = channelObj.m_nChannelID;
            this.m_pictures = channelObj.m_lPic;

            if (channelObj.m_lPic != null)
            {
                var pic = channelObj.m_lPic.Where(p => p.m_sSize.ToLower() == picSize.ToLower()).FirstOrDefault();
                PicURL = pic == null ? string.Empty : pic.m_sURL;
            }
            //MediaCount = channelObj;
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

        public Channel()
        {
            ChannelID = 0;
            Title = string.Empty;
            MediaCount = 0;
            PicURL = string.Empty;
        }



    }
}
