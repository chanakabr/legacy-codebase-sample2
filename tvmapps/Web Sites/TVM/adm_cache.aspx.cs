using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TVinciShared;

public partial class adm_cache : System.Web.UI.Page
{
    protected string m_sMenu;
    protected string m_sSubMenu;
    protected void Page_Load(object sender, EventArgs e)
    {
        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        Int32 nMenuID = 0;
        if (!IsPostBack)
        {
            m_sMenu = TVinciShared.Menu.GetMainMenu(7, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 5, true);
            if (Request.QueryString["submited"] != null && Request.QueryString["submited"].ToString() == "1")
            {
                DBManipulator.DoTheWork();
                Notifier.ClearServersCache(LoginManager.GetLoginGroupID());
                CachingManager.CachingManager.RemoveFromCache("");
            }
            if (Request.QueryString["after"] != null && Request.QueryString["after"].ToString() == "1")
                Session["cache_clean"] = "1";
            else
                Session["cache_clean"] = null;
        }
    }

    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
    }

    public void GetHeader()
    {
        string sRet = PageUtils.GetPreHeader() + ": Cache clear form";
        Response.Write(sRet);
    }

    protected void GetSubMenu()
    {
        Response.Write(m_sSubMenu);
    }

    public string GetPageContent(string sOrderBy, string sPageNum)
    {
        if (Session["cache_clean"] != null && Session["cache_clean"].ToString() == "1")
        {
            return "<br/><div class='alert_text nowrap'>&nbsp;&nbsp;&nbsp;*&nbsp;&nbsp;&nbsp;" + Session["tvp_cache_error"].ToString() + "</div>";
        }
        else
        {
            if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.PUBLISH))
            {
                if (Session["error_msg"] != null && Session["error_msg"].ToString() != "")
                {
                    Session["error_msg"] = "";
                    return Session["last_page_html"].ToString();
                }
                object t = null;

                string sBack = "adm_cache.aspx?after=1";
                DBRecordWebEditor theRecord = new DBRecordWebEditor("cache_clears", "adm_table_pager", sBack, "", "ID", t, sBack, "");

                DataRecordLongTextField dr_remarks = new DataRecordLongTextField("ltr", true, 60, 4);
                dr_remarks.Initialize("Clear reason", "adm_table_header_nbg", "FormInput", "EDITOR_REMARKS", true);
                theRecord.AddRecord(dr_remarks);

                DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
                dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", false);
                dr_groups.SetValue(LoginManager.GetLoginGroupID().ToString());
                theRecord.AddRecord(dr_groups);

                string sTable = theRecord.GetTableHTML("adm_cache.aspx?submited=1");

                return "<div class=alert_text nowrap>This page will clear the cache of the TVM. Do it only if the action is absolutly needed.</div>" + sTable;
            }
            else
                return "<br/><div class='alert_text nowrap'>&nbsp;&nbsp;&nbsp;*&nbsp;&nbsp;&nbsp;You are not allowed to do this action</div>";
        }
    }
}
