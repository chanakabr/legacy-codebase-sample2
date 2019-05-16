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

public partial class adm_browse_as : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        if (!IsPostBack)
        {
            if (Request.QueryString["group_id"] != null &&
                Request.QueryString["group_id"].ToString() != "")
            {
                Int32 nRequestedGroup = int.Parse(Request.QueryString["group_id"].ToString());
                bool bOK = PageUtils.DoesGroupIsParentOfGroup(nRequestedGroup);
                if (bOK == true)
                {
                    Session["LoginGroup"] = nRequestedGroup;
                    Response.Redirect(TVinciShared.Menu.GetFirstLink());
                }
                else
                {
                    LoginManager.LogoutFromSite("login.html");
                    return;
                }
            }
            else
            {
                LoginManager.LogoutFromSite("login.html");
                return;
            }
        }
        Response.CacheControl = "no-cache";
        Response.AddHeader("Pragma", "no-cache");
        Response.Expires = -1;
    }
}
