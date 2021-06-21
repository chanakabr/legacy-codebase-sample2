using ApiObjects;
using ApiObjects.AssetLifeCycleRules;
using ApiObjects.Base;
using ApiObjects.Pricing;
using ApiObjects.Pricing.Dto;
using CouchbaseManager;
using KLogMonitor;
using Newtonsoft.Json;
using ODBCWrapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Xml;
using static ODBCWrapper.Parameter;

namespace DAL
{
    public interface ICampaignRepository
    {
        bool Update_Campaign(Campaign campaign, ContextData contextData);
        List<CampaignDB> GetCampaignsByGroupId(int groupId, eCampaignType campaignType);
        Campaign GetCampaignById(int groupId, long id);
        T AddCampaign<T>(T campaign, ContextData contextData) where T : Campaign, new();
        bool DeleteCampaign(long groupId, long campaignId);
    }
    public interface IDiscountDetailsRepository
    {
        bool DeleteDiscountDetails(int groupId, long id, long userId);
        long InsertDiscountDetails(int groupId, string code, double price, double percentage, long currencyId,
                                           DateTime startDate, DateTime endDate, long userId, List<DiscountDTO> discounts,
                                           WhenAlgoType whenAlgoType, int whenAlgoTimes);
        bool IsDiscountCodeExists(int groupId, long id);
    }

    public interface IPriceDetailsRepository
    {
        bool DeletePriceDetails(int groupId, long id, long userId);
        long InsertPriceDetails(int groupId, string code, double price, long currencyId, List<PriceDTO> priceCodesLocales, long userId);
        List<PriceDetailsDTO> GetPriceCodesDTO(int groupId);
        bool IsPriceCodeExistsById(int groupId, long id);
    }

    public interface IPricePlanRepository
    {
        List<UsageModuleDTO> GetPricePlansDTO(int groupId, List<long> pricePlanIds = null);
        bool UpdatePricePlanAndSubscriptionsPriceCode(int groupId, int pricePlanId, int priceCodeId);
        bool DeletePricePlan(int groupId, long id, long userId);
        int InsertPricePlan(int groupId, IngestPricePlan pricePlan, int priceCodeId, int fullLifeCycleID, int viewLifeCycleID, int discountID);
    }
    public interface IModuleManagerRepository
    {
        int InsertUsageModule(long userId, int groupID, string name, int maxViews, int fullLifeCycleID,
                                int viewLifeCycleID, int waiverPeriod, bool isWaiverEnabled, bool isOfflinePlayback);
        bool DeletePricePlan(int groupId, long id, long userId);

        bool IsUsageModuleExistsById(int groupId, long id);

        DataTable GetPricePlans(int groupId, List<long> pricePlanIds = null);
    }

    public interface IPreviewModuleRepository
    {
        DataTable Get_PreviewModulesByGroupID(int nGroupID, bool bIsActive, bool bNotDeleted);
        long InsertPreviewModule(int groupID, string name, int fullLifeCycle, int nonRenewPeriod, long userId);
        bool DeletePreviewModule(int groupId, long id, long userId);
        bool IsPreviewModuleExsitsd(int groupId, long id);
    }

    public interface ICollectionRepository
    {
        long Insert_Collection(int groupId, int priceId, int discountId, int usageModuleId,
            DateTime? startDate, DateTime? endDate, string couponGroupCode, long userId, LanguageContainer[] description,
            LanguageContainer[] names, List<long> channelIds, List<SubscriptionCouponGroupDTO> couponsGroups, List<KeyValuePair<VerificationPaymentGateway, string>> externalProductCodes);
        bool IsCollectionExists(int groupId, long id);
        bool DeleteCollection(int groupId, long id, long userId);
    }
    public interface IPartnerRepository
    {
        bool SetupPartnerInPricingDb(long partnerId, List<KeyValuePair<long, long>> moduleIds, long updaterId);
        bool DeletePartnerInPricingDb(long partnerId, long updaterId);
    }

    public class PricingDAL : ICampaignRepository, IPriceDetailsRepository, IPricePlanRepository, IModuleManagerRepository, IDiscountDetailsRepository, IPreviewModuleRepository, ICollectionRepository, IPartnerRepository
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private static readonly Lazy<PricingDAL> lazy = new Lazy<PricingDAL>(() => new PricingDAL(), LazyThreadSafetyMode.PublicationOnly);

        public static PricingDAL Instance { get { return lazy.Value; } }

        private PricingDAL()
        {
        }

        public static DataTable Get_PPVModuleListForMediaFiles(int nGroupID, List<int> mediaFileList)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_PPVModuleListForMediaFiles");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@GroupID", nGroupID);
            sp.AddIDListParameter("@MediaFileList", mediaFileList, "id");

            DataSet ds = sp.ExecuteDataSet();

            if (ds != null)
                return ds.Tables[0];
            return null;
        }

        public static DataTable Get_PPVModuleListForMediaFilesWithExpired(int nGroupID, List<int> mediaFileList)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_PPVModuleListForMediaFilesWithExpired");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@GroupID", nGroupID);
            sp.AddIDListParameter("@MediaFileList", mediaFileList, "id");

            DataSet ds = sp.ExecuteDataSet();

            if (ds != null)
                return ds.Tables[0];
            return null;
        }

        public static DataTable Get_ChannelsBySubscription(int nGroupID, int nSubscriptionID)
        {
            ODBCWrapper.StoredProcedure spUserSocial = new ODBCWrapper.StoredProcedure("Get_ChannelsBySubscription");
            spUserSocial.SetConnectionKey("pricing_connection");

            spUserSocial.AddParameter("@GroupID", nGroupID);
            spUserSocial.AddParameter("@SubscriptionID", nSubscriptionID);

            DataSet ds = spUserSocial.ExecuteDataSet();

            if (ds != null)
                return ds.Tables[0];
            return null;
        }

        public static DataSet Get_SubscriptionData(int nGroupID, int? nIsActive, int? nSubscriptionID = null, string sProductCode = null, List<int> userTypesIDsList = null, int? nTopRows = null)
        {
            ODBCWrapper.StoredProcedure spSubscriptionData = new ODBCWrapper.StoredProcedure("GetSubscriptionData");
            spSubscriptionData.SetConnectionKey("pricing_connection");

            spSubscriptionData.AddParameter("@GroupID", nGroupID);
            spSubscriptionData.AddParameter("@IsActive", nIsActive);
            spSubscriptionData.AddParameter("@SubscriptionID", nSubscriptionID);
            spSubscriptionData.AddParameter("@ProductCode", sProductCode);

            int UserTypesIdIn = 0;
            if (userTypesIDsList != null && userTypesIDsList.Count > 0)
            {
                UserTypesIdIn = 1;
            }

            spSubscriptionData.AddParameter("@UserTypesIdIn", UserTypesIdIn);
            spSubscriptionData.AddIDListParameter("@UserTypesIdList", userTypesIDsList, "id");

            spSubscriptionData.AddParameter("@TopRows", nTopRows);

            DataSet ds = spSubscriptionData.ExecuteDataSet();
            return ds;
        }

        public static DataTable Get_PreviewModuleData(int nGroupID, long nPreviewModuleID)
        {
            ODBCWrapper.StoredProcedure spPreviewModuleData = new ODBCWrapper.StoredProcedure("Get_PreviewModuleData");
            spPreviewModuleData.SetConnectionKey("pricing_connection");
            spPreviewModuleData.AddParameter("@GroupID", nGroupID);
            spPreviewModuleData.AddParameter("@PreviewModuleID", nPreviewModuleID);

            DataSet ds = spPreviewModuleData.ExecuteDataSet();
            if (ds != null)
                return ds.Tables[0];
            return null;

        }

        public static string Get_ItemName(string sTableName, long lItemCode)
        {
            ODBCWrapper.StoredProcedure spItemName = new ODBCWrapper.StoredProcedure("Get_ItemName");
            spItemName.SetConnectionKey("pricing_connection");
            spItemName.AddParameter("@TableName", sTableName);
            spItemName.AddParameter("@ItemCode", lItemCode);

            DataSet ds = spItemName.ExecuteDataSet();
            if (ds != null)
            {
                DataTable dt = ds.Tables[0];
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0 && dt.Rows[0]["name"] != DBNull.Value)
                    return dt.Rows[0]["name"].ToString();
            }

            return string.Empty;

        }

        public static void Insert_NewCouponUse(string sSiteGuid, long lCouponID, long lGroupID, long lMediaFileID, long lSubscriptionCode, long lPrePaidCode, long nCollectionCode)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Insert_NewCouponUse");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@SiteGuid", sSiteGuid);
            sp.AddParameter("@CouponID", lCouponID);
            sp.AddParameter("@GroupID", lGroupID);
            DateTime dtToWriteToDB = DateTime.UtcNow;
            sp.AddParameter("@CreateDate", dtToWriteToDB);
            sp.AddParameter("@MediaFileID", lMediaFileID);
            sp.AddParameter("@SubscriptionCode", lSubscriptionCode);
            sp.AddParameter("@PrePaidCode", lPrePaidCode);
            sp.AddParameter("@UpdateDate", dtToWriteToDB);
            sp.AddParameter("@CollectionCode", nCollectionCode);
            sp.ExecuteNonQuery();
        }

        public DataTable Get_PreviewModulesByGroupID(int nGroupID, bool bIsActive, bool bNotDeleted)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_PreviewModulesByGroupID");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@GroupID", nGroupID);
            sp.AddParameter("@IsActive", bIsActive ? 1 : 0);
            sp.AddParameter("@Status", bNotDeleted ? 1 : 2);
            DataSet ds = sp.ExecuteDataSet();
            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
                return ds.Tables[0];
            return null;
        }

        public bool DeletePreviewModule(int groupId, long id, long userId)
        {
            try
            {
                var sp = new StoredProcedure("Delete_PreviewModule");
                sp.SetConnectionKey("pricing_connection");

                sp.AddParameter("@id", id);
                sp.AddParameter("@groupId", groupId);
                sp.AddParameter("@updaterId", userId);
                var result = sp.ExecuteReturnValue<int>() > 0;

                return result;
            }
            catch (Exception ex)
            {
                log.Error($"Error while Delete PreviewModule, groupId: {groupId}, Id: {id}", ex);
                return false;
            }
        }

        public static DataSet Get_SubscriptionsList(int nGroupID, int nFileTypeId)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_SubscriptionsList");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@GroupID", nGroupID);
            sp.AddParameter("@FileTypeId", nFileTypeId);
            DataSet ds = sp.ExecuteDataSet();

            return ds;
        }

        public static bool Handle_CouponUse(string sCouponCode, string sSiteGuid, long lGroupID, long lMediaFileID,
        long lSubscriptionCode, long lPrePaidCode, int nIncrementBy)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Handle_CouponUse");
            sp.SetConnectionKey("PRICING_CONNECTION");
            sp.AddParameter("@CouponCode", sCouponCode);
            sp.AddParameter("@SiteGuid", sSiteGuid);
            sp.AddParameter("@GroupID", lGroupID);
            sp.AddParameter("@MediaFileID", lMediaFileID);
            sp.AddParameter("@SubscriptionCode", lSubscriptionCode);
            sp.AddParameter("@PrePaidCode", lPrePaidCode);
            sp.AddParameter("@IncrementBy", nIncrementBy);
            DateTime dtToWriteToDB = DateTime.UtcNow;
            sp.AddParameter("@LastUsedDate", dtToWriteToDB);
            sp.AddParameter("@CreateAndUpdateDate", dtToWriteToDB);

            return sp.ExecuteReturnValue<long>() > 0;
        }

        public static DataTable Get_SubscriptionsByChannel(int nGroupID, List<int> nChannelIDs)
        {
            ODBCWrapper.StoredProcedure spSubscriptionByChannel = new ODBCWrapper.StoredProcedure("Get_SubscriptionsByChannel");
            spSubscriptionByChannel.SetConnectionKey("pricing_connection");

            spSubscriptionByChannel.AddParameter("@GroupID", nGroupID);
            spSubscriptionByChannel.AddIDListParameter<int>("@ChannelIDs", nChannelIDs, "Id");

            DataSet ds = spSubscriptionByChannel.ExecuteDataSet();

            if (ds != null)
                return ds.Tables[0];

            return null;
        }

        public static DataTable Get_MediaByFileID(int nGroupID, int nMediaFileID)
        {
            ODBCWrapper.StoredProcedure spMediaIDByFileID = new ODBCWrapper.StoredProcedure("Get_MediaByFileID");
            spMediaIDByFileID.SetConnectionKey("MAIN_CONNECTION_STRING");

            spMediaIDByFileID.AddParameter("@GroupID", nGroupID);
            spMediaIDByFileID.AddParameter("@MediaFileID", nMediaFileID);

            DataSet ds = spMediaIDByFileID.ExecuteDataSet();

            if (ds != null)
                return ds.Tables[0];

            return null;
        }

        public bool SetupPartnerInPricingDb(long partnerId, List<KeyValuePair<long, long>> moduleIds, long updaterId)
        {
            var sp = new StoredProcedure("Create_GroupBasicData");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@groupId", partnerId);
            sp.AddParameter("@updaterId", updaterId);
            sp.AddKeyValueListParameter("@moudleIds", moduleIds, "idKey", "value");

            return sp.ExecuteReturnValue<int>() > 0;
        }

        public bool DeletePartnerInPricingDb(long partnerId, long updaterId)
        {
            var sp = new StoredProcedure("Delete_GroupBasicData");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@groupId", partnerId);
            sp.AddParameter("@updaterId", updaterId);

            return sp.ExecuteReturnValue<int>() > 0;
        }
        public static DataTable Get_SubscriptionsListByChannelAndFileType(int nGroupID, List<int> nChannelIDs, int nMediaFileID)
        {
            ODBCWrapper.StoredProcedure spSubscriptionByChannel = new ODBCWrapper.StoredProcedure("Get_SubscriptionsListByChannelAndFileType");
            spSubscriptionByChannel.SetConnectionKey("pricing_connection");

            spSubscriptionByChannel.AddParameter("@GroupID", nGroupID);
            spSubscriptionByChannel.AddIDListParameter<int>("@ChannelIDs", nChannelIDs, "Id");
            spSubscriptionByChannel.AddParameter("@FileTypeId", nMediaFileID);

            DataSet ds = spSubscriptionByChannel.ExecuteDataSet();

            if (ds != null)
                return ds.Tables[0];

            return null;
        }

        public static DataTable Get_PPVModuleData(int nGroupID, int? nPPVModuleID)
        {
            ODBCWrapper.StoredProcedure spPPVModuleData = new ODBCWrapper.StoredProcedure("Get_PPV_ModuleData");
            spPPVModuleData.SetConnectionKey("pricing_connection");

            spPPVModuleData.AddParameter("@GroupID", nGroupID);
            spPPVModuleData.AddNullableParameter<int?>("@PPVModuleID", nPPVModuleID);

            DataSet ds = spPPVModuleData.ExecuteDataSet();

            if (ds != null)
                return ds.Tables[0];

            return null;
        }

        public static DataTable Get_PPVDescription(int nPPVModuleID)
        {
            ODBCWrapper.StoredProcedure spPPVDescription = new ODBCWrapper.StoredProcedure("Get_PPV_Description");

            spPPVDescription.SetConnectionKey("pricing_connection");
            spPPVDescription.AddParameter("@PPVModuleID", nPPVModuleID);

            DataSet ds = spPPVDescription.ExecuteDataSet();

            if (ds != null)
                return ds.Tables[0];

            return null;
        }

        public static DataTable Get_PPVFileTypes(int nGroupID, int nPPVModuleID)
        {
            ODBCWrapper.StoredProcedure spPPVFileTypes = new ODBCWrapper.StoredProcedure("Get_PPV_FileTypes");

            spPPVFileTypes.SetConnectionKey("pricing_connection");
            spPPVFileTypes.AddParameter("@GroupID", nGroupID);
            spPPVFileTypes.AddParameter("@PPVModuleID", nPPVModuleID);

            DataSet ds = spPPVFileTypes.ExecuteDataSet();

            if (ds != null)
                return ds.Tables[0];

            return null;
        }

        public static List<long> Get_OperatorChannelIDs(int nGroupID, int nOperatorID, string sConnKey)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_OperatorChannelIDs");
            sp.SetConnectionKey(!string.IsNullOrEmpty(sConnKey) ? sConnKey : "CONNECTION_STRING");
            sp.AddParameter("@OperatorID", nOperatorID);
            sp.AddParameter("@GroupID", nGroupID);

            DataSet ds = sp.ExecuteDataSet();
            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
            {
                DataTable dt = ds.Tables[0];
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    int length = dt.Rows.Count;
                    List<long> res = new List<long>(length);
                    for (int i = 0; i < length; i++)
                    {
                        long channelID = ODBCWrapper.Utils.GetLongSafeVal(dt.Rows[i]["channel_id"]);
                        if (channelID > 0)
                            res.Add(channelID);
                    }
                    return res;
                }
            }

            return new List<long>(0);
        }

        public static List<long> Get_OperatorChannelIDs(int nGroupID, int nOperatorID)
        {
            return Get_OperatorChannelIDs(nGroupID, nOperatorID, string.Empty);
        }

        public static List<long> Get_SubscriptionChannelIDs(int nGroupID, int nSubscriptionID, string sConnKey)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_SubscriptionChannelIDs");
            sp.SetConnectionKey(!string.IsNullOrEmpty(sConnKey) ? sConnKey : "CONNECTION_STRING");
            sp.AddParameter("@GroupID", nGroupID);
            sp.AddParameter("@SubscriptionID", nSubscriptionID);

            DataSet ds = sp.ExecuteDataSet();
            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
            {
                DataTable dt = ds.Tables[0];
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    int length = dt.Rows.Count;
                    List<long> res = new List<long>(length);
                    for (int i = 0; i < length; i++)
                    {
                        long lChannelID = ODBCWrapper.Utils.GetLongSafeVal(dt.Rows[i]["channel_id"]);
                        if (lChannelID > 0)
                            res.Add(lChannelID);
                    }

                    return res;
                }
            }

            return new List<long>(0);
        }

        public static List<long> Get_SubscriptionChannelIDs(int nGroupID, int nSubscriptionID)
        {
            return Get_SubscriptionChannelIDs(nGroupID, nSubscriptionID, string.Empty);
        }

        public static DataSet Get_CollectionData(int nGroupID, int? nIsActive, int? nCollectionID = null, string sProductCode = null, List<int> userTypesIDsList = null, int? nTopRows = null)
        {
            ODBCWrapper.StoredProcedure spCollectionData = new ODBCWrapper.StoredProcedure("GetCollectionData");
            spCollectionData.SetConnectionKey("pricing_connection");

            spCollectionData.AddParameter("@GroupID", nGroupID);
            spCollectionData.AddParameter("@IsActive", nIsActive);
            spCollectionData.AddParameter("@CollectionID", nCollectionID);
            spCollectionData.AddParameter("@ProductCode", sProductCode);
            spCollectionData.AddParameter("@TopRows", nTopRows);

            DataSet ds = spCollectionData.ExecuteDataSet();
            return ds;
        }

        public static DataTable GetUsageModulePPV(string sAssetCode)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("GetUsageModulePPV");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@PPVCode", sAssetCode);

            DataSet ds = sp.ExecuteDataSet();
            if (ds != null && ds.Tables != null)
                return ds.Tables[0];
            return null;
        }

        public static DataTable GetUsageModuleSubscription(string sAssetCode)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("GetUsageModuleSubscription");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@SubscriptiomCode", sAssetCode);

            DataSet ds = sp.ExecuteDataSet();
            if (ds != null && ds.Tables != null)
                return ds.Tables[0];
            return null;
        }

        public static DataTable GetUsageModuleCollection(string sAssetCode)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("GetUsageModuleCollection");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@CollectionCode", sAssetCode);

            DataSet ds = sp.ExecuteDataSet();
            if (ds != null && ds.Tables != null)
                return ds.Tables[0];
            return null;
        }

        public static DataSet Get_SubscriptionsData(int nGroupID, List<long> lstSubscriptions)
        {
            StoredProcedure sp = new StoredProcedure("Get_SubscriptionsDataWithServices");
            sp.SetConnectionKey("PRICING_CONNECTION");
            sp.AddParameter("@GroupID", nGroupID);
            sp.AddIDListParameter("@Subscriptions", lstSubscriptions, "ID");

            return sp.ExecuteDataSet();
        }

        public static Dictionary<long, List<int>> Get_SubscriptionsFileTypes(int nGroupID, List<long> lstSubsIDs)
        {
            StoredProcedure sp = new StoredProcedure("Get_SubscriptionsFileTypes");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@GroupID", nGroupID);
            sp.AddIDListParameter("@Subscriptions", lstSubsIDs, "ID");

            DataSet ds = sp.ExecuteDataSet();
            Dictionary<long, List<int>> res = new Dictionary<long, List<int>>();

            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
            {
                DataTable dt = ds.Tables[0];
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        long lSubCode = ODBCWrapper.Utils.GetLongSafeVal(dt.Rows[i]["subscription_id"]);
                        int nFileType = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[i]["file_type_id"]);
                        if (res.ContainsKey(lSubCode))
                        {
                            res[lSubCode].Add(nFileType);
                        }
                        else
                        {
                            res.Add(lSubCode, new List<int>() { nFileType });
                        }
                    }
                }
            }

            return res;
        }

        public static Dictionary<long, List<long>> Get_SubscriptionsChannels(int nGroupID, List<long> lstSubsIDs)
        {
            StoredProcedure sp = new StoredProcedure("Get_SubscriptionsChannels");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@GroupID", nGroupID);
            sp.AddIDListParameter("@Subscriptions", lstSubsIDs, "ID");

            DataSet ds = sp.ExecuteDataSet();
            Dictionary<long, List<long>> res = new Dictionary<long, List<long>>();
            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
            {
                DataTable dt = ds.Tables[0];
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        long lSubCode = ODBCWrapper.Utils.GetLongSafeVal(dt.Rows[i]["subscription_id"]);
                        long lChannelID = ODBCWrapper.Utils.GetLongSafeVal(dt.Rows[i]["channel_id"]);
                        if (res.ContainsKey(lSubCode))
                        {
                            res[lSubCode].Add(lChannelID);
                        }
                        else
                        {
                            res.Add(lSubCode, new List<long>() { lChannelID });
                        }
                    }
                }
            }

            return res;
        }

        public static Dictionary<long, List<string[]>> Get_SubscriptionsDescription(int nGroupID, List<long> lstSubCodes)
        {
            StoredProcedure sp = new StoredProcedure("Get_SubscriptionsDescription");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@GroupID", nGroupID);
            sp.AddIDListParameter("@Subscriptions", lstSubCodes, "ID");

            DataSet ds = sp.ExecuteDataSet();
            Dictionary<long, List<string[]>> res = new Dictionary<long, List<string[]>>();

            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
            {
                DataTable dt = ds.Tables[0];
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        long lSubCode = ODBCWrapper.Utils.GetLongSafeVal(dt.Rows[i]["subscription_id"]);
                        string[] arr = new string[2];
                        arr[0] = ODBCWrapper.Utils.GetSafeStr(dt.Rows[i]["language_code3"]);
                        arr[1] = ODBCWrapper.Utils.GetSafeStr(dt.Rows[i]["description"]);
                        if (res.ContainsKey(lSubCode))
                        {
                            res[lSubCode].Add(arr);
                        }
                        else
                        {
                            res.Add(lSubCode, new List<string[]>() { arr });
                        }
                    }
                }
            }

            return res;
        }

        public static Dictionary<long, List<string[]>> Get_SubscriptionsNames(int nGroupID, List<long> lstSubCodes)
        {
            StoredProcedure sp = new StoredProcedure("Get_SubscriptionsNames");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@GroupID", nGroupID);
            sp.AddIDListParameter("@Subscriptions", lstSubCodes, "ID");

            DataSet ds = sp.ExecuteDataSet();
            Dictionary<long, List<string[]>> res = new Dictionary<long, List<string[]>>();

            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
            {
                DataTable dt = ds.Tables[0];
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        long lSubCode = ODBCWrapper.Utils.GetLongSafeVal(dt.Rows[i]["subscription_id"]);
                        string[] arr = new string[2];
                        arr[0] = ODBCWrapper.Utils.GetSafeStr(dt.Rows[i]["language_code3"]);
                        arr[1] = ODBCWrapper.Utils.GetSafeStr(dt.Rows[i]["description"]);
                        if (res.ContainsKey(lSubCode))
                        {
                            res[lSubCode].Add(arr);
                        }
                        else
                        {
                            res.Add(lSubCode, new List<string[]>() { arr });
                        }
                    }
                }
            }

            return res;
        }

        public static DataSet Get_CollectionsData(int nGroupID, List<long> lstCollCodes)
        {
            StoredProcedure sp = new StoredProcedure("Get_CollectionsData");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@GroupID", nGroupID);
            sp.AddIDListParameter("@Collections", lstCollCodes, "ID");

            return sp.ExecuteDataSet();
        }

        public long Insert_Collection(int groupId, int priceId, int discountId, int usageModuleId, 
            DateTime? startDate, DateTime? endDate, string couponGroupCode, long userId, LanguageContainer[] description,
            LanguageContainer[] names, List<long> channelIds, List<SubscriptionCouponGroupDTO> couponsGroups, List<KeyValuePair<VerificationPaymentGateway, string>> externalProductCodes)
        {
            try
            {
                var sp = new StoredProcedure("Insert_Collection");
                sp.SetConnectionKey("pricing_connection");
                sp.AddParameter("@groupId", groupId);
                sp.AddParameter("@Name", names[0].m_sValue);
                sp.AddParameter("@PriceId", priceId);
                sp.AddParameter("@DiscountId", discountId); 
                sp.AddParameter("@UsageModuleId", usageModuleId);
                sp.AddParameter("@StartDate", startDate);
                sp.AddParameter("@EndDate", endDate);
                sp.AddParameter("@CouponGroupCode", couponGroupCode);
                sp.AddParameter("@UserId", userId);
                sp.AddParameter("@DescriptionsLocales", SetMultilingualStringLocales(description));
                sp.AddParameter("@NamesLocales", SetMultilingualStringLocales(names.Skip(1).ToArray()));
                sp.AddIDListParameter("@ChannelIds", channelIds, "ID");
                sp.AddParameter("@CouponGroupLocales", SetCouponsGroupsCodesLocales(couponsGroups));
                sp.AddParameter("@ProductsCodesLocales", SetProductsCodesLocales(externalProductCodes));

                var id = sp.ExecuteReturnValue<long>();

                return id;
            }
            catch (Exception ex)
            {
                log.Error($"Error while InsertCollection, groupId: {groupId}, ex:{ex} ");
                return 0;
            }
        }

        public bool IsCollectionExists(int groupId, long id)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Is_CollectionExists");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@groupId", groupId);
            sp.AddParameter("@CollectionId", id);
            return sp.ExecuteReturnValue<int>() > 0;
        }

        public bool DeleteCollection(int groupId, long id, long userId)
        {
            try
            {
                var sp = new StoredProcedure("Delete_Collection");
                sp.SetConnectionKey("pricing_connection");

                sp.AddParameter("@id", id);
                sp.AddParameter("@groupId", groupId);
                sp.AddParameter("@updaterId", userId);
                var result = sp.ExecuteReturnValue<int>() > 0;

                return result;
            }
            catch (Exception ex)
            {
                log.Error($"Error while Delete DrmAdapter, groupId: {groupId}, Id: {id}", ex);
                return false;
            }
        }
        private static DataTable SetMultilingualStringLocales(LanguageContainer[] descriptions)
        {
            DataTable ccTable = new DataTable("MultilingualStringLocalesValues");

            ccTable.Columns.Add("language_code3", typeof(string));
            ccTable.Columns.Add("description", typeof(string));

            if (descriptions != null)
            {
                DataRow dr = null;
                foreach (var description in descriptions)
                {
                    dr = ccTable.NewRow();
                    dr["language_code3"] = description.m_sLanguageCode3;
                    dr["description"] = description.m_sValue;
                    ccTable.Rows.Add(dr);
                }
            }
            return ccTable;
        }

        private static DataTable SetCouponsGroupsCodesLocales(List<SubscriptionCouponGroupDTO> CouponsGroups)
        {
            DataTable ccTable = new DataTable("CouponGroupLocalesValues");

            ccTable.Columns.Add("COUPON_GROUP_ID", typeof(int));
            ccTable.Columns.Add("START_DATE", typeof(DateTime));
            ccTable.Columns.Add("END_DATE", typeof(DateTime));

            if (CouponsGroups != null)
            {
                DataRow dr = null;
                foreach (var couponsGroups in CouponsGroups)
                {
                    dr = ccTable.NewRow();
                    dr["COUPON_GROUP_ID"] = couponsGroups.GroupCode;

                    if (couponsGroups.StartDate.HasValue)
                        dr["START_DATE"] = couponsGroups.StartDate;
                    else
                        dr["START_DATE"] = DBNull.Value;

                    if (couponsGroups.EndDate.HasValue)
                        dr["END_DATE"] = couponsGroups.EndDate;
                    else
                        dr["END_DATE"] = DBNull.Value;

                    ccTable.Rows.Add(dr);
                }
            }
            return ccTable;
        }

        private static DataTable SetProductsCodesLocales(List<KeyValuePair<VerificationPaymentGateway, string>> ExternalProductCodes)
        {
            DataTable ccTable = new DataTable("ProductsCodesLocalesValues");

            ccTable.Columns.Add("PRODUCT_CODE", typeof(string));
            ccTable.Columns.Add("verification_payment_gateway_id", typeof(int));

            if (ExternalProductCodes != null)
            {
                DataRow dr = null;
                foreach (var productCode in ExternalProductCodes)
                {
                    dr = ccTable.NewRow();
                    dr["PRODUCT_CODE"] = productCode.Value;
                    dr["verification_payment_gateway_id"] = (int)productCode.Key;

                    ccTable.Rows.Add(dr);
                }
            }
            return ccTable;
        }

        public static bool Get_GroupUsageModuleCode(int groupID, string connKey, ref string groupUsageModuleCode)
        {
            bool res = false;
            StoredProcedure sp = new StoredProcedure("Get_GroupUsageModuleCode");
            sp.SetConnectionKey(!string.IsNullOrEmpty(connKey) ? connKey : "CONNECTION_STRING");
            sp.AddParameter("@GroupID", groupID);

            DataSet ds = sp.ExecuteDataSet();
            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
            {
                res = true;
                DataTable dt = ds.Tables[0];
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    groupUsageModuleCode = ODBCWrapper.Utils.GetSafeStr(dt.Rows[0]["USAGE_MODULE_CODE"]);
                }
            }

            return res;
        }

        public static DataTable Get_SubscriptionsServices(int groupID, List<long> subscriptionsIDs)
        {
            DataTable subscriptionsServices = null;
            StoredProcedure sp = new StoredProcedure("Get_SubscriptionsServices");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@GroupID", groupID);
            sp.AddIDListParameter<long>("@Subscriptions", subscriptionsIDs, "IDList");


            DataSet spResult = sp.ExecuteDataSet();

            // If stored procedure was succesful, get the first one
            if (spResult != null && spResult.Tables.Count == 1)
            {
                subscriptionsServices = spResult.Tables[0];
            }

            return subscriptionsServices;
        }

        public static DataTable Get_PPVModuleForMediaFiles(int groupID, List<int> mediaFileList)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_PPVModuleForMediaFiles");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@GroupID", groupID);
            sp.AddIDListParameter("@MediaFileList", mediaFileList, "id");

            DataSet ds = sp.ExecuteDataSet();

            if (ds != null)
                return ds.Tables[0];
            return null;
        }

        public static DataRow Get_PPVModuleForMediaFile(int mediaFileID, long ppvModuleCode, int groupID)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_PPVModuleForMediaFile");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@groupID", groupID);
            if (ppvModuleCode > 0)
            {
                sp.AddParameter("@ppvModule", ppvModuleCode);
            }
            sp.AddParameter("@MediaFile", mediaFileID);

            DataTable dt = sp.Execute();

            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                return dt.Rows[0];
            return null;
        }

        public static Dictionary<string, string> Get_SubscriptionsFromProductCodes(List<string> productCodes, int groupID)
        {
            Dictionary<string, string> ret = new Dictionary<string, string>();

            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_SubscriptionsFromProductCodes");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@GroupID", groupID);
            sp.AddIDListParameter("@ProductCodesList", productCodes, "STR");

            DataSet ds = sp.ExecuteDataSet();

            if (ds != null && ds.Tables != null && ds.Tables.Count > 0 && ds.Tables[0].Rows != null && ds.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    string productCode = ODBCWrapper.Utils.GetSafeStr(row["Product_Code"]);
                    string id = ODBCWrapper.Utils.GetSafeStr(row, "ID");
                    ret.Add(id, productCode);
                }
            }

            return ret;
        }

        public int InsertUsageModule(long userId, int groupID, string name, int maxViews, int fullLifeCycleID,
                        int viewLifeCycleID, int waiverPeriod, bool isWaiverEnabled, bool isOfflinePlayback)
        {
            try
            {
                StoredProcedure sp = new StoredProcedure("InsertUsageModule");
                sp.SetConnectionKey("pricing_connection");
                sp.AddParameter("@GroupID", groupID);
                sp.AddParameter("@UserId", userId);
                sp.AddParameter("@Name", name);
                sp.AddParameter("@MaxViews", maxViews);
                sp.AddParameter("@WaiverPeriod", waiverPeriod);
                sp.AddParameter("@IsWaiverEnabled", isWaiverEnabled);
                sp.AddParameter("@IsOfflinePlayback", isOfflinePlayback);
                sp.AddParameter("@FullLifeCycleID", fullLifeCycleID);
                sp.AddParameter("@ViewLifeCycleID", viewLifeCycleID);
                sp.AddParameter("@Date", DateTime.UtcNow);

                return sp.ExecuteReturnValue<int>();
            }
            catch (Exception ex)
            {
                HandleException(string.Empty, ex);
            }
            return 0;
        }

        public static Dictionary<string, string> Get_PPVsFromProductCodes(List<string> productCodes, int groupID)
        {
            Dictionary<string, string> ret = new Dictionary<string, string>();

            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_PPVsFromProductCodes");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@GroupID", groupID);
            sp.AddIDListParameter("@ProductCodesList", productCodes, "STR");

            DataSet ds = sp.ExecuteDataSet();

            if (ds != null && ds.Tables != null && ds.Tables.Count > 0 && ds.Tables[0].Rows != null && ds.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    string productCode = ODBCWrapper.Utils.GetSafeStr(row["Product_Code"]);
                    string id = ODBCWrapper.Utils.GetSafeStr(row, "ID");
                    ret.Add(id, productCode);
                }
            }

            return ret;
        }

        public static DataTable Get_PPVModulesData(int nGroupID, List<long> ppmModulesIDs)
        {
            ODBCWrapper.StoredProcedure spPPVModuleData = new ODBCWrapper.StoredProcedure("Get_PPVModulesData");
            spPPVModuleData.SetConnectionKey("pricing_connection");

            spPPVModuleData.AddParameter("@GroupID", nGroupID);
            spPPVModuleData.AddIDListParameter("@PPVModulesIDs", ppmModulesIDs, "ID");

            return spPPVModuleData.Execute();
        }

        public static int Insert_PriceCode(int groupID, string code, double price, int currencyID)
        {
            try
            {
                StoredProcedure sp = new StoredProcedure("Insert_PriceCode");
                sp.SetConnectionKey("pricing_connection");
                sp.AddParameter("@GroupID", groupID);
                sp.AddParameter("@PriceCode", code);
                sp.AddParameter("@Price", price);
                sp.AddParameter("@CurrencyID", currencyID);
                sp.AddParameter("@Date", DateTime.UtcNow);

                return sp.ExecuteReturnValue<int>();
            }
            catch (Exception ex)
            {
                HandleException(string.Empty, ex);
            }
            return 999;
        }

        public static int Update_PriceCode(int groupID, string code, double price, int currencyID)
        {
            try
            {
                StoredProcedure sp = new StoredProcedure("Update_PriceCode");
                sp.SetConnectionKey("pricing_connection");
                sp.AddParameter("@GroupID", groupID);
                sp.AddParameter("@PriceCode", code);
                sp.AddParameter("@Price", price);
                sp.AddParameter("@CurrencyID", currencyID);
                sp.AddParameter("@Date", DateTime.UtcNow);

                return sp.ExecuteReturnValue<int>();
            }
            catch (Exception ex)
            {
                HandleException(string.Empty, ex);
            }
            return 999;
        }

        public static int Delete_PriceCode(int groupID, string code)
        {
            try
            {
                StoredProcedure sp = new StoredProcedure("Delete_PriceCode");
                sp.SetConnectionKey("pricing_connection");
                sp.AddParameter("@GroupID", groupID);
                sp.AddParameter("@PriceCode", code);
                sp.AddParameter("@Date", DateTime.UtcNow);
                return sp.ExecuteReturnValue<int>();
            }
            catch (Exception ex)
            {
                HandleException(string.Empty, ex);
            }
            return 999;
        }

        private static void HandleException(string message, Exception ex)
        {
            log.Error(message, ex);
        }

        public static int Insert_NewDiscountCode(int groupID, string code, double price, int currencyID, double percent, int relationType, DateTime startDate, DateTime endDate, int algoType, int ntimes, string alias)
        {
            try
            {
                StoredProcedure sp = new StoredProcedure("Insert_NewDiscountCode");
                sp.SetConnectionKey("pricing_connection");
                sp.AddParameter("@GroupID", groupID);
                sp.AddParameter("@DiscountCode", code);
                sp.AddParameter("@Price", price);
                sp.AddParameter("@CurrencyID", currencyID);
                sp.AddParameter("@Percent", percent);
                sp.AddParameter("@RelationType", relationType);
                sp.AddParameter("@StartDate", startDate);
                sp.AddParameter("@EndDate", endDate);
                sp.AddParameter("@AlgoType", algoType);
                sp.AddParameter("@Ntimes", ntimes);
                sp.AddParameter("@Date", DateTime.UtcNow);
                sp.AddParameter("@alias", alias);

                return sp.ExecuteReturnValue<int>();
            }
            catch (Exception ex)
            {
                HandleException(string.Empty, ex);
            }
            return 999;
        }

        public static int InsertCouponGroup(int groupID, string groupName, int discountCode, DateTime startDate, DateTime endDate, int maxUseCountForCoupon, int maxRecurringUsesCountForCoupon, int financialEntityID, string alias)
        {
            try
            {
                StoredProcedure sp = new StoredProcedure("Insert_NewCouponGroup");
                sp.SetConnectionKey("pricing_connection");
                sp.AddParameter("@GroupID", groupID);
                sp.AddParameter("@GroupName", groupName);
                sp.AddParameter("@DiscountCode", discountCode);
                sp.AddParameter("@StartDate", startDate);
                sp.AddParameter("@EndDate", endDate);
                sp.AddParameter("@MaxUseCountForCoupon", maxUseCountForCoupon);
                sp.AddParameter("@MaxRecrringUses", maxRecurringUsesCountForCoupon);
                sp.AddParameter("@FinancialEntityID", financialEntityID);
                sp.AddParameter("@Date", DateTime.UtcNow);
                sp.AddParameter("@alias", alias);

                return sp.ExecuteReturnValue<int>();
            }
            catch (Exception ex)
            {
                HandleException(string.Empty, ex);
            }
            return 999;
        }

        public static int InsertUsageModule(int groupID, string virtualName, int viewLifeCycle, int maxUsageModuleLifeCycle, int maxNumberOfViews, bool waiver, int waiverPeriod, bool isOfflinePlayBack,
            int ext_discount_id, int internal_discount_id, int pricing_id, int coupon_id, int type, int subscription_only, int is_renew, int num_of_rec_periods, int device_limit_id)
        {
            try
            {
                StoredProcedure sp = new StoredProcedure("Insert_UsageModule");
                sp.SetConnectionKey("pricing_connection");
                sp.AddParameter("@GroupID", groupID);
                sp.AddParameter("@Name", virtualName);
                sp.AddParameter("@viewLifeCycle", viewLifeCycle);
                sp.AddParameter("@maxUsageModuleLifeCycle", maxUsageModuleLifeCycle);
                sp.AddParameter("@maxNumberOfViews", maxNumberOfViews);
                // multi usage module
                if (type == 2)
                {
                    sp.AddParameter("@ext_discount_id", ext_discount_id);
                    sp.AddParameter("@internal_discount_id", internal_discount_id);
                    sp.AddParameter("@pricing_id", pricing_id);
                    sp.AddParameter("@coupon_id", coupon_id);
                    sp.AddParameter("@type", type);
                    sp.AddParameter("@subscription_only", subscription_only);
                    sp.AddParameter("@is_renew", is_renew);
                    sp.AddParameter("@num_of_rec_periods", num_of_rec_periods);
                    sp.AddParameter("@device_limit_id", device_limit_id);
                }
                //Regulation cancelation
                sp.AddParameter("@waiver", waiver == true ? 1 : 0);
                sp.AddParameter("@waiverPeriod", waiverPeriod);

                sp.AddParameter("@isOfflinePlayBack", isOfflinePlayBack == true ? 1 : 0);
                sp.AddParameter("@Date", DateTime.UtcNow);

                return sp.ExecuteReturnValue<int>();
            }
            catch (Exception ex)
            {
                HandleException(string.Empty, ex);
            }
            return 999;
        }

        public static int InserPPVModule(int groupID, string ppvName, int usageModuleCode, string couponGroupCode, int discountModuleCode, int priceCode, bool subscriptionOnly, bool firstDeviceLimitation,
            Dictionary<string, List<string>> descriptionDict, string productCode, string alias)
        {
            try
            {
                StoredProcedure sp = new StoredProcedure("Insert_NewPPVModule");
                sp.SetConnectionKey("pricing_connection");
                sp.AddParameter("@GroupID", groupID);
                sp.AddParameter("@ppvName", ppvName);
                sp.AddParameter("@usageModuleCode", usageModuleCode);
                sp.AddParameter("@couponGroupCode", couponGroupCode);
                sp.AddParameter("@discountModuleCode", discountModuleCode);
                sp.AddParameter("@priceCode", priceCode);
                sp.AddParameter("@subscriptionOnly", subscriptionOnly == true ? 1 : 0);
                sp.AddParameter("@firstDeviceLimitation", firstDeviceLimitation == true ? 1 : 0);
                sp.AddParameter("@productCode", productCode);
                sp.AddKeyValueListParameter<string, string>("@descriptionDict", descriptionDict, "key", "value");
                sp.AddParameter("@Date", DateTime.UtcNow);
                sp.AddParameter("@alias", alias);

                return sp.ExecuteReturnValue<int>();
            }
            catch (Exception ex)
            {
                HandleException(string.Empty, ex);
            }
            return 999;
        }

        public static bool CheckAliasIsUniqe(int groupID, string alias, string tableName)
        {
            try
            {
                StoredProcedure sp = new StoredProcedure("Check_AliasIsUniqe");
                sp.SetConnectionKey("pricing_connection");
                sp.AddParameter("@groupID", groupID);
                sp.AddParameter("@alias", alias);
                sp.AddParameter("@tableName", tableName);

                return sp.ExecuteReturnValue<bool>();
            }
            catch (Exception ex)
            {
                HandleException(string.Empty, ex);
            }
            return false;
        }

        public long InsertPreviewModule(int groupID, string name, int fullLifeCycle, int nonRenewPeriod, long userId)
        {
            try
            {
                StoredProcedure sp = new StoredProcedure("Insert_PreviewModule");
                sp.SetConnectionKey("pricing_connection");
                sp.AddParameter("@GroupID", groupID);
                sp.AddParameter("@Name", name);
                sp.AddParameter("@FullLifeCycle", fullLifeCycle);
                sp.AddParameter("@NonRenewPeriod", nonRenewPeriod);
                sp.AddParameter("@UpdaterId", userId);

                return sp.ExecuteReturnValue<int>();
            }
            catch (Exception ex)
            {
                HandleException(string.Empty, ex);
            }
            return 999;
        }

        public static int UpdatetUsageModule(int groupID, string virtualName, int viewLifeCycle, int maxUsageModuleLifeCycle, int maxNumberOfViews, bool waiver, int waiverPeriod, bool isOfflinePlayBack,
            int ext_discount_id, int internal_discount_id, int pricing_id, int coupon_id, int type, int subscription_only, int is_renew, int num_of_rec_periods, int device_limit_id)
        {
            try
            {
                StoredProcedure sp = new StoredProcedure("Update_UsageModule");
                sp.SetConnectionKey("pricing_connection");
                sp.AddParameter("@GroupID", groupID);
                sp.AddParameter("@Name", virtualName);
                sp.AddParameter("@viewLifeCycle", viewLifeCycle);
                sp.AddParameter("@maxUsageModuleLifeCycle", maxUsageModuleLifeCycle);
                sp.AddParameter("@maxNumberOfViews", maxNumberOfViews);
                // multi usage module
                if (type == 2)
                {
                    sp.AddParameter("@ext_discount_id", ext_discount_id);
                    sp.AddParameter("@internal_discount_id", internal_discount_id);
                    sp.AddParameter("@pricing_id", pricing_id);
                    sp.AddParameter("@coupon_id", coupon_id);
                    sp.AddParameter("@type", type);
                    sp.AddParameter("@subscription_only", subscription_only);
                    sp.AddParameter("@is_renew", is_renew);
                    sp.AddParameter("@num_of_rec_periods", num_of_rec_periods);
                    sp.AddParameter("@device_limit_id", device_limit_id);
                }
                //Regulation cancelation
                sp.AddParameter("@waiver", waiver == true ? 1 : 0);
                sp.AddParameter("@waiverPeriod", waiverPeriod);

                sp.AddParameter("@isOfflinePlayBack", isOfflinePlayBack == true ? 1 : 0);
                sp.AddParameter("@Date", DateTime.UtcNow);

                return sp.ExecuteReturnValue<int>();
            }
            catch (Exception ex)
            {
                HandleException(string.Empty, ex);
            }
            return 999;
        }

        public static bool IsCodeUnique(int groupID, string code)
        {
            try
            {
                StoredProcedure sp = new StoredProcedure("Is_CodeUnique");
                sp.SetConnectionKey("pricing_connection");
                sp.AddParameter("@GroupID", groupID);
                sp.AddParameter("@Code", code);
                return sp.ExecuteReturnValue<bool>();
            }
            catch (Exception ex)
            {
                HandleException(string.Empty, ex);
            }
            return false;
        }

        public static DataTable ValidateMPP(int groupID, string code, string internalDiscount, List<string> pricePlansCodes, List<string> channels, List<string> fileTypes,
            string previewModule, List<string> couponGroups, List<string> verificationPGW)
        {
            StoredProcedure sp = new StoredProcedure("ValidateMPP");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@GroupID", groupID);
            sp.AddParameter("@Name", code);
            sp.AddParameter("@PreviewModule", previewModule);
            sp.AddParameter("@InternalDiscount", internalDiscount);
            sp.AddIDListParameter<string>("@PricePlansCodes", pricePlansCodes, "STR");
            sp.AddIDListParameter<string>("@Channels", channels, "STR");
            sp.AddIDListParameter<string>("@FileTypes", fileTypes, "STR");
            sp.AddIDListParameter<string>("@CouponGroups", couponGroups, "STR");
            sp.AddIDListParameter<string>("@VerificationPGW", verificationPGW, "STR");

            return sp.Execute();
        }

        public static int InsertMPP(int groupID, ApiObjects.IngestMultiPricePlan mpp, List<KeyValuePair<long, int>> pricePlansCodes, List<long> channels, List<long> fileTypes,
            int previewModuleID, int internalDiscountID, XmlDocument couponsGroups, XmlDocument productCodes)
        {
            try
            {
                StoredProcedure sp = new StoredProcedure("Insert_MPP");
                sp.SetConnectionKey("pricing_connection");
                sp.AddParameter("@GroupID", groupID);
                sp.AddParameter("@Name", mpp.Code);
                sp.AddParameter("@StartDate", mpp.StartDate);
                sp.AddParameter("@EndDate", mpp.EndDate);
                sp.AddParameter("@GracePeriodMinutes", mpp.GracePeriodMinutes);
                sp.AddParameter("@IsActive", mpp.IsActive);
                sp.AddParameter("@InternalDiscountID", internalDiscountID);
                sp.AddParameter("@PreviewModuleID", previewModuleID);
                sp.AddParameter("@IsRenewable", mpp.IsRenewable);
                sp.AddParameter("@ProductCode", mpp.ProductCode);
                if (mpp.Descriptions != null)
                {
                    sp.AddKeyValueListParameter<string, string>("@Description", mpp.Descriptions.Select(d => new KeyValuePair<string, string>(d.key, d.value)).ToList(), "key", "value");
                }
                if (mpp.Titles != null)
                {
                    sp.AddKeyValueListParameter<string, string>("@Title", mpp.Titles.Select(t => new KeyValuePair<string, string>(t.key, t.value)).ToList(), "key", "value");
                }
                sp.AddKeyValueListParameter<long, int>("@PricePlansCodes", pricePlansCodes, "key", "value");

                sp.AddIDListParameter<long>("@Channels", channels, "Id");
                sp.AddIDListParameter<long>("@FileTypes", fileTypes, "Id");
                sp.AddParameter("@Date", DateTime.UtcNow);
                sp.AddParameter("@OrderNum", mpp.OrderNumber);
                sp.AddParameter("@couponsGroups", couponsGroups.InnerXml);
                sp.AddParameter("@productCodes", productCodes.InnerXml);

                return sp.ExecuteReturnValue<int>();
            }
            catch (Exception ex)
            {
                HandleException(string.Empty, ex);
            }
            return 0;
        }
        public static int DeleteMPP(int groupID, string multiPricePlan)
        {
            try
            {
                StoredProcedure sp = new StoredProcedure("Delete_MPP");
                sp.SetConnectionKey("pricing_connection");
                sp.AddParameter("@GroupID", groupID);
                sp.AddParameter("@Date", DateTime.UtcNow);
                sp.AddParameter("@PricePlansCode", multiPricePlan);
                return sp.ExecuteReturnValue<int>();
            }
            catch (Exception ex)
            {
                HandleException(string.Empty, ex);
            }
            return 0;
        }

        public static int UpdateMPP(int groupID, ApiObjects.IngestMultiPricePlan mpp, List<KeyValuePair<long, int>> pricePlansCodes, List<long> channels, List<long> fileTypes,
            int previewModuleID, int internalDiscountID, XmlDocument couponsGroups, XmlDocument productCodes)
        {
            StoredProcedure sp = new StoredProcedure("Update_MPP");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@GroupID", groupID);
            sp.AddParameter("@Name", mpp.Code);
            sp.AddParameter("@StartDate", mpp.StartDate);
            sp.AddParameter("@EndDate", mpp.EndDate);
            sp.AddParameter("@GracePeriodMinutes", mpp.GracePeriodMinutes);
            sp.AddParameter("@IsActive", mpp.IsActive);
            if (internalDiscountID >= 0)
                sp.AddParameter("@InternalDiscountID", internalDiscountID);

            if (previewModuleID >= 0)
                sp.AddParameter("@PreviewModuleID", previewModuleID);

            sp.AddParameter("@IsRenewable", mpp.IsRenewable);
            sp.AddParameter("@ProductCode", mpp.ProductCode);

            if (mpp.Descriptions != null)
                sp.AddKeyValueListParameter<string, string>("@Description", mpp.Descriptions.Select(d => new KeyValuePair<string, string>(d.key, d.value)).ToList(), "key", "value");

            if (mpp.Titles != null)
                sp.AddKeyValueListParameter<string, string>("@Title", mpp.Titles.Select(t => new KeyValuePair<string, string>(t.key, t.value)).ToList(), "key", "value");

            sp.AddKeyValueListParameter<long, int>("@PricePlansCodes", pricePlansCodes, "key", "value");
            sp.AddIDListParameter<long>("@Channels", channels, "Id");
            sp.AddIDListParameter<long>("@FileTypes", fileTypes, "Id");
            sp.AddParameter("@Date", DateTime.UtcNow);
            sp.AddParameter("@OrderNum", mpp.OrderNumber);

            sp.AddParameter("@couponsGroups", couponsGroups.InnerXml);
            sp.AddParameter("@xmlDocRowCount", (couponsGroups.ChildNodes != null && couponsGroups.FirstChild != null && couponsGroups.FirstChild.ChildNodes != null && couponsGroups.FirstChild.ChildNodes.Count > 0) ? 1 : 0);

            sp.AddParameter("@productCodes", productCodes.InnerXml);
            sp.AddParameter("@xmlDocRowCountProductCodes", (productCodes.ChildNodes != null && productCodes.FirstChild != null && productCodes.FirstChild.ChildNodes != null && productCodes.FirstChild.ChildNodes.Count > 0) ? 1 : 0);

            return sp.ExecuteReturnValue<int>(); ;
        }

        public static DataTable ValidatePricePlan(int groupID, string code, string fullLifeCycle, string viewLifeCycle, string currency, double? price, string discount)
        {
            StoredProcedure sp = new StoredProcedure("ValidatePricePlan");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@GroupID", groupID);
            sp.AddParameter("@Name", code);
            sp.AddParameter("@FullLifeCycle", fullLifeCycle);
            sp.AddParameter("@ViewLifeCycle", viewLifeCycle);
            sp.AddParameter("@currency", currency);
            sp.AddParameter("@price", price);
            sp.AddParameter("@Discount", discount); ;

            return sp.Execute();
        }

        //todo? IngestPricePlan?
        public int InsertPricePlan(int groupId, ApiObjects.IngestPricePlan pricePlan, int priceCodeId, int fullLifeCycleID, int viewLifeCycleID, int discountID)
        {
            try
            {
                StoredProcedure sp = new StoredProcedure("Insert_PricePlan");
                sp.SetConnectionKey("pricing_connection");
                sp.AddParameter("@GroupID", groupId);
                sp.AddParameter("@Name", pricePlan.Code);
                sp.AddParameter("@IsActive", pricePlan.IsActive);
                sp.AddParameter("@MaxViews", pricePlan.MaxViews);
                sp.AddParameter("@IsRenewable", pricePlan.IsRenewable);
                sp.AddParameter("@RecurringPeriods", pricePlan.RecurringPeriods);
                sp.AddParameter("@PricCodeID", priceCodeId);
                sp.AddParameter("@FullLifeCycleID", fullLifeCycleID);
                sp.AddParameter("@ViewLifeCycleID", viewLifeCycleID);
                sp.AddParameter("@DiscountID", discountID);
                sp.AddParameter("@Date", DateTime.UtcNow);

                return sp.ExecuteReturnValue<int>();
            }
            catch (Exception ex)
            {
                HandleException(string.Empty, ex);
            }
            return 0;
        }

        public static int UpdatePricePlan(int groupID, ApiObjects.IngestPricePlan pricePlan, int priceCodeID, int fullLifeCycleID, int viewLifeCycleID, int discountID)
        {
            try
            {
                StoredProcedure sp = new StoredProcedure("Update_PricePlan");
                sp.SetConnectionKey("pricing_connection");
                sp.AddParameter("@GroupID", groupID);
                sp.AddParameter("@Name", pricePlan.Code);

                sp.AddParameter("@IsActive", pricePlan.IsActive);
                sp.AddParameter("@MaxViews", pricePlan.MaxViews);

                sp.AddParameter("@RecurringPeriods", pricePlan.RecurringPeriods);

                if (priceCodeID >= 0)
                    sp.AddParameter("@PriceCodeID", priceCodeID);

                if (fullLifeCycleID >= 0)
                    sp.AddParameter("@FullLifeCycleID", fullLifeCycleID);

                if (viewLifeCycleID >= 0)
                    sp.AddParameter("@ViewLifeCycleID", viewLifeCycleID);

                if (discountID >= 0)
                    sp.AddParameter("@DiscountID", discountID);

                sp.AddParameter("@IsRenewable", pricePlan.IsRenewable);
                sp.AddParameter("@Date", DateTime.UtcNow);

                return sp.ExecuteReturnValue<int>();
            }
            catch (Exception ex)
            {
                HandleException(string.Empty, ex);
            }
            return 0;
        }

        public static int DeletePricePlan(int groupID, string pricePlan)
        {
            try
            {
                StoredProcedure sp = new StoredProcedure("Delete_PricePlan");
                sp.SetConnectionKey("pricing_connection");
                sp.AddParameter("@GroupID", groupID);
                sp.AddParameter("@Date", DateTime.UtcNow);
                sp.AddParameter("@PricePlan", pricePlan);
                return sp.ExecuteReturnValue<int>();
            }
            catch (Exception ex)
            {
                HandleException(string.Empty, ex);
            }
            return 0;
        }

        public static DataTable ValidatePPV(int groupID, string code, string currency, double? price, string usageModule, string discount, string couponGroup, List<string> fileTypes)
        {
            StoredProcedure sp = new StoredProcedure("ValidatePPV");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@GroupID", groupID);
            sp.AddParameter("@Name", code);
            sp.AddParameter("@currency", currency);
            sp.AddParameter("@price", price);
            sp.AddParameter("@usageModule", usageModule);
            sp.AddParameter("@discount", discount);
            sp.AddParameter("@couponGroup", couponGroup);
            sp.AddIDListParameter<string>("@FileTypes", fileTypes, "STR");

            return sp.Execute();
        }

        public static int InsertPPV(int groupID, ApiObjects.IngestPPV ppv, int priceCodeID, int usageModuleID, int discountID, int couponGroupID, List<long> fileTypes)
        {
            StoredProcedure sp = new StoredProcedure("Insert_PPVModule");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@GroupID", groupID);
            sp.AddParameter("@Name", ppv.Code);
            sp.AddParameter("@priceCode", priceCodeID);
            sp.AddParameter("@usageModule", usageModuleID);
            sp.AddParameter("@discount", discountID);
            sp.AddParameter("@couponGroup", couponGroupID);
            sp.AddParameter("@subscriptionOnly", ppv.SubscriptionOnly);
            sp.AddParameter("@firstDeviceLimitation", ppv.FirstDeviceLimitation);
            sp.AddParameter("@productCode", ppv.ProductCode);
            sp.AddIDListParameter<long>("@FileTypes", fileTypes, "Id");
            sp.AddParameter("@IsActive", ppv.IsActive);
            sp.AddParameter("@Date", DateTime.UtcNow);

            if (ppv.Descriptions != null)
            {
                sp.AddKeyValueListParameter<string, string>("@Description", ppv.Descriptions.Select(d => new KeyValuePair<string, string>(d.key, d.value)).ToList(), "key", "value");
            }

            return sp.ExecuteReturnValue<int>();
        }

        public static int UpdatePPV(int groupID, ApiObjects.IngestPPV ppv, int priceCodeID, int usageModuleID, int discountID, int couponGroupID, List<long> fileTypes)
        {
            StoredProcedure sp = new StoredProcedure("Update_PPVModule");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@GroupID", groupID);
            sp.AddParameter("@Name", ppv.Code);
            if (priceCodeID >= 0)
                sp.AddParameter("@priceCode", priceCodeID);

            if (usageModuleID >= 0)
                sp.AddParameter("@usageModule", usageModuleID);

            if (discountID >= 0)
                sp.AddParameter("@discount", discountID);

            if (couponGroupID >= 0)
                sp.AddParameter("@couponGroup", couponGroupID);

            sp.AddParameter("@subscriptionOnly", ppv.SubscriptionOnly);
            sp.AddParameter("@firstDeviceLimitation", ppv.FirstDeviceLimitation);
            sp.AddParameter("@productCode", ppv.ProductCode);
            sp.AddParameter("@IsActive", ppv.IsActive);
            sp.AddIDListParameter<long>("@FileTypes", fileTypes, "Id");
            sp.AddParameter("@Date", DateTime.UtcNow);
            if (ppv.Descriptions != null)
            {
                sp.AddKeyValueListParameter<string, string>("@Description", ppv.Descriptions.Select(d => new KeyValuePair<string, string>(d.key, d.value)).ToList(), "key", "value");
            }
            return sp.ExecuteReturnValue<int>();
        }

        public static int DeletePPV(int groupID, string ppv)
        {
            try
            {
                StoredProcedure sp = new StoredProcedure("Delete_PPV");
                sp.SetConnectionKey("pricing_connection");
                sp.AddParameter("@GroupID", groupID);
                sp.AddParameter("@Date", DateTime.UtcNow);
                sp.AddParameter("@PPV", ppv);
                return sp.ExecuteReturnValue<int>();
            }
            catch (Exception ex)
            {
                HandleException(string.Empty, ex);
            }
            return 0;
        }

        public static int InsertPriceCode(int groupID, int currencyID, double? price, string code)
        {
            StoredProcedure sp = new StoredProcedure("Insert_PriceCode");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@GroupID", groupID);
            sp.AddParameter("@CurrencyID", currencyID);
            sp.AddParameter("@Code", code);
            sp.AddParameter("@Price", price);
            return sp.ExecuteReturnValue<int>();
        }

        public static List<int> GetGiftCardReminders(int groupID)
        {
            List<int> res = null;
            try
            {
                StoredProcedure sp = new StoredProcedure("Get_GiftCardReminders");
                sp.SetConnectionKey("pricing_connection");
                sp.AddParameter("@GroupID", groupID);


                DataTable dt = sp.Execute();
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    res = dt.AsEnumerable().Select(x => x.Field<int>("NUMBER_OF_DAYS")).ToList();
                }
                else
                {
                    res = new List<int>(0);
                }
            }
            catch (Exception ex)
            {
                HandleException(string.Empty, ex);
            }
            return res;
        }

        public static DataSet GetPriceCodeLocale(int priceCodeId, string countryCode, string currencyCode)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("GetPriceCodeLocale");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@priceCodeId", priceCodeId);
            sp.AddParameter("@countryCode", countryCode);
            sp.AddParameter("@currencyCode", currencyCode);
            return sp.ExecuteDataSet();
        }

        public static DataSet GetDiscountModuleLocale(int discountCodeId, string countryCode, string currencyCode)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("GetDiscountModuleLocale");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@discountCodeId", discountCodeId);
            sp.AddParameter("@countryCode", countryCode);
            sp.AddParameter("@currencyCode", currencyCode);
            return sp.ExecuteDataSet();
        }

        public static bool RemoveFileTypesAndPpvsFromAssets(List<int> assetIds, LifeCycleFileTypesAndPpvsTransitions fileTypesAndPpvsToRemove)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("RemoveFileTypesAndPpvsFromAssets");
            sp.SetConnectionKey("pricing_connection");
            sp.AddIDListParameter("@AssetIds", assetIds, "id");
            sp.AddIDListParameter("@FileTypeIds", fileTypesAndPpvsToRemove.FileTypeIds.ToList(), "id");
            sp.AddIDListParameter("@PpvIds", fileTypesAndPpvsToRemove.PpvIds.ToList(), "id");
            return sp.ExecuteReturnValue<int>() > 0;
        }

        public static bool AddFileTypesAndPpvsToAssets(List<int> assetIds, LifeCycleFileTypesAndPpvsTransitions fileTypesAndPpvsToAdd)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("AddFileTypesAndPpvsToAssets");
            sp.SetConnectionKey("pricing_connection");
            sp.AddIDListParameter("@AssetIds", assetIds, "id");
            sp.AddIDListParameter("@FileTypeIds", fileTypesAndPpvsToAdd.FileTypeIds.ToList(), "id");
            sp.AddIDListParameter("@PpvIds", fileTypesAndPpvsToAdd.PpvIds.ToList(), "id");
            return sp.ExecuteReturnValue<int>() > 0;
        }

        public static DataTable GetGroupAdsControlParams(int groupID)
        {
            StoredProcedure sp = new StoredProcedure("GetGroupAdsControlParams");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@groupId", groupID);

            return sp.Execute();
        }

        public static bool IsCouponGroupExsits(int m_nGroupID, long couponGroupId)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Is_CouponGroupExsitsById");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@groupID", m_nGroupID);
            sp.AddParameter("@couponGroupID", couponGroupId);
            return sp.ExecuteReturnValue<int>() > 0;
        }

        public static DataTable InsertCoupons(System.Xml.XmlDocument xmlDoc)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Insert_NewCoupons");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@xmlDoc", xmlDoc.InnerXml);
            return sp.Execute();
        }

        public static DataTable Get_SubscriptionsCouponGroup(int groupID, List<long> list)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_SubscriptionsCouponGroup");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@GroupID", groupID);
            sp.AddIDListParameter("@Subscriptions", list, "id");
            return sp.Execute();
        }

        public static long Get_CouponGroupId(int groupID, string couponCode)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_CouponGroupId");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@GroupID", groupID);
            sp.AddParameter("@Code", couponCode);
            return sp.ExecuteReturnValue<long>();
        }

        public static DataTable Get_SubscriptionsCouponGroupWithExpiry(int groupID, List<long> list)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_SubscriptionsCouponGroupWithExpiry");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@GroupID", groupID);
            sp.AddIDListParameter("@Subscriptions", list, "id");
            return sp.Execute();
        }

        public static DataTable GetSetsContainingSubscriptionIds(int groupId, List<long> subscriptionIds, int type = 0)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("GetSetsContainingSubscriptionIds");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@GroupId", groupId);
            sp.AddIDListParameter("@SubscriptionIds", subscriptionIds, "id");
            sp.AddParameter("@Type", type);
            return sp.Execute();
        }

        public static DataTable Get_SubscriptionsExternalProductCodes(int groupId, List<long> list)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_SubscriptionsExternalProductCodes");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@GroupID", groupId);
            sp.AddIDListParameter("@Subscriptions", list, "id");
            return sp.Execute();
        }

        public static bool Update_SubscriptionsProductCodes(int groupId, int subscriptionId, string xml)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Update_SubscriptionsProductCodes");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@GroupId", groupId);
            sp.AddParameter("@SubscriptionId", subscriptionId);
            sp.AddParameter("@xmlDoc", xml);

            return sp.ExecuteReturnValue<int>() > 0;
        }

        public static DataSet GetSubscriptionSetsByIds(int groupId, List<long> ids, int? typeId)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("GetSubscriptionSetsByIds");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@GroupId", groupId);
            sp.AddIDListParameter("@Ids", ids, "id");
            sp.AddParameter("@IdsExist", ids != null && ids.Count > 0);
            if (typeId.HasValue)
            {
                sp.AddParameter("@Type", typeId.Value);
            }
            return sp.ExecuteDataSet();
        }

        public static DataSet InsertSubscriptionSet(int groupId, string name, List<KeyValuePair<long, int>> subscriptionIdsToPriority)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("InsertSubscriptionSet");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@GroupId", groupId);
            sp.AddParameter("@Name", name);
            sp.AddKeyValueListParameter<long, int>("@SubscriptionsIdsToPriority", subscriptionIdsToPriority, "key", "value");
            sp.AddParameter("@SubscriptionsIdsToPriorityExist", subscriptionIdsToPriority != null && subscriptionIdsToPriority.Count > 0);
            return sp.ExecuteDataSet();
        }


        public static DataSet UpdateSubscriptionSet(int groupId, long setId, string name, List<KeyValuePair<long, int>> subscriptionIdsToPriority, bool shouldUpdateSubscriptionIds)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("UpdateSubscriptionSet");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@GroupId", groupId);
            sp.AddParameter("@SetId", setId);
            sp.AddParameter("@Name", string.IsNullOrEmpty(name) ? null : name);
            sp.AddKeyValueListParameter<long, int>("@SubscriptionsIdsToPriority", subscriptionIdsToPriority, "key", "value");
            sp.AddParameter("@SubscriptionsIdsToPriorityExist", shouldUpdateSubscriptionIds);
            return sp.ExecuteDataSet();
        }

        public static bool DeleteSubscriptionSet(int groupId, long setId)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("DeleteSubscriptionSet");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@GroupId", groupId);
            sp.AddParameter("@SetId", setId);

            return sp.ExecuteReturnValue<int>() > 0;
        }

        public static DataTable GetSetsContainingBaseSubscription(int groupId, List<long> subscriptionIds, int type = 1)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("GetSetsContainingBaseSubscription");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@GroupId", groupId);
            sp.AddIDListParameter("@SubscriptionIds", subscriptionIds, "id");
            sp.AddParameter("@Type", type);
            return sp.Execute();
        }

        public static DataSet InsertSubscriptionDependencySet(int groupId, string name, long baseSubscriptionId, List<KeyValuePair<long, int>> subscriptionIdsToPriority, int setType = 1)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("InsertSubscriptionDependencySet");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@GroupId", groupId);
            sp.AddParameter("@Name", name);
            sp.AddParameter("@BaseSubscriptionId", baseSubscriptionId);
            sp.AddParameter("@Type", setType);
            sp.AddKeyValueListParameter<long, int>("@SubscriptionsIdsToPriority", subscriptionIdsToPriority, "key", "value");
            sp.AddParameter("@SubscriptionsIdsToPriorityExist", subscriptionIdsToPriority != null && subscriptionIdsToPriority.Count > 0);
            return sp.ExecuteDataSet();
        }

        public static DataSet UpdateSubscriptionDependencySet(int groupId, long setId, string name, long baseSubscriptionId, List<KeyValuePair<long, int>> subscriptionIdsToPriority,
            bool shouldUpdateSubscriptionIds, int type = 1)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("UpdateSubscriptionDependencySet");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@GroupId", groupId);
            sp.AddParameter("@SetId", setId);
            sp.AddParameter("@Name", string.IsNullOrEmpty(name) ? null : name);
            sp.AddKeyValueListParameter<long, int>("@SubscriptionsIdsToPriority", subscriptionIdsToPriority, "key", "value");
            sp.AddParameter("@SubscriptionsIdsToPriorityExist", shouldUpdateSubscriptionIds);
            sp.AddParameter("@Type", type);
            sp.AddParameter("@BaseSubscriptionId", baseSubscriptionId);

            return sp.ExecuteDataSet();
        }

        public DataTable GetPricePlans(int groupId, List<long> pricePlanIds = null)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("GetPricePlans");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@groupId", groupId);
            sp.AddIDListParameter("@usageModulesIds", pricePlanIds, "ID");
            sp.AddParameter("@shouldGetAll", pricePlanIds == null || pricePlanIds.Count == 0 ? 1 : 0);
            return sp.Execute();
        }

        public List<PriceDetailsDTO> GetPriceCodesDTO(int groupId)
        {
            List<PriceDetailsDTO> priceDetailsDTOList = null;

            var parameters = new Dictionary<string, object>() { { "@groupId", groupId } };
            var ds = UtilsDal.ExecuteDataSet("GetGroupPriceCodes", parameters, "pricing_connection");
            if (ds?.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                priceDetailsDTOList = BuildPriceCodesFromDataTable(ds.Tables[0]);
            }

            return priceDetailsDTOList;
        }

        public bool IsPriceCodeExistsById(int groupId, long id)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("IsPriceCodeExistsById");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@groupId", groupId);
            sp.AddParameter("@id", id);
            return sp.ExecuteReturnValue<long>() > 0;
        }

        public bool IsPreviewModuleExsitsd(int groupId, long id)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Is_PreviewModuleExsits");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@groupId", groupId);
            sp.AddParameter("@id", id);
            return sp.ExecuteReturnValue<long>() > 0;
        }

        public bool IsUsageModuleExistsById(int groupId, long id)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("IsUsageModuleExistsById");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@groupId", groupId);
            sp.AddParameter("@id", id);
            return sp.ExecuteReturnValue<long>() > 0;
        }

        public bool UpdatePricePlanAndSubscriptionsPriceCode(int groupId, int pricePlanId, int priceCodeId)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("UpdatePricePlanAndSubscriptiopnsPriceCode");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@groupId", groupId);
            sp.AddParameter("@pricePlanId", pricePlanId);
            sp.AddParameter("@priceCodeId", priceCodeId);
            return sp.ExecuteReturnValue<int>() > 0;
        }

        public static DataTable GetCollectionsChannels(int groupId)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("GetCollectionsChannels");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@GroupID", groupId);
            return sp.Execute();
        }

        public static DataTable Get_ExternalProductCodes(int groupId, List<long> productIds, ApiObjects.eTransactionType productType)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_ExternalProductCodes");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@groupId", groupId);
            sp.AddIDListParameter<long>("@productIds", productIds, "ID");
            sp.AddParameter("@productType", (int)productType);
            return sp.Execute();
        }

        public static bool Update_ExternalProductCodes(int groupId, long productId, ApiObjects.eTransactionType productType, string xml)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Update_ExternalProductCodes");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@groupId", groupId);
            sp.AddParameter("@productId", productId);
            sp.AddParameter("@productType", (int)productType);
            sp.AddParameter("@xmlDoc", xml);

            return sp.ExecuteReturnValue<int>() > 0;
        }

        public static DataTable Get_ProductsCouponGroup(int groupId, List<long> productIds, ApiObjects.eTransactionType productType)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_ProductsCouponGroup");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@groupId", groupId);
            sp.AddIDListParameter<long>("@productIds", productIds, "ID");
            sp.AddParameter("@productType", (int)productType);
            return sp.Execute();
        }

        public static DataTable GetCoupon(int groupId, string couponCode)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("GetCoupon");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@groupId", groupId);
            sp.AddParameter("@couponCode", couponCode);
            return sp.Execute();
        }

        // possible duplicate with Insert_NewCouponUse
        public static void SetCouponUsed(int couponId, int nGroupID, string sSiteGUID, int nCollectionCode,
            int nMediaFileID, int nSubCode, int nPrePaidCode, long domainId, bool doReduce = false)
        {
            DirectQuery directQuery = null;
            InsertQuery insertQuery = null;
            try
            {
                directQuery = new DirectQuery();
                directQuery.SetConnectionKey("pricing_connection");
                if (!doReduce)
                {
                    directQuery += "update coupons set USE_COUNT=USE_COUNT+1, LAST_USED_DATE=getdate() where ";
                }
                else
                {
                    directQuery += "update coupons set USE_COUNT=USE_COUNT-1, LAST_USED_DATE=getdate() where ";
                }
                directQuery += NEW_PARAM("ID", "=", couponId);
                directQuery.Execute();


                insertQuery = new InsertQuery("coupon_uses");
                insertQuery.SetConnectionKey("pricing_connection");
                insertQuery += NEW_PARAM("SITE_GUID", "=", sSiteGUID);
                insertQuery += NEW_PARAM("COUPON_ID", "=", couponId);
                insertQuery += NEW_PARAM("group_id", "=", nGroupID);
                insertQuery += NEW_PARAM("MEDIA_FILE_ID", "=", nMediaFileID);
                insertQuery += NEW_PARAM("SUBSCRIPTION_CODE", "=", nSubCode);
                insertQuery += NEW_PARAM("PRE_PAID_CODE", "=", nPrePaidCode);
                insertQuery += NEW_PARAM("COLLECTION_CODE", "=", nCollectionCode);
                insertQuery += NEW_PARAM("DOMAIN_ID", "=", domainId);
                insertQuery.Execute();
            }
            finally
            {
                directQuery?.Finish();
                insertQuery?.Finish();
            }
        }

        public static DataTable GetCouponsGroup(int groupId, long couponsGroupId)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("GetCouponsGroup");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@groupId", groupId);
            sp.AddParameter("@couponsGroupId", couponsGroupId);
            return sp.Execute();
        }

        public static int GetCouponDomainUses(int groupId, int couponId, long domainId)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("GetCouponDomainUses");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@groupId", groupId);
            sp.AddParameter("@couponId", couponId);
            sp.AddParameter("@domainId", domainId);
            return sp.ExecuteReturnValue<int>();
        }

        public static bool IsCouponCodeExists(int groupId, string couponCode)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Is_CouponCodeExists");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@groupId", groupId);
            sp.AddParameter("@couponCode", couponCode);
            return sp.ExecuteReturnValue<int>() > 0;
        }

        public static DataTable GetGroupCouponsGroups(int groupId)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("GetGroupCouponsGroups");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@groupId", groupId);
            return sp.Execute();
        }

        public static DataTable UpdateCouponsGroup(int groupId, long id, string name, DateTime? startDate, DateTime? endDate, int? maxUsesNumber,
            int? maxUsesNumberOnRenewableSub, int? maxHouseholdUses, CouponGroupType? couponGroupType, long? discountCode)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Update_CouponsGroups");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@groupId", groupId);
            sp.AddParameter("@id", id);
            sp.AddParameter("@name", name);
            sp.AddParameter("@startDate", startDate);
            sp.AddParameter("@endDate", endDate);
            sp.AddParameter("@maxUsesNumber", maxUsesNumber);
            sp.AddParameter("@maxUsesNumberOnRenewableSub", maxUsesNumberOnRenewableSub);
            sp.AddParameter("@maxHouseholdUses", maxHouseholdUses);
            sp.AddParameter("@couponGroupType", couponGroupType);
            sp.AddParameter("@discountCode", discountCode);
            DataSet ds = sp.ExecuteDataSet();

            if (ds != null)
                return ds.Tables[0];
            return null;
        }

        public static int DeleteCouponsGroup(int groupId, long id)
        {
            try
            {
                StoredProcedure sp = new StoredProcedure("Delete_CouponsGroups");
                sp.SetConnectionKey("pricing_connection");
                sp.AddParameter("@groupId", groupId);
                sp.AddParameter("@id", id);
                return sp.ExecuteReturnValue<int>();
            }
            catch (Exception ex)
            {
                HandleException(string.Empty, ex);
            }
            return 0;
        }

        public static DataTable AddCouponsGroup(int groupId, string name, DateTime? startDate, DateTime? endDate, int? maxUsesNumber,
            int? maxUsesNumberOnRenewableSub, int? maxHouseholdUses, CouponGroupType? couponGroupType, long? discountCode)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Insert_CouponsGroups");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@groupId", groupId);
            sp.AddParameter("@name", name);
            sp.AddParameter("@startDate", startDate);
            sp.AddParameter("@endDate", endDate);
            sp.AddParameter("@maxUsesNumber", maxUsesNumber);
            sp.AddParameter("@maxUsesNumberOnRenewableSub", maxUsesNumberOnRenewableSub);
            sp.AddParameter("@maxHouseholdUses", maxHouseholdUses);
            sp.AddParameter("@couponGroupType", couponGroupType);
            sp.AddParameter("@discountCode", discountCode);
            DataSet ds = sp.ExecuteDataSet();

            if (ds != null)
                return ds.Tables[0];
            return null;
        }

        public bool IsDiscountCodeExists(int groupId, long discountCode)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Is_DiscountCodeExists");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@groupId", groupId);
            sp.AddParameter("@discountCodeId", discountCode);
            return sp.ExecuteReturnValue<int>() > 0;
        }

        public static DataTable GetGroupDiscounts(int groupId)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("GetGroupDiscounts");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@groupId", groupId);
            return sp.Execute();
        }

        public static DataTable AddAssetFilePPV(int groupId, long mediaFileId, long ppvModuleId, DateTime? startDate, DateTime? endDate)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("InsertPpvModulesMediaFiles");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@groupId", groupId);
            sp.AddParameter("@mediaFileId", mediaFileId);
            sp.AddParameter("@ppvModuleId", ppvModuleId);
            sp.AddParameter("@startDate", startDate);
            sp.AddParameter("@endDate", endDate);
            DataSet ds = sp.ExecuteDataSet();

            if (ds != null)
                return ds.Tables[0];
            return null;
        }

        public static DataTable UpdateAssetFilePPV(int groupId, long mediaFileId, long ppvModuleId, NullableObj<DateTime?> startDate, NullableObj<DateTime?> endDate)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("UpdatePpvModulesMediaFiles");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@groupId", groupId);
            sp.AddParameter("@mediaFileId", mediaFileId);
            sp.AddParameter("@ppvModuleId", ppvModuleId);
            sp.AddParameter("@startDate", startDate.Obj);
            sp.AddParameter("@endDate", endDate.Obj);
            sp.AddParameter("@nullableStartDate", startDate.IsNull);
            sp.AddParameter("@nullableEndDate", endDate.IsNull);

            DataSet ds = sp.ExecuteDataSet();

            if (ds != null)
                return ds.Tables[0];
            return null;
        }

        public static int DeleteAssetFilePPV(int groupId, long mediaFileId, long ppvModuleId)
        {
            try
            {
                StoredProcedure sp = new StoredProcedure("DeletePpvModulesMediaFiles");
                sp.SetConnectionKey("pricing_connection");
                sp.AddParameter("@groupId", groupId);
                sp.AddParameter("@mediaFileId", mediaFileId);
                sp.AddParameter("@ppvModuleId", ppvModuleId);
                return sp.ExecuteReturnValue<int>();
            }
            catch (Exception ex)
            {
                HandleException(string.Empty, ex);
            }
            return 0;
        }

        public static HashSet<long> GetCollectionIds(int groupId)
        {
            HashSet<long> res = null;

            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_CollectionsIds");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@groupId", groupId);
            DataTable dt = sp.Execute();

            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
            {
                res = new HashSet<long>();
                foreach (DataRow item in dt.Rows)
                {
                    res.Add(ODBCWrapper.Utils.GetLongSafeVal(item, "ID"));
                }
            }

            return res;
        }

        public static HashSet<long> GetSubscriptions(int groupId)
        {
            HashSet<long> res = null;

            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_SubscriptionsIds");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@groupId", groupId);
            DataTable dt = sp.Execute();

            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
            {
                res = new HashSet<long>();
                foreach (DataRow item in dt.Rows)
                {
                    res.Add(ODBCWrapper.Utils.GetLongSafeVal(item, "ID"));
                }
            }

            return res;
        }

        public static List<CouponWallet> GetHouseholdCouponWalletCB(long householdId)
        {
            string key = GetCouponWalletKey(householdId);
            return UtilsDal.GetObjectFromCB<List<CouponWallet>>(eCouchbaseBucket.OTT_APPS, key);
        }

        public static bool SaveHouseholdCouponWalletCB(long householdId, List<CouponWallet> couponWalletList)
        {
            string key = GetCouponWalletKey(householdId);
            return UtilsDal.SaveObjectInCB<List<CouponWallet>>(eCouchbaseBucket.OTT_APPS, key, couponWalletList, true);
        }

        private static string GetCouponWalletKey(long householdId)
        {
            return string.Format("household_coupon_wallet:{0}", householdId);
        }

        #region Campaign

        public T AddCampaign<T>(T campaign, ContextData contextData) where T : Campaign, new()
        {
            campaign.UpdaterId = contextData.UserId.Value;

            var sp = new StoredProcedure("Insert_Campaign");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@groupId", contextData.GroupId);
            sp.AddParameter("@startDate", campaign.StartDate);
            sp.AddParameter("@endDate", campaign.EndDate);
            sp.AddParameter("@has_promotion", campaign.Promotion != null ? 1 : 0);
            sp.AddParameter("@type", (int)campaign.CampaignType);
            sp.AddParameter("@campaign_json", JsonConvert.SerializeObject(campaign));

            var ds = sp.ExecuteDataSet();
            if (ds?.Tables?.Count > 0 && ds.Tables[0].Rows?.Count > 0)
            {
                var dr = ds.Tables[0].Rows[0];
                var response = JsonConvert.DeserializeObject<T>(Utils.GetSafeStr(dr, "campaign_json"));
                if (response != null)
                {
                    response.Id = Utils.GetLongSafeVal(dr, "ID");
                }

                return response;
            }

            return null;
        }

        public bool Update_Campaign(Campaign campaign, ContextData contextData)
        {
            campaign.UpdaterId = contextData.UserId.Value;

            var sp = new StoredProcedure("Update_Campaign");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@id", campaign.Id);
            sp.AddParameter("@groupId", contextData.GroupId);
            sp.AddParameter("@startDate", campaign.StartDate);
            sp.AddParameter("@endDate", campaign.EndDate);
            sp.AddParameter("@has_promotion", campaign.Promotion != null ? 1 : 0);
            sp.AddParameter("@state", (int)campaign.State);
            sp.AddParameter("@campaign_json", JsonConvert.SerializeObject(campaign));

            return sp.ExecuteReturnValue<int>() > 0;
        }

        public bool DeleteCampaign(long groupId, long campaignId)
        {
            var sp = new StoredProcedure("Delete_Campaign");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@id", campaignId);
            sp.AddParameter("@groupId", groupId);

            return sp.ExecuteReturnValue<int>() > 0;
        }

        public List<CampaignDB> GetCampaignsByGroupId(int groupId, eCampaignType campaignType)
        {
            var sp = new StoredProcedure("Get_CampaignsByGroupId");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@groupId", groupId);
            sp.AddParameter("@campaignType", (int)campaignType);
            return sp.ExecuteDataSet().Tables[0].ToList<CampaignDB>();
        }

        public Campaign GetCampaignById(int groupId, long id)
        {
            var sp = new StoredProcedure("Get_CampaignById");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@groupId", groupId);
            sp.AddParameter("@ID", id);

            Campaign response = null;

            var tb = sp.ExecuteDataSet().Tables[0];
            if (tb?.Rows != null && tb.Rows.Count > 0 && tb.Rows[0] != null)
            {
                var dr = tb.Rows[0];
                var type = Utils.GetIntSafeVal(dr, "type");
                if (type == (int)eCampaignType.Trigger)
                {
                    var triggerCampaign = JsonConvert.DeserializeObject<TriggerCampaign>(Utils.GetSafeStr(dr, "campaign_json"));
                    triggerCampaign.Id = Utils.GetLongSafeVal(dr, "id");
                    response = triggerCampaign;
                }
                else if (type == (int)eCampaignType.Batch)
                {
                    var batchCampaign = JsonConvert.DeserializeObject<BatchCampaign>(Utils.GetSafeStr(dr, "campaign_json"));
                    batchCampaign.Id = Utils.GetLongSafeVal(dr, "id");
                    response = batchCampaign;
                }
            }

            return response;
        }

        #endregion

        public long InsertPriceDetails(int groupId, string code, double price, long currencyId, List<PriceDTO> priceCodesLocales, long userId)
        {
            try
            {
                DataTable priceCodesLocalesDt = SetPriceCodesLocalesTable(priceCodesLocales);

                var sp = new StoredProcedure("Insert_PriceDetails");
                sp.SetConnectionKey("pricing_connection");
                sp.AddParameter("@groupId", groupId);
                sp.AddParameter("@code", code);
                sp.AddParameter("@price", price);
                sp.AddParameter("@currencyId", currencyId);
                sp.AddParameter("@priceCodesLocalesExist", priceCodesLocales == null ? 0 : 1);
                sp.AddDataTableParameter("@priceCodesLocales", priceCodesLocalesDt);
                sp.AddParameter("@updaterId", userId);

                var id = sp.ExecuteReturnValue<long>();

                return id;
            }
            catch (Exception ex)
            {
                log.Error($"Error while InsertPriceDetails , groupId: {groupId}, code: {code}, ex:{ex} ");
                return 0;
            }
        }
        public long InsertDiscountDetails(int groupId, string code, double price, double percentage, long currencyId,
                                  DateTime startDate, DateTime endDate, long userId, List<DiscountDTO> discounts,
                                  WhenAlgoType whenAlgoType, int whenAlgoTimes)
        {
            try
            {
                var sp = new StoredProcedure("Insert_DiscountDetails");
                sp.SetConnectionKey("pricing_connection");
                sp.AddParameter("@startDate", startDate);
                sp.AddParameter("@endDate", endDate);
                sp.AddParameter("@groupId", groupId);
                sp.AddParameter("@code", code);
                sp.AddParameter("@price", price);
                sp.AddParameter("@discountPercent", percentage);
                sp.AddParameter("@currencyId", currencyId);
                sp.AddParameter("@whenAlgoType", whenAlgoType);
                sp.AddParameter("@whenAlgoTimes", whenAlgoTimes);
                sp.AddParameter("@discountCodesLocalesExist", discounts == null ? 0 : 1);
                sp.AddDataTableParameter("@discountCodesLocales", SetDiscountCodesLocales(discounts));
                sp.AddParameter("@updaterId", userId);

                return sp.ExecuteReturnValue<long>();
            }
            catch (Exception ex)
            {
                log.Error($"Error while InsertDiscounteDetails , groupId: {groupId}, code: {code}, ex:{ex} ");
                return 0;
            }
        }

        private static DataTable SetDiscountCodesLocales(List<DiscountDTO> discounts)
        {
            DataTable ccTable = new DataTable("DiscountCodesLocalesValues");

            ccTable.Columns.Add("COUNTRY_CODE", typeof(string));
            ccTable.Columns.Add("PRICE", typeof(double));
            ccTable.Columns.Add("CURRENCY_CD", typeof(long));
            ccTable.Columns.Add("DISCOUNT_PERECENT", typeof(long));

            if (discounts != null)
            {
                DataRow dr = null;
                foreach (var discount in discounts)
                {
                    dr = ccTable.NewRow();
                    dr["COUNTRY_CODE"] = discount.CountryId;
                    dr["PRICE"] = discount.Price;
                    dr["CURRENCY_CD"] = discount.CurrencyId;
                    dr["DISCOUNT_PERECENT"] = discount.Percentage;
                    ccTable.Rows.Add(dr);
                }
            }
            return ccTable;
        }

        public bool DeletePriceDetails(int groupId, long id, long userId)
        {
            try
            {
                var sp = new StoredProcedure("Delete_PriceDetails");
                sp.SetConnectionKey("pricing_connection");

                sp.AddParameter("@id", id);
                sp.AddParameter("@groupId", groupId);
                sp.AddParameter("@updaterId", userId);
                var result = sp.ExecuteReturnValue<int>() > 0;

                return result;
            }
            catch (Exception ex)
            {
                log.Error($"Error while DeletePriceDetails, groupId: {groupId}, Id: {id}", ex);
                return false;
            }
        }

        private List<PriceDetailsDTO> BuildPriceCodesFromDataTable(DataTable priceCodes)
        {
            Dictionary<long, PriceDetailsDTO> priceDetailsMap = new Dictionary<long, PriceDetailsDTO>();

            if (priceCodes != null && priceCodes.Rows != null && priceCodes.Rows.Count > 0)
            {
                foreach (DataRow dr in priceCodes.Rows)
                {
                    long id = ODBCWrapper.Utils.GetLongSafeVal(dr, "id");

                    if (!priceDetailsMap.ContainsKey(id))
                    {
                        PriceDetailsDTO pd = new PriceDetailsDTO()
                        {
                            Id = id,
                            Name = ODBCWrapper.Utils.GetSafeStr(dr, "code"),
                            Prices = new List<PriceDTO>()
                        };
                        priceDetailsMap.Add(id, pd);
                    }
                    PriceDTO price = new PriceDTO()
                    {
                        CountryId = ODBCWrapper.Utils.GetIntSafeVal(dr, "country_id"),
                        Price = ODBCWrapper.Utils.GetDoubleSafeVal(dr, "price"),
                        Currency = new CurrencyDTO() { CurrencyId = ODBCWrapper.Utils.GetIntSafeVal(dr, "CURRENCY_CD") }
                    };

                    priceDetailsMap[id].Prices.Add(price);
                }
            }
            return priceDetailsMap != null ? priceDetailsMap.Values.ToList() : null;
        }

        private DataTable SetPriceCodesLocalesTable(List<PriceDTO> prices)
        {
            DataTable ccTable = new DataTable("PriceCodesLocalesValues");

            ccTable.Columns.Add("COUNTRY_CODE", typeof(string));
            ccTable.Columns.Add("PRICE", typeof(double));
            ccTable.Columns.Add("CURRENCY_CD", typeof(long));

            if (prices != null)
            {
                DataRow dr = null;
                foreach (var price in prices)
                {
                    dr = ccTable.NewRow();
                    dr["COUNTRY_CODE"] = price.CountryId;
                    dr["PRICE"] = price.Price;
                    dr["CURRENCY_CD"] = price.Currency.CurrencyId;
                    ccTable.Rows.Add(dr);
                }
            }
            return ccTable;
        }

        public List<UsageModuleDTO> GetPricePlansDTO(int groupId, List<long> pricePlanIds = null)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("GetPricePlans");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@groupId", groupId);
            sp.AddIDListParameter("@usageModulesIds", pricePlanIds, "ID");
            sp.AddParameter("@shouldGetAll", pricePlanIds == null || pricePlanIds.Count == 0 ? 1 : 0);
            DataTable usageModulesTable = sp.Execute();

            return BuildUsageModulesFromDataTable(usageModulesTable);
        }

        private List<UsageModuleDTO> BuildUsageModulesFromDataTable(DataTable usageModulesTable)
        {
            List<UsageModuleDTO> response = new List<UsageModuleDTO>();
            if (usageModulesTable != null && usageModulesTable.Rows != null && usageModulesTable.Rows.Count > 0)
            {
                foreach (DataRow row in usageModulesTable.Rows)
                {
                    response.Add(BuildUsageModuleFromDataRow(row));
                }
            }
            return response;
        }

        private UsageModuleDTO BuildUsageModuleFromDataRow(DataRow usageModuleRow)
        {
            if (usageModuleRow != null)
            {
                return new UsageModuleDTO()
                {
                    IsOfflinePlayBack = ODBCWrapper.Utils.GetIntSafeVal(usageModuleRow, "OFFLINE_PLAYBACK") == 1 ? true : false,
                    Waiver = ODBCWrapper.Utils.GetIntSafeVal(usageModuleRow, "WAIVER") == 1 ? true : false,
                    CouponId = ODBCWrapper.Utils.GetIntSafeVal(usageModuleRow, "coupon_id"),
                    ExtDiscountId = ODBCWrapper.Utils.GetIntSafeVal(usageModuleRow, "ext_discount_id"),
                    IsRenew = ODBCWrapper.Utils.GetIntSafeVal(usageModuleRow, "is_renew"),
                    MaxNumberOfViews = ODBCWrapper.Utils.GetIntSafeVal(usageModuleRow, "MAX_VIEWS_NUMBER"),
                    Id = ODBCWrapper.Utils.GetIntSafeVal(usageModuleRow, "ID"),
                    NumOfRecPeriods = ODBCWrapper.Utils.GetIntSafeVal(usageModuleRow, "num_of_rec_periods"),
                    WaiverPeriod = ODBCWrapper.Utils.GetIntSafeVal(usageModuleRow, "WAIVER_PERIOD"),
                    PricingId = ODBCWrapper.Utils.GetIntSafeVal(usageModuleRow, "pricing_id"),
                    VirtualName = ODBCWrapper.Utils.GetSafeStr(usageModuleRow, "NAME"),
                    TsMaxUsageModuleLifeCycle = ODBCWrapper.Utils.GetIntSafeVal(usageModuleRow, "FULL_LIFE_CYCLE_MIN"),
                    TsViewLifeCycle = ODBCWrapper.Utils.GetIntSafeVal(usageModuleRow, "VIEW_LIFE_CYCLE_MIN")
                };
            }
            return null;
        }

        public bool DeletePricePlan(int groupId, long id, long userId)
        {
            try
            {
                var sp = new StoredProcedure("Delete_PricePlanById");
                sp.SetConnectionKey("pricing_connection");

                sp.AddParameter("@id", id);
                sp.AddParameter("@groupId", groupId);
                sp.AddParameter("@updaterId", userId);
                var result = sp.ExecuteReturnValue<int>() > 0;

                return result;
            }
            catch (Exception ex)
            {
                log.Error($"Error while DeletePricePlan, groupId: {groupId}, Id: {id}", ex);
                return false;
            }
        }

        public bool DeleteDiscountDetails(int groupId, long id, long userId)
        {
            try
            {
                var sp = new StoredProcedure("Delete_DiscountDetails");
                sp.SetConnectionKey("pricing_connection");

                sp.AddParameter("@id", id);
                sp.AddParameter("@groupId", groupId);
                sp.AddParameter("@updaterId", userId);
                var result = sp.ExecuteReturnValue<int>() > 0;

                return result;
            }
            catch (Exception ex)
            {
                log.Error($"Error while Delete DiscountDetails, groupId: {groupId}, Id: {id}", ex);
                return false;
            }
        }
    }
}