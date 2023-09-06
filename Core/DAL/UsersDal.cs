using ApiObjects;
using ApiObjects.Base;
using ApiObjects.CanaryDeployment.Microservices;
using ApiObjects.SSOAdapter;
using ApiObjects.User;
using AuthenticationGrpcClientWrapper;
using CanaryDeploymentManager;
using Phx.Lib.Appconfig;
using CouchbaseManager;
using Phx.Lib.Log;
using Newtonsoft.Json;
using ODBCWrapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;
using Tvinci.Core.DAL;

namespace DAL
{
    public interface IUserPartnerRepository
    {
        bool SetupPartnerInDb(long partnerId, long updaterId);
        bool DeletePartnerDb(long partnerId, long updaterId);
    }

    public class UsersDal : BaseDal, IUserPartnerRepository
    {
        private static readonly Lazy<UsersDal> LazyInstance = new Lazy<UsersDal>(() => new UsersDal(), LazyThreadSafetyMode.PublicationOnly);
        public static UsersDal Instance => LazyInstance.Value;

        private UsersDal()
        {
        }

        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        #region Private Constants

        // Default command timeout to 30 minutes ( needed for purge)
        private const int SQL_COMMAND_TIMEOUT_SEC = 1800;

        private const string SP_GET_USER_TYPE_DATA = "Get_UserTypeData";
        private const string SP_GET_USER_TYPE_DATA_BY_IDS = "Get_UserTypesDataByIDs";
        private const string SP_GET_USER_BASIC_DATA = "Get_UserBasicData";
        private const string SP_GET_USERS_BASIC_DATA = "Get_UsersBasicData";
        private const string SP_GET_USER_DOMAINS = "sp_GetUserDomains";
        private const string SP_INSERT_NEW_USER = "Insert_NewUser";
        private const string SP_GET_IS_ACTIVATION_NEEDED = "Get_IsActivationNeeded";
        private const string SP_GET_GROUP_USERS = "Get_GroupUsers";
        private const string SP_GET_GROUP_USERS_SEARCH_FIELDS = "Get_GroupUsersSearchFields";
        private const string SP_GET_DEVICES_TO_USERS_NON_PUSH = "Get_DevicesToUsersNonPushAction";
        private const string SP_GET_DEVICES_TO_USERS_PUSH = "Get_DevicesToUsersPushAction";
        private const string SP_GENERATE_TOKEN = "GenerateToken";
        private const string SP_GET_USER_TYPE = "Get_UserType";
        private const string SP_GET_DEFUALT_GROUP_OPERATOR = "Get_DefaultGroupOperator";
        private const string DELETE_USER = "Delete_User";
        private const string SSO_ADAPTER_WONERSHIP_ERR_MSG = "This request is disabled on Phoenix, ownership flag of SSOAdapters is turned on and thus this action should be done on Authentication micro service, Check TCM [MicroservicesClientConfiguration.Authentication.DataOwnershipConfiguration.SSOAdapterProfiles] if this is unexpected behavior";

        private const string USERS_CONNECTION_STRING = "USERS_CONNECTION_STRING";

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
                StoredProcedure spGetUserBasicData = new StoredProcedure(SP_GET_USER_BASIC_DATA);
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
            StoredProcedure spGetUserBasicData = new StoredProcedure(SP_GET_USERS_BASIC_DATA);
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

        public static InsertUserOutputModel InsertUser(InsertUserInputModel model)
        {
            try
            {
                var spInsertUser = new StoredProcedure(SP_INSERT_NEW_USER);
                spInsertUser.SetConnectionKey("USERS_CONNECTION_STRING");

                spInsertUser.AddParameter("@username", model.Username);
                spInsertUser.AddParameter("@password", model.Password);
                spInsertUser.AddParameter("@salt", model.Salt);
                spInsertUser.AddParameter("@firstName", model.FirstName);
                spInsertUser.AddParameter("@lastName", model.LastName);
                spInsertUser.AddParameter("@facebookID", model.FacebookId);
                spInsertUser.AddParameter("@facebookImage", model.FacebookImage);
                spInsertUser.AddParameter("@facebookToken", model.FacebookToken);
                spInsertUser.AddParameter("@isFacebookImagePermitted", model.IsFacebookImagePermitted);
                spInsertUser.AddParameter("@email", model.Email);
                spInsertUser.AddParameter("@activateStatus", model.ActivateStatus);
                spInsertUser.AddParameter("@activationToken", model.ActivationToken);

                if (!string.IsNullOrEmpty(model.CoGuid))
                {
                    spInsertUser.AddParameter("@coGuid", model.CoGuid);
                }
                if (!string.IsNullOrEmpty(model.ExternalToken))
                {
                    spInsertUser.AddParameter("@externalToken", model.ExternalToken);
                }
                if (model.UserTypeId.HasValue)
                {
                    spInsertUser.AddParameter("@userTypeID", model.UserTypeId.Value);
                }
                if (model.UsernameEncryptionEnabled)
                {
                    spInsertUser.AddParameter("@usernameEncryptionEnabled", model.UsernameEncryptionEnabled);
                }

                spInsertUser.AddParameter("@address", model.Address);
                spInsertUser.AddParameter("@city", model.City);
                spInsertUser.AddParameter("@country", model.CountryId);
                spInsertUser.AddParameter("@state", model.StateId);
                spInsertUser.AddParameter("@zip", model.Zip);
                spInsertUser.AddParameter("@phone", model.Phone);
                spInsertUser.AddParameter("@affiliateCode", model.AffiliateCode);
                spInsertUser.AddParameter("@twitterToken", model.TwitterToken);
                spInsertUser.AddParameter("@twitterTokenSecret", model.TwitterTokenSecret);
                spInsertUser.AddParameter("@groupID", model.GroupId);

                var resultDataset = spInsertUser.ExecuteDataSet();
                if (resultDataset?.Tables.Count != 1 || resultDataset.Tables[0]?.Rows.Count != 1)
                {
                    return null;
                }

                var dataRow = resultDataset.Tables[0].Rows[0];

                return new InsertUserOutputModel
                {
                    UserId = Utils.GetIntSafeVal(dataRow, "ID"),
                    CreateDate = Utils.GetDateSafeVal(dataRow, "CREATE_DATE"),
                    UpdateDate = Utils.GetDateSafeVal(dataRow, "UPDATE_DATE")
                };
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return null;
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

        /// <summary>WARNING do not use directly. use UserStorage class instead</summary>
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
                selectQuery.SetConnectionKey("USERS_CONNECTION_STRING");
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

        public static bool UpdateFailCount(int groupId, int nUserID, int nAdd, bool setLoginDate)
        {
            bool updateRes = false;
            ODBCWrapper.DirectQuery directQuery = null;
            try
            {
                if (CanaryDeploymentFactory.Instance.GetMicroservicesCanaryDeploymentManager().IsDataOwnershipFlagEnabled(groupId, CanaryDeploymentDataOwnershipEnum.AuthenticationUserLoginHistory))
                {
                    var authClient = AuthenticationClient.GetClientFromTCM();
                    var isSuccessfulLogin = nAdd == 0;
                    if (isSuccessfulLogin)
                    {
                        return authClient.RecordUserSuccessfulLogin(groupId, nUserID);
                    }
                    else
                    {
                        return authClient.RecordUserFailedLogin(groupId, nUserID);
                    }
                }

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
                if (setLoginDate)
                {
                    directQuery += ",LAST_LOGIN_DATE=getdate()";
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

        /// <summary>
        ///
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="userId"></param>
        /// <param name="roleIds"></param>
        /// <returns>Return True if roles update successfully</returns>
        public static bool UpsertUserRoleIds(int groupId, long userId, List<long> roleIds)
        {
            bool result = false;
            try
            {
                StoredProcedure sp = new StoredProcedure("UpsertUserRoleIds");
                sp.SetConnectionKey("USERS_CONNECTION_STRING");
                sp.AddParameter("@groupId", groupId);
                sp.AddParameter("@userId", userId);
                sp.AddIDListParameter("@roleIds", roleIds, "id");

                result = sp.ExecuteReturnValue<int>() > 0;
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while UpsertUserRoleIds in DB, groupId: {0}, userId: {1}, roleIds:{2}, ex:{3} ", groupId, userId, string.Join(", ", roleIds), ex);
            }

            return result;
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

        /// <summary>WARNING do not use directly. use UserStorage class instead</summary>
        public static int GetUserPasswordFailHistory(string username, int groupId, ref DateTime dNow, ref int failCount, ref DateTime lastFailDate, ref DateTime lastHitDate, ref DateTime passwordUpdateDate)
        {
            int userId = 0;

            var sp = new StoredProcedure("Get_LoginFailCount");
            sp.SetConnectionKey("USERS_CONNECTION_STRING");
            sp.AddParameter("@username", username);
            sp.AddParameter("@groupID", groupId);

            DataSet ds = sp.ExecuteDataSet();

            if (ds != null && ds.Tables[0] != null && ds.Tables[0].Rows != null && ds.Tables[0].Rows.Count > 0)
            {
                DataTable dt = ds.Tables[0];

                if (dt != null && dt.Rows.Count > 0)
                {
                    dNow = Utils.GetDateSafeVal(dt.Rows[0], "dNow");
                    failCount = Utils.GetIntSafeVal(dt.Rows[0], "FAIL_COUNT");
                    userId = Utils.GetIntSafeVal(dt.Rows[0], "id");
                    lastFailDate = Utils.GetDateSafeVal(dt.Rows[0], "LAST_FAIL_DATE");
                    lastHitDate = Utils.GetDateSafeVal(dt.Rows[0], "LAST_HIT_DATE");
                    passwordUpdateDate = Utils.GetDateSafeVal(dt.Rows[0], "PASSWORD_UPDATE_DATE");
                }
            }

            // in case we are connected to the authentication microservice
            // we need to overwrite the fail count, last fail date and last hit date.
            // we cannot remove te call to the origianl SPR because the password update date and userId are not owned by the authentication MS
            if (CanaryDeploymentFactory.Instance.GetMicroservicesCanaryDeploymentManager().IsDataOwnershipFlagEnabled(groupId, CanaryDeploymentDataOwnershipEnum.AuthenticationUserLoginHistory))
            {
                var authClient = AuthenticationClient.GetClientFromTCM();
                var failHistory = authClient.GetUserLoginHistory(groupId, userId);
                if (failHistory != null)
                {
                    failCount = failHistory.ConsecutiveFailedLoginCount;
                    lastFailDate = DateTimeOffset.FromUnixTimeSeconds(failHistory.LastLoginFailureDate).UtcDateTime;
                    lastHitDate = DateTimeOffset.FromUnixTimeSeconds(failHistory.LastLoginAttemptDate).UtcDateTime;
                }
            }

            return userId;
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

        public static ApiObjects.Response.GenericResponse<T> InsertDeviceReferenceData<T>(ContextData contextData, T coreObject, long utcNow) where T : DeviceReferenceData
        {
            var response = new ApiObjects.Response.GenericResponse<T>();
            var sp = new StoredProcedure("Insert_DeviceReferenceData");
            sp.SetConnectionKey("USERS_CONNECTION_STRING");
            sp.AddParameter("@groupID", contextData.GroupId);
            sp.AddParameter("@updaterID", contextData.UserId);
            sp.AddParameter("@createDate", utcNow);
            sp.AddParameter("@name", coreObject.Name.Trim().ToUpper());
            sp.AddParameter("@type", coreObject.GetReferenceType());

            var id = sp.ExecuteReturnValue<int>();

            if (id == -1)
            {
                response.SetStatus(ApiObjects.Response.eResponseStatus.AlreadyExist,
                    $"Device Reference already exist with name: {coreObject.Name}");
            }
            else if (id > 0)
            {
                coreObject.Id = id;
                response.Object = coreObject;
                response.SetStatus(ApiObjects.Response.eResponseStatus.OK);
            }
            else
                response.SetStatus(ApiObjects.Response.eResponseStatus.Error, $"Failed adding {coreObject.Name}");

            return response;
        }

        public static List<DeviceReferenceData> GetDeviceReferenceData(int groupId)
        {
            var res = new List<DeviceReferenceData>();
            var sp = new StoredProcedure("GetDeviceReferenceData");
            sp.SetConnectionKey("USERS_CONNECTION_STRING");
            sp.AddParameter("@groupId", groupId);
            DataSet ds = sp.ExecuteDataSet();

            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
            {
                DataTable dt = ds.Tables[0];
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (dt.Rows[i] != null)
                    {
                        if (Utils.GetLongSafeVal(dt.Rows[i], "type") == (int)DeviceInformationType.Manufacturer)
                        {
                            res.Add(new DeviceManufacturerInformation
                            {
                                Id = Utils.GetLongSafeVal(dt.Rows[i], "id"),
                                Name = Utils.GetSafeStr(dt.Rows[i], "name"),
                                Type = (int)DeviceInformationType.Manufacturer
                            });
                        }
                    }
                }
            }

            return res;
        }

        public static ApiObjects.Response.Status UpdateDeviceReferenceData(ContextData contextData, DeviceReferenceData coreObject)
        {
            var response = new ApiObjects.Response.Status(ApiObjects.Response.eResponseStatus.Error);
            var sp = new StoredProcedure("Update_DeviceReferenceData");
            sp.SetConnectionKey("USERS_CONNECTION_STRING");
            sp.AddParameter("@groupID", contextData.GroupId);
            sp.AddParameter("@id", coreObject.Id);
            sp.AddParameter("@name", coreObject.Name.Trim().ToUpper());
            sp.AddParameter("@updaterId", contextData.UserId);
            sp.AddParameter("@status", coreObject.Status);

            var id = sp.ExecuteReturnValue<int>();

            if (id > 0)
                response = new ApiObjects.Response.Status(ApiObjects.Response.eResponseStatus.OK);
            else
                response = new ApiObjects.Response.Status(ApiObjects.Response.eResponseStatus.Error, $"Failed updating {coreObject.Name}");

            return response;
        }

        public static ApiObjects.Response.GenericResponse<bool> DeleteDeviceInformation(int groupId, long? updaterId, long id)
        {
            var response = new ApiObjects.Response.GenericResponse<bool>();
            var sp = new StoredProcedure("Delete_DeviceReferenceData");
            sp.SetConnectionKey("USERS_CONNECTION_STRING");
            sp.AddParameter("@groupID", groupId);
            sp.AddParameter("@id", id);
            sp.AddParameter("@updaterId", updaterId);

            response.Object = sp.ExecuteReturnValue<bool>();
            if (response.Object)
            {
                response.SetStatus(ApiObjects.Response.eResponseStatus.OK);
            }
            return response;
        }

        /// <summary>WARNING do not use directly. use UserStorage class instead</summary>
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

        public static bool UpdateUserActivationToken(int groupId, int nUserID, string sToken, string sNewToken, int nUserState)
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
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", groupId);

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

        public static int GetUserActivateStatus(int nUserID, int groupId)
        {
            int nActivationStatus = -1;

            try
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey("USERS_CONNECTION_STRING");

                selectQuery += " select ACTIVATE_STATUS from users WITH (nolock) where status=1 and is_active=1 and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nUserID);
                selectQuery += " and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", groupId);

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

        public static bool Insert_ItemList(int nSiteGuid, List<KeyValuePair<int, int>> dItems, int listType, int itemType, int nGroupID)
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

        public static bool Remove_ItemsFromUsersList(int groupId, List<long> userIds, List<long> unavailableItemIds)
        {
            StoredProcedure sp = new StoredProcedure("Remove_ItemsFromUsersList");
            sp.SetConnectionKey("USERS_CONNECTION_STRING");
            sp.AddParameter("@groupID", groupId);
            sp.AddIDListParameter("@userIDs", userIds, "id");
            sp.AddIDListParameter("@itemIDs", unavailableItemIds, "id");
            var result = sp.ExecuteReturnValue<bool>();

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
        /// WARNING do not use directly. use UserStorage class instead.
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
        public static DALUserActivationState GetUserActivationState(int nParentGroupID, int nActivationMustHours, ref string sUserName, ref int nUserID, ref bool isGracePeriod)
        {
            DALUserActivationState res = DALUserActivationState.Error;

            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_UserActivationStatus");
                sp.SetConnectionKey("USERS_CONNECTION_STRING");
                sp.AddParameter("@Id", nUserID);
                sp.AddParameter("@UserName", sUserName);
                sp.AddParameter("@GroupID", nParentGroupID);
                DataSet ds = sp.ExecuteDataSet();
                if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
                {
                    // in case sent userid doesnt exist
                    if (ds.Tables.Count == 1) // looks like, SP returns 2 tables always and this code is never executed
                    {
                        res = DALUserActivationState.UserDoesNotExist;
                    }
                    else
                    {
                        DataTable userDetails = ds.Tables[0];
                        DataTable domainDetails = ds.Tables[1];
                        var userExist = userDetails != null
                            && userDetails.Rows != null
                            && userDetails.Rows.Count > 0
                            // dirty-hack: stored procedure shouldn't return rows when no-user/deleted, but it does return.
                            // USERNAME is not null column, that's why it's chosen as indicator of user's abscense.
                            && userDetails.Rows[0]["USERNAME"] != DBNull.Value;
                        if (userExist)
                        {
                            DataRow dr = userDetails.Rows[0];
                            if (nUserID <= 0)
                            {
                                nUserID = ODBCWrapper.Utils.GetIntSafeVal(dr, "ID");
                            }
                            int activateStatus = ODBCWrapper.Utils.GetIntSafeVal(dr, "ACTIVATE_STATUS"); ;
                            int isActivationNeeded = ODBCWrapper.Utils.GetIntSafeVal(dr, "IS_ACTIVATION_NEEDED");
                            string facebookbId = ODBCWrapper.Utils.GetSafeStr(dr, "FACEBOOK_ID");
                            if (string.IsNullOrEmpty(sUserName))
                            {
                                sUserName = ODBCWrapper.Utils.GetSafeStr(dr, "USERNAME");
                            }

                            DateTime createDate = ODBCWrapper.Utils.GetDateSafeVal(dr, "CREATE_DATE");

                            if (isActivationNeeded == 0 || activateStatus == 1 || !string.IsNullOrEmpty(facebookbId))
                            {
                                res = DALUserActivationState.Activated;
                            }
                            else if (isActivationNeeded == 1 && createDate.AddHours(nActivationMustHours) > DateTime.UtcNow)
                            {
                                res = DALUserActivationState.Activated;
                                isGracePeriod = true;
                            }
                            else
                            {
                                res = DALUserActivationState.NotActivated;
                            }

                            if (domainDetails != null && domainDetails.Rows != null && domainDetails.Rows.Count > 0)
                            {
                                DataRow domainRow = domainDetails.Rows[0];
                                int domainID = ODBCWrapper.Utils.GetIntSafeVal(domainRow, "DOMAIN_ID");
                                bool isMaster = ODBCWrapper.Utils.GetIntSafeVal(domainRow, "IS_MASTER") == 1;
                                bool isSuspended = ODBCWrapper.Utils.GetIntSafeVal(domainRow, "IS_SUSPENDED") == 1;
                                int status = ODBCWrapper.Utils.GetIntSafeVal(domainRow, "STATUS");

                                if (domainID == 0)
                                {
                                    res = DALUserActivationState.UserWIthNoDomain;
                                }
                                else if (status == 3)
                                {
                                    res = DALUserActivationState.NotActivatedByMaster;
                                }
                                else if (isSuspended)
                                {
                                    res = DALUserActivationState.UserDomainSuspended;
                                }
                            }
                        }
                        else
                        {
                            res = DALUserActivationState.UserDoesNotExist;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return res;
        }

        // TODO remove, not used
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


        public static bool SaveBasicData(int groupId, int nUserID, string sPassword, string sSalt, string sFacebookID, string sFacebookImage, bool bIsFacebookImagePermitted,
                                         string sFacebookToken, string sUserName, string sFirstName, string sLastName, string sEmail, string sAddress, string sCity,
                                         int? nCountryID, int nStateID, string sZip, string sPhone, string sAffiliateCode, string twitterToken, string twitterTokenSecret,
                                         DateTime updateDate, string sCoGuid, string externalToken, bool resetFailCount, bool updateUserPassword, bool usernameEncryptionEnabled)
        {
            try
            {
                var updateQuery = new UpdateQuery("users");
                updateQuery.SetConnectionKey("USERS_CONNECTION_STRING");
                updateQuery += Parameter.NEW_PARAM("SALT", "=", sSalt);
                updateQuery += Parameter.NEW_PARAM("FACEBOOK_ID", "=", sFacebookID);
                updateQuery += Parameter.NEW_PARAM("FACEBOOK_IMAGE", "=", sFacebookImage);
                updateQuery += Parameter.NEW_PARAM("FACEBOOK_IMAGE_PERMITTED", "=", bIsFacebookImagePermitted);
                updateQuery += Parameter.NEW_PARAM("FB_TOKEN", "=", sFacebookToken);
                updateQuery += Parameter.NEW_PARAM("ExternalToken", "=", externalToken);
                updateQuery += Parameter.NEW_PARAM("Twitter_Token", "=", twitterToken);
                updateQuery += Parameter.NEW_PARAM("Twitter_TokenSecret", "=", twitterTokenSecret);
                updateQuery += Parameter.NEW_PARAM("FIRST_NAME", "=", sFirstName);
                updateQuery += Parameter.NEW_PARAM("LAST_NAME", "=", sLastName);
                updateQuery += Parameter.NEW_PARAM("EMAIL_ADD", "=", sEmail);
                updateQuery += Parameter.NEW_PARAM("ADDRESS", "=", sAddress);
                updateQuery += Parameter.NEW_PARAM("CITY", "=", sCity);
                updateQuery += Parameter.NEW_PARAM("ZIP", "=", sZip);
                updateQuery += Parameter.NEW_PARAM("PHONE", "=", sPhone);
                updateQuery += Parameter.NEW_PARAM("UPDATE_DATE", "=", updateDate);
                updateQuery += Parameter.NEW_PARAM("REG_AFF", "=", sAffiliateCode);

                if (!string.IsNullOrEmpty(sUserName))
                {
                    updateQuery += Parameter.NEW_PARAM("USERNAME", "=", sUserName);
                }

                if (!string.IsNullOrEmpty(sCoGuid))
                {
                    updateQuery += Parameter.NEW_PARAM("COGUID", "=", sCoGuid);
                }

                if (nCountryID.HasValue && nCountryID >= 0)
                {
                    updateQuery += Parameter.NEW_PARAM("COUNTRY_ID", "=", nCountryID);
                }

                if (nStateID >= 0)
                {
                    updateQuery += Parameter.NEW_PARAM("STATE_ID", "=", nStateID);
                }

                if (resetFailCount)
                {
                    if (CanaryDeploymentFactory.Instance.GetMicroservicesCanaryDeploymentManager().IsDataOwnershipFlagEnabled(groupId, CanaryDeploymentDataOwnershipEnum.AuthenticationUserLoginHistory))
                    {
                        var authClient = AuthenticationClient.GetClientFromTCM();
                        _ = authClient.ResetUserFailedLoginCount(groupId, nUserID);
                    }
                    else
                    {
                        updateQuery += Parameter.NEW_PARAM("FAIL_COUNT", "=", 0);
                    }

                }

                if (updateUserPassword)
                {
                    updateQuery += Parameter.NEW_PARAM("PASSWORD", "=", sPassword);
                    updateQuery += Parameter.NEW_PARAM("PASSWORD_UPDATE_DATE", "=", updateDate);
                }

                if (usernameEncryptionEnabled)
                {
                    updateQuery += Parameter.NEW_PARAM("USERNAME_ENCRYPTION", "=", 1);
                }

                updateQuery += "WHERE";
                updateQuery += Parameter.NEW_PARAM("ID", "=", nUserID);

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

        // WARNING Should be used in username-lazy-migration ONLY. remove after lazy migration finish
        public static long? UpdateUsername(int groupId, string clearUsername, string encryptedUsername)
        {
            var sp = new StoredProcedure("Migrate_Username");
            sp.SetConnectionKey("USERS_CONNECTION_STRING");
            sp.AddParameter("@currentUsername", clearUsername, true);
            sp.AddParameter("@newUsername", encryptedUsername, true);
            sp.AddParameter("@groupId", groupId);
            var table = sp.Execute();
            var userId = table?.Rows?.Count > 0
                ? Utils.GetNullableLong(table.Rows[0], "ID")
                : null;
            return userId;
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

        // TODO remove. Cinepolis is not used
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
                if (dt != null && dt.DefaultView != null && dt.DefaultView.Count > 0)
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

        public static DataSet Get_UsersListByBulk(int groupId, string sFreeTxt, int top, int page)
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
                spUpdateUserDynamicData.SetConnectionKey("USERS_CONNECTION_STRING");

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

        public static DataTable Insert_LoginPIN(string siteGuid, string pinCode, int groupID, DateTime expired_date,
            string secret, int? pinUsages, long? pinDuration)
        {
            try
            {
                StoredProcedure sp = new StoredProcedure("Insert_LoginPIN");
                sp.SetConnectionKey("USERS_CONNECTION_STRING");
                sp.AddParameter("@groupID", groupID);
                sp.AddParameter("@siteGuid", siteGuid);
                sp.AddParameter("@pinCode", pinCode);
                sp.AddParameter("@expired_date", expired_date);
                sp.AddParameter("@secret", secret != null ? secret : string.Empty);
                sp.AddParameter("@usages", pinUsages == 0 ? -1 : pinUsages); //unlimited
                sp.AddParameter("@duration", pinDuration == 0 ? -1 : pinDuration); //unlimited
                DataSet ds = sp.ExecuteDataSet();
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
                sp.SetConnectionKey("USERS_CONNECTION_STRING");
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
                sp.SetConnectionKey("USERS_CONNECTION_STRING");
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
                sp.SetConnectionKey("USERS_CONNECTION_STRING");
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
                sp.SetConnectionKey("USERS_CONNECTION_STRING");
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
                sp.SetConnectionKey("USERS_CONNECTION_STRING");
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

        public static bool UpdateLoginPinStatusByPinCode(int groupID, string siteGuid, string pinCode)
        {
            try
            {
                StoredProcedure sp = new StoredProcedure("Update_LoginPinStatusByPinCode");
                sp.SetConnectionKey("USERS_CONNECTION_STRING");
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

        public static DataTable Get_FavoriteMediaIds(string userId, List<int> mediaIds, string udid, string mediaType, int orderBy = 0)
        {
            try
            {
                StoredProcedure sp = new StoredProcedure("Get_FavoriteMediaIds");
                sp.SetConnectionKey("USERS_CONNECTION_STRING");
                sp.AddParameter("@user_id", userId);
                sp.AddParameter("@udid", !string.IsNullOrEmpty(udid) ? udid : null);
                sp.AddParameter("@media_type", !string.IsNullOrEmpty(mediaType) ? mediaType : null);
                sp.AddParameter("@order_by", orderBy);
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

        public static List<long> Get_UserRoleIds(int groupId, string userId)
        {
            List<long> roleIds = new List<long>();

            try
            {
                StoredProcedure sp = new StoredProcedure("Get_UserRoleIds");
                sp.SetConnectionKey("USERS_CONNECTION_STRING");
                sp.AddParameter("@user_id", userId);
                sp.AddParameter("@group_id", groupId);
                DataSet ds = sp.ExecuteDataSet();

                if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
                {
                    DataTable dt = ds.Tables[0];
                    if (dt != null)
                    {
                        if (dt.Rows != null && dt.Rows.Count > 0)
                        {
                            foreach (DataRow row in dt.Rows)
                            {
                                roleIds.Add(ODBCWrapper.Utils.GetLongSafeVal(row["ROLE_ID"]));
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }
            return roleIds;
        }

        public static int Insert_UserRole(int groupId, string userId, long roleId, bool isSingle)
        {
            int rowCount;

            try
            {
                StoredProcedure sp = new StoredProcedure("Insert_UserRole");
                sp.SetConnectionKey("USERS_CONNECTION_STRING");
                sp.AddParameter("@user_id", userId);
                sp.AddParameter("@group_id", groupId);
                sp.AddParameter("@role_id", roleId);
                sp.AddParameter("@is_single", isSingle ? 1 : 0);
                rowCount = sp.ExecuteReturnValue<int>();

            }
            catch (Exception)
            {
                return 0;
            }

            return rowCount;
        }

        public static int Upsert_SuspendedUsersRole(int groupId, List<int> usersId, long currentRoleId, long newRoleId)
        {
            int rowCount;

            try
            {
                StoredProcedure sp = new StoredProcedure("Upsert_SuspendedUsersRole");
                sp.SetConnectionKey("USERS_CONNECTION_STRING");
                sp.AddIDListParameter<int>("@users_id", usersId, "Id");
                sp.AddParameter("@group_id", groupId);
                sp.AddParameter("@current_role_id", currentRoleId);
                sp.AddParameter("@new_role_id", newRoleId);

                rowCount = sp.ExecuteReturnValue<int>();

            }
            catch (Exception)
            {
                return 0;
            }
            return rowCount;

        }

        public static bool DeleteUser(int groupId, int userId)
        {
            int status = 0;
            bool ret = false;

            try
            {
                ODBCWrapper.StoredProcedure spRemoveDomain = new ODBCWrapper.StoredProcedure(DELETE_USER);
                spRemoveDomain.SetConnectionKey("USERS_CONNECTION_STRING");
                spRemoveDomain.AddParameter("@userId", userId);
                spRemoveDomain.AddParameter("@groupId", groupId);
                status = spRemoveDomain.ExecuteReturnValue<int>();

                return status == 2;
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return ret;
        }


        public static bool IsUserDomainMaster(int groupId, int userId)
        {
            bool result = false;

            try
            {
                StoredProcedure sp = new StoredProcedure("IsUserDomainMaster");
                sp.SetConnectionKey("USERS_CONNECTION_STRING");
                sp.AddParameter("@user_id", userId);
                sp.AddParameter("@group_id", groupId);
                DataSet ds = sp.ExecuteDataSet();

                if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
                {
                    DataTable dt = ds.Tables[0];
                    if (dt != null)
                    {
                        if (dt.Rows != null && dt.Rows.Count > 0)
                        {
                            if (ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0]["is_master"]) == 1)
                            {
                                result = true;
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
            return result;
        }

        public static DataTable Get_UserFavorites(string sUserGUID, string sUDID, int nType, int orderBy = 0)
        {
            StoredProcedure sp = new StoredProcedure("Get_UserFavorites");
            sp.SetConnectionKey("USERS_CONNECTION_STRING");
            sp.AddParameter("@SiteGUID", sUserGUID);
            sp.AddParameter("@MediaTypeID", nType);
            sp.AddParameter("@order_by", orderBy);

            if (!string.IsNullOrEmpty(sUDID))
                sp.AddParameter("@UDID", sUDID);

            DataSet ds = sp.ExecuteDataSet();

            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
            {
                return ds.Tables[0];
            }

            return null;
        }

        public static int DeleteItemFromUserList(int itemId, int listType, int itemType, string userId, int groupId)
        {
            int rowCount = 0;

            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("DeleteItemFromUserList");
                sp.SetConnectionKey("USERS_CONNECTION_STRING");
                sp.AddParameter("@userID", userId);
                sp.AddParameter("@itemID", itemId);
                sp.AddParameter("@listType", listType);
                sp.AddParameter("@itemType", itemType);
                sp.AddParameter("@groupID", groupId);
                rowCount = sp.ExecuteReturnValue<int>();
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return rowCount;
        }

        public static DataTable InsertItemToUserList(int userId, int order, int itemId, int listType, int itemType, int groupId)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("InsertItemToUserList");
            sp.SetConnectionKey("USERS_CONNECTION_STRING");
            sp.AddParameter("@userID", userId);
            sp.AddParameter("@listType", listType);
            sp.AddParameter("@itemType", itemType);
            sp.AddParameter("@order", order);
            sp.AddParameter("@itemID", itemId);
            sp.AddParameter("@groupID", groupId);
            DataSet ds = sp.ExecuteDataSet();

            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
            {
                return ds.Tables[0];
            }

            return null;
        }

        public static DataTable GetItemFromUserList(int userId, int itemId, int listType, int itemType, int groupId)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("GetItemFromUserList");
            sp.SetConnectionKey("USERS_CONNECTION_STRING");
            sp.AddParameter("@userID", userId);
            sp.AddParameter("@listType", listType);
            sp.AddParameter("@itemType", itemType);
            sp.AddParameter("@itemID", itemId);
            sp.AddParameter("@groupID", groupId);
            DataSet ds = sp.ExecuteDataSet();

            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
            {
                return ds.Tables[0];
            }

            return null;
        }

        public static DataTable GetCountryById(int id)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("GetCountry");
            sp.SetConnectionKey("USERS_CONNECTION_STRING");
            sp.AddParameter("@id", id);
            DataSet ds = sp.ExecuteDataSet();

            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
            {
                return ds.Tables[0];
            }

            return null;
        }

        public static long GetUserIDByExternalId(int groupId, string externalId)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("GetUserIDByExternalId");
            sp.SetConnectionKey("USERS_CONNECTION_STRING");
            sp.AddParameter("@groupId", groupId);
            sp.AddParameter("@externalId", externalId);
            return sp.ExecuteReturnValue<long>();
        }
        
        public static string GetExternalIdByUserId(int groupId, long userId)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("GetExternalIdByUserId");
            sp.SetConnectionKey("USERS_CONNECTION_STRING");
            sp.AddParameter("@groupId", groupId);
            sp.AddParameter("@userId", userId);
            
            var result = sp.ExecuteDataSet();
            var tbl = result.Tables[0];
            if (tbl != null && tbl.Rows?.Count > 0)
            {
                return (string)result.Tables[0].Rows[0][0];
            }

            return string.Empty;
        }

        public static int GetGroupAdapterId(int groupId)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("GetGroupAdapterId");
            sp.SetConnectionKey("USERS_CONNECTION_STRING");
            sp.AddParameter("@groupId", groupId);
            return sp.ExecuteReturnValue<int>();
        }

        public static int Purge()
        {
            int sqlCommandTimeoutSec = SQL_COMMAND_TIMEOUT_SEC;
            if (ApplicationConfiguration.Current.DatabaseConfiguration.DbCommandExecuteTimeoutSec.Value > 0)
            {
                sqlCommandTimeoutSec = ApplicationConfiguration.Current.DatabaseConfiguration.DbCommandExecuteTimeoutSec.Value;
            }

            var sp = new StoredProcedure("__Purge", true);
            sp.SetConnectionKey("USERS_CONNECTION_STRING");
            sp.SetTimeout(sqlCommandTimeoutSec);
            return sp.ExecuteReturnValue<int>();
        }

        public static IEnumerable<SSOAdapter> GetSSOAdapters(int groupId)
        {
            if (CanaryDeploymentFactory.Instance.GetMicroservicesCanaryDeploymentManager().IsDataOwnershipFlagEnabled(groupId, CanaryDeploymentDataOwnershipEnum.AuthenticationSSOAdapterProfiles))
            {
                var authClient = AuthenticationClient.GetClientFromTCM();
                var ssoAdapters = authClient.ListSSOAdapterProfiles(groupId);
                if (ssoAdapters?.SSOAdapterProfiles != null)
                {
                    var ssoAdaptersResponse = ssoAdapters.SSOAdapterProfiles.Select(s => new SSOAdapter
                    {
                        Id = (int)s.Id,
                        ExternalIdentifier = s.ProfileData.ExternalId,
                        AdapterUrl = s.ProfileData.AdapterUrl,
                        GroupId = groupId,
                        IsActive = s.ProfileData.IsActive ? 1 : 0,
                        Name = s.ProfileData.Name,
                        SharedSecret = s.ProfileData.SharedSecret,
                        Settings = ConvertSSOConfigDictionaryToListOfSSOParams(s.Id, groupId, s.ProfileData.SSOConfiguration),
                    });
                    return ssoAdaptersResponse;
                }
                else
                {
                    return null;
                }
            }

            var sp = new StoredProcedure("Get_GroupSSOAdapters");
            sp.SetConnectionKey("USERS_CONNECTION_STRING");
            sp.AddParameter("@groupId", groupId);
            var result = sp.ExecuteDataSet();

            var ssoAdapaterTbl = result.Tables[0];
            var ssoAdaptersConfig = result.Tables[1];

            var adapterConfigsList = ssoAdaptersConfig.ToList<SSOAdapterParam>();
            var adaptersList = ssoAdapaterTbl.ToList<SSOAdapter>()
                .Select(a =>
                {
                    a.Settings = adapterConfigsList.Where(c => c.AdapterId == a.Id).ToList();
                    return a;
                });

            return adaptersList;
        }

        private static IList<SSOAdapterParam> ConvertSSOConfigDictionaryToListOfSSOParams(long adapterId, int groupId, IDictionary<string, string> ssoConfiguration)
        {
            if (ssoConfiguration == null) { return null; }
            var response = ssoConfiguration.Select(kv => new SSOAdapterParam
            {
                AdapterId = (int)adapterId,
                GroupId = groupId,
                Key = kv.Key,
                Value = kv.Value,

            }).ToList();
            return response;
        }

        public static SSOAdapter AddSSOAdapters(SSOAdapter adapterDetails, int updaterId)
        {
            if (CanaryDeploymentFactory.Instance.GetMicroservicesCanaryDeploymentManager().IsDataOwnershipFlagEnabled(adapterDetails.GroupId, CanaryDeploymentDataOwnershipEnum.AuthenticationSSOAdapterProfiles))
            {
                log.Error(SSO_ADAPTER_WONERSHIP_ERR_MSG);
                throw new Exception(SSO_ADAPTER_WONERSHIP_ERR_MSG);
            }

            var sp = new StoredProcedure("Insert_SSOAdapter");
            sp.SetConnectionKey("USERS_CONNECTION_STRING");
            sp.AddParameter("@groupId", adapterDetails.GroupId);
            sp.AddParameter("@name", adapterDetails.Name);
            sp.AddParameter("@isActive", adapterDetails.IsActive);
            sp.AddParameter("@status", 1);
            sp.AddParameter("@updaterId", updaterId);
            sp.AddParameter("@adapterUrl", adapterDetails.AdapterUrl);
            sp.AddParameter("@externalId", adapterDetails.ExternalIdentifier);
            sp.AddParameter("@sharedSecret", adapterDetails.SharedSecret);
            var result = sp.ExecuteDataSet();
            var adapterId = (int)result.Tables[0].Rows[0][0];
            adapterDetails.Id = adapterId;

            MergeSSOAdapaterSettings(adapterDetails.GroupId, adapterId, updaterId, adapterDetails.Settings);

            return adapterDetails;
        }

        public static SSOAdapter UpdateSSOAdapter(SSOAdapter adapterDetails, int updaterId)
        {
            if (CanaryDeploymentFactory.Instance.GetMicroservicesCanaryDeploymentManager().IsDataOwnershipFlagEnabled(adapterDetails.GroupId, CanaryDeploymentDataOwnershipEnum.AuthenticationSSOAdapterProfiles))
            {
                var msg = "This code should not be called, ownership flag of SSOAdapters has been transfered to Authentication Service, Check TCM [MicroservicesClientConfiguration.Authentication.DataOwnershipConfiguration.SSOAdapterProfiles]";
                log.Error(msg);
                throw new Exception(msg);
            }

            var updateAdapterSp = new StoredProcedure("Update_SSOAdapter");
            updateAdapterSp.SetConnectionKey("USERS_CONNECTION_STRING");
            updateAdapterSp.AddParameter("@groupId", adapterDetails.GroupId);
            updateAdapterSp.AddParameter("@adapterId", adapterDetails.Id);
            updateAdapterSp.AddParameter("@name", adapterDetails.Name);
            updateAdapterSp.AddParameter("@isActive", adapterDetails.IsActive);
            updateAdapterSp.AddParameter("@status", 1);
            updateAdapterSp.AddParameter("@updaterId", updaterId);
            updateAdapterSp.AddParameter("@adapterUrl", adapterDetails.AdapterUrl);
            updateAdapterSp.AddParameter("@externalId", adapterDetails.ExternalIdentifier);
            updateAdapterSp.AddParameter("@sharedSecret", adapterDetails.SharedSecret);

            var resp = updateAdapterSp.ExecuteDataSet();
            var updatedSettings = MergeSSOAdapaterSettings(adapterDetails.GroupId, adapterDetails.Id.Value, updaterId, adapterDetails.Settings);

            var updatedAdapater = resp.Tables[0].ToList<SSOAdapter>().FirstOrDefault();
            if (updatedAdapater == null) { return null; }

            updatedAdapater.Settings = updatedSettings;
            return updatedAdapater;
        }

        private static IList<SSOAdapterParam> MergeSSOAdapaterSettings(int groupId, int adapaterId, int updaterId, IList<SSOAdapterParam> settings)
        {
            var settingsTbl = settings.Select(s => new KeyValuePair<string, string>(s.Key, s.Value)).ToList();
            var mergeAdapterSettingsSp = new StoredProcedure("Merge_SSOAdapterSettings");
            mergeAdapterSettingsSp.SetConnectionKey("USERS_CONNECTION_STRING");
            mergeAdapterSettingsSp.AddParameter("@groupId", groupId);
            mergeAdapterSettingsSp.AddParameter("@adapterId", adapaterId);
            mergeAdapterSettingsSp.AddParameter("@updaterId", updaterId);
            mergeAdapterSettingsSp.AddKeyValueListParameter("@KeyValueList", settingsTbl, "key", "value");
            var resp = mergeAdapterSettingsSp.ExecuteDataSet();
            var ssoParams = resp.Tables[0].ToList<SSOAdapterParam>();
            return ssoParams;
        }

        public static bool DeleteSSOAdapter(int groupId, int ssoAdapterId, int updaterId)
        {
            if (CanaryDeploymentFactory.Instance.GetMicroservicesCanaryDeploymentManager().IsDataOwnershipFlagEnabled(groupId, CanaryDeploymentDataOwnershipEnum.AuthenticationSSOAdapterProfiles))
            {
                log.Error(SSO_ADAPTER_WONERSHIP_ERR_MSG);
                throw new Exception(SSO_ADAPTER_WONERSHIP_ERR_MSG);
            }

            var updateAdapterSp = new StoredProcedure("Delete_SSOAdapter");
            updateAdapterSp.SetConnectionKey("USERS_CONNECTION_STRING");
            updateAdapterSp.AddParameter("@adapterId", ssoAdapterId);
            updateAdapterSp.AddParameter("@updaterId", updaterId);

            var updatedRows = updateAdapterSp.ExecuteReturnValue<int>();
            return updatedRows > 0;
        }

        public static SSOAdapter SetSharedSecret(int groupId, int ssoAdapterId, string sharedSecret, int updaterId)
        {
            if (CanaryDeploymentFactory.Instance.GetMicroservicesCanaryDeploymentManager().IsDataOwnershipFlagEnabled(groupId, CanaryDeploymentDataOwnershipEnum.AuthenticationSSOAdapterProfiles))
            {
                log.Error(SSO_ADAPTER_WONERSHIP_ERR_MSG);
                throw new Exception(SSO_ADAPTER_WONERSHIP_ERR_MSG);
            }

            var updateAdapterSp = new StoredProcedure("Set_SSOAdapterSecret");
            updateAdapterSp.SetConnectionKey("USERS_CONNECTION_STRING");
            updateAdapterSp.AddParameter("@adapterId", ssoAdapterId);
            updateAdapterSp.AddParameter("@sharedSecret", sharedSecret);
            updateAdapterSp.AddParameter("@updaterId", updaterId);

            var dsResult = updateAdapterSp.ExecuteDataSet();
            var updatedAdapater = dsResult.Tables[0].ToList<SSOAdapter>().FirstOrDefault();
            return updatedAdapater;
        }

        public static SSOAdapter GetSSOAdapterByExternalId(int groupId, string ssoAdapterExternalId)
        {
            // this method is called suring Add\update adapater to verify unique external id
            if (CanaryDeploymentFactory.Instance.GetMicroservicesCanaryDeploymentManager().IsDataOwnershipFlagEnabled(groupId, CanaryDeploymentDataOwnershipEnum.AuthenticationSSOAdapterProfiles))
            {
                log.Error(SSO_ADAPTER_WONERSHIP_ERR_MSG);
                throw new Exception(SSO_ADAPTER_WONERSHIP_ERR_MSG);
            }

            var updateAdapterSp = new StoredProcedure("Get_SSOAdapterByExternalId");
            updateAdapterSp.SetConnectionKey("USERS_CONNECTION_STRING");
            updateAdapterSp.AddParameter("@externalId", ssoAdapterExternalId);

            var dsResult = updateAdapterSp.ExecuteDataSet();
            var updatedAdapater = dsResult.Tables[0].ToList<SSOAdapter>().FirstOrDefault();
            return updatedAdapater;

        }

        public static List<long> GetUserIdsByRoleIds(int groupId, HashSet<long> roleIds)
        {
            List<long> userIds = null;
            try
            {
                StoredProcedure sp = new StoredProcedure("GetUserIdsByRoleIds");
                sp.SetConnectionKey("USERS_CONNECTION_STRING");
                sp.AddParameter("@groupId", groupId);
                sp.AddIDListParameter("@roleIds", roleIds, "id");
                DataTable dt = sp.Execute();

                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    userIds = new List<long>(dt.Rows.Count);

                    foreach (DataRow row in dt.Rows)
                    {
                        userIds.Add(Utils.GetLongSafeVal(row, "USER_ID"));
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while GetUserIdsByRoleIds in DB, groupId: {0}, roleIds:{1}, ex:{2} ", groupId, string.Join(", ", roleIds), ex);
            }

            return userIds;
        }

        private static string GetUserRolesToPasswordPolicyKey(int groupId)
        {
            return string.Format("user_roles_to_password_policy_{0}", groupId);
        }

        public static Dictionary<long, HashSet<long>> GetUserRolesToPasswordPolicy(int groupId)
        {
            var key = GetUserRolesToPasswordPolicyKey(groupId);
            return UtilsDal.GetObjectFromCB<Dictionary<long, HashSet<long>>>(eCouchbaseBucket.OTT_APPS, key);
        }

        private static string GetPasswordPolicyKey(long passwordPolicyId)
        {
            return string.Format("password_policy_{0}", passwordPolicyId);
        }

        public static PasswordPolicy GetPasswordPolicy(long passwordPolicyId)
        {
            var key = GetPasswordPolicyKey(passwordPolicyId);
            return UtilsDal.GetObjectFromCB<PasswordPolicy>(eCouchbaseBucket.OTT_APPS, key);
        }

        public static bool SavePasswordPolicy(PasswordPolicy policy)
        {
            string key = GetPasswordPolicyKey(policy.Id);
            return UtilsDal.SaveObjectInCB<PasswordPolicy>(eCouchbaseBucket.OTT_APPS, key, policy);
        }

        public static bool SaveUserRolesToPasswordPolicy(int groupId, Dictionary<long, HashSet<long>> policies)
        {
            var key = GetUserRolesToPasswordPolicyKey(groupId);
            return UtilsDal.SaveObjectInCB(eCouchbaseBucket.OTT_APPS, key, policies);
        }

        public static bool DeletePasswordPolicy(int groupId, long id)
        {
            string assetRuleKey = GetPasswordPolicyKey(id);
            return UtilsDal.DeleteObjectFromCB(eCouchbaseBucket.OTT_APPS, assetRuleKey);
        }

        #region Password History
        private static string GetPasswordsHistoryKey(long userId)
        {
            return string.Format("user_passwords_history_{0}", userId);
        }

        private static string GetHashedPasswordHistoryKey(long userId)
        {
            return string.Format("user_passwords_history_V2_{0}", userId);
        }

        // TODO should be removed when all passwords history will be converted to hashed version(BEO-8869)
        public static HashSet<string> GetPasswordsHistory(long userId)
        {
            var key = GetPasswordsHistoryKey(userId);
            return UtilsDal.GetObjectFromCB<HashSet<string>>(eCouchbaseBucket.OTT_APPS, key);
        }

        public static bool SavePasswordsHistory(long userId, HashSet<string> passwordsHistory)
        {
            var key = GetPasswordsHistoryKey(userId);
            return UtilsDal.SaveObjectInCB(eCouchbaseBucket.OTT_APPS, key, passwordsHistory);
        }

        public static bool DeletePasswordsHistory(long userId)
        {
            return UtilsDal.DeleteObjectFromCB(eCouchbaseBucket.OTT_APPS, GetPasswordsHistoryKey(userId));
        }

        public static PasswordHistory GetHashedPasswordHistory(long userId)
        {
            return UtilsDal.GetObjectFromCB<PasswordHistory>(eCouchbaseBucket.OTT_APPS, GetHashedPasswordHistoryKey(userId));
        }

        public static bool SaveHashedPasswordHistory(long userId, PasswordHistory passwordHistory)
        {
            return UtilsDal.SaveObjectInCB(eCouchbaseBucket.OTT_APPS, GetHashedPasswordHistoryKey(userId), passwordHistory);
        }
        #endregion Password History

        public static bool DeleteUserRolesToPasswordPolicy(int groupId, Dictionary<long, HashSet<long>> policies)
        {
            var key = GetUserRolesToPasswordPolicyKey(groupId);
            return UtilsDal.SaveObjectInCB(eCouchbaseBucket.OTT_APPS, key, policies);
        }

        public static bool RevokePasswordToken(int userId)
        {
            ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("users");
            updateQuery.SetConnectionKey("USERS_CONNECTION_STRING");
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("CP_TOKEN", "=", DBNull.Value);
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("CP_TOKEN_LAST_DATE", "=", DateTime.UtcNow.AddDays(-15));
            updateQuery += " where ";
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", userId);
            bool res = updateQuery.Execute();
            updateQuery.Finish();
            updateQuery = null;

            return res;
        }

        public static int GetUserIdByToken(int groupId, string token)
        {
            int nRet = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetConnectionKey("USERS_CONNECTION_STRING");
            selectQuery += "select id from users where status=1 and is_active=1 and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("CP_TOKEN", "=", token);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", groupId);
            selectQuery += "and CP_TOKEN_LAST_DATE>getdate()";
            if (selectQuery.Execute("query", true) != null)
            {
                int nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    nRet = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return nRet;
        }

        public static List<EncryptionKey> GetEncryptionKeys(int groupId)
        {
            var sp = new StoredProcedure("List_encryption_keys");
            sp.SetConnectionKey("USERS_CONNECTION_STRING");
            sp.AddParameter("@groupId", groupId);
            var table = sp.ExecuteDataSet().Tables[0];

            var list = new List<EncryptionKey>(table.Rows.Count);
            foreach (DataRow row in table.Rows)
            {
                list.Add(new EncryptionKey(
                    Utils.GetLongSafeVal(row, "ID"),
                    Utils.GetIntSafeVal(row, "group_id"),
                    Convert.FromBase64String(Utils.GetSafeStr(row, "value")),
                    (EncryptionType)Utils.GetIntSafeVal(row, "encryption_type")
                ));
            }

            return list;
        }

        public static int InsertEncryptionKey(EncryptionKey encryptionKey, long updaterId)
        {
            try
            {
                var keyAsString = Convert.ToBase64String(encryptionKey.Value);
                var sp = new StoredProcedure("Insert_encryption_key");
                sp.SetConnectionKey("USERS_CONNECTION_STRING");
                sp.AddParameter("@groupId", encryptionKey.GroupId);
                sp.AddParameter("@type", (int)encryptionKey.Type);
                sp.AddParameter("@value", keyAsString, true);
                sp.AddParameter("@updaterId", updaterId);

                return sp.ExecuteReturnValue<int>();
            }
            catch (Exception ex)
            {
                log.Error($"InsertEncryptionKey in DB, groupId: {encryptionKey.GroupId}, ex:{ex.Message}", ex);
                return 0;
            }
        }

        public bool SetupPartnerInDb(long partnerId, long updaterId)
        {
            var sp = new StoredProcedure("Create_GroupBasicData");
            sp.SetConnectionKey(USERS_CONNECTION_STRING);
            sp.AddParameter("@groupId", partnerId);
            sp.AddParameter("@updaterId", updaterId);
            // TODO IS_ACTIVATION_NEEDED
            // TODO ALLOW_DELETE_USER

            return sp.ExecuteReturnValue<int>() > 0;
        }

        public bool DeletePartnerDb(long partnerId, long updaterId)
        {
            var sp = new StoredProcedure("Delete_GroupBasicData");
            sp.SetConnectionKey(USERS_CONNECTION_STRING);
            sp.AddParameter("@groupId", partnerId);
            sp.AddParameter("@updaterId", updaterId);

            return sp.ExecuteReturnValue<int>() > 0;
        }
    }
}