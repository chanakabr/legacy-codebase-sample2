using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using TVPApi;
using Tvinci.Data.TVMDataLoader.Protocols.MediaMark;
using TVPPro.SiteManager.TvinciPlatform.Pricing;
using TVPApiModule.Objects;
using TVPPro.SiteManager.Context;
using TVPPro.SiteManager.TvinciPlatform.Users;
using TVPPro.SiteManager.TvinciPlatform.ConditionalAccess;
using Tvinci.Data.Loaders.TvinciPlatform.Catalog;
using TVPApiModule.Context;
using TVPApiModule.Objects.Responses;
using TVPApiModule.Helper;

namespace TVPApiServices
{
    [ServiceContract]
    public interface IMediaService
    {
        //[OperationContract]
        //Media GetMediaInfo(InitializationObject initObj, int MediaID, string picSize, bool withDynamic);

        //[OperationContract]
        //List<Media> GetChannelMediaList(InitializationObject initObj, int ChannelID, string picSize, int pageSize, int pageIndex, TVPApi.OrderBy orderBy);

        [OperationContract]
        List<Media> GetChannelMultiFilter(InitializationObject initObj, int ChannelID, string picSize, int pageSize, int pageIndex, TVPApiModule.Context.OrderBy orderBy, eOrderDirection orderDir, List<KeyValue> tagsMetas, CutWith cutWith);

        //[OperationContract]
        //List<Media> GetChannelMediaListWithMediaCount(InitializationObject initObj, int ChannelID, string picSize, int pageSize, int pageIndex, ref long mediaCount);

        //[OperationContract]
        //bool IsMediaFavorite(InitializationObject initObj, int mediaID);

        [OperationContract]
        List<KeyValuePair<int, bool>> AreMediasFavorite(InitializationObject initObj, List<int> mediaIds);

        //[OperationContract]
        //List<Media> GetRelatedMedias(InitializationObject initObj, int mediaID, int mediaType, string picSize, int pageSize, int pageIndex);

        //[OperationContract]
        //List<Media> GetRelatedMediaWithMediaCount(InitializationObject initObj, int mediaID, int mediaType, string picSize, int pageSize, int pageIndex, ref long mediaCount);

        [OperationContract]
        List<Media> GetPeopleWhoWatched(InitializationObject initObj, int mediaID, string picSize, int pageSize, int pageIndex);

        [OperationContract]
        List<Media> GetUserSocialMedias(InitializationObject initObj, int socialPlatform, int action, string picSize, int pageSize, int pageIndex);

        [OperationContract]
        List<Comment> GetMediaComments(InitializationObject initObj, int mediaID, int pageSize, int pageIndex);

        //[OperationContract]
        //List<Media> SearchMediaByTag(InitializationObject initObj, string tagName, string value, int mediaType, string picSize, int pageSize, int pageIndex, TVPApi.OrderBy orderBy);

        [OperationContract]
        TVPPro.SiteManager.TvinciPlatform.api.EPGChannelProgrammeObject[] SearchEPG(InitializationObject initObj, string text, string picSize, int pageSize, int pageIndex, TVPApiModule.Context.OrderBy orderBy);

        //[OperationContract]
        //List<Media> SearchMediaByMeta(InitializationObject initObj, string metaName, string value, int mediaType, string picSize, int pageSize, int pageIndex, TVPApi.OrderBy orderBy);

        //[OperationContract]
        //List<Media> SearchMediaByMetaWithMediaCount(InitializationObject initObj, string metaName, string value, int mediaType, string picSize, int pageSize, int pageIndex, TVPApi.OrderBy orderBy, ref long mediaCount);

        [OperationContract]
        Category GetCategory(InitializationObject initObj, int categoryID);

        //[OperationContract]
        //List<Media> SearchMedia(InitializationObject initObj, string text, int mediaType, string picSize, int pageSize, int pageIndex, TVPApi.OrderBy orderBy);

        //[OperationContract]
        //List<Media> SearchMediaWithMediaCount(InitializationObject initObj, string text, int mediaType, string picSize, int pageSize, int pageIndex, TVPApi.OrderBy orderBy, ref long mediaCount);

        [OperationContract]
        List<Media> GetUserItems(InitializationObject initObj, UserItemType itemType, string picSize, int pageSize, int start_index);

        //[OperationContract]
        //List<string> GetNMostSearchedTexts(InitializationObject initObj, int N, int pageSize, int start_index);

        [OperationContract]
        string[] GetAutoCompleteSearchList(InitializationObject initObj, string prefixText, int[] iMediaTypes);

        //[OperationContract]
        //string[] GetAutoCompleteSearch(InitializationObject initObj, string prefixText, int[] iMediaTypes, int pageSize, int pageIdx);

        [OperationContract]
        bool ActionDone(InitializationObject initObj, ActionType action, int mediaID, int mediaType, int extraVal);

        //[OperationContract]
        //TVPPro.SiteManager.TvinciPlatform.api.RateMediaObject RateMedia(InitializationObject initObj, int mediaID, int mediaType, int extraVal);

        //[OperationContract]
        //List<Media> GetMediasByMostAction(InitializationObject initObj, TVPApi.ActionType action, int mediaType);

        [OperationContract]
        List<Media> GetMediasByRating(InitializationObject initObj, int rating);

        [OperationContract]
        string MediaMark(InitializationObject initObj, action Action, int mediaType, int iMediaID, int iFileID, int iLocation);

        [OperationContract]
        string MediaHit(InitializationObject initObj, int mediaType, int iMediaID, int iFileID, int iLocation);

        [OperationContract]
        TVPPro.SiteManager.TvinciPlatform.api.MediaMarkObject GetMediaMark(InitializationObject initObj, int iMediaID);

        //[OperationContract]
        //bool AddUserSocialAction(InitializationObject initObj, int iMediaID, TVPPro.SiteManager.TvinciPlatform.api.SocialAction action, TVPPro.SiteManager.TvinciPlatform.api.SocialPlatform socialPlatform);

        [OperationContract]
        List<Media> GetMediasInfo(InitializationObject initObj, List<int> MediaID, string picSize);

        [OperationContract]
        string[] GetPrepaidBalance(InitializationObject initObj, string couponCode);

        [OperationContract]
        string ChargeMediaWithPrepaid(InitializationObject initObj, double price, string currency, int mediaFileID, string ppvModuleCode, string couponCode);

        [OperationContract]
        List<Media> GetLastWatchedMediasByPeriod(InitializationObject initObj, int mediaID, int mediaType, string picSize, int periodBefore, MediaHelper.ePeriod byPeriod);

        //[OperationContract]
        //bool IsUserVoted(InitializationObject initObj, int iMediaID);

        //[OperationContract]
        //UserOfflineObject[] GetUserOfflineList(InitializationObject initObj);

        //[OperationContract]
        //List<Media> GetUserOfflineListFull(InitializationObject initObj, string picSize, bool withDynamic);

        //[OperationContract]
        //bool AddUserOfflineMedia(InitializationObject initObj, int mediaID);

        //[OperationContract]
        //bool RemoveUserOfflineMedia(InitializationObject initObj, int mediaID);

        //[OperationContract]
        //bool ClearUserOfflineList(InitializationObject initObj);

        //[OperationContract]
        //TVPApi.PriceReason GetItemPriceReason(InitializationObject initObj, int iFileID);

        [OperationContract]
        bool IsItemPurchased(InitializationObject initObj, int iFileID, string sUserGuid);

        //[OperationContract]
        //List<Media> SearchMediaByMultiTag(InitializationObject initObj, List<TVPApi.TagMetaPair> tagPairs, int mediaType, string picSize, int pageSize, int pageIndex, TVPApi.OrderBy orderBy);

        [OperationContract]
        string GetSubscriptionProductCode(InitializationObject initObj, int subID);

        //[OperationContract]
        //string CheckGeoBlockForMedia(InitializationObject initObj, int iMediaID);

        //[OperationContract]
        //List<Media> GetRecommendedMedias(InitializationObject initObj, string picSize, int pageSize, int pageIndex);

        [OperationContract]
        TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.BillingResponse ChargeUserWithInApp(InitializationObject initObj, double price, string currency, string receipt, string productCode);

        [OperationContract]
        bool CancelSubscription(InitializationObject initObj, string sSubscriptionID, int sSubscriptionPurchaseID);

        [OperationContract]
        List<SubscriptionPrice> GetSubscriptionDataPrices(InitializationObject initObj, int[] subIDs);

        [OperationContract]
        List<Media> GetSubscriptionMedias(InitializationObject initObj, string[] sSubID, string picSize, Tvinci.Data.Loaders.TvinciPlatform.Catalog.OrderBy orderBy);

        [OperationContract]
        TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.PermittedSubscriptionContainer[] GetUserPermitedSubscriptions(InitializationObject initObj, string siteGuid);

        //[OperationContract]
        //void ToggleOfflineMode(InitializationObject initObj, bool isTurnOn);

        [OperationContract]
        List<PermittedPackages> GetUserPermittedPackages(InitializationObject initObj, string siteGuid, string picSize);

        //[OperationContract]
        //List<Media> SearchMediaByMetasTagsExact(InitializationObject initObj, List<TVPApi.TagMetaPair> tagPairs, List<TVPApi.TagMetaPair> metaPairs, int mediaType, string picSize, int pageSize, int pageIndex, TVPApi.OrderBy orderBy);

        //[OperationContract]
        //List<Media> SearchMediaByAndOrList(InitializationObject initObj, List<KeyValue> orList, List<KeyValue> andList, int mediaType, int pageSize, int pageIndex, bool exact, TVPApi.OrderBy orderBy);

        //[OperationContract]
        //List<Media> SearchMediaByTypes(InitializationObject initObj, string text, int[] mediaType, string picSize, int pageSize, int pageIndex, TVPApi.OrderBy orderBy);

        [OperationContract]
        TVPPro.SiteManager.TvinciPlatform.api.EPGChannelObject[] GetEPGChannels(InitializationObject initObj, string sPicSize, TVPApiModule.Context.OrderBy orderBy);
        
        //[OperationContract]
        //TVPPro.SiteManager.TvinciPlatform.api.EPGChannelProgrammeObject[] GetEPGChannelProgrammeByDates(InitializationObject initObj, string channelID, string picSize, DateTime fromDate, DateTime toDate, int utcOffset);

        //[OperationContract]
        //TVPPro.SiteManager.TvinciPlatform.api.EPGChannelProgrammeObject[] GetEPGChannelsPrograms(InitializationObject initObj, string sEPGChannelID, string sPicSize, TVPPro.SiteManager.TvinciPlatform.api.EPGUnit oUnit, int iFromOffset, int iToOffset, int iUTCOffSet);

        [OperationContract]
        TVPPro.SiteManager.TvinciPlatform.api.EPGMultiChannelProgrammeObject[] GetEPGMultiChannelProgram(InitializationObject initObj, string[] sEPGChannelID, string sPicSize, TVPPro.SiteManager.TvinciPlatform.api.EPGUnit oUnit, int iFromOffset, int iToOffset, int iUTCOffSet);

        [OperationContract]
        void SendMessage(string sSiteGuid, string sRecieverUDID, int iMediaID, int iMediaTypeID, int iLocation, string sAction, string sUsername, string sPassword);

        [OperationContract]
        MBMessage GetMessage(string sUDID);

        [OperationContract]
        bool AddComment(InitializationObject initObj, int mediaID, int mediaType, string writer, string header, string subheader, string content, bool autoActive);

        [OperationContract]
        TVPPro.SiteManager.TvinciPlatform.api.GroupRule[] GetGroupMediaRules(InitializationObject initObj, int mediaID);

        [OperationContract]
        bool SendToFriend(InitializationObject initObj, int mediaID, string senderName, string senderEmail, string toEmail);

        [OperationContract]
        string ChargeUserForMediaSubscription(InitializationObject initObj, double iPrice, string sCurrency, string sSubscriptionID, string sCouponCode, string sUserIP, string sExtraParameters, string sUDID);

        [OperationContract]
        string DummyChargeUserForSubscription(InitializationObject initObj, string siteGuid, double iPrice, string sCurrency, string sSubscriptionID, string sCouponCode, string sUserIP, string sExtraParameters, string sUDID);

        //[OperationContract]
        //string ChargeUserForMediaFile(InitializationObject initObj, double iPrice, string sCurrency, int iFileID, string sPPVModuleCode, string sUserIP, string sCoupon);

        [OperationContract]
        string DummyChargeUserForMediaFile(InitializationObject initObj, string siteGuid, double iPrice, string sCurrency, int iFileID, string sPPVModuleCode, string sUserIP, string sCoupon);

        [OperationContract]
        List<Channel> GetChannelsList(InitializationObject initObj, string sPicSize);

        [OperationContract]
        string GetMediaLicenseData(InitializationObject initObj, int iMediaFileID, int iMediaID);

        [OperationContract]
        List<TVPPro.SiteManager.Objects.EPGComment> GetEPGCommentsList(InitializationObject initObj, int epgProgramID, int pageSize, int pageIndex);

        [OperationContract]
        string AddEPGComment(InitializationObject initObj, int epgProgramID, string contentText, string header, string subHeader, string writer, bool autoActive);
    }
}
