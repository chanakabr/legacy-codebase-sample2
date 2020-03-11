using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;

public partial class AjaxManipPassword : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        string sRet = "FAIL";

        string sSiteGUID = "";
        string sNewPass = "";
        if (Request.Form["user_id"] != null)
            sSiteGUID = Request.Form["user_id"].ToString();
        if (Request.Form["pass"] != null)
            sNewPass = Request.Form["pass"].ToString();
        string sError = "";
        bool bStrongPass = TVinciShared.LoginManager.IsPasswordStrong(sNewPass);
        if (bStrongPass == true)
        {
            ODBCWrapper.UpdateQuery updateUqery = new ODBCWrapper.UpdateQuery("users");
            updateUqery.SetConnectionKey("users_connection");
            updateUqery += ODBCWrapper.Parameter.NEW_PARAM("password", "=", sNewPass);
            updateUqery += " where ";
            updateUqery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", TVinciShared.LoginManager.GetLoginGroupID());
            updateUqery += " and ";
            updateUqery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", int.Parse(sSiteGUID));
            updateUqery.Execute();
            updateUqery.Finish();
            updateUqery = null;
            sRet = "OK";
        }
        else
        {
            sRet = "FAIL";
            sError = "Password not strong";
        }
        Response.CacheControl = "no-cache";
        Response.AddHeader("Pragma", "no-cache");
        Response.Expires = -1;
        Response.Clear();
        Response.Write(sRet + "~~|~~" + sError);
    }
}
