using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using KLogMonitor;
using TVinciShared;

public partial class adm_engagement_adapter_config_new : System.Web.UI.Page
{
    protected string m_sMenu;
    protected string m_sSubMenu;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            LoginManager.LogoutFromSite("login.html");

        else if (LoginManager.IsPagePermitted("adm_engagement_adapter.aspx") == false)
            LoginManager.LogoutFromSite("login.html");

        else if (LoginManager.IsActionPermittedOnPage("adm_engagement_adapter.aspx", LoginManager.PAGE_PERMISION_TYPE.EDIT) == false)
            LoginManager.LogoutFromSite("login.html");

        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        Int32 nMenuID = 0;
        if (!IsPostBack)
        {
            m_sMenu = TVinciShared.Menu.GetMainMenu(23, true, ref nMenuID, "adm_engagement_adapter.aspx");
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 8, true);
            if (Request.QueryString["submited"] != null && Request.QueryString["submited"].ToString() == "1")
            {
                DBManipulator.DoTheWork("notifications_connection");
                return;
            }

            if (Request.QueryString["engagement_adapter_config_id"] != null && Request.QueryString["engagement_adapter_config_id"].ToString() != "")
            {
                Session["engagement_adapter_config_id"] = int.Parse(Request.QueryString["engagement_adapter_config_id"].ToString());
            }
            else
            {
                Session["engagement_adapter_config_id"] = 0;
            }


            if (Session["engagement_adapter_id"] == null || Session["engagement_adapter_id"].ToString() == "" || Session["engagement_adapter_id"].ToString() == "0")
            {
                LoginManager.LogoutFromSite("login.html");
                return;
            }
        }
    }

    public void GetHeader()
    {
        if (Session["engagement_adapter_config_id"] != null && Session["engagement_adapter_config_id"].ToString() != "" && int.Parse(Session["engagement_adapter_config_id"].ToString()) != 0)
            Response.Write(PageUtils.GetPreHeader() + ": Engagement adapter params - Edit");
        else
            Response.Write(PageUtils.GetPreHeader() + ": Engagement adapter params - New");
    }

    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
    }

    protected void GetSubMenu()
    {
        Response.Write(m_sSubMenu);
    }

    public string GetPageContent(string sOrderBy, string sPageNum)
    {
        if (Session["error_msg"] != null && Session["error_msg"].ToString() != "")
        {
            Session["error_msg"] = "";
            return Session["last_page_html"].ToString();
        }
        Int32 groupID = LoginManager.GetLoginGroupID();

        object t = null; ;
        if (Session["engagement_adapter_config_id"] != null && Session["engagement_adapter_config_id"].ToString() != "" && int.Parse(Session["engagement_adapter_config_id"].ToString()) != 0)
            t = Session["engagement_adapter_config_id"];
        string sBack = "adm_engagement_adapter_config.aspx?search_save=1&engagement_adapter_id=" + Session["engagement_adapter_id"].ToString();

        DBRecordWebEditor theRecord = new DBRecordWebEditor("engagement_adapter_config", "adm_table_pager", sBack, "", "ID", t, sBack, "engagement_adapter_id");
        theRecord.SetConnectionKey("notifications_connection");

        DataRecordShortTextField dr_key = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_key.Initialize("Key", "adm_table_header_nbg", "FormInput", "keyName", false);
        theRecord.AddRecord(dr_key);

        DataRecordLongTextField dr_value = new DataRecordLongTextField("ltr", true, 60, 4);
        dr_value.Initialize("Value", "adm_table_header_nbg", "FormInput", "value", false);
        theRecord.AddRecord(dr_value);

        DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
        dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", false);
        dr_groups.SetValue(groupID.ToString());
        theRecord.AddRecord(dr_groups);

        DataRecordShortIntField dr_engagement_adapter_id = new DataRecordShortIntField(false, 9, 9);
        dr_engagement_adapter_id.Initialize("engagement_adapter_id", "adm_table_header_nbg", "FormInput", "engagement_adapter_id", false);
        dr_engagement_adapter_id.SetValue(Session["engagement_adapter_id"].ToString());
        theRecord.AddRecord(dr_engagement_adapter_id);

        string sTable = theRecord.GetTableHTML("adm_engagement_adapter_config_new.aspx?submited=1");
        return sTable;
    }
}