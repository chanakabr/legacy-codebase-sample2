using RestfulTVPApi.ServiceModel;
using System.Collections.Generic;
using Tvinci.Data.Loaders.TvinciPlatform.Catalog;
using TVPApi;
using TVPApiModule.CatalogLoaders;
using TVPApiModule.Context;
using TVPApiModule.Helper;
using TVPApiModule.Manager;
using TVPApiModule.Objects;
using TVPApiModule.Objects.Responses;
using TVPPro.SiteManager.Helper;

namespace RestfulTVPApi.ServiceInterface
{
    public class ChannelsRepository : IChannelsRepository
    {
        public List<Media> GetChannelMultiFilter(GetChannelMultiFilterRequest request)
        {
            return new APIChannelMediaLoader(request.channel_id, request.GroupID, request.InitObj.Platform, request.InitObj.UDID, SiteHelper.GetClientIP(), request.page_size, request.page_number, request.pic_size, request.InitObj.Locale.LocaleLanguage,null, request.tags_metas, request.cut_with)
                {
                    UseStartDate = Utils.GetUseStartDateValue(request.GroupID, request.InitObj.Platform)
                }.Execute() as List<Media>;
        }

        public List<TVPApiModule.Objects.Responses.Channel> GetChannelsList(GetChannelsListRequest request)
        {
            return ChannelHelper.GetChannelsList(request.InitObj, request.pic_size, request.GroupID, request.site_guid);
        }

        public Category GetCategory(GetCategoryRequest request)
        {
            return CategoryTreeHelper.GetCategoryTree(request.category_id, request.GroupID, request.InitObj.Platform);            
        }

        public Category GetFullCategory(GetFullCategoryRequest request)
        {
            return CategoryTreeHelper.GetFullCategoryTree(request.category_id, request.pic_size, request.GroupID, request.InitObj.Platform);
        }

        public List<Media> GetOrderedChannelMultiFilter(GetOrderedChannelMultiFilterRequest request)
        {
            throw new System.NotImplementedException();
        }
    }
}