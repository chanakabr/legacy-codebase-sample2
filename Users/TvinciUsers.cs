using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Data;
using DAL;
using System.Diagnostics;
using System.Configuration;
using System.Threading;

namespace Users
{
    public class TvinciUsers : BaseUsers
    {
        private static object lockObj = new object();


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
                u.InitializeByUsername(sUsername, m_nGroupID);
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

            UserActivationState nAS = GetUserActivationStatus(ref sUserName, ref nUserID);

            return (nAS == UserActivationState.Activated);

            //if (!IsActivationNeeded(null))
            //{
            //    return true;
            //}

            //bool bRet = false;

            //int nActivateStatus = 0;
            //DateTime dCreateDate = new DateTime(2000, 1, 1);
            //DateTime dNow = DateTime.Now;

            //List<int> lGroupIDs = UtilsDal.GetAllRelatedGroups(m_nGroupID);
            //string[] arrGroupIDs = lGroupIDs.Select(g => g.ToString()).ToArray();

            //int activStatus = DAL.UsersDal.GetUserActivationState(arrGroupIDs, m_nActivationMustHours, ref sUserName, ref nUserID, ref nActivateStatus);

            //return (activStatus == 0);

            //if (nActivateStatus == 1)
            //{
            //    bRet = true;
            //    return bRet;
            //}

            //// else

            //bRet = !(nActivateStatus == 0 && dCreateDate.AddHours(m_nActivationMustHours) < dNow);

            //return bRet;

            #region OLD
            //ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
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
            //selectQuery += "and group_id " + TVinciShared.PageUtils.GetFullChildGroupsStr(m_nGroupID, "MAIN_CONNECTION_STRING");
            //if (selectQuery.Execute("query", true) != null)
            //{
            //    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            //    if (nCount > 0)
            //    {
            //        nUserID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["id"].ToString());
            //        sUserName = selectQuery.Table("query").DefaultView[0].Row["USERNAME"].ToString();
            //
            //       Int32 nAS = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ACTIVATE_STATUS"].ToString());
            //        DateTime dCreateDate = (DateTime)(selectQuery.Table("query").DefaultView[0].Row["create_date"]);
            //        DateTime dNow = (DateTime)(selectQuery.Table("query").DefaultView[0].Row["dNow"]);
            
            //if (nActivateStatus == 1)
            //{
            //    bRet = true;
            //    return bRet;
            //}
                        
            //// else
               
            //bRet = !(nActivateStatus == 0 && dCreateDate.AddHours(m_nActivationMustHours) < dNow);

            //selectQuery.Finish();
            //selectQuery = null;
            #endregion

        }

        public override UserActivationState GetUserActivationStatus(ref string sUserName, ref Int32 nUserID)
        {
            int nActivateStatus = 0;

            if (!IsActivationNeeded(null))
            {
                return UserActivationState.Activated;
            }

            List<int> lGroupIDs = UtilsDal.GetAllRelatedGroups(m_nGroupID);
            string[] arrGroupIDs = lGroupIDs.Select(g => g.ToString()).ToArray();

            int activStatus = DAL.UsersDal.GetUserActivationState(arrGroupIDs, m_nActivationMustHours, ref sUserName, ref nUserID, ref nActivateStatus);

            return (UserActivationState)activStatus;
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

            //bool bActivated = IsUserActivated(ref sUN, ref nUserID);
            UserActivationState nActivationStatus = GetUserActivationStatus(ref sUN, ref nUserID);
            UserResponseObject o = new UserResponseObject();

            if (nActivationStatus != UserActivationState.Activated)
            {                
                if (nActivationStatus == UserActivationState.UserWIthNoDomain)
                {
                    bool bValidDomainStat = CheckAddDomain(ref o, null, sUN, nUserID);
                    if (!bValidDomainStat)
                        return o;
                }
                else                                
                {                  
                    ResponseStatus ret = ResponseStatus.WrongPasswordOrUserName;

                    if (nUserID <= 0)
                    {
                        ret = ResponseStatus.WrongPasswordOrUserName;
                    }
                    else if (nActivationStatus == UserActivationState.UserDoesNotExist)
                    {
                        ret = ResponseStatus.UserDoesNotExist;
                    }
                    else if (nActivationStatus == UserActivationState.NotActivated)
                    {
                        ret = ResponseStatus.UserNotActivated;
                    }
                    else if (nActivationStatus == UserActivationState.NotActivatedByMaster)
                    {
                        ret = ResponseStatus.UserNotMasterApproved;
                    }
                    else if (nActivationStatus == UserActivationState.UserRemovedFromDomain)
                    {
                        ret = ResponseStatus.UserRemovedFromDomain;
                    }

                    o.m_RespStatus = ret;
                    return o;
                }                
            }            
            
            return User.SignIn(sUN, sPass, 3, 3, nGroupID, sessionID, sIP, deviceID, bPreventDoubleLogins);
        }




        public override DomainResponseObject AddNewDomain(string sUN, int nUserID, int nGroupID)
        {
            Users.BaseDomain t = null;               
            Utils.GetBaseDomainsImpl(ref t, nGroupID);                
            DomainResponseObject dr = t.AddDomain(sUN + "/Domain", sUN + "/Domain", nUserID, nGroupID, "");

            if (dr == null || dr.m_oDomainResponseStatus != DomainResponseStatus.OK)
            {
                // Error adding to domain
                Logger.Logger.Log("Add New Domain Error", "Domain = " + t.ToString(), "Domains");                
            }
            return dr;
        }


        public override UserResponseObject SignIn(int siteGuid, int nMaxFailCount, int nLockMinutes, int nGroupID, string sessionID, string sIP, string deviceID, bool bPreventDoubleLogins)
        {
            string sUN = string.Empty;
            UserResponseObject o = new UserResponseObject();
            UserActivationState nActivationStatus = GetUserActivationStatus(ref sUN, ref siteGuid);

            if (nActivationStatus != UserActivationState.Activated)
            {
                if (nActivationStatus == UserActivationState.UserWIthNoDomain)
                {
                    bool bValidDomainStat = CheckAddDomain(ref o, null, sUN, siteGuid);
                    if (!bValidDomainStat)
                        return o;                   
                }
                else               
                {                   
                    ResponseStatus ret = ResponseStatus.UserNotActivated;

                    if (nActivationStatus == UserActivationState.NotActivatedByMaster)
                    {
                        ret = ResponseStatus.UserNotMasterApproved;
                    }
                    else if (nActivationStatus == UserActivationState.UserDoesNotExist)
                    {
                        ret = ResponseStatus.UserDoesNotExist;
                    }
                    else if (nActivationStatus == UserActivationState.UserRemovedFromDomain)
                    {
                        ret = ResponseStatus.UserRemovedFromDomain;
                    }

                    o.m_RespStatus = ret;
                    return o;
                }                
            }            

            //bool bActivated = IsUserActivated(ref sUN, ref siteGuid);
            //if (bActivated == false)
            //{
            //    UserResponseObject o = new UserResponseObject();
            //    ResponseStatus ret = ResponseStatus.UserNotActivated;
            //    o.m_RespStatus = ret;
            //    return o;
            //}

            return User.SignIn(siteGuid, nMaxFailCount, nLockMinutes, m_nGroupID, sessionID, sIP, deviceID, bPreventDoubleLogins);
        }

        public override UserResponseObject SignInWithToken (string sToken, int nMaxFailCount, int nLockMinutes, int nGroupID, string sessionID, string sIP, string deviceID, bool bPreventDoubleLogins)
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
            return User.GetCurrentUserState(siteGuid);
        }

        public override UserState GetUserInstanceState(int siteGuid, string sessionID, string sIP, string deviceID)
        {
            return User.GetCurrentUserInstanceState(siteGuid, sessionID, sIP, deviceID);
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
            selectQuery += "select id from users where status=1 and is_active=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("CP_TOKEN", "=", sToken);
            selectQuery += " and ";
            //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", m_nGroupID);
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
                u.Initialize(nID, m_nGroupID);
                resp.m_user = u;
                resp.m_RespStatus = ResponseStatus.OK;

                ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("users");
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
                u.Initialize(nID, m_nGroupID);

                //string sInGroupIDs = TVinciShared.PageUtils.GetFullChildGroupsStr(m_nGroupID, "MAIN_CONNECTION_STRING");
                List<int> lGroupIDs = UtilsDal.GetAllRelatedGroups(m_nGroupID);
                string[] arrGroupIDs = lGroupIDs.Select(g => g.ToString()).ToArray();
                bool isActivated = DAL.UsersDal.UpdateUserActivationToken(arrGroupIDs, nID, sToken, Guid.NewGuid().ToString(), (int)UserState.LoggedOut);

                if (isActivated)
                {
                    bool resetSession = DAL.UsersDal.SetUserSessionStatus(nID, 0, 0);
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
                    Logger.Logger.Log("exception", nID.ToString() + " : " + ex.Message, "users_notifier");
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
            bool bInit = newUser.Initialize(nUserID, m_nGroupID) && masterUser.Initialize(nMasterUserID, m_nGroupID);

            if (nUserID <= 0 || nMasterUserID <= 0 || !bInit || !masterUser.m_isDomainMaster)
            {
                resp.m_user = null;
                resp.m_RespStatus = ResponseStatus.UserDoesNotExist;

                return resp;
            }

            // Check if user already activated by master
            string sNewUserName = sUN;
            int nDbUserID = nUserID;
            UserActivationState curActState = GetUserActivationStatus(ref sNewUserName, ref nDbUserID);

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
                Logger.Logger.Log("exception", nUserID.ToString() + " : " + ex.Message, "users_notifier");
            }

            return resp;
        }

        public override UserResponseObject AddNewUser(UserBasicData oBasicData, UserDynamicData sDynamicData, string sPassword)
        {
            UserResponseObject resp = new UserResponseObject();
            User u = new User();
            if (!string.IsNullOrEmpty(oBasicData.m_sUserName) && oBasicData.m_sUserName.ToLower().Contains("anonymous"))
            {
                oBasicData.m_sUserName = string.Format(oBasicData.m_sUserName + "_{0}", User.GetNextGUID());
            }
            u.Initialize(oBasicData, sDynamicData, m_nGroupID, sPassword);
            if (u.m_sSiteGUID != "")
            {
                resp.Initialize(ResponseStatus.UserExists, u);
                return resp;
            }
            else
            {
                if (u.m_oBasicData != oBasicData)
                {
                    resp.Initialize(ResponseStatus.UserExists, u);
                    return resp;
                }
            }
            //the save includes the initialization of  u.m_domianID
             int nUserID = u.Save(m_nGroupID, !IsActivationNeeded(oBasicData));    //u.Save(m_nGroupID);  

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

            //Send Wellcome Email
            if (m_mailImpl != null)
            {
                SendMailImpl(resp.m_user);
            }
            else
            {
                TvinciAPI.WelcomeMailRequest sMailReq = GetWelcomeMailRequest(GetUniqueTitle(oBasicData, sDynamicData), oBasicData.m_sUserName, sPassword, oBasicData.m_sUserName, oBasicData.m_sFacebookID);

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
            using (TvinciAPI.API client = new TvinciAPI.API())
            {
                string sWSURL = Utils.GetWSURL("api_ws");
                if (sWSURL != "")
                    client.Url = sWSURL;
                string sIP = "1.1.1.1";
                string sWSUserName = "";
                string sWSPass = "";
                TVinciShared.WS_Utils.GetWSUNPass(nGroupID, "CreateDefaultRules", "API", sIP, ref sWSUserName, ref sWSPass);
                Logger.Logger.Log("Default Rules", sWSUserName + " " + sWSPass + " " + client.Url, "Default Rules");
                return client.SetDefaultRules(sWSUserName, sWSPass, sSiteGuid);
            }
        }

        public override bool ResendActivationMail(string sUN)
        {
            Int32 nUserID = GetUserIDByUserName(sUN);
            User u = new User();
            u.Initialize(nUserID, m_nGroupID);
            if (u.m_oBasicData.m_sPassword != "")
            {
                TvinciAPI.WelcomeMailRequest sMailReq = GetWelcomeMailRequest(u.m_oBasicData.m_sFirstName, u.m_oBasicData.m_sUserName, u.m_oBasicData.m_sPassword, u.m_oBasicData.m_sEmail, u.m_oBasicData.m_sFacebookID);
                bool sendingMailResult = Utils.SendMail(m_nGroupID, sMailReq);
                return true;
            }
            return false;
        }

        public override bool ResendWelcomeMail(string sUN)
        {
            Int32 nUserID = GetUserIDByUserName(sUN);
            User u = new User();
            u.Initialize(nUserID, m_nGroupID);

            if (u.m_oBasicData.m_sPassword != "")
            {
                if (m_mailImpl != null)
                {
                    return SendMailImpl(u);
                }
                else
                {
                    TvinciAPI.WelcomeMailRequest sMailReq = GetWelcomeMailRequest(u.m_oBasicData.m_sFirstName, u.m_oBasicData.m_sUserName, u.m_oBasicData.m_sPassword, u.m_oBasicData.m_sEmail, u.m_oBasicData.m_sFacebookID);
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

        public override UserResponseObject GetUserData(string sSiteGUID)
        {
            try
            {
                Int32 nUserID = int.Parse(sSiteGUID);
                User u = new User();

                u.Initialize(nUserID, m_nGroupID);

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

        public override List<UserResponseObject> GetUsersData(string[] sSiteGUIDs)
        {
            try
            {
                // nInt32UserID = int.Parse(sSiteGUIDs);
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
                DataTable dtFields = UsersDal.GetGroupUsersSearchFields(m_nGroupID);

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

                        if (CachingManager.CachingManager.Exist("GroupUsers" + m_nGroupID.ToString()) == true)
                        {
                            dtGroupUsers = (DataTable)CachingManager.CachingManager.GetCachedData("GroupUsers" + m_nGroupID.ToString());

                            int cache_period = 10;

                            if (TVinciShared.WS_Utils.GetTcmConfigValue("SEARCH_USERS_CACHE_PERIOD") != string.Empty )
                                int.TryParse(TVinciShared.WS_Utils.GetTcmConfigValue("SEARCH_USERS_CACHE_PERIOD"), out cache_period);

                            DateTime timeStamp = (DateTime)CachingManager.CachingManager.GetCachedData("GroupUsersTimeStamp" + m_nGroupID.ToString());

                            if ((DateTime.UtcNow - timeStamp).TotalMinutes >= cache_period)
                            {
                                Thread thread = new Thread(() =>
                                {
                                    lock (lockObj)
                                    {
                                        timeStamp = (DateTime)CachingManager.CachingManager.GetCachedData("GroupUsersTimeStamp" + m_nGroupID.ToString());

                                        if ((DateTime.UtcNow - timeStamp).TotalMinutes >= cache_period)
                                        {
                                            dtGroupUsers = UsersDal.GetGroupUsers(m_nGroupID, sGroupUsersSearchFields);

                                            if (dtGroupUsers != null)
                                            {
                                                CachingManager.CachingManager.SetCachedData("GroupUsers" + m_nGroupID.ToString(), dtGroupUsers, 10800, System.Web.Caching.CacheItemPriority.Normal, 0, false);
                                                CachingManager.CachingManager.SetCachedData("GroupUsersTimeStamp" + m_nGroupID.ToString(), DateTime.UtcNow, 10800, System.Web.Caching.CacheItemPriority.Normal, 0, false);
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
                                CachingManager.CachingManager.SetCachedData("GroupUsers" + m_nGroupID.ToString(), dtGroupUsers, 10800, System.Web.Caching.CacheItemPriority.Normal, 0, false);
                                CachingManager.CachingManager.SetCachedData("GroupUsersTimeStamp" + m_nGroupID.ToString(), DateTime.UtcNow, 10800, System.Web.Caching.CacheItemPriority.Normal, 0, false);
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
                Logger.Logger.Log("SearchUsers error", string.Format("Terms: {0}, Fields: {1}, IsExact: {2}, Error: {3}", string.Join(";", sTerms), string.Join(";", sFields), bIsExact, ex.Message), "SearchUsers");

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
                u.Initialize(nUserID, m_nGroupID);
                if (u.m_oBasicData.m_sUserName != "")
                {
                    UserBasicData b = new UserBasicData();
                    b.Initialize(sBasicDataXML);
                    UserDynamicData d = new UserDynamicData();
                    d.Initialize(sDynamicDataXML);
                    u.Update(b, d, m_nGroupID);
                    resp.Initialize(ResponseStatus.OK, u);
                }
                else
                    resp.Initialize(ResponseStatus.UserDoesNotExist, null);
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
                //bool init = u.Initialize(nUserID, m_nGroupID);
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

                        //if (isNewNewsLetter && !isOldNewsLetter)
                        //{
                        //    isSubscribeNewsLetter = true;
                        //}
                        //else
                        //{
                        //    if (isOldNewsLetter && !isNewNewsLetter)
                        //    {
                        //        isUnSubscribeNewsLeter = true;
                        //    }
                        //}

                        if (isNewNewsLetter && oBasicData.m_sEmail != u.m_oBasicData.m_sEmail)
                        {
                            m_newsLetterImpl.UnSubscribe(u);
                            isSubscribeNewsLetter = true;
                        }

                    }
                }

                int saveID = u.Update(oBasicData, sDynamicData, m_nGroupID);
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

                resp.Initialize(ResponseStatus.OK, u);
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
            if (uro.m_user == null)
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

            uro.m_user.Save(m_nGroupID);
            ret.m_user = uro.m_user;
            ret.m_RespStatus = ResponseStatus.OK;
            return ret;
        }

        public override UserResponseObject RenewPassword(string sUN, string sPass, int nGroupID)
        {
            UserResponseObject ret = new UserResponseObject();
            Int32 nID = GetUserIDByUserName(sUN);
            User u = new User();
            u.Initialize(nID, m_nGroupID);
            if (u.m_oBasicData.m_sPassword == "")
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

            u.Save(m_nGroupID);
            ret.m_user = u;
            ret.m_RespStatus = ResponseStatus.OK;
            return ret;
        }

        private bool UserGenerateToken(string sUN, ref string sEmail, ref Int32 nID, ref string sFirstName, ref string sToken)
        {
            DataTable dt = DAL.UsersDal.GenerateToken(sUN, m_nGroupID, m_nTokenValidityHours);

            if (dt != null && dt.Rows != null != dt.Rows.Count > 0)
            {
                DataRow dr = dt.Rows[0];

                sEmail          = ODBCWrapper.Utils.GetSafeStr(dr["EMAIL_ADD"]);
                nID             = ODBCWrapper.Utils.GetIntSafeVal(dr["ID"]);
                sFirstName      = ODBCWrapper.Utils.GetSafeStr(dr["FIRST_NAME"]);
                sToken          = ODBCWrapper.Utils.GetSafeStr(dr["TOKEN"]);
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

            string sEmail       = string.Empty;
            Int32  nID          = 0;
            string sFirstName   = string.Empty;
            string sToken       = string.Empty;

            if (UserGenerateToken(sUN, ref sEmail, ref nID, ref sFirstName, ref sToken) == true)
            {
                //Send ForgotPasswordMail
                TvinciAPI.ForgotPasswordMailRequest sMailRequest = GetForgotPasswordMailRequest(sFirstName, sEmail, sToken);

                Logger.Logger.Log("Forgot Pass", "Start send to " + sEmail + " from" + m_sForgotPasswordMail, "ForgotPass");

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

            string sEmail       = string.Empty;
            Int32 nID           = 0;
            string sFirstName   = string.Empty;
            string sToken       = string.Empty;

            if (UserGenerateToken(sUN, ref sEmail, ref nID, ref sFirstName, ref sToken) == true)
            {
                //Send ChangePassword
                TvinciAPI.ChangePasswordMailRequest sMailRequest = GetChangePasswordMailRequest(sFirstName, sEmail, sToken);

                Logger.Logger.Log("Change Password", "Start send to " + sEmail + " from" + m_sForgotPasswordMail, "ChangePassword");

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
                selectQuery += "select username, email_add from users(nolock) where is_active=1 and status=1 and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nSiteGuid);

                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        List<TvinciAPI.GroupRule> groupRulesList = GetUserGroupsRules(sSiteGuid);
                        TvinciAPI.GroupRule groupRule = groupRulesList.Find(
                                                        delegate(TvinciAPI.GroupRule rule)
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
                        TvinciAPI.ChangedPinMailRequest sMailRequest = GetChangedPinMailRequest(sUn, sEmail, nSiteGuid.ToString(), changePinToken, ruleName);
                        Logger.Logger.Log("Change pin code", "Start send to " + sEmail + " from" + m_sChangedPinMail, "ChangePin");
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
                    try
                    {
                        WriteToLog(sSiteGuid, "Change parental pin code successfully to user id:" + sSiteGuid + " parental pin code: " + sCode, "Users module , ChangeParentalPInCodeByToken");
                    }
                    catch
                    { }
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


        protected TvinciAPI.ForgotPasswordMailRequest GetForgotPasswordMailRequest(string sFirstName, string sEmail, string sToken)
        {
            TvinciAPI.ForgotPasswordMailRequest retVal = new TvinciAPI.ForgotPasswordMailRequest();
            retVal.m_sToken = sToken;
            retVal.m_sTemplateName = m_sForgotPasswordMail;
            retVal.m_sSubject = m_sForgotPassMailSubject;
            retVal.m_sSenderTo = sEmail;
            retVal.m_sSenderName = m_sMailFromName;
            retVal.m_sSenderFrom = m_sMailFromAdd;
            retVal.m_sFirstName = sFirstName;
            retVal.m_eMailType = TvinciAPI.eMailTemplateType.ForgotPassword;
            return retVal;
        }

        protected TvinciAPI.ChangePasswordMailRequest GetChangePasswordMailRequest(string sFirstName, string sEmail, string sToken)
        {
            TvinciAPI.ChangePasswordMailRequest retVal = new TvinciAPI.ChangePasswordMailRequest();
            retVal.m_sToken = sToken;
            retVal.m_sTemplateName = m_sChangePasswordMail;
            retVal.m_sSubject = m_sChangePassMailSubject;
            retVal.m_sSenderTo = sEmail;
            retVal.m_sSenderName = m_sMailFromName;
            retVal.m_sSenderFrom = m_sMailFromAdd;
            retVal.m_sFirstName = sFirstName;
            retVal.m_eMailType = TvinciAPI.eMailTemplateType.ChangePassword;
            return retVal;
        }

        protected TvinciAPI.ChangedPinMailRequest GetChangedPinMailRequest(string sUserName, string sEmail, string sSiteGuid, string sToken, string sRuleName)
        {
            TvinciAPI.ChangedPinMailRequest retVal = new TvinciAPI.ChangedPinMailRequest();
            retVal.m_sSiteGuid = sSiteGuid;
            retVal.m_sRuleName = sRuleName;
            retVal.m_sToken = sToken;
            retVal.m_sTemplateName = m_sChangedPinMail;
            retVal.m_sSubject = m_sChangedPinMailSubject.Replace("*|subjectPrefix|*", sRuleName);
            retVal.m_sSenderTo = sEmail;
            retVal.m_sSenderName = m_sMailFromName;
            retVal.m_sSenderFrom = m_sMailFromAdd;
            retVal.m_sFirstName = sUserName;
            retVal.m_eMailType = TvinciAPI.eMailTemplateType.ChangedPin;
            return retVal;
        }


        protected virtual TvinciAPI.WelcomeMailRequest GetWelcomeMailRequest(string sFirstName, string sUserName, string sPassword, string sEmail, string sFacekookID)
        {
            string sMailData = string.Empty;
            string sActivation = string.Empty;

            TvinciAPI.WelcomeMailRequest retVal = new TvinciAPI.WelcomeMailRequest();
            retVal.m_sTemplateName = m_sWelcomeMailTemplate;
            retVal.m_eMailType = TvinciAPI.eMailTemplateType.Welcome;
            retVal.m_sFirstName = sFirstName;
            retVal.m_sLastName = string.Empty;
            retVal.m_sSenderFrom = m_sMailFromAdd;
            retVal.m_sSenderName = m_sMailFromName;
            retVal.m_sSenderTo = sEmail;
            retVal.m_sSubject = m_sWelcomeMailSubject;
            retVal.m_sUsername = sUserName;

            retVal.m_sPassword = (string.IsNullOrEmpty(sFacekookID)) ? sPassword : "Facebook Password";

            sActivation = DAL.UsersDal.GetActivationToken(m_nGroupID, sUserName);
            retVal.m_sToken = DAL.UsersDal.GetActivationToken(m_nGroupID, sUserName);

            //retVal.m_sToken = sActivation;

            return retVal;
        }

        protected virtual TvinciAPI.SendPasswordMailRequest GetSendPasswordMailRequest(string sFirstName, string sPassword, string sEmail)
        {
            TvinciAPI.SendPasswordMailRequest retVal = new TvinciAPI.SendPasswordMailRequest();
            retVal.m_sTemplateName = string.Format("PasswordReminder_{0}.html", m_nGroupID); // m_sSendPasswordMail;
            retVal.m_sSubject = "Uw Ximon wachtwoord";  //m_sSendPasswordMailSubject;
            retVal.m_sSenderTo = sEmail;
            retVal.m_sSenderName = m_sMailFromName;
            retVal.m_sSenderFrom = m_sMailFromAdd;
            retVal.m_sFirstName = sFirstName;
            retVal.m_sPassword = sPassword;
            retVal.m_eMailType = TvinciAPI.eMailTemplateType.SendPassword;
            return retVal;
        }

        public override bool SendPasswordMail(string sUN)
        {
            Int32 nUserID = GetUserIDByUserName(sUN);
            User u = new User();
            u.Initialize(nUserID, m_nGroupID);

            if (!string.IsNullOrEmpty(u.m_oBasicData.m_sPassword))
            {
                TvinciAPI.SendPasswordMailRequest sMailReq = GetSendPasswordMailRequest(u.m_oBasicData.m_sFirstName, u.m_oBasicData.m_sPassword, u.m_oBasicData.m_sEmail);

                bool sent = Utils.SendMail(m_nGroupID, sMailReq);
                return sent;
            }

            return false;
        }


        //protected bool SendMail(TvinciAPI.MailRequestObj request)
        //{
        //    TvinciAPI.API client = new TvinciAPI.API();
        //    string sWSURL = Utils.GetWSURL("api_ws");
        //    if (sWSURL != "")
        //        client.Url = sWSURL;
        //    string sIP = "1.1.1.1";
        //    string sWSUserName = "";
        //    string sWSPass = "";
        //    TVinciShared.WS_Utils.GetWSUNPass(m_nGroupID , "Mailer", "API", sIP, ref sWSUserName, ref sWSPass);
        //    bool result = client.SendMailTemplate(sWSUserName, sWSPass, request);
        //    return result;
        //}

        protected List<TvinciAPI.GroupRule> GetUserGroupsRules(string sSiteGuid)
        {
            TvinciAPI.API client = new TvinciAPI.API();
            string sWSURL = Utils.GetWSURL("api_ws");
            if (sWSURL != "")
                client.Url = sWSURL;
            string sIP = "1.1.1.1";
            string sWSUserName = "";
            string sWSPass = "";
            TVinciShared.WS_Utils.GetWSUNPass(m_nGroupID, "GetUserGroupsRules", "API", sIP, ref sWSUserName, ref sWSPass);
            TvinciAPI.GroupRule[] groupRules = client.GetUserGroupRules(sWSUserName, sWSPass, sSiteGuid);
            return groupRules.ToList();
        }

        public override bool WriteToLog(string sSiteGUID, string sMessage, string sWriter)
        {
            try
            {
                ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("users_log");
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
            //if (m_bIsInitialized == true)
            //return;
            if (m_sActivationMail == null)
                m_sActivationMail = "";

            lock (m_sActivationMail)
            {
                //if (m_bIsInitialized == true)
                //return;

                DataRowView dvMailParameters = DAL.UsersDal.GetGroupMailParameters(m_nGroupID);

                if (dvMailParameters != null)
                {

                    //ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                    //selectQuery += "select * from groups_parameters where status=1 and is_active=1 and ";
                    //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", m_nGroupID);
                    ////selectQuery += " group_id " + TVinciShared.PageUtils.GetFullChildGroupsStr(m_nGroupID, "MAIN_CONNECTION_STRING");
                    //selectQuery += " order by id desc";
                    //if (selectQuery.Execute("query", true) != null)
                    //{
                    //    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    //    if (nCount > 0)
                    //    {

                    object oWelcomeMail = dvMailParameters["WELCOME_MAIL"];             // selectQuery.Table("query").DefaultView[0].Row["WELCOME_MAIL"];
                    object oWelcomeFacebookMail = dvMailParameters["WELCOME_FACEBOOK_MAIL"];    // selectQuery.Table("query").DefaultView[0].Row["WELCOME_FACEBOOK_MAIL"];
                    object oForgotPassword = dvMailParameters["FORGOT_PASSWORD_MAIL"];     // selectQuery.Table("query").DefaultView[0].Row["FORGOT_PASSWORD_MAIL"];
                    object oChangedPinMail = dvMailParameters["CHANGED_PIN_MAIL"];         //selectQuery.Table("query").DefaultView[0].Row["CHANGED_PIN_MAIL"];
                    object oActivation = dvMailParameters["ACTIVATION_MAIL"];          //selectQuery.Table("query").DefaultView[0].Row["ACTIVATION_MAIL"];
                    object oMailFromName = dvMailParameters["MAIL_FROM_NAME"];           //selectQuery.Table("query").DefaultView[0].Row["MAIL_FROM_NAME"];
                    object oMailServer = dvMailParameters["MAIL_SERVER"];              //selectQuery.Table("query").DefaultView[0].Row["MAIL_SERVER"];
                    object oMailServerUN = dvMailParameters["MAIL_USER_NAME"];           //selectQuery.Table("query").DefaultView[0].Row["MAIL_USER_NAME"];
                    object oMailServerPass = dvMailParameters["MAIL_PASSWORD"];            //selectQuery.Table("query").DefaultView[0].Row["MAIL_PASSWORD"];
                    object oMailFromAdd = dvMailParameters["MAIL_FROM_ADD"];            //selectQuery.Table("query").DefaultView[0].Row["MAIL_FROM_ADD"];
                    object oWelcomMailSubject = dvMailParameters["WELCOME_MAIL_SUBJECT"];     //selectQuery.Table("query").DefaultView[0].Row["WELCOME_MAIL_SUBJECT"];
                    object oWelcomeFacebookMailSubject = dvMailParameters["WELCOME_FACEBOOK_MAIL_SUBJECT"];     //selectQuery.Table("query").DefaultView[0].Row["WELCOME_MAIL_SUBJECT"];
                    object oForgotPassMailSubject = dvMailParameters["FORGOT_PASS_MAIL_SUBJECT"]; //selectQuery.Table("query").DefaultView[0].Row["FORGOT_PASS_MAIL_SUBJECT"];
                    object oChangedPinMailSubject = dvMailParameters["CHANGED_PIN_MAIL_SUBJECT"]; //selectQuery.Table("query").DefaultView[0].Row["CHANGED_PIN_MAIL_SUBJECT"];
                    object oNewLetterImplID = dvMailParameters["NewsLetter_Impl_ID"];       //selectQuery.Table("query").DefaultView[0].Row["NewsLetter_Impl_ID"];

                    object oSendPasswordMail = dvMailParameters["SEND_PASSWORD_MAIL"];           // selectQuery.Table("query").DefaultView[0].Row["WELCOME_MAIL"];
                    object oSendPasswordMailSubject = dvMailParameters["SEND_PASSWORD_MAIL_SUBJECT"];   //selectQuery.Table("query").DefaultView[0].Row["WELCOME_MAIL_SUBJECT"];

                    Int32 nActivationNeeded = ODBCWrapper.Utils.GetIntSafeVal(dvMailParameters["IS_ACTIVATION_NEEDED"]);  //selectQuery.Table("query").DefaultView[0].Row["IS_ACTIVATION_NEEDED"]);
                    m_nActivationMustHours = ODBCWrapper.Utils.GetIntSafeVal(dvMailParameters["ACTIVATION_MUST_HOURS"]);  //selectQuery.Table("query").DefaultView[0].Row["ACTIVATION_MUST_HOURS"]);
                    m_nTokenValidityHours = ODBCWrapper.Utils.GetIntSafeVal(dvMailParameters["TOKEN_VALIDITY_HOURS"]);  //selectQuery.Table("query").DefaultView[0].Row["TOKEN_VALIDITY_HOURS"]);
                    m_nChangePinTokenValidityHours = ODBCWrapper.Utils.GetIntSafeVal(dvMailParameters["CHANGED_PIN_TOKEN_VALIDITY_HOURS"]);  //selectQuery.Table("query").DefaultView[0].Row["CHANGED_PIN_TOKEN_VALIDITY_HOURS"]);

                    object oMailSSL = dvMailParameters["MAIL_SSL"];     //selectQuery.Table("query").DefaultView[0].Row["MAIL_SSL"];
                    object oMailPort = dvMailParameters["MAIL_PORT"];    //selectQuery.Table("query").DefaultView[0].Row["MAIL_PORT"];

                    if (oNewLetterImplID != DBNull.Value && oNewLetterImplID != null && !string.IsNullOrEmpty(oNewLetterImplID.ToString()))
                    {
                        string apiKey = string.Empty;
                        string listID = string.Empty;
                        object oNewLetterApiKey = dvMailParameters["NewsLetter_API_Key"]; //selectQuery.Table("query").DefaultView[0].Row["NewsLetter_API_Key"];
                        object oNewLetterListID = dvMailParameters["NewsLetter_List_ID"]; //selectQuery.Table("query").DefaultView[0].Row["NewsLetter_List_ID"];

                        if (oNewLetterApiKey != DBNull.Value && oNewLetterApiKey != null && oNewLetterListID != DBNull.Value && oNewLetterListID != null)
                        {
                            m_newsLetterImpl = Utils.GetBaseNewsLetterImpl(oNewLetterApiKey.ToString(), oNewLetterListID.ToString(), int.Parse(oNewLetterImplID.ToString()));
                        }
                    }

                    m_bIsActivationNeeded = (nActivationNeeded == 1);

                    /***********************************/
                    object oChangePasswordMail = dvMailParameters["CHANGE_PASSWORD_MAIL"];        
                    object oChangePasswordMailSubject = dvMailParameters["CHANGE_PASSWORD_MAIL_SUBJECT"];   

                    if (oChangePasswordMail != null && oChangePasswordMail != DBNull.Value)
                        m_sChangePasswordMail = oChangePasswordMail.ToString();
                    if (oChangePasswordMailSubject != null && oChangePasswordMailSubject != DBNull.Value)
                        m_sChangePassMailSubject = oChangePasswordMailSubject.ToString();
                    /***********************************/


                    if (oWelcomeMail != null && oWelcomeMail != DBNull.Value)
                        m_sWelcomeMailTemplate = oWelcomeMail.ToString();
                    if (oWelcomeFacebookMail != null && oWelcomeFacebookMail != DBNull.Value)
                        m_sWelcomeFacebookMailTemplate = oWelcomeFacebookMail.ToString();
                    if (oWelcomMailSubject != null && oWelcomMailSubject != DBNull.Value)
                        m_sWelcomeMailSubject = oWelcomMailSubject.ToString();
                    if (oWelcomeFacebookMailSubject != null && oWelcomeFacebookMailSubject != DBNull.Value)
                        m_sWelcomeFacebookMailSubject = oWelcomeFacebookMailSubject.ToString();
                    if (oForgotPassword != null && oForgotPassword != DBNull.Value)
                        m_sForgotPasswordMail = oForgotPassword.ToString();
                    if (oForgotPassMailSubject != null && oForgotPassMailSubject != DBNull.Value)
                        m_sForgotPassMailSubject = oForgotPassMailSubject.ToString();
                    if (oChangedPinMail != null && oChangedPinMail != DBNull.Value)
                        m_sChangedPinMail = oChangedPinMail.ToString();
                    if (oChangedPinMailSubject != null && oChangedPinMailSubject != DBNull.Value)
                        m_sChangedPinMailSubject = oChangedPinMailSubject.ToString();
                    if (oActivation != null && oActivation != DBNull.Value)
                        m_sActivationMail = oActivation.ToString();
                    if (oMailFromName != null && oMailFromName != DBNull.Value)
                        m_sMailFromName = oMailFromName.ToString();
                    if (oMailFromAdd != null && oMailFromAdd != DBNull.Value)
                        m_sMailFromAdd = oMailFromAdd.ToString();
                    if (oMailServer != null && oMailServer != DBNull.Value)
                        m_sMailServer = oMailServer.ToString();
                    if (oMailServerUN != null && oMailServerUN != DBNull.Value)
                        m_sMailServerUN = oMailServerUN.ToString();
                    if (oMailServerPass != null && oMailServerPass != DBNull.Value)
                        m_sMailServerPass = oMailServerPass.ToString();
                    if (oMailSSL != null && oMailSSL != DBNull.Value)
                        m_sMailSSL = int.Parse(oMailSSL.ToString());
                    if (oMailPort != null && oMailPort != DBNull.Value)
                        m_sMailPort = int.Parse(oMailPort.ToString());

                    if (oSendPasswordMail != null && oSendPasswordMail != DBNull.Value)
                        m_sSendPasswordMailTemplate = oSendPasswordMail.ToString().Trim();
                    if (oSendPasswordMailSubject != null && oSendPasswordMailSubject != DBNull.Value)
                        m_sSendPasswordMailSubject = oSendPasswordMailSubject.ToString().Trim();

                    object oMailImplID = dvMailParameters["Mail_Impl_ID"];  //ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "Mail_Impl_ID", 0);
                    if (oMailImplID != null && oMailImplID != DBNull.Value)
                    {
                        int nMailImplID = int.Parse(oMailImplID.ToString());

                        if (nMailImplID > 0)
                        {
                            m_mailImpl = Utils.GetBaseMailImpl(m_nGroupID, 0, nMailImplID);
                        }
                    }
                }

                //selectQuery.Finish();
                //selectQuery = null;
                //m_bIsInitialized = true;
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
        //public override UserOfflineObject[] GetUserOfflineItemsByFileType(int nGroupID, string sSiteGuid, string sFileType)
        //{

        //    if (!string.IsNullOrEmpty(sFileType) && !string.IsNullOrEmpty(sSiteGuid))
        //    {
        //        return UserOfflineObject.GetUserOfflineItemsByFileType(nGroupID, sSiteGuid, sFileType);
        //    }
        //    else
        //    {
        //        return null;
        //    }
        //}
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

        //public override Domain AddDomain(string domainName, string domainDescription, Int32 masterUserGuid, Int32 nGroupID)
        //{

        //    //Create new domain
        //    Domain domain = new Domain();

        //    //Check if UserGuid is valid 
        //    if (isUserIsValid(nGroupID, masterUserGuid) == false)
        //    {
        //        domain.m_DomainStatus = DomainStatus.Error;
        //        return domain;
        //    }

        //    //Init new Domain Object with Params
        //    domain.Initialize(domainName, domainDescription, nGroupID, 0, masterUserGuid);

        //    return domain;

        //}

        //public override Domain SetDomainInfo(Int32 domainID, string domainName, Int32 nGroupID, string domainDescription)
        //{
        //    //New domain
        //    Domain domain = new Domain();

        //    //Init the domain according to domainId
        //    domain.Initialize(domainName, domainDescription, nGroupID, domainID);

        //    //Update the domain fields
        //    domain.Update();

        //    return domain;
        //}

        public override Domain AddUserToDomain(int nGroupID, int nDomainID, int nUserID, bool bIsMaster)
        {
            //Create new domain
            Domain domain = new Domain();

            //Check if UserGuid is valid
            if (IsUserIsValid(nGroupID, nUserID) == false)
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

        //public override Domain RemoveUserFromDomain(Int32 nGroupID, Int32 domainID, Int32 userGUID)
        //{
        //    //Create new domain
        //    Domain domain = new Domain();

        //    //Init the Domain
        //    domain.Initialize(nGroupID, domainID);

        //    //Delete the User from Domain
        //    domain.RemoveUserFromDomain(nGroupID, domainID, userGUID);

        //    return domain;
        //}

        //public override Domain GetDomainInfo(Int32 domainID, Int32 nGroupID)
        //{
        //    //Create & Init Domain 
        //    Domain domain = new Domain();
        //    domain.Initialize(nGroupID, domainID);

        //    return domain;
        //}

        private static bool IsUserIsValid(int nGroupID, int nUserID)
        {
            //Check if UserID is valid
            User user = new User();

            bool bInit = user.Initialize(nUserID, nGroupID);

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
    }
}
