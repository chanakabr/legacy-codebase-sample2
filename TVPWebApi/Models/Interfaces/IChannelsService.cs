using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TVPApi;

namespace TVPWebApi.Models
{
    public interface IChannelsService
    {
        List<Media> GetChannelMediaList(InitializationObject initObj, long ChannelID, string picSize, int pageSize, int pageIndex, TVPApi.OrderBy orderBy);
    }
}