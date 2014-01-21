using System.Collections.Generic;
using Tvinci.Data.Loaders.TvinciPlatform.Catalog;
using TVPApi;
using TVPApiModule.CatalogLoaders;
using TVPApiModule.Helper;
using TVPPro.SiteManager.Helper;

namespace RestfulTVPApi.ServiceInterface
{
    public class ChannelsRepository : IChannelsRepository
    {
        public List<Media> GetChannelMultiFilter(InitializationObject initObj, int ChannelID, string picSize, int pageSize, int pageIndex, TVPApi.OrderBy orderBy, eOrderDirection orderDir, List<KeyValue> tagsMetas, CutWith cutWith)
        {
            List<Media> lstMedia = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetChannelMultiFilter", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                lstMedia = new APIChannelMediaLoader(ChannelID, groupID, initObj.Platform, initObj.UDID, SiteHelper.GetClientIP(), pageSize, pageIndex, picSize, initObj.Locale.LocaleLanguage, tagsMetas, cutWith)
                {
                    UseStartDate = bool.Parse(ConfigManager.GetInstance().GetConfig(groupID, initObj.Platform).SiteConfiguration.Data.Features.FutureAssets.UseStartDate)
                }.Execute() as List<Media>;
            }
            else
            {
                throw new UnknownGroupException();
            }

            return lstMedia;
        }

        public List<Channel> GetChannelsList(InitializationObject initObj, string sPicSize)
        {
            List<Channel> sRet = null;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetChannelsList", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                sRet = ChannelHelper.GetChannelsList(initObj, sPicSize, groupId);
            }
            else
            {
                throw new UnknownGroupException();
            }

            return sRet;
        }

        public Category GetCategory(InitializationObject initObj, int categoryID)
        {
            Category retCategory = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetCategory", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                retCategory = CategoryTreeHelper.GetCategoryTree(categoryID, groupID, initObj.Platform);
            }
            else
            {
                throw new UnknownGroupException();
            }

            return retCategory;
        }

        public Category GetFullCategory(InitializationObject initObj, int categoryID, string picSize)
        {
            Category retCategory = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetFullCategory", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                retCategory = CategoryTreeHelper.GetFullCategoryTree(categoryID, picSize, groupID, initObj.Platform);
            }
            else
            {
                throw new UnknownGroupException();
            }

            return retCategory;
        }
    }
}