using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using KLogMonitor;
using TVinciShared;

public partial class adm_engagement_adapter_new : System.Web.UI.Page
{
    private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
    protected string m_sMenu;
    protected string m_sSubMenu;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        if (LoginManager.IsPagePermitted("adm_engagement_adapter.aspx") == false)
            LoginManager.LogoutFromSite("login.html");
        if (LoginManager.IsActionPermittedOnPage("adm_engagement_adapter.aspx", LoginManager.PAGE_PERMISION_TYPE.EDIT) == false)
            LoginManager.LogoutFromSite("login.html");

        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        if (!IsPostBack)
        {
            bool flag = false;

            if (Request.QueryString["submited"] != null && Request.QueryString["submited"].ToString().Trim() == "1")
            {
                DBManipulator.DoTheWork("notifications_connection");
                return;
            }
            
            int menuID = 0;
            m_sMenu = TVinciShared.Menu.GetMainMenu(23, true, ref menuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(menuID, 8, true);

            if (Request.QueryString["engagement_adapter_id"] != null && Request.QueryString["engagement_adapter_id"].ToString() != "")
            {
                Session["engagement_adapter_id"] = int.Parse(Request.QueryString["engagement_adapter_id"].ToString());
            }
            else if (!flag)
                Session["engagement_adapter_id"] = 0;
        }
    }

    public void GetHeader()
    {
        string sRet = PageUtils.GetPreHeader() + ": Engagement adapter";
        if (Session["engagement_adapter_id"] != null && Session["engagement_adapter_id"].ToString() != "" && Session["engagement_adapter_id"].ToString() != "0")
            sRet += " - Edit";
        else
            sRet += " - New";
        Response.Write(sRet);
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
        object t = null; ;
        if (Session["engagement_adapter_id"] != null && Session["engagement_adapter_id"].ToString() != "" && int.Parse(Session["engagement_adapter_id"].ToString()) != 0)
            t = Session["engagement_adapter_id"];
        string sBack = "adm_engagement_adapter.aspx?search_save=1";

        object group_id = LoginManager.GetLoginGroupID();

        DBRecordWebEditor theRecord = new DBRecordWebEditor("engagement_adapter", "adm_table_pager", sBack, "", "ID", t, sBack, "");

        DataRecordShortTextField dr_name = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_name.Initialize("Name", "adm_table_header_nbg", "FormInput", "NAME", true);
        theRecord.AddRecord(dr_name);

        //DataRecordShortTextField dr_external_identifier = new DataRecordShortTextField("ltr", true, 60, 128);
        //dr_external_identifier.Initialize("External Identifier", "adm_table_header_nbg", "FormInput", "external_identifier", true);
        //theRecord.AddRecord(dr_external_identifier);

        DataRecordShortTextField dr_adapter_url = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_adapter_url.Initialize("Provider URL", "adm_table_header_nbg", "FormInput", "provider_url", true);
        theRecord.AddRecord(dr_adapter_url);

        DataRecordShortTextField dr_shared_secret = new DataRecordShortTextField("ltr", false, 60, 128);
        dr_shared_secret.Initialize("Shared Secret", "adm_table_header_nbg", "FormInput", "shared_secret", false);
        theRecord.AddRecord(dr_shared_secret);

        if (t == null)
        {
            string sharedSecret = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 16);
            dr_shared_secret.SetValue(sharedSecret);
        }

        DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
        dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "group_id", false);
        dr_groups.SetValue(group_id.ToString());
        theRecord.AddRecord(dr_groups);

        string sTable = theRecord.GetTableHTML("adm_engagement_adapter_new.aspx?submited=1");

        return sTable;
    }

    static public string GetWSURL(string sKey)
    {
        return TVinciShared.WS_Utils.GetTcmConfigValue(sKey);
    }
}