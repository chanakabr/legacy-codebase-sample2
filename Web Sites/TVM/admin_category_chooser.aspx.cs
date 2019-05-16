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

public partial class admin_category_chooser : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        if (Request.QueryString["start_channel_id"] != null)
            Session["pl_channel_id"] = Request.QueryString["start_channel_id"].ToString();
        else
            Session["pl_channel_id"] = null;

        if (Request.QueryString["start_category_id"] != null)
            Session["pl_category_id"] = Request.QueryString["start_category_id"].ToString();
        else
            Session["pl_category_id"] = null;
        if (Request.QueryString["container_id"] != null)
            Session["container_id"] = Request.QueryString["container_id"].ToString();
        else
            Session["container_id"] = null;

        if (Request.QueryString["tvm_id"] != null)
        {
            Session["tvm_id"] = Request.QueryString["tvm_id"].ToString();
            Int32 nGroupID = LoginManager.GetLoginGroupID();
            string sConnKey = "tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString();
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select * from tvp_tvm_accounts where is_active=1 and status=1 and ";
            selectQuery.SetConnectionKey(sConnKey);
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", int.Parse(Session["tvm_id"].ToString()));
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    Session["pun"] = selectQuery.Table("query").DefaultView[0].Row["PLAYER_UN"].ToString();
                    Session["ppass"] = selectQuery.Table("query").DefaultView[0].Row["PLAYER_PASS"].ToString();
                }
            }
            selectQuery.Finish();
            selectQuery = null;
        }
        else
        {
            Session["tvm_id"] = null;

            if (Request.QueryString["pun"] != null)
                Session["pun"] = Request.QueryString["pun"].ToString();
            else
                Session["pun"] = null;

            if (Request.QueryString["ppass"] != null)
                Session["ppass"] = Request.QueryString["ppass"].ToString();
            else
                Session["ppass"] = null;
        }
    }

    public void GetRequieredType()
    {
        if (Session["pl_category_id"] != null)
            Response.Write("category");
        else if (Session["pl_channel_id"] != null)
            Response.Write("channel");
    }

    public void GetElementID()
    {
        if (Session["container_id"] != null)
            Response.Write(Session["container_id"].ToString() + "_val");
    }

    public void GetElementNum()
    {
        if (Session["container_id"] != null)
            Response.Write(Session["container_id"].ToString());
    }

    public void GetFlashVars()
    {
        string sRet = "auto_play=true&file_format=FLV&pic_size1=full&";
        sRet += "file_quality=HIGH";
        sRet += "&with_channels=true&no_cache=1&";
        if (Session["pl_channel_id"] != null)
            sRet += "&starting_channel_id=" + Session["pl_channel_id"].ToString();
        if (Session["pl_category_id"] != null)
            sRet += "&starting_category_id=" + Session["pl_category_id"].ToString();
        sRet += "&player_un=" + Session["pun"].ToString() + "&player_pass=" + Session["ppass"].ToString();
        Response.Write(sRet);
    }
}
