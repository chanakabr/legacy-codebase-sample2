using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Tvinci.Data.Loaders.TvinciPlatform.Catalog;
using TVPApi;
using TVPApiModule.Objects;
using TVPApiModule.Objects.Responses;
using TVPApiModule.Context;
using RestfulTVPApi.ServiceModel;

namespace RestfulTVPApi.ServiceInterface
{
    public interface IChannelsRepository
    {
        List<Media> GetChannelMultiFilter(GetChannelMultiFilterRequest request);

        List<Channel> GetChannelsList(GetChannelsListRequest request);

        Category GetCategory(GetCategoryRequest request);

        Category GetFullCategory(GetFullCategoryRequest request);

        List<Media> GetOrderedChannelMultiFilter(GetOrderedChannelMultiFilterRequest request);
    }
}