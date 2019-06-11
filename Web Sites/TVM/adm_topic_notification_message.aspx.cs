using System;
using System.Data;
using System.Web;
using TVinciShared;

public partial class adm_topic_notification_message : System.Web.UI.Page
{
    protected string m_sMenu;
    protected string m_sSubMenu;


    protected void Page_PreRender(object sender, EventArgs e)
    {
        if (HttpContext.Current.Session["error_msg"] != null || Session["error_msg"] != null)
        {
            hfError.Value = (HttpContext.Current.Session["error_msg"] != null)
                ? Session["error_msg"].ToString()
                : HttpContext.Current.Session["error_msg"].ToString();

            HttpContext.Current.Session["error_msg"] = null;
        }

    }
    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        else if (LoginManager.IsPagePermitted("adm_topic_notification.aspx") == false)
            LoginManager.LogoutFromSite("login.html");
        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;

        Int32 nMenuID = 0;
        if (!IsPostBack)
        {
            m_sMenu = TVinciShared.Menu.GetMainMenu(14, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 3, false);
            if (Request.QueryString["search_save"] != null)
                Session["search_save"] = "1";
            else
                Session["search_save"] = null;
        }
        Session["topic_notification_message_id"] = null;

        int topicNotificationId = 0;
        if ((Request.QueryString["topic_notification_id"] != null&& !string.IsNullOrEmpty(Request.QueryString["topic_notification_id"].ToString())
            && int.TryParse(Request.QueryString["topic_notification_id"].ToString(), out topicNotificationId) && topicNotificationId > 0)
            || (Session["topic_notification_id"] != null && !string.IsNullOrEmpty(Session["topic_notification_id"].ToString())
            && int.TryParse(Session["topic_notification_id"].ToString(), out topicNotificationId) && topicNotificationId > 0))
        {
            Session["topic_notification_id"] = topicNotificationId;
        }
        else
        {
            Session["topic_notification_id"] = 0;
        }
    }

    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
    }

    protected void GetSubMenu()
    {
        Response.Write(m_sSubMenu);
    }

    public string GetTableCSV()
    {
        string sOldOrderBy = "";
        if (Session["order_by"] != null)
            sOldOrderBy = Session["order_by"].ToString();
        DBTableWebEditor theTable = new DBTableWebEditor(true, true, false, "", "adm_table_header", "adm_table_cell", "adm_table_alt_cell", "adm_table_link", "adm_table_pager", "adm_table", sOldOrderBy, 50);
        FillTheTableEditor(ref theTable, sOldOrderBy);

        string sCSVFile = theTable.OpenCSV();
        theTable.Finish();
        theTable = null;
        return sCSVFile;
    }

    protected void FillTheTableEditor(ref DBTableWebEditor theTable, string sOrderBy)
    {
        int groupId = LoginManager.GetLoginGroupID();
        theTable.SetConnectionKey("notifications_connection");

        //GetAllMessageAnnouncements - return DataTable with column format manipulation(start_time)
        DataTable dt = GetTopicNotificationMessages(groupId);
        if (dt != null)
        {
            theTable.FillDataTable(dt);
            theTable.AddHiddenField("TopicNotificationId");

            DataTableLinkColumn linkColumn1 = new DataTableLinkColumn("adm_topic_notification_message_new.aspx", "Edit", string.Empty);
            linkColumn1.AddQueryStringValue("topic_notification_message_id", "field=id");
            linkColumn1.AddQueryStringValue("topic_notification_id", "field=TopicNotificationId");
            theTable.AddLinkColumn(linkColumn1);
        }
    }

    private DataTable GetTopicNotificationMessages(int groupId)
    {
        DataTable dt = new DataTable();
        dt.Columns.Add("ID");
        dt.Columns.Add("Message");
        dt.Columns.Add("status");
        dt.Columns.Add("TopicNotificationId");

        long topicNotificationId = 0;
        if (Session["topic_notification_id"] != null
            && long.TryParse(Session["topic_notification_id"].ToString(), out topicNotificationId))
        {
            var messages = TvinciImporter.NotificationHelper.GetTopicNotificationMessages(groupId, topicNotificationId, 100, 0);
            if (messages != null && messages.Length > 0)
            {
                foreach (var item in messages)
                {
                    DataRow dr = dt.NewRow();
                    dr["ID"] = item.Id;
                    dr["Message"] = item.Message;
                    dr["status"] = item.Status == 0 ? "pending" : "sent";
                    dr["TopicNotificationId"] = item.TopicNotificationId;
                    dt.Rows.Add(dr);
                }
            }
        }

        return dt;
    }

    public string GetPageContent(string sOrderBy, string sPageNum)
    {
        string sOldOrderBy = "";
        if (Session["order_by"] != null)
            sOldOrderBy = Session["order_by"].ToString();

        int topicNotificationId = 0;
        if (Session["topic_notification_id"] != null
            && int.TryParse(Session["topic_notification_id"].ToString(), out topicNotificationId) && topicNotificationId > 0)
        { }

        DBTableWebEditor theTable = new DBTableWebEditor(true, true, true, "topic_notification_id=" + topicNotificationId, "adm_table_header", "adm_table_cell", "adm_table_alt_cell", "adm_table_link", "adm_table_pager", "adm_table", sOldOrderBy, 50);
        theTable.SetConnectionKey("notifications_connection");
        FillTheTableEditor(ref theTable, sOrderBy);

        string sTable = theTable.GetPageHTML(int.Parse(sPageNum), sOrderBy);

        theTable.Finish();
        theTable = null;
        return sTable;
    }

    public void GetHeader()
    {
        Response.Write(PageUtils.GetPreHeader() + ": Topic Notification Messages");
    }

    public void UpdateOnOffStatus(string theTableName, string sID, string sStatus)
    {
        try
        {
        }
        catch (Exception)
        {

        }
    }
}