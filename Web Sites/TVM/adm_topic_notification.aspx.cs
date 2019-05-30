using System;
using System.Data;
using System.Web;
using TVinciShared;

public partial class adm_topic_notification : System.Web.UI.Page
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

        Session["topic_notification_id"] = null;
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
        Int32 nGroupID = LoginManager.GetLoginGroupID();
        theTable.SetConnectionKey("notifications_connection");

        //GetAllMessageAnnouncements - return DataTable with column format manipulation(start_time)
        DataTable dt = GetAllTopicNotifications(nGroupID);
        if (dt != null)
        {
            theTable.FillDataTable(dt);            
            theTable.AddHiddenField("group_id");
            theTable.AddHiddenField("status");
            theTable.AddHiddenField("is_active");
            theTable.AddHiddenField("updater_id");

            DataTableLinkColumn linkColumn2 = new DataTableLinkColumn("adm_topic_notification_message.aspx", "Messages", "");
            linkColumn2.AddQueryStringValue("topic_notification_id", "field=id");
            theTable.AddLinkColumn(linkColumn2);

            if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.EDIT))
            {
                DataTableLinkColumn linkColumn1 = new DataTableLinkColumn("adm_topic_notification_new.aspx", "Edit", string.Empty);
                linkColumn1.AddQueryStringValue("topic_notification_id", "field=id");
                theTable.AddLinkColumn(linkColumn1);
            }
        }
    }

    private DataTable GetAllTopicNotifications(int groupId)
    {
        DataTable dt = new DataTable();
        dt.Columns.Add("ID");
        dt.Columns.Add("Name");
        dt.Columns.Add("Description");
        dt.Columns.Add("Subscription Id");
        dt.Columns.Add("status");        
        dt.Columns.Add("updater_id");

        var topics = TvinciImporter.NotificationHelper.GetGroupTopicNotifications(groupId);
        if (topics != null && topics.Length > 0)
        {
            foreach (var item in topics)
            {
                DataRow dr = dt.NewRow();
                dr["ID"] = item.Id;
                dr["Name"] = item.Name;
                dr["Description"] = item.Description;
                dr["Subscription Id"] = ((ApiObjects.Notification.SubscriptionSubscribeReference)item.SubscribeReference).SubscriptionId;
                dr["status"] = "1";                
                dr["updater_id"] = 0;
                dt.Rows.Add(dr);
            }
        }

        return dt;
    }

    public string GetPageContent(string sOrderBy, string sPageNum)
    {
        string sOldOrderBy = "";
        if (Session["order_by"] != null)
            sOldOrderBy = Session["order_by"].ToString();
        DBTableWebEditor theTable = new DBTableWebEditor(true, true, true, "", "adm_table_header", "adm_table_cell", "adm_table_alt_cell", "adm_table_link", "adm_table_pager", "adm_table", sOldOrderBy, 50);
        theTable.SetConnectionKey("notifications_connection");
        FillTheTableEditor(ref theTable, sOrderBy);

        string sTable = theTable.GetPageHTML(int.Parse(sPageNum), sOrderBy);

        theTable.Finish();
        theTable = null;
        return sTable;
    }

    public void GetHeader()
    {
        Response.Write(PageUtils.GetPreHeader() + ": Topic Notifications");
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