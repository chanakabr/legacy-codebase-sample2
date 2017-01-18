using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;
using System.Net;
using System.IO;
using System.Data;
using Core.Users;
using System.Configuration;
using KLogMonitor;
using System.Reflection;
using ApiObjects;

namespace TVinciShared
{
    public static class OAuthUtil
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private static byte[] data;
        private static JavaScriptSerializer serializer = new JavaScriptSerializer();

        // Get OAuth provider details
        public static ProviderObject GetProviderDetails(int providerId)
        {
            ProviderObject prov = null;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetConnectionKey("MAIN_CONNECTION_STRING");
            selectQuery += "SELECT NAME, URL_CODE, URL_CREDS, URL_REFRESH, Client_Id, Client_Secret, Scope, GROUP_ID FROM groups_operators where status=1 and is_active=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", providerId);
            DataTable dt = selectQuery.Execute("query", true);
            selectQuery.Finish();

            if (dt != null)
            {
                if (dt.DefaultView.Count > 0)
                {
                    string sKey = "OAuthRedirectURL_" + dt.Rows[0]["GROUP_ID"].ToString();
                    prov = new ProviderObject
                    {
                        ID = providerId,
                        GroupID = int.Parse(dt.Rows[0]["GROUP_ID"].ToString()),
                        Name = dt.Rows[0]["NAME"].ToString(),
                        URL_Code = dt.Rows[0]["URL_CODE"].ToString(),
                        URL_Creds = dt.Rows[0]["URL_CREDS"].ToString(),
                        URL_Refesh = dt.Rows[0]["URL_REFRESH"].ToString(),
                        ClientId = dt.Rows[0]["Client_Id"].ToString(),
                        ClientSecret = dt.Rows[0]["Client_Secret"].ToString(),
                        RedirectURL = TVinciShared.WS_Utils.GetTcmConfigValue(sKey) + providerId
                    };
                }

            }

            return prov;
        }

        //Gets user credentials by access_token
        public static CredentialObject GetCredentials(ProviderObject prov, string sToken)
        {
            string sCredsJSON = GetResponse(prov, sToken, eRequestType.Credentials);
            CredentialObject credObj = null;
            if (!string.IsNullOrEmpty(sCredsJSON))
            {
                credObj = serializer.Deserialize<CredentialObject>(sCredsJSON);
            }

            return credObj;
        }

        //Gets the access/refresh tokens
        public static AuthenticationObject GetAuthenticationObject(ProviderObject prov, string sToken, eRequestType Type)
        {
            AuthenticationObject AuthenticationObj = null;
            string sAuthenticationJSON = GetResponse(prov, sToken, Type);
            log.Debug("oAuth - " + sAuthenticationJSON + ":" + prov.GroupID);
            if (!string.IsNullOrEmpty(sAuthenticationJSON))
            {
                AuthenticationObj = serializer.Deserialize<AuthenticationObject>(sAuthenticationJSON);
            }
            return AuthenticationObj;
        }

        private static int GetDomainID(string sSiteGuid)
        {
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "SELECT domain_id from users_domains where status=1 and is_active=1 and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("user_id", "=", sSiteGuid);
            DataTable dt = selectQuery.Execute("query", true);

            selectQuery.Finish();

            if (dt != null)
            {
                if (dt.DefaultView.Count > 0)
                {
                    return int.Parse(dt.Rows[0][0].ToString());
                }
                else return 0;
            }
            else return 0;
        }

        public static ResponseObject AddNewUser(ProviderObject prov, CredentialObject credsObj, AuthenticationObject authenticationObj)
        {
            bool bRes = false;
            int domain_id = 0;

            string sWSUserName = string.Empty;
            string sWSPassword = string.Empty;
            WS_Utils.GetWSUNPass(prov.GroupID, "SSOAddNewUSER", "users", "1.1.1.1", ref sWSUserName, ref sWSPassword);

            BaseUsers t = null;
            prov.GroupID = Utils.GetGroupID(sWSUserName, sWSPassword, "AddNewUser", ref t);
            UserResponseObject wsRespObject = new UserResponseObject();
            if (prov.GroupID != 0 && t != null)
            {
                wsRespObject = t.AddNewUser(new UserBasicData() { m_sUserName = credsObj.Customer_ID, m_CoGuid = credsObj.Customer_ID }, new UserDynamicData(), Guid.NewGuid().ToString());
            }

            if (wsRespObject.m_RespStatus == ResponseStatus.OK || wsRespObject.m_RespStatus == ResponseStatus.UserExists)
            {

                if (wsRespObject.m_RespStatus != ResponseStatus.UserExists)
                {
                    Domain d = new Domain();
                    d.CreateNewDomain(credsObj.Customer_ID + "'s Domain", string.Empty, prov.GroupID, int.Parse(wsRespObject.m_user.m_sSiteGUID), "");
                    domain_id = d.m_nDomainID;
                }
                else
                {
                    domain_id = wsRespObject.m_user.m_domianID;
                }

                ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("users_operators");
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("user_site_guid", "=", wsRespObject.m_user.m_sSiteGUID);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("co_guid", "=", credsObj.Customer_ID);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("acess_token", "=", authenticationObj.access_token);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("refresh_token", "=", authenticationObj.refresh_token);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("expires_in", "=", DateTime.UtcNow.AddSeconds(double.Parse(authenticationObj.expires_in)));
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("operator_id", "=", prov.ID);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("is_active", "=", 1);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("status", "=", 1);

                bRes = insertQuery.Execute();
                insertQuery.Finish();

                return new ResponseObject
                {
                    Provider_ID = prov.ID,
                    Tvinci_ID = wsRespObject.m_user.m_sSiteGUID,
                    Customer_ID = credsObj.Customer_ID,
                    Status = bRes ? "OK" : "Error",
                    Error = bRes ? string.Empty : "Internal error: failed add to users_operators",
                    Domain_ID = domain_id,
                    Scope = authenticationObj.scope
                };
            }
            else
            {
                return new ResponseObject
                    {
                        Provider_ID = prov.ID,
                        Tvinci_ID = wsRespObject.m_user.m_sSiteGUID,
                        Customer_ID = credsObj.Customer_ID,
                        Status = "Error",
                        Error = "Internal Error: " + wsRespObject.m_RespStatus,
                        Domain_ID = domain_id,
                        Scope = authenticationObj.scope
                    };
            }

        }


        public static ResponseObject UpdateUser(string sSiteGuid, AuthenticationObject authenticationObj, CredentialObject credsObj, ProviderObject prov)
        {
            ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("users_operators");
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("acess_token", "=", authenticationObj.access_token);
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("refresh_token", "=", authenticationObj.refresh_token);
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("expires_in", "=", DateTime.UtcNow.AddSeconds(double.Parse(authenticationObj.expires_in)));
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("update_date", "=", DateTime.UtcNow);
            updateQuery += " WHERE ";

            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("co_guid", "=", credsObj.Customer_ID);
            updateQuery += " AND ";
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("operator_id", "=", prov.ID);
            updateQuery += " AND ";
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("user_site_guid", "=", sSiteGuid);
            updateQuery += " AND ";
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("status", "=", 1);
            updateQuery += " AND ";
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("is_active", "=", 1);
            bool bRes = updateQuery.Execute();
            updateQuery.Finish();

            return new ResponseObject
                {
                    Provider_ID = prov.ID,
                    Tvinci_ID = sSiteGuid,
                    Customer_ID = credsObj.Customer_ID,
                    Status = bRes ? "OK" : "Error",
                    Error = bRes ? string.Empty : "Internal error in update users_operators",
                    Domain_ID = GetDomainID(sSiteGuid),
                    Scope = authenticationObj.scope
                };
        }

        // Creates a web request, initializes the content and returns the response as a string
        private static string GetResponse(ProviderObject prov, string sToken, eRequestType type)
        {
            HttpWebRequest req = null;

            switch (type)
            {
                case eRequestType.Credentials:
                    req = (HttpWebRequest)WebRequest.Create(prov.URL_Creds + "?client_id=" + prov.ClientId);
                    log.Debug("oAuth - " + prov.URL_Creds + "?client_id=" + prov.ClientId);
                    break;
                case eRequestType.AccessToken:
                    req = (HttpWebRequest)WebRequest.Create(prov.URL_Code);
                    log.Debug("oAuth - " + prov.URL_Code);
                    break;
                case eRequestType.Refresh:
                    req = (HttpWebRequest)WebRequest.Create(prov.URL_Refesh);
                    break;

                default:
                    break;
            }
            if (req != null)
            {
                InitWebRequest(prov, sToken, ref req, type);
                try
                {
                    using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
                    {
                        using (StreamReader reader = new StreamReader(resp.GetResponseStream()))
                        {
                            string s = reader.ReadToEnd();
                            return s;
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.Error("", ex);
                    return string.Empty;
                }

            }

            return string.Empty;
        }

        //Initializes the WebRequest with data/headers whatever needed
        private static void InitWebRequest(ProviderObject prov, string sToken, ref HttpWebRequest req, eRequestType type)
        {
            req.ContentType = "application/x-www-form-urlencoded";
            req.Method = "POST";

            switch (type)
            {
                case eRequestType.AccessToken:
                    setDataObjForRequest(prov, sToken, eRequestType.AccessToken);
                    GetRequestStream(ref req);
                    break;
                case eRequestType.Credentials:
                    req.ContentLength = 0;
                    req.Headers.Add("Authorization", "Bearer " + sToken);
                    req.Headers.Add("grant_type", "credentials");
                    data = new byte[0];
                    break;
                case eRequestType.Refresh:
                    setDataObjForRequest(prov, sToken, eRequestType.Refresh);
                    GetRequestStream(ref req);
                    break;
                default:
                    break;
            }

        }

        // Writes the request Data-Stream
        private static void GetRequestStream(ref HttpWebRequest req)
        {
            req.ContentLength = data.Length;
            using (Stream dataStream = req.GetRequestStream())
            {
                dataStream.Write(data, 0, data.Length);
                dataStream.Close();
            }
        }

        // Sets the data object for the request content
        private static void setDataObjForRequest(ProviderObject prov, string token, eRequestType type)
        {
            string sTokenVarName = string.Empty;
            string sGrantType = string.Empty;
            string sURL = string.Empty;

            switch (type)
            {
                case eRequestType.AccessToken:
                    sTokenVarName = "code";
                    sGrantType = "authorization_code";
                    break;
                case eRequestType.Credentials:
                    sTokenVarName = "access_token";
                    sGrantType = "credentials";
                    break;
                case eRequestType.Refresh:
                    sTokenVarName = "refresh_token";
                    sGrantType = "refresh_token";
                    break;

                default:
                    break;

            }

            data = Encoding.UTF8.GetBytes(
                                          string.Format(
                                                        "client_id={0}&client_secret={1}&redirect_uri={2}&{3}={4}&grant_type={5}",
                                                        prov.ClientId,
                                                        prov.ClientSecret,
                                                        prov.RedirectURL,
                                                        sTokenVarName,
                                                        token,
                                                        sGrantType
                                                        )
                                          );
        }
    }

    public class AuthenticationObject
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public string expires_in { get; set; }
        public string refresh_token { get; set; }
        public string scope { get; set; }
    }

    public class CredentialObject
    {
        public string Status { get; set; }
        public string Error { get; set; }
        public string Customer_ID { get; set; }
    }


    public enum eRequestType
    {
        AccessToken = 1,
        Credentials = 2,
        Refresh = 3
    }

    public class ProviderObject
    {
        public int ID { get; set; }
        public int GroupID { get; set; }
        public string Name { get; set; }
        public string URL_Code { get; set; }
        public string URL_Creds { get; set; }
        public string URL_Refesh { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string RedirectURL { get; set; }
    }

    public class ResponseObject
    {
        public int Provider_ID { get; set; }
        public string Tvinci_ID { get; set; }
        public string Customer_ID { get; set; }
        public int Domain_ID { get; set; }
        public string Scope { get; set; }
        public string Status { get; set; }
        public string Error { get; set; }
    }
}

