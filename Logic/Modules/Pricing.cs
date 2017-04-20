using ApiObjects;
using ApiObjects.Response;
using KLogMonitor;
using Core.Pricing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using ApiObjects.Pricing;

namespace Core.Pricing
{
    public class Module
    {
        public static Currency GetCurrencyValues(int nGroupID, string sCurrencyCode3)
        {
            Currency t = new Currency();
            t.InitializeByCode3(sCurrencyCode3);
            return t;
        }

        
        public static Subscription[] GetSubscriptionsList(int nGroupID, string sCountryCd2, string sLanguageCode3, string sDeviceName)
        {
            BaseSubscription t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                return (new SubscriptionCacheWrapper(t)).GetSubscriptionsList(sCountryCd2, sLanguageCode3, sDeviceName);
            }
            else
            {
                return null;
            }
        }

        
        public static Subscription[] GetSubscriptionsContainingUserTypes(int nGroupID, string sCountryCd2, string sLanguageCode3, string sDeviceName, int nIsActive, int[] userTypesIDs)
        {
            BaseSubscription t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                return (new SubscriptionCacheWrapper(t)).GetSubscriptionsList(sCountryCd2, sLanguageCode3, sDeviceName, nIsActive, userTypesIDs);
            }
            else
            {
                return null;
            }
        }


        
        public static Subscription[] GetSubscriptionsContainingMedia(int nGroupID, Int32 nMediaID, Int32 nFileTypeID)
        {
            BaseSubscription t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                return (new SubscriptionCacheWrapper(t)).GetSubscriptionsContainingMedia(nMediaID, nFileTypeID);
            }
            else
            {
                return null;
            }
        }

        
        public static string GetSubscriptionsContainingMediaSTR(int nGroupID, Int32 nMediaID, Int32 nFileTypeID)
        {
            BaseSubscription t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                return (new SubscriptionCacheWrapper(t)).GetSubscriptionsContainingMediaSTR(nMediaID, nFileTypeID, true);
            }
            else
            {
                return null;
            }
        }

        
        public static Subscription[] GetIndexedSubscriptionsContainingMedia(int nGroupID, Int32 nMediaID, Int32 nFileTypeID, int count)
        {
            BaseSubscription t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                return (new SubscriptionCacheWrapper(t)).GetSubscriptionsContainingMedia(nMediaID, nFileTypeID, false, count);
            }
            else
            {
                return null;
            }
        }

        
        public static Subscription[] GetSubscriptionsContainingMediaShrinked(int nGroupID, Int32 nMediaID, Int32 nFileTypeID)
        {
            BaseSubscription t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                return (new SubscriptionCacheWrapper(t)).GetSubscriptionsContainingMedia(nMediaID, nFileTypeID, true);
            }
            else
            {
                return null;
            }
        }

        
        public static Subscription[] GetSubscriptionsContainingMediaFile(int nGroupID, Int32 nMediaID, Int32 nMediaFileID)
        {
            BaseSubscription t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                return (new SubscriptionCacheWrapper(t)).GetSubscriptionsContainingMediaFile(nMediaID, nMediaFileID);
            }
            else
            {
                return null;
            }
        }


        
        public static ApiObjects.Response.IdsResponse GetSubscriptionIDsContainingMediaFile(int nGroupID, Int32 nMediaID, Int32 nMediaFileID)
        {
            BaseSubscription t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                return (new SubscriptionCacheWrapper(t)).GetSubscriptionIDsContainingMediaFile(nMediaID, nMediaFileID);
            }
            else
            {
                return null;
            }
        }

        
        public static Subscription[] GetSubscriptionsShrinkList(int nGroupID, string sCountryCd2, string sLanguageCode3, string sDeviceName)
        {
            BaseSubscription t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                return (new SubscriptionCacheWrapper(t)).GetSubscriptionsShrinkList(sCountryCd2, sLanguageCode3, sDeviceName);
            }
            else
            {
                return null;
            }
        }

        
        public static Campaign[] GetMediaCampaigns(int nGroupID, int nMediaID
            , string sCountryCd2, string sLanguageCode3, string sDeviceName, bool bGetAlsoUnActive)
        {

            BaseCampaign t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                return t.GetMediaCampaigns(nMediaID);
            }
            else
            {
                return null;
            }
        }

        
        public static Campaign[] GetCampaignsByType(int nGroupID, CampaignTrigger triggerType
            , string sCountryCd2, string sLanguageCode3, string sDeviceName, bool bGetAlsoUnActive)
        {

            BaseCampaign t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                return t.GetCampaignsByType(triggerType);
            }
            else
            {
                return null;
            }
        }

        
        public static Campaign GetCampaignsByHash(int nGroupID, string hashCode)
        {

            BaseCampaign t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                return t.GetCampaignByHash(hashCode);
            }
            else
            {
                return null;
            }
        }

        
        public static Campaign GetCampaignData(int nGroupID, long nCampaignID)
        {
            BaseCampaign t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                return t.GetCampaignData(nCampaignID);
            }
            else
            {
                return null;
            }
        }


        
        public static Subscription GetSubscriptionData(int nGroupID, string sSubscriptionCode
            , string sCountryCd2, string sLanguageCode3, string sDeviceName, bool bGetAlsoUnActive)
        {
            BaseSubscription t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                return (new SubscriptionCacheWrapper(t)).GetSubscriptionData(sSubscriptionCode, sCountryCd2, sLanguageCode3, sDeviceName, bGetAlsoUnActive);
            }
            else
            {
                return null;
            }
        }

        
        public static Collection GetCollectionData(int nGroupID, string sCollectionCode
            , string sCountryCd2, string sLanguageCode3, string sDeviceName, bool bGetAlsoUnActive)
        {
            BaseCollection t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                return (new CollectionCacheWrapper(t)).GetCollectionData(sCollectionCode, sCountryCd2, sLanguageCode3, sDeviceName, bGetAlsoUnActive);
            }
            else
            {
                return null;
            }
        }


        
        public static Subscription GetSubscriptionDataByProductCode(int nGroupID, string sProductCode
            , string sCountryCd2, string sLanguageCode3, string sDeviceName, bool bGetAlsoUnActive)
        {
            BaseSubscription t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                return (new SubscriptionCacheWrapper(t)).GetSubscriptionDataByProductCode(sProductCode, sCountryCd2, sLanguageCode3, sDeviceName, bGetAlsoUnActive);
            }
            else
            {
                return null;
            }
        }

        
        public static int[] GetSubscriptionMediaList(int nGroupID, string sSubscriptionCode,
            Int32 nFileTypeID, string sDevice)
        {
            BaseSubscription t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                return (new SubscriptionCacheWrapper(t)).GetMediaList(sSubscriptionCode, nFileTypeID, sDevice);
            }
            else
            {
                return null;
            }
        }

        
        public static List<int> GetSubscriptionMediaList2(int nGroupID, string sSubscriptionCode,
            Int32 nFileTypeID, string sDevice)
        {
            BaseSubscription t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                int[] temp = null;

                temp = (new SubscriptionCacheWrapper(t)).GetMediaList(sSubscriptionCode, nFileTypeID, sDevice);

                if (temp != null)
                    return temp.ToList<int>();

                return null;
            }
            else
            {
                return null;
            }
        }

        
        public static bool DoesMediaBelongToSubscription(int nGroupID, string sSubscriptionCode, Int32 nMediaID)
        {
            BaseSubscription t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                return (new SubscriptionCacheWrapper(t)).DoesMediasExists(sSubscriptionCode, nMediaID);
            }
            else
            {
                return false;
            }
        }

        
        public static PPVModule[] GetPPVModuleList(int nGroupID, string sCountryCd2, string sLanguageCode3, string sDeviceName)
        {
            BasePPVModule t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                return (new PPVModuleCacheWrapper(t)).GetPPVModuleList(sCountryCd2, sLanguageCode3, sDeviceName);
            }
            else
            {
                return null;
            }
        }

        
        public static PPVModuleContainer[] GetPPVModuleListForAdmin(int nGroupID, Int32 nMediaFileID,
            string sCountryCd2, string sLanguageCode3, string sDeviceName)
        {
            BasePPVModule t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                return (new PPVModuleCacheWrapper(t)).GetPPVModuleListForAdmin(nMediaFileID, sCountryCd2, sLanguageCode3, sDeviceName);
            }
            else
            {
                return null;
            }
        }

        
        public static DiscountModule[] GetDiscountsModuleListForAdmin(int nGroupID)
        {
            BaseDiscount t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                return t.GetDiscountsModuleListForAdmin();
            }
            else
            {
                return null;
            }
        }

        
        public static MediaFilePPVModule[] GetPPVModuleListForMediaFiles(int nGroupID, Int32[] nMediaFileIDs,
            string sCountryCd2, string sLanguageCode3, string sDeviceName)
        {
            BasePPVModule t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                return (new PPVModuleCacheWrapper(t)).GetPPVModuleListForMediaFiles(nMediaFileIDs, sCountryCd2, sLanguageCode3, sDeviceName);
            }
            else
            {
                return null;
            }
        }

        
        public static MediaFilePPVModule[] GetPPVModuleListForMediaFilesST(int nGroupID,
            string sMediaFileIDsCommaSeperated, string sCountryCd2, string sLanguageCode3, string sDeviceName)
        {
            string[] sSep = { ";" };
            Int32[] nMediaFileIDs = null;
            string[] sMediaIDs = sMediaFileIDsCommaSeperated.Split(sSep, StringSplitOptions.RemoveEmptyEntries);
            if (sMediaIDs.Length > 0)
                nMediaFileIDs = new int[sMediaIDs.Length];
            for (int j = 0; j < sMediaIDs.Length; j++)
                nMediaFileIDs[j] = int.Parse(sMediaIDs[j]);

            BasePPVModule t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                return (new PPVModuleCacheWrapper(t)).GetPPVModuleListForMediaFiles(nMediaFileIDs, sCountryCd2, sLanguageCode3, sDeviceName);
            }
            else
            {
                return null;
            }
        }

        
        public static MediaFilePPVContainer[] GetPPVModuleListForMediaFilesWithExpiry(int nGroupID, Int32[] nMediaFileIDs,
            string sCountryCd2, string sLanguageCode3, string sDeviceName)
        {
            BasePPVModule t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                return (new PPVModuleCacheWrapper(t)).GetPPVModuleListForMediaFilesWithExpiry(nMediaFileIDs, sCountryCd2, sLanguageCode3, sDeviceName);
            }
            else
            {
                return null;
            }
        }

        
        public static PPVModule[] GetPPVModuleShrinkList(int nGroupID, string sCountryCd2, string sLanguageCode3, string sDeviceName)
        {
            BasePPVModule t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                return (new PPVModuleCacheWrapper(t)).GetPPVModuleShrinkList(sCountryCd2, sLanguageCode3, sDeviceName);
            }
            else
            {
                return null;
            }
        }

        
        public static PPVModule GetPPVModuleData(int nGroupID, string sPPVCode
            , string sCountryCd2, string sLanguageCode3, string sDeviceName)
        {
            BasePPVModule t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                return (new PPVModuleCacheWrapper(t)).GetPPVModuleData(sPPVCode, sCountryCd2, sLanguageCode3, sDeviceName);
            }
            else
            {
                return null;
            }
        }

        
        public static PrePaidModule GetPrePaidModuleData(int nGroupID, int nPrePaidCode
            , string sCountryCd2, string sLanguageCode3, string sDeviceName)
        {
            BasePrePaidModule t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                return (new PrePaidModuleCacheWrapper(t)).GetPrePaidModuleData(nPrePaidCode, sCountryCd2, sLanguageCode3, sDeviceName);
            }
            else
            {
                return null;
            }
        }


        
        public static PriceCode[] GetPriceCodeList(int nGroupID, string sCountryCd2, string sLanguageCode3, string sDeviceName)
        {
            BasePricing t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                return (new PricingCacheWrapper(t)).GetPriceCodeList(sCountryCd2, sLanguageCode3, sDeviceName);
            }
            else
            {
                return null;
            }
        }

        
        public static PriceCode GetPriceCodeData(int nGroupID, string sPriceCode
            , string sCountryCd2, string sLanguageCode3, string sDeviceName)
        {
            BasePricing t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                return (new PricingCacheWrapper(t)).GetPriceCodeData(sPriceCode, sCountryCd2, sLanguageCode3, sDeviceName);
            }
            else
            {
                return null;
            }
        }

        
        public static DiscountModule GetDiscountCodeData(int nGroupID, string sDiscountCode)
        {
            BaseDiscount t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                return t.GetDiscountCodeData(sDiscountCode);
            }
            else
            {
                return null;
            }
        }

        
        public static UsageModule GetUsageModuleData(int nGroupID, string sUsageModuleCode, string sCountryCd2, string sLanguageCode3, string sDeviceName)
        {
            BaseUsageModule t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                return (new UsageModuleCacheWrapper(t)).GetUsageModuleData(sUsageModuleCode);
            }
            else
            {
                return null;
            }
        }

        
        public static UsageModule[] GetUsageModuleList(int nGroupID, string sCountryCd2, string sLanguageCode3, string sDeviceName)
        {
            BaseUsageModule t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                return (new UsageModuleCacheWrapper(t)).GetUsageModuleList();
            }
            else
            {
                return null;
            }
        }

        
        public static UsageModule GetOfflineUsageModule(int nGroupID, string sCountryCd2, string sLanguageCode3, string sDeviceName)
        {
            BaseUsageModule t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                return (new UsageModuleCacheWrapper(t)).GetOfflineUsageModuleData();
            }
            else
            {
                return null;
            }
        }

        
        public static CouponsGroup GetCouponGroupData(int nGroupID, string sCouponGroupID)
        {
            BaseCoupons t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                return t.GetCouponGroupData(sCouponGroupID);
            }
            else
            {
                return null;
            }
        }

        
        public static CouponsGroup[] GetCouponGroupListForAdmin(int nGroupID)
        {
            BaseCoupons t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                return t.GetCouponGroupListForAdmin();
            }
            else
            {
                return null;
            }
        }

        
        public static CouponsGroup[] GetVoucherGroupList(int nGroupID)
        {
            BaseCoupons t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                return t.GetCouponGroupListForAdmin(true);
            }
            else
            {
                return null;
            }
        }

        
        public static CouponDataResponse GetCouponStatus(int nGroupID, string sCouponCode)
        {
            CouponDataResponse response = new CouponDataResponse();
            BaseCoupons t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                CouponData coupon = t.GetCouponStatus(sCouponCode);
                response.Status = new Status((int)eResponseStatus.Error, "Error");

                if (coupon != null)
                {
                    response.Coupon = coupon;
                    response.Status = new Status((int)eResponseStatus.OK, "OK");
                    if (coupon.m_CouponStatus == CouponsStatus.NotExists)
                    {
                        response.Status = new Status((int)eResponseStatus.CouponNotValid, "Coupon Not Valid");
                    }
                }

            }
            else
            {
                response.Coupon = new CouponData();
                response.Coupon.Initialize(null, CouponsStatus.NotExists);
                response.Status = new Status((int)eResponseStatus.Error, "Error");
            }
            return response;
        }

        
        public static CouponsStatus SetCouponUsed(int nGroupID, string sCouponCode, string sSiteGUID)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            BaseCoupons t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                return t.SetCouponUsed(sCouponCode, sSiteGUID, 0, 0, 0, 0);
            }
            else
            {
                return CouponsStatus.NotExists;
            }
        }

        
        public static CouponsStatus SetCouponUses(int nGroupID, string sCouponCode, string sSiteGUID, Int32 nMediaFileID, Int32 nSubCode, Int32 nCollectionCode, int nPrePaidCode)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            BaseCoupons t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                return t.SetCouponUsed(sCouponCode, sSiteGUID, nMediaFileID, nSubCode, nCollectionCode, nPrePaidCode);
            }
            else
            {
                return CouponsStatus.NotExists;
            }
        }

        
        public static PreviewModule GetPreviewModuleByID(int nGroupID, long lPreviewModuleID)
        {
            BasePreviewModule t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                return t.GetPreviewModuleByID(lPreviewModuleID);
            }
            else
            {
                return null;
            }
        }

        
        public static PreviewModule[] GetPreviewModulesArrayByGroupIDForAdmin(int nGroupID)
        {
            BasePreviewModule t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                return t.GetPreviewModulesArrayByGroupID(nGroupID);
            }
            else
            {
                return null;
            }
        }


        
        public static UsageModule GetUsageModule(int nGroupID, string sAssetCode, eTransactionType transactionType)
        {
            BasePreviewModule t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                return t.GetUsageModule(nGroupID, sAssetCode, transactionType);
            }
            else
            {
                return null;
            }
        }

        
        public static SubscriptionsResponse GetSubscriptionsData(int nGroupID, string[] oSubCodes,
            string sCountryCd2, string sLanguageCode3, string sDeviceName)
        {
            return GetSubscriptions(nGroupID, oSubCodes, sCountryCd2, sLanguageCode3, sDeviceName, SubscriptionOrderBy.StartDateAsc);
        }

        
        public static SubscriptionsResponse GetSubscriptions(int nGroupID, string[] oSubCodes,
            string sCountryCd2, string sLanguageCode3, string sDeviceName, SubscriptionOrderBy orderBy = SubscriptionOrderBy.StartDateAsc)
        {
            SubscriptionsResponse response = new SubscriptionsResponse();
            BaseSubscription t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                try
                {
                    response.Subscriptions = (new SubscriptionCacheWrapper(t)).GetSubscriptionsData(oSubCodes, sCountryCd2, sLanguageCode3, sDeviceName, orderBy);
                    response.Status = new Status((int)eResponseStatus.OK, "OK");
                }
                catch (Exception)
                {
                    response.Status = new Status((int)eResponseStatus.Error, "Error");
                }
            }
            else
            {
                response.Status = new Status((int)eResponseStatus.Error, "Error");
            }
            return response;
        }

        
        public static Collection[] GetCollectionsData(int nGroupID, string[] oCollCodes,
            string sCountryCd2, string sLanguageCode3, string sDeviceName)
        {
            BaseCollection t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                return (new CollectionCacheWrapper(t)).GetCollectionsData(oCollCodes, sCountryCd2, sLanguageCode3, sDeviceName);
            }
            else
            {
                return null;
            }
        }



        public static PPVModule ValidatePPVModuleForMediaFile(int groupID, Int32 mediaFileID, long ppvModuleCode)
        {
            BasePPVModule t = null;
            Utils.GetBaseImpl(ref t, groupID);
            if (groupID != 0 && t != null)
            {
                return (new PPVModuleCacheWrapper(t)).ValidatePPVModuleForMediaFile(groupID, mediaFileID, ppvModuleCode);
            }
            else
            {
                if (groupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        
        public static Subscription[] GetSubscriptionsByProductCodes(int nGroupID, string[] productCodes)
        {
            BaseSubscription t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                return (new SubscriptionCacheWrapper(t)).GetSubscriptionsDataByProductCodes(productCodes.ToList(), false);
            }
            else
            {
                return null;
            }
        }

        
        public static PPVModule[] GetPPVModulesByProductCodes(int nGroupID, string[] productCodes)
        {
            BasePPVModule t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                return (new PPVModuleCacheWrapper(t)).GetPPVModulesDataByProductCodes(productCodes.ToList());
            }
            else
            {
                return null;
            }
        }

        
        public static PPVModuleResponse GetPPVModulesData(int nGroupID, string[] sPPVCode, string sCountryCd2, string sLanguageCode3, string sDeviceName)
        {
            PPVModuleResponse response = new PPVModuleResponse();
            BasePPVModule t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                try
                {
                    response.PPVModules = (new PPVModuleCacheWrapper(t)).GetPPVModulesData(sPPVCode, sCountryCd2, sLanguageCode3, sDeviceName);
                    response.Status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
                catch (Exception)
                {
                    response.Status = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                }
            }
            else
            {
                response.Status = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            }

            return response;
        }

        public static ApiObjects.BusinessModuleResponse InsertPPV(int groupID, ApiObjects.IngestPPV ppv)
        {
            ApiObjects.BusinessModuleResponse response = new ApiObjects.BusinessModuleResponse(); ;
            BasePricing t = null;
            t = Utils.GetBasePricing(groupID, "InsertPPV");
            if (groupID != 0 && t != null)
            {
                response = t.InsertPPV(ppv);
            }
            else
            {
                if (groupID == 0)
                {
                    HttpContext.Current.Response.StatusCode = 404;
                }
                response = new ApiObjects.BusinessModuleResponse(0, new ApiObjects.Response.Status((int)eResponseStatus.Error, "Error"));
            }
            return response;
        }
        
        public static ApiObjects.BusinessModuleResponse UpdatePPV(int groupID, ApiObjects.IngestPPV ppv)
        {
            ApiObjects.BusinessModuleResponse response = new ApiObjects.BusinessModuleResponse(); ;
            BasePricing t = null;
            t = Utils.GetBasePricing(groupID, "UpdatePPV");
            if (groupID != 0 && t != null)
            {
                response = t.UpdatePPV(ppv);
            }
            else
            {
                if (groupID == 0)
                {
                    HttpContext.Current.Response.StatusCode = 404;
                }
                response = new ApiObjects.BusinessModuleResponse(0, new ApiObjects.Response.Status((int)eResponseStatus.Error, "Error"));
            }
            return response;
        }
        
        public static ApiObjects.BusinessModuleResponse DeletePPV(int groupID, string ppv)
        {
            ApiObjects.BusinessModuleResponse response = new ApiObjects.BusinessModuleResponse();

            BasePricing t = null;
            t = Utils.GetBasePricing(groupID, "DeletePPV");
            if (groupID != 0 && t != null)
            {
                response = t.DeletePPV(ppv);
            }
            else
            {
                if (groupID == 0)
                {
                    HttpContext.Current.Response.StatusCode = 404;
                }
                response = new ApiObjects.BusinessModuleResponse(0, new ApiObjects.Response.Status((int)eResponseStatus.Error, "Error"));
            }
            return response;
        }

        
        public static ApiObjects.BusinessModuleResponse InsertMPP(int groupID, ApiObjects.IngestMultiPricePlan multiPricePlan)
        {
            ApiObjects.BusinessModuleResponse response = new ApiObjects.BusinessModuleResponse();

            BasePricing t = null;
            t = Utils.GetBasePricing(groupID, "InsertMPP");
            if (groupID != 0 && t != null)
            {
                response = t.InsertMPP(multiPricePlan);
            }
            else
            {
                if (groupID == 0)
                {
                    HttpContext.Current.Response.StatusCode = 404;
                }
                response = new ApiObjects.BusinessModuleResponse(0, new ApiObjects.Response.Status((int)eResponseStatus.Error, "Error"));
            }
            return response;
        }

        
        public static ApiObjects.BusinessModuleResponse UpdateMPP(int groupID, ApiObjects.IngestMultiPricePlan multiPricePlan)
        {
            ApiObjects.BusinessModuleResponse response = new ApiObjects.BusinessModuleResponse();

            BasePricing t = null;
            t = Utils.GetBasePricing(groupID, "UpdateMPP");
            if (groupID != 0 && t != null)
            {
                response = t.UpdateMPP(multiPricePlan);
            }
            else
            {
                if (groupID == 0)
                {
                    HttpContext.Current.Response.StatusCode = 404;
                }
                response = new ApiObjects.BusinessModuleResponse(0, new ApiObjects.Response.Status((int)eResponseStatus.Error, "Error"));
            }
            return response;
        }

        
        public static ApiObjects.BusinessModuleResponse DeleteMPP(int groupID, string multiPricePlan)
        {
            ApiObjects.BusinessModuleResponse response = new ApiObjects.BusinessModuleResponse();

            BasePricing t = null;
            t = Utils.GetBasePricing(groupID, "DeleteMPP");
            if (groupID != 0 && t != null)
            {
                response = t.DeleteMPP(multiPricePlan);
            }
            else
            {
                if (groupID == 0)
                {
                    HttpContext.Current.Response.StatusCode = 404;
                }
                response = new ApiObjects.BusinessModuleResponse(0, new ApiObjects.Response.Status((int)eResponseStatus.Error, "Error"));
            }
            return response;
        }

        
        public static ApiObjects.BusinessModuleResponse InsertPricePlan(int groupID, ApiObjects.IngestPricePlan pricePlan)
        {
            ApiObjects.BusinessModuleResponse response = new ApiObjects.BusinessModuleResponse();

            BasePricing t = null;
            t = Utils.GetBasePricing(groupID, "InsertPricePlan");
            if (groupID != 0 && t != null)
            {
                response = t.InsertPricePlan(pricePlan);
            }
            else
            {
                if (groupID == 0)
                {
                    HttpContext.Current.Response.StatusCode = 404;
                }
                response = new ApiObjects.BusinessModuleResponse(0, new ApiObjects.Response.Status((int)eResponseStatus.Error, "Error"));
            }
            return response;
        }

        
        public static ApiObjects.BusinessModuleResponse UpdatePricePlan(int groupID, ApiObjects.IngestPricePlan pricePlan)
        {
            ApiObjects.BusinessModuleResponse response = new ApiObjects.BusinessModuleResponse();

            BasePricing t = null;
            t = Utils.GetBasePricing(groupID, "UpdatePricePlan");
            if (groupID != 0 && t != null)
            {
                response = t.UpdatePricePlan(pricePlan);
            }
            else
            {
                if (groupID == 0)
                {
                    HttpContext.Current.Response.StatusCode = 404;
                }
                response = new ApiObjects.BusinessModuleResponse(0, new ApiObjects.Response.Status((int)eResponseStatus.Error, "Error"));
            }
            return response;
        }

        
        public static ApiObjects.BusinessModuleResponse DeletePricePlan(int groupID, string pricePlan)
        {
            ApiObjects.BusinessModuleResponse response = new ApiObjects.BusinessModuleResponse();
            BasePricing t = null;
            t = Utils.GetBasePricing(groupID, "DeletePricePlan");
            if (groupID != 0 && t != null)
            {
                response = t.DeletePricePlan(pricePlan);
            }
            else
            {
                if (groupID == 0)
                {
                    HttpContext.Current.Response.StatusCode = 404;
                }
                response = new ApiObjects.BusinessModuleResponse(0, new ApiObjects.Response.Status((int)eResponseStatus.Error, "Error"));
            }
            return response;
        }

        
        public static ApiObjects.BusinessModuleResponse test(int nGroupID, string name)
        {
            ApiObjects.IngestMultiPricePlan mpp = new ApiObjects.IngestMultiPricePlan();

            mpp.Code = "MPP_3412307456_7";
            mpp.Action = ApiObjects.eIngestAction.Insert;
            mpp.StartDate = DateTime.UtcNow;
            mpp.EndDate = DateTime.UtcNow.AddDays(24);

            mpp.Channels = new List<string>();
            mpp.Channels.Add("Shai_Channel_Regression");

            mpp.PricePlansCodes = new List<string>();
            mpp.PricePlansCodes.Add("Price Plan for Ingest Sharon");

            mpp.FileTypes = new List<string>();
            mpp.FileTypes.Add("shdhsdfhsdfhdfs");
            mpp.FileTypes.Add("");

            ApiObjects.KeyValuePair kv = new ApiObjects.KeyValuePair();
            mpp.Titles = new List<ApiObjects.KeyValuePair>();
            kv.key = "eng";
            kv.value = "Ingest MPP title";
            mpp.Titles.Add(kv);

            mpp.Descriptions = new List<ApiObjects.KeyValuePair>();
            kv = new ApiObjects.KeyValuePair();
            kv.key = "eng";
            kv.value = "Ingest MPP description";
            mpp.Descriptions.Add(kv);

            mpp.InternalDiscount = "100% discount";
            ApiObjects.BusinessModuleResponse response = InsertMPP(203, mpp);

            return new ApiObjects.BusinessModuleResponse();
        }

        
        public static PPVModuleDataResponse GetPPVModuleResponse(int nGroupID, string sPPVCode
            , string sCountryCd2, string sLanguageCode3, string sDeviceName)
        {
            BasePPVModule t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                return (new PPVModuleCacheWrapper(t)).GetPPVModuleDataResponse(sPPVCode, sCountryCd2, sLanguageCode3, sDeviceName);
            }
            else
            {
                return null;
            }
        }

        public static PriceCode GetPriceCodeDataByCountyAndCurrency(int nGroupID, int priceCodeId, string countryCode, string currencyCode)
        {
            Pricing.BasePricing t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                return (new PricingCacheWrapper(t)).GetPriceCodeDataByCountyAndCurrency(priceCodeId, countryCode, currencyCode);
            }
            else
            {
                return null;
            }
        }

        public static DiscountModule GetDiscountCodeDataByCountryAndCurrency(int nGroupID, int discountCodeId, string countryCode, string currencyCode)
        {
            Pricing.BaseDiscount t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                return t.GetDiscountCodeDataByCountryAndCurrency(discountCodeId, countryCode, currencyCode);
            }
            else
            {
                return null;
            }
        }

        public static List<Coupon> GenerateCoupons(int groupId, int numberOfCoupons, long couponGroupId)
        {
            Pricing.BaseCoupons t = null;
            Utils.GetBaseImpl(ref t, groupId);
            if (t != null)
            {
                return t.GenerateCoupons(numberOfCoupons, couponGroupId);
            }
            else
            {
                return null;
            }
        }
    }
}
