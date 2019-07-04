using System;
using System.Data;
using System.Web.Script.Serialization;
using ODBCWrapper;
using TVinciShared;
using System.Configuration;

public partial class OAuth : System.Web.UI.Page
{


    ProviderObject prov;
    AuthenticationObject authenticationObj = null;
    CredentialObject credsObj;

    ResponseObject responseObj;

    protected void Page_Load(object sender, EventArgs e)
    {
        int providerId;
        if (int.TryParse(Request.QueryString["ref"], out providerId))
        {
            // Get the provider details
            prov = OAuthUtil.GetProviderDetails(providerId);
            if (prov != null)
            {
                if (!string.IsNullOrEmpty(Request.QueryString["code"]))
                {

                    // Get Authentication object (access/refresh token) by authorization_code
                    authenticationObj = OAuthUtil.GetAuthenticationObject(prov, Request.QueryString["code"],
                                                                          eRequestType.AccessToken);
                    if (authenticationObj != null)
                    {
                        // Get user's credetials by access_token 
                        credsObj = OAuthUtil.GetCredentials(prov, authenticationObj.access_token);


                        if (credsObj.Status == "OK") // access_token OK:
                        {
                            // Adds user or updates existing user
                            HandleCredentials(credsObj);
                        }
                        else // Try the refresh_token
                        {
                            authenticationObj = OAuthUtil.GetAuthenticationObject(prov, authenticationObj.refresh_token,
                                                                                  eRequestType.Refresh);
                            credsObj = OAuthUtil.GetCredentials(prov, authenticationObj.access_token);
                            if (credsObj.Status == "OK")
                            {
                                HandleCredentials(credsObj);
                            }
                        }
                        Redirect();
                    }
                }
                // Error from operator
                else
                {

                    responseObj = new ResponseObject()
                        {
                            Status = "Error",
                            Error = "Credentials"
                        };

                    Redirect();
                }
            }
        }
    }

    private void Redirect()
    {
        JavaScriptSerializer serializer = new JavaScriptSerializer();
        string sRespURL = GetScopeRedirectUrl();


        if (!string.IsNullOrEmpty(sRespURL))
        {
            sRespURL += serializer.Serialize(responseObj);
            Response.Redirect(sRespURL);
        }
    }

    private string GetScopeRedirectUrl()
    {
        ODBCWrapper.DataSetSelectQuery selectQuery = new DataSetSelectQuery();
        selectQuery.SetConnectionKey("CONNECTION_STRING_TVINCI");
        selectQuery += "SELECT Redirect_URL FROM Operator_Scopes WHERE";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("Operator_ID", "=", prov.ID);
        selectQuery += "AND";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("Scope", "=", authenticationObj.scope);
        selectQuery += "AND";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("Group_id", "=", prov.GroupID);

        DataTable dt = selectQuery.Execute("query", true);

        if (dt != null)
        {
            if (dt.DefaultView.Count > 0)
            {
                return dt.Rows[0][0].ToString();
            }
        }
        return string.Empty;
    }

    private void HandleCredentials(CredentialObject credObj)
    {
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery.SetConnectionKey("USERS_CONNECTION_STRING");
        selectQuery += "SELECT uo.user_site_guid, u.Uid ";
        selectQuery += " FROM users_operators uo left	Join users u On		uo.user_site_guid = u.ID ";
        selectQuery += " where uo.status = 1 and uo.is_active = 1 and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("uo.co_guid", "=", credObj.Customer_ID);
        selectQuery += "AND";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("uo.operator_id", "=", prov.ID);
        DataTable dt = selectQuery.Execute("query", true);
        selectQuery.Finish();

        if (dt != null)
        {
            if (dt.DefaultView.Count > 0)
            {
                responseObj = OAuthUtil.UpdateUser(dt.Rows[0]["Uid"].ToString(), authenticationObj, credObj, prov);
            }
            else
            {
                responseObj = OAuthUtil.AddNewUser(prov, credObj, authenticationObj);
            }

        }
        else
        {
            responseObj = new ResponseObject() { Customer_ID = credObj.Customer_ID, Error = "Internal Error", Status = "Error", Provider_ID = prov.ID };
        }
    }




}
