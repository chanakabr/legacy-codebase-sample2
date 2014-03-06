using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TVPPro.SiteManager.DataEntities;
using TVPPro.SiteManager.DataLoaders;
using TVPPro.Configuration.Technical;
using TVPPro.SiteManager.TvinciPlatform.ConditionalAccess;
using TVPPro.SiteManager.Services;
using TVPPro.SiteManager.Helper;
using System.Text;
using TVPPro.Configuration.Media;
using TVPApiModule.Services;
using TVPPro.SiteManager.TvinciPlatform.Users;
using Tvinci.Data.Loaders.TvinciPlatform.Catalog;
using TVPApiModule.CatalogLoaders;
using TVPApiModule.Objects.Responses;
using TVPApiModule.Context;
using TVPApiModule.Parsers;

/// <summary>
/// Summary description for MediaHelper
/// </summary>
/// 

namespace TVPApiModule.Helper
{
    public class MediaHelper
    {
        // Not used!
        //public enum LoaderType
        //{
        //    Channel,
        //    Related,
        //    PeopleWhoWatched,
        //    LastWatched,
        //    Recommended
        //}

        public enum ePeriod
        {
            All = 0,
            Day = 1,
            Week = 7,
            Month = 30
        }

        public MediaHelper()
        {

        }        

        //Get User Items (favorites, Purchases, Packages)
        public static List<Media> GetUserItems(PlatformType platform, string siteGuid, int domainID, string udid, string language, UserItemType userItemType, string picSize, int pageSize, int pageIndex, int groupID)
        {
            List<Media> retVal = new List<Media>();
            string guid = siteGuid;
            if (platform == PlatformType.STB)
            {
                guid = UsersXMLParser.Instance.GetGuid(platform.ToString(), guid);
            }
            
            List<int> mediaIDsList;

            switch (userItemType)
            {
                case UserItemType.Favorite:
                    {
                        var favoritesObj = ServicesManager.UsersService(groupID, platform).GetUserFavorites(siteGuid, string.Empty, domainID, string.Empty);

                        if (favoritesObj != null)
                        {
                            mediaIDsList = favoritesObj.Select(f => int.Parse(f.item_code)).ToList();
                            retVal = new TVPApiModule.CatalogLoaders.APIMediaLoader(mediaIDsList, groupID, platform, udid, SiteHelper.GetClientIP(), picSize, language).Execute() as List<Media>;
                        }

                        break;
                    }
                case UserItemType.Rental:
                    {
                        var MediaPermitedItems = ServicesManager.ConditionalAccessService(groupID, platform).GetUserPermittedItems(guid);
                        mediaIDsList = MediaPermitedItems.Select(mp => mp.media_id).ToList();
                        retVal = new TVPApiModule.CatalogLoaders.APIMediaLoader(mediaIDsList, groupID, platform, udid, SiteHelper.GetClientIP(), picSize, language).Execute() as List<Media>;
                        break;
                    }
                case UserItemType.Package:
                    {
                        var PermitedPackages = ServicesManager.ConditionalAccessService(groupID, platform).GetUserPermitedSubscriptions(guid);
                        if (PermitedPackages != null && PermitedPackages.Count() > 0)
                        {
                            List<KeyValue> BaseIdsDict = new List<KeyValue>();
                            StringBuilder sb = new StringBuilder();

                            foreach (TVPApiModule.Objects.Responses.PermittedSubscriptionContainer sub in PermitedPackages)
                            {
                                sb.AppendFormat("{0}{1}", sub.subscription_code, ";");
                                var pair = BaseIdsDict.Where(bid => bid.m_sKey == "Base ID").FirstOrDefault();
                                if (pair == null)
                                {
                                    BaseIdsDict.Add(new KeyValue() { m_sKey = "Base ID", m_sValue = sub.subscription_code});
                                }
                                else
                                {
                                    pair.m_sValue = string.Concat(pair.m_sValue, ";", sub.subscription_code);
                                }
                            }
                            if (BaseIdsDict != null && BaseIdsDict.Count > 0)
                            {
                                retVal = new TVPApiModule.CatalogLoaders.APISearchMediaLoader(groupID, platform, udid, SiteHelper.GetClientIP(), language, pageSize,  pageIndex, picSize, string.Empty)
                                {
                                    Metas = BaseIdsDict
                                }.Execute() as List<Media>;
                            }
                        }
                        break;
                    }
                default:
                    break;
            }

            // paging for the results from media loader (does not support paging)
            if (userItemType != UserItemType.Package && retVal != null && retVal.Count > 0)
            {
                int startIndex = (pageIndex) * pageSize;
                
                //Local server Paging
                IEnumerable<Media> pagedDT = PagingHelper.GetPagedData<Media>(startIndex, pageSize, retVal);
                retVal = pagedDT.ToList();
            }
            return retVal;
        }
    }
}
