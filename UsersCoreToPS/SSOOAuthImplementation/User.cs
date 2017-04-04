using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApiObjects;
using TVinciShared;
using Core.Users;

namespace SSOOAuthImplementation
{
    public class User : KalturaSSOUsers, ISSOProvider
    {
        ProviderObject prov = null;
        CredentialObject creds = null;
        AuthenticationObject authObj = null;
        OAuthUserDetails userDetails = null;

        public User(int nGroupID, int operatorId)
            : base(nGroupID, operatorId) { }


        public override UserResponseObject PreSignIn(ref Int32 siteGuid, ref string userName, ref string password, ref int maxFailCount, ref int lockMin, ref int groupId, ref string sessionId, ref string ip, ref string deviceId, ref bool preventDoubleLogin, ref List<KeyValuePair> keyValueList)
        {
            // get operation ID from key-value pair list
            int operatorId;
            var keyValueOperatorId = keyValueList.FirstOrDefault(x => x.key == "operator");
            if (keyValueOperatorId != null)
                operatorId = Convert.ToInt32(keyValueOperatorId.value);
            else
                return new UserResponseObject() { m_RespStatus = ResponseStatus.InternalError };


            prov = OAuthUtil.GetProviderDetails(operatorId);
            if (prov != null)
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "SELECT * FROM users_operators WHERE";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("CO_GUID", "=", userName);
                selectQuery += " AND ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("OPERATOR_ID", "=", operatorId);
                selectQuery += " AND ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("status", "=", 1);
                selectQuery += " AND ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("is_active", "=", 1);

                DataTable dt = selectQuery.Execute("query", true);
                selectQuery.Finish();

                if (dt != null)
                {
                    if (dt.Rows.Count > 0)
                    {
                        userDetails = new OAuthUserDetails()
                        {
                            SiteGuid = dt.Rows[0]["user_site_guid"].ToString(),
                            CoGuid = dt.Rows[0]["CO_GUID"].ToString(),
                            ExpiresIn = DateTime.Parse(dt.Rows[0]["expires_in"].ToString()),
                            AccessToken = dt.Rows[0]["acess_token"].ToString(),
                            RefreshToken = dt.Rows[0]["refresh_token"].ToString()
                        };
                        if (userDetails.ExpiresIn > DateTime.UtcNow)
                        {

                            if (prov != null)
                                creds = OAuthUtil.GetCredentials(prov, userDetails.AccessToken);

                            if (creds != null)
                            {
                                if (creds.Status == "OK")
                                {
                                    siteGuid = int.Parse(userDetails.SiteGuid);
                                    return new UserResponseObject();
                                }
                                else
                                    return UseRefreshToken(ref siteGuid, ref maxFailCount, ref lockMin, ref sessionId, ref ip, ref deviceId, ref preventDoubleLogin);
                            }
                            else
                                return new UserResponseObject() { m_RespStatus = ResponseStatus.UserDoesNotExist };
                        }
                        else
                            return UseRefreshToken(ref siteGuid, ref maxFailCount, ref lockMin, ref sessionId, ref ip, ref deviceId, ref preventDoubleLogin);
                    }
                    else
                        return new UserResponseObject() { m_RespStatus = ResponseStatus.UserDoesNotExist };
                }
                else
                    return new UserResponseObject() { m_RespStatus = ResponseStatus.UserDoesNotExist };
            }
            else return new UserResponseObject() { m_RespStatus = ResponseStatus.UserDoesNotExist };
        }

        public UserResponseObject CheckLogin(string sUserName, int nOperatorID)
        {
            //return this.MidSignIn(sUserName, string.Empty, nOperatorID, 0, 0, string.Empty, string.Empty, string.Empty, false);
            // TODO: talk to Michael Mars about this!
            return null;
        }


        private UserResponseObject UseRefreshToken(ref Int32 siteGuid, ref int nMaxFailCount, ref int nLockMinutes, ref string sSessionID, ref string sIP, ref string sDeviceID, ref bool bPreventDoubleLogins)
        {
            authObj = OAuthUtil.GetAuthenticationObject(prov, userDetails.RefreshToken, eRequestType.Refresh);
            creds = OAuthUtil.GetCredentials(prov, authObj.access_token);
            if (creds.Status == "OK")
            {
                OAuthUtil.UpdateUser(userDetails.SiteGuid, authObj, creds, prov);
                siteGuid = int.Parse(userDetails.SiteGuid);
                return new UserResponseObject();
            }
            else
                return new UserResponseObject() { m_RespStatus = ResponseStatus.UserDoesNotExist, m_user = new Core.Users.User() { m_sSiteGUID = userDetails.SiteGuid } };
        }

        private class OAuthUserDetails
        {
            public string CoGuid { get; set; }
            public string SiteGuid { get; set; }
            public string AccessToken { get; set; }
            public string RefreshToken { get; set; }
            public DateTime ExpiresIn { get; set; }
        }
    }
}

