using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Tvinci.Data.Loaders.TvinciPlatform.Catalog;
using TVPApi;
using TVPApiModule.Objects.Responses;

namespace RestfulTVPApi.ServiceInterface
{
    public interface IChannelsRepository
    {
        IEnumerable<Media> GetChannelMultiFilter(InitializationObject initObj, int ChannelID, string picSize, int pageSize, int pageIndex, TVPApi.OrderBy orderBy, eOrderDirection orderDir, List<KeyValue> tagsMetas, CutWith cutWith);

        IEnumerable<Channel> GetChannelsList(InitializationObject initObj, string sPicSize);

        Category GetCategory(InitializationObject initObj, int categoryID);

        Category GetFullCategory(InitializationObject initObj, int categoryID, string picSize);
    }
}