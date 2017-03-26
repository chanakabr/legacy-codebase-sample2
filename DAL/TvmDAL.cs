using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using ODBCWrapper;
using KLogMonitor;

namespace DAL
{
    public class TvmDAL
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
               
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

        #region Permissions Methods

        #region Constants

        private const string GET_USERS_QUERY = "select distinct id, username from accounts where status=1  order by username";
        private const string USER_ALLOWED_PERMISSION_QUERY = "select distinct menu_id, menu_text, menu_href from admin_accounts_permissions ap join admin_menu menu on (menu.ID=ap.menu_id) where menu.status=1 and ap.status=1 and account_id = {0} order by menu_id";
        private const string USER_NOT_ALLOWED_PERMISSION_QUERY = "select distinct menu.id menu_id, menu_text, menu_href from admin_menu menu left join admin_accounts_permissions ap  on (menu.ID=ap.menu_id) where menu.id not in (select distinct menu_id from admin_accounts_permissions where status=1 and account_id = {0}) and menu.status=1 order by menu_id";
        private const string GET_USER_SPECIFIC_MENU_STATUS_QUERY = "select top(1) status from admin_accounts_permissions where account_id={0} and menu_id={1} order by status desc";
        private const string GET_GROUPS_QUERY = "select distinct moto_text from accounts where (moto_text is not null or moto_text<>'') and status=1 and is_active=1 order by moto_text";
        private const string GET_USERS_IN_GROUP_QUERY = "select distinct id, username from accounts where moto_text='{0}' and status=1 and is_active=1 order by username";
        private const string GET_USERS_NOT_IN_GROUP_QUERY = "select distinct id, username from accounts where (moto_text is null or moto_text<>'{0}') and status=1 and is_active=1 order by username";
        private const string GET_MENUS_QUERY = "select distinct id, menu_text, menu_href from admin_menu where status=1 order by id";
        private const string GET_MENUS_TO_ADD_QUREY = "select distinct menu_id from admin_accounts_permissions where account_id={0} and status=1 and menu_id not in (select menu_id	from admin_accounts_permissions	where account_id={1} and status=1)";

        #endregion

        public static DataTable GetAllUsers()
        {
            DataTable dt = null;
            DataSetSelectQuery selectQuery = null;
            try
            {
                selectQuery = new DataSetSelectQuery();
                selectQuery += GET_USERS_QUERY;
                selectQuery.SetCachedSec(0);

                if (selectQuery.Execute("usersQuery", true) != null)
                {
                    dt = selectQuery.Table("usersQuery");
                }
                selectQuery.Finish();
                selectQuery = null;

                return dt;
            }
            catch (Exception ex)
            {
                log.Error("Error occurred while trying to execute GetAllUsers", ex);
            }

            return null;
        }

        public static string GetUser(int id)
        {
            string user = string.Empty;
            DataSetSelectQuery selectQuery = null;
            try
            {
                selectQuery = new DataSetSelectQuery();
                selectQuery += string.Format("select username from accounts where status=1 and id={0}", id);
                selectQuery.SetCachedSec(0);

                if (selectQuery.Execute("userQuery", true) != null)
                {
                    DataTable dt = selectQuery.Table("userQuery");
                    if (dt != null && dt.Rows.Count == 1)
                    {
                        user = Utils.GetSafeStr(dt.Rows[0], "username");
                    }
                }
                selectQuery.Finish();
                selectQuery = null;

                return user;
            }
            catch (Exception ex)
            {
                log.Error("Error occurred while trying to execute GetUser", ex);
            }

            return null;
        }

        public static DataTable GetUsersAllowedMenus(int userID)
        {
            DataTable dt = null;
            DataSetSelectQuery selectQuery = null;
            try
            {
                selectQuery = new DataSetSelectQuery();
                selectQuery += string.Format(USER_ALLOWED_PERMISSION_QUERY, userID);
                selectQuery.SetCachedSec(0);

                if (selectQuery.Execute("userAllowedQuery", true) != null)
                {
                    dt = selectQuery.Table("userAllowedQuery");
                }
                selectQuery.Finish();
                selectQuery = null;

                return dt;
            }
            catch (Exception ex)
            {
                log.Error("Error occurred while trying to execute GetUsersAllowedMenus", ex);
            }

            return null;
        }

        public static DataTable GetUsersNotAllowedMenus(int userID)
        {
            DataTable dt = null;
            DataSetSelectQuery selectQuery = null;
            try
            {
                selectQuery = new DataSetSelectQuery();
                selectQuery += string.Format(USER_NOT_ALLOWED_PERMISSION_QUERY, userID);
                selectQuery.SetCachedSec(0);

                if (selectQuery.Execute("userNotAllowedQuery", true) != null)
                {
                    dt = selectQuery.Table("userNotAllowedQuery");
                }
                selectQuery.Finish();
                selectQuery = null;

                return dt;
            }
            catch (Exception ex)
            {
                log.Error("Error occurred while trying to execute GetUsersNotAllowedMenus", ex);
            }

            return null;
        }

        public static DataTable GetAllGroups()
        {
            DataTable dt = null;
            DataSetSelectQuery selectQuery = null;
            try
            {
                selectQuery = new DataSetSelectQuery();
                selectQuery += GET_GROUPS_QUERY;
                selectQuery.SetCachedSec(0);

                if (selectQuery.Execute("groupsQuery", true) != null)
                {
                    dt = selectQuery.Table("groupsQuery");
                }
                selectQuery.Finish();
                selectQuery = null;

                return dt;
            }
            catch (Exception ex)
            {
                log.Error("Error occurred while trying to execute GetAllGroups", ex);
            }

            return null;
        }

        public static DataTable GetAllUsersInGroup(string groupName)
        {
            DataTable dt = null;
            DataSetSelectQuery selectQuery = null;
            try
            {
                selectQuery = new DataSetSelectQuery();
                selectQuery += string.Format(GET_USERS_IN_GROUP_QUERY, groupName);
                selectQuery.SetCachedSec(0);

                if (selectQuery.Execute("usersIngroupQuery", true) != null)
                {
                    dt = selectQuery.Table("usersIngroupQuery");
                }
                selectQuery.Finish();
                selectQuery = null;

                return dt;
            }
            catch (Exception ex)
            {
                log.Error("Error occurred while trying to execute GetAllUsersInGroup", ex);
            }

            return null;
        }

        public static DataTable GetAllUsersNotInGroup(string groupName)
        {
            DataTable dt = null;
            DataSetSelectQuery selectQuery = null;
            try
            {
                selectQuery = new DataSetSelectQuery();
                selectQuery += string.Format(GET_USERS_NOT_IN_GROUP_QUERY, groupName);
                selectQuery.SetCachedSec(0);

                if (selectQuery.Execute("usersIngroupQuery", true) != null)
                {
                    dt = selectQuery.Table("usersIngroupQuery");
                }
                selectQuery.Finish();
                selectQuery = null;

                return dt;
            }
            catch (Exception ex)
            {
                log.Error("Error occurred while trying to execute GetAllUsersNotInGroup", ex);
            }

            return null;
        }

        public static DataTable GetAllMenus()
        {
            DataTable dt = null;
            DataSetSelectQuery selectQuery = null;
            try
            {
                selectQuery = new DataSetSelectQuery();
                selectQuery += GET_MENUS_QUERY;
                selectQuery.SetCachedSec(0);

                if (selectQuery.Execute("menusQuery", true) != null)
                {
                    dt = selectQuery.Table("menusQuery");
                }
                selectQuery.Finish();
                selectQuery = null;

                return dt;
            }
            catch (Exception ex)
            {
                log.Error("Error occurred while trying to execute GetAllMenus", ex);
            }

            return null;
        }

        public static DataTable GetMenusToAdd(int sourceUserID, int destinationUserID)
        {
            DataTable dt = null;
            DataSetSelectQuery selectQuery = null;
            try
            {
                selectQuery = new DataSetSelectQuery();
                selectQuery += string.Format(GET_MENUS_TO_ADD_QUREY, sourceUserID, destinationUserID);
                selectQuery.SetCachedSec(0);

                if (selectQuery.Execute("menusToAddQuery", true) != null)
                {
                    dt = selectQuery.Table("menusToAddQuery");
                }
                selectQuery.Finish();
                selectQuery = null;

                return dt;
            }
            catch (Exception ex)
            {
                log.Error("Error occurred while trying to execute GetMenusToAdd", ex);
            }

            return null;
        }

        public static bool AddMenuToUser(int userID, int menuID, bool shouldCheckStatus = true)
        {
            bool isSuccessful = false;
            int currentMenuStatus = -1;
            try
            {
                //Check if user already has a row that needs to be updated or insert a new row
                if (shouldCheckStatus)
                {
                    currentMenuStatus = GetUserMenuStatus(userID, menuID);
                }

                //Insert new row in admin_accounts_permissions
                if (currentMenuStatus == -1)
                {
                    InsertQuery insertQuery = null;
                    insertQuery = new InsertQuery("admin_accounts_permissions");
                    insertQuery += Parameter.NEW_PARAM("view_permit", "=", 1);
                    insertQuery += Parameter.NEW_PARAM("edit_permit", "=", 1);
                    insertQuery += Parameter.NEW_PARAM("new_permit", "=", 1);
                    insertQuery += Parameter.NEW_PARAM("remove_permit", "=", 1);
                    insertQuery += Parameter.NEW_PARAM("publish_permit", "=", 1);
                    insertQuery += Parameter.NEW_PARAM("status", "=", 1);
                    insertQuery += Parameter.NEW_PARAM("account_id", "=", userID);
                    insertQuery += Parameter.NEW_PARAM("menu_id", "=", menuID);

                    if (insertQuery.Execute())
                    {
                        isSuccessful = true;
                    }

                    insertQuery.Finish();
                    insertQuery = null;
                }
                //Update row status to 1
                else if (currentMenuStatus == 0)
                {
                    UpdateQuery updateQuery = null;
                    updateQuery = new UpdateQuery("admin_accounts_permissions");
                    updateQuery += Parameter.NEW_PARAM("status", "=", 1);
                    updateQuery += " where ";
                    updateQuery += Parameter.NEW_PARAM("account_id", "=", userID);
                    updateQuery += " and ";
                    updateQuery += Parameter.NEW_PARAM("menu_id", "=", menuID);
                    if (updateQuery.Execute())
                    {
                        isSuccessful = true;
                    }
                    updateQuery.Finish();
                    updateQuery = null;
                }
                // row status is already 1
                else
                {
                    isSuccessful = true;
                }
            }
            catch (Exception ex)
            {
                log.Error("Error occurred while trying to execute AddMenuToUser", ex);
            }

            return isSuccessful;
        }

        public static bool RemoveMenuFromUser(int userID, int menuID, bool shouldCheckStatus = true)
        {
            bool isSuccessful = false;
            int currentMenuStatus = 1;
            try
            {
                //Check if user already has a row that needs to be updated or insert a new row
                if (shouldCheckStatus)
                {
                    currentMenuStatus = GetUserMenuStatus(userID, menuID);
                }

                //update row status to 0
                if (currentMenuStatus == 1)
                {
                    UpdateQuery updateQuery = null;
                    updateQuery = new UpdateQuery("admin_accounts_permissions");
                    updateQuery += Parameter.NEW_PARAM("status", "=", 0);
                    updateQuery += " where ";
                    updateQuery += Parameter.NEW_PARAM("account_id", "=", userID);
                    updateQuery += " and ";
                    updateQuery += Parameter.NEW_PARAM("menu_id", "=", menuID);
                    if (updateQuery.Execute())
                    {
                        isSuccessful = true;
                    }
                    updateQuery.Finish();
                    updateQuery = null;
                }
                //row status is 0 or row doesn't exists
                else
                {
                    isSuccessful = true;
                }
            }
            catch (Exception ex)
            {
                log.Error("Error occurred while trying to execute RemoveMenuFromUser", ex);
            }

            return isSuccessful;
        }

        public static bool AddMenuToGroup(string groupName, int menuID)
        {
            bool isSuccessful = false;
            int countUsersAdded = 0;
            try
            {
                DataTable dt = GetAllUsersInGroup(groupName);
                if (dt != null)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        int userID = 0;
                        if (int.TryParse(dr["id"].ToString(), out userID))
                        {
                            if (AddMenuToUser(userID, menuID))
                            {
                                countUsersAdded++;
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                    if (countUsersAdded == dt.Rows.Count)
                    {
                        isSuccessful = true;
                    }
                    else
                    {
                        //TODO: write log? or remove menu from all users?
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Error occurred while trying to execute AddMenuToGroup", ex);
            }

            return isSuccessful;
        }

        public static bool RemoveMenuFromGroup(string groupName, int menuID)
        {
            bool isSuccessful = false;
            int countUsersRemoved = 0;
            try
            {
                DataTable dt = GetAllUsersInGroup(groupName);
                if (dt != null)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        int userID = 0;
                        if (int.TryParse(dr["id"].ToString(), out userID))
                        {
                            if (RemoveMenuFromUser(userID, menuID))
                            {
                                countUsersRemoved++;
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                    if (countUsersRemoved == dt.Rows.Count)
                    {
                        isSuccessful = true;
                    }
                    else
                    {
                        //TODO: write log? or add menu to all users?
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Error occurred while trying to execute RemoveMenuFromGroup", ex);
            }

            return isSuccessful;
        }

        public static bool AddUserToGroup(string groupName, int userID)
        {
            bool isSuccessful = false;
            UpdateQuery updateQuery = null;
            try
            {
                updateQuery = new UpdateQuery("accounts");
                updateQuery += Parameter.NEW_PARAM("moto_text", "=", groupName);
                updateQuery += " where ";
                updateQuery += Parameter.NEW_PARAM("id", "=", userID);
                if (updateQuery.Execute())
                {
                    isSuccessful = true;
                }
                updateQuery.Finish();
                updateQuery = null;
            }
            catch (Exception ex)
            {
                log.Error("Error occurred while trying to execute AddUserToGroup", ex);
            }
            return isSuccessful;
        }

        public static bool RemoveUserFromGroup(int userID)
        {
            bool isSuccessful = false;
            UpdateQuery updateQuery = null;
            try
            {
                updateQuery = new UpdateQuery("accounts");
                updateQuery += Parameter.NEW_PARAM("moto_text", "=", DBNull.Value);
                updateQuery += " where ";
                updateQuery += Parameter.NEW_PARAM("id", "=", userID);
                if (updateQuery.Execute())
                {
                    isSuccessful = true;
                }
                updateQuery.Finish();
                updateQuery = null;
            }
            catch (Exception ex)
            {
                log.Error("Error occurred while trying to execute RemoveUserFromGroup", ex);
            }
            return isSuccessful;
        }

        public static bool CopyUserPermissions(int sourceUserID, int destinationUserID)
        {
            bool isSuccessful = false;
            int countMenusAdded = 0;
            try
            {
                DataTable dt = GetMenusToAdd(sourceUserID, destinationUserID);
                if (dt != null)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        int menuID = 0;
                        if (int.TryParse(dr["menu_id"].ToString(), out menuID))
                        {
                            if (AddMenuToUser(destinationUserID, menuID))
                            {
                                countMenusAdded++;
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                    if (countMenusAdded == dt.Rows.Count)
                    {
                        isSuccessful = true;
                    }
                    else
                    {
                        //TODO: write log?
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Error occurred while trying to execute CopyUserPermissions", ex);
            }

            return isSuccessful;
        }

        public static int GetUserMenuStatus(int userID, int menuID)
        {
            int returnValue = -1;
            DataSetSelectQuery selectQuery = null;
            try
            {
                selectQuery = new DataSetSelectQuery();
                selectQuery += string.Format(GET_USER_SPECIFIC_MENU_STATUS_QUERY, userID, menuID);
                selectQuery.SetCachedSec(0);
                DataTable dt = selectQuery.Execute("userSpecificStatusQuery", true);

                if (dt != null && dt.Rows.Count > 0)
                {
                    returnValue = Utils.GetIntSafeVal(selectQuery.Table("userSpecificStatusQuery").Rows[0], "status");                    
                }
                selectQuery.Finish();
                selectQuery = null;
            }
            catch (Exception ex)
            {
                log.Error("Error occurred while trying to execute GetUserMenuStatus", ex);
            }

            return returnValue;
        }

        #endregion

        public static DataTable GetAllRegistry(int groupID)
        {
            try
            {

                StoredProcedure sp = new StoredProcedure("Get_AllRegistry");
                sp.SetConnectionKey("MAIN_CONNECTION_STRING");
                sp.AddParameter("@GroupID", groupID);
                DataSet ds = sp.ExecuteDataSet();
                if (ds != null)
                    return ds.Tables[0];
                return null;
            }
            catch 
            {
                return null;
            }
        }

        public static int GetTimeShiftedTVSettingsID(int groupID)
        {
            int idFromTable = 0;
            DataTable dt = null;
            try
            {
                ODBCWrapper.StoredProcedure spGetTimeShiftedTVSettingsID = new ODBCWrapper.StoredProcedure("GetTimeShiftedTvSettingsID");
                spGetTimeShiftedTVSettingsID.SetConnectionKey("MAIN_CONNECTION_STRING");
                spGetTimeShiftedTVSettingsID.AddParameter("@GroupID", groupID);

                dt = spGetTimeShiftedTVSettingsID.Execute();

                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    idFromTable = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0], "id");
                }
            }

            catch (Exception ex)
            {
                log.Error("Failed getting id from table on GetTimeShiftedTVSettingsID", ex);
            }

            return idFromTable;
        }

        public static DataSet GetSubscriptionPossibleChannels(int groupId, long subscriptionId)
        {
            StoredProcedure sp_GetSubscriptionPossibleChannels = new StoredProcedure("GetSubscriptionPossibleChannels");
            sp_GetSubscriptionPossibleChannels.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp_GetSubscriptionPossibleChannels.AddParameter("@GroupID", groupId);
            sp_GetSubscriptionPossibleChannels.AddParameter("@SubscriptionId", subscriptionId);

            return sp_GetSubscriptionPossibleChannels.ExecuteDataSet();
        }

        public static DataSet GetSubscriptionPossibleFileTypes(int groupId, long subscriptionId)
        {
            StoredProcedure sp_GetSubscriptionPossibleFileTypes = new StoredProcedure("GetSubscriptionPossibleFileTypes");
            sp_GetSubscriptionPossibleFileTypes.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp_GetSubscriptionPossibleFileTypes.AddParameter("@GroupID", groupId);
            sp_GetSubscriptionPossibleFileTypes.AddParameter("@SubscriptionId", subscriptionId);

            return sp_GetSubscriptionPossibleFileTypes.ExecuteDataSet();
        }

        public static DataSet GetCategoriesPossibleChannels(int groupId, long categoryId)
        {
            StoredProcedure sp_GetCategoriesPossibleChannels = new StoredProcedure("GetCategoriesPossibleChannels");
            sp_GetCategoriesPossibleChannels.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp_GetCategoriesPossibleChannels.AddParameter("@GroupID", groupId);
            sp_GetCategoriesPossibleChannels.AddParameter("@CategoryId", categoryId);

            return sp_GetCategoriesPossibleChannels.ExecuteDataSet();
        }

        public static bool InsertChannelToCategory(int groupId, int updaterId, long categoryId, long channelId)
        {
            ODBCWrapper.StoredProcedure sp_InsertChannelToCategory = new ODBCWrapper.StoredProcedure("InsertChannelToCategory");
            sp_InsertChannelToCategory.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp_InsertChannelToCategory.AddParameter("@GroupID", groupId);
            sp_InsertChannelToCategory.AddParameter("@UpdaterId", updaterId);
            sp_InsertChannelToCategory.AddParameter("@CategoryId", categoryId);
            sp_InsertChannelToCategory.AddParameter("@ChannelId", channelId);

            int updatedRows = sp_InsertChannelToCategory.ExecuteReturnValue<int>();

            return updatedRows == 1;
        }

        public static bool RemoveChannelFromCategory(int updaterId, long categoryId, long channelId)
        {
            ODBCWrapper.StoredProcedure sp_RemoveChannelFromCategory = new ODBCWrapper.StoredProcedure("RemoveChannelFromCategory");
            sp_RemoveChannelFromCategory.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp_RemoveChannelFromCategory.AddParameter("@UpdaterId", updaterId);
            sp_RemoveChannelFromCategory.AddParameter("@CategoryId", categoryId);
            sp_RemoveChannelFromCategory.AddParameter("@ChannelId", channelId);

            int updatedRows = sp_RemoveChannelFromCategory.ExecuteReturnValue<int>();

            return updatedRows == 1;
        }

        public static bool UpdateChannelOrderNumInCategory(int updaterId, long categoryId, long channelId, int orderNum)
        {
            ODBCWrapper.StoredProcedure sp_UpdateChannelOrderNumInCategory = new ODBCWrapper.StoredProcedure("UpdateChannelOrderNumInCategory");
            sp_UpdateChannelOrderNumInCategory.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp_UpdateChannelOrderNumInCategory.AddParameter("@UpdaterId", updaterId);
            sp_UpdateChannelOrderNumInCategory.AddParameter("@CategoryId", categoryId);
            sp_UpdateChannelOrderNumInCategory.AddParameter("@ChannelId", channelId);
            sp_UpdateChannelOrderNumInCategory.AddParameter("@OrderNum", orderNum);

            int updatedRows = sp_UpdateChannelOrderNumInCategory.ExecuteReturnValue<int>();

            return updatedRows == 1;
        }

        public static bool InsertOrUpdateGroupLocaleExtraLanguage(int group_locale_id, int groupLocaleExtraLanguageId, int status, int updaterId)
        {
            StoredProcedure sp = new StoredProcedure("InsertOrUpdateGroupLocaleExtraLanguage");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@GroupLocaleConfigurationId", group_locale_id);
            sp.AddParameter("@LanguageId", groupLocaleExtraLanguageId);
            sp.AddParameter("@Status", status);
            sp.AddParameter("@UpdaterId", updaterId);

            return  sp.ExecuteReturnValue<int>() > 0;
        }

    }    
}
