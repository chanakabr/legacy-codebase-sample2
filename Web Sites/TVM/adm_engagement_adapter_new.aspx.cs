using ApiObjects.Response;
using KLogMonitor;
using System;
using System.Reflection;
using TvinciImporter;
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
                int groupId = LoginManager.GetLoginGroupID();
                ApiObjects.Response.Status result = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

                int engagementAdapterId = DBManipulator.DoTheWork("notifications_connection");
                if (engagementAdapterId > 0)
                {
                    result = ImporterImpl.SetEngagementAdapterConfiguration(groupId, engagementAdapterId);
                }
                if (result == null)
                {
                    Session["error_msg_s"] = "Error";
                    Session["error_msg"] = "Error";
                }
                else if (result.Code != (int)ApiObjects.Response.eResponseStatus.OK)
                {
                    Session["error_msg"] = result.Message;
                    Session["error_msg_s"] = result.Message;
                }
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

        DataRecordShortTextField shortTextField = new DataRecordShortTextField("ltr", true, 60, 128);
        shortTextField.Initialize("Name", "adm_table_header_nbg", "FormInput", "NAME", true);
        theRecord.AddRecord(shortTextField);

        shortTextField = new DataRecordShortTextField("ltr", true, 60, 128);
        shortTextField.Initialize("Adapter URL", "adm_table_header_nbg", "FormInput", "adapter_url", true);
        theRecord.AddRecord(shortTextField);

        shortTextField = new DataRecordShortTextField("ltr", true, 60, 128);
        shortTextField.Initialize("Provider URL", "adm_table_header_nbg", "FormInput", "provider_url", true);
        theRecord.AddRecord(shortTextField);

        shortTextField = new DataRecordShortTextField("ltr", false, 60, 128);
        shortTextField.Initialize("Shared Secret", "adm_table_header_nbg", "FormInput", "shared_secret", false);
        theRecord.AddRecord(shortTextField);

        if (t == null)
        {
            string sharedSecret = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 16);
            shortTextField.SetValue(sharedSecret);
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