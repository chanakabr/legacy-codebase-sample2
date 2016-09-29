using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using DAL;
using ApiObjects;
using KLogMonitor;
using System.Reflection;

namespace Users
{
    public class User
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        public User()
        {
            m_oBasicData = new UserBasicData();
            m_oDynamicData = new UserDynamicData();
            m_sSiteGUID = "";
            m_eUserState = UserState.Unknown;
            m_eSuspendState = DomainSuspentionStatus.OK;
        }

        public User(int nGroupID, int nUserID)
            : this()
        {
            Initialize(nUserID, nGroupID);
        }

        public User Clone()
        {
            return CloneImpl();
        }

        protected virtual User CloneImpl()
        {
            var copy = (User)MemberwiseClone();

            return copy;
        }

        public static User GetUser(int nUserID, int nGroupID)
        {
            User user = new User(nUserID, nGroupID);
            return user;
        }

        public static UserState DoUserAction(int siteGuid, string sessionID, string sIP, string sIDInDevices, UserState currentState, UserAction action, bool needActivation, ref int instanceID)
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

        public static UserState GetCurrentUserState(int siteGuid)
        {
            UserState retVal = UserState.Unknown;
            int nUserState = 0;
            nUserState = UsersDal.Get_UserStateFromUsers(siteGuid);
            if (Enum.IsDefined(typeof(UserState), nUserState))
                retVal = (UserState)nUserState;
            return retVal;
        }

        public static UserState GetCurrentUserInstanceState(int siteGuid, string sessionID, string sIP, string deviceID, int nGroupID)
        {
            UserState retVal = UserState.Unknown;
            int userSessionID = 0;

            long lIDInDevices = string.IsNullOrEmpty(deviceID) ? 0 : DeviceDal.Get_IDInDevicesByDeviceUDID(deviceID, nGroupID);

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
            if (m_sSiteGUID != "")
            {
                if (!string.IsNullOrEmpty(oBasicData.m_sUserName))
                {
                    m_oBasicData.m_sUserName = oBasicData.m_sUserName;
                }

                m_oBasicData.m_sEmail = oBasicData.m_sEmail;
                m_oBasicData.m_sFirstName = oBasicData.m_sFirstName;
                m_oBasicData.m_sLastName = oBasicData.m_sLastName;
                m_oBasicData.m_Country = oBasicData.m_Country;
                m_oBasicData.m_sAddress = oBasicData.m_sAddress;
                m_oBasicData.m_sCity = oBasicData.m_sCity;
                m_oBasicData.m_sPhone = oBasicData.m_sPhone;
                m_oBasicData.m_State = oBasicData.m_State;
                m_oBasicData.m_sZip = oBasicData.m_sZip;
                m_oBasicData.m_sFacebookID = oBasicData.m_sFacebookID;
                m_oBasicData.m_sFacebookImage = oBasicData.m_sFacebookImage;
                m_oBasicData.m_bIsFacebookImagePermitted = oBasicData.m_bIsFacebookImagePermitted;
                m_oBasicData.m_sFacebookToken = oBasicData.m_sFacebookToken;
                m_oBasicData.m_UserType = oBasicData.m_UserType;
                m_oDynamicData = oDynamicData;
            }

            int userID = Save(nGroupID);
            return userID;
        }

        public void UpdateDynamicData(UserDynamicData oDynamicData, Int32 nGroupID)
        {
            if (m_sSiteGUID != "")
            {
                m_oDynamicData = oDynamicData;
                SaveDynamicData(nGroupID);
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

        public bool Initialize(UserBasicData oBasicData, UserDynamicData oDynamicData, Int32 nGroupID, string sPassword)
        {
            try
            {
                UserResponseObject u = null;

                Utils.SetPassword(sPassword, ref oBasicData, nGroupID);

                u = CheckUserPassword(oBasicData.m_sUserName, oBasicData.m_sPassword, 3, 3, nGroupID, false, true);
                if (u.m_RespStatus == ResponseStatus.WrongPasswordOrUserName)
                {
                    return false;
                }

                m_sSiteGUID = (u != null && u.m_user != null) ? u.m_user.m_sSiteGUID : "";

                m_oBasicData = oBasicData;
                m_oDynamicData = oDynamicData;
                if (!string.IsNullOrEmpty(m_sSiteGUID))
                {
                    m_domianID = DAL.UsersDal.GetUserDomainID(m_sSiteGUID, ref m_nSSOOperatorID, ref m_isDomainMaster, ref m_eSuspendState);
                }

                return true;
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder("Exception at User.Initialize ");
                sb.Append(String.Concat(" Basic Data: ", oBasicData.ToString()));
                sb.Append(String.Concat(" Group ID: ", nGroupID));
                sb.Append(String.Concat(" Msg: ", ex.Message));
                sb.Append(String.Concat(" Stack Trace: ", ex.StackTrace));

                log.Error("Exception - " + sb.ToString(), ex);
            }

            return false;
        }

        public bool Initialize(Int32 nUserID, Int32 nGroupID)
        {
            bool res = false;

            try
            {
                res = m_oBasicData.Initialize(nUserID, nGroupID);

                if (!res)
                {
                    return res;
                }

                res = m_oDynamicData.Initialize(nUserID, nGroupID);

                m_sSiteGUID = nUserID.ToString();

                m_domianID = UsersDal.GetUserDomainID(m_sSiteGUID, ref m_nSSOOperatorID, ref m_isDomainMaster, ref m_eSuspendState);

                if (m_domianID <= 0)
                {
                    m_domianID = DomainDal.GetDomainIDBySiteGuid(nGroupID, nUserID, ref m_nSSOOperatorID, ref m_isDomainMaster, ref m_eSuspendState);
                }

                m_eUserState = GetCurrentUserState(nUserID);

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
                res = false;
            }

            return res;
        }

        public bool Initialize(Int32 nUserID, Int32 nGroupID, int domainID, bool isDomainMaster)
        {
            bool res = false;

            try
            {
                res = Initialize(nUserID, nGroupID);
                m_domianID = domainID;
                m_isDomainMaster = isDomainMaster;
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
            Int32 nUserID = DAL.UsersDal.GetUserIDByUsername(sUsername, nGroupID);
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

        public int Save(int nGroupID)
        {
            return (Save(nGroupID, false));
        }

        public int Save(Int32 nGroupID, bool bIsSetUserActive)
        {
            int nID = (-1);

            try
            {
                // New user - Insert
                if (string.IsNullOrEmpty(m_sSiteGUID))
                {
                    UpdateUserTypeOnBasicData(nGroupID);

                    int bIsFacebookImagePermitted = m_oBasicData.m_bIsFacebookImagePermitted ? 1 : 0;

                    string sActivationToken = bIsSetUserActive ? string.Empty : System.Guid.NewGuid().ToString();

                    int userInserted = DAL.UsersDal.InsertUser(m_oBasicData.m_sUserName,
                                                               m_oBasicData.m_sPassword,
                                                               m_oBasicData.m_sSalt,
                                                               m_oBasicData.m_sFirstName,
                                                               m_oBasicData.m_sLastName,
                                                               m_oBasicData.m_sFacebookID,
                                                               m_oBasicData.m_sFacebookImage,
                                                               m_oBasicData.m_sFacebookToken,
                                                               bIsFacebookImagePermitted,
                                                               m_oBasicData.m_sEmail,
                                                               (bIsSetUserActive ? 1 : 0),
                                                               sActivationToken,
                                                               m_oBasicData.m_CoGuid,
                                                               m_oBasicData.m_ExternalToken,
                                                               m_oBasicData.m_UserType.ID,
                                                               nGroupID);

                    bool bInit = Initialize(m_oBasicData, m_oDynamicData, nGroupID, "");

                    if ((!bInit) || (!int.TryParse(m_sSiteGUID, out nID)) || (!m_oBasicData.Save(nID)))
                    {
                        return (-1);
                    }

                    if ((m_oDynamicData.m_sUserData != null) && (!m_oDynamicData.Save(nID)))
                    {
                        return (-1);
                    }

                    return userInserted;
                }

                // Existing user - Update
                nID = int.Parse(m_sSiteGUID);
                bool saved = m_oBasicData.Save(nID);

                if (!saved) { return (-1); }

                if (m_oDynamicData.m_sUserData != null)
                {
                    saved = m_oDynamicData.Save(nID);

                    if (!saved) { return (-2); }
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

            }
            catch
            {
                return (-1);
            }

            return nID;
        }

        public bool SaveDynamicData(int nGroupID)
        {
            bool saved = false;
            if (m_oDynamicData != null && m_oDynamicData.m_sUserData != null)
            {
                int nID = int.Parse(m_sSiteGUID);
                saved = m_oDynamicData.Save(nID, nGroupID);
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

        static protected bool UpdateFailCount(Int32 nAdd, Int32 nUserID)
        {
            bool updateRes = DAL.UsersDal.UpdateFailCount(nUserID, nAdd);
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
            UserState currentState = GetCurrentUserState(siteGuid);
            long lIDInDevices = DeviceDal.Get_IDInDevicesByDeviceUDID(sDeviceUDID, nGroupID);
            int instanceID = 0;
            UserState userStats = DoUserAction(siteGuid, sessionID, sIP, lIDInDevices > 0 ? lIDInDevices + "" : string.Empty, currentState, UserAction.SignOut, false, ref instanceID);

            retVal.Initialize(ResponseStatus.SessionLoggedOut, u);
            retVal.m_userInstanceID = instanceID.ToString();
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
                Device device = CreateAndInitializeDevice(deviceUDID, groupID, retObj.m_user.m_domianID);
                bIsDeviceActivated = (device != null && device.m_state == DeviceState.Activated) || (device == null); // device == null means web login
                
                //Ignore device check for now (token issue)
                //if (!bIsDeviceActivated)
                //{
                //    retObj.m_RespStatus = ResponseStatus.DeviceNotRegistered;
                //    return retObj;
                //}
                //else
                
                {
                    string sDeviceIDToUse = device != null ? device.m_id : string.Empty;
                    int nSiteGuid = 0;
                    Int32.TryParse(retObj.m_user.m_sSiteGUID, out nSiteGuid);
                    UserState currUserState = GetCurrentUserState(nSiteGuid);

                    if (retObj.m_user.m_eSuspendState == DomainSuspentionStatus.Suspended)
                    {
                        retObj.m_RespStatus = ResponseStatus.UserSuspended;
                    }

                    if (currUserState == UserState.Unknown || currUserState == UserState.LoggedOut)
                    {
                        DoUserAction(nSiteGuid, sessionID, sIP, sDeviceIDToUse, currUserState, UserAction.SignIn, false, ref instanceID);
                    }
                    else if (currUserState == UserState.Activated)
                    {
                        DoUserAction(nSiteGuid, sessionID, sIP, sDeviceIDToUse, currUserState, UserAction.SignIn, true, ref instanceID);
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
                                        DoUserAction(nSiteGuid, sessionID, sIP, sDeviceIDToUse, UserState.LoggedOut, UserAction.SignIn, false, ref instanceID);
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
                                DoUserAction(nSiteGuid, sessionID, sIP, sDeviceIDToUse, currUserState, UserAction.SignIn, false, ref instanceID);
                            }
                        }

                        else
                        {
                            DoUserAction(nSiteGuid, sessionID, sIP, sDeviceIDToUse, currUserState, UserAction.SignIn, false, ref instanceID);
                        }
                    }
                    retObj.m_user.m_eUserState = GetCurrentUserState(nSiteGuid);
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
            return InnerSignIn(ref retObj, nMaxFailCount, nLockMinutes, nGroupID, sessionID, sIP, deviceID, bPreventDoubleLogins, nGroupID);
        }

        static public UserResponseObject SignIn(string sUN, string sPass, int nMaxFailCount, int nLockMinutes, int nGroupID, string sessionID, string sIP, string deviceID, bool bPreventDoubleLogins)
        {
            UserResponseObject retObj = CheckUserPassword(sUN, sPass, nMaxFailCount, nLockMinutes, nGroupID, bPreventDoubleLogins, false);
            return InnerSignIn(ref retObj, nMaxFailCount, nLockMinutes, nGroupID, sessionID, sIP, deviceID, bPreventDoubleLogins, nGroupID);
        }

        static public UserResponseObject CheckUserPassword(string sUN, string sPass, Int32 nMaxFailCount, Int32 nLockMinutes, Int32 nGroupID, bool bPreventDoubleLogins, bool checkHitDate)
        {
            UserResponseObject o = new UserResponseObject();
            ResponseStatus ret = ResponseStatus.WrongPasswordOrUserName;
            User res = null;

            if (!string.IsNullOrEmpty(sUN) && !string.IsNullOrEmpty(sPass))
            {
                int nFailCount = 0;
                DateTime dNow = DateTime.Now;
                DateTime dLastFailDate = new DateTime(2020, 1, 1);
                DateTime dLastHitDate = new DateTime(2020, 1, 1);

                int nID = DAL.UsersDal.GetUserPasswordFailHistory(sUN, nGroupID, ref dNow, ref nFailCount, ref dLastFailDate, ref dLastHitDate);

                if (nID <= 0)
                {
                    o.m_RespStatus = ResponseStatus.UserDoesNotExist;
                    return o;
                }

                if (nID > 0)
                {
                    User u = new User();
                    bool bOk = u.Initialize(nID, nGroupID);

                    if (bOk && u.m_oBasicData != null && u.m_oDynamicData != null && !string.IsNullOrEmpty(u.m_oBasicData.m_sPassword))
                    {
                        ret = ResponseStatus.OK;

                        bool bOK = (sPass == u.m_oBasicData.m_sPassword);

                        if (bOK == false)
                        {
                            BaseEncrypter encrypter = null;

                            Utils.GetBaseEncrypterImpl(ref encrypter, nGroupID);

                            if (encrypter != null)
                            {
                                bOK = (u.m_oBasicData.m_sPassword == encrypter.Encrypt(sPass, u.m_oBasicData.m_sSalt));
                            }
                        }

                        if (bOK == true)
                        {
                            if (nFailCount > nMaxFailCount && ((TimeSpan)(dNow - dLastFailDate)).TotalMinutes < nLockMinutes)
                            {
                                ret = ResponseStatus.InsideLockTime;
                            }
                            else if (bPreventDoubleLogins == true)
                            {
                                if (dLastHitDate.AddSeconds(60) > dNow && checkHitDate)
                                {
                                    ret = ResponseStatus.UserAllreadyLoggedIn;
                                }
                                else
                                {
                                    if (nFailCount > 0)
                                        UpdateFailCount(0, nID);
                                }
                            }
                            else
                            {
                                if (nFailCount > 0)
                                    UpdateFailCount(0, nID);

                                res = u;
                            }
                        }
                        else
                        {
                            ret = ResponseStatus.WrongPasswordOrUserName;
                            UpdateFailCount(1, nID);
                        }
                    }
                    else
                        ret = ResponseStatus.UserDoesNotExist;
                }
                else
                    ret = ResponseStatus.UserDoesNotExist;
            }

            o.Initialize(ret, res);
            return o;
        }

        private static bool IsIDInDevicesExist(string sIDInDevices)
        {
            long l = 0;
            return !string.IsNullOrEmpty(sIDInDevices) && Int64.TryParse(sIDInDevices, out l) && l > 0;
        }

        private static Device CreateAndInitializeDevice(string sDeviceID, int nGroupID, int nDomainID)
        {
            if (string.IsNullOrEmpty(sDeviceID))
                return null;
            Device res = new Device(nGroupID);
            res.Initialize(sDeviceID, nDomainID);

            return res;
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
            //Check if UserGuid is valid
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


        public UserBasicData m_oBasicData;
        public UserDynamicData m_oDynamicData;
        public string m_sSiteGUID;
        public int m_domianID;
        public bool m_isDomainMaster;
        public UserState m_eUserState;
        public int m_nSSOOperatorID;
        public DomainSuspentionStatus m_eSuspendState;
    }
}
