using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using ODBCWrapper;
using KLogMonitor;
using System.Reflection;
using ApiObjects.AssetLifeCycleRules;
using ApiObjects.IngestBusinessModules;
using System.Xml.Serialization;
using System.IO;
using System.Xml;

namespace DAL
{
    public class PricingDAL
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

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

        public static DataTable Get_PreviewModulesByGroupID(int nGroupID, bool bIsActive, bool bNotDeleted)
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

        public static int InsertPreviewModule(int groupID, string name, int fullLifeCycle, int nonRenewPeriod, string alias)
        {
            try
            {
                StoredProcedure sp = new StoredProcedure("Insert_NewPreviewModule");
                sp.SetConnectionKey("pricing_connection");
                sp.AddParameter("@GroupID", groupID);
                sp.AddParameter("@Name", name);
                sp.AddParameter("@FullLifeCycle", fullLifeCycle);
                sp.AddParameter("@NonRenewPeriod", nonRenewPeriod);
                sp.AddParameter("@Alias", alias);
                sp.AddParameter("@Date", DateTime.UtcNow);

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
            string previewModule, List<string> couponGroups)
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

            return sp.Execute();
        }

        public static int InsertMPP(int groupID, ApiObjects.IngestMultiPricePlan mpp, List<KeyValuePair<long, int>> pricePlansCodes, List<long> channels, List<long> fileTypes,
            int previewModuleID, int internalDiscountID, XmlDocument couponsGroups)
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
            int previewModuleID, int internalDiscountID, XmlDocument couponsGroups)
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
            sp.AddParameter("@xmlDocRowCount", (couponsGroups.ChildNodes != null && couponsGroups.ChildNodes.Count > 0) ? 1 : 0);

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
            sp.AddParameter("@Discount", discount);;

            return sp.Execute();
        }

        public static int InsertPricePlan(int groupID, ApiObjects.IngestPricePlan pricePlan, int pricCodeID, int fullLifeCycleID, int viewLifeCycleID, int discountID)
        {
            try
            {
                StoredProcedure sp = new StoredProcedure("Insert_PricePlan");
                sp.SetConnectionKey("pricing_connection");
                sp.AddParameter("@GroupID", groupID);
                sp.AddParameter("@Name", pricePlan.Code);
                sp.AddParameter("@IsActive", pricePlan.IsActive);
                sp.AddParameter("@MaxViews", pricePlan.MaxViews);
                sp.AddParameter("@IsRenewable", pricePlan.IsRenewable);
                sp.AddParameter("@RecurringPeriods", pricePlan.RecurringPeriods);
                sp.AddParameter("@PricCodeID", pricCodeID);
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
    }
}

