using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TVPApi;
using TVPPro.SiteManager.Helper;

namespace TVPWebApi.Models
{
    public class ChannelsService : IChannelsService
    {

        public List<Media> GetChannelMediaList(InitializationObject initObj, long ChannelID, string picSize, int pageSize, int pageIndex, TVPApi.OrderBy orderBy)
        {
            List<Media> lstMedia = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetChannelMediaList", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                lstMedia = MediaHelper.GetChannelMediaList(initObj, ChannelID, picSize, pageSize, pageIndex, groupID, orderBy);
            }
            else
            {
                throw new UnknownGroupException();
            }

            return lstMedia;
        }

    }
}