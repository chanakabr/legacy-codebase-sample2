using ApiObjects;
using ApiObjects.Response;
using DAL;
using KLogMonitor;
using KlogMonitorHelper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Users
{
    public class TvinciUsers : BaseUsers
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private static object lockObj = new object();

        private const string DEFAULT_USER_CANNOT_BE_DELETED = "Default user cannot be deleted";
        private const string EXCLUSIVE_MASTER_USER_CANNOT_BE_DELETED = "Exclusive master user cannot be deleted";
        private const string HOUSEHOLD_NOT_INITIALIZED = "Household not initialized";
        private const string USER_NOT_EXISTS_IN_DOMAIN = "User not exists in domain";

        public TvinciUsers(Int32 nGroupID)
            : base(nGroupID)
        {
        }

        public override void Hit(string sSiteGUID)
        {
            User.Hit(sSiteGUID);
        }

        public override void Logout(string sSiteGUID)
        {
            User.Logout(sSiteGUID);
        }

        public override UserResponseObject GetUserByUsername(string sUsername, int nGroupID)
        {
            try
            {
                User u = new User();
                int nSiteGuid = u.InitializeByUsername(sUsername, m_nGroupID);
                UserResponseObject resp = new UserResponseObject();
                if (nSiteGuid < 1 || u.m_oBasicData.m_sUserName.Length == 0)
                    resp.Initialize(ResponseStatus.UserDoesNotExist, u);
                else
                    resp.Initialize(ResponseStatus.OK, u);
                return resp;
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder("Exception at GetUserByUsername. ");
                sb.Append(String.Concat("Username: ", sUsername));
                sb.Append(String.Concat(" Group ID: ", nGroupID));
                sb.Append(String.Concat(" this is: ", this.GetType().Name));
                sb.Append(String.Concat(" Msg: ", ex.Message));
                sb.Append(String.Concat(" Stack trace: ", ex.StackTrace));

                log.Error("Exception - " + sb.ToString(), ex);
                return null;
            }
        }

        public override UserResponseObject GetUserByFacebookID(string sFacebookID, int nGroupID)
        {
            try
            {
                User u = new User();
                u.InitializeByFacebook(sFacebookID, m_nGroupID);
                UserResponseObject resp = new UserResponseObject();
                if (u.m_oBasicData.m_sUserName == "")
                    resp.Initialize(ResponseStatus.UserDoesNotExist, u);
                else
                    resp.Initialize(ResponseStatus.OK, u);
                return resp;
            }
            catch
            {
                return null;
            }
        }

        public override bool IsUserActivated(ref string sUserName, ref Int32 nUserID)
        {
            bool isGracePeriod = false;
            UserActivationState nAS = GetUserActivationStatus(ref sUserName, ref nUserID, ref isGracePeriod);

            return nAS == UserActivationState.Activated;

        }

        public override UserActivationState GetUserActivationStatus(ref string sUserName, ref Int32 nUserID, ref bool isGracePeriod)
        {
            if (!IsActivationNeeded(null))
            {
                return UserActivationState.Activated;
            }

            UserActivationState activStatus = (UserActivationState)DAL.UsersDal.GetUserActivationState(m_nGroupID, m_nActivationMustHours, ref sUserName, ref nUserID, ref isGracePeriod);

            return activStatus;
        }

        public UserActivationState GetUserStatus(ref string sUserName, ref Int32 nUserID, ref bool isGracePeriod)
        {
            UserActivationState activStatus = (UserActivationState)DAL.UsersDal.GetUserActivationState(m_nGroupID, m_nActivationMustHours, ref sUserName, ref nUserID, ref isGracePeriod);

            return activStatus;
        }

        public override UserResponseObject CheckUserPassword(string sUN, string sPass, int nMaxFailCount, int nLockMinutes, Int32 nGroupID, bool bPreventDoubleLogins)
        {
            Int32 nUserID = -2;

            bool bActivated = IsUserActivated(ref sUN, ref nUserID);
            if (bActivated == false)
            {
                UserResponseObject o = new UserResponseObject();
                ResponseStatus ret = ResponseStatus.WrongPasswordOrUserName;
                if (nUserID <= 0)
                    ret = ResponseStatus.WrongPasswordOrUserName;
                else
                    ret = ResponseStatus.UserNotActivated;
                o.m_RespStatus = ret;
                return o;
            }
            return User.CheckUserPassword(sUN, sPass, 3, 3, nGroupID, bPreventDoubleLogins, true);
        }

        public override UserResponseObject SignIn(string sUN, string sPass, int nMaxFailCount, int nLockMinutes, int nGroupID, string sessionID, string sIP, string deviceID, bool bPreventDoubleLogins)
        {
            Int32 nUserID = -2;

            bool isGracePeriod = false;
            UserActivationState nUserStatus = GetUserStatus(ref sUN, ref nUserID, ref isGracePeriod);

            if (nUserStatus != UserActivationState.Activated)
            {
                UserResponseObject o = new UserResponseObject();
                ResponseStatus ret = ResponseStatus.UserNotActivated;

                if (nUserID <= 0)
                {
                    ret = ResponseStatus.WrongPasswordOrUserName;
                }
                else
                {
                    switch (nUserStatus)
                    {
                        case UserActivationState.UserDoesNotExist:
                            ret = ResponseStatus.UserDoesNotExist;
                            break;

                        //case UserActivationState.UserSuspended:
                        //    ret = ResponseStatus.UserSuspended;
                        //    break;

                        case UserActivationState.NotActivated:
                            o.m_user = new User(nGroupID, nUserID);
                            ret = ResponseStatus.UserNotActivated;
                            break;

                        case UserActivationState.NotActivatedByMaster:
                            o.m_user = new User(nGroupID, nUserID);
                            ret = ResponseStatus.UserNotMasterApproved;
                            break;

                        case UserActivationState.UserRemovedFromDomain:
                            o.m_user = new User(nGroupID, nUserID);
                            ret = ResponseStatus.UserNotIndDomain;
                            break;

                        case UserActivationState.UserWIthNoDomain:
                            o.m_user = new User(nGroupID, nUserID);
                            bool bValidDomainStat = CheckAddDomain(ref o, o.m_user, sUN, nUserID);
                            if (!bValidDomainStat)
                                return o;
                            break;
                    }
                }

                if (nUserStatus != UserActivationState.UserWIthNoDomain && nUserStatus != UserActivationState.UserSuspended)
                {
                    o.m_RespStatus = ret;
                    return o;
                }
            }

            var response =  User.SignIn(sUN, sPass, 3, 3, nGroupID, sessionID, sIP, deviceID, bPreventDoubleLogins);
            if (response != null && response.m_user != null)
            {
                response.m_user.IsActivationGracePeriod = isGracePeriod;
            }

            return response;
        }

        public override DomainResponseObject AddNewDomain(string sUN, int nUserID, int nGroupID)
        {
            Users.BaseDomain t = null;
            Utils.GetBaseDomainsImpl(ref t, nGroupID);
            DomainResponseObject dr = t.AddDomain(sUN + "/Domain", sUN + "/Domain", nUserID, nGroupID, "");

            if (dr == null || dr.m_oDomainResponseStatus != DomainResponseStatus.OK)
            {
                // Error adding to domain
                log.Error("Add New Domain Error - Domain = " + t.ToString());

            }
            return dr;
        }

        public override UserResponseObject SignIn(int siteGuid, int nMaxFailCount, int nLockMinutes, int nGroupID, string sessionID, string sIP, string deviceID, bool bPreventDoubleLogins)
        {
            string sUN = string.Empty;

            bool isGracePeriod = false;
            UserActivationState nUserStatus = GetUserStatus(ref sUN, ref siteGuid, ref isGracePeriod);

            if (nUserStatus != UserActivationState.Activated)
            {
                UserResponseObject o = new UserResponseObject();
                ResponseStatus ret = ResponseStatus.UserNotActivated;

                if (siteGuid <= 0)
                {
                    ret = ResponseStatus.WrongPasswordOrUserName;
                }
                else
                {
                    switch (nUserStatus)
                    {
                        case UserActivationState.UserDoesNotExist:
                            ret = ResponseStatus.UserDoesNotExist;
                            break;
                        //case UserActivationState.UserSuspended:
                        //    ret = ResponseStatus.UserSuspended;
                        //    break;
                        case UserActivationState.NotActivated:
                            o.m_user = new User(nGroupID, siteGuid);
                            ret = ResponseStatus.UserNotActivated;
                            break;
                        case UserActivationState.NotActivatedByMaster:
                            o.m_user = new User(nGroupID, siteGuid);
                            ret = ResponseStatus.UserNotMasterApproved;
                            break;
                        case UserActivationState.UserRemovedFromDomain:
                            o.m_user = new User(nGroupID, siteGuid);
                            ret = ResponseStatus.UserNotIndDomain;
                            break;
                        case UserActivationState.UserWIthNoDomain:
                            o.m_user = new User(nGroupID, siteGuid);
                            bool bValidDomainStat = CheckAddDomain(ref o, o.m_user, sUN, siteGuid);
                            if (!bValidDomainStat)
                                return o;
                            break;
                    }
                }

                if (nUserStatus != UserActivationState.UserWIthNoDomain && nUserStatus != UserActivationState.UserSuspended)
                {
                    o.m_RespStatus = ret;
                    return o;
                }
            }

            var response = User.SignIn(siteGuid, nMaxFailCount, nLockMinutes, m_nGroupID, sessionID, sIP, deviceID, bPreventDoubleLogins);
            if (response != null && response.m_user != null)
            {
                response.m_user.IsActivationGracePeriod = isGracePeriod;
            }
            return response;
        }

        public override UserResponseObject SignInWithToken(string sToken, int nMaxFailCount, int nLockMinutes, int nGroupID, string sessionID, string sIP, string deviceID, bool bPreventDoubleLogins)
        {
            //this function is for YesUsers
            return null;
        }

        public override UserResponseObject SignOut(int siteGuid, string sessionID, string sIP, string deviceUDID)
        {

            return User.SignOut(siteGuid, m_nGroupID, sessionID, sIP, deviceUDID);
        }

        public override UserState GetUserState(int siteGuid)
        {
            return User.GetCurrentUserState(siteGuid, m_nGroupID);
        }

        public override UserState GetUserInstanceState(int siteGuid, string sessionID, string sIP, string deviceID)
        {
            return User.GetCurrentUserInstanceState(siteGuid, sessionID, sIP, deviceID, m_nGroupID);
        }

        protected Int32 GetUserIDByUserName(string sUserName)
        {
            List<int> lGroupIDs = UtilsDal.GetAllRelatedGroups(m_nGroupID);
            string[] arrGroupIDs = lGroupIDs.Select(g => g.ToString()).ToArray();

            int userID = DAL.UsersDal.GetUserIDByUsername(sUserName, arrGroupIDs);

            return userID;
        }

        protected Int32 GetUserIDByToken(string sToken)
        {
            Int32 nRet = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetConnectionKey("USERS_CONNECTION_STRING");
            selectQuery += "select id from users where status=1 and is_active=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("CP_TOKEN", "=", sToken);
            selectQuery += " and ";
            selectQuery += " group_id " + TVinciShared.PageUtils.GetFullChildGroupsStr(m_nGroupID, "MAIN_CONNECTION_STRING");
            selectQuery += " and CP_TOKEN_LAST_DATE>getdate()";
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    nRet = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return nRet;
        }

        protected int GetUserIDByActivationToken(string sToken)
        {
            List<int> lGroupIDs = UtilsDal.GetAllRelatedGroups(m_nGroupID);
            string[] arrGroupIDs = lGroupIDs.Select(g => g.ToString()).ToArray();

            int nRet = DAL.UsersDal.GetUserIDByActivationToken(sToken, arrGroupIDs);
            return nRet;
        }

        public override UserResponseObject CheckToken(string sToken)
        {
            User u = new User();
            Int32 nID = GetUserIDByToken(sToken);
            UserResponseObject resp = new UserResponseObject();

            if (nID != 0)
            {
                u.Initialize(nID, m_nGroupID, false);
                resp.m_user = u;
                resp.m_RespStatus = ResponseStatus.OK;

                ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("users");
                updateQuery.SetConnectionKey("USERS_CONNECTION_STRING");
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("CP_TOKEN", "=", DBNull.Value);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("CP_TOKEN_LAST_DATE", "=", DateTime.UtcNow.AddDays(-15));
                updateQuery += " where ";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nID);
                updateQuery.Execute();
                updateQuery.Finish();
                updateQuery = null;

            }
            else
            {
                resp.m_RespStatus = ResponseStatus.UserDoesNotExist;
            }
            return resp;
        }

        public override UserResponseObject ActivateAccount(string sUN, string sToken)
        {
            User u = new User();
            Int32 nID = GetUserIDByUserName(sUN);
            UserResponseObject resp = new UserResponseObject();
            int tokenUserID = GetUserIDByActivationToken(sToken);

            if (nID != 0 && nID == tokenUserID)
            {
                u.Initialize(nID, m_nGroupID, false);


                List<int> lGroupIDs = UtilsDal.GetAllRelatedGroups(m_nGroupID);
                string[] arrGroupIDs = lGroupIDs.Select(g => g.ToString()).ToArray();
                bool isActivated = DAL.UsersDal.UpdateUserActivationToken(arrGroupIDs, nID, sToken, Guid.NewGuid().ToString(), (int)UserState.LoggedOut);

                if (isActivated)
                {
                    bool resetSession = DAL.UsersDal.SetUserSessionStatus(nID, 0, 0);
                    // remove user from cache
                    UsersCache usersCache = UsersCache.Instance();
                    usersCache.RemoveUser(nID, m_nGroupID);
                }
                try
                {
                    Notifiers.BaseUsersNotifier t = null;
                    Notifiers.Utils.GetBaseUsersNotifierImpl(ref t, m_nGroupID);
                    if (t != null)
                    {
                        t.NotifyChange(nID.ToString());
                    }
                }
                catch (Exception ex)
                {
                    log.Error("exception - " + nID.ToString() + " : " + ex.Message, ex);
                }


                int nActivationStatus = DAL.UsersDal.GetUserActivateStatus(nID, arrGroupIDs);

                if (nActivationStatus == 1)
                {
                    resp.m_user = u;
                    resp.m_RespStatus = ResponseStatus.OK;
                }
                else
                {
                    resp.m_user = null;
                    resp.m_RespStatus = ResponseStatus.UserNotActivated;
                }

            }
            else
            {
                resp.m_RespStatus = ResponseStatus.UserDoesNotExist;
            }

            return resp;
        }

        public override UserResponseObject ActivateAccountByDomainMaster(string sMasterUN, string sUN, string sToken)
        {
            UserResponseObject resp = new UserResponseObject();

            int nMasterUserID = GetUserIDByUserName(sMasterUN);
            int nUserID = GetUserIDByUserName(sUN);

            User newUser = new User();
            User masterUser = new User();
            bool bInit = newUser.Initialize(nUserID, m_nGroupID, false) && masterUser.Initialize(nMasterUserID, m_nGroupID, false);

            if (nUserID <= 0 || nMasterUserID <= 0 || !bInit || !masterUser.m_isDomainMaster)
            {
                resp.m_user = null;
                resp.m_RespStatus = ResponseStatus.UserDoesNotExist;

                return resp;
            }

            // Check if user already activated by master
            string sNewUserName = sUN;
            int nDbUserID = nUserID;
            bool isGracePeriod = false;
            UserActivationState curActState = GetUserActivationStatus(ref sNewUserName, ref nDbUserID, ref isGracePeriod);

            // Find user ID by given token
            int nUsersDomainID = 0;
            int nTokenUserID = DAL.DomainDal.GetUserIDByDomainActivationToken(m_nGroupID, sToken, ref nUsersDomainID);

            if ((nUserID == nDbUserID) && (string.Compare(sNewUserName, sUN, true) == 0) &&
                (curActState == UserActivationState.Activated) && (nUserID != nTokenUserID))
            {
                resp.m_user = newUser;
                resp.m_RespStatus = ResponseStatus.UserAlreadyMasterApproved;

                return resp;
            }


            if ((curActState != UserActivationState.Activated) && (nUserID != nTokenUserID))
            {
                resp.m_user = null;
                resp.m_RespStatus = ResponseStatus.TokenNotFound;

                return resp;
            }


            // Activate the user
            string sNewGuid = Guid.NewGuid().ToString();
            bool isActivated = DAL.DomainDal.UpdateUserDomainActivationToken(m_nGroupID, nUsersDomainID, sToken, sNewGuid);

            if (isActivated)
            {
                bool resetSession = DAL.UsersDal.SetUserSessionStatus(nUserID, 0, 0);
            }


            int nActivationStatus = DAL.DomainDal.GetUserDomainActivateStatus(m_nGroupID, nUserID);

            resp.m_user = (nActivationStatus == 1) ? newUser : null;
            resp.m_RespStatus = (nActivationStatus == 1) ? ResponseStatus.OK : ResponseStatus.UserNotActivated;


            try
            {
                Notifiers.BaseUsersNotifier t = null;
                Notifiers.Utils.GetBaseUsersNotifierImpl(ref t, m_nGroupID);
                if (t != null)
                {
                    t.NotifyChange(nUserID.ToString());
                }
            }
            catch (Exception ex)
            {
                log.Error("exception - " + nUserID.ToString() + " : " + ex.Message, ex);
            }

            return resp;
        }

        public override UserResponseObject AddNewUser(UserBasicData oBasicData, UserDynamicData sDynamicData, string sPassword)
        {
            UserResponseObject resp = new UserResponseObject();
            User u = new User();
            // if username or password empty return with WrongPasswordOrUserName response
            if (string.IsNullOrEmpty(oBasicData.m_sUserName) || string.IsNullOrEmpty(sPassword))
            {
                resp.Initialize(ResponseStatus.WrongPasswordOrUserName, u);
                return resp;
            }

            if (!string.IsNullOrEmpty(oBasicData.m_sUserName) && oBasicData.m_sUserName.ToLower().Contains("anonymous"))
            {
                oBasicData.m_sUserName = string.Format(oBasicData.m_sUserName + "_{0}", User.GetNextGUID());
            }

            int userID = GetUserIDByUserName(oBasicData.m_sUserName);
            if (userID > 0)
            {
                resp.Initialize(ResponseStatus.UserExists, u);
                return resp;
            }

            if (!Utils.SetPassword(sPassword, ref oBasicData, m_nGroupID))
            {
                resp.Initialize(ResponseStatus.WrongPasswordOrUserName, u);
                return resp;
            }

            u.InitializeBasicAndDynamicData(oBasicData, sDynamicData);
            
            int nUserID = u.Save(m_nGroupID, !IsActivationNeeded(oBasicData), true);    //u.Save(m_nGroupID);  

            // add role to user
            if (nUserID > 0)
            {
                long roleId;

                if (DAL.UsersDal.IsUserDomainMaster(m_nGroupID, nUserID))
                {
                    long.TryParse(Utils.GetTcmConfigValue("master_role_id"), out roleId);
                }
                else
                {
                    long.TryParse(Utils.GetTcmConfigValue("user_role_id"), out roleId);
                }

                if (roleId != 0)
                {
                    DAL.UsersDal.Insert_UserRole(m_nGroupID, nUserID.ToString(), roleId, true);
                }
                else
                {
                    resp.m_RespStatus = ResponseStatus.UserCreatedWithNoRole;
                    log.ErrorFormat("User created with no role. userId = {0}", userID);
                }
            }

            if (u.m_domianID <= 0)
            {
                bool bValidDomainStatus = CheckAddDomain(ref resp, u, oBasicData.m_sUserName, nUserID);
            }
            else
            {
                resp.Initialize(ResponseStatus.OK, u);
            }

            CreateDefaultRules(u.m_sSiteGUID, m_nGroupID);

            string sNewsLetter = sDynamicData.GetValByKey("newsletter");
            if (!string.IsNullOrEmpty(sNewsLetter) && sNewsLetter.ToLower().Equals("true"))
            {
                if (m_newsLetterImpl != null)
                {
                    if (!m_newsLetterImpl.IsUserSubscribed(u))
                    {
                        m_newsLetterImpl.Subscribe(resp.m_user);
                    }
                }
            }

            //Send Welcome Email
            if (m_mailImpl != null)
            {
                SendMailImpl(resp.m_user);
            }
            else
            {
                WelcomeMailRequest sMailReq = GetWelcomeMailRequest(GetUniqueTitle(oBasicData, sDynamicData), oBasicData.m_sUserName, sPassword, oBasicData.m_sEmail, oBasicData.m_sFacebookID);

                log.DebugFormat("params for welcom mail ws_users sMailReq.m_sSubject={0}, oBasicData.m_sUserName={1}, sMailReq.m_sTemplateName={2}", sMailReq.m_sSubject, oBasicData.m_sUserName, sMailReq.m_sTemplateName);
                bool sendingMailResult = Utils.SendMail(m_nGroupID, sMailReq);
            }

            return resp;
        }

        protected bool CheckAddDomain(ref UserResponseObject resp, User u, string sUserName, int nUserID)
        {
            bool succeded = true;
            //check if user needs a domain           
            bool bDomainIsMandatory = DAL.DomainDal.IsSingleDomainEnvironment(m_nGroupID);
            if (bDomainIsMandatory)
            {
                //add new domain
                DomainResponseObject dResp = AddNewDomain(sUserName, nUserID, m_nGroupID);
                if (dResp != null && dResp.m_oDomain != null)
                {
                    u.m_domianID = dResp.m_oDomain.m_nDomainID;
                    //Remove user from cache
                    UsersCache usersCache = UsersCache.Instance();
                    usersCache.RemoveUser(nUserID, m_nGroupID);
                }

                if (dResp.m_oDomainResponseStatus != DomainResponseStatus.OK)
                {
                    resp.Initialize(ResponseStatus.UserWithNoDomain, u);
                    succeded = false;
                }
                else
                    resp.Initialize(ResponseStatus.OK, u);
            }
            else
            {
                resp.Initialize(ResponseStatus.UserWithNoDomain, u);
            }
            return succeded;
        }

        protected bool SendMailImpl(User user)
        {
            string sMCTemplate = user.m_oDynamicData.GetValByKey("mailtemplate");
            if (!string.IsNullOrEmpty(sMCTemplate))
            {
                int val = int.Parse(sMCTemplate);

                //Dont send email
                if (val == 0)
                {
                    return false;
                }

                m_mailImpl.m_nRuleID = int.Parse(sMCTemplate);
            }

            return m_mailImpl.SendMail(user);
        }

        public bool CreateDefaultRules(string sSiteGuid, int nGroupID)
        {
            // return client.SetDefaultRules(sWSUserName, sWSPass, sSiteGuid);
            return true;
        }

        public override bool ResendActivationMail(string username)
        {
            Int32 userID = GetUserIDByUserName(username);
            if (userID == 0)
                return false;

            User u = new User();
            if (!u.Initialize(userID, m_nGroupID, false))
                return false;

            if (u != null && u.m_oBasicData != null && !string.IsNullOrEmpty(u.m_oBasicData.m_sPassword))
            {
                WelcomeMailRequest mailReq = GetWelcomeMailRequest(u.m_oBasicData.m_sFirstName, u.m_oBasicData.m_sUserName, u.m_oBasicData.m_sPassword, u.m_oBasicData.m_sEmail, u.m_oBasicData.m_sFacebookID);
                if (Utils.SendMail(m_nGroupID, mailReq))
                    return true;
            }
            return false;
        }

        public override bool ResendWelcomeMail(string sUN)
        {
            Int32 nUserID = GetUserIDByUserName(sUN);
            User u = new User();
            u.Initialize(nUserID, m_nGroupID, false);

            if (u.m_oBasicData.m_sPassword != "")
            {
                if (m_mailImpl != null)
                {
                    return SendMailImpl(u);
                }
                else
                {
                    WelcomeMailRequest sMailReq = GetWelcomeMailRequest(u.m_oBasicData.m_sFirstName, u.m_oBasicData.m_sUserName, u.m_oBasicData.m_sPassword, u.m_oBasicData.m_sEmail, u.m_oBasicData.m_sFacebookID);
                    bool sendingMailResult = Utils.SendMail(m_nGroupID, sMailReq);
                    return true;
                }
            }
            return false;
        }

        public override bool DoesUserNameExists(string sUserName)
        {
            Int32 nUserID = GetUserIDByUserName(sUserName);
            if (nUserID == 0)
                return false;
            return true;
        }

        public override UserResponseObject AddNewUser(string sBasicDataXML, string sDynamicDataXML, string sPassword)
        {
            UserBasicData b = new UserBasicData();
            b.Initialize(sBasicDataXML);

            bool bOk = Users.Utils.SetPassword(sPassword, ref b, m_nGroupID);
            if (bOk == false)
            {
                UserResponseObject ret = new UserResponseObject();
                ret.m_RespStatus = ResponseStatus.WrongPasswordOrUserName;
                ret.m_user = null;
                return ret;
            }

            UserDynamicData d = new UserDynamicData();
            d.Initialize(sDynamicDataXML);
            return AddNewUser(b, d, "");
        }

        public override UserResponseObject GetUserByCoGuid(string sCoGuid, int operatorID)
        {
            UserResponseObject retVal = new UserResponseObject();
            retVal.m_RespStatus = ResponseStatus.UserDoesNotExist;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetConnectionKey("USERS_CONNECTION_STRING");
            selectQuery += " select id from users where is_active = 1 and status = 1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("coguid", "=", sCoGuid);
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", m_nGroupID);
            if (selectQuery.Execute("query", true) != null)
            {
                int count = selectQuery.Table("query").DefaultView.Count;
                if (count > 0)
                {
                    string sID = selectQuery.Table("query").DefaultView[0].Row["id"].ToString();
                    retVal = GetUserData(sID);
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return retVal;

        }



        public override UserResponseObject GetUserData(string sSiteGUID, bool shouldSaveInCache = true)
        {
            try
            {
                Int32 nUserID = int.Parse(sSiteGUID);
                User u = new User();

                u.Initialize(nUserID, m_nGroupID, shouldSaveInCache);

                if (m_newsLetterImpl != null)
                {
                    if (u.m_oDynamicData != null && u.m_oDynamicData.GetDynamicData() != null)
                    {
                        foreach (UserDynamicDataContainer udc in u.m_oDynamicData.GetDynamicData())
                        {
                            if (udc.m_sDataType.ToLower().Equals("newsletter"))
                            {
                                udc.m_sValue = m_newsLetterImpl.IsUserSubscribed(u).ToString().ToLower();
                            }
                        }
                    }
                }
                UserResponseObject resp = new UserResponseObject();
                if (u.m_oBasicData.m_sUserName == "")
                {
                    resp.Initialize(ResponseStatus.UserDoesNotExist, u);
                }
                else
                {
                    resp.Initialize(ResponseStatus.OK, u);
                }
                return resp;
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder(String.Concat("Exception at GetUserData. Site Guid: ", sSiteGUID));
                sb.Append(String.Concat(" G ID: ", m_nGroupID));
                sb.Append(String.Concat(" Ex Msg: ", ex.Message));
                sb.Append(String.Concat(" Ex Type: ", ex.GetType().Name));
                sb.Append(String.Concat(" ST: ", ex.StackTrace));
                log.Error("Exception - " + sb.ToString(), ex);
                throw;
            }
        }

        public override List<UserResponseObject> GetUsersData(string[] sSiteGUIDs)
        {
            try
            {
                List<UserResponseObject> resp = new List<UserResponseObject>();


                for (int i = 0; i < sSiteGUIDs.Length; i++)
                {

                    try
                    {
                        UserResponseObject temp = GetUserData(sSiteGUIDs[i]);
                        if (temp != null)
                        {
                            resp.Add(temp);
                        }
                    }
                    catch
                    {

                    }

                }
                return resp;
            }
            catch
            {
                return null;
            }
        }

        public override List<UserBasicData> GetUsersBasicData(long[] nSiteGUIDs)
        {
            try
            {
                List<UserBasicData> resp = new List<UserBasicData>();

                DataTable dtUserBasicData = UsersDal.GetUsersBasicData(nSiteGUIDs);

                if (dtUserBasicData != null)
                {
                    Int32 nCount = dtUserBasicData.DefaultView.Count;

                    if (nCount > 0)
                    {
                        for (int i = 0; i < dtUserBasicData.Rows.Count; i++)
                        {
                            try
                            {
                                string sUserName = dtUserBasicData.DefaultView[i].Row["USERNAME"].ToString();
                                string sPass = dtUserBasicData.DefaultView[i].Row["PASSWORD"].ToString();
                                string sSalt = dtUserBasicData.DefaultView[i].Row["SALT"].ToString();
                                string sFirstName = dtUserBasicData.DefaultView[i].Row["FIRST_NAME"].ToString();
                                string sLastName = dtUserBasicData.DefaultView[i].Row["LAST_NAME"].ToString();
                                string sEmail = dtUserBasicData.DefaultView[i].Row["EMAIL_ADD"].ToString();

                                string sAddress = dtUserBasicData.DefaultView[i].Row["ADDRESS"].ToString();
                                object oAffiliates = dtUserBasicData.DefaultView[i].Row["REG_AFF"];
                                string sAffiliate = "";
                                if (oAffiliates != null && oAffiliates != DBNull.Value)
                                    sAffiliate = oAffiliates.ToString();
                                string sCity = dtUserBasicData.DefaultView[i].Row["CITY"].ToString();
                                Int32 nStateID = int.Parse(dtUserBasicData.DefaultView[i].Row["STATE_ID"].ToString());
                                Int32 nCountryID = int.Parse(dtUserBasicData.DefaultView[i].Row["COUNTRY_ID"].ToString());
                                string sZip = dtUserBasicData.DefaultView[i].Row["ZIP"].ToString();
                                string sPhone = dtUserBasicData.DefaultView[i].Row["PHONE"].ToString();
                                object oFacebookID = dtUserBasicData.DefaultView[i].Row["FACEBOOK_ID"];
                                object oFacebookImage = dtUserBasicData.DefaultView[i].Row["FACEBOOK_IMAGE"];
                                Int32 nFacebookImagePermitted = int.Parse(dtUserBasicData.DefaultView[i].Row["FACEBOOK_IMAGE_PERMITTED"].ToString());
                                string sCoGuid = dtUserBasicData.DefaultView[i].Row["CoGuid"].ToString();
                                string sExternalToken = dtUserBasicData.DefaultView[i].Row["ExternalToken"].ToString();
                                bool bFacebookImagePermitted = false;
                                if (nFacebookImagePermitted == 1)
                                    bFacebookImagePermitted = true;
                                string sFacebookImage = "";
                                if (oFacebookImage != null && oFacebookImage != DBNull.Value)
                                    sFacebookImage = oFacebookImage.ToString();
                                string sFacebookID = "";
                                if (oFacebookID != null && oFacebookID != DBNull.Value)
                                    sFacebookID = oFacebookID.ToString();


                                string sFacebookToken = ODBCWrapper.Utils.GetSafeStr(dtUserBasicData.DefaultView[i].Row["fb_token"]);

                                int? nUserTypeID = ODBCWrapper.Utils.GetIntSafeVal(dtUserBasicData.DefaultView[i].Row["user_type_id"]);
                                if (nUserTypeID == 0)
                                {
                                    nUserTypeID = null;
                                }
                                string sUserType = ODBCWrapper.Utils.GetSafeStr(dtUserBasicData.DefaultView[i].Row["user_type_desc"]);
                                bool isDefault = Convert.ToBoolean(ODBCWrapper.Utils.GetByteSafeVal(dtUserBasicData.DefaultView[i].Row, "is_default"));
                                UserType userType = new UserType(nUserTypeID, sUserType, isDefault);

                                UserBasicData userBasicData = new UserBasicData();

                                userBasicData.Initialize(sUserName, sPass, sSalt, sFirstName, sLastName, sEmail, sAddress,
                                    sCity, nStateID, nCountryID, sZip, sPhone, sFacebookID, bFacebookImagePermitted, sFacebookImage, sAffiliate, sFacebookToken, sCoGuid, sExternalToken, userType);

                                resp.Add(userBasicData);
                            }
                            catch
                            {

                            }
                        }
                    }
                }

                return resp;
            }
            catch
            {
                return null;
            }
        }

        public override List<UserBasicData> SearchUsers(string[] sTerms, string[] sFields, bool bIsExact)
        {
            List<UserBasicData> resp = new List<UserBasicData>();

            try
            {
                string mainKey = string.Format("{0}_GetGroupUsersSearchFields_{1}", eWSModules.USERS, m_nGroupID);
                DataTable dtFields;
                bool bExists = UsersCache.GetItem<DataTable>(mainKey, out dtFields);
                if (!bExists)
                {
                    dtFields = UsersDal.GetGroupUsersSearchFields(m_nGroupID);
                    if (dtFields != null && dtFields.Rows != null && dtFields.Rows.Count > 0)
                    {
                        UsersCache.AddItem(mainKey, dtFields);
                    }
                }


                if (dtFields != null && dtFields.Rows.Count > 0)
                {
                    string[] sGroupUsersSearchFields = (from row in dtFields.AsEnumerable()
                                                        select row["name"].ToString()).ToArray();

                    string[] sRequestSearchFields = null;

                    if (sFields == null || sFields.Length == 0)
                    {
                        sRequestSearchFields = sGroupUsersSearchFields;
                    }
                    else
                    {
                        sRequestSearchFields = (from row in dtFields.AsEnumerable()
                                                where sFields.Contains(row["friendly_name"].ToString())
                                                select row["name"].ToString()).ToArray();
                    }

                    if (sGroupUsersSearchFields != null && sGroupUsersSearchFields.Length > 0)
                    {
                        DataTable dtGroupUsers = null;
                        string key = string.Format("{0}_GroupUsers_{1}", eWSModules.USERS, m_nGroupID);
                        string dateTimeKey = string.Format("{0}_GroupUsersTimeStamp_{1}", eWSModules.USERS, m_nGroupID);

                        bool bRes = UsersCache.GetItem<DataTable>(key, out dtGroupUsers);
                        if (bRes)
                        {
                            int cache_period = 10;

                            if (TVinciShared.WS_Utils.GetTcmConfigValue("SEARCH_USERS_CACHE_PERIOD") != string.Empty)
                                int.TryParse(TVinciShared.WS_Utils.GetTcmConfigValue("SEARCH_USERS_CACHE_PERIOD"), out cache_period);

                            DateTime timeStamp;
                            bRes = UsersCache.GetItem<DateTime>(dateTimeKey, out timeStamp);

                            if ((DateTime.UtcNow - timeStamp).TotalMinutes >= cache_period)
                            {
                                // save monitor and logs context data
                                ContextData contextData = new ContextData();

                                Thread thread = new Thread(() =>
                                {
                                    // load monitor and logs context data
                                    contextData.Load();

                                    lock (lockObj)
                                    {
                                        bRes = UsersCache.GetItem<DateTime>(dateTimeKey, out timeStamp);

                                        if ((DateTime.UtcNow - timeStamp).TotalMinutes >= cache_period)
                                        {
                                            dtGroupUsers = UsersDal.GetGroupUsers(m_nGroupID, sGroupUsersSearchFields);

                                            if (dtGroupUsers != null)
                                            {
                                                UsersCache.AddItem(key, dtGroupUsers);
                                                UsersCache.AddItem(dateTimeKey, DateTime.UtcNow);
                                            }
                                        }
                                    }
                                });

                                thread.Start();
                            }
                        }
                        else
                        {
                            dtGroupUsers = UsersDal.GetGroupUsers(m_nGroupID, sGroupUsersSearchFields);

                            if (dtGroupUsers != null)
                            {
                                UsersCache.AddItem(key, dtGroupUsers);
                                UsersCache.AddItem(dateTimeKey, DateTime.UtcNow);
                            }
                        }

                        if (dtGroupUsers != null && dtGroupUsers.Rows.Count > 0)
                        {
                            string sQuery = string.Empty;

                            for (int i = 0; i < sTerms.Length; i++)
                            {
                                for (int j = 0; j < sRequestSearchFields.Length; j++)
                                {
                                    if (bIsExact)
                                        sQuery += string.Format("{0} = '{1}'", sRequestSearchFields[j], sTerms[i]);
                                    else
                                        sQuery += string.Format("{0} like '%{1}%'", sRequestSearchFields[j], sTerms[i]);

                                    if (j < (sRequestSearchFields.Length - 1))
                                        sQuery += " OR ";
                                }

                                if (i < (sTerms.Length - 1))
                                    sQuery += " OR ";
                            }

                            var filteredDataTable = dtGroupUsers.Select(sQuery);

                            if (filteredDataTable.Length > 0)
                            {
                                int limit = 40;


                                if (TVinciShared.WS_Utils.GetTcmConfigValue("SEARCH_USERS_RESULTS_LIMIT") != string.Empty)
                                    int.TryParse(TVinciShared.WS_Utils.GetTcmConfigValue("SEARCH_USERS_RESULTS_LIMIT"), out limit);

                                long[] usersIDs = filteredDataTable.Take(limit).Select(x => ODBCWrapper.Utils.GetLongSafeVal(x["id"])).ToArray();

                                resp = GetUsersBasicData(usersIDs);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("SearchUsers error - " + string.Format("Terms: {0}, Fields: {1}, IsExact: {2}, Error: {3}", string.Join(";", sTerms), string.Join(";", sFields), bIsExact, ex.Message), ex);


                return null;
            }

            return resp;
        }

        public override string GetUserToken(string sSiteGUID, Int32 nGroupID)
        {
            Int32 nSiteUserID = 0;
            try
            {
                nSiteUserID = int.Parse(sSiteGUID);
            }
            catch
            {
                return "";
            }
            User u = new User();
            u.Initialize(nSiteUserID, nGroupID);
            return u.GetUserToken();
        }

        public override UserResponseObject SetUserData(string sSiteGUID, string sBasicDataXML, string sDynamicDataXML)
        {
            try
            {
                UserResponseObject resp = new UserResponseObject();
                Int32 nUserID = int.Parse(sSiteGUID);
                User u = new User();
                u.Initialize(nUserID, m_nGroupID, false);
                if (u.m_oBasicData.m_sUserName != "")
                {
                    UserBasicData b = new UserBasicData();
                    b.Initialize(sBasicDataXML);
                    UserDynamicData d = new UserDynamicData();
                    d.Initialize(sDynamicDataXML);
                    u.Update(b, d, m_nGroupID);
                    if (u.m_eSuspendState == DomainSuspentionStatus.Suspended)
                    {
                        resp.Initialize(ResponseStatus.UserSuspended, u);
                    }
                    else
                    {
                        resp.Initialize(ResponseStatus.OK, u);
                    }
                }
                else
                {
                    resp.Initialize(ResponseStatus.UserDoesNotExist, null);
                }
                return resp;
            }
            catch
            {
                return null;
            }
        }

        public override UserResponseObject SetUserData(string sSiteGUID, UserBasicData oBasicData, UserDynamicData sDynamicData)
        {
            UserResponseObject resp = new UserResponseObject();

            try
            {
                Int32 nUserID = int.Parse(sSiteGUID);
                User u = new User(m_nGroupID, nUserID);

                bool isSubscribeNewsLetter = false;
                bool isUnSubscribeNewsLeter = false;

                if (string.IsNullOrEmpty(u.m_oBasicData.m_sUserName))
                {
                    resp.Initialize(ResponseStatus.UserDoesNotExist, null);
                    return resp;
                }

                if (m_newsLetterImpl != null)
                {
                    if (sDynamicData != null && u.m_oDynamicData != null)
                    {
                        bool isNewNewsLetter = false;
                        bool isOldNewsLetter = false;

                        foreach (UserDynamicDataContainer data in sDynamicData.GetDynamicData())
                        {
                            if (data.m_sDataType.ToLower().Equals("newsletter") && data.m_sValue.ToLower().Equals("true"))
                            {
                                isNewNewsLetter = true;
                                break;
                            }
                        }

                        foreach (UserDynamicDataContainer olddata in u.m_oDynamicData.GetDynamicData())
                        {
                            if (olddata.m_sDataType.ToLower().Equals("newsletter") && olddata.m_sValue.ToLower().Equals("true"))
                            {
                                isOldNewsLetter = true;
                                break;
                            }
                        }

                        isSubscribeNewsLetter = (isNewNewsLetter && !isOldNewsLetter);
                        isUnSubscribeNewsLeter = (isOldNewsLetter && !isNewNewsLetter);

                        if (isNewNewsLetter && oBasicData.m_sEmail != u.m_oBasicData.m_sEmail)
                        {
                            m_newsLetterImpl.UnSubscribe(u);
                            isSubscribeNewsLetter = true;
                        }

                    }
                }

                int saveID = u.Update(oBasicData, sDynamicData, m_nGroupID);
                // failed updating basicData or dynmaicData
                if (saveID == -1)
                {                    
                    resp.Initialize(ResponseStatus.WrongPasswordOrUserName, null);
                    return resp;
                }
                if (isSubscribeNewsLetter && m_newsLetterImpl != null)
                {
                    m_newsLetterImpl.Subscribe(u);
                }
                else
                {
                    if (isUnSubscribeNewsLeter)
                    {
                        m_newsLetterImpl.UnSubscribe(u);

                    }
                }

                if (u.m_eSuspendState == DomainSuspentionStatus.Suspended)
                {
                    resp.Initialize(ResponseStatus.UserSuspended, u);
                }
                else
                {
                    resp.Initialize(ResponseStatus.OK, u);
                }
            }
            catch
            {
                return null;
            }

            return resp;
        }

        public override UserResponseObject ChangeUserPassword(string sUN, string sOldPass, string sPass, int nGroupID)
        {
            UserResponseObject ret = new UserResponseObject();
            UserResponseObject uro = User.CheckUserPassword(sUN, sOldPass, 3, 3, nGroupID, false, true);
            if (uro.m_RespStatus != ResponseStatus.OK)
            {
                ret.m_RespStatus = uro.m_RespStatus;
                ret.m_user = null;
                return ret;
            }

            bool bOk = Users.Utils.SetPassword(sPass, ref uro.m_user.m_oBasicData, nGroupID);
            if (bOk == false)
            {
                ret.m_RespStatus = ResponseStatus.WrongPasswordOrUserName;
                ret.m_user = null;
                return ret;
            }

            uro.m_user.Save(m_nGroupID, false, true);
            ret.m_user = uro.m_user;
            ret.m_RespStatus = ResponseStatus.OK;
            return ret;
        }

        public override ApiObjects.Response.Status UpdateUserPassword(int userId, string password)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            User user = new User();
            if (!user.Initialize(userId, m_nGroupID, false))
            {
                response = new ApiObjects.Response.Status((int)eResponseStatus.Error, "Failed to get user data");
                return response;
            }

            if (!Users.Utils.SetPassword(password, ref user.m_oBasicData, m_nGroupID))
            {
                response = new ApiObjects.Response.Status((int)eResponseStatus.Error, "Failed to set user password");
                return response;
            }

            if (user.Save(m_nGroupID, false, true) != userId)
            {
                response = new ApiObjects.Response.Status((int)eResponseStatus.Error, "Failed to save user data");
                return response;
            }

            response = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            return response;
        }

        public override UserResponseObject RenewPassword(string sUN, string sPass, int nGroupID)
        {
            UserResponseObject ret = new UserResponseObject();
            Int32 nID = GetUserIDByUserName(sUN);
            User u = new User();
            u.Initialize(nID, m_nGroupID, false);
            if (string.IsNullOrEmpty(u.m_oBasicData.m_sPassword))
            {
                ret.m_RespStatus = ResponseStatus.UserDoesNotExist;
                ret.m_user = null;
                return ret;
            }

            bool bOk = Users.Utils.SetPassword(sPass, ref u.m_oBasicData, nGroupID);
            if (bOk == false)
            {
                ret.m_RespStatus = ResponseStatus.WrongPasswordOrUserName;
                ret.m_user = null;
                return ret;
            }

            u.Save(m_nGroupID, false, true);
            ret.m_user = u;
            ret.m_RespStatus = ResponseStatus.OK;
            return ret;
        }

        private bool UserGenerateToken(string sUN, ref string sEmail, ref Int32 nID, ref string sFirstName, ref string sToken)
        {
            DataTable dt = DAL.UsersDal.GenerateToken(sUN, m_nGroupID, m_nTokenValidityHours);

            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
            {
                DataRow dr = dt.Rows[0];

                sEmail = ODBCWrapper.Utils.GetSafeStr(dr["EMAIL_ADD"]);
                nID = ODBCWrapper.Utils.GetIntSafeVal(dr["ID"]);
                sFirstName = ODBCWrapper.Utils.GetSafeStr(dr["FIRST_NAME"]);
                sToken = ODBCWrapper.Utils.GetSafeStr(dr["TOKEN"]);
            }
            else
            {
                return false;
            }

            return true;
        }

        public override UserResponseObject ForgotPassword(string sUN)
        {
            UserResponseObject ret = new UserResponseObject();

            string sEmail = string.Empty;
            Int32 nID = 0;
            string sFirstName = string.Empty;
            string sToken = string.Empty;

            if (UserGenerateToken(sUN, ref sEmail, ref nID, ref sFirstName, ref sToken) == true)
            {
                //Send ForgotPasswordMail
                ForgotPasswordMailRequest sMailRequest = GetForgotPasswordMailRequest(sFirstName, sEmail, sToken);

                log.Debug("Forgot Pass - Start send to " + sEmail + " from" + m_sForgotPasswordMail);


                Utils.SendMail(m_nGroupID, sMailRequest);

                ret.Initialize(ResponseStatus.OK, null);
            }
            else
            {
                ret.Initialize(ResponseStatus.UserDoesNotExist, null);
            }

            return ret;
        }

        public override UserResponseObject ChangePassword(string sUN)
        {
            UserResponseObject ret = new UserResponseObject();

            string sEmail = string.Empty;
            Int32 nID = 0;
            string sFirstName = string.Empty;
            string sToken = string.Empty;

            if (UserGenerateToken(sUN, ref sEmail, ref nID, ref sFirstName, ref sToken) == true)
            {
                //Send ChangePassword
                ChangePasswordMailRequest sMailRequest = GetChangePasswordMailRequest(sFirstName, sEmail, sToken);

                log.Debug("Change Password - Start send to " + sEmail + " from" + m_sForgotPasswordMail);

                bool sendingMailResult = Utils.SendMail(m_nGroupID, sMailRequest);

                ret.Initialize(ResponseStatus.OK, null);
            }
            else
            {
                ret.Initialize(ResponseStatus.UserDoesNotExist, null);
            }

            return ret;
        }

        public override ResponseStatus SendChangedPinMail(string sSiteGuid, int nUserRuleID)
        {
            int nSiteGuid = 0;
            bool result = int.TryParse(sSiteGuid, out nSiteGuid);
            ResponseStatus ret = default(ResponseStatus);
            if (result == true)
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey("USERS_CONNECTION_STRING");
                selectQuery += "select username, email_add from users(nolock) where is_active=1 and status=1 and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nSiteGuid);

                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        List<GroupRule> groupRulesList = GetUserGroupsRules(m_nGroupID, sSiteGuid);
                        GroupRule groupRule = groupRulesList.Find(
                                                        delegate(GroupRule rule)
                                                        {
                                                            return rule.RuleID == nUserRuleID;
                                                        }
                                                        );

                        string ruleName = string.Empty;
                        if (groupRule != null)
                        {
                            ruleName = groupRule.Name;
                        }


                        string sUn = selectQuery.Table("query").DefaultView[0].Row["username"].ToString();
                        string sEmail = selectQuery.Table("query").DefaultView[0].Row["email_add"].ToString();
                        //Create token
                        string changePinToken = System.Guid.NewGuid().ToString();
                        DateTime changePinTokenLastDate = DateTime.UtcNow.AddHours(m_nChangePinTokenValidityHours);
                        ApiDAL.Update_UserGroupRule_Token(nSiteGuid, nUserRuleID, changePinToken, changePinTokenLastDate);
                        ChangedPinMailRequest sMailRequest = GetChangedPinMailRequest(sUn, sEmail, nSiteGuid.ToString(), changePinToken, ruleName);
                        log.Debug("Change pin code - Start send to " + sEmail + " from" + m_sChangedPinMail);

                        bool sendingMailResult = Utils.SendMail(m_nGroupID, sMailRequest);
                        if (sendingMailResult == true)
                        {
                            ret = ResponseStatus.OK;
                        }
                        else
                        {
                            ret = ResponseStatus.ErrorOnSendingMail;
                        }
                    }
                    else
                    {
                        ret = ResponseStatus.UserDoesNotExist;
                    }
                }
                selectQuery.Finish();
                selectQuery = null;
            }
            else
            {
                ret = ResponseStatus.UserDoesNotExist;
            }
            return ret;
        }

        public override UserGroupRuleResponse CheckParentalPINToken(string sChangePinToken)
        {
            UserGroupRuleResponse result = new UserGroupRuleResponse();
            result.ResponseStatus = UserGroupRuleResponseStatus.OK;

            DataTable dtUserGroupRule = ApiDAL.Check_UserGroupRule_Token(sChangePinToken);

            if (dtUserGroupRule != null && dtUserGroupRule.Rows.Count > 0)
            {
                int siteGuid = ODBCWrapper.Utils.GetIntSafeVal(dtUserGroupRule.DefaultView[0]["user_site_guid"]);
                int ruleID = ODBCWrapper.Utils.GetIntSafeVal(dtUserGroupRule.DefaultView[0]["rule_id"]);
                string changePinToken = ODBCWrapper.Utils.GetSafeStr(dtUserGroupRule.DefaultView[0]["change_pin_token"]);
                DateTime changePinTokenLastDate = ODBCWrapper.Utils.GetDateSafeVal(dtUserGroupRule.DefaultView[0]["change_pin_token_last_date"]);

                if (changePinTokenLastDate <= DateTime.UtcNow)
                {
                    result.Initialize(UserGroupRuleResponseStatus.TokenExpired);
                }
                else
                {
                    result.Initialize(siteGuid, ruleID, changePinToken, UserGroupRuleResponseStatus.OK);

                }
            }
            else
            {
                result.Initialize(UserGroupRuleResponseStatus.TokenNotExist);
            }
            return result;
        }

        public override UserGroupRuleResponse ChangeParentalPInCodeByToken(string sSiteGuid, int nUserRuleID, string sChangePinToken, string sCode)
        {
            UserGroupRuleResponse response = new UserGroupRuleResponse();
            try
            {
                int nSiteGuid = 0;
                bool result = int.TryParse(sSiteGuid, out nSiteGuid);
                if (result == true)
                {
                    ApiDAL.Update_UserGroupRule_Code(nSiteGuid, nUserRuleID, sChangePinToken, sCode);
                    ApiDAL.Update_UserGroupRule_Token(nSiteGuid, nUserRuleID, null, DateTime.UtcNow.AddDays(-15));
                    response.SiteGuid = nSiteGuid;
                    response.RuleID = nUserRuleID;
                    response.ResponseStatus = UserGroupRuleResponseStatus.OK;
                    WriteToLog(sSiteGuid, "Change parental pin code successfully to user id:" + sSiteGuid + " parental pin code: " + sCode, "Users module , ChangeParentalPInCodeByToken");

                }
                else
                {
                    response.ResponseStatus = UserGroupRuleResponseStatus.Error;
                }
            }
            catch
            {
                response.ResponseStatus = UserGroupRuleResponseStatus.Error;
            }
            return response;
        }

        protected ForgotPasswordMailRequest GetForgotPasswordMailRequest(string sFirstName, string sEmail, string sToken)
        {
            ForgotPasswordMailRequest retVal = new ForgotPasswordMailRequest();
            retVal.m_sToken = sToken;
            retVal.m_sTemplateName = m_sForgotPasswordMail;
            retVal.m_sSubject = m_sForgotPassMailSubject;
            retVal.m_sSenderTo = sEmail;
            retVal.m_sSenderName = m_sMailFromName;
            retVal.m_sSenderFrom = m_sMailFromAdd;
            retVal.m_sFirstName = sFirstName;
            retVal.m_eMailType = eMailTemplateType.ForgotPassword;
            return retVal;
        }

        protected ChangePasswordMailRequest GetChangePasswordMailRequest(string sFirstName, string sEmail, string sToken)
        {
            ChangePasswordMailRequest retVal = new ChangePasswordMailRequest();
            retVal.m_sToken = sToken;
            retVal.m_sTemplateName = m_sChangePasswordMail;
            retVal.m_sSubject = m_sChangePassMailSubject;
            retVal.m_sSenderTo = sEmail;
            retVal.m_sSenderName = m_sMailFromName;
            retVal.m_sSenderFrom = m_sMailFromAdd;
            retVal.m_sFirstName = sFirstName;
            retVal.m_eMailType = eMailTemplateType.ChangePassword;
            return retVal;
        }

        protected ChangedPinMailRequest GetChangedPinMailRequest(string sUserName, string sEmail, string sSiteGuid, string sToken, string sRuleName)
        {
            ChangedPinMailRequest retVal = new ChangedPinMailRequest();
            retVal.m_sSiteGuid = sSiteGuid;
            retVal.m_sRuleName = sRuleName;
            retVal.m_sToken = sToken;
            retVal.m_sTemplateName = m_sChangedPinMail;
            retVal.m_sSubject = m_sChangedPinMailSubject.Replace("*|subjectPrefix|*", sRuleName);
            retVal.m_sSenderTo = sEmail;
            retVal.m_sSenderName = m_sMailFromName;
            retVal.m_sSenderFrom = m_sMailFromAdd;
            retVal.m_sFirstName = sUserName;
            retVal.m_eMailType = eMailTemplateType.ChangedPin;
            return retVal;
        }

        protected virtual WelcomeMailRequest GetWelcomeMailRequest(string sFirstName, string sUserName, string sPassword, string sEmail, string sFacekookID)
        {
            string sMailData = string.Empty;

            WelcomeMailRequest retVal = new WelcomeMailRequest();
            retVal.m_sTemplateName = m_sWelcomeMailTemplate;
            retVal.m_eMailType = eMailTemplateType.Welcome;
            retVal.m_sFirstName = sFirstName;
            retVal.m_sLastName = string.Empty;
            retVal.m_sSenderFrom = m_sMailFromAdd;
            retVal.m_sSenderName = m_sMailFromName;
            retVal.m_sSenderTo = sEmail;
            retVal.m_sSubject = m_sWelcomeMailSubject;
            retVal.m_sUsername = sUserName;

            retVal.m_sPassword = (string.IsNullOrEmpty(sFacekookID)) ? sPassword : "Facebook Password";
            //TO DO merge it to one call. IRA?????
            retVal.m_sToken = DAL.UsersDal.GetActivationToken(m_nGroupID, sUserName);

            return retVal;
        }

        protected virtual SendPasswordMailRequest GetSendPasswordMailRequest(string sFirstName, string sPassword, string sEmail)
        {
            SendPasswordMailRequest retVal = new SendPasswordMailRequest();
            retVal.m_sTemplateName = string.Format("PasswordReminder_{0}.html", m_nGroupID);
            retVal.m_sSubject = "Uw Ximon wachtwoord";
            retVal.m_sSenderTo = sEmail;
            retVal.m_sSenderName = m_sMailFromName;
            retVal.m_sSenderFrom = m_sMailFromAdd;
            retVal.m_sFirstName = sFirstName;
            retVal.m_sPassword = sPassword;
            retVal.m_eMailType = eMailTemplateType.SendPassword;
            return retVal;
        }

        public override bool SendPasswordMail(string sUN)
        {
            Int32 nUserID = GetUserIDByUserName(sUN);
            User u = new User();
            u.Initialize(nUserID, m_nGroupID, false);

            if (!string.IsNullOrEmpty(u.m_oBasicData.m_sPassword))
            {
                SendPasswordMailRequest sMailReq = GetSendPasswordMailRequest(u.m_oBasicData.m_sFirstName, u.m_oBasicData.m_sPassword, u.m_oBasicData.m_sEmail);

                bool sent = Utils.SendMail(m_nGroupID, sMailReq);
                return sent;
            }

            return false;
        }

        protected List<GroupRule> GetUserGroupsRules(Int32 nGroupID, string sSiteGuid)
        {
            List<GroupRule> groupRules = GetUserDomainGroupRules(nGroupID, sSiteGuid, 0);
            return groupRules;
        }

        private static List<GroupRule> ConvertParentalToGroupRule(List<ParentalRule> parentalRules)
        {
            List<GroupRule> groupRules = new List<GroupRule>();

            foreach (var parentalRule in parentalRules)
            {
                if (parentalRule.mediaTagTypeId > 0)
                {
                    // Convert parental rule into group rule
                    GroupRule groupRule = new GroupRule()
                    {
                        RuleID = (int)parentalRule.id,
                        IsActive = true,
                        Name = parentalRule.name,
                        TagTypeID = parentalRule.mediaTagTypeId,
                        OrderNum = parentalRule.order,
                        GroupRuleType = eGroupRuleType.Parental,
                        AllTagValues = parentalRule.mediaTagValues,
                        BlockAnonymous = parentalRule.blockAnonymousAccess,
                        BlockType = eBlockType.Validation
                    };

                    groupRules.Add(groupRule);
                }

                if (parentalRule.epgTagTypeId > 0)
                {
                    // Convert parental rule into group rule
                    GroupRule groupRule = new GroupRule()
                    {
                        RuleID = (int)parentalRule.id,
                        IsActive = true,
                        Name = parentalRule.name,
                        TagTypeID = parentalRule.epgTagTypeId,
                        OrderNum = parentalRule.order,
                        GroupRuleType = eGroupRuleType.EPG,
                        AllTagValues = parentalRule.epgTagValues,
                        BlockAnonymous = parentalRule.blockAnonymousAccess,
                        BlockType = eBlockType.Validation
                    };

                    groupRules.Add(groupRule);
                }
            }

            return groupRules;
        }

        private static GroupRule CreateSettingsGroupRule(ePurchaeSettingsType type)
        {
            GroupRule settingsRule = new GroupRule()
            {
                RuleID = (int)0,
                IsActive = true,
                Name = "Purchase",
                OrderNum = 0,
                GroupRuleType = eGroupRuleType.Purchase,
                BlockType = eBlockType.Validation
            };

            return settingsRule;
        }

        public static List<GroupRule> GetUserDomainGroupRules(int groupId, string siteGuid, int domainId)
        {
            List<GroupRule> groupRules = new List<GroupRule>();

            if (!string.IsNullOrEmpty(siteGuid))
            {
                // Get parental rule from new DAL method
                var parentalRules = DAL.ApiDAL.Get_User_ParentalRules(groupId, siteGuid);

                groupRules.AddRange(ConvertParentalToGroupRule(parentalRules));

                eRuleLevel ruleLevel = eRuleLevel.User;
                ePurchaeSettingsType type = ePurchaeSettingsType.Block;

                bool hasPurchaseSetting = DAL.ApiDAL.Get_PurchaseSettings(groupId, domainId, siteGuid, out ruleLevel, out type);

                // Create purchase rule if setting is ask or block (block = known backward compatibility issue)
                if (hasPurchaseSetting && (type == ePurchaeSettingsType.Ask || type == ePurchaeSettingsType.Block))
                {
                    GroupRule settingsRule = CreateSettingsGroupRule(type);

                    groupRules.Add(settingsRule);
                }
            }
            else
            {
                // Get parental rule from new DAL method
                var parentalRules = DAL.ApiDAL.Get_Domain_ParentalRules(groupId, domainId);

                groupRules.AddRange(ConvertParentalToGroupRule(parentalRules));

                eRuleLevel ruleLevel = eRuleLevel.User;
                ePurchaeSettingsType type = ePurchaeSettingsType.Block;

                bool hasPurchaseSetting = DAL.ApiDAL.Get_PurchaseSettings(groupId, domainId, "0", out ruleLevel, out type);

                // Create purchase rule if setting is ask or block (block = known backward compatibility issue)
                if (hasPurchaseSetting && (type == ePurchaeSettingsType.Ask || type == ePurchaeSettingsType.Block))
                {
                    GroupRule settingsRule = CreateSettingsGroupRule(type);

                    groupRules.Add(settingsRule);
                }
            }

            return groupRules;
        }


        public override bool WriteToLog(string sSiteGUID, string sMessage, string sWriter)
        {
            try
            {
                ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("users_log");
                insertQuery.SetConnectionKey("USERS_CONNECTION_STRING");
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", m_nGroupID);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("USER_ID", "=", int.Parse(sSiteGUID));
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("MESSAGE", "=", sMessage);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("WRITER", "=", sWriter);
                insertQuery.Execute();
                insertQuery.Finish();
                insertQuery = null;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public override void Initialize()
        {
            if (m_sActivationMail == null)
                m_sActivationMail = "";

            lock (m_sActivationMail)
            {

                // try get mail parameters from cache 
                TvinciUsers tUser = null;
                string key = string.Format("users_TvinciUsersInitialize_{0}", m_nGroupID);
                bool bRes = UsersCache.GetItem<TvinciUsers>(key, out tUser);
                if (bRes)
                {
                    #region Initialize from cache
                    this.m_bIsActivationNeeded = tUser.m_bIsActivationNeeded;
                    this.m_mailImpl = tUser.m_mailImpl;
                    this.m_newsLetterImpl = tUser.m_newsLetterImpl;
                    this.m_nGroupID = tUser.m_nGroupID;
                    this.m_sActivationMail = tUser.m_sActivationMail;
                    this.m_sChangedPinMail = tUser.m_sChangedPinMail;
                    this.m_sChangedPinMailSubject = tUser.m_sChangedPinMailSubject;
                    this.m_sChangePassMailSubject = tUser.m_sChangePassMailSubject;
                    this.m_sChangePasswordMail = tUser.m_sChangePasswordMail;
                    this.m_sForgotPassMailSubject = tUser.m_sForgotPassMailSubject;
                    this.m_sForgotPasswordMail = tUser.m_sForgotPasswordMail;
                    this.m_sMailFromAdd = tUser.m_sMailFromAdd;
                    this.m_sMailFromName = tUser.m_sMailFromName;
                    this.m_sMailPort = tUser.m_sMailPort;
                    this.m_sMailServer = tUser.m_sMailServer;
                    this.m_sMailServerPass = tUser.m_sMailServerPass;
                    this.m_sMailServerUN = tUser.m_sMailServerUN;
                    this.m_sMailSSL = tUser.m_sMailSSL;
                    this.m_sSendPasswordMailSubject = tUser.m_sSendPasswordMailSubject;
                    this.m_sSendPasswordMailTemplate = tUser.m_sSendPasswordMailTemplate;
                    this.m_sWelcomeFacebookMailSubject = tUser.m_sWelcomeFacebookMailSubject;
                    this.m_sWelcomeFacebookMailTemplate = tUser.m_sWelcomeFacebookMailTemplate;
                    this.m_sWelcomeMailSubject = tUser.m_sWelcomeMailSubject;
                    this.m_sWelcomeMailTemplate = tUser.m_sWelcomeMailTemplate;
                    this.m_nActivationMustHours = tUser.m_nActivationMustHours;
                    this.m_nTokenValidityHours = tUser.m_nTokenValidityHours;
                    this.m_nChangePinTokenValidityHours = tUser.m_nChangePinTokenValidityHours;
                    #endregion
                }
                else
                {
                    #region GetValues from DB
                    DataRowView dvMailParameters = DAL.UsersDal.GetGroupMailParameters(m_nGroupID);
                    if (dvMailParameters != null)
                    {
                        // string members
                        m_sWelcomeMailTemplate = ODBCWrapper.Utils.GetSafeStr(dvMailParameters, "WELCOME_MAIL");
                        m_sWelcomeFacebookMailTemplate = ODBCWrapper.Utils.GetSafeStr(dvMailParameters, "WELCOME_FACEBOOK_MAIL");
                        m_sMailFromAdd = ODBCWrapper.Utils.GetSafeStr(dvMailParameters, "MAIL_FROM_ADD");
                        m_sWelcomeMailSubject = ODBCWrapper.Utils.GetSafeStr(dvMailParameters, "WELCOME_MAIL_SUBJECT");
                        m_sWelcomeFacebookMailSubject = ODBCWrapper.Utils.GetSafeStr(dvMailParameters, "WELCOME_FACEBOOK_MAIL_SUBJECT");
                        m_sForgotPasswordMail = ODBCWrapper.Utils.GetSafeStr(dvMailParameters, "FORGOT_PASSWORD_MAIL");
                        m_sChangedPinMail = ODBCWrapper.Utils.GetSafeStr(dvMailParameters, "CHANGED_PIN_MAIL");
                        m_sChangedPinMailSubject = ODBCWrapper.Utils.GetSafeStr(dvMailParameters, "CHANGED_PIN_MAIL_SUBJECT");
                        m_sActivationMail = ODBCWrapper.Utils.GetSafeStr(dvMailParameters, "ACTIVATION_MAIL");
                        m_sMailFromName = ODBCWrapper.Utils.GetSafeStr(dvMailParameters, "MAIL_FROM_NAME");
                        m_sMailServer = ODBCWrapper.Utils.GetSafeStr(dvMailParameters, "MAIL_SERVER");
                        m_sMailServerUN = ODBCWrapper.Utils.GetSafeStr(dvMailParameters, "MAIL_USER_NAME");
                        m_sMailServerPass = ODBCWrapper.Utils.GetSafeStr(dvMailParameters, "MAIL_PASSWORD");
                        m_sForgotPassMailSubject = ODBCWrapper.Utils.GetSafeStr(dvMailParameters, "FORGOT_PASS_MAIL_SUBJECT");
                        m_sSendPasswordMailTemplate = ODBCWrapper.Utils.GetSafeStr(dvMailParameters, "SEND_PASSWORD_MAIL");
                        m_sSendPasswordMailSubject = ODBCWrapper.Utils.GetSafeStr(dvMailParameters, "SEND_PASSWORD_MAIL_SUBJECT");
                        m_sChangePasswordMail = ODBCWrapper.Utils.GetSafeStr(dvMailParameters, "CHANGE_PASSWORD_MAIL");
                        m_sChangePassMailSubject = ODBCWrapper.Utils.GetSafeStr(dvMailParameters, "CHANGE_PASSWORD_MAIL_SUBJECT");
                        //int members
                        Int32 nActivationNeeded = ODBCWrapper.Utils.GetIntSafeVal(dvMailParameters["IS_ACTIVATION_NEEDED"]);
                        m_nActivationMustHours = ODBCWrapper.Utils.GetIntSafeVal(dvMailParameters["ACTIVATION_MUST_HOURS"]);
                        m_nTokenValidityHours = ODBCWrapper.Utils.GetIntSafeVal(dvMailParameters["TOKEN_VALIDITY_HOURS"]);
                        m_nChangePinTokenValidityHours = ODBCWrapper.Utils.GetIntSafeVal(dvMailParameters["CHANGED_PIN_TOKEN_VALIDITY_HOURS"]);
                        m_sMailSSL = ODBCWrapper.Utils.GetIntSafeVal(dvMailParameters, "MAIL_SSL");
                        m_sMailPort = ODBCWrapper.Utils.GetIntSafeVal(dvMailParameters, "MAIL_PORT");
                        //bool member
                        m_bIsActivationNeeded = (nActivationNeeded == 1);
                        //m_newsLetterImpl composition
                        object oNewLetterImplID = dvMailParameters["NewsLetter_Impl_ID"];
                        if (oNewLetterImplID != DBNull.Value && oNewLetterImplID != null && !string.IsNullOrEmpty(oNewLetterImplID.ToString()))
                        {
                            object oNewLetterApiKey = dvMailParameters["NewsLetter_API_Key"];
                            object oNewLetterListID = dvMailParameters["NewsLetter_List_ID"];

                            if (oNewLetterApiKey != DBNull.Value && oNewLetterApiKey != null && oNewLetterListID != DBNull.Value && oNewLetterListID != null)
                            {
                                m_newsLetterImpl = Utils.GetBaseNewsLetterImpl(oNewLetterApiKey.ToString(), oNewLetterListID.ToString(), int.Parse(oNewLetterImplID.ToString()));
                            }
                        }

                        int nMailImplID = ODBCWrapper.Utils.GetIntSafeVal(dvMailParameters, "Mail_Impl_ID");

                        if (nMailImplID > 0)
                        {
                            m_mailImpl = Utils.GetBaseMailImpl(m_nGroupID, 0, nMailImplID);
                        }

                        // add to cache 
                        bRes = UsersCache.AddItem(key, this);
                    }
                    #endregion
                }
            }
        }

        #region Offline user Media Asset
        public override UserOfflineObject[] GetUserOfflineMedia(int nGroupID, string sSiteGuid)
        {
            if (!string.IsNullOrEmpty(sSiteGuid))
            {
                return UserOfflineObject.GetUserOfflineMedia(nGroupID, sSiteGuid);
            }
            else
            {
                return null;
            }
        }

        public override bool AddUserOfflineItems(int nGroupID, string sSiteGuid, string sMediaID)
        {

            if (nGroupID != 0 && !string.IsNullOrEmpty(sSiteGuid) && !string.IsNullOrEmpty(sMediaID))
            {
                return UserOfflineObject.AddUserOfflineItems(nGroupID, sSiteGuid, sMediaID);
            }
            else
            {
                return false;
            }
        }

        public override bool RemoveUserOfflineItems(int nGroupID, string sSiteGuid, string sMediaID)
        {
            if (nGroupID != 0 && !string.IsNullOrEmpty(sSiteGuid) && !string.IsNullOrEmpty(sMediaID))
            {
                return UserOfflineObject.RemoveUserOfflineItems(nGroupID, sSiteGuid, sMediaID);
            }
            else
            {
                return false;
            }
        }

        public override bool ClearUserOfflineItems(int nGroupID, string sSiteGuid)
        {
            if (nGroupID != 0 && !string.IsNullOrEmpty(sSiteGuid))
            {
                return UserOfflineObject.ClearUserOfflineItems(nGroupID, sSiteGuid);
            }
            else
            {
                return false;
            }
        }
        #endregion

        public override Domain AddUserToDomain(int nGroupID, int nDomainID, int nUserID, bool bIsMaster)
        {
            //Create new domain
            Domain domain = new Domain();

            //Check if UserGuid is valid
            if (IsUserIsValid(nGroupID, nUserID, false) == false)
            {
                domain.m_DomainStatus = DomainStatus.Error;
                return domain;
            }

            //Init The Domain
            domain.Initialize(nGroupID, nDomainID);

            //Add new User to Domain
            DomainResponseStatus respStatus = domain.AddUserToDomain(nGroupID, nDomainID, nUserID, bIsMaster);

            return domain;

        }

        private static bool IsUserIsValid(int nGroupID, int nUserID, bool shouldSaveInCache = true)
        {
            //Check if UserID is valid
            User user = new User();

            bool bInit = user.Initialize(nUserID, nGroupID, shouldSaveInCache);

            UserResponseObject resp = new UserResponseObject();

            if (!bInit || (string.IsNullOrEmpty(user.m_oBasicData.m_sUserName)))
            {
                resp.Initialize(ResponseStatus.UserDoesNotExist, user);
            }
            else
            {
                resp.Initialize(ResponseStatus.OK, user);
            }

            return (resp.m_RespStatus == ResponseStatus.OK);
        }

        public override ApiObjects.Response.Status DeleteUser(int userId)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status() { Code = (int)eResponseStatus.Error, Message = eResponseStatus.Error.ToString() };

            try
            {
                UserResponseObject userResponse = GetUserData(userId.ToString());

                if (userResponse.m_RespStatus != ResponseStatus.OK)
                {
                    response = Utils.ConvertResponseStatusToResponseObject(userResponse.m_RespStatus);
                    return response;
                }

                // get User's domain 
                Users.BaseDomain baseDomain = null;
                Utils.GetBaseDomainsImpl(ref baseDomain, m_nGroupID);

                if (baseDomain == null)
                {
                    response.Code = (int)eResponseStatus.DomainNotInitialized;
                    response.Message = HOUSEHOLD_NOT_INITIALIZED;
                    return response;
                }


                Domain userDomain = baseDomain.GetDomainInfo(userResponse.m_user.m_domianID, m_nGroupID);

                if (userDomain == null)
                {
                    response.Code = (int)eResponseStatus.UserNotExistsInDomain;
                    response.Message = USER_NOT_EXISTS_IN_DOMAIN;
                    return response;
                }

                //Delete is not allowed if the user is in the DefaultUsersIDs list.
                if (userDomain.m_DefaultUsersIDs.Contains(userId))
                {
                    response.Code = (int)eResponseStatus.DefaultUserCannotBeDeleted;
                    response.Message = DEFAULT_USER_CANNOT_BE_DELETED;
                    return response;
                }

                //Delete is not allowed if the user is in the master in the domain and there is only 1 master.
                if (userDomain.m_masterGUIDs.Contains(userId) && userDomain.m_masterGUIDs.Count == 1)
                {
                    response.Code = (int)eResponseStatus.ExclusiveMasterUserCannotBeDeleted;
                    response.Message = EXCLUSIVE_MASTER_USER_CANNOT_BE_DELETED;
                    return response;
                }

                //Delete 
                // in case user in domain ( domain id > 0 ) remove user from domain 
                if (userResponse.m_user.m_domianID > 0)
                {
                    DomainResponseObject domainResponse = baseDomain.RemoveUserFromDomain(m_nGroupID, userResponse.m_user.m_domianID, userId);
                    if (domainResponse.m_oDomainResponseStatus != DomainResponseStatus.OK)
                    {
                        response = Utils.ConvertDomainResponseStatusToResponseObject(domainResponse.m_oDomainResponseStatus);
                        return response;
                    }
                }

                // delete user 
                if (UsersDal.DeleteUser(m_nGroupID, userId))
                {
                    response.Code = (int)eResponseStatus.OK;
                    response.Message = eResponseStatus.OK.ToString();

                    // remove user from cache
                    UsersCache usersCache = UsersCache.Instance();
                    usersCache.RemoveUser(userId, m_nGroupID);

                    // add invalidation key for user roles cache
                    string invalidationKey = CachingProvider.LayeredCache.LayeredCacheKeys.GetAddRoleInvalidationKey(userId.ToString());
                    if (!CachingProvider.LayeredCache.LayeredCache.Instance.SetInvalidationKey(invalidationKey))
                    {
                        log.ErrorFormat("Failed to set invalidation key on User.Save key = {0}", invalidationKey);
                    }

                    return response;
                }
            }
            catch (Exception ex)
            {
                response = new ApiObjects.Response.Status((int)eResponseStatus.Error, ex.Message);
                log.Error("DeleteUser - " + string.Format("Failed ex={0}, siteGuid={1}, groupID ={2}, ", ex.Message, userId, m_nGroupID), ex);
            }
            return response;
        }

        public override ApiObjects.Response.Status ChangeUsers(string userId, string userIdToChange, string udid, int groupId)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());

            if (string.IsNullOrEmpty(userId))
            {
                log.ErrorFormat("ChangeUsers: initSiteGuid or siteGuid are empty. userId {0}, userIdToChange = {1}", userId, userIdToChange);
                response = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                return response;
            }

            if (!string.IsNullOrEmpty(userIdToChange) && userId != userIdToChange)
            {
                UserResponseObject initialUserObj = this.GetUserData(userId);
                UserResponseObject newUserObj = this.GetUserData(userIdToChange);

                if (initialUserObj == null || newUserObj == null || initialUserObj.m_RespStatus != ResponseStatus.OK || newUserObj.m_RespStatus != ResponseStatus.OK ||
                    initialUserObj.m_user == null || newUserObj.m_user == null)
                {
                    log.ErrorFormat("ChangeUsers: users not found - {0},{1}", userId, userIdToChange);
                    response = new ApiObjects.Response.Status((int)eResponseStatus.UserDoesNotExist, "Users not found");
                    return response;
                }

                // check if user not activated
                int userIdentifierToChange = 0;
                int.TryParse(userIdToChange, out userIdentifierToChange);
                var userActivatedStatus = IsUserActivated(userIdentifierToChange);
                if (userActivatedStatus == null || userActivatedStatus.Code != (int)eResponseStatus.OK)
                {
                    log.ErrorFormat("ChangeUsers: users not activated  - {0},{1}", userId, userIdToChange);
                    response = new ApiObjects.Response.Status((int)eResponseStatus.UserNotActivated, "Users not activated");
                    return response;
                }

                int initialDomaId = initialUserObj.m_user.m_domianID;
                int newDomainId = newUserObj.m_user.m_domianID;

                // if the domain is not the users domain
                if (initialDomaId == 0 || newDomainId == 0 || initialDomaId != newDomainId)
                {
                    log.ErrorFormat("ChangeUsers: users are not in the same domain. siteGuid = {0}, domainId = {2}, siteGuid = {3}, domainId = {4}", userId, initialDomaId, userIdToChange, newDomainId);
                    response = new ApiObjects.Response.Status((int)eResponseStatus.UserNotExistsInDomain, "Users are not in the same domain");
                    return response;
                }

                // if udid is not in domain
                if (!string.IsNullOrEmpty(udid))
                {
                    Domain domain = new Domain();

                    if (!domain.Initialize(groupId, newDomainId))
                    {
                        log.ErrorFormat("ChangeUsers: error initializing domain = {0}", newDomainId);
                        response = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                        return response;
                    }

                    if (domain.m_deviceFamilies == null || domain.m_deviceFamilies.Count == 0)
                    {
                        log.ErrorFormat("ChangeUsers: udid is not in the domain. udid = {0}, domainId = {1}", udid, domain.m_nDomainID);
                        response = new ApiObjects.Response.Status((int)eResponseStatus.DeviceNotInDomain, "Device is not in domain");
                        return response;
                    }

                    foreach (var family in domain.m_deviceFamilies)
                    {
                        if (family.DeviceInstances.Where(d => d.m_deviceUDID == udid).FirstOrDefault() != null)
                        {
                            Utils.AddInitiateNotificationAction(groupId, eUserMessageAction.ChangeUsers, int.Parse(userIdToChange), udid);
                            return response;
                        }
                    }

                    log.ErrorFormat("ChangeUsers: udid is not in the domain. udid = {0}, domainId = {1}", udid, domain.m_nDomainID);
                    response = new ApiObjects.Response.Status((int)eResponseStatus.DeviceNotInDomain, "Device is not in domain");
                }

                return response;
            }
            else
            {
                log.ErrorFormat("ChangeUsers: User and new user and identical. {0}", userId);
                response = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                return response;
            }
        }

        public override ApiObjects.Response.Status ResendActivationToken(string username)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            UserResponseObject user = GetUserByUsername(username, m_nGroupID);

            if (user == null)
                return response;

            response = Utils.ConvertResponseStatusToResponseObject(user.m_RespStatus);

            if (response.Code != (int)eResponseStatus.OK)
                return response;

            WelcomeMailRequest mailReq = GetWelcomeMailRequest(user.m_user.m_oBasicData.m_sFirstName, user.m_user.m_oBasicData.m_sUserName, user.m_user.m_oBasicData.m_sPassword,
                user.m_user.m_oBasicData.m_sEmail, user.m_user.m_oBasicData.m_sFacebookID);

            if (Utils.SendMail(m_nGroupID, mailReq))
                response = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            else
                response = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            return response;
        }

        public override UserResponse GetUserByExternalID(string externalID, int operatorID)
        {
            UserResponse response = new UserResponse();
            try
            {
                response.user = GetUserByCoGuid(externalID, operatorID);
                if (response.user.m_RespStatus == ResponseStatus.UserDoesNotExist)
                {
                    response.resp = new ApiObjects.Response.Status((int)eResponseStatus.UserDoesNotExist, eResponseStatus.UserDoesNotExist.ToString());
                }
                else
                {
                    response.resp = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
            }
            catch (Exception ex)
            {

                StringBuilder sb = new StringBuilder("Exception at GetUserByExternalID. ");
                sb.Append(String.Concat("externalID: ", externalID));
                sb.Append(String.Concat(" this is: ", this.GetType().Name));
                sb.Append(String.Concat(" Msg: ", ex.Message));
                sb.Append(String.Concat(" Stack trace: ", ex.StackTrace));
                log.Error("Exception - " + sb.ToString(), ex);

                response = new UserResponse()
                {
                    resp = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString())
                };
            }
            return response;
        }

        public override UserResponse GetUserByName(string userName, int groupId)
        {
            UserResponse response = new UserResponse();
            try
            {
                response.user = GetUserByUsername(userName, groupId);
                if (response.user.m_RespStatus == ResponseStatus.UserDoesNotExist)
                {
                    response.resp = new ApiObjects.Response.Status((int)eResponseStatus.UserDoesNotExist, eResponseStatus.UserDoesNotExist.ToString());
                }
                else
                {
                    response.resp = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
            }
            catch (Exception ex)
            {

                StringBuilder sb = new StringBuilder("Exception at GetUserByName. ");
                sb.Append(String.Concat("userName: ", userName));
                sb.Append(String.Concat("groupId: ", groupId));
                sb.Append(String.Concat(" this is: ", this.GetType().Name));
                sb.Append(String.Concat(" Msg: ", ex.Message));
                sb.Append(String.Concat(" Stack trace: ", ex.StackTrace));
                log.Error("Exception - " + sb.ToString(), ex);

                response = new UserResponse()
                {
                    resp = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString())
                };
            }
            return response;
        }
    }
}
