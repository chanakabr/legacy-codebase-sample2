using ApiObjects;
using ApiObjects.Pricing;
using ApiObjects.Response;
using CachingProvider.LayeredCache;
using Core.Api;
using Phx.Lib.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Web;

namespace Core.Pricing
{
    public interface IPricingModule
    {
        GenericListResponse<DiscountDetails> GetValidDiscounts(int groupId);
        Subscription GetSubscriptionData(int nGroupID, string sSubscriptionCode, string sCountryCd2, string sLanguageCode3, string sDeviceName, bool bGetAlsoUnActive, string userId = null);

        SubscriptionsResponse GetSubscriptions(int groupId, HashSet<long> subscriptionIds, string sCountryCd2, string sLanguageCode3, string sDeviceName,
            AssetSearchDefinition assetSearchDefinition, SubscriptionOrderBy orderBy = SubscriptionOrderBy.StartDateAsc, int pageIndex = 0, int pageSize = 30,
            bool shouldIgnorePaging = true, int? couponGroupIdEqual = null, bool getAlsoInActive = false, long? previewModuleIdEqual = null, long? pricePlanIdEqual = null, 
            long? channelIdEqual = null, HashSet<SubscriptionType> subscriptionTypes = null);

        SubscriptionsResponse GetSubscriptionsByProductCodes(int nGroupID, List<string> productCodes, SubscriptionOrderBy orderBy = SubscriptionOrderBy.StartDateAsc);

        SubscriptionsResponse GetSubscriptions(int groupId, string language, string udid, SubscriptionOrderBy orderBy, int pageIndex, int pageSize,
            bool shouldIgnorePaging, bool getAlsoInActive, int? couponGroupIdEqual = null, long? previewModuleIdEqual = null, long? pricePlanIdEqual = null, 
            long? channelIdEqual = null, HashSet<SubscriptionType> subscriptionTypes = null);

        CouponsGroupResponse GetCouponsGroup(int groupId, long id);

        void InvalidateSubscription(int groupId, int subId = 0);

        Collection GetCollectionData(int groupId, string collectionCode, string country, string language, string udid, bool bGetAlsoUnActive);

        void InvalidateCollection(int groupId, long collId = 0);

        List<Collection> GetCollections(int groupId, List<long> collectionIds, string country, string udid, string lang, int? couponGroupIdEqual, bool getAlsoUnactive = false);

        DiscountModule GetDiscountCodeDataByCountryAndCurrency(int nGroupID, int discountCodeId, string countryCode, string currencyCode);
    }

    public interface IPPVModuleManager
    {
        GenericListResponse<PPVModule> GetPPVModuleList(int groupId, int? couponGroupIdEqual = null);
    }

    public interface IPagoModule
    {
        void InvalidateProgramAssetGroupOffer(long groupId, long pagoId = 0);
    }

    public class Module : IPricingModule, IPPVModuleManager, IPagoModule
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private static readonly Lazy<Module> lazy = new Lazy<Module>(() => new Module(), LazyThreadSafetyMode.PublicationOnly);

        public static Module Instance { get { return lazy.Value; } }

        public Module()
        {
        }

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

        public void InvalidateSubscription(int groupId, int subId = 0)
        {
            PricingCache.Instance.InvalidateSubscription(groupId, subId);
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

        public Subscription GetSubscriptionData(int nGroupID, string sSubscriptionCode, string sCountryCd2, string sLanguageCode3, string sDeviceName, bool bGetAlsoUnActive, string userId = null)
        {
            long nUserId = 0;

            if (long.TryParse(userId, out nUserId))
            {
                var res = GetSubscriptions(nGroupID, new HashSet<long>() { long.Parse(sSubscriptionCode) },
                    string.Empty, string.Empty, string.Empty, new AssetSearchDefinition() { UserId = nUserId }, SubscriptionOrderBy.StartDateAsc,
                    0, 30, true, null, false, null, null, null, null);

                if (res != null && res.Subscriptions?.Length == 1)
                {
                    return res.Subscriptions[0];
                }
            }
            else
            {
                var res = PricingCache.Instance.GetSubscriptions(nGroupID, new List<long>() { long.Parse(sSubscriptionCode) });

                if (res?.Count > 0)
                {
                    return res[0];
                }
            }

            return null;
        }

        public Collection GetCollectionData(int groupId, string collectionCode, string country, string language, string udid, bool bGetAlsoUnActive)
        {
            List<Collection> res = PricingCache.Instance.GetCollections(groupId, new List<long>() { long.Parse(collectionCode) });

            if (res?.Count > 0)
            {
                return res[0];
            }

            return null;
        }

        public List<Collection> GetCollections(int groupId, List<long> collectionIds, string country, string udid, string lang, int? couponGroupIdEqual, bool getAlsoUnactive = false)
        {
            List<Collection> collections = new List<Collection>();

            if (collectionIds == null || collectionIds.Count == 0)
            {
                return collections;
            }

            Dictionary<string, string> keysToOriginalValueMap = new Dictionary<string, string>();
            Dictionary<string, List<string>> invalidationKeysMap = new Dictionary<string, List<string>>();

            foreach (long id in collectionIds)
            {
                string key = LayeredCacheKeys.GetCollectionKey(groupId, id);
                keysToOriginalValueMap.Add(key, id.ToString());
                invalidationKeysMap.Add(key, new List<string>() { LayeredCacheKeys.GetCollectionInvalidationKey(groupId, id) });
            }

            Dictionary<string, Collection> collectionsMap = null;

            if (!LayeredCache.Instance.GetValues(keysToOriginalValueMap,
                                                ref collectionsMap,
                                                GetCollections,
                                                new Dictionary<string, object>() {
                                                        { "groupId", groupId },
                                                        { "country", country },
                                                        { "udid", udid },
                                                        { "lang", lang },
                                                        { "getAlsoUnactive", getAlsoUnactive },
                                                        { "couponGroupIdEqual", couponGroupIdEqual },
                                                        { "collectionIds", keysToOriginalValueMap.Values.ToList() }
                                                   },
                                                groupId,
                                                LayeredCacheConfigNames.GET_SUBSCRIPTIONS,
                                                invalidationKeysMap))
            {
                log.Warn($"Failed getting Collections from LayeredCache, groupId: {groupId}, subIds: {string.Join(",", collectionIds)}");
                return collections;
            }

            collections = collectionsMap == null ? new List<Collection>() : collectionsMap.Values.ToList();

            if (!getAlsoUnactive && collections?.Count > 0)
            {
                collections = collections.Where((item) => item.IsActive.HasValue && item.IsActive.Value).ToList();
            }

            if (couponGroupIdEqual.HasValue)
            {
                collections = collections?.Where(x => x.m_oCouponsGroup.m_sGroupCode == couponGroupIdEqual.Value.ToString()).ToList();
            }

            return collections;
        }

        public static Tuple<Dictionary<string, Collection>, bool> GetCollections(Dictionary<string, object> funcParams)
        {
            Dictionary<string, Collection> collections = new Dictionary<string, Collection>();
            List<string> collectionIds = null;

            int? groupId = funcParams["groupId"] as int?;
            string country = funcParams["country"] as string;
            string udid = funcParams["udid"] as string;
            string lang = funcParams["lang"] as string;
            bool? getAlsoUnactive = funcParams["getAlsoUnactive"] as bool?;
            int? couponGroupIdEqual = funcParams["couponGroupIdEqual"] as int?;

            if (funcParams.ContainsKey(LayeredCache.MISSING_KEYS) && funcParams[LayeredCache.MISSING_KEYS] != null)
            {
                collectionIds = ((List<string>)funcParams[LayeredCache.MISSING_KEYS]);
            }
            else if (funcParams["collectionIds"] != null)
            {
                collectionIds = (List<string>)funcParams["collectionIds"];
            }

            if (collectionIds?.Count > 0)
            {
                BaseCollection t = null;
                Utils.GetBaseImpl(ref t, groupId.Value);
                if (t != null)
                {
                    var response = t.GetCollectionsData(collectionIds.ToArray(), country, lang, udid, couponGroupIdEqual, getAlsoUnactive.Value);
                    if (response != null && response.Collections != null && response.Collections.Length > 0)
                    {
                        collections = response.Collections.ToDictionary(x => LayeredCacheKeys.GetCollectionKey(groupId.Value, long.Parse(x.m_sObjectCode)), y => y);
                    }
                }
            }

            return Tuple.Create(collections, true);
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
                return (new PPVModuleCacheWrapper(t)).GetPPVModuleList();
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

        // TODO: check if country / language / udid are needed
        public static MediaFilePPVContainer[] GetPPVModuleListForMediaFilesWithExpiry(int nGroupID, Int32[] nMediaFileIDs)
        {
            BasePPVModule t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                return (new PPVModuleCacheWrapper(t)).GetPPVModuleListForMediaFilesWithExpiry(nMediaFileIDs);
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
                return (new PPVModuleCacheWrapper(t)).GetPPVModuleShrinkList();
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
                return (new PPVModuleCacheWrapper(t)).GetPPVModuleData(sPPVCode);
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

        public static PriceCode GetPriceCodeData(int nGroupID, string sPriceCode, string sCountryCd2, string sLanguageCode3, string sDeviceName)
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

        public static CouponDataResponse GetCouponStatus(int nGroupID, string sCouponCode, long domainId)
        {
            CouponDataResponse response = new CouponDataResponse();
            BaseCoupons t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                CouponData coupon = t.GetCouponStatus(sCouponCode, domainId);
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
                response.Coupon = CouponData.NotExist;
                response.Status = new Status((int)eResponseStatus.Error, "Error");
            }
            return response;
        }

        public static CouponsStatus SetCouponUsed(int nGroupID, string sCouponCode, string sSiteGUID, long domainId)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            BaseCoupons t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                return t.SetCouponUsed(sCouponCode, sSiteGUID, 0, 0, 0, 0, domainId);
            }
            else
            {
                return CouponsStatus.NotExists;
            }
        }

        public static CouponsStatus SetCouponUses(int nGroupID, string sCouponCode, string sSiteGUID, Int32 nMediaFileID, Int32 nSubCode, Int32 nCollectionCode, int nPrePaidCode, long domainId, bool doReduce = false)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            BaseCoupons t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                return t.SetCouponUsed(sCouponCode, sSiteGUID, nMediaFileID, nSubCode, nCollectionCode, nPrePaidCode, domainId, doReduce);
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

        public SubscriptionsResponse GetSubscriptions(int groupId, HashSet<long> subscriptionIds, string sCountryCd2, string sLanguageCode3, string sDeviceName,
            AssetSearchDefinition assetSearchDefinition, SubscriptionOrderBy orderBy = SubscriptionOrderBy.StartDateAsc, int pageIndex = 0, int pageSize = 30,
            bool shouldIgnorePaging = true, int? couponGroupIdEqual = null, bool getAlsoInActive = false, long? previewModuleIdEqual = null, long? pricePlanIdEqual = null, 
            long? channelIdEqual = null, HashSet<SubscriptionType> subscriptionTypes = null)
        {
            SubscriptionsResponse response = new SubscriptionsResponse();

            try
            {
                var filter = api.Instance.GetObjectVirtualAssetObjectIds(groupId, assetSearchDefinition, ObjectVirtualAssetInfoType.Subscription, subscriptionIds);
                if (filter.ResultStatus == ObjectVirtualAssetFilterStatus.Error)
                {
                    response.Status = filter.Status;
                    return response;
                }

                if (filter.ResultStatus == ObjectVirtualAssetFilterStatus.None)
                {
                    response.Status = new Status((int)eResponseStatus.OK, "OK");
                    return response;
                }

                response.Status = new Status((int)eResponseStatus.OK, "OK");
                if (filter.ObjectIds == null && filter.ObjectIds.Any())
                {
                    return response;
                }

                response.TotalItems = filter.ObjectIds.Count;

                var subscriptions = PricingCache.Instance.GetSubscriptions(groupId, filter.ObjectIds);
                if (subscriptions == null || !subscriptions.Any())
                {
                    return response;
                }

                response.Subscriptions = subscriptions.ToArray();
                response.TotalItems = response.Subscriptions.Length;

                // filter
                if (couponGroupIdEqual.HasValue)
                {
                    FilterSubscriptionsByCoupon(couponGroupIdEqual.Value, response);
                }
                else if (previewModuleIdEqual.HasValue)
                {
                    FilterSubscriptionsByPreviewModule(previewModuleIdEqual.Value, response);
                }
                else if (pricePlanIdEqual.HasValue)
                {
                    FilterSubscriptionsByPricePlan(pricePlanIdEqual.Value, response);
                }
                else if (channelIdEqual.HasValue)
                {
                    FilterSubscriptionsByChannel(channelIdEqual.Value, response);
                }

                if (subscriptionTypes != null && subscriptionTypes.Any())
                {
                    FilterSubscriptionsByType(subscriptionTypes, response);
                }

                if (response.Subscriptions.Length == 0)
                {
                    return response;
                }

                // order
                if (orderBy == SubscriptionOrderBy.CreateDateAsc)
                {
                    response.Subscriptions = response.Subscriptions.OrderBy(x => x.CreateDate).ToArray();
                }
                else if (orderBy == SubscriptionOrderBy.CreateDateDesc)
                {
                    response.Subscriptions = response.Subscriptions.OrderByDescending(x => x.CreateDate).ToArray();
                }
                else if (orderBy == SubscriptionOrderBy.NameAsc)
                {
                    response.Subscriptions = response.Subscriptions.OrderBy(x => x.m_sName?.Length > 0 ? x.m_sName[0].m_sValue : "").ToArray();
                }
                else if (orderBy == SubscriptionOrderBy.NameDesc)
                {
                    response.Subscriptions = response.Subscriptions.OrderByDescending(x => x.m_sName?.Length > 0 ? x.m_sName[0].m_sValue : "").ToArray();
                }
                else if (orderBy == SubscriptionOrderBy.StartDateAsc)
                {
                    response.Subscriptions = response.Subscriptions.OrderBy(x => x.m_dStartDate).ToArray();
                }
                else if (orderBy == SubscriptionOrderBy.StartDateDesc)
                {
                    response.Subscriptions = response.Subscriptions.OrderByDescending(x => x.m_dStartDate).ToArray();
                }
                else if (orderBy == SubscriptionOrderBy.UpdateDateAsc)
                {
                    response.Subscriptions = response.Subscriptions.OrderBy(x => x.UpdateDate).ToArray();
                }
                else if (orderBy == SubscriptionOrderBy.UpdateDateDesc)
                {
                    response.Subscriptions = response.Subscriptions.OrderByDescending(x => x.UpdateDate).ToArray();
                }

                // page
                if (!shouldIgnorePaging && !couponGroupIdEqual.HasValue && !previewModuleIdEqual.HasValue && !pricePlanIdEqual.HasValue && !channelIdEqual.HasValue)
                {
                    int startIndexOnList = pageIndex * pageSize;
                    int rangeToGetFromList = (startIndexOnList + pageSize) > response.Subscriptions.Length ? (response.Subscriptions.Length - startIndexOnList) > 0 ? (response.Subscriptions.Length - startIndexOnList) : 0 : pageSize;

                    if (rangeToGetFromList == 0)
                    {
                        response.Status = new Status((int)eResponseStatus.OK, "OK");
                        response.Subscriptions = new Subscription[0];
                        return response;
                    }

                    response.Subscriptions = response.Subscriptions.Skip(startIndexOnList).Take(rangeToGetFromList).ToArray();
                }
            }
            catch (Exception ex)
            {
                log.Error($"Exception while calling GetSubscriptions, ex:[{ex}].");
                response.Status = new Status((int)eResponseStatus.Error, "Error");
            }

            return response;
        }

        private static void FilterSubscriptionsByCoupon(int couponGroupIdEqual, SubscriptionsResponse response)
        {
            var value = couponGroupIdEqual.ToString();
            var subscriptions = new List<Subscription>();

            foreach (var sub in response.Subscriptions)
            {
                if (sub.GetValidSubscriptionCouponGroup(value)?.Count > 0 || (sub.m_oCouponsGroup != null && sub.m_oCouponsGroup.m_sGroupCode == value))
                {
                    subscriptions.Add(sub);
                }
            }

            response.TotalItems = subscriptions.Count;
            response.Subscriptions = subscriptions?.ToArray();
        }

        private static void FilterSubscriptionsByPreviewModule(long previewModuleIdEqual, SubscriptionsResponse response)
        {
            response.Subscriptions = response.Subscriptions.Where(x => x.m_oPreviewModule != null && x.m_oPreviewModule.m_nID == previewModuleIdEqual).ToArray();
            response.TotalItems = response.Subscriptions.Length;
        }

        private static void FilterSubscriptionsByPricePlan(long pricePlanIdEqual, SubscriptionsResponse response)
        {
            response.Subscriptions = response.Subscriptions.Where(x => x.m_MultiSubscriptionUsageModule != null && x.m_MultiSubscriptionUsageModule.Length > 0 &&
                                                                  x.m_MultiSubscriptionUsageModule.Where(y => pricePlanIdEqual == y.m_nObjectID).ToList().Count > 0).ToArray();
            response.TotalItems = response.Subscriptions.Length;
        }

        private static void FilterSubscriptionsByChannel(long channelIdEqual, SubscriptionsResponse response)
        {
            response.Subscriptions = response.Subscriptions.Where(x => x.m_sCodes != null && x.m_sCodes.Length > 0 &&
                                                                  x.m_sCodes.Where(y => y.m_sCode.Equals(channelIdEqual.ToString())).ToList().Count > 0).ToArray();
            response.TotalItems = response.Subscriptions.Length;
        }

        private static void FilterSubscriptionsByType(HashSet<SubscriptionType> subscriptionTypes, SubscriptionsResponse response)
        {
            response.Subscriptions = response.Subscriptions.Where(x => subscriptionTypes.Contains(x.Type)).ToArray();
            response.TotalItems = response.Subscriptions.Length;
        }

        public SubscriptionsResponse GetSubscriptions(int groupId, string language, string udid, SubscriptionOrderBy orderBy, int pageIndex, int pageSize, bool shouldIgnorePaging, 
            bool getAlsoInActive, int? couponGroupIdEqual = null, long? previewModuleIdEqual = null, long? pricePlanIdEqual = null, long? channelIdEqual = null, HashSet<SubscriptionType> subscriptionTypes = null)
        {
            // get group's subscriptionIds
            var groupSubscriptions = PricingCache.Instance.GetGroupSubscriptionsItems(groupId, getAlsoInActive);

            if (groupSubscriptions == null)
            {
                return null;
            }

            var subscriptionIds = new HashSet<long>(groupSubscriptions.Select(x => x.Id).ToList());

            return GetSubscriptions(groupId, subscriptionIds, string.Empty, language, udid, null, orderBy, pageIndex, pageSize, shouldIgnorePaging, couponGroupIdEqual, 
                getAlsoInActive, previewModuleIdEqual, pricePlanIdEqual, channelIdEqual, subscriptionTypes);
        }

        public static CollectionsResponse GetCollectionsData(int nGroupID, string[] oCollCodes, string sCountryCd2, string sLanguageCode3, string sDeviceName, int pageIndex = 0, int pageSize = 30, bool shouldIgnorePaging = true, int? couponGroupIdEqual = null)
        {
            BaseCollection t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                if (!shouldIgnorePaging)
                {
                    int startIndexOnList = pageIndex * pageSize;
                    int rangeToGetFromList = (startIndexOnList + pageSize) > oCollCodes.Length ? (oCollCodes.Length - startIndexOnList) > 0 ? (oCollCodes.Length - startIndexOnList) : 0 : pageSize;
                    if (rangeToGetFromList > 0)
                    {
                        oCollCodes = oCollCodes.Skip(startIndexOnList).Take(rangeToGetFromList).ToArray();
                    }
                }

                return (new CollectionCacheWrapper(t)).GetCollectionsData(oCollCodes, sCountryCd2, sLanguageCode3, sDeviceName, couponGroupIdEqual);
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

        public SubscriptionsResponse GetSubscriptionsByProductCodes(int nGroupID, List<string> productCodes, SubscriptionOrderBy orderBy = SubscriptionOrderBy.StartDateAsc)
        {
            SubscriptionsResponse response = new SubscriptionsResponse()
            {
                Status = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString())
            };

            BaseSubscription t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                response = (new SubscriptionCacheWrapper(t)).GetSubscriptionsDataByProductCodes(productCodes, false, orderBy);
            }

            return response;
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

        public static PPVModuleResponse GetPPVModulesData(int nGroupID, string[] sPPVCode)
        {
            PPVModuleResponse response = new PPVModuleResponse();
            BasePPVModule t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                try
                {
                    response.PPVModules = (new PPVModuleCacheWrapper(t)).GetPPVModulesData(sPPVCode);
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

        public static PPVModuleDataResponse GetPPVModuleResponse(int nGroupID, string sPPVCode, string sCountryCd2, string sLanguageCode3, string sDeviceName)
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

        public DiscountModule GetDiscountCodeDataByCountryAndCurrency(int nGroupID, int discountCodeId, string countryCode, string currencyCode)
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

        public static List<Coupon> GenerateCoupons(int groupId, int numberOfCoupons, long couponGroupId, out Status status, bool useLetters = true, bool useNumbers = true,
            bool useSpecialCharacters = true)
        {
            status = null;
            Pricing.BaseCoupons t = null;
            Utils.GetBaseImpl(ref t, groupId);
            if (t != null)
            {
                return t.GenerateCoupons(numberOfCoupons, couponGroupId, out status, useLetters, useNumbers, useSpecialCharacters);
            }
            else
            {
                return null;
            }
        }

        public static CouponDataResponse ValidateCouponForSubscription(int groupId, int subscriptionId, string couponCode, long domainId)
        {
            Pricing.BaseCoupons t = null;
            Utils.GetBaseImpl(ref t, groupId);
            if (t != null)
            {
                return t.ValidateCouponForSubscription(groupId, subscriptionId, couponCode, domainId);
            }
            else
            {
                return null;
            }
        }

        public static SubscriptionSetsResponse GetSubscriptionSets(int groupId, List<long> ids, SubscriptionSetType? type = null)
        {
            SubscriptionSetsResponse response = new SubscriptionSetsResponse();
            try
            {
                response.SubscriptionSets = Utils.GetSubscriptionSets(groupId, ids, type);
                response.Status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            }
            catch (Exception ex)
            {
                response.Status = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.Error(string.Format("Failed GetSubscriptionSets, groupId: {0}, ids: {1}", groupId, ids != null ? string.Join(",", ids) : ""), ex);
            }

            return response;
        }

        public static SubscriptionSetsResponse GetSubscriptionSetsBySubscriptionIds(int groupId, List<long> subscriptionIds, SubscriptionSetType? type = null)
        {
            SubscriptionSetsResponse response = new SubscriptionSetsResponse();
            try
            {
                response.SubscriptionSets = Utils.GetSubscriptionSetsBySubscriptionIds(groupId, subscriptionIds, type);
                response.Status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            }
            catch (Exception ex)
            {
                response.Status = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.Error(string.Format("Failed GetSubscriptionSetsBySubscriptionIds, groupId: {0}, subscriptionIds: {1}", groupId, subscriptionIds != null ? string.Join(",", subscriptionIds) : ""), ex);
            }

            return response;
        }

        public static SubscriptionSetsResponse GetSubscriptionSetsBySetIds(int groupId, List<long> setIds)
        {
            SubscriptionSetsResponse response = new SubscriptionSetsResponse();
            try
            {
                response.SubscriptionSets = Utils.GetSubscriptionSets(groupId, setIds);
                response.Status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            }
            catch (Exception ex)
            {
                response.Status = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.Error(string.Format("Failed GetSubscriptionSetsBySetIds, groupId: {0}, setIds: {1}", groupId, setIds != null ? string.Join(",", setIds) : ""), ex);
            }

            return response;
        }

        public static SubscriptionSetsResponse AddSubscriptionSet(int groupId, string name, List<long> subscriptionIds)
        {
            SubscriptionSetsResponse response = new SubscriptionSetsResponse();
            try
            {
                if (subscriptionIds != null && subscriptionIds.Count > 0)
                {
                    Dictionary<long, Dictionary<long, int>> subscriptionIdToSetIdsMap = Utils.GetSubscriptionIdToSetIdsMap(groupId, subscriptionIds);
                    if (subscriptionIdToSetIdsMap != null && subscriptionIdToSetIdsMap.Count > 0)
                    {
                        List<KeyValuePair<long, int>> setToPriorities = subscriptionIdToSetIdsMap.Where(x => x.Value != null).SelectMany(x => x.Value).ToList();
                        List<long> setIds = setToPriorities.Select(x => x.Key).Distinct().ToList();
                        if (setIds != null && setIds.Any())
                        {
                            List<long> usedSubscriptionIds = subscriptionIdToSetIdsMap.Where(x => x.Value != null & x.Value.Count > 0).Select(x => x.Key).ToList();
                            string msg = string.Format("{0} for the following subbscriptionIds: {1}", eResponseStatus.SubscriptionAlreadyBelongsToAnotherSubscriptionSet.ToString(), string.Join(",", usedSubscriptionIds));
                            response.Status = new Status((int)eResponseStatus.SubscriptionAlreadyBelongsToAnotherSubscriptionSet, msg);
                            return response;
                        }
                    }
                }

                SubscriptionSet subscriptionSet = Utils.InsertSubscriptionSet(groupId, name, subscriptionIds);
                if (subscriptionSet != null && subscriptionSet.Id > 0)
                {
                    response.SubscriptionSets.Add(subscriptionSet);
                    response.Status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());

                    List<int> subsToClear = new List<int>();

                    if (subscriptionIds?.Count > 0)
                    {
                        subsToClear.AddRange(subscriptionIds.Select(x => (int)x));
                    }

                    PricingCache.Instance.InvalidateSubscriptions(groupId, subsToClear);

                }
            }
            catch (Exception ex)
            {
                response.Status = new Status((int)eResponseStatus.Error, "Error");
                log.Error(string.Format("Failed AddSubscriptionSet, groupId: {0}, name: {1}, subscriptionIds: {2}", groupId, name, subscriptionIds != null ? string.Join(",", subscriptionIds) : ""), ex);
            }

            return response;
        }

        public static SubscriptionSetsResponse UpdateSubscriptionSet(int groupId, long setId, string name, List<long> subscriptionIds, bool shouldUpdateSubscriptionIds,
            SubscriptionSetType type = SubscriptionSetType.Switch)
        {
            SubscriptionSetsResponse response = new SubscriptionSetsResponse();
            try
            {
                response = GetSubscriptionSets(groupId, new List<long>() { setId });
                if (response.Status.Code != (int)eResponseStatus.OK)
                {
                    return response;
                }
                else if (response.SubscriptionSets.Count != 1)
                {
                    response.Status = new Status((int)eResponseStatus.SubscriptionSetDoesNotExist, eResponseStatus.SubscriptionSetDoesNotExist.ToString());
                    return response;
                }

                SubscriptionSet subscriptionSet = response.SubscriptionSets[0];
                subscriptionSet.Name = !string.IsNullOrEmpty(name) ? name : subscriptionSet.Name;
                if (shouldUpdateSubscriptionIds)
                {
                    Dictionary<long, Dictionary<long, int>> subscriptionIdToSetIdsMap = Utils.GetSubscriptionIdToSetIdsMap(groupId, subscriptionIds);
                    if (subscriptionIdToSetIdsMap != null && subscriptionIdToSetIdsMap.Count > 0)
                    {

                        List<KeyValuePair<long, int>> setToPriorities = subscriptionIdToSetIdsMap.Where(x => x.Value != null).SelectMany(x => x.Value).ToList();
                        List<long> setIds = setToPriorities.Where(x => x.Key != setId).Distinct().Select(x => x.Key).ToList();
                        if (setIds != null && setIds.Any())
                        {
                            List<long> usedSubscriptionIds = subscriptionIdToSetIdsMap.Where(x => x.Value != null & x.Value.Count > 0).Select(x => x.Key).ToList();
                            string msg = string.Format("{0} for the following subbscriptionIds: {1}", eResponseStatus.SubscriptionAlreadyBelongsToAnotherSubscriptionSet.ToString(), string.Join(",", usedSubscriptionIds));
                            response.Status = new Status((int)eResponseStatus.SubscriptionAlreadyBelongsToAnotherSubscriptionSet, msg);
                            return response;
                        }
                    }

                    if (type == SubscriptionSetType.Switch)
                    {
                        ((SwitchSet)subscriptionSet).SubscriptionIds = new List<long>(subscriptionIds);
                    }
                }
                if (type == SubscriptionSetType.Switch)
                {
                    SubscriptionSet updatedSubscriptionSet = Utils.UpdateSubscriptionSet(groupId, subscriptionSet.Id, subscriptionSet.Name, ((SwitchSet)subscriptionSet).SubscriptionIds, shouldUpdateSubscriptionIds);
                }

                if (subscriptionSet != null && subscriptionSet.Id > 0)
                {
                    response.SubscriptionSets.Clear();
                    response.SubscriptionSets.Add(subscriptionSet);
                    response.Status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
            }
            catch (Exception ex)
            {
                response.Status = new Status((int)eResponseStatus.Error, "Error");
                log.Error(string.Format("Failed UpdateSubscriptionSet, groupId: {0}, name: {1}, setId: {2} subscriptionIds: {3}",
                                        groupId, name, setId, subscriptionIds != null ? string.Join(",", subscriptionIds) : ""), ex);
            }

            return response;
        }

        public static Status DeleteSubscriptionSet(int groupId, long setId)
        {
            Status response = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            try
            {
                SubscriptionSetsResponse subscriptionSetsResponse = new SubscriptionSetsResponse();
                subscriptionSetsResponse = GetSubscriptionSets(groupId, new List<long>() { setId });
                if (subscriptionSetsResponse.Status.Code != (int)eResponseStatus.OK)
                {
                    response.Code = subscriptionSetsResponse.Status.Code;
                    response.Message = subscriptionSetsResponse.Status.Message;
                    return response;
                }
                else if (subscriptionSetsResponse.SubscriptionSets.Count != 1)
                {
                    response = new Status((int)eResponseStatus.SubscriptionSetDoesNotExist, eResponseStatus.SubscriptionSetDoesNotExist.ToString());
                    return response;
                }

                if (DAL.PricingDAL.DeleteSubscriptionSet(groupId, setId))
                {
                    response = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());

                    // call layered cache . setinvalidateion key
                    if (!LayeredCache.Instance.SetInvalidationKey(LayeredCacheKeys.GetSubscriptionSetInvalidationKey(groupId, setId)))
                    {
                        log.ErrorFormat("Failed LayeredCache.Instance.SetInvalidationKey, groupId: {0}, setId: {1}", groupId, setId);
                    }
                }
            }
            catch (Exception ex)
            {
                response = new Status((int)eResponseStatus.Error, "Error");
                log.Error(string.Format("Failed DeleteSubscriptionSet, groupId: {0}, setId: {1}", groupId, setId), ex);
            }

            return response;
        }

        public static SubscriptionSetsResponse GetSubscriptionSet(int groupId, long setId)
        {
            SubscriptionSetsResponse response = new SubscriptionSetsResponse();
            try
            {
                response = GetSubscriptionSets(groupId, new List<long>() { setId });
                if (response.Status.Code != (int)eResponseStatus.OK)
                {
                    return response;
                }
                else if (response.SubscriptionSets == null || response.SubscriptionSets.Count == 0 || response.SubscriptionSets[0].Id != setId)
                {
                    response.Status = new Status((int)eResponseStatus.SubscriptionSetDoesNotExist, eResponseStatus.SubscriptionSetDoesNotExist.ToString());
                    return response;
                }
            }
            catch (Exception ex)
            {
                response.Status = new Status((int)eResponseStatus.Error, "Error");
                log.Error(string.Format("Failed GetSubscriptionSet, groupId: {0}, setId: {1}", groupId, setId), ex);
            }

            return response;
        }

        public static SubscriptionSetsResponse GetSubscriptionSetsByBaseSubscriptionIds(int groupId, List<long> subscriptionIds, SubscriptionSetType? setType)
        {
            SubscriptionSetsResponse response = new SubscriptionSetsResponse();
            try
            {
                response.SubscriptionSets = Utils.GetSubscriptionSetsByBaseSubscriptionIds(groupId, subscriptionIds, setType);
                response.Status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            }
            catch (Exception ex)
            {
                response.Status = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.Error(string.Format("Failed GetSubscriptionSetsBySubscriptionIds, groupId: {0}, subscriptionIds: {1}", groupId, subscriptionIds != null ? string.Join(",", subscriptionIds) : ""), ex);
            }

            return response;
        }

        public static SubscriptionSetsResponse AddSubscriptionDependencySet(int groupId, string name, long baseSubscriptionId, List<long> subscriptionIds, SubscriptionSetType setType)
        {
            SubscriptionSetsResponse response = new SubscriptionSetsResponse();
            try
            {
                // check that base not belong to any other set (ass add on or as base)

                List<SubscriptionSet> baseInSet = Utils.GetSubscriptionSetsByBaseSubscriptionIds(groupId, new List<long>() { baseSubscriptionId }, setType);
                if (baseInSet != null && baseInSet.Count() > 0)
                {
                    string msg = string.Format("{0} for the following baseSubscriptionId: {1}", eResponseStatus.BaseSubscriptionAlreadyBelongsToAnotherSubscriptionSet.ToString(), baseSubscriptionId);
                    response.Status = new Status((int)eResponseStatus.BaseSubscriptionAlreadyBelongsToAnotherSubscriptionSet, msg);
                    return response;
                }

                // check validate subscription type 
                Status typeBase = Instance.ValidateSubscriptionsType(groupId, new List<long>() { baseSubscriptionId }, SubscriptionType.Base);
                if (typeBase.Code != (int)eResponseStatus.OK)
                {
                    response.Status = typeBase;
                    return response;
                }
                typeBase = Instance.ValidateSubscriptionsType(groupId, subscriptionIds, SubscriptionType.AddOn);
                if (typeBase.Code != (int)eResponseStatus.OK)
                {
                    response.Status = typeBase;
                    return response;
                }

                SubscriptionSet subscriptionSet = Utils.InsertSubscriptionDependencySet(groupId, name, baseSubscriptionId, subscriptionIds, setType);
                if (subscriptionSet != null && subscriptionSet.Id > 0)
                {
                    response.SubscriptionSets.Add(subscriptionSet);

                    List<int> subsToClear = new List<int>() { (int)baseSubscriptionId };

                    if (subscriptionIds?.Count > 0)
                    {
                        subsToClear.AddRange(subscriptionIds.Select(x => (int)x));
                    }

                    PricingCache.Instance.InvalidateSubscriptions(groupId, subsToClear);

                    response.Status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
            }
            catch (Exception ex)
            {
                response.Status = new Status((int)eResponseStatus.Error, "Error");
                log.Error(string.Format("Failed AddSubscriptionSet, groupId: {0}, name: {1}, subscriptionIds: {2}", groupId, name, subscriptionIds != null ? string.Join(",", subscriptionIds) : ""), ex);
            }

            return response;
        }

        private Status ValidateSubscriptionsType(int groupId, List<long> subscriptionIds, SubscriptionType subscriptionType)
        {
            Status status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            try
            {
                switch (subscriptionType)
                {
                    case SubscriptionType.NotApplicable:
                        status = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                        break;
                    case SubscriptionType.Base:
                    case SubscriptionType.AddOn:
                        if (subscriptionIds != null && subscriptionIds.Count() > 0)
                        {
                            HashSet<long> subIds = new HashSet<long>();
                            foreach (var item in subscriptionIds)
                            {
                                subIds.Add(item);
                            }

                            SubscriptionsResponse subscriptionsResponse = Instance.GetSubscriptions(groupId, subIds, string.Empty, string.Empty, string.Empty, null, 
                                SubscriptionOrderBy.StartDateAsc, 0, 30, true, null, false, null, null, null, null);
                            if (subscriptionsResponse != null && subscriptionsResponse.Status.Code == (int)eResponseStatus.OK && subscriptionsResponse.Subscriptions != null && subscriptionsResponse.Subscriptions.Count() > 0)
                            {
                                foreach (Subscription sub in subscriptionsResponse.Subscriptions)
                                {
                                    if (sub.Type != subscriptionType)
                                    {
                                        string msg = string.Format("{0} for the following subscriptionId: {1}", eResponseStatus.WrongSubscriptionType.ToString(), sub.m_SubscriptionCode);
                                        status = new Status((int)eResponseStatus.WrongSubscriptionType, msg);
                                        return status;
                                    }
                                }
                            }
                        }
                        break;
                    default:
                        break;
                }

                return status;
            }
            catch (Exception ex)
            {
                log.ErrorFormat("fail in ValidateSubscriptionsType subscriptionIds :{0}, subscriptionType: {1}, ex: {2}", string.Join(",", subscriptionIds), subscriptionType.ToString(), ex);
                status = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            }
            return status;
        }

        public static SubscriptionSetsResponse UpdateSubscriptionDependencySet(int groupId, long setId, string name, long? baseSubscriptionId, List<long> subscriptionIds,
            bool shouldUpdateSubscriptionIds, SubscriptionSetType setType = SubscriptionSetType.Dependency)
        {
            SubscriptionSetsResponse response = new SubscriptionSetsResponse();
            try
            {
                response = GetSubscriptionSets(groupId, new List<long>() { setId });
                if (response.Status.Code != (int)eResponseStatus.OK)
                {
                    return response;
                }
                else if (response.SubscriptionSets.Count != 1)
                {
                    response.Status = new Status((int)eResponseStatus.SubscriptionSetDoesNotExist, eResponseStatus.SubscriptionSetDoesNotExist.ToString());
                    return response;
                }

                SubscriptionSet subscriptionSet = response.SubscriptionSets[0];

                subscriptionSet.Name = !string.IsNullOrEmpty(name) ? name : subscriptionSet.Name;

                if (setType == SubscriptionSetType.Dependency)
                {
                    if (baseSubscriptionId.HasValue) // check that this base not belong to other set
                    {
                        List<SubscriptionSet> baseInSet = Utils.GetSubscriptionSetsByBaseSubscriptionIds(groupId, new List<long>() { baseSubscriptionId.Value }, setType);
                        if (baseInSet != null && baseInSet.Count() > 0 && baseInSet.Where(x => x.Id != setId).Count() > 0)
                        {
                            string msg = string.Format("{0} for the following baseSubscriptionId: {1}", eResponseStatus.BaseSubscriptionAlreadyBelongsToAnotherSubscriptionSet.ToString(), baseSubscriptionId);
                            response.Status = new Status((int)eResponseStatus.BaseSubscriptionAlreadyBelongsToAnotherSubscriptionSet, msg);
                            return response;
                        }
                    }
                    else
                    {
                        baseSubscriptionId = ((DependencySet)subscriptionSet).BaseSubscriptionId;
                    }

                    // check validate subscription type 
                    Status typeBase = Instance.ValidateSubscriptionsType(groupId, new List<long>() { baseSubscriptionId.Value }, SubscriptionType.Base);
                    if (typeBase.Code != (int)eResponseStatus.OK)
                    {
                        response.Status = typeBase;
                        return response;
                    }
                    typeBase = Instance.ValidateSubscriptionsType(groupId, subscriptionIds, SubscriptionType.AddOn);
                    if (typeBase.Code != (int)eResponseStatus.OK)
                    {
                        response.Status = typeBase;
                        return response;
                    }

                    SubscriptionSet updatedSubscriptionSet = Utils.UpdateSubscriptionDependencySet(groupId, subscriptionSet.Id, subscriptionSet.Name, baseSubscriptionId.Value,
                        subscriptionIds, shouldUpdateSubscriptionIds, setType);

                    if (updatedSubscriptionSet != null && updatedSubscriptionSet.Id > 0)
                    {
                        response.SubscriptionSets.Clear();
                        response.SubscriptionSets.Add(updatedSubscriptionSet);
                        response.Status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());

                        // call layered cache . setinvalidateion key
                        if (!LayeredCache.Instance.SetInvalidationKey(LayeredCacheKeys.GetSubscriptionSetInvalidationKey(groupId, updatedSubscriptionSet.Id)))
                        {
                            log.ErrorFormat("Failed LayeredCache.Instance.SetInvalidationKey, groupId: {0}, setId: {1}", groupId, updatedSubscriptionSet.Id);
                        }

                        List<int> subsToClear = new List<int>() { (int)baseSubscriptionId };

                        if (subscriptionIds?.Count > 0)
                        {
                            subsToClear.AddRange(subscriptionIds.Select(x => (int)x));
                        }

                        PricingCache.Instance.InvalidateSubscriptions(groupId, subsToClear);
                    }
                }
            }
            catch (Exception ex)
            {
                response.Status = new Status((int)eResponseStatus.Error, "Error");
                log.Error(string.Format("Failed UpdateSubscriptionSet, groupId: {0}, name: {1}, setId: {2} subscriptionIds: {3}",
                                        groupId, name, setId, subscriptionIds != null ? string.Join(",", subscriptionIds) : ""), ex);
            }

            return response;
        }

        public static UsageModulesResponse GetPricePlans(int groupId, List<long> pricePlanIds)
        {
            UsageModulesResponse response = new UsageModulesResponse()
            {
                Status = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString())
            };

            BasePricing t = null;
            Utils.GetBaseImpl(ref t, groupId);
            if (t != null)
            {
                return (new PricingCacheWrapper(t)).GetPricePlans(pricePlanIds);
            }

            return response;
        }

        public static IdsResponse GetCollectionIdsContainingMediaFile(int groupId, int mediaId, int mediaFileID)
        {
            BaseCollection t = null;
            Utils.GetBaseImpl(ref t, groupId);
            if (t != null)
            {
                return (new CollectionCacheWrapper(t)).GetCollectionIdsContainingMediaFile(mediaId, mediaFileID);
            }
            else
            {
                return null;
            }
        }

        public static List<Coupon> GeneratePublicCode(int groupId, long couponGroupId, string code, out ApiObjects.Response.Status status)
        {
            status = null;
            Pricing.BaseCoupons t = null;
            Utils.GetBaseImpl(ref t, groupId);
            if (t != null)
            {
                return t.GeneratePublicCode(groupId, couponGroupId, code, out status); ;
            }
            else
            {
                return null;
            }
        }

        public CouponsGroupResponse GetCouponsGroup(int groupId, long id)
        {
            CouponsGroupResponse response = new CouponsGroupResponse()
            {
                Status = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString())
            };

            BaseCoupons t = null;
            Utils.GetBaseImpl(ref t, groupId);
            if (t != null)
            {
                response = t.GetCouponGroupData(id);
            }

            return response;
        }

        public static CouponsGroupsResponse GetCouponsGroups(int groupId)
        {
            CouponsGroupsResponse response = new CouponsGroupsResponse();

            BaseCoupons t = null;
            Utils.GetBaseImpl(ref t, groupId);
            if (t != null)
            {
                response = t.GetCouponGroups();
            }

            return response;
        }

        public static CouponsGroupResponse UpdateCouponsGroup(int groupId, long id, string name, DateTime? startDate, DateTime? endDate,
            int? maxUsesNumber, int? maxUsesNumberOnRenewableSub, int? maxHouseholdUses, CouponGroupType? couponGroupType, long? discountCode)
        {
            CouponsGroupResponse response = new CouponsGroupResponse() { Status = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString()) };

            BaseCoupons t = null;
            Utils.GetBaseImpl(ref t, groupId);
            if (t != null)
            {
                response = t.UpdateCouponsGroup(groupId, id, name, startDate, endDate, maxUsesNumber, maxUsesNumberOnRenewableSub,
                    maxHouseholdUses, couponGroupType, discountCode);
            }

            return response;
        }

        public static Status DeleteCouponsGroups(int groupId, long id)
        {
            Status status = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            BaseCoupons t = null;
            Utils.GetBaseImpl(ref t, groupId);
            if (t != null)
            {
                status = t.DeleteCouponsGroups(groupId, id);
            }

            return status;
        }

        public static CouponsGroupResponse AddCouponsGroup(int groupId, string name, DateTime? startDate, DateTime? endDate,
            int? maxUsesNumber, int? maxUsesNumberOnRenewableSub, int? maxHouseholdUses, CouponGroupType? couponGroupType, long? discountCode)
        {
            CouponsGroupResponse response = new CouponsGroupResponse() { Status = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString()) };

            BaseCoupons t = null;
            Utils.GetBaseImpl(ref t, groupId);
            if (t != null)
            {
                response = t.AddCouponsGroup(groupId, name, startDate, endDate, maxUsesNumber, maxUsesNumberOnRenewableSub,
                    maxHouseholdUses, couponGroupType, discountCode);
            }

            return response;
        }

        public GenericListResponse<PPVModule> GetPPVModuleList(int groupId, int? couponGroupIdEqual = null)
        {
            GenericListResponse<PPVModule> response = new GenericListResponse<PPVModule>();

            BasePPVModule t = null;
            Utils.GetBaseImpl(ref t, groupId);
            if (t != null)
            {
                try
                {
                    PPVModule[] ppvModules = (new PPVModuleCacheWrapper(t)).GetPPVModuleList();
                    if (ppvModules != null && ppvModules.Length > 0)
                    {
                        if (couponGroupIdEqual.HasValue)
                        {
                            var value = couponGroupIdEqual.Value.ToString();
                            ppvModules = ppvModules.Where(ppv => ppv.m_oCouponsGroup.m_sGroupCode
                            == value).ToArray();
                        }
                        if (ppvModules?.Count() > 0)
                        {
                            response.Objects.AddRange(ppvModules.ToList());
                        }
                    }
                    response.SetStatus(eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
                catch (Exception)
                {
                    response.SetStatus(eResponseStatus.Error, eResponseStatus.Error.ToString());
                }
            }
            else
            {
                response.SetStatus(eResponseStatus.Error, eResponseStatus.Error.ToString());
            }

            return response;
        }
        public GenericListResponse<DiscountDetails> GetValidDiscounts(int groupId)
        {
            var response = new GenericListResponse<DiscountDetails>();

            BasePricing t = null;
            t = Utils.GetBasePricing(groupId, "GetValidDiscounts");
            if (t != null)
            {
                return t.GetValidDiscounts();
            }

            return response;
        }

        public void InvalidateCollection(int groupId, long coll = 0)
        {
            PricingCache.Instance.InvalidateCollection(groupId, coll);
        }

        public void InvalidateProgramAssetGroupOffer(long groupId, long pagoId = 0)
        {
            PricingCache.Instance.InvalidatePago(groupId, pagoId);
        }
    }
}