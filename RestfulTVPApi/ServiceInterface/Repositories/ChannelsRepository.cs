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
        public List<Media> GetChannelMultiFilter(InitializationObject initObj, int ChannelID, string picSize, int pageSize, int pageIndex, TVPApiModule.Context.OrderBy orderBy, eOrderDirection orderDir, List<KeyValue> tagsMetas, CutWith cutWith)
        {
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetChannelMultiFilter", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                return new APIChannelMediaLoader(ChannelID, groupID, initObj.Platform, initObj.UDID, SiteHelper.GetClientIP(), pageSize, pageIndex, picSize, initObj.Locale.LocaleLanguage,null, tagsMetas, cutWith)
                {
                    UseStartDate = bool.Parse(ConfigManager.GetInstance().GetConfig(groupID, initObj.Platform).SiteConfiguration.Data.Features.FutureAssets.UseStartDate)
                }.Execute() as List<Media>;
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public List<Channel> GetChannelsList(InitializationObject initObj, string sPicSize)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetChannelsList", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                return ChannelHelper.GetChannelsList(initObj, sPicSize, groupId);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public Category GetCategory(InitializationObject initObj, int categoryID)
        {
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetCategory", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                return CategoryTreeHelper.GetCategoryTree(categoryID, groupID, initObj.Platform);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public Category GetFullCategory(InitializationObject initObj, int categoryID, string picSize)
        {
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetFullCategory", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                return CategoryTreeHelper.GetFullCategoryTree(categoryID, picSize, groupID, initObj.Platform);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }
    }
}