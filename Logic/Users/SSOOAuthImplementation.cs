using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Web.Script.Serialization;
using System.IO;
using System.Data;
using TVinciShared;
using ApiObjects;

namespace Core.Users
{
    public class SSOOAuthImplementation : SSOUsers, ISSOProvider
    {
        ProviderObject prov = null;
        CredentialObject creds = null;
        AuthenticationObject authObj = null;
        OAuthUserDetails userDetails = null;

        public SSOOAuthImplementation(int nGroupID, int operatorId)
            : base(nGroupID, operatorId)
        {
            
        }



        public override UserResponseObject SignIn(string sCoGuid, string sPass, int nOperatorID, int nMaxFailCount, int nLockMinutes, string sSessionID, string sIP, string sDeviceID, bool bPreventDoubleLogins)
        {
            prov = OAuthUtil.GetProviderDetails(nOperatorID);
            if (prov != null)
            {

                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "SELECT * FROM users_operators WHERE";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("CO_GUID", "=", sCoGuid);
                selectQuery += " AND ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("OPERATOR_ID", "=", nOperatorID);
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
                            {
                                creds = OAuthUtil.GetCredentials(prov, userDetails.AccessToken);
                            }
                            if (creds != null)
                            {
                                if (creds.Status == "OK")
                                {
                                    return User.SignIn(int.Parse(userDetails.SiteGuid), nMaxFailCount, nLockMinutes, m_nGroupID, sSessionID, sIP, sDeviceID, bPreventDoubleLogins);
                                }
                                else
                                {
                                    return UseRefreshToken(nMaxFailCount, nLockMinutes, sSessionID, sIP, sDeviceID, bPreventDoubleLogins);
                                }
                            }
                            else
                            {
                                return new UserResponseObject() { m_RespStatus = ResponseStatus.UserDoesNotExist };
                            }

                        }
                        else
                        {
                            return UseRefreshToken(nMaxFailCount, nLockMinutes, sSessionID, sIP, sDeviceID, bPreventDoubleLogins);
                        }

                    }
                    else
                    {
                        return new UserResponseObject() { m_RespStatus = ResponseStatus.UserDoesNotExist };
                    }
                }
                else
                {
                    return new UserResponseObject() { m_RespStatus = ResponseStatus.UserDoesNotExist };
                }
            }
            else return new UserResponseObject() { m_RespStatus = ResponseStatus.UserDoesNotExist };
        }

    
        public UserResponseObject CheckLogin(string sUserName, int nOperatorID)
        {
            return this.SignIn(sUserName, string.Empty, nOperatorID, 0, 0, string.Empty, string.Empty, string.Empty, false);
        }


        private UserResponseObject UseRefreshToken(int nMaxFailCount, int nLockMinutes, string sSessionID, string sIP, string sDeviceID, bool bPreventDoubleLogins)
        {
            authObj = OAuthUtil.GetAuthenticationObject(prov, userDetails.RefreshToken, eRequestType.Refresh);
            creds = OAuthUtil.GetCredentials(prov, authObj.access_token);
            if (creds.Status == "OK")
            {
                OAuthUtil.UpdateUser(userDetails.SiteGuid, authObj, creds, prov);
                return User.SignIn(int.Parse(userDetails.SiteGuid), nMaxFailCount, nLockMinutes, m_nGroupID, sSessionID, sIP, sDeviceID, bPreventDoubleLogins);
            }
            else
            {
                return new UserResponseObject() { m_RespStatus = ResponseStatus.UserDoesNotExist, m_user = new User() { m_sSiteGUID = userDetails.SiteGuid } };
            }
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
