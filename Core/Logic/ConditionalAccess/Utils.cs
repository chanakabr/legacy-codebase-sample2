using ApiLogic.Api.Managers;
using ApiLogic.ConditionalAccess;
using ApiLogic.Pricing;
using ApiLogic.Pricing.Handlers;
using APILogic.Api.Managers;
using APILogic.ConditionalAccess.Managers;
using ApiObjects;
using ApiObjects.Base;
using ApiObjects.Billing;
using ApiObjects.Catalog;
using ApiObjects.CDNAdapter;
using ApiObjects.ConditionalAccess;
using ApiObjects.Pricing;
using ApiObjects.Response;
using ApiObjects.Roles;
using ApiObjects.Rules;
using ApiObjects.SubscriptionSet;
using ApiObjects.TimeShiftedTv;
using CachingProvider.LayeredCache;
using Core.Api.Managers;
using Core.Catalog;
using Core.Catalog.CatalogManagement;
using Core.Catalog.Request;
using Core.Catalog.Response;
using Core.GroupManagers;
using Core.Pricing;
using Core.Recordings;
using Core.Users;
using DAL;
using EpgBL;
using GroupsCacheManager;
using MoreLinq;
using MoreLinq.Extensions;
using NPVR;
using OffersGrpcClientWrapper;
using OTT.Service.Offers;
using Phx.Lib.Appconfig;
using Phx.Lib.Log;
using QueueWrapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using FeatureFlag;
using ApiLogic.Segmentation;
using ApiObjects.Recordings;
using TVinciShared;
using Tvinic.GoogleAPI;
using SlimAsset = ApiObjects.Rules.SlimAsset;
using TransactionType = OTT.Service.Offers.TransactionType;

namespace Core.ConditionalAccess
{
    public interface IConditionalAccessUtils
    {
        List<ApiObjects.Epg.FieldTypeEntity> GetAliasMappingFields(int groupId);
        Price GetLowestPriceByCouponCode(int groupId, ref string couponCode, List<SubscriptionCouponGroup> subscriptionCouponGroups, Price currentPrice, int domainId, CouponsGroup couponsGroup, string countryCode);
        Price GetPriceAfterDiscount(Price price, DiscountModule disc, Int32 nUseTime);
    }

    public class Utils : IConditionalAccessUtils
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private static readonly KLogger offlinePpvLogger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString(), "OfflinePpvLogger");
        private static readonly KLogger offlineSubscriptionLogger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString(), "OfflineSubscriptionLogger");
        private static readonly KLogger offlineCollectionLogger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString(), "offlineCollectionLogger");
        private static object lck = new object();

        private static readonly Lazy<Utils> lazy = new Lazy<Utils>(() => new Utils(), LazyThreadSafetyMode.PublicationOnly);
        public static Utils Instance { get { return lazy.Value; } }

        public const string SERIES_ID = "seriesId";
        public const string SEASON_NUMBER = "seasonNumber";
        internal const string EPISODE_NUMBER = "episodeNumber";
        private const string SERIES_ALIAS = "series_id";
        private const string SEASON_ALIAS = "season_number";
        private const string EPISODE_ALIAS = "episode_number";

        private const string MEDIA_FILES_CACHE_KEY_FORMAT = "MediaFiles_{0}";
        internal const double DEFAULT_MIN_PRICE_FOR_PREVIEW_MODULE = 0.2;
        public const int DEFAULT_MPP_RENEW_FAIL_COUNT = 10; // to be group specific override this value in the 
        // table groups_parameters, column FAIL_COUNT under ConditionalAccess DB.

        internal const string EPG_DATETIME_FORMAT = "dd/MM/yyyy HH:mm:ss";

        private static readonly string BASIC_LINK_TICK_TIME = "!--tick_time--";
        private static readonly string BASIC_LINK_COUNTRY_CODE = "!--COUNTRY_CD--";
        private static readonly string BASIC_LINK_HASH = "!--hash--";
        private static readonly string BASIC_LINK_GROUP = "!--group--";
        private static readonly string BASIC_LINK_CONFIG_DATA = "!--config_data--";
        private static readonly int RECOVERY_GRACE_PERIOD = 864000;

        public const string ROUTING_KEY_PROCESS_UNIFIED_RENEW_SUBSCRIPTION = "PROCESS_UNIFIED_RENEW_SUBSCRIPTION\\{0}";

        private Utils()
        {
        }

        public static void GetBaseConditionalAccessImpl(ref BaseConditionalAccess t, Int32 nGroupID)
        {
            GetBaseConditionalAccessImpl(ref t, nGroupID, "CA_CONNECTION_STRING");
        }

        public static void GetBaseConditionalAccessImpl(ref BaseConditionalAccess oConditionalAccess, Int32 nGroupID, string sConnKey)
        {
            int nImplID = TvinciCache.ModulesImplementation.GetModuleID(eWSModules.CONDITIONALACCESS, nGroupID, 1, sConnKey);

            switch (nImplID)
            {
                case (1):
                    {
                        oConditionalAccess = new TvinciConditionalAccess(nGroupID, sConnKey);
                        break;
                    }
                case (4):
                    {
                        oConditionalAccess = new FilmoConditionalAccess(nGroupID, sConnKey);
                        break;
                    }

                case (6):
                    {
                        oConditionalAccess = new ElisaConditionalAccess(nGroupID, sConnKey);
                        break;
                    }
                case (9):
                    {
                        oConditionalAccess = new CinepolisConditionalAccess(nGroupID, sConnKey);
                        break;
                    }
                case (10):
                    {
                        oConditionalAccess = new VodafoneConditionalAccess(nGroupID);
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }

        //internal static bool CheckSuspendedRole(int groupId, int roleId, SuspendedPermissions suspendedPermission)
        //{
        //    try
        //    {
        //        ApiObjects.Roles.Role role = null;
        //        string key = LayeredCacheKeys.GetRoleIdKey(roleId);
        //        string invalidationKey = LayeredCacheKeys.GetRoleIdInvalidationKey(roleId);
        //        if (!LayeredCache.Instance.Get<ApiObjects.Roles.Role>(key, ref role, Utils.GetRoleByRoleId, new Dictionary<string, object>() { { "groupId", groupId }, { "roleId", (long)roleId } },
        //                                                groupId, LayeredCacheConfigNames.GET_ROLE_BY_ROLE_ID, new List<string>() { invalidationKey }))
        //        {
        //            log.ErrorFormat("Failed getting Role by roleId from LayeredCache, roleId: {0}, key: {1}", roleId, key);
        //        }

        //        if (role.Permissions.Where(x => x.Name == suspendedPermission.ToString()).Count() > 0)
        //        {
        //            log.DebugFormat("CheckSuspendedRole role suspend for : {0}, groupId : {1}, roleId: {2}", suspendedPermission.ToString(), groupId, roleId);
        //            return true;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        log.ErrorFormat("fail CheckSuspendedRole : {0}, groupId : {1}, roleId: {2} , ex = {3}", suspendedPermission.ToString(), groupId, roleId, ex);
        //        return false;
        //    }
        //    return false;
        //}

        private static Tuple<Role, bool> GetRoleByRoleId(Dictionary<string, object> funcParams)
        {
            bool res = false;
            Role role = null;
            try
            {
                if (funcParams != null && funcParams.Count == 2 && funcParams.ContainsKey("groupId") && funcParams.ContainsKey("roleId"))
                {
                    int? groupId = funcParams["groupId"] as int?;
                    long? roleId = funcParams["roleId"] as long?;

                    if (groupId.HasValue && roleId.HasValue)
                    {
                        List<Role> roles = ApiDAL.GetRoles(groupId.Value, new List<long>() { roleId.Value });
                        if (roles != null && roles.Count() > 0)
                        {
                            role = roles.Where(x => x.Id == roleId.Value).Select(x => x).FirstOrDefault();
                            res = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("GetRoleByRoleId failed, parameters : {0}", string.Join(";", funcParams.Keys)), ex);
            }

            return new Tuple<Role, bool>(role, res);
        }

        public static void GetWSCredentials(int nGroupID, eWSModules eWSModule, ref string sUN, ref string sPass)
        {
            Credentials uc = TvinciCache.WSCredentials.GetWSCredentials(eWSModules.CONDITIONALACCESS, nGroupID, eWSModule);
            sUN = uc.m_sUsername;
            sPass = uc.m_sPassword;
        }

        public static BaseCampaignActionImpl GetCampaignActionByType(CampaignResult result)
        {
            BaseCampaignActionImpl retVal = null;
            switch (result)
            {
                case CampaignResult.Voucher:
                    {
                        retVal = new VoucherCampaignImpl();
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
            return retVal;
        }

        public static BaseCampaignActionImpl GetCampaignActionByTriggerType(CampaignTrigger trigger)
        {
            BaseCampaignActionImpl retVal = null;
            switch (trigger)
            {
                case CampaignTrigger.Purchase:
                    {
                        retVal = new VoucherCampaignImpl();
                        break;
                    }
                case CampaignTrigger.SocialInvite:
                    {
                        retVal = new SocialInviteCampaignImpl();
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
            return retVal;
        }

        public Price GetPriceAfterDiscount(Price price, DiscountModule disc, Int32 nUseTime)
        {
            Price discRetPrice = CopyPrice(price);

            if (disc.m_dEndDate < DateTime.UtcNow || disc.m_dStartDate > DateTime.UtcNow)
                return price;

            WhenAlgo whenAlgo = disc.m_oWhenAlgo;
            if (whenAlgo.m_eAlgoType == WhenAlgoType.N_FIRST_TIMES && whenAlgo.m_nNTimes != 0 && nUseTime >= whenAlgo.m_nNTimes)
                return price;

            if (whenAlgo.m_eAlgoType == WhenAlgoType.EVERY_N_TIMES && whenAlgo.m_nNTimes != 0 &&
                (double)(((double)nUseTime) / ((double)(whenAlgo.m_nNTimes))) - (Int32)((double)(((double)nUseTime) / ((double)(whenAlgo.m_nNTimes)))) != 0)
                return price;

            double dPer = disc.m_dPercent;
            Price discPrice = CopyPrice(disc.m_oPrise);

            if (disc.m_eTheRelationType == RelationTypes.And ||
                disc.m_eTheRelationType == RelationTypes.Or)
            {
                if (discPrice != null && discPrice.m_dPrice != 0 && discPrice.m_oCurrency.m_sCurrencyCD3 == discRetPrice.m_oCurrency.m_sCurrencyCD3)
                {
                    discRetPrice.m_dPrice -= discPrice.m_dPrice;
                    if (discRetPrice.m_dPrice < 0)
                        discRetPrice.m_dPrice = 0;
                }

                if (dPer > 0.0)
                {
                    discRetPrice.m_dPrice = (double)((Int32)((discRetPrice.m_dPrice * (100 - dPer)))) / 100;
                }
                else
                {
                    discRetPrice.m_dPrice = Math.Round((discRetPrice.m_dPrice * 100.0), MidpointRounding.AwayFromZero) / 100.0;
                }
            }
            return discRetPrice;
        }

        public static Int32 GetCustomData(string sCustomData)
        {
            return (int)BillingDAL.Get_LatestCustomDataID(sCustomData, "BILLING_CONNECTION_STRING");
        }

        internal static string GetCustomData(Int32 nCustomDataID)
        {
            string sRet = string.Empty;
            ODBCWrapper.DataSetSelectQuery selectQuery = null;
            try
            {
                selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey("BILLING_CONNECTION_STRING");
                selectQuery += "select CUSTOMDATA from customdata_indexer where ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nCustomDataID);
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                        sRet = selectQuery.Table("query").DefaultView[0].Row["CUSTOMDATA"].ToString();
                }
            }
            finally
            {
                if (selectQuery != null)
                {
                    selectQuery.Finish();
                }
            }
            return sRet;
        }

        internal static string GetSubscriptiopnPurchaseCoupon(long purchaseId)
        {
            string sRet = string.Empty;
            object oExistingCustomData = ODBCWrapper.Utils.GetTableSingleVal("subscriptions_purchases", "customdata", purchaseId, 0, "CA_CONNECTION_STRING");

            if (oExistingCustomData != null)
            {
                string sExistingCustomData = oExistingCustomData.ToString();
                XmlDocument docCustomData = new XmlDocument();
                docCustomData.LoadXml(sExistingCustomData);
                if (docCustomData.DocumentElement != null)
                {
                    XmlNode rootNode = docCustomData.DocumentElement;
                    sRet = Utils.GetSafeValue("cc", ref rootNode);
                }
            }
            return sRet;
        }

        internal static Int32 AddCustomData(string sCustomData)
        {
            Int32 nRet = GetCustomData(sCustomData);
            if (nRet == 0)
            {
                return (int)BillingDAL.Insert_NewCustomData(sCustomData, "BILLING_CONNECTION_STRING");
            }
            return nRet;
        }

        internal static bool ValidateBaseLink(Int32 nGroupID, Int32 nMediaFileID, string sBaseLink)
        {
            bool bRet = false;
            bRet = Api.Module.ValidateBaseLink(nGroupID, nMediaFileID, sBaseLink);
            return bRet;
        }

        internal static MeidaMaper[] GetMediaMapper(Int32 nGroupID, Int32[] nMediaFilesIDs)
        {
            if (nMediaFilesIDs == null)
                return null;

            string nMediaFilesIDsToCache = ConvertArrayIntToStr(nMediaFilesIDs);
            MeidaMaper[] mapper = Api.Module.MapMediaFiles(nGroupID, nMediaFilesIDs);

            return mapper;
        }

        public static int GetMediaIdByFileId(int groupId, int mediaFileId)
        {
            int mediaId = 0;
            try
            {
                MeidaMaper[] mapper = GetMediaMapper(groupId, new int[1] { mediaFileId });
                if (mapper != null && mapper.Length == 1)
                {
                    mediaId = mapper[0].m_nMediaID;
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetMediaIdByFileId for groupId: {0}, mediaFileId: {1}", groupId, mediaFileId), ex);
            }

            return mediaId;
        }

        internal static int GetMediaFileTypeID(int nGroupID, int nMediaFileID)
        {
            return Api.Module.GetMediaFileTypeID(nGroupID, nMediaFileID);
        }

        internal static Dictionary<int, bool> PPVBulkDoCreditNeedToDownloadedUsingCollections(int nGroupID, int nMediaFileID,
            List<int> lstAllUsersInDomain, List<int> lstCollectionCodes)
        {
            Dictionary<int, bool> res = new Dictionary<int, bool>();
            Dictionary<int, DateTime> collToCreateDateMapping = new Dictionary<int, DateTime>();
            DateTime dbTimeNow = ODBCWrapper.Utils.FICTIVE_DATE;
            if (lstCollectionCodes != null && lstCollectionCodes.Count > 0)
            {
                CollectionsResponse collectionsResponse = GetCollectionsDataWithCaching(lstCollectionCodes, nGroupID);

                if (collectionsResponse.Status.Code != (int)eResponseStatus.OK && collectionsResponse.Collections != null)
                {
                    // TODO: log
                    return res;
                }

                InitializePPVBulkDoCreditNeedDownloadedDictionary(ref res, lstCollectionCodes);

                if (ConditionalAccessDAL.Get_AllDomainsPPVUsesUsingCollections(lstAllUsersInDomain, nGroupID, nMediaFileID, lstCollectionCodes,
                    ref dbTimeNow, ref collToCreateDateMapping) && collToCreateDateMapping.Count > 0)
                {
                    for (int i = 0; i < collectionsResponse.Collections.Length; i++)
                    {
                        int collCode = 0;
                        if (collectionsResponse.Collections[i] != null && Int32.TryParse(collectionsResponse.Collections[i].m_CollectionCode, out collCode) &&
                            collCode > 0 && res.ContainsKey(collCode) && collToCreateDateMapping.ContainsKey(collCode)
                            && collectionsResponse.Collections[i].m_oCollectionUsageModule != null)
                        {
                            int nViewLifeCycle = collectionsResponse.Collections[i].m_oCollectionUsageModule.m_tsViewLifeCycle;
                            DateTime lastCreateDate = collToCreateDateMapping[collCode];
                            DateTime endDate = Utils.GetEndDateTime(lastCreateDate, nViewLifeCycle);
                            res[collCode] = dbTimeNow >= endDate;
                        }
                    }
                }

            }

            return res;
        }

        private static void InitializePPVBulkDoCreditNeedDownloadedDictionary(ref Dictionary<int, bool> dict, List<int> lstCollectionCodes)
        {
            for (int i = 0; i < lstCollectionCodes.Count; i++)
            {
                if (!dict.ContainsKey(lstCollectionCodes[i]))
                {
                    dict.Add(lstCollectionCodes[i], false);
                }
            }
        }

        /*
         * 1. Pass string or int or long as T
         * 2. Caching of pricing modules in CAS side is deprecated. Caching is now done on Pricing side.
         */
        internal static CollectionsResponse GetCollectionsDataWithCaching<T>(List<T> lstCollsCodes, int nGroupID) where T : IComparable, IComparable<T>, IEquatable<T>, IConvertible
        {
            string[] colls = lstCollsCodes.Select((item) => item.ToString()).Distinct().ToArray();
            return Pricing.Module.GetCollectionsData(nGroupID, colls, string.Empty, string.Empty, string.Empty);
        }

        /*
         * 1. Pass string or int or long as T
         * 2. Caching of pricing modules in CAS side is deprecated. Caching is now done on Pricing side.
         */
        internal static Subscription[] GetSubscriptionsDataWithCaching<T>(List<T> lstSubsCodes, int nGroupID) where T : IComparable, IComparable<T>, IEquatable<T>, IConvertible
        {
            HashSet<long> subIds = new HashSet<long>();
            long subId = 0;
            foreach (var item in lstSubsCodes)
            {
                if (long.TryParse(item.ToString(), out subId) && !subIds.Contains(subId))
                {
                    subIds.Add(subId);
                }
            }

            var res = Pricing.Module.Instance.GetSubscriptions(nGroupID, subIds, string.Empty, string.Empty, string.Empty, null);
            if (res != null)
                return res.Subscriptions;
            return null;
        }

        private static List<string> GetSubCodesForDBQuery(Subscription[] subs)
        {
            List<string> res = new List<string>();
            if (subs != null && subs.Length > 0)
            {
                for (int i = 0; i < subs.Length; i++)
                {
                    if (subs[i] != null)
                    {
                        res.Add(subs[i].m_SubscriptionCode);
                    }
                }
            }

            return res;
        }

        private static List<string> GetColCodesForDBQuery(CollectionsResponse colls)
        {
            List<string> res = new List<string>();
            if (colls == null || colls.Collections == null || colls.Collections.Length == 0)
            {
                return res;
            }

            for (int i = 0; i < colls.Collections.Length; i++)
            {
                if (colls.Collections[i] != null)
                {
                    res.Add(colls.Collections[i].m_CollectionCode);
                }
            }

            return res;
        }

        private static Dictionary<string, bool> InitializeCreditDownloadedDict(List<string> lst)
        {
            Dictionary<string, bool> res = new Dictionary<string, bool>();
            if (lst != null && lst.Count > 0)
            {
                for (int i = 0; i < lst.Count; i++)
                {
                    if (!res.ContainsKey(lst[i]))
                    {
                        res.Add(lst[i], true);
                    }
                }
            }

            return res;
        }

        // bulk version of Bundle_DoesCreditNeedToDownloaded
        internal static void DoBundlesCreditNeedToBeDownloaded(List<string> lstSubCodes, List<string> lstColCodes,
            int nMediaFileID, int nGroupID, List<int> allUsersInDomain, List<int> relatedMediaFiles,
            ref Dictionary<string, bool> subsRes, ref Dictionary<string, bool> collsRes)
        {
            Subscription[] subs = null;
            CollectionsResponse collectionsResponse = null;
            try
            {
                subsRes = InitializeCreditDownloadedDict(lstSubCodes);
                collsRes = InitializeCreditDownloadedDict(lstColCodes);

                if (lstSubCodes != null && lstSubCodes.Count > 0)
                {
                    subs = GetSubscriptionsDataWithCaching(lstSubCodes, nGroupID);
                }
                if (lstColCodes != null && lstColCodes.Count > 0)
                {
                    collectionsResponse = GetCollectionsDataWithCaching(lstColCodes, nGroupID);
                }

                Dictionary<string, DateTime> subsToCreateDateMapping = null;
                Dictionary<string, DateTime> colsToCreateDateMapping = null;
                DateTime dbTimeNow = ODBCWrapper.Utils.FICTIVE_DATE;
                List<string> subsLst = GetSubCodesForDBQuery(subs);
                List<string> colsLst = GetColCodesForDBQuery(collectionsResponse);
                List<string> domainUsers = allUsersInDomain.Select(item => item.ToString()).ToList<string>();
                if (ConditionalAccessDAL.Get_LatestCreateDateOfBundlesUses(subsLst, colsLst, domainUsers, relatedMediaFiles, nGroupID,
                    ref subsToCreateDateMapping, ref colsToCreateDateMapping, ref dbTimeNow))
                {
                    if (subs != null && subs.Length > 0)
                    {
                        for (int i = 0; i < subs.Length; i++)
                        {
                            if (subs[i] != null && subsToCreateDateMapping.ContainsKey(subs[i].m_SubscriptionCode))
                            {
                                subsRes[subs[i].m_SubscriptionCode] = CalcIsCreditNeedToBeDownloadedForSub(dbTimeNow, subsToCreateDateMapping[subs[i].m_SubscriptionCode], subs[i]);
                            }
                        }
                    }

                    if (collectionsResponse != null && collectionsResponse.Status.Code == (int)eResponseStatus.OK && collectionsResponse.Collections != null && collectionsResponse.Collections.Length > 0)
                    {
                        for (int i = 0; i < collectionsResponse.Collections.Length; i++)
                        {
                            if (collectionsResponse.Collections[i] != null && colsToCreateDateMapping.ContainsKey(collectionsResponse.Collections[i].m_CollectionCode))
                            {
                                collsRes[collectionsResponse.Collections[i].m_CollectionCode] = CalcIsCreditNeedToBeDownloadedForCol(dbTimeNow, colsToCreateDateMapping[collectionsResponse.Collections[i].m_CollectionCode], collectionsResponse.Collections[i]);
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                log.ErrorFormat("fail in DoBundlesCreditNeedToBeDownloaded lstSubCodes:{0}, lstColCodes:{1}, nMediaFileID:{2}, nGroupID:{3}, allUsersInDomain:{4}, ex:{5}",
                    string.Join(",", lstSubCodes), string.Join(",", lstColCodes), nMediaFileID, nGroupID, string.Join(",", allUsersInDomain), ex);
            }
        }

        private static bool CalcIsCreditNeedToBeDownloadedForSub(DateTime dbTimeNow, DateTime lastCreateDate, Subscription s)
        {
            bool res = true;
            if (s.m_oSubscriptionUsageModule != null && !lastCreateDate.Equals(ODBCWrapper.Utils.FICTIVE_DATE)
                && !dbTimeNow.Equals(ODBCWrapper.Utils.FICTIVE_DATE))
            {
                if (GetEndDateTime(lastCreateDate, s.m_oSubscriptionUsageModule.m_tsViewLifeCycle) > dbTimeNow)
                {
                    res = false;
                }
            }

            return res;
        }

        private static bool CalcIsCreditNeedToBeDownloadedForCol(DateTime dbTimeNow, DateTime lastCreateDate, Collection c)
        {
            bool res = true;
            if (c.m_oCollectionUsageModule != null && !lastCreateDate.Equals(ODBCWrapper.Utils.FICTIVE_DATE) &&
                !dbTimeNow.Equals(ODBCWrapper.Utils.FICTIVE_DATE))
            {
                if (GetEndDateTime(lastCreateDate, c.m_oCollectionUsageModule.m_tsViewLifeCycle) > dbTimeNow)
                {
                    res = false;
                }
            }

            return res;
        }

        internal static bool Bundle_DoesCreditNeedToDownloaded(string productCode, string userId, List<int> relatedMediaFiles, int groupID, eBundleType bundleType, int numOfUses)
        {
            bool bIsSub = true;
            bool isCreditDownloaded = false;

            PPVModule theBundle = null;
            UsageModule u = null;

            string sTableName = string.Empty;

            switch (bundleType)
            {
                case eBundleType.SUBSCRIPTION:
                    {
                        Subscription theSub = null;
                        theSub = Core.Pricing.Module.Instance.GetSubscriptionData(groupID, productCode, String.Empty, String.Empty, String.Empty, false);
                        u = theSub.m_oSubscriptionUsageModule;
                        theBundle = theSub;
                        bIsSub = true;

                        break;
                    }
                case eBundleType.COLLECTION:
                    {
                        Collection theCol = null;
                        theCol = Core.Pricing.Module.Instance.GetCollectionData(groupID, productCode, String.Empty, String.Empty, String.Empty, false);
                        u = theCol.m_oCollectionUsageModule;
                        theBundle = theCol;
                        bIsSub = false;

                        break;
                    }
            }

            Int32 nViewLifeCycle = u.m_tsViewLifeCycle;
            DateTime dtCreateDateOfLatestBundleUse = ODBCWrapper.Utils.FICTIVE_DATE;
            DateTime dtNow = ODBCWrapper.Utils.FICTIVE_DATE;

            if (numOfUses == 0)
            {
                isCreditDownloaded = true;
            }
            else if (u.m_nMaxNumberOfViews > 0)
            {
                int domainId = 0;
                List<int> allUsersInDomain = Utils.GetAllUsersInDomainBySiteGUIDIncludeDeleted(userId, groupID, ref domainId);
                if (ConditionalAccessDAL.Get_LatestCreateDateOfBundleUses(productCode, groupID, allUsersInDomain, relatedMediaFiles, bIsSub,
                    ref dtCreateDateOfLatestBundleUse, ref dtNow)
                    && !dtCreateDateOfLatestBundleUse.Equals(ODBCWrapper.Utils.FICTIVE_DATE)
                    && !dtNow.Equals(ODBCWrapper.Utils.FICTIVE_DATE)
                    && ((dtNow - dtCreateDateOfLatestBundleUse).TotalMinutes < nViewLifeCycle))
                {
                    isCreditDownloaded = false;
                }
                else
                {
                    isCreditDownloaded = true;
                }
            }

            return isCreditDownloaded;
        }

        // find in process defined to old payment and change it to new - also change in subscription purchases
        internal static void HandleUnifiedBillingCycle(int groupId, long domainId, int paymentGatewayId, DateTime endDate, int purchaseID, long oldUnifiedProcessId, long cycle)
        {
            try
            {
                // find in process defined to old payment and change it to new - also change in subscription purchases?
                long processId = 0;
                int state = 0;
                ProcessUnifiedState processState = ProcessUnifiedState.Renew;
                DataTable dt = ConditionalAccessDAL.GetUnifiedProcessId(groupId, paymentGatewayId, endDate, domainId, cycle, null);
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    processId = ODBCWrapper.Utils.GetLongSafeVal(dt.Rows[0], "ID");
                    state = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0], "state");
                    processState = (ProcessUnifiedState)state;
                }

                if (processId == 0) // create new process id + add message to queue
                {
                    // get details of OLD processState needed to be changed                    
                    DataRow dr = ConditionalAccessDAL.UpdateProcessDetailsForRenewal(oldUnifiedProcessId);
                    if (dr != null)
                    {
                        state = ODBCWrapper.Utils.GetIntSafeVal(dr, "STATE");
                        processState = (ProcessUnifiedState)state;
                    }
                    // create new process Id 
                    processId = ConditionalAccessDAL.InsertUnifiedProcess(groupId, paymentGatewayId, endDate, domainId, cycle, (int)processState);

                    // insert new message to queue

                    PaymentGateway paymentGateway = DAL.BillingDAL.GetPaymentGateway(groupId, paymentGatewayId, 1, 1);
                    DateTime nextRenewalDate;
                    if (processState == ProcessUnifiedState.Renew)
                    {
                        nextRenewalDate = endDate.AddMinutes(paymentGateway.RenewalStartMinutes);
                    }
                    else
                    {
                        nextRenewalDate = endDate.AddMinutes(paymentGateway.RenewalIntervalMinutes);
                    }

                    bool isKronos = PhoenixFeatureFlagInstance.Get().IsUnifiedRenewUseKronos();
                    Utils.RenewUnifiedTransactionMessageInQueue(groupId, domainId, DateUtils.DateTimeToUtcUnixTimestampMilliseconds(endDate), nextRenewalDate, processId, isKronos);
                }

                if (processId > 0) // already have message to queue so update subscription purchase row
                {
                    // update subscription Purchase
                    ConditionalAccessDAL.UpdateMPPRenewalProcessId(new List<int>() { purchaseID }, processId);
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        internal static void FillCatalogSignature(BaseRequest request)
        {
            request.m_sSignString = Guid.NewGuid().ToString();
            request.m_sSignature = TVinciShared.WS_Utils.GetCatalogSignature(request.m_sSignString, ApplicationConfiguration.Current.CatalogSignatureKey.Value);
        }

        private static BundlesContainingMediaRequest InitializeCatalogRequest(int nGroupID, int nMediaID,
            List<int> lstDistinctSubs, List<int> lstDistinctColls)
        {
            BundlesContainingMediaRequest request = new BundlesContainingMediaRequest();
            request.m_nGroupID = nGroupID;
            request.m_nMediaID = nMediaID;
            request.m_oFilter = new Filter();
            FillCatalogSignature(request);
            int sizeOfArr = lstDistinctSubs.Count + lstDistinctColls.Count, i = 0;
            request.m_oBundles = new BundleKeyValue[sizeOfArr];
            for (i = 0; i < lstDistinctSubs.Count; i++)
            {
                request.m_oBundles[i] = new BundleKeyValue() { m_nBundleCode = lstDistinctSubs[i], m_eBundleType = eBundleType.SUBSCRIPTION };
            }
            for (int j = 0; j < lstDistinctColls.Count; j++)
            {
                request.m_oBundles[j + i] = new BundleKeyValue() { m_nBundleCode = lstDistinctColls[j], m_eBundleType = eBundleType.COLLECTION };
            }

            return request;
        }

        private static bool IsUserCanStillUseEntitlement(int numOfUses, int maxNumOfUses)
        {
            // maxNumOfUses==0 means unlimited uses.
            return maxNumOfUses == 0 || numOfUses < maxNumOfUses;
        }

        private static void GetUserValidBundlesFromListOptimized(string sSiteGuid, int nMediaID, int nMediaFileID, MediaFileStatus eMediaFileStatus, int nGroupID,
            int[] nFileTypes, List<int> lstUserIDs, List<int> relatedMediaFiles,
            ref Subscription[] subsRes, ref Collection[] collsRes,
            ref Dictionary<string, UserBundlePurchase> subsPurchase, ref Dictionary<string, UserBundlePurchase> collPurchase, int domainID)
        {
            DataSet dataSet = ConditionalAccessDAL.Get_AllBundlesInfoByUserIDs(lstUserIDs, nFileTypes != null && nFileTypes.Length > 0 ? nFileTypes.ToList<int>() : new List<int>(0), nGroupID, domainID);
            if (IsBundlesDataSetValid(dataSet))
            {
                // the subscriptions and collections we add to those list will be sent to the Catalog in order to determine whether the media
                // given as input belongs to it.
                List<int> subsToSendToCatalog = new List<int>();
                List<int> collsToSendToCatalog = new List<int>();

                List<string> subsToBundleCreditDownloadedQuery = new List<string>();
                List<string> colsToBundleCreditDownloadedQuery = new List<string>();

                // iterate over subscriptions
                DataTable subs = dataSet.Tables[0];
                int waiver = 0;
                DateTime purchaseDate = DateTime.MinValue;
                DateTime endDate = DateTime.MinValue;

                if (subs != null && subs.Rows != null && subs.Rows.Count > 0)
                {
                    for (int i = 0; i < subs.Rows.Count; i++)
                    {
                        int numOfUses = 0;
                        int maxNumOfUses = 0;
                        string bundleCode = string.Empty;
                        waiver = 0;
                        purchaseDate = DateTime.MinValue;
                        endDate = DateTime.MinValue;
                        int gracePeriodMinutes = 0;
                        bool isSuspend = false;
                        bool isPending = false;
                        GetSubscriptionBundlePurchaseData(subs.Rows[i], "SUBSCRIPTION_CODE", ref numOfUses, ref maxNumOfUses, ref bundleCode, ref waiver,
                                                            ref purchaseDate, ref endDate, ref gracePeriodMinutes, ref isSuspend, ref isPending);

                        if (isPending) //pending entitlement BEO-8661
                        {
                            continue;
                        }

                        // decide which is the correct end period
                        if (endDate < DateTime.UtcNow)
                            endDate = endDate.AddMinutes(gracePeriodMinutes);

                        // add to bulk query of Bundle_DoesCreditNeedToDownloaded to DB
                        //afterwards, the subs who pass the Bundle_DoesCreditNeedToDownloaded to DB test add to Catalog request.
                        if (eMediaFileStatus == MediaFileStatus.ValidOnlyIfPurchase || !IsUserCanStillUseEntitlement(numOfUses, maxNumOfUses))
                        {
                            subsToBundleCreditDownloadedQuery.Add(bundleCode);
                        }
                        else
                        {
                            // add to Catalog's BundlesContainingMediaRequest
                            int subCode = 0;
                            if (Int32.TryParse(bundleCode, out subCode) && subCode > 0)
                            {
                                subsToSendToCatalog.Add(subCode);
                                if (!subsPurchase.ContainsKey(bundleCode))
                                {
                                    subsPurchase.Add(bundleCode, new UserBundlePurchase()
                                    {
                                        sBundleCode = bundleCode,
                                        nWaiver = waiver,
                                        dtPurchaseDate = purchaseDate,
                                        dtEndDate = endDate
                                    });
                                }
                            }
                            else
                            {
                                // log
                            }
                        }
                    }
                }

                //iterate over collections
                DataTable colls = dataSet.Tables[1];
                if (colls != null && colls.Rows != null && colls.Rows.Count > 0)
                {
                    for (int i = 0; i < colls.Rows.Count; i++)
                    {
                        int numOfUses = 0;
                        int maxNumOfUses = 0;
                        string bundleCode = string.Empty;
                        waiver = 0;
                        purchaseDate = DateTime.MinValue;
                        endDate = DateTime.MinValue;
                        bool isPending = false;

                        GetCollectionBundlePurchaseData(colls.Rows[i], "COLLECTION_CODE", ref numOfUses, ref maxNumOfUses, ref bundleCode, ref waiver, ref purchaseDate, ref endDate, ref isPending);

                        if (isPending) //pending entitlement BEO-8661
                        {
                            continue;
                        }

                        // add to bulk query of Bundle_DoesCreditNeedToDownload to DB
                        //afterwards, the colls which pass the Bundle_DoesCreditNeedToDownloaded to DB test add to Catalog request.
                        // finally, the colls which pass the catalog need to be validated against PPV_DoesCreditNeedToDownloadedUsingCollection
                        if (eMediaFileStatus == MediaFileStatus.ValidOnlyIfPurchase || !IsUserCanStillUseEntitlement(numOfUses, maxNumOfUses))
                        {
                            colsToBundleCreditDownloadedQuery.Add(bundleCode);
                        }
                        else
                        {
                            // add to Catalog's BundlesContainingMediaRequest
                            int collCode = 0;
                            if (Int32.TryParse(bundleCode, out collCode) && collCode > 0)
                            {
                                collsToSendToCatalog.Add(collCode);
                                if (!collPurchase.ContainsKey(bundleCode))
                                {
                                    collPurchase.Add(bundleCode, new UserBundlePurchase()
                                    {
                                        sBundleCode = bundleCode,
                                        nWaiver = waiver,
                                        dtPurchaseDate = purchaseDate,
                                        dtEndDate = endDate,
                                        isPending = isPending
                                    });
                                }
                            }
                            else
                            {
                                //log
                            }
                        }
                    }
                }

                HandleBundleCreditNeedToDownloadedQuery(subsToBundleCreditDownloadedQuery, colsToBundleCreditDownloadedQuery,
                    nMediaFileID, nGroupID, lstUserIDs, relatedMediaFiles, ref subsToSendToCatalog,
                    ref collsToSendToCatalog);
                // the subs / colls already purchased (no need to download creadit ) - can return it as OK 
                if (eMediaFileStatus == MediaFileStatus.ValidOnlyIfPurchase)
                {
                    if (subsToSendToCatalog != null && subsToSendToCatalog.Count > 0)
                    {
                        // check if credit need to be downloaded for specific mediafile 
                        subsRes = GetSubscriptionsDataWithCaching(subsToSendToCatalog, nGroupID);
                    }
                    if (collsToSendToCatalog != null && collsToSendToCatalog.Count > 0)
                    {
                        // TODO: rewrite
                        collsRes = GetCollectionsDataWithCaching(collsToSendToCatalog, nGroupID).Collections;
                    }
                }
                else // only if in the gap between end date to final end date - continue the check
                {
                    // get distinct subs from subs list, same for collection
                    List<int> distinctSubs = subsToSendToCatalog.Distinct().ToList<int>();
                    List<int> distinctColls = collsToSendToCatalog.Distinct().ToList<int>();

                    List<int> validatedSubs = null;
                    List<int> validatedColls = null;

                    if (distinctSubs.Count > 0 || distinctColls.Count > 0)
                    {
                        ValidateMediaContainedInBundles(nMediaID, nGroupID, distinctSubs, distinctColls, ref validatedSubs, ref validatedColls);
                    }

                    if (validatedSubs != null && validatedSubs.Count > 0)
                    {
                        subsRes = GetSubscriptionsDataWithCaching(validatedSubs, nGroupID);
                    }

                    // now validate bulk collections - PPV_CreditNeedToDownloadedUsingCollection

                    if (validatedColls != null && validatedColls.Count > 0)
                    {
                        Dictionary<int, bool> collsAfterPPVCreditValidation = PPVBulkDoCreditNeedToDownloadedUsingCollections(nGroupID,
                            nMediaFileID, lstUserIDs, validatedColls);
                        List<int> finalCollCodes = GetFinalCollectionCodes(collsAfterPPVCreditValidation);
                        if (finalCollCodes != null && finalCollCodes.Count > 0)
                        {
                            // TODO: rewrite
                            collsRes = GetCollectionsDataWithCaching(finalCollCodes, nGroupID).Collections;
                        }
                    }
                }
            }
            else
            {
                #region Logging
                StringBuilder sb = new StringBuilder("SP: ConditionalAccessDAL.Get_AllBundlesInfoByUserIDs returned corrupted data. ");
                sb.Append(String.Concat(" Site Guid: ", sSiteGuid));
                sb.Append(String.Concat(" Media ID: ", nMediaID));
                sb.Append(String.Concat(" Media File ID: ", nMediaFileID));
                if (lstUserIDs != null && lstUserIDs.Count > 0)
                {
                    sb.Append(" User IDs: ");
                    for (int i = 0; i < lstUserIDs.Count; i++)
                    {
                        sb.Append(String.Concat(lstUserIDs[i], ", "));
                    }
                }
                else
                {
                    sb.Append(" User IDs is null or empty. ");
                }
                if (nFileTypes != null && nFileTypes.Length > 0)
                {
                    sb.Append(" File Types: ");
                    for (int i = 0; i < nFileTypes.Length; i++)
                    {
                        sb.Append(String.Concat(nFileTypes[i], ", "));
                    }
                }
                else
                {
                    sb.Append("File Types is null of empty");
                }

                log.Error("Error - " + sb.ToString());
                #endregion

                throw new Exception("Error occurred in GetUserValidBundlesFromListOptimized. Refer to CAS.Utils log file");

            }
        }

        /// <summary>
        /// Partially defines a user's purchase of a bundle, so data is easily transferred between methods
        /// </summary>
        [Serializable]
        public class UserBundlePurchase
        {
            public string sBundleCode;
            public int nWaiver;
            public DateTime dtPurchaseDate;
            public DateTime dtEndDate;
            public int nNumOfUses;
            public int nMaxNumOfUses;
            public bool isPending;

            public UserBundlePurchase() { }
        }

        /// <summary>
        /// Partially defines a user's purchase of a bundle, so data is easily transferred between methods
        /// </summary>
        [Serializable]
        public class UserBundlePurchaseWithSuspend : UserBundlePurchase
        {
            public bool isSuspend;

            public UserBundlePurchaseWithSuspend() : base() { }
        }

        private static List<int> GetFinalCollectionCodes(Dictionary<int, bool> collsAfterPPVCreditValidation)
        {
            List<int> res = new List<int>();
            foreach (KeyValuePair<int, bool> kvp in collsAfterPPVCreditValidation)
            {
                if (!kvp.Value)
                {
                    res.Add(kvp.Key);
                }
            }

            return res;
        }

        private static void HandleBundleCreditNeedToDownloadedQuery(List<string> subsToBundleCreditDownloadedQuery, List<string> colsToBundleCreditDownloadedQuery, int nMediaFileID, int nGroupID,
                                                                    List<int> lstUserIDs, List<int> relatedMediaFileIDs, ref List<int> subsToSendToCatalog, ref List<int> collsToSendToCatalog)
        {
            if (subsToBundleCreditDownloadedQuery.Count > 0 || colsToBundleCreditDownloadedQuery.Count > 0)
            {
                Dictionary<string, bool> subsRes = null;
                Dictionary<string, bool> colsRes = null;
                DoBundlesCreditNeedToBeDownloaded(subsToBundleCreditDownloadedQuery, colsToBundleCreditDownloadedQuery, nMediaFileID, nGroupID, lstUserIDs, relatedMediaFileIDs, ref subsRes, ref colsRes);

                if (subsRes.Count > 0)
                {
                    foreach (KeyValuePair<string, bool> kvp in subsRes)
                    {
                        int temp = 0;
                        if (!kvp.Value && Int32.TryParse(kvp.Key, out temp) && temp > 0)
                        {

                            subsToSendToCatalog.Add(temp);
                        }
                    }
                }

                if (colsRes.Count > 0)
                {
                    foreach (KeyValuePair<string, bool> kvp in colsRes)
                    {
                        int temp = 0;
                        if (!kvp.Value && Int32.TryParse(kvp.Key, out temp) && temp > 0)
                        {
                            collsToSendToCatalog.Add(temp);
                        }
                    }
                }
            }
        }

        private static void ValidateMediaContainedInBundles(int nMediaID, int nGroupID, List<int> distinctSubs, List<int> distinctColls,
            ref List<int> subsRes, ref List<int> collsRes)
        {
            BundlesContainingMediaRequest request = InitializeCatalogRequest(nGroupID, nMediaID, distinctSubs, distinctColls);

            subsRes = new List<int>();
            collsRes = new List<int>();

            BundlesContainingMediaResponse response = request.GetResponse(request) as BundlesContainingMediaResponse;
            if (response != null && response.m_oBundles != null && response.m_oBundles.Count > 0)
            {
                for (int i = 0; i < response.m_oBundles.Count; i++)
                {
                    BundleTriple bt = response.m_oBundles[i];
                    if (bt.m_bIsContained)
                    {
                        switch (bt.m_eBundleType)
                        {
                            case eBundleType.SUBSCRIPTION:
                                subsRes.Add(bt.m_nBundleCode);
                                break;
                            case eBundleType.COLLECTION:
                                collsRes.Add(bt.m_nBundleCode);
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
        }

        private static void GetSubscriptionBundlePurchaseData(DataRow dataRow, string codeColumnName, ref int numOfUses, ref int maxNumOfUses, ref string bundleCode, ref int waiver,
                                                                ref DateTime purchaseDate, ref DateTime endDate, ref int gracePeriodMin, ref bool isSuspend, ref bool isPending)
        {
            numOfUses = ODBCWrapper.Utils.GetIntSafeVal(dataRow["NUM_OF_USES"]);
            maxNumOfUses = ODBCWrapper.Utils.GetIntSafeVal(dataRow["MAX_NUM_OF_USES"]);
            bundleCode = ODBCWrapper.Utils.GetSafeStr(dataRow[codeColumnName]);
            waiver = ODBCWrapper.Utils.GetIntSafeVal(dataRow, "WAIVER");
            purchaseDate = ODBCWrapper.Utils.GetDateSafeVal(dataRow, "CREATE_DATE");
            endDate = ODBCWrapper.Utils.ExtractDateTime(dataRow, "END_DATE");
            gracePeriodMin = ODBCWrapper.Utils.GetIntSafeVal(dataRow["GRACE_PERIOD_MINUTES"]);
            int subscriptionStatus = ODBCWrapper.Utils.GetIntSafeVal(dataRow, "subscription_status");
            isSuspend = SubscriptionPurchaseStatus.Suspended == (SubscriptionPurchaseStatus)subscriptionStatus;
            isPending = ODBCWrapper.Utils.GetIntSafeVal(dataRow, "IS_PENDING") == 1;
        }

        private static void GetCollectionBundlePurchaseData(DataRow dataRow, string codeColumnName, ref int numOfUses, ref int maxNumOfUses,
            ref string bundleCode, ref int waiver, ref DateTime purchaseDate, ref DateTime endDate, ref bool isPending)
        {
            numOfUses = ODBCWrapper.Utils.GetIntSafeVal(dataRow["NUM_OF_USES"]);
            maxNumOfUses = ODBCWrapper.Utils.GetIntSafeVal(dataRow["MAX_NUM_OF_USES"]);
            bundleCode = ODBCWrapper.Utils.GetSafeStr(dataRow[codeColumnName]);
            waiver = ODBCWrapper.Utils.GetIntSafeVal(dataRow, "WAIVER");
            purchaseDate = ODBCWrapper.Utils.GetDateSafeVal(dataRow, "CREATE_DATE");
            endDate = ODBCWrapper.Utils.ExtractDateTime(dataRow, "END_DATE");
            isPending = ODBCWrapper.Utils.GetIntSafeVal(dataRow["is_pending"]) == 1;
        }

        private static bool IsBundlesDataSetValid(DataSet ds)
        {
            return ds != null && ds.Tables != null && ds.Tables.Count == 2;
        }

        internal static Price CopyPrice(Price toCopy)
        {
            if (toCopy == null) return null;
            Price ret = new Price
            {
                m_dPrice = toCopy.m_dPrice,
                m_oCurrency = toCopy.m_oCurrency,
                countryId = toCopy.countryId
            };
            return ret;
        }

        internal static Price CalculateCouponDiscount(ref Price pModule, CouponsGroup couponsGroup, ref string couponCode, int groupID, long domainId, string countryCode)
        {
            Price price = CopyPrice(pModule);
            if (!string.IsNullOrEmpty(couponCode))
            {
                CouponDataResponse theCouponData = Pricing.Module.GetCouponStatus(groupID, couponCode, domainId);

                if (couponsGroup != null &&
                    theCouponData != null &&
                    theCouponData.Status != null &&
                    theCouponData.Status.Code == (int)eResponseStatus.OK &&
                    theCouponData.Coupon != null &&
                    theCouponData.Coupon.m_CouponStatus == CouponsStatus.Valid &&
                    theCouponData.Coupon.m_oCouponGroup.m_sGroupCode == couponsGroup.m_sGroupCode)
                {
                    // if it is a valid gift card, set price to be 0
                    if (theCouponData.Coupon.m_oCouponGroup.couponGroupType == CouponGroupType.GiftCard)
                    {
                        price.m_dPrice = 0.0;
                    }
                    else
                    {
                        //Coupon discount should take place
                        var discountModule = Pricing.Module.Instance.GetDiscountCodeDataByCountryAndCurrency(groupID,
                            couponsGroup.m_oDiscountCode.m_nObjectID, countryCode, pModule.m_oCurrency.m_sCurrencyCD3);

                        price = Instance.GetPriceAfterDiscount(price, discountModule ?? couponsGroup.m_oDiscountCode, 0);
                    }
                }
                else //the coupon is not valid
                {
                    couponCode = string.Empty;
                }
            }

            return price;
        }

        private static bool IsVoucherValid(int nLifeCycle, long nOwnerGuid, long campaignID)
        {
            bool retVal = false;
            ODBCWrapper.DataSetSelectQuery selectQuery = null;
            try
            {
                selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select CREATE_DATE,getdate() as dNow from campaigns_uses with (nolock) where ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("campaign_id", "=", campaignID);
                selectQuery += " and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_GUID", "=", nOwnerGuid);
                selectQuery += " order by id desc";
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        DateTime dNow = (DateTime)(selectQuery.Table("query").DefaultView[0].Row["dNow"]);
                        DateTime dUsed = (DateTime)(selectQuery.Table("query").DefaultView[0].Row["CREATE_DATE"]);
                        if ((dNow - dUsed).TotalMinutes < nLifeCycle)
                            retVal = true;
                    }
                }
            }
            finally
            {
                if (selectQuery != null)
                {
                    selectQuery.Finish();
                }
            }
            return retVal;
        }


        internal static Price CalculateMediaFileFinalPriceNoSubs(Int32 nMediaFileID, int mediaID, Price pModule,
            DiscountModule discModule, CouponsGroup oCouponsGroup, string sSiteGUID,
            string sCouponCode, Int32 nGroupID, string subCode, out DateTime? dtDiscountEnd, long domainId, string countryCode)
        {
            dtDiscountEnd = null;
            Price p = CopyPrice(pModule);
            if (discModule != null)
            {
                int nPPVPurchaseCount = 0;
                if (discModule.m_oWhenAlgo.m_nNTimes > 0)
                {
                    if (discModule.m_dPercent == 100 && !string.IsNullOrEmpty(subCode))
                    {
                        nPPVPurchaseCount = ConditionalAccessDAL.Get_SubscriptionUseCount(sSiteGUID, subCode, nGroupID);
                    }
                    else
                    {
                        nPPVPurchaseCount = ConditionalAccessDAL.Get_PPVPurchaseCount(nGroupID, sSiteGUID, subCode, nMediaFileID);
                    }
                }

                p = Instance.GetPriceAfterDiscount(p, discModule, nPPVPurchaseCount);

                dtDiscountEnd = discModule.m_dEndDate;
            }
            else if (!string.IsNullOrEmpty(subCode))
            {
                //BEO-10342 - 100% discount for subs
                p.m_dPrice = 0;
            }

            if (sCouponCode.Length > 0)
            {
                CouponDataResponse theCouponData = Pricing.Module.GetCouponStatus(nGroupID, sCouponCode, domainId);

                if (oCouponsGroup == null ||
                    theCouponData == null ||
                    theCouponData.Status == null ||
                    theCouponData.Status.Code != (int)eResponseStatus.OK ||
                    theCouponData.Coupon == null)
                {

                }
                else if (theCouponData.Coupon.m_CouponType == CouponType.Voucher &&
                        theCouponData.Coupon.m_campID > 0 &&
                        theCouponData.Coupon.m_ownerMedia == mediaID)
                {
                    bool isCampaignValid = false;
                    Core.Pricing.Campaign camp = Core.Pricing.Module.GetCampaignData(nGroupID, theCouponData.Coupon.m_campID);

                    if (camp != null && camp.m_ID == theCouponData.Coupon.m_campID)
                    {
                        int nViewLS = camp.m_usageModule.m_tsViewLifeCycle;
                        long ownerGuid = theCouponData.Coupon.m_ownerGUID;
                        isCampaignValid = IsVoucherValid(nViewLS, ownerGuid, theCouponData.Coupon.m_campID);
                    }

                    if (isCampaignValid)
                    {
                        var discountModule = Pricing.Module.Instance.GetDiscountCodeDataByCountryAndCurrency(nGroupID,
                            theCouponData.Coupon.m_oCouponGroup.m_oDiscountCode.m_nObjectID, countryCode, pModule.m_oCurrency.m_sCurrencyCD3);

                        p = Instance.GetPriceAfterDiscount(p, discountModule ?? theCouponData.Coupon.m_oCouponGroup.m_oDiscountCode, 1);
                    }
                }
                // If it is a gift card - it should be free
                else if (theCouponData.Coupon.m_CouponStatus == CouponsStatus.Valid &&
                    theCouponData.Coupon.m_oCouponGroup.couponGroupType == CouponGroupType.GiftCard)
                {
                    p.m_dPrice = 0;
                }
                else if (theCouponData.Coupon.m_CouponStatus == CouponsStatus.Valid &&
                        theCouponData.Coupon.m_oCouponGroup.m_sGroupCode == oCouponsGroup.m_sGroupCode)
                {
                    //Coupon discount should take place
                    var discountModule = Pricing.Module.Instance.GetDiscountCodeDataByCountryAndCurrency(nGroupID,
                            oCouponsGroup.m_oDiscountCode.m_nObjectID, countryCode, pModule.m_oCurrency.m_sCurrencyCD3);
                    p = Instance.GetPriceAfterDiscount(p, discountModule ?? oCouponsGroup.m_oDiscountCode, 0);
                }
            }

            return p;
        }

        private static Price GetMediaFileFinalPriceNoSubs(Int32 nMediaFileID, int mediaID, PPVModule ppvModule, string sSiteGUID, string sCouponCode, 
                                                          Int32 nGroupID, string subCode, out DateTime? dtDiscountEnd, long domainId, string countryCode)
        {
            Price pModule = ObjectCopier.Clone(ppvModule.m_oPriceCode.m_oPrise);
            DiscountModule discModule = ObjectCopier.Clone(ppvModule.m_oDiscountModule);
            CouponsGroup couponGroups = ObjectCopier.Clone(ppvModule.m_oCouponsGroup);

            return CalculateMediaFileFinalPriceNoSubs(nMediaFileID, mediaID, pModule, discModule, couponGroups, sSiteGUID, sCouponCode, nGroupID, subCode, 
                out dtDiscountEnd, domainId, countryCode);
        }

        internal static Price GetSubscriptionFinalPrice(int groupId, string subCode, string userId, string couponCode, ref PriceReason theReason, ref Subscription subscription,
                                                        string countryCode, string languageCode, string udid)
        {
            string ip = string.Empty;

            var fullPrice = GetSubscriptionFullPrice(groupId, subCode, userId, couponCode, ref subscription, countryCode, languageCode, udid, ip);

            theReason = fullPrice.PriceReason;

            return fullPrice.FinalPrice;
        }

        internal static Price GetSubscriptionFinalPrice(int groupId, string subCode, string userId, string couponCode, ref PriceReason theReason, ref Subscription subscription,
                                                        string countryCode, string languageCode, string udid, string ip, string currencyCode = null, bool isSubscriptionSetModifySubscription = false)
        {
            var fullPrice = GetSubscriptionFullPrice(groupId, subCode, userId, couponCode, ref subscription, countryCode, languageCode, udid, ip,
                                                        currencyCode, isSubscriptionSetModifySubscription);

            theReason = fullPrice.PriceReason;

            return fullPrice.FinalPrice;
        }

        internal static FullPrice GetSubscriptionFullPrice(int groupId, string subCode, string userId, string couponCode, ref Subscription subscription,
                                                        string countryCode, string languageCode, string udid, string ip, string currencyCode = null,
                                                        bool isSubscriptionSetModifySubscription = false, BlockEntitlementType blockEntitlement = BlockEntitlementType.NONE)
        {
            //create web service pricing insatance
            FullPrice fullPrice = new FullPrice()
            {
                PriceReason = PriceReason.UnKnown,
                CouponCode = couponCode,
                CouponRemainder = 0,
                SubscriptionCycle = new SubscriptionCycle()
            };

            try
            {
                subscription = Pricing.Module.Instance.GetSubscriptionData(groupId, subCode, countryCode, languageCode, udid, false, userId);
                if (subscription == null)
                {
                    return fullPrice;
                }
                
                if(subscription.IsActive.HasValue && !subscription.IsActive.Value)
                {
                    fullPrice.PriceReason = PriceReason.NotForPurchase;
                    return fullPrice;
                }

                if (subscription.PreSaleDate.HasValue)
                {
                    if (subscription.PreSaleDate.Value > DateTime.UtcNow)
                    {
                        fullPrice.PriceReason = PriceReason.NotForPurchase;
                        return fullPrice;
                    }
                }
                else if (subscription.m_dStartDate > DateTime.UtcNow)
                {
                    fullPrice.PriceReason = PriceReason.NotForPurchase;
                    return fullPrice;
                }

                if (subscription.m_dEndDate < DateTime.UtcNow)
                {
                    fullPrice.PriceReason = PriceReason.NotForPurchase;
                    return fullPrice;
                }

                bool isGeoCommerceBlock = false;
                if (!string.IsNullOrEmpty(ip))
                {
                    isGeoCommerceBlock = IsGeoBlock(groupId, subscription.n_GeoCommerceID, ip);
                }

                if (isGeoCommerceBlock)
                {
                    fullPrice.PriceReason = PriceReason.GeoCommerceBlocked;
                    return fullPrice;
                }

                PriceReason theReason = fullPrice.PriceReason;
                DiscountModule externalDiscount;
                PriceCode priceCode = subscription.m_oSubscriptionPriceCode;
                fullPrice.OriginalPrice = HandlePriceCodeAndExternalDiscount(ref theReason, groupId, ref currencyCode, ref countryCode, subscription.m_oExtDisountModule,
                    out externalDiscount, ref priceCode, blockEntitlement == BlockEntitlementType.BLOCK_SUBSCRIPTION, ip, userId);

                fullPrice.FinalPrice = CopyPrice(fullPrice.OriginalPrice);

                subscription.m_oSubscriptionPriceCode = priceCode;
                fullPrice.PriceReason = theReason;

                if (fullPrice.PriceReason != PriceReason.ForPurchase)
                {
                    return fullPrice;
                }

                bool blockDoublePurchase = false;
                DomainSuspentionStatus userSuspendStatus = DomainSuspentionStatus.OK;
                int domainId = 0;
                if (Utils.IsUserValid(userId, groupId, ref domainId, ref userSuspendStatus))
                {
                    DomainBundles domainBundles = GetDomainBundles(groupId, domainId);
                    if (domainBundles != null && domainBundles.EntitledSubscriptions != null && domainBundles.EntitledSubscriptions.ContainsKey(subCode))
                    {
                        fullPrice.PriceReason = domainBundles.EntitledSubscriptions[subCode][0].isPending ? PriceReason.PendingEntitlement : PriceReason.SubscriptionPurchased;

                        if (subscription.m_bIsRecurring)
                        {
                            fullPrice.FinalPrice.m_dPrice = 0.0;
                        }
                        else if (domainBundles.EntitledSubscriptions[subCode].Count == 1)
                        {
                            object dbBlockDoublePurchase = ODBCWrapper.Utils.GetTableSingleVal("groups_parameters", "BLOCK_DOUBLE_PURCHASE", "GROUP_ID", "=", groupId, 60 * 60 * 24, "billing_connection");
                            if (dbBlockDoublePurchase != null && dbBlockDoublePurchase != DBNull.Value && ODBCWrapper.Utils.GetIntSafeVal(dbBlockDoublePurchase) == 1)
                            {
                                blockDoublePurchase = true;
                                fullPrice.FinalPrice.m_dPrice = 0.0;
                            }
                        }
                        else
                        {
                            fullPrice.FinalPrice.m_dPrice = -1;
                        }
                    }
                }

                if ((fullPrice.PriceReason != PriceReason.SubscriptionPurchased && fullPrice.PriceReason != PriceReason.PendingEntitlement) || !blockDoublePurchase)
                {
                    Price finalPrice = fullPrice.FinalPrice;
                    if (!isSubscriptionSetModifySubscription && subscription.m_oPreviewModule != null &&
                        IsEntitledToPreviewModule(userId, groupId, subCode, subscription, ref finalPrice, ref theReason, domainId))
                    {
                        fullPrice.FinalPrice = finalPrice;
                        fullPrice.PriceReason = theReason;
                        var subscriptionCycle = CalcSubscriptionCycle(groupId, subscription, domainId);

                        //BEO-9091
                        if (subscriptionCycle != null &&
                            subscription.m_oPreviewModule.m_tsFullLifeCycle <= subscription.m_MultiSubscriptionUsageModule[0].m_tsMaxUsageModuleLifeCycle)
                        {
                            fullPrice.SubscriptionCycle = subscriptionCycle;
                        }

                        //search for campaign
                        Price lowestPrice = CopyPrice(fullPrice.OriginalPrice);
                        fullPrice.CampaignDetails = GetValidCampaign(groupId, domainId, fullPrice.OriginalPrice, ref lowestPrice, eTransactionType.Subscription,
                            currencyCode, long.Parse(subCode), countryCode, couponCode, 0, null);
                        if (fullPrice.CampaignDetails != null)
                        {
                            fullPrice.CouponCode = couponCode;
                            fullPrice.FinalPrice = lowestPrice;
                        }
                        return fullPrice;
                    }

                    Price discountPrice = null;
                    if (externalDiscount != null)
                    {
                        discountPrice = Instance.GetPriceAfterDiscount(finalPrice, externalDiscount, 0);
                    }

                    if (domainId > 0)
                    {
                        Price lowestPrice = CopyPrice(discountPrice) ?? CopyPrice(fullPrice.FinalPrice);
                        fullPrice.CampaignDetails = GetValidCampaign(groupId, domainId, fullPrice.OriginalPrice, ref lowestPrice, eTransactionType.Subscription, currencyCode, 
                            long.Parse(subCode), countryCode, couponCode, 0, null);

                        if (fullPrice.CampaignDetails != null)
                        {
                            fullPrice.FinalPrice = lowestPrice;
                            fullPrice.CouponCode = couponCode;

                            CalcPriceAndCampaignRemainderByUnifiedBillingCycle(groupId, subscription, false, domainId, ref fullPrice);
                            return fullPrice;
                        }
                    }

                    fullPrice.FinalPrice = PriceManager.GetLowestPrice(groupId, fullPrice.FinalPrice, domainId, discountPrice, eTransactionType.Subscription, currencyCode, long.Parse(subCode),
                                                countryCode, ref couponCode, subscription.m_oCouponsGroup, subscription.GetValidSubscriptionCouponGroup(), null);

                    fullPrice.CouponCode = couponCode;

                    if (fullPrice.FinalPrice != null && fullPrice.OriginalPrice != null)
                    {
                        var subscriptionCycle = fullPrice.SubscriptionCycle;

                        var finalPriceAndCouponRemainder =
                            CalcPriceAndCouponRemainderByUnifiedBillingCycle(fullPrice.OriginalPrice.m_dPrice, couponCode, fullPrice.FinalPrice.m_dPrice, ref subscriptionCycle,
                                                                             groupId, subscription, false, domainId);

                        fullPrice.FinalPrice.m_dPrice = finalPriceAndCouponRemainder.Item1;
                        fullPrice.CouponRemainder = finalPriceAndCouponRemainder.Item2;
                        fullPrice.SubscriptionCycle = subscriptionCycle;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("GetSubscriptionFinalPrice failed, groupId: {0}, subCode: {1}, userId: {2}, couponCode: {3}, countryCode: {4}, languageCode: {5}, udid: {6}, ip: {7}, currency: {8}",
                    groupId, subCode, userId, couponCode, countryCode, languageCode, udid, !string.IsNullOrEmpty(ip) ? ip : string.Empty, !string.IsNullOrEmpty(currencyCode) ? currencyCode : string.Empty), ex);
            }

            return fullPrice;
        }

        private static SubscriptionCycle CalcSubscriptionCycle(int groupId, Subscription subscription, int domainId)
        {
            var subscriptionCycle = new SubscriptionCycle();

            //chcek that subscription contain this group billing cycle and subscription is renew                                             
            if (subscription != null && subscription.m_bIsRecurring && !subscription.PreSaleDate.HasValue &&
              subscription.m_MultiSubscriptionUsageModule != null && subscription.m_MultiSubscriptionUsageModule.Count() == 1 /*only one price plan*/)
            {
                var maxUsageModuleLifeCycle = (long)subscription.m_MultiSubscriptionUsageModule[0].m_tsMaxUsageModuleLifeCycle;
                subscriptionCycle = GetSubscriptionCycle(groupId, domainId, maxUsageModuleLifeCycle);
            }

            return subscriptionCycle;
        }

        public static SubscriptionCycle GetSubscriptionCycle(int groupId, int domainId, long maxUsageModuleLifeCycle)
        {
            var subscriptionCycle = new SubscriptionCycle();
            subscriptionCycle.SubscriptionLifeCycle = new Duration(maxUsageModuleLifeCycle);

            var paymentConfigurationResponse = PartnerConfigurationManager.GetPaymentConfig(groupId);
            PaymentPartnerConfig paymentConfig = null;
            if (paymentConfigurationResponse.HasObject() && paymentConfigurationResponse.Object.UnifiedBillingCycles != null)
            {
                paymentConfig = paymentConfigurationResponse.Object;
                var unifiedBillingCycleObj = paymentConfig.UnifiedBillingCycles.FirstOrDefault(x => x.Duration.Equals(subscriptionCycle.SubscriptionLifeCycle));

                if (unifiedBillingCycleObj != null)
                {
                    //get key from CB household_renewBillingCyclepublic class Subscription : PPVModule
                    subscriptionCycle.HasCycle = true;
                    subscriptionCycle.PaymentGatewayId = unifiedBillingCycleObj.PaymentGatewayId ?? 0;
                    subscriptionCycle.IgnorePartialBilling = unifiedBillingCycleObj.IgnorePartialBilling ?? false;
                }
            }

            if (!subscriptionCycle.HasCycle)
            {
                long? groupUnifiedBillingCycle = GetGroupUnifiedBillingCycle(groupId);
                if (groupUnifiedBillingCycle.HasValue && maxUsageModuleLifeCycle == groupUnifiedBillingCycle.Value)
                {
                    subscriptionCycle.HasCycle = true;
                }
            }

            if (subscriptionCycle.HasCycle)
            {
                subscriptionCycle.UnifiedBillingCycle = TryGetHouseholdUnifiedBillingCycle(domainId, maxUsageModuleLifeCycle);
            }

            return subscriptionCycle;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="originalPrice"></param>
        /// <param name="couponCode"></param>
        /// <param name="finalPrice"></param>
        /// <param name="unifiedBillingCycle"></param>
        /// <param name="groupId"></param>
        /// <param name="subscription"></param>
        /// <param name="isFirstTimePreviewModuleEnd"></param>
        /// <param name="domainId"></param>
        /// <returns>item1 = priceAfterUnified, item2 = couponRemainder</returns>
        internal static Tuple<double, double> CalcPriceAndCouponRemainderByUnifiedBillingCycle(double originalPrice, string couponCode, double finalPrice,
            ref SubscriptionCycle subscriptionCycle, int groupId, Subscription subscription, bool isFirstTimePreviewModuleEnd, int domainId)
        {
            log.DebugFormat("CalcPriceAndCouponRemainderByUnifiedBillingCycle - {0}, original Price:{1}, price after discount and coupon:{2}.",
                            subscription != null ? subscription.ToString() : "Subscription:null", originalPrice, finalPrice);

            double couponRemainder = 0;
            bool fullCouponDiscount = (!string.IsNullOrEmpty(couponCode) && originalPrice > 0 && finalPrice == 0);
            double priceBeforeUnified;
            if (fullCouponDiscount)
            {
                priceBeforeUnified = originalPrice;
            }
            else
            {
                priceBeforeUnified = finalPrice;
            }

            double priceAfterUnified = priceBeforeUnified;
            if (subscriptionCycle == null || subscriptionCycle.UnifiedBillingCycle == null)
            {
                subscriptionCycle = CalcSubscriptionCycle(groupId, subscription, domainId);
            }

            // check that end date between next end date and unified billing cycle end date are different
            if (!subscriptionCycle.IgnorePartialBilling && subscriptionCycle.HasCycle && subscriptionCycle.UnifiedBillingCycle != null && subscriptionCycle.UnifiedBillingCycle.endDate > DateUtils.DateTimeToUtcUnixTimestampMilliseconds(DateTime.UtcNow))
            {
                DateTime nextRenew = Utils.GetEndDateTime(subscriptionCycle.SubscriptionLifeCycle, DateTime.UtcNow);
                int numOfUnitsByBillingCycle = 1;
                int numOfUnitsForSubscription = 1;

                bool dayCycle = subscriptionCycle.SubscriptionLifeCycle.Unit == DurationUnit.Days && subscriptionCycle.SubscriptionLifeCycle.Value <= 1;

                if (dayCycle)
                {
                    numOfUnitsForSubscription = (int)Math.Ceiling((nextRenew - DateTime.UtcNow).TotalHours);
                    numOfUnitsByBillingCycle = (int)Math.Ceiling((DateUtils.UtcUnixTimestampMillisecondsToDateTime(subscriptionCycle.UnifiedBillingCycle.endDate) - DateTime.UtcNow).TotalHours);
                }
                else
                {
                    numOfUnitsForSubscription = (int)Math.Ceiling((nextRenew - DateTime.UtcNow).TotalDays);
                    numOfUnitsByBillingCycle = (int)Math.Ceiling((DateUtils.UtcUnixTimestampMillisecondsToDateTime(subscriptionCycle.UnifiedBillingCycle.endDate) - DateTime.UtcNow).TotalDays);
                }

                priceAfterUnified = Math.Round(numOfUnitsByBillingCycle * (priceBeforeUnified / numOfUnitsForSubscription), 2);

                // check if need to calc couponRemainder by PreviewModule
                if (isFirstTimePreviewModuleEnd && subscription != null && subscription.m_oPreviewModule != null)
                {
                    var totalRemainUnits = numOfUnitsForSubscription - numOfUnitsByBillingCycle;
                    if (totalRemainUnits > 0)
                    {
                        var previewDuration = new Duration(subscription.m_oPreviewModule.m_tsFullLifeCycle);
                        bool dayPreviewCycle = previewDuration.Unit == DurationUnit.Days && previewDuration.Value <= 1;
                        int unitsWithPreviewModel = 24;
                        if (!dayPreviewCycle)
                        {
                            // days
                            unitsWithPreviewModel = subscription.m_oPreviewModule.m_tsFullLifeCycle / 60 / 24;
                        }

                        couponRemainder = Math.Round((priceBeforeUnified - priceAfterUnified) / totalRemainUnits * unitsWithPreviewModel, 2);
                    }
                }
                else
                {
                    couponRemainder = priceBeforeUnified - priceAfterUnified;
                }

                //log.Debug($"CalcPriceAndCouponRemainderByUnifiedBillingCycle - [nextRenewDate:{nextRenew.ToLongDateString()}, numOfDaysForSubscription:{numOfDaysForSubscription}]," +
                //          $"[unifiedBillingCycle.endDate:{subscriptionCycle.UnifiedBillingCycle.endDate}, numOfDaysByBillingCycle:{numOfDaysByBillingCycle}]");
            }

            if (fullCouponDiscount)
            {
                priceAfterUnified = finalPrice; // set to 0 because full Coupon Discount
            }

            log.Debug($"CalcPriceAndCouponRemainderByUnifiedBillingCycle - price after unified:{priceAfterUnified}, coupon remainder:{couponRemainder}");
            return new Tuple<double, double>(priceAfterUnified, couponRemainder);
        }

        internal static void CalcPriceAndCampaignRemainderByUnifiedBillingCycle(int groupId, Subscription subscription, bool isFirstTimePreviewModuleEnd,
            int domainId, ref FullPrice fullPrice)
        {
            log.DebugFormat("CalcPriceAndCampaignRemainderByUnifiedBillingCycle - {0}, original Price:{1}, price after discount and campaign:{2}.",
                            subscription != null ? subscription.ToString() : "Subscription:null", fullPrice.OriginalPrice.m_dPrice, fullPrice.FinalPrice.m_dPrice);

            var subscriptionCycle = fullPrice.SubscriptionCycle;

            bool fullDiscount = fullPrice.FinalPrice.m_dPrice == 0;

            double priceBeforeUnified;
            if (fullDiscount)
            {
                priceBeforeUnified = fullPrice.OriginalPrice.m_dPrice;
            }
            else
            {
                priceBeforeUnified = fullPrice.FinalPrice.m_dPrice;
            }

            double priceAfterUnified = priceBeforeUnified;
            if (subscriptionCycle == null || subscriptionCycle.UnifiedBillingCycle == null)
            {
                subscriptionCycle = CalcSubscriptionCycle(groupId, subscription, domainId);
            }

            // check that end date between next end date and unified billing cycle end date are different
            if (!subscriptionCycle.IgnorePartialBilling && subscriptionCycle.HasCycle && subscriptionCycle.UnifiedBillingCycle != null && subscriptionCycle.UnifiedBillingCycle.endDate > DateUtils.DateTimeToUtcUnixTimestampMilliseconds(DateTime.UtcNow))
            {
                DateTime nextRenew = Utils.GetEndDateTime(subscriptionCycle.SubscriptionLifeCycle, DateTime.UtcNow);
                int numOfUnitsByBillingCycle = 1;
                int numOfUnitsForSubscription = 1;

                bool dayCycle = subscriptionCycle.SubscriptionLifeCycle.Unit == DurationUnit.Days && subscriptionCycle.SubscriptionLifeCycle.Value <= 1;

                if (dayCycle)
                {
                    numOfUnitsForSubscription = (int)Math.Ceiling((nextRenew - DateTime.UtcNow).TotalHours);
                    numOfUnitsByBillingCycle = (int)Math.Ceiling((DateUtils.UtcUnixTimestampMillisecondsToDateTime(subscriptionCycle.UnifiedBillingCycle.endDate) - DateTime.UtcNow).TotalHours);
                }
                else
                {
                    numOfUnitsForSubscription = (int)Math.Ceiling((nextRenew - DateTime.UtcNow).TotalDays);
                    numOfUnitsByBillingCycle = (int)Math.Ceiling((DateUtils.UtcUnixTimestampMillisecondsToDateTime(subscriptionCycle.UnifiedBillingCycle.endDate) - DateTime.UtcNow).TotalDays);
                }

                priceAfterUnified = Math.Round(numOfUnitsByBillingCycle * (priceBeforeUnified / numOfUnitsForSubscription), 2);

                // check if need to calc couponRemainder by PreviewModule
                if (isFirstTimePreviewModuleEnd && subscription != null && subscription.m_oPreviewModule != null)
                {
                    var totalRemainUnits = numOfUnitsForSubscription - numOfUnitsByBillingCycle;
                    if (totalRemainUnits > 0)
                    {
                        var previewDuration = new Duration(subscription.m_oPreviewModule.m_tsFullLifeCycle);
                        bool dayPreviewCycle = previewDuration.Unit == DurationUnit.Days && previewDuration.Value <= 1;
                        int unitsWithPreviewModel = 24;
                        if (!dayPreviewCycle)
                        {
                            // days
                            unitsWithPreviewModel = subscription.m_oPreviewModule.m_tsFullLifeCycle / 60 / 24;
                        }

                        fullPrice.CampaignDetails.Remainder = Math.Round((priceBeforeUnified - priceAfterUnified) / totalRemainUnits * unitsWithPreviewModel, 2);
                    }
                }
                else
                {
                    fullPrice.CampaignDetails.Remainder = priceBeforeUnified - priceAfterUnified;
                }
            }

            if (fullDiscount)
            {
                priceAfterUnified = 0; // set to 0 because full Discount
            }

            fullPrice.FinalPrice.m_dPrice = priceAfterUnified;
            fullPrice.SubscriptionCycle = subscriptionCycle;

            log.Debug($"CalcPriceAndCampaignRemainderByUnifiedBillingCycle - price after unified:{priceAfterUnified}, campaign remainder:{fullPrice.CampaignDetails.Remainder}");
        }


        public static Price HandlePriceCodeAndExternalDiscount(ref PriceReason theReason, int groupId, ref string currencyCode, ref string countryCode,
            DiscountModule externalDiscountModule, out DiscountModule externalDiscount, ref PriceCode priceCode, bool isBlockEntitlementType = false,
            string ip = null, string userId = null)
        {
            Price price = null;
            externalDiscount = externalDiscountModule != null ? ObjectCopier.Clone(externalDiscountModule) : null;

            if (priceCode != null)
            {
                bool isValidCurrencyCode = false;

                // Validate currencyCode if it was passed in the request
                if (!string.IsNullOrEmpty(currencyCode))
                {
                    if (!GeneralPartnerConfigManager.Instance.IsValidCurrencyCode(groupId, currencyCode))
                    {
                        theReason = PriceReason.InvalidCurrency;
                        return new Price();
                    }

                    isValidCurrencyCode = true;
                }

                countryCode = !string.IsNullOrEmpty(ip) ? APILogic.Utils.GetIP2CountryCode(groupId, ip): string.Empty;
                // Get price code according to country and currency (if exists on the request)
                if (!string.IsNullOrEmpty(countryCode) && (isValidCurrencyCode || GeneralPartnerConfigManager.Instance.GetGroupDefaultCurrency(groupId, ref currencyCode)))
                {
                    PriceCode priceCodeWithCurrency = Pricing.Module.GetPriceCodeDataByCountyAndCurrency(groupId, priceCode.m_nObjectID, countryCode, currencyCode);
                    if (priceCodeWithCurrency == null)
                    {
                        theReason = PriceReason.CurrencyNotDefinedOnPriceCode;
                        return new Price();
                    }

                    priceCode = ObjectCopier.Clone(priceCodeWithCurrency);

                    if (externalDiscount != null)
                    {
                        DiscountModule externalDisountWithCurrency =
                            Pricing.Module.Instance.GetDiscountCodeDataByCountryAndCurrency(groupId, externalDiscount.m_nObjectID, countryCode, currencyCode);
                        externalDiscount = externalDisountWithCurrency != null ? ObjectCopier.Clone(externalDisountWithCurrency) : externalDiscount;
                    }
                }

                price = ObjectCopier.Clone(priceCode.m_oPrise);
            }

            if (!string.IsNullOrEmpty(userId) && !userId.Equals("0") && isBlockEntitlementType)
            {
                theReason = PriceReason.UserSuspended;
                return price;
            }

            theReason = PriceReason.ForPurchase;
            return price;
        }
      
        public static List<long> GetDomainSegments(int groupId, long domainId, List<string> userIds)
        {
            List<long> segmentIds = new List<long>();
            if (userIds?.Count > 0)
            {
                //BEO-8004
                string key = $"usersSegmentIds_{domainId}";
                if (!LayeredCache.Instance.TryGetKeyFromCurrentRequest<List<long>>(key, ref segmentIds))
                {
                    segmentIds = new List<long>();
                    foreach (var userId in userIds)
                    {
                        var userSegments = Api.Module.GetUserSegments(groupId, userId, null, 0, 0);
                        if (userSegments != null && userSegments.HasObjects())
                        {
                            segmentIds.AddRange(userSegments.Objects.Select(x => x.SegmentId));
                        }
                    }

                    int totalCount = 0;
                    var householdSegment = HouseholdSegmentLogic.List(groupId, domainId, out totalCount);
                    if (totalCount > 0)
                    {
                        segmentIds.AddRange(householdSegment);
                    }

                    if (segmentIds.Count > 0)
                    {
                        segmentIds = segmentIds.Distinct().ToList();
                    }

                    Dictionary<string, List<long>> resultsToAdd = new Dictionary<string, List<long>>();
                    resultsToAdd.Add(key, segmentIds);
                    LayeredCache.Instance.InsertResultsToCurrentRequest<List<long>>(resultsToAdd, null);
                }
            }

            return segmentIds;
        }

        public static RecurringCampaignDetails GetValidCampaign(int groupId, int domainId, Price originalPrice, ref Price lowestPrice, eTransactionType productType,
            string currencyCode, long productId, string countryCode, string couponCode, long mediaId, List<long> fileTypeIds)
        {
            RecurringCampaignDetails recurringCampaignDetails = null;

            try
            {
                // get all active campaigns with promotion
                var campaignFilter = new CampaignSearchFilter()
                {
                    HasPromotion = true,
                    StateEqual = CampaignState.ACTIVE,
                    IsActiveNow = true
                };
                var campaigns = ApiLogic.Users.Managers.CampaignManager.Instance.SearchCampaigns(new ContextData(groupId) { DomainId = domainId }, campaignFilter);
                if (!campaigns.HasObjects()) { return recurringCampaignDetails; }

                // evaluate all campaigns which are compatible with current price details / purchase details
                var campaignPromotionScope = new BusinessModuleRuleConditionScope()
                {
                    BusinessModuleId = productId,
                    BusinessModuleType = productType,
                    FilterByDate = true,
                    GroupId = groupId,
                    MediaId = mediaId,
                    FileTypeIds = fileTypeIds
                };
                var validCampaigns = campaigns.Objects.Where(x => x.Promotion.EvaluateConditions(campaignPromotionScope)).ToList();
                if (validCampaigns == null || validCampaigns.Count == 0) { return recurringCampaignDetails; }

                //get user campaigns map
                var domainResponse = Domains.Module.GetDomainInfo(groupId, domainId);
                long userId = domainResponse.Domain.m_masterGUIDs.FirstOrDefault();
                var userCampaigns = CampaignUsageRepository.Instance.GetCampaignInboxMessageMapCB(groupId, userId);

                var batchCampaignScope = new BatchCampaignConditionScope();

                ApiObjects.Campaign lowestCampaign = null;
                var promotionEvaluator = new PromotionEvaluator(Pricing.Module.Instance, Instance, groupId, domainId,
                    countryCode, currencyCode, couponCode, originalPrice);

                foreach (var promotedCampaign in validCampaigns)
                {
                    string triggerUdid = null;
                    var campaignAssignedToUser = userCampaigns.Campaigns.ContainsKey(promotedCampaign.Id);
                    if (campaignAssignedToUser)
                    {
                        // handle anti fraud for Subscription only
                        if (productType == eTransactionType.Subscription && userCampaigns.Campaigns[promotedCampaign.Id].SubscriptionUses.ContainsKey(productId)) //Don't allow campaign usage
                        {
                            continue;
                        }

                        if (promotedCampaign.CampaignType == eCampaignType.Trigger && userCampaigns.Campaigns[promotedCampaign.Id].Devices.Count > 0)
                        {
                            foreach (var udid in userCampaigns.Campaigns[promotedCampaign.Id].Devices)
                            {
                                if (IsDeviceInDomain(domainResponse.Domain, udid))
                                {
                                    var deviceTriggerCampainsUses = CampaignUsageRepository.Instance.GetDeviceTriggerCampainsUses(groupId, udid);
                                    if (deviceTriggerCampainsUses == null || !deviceTriggerCampainsUses.Uses.ContainsKey(promotedCampaign.Id))
                                    {
                                        triggerUdid = udid;
                                        break;
                                    }
                                }
                            }

                            //Don't allow campaign usage
                            if (string.IsNullOrEmpty(triggerUdid)) { continue; }
                        }
                    }

                    // save batch campaign to user in lazy
                    if (!campaignAssignedToUser && promotedCampaign.CampaignType == eCampaignType.Batch)
                    {
                        if (promotedCampaign.GetConditions()?.Count > 0)
                        {
                            if (!batchCampaignScope.FilterBySegments)
                            {
                                List<string> allUserIdsInDomain = Domains.Module.GetDomainUserList(groupId, domainId);
                                batchCampaignScope.SegmentIds = GetDomainSegments(groupId, domainId, allUserIdsInDomain);
                                batchCampaignScope.FilterBySegments = true;
                            }
                        }

                        if (promotedCampaign.EvaluateConditions(batchCampaignScope))
                        {
                            campaignAssignedToUser = true;
                            Task.Run(() => Notification.MessageInboxManger.Instance.AddCampaignMessageToUser(promotedCampaign, groupId, userId));
                        }
                    }

                    if (campaignAssignedToUser)
                    {
                        var tempPrice = promotionEvaluator.Evaluate(promotedCampaign.Promotion, promotedCampaign.Id);
                        if (tempPrice == null) { continue; }

                        bool isLowest = false;
                        if (tempPrice.m_dPrice < lowestPrice.m_dPrice)
                        {
                            lowestPrice = tempPrice;
                            isLowest = true;
                        }
                        else if (tempPrice.m_dPrice == lowestPrice.m_dPrice)
                        {
                            int numberOfRecurring = lowestCampaign == null ? -1 : lowestCampaign.Promotion.GetNumberOfRecurring();
                            int newNumberOfRecurring = promotedCampaign.Promotion.GetNumberOfRecurring();

                            isLowest = lowestCampaign == null || (newNumberOfRecurring > numberOfRecurring) ||
                                (newNumberOfRecurring == numberOfRecurring && promotedCampaign.EndDate > lowestCampaign.EndDate);
                        }

                        if (isLowest)
                        {
                            lowestCampaign = promotedCampaign;
                            var numberOfRecurring = lowestCampaign.Promotion.GetNumberOfRecurring();
                            if (numberOfRecurring < 0)
                            {
                                numberOfRecurring = 0;
                            }

                            recurringCampaignDetails = new RecurringCampaignDetails
                            {
                                Id = lowestCampaign.Id,
                                LeftRecurring = numberOfRecurring,
                                Udid = triggerUdid,
                                CampaignEndDate = lowestCampaign.EndDate
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("GetValidCampaign", ex);
            }

            return recurringCampaignDetails;
        }

        internal static void GetMultiSubscriptionUsageModule(List<RenewDetails> rsDetails, string userIp, Dictionary<long, Subscription> subscriptions, BaseConditionalAccess cas,
                                                             ref SubscriptionCycle subscriptionCycle, int householdId, int groupId, bool isRenew = true)
        {
            try
            {
                // get MPP
                int recPeriods = 0;
                bool isMPPRecurringInfinitely = false;
                string previousPurchaseCurrencyCode = string.Empty;
                List<RenewDetails> rsDetailsToRemove = new List<RenewDetails>();

                foreach (RenewDetails rsDetail in rsDetails)
                {
                    rsDetail.PreviousPurchaseCurrencyCode = rsDetail.Currency;
                    var subscription = subscriptions[rsDetail.ProductId];
                    rsDetail.DomainId = householdId;
                    rsDetail.GroupId = groupId;
                    if (!cas.GetMultiSubscriptionUsageModule(rsDetail, userIp, ref recPeriods, ref isMPPRecurringInfinitely, subscription, ref subscriptionCycle, groupId, true, isRenew))
                    {
                        // "Error while trying to get Price plan
                        log.ErrorFormat("Error while trying to get Price plan to renew productId : {0}, purchaseId : {1}, householdId : {2}", rsDetail.ProductId, rsDetail.PurchaseId, householdId);
                        //save object to remove + continue 
                        rsDetailsToRemove.Add(rsDetail);
                        continue;
                    }

                    rsDetail.GracePeriodMinutes = subscription.m_GracePeriodMinutes;
                }

                // remove if needed
                rsDetails.RemoveAll(x => rsDetailsToRemove.Contains(x));
            }
            catch (Exception ex)
            {
                log.ErrorFormat("GetMultiSubscriptionUsageModule failed ex = {0}", ex);
            }
        }

        internal static UnifiedBillingCycle TryGetHouseholdUnifiedBillingCycle(int domainId, long renewLifeCycle)
        {
            UnifiedBillingCycle unifiedBillingCycle = null;
            try
            {
                // save in CB 
                unifiedBillingCycle = UnifiedBillingCycleManager.GetDomainUnifiedBillingCycle(domainId, renewLifeCycle);

                if (unifiedBillingCycle == null)
                {
                    log.DebugFormat(string.Format("TryGetHouseholdUnifiedBillingCycle - no billingCycle found for domainId={0} renewLifeCycle={1}", domainId, renewLifeCycle));
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("TryGetUnifiedBillingCycle - Failed for domainID = {0}, billingCycle= {1} due ex ={2}", domainId, renewLifeCycle, ex.Message);
                return null;
            }
            return unifiedBillingCycle;
        }

        internal static long GetUnifiedProcessId(int groupId, int paymentGatewayId, DateTime endDate, long householdId, long cycle, out bool isNew, ProcessUnifiedState processPurchasesState = ProcessUnifiedState.Renew)
        {
            long processId = 0;
            isNew = false;
            try
            {
                DataTable dt = ConditionalAccessDAL.GetUnifiedProcessId(groupId, paymentGatewayId, endDate, householdId, cycle, (int)processPurchasesState);
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    processId = ODBCWrapper.Utils.GetLongSafeVal(dt.Rows[0], "id");
                }

                if (processId == 0)
                {
                    // insert new one to DB 
                    processId = ConditionalAccessDAL.InsertUnifiedProcess(groupId, paymentGatewayId, endDate, householdId, cycle, (int)processPurchasesState);
                    if (processId > 0)
                    {
                        isNew = true;
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("GetProcessPurchasesId - Failed for groupId = {0}, paymentGatewayID= {1} endDate={2}, householdId={3}, ex={4}", groupId, paymentGatewayId, endDate,
                    householdId, ex.Message);
            }
            return processId;
        }

        public static long? GetGroupUnifiedBillingCycle(int groupId)
        {
            long? unifiedBillingCycle = null;
            try
            {
                // get unified billing cycle for group from cach / DB
                string key = LayeredCacheKeys.GetGroupUnifiedBillingCycleKey(groupId);
                List<string> inValidationKeys = new List<string>();
                inValidationKeys.Add(LayeredCacheKeys.GetGroupUnifiedBillingCycleInvalidationKey(groupId));

                // try to get from cache            
                bool cacheResult = LayeredCache.Instance.Get<long?>(key, ref unifiedBillingCycle, Get_GroupUnifiedBillingCycle, new Dictionary<string, object>() { { "groupId", groupId } },
                                                                                groupId, LayeredCacheConfigNames.GET_GROUP_UNIFIED_BILLING_CYCLE, inValidationKeys);

                if (!cacheResult)
                {
                    log.Error(string.Format("TryGetGroupUnifiedBillingCycle - Failed get data from cache groupId={0}", groupId));
                    return null;
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("TryGetGroupUnifiedBillingCycle - Failed for groupId = {0} due ex = {1}", groupId, ex.Message);
                return null;
            }
            return unifiedBillingCycle;
        }

        private static Tuple<long?, bool> Get_GroupUnifiedBillingCycle(Dictionary<string, object> funcParams)
        {
            bool res = false;
            long? unifiedBillingCycle = null;
            try
            {
                if (funcParams != null && funcParams.Count == 1 && funcParams.ContainsKey("groupId"))
                {
                    int? groupId = funcParams["groupId"] as int?;
                    if (groupId.HasValue)
                    {
                        DataRow dr = BillingDAL.GetGroupParameters(groupId.Value);
                        if (dr != null)
                        {
                            unifiedBillingCycle = ODBCWrapper.Utils.GetLongSafeVal(dr, "unified_billing_cycle_period");
                            if (unifiedBillingCycle == 0)
                            {
                                unifiedBillingCycle = null;
                            }
                            res = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Get_GroupUnifiedBillingCycle failed, parameters : {0}", string.Join(";", funcParams.Keys)), ex);
            }

            return new Tuple<long?, bool>(unifiedBillingCycle, res);
        }


        internal static FullPrice GetCollectionFinalPrice(Int32 groupId, string sColCode, string sSiteGUID, string couponCode, ref Collection collection,
            string countryCode, string sLANGUAGE_CODE, string sDEVICE_NAME, string connStr, string ip, string currencyCode = null, BlockEntitlementType blockEntitlement = BlockEntitlementType.NONE)
        {
            var fullPrice = new FullPrice()
            {
                PriceReason = PriceReason.UnKnown,
                CouponCode = couponCode,
                CouponRemainder = 0,
            };

            collection = Pricing.Module.Instance.GetCollectionData(groupId, sColCode, countryCode, sLANGUAGE_CODE, sDEVICE_NAME, false);
            if (collection == null)
            {
                return fullPrice;
            }

            // get user status and validity if needed
            int domainID = 0;
            DomainSuspentionStatus userSuspendStatus = DomainSuspentionStatus.Suspended;
            bool isUserValidRes = IsUserValid(sSiteGUID, groupId, ref domainID, ref userSuspendStatus);

            PriceReason priceReason = fullPrice.PriceReason;
            DiscountModule externalDiscount;
            PriceCode priceCode = collection.m_oCollectionPriceCode;
            fullPrice.OriginalPrice = HandlePriceCodeAndExternalDiscount(ref priceReason, groupId, ref currencyCode, ref countryCode, collection.m_oExtDisountModule,
                out externalDiscount, ref priceCode, blockEntitlement == BlockEntitlementType.BLOCK_PPV, ip, sSiteGUID);
            fullPrice.FinalPrice = CopyPrice(fullPrice.OriginalPrice);
            collection.m_oCollectionPriceCode = priceCode;
            fullPrice.PriceReason = priceReason;
            if (fullPrice.PriceReason != PriceReason.ForPurchase)
            {
                return fullPrice;
            }
            
            DomainEntitlements domainEntitlements = null;
            if (TryGetDomainEntitlementsFromCache(groupId, domainID, null, ref domainEntitlements))
            {
                if (domainEntitlements.DomainBundleEntitlements != null && domainEntitlements.DomainBundleEntitlements.EntitledCollections != null
                    && domainEntitlements.DomainBundleEntitlements.EntitledCollections.ContainsKey(sColCode))
                {
                    bool isPending = domainEntitlements.DomainBundleEntitlements.EntitledCollections[sColCode].isPending;
                    fullPrice.PriceReason = isPending ? PriceReason.PendingEntitlement : PriceReason.CollectionPurchased;
                    fullPrice.FinalPrice.m_dPrice = 0.0;
                    return fullPrice;
                }
            }

            Price finalPrice = fullPrice.FinalPrice;
            Price discountPrice = null;
            if (externalDiscount != null)
            {
                discountPrice = Instance.GetPriceAfterDiscount(finalPrice, externalDiscount, 0);
            }

            var collectionId = long.Parse(sColCode);
            if (domainID > 0)
            {
                Price lowestPrice = CopyPrice(discountPrice) ?? CopyPrice(fullPrice.FinalPrice);
                fullPrice.CampaignDetails = GetValidCampaign(groupId, domainID, fullPrice.OriginalPrice, ref lowestPrice, eTransactionType.Collection, currencyCode,
                    collectionId, countryCode, couponCode, 0, null);

                if (fullPrice.CampaignDetails != null)
                {
                    fullPrice.FinalPrice = lowestPrice;
                    fullPrice.CouponCode = couponCode;
                    return fullPrice;
                }
            }
            
            List<int> lUsersIds = Utils.GetAllUsersDomainBySiteGUID(sSiteGUID, groupId, ref domainID);
            fullPrice.FinalPrice = PriceManager.GetLowestPrice(groupId, fullPrice.FinalPrice, domainID, discountPrice, eTransactionType.Collection, currencyCode, collectionId, countryCode,
                                   ref couponCode, collection.m_oCouponsGroup, collection.CouponsGroups, lUsersIds.ConvertAll(x => x.ToString()));
            fullPrice.CouponCode = couponCode;
            return fullPrice;
        }

        internal static Price GetPrePaidFinalPrice(Int32 groupId, string prePaidCode, ref PriceReason theReason, ref PrePaidModule thePrePaid, string countryCode,
                                                   string sLANGUAGE_CODE, string sDEVICE_NAME, string sCouponCode = "")
        {
            Price p = null;

            if (thePrePaid == null)
            {
                PrePaidModule ppModule = null;

                ppModule = Pricing.Module.GetPrePaidModuleData(groupId, int.Parse(prePaidCode), countryCode, sLANGUAGE_CODE, sDEVICE_NAME);
                thePrePaid = ObjectCopier.Clone<PrePaidModule>((PrePaidModule)(ppModule));
                if (thePrePaid == null)
                {
                    theReason = PriceReason.UnKnown;
                    return null;
                }
            }

            if (thePrePaid.m_PriceCode != null)
            {
                p = ObjectCopier.Clone<Price>((Price)(thePrePaid.m_PriceCode.m_oPrise));

                if (!string.IsNullOrEmpty(sCouponCode))
                {
                    CouponsGroup couponGroups = ObjectCopier.Clone<CouponsGroup>((CouponsGroup)(thePrePaid.m_CouponsGroup));
                    p = CalculateCouponDiscount(ref p, couponGroups, ref sCouponCode, groupId, 0, countryCode);
                }
            }

            theReason = PriceReason.ForPurchase;

            return p;
        }

        internal static string ConvertArrayIntToStr(int[] theArray)
        {

            StringBuilder sb = new StringBuilder();
            if (theArray != null && theArray.Length > 0)
            {
                for (int i = 0; i < theArray.Length; i++)
                {
                    sb.Append(String.Concat(theArray[i], "-"));
                }
            }
            return sb.ToString();
        }

        public static Int32 GetMediaIDFromFileID(Int32 nMediaFileID, Int32 nGroupID)
        {
            Int32[] nMediaFilesIDs = { nMediaFileID };
            MeidaMaper[] mapper = null;
            string nMediaFilesIDsForCache = ConvertArrayIntToStr(nMediaFilesIDs);

            mapper = GetMediaMapper(nGroupID, nMediaFilesIDs);

            if (mapper == null || mapper.Length == 0)
                return 0;

            if (mapper[0].m_nMediaFileID == nMediaFileID)
                return mapper[0].m_nMediaID;

            return 0;
        }

        //Get ProductCode and get it MediaFileID - then continue as it was mediaFileID
        public static Int32 GetMediaIDFromFileID(string sProductCode, Int32 nGroupID, ref int nMediaFileID)
        {

            DataTable dt = ConditionalAccessDAL.Get_MediaFileByProductCode(nGroupID, sProductCode);

            if (dt != null && dt.DefaultView.Count > 0)
            {
                nMediaFileID = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0]["ID"]);
            }

            return GetMediaIDFromFileID(nMediaFileID, nGroupID);
        }

        static public DateTime GetEndDateTime(DateTime dBase, long value, bool bIsAddLifeCycle = true, bool includeMillisecond = false)
        {
            var duration = new Duration(value);
            return GetEndDateTime(duration, dBase, bIsAddLifeCycle, includeMillisecond);
        }

        static public DateTime GetEndDateTime(Duration duration, DateTime dBase, bool bIsAddLifeCycle = true, bool includeMillisecond = false)
        {
            int mulFactor = bIsAddLifeCycle ? 1 : -1;

            DateTime dRet = includeMillisecond ? dBase : dBase.AddTicks(-(dBase.Ticks % TimeSpan.TicksPerSecond));

            switch (duration.Unit)
            {
                case DurationUnit.Minutes:
                    dRet = dRet.AddMinutes(mulFactor * duration.Value);
                    break;
                case DurationUnit.Hours:
                    dRet = dRet.AddHours(mulFactor * duration.Value);
                    break;
                case DurationUnit.Days:
                    dRet = dRet.AddDays(mulFactor * duration.Value);
                    break;
                case DurationUnit.Weeks:
                    dRet = dRet.AddDays(mulFactor * duration.Value * 7);
                    break;
                case DurationUnit.Months:
                    dRet = dRet.AddMonths(mulFactor * (int)duration.Value);
                    break;
                case DurationUnit.Years:
                    dRet = dRet.AddYears(mulFactor * (int)duration.Value);
                    break;
            }

            return dRet;
        }

        public static string GetLocaleStringForCache(string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME)
        {

            StringBuilder sb = new StringBuilder();
            if (!string.IsNullOrEmpty(sCountryCd))
                sb.Append(String.Concat("_", sCountryCd));
            if (!string.IsNullOrEmpty(sLANGUAGE_CODE))
                sb.Append(String.Concat("_", sLANGUAGE_CODE));
            if (!string.IsNullOrEmpty(sDEVICE_NAME))
                sb.Append(String.Concat("_", sDEVICE_NAME));

            return sb.ToString();
        }

        internal static FullPrice GetMediaFileFinalPriceForNonGetItemsPrices(Int32 nMediaFileID, PPVModule ppvModule, string sSiteGUID, string sCouponCode, Int32 nGroupID,
                            ref Subscription relevantSub, ref Collection relevantCol,
                            ref PrePaidModule relevantPP, string countryCode, string sLANGUAGE_CODE, string sDEVICE_NAME,
                            bool shouldIgnoreBundlePurchases = false, string ip = null, string currencyCode = null, BlockEntitlementType blockEntitlement = BlockEntitlementType.NONE)
        {
            Dictionary<int, int> mediaFileTypesMapping = null;
            List<int> allUsersInDomain = null;
            string sFirstDeviceNameFound = string.Empty;
            int nMediaFileTypeID = 0;
            int domainID = 0;
            var fullPrice = new FullPrice()
            {
                PriceReason = PriceReason.UnKnown
            };

            // check if file is avilable             
            Dictionary<int, string> mediaFilesProductCode = new Dictionary<int, string>();
            Dictionary<int, MediaFileStatus> validMediaFiles = Utils.ValidateMediaFiles(new int[1] { nMediaFileID }, ref mediaFilesProductCode, nGroupID, GetIP2CountryId(nGroupID, ip));
            if (validMediaFiles[nMediaFileID] == MediaFileStatus.NotForPurchase)
            {
                fullPrice.PriceReason = PriceReason.NotForPurchase;
                return fullPrice;
            }

            bool isValidCurrencyCode = false;
            // Validate currencyCode if it was passed in the request
            if (!string.IsNullOrEmpty(currencyCode))
            {
                if (!GeneralPartnerConfigManager.Instance.IsValidCurrencyCode(nGroupID, currencyCode))
                {
                    fullPrice.PriceReason = PriceReason.InvalidCurrency;
                    fullPrice.FinalPrice = new Price();
                    return fullPrice;
                }
                else
                {
                    isValidCurrencyCode = true;
                }
            }

            if (nMediaFileID > 0)
            {
                nMediaFileTypeID = GetMediaFileTypeID(nGroupID, nMediaFileID);
            }
            if (!string.IsNullOrEmpty(sSiteGUID))
            {
                allUsersInDomain = GetAllUsersInDomainBySiteGUIDIncludeDeleted(sSiteGUID, nGroupID, ref domainID);

                if (ppvModule != null && ppvModule.m_relatedFileTypes != null && ppvModule.m_relatedFileTypes.Count > 0)
                {
                    /*
                     * 1. In this case GetMediaFileFinalPrice needs the mapping of ppv related file types to media file types from DB.
                     * 2. Otherwise, GetItemsPrices does not need it, and we just send an empty dictionary in order to avoid a DB connection.
                     * 3. GetItemsPrices iterate over this function, hence, if possible, invoke Get_GroupMediaTypesIDs SP just once from GetItemsPrices.
                     */
                    mediaFileTypesMapping = ConditionalAccessDAL.Get_GroupMediaTypesIDs(nGroupID);
                }
                else
                {
                    mediaFileTypesMapping = new Dictionary<int, int>(0);
                }
            }
            else
            {
                allUsersInDomain = new List<int>(0);
                mediaFileTypesMapping = new Dictionary<int, int>(0);
            }
            bool bCancellationWindow = false;

            // purchasedBySiteGuid, purchasedAsMediaFileID, EndDate and StartDate are only needed in GetItemsPrices.
            string purchasedBySiteGuid = string.Empty;
            int purchasedAsMediaFileID = 0;
            DateTime? dtStartDate = null;
            DateTime? dtEndDate = null;
            DateTime? dtDiscountEndDate = null;

            if (!string.IsNullOrEmpty(ip) && (isValidCurrencyCode || GeneralPartnerConfigManager.Instance.GetGroupDefaultCurrency(nGroupID, ref currencyCode)))
            {
                countryCode = APILogic.Utils.GetIP2CountryCode(nGroupID, ip);
                PriceCode priceCodeWithCurrency = Core.Pricing.Module.GetPriceCodeDataByCountyAndCurrency(nGroupID, ppvModule.m_oPriceCode.m_nObjectID, countryCode, currencyCode);
                bool shouldUpdateDiscountModule = false;
                DiscountModule discountModuleWithCurrency = null;
                if (ppvModule.m_oDiscountModule != null)
                {
                    discountModuleWithCurrency = Core.Pricing.Module.Instance.GetDiscountCodeDataByCountryAndCurrency(nGroupID, ppvModule.m_oDiscountModule.m_nObjectID, countryCode, currencyCode);
                    shouldUpdateDiscountModule = discountModuleWithCurrency != null;
                }

                if (priceCodeWithCurrency == null || (shouldUpdateDiscountModule && discountModuleWithCurrency == null))
                {
                    fullPrice.PriceReason = PriceReason.CurrencyNotDefinedOnPriceCode;
                    fullPrice.FinalPrice = new Price();
                    return fullPrice;
                }
                else
                {
                    ppvModule.m_oPriceCode = TVinciShared.ObjectCopier.Clone<PriceCode>(priceCodeWithCurrency);
                    if (shouldUpdateDiscountModule)
                    {
                        ppvModule.m_oDiscountModule = TVinciShared.ObjectCopier.Clone<DiscountModule>(discountModuleWithCurrency);
                    }
                }
            }

            // relatedMediaFileIDs is needed only GetLicensedLinks (which calls GetItemsPrices in order to get to GetMediaFileFinalPrice)
            List<int> relatedMediaFileIDs = new List<int>();
            fullPrice = GetMediaFileFinalPrice(nMediaFileID, validMediaFiles[nMediaFileID], ppvModule, sSiteGUID, sCouponCode, nGroupID, true, ref relevantSub,
                                          ref relevantCol, ref relevantPP, ref sFirstDeviceNameFound, countryCode, sLANGUAGE_CODE, sDEVICE_NAME, ip,
                                          mediaFileTypesMapping, allUsersInDomain, nMediaFileTypeID, ref bCancellationWindow, ref purchasedBySiteGuid, ref purchasedAsMediaFileID,
                                          ref relatedMediaFileIDs, ref dtStartDate, ref dtEndDate, ref dtDiscountEndDate, domainID, currencyCode, null, 0,
                                          DomainSuspentionStatus.Suspended, true, shouldIgnoreBundlePurchases, blockEntitlement);
            return fullPrice;
        }

        internal static void GetApiAndPricingCredentials(int nGroupID, ref string sPricingUsername, ref string sPricingPassword,
            ref string sAPIUsername, ref string sAPIPassword)
        {
            Dictionary<string, string[]> dict = Get_MultipleWSCredentials(nGroupID, new List<string>(2) { "api", "pricing" });
            string[] apiDetails = dict["api"];
            sAPIUsername = apiDetails[0];
            sAPIPassword = apiDetails[1];
            string[] pricingDetails = dict["pricing"];
            sPricingUsername = pricingDetails[0];
            sPricingPassword = pricingDetails[1];
        }

        private static List<int> GetMediaTypesOfPPVRelatedFileTypes(int nGroupID, int[] ppvRelatedFileTypes, Dictionary<int, int> mediaFileTypesMapping, ref bool bIsMultiMediaTypes)
        {
            List<int> res = null;
            if (ppvRelatedFileTypes != null && ppvRelatedFileTypes.Length > 0)
            {
                res = new List<int>(ppvRelatedFileTypes.Length);
                if (mediaFileTypesMapping != null && mediaFileTypesMapping.Count > 0)
                {
                    for (int i = 0; i < ppvRelatedFileTypes.Length; i++)
                    {
                        int mediaTypeID = 0;
                        if (mediaFileTypesMapping.TryGetValue(ppvRelatedFileTypes[i], out mediaTypeID) && mediaTypeID > 0)
                        {
                            res.Add(mediaTypeID);
                        }
                    }
                }
            }
            else
            {
                res = new List<int>(0);
            }

            if (res.Count > 1)
            {
                bIsMultiMediaTypes = true;
            }

            return res;
        }

        internal static int ExtractMediaIDOutOfMediaMapper(MeidaMaper[] mapper, int nMediaFileID)
        {
            for (int i = 0; i < mapper.Length; i++)
            {
                if (mapper[i] != null && mapper[i].m_nMediaFileID == nMediaFileID)
                {
                    return mapper[i].m_nMediaID;
                }
            }

            return 0;
        }

        internal static string GetCachingManagerKey(string sMethodName, string sBusinessModuleCode, int nGroupID, string sCountryCd, string sLanguageCode, string sDeviceName)
        {
            return String.Concat(sMethodName, sBusinessModuleCode, "_", nGroupID, GetLocaleStringForCache(sCountryCd, sLanguageCode, sDeviceName));
        }

        internal static string GetCachingManagerKey(string sMethodName, string sBusinessModuleCode, int nGroupID)
        {
            return GetCachingManagerKey(sMethodName, sBusinessModuleCode, nGroupID, string.Empty, string.Empty, string.Empty);
        }

        private static List<int> GetFileIDs(List<int> mediaFilesList, int nMediaFileID, bool isMultiMediaTypes, int nMediaID)
        {
            List<int> lFiles = new List<int>();
            if ((mediaFilesList != null && mediaFilesList.Count > 0) || !isMultiMediaTypes)
            {
                lFiles = ConditionalAccessDAL.Get_MediaFileByID(mediaFilesList, nMediaFileID, isMultiMediaTypes, nMediaID);
                if (!lFiles.Contains(nMediaFileID))
                {
                    lFiles.Add(nMediaFileID);
                }
                return lFiles;
            }

            return new List<int>(0);
        }

        private static bool IsPurchasedAsPurePPV(string sSubCode, string sPrePaidCode)
        {
            return sSubCode.Length == 0 && sPrePaidCode.Length == 0;
        }

        internal static bool IsAnonymousUser(string siteGuid)
        {
            int userID = 0;
            return (!int.TryParse(siteGuid, out userID) || userID <= 0);
        }

        internal static FullPrice GetMediaFileFinalPrice(Int32 nMediaFileID, MediaFileStatus eMediaFileStatus, PPVModule ppvModule, string sSiteGUID, string couponCode,
                                                     Int32 groupID, bool bIsValidForPurchase, ref Subscription relevantSub, ref Collection relevantCol,
                                                     ref PrePaidModule relevantPP, ref string sFirstDeviceNameFound, string countryCode, string sLANGUAGE_CODE, string sDEVICE_NAME,
                                                     string clientIP, Dictionary<int, int> mediaFileTypesMapping, List<int> allUserIDsInDomain, int nMediaFileTypeID,
                                                     ref bool bCancellationWindow, ref string purchasedBySiteGuid, ref int purchasedAsMediaFileID, ref List<int> relatedMediaFileIDs,
                                                     ref DateTime? p_dtStartDate, ref DateTime? p_dtEndDate, ref DateTime? dtDiscountEndDate, int domainID,
                                                     string currencyCode, DomainEntitlements domainEntitlements = null, int mediaID = 0,
                                                     DomainSuspentionStatus userSuspendStatus = DomainSuspentionStatus.Suspended, bool shouldCheckUserStatus = true,
                                                     bool shouldIgnoreBundlePurchases = false, BlockEntitlementType blockEntitlement = BlockEntitlementType.NONE)
        {
            var fullPrice = new FullPrice()
            {
                PriceReason = PriceReason.Free,
                CouponCode = couponCode,
                CouponRemainder = 0,
                CampaignDetails = null,
                FinalPrice = null,
                OriginalPrice = null
            };

            if (ppvModule == null)
            {
                fullPrice.PriceReason = PriceReason.Free;
                return fullPrice;
            }

            // get user status and validity if needed
            bool isUserValidRes = !string.IsNullOrEmpty(sSiteGUID) && sSiteGUID != "0";
            if (shouldCheckUserStatus)
            {
                isUserValidRes = IsUserValid(sSiteGUID, groupID, ref domainID, ref userSuspendStatus);
            }

            // check user status and validity
            if (isUserValidRes && ((blockEntitlement == BlockEntitlementType.NONE && (userSuspendStatus == DomainSuspentionStatus.Suspended && !RolesPermissionsManager.Instance.AllowActionInSuspendedDomain(groupID, long.Parse(sSiteGUID)))) ||
                                   (blockEntitlement == BlockEntitlementType.BLOCK_ALL)))
            {
                fullPrice.PriceReason = PriceReason.UserSuspended;
                return fullPrice;
            }

            if (userSuspendStatus == DomainSuspentionStatus.Suspended && !RolesPermissionsManager.Instance.AllowActionInSuspendedDomain(groupID, long.Parse(sSiteGUID)))
            {
                userSuspendStatus = DomainSuspentionStatus.OK;
            }

            fullPrice.PriceReason = PriceReason.UnKnown;
            
            int[] fileTypes = new int[1] { nMediaFileTypeID };

            // get mediaID
            if (mediaID == 0)
            {
                Int32[] nMediaFilesIDs = { nMediaFileID };
                MeidaMaper[] mapper = GetMediaMapper(groupID, nMediaFilesIDs);
                if (mapper == null || mapper.Length == 0)
                    return null;

                mediaID = ExtractMediaIDOutOfMediaMapper(mapper, nMediaFileID);
            }

            fullPrice.OriginalPrice = ObjectCopier.Clone((Price)(ppvModule.m_oPriceCode.m_oPrise));
            fullPrice.FinalPrice = ObjectCopier.Clone((Price)(ppvModule.m_oPriceCode.m_oPrise));

            if (!IsAnonymousUser(sSiteGUID))
            {
                bool bEnd = false;
                int nWaiver = 0;
                DateTime dPurchaseDate = DateTime.MinValue;
                int ppvID = StringUtils.ConvertTo<int>(ppvModule.m_sObjectCode);

                if (allUserIDsInDomain == null || allUserIDsInDomain.Count == 0)
                {
                    allUserIDsInDomain = GetAllUsersDomainBySiteGUID(sSiteGUID, groupID, ref domainID);
                }

                if (blockEntitlement != BlockEntitlementType.BLOCK_PPV)
                {
                    int[] ppvGroupFileTypes = ppvModule.m_relatedFileTypes != null ? ppvModule.m_relatedFileTypes.ToArray() : null;
                    List<int> lstFileIDs = new List<int>();

                    // get list of mediaFileIDs
                    if (domainEntitlements != null && domainEntitlements.DomainPpvEntitlements.MediaIdGroupFileTypeMapper != null)
                    {
                        lstFileIDs = GetRelatedFileIDs(mediaID, ppvGroupFileTypes, domainEntitlements.DomainPpvEntitlements.MediaIdGroupFileTypeMapper);
                    }
                    else
                    {
                        bool isMultiMediaTypes = false;
                        List<int> mediaFilesList = GetMediaTypesOfPPVRelatedFileTypes(groupID, ppvGroupFileTypes, mediaFileTypesMapping, ref isMultiMediaTypes);
                        lstFileIDs = GetFileIDs(mediaFilesList, nMediaFileID, isMultiMediaTypes, mediaID);
                    }

                    relatedMediaFileIDs.AddRange(lstFileIDs);
                    relatedMediaFileIDs = relatedMediaFileIDs.Distinct().ToList();
                    fullPrice.OriginalPrice = ObjectCopier.Clone((Price)(ppvModule.m_oPriceCode.m_oPrise));
                    fullPrice.FinalPrice = ObjectCopier.Clone((Price)(ppvModule.m_oPriceCode.m_oPrise));

                    string sSubCode = string.Empty;
                    string sPPCode = string.Empty;
                    bool isEntitled = false;
                    bool isPending = false;

                    if (domainEntitlements != null && domainEntitlements.DomainPpvEntitlements.EntitlementsDictionary != null)
                    {
                        bool isRelated = lstFileIDs.Contains(nMediaFileID);
                        HashSet<int> mediaFiles = new HashSet<int>();
                        log.DebugFormat("MediaIdGroupFileTypeMapper size {0}", domainEntitlements.DomainPpvEntitlements.MediaIdGroupFileTypeMapper.Count);
                        if (isRelated && domainEntitlements.DomainPpvEntitlements.MediaIdToMediaFiles.ContainsKey(mediaID))
                        {
                            mediaFiles = domainEntitlements.DomainPpvEntitlements.MediaIdToMediaFiles[mediaID];
                        }

                        isEntitled = IsUserEntitled(isRelated, ppvModule.m_sObjectCode, ref ppvID, ref sSubCode, ref sPPCode, ref nWaiver, ref dPurchaseDate, ref purchasedBySiteGuid,
                                                    ref purchasedAsMediaFileID, ref p_dtStartDate, ref p_dtEndDate, domainEntitlements.DomainPpvEntitlements.EntitlementsDictionary,
                                                    nMediaFileID, mediaFiles, ref isPending);
                    }
                    else
                    {
                        isEntitled = ConditionalAccessDAL.Get_AllUsersPurchases(allUserIDsInDomain, lstFileIDs, nMediaFileID, ppvModule.m_sObjectCode, ref ppvID, ref sSubCode,
                                                                            ref sPPCode, ref nWaiver, ref dPurchaseDate, ref purchasedBySiteGuid, ref purchasedAsMediaFileID,
                                                                            ref p_dtStartDate, ref p_dtEndDate, ref isPending, domainID);
                    }

                    if (isPending)
                    {
                        fullPrice.PriceReason = PriceReason.PendingEntitlement;
                    }
                    // user or domain users have entitlements \ purchases
                    else if (isEntitled)
                    {
                        fullPrice.FinalPrice.m_dPrice = 0;
                        // Cancellation Window check by ppvUsageModule + purchase date
                        bCancellationWindow = IsCancellationWindowPerPurchase(ppvModule.m_oUsageModule, bCancellationWindow, nWaiver, dPurchaseDate);

                        if (IsPurchasedAsPurePPV(sSubCode, sPPCode))
                        {
                            if (ppvModule.m_bFirstDeviceLimitation &&
                                !IsFirstDeviceEqualToCurrentDevice(nMediaFileID, ppvModule.m_sObjectCode, allUserIDsInDomain, sDEVICE_NAME, ref sFirstDeviceNameFound))
                            {
                                fullPrice.PriceReason = PriceReason.FirstDeviceLimitation;
                            }
                            else
                            {
                                fullPrice.PriceReason = PriceReason.PPVPurchased;
                            }
                        }
                        else if (!shouldIgnoreBundlePurchases)
                        {
                            if (sSubCode.Length > 0)
                            {
                                // purchased as part of subscription
                                fullPrice.PriceReason = PriceReason.SubscriptionPurchased;
                                Subscription[] sub = GetSubscriptionsDataWithCaching(new List<string>(1) { sSubCode }, groupID);
                                if (sub != null && sub.Length > 0)
                                {
                                    relevantSub = sub[0];
                                }
                                else
                                {
                                    relevantSub = null;
                                }
                            }
                            else if (sPPCode.Length > 0)
                            {
                                // purchased as part of pre paid
                                fullPrice.PriceReason = PriceReason.PrePaidPurchased;
                                relevantPP = Pricing.Module.GetPrePaidModuleData(groupID, int.Parse(sPPCode), countryCode, sLANGUAGE_CODE, sDEVICE_NAME);
                            }
                        }

                        bEnd = true;
                    }
                    else if (lstFileIDs.Count > 0 && eMediaFileStatus == MediaFileStatus.ValidOnlyIfPurchase) // user didn't purchase and mediaFileREson is ValidOnlyIfPurchase
                    {
                        fullPrice.PriceReason = PriceReason.NotForPurchase;
                    }
                    else if (IsPPVModuleToBePurchasedAsSubOnly(ppvModule))
                    {
                        fullPrice.PriceReason = PriceReason.ForPurchaseSubscriptionOnly;
                    }

                    if (bEnd || (!bIsValidForPurchase && !isPending))
                    {
                        return fullPrice;
                    }
                }

                Subscription[] relevantValidSubscriptions = null;
                Collection[] relevantValidCollections = null;
                // dictionary(subscriptionCode, [nWaiver, dPurchaseDate, dEndDate])
                Dictionary<string, UserBundlePurchase> subsPurchase = new Dictionary<string, UserBundlePurchase>();
                Dictionary<string, UserBundlePurchase> collPurchase = new Dictionary<string, UserBundlePurchase>();

                //check here if it is part of a purchased subscription or part of purchased collections
                if (blockEntitlement != BlockEntitlementType.BLOCK_SUBSCRIPTION)
                {
                    if (domainEntitlements != null && domainEntitlements.DomainBundleEntitlements.EntitledSubscriptions != null && domainEntitlements.DomainBundleEntitlements.EntitledCollections != null)
                    {
                        subsPurchase = domainEntitlements.DomainBundleEntitlements.EntitledSubscriptions;

                        // filter pre sale subs
                        if (subsPurchase?.Count > 0 && domainEntitlements.DomainBundleEntitlements?.SubscriptionsData?.Count > 0)
                        {
                            List<string> subIds = subsPurchase.Keys.ToList();
                            int subId = 0;
                            foreach (string subIdentifier in subIds)
                            {
                                subId = int.Parse(subIdentifier);
                                if (domainEntitlements.DomainBundleEntitlements.SubscriptionsData.ContainsKey(subId))
                                {
                                    Subscription s = domainEntitlements.DomainBundleEntitlements.SubscriptionsData[subId];
                                    if (s.m_dStartDate > DateTime.UtcNow)
                                    {
                                        subsPurchase.Remove(subIdentifier);
                                    }
                                }
                            }
                        }

                        collPurchase = domainEntitlements.DomainBundleEntitlements.EntitledCollections;
                        GetUserValidBundles(mediaID, nMediaFileID, eMediaFileStatus, groupID, fileTypes, allUserIDsInDomain, relatedMediaFileIDs, subsPurchase,
                                            collPurchase, domainEntitlements.DomainBundleEntitlements.FileTypeIdToSubscriptionMappings, domainEntitlements.DomainBundleEntitlements.SubscriptionsData,
                                            domainEntitlements.DomainBundleEntitlements.CollectionsData, domainEntitlements.DomainBundleEntitlements.ChannelsToSubscriptionMappings,
                                            domainEntitlements.DomainBundleEntitlements.ChannelsToCollectionsMappings, ref relevantValidSubscriptions, ref relevantValidCollections,
                                            domainEntitlements.DomainBundleEntitlements.FileTypeIdToCollectionMappings);
                    }
                    else
                    {
                        GetUserValidBundlesFromListOptimized(sSiteGUID, mediaID, nMediaFileID, eMediaFileStatus, groupID, fileTypes, allUserIDsInDomain,
                                                            relatedMediaFileIDs, ref relevantValidSubscriptions, ref relevantValidCollections, ref subsPurchase, ref collPurchase, domainID);
                    }

                    if (relevantValidSubscriptions != null && relevantValidSubscriptions.Length > 0)
                    {
                        Dictionary<long, List<Subscription>> groupedSubs = (from s in relevantValidSubscriptions
                                                                            group s by s.m_Priority).OrderByDescending(gr => gr.Key).ToDictionary(gr => gr.Key, gr => gr.ToList());

                        if (groupedSubs != null)
                        {
                            List<Subscription> prioritySubs = groupedSubs.Values.LastOrDefault();
                            for (int i = 0; i < prioritySubs.Count; i++)
                            {
                                Subscription s = prioritySubs[i];
                                DiscountModule d = s.m_oDiscountModule;
                                Price subp = ObjectCopier.Clone(CalculateMediaFileFinalPriceNoSubs(nMediaFileID, mediaID, ppvModule.m_oPriceCode.m_oPrise,
                                    s.m_oDiscountModule, s.m_oCouponsGroup, sSiteGUID, couponCode, groupID, s.m_sObjectCode, out dtDiscountEndDate, domainID,
                                    countryCode));
                                if (subp != null)
                                {
                                    if (IsGeoBlock(groupID, s.n_GeoCommerceID, clientIP))
                                    {
                                        fullPrice.FinalPrice = ObjectCopier.Clone(subp);
                                        relevantSub = ObjectCopier.Clone(s);
                                        fullPrice.PriceReason = PriceReason.GeoCommerceBlocked;
                                    }
                                    else if (IsItemPurchased(fullPrice.FinalPrice, subp, ppvModule) && !shouldIgnoreBundlePurchases)
                                    {
                                        fullPrice.FinalPrice = ObjectCopier.Clone(subp);
                                        relevantSub = ObjectCopier.Clone(s);
                                        fullPrice.PriceReason = PriceReason.SubscriptionPurchased;
                                        bEnd = true;

                                        break;
                                    }
                                }
                            }

                            //cancellationWindow by relevantSub
                            if (relevantSub != null && subsPurchase.ContainsKey(relevantSub.m_SubscriptionCode))
                            {
                                nWaiver = subsPurchase[relevantSub.m_SubscriptionCode].nWaiver;
                                dPurchaseDate = subsPurchase[relevantSub.m_SubscriptionCode].dtPurchaseDate;
                                p_dtStartDate = dPurchaseDate;
                                p_dtEndDate = subsPurchase[relevantSub.m_SubscriptionCode].dtEndDate;

                                if (relevantSub.m_MultiSubscriptionUsageModule != null && relevantSub.m_MultiSubscriptionUsageModule.Count() > 0)
                                {
                                    bCancellationWindow = IsCancellationWindowPerPurchase(relevantSub.m_MultiSubscriptionUsageModule[0], bCancellationWindow, nWaiver, dPurchaseDate);
                                }
                                else if (relevantSub.m_oSubscriptionUsageModule != null)
                                {
                                    bCancellationWindow = IsCancellationWindowPerPurchase(relevantSub.m_oSubscriptionUsageModule, bCancellationWindow, nWaiver, dPurchaseDate);
                                }
                            }
                        }
                    }
                }

                if (bEnd)
                {
                    return fullPrice;
                }

                // check here if its part of a purchased collection
                if (relevantValidCollections != null && relevantValidCollections.Length > 0)
                {
                    for (int i = 0; i < relevantValidCollections.Length; i++)
                    {
                        Collection collection = relevantValidCollections[i];
                        DiscountModule discount = collection.m_oDiscountModule;
                        Price collectionsPrice = ObjectCopier.Clone(CalculateMediaFileFinalPriceNoSubs(nMediaFileID, mediaID, ppvModule.m_oPriceCode.m_oPrise, 
                            collection.m_oDiscountModule, collection.m_oCouponsGroup, sSiteGUID, couponCode, groupID, collection.m_sObjectCode, 
                            out dtDiscountEndDate, domainID, countryCode));
                        if (collectionsPrice != null)
                        {
                            if (IsItemPurchased(fullPrice.FinalPrice, collectionsPrice, ppvModule))
                            {
                                fullPrice.FinalPrice = ObjectCopier.Clone(collectionsPrice);
                                relevantCol = ObjectCopier.Clone(collection);
                                fullPrice.PriceReason = PriceReason.CollectionPurchased;
                                break;
                            }
                        }
                    }

                    //cancellationWindow by relevantSub
                    if (relevantCol != null && relevantCol.m_oCollectionUsageModule != null)
                    {
                        if (collPurchase.ContainsKey(relevantCol.m_CollectionCode))
                        {
                            nWaiver = collPurchase[relevantCol.m_CollectionCode].nWaiver;
                            dPurchaseDate = collPurchase[relevantCol.m_CollectionCode].dtPurchaseDate;
                            p_dtStartDate = dPurchaseDate;
                            p_dtEndDate = collPurchase[relevantCol.m_CollectionCode].dtEndDate;

                            bCancellationWindow = IsCancellationWindowPerPurchase(relevantCol.m_oCollectionUsageModule, bCancellationWindow, nWaiver, dPurchaseDate);
                        }
                    }
                }
                else
                {
                    if (blockEntitlement == BlockEntitlementType.BLOCK_PPV)
                    {
                        fullPrice.PriceReason = PriceReason.UserSuspended;
                        return fullPrice;
                    }

                    if (fullPrice.PriceReason == PriceReason.PendingEntitlement) // BEO-8661
                    {
                        return fullPrice;
                    }

                    // the media file was not purchased in any way. calculate its price as a single media file and its price reason
                    Price discountPrice = GetMediaFileFinalPriceNoSubs(nMediaFileID, mediaID, ppvModule, sSiteGUID, couponCode, groupID, string.Empty, 
                        out dtDiscountEndDate, domainID, countryCode);

                    fullPrice.FinalPrice = PriceManager.GetLowestPrice(groupID, fullPrice.FinalPrice, domainID, discountPrice, eTransactionType.PPV, currencyCode, ppvID, countryCode, ref couponCode, null, null,
                                           allUserIDsInDomain.ConvertAll(x => x.ToString()), mediaID);

                    var lowestPrice = fullPrice.FinalPrice;
                    fullPrice.CampaignDetails = Utils.GetValidCampaign(groupID, domainID, fullPrice.OriginalPrice, ref lowestPrice, eTransactionType.PPV, currencyCode,
                                ppvID, countryCode, couponCode, mediaID, new List<long>() { nMediaFileTypeID });
                    if (fullPrice.CampaignDetails != null)
                    {
                        fullPrice.FinalPrice = lowestPrice;
                        fullPrice.CouponCode = couponCode;
                    }

                    if (IsFreeMediaFile(fullPrice.PriceReason, fullPrice.FinalPrice))
                    {
                        fullPrice.PriceReason = PriceReason.Free;
                    }
                    else if (fullPrice.PriceReason != PriceReason.ForPurchaseSubscriptionOnly && fullPrice.PriceReason != PriceReason.NotForPurchase)
                    {
                        fullPrice.PriceReason = PriceReason.ForPurchase;
                    }
                }
            }
            else
            {
                // end if site guid is not null or empty       
                fullPrice.FinalPrice = GetMediaFileFinalPriceNoSubs(nMediaFileID, mediaID, ppvModule, sSiteGUID, couponCode, groupID, string.Empty, 
                    out dtDiscountEndDate, domainID, countryCode);

                if (IsPPVModuleToBePurchasedAsSubOnly(ppvModule))
                {
                    fullPrice.PriceReason = PriceReason.ForPurchaseSubscriptionOnly;
                }
                else
                {
                    fullPrice.PriceReason = PriceReason.ForPurchase;
                }
            }

            return fullPrice;
        }

        private static bool IsPPVModuleToBePurchasedAsSubOnly(PPVModule ppvModule)
        {
            return ppvModule != null && ppvModule.m_bSubscriptionOnly;
        }

        private static bool IsCancellationWindowPerPurchase(UsageModule oUsageModule, bool bCancellationWindow, int nWaiver, DateTime dCreateDate)
        {
            try
            {
                if (nWaiver == 0) // user never waiver the cancel transaction option  - so bCancellationWindow = true
                {
                    // check date 
                    if (oUsageModule != null && oUsageModule.m_bWaiver)
                    {
                        DateTime waiverDate = Utils.GetEndDateTime(dCreateDate, oUsageModule.m_nWaiverPeriod); // dCreateDate = ppv purchase date
                        if (DateTime.UtcNow <= waiverDate)
                        {
                            bCancellationWindow = true;
                        }
                    }
                }
                return bCancellationWindow;
            }
            catch (Exception ex)
            {
                log.Error("", ex);
                return false;
            }
        }

        private static bool IsFreeMediaFile(PriceReason reason, Price p)
        {
            return p != null && p.m_dPrice == 0 && reason != PriceReason.ForPurchaseSubscriptionOnly;
        }

        private static bool IsItemPurchased(Price initialPrice, Price businessModulePrice, PPVModule ppvModule)
        {
            return initialPrice == null ||
                (businessModulePrice.m_oCurrency.m_sCurrencyCD3 == ppvModule.m_oPriceCode.m_oPrise.m_oCurrency.m_sCurrencyCD3 && businessModulePrice.m_dPrice <= initialPrice.m_dPrice) ||
                (businessModulePrice.m_oCurrency.m_sCurrencyCD3 != ppvModule.m_oPriceCode.m_oPrise.m_oCurrency.m_sCurrencyCD3 && initialPrice.m_dPrice > 0);
        }

        private static bool IsGeoBlock(int nGroupID, int nSubGeoCommerceID, string sClientIP)
        {
            return IsGeoBlock(nGroupID, nSubGeoCommerceID, sClientIP, string.Empty, string.Empty);
        }

        private static bool IsGeoBlock(int nGroupID, int nSubGeoCommerceID, string sClientIP, string sAPIUsername,
            string sAPIPassword)
        {
            bool res = false;
            if (nSubGeoCommerceID <= 0)
            {
                return res;
            }

            if (!string.IsNullOrEmpty(sClientIP))
            {
                res = Api.Module.CheckGeoCommerceBlock(nGroupID, nSubGeoCommerceID, sClientIP);
            }

            return res;
        }

        private static void GetUsersAndDomainsCredentials(int nGroupID, ref string sUsersUsername, ref string sUsersPassword,
            ref string sDomainsUsername, ref string sDomainsPassword)
        {
            Dictionary<string, string[]> dict = Utils.Get_MultipleWSCredentials(nGroupID, new List<string>(2) { "users", "domains" });
            string[] usersCreds = dict["users"];
            sUsersUsername = usersCreds[0];
            sUsersPassword = usersCreds[1];
            string[] domainsCreds = dict["domains"];
            sDomainsUsername = domainsCreds[0];
            sDomainsPassword = domainsCreds[1];
        }

        private static Dictionary<string, string[]> Get_MultipleWSCredentials(int groupId, List<string> serviceList)
        {
            Dictionary<string, string[]> result = new Dictionary<string, string[]>();
            List<string> missingWsCredentials = new List<string>();
            foreach (string serviceName in serviceList)
            {
                eWSModules wsModule;
                if (Enum.TryParse(serviceName, true, out wsModule))
                {
                    string userName = string.Empty;
                    string password = string.Empty;
                    GetWSCredentials(groupId, wsModule, ref userName, ref password);
                    if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
                    {
                        missingWsCredentials.Add(serviceName);
                    }
                    else
                    {
                        result.Add(serviceName, new string[2] { userName, password });
                    }
                }
            }

            if (missingWsCredentials != null && missingWsCredentials.Count > 0)
            {
                result.Union(ConditionalAccessDAL.Get_MultipleWSCredentials(groupId, missingWsCredentials));
            }

            return result;
        }

        internal static List<int> GetAllUsersDomainBySiteGUID(string sSiteGUID, Int32 nGroupID, ref int domainID)
        {
            List<int> lDomainsUsers = new List<int>();
            int userId = 0;
            if (string.IsNullOrEmpty(sSiteGUID) || !int.TryParse(sSiteGUID, out userId) || userId < 1)
            {
                return lDomainsUsers;
            }

            UserResponseObject userResponseObj = Core.Users.Module.GetUserData(nGroupID, sSiteGUID, string.Empty);

            if (userResponseObj.m_RespStatus == ResponseStatus.OK && userResponseObj.m_user.m_domianID != 0)
            {
                domainID = userResponseObj.m_user.m_domianID;
                lDomainsUsers = GetDomainsUsers(userResponseObj.m_user.m_domianID, nGroupID, true);
            }
            else
            {
                lDomainsUsers.Add(int.Parse(sSiteGUID));
            }

            //change the user pending to users without (-1)
            lDomainsUsers = lDomainsUsers.ConvertAll(x => Math.Abs(x));

            return lDomainsUsers;
        }

        internal static List<int> GetAllUsersInDomain(int groupID, int domainId)
        {
            List<int> lDomainsUsers = GetDomainsUsers(domainId, groupID);
            //change the user pending to users without (-1)
            lDomainsUsers = lDomainsUsers.ConvertAll(x => Math.Abs(x));

            return lDomainsUsers;
        }

        internal static List<int> GetAllUsersInDomainBySiteGUIDIncludeDeleted(string sSiteGUID, Int32 nGroupID, ref int domainID)
        {
            List<int> lDomainsUsers = new List<int>();

            if (string.IsNullOrEmpty(sSiteGUID) || sSiteGUID.Equals("0"))
            {
                return lDomainsUsers;
            }

            UserResponseObject userResponseObj = Core.Users.Module.GetUserData(nGroupID, sSiteGUID, string.Empty);

            if (userResponseObj.m_RespStatus == ResponseStatus.OK && userResponseObj.m_user.m_domianID != 0)
            {
                domainID = userResponseObj.m_user.m_domianID;
                Dictionary<int, int> allUsersFromDB = DomainDal.GetUsersInDomainIncludeDeleted(domainID, nGroupID);
                if (allUsersFromDB != null)
                {
                    lDomainsUsers = allUsersFromDB.Keys.ToList();
                }
                else
                {
                    lDomainsUsers.Add(int.Parse(sSiteGUID));
                }
            }
            else
            {
                lDomainsUsers.Add(int.Parse(sSiteGUID));
            }

            //change the user pending to users without (-1)
            lDomainsUsers = lDomainsUsers.ConvertAll(x => Math.Abs(x));

            return lDomainsUsers;
        }

        public static List<int> GetDomainsUsers(int nDomainID, Int32 nGroupID, bool bGetAlsoPendingUsers = true)
        {

            List<int> intUsersList = new List<int>();
            List<string> usersList = Core.Domains.Module.GetDomainUserList(nGroupID, nDomainID);

            if (usersList != null && usersList.Count > 0)
            {
                for (int i = 0; i < usersList.Count; i++)
                {
                    int temp = 0;
                    // pending users are returned with domains with a minus before their site guid.
                    // for example: site 123456 which is pending in the domain will be returned as -123456
                    if (Int32.TryParse(usersList[i], out temp) && (bGetAlsoPendingUsers || temp > 0))
                    {
                        intUsersList.Add(temp);
                    }
                }
            }

            return intUsersList;
        }

        public static Int32 GetGroupID(string sWSUserName, string sWSPassword, string sFunctionName, ref BaseConditionalAccess t)
        {
            Credentials wsc = new Credentials(sWSUserName, sWSPassword);
            int nGroupID = TvinciCache.WSCredentials.GetGroupID(eWSModules.CONDITIONALACCESS, wsc);

            if (nGroupID > 0)
            {
                Utils.GetBaseConditionalAccessImpl(ref t, nGroupID);
            }
            else
            {
                log.Debug("WS ignored - " + string.Format("user:{0}, pass:{1}", sWSPassword, sWSPassword));
            }

            return nGroupID;
        }

        public static Int32 GetGroupID(string sWSUserName, string sWSPassword)
        {
            Credentials wsc = new Credentials(sWSUserName, sWSPassword);
            int nGroupID = TvinciCache.WSCredentials.GetGroupID(eWSModules.CONDITIONALACCESS, wsc);

            if (nGroupID == 0)
            {
                log.Debug("WS ignored - " + string.Format("user:{0}, pass:{1}", sWSPassword, sWSPassword));
            }

            return nGroupID;
        }

        public static double GetCouponDiscountPercent(Int32 nGroupID, string sCouponCode, long domainId)
        {
            double dCouponDiscountPercent = 0;
            CouponDataResponse theCouponData = null;

            theCouponData = Pricing.Module.GetCouponStatus(nGroupID, sCouponCode, domainId);

            if (theCouponData != null &&
                theCouponData.Status != null &&
                theCouponData.Status.Code == (int)eResponseStatus.OK &&
                theCouponData.Coupon != null &&
                theCouponData.Coupon.m_oCouponGroup != null &&
                theCouponData.Coupon.m_CouponStatus == CouponsStatus.Valid)
            {

                DiscountModule dCouponDiscount = theCouponData.Coupon.m_oCouponGroup.m_oDiscountCode;
                dCouponDiscountPercent = dCouponDiscount.m_dPercent;
            }

            return dCouponDiscountPercent;
        }

        public static string GetMediaFileCoGuid(int nGroupID, int nMediaFileID)
        {
            string sMediaFileCoGuid =
                DAL.ConditionalAccessDAL.GetMediaFileCoGuid(nGroupID, nMediaFileID);

            return sMediaFileCoGuid;
        }

        public static Subscription GetSubscriptionBytProductCode(Int32 nGroupID, string sProductCode, string sCountryCd2, string sLanguageCode3, string sDeviceName, bool bGetAlsoUnActive)
        {
            return Pricing.Module.GetSubscriptionDataByProductCode(nGroupID, sProductCode, sCountryCd2, sLanguageCode3, sDeviceName, bGetAlsoUnActive);
        }

        internal static string GetBasicLink(int nGroupID, int[] nMediaFileIDs, int nMediaFileID, string sBasicLink, out int nStreamingCompanyID, out string fileType, out int drmId)
        {
            MeidaMaper[] mapper = GetMediaMapper(nGroupID, nMediaFileIDs);
            nStreamingCompanyID = 0;
            fileType = string.Empty;
            drmId = 0;
            int mediaID = 0;
            if (mapper != null && mapper.Length > 0)
            {
                mediaID = ExtractMediaIDOutOfMediaMapper(mapper, nMediaFileID);
            }

            if (sBasicLink.Equals(string.Format("{0}||{1}", mediaID, nMediaFileID)))
            {
                string sBaseURL = string.Empty;
                string sStreamID = string.Empty;

                ConditionalAccessDAL.Get_BasicLinkData(nMediaFileID, ref sBaseURL, ref sStreamID, ref nStreamingCompanyID, ref fileType, out drmId);

                sBasicLink = string.Format("{0}{1}", sBaseURL, sStreamID);
                if (sStreamID.Length > 0)
                {
                    string groupCountryCode = string.Empty;
                    string groupSecretCode = string.Empty;
                    ConditionalAccessDAL.Get_GroupSecretAndCountryCode(nGroupID, ref groupSecretCode, ref groupCountryCode);
                    if (sBasicLink.Contains(BASIC_LINK_COUNTRY_CODE))
                    {
                        sBasicLink = sBasicLink.Replace(BASIC_LINK_COUNTRY_CODE, groupCountryCode.Trim().ToLower());
                    }

                    if (sBasicLink.Contains(BASIC_LINK_TICK_TIME))
                    {
                        long lT = DateTime.UtcNow.Ticks;
                        sBasicLink = sBasicLink.Replace(BASIC_LINK_TICK_TIME, String.Concat("tick=", lT.ToString()));
                        string sToHash = string.Empty;
                        string sHashed = string.Empty;
                        sToHash = String.Concat(groupSecretCode, lT);
                        sHashed = TVinciShared.ProtocolsFuncs.CalculateMD5Hash(sToHash);
                        sBasicLink = sBasicLink.Replace(BASIC_LINK_HASH, String.Concat("hash=", sHashed));
                    }
                    if (sBasicLink.Contains(BASIC_LINK_GROUP))
                    {
                        sBasicLink = sBasicLink.Replace(BASIC_LINK_GROUP, String.Concat("group=", nGroupID.ToString()));
                    }
                    if (sBasicLink.Contains(BASIC_LINK_CONFIG_DATA))
                    {
                        sBasicLink = sBasicLink.Replace(BASIC_LINK_CONFIG_DATA, "brt=");
                    }
                }
                sBasicLink = WebUtility.HtmlDecode(sBasicLink).Replace("''", "\"");
            }
            return sBasicLink;
        }

        public static int GetGroupFAILCOUNT(int nGroupID, string sConnKey)
        {
            int res = ConditionalAccessDAL.Get_GroupFailCount(nGroupID, sConnKey);
            return res > 0 ? res : DEFAULT_MPP_RENEW_FAIL_COUNT;
        }

        public static int GetGroupFAILCOUNT(int nGroupID)
        {
            return GetGroupFAILCOUNT(nGroupID, string.Empty);
        }

        public static string GetSafeParValue(string sQueryKey, string sParName, ref System.Xml.XmlNode theRoot)
        {
            try
            {
                return theRoot.SelectSingleNode(sQueryKey).Attributes[sParName].Value;
            }
            catch
            {
                return "";
            }
        }

        public static string GetSafeValue(string sQueryKey, ref System.Xml.XmlNode theRoot)
        {
            try
            {
                return theRoot.SelectSingleNode(sQueryKey).FirstChild.Value;
            }
            catch
            {
                return "";
            }
        }
        internal static void SplitRefference(string sRefference, ref Int32 nMediaFileID, ref Int32 nMediaID, ref string sSubscriptionCode, ref string sPPVCode, ref string sPrePaidCode,
           ref string sPriceCode, ref double dPrice, ref string sCurrencyCd, ref bool bIsRecurring, ref string sPPVModuleCode,
           ref Int32 nNumberOfPayments, ref string sSiteGUID, ref string sRelevantSub, ref Int32 nMaxNumberOfUses,
           ref Int32 nMaxUsageModuleLifeCycle, ref Int32 nViewLifeCycleSecs, ref string sPurchaseType,
           ref string sCountryCd, ref string sLanguageCd, ref string sDeviceName)
        {
            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
            doc.LoadXml(sRefference);
            System.Xml.XmlNode theRequest = doc.FirstChild;

            bIsRecurring = false;
            string sType = Utils.GetSafeParValue(".", "type", ref theRequest);
            string sSubscriptionID = Utils.GetSafeValue("s", ref theRequest);
            string scouponcode = Utils.GetSafeValue("cc", ref theRequest);
            string sPayNum = Utils.GetSafeParValue("//p", "n", ref theRequest);
            string sPayOutOf = Utils.GetSafeParValue("//p", "o", ref theRequest);
            string suid = Utils.GetSafeParValue("//u", "id", ref theRequest);
            string smedia_file = GetSafeValue("mf", ref theRequest);
            string smedia_id = GetSafeValue("m", ref theRequest);
            string ssub = Utils.GetSafeValue("s", ref theRequest);
            string sPP = Utils.GetSafeValue("pp", ref theRequest);
            string sppvmodule = Utils.GetSafeValue("ppvm", ref theRequest);
            string srelevantsub = Utils.GetSafeValue("rs", ref theRequest);
            string smnou = Utils.GetSafeValue("mnou", ref theRequest);
            string smaxusagemodulelifecycle = Utils.GetSafeValue("mumlc", ref theRequest);
            string sviewlifecyclesecs = Utils.GetSafeValue("vlcs", ref theRequest);
            string sppvcode = Utils.GetSafeValue("ppvm", ref theRequest);
            string spc = Utils.GetSafeValue("pc", ref theRequest);
            string spri = Utils.GetSafeValue("pri", ref theRequest);
            string scur = Utils.GetSafeValue("cu", ref theRequest);
            string sir = Utils.GetSafeParValue("//p", "ir", ref theRequest);
            string srs = Utils.GetSafeValue("rs", ref theRequest);

            string slcc = Utils.GetSafeValue("lcc", ref theRequest);
            if (!string.IsNullOrEmpty(slcc))
            {
                sCountryCd = slcc;
            }
            string sllc = Utils.GetSafeValue("llc", ref theRequest);
            sLanguageCd = sllc;
            string sldn = Utils.GetSafeValue("ldn", ref theRequest);
            sDeviceName = sldn;

            if (smedia_file != "")
                nMediaFileID = int.Parse(smedia_file);
            if (smedia_id != "")
                nMediaID = int.Parse(smedia_id);
            sSubscriptionCode = sSubscriptionID;
            sPPVCode = sppvcode;
            sPrePaidCode = sPP;
            sPriceCode = spc;
            if (spri != "")
                dPrice = double.Parse(spri);
            sCurrencyCd = scur;
            if (sir == "true")
                bIsRecurring = true;
            sPPVModuleCode = sppvmodule;
            if (sPayOutOf != "")
                nNumberOfPayments = int.Parse(sPayOutOf);
            sSiteGUID = suid;
            sRelevantSub = srs;
            if (smnou != "")
                nMaxNumberOfUses = int.Parse(smnou);
            if (smaxusagemodulelifecycle != "")
                nMaxUsageModuleLifeCycle = int.Parse(smaxusagemodulelifecycle);
            if (sviewlifecyclesecs != "")
                nViewLifeCycleSecs = int.Parse(sviewlifecyclesecs);
            sPurchaseType = sType;
        }

        public static string GetGoogleSignature(int nGroupID, int nCustomDataID)
        {
            string MY_SELLER_ID = "06511210546291891713"; //"YOUR SELLER ID";
            string MY_SELLER_SECRET = "hRVpATY0ZIsANB0gv756OQ"; //"YOUR SELLER SECRET";

            JWTHeaderObject HeaderObj = null;
            InAppItemObject ClaimObj = null;

            #region Reset callback custom data varibles
            string price = string.Empty;
            string currencyCode = string.Empty;
            string sSiteGUID = string.Empty;
            string assetID = string.Empty;
            string ppvOrSub = string.Empty;
            string sPrePaidID = string.Empty;
            string smedia_file = string.Empty;
            string sSubscriptionID = string.Empty;
            string sType = string.Empty;
            string scouponcode = string.Empty;
            string sPayNum = string.Empty;
            string sPayOutOf = string.Empty;
            string sppvmodule = string.Empty;
            string srelevantsub = string.Empty;
            string smnou = string.Empty;
            string smaxusagemodulelifecycle = string.Empty;
            string sviewlifecyclesecs = string.Empty;
            string sDigits = string.Empty;
            string sCountryCode = string.Empty;
            string sLangCode = string.Empty;
            string sDevice = string.Empty;
            string scurrency = string.Empty;
            string isRecurringStr = string.Empty;
            string sPPCreditValue = string.Empty;
            string sUserIP = string.Empty;
            string sCampCode = string.Empty;
            string sCampMNOU = string.Empty;
            string sCampLS = string.Empty;
            #endregion

            //The custom data is created by calling the AD_GetCustomDataID function in the CA/ 
            string sCustomData = GetCustomData(nCustomDataID);
            if (sCustomData != "")
            {
                #region Parse custom data xml

                //Parse the custom data xml
                System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
                doc.LoadXml(sCustomData);
                System.Xml.XmlNode theRequest = doc.FirstChild;

                sType = GetSafeParValue(".", "type", ref theRequest);
                sSiteGUID = GetSafeParValue("//u", "id", ref theRequest);
                sSubscriptionID = GetSafeValue("s", ref theRequest);
                sPrePaidID = GetSafeValue("pp", ref theRequest);
                sPPCreditValue = GetSafeValue("cpri", ref theRequest);
                scouponcode = GetSafeValue("cc", ref theRequest);
                sPayNum = GetSafeParValue("//p", "n", ref theRequest);
                sPayOutOf = GetSafeParValue("//p", "o", ref theRequest);
                isRecurringStr = GetSafeParValue("//p", "ir", ref theRequest);
                smedia_file = GetSafeValue("mf", ref theRequest);
                sppvmodule = GetSafeValue("ppvm", ref theRequest);
                srelevantsub = GetSafeValue("rs", ref theRequest);
                smnou = GetSafeValue("mnou", ref theRequest);
                sCountryCode = GetSafeValue("lcc", ref theRequest);
                sLangCode = GetSafeValue("llc", ref theRequest);
                sDevice = GetSafeValue("ldn", ref theRequest);
                smaxusagemodulelifecycle = GetSafeValue("mumlc", ref theRequest);
                sviewlifecyclesecs = GetSafeValue("vlcs", ref theRequest);
                sDigits = GetSafeValue("cc_card_number", ref theRequest);
                price = GetSafeValue("pri", ref theRequest);
                scurrency = GetSafeValue("cu", ref theRequest);
                sUserIP = GetSafeValue("up", ref theRequest);
                sCampCode = GetSafeValue("campcode", ref theRequest);
                sCampMNOU = GetSafeValue("cmnov", ref theRequest);
                sCampLS = GetSafeValue("cmumlc", ref theRequest);
                if (price == "")
                    price = "0.0";
                Int32 nPaymentNum = 0;
                Int32 nNumberOfPayments = 0;
                if (sPayNum != "")
                    nPaymentNum = int.Parse(sPayNum);
                if (sPayOutOf != "")
                    nNumberOfPayments = int.Parse(sPayOutOf);

                #endregion

                Int32 nMediaFileID = 0;
                Int32 nMediaID = 0;
                string sSubscriptionCode = "";
                string sPPVCode = "";
                string sPriceCode = "";
                string sPPVModuleCode = "";
                bool bIsRecurring = false;
                string sCurrencyCode = "";
                double dChargePrice = 0.0;
                string sRelevantSub = "";
                string sUserGUID = "";
                Int32 nMaxNumberOfUses = 0;
                Int32 nMaxUsageModuleLifeCycle = 0;
                Int32 nViewLifeCycleSecs = 0;
                string sPurchaseType = "";

                string sCountryCd = "";
                string sLanguageCode = "";
                string sDeviceName = "";
                string sPrePaidCode = string.Empty;


                SplitRefference(sCustomData, ref nMediaFileID, ref nMediaID, ref sSubscriptionCode, ref sPPVCode, ref sPrePaidCode, ref sPriceCode,
                        ref dChargePrice, ref sCurrencyCode, ref bIsRecurring, ref sPPVModuleCode, ref nNumberOfPayments, ref sUserGUID,
                                    ref sRelevantSub, ref nMaxNumberOfUses, ref nMaxUsageModuleLifeCycle, ref nViewLifeCycleSecs, ref sPurchaseType,
                                    ref sCountryCd, ref sLanguageCode, ref sDeviceName);

                if (!string.IsNullOrEmpty(sCampCode))
                {
                    int nCampCode = int.Parse(sCampCode);
                    if (nCampCode > 0)
                    {
                        //HandleCampaignUse(nCampCode, sSiteGUID, int.Parse(sCampMNOU), sCampLS);
                    }
                }

                switch (sType)
                {
                    case "pp":
                        #region Handle PPV Transaction

                        HeaderObj = new JWTHeaderObject(JWTHeaderObject.JWTHash.HS256, "1", "JWT");
                        PPVModule pp = Pricing.Module.GetPPVModuleData(nGroupID, sPPVModuleCode, sCountryCd, sLanguageCode, sDeviceName);


                        var pp_description = (from descValue in pp.m_sDescription
                                              where descValue.m_sLanguageCode3 == sLanguageCode
                                              select descValue.m_sValue.ToString()).FirstOrDefault();

                        ClaimObj = new InAppItemObject(pp.m_sObjectVirtualName, pp_description.ToString(), dChargePrice.ToString(), sCurrencyCode, nCustomDataID.ToString(), MY_SELLER_ID, 60, "Google", "google/payments/inapp/item/v1", 0);

                        #endregion
                        break;
                    case "sp":
                        #region Subscription Purchase

                        HeaderObj = new JWTHeaderObject(JWTHeaderObject.JWTHash.HS256, "1", "JWT");

                        Subscription sp = Pricing.Module.Instance.GetSubscriptionData(nGroupID, sSubscriptionCode, sCountryCd, sLanguageCode, sDeviceName, false);

                        DateTime nextdate = GetEndDateTime(DateTime.UtcNow, sp.m_oSubscriptionUsageModule.m_tsMaxUsageModuleLifeCycle);
                        string fequencey = "";
                        if (nextdate.Month > DateTime.UtcNow.Month)
                        {
                            fequencey = "monthly";
                        }
                        else if (nextdate.Year > DateTime.UtcNow.Year)
                        {
                            fequencey = "yearly";
                        }


                        var sp_description = (from descValue in sp.m_sDescription
                                              where descValue.m_sLanguageCode3 == sLanguageCode
                                              select descValue.m_sValue.ToString()).FirstOrDefault();


                        string sNumberOfRecPeriods = sp.m_nNumberOfRecPeriods == 0 ? null : sp.m_nNumberOfRecPeriods.ToString();

                        ClaimObj = new InAppItemObject(sp.m_sObjectVirtualName, sp_description.ToString(), dChargePrice.ToString(), scurrency, "prorated", nCustomDataID.ToString(), MY_SELLER_ID, 60, dChargePrice.ToString(), scurrency, "", fequencey, sNumberOfRecPeriods, "Google", "google/payments/inapp/subscription/v1", 0);



                        #endregion
                        break;
                    case "prepaid":
                        #region Handle PrePaid Transaction

                        #endregion
                        break;
                }


            }
            return JWTHelpers.buildJWT(HeaderObj, ClaimObj, MY_SELLER_SECRET);


        }

        internal static bool CheckStartDateBeforeEndDate(DateTime startDate, DateTime endDate)
        {
            return (startDate.CompareTo(endDate) < 0); // If true, then startDate is earlier than endDate
        }

        internal static void ReplaceSubStr(ref string url, Dictionary<string, object> oValuesToReplace)
        {
            if (oValuesToReplace.Count > 0)
            {
                foreach (KeyValuePair<string, object> pair in oValuesToReplace)
                {
                    string sKeyToSearch = string.Format("{0}{1}{2}", "{", pair.Key, "}");
                    if (url.Contains(sKeyToSearch))
                    {
                        url = url.Replace(sKeyToSearch, pair.Value.ToString());
                    }
                }
            }
        }

        internal static eStreamType GetStreamType(string sBaseLink)
        {
            eStreamType streamType = eStreamType.HLS;

            if ((sBaseLink.ToLower().Contains("ism")) && (sBaseLink.ToLower().Contains("manifest")))
            {
                streamType = eStreamType.SS;
            }
            else if (sBaseLink.Contains(".m3u8"))
            {
                streamType = eStreamType.HLS;
            }
            else if (sBaseLink.Contains(".mpd"))
            {
                streamType = eStreamType.DASH;
            }

            return streamType;
        }



        /*
         * 1. The user is entitled to preview module iff 
         *    The MPP has a valid preview module object AND
         *        (a. The user did not purchase preview module before OR
         *         b. User purchased a preview module before and satsifies the following property:
         *            (billing transactions row's create date of this MPP with preview module + full life cycle of the 
         *            preview module + non renewing period of preview module < datetime.utcnow)
         *         )
         *  2. According to MCORP-1723 specification document, free trial non renewing period is counted from the day
         *     the preview module full life cycle expires.
         *  3. The procedure of purchasing an MPP with preview module within customers who use Adyen is the following:
         *      a. If we hold the user's CC details, we just dummy charge him
         *      b. Otherwise, we charge him with a minimum amount, and automatically issue to Adyen a
         *         cancelOrRefund request. Hence, in this method, we extract from configuration a minimum amount to
         *         charge the user.
         */

        private static bool IsEntitledToPreviewModule(string sSiteGUID, Int32 nGroupID, string sSubCode, Subscription s, ref Price p, ref PriceReason theReason, int domainID)
        {
            bool res = true;
            if (s.m_oPreviewModule == null || s.m_oPreviewModule.m_nID == 0)
                return false;
            Dictionary<DateTime, List<int>> dict = GetPreviewModuleDataRelatedToUserFromDB(sSiteGUID, nGroupID, sSubCode, domainID);
            if (dict != null)
            {
                DateTime dtUtcNow = DateTime.UtcNow;
                foreach (KeyValuePair<DateTime, List<int>> kvp in dict)
                {
                    DateTime dtPreviousPreviewModuleStartDate = kvp.Key;
                    DateTime dtEndDateOfPreviousPreviewModule = GetEndDateTime(dtPreviousPreviewModuleStartDate, kvp.Value[1]);
                    DateTime dtEndDateOfNonRenewingPeriod = GetEndDateTime(dtEndDateOfPreviousPreviewModule, kvp.Value[2]);
                    if (dtUtcNow <= dtEndDateOfNonRenewingPeriod)
                    {
                        res = false;
                        break;
                    }
                }
            }
            if (res)
            {
                string sKeyOfMinPrice = String.Concat("PreviewModuleMinPrice", nGroupID);
                double dMinPriceForPreviewModule = DEFAULT_MIN_PRICE_FOR_PREVIEW_MODULE;
                if (TVinciShared.WS_Utils.GetTcmConfigValue(sKeyOfMinPrice) != string.Empty)
                    double.TryParse(TVinciShared.WS_Utils.GetTcmConfigValue(sKeyOfMinPrice), out dMinPriceForPreviewModule);
                p.m_dPrice = dMinPriceForPreviewModule;
                theReason = PriceReason.EntitledToPreviewModule;
            }
            return res;
        }

        private static Dictionary<DateTime, List<int>> GetPreviewModuleDataRelatedToUserFromDB(string sSiteGuid, int nGroupID, string sSubCode, int domainID)
        {
            Dictionary<DateTime, List<int>> res = null;
            DataTable dt = ConditionalAccessDAL.Get_PreviewModuleDataForEntitlementCalc(nGroupID, sSiteGuid, sSubCode, domainID);
            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    bool bIsParsingSuccessful = true;
                    DateTime dtStartDateOfMPP = DateTime.MaxValue;
                    int nPreviewModuleID = 0;
                    int nFullLifeCycleOfPreviewModule = 0;
                    int nNonRenewingPeriod = 0;
                    if (dt.Rows[i]["create_date"] != DBNull.Value && dt.Rows[i]["create_date"] != null)
                        bIsParsingSuccessful = DateTime.TryParse(dt.Rows[i]["create_date"].ToString(), out dtStartDateOfMPP);
                    if (dt.Rows[i]["preview_module_id"] != DBNull.Value && dt.Rows[i]["preview_module_id"] != null)
                        bIsParsingSuccessful = Int32.TryParse(dt.Rows[i]["preview_module_id"].ToString(), out nPreviewModuleID);
                    if (dt.Rows[i]["full_life_cycle_id"] != DBNull.Value && dt.Rows[i]["full_life_cycle_id"] != null)
                        bIsParsingSuccessful = Int32.TryParse(dt.Rows[i]["full_life_cycle_id"].ToString(), out nFullLifeCycleOfPreviewModule);
                    if (dt.Rows[i]["non_renewing_period_id"] != DBNull.Value && dt.Rows[i]["non_renewing_period_id"] != null)
                        bIsParsingSuccessful = Int32.TryParse(dt.Rows[i]["non_renewing_period_id"].ToString(), out nNonRenewingPeriod);
                    if (bIsParsingSuccessful)
                    {
                        if (res == null)
                            res = new Dictionary<DateTime, List<int>>();
                        List<int> lst = new List<int>(3);
                        lst.Add(nPreviewModuleID);
                        lst.Add(nFullLifeCycleOfPreviewModule);
                        lst.Add(nNonRenewingPeriod);
                        res.Add(dtStartDateOfMPP, lst);
                    }
                }
            }

            return res;
        }

        public static long ParseLongIfNotEmpty(string sStrToParse)
        {
            if (sStrToParse.Length > 0)
                return Int64.Parse(sStrToParse);
            return 0;
        }

        public static UserResponseObject GetExistUser(string sSiteGUID, int nGroupID)
        {
            return Core.Users.Module.GetUserData(nGroupID, sSiteGUID, string.Empty);
        }

        public static bool IsCouponValid(int nGroupID, string sCouponCode, long domainId)
        {
            bool result = false;
            try
            {
                if (!string.IsNullOrEmpty(sCouponCode))
                {
                    CouponDataResponse couponData = Pricing.Module.GetCouponStatus(nGroupID, sCouponCode, domainId);

                    if (couponData != null &&
                        couponData.Status != null &&
                        couponData.Status.Code == (int)eResponseStatus.OK &&
                        couponData.Coupon != null &&
                        couponData.Coupon.m_CouponStatus == CouponsStatus.Valid)
                    {
                        result = true;
                    }
                }
            }
            catch (Exception ex)
            {
                result = false;
                log.Error("IsCouponValid - " + string.Format("Error on IsCouponValid(), group id:{0}, coupon code:{1}, errorMessage:{2}", nGroupID, sCouponCode, ex.ToString()), ex);
            }
            return result;
        }

        public static CouponData GetCouponData(int groupID, string couponCode, long domainId)
        {
            CouponData result = null;
            try
            {
                if (!string.IsNullOrEmpty(couponCode))
                {
                    CouponDataResponse couponResponse = Core.Pricing.Module.GetCouponStatus(groupID, couponCode, domainId);

                    if (couponResponse != null &&
                        couponResponse.Status != null &&
                        couponResponse.Status.Code == (int)eResponseStatus.OK &&
                        couponResponse.Coupon != null &&
                        couponResponse.Coupon.m_CouponStatus == CouponsStatus.Valid)
                    {
                        result = couponResponse.Coupon;
                    }
                }
            }
            catch (Exception ex)
            {
                result = null;
                log.Error("GetCouponData - " + string.Format("Error on GetCouponData(), group id:{0}, coupon code:{1}, errorMessage:{2}", groupID, couponCode, ex.ToString()), ex);
            }

            return result;
        }


        internal static bool IsFirstDeviceEqualToCurrentDevice(int nMediaFileID, string sPPVCode, List<int> lUsersIds, string sCurrentDeviceName, ref string sFirstDeviceName)
        {
            int numOfRowsReturned = 0;

            sFirstDeviceName = ConditionalAccessDAL.Get_FirstDeviceUsedByPPVModule(nMediaFileID, sPPVCode, lUsersIds, out numOfRowsReturned);
            if (numOfRowsReturned == 0)
                return true;
            return sCurrentDeviceName.Equals(sFirstDeviceName);
        }

        /*
         * 1. Caching of pricing items in CAS is deprecated. Now all caching is done on Pricing side.
         * 
         */
        internal static PPVModule GetPPVModuleDataWithCaching<T>(T ppvCode, string wsUsername, string wsPassword,
            int groupID, string countryCd, string langCode, string deviceName)
        {
            return Pricing.Module.GetPPVModuleData(groupID, ppvCode.ToString(), countryCd, langCode, deviceName);
        }

        /*
         * 1. Caching of pricing items in CAS is deprecated. Now all caching is done on Pricing side.
         */
        internal static UsageModule GetUsageModuleDataWithCaching<T>(T usageModuleCode, string wsUsername, string wsPassword,
            string countryCode, string langCode, string deviceName, int groupID, string methodName)
        {
            return Pricing.Module.GetUsageModuleData(groupID, usageModuleCode.ToString(), countryCode, langCode, deviceName);
        }

        internal static bool GetMediaFileIDByCoGuid(string coGuid, int groupID, string siteGuid, ref int mediaFileID)
        {
            string key = LayeredCacheKeys.GetFileByCoGuidKey(coGuid);
            bool cacheResult = LayeredCache.Instance.Get<int>(key, ref mediaFileID, Get_MediaFileIDByCoGuid, new Dictionary<string, object>() { { "groupID", groupID }, { "coGuid", coGuid } },
                                                              groupID, LayeredCacheConfigNames.MEDIA_FILE_ID_BY_CO_GUID_LAYERED_CACHE_CONFIG_NAME);
            if (!cacheResult)
            {
                log.ErrorFormat("fails Get Media FileID By CoGuid groupID:{0}, siteGuid:{1} ", groupID, siteGuid);
            }
            return cacheResult;
        }

        internal static bool GetStreamingUrlType(int fileMainStreamingCoID, ref string CdnStrID)
        {
            bool isDynamic = false;

            string keyUrlType = string.Format("{0}_GetStreamingUrlType_{1}", ApiObjects.eWSModules.CONDITIONALACCESS, fileMainStreamingCoID);
            string keyCDN = string.Format("{0}_GetStreamingCDN_{1}", ApiObjects.eWSModules.CONDITIONALACCESS, fileMainStreamingCoID);

            bool resURL = ConditionalAccessCache.GetItem<bool>(keyUrlType, out isDynamic);
            bool resCDN = ConditionalAccessCache.GetItem<string>(keyCDN, out CdnStrID);

            if (!resURL || !resCDN)
            {

                int nUrlType = DAL.ConditionalAccessDAL.GetStreamingUrlType(fileMainStreamingCoID, ref CdnStrID);
                switch (nUrlType)
                {
                    case (int)eUrlType.Dynamic:
                        isDynamic = true;
                        break;
                    case (int)eUrlType.Static:
                        break;
                    default:
                        break;
                }
                ConditionalAccessCache.AddItem(keyUrlType, isDynamic);
                ConditionalAccessCache.AddItem(keyCDN, CdnStrID);

            }

            return isDynamic;
        }

        internal static string GetDateFormat(DateTime dateTime, string formatDate)
        {
            if (dateTime != null)
            {
                return dateTime.ToString(formatDate);
            }
            return string.Empty;
        }

        internal static bool IsUserValid(string siteGuid, int groupID, ref int domainID, ref DAL.DomainSuspentionStatus eSuspnedStatus)
        {
            bool res = false;

            long temp = 0;
            if (!Int64.TryParse(siteGuid, out temp) || temp < 1)
                return false;

            UserResponseObject resp = Core.Users.Module.GetUserData(groupID, siteGuid, string.Empty);
            if (resp != null && resp.m_RespStatus == ResponseStatus.OK && resp.m_user != null && resp.m_user.m_domianID > 0)
            {
                domainID = resp.m_user.m_domianID;
                eSuspnedStatus = resp.m_user.m_eSuspendState;
                res = true;
            }
            else
            {
                res = false;
            }

            return res;
        }

        /// <summary>
        /// Returns a full domain object for a given ID
        /// </summary>
        /// <param name="p_nDomainId"></param>
        /// <param name="p_nGroupId"></param>
        /// <returns></returns>
        public static Domain GetDomainInfo(int p_nDomainId, int p_nGroupId)
        {
            Domain oDomain = null;

            try
            {
                var res = Core.Domains.Module.GetDomainInfo(p_nGroupId, p_nDomainId);
                if (res != null && res.Status != null && res.Status.Code == (int)eResponseStatus.OK)
                {
                    oDomain = res.Domain;
                }
            }
            catch (Exception ex)
            {
                log.Error("Exception - " +
                    string.Format("Failed getting domain info from WS. Domain Id = {0}, Group Id = {1}, Msg = {2}", p_nDomainId, p_nGroupId, ex.Message), ex);
            }
            return (oDomain);
        }

        public static ChangeDLMObj ChangeDLM(int groupID, long domainId, int dlmID)
        {
            ChangeDLMObj changeDLMObj = null;

            try
            {
                changeDLMObj = Core.Domains.Module.ChangeDLM(groupID, (int)domainId, dlmID);
            }
            catch (Exception ex)
            {
                log.Error("Exception - " + string.Format("Failed changing DLM using Domains WS. domainId = {0}, dlmID = {1}, groupID = {2}, Msg = {3}", domainId, dlmID, groupID, ex.Message), ex);
            }
            return changeDLMObj;
        }


        internal static Tuple<Dictionary<string, DataTable>, bool> Get_FileAndMediaBasicDetails(Dictionary<string, object> funcParams)
        {
            bool res = false;
            Dictionary<string, DataTable> result = new Dictionary<string, DataTable>();
            try
            {
                if (funcParams != null && funcParams.ContainsKey("fileIDs") && funcParams.ContainsKey("groupId"))
                {
                    string key = string.Empty;
                    int[] fileIDs;
                    int? groupId = funcParams["groupId"] as int?;
                    if (funcParams.ContainsKey(LayeredCache.MISSING_KEYS) && funcParams[LayeredCache.MISSING_KEYS] != null)
                    {
                        fileIDs = ((List<string>)funcParams[LayeredCache.MISSING_KEYS]).Select(x => int.Parse(x)).ToArray();
                    }
                    else
                    {
                        fileIDs = funcParams["fileIDs"] != null ? funcParams["fileIDs"] as int[] : null;
                    }

                    if (fileIDs != null && groupId.HasValue)
                    {
                        DataTable dt = Tvinci.Core.DAL.CatalogDAL.Get_ValidateMediaFiles(fileIDs, groupId.Value);
                        if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                        {
                            DataTable tempDt;
                            foreach (DataRow dr in dt.Rows)
                            {
                                tempDt = dt.Clone();
                                tempDt.ImportRow(dr);
                                result.Add(ODBCWrapper.Utils.GetSafeStr(dr, "media_file_id"), tempDt);
                            }
                        }

                        List<int> missingKeys = fileIDs.Where(x => !result.ContainsKey(x.ToString())).ToList();
                        if (missingKeys != null)
                        {
                            DataTable tempDt = dt != null ? dt.Clone() : new DataTable();
                            foreach (int missingKey in missingKeys)
                            {
                                result.Add(missingKey.ToString(), tempDt);
                            }
                        }
                    }
                    res = result.Keys.Count() == fileIDs.Count();

                    result = result.ToDictionary(x => LayeredCacheKeys.GetFileAndMediaBasicDetailsKey(int.Parse(x.Key), groupId.Value), x => x.Value);
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Get_FileAndMediaBasicDetails failed params : {0}", string.Join(";", funcParams.Keys)), ex);
            }
            return new Tuple<Dictionary<string, DataTable>, bool>(result, res);
        }

        // build dictionary - for each media file get one priceResonStatus mediaFilesStatus NotForPurchase, if UnKnown need to continue check that mediafile
        internal static Dictionary<int, MediaFileStatus> ValidateMediaFiles(int[] nMediaFiles, ref Dictionary<int, string> mediaFilesProductCode,
            int groupId, int countryId, bool withMediaFilesInvalidation = false)
        {
            Dictionary<int, MediaFileStatus> mediaFilesStatus = new Dictionary<int, MediaFileStatus>();
            mediaFilesProductCode = new Dictionary<int, string>();
            try
            {
                string productCode = string.Empty;
                MediaFileStatus eMediaFileStatus = MediaFileStatus.OK;
                bool isOpc = false;

                Dictionary<int, int> mapperDic = new Dictionary<int, int>();

                if (withMediaFilesInvalidation)
                {
                    isOpc = CatalogManager.Instance.DoesGroupUsesTemplates(groupId);
                    MeidaMaper[] mapper = GetMediaMapper(groupId, nMediaFiles);
                    if (mapper != null)
                    {
                        foreach (var item in mapper)
                        {
                            if (!mapperDic.ContainsKey(item.m_nMediaFileID))
                            {
                                mapperDic.Add(item.m_nMediaFileID, item.m_nMediaID);
                            }
                        }
                    }
                }

                Dictionary<string, string> keysToOriginalValueMap = new Dictionary<string, string>();
                Dictionary<string, List<string>> invalidationKeysMap = new Dictionary<string, List<string>>();

                //initialize all status as OK 
                foreach (int mf in nMediaFiles)
                {
                    if (!mediaFilesStatus.ContainsKey(mf))
                    {
                        mediaFilesStatus.Add(mf, eMediaFileStatus);
                        mediaFilesProductCode.Add(mf, productCode);
                    }

                    string mfKey = LayeredCacheKeys.GetFileAndMediaBasicDetailsKey(mf, groupId);
                    keysToOriginalValueMap.Add(mfKey, mf.ToString());

                    if (mapperDic.ContainsKey(mf))
                    {
                        string invalidationKey = LayeredCacheKeys.GetAssetInvalidationKey(groupId, eAssetTypes.MEDIA.ToString(), mapperDic[mf]);
                        
                        if (!isOpc)
                        {
                            invalidationKey = LayeredCacheKeys.GetMediaInvalidationKey(groupId, mapperDic[mf]);
                        }
                        
                        invalidationKeysMap.Add(mfKey, new List<string>() { invalidationKey });
                    }
                }

                // get basic file details from cach / DB                 
                Dictionary<string, DataTable> fileDatatables = null;

                // try to get from cache            
                bool cacheResult = LayeredCache.Instance.GetValues<DataTable>(keysToOriginalValueMap, ref fileDatatables, Get_FileAndMediaBasicDetails, new Dictionary<string, object>() { { "fileIDs", nMediaFiles }, { "groupId", groupId } },
                                                                                groupId, LayeredCacheConfigNames.VALIDATE_MEDIA_FILES_LAYERED_CACHE_CONFIG_NAME, invalidationKeysMap);

                long mediaId = 0;
                int mediaFileID;
                int mediaIsActive = 0, mediaFileIsActive = 0;
                int mediaStatus = 0, mediaFileStatus = 0;
                DateTime mediaStartDate, mediaFileStartDate;
                DateTime? mediaEndDate, mediaFileEndDate, mediaFinalEndDate, mediaFileCatalogEndDate;
                DateTime currentDate;

                if (cacheResult && fileDatatables != null)
                {
                    // get the media_file_id from key
                    // find keys not exsits in result
                    List<string> missingKeys = fileDatatables.Where(kvp => kvp.Value == null || kvp.Value.Rows == null || kvp.Value.Rows.Count == 0).Select(kvp => kvp.Key).ToList();
                    if (missingKeys != null && missingKeys.Count > 0)
                    {
                        foreach (string missKey in missingKeys)
                        {
                            //todo
                            int mf = 0;
                            string[] missKeyArray = missKey.Split('_');
                            if (missKeyArray != null && missKeyArray.Count() > 2)
                            {
                                mf = int.Parse(missKeyArray[2]);
                            }

                            if (mediaFilesStatus.ContainsKey(mf))
                            {
                                mediaFilesStatus[mf] = MediaFileStatus.NotForPurchase;
                            }
                            else
                            {
                                mediaFilesStatus.Add(mf, MediaFileStatus.NotForPurchase);
                            }
                        }
                    }

                    Group group = new GroupManager().GetGroup(groupId);
                    foreach (DataTable dt in fileDatatables.Values)
                    {
                        eMediaFileStatus = MediaFileStatus.OK;

                        if (dt != null && dt.Rows != null && dt.Rows.Count == 1)
                        {
                            DataRow dr = dt.Rows[0];
                            currentDate = DateTime.UtcNow;

                            mediaFileID = ODBCWrapper.Utils.GetIntSafeVal(dr, "media_file_id");
                            productCode = ODBCWrapper.Utils.GetSafeStr(dr, "Product_Code");

                            if (!mediaFilesProductCode.ContainsKey(mediaFileID))
                            {
                                mediaFilesProductCode.Add(mediaFileID, productCode);
                            }
                            else
                            {
                                mediaFilesProductCode[mediaFileID] = productCode;
                            }
                            mediaId = ODBCWrapper.Utils.GetIntSafeVal(dr, "media_id");

                            bool isGeoAvailability;
                            if (Core.Api.api.IsMediaBlockedForCountryGeoAvailability(groupId, countryId, mediaId, out isGeoAvailability, group))
                            {
                                eMediaFileStatus = MediaFileStatus.NotForPurchase;
                            }

                            if (eMediaFileStatus == MediaFileStatus.OK)
                            {
                                //media
                                mediaIsActive = ODBCWrapper.Utils.GetIntSafeVal(dr, "media_is_active");
                                mediaStatus = ODBCWrapper.Utils.GetIntSafeVal(dr, "media_status");
                                mediaStartDate = ODBCWrapper.Utils.GetDateSafeVal(dr, "media_start_date");
                                mediaEndDate = ODBCWrapper.Utils.GetNullableDateSafeVal(dr, "media_end_date");
                                mediaFinalEndDate = ODBCWrapper.Utils.GetNullableDateSafeVal(dr, "media_final_end_date");

                                //mediaFiles
                                mediaFileIsActive = ODBCWrapper.Utils.GetIntSafeVal(dr, "file_is_active");
                                mediaFileStatus = ODBCWrapper.Utils.GetIntSafeVal(dr, "file_status");
                                mediaFileStartDate = ODBCWrapper.Utils.GetDateSafeVal(dr, "file_start_date");
                                mediaFileEndDate = ODBCWrapper.Utils.GetNullableDateSafeVal(dr, "file_end_date");
                                mediaFileCatalogEndDate = ODBCWrapper.Utils.GetNullableDateSafeVal(dr, "file_catalog_end_date");


                                if (mediaIsActive != 1 || mediaStatus != 1 || mediaFileIsActive != 1 || mediaFileStatus != 1)
                                {
                                    eMediaFileStatus = MediaFileStatus.NotForPurchase;
                                }
                                else if (!isGeoAvailability && (mediaStartDate > currentDate || mediaFileStartDate > currentDate))
                                {
                                    eMediaFileStatus = MediaFileStatus.NotForPurchase;
                                }
                                else if (!isGeoAvailability && ((mediaFinalEndDate.HasValue && mediaFinalEndDate.Value < currentDate) || (mediaFileEndDate.HasValue && mediaFileEndDate.Value < currentDate)))
                                {
                                    eMediaFileStatus = MediaFileStatus.NotForPurchase;
                                }
                                else if (!isGeoAvailability && ((mediaEndDate.HasValue && mediaEndDate.Value < currentDate) &&
                                    (!mediaFinalEndDate.HasValue || (mediaFinalEndDate.HasValue && mediaFinalEndDate.Value > currentDate)))) // can see only if purchased
                                {
                                    eMediaFileStatus = MediaFileStatus.ValidOnlyIfPurchase;
                                }
                                else if (!isGeoAvailability && (mediaFileCatalogEndDate.HasValue && mediaFileCatalogEndDate.Value < currentDate))
                                {
                                    eMediaFileStatus = MediaFileStatus.ValidOnlyIfPurchase;
                                }
                            }

                            if (eMediaFileStatus != MediaFileStatus.OK)
                            {
                                if (mediaFilesStatus.ContainsKey(mediaFileID))
                                {
                                    mediaFilesStatus[mediaFileID] = eMediaFileStatus;
                                }
                                else
                                {
                                    mediaFilesStatus.Add(mediaFileID, eMediaFileStatus);
                                }
                            }
                        }
                    }
                }
                else
                {
                    log.Error(string.Format("ValidateMediaFiles -  LayeredCache return false for keys :{0}", string.Join(",", keysToOriginalValueMap.Keys)));
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Empty, ex);
            }
            return mediaFilesStatus;
        }

        /// <summary>
        /// Validates that a user exists and belongs to a given domain
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="siteGuid"></param>
        /// <param name="domainId"></param>
        /// <returns></returns>
        public static ResponseStatus ValidateUser(int groupId, string siteGuid, ref long houseHoldID, bool ignoreSuspend = false, bool operatorDefaultSuspendBlock = false)
        {
            Users.User user;

            return ValidateUser(groupId, siteGuid, ref houseHoldID, out user, ignoreSuspend, operatorDefaultSuspendBlock);
        }

        /// <summary>
        /// Validates that a user exists and belongs to a given domain
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="siteGuid"></param>
        /// <param name="domainId"></param>
        /// <returns></returns>
        public static ResponseStatus ValidateUser(int groupId, string siteGuid, ref long houseHoldID, out Users.User user, bool ignoreSuspend = false, bool operatorDefaultSuspendBlock = false)
        {
            user = null;
            ResponseStatus status = ResponseStatus.InternalError;
            long lSiteGuid = 0;
            if (siteGuid.Length == 0 || !Int64.TryParse(siteGuid, out lSiteGuid) || lSiteGuid == 0)
            {
                status = ResponseStatus.UserDoesNotExist;
                return status;
            }

            try
            {
                UserResponseObject response = Core.Users.Module.GetUserData(groupId, siteGuid, string.Empty);

                // Make sure response is OK
                if (response != null)
                {
                    status = response.m_RespStatus;

                    if (status == ResponseStatus.OK)
                    {
                        //check Domain and suspend
                        if (response.m_user != null)
                        {
                            user = response.m_user;

                            if (houseHoldID != 0 && houseHoldID != response.m_user.m_domianID)
                            {
                                status = ResponseStatus.UserNotIndDomain;
                            }
                            else // no domain id was sent
                            {
                                houseHoldID = response.m_user.m_domianID;

                                if (houseHoldID == 0)
                                {
                                    status = ResponseStatus.UserNotIndDomain;
                                }
                            }

                            if (response.m_user.m_eSuspendState == DAL.DomainSuspentionStatus.Suspended
                                && !ignoreSuspend
                                && !RolesPermissionsManager.Instance.AllowActionInSuspendedDomain(groupId, long.Parse(siteGuid), !operatorDefaultSuspendBlock))
                            {
                                status = ResponseStatus.UserSuspended;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("ValidateUser - " + string.Format("Error when validating user {0} in group {1}. ex = {2}, ST = {3}", siteGuid, groupId, ex.Message, ex.StackTrace), ex);
            }

            return status;
        }

        /// <summary>
        /// Validates that a domain exists
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="domainId"></param>
        /// <returns></returns>
        public static ApiObjects.Response.Status ValidateDomain(int groupId, int domainId, out Domain domain)
        {
            domain = null;

            ApiObjects.Response.Status status = new ApiObjects.Response.Status() { Code = (int)eResponseStatus.Error, Message = "Error validating domain" };

            try
            {
                DomainResponse response = Core.Domains.Module.GetDomainInfo(groupId, domainId);
                status = new ApiObjects.Response.Status(response.Status.Code, response.Status.Message);

                domain = response.Domain;
            }
            catch (Exception ex)
            {
                log.Error("ValidateDomain - " +
                    string.Format("Error when validating domain {0} in group {1}. ex = {2}, ST = {3}", domainId, groupId, ex.Message, ex.StackTrace),
                    ex);
            }
            return status;
        }

        public static int CalcPaymentNumber(int nNumOfPayments, int nPaymentNumber, bool bIsPurchasedWithPreviewModule)
        {
            int res = nPaymentNumber;
            if (nPaymentNumber == 0 && bIsPurchasedWithPreviewModule)
                res = 0;
            else
            {
                if (nNumOfPayments != 0)
                {
                    res = nPaymentNumber % nNumOfPayments;
                    if (res == 0)
                        res = nNumOfPayments;
                }
            }
            return res;
        }

        private static bool IsUserEntitled(bool isRelated, string p_sPPVCode, ref int p_nPPVID, ref string p_sSubCode, ref string p_sPPCode, ref int p_nWaiver, ref DateTime p_dCreateDate,
                                            ref string p_sPurchasedBySiteGuid, ref int p_nPurchasedAsMediaFileID, ref DateTime? p_dtStartDate, ref DateTime? p_dtEndDate,
                                            Dictionary<string, EntitlementObject> entitlements, int mediaFileId, HashSet<int> files, ref bool isPending)
        {
            bool res = false;
            int ppvId;

            if (entitlements.Count > 0 && int.TryParse(p_sPPVCode, out ppvId) && ppvId > 0)
            {
                foreach (EntitlementObject ppv in entitlements.Values)
                {
                    if (ppv.ppvCode == ppvId && (ppv.purchasedAsMediaFileID == mediaFileId || (isRelated && files.Contains(ppv.purchasedAsMediaFileID))))
                    {
                        p_nPPVID = ppv.ID;
                        p_sSubCode = ppv.subscriptionCode;
                        p_sPPCode = ppv.relPP.ToString();
                        p_nWaiver = ppv.waiver;
                        p_dtStartDate = ppv.startDate;
                        p_dtEndDate = ppv.endDate;
                        p_dCreateDate = ppv.createDate;
                        p_sPurchasedBySiteGuid = ppv.purchasedBySiteGuid;
                        p_nPurchasedAsMediaFileID = ppv.purchasedAsMediaFileID;
                        res = true;
                        isPending = ppv.isPending;
                        if (!isPending)
                            break;
                    }
                }
            }

            return res;
        }

        internal static DomainEntitlementsCache.PPVEntitlements InitializeDomainPpvs(int groupId, int domainId, List<int> allUsersInDomain)
        {
            DomainEntitlementsCache.PPVEntitlements domainPpvEntitlements = new DomainEntitlementsCache.PPVEntitlements();
            try
            {
                // Get all user entitlements
                domainPpvEntitlements.EntitlementsDictionary = ConditionalAccessDAL.Get_AllUsersEntitlements(domainId, allUsersInDomain);
            }

            catch (Exception ex)
            {
                log.Error(string.Format("Failed InitializeDomainPpvs, groupId: {0}, domainId: {1}, allUsersInDomain: {2}", groupId, domainId, allUsersInDomain != null ? string.Join(",", allUsersInDomain) : ""), ex);
            }

            return domainPpvEntitlements;
        }

        private static List<int> GetRelatedFileIDs(int mediaID, int[] ppvGroupFileTypes, Dictionary<string, List<int>> mediaIdGroupFileTypeMappings)
        {
            List<int> relatedFileTypes = new List<int>();
            if (ppvGroupFileTypes != null && ppvGroupFileTypes.Length > 0 && mediaIdGroupFileTypeMappings.Count > 0)
            {
                foreach (int groupFileTypeID in ppvGroupFileTypes)
                {
                    string mapKey = mediaID + "_" + groupFileTypeID;
                    if (mediaIdGroupFileTypeMappings.ContainsKey(mapKey))
                    {
                        relatedFileTypes.AddRange(mediaIdGroupFileTypeMappings[mapKey]);
                    }
                }
            }
            else
            {
                foreach (int mediaFileID in mediaIdGroupFileTypeMappings.Where(dic => dic.Key.StartsWith(mediaID.ToString())).SelectMany(dic => dic.Value))
                {
                    relatedFileTypes.Add(mediaFileID);
                }
            }

            return relatedFileTypes.ToList();
        }

        internal static void GetAllUserBundles(int nGroupID, int domainID, List<int> lstUserIDs, ref DomainEntitlementsCache.BundleEntitlements userBundleEntitlements)
        {
            DataSet dataSet = ConditionalAccessDAL.Get_AllBundlesInfoByUserIDsOrDomainID(domainID, lstUserIDs, nGroupID);
            if (dataSet != null && IsBundlesDataSetValid(dataSet))
            {
                userBundleEntitlements.EntitledSubscriptions = new Dictionary<string, UserBundlePurchase>();
                userBundleEntitlements.EntitledCollections = new Dictionary<string, UserBundlePurchase>();
                // iterate over subscriptions
                DataTable subs = dataSet.Tables[0];
                int waiver = 0;
                DateTime purchaseDate = DateTime.MinValue;
                DateTime endDate = DateTime.MinValue;

                if (subs != null && subs.Rows != null && subs.Rows.Count > 0)
                {
                    DataRow[] subsRows = subs.Select().OrderBy(u => u["END_DATE"]).ToArray();

                    foreach (var subsRow in subsRows)
                    {
                        int numOfUses = 0;
                        int maxNumOfUses = 0;
                        string bundleCode = string.Empty;
                        int gracePeriodMinutes = 0;
                        bool isSuspend = false;
                        bool isPending = false;
                        GetSubscriptionBundlePurchaseData(subsRow, "SUBSCRIPTION_CODE", ref numOfUses, ref maxNumOfUses, ref bundleCode, ref waiver,
                                                            ref purchaseDate, ref endDate, ref gracePeriodMinutes, ref isSuspend, ref isPending);

                        // decide which is the correct end period
                        if (endDate < DateTime.UtcNow)
                            endDate = endDate.AddMinutes(gracePeriodMinutes);

                        int subCode = 0;
                        if (Int32.TryParse(bundleCode, out subCode) && subCode > 0)
                        {
                            if (!userBundleEntitlements.EntitledSubscriptions.ContainsKey(bundleCode))
                            {
                                userBundleEntitlements.EntitledSubscriptions.Add(bundleCode, new UserBundlePurchase()
                                {
                                    sBundleCode = bundleCode,
                                    nWaiver = waiver,
                                    dtPurchaseDate = purchaseDate,
                                    dtEndDate = endDate,
                                    nNumOfUses = numOfUses,
                                    nMaxNumOfUses = maxNumOfUses,
                                    isPending = isPending
                                });
                            }
                        }
                    }
                }

                //iterate over collections
                DataTable colls = dataSet.Tables[1];
                if (colls != null && colls.Rows != null && colls.Rows.Count > 0)
                {
                    for (int i = 0; i < colls.Rows.Count; i++)
                    {
                        int numOfUses = 0;
                        int maxNumOfUses = 0;
                        string bundleCode = string.Empty;
                        waiver = 0;
                        purchaseDate = DateTime.MinValue;
                        endDate = DateTime.MinValue;
                        bool isPending = false;

                        GetCollectionBundlePurchaseData(colls.Rows[i], "COLLECTION_CODE", ref numOfUses, ref maxNumOfUses, ref bundleCode, ref waiver, ref purchaseDate, ref endDate, ref isPending);

                        int collCode = 0;
                        if (Int32.TryParse(bundleCode, out collCode) && collCode > 0)
                        {
                            if (!userBundleEntitlements.EntitledCollections.ContainsKey(bundleCode) && endDate >= DateTime.UtcNow)
                            {
                                userBundleEntitlements.EntitledCollections.Add(bundleCode, new UserBundlePurchase()
                                {
                                    sBundleCode = bundleCode,
                                    nWaiver = waiver,
                                    dtPurchaseDate = purchaseDate,
                                    dtEndDate = endDate,
                                    nNumOfUses = numOfUses,
                                    nMaxNumOfUses = maxNumOfUses,
                                    isPending = isPending
                                });
                            }
                        }
                        else
                        {
                            //log
                        }
                    }
                }
            }
            else
            {
                #region Logging
                StringBuilder sb = new StringBuilder("SP: ConditionalAccessDAL.Get_AllBundlesInfoByUserIDsOrDomainID returned corrupted data. ");
                if (lstUserIDs != null && lstUserIDs.Count > 0)
                {
                    sb.Append(" User IDs: ");
                    for (int i = 0; i < lstUserIDs.Count; i++)
                    {
                        sb.Append(String.Concat(lstUserIDs[i], ", "));
                    }
                }
                else
                {
                    sb.Append(" User IDs is null or empty. ");
                }
                sb.Append(string.Format(" domainID: {0}, group_id: {1}", domainID, nGroupID));
                log.Error("Error - " + sb.ToString());
                #endregion
            }
        }

        internal static DomainEntitlementsCache.BundleEntitlements InitializeDomainBundles(int domainId, int groupId, List<int> allUsersInDomain)
        {
            DomainEntitlementsCache.BundleEntitlements domainBundleEntitlements = new DomainEntitlementsCache.BundleEntitlements();
            try
            {
                GetAllUserBundles(groupId, domainId, allUsersInDomain, ref domainBundleEntitlements);
                //removed didn't find any usage for that
                // if (shouldPopulateBundles)
                // {
                //     PopulateDomainBundles(domainId, groupId, domainBundleEntitlements);
                // }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed domainBundleEntitlements, groupId: {0}, domainId: {1}, allUsersInDomain: {2}", groupId, domainId, allUsersInDomain != null ? string.Join(",", allUsersInDomain) : ""), ex);
            }

            return domainBundleEntitlements;
        }

        internal static void PopulateDomainBundles(int domainId, int groupId, DomainEntitlements.BundleEntitlements domainBundleEntitlements)
        {
            domainBundleEntitlements.ChannelsToCollectionsMappings = new Dictionary<int, List<Collection>>();
            domainBundleEntitlements.ChannelsToSubscriptionMappings = new Dictionary<int, List<Subscription>>();
            domainBundleEntitlements.CollectionsData = new Dictionary<int, Collection>();
            domainBundleEntitlements.SubscriptionsData = new Dictionary<int, Subscription>();
            domainBundleEntitlements.FileTypeIdToSubscriptionMappings = new Dictionary<int, List<Subscription>>();

            try
            {
                if (domainBundleEntitlements.EntitledSubscriptions != null && domainBundleEntitlements.EntitledSubscriptions.Count > 0)
                {
                    HashSet<long> subIds = new HashSet<long>();
                    foreach (var item in domainBundleEntitlements.EntitledSubscriptions.Keys)
                    {
                        subIds.Add(long.Parse(item));
                    }

                    SubscriptionsResponse subscriptionsResponse = Core.Pricing.Module.Instance.GetSubscriptions(groupId, subIds, String.Empty, String.Empty,
                        String.Empty, null);

                    if (subscriptionsResponse != null && subscriptionsResponse.Status.Code == (int)eResponseStatus.OK && subscriptionsResponse.Subscriptions.Count() > 0)
                    {
                        foreach (Subscription subscription in subscriptionsResponse.Subscriptions)
                        {
                            // Insert to subscriptionData if subscriptionCode isn't already contained
                            int subscriptionCode;
                            if (int.TryParse(subscription.m_sObjectCode, out subscriptionCode) && !domainBundleEntitlements.SubscriptionsData.ContainsKey(subscriptionCode))
                            {
                                domainBundleEntitlements.SubscriptionsData.Add(subscriptionCode, subscription);
                            }

                            // Insert to channelsToSubscriptionMappings
                            if (subscription.m_sCodes != null)
                            {
                                foreach (BundleCodeContainer bundleCode in subscription.m_sCodes)
                                {
                                    int channelID;
                                    if (int.TryParse(bundleCode.m_sCode, out channelID) && domainBundleEntitlements.ChannelsToSubscriptionMappings.ContainsKey(channelID))
                                    {
                                        domainBundleEntitlements.ChannelsToSubscriptionMappings[channelID].Add(subscription);
                                    }
                                    else if (channelID > 0)
                                    {
                                        domainBundleEntitlements.ChannelsToSubscriptionMappings.Add(channelID, new List<Subscription>() { subscription });
                                    }
                                }
                            }

                            // Insert to fileTypeIdToSubscriptionMappings
                            if (subscription.m_sFileTypes != null && subscription.m_sFileTypes.Count() > 0)
                            {
                                foreach (int fileTypeID in subscription.m_sFileTypes)
                                {
                                    if (domainBundleEntitlements.FileTypeIdToSubscriptionMappings.ContainsKey(fileTypeID))
                                    {
                                        domainBundleEntitlements.FileTypeIdToSubscriptionMappings[fileTypeID].Add(subscription);
                                    }
                                    else
                                    {
                                        domainBundleEntitlements.FileTypeIdToSubscriptionMappings.Add(fileTypeID, new List<Subscription>() { subscription });
                                    }
                                }
                            }
                            else
                            {
                                if (domainBundleEntitlements.FileTypeIdToSubscriptionMappings.ContainsKey(0))
                                {
                                    domainBundleEntitlements.FileTypeIdToSubscriptionMappings[0].Add(subscription);
                                }
                                else
                                {
                                    domainBundleEntitlements.FileTypeIdToSubscriptionMappings.Add(0, new List<Subscription>() { subscription });
                                }
                            }
                        }
                    }
                }

                if (domainBundleEntitlements.EntitledCollections != null && domainBundleEntitlements.EntitledCollections.Count > 0)
                {
                    CollectionsResponse collectionsResponse = Core.Pricing.Module.GetCollectionsData(groupId, domainBundleEntitlements.EntitledCollections.Keys.ToArray(), String.Empty, String.Empty, String.Empty);
                    if (collectionsResponse != null && collectionsResponse.Status.Code == (int)eResponseStatus.OK && collectionsResponse.Collections != null && collectionsResponse.Collections.Length > 0)
                    {
                        foreach (Collection collection in collectionsResponse.Collections)
                        {
                            int collectionCode;
                            if (int.TryParse(collection.m_sObjectCode, out collectionCode) && !domainBundleEntitlements.CollectionsData.ContainsKey(collectionCode))
                            {
                                domainBundleEntitlements.CollectionsData.Add(collectionCode, collection);

                                // Insert to channelsToSubscriptionMappings
                                if (collection.m_sCodes != null)
                                {
                                    foreach (BundleCodeContainer bundleCode in collection.m_sCodes)
                                    {
                                        int channelID;
                                        if (int.TryParse(bundleCode.m_sCode, out channelID) && domainBundleEntitlements.ChannelsToCollectionsMappings.ContainsKey(channelID))
                                        {
                                            domainBundleEntitlements.ChannelsToCollectionsMappings[channelID].Add(collection);
                                        }
                                        else if (channelID > 0)
                                        {
                                            domainBundleEntitlements.ChannelsToCollectionsMappings.Add(channelID, new List<Collection>() { collection });
                                        }
                                    }
                                }

                            }

                            // Insert to fileTypeIdToCollectionMappings
                            if (collection.m_sFileTypes != null && collection.m_sFileTypes.Count() > 0)
                            {

                                foreach (int fileTypeID in collection.m_sFileTypes)
                                {
                                    if (domainBundleEntitlements.FileTypeIdToCollectionMappings.ContainsKey(fileTypeID))
                                    {
                                        domainBundleEntitlements.FileTypeIdToCollectionMappings[fileTypeID].Add(collection);
                                    }
                                    else
                                    {
                                        domainBundleEntitlements.FileTypeIdToCollectionMappings.Add(fileTypeID, new List<Collection>() { collection });
                                    }
                                }
                            }
                            else
                            {
                                if (domainBundleEntitlements.FileTypeIdToCollectionMappings.ContainsKey(0))
                                {
                                    domainBundleEntitlements.FileTypeIdToCollectionMappings[0].Add(collection);
                                }
                                else
                                {
                                    domainBundleEntitlements.FileTypeIdToCollectionMappings.Add(0, new List<Collection>() { collection });
                                }
                            }
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                log.Error(string.Format("Failed PopulateDomainBundles, groupId: {0}, domainId: {1}", groupId, domainId), ex);
            }
        }

        private static void GetUserValidBundles(int mediaID, int nMediaFileID, MediaFileStatus eMediaFileStatus, int nGroupID, int[] fileTypes, List<int> allUserIDsInDomain,
                List<int> relatedMediaFileIDs, Dictionary<string, UserBundlePurchase> subsPurchase, Dictionary<string, UserBundlePurchase> collPurchases,
                Dictionary<int, List<Subscription>> fileTypeIdToSubscriptionMappings, Dictionary<int, Subscription> subscriptionsData, Dictionary<int, Collection> collectionsData,
                Dictionary<int, List<Subscription>> channelsToSubscriptionMappings, Dictionary<int, List<Collection>> channelsToCollectionsMappings,
                ref Subscription[] relevantValidSubscriptions, ref Collection[] relevantValidCollections, Dictionary<int, List<Collection>> fileTypeIdToCollectionMappings)
        {
            //todo channelsToSubscriptionMappings for collection
            var subsToBundleCreditDownloadedQuery = new List<string>();
            var colsToBundleCreditDownloadedQuery = new List<string>();
            var subsToGetFromSubsDictionary = new List<int>();
            var collsToGetFromDictionary = new List<int>();

            if (subsPurchase?.Count > 0 && fileTypeIdToSubscriptionMappings.Count > 0)
            {
                // the subscriptions and collections we add to those list will be sent to the Catalog in order to determine whether the media given as input belongs to it.                
                var subscriptionsToCheck = new List<UserBundlePurchase>();

                // subscriptions with all fileTypes
                int allFileTypeIDs_key = 0;
                AddSubscriptionsToCheck(allFileTypeIDs_key, ref subscriptionsToCheck, fileTypeIdToSubscriptionMappings, subsPurchase);

                // subscriptions with the current fileTypeID
                foreach (int filetypeID in fileTypes)
                {
                    AddSubscriptionsToCheck(filetypeID, ref subscriptionsToCheck, fileTypeIdToSubscriptionMappings, subsPurchase);
                }

                AddUserBundlePurchasesToReleventList(subscriptionsToCheck, eMediaFileStatus, ref subsToBundleCreditDownloadedQuery, ref subsToGetFromSubsDictionary);
            }

            if (collPurchases?.Count > 0 && fileTypeIdToCollectionMappings?.Count > 0)
            {
                var collectionToCheck = new List<UserBundlePurchase>();

                // collections with all fileTypes
                int allFileTypeIDs_key = 0;
                AddCollectionsToCheck(allFileTypeIDs_key, ref collectionToCheck, fileTypeIdToCollectionMappings, collPurchases);

                // collections with the current fileTypeID
                foreach (int filetypeID in fileTypes)
                {
                    AddCollectionsToCheck(filetypeID, ref collectionToCheck, fileTypeIdToCollectionMappings, collPurchases);
                }

                AddUserBundlePurchasesToReleventList(collectionToCheck, eMediaFileStatus, ref colsToBundleCreditDownloadedQuery, ref collsToGetFromDictionary);
            }

            // check if credit need to be downloaded for specific mediaFileID 
            HandleBundleCreditNeedToDownloadedQuery(subsToBundleCreditDownloadedQuery, colsToBundleCreditDownloadedQuery, nMediaFileID, nGroupID, allUserIDsInDomain,
                                                    relatedMediaFileIDs, ref subsToGetFromSubsDictionary, ref collsToGetFromDictionary);

            // the subs / collections already purchased (no need to download credit)
            if (eMediaFileStatus == MediaFileStatus.ValidOnlyIfPurchase)
            {
                List<Subscription> relevantValidSubscriptionsList = new List<Subscription>();
                List<Collection> relevantValidCollectionsList = new List<Collection>();
                //get subscriptions data from dictionary instead of going to catalog
                foreach (int subscriptionCode in subsToGetFromSubsDictionary)
                {
                    //get subscription from all subscriptions data dictionary
                    relevantValidSubscriptionsList.Add(subscriptionsData[subscriptionCode]);
                }

                relevantValidSubscriptions = relevantValidSubscriptionsList.ToArray();

                //get collections data from dictionary instead of going to catalog
                foreach (int collectionCode in subsToGetFromSubsDictionary)
                {
                    //get subscription from all subscriptions data dictionary
                    relevantValidCollectionsList.Add(collectionsData[collectionCode]);
                }

                relevantValidCollections = relevantValidCollectionsList.ToArray();
            }
            else // only if in the gap between end date to final end date - continue the check
            {
                // get distinct subs from subs list, same for collection
                List<int> validatedColls = new List<int>();
                var channelsToCheck = new HashSet<int>();

                foreach (int subsCode in subsToGetFromSubsDictionary.Distinct().ToList())
                {
                    if (subscriptionsData[subsCode].m_sCodes != null)
                    {
                        foreach (var bundleCode in subscriptionsData[subsCode].m_sCodes)
                        {
                            if (int.TryParse(bundleCode.m_sCode, out int channelID) && !channelsToCheck.Contains(channelID))
                            {
                                channelsToCheck.Add(channelID);
                            }
                        }
                    }
                }

                foreach (int collCode in collsToGetFromDictionary.Distinct().ToList())
                {
                    if (collectionsData[collCode].m_sCodes != null)
                    {
                        foreach (var bundleCode in collectionsData[collCode].m_sCodes)
                        {
                            if (int.TryParse(bundleCode.m_sCode, out int channelID) && !channelsToCheck.Contains(channelID))
                            {
                                channelsToCheck.Add(channelID);
                            }
                        }
                    }
                }

                List<int> validatedChannels = null;
                if (channelsToCheck.Count > 0)
                {
                    validatedChannels = ValidateMediaContainedInChannels(mediaID, nGroupID, channelsToCheck);
                }

                if (validatedChannels != null)
                {
                    List<Subscription> relevantValidSubscriptionsList = new List<Subscription>();
                    foreach (int channelID in validatedChannels)
                    {
                        //get subscriptions data from dictionary instead of going to catalog
                        if (channelsToSubscriptionMappings.ContainsKey(channelID))
                        {
                            relevantValidSubscriptionsList.AddRange(channelsToSubscriptionMappings[channelID]);
                        }

                        // save validated collections, used for checking PPV_CreditNeedToDownloadedUsingCollection
                        if (channelsToCollectionsMappings.ContainsKey(channelID))
                        {
                            foreach (Collection collection in channelsToCollectionsMappings[channelID])
                            {
                                int collectionCode;
                                if (int.TryParse(collection.m_sObjectCode, out collectionCode))
                                {
                                    validatedColls.Add(collectionCode);
                                }
                            }
                        }
                    }
                    relevantValidSubscriptions = relevantValidSubscriptionsList.ToArray();
                }

                // now validate bulk collections - PPV_CreditNeedToDownloadedUsingCollection

                if (validatedColls != null && validatedColls.Count > 0)
                {
                    Dictionary<int, bool> collsAfterPPVCreditValidation = PPVBulkDoCreditNeedToDownloadedUsingCollections(nGroupID,
                        nMediaFileID, allUserIDsInDomain, validatedColls);
                    List<int> finalCollCodes = GetFinalCollectionCodes(collsAfterPPVCreditValidation);
                    if (finalCollCodes != null && finalCollCodes.Count > 0)
                    {
                        List<Collection> relevantValidCollectionsList = new List<Collection>();
                        //get collections data from dictionary instead of going to catalog
                        foreach (int collectionCode in finalCollCodes)
                        {
                            //get subscription from all subscriptions data dictionary
                            relevantValidCollectionsList.Add(collectionsData[collectionCode]);
                        }

                        relevantValidCollections = relevantValidCollectionsList.ToArray();
                    }
                }
            }
        }        

        private static void AddSubscriptionsToCheck(int fileTypeIdKey, ref List<UserBundlePurchase> subscriptionsToCheck,
                                                    Dictionary<int, List<Subscription>> fileTypeIdToSubscriptionMappings, Dictionary<string, UserBundlePurchase> subsPurchase)
        {
            if (fileTypeIdToSubscriptionMappings.ContainsKey(fileTypeIdKey))
            {
                foreach (var subscription in fileTypeIdToSubscriptionMappings[fileTypeIdKey])
                {
                    if (subsPurchase.ContainsKey(subscription.m_SubscriptionCode))
                    {
                        subscriptionsToCheck.Add(subsPurchase[subscription.m_SubscriptionCode]);
                    }
                }
            }
        }

        private static void AddUserBundlePurchasesToReleventList(List<UserBundlePurchase> userBundlePurchaseToCheck, MediaFileStatus mediaFileStatus,
                                                                 ref List<string> itemsToBundleCreditDownloadedQuery, ref List<int> itemsToGetFromSubsDictionary)
        {
            foreach (UserBundlePurchase bundle in userBundlePurchaseToCheck)
            {
                // add to bulk query of Bundle_DoesCreditNeedToDownloaded to DB
                //afterwards, the subs who pass the Bundle_DoesCreditNeedToDownloaded to DB test add to Catalog request.
                if (mediaFileStatus == MediaFileStatus.ValidOnlyIfPurchase || !IsUserCanStillUseEntitlement(bundle.nNumOfUses, bundle.nMaxNumOfUses))
                {
                    itemsToBundleCreditDownloadedQuery.Add(bundle.sBundleCode);
                }
                else
                {
                    // add to Catalog's BundlesContainingMediaRequest
                    if (Int32.TryParse(bundle.sBundleCode, out int subCode) && subCode > 0)
                    {
                        itemsToGetFromSubsDictionary.Add(subCode);
                    }
                }
            }
        }

        private static DomainBundles GetDomainBundlesFromDb(int groupId, int domainId)
        {
            DomainBundles result = null;
            try
            {
                List<int> usersInDomain = Utils.GetAllUsersInDomain(groupId, domainId);
                DataSet dataSet = ConditionalAccessDAL.Get_AllBundlesInfoByUserIDsOrDomainID(domainId, usersInDomain, groupId);
                if (dataSet != null && IsBundlesDataSetValid(dataSet))
                {
                    result = new DomainBundles();
                    // iterate over subscriptions
                    DataTable subs = dataSet.Tables[0];
                    int waiver = 0;
                    DateTime purchaseDate = DateTime.MinValue;
                    DateTime endDate = DateTime.MinValue;

                    if (subs != null && subs.Rows != null && subs.Rows.Count > 0)
                    {
                        DataRow[] subsRows = subs.Select().OrderBy(u => u["END_DATE"]).ToArray();

                        foreach (var subsRow in subsRows)
                        {
                            int numOfUses = 0;
                            int maxNumOfUses = 0;
                            string bundleCode = string.Empty;
                            int gracePeriodMinutes = 0;
                            bool isSuspend = false;
                            bool isPending = false;
                            GetSubscriptionBundlePurchaseData(subsRow, "SUBSCRIPTION_CODE", ref numOfUses, ref maxNumOfUses, ref bundleCode, ref waiver,
                                                                ref purchaseDate, ref endDate, ref gracePeriodMinutes, ref isSuspend, ref isPending);

                            // decide which is the correct end period
                            if (endDate < DateTime.UtcNow)
                                endDate = endDate.AddMinutes(gracePeriodMinutes);

                            int subCode = 0;
                            if (Int32.TryParse(bundleCode, out subCode) && subCode > 0)
                            {
                                if (!result.EntitledSubscriptions.ContainsKey(bundleCode))
                                {
                                    result.EntitledSubscriptions[bundleCode] = new List<UserBundlePurchaseWithSuspend>();
                                }

                                result.EntitledSubscriptions[bundleCode].Add(new UserBundlePurchaseWithSuspend()
                                {
                                    sBundleCode = bundleCode,
                                    nWaiver = waiver,
                                    dtPurchaseDate = purchaseDate,
                                    dtEndDate = endDate,
                                    nNumOfUses = numOfUses,
                                    nMaxNumOfUses = maxNumOfUses,
                                    isSuspend = isSuspend,
                                    isPending = isPending

                                });
                            }
                        }
                    }

                    //iterate over collections
                    DataTable colls = dataSet.Tables[1];
                    if (colls != null && colls.Rows != null && colls.Rows.Count > 0)
                    {
                        for (int i = 0; i < colls.Rows.Count; i++)
                        {
                            int numOfUses = 0;
                            int maxNumOfUses = 0;
                            string bundleCode = string.Empty;
                            waiver = 0;
                            purchaseDate = DateTime.MinValue;
                            endDate = DateTime.MinValue;
                            bool isPending = false;

                            GetCollectionBundlePurchaseData(colls.Rows[i], "COLLECTION_CODE", ref numOfUses, ref maxNumOfUses, ref bundleCode, ref waiver, ref purchaseDate, ref endDate, ref isPending);

                            int collCode = 0;
                            if (Int32.TryParse(bundleCode, out collCode) && collCode > 0)
                            {
                                if (!result.EntitledCollections.ContainsKey(bundleCode))
                                {
                                    result.EntitledCollections[bundleCode] = new List<UserBundlePurchase>();
                                }

                                result.EntitledCollections[bundleCode].Add(new UserBundlePurchase()
                                {
                                    sBundleCode = bundleCode,
                                    nWaiver = waiver,
                                    dtPurchaseDate = purchaseDate,
                                    dtEndDate = endDate,
                                    nNumOfUses = numOfUses,
                                    nMaxNumOfUses = maxNumOfUses
                                });
                            }
                            else
                            {
                                //log
                            }
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetAllDomainBundles for groupId: {0}, domainId: {1}", groupId, domainId), ex);
            }

            return result;
        }

        private static Tuple<DomainBundles, bool> InitializeDomainBundles(Dictionary<string, object> funcParams)
        {
            DomainBundles domainBundles = null;
            try
            {
                if (funcParams != null && funcParams.ContainsKey("groupId") && funcParams.ContainsKey("domainId"))
                {
                    int? groupId = funcParams["groupId"] as int?, domainId = funcParams["domainId"] as int?;
                    if (groupId.HasValue && domainId.HasValue)
                    {
                        domainBundles = GetDomainBundlesFromDb(groupId.Value, domainId.Value);
                    }
                }
            }

            catch (Exception ex)
            {
                log.Error(string.Format("InitializeDomainBundles failed, parameters : {0}", string.Join(";", funcParams.Keys)), ex);
            }

            bool res = domainBundles != null;

            return new Tuple<DomainBundles, bool>(domainBundles, res);
        }

        public static DomainBundles GetDomainBundles(int groupId, int domainId)
        {
            DomainBundles domainBundles = null;
            try
            {
                string key = LayeredCacheKeys.GetDomainBundlesKey(groupId, domainId);
                DomainBundles tempDomainBundles = null;
                Dictionary<string, object> funcParams = new Dictionary<string, object>() { { "groupId", groupId }, { "domainId", domainId } };
                if (LayeredCache.Instance.Get<DomainBundles>(key, ref tempDomainBundles, InitializeDomainBundles, funcParams, groupId, LayeredCacheConfigNames.GET_DOMAIN_BUNDLES_LAYERED_CACHE_CONFIG_NAME,
                                                            LayeredCacheKeys.GetDomainBundlesInvalidationKeys(groupId, domainId)) && tempDomainBundles != null)
                {
                    domainBundles = ObjectCopier.Clone<DomainBundles>(tempDomainBundles);
                    // remove expired subscriptions
                    if (tempDomainBundles.EntitledSubscriptions != null)
                    {
                        foreach (KeyValuePair<string, List<UserBundlePurchaseWithSuspend>> pair in tempDomainBundles.EntitledSubscriptions)
                        {
                            List<UserBundlePurchaseWithSuspend> validPurchases = new List<UserBundlePurchaseWithSuspend>();
                            if (pair.Value != null)
                            {
                                validPurchases = pair.Value.Where(x => x.dtEndDate > DateTime.UtcNow || x.isSuspend).ToList();
                                if (validPurchases != null && validPurchases.Count > 0)
                                {
                                    domainBundles.EntitledSubscriptions[pair.Key] = new List<UserBundlePurchaseWithSuspend>(validPurchases);
                                }
                                else
                                {
                                    domainBundles.EntitledSubscriptions.Remove(pair.Key);
                                }
                            }
                        }
                    }

                    // remove expired collections
                    if (tempDomainBundles.EntitledCollections != null)
                    {
                        foreach (KeyValuePair<string, List<UserBundlePurchase>> pair in tempDomainBundles.EntitledCollections)
                        {
                            List<UserBundlePurchase> validPurchases = new List<UserBundlePurchase>();
                            if (pair.Value != null)
                            {
                                validPurchases = pair.Value.Where(x => x.dtEndDate > DateTime.UtcNow).ToList();
                                if (validPurchases != null && validPurchases.Count > 0)
                                {
                                    domainBundles.EntitledCollections[pair.Key] = new List<UserBundlePurchase>(validPurchases);
                                }
                                else
                                {
                                    domainBundles.EntitledCollections.Remove(pair.Key);
                                }
                            }
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetDomainBundles, groupId: {0}, domainId: {1}", groupId, domainId), ex);
            }

            return domainBundles;
        }

        private static ChannelsContainingMediaRequest InitializeCatalogChannelsRequest(int nGroupID, int nMediaID, HashSet<int> channelsToCheck)
        {
            var request = new ChannelsContainingMediaRequest
            {
                m_nGroupID = nGroupID,
                m_nMediaID = nMediaID,
                m_oFilter = new Filter(),
                m_lChannles = new List<int>(channelsToCheck)
            };
            FillCatalogSignature(request);

            return request;
        }

        public static List<int> ValidateMediaContainedInChannels(int mediaID, int nGroupID, HashSet<int> channelsToCheck)
        {
            var request = InitializeCatalogChannelsRequest(nGroupID, mediaID, channelsToCheck);

            try
            {
                var response = request.GetResponse(request) as ChannelsContainingMediaResponse;
                if (response != null && response.m_lChannellList != null && response.m_lChannellList.Count > 0)
                {
                    // valid Channels
                    return response.m_lChannellList;
                }
            }
            catch (Exception ex)
            {
                log.Error("Failed ValidateMediaContainedInChannels Request To Catalog", ex);
            }

            return null;
        }

        internal static bool ValidateFileTypesConatainedInGroup(int m_nGroupID, int[] fileTypeIDs)
        {
            bool isContained = true;
            // isContained set to true incase we want to ignore filetypeIDs (sent as null or empty)
            if (fileTypeIDs != null && fileTypeIDs.Length > 0)
            {
                // Get all the group file types
                Dictionary<int, int> groupFileTypes = ConditionalAccessDAL.Get_GroupMediaTypesIDs(m_nGroupID);
                if (groupFileTypes != null && groupFileTypes.Count > 0)
                {
                    // Validate that all the fileTypeIDs in the request are contained in the groupFileTypes
                    if (groupFileTypes.Keys.Intersect(fileTypeIDs).Count() != fileTypeIDs.Length)
                    {
                        isContained = false;
                    }
                }
            }

            return isContained;
        }

        public static ApiObjects.Response.Status ValidateUserAndDomain(int groupId, string siteGuid, ref long householdId)
        {
            Domain domain;
            return ValidateUserAndDomain(groupId, siteGuid, ref householdId, out domain);
        }

        public static ApiObjects.Response.Status ValidateUserAndDomain(int groupId, string siteGuid, ref long householdId, out Domain domain)
        {
            User user;
            return ValidateUserAndDomain(groupId, siteGuid, ref householdId, out domain, out user);
        }

        internal static ApiObjects.Response.Status ValidateUserAndDomain(int groupId, string siteGuid, ref long householdId, out Domain domain, out Users.User user)
        {
            ApiObjects.Response.Status status = new ApiObjects.Response.Status();
            status.Code = -1;
            domain = null;
            user = null;

            // If no user - go immediately to domain validation
            if (string.IsNullOrEmpty(siteGuid) || siteGuid == "0")
            {
                status.Code = (int)eResponseStatus.OK;
            }
            else
            {
                // Get response from users WS
                ResponseStatus userStatus = ValidateUser(groupId, siteGuid, ref householdId, out user);
                if (householdId == 0)
                {
                    status.Code = (int)eResponseStatus.UserWithNoDomain;
                    status.Message = eResponseStatus.UserWithNoDomain.ToString();
                }
                else
                {
                    // Most of the cases are not interesting - focus only on those that matter
                    switch (userStatus)
                    {
                        case ResponseStatus.OK:
                            {
                                status.Code = (int)eResponseStatus.OK;
                                break;
                            }
                        case ResponseStatus.UserDoesNotExist:
                            {
                                status.Code = (int)eResponseStatus.UserDoesNotExist;
                                status.Message = eResponseStatus.UserDoesNotExist.ToString();
                                break;
                            }
                        case ResponseStatus.UserNotIndDomain:
                            {
                                status.Code = (int)eResponseStatus.UserNotInDomain;
                                status.Message = "User Not In Domain";
                                break;
                            }
                        case ResponseStatus.UserWithNoDomain:
                            {
                                status.Code = (int)eResponseStatus.UserWithNoDomain;
                                status.Message = eResponseStatus.UserWithNoDomain.ToString();
                                break;
                            }
                        case ResponseStatus.UserSuspended:
                        {
                            status = RolesPermissionsManager.Instance.AllowActionInSuspendedDomain(groupId, long.Parse(siteGuid))
                                ? ApiObjects.Response.Status.Ok
                                : new ApiObjects.Response.Status(eResponseStatus.UserSuspended);
                            break;
                        }
                        // Most cases will return general error
                        default:
                            {
                                status.Code = (int)eResponseStatus.Error;
                                status.Message = "Error validating user";
                                break;
                            }
                    }
                }
            }

            // If user is valid (or we don't have one)
            if (status.Code == (int)eResponseStatus.OK && householdId != 0)
            {
                //Get response from domains WS                
                status = ValidateDomain(groupId, (int)householdId, out domain);
            }
            return status;
        }

        internal static List<int> GetChannelsListFromSubscriptions(List<Subscription> subscriptions)
        {
            List<int> channelsList = new List<int>();
            if (subscriptions != null && subscriptions.Count > 0)
            {
                foreach (Subscription subscription in subscriptions)
                {
                    if (subscription.m_sCodes != null)
                    {
                        // Get channels from subscriptions
                        foreach (BundleCodeContainer bundleCode in subscription.m_sCodes)
                        {
                            int channelID;
                            if (int.TryParse(bundleCode.m_sCode, out channelID) && !channelsList.Contains(channelID))
                            {
                                channelsList.Add(channelID);
                            }
                        }
                    }
                }
            }

            return channelsList;
        }

        private static Tuple<bool, bool> GetIsTstvSettingsExists(Dictionary<string, object> funcParams)
        {
            bool isExists = false;
            bool result = true;

            try
            {
                if (funcParams != null && funcParams.ContainsKey("groupId"))
                {
                    int? groupId = funcParams["groupId"] as int?;
                    if (groupId.HasValue)
                    {
                        DataRow dr = DAL.ApiDAL.GetTimeShiftedTvPartnerSettings(groupId.Value, out result);

                        isExists = dr != null;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("GetIsTimeShiftedTvPartnerSettingsExists failed, parameters : {0}", string.Join(";", funcParams.Keys)), ex);
            }

            return new Tuple<bool, bool>(isExists, result);
        }

        internal static bool GetIsTimeShiftedTvPartnerSettingsExists(int groupId)
        {
            bool isTstvSettingsExists = false;
            try
            {
                string key = LayeredCacheKeys.GetIsTstvSettingsExistsKey(groupId);
                string invalidationKey = LayeredCacheKeys.GetTstvAccountSettingsInvalidationKey(groupId);
                if (!LayeredCache.Instance.Get<bool>(key, ref isTstvSettingsExists, GetIsTstvSettingsExists, new Dictionary<string, object>() { { "groupId", groupId } },
                                                                                groupId, LayeredCacheConfigNames.GET_TSTV_ACCOUNT_SETTINGS_CACHE_CONFIG_NAME, new List<string>() { invalidationKey }))
                {
                    log.ErrorFormat("Failed getting is tstv settings exists from LayeredCache, groupId: {0}", groupId);
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetIsTimeShiftedTvPartnerSettingsExists for groupId: {0}", groupId), ex);
            }

            return isTstvSettingsExists;
        }

        private static Tuple<TimeShiftedTvPartnerSettings, bool> GetTimeShiftedTvPartnerSettings(Dictionary<string, object> funcParams)
        {
            TimeShiftedTvPartnerSettings tstvAccountSettings = null;
            bool getTstsvSuccess = false;

            try
            {
                if (funcParams != null && funcParams.ContainsKey("groupId"))
                {
                    int? groupId = funcParams["groupId"] as int?;
                    if (groupId.HasValue)
                    {
                        DataRow dr = DAL.ApiDAL.GetTimeShiftedTvPartnerSettings(groupId.Value, out getTstsvSuccess);
                        
                        if (dr != null)
                        {
                            int catchup = ODBCWrapper.Utils.GetIntSafeVal(dr, "enable_catch_up", 0);
                            int cdvr = ODBCWrapper.Utils.GetIntSafeVal(dr, "enable_cdvr", 0);
                            int startOver = ODBCWrapper.Utils.GetIntSafeVal(dr, "enable_start_over", 0);
                            int trickPlay = ODBCWrapper.Utils.GetIntSafeVal(dr, "enable_trick_play", 0);
                            long catchUpBuffer = ODBCWrapper.Utils.GetLongSafeVal(dr, "catch_up_buffer", 7);
                            long trickPlayBuffer = ODBCWrapper.Utils.GetLongSafeVal(dr, "trick_play_buffer", 1);
                            long recordingScheduleWindowBuffer = ODBCWrapper.Utils.GetLongSafeVal(dr, "recording_schedule_window_buffer", 0);
                            int recordingScheduleWindow = ODBCWrapper.Utils.GetIntSafeVal(dr, "enable_recording_schedule_window", -1);
                            long paddingAfterProgramEnds = ODBCWrapper.Utils.GetLongSafeVal(dr, "padding_after_program_ends", 0);
                            long paddingBeforeProgramStarts = ODBCWrapper.Utils.GetLongSafeVal(dr, "padding_before_program_starts", 0);
                            int protection = ODBCWrapper.Utils.GetIntSafeVal(dr, "enable_protection", 0);
                            int protectionPeriod = ODBCWrapper.Utils.GetIntSafeVal(dr, "protection_period", 90);
                            int protectionQuotaPercentage = ODBCWrapper.Utils.GetIntSafeVal(dr, "protection_quota_percentage", 25);
                            int recordingLifetimePeriod = ODBCWrapper.Utils.GetIntSafeVal(dr, "recording_lifetime_period", 182);
                            int cleanupNoticePeriod = ODBCWrapper.Utils.GetIntSafeVal(dr, "cleanup_notice_period", 7);
                            int enableSeriesRecording = ODBCWrapper.Utils.GetIntSafeVal(dr, "enable_series_recording", 1); //Default = enabled
                            int recordingPlaybackNonEntitledChannel = ODBCWrapper.Utils.GetIntSafeVal(dr, "enable_recording_playback_non_entitled", 0); // Default = disabled
                            int recordingPlaybackNonExistingChannel = ODBCWrapper.Utils.GetIntSafeVal(dr, "enable_recording_playback_non_existing", 0); // Default = disabled
                            int quotaOveragePolicy = ODBCWrapper.Utils.GetIntSafeVal(dr, "quota_overage_policy", 0);
                            int protectionPolicy = ODBCWrapper.Utils.GetIntSafeVal(dr, "protection_policy", 0);
                            int recoveryGracePeriod = ODBCWrapper.Utils.GetIntSafeVal(dr, "recovery_grace_period_seconds", 0); // seconds
                            int privateCopy = ODBCWrapper.Utils.GetIntSafeVal(dr, "enable_private_copy", 0);
                            int quotaModuleId = ODBCWrapper.Utils.GetIntSafeVal(dr, "quota_module_id", 0);
                            int defaultQuota = ODBCWrapper.Utils.GetIntSafeVal(dr, "quota_in_seconds", 0);
                            int personalizedRecordingEnable = ODBCWrapper.Utils.GetIntSafeVal(dr, "personalized_recording_enable", 0);
                            int maxRecordingConcurrency = ODBCWrapper.Utils.GetIntSafeVal(dr, "max_recording_concurrency", 0);
                            int maxConcurrencyMargin = ODBCWrapper.Utils.GetIntSafeVal(dr, "max_concurrency_margin", 0);
                            
                            if (defaultQuota == 0 && quotaModuleId != 0)
                            {
                                int id = ODBCWrapper.Utils.GetIntSafeVal(dr, "id", 0);
                                defaultQuota =  ConditionalAccessDAL.GetDefaultQuotaByModuleIdInSeconds(groupId.Value, quotaModuleId);
                                if (defaultQuota != 0)
                                {
                                    ConditionalAccessDAL.SetDefaultQuotaInSeconds(id, defaultQuota);
                                }
                            }
                            if (recordingScheduleWindow > -1)
                            {
                                tstvAccountSettings = new TimeShiftedTvPartnerSettings(catchup == 1, cdvr == 1, startOver == 1, trickPlay == 1, recordingScheduleWindow == 1, catchUpBuffer,
                                            trickPlayBuffer, recordingScheduleWindowBuffer, paddingAfterProgramEnds, paddingBeforeProgramStarts,
                                            protection == 1, protectionPeriod, protectionQuotaPercentage, recordingLifetimePeriod, cleanupNoticePeriod, enableSeriesRecording == 1,
                                            recordingPlaybackNonEntitledChannel == 1, recordingPlaybackNonExistingChannel == 1, quotaOveragePolicy, protectionPolicy,
                                            recoveryGracePeriod, privateCopy == 1, defaultQuota, personalizedRecordingEnable == 1, maxRecordingConcurrency, maxConcurrencyMargin);
                            }
                        }
                        else
                        {
                            tstvAccountSettings = new TimeShiftedTvPartnerSettings(false, false, false, false, false, 7, 1, 0, 0, 0, false, 90, 25, 182, 7, true, false, false, 0, 0, 0, false, 0, false, 0, 0);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("GetTimeShiftedTvPartnerSettings failed, parameters : {0}", string.Join(";", funcParams.Keys)), ex);
            }

            return new Tuple<TimeShiftedTvPartnerSettings, bool>(tstvAccountSettings, getTstsvSuccess);
        }

        public static TimeShiftedTvPartnerSettings GetTimeShiftedTvPartnerSettings(int groupId)
        {
            TimeShiftedTvPartnerSettings tstvAccountSettings = null;
            try
            {
                string key = LayeredCacheKeys.GetTstvAccountSettingsKey(groupId);
                string invalidationKey = LayeredCacheKeys.GetTstvAccountSettingsInvalidationKey(groupId);
                if (!LayeredCache.Instance.Get<TimeShiftedTvPartnerSettings>(key, ref tstvAccountSettings, GetTimeShiftedTvPartnerSettings, new Dictionary<string, object>() { { "groupId", groupId } },
                                                                                groupId, LayeredCacheConfigNames.GET_TSTV_ACCOUNT_SETTINGS_CACHE_CONFIG_NAME, new List<string>() { invalidationKey }))
                {
                    log.ErrorFormat("Failed getting tstv account settings from LayeredCache, groupId: {0}", groupId);
                }
                else
                {
                    log.DebugFormat("current TSTV settings values are: {0}", tstvAccountSettings != null ? tstvAccountSettings.ToString() : "null");
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetTimeShiftedTvPartnerSettings for groupId: {0}", groupId), ex);
            }

            return tstvAccountSettings;
        }

        public static List<EPGChannelProgrammeObject> GetEpgsByIds(int nGroupID, List<long> epgIds)
        {
            List<EPGChannelProgrammeObject> epgs = null;

            try
            {
                EpgProgramDetailsRequest request = new EpgProgramDetailsRequest();
                request.m_nGroupID = nGroupID;
                //don't get the same EPG from catalog
                request.m_lProgramsIds = epgIds.ConvertAll<int>(x => (int)x).Distinct().ToList();
                request.m_oFilter = new Filter();
                FillCatalogSignature(request);

                EpgProgramResponse response = request.GetProgramsByIDs(request) as EpgProgramResponse;
                if (response != null && response.m_nTotalItems > 0 && response.m_lObj != null && response.m_lObj.Count > 0)
                {
                    epgs = new List<EPGChannelProgrammeObject>();
                    foreach (ProgramObj program in response.m_lObj)
                    {
                        // no need to check epg status since catalog returns only active epg's
                        if (program.AssetType == eAssetTypes.EPG && program.m_oProgram != null && program.m_oProgram.EPG_ID > 0)
                        {
                            epgs.Add(program.m_oProgram);
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                log.Error("Failed GetEpgsByIds Request To Catalog", ex);
            }

            return epgs;
        }

        internal static bool GetProgramFromRecordingCB(int groupId, long epgId, out EPGChannelProgrammeObject program, TvinciEpgBL epgBLTvinci = null)
        {
            bool response = false;
            program = null;

            try
            {
                if (epgBLTvinci == null)
                {
                    epgBLTvinci = new TvinciEpgBL(groupId);
                }

                List<string> epgIds = new List<string>() { epgId.ToString() };
                List<EpgCB> epgs = epgBLTvinci.GetEpgs(epgIds, true);

                if (epgs?.Count > 0)
                {
                    var programs = TvinciEpgBL.ConvertEpgCBtoEpgProgramm(epgs);
                    Catalog.CatalogLogic.GetLinearChannelSettings(groupId, programs);
                    program = programs[0];
                    response = true;
                }
                else
                {
                    log.Debug($"GetProgramFromRecordingCB - failed to get program {epgId}");
                }
            }

            catch (Exception ex)
            {
                log.Error("GetProgramFromRecordingCB", ex);
            }

            return response;
        }

        internal static bool IsValidRecordingStatus(TstvRecordingStatus recordingStatus, bool isOkStatusValid = false)
        {
            bool res = false;
            switch (recordingStatus)
            {
                case TstvRecordingStatus.OK:
                    res = isOkStatusValid;
                    break;

                case TstvRecordingStatus.Recording:
                case TstvRecordingStatus.Recorded:
                case TstvRecordingStatus.Scheduled:
                    res = true;
                    break;

                case TstvRecordingStatus.Deleted:
                case TstvRecordingStatus.Failed:
                case TstvRecordingStatus.Canceled:
                case TstvRecordingStatus.LifeTimePeriodExpired:
                default:
                    res = false;
                    break;
            }

            return res;
        }

        internal static List<int> GetGroupEnforcedServices(int groupID)
        {
            List<int> services;
            string key = string.Format("GroupEnforcedServices_{0}", groupID);
            if (!ConditionalAccessCache.GetItem<List<int>>(key, out services))
            {
                log.DebugFormat("Failed getting GroupEnforcedServices from cache, key: {0}", key);
                services = Tvinci.Core.DAL.CatalogDAL.GetGroupServices(groupID);
                if (services == null)
                {
                    log.ErrorFormat("Failed CatalogDAL.GetGroupServices for groupID: {0}", groupID);
                }
                else if (!ConditionalAccessCache.AddItem(key, services))
                {
                    log.ErrorFormat("Failed inserting GroupEnforcedServices to cache, key: {0}", key);
                }
            }

            return services;
        }

        internal static List<Recording> SearchDomainRecordingIDsByFilter(int groupID, string userID, long domainID, Dictionary<long, Recording> recordingIdToDomainRecording,
                                                                        string filter, int pageIndex, int pageSize, ApiObjects.SearchObjects.OrderObj orderBy, ref int totalResults)
        {
            List<Recording> recordings = null;

            try
            {
                UnifiedSearchRequest request = new UnifiedSearchRequest();
                request.m_nGroupID = groupID;
                request.m_dServerTime = DateTime.UtcNow;
                request.m_sSiteGuid = userID;
                request.domainId = (int)domainID;
                request.m_nPageIndex = pageIndex;
                request.m_nPageSize = pageSize;
                request.assetTypes = new List<int> { 1 };
                request.filterQuery = filter;
                request.order = orderBy;
                request.hasPredefinedRecordings = true;

                KeyValuePair<eAssetTypes, long>[] recordingAssets = new KeyValuePair<eAssetTypes, long>[recordingIdToDomainRecording.Count];
                for (int i = 0; i < recordingIdToDomainRecording.Count; i++)
                {
                    recordingAssets[i] = new KeyValuePair<eAssetTypes, long>(eAssetTypes.NPVR, recordingIdToDomainRecording.ElementAt(i).Key);
                }
                request.specificAssets = recordingAssets.ToList();
                request.m_oFilter = new Filter()
                {
                    m_bOnlyActiveMedia = true
                };
                FillCatalogSignature(request);

                UnifiedSearchResponse response = request.GetResponse(request) as UnifiedSearchResponse;
                if (response != null && response.status.Code == (int)eResponseStatus.OK && response.m_nTotalItems > 0 && response.searchResults != null)
                {
                    recordings = new List<Recording>();
                    totalResults = response.m_nTotalItems;
                    foreach (UnifiedSearchResult unifiedSearchResult in response.searchResults)
                    {
                        // no need to check epg status since catalog returns only active epg's
                        long searchRecordingID;
                        if (unifiedSearchResult.AssetType == eAssetTypes.NPVR && long.TryParse(unifiedSearchResult.AssetId, out searchRecordingID) && searchRecordingID > 0)
                        {
                            if (recordingIdToDomainRecording.ContainsKey(searchRecordingID))
                            {
                                Recording recording = recordingIdToDomainRecording[searchRecordingID].isExternalRecording ?
                                                      recordingIdToDomainRecording[searchRecordingID] as ExternalRecording : recordingIdToDomainRecording[searchRecordingID];
                                recordings.Add(recording);
                            }
                        }
                    }
                }

            }

            catch (Exception ex)
            {
                log.Error("Failed UnifiedSearchRequest Request To Catalog", ex);
            }

            return recordings;
        }

        internal static List<TstvRecordingStatus> ConvertToTstvRecordingStatus(List<DomainRecordingStatus> domainRecordingStatuses)
        {
            List<TstvRecordingStatus> result = new List<TstvRecordingStatus>();
            foreach (DomainRecordingStatus status in domainRecordingStatuses.Distinct())
            {
                switch (status)
                {
                    case DomainRecordingStatus.OK:
                        result.Add(TstvRecordingStatus.OK);
                        break;
                    case DomainRecordingStatus.Canceled:
                        result.Add(TstvRecordingStatus.Canceled);
                        break;
                    case DomainRecordingStatus.Deleted:
                        result.Add(TstvRecordingStatus.Deleted);
                        break;
                    case DomainRecordingStatus.Failed:
                        result.Add(TstvRecordingStatus.Failed);
                        break;
                    default:
                        break;
                }
            }
            return result;
        }

        internal static TstvRecordingStatus? ConvertToTstvRecordingStatus(RecordingInternalStatus recordingInternalStatus, DateTime epgStartDate, DateTime epgEndDate,
            DateTime? createDate = null)
        {
            TstvRecordingStatus? recordingStatus = null;
            switch (recordingInternalStatus)
            {
                case RecordingInternalStatus.Canceled:
                    recordingStatus = TstvRecordingStatus.Canceled;
                    break;
                case RecordingInternalStatus.Deleted:
                    recordingStatus = TstvRecordingStatus.Deleted;
                    break;
                case RecordingInternalStatus.Failed:
                    recordingStatus = TstvRecordingStatus.Failed;
                    break;
                case RecordingInternalStatus.Waiting:
                    /* Unlike RecordingInternalStatus.OK we don't check the epg end date because
                     * we won't return recorded since the adapter call is async, so if the program
                     * already started it doesn't matter if it finished or not we will return recording */
                    if (epgEndDate < DateTime.UtcNow && (!createDate.HasValue || createDate.Value.AddMinutes(2) < DateTime.UtcNow))
                    {
                        recordingStatus = TstvRecordingStatus.Failed;
                    }
                    else if (epgStartDate < DateTime.UtcNow)
                    {
                        recordingStatus = TstvRecordingStatus.Recording;
                    }
                    else
                    {
                        recordingStatus = TstvRecordingStatus.Scheduled;
                    }
                    break;
                case RecordingInternalStatus.OK:
                    // If program already finished, we say it is recorded
                    recordingStatus = SetRecordingStatus(epgStartDate, epgEndDate);
                    break;
                default:
                    break;
            }

            return recordingStatus;
        }

        internal static TstvRecordingStatus SetRecordingStatus(DateTime epgStartDate, DateTime epgEndDate)
        {
            if (epgEndDate < DateTime.UtcNow)
            {
                return TstvRecordingStatus.Recorded;
            }
            // If program already started but didn't finish, we say it is recording
            else if (epgStartDate < DateTime.UtcNow)
            {
                return TstvRecordingStatus.Recording;
            }
            else
            {
                return TstvRecordingStatus.Scheduled;
            }
        }

        internal static void SetRecordingStatus(Dictionary<long, Recording> dic, int groupId)
        {
            TimeShiftedTvPartnerSettings accountSettings = GetTimeShiftedTvPartnerSettings(groupId);
            int recordingLifetime = -1;
            foreach (var recording in dic.Values)
            {
                if (recording.RecordingStatus == TstvRecordingStatus.OK
                    || recording.RecordingStatus == TstvRecordingStatus.Recorded
                    || recording.RecordingStatus == TstvRecordingStatus.Recording
                    || recording.RecordingStatus == TstvRecordingStatus.Scheduled)
                {
                    DateTime startDate = recording.EpgStartDate;
                    DateTime endDate = recording.EpgEndDate;
                    if (accountSettings.PersonalizedRecordingEnable == true)
                    {
                        startDate = recording.AbsoluteStartTime ?? recording.EpgStartDate.AddMinutes(-1 * (recording.StartPadding ?? 0));
                        endDate =   recording.AbsoluteEndTime ?? recording.EpgEndDate.AddMinutes(recording.EndPadding ?? 0);
                    }
                    
                    recording.RecordingStatus = SetRecordingStatus(startDate, endDate);

                    if (recording.RecordingStatus == TstvRecordingStatus.Recorded && (!recording.ViewableUntilDate.HasValue || recording.ViewableUntilDate.Value == 0))
                    {
                        if (recordingLifetime == -1)
                        {
                            recordingLifetime = accountSettings.RecordingLifetimePeriod.HasValue ? accountSettings.RecordingLifetimePeriod.Value : 0;
                        }

                        if (recordingLifetime > 0)
                        {
                            recording.ViewableUntilDate = TVinciShared.DateUtils.DateTimeToUtcUnixTimestampSeconds(recording.EpgEndDate.AddDays(recordingLifetime));
                        }
                    }
                }
            }
        }

        internal static TstvRecordingStatus? ConvertToTstvRecordingStatus(DomainRecordingStatus domainRecordingStatus)
        {
            TstvRecordingStatus? recordingStatus = null;
            switch (domainRecordingStatus)
            {
                case DomainRecordingStatus.Canceled:
                    recordingStatus = TstvRecordingStatus.Canceled;
                    break;
                case DomainRecordingStatus.Deleted:
                    recordingStatus = TstvRecordingStatus.Deleted;
                    break;
                case DomainRecordingStatus.DeletedBySystem:
                    /***** Currently the LifeTimePeriodExpired status is only for backend inner needs and we are not exposing it on the REST to the client *****/
                    //recordingStatus = TstvRecordingStatus.LifeTimePeriodExpired;
                    recordingStatus = TstvRecordingStatus.Deleted;
                    break;
                case DomainRecordingStatus.OK:
                    recordingStatus = TstvRecordingStatus.OK;
                    break;
                case DomainRecordingStatus.SeriesDelete:
                    recordingStatus = TstvRecordingStatus.SeriesDelete;
                    break;
                case DomainRecordingStatus.SeriesCancel:
                    recordingStatus = TstvRecordingStatus.SeriesCancel;
                    break;
                case DomainRecordingStatus.DeletePending:
                    recordingStatus = TstvRecordingStatus.DeletePending;
                    break;
                case DomainRecordingStatus.Failed:
                    recordingStatus = TstvRecordingStatus.Failed;
                    break;
                default:
                    break;
            }

            return recordingStatus;
        }

        internal static TstvRecordingStatus? ConvertToSeriesStatus(TstvRecordingStatus status)
        {
            TstvRecordingStatus? recordingStatus = null;
            switch (status)
            {
                case TstvRecordingStatus.Canceled:
                    recordingStatus = TstvRecordingStatus.SeriesCancel;
                    break;
                case TstvRecordingStatus.Deleted:
                    recordingStatus = TstvRecordingStatus.SeriesDelete;
                    break;
                default:
                    break;
            }

            return recordingStatus;
        }

        internal static List<DomainRecordingStatus> ConvertToDomainRecordingStatus(List<TstvRecordingStatus> recordingStatus)
        {
            List<DomainRecordingStatus> result = new List<DomainRecordingStatus>();
            foreach (TstvRecordingStatus status in recordingStatus)
            {
                switch (status)
                {
                    case TstvRecordingStatus.Failed:
                    case TstvRecordingStatus.Scheduled:
                    case TstvRecordingStatus.Recording:
                    case TstvRecordingStatus.Recorded:
                        if (!result.Contains(DomainRecordingStatus.OK))
                        {
                            result.Add(DomainRecordingStatus.OK);
                        }
                        break;
                    case TstvRecordingStatus.Canceled:
                        if (!result.Contains(DomainRecordingStatus.Canceled))
                        {
                            result.Add(DomainRecordingStatus.Canceled);
                        }
                        break;
                    case TstvRecordingStatus.SeriesCancel:
                        if (!result.Contains(DomainRecordingStatus.SeriesCancel))
                        {
                            result.Add(DomainRecordingStatus.SeriesCancel);
                        }
                        break;
                    /***** Currently the LifeTimePeriodExpired status is only for backend inner needs and we are not exposing it on the REST to the client *****/
                    /*
                    // add both DomainRecordingStatus.OK and DomainRecordingStatus.DeletedByCleanup because we don't know if the recording has already been deleted
                    case TstvRecordingStatus.LifeTimePeriodExpired:
                        if (!result.Contains(DomainRecordingStatus.OK))
                        {
                            result.Add(DomainRecordingStatus.OK);
                        }

                        if (!result.Contains(DomainRecordingStatus.DeletedBySystem))
                        {
                            result.Add(DomainRecordingStatus.DeletedBySystem);
                        }
                        break;
                     */
                    case TstvRecordingStatus.Deleted:
                    case TstvRecordingStatus.SeriesDelete:
                    default:
                        break;
                }
            }

            return result;
        }

        public static DomainRecordingStatus? ConvertToDomainRecordingStatus(TstvRecordingStatus recordingStatus)
        {
            DomainRecordingStatus? result = null;
            switch (recordingStatus)
            {
                case TstvRecordingStatus.Failed:
                case TstvRecordingStatus.Scheduled:
                case TstvRecordingStatus.Recording:
                case TstvRecordingStatus.Recorded:
                    result = DomainRecordingStatus.OK;
                    break;
                case TstvRecordingStatus.Canceled:
                    result = DomainRecordingStatus.Canceled;
                    break;
                case TstvRecordingStatus.Deleted:
                    result = DomainRecordingStatus.Deleted;
                    break;
                case TstvRecordingStatus.SeriesCancel:
                    result = DomainRecordingStatus.SeriesCancel;
                    break;
                case TstvRecordingStatus.SeriesDelete:
                    result = DomainRecordingStatus.SeriesDelete;
                    break;
                default:
                    break;
            }

            return result;
        }

        internal static ApiObjects.Response.Status ValidateEpgForRecord(TimeShiftedTvPartnerSettings accountSettings, EPGChannelProgrammeObject epg, bool shouldCheckCatchUp)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            try
            {
                if (epg.ENABLE_CDVR != 1)
                {
                    response.Set((int)eResponseStatus.ProgramCdvrNotEnabled, eResponseStatus.ProgramCdvrNotEnabled.ToString());
                    return response;
                }

                if (shouldCheckCatchUp)
                {
                    DateTime epgStartDate;
                    if (!DateTime.TryParseExact(epg.START_DATE, EPG_DATETIME_FORMAT, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out epgStartDate))
                    {
                        log.ErrorFormat("Failed parsing EPG start date, epgID: {0}, startDate: {1}", epg.EPG_ID, epg.START_DATE);
                        response.Set((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                        return response;
                    }

                    // validate recording schedule window according to the paddedStartDate
                    DateTime paddedStartDate = epgStartDate.AddSeconds(accountSettings.PaddingBeforeProgramStarts.HasValue ? (-1) * accountSettings.PaddingBeforeProgramStarts.Value : 0);
                    if (accountSettings.IsRecordingScheduleWindowEnabled.HasValue && accountSettings.IsRecordingScheduleWindowEnabled.Value &&
                        accountSettings.RecordingScheduleWindow.HasValue && paddedStartDate.AddMinutes(accountSettings.RecordingScheduleWindow.Value) < DateTime.UtcNow)
                    {
                        response.Set((int)eResponseStatus.ProgramNotInRecordingScheduleWindow, eResponseStatus.ProgramNotInRecordingScheduleWindow.ToString());
                        return response;
                    }

                    return ValidateEpgForCatchUp(accountSettings, epg, epgStartDate);
                }
            }
            catch (Exception ex)
            {
                response.Set((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.Error(string.Format("Exception at ValidateEpgForRecord. epgID: {0}", epg.EPG_ID), ex);
            }

            return response;
        }

        internal static ApiObjects.Response.Status ValidateEpgForCatchUp(TimeShiftedTvPartnerSettings accountSettings, EPGChannelProgrammeObject epg, DateTime? epgStartDate = null)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());

            DateTime newEpgStartDate;
            if (epgStartDate.HasValue)
            {
                newEpgStartDate = epgStartDate.Value;
            }
            else if (!DateTime.TryParseExact(epg.START_DATE, EPG_DATETIME_FORMAT, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out newEpgStartDate))
            {
                log.ErrorFormat("Failed parsing EPG start date, epgID: {0}, startDate: {1}", epg.EPG_ID, epg.START_DATE);
                response.Set((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                return response;
            }

            if (newEpgStartDate < DateTime.UtcNow)
            {
                if (!accountSettings.IsCatchUpEnabled.HasValue || !accountSettings.IsCatchUpEnabled.Value)
                {
                    response.Set((int)eResponseStatus.AccountCatchUpNotEnabled, eResponseStatus.AccountCatchUpNotEnabled.ToString());
                    return response;
                }
                if (epg.ENABLE_CATCH_UP != 1)
                {
                    response.Set((int)eResponseStatus.ProgramCatchUpNotEnabled, eResponseStatus.ProgramCatchUpNotEnabled.ToString());
                    return response;
                }
                if (epg.CHANNEL_CATCH_UP_BUFFER == 0 || newEpgStartDate.AddMinutes(epg.CHANNEL_CATCH_UP_BUFFER) < DateTime.UtcNow)
                {
                    response.Set((int)eResponseStatus.CatchUpBufferLimitation, eResponseStatus.CatchUpBufferLimitation.ToString());
                    return response;
                }
            }

            return response;
        }

        internal static ApiObjects.Response.Status ValidateEpgForStartOver(EPGChannelProgrammeObject epg)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            if (epg.ENABLE_START_OVER != 1)
            {
                response.Set((int)eResponseStatus.ProgramStartOverNotEnabled, eResponseStatus.ProgramStartOverNotEnabled.ToString());
            }

            return response;
        }

        internal static Recording CheckDomainExistingRecordingsByEpgs(int groupId, long domainID, EPGChannelProgrammeObject epg, 
            int? paddingBefore = null, int? paddingAfter = null, long? absoluteStart = null, long? absoluteEnd = null)
        {
            Recording recording = new Recording() { EpgId = epg.EPG_ID };
            try
            {
                var accountSettings = GetTimeShiftedTvPartnerSettings(groupId);

                if (accountSettings == null)
                {
                    log.ErrorFormat("Failed getting account padding, epgId: {0}, groupID: {1}", groupId, recording.EpgId);
                    recording.Status.Set((int)eResponseStatus.Error, "Failed getting account padding settings");
                    return recording;
                }

                if (epg.ParseDate(epg.START_DATE, out DateTime epgStartDate) 
                    && epg.ParseDate(epg.END_DATE, out DateTime epgEndDate) 
                    && long.TryParse(epg.EPG_CHANNEL_ID, out long epgChannelId))
                {
                    recording = new Recording()
                    {
                        Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString()),
                        EpgId = epg.EPG_ID,
                        ChannelId = epgChannelId,
                        Id = 0,
                        EpgStartDate = epgStartDate,
                        EpgEndDate = epgEndDate,
                        Crid = epg.CRID
                    };

                    if (accountSettings.PersonalizedRecordingEnable == true)
                    {
                        if(absoluteStart.HasValue)
                        {
                            recording.AbsoluteStartTime = DateUtils.UtcUnixTimestampSecondsToDateTime(absoluteStart.Value);
                            recording.AbsoluteEndTime = DateUtils.UtcUnixTimestampSecondsToDateTime(absoluteEnd.Value);
                        }
                        else
                        {
                            recording.StartPadding = paddingBefore ?? (accountSettings.PaddingBeforeProgramStarts.HasValue ? (int)accountSettings.PaddingBeforeProgramStarts : 0);
                            recording.EndPadding = paddingAfter ?? (accountSettings.PaddingAfterProgramEnds.HasValue ? (int)accountSettings.PaddingAfterProgramEnds : 0);   
                        }
                    }
                    else if (accountSettings.PaddingBeforeProgramStarts.HasValue && accountSettings.PaddingAfterProgramEnds.HasValue)
                    {
                        recording.EpgStartDate = recording.EpgStartDate.AddSeconds((-1) * accountSettings.PaddingBeforeProgramStarts.Value);
                        recording.EpgEndDate = recording.EpgEndDate.AddSeconds(accountSettings.PaddingAfterProgramEnds.Value);
                    }
                }
                else
                {
                    log.ErrorFormat("Failed parsing EPG start / end date / epgChannelId, epgID: {0}, domainID: {1}, startDate: {2}, endDate: {3}, epgChannelId: {4}", epg.EPG_ID, domainID, epg.START_DATE, epg.END_DATE, epg.EPG_CHANNEL_ID);
                }

                if (accountSettings.PersonalizedRecordingEnable == true)
                {
                    string recordingKey;
                    if (recording.AbsoluteStartTime.HasValue)
                    {
                        recordingKey =
                            PaddedRecordingsManager.GetImmediateRecordingKey(epg.EPG_ID, absoluteStart,
                                absoluteEnd);
                    }
                    else
                    {
                        recordingKey = PaddedRecordingsManager.GetRecordingKey(epg.EPG_ID, recording.StartPadding ?? 0, recording.EndPadding ?? 0);
                    }
                    
                    Recording householdRecording = PaddedRecordingsManager.Instance.GetHouseholdRecording(groupId, domainID, epg.EPG_ID, recordingKey);
                    
                    if (householdRecording != null && householdRecording.Status != null && householdRecording.Status.Code == (int)eResponseStatus.OK
                         && householdRecording.RecordingStatus != TstvRecordingStatus.Canceled && householdRecording.RecordingStatus != TstvRecordingStatus.Deleted)
                    {
                        recording = new Recording(householdRecording);
                    }
                }
                else
                {
                    Dictionary<long, Recording> domainIdToDomainRecordingMap = Utils.GetDomainRecordingIdToRecordingMapByEpgIds(groupId, domainID, new List<long>() { epg.EPG_ID });
                    if (domainIdToDomainRecordingMap != null)
                    {
                        foreach (KeyValuePair<long, Recording> pair in domainIdToDomainRecordingMap)
                        {
                            Recording domainRecoridng = pair.Value;
                            // add domain recording if it doesn't already exist in dictionary and wasn't canceled or deleted
                            if (domainRecoridng != null && domainRecoridng.Status != null && domainRecoridng.Status.Code == (int)eResponseStatus.OK
                                && domainRecoridng.RecordingStatus != TstvRecordingStatus.Canceled && domainRecoridng.RecordingStatus != TstvRecordingStatus.Deleted)
                            {
                                domainRecoridng.Id = pair.Key;
                                recording = new Recording(domainRecoridng);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error($"Exception at CheckDomainExistingRecording. domainID {domainID}", ex);
            }

            return recording;
        }

        internal static int MapActionTypeForAdapter(eEPGFormatType eformat)
        {
            int response = -1;

            switch (eformat)
            {
                case eEPGFormatType.Catchup:
                    response = (int)CdnAdapterActionType.Catchup;
                    break;
                case eEPGFormatType.StartOver:
                    response = (int)CdnAdapterActionType.StartOver;
                    break;
                case eEPGFormatType.LivePause:
                    response = (int)CdnAdapterActionType.LivePause;
                    break;
                case eEPGFormatType.NPVR:
                    break;
                default:
                    break;
            }

            return response;
        }

        internal static CDNAdapterResponse GetRelevantCDN(int groupId, int fileStreamingCompanyId, ApiObjects.eAssetTypes assetType, ref bool isDefaultAdapter)
        {
            CDNAdapterResponse adapterResponse = new CDNAdapterResponse();

            try
            {
                // if nStreamingCompany is 0 - call api service for getting the default adapter / streaming company
                if (fileStreamingCompanyId == 0)
                {
                    isDefaultAdapter = true;
                    adapterResponse = Api.Module.GetGroupDefaultCDNAdapter(groupId, assetType);
                }
                // else - call api service for getting the adapter / streaming company with the nStreamingCompany ID                
                else
                {
                    adapterResponse = Api.Module.GetCDNAdapter(groupId, fileStreamingCompanyId);
                }
            }
            catch (Exception ex)
            {
                log.Error($"GetRelevantCDN. groupId={groupId}, fileStreamingCompanyId={fileStreamingCompanyId}, assetType={assetType}.", ex);
                adapterResponse.Status = ApiObjects.Response.Status.Error;
                return adapterResponse;
            }

            if (adapterResponse.Status == null)
            {
                log.Error($"GetRelevantCDN: failed to get adapter. groupId={groupId}, fileStreamingCompanyId={fileStreamingCompanyId}, assetType={assetType}.");
                adapterResponse.Status = ApiObjects.Response.Status.Error;
                return adapterResponse;
            }

            if (!adapterResponse.Status.IsOkStatusCode())
            {
                log.Debug($"GetRelevantCDN: failed to get adater. groupId={groupId}, fileStreamingCompanyId={fileStreamingCompanyId}, assetType={assetType}, status.Code={adapterResponse.Status.Code}, status.Message={adapterResponse.Status.Message}.");
            }

            return adapterResponse;
        }

        internal static bool IsValidRecordingStatus(TstvRecordingStatus recordStatus, List<TstvRecordingStatus> RecordingStatus)
        {
            if (RecordingStatus.Contains(recordStatus))
            {
                return true;
            }
            return false;
        }

        internal static Dictionary<long, Recording> GetDomainProtectedRecordings(int groupID, long domainID)
        {
            Dictionary<long, Recording> domainProtectedRecordings = null;
            DataTable dt = RecordingsDAL.GetDomainProtectedRecordings(groupID, domainID, TVinciShared.DateUtils.GetUtcUnixTimestampNow());
            if (dt != null && dt.Rows != null)
            {
                domainProtectedRecordings = new Dictionary<long, Recording>();
                foreach (DataRow dr in dt.Rows)
                {
                    long recordingID = ODBCWrapper.Utils.GetLongSafeVal(dr, "ID");
                    if (recordingID > 0)
                    {
                        Recording protectedRecording = new Recording()
                        {
                            Id = recordingID,
                            EpgId = ODBCWrapper.Utils.GetLongSafeVal(dr, "EPG_ID"),
                            EpgStartDate = ODBCWrapper.Utils.GetDateSafeVal(dr, "START_DATE"),
                            EpgEndDate = ODBCWrapper.Utils.GetDateSafeVal(dr, "END_DATE"),
                            Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString())
                        };
                        if (!domainProtectedRecordings.ContainsKey(recordingID))
                        {
                            domainProtectedRecordings.Add(recordingID, protectedRecording);
                        }
                    }
                }
            }

            return domainProtectedRecordings;
        }

        internal static Dictionary<long, Recording> GetDomainRecordingIdsToRecordingsMap(int groupID, long domainID, List<long> domainRecordingIds, bool shouldFilterViewableRecordingsOnly = true)
        {
            var ret = GetDomainRecordings(groupID, domainID, shouldFilterViewableRecordingsOnly);

            return ret == null
                ? ret
                : ret.Where(x => domainRecordingIds.Contains(x.Key))
                    .ToDictionary(x => x.Key, x => x.Value);
        }

        public static Dictionary<long, Recording> GetDomainRecordings(int groupId, long domainId, bool shouldFilterViewableRecordingsOnly = true)
        {
            Dictionary<long, Recording> domainRecordingIdToRecordingMap = null;
            Dictionary<long, Recording> response = new Dictionary<long, Recording>();

            try
            {
                Dictionary<string, object> funcParams = new Dictionary<string, object>() { { "groupId", groupId }, { "domainId", domainId } };

                if (LayeredCache.Instance.Get(LayeredCacheKeys.GetDomainRecordingsKey(domainId), ref domainRecordingIdToRecordingMap, GetDomainRecordings, funcParams, groupId,
                                        LayeredCacheConfigNames.GET_DOMAIN_RECORDINGS_LAYERED_CACHE_CONFIG_NAME, new List<string> { LayeredCacheKeys.GetDomainRecordingsInvalidationKeys(groupId, domainId) }, true)
                    && domainRecordingIdToRecordingMap != null && domainRecordingIdToRecordingMap.Count > 0)
                {
                    SetRecordingStatus(domainRecordingIdToRecordingMap, groupId);
                    Dictionary<long, Recording> recordingsToCopy = new Dictionary<long, Recording>();
                    if (shouldFilterViewableRecordingsOnly)
                    {
                        long epoc = DateTime.UtcNow.ToUtcUnixTimestampSeconds();
                        if (domainRecordingIdToRecordingMap.Any(x => !x.Value.ViewableUntilDate.HasValue || (x.Value.ViewableUntilDate.HasValue && x.Value.ViewableUntilDate.Value > epoc)))
                        {
                            recordingsToCopy = domainRecordingIdToRecordingMap.Where(x => !x.Value.ViewableUntilDate.HasValue
                                                || (x.Value.ViewableUntilDate.HasValue && x.Value.ViewableUntilDate.Value > epoc)).ToDictionary(x => x.Key, x => x.Value);
                        }
                    }
                    else
                    {
                        recordingsToCopy = domainRecordingIdToRecordingMap;
                    }

                    if (recordingsToCopy != null)
                    {
                        foreach (KeyValuePair<long, Recording> recToCopy in recordingsToCopy)
                        {
                            response[recToCopy.Key] = recToCopy.Value.isExternalRecording ? new ExternalRecording(recToCopy.Value as ExternalRecording) : new Recording(recToCopy.Value);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("failed GetDomainRecordings, groupId: {0}, domainId: {1}", groupId, domainId), ex);
            }

            return response;
        }

        internal static Dictionary<string, ExternalRecording> GetDomainExternalRecordings(int groupId, long domainId, bool shouldFilterViewableRecordingsOnly = true)
        {
            Dictionary<long, Recording> domainRecordingIdToRecordingMap = null;
            Dictionary<string, ExternalRecording> domainExternalRecordingIdToRecordingMap = new Dictionary<string, ExternalRecording>();

            try
            {
                Dictionary<string, object> funcParams = new Dictionary<string, object>() { { "groupId", groupId }, { "domainId", domainId } };

                if (LayeredCache.Instance.Get(LayeredCacheKeys.GetDomainRecordingsKey(domainId), ref domainRecordingIdToRecordingMap, GetDomainRecordingsFromDB, funcParams, groupId,
                                        LayeredCacheConfigNames.GET_DOMAIN_RECORDINGS_LAYERED_CACHE_CONFIG_NAME, new List<string> { LayeredCacheKeys.GetDomainRecordingsInvalidationKeys(groupId, domainId) }, true)
                    && domainRecordingIdToRecordingMap != null && domainRecordingIdToRecordingMap.Count > 0)
                {
                    SetRecordingStatus(domainRecordingIdToRecordingMap, groupId);

                    Dictionary<long, Recording> recordingsToCopy = new Dictionary<long, Recording>();
                    if (shouldFilterViewableRecordingsOnly)
                    {
                        long epoc = DateTime.UtcNow.ToUtcUnixTimestampSeconds();
                        if (domainRecordingIdToRecordingMap.Any(x => !x.Value.ViewableUntilDate.HasValue || (x.Value.ViewableUntilDate.HasValue && x.Value.ViewableUntilDate.Value > epoc)))
                        {
                            recordingsToCopy = domainRecordingIdToRecordingMap.Where(x => !x.Value.ViewableUntilDate.HasValue
                                                || (x.Value.ViewableUntilDate.HasValue && x.Value.ViewableUntilDate.Value > epoc)).ToDictionary(x => x.Key, x => x.Value);
                        }
                    }
                    else
                    {
                        recordingsToCopy = domainRecordingIdToRecordingMap;
                    }

                    if (recordingsToCopy != null)
                    {
                        foreach (KeyValuePair<long, Recording> recToCopy in recordingsToCopy)
                        {
                            if (recToCopy.Value.isExternalRecording)
                            {
                                ExternalRecording externalRecording = new ExternalRecording(recToCopy.Value as ExternalRecording);
                                domainExternalRecordingIdToRecordingMap[externalRecording.ExternalDomainRecordingId] = externalRecording;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("failed GetDomainExternalRecordings, groupId: {0}, domainId: {1}", groupId, domainId), ex);
            }

            return domainExternalRecordingIdToRecordingMap;
        }

        private static Tuple<Dictionary<long, Recording>, bool> GetDomainRecordings(
            Dictionary<string, object> arg)
        {
            Dictionary<long, Recording> domainRecordingIdToRecordingMap = null;
            int groupID = int.Parse(arg["groupId"].ToString());
            long domainID = long.Parse(arg["domainId"].ToString());
            var domainRecordingStatuses = new List<DomainRecordingStatus>()
            {
                DomainRecordingStatus.OK,
                DomainRecordingStatus.Canceled,
                DomainRecordingStatus.SeriesCancel,
                DomainRecordingStatus.Failed
            };
            TimeShiftedTvPartnerSettings accountSettings = GetTimeShiftedTvPartnerSettings(groupID);
            if (accountSettings.PersonalizedRecordingEnable == true)
            {
                domainRecordingIdToRecordingMap = PaddedRecordingsManager.Instance.GetHouseholdRecordingsByRecordingStatuses(groupID, domainID,
                    domainRecordingStatuses);
            }
            else
            {
                domainRecordingIdToRecordingMap = GetDomainRecordingsFromSql(groupID, domainID,
                    domainRecordingStatuses);
            }
            
            return new Tuple<Dictionary<long, Recording>, bool>(domainRecordingIdToRecordingMap, true);
        }
        
        private static Dictionary<long, Recording> GetDomainRecordingsFromSql(int groupId, long domainId, List<DomainRecordingStatus> domainRecordingStatuses)
        {
            Dictionary<long, Recording> DomainRecordingIdToRecordingMap = null;
            
            DataTable dt = RecordingsDAL.GetDomainRecordingsByRecordingStatuses(groupId, domainId, domainRecordingStatuses.Select(x => (int)x).ToList());
            if (dt != null && dt.Rows != null)
            {
                DomainRecordingIdToRecordingMap = new Dictionary<long, Recording>();

                foreach (DataRow dr in dt.Rows)
                {
                    long domainRecordingID = ODBCWrapper.Utils.GetLongSafeVal(dr, "ID");
                    if (domainRecordingID > 0)
                    {
                        Recording domainRecording = BuildDomainRecordingFromDataRow(dr);
                        // add domain recording if its valid and doesn't already exist in dictionary
                        if (domainRecording != null && domainRecording.Status != null && domainRecording.Status.Code == (int)eResponseStatus.OK
                            && !DomainRecordingIdToRecordingMap.ContainsKey(domainRecordingID))
                        {
                            DomainRecordingIdToRecordingMap.Add(domainRecordingID, domainRecording);
                        }
                    }
                }
            }

            return DomainRecordingIdToRecordingMap;
        }
        
        private static Tuple<Dictionary<long, Recording>, bool> GetDomainRecordingsFromDB(Dictionary<string, object> arg)
        {
            Dictionary<long, Recording> DomainRecordingIdToRecordingMap = null;

            int groupID = int.Parse(arg["groupId"].ToString());
            long domainID = long.Parse(arg["domainId"].ToString());

            var domainRecordingStatuses = new List<DomainRecordingStatus>()
            {
                DomainRecordingStatus.OK,
                DomainRecordingStatus.Canceled,
                DomainRecordingStatus.SeriesCancel,
                DomainRecordingStatus.Failed
            };
            DataTable dt = RecordingsDAL.GetDomainRecordingsByRecordingStatuses(groupID, domainID, domainRecordingStatuses.Select(x => (int)x).ToList());
            if (dt != null && dt.Rows != null)
            {
                DomainRecordingIdToRecordingMap = new Dictionary<long, Recording>();

                foreach (DataRow dr in dt.Rows)
                {
                    long domainRecordingID = ODBCWrapper.Utils.GetLongSafeVal(dr, "ID");
                    if (domainRecordingID > 0)
                    {
                        Recording domainRecording = BuildDomainRecordingFromDataRow(dr);
                        // add domain recording if its valid and doesn't already exist in dictionary
                        if (domainRecording != null && domainRecording.Status != null && domainRecording.Status.Code == (int)eResponseStatus.OK
                            && !DomainRecordingIdToRecordingMap.ContainsKey(domainRecordingID))
                        {
                            DomainRecordingIdToRecordingMap.Add(domainRecordingID, domainRecording);
                        }
                    }
                }
            }

            return new Tuple<Dictionary<long, Recording>, bool>(DomainRecordingIdToRecordingMap, dt != null);
        }

        internal static Dictionary<long, Recording> GetDomainRecordingsByTstvRecordingStatuses(int groupID, long domainID, List<ApiObjects.TstvRecordingStatus> recordingStatuses,
                                                                                                bool shouldFilterViewableRecordingOnly = true)
        {
            Dictionary<long, Recording> DomainRecordingIdToRecordingMap = new Dictionary<long, Recording>();

            var domainRecordings = GetDomainRecordings(groupID, domainID, shouldFilterViewableRecordingOnly);
            foreach (var record in domainRecordings)
            {
                if (recordingStatuses.Contains(record.Value.RecordingStatus))
                {
                    DomainRecordingIdToRecordingMap.Add(record.Key, record.Value);
                }
            }

            return DomainRecordingIdToRecordingMap;
        }

        internal static Recording ValidateRecordID(int groupID, long domainID, long domainRecordingID, bool shouldFilterViewableRecordingsOnly = true, Dictionary<long, Recording> domainRecordingIdToRecordingMap = null)
        {
            Recording recording = new Recording()
            {
                Status = new ApiObjects.Response.Status((int)eResponseStatus.RecordingNotFound, eResponseStatus.RecordingNotFound.ToString())
            };

            try
            {
                domainRecordingIdToRecordingMap = domainRecordingIdToRecordingMap ?? Utils.GetDomainRecordingIdsToRecordingsMap(groupID, domainID, new List<long>() { domainRecordingID }, shouldFilterViewableRecordingsOnly);
                if (domainRecordingIdToRecordingMap == null || domainRecordingIdToRecordingMap.Count == 0 ||
                    !domainRecordingIdToRecordingMap.ContainsKey(domainRecordingID) || domainRecordingIdToRecordingMap[domainRecordingID].RecordingStatus == TstvRecordingStatus.Deleted)
                {
                    log.DebugFormat("No valid recording was returned from Utils.GetDomainRecordingIdsToRecordingsMap");
                    recording.Status = new ApiObjects.Response.Status((int)eResponseStatus.RecordingNotFound, eResponseStatus.RecordingNotFound.ToString());
                    recording.Id = domainRecordingID;
                    return recording;
                }

                recording = domainRecordingIdToRecordingMap[domainRecordingID];
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder("Exception at ValidateRecordID. ");
                sb.Append(String.Concat("domainID: ", domainID));
                sb.Append(String.Concat(", domainRecordingID: ", domainRecordingID));
                sb.Append(String.Concat("Ex Msg: ", ex.Message));
                sb.Append(String.Concat(", Ex Type: ", ex.GetType().Name));
                sb.Append(String.Concat(", Stack Trace: ", ex.StackTrace));

                log.Error(sb.ToString(), ex);

                recording.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "failed ValidateRecordID");
            }

            return recording;
        }


        internal static SeriesRecording ValidateSeriesRecordID(int groupId, long domainId, long domainSeriesRecordingId)
        {
            SeriesRecording seriesRecording = new SeriesRecording()
            {
                Status = new ApiObjects.Response.Status((int)eResponseStatus.SeriesRecordingNotFound, eResponseStatus.SeriesRecordingNotFound.ToString())
            };

            try
            {
                DataSet ds = RecordingsDAL.GetDomainSeriesRecordingsById(groupId, domainId, domainSeriesRecordingId);
                var seriesRecordings = BuildSeriesRecordingDetails(ds);
                if (seriesRecordings != null && seriesRecordings.Count > 0)
                {
                    seriesRecording = seriesRecordings[0];
                }
                if (seriesRecording == null)
                {
                    log.DebugFormat("No valid series recording was returned from Utils.GetDomainSeriesRecording");
                    seriesRecording = new SeriesRecording()
                    {
                        Status = new ApiObjects.Response.Status((int)eResponseStatus.SeriesRecordingNotFound, eResponseStatus.SeriesRecordingNotFound.ToString()),
                        Id = domainSeriesRecordingId
                    };
                }
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder("Exception at ValidateSeriesRecordID. ");
                sb.Append(String.Concat("domainID: ", domainId));
                sb.Append(String.Concat(", domainSeriesRecordingId: ", domainSeriesRecordingId));
                sb.Append(String.Concat("Ex Msg: ", ex.Message));
                sb.Append(String.Concat(", Ex Type: ", ex.GetType().Name));
                sb.Append(String.Concat(", Stack Trace: ", ex.StackTrace));

                log.Error(sb.ToString(), ex);
                seriesRecording = new SeriesRecording()
                {
                    Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "failed Validate series recordID"),
                    Id = domainSeriesRecordingId
                };
            }
            return seriesRecording;
        }

        public static List<SeriesRecording> BuildSeriesRecordingDetails(DataSet ds)
        {
            Dictionary<long, SeriesRecording> result = new Dictionary<long, SeriesRecording>();
            SeriesRecording seriesRecording = null;

            if (ds != null && ds.Tables != null && ds.Tables.Count >= 2)
            {
                if (ds.Tables[0] != null && ds.Tables[0].Rows != null && ds.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        int? minSeasonNumber = ODBCWrapper.Utils.GetIntSafeVal(dr, "MIN_SEASON_NUMBER");
                        int? minEpisodeNumber = ODBCWrapper.Utils.GetIntSafeVal(dr, "MIN_EPISODE_NUMBER");
                        long? startDateRecording = ODBCWrapper.Utils.GetLongSafeVal(dr, "START_DATE_RECORDING");
                        var chronological_Record_StartTime = ODBCWrapper.Utils.GetIntSafeVal(dr, "CHRONOLOGICAL_RECORD_STARTTIME");

                        seriesRecording = new SeriesRecording()
                        {
                            EpgId = ODBCWrapper.Utils.GetLongSafeVal(dr, "EPG_ID"),
                            EpgChannelId = ODBCWrapper.Utils.GetLongSafeVal(dr, "EPG_CHANNEL_ID"),
                            Id = ODBCWrapper.Utils.GetLongSafeVal(dr, "ID"),
                            SeasonNumber = ODBCWrapper.Utils.GetIntSafeVal(dr, "SEASON_NUMBER"),
                            EpisodeNumber = ODBCWrapper.Utils.GetIntSafeVal(dr, "EPISODE_NUMBER"),
                            SeriesId = ODBCWrapper.Utils.GetSafeStr(dr, "SERIES_ID"),
                            CreateDate = ODBCWrapper.Utils.GetDateSafeVal(dr, "CREATE_DATE"),
                            UpdateDate = ODBCWrapper.Utils.GetDateSafeVal(dr, "UPDATE_DATE"),
                            ExcludedSeasons = new List<int>(),
                            Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString()),
                            Type = (RecordingType)ODBCWrapper.Utils.GetIntSafeVal(dr, "RECORD_TYPE", 0),
                            SeriesRecordingOption = new SeriesRecordingOption
                            {
                                MinSeasonNumber = minSeasonNumber > 0 ? minSeasonNumber : null,
                                MinEpisodeNumber = minEpisodeNumber > 0 ? minEpisodeNumber : null,
                                StartDateRecording = startDateRecording > 0 ? startDateRecording : null,
                                ChronologicalRecordStartTime = (ChronologicalRecordStartTime)chronological_Record_StartTime
                            }
                        };

                        result.Add(seriesRecording.Id, seriesRecording);
                    }
                }
                if (ds.Tables[1] != null && ds.Tables[1].Rows != null && ds.Tables[1].Rows.Count > 0)
                {
                    long seriesRecordingId = 0;
                    foreach (DataRow dr in ds.Tables[1].Rows)
                    {
                        seriesRecordingId = ODBCWrapper.Utils.GetIntSafeVal(dr, "DOMAIN_SERIES_ID");
                        if (result.ContainsKey(seriesRecordingId))
                        {
                            result[seriesRecordingId].ExcludedSeasons.Add(ODBCWrapper.Utils.GetIntSafeVal(dr, "SEASON_NUMBER"));
                        }
                    }
                }
            }

            return result.Values.ToList();
        }

        internal static Recording BuildDomainRecordingFromDataRow(DataRow dr)
        {
            Recording recording = new Recording();
            long recordingID = ODBCWrapper.Utils.GetLongSafeVal(dr, "RECORDING_ID");
            if (recordingID > 0)
            {
                long epgId = ODBCWrapper.Utils.GetLongSafeVal(dr, "EPG_ID");
                string userId = ODBCWrapper.Utils.GetSafeStr(dr, "USER_ID");
                long epgChannelId = ODBCWrapper.Utils.GetLongSafeVal(dr, "EPG_CHANNEL_ID");
                DateTime createDate = ODBCWrapper.Utils.GetDateSafeVal(dr, "CREATE_DATE");
                DateTime updateDate = ODBCWrapper.Utils.GetDateSafeVal(dr, "UPDATE_DATE");
                long viewableUntilEpoch = ODBCWrapper.Utils.GetLongSafeVal(dr, "VIEWABLE_UNTIL_EPOCH", 0);
                long protectedUntilDate = ODBCWrapper.Utils.GetLongSafeVal(dr, "PROTECTED_UNTIL_EPOCH", 0);
                RecordingInternalStatus recordingInternalStatus =
                    (RecordingInternalStatus)ODBCWrapper.Utils.GetIntSafeVal(dr, "RECORDING_STATUS");
                DomainRecordingStatus domainRecordingStatus =
                    (DomainRecordingStatus)ODBCWrapper.Utils.GetIntSafeVal(dr, "DOMAIN_RECORDING_STATUS");
                TstvRecordingStatus? recordingStatus = ConvertToTstvRecordingStatus(domainRecordingStatus);
                DateTime epgStartDate = ODBCWrapper.Utils.GetDateSafeVal(dr, "START_DATE");
                DateTime epgEndDate = ODBCWrapper.Utils.GetDateSafeVal(dr, "END_DATE");
                string externalRecordingId = ODBCWrapper.Utils.GetSafeStr(dr, "EXTERNAL_RECORDING_ID");
                string domainExternalRecordingId = ODBCWrapper.Utils.GetSafeStr(dr, "EXTERNAL_DOMAIN_RECORDING_ID");
                string crid = ODBCWrapper.Utils.GetSafeStr(dr, "CRID");
                RecordingType recordingType = (RecordingType)ODBCWrapper.Utils.GetIntSafeVal(dr, "RECORDING_TYPE");
                string metaDataStr = ODBCWrapper.Utils.GetSafeStr(dr, "META_DATA");
                long externalExpiryDate = ODBCWrapper.Utils.GetLongSafeVal(dr, "EXTERNAL_EXPIRY_DATE", 0);

                Dictionary<string, string> metaData = null;
                try
                {
                    metaData = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(metaDataStr);
                }
                catch { }

                if (!recordingStatus.HasValue)
                {
                    log.ErrorFormat(
                        "Failed Convert DomainRecordingStatus: {0} to TstvRecordingStatus for recordingID: {1}, epgID: {2}",
                        domainRecordingStatus, recordingID, epgId);
                    return recording;
                }
                // if the domain recording status was 1 now recordingStatus is OK and we need to get recordingStatus from recordings and not domains table
                else if (recordingStatus.Value == TstvRecordingStatus.OK)
                {
                    recordingStatus = ConvertToTstvRecordingStatus(recordingInternalStatus, epgStartDate, epgEndDate, createDate);
                    if (!recordingStatus.HasValue)
                    {
                        log.ErrorFormat(
                            "Failed Convert RecordingInternalStatus: {0} to TstvRecordingStatus for recordingID: {1}, epgID: {2}",
                            recordingInternalStatus, recordingID, epgId);
                        return recording;
                    }

                    // if internal recording status was 0 now recordingStatus is OK and we need to set recording status according to RecordingsManager
                    if (recordingStatus.Value == TstvRecordingStatus.OK)
                    {
                        recordingStatus = RecordingsUtils.GetTstvRecordingStatus(epgStartDate, epgEndDate,
                            TstvRecordingStatus.Scheduled);
                    }
                }

                if (string.IsNullOrEmpty(domainExternalRecordingId))
                {
                    // create recording object
                    recording = new Recording()
                    {
                        Id = recordingID,
                        UserId = userId,
                        EpgId = epgId,
                        ChannelId = epgChannelId,
                        EpgStartDate = epgStartDate,
                        EpgEndDate = epgEndDate,
                        CreateDate = createDate,
                        UpdateDate = updateDate,
                        RecordingStatus = recordingStatus.Value,
                        ExternalRecordingId = externalRecordingId,
                        Crid = crid,
                        Type = recordingType
                    };
                }
                else
                {
                    // create external recording object
                    recording = new ExternalRecording()
                    {
                        Id = recordingID,
                        UserId = userId,
                        EpgId = epgId,
                        ChannelId = epgChannelId,
                        EpgStartDate = epgStartDate,
                        EpgEndDate = epgEndDate,
                        CreateDate = createDate,
                        UpdateDate = updateDate,
                        RecordingStatus = recordingStatus.Value,
                        ExternalRecordingId = externalRecordingId,
                        Crid = crid,
                        Type = recordingType,
                        ExternalDomainRecordingId = domainExternalRecordingId,
                        MetaData = metaData
                    };
                }

                // if recording status is Recorded then set ViewableUntilDate
                if (recording.RecordingStatus == TstvRecordingStatus.Recorded)
                {
                    recording.ViewableUntilDate = viewableUntilEpoch;

                    if (externalExpiryDate > 0)
                    {
                        recording.ViewableUntilDate = externalExpiryDate;
                    }

                    // if recording is/was protected then set ProtectedUntilDate
                    if (protectedUntilDate > 0)
                    {
                        recording.ProtectedUntilDate = protectedUntilDate;
                        // update viewableUntilDate incase protectedUntilDate is bigger than viewableUntilDate
                        if (recording.ProtectedUntilDate.Value > recording.ViewableUntilDate.Value)
                        {
                            recording.ViewableUntilDate = recording.ProtectedUntilDate;
                        }
                    }

                    long currentUtcEpoch = TVinciShared.DateUtils.GetUtcUnixTimestampNow();
                    // modify recordings status to Deleted if it's currently not viewable
                    if (recording.ViewableUntilDate.Value < currentUtcEpoch)
                    {
                        /***** Currently the LifeTimePeriodExpired status is only for backend inner needs and we are not exposing it on the REST to the client *****
                        recording.RecordingStatus = TstvRecordingStatus.LifeTimePeriodExpired;*/
                        recording.RecordingStatus = TstvRecordingStatus.Deleted;
                    }
                }

                recording.IsProtected = recording.ProtectedUntilDate.HasValue;

                // if we got until here then recording.Status is OK
                recording.Status =
                    new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            }

            return recording;
        }

        internal static Dictionary<long, HandleDomainQuataByRecordingTask> UpdateAndGetExpiredRecordingsTasks(long unixTimeStampNow)
        {
            Dictionary<long, HandleDomainQuataByRecordingTask> expiredRecordings = new Dictionary<long, HandleDomainQuataByRecordingTask>();
            DataTable dt = RecordingsDAL.UpdateAndGetExpiredRecordingsTasks(unixTimeStampNow);
            if (dt != null && dt.Rows != null)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    long id = ODBCWrapper.Utils.GetLongSafeVal(dr, "ID", 0);
                    if (id > 0 && !expiredRecordings.ContainsKey(id))
                    {
                        long recordingId = ODBCWrapper.Utils.GetLongSafeVal(dr, "RECORDING_ID", 0);
                        int groupId = ODBCWrapper.Utils.GetIntSafeVal(dr, "GROUP_ID", 0);
                        long scheduledExpirationEpoch = ODBCWrapper.Utils.GetLongSafeVal(dr, "scheduled_expiration_epoch", 0);
                        HandleDomainQuataByRecordingTask expiredRecording = new HandleDomainQuataByRecordingTask()
                        {
                            Id = id,
                            RecordingId = recordingId,
                            ScheduledExpirationEpoch = scheduledExpirationEpoch,
                            GroupId = groupId
                        };
                        expiredRecordings.Add(id, expiredRecording);
                    }
                }
            }

            return expiredRecordings;
        }

        internal static int GetDomainDefaultQuota(int groupId, long domainId)
        {
            int domainDefaultQuota = 0;
            try
            {
                string key = UtilsDal.GetDefaultQuotaInSecondsKey(groupId, domainId);
                bool res = ConditionalAccessCache.GetItem<int>(key, out domainDefaultQuota);
                if (!res || domainDefaultQuota == 0)
                {
                    domainDefaultQuota = (int)GetTimeShiftedTvPartnerSettings(groupId).DefaultQuota;
                    res = ConditionalAccessCache.AddItem(key, domainDefaultQuota);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("failed to get default quota in seconds for domainID = {0}, groupID = {1} , ex = {2}", domainId, groupId, ex.Message);
                domainDefaultQuota = 0;
            }

            return domainDefaultQuota;
        }

        internal static List<ExtendedSearchResult> GetFirstFollowerEpgIdsToRecord(int groupId, string epgChannelId, string seriesId, int seasonNumber, DateTime startDate)
        {
            List<ExtendedSearchResult> programs = null;

            try
            {
                var seriesIdName = SERIES_ALIAS;
                var seasonNumberName = SEASON_ALIAS;

                // support for OPC accout
                if (CatalogManager.Instance.DoesGroupUsesTemplates(groupId))
                {
                    seriesIdName = SERIES_ID;
                    seasonNumberName = SEASON_NUMBER;
                }

                StringBuilder ksql = new StringBuilder();
                ksql.AppendFormat("(and {0} = '{1}' ", seriesIdName, seriesId);

                if (seasonNumber > 0)
                    ksql.AppendFormat("{0} = '{1}' ", seasonNumberName, seasonNumber);

                ksql.AppendFormat("start_date > '{0}' ", TVinciShared.DateUtils.DateTimeToUtcUnixTimestampSeconds(startDate));
                ksql.AppendFormat("epg_channel_id = '{0}' enable_cdvr != '2')", epgChannelId);

                ExtendedSearchRequest request = new ExtendedSearchRequest()
                {
                    m_nGroupID = groupId,
                    m_dServerTime = DateTime.UtcNow,
                    m_nPageIndex = 0,
                    m_nPageSize = 0,
                    assetTypes = new List<int> { 0 },
                    filterQuery = ksql.ToString(),
                    order = new ApiObjects.SearchObjects.OrderObj()
                    {
                        m_eOrderBy = ApiObjects.SearchObjects.OrderBy.START_DATE,
                        m_eOrderDir = ApiObjects.SearchObjects.OrderDir.ASC
                    },
                    m_oFilter = new Filter()
                    {
                        m_bOnlyActiveMedia = true
                    },
                    ExtraReturnFields = new List<string> { "epg_channel_id", "crid", "metas.episodenumber", "metas.seasonnumber" },
                };
                FillCatalogSignature(request);

                UnifiedSearchResponse response = request.GetResponse(request) as UnifiedSearchResponse;

                if (response == null || response.status == null)
                {
                    log.ErrorFormat("Got empty response from Catalog 'GetResponse' for 'ExtendedSearchRequest'");
                    return programs;
                }
                if (response.status.Code != (int)eResponseStatus.OK)
                {
                    log.ErrorFormat("Got error response from catalog 'GetResponse' for 'ExtendedSearchRequest'. response: code = {0}, message = {1}", response.status.Code, response.status.Message);
                    return programs;
                }

                programs = response.searchResults.Select(sr => (ExtendedSearchResult)sr).ToList();
            }

            catch
            {
                log.ErrorFormat("Failed GetFirstFollowerEpgIdsToRecord, channelId: {0}, seriesId: {1}, seassonNumber: {2}, windowStartDate: {3}", epgChannelId, seriesId, seasonNumber, startDate);
            }

            return programs;
        }

        internal static Dictionary<long, Recording> GetEpgToRecordingsMapByCridAndChannel(int groupId, string crid, long channelId, long epgId)
        {
            Dictionary<long, Recording> epgToRecordingMap = new Dictionary<long, Recording>();
            DataTable dt = RecordingsDAL.GetEpgToRecordingsMapByCridChannelAndEpgId(groupId, crid, channelId, epgId);
            if (dt != null && dt.Rows != null)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    Recording recording = BuildRecordingFromDataRow(dr);
                    // add recording if its valid and the epg doesn't already exist in dictionary
                    if (recording != null && recording.Status != null && recording.Status.Code == (int)eResponseStatus.OK && !epgToRecordingMap.ContainsKey(recording.EpgId))
                    {
                        epgToRecordingMap.Add(recording.EpgId, recording);
                    }
                }
            }

            return epgToRecordingMap;
        }

        internal static Recording BuildRecordingFromDataRow(DataRow dr)
        {
            Recording recording = new Recording();
            long id = ODBCWrapper.Utils.GetLongSafeVal(dr, "ID");
            if (id > 0)
            {
                long epgId = ODBCWrapper.Utils.GetLongSafeVal(dr, "EPG_PROGRAM_ID");
                long epgChannelId = ODBCWrapper.Utils.GetLongSafeVal(dr, "EPG_CHANNEL_ID");
                RecordingInternalStatus recordingInternalStatus = (RecordingInternalStatus)ODBCWrapper.Utils.GetIntSafeVal(dr, "RECORDING_STATUS", 0);
                DateTime epgStartDate = ODBCWrapper.Utils.GetDateSafeVal(dr, "START_DATE");
                DateTime epgEndDate = ODBCWrapper.Utils.GetDateSafeVal(dr, "END_DATE");
                DateTime createDate = ODBCWrapper.Utils.GetDateSafeVal(dr, "CREATE_DATE");
                DateTime updateDate = ODBCWrapper.Utils.GetDateSafeVal(dr, "UPDATE_DATE");
                long? viewableUntilDate = ODBCWrapper.Utils.GetLongSafeVal(dr, "VIEWABLE_UNTIL_EPOCH", 0);
                string externalRecordingId = ODBCWrapper.Utils.GetSafeStr(dr, "EXTERNAL_RECORDING_ID");
                string crid = ODBCWrapper.Utils.GetSafeStr(dr, "CRID");
                int getStatusRetries = ODBCWrapper.Utils.GetIntSafeVal(dr, "GET_STATUS_RETRIES", 0);

                if (recordingInternalStatus < 0)
                {
                    log.ErrorFormat("Failed getting recordingInternalStatus for recording with id: {0}", id);
                    return recording;
                }

                TstvRecordingStatus? recordingStatus = ConvertToTstvRecordingStatus(recordingInternalStatus, epgStartDate, epgEndDate, createDate);

                if (!recordingStatus.HasValue)
                {
                    log.ErrorFormat("Failed Convert RecordingInternalStatus: {0} to TstvRecordingStatus for recordingID: {1}", recordingInternalStatus, id);
                    return recording;
                }

                // create recording object
                recording = new Recording()
                {
                    Id = id,
                    EpgId = epgId,
                    ChannelId = epgChannelId,
                    EpgStartDate = epgStartDate,
                    EpgEndDate = epgEndDate,
                    CreateDate = createDate,
                    UpdateDate = updateDate,
                    RecordingStatus = recordingStatus.Value,
                    ExternalRecordingId = externalRecordingId,
                    Crid = crid,
                    GetStatusRetries = getStatusRetries
                };

                // if recording status is Recorded then set ViewableUntilDate
                if (recording.RecordingStatus == TstvRecordingStatus.Recorded)
                {
                    recording.ViewableUntilDate = viewableUntilDate > 0 ? viewableUntilDate : null;
                }

                // if we got until here then recording.Status is OK
                recording.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            }

            return recording;
        }

        public SeriesRecording FollowSeasonOrSeries(int groupId, string userId, long domainID, long epgId, RecordingType recordingType, ref bool isSeriesFollowed,
            ref List<long> futureSeriesRecordingIds, EPGChannelProgrammeObject epg = null, SeriesRecordingOption seriesRecordingOption = null)
        {
            SeriesRecording seriesRecording = new SeriesRecording();
            if (epg == null)
            {
                List<EPGChannelProgrammeObject> epgs = Utils.GetEpgsByIds(groupId, new List<long>() { epgId });
                if (epgs == null || epgs.Count != 1)
                {
                    log.DebugFormat("Failed Getting EPG from Catalog, DomainID: {0}, UserID: {1}, EpgId: {2}", domainID, userId, epgId);
                    return seriesRecording;
                }
                else
                {
                    epg = epgs[0];
                }
            }

            Dictionary<string, string> epgFieldMappings = GetEpgFieldTypeEntitys(groupId, epg, recordingType == RecordingType.Season);
            if (epgFieldMappings == null || epgFieldMappings.Count == 0)
            {
                log.ErrorFormat("failed GetEpgFieldTypeEntitys, groupId: {0}, epgId: {1}, recordingType: {2}", groupId, epg.EPG_ID, recordingType.ToString());
                return seriesRecording;
            }

            string seriesId = epgFieldMappings[SERIES_ID];
            int seasonNumber = 0, episodeNumber = 0;

            if (recordingType == RecordingType.Season && (!epgFieldMappings.ContainsKey(SEASON_NUMBER) || !int.TryParse(epgFieldMappings[SEASON_NUMBER], out seasonNumber)))
            {
                log.ErrorFormat("failed parsing SEASON_NUMBER, groupId: {0}, epgId: {1}, recordingType: {2}", groupId, epg.EPG_ID, recordingType.ToString());
                return seriesRecording;
            }

            // currently we don't care about episode number so no log + error if we can't parse it
            if (epgFieldMappings.ContainsKey(EPISODE_NUMBER))
            {
                int.TryParse(epgFieldMappings[EPISODE_NUMBER], out episodeNumber);
            }

            long channelId;
            if (!long.TryParse(epg.EPG_CHANNEL_ID, out channelId))
            {
                log.ErrorFormat("Error on FollowSeasonOrSeries while trying to parse epgChannelId: {0}", epg.EPG_CHANNEL_ID);
                return seriesRecording;
            }

            isSeriesFollowed = RecordingsDAL.IsSeriesFollowed(groupId, seriesId, seasonNumber, channelId);

            long? startDateRecording = null;
            if (seriesRecordingOption != null && seriesRecordingOption.ChronologicalRecordStartTime != ChronologicalRecordStartTime.None)
            {
                if (seriesRecordingOption.ChronologicalRecordStartTime == ChronologicalRecordStartTime.Now)
                {
                    startDateRecording = DateUtils.GetUtcUnixTimestampNow();
                }
                else if (seriesRecordingOption.ChronologicalRecordStartTime == ChronologicalRecordStartTime.EpgStartTime && !string.IsNullOrEmpty(epg.START_DATE) &&
                    DateTime.TryParseExact(epg.START_DATE, Notification.AnnouncementManager.EPG_DATETIME_FORMAT,
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out DateTime epgStartDate))
                {
                    startDateRecording = DateUtils.DateTimeToUtcUnixTimestampSeconds(epgStartDate);
                }
            }

            // insert or update domain_series table
            seriesRecording = RecordingsDAL.FollowSeries(groupId, userId, domainID, epgId, channelId, seriesId, seasonNumber,
                episodeNumber, (int)recordingType, seriesRecordingOption, startDateRecording);
            
            // check if the user has future single episodes of the series/season and return them so we will cancel them and they will be recorded as part of series/season
            var domainSeriesRecording = (DomainSeriesRecording)seriesRecording;
            List<ExtendedSearchResult> futureRecordingsOfSeasonOrSeries = SearchSeriesRecordings(groupId, new List<string>(), new List<DomainSeriesRecording>() { domainSeriesRecording }, SearchSeriesRecordingsTimeOptions.future);

            if (futureRecordingsOfSeasonOrSeries != null)
            {
                foreach (ExtendedSearchResult futureRecordingSearchResult in futureRecordingsOfSeasonOrSeries)
                {
                    long recordingId;
                    if (long.TryParse(futureRecordingSearchResult.AssetId, out recordingId))
                    {
                        futureSeriesRecordingIds.Add(recordingId);
                    }
                }
            }

            return seriesRecording;
        }

        public Dictionary<string, string> GetEpgFieldTypeEntitys(int groupId, EPGChannelProgrammeObject epg, bool isSeasonRequired = false)
        {
            Dictionary<string, string> epgFieldMappings = new Dictionary<string, string>();
            try
            {
                // support for OPC accout
                if (CatalogManager.Instance.DoesGroupUsesTemplates(groupId))
                {
                    return GetEpgFieldTypeEntitysForOpcAccount(groupId, epg);
                }

                List<ApiObjects.Epg.FieldTypeEntity> metaTagsMappings = GetAliasMappingFields(groupId);
                if (metaTagsMappings == null || metaTagsMappings.Count == 0)
                {
                    log.DebugFormat("No alias mapping returned from 'GetAliasMappingFields'. groupId = {0} ", groupId);
                    return epgFieldMappings;
                }

                ApiObjects.Epg.FieldTypeEntity field = metaTagsMappings.Where(m => m.Alias.ToLower() == SERIES_ALIAS).FirstOrDefault();
                if (field == null)
                {
                    log.DebugFormat("alias for series_id was not found. group_id = {0}", groupId);
                    return epgFieldMappings;
                }
                else if (field.FieldType == FieldTypes.Meta)
                {
                    if (epg.EPG_Meta != null && epg.EPG_Meta.Count > 0)
                    {
                        var meta = epg.EPG_Meta.Where(x => x.Key == field.Name.ToLower()).FirstOrDefault();
                        if (!meta.Equals(default(EPGDictionary)))
                        {
                            epgFieldMappings.Add(SERIES_ID, meta.Value);
                        }
                    }
                    else
                    {
                        log.DebugFormat("alias for series_id was not found - no metas on epg. group_id = {0}", groupId);
                        return epgFieldMappings;
                    }
                }
                else if (field.FieldType == FieldTypes.Tag)
                {
                    if (epg.EPG_TAGS != null && epg.EPG_TAGS.Count > 0)
                    {
                        var tag = epg.EPG_TAGS.Where(x => x.Key == field.Name.ToLower()).FirstOrDefault();
                        if (!tag.Equals(default(EPGDictionary)))
                        {
                            epgFieldMappings.Add(SERIES_ID, tag.Value);
                        }
                    }
                    else
                    {
                        log.DebugFormat("alias for series_id was not found - no tags on epg. group_id = {0}", groupId);
                        return epgFieldMappings;
                    }
                }

                field = metaTagsMappings.Where(m => m.Alias.ToLower() == SEASON_ALIAS).FirstOrDefault();
                if (isSeasonRequired && field == null)
                {
                    log.DebugFormat("alias for season_number was not found. group_id = {0}", groupId);
                    return epgFieldMappings;
                }
                else if (field != null)
                {
                    if (field.FieldType == FieldTypes.Meta && epg.EPG_Meta != null && epg.EPG_Meta.Count > 0)
                    {
                        var meta = epg.EPG_Meta.Where(x => x.Key == field.Name.ToLower()).FirstOrDefault();
                        if (!meta.Equals(default(EPGDictionary)))
                        {
                            epgFieldMappings.Add(SEASON_NUMBER, meta.Value);
                        }
                    }
                    else if (field.FieldType == FieldTypes.Tag && epg.EPG_TAGS != null && epg.EPG_TAGS.Count > 0)
                    {
                        var tag = epg.EPG_TAGS.Where(x => x.Key == field.Name.ToLower()).FirstOrDefault();
                        if (!tag.Equals(default(EPGDictionary)))
                        {
                            epgFieldMappings.Add(SEASON_NUMBER, tag.Value);
                        }
                    }
                }

                field = metaTagsMappings.Where(m => m.Alias.ToLower() == EPISODE_ALIAS).FirstOrDefault();
                if (field != null)
                {
                    if (field.FieldType == FieldTypes.Meta && epg.EPG_Meta != null && epg.EPG_Meta.Count > 0)
                    {
                        var meta = epg.EPG_Meta.Where(x => x.Key == field.Name.ToLower()).FirstOrDefault();
                        if (!meta.Equals(default(EPGDictionary)))
                        {
                            epgFieldMappings.Add(EPISODE_NUMBER, meta.Value);
                        }
                    }
                    else if (field.FieldType == FieldTypes.Tag && epg.EPG_TAGS != null && epg.EPG_TAGS.Count > 0)
                    {
                        var tag = epg.EPG_TAGS.Where(x => x.Key == field.Name.ToLower()).FirstOrDefault();
                        if (!tag.Equals(default(EPGDictionary)))
                        {
                            epgFieldMappings.Add(EPISODE_NUMBER, tag.Value);
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                log.Error("Failed GetEpgFieldTypeEntitys", ex);
            }

            return epgFieldMappings;
        }

        internal static Dictionary<string, string> GetEpgFieldTypeEntitysForOpcAccount(int groupId, EPGChannelProgrammeObject epg)
        {
            Dictionary<string, string> epgFieldMappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            try
            {
                CatalogGroupCache catalogGroupCache;
                if (!CatalogManager.Instance.TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
                {
                    log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling GetEpgFieldTypeEntitysForOpcAccount", groupId);
                    return epgFieldMappings;
                }

                foreach (string field in EpgAssetManager.RecordingFieldsSystemName)
                {
                    if (catalogGroupCache.TopicsMapBySystemNameAndByType.ContainsKey(field))
                    {
                        bool isTag = catalogGroupCache.TopicsMapBySystemNameAndByType[field].ContainsKey(MetaType.Tag.ToString());
                        if (isTag)
                        {
                            if (epg.EPG_TAGS != null && epg.EPG_TAGS.Count > 0 && epg.EPG_TAGS.Any(x => x.Key.ToLower() == field.ToLower()))
                            {
                                epgFieldMappings.Add(field, epg.EPG_TAGS.First(x => x.Key.ToLower() == field.ToLower()).Value);
                            }
                            else
                            {
                                log.Warn($"alias for {field} was not found - no tags on epg. group_id = {groupId}");
                            }
                        }
                        else
                        {
                            if (epg.EPG_Meta != null && epg.EPG_Meta.Count > 0 && epg.EPG_Meta.Any(x => x.Key.ToLower() == field.ToLower()))
                            {
                                epgFieldMappings.Add(field, epg.EPG_Meta.First(x => x.Key.ToLower() == field.ToLower()).Value);
                            }
                            else
                            {
                                log.Warn($"alias for {field} was not found - no metas on epg. group_id = {groupId}");
                            }
                        }
                    }
                    else
                    {
                        log.ErrorFormat("field {0} was not found as meta or tag for group_id = {1}", field, groupId);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Failed GetEpgFieldTypeEntitysForOpcAccount", ex);
            }

            return epgFieldMappings;
        }

        public List<ExtendedSearchResult> SearchSeriesRecordings(int groupID, List<string> excludedCrids, List<DomainSeriesRecording> series, SearchSeriesRecordingsTimeOptions SearchSeriesRecordingsTimeOption, bool limitPageSize = false)
        {
            List<ExtendedSearchResult> recordings = null;

            // build the KSQL for the series
            string seriesId;
            string seasonNumber;
            string episodeNumber;

            if (!GetSeriesMetaTagsFieldsNamesForSearch(groupID, out seriesId, out seasonNumber, out episodeNumber))
            {
                log.ErrorFormat("failed to 'GetSeriesMetaTagsNamesForGroup' for groupId = {0} ", groupID);
                return recordings;
            }

            // build the filter query for the search
            StringBuilder ksql = new StringBuilder("(and (or ");
            StringBuilder seasonsToExclude = null;
            string season = null;

            foreach (var serie in series)
            {
                season = (serie.SeasonNumber > 0 && !string.IsNullOrEmpty(seasonNumber)) ? string.Format("{0} = '{1}' ", seasonNumber, serie.SeasonNumber) : string.Empty;
                seasonsToExclude = new StringBuilder();
                if (serie.ExcludedSeasons != null && serie.ExcludedSeasons.Count > 0)
                {
                    foreach (int seasonNumberToExclude in serie.ExcludedSeasons)
                    {
                        seasonsToExclude.AppendFormat("{0} != '{1}' ", seasonNumber, seasonNumberToExclude);
                    }
                }

                var futureRecording = string.Empty;

                //(or (and seasonnumber = '{serie.SeriesRecordingOption.MinSeasonNumber}' episodenumber >= '{serie.SeriesRecordingOption.MinEpisodeNumber}') seasonnumber > '{serie.SeriesRecordingOption.MinSeasonNumber}' end_date >= '{minDate}' )
                if (serie.SeriesRecordingOption != null && serie.SeriesRecordingOption.IsValid())
                {
                    futureRecording = "(or ";
                    if (serie.SeriesRecordingOption.MinSeasonNumber > 0 && serie.SeriesRecordingOption.MinEpisodeNumber > 0)
                    {
                        //metas.X
                        futureRecording += $"(and seasonnumber = '{serie.SeriesRecordingOption.MinSeasonNumber}" +
                            $"' episodenumber >= '{serie.SeriesRecordingOption.MinEpisodeNumber}') seasonnumber > " +
                            $"'{serie.SeriesRecordingOption.MinSeasonNumber}'";
                    }

                    if (serie.SeriesRecordingOption.StartDateRecording.HasValue && serie.SeriesRecordingOption.StartDateRecording > 0)
                    {
                        var minDate = serie.SeriesRecordingOption.StartDateRecording.Value;
                        var addPrefix = !futureRecording.Contains("and");
                        var prefixAnd = addPrefix ? "(and" : "";
                        var bracket = addPrefix ? ")" : string.Empty;
                        futureRecording += $"{prefixAnd} end_date >= '{minDate}'{bracket}"; //future
                    }
                    futureRecording += ")";
                    log.Debug($"**futureRecording: {futureRecording}");
                }

                ksql.AppendFormat("(and {0} = '{1}' epg_channel_id = '{2}' {3} {4} {5})", seriesId, serie.SeriesId, serie.EpgChannelId, season, seasonsToExclude.ToString(), futureRecording);
            }

            var order = new ApiObjects.SearchObjects.OrderObj()
            {
                m_eOrderBy = ApiObjects.SearchObjects.OrderBy.START_DATE,
                m_eOrderDir = ApiObjects.SearchObjects.OrderDir.ASC
            };

            var pageSize = 0;

            switch (SearchSeriesRecordingsTimeOption)
            {
                case SearchSeriesRecordingsTimeOptions.past:
                    {
                        string recordingLifetime = string.Empty;
                        var tstvSettings = Utils.GetTimeShiftedTvPartnerSettings(groupID);

                        DateTime? minDate = null;
                        if (tstvSettings.IsRecordingScheduleWindowEnabled.Value)
                        {
                            if (tstvSettings.RecordingScheduleWindow.Value <= 0)
                            {
                                return recordings;
                            }

                            minDate = DateTime.UtcNow.AddMinutes(-tstvSettings.RecordingScheduleWindow.Value);
                        }

                        if (tstvSettings.RecordingLifetimePeriod.HasValue)
                        {
                            DateTime dateTime = DateTime.UtcNow.AddDays(-tstvSettings.RecordingLifetimePeriod.Value);
                            if (!minDate.HasValue || dateTime > minDate.Value)
                            {
                                minDate = dateTime;
                            }
                        }

                        if (minDate.HasValue)
                        {
                            var minDateUnix = DateUtils.DateTimeToUtcUnixTimestampSeconds(minDate.Value);
                            recordingLifetime = $"end_date > '{minDateUnix}'";
                        }

                        //BEO - BEO-10020: order by, limit top 200 (TCM value)
                        if (limitPageSize)
                        {
                            order = new ApiObjects.SearchObjects.OrderObj()
                            {
                                m_eOrderBy = ApiObjects.SearchObjects.OrderBy.START_DATE,
                                m_eOrderDir = ApiObjects.SearchObjects.OrderDir.DESC
                            };

                            pageSize = ApplicationConfiguration.Current.CatalogLogicConfiguration.SearchPastSeriesRecordingsPageSize.Value;
                        }

                        ksql.AppendFormat($") start_date < '0' {recordingLifetime})");

                        break;
                    }
                case SearchSeriesRecordingsTimeOptions.future:
                    ksql.AppendFormat(") start_date > '0')");
                    break;
                case SearchSeriesRecordingsTimeOptions.all:
                default:
                    ksql.AppendFormat("))");
                    break;
            }

            // get program ids
            try
            {
                ExtendedSearchRequest request = new ExtendedSearchRequest()
                {
                    m_nGroupID = groupID,
                    m_dServerTime = DateTime.UtcNow,
                    m_nPageIndex = 0,
                    m_nPageSize = pageSize,
                    assetTypes = new List<int> { 1 },
                    filterQuery = ksql.ToString(),
                    order = order,
                    m_oFilter = new Filter()
                    {
                        m_bOnlyActiveMedia = true
                    },
                    excludedCrids = excludedCrids != null ? excludedCrids : null,
                    ExtraReturnFields = new List<string> { "epg_id", "crid", "epg_channel_id", seriesId, seasonNumber },
                    ShouldUseSearchEndDate = true
                };
                FillCatalogSignature(request);
                string catalogUrl = ApplicationConfiguration.Current.WebServicesConfiguration.Catalog.URL.Value;
                if (string.IsNullOrEmpty(catalogUrl))
                {
                    log.Error("Catalog Url is null or empty");
                    return recordings;
                }

                UnifiedSearchResponse response = request.GetResponse(request) as UnifiedSearchResponse;

                if (response == null || response.status == null)
                {
                    log.ErrorFormat("Got empty response from Catalog 'GetResponse' for 'ExtendedSearchRequest'");
                    return recordings;
                }
                if (response.status.Code != (int)eResponseStatus.OK)
                {
                    log.ErrorFormat("Got error response from catalog 'GetResponse' for 'ExtendedSearchRequest'. response: code = {0}, message = {1}", response.status.Code, response.status.Message);
                    return recordings;
                }

                recordings = response.searchResults.Select(sr => (ExtendedSearchResult)sr).ToList();
            }

            catch (Exception ex)
            {
                log.Error("SearchSeriesRecordings - Failed UnifiedSearchRequest Request To Catalog", ex);
            }

            return recordings;
        }

        internal bool GetSeriesMetaTagsFieldsNamesForSearch(int groupId, out string seriesIdName, out string seasonNumberName, out string episodeNumberName)
        {
            seriesIdName = seasonNumberName = episodeNumberName = string.Empty;
            // support for OPC accout
            if (CatalogManager.Instance.DoesGroupUsesTemplates(groupId))
            {
                if (!CatalogManager.Instance.TryGetCatalogGroupCacheFromCache(groupId, out CatalogGroupCache catalogGroupCache))
                {
                    log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling GetSeriesMetaTagsFieldsNamesForSearch", groupId);
                    return false;
                }

                if (!catalogGroupCache.TopicsMapBySystemNameAndByType.ContainsKey(SERIES_ID)
                    || !catalogGroupCache.TopicsMapBySystemNameAndByType.ContainsKey(SEASON_NUMBER)
                    || !catalogGroupCache.TopicsMapBySystemNameAndByType.ContainsKey(EPISODE_NUMBER))
                {
                    log.ErrorFormat("topic for seriesId / seasonNumber / episodeNumber weren't found. group_id = {0}", groupId);
                    return false;
                }

                seriesIdName = $"metas.{SERIES_ID}";
                seasonNumberName = $"metas.{SEASON_NUMBER}";
                episodeNumberName = $"metas.{EPISODE_NUMBER}";
                return true;
            }

            var metaTagsMappings = GetAliasMappingFields(groupId);
            if (metaTagsMappings == null || metaTagsMappings.Count == 0)
            {
                log.ErrorFormat("failed to 'GetAliasMappingFields' for seriesId. groupId = {0} ", groupId);
                return false;
            }

            var feild = metaTagsMappings.Where(m => m.Alias.ToLower() == "series_id").FirstOrDefault();
            if (feild == null)
            {
                log.ErrorFormat("alias for series_id was not found. group_id = {0}", groupId);
                return false;
            }

            seriesIdName = string.Format("{0}.{1}", feild.FieldType == FieldTypes.Meta ? "metas" : "tags", feild.Name);

            feild = metaTagsMappings.Where(m => m.Alias.ToLower() == "season_number").FirstOrDefault();
            if (feild != null)
            {
                seasonNumberName = string.Format("{0}.{1}", feild.FieldType == FieldTypes.Meta ? "metas" : "tags", feild.Name);
            }

            feild = metaTagsMappings.Where(m => m.Alias.ToLower() == "episode_number").FirstOrDefault();
            if (feild != null)
            {
                episodeNumberName = string.Format("{0}.{1}", feild.FieldType == FieldTypes.Meta ? "metas" : "tags", feild.Name);
            }

            return true;
        }

        public List<EpgCB> GetEpgRelatedToSeriesRecording(int groupId, SeriesRecording seriesRecording, List<EpgCB> epgs, long seasonNumber = 0)
        {
            List<EpgCB> epgMatch = new List<EpgCB>();
            try
            {
                if (CatalogManager.Instance.DoesGroupUsesTemplates(groupId))
                {
                    return GetEpgRelatedToSeriesRecordingForOpcAccount(groupId, seriesRecording, epgs, seasonNumber);
                }

                List<ApiObjects.Epg.FieldTypeEntity> metaTagsMappings = GetAliasMappingFields(groupId);
                if (metaTagsMappings == null || metaTagsMappings.Count == 0)
                {
                    log.ErrorFormat("failed to 'GetAliasMappingFields' for seriesId. groupId = {0} ", groupId);
                    return new List<EpgCB>();
                }

                ApiObjects.Epg.FieldTypeEntity series_alias = metaTagsMappings.Where(m => m.Alias.ToLower() == SERIES_ALIAS).FirstOrDefault();
                if (series_alias == null)
                {
                    log.ErrorFormat("alias for series_id was not found. group_id = {0}", groupId);
                    return new List<EpgCB>();
                }

                if (series_alias.FieldType == FieldTypes.Meta)
                {
                    epgMatch = epgs.Where(x => x.Metas.Any(y => y.Key == series_alias.Name && y.Value.Contains(seriesRecording.SeriesId))).ToList();
                }
                else if (series_alias.FieldType == FieldTypes.Tag)
                {
                    epgMatch = epgs.Where(x => x.Tags.Any(y => y.Key == series_alias.Name && y.Value.Contains(seriesRecording.SeriesId))).ToList();
                }


                if (seriesRecording.SeasonNumber > 0 || seasonNumber > 0)
                {
                    long seasonNumberEqual = seriesRecording.SeasonNumber > 0 ? seriesRecording.SeasonNumber : seasonNumber;
                    ApiObjects.Epg.FieldTypeEntity season_alias = metaTagsMappings.Where(m => m.Alias.ToLower() == SEASON_ALIAS).FirstOrDefault();
                    if (season_alias == null)
                    {
                        log.ErrorFormat("alias for season_number was not found. group_id = {0}", groupId);
                        return new List<EpgCB>();
                    }

                    if (season_alias.FieldType == FieldTypes.Meta)
                    {
                        epgMatch = epgMatch.Where(x => x.Metas.Any(y => y.Key == season_alias.Name && y.Value.Contains(seasonNumberEqual.ToString()))).ToList();
                    }
                    else if (season_alias.FieldType == FieldTypes.Tag)
                    {
                        epgMatch = epgMatch.Where(x => x.Tags.Any(y => y.Key == season_alias.Name && y.Value.Contains(seasonNumberEqual.ToString()))).ToList();
                    }
                }
            }
            catch
            {
                log.ErrorFormat("failed to 'GetEpgRelatedToSeriesRecording groupId = {0}, seriesRecordingID = {1}", groupId, seriesRecording.Id);
            }
            return epgMatch;
        }

        internal static List<EpgCB> GetEpgRelatedToSeriesRecordingForOpcAccount(int groupId, SeriesRecording seriesRecording, List<EpgCB> epgs, long seasonNumber = 0)
        {
            List<EpgCB> epgMatch = new List<EpgCB>();
            try
            {
                if (!CatalogManager.Instance.TryGetCatalogGroupCacheFromCache(groupId, out CatalogGroupCache catalogGroupCache))
                {
                    log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling GetEpgRelatedToSeriesRecordingForOpcAccount", groupId);
                    return epgMatch;
                }

                if (!catalogGroupCache.TopicsMapBySystemNameAndByType.ContainsKey(SERIES_ID))
                {
                    log.ErrorFormat("topic for seriesId wasn't found. group_id = {0}", groupId);
                    return epgMatch;
                }

                bool isTag = catalogGroupCache.TopicsMapBySystemNameAndByType[SERIES_ID].ContainsKey(MetaType.Tag.ToString());

                if (isTag)
                {
                    epgMatch = epgs.Where(x => x.Tags.Any(y => y.Key.ToLower() == SERIES_ID.ToLower() && y.Value.Contains(seriesRecording.SeriesId))).ToList();
                }
                else
                {
                    epgMatch = epgs.Where(x => x.Metas.Any(y => y.Key.ToLower() == SERIES_ID.ToLower() && y.Value.Contains(seriesRecording.SeriesId))).ToList();
                }

                if (seriesRecording.SeasonNumber > 0 || seasonNumber > 0)
                {
                    long seasonNumberEqual = seriesRecording.SeasonNumber > 0 ? seriesRecording.SeasonNumber : seasonNumber;
                    if (!catalogGroupCache.TopicsMapBySystemNameAndByType.ContainsKey(SEASON_NUMBER))
                    {
                        log.ErrorFormat("topic for seasonNumber wasn't found. group_id = {0}", groupId);
                        return epgMatch;
                    }

                    isTag = catalogGroupCache.TopicsMapBySystemNameAndByType[SEASON_NUMBER].ContainsKey(MetaType.Tag.ToString());

                    if (isTag)
                    {
                        epgMatch = epgMatch.Where(x => x.Tags.Any(y => y.Key.ToLower() == SEASON_NUMBER.ToLower() && y.Value.Contains(seasonNumberEqual.ToString()))).ToList();
                    }
                    else
                    {
                        epgMatch = epgMatch.Where(x => x.Metas.Any(y => y.Key.ToLower() == SEASON_NUMBER.ToLower() && y.Value.Contains(seasonNumberEqual.ToString()))).ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("failed GetEpgRelatedToSeriesRecordingForOpcAccount groupId = {0}, seriesRecordingID = {1}", groupId, seriesRecording.Id), ex);
            }

            return epgMatch;
        }

        public string GetFollowingUserIdForSerie(int groupId, List<DomainSeriesRecording> series, ExtendedSearchResult potentialRecording,
                                                            out RecordingType recordingType, out long domainSeriesRecordingId)
        {
            string userId = null;
            domainSeriesRecordingId = 0;
            recordingType = RecordingType.Series;

            string seriesIdName;
            string seasonNumberName;
            string episodeNumberName;

            GetSeriesMetaTagsFieldsNamesForSearch(groupId, out seriesIdName, out seasonNumberName, out episodeNumberName);

            if (potentialRecording != null && potentialRecording.ExtraFields != null)
            {
                string seriesId = null;
                int seasonNumber = 0;
                foreach (var field in potentialRecording.ExtraFields)
                {
                    string key = field.key.ToLower();

                    if (key == seriesIdName.ToLower())
                    {
                        seriesId = field.value;
                    }

                    if (key == seasonNumberName.ToLower())
                    {
                        int.TryParse(field.value, out seasonNumber);
                    }
                }

                foreach (var serie in series)
                {
                    if (serie.SeriesId == seriesId && (serie.SeasonNumber == 0 || serie.SeasonNumber == seasonNumber))
                    {
                        userId = serie.UserId;
                        domainSeriesRecordingId = serie.Id;
                        recordingType = serie.Type;
                        break;
                    }
                }
            }

            return userId;
        }

        internal static long GetLongParamFromExtendedSearchResult(ExtendedSearchResult extendedResult, string paramName)
        {
            long result = 0;

            if (extendedResult != null && extendedResult.ExtraFields != null)
            {
                var field = extendedResult.ExtraFields.Where(ef => ef.key.ToLower() == paramName.ToLower()).FirstOrDefault();
                if (field != null)
                {
                    long.TryParse(field.value, out result);
                }
            }
            return result;
        }

        internal static string GetStringParamFromExtendedSearchResult(ExtendedSearchResult extendedResult, string paramName)
        {
            string result = string.Empty;

            if (extendedResult != null && extendedResult.ExtraFields != null)
            {
                var field = extendedResult.ExtraFields.Where(ef => ef.key.ToLower() == paramName.ToLower()).FirstOrDefault();
                if (field != null)
                {
                    result = field.value;
                }
            }
            return result;
        }

        internal static bool UpdateRecording(Recording recording, int groupId, int rowStatus, int isActive, RecordingInternalStatus? status)
        {
            TimeShiftedTvPartnerSettings accountSettings = GetTimeShiftedTvPartnerSettings(groupId);
            int? recordingLifetime = accountSettings.RecordingLifetimePeriod;
            DateTime? viewableUntilDate = null;

            //BEO - 7188
            if (recordingLifetime.HasValue)
            {
                viewableUntilDate = recording.EpgEndDate.AddDays(recordingLifetime.Value);
                recording.ViewableUntilDate = TVinciShared.DateUtils.DateTimeToUtcUnixTimestampSeconds(viewableUntilDate.Value);

                if (!string.IsNullOrEmpty(recording.ExternalRecordingId) && status.HasValue && status.Value == RecordingInternalStatus.Waiting)
                {
                    status = null;
                }

                return RecordingsDAL.UpdateRecording(recording, groupId, rowStatus, isActive, status, viewableUntilDate);
            }

            return false;
        }

        internal static List<Recording> GetRecordings(int groupId, List<long> recordingIds)
        {
            List<Recording> recordings = new List<Recording>();
            DataSet dataSet = RecordingsDAL.GetRecordings(groupId, recordingIds);
            if (dataSet != null && dataSet.Tables != null)
            {
                recordings = BuildRecordingsFromDataSet(dataSet);
            }

            return recordings;
        }

        internal static List<Recording> BuildRecordingsFromDataSet(DataSet dataSet)
        {
            List<Recording> recordings = new List<Recording>();

            if (dataSet != null && dataSet.Tables != null && dataSet.Tables.Count > 0 &&
                dataSet.Tables[0] != null && dataSet.Tables[0].Rows != null)
            {
                foreach (DataRow row in dataSet.Tables[0].Rows)
                {
                    Recording recording = BuildRecordingFromDataRow(row);

                    recordings.Add(recording);
                }
            }

            return recordings;
        }

        internal static Recording GetRecordingByEpgId(int groupId, long epgId)
        {
            Recording recording = null;
            DataTable dt = RecordingsDAL.GetRecordingByEpgId(groupId, epgId);
            if (dt != null && dt.Rows != null && dt.Rows.Count == 1)
            {
                recording = BuildRecordingFromDataRow(dt.Rows[0]);
            }

            return recording;
        }

        internal static Recording InsertRecording(Recording recording, int groupId, RecordingInternalStatus? status)
        {
            Recording insertedRecording = null;
            TimeShiftedTvPartnerSettings accountSettings = GetTimeShiftedTvPartnerSettings(groupId);
            int? recordingLifetime = accountSettings.RecordingLifetimePeriod;
            DateTime? viewableUntilDate = null;
            if (recordingLifetime.HasValue)
            {
                viewableUntilDate = recording.EpgEndDate.AddDays(recordingLifetime.Value);
                recording.ViewableUntilDate = TVinciShared.DateUtils.DateTimeToUtcUnixTimestampSeconds(viewableUntilDate.Value);
            }

            DataTable dt = RecordingsDAL.InsertRecording(recording, groupId, status, viewableUntilDate);
            if (dt != null && dt.Rows != null && dt.Rows.Count == 1)
            {
                insertedRecording = BuildRecordingFromDataRow(dt.Rows[0]);
            }

            return insertedRecording;
        }

        internal static Recording GetRecordingById(long id, bool takeOnlyValidRecording = true)
        {
            Recording recording = null;
            DataTable dt = RecordingsDAL.GetRecordingById(id, takeOnlyValidRecording);
            if (dt != null && dt.Rows != null && dt.Rows.Count == 1)
            {
                recording = BuildRecordingFromDataRow(dt.Rows[0]);
            }

            return recording; 
        }

        internal static List<Recording> GetAllRecordingsByStatuses(int groupId, List<int> statuses)
        {
            List<Recording> recordings = new List<Recording>();
            DataSet dataSet = RecordingsDAL.GetAllRecordingsByStatuses(groupId, statuses);
            if (dataSet != null && dataSet.Tables != null)
            {
                recordings = BuildRecordingsFromDataSet(dataSet);
            }

            return recordings;
        }

        internal static List<DomainSeriesRecording> GetDomainSeriesRecordingFromDataSet(DataSet serieDataSet)
        {
            Dictionary<long, DomainSeriesRecording> result = new Dictionary<long, DomainSeriesRecording>();

            if (serieDataSet != null && serieDataSet.Tables != null && serieDataSet.Tables.Count >= 2)
            {
                if (serieDataSet.Tables[0] != null && serieDataSet.Tables[0].Rows != null && serieDataSet.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow dr in serieDataSet.Tables[0].Rows)
                    {
                        long id = ODBCWrapper.Utils.GetLongSafeVal(dr, "ID", 0);
                        result.Add(id, new DomainSeriesRecording()
                        {
                            Id = id,
                            EpgId = ODBCWrapper.Utils.GetLongSafeVal(dr, "EPG_ID", 0),
                            SeasonNumber = ODBCWrapper.Utils.GetIntSafeVal(dr, "SEASON_NUMBER", 0),
                            SeriesId = ODBCWrapper.Utils.GetSafeStr(dr, "SERIES_ID"),
                            UserId = ODBCWrapper.Utils.GetSafeStr(dr, "USER_ID"),
                            EpgChannelId = ODBCWrapper.Utils.GetLongSafeVal(dr, "EPG_CHANNEL_ID", 0),
                            ExcludedSeasons = new List<int>(),
                            Type = (RecordingType)ODBCWrapper.Utils.GetIntSafeVal(dr, "RECORD_TYPE"),
                            EpisodeNumber = ODBCWrapper.Utils.GetIntSafeVal(dr, "EPISODE_NUMBER", 0),
                            SeriesRecordingOption = new SeriesRecordingOption
                            {
                                MinSeasonNumber = ODBCWrapper.Utils.GetIntSafeVal(dr, "MIN_SEASON_NUMBER", 0),
                                MinEpisodeNumber = ODBCWrapper.Utils.GetIntSafeVal(dr, "MIN_EPISODE_NUMBER", 0),
                                StartDateRecording = ODBCWrapper.Utils.GetLongSafeVal(dr, "START_DATE_RECORDING", 0),
                                ChronologicalRecordStartTime = (ChronologicalRecordStartTime)ODBCWrapper.Utils.GetIntSafeVal(dr, "CHRONOLOGICAL_RECORD_STARTTIME", 0)
                            }
                        });
                    }
                }
                if (serieDataSet.Tables[1] != null && serieDataSet.Tables[1].Rows != null && serieDataSet.Tables[1].Rows.Count > 0)
                {
                    long domainSeriesId = 0;
                    foreach (DataRow dr in serieDataSet.Tables[1].Rows)
                    {
                        domainSeriesId = ODBCWrapper.Utils.GetIntSafeVal(dr, "DOMAIN_SERIES_ID", 0);
                        if (result.ContainsKey(domainSeriesId))
                        {
                            result[domainSeriesId].ExcludedSeasons.Add(ODBCWrapper.Utils.GetIntSafeVal(dr, "SEASON_NUMBER", 0));
                        }
                    }
                }
            }

            return result.Values.ToList();
        }

        public static MediaObj GetMediaById(int groupID, int mediaId)
        {
            MediaObj media = null;

            try
            {
                MediasProtocolRequest request = new MediasProtocolRequest();
                request.m_nGroupID = groupID;
                request.m_nPageIndex = 0;
                request.m_nPageSize = 0;
                request.m_lMediasIds = new List<int> { mediaId };
                request.m_oFilter = new Filter()
                {
                    m_bOnlyActiveMedia = true
                };
                FillCatalogSignature(request);

                MediaResponse response = request.GetMediasByIDs(request) as MediaResponse;
                if (response != null && response.m_lObj != null && response.m_lObj.Count > 0)
                {
                    media = response.m_lObj[0] as MediaObj;
                }
            }

            catch (Exception ex)
            {
                log.Error("Failed GetMediasByIDs to Catalog", ex);
            }

            return media;
        }

        internal static bool GetRecordingPlaybackSettingsByLinearMediaIdFromCache(int groupId, int mediaId)
        {
            string key = LayeredCacheKeys.GetRecordingPlaybackSettingsKey(groupId, mediaId);
            bool? enableRecordingPlaybackNonEntitledChannel = null;
            bool res = false;
            try
            {
                bool cacheResult = LayeredCache.Instance.Get<bool?>(key, ref enableRecordingPlaybackNonEntitledChannel, GetRecordingPlaybackSettingsByLinearMediaId, new Dictionary<string, object>() { { "groupId", groupId }, { "mediaId", mediaId } }, groupId, LayeredCacheConfigNames.GET_RECORDING_PLAYBACK_SETTINGS_LAYERED_CACHE_CONFIG_NAME);

                if (cacheResult && enableRecordingPlaybackNonEntitledChannel.HasValue)
                {
                    res = enableRecordingPlaybackNonEntitledChannel.Value;
                }
            }

            catch (Exception ex)
            {
                log.Error(string.Format("failed GetRecordingPlaybackSettingsByLinearMediaIdFromCache, groupId: {0}, mediaId: {1}", groupId, mediaId), ex);
            }

            return res;
        }

        internal static Tuple<bool?, bool> GetRecordingPlaybackSettingsByLinearMediaId(Dictionary<string, object> funcParams)
        {
            bool? enableRecordingPlaybackNonEntitledChannel = null;
            bool res = false;

            try
            {
                if (funcParams != null && funcParams.Count == 2 && funcParams.ContainsKey("mediaId") && funcParams.ContainsKey("groupId"))
                {
                    int? groupId = funcParams["groupId"] as int?;
                    int? mediaId = funcParams["mediaId"] as int?;
                    if (groupId.HasValue && mediaId.HasValue)
                    {
                        MediaObj media = GetMediaById(groupId.Value, mediaId.Value);
                        if (media != null && !string.IsNullOrEmpty(media.AssetId))
                        {
                            enableRecordingPlaybackNonEntitledChannel = media.EnableRecordingPlaybackNonEntitledChannel;
                            res = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("GetRecordingPlaybackSettingsByLinearMediaId failed params : {0}", string.Join(";", funcParams.Keys)), ex);
            }

            return new Tuple<bool?, bool>(enableRecordingPlaybackNonEntitledChannel, res);
        }

        internal static bool IsDeviceInDomain(Domain domain, string udid)
        {
            if (domain != null && domain.m_deviceFamilies != null && domain.m_deviceFamilies.Count > 0)
            {
                foreach (var deviceFamily in domain.m_deviceFamilies)
                {
                    if (deviceFamily.DeviceInstances != null && deviceFamily.DeviceInstances.Count > 0 && deviceFamily.DeviceInstances.FirstOrDefault(d => d.m_deviceUDID == udid) != null)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        internal static bool IsFollowingSeries(int groupId, long domainID, string seriesId, int seasonNumber, long channelId)
        {
            long domainSeriesId = RecordingsDAL.GetDomainSeriesId(groupId, domainID, seriesId, seasonNumber, channelId);
            return domainSeriesId > 0;
        }

        internal static Dictionary<long, Recording> GetFutureDomainRecordingsByRecordingIDs(int groupID, long domainID, List<long> recordingIds, RecordingType recordingType)
        {
            Dictionary<long, Recording> DomainRecordingIdToRecordingMap = null;
            DataTable dt = RecordingsDAL.GetFutureDomainRecordingsByRecordingIDs(groupID, domainID, recordingIds, recordingType);
            if (dt != null && dt.Rows != null)
            {
                DomainRecordingIdToRecordingMap = new Dictionary<long, Recording>();
                foreach (DataRow dr in dt.Rows)
                {
                    long domainRecordingID = ODBCWrapper.Utils.GetLongSafeVal(dr, "ID");
                    if (domainRecordingID > 0)
                    {
                        Recording domainRecording = BuildDomainRecordingFromDataRow(dr);
                        // add domain recording if its valid and doesn't already exist in dictionary
                        if (domainRecording != null && domainRecording.Status != null && domainRecording.Status.Code == (int)eResponseStatus.OK && !DomainRecordingIdToRecordingMap.ContainsKey(domainRecordingID))
                        {
                            DomainRecordingIdToRecordingMap.Add(domainRecordingID, domainRecording);
                        }
                    }
                }
            }

            return DomainRecordingIdToRecordingMap;
        }

        public ApiObjects.Response.Status IsFollowingEpgAsSeriesOrSeason(int groupId, EPGChannelProgrammeObject epg, long domainId, RecordingType recordingType)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            Dictionary<string, string> epgFieldMappings = GetEpgFieldTypeEntitys(groupId, epg);
            if (epgFieldMappings == null || epgFieldMappings.Count == 0 || !epgFieldMappings.ContainsKey(Utils.SERIES_ID))
            {
                log.DebugFormat("no epgFieldMappings found, groupId: {0}, epgId: {1}", groupId, epg.EPG_ID);
                // if no mapping found we assume the user is not following the epg's season/series
                response = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                return response;
            }

            string seriesId = epgFieldMappings[Utils.SERIES_ID];
            int seasonNumber = 0;
            long channelId;
            if ((epgFieldMappings.ContainsKey(Utils.SEASON_NUMBER) && !int.TryParse(epgFieldMappings[Utils.SEASON_NUMBER], out seasonNumber)) || !long.TryParse(epg.EPG_CHANNEL_ID, out channelId))
            {
                log.ErrorFormat("failed parsing SEASON_NUMBER or EPG_CHANNEL_ID, groupId: {0}, epgId: {1}", groupId, epg.EPG_ID);
                return response;
            }

            seasonNumber = recordingType != RecordingType.Season ? 0 : seasonNumber;
            if (IsFollowingSeries(groupId, domainId, seriesId, seasonNumber, channelId))
            {
                log.DebugFormat("domain already follows the series, can't record as single, DomainID: {0}, seriesID: {1}", domainId, seriesId);
                response = new ApiObjects.Response.Status((int)eResponseStatus.AlreadyRecordedAsSeriesOrSeason, eResponseStatus.AlreadyRecordedAsSeriesOrSeason.ToString());
                return response;
            }

            response = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            return response;
        }

        internal static bool ShouldOrderByWithoutCatalg(ApiObjects.SearchObjects.OrderObj orderBy)
        {
            if (orderBy.m_eOrderBy == ApiObjects.SearchObjects.OrderBy.CREATE_DATE
                || orderBy.m_eOrderBy == ApiObjects.SearchObjects.OrderBy.ID
                || orderBy.m_eOrderBy == ApiObjects.SearchObjects.OrderBy.START_DATE)
            {
                return true;
            }
            else
            {
                return false;
            }


        }

        internal static List<Recording> OrderRecordingWithoutCatalog(List<Recording> recordings, ApiObjects.SearchObjects.OrderObj orderBy, int pageIndex, int pageSize,
                                                                     ref int totalResults, bool shouldIgnorePaging = false)
        {
            List<Recording> orderedRecordings = new List<Recording>();
            switch (orderBy.m_eOrderBy)
            {
                case ApiObjects.SearchObjects.OrderBy.ID:
                    if (orderBy.m_eOrderDir == ApiObjects.SearchObjects.OrderDir.DESC)
                    {
                        orderedRecordings = recordings.OrderByDescending(x => x.Id).ToList();
                    }
                    else
                    {
                        orderedRecordings = recordings.OrderBy(x => x.Id).ToList();
                    }
                    break;
                case ApiObjects.SearchObjects.OrderBy.START_DATE:
                    if (orderBy.m_eOrderDir == ApiObjects.SearchObjects.OrderDir.DESC)
                    {
                        orderedRecordings = recordings.OrderByDescending(x => x.EpgStartDate).ToList();
                    }
                    else
                    {
                        orderedRecordings = recordings.OrderBy(x => x.EpgStartDate).ToList();
                    }
                    break;
                case ApiObjects.SearchObjects.OrderBy.CREATE_DATE:
                    if (orderBy.m_eOrderDir == ApiObjects.SearchObjects.OrderDir.DESC)
                    {
                        orderedRecordings = recordings.OrderByDescending(x => x.CreateDate).ToList();
                    }
                    else
                    {
                        orderedRecordings = recordings.OrderBy(x => x.CreateDate).ToList();
                    }
                    break;
                default:
                    log.DebugFormat("Invalid orderBy type: {0} on OrderRecordingWithoutCatalog", orderBy.m_eOrderBy.ToString());
                    break;
            }

            totalResults = orderedRecordings.Count;
            if (!shouldIgnorePaging)
            {
                int startIndexOnList = pageIndex * pageSize;
                int rangeToGetFromList = (startIndexOnList + pageSize) > totalResults ? (totalResults - startIndexOnList) > 0 ? (totalResults - startIndexOnList) : 0 : pageSize;
                if (rangeToGetFromList > 0)
                {
                    orderedRecordings = orderedRecordings.GetRange(startIndexOnList, rangeToGetFromList);
                }
                else
                {
                    orderedRecordings.Clear();
                }
            }

            return orderedRecordings;
        }

        internal static Dictionary<long, Recording> GetDomainRecordingIdToRecordingMapByEpgIds(int groupId, long domainId, List<long> epgIds)
        {
            Dictionary<long, Recording> domainIdToRecordingMap = new Dictionary<long, Recording>();
            DataTable dt = RecordingsDAL.GetDomainExistingRecordingsByEpgIds(groupId, domainId, epgIds);
            if (dt != null && dt.Rows != null)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    long domainRecordingID = ODBCWrapper.Utils.GetLongSafeVal(dr, "ID");
                    if (domainRecordingID > 0 && !domainIdToRecordingMap.ContainsKey(domainRecordingID))
                    {
                        Recording domainRecording = BuildDomainRecordingFromDataRow(dr);
                        domainIdToRecordingMap.Add(domainRecordingID, domainRecording);
                    }
                }
            }

            return domainIdToRecordingMap;
        }

        internal static List<Recording> GetRecordingsByExternalRecordingId(int groupId, string externalRecordingId, bool isPrivateCopy)
        {
            List<Recording> recordings = new List<Recording>();
            DataTable dt = RecordingsDAL.GetRecordingsByExternalRecordingId(groupId, externalRecordingId, isPrivateCopy);
            if (dt != null && dt.Rows != null)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    Recording recording = BuildRecordingFromDataRow(dr);
                    // add recording if its valid
                    if (recording != null && recording.Status != null && recording.Status.Code == (int)eResponseStatus.OK)
                    {
                        recordings.Add(recording);
                    }
                }
            }

            return recordings;
        }

        internal static Dictionary<long, Recording> GetDomainRecordingsByDomainSeriesId(int groupID, long domainID, long domainSeriesRecordingId)
        {
            Dictionary<long, Recording> DomainRecordingIdToRecordingMap = null;
            DataTable dt = RecordingsDAL.GetDomainRecordingsByDomainSeriesId(groupID, domainID, domainSeriesRecordingId);
            if (dt != null && dt.Rows != null)
            {
                DomainRecordingIdToRecordingMap = new Dictionary<long, Recording>();
                foreach (DataRow dr in dt.Rows)
                {
                    long domainRecordingID = ODBCWrapper.Utils.GetLongSafeVal(dr, "ID");
                    if (domainRecordingID > 0)
                    {
                        Recording domainRecording = BuildDomainRecordingFromDataRow(dr);
                        // add domain recording if its valid and doesn't already exist in dictionary
                        if (domainRecording != null && domainRecording.Status != null && domainRecording.Status.Code == (int)eResponseStatus.OK && !DomainRecordingIdToRecordingMap.ContainsKey(domainRecordingID))
                        {
                            DomainRecordingIdToRecordingMap.Add(domainRecordingID, domainRecording);
                        }
                    }
                }
            }

            return DomainRecordingIdToRecordingMap;
        }

        internal static List<int> GetFileIdsByEpgChannelId(int groupId, string epgChannelId)
        {
            List<int> channelFileIds = null;
            try
            {
                string key = string.Format("Channel_{0}_FileIds", epgChannelId);
                if (!TvinciCache.WSCache.Instance.TryGet(key, out channelFileIds))
                {
                    lock (lck)
                    {
                        if (!TvinciCache.WSCache.Instance.TryGet(key, out channelFileIds))
                        {
                            HashSet<int> fileIds = new HashSet<int>();
                            log.DebugFormat("Getting file ids from DB for epgChannelId {0}", epgChannelId);
                            DataTable dt = ConditionalAccessDAL.GetFileIdsByEpgChannelId(groupId, epgChannelId);
                            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                            {
                                foreach (DataRow dr in dt.Rows)
                                {
                                    int fileId = ODBCWrapper.Utils.GetIntSafeVal(dr, "media_file_id", 0);
                                    if (fileId > 0 && !fileIds.Contains(fileId))
                                    {
                                        fileIds.Add(fileId);
                                    }
                                }
                            }

                            channelFileIds = fileIds.ToList();
                            TvinciCache.WSCache.Instance.Add(key, channelFileIds, 600);
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                log.Error($"Error in GetFileIdsToEpgIdsMap: groupID = {groupId}, epgChannelId: {epgChannelId}", ex);
            }

            return channelFileIds;
        }

        public static bool UpdateDomainSeriesRecordingsUserToMaster(int groupId, int domainId, string userId, string masterUserId)
        {
            bool result = false;
            // update domain series recordings to master user
            var domainSeriesDs = RecordingsDAL.GetDomainSeriesRecordings(groupId, domainId);
            var domainSeries = GetDomainSeriesRecordingFromDataSet(domainSeriesDs);

            if (domainSeries != null && domainSeries.Count > 0)
            {
                List<long> domainSeriesIdsToUpdate = domainSeries.Where(s => s.UserId == userId).Select(s => s.Id).ToList();
                if (domainSeriesIdsToUpdate != null)
                {
                    result = RecordingsDAL.UpdateDomainSeriesRecordingsUserId(groupId, domainSeriesIdsToUpdate, masterUserId);
                    if (!result)
                    {
                        log.ErrorFormat("Failed to update DomainSeriesRecordings to master user after deleting user = {0}, domainId = {1}", userId, domainId);
                    }
                    else
                    {
                        log.DebugFormat("Successfully updated DomainSeriesRecordings to master user after deleting user = {0}, domainId = {1}, masterUserId = {2}", userId, domainId, masterUserId);
                    }
                }
            }
            else
            {
                return true;
            }

            return result;
        }

        public static bool UpdateScheduledRecordingsUserToMaster(int groupId, int domainId, string userId, string masterUserId)
        {
            bool result = false;
            var domainScheduledRecordings = GetDomainRecordingsByTstvRecordingStatuses(groupId, domainId, new List<TstvRecordingStatus> { TstvRecordingStatus.Scheduled });

            if (domainScheduledRecordings != null && domainScheduledRecordings.Count > 0)
            {
                List<long> domainSceduledIdsToUpdate = domainScheduledRecordings.Where(r => r.Value.UserId == userId).Select(r => r.Key).ToList();
                if (domainSceduledIdsToUpdate != null)
                {
                    result = RecordingsDAL.UpdateDomainScheduledRecordingsUserId(groupId, domainSceduledIdsToUpdate, masterUserId);
                    if (!result)
                    {
                        log.ErrorFormat("Failed to update domain scheduled recordings to master user after deleting user = {0}, domainId = {1}", userId, domainId);
                    }
                    else
                    {
                        log.DebugFormat("Successfully updated domain scheduled recordings to master user after deleting user = {0}, domainId = {1}, masterUserId = {2}", userId, domainId, masterUserId);
                    }
                }
            }
            else
            {
                return true;
            }

            return result;
        }

        public static bool GetLinearMediaInfoByEpgChannelIdAndFileType(int groupId, string epgChannelId, string fileType, ref int linearMediaId, ref int mediaFileId)
        {
            bool res = false;
            DataTable dt = ApiDAL.GetLinearMediaInfoByEpgChannelIdAndFileType(groupId, epgChannelId, fileType);
            if (dt != null && dt.Rows != null && dt.Rows.Count == 1)
            {
                DataRow dr = dt.Rows[0];
                linearMediaId = ODBCWrapper.Utils.GetIntSafeVal(dr, "media_id", 0);
                mediaFileId = ODBCWrapper.Utils.GetIntSafeVal(dr, "media_file_id", 0);
                res = true;
            }

            return res;
        }

        public static bool InsertOrSetCachedEntitlementResults(long domainId, int mediaFileId, CachedEntitlementResults cachedEntitlementResults)
        {
            return ConditionalAccessDAL.InsertOrSetCachedEntitlementResults(ApplicationConfiguration.Current.Version.Value, domainId, mediaFileId, cachedEntitlementResults);
        }

        internal static CachedEntitlementResults GetCachedEntitlementResults(long domainId, int mediaFileId)
        {
            return ConditionalAccessDAL.GetCachedEntitlementResults(ApplicationConfiguration.Current.Version.Value, domainId, mediaFileId);
        }

        internal static ApiObjects.Response.Status SetResponseStatus(PriceReason priceReason)
        {
            ApiObjects.Response.Status status = null;
            switch (priceReason)
            {
                case PriceReason.PPVPurchased:
                    status = new ApiObjects.Response.Status((int)eResponseStatus.UnableToPurchasePPVPurchased, "PPV already purchased");
                    break;
                case PriceReason.Free:
                    status = new ApiObjects.Response.Status((int)eResponseStatus.UnableToPurchaseFree, "Free");
                    break;
                case PriceReason.ForPurchaseSubscriptionOnly:
                    status = new ApiObjects.Response.Status((int)eResponseStatus.UnableToPurchaseForPurchaseSubscriptionOnly, "Subscription only");
                    break;
                case PriceReason.SubscriptionPurchased:
                    status = new ApiObjects.Response.Status((int)eResponseStatus.UnableToPurchaseSubscriptionPurchased, "Already purchased (subscription)");
                    break;
                case PriceReason.NotForPurchase:
                    status = new ApiObjects.Response.Status((int)eResponseStatus.NotForPurchase, "Not valid for purchase");
                    break;
                case PriceReason.CollectionPurchased:
                    status = new ApiObjects.Response.Status((int)eResponseStatus.UnableToPurchaseCollectionPurchased, "Collection already purchased");
                    break;
                case PriceReason.PendingEntitlement: //entitlement
                    status = new ApiObjects.Response.Status((int)eResponseStatus.PendingEntitlement, "Entitlement is pending");
                    break;
                case PriceReason.PagoPurchased:
                    status = new ApiObjects.Response.Status((int)eResponseStatus.UnableToPurchaseProgramAssetGroupOfferPurchased, "ProgramAssetGroupOffer already purchased");
                    break;
                default:
                    status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "Error");
                    break;
            }

            return status;
        }

        internal static ApiObjects.Response.Status SetResponseStatus(ResponseStatus userValidStatus)
        {
            ApiObjects.Response.Status status = null;
            // user validation failed
            switch (userValidStatus)
            {
                case ResponseStatus.UserDoesNotExist:
                    status = new ApiObjects.Response.Status((int)eResponseStatus.UserDoesNotExist, "User doesn't exists");
                    break;
                case ResponseStatus.UserSuspended:
                    status = new ApiObjects.Response.Status((int)eResponseStatus.UserSuspended, "Suspended user");
                    break;
                case ResponseStatus.UserNotIndDomain:
                    status = new ApiObjects.Response.Status((int)eResponseStatus.UserNotInDomain, "User doesn't exist in household");
                    break;
                default:
                    status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "Failed to validate user");
                    break;
            }

            return status;
        }

        internal static List<MediaFile> FilterMediaFilesForAsset(int groupId, eAssetTypes assetType, long mediaId, StreamerType? streamerType, string mediaProtocol,
                                                                    PlayContextType context, List<long> fileIds, bool filterOnlyByIds = false, string sourceType = null)
        {
            List<MediaFile> files = null;

            List<MediaFile> allMediafiles = null;
            // Once we get rid of TVM parent/child groups, groupId should be used in stored procedure for security.
            // Please, note after that MediaFileCacheKey should be extended with groupId as well.
            string key = LayeredCacheKeys.GetMediaFilesKey(mediaId, assetType.ToString());
            bool cacheResult = LayeredCache.Instance.Get<List<MediaFile>>(key, ref allMediafiles, GetMediaFiles, new Dictionary<string, object>() { { "mediaId", mediaId }, { "groupId", groupId },
                                                                        { "assetType", assetType } }, groupId, LayeredCacheConfigNames.MEDIA_FILES_LAYERED_CACHE_CONFIG_NAME,
                                                                        new List<string>() { LayeredCacheKeys.GetMediaInvalidationKey(groupId, mediaId) });

            allMediafiles = allMediafiles ?? new List<MediaFile>();
            // We're using check for IsOPC to apply security concerns, once we get rid of TVM parent/child groups, this logic will be moved to stored procedure.
            allMediafiles = ValidateMediaFilesUponSecurity(allMediafiles, groupId);

            // filter
            if (allMediafiles != null && allMediafiles.Count > 0)
            {
                if (filterOnlyByIds)
                {
                    files = allMediafiles.Where(f => fileIds != null && fileIds.Contains(f.Id)).ToList();
                }
                else
                {
                    files = allMediafiles.Where(f => (!streamerType.HasValue || streamerType.Value == f.StreamerType) &&
                        ((context == PlayContextType.Trailer && f.IsTrailer) ||
                        ((context == PlayContextType.Playback || context == PlayContextType.CatchUp || context == PlayContextType.StartOver || context == PlayContextType.Download) && !f.IsTrailer)) && // check with Ira
                        (string.IsNullOrEmpty(mediaProtocol) || string.IsNullOrEmpty(f.Url) || f.Url.ToLower().StartsWith(string.Format("{0}:", mediaProtocol.ToLower()))) &&
                        (fileIds == null || fileIds.Count == 0 || fileIds.Contains(f.Id))).ToList();
                }
            }

            if (!string.IsNullOrEmpty(sourceType))
            {
                files = files?.Where(file => file.Type?.ToLower() == sourceType.ToLower()).ToList();
            }

            return files;
        }

        internal static bool ValidateMediaFileForAsset(int groupId, long mediaId, eAssetTypes assetType, long fileId)
        {
            bool result = false;

            List<MediaFile> allMediafiles = null;
            string key = LayeredCacheKeys.GetMediaFilesKey(mediaId, assetType.ToString());
            bool cacheResult = LayeredCache.Instance.Get<List<MediaFile>>(key, ref allMediafiles, GetMediaFiles, new Dictionary<string, object>() { { "mediaId", mediaId }, { "groupId", groupId },
                                                                        { "assetType", assetType } }, groupId, LayeredCacheConfigNames.MEDIA_FILES_LAYERED_CACHE_CONFIG_NAME,
                                                                        new List<string>() { LayeredCacheKeys.GetMediaInvalidationKey(groupId, mediaId) });

            if (cacheResult && allMediafiles != null)
            {
                result = allMediafiles.Any(mediaFile => mediaFile.Id == fileId);
            }

            return result; 
        }

        public static List<MediaFile> ValidateMediaFilesUponSecurity(List<MediaFile> allMediafiles, int groupId)
        {
            if (!GroupSettingsManager.Instance.IsOpc(groupId))
            {
                // If group is not OPC, we should check child subgroups for permissions as well.
                var subGroups = new GroupManager().GetSubGroup(groupId);
                return allMediafiles.Where(m => subGroups.Any(sg => sg == m.GroupId) || m.GroupId == groupId).ToList();
            }

            return allMediafiles.Where(m => m.GroupId == groupId).ToList();
        }

        public static bool IsOpc(int groupId)
        {
            return GroupSettingsManager.Instance.IsOpc(groupId);
        }

        public static ApiObjects.Response.Status GetMediaIdForAsset(int groupId, string assetId, eAssetTypes assetType, string userId, Domain domain, string udid,
            out long mediaId, out Recording recording, out EPGChannelProgrammeObject program)
        {
            ApiObjects.Response.Status status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            mediaId = 0;
            recording = null;
            program = null;
            Tuple<long, Recording, EPGChannelProgrammeObject, ApiObjects.Response.Status> tupleResult = null;
            string key = LayeredCacheKeys.GetMediaIdForAssetKey(assetId, assetType.ToString());

            try
            {
                long parsedAssetId = 0;
                long.TryParse(assetId, out parsedAssetId);
                bool cacheResult = LayeredCache.Instance.Get<Tuple<long, Recording, EPGChannelProgrammeObject, ApiObjects.Response.Status>>(key, ref tupleResult, GetMediaIdForAssetFromCache,
                new Dictionary<string, object>() { { "assetId", assetId }, { "groupId", groupId }, { "assetType", assetType }, { "userId", userId }, { "domain", domain }, { "udid", udid } },
                groupId, LayeredCacheConfigNames.MEDIA_ID_FOR_ASSET_LAYERED_CACHE_CONFIG_NAME,
                parsedAssetId > 0 ? LayeredCacheKeys.GetAssetMultipleInvalidationKeys(groupId, assetType.ToString(), parsedAssetId) : new List<string>());

                if (cacheResult && tupleResult != null)
                {
                    mediaId = tupleResult.Item1;
                    recording = tupleResult.Item2;
                    program = tupleResult.Item3;
                    status = tupleResult.Item4;
                }
            }

            catch (Exception ex)
            {
                log.Error(string.Format("failed GetMediaIdForAsset, groupId: {0}, assetId: {1}, assetType: {2}", groupId, assetId, assetType.ToString()), ex);
            }

            return status;
        }

        private static Tuple<Tuple<long, Recording, EPGChannelProgrammeObject, ApiObjects.Response.Status>, bool> GetMediaIdForAssetFromCache(Dictionary<string, object> funcParams)
        {
            bool res = false;
            Tuple<long, Recording, EPGChannelProgrammeObject, ApiObjects.Response.Status> tupleResults = null;
            long mediaId = 0;
            Recording recording = null;
            EPGChannelProgrammeObject program = null;
            ApiObjects.Response.Status status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            try
            {
                if (funcParams != null && funcParams.Count == 6)
                {
                    if (funcParams.ContainsKey("assetId") && funcParams.ContainsKey("groupId") && funcParams.ContainsKey("assetType"))
                    {
                        long id = 0;
                        string assetId = funcParams["assetId"] as string;
                        int? groupId = funcParams["groupId"] as int?;
                        eAssetTypes? assetType = funcParams["assetType"] as eAssetTypes?;
                        bool isExternalRecordingAccount = TvinciCache.GroupsFeatures.GetGroupFeatureStatus(groupId.Value, GroupFeature.EXTERNAL_RECORDINGS);
                        if (!string.IsNullOrEmpty(assetId) && assetType.HasValue && groupId.HasValue
                            && (long.TryParse(assetId, out id) || (isExternalRecordingAccount && assetType.Value == eAssetTypes.NPVR)))
                        {
                            switch (assetType)
                            {
                                case eAssetTypes.NPVR:
                                    {
                                        Domain domain = funcParams.ContainsKey("domain") ? funcParams["domain"] as Domain : null;
                                        string udid = funcParams.ContainsKey("udid") ? funcParams["udid"] as string : string.Empty;
                                        string userId = funcParams["userId"] as string;
                                        // check recording valid
                                        ApiObjects.Response.Status validateStatus = null;
                                        if (isExternalRecordingAccount)
                                        {
                                            ExternalRecording externalRecording = null;
                                            validateStatus = ValidateExternalRecording(groupId.Value, domain, udid, userId, assetId, ref externalRecording);
                                            if (externalRecording != null)
                                            {
                                                recording = externalRecording;
                                            }
                                        }
                                        else
                                        {
                                            validateStatus = ValidateRecording(groupId.Value, domain, udid, userId, id, ref recording, true);
                                        }

                                        if (validateStatus.Code != (int)eResponseStatus.OK)
                                        {
                                            log.ErrorFormat("recording is not valid - recordingId = {0}", assetId);
                                            status = new ApiObjects.Response.Status(validateStatus.Code, validateStatus.Message);
                                        }
                                        else
                                        {
                                            if (GetProgramFromRecordingCB(groupId.Value, recording.EpgId, out program))
                                            {
                                                mediaId = program.LINEAR_MEDIA_ID;
                                            }
                                            else
                                            {
                                                status = new ApiObjects.Response.Status((int)eResponseStatus.ProgramDoesntExist, "Program not found");
                                            }
                                        }
                                    }
                                    break;
                                case eAssetTypes.EPG:
                                    {
                                        List<EPGChannelProgrammeObject> epgs = Utils.GetEpgsByIds(groupId.Value, new List<long> { id });
                                        if (epgs != null && epgs.Count > 0)
                                        {
                                            program = epgs[0];
                                            mediaId = program.LINEAR_MEDIA_ID;
                                        }
                                        else
                                        {
                                            status = new ApiObjects.Response.Status((int)eResponseStatus.ProgramDoesntExist, "Program not found");
                                        }
                                    }
                                    break;
                                case eAssetTypes.MEDIA:
                                    mediaId = id;
                                    break;
                                default:
                                    break;
                            }

                            tupleResults = new Tuple<long, Recording, EPGChannelProgrammeObject, ApiObjects.Response.Status>(mediaId, recording, program, status);
                            res = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("GetMediaIdForAssetFromCache failed, parameters : {0}", string.Join(";", funcParams.Keys)), ex);
            }

            return new Tuple<Tuple<long, Recording, EPGChannelProgrammeObject, ApiObjects.Response.Status>, bool>(tupleResults, res);
        }

        internal static eService GetServiceByPlayContextType(PlayContextType contextType)
        {
            eService service;
            switch (contextType)
            {
                case PlayContextType.CatchUp:
                    service = eService.CatchUp;
                    break;
                case PlayContextType.StartOver:
                    service = eService.StartOver;
                    break;
                case PlayContextType.Trailer:
                case PlayContextType.Playback:
                default:
                    service = eService.Unknown;
                    break;
            }

            return service;
        }

        internal static eEPGFormatType GetEpgFormatTypeByPlayContextType(PlayContextType contextType)
        {
            eEPGFormatType type;

            switch (contextType)
            {
                case PlayContextType.CatchUp:
                    type = eEPGFormatType.Catchup;
                    break;
                case PlayContextType.StartOver:
                    type = eEPGFormatType.StartOver;
                    break;
                default:
                    throw new Exception("not supported context for EPG");
            }

            return type;
        }

        internal static ApiObjects.Response.Status ValidateRecording(int groupId, Domain domain, string udid, string userId,
            long domainRecordingId, ref Recording recording, bool skipDeviceCheck = false)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());

            // get device brand ID - and make sure the device is in the domain
            if (!skipDeviceCheck && !Utils.IsDeviceInDomain(domain, udid))
            {
                log.ErrorFormat("Device not in the user's domain. groupId = {0}, userId = {1}, domainId = {2}, domainRecordingId = {3}, udid = {4}",
                    groupId, userId, domain.m_nDomainID, domainRecordingId, udid);
                response = new ApiObjects.Response.Status((int)eResponseStatus.DeviceNotInDomain, "Device not in the user's domain");
                return response;
            }

            // validate recording
            var domainRecordings = Utils.GetDomainRecordingIdsToRecordingsMap(groupId, domain.m_nDomainID, new List<long>() { domainRecordingId });
            if (domainRecordings == null || domainRecordings.Count == 0 || (recording = domainRecordings[domainRecordingId]) == null)
            {
                log.ErrorFormat("Recording does not exist. groupId = {0}, userId = {1}, domainId = {2}, domainRecordingId = {3}", groupId, userId, domain.m_nDomainID, domainRecordingId);
                response = new ApiObjects.Response.Status((int)eResponseStatus.RecordingNotFound, "Recording was not found");
                return response;
            }
            if (recording.RecordingStatus != TstvRecordingStatus.Recorded)
            {
                log.ErrorFormat("Recording status is not valid for playback. groupId = {0}, userId = {1}, domainId = {2}, domainRecordingId = {3}, recording = {4}, recordingStatus = {5}",
                    groupId, userId, domain.m_nDomainID, domainRecordingId, recording.Id, recording.RecordingStatus);
                response = new ApiObjects.Response.Status((int)eResponseStatus.RecordingStatusNotValid, "Recording status is not valid");
                return response;
            }

            return response;
        }

        internal static ApiObjects.Response.Status ValidateExternalRecording(int groupId, Domain domain, string udid, string userId, string domainExternalRecordingId, ref ExternalRecording recording)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());

            // get device brand ID - and make sure the device is in the domain
            if (!Utils.IsDeviceInDomain(domain, udid))
            {
                log.ErrorFormat("Device not in the user's domain. groupId = {0}, userId = {1}, domainId = {2}, domainExternalRecordingId = {3}, udid = {4}",
                    groupId, userId, domain.m_nDomainID, domainExternalRecordingId, udid);
                response = new ApiObjects.Response.Status((int)eResponseStatus.DeviceNotInDomain, "Device not in the user's domain");
                return response;
            }

            // validate external recording, since its external and we may not be updated, we don't filter out recordings
            Dictionary<string, ExternalRecording> domainExternalRecordings = Utils.GetDomainExternalRecordings(groupId, domain.m_nDomainID, false);
            if (domainExternalRecordings == null || domainExternalRecordings.Count == 0 || !domainExternalRecordings.ContainsKey(domainExternalRecordingId) || domainExternalRecordings[domainExternalRecordingId] == null)
            {
                log.ErrorFormat("Recording does not exist. groupId = {0}, userId = {1}, domainId = {2}, domainRecordingId = {3}", groupId, userId, domain.m_nDomainID, domainExternalRecordingId);
                response = new ApiObjects.Response.Status((int)eResponseStatus.RecordingNotFound, "Recording was not found");
                return response;
            }
            else
            {
                recording = domainExternalRecordings[domainExternalRecordingId];
            }

            return response;
        }

        public static string GetAssetUrl(int groupId, eAssetTypes assetType, string url, int cdnId)
        {
            // get adapter
            bool isDefaultAdapter = false;
            var adapterResponse = GetRelevantCDN(groupId, cdnId, assetType, ref isDefaultAdapter);

            url = string.Format("{0}{1}", adapterResponse.Adapter.BaseUrl, url);

            return url;
        }

        public static Dictionary<string, string> GetLicensedLinkParamsDict(string sSiteGuid, string mediaFileIDStr, string basicLink,
            string userIP, string countryCode, string langCode,
            string deviceName, string couponCode)
        {
            Dictionary<string, string> res = new Dictionary<string, string>(8);

            res.Add(CDNTokenizers.Constants.SITE_GUID, sSiteGuid);
            res.Add(CDNTokenizers.Constants.MEDIA_FILE_ID, mediaFileIDStr);
            res.Add(CDNTokenizers.Constants.URL, basicLink);
            res.Add(CDNTokenizers.Constants.IP, userIP);
            res.Add(CDNTokenizers.Constants.COUNTRY_CODE, countryCode);
            res.Add(CDNTokenizers.Constants.LANGUAGE_CODE, langCode);
            res.Add(CDNTokenizers.Constants.DEVICE_NAME, deviceName);
            res.Add(CDNTokenizers.Constants.COUPON_CODE, couponCode);

            return res;
        }

        public static bool IsItemPurchased(MediaFileItemPricesContainer price)
        {
            bool res = false;
            if (price == null || price.m_oItemPrices == null || price.m_oItemPrices.Length == 0)
            {
                return res;
            }
            PriceReason reason = price.m_oItemPrices[0].m_PriceReason;
            switch (reason)
            {
                case PriceReason.SubscriptionPurchased:
                case PriceReason.PrePaidPurchased:
                case PriceReason.CollectionPurchased:
                case PriceReason.PPVPurchased:
                    res = price.m_oItemPrices[0].m_oPrice.m_dPrice == 0d;
                    break;
                default:
                    break;

            }

            return res;
        }

        public static bool IsItemPurchased(MediaFileItemPricesContainer price, ref PriceReason reason)
        {
            bool res = false;
            if (price == null || price.m_oItemPrices == null || price.m_oItemPrices.Length == 0)
            {
                return res;
            }
            reason = price.m_oItemPrices[0].m_PriceReason;
            switch (reason)
            {
                case PriceReason.SubscriptionPurchased:
                case PriceReason.PrePaidPurchased:
                case PriceReason.CollectionPurchased:
                case PriceReason.PPVPurchased:
                    res = price.m_oItemPrices[0].m_oPrice.m_dPrice == 0d;
                    break;
                default:
                    break;

            }

            return res;
        }

        public static void GetDataFromCustomData(int customDataId, string customData, ref double customDataPrice, ref string customDataCurrency, ref string userIP, ref string coupon, ref string udid)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(customData);
                XmlNode theRequest = doc.FirstChild;

                customDataCurrency = XmlUtils.GetSafeValue(BaseConditionalAccess.CURRENCY, ref theRequest);
                userIP = XmlUtils.GetSafeValue(BaseConditionalAccess.USER_IP, ref theRequest);
                coupon = XmlUtils.GetSafeValue(BaseConditionalAccess.COUPON_CODE, ref theRequest);
                udid = XmlUtils.GetSafeValue(BaseConditionalAccess.DEVICE_NAME, ref theRequest);
                if (!Double.TryParse(XmlUtils.GetSafeValue(BaseConditionalAccess.PRICE, ref theRequest), out customDataPrice))
                {
                    customDataPrice = 0.0;
                }

            }
            catch (Exception exc)
            {
                log.ErrorFormat("SetEntitlement - error load custom data xml {0} Exception:{1}", customDataId, exc);
                throw exc;
            }
        }


        public static ApiObjects.Response.Status HandleNPVRQuota(int groupId, Subscription subscription, long householdId, bool isCreate)
        {
            ApiObjects.Response.Status status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            NpvrServiceObject npvrObject = (NpvrServiceObject)subscription.m_lServices.Where(x => x.ID == (int)eService.NPVR).FirstOrDefault();

            INPVRProvider npvr;
            if (NPVRProviderFactory.Instance().IsGroupHaveNPVRImpl(groupId, out npvr, null) && npvr.SynchronizeNpvrWithDomain)
            {
                log.DebugFormat("Subscription with NPVR service, Quota: {0}, Create={1}", npvrObject.Quota, isCreate);
                NPVRUserActionResponse userActionResponse = new NPVRUserActionResponse();
                try
                {
                    if (isCreate)
                    {
                        userActionResponse = npvr.CreateAccount(new NPVRParamsObj() { EntityID = householdId.ToString(), Quota = npvrObject.Quota /*in minutes*/});
                    }
                    else
                    {
                        userActionResponse = npvr.UpdateAccount(new NPVRParamsObj() { EntityID = householdId.ToString(), Quota = npvrObject.Quota /*in minutes*/});
                    }

                    if (userActionResponse != null)
                    {
                        status = new ApiObjects.Response.Status(userActionResponse.isOK ? (int)eResponseStatus.OK : (int)eResponseStatus.Error, userActionResponse.msg);
                    }
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("fail to get response from Npvr provider groupId={0}, householdId={1}, npvrObject.Quota={2}", groupId, householdId, npvrObject.Quota);
                }
            }
            else // Kaltura Recordings
            {
                log.DebugFormat("Kaltura Recordings Quota: {0}, Create={1}", npvrObject.Quota, isCreate);
                if (isCreate)
                {
                    status = QuotaManager.Instance.SetDomainTotalQuota(groupId, householdId, npvrObject.Quota * 60 /*in seconds*/);
                    log.DebugFormat("after SetDomainTotalQuota groupId={0}, householdId={1}, Quota={2}", groupId, householdId, npvrObject.Quota * 60);
                }
                else
                {
                    // get current user quota
                    DomainQuotaResponse hhQuota = QuotaManager.Instance.GetDomainQuotaResponse(groupId, householdId);
                    status = QuotaManager.Instance.SetDomainTotalQuota(groupId, householdId, npvrObject.Quota * 60 /*in seconds*/);
                    if (status.Code == (int)eResponseStatus.OK)
                    {
                        if (hhQuota != null && hhQuota.Status.Code == (int)eResponseStatus.OK)
                        {
                            var shouldInvalidate = false;
                            int usedQuota = hhQuota.Used;// hhQuota.TotalQuota - hhQuota.AvailableQuota; // get used quota

                            if (usedQuota > npvrObject.Quota * 60)
                            {
                                // call the handel to delete all recordings
                                var deleted = QuotaManager.Instance.HandleDomainAutoDelete(groupId, householdId, (int)(usedQuota - npvrObject.Quota * 60), DomainRecordingStatus.DeletePending);
                                shouldInvalidate = deleted?.Count > 0;
                            }
                            else if (usedQuota >= 0 && usedQuota <= npvrObject.Quota * 60) // recover recording from auto-delete by grace period recovery 
                            {
                                status = QuotaManager.Instance.HandleDomainRecoveringRecording(groupId, householdId, (int)(npvrObject.Quota * 60 - usedQuota));
                                shouldInvalidate = status.IsOkStatusCode();
                            }

                            if (shouldInvalidate)
                            {
                                LayeredCache.Instance.SetInvalidationKey(LayeredCacheKeys.GetDomainRecordingsInvalidationKeys(groupId, householdId));
                            }
                        }
                    }
                    log.DebugFormat("after SetDomainTotalQuota groupId={0}, householdId={1}, Quota={2}", groupId, householdId, npvrObject.Quota * 60);
                }
            }

            return status;
        }       

        internal static ApiObjects.Country GetCountryByCountryName(int groupId, string countryName)
        {
            ApiObjects.Country res = null;
            try
            {
                res = Core.Api.Module.GetCountryByCountryName(groupId, countryName);
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed Utils.GetCountryByCountryName with groupId: {0}, countryName: {1}", groupId, countryName), ex);
            }

            return res;
        }

        internal static string GetIP2CountryName(int groupId, string ip)
        {
            string res = string.Empty;
            try
            {
                ApiObjects.Country country = APILogic.Utils.GetCountryByIp(groupId, ip);
                res = country != null ? country.Name : res;
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed Utils.GetIP2CountryName with groupId: {0}, ip: {1}", groupId, ip), ex);
            }

            return res;
        }       

        internal static int GetIP2CountryId(int groupId, string ip)
        {
            int res = 0;
            try
            {
                ApiObjects.Country country = APILogic.Utils.GetCountryByIp(groupId, ip);
                res = country != null ? country.Id : res;
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed Utils.GetIP2CountryId with groupId: {0}, ip: {1}", groupId, ip), ex);
            }

            return res;
        }

        internal static string GetCountryCodeByCountryName(int groupId, string countryName)
        {
            string res = string.Empty;
            try
            {
                ApiObjects.Country country = GetCountryByCountryName(groupId, countryName);
                res = country != null ? country.Code : res;
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed Utils.GetCountryCodeByCountryName with groupId: {0}, countryName: {1}", groupId, countryName), ex);
            }

            return res;
        }

        internal static bool TryGetDomainEntitlementsFromCache(int groupId, int domainId, MeidaMaper[] mapperMapperList, ref DomainEntitlements domainEntitlements)
        {
            bool result = false;
            DomainEntitlementsCache entitlementsFromCache = null;
            try
            {
                string key = LayeredCacheKeys.GetDomainEntitlementsKey(groupId, domainId);

                // if mapper is null init it to empty for passing validation in InitializeDomainEntitlements
                if (mapperMapperList == null)
                {
                    mapperMapperList = new MeidaMaper[0];
                }

                if (domainId == 0)
                {
                    entitlementsFromCache = new DomainEntitlementsCache();
                    result = true;
                }
                else
                {
                    Dictionary<string, object> funcParams = new Dictionary<string, object>() { { "groupId", groupId }, { "domainId", domainId } };
                    result = LayeredCache.Instance.Get<DomainEntitlementsCache>(key, ref entitlementsFromCache, InitializeDomainEntitlements, funcParams, groupId,
                                                                        LayeredCacheConfigNames.GET_DOMAIN_ENTITLEMENTS_LAYERED_CACHE_CONFIG_NAME,
                                                                        new List<string>() { LayeredCacheKeys.GetDomainEntitlementInvalidationKey(groupId, domainId) });
                }
                
                if (result && entitlementsFromCache != null)
                {
                    domainEntitlements = new DomainEntitlements(entitlementsFromCache);
                    // remove expired PPV's
                    if (domainEntitlements.DomainPpvEntitlements != null && domainEntitlements.DomainPpvEntitlements.EntitlementsDictionary != null)
                    {
                        List<string> keysToRemove = new List<string>();
                        foreach (KeyValuePair<string, EntitlementObject> pair in domainEntitlements.DomainPpvEntitlements.EntitlementsDictionary)
                        {
                            if (pair.Value.endDate.HasValue && pair.Value.endDate.Value <= DateTime.UtcNow)
                            {
                                keysToRemove.Add(pair.Key);
                            }
                        }

                        foreach (string keyToRemove in keysToRemove)
                        {
                            domainEntitlements.DomainPpvEntitlements.EntitlementsDictionary.Remove(keyToRemove);
                        }
                    }

                    // remove expired pago's
                    if (domainEntitlements.PagoEntitlements != null && domainEntitlements.PagoEntitlements.Any())
                    {
                        List<long> keysToRemove = new List<long>();
                        foreach (KeyValuePair<long, PagoEntitlement> pair in domainEntitlements.PagoEntitlements)
                        {
                            if (pair.Value.EndDate.HasValue && pair.Value.EndDate.Value <= DateTime.UtcNow)
                            {
                                keysToRemove.Add(pair.Key);
                            }
                        }

                        foreach (long keyToRemove in keysToRemove)
                        {
                            domainEntitlements.PagoEntitlements.Remove(keyToRemove);
                        }
                    }

                    // Get mappings of mediaFileIDs - MediaIDs
                    if (mapperMapperList != null && mapperMapperList.Length > 0)
                    {
                        HashSet<int> mediaIdsToMap = new HashSet<int>();
                        Dictionary<string, List<string>> invalidationKeysMap = new Dictionary<string, List<string>>();
                        Dictionary<string, string> keyToOriginalValueMap = new Dictionary<string, string>();
                        foreach (MeidaMaper mediaMapper in mapperMapperList)
                        {
                            if (!mediaIdsToMap.Contains(mediaMapper.m_nMediaID))
                            {
                                mediaIdsToMap.Add(mediaMapper.m_nMediaID);
                                invalidationKeysMap.Add(DAL.UtilsDal.MediaIdGroupFileTypesKey(mediaMapper.m_nMediaID), new List<string>() { LayeredCacheKeys.GetMediaInvalidationKey(groupId, mediaMapper.m_nMediaID) });
                                keyToOriginalValueMap.Add(DAL.UtilsDal.MediaIdGroupFileTypesKey(mediaMapper.m_nMediaID), mediaMapper.m_nMediaID.ToString());
                            }
                        }

                        Dictionary<string, Dictionary<string, List<int>>> mediaIdGroupFileTypeMapper = null;
                        bool cacheResult = LayeredCache.Instance.GetValues<Dictionary<string, List<int>>>(keyToOriginalValueMap, ref mediaIdGroupFileTypeMapper, Get_AllMediaIdGroupFileTypesMappings,
                                                                                                    new Dictionary<string, object>() { { "mediaIds", mediaIdsToMap } },
                                                                                                    groupId, LayeredCacheConfigNames.GET_MEDIA_ID_GROUP_FILE_MAPPER_LAYERED_CACHE_CONFIG_NAME,
                                                                                                    invalidationKeysMap);
                        if (!cacheResult)
                        {
                            log.Error(string.Format("InitializeUsersEntitlements fail get mediaId group file types mappings from cache keys: {0}", string.Join(",", keyToOriginalValueMap.Keys)));
                        }

                        Dictionary<string, List<int>> mapping = new Dictionary<string, List<int>>();

                        // combine all the results (all dictionaries that return to ONE dictionary)
                        foreach (Dictionary<string, List<int>> val in mediaIdGroupFileTypeMapper.Values)
                        {
                            foreach (KeyValuePair<string, List<int>> item in val)
                            {
                                if (!mapping.ContainsKey(item.Key))
                                {
                                    mapping.Add(item.Key, item.Value);
                                }
                            }
                        }

                        domainEntitlements.DomainPpvEntitlements.MediaIdGroupFileTypeMapper = mapping;
                        domainEntitlements.DomainPpvEntitlements.MediaIdToMediaFiles = new Dictionary<int, HashSet<int>>();
                        foreach (KeyValuePair<string, List<int>> pair in domainEntitlements.DomainPpvEntitlements.MediaIdGroupFileTypeMapper)
                        {
                            string[] keys = pair.Key.Split('_');
                            int mediaIdInKey = 0;
                            if (keys != null && keys.Length > 0 && int.TryParse(keys[0], out mediaIdInKey) && mediaIdInKey > 0)
                            {
                                if (domainEntitlements.DomainPpvEntitlements.MediaIdToMediaFiles.ContainsKey(mediaIdInKey))
                                {
                                    foreach (int mediaFile in pair.Value)
                                    {
                                        if (!domainEntitlements.DomainPpvEntitlements.MediaIdToMediaFiles[mediaIdInKey].Contains(mediaFile))
                                        {
                                            domainEntitlements.DomainPpvEntitlements.MediaIdToMediaFiles[mediaIdInKey].Add(mediaFile);
                                        }
                                    }

                                }
                                else
                                {
                                    domainEntitlements.DomainPpvEntitlements.MediaIdToMediaFiles.Add(mediaIdInKey, new HashSet<int>(pair.Value));
                                }
                            }
                        }
                    }

                    // remove expired Bundles
                    if (domainEntitlements.DomainBundleEntitlements != null)
                    {
                        // remove expired subscriptions
                        if (domainEntitlements.DomainBundleEntitlements.EntitledSubscriptions != null)
                        {
                            List<string> keysToRemove = new List<string>();
                            foreach (KeyValuePair<string, UserBundlePurchase> pair in domainEntitlements.DomainBundleEntitlements.EntitledSubscriptions)
                            {
                                if (pair.Value.dtEndDate != null && pair.Value.dtEndDate <= DateTime.UtcNow)
                                {
                                    keysToRemove.Add(pair.Key);
                                }
                            }

                            foreach (string keyToRemove in keysToRemove)
                            {
                                domainEntitlements.DomainBundleEntitlements.EntitledSubscriptions.Remove(keyToRemove);
                            }
                        }

                        // remove expired collections
                        if (domainEntitlements.DomainBundleEntitlements.EntitledCollections != null)
                        {
                            List<string> keysToRemove = new List<string>();
                            foreach (KeyValuePair<string, UserBundlePurchase> pair in domainEntitlements.DomainBundleEntitlements.EntitledCollections)
                            {
                                if (pair.Value.dtEndDate != null && pair.Value.dtEndDate <= DateTime.UtcNow)
                                {
                                    keysToRemove.Add(pair.Key);
                                }
                            }

                            foreach (string keyToRemove in keysToRemove)
                            {
                                domainEntitlements.DomainBundleEntitlements.EntitledCollections.Remove(keyToRemove);
                            }
                        }

                        PopulateDomainBundles(domainId, groupId, domainEntitlements.DomainBundleEntitlements);
                    }
                }
            }

            catch (Exception ex)
            {
                log.Error(string.Format("Failed TryGetDomainEntitlementsFromCache, groupId: {0}, domainId: {1}", groupId, domainId), ex);
            }

            return result && domainEntitlements != null;
        }

        private static Tuple<DomainEntitlementsCache, bool> InitializeDomainEntitlements(Dictionary<string, object> funcParams)
        {
            DomainEntitlementsCache domainEntitlements = null;
            try
            {
                if (funcParams != null && funcParams.Count == 2 && funcParams.ContainsKey("groupId") && funcParams.ContainsKey("domainId"))
                {
                    int? groupId = funcParams["groupId"] as int?, domainId = funcParams["domainId"] as int?;
                    if (groupId.HasValue && domainId.HasValue)
                    {
                        List<int> usersInDomain = Utils.GetAllUsersInDomain(groupId.Value, domainId.Value);
                        domainEntitlements = new DomainEntitlementsCache();
                        //Get domain PPV entitlements
                        domainEntitlements.DomainPpvEntitlements = InitializeDomainPpvs(groupId.Value, domainId.Value, usersInDomain);
                        //Get domain bundle entitlements
                        domainEntitlements.DomainBundleEntitlements = InitializeDomainBundles(domainId.Value, groupId.Value, usersInDomain);
                        //Get domain PAGO entitlements
                        domainEntitlements.PagoEntitlements = InitializeDomainPagos(groupId.Value, domainId.Value, usersInDomain);
                    }
                }
            }

            catch (Exception ex)
            {
                log.Error(string.Format("InitializeDomainEntitlements failed, parameters : {0}", string.Join(";", funcParams.Keys)), ex);
            }

            bool res = domainEntitlements != null;

            return new Tuple<DomainEntitlementsCache, bool>(domainEntitlements, res);
        }

        internal static bool TryGetFileUrlLinks(int groupId, int mediaFileID, ref string mainUrl, ref string altUrl, ref int mainStreamingCoID,
                                         ref int altStreamingCoID, ref int mediaID, ref string fileCoGuid)

        {
            bool res = false;

            string key = LayeredCacheKeys.GetFileCdnDataKey(mediaFileID);
            DataTable dt = null;
            // try to get from cache            
            bool cacheResult = LayeredCache.Instance.Get<DataTable>(key,
                                                                    ref dt,
                                                                    Utils.GetFileUrlLinks,
                                                                    new Dictionary<string, object>() { { "mediaFileId", mediaFileID } },
                                                                    groupId,
                                                                    LayeredCacheConfigNames.FILE_CDN_DATA_LAYERED_CACHE_CONFIG_NAME);
            if (cacheResult && dt != null && dt.Rows != null && dt.Rows.Count > 0)
            {
                DataRow dr = dt.Rows[0];

                mainUrl = ODBCWrapper.Utils.GetSafeStr(dr, "mainUrl");
                altUrl = ODBCWrapper.Utils.GetSafeStr(dr, "altUrl");
                mainStreamingCoID = ODBCWrapper.Utils.GetIntSafeVal(dr, "CdnID");
                altStreamingCoID = ODBCWrapper.Utils.GetIntSafeVal(dr, "AltCdnID");
                mediaID = ODBCWrapper.Utils.GetIntSafeVal(dr, "media_id");
                fileCoGuid = ODBCWrapper.Utils.GetSafeStr(dr, "CO_GUID");
                res = true;
            }
            return res;
        }


        private static Tuple<DataTable, bool> GetFileUrlLinks(Dictionary<string, object> funcParams)
        {
            bool res = false;
            DataTable dt = null;

            try
            {
                if (funcParams != null && funcParams.Count == 1)
                {
                    if (funcParams.ContainsKey("mediaFileId"))
                    {
                        int? mediaFileId;
                        mediaFileId = funcParams["mediaFileId"] as int?;

                        if (mediaFileId.HasValue)
                        {
                            dt = ConditionalAccessDAL.GetFileCdnData(mediaFileId.Value);
                            res = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("GetFileUrlLinks failed, parameters : {0}", string.Join(";", funcParams.Keys)), ex);
            }
            return new Tuple<DataTable, bool>(dt, res);
        }

        internal static Tuple<List<MediaFile>, bool> GetMediaFiles(Dictionary<string, object> funcParams)
        {
            bool res = false;
            List<MediaFile> mediaFiles = null;

            try
            {
                if (funcParams != null && funcParams.Count == 3)
                {
                    if (funcParams.ContainsKey("mediaId") && funcParams.ContainsKey("groupId") && funcParams.ContainsKey("assetType"))
                    {
                        long? mediaId = funcParams["mediaId"] as long?;
                        int? groupId = funcParams["groupId"] as int?;
                        eAssetTypes? assetType = funcParams["assetType"] as eAssetTypes?;

                        if (mediaId.HasValue && groupId.HasValue && assetType.HasValue)
                        {
                            mediaFiles = ApiDAL.GetMediaFiles(groupId.Value, mediaId.Value);
                            if (mediaFiles != null)
                            {
                                foreach (MediaFile mediaFile in mediaFiles)
                                {
                                    mediaFile.Url = GetAssetUrl(groupId.Value, assetType.Value, mediaFile.Url, mediaFile.CdnId);
                                    mediaFile.AltUrl = GetAssetUrl(groupId.Value, assetType.Value, mediaFile.AltUrl, mediaFile.AltCdnId);
                                }
                            }
                            res = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("GetMediaFiles failed, parameters : {0}", string.Join(";", funcParams.Keys)), ex);
            }
            return new Tuple<List<MediaFile>, bool>(mediaFiles, res);
        }

        internal static List<int> GetRelatedMediaFiles(ItemPriceContainer price, int mediaFileID)
        {
            List<int> lRelatedMediaFiles = new List<int>();

            if (price != null && price.m_lRelatedMediaFileIDs != null && price.m_lRelatedMediaFileIDs.Length > 0)
            {
                lRelatedMediaFiles.AddRange(price.m_lRelatedMediaFileIDs.ToList());
            }
            if (!lRelatedMediaFiles.Contains(mediaFileID))
            {
                lRelatedMediaFiles.Add(mediaFileID);
            }
            return lRelatedMediaFiles;
        }

        internal static DateTime? GetStartDate(ItemPriceContainer price)
        {
            DateTime? dtStartDate = null;

            if (price != null)
            {
                dtStartDate = price.m_dtStartDate;
            }

            return (dtStartDate);
        }

        internal static DateTime? GetEndDate(ItemPriceContainer price)
        {
            DateTime? dtEndDate = null;

            if (price != null)
            {
                dtEndDate = price.m_dtEndDate;
            }

            return (dtEndDate);
        }

        internal static void InsertOfflinePpvUse(int groupId, int mediaFileId, string productCode, string userId, string countryCode, string languageCode, string udid, int nRelPP, int releventCollectionID, LogContextData context)
        {
            try
            {
                context.Load();
                // We write an empty string as the first parameter to split the start of the log from the offlinePpvUsesLog row data
                string infoToLog = string.Join(",", new object[] { " ", groupId, mediaFileId, productCode, userId, countryCode, languageCode, udid, nRelPP, releventCollectionID });
                offlinePpvLogger.Info(infoToLog);
            }
            catch (Exception ex)
            {
                log.Error(string.Format(@"Error in InsertOfflinePpvUse, groupId: {0}, mediaFileId: {1}, productCode: {2}, userId: {3}, countryCode: {4}, languageCode: {5}, udid: {6}, nRelPP: {7},
                                            releventCollectionID: {8}", groupId, mediaFileId, productCode, userId, countryCode, languageCode, udid, nRelPP, releventCollectionID), ex);
            }
        }

        internal static void InsertOfflineSubscriptionUse(int groupId, int mediaFileId, string productCode, string userId, string countryCode, string languageCode, string udid, int nRelPP, LogContextData context)
        {
            try
            {
                context.Load();
                // We write an empty string as the first parameter to split the start of the log from the offlineSubscriptionUsesLog row data
                string infoToLog = string.Join(",", new object[] { " ", groupId, mediaFileId, productCode, userId, countryCode, languageCode, udid, nRelPP });
                offlineSubscriptionLogger.Info(infoToLog);
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Error in InsertOfflineSubscriptionUse, groupId: {0}, mediaFileId: {1}, productCode: {2}, userId: {3}, countryCode: {4}, languageCode: {5}, udid: {6}, nRelPP: {7}",
                                        groupId, mediaFileId, productCode, userId, countryCode, languageCode, udid, nRelPP), ex);
            }
        }

        private static Tuple<int, bool> Get_MediaFileIDByCoGuid(Dictionary<string, object> funcParams)
        {
            bool res = false;
            int mediaFileID = 0;
            Dictionary<string, int> result = new Dictionary<string, int>();
            try
            {
                int? groupID = 0;
                string coGuid = string.Empty;
                if (funcParams.ContainsKey("groupID"))
                {
                    groupID = funcParams["groupID"] as int?;
                }
                if (funcParams.ContainsKey("coGuid"))
                {
                    coGuid = funcParams["coGuid"] as string;
                }
                if (groupID > 0 && !string.IsNullOrEmpty(coGuid))
                {
                    res = ConditionalAccessDAL.Get_MediaFileIDByCoGuid(coGuid, groupID.Value, ref mediaFileID);
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Get_MediaFileIDByCoGuid faild params : {0}", string.Join(";", funcParams.Keys)), ex);
            }
            return new Tuple<int, bool>(mediaFileID, res);
        }

        private static Tuple<Dictionary<string, Dictionary<string, List<int>>>, bool> Get_AllMediaIdGroupFileTypesMappings(Dictionary<string, object> funcParams)
        {
            bool res = false;
            Dictionary<string, Dictionary<string, List<int>>> result = new Dictionary<string, Dictionary<string, List<int>>>();
            try
            {
                HashSet<int> mediaIds = null;
                if (funcParams.ContainsKey(LayeredCache.MISSING_KEYS) && funcParams[LayeredCache.MISSING_KEYS] != null)
                {
                    List<int> ids = ((List<string>)funcParams[LayeredCache.MISSING_KEYS]).Select(x => int.Parse(x)).ToList();
                    mediaIds = new HashSet<int>(ids);
                }
                else if (funcParams.ContainsKey("mediaIds"))
                {
                    mediaIds = funcParams["mediaIds"] != null ? funcParams["mediaIds"] as HashSet<int> : null;
                }
                if (mediaIds != null)
                {
                    result = ConditionalAccessDAL.Get_AllMediaIdGroupFileTypesMappings(mediaIds);
                    res = true;
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Get_FileAndMediaBasicDetails faild params : {0}", string.Join(";", funcParams.Keys)), ex);
            }

            return new Tuple<Dictionary<string, Dictionary<string, List<int>>>, bool>(result, res);
        }

        internal static ApiObjects.Response.Status ValidatePPVModuleCode(int groupId, int productId, int contentId, ref PPVModule thePPVModule)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status();

            try
            {
                long ppvModuleCode = 0;
                long.TryParse(productId.ToString(), out ppvModuleCode);

                thePPVModule = Pricing.Module.ValidatePPVModuleForMediaFile(groupId, contentId, ppvModuleCode);

                if (thePPVModule == null)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.UnKnownPPVModule, "The ppv module is unknown");
                    return response;
                }

                if (!thePPVModule.m_sObjectCode.Equals(productId.ToString()))
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.UnKnownPPVModule, "This PPVModule does not belong to item");
                    return response;
                }

                response = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                return response;
            }
            catch (Exception ex)
            {
                log.Error("ValidateModuleCode  ", ex);
                response = new ApiObjects.Response.Status((int)eResponseStatus.Error, "error ValidateModuleCode");
                return response;
            }
        }

        internal static DateTime CalcSubscriptionEndDate(Subscription sub, bool bIsEntitledToPreviewModule, DateTime dtToInitializeWith)//, int domainId = 0)
        {
            DateTime res = dtToInitializeWith;
            if (sub != null)
            {
                if (bIsEntitledToPreviewModule && sub.m_oPreviewModule != null && sub.m_oPreviewModule.m_tsFullLifeCycle > 0)
                {
                    // calc end date according to preview module life cycle
                    var previewDuration = new Duration(sub.m_oPreviewModule.m_tsFullLifeCycle);
                    res = Utils.GetEndDateTime(previewDuration, res);
                }
                else
                {
                    if (sub.m_oSubscriptionUsageModule != null)
                    {
                        // calc end date as before.
                        var subDuration = new Duration(sub.m_oSubscriptionUsageModule.m_tsMaxUsageModuleLifeCycle);
                        res = Utils.GetEndDateTime(subDuration, res);
                    }
                }

                //BEO-10088
                if (res > sub.m_dEndDate)
                {
                    res = sub.m_dEndDate;
                }
            }

            return res;
        }

        internal static void GetFreeItemLeftLifeCycle(int groupId, ref string p_strViewLifeCycle, ref string p_strFullLifeCycle, DateTime? endDate = null)
        {
            // Default is 2 days
            TimeSpan freeTimeSpan = new TimeSpan(2, 0, 0, 0);
            DateTime now = DateTime.UtcNow;

            // Get the group's configuration for free view life cycle
            string freeLeftView = TVinciShared.WS_Utils.GetTcmConfigValue(string.Format("free_left_view_{0}", groupId));
            if (!string.IsNullOrEmpty(freeLeftView))
            {
                DateTime tcmEndDate = Utils.GetEndDateTime(now, int.Parse(freeLeftView), true);
                freeTimeSpan = tcmEndDate.Subtract(now);
            }

            if (endDate.HasValue)
            {
                var ts = endDate.Value.Subtract(now);
                if (ts < freeTimeSpan)
                {
                    freeTimeSpan = ts;
                }
            }

            if (freeTimeSpan.TotalSeconds < 0)
            {
                freeTimeSpan = new TimeSpan();
            }

            p_strViewLifeCycle = freeTimeSpan.ToString();
            // TODO: Understand what to do with full life cycle of free item. Right now I write it the same as view
            p_strFullLifeCycle = freeTimeSpan.ToString();
        }

        public static bool IsFreeItem(MediaFileItemPricesContainer container)
        {
            return container != null && (container.m_oItemPrices == null || container.m_oItemPrices.Length == 0 || container.m_oItemPrices[0].m_PriceReason == PriceReason.Free);
        }

        internal static eTransactionType GetBusinessModuleType(string moduleCode)
        {
            if (!string.IsNullOrEmpty(moduleCode))
            {
                if (moduleCode.Contains("s:"))
                    return eTransactionType.Subscription;
                if (moduleCode.Contains("b:"))
                    return eTransactionType.Collection;
            }
            return eTransactionType.PPV;
        }

        internal static string GetDeviceName(string deviceUDID)
        {
            return DAL.ConditionalAccessDAL.GetDeviceName(deviceUDID);
        }

        /// <summary>
        /// Get Billing Trans Method
        /// </summary>
        internal static ePaymentMethod GetBillingTransMethod(int billingTransID, string billingGuid)
        {
            ePaymentMethod paymentMethod = ePaymentMethod.Unknown;
            var data = GetBillingTransactionData(billingTransID, billingGuid);

            if (data != null)
            {
                paymentMethod = data.Item1;
            }

            return paymentMethod;
        }

        internal static Tuple<ePaymentMethod, string> GetBillingTransactionData(int billingTransID, string billingGuid)
        {
            ePaymentMethod paymentMethod = ePaymentMethod.Unknown;
            string customdata = null;

            if (billingTransID > 0 || !string.IsNullOrEmpty(billingGuid))
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = null;
                try
                {
                    selectQuery = new ODBCWrapper.DataSetSelectQuery();
                    selectQuery.SetConnectionKey("MAIN_CONNECTION_STRING");

                    selectQuery += " select BILLING_METHOD, billing_provider, CUSTOMDATA from billing_transactions with (nolock) where status=1  and ";
                    if (billingTransID > 0)
                    {
                        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", billingTransID);
                    }
                    else
                    {
                        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("billing_guid", "=", billingGuid);
                    }

                    DataTable dt = selectQuery.Execute("query", true);
                    if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                    {
                        customdata = ODBCWrapper.Utils.GetSafeStr(dt.Rows[0], "CUSTOMDATA");
                        int billingInt = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0], "BILLING_METHOD");
                        int billingProvider = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0], "billing_provider");
                        if (billingProvider == 1000)
                        {
                            paymentMethod = ePaymentMethod.Unknown;
                        }
                        else if (billingInt > 0)
                        {
                            if (Enum.IsDefined(typeof(ePaymentMethod), ((ePaymentMethod)billingInt).ToString()))
                                paymentMethod = (ePaymentMethod)billingInt;
                        }
                    }
                    else
                    {
                        paymentMethod = ePaymentMethod.Gift;
                    }
                }

                finally
                {
                    if (selectQuery != null)
                    {
                        selectQuery.Finish();
                    }
                }
            }

            return new Tuple<ePaymentMethod, string>(paymentMethod, customdata); ;
        }

        internal static bool IsGroupIDContainedInConfig(long lGroupID, string rawStrFromConfig, char cSeperator)
        {
            bool res = false;
            if (rawStrFromConfig.Length > 0)
            {
                string[] strArrOfIDs = rawStrFromConfig.Split(cSeperator);
                if (strArrOfIDs != null && strArrOfIDs.Length > 0)
                {
                    List<long> listOfIDs = strArrOfIDs.Select(s =>
                    {
                        long l = 0;
                        if (Int64.TryParse(s, out l))
                            return l;
                        return 0;
                    }).ToList();

                    res = listOfIDs.Contains(lGroupID);
                }
            }

            return res;
        }

        internal static ApiObjects.Response.Status ConcurrencyResponseToResponseStatus(DomainResponseStatus mediaConcurrencyResponse)
        {
            ApiObjects.Response.Status status;

            switch (mediaConcurrencyResponse)
            {
                case DomainResponseStatus.LimitationPeriod:
                    status = new ApiObjects.Response.Status((int)eResponseStatus.LimitationPeriod, "Limitation period");
                    break;
                case DomainResponseStatus.Error:
                    status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                    break;
                case DomainResponseStatus.ExceededLimit:
                    status = new ApiObjects.Response.Status((int)eResponseStatus.ExceededLimit, "Exceeded limit");
                    break;
                case DomainResponseStatus.DeviceTypeNotAllowed:
                    status = new ApiObjects.Response.Status((int)eResponseStatus.DeviceTypeNotAllowed, "Device type not allowed");
                    break;
                case DomainResponseStatus.DeviceNotInDomain:
                    status = new ApiObjects.Response.Status((int)eResponseStatus.DeviceNotInDomain, "Device not in household");
                    break;
                case DomainResponseStatus.DeviceAlreadyExists:
                    status = new ApiObjects.Response.Status((int)eResponseStatus.DeviceAlreadyExists, "Device already exists");
                    break;
                case DomainResponseStatus.OK:
                    status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                    break;
                case DomainResponseStatus.DeviceExistsInOtherDomains:
                    status = new ApiObjects.Response.Status((int)eResponseStatus.DeviceExistsInOtherDomains, "Device exists in other household");
                    break;
                case DomainResponseStatus.ConcurrencyLimitation:
                    status = new ApiObjects.Response.Status((int)eResponseStatus.ConcurrencyLimitation, "Concurrency limitation");
                    break;
                case DomainResponseStatus.MediaConcurrencyLimitation:
                    status = new ApiObjects.Response.Status((int)eResponseStatus.MediaConcurrencyLimitation, "Media concurrency limitation");
                    break;
                default:
                    status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                    break;
            }

            return status;
        }

        public static Subscription GetSubscription(int groupId, int subscriptionId, string userId = null)
        {
            Subscription subscription = null;

            try
            {
                subscription = Core.Pricing.Module.Instance.GetSubscriptionData(groupId, subscriptionId.ToString(), string.Empty, string.Empty, string.Empty, false, userId);
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Error while trying to fetch subscription data. subscriptionId = {0}", subscriptionId), ex);
            }

            return subscription;
        }

        public static Dictionary<long, Recording> GetDomainRecordingsToRecover(int groupId, long domainId)
        {
            Dictionary<long, Recording> DomainRecordingIdToRecordingMap = null;
            try
            {
                Recording DomainRecording = new Recording();
                DataTable dt = RecordingsDAL.GetDomainRecordingsByRecordingStatuses(groupId, domainId, new List<int>() { (int)DomainRecordingStatus.DeletePending },
                    new List<int>() { (int)RecordingInternalStatus.OK });

                if (dt != null && dt.Rows != null)
                {
                    // get tstv Settings to get the grace period recovery 
                    var tstvSettings = Utils.GetTimeShiftedTvPartnerSettings(groupId);
                    int recoveryGracePeriod = RECOVERY_GRACE_PERIOD; // default value 10 days 
                    if (tstvSettings != null && tstvSettings.RecoveryGracePeriod.HasValue)
                    {
                        recoveryGracePeriod = tstvSettings.RecoveryGracePeriod.Value;
                    }

                    DomainRecordingIdToRecordingMap = new Dictionary<long, Recording>();
                    foreach (DataRow dr in dt.Rows)
                    {
                        long domainRecordingID = ODBCWrapper.Utils.GetLongSafeVal(dr, "ID");
                        if (domainRecordingID > 0)
                        {
                            Recording domainRecording = BuildDomainRecordingFromDataRow(dr);
                            // add domain recording if its valid and doesn't already exist in dictionary
                            if (DomainRecording != null && domainRecording.Status != null && domainRecording.Status.Code == (int)eResponseStatus.OK
                                && !DomainRecordingIdToRecordingMap.ContainsKey(domainRecordingID))
                            {
                                if (domainRecording.UpdateDate.AddSeconds(recoveryGracePeriod) >= DateTime.UtcNow)
                                {
                                    DomainRecordingIdToRecordingMap.Add(domainRecordingID, domainRecording);
                                }
                            }
                        }
                    }
                    if (DomainRecordingIdToRecordingMap != null && DomainRecordingIdToRecordingMap.Count > 0)
                    {
                        DomainRecordingIdToRecordingMap = DomainRecordingIdToRecordingMap.OrderByDescending(kvp => kvp.Value.EpgStartDate)
                            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("groupId={0}, domainId={1}, ex={2}", groupId, domainId, ex);
            }
            return DomainRecordingIdToRecordingMap;
        }

        internal static List<int> GetMediaFilesByMediaId(int groupId, int mediaId)
        {
            List<int> result = null;
            try
            {
                if (mediaId > 0)
                {
                    result = Api.Module.GetMediaFilesByMediaId(groupId, mediaId);
                }
            }

            catch (Exception ex)
            {
                log.Error(string.Format("GetMediaFilesByMediaId failed for groupId: {0}, mediaId: {1}", groupId, mediaId), ex);
            }

            return result;
        }

        internal static long GetSubscriptiopnPurchaseCoupon(ref string couponCode, long purchaseId, int groupId)
        {
            long couponGroupId = 0;

            if (string.IsNullOrEmpty(couponCode))
            {
                couponCode = GetSubscriptiopnPurchaseCoupon(purchaseId);
            }

            if (!string.IsNullOrEmpty(couponCode))
            {
                couponGroupId = PricingDAL.Get_CouponGroupId(groupId, couponCode);
                log.DebugFormat("GetSubscriptiopnPurchaseCoupon purchaseId={0}, groupId={1}, couponGroupId={2}, couponCode={3}", purchaseId, groupId, couponGroupId, couponCode);
            }

            return couponGroupId;
        }

        public List<ApiObjects.Epg.FieldTypeEntity> GetAliasMappingFields(int groupId)
        {
            List<ApiObjects.Epg.FieldTypeEntity> res = null;

            try
            {
                if (CatalogManager.Instance.DoesGroupUsesTemplates(groupId))
                {
                    return new List<ApiObjects.Epg.FieldTypeEntity>();
                }
                else if (!LayeredCache.Instance.Get<List<ApiObjects.Epg.FieldTypeEntity>>(LayeredCacheKeys.GetAliasMappingFields(groupId), ref res, GetAliasMappingFields,
                        new Dictionary<string, object>() { { "groupId", groupId } }, groupId, LayeredCacheConfigNames.GET_ALIAS_MAPPING_FIELDS_CACHE_CONFIG_NAME,
                        new List<string>() { LayeredCacheKeys.GetAliasMappingFieldsInvalidationKey(groupId) }))
                {
                    log.ErrorFormat("Failed getting alias mapping fields from LayeredCache, groupId: {0}", groupId);
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetAliasMappingFields for groupId: {0}", groupId), ex);
            }

            return res;
        }

        internal static Tuple<List<ApiObjects.Epg.FieldTypeEntity>, bool> GetAliasMappingFields(Dictionary<string, object> funcParams)
        {
            bool res = false;
            List<ApiObjects.Epg.FieldTypeEntity> result = new List<ApiObjects.Epg.FieldTypeEntity>();
            try
            {
                if (funcParams != null && funcParams.ContainsKey("groupId"))
                {
                    int? groupId = funcParams["groupId"] as int?;

                    if (groupId.HasValue)
                    {
                        result = Tvinci.Core.DAL.CatalogDAL.GetAliasMappingFields(groupId.Value);
                        if (result != null)
                        {
                            res = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("GetAliasMappingFields failed with params : {0}", string.Join(";", funcParams.Keys)), ex);
            }

            return new Tuple<List<ApiObjects.Epg.FieldTypeEntity>, bool>(result, res);
        }

        internal static long InsertSubscriptionSetModifyDetails(int groupId, long domainId, long associatedPurchaseId, long scheduledSubscriptionId, SubscriptionSetModifyType type)
        {
            long id = 0;
            try
            {
                id = ConditionalAccessDAL.InsertSubscriptionSetModifyDetails(groupId, domainId, associatedPurchaseId, scheduledSubscriptionId, type);
                if (id <= 0)
                {
                    log.ErrorFormat("Failed ConditionalAccessDAL.InsertScheduledPurchase, groupId: {0}, domaindId: {1}, associatedPurchaseId: {2}, scheduledSubscriptionId: {3}, type: {4}",
                                    groupId, domainId, associatedPurchaseId, scheduledSubscriptionId, type.ToString());
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed InsertScheduledPurchase, groupId: {0}, domaindId: {1}, associatedPurchaseId: {2}, scheduledSubscriptionId: {3}, type: {4}",
                                    groupId, domainId, associatedPurchaseId, scheduledSubscriptionId, type.ToString()), ex);
            }

            return id;
        }

        internal static bool InsertSubscriptionSetDowngradeDetails(SubscriptionSetDowngradeDetails subscriptionSetDowngradeDetails)
        {
            return ConditionalAccessDAL.InsertSubscriptionSetDowngradeDetails(subscriptionSetDowngradeDetails);
        }

        internal static SubscriptionSetDowngradeDetails GetSubscriptionSetDowngradeDetails(int groupId, long id)
        {
            return ConditionalAccessDAL.GetSubscriptionSetDowngradeDetails(groupId, id);
        }

        internal static bool DeleteSubscriptionSetDowngradeDetails(int groupId, long id)
        {
            return ConditionalAccessDAL.DeleteSubscriptionSetDowngradeDetails(groupId, id);
        }

        internal static bool GetSubscriptionSetModifyDetailsByDomainAndSubscriptionId(int groupId, long domainId, long scheduledSubscriptionId, ref long subscriptionSetModifyDetailsId,
                                                                                        ref long purchaseId, SubscriptionSetModifyType type)
        {
            bool res = false;
            try
            {
                DataTable dt = ConditionalAccessDAL.GetSubscriptionSetModifyDetailsByDomainIdAndSubscriptionId(groupId, domainId, scheduledSubscriptionId, type);
                if (dt != null && dt.Rows != null && dt.Rows.Count == 1)
                {
                    subscriptionSetModifyDetailsId = ODBCWrapper.Utils.GetLongSafeVal(dt.Rows[0], "ID", 0);
                    purchaseId = ODBCWrapper.Utils.GetLongSafeVal(dt.Rows[0], "PURCHASE_ID", 0);
                    res = subscriptionSetModifyDetailsId > 0 && purchaseId > 0;
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetSubscriptionSetModifyDetailsByDomainAndSubscriptionId, groupId: {0}, domainId: {1}, scheduledSubscriptionId: {2}",
                                        groupId, domainId, scheduledSubscriptionId), ex);
            }

            return res;
        }

        internal static Dictionary<long, long> GetPurchaseIdToScheduledSubscriptionIdMap(int groupId, int domainId, List<long> purchaseIds, SubscriptionSetModifyType type)
        {
            Dictionary<long, long> result = new Dictionary<long, long>();
            try
            {
                DataTable dt = ConditionalAccessDAL.GetScheduledSubscriptionIdsByPurchaseIdsAndDomainId(groupId, domainId, purchaseIds, type);
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        long purchaseId = ODBCWrapper.Utils.GetLongSafeVal(dt.Rows[0], "PURCHASE_ID", 0);
                        long subscriptionId = ODBCWrapper.Utils.GetLongSafeVal(dt.Rows[0], "SUBSCRIPTION_ID", 0);
                        if (purchaseId > 0 && subscriptionId > 0 && !result.ContainsKey(purchaseId))
                        {
                            result.Add(purchaseId, subscriptionId);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetPurchaseIdToScheduledSubscriptionIdMap, groupId: {0}, purchaseIds: {1}",
                                        groupId, purchaseIds != null && purchaseIds.Count > 0 ? string.Join(",", purchaseIds) : string.Empty), ex);
            }

            return result;
        }

        internal static bool GetPreviousSubscriptionPurchaseDetails(int groupId, long domainId, UserBundlePurchase previousBundlePurchase, SubscriptionSetModifyType type,
                                                                    ref Core.ConditionalAccess.PurchaseManager.DomainSubscriptionPurchaseDetails previousSubsriptionPurchaseDetails)
        {
            bool res = false;
            try
            {
                if (!string.IsNullOrEmpty(previousBundlePurchase.sBundleCode))
                {
                    DataTable dt = ConditionalAccessDAL.GetPreviousSubscriptionPurchaseDetails(groupId, domainId, previousBundlePurchase.sBundleCode, type);
                    if (dt != null && dt.Rows != null && dt.Rows.Count == 1)
                    {
                        long purchaseId = ODBCWrapper.Utils.GetLongSafeVal(dt.Rows[0], "ID", 0);
                        double price = ODBCWrapper.Utils.GetDoubleSafeVal(dt.Rows[0], "PRICE");
                        string currencyCode = ODBCWrapper.Utils.GetSafeStr(dt.Rows[0], "CURRENCY_CD");
                        bool isRecurring = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0], "IS_RECURRING", 0) == 1;
                        string billingGuid = ODBCWrapper.Utils.GetSafeStr(dt.Rows[0], "BILLING_GUID");
                        bool isFirstSubscriptionSetModify = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0], "IS_SUBSCRIPTION_SET_MODIFY_EXISTS", 0) == 0;
                        if (purchaseId > 0)
                        {
                            previousSubsriptionPurchaseDetails = new PurchaseManager.DomainSubscriptionPurchaseDetails(previousBundlePurchase, purchaseId, price, currencyCode, isRecurring, billingGuid, isFirstSubscriptionSetModify);
                            res = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetPreviousSubscriptionPurchaseDetails, groupId: {0}, domainId: {1}, previousSubscriptionId: {2}", groupId, domainId, previousBundlePurchase.sBundleCode), ex);
            }

            return res;
        }



        internal static bool TryGetSubscriptionSets(int groupId, List<long> setIds, ref List<SubscriptionSet> subscriptionSets)
        {
            bool res = false;
            try
            {
                if (setIds == null || setIds.Count == 0)
                {
                    return res;
                }

                Dictionary<string, SubscriptionSet> subscriptionSetMap = null;
                Dictionary<string, string> keyToOriginalValueMap = LayeredCacheKeys.GetSubscriptionSetsKeysMap(groupId, setIds);
                Dictionary<string, List<string>> invalidationKeysMap = LayeredCacheKeys.GetSubscriptionSetsInvalidationKeysMap(groupId, setIds);

                if (!LayeredCache.Instance.GetValues<SubscriptionSet>(keyToOriginalValueMap, ref subscriptionSetMap, GetSubscriptionSets,
                    new Dictionary<string, object>() { { "groupId", groupId }, { "setIds", setIds } },
                    groupId, LayeredCacheConfigNames.GET_SUBSCRIPTION_SETS_CACHE_CONFIG_NAME,
                                                                        invalidationKeysMap))
                {
                    log.ErrorFormat("Failed getting SubscriptionSets from LayeredCache, groupId: {0}, setIds", groupId, string.Join(",", setIds));
                }
                else if (subscriptionSetMap != null)
                {
                    subscriptionSets = subscriptionSetMap.Values.ToList();
                    res = true;
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed TryGetSubscriptionSets for groupId: {0}, setIds: {1}", groupId, string.Join(",", setIds)), ex);
            }
            return res && subscriptionSets != null && subscriptionSets.Count > 0;
        }

        private static Tuple<Dictionary<string, SubscriptionSet>, bool> GetSubscriptionSets(Dictionary<string, object> funcParams)
        {
            bool res = false;
            Dictionary<string, SubscriptionSet> result = new Dictionary<string, SubscriptionSet>();
            try
            {
                if (funcParams != null && funcParams.ContainsKey("setIds") && funcParams.ContainsKey("groupId"))
                {
                    string key = string.Empty;
                    List<long> setIds;
                    int? groupId = funcParams["groupId"] as int?;
                    if (funcParams.ContainsKey(LayeredCache.MISSING_KEYS) && funcParams[LayeredCache.MISSING_KEYS] != null)
                    {
                        setIds = ((List<string>)funcParams[LayeredCache.MISSING_KEYS]).Select(x => long.Parse(x)).ToList();
                    }
                    else
                    {
                        setIds = funcParams["setIds"] != null ? funcParams["setIds"] as List<long> : null;
                    }

                    if (setIds != null && groupId.HasValue)
                    {
                        List<SubscriptionSet> subscriptionSets = Pricing.Utils.GetSubscriptionSets(groupId.Value, setIds);
                        foreach (SubscriptionSet item in subscriptionSets)
                        {
                            result.Add(LayeredCacheKeys.GetSubscriptionSetKey(groupId.Value, item.Id), item);
                        }
                        res = true;

                        //List<long> missingKeys = setIds.Where(x => !(subscriptionSets.Select(s=>s.Id).ToList()).Contains(x)).ToList();
                        //if (missingKeys != null)
                        //{
                        //    SubscriptionSet tempSubscriptionSet = new SwitchSet();
                        //    foreach (int missingKey in missingKeys)
                        //    {
                        //        result.Add(LayeredCacheKeys.GetSubscriptionSetKey(missingKey, groupId.Value), tempSubscriptionSet);
                        //    }
                        //}
                    }
                    //res = result.Keys.Count() == setIds.Count();

                    //result = result.ToDictionary(x => LayeredCacheKeys.GetSubscriptionSetKey(int.Parse(x.Key), groupId.Value), x => x.Value);
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("GetSubscriptionSets failed params : {0}", string.Join(";", funcParams.Keys)), ex);
            }

            return new Tuple<Dictionary<string, SubscriptionSet>, bool>(result, res);
        }

        internal static ApiObjects.Response.Status CanPurchaseAddOn(int groupId, long householdId, Subscription subscription, List<Subscription> baseSubscriptionsInUnified = null,
                                                                    DateTime? endDate = null)
        {
            var status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());

            try
            {
                List<long> setIds = subscription.GetSubscriptionSetIdsToPriority().Select(x => x.Key).ToList();

                // get all base set containing this add on 
                List<SubscriptionSet> subscriptionSets = new List<SubscriptionSet>();
                if (!TryGetSubscriptionSets(groupId, setIds, ref subscriptionSets))
                {
                    status = new ApiObjects.Response.Status((int)eResponseStatus.SubscriptionSetDoesNotExist, "No Sets found");
                    return status;
                }

                if (subscriptionSets != null && subscriptionSets.Count > 0)
                {
                    List<long> subscriptionIds = new List<long>();

                    List<DependencySet> dependencySet = subscriptionSets.Where(x => x.Type == SubscriptionSetType.Dependency).Select(x => (DependencySet)x).ToList();

                    DomainBundles domainBundles = GetDomainBundles(groupId, (int)householdId);
                    if (domainBundles?.EntitledSubscriptions?.Count > 0)
                    {
                        if (endDate.HasValue)
                        {
                            foreach (var item in domainBundles.EntitledSubscriptions.Values)
                            {
                                var bundle = item[0];

                                if (bundle.dtEndDate > endDate.Value.AddSeconds(1) || bundle.isSuspend)
                                {
                                    subscriptionIds.Add(long.Parse(bundle.sBundleCode));
                                }
                            }
                        }
                        else
                        {
                            subscriptionIds = domainBundles.EntitledSubscriptions.Keys.Select(long.Parse).ToList();
                        }
                    }

                    if (subscriptionIds.Count == 0 || dependencySet.Count(x => subscriptionIds.Contains(x.BaseSubscriptionId)) == 0)
                    {
                        status = new ApiObjects.Response.Status((int)eResponseStatus.MissingBasePackage, eResponseStatus.MissingBasePackage.ToString());
                        return status;
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("CanPurchaseAddOn  SubscriptionCode: {0}, groupId: {1} setsIds: {2},  ex: {3}",
                    subscription != null ? string.Join(",", subscription.GetSubscriptionSetIdsToPriority().Select(x => x.Key).ToList()) : string.Empty,
                    groupId, householdId, ex);
                status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                return status;
            }

            return status;
        }

        /// update unified billing cycle (if exsits) with new paymentGatewayId
        /// get group unified billing cycle - and see if any document exsits for this domain 
        internal static void HandleDomainUnifiedBillingCycle(int groupId, long householdId, int maxUsageModuleLifeCycle, long? endDate = null)
        {
            try
            {
                if (endDate.HasValue)
                {
                    var subscriptionCycle = GetSubscriptionCycle(groupId, (int)householdId, maxUsageModuleLifeCycle);
                    if (subscriptionCycle.HasCycle && subscriptionCycle.UnifiedBillingCycle != null && subscriptionCycle.UnifiedBillingCycle.endDate != endDate.Value)
                    {
                        UnifiedBillingCycleManager.SetDomainUnifiedBillingCycle(householdId, maxUsageModuleLifeCycle, subscriptionCycle.UnifiedBillingCycle.endDate);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("HandleUpdateDomainUnifiedBillingCycle failed groupId : {0}, householdId : {1}, ex : {2}", groupId, householdId, ex));
            }
        }

        internal static bool RenewUnifiedTransactionMessageInQueue(int groupId, long householdId, long endDateUnix, DateTime nextRenewalDate, long processId, bool isKronos)
        {
            log.DebugFormat("RenewUnifiedTransactionMessageInQueue (RenewUnifiedData) processId:{0}", processId);
            
            bool enqueueSuccessful = true;
            RenewUnifiedData data = new RenewUnifiedData(groupId, householdId, processId, endDateUnix, nextRenewalDate);

            if (isKronos)
            {
                log.Debug($"Kronos - RenewUnified processId:{processId}");
                RenewManager.addEventToKronos(groupId, data);
            }
            else
            {
                if (nextRenewalDate > DateTime.UtcNow.AddYears(1).AddDays(5))
                {
                    //BEO-11219
                    log.Debug($"BEO-11219 - skip Enqueue unified renew msg (more then 1 year)! processId:{processId}, endDateUnix:{endDateUnix}");
                    return true;
                }
                // add new message to new routing key queue
                RenewTransactionsQueue queue = new RenewTransactionsQueue();
                enqueueSuccessful = queue.Enqueue(data, string.Format(ROUTING_KEY_PROCESS_UNIFIED_RENEW_SUBSCRIPTION, groupId)); 
             }

            if (!enqueueSuccessful)
            {
                log.ErrorFormat("Failed enqueue of renew unified transaction {0}", data);
            }
            else
            {
                PurchaseManager.SendRenewalReminder(groupId, data);
                log.DebugFormat("New task created (upon subscription purchase success). next unified renewal date: {0}, data: {1}",
                    nextRenewalDate, data);
            }

            return enqueueSuccessful;
        }

        ///If needed create/ update doc in cb for unifiedBilling_household_{ household_id }_renewBillingCycle
        ///create: unified billing cycle for household (CB)
        ///update: the current one with payment gateway id or end date 
        internal static void HandleDomainUnifiedBillingCycle(int groupId, long householdId, ref SubscriptionCycle subscriptionCycle, int maxUsageModuleLifeCycle, DateTime endDate, bool useCoupon)
        {
            try
            {
                var tvmDuration = subscriptionCycle.SubscriptionLifeCycle.GetTvmDuration();
                if (subscriptionCycle.HasCycle && tvmDuration == maxUsageModuleLifeCycle)
                {
                    long nextEndDate = DateUtils.DateTimeToUtcUnixTimestampMilliseconds(endDate);
                    if (subscriptionCycle.UnifiedBillingCycle == null || (subscriptionCycle.UnifiedBillingCycle != null && !useCoupon && subscriptionCycle.UnifiedBillingCycle.endDate != nextEndDate))
                    {
                        // update unified billing by endDate or paymentGatewatId                  
                        bool setResult = UnifiedBillingCycleManager.SetDomainUnifiedBillingCycle(householdId, tvmDuration, nextEndDate);
                        if (setResult)
                        {
                            subscriptionCycle.UnifiedBillingCycle = UnifiedBillingCycleManager.GetDomainUnifiedBillingCycle((int)householdId, tvmDuration);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("HandleDomainUnifiedBillingCycle failed with ex = {0}", ex);
            }
        }

        internal static void InsertOfflineCollectionUse(int groupId, int mediaFileId, string productCode, string userId, string countryCode, string languageCode, string udid, int nRelPP, LogContextData context)
        {
            try
            {
                context.Load();
                // We write an empty string as the first parameter to split the start of the log from the offlineCollectionUsesLog row data
                string infoToLog = string.Join(",", new object[] { " ", groupId, mediaFileId, productCode, userId, countryCode, languageCode, udid, nRelPP });
                offlineSubscriptionLogger.Info(infoToLog);
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Error in InsertOfflineCollectionUse, groupId: {0}, mediaFileId: {1}, productCode: {2}, userId: {3}, countryCode: {4}, languageCode: {5}, udid: {6}, nRelPP: {7}",
                                        groupId, mediaFileId, productCode, userId, countryCode, languageCode, udid, nRelPP), ex);
            }
        }

        public static List<long> GetAssetMediaRuleIds(int groupId, int mediaId)
        {
            List<long> assetMediaRuleIds = new List<long>();

            GenericListResponse<AssetRule> assetRulesMediaResponse =
                AssetRuleManager.Instance.GetAssetRules(RuleConditionType.Concurrency, groupId, new SlimAsset(mediaId, eAssetTypes.MEDIA));

            if (assetRulesMediaResponse != null && assetRulesMediaResponse.HasObjects())
            {
                assetMediaRuleIds.AddRange(assetRulesMediaResponse.Objects.Select(x => x.Id));
            }

            return assetMediaRuleIds;
        }

        public static List<long> GetAssetEpgRuleIds(int groupId, int mediaId, ref long programId)
        {
            List<long> assetEpgRuleIds = new List<long>();

            if (programId == 0)
            {
                programId = GetCurrentProgramByMediaId(groupId, mediaId);
            }

            if (programId > 0)
            {
                GenericListResponse<AssetRule> assetRulesEpgResponse =
                    AssetRuleManager.Instance.GetAssetRules(RuleConditionType.Concurrency, groupId, new SlimAsset(programId, eAssetTypes.EPG));
                if (assetRulesEpgResponse != null && assetRulesEpgResponse.HasObjects())
                {
                    assetEpgRuleIds.AddRange(assetRulesEpgResponse.Objects.Select(x => x.Id));
                }
            }

            return assetEpgRuleIds;
        }

        internal static long GetCurrentProgramByMediaId(int groupId, int mediaId)
        {
            long programId = 0;
            string epgChannelId = APILogic.Api.Managers.EpgManager.GetEpgChannelId(mediaId, groupId);
            if (!string.IsNullOrEmpty(epgChannelId))
            {
                programId = APILogic.Api.Managers.EpgManager.GetCurrentProgram(groupId, epgChannelId);
            }

            return programId;
        }

        private static List<ExtendedSearchResult> GetProgramsByMediaId(int groupId, int mediaId, int numberOfProgram)
        {
            List<ExtendedSearchResult> programs = new List<ExtendedSearchResult>();
            string epgChannelId = APILogic.Api.Managers.EpgManager.GetEpgChannelId(mediaId, groupId);
            if (!string.IsNullOrEmpty(epgChannelId))
            {
                programs = APILogic.Api.Managers.EpgManager.GetPrograms(groupId, epgChannelId, numberOfProgram);
            }

            return programs;
        }
        
        public static PagoProgramAvailability GetEntitledPagoWindow(int groupId, int domainId, int assetId,
            eAssetTypes assetType, List<MediaFile> files, EPGChannelProgrammeObject EpgProgram)
        {
            DomainEntitlements domainEntitlements = null;
            if (!TryGetDomainEntitlementsFromCache(groupId, domainId, null, ref domainEntitlements))
            {
                return null;
            }

            var userPagoIds = domainEntitlements.PagoEntitlements.Select(x => x.Value.PagoId)
                .ToList();

            //user don't have pagos
            if (userPagoIds.Count == 0) return null; 
            var userPagos = PagoManager.Instance.GetProgramAssetGroupOffers(groupId, userPagoIds);
            
            var partnerConfig = ApiDAL.GetCommercePartnerConfig(groupId, out _);

            List<ExtendedSearchResult> programs = new List<ExtendedSearchResult>();
            switch (assetType)
            {
                case eAssetTypes.EPG:
                    DateTime.TryParseExact(EpgProgram.START_DATE, EPG_DATETIME_FORMAT,
                        System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None,
                        out var startDate);
                    DateTime.TryParseExact(EpgProgram.END_DATE, EPG_DATETIME_FORMAT,
                        System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None,
                        out var endDate);
                    
                    programs.Add(new ExtendedSearchResult
                    {
                        AssetId = EpgProgram.EPG_ID.ToString(),
                        StartDate = startDate,
                        EndDate = endDate
                    });
                    break;
                case eAssetTypes.MEDIA:
                    programs = GetProgramsByMediaId(groupId, assetId, 5);
                    break;
                default:
                    return null;
            }

            var slimAssets = programs.Select(x => 
                new OTT.Service.Offers.SlimAsset
                {
                    Id = Convert.ToInt64(x.AssetId), 
                    Type = AssetType.Epg
                });
            var pagosForProgram = OffersClient.Instance.GetAssetProductOffers(groupId, slimAssets);
            if (pagosForProgram == null || pagosForProgram.Count == 0)
            {
                log.Debug($"There is no Pagos for programs id {string.Join(string.Empty, programs.Select(x => x.AssetId))}");
                return null;
            }
            
            var pago = new PagoProgramAvailability();
            foreach (var program in programs.OrderBy(prog => prog.StartDate))
            {
                //get relevant pagos for program
                var pagoPerProgram = pagosForProgram.FirstOrDefault(x => long.Parse(program.AssetId) == x.AssetId);
                //check existance of available purchased pagos and program playing.
                var availableUserPagosPerProgram = userPagos.Where(userPago => 
                    pagoPerProgram.Products?.Any(productOffer =>
                    productOffer.ProductId == userPago.Id &&
                    productOffer.ProductType == TransactionType.ProgramAssetGroupOffer) ?? false).ToList();

                //there is no available pago for user, there's no reason to keep checking and break with current program
                if(availableUserPagosPerProgram.Count == 0) break;

                //add padding for program for entitlement window
                var padStartDate = program.StartDate.AddSeconds(-partnerConfig.ProgramAssetEntitlementPaddingStart ?? 0);
                var padEndDate = program.EndDate.AddSeconds(partnerConfig.ProgramAssetEntitlementPaddingEnd ?? 0);
                //set the first program with pago.
                if (!pago.IsStartDateSet() && padStartDate < DateTime.UtcNow)
                {
                    pago.StartDate = padStartDate;
                    //it doesnt matter so we're taking the first one
                    pago.PagoId = availableUserPagosPerProgram.First().Id;
                    var allTypesAreValid = availableUserPagosPerProgram.Any(availableUserPagoPerProgram => availableUserPagoPerProgram.FileTypeIds == null);
                    var availableFiles = files;
                    if (!allTypesAreValid)
                        availableFiles = availableFiles.Where(file =>
                            availableUserPagosPerProgram.Any(
                                availableUserPagoPerProgram => availableUserPagoPerProgram.FileTypeIds.Contains(file.TypeId)))
                            .ToList();
                    pago.FileIds = availableFiles.Select(file => file.Id).ToList();
                }
                // keep the end date of the last one that fit.   
                pago.EndDate = padEndDate;
            }

            log.Debug($"PagoId {pago.PagoId} StartDate {pago.StartDate} EndDate {pago.EndDate} FileIds {pago.FileIds}");
            return pago.IsValid() ? pago : null;

        }
        
        internal static List<EPGChannelProgrammeObject> GetEpgsByExternalIds(int nGroupID, List<string> epgExternalIds)
        {
            List<EPGChannelProgrammeObject> epgs = null;

            try
            {
                EPGProgramsByProgramsIdentefierRequest request = new EPGProgramsByProgramsIdentefierRequest();
                request.m_nGroupID = nGroupID;
                //don't get the same EPG from catalog
                request.pids = epgExternalIds;
                request.m_oFilter = new Filter();
                FillCatalogSignature(request);

                EpgProgramResponse response = request.GetResponse(request) as EpgProgramResponse;
                if (response != null && response.m_nTotalItems > 0 && response.m_lObj != null && response.m_lObj.Count > 0)
                {
                    epgs = new List<EPGChannelProgrammeObject>();
                    foreach (ProgramObj program in response.m_lObj)
                    {
                        // no need to check epg status since catalog returns only active epg's
                        if (program.AssetType == eAssetTypes.EPG && program.m_oProgram != null && program.m_oProgram.EPG_ID > 0)
                        {
                            epgs.Add(program.m_oProgram);
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                log.Error("Failed GetEpgsByExternalIds Request To Catalog", ex);
            }

            return epgs;
        }

        internal static long GetCouponGroupIdForFirstCoupon(int groupId, Subscription subscription, ref string couponCode, long purchaseId)
        {
            // get all SubscriptionsCouponGroup (with expiry date !!!!)
            var allCoupons = Pricing.Utils.GetSubscriptionCouponsGroup(long.Parse(subscription.m_SubscriptionCode), groupId, false);

            if ((subscription.m_oCouponsGroup != null && subscription.m_oCouponsGroup.m_oDiscountCode != null) ||
                (allCoupons != null && allCoupons.Count > 0 && allCoupons.Count(x => x.m_oDiscountCode == null) == 0))
            {
                // check if coupon related to subscription the type is coupon gift card or coupon                        
                long couponGroupId = Utils.GetSubscriptiopnPurchaseCoupon(ref couponCode, purchaseId, groupId); // return only if valid .

                if (couponGroupId > 0 && ((subscription.m_oCouponsGroup != null && !string.IsNullOrEmpty(subscription.m_oCouponsGroup.m_sGroupCode) && subscription.m_oCouponsGroup.m_sGroupCode.Equals(couponGroupId.ToString())) ||
                                          (allCoupons != null && allCoupons.Count(x => !string.IsNullOrEmpty(x.m_sGroupCode) && x.m_sGroupCode.Equals(couponGroupId.ToString())) > 0)))
                {
                    return couponGroupId;
                }
            }

            return 0;
        }

        internal static List<ApiObjects.KeyValuePair> GetResumRenewAdapterData(long purchaseId)
        {
            return ConditionalAccessDAL.GetResumRenewAdapterData(purchaseId);
        }

        internal static List<ApiObjects.KeyValuePair> GetResumRenewUnifiedAdapterData(long processId)
        {
            return ConditionalAccessDAL.GetResumRenewUnifiedAdapterData(processId);
        }

        public static DateTime CalcCollectionEndDate(Collection col, DateTime dtToInitializeWith)
        {
            DateTime res = dtToInitializeWith;
            if (col != null)
            {
                if (col.m_oCollectionUsageModule != null)
                {
                    // calc end date as before.
                    res = Utils.GetEndDateTime(res, col.m_oCollectionUsageModule.m_tsMaxUsageModuleLifeCycle);
                }
            }
            return res;
        }

        public static Price GetLowestPriceByCouponCodeOfSubcription(int groupId, string couponCode, Subscription subscription, Price lowestPrice, 
            long domainId, string countryCode)
        {
            long couponGroupId = PricingDAL.Get_CouponGroupId(groupId, couponCode); // return only if valid 
            if (couponGroupId > 0)
            {
                // look if this coupon group id exists in coupon list 
                CouponsGroup currCouponGroup = null;
                if (subscription.m_oCouponsGroup != null && !string.IsNullOrEmpty(subscription.m_oCouponsGroup.m_sGroupCode) && subscription.m_oCouponsGroup.m_sGroupCode.Equals(couponGroupId.ToString()))
                {
                    currCouponGroup = ObjectCopier.Clone(subscription.m_oCouponsGroup);
                }
                else
                {
                    currCouponGroup = ObjectCopier.Clone<CouponsGroup>(subscription.GetValidSubscriptionCouponGroup(couponGroupId.ToString()).FirstOrDefault());
                }

                lowestPrice = Utils.CalculateCouponDiscount(ref lowestPrice, currCouponGroup, ref couponCode, groupId, domainId, countryCode);
            }

            return lowestPrice;
        }

        public Price GetLowestPriceByCouponCode(int groupId, ref string couponCode, List<SubscriptionCouponGroup> subscriptionCouponGroups, Price currentPrice, 
            int domainId, CouponsGroup couponsGroup, string countryCode)
        {
            long couponGroupId = PricingDAL.Get_CouponGroupId(groupId, couponCode); // return only if valid 
            if (couponGroupId <= 0) { return currentPrice; }

            // look if this coupon group id exsits in coupon list 
            CouponsGroup currCouponGroup = null;
            if (couponsGroup != null && !string.IsNullOrEmpty(couponsGroup.m_sGroupCode) && couponsGroup.m_sGroupCode.Equals(couponGroupId.ToString()))
            {
                currCouponGroup = ObjectCopier.Clone(couponsGroup);
            }
            else if (subscriptionCouponGroups != null)
            {
                var subscriptionCouponGroup = subscriptionCouponGroups.FirstOrDefault
                    (x => x.m_sGroupCode.Equals(couponGroupId.ToString()) && (!x.endDate.HasValue || x.endDate.Value >= DateTime.UtcNow));
                currCouponGroup = ObjectCopier.Clone<CouponsGroup>(subscriptionCouponGroup);
            }

            return Utils.CalculateCouponDiscount(ref currentPrice, currCouponGroup, ref couponCode, groupId, domainId, countryCode);
        }

        private static Dictionary<long, PagoEntitlement> InitializeDomainPagos(int groupId, int domainId, List<int> usersInDomain)
        {
            Dictionary<long, PagoEntitlement> pagoEntitlements = new Dictionary<long, PagoEntitlement>();
            DataTable dt = ConditionalAccessDAL.GetPagoPurchases(domainId, usersInDomain);

            if (dt?.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    long pagoId = ODBCWrapper.Utils.GetLongSafeVal(row, "PROGRAM_ASSET_GROUP_OFFER_ID");

                    if (!pagoEntitlements.ContainsKey(pagoId))
                    {
                        PagoEntitlement pagoEntitlement = new PagoEntitlement()
                        {
                            CreateDate = ODBCWrapper.Utils.GetDateSafeVal(row, "CREATE_DATE"),
                            EndDate = ODBCWrapper.Utils.GetDateSafeVal(row, "END_DATE"),
                            Id = ODBCWrapper.Utils.GetLongSafeVal(row, "ID"),
                            IsPending = ODBCWrapper.Utils.GetIntSafeVal(row, "IS_PENDING") == 1,
                            PagoId = pagoId,
                            PurchasedByUserId = ODBCWrapper.Utils.GetLongSafeVal(row, "SITE_USER_GUID"),
                            StartDate = ODBCWrapper.Utils.GetDateSafeVal(row, "CREATE_DATE")
                        };

                        pagoEntitlements.Add(pagoId, pagoEntitlement);
                    }
                }
            }

            return pagoEntitlements;
        }

        private static void AddCollectionsToCheck(int fileTypeIdKey, ref List<UserBundlePurchase> collectionToCheck, Dictionary<int, List<Collection>> fileTypeIdToCollectionMappings, Dictionary<string, UserBundlePurchase> collPurchases)
        {
            if (fileTypeIdToCollectionMappings.ContainsKey(fileTypeIdKey))
            {
                foreach (var collection in fileTypeIdToCollectionMappings[fileTypeIdKey])
                {
                    if (collPurchases.ContainsKey(collection.m_CollectionCode))
                    {
                        collectionToCheck.Add(collPurchases[collection.m_CollectionCode]);
                    }
                }
            }

        }
    }
}