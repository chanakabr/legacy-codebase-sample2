using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Users
{
    public class EutelsatUsers : TvinciUsers
    {
        //private const int USER_COGUID_LENGTH = 15;


        public EutelsatUsers(int nGroupID)
            : base(nGroupID)
        {

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
            //{
            //    resp.Initialize(ResponseStatus.ErrorOnInitUser, u);
            //    return resp;
            //}

            if (!string.IsNullOrEmpty(u.m_sSiteGUID))
            {
                resp.Initialize(ResponseStatus.UserExists, u);
                return resp;
            }

            if (u.m_oBasicData != oBasicData)
            {
                resp.Initialize(ResponseStatus.UserExists, u);
                return resp;
            }
            
            int newUserID = u.Save(m_nGroupID, !IsActivationNeeded(oBasicData));
            
            if (newUserID <= 0)
            {
                resp.Initialize(ResponseStatus.ErrorOnSaveUser, u);
                return resp;
            }

            if (u.m_domianID <= 0)
            {
                bool bValidDomainStatus = base.CheckAddDomain(ref resp, u, oBasicData.m_sUserName, newUserID);
            }
            else
            {
                resp.Initialize(ResponseStatus.OK, u);
            }          
            
            //CreateDefaultRules(u.m_sSiteGUID, m_nGroupID);           

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
                TvinciAPI.WelcomeMailRequest sMailReq = GetWelcomeMailRequest(GetUniqueTitle(oBasicData, sDynamicData), oBasicData.m_sUserName, sPassword, oBasicData.m_sEmail, oBasicData.m_sFacebookID);

                bool sendingMailResult = Utils.SendMail(m_nGroupID, sMailReq);
            }

            //UserResponseObject resp = base.AddNewUser(oBasicData, sDynamicData, sPassword);

            if (resp.m_RespStatus != ResponseStatus.OK)
            {
                return resp;
            }

            string sOperatorCoGuid = string.Empty;
            int nOperatorID = 0;
            int nOperatorGroupID = 0;
            int nHouseholdID = 0;

            if (Utils.GetUserOperatorAndHouseholdIDs(m_nGroupID, resp.m_user.m_oBasicData.m_CoGuid, ref nOperatorID, ref sOperatorCoGuid, ref nOperatorGroupID, ref nHouseholdID))
            {
                bool res = DAL.UsersDal.InsertUserOperator(resp.m_user.m_sSiteGUID, resp.m_user.m_oBasicData.m_CoGuid, nOperatorID);
            }

            return resp;
        }

        public override UserResponseObject ActivateAccount(string sUN, string sToken)
        {
            int nUserID = GetUserIDByUserName(sUN);

            User u = new User();
            bool bInit = u.Initialize(nUserID, m_nGroupID);

            UserResponseObject resp = new UserResponseObject();
            if (nUserID <= 0 || !bInit)
            {
                resp.m_user = null;
                resp.m_RespStatus = ResponseStatus.UserDoesNotExist;

                return resp;
            }


            bool bNotify = false;
            //bool bIsMaster = u.m_isDomainMaster;
            //int nOperatorID = 0;
            
            //int nDomainID = DAL.UsersDal.GetUserDomainID(nUserID.ToString(), ref nOperatorID, ref bIsMaster);
            //if (nDomainID <= 0)
            //{
            //    nDomainID = DAL.UsersDal.GetUserDomainID(nUserID.ToString(), ref nOperatorID, ref bIsMaster);
            //}

            int tokenUserID = GetUserIDByActivationToken(sToken);
            
            // Username does not correspond to token
            if (tokenUserID > 0 && nUserID != tokenUserID)
            {
                resp.m_user = null;
                resp.m_RespStatus = ResponseStatus.WrongPasswordOrUserName;

                return resp;
            }

            if (tokenUserID > 0 && nUserID == tokenUserID)  //(u.m_isDomainMaster)
            {          
                //string sInGroupIDs = TVinciShared.PageUtils.GetFullChildGroupsStr(m_nGroupID, "MAIN_CONNECTION_STRING");
                List<int> lGroupIDs = DAL.UtilsDal.GetAllRelatedGroups(m_nGroupID);
                string[] arrGroupIDs = lGroupIDs.Select(g => g.ToString()).ToArray();

                string sNewGuid = Guid.NewGuid().ToString();
                bool isActivated = DAL.UsersDal.UpdateUserActivationToken(arrGroupIDs, nUserID, sToken, sNewGuid, (int)UserState.LoggedOut);

                if (isActivated)
                {
                    bool resetSession = DAL.UsersDal.SetUserSessionStatus(nUserID, 0, 0);
                }

                bNotify = true;

                int nActivationStatus = DAL.UsersDal.GetUserActivateStatus(nUserID, arrGroupIDs);

                resp.m_user = (nActivationStatus == 1) ? u : null;
                resp.m_RespStatus = (nActivationStatus == 1) ? ResponseStatus.OK : ResponseStatus.UserNotActivated;

            }
            else    // Not Master
            {
                int nUsersDomainID = 0;
                int nTokenUserID = DAL.DomainDal.GetUserIDByDomainActivationToken(m_nGroupID, sToken, ref nUsersDomainID);

                if (nUserID != nTokenUserID)
                {
                    resp.m_user = null;
                    resp.m_RespStatus = ResponseStatus.UserNotActivated;

                    return resp;
                }

                string sNewGuid = Guid.NewGuid().ToString();
                bool isActivated = DAL.DomainDal.UpdateUserDomainActivationToken(m_nGroupID, nUsersDomainID, sToken, sNewGuid);

                if (isActivated)
                {
                    bool resetSession = DAL.UsersDal.SetUserSessionStatus(nUserID, 0, 0);
                }

                bNotify = true;

                int nActivationStatus = DAL.DomainDal.GetUserDomainActivateStatus(m_nGroupID, nUserID);

                resp.m_user = (nActivationStatus == 1) ? u : null;
                resp.m_RespStatus = (nActivationStatus == 1) ? ResponseStatus.OK : ResponseStatus.UserNotActivated;
            }

            if (bNotify)
            {
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
            }

            return resp;
        }

        protected override TvinciAPI.SendPasswordMailRequest GetSendPasswordMailRequest(string sFirstName, string sPassword, string sEmail)
        {
            TvinciAPI.SendPasswordMailRequest retVal = new TvinciAPI.SendPasswordMailRequest();
            retVal.m_sTemplateName  = m_sSendPasswordMailTemplate;
            retVal.m_sSubject       = m_sSendPasswordMailSubject;
            retVal.m_sSenderTo      = sEmail;
            retVal.m_sSenderName    = m_sMailFromName;
            retVal.m_sSenderFrom    = m_sMailFromAdd;
            retVal.m_sFirstName     = sFirstName;
            retVal.m_sPassword      = sPassword;
            retVal.m_eMailType      = TvinciAPI.eMailTemplateType.SendPassword;
            return retVal;
        }

        public override bool SendPasswordMail(string sUN)
        {
            if (string.IsNullOrEmpty(sUN))
            {
                return false;
            }

            Int32 nUserID = GetUserIDByUserName(sUN.Trim().Replace("\t", "").Replace("\n", ""));

            if (nUserID <= 0)
            {
                return false;
            }

            User u = new User(m_nGroupID, nUserID);
            //u.Initialize(nUserID, m_nGroupID);

            if ((u.m_oBasicData != null) && (!string.IsNullOrEmpty(u.m_oBasicData.m_sPassword)))
            {
                TvinciAPI.SendPasswordMailRequest sMailReq = GetSendPasswordMailRequest(u.m_oBasicData.m_sFirstName, u.m_oBasicData.m_sPassword, u.m_oBasicData.m_sEmail);

                bool sent = Utils.SendMail(m_nGroupID, sMailReq);
                return sent;
            }

            return false;
        }

        //public override bool IsUserActivated(ref string sUserName, ref int nUserID)
        //{
        //    if (!IsActivationNeeded(null))
        //        return true;

        //    bool bRet = DAL.UsersDal.IsUserActivated(m_nGroupID, m_nActivationMustHours, ref sUserName, ref nUserID);
        //    return bRet;
        //}


        public bool SendWelcomePasswordMail(UserResponseObject user)
        {
            bool sent = false;

            if (m_mailImpl != null)
            {
                sent = SendMailImpl(user.m_user);
            }
            else
            {
                TvinciAPI.WelcomeMailRequest sMailReq =
                    GetWelcomeMailRequest(GetUniqueTitle(user.m_user.m_oBasicData, user.m_user.m_oDynamicData),
                                        user.m_user.m_oBasicData.m_sUserName, user.m_user.m_oBasicData.m_sPassword, 
                                        user.m_user.m_oBasicData.m_sEmail, user.m_user.m_oBasicData.m_sFacebookID);

                sent = Utils.SendMail(m_nGroupID, sMailReq);
            }

            return sent;
        }
    }
}
