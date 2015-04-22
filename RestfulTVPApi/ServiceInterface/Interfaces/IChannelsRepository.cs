using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RestfulTVPApi.ServiceModel;
using RestfulTVPApi.Objects.Responses;

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