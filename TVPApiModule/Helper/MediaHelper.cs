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

/// <summary>
/// Summary description for MediaHelper
/// </summary>
/// 

namespace TVPApi
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

        //// Get medias info (used for get media info too)
        //public static List<Media> GetMediasInfo(InitializationObject initObj, List<int> MediaIDs, string picSize, int groupID)
        //{
        //    List<Media> retVal = new TVPApiModule.CatalogLoaders.APIMediaLoader(MediaIDs, groupID, initObj.Platform, initObj.UDID, SiteHelper.GetClientIP(), picSize, initObj.Locale.LocaleLanguage)
        //    .Execute() as List<Media>;

        //    //if (withDynamic && retVal != null && retVal.Count > 0)
        //    //{
        //    //    foreach (var media in retVal)
        //    //        media.BuildDynamicObj(initObj, groupID);
        //    //}
        //    return retVal;
        //}

        //// removed
        ////public static bool IsFavoriteMedia(InitializationObject initObj, int groupID, int mediaID)
        ////{
        ////    bool retVal = false;
        ////    FavoritObject[] favoriteObj = new ApiUsersService(groupID, initObj.Platform).GetUserFavorites(initObj.SiteGuid, string.Empty, initObj.DomainID, string.Empty);
        ////    if (favoriteObj != null)
        ////    {
        ////        for (int i = 0; i < favoriteObj.Length; i++)
        ////        {
        ////            if (favoriteObj[i].m_sItemCode == mediaID.ToString())
        ////            {
        ////                retVal = true;
        ////                break;
        ////            }
        ////        }
        ////    }
        ////    return retVal;
        ////}

        //public static List<KeyValuePair<int, bool>> AreMediasFavorite(InitializationObject initObj, int groupID, List<int> mediaIDs)
        //{
        //    List<KeyValuePair<int, bool>> retVal = new List<KeyValuePair<int, bool>>();

        //    // get all user favorites
        //    FavoritObject[] favoriteObjects = new ApiUsersService(groupID, initObj.Platform).GetUserFavorites(initObj.SiteGuid, string.Empty, initObj.DomainID, string.Empty);

        //    if (favoriteObjects != null)
        //        retVal = mediaIDs.Select(y => new KeyValuePair<int, bool>(y, favoriteObjects.Where(x => x.m_sItemCode == y.ToString()).Count() > 0)).ToList();

        //    return retVal;
        //}

        //// removed
        ////public static List<Media> SearchMediaByTag(InitializationObject initObj, int mediaType, List<TVPApi.TagMetaPair> tagPairs, string picSize, int pageSize, int pageIndex, int groupID, int orderBy)
        ////{
        ////    List<Media> retVal = new List<Media>();
        ////    SiteMapManager.GetInstance.GetSiteMapInstance(groupID, initObj.Platform, initObj.Locale);
        ////    TVMAccountType account = SiteMapManager.GetInstance.GetPageData(groupID, initObj.Platform).GetTVMAccountByGroupID(groupID);

        ////    Dictionary<string, string> dictTags = tagPairs.ToDictionary((keyItem) => keyItem.Key, (valueItem) => valueItem.Value);

        ////    string sSigature = string.Join("|", dictTags.Select(pair => string.Format("{0}={1}", pair.Key, pair.Value)).ToArray());

        ////    // create a signature for search loader
        ////    //string sSigature = string.Format(@"{0}={1}|{2}|{3}", tagName, value, groupID, initObj.Platform);

        ////    APISearchLoader searchLoader = new APISearchLoader(account.TVMUser, account.TVMPass, dictTags)
        ////        {
        ////            MediaType = mediaType,
        ////            SearchTokenSignature = sSigature,
        ////            Platform = initObj.Platform,
        ////            GroupID = groupID,
        ////            WithInfo = true,
        ////            PageSize = pageSize,
        ////            PageIndex = pageIndex,
        ////            OrderBy = (OrderBy)orderBy,
        ////            PictureSize = picSize,
        ////            CutType = SearchMediaLoader.eCutType.And,
        ////            DeviceUDID = initObj.UDID,
        ////            Country = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).IpToCountry(TVPPro.SiteManager.Helper.SiteHelper.GetClientIP()),
        ////            Language = initObj.Locale.LocaleLanguage
        ////        };

        ////    dsItemInfo mediaInfo = searchLoader.Execute();
        ////    long mediaCount = 0;
        ////    searchLoader.TryGetItemsCount(out mediaCount);

        ////    if (mediaInfo.Item != null && mediaInfo.Item.Count > 0)
        ////    {
        ////        foreach (dsItemInfo.ItemRow row in mediaInfo.Item)
        ////        {
        ////            retVal.Add(new Media(row, initObj, groupID, false, mediaCount));
        ////        }
        ////    }

        ////    return retVal;
        ////}

        //// removed
        ////public static List<Media> SearchMediaByMetasTags(InitializationObject initObj, int mediaType, List<TagMetaPair> tagPairs, List<TagMetaPair> metaPairs, string picSize, int pageSize, int pageIndex, int groupID, int orderBy)
        ////{
        ////    List<Media> retVal = new List<Media>();
        ////    SiteMapManager.GetInstance.GetSiteMapInstance(groupID, initObj.Platform, initObj.Locale);
        ////    TVMAccountType account = SiteMapManager.GetInstance.GetPageData(groupID, initObj.Platform).GetTVMAccountByGroupID(groupID);

        ////    Dictionary<string, string> dictTags = tagPairs.ToDictionary((keyItem) => keyItem.Key, (valueItem) => valueItem.Value);
        ////    Dictionary<string, string> dictMetas = metaPairs.ToDictionary((keyItem) => keyItem.Key, (valueItem) => valueItem.Value);

        ////    string sSigature = string.Format("{0}|{1}", string.Join("|", dictTags.Select(pair => string.Format("{0}={1}", pair.Key, pair.Value)).ToArray()), string.Join("|", dictMetas.Select(pair => string.Format("{0}={1}", pair.Key, pair.Value)).ToArray()));

        ////    // create a signature for search loader
        ////    //string sSigature = string.Format(@"{0}={1}|{2}|{3}", tagName, value, groupID, initObj.Platform);

        ////    APISearchLoader searchLoader = new APISearchLoader(account.TVMUser, account.TVMPass, dictTags) { dictMetas = dictMetas, MediaType = mediaType, SearchTokenSignature = sSigature, Platform = initObj.Platform, GroupID = groupID, WithInfo = true, PageSize = pageSize, PageIndex = pageIndex, OrderBy = (OrderBy)orderBy, PictureSize = picSize, CutType = SearchMediaLoader.eCutType.And, DeviceUDID = initObj.UDID, Country = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).IpToCountry(TVPPro.SiteManager.Helper.SiteHelper.GetClientIP()), Language = initObj.Locale.LocaleLanguage };
        ////    dsItemInfo mediaInfo = searchLoader.Execute();
        ////    long mediaCount = 0;
        ////    searchLoader.TryGetItemsCount(out mediaCount);

        ////    if (mediaInfo.Item != null && mediaInfo.Item.Count > 0)
        ////    {
        ////        foreach (dsItemInfo.ItemRow row in mediaInfo.Item)
        ////        {
        ////            retVal.Add(new Media(row, initObj, groupID, false, mediaCount));
        ////        }
        ////    }

        ////    return retVal;
        ////}

        //// removed!
        ////public static List<Media> SearchMediaByMetasTagsExact(InitializationObject initObj, int mediaType, List<TagMetaPair> tagPairs, List<TagMetaPair> metaPairs, string picSize, int pageSize, int pageIndex, int groupID, int orderBy)
        ////{
        ////    List<Media> retVal = new List<Media>();
        ////    SiteMapManager.GetInstance.GetSiteMapInstance(groupID, initObj.Platform, initObj.Locale);
        ////    TVMAccountType account = SiteMapManager.GetInstance.GetPageData(groupID, initObj.Platform).GetTVMAccountByGroupID(groupID);

        ////    Dictionary<string, string> dictTags = tagPairs.ToDictionary((keyItem) => keyItem.Key, (valueItem) => valueItem.Value);
        ////    Dictionary<string, string> dictMetas = metaPairs.ToDictionary((keyItem) => keyItem.Key, (valueItem) => valueItem.Value);

        ////    string sSigature = string.Format("{0}|{1}", string.Join("|", dictTags.Select(pair => string.Format("{0}={1}", pair.Key, pair.Value)).ToArray()), string.Join("|", dictMetas.Select(pair => string.Format("{0}={1}", pair.Key, pair.Value)).ToArray()));

        ////    // create a signature for search loader
        ////    //string sSigature = string.Format(@"{0}={1}|{2}|{3}", tagName, value, groupID, initObj.Platform);

        ////    APISearchLoader searchLoader = new APISearchLoader(account.TVMUser, account.TVMPass, dictTags) { ExactSearch = true, dictMetas = dictMetas, MediaType = mediaType, SearchTokenSignature = sSigature, Platform = initObj.Platform, GroupID = groupID, WithInfo = true, PageSize = pageSize, PageIndex = pageIndex, OrderBy = (OrderBy)orderBy, PictureSize = picSize, CutType = SearchMediaLoader.eCutType.And, DeviceUDID = initObj.UDID, Language = initObj.Locale.LocaleLanguage };
        ////    dsItemInfo mediaInfo = searchLoader.Execute();
        ////    long mediaCount = 0;
        ////    searchLoader.TryGetItemsCount(out mediaCount);

        ////    if (mediaInfo.Item != null && mediaInfo.Item.Count > 0)
        ////    {
        ////        foreach (dsItemInfo.ItemRow row in mediaInfo.Item)
        ////        {
        ////            retVal.Add(new Media(row, initObj, groupID, false, mediaCount));
        ////        }
        ////    }
        ////    return retVal;
        ////}

        //public static List<Media> SearchMediaByAndOrList(InitializationObject initObj, int mediaType, List<KeyValue> orList, List<KeyValue> andList, string picSize, int pageSize, int pageIndex, int groupID, Tvinci.Data.Loaders.TvinciPlatform.Catalog.OrderBy orderBy, Tvinci.Data.Loaders.TvinciPlatform.Catalog.OrderDir orderDir, string orderMetaName, bool exact)
        //{
        //   return new APISearchMediaLoader(groupID, initObj.Platform, initObj.UDID, TVPPro.SiteManager.Helper.SiteHelper.GetClientIP(), initObj.Locale.LocaleLanguage, pageSize, pageIndex, picSize, exact, orList, andList, new List<int>() { mediaType })
        //    {
        //        OrderBy = orderBy,
        //        OrderDir = orderDir,
        //        OrderMetaMame = orderMetaName
        //    }.Execute() as List<Media>;
        //}

        ////Call search protocol - removed!
        ////public static List<Media> SearchMediaByTag(InitializationObject initObj, int mediaType, string tagName, string value, string picSize, int pageSize, int pageIndex, int groupID, int orderBy)
        ////{
        ////    List<Media> retVal = new List<Media>();
        ////    SiteMapManager.GetInstance.GetSiteMapInstance(groupID, initObj.Platform, initObj.Locale);
        ////    TVMAccountType account = SiteMapManager.GetInstance.GetPageData(groupID, initObj.Platform).GetTVMAccountByGroupID(groupID);

        ////    Dictionary<string, string> dictTags = new Dictionary<string, string>();
        ////    dictTags.Add(tagName, value);

        ////    // create a signature for search loader
        ////    string sSigature = string.Format(@"{0}={1}|{2}|{3}", tagName, value, groupID, initObj.Platform);

        ////    APISearchLoader searchLoader = new APISearchLoader(account.TVMUser, account.TVMPass, dictTags) { MediaType = mediaType, SearchTokenSignature = sSigature, Platform = initObj.Platform, GroupID = groupID, WithInfo = true, PageSize = pageSize, PageIndex = pageIndex, OrderBy = (OrderBy)orderBy, PictureSize = picSize, DeviceUDID = initObj.UDID, Country = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).IpToCountry(TVPPro.SiteManager.Helper.SiteHelper.GetClientIP()), Language = initObj.Locale.LocaleLanguage };
        ////    dsItemInfo mediaInfo = searchLoader.Execute();
        ////    long mediaCount = 0;
        ////    searchLoader.TryGetItemsCount(out mediaCount);

        ////    if (mediaInfo.Item != null && mediaInfo.Item.Count > 0)
        ////    {
        ////        foreach (dsItemInfo.ItemRow row in mediaInfo.Item)
        ////        {
        ////            retVal.Add(new Media(row, initObj, groupID, false, mediaCount));
        ////        }
        ////    }

        ////    return retVal;
        ////}

        //// removed
        ////public static List<Media> SearchMediaBySubID(InitializationObject initObj, string sSubID, string picSize, int groupID, int orderBy)
        ////{
        ////    List<Media> retVal = new List<Media>();
        ////    SiteMapManager.GetInstance.GetSiteMapInstance(groupID, initObj.Platform, initObj.Locale);
        ////    TVMAccountType account = SiteMapManager.GetInstance.GetPageData(groupID, initObj.Platform).GetTVMAccountByAccountType(AccountType.Fictivic);
        ////    Dictionary<string, string> dictMetas = new Dictionary<string, string>();

        ////    string[] arrValues = sSubID.Split(';');
        ////    dictMetas.Add("Base ID", sSubID);

        ////    //Remote paging
        ////    APISearchLoader searchLoader = new APISearchLoader(account.TVMUser, account.TVMPass) { SearchTokenSignature = sSubID, GroupID = groupID, Platform = initObj.Platform, dictMetas = dictMetas, WithInfo = true, PageSize = arrValues.Length, PictureSize = picSize, PageIndex = 0, OrderBy = (OrderBy)orderBy, MetaValues = sSubID, UseFinalEndDate = "true", DeviceUDID = initObj.UDID, Country = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).IpToCountry(TVPPro.SiteManager.Helper.SiteHelper.GetClientIP()), Language = initObj.Locale.LocaleLanguage };
        ////    dsItemInfo mediaInfo = searchLoader.Execute();
        ////    long mediaCount = 0;
        ////    searchLoader.TryGetItemsCount(out mediaCount);

        ////    if (mediaInfo.Item != null && mediaInfo.Item.Count > 0)
        ////    {
        ////        foreach (dsItemInfo.ItemRow row in mediaInfo.Item)
        ////        {
        ////            retVal.Add(new Media(row, initObj, groupID, false, mediaCount));
        ////        }
        ////    }

        ////    return retVal;
        ////}

        ////Call search protocol - removed!
        ////public static List<Media> SearchMediaByMeta(InitializationObject initObj, int mediaType, string metaName, string value, string picSize, int pageSize, int pageIndex, int groupID, int orderBy)
        ////{
        ////    List<Media> retVal = new List<Media>();
        ////    SiteMapManager.GetInstance.GetSiteMapInstance(groupID, initObj.Platform, initObj.Locale);
        ////    TVMAccountType account = SiteMapManager.GetInstance.GetPageData(groupID, initObj.Platform).GetTVMAccountByGroupID(groupID);

        ////    Dictionary<string, string> dictMetas = new Dictionary<string, string>();

        ////    string[] arrValues = value.Split(';');
        ////    foreach (string sValue in arrValues)
        ////    {
        ////        dictMetas.Add(metaName, sValue);
        ////    }
        ////    //Remote paging
        ////    APISearchLoader searchLoader = new APISearchLoader(account.TVMUser, account.TVMPass) { MediaType = mediaType, SearchTokenSignature = value, GroupID = groupID, Platform = initObj.Platform, dictMetas = dictMetas, WithInfo = true, PageSize = pageSize, PictureSize = picSize, PageIndex = pageIndex, OrderBy = (OrderBy)orderBy, MetaValues = value, DeviceUDID = initObj.UDID, Country = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).IpToCountry(TVPPro.SiteManager.Helper.SiteHelper.GetClientIP()), Language = initObj.Locale.LocaleLanguage };
        ////    dsItemInfo mediaInfo = searchLoader.Execute();
        ////    long mediaCount = 0;
        ////    searchLoader.TryGetItemsCount(out mediaCount);

        ////    if (mediaInfo.Item != null && mediaInfo.Item.Count > 0)
        ////    {
        ////        foreach (dsItemInfo.ItemRow row in mediaInfo.Item)
        ////        {
        ////            retVal.Add(new Media(row, initObj, groupID, false, mediaCount));
        ////        }
        ////    }

        ////    return retVal;
        ////}

        ////Call search protocol - removed
        ////public static List<Media> SearchMediaByMeta(InitializationObject initObj, int mediaType, string metaName, string value, string picSize, int pageSize, int pageIndex, int groupID, int orderBy, ref long mediaCount)
        ////{
        ////    List<Media> retVal = new List<Media>();
        ////    SiteMapManager.GetInstance.GetSiteMapInstance(groupID, initObj.Platform, initObj.Locale);
        ////    TVMAccountType account = SiteMapManager.GetInstance.GetPageData(groupID, initObj.Platform).GetTVMAccountByAccountType(AccountType.Regular);
        ////    Dictionary<string, string> dictMetas = new Dictionary<string, string>();
        ////    dictMetas.Add(metaName, value);

        ////    //Remote paging
        ////    APISearchLoader searchLoader = new APISearchLoader(account.TVMUser, account.TVMPass) { GroupID = groupID, Platform = initObj.Platform, dictMetas = dictMetas, WithInfo = true, PageSize = pageSize, PictureSize = picSize, PageIndex = pageIndex, OrderBy = (OrderBy)orderBy, IsPosterPic = false, MetaValues = value, DeviceUDID = initObj.UDID, Country = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).IpToCountry(TVPPro.SiteManager.Helper.SiteHelper.GetClientIP()), Language = initObj.Locale.LocaleLanguage };
        ////    dsItemInfo mediaInfo = searchLoader.Execute();
        ////    searchLoader.TryGetItemsCount(out mediaCount);

        ////    if (mediaInfo.Item != null && mediaInfo.Item.Count > 0)
        ////    {
        ////        foreach (dsItemInfo.ItemRow row in mediaInfo.Item)
        ////        {
        ////            retVal.Add(new Media(row, initObj, groupID, false, mediaInfo.Item.Count));
        ////        }
        ////    }

        ////    return retVal;
        ////}

        ////Call search protocol - removed
        ////public static List<Media> SearchMedia(InitializationObject initObj, int mediaType, string text, string picSize, int pageSize, int pageIndex, int groupID, int orderBy)
        ////{
        ////    List<Media> retVal = new List<Media>();
        ////    SiteMapManager.GetInstance.GetSiteMapInstance(groupID, initObj.Platform, initObj.Locale);
        ////    TVMAccountType account;

        ////    account = SiteMapManager.GetInstance.GetPageData(groupID, initObj.Platform).GetTVMAccountByGroupID(groupID);

        ////    //Remote paging
        ////    APISearchLoader searchLoader = new APISearchLoader(account.TVMUser, account.TVMPass)
        ////    {
        ////        MediaType = mediaType,
        ////        Name = string.IsNullOrEmpty(text) ? null : text,
        ////        PictureSize = picSize,
        ////        GroupID = groupID,
        ////        Platform = initObj.Platform,
        ////        WithInfo = true,
        ////        PageSize = pageSize,
        ////        PageIndex = pageIndex,
        ////        OrderBy = (TVPApi.OrderBy)orderBy,
        ////        DeviceUDID = initObj.UDID,
        ////        Country = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).IpToCountry(TVPPro.SiteManager.Helper.SiteHelper.GetClientIP()),
        ////        Language = initObj.Locale.LocaleLanguage
        ////    };

        ////    dsItemInfo mediaInfo = searchLoader.Execute();
        ////    long mediaCount = 0;
        ////    searchLoader.TryGetItemsCount(out mediaCount);

        ////    if (mediaInfo.Item != null && mediaInfo.Item.Count > 0)
        ////    {
        ////        foreach (dsItemInfo.ItemRow row in mediaInfo.Item)
        ////        {
        ////            retVal.Add(new Media(row, initObj, groupID, false, mediaCount));
        ////        }
        ////    }

        ////    return retVal;
        ////}

        ////Call search protocols with multi types - removed
        ////public static List<Media> SearchMedia(InitializationObject initObj, int[] mediaType, string text, string picSize, int pageSize, int pageIndex, int groupID, TVPApi.OrderBy orderBy)
        ////{
        ////    List<Media> retVal = new List<Media>();

        ////    long iTotal = 0;
        ////    List<Media> tmpVal = new List<Media>();
        ////    foreach (int type in mediaType)
        ////    {
        ////        List<Media> lst = SearchMedia(initObj, type, text, picSize, 100, 0, groupID, (int)orderBy);
        ////        tmpVal.AddRange(lst);
        ////        iTotal += lst.Count;
        ////    }

        ////    switch (orderBy)
        ////    {
        ////        case (TVPApi.OrderBy.Added):
        ////            tmpVal = tmpVal.OrderByDescending(m => m.CreationDate).ToList();
        ////            //copyObject.Item.DefaultView.Sort = "CreationDate desc";
        ////            break;
        ////        case (TVPApi.OrderBy.Rating):
        ////            tmpVal = tmpVal.OrderByDescending(m => m.Rating).ToList();
        ////            //copyObject.Item.DefaultView.Sort = "Rate desc";
        ////            break;
        ////        case (TVPApi.OrderBy.Views):
        ////            tmpVal = tmpVal.OrderByDescending(m => m.ViewCounter).ToList();
        ////            //copyObject.Item.DefaultView.Sort = "ViewCounter desc";
        ////            break;
        ////        default:
        ////            tmpVal = tmpVal.OrderBy(m => m.MediaName).ToList();
        ////            //copyObject.Item.DefaultView.Sort = "Title asc";
        ////            break;
        ////    }

        ////    for (int i = 0; i < tmpVal.Count; i++)
        ////    {
        ////        if (i >= pageIndex * pageSize && i < (pageIndex + 1) * pageSize)
        ////        {
        ////            retVal.Add(tmpVal[i]);
        ////        }
        ////    }

        ////    iTotal = (iTotal > mediaType.Length * 50) ? mediaType.Length * 50 : iTotal;
        ////    retVal.ForEach(i => i.TotalItems = iTotal);

        ////    return retVal;
        ////}

        ////Call search protocol - removed
        ////public static List<Media> SearchMedia(InitializationObject initObj, int mediaType, string text, string picSize, int pageSize, int pageIndex, int groupID, int orderBy, ref long mediaCount)
        ////{
        ////    List<Media> retVal = new List<Media>();
        ////    SiteMapManager.GetInstance.GetSiteMapInstance(groupID, initObj.Platform, initObj.Locale);
        ////    TVMAccountType account;
        ////    if (mediaType > 0)
        ////    {
        ////        account = SiteMapManager.GetInstance.GetPageData(groupID, initObj.Platform).GetTVMAccountByMediaType(mediaType);
        ////    }
        ////    else
        ////    {
        ////        account = SiteMapManager.GetInstance.GetPageData(groupID, initObj.Platform).GetTVMAccountByAccountType(AccountType.Regular);
        ////    }

        ////    //Remote paging
        ////    APISearchLoader searchLoader = new APISearchLoader(account.TVMUser, account.TVMPass) { Name = string.IsNullOrEmpty(text) ? null : text, GroupID = groupID, Platform = initObj.Platform, WithInfo = true, PageSize = pageSize, PageIndex = pageIndex, OrderBy = (TVPApi.OrderBy)orderBy, PictureSize = picSize, DeviceUDID = initObj.UDID, Country = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).IpToCountry(TVPPro.SiteManager.Helper.SiteHelper.GetClientIP()), Language = initObj.Locale.LocaleLanguage };
        ////    dsItemInfo mediaInfo = searchLoader.Execute();
        ////    searchLoader.TryGetItemsCount(out mediaCount);

        ////    if (mediaInfo.Item != null && mediaInfo.Item.Count > 0)
        ////    {
        ////        foreach (dsItemInfo.ItemRow row in mediaInfo.Item)
        ////        {
        ////            retVal.Add(new Media(row, initObj, groupID, false));
        ////        }
        ////    }

        ////    return retVal;
        ////}

        //public static List<string> GetAutoCompleteList(int groupID, PlatformType platform, int[] iMediaTypes, string prefix, string lang, int pageIdx, int pageSize)
        //{
        //    string[] arrMetaNames = ConfigManager.GetInstance().GetConfig(groupID, platform).MediaConfiguration.Data.TVM.AutoCompleteValues.Metadata.ToString().Split(new Char[] { ';' });
        //    string[] arrTagNames = ConfigManager.GetInstance().GetConfig(groupID, platform).MediaConfiguration.Data.TVM.AutoCompleteValues.Tags.ToString().Split(new Char[] { ';' });
        //    List<String> lstResponse = new List<String>();

        //    return new ApiApiService(groupID, platform).GetAutoCompleteList(iMediaTypes, arrMetaNames, arrTagNames, prefix, lang, pageIdx, pageSize).ToList();
        //}

        ////Call search protocol - removed
        ////public static List<Media> SearchMedia(InitializationObject initObj, int mediaType, string text, string picSize, int pageSize, int pageIndex, int groupID, int subGroupID, int orderBy)
        ////{
        ////    List<Media> retVal = new List<Media>();
        ////    SiteMapManager.GetInstance.GetSiteMapInstance(groupID, initObj.Platform, initObj.Locale);
        ////    TVMAccountType account;
        ////    if (mediaType > 0)
        ////    {
        ////        account = SiteMapManager.GetInstance.GetPageData(groupID, initObj.Platform).GetTVMAccountByMediaType(mediaType);
        ////    }
        ////    else
        ////    {
        ////        account = SiteMapManager.GetInstance.GetPageData(groupID, initObj.Platform).GetTVMAccountByAccountType(AccountType.Regular);
        ////    }

        ////    //Remote paging
        ////    APISearchLoader searchLoader = new APISearchLoader(account.TVMUser, account.TVMPass) { Name = string.IsNullOrEmpty(text) ? null : text, PictureSize = picSize, GroupID = groupID, Platform = initObj.Platform, WithInfo = true, PageSize = pageSize, PageIndex = pageIndex, OrderBy = (TVPApi.OrderBy)orderBy, DeviceUDID = initObj.UDID, Country = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).IpToCountry(TVPPro.SiteManager.Helper.SiteHelper.GetClientIP()), Language = initObj.Locale.LocaleLanguage };
        ////    dsItemInfo mediaInfo = searchLoader.Execute();
        ////    long mediaCount = 0;
        ////    searchLoader.TryGetItemsCount(out mediaCount);

        ////    if (mediaInfo.Item != null && mediaInfo.Item.Count > 0)
        ////    {
        ////        foreach (dsItemInfo.ItemRow row in mediaInfo.Item)
        ////        {
        ////            retVal.Add(new Media(row, initObj, groupID, false, mediaCount));
        ////        }
        ////    }

        ////    return retVal;
        ////}

        //public static List<Media> GetRecommendedMediasList(InitializationObject initObj, string picSize, int pageSize, int pageIndex, int groupID, int[] reqMediaTypes = null)
        //{
        //    return new TVPApiModule.CatalogLoaders.APIPersonalRecommendedLoader(initObj.SiteGuid, groupID, initObj.Platform, initObj.UDID, SiteHelper.GetClientIP(), initObj.Locale.LocaleLanguage, pageSize, pageIndex, picSize).Execute() as List<Media>;
        //}

        //public static List<Media> GetRelatedMediaList(InitializationObject initObj, int mediaID, string picSize, int pageSize, int pageIndex, int groupID, List<int> reqMediaTypes = null)
        //{
        //    return new TVPApiModule.CatalogLoaders.APIRelatedMediaLoader(mediaID, reqMediaTypes, groupID, initObj.Platform, initObj.UDID, SiteHelper.GetClientIP(), pageSize, pageIndex, picSize, initObj.Locale.LocaleLanguage).Execute() as List<Media>;
        //}


        //// Not used!
        ////public static List<Media> GetRelatedMediaList(InitializationObject initObj, int mediaID, int mediaType, string picSize, int pageSize, int pageIndex, int groupID, ref long mediaCount)
        ////{
        ////    TVMAccountType account;

        ////    if (mediaType != 0)
        ////    {
        ////        account = SiteMapManager.GetInstance.GetPageData(groupID, initObj.Platform).GetTVMAccountByMediaType(mediaType);
        ////    }
        ////    else
        ////    {
        ////        account = SiteMapManager.GetInstance.GetPageData(groupID, initObj.Platform).GetTVMAccountByAccountType(AccountType.Regular);
        ////    }

        ////    List<Media> lstMedia = GetMediaList(initObj, account.TVMUser, account.TVMPass, mediaID, picSize, pageSize, pageIndex, groupID, LoaderType.Related, ref mediaCount, OrderBy.None);
        ////    return lstMedia;
        ////}

        //// Not used!
        ////public static List<Media> GetRelatedMediaList(InitializationObject initObj, int mediaID, int mediaType, string picSize, int pageSize, int pageIndex, int groupID, int subGroupID)
        ////{
        ////    TVMAccountType account = SiteMapManager.GetInstance.GetPageData(groupID, initObj.Platform).GetTVMAccountByAccountType(AccountType.Regular);
        ////    return GetMediaList(initObj, account.TVMUser, account.TVMPass, mediaID, picSize, pageSize, pageIndex, groupID, LoaderType.Related, OrderBy.None);
        ////}

        //// Not used!
        ////public static List<Media> GetRelatedMediaList(InitializationObject initObj, string tvmPass, string tvmUser, int mediaID, int mediaType, string picSize, int pageSize, int pageIndex, int groupID, ref long mediaCount)
        ////{
        ////    //TVMAccountType account = PageData.GetInstance.GetMember(groupID, initObj.Platform).GetTVMAccountByGroupID(subGroupID);
        ////    return GetMediaList(initObj, tvmUser, tvmPass, mediaID, picSize, pageSize, pageIndex, groupID, LoaderType.Related, ref mediaCount, OrderBy.None);
        ////}

        //public static List<Media> GetPeopleWhoWatchedList(InitializationObject initObj, int mediaID, string picSize, int pageSize, int pageIndex, int groupID)
        //{
        //    return new TVPApiModule.CatalogLoaders.APIPeopleWhoWatchedLoader(mediaID, 0, groupID, initObj.Platform, initObj.UDID ,SiteHelper.GetClientIP(), pageSize, pageIndex, picSize, initObj.Locale.LocaleLanguage).Execute() as List<Media>;
        //}

        //public static List<Media> GetUserSocialMedias(InitializationObject initObj, string picSize, int pageSize, int pageIndex, int groupID, int socialAction, int socialPlatform)
        //{
        //    return new TVPApiModule.CatalogLoaders.APIUserSocialMediaLoader(initObj.SiteGuid, socialAction, socialPlatform, groupID, initObj.Platform, initObj.UDID, SiteHelper.GetClientIP(), initObj.Locale.LocaleLanguage, pageSize, pageIndex, picSize).Execute() as List<Media>;
        //}

        //public static List<Media> GetLastWatchedMedias(InitializationObject initObj, string picSize, int pageSize, int pageIndex, int groupID)
        //{
        //    return new TVPApiModule.CatalogLoaders.APIPersonalLastWatchedLoader(initObj.SiteGuid, groupID, initObj.Platform, initObj.UDID, SiteHelper.GetClientIP(), initObj.Locale.LocaleLanguage, pageSize, pageIndex, picSize).Execute() as List<Media>;  
        //}

        //public static List<Media> GetLastWatchedMediasByPeriod(InitializationObject initObj, string picSize, int periodBefore, int groupID, ePeriod byPeriod)
        //{
        //    List<Media> lstMedias = new List<Media>();
        //    List<Media> lstAllMedias = new TVPApiModule.CatalogLoaders.APIPersonalLastWatchedLoader(initObj.SiteGuid, groupID, initObj.Platform, initObj.UDID, SiteHelper.GetClientIP(), initObj.Locale.LocaleLanguage, 100, 0, picSize).Execute() as List<Media>;  

        //    lstMedias = (from media in lstAllMedias
        //                 where
        //                     (DateTime.Now.AddDays((double)byPeriod * periodBefore * -1) - (DateTime)media.LastWatchDate).TotalDays >= 0 &&
        //                     (DateTime.Now.AddDays((double)byPeriod * periodBefore * -1) - (DateTime)media.LastWatchDate).TotalDays <= (periodBefore + 1) * (int)byPeriod
        //                 select media).ToList<Media>();

        //    return lstMedias;
        //}

        //// removed!
        ////public static List<Media> GetChannelMediaList(InitializationObject initObj, int channelID, string picSize, int pageSize, int pageIndex, int groupID, OrderBy orderBy)
        ////{
        ////    List<Media> retVal = new List<Media>();

        ////    retVal = new APIChannelMediaLoader(channelID, groupID, initObj.Platform.ToString(), initObj.UDID, SiteHelper.GetClientIP(), pageSize, pageIndex, picSize, initObj.Locale.LocaleLanguage)
        ////    {
        ////        UseStartDate = bool.Parse(ConfigManager.GetInstance().GetConfig(groupID, initObj.Platform).SiteConfiguration.Data.Features.FutureAssets.UseStartDate)
        ////    }.Execute() as List<Media>;

        ////    // What to do with that???
        ////    //OrderBy = (TVPPro.SiteManager.Context.Enums.eOrderBy)Enum.Parse(typeof(TVPPro.SiteManager.Context.Enums.eOrderBy), orderBy.ToString()),

        ////    return retVal;
        ////}

        //public static List<Media> GetChannelMultiFilter(PlatformType platform, string udid, string language, int channelID, string picSize, int pageSize, int pageIndex, int groupID, OrderBy orderBy, List<KeyValue> tagsMetas, CutWith cutWith)
        //{
        //    List<Media> retVal = new List<Media>();

        //    retVal = new APIChannelMediaLoader(channelID, groupID, platform, udid, SiteHelper.GetClientIP(), pageSize, pageIndex, picSize, language, tagsMetas, cutWith)
        //    {
        //        UseStartDate = bool.Parse(ConfigManager.GetInstance().GetConfig(groupID, platform).SiteConfiguration.Data.Features.FutureAssets.UseStartDate)
        //    }.Execute() as List<Media>;

        //    // What to do with that???
        //    //OrderBy = (TVPPro.SiteManager.Context.Enums.eOrderBy)Enum.Parse(typeof(TVPPro.SiteManager.Context.Enums.eOrderBy), orderBy.ToString()),

        //    return retVal;
        //}

        //// removed!
        ////public static List<Media> GetChannelMediaList(InitializationObject initObj, int channelID, string picSize, int pageSize, int pageIndex, int groupID, ref long mediaCount)
        ////{
        ////    List<Media> retVal = new List<Media>();

        ////    APIChannelMediaLoader loader = new APIChannelMediaLoader(channelID, groupID, initObj.Platform.ToString(), initObj.UDID, SiteHelper.GetClientIP(), pageSize, pageIndex, picSize, initObj.Locale.LocaleLanguage)
        ////    {
        ////        DeviceId = initObj.UDID,
        ////        UseStartDate = bool.Parse(ConfigManager.GetInstance().GetConfig(groupID, initObj.Platform).SiteConfiguration.Data.Features.FutureAssets.UseStartDate)
        ////    };

        ////    retVal = loader.Execute() as List<Media>;

        ////    loader.TryGetItemsCount(out mediaCount);

        ////    return retVal;
        ////}

        //// removed!
        ////public static List<Media> GetChannelMediaList(InitializationObject initObj, string user, string pass, long channelID, string picSize, int pageSize, int pageIndex, int groupID, ref long itemCount)
        ////{
        ////    //TVMAccountType account = SiteMapManager.GetInstance.GetPageData[SiteMapManager.GetInstance.GetKey(groupID, initObj.Platform)].GetTVMAccountByGroupID(subGroupID);
        ////    return GetMediaList(initObj, user, pass, channelID, picSize, pageSize, pageIndex, groupID, LoaderType.Channel, ref itemCount, OrderBy.None);
        ////}

        ////Get all channel medias

        //// Not used!
        ////public static List<Media> GetMediaList(InitializationObject initObj, string user, string pass, long ID, string picSize, int pageSize, int pageIndex, int groupID, LoaderType loaderType, ref long mediaCount, OrderBy orderBy, int[] reqMediaTypes = null, List<KeyValue> tagsMetas = null, CutWith cutWith = CutWith.AND)
        ////{
        ////    List<Media> retVal = new List<Media>();
        ////    dsItemInfo mediaInfo;
        ////    bool isPaged = false;
        ////    switch (loaderType)
        ////    {
        ////        case LoaderType.Channel:
        ////            APIChannelLoader channelLoader = new APIChannelLoader(user, pass, ID, picSize)
        ////            {
        ////                WithInfo = true,
        ////                GroupID = groupID,
        ////                Platform = initObj.Platform,
        ////                PageSize = pageSize,
        ////                PageIndex = pageIndex,
        ////                OrderBy = (TVPPro.SiteManager.Context.Enums.eOrderBy)Enum.Parse(typeof(TVPPro.SiteManager.Context.Enums.eOrderBy),
        ////                orderBy.ToString()),
        ////                DeviceUDID = initObj.UDID,
        ////                GetFutureStartDate = ConfigManager.GetInstance().GetConfig(groupID, initObj.Platform).SiteConfiguration.Data.Features.FutureAssets.UseStartDate,
        ////                Language = initObj.Locale.LocaleLanguage,
        ////                TagsMetas = tagsMetas,
        ////                CutWith = cutWith
        ////            };

        ////            mediaInfo = channelLoader.Execute();
        ////            channelLoader.TryGetItemsCount(out mediaCount);
        ////            isPaged = true;
        ////            break;
        ////        case LoaderType.Related:
        ////            APIRelatedMediaLoader relatedLoader = new APIRelatedMediaLoader(ID, user, pass) { GroupID = groupID, Platform = initObj.Platform, PicSize = picSize, WithInfo = true, PageSize = pageSize, PageIndex = pageIndex, IsPosterPic = false, DeviceUDID = initObj.UDID, MediaTypes = reqMediaTypes, Language = initObj.Locale.LocaleLanguage };
        ////            mediaInfo = relatedLoader.Execute();
        ////            relatedLoader.TryGetItemsCount(out mediaCount);
        ////            isPaged = true;
        ////            break;
        ////        case LoaderType.PeopleWhoWatched:
        ////            mediaInfo = (new APIPeopleWhoWatchedLoader(user, pass, ID, picSize) { GroupID = groupID, Platform = initObj.Platform, WithInfo = true, IsPosterPic = false, Language = initObj.Locale.LocaleLanguage }).Execute();
        ////            isPaged = false;
        ////            break;
        ////        case LoaderType.LastWatched:
        ////            TVMAccountType account = SiteMapManager.GetInstance.GetPageData(groupID, initObj.Platform).GetTVMAccountByAccountType(AccountType.Parent);
        ////            mediaInfo = new APILastWatchedLoader(account.TVMUser, account.TVMPass) { GroupID = groupID, Platform = initObj.Platform, WithInfo = true, SiteGuid = initObj.SiteGuid, PageSize = pageSize, PageIndex = pageIndex, PicSize = picSize, Language = initObj.Locale.LocaleLanguage }.Execute();
        ////            break;
        ////        case LoaderType.Recommended:
        ////            mediaInfo = new TVPApiModule.DataLoaders.APIPersonalRecommendedLoader(user, pass) { GroupID = groupID, Platform = initObj.Platform, WithInfo = true, SiteGuid = initObj.SiteGuid, PageSize = pageSize, PageIndex = pageIndex, PicSize = picSize, MediaTypes = reqMediaTypes, Language = initObj.Locale.LocaleLanguage }.Execute();
        ////            break;
        ////        default:
        ////            mediaInfo = (new APIChannelLoader(user, pass, ID, picSize) { WithInfo = true, GroupID = groupID, Platform = initObj.Platform, DeviceUDID = initObj.UDID, Language = initObj.Locale.LocaleLanguage }.Execute());
        ////            break;
        ////    }

        ////    if (mediaInfo.Item != null && mediaInfo.Item.Count > 0)
        ////    {
        ////        int startIndex = (pageIndex) * pageSize;
        ////        //Local server Paging
        ////        IEnumerable<dsItemInfo.ItemRow> pagedDT;
        ////        if (!isPaged)
        ////        {
        ////            pagedDT = PagingHelper.GetPagedData<dsItemInfo.ItemRow>(startIndex, pageSize, mediaInfo.Item);
        ////        }
        ////        else
        ////        {
        ////            pagedDT = mediaInfo.Item;
        ////        }
        ////        //Parse to WS return objects
        ////        if (pagedDT != null)
        ////        {
        ////            foreach (dsItemInfo.ItemRow row in pagedDT)
        ////            {
        ////                retVal.Add(new Media(row, initObj, groupID, false, mediaCount));
        ////            }
        ////        }
        ////    }
        ////    return retVal;
        ////}

        ////Get all channel medias - Not used!
        ////public static List<Media> GetMediaList(InitializationObject initObj, string user, string pass, long ID, string picSize, int pageSize, int pageIndex, int groupID, LoaderType loaderType, OrderBy orderBy)
        ////{
        ////    long mediaCount = 0;
        ////    return GetMediaList(initObj, user, pass, ID, picSize, pageSize, pageIndex, groupID, loaderType, ref mediaCount, orderBy);
        ////}

        //// Not used!
        ////public static List<Media> GetMediaList(InitializationObject initObj, string user, string pass, long ID, string picSize, int pageSize, int pageIndex, int groupID, LoaderType loaderType, OrderBy orderBy, int[] reqMediaTypes = null, List<KeyValue> tagsMetas = null, CutWith cutWith = CutWith.AND)
        ////{
        ////    long mediaCount = 0;
        ////    return GetMediaList(initObj, user, pass, ID, picSize, pageSize, pageIndex, groupID, loaderType, ref mediaCount, orderBy, reqMediaTypes, tagsMetas, cutWith);
        ////}

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
                        FavoritObject[] favoritesObj = new ApiUsersService(groupID, platform).GetUserFavorites(siteGuid, string.Empty, domainID, string.Empty);//initObj.UDID);

                        if (favoritesObj != null)
                        {
                            mediaIDsList = favoritesObj.Select(f => int.Parse(f.m_sItemCode)).ToList();
                            retVal = new TVPApiModule.CatalogLoaders.APIMediaLoader(mediaIDsList, groupID, platform, udid, SiteHelper.GetClientIP(), picSize, language).Execute() as List<Media>;
                        }

                        break;
                    }
                case UserItemType.Rental:
                    {
                        PermittedMediaContainer[] MediaPermitedItems = new ApiConditionalAccessService(groupID, platform).GetUserPermittedItems(guid);
                        mediaIDsList = MediaPermitedItems.Select(mp => mp.m_nMediaID).ToList();
                        retVal = new TVPApiModule.CatalogLoaders.APIMediaLoader(mediaIDsList, groupID, platform, udid, SiteHelper.GetClientIP(), picSize, language).Execute() as List<Media>;
                        break;
                    }
                case UserItemType.Package:
                    {
                        PermittedSubscriptionContainer[] PermitedPackages = new ApiConditionalAccessService(groupID, platform).GetUserPermitedSubscriptions(guid);
                        if (PermitedPackages != null && PermitedPackages.Length > 0)
                        {
                            List<KeyValue> BaseIdsDict = new List<KeyValue>();
                            StringBuilder sb = new StringBuilder();

                            foreach (PermittedSubscriptionContainer sub in PermitedPackages)
                            {
                                sb.AppendFormat("{0}{1}", sub.m_sSubscriptionCode, ";");
                                var pair = BaseIdsDict.Where(bid => bid.m_sKey == "Base ID").FirstOrDefault();
                                if (pair == null)
                                {
                                    BaseIdsDict.Add(new KeyValue() { m_sKey = "Base ID", m_sValue = sub.m_sSubscriptionCode});
                                }
                                else
                                {
                                    pair.m_sValue = string.Concat(pair.m_sValue, ";", sub.m_sSubscriptionCode);
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

        ////Parse a data row elenment to media - Not used!
        ////private static Media parseItemRowToMedia(dsItemInfo.ItemRow row, string picSize, bool withDynamic, string siteGuid)
        ////{
        ////    Media retVal = new Media();
        ////    retVal.MediaName = row.Title;
        ////    retVal.MediaID = row.ID;
        ////    if (!row.IsMediaTypeIDNull())
        ////    {
        ////        retVal.MediaTypeID = row.MediaTypeID;
        ////    }
        ////    if (!row.IsMediaTypeNull())
        ////    {
        ////        retVal.MediaTypeName = row.MediaType;
        ////    }
        ////    if (!row.IsDescriptionShortNull())
        ////    {
        ////        retVal.Description = row.DescriptionShort;
        ////    }

        ////    if (!row.IsImageLinkNull())
        ////    {
        ////        retVal.PicURL = row.ImageLink;
        ////    }
        ////    if (!row.IsURLNull())
        ////    {
        ////        retVal.URL = row.URL;
        ////    }

        ////    if (!row.IsDurationNull())
        ////    {
        ////        retVal.Duration = row.Duration;
        ////    }

        ////    if (!row.IsRateNull())
        ////    {
        ////        retVal.Rating = row.Rate;
        ////    }
        ////    if (!row.IsViewCounterNull())
        ////    {
        ////        retVal.ViewCounter = row.ViewCounter;
        ////    }
        ////    string[] TagNames = MediaConfiguration.Instance.Data.TVM.MediaInfoStruct.Tags.ToString().Split(new Char[] { ';' });
        ////    System.Data.DataRow[] tagsRow = row.GetChildRows("Item_Tags");
        ////    if (tagsRow != null && tagsRow.Length > 0)
        ////    {
        ////        //Create tag meta pair objects list for all tags
        ////        foreach (string tagName in TagNames)
        ////        {
        ////            if (tagsRow[0].Table.Columns.Contains(tagName) && !string.IsNullOrEmpty(tagsRow[0][tagName].ToString()))
        ////            {
        ////                TagMetaPair pair = new TagMetaPair(tagName, tagsRow[0][tagName].ToString());
        ////                retVal.Tags.Add(pair);
        ////            }
        ////        }
        ////    }
        ////    string[] MetaNames = MediaConfiguration.Instance.Data.TVM.MediaInfoStruct.Metadata.ToString().Split(new Char[] { ';' });
        ////    System.Data.DataRow[] metasRow = row.GetChildRows("Item_Metas");
        ////    if (metasRow != null && metasRow.Length > 0)
        ////    {
        ////        //Create tag meta pair objects list for all metas
        ////        foreach (string metaName in MetaNames)
        ////        {
        ////            if (metasRow[0].Table.Columns.Contains(metaName) && !string.IsNullOrEmpty(metasRow[0][metaName].ToString()))
        ////            {
        ////                TagMetaPair pair = new TagMetaPair(metaName, metasRow[0][metaName].ToString());
        ////                retVal.Metas.Add(pair);
        ////            }
        ////        }
        ////    }
        ////    if (withDynamic)
        ////    {
        ////        DynamicData dynamicObj = new DynamicData();

        ////    }
        ////    return retVal;
        ////}

        //public static List<Media> GetMediasInPackage(InitializationObject initObj, int baseID, int mediaType, int iGroupID, string picSize, int pageSize, int pageIndex)
        //{
        //    return new APISubscriptionMediaLoader(baseID, iGroupID, initObj.Platform, initObj.UDID, SiteHelper.GetClientIP(), initObj.Locale.LocaleLanguage, pageSize, pageIndex, picSize)
        //    {
        //        MediaTypes = new List<int>() { mediaType },
        //    }.Execute() as List<Media>;
        //}

        //public static List<Media> SearchMediaBySubIDs(InitializationObject initObj, string[] subIDs, string picSize, int groupID, Tvinci.Data.Loaders.TvinciPlatform.Catalog.OrderBy orderBy)
        //{
        //    List<KeyValue> orList = new List<KeyValue>();
        //    foreach (var id in subIDs)
        //        orList.Add(new KeyValue() { m_sKey = "Base ID", m_sValue = id });

        //    return new APISearchMediaLoader(groupID, initObj.Platform, initObj.UDID, SiteHelper.GetClientIP(), initObj.Locale.LocaleLanguage, 0, 0, picSize, true, orList, null, null)
        //    {
        //        UseStartDate = true,
        //        OrderBy = orderBy,
        //    }.Execute() as List<Media>;

        //}

    }
}
