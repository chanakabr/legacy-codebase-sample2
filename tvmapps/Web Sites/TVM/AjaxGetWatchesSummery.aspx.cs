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
using TVinciShared;

public partial class AjaxGetWatchesSummery : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        if (Request.QueryString["media_id"] != null &&
                Request.QueryString["media_id"].ToString() != "")
        {
            Session["media_id"] = int.Parse(Request.QueryString["media_id"].ToString());
            Int32 nOwnerGroupID = int.Parse(PageUtils.GetTableSingleVal("media", "group_id", int.Parse(Session["media_id"].ToString())).ToString());
            Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();
            if (PageUtils.GetUpperGroupID(nOwnerGroupID) != PageUtils.GetUpperGroupID(nLogedInGroupID) && PageUtils.IsTvinciUser() == false)
            {
                LoginManager.LogoutFromSite("login.html");
                return;
            }
        }
        else if (Request.QueryString["channel_id"] != null &&
                Request.QueryString["channel_id"].ToString() != "")
        {
            Session["channel_id"] = int.Parse(Request.QueryString["channel_id"].ToString());
            Int32 nOwnerGroupID = int.Parse(PageUtils.GetTableSingleVal("channels", "group_id", int.Parse(Session["channel_id"].ToString())).ToString());
            Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();

            if (PageUtils.GetUpperGroupID(nOwnerGroupID) != PageUtils.GetUpperGroupID(nLogedInGroupID) && PageUtils.IsTvinciUser() == false)
            {
                LoginManager.LogoutFromSite("login.html");
                return;
            }
        }
        else if (Request.QueryString["player_id"] != null &&
            Request.QueryString["player_id"].ToString() != "")
        {
            Session["player_id"] = int.Parse(Request.QueryString["player_id"].ToString());
            Int32 nOwnerGroupID = int.Parse(PageUtils.GetTableSingleVal("groups_passwords", "group_id", int.Parse(Session["player_id"].ToString())).ToString());
            Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();

            if (PageUtils.GetUpperGroupID(nOwnerGroupID) != PageUtils.GetUpperGroupID(nLogedInGroupID) && PageUtils.IsTvinciUser() == false)
            {
                LoginManager.LogoutFromSite("login.html");
                return;
            }
        }
        if (Request.QueryString["start_date"] != null)
        {
            DateTime tStart = DateUtils.GetDateFromStr(Request.QueryString["start_date"].ToString());
            Session["start_date"] = tStart;
        }
        else
            Session["start_date"] = null;

        if (Request.QueryString["end_date"] != null)
        {
            DateTime tEnd = DateUtils.GetDateFromStr(Request.QueryString["end_date"].ToString());
            Session["end_date"] = tEnd;
        }
        else
            Session["end_date"] = null;


    }
}
