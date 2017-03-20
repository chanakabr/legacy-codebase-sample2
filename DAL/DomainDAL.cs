using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tvinci.Core.DAL;
using System.Data;
using ApiObjects;
using ODBCWrapper;
using ApiObjects.DRM;
using KLogMonitor;
using System.Reflection;
using Newtonsoft.Json;

namespace DAL
{
    public class DomainDal : BaseDal
    {   
        #region Private Constants
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private const int RETRY_LIMIT = 5;

        private const string SP_GET_USER_EXISTS_IN_DOMAIN = "Get_UserExistsInDomain";
        private const string SP_GET_USER_IN_DOMAIN = "Get_UserInDomain";
        private const string SP_GET_USERS_IN_DOMAIN = "Get_UsersInDomain";
        private const string SP_GET_USERS_IN_DOMAIN_INCLUDE_DELETED = "Get_UserInDomainInIncludeDeleted";
        private const string SP_GET_DOMAIN_SETTINGS = "sp_GetDomainSettings";
        private const string SP_GET_DEVICE_FAMILIES_LIMITS = "sp_GetDeviceFamiliesLimits";
        private const string SP_GET_DOMAIN_IDS_BY_EMAIL = "sp_GetDomainIDsByEmail";
        private const string SP_GET_DOMAIN_IDS_BY_OPERATOR_COGUID = "sp_GetDomainIDsByOperatorCoGuid";
        private const string SP_GET_DEVICE_DOMAIN_DATA = "Get_DeviceDomainData";
        private const string SP_GET_DOMAIN_COGUID = "Get_DomainCoGuid";
        private const string SP_GET_DOMAIN_COGUID_BY_SITEGUID = "Get_DomainCoGuidBySiteGuid";
        private const string SP_GET_DEVICE_ID_AND_BRAND_BY_PIN = "Get_DeviceIDAndBrandByPIN";


        private const string SP_INSERT_USER_TO_DOMAIN = "sp_InsertUserToDomain";
        private const string SP_INSERT_DEVICE_TO_DOMAIN = "sp_InsertDeviceToDomain";

        private const string SP_UPDATE_SET_USER_STATUS_IN_DOMAIN = "Update_SetUserStatusInDomain";
        private const string SP_UPDATE_SET_DEVICE_STATUS_IN_DOMAIN = "Update_SetDeviceStatusInDomain";
        private const string SP_UPDATE_DOMAIN_DATA = "Update_DomainData";
        private const string SP_UPDATE_SWITCH_DOMAIN_MASTER = "Update_SwitchDomainMaster";

        private const string SP_REMOVE_DOMAIN = "sp_RemoveDomain";
        private const string SP_RESET_DOMAIN_FREQUENCY = "sp_ResetDomainFrequency";
        private const string SP_UPDATE_DOMAIN_CoGuid = "Update_DomainCoGuid";
        #endregion


        private static void HandleException(Exception ex)
        {

        }


        public static bool InitDeviceInDb(int nDeviceID, int nDomainID,
                                    ref int nGroupID, ref string sDbDeviceUDID, ref int nDbDeviceBrandID, ref string sDbDeviceName, ref int nDbDeviceFamilyID, ref string sDbPin, ref DateTime dtDbActivationDate, ref string sDbState)
        {
            bool res = false;

            try
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey("USERS_CONNECTION_STRING");

                // Search for device in devices table
                selectQuery += "select device_id, device_brand_id, Name, group_id, device_family_id, pin, is_active from devices WITH (nolock) where status=1";
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nDeviceID);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
                if (selectQuery.Execute("query", true) != null)
                {
                    int count = selectQuery.Table("query").DefaultView.Count;
                    if (count > 0) // Device found
                    {
                        sDbDeviceUDID = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "device_id", 0);
                        nDbDeviceBrandID = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "device_brand_id", 0);
                        sDbDeviceName = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "Name", 0);
                        //nGroupID = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "group_id", 0);
                        nDbDeviceFamilyID = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "device_family_id", 0);
                        sDbPin = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "pin", 0);

                        int nActive = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "is_active", 0);
                        if (nActive == 0)
                        {
                            sDbState = "Pending";
                        }
                        else if (nActive == 1)
                        {
                            ODBCWrapper.DataSetSelectQuery selectQuery1 = new ODBCWrapper.DataSetSelectQuery();
                            selectQuery1 += " select last_activation_date, is_active from domains_devices WITH (nolock) where status=1 and";
                            selectQuery1 += ODBCWrapper.Parameter.NEW_PARAM("device_id", "=", nDeviceID);
                            selectQuery1 += " and ";
                            selectQuery1 += ODBCWrapper.Parameter.NEW_PARAM("domain_id", "=", nDomainID);
                            selectQuery1 += " and ";
                            selectQuery1 += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
                            if (selectQuery1.Execute("query", true) != null)
                            {
                                count = selectQuery1.Table("query").DefaultView.Count;
                                if (count > 0)
                                {
                                    dtDbActivationDate = ODBCWrapper.Utils.GetDateSafeVal(selectQuery1, "last_activation_date", 0);

                                    nActive = ODBCWrapper.Utils.GetIntSafeVal(selectQuery1, "is_active", 0);
                                    if (nActive == 1)
                                    {
                                        sDbState = "Activated";
                                    }
                                    else
                                    {
                                        sDbState = "UnActivated";
                                    }
                                }
                                else // Device is not registered
                                {
                                    sDbState = "UnKnown";
                                }
                            }
                            else // Error
                            {
                                sDbState = "Error";
                            }

                            selectQuery1.Finish();
                            selectQuery1 = null;
                        }
                        else
                        {
                            sDbState = "UnKnown";
                        }

                    }
                    else // Device not found
                    {

                        sDbState = "NotExists";
                    }
                }
                else
                {
                    sDbState = "Error";
                }

                selectQuery.Finish();
                selectQuery = null;

                res = true;
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return res;
        }


        public static bool GetDeviceIdAndBrandByPin(string sPIN, int nGroupID, ref string sUDID, ref int nBrandID)
        {
            bool res = false;

            ODBCWrapper.StoredProcedure spGetDeviceIDandBrandByPIN = new ODBCWrapper.StoredProcedure(SP_GET_DEVICE_ID_AND_BRAND_BY_PIN);
            spGetDeviceIDandBrandByPIN.SetConnectionKey("USERS_CONNECTION_STRING");
            spGetDeviceIDandBrandByPIN.AddParameter("@groupID", nGroupID);
            spGetDeviceIDandBrandByPIN.AddParameter("@PIN", sPIN);

            DataSet ds = spGetDeviceIDandBrandByPIN.ExecuteDataSet();

            if (ds != null && ds.Tables[0].DefaultView.Count > 0)
            {
                int nCount = ds.Tables[0].DefaultView.Count;
                if (nCount > 0)
                {
                    DataRow dr = ds.Tables[0].DefaultView[0].Row;
                    sUDID = ODBCWrapper.Utils.GetSafeStr(dr["device_id"]);
                    nBrandID = ODBCWrapper.Utils.GetIntSafeVal(dr, "device_brand_id");
                }
            }
            res = true;
            return res;
        }

        public static int InsertDeviceToDomain(int nDeviceID, int nDomainID, int nGroupID, int nIsActive, int nStatus, string sActivationToken = "")
        {
            ODBCWrapper.StoredProcedure spInsertDeviceToDomain = new ODBCWrapper.StoredProcedure(SP_INSERT_DEVICE_TO_DOMAIN);
            spInsertDeviceToDomain.SetConnectionKey("USERS_CONNECTION_STRING");

            spInsertDeviceToDomain.AddParameter("@deviceID", nDeviceID);
            spInsertDeviceToDomain.AddParameter("@domainID", nDomainID);
            spInsertDeviceToDomain.AddParameter("@groupID", nGroupID);
            spInsertDeviceToDomain.AddParameter("@status", nStatus);
            spInsertDeviceToDomain.AddParameter("@isActive", nIsActive);
            spInsertDeviceToDomain.AddParameter("@activationToken", sActivationToken);

            return spInsertDeviceToDomain.ExecuteReturnValue<int>();
        }

        public static bool UpdateDomainsDevicesStatus(int nDomainsDevicesID, int nIsActive, int nStatus)
        {
            StoredProcedure sp = new StoredProcedure("Update_DomainsDevicesStatus");
            sp.SetConnectionKey("USERS_CONNECTION_STRING");
            sp.AddParameter("@DomainsDevicesID", nDomainsDevicesID);
            sp.AddParameter("@IsActive", nIsActive);
            sp.AddParameter("@Status", nStatus);
            sp.AddParameter("@UpdateDate", DateTime.UtcNow);

            return sp.ExecuteReturnValue<bool>();
        }


        public static int DoesDeviceExistInDomain(int nDomainID, int nGroupID, string deviceUdid, ref int isActive, ref int nDeviceID)
        {
            return Get_IsDeviceExistInDomain(nDomainID, nGroupID, deviceUdid, ref isActive, ref nDeviceID);
        }

        public static int GetDeviceDomainData(int nGroupID, string sDeviceUdid, ref int nDeviceID, ref int nIsActive, ref int nStatus, ref int nDbDomainDeviceID)
        {
            int nDomainID = 0;

            ODBCWrapper.StoredProcedure spGetDeviceDomainData = new ODBCWrapper.StoredProcedure(SP_GET_DEVICE_DOMAIN_DATA);
            spGetDeviceDomainData.SetConnectionKey("USERS_CONNECTION_STRING");
            spGetDeviceDomainData.AddParameter("@groupID", nGroupID);
            spGetDeviceDomainData.AddParameter("@deviceID", sDeviceUdid);
            DataSet ds = spGetDeviceDomainData.ExecuteDataSet();

            if (ds != null && ds.Tables[0].DefaultView.Count > 0)
            {
                int nCount = ds.Tables[0].DefaultView.Count;
                if (nCount > 0)
                {
                    nDbDomainDeviceID = int.Parse(ds.Tables[0].DefaultView[0].Row["id"].ToString());
                    nDomainID = int.Parse(ds.Tables[0].DefaultView[0].Row["domain_id"].ToString());
                    nIsActive = int.Parse(ds.Tables[0].DefaultView[0].Row["is_active"].ToString());
                    nStatus = int.Parse(ds.Tables[0].DefaultView[0].Row["status"].ToString());
                    nDeviceID = ODBCWrapper.Utils.GetIntSafeVal(ds.Tables[0].DefaultView[0].Row, "device_id");
                }
            }
            return nDomainID;
        }



        public static int GetDomainOfDevice(int nGroupID, string sDeviceUdid, ref int nDeviceID, ref int nIsActive, ref int nStatus)
        {
            int nDomainID = 0;

            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetConnectionKey("USERS_CONNECTION_STRING");
            selectQuery += "select dd.id, dd.is_active, dd.status, dd.device_id, dd.domain_id from domains_devices dd WITH (nolock), devices d WITH (nolock) where ";

            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("dd.GROUP_ID", "=", nGroupID);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("d.device_id", "=", sDeviceUdid);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("d.status", "<>", 2);
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("d.group_id", "=", nGroupID);
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("dd.STATUS", "<>", 2);
            selectQuery += "and";
            selectQuery += "dd.DEVICE_ID=d.id";
            if (selectQuery.Execute("query", true) != null)
            {
                int nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    nDomainID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["domain_id"].ToString());

                    nIsActive = int.Parse(selectQuery.Table("query").DefaultView[0].Row["is_active"].ToString());
                    nStatus = int.Parse(selectQuery.Table("query").DefaultView[0].Row["status"].ToString());
                    nDeviceID = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "device_id", 0);
                }
            }

            selectQuery.Finish();
            selectQuery = null;

            return nDomainID;
        }

        public static bool UpdateDomainsDevicesIsActive(int nDomainDeviceID, int enableInt, bool bIsEnable)
        {
            return Update_DomainsDevicesIsActive(nDomainDeviceID, enableInt, bIsEnable);
        }

        public static bool Update_DomainsDevicesIsActive(int nDomainsDevicesID, int nEnableInt, bool bIsUpdateLastActivationDate)
        {
            StoredProcedure sp = new StoredProcedure("Update_DomainsDevicesIsActive");
            sp.SetConnectionKey("USERS_CONNECTION_STRING");
            sp.AddParameter("@DomainsDevicesID", nDomainsDevicesID);
            sp.AddParameter("@IsActive", nEnableInt);
            sp.AddParameter("@UpdateDate", DateTime.UtcNow);
            sp.AddParameter("@IsUpdateLastActivationDate", bIsUpdateLastActivationDate);

            return sp.ExecuteReturnValue<bool>();
        }

        /// <summary>
        /// Check if User is assign to Domain
        /// </summary>
        /// <param name="nGroupID"></param>
        /// <param name="nDomainID"></param>
        /// <param name="nUserGuid"></param>
        /// <returns>True if User is assign to Domain</returns>
        public static int DoesUserExistInDomain(int nGroupID, int nDomainID, int nUserGuid, bool onlyActive = true)
        {
            int nUserDomainID = 0;

            ODBCWrapper.StoredProcedure spGetUserExistsInDomain = new ODBCWrapper.StoredProcedure(SP_GET_USER_EXISTS_IN_DOMAIN);
            spGetUserExistsInDomain.SetConnectionKey("USERS_CONNECTION_STRING");

            spGetUserExistsInDomain.AddParameter("@domainID", nDomainID);
            spGetUserExistsInDomain.AddParameter("@groupID", nGroupID);
            spGetUserExistsInDomain.AddParameter("@userID", nUserGuid);

            int status = onlyActive ? 1 : 0;
            if (onlyActive)
            {
                spGetUserExistsInDomain.AddParameter("@isActive", 1);
            }

            object res = spGetUserExistsInDomain.ExecuteReturnValue();

            if (res == null)
            {
                return nUserDomainID;
            }

            int tmp;
            nUserDomainID = int.TryParse(res.ToString(), out tmp) ? tmp : 0;

            return nUserDomainID;
        }

        /// <summary>
        /// Get User In Domain
        /// </summary>
        /// <param name="nGroupID"></param>
        /// <param name="nDomainID"></param>
        /// <param name="nUserGuid"></param>
        /// <param name="onlyActive"></param>
        /// <returns>Table with User Data</returns>
        public static DataTable GetUserInDomain(int nGroupID, int nDomainID, int nUserGuid, bool onlyActive = true)
        {
            ODBCWrapper.StoredProcedure spGetUserInDomain = new ODBCWrapper.StoredProcedure(SP_GET_USER_IN_DOMAIN);
            spGetUserInDomain.SetConnectionKey("USERS_CONNECTION_STRING");

            spGetUserInDomain.AddParameter("@domainID", nDomainID);
            spGetUserInDomain.AddParameter("@groupID", nGroupID);
            spGetUserInDomain.AddParameter("@userID", nUserGuid);

            DataSet ds = spGetUserInDomain.ExecuteDataSet();
            if (ds != null)
                return ds.Tables[0];
            return null;
        }

        public static int InsertUserToDomain(int nUserGuid, int nDomainID, int nGroupID, int userType, int status, int isActive, int nMasterUserGuid, string sActivationToken = "")
        {
            ODBCWrapper.StoredProcedure spInsertUserToDomain = new ODBCWrapper.StoredProcedure(SP_INSERT_USER_TO_DOMAIN);
            spInsertUserToDomain.SetConnectionKey("USERS_CONNECTION_STRING");

            spInsertUserToDomain.AddParameter("@userID", nUserGuid);
            spInsertUserToDomain.AddParameter("@domainID", nDomainID);
            spInsertUserToDomain.AddParameter("@groupID", nGroupID);
            spInsertUserToDomain.AddParameter("@userType", userType);
            spInsertUserToDomain.AddParameter("@status", status);
            spInsertUserToDomain.AddParameter("@isActive", isActive);
            spInsertUserToDomain.AddParameter("@masterUserID", nMasterUserGuid);
            spInsertUserToDomain.AddParameter("@activationToken", sActivationToken);

            int retVal = spInsertUserToDomain.ExecuteReturnValue<int>();

            return retVal;
        }

        /// <summary>
        /// Returns user-type duples in a domain. The Master user(s) is always returned first
        /// </summary>
        /// <param name="nDomainID"></param>
        /// <param name="nGroupID"></param>
        /// <param name="status"></param>
        /// <param name="isActive"></param>
        /// <returns></returns>
        public static Dictionary<int, int> GetUsersInDomain(int nDomainID, int nGroupID, int status, int isActive)
        {
            Dictionary<int, int> dTypedUsers = new Dictionary<int, int>();

            ODBCWrapper.StoredProcedure spGetUsersInDomain = new ODBCWrapper.StoredProcedure(SP_GET_USERS_IN_DOMAIN);
            spGetUsersInDomain.SetConnectionKey("USERS_CONNECTION_STRING");

            spGetUsersInDomain.AddParameter("@domainID", nDomainID);
            spGetUsersInDomain.AddParameter("@groupID", nGroupID);
            spGetUsersInDomain.AddParameter("@status", status);
            spGetUsersInDomain.AddParameter("@isActive", isActive);

            DataSet ds = spGetUsersInDomain.ExecuteDataSet();

            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
            {
                int nCount = ds.Tables[0].DefaultView.Count;

                for (int i = 0; i < nCount; i++)
                {
                    int nUserId = int.Parse(ds.Tables[0].DefaultView[i].Row["user_id"].ToString());
                    int nUserType = ODBCWrapper.Utils.GetIntSafeVal(ds.Tables[0].DefaultView[i], "is_master");

                    dTypedUsers[nUserId] = nUserType;
                }
            }

            return dTypedUsers;
        }

        /// <summary>
        /// Returns user-type duples in a domain. The Master user(s) is always returned first
        /// </summary>
        /// <param name="nDomainID"></param>
        /// <param name="nGroupID"></param>
        /// <param name="status"></param>
        /// <param name="isActive"></param>
        /// <returns></returns>
        public static Dictionary<int, int> GetUsersInDomainIncludeDeleted(int nDomainID, int nGroupID)
        {
            Dictionary<int, int> dTypedUsers = new Dictionary<int, int>();

            ODBCWrapper.StoredProcedure spGetUsersInDomain = new ODBCWrapper.StoredProcedure(SP_GET_USERS_IN_DOMAIN_INCLUDE_DELETED);
            spGetUsersInDomain.SetConnectionKey("USERS_CONNECTION_STRING");

            spGetUsersInDomain.AddParameter("@domainID", nDomainID);
            spGetUsersInDomain.AddParameter("@groupID", nGroupID);

            DataSet ds = spGetUsersInDomain.ExecuteDataSet();

            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
            {
                int nCount = ds.Tables[0].DefaultView.Count;

                for (int i = 0; i < nCount; i++)
                {
                    int nUserId = int.Parse(ds.Tables[0].DefaultView[i].Row["user_id"].ToString());
                    int nUserType = ODBCWrapper.Utils.GetIntSafeVal(ds.Tables[0].DefaultView[i], "is_master");

                    dTypedUsers[nUserId] = nUserType;
                }
            }

            return dTypedUsers;
        }

        public static List<int> GetOperatorUsers(int nOperatorID, List<int> nUserIDs)
        {
            List<int> operatorUsersList = new List<int>();

            ODBCWrapper.StoredProcedure spGetUsersToOperator = new ODBCWrapper.StoredProcedure("Get_UsersToOperator");
            spGetUsersToOperator.SetConnectionKey("USERS_CONNECTION_STRING");
            spGetUsersToOperator.AddParameter("@operatorID", nOperatorID);
            spGetUsersToOperator.AddIDListParameter("@userIDs", nUserIDs, "Id");
            DataSet ds = spGetUsersToOperator.ExecuteDataSet();
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0] != null)
            {
                DataTable dtOperatorUsers = ds.Tables[0];
                foreach (DataRow rowOperatorUser in dtOperatorUsers.Rows)
                {
                    int nOperatorUserID = ODBCWrapper.Utils.GetIntSafeVal(rowOperatorUser["user_id"]);
                    operatorUsersList.Add(nOperatorUserID);
                }
            }

            return operatorUsersList;
        }

        public static int SetUserStatusInDomain(int nUserGuid, int nDomainID, int nGroupID, int? nUserDomainID = null, int nStatus = 1, int nIsActive = 1)
        {
            ODBCWrapper.StoredProcedure spUpdateSetUserStatusInDomain = new ODBCWrapper.StoredProcedure(SP_UPDATE_SET_USER_STATUS_IN_DOMAIN);
            spUpdateSetUserStatusInDomain.SetConnectionKey("USERS_CONNECTION_STRING");

            spUpdateSetUserStatusInDomain.AddParameter("@userID", nUserGuid);
            spUpdateSetUserStatusInDomain.AddParameter("@domainID", nDomainID);
            spUpdateSetUserStatusInDomain.AddParameter("@groupID", nGroupID);
            spUpdateSetUserStatusInDomain.AddParameter("@userDomainID", nUserDomainID);
            spUpdateSetUserStatusInDomain.AddParameter("@status", nStatus);
            spUpdateSetUserStatusInDomain.AddParameter("@isActive", nIsActive);

            DataTable dtResult = spUpdateSetUserStatusInDomain.Execute();

            if (dtResult == null)
            {
                return 1;
            }

            return dtResult.DefaultView.Count;

        }

        public static int SetDeviceStatusInDomain(int nDeviceID, int nDomainID, int nGroupID, int? nDeviceDomainID = null, int nStatus = 1, int nIsActive = 1)
        {
            ODBCWrapper.StoredProcedure spUpdateSetDeviceStatusInDomain = new ODBCWrapper.StoredProcedure(SP_UPDATE_SET_DEVICE_STATUS_IN_DOMAIN);
            spUpdateSetDeviceStatusInDomain.SetConnectionKey("USERS_CONNECTION_STRING");

            spUpdateSetDeviceStatusInDomain.AddParameter("@DeviceID", nDeviceID);
            spUpdateSetDeviceStatusInDomain.AddParameter("@domainID", nDomainID);
            spUpdateSetDeviceStatusInDomain.AddParameter("@groupID", nGroupID);
            spUpdateSetDeviceStatusInDomain.AddParameter("@DeviceDomainID", nDeviceDomainID);
            spUpdateSetDeviceStatusInDomain.AddParameter("@status", nStatus);
            spUpdateSetDeviceStatusInDomain.AddParameter("@isActive", nIsActive);

            DataTable dtResult = spUpdateSetDeviceStatusInDomain.Execute();

            if (dtResult == null)
            {
                return 1;
            }

            return dtResult.DefaultView.Count;

        }

        public static int GetDomainDefaultLimitsID(int nGroupID, ref int defaultDeviceLimit, ref int defaultUserLimit, ref int defaultConcurrentLimit)
        {
            int retVal = 0;

            int defaultGroupConcurrentLimit = 0;
            retVal = GetDomainDefaultLimitsID(nGroupID, ref defaultDeviceLimit, ref defaultUserLimit, ref defaultConcurrentLimit, ref defaultGroupConcurrentLimit);

            return retVal;
        }

        public static int GetDomainDefaultLimitsID(int nGroupID, ref int defaultDeviceLimit, ref int defaultUserLimit, ref int defaultConcurrentLimit, ref int defaultGroupConcurrentLimit)
        {
            int nDeviceFreqLimitID = 0;

            return GetDomainDefaultLimitsID(nGroupID, ref defaultDeviceLimit, ref defaultUserLimit, ref defaultConcurrentLimit, ref defaultGroupConcurrentLimit, ref nDeviceFreqLimitID);
        }
        public static int GetDomainDefaultLimitsID(int nGroupID, ref int defaultDeviceLimit, ref int defaultUserLimit,
            ref int defaultConcurrentLimit, ref int defaultGroupConcurrentLimit, ref int defaultDeviceFreqLimit)
        {
            long npvrQuotaInMins = 0;
            return GetDomainDefaultLimitsID(nGroupID, ref defaultDeviceLimit, ref defaultUserLimit, ref defaultConcurrentLimit,
                ref defaultGroupConcurrentLimit, ref defaultDeviceFreqLimit, ref npvrQuotaInMins);
        }

        public static int GetDomainDefaultLimitsID(int nGroupID, ref int defaultDeviceLimit, ref int defaultUserLimit, 
            ref int defaultConcurrentLimit, ref int defaultGroupConcurrentLimit, ref int defaultDeviceFreqLimit,
            ref long npvrQuotaInMins)
        {
            int retVal = 0;


            ODBCWrapper.StoredProcedure spGetGroupLimits = new ODBCWrapper.StoredProcedure("Get_GroupLimits");
            spGetGroupLimits.SetConnectionKey("MAIN_CONNECTION_STRING");
            spGetGroupLimits.AddParameter("@GroupID", nGroupID);
            DataSet ds = spGetGroupLimits.ExecuteDataSet();

            if (ds != null && ds.Tables != null && ds.Tables.Count > 0 && ds.Tables[0].DefaultView.Count > 0)
            {
                int nCount = ds.Tables[0].DefaultView.Count;
                if (nCount > 0)
                {
                    DataRow dr = ds.Tables[0].DefaultView[0].Row;

                    retVal = ODBCWrapper.Utils.GetIntSafeVal(dr, "LIMIT_ID");
                    defaultDeviceLimit = ODBCWrapper.Utils.GetIntSafeVal(dr, "DEVICE_MAX_LIMIT");
                    defaultUserLimit = ODBCWrapper.Utils.GetIntSafeVal(dr, "USER_MAX_LIMIT");
                    defaultConcurrentLimit = ODBCWrapper.Utils.GetIntSafeVal(dr, "CONCURRENT_MAX_LIMIT");
                    defaultGroupConcurrentLimit = ODBCWrapper.Utils.GetIntSafeVal(dr, "GROUP_CONCURRENT_MAX_LIMIT");
                    defaultDeviceFreqLimit = ODBCWrapper.Utils.GetIntSafeVal(dr, "freq_period_id");
                    npvrQuotaInMins = ODBCWrapper.Utils.GetIntSafeVal(dr, "npvr_quota_in_seconds");
                }
            }


            return retVal;
        }


        public static bool InsertNewDomain(string sName, string sDescription, int nGroupID, DateTime dDateTime, int nDomainLimitID, ref int nDbDomainID, string sCoGuid = null, int? nOperatorID = null)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("InsertNewDomain");
            sp.SetConnectionKey("USERS_CONNECTION_STRING");

            sp.AddParameter("@GroupID", nGroupID);
            sp.AddParameter("@Name", sName);
            sp.AddParameter("@Description", sDescription);
            sp.AddParameter("@DomainLimitID", nDomainLimitID);
            sp.AddParameter("@DateTime", dDateTime);
            sp.AddParameter("@CoGuid", sCoGuid);
            sp.AddParameter("@OperatorID", nOperatorID);

            DataTable dt = sp.Execute();

            if (dt == null || dt.Rows == null || dt.Rows.Count == 0)
            {
                return false;
            }
            else
            {
                nDbDomainID = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0], "id");
                if (nDbDomainID == 0)
                {
                    return false;
                }
            }

            return true;
        }

        public static bool InsertNewDomain(string sName, string sDescription, int nGroupID, DateTime dDateTime, int nDomainLimitID, string sCoGuid = null, int? nOperatorID = null)
        {
            bool bInserRes = false;



            ODBCWrapper.InsertQuery insertQuery = null;
            try
            {
                //Insert New Domain to DB
                insertQuery = new ODBCWrapper.InsertQuery("domains");
                insertQuery.SetConnectionKey("USERS_CONNECTION_STRING");

                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("Name", "=", sName);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("Description", "=", sDescription);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("Max_Limit", "=", nDomainLimitID);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATER_ID", "=", 43);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("Status", "=", 1);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("Is_Active", "=", 1);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("create_date", "=", dDateTime);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("update_date", "=", dDateTime);

                if (!string.IsNullOrEmpty(sCoGuid))
                {
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CoGuid", "=", sCoGuid);
                }

                if (nOperatorID.HasValue)
                {
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("operator_id", "=", nOperatorID.Value);
                }

                bInserRes = insertQuery.Execute();

            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
            finally
            {
                if (insertQuery != null)
                {
                    insertQuery.Finish();
                    insertQuery = null;
                }
            }

            return bInserRes;
        }

        public static bool GetDomainDbObject(int groupID, DateTime dateTime, ref string name, ref string description, int domainID, ref int isActive, ref int status, ref string coGuid, ref int regionId)
        {
            bool res = false;
            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_DomainDataByID");
                sp.SetConnectionKey("USERS_CONNECTION_STRING");
                sp.AddParameter("@GroupId", groupID);
                sp.AddParameter("@Id", domainID);

                DataTable dt = sp.Execute();
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {                    
                    DataRow dr = dt.Rows[0];
                    name = ODBCWrapper.Utils.GetSafeStr(dr, "name");
                    description = ODBCWrapper.Utils.GetSafeStr(dr, "description");
                    domainID = ODBCWrapper.Utils.GetIntSafeVal(dr, "id");
                    isActive = ODBCWrapper.Utils.GetIntSafeVal(dr, "is_active");
                    status = ODBCWrapper.Utils.GetIntSafeVal(dr, "status");
                    coGuid = ODBCWrapper.Utils.GetSafeStr(dr, "CoGuid");
                    regionId = ODBCWrapper.Utils.GetIntSafeVal(dr, "Region_ID");
                    res = true;
                }

            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return res;
        }

        public static bool GetDomainDbObject(int nGroupID, DateTime dDateTime, ref string sName, ref string sDbDescription, 
            ref int nDbDomainID, ref int nDbIsActive, ref int nDbStatus, ref string sCoGuid, ref int regionId)
        {
            bool res = false;
            ODBCWrapper.DataSetSelectQuery selectQuery = null;
            try
            {
                selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey("USERS_CONNECTION_STRING");

                selectQuery += "SELECT name,description,id,is_active,status,CoGuid,Region_ID FROM DOMAINS WITH (NOLOCK) WHERE ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("Name", "=", sName);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("create_date", "=", dDateTime);
                if (selectQuery.Execute("query", true) != null)
                {
                    int nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        sName = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "name", 0);
                        sDbDescription = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "description", 0);
                        nDbDomainID = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "id", 0);
                        nDbIsActive = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "is_active", 0);
                        nDbStatus = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "status", 0);
                        sCoGuid = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "CoGuid", 0);
                        regionId = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "Region_ID", 0);

                        res = true;
                    }
                }



            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
            finally
            {
                if (selectQuery != null)
                {
                    selectQuery.Finish();
                    selectQuery = null;
                }
            }

            return res;
        }

        public static bool ResetDomain(int nDomainID, int nGroupID, int nFrequencyType = 0)
        {
            ODBCWrapper.StoredProcedure spResetDomainFrequency = new ODBCWrapper.StoredProcedure(SP_RESET_DOMAIN_FREQUENCY);
            spResetDomainFrequency.SetConnectionKey("USERS_CONNECTION_STRING");

            spResetDomainFrequency.AddParameter("@domainID", nDomainID);
            spResetDomainFrequency.AddParameter("@groupID", nGroupID);
            spResetDomainFrequency.AddParameter("@status", 2);
            spResetDomainFrequency.AddParameter("@isActive", 2);
            spResetDomainFrequency.AddParameter("@freqType", nFrequencyType);

            DataTable dtResult = spResetDomainFrequency.Execute();

            if (dtResult == null)
            {
                return true;
            }

            return dtResult.DefaultView.Count > 0;
        }

        public static bool SetDomainFlag(int domainId, int val, DateTime dt, int deviceFlag = 1)
        {
            bool res = false;
            ODBCWrapper.UpdateQuery updateQuery = null;

            try
            {
                updateQuery = new ODBCWrapper.UpdateQuery("domains");
                updateQuery.SetConnectionKey("USERS_CONNECTION_STRING");
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("Frequency_flag", "=", val);

                if (deviceFlag == 1)
                {
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("Frequency_last_action", "=", dt);
                }
                else
                {
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("User_Frequency_last_action", "=", dt);
                }

                updateQuery += " WHERE ";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", domainId);

                res = updateQuery.Execute();
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
            finally
            {
                if (updateQuery != null)
                {
                    updateQuery.Finish();
                    updateQuery = null;
                }
            }

            return res;
        }

        public static List<int> GetDeviceDomains(int deviceID, int groupID)
        {
            List<int> domainIDs = null;

            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("GetDeviceDomains");
                sp.SetConnectionKey("USERS_CONNECTION_STRING");
                sp.AddParameter("@GroupID", groupID);
                sp.AddParameter("@DeviceID", deviceID);
                DataTable dtResult = sp.Execute();
                if (dtResult != null && dtResult.Rows != null && dtResult.Rows.Count > 0)
                {
                    foreach (DataRow dr in dtResult.Rows)
                    {
                         int domainID = ODBCWrapper.Utils.GetIntSafeVal(dr, "domain_id");
                         if (domainIDs == null)
                         {
                             domainIDs = new List<int>();
                         }
                         domainIDs.Add(domainID);
                    }
                }        
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return domainIDs;
        }

        public static List<int> GetDevicesInDomain(int m_nGroupID, int m_nDomainID)
        {
            List<int> devicesIDs = new List<int>();

            try
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey("USERS_CONNECTION_STRING");

                selectQuery += "select device_id from domains_devices WITH (nolock) where status = 1 and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("DOMAIN_ID", "=", m_nDomainID);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", m_nGroupID);

                if (selectQuery.Execute("query", true) != null)
                {
                    int nCount = selectQuery.Table("query").DefaultView.Count;

                    for (int i = 0; i < nCount; i++)
                    {
                        int nDeviceId = int.Parse(selectQuery.Table("query").DefaultView[i].Row["device_id"].ToString());

                        devicesIDs.Add(nDeviceId);
                    }
                }

                selectQuery.Finish();
                selectQuery = null;
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return devicesIDs;
        }

        public static DataTable GetDevicesToUser(int nSiteGuid)
        {
            ODBCWrapper.StoredProcedure spGetDevicesToUser = new ODBCWrapper.StoredProcedure("Get_DevicesToUser");
            spGetDevicesToUser.SetConnectionKey("USERS_CONNECTION_STRING");
            spGetDevicesToUser.AddParameter("@userID", nSiteGuid);
            DataSet ds = spGetDevicesToUser.ExecuteDataSet();
            if (ds != null)
                return ds.Tables[0];
            return null;
        }
               
        public static bool UpdateDomain(string sName, string sDescription, int nDomainID, int nGroupID, int nDomainRestriciton = 0)
        {
            bool res = false;

            ODBCWrapper.StoredProcedure spUpdateDomain = new ODBCWrapper.StoredProcedure(SP_UPDATE_DOMAIN_DATA);
            spUpdateDomain.SetConnectionKey("USERS_CONNECTION_STRING");

            spUpdateDomain.AddParameter("@domainID", nDomainID);
            spUpdateDomain.AddParameter("@groupID", nGroupID);
            spUpdateDomain.AddParameter("@name", sName);
            spUpdateDomain.AddParameter("@description", sDescription);
            spUpdateDomain.AddParameter("@restriction", nDomainRestriciton);

            int rowCount = spUpdateDomain.ExecuteReturnValue<int>();
            res = rowCount > 0;
            return res;
        }

        public static bool UpdateDomainCoGuid(int nDomainID, int nGroupID, string coGuid)
        {
            bool res = false;

            ODBCWrapper.StoredProcedure spUpdateDomain = new ODBCWrapper.StoredProcedure(SP_UPDATE_DOMAIN_CoGuid);
            spUpdateDomain.SetConnectionKey("USERS_CONNECTION_STRING");

            spUpdateDomain.AddParameter("@domainID", nDomainID);
            spUpdateDomain.AddParameter("@groupID", nGroupID);
            spUpdateDomain.AddParameter("@coGuid", coGuid);

            int rowCount = spUpdateDomain.ExecuteReturnValue<int>();
            res = rowCount > 0;



            return res;
        }

        public static bool DoesDomainNameExist(string sName, int nGroupID)
        {
            bool nameExists = false;

            try
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey("USERS_CONNECTION_STRING");

                selectQuery += "select id from domains WITH (nolock) where ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("name", "=", sName);
                selectQuery += " and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
                if (selectQuery.Execute("query", true) != null)
                {
                    int nCount = selectQuery.Table("query").DefaultView.Count;
                    nameExists = (nCount > 0);
                }

                selectQuery.Finish();
                selectQuery = null;
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return nameExists;
        }

        public static bool GetDomainSettings(int nDomainID, int nGroupID, ref string sName, ref string sDescription, ref int nDeviceLimitationModule,
            ref int nDeviceLimit, ref int nUserLimit, ref int nConcurrentLimit, ref int nStatus, ref int nIsActive, ref int nFrequencyFlag,
            ref int nDeviceMinPeriodId, ref int nUserMinPeriodId, ref DateTime dDeviceFrequencyLastAction, ref DateTime dUserFrequencyLastAction,
            ref string sCoGuid, ref int nDomainRestriction, ref DomainSuspentionStatus eDomainSuspendStat, ref int regionId)
        {
            int nGroupConcurrentMaxLimit = 0;
           

            return GetDomainSettings(nDomainID, nGroupID, ref sName, ref sDescription, ref nDeviceLimitationModule, ref nDeviceLimit,
                ref nUserLimit, ref nConcurrentLimit, ref nStatus, ref nIsActive, ref nFrequencyFlag, ref nDeviceMinPeriodId, ref nUserMinPeriodId,
                ref dDeviceFrequencyLastAction, ref dUserFrequencyLastAction, ref sCoGuid, ref nDomainID, ref nGroupConcurrentMaxLimit, ref eDomainSuspendStat, ref regionId);
        }


        public static bool GetDomainSettings(int nDomainID, int nGroupID, ref string sName, ref string sDescription, ref int nDeviceLimitationModule,
            ref int nDeviceLimit, ref int nUserLimit, ref int nConcurrentLimit, ref int nStatus, ref int nIsActive, ref int nFrequencyFlag,
            ref int nDeviceMinPeriodId, ref int nUserMinPeriodId, ref DateTime dDeviceFrequencyLastAction, ref DateTime dUserFrequencyLastAction,
            ref string sCoGuid, ref int nDomainRestriction, ref int nGroupConcurrentLimit, ref DomainSuspentionStatus suspendStatus, ref int regionId)
        {

            bool res = false;

            ODBCWrapper.StoredProcedure spGetDomainSettings = new ODBCWrapper.StoredProcedure(SP_GET_DOMAIN_SETTINGS);
            spGetDomainSettings.SetConnectionKey("USERS_CONNECTION_STRING");
            spGetDomainSettings.AddParameter("@domainID", nDomainID);
            spGetDomainSettings.AddNullableParameter<long?>("@groupID", nGroupID);
            DataSet ds = spGetDomainSettings.ExecuteDataSet();

            if (ds == null || ds.Tables == null || ds.Tables.Count == 0 || ds.Tables[0].DefaultView.Count == 0)
            {
                return false;
            }

            int nCount = ds.Tables[0].DefaultView.Count;
            if (nCount > 0)
            {
                DataRow dr = ds.Tables[0].DefaultView[0].Row;

                if (dr == null)
                {
                    return false;
                }

                sName = ODBCWrapper.Utils.GetSafeStr(dr, "NAME");
                sDescription = ODBCWrapper.Utils.GetSafeStr(dr, "DESCRIPTION");
                nDeviceLimitationModule = ODBCWrapper.Utils.GetIntSafeVal(dr, "MODULE_ID");
                nDeviceLimit = ODBCWrapper.Utils.GetIntSafeVal(dr, "MAX_LIMIT");
                nUserLimit = ODBCWrapper.Utils.GetIntSafeVal(dr, "USER_MAX_LIMIT");
                nConcurrentLimit = ODBCWrapper.Utils.GetIntSafeVal(dr, "CONCURRENT_MAX_LIMIT");
                nStatus = ODBCWrapper.Utils.GetIntSafeVal(dr, "STATUS");
                nIsActive = ODBCWrapper.Utils.GetIntSafeVal(dr, "IS_ACTIVE");
                nFrequencyFlag = ODBCWrapper.Utils.GetIntSafeVal(dr, "FREQUENCY_FLAG");
                nDeviceMinPeriodId = ODBCWrapper.Utils.GetIntSafeVal(dr, "freq_period_id");
                nUserMinPeriodId = ODBCWrapper.Utils.GetIntSafeVal(dr, "user_freq_period_id");
                sCoGuid = ODBCWrapper.Utils.GetSafeStr(dr, "COGUID");
                dDeviceFrequencyLastAction = ODBCWrapper.Utils.GetDateSafeVal(dr, "FREQUENCY_LAST_ACTION");
                dUserFrequencyLastAction = ODBCWrapper.Utils.GetDateSafeVal(dr, "USER_FREQUENCY_LAST_ACTION");
                nDomainRestriction = ODBCWrapper.Utils.GetIntSafeVal(dr, "RESTRICTION");
                nGroupConcurrentLimit = ODBCWrapper.Utils.GetIntSafeVal(dr, "GROUP_CONCURRENT_MAX_LIMIT");
                int suspendStatInt  = ODBCWrapper.Utils.GetIntSafeVal(dr, "IS_SUSPENDED");
                if (Enum.IsDefined(typeof(DomainSuspentionStatus), suspendStatInt))
                {
                    suspendStatus = (DomainSuspentionStatus)suspendStatInt;
                }
                regionId = ODBCWrapper.Utils.GetIntSafeVal(dr, "REGION_ID");
                res = true;
            }
            return res;
        }

        private static int GetGroupUserMinPeriodId(int nGroupID)
        {
            int nMinPeriod = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = null;

            try
            {
                selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey("MAIN_CONNECTION_STRING");
                selectQuery += "select mp.ID from groups g WITH (nolock), groups_device_limitation_modules lm WITH (nolock), lu_min_periods mp WITH (nolock) where ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("g.ID", "=", nGroupID);
                selectQuery += " and lm.user_freq_period_id = mp.ID and g.max_device_limit = lm.ID";
                selectQuery.Execute("query", true);
                if (selectQuery.Table("query").DefaultView.Count > 0)
                {
                    Object o_tmpMinPeriod = selectQuery.Table("query").Rows[0][0];
                    if (o_tmpMinPeriod != null && o_tmpMinPeriod != DBNull.Value)
                    {
                        nMinPeriod = int.Parse(o_tmpMinPeriod.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
            finally
            {
                if (selectQuery != null)
                {
                    selectQuery.Finish();
                    selectQuery = null;
                }
            }

            return nMinPeriod;
        }


        public static int GetGroupDeviceMinPeriodId(int nGroupId)
        {
            int nMinPeriod = 0;

            try
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey("MAIN_CONNECTION_STRING");
                selectQuery += "select mp.ID from groups g WITH (nolock), groups_device_limitation_modules lm WITH (nolock), lu_min_periods mp WITH (nolock) where ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("g.ID", "=", nGroupId);
                selectQuery += " and lm.freq_period_id = mp.ID and g.max_device_limit = lm.ID";
                selectQuery.Execute("query", true);
                if (selectQuery.Table("query").DefaultView.Count > 0)
                {
                    Object o_tmpMinPeriod = selectQuery.Table("query").Rows[0][0];
                    if (o_tmpMinPeriod != null && o_tmpMinPeriod != DBNull.Value)
                    {
                        nMinPeriod = int.Parse(o_tmpMinPeriod.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return nMinPeriod;
        }

        public static int GetDomainsDevicesCount(int nGroupID, int nDeviceID)
        {
            StoredProcedure sp = new StoredProcedure("Get_DomainsDevicesCount");
            sp.SetConnectionKey("USERS_CONNECTION_STRING");
            sp.AddParameter("@GroupID", nGroupID);
            sp.AddParameter("@DeviceID", nDeviceID);

            return sp.ExecuteReturnValue<int>();
        }

        public static bool UpdateDeviceStatus(int nDeviceID, int nIsActive, int nStatus)
        {
            bool res = false;

            try
            {
                ODBCWrapper.UpdateQuery updateQuery1 = new ODBCWrapper.UpdateQuery("devices");
                updateQuery1.SetConnectionKey("USERS_CONNECTION_STRING");
                updateQuery1 += ODBCWrapper.Parameter.NEW_PARAM("status", "=", nStatus);
                updateQuery1 += ODBCWrapper.Parameter.NEW_PARAM("is_Active", "=", nIsActive);
                updateQuery1 += " where ";
                updateQuery1 += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nDeviceID);
                res = updateQuery1.Execute();
                updateQuery1.Finish();
                updateQuery1 = null;
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return res;
        }

        public static int GetDomainIDByCoGuid(string coGuid, int groupId)
        {
            int nDomainID = 0;
            try
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey("USERS_CONNECTION_STRING");
                selectQuery += "select ID from domains WITH (nolock) where Status = 1 and Is_Active = 1 and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("CoGuid", "=", coGuid);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", groupId);
                if (selectQuery.Execute("query", true) != null)
                {
                    int nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        nDomainID = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "ID", 0);
                    }
                }
                selectQuery.Finish();
                selectQuery = null;

            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
            return nDomainID;
        }

        public static int GetUserIDByDomainActivationToken(int nGroupID, string sToken, ref int nUsersDomainID)
        {
            int nUserID = 0;

            try
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey("USERS_CONNECTION_STRING");

                selectQuery += "SELECT ID, USER_ID FROM USERS_DOMAINS WITH (NOLOCK) WHERE STATUS<>2 AND";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("ACTIVATION_TOKEN", "=", sToken);

                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        nUserID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["USER_ID"].ToString());
                        nUsersDomainID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
                    }
                }
                selectQuery.Finish();
                selectQuery = null;

            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return nUserID;

        }

        public static bool UpdateUserDomainActivationToken(int nGroupID, int nUsersDomainID, string sToken, string sNewToken)
        {
            bool isActivated = false;

            try
            {
                ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("users_domains");
                updateQuery.SetConnectionKey("USERS_CONNECTION_STRING");

                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ACTIVATION_TOKEN", "=", sNewToken);
                updateQuery += " where ";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
                updateQuery += " and ";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nUsersDomainID);
                updateQuery += " and ";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ACTIVATION_TOKEN", "=", sToken);

                isActivated = updateQuery.Execute();

                updateQuery.Finish();
                updateQuery = null;
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return isActivated;
        }

        public static int GetUserDomainActivateStatus(int nGroupID, int nUsersDomainID)
        {
            int nActivationStatus = -1;

            try
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey("USERS_CONNECTION_STRING");

                selectQuery += " SELECT IS_ACTIVE FROM USERS WITH (NOLOCK) WHERE STATUS=1 AND IS_ACTIVE=1 AND ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nUsersDomainID);
                selectQuery += " and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);

                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {

                        nActivationStatus = int.Parse(selectQuery.Table("query").DefaultView[0].Row["IS_ACTIVE"].ToString());
                    }
                }

                selectQuery.Finish();
                selectQuery = null;

            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return nActivationStatus;
        }

        public static int GetDomainIDBySiteGuid(int nGroupID, int nSiteGuid, ref int nOperatorID, ref bool bIsDomainMaster, ref DomainSuspentionStatus eSuspendStat)
        {
            return (int)Get_DomainDataBySiteGuid(nGroupID, nSiteGuid, ref nOperatorID, ref bIsDomainMaster,ref eSuspendStat);
        }

        public static List<int> GetDomainIDsByEmail(int nGroupID, string sEmail)
        {
            List<int> lDomainIDs = new List<int>();

            try
            {
                ODBCWrapper.StoredProcedure spGetDomainIDs = new ODBCWrapper.StoredProcedure(SP_GET_DOMAIN_IDS_BY_EMAIL);
                spGetDomainIDs.SetConnectionKey("USERS_CONNECTION_STRING");

                spGetDomainIDs.AddParameter("@groupID", nGroupID);
                spGetDomainIDs.AddParameter("@email", sEmail);
                DataSet ds = spGetDomainIDs.ExecuteDataSet();

                if ((ds == null) || (ds.Tables.Count == 0) || (ds.Tables[0].DefaultView.Count == 0))
                {
                    return lDomainIDs;
                }

                int nCount = ds.Tables[0].DefaultView.Count;

                for (int i = 0; i < nCount; i++)
                {
                    DataRow dr = ds.Tables[0].DefaultView[i].Row;

                    if (dr == null)
                    {
                        return lDomainIDs;
                    }

                    int nDomainID = int.Parse(dr["DOMAIN_ID"].ToString());

                    if (nDomainID > 0)
                    {
                        lDomainIDs.Add(nDomainID);
                    }
                }

                spGetDomainIDs = null;
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return lDomainIDs;

        }

        public static int SetDomainStatus(int nGroupID, int nDomainID, int nIsActive, int nStatus)
        {
            int status = (-1);


            ODBCWrapper.StoredProcedure spRemoveDomain = new ODBCWrapper.StoredProcedure(SP_REMOVE_DOMAIN);
            spRemoveDomain.SetConnectionKey("USERS_CONNECTION_STRING");

            spRemoveDomain.AddParameter("@DomainID", nDomainID);
            spRemoveDomain.AddParameter("@GroupID", nGroupID);
            spRemoveDomain.AddParameter("@Status", nStatus);
            spRemoveDomain.AddParameter("@IsActive", nIsActive);

            status = spRemoveDomain.ExecuteReturnValue<int>();


            return status;
        }

        public static List<int> GetDomainIDsByOperatorCoGuid(string coGuid)
        {
            List<int> lDomainIDs = new List<int>();

            try
            {
                ODBCWrapper.StoredProcedure spGetDomainIDs = new ODBCWrapper.StoredProcedure(SP_GET_DOMAIN_IDS_BY_OPERATOR_COGUID);
                spGetDomainIDs.SetConnectionKey("USERS_CONNECTION_STRING");

                spGetDomainIDs.AddParameter("@CoGuid", coGuid);
                DataSet ds = spGetDomainIDs.ExecuteDataSet();

                if ((ds == null) || (ds.Tables.Count == 0) || (ds.Tables[0].DefaultView.Count == 0))
                {
                    return lDomainIDs;
                }

                int nCount = ds.Tables[0].DefaultView.Count;

                for (int i = 0; i < nCount; i++)
                {
                    DataRow dr = ds.Tables[0].DefaultView[i].Row;

                    if (dr == null)
                    {
                        return lDomainIDs;
                    }

                    int nDomainID = int.Parse(dr["ID"].ToString());

                    if (nDomainID > 0)
                    {
                        lDomainIDs.Add(nDomainID);
                    }
                }

                spGetDomainIDs = null;
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return lDomainIDs;
        }

        public static string GetDomainCoGuid(int nDomainID = 0, string sSiteGuid = "")
        {
            string sCoGuid = string.Empty;

            if (nDomainID == 0 && string.IsNullOrEmpty(sSiteGuid))
            {
                return sCoGuid;
            }

            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure(SP_GET_DOMAIN_COGUID);
            sp.SetConnectionKey("USERS_CONNECTION_STRING");
            sp.AddParameter("@domainID", nDomainID);
            sp.AddParameter("@SiteGuid", sSiteGuid);

            sCoGuid = sp.ExecuteReturnValue<string>();

            return sCoGuid;
        }

        public static string GetDomainDesc(int nGroupID, int nDomainID)
        {
            string desc = string.Empty;

            try
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey("USERS_CONNECTION_STRING");

                selectQuery += "SELECT DESCRIPTION FROM DOMAINS WITH (NOLOCK) WHERE STATUS=1 AND IS_ACTIVE = 1 AND ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nDomainID);
                selectQuery += " AND ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);

                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        desc = selectQuery.Table("query").DefaultView[0].Row["DESCRIPTION"].ToString();
                    }
                }
                selectQuery.Finish();
                selectQuery = null;
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return desc;
        }


        public static int GetDeviceIDByDomainActivationToken(int nGroupID, string sToken, ref int nDomainsDevicesID)
        {
            int nDeviceID = 0;

            try
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey("USERS_CONNECTION_STRING");

                selectQuery += "SELECT ID, DEVICE_ID FROM DOMAINS_DEVICES WITH (NOLOCK) WHERE STATUS=3 AND";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
                selectQuery += " and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("ACTIVATION_TOKEN", "=", sToken);

                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        nDeviceID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["DEVICE_ID"].ToString());
                        nDomainsDevicesID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
                    }
                }
                selectQuery.Finish();
                selectQuery = null;

            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return nDeviceID;
        }

        public static int UpdateDeviceDomainActivationToken(int nGroupID, int nDomainsDevicesID, int nDeviceID, string sToken, string sNewToken)
        {
            ODBCWrapper.StoredProcedure spUpdateDeviceActivation = new ODBCWrapper.StoredProcedure("Update_DeviceActivation");
            spUpdateDeviceActivation.SetConnectionKey("USERS_CONNECTION_STRING");
            spUpdateDeviceActivation.AddParameter("@domainsDevicesID", nDomainsDevicesID);
            spUpdateDeviceActivation.AddParameter("@deviceID", nDeviceID);
            spUpdateDeviceActivation.AddParameter("@groupID", nGroupID);
            spUpdateDeviceActivation.AddParameter("@token", sToken);
            spUpdateDeviceActivation.AddParameter("@newToken", sNewToken);

            int rowsAffected = spUpdateDeviceActivation.ExecuteReturnValue<int>();

            return rowsAffected;
        }

        public static int GetDomainDeviceActivateStatus(int nGroupID, int nDeviceID)
        {
            int nActivationStatus = -1;

            try
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey("USERS_CONNECTION_STRING");

                selectQuery += " SELECT IS_ACTIVE FROM DEVICES WITH (NOLOCK) WHERE STATUS=1 AND IS_ACTIVE=1 AND ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nDeviceID);
                selectQuery += " and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);

                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {

                        nActivationStatus = int.Parse(selectQuery.Table("query").DefaultView[0].Row["IS_ACTIVE"].ToString());
                    }
                }

                selectQuery.Finish();
                selectQuery = null;

            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return nActivationStatus;
        }

        public static int SwitchDomainMaster(int nGroupID, int nDomainID, int nCurrentMasterID, int nNewMasterID)
        {
            ODBCWrapper.StoredProcedure spSwitchDomainMaster = new ODBCWrapper.StoredProcedure(SP_UPDATE_SWITCH_DOMAIN_MASTER);
            spSwitchDomainMaster.SetConnectionKey("USERS_CONNECTION_STRING");
            spSwitchDomainMaster.AddParameter("@domainID", nDomainID);
            spSwitchDomainMaster.AddParameter("@groupID", nGroupID);
            spSwitchDomainMaster.AddParameter("@oldMasterID", nCurrentMasterID);
            spSwitchDomainMaster.AddParameter("@newMasterID", nNewMasterID);

            int rowsAffected = spSwitchDomainMaster.ExecuteReturnValue<int>();

            return rowsAffected;
        }

        public static bool IsSingleDomainEnvironment(int nGroupID)
        {
            bool isSingleDomainEnv = true;

            ODBCWrapper.StoredProcedure spGetDomainEnvironment = new ODBCWrapper.StoredProcedure("GET_DomainEnvironment");
            spGetDomainEnvironment.SetConnectionKey("MAIN_CONNECTION_STRING");
            spGetDomainEnvironment.AddParameter("@groupID", nGroupID);
            DataSet ds = spGetDomainEnvironment.ExecuteDataSet();

            if ((ds != null) && (ds.Tables.Count != 0) && (ds.Tables[0].DefaultView.Count != 0))
            {
                DataRow dr = ds.Tables[0].DefaultView[0].Row;
                if (dr != null)
                {
                    string sDomainEnv = ODBCWrapper.Utils.GetSafeStr(dr, "description");
                    DomianEnvironmentType eType = (DomianEnvironmentType)Enum.Parse(typeof(DomianEnvironmentType), sDomainEnv, true);
                    if (eType == ApiObjects.DomianEnvironmentType.MUS)
                        return isSingleDomainEnv = false;
                }
            }


            return isSingleDomainEnv;
        }

        public static bool Get_ProximityDetectionDataForInsertion(int nGroupID, long lDomainID, ref int quantity, ref DataTable homeNetworksTable, long dlmId = 0)
        {
            bool res = false;
            StoredProcedure sp = new StoredProcedure("Get_ProximityDetectionDataForInsertion");
            sp.SetConnectionKey("USERS_CONNECTION_STRING");
            sp.AddParameter("@GroupID", nGroupID);
            sp.AddParameter("@DomainID", lDomainID);
            sp.AddParameter("@dlmID", dlmId);

            DataSet ds = sp.ExecuteDataSet();
            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
            {
                DataTable dt = ds.Tables[0];
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    res = true;
                    quantity = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0]["quantity"]);
                    if (quantity < 1)
                        quantity = Int32.MaxValue;
                    if (ds.Tables.Count > 1)
                        homeNetworksTable = ds.Tables[1];
                    else
                        homeNetworksTable = null;
                }
                else
                {
                    res = false;
                }
            }
            else
            {
                res = false;
            }

            return res;

        }

        public static DataTable Insert_NewHomeNetwork(int nGroupID, string sNetworkID, long lDomainID, string sName, string sDesc, bool bIsActive, DateTime dtCreateDate)
        {
            DataTable dt = null;

            StoredProcedure sp = new StoredProcedure("Insert_NewHomeNetwork_New");
            sp.SetConnectionKey("USERS_CONNECTION_STRING");
            sp.AddParameter("@GroupID", nGroupID);
            sp.AddParameter("@NetworkID", sNetworkID);
            sp.AddParameter("@DomainID", lDomainID);
            sp.AddParameter("@Name", sName);
            sp.AddParameter("@Description", sDesc);
            sp.AddParameter("@IsActive", bIsActive);
            sp.AddParameter("@CreateDate", dtCreateDate);

            DataSet ds = sp.ExecuteDataSet();
            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
            {
                dt = ds.Tables[0];
            }

            return dt;
        }

        public static bool Get_ProximityDetectionDataForUpdating(int nGroupID, long lDomainID, string sNetworkID, ref int quantity, ref int frequency, ref DateTime lastDeactivationDate, ref DataTable homeNetworksTable, long dlmId = 0)
        {
            bool res = false;
            StoredProcedure sp = new StoredProcedure("Get_ProximityDetectionDataForUpdating");
            sp.SetConnectionKey("USERS_CONNECTION_STRING");
            sp.AddParameter("@GroupID", nGroupID);
            sp.AddParameter("@DomainID", lDomainID);
            sp.AddParameter("@dlmId", dlmId);

            DataSet ds = sp.ExecuteDataSet();
            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
            {
                DataTable dt = ds.Tables[0];
                quantity = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0]["quantity"]);
                if (quantity < 1)
                    quantity = Int32.MaxValue;
                frequency = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0]["frequency"]);
                if (ds.Tables.Count == 3)
                {
                    dt = ds.Tables[1];
                    if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                    {
                        lastDeactivationDate = ODBCWrapper.Utils.GetDateSafeVal(dt.Rows[0]["last_deactivation_date"]);
                        if (lastDeactivationDate.Equals(FICTIVE_DATE))
                            lastDeactivationDate = DateTime.MinValue;
                    }
                    else
                    {
                        lastDeactivationDate = DateTime.MinValue;
                    }
                    homeNetworksTable = ds.Tables[2];

                    res = true;
                }
                else
                {
                    res = false;
                }
            }
            else
            {
                res = false;
            }

            return res;
        }

        public static DataRow Update_HomeNetworkWithoutDeactivationDate(long lDomainID, string sNetworkID, int nGroupID, string sName,
            string sDesc, bool bIsActive)
        {
            StoredProcedure sp = new StoredProcedure("Update_HomeNetworkWithoutDeactivationDate");
            sp.SetConnectionKey("USERS_CONNECTION_STRING");
            sp.AddParameter("@DomainID", lDomainID);
            sp.AddParameter("@NetworkID", sNetworkID);
            sp.AddParameter("@GroupID", nGroupID);
            sp.AddParameter("@Name", sName);
            sp.AddParameter("@Description", sDesc);
            sp.AddParameter("@UpdateDate", DateTime.UtcNow);
            sp.AddParameter("@IsActive", bIsActive);

            DataSet ds = sp.ExecuteDataSet();
            if (ds == null || ds.Tables == null || ds.Tables.Count == 0)
                return null;

            DataTable dt = ds.Tables[0];
            if (dt == null || dt.Rows == null || dt.Rows.Count == 0)
                return null;

            return dt.Rows[0];
        }

        public static DataRow Update_HomeNetworkWithDeactivationDate(long lDomainID, string sNetworkID, int nGroupID, string sName,
            string sDesc, bool bTrueForDeactivationFalseForDeletion)
        {
            StoredProcedure sp = new StoredProcedure("Update_HomeNetworkWithDeactivationDate");
            sp.SetConnectionKey("USERS_CONNECTION_STRING");
            sp.AddParameter("@DomainID", lDomainID);
            sp.AddParameter("@NetworkID", sNetworkID);
            sp.AddParameter("@GroupID", nGroupID);
            sp.AddParameter("@Name", sName);
            sp.AddParameter("@Description", sDesc);
            sp.AddParameter("@UpdateDate", DateTime.UtcNow);
            sp.AddParameter("@IsDelete", !bTrueForDeactivationFalseForDeletion);

            DataSet ds = sp.ExecuteDataSet();
            if (ds == null || ds.Tables == null || ds.Tables.Count == 0)
                return null;

            DataTable dt = ds.Tables[0];
            if (dt == null || dt.Rows == null || dt.Rows.Count == 0)
                return null;

            return dt.Rows[0];
        }

        public static DataTable Get_DomainHomeNetworks(long lDomainID, int nGroupID)
        {
            StoredProcedure sp = new StoredProcedure("Get_DomainHomeNetworks");
            sp.SetConnectionKey("USERS_CONNECTION_STRING");
            sp.AddParameter("@DomainID", lDomainID);
            sp.AddParameter("@GroupID", nGroupID);

            DataSet ds = sp.ExecuteDataSet();

            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
                return ds.Tables[0];
            return null;
        }

        public static long Get_DomainDataBySiteGuid(int nGroupID, long lSiteGuid, ref int nOperatorID, ref bool bIsDomainMaster, ref DomainSuspentionStatus eDomainSuspendStatus)
        {
            long res = 0;
            StoredProcedure sp = new StoredProcedure("Get_DomainDataBySiteGuid");
            sp.SetConnectionKey("USERS_CONNECTION_STRING");
            sp.AddParameter("@GroupID", nGroupID);
            sp.AddParameter("@SiteGuid", lSiteGuid);

            DataSet ds = sp.ExecuteDataSet();
            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
            {
                DataTable dt = ds.Tables[0];
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    res = ODBCWrapper.Utils.GetLongSafeVal(dt.Rows[0]["domain_id"]);
                    nOperatorID = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0]["operator_id"]);
                    bIsDomainMaster = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0]["is_master"]) == 1;
                    int domainSusStat = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0]["IS_SUSPENDED"]);
                    if (Enum.IsDefined(typeof(DomainSuspentionStatus), domainSusStat))
                    {
                        eDomainSuspendStatus = (DomainSuspentionStatus)domainSusStat;
                    }  
                }
            }

            return res;
        }

        public static DataTable Get_DomainDevices(int nGroupID, long lDomainID)
        {
            StoredProcedure sp = new StoredProcedure("Get_DomainDevices");
            sp.SetConnectionKey("USERS_CONNECTION_STRING");
            sp.AddParameter("@GroupID", nGroupID);
            sp.AddParameter("@DomainID", lDomainID);

            DataSet ds = sp.ExecuteDataSet();
            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
                return ds.Tables[0];
            return null;
        }

        public static int Get_IsDeviceExistInDomain(int nDomainID, int nGroupID, string sDeviceUDID, ref int isActive, ref int nDeviceID)
        {
            int res = 0;
            StoredProcedure sp = new StoredProcedure("Get_IsDeviceExistInDomain");
            sp.SetConnectionKey("USERS_CONNECTION_STRING");
            sp.AddParameter("@DomainID", nDomainID);
            sp.AddParameter("@GroupID", nGroupID);
            sp.AddParameter("@DeviceUDID", sDeviceUDID);

            DataSet ds = sp.ExecuteDataSet();
            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
            {
                DataTable dt = ds.Tables[0];
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    res = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0]["id"]);
                    isActive = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0]["is_active"]);
                    nDeviceID = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0]["device_id"]);
                }
            }
            return res;
        }

        public static List<string[]> Get_DeviceFamiliesLimits(int nGroupID, int nDomainLimitID, ref Dictionary<int, int> concurrenyOverride, ref Dictionary<int, int> quantityOverride)
        {
            List<string[]> res = null;
            StoredProcedure sp = new StoredProcedure("Get_DeviceFamiliesLimits");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@GroupID", nGroupID);
            sp.AddParameter("@DomainLimitID", nDomainLimitID);
            DataSet ds = sp.ExecuteDataSet();
            if (ds != null && ds.Tables != null && ds.Tables.Count == 2)
            {
                DataTable dt = ds.Tables[0];
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    res = new List<string[]>(dt.Rows.Count);
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        string sFamilyID = dt.Rows[i]["ID"].ToString();
                        string sFamilyName = ODBCWrapper.Utils.GetSafeStr(dt.Rows[i]["NAME"]);
                        string[] dbDeviceContainer = new string[2] { sFamilyID, sFamilyName };
                        res.Add(dbDeviceContainer);
                    } // end for

                    DataTable dtSpecificLimits = ds.Tables[1];
                    if (dtSpecificLimits != null && dtSpecificLimits.Rows != null && dtSpecificLimits.Rows.Count > 0)
                    {
                        for (int i = 0; i < dtSpecificLimits.Rows.Count; i++)
                        {
                            int nFamilyID = ODBCWrapper.Utils.GetIntSafeVal(dtSpecificLimits.Rows[i]["device_family_id"]);
                            string sLimitationType = ODBCWrapper.Utils.GetSafeStr(dtSpecificLimits.Rows[i]["description"]);
                            int nLimitationValue = ODBCWrapper.Utils.GetIntSafeVal(dtSpecificLimits.Rows[i]["value"], -1);

                            if (nFamilyID > 0 && nLimitationValue > -1 && sLimitationType.Length > 0)
                            {
                                if (String.Compare(sLimitationType, "concurrency", true) == 0)
                                {
                                    concurrenyOverride.Add(nFamilyID, nLimitationValue);
                                }
                                else
                                {
                                    if (String.Compare(sLimitationType, "quantity", true) == 0)
                                    {
                                        quantityOverride.Add(nFamilyID, nLimitationValue);
                                    }
                                }
                            }
                        } // end for
                    }
                }
                else
                {
                    res = new List<string[]>(0);
                }
            }
            else
            {
                res = new List<string[]>(0);
            }

            return res;
        }

        public static List<string> Get_FullUserListOfDomain(int nGroupID, int nDomainID)
        {
            List<string> res = null;
            StoredProcedure sp = new StoredProcedure("Get_FullUserListOfDomain");
            sp.SetConnectionKey("USERS_CONNECTION_STRING");
            sp.AddParameter("@GroupID", nGroupID);
            sp.AddParameter("@DomainID", nDomainID);

            DataSet ds = sp.ExecuteDataSet();
            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
            {
                DataTable dt = ds.Tables[0];
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    res = new List<string>(dt.Rows.Count);
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        long lSiteGuid = ODBCWrapper.Utils.GetLongSafeVal(dt.Rows[i]["USER_ID"]);
                        int nStatus = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[i]["STATUS"]);
                        if (nStatus == 1)
                        {
                            // user is approved for this domain
                            res.Add(lSiteGuid + "");
                        }
                        else
                        {
                            // user is pending.
                            res.Add((lSiteGuid * (-1)) + "");
                        }
                    }
                }
                else
                {
                    res = new List<string>(0);
                }
            }
            else
            {
                res = new List<string>(0);
            }

            return res;
        }

        public static bool UpdateDomainsDevicesStatus(int m_nDomainID, int m_nGroupID, string sUDID, int nIsActive, int nStatus)
        {
            StoredProcedure sp = new StoredProcedure("Update_DomainsDevicesStatusByParams");
            sp.SetConnectionKey("USERS_CONNECTION_STRING");
            sp.AddParameter("@DomainID", m_nDomainID);
            sp.AddParameter("@GroupID", m_nGroupID);
            sp.AddParameter("@UDID", sUDID);
            sp.AddParameter("@IsActive", nIsActive);
            sp.AddParameter("@Status", nStatus);
            sp.AddParameter("@UpdateDate", DateTime.UtcNow);

            return sp.ExecuteReturnValue<bool>();
        }

        public static bool ChangeSuspendDomainStatus(int nDomainID, int nGroupID, DomainSuspentionStatus nStatus)
        {
            StoredProcedure sp = new StoredProcedure("Update_DomainSuspendStatus");
            sp.SetConnectionKey("USERS_CONNECTION_STRING");
            sp.AddParameter("@domainID", nDomainID);
            sp.AddParameter("@groupID", nGroupID);
            sp.AddParameter("@IsSuspended", nStatus);           
            return sp.ExecuteReturnValue<bool>();
        }

        public static int Get_DomainLimitID(int nGroupID)
        {
            StoredProcedure sp = new StoredProcedure("Get_DomainLimitID");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@GroupID", nGroupID);

            return sp.ExecuteReturnValue<int>();
        }

        public static DataSet Get_GroupLimitsAndDeviceFamilies(int nGroupID, int nDomainLimitID)
        {
            StoredProcedure sp = new StoredProcedure("Get_GroupLimitsAndDeviceFamilies");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@GroupID", nGroupID);
            sp.AddParameter("@DomainLimitID", nDomainLimitID);
            DataSet ds = sp.ExecuteDataSet();
            return ds;
        }

        public static List<string> SetUsersStatus(List<int> users, int nUserToDelete, int status, int isActive, int domainID)
        {
            List<string> usersChange = new List<string>();
            StoredProcedure sp = new StoredProcedure("SetUsersStatus");
            sp.SetConnectionKey("USERS_CONNECTION_STRING");
            sp.AddParameter("@top", nUserToDelete);
            sp.AddParameter("@status", status);
            sp.AddParameter("@isActive", isActive);
            sp.AddParameter("@dominID", domainID);
            sp.AddIDListParameter<int>("@usersID", users, "Id");            
            sp.AddParameter("@UpdateDate", DateTime.UtcNow);

            DataSet ds = sp.ExecuteDataSet();

            if (ds != null && ds.Tables != null && ds.Tables.Count > 0 && ds.Tables[0] != null && ds.Tables[0].Rows != null)
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    usersChange.Add(ODBCWrapper.Utils.GetSafeStr(dr, "user_id"));
                }
            }
            return usersChange; 
        }

        public static List<string> SetDevicesDomainStatus(int nDeviceToDelete, int isActive, int domainID, List<int> lDevicesID, int? status = null)            
        {
            List<string> devicesChange = new List<string>();
            StoredProcedure sp = new StoredProcedure("SetDevicesDomainStatus");
            sp.SetConnectionKey("USERS_CONNECTION_STRING");
            sp.AddParameter("@top", nDeviceToDelete);            
            sp.AddParameter("@isActive", isActive);
            sp.AddParameter("@dominID", domainID);
            sp.AddIDListParameter<int>("@devicesID", lDevicesID, "Id");
            sp.AddParameter("@UpdateDate", DateTime.UtcNow);
            if (status != null)
                sp.AddParameter("@status", status);

            DataSet ds = sp.ExecuteDataSet();

            if (ds != null && ds.Tables != null && ds.Tables.Count > 0 && ds.Tables[0] != null && ds.Tables[0].Rows != null)
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    devicesChange.Add(ODBCWrapper.Utils.GetSafeStr(dr, "device_id"));
                }
            }
            return devicesChange;
        }

        public static List<string> SetDevicesDomainStatusNotInList(int nDeviceToDelete, int isActive, int domainID, List<int> lDevicesID, int? status = null)
        {
            List<string> devicesChange = new List<string>();
            StoredProcedure sp = new StoredProcedure("SetDevicesDomainStatusNotInList");
            sp.SetConnectionKey("USERS_CONNECTION_STRING");
            sp.AddParameter("@top", nDeviceToDelete);
            sp.AddParameter("@isActive", isActive);
            sp.AddParameter("@dominID", domainID);
            sp.AddIDListParameter<int>("@devicesID", lDevicesID, "Id");
            sp.AddParameter("@UpdateDate", DateTime.UtcNow);
            if (status != null)
                sp.AddParameter("@status", status);

            DataSet ds = sp.ExecuteDataSet();

            if (ds != null && ds.Tables != null && ds.Tables.Count > 0 && ds.Tables[0] != null && ds.Tables[0].Rows != null)
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    devicesChange.Add(ODBCWrapper.Utils.GetSafeStr(dr, "device_id"));
                }
            }
            return devicesChange;
        }

        public static bool ChangeDomainDLM(int domainID, int domianLimitID)
        {
            List<string> devicesChange = new List<string>();
            StoredProcedure sp = new StoredProcedure("ChangeDomainDLM");
            sp.SetConnectionKey("USERS_CONNECTION_STRING");
            sp.AddParameter("@domainID", domainID);
            sp.AddParameter("@domianLimitID", domianLimitID);

            return sp.ExecuteReturnValue<bool>();
        }

        public static bool UpdateDomainRegion(int domainId, int groupId, string extRegionId, string lookupKey)
        {
            StoredProcedure sp = new StoredProcedure("Update_DomainRegion");
            sp.SetConnectionKey("USERS_CONNECTION_STRING");
            sp.AddParameter("@domainID", domainId);
            sp.AddParameter("@groupID", groupId);

            if (!string.IsNullOrEmpty(extRegionId))
            {
                sp.AddParameter("@extRegionID", extRegionId);
                sp.AddParameter("@lookupKey", string.Empty);
            }
            else 
            {
                sp.AddParameter("@lookupKey", lookupKey);
                sp.AddParameter("@extRegionID", string.Empty);
            }

            return sp.ExecuteReturnValue<bool>();
        }

        public static int GetDomainIDBySiteGuid(int groupId, string siteGuid)
        {
            StoredProcedure sp = new StoredProcedure("Get_DomainIDByUser");
            sp.SetConnectionKey("USERS_CONNECTION_STRING");
            sp.AddParameter("@userID", siteGuid);

            var res = sp.ExecuteReturnValue();
            if (res == null)
                return 0;

            return Convert.ToInt32(res);
        }

        public static DateTime? GetDomainLastReconciliationDate(int groupId, long domainId)
        {
            List<string> devicesChange = new List<string>();
            StoredProcedure sp = new StoredProcedure("Get_DomainLastReconciliationDate");
            sp.SetConnectionKey("USERS_CONNECTION_STRING");
            sp.AddParameter("@domain_id", domainId);
            sp.AddParameter("@group_id", groupId);

            return sp.ExecuteReturnValue<DateTime?>();
        }

        public static int Set_DomainLastReconciliationDate(int groupID, long domainId, DateTime reconciliationDate)
        {
            StoredProcedure sp = new StoredProcedure("Set_DomainLastReconciliationDate");
            sp.SetConnectionKey("USERS_CONNECTION_STRING");
            sp.AddParameter("@reconciliation_date", reconciliationDate);
            sp.AddParameter("@domain_id", domainId);
            sp.AddParameter("@group_id", groupID);

            return sp.ExecuteReturnValue<int>();
        }

        public static bool GetCloseAccountMailTrigger(int groupID)
        {
            bool removeHHMail = true;
            DataRow row = GetMailTriggerAccountSettings(groupID);
            removeHHMail = ODBCWrapper.Utils.GetIntSafeVal(row, "SEND_CLOSE_ACCOUNT_MAIL") == 1 ? true : false;
            return removeHHMail;
        }

        private static DataRow GetMailTriggerAccountSettings(int groupID)
        {
            DataRow row = null;
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_MailTriggerAccountSettings");
            sp.SetConnectionKey("USERS_CONNECTION_STRING");
            sp.AddParameter("@groupID", groupID);
            DataSet ds = sp.ExecuteDataSet();

            if (ds != null && ds.Tables != null && ds.Tables.Count > 0 && ds.Tables[0].Rows != null && ds.Tables[0].Rows.Count > 0)
            {
                row = ds.Tables[0].Rows[0];
            }
            return row;
        }


        public static bool UpdateDeviceDrmID(int groupId, string deviceId, string drmId, int domainId)
        {
            StoredProcedure sp = new StoredProcedure("Update_DeviceDrmID");
            sp.SetConnectionKey("USERS_CONNECTION_STRING");
            sp.AddParameter("@groupId", groupId);
            sp.AddParameter("@deviceId", deviceId);
            sp.AddParameter("@drmId", drmId);

            bool result = sp.ExecuteReturnValue<bool>();
            if (result)
            {
                // remove all object from CB
                result = RemoveDomainDrmId(domainId);
            }
            return result;
        }


        public static DrmPolicy GetDrmPolicy(int groupId)
        {   
            DrmPolicy response = null;
            CouchbaseManager.CouchbaseManager cbClient = new CouchbaseManager.CouchbaseManager(CouchbaseManager.eCouchbaseBucket.CACHE);
            int limitRetries = RETRY_LIMIT;
            Random r = new Random();
            Couchbase.IO.ResponseStatus getResult = new Couchbase.IO.ResponseStatus();
            string drmPolicyKey = UtilsDal.GetDrmPolicyKey(groupId);
            if (string.IsNullOrEmpty(drmPolicyKey))
            {
                log.ErrorFormat("Failed getting drmPolicyKey for groupId: {0}", groupId);
            }
            else
            {
                try
                {
                    int numOfRetries = 0;
                    while (numOfRetries < limitRetries)
                    {
                        response = cbClient.Get<DrmPolicy>(drmPolicyKey, out getResult);

                        if (getResult == Couchbase.IO.ResponseStatus.Success)
                        {
                            break;
                        }
                        else
                        {
                            log.ErrorFormat("Retrieving drm policy groupId: {0} and key {1} failed with status: {2}, retryAttempt: {3}, maxRetries: {4}", groupId, drmPolicyKey, getResult, numOfRetries, limitRetries);
                            numOfRetries++;
                            System.Threading.Thread.Sleep(r.Next(50));
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("Error while trying to get drm policy, groupId: {0}, ex: {1}", groupId, ex);
                }
            }

            return response;
        }

        public static Dictionary<int, string> GetDomainDrmId(int domainId)
        {
            Dictionary<int, string> response = null;
            CouchbaseManager.CouchbaseManager cbClient = new CouchbaseManager.CouchbaseManager(CouchbaseManager.eCouchbaseBucket.CACHE);
            int limitRetries = RETRY_LIMIT;
            Random r = new Random();
            Couchbase.IO.ResponseStatus getResult = new Couchbase.IO.ResponseStatus();
            string domainDrmIdKey = UtilsDal.GetDomainDrmIdKey(domainId);
            if (string.IsNullOrEmpty(domainDrmIdKey))
            {
                log.ErrorFormat("Failed getting domainDrmIdKey for domainId: {0}", domainId);
            }
            else
            {
                try
                {
                    int numOfRetries = 0;
                    while (numOfRetries < limitRetries)
                    {
                        object document = cbClient.Get<object>(domainDrmIdKey, out getResult);

                        if (getResult == Couchbase.IO.ResponseStatus.Success)
                        {
                            // Deserialize to known class - for comfortable access
                            response = JsonConvert.DeserializeObject<Dictionary<int, string>>(document.ToString());
                            break;
                        }
                        else
                        {
                            log.ErrorFormat("Retrieving drm policy domainId: {0} and key {1} failed with status: {2}, retryAttempt: {3}, maxRetries: {4}", domainId, domainDrmIdKey, getResult, numOfRetries, limitRetries);
                            numOfRetries++;
                            System.Threading.Thread.Sleep(r.Next(50));
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("Error while trying to get drm policy, domainId: {0}, ex: {1}", domainId, ex);
                }
            }

            return response;
        }

        public static bool SetDomainDrmId(Dictionary<int, string> domainDrmId, int domainId)
        {
            bool result = false;
            if (domainDrmId != null && domainDrmId.Count > 0)
            {
                string domainDrmIdKey = UtilsDal.GetDomainDrmIdKey(domainId);
                if (string.IsNullOrEmpty(domainDrmIdKey))
                {
                    log.ErrorFormat("Failed getting domainDrmIdKey for domainId: {0}", domainId);
                }
                else
                {
                    CouchbaseManager.CouchbaseManager client = new CouchbaseManager.CouchbaseManager(CouchbaseManager.eCouchbaseBucket.CACHE);
                    
                    var json = JsonConvert.SerializeObject(domainDrmId);
                    result = client.Set<object>(domainDrmIdKey, json);

                    if (!result)
                    {
                        log.ErrorFormat("Failed updating domainDrmId in Couchbase. domainId = {0}", domainId);
                    }
                }
            }
            return result;
        }

        public static bool RemoveDomainDrmId(int domainId)
        {
            bool result = false;

            string domainDrmIdKey = UtilsDal.GetDomainDrmIdKey(domainId);
            if (string.IsNullOrEmpty(domainDrmIdKey))
            {
                log.ErrorFormat("Failed getting domainDrmIdKey for domainId: {0}", domainId);
            }
            else
            {
                CouchbaseManager.CouchbaseManager client = new CouchbaseManager.CouchbaseManager(CouchbaseManager.eCouchbaseBucket.CACHE);
                               
                result = client.Remove(domainDrmIdKey);

                if (!result)
                {
                    log.ErrorFormat("Failed Remove DomainDrmId domainDrmId in Couchbase. domainId = {0}", domainId);
                }
            }

            return result;
        }
    }
}
