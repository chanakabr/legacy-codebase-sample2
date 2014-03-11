using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tvinci.Core.DAL;
using System.Data;


namespace DAL
{
    public class DomainDal : BaseDal
    {
        #region Private Constants

        private const string SP_GET_USER_EXISTS_IN_DOMAIN           = "Get_UserExistsInDomain";
        private const string SP_GET_USERS_IN_DOMAIN                 = "Get_UsersInDomain";
        private const string SP_GET_DOMAIN_SETTINGS                 = "sp_GetDomainSettings";
        private const string SP_GET_DEVICE_FAMILIES_LIMITS          = "sp_GetDeviceFamiliesLimits";
        private const string SP_GET_DOMAIN_IDS_BY_EMAIL             = "sp_GetDomainIDsByEmail";
        private const string SP_GET_DOMAIN_IDS_BY_OPERATOR_COGUID   = "sp_GetDomainIDsByOperatorCoGuid";
        private const string SP_GET_DEVICE_DOMAIN_DATA              = "Get_DeviceDomainData";
        private const string SP_GET_DOMAIN_COGUID                   = "Get_DomainCoGuid";

        //private const string SP_UPDATE_USER_IN_DOMAIN             = "Update_UserInDomain";
        private const string SP_INSERT_USER_TO_DOMAIN               = "sp_InsertUserToDomain";
        private const string SP_INSERT_DEVICE_TO_DOMAIN             = "sp_InsertDeviceToDomain";

        private const string SP_UPDATE_SET_USER_STATUS_IN_DOMAIN    = "Update_SetUserStatusInDomain";
        private const string SP_UPDATE_SET_DEVICE_STATUS_IN_DOMAIN  = "Update_SetDeviceStatusInDomain";
        private const string SP_UPDATE_DOMAIN_DATA                  = "Update_DomainData";

        private const string SP_REMOVE_DOMAIN                       = "sp_RemoveDomain";
        private const string SP_RESET_DOMAIN_FREQUENCY              = "sp_ResetDomainFrequency";

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
                        nGroupID = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "group_id", 0);
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
                            if (selectQuery1.Execute("query", true) != null)
                            {
                                count = selectQuery1.Table("query").DefaultView.Count;
                                if (count > 0)
                                {
                                    //m_domainID = nDomainID;
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

            try
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey("USERS_CONNECTION_STRING");

                selectQuery += "select device_id, device_brand_id from devices WITH (nolock) where status=1 and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("PIN", "=", sPIN);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
                if (selectQuery.Execute("query", true) != null)
                {
                    int nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        sUDID = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "device_id", 0); // selectQuery.Table("query").DefaultView[0].Row["device_id"].ToString();
                        nBrandID = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "device_brand_id", 0); // int.Parse(selectQuery.Table("query").DefaultView[0].Row["device_brand_id"].ToString());
                    }
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

        public static int InsertDeviceToDomain(int nDeviceID, int nDomainID, int nGroupID, int nIsActive, int nStatus, string sActivationToken = "")
        {
            int res = 0;

            try
            {
                ODBCWrapper.StoredProcedure spInsertDeviceToDomain = new ODBCWrapper.StoredProcedure(SP_INSERT_DEVICE_TO_DOMAIN);
                spInsertDeviceToDomain.SetConnectionKey("USERS_CONNECTION_STRING");

                spInsertDeviceToDomain.AddParameter("@deviceID", nDeviceID);
                spInsertDeviceToDomain.AddParameter("@domainID", nDomainID);
                spInsertDeviceToDomain.AddParameter("@groupID", nGroupID);
                spInsertDeviceToDomain.AddParameter("@status", nStatus);
                spInsertDeviceToDomain.AddParameter("@isActive", nIsActive);
                spInsertDeviceToDomain.AddParameter("@activationToken", sActivationToken);

                res = spInsertDeviceToDomain.ExecuteReturnValue<int>();
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return res;
        }

        public static bool UpdateDomainsDevicesStatus(int nDomainsDevicesID, int nIsActive, int nStatus)
        {
            bool res = false;

            try
            {
                ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("domains_devices");
                updateQuery.SetConnectionKey("USERS_CONNECTION_STRING");
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("is_active", "=", nIsActive);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("status", "=", nStatus);
                updateQuery += " where ";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nDomainsDevicesID);
                res = updateQuery.Execute();

                updateQuery.Finish();
                updateQuery = null;
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return res;
        }

        public static int DoesDeviceExistInDomain(int nDomainID, int nGroupID, string deviceUdid, ref int isActive, ref int nDeviceID)
        {
            int nDeviceDomainID = 0;

            try
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey("USERS_CONNECTION_STRING");
                selectQuery += "select dd.id, dd.is_active, dd.device_id from domains_devices dd WITH (nolock), devices d WITH (nolock) where ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("dd.DOMAIN_ID", "=", nDomainID);
                selectQuery += " and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("dd.GROUP_ID", "=", nGroupID);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("d.device_id", "=", deviceUdid);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("d.status", "=", 1);
                selectQuery += " and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("d.group_id", "=", nGroupID);
                selectQuery += " and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("dd.STATUS", "=", 1);
                selectQuery += "and";
                selectQuery += "dd.DEVICE_ID=d.id";
                if (selectQuery.Execute("query", true) != null)
                {
                    int nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        nDeviceDomainID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["id"].ToString());
                        isActive = int.Parse(selectQuery.Table("query").DefaultView[0].Row["is_active"].ToString());
                        nDeviceID = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "device_id", 0);
                    }
                }

                selectQuery.Finish();
                selectQuery = null;
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return nDeviceDomainID;
        }

        public static int GetDeviceDomainData(int nGroupID, string sDeviceUdid, ref int nDeviceID, ref int nIsActive, ref int nStatus, ref int nDbDomainDeviceID)
        {
            int nDomainID = 0;

            try 
	        {
                ODBCWrapper.StoredProcedure spGetDeviceDomainData = new ODBCWrapper.StoredProcedure(SP_GET_DEVICE_DOMAIN_DATA);
                spGetDeviceDomainData.SetConnectionKey("USERS_CONNECTION_STRING");
                spGetDeviceDomainData.AddParameter("@groupID", nGroupID);
                spGetDeviceDomainData.AddParameter("@deviceID", sDeviceUdid);

                DataSet ds = spGetDeviceDomainData.ExecuteDataSet();

                if ((ds != null) && (ds.Tables[0].DefaultView.Count > 0))
                {
                    int nCount = ds.Tables[0].DefaultView.Count;
                    if (nCount > 0)
                    {
                        nDbDomainDeviceID   = int.Parse(ds.Tables[0].DefaultView[0].Row["id"].ToString());
                        nDomainID           = int.Parse(ds.Tables[0].DefaultView[0].Row["domain_id"].ToString());
                        nIsActive           = int.Parse(ds.Tables[0].DefaultView[0].Row["is_active"].ToString());
                        nStatus             = int.Parse(ds.Tables[0].DefaultView[0].Row["status"].ToString());
                        nDeviceID           = ODBCWrapper.Utils.GetIntSafeVal(ds.Tables[0].DefaultView[0].Row, "device_id");

                    }
                }
	        }
	        catch (Exception ex)
	        {
                HandleException(ex);
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
                    //nDbDomainDeviceID   = int.Parse(selectQuery.Table("query").DefaultView[0].Row["id"].ToString());
                    nDomainID           = int.Parse(selectQuery.Table("query").DefaultView[0].Row["domain_id"].ToString());

                    nIsActive           = int.Parse(selectQuery.Table("query").DefaultView[0].Row["is_active"].ToString());
                    nStatus             = int.Parse(selectQuery.Table("query").DefaultView[0].Row["status"].ToString());
                    nDeviceID           = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "device_id", 0);
                }
            }

            selectQuery.Finish();
            selectQuery = null;

            return nDomainID;
        }

        public static bool UpdateDomainsDevicesIsActive(int nDomainDeviceID, int enableInt, bool bIsEnable)
        {
            bool res = false;

            try
            {
                ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("domains_devices");
                updateQuery.SetConnectionKey("USERS_CONNECTION_STRING");
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("is_active", "=", enableInt);
                if (bIsEnable)
                {
                    updateQuery += ", last_activation_date = getdate()";
                }
                updateQuery += " where ";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nDomainDeviceID);

                res = updateQuery.Execute();

                updateQuery.Finish();
                updateQuery = null;
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return res;
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
                //spGetUserExistsInDomain.AddParameter("@status", 1);
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

            try
            {
                ODBCWrapper.StoredProcedure spGetUsersInDomain = new ODBCWrapper.StoredProcedure(SP_GET_USERS_IN_DOMAIN);
                spGetUsersInDomain.SetConnectionKey("USERS_CONNECTION_STRING");

                spGetUsersInDomain.AddParameter("@domainID", nDomainID);
                spGetUsersInDomain.AddParameter("@groupID", nGroupID);
                spGetUsersInDomain.AddParameter("@status", status);
                spGetUsersInDomain.AddParameter("@isActive", isActive);

                DataSet ds = spGetUsersInDomain.ExecuteDataSet();

                if ((ds != null) && (ds.Tables[0].DefaultView.Count > 0))
                {
                    int nCount = ds.Tables[0].DefaultView.Count;

                    //List<int> masterUserIDs = new List<int>();

                    for (int i = 0; i < nCount; i++)
                    {
                        int nUserId     = int.Parse(ds.Tables[0].DefaultView[i].Row["user_id"].ToString());
                        int nUserType   = int.Parse(ds.Tables[0].DefaultView[i].Row["is_master"].ToString());

                        //if (nUserType == 1)
                        //{
                        //    masterUserIDs.Add(nUserId); // masterUserID = nUserId;
                        //}
                        //else
                        //{
                            dTypedUsers[nUserId] = nUserType;
                        //}
                    }

                    // Master User IDs are placed at the head of the list
                    //for (int i = masterUserIDs.Count-1; i >= 0; --i)
                    //{
                    //    dTypedUsers.Insert(0, masterUserID);    
                    //}
                }

                return dTypedUsers;
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return dTypedUsers;

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

            return (dtResult.DefaultView.Count);

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

            return (dtResult.DefaultView.Count);

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
            int retVal = 0;

            try
            {
                ODBCWrapper.StoredProcedure spGetGroupLimits = new ODBCWrapper.StoredProcedure("Get_GroupLimits");
                spGetGroupLimits.SetConnectionKey("MAIN_CONNECTION_STRING");
                spGetGroupLimits.AddParameter("@GroupID", nGroupID);
                DataSet ds = spGetGroupLimits.ExecuteDataSet();

                if ((ds != null) && (ds.Tables[0].DefaultView.Count > 0))
                {
                    int nCount = ds.Tables[0].DefaultView.Count;
                    if (nCount > 0)
                    {
                        DataRow dr = ds.Tables[0].DefaultView[0].Row;

                        retVal                      = ODBCWrapper.Utils.GetIntSafeVal(dr, "LIMIT_ID");
                        defaultDeviceLimit          = ODBCWrapper.Utils.GetIntSafeVal(dr, "DEVICE_MAX_LIMIT");
                        defaultUserLimit            = ODBCWrapper.Utils.GetIntSafeVal(dr, "USER_MAX_LIMIT");
                        defaultConcurrentLimit      = ODBCWrapper.Utils.GetIntSafeVal(dr, "CONCURRENT_MAX_LIMIT");
                        defaultGroupConcurrentLimit = ODBCWrapper.Utils.GetIntSafeVal(dr, "GROUP_CONCURRENT_MAX_LIMIT");
                    }
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return retVal;
        }
              
        public static bool InsertNewDomain(string sName, string sDescription, int nGroupID, DateTime dDateTime, int nDomainLimitID, string sCoGuid = null, int? nOperatorID = null)
        {
            bool bInserRes = false;

            try
            {
                //Insert New Domain to DB
                ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("domains");
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
                insertQuery.Finish();
                insertQuery = null;
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return bInserRes;
        }


        //public static bool GetDomainDbObject(int nGroupID, int nMasterGuID, DateTime dDateTime, //int nDeviceLimit, int nUserLimit, int nConcurrentLimit, int nDomainLimitID,
        public static bool GetDomainDbObject(int nGroupID, DateTime dDateTime,
                                            ref string sName, ref string sDbDescription, ref int nDbDomainID, ref int nDbIsActive, ref int nDbStatus, ref string sCoGuid)
        {
            bool res = false;

            try
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey("USERS_CONNECTION_STRING");

                selectQuery += "SELECT * FROM DOMAINS WITH (NOLOCK) WHERE ";
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
                        //domainObj.m_nGroupID = nGroupID;
                        sName           = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "name", 0);
                        sDbDescription  = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "description", 0);
                        nDbDomainID     = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "id", 0);
                        nDbIsActive     = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "is_active", 0);
                        nDbStatus       = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "status", 0);
                        sCoGuid         = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "CoGuid", 0);

                        res             = true;
                    }
                }

                selectQuery.Finish();
                selectQuery = null;

            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return res;
        }

        public static bool ResetDomain(int nDomainID, int nGroupID)
        {
            try
            {

                ODBCWrapper.StoredProcedure spResetDomainFrequency = new ODBCWrapper.StoredProcedure(SP_RESET_DOMAIN_FREQUENCY);
                spResetDomainFrequency.SetConnectionKey("USERS_CONNECTION_STRING");

                spResetDomainFrequency.AddParameter("@domainID", nDomainID);
                spResetDomainFrequency.AddParameter("@groupID", nGroupID);
                spResetDomainFrequency.AddParameter("@status", 2);
                spResetDomainFrequency.AddParameter("@isActive", 2);

                DataTable dtResult = spResetDomainFrequency.Execute();

                if (dtResult == null)
                {
                    return true;
                }

                return (dtResult.DefaultView.Count > 0);


                //ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("domains");
                //updateQuery.SetConnectionKey("USERS_CONNECTION_STRING");
                //updateQuery += ODBCWrapper.Parameter.NEW_PARAM("Frequency_flag", "=", 0);
                //updateQuery += ODBCWrapper.Parameter.NEW_PARAM("frequency_last_action", "=", string.Empty);
                //updateQuery += "WHERE";
                //updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nDomainID);
                //updateQuery += "AND";
                //updateQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
                //if (!updateQuery.Execute())
                //{
                //    updateQuery.Finish();
                //    return false;
                //}

                //updateQuery.Finish();

                //updateQuery = new ODBCWrapper.UpdateQuery("domains_devices");
                //updateQuery.SetConnectionKey("USERS_CONNECTION_STRING");
                //updateQuery += ODBCWrapper.Parameter.NEW_PARAM("status", "=", 2);
                //updateQuery += ODBCWrapper.Parameter.NEW_PARAM("is_Active", "=", 2);
                //updateQuery += "WHERE";
                //updateQuery += ODBCWrapper.Parameter.NEW_PARAM("domain_id", "=", nDomainID);
                //updateQuery += "AND";
                //updateQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);

                //if (!updateQuery.Execute())
                //{
                //    updateQuery.Finish();
                //    return false;
                //}
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return true;
        }

        public static bool SetDomainFlag(int domainId, int val, DateTime dt)
        {
            bool res = false;

            try
            {
                ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("domains");
                updateQuery.SetConnectionKey("USERS_CONNECTION_STRING");
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("Frequency_flag", "=", val);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("Frequency_last_action", "=", dt);
                updateQuery += " WHERE ";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", domainId);

                res = updateQuery.Execute();
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return res;
        }

        public static List<int> GetDeviceDomains(int deviceID, int groupID)
        {
            List<int> domainIDs = null;

            try
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey("USERS_CONNECTION_STRING");
                selectQuery += " select dd.domain_id from domains_devices dd WITH (nolock), domains d WITH (nolock) where d.id = dd.domain_id and dd.status = 1 and d.is_Active = 1 and d.status = 1 and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("dd.device_id", "=", deviceID);
                selectQuery += " and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("d.group_id", "=", groupID);
                selectQuery += " and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("dd.group_id", "=", groupID);
                if (selectQuery.Execute("query", true) != null)
                {
                    int count = selectQuery.Table("query").DefaultView.Count;
                    if (count > 0)
                    {
                        for (int i = 0; i < count; i++)
                        {
                            int domainID = int.Parse(selectQuery.Table("query").DefaultView[i].Row["domain_id"].ToString());

                            //Domain domain = new Domain();
                            //domain.Initialize(groupID, domainID);
                            if (domainIDs == null)
                            {
                                domainIDs = new List<int>();
                            }

                            domainIDs.Add(domainID);
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

        public static List<string[]> InitializeDeviceFamilies(int nDomainLimitID, int nGroupID)
        {
            List<string[]> dbDeviceFamilies = new List<string[]>();

            try
            {
                ODBCWrapper.StoredProcedure spGetDeviceFamiliesLimits = new ODBCWrapper.StoredProcedure(SP_GET_DEVICE_FAMILIES_LIMITS);
                spGetDeviceFamiliesLimits.SetConnectionKey("MAIN_CONNECTION_STRING");

                spGetDeviceFamiliesLimits.AddParameter("@groupID", nGroupID);
                spGetDeviceFamiliesLimits.AddParameter("@domainLimitID", nDomainLimitID);
                DataSet ds = spGetDeviceFamiliesLimits.ExecuteDataSet();

                if ((ds == null) || (ds.Tables.Count == 0) || (ds.Tables[0].DefaultView.Count == 0))
                {
                    return dbDeviceFamilies;
                }

                int nCount = ds.Tables[0].DefaultView.Count;

                for (int i = 0; i < nCount; i++)
                {
                    DataRow dr = ds.Tables[0].DefaultView[i].Row;

                    if (dr == null)
                    {
                        break;
                    }

                    string sFamilyID = dr["ID"].ToString();
                    string sFamilyLimit = ODBCWrapper.Utils.GetSafeStr(dr["MAX_LIMIT"]);
                    string sFamilyConcurrentLimit = ODBCWrapper.Utils.GetSafeStr(dr["MAX_CONCURRENT_LIMIT"]);
                    string sFamilyName = ODBCWrapper.Utils.GetSafeStr(dr["NAME"]);
                    string[] dbDeviceContainer = new string[] { sFamilyID, sFamilyLimit, sFamilyConcurrentLimit, sFamilyName };
                    dbDeviceFamilies.Add(dbDeviceContainer);
                }

                spGetDeviceFamiliesLimits = null;

            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return dbDeviceFamilies;

        }

        public static bool UpdateDomain(string sName, string sDescription, int nDomainID, int nGroupID, int nDomainRestriciton = 0)
        {
            bool res = false;

            try
            {
                ODBCWrapper.StoredProcedure spUpdateDomain = new ODBCWrapper.StoredProcedure(SP_UPDATE_DOMAIN_DATA);
                spUpdateDomain.SetConnectionKey("USERS_CONNECTION_STRING");

                spUpdateDomain.AddParameter("@domainID", nDomainID);
                spUpdateDomain.AddParameter("@groupID", nGroupID);
                spUpdateDomain.AddParameter("@name", sName);
                spUpdateDomain.AddParameter("@description", sDescription);
                spUpdateDomain.AddParameter("@restriction", nDomainRestriciton);

                int rowCount = spUpdateDomain.ExecuteReturnValue<int>();
                res = (rowCount > 0);


                //ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("Domains");
                //updateQuery.SetConnectionKey("USERS_CONNECTION_STRING");
                //updateQuery += ODBCWrapper.Parameter.NEW_PARAM("Name", "=", sName);
                //updateQuery += ODBCWrapper.Parameter.NEW_PARAM("Description", "=", sDescription);
                //updateQuery += "where";
                //updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nDomainID);
                //updateQuery += "and";
                //updateQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);

                //res = updateQuery.Execute();    //m_DomainStatus = DomainStatus.Error;

                //updateQuery.Finish();
                //updateQuery = null;
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

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


        public static bool GetDomainSettings(int nDomainID,
                                            int nGroupID,
                                            ref string sName,
                                            ref string sDescription,
                                            ref int nDeviceLimitationModule,
                                            ref int nDeviceLimit,
                                            ref int nUserLimit,
                                            ref int nConcurrentLimit,
                                            ref int nStatus,
                                            ref int nIsActive,
                                            ref int nFrequencyFlag,
                                            ref int nDeviceMinPeriodId,
                                            ref int nUserMinPeriodId,
                                            ref DateTime dFrequencyLastAction,
                                            ref string sCoGuid,
                                            ref int nDomainRestriction)
        {

            bool res = false;

            try
            {
                ODBCWrapper.StoredProcedure spGetDomainSettings = new ODBCWrapper.StoredProcedure(SP_GET_DOMAIN_SETTINGS);
                spGetDomainSettings.SetConnectionKey("USERS_CONNECTION_STRING");

                spGetDomainSettings.AddParameter("@domainID", nDomainID);
                spGetDomainSettings.AddNullableParameter<long?>("@groupID", nGroupID);
                DataSet ds = spGetDomainSettings.ExecuteDataSet();

                if ((ds == null) || (ds.Tables.Count == 0) || (ds.Tables[0].DefaultView.Count == 0))
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

                    //int nDomainID = int.Parse(ds.Tables[0].DefaultView[i].Row["DOMAIN_ID"].ToString());


                    dFrequencyLastAction    = ODBCWrapper.Utils.GetDateSafeVal(dr["FREQUENCY_LAST_ACTION"]); // ODBCWrapper.Utils.GetDateSafeVal(selectQuery, "Frequency_last_action", 0);

                    sName                   = ODBCWrapper.Utils.GetSafeStr(dr, "NAME");
                    sDescription            = ODBCWrapper.Utils.GetSafeStr(dr, "DESCRIPTION");
                    nDeviceLimitationModule = ODBCWrapper.Utils.GetIntSafeVal(dr, "MODULE_ID");
                    nDeviceLimit            = ODBCWrapper.Utils.GetIntSafeVal(dr, "MAX_LIMIT");
                    nUserLimit              = ODBCWrapper.Utils.GetIntSafeVal(dr, "USER_MAX_LIMIT");
                    nConcurrentLimit        = ODBCWrapper.Utils.GetIntSafeVal(dr, "CONCURRENT_MAX_LIMIT");
                    nStatus                 = ODBCWrapper.Utils.GetIntSafeVal(dr, "STATUS");
                    nIsActive               = ODBCWrapper.Utils.GetIntSafeVal(dr, "IS_ACTIVE");
                    nFrequencyFlag          = ODBCWrapper.Utils.GetIntSafeVal(dr, "FREQUENCY_FLAG");
                    nDeviceMinPeriodId      = GetGroupDeviceMinPeriodId(nGroupID);
                    nUserMinPeriodId        = GetGroupUserMinPeriodId(nGroupID);
                    sCoGuid                 = ODBCWrapper.Utils.GetSafeStr(dr,"COGUID");
                    dFrequencyLastAction    = ODBCWrapper.Utils.GetDateSafeVal(dr, "FREQUENCY_LAST_ACTION");
                    nDomainRestriction      = ODBCWrapper.Utils.GetIntSafeVal(dr, "RESTRICTION");

                    res = true;

                }

                spGetDomainSettings = null;

            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return res;
        }

        private static int GetGroupUserMinPeriodId(int nGroupID)
        {
            int nMinPeriod = 0;

            try
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
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

        public static int GetDomainsDevicesCount(int m_nGroupID, int nDeviceID)
        {
            int nCount = 0;

            try
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey("USERS_CONNECTION_STRING");
                selectQuery += "select ID from domains_devices WITH (nolock) where status=1 and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", m_nGroupID);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("device_id", "=", nDeviceID);
                if (selectQuery.Execute("query", true) != null)
                {
                    nCount = selectQuery.Table("query").DefaultView.Count;
                }
                selectQuery.Finish();
                selectQuery = null;
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return nCount;
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



        public static int GetDomainIDByCoGuid(string coGuid)
        {
            int nDomainID = 0;

            try
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey("USERS_CONNECTION_STRING");
                selectQuery += "select ID from domains WITH (nolock) where Status = 1 and Is_Active = 1 and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("CoGuid", "=", coGuid);
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
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ACTIVATION_TOKEN", "=", sNewToken);
                updateQuery += " where ";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
                updateQuery += " and ";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nUsersDomainID);
                updateQuery += " and ";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ACTIVATION_TOKEN", "=", sToken);
                //updateQuery += " and status=1";

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

        public static int GetDomainIDBySiteGuid(int nGroupID, int nSiteGuid, ref int nOperatorID, ref bool bIsDomainMaster)
        {
            int nDomainID = 0;

            try
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey("USERS_CONNECTION_STRING");
                                
                selectQuery += "SELECT UD.DOMAIN_ID, UD.IS_MASTER, D.OPERATOR_ID FROM USERS_DOMAINS UD WITH (NOLOCK) INNER JOIN DOMAINS D WITH (NOLOCK) ON UD.DOMAIN_ID = D.ID WHERE UD.STATUS = 1 AND D.IS_ACTIVE = 1 AND D.STATUS = 1 AND";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("UD.GROUP_ID", "=", nGroupID);
                selectQuery += " AND ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("UD.USER_ID", "=", nSiteGuid);
                if (selectQuery.Execute("query", true) != null)
                {
                    int count = selectQuery.Table("query").DefaultView.Count;
                    if (count > 0)
                    {
                        nDomainID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["DOMAIN_ID"].ToString());

                        if (!string.IsNullOrEmpty(selectQuery.Table("query").DefaultView[0].Row["OPERATOR_ID"].ToString()))
                        {
                            nOperatorID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["OPERATOR_ID"].ToString());
                        }

                        if (selectQuery.Table("query").DefaultView[0].Row["IS_MASTER"] != System.DBNull.Value && selectQuery.Table("query").DefaultView[0].Row["IS_MASTER"] != null)
                        {
                            bIsDomainMaster = (selectQuery.Table("query").DefaultView[0].Row["IS_MASTER"].ToString() == "1");
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

            return nDomainID;

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
            //bool res = false;
            int status = (-1);

            try
            {
                ODBCWrapper.StoredProcedure spRemoveDomain = new ODBCWrapper.StoredProcedure(SP_REMOVE_DOMAIN);
                spRemoveDomain.SetConnectionKey("USERS_CONNECTION_STRING");

                spRemoveDomain.AddParameter("@DomainID", nDomainID);
                spRemoveDomain.AddParameter("@GroupID", nGroupID);
                spRemoveDomain.AddParameter("@Status", nStatus);
                spRemoveDomain.AddParameter("@IsActive", nIsActive);

                status = spRemoveDomain.ExecuteReturnValue<int>();
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

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
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("ACTIVATION_TOKEN", "=", sToken);
                //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("DOMAIN_ID", "=", nDomainID);

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
    }
}
