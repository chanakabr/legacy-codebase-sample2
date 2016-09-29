using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using ODBCWrapper;
using Tvinci.Core.DAL;


namespace DAL
{
    public class UsersDal : BaseDal
    {
        #region Private Constants

        private const string SP_GET_USER_TYPE_DATA = "Get_UserTypeData";
        private const string SP_GET_USER_TYPE_DATA_BY_IDS = "Get_UserTypesDataByIDs";
        private const string SP_GET_USER_BASIC_DATA = "Get_UserBasicData";
        private const string SP_GET_USER_DOMAINS = "sp_GetUserDomains";
        private const string SP_INSERT_USER = "sp_InsertUser";
        private const string SP_GET_IS_ACTIVATION_NEEDED = "Get_IsActivationNeeded";
        private const string SP_GET_ACTIVATION_TOKEN = "Get_ActivationToken";
        private const string SP_GET_USERS_BASIC_DATA = "Get_UsersBasicData";
        private const string SP_GET_GROUP_USERS = "Get_GroupUsers";
        private const string SP_GET_GROUP_USERS_SEARCH_FIELDS = "Get_GroupUsersSearchFields";
        private const string SP_GET_DEVICES_TO_USERS_NON_PUSH = "Get_DevicesToUsersNonPushAction";
        private const string SP_GET_DEVICES_TO_USERS_PUSH = "Get_DevicesToUsersPushAction";
        private const string SP_GENERATE_TOKEN = "GenerateToken";
        private const string SP_GET_USER_TYPE = "Get_UserType";
        private const string SP_GET_DEFUALT_GROUP_OPERATOR = "Get_DefaultGroupOperator";
        #endregion


        /*
         * IsUseModifiedSP == true calls SP: Get_DevicesToUsersPushAction
         * IsUseModifiedSP == false calls SP: Get_DevicesToUsersNonPushAction
         */
        public static DataTable GetDevicesToUsers(long? lGroupID, long? lUserID, bool bIsUseModifiedSP)
        {
            try
            {
                ODBCWrapper.StoredProcedure spGetDevicesToUsers = new ODBCWrapper.StoredProcedure(bIsUseModifiedSP ? SP_GET_DEVICES_TO_USERS_PUSH : SP_GET_DEVICES_TO_USERS_NON_PUSH);
                spGetDevicesToUsers.SetConnectionKey("USERS_CONNECTION_STRING");

                spGetDevicesToUsers.AddParameter("@groupID", lGroupID);
                spGetDevicesToUsers.AddNullableParameter<long?>("@userID", lUserID);
                DataSet ds = spGetDevicesToUsers.ExecuteDataSet();

                if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
                {
                    return ds.Tables[0];
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return null;

        }

        public static DataTable GetDevicesToUsers(long? lGroupID, long? lUserID)
        {
            return GetDevicesToUsers(lGroupID, lUserID, false);
        }

        public static DataTable GetUserTypeData(long groupID, int? isDefault)
        {
            try
            {
                ODBCWrapper.StoredProcedure spGetUserTypeData = new ODBCWrapper.StoredProcedure(SP_GET_USER_TYPE_DATA);
                spGetUserTypeData.SetConnectionKey("USERS_CONNECTION_STRING");

                spGetUserTypeData.AddParameter("@groupID", groupID);
                spGetUserTypeData.AddNullableParameter<int?>("@is_default", isDefault);
                DataSet ds = spGetUserTypeData.ExecuteDataSet();

                if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
                {
                    return ds.Tables[0];
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return null;
        }

        public static DataTable GetUserTypeDataByIDs(long groupID, List<int> userTypesIDs)
        {
            try
            {
                ODBCWrapper.StoredProcedure spGetUserTypeDataByIDs = new ODBCWrapper.StoredProcedure(SP_GET_USER_TYPE_DATA_BY_IDS);
                spGetUserTypeDataByIDs.SetConnectionKey("USERS_CONNECTION_STRING");

                spGetUserTypeDataByIDs.AddParameter("@groupID", groupID);
                spGetUserTypeDataByIDs.AddIDListParameter("@userTypesIDs", userTypesIDs, "id");
                DataSet ds = spGetUserTypeDataByIDs.ExecuteDataSet();

                if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
                {
                    return (ds.Tables[0]);
                }

            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return null;
        }

        public static bool UpdateUserTypeByUserID(int nUserID, int nUserTypeID)
        {
            bool updateRes = false;
            ODBCWrapper.DirectQuery directQuery = null;
            try
            {
                directQuery = new ODBCWrapper.DirectQuery();
                directQuery.SetConnectionKey("USERS_CONNECTION_STRING");
                directQuery += "update users set ";
                directQuery += "User_Type = " + nUserTypeID;
                directQuery += " where ";
                directQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nUserID);
                updateRes = directQuery.Execute();
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
            finally
            {
                if (directQuery != null)
                {
                    directQuery.Finish();
                }
            }

            return updateRes;
        }

        public static DataTable GetUserBasicData(long userID, int groupID = 0)
        {
            try
            {
                ODBCWrapper.StoredProcedure spGetUserBasicData = new ODBCWrapper.StoredProcedure(SP_GET_USER_BASIC_DATA);
                spGetUserBasicData.SetConnectionKey("USERS_CONNECTION_STRING");

                spGetUserBasicData.AddParameter("@userID", userID);
                if (groupID > 0)
                {
                    spGetUserBasicData.AddParameter("@groupID", groupID);
                }
                DataSet ds = spGetUserBasicData.ExecuteDataSet();

                if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
                {
                    return ds.Tables[0];
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return null;
        }

        public static DataTable GetUsersBasicData(long[] usersIDs)
        {
            ODBCWrapper.StoredProcedure spGetUserBasicData = new ODBCWrapper.StoredProcedure(SP_GET_USERS_BASIC_DATA);
            spGetUserBasicData.SetConnectionKey("USERS_CONNECTION_STRING");

            spGetUserBasicData.AddIDListParameter("@usersIDs", usersIDs.ToList(), "Id");

            DataSet ds = spGetUserBasicData.ExecuteDataSet();

            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
            {
                return ds.Tables[0];
            }
            return null;
        }

        public static DataTable GetGroupUsers(long groupID, string[] fields)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure(SP_GET_GROUP_USERS);
            sp.SetConnectionKey("USERS_CONNECTION_STRING");

            sp.AddParameter("@groupID", groupID);
            sp.AddParameter("@ColumnList", string.Join(",", fields));

            DataSet ds = sp.ExecuteDataSet();

            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
            {
                return ds.Tables[0];
            }
            return null;
        }

        public static DataTable GetGroupUsersSearchFields(long groupID)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure(SP_GET_GROUP_USERS_SEARCH_FIELDS);
            sp.SetConnectionKey("USERS_CONNECTION_STRING");

            sp.AddParameter("@groupID", groupID);

            DataSet ds = sp.ExecuteDataSet();

            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
            {
                return ds.Tables[0];
            }

            return null;
        }

        public static string Get_SiteGuid(string sUID)
        {
            string sSiteGuid = string.Empty;


            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_SiteGuid");
            sp.SetConnectionKey("USERS_CONNECTION_STRING");
            sp.AddParameter("@UID", sUID);
            sSiteGuid = sp.ExecuteReturnValue<string>();

            return sSiteGuid;
        }

        /// <summary>
        /// This Method goes to Users Function : Get_UID
        /// and replace the siteGuid into UID (which is the new identity for a userid)
        /// </summary>
        /// <param name="sSiteGuid"></param>
        /// <returns></returns>
        public static string Get_UID(string sSiteGuid)
        {
            string UID = string.Empty;


            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_UID");
            sp.SetConnectionKey("USERS_CONNECTION_STRING");
            sp.AddParameter("@SiteGuid", sSiteGuid);

            UID = sp.ExecuteReturnValue<string>();



            return UID;
        }

        public static int InsertUser(string sUserName,
                                    string sPassword,
                                    string sSalt,
                                    string sFirstName,
                                    string sLastName,
                                    string sFacebookID,
                                    string sFacebookImage,
                                    string sFacebookToken,
                                    int nIsFacebookImagePermitted,
                                    string sEmail,
                                    int nActivateStatus,
                                    string sActivationToken,
                                    string sCoGuid,
                                    string sExternalToken,
                                    int? nUserTypeID,
                                    int nGroupID)
        {
            int nInserted = 0;

            try
            {

                ODBCWrapper.StoredProcedure spInsertUser = new ODBCWrapper.StoredProcedure(SP_INSERT_USER);
                spInsertUser.SetConnectionKey("USERS_CONNECTION_STRING");

                spInsertUser.AddParameter("@username", sUserName);
                spInsertUser.AddParameter("@password", sPassword);
                spInsertUser.AddParameter("@salt", sSalt);
                spInsertUser.AddParameter("@firstName", sFirstName);
                spInsertUser.AddParameter("@lastName", sLastName);
                spInsertUser.AddParameter("@facebookID", sFacebookID);
                spInsertUser.AddParameter("@facebookImage", sFacebookImage);
                spInsertUser.AddParameter("@facebookToken", sFacebookToken);
                spInsertUser.AddParameter("@isFacebookImagePermitted", nIsFacebookImagePermitted);
                spInsertUser.AddParameter("@email", sEmail);
                spInsertUser.AddParameter("@activateStatus", nActivateStatus);
                spInsertUser.AddParameter("@activationToken", sActivationToken);

                if (!string.IsNullOrEmpty(sCoGuid))
                {
                    spInsertUser.AddParameter("@coGuid", sCoGuid);
                }
                if (!string.IsNullOrEmpty(sExternalToken))
                {
                    spInsertUser.AddParameter("@externalToken", sExternalToken);
                }
                if (nUserTypeID != null)
                {
                    spInsertUser.AddParameter("@userTypeID", nUserTypeID.Value);
                }

                spInsertUser.AddParameter("@groupID", nGroupID);

                int retVal = spInsertUser.ExecuteReturnValue<int>();

                return retVal;
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return nInserted;
        }

        public static bool UpdateHitDate(int nSiteGuid, bool bLogOut = false)
        {
            bool res = false;
            ODBCWrapper.DirectQuery directQuery = null;
            try
            {
                string sLastHitDate = bLogOut ? "DATEADD(minute , -1 , getdate())" : "getdate()";

                directQuery = new ODBCWrapper.DirectQuery();
                directQuery.SetConnectionKey("USERS_CONNECTION_STRING");

                directQuery += "update users set ";
                directQuery += "LAST_HIT_DATE=" + sLastHitDate;
                directQuery += " where ";
                directQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nSiteGuid);
                res = directQuery.Execute();
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
            finally
            {
                if (directQuery != null)
                {
                    directQuery.Finish();
                }
            }

            return res;
        }

        public static int GetUserIDByUsername(string sUsername, int nGroupID)
        {
            int nUserID = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = null;
            try
            {
                selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey("USERS_CONNECTION_STRING");

                selectQuery += "select id from users WITH (nolock) where is_active=1 and status=1 and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("USERNAME", "=", sUsername);
                selectQuery += " and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        nUserID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
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
                }
            }

            return nUserID;
        }

        public static int GetUserIDByFacebookID(string sFacebookID, int nGroupID)
        {
            int nUserID = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = null;
            try
            {
                selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select id from users WITH (nolock) where is_active=1 and status=1 and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("FACEBOOK_ID", "=", sFacebookID);
                selectQuery += " and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        nUserID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
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
                }
            }

            return nUserID;
        }

        public static int GetUserDomainID(string sSiteGUID)
        {
            int nOperatorID = 0;
            bool bIsDomainMaster = false;
            DomainSuspentionStatus eSuspendStat = DomainSuspentionStatus.OK;
            int nDomainID = GetUserDomainID(sSiteGUID, ref nOperatorID, ref bIsDomainMaster, ref eSuspendStat);
            return nDomainID;
        }

        public static int GetUserDomainID(string sSiteGUID, ref int nOperatorID, ref bool bIsDomainMaster, ref DomainSuspentionStatus eSuspendStatus)
        {
            int nDomainID = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = null;
            try
            {
                selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey("USERS_CONNECTION_STRING");

                selectQuery += "SELECT UD.DOMAIN_ID, UD.IS_MASTER, D.OPERATOR_ID, D.IS_SUSPENDED FROM USERS_DOMAINS UD WITH (NOLOCK), DOMAINS D WITH (NOLOCK) WHERE UD.DOMAIN_ID=D.ID AND UD.STATUS<>2 AND D.STATUS<>2 AND";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("UD.USER_ID", "=", sSiteGUID);
                if (selectQuery.Execute("query", true) != null)
                {
                    int count = selectQuery.Table("query").DefaultView.Count;
                    if (count > 0)
                    {
                        nDomainID = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "domain_id", 0);
                        
                        nOperatorID = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "operator_id", 0);

                        bIsDomainMaster = (ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "is_master", 0) == "1");                        

                        int suspendInt = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "IS_SUSPENDED", 0);
                        if (Enum.IsDefined(typeof(DomainSuspentionStatus), suspendInt))
                        {
                            eSuspendStatus = (DomainSuspentionStatus)suspendInt;
                        }
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

            return nDomainID;
        }

        public static List<int> GetUserDomainIDs(int nGroupID, int nUserID)
        {
            List<int> lDomainIDs = null;

            ODBCWrapper.StoredProcedure spGetUserDomains = new ODBCWrapper.StoredProcedure(SP_GET_USER_DOMAINS);
            spGetUserDomains.SetConnectionKey("USERS_CONNECTION_STRING");

            spGetUserDomains.AddParameter("@groupID", nGroupID);
            spGetUserDomains.AddNullableParameter<long?>("@userID", nUserID);
            DataSet ds = spGetUserDomains.ExecuteDataSet();

            if (ds == null || ds.Tables.Count == 0 || ds.Tables[0].DefaultView.Count == 0)
            {
                return new List<int>(0);
            }

            int nCount = ds.Tables[0].DefaultView.Count;
            lDomainIDs = new List<int>(nCount);
            
            for (int i = 0; i < nCount; i++)
            {
                int tempDomainID = 0;
                if (Int32.TryParse(ds.Tables[0].DefaultView[i].Row["DOMAIN_ID"].ToString(), out tempDomainID) && tempDomainID > 0)
                {
                    lDomainIDs.Add(tempDomainID);
                }
            }
            return lDomainIDs;
        }

        public static int GetAllowedLogins(int nGroupID)
        {
            int nAllowedLogins = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = null;
            try
            {
                selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey("USERS_CONNECTION_STRING");

                selectQuery += " select allowed_logins from groups_parameters WITH (nolock) where ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
                if (selectQuery.Execute("query", true) != null)
                {
                    int count = selectQuery.Table("query").DefaultView.Count;
                    if (count > 0)
                    {
                        object oLimit = selectQuery.Table("query").DefaultView[0].Row["allowed_logins"];
                        if (oLimit != System.DBNull.Value && oLimit != null)
                        {
                            nAllowedLogins = int.Parse(oLimit.ToString());
                        }
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
                }
            }

            return nAllowedLogins;
        }

        public static bool UpdateFailCount(int nUserID, int nAdd)
        {
            bool updateRes = false;
            ODBCWrapper.DirectQuery directQuery = null;
            try
            {
                directQuery = new ODBCWrapper.DirectQuery();
                directQuery.SetConnectionKey("USERS_CONNECTION_STRING");

                directQuery += "update users set ";
                if (nAdd > 0)
                {
                    directQuery += "FAIL_COUNT=FAIL_COUNT+" + nAdd.ToString();
                    directQuery += ",LAST_FAIL_DATE=getdate()";
                }
                else
                {
                    directQuery += "FAIL_COUNT=0 ";
                }
                directQuery += " where ";
                directQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nUserID);
                updateRes = directQuery.Execute();
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
            finally
            {
                if (directQuery != null)
                {
                    directQuery.Finish();
                }
            }

            return updateRes;
        }

        public static bool UpdateUserSession(int id, int statusInt)
        {
            bool updateRes = false;
            ODBCWrapper.UpdateQuery updateQuery = null;
            try
            {
                updateQuery = new ODBCWrapper.UpdateQuery("users_sessions");
                updateQuery.SetConnectionKey("USERS_CONNECTION_STRING");

                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("is_active", "=", statusInt);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("status", "=", statusInt);
                DateTime dtToWriteToDB = DateTime.UtcNow;
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("update_date", "=", dtToWriteToDB);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("last_action_date", "=", dtToWriteToDB);
                updateQuery += " where ";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", id);
                updateRes = updateQuery.Execute();

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
            return updateRes;

        }

        public static int Update_ActivenessForUserSessionByDeviceIDAndReturnID(long lSiteGuid, string sIDInDevices, bool bSetIsActive)
        {
            long res = 0;
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Update_ActivenessForUserSessionByDeviceID");
            sp.SetConnectionKey("USERS_CONNECTION_STRING");
            sp.AddParameter("@SiteGuid", lSiteGuid);
            sp.AddParameter("@DeviceID", sIDInDevices);
            int nIsActiveAndStatus = bSetIsActive ? 1 : 0;
            sp.AddParameter("@IsActive", nIsActiveAndStatus);
            sp.AddParameter("@Status", nIsActiveAndStatus);
            DateTime dtToWriteToDB = DateTime.UtcNow;
            sp.AddParameter("@UpdateDate", dtToWriteToDB);
            sp.AddParameter("@LastActionDate", dtToWriteToDB);
            sp.ExecuteNonQuery();

            Get_UserSessionByDeviceID(lSiteGuid, sIDInDevices, ref bSetIsActive, ref res);

            return (int)res;

        }

        public static int Update_UserActivenessForUserSessionBySessionIDAndIPAndReturnID(long lSiteGuid, string sSessionID, string sIP, bool bSetIsActive)
        {
            long res = 0;
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Update_ActivenessForUserSessionBySessionIDAndIP");
            sp.SetConnectionKey("USERS_CONNECTION_STRING");
            sp.AddParameter("@SiteGuid", lSiteGuid);
            sp.AddParameter("@SessionID", sSessionID);
            sp.AddParameter("@UserIP", sIP);
            int nIsActiveAndStatus = bSetIsActive ? 1 : 0;
            sp.AddParameter("@IsActive", nIsActiveAndStatus);
            sp.AddParameter("@Status", nIsActiveAndStatus);
            DateTime dtToWriteToDB = DateTime.UtcNow;
            sp.AddParameter("@UpdateDate", dtToWriteToDB);
            sp.AddParameter("@LastActionDate", dtToWriteToDB);
            sp.ExecuteNonQuery();

            Get_UserSessionDetailsBySessionIDAndIP(lSiteGuid, sSessionID, sIP, ref bSetIsActive, ref res);

            return (int)res;
        }



        public static DateTime GetLastUserSessionDate(int nSiteGuid, ref int userSessionID, ref string userSession, ref string lastUserIP, ref DateTime dbNow)
        {
            DateTime retVal = DateTime.MaxValue;
            ODBCWrapper.DataSetSelectQuery selectQuery = null;
            try
            {
                selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey("USERS_CONNECTION_STRING");

                selectQuery += "select id,user_ip, last_action_date, session_id, getdate() as 'Now' from users_sessions WITH (nolock) where is_active = 1 and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("user_site_guid", "=", nSiteGuid);
                //selectQuery += " and ";
                //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("user_ip", "=", sIP);
                selectQuery += " order by last_action_date desc";

                if (selectQuery.Execute("query", true) != null)
                {
                    int count = selectQuery.Table("query").DefaultView.Count;
                    if (count > 0)
                    {
                        userSessionID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["id"].ToString());
                        userSession = selectQuery.Table("query").DefaultView[0].Row["session_id"].ToString();
                        lastUserIP = selectQuery.Table("query").DefaultView[0].Row["user_ip"].ToString();
                        dbNow = (DateTime)selectQuery.Table("query").DefaultView[0].Row["Now"];

                        if (selectQuery.Table("query").DefaultView[0].Row["last_action_date"] != System.DBNull.Value && selectQuery.Table("query").DefaultView[0].Row["last_action_date"] != null)
                        {
                            retVal = (DateTime)(selectQuery.Table("query").DefaultView[0].Row["last_action_date"]);
                        }
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
                }
            }

            return retVal;
        }

        public static int GetUserPasswordFailHistory(string sUN, int nGroupID, ref DateTime dNow, ref int nFailCount, ref DateTime dLastFailDate, ref DateTime dLastHitDate)
        {
            int nID = 0;
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_LoginFailCount");
            sp.SetConnectionKey("USERS_CONNECTION_STRING");
            sp.AddParameter("@username", sUN);
            sp.AddParameter("@groupID", nGroupID);

            DataSet ds = sp.ExecuteDataSet();

            if (ds != null && ds.Tables[0] != null && ds.Tables[0].Rows != null && ds.Tables[0].Rows.Count > 0)
            {
                DataTable dt = ds.Tables[0];

                if (dt != null && dt.Rows.Count > 0)
                {
                    dNow = ODBCWrapper.Utils.GetDateSafeVal(dt.Rows[0]["dNow"]);
                    nFailCount = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0]["FAIL_COUNT"]);
                    nID = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0]["id"]);
                    dLastFailDate = new DateTime(2020, 1, 1);
                    dLastHitDate = new DateTime(2020, 1, 1);
                    dLastFailDate = ODBCWrapper.Utils.GetDateSafeVal(dt.Rows[0]["LAST_FAIL_DATE"]);
                    dLastHitDate = ODBCWrapper.Utils.GetDateSafeVal(dt.Rows[0]["LAST_HIT_DATE"]);
                }
            }

            return nID;
        }

        public static bool InsertUserOperator(string nSiteGuid, string sCoGuid, int nOperatorID)
        {
            bool res = false;

            try
            {
                ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("users_operators");
                insertQuery.SetConnectionKey("USERS_CONNECTION_STRING");

                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("user_site_guid", "=", nSiteGuid);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("co_guid", "=", sCoGuid);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("operator_id", "=", nOperatorID);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("is_active", "=", 1);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("status", "=", 1);

                res = insertQuery.Execute();
                insertQuery.Finish();
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return res;
        }

        public static string GetActivationToken(int nGroupID, string sUserName)
        {
            string sActivationToken = string.Empty;
            ODBCWrapper.DataSetSelectQuery selectQuery = null;
            try
            {
                selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey("USERS_CONNECTION_STRING");

                selectQuery += "select ACTIVATION_TOKEN from users WITH (nolock) where is_active=1 and status=1 and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("USERNAME", "=", sUserName);
                selectQuery += " and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        sActivationToken = selectQuery.Table("query").DefaultView[0].Row["ACTIVATION_TOKEN"].ToString();
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
                }
            }

            return sActivationToken;
        }

        public static string GetActivationToken(int nGroupID, int nSiteGuid)
        {
            string sActivationToken = string.Empty;
            ODBCWrapper.DataSetSelectQuery selectQuery = null;
            try
            {
                selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey("USERS_CONNECTION_STRING");

                selectQuery += "select ACTIVATION_TOKEN from users WITH (nolock) where ";     //is_active=1 and status=1 and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nSiteGuid);
                selectQuery += " and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        sActivationToken = selectQuery.Table("query").DefaultView[0].Row["ACTIVATION_TOKEN"].ToString();
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
                }
            }

            return sActivationToken;
        }

        public static DataRowView GetGroupMailParameters(int m_nGroupID)
        {
            DataRowView dvMailParameters = null;
            ODBCWrapper.DataSetSelectQuery selectQuery = null;
            try
            {
                selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey("USERS_CONNECTION_STRING");

                selectQuery += "select * from groups_parameters with (nolock) where status=1 and is_active=1 and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", m_nGroupID);
                selectQuery += " order by id desc";
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        dvMailParameters = selectQuery.Table("query").DefaultView[0];
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
                }
            }

            return dvMailParameters;
        }

        public static bool UpdateUserActivationToken(string[] arrGroupIDs, int nUserID, string sToken, string sNewToken, int nUserState)
        {
            bool isActivated = false;

            try
            {
                ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("users");
                updateQuery.SetConnectionKey("USERS_CONNECTION_STRING");

                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ACTIVATE_STATUS", "=", 1);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ACTIVATION_TOKEN", "=", sNewToken);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("user_state", "=", nUserState);
                updateQuery += " where ";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nUserID);
                updateQuery += " and ";
                updateQuery += " group_id in (" + string.Join(",", arrGroupIDs) + ")";
                updateQuery += " and status=1 and is_active=1 and ";
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

        public static bool SetUserSessionStatus(int nUserID, int nIsActive, int nStatus)
        {
            bool res = false;

            try
            {
                ODBCWrapper.UpdateQuery sessionsUpdateQuery = new ODBCWrapper.UpdateQuery("users_sessions");
                sessionsUpdateQuery.SetConnectionKey("USERS_CONNECTION_STRING");

                sessionsUpdateQuery += ODBCWrapper.Parameter.NEW_PARAM("is_active", "=", nIsActive);
                sessionsUpdateQuery += ODBCWrapper.Parameter.NEW_PARAM("status", "=", nStatus);
                sessionsUpdateQuery += " where ";
                sessionsUpdateQuery += ODBCWrapper.Parameter.NEW_PARAM("user_site_guid", "=", nUserID);

                res = sessionsUpdateQuery.Execute();

                sessionsUpdateQuery.Finish();
                sessionsUpdateQuery = null;
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return res;
        }

        public static int GetUserActivateStatus(int nUserID, string[] arrGroupIDs)
        {
            int nActivationStatus = -1;

            try
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey("USERS_CONNECTION_STRING");

                selectQuery += " select ACTIVATE_STATUS from users WITH (nolock) where status=1 and is_active=1 and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nUserID);
                selectQuery += " and ";
                selectQuery += " group_id in (" + string.Join(",", arrGroupIDs) + ")";
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {

                        nActivationStatus = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ACTIVATE_STATUS"].ToString());
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

        public static int GetUserIDByActivationToken(string sToken, string[] arrGroupIDs)
        {
            int nUserID = 0;

            try
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey("USERS_CONNECTION_STRING");

                selectQuery += "select id from users WITH (nolock) where status=1 and is_active=1 and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("ACTIVATION_TOKEN", "=", sToken);
                selectQuery += " and ";
                selectQuery += " group_id in (" + string.Join(",", arrGroupIDs) + ")";
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        nUserID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
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

        public static int GetUserIDByUsername(string sUserName, string[] arrGroupIDs)
        {
            int nUserID = 0;

            try
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey("USERS_CONNECTION_STRING");

                selectQuery += "SELECT ID FROM USERS WITH (NOLOCK) WHERE STATUS=1 AND IS_ACTIVE = 1 AND ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("USERNAME", "=", sUserName);
                selectQuery += " AND ";
                selectQuery += " GROUP_ID IN (" + string.Join(",", arrGroupIDs) + ")";
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        nUserID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
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

        public static bool GetIsActivationNeeded(int nGroupID)
        {
            bool bIsActivationNeeded = true;

            try
            {
                ODBCWrapper.StoredProcedure spGetIsActivationNeeded = new ODBCWrapper.StoredProcedure(SP_GET_IS_ACTIVATION_NEEDED);
                spGetIsActivationNeeded.SetConnectionKey("USERS_CONNECTION_STRING");

                spGetIsActivationNeeded.AddParameter("@groupID", nGroupID);
                DataSet ds = spGetIsActivationNeeded.ExecuteDataSet();

                if (ds != null && ds.Tables.Count > 0)
                {
                    DataTable dt = ds.Tables[0];

                    if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                    {
                        bIsActivationNeeded = (ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0], "IS_ACTIVATION_NEEDED") != 0);
                    }
                }

            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return bIsActivationNeeded;
        }

        public static bool Insert_ItemList(int nSiteGuid, Dictionary<int, List<int>> dItems, int listType, int itemType, int nGroupID)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Insert_ItemList");
            sp.SetConnectionKey("USERS_CONNECTION_STRING");
            sp.AddParameter("@userID", nSiteGuid);
            sp.AddParameter("@listType", listType);
            sp.AddParameter("@itemType", itemType);
            sp.AddKeyValueListParameter<int, int>("@itemIDs", dItems, "Id", "OrderNum");
            sp.AddParameter("@groupID", nGroupID);
            bool result = sp.ExecuteReturnValue<bool>();
            return result;
        }

        public static bool Remove_ItemFromList(int nSiteGuid, Dictionary<int, List<int>> dItems, int listType, int itemType, int nGroupID)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Remove_ItemFromList");
            sp.SetConnectionKey("USERS_CONNECTION_STRING");
            sp.AddParameter("@userID", nSiteGuid);
            sp.AddParameter("@listType", listType);
            sp.AddParameter("@itemType", itemType);
            sp.AddKeyValueListParameter<int, int>("@itemIDs", dItems, "Id", "OrderNum");
            sp.AddParameter("@groupID", nGroupID);
            bool result = sp.ExecuteReturnValue<bool>();
            return result;
        }

        public static bool Update_ItemInList(int nSiteGuid, Dictionary<int, List<int>> dItems, int listType, int itemType, int nGroupID)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Update_ItemInList");
            sp.SetConnectionKey("USERS_CONNECTION_STRING");
            sp.AddParameter("@userID", nSiteGuid);
            sp.AddParameter("@listType", listType);
            sp.AddParameter("@itemType", itemType);
            sp.AddKeyValueListParameter<int, int>("@itemIDs", dItems, "Id", "OrderNum");
            sp.AddParameter("@groupID", nGroupID);
            bool result = sp.ExecuteReturnValue<bool>();
            return result;
        }

        public static DataTable GetItemFromList(int nSiteGuid, int listType, int itemType, int nGroupID)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_ItemFromList");
            sp.SetConnectionKey("USERS_CONNECTION_STRING");
            sp.AddParameter("@userID", nSiteGuid);
            sp.AddParameter("@listType", listType);
            sp.AddParameter("@itemType", itemType);
            sp.AddParameter("@groupID", nGroupID);
            DataSet ds = sp.ExecuteDataSet();
            if (ds != null)
                return ds.Tables[0];
            return null;
        }

        public static DataTable GetItemsFromUsersLists(List<int> userIds, int listType, int itemType, int groupId)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_ItemsFromUsersLists");
            sp.SetConnectionKey("USERS_CONNECTION_STRING");
            sp.AddIDListParameter<int>("@userIDs", userIds, "Id"); 
            sp.AddParameter("@listType", listType);
            sp.AddParameter("@itemType", itemType);
            sp.AddParameter("@groupID", groupId);
            DataSet ds = sp.ExecuteDataSet();
            if (ds != null)
                return ds.Tables[0];
            return null;
        }

        public static DataTable IsItemExists(List<int> lItems, int nGroupID, string siteGuid)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("IsItemExists");
            sp.SetConnectionKey("USERS_CONNECTION_STRING");
            sp.AddIDListParameter<int>("@lItems", lItems, "Id");
            sp.AddParameter("@groupID", nGroupID);
            sp.AddParameter("@siteGuid", siteGuid);

            DataSet ds = sp.ExecuteDataSet();
            if (ds != null)
                return ds.Tables[0];
            return null;
        }

        /// <summary>
        /// GetUserActivationState
        /// </summary>
        /// <param name="arrGroupIDs"></param>
        /// <param name="nActivationMustHours"></param>
        /// <param name="sUserName"></param>
        /// <param name="nUserID"></param>
        /// <param name="nActivateStatus"></param>
        /// <param name="dCreateDate"></param>
        /// <param name="dNow"></param>
        /// <returns>
        ///     -2 - error
        ///     -1 - user does not exist or was removed/deactivated   
        ///      0 - user activated 
        ///      1 - user not activated 
        ///      2 - user not activated by master
        ///      3 - user removed from domain
        ///      
        /// </returns>
        public static DALUserActivationState GetUserActivationState(int nParentGroupID, List<int> lGroupIDs, int nActivationMustHours, ref string sUserName, ref int nUserID, ref int nActivateStatus)
        {
            DALUserActivationState res = DALUserActivationState.Error;

            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_UserState");
                sp.SetConnectionKey("USERS_CONNECTION_STRING");
                sp.AddParameter("@UserName", sUserName);
                sp.AddParameter("@Id", nUserID);
                sp.AddIDListParameter<int>("@GroupsID", lGroupIDs, "Id");                
                sp.AddParameter("@ActivationMustHours", nActivationMustHours);
                int nSPReault = sp.ExecuteReturnValue<int>();
                if (Enum.IsDefined(typeof(DALUserActivationState), nSPReault))
                {
                    res = (DALUserActivationState)nSPReault;
                }

           
                #region OldCode
                // CHECK ACTIVATION STATUS IN USERS

                //ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                //selectQuery.SetConnectionKey("USERS_CONNECTION_STRING");
                //selectQuery.SetCachedSec(0);
                //selectQuery += "SELECT GETDATE() AS DNOW, ID,CREATE_DATE, ACTIVATE_STATUS, USERNAME FROM USERS WITH (NOLOCK) WHERE IS_ACTIVE=1 AND STATUS=1 AND ";

                //if (!string.IsNullOrEmpty(sUserName))
                //{
                //    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("USERNAME", "=", sUserName);
                //}
                //else
                //{
                //    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nUserID);
                //}

                //selectQuery += " AND GROUP_ID IN (" + string.Join(",", arrGroupIDs) + ")";

                //if (selectQuery.Execute("query", true) != null)
                //{
                //    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                //    if (nCount > 0)
                //    {
                //        nUserID = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "ID", 0);
                //        sUserName = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "USERNAME", 0);

                //        nActivateStatus = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "ACTIVATE_STATUS", 0);
                //        DateTime dCreateDate = ODBCWrapper.Utils.GetDateSafeVal(selectQuery, "CREATE_DATE", 0);
                //        DateTime dNow = ODBCWrapper.Utils.GetDateSafeVal(selectQuery, "DNOW", 0);

                //        bool isActive = ((nActivateStatus == 1) || !(nActivateStatus == 0 && dCreateDate.AddHours(nActivationMustHours) < dNow));

                //        res = isActive ? DALUserActivationState.Activated : DALUserActivationState.NotActivated;
                //    }
                //    else
                //    {

                //        res = DALUserActivationState.UserDoesNotExist;
                //    }
                //}

                //selectQuery.Finish();
                //selectQuery = null;

                //if (res == DALUserActivationState.UserDoesNotExist || (res == DALUserActivationState.NotActivated && GetIsActivationNeeded(nParentGroupID)))
                //{
                //    return res;
                //}
                //else
                //{
                //    res = DALUserActivationState.Activated;
                //}

                //// If reached here (res == 0), user's activation status is true, so need to check if he is non-master awaiting master's approval
                ////
                //// CHECK ACTIVATION STATUS IN USERS

                //ODBCWrapper.DataSetSelectQuery selectQuery1 = new ODBCWrapper.DataSetSelectQuery();
                //selectQuery1.SetConnectionKey("USERS_CONNECTION_STRING");
                //selectQuery1.SetCachedSec(0);

                //selectQuery1 += "SELECT TOP 1 GETDATE() AS DNOW, ID, IS_MASTER, CREATE_DATE, IS_ACTIVE, STATUS FROM USERS_DOMAINS WITH (NOLOCK) WHERE ";
                //selectQuery1 += ODBCWrapper.Parameter.NEW_PARAM("USER_ID", "=", nUserID);
                //selectQuery1 += " AND GROUP_ID IN (" + string.Join(",", arrGroupIDs) + ")";
                ////selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
                //selectQuery1 += " ORDER BY CREATE_DATE DESC";

                //if (selectQuery1.Execute("query", true) != null)
                //{
                //    Int32 nCount = selectQuery1.Table("query").DefaultView.Count;
                //    if (nCount > 0)
                //    {

                //        int isMaster = ODBCWrapper.Utils.GetIntSafeVal(selectQuery1, "IS_MASTER", 0);
                //        int isActive = ODBCWrapper.Utils.GetIntSafeVal(selectQuery1, "IS_ACTIVE", 0);
                //        int nStatus = ODBCWrapper.Utils.GetIntSafeVal(selectQuery1, "STATUS", 0);

                //        DateTime dCreateDate1 = ODBCWrapper.Utils.GetDateSafeVal(selectQuery1, "CREATE_DATE", 0);
                //        DateTime dNow1 = ODBCWrapper.Utils.GetDateSafeVal(selectQuery1, "DNOW", 0);

                //        bool isActive1 = ((isMaster > 0) || !(isActive == 0 && dCreateDate1.AddHours(nActivationMustHours) < dNow1));

                //        if (nStatus != 2)
                //        {
                //            res = isActive1 ? DALUserActivationState.Activated : DALUserActivationState.NotActivatedByMaster;
                //        }
                //        else
                //        {
                //            res = DALUserActivationState.UserRemovedFromDomain;
                //        }

                //    }
                //    else //user does not have a Domain
                //    {

                //        res = DALUserActivationState.UserWIthNoDomain;
                //    }
                //}

                //selectQuery1.Finish();
                //selectQuery1 = null;

                #endregion

            }
            catch (Exception ex)
            {
                HandleException(ex);              
            }

            return res;
        }

        public static bool IsUserActivated(int nGroupID, int nActivationMustHours, ref string sUserName, ref int nUserID)
        {
            bool bRet = false;

            try
            {
                // Check Activation Status in Users

                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey("USERS_CONNECTION_STRING");
                selectQuery.SetCachedSec(0);

                selectQuery += "SELECT GETDATE() AS DNOW, ID, CREATE_DATE, ACTIVATE_STATUS, USERNAME FROM USERS WHERE IS_ACTIVE=1 AND STATUS=1 AND ";
                if (!string.IsNullOrEmpty(sUserName))
                {
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("USERNAME", "=", sUserName);
                }
                else
                {
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nUserID);
                }

                selectQuery += " AND ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
                //selectQuery += "and group_id " + TVinciShared.PageUtils.GetFullChildGroupsStr(m_nGroupID, "MAIN_CONNECTION_STRING");

                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        nUserID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
                        sUserName = selectQuery.Table("query").DefaultView[0].Row["USERNAME"].ToString();

                        Int32 nAS = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ACTIVATE_STATUS"].ToString());
                        DateTime dCreateDate = (DateTime)(selectQuery.Table("query").DefaultView[0].Row["CREATE_DATE"]);
                        DateTime dNow = (DateTime)(selectQuery.Table("query").DefaultView[0].Row["DNOW"]);

                        bRet = ((nAS == 1) || !(nAS == 0 && dCreateDate.AddHours(nActivationMustHours) < dNow));

                        if (!bRet)
                        {
                            return bRet;
                        }
                    }
                }

                selectQuery.Finish();
                selectQuery = null;


                // If reached here, user's activation status is true, so need to check if still requires master's approval

                selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey("USERS_CONNECTION_STRING");
                selectQuery.SetCachedSec(0);

                selectQuery += "SELECT TOP 1 GETDATE() AS DNOW, ID, IS_MASTER, CREATE_DATE, IS_ACTIVE FROM USERS_DOMAINS WITH (NOLOCK) WHERE ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("USER_ID", "=", nUserID);
                selectQuery += " AND ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
                selectQuery += " ORDER BY CREATE_DATE DESC";

                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        int isMaster = int.Parse(selectQuery.Table("query").DefaultView[0].Row["IS_MASTER"].ToString());
                        int isActive = int.Parse(selectQuery.Table("query").DefaultView[0].Row["IS_ACTIVE"].ToString());

                        DateTime dCreateDate = (DateTime)(selectQuery.Table("query").DefaultView[0].Row["CREATE_DATE"]);
                        DateTime dNow = (DateTime)(selectQuery.Table("query").DefaultView[0].Row["DNOW"]);

                        bRet = ((isMaster > 0) || !(isActive == 0 && dCreateDate.AddHours(nActivationMustHours) < dNow));
                    }
                }

                selectQuery.Finish();
                selectQuery = null;

            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return bRet;
        }


        public static bool SaveBasicData(int nUserID, string sPassword, string sSalt, string sFacebookID, string sFacebookImage, bool bIsFacebookImagePermitted, string sFacebookToken, string sUserName, string sFirstName,
                                        string sLastName, string sEmail, string sAddress, string sCity, int nCountryID, int nStateID, string sZip, string sPhone, string sAffiliateCode, string twitterToken, string twitterTokenSecret,
                                        string sCoGuid = "")
        {
            try
            {
                ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("users");
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("PASSWORD", "=", sPassword);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("SALT", "=", sSalt);

                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("FACEBOOK_ID", "=", sFacebookID);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("FACEBOOK_IMAGE", "=", sFacebookImage);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("FACEBOOK_IMAGE_PERMITTED", "=", bIsFacebookImagePermitted);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("FB_TOKEN", "=", sFacebookToken);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("Twitter_Token", "=", twitterToken);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("Twitter_TokenSecret", "=", twitterTokenSecret);
                if (!string.IsNullOrEmpty(sUserName))
                {
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("USERNAME", "=", sUserName);
                }

                if (!string.IsNullOrEmpty(sCoGuid))
                {
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("COGUID", "=", sCoGuid);
                }

                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("FIRST_NAME", "=", sFirstName);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("LAST_NAME", "=", sLastName);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("EMAIL_ADD", "=", sEmail);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ADDRESS", "=", sAddress);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("CITY", "=", sCity);

                if (nCountryID >= 0)
                {
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("COUNTRY_ID", "=", nCountryID);
                }

                if (nStateID >= 0)
                {
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("STATE_ID", "=", nStateID);
                }

                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ZIP", "=", sZip);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("PHONE", "=", sPhone);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("REG_AFF", "=", sAffiliateCode);

                updateQuery += "WHERE";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nUserID);

                bool inserted = updateQuery.Execute();

                updateQuery.Finish();
                updateQuery = null;


                return inserted;
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return false;
        }


        private static void HandleException(Exception ex)
        {
            //throw new NotImplementedException();
        }


        public static int IsUserActivated(int m_nGroupID, int nUserID)
        {
            int nAS = 0;

            try
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey("USERS_CONNECTION_STRING");
                selectQuery.SetCachedSec(0);
                selectQuery += "SELECT ACTIVATE_STATUS FROM USERS WITH (NOLOCK) WHERE IS_ACTIVE=1 AND STATUS=1 AND ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nUserID);
                selectQuery += " AND ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", m_nGroupID);
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        nAS = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ACTIVATE_STATUS"].ToString());
                    }
                }
                selectQuery.Finish();
                selectQuery = null;

            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return nAS;
        }

        public static bool Get_UserSessionByDeviceID(long lSiteGuid, string sDeviceID, ref bool bIsActive, ref long lIDInUsersSessions)
        {
            bool res = false;
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_UserSessionDetailsByDeviceID");
            sp.SetConnectionKey("USERS_CONNECTION_STRING");
            sp.AddParameter("@SiteGuid", lSiteGuid);
            sp.AddParameter("@DeviceID", sDeviceID);
            DataSet ds = sp.ExecuteDataSet();
            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
            {
                DataTable dt = ds.Tables[0];
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    lIDInUsersSessions = ODBCWrapper.Utils.GetLongSafeVal(dt.Rows[0], "id");
                    bIsActive = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0], "is_active") != 0;
                    res = true;
                }
            }

            return res;
        }

        public static bool Get_UserSessionDetailsBySessionIDAndIP(long lSiteGuid, string sSessionID, string sIP, ref bool bIsActive, ref long lIDInUserSessions)
        {
            bool res = false;
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_UserSessionDetailsBySessionIDAndIP");
            sp.SetConnectionKey("USERS_CONNECTION_STRING");
            sp.AddParameter("@SiteGuid", lSiteGuid);
            sp.AddParameter("@SessionID", sSessionID);
            sp.AddParameter("@UserIP", sIP);
            DataSet ds = sp.ExecuteDataSet();
            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
            {
                DataTable dt = ds.Tables[0];
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    lIDInUserSessions = ODBCWrapper.Utils.GetLongSafeVal(dt.Rows[0], "id");
                    bIsActive = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0], "is_active") != 0;
                    res = true;
                }
            }

            return res;
        }

        public static long Insert_NewUserSessionAndReturnID(long lSiteGuid, string sSessionID, string sIP, string sDeviceID, bool bIsActive, int nStatus)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Insert_NewUserSession");
            sp.SetConnectionKey("USERS_CONNECTION_STRING");
            sp.AddParameter("@SiteGuid", lSiteGuid);
            sp.AddParameter("@SessionID", sSessionID);
            sp.AddParameter("@UserIP", sIP);
            sp.AddParameter("@DeviceID", sDeviceID);
            sp.AddParameter("@IsActive", bIsActive ? 1 : 0);
            sp.AddParameter("@Status", nStatus);
            DateTime dtToInsertToDB = DateTime.UtcNow;
            sp.AddParameter("@CreateDate", dtToInsertToDB);
            sp.AddParameter("@UpdateDate", dtToInsertToDB);
            sp.AddParameter("@LastActionDate", dtToInsertToDB);
            sp.ExecuteNonQuery();
            return Get_UserSessionID(lSiteGuid, sSessionID, sIP, sDeviceID, bIsActive, nStatus);

        }

        public static long Get_UserSessionID(long lSiteGuid, string sSessionID, string sIP, string sDeviceID, bool bIsActive, int nStatus)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_UserSessionID");
            sp.SetConnectionKey("USERS_CONNECTION_STRING");
            sp.AddParameter("@SiteGuid", lSiteGuid);
            sp.AddParameter("@SessionID", sSessionID);
            sp.AddParameter("@UserIP", sIP);
            sp.AddParameter("@DeviceID", sDeviceID);
            sp.AddParameter("@IsActive", bIsActive ? 1 : 0);
            sp.AddParameter("@Status", nStatus);
            return sp.ExecuteReturnValue<long>();
        }

        public static void Update_IsActiveForAllUserSessionsOtherThan(long lOtherThanThisSiteGuid, string sDeviceID, bool bIsActive)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Update_IsActiveForAllUserSessionsOtherThan");
            sp.SetConnectionKey("USERS_CONNECTION_STRING");
            sp.AddParameter("@OtherThanThisSiteGuid", lOtherThanThisSiteGuid);
            sp.AddParameter("@DeviceID", sDeviceID);
            sp.AddParameter("@IsActive", bIsActive ? 1 : 0);
            DateTime dtToWriteToDB = DateTime.UtcNow;
            sp.AddParameter("@UpdateDate", dtToWriteToDB);
            sp.AddParameter("@LastActionDate", dtToWriteToDB);
            sp.ExecuteNonQuery();

        }

        public static int Get_CountOfActiveUserSessions(long lSiteGuid)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_CountOfActiveUserSessions");
            sp.SetConnectionKey("USERS_CONNECTION_STRING");
            sp.AddParameter("@SiteGuid", lSiteGuid);
            return sp.ExecuteReturnValue<int>();
        }

        public static void Update_UserStateInUsers(long lSiteGuid, int nUserState)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Update_UserStateInUsers");
            sp.SetConnectionKey("USERS_CONNECTION_STRING");
            sp.AddParameter("@ID", lSiteGuid);
            sp.AddParameter("@UserState", nUserState);
            sp.ExecuteNonQuery();
        }

        public static int Get_UserStateFromUsers(long lSiteGuid)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_UserStateFromUsers");
            sp.SetConnectionKey("USERS_CONNECTION_STRING");
            sp.AddParameter("@SiteGuid", lSiteGuid);
            return sp.ExecuteReturnValue<int>();
        }

        public static bool Get_UserEmailBySiteGuid(long lSiteGuid, string sConnKey, ref string sEmail)
        {
            bool res = false;
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_UserEmailBySiteGuid");
            sp.SetConnectionKey(!string.IsNullOrEmpty(sConnKey) ? sConnKey : "CONNECTION_STRING");
            sp.AddParameter("@SiteGuid", lSiteGuid);
            DataSet ds = sp.ExecuteDataSet();
            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
            {
                DataTable dt = ds.Tables[0];
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    if (dt.Rows[0]["email_add"] != DBNull.Value && dt.Rows[0]["email_add"] != null)
                    {
                        sEmail = dt.Rows[0]["email_add"].ToString();
                        res = true;
                    }
                }
            }
            return res;
        }

        public static string Get_UsernameBySiteGuid(long lSiteGuid, string sConnKey)
        {
            string res = string.Empty;
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_UsernameBySiteGuid");
            sp.SetConnectionKey(!string.IsNullOrEmpty(sConnKey) ? sConnKey : "CONNECTION_STRING");
            sp.AddParameter("@SiteGuid", lSiteGuid);

            DataSet ds = sp.ExecuteDataSet();
            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
            {
                DataTable dt = ds.Tables[0];
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    res = ODBCWrapper.Utils.GetSafeStr(dt.Rows[0]["username"]);
                }
            }

            return res;
        }

        public static string Get_FirstnameBySiteGuid(long lSiteGuid, string sConnKey)
        {
            string res = string.Empty;
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_FirstnameBySiteGuid");
            sp.SetConnectionKey(!string.IsNullOrEmpty(sConnKey) ? sConnKey : "CONNECTION_STRING");
            sp.AddParameter("@SiteGuid", lSiteGuid);

            DataSet ds = sp.ExecuteDataSet();
            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
            {
                DataTable dt = ds.Tables[0];
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    res = ODBCWrapper.Utils.GetSafeStr(dt.Rows[0]["first_name"]);
                }
            }

            return res;
        }

        public static DataTable GenerateToken(string sUserName, int nGroupID, int nTokenValidityHours)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure(SP_GENERATE_TOKEN);
            sp.SetConnectionKey("USERS_CONNECTION_STRING");
            sp.AddParameter("@sUserName", sUserName);
            sp.AddParameter("@nGroupID", nGroupID);
            sp.AddParameter("@nTokenValidityHours", nTokenValidityHours);

            DataSet ds = sp.ExecuteDataSet();

            if (ds != null)
                return ds.Tables[0];

            return null;
        }

        public static DataTable GetUserType(long groupID, long lSiteGuid)
        {
            try
            {
                ODBCWrapper.StoredProcedure spGetUserType = new ODBCWrapper.StoredProcedure(SP_GET_USER_TYPE);
                spGetUserType.SetConnectionKey("USERS_CONNECTION_STRING");

                spGetUserType.AddParameter("@groupID", groupID);
                spGetUserType.AddParameter("@User", lSiteGuid);
                DataSet ds = spGetUserType.ExecuteDataSet();

                if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
                {
                    return (ds.Tables[0]);
                }

            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return null;
        }

        public static int GetDefaultGroupOperator(int groupId)
        {
            int retOperatorId = 0;
            try
            {
                ODBCWrapper.StoredProcedure spGetDefaultGroupOperator = new ODBCWrapper.StoredProcedure(SP_GET_DEFUALT_GROUP_OPERATOR);
                spGetDefaultGroupOperator.SetConnectionKey("MAIN_CONNECTION_STRING");
                spGetDefaultGroupOperator.AddParameter("@GroupID", groupId);
                DataTable dt = spGetDefaultGroupOperator.Execute();
                if (dt != null)
                {
                    if (dt.DefaultView[0].Row != null)
                    {
                        retOperatorId = Utils.GetIntSafeVal(dt.DefaultView[0].Row, "operatorId");
                    }
                }
            }
            catch (Exception)
            {
                
            }

            return retOperatorId;
        }

        public static DataSet Get_UsersListByBulk(int groupId, string sFreeTxt , int top , int page)
        {   
            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_UsersListByBulk");
                sp.SetConnectionKey("users_connection_heart");
                sp.AddParameter("@GroupId", groupId);
                sp.AddParameter("@FreeTxt", sFreeTxt);
                sp.AddParameter("@Top", top);
                sp.AddParameter("@Page", page);
                DataSet ds = sp.ExecuteDataSet();
                if (ds != null)
                {
                    return ds;
                }
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static int Update_UserDynamicData(int nUserID, int nGroupID, string xmlTypeValue)
        {
            int rows = 0;

            if (string.IsNullOrEmpty(xmlTypeValue))
            {
                return rows;
            }

            try
            {
                StoredProcedure spUpdateUserDynamicData = new StoredProcedure("Update_UserDynamicData");

                spUpdateUserDynamicData.AddParameter("@doc", xmlTypeValue);
                spUpdateUserDynamicData.AddParameter("@group_id", nGroupID);
                spUpdateUserDynamicData.AddParameter("@site_guid", nUserID);

                rows = spUpdateUserDynamicData.ExecuteReturnValue<int>();
            }
            catch
            {
            }

            return rows;
        }

        public static DataTable Insert_LoginPIN(string siteGuid, string pinCode, int groupID, DateTime expired_date, string secret)
        {
            try
            {
                StoredProcedure sp = new StoredProcedure("Insert_LoginPIN");
                sp.AddParameter("@groupID", groupID);
                sp.AddParameter("@siteGuid", siteGuid);
                sp.AddParameter("@pinCode", pinCode);
                sp.AddParameter("@expired_date", expired_date);
                sp.AddParameter("@secret", secret != null ? secret : string.Empty);
                DataSet ds = sp.ExecuteDataSetWithListParam();
                if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
                {
                    return ds.Tables[0];
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        public static bool PinCodeExsits(int groupID, string newPIN, DateTime expired_date)
        {
            try
            {
                StoredProcedure sp = new StoredProcedure("Is_PinCodeExsits");  
                sp.AddParameter("@groupID", groupID);
                sp.AddParameter("@newPIN", newPIN);
                sp.AddParameter("@expired_date", expired_date);
                bool res = sp.ExecuteReturnValue<bool>();
                return res;
            }
            catch 
            {
                return false;
            }
        }

        public static DataRow GetUserByPIN(int groupID, string pinCode, string secret, out bool security, out bool loginViaPin, out DateTime expiredPIN)
        {
            security = false;
            loginViaPin = false;
            expiredPIN = DateTime.MaxValue;
            try
            {
                StoredProcedure sp = new StoredProcedure("Get_UserByPIN");
                sp.AddParameter("@groupID", groupID);
                sp.AddParameter("@pinCode", pinCode);
                sp.AddParameter("@secret", secret);
                DataSet ds = sp.ExecuteDataSet();

                if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
                {
                    if (ds.Tables.Count > 0 && ds.Tables[0] != null && ds.Tables[0].Rows != null && ds.Tables[0].Rows.Count > 0)
                    {
                        int nSecurity = ODBCWrapper.Utils.GetIntSafeVal(ds.Tables[0].Rows[0], "security");
                        int nLoginViaPin = ODBCWrapper.Utils.GetIntSafeVal(ds.Tables[0].Rows[0], "loginViaPin");
                        security = (nSecurity == 1 ? true : false);
                        loginViaPin = (nLoginViaPin == 1 ? true : false);
                    }
                    // perfect matched 
                    if (ds.Tables.Count > 1 && ds.Tables[1] != null && ds.Tables[1].Rows != null && ds.Tables[1].Rows.Count > 0)
                    {
                        return ds.Tables[1].Rows[0];
                    }

                    if (security)
                    {
                        //check secret
                        if (ds.Tables.Count > 2 && ds.Tables[2] != null && ds.Tables[2].Rows != null && ds.Tables[2].Rows.Count > 0)
                        {
                            return ds.Tables[2].Rows[0];
                        }
                    }
                    //pin not valid
                    else if (ds.Tables.Count > 3 && ds.Tables[3] != null && ds.Tables[3].Rows != null && ds.Tables[3].Rows.Count > 0)
                    {
                        expiredPIN = ODBCWrapper.Utils.GetDateSafeVal(ds.Tables[3].Rows[0], "expired_date");
                    }  
                 
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        public static bool ExpirePIN(int groupID, string PIN)
        {
            try
            {
                StoredProcedure sp = new StoredProcedure("Is_ExpirePIN");
                sp.AddParameter("@groupID", groupID);
                sp.AddParameter("@PIN", PIN);
                bool res = sp.ExecuteReturnValue<bool>();
                return res;
            }
            catch
            {
                return false;
            }
        }
               
        public static void Get_LoginSettings(int groupID, out bool security, out bool loginViaPin)
        {
            security = false;
            loginViaPin = false;
            try
            {
                StoredProcedure sp = new StoredProcedure("Get_LoginSettings");
                sp.AddParameter("@groupID", groupID);
                DataSet ds = sp.ExecuteDataSetWithListParam();

                if (ds != null && ds.Tables != null && ds.Tables.Count > 0 && ds.Tables[0] != null && ds.Tables[0].Rows != null && ds.Tables[0].Rows.Count > 0)
                {
                    int nSecurity = ODBCWrapper.Utils.GetIntSafeVal(ds.Tables[0].Rows[0], "security");
                    int nLoginViaPin = ODBCWrapper.Utils.GetIntSafeVal(ds.Tables[0].Rows[0], "loginViaPin");
                    security = (nSecurity == 1 ? true : false);
                    loginViaPin = (nLoginViaPin == 1 ? true : false);
                }
            }
            catch
            {
                security = false;
                loginViaPin = true;
            }
        }

        public static bool ExpirePINsByUserID(int groupID, string siteGuid)
        {
            try
            {
                StoredProcedure sp = new StoredProcedure("Update_ExpirePINByUserID");
                sp.AddParameter("@groupID", groupID);
                sp.AddParameter("@siteGuid", siteGuid);
                bool res = sp.ExecuteReturnValue<bool>();
                return res;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// GetUserActivationState
        /// </summary>
        /// <param name="arrGroupIDs"></param>
        /// <param name="nActivationMustHours"></param>
        /// <param name="sUserName"></param>
        /// <param name="nUserID"></param>
        /// <param name="nActivateStatus"></param>
        /// <param name="dCreateDate"></param>
        /// <param name="dNow"></param>
        /// <returns>
        ///     -2 - error
        ///     -1 - user does not exist or was removed/deactivated   
        ///      0 - user activated 
        ///      1 - user not activated 
        ///      2 - user not activated by master
        ///      3 - user removed from domain
        ///      
        /// </returns>
        public static DALUserActivationState GetUserActivationState(int nParentGroupID, List<int> lGroupIDs, int nActivationMustHours, int nUserID)
        {
            DALUserActivationState res = DALUserActivationState.Error;

            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_UserState");
                sp.SetConnectionKey("USERS_CONNECTION_STRING");                
                sp.AddParameter("@Id", nUserID);
                sp.AddIDListParameter<int>("@GroupsID", lGroupIDs, "Id");
                sp.AddParameter("@ActivationMustHours", nActivationMustHours);
                DataTable dt = sp.Execute();
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    int nSPReault = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0], "STATE");
                    if (Enum.IsDefined(typeof(DALUserActivationState), nSPReault))
                    {
                        res = (DALUserActivationState)nSPReault;
                    }
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return res;
        }


        public static bool UpdateLoginPinStatusByPinCode(int groupID, string siteGuid, string pinCode)
        {
            try
            {
                StoredProcedure sp = new StoredProcedure("Update_LoginPinStatusByPinCode");
                sp.AddParameter("@group_id", groupID);
                sp.AddParameter("@site_guid", siteGuid);
                sp.AddParameter("@pin_code", pinCode);
                int rows = sp.ExecuteReturnValue<int>();
                return rows > 0;
            }
            catch
            {
                return false;
            }
        }

        public static DataTable Get_FavoriteMediaIds(string userId, List<int> mediaIds, string udid, string mediaType)
        {
            try
            {
                StoredProcedure sp = new StoredProcedure("Get_FavoriteMediaIds");
                sp.AddParameter("@user_id", userId);
                sp.AddParameter("@udid", !string.IsNullOrEmpty(udid) ? udid : null);
                sp.AddParameter("@media_type", !string.IsNullOrEmpty(mediaType) ? mediaType : null);
                sp.AddIDListParameter("@media_ids", mediaIds, "id");
                DataSet ds = sp.ExecuteDataSet();
                if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
                {
                    return ds.Tables[0];
                }
            }
            catch (Exception)
            {
                return null;
            }
            return null;
        }

        public static DataTable Get_UserFavorites(string sUserGUID, string sUDID, int nType)
        {
            StoredProcedure sp = new StoredProcedure("Get_UserFavorites");
            sp.AddParameter("@SiteGUID", sUserGUID);
            sp.AddParameter("@MediaTypeID", nType);
            
            if (!string.IsNullOrEmpty(sUDID))
                sp.AddParameter("@UDID", sUDID);

            DataSet ds = sp.ExecuteDataSet();

            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
            {
                return ds.Tables[0];
            }

            return null;
        }
    }         
}
