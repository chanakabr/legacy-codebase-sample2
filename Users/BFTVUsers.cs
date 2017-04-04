using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Users
{
    public class BFTVUsers : TvinciUsers
    {
        public BFTVUsers(Int32 nGroupID)
            : base(nGroupID)
        {
        }

        public override void Hit(string sSiteGUID)
        {
            throw new NotImplementedException();
        }

        public override UserResponseObject GetUserByCoGuid(string sCoGuid, int operatorID)
        {
            throw new NotImplementedException();
        }

        public override UserResponseObject SignIn(string sUN, string sPass, int nMaxFailCount, int nLockMinutes, int nGroupID, string sessionID, string sIP, string deviceID, bool bPreventDoubleLogins)
        {
            return base.SignIn(sUN, sPass, nMaxFailCount, nLockMinutes, nGroupID, sessionID, sIP, deviceID, bPreventDoubleLogins);
        }

        public override void Logout(string sSiteGUID)
        {
            throw new NotImplementedException();
        }

        public override UserResponseObject GetUserByUsername(string sUsername, int nGroupID)
        {
            throw new NotImplementedException();
        }

        public override UserResponseObject GetUserByFacebookID(string sFacebookID, int nGroupID)
        {
            throw new NotImplementedException();
        }

        public override ApiObjects.Response.Status IsUserActivated(ref string sUserName, ref Int32 nUserID)
        {
            throw new NotImplementedException();
        }

        public override UserResponseObject CheckUserPassword(string sUN, string sPass, int nMaxFailCount, int nLockMinutes, Int32 nGroupID, bool bPreventDoubleLogins)
        {
            throw new NotImplementedException();
        }

        public override UserResponseObject ActivateAccount(string sUN, string sToken)
        {
            throw new NotImplementedException();
        }

        public override UserResponseObject AddNewUser(UserBasicData oBasicData, UserDynamicData sDynamicData, string sPassword)
        {
            throw new NotImplementedException();
        }

        public override bool ResendActivationMail(string sUN)
        {
            throw new NotImplementedException();
        }

        public override bool ResendWelcomeMail(string sUN)
        {
            throw new NotImplementedException();
        }

        public override bool DoesUserNameExists(string sUserName)
        {
            throw new NotImplementedException();
        }

        public override UserResponseObject AddNewUser(string sBasicDataXML, string sDynamicDataXML, string sPassword)
        {
            throw new NotImplementedException();
        }

        public override UserResponseObject GetUserData(string sSiteGUID)
        {
            throw new NotImplementedException();
        }

        public override string GetUserToken(string sSiteGUID, Int32 nGroupID)
        {
            throw new NotImplementedException();
        }

        public override UserResponseObject SetUserData(string sSiteGUID, string sBasicDataXML, string sDynamicDataXML)
        {
            throw new NotImplementedException();
        }

        public override UserResponseObject SetUserData(string sSiteGUID, UserBasicData oBasicData, UserDynamicData sDynamicData)
        {
            throw new NotImplementedException();
        }

        public override UserResponseObject ChangeUserPassword(string sUN, string sOldPass, string sPass, int nGroupID)
        {
            throw new NotImplementedException();
        }

        public override UserResponseObject RenewPassword(string sUN, string sPass, int nGroupID)
        {
            throw new NotImplementedException();
        }

        public override UserResponseObject ForgotPassword(string sUN)
        {
            throw new NotImplementedException();
        }

        public override void Initialize()
        {
            return;
        }

        public override UserResponseObject CheckToken(string sToken)
        {
            UserResponseObject resp = new UserResponseObject();
            string sUN = TVinciShared.WS_Utils.GetTcmConfigValue("BF_WS_UN");
            string sPass = TVinciShared.WS_Utils.GetTcmConfigValue("BF_WS_PASS");
            string sSecret = TVinciShared.WS_Utils.GetTcmConfigValue("BF_WS_SECRET");
            User u = new User();
            string sName = "";
            string sUid = "";
            BFTVFeeder.feeder.GetUserDetails(sToken, ref sUid, ref sName, sUN, sPass , sSecret);
            if (sUid != "")
            {
                u.m_sSiteGUID = sUid;
                u.m_oBasicData = new UserBasicData();
                u.m_oBasicData.m_sUserName = sName;
                resp.m_user = u;
                resp.m_RespStatus = ResponseStatus.OK;
            }
            else
                resp.m_RespStatus = ResponseStatus.UserDoesNotExist;
            return resp;
        }

        //public override Domain AddDomain(string domainName, string domainDescription, Int32 masterUserGuid, Int32 nGroupID)
        //{
        //    return null;
        //}
        //public override Domain SetDomainInfo(Int32 domainID, string domainName, Int32 nGroupID, string domainDescription)
        //{
        //    return null;
        //}
        //public override Domain AddUserToDomain(Int32 nGroupID, Int32 domainID, Int32 userGuid, bool isMaster)
        //{
        //    return null;
        //}
        //public override Domain RemoveUserFromDomain(Int32 nGroupID, Int32 domainID, Int32 userGUID)
        //{
        //    return null;
        //}
        //public override Domain GetDomainInfo(Int32 domainID, Int32 nGroupID)
        //{
        //    return null;
        //}
    }
}
