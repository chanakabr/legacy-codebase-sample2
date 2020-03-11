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

public partial class adm_generic_insert : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
        {
            Response.Redirect("login.html");
            return;
        }
        if (!IsPostBack)
            Session["error_msg"] = "";
        string sBasePageURL = "";
        if (Session["ContentPage"] != null)
            sBasePageURL = Session["ContentPage"].ToString();
        if (sBasePageURL != "")
        {
            if (LoginManager.IsPagePermitted(sBasePageURL) == false)
            {
                LoginManager.LogoutFromSite("login.html");
                return;
            }
            if (LoginManager.IsActionPermittedOnPage(sBasePageURL, LoginManager.PAGE_PERMISION_TYPE.EDIT) == false)
            {
                LoginManager.LogoutFromSite("login.html");
                return;
            }
        }
        else
        {
            if (LoginManager.IsPagePermitted() == false)
            {
                LoginManager.LogoutFromSite("login.html");
                return;
            }
            if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.EDIT) == false)
            {
                LoginManager.LogoutFromSite("login.html");
                return;
            }
        }
        DBManipulator.DoTheWork();
    }
    
}
