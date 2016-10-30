using ApiObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Users
{
    public class SSOTvinciImplementation : SSOUsers, ISSOProvider
    {
        public SSOTvinciImplementation(int nGroupID, int operatorId)
            : base(nGroupID, operatorId)
        {
        }

        public override UserResponseObject SignIn(string sCoGuid, string sPass, int nOperatorID, int nMaxFailCount, int nLockMinutes, string sSessionID, string sIP, string sDeviceID, bool bPreventDoubleLogins)
        {
            UserResponseObject resObj = new UserResponseObject();
            User u = new User();
            int nSiteGuid = u.InitializeByUsername(sCoGuid, m_nGroupID);
            if (nSiteGuid > 0)
            {
                resObj = User.CheckUserPassword(sCoGuid, sPass, 0, 0, m_nGroupID, false, false);
                if (resObj.m_RespStatus == ResponseStatus.OK)
                {
                    resObj = User.CheckUserPassword(sCoGuid, sPass, 0, 0, m_nGroupID, false, false);
                    if (resObj.m_RespStatus == ResponseStatus.OK)
                    {
                        resObj.Initialize(ResponseStatus.OK, u);
                        return User.InnerSignIn(ref resObj, 0, 0, m_nGroupID, sSessionID, sIP, sDeviceID, bPreventDoubleLogins, m_nGroupID);
                    }
                    else return resObj;
                }
                else return resObj;
            }
            else resObj.m_RespStatus = ResponseStatus.UserDoesNotExist;
            return resObj;
        }


        public UserResponseObject CheckLogin(string sUserName, int nOperatorID)
        {
            User u = new User();
            UserResponseObject uRepsObj = new UserResponseObject();
            int nSiteGuid = u.InitializeByUsername(sUserName, m_nGroupID);

            if (nSiteGuid == 0 || u.m_sSiteGUID.Length == 0)
            {
                uRepsObj.Initialize(ResponseStatus.UserDoesNotExist, u);
            }
            else
            {
                uRepsObj.Initialize(ResponseStatus.OK, u);
            }

            return uRepsObj;
        }


    }
}
