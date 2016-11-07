using System;
using TVinciShared;

public partial class adm_notification_settings : System.Web.UI.Page
{
    protected string m_sMenu;
    protected string m_sSubMenu;
    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        else if (LoginManager.IsPagePermitted("adm_notification_settings.aspx") == false)
            LoginManager.LogoutFromSite("login.html");
        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        Int32 nMenuID = 0;
        m_sMenu = TVinciShared.Menu.GetMainMenu(14, true, ref nMenuID);
        m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, true);

        if (!IsPostBack)
        {
            if (Request.QueryString["submited"] != null && Request.QueryString["submited"].ToString() == "1")
            {
                DBManipulator.DoTheWork("notifications_connection");
                return;
            }
        }
    }

    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
    }

    public void GetHeader()
    {
        Response.Write(PageUtils.GetPreHeader() + ": Notification Settings");
    }

    static protected Int32 GetMainLang(ref string sMainLang, ref string sCode)
    {
        Int32 nLangID = 0;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select l.CODE3,l.NAME,l.id from groups g,lu_languages l where l.id=g.language_id and  ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("g.id", "=", LoginManager.GetLoginGroupID());
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
            {
                sMainLang = selectQuery.Table("query").DefaultView[0].Row["NAME"].ToString();
                nLangID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
                sCode = selectQuery.Table("query").DefaultView[0].Row["CODE3"].ToString();
            }
        }
        selectQuery.Finish();
        selectQuery = null;
        return nLangID;
    }

    protected void GetSubMenu()
    {
        Response.Write(m_sSubMenu);
    }

    public string GetPageContent(string sOrderBy, string sPageNum)
    {
        object t = null;
        if (Session["error_msg"] != null && Session["error_msg"].ToString() != "")
        {
            Session["error_msg"] = "";
            return Session["last_page_html"].ToString();
        }

        object groupId = LoginManager.GetLoginGroupID();
        bool push = true;
        bool push_sa = true;
        int tableID = GetNotificationSettingsID(ODBCWrapper.Utils.GetIntSafeVal(groupId), ref push, ref push_sa);
        // check ig groupid is parent , if so show page with all filed else (not parent show a message)
        bool isParentGroup = IsParentGroup(ODBCWrapper.Utils.GetIntSafeVal(groupId));

        string sTable = string.Empty;
        if (!isParentGroup)
        {
            sTable = (PageUtils.GetPreHeader() + ": Module is not implemented");
        }
        else
        {
            if (tableID > 0) // a new record
            {
                t = tableID;
            }

            string sBack = "adm_notification_settings.aspx";

            DBRecordWebEditor theRecord = new DBRecordWebEditor("notification_settings", "adm_table_pager", sBack, "", "ID", t, sBack, "");
            theRecord.SetConnectionKey("notifications_connection");

            DataRecordCheckBoxField push_notification_enabled = new DataRecordCheckBoxField(true);
            push_notification_enabled.Initialize("Push notification enabled", "adm_table_header_nbg", "FormInput", "push_notification_enabled", false);
            push_notification_enabled.SetValue(push ? "1" : "0");

            theRecord.AddRecord(push_notification_enabled);

            DataRecordCheckBoxField push_system_announcements_enabled = new DataRecordCheckBoxField(true);
            push_system_announcements_enabled.Initialize("Push system announcements enabled", "adm_table_header_nbg", "FormInput", "push_system_announcements_enabled", false);
            push_system_announcements_enabled.SetValue(push_sa ? "1" : "0");
            theRecord.AddRecord(push_system_announcements_enabled);

            DataRecordShortIntField dr_start_time = new DataRecordShortIntField(true, 6, 6, 0, 24);
            dr_start_time.Initialize("Push start hour", "adm_table_header_nbg", "FormInput", "push_start_hour", false);
            theRecord.AddRecord(dr_start_time);

            DataRecordShortIntField dr_end_time = new DataRecordShortIntField(true, 6, 6, 0, 24);
            dr_end_time.Initialize("Push end hour", "adm_table_header_nbg", "FormInput", "push_end_hour", false);
            theRecord.AddRecord(dr_end_time);

            DataRecordCheckBoxField is_inbox_enable = new DataRecordCheckBoxField(true);
            is_inbox_enable.Initialize("Inbox enabled", "adm_table_header_nbg", "FormInput", "is_inbox_enable", false);
            is_inbox_enable.SetDefault(0);
            theRecord.AddRecord(is_inbox_enable);

            DataRecordShortIntField drTTL = new DataRecordShortIntField(true, 6, 6, 0, 90);
            drTTL.Initialize("Inbox message TTL(days)", "adm_table_header_nbg", "FormInput", "message_ttl", false);
            theRecord.AddRecord(drTTL);

            DataRecordCheckBoxField drAutomaticallyIssueNotification = new DataRecordCheckBoxField(true);
            drAutomaticallyIssueNotification.Initialize("Automatically issue follow notification ", "adm_table_header_nbg", "FormInput", "automatic_sending", false);
            drAutomaticallyIssueNotification.SetDefault(1);
            theRecord.AddRecord(drAutomaticallyIssueNotification);

            DataRecordShortIntField drTopicCeanup = new DataRecordShortIntField(true, 6, 6, 0, 365);
            drTopicCeanup.Initialize("Topic cleanup expiration(days)", "adm_table_header_nbg", "FormInput", "topic_cleanup_expiration_days", false);
            theRecord.AddRecord(drTopicCeanup);

            DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
            dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", false);
            dr_groups.SetValue(LoginManager.GetLoginGroupID().ToString());
            theRecord.AddRecord(dr_groups);

            sTable = theRecord.GetTableHTML("adm_notification_settings.aspx?submited=1");
        }
        return sTable;
    }

    private int GetNotificationSettingsID(int groupID, ref bool push, ref bool push_sa)
    {

        int notificationSettingId = 0;
        try
        {
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetConnectionKey("notifications_connection");
            selectQuery += "select ID, push_notification_enabled, push_system_announcements_enabled from notification_settings where status=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", groupID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    notificationSettingId = ODBCWrapper.Utils.GetIntSafeVal(selectQuery.Table("query").DefaultView[0].Row, "ID");
                    push = ODBCWrapper.Utils.ExtractBoolean(selectQuery.Table("query").DefaultView[0].Row, "push_notification_enabled");
                    push_sa = ODBCWrapper.Utils.ExtractBoolean(selectQuery.Table("query").DefaultView[0].Row, "push_system_announcements_enabled");
                }
            }
            selectQuery.Finish();
            selectQuery = null;
        }
        catch (Exception)
        {
            notificationSettingId = 0;
        }
        return notificationSettingId;

    }

    private bool IsParentGroup(int groupID)
    {
        bool res = false;
        try
        {
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetConnectionKey("MAIN_CONNECTION_STRING");
            selectQuery += "select PARENT_GROUP_ID from groups where";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", groupID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    int parentGroupID = ODBCWrapper.Utils.GetIntSafeVal(selectQuery.Table("query").DefaultView[0].Row, "PARENT_GROUP_ID");
                    if (parentGroupID == 1)
                    {
                        res = true;
                    }
                }
            }
            selectQuery.Finish();
            selectQuery = null;
        }
        catch (Exception)
        {
            res = false;
        }
        return res;
    }
}