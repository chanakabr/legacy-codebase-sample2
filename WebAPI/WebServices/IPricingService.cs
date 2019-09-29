using System.Collections.Generic;
using System.ServiceModel;
using ApiObjects;
using ApiObjects.Pricing;
using ApiObjects.Response;
using Core.Pricing;

namespace WebAPI.WebServices
{
    [ServiceContract(Namespace= "http://pricing.tvinci.com/")]
    public interface IPricingService
    {
        [OperationContract]
        BusinessModuleResponse DeleteMPP(int nGroupID, string multiPricePlan);
        [OperationContract]
        BusinessModuleResponse DeletePPV(int nGroupID, string ppv);
        [OperationContract]
        BusinessModuleResponse DeletePricePlan(int nGroupID, string pricePlan);
        [OperationContract]
        bool DoesMediaBelongToSubscription(string sWSUserName, string sWSPassword, string sSubscriptionCode, int nMediaID);
        [OperationContract]
        List<Coupon> GenerateCoupons(string sWSUserName, string sWSPassword, int numberOfCoupons, long couponGroupId, bool useLetters = true, bool useNumbers = true, bool useSpecialCharacters = true);
        [OperationContract]
        List<Coupon> GeneratePublicCoupons(string sWSUserName, string sWSPassword, long couponGroupId, string code);
        [OperationContract]
        Campaign GetCampaignData(string sWSUserName, string sWSPassword, long nCampaignID);
        [OperationContract]
        Campaign GetCampaignsByHash(string sWSUserName, string sWSPassword, string hashCode);
        [OperationContract]
        Campaign[] GetCampaignsByType(string sWSUserName, string sWSPassword, CampaignTrigger triggerType, string sCountryCd2, string sLanguageCode3, string sDeviceName, bool bGetAlsoUnActive);
        [OperationContract]
        Collection GetCollectionData(string sWSUserName, string sWSPassword, string sCollectionCode, string sCountryCd2, string sLanguageCode3, string sDeviceName, bool bGetAlsoUnActive);
        [OperationContract]
        CollectionsResponse GetCollectionsData(string sWSUserName, string sWSPassword, string[] oCollCodes, string sCountryCd2, string sLanguageCode3, string sDeviceName);
        [OperationContract]
        CouponsGroup GetCouponGroupData(string sWSUserName, string sWSPassword, string sCouponGroupID);
        [OperationContract]
        CouponsGroup[] GetCouponGroupListForAdmin(string sWSUserName, string sWSPassword);
        [OperationContract]
        CouponDataResponse GetCouponStatus(string sWSUserName, string sWSPassword, string sCouponCode);
        [OperationContract]
        Currency GetCurrencyValues(string sWSUserName, string sWSPassword, string sCurrencyCode3);
        [OperationContract]
        DiscountModule GetDiscountCodeData(string sWSUserName, string sWSPassword, string sDiscountCode);
        [OperationContract]
        DiscountModule[] GetDiscountsModuleListForAdmin(string sWSUserName, string sWSPassword);
        [OperationContract]
        Subscription[] GetIndexedSubscriptionsContainingMedia(string sWSUserName, string sWSPassword, int nMediaID, int nFileTypeID, int count);
        [OperationContract]
        Campaign[] GetMediaCampaigns(string sWSUserName, string sWSPassword, int nMediaID, string sCountryCd2, string sLanguageCode3, string sDeviceName, bool bGetAlsoUnActive);
        [OperationContract]
        UsageModule GetOfflineUsageModule(string sWSUserName, string sWSPassword, string sCountryCd2, string sLanguageCode3, string sDeviceName);
        [OperationContract]
        PPVModule GetPPVModuleData(string sWSUserName, string sWSPassword, string sPPVCode, string sCountryCd2, string sLanguageCode3, string sDeviceName);
        [OperationContract]
        PPVModule[] GetPPVModuleList(string sWSUserName, string sWSPassword, string sCountryCd2, string sLanguageCode3, string sDeviceName);
        [OperationContract]
        PPVModuleContainer[] GetPPVModuleListForAdmin(string sWSUserName, string sWSPassword, int nMediaFileID, string sCountryCd2, string sLanguageCode3, string sDeviceName);
        [OperationContract]
        MediaFilePPVModule[] GetPPVModuleListForMediaFiles(string sWSUserName, string sWSPassword, int[] nMediaFileIDs, string sCountryCd2, string sLanguageCode3, string sDeviceName);
        [OperationContract]
        MediaFilePPVModule[] GetPPVModuleListForMediaFilesST(string sWSUserName, string sWSPassword, string sMediaFileIDsCommaSeperated, string sCountryCd2, string sLanguageCode3, string sDeviceName);
        [OperationContract]
        MediaFilePPVContainer[] GetPPVModuleListForMediaFilesWithExpiry(string sWSUserName, string sWSPassword, int[] nMediaFileIDs, string sCountryCd2, string sLanguageCode3, string sDeviceName);
        [OperationContract]
        PPVModuleDataResponse GetPPVModuleResponse(string sWSUserName, string sWSPassword, string sPPVCode, string sCountryCd2, string sLanguageCode3, string sDeviceName);
        [OperationContract]
        PPVModule[] GetPPVModulesByProductCodes(string sWSUserName, string sWSPassword, string[] productCodes);
        [OperationContract]
        PPVModuleResponse GetPPVModulesData(string sWSUserName, string sWSPassword, string[] sPPVCode, string sCountryCd2, string sLanguageCode3, string sDeviceName);
        [OperationContract]
        PPVModule[] GetPPVModuleShrinkList(string sWSUserName, string sWSPassword, string sCountryCd2, string sLanguageCode3, string sDeviceName);
        [OperationContract]
        PrePaidModule GetPrePaidModuleData(string sWSUserName, string sWSPassword, int nPrePaidCode, string sCountryCd2, string sLanguageCode3, string sDeviceName);
        [OperationContract]
        PreviewModule GetPreviewModuleByID(string sWSUserName, string sWSPassword, long lPreviewModuleID);
        [OperationContract]
        PreviewModule[] GetPreviewModulesArrayByGroupIDForAdmin(string sWSUserName, string sWSPassword);
        [OperationContract]
        PriceCode GetPriceCodeData(string sWSUserName, string sWSPassword, string sPriceCode, string sCountryCd2, string sLanguageCode3, string sDeviceName);
        [OperationContract]
        PriceCode[] GetPriceCodeList(string sWSUserName, string sWSPassword, string sCountryCd2, string sLanguageCode3, string sDeviceName);
        [OperationContract]
        Subscription GetSubscriptionData(string sWSUserName, string sWSPassword, string sSubscriptionCode, string sCountryCd2, string sLanguageCode3, string sDeviceName, bool bGetAlsoUnActive);
        [OperationContract]
        Subscription GetSubscriptionDataByProductCode(string sWSUserName, string sWSPassword, string sProductCode, string sCountryCd2, string sLanguageCode3, string sDeviceName, bool bGetAlsoUnActive);
        [OperationContract]
        IdsResponse GetSubscriptionIDsContainingMediaFile(string sWSUserName, string sWSPassword, int nMediaID, int nMediaFileID);
        [OperationContract]
        int[] GetSubscriptionMediaList(string sWSUserName, string sWSPassword, string sSubscriptionCode, int nFileTypeID, string sDevice);
        [OperationContract]
        List<int> GetSubscriptionMediaList2(string sWSUserName, string sWSPassword, string sSubscriptionCode, int nFileTypeID, string sDevice);
        [OperationContract]
        SubscriptionsResponse GetSubscriptions(string sWSUsername, string sWSPassword, string[] oSubCodes, string sCountryCd2, string sLanguageCode3, string sDeviceName, SubscriptionOrderBy orderBy = SubscriptionOrderBy.StartDateAsc);
        [OperationContract]
        Subscription[] GetSubscriptionsByProductCodes(string sWSUserName, string sWSPassword, List<string> productCodes);
        [OperationContract]
        Subscription[] GetSubscriptionsContainingMedia(string sWSUserName, string sWSPassword, int nMediaID, int nFileTypeID);
        [OperationContract]
        Subscription[] GetSubscriptionsContainingMediaFile(string sWSUserName, string sWSPassword, int nMediaID, int nMediaFileID);
        [OperationContract]
        Subscription[] GetSubscriptionsContainingMediaShrinked(string sWSUserName, string sWSPassword, int nMediaID, int nFileTypeID);
        [OperationContract]
        string GetSubscriptionsContainingMediaSTR(string sWSUserName, string sWSPassword, int nMediaID, int nFileTypeID);
        [OperationContract]
        Subscription[] GetSubscriptionsContainingUserTypes(string sWSUserName, string sWSPassword, string sCountryCd2, string sLanguageCode3, string sDeviceName, int nIsActive, int[] userTypesIDs);
        [OperationContract]
        SubscriptionsResponse GetSubscriptionsData(string sWSUsername, string sWSPassword, string[] oSubCodes, string sCountryCd2, string sLanguageCode3, string sDeviceName);
        [OperationContract]
        Subscription[] GetSubscriptionsList(string sWSUserName, string sWSPassword, string sCountryCd2, string sLanguageCode3, string sDeviceName);
        [OperationContract]
        Subscription[] GetSubscriptionsShrinkList(string sWSUserName, string sWSPassword, string sCountryCd2, string sLanguageCode3, string sDeviceName);
        [OperationContract]
        UsageModule GetUsageModule(string sWSUserName, string sWSPassword, string sAssetCode, eTransactionType transactionType);
        [OperationContract]
        UsageModule GetUsageModuleData(string sWSUserName, string sWSPassword, string sUsageModuleCode, string sCountryCd2, string sLanguageCode3, string sDeviceName);
        [OperationContract]
        UsageModule[] GetUsageModuleList(string sWSUserName, string sWSPassword, string sCountryCd2, string sLanguageCode3, string sDeviceName);
        [OperationContract]
        CouponsGroup[] GetVoucherGroupList(string sWSUserName, string sWSPassword);
        [OperationContract]
        BusinessModuleResponse InsertMPP(int nGroupID, IngestMultiPricePlan multiPricePlan);
        [OperationContract]
        BusinessModuleResponse InsertPPV(int nGroupID, IngestPPV ppv);
        [OperationContract]
        BusinessModuleResponse InsertPricePlan(int nGroupID, IngestPricePlan pricePlan);
        [OperationContract]
        CouponsStatus SetCouponUsed(string sWSUserName, string sWSPassword, string sCouponCode, string sSiteGUID);
        [OperationContract]
        CouponsStatus SetCouponUses(string sWSUserName, string sWSPassword, string sCouponCode, string sSiteGUID, int nMediaFileID, int nSubCode, int nCollectionCode, int nPrePaidCode);
        [OperationContract]
        BusinessModuleResponse test(string sWSUserName, string sWSPassword, string name);
        [OperationContract]
        BusinessModuleResponse UpdateMPP(int nGroupID, IngestMultiPricePlan multiPricePlan);
        [OperationContract]
        BusinessModuleResponse UpdatePPV(int nGroupID, IngestPPV ppv);
        [OperationContract]
        BusinessModuleResponse UpdatePricePlan(int nGroupID, IngestPricePlan pricePlan);
        [OperationContract]
        PPVModule ValidatePPVModuleForMediaFile(string sWSUserName, string sWSPassword, int mediaFileID, long ppvModuleCode);
    }
}