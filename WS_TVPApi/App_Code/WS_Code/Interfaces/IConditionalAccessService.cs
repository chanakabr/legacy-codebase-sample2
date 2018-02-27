using System;
using System.Collections.Generic;
using System.ServiceModel;
using TVPApi;
using TVPApiModule.Objects.Responses;
using TVPPro.SiteManager.TvinciPlatform.ConditionalAccess;

namespace TVPApiServices
{
    [ServiceContract]
    public interface IConditionalAccessService
    {
        [OperationContract]
        TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.CampaignActionInfo ActivateCampaignWithInfo(InitializationObject initObj, long campID, string hashCode, int mediaID, string mediaLink,
                                                                                                         string senderEmail, string senderName, CampaignActionResult status, VoucherReceipentInfo[] voucherReceipents);
        [OperationContract]
        int AD_GetCustomDataID(InitializationObject initObj, double price, string currencyCode3, int assetId, string ppvModuleCode, string campaignCode, string couponCode, string paymentMethod, string userIp, string countryCd2, string languageCode3, string deviceName, int assetType);

        [OperationContract]
        bool ActivateCampaign(InitializationObject initObj, int campaignID, string hashCode, int mediaID, string mediaLink, string senderEmail, string senderName,
                                                           CampaignActionResult status, VoucherReceipentInfo[] voucherReceipents);

        [OperationContract]
        MediaFileItemPricesContainer[] GetItemsPricesWithCoupons(InitializationObject initObj, int[] nMediaFiles, string sUserGUID, string sCouponCode, bool bOnlyLowest, string sCountryCd2, string sLanguageCode3, string sDeviceName);

        [OperationContract]
        SubscriptionsPricesContainer[] GetSubscriptionsPricesWithCoupon(InitializationObject initObj, string[] sSubscriptions, string sUserGUID, string sCouponCode, string sCountryCd2, string sLanguageCode3, string sDeviceName);

        [OperationContract]
        bool IsPermittedItem(InitializationObject initObj, int mediaId);

        [OperationContract]
        bool IsPermittedSubscription(InitializationObject initObj, int subId);

        [OperationContract]
        BillingResponse InApp_ChargeUserForMediaFile(InitializationObject initObj, double price, string currency, string productCode, string ppvModuleCode, string receipt);

        [OperationContract]
        BillingResponse CC_ChargeUserForPrePaid(InitializationObject initObj, double price, string currency, string productCode, string ppvModuleCode);

        [OperationContract]
        string GetEPGLicensedLink(InitializationObject initObj, int mediaFileID, int EPGItemID, DateTime startTime, string basicLink, string userIP, string refferer, string countryCd2, string languageCode3, string deviceName, int formatType);

        [OperationContract]
        UserBillingTransactionsResponse[] GetUsersBillingHistory(InitializationObject initObj, string[] siteGuids, DateTime startDate, DateTime endDate);

        [OperationContract]
        DomainBillingTransactionsResponse[] GetDomainsBillingHistory(InitializationObject initObj, int[] domainIDs, DateTime startDate, DateTime endDate);

        [OperationContract]
        PermittedMediaContainer[] GetDomainPermittedItems(InitializationObject initObj);

        [OperationContract]
        PermittedSubscriptionContainer[] GetDomainPermittedSubscriptions(InitializationObject initObj);

        [OperationContract]
        string ChargeUserForMediaFileUsingCC(InitializationObject initObj, double iPrice, string sCurrency, int iFileID, string sPPVModuleCode, string sUserIP, string sCoupon, string sPaymentMethodID, string sEncryptedCVV);

        [OperationContract]
        string ChargeUserForSubscriptionByPaymentMethod(InitializationObject initObj, double iPrice, string sCurrency, string sSubscriptionID, string sCouponCode, string sExtraParameters, string sPaymentMethodID, string sEncryptedCVV);

        [OperationContract]
        ChangeSubscriptionStatus ChangeSubscription(InitializationObject initObj, string sSiteGuid, int nOldSubscription, int nNewSubscription);

        [OperationContract]
        string ChargeUserForMediaSubscriptionUsingCC(InitializationObject initObj, double iPrice, string sCurrency, string sSubscriptionID, string sCouponCode, string sUserIP, string sExtraParameters, string sUDID, string sPaymentMethodID, string sEncryptedCVV);

        [OperationContract]
        int CreatePurchaseToken(InitializationObject initObj, double price, string currencyCode3, int assetId, string ppvModuleCode, string campaignCode, string couponCode, string paymentMethod, string userIp, string countryCd2, string languageCode3, string deviceName, int assetType, string overrideEndDate, string previewModuleID);

        [OperationContract]
        CollectionsPricesContainer[] GetCollectionsPrices(InitializationObject initObj, string[] collections, string userGuid, string countryCode2, string languageCode3);

        [OperationContract]
        CollectionsPricesContainer[] GetCollectionsPricesWithCoupon(InitializationObject initObj, string[] collections, string userGuid, string countryCode2, string languageCode3, string couponCode);

        [OperationContract]
        PermittedCollectionContainer[] GetUserPermittedCollections(InitializationObject initObj, string siteGuid);

        [OperationContract]
        PermittedCollectionContainer[] GetDomainPermittedCollections(InitializationObject initObj);

        [OperationContract]
        BillingResponse ChargeUserForCollection(InitializationObject initObj, double price, string currencyCode3, string collectionCode, string couponCode, string extraParameters, string countryCode2, string languageCode3, string paymentMethodID, string encryptedCvv);

        [OperationContract]
        string DummyChargeUserForCollection(InitializationObject initObj, double price, string currency, string collectionCode, string couponCode, string userIP, string extraParameters, string countryCode2, string languageCode3);

        [OperationContract]
        bool CancelTransaction(InitializationObject initObj, string siteGuid, int assetId, eTransactionType transactionType, bool bIsForce);

        [OperationContract]
        bool WaiverTransaction(InitializationObject initObj, string siteGuid, int assetId, eTransactionType transactionType);

        [OperationContract]
        PermittedCollectionContainer[] GetUserExpiredCollections(InitializationObject initObj, string siteGuid, int numOfItems);

        [OperationContract]
        TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.LicensedLinkResponse GetLicensedLinks(InitializationObject initObj, int mediaFileID, string baseLink);

        [OperationContract]
        RecordResponse RecordAsset(InitializationObject initObj, string epgId, int? version);

        [OperationContract]
        NPVRResponse CancelAssetRecording(InitializationObject initObj, string recordingId, int? version);

        [OperationContract]
        NPVRResponse DeleteAssetRecording(InitializationObject initObj, string recordingId, int? version);

        [OperationContract]
        QuotaResponse GetNPVRQuota(InitializationObject initObj, int? version);

        //[OperationContract]
        //NPVRResponse RecordSeriesByName(InitializationObject initObj, string assetId);

        [OperationContract]
        NPVRResponse RecordSeriesByProgramId(InitializationObject initObj, string assetId, int? version);

        [OperationContract]
        NPVRResponse DeleteSeriesRecording(InitializationObject initObj, string seriesRecordingId, int? version);

        [OperationContract]
        NPVRResponse CancelSeriesRecording(InitializationObject initObj, string seriesRecordingId, int? version);

        [OperationContract]
        NPVRResponse SetAssetProtectionStatus(InitializationObject initObj, string recordingId, bool isProtect, int? version);

        [OperationContract]
        LicensedLinkNPVRResponse GetNPVRLicensedLink(InitializationObject initObj, string recordingId, DateTime startTime, int mediaFileID, string basicLink,
            string referrer, string couponCode);

        [OperationContract]
        ClientResponseStatus CancelServiceNow(InitializationObject initObj, int domainID, int serviceID, eTransactionType serviceType, bool forceCancel);

        [OperationContract]
        bool CancelSubscription(InitializationObject initObj, string sSubscriptionID, int sSubscriptionPurchaseID);

        [OperationContract]
        ClientResponseStatus CancelSubscriptionRenewal(InitializationObject initObj, int domainId, string sSubscriptionID);

        [OperationContract]
        TVPApiModule.Objects.Responses.LicensedLinkResponse GetEPGLicensedData(InitializationObject initObj, int mediaFileID, int EPGItemID, DateTime startTime, string basicLink, string userIP, string refferer, string countryCd2, string languageCode3, int formatType);

        [OperationContract]
        ServicesResponse GetDomainServices(InitializationObject initObj, int domainID);

        [OperationContract]
        TVPApiModule.Objects.Responses.ConditionalAccess.TransactionResponse Purchase(InitializationObject initObj, string user_id, double price, string currency, int content_id, int product_id,
            string product_type, string coupon, int payment_gateway_id, int payment_method_id);

        [OperationContract]
        ClientResponseStatus GrantEntitlements(InitializationObject initObj, string user_id, int content_id, int product_id, string product_type, bool history);

        [OperationContract]
        NPVRResponse RecordingWatchStatus(InitializationObject initObj, int recordingId, int alreadyWatched);

        [OperationContract]
        NPVRResponse RecordSeriesBySeriesId(InitializationObject initObj, string seriesId, int seasonNumber, int seasonSeed, int episodeSeed, int channelId,
            List<string> lookupCriteria);

        [OperationContract]
        NPVRResponse DeleteRecordingsBy(InitializationObject initObj, string bySeriesId, string bySeasonNumber, string byChannelId,
            List<string> byStatus);
    }
}