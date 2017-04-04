using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApiObjects;
using Core.Users;

namespace SSOTvinciImplementation
{
    public class User : KalturaSSOUsers, ISSOProvider
    {
        public User(int nGroupID, int operatorId)
            : base(nGroupID, operatorId) { }

        public override UserResponseObject PreSignIn(ref Int32 siteGuid, ref string userName, ref string password, ref int maxFailCount, ref int lockMin, ref int groupId, ref string sessionId, ref string ip, ref string deviceId, ref bool preventDoubleLogin, ref List<KeyValuePair> keyValueList)
        {
            UserResponseObject resObj = new UserResponseObject();
            Core.Users.User u = new Core.Users.User();
            siteGuid = u.InitializeByUsername(userName, GroupId);
            if (siteGuid > 0)
            {
                resObj = Core.Users.User.CheckUserPassword(userName, password, 0, 0, GroupId, false, false);
                if (resObj.m_RespStatus == ResponseStatus.OK)
                {
                    resObj = Core.Users.User.CheckUserPassword(userName, password, 0, 0, GroupId, false, false);
                    if (resObj.m_RespStatus == ResponseStatus.OK)
                    {
                        resObj.Initialize(ResponseStatus.OK, u);

                        // TODO: check this with Michael
                        //return Users.User.InnerSignIn(ref resObj, 0, 0, GroupId, sSessionID, sIP, sDeviceID, bPreventDoubleLogins, GroupId);
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
            Core.Users.User u = new Core.Users.User();
            UserResponseObject uRepsObj = new UserResponseObject();
            int nSiteGuid = u.InitializeByUsername(sUserName, GroupId);

            if (nSiteGuid == 0 || u.m_sSiteGUID.Length == 0)
                uRepsObj.Initialize(ResponseStatus.UserDoesNotExist, u);
            else
                uRepsObj.Initialize(ResponseStatus.OK, u);

            return uRepsObj;
        }
    }
}
