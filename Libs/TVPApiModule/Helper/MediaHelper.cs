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
using Tvinci.Data.Loaders.TvinciPlatform.Catalog;
using TVPApiModule.CatalogLoaders;
using TVPApiModule.Objects.Responses;

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
            LastWatched,
            Recommended,
            RelatedExternal
        }

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

        //Get media info
        public static Media GetMediaInfo(InitializationObject initObj, long MediaID, int mediaType, string picSize, int groupID, bool withDynamic)
        {
            Media retVal = null;
            TVMAccountType account = SiteMapManager.GetInstance.GetPageData(groupID, initObj.Platform).GetTVMAccountByAccountType(AccountType.Parent);
            dsItemInfo mediaInfo = (new APIMediaLoader(account.TVMUser, account.TVMPass, MediaID.ToString()) { GroupID = groupID, Platform = initObj.Platform, PicSize = picSize, DeviceUDID = initObj.UDID, Language = initObj.Locale.LocaleLanguage, SiteGuid = initObj.SiteGuid }.Execute());
            if (mediaInfo.Item != null && mediaInfo.Item.Count == 1)
            {
                dsItemInfo.ItemRow row = mediaInfo.Item.Rows[0] as dsItemInfo.ItemRow;
                if (row != null)
                    retVal = new Media(row, initObj, groupID, withDynamic, mediaInfo.Item.Count);
            }

            return retVal;
        }

        public static Media GetMediaInfo(InitializationObject initObj, long MediaID, string picSize, int groupID)
        {
            Media retVal = null;
            TVMAccountType account = SiteMapManager.GetInstance.GetPageData(groupID, initObj.Platform).GetTVMAccountByAccountType(AccountType.Parent);
            dsItemInfo mediaInfo = (new APIMediaLoader(account.TVMUser, account.TVMPass, MediaID.ToString()) { GroupID = groupID, Platform = initObj.Platform, PicSize = picSize, Language = initObj.Locale.LocaleLanguage, SiteGuid = initObj.SiteGuid }.Execute());
            if (mediaInfo.Item != null && mediaInfo.Item.Count == 1)
            {
                dsItemInfo.ItemRow row = mediaInfo.Item.Rows[0] as dsItemInfo.ItemRow;
                if (row != null)
                    retVal = new Media(row, initObj, groupID, false, mediaInfo.Item.Count);
            }

            return retVal;
        }

        public static List<Media> GetMediasInfo(InitializationObject initObj, long[] MediaIDs, int mediaType, string picSize, int groupID, bool withDynamic)
        {
            List<Media> retVal = new List<Media>();
            TVMAccountType account = SiteMapManager.GetInstance.GetPageData(groupID, initObj.Platform).GetTVMAccountByAccountType(AccountType.Parent);
            dsItemInfo mediaInfo = (new APIMultiMediaLoader(account.TVMUser, account.TVMPass, MediaIDs.Select(i => i.ToString()).ToArray(), picSize, mediaType) { GroupID = groupID, Platform = initObj.Platform, PicSize = picSize, Language = initObj.Locale.LocaleLanguage, SiteGuid = initObj.SiteGuid }.Execute());

            foreach (dsItemInfo.ItemRow row in mediaInfo.Item.Rows)
                retVal.Add(new Media(row, initObj, groupID, withDynamic, mediaInfo.Item.Count));

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

        public static List<KeyValuePair<long, bool>> AreMediasFavorite(InitializationObject initObj, int groupID, List<long> mediaIDs)
        {
            List<KeyValuePair<long, bool>> retVal = new List<KeyValuePair<long, bool>>();

            // get all user favorites
            FavoritObject[] favoriteObjects = new ApiUsersService(groupID, initObj.Platform).GetUserFavorites(initObj.SiteGuid, string.Empty, initObj.DomainID, string.Empty);

            if (favoriteObjects != null)
                retVal = mediaIDs.Select(y => new KeyValuePair<long, bool>(y, favoriteObjects.Where(x => x.m_sItemCode == y.ToString()).Count() > 0)).ToList();

            return retVal;
        }

        public static List<Media> SearchMediaByTag(InitializationObject initObj, int mediaType, List<TVPApi.TagMetaPair> tagPairs, string picSize, int pageSize, int pageIndex, int groupID, int orderBy)
        {
            List<Media> retVal = new List<Media>();
            SiteMapManager.GetInstance.GetSiteMapInstance(groupID, initObj.Platform, initObj.Locale);
            TVMAccountType account = SiteMapManager.GetInstance.GetPageData(groupID, initObj.Platform).GetTVMAccountByGroupID(groupID);

            Dictionary<string, string> dictTags = tagPairs.ToDictionary((keyItem) => keyItem.Key, (valueItem) => valueItem.Value);

            string sSigature = string.Join("|", dictTags.Select(pair => string.Format("{0}={1}", pair.Key, pair.Value)).ToArray());

            // create a signature for search loader
            //string sSigature = string.Format(@"{0}={1}|{2}|{3}", tagName, value, groupID, initObj.Platform);

            APISearchLoader searchLoader = new APISearchLoader(account.TVMUser, account.TVMPass, dictTags) 
                { 
                    MediaType = mediaType,
                    SearchTokenSignature = sSigature,
                    Platform = initObj.Platform,
                    GroupID = groupID, WithInfo = true,
                    PageSize = pageSize,
                    PageIndex = pageIndex,
                    OrderBy = (OrderBy)orderBy,
                    PictureSize = picSize,
                    CutType = SearchMediaLoader.eCutType.And,
                    DeviceUDID = initObj.UDID,
                    Country = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).IpToCountry(TVPPro.SiteManager.Helper.SiteHelper.GetClientIP()),
                    Language = initObj.Locale.LocaleLanguage ,
                    SiteGuid = initObj.SiteGuid,
                    DomainID = initObj.DomainID
                };

            dsItemInfo mediaInfo = searchLoader.Execute();
            long mediaCount = 0;
            searchLoader.TryGetItemsCount(out mediaCount);

            if (mediaInfo != null && mediaInfo.Item.Count > 0)
            {
                dsItemInfo.ItemRow row = mediaInfo.Item.Rows[0] as dsItemInfo.ItemRow;
                if (row != null)
                    retVal.Add(new Media(row, initObj, groupID, true, mediaInfo.Item.Count));
            }

            return retVal;
        }

        public static List<Media> SearchMediaByMetasTags(InitializationObject initObj, int mediaType, List<TagMetaPair> tagPairs, List<TagMetaPair> metaPairs, string picSize, int pageSize, int pageIndex, int groupID, int orderBy)
        {
            List<Media> retVal = new List<Media>();
            SiteMapManager.GetInstance.GetSiteMapInstance(groupID, initObj.Platform, initObj.Locale);
            TVMAccountType account = SiteMapManager.GetInstance.GetPageData(groupID, initObj.Platform).GetTVMAccountByGroupID(groupID);

            Dictionary<string, string> dictTags = tagPairs.ToDictionary((keyItem) => keyItem.Key, (valueItem) => valueItem.Value);
            Dictionary<string, string> dictMetas = metaPairs.ToDictionary((keyItem) => keyItem.Key, (valueItem) => valueItem.Value);

            string sSigature = string.Format("{0}|{1}", string.Join("|", dictTags.Select(pair => string.Format("{0}={1}", pair.Key, pair.Value)).ToArray()), string.Join("|", dictMetas.Select(pair => string.Format("{0}={1}", pair.Key, pair.Value)).ToArray()));

            // create a signature for search loader
            //string sSigature = string.Format(@"{0}={1}|{2}|{3}", tagName, value, groupID, initObj.Platform);

            APISearchLoader searchLoader = new APISearchLoader(account.TVMUser, account.TVMPass, dictTags) 
            {
                dictMetas = dictMetas, 
                MediaType = mediaType, 
                SearchTokenSignature = sSigature, 
                Platform = initObj.Platform, 
                GroupID = groupID, 
                WithInfo = true, 
                PageSize = pageSize, 
                PageIndex = pageIndex, 
                OrderBy = (OrderBy)orderBy, 
                PictureSize = picSize, 
                CutType = SearchMediaLoader.eCutType.And, 
                DeviceUDID = initObj.UDID, 
                Country = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).IpToCountry(TVPPro.SiteManager.Helper.SiteHelper.GetClientIP()), 
                Language = initObj.Locale.LocaleLanguage, 
                SiteGuid = initObj.SiteGuid,
                DomainID = initObj.DomainID
            };
            dsItemInfo mediaInfo = searchLoader.Execute();
            long mediaCount = 0;
            searchLoader.TryGetItemsCount(out mediaCount);

            if (mediaInfo.Item != null && mediaInfo.Item.Count > 0)
            {
                foreach (dsItemInfo.ItemRow row in mediaInfo.Item)
                {
                    retVal.Add(new Media(row, initObj, groupID, false, mediaCount));
                }
            }

            return retVal;
        }

        public static List<Media> SearchMediaByMetasTagsExact(InitializationObject initObj, int mediaType, List<TagMetaPair> tagPairs, List<TagMetaPair> metaPairs, string picSize, int pageSize, int pageIndex, int groupID, int orderBy)
        {
            List<Media> retVal = new List<Media>();
            SiteMapManager.GetInstance.GetSiteMapInstance(groupID, initObj.Platform, initObj.Locale);
            TVMAccountType account = SiteMapManager.GetInstance.GetPageData(groupID, initObj.Platform).GetTVMAccountByGroupID(groupID);

            Dictionary<string, string> dictTags = tagPairs.ToDictionary((keyItem) => keyItem.Key, (valueItem) => valueItem.Value);
            Dictionary<string, string> dictMetas = metaPairs.ToDictionary((keyItem) => keyItem.Key, (valueItem) => valueItem.Value);

            string sSigature = string.Format("{0}|{1}", string.Join("|", dictTags.Select(pair => string.Format("{0}={1}", pair.Key, pair.Value)).ToArray()), string.Join("|", dictMetas.Select(pair => string.Format("{0}={1}", pair.Key, pair.Value)).ToArray()));

            // create a signature for search loader
            //string sSigature = string.Format(@"{0}={1}|{2}|{3}", tagName, value, groupID, initObj.Platform);

            APISearchLoader searchLoader = new APISearchLoader(account.TVMUser, account.TVMPass, dictTags) 
            { 
                ExactSearch = true, 
                dictMetas = dictMetas, 
                MediaType = mediaType, 
                SearchTokenSignature = sSigature, 
                Platform = initObj.Platform, 
                GroupID = groupID, 
                WithInfo = true, 
                PageSize = pageSize, 
                PageIndex = pageIndex, 
                OrderBy = (OrderBy)orderBy, 
                PictureSize = picSize, 
                CutType = SearchMediaLoader.eCutType.And, 
                DeviceUDID = initObj.UDID, 
                Language = initObj.Locale.LocaleLanguage, 
                SiteGuid = initObj.SiteGuid,
                DomainID = initObj.DomainID
            };
            dsItemInfo mediaInfo = searchLoader.Execute();
            long mediaCount = 0;
            searchLoader.TryGetItemsCount(out mediaCount);

            if (mediaInfo.Item != null && mediaInfo.Item.Count > 0)
            {
                foreach (dsItemInfo.ItemRow row in mediaInfo.Item)
                {
                    retVal.Add(new Media(row, initObj, groupID, false, mediaCount));
                }
            }
            return retVal;
        }

        public static List<Media> SearchMediaByAndOrList(InitializationObject initObj, int mediaType, List<KeyValue> orList, List<KeyValue> andList, string picSize, int pageSize, int pageIndex, int groupID, bool exact, Tvinci.Data.Loaders.TvinciPlatform.Catalog.OrderBy orderBy, Tvinci.Data.Loaders.TvinciPlatform.Catalog.OrderDir orderDir, string orderMeta)
        {
            List<Media> retVal = new List<Media>();
            //SiteMapManager.GetInstance.GetSiteMapInstance(groupID, initObj.Platform, initObj.Locale);

            APISearchMediaLoader searchLoader = new APISearchMediaLoader(groupID, initObj.Platform, TVPPro.SiteManager.Helper.SiteHelper.GetClientIP(), pageSize, pageIndex, picSize, exact, orList, andList, new List<int>() {mediaType}) 
            {
                DeviceId = initObj.UDID, 
                OrderMetaMame = orderMeta, 
                OrderBy = orderBy, 
                OrderDir = orderDir ,
                SiteGuid = initObj.SiteGuid,
                Culture = initObj.Locale.LocaleLanguage,
                DomainId = initObj.DomainID
            };

            //Removed 30/12/2013. Using orderBy and orderDir and orderMeta from request.

                //if (orderBy != (int)TVPApi.OrderBy.None)
                //{
                //    searchLoader.OrderBy = TVPApiModule.Helper.APICatalogHelper.GetCatalogOrderBy(orderBy);
                //    // XXX: For specific date sorting, make this by Descending
                //    if (searchLoader.OrderBy == Tvinci.Data.Loaders.TvinciPlatform.Catalog.OrderBy.START_DATE || searchLoader.OrderBy == Tvinci.Data.Loaders.TvinciPlatform.Catalog.OrderBy.CREATE_DATE)
                //        searchLoader.OrderDir = Tvinci.Data.Loaders.TvinciPlatform.Catalog.OrderDir.DESC;
                //    else
                //        searchLoader.OrderDir = Tvinci.Data.Loaders.TvinciPlatform.Catalog.OrderDir.ASC;
                //    searchLoader.OrderMetaMame = string.Empty;
                //}

            dsItemInfo mediaInfo = searchLoader.Execute() as dsItemInfo;

            long mediaCount = 0;
            searchLoader.TryGetItemsCount(out mediaCount);

            if (mediaInfo.Item != null && mediaInfo.Item.Count > 0)
            {
                foreach (dsItemInfo.ItemRow row in mediaInfo.Item)
                {
                    retVal.Add(new Media(row, initObj, groupID, false, mediaCount));
                }
            }
            return retVal;
        }

        //Call search protocol
        public static List<Media> SearchMediaByTag(InitializationObject initObj, int mediaType, string tagName, string value, string picSize, int pageSize, int pageIndex, int groupID, int orderBy)
        {
            List<Media> retVal = new List<Media>();
            SiteMapManager.GetInstance.GetSiteMapInstance(groupID, initObj.Platform, initObj.Locale);
            TVMAccountType account = SiteMapManager.GetInstance.GetPageData(groupID, initObj.Platform).GetTVMAccountByGroupID(groupID);

            Dictionary<string, string> dictTags = new Dictionary<string, string>();
            dictTags.Add(tagName, value);

            // create a signature for search loader
            string sSigature = string.Format(@"{0}={1}|{2}|{3}", tagName, value, groupID, initObj.Platform);

            APISearchLoader searchLoader = new APISearchLoader(account.TVMUser, account.TVMPass, dictTags) 
            { 
                MediaType = mediaType, 
                SearchTokenSignature = sSigature, 
                Platform = initObj.Platform, 
                GroupID = groupID, 
                WithInfo = true, 
                PageSize = pageSize, 
                PageIndex = pageIndex, 
                OrderBy = (OrderBy)orderBy, 
                PictureSize = picSize, 
                DeviceUDID = initObj.UDID, 
                Country = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).IpToCountry(TVPPro.SiteManager.Helper.SiteHelper.GetClientIP()), 
                Language = initObj.Locale.LocaleLanguage, 
                SiteGuid = initObj.SiteGuid,
                DomainID = initObj.DomainID
            };
            dsItemInfo mediaInfo = searchLoader.Execute();
            long mediaCount = 0;
            searchLoader.TryGetItemsCount(out mediaCount);

            if (mediaInfo.Item != null && mediaInfo.Item.Count > 0)
            {
                foreach (dsItemInfo.ItemRow row in mediaInfo.Item)
                {
                    retVal.Add(new Media(row, initObj, groupID, false, mediaCount));
                }
            }

            return retVal;
        }

        public static List<Media> SearchMediaBySubID(InitializationObject initObj, string sSubID, string picSize, int groupID, int orderBy)
        {
            List<Media> retVal = new List<Media>();
            SiteMapManager.GetInstance.GetSiteMapInstance(groupID, initObj.Platform, initObj.Locale);
            TVMAccountType account = SiteMapManager.GetInstance.GetPageData(groupID, initObj.Platform).GetTVMAccountByAccountType(AccountType.Parent);
            Dictionary<string, string> dictMetas = new Dictionary<string, string>();

            string[] arrValues = sSubID.Split(';');
            dictMetas.Add("Base ID", sSubID);

            //Remote paging
            APISearchLoader searchLoader = new APISearchLoader(account.TVMUser, account.TVMPass) 
            { 
                SearchTokenSignature = sSubID, 
                GroupID = groupID, 
                Platform = initObj.Platform, 
                dictMetas = dictMetas, 
                WithInfo = true, 
                PageSize = arrValues.Length, 
                PictureSize = picSize, 
                PageIndex = 0, 
                OrderBy = (OrderBy)orderBy, 
                MetaValues = sSubID, 
                UseFinalEndDate = "true", 
                DeviceUDID = initObj.UDID, 
                Country = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).IpToCountry(TVPPro.SiteManager.Helper.SiteHelper.GetClientIP()), 
                Language = initObj.Locale.LocaleLanguage, 
                SiteGuid = initObj.SiteGuid,
                DomainID = initObj.DomainID
            };
            dsItemInfo mediaInfo = searchLoader.Execute();
            long mediaCount = 0;
            searchLoader.TryGetItemsCount(out mediaCount);

            if (mediaInfo.Item != null && mediaInfo.Item.Count > 0)
            {
                foreach (dsItemInfo.ItemRow row in mediaInfo.Item)
                {
                    retVal.Add(new Media(row, initObj, groupID, false, mediaCount));
                }
            }

            return retVal;
        }

        //Call search protocol
        public static List<Media> SearchMediaByMeta(InitializationObject initObj, int mediaType, string metaName, string value, string picSize, int pageSize, int pageIndex, int groupID, int orderBy)
        {
            List<Media> retVal = new List<Media>();
            SiteMapManager.GetInstance.GetSiteMapInstance(groupID, initObj.Platform, initObj.Locale);
            TVMAccountType account = SiteMapManager.GetInstance.GetPageData(groupID, initObj.Platform).GetTVMAccountByGroupID(groupID);

            Dictionary<string, string> dictMetas = new Dictionary<string, string>();

            string[] arrValues = value.Split(';');
            foreach (string sValue in arrValues)
            {
                dictMetas.Add(metaName, sValue);
            }
            //Remote paging
            APISearchLoader searchLoader = new APISearchLoader(account.TVMUser, account.TVMPass) 
            { 
                MediaType = mediaType, 
                SearchTokenSignature = value, 
                GroupID = groupID, 
                Platform = initObj.Platform, 
                dictMetas = dictMetas, 
                WithInfo = true, 
                PageSize = pageSize, 
                PictureSize = picSize, 
                PageIndex = pageIndex, 
                OrderBy = (OrderBy)orderBy, 
                MetaValues = value, 
                DeviceUDID = initObj.UDID, 
                Country = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).IpToCountry(TVPPro.SiteManager.Helper.SiteHelper.GetClientIP()), 
                Language = initObj.Locale.LocaleLanguage, 
                SiteGuid = initObj.SiteGuid,
                DomainID = initObj.DomainID
            };
            dsItemInfo mediaInfo = searchLoader.Execute();
            long mediaCount = 0;
            searchLoader.TryGetItemsCount(out mediaCount);

            if (mediaInfo.Item != null && mediaInfo.Item.Count > 0)
            {
                foreach (dsItemInfo.ItemRow row in mediaInfo.Item)
                {
                    retVal.Add(new Media(row, initObj, groupID, false, mediaCount));
                }
            }

            return retVal;
        }

        //Call search protocol
        public static List<Media> SearchMediaByMeta(InitializationObject initObj, int mediaType, string metaName, string value, string picSize, int pageSize, int pageIndex, int groupID, int orderBy, ref long mediaCount)
        {
            List<Media> retVal = new List<Media>();
            SiteMapManager.GetInstance.GetSiteMapInstance(groupID, initObj.Platform, initObj.Locale);
            TVMAccountType account = SiteMapManager.GetInstance.GetPageData(groupID, initObj.Platform).GetTVMAccountByAccountType(AccountType.Parent);
            Dictionary<string, string> dictMetas = new Dictionary<string, string>();
            dictMetas.Add(metaName, value);

            //Remote paging
            APISearchLoader searchLoader = new APISearchLoader(account.TVMUser, account.TVMPass) 
            { 
                GroupID = groupID, 
                Platform = initObj.Platform, 
                dictMetas = dictMetas, 
                WithInfo = true, 
                PageSize = pageSize, 
                PictureSize = picSize, 
                PageIndex = pageIndex, 
                OrderBy = (OrderBy)orderBy, 
                IsPosterPic = false, 
                MetaValues = value, 
                DeviceUDID = initObj.UDID, 
                Country = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).IpToCountry(TVPPro.SiteManager.Helper.SiteHelper.GetClientIP()), 
                Language = initObj.Locale.LocaleLanguage, 
                SiteGuid = initObj.SiteGuid,
                DomainID = initObj.DomainID
            };
            dsItemInfo mediaInfo = searchLoader.Execute();
            searchLoader.TryGetItemsCount(out mediaCount);

            if (mediaInfo.Item != null && mediaInfo.Item.Count > 0)
            {
                foreach (dsItemInfo.ItemRow row in mediaInfo.Item)
                {
                    retVal.Add(new Media(row, initObj, groupID, false, mediaInfo.Item.Count));
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

            account = SiteMapManager.GetInstance.GetPageData(groupID, initObj.Platform).GetTVMAccountByGroupID(groupID);

            //Remote paging
            APISearchLoader searchLoader = new APISearchLoader(account.TVMUser, account.TVMPass)
            {
                MediaType = mediaType,
                Name = string.IsNullOrEmpty(text) ? null : text,
                PictureSize = picSize,
                GroupID = groupID,
                Platform = initObj.Platform,
                WithInfo = true,
                PageSize = pageSize,
                PageIndex = pageIndex,
                OrderBy = (TVPApi.OrderBy)orderBy,
                DeviceUDID = initObj.UDID,
                Country = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).IpToCountry(TVPPro.SiteManager.Helper.SiteHelper.GetClientIP()),
                Language = initObj.Locale.LocaleLanguage,
                SiteGuid = initObj.SiteGuid,
                DomainID = initObj.DomainID
            };

            dsItemInfo mediaInfo = searchLoader.Execute();
            long mediaCount = 0;
            searchLoader.TryGetItemsCount(out mediaCount);

            if (mediaInfo.Item != null && mediaInfo.Item.Count > 0)
            {
                foreach (dsItemInfo.ItemRow row in mediaInfo.Item)
                {
                    retVal.Add(new Media(row, initObj, groupID, false, mediaCount));
                }
            }

            return retVal;
        }

        //Call search protocols with multi types
        public static List<Media> SearchMedia(InitializationObject initObj, int[] mediaType, string text, string picSize, int pageSize, int pageIndex, int groupID, TVPApi.OrderBy orderBy)
        {
            List<Media> retVal = new List<Media>();

            long iTotal = 0;
            List<Media> tmpVal = new List<Media>();
            foreach (int type in mediaType)
            {
                List<Media> lst = SearchMedia(initObj, type, text, picSize, 100, 0, groupID, (int)orderBy);
                tmpVal.AddRange(lst);
                iTotal += lst.Count;
            }

            switch (orderBy)
            {
                case (TVPApi.OrderBy.Added):
                    tmpVal = tmpVal.OrderByDescending(m => m.CreationDate).ToList();
                    //copyObject.Item.DefaultView.Sort = "CreationDate desc";
                    break;
                case (TVPApi.OrderBy.Rating):
                    tmpVal = tmpVal.OrderByDescending(m => m.Rating).ToList();
                    //copyObject.Item.DefaultView.Sort = "Rate desc";
                    break;
                case (TVPApi.OrderBy.Views):
                    tmpVal = tmpVal.OrderByDescending(m => m.ViewCounter).ToList();
                    //copyObject.Item.DefaultView.Sort = "ViewCounter desc";
                    break;
                default:
                    tmpVal = tmpVal.OrderBy(m => m.MediaName).ToList();
                    //copyObject.Item.DefaultView.Sort = "Title asc";
                    break;
            }

            for (int i = 0; i < tmpVal.Count; i++)
            {
                if (i >= pageIndex * pageSize && i < (pageIndex + 1) * pageSize)
                {
                    retVal.Add(tmpVal[i]);
                }
            }

            iTotal = (iTotal > mediaType.Length * 50) ? mediaType.Length * 50 : iTotal;
            retVal.ForEach(i => i.TotalItems = iTotal);

            return retVal;
        }

        //Call search protocol
        public static List<Media> SearchMedia(InitializationObject initObj, int mediaType, string text, string picSize, int pageSize, int pageIndex, int groupID, int orderBy, ref long mediaCount)
        {
            List<Media> retVal = new List<Media>();
            SiteMapManager.GetInstance.GetSiteMapInstance(groupID, initObj.Platform, initObj.Locale);
            TVMAccountType account = SiteMapManager.GetInstance.GetPageData(groupID, initObj.Platform).GetTVMAccountByAccountType(AccountType.Parent);

            //Remote paging
            APISearchLoader searchLoader = new APISearchLoader(account.TVMUser, account.TVMPass) 
            { 
                Name = string.IsNullOrEmpty(text) ? null : text, 
                GroupID = groupID, 
                Platform = initObj.Platform, 
                WithInfo = true, 
                PageSize = pageSize, 
                PageIndex = pageIndex, 
                OrderBy = (TVPApi.OrderBy)orderBy, 
                PictureSize = picSize, 
                DeviceUDID = initObj.UDID, 
                Country = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).IpToCountry(TVPPro.SiteManager.Helper.SiteHelper.GetClientIP()), 
                Language = initObj.Locale.LocaleLanguage, 
                SiteGuid = initObj.SiteGuid,
                DomainID = initObj.DomainID
            };
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

        public static List<string> GetAutoCompleteList(int groupID, InitializationObject initObj, int[] iMediaTypes, string prefix, string lang, int pageIdx, int pageSize)
        {
            //TVMAccountType account = SiteMapManager.GetInstance.GetPageData(groupID, platform).GetTVMAccountByGroupID(groupID);
            //string[] arrMetaNames = ConfigManager.GetInstance().GetConfig(groupID, platform).MediaConfiguration.Data.TVM.AutoCompleteValues.Metadata.ToString().Split(new Char[] { ';' });
            //string[] arrTagNames = ConfigManager.GetInstance().GetConfig(groupID, platform).MediaConfiguration.Data.TVM.AutoCompleteValues.Tags.ToString().Split(new Char[] { ';' });
            //List<String> lstResponse = new List<String>();
            List<int> _mediaTypes = new List<int>();

            foreach (int __mediaType in iMediaTypes)
            {
                _mediaTypes.Add(__mediaType);
            }

            return new APIMediaAutoCompleteLoader(groupID, initObj.Platform.ToString(), SiteHelper.GetClientIP(), pageSize, pageIdx, prefix, _mediaTypes, initObj.Locale.LocaleLanguage)
            {
                SiteGuid = initObj.SiteGuid,
                DomainId = initObj.DomainID
            }.Execute() as List<string>;
            

            //return new ApiApiService(groupID, platform).GetAutoCompleteList(iMediaTypes, arrMetaNames, arrTagNames, prefix, lang, pageIdx, pageSize).ToList();
        }

        //Call search protocol
        public static List<Media> SearchMedia(InitializationObject initObj, int mediaType, string text, string picSize, int pageSize, int pageIndex, int groupID, int subGroupID, int orderBy)
        {
            List<Media> retVal = new List<Media>();
            SiteMapManager.GetInstance.GetSiteMapInstance(groupID, initObj.Platform, initObj.Locale);
            TVMAccountType account = SiteMapManager.GetInstance.GetPageData(groupID, initObj.Platform).GetTVMAccountByAccountType(AccountType.Parent);
           
            //Remote paging
            APISearchLoader searchLoader = new APISearchLoader(account.TVMUser, account.TVMPass) { Name = string.IsNullOrEmpty(text) ? null : text, PictureSize = picSize, GroupID = groupID, Platform = initObj.Platform, WithInfo = true, PageSize = pageSize, PageIndex = pageIndex, OrderBy = (TVPApi.OrderBy)orderBy, DeviceUDID = initObj.UDID, Country = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).IpToCountry(TVPPro.SiteManager.Helper.SiteHelper.GetClientIP()), Language = initObj.Locale.LocaleLanguage, SiteGuid = initObj.SiteGuid };
            dsItemInfo mediaInfo = searchLoader.Execute();
            long mediaCount = 0;
            searchLoader.TryGetItemsCount(out mediaCount);

            if (mediaInfo.Item != null && mediaInfo.Item.Count > 0)
            {
                foreach (dsItemInfo.ItemRow row in mediaInfo.Item)
                {
                    retVal.Add(new Media(row, initObj, groupID, false, mediaCount));
                }
            }

            return retVal;
        }

        public static List<Media> GetRecommendedMediasList(InitializationObject initObj, string picSize, int pageSize, int pageIndex, int groupID, int[] reqMediaTypes = null)
        {
            TVMAccountType account = SiteMapManager.GetInstance.GetPageData(groupID, initObj.Platform).GetTVMAccountByAccountType(AccountType.Parent);
            return GetMediaList(initObj, account.TVMUser, account.TVMPass, 0, picSize, pageSize, pageIndex, groupID, LoaderType.Recommended, OrderBy.None, reqMediaTypes);
        }

        public static List<Media> GetRelatedMediaList(InitializationObject initObj, int mediaID, int mediaType, string picSize, int pageSize, int pageIndex, int groupID, int[] reqMediaTypes = null)
        {
            TVMAccountType account; ;
            //if (mediaType != 0)
            //{
            //    account = SiteMapManager.GetInstance.GetPageData(groupID, initObj.Platform).GetTVMAccountByMediaType(mediaType);
            //}
            //else
            //{
            //    account = SiteMapManager.GetInstance.GetPageData(groupID, initObj.Platform).GetTVMAccountByAccountType(AccountType.Regular);
            //}
            account = SiteMapManager.GetInstance.GetPageData(groupID, initObj.Platform).GetTVMAccountByAccountType(AccountType.Parent);

            return GetMediaList(initObj, account.TVMUser, account.TVMPass, mediaID, picSize, pageSize, pageIndex, groupID, LoaderType.Related, OrderBy.None, reqMediaTypes);
        }

        public static TVPApiModule.Objects.Responses.UnifiedSearchResponseWithRequestId GetExternalRelatedMediaList(InitializationObject initObj, int mediaID, int pageSize, int pageIndex, int groupID, int[] reqMediaTypes = null, string freeParam = null, List<string> with = null)
        {
            TVMAccountType account; ;
            
            account = SiteMapManager.GetInstance.GetPageData(groupID, initObj.Platform).GetTVMAccountByAccountType(AccountType.Parent);

            return GetMediaExternalRelatedList(initObj, account.TVMUser, account.TVMPass, mediaID, pageSize, pageIndex, groupID, reqMediaTypes, freeParam, with);
        }

        public static TVPApiModule.Objects.Responses.UnifiedSearchResponseWithRequestId GetExternalSearchMediaList(InitializationObject initObj, string query, int pageSize, int pageIndex, int groupID, int[] reqMediaTypes = null, List<string> with = null)
        {
            TVMAccountType account; ;

            account = SiteMapManager.GetInstance.GetPageData(groupID, initObj.Platform).GetTVMAccountByAccountType(AccountType.Parent);

            return GetMediaExternalSearchList(initObj, account.TVMUser, account.TVMPass, query, pageSize, pageIndex, groupID, OrderBy.None, reqMediaTypes, with);
        }

        public static List<Media> GetRelatedMediaList(InitializationObject initObj, int mediaID, int mediaType, string picSize, int pageSize, int pageIndex, int groupID, ref long mediaCount)
        {
            TVMAccountType account = SiteMapManager.GetInstance.GetPageData(groupID, initObj.Platform).GetTVMAccountByAccountType(AccountType.Parent);
            

            List<Media> lstMedia = GetMediaList(initObj, account.TVMUser, account.TVMPass, mediaID, picSize, pageSize, pageIndex, groupID, LoaderType.Related, ref mediaCount, OrderBy.None);
            return lstMedia;
        }

        public static List<Media> GetRelatedMediaList(InitializationObject initObj, int mediaID, int mediaType, string picSize, int pageSize, int pageIndex, int groupID, int subGroupID)
        {
            TVMAccountType account = SiteMapManager.GetInstance.GetPageData(groupID, initObj.Platform).GetTVMAccountByAccountType(AccountType.Parent);
            return GetMediaList(initObj, account.TVMUser, account.TVMPass, mediaID, picSize, pageSize, pageIndex, groupID, LoaderType.Related, OrderBy.None);
        }

        public static List<Media> GetRelatedMediaList(InitializationObject initObj, string tvmPass, string tvmUser, int mediaID, int mediaType, string picSize, int pageSize, int pageIndex, int groupID, ref long mediaCount)
        {
            //TVMAccountType account = PageData.GetInstance.GetMember(groupID, initObj.Platform).GetTVMAccountByGroupID(subGroupID);
            return GetMediaList(initObj, tvmUser, tvmPass, mediaID, picSize, pageSize, pageIndex, groupID, LoaderType.Related, ref mediaCount, OrderBy.None);
        }

        public static List<Media> GetPeopleWhoWatchedList(InitializationObject initObj, long mediaID, int mediaType, string picSize, int pageSize, int pageIndex, int groupID)
        {
            //TVMAccountType account = SiteMapManager.GetInstance.GetPageData(groupID, initObj.Platform).GetTVMAccountByMediaType(mediaType);
            TVMAccountType account = SiteMapManager.GetInstance.GetPageData(groupID, initObj.Platform).GetTVMAccountByAccountType(AccountType.Parent);
            return GetMediaList(initObj, account.TVMUser, account.TVMPass, mediaID, picSize, pageSize, pageIndex, account.BaseGroupID, LoaderType.PeopleWhoWatched, OrderBy.None);
        }

        public static List<Media> GetUserSocialMedias(InitializationObject initObj, string picSize, int pageSize, int pageIndex, int groupID, TVPPro.SiteManager.TvinciPlatform.api.SocialAction socialAction, TVPPro.SiteManager.TvinciPlatform.api.SocialPlatform socialPlatform)
        {
            List<Media> retVal = new List<Media>();

            TVMAccountType account = SiteMapManager.GetInstance.GetPageData(groupID, initObj.Platform).GetTVMAccountByAccountType(AccountType.Parent);
            dsItemInfo mediaInfo = new APIUserSocialMediasLoader(account.TVMUser, account.TVMPass, picSize) 
            { 
                WithInfo = true, 
                SiteGuid = initObj.SiteGuid, 
                Platform = initObj.Platform, 
                SocialAction = socialAction, 
                SocialPlatform = socialPlatform, 
                GroupID = groupID, 
                PageSize = pageSize, 
                PageIndex = pageIndex, 
                Language = initObj.Locale.LocaleLanguage, 
                DomainID = initObj.DomainID
            }.Execute();

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
            TVMAccountType account = SiteMapManager.GetInstance.GetPageData(groupID, initObj.Platform).GetTVMAccountByAccountType(AccountType.Parent);
            return GetMediaList(initObj, account.TVMUser, account.TVMPass, 0, picSize, pageSize, pageIndex, groupID, LoaderType.LastWatched, OrderBy.None);
        }

        public static List<Media> GetLastWatchedMediasByPeriod(InitializationObject initObj, string picSize, int periodBefore, int groupID, ePeriod byPeriod)
        {
            List<Media> lstMedias = new List<Media>();
            TVMAccountType account = SiteMapManager.GetInstance.GetPageData(groupID, initObj.Platform).GetTVMAccountByAccountType(AccountType.Parent);
            List<Media> lstAllMedias = GetMediaList(initObj, account.TVMUser, account.TVMPass, 0, picSize, 100, 0, groupID, LoaderType.LastWatched, OrderBy.Added);

            lstMedias = (from media in lstAllMedias
                         where
                             (media.LastWatchDate.HasValue) &&
                             (DateTime.Now.AddDays((double)byPeriod * periodBefore * -1) - (DateTime)media.LastWatchDate).TotalDays >= 0 &&
                             (DateTime.Now.AddDays((double)byPeriod * periodBefore * -1) - (DateTime)media.LastWatchDate).TotalDays <= (periodBefore + 1) * (int)byPeriod
                         select media).ToList<Media>();

            return lstMedias;
        }

        public static List<Media> GetChannelMediaList(InitializationObject initObj, long channelID, string picSize, int pageSize, int pageIndex, int groupID, OrderBy orderBy)
        {
            List<Media> lstRet = new List<Media>();

            TVMAccountType account = SiteMapManager.GetInstance.GetPageData(groupID, initObj.Platform).GetTVMAccountByAccountType(AccountType.Parent);
            lstRet = GetMediaList(initObj, account.TVMUser, account.TVMPass, channelID, picSize, pageSize, pageIndex, groupID, LoaderType.Channel, orderBy);
            //if (lstRet == null || lstRet.Count == 0)
            //{
            //    account = SiteMapManager.GetInstance.GetPageData(groupID, initObj.Platform).GetTVMAccountByAccountType(AccountType.Regular);
            //    lstRet = GetMediaList(initObj, account.TVMUser, account.TVMPass, channelID, picSize, pageSize, pageIndex, groupID, LoaderType.Channel, orderBy);

            //    if (lstRet == null || lstRet.Count == 0)
            //    {
            //        account = SiteMapManager.GetInstance.GetPageData(groupID, initObj.Platform).GetTVMAccountByAccountType(AccountType.Fictivic);
            //        lstRet = GetMediaList(initObj, account.TVMUser, account.TVMPass, channelID, picSize, pageSize, pageIndex, groupID, LoaderType.Channel, orderBy);
            //    }
            //}

            return lstRet;
        }

        public static List<Media> GetChannelMultiFilter(InitializationObject initObj, long channelID, string picSize, int pageSize, int pageIndex, int groupID, OrderBy orderBy, eOrderDirection orderDir, List<TagMetaPair> tagsMetas, TVPApiModule.Objects.Enums.eCutWith cutWith)
        {
            // convert TagMetaPair to KeyValue 
            List<KeyValue> newTagsMetas = tagsMetas.Select(x => new KeyValue { m_sKey = x.Key, m_sValue = x.Value }).ToList();

            // convert enum to TVM enum
            CutWith newCutWith = (CutWith)cutWith + 1;

            List<Media> lstRet = new List<Media>();
            TVMAccountType account = SiteMapManager.GetInstance.GetPageData(groupID, initObj.Platform).GetTVMAccountByAccountType(AccountType.Parent);
           
            //lstRet = GetMediaList(initObj, account.TVMUser, account.TVMPass, channelID, picSize, pageSize, pageIndex, groupID, LoaderType.Channel, orderBy, null, newTagsMetas, newCutWith);


            var channelLoader = new APIChannelLoader(account.TVMUser, account.TVMPass, channelID, picSize)
            {
                PageIndex = pageIndex,
                PageSize = pageSize,
                OrderObj = new Tvinci.Data.Loaders.TvinciPlatform.Catalog.OrderObj()
                {
                    m_eOrderBy = CatalogHelper.GetCatalogOrderBy((TVPPro.SiteManager.Context.Enums.eOrderBy)Enum.Parse(typeof(TVPPro.SiteManager.Context.Enums.eOrderBy), orderBy.ToString())),
                    m_eOrderDir = CatalogHelper.GetCatalogOrderDirection((TVPPro.SiteManager.DataLoaders.SearchMediaLoader.eOrderDirection)Enum.Parse(typeof(TVPPro.SiteManager.DataLoaders.SearchMediaLoader.eOrderDirection), orderDir.ToString())),
                    
                },
                CutWith = newCutWith,
                TagsMetas = newTagsMetas,
                Platform = initObj.Platform,
                GroupID = groupID,
                SiteGuid = initObj.SiteGuid,
                DomainID = initObj.DomainID,
                DeviceUDID = initObj.UDID
            };

            dsItemInfo mediaInfo = channelLoader.Execute();

            long mediaCount;
            channelLoader.TryGetItemsCount(out mediaCount);


            if (mediaInfo.Item != null && mediaInfo.Item.Count > 0)
            {
                foreach (dsItemInfo.ItemRow row in mediaInfo.Item)
                {
                    lstRet.Add(new Media(row, initObj, groupID, false, mediaCount));
                }
            }
            return lstRet;

        }

        public static List<Media> GetChannelMediaList(InitializationObject initObj, long channelID, string picSize, int pageSize, int pageIndex, int groupID, ref long mediaCount)
        {
            TVMAccountType account = SiteMapManager.GetInstance.GetPageData(groupID, initObj.Platform).GetTVMAccountByAccountType(AccountType.Parent);
            return GetMediaList(initObj, account.TVMUser, account.TVMPass, channelID, picSize, pageSize, pageIndex, groupID, LoaderType.Channel, ref mediaCount, OrderBy.None);
        }

        public static List<Media> GetChannelMediaList(InitializationObject initObj, string user, string pass, long channelID, string picSize, int pageSize, int pageIndex, int groupID, ref long itemCount)
        {
            //TVMAccountType account = SiteMapManager.GetInstance.GetPageData[SiteMapManager.GetInstance.GetKey(groupID, initObj.Platform)].GetTVMAccountByGroupID(subGroupID);
            return GetMediaList(initObj, user, pass, channelID, picSize, pageSize, pageIndex, groupID, LoaderType.Channel, ref itemCount, OrderBy.None);
        }

        //Get all channel medias
        public static List<Media> GetMediaList(InitializationObject initObj, string user, string pass, 
            long ID, string picSize, int pageSize, int pageIndex, int groupID, LoaderType loaderType, ref long mediaCount, 
            OrderBy orderBy, int[] reqMediaTypes = null, List<KeyValue> tagsMetas = null, CutWith cutWith = CutWith.AND, string freeParam = null)
        {
            List<Media> retVal = new List<Media>();
            dsItemInfo mediaInfo;
            //bool isPaged;
            switch (loaderType)
            {
                case LoaderType.Channel:
                    APIChannelLoader channelLoader = new APIChannelLoader(user, pass, ID, picSize)
                    {
                        WithInfo = true,
                        GroupID = groupID,
                        Platform = initObj.Platform,
                        PageSize = pageSize,
                        PageIndex = pageIndex,
                        OrderBy = (TVPPro.SiteManager.Context.Enums.eOrderBy)Enum.Parse(typeof(TVPPro.SiteManager.Context.Enums.eOrderBy),orderBy.ToString()),
                        DeviceUDID = initObj.UDID,
                        GetFutureStartDate = ConfigManager.GetInstance().GetConfig(groupID, initObj.Platform).SiteConfiguration.Data.Features.FutureAssets.UseStartDate,
                        Language = initObj.Locale.LocaleLanguage,
                        TagsMetas = tagsMetas,
                        CutWith = cutWith,
                        SiteGuid = initObj.SiteGuid,
                        OrderObj = new Tvinci.Data.Loaders.TvinciPlatform.Catalog.OrderObj()
                        {
                            m_eOrderBy = CatalogHelper.GetCatalogOrderBy((TVPPro.SiteManager.Context.Enums.eOrderBy)Enum.Parse(typeof(TVPPro.SiteManager.Context.Enums.eOrderBy), orderBy.ToString())),
                            m_eOrderDir = Tvinci.Data.Loaders.TvinciPlatform.Catalog.OrderDir.ASC
                        },
                        DomainID = initObj.DomainID
                    };

                    mediaInfo = channelLoader.Execute();
                    channelLoader.TryGetItemsCount(out mediaCount);
                    //isPaged = true;
                    break;
                case LoaderType.Related:
                    APIRelatedMediaLoader relatedLoader = new APIRelatedMediaLoader(ID, user, pass)
                    {
                        GroupID = groupID,
                        Platform = initObj.Platform,
                        PicSize = picSize,
                        WithInfo = true,
                        PageSize = pageSize,
                        PageIndex = pageIndex,
                        IsPosterPic = false,
                        DeviceUDID = initObj.UDID,
                        MediaTypes = reqMediaTypes,
                        Language = initObj.Locale.LocaleLanguage,
                        SiteGuid = initObj.SiteGuid,
                        DomainID = initObj.DomainID
                    };
                    mediaInfo = relatedLoader.Execute();
                    relatedLoader.TryGetItemsCount(out mediaCount);
                    //isPaged = true;
                    break;                
                case LoaderType.PeopleWhoWatched:
                    mediaInfo = (new APIPeopleWhoWatchedLoader(user, pass, ID, picSize) { 
                        GroupID = groupID, 
                        Platform = initObj.Platform, 
                        WithInfo = true, 
                        IsPosterPic = false, 
                        Language = initObj.Locale.LocaleLanguage, 
                        SiteGuid = initObj.SiteGuid,
                        DomainID = initObj.DomainID
                    }).Execute();
                    //isPaged = true;
                    break;
                case LoaderType.LastWatched:
                    TVMAccountType account = SiteMapManager.GetInstance.GetPageData(groupID, initObj.Platform).GetTVMAccountByAccountType(AccountType.Parent);
                    APILastWatchedLoader lastWatchedLoader = new APILastWatchedLoader(account.TVMUser, account.TVMPass) { GroupID = groupID, Platform = initObj.Platform, WithInfo = true, SiteGuid = initObj.SiteGuid, PageSize = pageSize, PageIndex = pageIndex, PicSize = picSize, Language = initObj.Locale.LocaleLanguage };
                    mediaInfo = lastWatchedLoader.Execute();
                    lastWatchedLoader.TryGetItemsCount(out mediaCount);
                    //isPaged = true;
                    break;
                case LoaderType.Recommended:
                    mediaInfo = new TVPApiModule.DataLoaders.APIPersonalRecommendedLoader(user, pass) 
                    { 
                        GroupID = groupID, 
                        Platform = initObj.Platform, 
                        WithInfo = true, 
                        SiteGuid = initObj.SiteGuid, 
                        PageSize = pageSize, 
                        PageIndex = pageIndex, 
                        PicSize = picSize, 
                        MediaTypes = reqMediaTypes, 
                        Language = initObj.Locale.LocaleLanguage, 
                        DomainID = initObj.DomainID
                    }.Execute();
                    //isPaged = true;
                    break;
                default:
                    mediaInfo = (new APIChannelLoader(user, pass, ID, picSize) { WithInfo = true, GroupID = groupID, Platform = initObj.Platform, DeviceUDID = initObj.UDID, Language = initObj.Locale.LocaleLanguage, SiteGuid = initObj.SiteGuid }.Execute());
                    //isPaged = true;
                    break;
            }

            //if (mediaInfo.Item != null && mediaInfo.Item.Count > 0)
            //{
            //    int startIndex = (pageIndex) * pageSize;
            //    //Local server Paging
            //    IEnumerable<dsItemInfo.ItemRow> pagedDT;
            //    if (!isPaged)
            //    {
            //        pagedDT = PagingHelper.GetPagedData<dsItemInfo.ItemRow>(startIndex, pageSize, mediaInfo.Item);
            //    }
            //    else
            //    {
            //        pagedDT = mediaInfo.Item;
            //    }
            //    //Parse to WS return objects
            //    if (pagedDT != null)
            //    {
            //        foreach (dsItemInfo.ItemRow row in pagedDT)
            //        {
            //            retVal.Add(new Media(row, initObj, groupID, false, mediaCount));
            //        }
            //    }
            //}
            //return retVal;

            if (mediaInfo.Item != null && mediaInfo.Item.Count > 0)
            {
                foreach (dsItemInfo.ItemRow row in mediaInfo.Item)
                {
                    Media media = new Media(row, initObj, groupID, false, mediaCount);
                    retVal.Add(media);
                }
            }
            return retVal;

        }

        //Get all channel medias
        public static List<Media> GetMediaList(InitializationObject initObj, string user, string pass, long ID, string picSize, int pageSize, int pageIndex, int groupID, LoaderType loaderType, ref long mediaCount, 
            Tvinci.Data.Loaders.TvinciPlatform.Catalog.OrderObj orderObj, int[] reqMediaTypes = null, List<KeyValue> tagsMetas = null, CutWith cutWith = CutWith.AND)
        {
            List<Media> retVal = new List<Media>();
            dsItemInfo mediaInfo;
            bool isPaged = false;
            switch (loaderType)
            {
                case LoaderType.Channel:
                    APIChannelLoader channelLoader = new APIChannelLoader(user, pass, ID, picSize)
                    {
                        ChannelID = ID,
                        WithInfo = true,
                        GroupID = groupID,
                        Platform = initObj.Platform,
                        PageSize = pageSize,
                        PageIndex = pageIndex,
                        OrderObj = orderObj,
                        DeviceUDID = initObj.UDID,
                        GetFutureStartDate = ConfigManager.GetInstance().GetConfig(groupID, initObj.Platform).SiteConfiguration.Data.Features.FutureAssets.UseStartDate,
                        Language = initObj.Locale.LocaleLanguage,
                        TagsMetas = tagsMetas,
                        CutWith = cutWith,
                        SiteGuid = initObj.SiteGuid
                    };

                    mediaInfo = channelLoader.Execute();
                    channelLoader.TryGetItemsCount(out mediaCount);
                    isPaged = true;
                    break;
                case LoaderType.Related:
                    APIRelatedMediaLoader relatedLoader = new APIRelatedMediaLoader(ID, user, pass) { GroupID = groupID, Platform = initObj.Platform, PicSize = picSize, WithInfo = true, PageSize = pageSize, PageIndex = pageIndex, IsPosterPic = false, DeviceUDID = initObj.UDID, MediaTypes = reqMediaTypes, Language = initObj.Locale.LocaleLanguage, SiteGuid = initObj.SiteGuid };
                    mediaInfo = relatedLoader.Execute();
                    relatedLoader.TryGetItemsCount(out mediaCount);
                    isPaged = true;
                    break;
                case LoaderType.PeopleWhoWatched:
                    mediaInfo = (new APIPeopleWhoWatchedLoader(user, pass, ID, picSize) { GroupID = groupID, Platform = initObj.Platform, WithInfo = true, IsPosterPic = false, Language = initObj.Locale.LocaleLanguage, SiteGuid = initObj.SiteGuid }).Execute();
                    isPaged = false;
                    break;
                case LoaderType.LastWatched:
                    TVMAccountType account = SiteMapManager.GetInstance.GetPageData(groupID, initObj.Platform).GetTVMAccountByAccountType(AccountType.Parent);
                    mediaInfo = new APILastWatchedLoader(account.TVMUser, account.TVMPass) { GroupID = groupID, Platform = initObj.Platform, WithInfo = true, SiteGuid = initObj.SiteGuid, PageSize = pageSize, PageIndex = pageIndex, PicSize = picSize, Language = initObj.Locale.LocaleLanguage }.Execute();
                    break;
                case LoaderType.Recommended:
                    mediaInfo = new TVPApiModule.DataLoaders.APIPersonalRecommendedLoader(user, pass) { GroupID = groupID, Platform = initObj.Platform, WithInfo = true, SiteGuid = initObj.SiteGuid, PageSize = pageSize, PageIndex = pageIndex, PicSize = picSize, MediaTypes = reqMediaTypes, Language = initObj.Locale.LocaleLanguage }.Execute();
                    break;
                default:
                    mediaInfo = (new APIChannelLoader(user, pass, ID, picSize) { WithInfo = true, GroupID = groupID, Platform = initObj.Platform, DeviceUDID = initObj.UDID, Language = initObj.Locale.LocaleLanguage, SiteGuid = initObj.SiteGuid }.Execute());
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
                        retVal.Add(new Media(row, initObj, groupID, false, mediaCount));
                    }
                }
            }
            return retVal;
        }

        //Get all channel medias
        public static List<Media> GetMediaList(InitializationObject initObj, string user, string pass, long ID, string picSize, int pageSize, int pageIndex, int groupID, LoaderType loaderType, OrderBy orderBy)
        {
            long mediaCount = 0;
            return GetMediaList(initObj, user, pass, ID, picSize, pageSize, pageIndex, groupID, loaderType, ref mediaCount, orderBy);
        }

        public static List<Media> GetMediaList(InitializationObject initObj, string user, string pass, long ID, string picSize, int pageSize, int pageIndex, int groupID, LoaderType loaderType, OrderBy orderBy, int[] reqMediaTypes = null, List<KeyValue> tagsMetas = null, CutWith cutWith = CutWith.AND, string freeParam=null)
        {
            long mediaCount = 0;
            return GetMediaList(initObj, user, pass, ID, picSize, pageSize, pageIndex, groupID, loaderType, ref mediaCount, orderBy, reqMediaTypes, tagsMetas, cutWith, freeParam);
        }

        public static List<Media> GetMediaList(InitializationObject initObj, string user, string pass, long ID, string picSize, int pageSize, int pageIndex, int groupID, LoaderType loaderType, Tvinci.Data.Loaders.TvinciPlatform.Catalog.OrderObj orderObj, int[] reqMediaTypes = null, List<KeyValue> tagsMetas = null, CutWith cutWith = CutWith.AND)
        {
            long mediaCount = 0;
            return GetMediaList(initObj, user, pass, ID, picSize, pageSize, pageIndex, groupID, loaderType, ref mediaCount, orderObj, reqMediaTypes, tagsMetas, cutWith);
        }

        public static TVPApiModule.Objects.Responses.UnifiedSearchResponseWithRequestId GetMediaExternalRelatedList(InitializationObject initObj, string user, string pass, long ID, int pageSize, int pageIndex, 
                                                                                int groupID, int[] reqMediaTypes = null, string freeParam = null, List<string> with = null)
        {
            List<BaseObject> mediaInfo;

            APIExternalRelatedMediaLoader externalRelatedLoader = new APIExternalRelatedMediaLoader(ID, user, pass, freeParam)
            {
                GroupID = groupID,
                Platform = initObj.Platform,
                WithInfo = true,
                PageSize = pageSize,
                PageIndex = pageIndex,
                IsPosterPic = false,
                DeviceUDID = initObj.UDID,
                MediaTypes = reqMediaTypes,
                Language = initObj.Locale.LocaleLanguage,
                SiteGuid = initObj.SiteGuid,
                DomainID = initObj.DomainID,
                With = with
            };
            mediaInfo = externalRelatedLoader.Execute();

            bool shouldAddFiles = false;
            bool shouldAddImages = false;
            List<AssetStatsResult> mediaAssetsStats = null;
            Dictionary<string, AssetStatsResult> mediaAssetsStatsDic = new Dictionary<string,AssetStatsResult>();

            if (externalRelatedLoader.With != null)
            {
                if (externalRelatedLoader.With.Contains("stats")) // if stats are required - gets the stats from Catalog
                {
                    if (mediaInfo != null && mediaInfo.Count > 0)
                    {
                        mediaAssetsStats = new TVPPro.SiteManager.CatalogLoaders.AssetStatsLoader(groupID, SiteHelper.GetClientIP(), 0, 0, mediaInfo.Select(m => int.Parse(m.AssetId)).ToList(),
                            StatsType.MEDIA, DateTime.MinValue, DateTime.MaxValue).Execute() as List<AssetStatsResult>;

                        foreach (var stat in mediaAssetsStats)
                        {
                            if (!mediaAssetsStatsDic.ContainsKey(stat.m_nAssetID.ToString()))
                                mediaAssetsStatsDic.Add(stat.m_nAssetID.ToString(), stat);
                        }
                    }                    
                }
                if (externalRelatedLoader.With.Contains("files"))
                {
                    shouldAddFiles = true;
                }
                if (externalRelatedLoader.With.Contains("images")) 
                {
                    shouldAddImages = true;
                }
            }

            long mediaCount;

            externalRelatedLoader.TryGetItemsCount(out mediaCount);

            TVPApiModule.Objects.Responses.UnifiedSearchResponseWithRequestId ret = new TVPApiModule.Objects.Responses.UnifiedSearchResponseWithRequestId();

            try
            {
                List<MediaObj> mediaList = mediaInfo.Cast<MediaObj>().ToList();

                ret.TotalItems = mediaList.Count;
                ret.Assets = mediaList.Select(m => new AssetInfo(m,
                                                                mediaAssetsStatsDic.ContainsKey(m.AssetId) ? mediaAssetsStatsDic[m.AssetId] : null, 
                                                                shouldAddFiles)).ToList();

                ret.RequestId = externalRelatedLoader.RequestId;
                ret.Status = new TVPApiModule.Objects.Responses.Status();
                ret.Status.Code = externalRelatedLoader.Status.Code;
                ret.Status.Message = externalRelatedLoader.Status.Message;
                if (!shouldAddImages)
                    ret.Assets.ForEach(m => m.Images = null);

                return ret;
            }
            catch (Exception ex)
            {
                throw;
            }
            
            return ret;
        }

        public static TVPApiModule.Objects.Responses.UnifiedSearchResponseWithRequestId GetMediaExternalSearchList(InitializationObject initObj, string user, string pass, string query, int pageSize, int pageIndex, int groupID, OrderBy orderBy, int[] reqMediaTypes = null, List<string> with = null)
        {
            List<BaseObject> mediaInfo;

            APIExternalSearchMediaLoader externalRelatedLoader = new APIExternalSearchMediaLoader(query, user, pass)
            {
                GroupID = groupID,
                Platform = initObj.Platform,
                WithInfo = true,
                PageSize = pageSize,
                PageIndex = pageIndex,
                IsPosterPic = false,
                DeviceUDID = initObj.UDID,
                MediaTypes = reqMediaTypes,
                Language = initObj.Locale.LocaleLanguage,
                SiteGuid = initObj.SiteGuid,
                DomainID = initObj.DomainID,
                With = with
            };
            mediaInfo = externalRelatedLoader.Execute();

            bool shouldAddFiles = false;
            bool shouldAddImages = false;
            List<AssetStatsResult> mediaAssetsStats = null;
            Dictionary<string, AssetStatsResult> mediaAssetsStatsDic = new Dictionary<string,AssetStatsResult>();

            if (externalRelatedLoader.With != null)
            {
                if (externalRelatedLoader.With.Contains("stats")) // if stats are required - gets the stats from Catalog
                {
                    if (mediaInfo != null && mediaInfo.Count > 0)
                    {
                        mediaAssetsStats = new TVPPro.SiteManager.CatalogLoaders.AssetStatsLoader(groupID, SiteHelper.GetClientIP(), 0, 0, mediaInfo.Select(m => int.Parse(m.AssetId)).ToList(),
                            StatsType.MEDIA, DateTime.MinValue, DateTime.MaxValue).Execute() as List<AssetStatsResult>;

                        foreach (var stat in mediaAssetsStats)
                        {
                            if (!mediaAssetsStatsDic.ContainsKey(stat.m_nAssetID.ToString()))
                                mediaAssetsStatsDic.Add(stat.m_nAssetID.ToString(), stat);
                        }
                    }                    
                }
                if (externalRelatedLoader.With.Contains("files"))
                {
                    shouldAddFiles = true;
                }
                if (externalRelatedLoader.With.Contains("images")) 
                {
                    shouldAddImages = true;
                }
            }

            TVPApiModule.Objects.Responses.UnifiedSearchResponseWithRequestId ret = new TVPApiModule.Objects.Responses.UnifiedSearchResponseWithRequestId();

            try
            {
                List<MediaObj> mediaList = mediaInfo.Cast<MediaObj>().ToList();

                ret.TotalItems = mediaList.Count;
                ret.Assets = mediaList.Select(m => new AssetInfo(m,
                                                                mediaAssetsStatsDic.ContainsKey(m.AssetId) ? mediaAssetsStatsDic[m.AssetId] : null, 
                                                                shouldAddFiles)).ToList();

                ret.RequestId = externalRelatedLoader.RequestId;
                ret.Status = new TVPApiModule.Objects.Responses.Status();
                ret.Status.Code = externalRelatedLoader.Status.Code;
                ret.Status.Message = externalRelatedLoader.Status.Message;
                if (!shouldAddImages)
                    ret.Assets.ForEach(m => m.Images = null);

                return ret;
            }
            catch (Exception ex)
            {
                throw;
            }

            return ret;            
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
                        FavoritObject[] favoritesObj = new ApiUsersService(groupID, initObj.Platform).GetUserFavorites(initObj.SiteGuid, mediaType != 0 ? mediaType.ToString() : string.Empty, initObj.DomainID, string.Empty);//initObj.UDID);
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
                            mediaInfo = (new APIMultiMediaLoader(parentAccount.TVMUser, parentAccount.TVMPass, favoritesList, picSize, tempMediaType) { GroupID = groupID, Platform = initObj.Platform, PageSize = 20, Language = initObj.Locale.LocaleLanguage, SiteGuid = initObj.SiteGuid }).Execute();
                        }
                        //LogManager.Instance.Log(groupID, "Favorites", string.Format("Found {0} favorites", favoritesList != null ? favoritesList.Length.ToString() : "0")); 

                        break;
                    }
                case UserItemType.Rental:
                    {
                        PermittedMediaContainer[] MediaPermitedItems = new ApiConditionalAccessService(groupID, initObj.Platform).GetUserPermittedItems(guid);
                        mediaInfo = (new TVMRentalMultiMediaLoader(parentAccount.TVMUser, parentAccount.TVMPass, picSize, 1) { MediasIdCotainer = MediaPermitedItems }).Execute();
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
                                mediaInfo = new SearchMediaLoader(parentAccount.TVMUser, parentAccount.TVMPass) { dictMetas = BaseIdsDict, MetaValues = sb.ToString(), SearchTokenSignature = sb.ToString(), PageSize = 20, PictureSize = picSize, DeviceUDID = initObj.UDID, Language = initObj.Locale.LocaleLanguage }.Execute();
                            }
                        }

                        break;
                    }
                case UserItemType.All:
                    {
                        List<Media> favorites = null;
                        List<Media> packages = null;
                        List<Media> rentals = null;

                        try
                        {
                            favorites = GetUserItems(initObj, UserItemType.Favorite, mediaType, picSize, int.MaxValue, 0, groupID);
                        }
                        catch (Exception ex)
                        {

                        }

                        try
                        {
                            packages = GetUserItems(initObj, UserItemType.Package, mediaType, picSize, int.MaxValue, 0, groupID);
                        }
                        catch (Exception ex)
                        {

                        }

                        try
                        {
                            rentals = GetUserItems(initObj, UserItemType.Rental, mediaType, picSize, int.MaxValue, 0, groupID);
                        }
                        catch (Exception ex)
                        {

                        }

                        List<Media> finalList = new List<Media>();

                        if (favorites != null)
                        {
                            finalList.AddRange(favorites);
                        }

                        if (packages != null)
                        {
                            finalList.AddRange(packages);
                        }

                        if (rentals != null)
                        {
                            finalList.AddRange(rentals);
                        }

                        int startIndex = (pageIndex) * pageSize;

                        retVal = finalList.Skip(startIndex).Take(pageSize).ToList();

                        break;
                    }
                default:
                    {
                        break;
                    }
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
                    retVal.Add(new Media(row, initObj, groupID, false, mediaInfo.Item.Count));
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

        public static List<Media> GetMediasInPackage(InitializationObject initObj, long sBaseID, int mediaType, int iGroupID, string picSize, int pageSize, int pageIndex)
        {
            List<Media> retVal = new List<Media>();

            TVMAccountType parentAccount = SiteMapManager.GetInstance.GetPageData(iGroupID, initObj.Platform).GetTVMAccountByAccountType(AccountType.Parent);
            APITVMSubscriptionMediaLoader oSubscriptionMediaLoader = new APITVMSubscriptionMediaLoader(
                parentAccount.TVMUser, parentAccount.TVMPass, sBaseID)
                {
                    MediaType = mediaType,
                    WithInfo = true,
                    Platform = initObj.Platform,
                    GroupID = iGroupID,
                    PageIndex = pageIndex,
                    PageSize = pageSize,
                    PicSize = picSize,
                    BaseID = sBaseID,
                    Language = initObj.Locale.LocaleLanguage,
                    SiteGuid = initObj.SiteGuid,
                    DomainID = initObj.DomainID
                };

            dsItemInfo mediaInfo = oSubscriptionMediaLoader.Execute();

            if (mediaInfo.Item != null && mediaInfo.Item.Count > 0)
            {
                long lTotalItems;
                oSubscriptionMediaLoader.TryGetItemsCount(out lTotalItems);

                foreach (dsItemInfo.ItemRow row in mediaInfo.Item.Rows)
                {
                    retVal.Add(new Media(row, initObj, iGroupID, false, lTotalItems));
                }
            }

            return retVal;
        }

        public static List<Media> GetOrderedChannelMultiFilter(InitializationObject initObj, long channelID, string picSize, int pageSize, int pageIndex, int groupID,
            Tvinci.Data.Loaders.TvinciPlatform.Catalog.OrderObj orderObj, List<TagMetaPair> tagsMetas, TVPApiModule.Objects.Enums.eCutWith cutWith)
        {

            // convert TagMetaPair to KeyValue 
            List<KeyValue> newTagsMetas = tagsMetas.Select(x => new KeyValue { m_sKey = x.Key, m_sValue = x.Value }).ToList();

            // convert enum to TVM enum
            CutWith newCutWith = (CutWith)cutWith + 1;

            List<Media> lstRet = new List<Media>();
            TVMAccountType account = SiteMapManager.GetInstance.GetPageData(groupID, initObj.Platform).GetTVMAccountByAccountType(AccountType.Parent);
            lstRet = GetMediaList(initObj, account.TVMUser, account.TVMPass, channelID, picSize, pageSize, pageIndex, groupID, LoaderType.Channel, orderObj, null, newTagsMetas, newCutWith);

            return lstRet;
        
        }
    }
}
