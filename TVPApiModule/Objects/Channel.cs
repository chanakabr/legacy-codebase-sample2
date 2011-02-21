using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
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
            if (!channelRow.IsNumOfItemsNull())
            {
                MediaCount = channelRow.NumOfItems;
            }
        }

        public Channel()
        {
            ChannelID = 0;
            Title = string.Empty;
            MediaCount = 0;
        }



    }
}
