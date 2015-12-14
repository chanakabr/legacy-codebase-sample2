using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using ODBCWrapper;

namespace DAL
{
    public class TvmDAL
    {
               
        public static int GetSubscriptionsNotifierImpl(int nGroupID, int nModuleID)
        {
            int nImplID = 0;

            try
            {                                              
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_SubNotifierImplementation");
                sp.SetConnectionKey("MAIN_CONNECTION_STRING");
                sp.AddParameter("@GroupID", nGroupID);
                sp.AddParameter("@ModuleID", nModuleID);

                DataSet ds = sp.ExecuteDataSet();

                if (ds != null && ds.Tables != null && ds.Tables.Count > 0 && ds.Tables[0] != null && ds.Tables[0].DefaultView.Count > 0)
                {
                    DataTable dt = ds.Tables[0];

                    if (dt == null || dt.Rows.Count == 0)
                    {
                        return nImplID;
                    }

                    nImplID = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0], "IMPLEMENTATION_ID");
                }

            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return nImplID;
        }

        public static Dictionary<string, string> GetSubscriptionInfo(int nGroupID, string sSubscriptionID)
        {
            Dictionary<string, string> prodDict = new Dictionary<string, string>();

            try
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey("PRICING_CONNECTION");

                selectQuery += "SELECT S.ID, S.COGUID, SN.DESCRIPTION AS 'TITLE', SD.DESCRIPTION, S.IS_ACTIVE, COALESCE(PC.PRICE, 0) AS 'PRICE', LC.CODE3, S.START_DATE, S.END_DATE " + 
                                "FROM SUBSCRIPTIONS S WITH (NOLOCK) " +
                                "LEFT JOIN SUBSCRIPTION_NAMES SN WITH (NOLOCK) ON S.ID = SN.SUBSCRIPTION_ID " +
                                "LEFT JOIN SUBSCRIPTION_DESCRIPTIONS SD WITH (NOLOCK) ON S.ID = SD.SUBSCRIPTION_ID " +
                                "LEFT JOIN PRICE_CODES PC WITH (NOLOCK) ON S.SUB_PRICE_CODE = PC.ID " +
                                "LEFT JOIN LU_CURRENCY LC WITH (NOLOCK) ON PC.CURRENCY_CD = LC.ID " +
                                "WHERE S.STATUS = 1 AND ";

                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("S.GROUP_ID", "=", nGroupID);
                selectQuery += "AND";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("S.ID", "=", sSubscriptionID);
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        prodDict["InternalProductID"]   = selectQuery.Table("query").DefaultView[0].Row["ID"].ToString();
                        prodDict["ExternalProductID"]   = selectQuery.Table("query").DefaultView[0].Row["COGUID"].ToString();
                        prodDict["Title"]               = selectQuery.Table("query").DefaultView[0].Row["TITLE"].ToString();
                        prodDict["Description"]         = selectQuery.Table("query").DefaultView[0].Row["DESCRIPTION"].ToString();
                        prodDict["Status"]              = selectQuery.Table("query").DefaultView[0].Row["IS_ACTIVE"].ToString();

                        prodDict["PriceB2C"] = "0.0";
                        if (selectQuery.Table("query").DefaultView[0].Row["PRICE"] != null)
                        {
                            prodDict["PriceB2C"] = selectQuery.Table("query").DefaultView[0].Row["PRICE"].ToString();
                        }

                        prodDict["StartDate"] = new DateTime(2000, 1, 1).ToString();
                        if (selectQuery.Table("query").DefaultView[0].Row["START_DATE"] != null &&
                            selectQuery.Table("query").DefaultView[0].Row["START_DATE"] != DBNull.Value)
                        {
                            prodDict["StartDate"] = selectQuery.Table("query").DefaultView[0].Row["START_DATE"].ToString();
                        }

                        prodDict["EndDate"] = new DateTime(2099, 1, 1).ToString();
                        if (selectQuery.Table("query").DefaultView[0].Row["END_DATE"] != null &&
                            selectQuery.Table("query").DefaultView[0].Row["END_DATE"] != DBNull.Value)
                        {
                            prodDict["EndDate"] = selectQuery.Table("query").DefaultView[0].Row["END_DATE"].ToString();
                        }
                    }

                }

                selectQuery.Finish();
                selectQuery = null;                
            }
            catch (Exception)
            {
            }

            return prodDict;
        }


        private static void HandleException(Exception ex)
        {
        }



        public static List<string> GetSubscriptionOperatorCoGuids(int nGroupID, string sSubscriptionID)
        {
            List<string> lOperatorCoGuids = new List<string>();

            try
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey("PRICING_CONNECTION");

                selectQuery += "SELECT GO.CLIENT_ID FROM SUBSCRIPTION_OPERATORS SO WITH (NOLOCK) JOIN TVINCI..GROUPS_OPERATORS GO WITH (NOLOCK) ON GO.ID = SO.OPERATOR_ID " +
                                "WHERE SO.IS_ACTIVE = 1 AND SO.STATUS = 1 AND ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("SO.GROUP_ID", "=", nGroupID);
                selectQuery += "AND";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("SO.SUBSCRIPTION_ID", "=", sSubscriptionID);
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        for (int i = 0; i < nCount; i++)
			            {
                            string operatorCoGuid = selectQuery.Table("query").DefaultView[i].Row["CLIENT_ID"].ToString();
                            lOperatorCoGuids.Add(operatorCoGuid);
			            }
                    }

                }

                selectQuery.Finish();
                selectQuery = null;
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return lOperatorCoGuids.Distinct().ToList();
        }

        public static List<int> GetSubscriptionChannelIDs(int nGroupID, string sSubscriptionID)
        {
            List<int> lChannelIDs = new List<int>();

            try
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey("PRICING_CONNECTION");

                selectQuery += "SELECT CHANNEL_ID FROM SUBSCRIPTIONS_CHANNELS WITH (NOLOCK) WHERE IS_ACTIVE = 1 AND STATUS = 1 AND ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
                selectQuery += "AND";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("SUBSCRIPTION_ID", "=", sSubscriptionID);
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        for (int i = 0; i < nCount; i++)
                        {
                            int channelID = int.Parse(selectQuery.Table("query").DefaultView[i].Row["CHANNEL_ID"].ToString());
                            lChannelIDs.Add(channelID);
                        }
                    }

                }

                selectQuery.Finish();
                selectQuery = null;
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return lChannelIDs.Distinct().ToList();

        }

        public static Dictionary<string, string> GetVirtualPackageInfo(int nGroupID, string sMediaID)
        {
            Dictionary<string, string> dPackage = new Dictionary<string, string>();

            dPackage["InternalProductID"] = null;
            dPackage["ExternalProductID"] = null;
            dPackage["OperatorID"] = null;
            dPackage["Title"] = null;
            dPackage["Price"] = null;
            dPackage["Description"] = null;
            dPackage["StartDate"] = null;
            dPackage["EndDate"] = null;
            dPackage["ImageUrl"] = null;


            try
            {

                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_PackageDetails");
                sp.SetConnectionKey("MAIN_CONNECTION_STRING");
                sp.AddParameter("@GroupID", nGroupID);
                sp.AddParameter("@MediaID", sMediaID);

                DataSet ds = sp.ExecuteDataSet();

                if (ds != null && ds.Tables != null && ds.Tables.Count > 0 && ds.Tables[0] != null && ds.Tables[0].DefaultView.Count > 0)
                {
                    DataTable dt = ds.Tables[0];

                    if (dt == null || dt.Rows.Count == 0)
                    {
                        return dPackage;
                    }

                    dPackage["InternalProductID"] = ODBCWrapper.Utils.GetSafeStr(dt.Rows[0], "ID");

                    dPackage["ExternalProductID"] = ODBCWrapper.Utils.GetSafeStr(dt.Rows[0], "COGUID");

                    dPackage["OperatorID"] = ODBCWrapper.Utils.GetSafeStr(dt.Rows[0], "OPERATOR_ID");

                    dPackage["Title"] = ODBCWrapper.Utils.GetSafeStr(dt.Rows[0], "TITLE");

                    dPackage["Price"] = ODBCWrapper.Utils.GetSafeStr(dt.Rows[0], "PRICE");

                    dPackage["Description"] = ODBCWrapper.Utils.GetSafeStr(dt.Rows[0], "DESCRIPTION");

                    dPackage["StartDate"] = ODBCWrapper.Utils.GetSafeStr(dt.Rows[0], "START_DATE");

                    dPackage["EndDate"] = ODBCWrapper.Utils.GetSafeStr(dt.Rows[0], "END_DATE");

                    dt = ds.Tables[1];

                    if (dt == null || dt.Rows.Count == 0)
                    {
                        return dPackage;
                    }

                    dPackage["ImageUrl"] = ODBCWrapper.Utils.GetSafeStr(dt.Rows[0], "PIC_URL");

                    dt = null;
                }

                ds = null;
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return dPackage;
        }


        public static List<KeyValuePair<string,string>> GetMediaDescription(List<string> lEpgIdentifier)
        {
            List<KeyValuePair<string, string>> lMediaDescription = new  List<KeyValuePair<string,string>>(); 
            KeyValuePair<string, string> keyValuePair;
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("GetMediaDescription");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddIDListParameter<string>("@EpgIdentifier", lEpgIdentifier, "key");
            DataSet ds = sp.ExecuteDataSet();

            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
            {
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    keyValuePair = new KeyValuePair<string,string>(row["epg_identifier"].ToString(), row["media_description"].ToString());

                    lMediaDescription.Add(keyValuePair);
                }
                return lMediaDescription;
            }

            return null;
        }

        public static bool Insert_DeviceFamilyToGroup(int nGroupID, int nDeviceFamilyID, int nLimitationModuleID, string sConnKey)
        {
            StoredProcedure sp = new StoredProcedure("Insert_DeviceFamilyToGroup");
            sp.SetConnectionKey(!string.IsNullOrEmpty(sConnKey) ? sConnKey : "CONNECTION_STRING");
            sp.AddParameter("@GroupID", nGroupID);
            sp.AddParameter("@DeviceFamilyID", nDeviceFamilyID);
            sp.AddParameter("@LimitationModuleID", nLimitationModuleID);

            return sp.ExecuteReturnValue<long>() > 0;
        }

        public static bool Insert_DeviceFamilyToGroup(int nGroupID, int nDeviceFamilyID, int nLimitationModuleID)
        {
            return Insert_DeviceFamilyToGroup(nGroupID, nDeviceFamilyID, nLimitationModuleID, string.Empty);
        }

        public static bool Update_DeviceFamilyStatus(int nGroupID, int nDeviceFamilyID, int nLimitationModuleID, bool bIsDelete,
            string sConnKey)
        {
            StoredProcedure sp = new StoredProcedure("Update_DeviceFamilyStatus");
            sp.SetConnectionKey(!string.IsNullOrEmpty(sConnKey) ? sConnKey : "CONNECTION_STRING");
            sp.AddParameter("@GroupID", nGroupID);
            sp.AddParameter("@DeviceFamilyID", nDeviceFamilyID);
            sp.AddParameter("@LimitationModuleID", nLimitationModuleID);
            sp.AddParameter("@NewStatus", bIsDelete ? 2 : 1);

            return sp.ExecuteReturnValue<bool>();
            
        }

        public static bool Update_DeviceFamilyStatus(int nGroupID, int nDeviceFamilyID, int nLimitationModuleID, bool bIsDelete)
        {
            return Update_DeviceFamilyStatus(nGroupID, nDeviceFamilyID, nLimitationModuleID, bIsDelete, string.Empty);
        }

        public static bool Insert_DeviceFamilyToLimitationModule(int nGroupID, int nDeviceFamilyID, int nLimitationModuleID, string sConnKey)
        {
            StoredProcedure sp = new StoredProcedure("Insert_DeviceFamilyToLimitationModule");
            sp.SetConnectionKey(!string.IsNullOrEmpty(sConnKey) ? sConnKey : "CONNECTION_STRING");
            sp.AddParameter("@DeviceFamilyID", nDeviceFamilyID);
            sp.AddParameter("@GroupID", nGroupID);
            sp.AddParameter("@LimitationModuleID", nLimitationModuleID);

            return sp.ExecuteReturnValue<long>() > 0;
        }

        public static bool Insert_DeviceFamilyToLimitationModule(int nGroupID, int nDeviceFamilyID, int nLimitationModuleID)
        {
            return Insert_DeviceFamilyToLimitationModule(nGroupID, nDeviceFamilyID, nLimitationModuleID, string.Empty);
        }

        public static bool Update_DeviceFamilyToLimitationID(int nGroupID, int nLimitationID, int nDeviceFamilyID, bool bIsDelete,
            string sConnKey)
        {
            StoredProcedure sp = new StoredProcedure("Update_DeviceFamilyToLimitationID");
            sp.SetConnectionKey(!string.IsNullOrEmpty(sConnKey) ? sConnKey : "CONNECTION_STRING");
            sp.AddParameter("@LimitationID", nLimitationID);
            sp.AddParameter("@GroupID", nGroupID);
            sp.AddParameter("@DeviceFamilyID", nDeviceFamilyID);
            sp.AddParameter("@IsDelete", bIsDelete ? 2 : 1);

            return sp.ExecuteReturnValue<bool>();
        }

        public static bool Update_DeviceFamilyToLimitationID(int nGroupID, int nLimitationID, int nDeviceFamilyID, bool bIsDelete)
        {
            return Update_DeviceFamilyToLimitationID(nGroupID, nLimitationID, nDeviceFamilyID, bIsDelete, string.Empty);
        }

        public static DataSet Get_DeviceFamiliesLimitationsData(int nGroupID, int nLimitationID, string sConnKey)
        {
            StoredProcedure sp = new StoredProcedure("Get_DeviceFamiliesLimitationsData");
            sp.SetConnectionKey(!string.IsNullOrEmpty(sConnKey) ? sConnKey : "CONNECTION_STRING");
            sp.AddParameter("@GroupID", nGroupID);
            sp.AddParameter("@LimitationID", nLimitationID);

            return sp.ExecuteDataSet();

        }

        public static DataSet Get_DeviceFamiliesLimitationsData(int nGroupID, int nLimitationID)
        {
            return Get_DeviceFamiliesLimitationsData(nGroupID, nLimitationID, string.Empty);
        }

        public static long GetPackageMediaID(int nGroupID, string sMediaID)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_PackageMediaID");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@GroupID", nGroupID);
            sp.AddParameter("@MediaID", sMediaID);

            long mediaID = sp.ExecuteReturnValue<long>();

            return mediaID;
        }


        public static DataTable GetTagsWithDefaultValues(int nChannelID, int nGroupID)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("GetTagsWithDefaultValues");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@channelID", nChannelID);
            sp.AddParameter("@groupID", nGroupID);


            DataSet ds = sp.ExecuteDataSet();

            if (ds != null)
                return ds.Tables[0];
            return null;

        }

        public static DataTable GetChildGroupTreeStr(int nGroupID)
        {
            DataTable dtGroups = null;

            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetConnectionKey("MAIN_CONNECTION_STRING");

            selectQuery += "select * from dbo.F_Get_GroupsChild(" + nGroupID.ToString() + ")";

            if (selectQuery.Execute("query", true) != null)
            {
                dtGroups = selectQuery.Table("query");
            }
            selectQuery.Finish();
            selectQuery = null;

            return dtGroups;
        }


        public static DataSet Get_ChannelMediaTypes(int groupID, int channelID)
        {
            StoredProcedure sp = new StoredProcedure("Get_ChannelMediaTypes");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@GroupID", groupID);
            sp.AddParameter("@ChannelID", channelID);

            return sp.ExecuteDataSet();

        }

        public static DataSet Get_ChannelAssetTypes(int groupID, int channelID)
        {
            StoredProcedure storedProcedure = new StoredProcedure("Get_ChannelAssetTypes");
            storedProcedure.SetConnectionKey("MAIN_CONNECTION_STRING");
            storedProcedure.AddParameter("@GroupID", groupID);
            storedProcedure.AddParameter("@ChannelID", channelID);

            return storedProcedure.ExecuteDataSet();
        }

        public static DataTable GetChannelMediaType(int groupID, int channelID, int mediaTypeID)
        {
            StoredProcedure sp = new StoredProcedure("GetChannelMediaType");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@GroupID", groupID);
            sp.AddParameter("@ChannelID", channelID);
            sp.AddParameter("@MediaTypeID", mediaTypeID);

             DataSet ds = sp.ExecuteDataSet();
             if (ds != null)
                return ds.Tables[0];
            return null;
        }


        public static bool UpdateChannelMediaType(int channelMediaTypeID, int status, int groupID, int channelID)
        {
            StoredProcedure sp = new StoredProcedure("UpdateChannelMediaType");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@GroupID", groupID);
            sp.AddParameter("@Status", status);
            sp.AddParameter("@channelMediaTypeID", channelMediaTypeID);
            sp.AddParameter("@channelID", channelID);

            return sp.ExecuteReturnValue<bool>();
        }

        public static bool UpdateChannelAssetType(int channelAssetTypeID, int status, int groupID, int channelID)
        {
            return UpdateChannelMediaType(channelAssetTypeID, status, groupID, channelID);

            //StoredProcedure sp = new StoredProcedure("UpdateChannelAssetType");
            //sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            //sp.AddParameter("@GroupID", groupID);
            //sp.AddParameter("@Status", status);
            //sp.AddParameter("@channelAssetTypeID", channelAssetTypeID);
            //sp.AddParameter("@channelID", channelID);

            //return sp.ExecuteReturnValue<bool>();
        }

        public static bool InsertChannelMediaType(int groupID, int channelID, List<int> mediaTypeIDs)
        {
            StoredProcedure sp = new StoredProcedure("InsertChannelMediaType");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@GroupID", groupID);
            sp.AddParameter("@ChannelID", channelID);
            sp.AddIDListParameter<int>("@MediaTypeID", mediaTypeIDs, "Id");

            return sp.ExecuteReturnValue<bool>();
        }

        public static bool Insert_ChannelAssetType(int groupID, int channelID, List<int> assetTypeIDs)
        {
            StoredProcedure sp = new StoredProcedure("Insert_ChannelAssetType");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@GroupID", groupID);
            sp.AddParameter("@ChannelID", channelID);
            sp.AddIDListParameter<int>("@AssetTypeID", assetTypeIDs, "Id");

            return sp.ExecuteReturnValue<bool>();
        }


        public static bool insertValueToLookupTable(DataTable dt)
        {
            StoredProcedure sp = new StoredProcedure("insertValueToLookupTable");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");           
            sp.AddDataTableParameter("@dt", dt);
            
            return sp.ExecuteReturnValue<bool>();
        }
    }
    
}
