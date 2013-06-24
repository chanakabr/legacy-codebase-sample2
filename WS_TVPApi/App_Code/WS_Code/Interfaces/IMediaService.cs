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

namespace TVPApiServices
{
    [ServiceContract]
    public interface IMediaService
    {
        [OperationContract]
        Media GetMediaInfo(InitializationObject initObj, long MediaID, int mediaType, string picSize, bool withDynamic);

        [OperationContract]
        List<Media> GetChannelMediaList(InitializationObject initObj, long ChannelID, string picSize, int pageSize, int pageIndex, OrderBy orderBy);

        [OperationContract]
        List<Media> GetChannelMultiFilter(InitializationObject initObj, long ChannelID, string picSize, int pageSize, int pageIndex, OrderBy orderBy, string metaName, eOrderDirection orderDir, List<TVPApi.TagMetaPair> metas, List<TVPApi.TagMetaPair> tags);

        [OperationContract]
        List<Media> GetChannelMediaListWithMediaCount(InitializationObject initObj, long ChannelID, string picSize, int pageSize, int pageIndex, ref long mediaCount);

        [OperationContract]
        bool IsMediaFavorite(InitializationObject initObj, int mediaID);

        [OperationContract]
        List<TVPApi.TagMetaPair> AreMediasFavorite(InitializationObject initObj, string[] mediaIds);

        [OperationContract]
        List<Media> GetRelatedMedias(InitializationObject initObj, int mediaID, int mediaType, string picSize, int pageSize, int pageIndex);

        [OperationContract]
        List<Media> GetRelatedMediaWithMediaCount(InitializationObject initObj, int mediaID, int mediaType, string picSize, int pageSize, int pageIndex, ref long mediaCount);

        [OperationContract]
        List<Media> GetPeopleWhoWatched(InitializationObject initObj, int mediaID, int mediaType, string picSize, int pageSize, int pageIndex);

        [OperationContract]
        List<Media> GetUserSocialMedias(InitializationObject initObj, string socialPlatform, string action, string picSize, int pageSize, int pageIndex);

        [OperationContract]
        List<Comment> GetMediaComments(InitializationObject initObj, int mediaID, int pageSize, int pageIndex);

        [OperationContract]
        List<Media> SearchMediaByTag(InitializationObject initObj, string tagName, string value, int mediaType, string picSize, int pageSize, int pageIndex, OrderBy orderBy);

        [OperationContract]
        TVPPro.SiteManager.TvinciPlatform.api.EPGChannelProgrammeObject[] SearchEPG(InitializationObject initObj, string text, string picSize, int pageSize, int pageIndex, OrderBy orderBy);

        [OperationContract]
        List<Media> SearchMediaByMeta(InitializationObject initObj, string metaName, string value, int mediaType, string picSize, int pageSize, int pageIndex, OrderBy orderBy);

        [OperationContract]
        List<Media> SearchMediaByMetaWithMediaCount(InitializationObject initObj, string metaName, string value, int mediaType, string picSize, int pageSize, int pageIndex, OrderBy orderBy, ref long mediaCount);

        [OperationContract]
        Category GetCategory(InitializationObject initObj, int categoryID);

        [OperationContract]
        List<Media> SearchMedia(InitializationObject initObj, string text, int mediaType, string picSize, int pageSize, int pageIndex, OrderBy orderBy);

        [OperationContract]
        List<Media> SearchMediaWithMediaCount(InitializationObject initObj, string text, int mediaType, string picSize, int pageSize, int pageIndex, OrderBy orderBy, ref long mediaCount);

        [OperationContract]
        List<Media> GetUserItems(InitializationObject initObj, UserItemType itemType, int mediaType, string picSize, int pageSize, int start_index);

        [OperationContract]
        List<string> GetNMostSearchedTexts(InitializationObject initObj, int N, int pageSize, int start_index);

        [OperationContract]
        string[] GetAutoCompleteSearchList(InitializationObject initObj, string prefixText, int?[] iMediaTypes);

        [OperationContract]
        bool ActionDone(InitializationObject initObj, TVPApi.ActionType action, int mediaID, int mediaType, int extraVal);

        [OperationContract]
        TVPPro.SiteManager.TvinciPlatform.api.RateMediaObject RateMedia(InitializationObject initObj, int mediaID, int mediaType, int extraVal);

        [OperationContract]
        List<Media> GetMediasByMostAction(InitializationObject initObj, TVPApi.ActionType action, int mediaType);

        [OperationContract]
        List<Media> GetMediasByRating(InitializationObject initObj, int rating);

        [OperationContract]
        string MediaMark(InitializationObject initObj, action Action, int mediaType, long iMediaID, long iFileID, int iLocation);

        [OperationContract]
        string MediaHit(InitializationObject initObj, int mediaType, long iMediaID, long iFileID, int iLocation);

        [OperationContract]
        TVPPro.SiteManager.TvinciPlatform.api.MediaMarkObject GetMediaMark(InitializationObject initObj, int iMediaID);

        [OperationContract]
        bool AddUserSocialAction(InitializationObject initObj, int iMediaID, TVPPro.SiteManager.TvinciPlatform.api.SocialAction action, TVPPro.SiteManager.TvinciPlatform.api.SocialPlatform socialPlatform);

        [OperationContract]
        List<Media> GetMediasInfo(InitializationObject initObj, long[] MediaID, int mediaType, string picSize, bool withDynamic);

        [OperationContract]
        string[] GetPrepaidBalance(InitializationObject initObj, string couponCode);

        [OperationContract]
        string ChargeMediaWithPrepaid(InitializationObject initObj, double price, string currency, int mediaFileID, string ppvModuleCode, string couponCode);

        [OperationContract]
        List<Media> GetLastWatchedMediasByPeriod(InitializationObject initObj, int mediaID, int mediaType, string picSize, int periodBefore, MediaHelper.ePeriod byPeriod);

        [OperationContract]
        bool IsUserVoted(InitializationObject initObj, int iMediaID);

        [OperationContract]
        UserOfflineObject[] GetUserOfflineList(InitializationObject initObj);

        [OperationContract]
        List<Media> GetUserOfflineListFull(InitializationObject initObj, string picSize, bool withDynamic);

        [OperationContract]
        bool AddUserOfflineMedia(InitializationObject initObj, int mediaID);

        [OperationContract]
        bool RemoveUserOfflineMedia(InitializationObject initObj, int mediaID);

        [OperationContract]
        bool ClearUserOfflineList(InitializationObject initObj);

        [OperationContract]
        TVPApi.PriceReason GetItemPriceReason(InitializationObject initObj, int iFileID);

        [OperationContract]
        bool IsItemPurchased(InitializationObject initObj, int iFileID, string sUserGuid);

        [OperationContract]
        List<Media> SearchMediaByMultiTag(InitializationObject initObj, List<TVPApi.TagMetaPair> tagPairs, int mediaType, string picSize, int pageSize, int pageIndex, OrderBy orderBy);

        [OperationContract]
        string GetSubscriptionProductCode(InitializationObject initObj, int subID);

        [OperationContract]
        string CheckGeoBlockForMedia(InitializationObject initObj, int iMediaID);

        [OperationContract]
        List<Media> GetRecommendedMedias(InitializationObject initObj, string picSize, int pageSize, int pageIndex);

        [OperationContract]
        BillingResponse ChargeUserWithInApp(InitializationObject initObj, double price, string currency, string receipt, string productCode);

        [OperationContract]
        bool CancelSubscription(InitializationObject initObj, string sSubscriptionID, int sSubscriptionPurchaseID);

        [OperationContract]
        List<SubscriptionPrice> GetSubscriptionDataPrices(InitializationObject initObj, int[] subIDs);

        [OperationContract]
        List<Media> GetSubscriptionMedias(InitializationObject initObj, string[] sSubID, string picSize, OrderBy orderBy);

        [OperationContract]
        PermittedSubscriptionContainer[] GetUserPermitedSubscriptions(InitializationObject initObj);

        [OperationContract]
        void ToggleOfflineMode(InitializationObject initObj, bool isTurnOn);

        [OperationContract]
        List<PermittedPackages> GetUserPermittedPackages(InitializationObject initObj);

        [OperationContract]
        List<Media> SearchMediaByMetasTagsExact(InitializationObject initObj, List<TVPApi.TagMetaPair> tagPairs, List<TVPApi.TagMetaPair> metaPairs, int mediaType, string picSize, int pageSize, int pageIndex, OrderBy orderBy);

        [OperationContract]
        TVPPro.SiteManager.TvinciPlatform.api.EPGChannelObject[] GetEPGChannels(InitializationObject initObj, string sPicSize, OrderBy orderBy);

        [OperationContract]
        TVPPro.SiteManager.TvinciPlatform.api.EPGChannelProgrammeObject[] GetEPGChannelsPrograms(InitializationObject initObj, string sEPGChannelID, string sPicSize, TVPPro.SiteManager.TvinciPlatform.api.EPGUnit oUnit, int iFromOffset, int iToOffset, int iUTCOffSet);

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
        string SendToFriend(InitializationObject initObj, int mediaID, string senderName, string senderEmail, string toEmail, string msg);

        [OperationContract]
        string ChargeUserForMediaSubscription(InitializationObject initObj, double iPrice, string sCurrency, string sSubscriptionID, string sCouponCode, string sUserIP, string sExtraParameters, string sUDID);

        [OperationContract]
        string DummyChargeUserForSubscription(InitializationObject initObj, double iPrice, string sCurrency, string sSubscriptionID, string sCouponCode, string sUserIP, string sExtraParameters, string sUDID);

        [OperationContract]
        string ChargeUserForMediaFile(InitializationObject initObj, double iPrice, string sCurrency, int iFileID, string sPPVModuleCode, string sUserIP, string sCoupon);

        [OperationContract]
        string DummyChargeUserForMediaFile(InitializationObject initObj, double iPrice, string sCurrency, int iFileID, string sPPVModuleCode, string sUserIP, string sCoupon);

        [OperationContract]
        List<Channel> GetChannelsList(InitializationObject initObj, string sPicSize);
    }
}
