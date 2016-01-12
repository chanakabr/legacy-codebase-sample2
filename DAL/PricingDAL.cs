using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using ODBCWrapper;

namespace DAL
{
    public class PricingDAL
    {
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

        public static Dictionary<string, int> Get_SubscriptionsFromProductCodes(List<string> productCodes, int groupID)
        {
            Dictionary<string, int> ret = new Dictionary<string,int>();

            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_SubscriptionsFromProductCodes");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@GroupID", groupID);
            sp.AddIDListParameter("@ProductCodesList", productCodes, "id");

            DataSet ds = sp.ExecuteDataSet();

            if (ds != null && ds.Tables != null && ds.Tables.Count > 0 && ds.Tables[0].Rows != null && ds.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    string productCode = ODBCWrapper.Utils.GetSafeStr(row["Product_Code"]);
                    int id = ODBCWrapper.Utils.GetIntSafeVal(row, "ID");
                    ret.Add(productCode, id);
                }
            }

            return ret;
        }

        public static Dictionary<string, int> Get_PPVsFromProductCodes(List<string> productCodes, int groupID)
        {
            Dictionary<string, int> ret = new Dictionary<string, int>();

            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_PPVsFromProductCodes");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@GroupID", groupID);
            sp.AddIDListParameter("@ProductCodesList", productCodes, "id");

            DataSet ds = sp.ExecuteDataSet();

            if (ds != null && ds.Tables != null && ds.Tables.Count > 0 && ds.Tables[0].Rows != null && ds.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    string productCode = ODBCWrapper.Utils.GetSafeStr(row["Product_Code"]);
                    int id = ODBCWrapper.Utils.GetIntSafeVal(row, "ID");
                    ret.Add(productCode, id);
                }
            }

            return ret;
        }
    }
}

