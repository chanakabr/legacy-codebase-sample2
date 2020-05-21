using System;
using System.Collections.Generic;
using System.ServiceModel;
using ApiObjects;
using ApiObjects.Billing;
using ApiObjects.ConditionalAccess;
using ApiObjects.Response;
using ApiObjects.SearchObjects;
using ApiObjects.TimeShiftedTv;
using Core.ConditionalAccess;
using Core.ConditionalAccess.Response;

namespace WebAPI.WebServices
{
    
    [ServiceContract(Namespace = "http://ca.tvinci.com/")]
    public interface IConditionalAccessService
    {
        [OperationContract]
        bool ActivateCampaign(string sWSUserName, string sWSPassword, int campaignID, CampaignActionInfo actionInfo);
        [OperationContract]
        CampaignActionInfo ActivateCampaignWithInfo(string sWSUserName, string sWSPassword, int campaignID, CampaignActionInfo actionInfo);
        [OperationContract]
        CompensationResponse AddCompensation(string sWSUserName, string sWSPassword, string userId, Compensation compensation);
        [OperationContract]
        int AD_GetCustomDataID(string sWSUserName, string sWSPassword, string sSiteGUID, double dPrice, string sCurrencyCode3, int assetID, string sPPVModuleCode, string sCampaignCode, string sCouponCode, string sPaymentMethod, string sUserIP, string sCountryCd2, string sLanguageCode3, string sDeviceName, int assetType);
        [OperationContract]
        string BulkRecoveryForRenewSubscriptions(string sWSUserName, string sWSPassword, DateTime endDateStartRange, DateTime endDateEndRange);
        [OperationContract]
        Recording CancelRecord(string sWSUserName, string sWSPassword, string userId, long domainId, long recordingId);
        [OperationContract]
        SeriesRecording CancelSeriesRecord(string sWSUserName, string sWSPassword, string userId, long domainId, long recordingId, long epgId, long seasonNumber);
        [OperationContract]
        Status CancelServiceNow(string sWSUserName, string sWSPassword, int nDomainId, int nAssetID, eTransactionType transactionType, bool bIsForce = false, string udid = null);
        [OperationContract]
        bool CancelSubscription(string sWSUserName, string sWSPassword, string sSiteGUID, string sSubscriptionCode, int nSubscriptionPurchaseID);
        [OperationContract]
        Status CancelSubscriptionRenewal(string sWSUserName, string sWSPassword, int nDomainId, string sSubscriptionCode, string userId, string udid, string userIp);
        [OperationContract]
        bool CancelTransaction(string sWSUserName, string sWSPassword, string sSiteGuid, int nAssetID, eTransactionType transactionType, bool bIsForce = false);
        [OperationContract]
        BillingResponse CC_ChargeUserForCollection(string sWSUserName, string sWSPassword, string sSiteGUID, double dPrice, string sCurrencyCode3, string sCollectionCode, string sCouponCode, string sUserIP, string sExtraParameters, string sCountryCd2, string sLanguageCode3, string sDeviceName, string sPaymentMethodID, string sEncryptedCVV);
        [OperationContract]
        BillingStatusResponse CC_ChargeUserForMediaFile(string sWSUserName, string sWSPassword, string sSiteGUID, double dPrice, string sCurrencyCode3, int nMediaFileID, string sPPVModuleCode, string sCouponCode, string sUserIP, string sExtraParameters, string sCountryCd2, string sLanguageCode3, string sDeviceName, string sPaymentMethodID, string sEncryptedCVV);
        [OperationContract]
        BillingResponse CC_ChargeUserForPrePaid(string sWSUserName, string sWSPassword, string sSiteGUID, double dPrice, string sCurrencyCode3, string sPrePaidCode, string sCouponCode, string sUserIP, string sExtraParameters, string sCountryCd2, string sLanguageCode3, string sDeviceName);
        [OperationContract]
        BillingStatusResponse CC_ChargeUserForSubscription(string sWSUserName, string sWSPassword, string sSiteGUID, double dPrice, string sCurrencyCode3, string sSubscriptionCode, string sCouponCode, string sUserIP, string sExtraParameters, string sCountryCd2, string sLanguageCode3, string sDeviceName, string sPaymentMethodID, string sEncryptedCVV);
        [OperationContract]
        BillingResponse CC_DummyChargeUserForCollection(string sWSUserName, string sWSPassword, string sSiteGUID, double dPrice, string sCurrencyCode3, string sCollectionCode, string sCouponCode, string sUserIP, string sExtraParameters, string sCountryCd2, string sLanguageCode3, string sDeviceName);
        [OperationContract]
        BillingResponse CC_DummyChargeUserForMediaFile(string sWSUserName, string sWSPassword, string sSiteGUID, double dPrice, string sCurrencyCode3, int nMediaFileID, string sPPVModuleCode, string sCouponCode, string sUserIP, string sExtraParameters, string sCountryCd2, string sLanguageCode3, string sDeviceName);
        [OperationContract]
        BillingResponse CC_DummyChargeUserForPrePaid(string sWSUserName, string sWSPassword, string sSiteGUID, double dPrice, string sCurrencyCode3, string sPrePaidCode, string sCouponCode, string sUserIP, string sExtraParameters, string sCountryCd2, string sLanguageCode3, string sDeviceName);
        [OperationContract]
        BillingResponse CC_DummyChargeUserForSubscription(string sWSUserName, string sWSPassword, string sSiteGUID, double dPrice, string sCurrencyCode3, string sSubscriptionCode, string sCouponCode, string sUserIP, string sExtraParameters, string sCountryCd2, string sLanguageCode3, string sDeviceName);
        [OperationContract]
        BillingResponse Cellular_ChargeUserForMediaFile(string sWSUserName, string sWSPassword, string sSiteGUID, double dPrice, string sCurrencyCode3, int nMediaFileID, string sPPVModuleCode, string sCouponCode, string sUserIP, string sExtraParameters, string sCountryCd2, string sLanguageCode3, string sDeviceName);
        [OperationContract]
        BillingResponse Cellular_ChargeUserForSubscription(string sWSUserName, string sWSPassword, string sSiteGUID, double dPrice, string sCurrencyCode3, string sSubscriptionCode, string sCouponCode, string sUserIP, string sExtraParameters, string sCountryCd2, string sLanguageCode3, string sDeviceName);
        [OperationContract]
        ChangeSubscriptionStatus ChangeSubscription(string sWSUserName, string sWSPassword, string sSiteGuid, int nOldSubscription, int nNewSubscription, string udid);
        [OperationContract]
        bool ChangeSubscriptionDates(string sWSUserName, string sWSPassword, string sSiteGUID, string sSubscriptionCode, int nSubscriptionPurchaseID, int dAdditionInDays, bool bNewRenewable);
        [OperationContract]
        Status CheckPendingTransaction(string wsUserName, string wsPassword, long paymentGatewayPendingId, int numberOfRetries, string billingGuid, long paymentGatewayTransactionId, string siteGuid, int productId, int productType);
        [OperationContract]
        bool CheckRecordingDuplicateCrids(string sWSUserName, string sWSPassword, long recordingId);
        [OperationContract]
        bool CleanupRecordings();
        [OperationContract]
        bool CompleteDomainSeriesRecordings(string sWSUserName, string sWSPassword, long domainId);
        [OperationContract]
        Status DeleteCDVRAdapter(string sWSUserName, string sWSPassword, int adapterId);
        [OperationContract]
        Status DeleteCompensation(string sWSUserName, string sWSPassword, long compensationId);
        [OperationContract]
        Recording DeleteRecord(string sWSUserName, string sWSPassword, string userId, long domainId, long recordingId);
        [OperationContract]
        SeriesRecording DeleteSeriesRecord(string sWSUserName, string sWSPassword, string userId, long domainId, long recordingId, long epgId, long seasonNumber);
        [OperationContract]
        bool DistributeRecording(string sWSUserName, string sWSPassword, long epgId, long Id, DateTime epgStartDate);
        [OperationContract]
        bool DistributeRecordingWithDomainIds(string sWSUserName, string sWSPassword, long epgId, long Id, DateTime epgStartDate, long[] domainSeriesIds);
        [OperationContract]
        CDVRAdapterResponse GenerateCDVRSharedSecret(string sWSUserName, string sWSPassword, int adapterId);
        [OperationContract]
        AssetItemPriceResponse GetAssetPrices(string username, string password, string siteGuid, string couponCode, string countryCd2, string languageCode3, string deviceName, string clientIP, List<AssetFiles> assetFiles);
        [OperationContract]
        CDVRAdapterResponseList GetCDVRAdapters(string sWSUserName, string sWSPassword);
        [OperationContract]
        CollectionsPricesResponse GetCollectionsPrices(string sWSUserName, string sWSPassword, string[] sCollections, string sUserGUID, string sCountryCd2, string sLanguageCode3, string sDeviceName, string clientIP);
        [OperationContract]
        CollectionsPricesResponse GetCollectionsPricesST(string sWSUserName, string sWSPassword, string sCollectionsList, string sUserGUID, string sCountryCd2, string sLanguageCode3, string sDeviceName, string sClientIP);
        [OperationContract]
        CollectionsPricesResponse GetCollectionsPricesSTWithCoupon(string sWSUserName, string sWSPassword, string sCollectionsList, string sUserGUID, string sCouponCode, string sCountryCd2, string sLanguageCode3, string sDeviceName);
        [OperationContract]
        CollectionsPricesResponse GetCollectionsPricesWithCoupon(string sWSUserName, string sWSPassword, string[] sCollections, string sUserGUID, string sCouponCode, string sCountryCd2, string sLanguageCode3, string sDeviceName, string sClientIP);
        [OperationContract]
        CompensationResponse GetCompensation(string sWSUserName, string sWSPassword, long compensationId);
        [OperationContract]
        int GetCustomDataID(string sWSUserName, string sWSPassword, string sSiteGUID, double dPrice, string sCurrencyCode3, int assetID, string sPPVModuleCode, string sCampaignCode, string sCouponCode, string sPaymentMethod, string sUserIP, string sCountryCd2, string sLanguageCode3, string sDeviceName, int assetType, string sOverrideEndDate, string sPreviewModuleID);
        [OperationContract]
        Entitlements GetDomainEntitlements(string sWSUserName, string sWSPassword, int domainId, eTransactionType type, bool isExpired, int pageSize, int pageIndex, EntitlementOrderBy orderBy);
        [OperationContract]
        PermittedCollectionContainer[] GetDomainPermittedCollections(string sWSUserName, string sWSPassword, int nDomainID);
        [OperationContract]
        PermittedMediaContainerResponse GetDomainPermittedItems(string sWSUserName, string sWSPassword, int nDomainID);
        [OperationContract]
        PermittedSubscriptionContainer[] GetDomainPermittedSubscriptions(string sWSUserName, string sWSPassword, int nDomainID);
        [OperationContract]
        DomainQuotaResponse GetDomainQuota(string sWSUserName, string sWSPassword, string userID, long domainID);
        [OperationContract]
        DomainsBillingTransactionsResponse GetDomainsBillingHistory(string sWSUserName, string sWSPassword, int[] domainIDs, DateTime dStartDate, DateTime dEndDate);
        [OperationContract]
        SearchableRecording[] GetDomainSearchableRecordings(string sWSUserName, string sWSPassword, long domainId);
        [OperationContract]
        DomainServicesResponse GetDomainServices(string sWSUserName, string sWSPassword, int domainID);
        [OperationContract]
        DomainTransactionsHistoryResponse GetDomainTransactionsHistory(string sWSUserName, string sWSPassword, int domainID, DateTime dStartDate, DateTime dEndDate, int pageSize, int pageIndex, TransactionHistoryOrderBy orderBy);
        [OperationContract]
        EntitlementResponse GetEntitlement(string sWSUserName, string sWSPassword, string sMediaFileID, string sSiteGUID, bool bIsCoGuid, string sCOUNTRY_CODE, string sLANGUAGE_CODE, string sDEVICE_NAME, bool isRecording);
        [OperationContract]
        LicensedLinkResponse GetEPGLicensedLink(string sWSUserName, string sWSPassword, string sSiteGUID, int nMediaFileID, int nEPGItemID, DateTime startTime, string sBasicLink, string sUserIP, string sRefferer, string sCountryCd2, string sLanguageCode3, string sDeviceName, int nFormatType);
        [OperationContract]
        SeriesResponse GetFollowSeries(string sWSUserName, string sWSPassword, string userId, long domainId, SeriesRecordingOrderObj orderBy);
        [OperationContract]
        string GetGoogleSignature(string sWSUserName, string sWSPassword, int nCustomDataID);
        [OperationContract]
        string GetItemLeftViewLifeCycle(string sWSUserName, string sWSPassword, string sMediaFileID, string sSiteGUID, bool bIsCoGuid, string sCOUNTRY_CODE, string sLANGUAGE_CODE, string sDEVICE_NAME);
        [OperationContract]
        MediaFileItemPricesContainer[] GetItemsPrices(string sWSUserName, string sWSPassword, int[] nMediaFiles, string sUserGUID, bool bOnlyLowest, string sCountryCd2, string sLanguageCode3, string sDeviceName, string sClientIP);
        [OperationContract]
        MediaFileItemPricesContainer[] GetItemsPricesByIP(string sWSUserName, string sWSPassword, int[] nMediaFiles, string sUserGUID, bool bOnlyLowest, string sCountryCd2, string sLanguageCode3, string sDeviceName, string sClientIP);
        [OperationContract]
        MediaFileItemPricesContainer[] GetItemsPricesEx(string sWSUserName, string sWSPassword, WSInt32[] nMediaFiles, string sUserGUID, bool bOnlyLowest, string sCountryCd2, string sLanguageCode3, string sDeviceName);
        [OperationContract]
        MediaFileItemPricesContainer[] GetItemsPricesST(string sWSUserName, string sWSPassword, string sMediaFilesCommaSeperated, string sUserGUID, bool bOnlyLowest, string sCountryCd2, string sLanguageCode3, string sDeviceName, string sClientIP);
        [OperationContract]
        MediaFileItemPricesContainer[] GetItemsPricesSTByIP(string sWSUserName, string sWSPassword, string sMediaFilesCommaSeperated, string sUserGUID, bool bOnlyLowest, string sCountryCd2, string sLanguageCode3, string sDeviceName, string sClientIP);
        [OperationContract]
        MediaFileItemPricesContainerResponse GetItemsPricesWithCoupons(string sWSUserName, string sWSPassword, int[] nMediaFiles, string sUserGUID, string sCouponCode, bool bOnlyLowest, string sCountryCd2, string sLanguageCode3, string sDeviceName, string sClientIP);
        [OperationContract]
        MediaFileItemPricesContainer[] GetItemsPricesWithCouponsByIP(string sWSUserName, string sWSPassword, int[] nMediaFiles, string sUserGUID, string sCouponCode, bool bOnlyLowest, string sCountryCd2, string sLanguageCode3, string sDeviceName, string sClientIP);
        [OperationContract]
        MediaFileItemPricesContainer[] GetItemsPricesWithCouponsEx(string sWSUserName, string sWSPassword, WSInt32[] nMediaFiles, string sUserGUID, string sCouponCode, bool bOnlyLowest, string sCountryCd2, string sLanguageCode3, string sDeviceName, string sClientIP);
        [OperationContract]
        string GetLicensedLink(string sWSUserName, string sWSPassword, string sSiteGUID, int nMediaFileID, string sBasicLink, string sUserIP, string sRefferer, string sCountryCd2, string sLanguageCode3, string sDeviceName);
        [OperationContract]
        LicensedLinkResponse GetLicensedLinks(string sWSUserName, string sWSPassword, string sSiteGUID, int nMediaFileID, string sBasicLink, string sUserIP, string sRefferer, string sCountryCd2, string sLanguageCode3, string sDeviceName);
        [OperationContract]
        string GetLicensedLinkWithCoupon(string sWSUserName, string sWSPassword, string sSiteGUID, int nMediaFileID, string sBasicLink, string sUserIP, string sRefferer, string sCountryCd2, string sLanguageCode3, string sDeviceName, string couponCode);
        [OperationContract]
        NPVRResponse GetNPVRResponse(BaseNPVRCommand command);
        [OperationContract]
        PlaybackContextResponse GetPlaybackContext(string sWSUserName, string sWSPassword, string userId, string udid, string ip, string assetId, eAssetTypes assetType, List<long> fileIds, StreamerType? streamerType, string mediaProtocol, PlayContextType context, UrlType urlType);
        [OperationContract]
        PlayManifestResponse GetPlayManifest(string sWSUserName, string sWSPassword, string userId, string assetId, eAssetTypes assetType, long fileId, string ip, string udid, PlayContextType playContextType);
        [OperationContract]
        PrePaidPricesContainer[] GetPrePaidPrices(string sWSUserName, string sWSPassword, string[] sPrePaids, string sUserGUID, string sCouponCode, string sCountryCd2, string sLanguageCode3, string sDeviceName);
        [OperationContract]
        PrePaidPricesContainer[] GetPrePaidPricesST(string sWSUserName, string sWSPassword, string sPrePaidList, string sUserGUID, string sCouponCode, string sCountryCd2, string sLanguageCode3, string sDeviceName);
        [OperationContract]
        PurchaseSessionIdResponse GetPurchaseSessionID(string sWSUserName, string sWSPassword, string userId, double price, string currency, int contentId, string productId, string coupon, string userIP, string udid, eTransactionType transactionType, int previewModuleID);
        [OperationContract]
        Recording GetRecordingByDomainRecordingId(string sWSUserName, string sWSPassword, long domainID, long domainRecordingID);
        [OperationContract]
        Recording GetRecordingByID(string sWSUserName, string sWSPassword, long domainID, long domainRecordingID);
        [OperationContract]
        LicensedLinkResponse GetRecordingLicensedLink(string sWSUserName, string sWSPassword, string userId, int recordingId, string udid, string userIp, string fileType);
        [OperationContract]
        Recording GetRecordingStatus(string sWSUserName, string sWSPassword, long recordingId);
        [OperationContract]
        SubscriptionsPricesContainer[] GetSubscriptionsPrices(string sWSUserName, string sWSPassword, string[] sSubscriptions, string sUserGUID, string sCountryCd2, string sLanguageCode3, string sDeviceName, string sClientIP);
        [OperationContract]
        SubscriptionsPricesContainer[] GetSubscriptionsPricesByIP(string sWSUserName, string sWSPassword, string[] sSubscriptions, string sUserGUID, string sCountryCd2, string sLanguageCode3, string sDeviceName, string sClientIP);
        [OperationContract]
        SubscriptionsPricesContainer[] GetSubscriptionsPricesST(string sWSUserName, string sWSPassword, string sSubscriptionsList, string sUserGUID, string sCountryCd2, string sLanguageCode3, string sDeviceName, string sClientIP);
        [OperationContract]
        SubscriptionsPricesContainer[] GetSubscriptionsPricesSTByIP(string sWSUserName, string sWSPassword, string sSubscriptionsList, string sUserGUID, string sCountryCd2, string sLanguageCode3, string sDeviceName, string sClientIP);
        [OperationContract]
        SubscriptionsPricesContainer[] GetSubscriptionsPricesSTWithCoupon(string sWSUserName, string sWSPassword, string sSubscriptionsList, string sUserGUID, string sCouponCode, string sCountryCd2, string sLanguageCode3, string sDeviceName);
        [OperationContract]
        SubscriptionsPricesContainer[] GetSubscriptionsPricesSTWithCouponByIP(string sWSUserName, string sWSPassword, string sSubscriptionsList, string sUserGUID, string sCouponCode, string sCountryCd2, string sLanguageCode3, string sDeviceName, string sClientIP);
        [OperationContract]
        SubscriptionsPricesResponse GetSubscriptionsPricesWithCoupon(string sWSUserName, string sWSPassword, string[] sSubscriptions, string sUserGUID, string sCouponCode, string sCountryCd2, string sLanguageCode3, string sDeviceName, string sClientIP);
        [OperationContract]
        SubscriptionsPricesContainer[] GetSubscriptionsPricesWithCouponByIP(string sWSUserName, string sWSPassword, string[] sSubscriptions, string sUserGUID, string sCouponCode, string sCountryCd2, string sLanguageCode3, string sDeviceName, string sClientIP);
        [OperationContract]
        BillingTransactions GetUserBillingHistory(string sWSUserName, string sWSPassword, string sSiteGUID, int nStartIndex, int nNumberOfItems, TransactionHistoryOrderBy orderBy);
        [OperationContract]
        UserBundlesResponse GetUserBundles(string sWSUserName, string sWSPassword, int domainID, int[] fileTypeIDs);
        [OperationContract]
        UserCAStatus GetUserCAStatus(string sWSUserName, string sWSPassword, string sSiteGUID);
        [OperationContract]
        Entitlements GetUserEntitlements(string sWSUserName, string sWSPassword, string sSiteGUID, eTransactionType type, bool isExpired, int pageSize, int pageIndex, EntitlementOrderBy orderBy);
        [OperationContract]
        PermittedCollectionContainer[] GetUserExpiredCollections(string sWSUserName, string sWSPassword, string sSiteGUID, int numOfItems);
        [OperationContract]
        PermittedMediaContainer[] GetUserExpiredItems(string sWSUserName, string sWSPassword, string sSiteGUID, int numOfItems);
        [OperationContract]
        PermittedSubscriptionContainer[] GetUserExpiredSubscriptions(string sWSUserName, string sWSPassword, string sSiteGUID, int numOfItems);
        [OperationContract]
        PermittedCollectionContainer[] GetUserPermittedCollections(string sWSUserName, string sWSPassword, string sSiteGUID);
        [OperationContract]
        PermittedMediaContainer[] GetUserPermittedItems(string sWSUserName, string sWSPassword, string sSiteGUID);
        [OperationContract]
        PermittedSubscriptionContainer[] GetUserPermittedSubscriptions(string sWSUserName, string sWSPassword, string sSiteGUID);
        [OperationContract]
        PrePaidHistoryResponse GetUserPrePaidHistory(string sWSUserName, string sWSPassword, string sSiteGUID, int nNumberOfItems);
        [OperationContract]
        UserPrePaidContainer GetUserPrePaidStatus(string sWSUserName, string sWSPassword, string sSiteGUID, string sCurrencyCode3);
        [OperationContract]
        UserPurhcasedAssetsResponse GetUserPurchasedAssets(string sWSUserName, string sWSPassword, int domainID, int[] fileTypeIDs);
        [OperationContract]
        UserBillingTransactionsResponse[] GetUsersBillingHistory(string sWSUserName, string sWSPassword, string[] arrSiteGUIDs, DateTime dStartDate, DateTime dEndDate);
        [OperationContract]
        Entitlements GetUserSubscriptions(string sWSUserName, string sWSPassword, string sSiteGUID);
        [OperationContract]
        bool GiftCardReminder(string sWSUserName, string sWSPassword, string siteguid, long purchaseId, string billingGuid, long endDate);
        [OperationContract]
        Status GrantEntitlements(string sWSUserName, string sWSPassword, string siteguid, long housholdId, int contentId, int productId, eTransactionType transactionType, string userIp, string deviceName, bool history);
        [OperationContract]
        bool HandleDomainQuotaByRecording(HandleDomainQuataByRecordingTask expiredRecording);
        [OperationContract]
        bool HandleFirstFollowerRecording(string sWSUserName, string sWSPassword, string userId, long domainId, string channelId, string seriesId, int seasonNumber);
        [OperationContract]
        bool HandleRecordingsLifetime();
        [OperationContract]
        bool HandleRecordingsScheduledTasks();
        [OperationContract]
        Status HandleUserTask(string sWSUserName, string sWSPassword, int domainId, string userId, UserTaskType actionType);
        [OperationContract]
        BillingResponse InApp_ChargeUserForMediaFile(string sWSUserName, string sWSPassword, string sSiteGUID, double dPrice, string sCurrencyCode3, string sProductCode, string sPPVModuleCode, string sCouponCode, string sUserIP, string sExtraParameters, string sCountryCd2, string sLanguageCode3, string sDeviceName, string ReceiptData);
        [OperationContract]
        BillingResponse InApp_ChargeUserForSubscription(string sWSUserName, string sWSPassword, string sSiteGUID, double dPrice, string sCurrencyCode, string sProductCode, string sUserIP, string sExtraParameters, string sCountryCd2, string sLanguageCode3, string sDeviceName, string ReceiptData);
        [OperationContract]
        Status IngestRecording(string sWSUserName, string sWSPassword, long[] epgs, eAction action);
        [OperationContract]
        CDVRAdapterResponse InsertCDVRAdapter(string sWSUserName, string sWSPassword, CDVRAdapter adapter);
        [OperationContract]
        bool IsPermittedItem(string sWSUserName, string sWSPassword, string sSiteGUID, int mediaID);
        [OperationContract]
        bool IsPermittedSubscription(string sWSUserName, string sWSPassword, string sSiteGUID, int subID, ref string reason);
        [OperationContract]
        PrePaidResponse PP_ChargeUserForMediaFile(string sWSUserName, string sWSPassword, string sSiteGUID, double dPrice, string sCurrencyCode3, int nMediaFileID, string sPPVModuleCode, string sCouponCode, string sUserIP, string sExtraParameters, string sCountryCd2, string sLanguageCode3, string sDeviceName);
        [OperationContract]
        PrePaidResponse PP_ChargeUserForSubscription(string sWSUserName, string sWSPassword, string sSiteGUID, double dPrice, string sCurrencyCode3, string sSubscriptionCode, string sCouponCode, string sUserIP, string sExtraParameters, string sCountryCd2, string sLanguageCode3, string sDeviceName);
        [OperationContract]
        TransactionResponse ProcessReceipt(string sWSUserName, string sWSPassword, string siteguid, long household, int contentId, int productId, eTransactionType transactionType, string userIp, string deviceName, string purchaseToken, string paymentGatewayName);
        [OperationContract]
        Recording ProtectRecord(string sWSUserName, string sWSPassword, string userID, long recordID);
        [OperationContract]
        TransactionResponse Purchase(string sWSUserName, string sWSPassword, string siteguid, long householdId, double price, string currency, int contentId, int productId, eTransactionType transactionType, string coupon, string userIp, string deviceName, int paymentGatewayId, int paymentMethodId, string adapterData);
        [OperationContract]
        BillingResponse PU_GetPPVPopupPaymentMethodURL(string sWSUserName, string sWSPassword, string sSiteGUID, double dPrice, string sCurrencyCode3, int nMediaFileID, string sPPVModuleCode, string sCouponCode, string sPaymentMethod, string sExtraParameters, string sCountryCd2, string sLanguageCode3, string sDeviceName);
        [OperationContract]
        BillingResponse PU_GetSubscriptionPopupPaymentMethodURL(string sWSUserName, string sWSPassword, string sSiteGUID, double dPrice, string sCurrencyCode3, string sSubscriptionCode, string sCouponCode, string sPaymentMethod, string sExtraParameters, string sCountryCd2, string sLanguageCode3, string sDeviceName);
        [OperationContract]
        Recording QueryRecords(string sWSUserName, string sWSPassword, string userID, long epgId);
        [OperationContract]
        Status ReconcileEntitlements(string sWSUserName, string sWSPassword, string userId);
        [OperationContract]
        Recording Record(string sWSUserName, string sWSPassword, string userID, long epgID, RecordingType recordingType);
        [OperationContract]
        Recording RecordRetry(string sWSUserName, string sWSPassword, long recordingId);
        [OperationContract]
        SeriesRecording RecordSeasonOrSeries(string sWSUserName, string sWSPassword, string userID, long epgID, RecordingType recordingType);
        [OperationContract]
        Status RecordTransaction(string sWSUserName, string sWSPassword, string userId, long householdId, int state, string paymentGatewayReferenceID, string paymentGatewayResponseCode, int customDataId, double price, string currency, int contentId, int productId, eTransactionType transactionType, string paymentDetails, string paymentMethod, int paymentGatewayId, string paymentMethodExternalID);
        [OperationContract]
        Status RecoverRecordingMessages(string sWSUserName, string sWSPassword);
        [OperationContract]
        Status RemoveHouseholdEntitlements(string sWSUserName, string sWSPassword, int householdId);
        [OperationContract]
        Status RemovePaymentMethodHouseholdPaymentGateway(string sWSUserName, string sWSPassword, int paymentGatewayID, string siteGuid, int householdId, int paymentMethodId, bool force);
        [OperationContract]
        bool Renew(string sWSUserName, string sWSPassword, string siteguid, long purchaseId, string billingGuid, long endDate);
        [OperationContract]
        bool RenewCancledSubscription(string sWSUserName, string sWSPassword, string sSiteGUID, string sSubscriptionCode, int nSubscriptionPurchaseID);
        [OperationContract]
        RecordingResponse SearchDomainRecordings(string sWSUserName, string sWSPassword, string userID, long domainID, TstvRecordingStatus[] recordingStatuses, string filter, int pageIndex, int pageSize, OrderObj orderBy, bool shouldIgnorePaging);
        [OperationContract]
        CDVRAdapterResponse SendCDVRAdapterConfiguration(string sWSUserName, string sWSPassword, int adapterID);
        [OperationContract]
        CDVRAdapterResponse SetCDVRAdapter(string sWSUserName, string sWSPassword, CDVRAdapter adapter);
        [OperationContract]
        BillingResponse SMS_ChargeUserForMediaFile(string sWSUserName, string sWSPassword, string sSiteGUID, string sCellPhone, double dPrice, string sCurrencyCode3, int nMediaFileID, string sPPVModuleCode, string sCouponCode, string sExtraParameters, string sCountryCd2, string sLanguageCode3, string sDeviceName);
        [OperationContract]
        BillingResponse SMS_ChargeUserForSubscription(string sWSUserName, string sWSPassword, string sSiteGUID, string sCellPhone, double dPrice, string sCurrencyCode3, string sSubscriptionCode, string sCouponCode, string sExtraParameters, string sCountryCd2, string sLanguageCode3, string sDeviceName);
        [OperationContract]
        BillingResponse SMS_CheckCodeForMediaFile(string sWSUserName, string sWSPassword, string sSiteGUID, string sCellPhone, string sSMSCode, int nMediaFileID, string sCountryCd2, string sLanguageCode3, string sDeviceName);
        [OperationContract]
        BillingResponse SMS_CheckCodeForSubscription(string sWSUserName, string sWSPassword, string sSiteGUID, string sCellPhone, string sSMSCode, string sSubscriptionCode, string sCountryCd2, string sLanguageCode3, string sDeviceName);
        [OperationContract]
        Status SwapSubscription(string sWSUserName, string sWSPassword, string userId, int oldSubscription, int newSubscription, string ip, string udid, bool history);
        [OperationContract]
        Entitlements UpdateEntitlement(string sWSUserName, string sWSPassword, int domainID, Entitlement entitlement);
        [OperationContract]
        Status UpdatePendingTransaction(string sWSUserName, string sWSPassword, string paymentGatewayId, int adapterTransactionState, string externalTransactionId, string externalStatus, string externalMessage, int failReason, string signature);
        [OperationContract]
        Status UpdateRecordedTransaction(string sWSUserName, string sWSPassword, long householdId, string paymentGatewayReferenceID, string paymentDetails, string paymentMethod, int paymentGatewayId, string paymentMethodExternalID);
        [OperationContract]
        Status WaiverTransaction(string sWSUserName, string sWSPassword, string sSiteGuid, int nAssetID, eTransactionType transactionType);
    }
}