using ApiLogic.Users.Managers;
using ApiLogic.Users.Security;
using ApiObjects;
using ApiObjects.Base;
using CachingProvider.LayeredCache;
using Phx.Lib.Appconfig;
using DAL;
using Phx.Lib.Log;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;
using ApiObjects.User;
using TVinciShared;

namespace Core.Users
{
    /// <summary>
    /// This class represents user object
    /// </summary>
    [Serializable]
    [JsonObject(Id = "User")]
    public class User : CoreObject
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        [JsonProperty()]
        public UserBasicData m_oBasicData;

        [JsonProperty()]
        public UserDynamicData m_oDynamicData;

        [JsonProperty()]
        public UserState m_eUserState;

        [JsonProperty()]
        public DomainSuspentionStatus m_eSuspendState;

        public string m_sSiteGUID;
        public int m_domianID;
        public bool m_isDomainMaster;
        public int m_nSSOOperatorID;
        public bool IsActivationGracePeriod;

        [XmlIgnore]
        [JsonIgnore()]
        public bool shouldSetUserActive;

        [XmlIgnore]
        [JsonIgnore()]
        public bool resetFailCount;

        [XmlIgnore]
        [JsonIgnore()]
        public bool shouldRemoveFromCache;

        [XmlIgnore]
        [JsonIgnore()]
        protected int userId;

        [XmlIgnore]
        [JsonIgnore()]
        public string activationToken;

        [XmlIgnore]
        [JsonIgnore()]
        private bool UpdateUserPassword;

        public User()
        {
            m_oBasicData = new UserBasicData();
            m_oDynamicData = new UserDynamicData();
            m_sSiteGUID = "";
            m_eUserState = UserState.Unknown;
            m_eSuspendState = DomainSuspentionStatus.OK;
        }

        public User(int nGroupID, int nUserID, bool shouldSaveInCache = false)
            : this()
        {
            Initialize(nUserID, nGroupID);
        }

        public User Clone()
        {
            return TVinciShared.ObjectCopier.Clone<User>(this);
        }

        public override CoreObject CoreClone()
        {
            return this.Clone();
        }

        public static User GetUser(int nUserID, int nGroupID)
        {
            User user = new User(nGroupID, nUserID);
            return user;
        }

        public static UserState DoUserAction(int siteGuid, int groupID, string sessionID, string sIP, string sIDInDevices, UserState currentState, UserAction action, bool needActivation, ref int instanceID)
        {
            UserState retVal = UserState.Unknown;
            switch (currentState)
            {
                case UserState.Activated:
                    if (action == UserAction.SignIn)
                    {
                        instanceID = AddUserSession(siteGuid, sessionID, sIP, sIDInDevices);
                        retVal = UserState.SingleSignIn;
                        break;
                    }
                    break;
                case UserState.SingleSignIn:
                    if (action == UserAction.SignIn)
                    {
                        instanceID = AddUserSession(siteGuid, sessionID, sIP, sIDInDevices);
                        retVal = UserState.SingleSignIn;
                    }
                    else if (action == UserAction.SignOut)
                    {
                        instanceID = UpdateUserSession(siteGuid, sessionID, sIP, sIDInDevices, true);
                        int numOfLogins = getNumOfUserLogins(siteGuid);
                        if (numOfLogins == 0)
                        {
                            retVal = UserState.LoggedOut;
                        }
                        else
                        {
                            retVal = UserState.SingleSignIn;
                        }
                    }
                    break;
                case UserState.DoubleSignIn:
                    {
                        if (action == UserAction.SignIn)
                        {
                            instanceID = AddUserSession(siteGuid, sessionID, sIP, sIDInDevices);
                            retVal = UserState.DoubleSignIn;
                        }
                        else if (action == UserAction.SignOut)
                        {
                            instanceID = UpdateUserSession(siteGuid, sessionID, sIP, sIDInDevices, true);
                            int nCountOfActiveUserSessions = UsersDal.Get_CountOfActiveUserSessions(siteGuid);
                            switch (nCountOfActiveUserSessions)
                            {
                                case 0:
                                    retVal = UserState.LoggedOut;
                                    break;
                                case 1:
                                    retVal = UserState.SingleSignIn;
                                    break;
                                default:
                                    retVal = UserState.DoubleSignIn;
                                    break;
                            }
                        }
                        break;
                    }
                case UserState.LoggedOut:
                case UserState.Unknown:
                    {
                        if (action == UserAction.SignIn)
                        {
                            retVal = UsersDal.Get_CountOfActiveUserSessions(siteGuid) > 0 ? UserState.DoubleSignIn : UserState.SingleSignIn;
                            instanceID = AddUserSession(siteGuid, sessionID, sIP, sIDInDevices);
                        }
                        break;
                    }
            }
            UsersDal.Update_UserStateInUsers(siteGuid, (int)retVal);

            // Remove user from cache
            UsersCache usersCache = UsersCache.Instance();
            usersCache.RemoveUser(siteGuid, groupID);

            return retVal;
        }

        private static void UpdateUserState(int siteGuid, int state)
        {
            ODBCWrapper.UpdateQuery updateQUery = new ODBCWrapper.UpdateQuery("users");
            updateQUery += ODBCWrapper.Parameter.NEW_PARAM("user_state", "=", state);
            updateQUery += " where ";
            updateQUery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", siteGuid);
            updateQUery.Execute();
            updateQUery.Finish();
            updateQUery = null;
        }

        public static UserState GetCurrentUserState(int siteGuid, int groupID, bool shouldGetFromCache = true)
        {
            UserState retVal = UserState.Unknown;
            int nUserState = 0;
            User user = null;

            if (shouldGetFromCache)
            {
                //Get user from cache by siteGUID
                UsersCache usersCache = UsersCache.Instance();
                user = usersCache.GetUser(siteGuid, groupID);
            }

            if (user != null)
            {
                retVal = user.m_eUserState;
            }
            else
            {
                nUserState = UsersDal.Get_UserStateFromUsers(siteGuid);
                if (Enum.IsDefined(typeof(UserState), nUserState))
                {
                    retVal = (UserState)nUserState;
                }
            }

            return retVal;
        }

        public static UserState GetCurrentUserInstanceState(int siteGuid, string sessionID, string sIP, string deviceID, int nGroupID)
        {
            UserState retVal = UserState.Unknown;
            int userSessionID = 0;

            var lIDInDevices = string.IsNullOrEmpty(deviceID) ? 0 : DeviceDal.GetDeviceIdByUDID(deviceID, nGroupID);

            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select is_active, id from users_sessions with (nolock)  where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("user_site_guid", "=", siteGuid);
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("session_id", "=", sessionID);
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("user_ip", "=", sIP);
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("device_id", "=", lIDInDevices > 0 ? lIDInDevices + "" : string.Empty);
            if (selectQuery.Execute("query", true) != null)
            {
                int count = selectQuery.Table("query").DefaultView.Count;
                if (count > 0)
                {
                    int isActive = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "is_active", 0);
                    if (isActive == 1)
                    {
                        userSessionID = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "id", 0);
                        retVal = UserState.SingleSignIn;
                    }
                    else
                    {
                        retVal = UserState.LoggedOut;
                    }
                }
            }
            selectQuery.Finish();
            selectQuery = null;

            if (userSessionID > 0)
            {
                ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("users_sessions");
                updateQuery += "last_action_date = getdate()";
                updateQuery += " where ";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", userSessionID);
                updateQuery.Execute();
                updateQuery.Finish();
            }
            return retVal;
        }

        public int Update(UserBasicData oBasicData, UserDynamicData oDynamicData, Int32 nGroupID)
        {
            if (!string.IsNullOrEmpty(m_sSiteGUID))
            {
                //Try Getting user current object so we return the current values on the response
                int currentUserID;
                if (int.TryParse(m_sSiteGUID, out currentUserID))
                {
                    UsersCache usersCache = UsersCache.Instance();
                    User user = usersCache.GetUser(currentUserID, nGroupID);
                    if (user != null)
                    {
                        m_domianID = user.m_domianID;
                        m_eSuspendState = user.m_eSuspendState;
                        m_eUserState = user.m_eUserState;
                        m_isDomainMaster = user.m_isDomainMaster;
                        m_nSSOOperatorID = user.m_nSSOOperatorID;
                    }
                }

                //Update basic and dynamic data
                this.m_oBasicData.CopyForUpdate(oBasicData, false);
                this.m_oDynamicData = oDynamicData;
            }

            return SaveForUpdate(nGroupID);
        }

        public void UpdateDynamicData(UserDynamicData oDynamicData, Int32 nGroupID)
        {
            if (m_sSiteGUID != "")
            {
                m_oDynamicData = oDynamicData;
                SaveDynamicData(nGroupID);

                try
                {
                    // Remove user from cache
                    UsersCache usersCache = UsersCache.Instance();
                    usersCache.RemoveUser(int.Parse(m_sSiteGUID), nGroupID);
                }
                catch (Exception ex)
                {

                    log.Error(string.Format("Failed removing user {0} from cache", m_sSiteGUID), ex);
                }

            }
        }

        public static int GetNextGUID()
        {
            int retVal = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += " select max(id) as 'id' from users with (nolock)";
            if (selectQuery.Execute("query", true) != null)
            {
                int count = selectQuery.Table("query").DefaultView.Count;
                if (count > 0)
                {
                    retVal = int.Parse(selectQuery.Table("query").DefaultView[0].Row["id"].ToString()) + 1;
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return retVal;
        }

        public ResponseStatus SetUserTypeByUserID(Int32 nGroupID, string sSiteGUID, Int32 nUserTypeID)
        {
            ResponseStatus ret = ResponseStatus.OK;
            try
            {
                int userID = 0;
                bool parseResult = int.TryParse(sSiteGUID, out userID);

                if (parseResult == true)
                {
                    if (IsUserTypeExist(nGroupID, nUserTypeID) == true)
                    {
                        bool result = UsersDal.UpdateUserTypeByUserID(userID, nUserTypeID);
                        if (!result)
                        {
                            ret = ResponseStatus.ErrorOnUpdatingUserType;
                        }
                    }
                    else
                    {
                        ret = ResponseStatus.UserTypeNotExist;
                    }
                }
                else
                {
                    ret = ResponseStatus.UserDoesNotExist;
                }
            }
            catch
            {
                ret = ResponseStatus.ErrorOnUpdatingUserType;
            }

            try
            {
                // Remove user from cache
                UsersCache usersCache = UsersCache.Instance();
                usersCache.RemoveUser(int.Parse(m_sSiteGUID), nGroupID);
            }
            catch (Exception)
            {

                throw;
            }


            return ret;
        }

        private static int getNumOfUserLogins(int nSiteGuid)
        {
            return UsersDal.Get_CountOfActiveUserSessions(nSiteGuid);
        }

        private static int getUserLimit(int nGroupID)
        {
            int retVal = DAL.UsersDal.GetAllowedLogins(nGroupID);
            return retVal;
        }

        public bool InitializeNewUser(UserBasicData userBasicData, UserDynamicData dynamicData, int groupID, string password)
        {
            try
            {
                userBasicData.SetPassword(password, groupID);

                UserResponseObject u = CheckUserPassword(userBasicData.m_sUserName, userBasicData.m_sPassword, 3, 3, groupID, false, true);
                if (u.m_RespStatus == ResponseStatus.WrongPasswordOrUserName)
                {
                    return false;
                }

                m_sSiteGUID = (u != null && u.m_user != null) ? u.m_user.m_sSiteGUID : "";

                m_oBasicData = userBasicData;
                m_oDynamicData = dynamicData;
                if (!string.IsNullOrEmpty(m_sSiteGUID))
                {
                    m_domianID = UsersDal.GetUserDomainID(m_sSiteGUID, ref m_nSSOOperatorID, ref m_isDomainMaster, ref m_eSuspendState);
                }

                return true;
            }
            catch (Exception ex)
            {
                log.Error($"Exception at User.InitializeNewUser. Basic Data: {userBasicData.ToString()}, groupID:{groupID}, Msg: {ex.Message}, Stack Trace:{ex.StackTrace}");
            }

            return false;
        }

        public void InitializeBasicAndDynamicData(UserBasicData oBasicData, UserDynamicData oDynamicData)
        {
            this.m_oBasicData = oBasicData;
            this.m_oDynamicData = oDynamicData;
        }

        public bool Initialize(Int32 nUserID, Int32 nGroupID, bool shouldGoToDb = false)
        {
            bool result = false;

            try
            {
                if (shouldGoToDb)
                {
                    this.GroupId = nGroupID;

                    result = m_oBasicData.Initialize(nUserID, nGroupID);

                    if (!result)
                    {
                        return result;
                    }

                    result = m_oDynamicData.Initialize(nUserID, nGroupID);

                    m_sSiteGUID = nUserID.ToString();

                    m_domianID = UsersDal.GetUserDomainID(m_sSiteGUID, ref m_nSSOOperatorID, ref m_isDomainMaster, ref m_eSuspendState);

                    if (m_domianID <= 0)
                    {
                        m_domianID = DomainDal.GetDomainIDBySiteGuid(nGroupID, nUserID, ref m_nSSOOperatorID, ref m_isDomainMaster, ref m_eSuspendState);
                    }

                    m_eUserState = GetCurrentUserState(nUserID, nGroupID, false);

                    return result;
                }

                // Get user from cache by siteGUID
                UsersCache usersCache = UsersCache.Instance();
                var user = usersCache.GetUser(nUserID, nGroupID);
                if (user != null)
                {
                    m_oBasicData = user.m_oBasicData;
                    m_oDynamicData = user.m_oDynamicData;
                    m_sSiteGUID = user.m_sSiteGUID;
                    m_domianID = user.m_domianID;
                    m_eUserState = user.m_eUserState;
                    m_eSuspendState = user.m_eSuspendState;
                    m_nSSOOperatorID = user.m_nSSOOperatorID;
                    m_isDomainMaster = user.m_isDomainMaster;
                    m_eSuspendState = user.m_eSuspendState;
                    this.GroupId = nGroupID;

                    result = true;
                }
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder("Exception at User.Initialize(UserID, GroupID)");
                sb.Append(String.Concat(" Ex Msg: ", ex.Message));
                sb.Append(String.Concat(" User ID: ", nUserID));
                sb.Append(String.Concat(" Group ID: ", nGroupID));
                sb.Append(String.Concat(" Ex Type: ", ex.GetType().Name));
                sb.Append(String.Concat(" ST: ", ex.StackTrace));
                log.Error("Exception - " + sb.ToString(), ex);
                result = false;
            }

            return result;
        }

        public bool Initialize(Int32 nUserID, Int32 nGroupID, int domainID, bool isDomainMaster)
        {
            bool res = false;

            try
            {
                res = Initialize(nUserID, nGroupID);
                m_domianID = domainID;
                m_isDomainMaster = isDomainMaster;

                // Add user to cache
                //UsersCache usersCache = UsersCache.Instance();
                //usersCache.InsertUser(this, nGroupID);
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder("Exception at User.Initialize(UserID, GroupID, DomainID, IsDomainMaster)");
                sb.Append(String.Concat(" Ex Msg: ", ex.Message));
                sb.Append(String.Concat(" User ID: ", nUserID));
                sb.Append(String.Concat(" Group ID: ", nGroupID));
                sb.Append(String.Concat(" D ID: ", domainID));
                sb.Append(String.Concat(" Is Domain Master: ", isDomainMaster.ToString().ToLower()));
                sb.Append(String.Concat(" Ex Type: ", ex.GetType().Name));
                sb.Append(String.Concat(" ST: ", ex.StackTrace));
                log.Error("Exception - " + sb.ToString(), ex);
                res = false;
            }

            return res;
        }

        private UserType GetDefaultUserType(Int32 nGroupID)
        {
            int? nUserTypeID = null;
            string sUserTypeDesc = string.Empty;
            int nIsDefault = 0;

            string key = string.Format("users_GetGroupUserTypes_{0}_Default_1", nGroupID);
            UserType userType;
            bool bRes = UsersCache.GetItem<UserType>(key, out userType);

            if (!bRes)
            {
                DataTable dtUserData = UsersDal.GetUserTypeData(nGroupID, 1);
                if (dtUserData != null && dtUserData.Rows.Count > 0)
                {
                    nUserTypeID = ODBCWrapper.Utils.GetIntSafeVal(dtUserData.DefaultView[0]["ID"]);
                    sUserTypeDesc = ODBCWrapper.Utils.GetSafeStr(dtUserData.DefaultView[0]["description"]);
                    nIsDefault = ODBCWrapper.Utils.GetIntSafeVal(dtUserData.DefaultView[0]["is_default"]);
                }

                userType = new UserType(nUserTypeID, sUserTypeDesc, Convert.ToBoolean(nIsDefault));
                UsersCache.AddItem(key, userType);
            }

            return userType;
        }

        public int InitializeByFacebook(string sFacebookID, Int32 nGroupID)
        {
            Int32 nUserID = DAL.UsersDal.GetUserIDByFacebookID(sFacebookID, nGroupID);

            bool res = Initialize(nUserID, nGroupID);

            return nUserID;
        }

        public int InitializeByUsername(string sUsername, Int32 nGroupID)
        {
            Int32 nUserID = UserStorage.Instance().GetUserIDByUsername(sUsername, nGroupID);
            if (nUserID > 0)
            {
                bool bInit = Initialize(nUserID, nGroupID);
            }

            return nUserID;
        }

        public string GetUserToken()
        {
            return "";
        }

        public int SaveForInsert(Int32 groupId, bool bIsSetUserActive = false, bool isSetFailCount = false)
        {
            try
            {
                // add user role
                long roleId = ApplicationConfiguration.Current.RoleIdsConfiguration.UserRoleId.Value;
                if (roleId > 0 && !m_oBasicData.RoleIds.Contains(roleId))
                {
                    m_oBasicData.RoleIds.Add(roleId);
                }

                this.GroupId = groupId;
                this.shouldRemoveFromCache = true;
                this.shouldSetUserActive = bIsSetUserActive;
                this.resetFailCount = isSetFailCount;

                // New user - Insert
                if (string.IsNullOrEmpty(m_sSiteGUID))
                {
                    this.Insert();
                }
            }
            catch
            {
                this.userId = -1;
            }

            return this.userId;
        }

        public int SaveForUpdate(Int32 groupId, bool bIsSetUserActive = false, bool isSetFailCount = false, bool updateUserPassword = false)
        {
            try
            {
                this.GroupId = groupId;
                this.shouldRemoveFromCache = true;
                this.shouldSetUserActive = bIsSetUserActive;
                this.resetFailCount = isSetFailCount;
                this.UpdateUserPassword = updateUserPassword;

                // Existing user - Remove & Update from cache
                if (int.TryParse(m_sSiteGUID, out this.userId))
                {
                    this.Update();
                }
            }
            catch
            {
                this.userId = -1;
            }

            return this.userId;
        }

        protected override bool DoInsert()
        {
            UpdateUserTypeOnBasicData(GroupId);
            
            var userDataEncryptor = UserDataEncryptor.Instance();
            var encryptionType = userDataEncryptor.GetUsernameEncryptionType(GroupId);
            m_oBasicData.m_sUserName = userDataEncryptor.CorrectUsernameCase(encryptionType, m_oBasicData.m_sUserName);
            var encryptedUsername = userDataEncryptor.EncryptUsername(GroupId, encryptionType, m_oBasicData.m_sUserName);
            
            var userInputModel = MapToUserInputModel(encryptedUsername, encryptionType);
            var result = UsersDal.InsertUser(userInputModel);
            if (result != null)
            {
                userId = result.UserId;
                m_oBasicData.CreateDate = result.CreateDate;
                m_oBasicData.UpdateDate = result.UpdateDate;
                m_sSiteGUID = userId.ToString();
                
                if (UsersDal.UpsertUserRoleIds(GroupId, userId, m_oBasicData.RoleIds))
                {
                    var invalidationKey = LayeredCacheKeys.GetUserRolesInvalidationKey(GroupId, m_sSiteGUID);
                    if (!LayeredCache.Instance.SetInvalidationKey(invalidationKey))
                    {
                        log.ErrorFormat("Failed to set invalidation key on DoInsert key = {0}", invalidationKey);
                    }
                }
                else
                {
                    log.ErrorFormat("User created with no role. userId = {0}", m_sSiteGUID);
                }

                if (m_oDynamicData?.m_sUserData == null)
                {
                    return true;
                }

                m_oDynamicData.UserId = userId;
                m_oDynamicData.GroupId = GroupId;
                if (m_oDynamicData.Save())
                {
                    return true;
                }
            }

            userId = -1;

            return false;
        }

        protected override bool DoUpdate()
        {
            bool success = false;

            this.userId = int.Parse(this.m_sSiteGUID);

            // activation
            if (this.shouldSetUserActive)
            {
                success = DAL.UsersDal.UpdateUserActivationToken(GroupId, userId, activationToken, Guid.NewGuid().ToString(), (int)UserState.LoggedOut);

                if (success)
                {
                    bool resetSession = DAL.UsersDal.SetUserSessionStatus(userId, 0, 0);
                }
            }
            // update
            else
            {
                bool saved = m_oBasicData.Save(this.userId, this.GroupId, this.resetFailCount, this.UpdateUserPassword);

                if (!saved)
                {
                    this.userId = -1;
                    return success;
                }

                if (m_oDynamicData != null && m_oDynamicData.m_sUserData != null)
                {
                    m_oDynamicData.GroupId = this.GroupId;
                    m_oDynamicData.UserId = this.userId;
                    saved = m_oDynamicData.Save();

                    if (!saved)
                    {
                        this.userId = -2;
                        return success;
                    }
                }

                try
                {
                    Notifiers.BaseUsersNotifier t = null;
                    Notifiers.Utils.GetBaseUsersNotifierImpl(ref t, this.GroupId);

                    if (t != null)
                    {
                        t.NotifyChange(m_sSiteGUID);
                    }
                }
                catch (Exception ex)
                {
                    log.Error("exception - " + m_sSiteGUID + " : " + ex.Message, ex);
                }

                if (this.userId > 0)
                {
                    success = true;
                }
            }

            if (this.shouldRemoveFromCache)
            {
                UsersCache usersCache = UsersCache.Instance();
                usersCache.RemoveUser(this.userId, this.GroupId);
            }

            return success;
        }

        protected override bool DoDelete()
        {
            throw new NotImplementedException();
        }

        public bool SaveDynamicData(int nGroupID)
        {
            bool saved = false;
            if (m_oDynamicData != null && m_oDynamicData.m_sUserData != null)
            {
                m_oDynamicData.UserId = int.Parse(m_sSiteGUID);
                m_oDynamicData.GroupId = nGroupID;
                saved = m_oDynamicData.Save();
            }

            try
            {
                Notifiers.BaseUsersNotifier t = null;
                Notifiers.Utils.GetBaseUsersNotifierImpl(ref t, nGroupID);

                if (t != null)
                {
                    t.NotifyChange(m_sSiteGUID);
                }
            }
            catch (Exception ex)
            {
                log.Error("exception - " + m_sSiteGUID + " : " + ex.Message, ex);
            }
            return saved;
        }

        static public bool Hit(string sSiteGUID)
        {
            if (string.IsNullOrEmpty(sSiteGUID))
            {
                return false;
            }

            bool res = DAL.UsersDal.UpdateHitDate(int.Parse(sSiteGUID));
            return res;
        }

        static public bool Logout(string sSiteGUID)
        {
            if (string.IsNullOrEmpty(sSiteGUID))
            {
                return false;
            }

            bool res = DAL.UsersDal.UpdateHitDate(int.Parse(sSiteGUID), true);
            return res;
        }

        static protected bool UpdateFailCount(int groupId, int add, int userId, User user, bool setLoginDate = false)
        {
            bool updateRes = DAL.UsersDal.UpdateFailCount(groupId, userId, add, setLoginDate);
            UsersCache usersCache = UsersCache.Instance();
            usersCache.RemoveUser(userId, groupId);
            return updateRes;
        }

        private static int AddUserSession(int nSiteGuid, string sSessionID, string sIP, string sIDInDevices)
        {
            int retVal = 0;
            bool bIsActive = false;
            long lIDInUserSessions = 0;
            bool bIsDeviceIDExist = IsIDInDevicesExist(sIDInDevices);
            if (bIsDeviceIDExist && UsersDal.Get_UserSessionByDeviceID(nSiteGuid, sIDInDevices, ref bIsActive, ref lIDInUserSessions) && lIDInUserSessions > 0)
            {
                retVal = (int)lIDInUserSessions;
            }
            else
            {
                if (UsersDal.Get_UserSessionDetailsBySessionIDAndIP(nSiteGuid, sSessionID, sIP, ref bIsActive, ref lIDInUserSessions) && lIDInUserSessions > 0)
                    retVal = (int)lIDInUserSessions;
            }
            if (retVal > 0)
            {
                // there is a row with relevant data. check whether we need to update it or not
                if (!bIsActive)
                {
                    UpdateUserSession((int)lIDInUserSessions, false);
                    if (bIsDeviceIDExist)
                    {
                        // deactivate other users with same device
                        UsersDal.Update_IsActiveForAllUserSessionsOtherThan(nSiteGuid, sIDInDevices, false);
                    }
                }

            }
            else
            {
                // insert new row into users_sessions
                retVal = (int)UsersDal.Insert_NewUserSessionAndReturnID(nSiteGuid, sSessionID, sIP, sIDInDevices, true, 1);
                if (retVal > 0 && bIsDeviceIDExist)
                {
                    // deactivate other users with same device
                    UsersDal.Update_IsActiveForAllUserSessionsOtherThan(nSiteGuid, sIDInDevices, false);
                }
            }

            return retVal;

        }

        private static int UpdateUserSession(int id, bool isRemove)
        {
            int statusInt = isRemove ? 0 : 1;
            bool updateRes = DAL.UsersDal.UpdateUserSession(id, statusInt);

            return id;
        }

        private static int UpdateUserSession(int siteGuid, string sessionID, string sIP, string sIDInDevices, bool isRemove)
        {
            if (IsIDInDevicesExist(sIDInDevices))
                return UsersDal.Update_ActivenessForUserSessionByDeviceIDAndReturnID(siteGuid, sIDInDevices, !isRemove);
            return UsersDal.Update_UserActivenessForUserSessionBySessionIDAndIPAndReturnID(siteGuid, sessionID, sIP, !isRemove);
        }

        static public UserResponseObject SignOut(int siteGuid, int nGroupID, string sessionID, string sIP, string sDeviceUDID)
        {
            UserResponseObject retVal = new UserResponseObject();
            User u = new User();
            u.Initialize(siteGuid, nGroupID);
            UserState currentState = GetCurrentUserState(siteGuid, nGroupID);
            var lIDInDevices = DeviceDal.GetDeviceIdByUDID(sDeviceUDID, nGroupID);
            int instanceID = 0;
            UserState userStats = DoUserAction(siteGuid, nGroupID, sessionID, sIP, lIDInDevices > 0 ? lIDInDevices + "" : string.Empty, currentState, UserAction.SignOut, false, ref instanceID);

            retVal.Initialize(ResponseStatus.SessionLoggedOut, u);
            retVal.m_userInstanceID = instanceID.ToString();

            if (retVal.m_RespStatus == ResponseStatus.SessionLoggedOut)
            {
                Utils.AddInitiateNotificationActionToQueue(nGroupID, eUserMessageAction.Logout, siteGuid, sDeviceUDID);
            }
            else
                log.ErrorFormat("SignOut: error while logging user out: user: {0}, group: {1}, error: {2}", siteGuid, nGroupID, retVal.m_RespStatus);

            return retVal;
        }

        static private DateTime GetLastUserSessionDate(int nSiteGuid, string sIP, ref int userSessionID, ref string userSession, ref string lastUserIP, ref DateTime dbNow)
        {
            DateTime retVal = DateTime.MaxValue;

            retVal = DAL.UsersDal.GetLastUserSessionDate(nSiteGuid, ref userSessionID, ref userSession, ref lastUserIP, ref dbNow);
            return retVal;
        }

        private bool IsUserTypeExist(int nGroupID, int nUserTypeID)
        {
            bool result = false;
            DataTable dtUserTypes = UsersDal.GetUserTypeData(nGroupID, null);
            if (dtUserTypes != null && dtUserTypes.Rows.Count > 0)
            {
                DataRow userTypeRow = dtUserTypes.AsEnumerable().FirstOrDefault(tt => tt.Field<long>("ID") == nUserTypeID);
                if (userTypeRow != null)
                {
                    result = true;
                }
                else
                {
                    result = false;
                }
            }
            return result;
        }

        private void UpdateUserTypeOnBasicData(Int32 nGroupID)
        {
            if (m_oBasicData.m_UserType.ID == null)
            {
                m_oBasicData.m_UserType = GetDefaultUserType(nGroupID);
            }
            else //Check if user type id exists at users_types table
            {
                if (IsUserTypeExist(nGroupID, m_oBasicData.m_UserType.ID.Value) == false)
                {
                    m_oBasicData.m_UserType.ID = null;
                }
            }
        }

        static protected internal UserResponseObject InnerSignIn(ref UserResponseObject retObj, int nMaxFailCount, int nLockMinutes, int nGroupID, string sessionID, string sIP, string deviceUDID, bool bPreventDoubleLogins, int groupID)
        {
            int instanceID = 0;
            if (retObj != null && retObj.m_RespStatus == ResponseStatus.OK && retObj.m_user != null)
            {
                bool bIsDeviceActivated = false;
                Device device = GetDevice(deviceUDID, groupID, retObj.m_user.m_domianID);
                bIsDeviceActivated = (device != null && device.m_state == DeviceState.Activated) || (device == null); // device == null means web login

                {
                    string sDeviceIDToUse = device != null ? device.m_id : string.Empty;
                    int nSiteGuid = 0;
                    Int32.TryParse(retObj.m_user.m_sSiteGUID, out nSiteGuid);
                    UserState currUserState = GetCurrentUserState(nSiteGuid, nGroupID);

                    if (retObj.m_user.m_eSuspendState == DomainSuspentionStatus.Suspended)
                    {
                        retObj.m_RespStatus = ResponseStatus.UserSuspended;
                    }

                    if (currUserState == UserState.Unknown || currUserState == UserState.LoggedOut)
                    {
                        DoUserAction(nSiteGuid, nGroupID, sessionID, sIP, sDeviceIDToUse, currUserState, UserAction.SignIn, false, ref instanceID);
                    }
                    else if (currUserState == UserState.Activated)
                    {
                        DoUserAction(nSiteGuid, nGroupID, sessionID, sIP, sDeviceIDToUse, currUserState, UserAction.SignIn, true, ref instanceID);
                    }
                    else if (currUserState == UserState.SingleSignIn || currUserState == UserState.DoubleSignIn)
                    {
                        if (bPreventDoubleLogins)
                        {
                            int currentUserLogins = getNumOfUserLogins(nSiteGuid);
                            int currentGroupLimit = getUserLimit(groupID);

                            if (currentUserLogins >= currentGroupLimit)
                            {
                                int lastUserSessionID = 0;
                                string lastUserSession = string.Empty;
                                string lastUserIP = string.Empty;
                                DateTime dbNow = DateTime.MaxValue;
                                DateTime lastActionDate = GetLastUserSessionDate(nSiteGuid, sIP, ref lastUserSessionID, ref lastUserSession, ref lastUserIP, ref dbNow);

                                if (lastActionDate != DateTime.MaxValue)
                                {
                                    TimeSpan nowTS = TimeSpan.MaxValue;
                                    if (sIP.Equals(lastUserIP))
                                    {
                                        nowTS = new TimeSpan(0, 30, 0);
                                    }
                                    else
                                    {
                                        nowTS = new TimeSpan(2, 0, 0);
                                    }
                                    DateTime compDate = lastActionDate.Add(nowTS);
                                    if (dbNow > compDate)
                                    {
                                        UpdateUserSession(lastUserSessionID, true);
                                        DoUserAction(nSiteGuid, nGroupID, sessionID, sIP, sDeviceIDToUse, UserState.LoggedOut, UserAction.SignIn, false, ref instanceID);
                                    }
                                    else
                                    {
                                        if (!lastUserSession.Equals(sessionID))
                                        {
                                            retObj.m_RespStatus = ResponseStatus.UserDoubleLogIn;
                                        }
                                        else
                                        {
                                            retObj.m_RespStatus = ResponseStatus.UserAllreadyLoggedIn;
                                        }
                                    }
                                }
                                else
                                {
                                    //User is loggen in from a different session
                                    if (!lastUserSession.Equals(sessionID))
                                    {
                                        retObj.m_RespStatus = ResponseStatus.UserDoubleLogIn;
                                    }
                                    else
                                    {
                                        retObj.m_RespStatus = ResponseStatus.UserAllreadyLoggedIn;
                                    }
                                }
                            }
                            else
                            {
                                DoUserAction(nSiteGuid, nGroupID, sessionID, sIP, sDeviceIDToUse, currUserState, UserAction.SignIn, false, ref instanceID);
                            }
                        }

                        else
                        {
                            DoUserAction(nSiteGuid, nGroupID, sessionID, sIP, sDeviceIDToUse, currUserState, UserAction.SignIn, false, ref instanceID);
                        }
                    }
                    retObj.m_user.m_eUserState = GetCurrentUserState(nSiteGuid, nGroupID);
                }
            }
            retObj.m_userInstanceID = instanceID.ToString();

            return retObj;
        }

        static public UserResponseObject SignIn(int siteGuid, int nMaxFailCount, int nLockMinutes, int nGroupID, string sessionID, string sIP, string deviceID, bool bPreventDoubleLogins)
        {
            UserResponseObject retObj = new UserResponseObject();
            User u = new User();
            bool init = u.Initialize(siteGuid, nGroupID);

            if (!init)
            {
                retObj.Initialize(ResponseStatus.ErrorOnInitUser, u);
                return retObj;
            }

            retObj.Initialize(ResponseStatus.OK, u);

            //BEO-11890
            UpdateFailCount(nGroupID, 0, siteGuid, u, true);

            return InnerSignIn(ref retObj, nMaxFailCount, nLockMinutes, nGroupID, sessionID, sIP, deviceID, bPreventDoubleLogins, nGroupID);
        }

        static public UserResponseObject SignIn(string username, string password, int maxFailCount, int lockMinutes, int groupId, string sessionID, string sIP, string deviceID, bool bPreventDoubleLogins)
        {
            var userResponseObject = CheckUserPassword(username, password, maxFailCount, lockMinutes, groupId, bPreventDoubleLogins, false, true);
            return InnerSignIn(ref userResponseObject, maxFailCount, lockMinutes, groupId, sessionID, sIP, deviceID, bPreventDoubleLogins, groupId);
        }

        static public UserResponseObject CheckUserPassword(string username, string password, int defaultMaxFailCount, int lockMinutes, int groupId, bool preventDoubleLogins,
            bool checkHitDate, bool isSignIn = false)
        {
            var userResponseObject = new UserResponseObject();
            var responseStatus = ResponseStatus.WrongPasswordOrUserName;
            User user = null;

            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
            {
                DateTime passwordUpdateDate = DateTime.MinValue;
                int nFailCount = 0;
                var dNow = DateTime.UtcNow;
                DateTime dLastFailDate = new DateTime(2020, 1, 1);
                DateTime dLastHitDate = new DateTime(2020, 1, 1);

                var userId = UserStorage.Instance().GetUserPasswordFailHistory(username, groupId, ref dNow, ref nFailCount, ref dLastFailDate, ref dLastHitDate, ref passwordUpdateDate);
                if (userId <= 0)
                {
                    userResponseObject.m_RespStatus = ResponseStatus.UserDoesNotExist;
                    return userResponseObject;
                }

                var initializedUser = new User();
                var isUserInitialized = initializedUser.Initialize(userId, groupId);

                if (isUserInitialized && initializedUser.m_oBasicData != null && initializedUser.m_oDynamicData != null && !string.IsNullOrEmpty(initializedUser.m_oBasicData.m_sPassword))
                {
                    responseStatus = ResponseStatus.OK;
                    bool isPasswordEqual = IsPasswordEqual(password, initializedUser, groupId, userId);

                    GetMaxFailuresCountAndExpiration(defaultMaxFailCount, initializedUser.m_oBasicData.RoleIds, groupId, out int maxFailuresCount, out int maxExpiration);

                    if (isPasswordEqual)
                    {
                        if (nFailCount > maxFailuresCount && ((TimeSpan)(dNow - dLastFailDate)).TotalMinutes < lockMinutes)
                        {
                            responseStatus = ResponseStatus.InsideLockTime;
                        }
                        else if (preventDoubleLogins)
                        {
                            if (dLastHitDate.AddSeconds(60) > dNow && checkHitDate)
                            {
                                responseStatus = ResponseStatus.UserAllreadyLoggedIn;
                            }
                            else
                            {
                                UpdateFailCount(groupId, 0, userId, initializedUser, true);
                            }
                        }
                        else if (maxExpiration > 0 && passwordUpdateDate.AddDays(maxExpiration) < dNow)
                        {
                            responseStatus = ResponseStatus.PasswordExpired;
                        }
                        else
                        {
                            UpdateFailCount(groupId, 0, userId, initializedUser, true);

                            user = initializedUser;
                        }
                    }
                    else
                    {
                        UpdateFailCount(groupId, 1, userId, initializedUser);

                        if (nFailCount >= maxFailuresCount && ((TimeSpan)(dNow - dLastFailDate)).TotalMinutes < lockMinutes)
                        {
                            responseStatus = ResponseStatus.InsideLockTime;
                        }
                        else
                        {
                            responseStatus = ResponseStatus.WrongPasswordOrUserName;
                        }
                    }
                }
                else
                {
                    responseStatus = ResponseStatus.UserDoesNotExist;
                }
            }

            userResponseObject.Initialize(responseStatus, user);
            return userResponseObject;
        }

        private static bool IsPasswordEqual(string password, User user, int groupId, int userId)
        {
            var passwordFromDb = user.m_oBasicData.m_sPassword;
            var salt = user.m_oBasicData.m_sSalt;
            BaseEncrypter encrypter = Utils.GetBaseImpl(groupId);

            var encryptionEnabled = encrypter != null;
            var passwordWasHashed = !salt.IsNullOrEmpty();

            var passwordToCheck = encryptionEnabled && passwordWasHashed
                ? encrypter.Encrypt(password, salt)
                : password;

            bool isPasswordEqual = passwordFromDb == passwordToCheck;

            if (isPasswordEqual && encryptionEnabled)
            {
                if (!passwordWasHashed) MigratePassword(password, groupId, user);

                MigratePasswordHistory(userId, encrypter);
            }

            return isPasswordEqual;
        }

        public static void MigratePassword(string password, int groupId, User initializedUser)
        {
            initializedUser.m_oBasicData.SetPassword(password, groupId); // generate salt and encrypt password

            initializedUser.SaveForUpdate(groupId, false, false, true);
        }

        public static void MigratePasswordHistory(int userId, BaseEncrypter encrypter)
        {
            var passwordsHistory = UsersDal.GetPasswordsHistory(userId);
            if (passwordsHistory?.Count > 0)
            {
                var hashedPasswords = passwordsHistory.Select(p => {
                    var salt = BaseEncrypter.GetRand64String();
                    return new Password(encrypter.Encrypt(p, salt), salt);
                }).ToList();
                var hashedPasswordHistory = new PasswordHistory(hashedPasswords);
                if (UsersDal.SaveHashedPasswordHistory(userId, hashedPasswordHistory))
                {
                    UsersDal.DeletePasswordsHistory(userId);
                }
            }
        }

        private static void GetMaxFailuresCountAndExpiration(int defaultMaxFailCount, List<long> roleIds, int groupId, out int maxFailuresCount, out int maxExpiration)
        {
            maxFailuresCount = 0;
            maxExpiration = 0;

            if (roleIds?.Count > 0)
            {
                var passwordPoliciesResponse = PasswordPolicyManager.Instance.List(new ContextData(groupId), new PasswordPolicyFilter() { RoleIdsIn = roleIds });
                if (passwordPoliciesResponse.HasObjects())
                {
                    foreach (var passwordPolicy in passwordPoliciesResponse.Objects)
                    {
                        if (passwordPolicy.LockoutFailuresCount.HasValue && passwordPolicy.LockoutFailuresCount > 0 && passwordPolicy.LockoutFailuresCount > maxFailuresCount)
                        {
                            maxFailuresCount = passwordPolicy.LockoutFailuresCount.Value;
                        }

                        if (passwordPolicy.Expiration.HasValue & passwordPolicy.Expiration > 0 && passwordPolicy.Expiration > maxExpiration)
                        {
                            maxExpiration = passwordPolicy.Expiration.Value;
                        }
                    }
                }
            }

            if (maxFailuresCount == 0)
            {
                maxFailuresCount = defaultMaxFailCount;
            }
        }

        private static bool IsIDInDevicesExist(string sIDInDevices)
        {
            long l = 0;
            return !string.IsNullOrEmpty(sIDInDevices) && Int64.TryParse(sIDInDevices, out l) && l > 0;
        }

        private static Device GetDevice(string sDeviceUDID, int nGroupID, int nDomainID)
        {
            return string.IsNullOrEmpty(sDeviceUDID)
                ? null
                : DeviceRepository.Get(sDeviceUDID, nDomainID, nGroupID);
        }

        public static bool IsUserValid(int nGroupID, int userGuid)
        {
            //Check if UserGuid is valid
            User user = new User();

            bool init = user.Initialize(userGuid, nGroupID);

            UserResponseObject resp = new UserResponseObject();

            if (user.m_oBasicData.m_sUserName == "")
            {
                resp.Initialize(ResponseStatus.UserDoesNotExist, user);
            }
            else
            {
                resp.Initialize(ResponseStatus.OK, user);
            }

            return (resp.m_RespStatus == ResponseStatus.OK);
        }

        public static bool IsUserValid(int nGroupID, int userGuid, ref User user)
        {
            // Check if UserGuid is valid
            bool init = user.Initialize(userGuid, nGroupID);

            UserResponseObject resp = new UserResponseObject();

            if (user.m_oBasicData.m_sUserName == "")
            {
                resp.Initialize(ResponseStatus.UserDoesNotExist, user);
            }
            else
            {
                resp.Initialize(ResponseStatus.OK, user);
            }

            return (resp.m_RespStatus == ResponseStatus.OK);
        }

        //backward compatibility - BEO-7137
        public int Save(Int32 groupId, bool bIsSetUserActive = false)
        {
            return SaveForUpdate(groupId, bIsSetUserActive);
        }

        static public bool UpdateLoginViaStartSession(int groupId, int add, int userId, User user, bool setLoginDate = false)
        {
            return UpdateFailCount(groupId, add, userId, user, setLoginDate);
        }

        private InsertUserInputModel MapToUserInputModel(string encryptedUsername, EncryptionType? encryptionType)
        {
            var countryId = m_oBasicData.m_Country?.m_nObjecrtID;

            return new InsertUserInputModel
            {
                Username = encryptedUsername,
                Password = m_oBasicData.m_sPassword,
                Salt = m_oBasicData.m_sSalt,
                FirstName = m_oBasicData.m_sFirstName,
                LastName = m_oBasicData.m_sLastName,
                FacebookId = m_oBasicData.m_sFacebookID,
                FacebookImage = m_oBasicData.m_sFacebookImage,
                FacebookToken = m_oBasicData.m_sFacebookToken,
                Email = m_oBasicData.m_sEmail,
                ActivateStatus = shouldSetUserActive,
                CoGuid = m_oBasicData.m_CoGuid,
                ExternalToken = m_oBasicData.m_ExternalToken,
                UserTypeId = m_oBasicData.m_UserType.ID,
                Address = m_oBasicData.m_sAddress,
                City = m_oBasicData.m_sCity,
                Zip = m_oBasicData.m_sZip,
                Phone = m_oBasicData.m_sPhone,
                AffiliateCode = m_oBasicData.m_sAffiliateCode,
                TwitterToken = m_oBasicData.m_sTwitterToken,
                TwitterTokenSecret = m_oBasicData.m_sTwitterTokenSecret,
                GroupId = GroupId,
                UsernameEncryptionEnabled = encryptionType.HasValue,
                IsFacebookImagePermitted = m_oBasicData.m_bIsFacebookImagePermitted,
                ActivationToken = shouldSetUserActive ? string.Empty : Guid.NewGuid().ToString(),
                CountryId = countryId ?? default,
                StateId = m_oBasicData.m_State != null && countryId > 0 ? m_oBasicData.m_State.m_nObjecrtID : default
            };
        }
    }
}