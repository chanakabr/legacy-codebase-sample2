using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using TVPApi;
using Tvinci.Data.TVMDataLoader.Protocols.MediaMark;
using TVPPro.SiteManager.TvinciPlatform.ConditionalAccess;
using TVPApiModule.Objects;
using TVPPro.SiteManager.Context;

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
        BillingResponse InApp_ChargeUserForMediaFile(InitializationObject initObj, double price, string currencyCode3, int productCode, string ppvModuleCode, string receiptData);
    }
}