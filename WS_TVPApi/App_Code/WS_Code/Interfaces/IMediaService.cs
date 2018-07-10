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
using TVPPro.SiteManager.Objects;
using TVPApiModule.Objects.Responses;
using TVPApiModule.Objects.Requests;

namespace TVPApiServices
{
    [ServiceContract]
    public interface IMediaService
    {
        [OperationContract]
        Media GetMediaInfo(InitializationObject initObj, long MediaID, int mediaType, string picSize, bool withDynamic);

        [OperationContract]
        List<Media> GetChannelMediaList(InitializationObject initObj, long ChannelID, string picSize, int pageSize, int pageIndex, TVPApi.OrderBy orderBy);

        [OperationContract]
        List<Media> GetChannelMultiFilter(InitializationObject initObj, long ChannelID, string picSize, int pageSize, int pageIndex, TVPApi.OrderBy orderBy, eOrderDirection orderDir, List<TagMetaPair> tagsMetas, TVPApiModule.Objects.Enums.eCutWith cutWith);

        [OperationContract]
        List<Media> GetChannelMediaListWithMediaCount(InitializationObject initObj, long ChannelID, string picSize, int pageSize, int pageIndex, ref long mediaCount);

        [OperationContract]
        bool IsMediaFavorite(InitializationObject initObj, int mediaID);

        [OperationContract]
        List<KeyValuePair<long, bool>> AreMediasFavorite(InitializationObject initObj, List<long> mediaIds);

        [OperationContract]
        List<Media> GetRelatedMedias(InitializationObject initObj, int mediaID, int mediaType, string picSize, int pageSize, int pageIndex);

        [OperationContract]
        List<Media> GetRelatedMediaWithMediaCount(InitializationObject initObj, int mediaID, int mediaType, string picSize, int pageSize, int pageIndex, ref long mediaCount);

        [OperationContract]
        List<Media> GetPeopleWhoWatched(InitializationObject initObj, int mediaID, int mediaType, string picSize, int pageSize, int pageIndex);

        [OperationContract]
        List<Media> GetUserSocialMedias(InitializationObject initObj, TVPPro.SiteManager.TvinciPlatform.api.SocialPlatform socialPlatform, TVPPro.SiteManager.TvinciPlatform.api.SocialAction socialAction, string picSize, int pageSize, int pageIndex);

        [OperationContract]
        List<Comment> GetMediaComments(InitializationObject initObj, int mediaID, int pageSize, int pageIndex);

        [OperationContract]
        List<Media> SearchMediaByTag(InitializationObject initObj, string tagName, string value, int mediaType, string picSize, int pageSize, int pageIndex, TVPApi.OrderBy orderBy);

        [OperationContract]
        List<Tvinci.Data.Loaders.TvinciPlatform.Catalog.EPGChannelProgrammeObject> SearchEPG(InitializationObject initObj, string text, string picSize, int pageSize, int pageIndex, TVPApi.OrderBy orderBy);

        [OperationContract]
        List<Media> SearchMediaByMeta(InitializationObject initObj, string metaName, string value, int mediaType, string picSize, int pageSize, int pageIndex, TVPApi.OrderBy orderBy);

        [OperationContract]
        List<Media> SearchMediaByMetaWithMediaCount(InitializationObject initObj, string metaName, string value, int mediaType, string picSize, int pageSize, int pageIndex, TVPApi.OrderBy orderBy, ref long mediaCount);

        [OperationContract]
        Category GetCategory(InitializationObject initObj, int categoryID);

        [OperationContract]
        List<Media> SearchMedia(InitializationObject initObj, string text, int mediaType, string picSize, int pageSize, int pageIndex, TVPApi.OrderBy orderBy);

        [OperationContract]
        List<Media> SearchMediaWithMediaCount(InitializationObject initObj, string text, int mediaType, string picSize, int pageSize, int pageIndex, TVPApi.OrderBy orderBy, ref long mediaCount);

        [OperationContract]
        List<Media> GetUserItems(InitializationObject initObj, UserItemType itemType, int mediaType, string picSize, int pageSize, int start_index);

        [OperationContract]
        List<string> GetNMostSearchedTexts(InitializationObject initObj, int N, int pageSize, int start_index);

        [OperationContract]
        string[] GetAutoCompleteSearchList(InitializationObject initObj, string prefixText, int?[] iMediaTypes);

        [OperationContract]
        string[] GetAutoCompleteSearch(InitializationObject initObj, string prefixText, int[] iMediaTypes, int pageSize, int pageIdx);

        [OperationContract]
        bool ActionDone(InitializationObject initObj, TVPApi.ActionType action, int mediaID, int mediaType, int extraVal);

        [OperationContract]
        TVPPro.SiteManager.TvinciPlatform.api.RateMediaObject RateMedia(InitializationObject initObj, int mediaID, int mediaType, int extraVal);

        [OperationContract]
        List<Media> GetMediasByMostAction(InitializationObject initObj, TVPApi.ActionType action, int mediaType);

        [OperationContract]
        List<Media> GetMediasByRating(InitializationObject initObj, int rating);

        [OperationContract]
        string MediaMark(InitializationObject initObj, action Action, int mediaType, long iMediaID, long iFileID, int iLocation, string NPVRID, long programId);

        [OperationContract]
        string MediaHit(InitializationObject initObj, int mediaType, long iMediaID, long iFileID, int iLocation, string NPVRID, long programId);

        [OperationContract]
        MediaMarkObject GetMediaMark(InitializationObject initObj, int iMediaID, string npvrID);

        [OperationContract]
        bool AddUserSocialAction(InitializationObject initObj, int iMediaID, TVPPro.SiteManager.TvinciPlatform.api.SocialAction action, TVPPro.SiteManager.TvinciPlatform.api.SocialPlatform socialPlatform);

        [OperationContract]
        List<Media> GetMediasInfo(InitializationObject initObj, long[] MediaID, int mediaType, string picSize, bool withDynamic);

        [OperationContract]
        TVPApiModule.Objects.Responses.AssetsBookmarksResponse GetAssetsBookmarks(InitializationObject initObj, List<SlimAssetRequest> assets);

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
        List<Media> SearchMediaByMultiTag(InitializationObject initObj, List<TVPApi.TagMetaPair> tagPairs, int mediaType, string picSize, int pageSize, int pageIndex, TVPApi.OrderBy orderBy);

        [OperationContract]
        string GetSubscriptionProductCode(InitializationObject initObj, int subID);

        [OperationContract]
        string CheckGeoBlockForMedia(InitializationObject initObj, int iMediaID);

        [OperationContract]
        List<Media> GetRecommendedMedias(InitializationObject initObj, string picSize, int pageSize, int pageIndex);

        [OperationContract]
        BillingResponse ChargeUserWithInApp(InitializationObject initObj, double price, string currency, string receipt, string productCode);

        [OperationContract]
        List<SubscriptionPrice> GetSubscriptionDataPrices(InitializationObject initObj, int[] subIDs);

        [OperationContract]
        List<Media> GetSubscriptionMedias(InitializationObject initObj, string[] sSubID, string picSize, TVPApi.OrderBy orderBy);

        [OperationContract]
        PermittedSubscriptionContainer[] GetUserPermitedSubscriptions(InitializationObject initObj);

        [OperationContract]
        void ToggleOfflineMode(InitializationObject initObj, bool isTurnOn);

        [OperationContract]
        List<PermittedPackages> GetUserPermittedPackages(InitializationObject initObj, string picSize);

        [OperationContract]
        List<Media> SearchMediaByMetasTagsExact(InitializationObject initObj, List<TVPApi.TagMetaPair> tagPairs, List<TVPApi.TagMetaPair> metaPairs, int mediaType, string picSize, int pageSize, int pageIndex, TVPApi.OrderBy orderBy);

        [OperationContract]
        TVPPro.SiteManager.TvinciPlatform.api.EPGChannelObject[] GetEPGChannels(InitializationObject initObj, string sPicSize, TVPApi.OrderBy orderBy);

        [OperationContract]
        EPGChannelProgrammeObject[] GetEPGChannelsPrograms(InitializationObject initObj, string sEPGChannelID, string sPicSize, EPGUnit oUnit, int iFromOffset, int iToOffset, int iUTCOffSet);

        [OperationContract]
        List<EPGMultiChannelProgrammeObject> GetEPGMultiChannelProgram(InitializationObject initObj, string[] sEPGChannelID, string sPicSize, EPGUnit oUnit, int iFromOffset, int iToOffset, int iUTCOffSet);

        [OperationContract]
        void SendMessage(InitializationObject initObj, string sSiteGuid, string sRecieverUDID, int iMediaID, int iMediaTypeID, int iLocation, string sAction, string sUsername, string sPassword);

        [OperationContract]
        MBMessage GetMessage(InitializationObject initObj);

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
        List<TVPApi.Channel> GetChannelsList(InitializationObject initObj, string sPicSize);

        [OperationContract]
        string GetMediaLicenseData(InitializationObject initObj, int iMediaFileID, int iMediaID);

        [OperationContract]
        List<TVPPro.SiteManager.Objects.EPGComment> GetEPGCommentsList(InitializationObject initObj, int epgProgramID, int pageSize, int pageIndex);

        [OperationContract]
        string AddEPGComment(InitializationObject initObj, int epgProgramID, string contentText, string header, string subHeader, string writer, bool autoActive);

        [OperationContract]
        List<Media> SearchMediaByAndOrList(InitializationObject initObj, List<KeyValue> orList, List<KeyValue> andList, int mediaType, int pageSize, int pageIndex, bool exact, Tvinci.Data.Loaders.TvinciPlatform.Catalog.OrderBy orderBy, Tvinci.Data.Loaders.TvinciPlatform.Catalog.OrderDir orderDir, string orderMeta);

        [OperationContract]
        List<string> GetEPGAutoComplete(InitializationObject initObj, string searchText, int pageSize, int pageIndex);

        [OperationContract]
        List<EPGChannelProgrammeObject> SearchEPGPrograms(InitializationObject initObj, string searchText, int pageSize, int pageIndex);

        [OperationContract]
        List<AssetStatsResult> GetAssetsStatsForTimePeriod(InitializationObject initObj, int pageSize, int pageIndex, List<int> assetsIDs, StatsType assetType, DateTime startTime, DateTime endTime);

        [OperationContract]
        List<AssetStatsResult> GetAssetsStats(InitializationObject initObj, int pageSize, int pageIndex, List<int> assetsIDs, StatsType assetType);

        [OperationContract]
        List<Media> GetRecommendedMediasByTypes(InitializationObject initObj, string picSize, int pageSize, int pageIndex, int[] reqMediaTypes);

        [OperationContract]
        List<Media> GetRelatedMediasByTypes(InitializationObject initObj, int mediaID, int mediaType, string picSize, int pageSize, int pageIndex, int[] reqMediaTypes);

        [OperationContract]
        List<EPGChannelProgrammeObject> SearchEPGByAndOrList(InitializationObject initObj, List<KeyValue> orList, List<KeyValue> andList, int pageSize, int pageIndex);

        [OperationContract]
        List<Media> GetBundleMedia(InitializationObject initObj, eBundleType bundleType, int bundleId,
            Tvinci.Data.Loaders.TvinciPlatform.Catalog.OrderBy orderBy, Tvinci.Data.Loaders.TvinciPlatform.Catalog.OrderDir orderDir, string mediaType, int pageIndex, int pageSize);

        [OperationContract]
        bool DoesBundleContainMedia(InitializationObject initObj, eBundleType bundleType, int bundleId, int mediaId, string mediaType);

        [OperationContract]
        BuzzWeightedAverScore GetBuzzMeterData(InitializationObject initObj, string sKey);

        [OperationContract]
        List<BaseCrowdsourceItem> GetCrowdsourceFeed(InitializationObject initObj, int pageSize, long epochLastTime);

        [OperationContract]
        string GetMediaLicenseLinkWithIP(InitializationObject initObj, int mediaFileID, string baseLink, string clientIP);

        [OperationContract]
        List<RecordedEPGChannelProgrammeObject> GetRecordings(InitializationObject initObj, int pageSize, int pageIndex,
            NPVRSearchBy searchBy, int epgChannelID, RecordingStatus recordingStatus, List<string> recordingIDs, List<int> programIDs,
            List<string> seriesIDs, DateTime startDate, RecordedEPGOrderObj recordedEPGOrderObj, int? version);

        [OperationContract]
        List<RecordedSeriesObject> GetSeriesRecordings(InitializationObject initObj, int pageSize, int pageIndex, RecordedEPGOrderObj recordedEPGOrderObj
            , int? version, string seriesId, int? seasonNumber);

        [OperationContract]
        TVPApiModule.Objects.Responses.UnifiedSearchResponse SearchAssets(InitializationObject initObj,
            List<int> filter_types, string filter, string order_by, List<string> with, int page_index, int? page_size, string request_id);

        [OperationContract]
        WatchHistory WatchHistory(InitializationObject initObj, List<int> filter_types, eWatchStatus filter_status, int? days, List<string> with, int? page_index, int page_size);

        [OperationContract]
        TVPApiModule.Objects.Responses.AutocompleteResponse Autocomplete(InitializationObject initObj, List<int> filter_types, string query, string order_by, List<string> with, int? page_size);

        [OperationContract]
        TVPApiModule.Objects.Responses.UnifiedSearchResponse GetChannelAssets(InitializationObject initObj,
            int kaltura_identifier,
            string filter,
            string order_by,
            List<string> with, int page_index, int? page_size);

        [OperationContract]
        TVPApiModule.Objects.Responses.UnifiedSearchResponseWithRequestId GetExternalAssets(InitializationObject initObj,
            string alias,
            string utc_offset,
            string free_param,
            List<string> with, int page_index, int? page_size);

        [OperationContract]
        TVPApiModule.Objects.Responses.UnifiedSearchResponse GetBundleAssets(InitializationObject initObj, eBundleType bundleType, int bundleId,
            Tvinci.Data.Loaders.TvinciPlatform.Catalog.OrderBy orderBy, Tvinci.Data.Loaders.TvinciPlatform.Catalog.OrderDir orderDir, string mediaType, int pageIndex, int pageSize);
    }
}
