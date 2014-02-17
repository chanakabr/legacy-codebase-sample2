using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Users
{
    public class SSOTvinciImplementation : SSOUsers, ISSOProviderImplementation
    {
        public SSOTvinciImplementation(int nGroupID)
            : base(nGroupID)
        {
        }



        public override UserResponseObject SignIn(string sUN, string sPass, int nMaxFailCount, int nLockMinutes, int nGroupID, string sessionID, string sIP, string deviceID, bool bPreventDoubleLogins)
        {
            UserResponseObject resObj = new UserResponseObject();
            User u = new User();
            u.InitializeByUsername(sUN, m_nGroupID);
            if (int.Parse(u.m_sSiteGUID) != 0)
            {
                resObj = User.CheckUserPassword(sUN, sPass, 0, 0, m_nGroupID, false, false);
                if (resObj.m_RespStatus == ResponseStatus.OK)
                {
                    resObj = User.CheckUserPassword(sUN, sPass, 0, 0, m_nGroupID, false, false);
                    if (resObj.m_RespStatus == ResponseStatus.OK)
                    {
                        resObj.Initialize(ResponseStatus.OK, u);
                        return User.InnerSignIn(ref resObj, 0, 0, m_nGroupID, sessionID, sIP, deviceID, bPreventDoubleLogins, m_nGroupID);
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
            u.InitializeByUsername(sUserName, m_nGroupID);

            if (u.m_sSiteGUID == string.Empty)
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
