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
using TVPApiModule.DataLoaders;
using TVPApiModule.Services;
using TVPPro.SiteManager.TvinciPlatform.Users;

/// <summary>
/// Summary description for MediaHelper
/// </summary>
/// 

namespace TVPApi
{
    public class MediaHelper
    {
        public enum LoaderType
        {
            Channel,
            Related,
            PeopleWhoWatched,
            LastWatched
        }

        public MediaHelper()
        {
            
        }

        //Get media info
        public static Media GetMediaInfo(InitializationObject initObj, long MediaID, int mediaType, string picSize, int groupID, bool withDynamic)
        {
            Media retVal = null;
            TVMAccountType account = SiteMapManager.GetInstance.GetPageData(groupID, initObj.Platform).GetTVMAccountByMediaType(mediaType);
            dsItemInfo mediaInfo = (new APIMediaLoader(account.TVMUser, account.TVMPass, MediaID.ToString()) { GroupID = groupID, Platform = initObj.Platform, PicSize = picSize }.Execute());
            if (mediaInfo.Item != null && mediaInfo.Item.Count == 1)
            {
                dsItemInfo.ItemRow row = mediaInfo.Item.Rows[0] as dsItemInfo.ItemRow;
                if (row != null)
                {
                    retVal = new Media(row, initObj, groupID, withDynamic);
                }
            }

            return retVal;

        }

        public static List<Media> GetMediasInfo(InitializationObject initObj, long[] MediaIDs, int mediaType, string picSize, int groupID, bool withDynamic)
        {
            List<Media> retVal = new List<Media>();
            TVMAccountType account = SiteMapManager.GetInstance.GetPageData(groupID, initObj.Platform).GetTVMAccountByMediaType(mediaType);
            dsItemInfo mediaInfo = (new APIMultiMediaLoader(account.TVMUser, account.TVMPass, MediaIDs.Select(i => i.ToString()).ToArray(), picSize, mediaType) { GroupID = groupID, Platform = initObj.Platform, PicSize = picSize }.Execute());

            foreach (dsItemInfo.ItemRow row in mediaInfo.Item.Rows)
            {
                retVal.Add(new Media(row, initObj, groupID, withDynamic));
            }

            return retVal;

        }

        public static bool IsFavoriteMedia(InitializationObject initObj, int groupID, int mediaID)
        {
            //long guidNum = Convert.ToInt64(sID);
            bool retVal = false;
            FavoritObject[] favoriteObj = new ApiUsersService(groupID, initObj.Platform).GetUserFavorites(initObj.SiteGuid, string.Empty, initObj.DomainID, string.Empty);
            if (favoriteObj != null)
            {
                for (int i = 0; i < favoriteObj.Length; i++)
                {
                    if (favoriteObj[i].m_sItemCode == mediaID.ToString())
                    {
                        retVal = true;
                        break;
                    }
                }
            }
            return retVal;
            //return FavoritesHelper.ItemExistOnFavorite(itemID.ToString(), sID);
        }

        //Call search protocol
        public static List<Media> SearchMediaByTag(InitializationObject initObj, int mediaType, string tagName, string value, string picSize, int pageSize, int pageIndex, int groupID, int orderBy)
        {
            List<Media> retVal = new List<Media>();
            SiteMapManager.GetInstance.GetSiteMapInstance(groupID, initObj.Platform, initObj.Locale);
            TVMAccountType account = SiteMapManager.GetInstance.GetPageData(groupID, initObj.Platform).GetTVMAccountByMediaType(mediaType);
            Dictionary<string, string> dictTags = new Dictionary<string, string>();
            dictTags.Add(tagName, value);

            // create a signature for search loader
            string sSigature = string.Format(@"{0}={1}|{2}|{3}", tagName, value, groupID, initObj.Platform);

            dsItemInfo mediaInfo = (new APISearchLoader(account.TVMUser, account.TVMPass, dictTags) { MediaType = mediaType, SearchTokenSignature = sSigature, Platform = initObj.Platform, GroupID = groupID, WithInfo = true, PageSize = pageSize, PageIndex = pageIndex, OrderBy = (OrderBy)orderBy, PictureSize = picSize }.Execute());
            if (mediaInfo.Item != null && mediaInfo.Item.Count > 0)
            {
                foreach (dsItemInfo.ItemRow row in mediaInfo.Item)
                {
                    retVal.Add(new Media(row, initObj, groupID, false));
                }
            }

            return retVal;
        }

        public static List<Media> SearchMediaBySubID(InitializationObject initObj, string sSubID, string picSize, int groupID, int orderBy)
        {
            List<Media> retVal = new List<Media>();
            SiteMapManager.GetInstance.GetSiteMapInstance(groupID, initObj.Platform, initObj.Locale);
            TVMAccountType account = SiteMapManager.GetInstance.GetPageData(groupID, initObj.Platform).GetTVMAccountByAccountType(AccountType.Fictivic);
            Dictionary<string, string> dictMetas = new Dictionary<string, string>();

            string[] arrValues = sSubID.Split(';');
            dictMetas.Add("Base ID", sSubID);

            //Remote paging
            dsItemInfo mediaInfo = (new APISearchLoader(account.TVMUser, account.TVMPass) { SearchTokenSignature = sSubID, GroupID = groupID, Platform = initObj.Platform, dictMetas = dictMetas, WithInfo = true, PageSize = arrValues.Length, PictureSize = picSize, PageIndex = 0, OrderBy = (OrderBy)orderBy, MetaValues = sSubID, UseFinalEndDate = "true" }.Execute());
            if (mediaInfo.Item != null && mediaInfo.Item.Count > 0)
            {
                foreach (dsItemInfo.ItemRow row in mediaInfo.Item)
                {
                    retVal.Add(new Media(row, initObj, groupID, false));
                }
            }

            return retVal;
        }

        //Call search protocol
        public static List<Media> SearchMediaByMeta(InitializationObject initObj, int mediaType, string metaName, string value, string picSize, int pageSize, int pageIndex, int groupID, int orderBy)
        {
            List<Media> retVal = new List<Media>();
            SiteMapManager.GetInstance.GetSiteMapInstance(groupID, initObj.Platform, initObj.Locale);
            TVMAccountType account = SiteMapManager.GetInstance.GetPageData(groupID, initObj.Platform).GetTVMAccountByMediaType(mediaType);
            Dictionary<string, string> dictMetas = new Dictionary<string, string>();

            string[] arrValues = value.Split(';');
            foreach(string sValue in arrValues)
            {
                dictMetas.Add(metaName, sValue);
            }
            //Remote paging
            dsItemInfo mediaInfo = (new APISearchLoader(account.TVMUser, account.TVMPass) { SearchTokenSignature = value, GroupID = groupID, Platform = initObj.Platform, dictMetas = dictMetas,  WithInfo = true, PageSize = pageSize,PictureSize = picSize, PageIndex = pageIndex, OrderBy = (OrderBy)orderBy, MetaValues = value }.Execute());
            if (mediaInfo.Item != null && mediaInfo.Item.Count > 0)
            {
                foreach (dsItemInfo.ItemRow row in mediaInfo.Item)
                {
                    retVal.Add(new Media(row, initObj, groupID, false));
                }
            }

            return retVal;
        }

        //Call search protocol
        public static List<Media> SearchMediaByMeta(InitializationObject initObj, int mediaType, string metaName, string value, string picSize, int pageSize, int pageIndex, int groupID, int orderBy, ref long mediaCount)
        {
            List<Media> retVal = new List<Media>();
            SiteMapManager.GetInstance.GetSiteMapInstance(groupID, initObj.Platform, initObj.Locale);
            TVMAccountType account = SiteMapManager.GetInstance.GetPageData(groupID, initObj.Platform).GetTVMAccountByAccountType(AccountType.Regular);
            Dictionary<string, string> dictMetas = new Dictionary<string, string>();
            dictMetas.Add(metaName, value);
            //Remote paging
            APISearchLoader searchLoader = new APISearchLoader(account.TVMUser, account.TVMPass) { GroupID = groupID, Platform = initObj.Platform, dictMetas = dictMetas, WithInfo = true, PageSize = pageSize, PictureSize = picSize, PageIndex = pageIndex, OrderBy = (OrderBy)orderBy, IsPosterPic = false, MetaValues = value };
            dsItemInfo mediaInfo = searchLoader.Execute();
            searchLoader.TryGetItemsCount(out mediaCount);
            if (mediaInfo.Item != null && mediaInfo.Item.Count > 0)
            {
                foreach (dsItemInfo.ItemRow row in mediaInfo.Item)
                {
                    retVal.Add(new Media(row, initObj, groupID, false));
                }
            }

            return retVal;
        }

        //Call search protocol
        public static List<Media> SearchMedia(InitializationObject initObj, int mediaType, string text, string picSize, int pageSize, int pageIndex, int groupID, int orderBy)
        {
            List<Media> retVal = new List<Media>();
            SiteMapManager.GetInstance.GetSiteMapInstance(groupID, initObj.Platform, initObj.Locale);
            TVMAccountType account;
            if (mediaType > 0)
            {
                account = SiteMapManager.GetInstance.GetPageData(groupID, initObj.Platform).GetTVMAccountByMediaType(mediaType);
            }
            else
            {
                account = SiteMapManager.GetInstance.GetPageData(groupID, initObj.Platform).GetTVMAccountByAccountType(AccountType.Regular);
            }

            //Remote paging
            dsItemInfo mediaInfo = (new APISearchLoader(account.TVMUser, account.TVMPass) { MediaType = mediaType, Name = string.IsNullOrEmpty(text) ? null : text, PictureSize = picSize, GroupID = groupID, Platform = initObj.Platform, WithInfo = true, PageSize = pageSize, PageIndex = pageIndex, OrderBy = (TVPApi.OrderBy)orderBy }.Execute());
            if (mediaInfo.Item != null && mediaInfo.Item.Count > 0)
            {
                foreach (dsItemInfo.ItemRow row in mediaInfo.Item)
                {
                    retVal.Add(new Media(row, initObj, groupID, false));
                }
            }

            return retVal;
        }

        //Call search protocol
        public static List<Media> SearchMedia(InitializationObject initObj, int mediaType, string text, string picSize, int pageSize, int pageIndex, int groupID, int orderBy, ref long mediaCount)
        {
            List<Media> retVal = new List<Media>();
            SiteMapManager.GetInstance.GetSiteMapInstance(groupID, initObj.Platform, initObj.Locale);
            TVMAccountType account;
            if (mediaType > 0)
            {
                account = SiteMapManager.GetInstance.GetPageData(groupID, initObj.Platform).GetTVMAccountByMediaType(mediaType);
            }
            else
            {
                account = SiteMapManager.GetInstance.GetPageData(groupID, initObj.Platform).GetTVMAccountByAccountType(AccountType.Regular);
            }

            //Remote paging
            APISearchLoader searchLoader = new APISearchLoader(account.TVMUser, account.TVMPass) { Name = string.IsNullOrEmpty(text) ? null : text, GroupID = groupID, Platform = initObj.Platform, WithInfo = true, PageSize = pageSize, PageIndex = pageIndex, OrderBy = (TVPApi.OrderBy)orderBy, PictureSize = picSize };
            dsItemInfo mediaInfo = searchLoader.Execute();
            searchLoader.TryGetItemsCount(out mediaCount);
            if (mediaInfo.Item != null && mediaInfo.Item.Count > 0)
            {
                foreach (dsItemInfo.ItemRow row in mediaInfo.Item)
                {
                    retVal.Add(new Media(row, initObj, groupID, false));
                }
            }

            return retVal;
        }

        public static List<string> GetAutoCompleteList(int groupID, PlatformType platform, int subGroupID)
        {
            TVMAccountType account = SiteMapManager.GetInstance.GetPageData(groupID, platform).GetTVMAccountByAccountType(AccountType.Regular);
            string[] arrMetaNames = ConfigManager.GetInstance().GetConfig(groupID, platform).MediaConfiguration.Data.TVM.AutoCompleteValues.Metadata.ToString().Split(new Char[] { ';' });
            string[] arrTagNames = ConfigManager.GetInstance().GetConfig(groupID, platform).MediaConfiguration.Data.TVM.AutoCompleteValues.Tags.ToString().Split(new Char[] { ';' });

            CustomAutoCompleteLoader customAutoCompleteLoader = new APICustomAutoCompleteLoader(account.TVMUser, account.TVMPass) { MetaNames = arrMetaNames, TagNames = arrTagNames, Platform = platform, GroupID = groupID };
            List<String> lstResponse = new List<String>(customAutoCompleteLoader.Execute());

            return lstResponse;
        }

        //Call search protocol
        public static List<Media> SearchMedia(InitializationObject initObj, int mediaType, string text, string picSize, int pageSize, int pageIndex, int groupID, int subGroupID, int orderBy)
        {
            List<Media> retVal = new List<Media>();
            SiteMapManager.GetInstance.GetSiteMapInstance(groupID, initObj.Platform, initObj.Locale);
            TVMAccountType account;
            if (mediaType > 0)
            {
                account = SiteMapManager.GetInstance.GetPageData(groupID, initObj.Platform).GetTVMAccountByMediaType(mediaType);
            }
            else
            {
                account = SiteMapManager.GetInstance.GetPageData(groupID, initObj.Platform).GetTVMAccountByAccountType(AccountType.Regular);
            }

            //Remote paging
            dsItemInfo mediaInfo = (new APISearchLoader(account.TVMUser, account.TVMPass) { Name = string.IsNullOrEmpty(text) ? null : text, PictureSize = picSize, GroupID = groupID, Platform = initObj.Platform, WithInfo = true, PageSize = pageSize, PageIndex = pageIndex, OrderBy = (TVPApi.OrderBy)orderBy }.Execute());
            if (mediaInfo.Item != null && mediaInfo.Item.Count > 0)
            {
                foreach (dsItemInfo.ItemRow row in mediaInfo.Item)
                {
                    retVal.Add(new Media(row, initObj, groupID, false));
                }
            }

            return retVal;
        }

        public static List<Media> GetRelatedMediaList(InitializationObject initObj, int mediaID, int mediaType, string picSize, int pageSize, int pageIndex, int groupID)
        {
            TVMAccountType account;;
            if (mediaType != 0)
            {
                account = SiteMapManager.GetInstance.GetPageData(groupID, initObj.Platform).GetTVMAccountByMediaType(mediaType);
            }
            else
            {
                account = SiteMapManager.GetInstance.GetPageData(groupID, initObj.Platform).GetTVMAccountByAccountType(AccountType.Regular);
            }
            return GetMediaList(initObj, account.TVMUser, account.TVMPass, mediaID, picSize, pageSize, pageIndex, groupID, LoaderType.Related);
        }

        public static List<Media> GetRelatedMediaList(InitializationObject initObj, int mediaID, int mediaType, string picSize, int pageSize, int pageIndex, int groupID, ref long mediaCount)
        {
            TVMAccountType account;
            
            if (mediaType != 0)
            {
                account = SiteMapManager.GetInstance.GetPageData(groupID, initObj.Platform).GetTVMAccountByMediaType(mediaType);
            }
            else
            {
                account = SiteMapManager.GetInstance.GetPageData(groupID, initObj.Platform).GetTVMAccountByAccountType(AccountType.Regular);
            }

            List<Media> lstMedia = GetMediaList(initObj, account.TVMUser, account.TVMPass, mediaID, picSize, pageSize, pageIndex, groupID, LoaderType.Related, ref mediaCount);
            return lstMedia;
        }

        public static List<Media> GetRelatedMediaList(InitializationObject initObj, int mediaID, int mediaType, string picSize, int pageSize, int pageIndex, int groupID, int subGroupID)
        {
            TVMAccountType account = SiteMapManager.GetInstance.GetPageData(groupID, initObj.Platform).GetTVMAccountByAccountType(AccountType.Regular);
            return GetMediaList(initObj, account.TVMUser, account.TVMPass, mediaID, picSize, pageSize, pageIndex, groupID, LoaderType.Related);
        }

        public static List<Media> GetRelatedMediaList(InitializationObject initObj, string tvmPass, string tvmUser, int mediaID, int mediaType, string picSize, int pageSize, int pageIndex, int groupID,ref long mediaCount)
        {
            //TVMAccountType account = PageData.GetInstance.GetMember(groupID, initObj.Platform).GetTVMAccountByGroupID(subGroupID);
            return GetMediaList(initObj, tvmUser, tvmPass, mediaID, picSize, pageSize, pageIndex, groupID, LoaderType.Related, ref mediaCount);
        }

        public static List<Media> GetPeopleWhoWatchedList(InitializationObject initObj, long mediaID, int mediaType, string picSize, int pageSize, int pageIndex, int groupID)
        {
            TVMAccountType account = SiteMapManager.GetInstance.GetPageData(groupID, initObj.Platform).GetTVMAccountByMediaType(mediaType);
            return GetMediaList(initObj, account.TVMUser, account.TVMPass, mediaID, picSize, pageSize, pageIndex, groupID, LoaderType.PeopleWhoWatched);
        }

        public static List<Media> GetUserSocialMedias(InitializationObject initObj, string picSize, int pageSize, int pageIndex, int groupID, string socialAction, string socialPlatform)
        {
            List<Media> retVal = new List<Media>();

            TVMAccountType account = SiteMapManager.GetInstance.GetPageData(groupID, initObj.Platform).GetTVMAccountByAccountType(AccountType.Parent);
            dsItemInfo mediaInfo = new APIUserSocialMediasLoader(account.TVMUser, account.TVMPass, picSize) { WithInfo = true, SiteGuid = initObj.SiteGuid, Platform = initObj.Platform, SocialAction = socialAction, SocialPlatform = socialPlatform, GroupID = groupID, PageSize = pageSize, PageIndex = pageIndex }.Execute();

            IEnumerable<dsItemInfo.ItemRow> pagedDT;
            if (mediaInfo.Item != null && mediaInfo.Item.Count > 0)
            {
                int startIndex = (pageIndex) * pageSize;
                //Local server Paging
                //IEnumerable<dsItemInfo.ItemRow> pagedDT;
                //if (!isPaged)
                //{
                //    pagedDT = PagingHelper.GetPagedData<dsItemInfo.ItemRow>(startIndex, pageSize, mediaInfo.Item);
                //}
                //else
                {
                    pagedDT = mediaInfo.Item;
                }
                //Parse to WS return objects
                if (pagedDT != null)
                {
                    foreach (dsItemInfo.ItemRow row in pagedDT)
                    {
                        retVal.Add(new Media(row, initObj, groupID, false));
                    }
                }
            }

            return retVal;
        }

        public static List<Media> GetLastWatchedMedias(InitializationObject initObj, string picSize, int pageSize, int pageIndex, int groupID)
        {
            TVMAccountType account = SiteMapManager.GetInstance.GetPageData(groupID, initObj.Platform).GetTVMAccountByAccountType(AccountType.Regular);
            return GetMediaList(initObj, account.TVMUser, account.TVMPass, 0, picSize, pageSize, pageIndex, groupID, LoaderType.LastWatched);
        }

        public static List<Media> GetChannelMediaList(InitializationObject initObj, long channelID, string picSize, int pageSize, int pageIndex, int groupID)
        {
            List<Media> lstRet = new List<Media>();
            TVMAccountType account = SiteMapManager.GetInstance.GetPageData(groupID, initObj.Platform).GetTVMAccountByAccountType(AccountType.Regular);
            lstRet = GetMediaList(initObj, account.TVMUser, account.TVMPass, channelID, picSize, pageSize, pageIndex, groupID, LoaderType.Channel);
            if (lstRet == null || lstRet.Count == 0)
            {
                account = SiteMapManager.GetInstance.GetPageData(groupID, initObj.Platform).GetTVMAccountByAccountType(AccountType.Fictivic);
                lstRet = GetMediaList(initObj, account.TVMUser, account.TVMPass, channelID, picSize, pageSize, pageIndex, groupID, LoaderType.Channel);

                if (lstRet == null || lstRet.Count == 0)
                {
                    account = SiteMapManager.GetInstance.GetPageData(groupID, initObj.Platform).GetTVMAccountByAccountType(AccountType.Parent);
                    lstRet = GetMediaList(initObj, account.TVMUser, account.TVMPass, channelID, picSize, pageSize, pageIndex, groupID, LoaderType.Channel);
                }
            }

            return lstRet;
        }

        public static List<Media> GetChannelMediaList(InitializationObject initObj, long channelID, string picSize, int pageSize, int pageIndex, int groupID, ref long mediaCount)
        {
            TVMAccountType account = SiteMapManager.GetInstance.GetPageData(groupID, initObj.Platform).GetTVMAccountByAccountType(AccountType.Regular);
            return GetMediaList(initObj, account.TVMUser, account.TVMPass, channelID, picSize, pageSize, pageIndex, groupID, LoaderType.Channel, ref mediaCount);
        }

        public static List<Media> GetChannelMediaList(InitializationObject initObj, string user, string pass, long channelID, string picSize, int pageSize, int pageIndex, int groupID, ref long itemCount)
        {
            //TVMAccountType account = SiteMapManager.GetInstance.GetPageData[SiteMapManager.GetInstance.GetKey(groupID, initObj.Platform)].GetTVMAccountByGroupID(subGroupID);
            return GetMediaList(initObj, user, pass, channelID, picSize, pageSize, pageIndex, groupID, LoaderType.Channel, ref itemCount);
        }

        //Get all channel medias
        public static List<Media> GetMediaList(InitializationObject initObj, string user, string pass, long ID, string picSize, int pageSize, int pageIndex, int groupID, LoaderType loaderType, ref long mediaCount)
        {
            List<Media> retVal = new List<Media>();
            dsItemInfo mediaInfo;
            bool isPaged = false;
            switch (loaderType)
            {
                case LoaderType.Channel:
                    APIChannelLoader channelLoader = new APIChannelLoader(user, pass, ID, picSize) { WithInfo = true, GroupID = groupID, Platform = initObj.Platform, PageSize = pageSize, PageIndex = pageIndex };
                    mediaInfo = channelLoader.Execute();
                    channelLoader.TryGetItemsCount(out mediaCount);
                    isPaged = true;
                    break;
                case LoaderType.Related:
                    APIRelatedMediaLoader relatedLoader = new APIRelatedMediaLoader(ID, user, pass) { GroupID = groupID, Platform = initObj.Platform, PicSize = picSize, WithInfo = true, PageSize = pageSize, PageIndex = pageIndex, IsPosterPic = false };
                    mediaInfo = relatedLoader.Execute();
                    relatedLoader.TryGetItemsCount(out mediaCount);
                    isPaged = true;
                    break;
                case LoaderType.PeopleWhoWatched:
                    mediaInfo = (new APIPeopleWhoWatchedLoader(user, pass, ID, picSize) { GroupID = groupID, Platform = initObj.Platform, WithInfo = true, IsPosterPic = false }).Execute();
                    isPaged = false;
                    break;
                case LoaderType.LastWatched:
                    mediaInfo = new APILastWatchedLoader(user, pass) { GroupID = groupID, Platform = initObj.Platform, WithInfo = true, SiteGuid = initObj.SiteGuid, PageSize = pageSize, PageIndex = pageIndex, PicSize = picSize }.Execute();
                    break;
                default:
                    mediaInfo = (new APIChannelLoader(user, pass, ID, picSize) { WithInfo = true, GroupID = groupID, Platform = initObj.Platform }.Execute());
                    break;
            }

            if (mediaInfo.Item != null && mediaInfo.Item.Count > 0)
            {
                int startIndex = (pageIndex) * pageSize;
                //Local server Paging
                IEnumerable<dsItemInfo.ItemRow> pagedDT;
                if (!isPaged)
                {
                    pagedDT = PagingHelper.GetPagedData<dsItemInfo.ItemRow>(startIndex, pageSize, mediaInfo.Item);
                }
                else
                {
                    pagedDT = mediaInfo.Item;
                }
                //Parse to WS return objects
                if (pagedDT != null)
                {
                    foreach (dsItemInfo.ItemRow row in pagedDT)
                    {
                        retVal.Add(new Media(row, initObj, groupID, false));
                    }
                }
            }
            return retVal;
        }

        //Get all channel medias
        public static List<Media> GetMediaList(InitializationObject initObj, string user, string pass, long ID, string picSize, int pageSize, int pageIndex, int groupID, LoaderType loaderType)
        {
            long mediaCount = 0;
            return GetMediaList(initObj, user, pass, ID, picSize, pageSize, pageIndex, groupID, loaderType, ref mediaCount);
        }

        //Get User Items (favorites, Purchases, Packages)
        public static List<Media> GetUserItems(InitializationObject initObj, UserItemType userItemType, int mediaType, string picSize, int pageSize, int pageIndex, int groupID)
        {
            List<Media> retVal = new List<Media>();
            dsItemInfo mediaInfo = null;
            string guid = initObj.SiteGuid;
            if (initObj.Platform == PlatformType.STB)
            {
                guid = UsersXMLParser.Instance.GetGuid(initObj.Platform.ToString(), guid);
            }
            TVMAccountType parentAccount = SiteMapManager.GetInstance.GetPageData(groupID, initObj.Platform).GetTVMAccountByAccountType(AccountType.Parent);
            TVMAccountType regAccount = SiteMapManager.GetInstance.GetPageData(groupID, initObj.Platform).GetTVMAccountByAccountType(AccountType.Regular);
            switch (userItemType)
            {

                case UserItemType.Favorite:
                    {
                        FavoritObject[] favoritesObj = new ApiUsersService(groupID, initObj.Platform).GetUserFavorites(initObj.SiteGuid, string.Empty, initObj.DomainID, string.Empty);//initObj.UDID);
                        string[] favoritesList;// = TVPPro.SiteManager.Helper.FavoritesHelper.GetUserFavoriteMedias(mediaType, guid, true);

                        if (favoritesObj != null)
                        {
                            favoritesList = new string[favoritesObj.Length];
                            for (int i = 0; i < favoritesObj.Length; i++)
                            {
                                favoritesList[i] = favoritesObj[i].m_sItemCode;
                            }
                            int tempMediaType = 1;
                            if (mediaType > 0)
                            {
                                tempMediaType = mediaType;
                            }
                            mediaInfo = (new APIMultiMediaLoader(parentAccount.TVMUser, parentAccount.TVMPass, favoritesList, picSize, tempMediaType) { GroupID = groupID, Platform = initObj.Platform, PageSize = 20 }).Execute();
                        }
                        //LogManager.Instance.Log(groupID, "Favorites", string.Format("Found {0} favorites", favoritesList != null ? favoritesList.Length.ToString() : "0")); 
                        
                        break;
                    }
                case UserItemType.Rental:
                    {
                        PermittedMediaContainer[] MediaPermitedItems = new ApiConditionalAccessService(groupID, initObj.Platform).GetUserPermittedItems(guid);
                        mediaInfo = (new TVMRentalMultiMediaLoader(parentAccount.TVMUser, parentAccount.TVMPass, picSize, 1) {  MediasIdCotainer = MediaPermitedItems }).Execute();
                        break;
                    }
                case UserItemType.Package:
                    {
                        PermittedSubscriptionContainer[] PermitedPackages = new ApiConditionalAccessService(groupID, initObj.Platform).GetUserPermitedSubscriptions(guid);
                        if (PermitedPackages != null && PermitedPackages.Length > 0)
                        {
                            Dictionary<string, string> BaseIdsDict = new Dictionary<string, string>();
                            StringBuilder sb = new StringBuilder();

                            foreach (PermittedSubscriptionContainer sub in PermitedPackages)
                            {
                                sb.AppendFormat("{0}{1}", sub.m_sSubscriptionCode, ";");
                                if (!BaseIdsDict.ContainsKey("Base ID"))
                                {
                                    BaseIdsDict.Add("Base ID", sub.m_sSubscriptionCode);
                                }
                                else
                                {
                                    BaseIdsDict["Base ID"] = string.Concat(BaseIdsDict["Base ID"], ";", sub.m_sSubscriptionCode);
                                }
                            }
                            if (BaseIdsDict != null && BaseIdsDict.Count > 0)
                            {
                                TVMAccountType fictivicAccount = SiteMapManager.GetInstance.GetPageData(groupID, initObj.Platform).GetTVMAccountByAccountType(AccountType.Fictivic);

                                mediaInfo = new SearchMediaLoader(fictivicAccount.TVMUser, fictivicAccount.TVMPass) { dictMetas = BaseIdsDict, MetaValues = sb.ToString(), SearchTokenSignature = sb.ToString(), PageSize = 20, PictureSize = picSize }.Execute();
                            }
                        }
                    
                        break;
                    }
                    
                default:
                    break;
            }
            //LogManager.Instance.Log(groupID, "UserItems", string.Format("Found {0} medias from loader", (mediaInfo.Item != null && mediaInfo.Item.Count > 0) ? mediaInfo.Item.Count.ToString() : "0"));
            if (mediaInfo != null && mediaInfo.Item != null && mediaInfo.Item.Count > 0)
            {
                int startIndex = (pageIndex) * pageSize;
                //Local server Paging
                IEnumerable<dsItemInfo.ItemRow> pagedDT = PagingHelper.GetPagedData<dsItemInfo.ItemRow>(startIndex, pageSize, mediaInfo.Item);
                //Parse to WS return objects
                foreach (dsItemInfo.ItemRow row in pagedDT)
                {
                    retVal.Add(new Media(row, initObj, groupID, false));
                }
            }
            return retVal;
        }


        //Parse a data row elenment to media 
        private static Media parseItemRowToMedia(dsItemInfo.ItemRow row, string picSize, bool withDynamic, string siteGuid)
        {
            Media retVal = new Media();
            retVal.MediaName = row.Title;
            retVal.MediaID = row.ID;
            if (!row.IsMediaTypeIDNull())
            {
                retVal.MediaTypeID = row.MediaTypeID;
            }
            if (!row.IsMediaTypeNull())
            {
                retVal.MediaTypeName = row.MediaType;
            }
            if (!row.IsDescriptionShortNull())
            {
                retVal.Description = row.DescriptionShort;
            }

            if (!row.IsImageLinkNull())
            {
                retVal.PicURL = row.ImageLink;
            }
            if (!row.IsURLNull())
            {
                retVal.URL = row.URL;
            }

            if (!row.IsDurationNull())
            {
                retVal.Duration = row.Duration;
            }

            if (!row.IsRateNull())
            {
                retVal.Rating = row.Rate;
            }
            if (!row.IsViewCounterNull())
            {
                retVal.ViewCounter = row.ViewCounter;
            }
            string[] TagNames = MediaConfiguration.Instance.Data.TVM.MediaInfoStruct.Tags.ToString().Split(new Char[] { ';' });
            System.Data.DataRow[] tagsRow = row.GetChildRows("Item_Tags");
            if (tagsRow != null && tagsRow.Length > 0)
            {
                //Create tag meta pair objects list for all tags
                foreach (string tagName in TagNames)
                {
                    if (tagsRow[0].Table.Columns.Contains(tagName) && !string.IsNullOrEmpty(tagsRow[0][tagName].ToString()))
                    {
                        TagMetaPair pair = new TagMetaPair(tagName, tagsRow[0][tagName].ToString());
                        retVal.Tags.Add(pair);
                    }
                }
            }
            string[] MetaNames = MediaConfiguration.Instance.Data.TVM.MediaInfoStruct.Metadata.ToString().Split(new Char[] { ';' });
            System.Data.DataRow[] metasRow = row.GetChildRows("Item_Metas");
            if (metasRow != null && metasRow.Length > 0)
            {
                //Create tag meta pair objects list for all metas
                foreach (string metaName in MetaNames)
                {
                    if (metasRow[0].Table.Columns.Contains(metaName) && !string.IsNullOrEmpty(metasRow[0][metaName].ToString()))
                    {
                        TagMetaPair pair = new TagMetaPair(metaName, metasRow[0][metaName].ToString());
                        retVal.Metas.Add(pair);
                    }
                }
            }
            if (withDynamic)
            {
                DynamicData dynamicObj = new DynamicData();

            }
            return retVal;
        }

        public static List<Media> GetMediasInPackage(InitializationObject initObj, long sBaseID, int iGroupID, string picSize, int pageSize, int pageIndex)
        {
            List<Media> retVal = new List<Media>();

            TVMAccountType parentAccount = SiteMapManager.GetInstance.GetPageData(iGroupID, initObj.Platform).GetTVMAccountByAccountType(AccountType.Parent);
            dsItemInfo mediaInfo = new APITVMSubscriptionMediaLoader(parentAccount.TVMUser, parentAccount.TVMPass, sBaseID) { WithInfo = true, Platform = initObj.Platform, GroupID = iGroupID, PageIndex = pageIndex, PageSize = pageSize, PicSize = picSize, BaseID = sBaseID }.Execute();

            if (mediaInfo.Item != null && mediaInfo.Item.Count > 0)
            {
                foreach (dsItemInfo.ItemRow row in mediaInfo.Item.Rows)
                {
                    retVal.Add(new Media(row, initObj, iGroupID, false));
                }
            }

            return retVal;
        }
    }
}
