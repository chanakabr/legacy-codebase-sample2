using ApiObjects;
using ApiObjects.AssetLifeCycleRules;
using ApiObjects.Base;
using ApiObjects.Pricing;
using ApiObjects.Pricing.Dto;
using CouchbaseManager;
using Newtonsoft.Json;
using ODBCWrapper;
using Phx.Lib.Log;
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
        long InsertDiscountDetails(int groupId, long userId, DiscountDetailsDTO discountDetailsDTO);
        bool IsDiscountCodeExists(int groupId, long id);

        long UpdateDiscountDetails(long id, int groupId, long userId, bool needToUpdateDiscountCodeLocals,
                                        bool needToUpdateDiscountCode, DiscountDetailsDTO discountDetailsDTO);
        DataTable GetGroupDiscounts(int groupId);
    }

    public interface IPriceDetailsRepository
    {
        bool DeletePriceDetails(int groupId, long id, long userId);
        long InsertPriceDetails(int groupId, PriceDetailsDTO priceDetails, long userId);
        List<PriceDetailsDTO> GetPriceDetails(int groupId);
        bool UpdatePriceDetails(int groupId, long id, bool updatePriceCode, PriceDetailsDTO priceDetails, bool updatePriceCodesLocales, long userId);
    }

    public interface IPricePlanRepository
    {
        bool DeletePricePlan(int groupId, long id, long userId);
        long InsertPricePlan(int groupId, PricePlan pricePlan, long userId);
        int UpdatePricePlan(int groupID, PricePlan pricePlan, long id, long userId);
        DataTable GetPricePlansDT(int groupId, List<long> pricePlanIds = null);

    }
    public interface IModuleManagerRepository
    {
        int InsertUsageModule(long userId, int groupID, UsageModuleDTO usageModuleDTO);
        bool DeletePricePlan(int groupId, long id, long userId);
        bool IsUsageModuleExistsById(int groupId, long id);
        List<UsageModuleDTO> GetUsageModule(int groupId, List<long> usageModuleIds = null);
        int UpdateUsageModule(int groupId, long userId, long id, UsageModuleDTO usageModuleDTO);
    }

    public interface IPreviewModuleRepository
    {
        List<PreviewModuleDTO> Get_PreviewModules(int nGroupID);
        long InsertPreviewModule(int groupID, PreviewModuleDTO previewModuleDTO, long userId);
        bool DeletePreviewModule(int groupId, long id, long userId);
        bool IsPreviewModuleExsitsd(int groupId, long id);
        long UpdatePreviewModule(long id, PreviewModuleDTO previewModuleDTO, long userId);
    }

    public interface ICollectionRepository
    {
        bool IsCollectionExists(int groupId, long id);
        int DeleteCollection(int groupId, long id, long userId);
        long GetCollectionByExternalId(int groupId, string externalId);
        long AddCollection(int groupId, long value, CollectionInternal collectionToInsert);
        bool UpdateCollection(int groupId, long value, CollectionInternal collectionToUpdate, NullableObj<DateTime?> nullableStartDate, NullableObj<DateTime?> nullableEndDate, long? virtualAssetId);
        List<long> GetCollectionsByChannelId(int groupId, long channelId);
        void DeleteCollectionsChannelsByChannel(int groupId, long userId, long channelId);
        bool UpdateCollectionVirtualAssetId(int groupId, long id, long assetId, long value);
    }
    public interface IPricingPartnerRepository
    {
        bool SetupPartnerInDb(long partnerId, long updaterId);
        bool DeletePartnerBasicDataDb(long partnerId, long updaterId);
    }

    public interface ISubscriptionManagerRepository
    {
        Dictionary<string, string> Get_SubscriptionsFromProductCodes(List<string> productCodes, int groupId);
        int DeleteSubscription(int groupID, long id, long updaterId);
        bool IsSubscriptionExists(int groupId, long id);
        int AddSubscription(int groupId, long updaterId, SubscriptionInternal subscription, long? basePricePlanId, long? basePriceCodeId, bool isRecurring, long? extDiscountId);
        long GetSubscriptionByExternalId(int groupId, string externalId);
        bool UpdateSubscription(int groupId, long updaterId, SubscriptionInternal subscription, long? basePricePlanId, long? basePriceCodeId, bool? isRecurring,
            NullableObj<DateTime?> startDate, NullableObj<DateTime?> endDate, long? extDiscountId);

        bool UpdateSubscriptionVirtualAssetId(int groupId, long id, long? virtualAssetId, long userId);
        List<int> GetSubscriptionsByChannelId(int groupId, int channelId);
        void DeleteSubscriptionsChannelsByChannel(int groupId, int channelId);
    }

    public interface IPpvManagerRepository
    {
        List<PpvDTO> Get_AllPpvModuleData(int groupID);
        int InsertPPV(int groupID, long updaterId, PpvDTO ppv);
        bool DeletePPV(int groupID, long userId, long id);
        int UpdatePPV(int groupID, long updaterId, int id, PpvDTO ppv);

        int UpdatePPVFileTypes(int groupID, int id, List<int> fileTypesIds);
        int UpdatePPVDescriptions(int groupID, long updaterId, int id, LanguageContainer[] descriptions);
        bool UpdatePpvVirtualAssetId(int groupId, long id, long? virtualAssetId, long userId);
    }

    public class PricingDAL : ICampaignRepository, IPriceDetailsRepository, IPricePlanRepository, IModuleManagerRepository, IDiscountDetailsRepository, IPreviewModuleRepository,
        ICollectionRepository, IPricingPartnerRepository, ISubscriptionManagerRepository, IPpvManagerRepository
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private static readonly Lazy<PricingDAL> lazy = new Lazy<PricingDAL>(() => new PricingDAL(), LazyThreadSafetyMode.PublicationOnly);

        public static PricingDAL Instance { get { return lazy.Value; } }

        public PricingDAL()
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
        public List<PreviewModuleDTO> Get_PreviewModules(int nGroupID)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_PreviewModulesByGroupID");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@GroupID", nGroupID);
            sp.AddParameter("@IsActive", 1);
            sp.AddParameter("@Status", 1);

            return BuildPreviewModulesFromDataTable(sp.Execute());
        }

        private List<PreviewModuleDTO> BuildPreviewModulesFromDataTable(DataTable previewModules)
        {
            List<PreviewModuleDTO> previewModulesList = new List<PreviewModuleDTO>();
            if (previewModules != null && previewModules.Rows != null && previewModules.Rows.Count > 0)
            {
                foreach (DataRow dr in previewModules.Rows)
                {
                    previewModulesList.Add(new PreviewModuleDTO(Utils.GetSafeStr(dr, "name"),
                                                                Utils.GetIntSafeVal(dr, "FULL_LIFE_CYCLE_ID"),
                                                                Utils.GetIntSafeVal(dr, "NON_RENEWING_PERIOD_ID"),
                                                                Utils.GetLongSafeVal(dr, "id")));
                }
            }
            return previewModulesList;
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

        public static bool GetGroupHasSubWithAds(int groupId)
        {
            try
            {
                var sp = new StoredProcedure("Group_Has_Ads");
                sp.SetConnectionKey("pricing_connection");

                sp.AddParameter("@GroupID", groupId);
                var result = sp.ExecuteReturnValue<int>() > 0;

                return result;
            }
            catch (Exception ex)
            {
                log.Error($"Error while Checking if group contain subscriptions with ads, groupId: {groupId}", ex);
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

        public bool SetupPartnerInDb(long partnerId, long updaterId)
        {
            var sp = new StoredProcedure("Create_GroupBasicData");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@groupId", partnerId);
            sp.AddParameter("@updaterId", updaterId);

            return sp.ExecuteReturnValue<int>() > 0;
        }

        public bool DeletePartnerBasicDataDb(long partnerId, long updaterId)
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

        public List<PpvDTO> Get_PPVModuleData(int nGroupID, int? nPPVModuleID)
        {
            var spPPVModuleData = new StoredProcedure("Get_PPV_ModuleData");
            spPPVModuleData.SetConnectionKey("pricing_connection");
            spPPVModuleData.AddParameter("@GroupID", nGroupID);
            spPPVModuleData.AddNullableParameter<int?>("@PPVModuleID", nPPVModuleID);

            DataSet ds = spPPVModuleData.ExecuteDataSet();
            if (ds != null)
                return BuildPPVDTOFromDataTable(ds.Tables[0]);

            return null;
        }

        public List<PpvDTO> Get_AllPpvModuleData(int groupID)
        {
            StoredProcedure spPPVModuleData = new ODBCWrapper.StoredProcedure("Get_All_PPV_ModuleData");
            spPPVModuleData.SetConnectionKey("pricing_connection");
            spPPVModuleData.AddParameter("@GroupID", groupID);
            DataSet ds = spPPVModuleData.ExecuteDataSet();

            if (ds != null)
                return BuildPPVDTOFromDataTable(ds.Tables[0]);

            return null;
        }
        private List<PpvDTO> BuildPPVDTOFromDataTable(DataTable ppvsTable)
        {
            List<PpvDTO> response = new List<PpvDTO>();
            if (ppvsTable != null && ppvsTable.Rows != null && ppvsTable.Rows.Count > 0)
            {
                foreach (DataRow row in ppvsTable.Rows)
                {
                    response.Add(BuildPPVDTOFromDataRow(row));
                }
            }
            return response;
        }
        private PpvDTO BuildPPVDTOFromDataRow(DataRow ppvModule)
        {
            if (ppvModule != null)
            {
                PpvDTO ppv = new PpvDTO()
                {
                    Id =  Utils.GetIntSafeVal(ppvModule["ID"]),
                    PriceCode = Utils.GetIntSafeVal(ppvModule["PRICE_CODE"]),
                    UsageModuleCode = Utils.GetIntSafeVal(ppvModule["USAGE_MODULE_CODE"]),
                    DiscountCode = Utils.GetIntSafeVal(ppvModule["DISCOUNT_MODULE_CODE"]),
                    CouponsGroupCode = Utils.GetIntSafeVal(ppvModule["COUPON_GROUP_CODE"]),
                    Name = Utils.GetSafeStr(ppvModule["NAME"]),
                    SubscriptionOnly = Convert.ToBoolean(Utils.GetIntSafeVal(ppvModule["SUBSCRIPTION_ONLY"])),
                    FirstDeviceLimitation = Convert.ToBoolean(Utils.GetIntSafeVal(ppvModule["FIRSTDEVICELIMITATION"])),
                    ProductCode = Utils.GetSafeStr(ppvModule["Product_Code"]),
                    IsActive = Convert.ToBoolean(Utils.GetIntSafeVal(ppvModule["IS_ACTIVE"])),
                    AdsParam = Utils.GetSafeStr(ppvModule["ADS_PARAM"]),
                    CreateDate = Utils.GetDateSafeVal((ppvModule["CREATE_DATE"])),
                    UpdateDate = Utils.GetDateSafeVal((ppvModule["UPDATE_DATE"])),
                };
                
                ppv.VirtualAssetId = Utils.GetNullableLong(ppvModule, "VIRTUAL_ASSET_ID");
                ppv.AssetUserRuleId = Utils.GetNullableLong(ppvModule, "ASSET_USER_RULE_ID");
               
                int adsPolicyInt = Utils.GetIntSafeVal(ppvModule["ADS_POLICY"]);
                if (adsPolicyInt > 0)
                {
                    ppv.AdsPolicy = (AdsPolicy)adsPolicyInt;
                }

                return ppv;
            }
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

        public long AddCollection(int groupId, long updaterId, CollectionInternal collection)
        {
            DataTable couponGroups = null;
            if (collection.CouponGroups?.Count > 0)
            {
                couponGroups = SetCouponGroupsTable(collection.CouponGroups);
            }

            var externalProductCodes = SetExternalProductCodes(collection.ExternalProductCodes);

            var sp = new StoredProcedure("Add_Collection");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@groupId", groupId);
            sp.AddParameter("@needToUpdateChannels", collection.ChannelsIds?.Count > 0 ? 1 : 0);
            sp.AddIDListParameter<long>("@channels", collection.ChannelsIds, "Id");
            sp.AddParameter("@needToUpdateCouponGroups", couponGroups == null ? 0 : 1);
            sp.AddDataTableParameter("@couponGroups", couponGroups);
            sp.AddParameter("@needToUpdateDescriptions", collection.Descriptions == null ? 0 : 1);
            sp.AddKeyValueListParameter<string, string>("@descriptions", collection.Descriptions?.Select(t => new KeyValuePair<string, string>(t.m_sLanguageCode3, t.m_sValue)).ToList(), "key", "value");
            sp.AddParameter("@endDate", collection.EndDate);
            sp.AddParameter("@externalId", collection.ExternalId);
            sp.AddParameter("@needToUpdateProductCodes", externalProductCodes == null ? 0 : 1);
            sp.AddKeyValueListParameter<int, string>("@productCodes", externalProductCodes, "key", "value");
            sp.AddParameter("@discountModuleId", collection.DiscountModuleId);
            sp.AddParameter("@usageModuleId", collection.UsageModuleId);
            sp.AddParameter("@isActive", collection.IsActive.HasValue ? collection.IsActive.Value : false);
            sp.AddParameter("@name", collection.Names[0].m_sValue);
            sp.AddKeyValueListParameter<string, string>("@names", collection.Names.Select(t => new KeyValuePair<string, string>(t.m_sLanguageCode3, t.m_sValue)).ToList(), "key", "value");
            sp.AddParameter("@priceDetailsId", collection.PriceDetailsId);
            sp.AddParameter("@startDate", collection.StartDate);
            sp.AddParameter("@updaterId", updaterId);
            sp.AddParameter("@fileTypesIds_json", JsonConvert.SerializeObject(collection.FileTypesIds));
            if (collection.AssetUserRuleId.HasValue && collection.AssetUserRuleId > 0)
            {
                sp.AddParameter("@assetUserRuleId", collection.AssetUserRuleId);
            }

            return sp.ExecuteReturnValue<long>();
        }

        public bool IsCollectionExists(int groupId, long id)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Is_CollectionExists");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@groupId", groupId);
            sp.AddParameter("@CollectionId", id);
            return sp.ExecuteReturnValue<int>() > 0;
        }

        public int DeleteCollection(int groupId, long id, long userId)
        {
            var sp = new StoredProcedure("Delete_CollectionById");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@id", id);
            sp.AddParameter("@groupId", groupId);
            sp.AddParameter("@updaterId", userId);
            return sp.ExecuteReturnValue<int>();
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

        public Dictionary<string, string> Get_SubscriptionsFromProductCodes(List<string> productCodes, int groupID)
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

        public int InsertUsageModule(long userId, int groupID, UsageModuleDTO usageModuleDTO)
        {
            try
            {
                StoredProcedure sp = new StoredProcedure("InsertUsageModule");
                sp.SetConnectionKey("pricing_connection");
                sp.AddParameter("@GroupID", groupID);
                sp.AddParameter("@UserId", userId);
                sp.AddParameter("@Name", usageModuleDTO.VirtualName);
                sp.AddParameter("@MaxViews", usageModuleDTO.MaxNumberOfViews);
                sp.AddParameter("@WaiverPeriod", usageModuleDTO.WaiverPeriod);
                sp.AddParameter("@IsWaiverEnabled", usageModuleDTO.Waiver);
                sp.AddParameter("@IsOfflinePlayback", usageModuleDTO.IsOfflinePlayBack);
                sp.AddParameter("@FullLifeCycleID", usageModuleDTO.TsMaxUsageModuleLifeCycle);
                sp.AddParameter("@ViewLifeCycleID", usageModuleDTO.TsViewLifeCycle);
                sp.AddParameter("@Date", DateTime.UtcNow);

                return sp.ExecuteReturnValue<int>();
            }
            catch (Exception ex)
            {
                HandleException(string.Empty, ex);
            }
            return 0;
        }
        
        public int UpdateUsageModule(int groupId, long userId, long id, UsageModuleDTO usageModuleDTO)
        {
            StoredProcedure sp = new StoredProcedure("Update_UsageModule");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@GroupID", groupId);
            sp.AddParameter("@UpdaterID", userId);
            sp.AddParameter("@Name", usageModuleDTO.VirtualName);
            sp.AddParameter("@MaxViews", usageModuleDTO.MaxNumberOfViews);
            sp.AddParameter("@WaiverPeriod", usageModuleDTO.WaiverPeriod);
            sp.AddParameter("@IsWaiverEnabled", usageModuleDTO.Waiver);
            sp.AddParameter("@IsOfflinePlayback", usageModuleDTO.IsOfflinePlayBack);
            sp.AddParameter("@FullLifeCycleID", usageModuleDTO.TsMaxUsageModuleLifeCycle);
            sp.AddParameter("@ViewLifeCycleID", usageModuleDTO.TsViewLifeCycle);
            sp.AddParameter("@ID", id);
            return sp.ExecuteReturnValue<int>();
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

        public long InsertPreviewModule(int groupID, PreviewModuleDTO previewModuleDTO, long userId)
        {
            try
            {
                StoredProcedure sp = new StoredProcedure("Insert_PreviewModule");
                sp.SetConnectionKey("pricing_connection");
                sp.AddParameter("@GroupID", groupID);
                sp.AddParameter("@Name", previewModuleDTO.Name);
                sp.AddParameter("@FullLifeCycle", previewModuleDTO.FullLifeCycle);
                sp.AddParameter("@NonRenewPeriod", previewModuleDTO.NonRenewPeriod);
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


        public int UpdatePricePlan(int groupID, PricePlan pricePlan, long id, long userId)
        {
            StoredProcedure sp = new StoredProcedure("Update_PricePlanById");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@GroupID", groupID);
            sp.AddParameter("@UpdaterID", userId);
            sp.AddParameter("@Name", pricePlan.Name);
            sp.AddParameter("@ID", id);
            sp.AddParameter("@MaxViews", pricePlan.MaxViewsNumber);
            sp.AddParameter("@RecurringPeriods", pricePlan.RenewalsNumber);
            sp.AddParameter("@PriceCodeID", pricePlan.PriceDetailsId);
            sp.AddParameter("@FullLifeCycleID", pricePlan.FullLifeCycle);
            sp.AddParameter("@ViewLifeCycleID", pricePlan.ViewLifeCycle);
            sp.AddParameter("@DiscountID", pricePlan.DiscountId);
            sp.AddParameter("@IsRenewable", pricePlan.IsRenewable);
            sp.AddParameter("@Date", DateTime.UtcNow);
            sp.AddParameter("@IsOfflinePlayBack", pricePlan.IsOfflinePlayBack);
            sp.AddParameter("@Waiver", pricePlan.IsWaiverEnabled);
            sp.AddParameter("@WaiverPeriod", pricePlan.WaiverPeriod);

            return sp.ExecuteReturnValue<int>();
        }

        public long InsertPricePlan(int groupId, PricePlan pricePlan, long userId)
        {
            StoredProcedure sp = new StoredProcedure("Insert_PricePlan");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@GroupID", groupId);
            sp.AddParameter("@Name", pricePlan.Name);
            sp.AddParameter("@UserId", userId);
            sp.AddParameter("@IsActive", true);
            sp.AddParameter("@MaxViews", pricePlan.MaxViewsNumber.HasValue ? pricePlan.MaxViewsNumber.Value : 0);
            sp.AddParameter("@IsRenewable", pricePlan.IsRenewable);
            sp.AddParameter("@RecurringPeriods", pricePlan.RenewalsNumber);
            sp.AddParameter("@PricCodeID", pricePlan.PriceDetailsId);
            sp.AddParameter("@FullLifeCycleID", pricePlan.FullLifeCycle);
            sp.AddParameter("@ViewLifeCycleID", pricePlan.ViewLifeCycle);
            sp.AddParameter("@DiscountID", pricePlan.DiscountId);
            sp.AddParameter("@Date", DateTime.UtcNow);
            sp.AddParameter("@IsOfflinePlayBack", pricePlan.IsOfflinePlayBack.HasValue ? pricePlan.IsOfflinePlayBack : false);
            sp.AddParameter("@Waiver", pricePlan.IsWaiverEnabled.HasValue ? pricePlan.IsWaiverEnabled : false);
            sp.AddParameter("@WaiverPeriod", pricePlan.WaiverPeriod.HasValue ? pricePlan.WaiverPeriod : 0);

            return sp.ExecuteReturnValue<long>();
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
        
        public bool UpdatePpvVirtualAssetId(int groupId, long id, long? virtualAssetId, long userId)
        {
            var sp = new StoredProcedure("UpdatePpvVirtualAssetId");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@groupId", groupId);
            sp.AddParameter("@id", id);
            sp.AddParameter("@virtualAssetId", virtualAssetId);
            sp.AddParameter("@updateDate", DateTime.UtcNow);
            sp.AddParameter("@userId", userId);

            return sp.ExecuteReturnValue<int>() > 0;
        }
        
        public int InsertPPV(int groupID, long updaterId, PpvDTO ppv)
        {
            StoredProcedure sp = new StoredProcedure("Insert_PPVModule");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@GroupID", groupID);
            sp.AddParameter("@Name", ppv.Name);
            sp.AddParameter("@priceCode", ppv.PriceCode);
            sp.AddParameter("@usageModule", ppv.UsageModuleCode);
            sp.AddParameter("@discount", ppv.DiscountCode);
            sp.AddParameter("@UpdaterId", updaterId);
            sp.AddParameter("@couponGroup", ppv.CouponsGroupCode);
            sp.AddParameter("@subscriptionOnly", ppv.SubscriptionOnly);
            sp.AddParameter("@firstDeviceLimitation", ppv.FirstDeviceLimitation);
            sp.AddParameter("@productCode", ppv.ProductCode);
            sp.AddIDListParameter<int>("@FileTypes", ppv.FileTypesIds, "Id");
            sp.AddParameter("@Date", ppv.CreateDate);
            if (ppv.AdsPolicy.HasValue)
            {
                sp.AddParameter("@AdsPolicy", (int)ppv.AdsPolicy);
            }
            
            if (ppv.AssetUserRuleId > 0)
            {
                sp.AddParameter("@assetUserRuleId", ppv.AssetUserRuleId.Value);
            }

            if (ppv.Descriptions != null)
            {
                sp.AddKeyValueListParameter<string, string>("@Description", ppv.Descriptions.Select(d => new KeyValuePair<string, string>(d.m_sLanguageCode3, d.m_sValue)).ToList(), "key", "value");
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
            sp.AddParameter("@IsAct1ive", ppv.IsActive);
            sp.AddIDListParameter<long>("@FileTypes", fileTypes, "Id");
            sp.AddParameter("@Date", DateTime.UtcNow);
            if (ppv.Descriptions != null)
            {
                sp.AddKeyValueListParameter<string, string>("@Description", ppv.Descriptions.Select(d => new KeyValuePair<string, string>(d.key, d.value)).ToList(), "key", "value");
            }
            return sp.ExecuteReturnValue<int>();
        }
        
        public int UpdatePPV(int groupID, long updaterId, int id, PpvDTO ppv)
        {
            StoredProcedure sp = new StoredProcedure("Update_PPVModuleById");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@GroupID", groupID);
            sp.AddParameter("@Name", ppv.Name);
            sp.AddParameter("@ID", id);
            sp.AddParameter("@UpdaterId", updaterId);
            sp.AddParameter("@priceCode", ppv.PriceCode);
            sp.AddParameter("@usageModule", ppv.UsageModuleCode);
            sp.AddParameter("@discount", ppv.DiscountCode);
            sp.AddParameter("@couponGroup", ppv.CouponsGroupCode);
            sp.AddParameter("@subscriptionOnly", ppv.SubscriptionOnly);
            sp.AddParameter("@firstDeviceLimitation", ppv.FirstDeviceLimitation);
            sp.AddParameter("@productCode", ppv.ProductCode);
            sp.AddParameter("@IsActive", ppv.IsActive);
            sp.AddParameter("@Date", DateTime.UtcNow);
            if (ppv.AdsPolicy.HasValue)
            {
                sp.AddParameter("@AdsPolicy", (int)ppv.AdsPolicy);
            }

            if (ppv.VirtualAssetId.HasValue)
            {
                sp.AddParameter("@virtualAssetId", ppv.VirtualAssetId.Value);
            }
            return sp.ExecuteReturnValue<int>();
        }
        
        public int UpdatePPVFileTypes(int groupID, int id, List<int> fileTypesIds)
        {
            StoredProcedure sp = new StoredProcedure("UpdatePPVFileTypes");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@GroupID", groupID);
            sp.AddParameter("@PPVID", id);
            sp.AddIDListParameter<int>("@FileTypes", fileTypesIds, "Id");
            sp.AddParameter("@Date", DateTime.UtcNow);
            return sp.ExecuteReturnValue<int>();
        }
        
        public int UpdatePPVDescriptions(int groupID, long updaterId, int id, LanguageContainer[] descriptions)
        {
            StoredProcedure sp = new StoredProcedure("UpdatePPVDescriptions");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@GroupID", groupID);
            sp.AddParameter("@PPVID", id);
            sp.AddParameter("@UpdaterId", updaterId);
            sp.AddKeyValueListParameter("@Description", descriptions.Select(d => new KeyValuePair<string, string>(d.m_sLanguageCode3, d.m_sValue)).ToList(), "key", "value");
            sp.AddParameter("@Date", DateTime.UtcNow);
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

        public bool DeletePPV(int groupID, long userId, long id)
        {

            StoredProcedure sp = new StoredProcedure("Delete_PPVById");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@GroupID", groupID);
            sp.AddParameter("@UpdaterId", userId);
            sp.AddParameter("@Date", DateTime.UtcNow);
            sp.AddParameter("@id", id);
            return sp.ExecuteReturnValue<int>() > 0;
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

        public DataTable GetPricePlansDT(int groupId, List<long> pricePlanIds = null)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("GetPricePlans");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@groupId", groupId);
            sp.AddIDListParameter("@usageModulesIds", pricePlanIds, "ID");
            sp.AddParameter("@shouldGetAll", pricePlanIds == null || pricePlanIds.Count == 0 ? 1 : 0);
            return sp.Execute();
        }

        public List<PriceDetailsDTO> GetPriceDetails(int groupId)
        {
            List<PriceDetailsDTO> priceDetailsDTOList = null;

            var parameters = new Dictionary<string, object>() { { "@groupId", groupId } };
            var ds = UtilsDal.ExecuteDataSet("GetGroupPriceCodes", parameters, "pricing_connection");
            if (ds?.Tables.Count > 0)
            {
                priceDetailsDTOList = BuildPriceDetailsDTOList(ds.Tables[0]);
            }

            return priceDetailsDTOList;
        }
        public bool IsPreviewModuleExsitsd(int groupId, long id)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Is_PreviewModuleExsits");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@groupId", groupId);
            sp.AddParameter("@id", id);
            return sp.ExecuteReturnValue<long>() > 0;
        }

        public long UpdatePreviewModule(long id, PreviewModuleDTO previewModuleDTO, long userId)
        {
            StoredProcedure sp = new StoredProcedure("Update_PreviewModule");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@ID", id);
            sp.AddParameter("@Name", previewModuleDTO.Name);
            sp.AddParameter("@FullLifeCycle", previewModuleDTO.FullLifeCycle);
            sp.AddParameter("@NonRenewPeriod", previewModuleDTO.NonRenewPeriod);
            sp.AddParameter("@UpdaterId", userId);

            return sp.ExecuteReturnValue<int>();
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

        public DataTable GetGroupDiscounts(int groupId)
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

        public static Dictionary<long, bool> GetAllCollectionIds(int groupId)
        {
            Dictionary<long, bool> res = null;

            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_AllCollectionsIds");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@groupId", groupId);
            DataTable dt = sp.Execute();

            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
            {
                res = new Dictionary<long, bool>();
                foreach (DataRow dr in dt.Rows)
                {
                    res.Add(Utils.GetLongSafeVal(dr, "ID"), Utils.GetIntSafeVal(dr, "IS_ACTIVE") == 0 ? false : true);
                }
            }

            return res;
        }

        public static List<CollectionItemDTO> GetGroupCollectionsItems(int groupId)
        {
            List<CollectionItemDTO> res = null;

            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_AllCollectionsIds");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@groupId", groupId);
            DataTable dt = sp.Execute();

            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
            {
                res = new List<CollectionItemDTO>();
                CollectionItemDTO collectionItemDTO;
                foreach (DataRow dr in dt.Rows)
                {
                    collectionItemDTO = new CollectionItemDTO()
                    {
                        Id = Utils.GetLongSafeVal(dr, "ID"),
                        IsActive = Utils.GetIntSafeVal(dr, "IS_ACTIVE") == 0 ? false : true,
                        AssetUserRuleId = Utils.GetNullableLong(dr, "ASSET_USER_RULE_ID")
                    };

                    res.Add(collectionItemDTO);
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
            campaign.UpdaterId = contextData.UserId ?? 999;

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

        public long InsertPriceDetails(int groupId, PriceDetailsDTO priceDetails, long userId)
        {
            DataTable priceCodesLocalesDt = ConvertToDataTable(priceDetails.Prices);
            var sp = new StoredProcedure("Insert_PriceDetails");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@groupId", groupId);
            sp.AddParameter("@code", priceDetails.Name);
            sp.AddParameter("@priceCodesLocalesExist", priceDetails.Prices == null || priceDetails.Prices.Count == 0 ? 0 : 1);
            sp.AddDataTableParameter("@priceCodesLocales", priceCodesLocalesDt);
            sp.AddParameter("@updaterId", userId);

            var id = sp.ExecuteReturnValue<long>();
            return id;
        }

        public long InsertDiscountDetails(int groupId, long userId, DiscountDetailsDTO discountDetailsDTO)
        {
            var sp = new StoredProcedure("Add_DiscountDetails");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@startDate", discountDetailsDTO.StartDate);
            sp.AddParameter("@endDate", discountDetailsDTO.EndDate);
            sp.AddParameter("@groupId", groupId);
            sp.AddParameter("@code", discountDetailsDTO.Name);
            sp.AddParameter("@whenAlgoType", discountDetailsDTO.WhenAlgoType);
            sp.AddParameter("@whenAlgoTimes", discountDetailsDTO.WhenAlgoTimes);
            sp.AddParameter("@discountCodesLocalesExist", discountDetailsDTO.Discounts == null ? 0 : 1);
            sp.AddDataTableParameter("@discountCodesLocales", SetDiscountCodesLocales(discountDetailsDTO.Discounts));
            sp.AddParameter("@updaterId", userId);

            return sp.ExecuteReturnValue<long>();
        }

        public long UpdateDiscountDetails(long id, int groupId, long userId, bool needToUpdateDiscountCodeLocals, bool needToUpdateDiscountCode, DiscountDetailsDTO discountDetailsDTO)
        {
            var sp = new StoredProcedure("Modify_DiscountDetails");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@startDate", discountDetailsDTO.StartDate);
            sp.AddParameter("@endDate", discountDetailsDTO.EndDate);
            sp.AddParameter("@id", id);
            sp.AddParameter("@groupId", groupId);
            sp.AddParameter("@code", discountDetailsDTO.Name);
            sp.AddParameter("@price", needToUpdateDiscountCodeLocals ? "0" : null);
            sp.AddParameter("@discountPercent", needToUpdateDiscountCodeLocals ? "0" : null);
            sp.AddParameter("@currencyId", needToUpdateDiscountCodeLocals ? "0" : null);
            sp.AddParameter("@whenAlgoType", discountDetailsDTO.WhenAlgoType);
            sp.AddParameter("@whenAlgoTimes", discountDetailsDTO.WhenAlgoTimes);
            sp.AddParameter("@NeedToUpdateDiscountCode", needToUpdateDiscountCode ? 1 : 0);
            sp.AddDataTableParameter("@discountCodesLocales", SetDiscountCodesLocales(discountDetailsDTO.Discounts));
            sp.AddParameter("@updaterId", userId);

            return sp.ExecuteReturnValue<long>();
        }

        private static DataTable SetDiscountCodesLocales(List<DiscountDTO> discounts)
        {
            DataTable ccTable = new DataTable("DiscountCodesCurrencyValues");

            ccTable.Columns.Add("COUNTRY_CODE", typeof(string));
            ccTable.Columns.Add("PRICE", typeof(double));
            ccTable.Columns.Add("CURRENCY_CD", typeof(long));
            ccTable.Columns.Add("DISCOUNT_PERECENT", typeof(double));

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

        private List<PriceDetailsDTO> BuildPriceDetailsDTOList(DataTable dt)
        {
            Dictionary<long, PriceDetailsDTO> priceDetailsMap = new Dictionary<long, PriceDetailsDTO>();

            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    long id = Utils.GetLongSafeVal(dr, "id");

                    if (!priceDetailsMap.ContainsKey(id))
                    {
                        PriceDetailsDTO pd = new PriceDetailsDTO()
                        {
                            Id = id,
                            Name = Utils.GetSafeStr(dr, "code"),
                            Prices = new List<PriceCodeLocaleDTO>()
                        };
                        priceDetailsMap.Add(id, pd);
                    }
                    var price = new PriceCodeLocaleDTO()
                    {
                        CountryId = Utils.GetIntSafeVal(dr, "country_id"),
                        Price = Utils.GetDoubleSafeVal(dr, "price"),
                        CurrencyId = Utils.GetIntSafeVal(dr, "CURRENCY_CD")
                    };

                    priceDetailsMap[id].Prices.Add(price);
                }
            }

            return priceDetailsMap.Values.ToList();
        }

        private DataTable ConvertToDataTable(List<PriceCodeLocaleDTO> priceCodeLocales)
        {
            DataTable ccTable = new DataTable("PriceCodesLocalesValues");
            ccTable.Columns.Add("COUNTRY_CODE", typeof(string));
            ccTable.Columns.Add("PRICE", typeof(double));
            ccTable.Columns.Add("CURRENCY_CD", typeof(long));

            if (priceCodeLocales != null)
            {
                foreach (var price in priceCodeLocales)
                {
                    var dr = ccTable.NewRow();
                    dr["COUNTRY_CODE"] = price.CountryCode;
                    dr["PRICE"] = price.Price;
                    dr["CURRENCY_CD"] = price.CurrencyId;
                    ccTable.Rows.Add(dr);
                }
            }
            return ccTable;
        }

        public List<UsageModuleDTO> GetUsageModule(int groupId, List<long> usageModuleIds = null)
        {
            StoredProcedure sp = new StoredProcedure("GetPricePlans");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@groupId", groupId);
            sp.AddIDListParameter("@usageModulesIds", usageModuleIds, "ID");
            sp.AddParameter("@shouldGetAll", usageModuleIds == null || usageModuleIds.Count == 0 ? 1 : 0);
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
                    IsOfflinePlayBack = Utils.GetIntSafeVal(usageModuleRow, "OFFLINE_PLAYBACK") == 1 ? true : false,
                    Waiver = Utils.GetIntSafeVal(usageModuleRow, "WAIVER") == 1 ? true : false,
                    MaxNumberOfViews = Utils.GetIntSafeVal(usageModuleRow, "MAX_VIEWS_NUMBER"),
                    Id = Utils.GetIntSafeVal(usageModuleRow, "ID"),
                    WaiverPeriod = Utils.GetIntSafeVal(usageModuleRow, "WAIVER_PERIOD"),
                    VirtualName = Utils.GetSafeStr(usageModuleRow, "NAME"),
                    TsMaxUsageModuleLifeCycle = Utils.GetIntSafeVal(usageModuleRow, "FULL_LIFE_CYCLE_MIN"),
                    TsViewLifeCycle = Utils.GetIntSafeVal(usageModuleRow, "VIEW_LIFE_CYCLE_MIN"),
                    Type = Utils.GetIntSafeVal(usageModuleRow, "type")
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

        public bool IsSubscriptionExists(int groupId, long id)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Is_SubscriptionExist");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@groupId", groupId);
            sp.AddParameter("@id", id);
            return sp.ExecuteReturnValue<int>() > 0;
        }

        public int AddSubscription(int groupId, long updaterId, SubscriptionInternal subscription, long? basePricePlanId, long? basePriceCodeId, bool isRecurring, long? extDiscountId)
        {
            DataTable couponGroups = null;
            DataTable premiumServices = null;

            if (subscription.CouponGroups?.Count > 0)
            {
                couponGroups = SetCouponGroupsTable(subscription.CouponGroups);
            }

            if (subscription.PremiumServices != null && subscription.PremiumServices.Length > 0)
            {
                premiumServices = SetPremiumServices(subscription.PremiumServices);
            }

            var externalProductCodes = SetExternalProductCodes(subscription.ExternalProductCodes);

            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Insert_Subscription");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@groupId", groupId);
            sp.AddParameter("@name", subscription.Names[0].m_sValue);
            sp.AddKeyValueListParameter<string, string>("@names", subscription.Names.Select(t => new KeyValuePair<string, string>(t.m_sLanguageCode3, t.m_sValue)).ToList(), "key", "value");
            sp.AddParameter("@startDate", subscription.StartDate);
            sp.AddParameter("@endDate", subscription.EndDate);
            sp.AddParameter("@needToUpdateChannels", subscription.ChannelsIds?.Count > 0 ? 1 : 0);
            sp.AddIDListParameter<long>("@channels", subscription.ChannelsIds, "Id");
            sp.AddParameter("@adsParams", subscription.AdsParams);
            sp.AddParameter("@adsPolicy", subscription.AdsPolicy.HasValue ? (int)subscription.AdsPolicy.Value : (int?)null);
            sp.AddParameter("@needToUpdateCouponGroups", couponGroups == null ? 0 : 1);
            sp.AddDataTableParameter("@couponGroups", couponGroups);
            sp.AddParameter("@type", (int)subscription.DependencyType);
            sp.AddParameter("@needToUpdateDescriptions", subscription.Descriptions == null ? 0 : 1);
            sp.AddKeyValueListParameter<string, string>("@descriptions", subscription.Descriptions != null ? subscription.Descriptions.Select(t => new KeyValuePair<string, string>(t.m_sLanguageCode3, t.m_sValue)).ToList() : null, "key", "value");
            sp.AddParameter("@externalId", subscription.ExternalId);
            sp.AddParameter("@needToUpdateProductCodes", externalProductCodes == null ? 0 : 1);
            sp.AddKeyValueListParameter<int, string>("@productCodes", externalProductCodes, "key", "value");
            sp.AddParameter("@needToUpdateFileTypes", subscription.FileTypesIds?.Count > 0 ? 1 : 0);
            sp.AddIDListParameter<long>("@fileTypesIds", subscription.FileTypesIds, "Id");
            sp.AddParameter("@gracePeriodMinutes", subscription.GracePeriodMinutes);
            sp.AddParameter("@householdLimitationsId", subscription.HouseholdLimitationsId);
            sp.AddParameter("@discountModuleId", subscription.InternalDiscountModuleId);
            sp.AddParameter("@isActive", subscription.IsActive.HasValue ? subscription.IsActive.Value : false);
            sp.AddParameter("@isCancellationBlocked", subscription.IsCancellationBlocked);
            sp.AddParameter("@needToUpdatePremiumServices", premiumServices == null ? 0 : 1);
            sp.AddDataTableParameter("@premiumServices", premiumServices);
            sp.AddParameter("@preSaleDate", subscription.PreSaleDate);
            sp.AddParameter("@previewModuleId", subscription.PreviewModuleId);
            sp.AddParameter("@needToUpdatePricePlanIds", subscription.PricePlanIds?.Count > 0 ? 1 : 0);
            sp.AddOrderKeyListParameter<long>("@pricePlanIds", subscription.PricePlanIds, "idKey");
            sp.AddParameter("@prorityInOrder", subscription.ProrityInOrder);
            sp.AddParameter("@isRecurring", isRecurring);
            sp.AddParameter("@updaterId", updaterId);

            if (basePricePlanId.HasValue)
            {
                sp.AddParameter("@basePricePlanId", basePricePlanId.ToString());
            }

            if (basePriceCodeId.HasValue)
            {
                sp.AddParameter("@basePriceId", basePriceCodeId.ToString());
            }

            sp.AddParameter("@extDiscountId", extDiscountId.ToString());

            return sp.ExecuteReturnValue<int>();
        }

        private List<KeyValuePair<int, string>> SetExternalProductCodes(List<KeyValuePair<VerificationPaymentGateway, string>> externalProductCodes)
        {
            List<KeyValuePair<int, string>> productCodes = null;

            if (externalProductCodes?.Count > 0)
            {
                productCodes = new List<KeyValuePair<int, string>>();
                foreach (var item in externalProductCodes)
                {
                    productCodes.Add(new KeyValuePair<int, string>((int)item.Key, item.Value));
                }
            }

            return productCodes;
        }

        public bool UpdatePriceDetails(int groupId, long id, bool updatePriceCode, PriceDetailsDTO priceDetails, bool updatePriceCodesLocales, long userId)
        {
            DataTable priceCodesLocalesDt = ConvertToDataTable(priceDetails.Prices);
            var sp = new StoredProcedure("Update_PriceDetails");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@groupId", groupId);
            sp.AddParameter("@id", id);
            sp.AddParameter("@needToUpdatePriceCodes", updatePriceCode);
            sp.AddParameter("@code", priceDetails.Name);
            sp.AddParameter("@price", updatePriceCodesLocales ? "0" : null);
            sp.AddParameter("@currencyId", updatePriceCodesLocales ? "0" : null);
            sp.AddParameter("@priceCodesLocalesExist", (priceDetails.Prices == null || priceDetails.Prices.Count == 0) && !updatePriceCodesLocales ? 0 : 1);
            sp.AddDataTableParameter("@priceCodesLocales", priceCodesLocalesDt);
            sp.AddParameter("@updaterId", userId);

            var result = sp.ExecuteReturnValue<int>() > 0;
            return result;
        }

        public long GetSubscriptionByExternalId(int groupId, string externalId)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_SubscriptionByExternalId");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@groupId", groupId);
            sp.AddParameter("@externalId", externalId);
            return sp.ExecuteReturnValue<long>();
        }

        private DataTable SetCouponGroupsTable(List<SubscriptionCouponGroupDTO> couponGroups)
        {
            DataTable ccTable = new DataTable("CouponGroupValues");

            ccTable.Columns.Add("COUPON_GROUP_ID", typeof(long));
            ccTable.Columns.Add("START_DATE", typeof(DateTime));
            ccTable.Columns.Add("END_DATE", typeof(DateTime));

            if (couponGroups?.Count > 0)
            {
                DataRow dr = null;
                foreach (var couponGroup in couponGroups)
                {
                    dr = ccTable.NewRow();
                    dr["COUPON_GROUP_ID"] = couponGroup.GroupCode;

                    if (couponGroup.StartDate.HasValue)
                    {
                        dr["START_DATE"] = couponGroup.StartDate.Value;
                    }
                    else
                    {
                        dr["START_DATE"] = DBNull.Value;
                    }

                    if (couponGroup.EndDate.HasValue)
                    {
                        dr["END_DATE"] = couponGroup.EndDate.Value;
                    }
                    else
                    {
                        dr["END_DATE"] = DBNull.Value;
                    }

                    ccTable.Rows.Add(dr);
                }
            }

            return ccTable;
        }

        private DataTable SetPremiumServices(ServiceObject[] premiumServices)
        {
            DataTable services = new DataTable("KeyValueIdList");
            services.Columns.Add("idKey", typeof(long));
            services.Columns.Add("value", typeof(long));

            DataRow dr = null;

            foreach (var item in premiumServices)
            {
                dr = services.NewRow();
                dr["idKey"] = item.ID;

                if (item is NpvrServiceObject @npvrServiceObject)
                {
                    dr["value"] = @npvrServiceObject.Quota;
                }

                services.Rows.Add(dr);
            }

            return services;
        }

        public bool UpdateSubscription(int groupId, long updaterId, SubscriptionInternal subscription, long? basePricePlanId, long? basePriceCodeId, bool? isRecurring,
            NullableObj<DateTime?> startDate, NullableObj<DateTime?> endDate, long? extDiscountId)
        {
            DataTable couponGroups = null;
            DataTable premiumServices = null;

            if (subscription.CouponGroups != null)
            {
                couponGroups = SetCouponGroupsTable(subscription.CouponGroups);
            }

            if (subscription.PremiumServices != null)
            {
                premiumServices = SetPremiumServices(subscription.PremiumServices);
            }

            var externalProductCodes = SetExternalProductCodes(subscription.ExternalProductCodes);

            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Update_Subscription");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@groupId", groupId);
            sp.AddParameter("@subscriptionId", subscription.Id);
            sp.AddParameter("@name", subscription.Names != null ? subscription.Names[0].m_sValue : null);
            sp.AddParameter("@needToUpdateNames", subscription.Names == null ? 0 : 1);
            sp.AddKeyValueListParameter<string, string>("@names", subscription.Names == null ? null : subscription.Names.Select(t => new KeyValuePair<string, string>(t.m_sLanguageCode3, t.m_sValue)).ToList(), "key", "value");
            sp.AddParameter("@startDate", startDate.Obj);
            sp.AddParameter("@endDate", endDate.Obj);
            sp.AddParameter("@nullableStartDate", startDate.IsNull);
            sp.AddParameter("@nullableEndDate", endDate.IsNull);
            sp.AddParameter("@needToUpdateChannels", subscription.ChannelsIds == null ? 0 : 1);
            sp.AddIDListParameter<long>("@channels", subscription.ChannelsIds, "Id");
            sp.AddParameter("@adsParams", subscription.AdsParams);
            sp.AddParameter("@adsPolicy", subscription.AdsPolicy.HasValue ? (int)subscription.AdsPolicy.Value : (int?)null);
            sp.AddParameter("@needToUpdateCouponGroups", subscription.CouponGroups == null ? 0 : 1);
            sp.AddDataTableParameter("@couponGroups", couponGroups);
            sp.AddParameter("@type", (int)subscription.DependencyType);
            sp.AddParameter("@needToUpdateDescriptions", subscription.Descriptions == null ? 0 : 1);
            sp.AddKeyValueListParameter<string, string>("@descriptions", subscription.Descriptions == null ? null : subscription.Descriptions.Select(t => new KeyValuePair<string, string>(t.m_sLanguageCode3, t.m_sValue)).ToList(), "key", "value");
            sp.AddParameter("@externalId", subscription.ExternalId);
            sp.AddParameter("@needToUpdateProductCodes", externalProductCodes == null ? 0 : 1);
            sp.AddKeyValueListParameter<int, string>("@productCodes", externalProductCodes, "idKey", "value");
            sp.AddParameter("@needToUpdateFileTypes", subscription.FileTypesIds == null ? 0 : 1);
            sp.AddIDListParameter<long>("@fileTypesIds", subscription.FileTypesIds, "Id");
            sp.AddParameter("@gracePeriodMinutes", subscription.GracePeriodMinutes);
            sp.AddParameter("@householdLimitationsId", subscription.HouseholdLimitationsId);
            sp.AddParameter("@discountModuleId", subscription.InternalDiscountModuleId);
            sp.AddParameter("@isActive", subscription.IsActive);
            sp.AddParameter("@isCancellationBlocked", subscription.IsCancellationBlocked);
            sp.AddParameter("@needToUpdatePremiumServices", subscription.PremiumServices != null && subscription.PremiumServices.Length >= 0 ? 1 : 0);
            sp.AddDataTableParameter("@premiumServices", premiumServices);
            sp.AddParameter("@preSaleDate", subscription.PreSaleDate);
            sp.AddParameter("@previewModuleId", subscription.PreviewModuleId);
            sp.AddParameter("@needToUpdatePricePlanIds", subscription.PricePlanIds == null ? 0 : 1);
            sp.AddOrderKeyListParameter<long>("@pricePlanIds", subscription.PricePlanIds, "idKey");
            sp.AddParameter("@prorityInOrder", subscription.ProrityInOrder);
            sp.AddParameter("@isRecurring", isRecurring);
            sp.AddParameter("@updaterId", updaterId);
            sp.AddParameter("@basePricePlanId", basePricePlanId.HasValue ? basePricePlanId.ToString() : null);
            sp.AddParameter("@basePriceId", basePriceCodeId.HasValue ? basePriceCodeId.ToString() : null);
            sp.AddParameter("@extDiscountId", extDiscountId.ToString());

            var result = sp.ExecuteReturnValue<int>() > 0;
            return result;
        }

        public bool UpdateSubscriptionVirtualAssetId(int groupId, long id, long? virtualAssetId, long userId)
        {
            var sp = new StoredProcedure("UpdateSubscriptionVirtualAssetId");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@groupId", groupId);
            sp.AddParameter("@id", id);
            sp.AddParameter("@virtualAssetId", virtualAssetId);
            sp.AddParameter("@userId", userId);

            return sp.ExecuteReturnValue<int>() > 0;
        }

        public int DeleteSubscription(int groupId, long id, long updaterId)
        {
            StoredProcedure sp = new StoredProcedure("Delete_SubscriptionById");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@groupId", groupId);
            sp.AddParameter("@id", id);
            sp.AddParameter("@updaterId", updaterId);
            return sp.ExecuteReturnValue<int>();
        }

        public static List<SubscriptionItemDTO> GetGroupSubscriptionsItems(int groupId)
        {
            List<SubscriptionItemDTO> res = new List<SubscriptionItemDTO>();

            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_GroupSubscriptionsItems");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@groupId", groupId);
            DataTable dt = sp.Execute();
            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
            {
                foreach (DataRow item in dt.Rows)
                {
                    var sub = new SubscriptionItemDTO()
                    {
                        Id = ODBCWrapper.Utils.GetLongSafeVal(item, "ID"),
                        StartDate = ODBCWrapper.Utils.GetDateSafeVal(item, "START_DATE"),
                        UpdateDate = ODBCWrapper.Utils.GetDateSafeVal(item, "UPDATE_DATE"),
                        IsActive = ODBCWrapper.Utils.GetIntSafeVal(item, "IS_ACTIVE") == 1,
                        Name = ODBCWrapper.Utils.GetSafeStr(item, "NAME"),
                    };

                    res.Add(sub);
                }
            }

            return res;
        }

        public long GetCollectionByExternalId(int groupId, string externalId)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_CollectionByExternalId");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@groupId", groupId);
            sp.AddParameter("@externalId", externalId);
            return sp.ExecuteReturnValue<long>();
        }

        public bool UpdateCollection(int groupId, long updaterId, CollectionInternal collection, NullableObj<DateTime?> startDate, NullableObj<DateTime?> endDate, long? virtualAssetId)
        {
            DataTable couponGroups = null;
            if (collection.CouponGroups?.Count > 0)
            {
                couponGroups = SetCouponGroupsTable(collection.CouponGroups);
            }

            var externalProductCodes = SetExternalProductCodes(collection.ExternalProductCodes);


            var sp = new StoredProcedure("Update_Collection");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@groupId", groupId);
            sp.AddParameter("@collectionId", collection.Id);
            sp.AddParameter("@needToUpdateChannels", collection.ChannelsIds == null ? 0 : 1);
            sp.AddIDListParameter<long>("@channels", collection.ChannelsIds, "Id");
            sp.AddParameter("@needToUpdateCouponGroups", couponGroups == null ? 0 : 1);
            sp.AddDataTableParameter("@couponGroups", couponGroups);
            sp.AddParameter("@needToUpdateDescriptions", collection.Descriptions == null ? 0 : 1);
            sp.AddKeyValueListParameter<string, string>("@descriptions", collection.Descriptions?.Select(t => new KeyValuePair<string, string>(t.m_sLanguageCode3, t.m_sValue)).ToList(), "key", "value");
            sp.AddParameter("@externalId", collection.ExternalId);
            sp.AddParameter("@needToUpdateProductCodes", externalProductCodes == null ? 0 : 1);
            sp.AddKeyValueListParameter<int, string>("@productCodes", externalProductCodes, "key", "value");
            sp.AddParameter("@discountModuleId", collection.DiscountModuleId);
            sp.AddParameter("@usageModuleId", collection.UsageModuleId);
            sp.AddParameter("@isActive", collection.IsActive);
            sp.AddParameter("@name", collection.Names != null && collection.Names.Length > 0 ? collection.Names[0].m_sValue : null);
            sp.AddKeyValueListParameter<string, string>("@names", collection.Names?.Select(t => new KeyValuePair<string, string>(t.m_sLanguageCode3, t.m_sValue)).ToList(), "key", "value");
            sp.AddParameter("@priceDetailsId", collection.PriceDetailsId);
            sp.AddParameter("@startDate", startDate.Obj);
            sp.AddParameter("@endDate", endDate.Obj);
            sp.AddParameter("@nullableStartDate", startDate.IsNull);
            sp.AddParameter("@nullableEndDate", endDate.IsNull);
            sp.AddParameter("@updaterId", updaterId);
            if (virtualAssetId.HasValue)
            {
                sp.AddParameter("@virtualAssetId", virtualAssetId.Value);
            }
            if (collection.FileTypesIds != null)
            {
                sp.AddParameter("@fileTypesIds_json", JsonConvert.SerializeObject(collection.FileTypesIds));
            }

            var result = sp.ExecuteReturnValue<int>() > 0;
            return result;
        }

        public List<long> GetCollectionsByChannelId(int groupId, long channelId)
        {
            var sp = new StoredProcedure("Get_CollectionsByChannelId");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@groupId", groupId);
            sp.AddParameter("@channelId", channelId);

            DataSet ds = sp.ExecuteDataSet();

            if (ds?.Tables?.Count > 0)
            {
                List<long> res = new List<long>();
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    res.Add(Utils.GetLongSafeVal(dr, "COLLECTION_ID"));
                }

                return res;
            }

            return new List<long>();
        }

        public void DeleteCollectionsChannelsByChannel(int groupId, long userId, long channelId)
        {
            StoredProcedure sp = new StoredProcedure("Delete_CollectionsChannelsByChannel");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@groupId", groupId);
            sp.AddParameter("@channelId", channelId);
            sp.AddParameter("@updaterId", userId);
            sp.Execute();
        }

        public List<int> GetSubscriptionsByChannelId(int groupId, int channelId)
        {
            var sp = new StoredProcedure("Get_SubscriptionsByChannelId");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@groupId", groupId);
            sp.AddParameter("@channelId", channelId);

            DataSet ds = sp.ExecuteDataSet();

            if (ds?.Tables?.Count > 0)
            {
                List<int> res = new List<int>();
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    res.Add(Utils.GetIntSafeVal(dr, "SUBSCRIPTION_ID"));
                }

                return res;
            }

            return new List<int>();
        }

        public void DeleteSubscriptionsChannelsByChannel(int groupId, int channelId)
        {
            StoredProcedure sp = new StoredProcedure("Delete_SubscriptionsChannelsByChannel");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@groupId", groupId);
            sp.AddParameter("@channelId", channelId);
            sp.Execute();
        }

        public bool UpdateCollectionVirtualAssetId(int groupId, long id, long virtualAssetId, long userId)
        {
            var sp = new StoredProcedure("UpdateCollectionVirtualAssetId");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@groupId", groupId);
            sp.AddParameter("@id", id);
            sp.AddParameter("@virtualAssetId", virtualAssetId);
            sp.AddParameter("@userId", userId);

            return sp.ExecuteReturnValue<int>() > 0;
        }
    }
}