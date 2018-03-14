using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using TVinciShared;
using ConfigurationManager;

public partial class AjaxLogin : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        string sErr = "";
        string sRet = "FAIL";

        bool http = ApplicationConfiguration.EnableHttpLogin.Value;    
        
        if (Request.Url.Host != "localhost" && Request.Url.Host != "127.0.0.1" && Request.Url.Scheme.ToUpper().Trim() != "HTTPS" && !http)
            sRet = "HTTPS_REQUIERED";
        else
        {
            string sEmail = "";
            string sPass = "";
            if (Request.Form["email"] != null)
                sEmail = Request.Form["email"].ToString();
            if (Request.Form["pass"] != null)
                sPass = Request.Form["pass"].ToString();

            bool bOK = LoginManager.LoginToSite(sEmail, sPass, ref sErr);
            if (bOK == true)
            {
                if (Session["LOGOUT_FROM_PAGE"] != null &&
                    Session["LOGOUT_FROM_PAGE"].ToString() != "")
                    sRet = Session["LOGOUT_FROM_PAGE"].ToString();
                else
                    sRet = TVinciShared.Menu.GetFirstLink();

                if (Session["RightHolder"] != null &&
                    Session["RightHolder"].ToString() != "")
                    sRet = "adm_fr_reports.aspx";

                Session["LOGOUT_FROM_PAGE"] = null;
            }
            else
            {
                sRet = sErr;
                Session["LOGOUT_FROM_PAGE"] = null;
            }
        }

        Response.CacheControl = "no-cache";
        Response.AddHeader("Pragma", "no-cache");
        Response.Expires = -1;
        Response.Clear();
        Response.Write(sRet + "~~|~~");
    }
}
