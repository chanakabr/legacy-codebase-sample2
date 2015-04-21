using RestfulTVPApi.Objects.Responses;
using RestfulTVPApi.ServiceModel;
using System.Collections.Generic;


namespace RestfulTVPApi.ServiceInterface
{
    public class ChannelsRepository : IChannelsRepository
    {
        public List<Media> GetChannelMultiFilter(GetChannelMultiFilterRequest request)
        {
            //return new APIChannelMediaLoader(request.channel_id, request.GroupID, request.InitObj.Platform, request.InitObj.UDID, SiteHelper.GetClientIP(), request.page_size, request.page_number, request.pic_size, request.InitObj.Locale.LocaleLanguage,null, request.tags_metas, request.cut_with)
            //    {
            //        UseStartDate = Utils.GetUseStartDateValue(request.GroupID, request.InitObj.Platform)
            //    }.Execute() as List<Media>;
            return null;
        }

        public List<Channel> GetChannelsList(GetChannelsListRequest request)
        {
            //return new APIChannelsListsLoader(0, groupID, initObj.Platform, initObj.UDID, SiteHelper.GetClientIP(), initObj.Locale.LocaleLanguage, 0, 0, picSize) { SiteGuid = siteGuid }.Execute() as List<Channel>;
            return null;
        }

        public Category GetCategory(GetCategoryRequest request)
        {
            //return CategoryTreeHelper.GetCategoryTree(request.category_id, request.GroupID, request.InitObj.Platform);            
            return null;
        }

        public Category GetFullCategory(GetFullCategoryRequest request)
        {
            //return CategoryTreeHelper.GetFullCategoryTree(request.category_id, request.pic_size, request.GroupID, request.InitObj.Platform);
            return null;
        }

        public List<Media> GetOrderedChannelMultiFilter(GetOrderedChannelMultiFilterRequest request)
        {
            throw new System.NotImplementedException();
        }
    }
}