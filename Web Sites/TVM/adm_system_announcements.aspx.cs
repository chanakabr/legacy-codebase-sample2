using System;
using System.Data;
using System.Web;
using TVinciShared;

public partial class adm_system_announcements : System.Web.UI.Page
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
        else if (LoginManager.IsPagePermitted("adm_system_announcements.aspx") == false)
            LoginManager.LogoutFromSite("login.html");
        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        // add permission for the page
        if (!IsPostBack)
        {
           
            Int32 nMenuID = 0;
            m_sMenu = TVinciShared.Menu.GetMainMenu(14, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 3, false);
            if (Request.QueryString["search_save"] != null)
                Session["search_save"] = "1";
            else
                Session["search_save"] = null;
        }
        Session["message_announcement_id"] = null;
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

    static protected string GetMainLang()
    {
        string sMainLang = "";
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select l.CODE3,l.id from groups g,lu_languages l where l.id=g.language_id and  ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("g.id", "=", LoginManager.GetLoginGroupID());
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
            {
                sMainLang = selectQuery.Table("query").DefaultView[0].Row["CODE3"].ToString();
            }
        }
        selectQuery.Finish();
        selectQuery = null;
        return sMainLang;
    }

    protected void FillTheTableEditor(ref DBTableWebEditor theTable, string sOrderBy)
    {
        Int32 nGroupID = LoginManager.GetLoginGroupID();
        string sGroupLang = GetMainLang();
        theTable.SetConnectionKey("notifications_connection");

        // GetAllMessageAnnouncements - return DataTable with column format manipulation (start_time)        
        DataTable dt = GetAllMessageAnnouncements(nGroupID);
        if (dt != null)
        {
            theTable.FillDataTable(dt);
        }
        else
        {
            // in case Data Table return null ( no rows found) 
            // add Select query for empty grid layout
            theTable += "select a.ID, a.recipients as 'recipientsCode', a.status, a.is_active, a.name, a.message, a.start_time, a.sent, a.updater_id, a.update_date,a.create_date, ";
            theTable += " a.group_id, a.timezone , ";
            theTable += " CASE  WHEN a.recipients = 0 THEN  'All'  WHEN a.recipients = 1 THEN 'LoggedIn'  when a.recipients = 2 then 'Guests' ";
            theTable += " when a.recipients = 3 then 'Other' end as 'recipients' , ";
            theTable += " CASE WHEN a.sent = 0 THEN  'Not Sent'  WHEN a.sent = 1 THEN 'Sending' when a.sent = 2 then 'Sent' when a.sent = 3 then 'Aborted' end as 'message status' ";
            theTable += "  from message_announcements a   ";
            theTable += "  where a.status <> 2  And a.recipients <> 3 And";
            theTable += ODBCWrapper.Parameter.NEW_PARAM("a.group_id", "=", nGroupID);
            theTable += " order by id desc ";
        }
      
        theTable.AddHiddenField("ID");
        theTable.AddHiddenField("group_id");
        theTable.AddHiddenField("status");
        theTable.AddHiddenField("is_active");
        theTable.AddHiddenField("recipientsCode");
        theTable.AddHiddenField("updater_id");
        theTable.AddHiddenField("update_date");
        theTable.AddHiddenField("create_date");
        theTable.AddHiddenField("sent");
       
        if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.PUBLISH) &&  LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.EDIT))
        {
            theTable.AddActivationField("message_announcements", "adm_system_announcements.aspx");
        }

        if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.EDIT))
        {
            DataTableLinkColumn linkColumn1 = new DataTableLinkColumn("adm_system_announcements_new.aspx", "Edit", "sent=0");
            linkColumn1.AddQueryStringValue("message_announcement_id", "field=id");
            theTable.AddLinkColumn(linkColumn1);
        }

        if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.REMOVE))
        {
            DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_remove.aspx", "Delete", "STATUS=1;STATUS=3");
            linkColumn.AddQueryStringValue("id", "field=id");
            linkColumn.AddQueryStringValue("db", "notifications_connection");
            linkColumn.AddQueryStringValue("table", "message_announcements");
            linkColumn.AddQueryStringValue("confirm", "true");
            linkColumn.AddQueryStringValue("main_menu", "14");
            linkColumn.AddQueryStringValue("sub_menu", "2");
            linkColumn.AddQueryStringValue("rep_field", "username");
            linkColumn.AddQueryStringValue("rep_name", "Username");
            theTable.AddLinkColumn(linkColumn);
        }

        if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.PUBLISH))
        {
            DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_confirm.aspx", "Confirm", "STATUS=3;STATUS=4");
            linkColumn.AddQueryStringValue("id", "field=id");
            linkColumn.AddQueryStringValue("db", "notifications_connection");
            linkColumn.AddQueryStringValue("table", "message_announcements");
            linkColumn.AddQueryStringValue("confirm", "true");
            linkColumn.AddQueryStringValue("main_menu", "14");
            linkColumn.AddQueryStringValue("sub_menu", "2");
            linkColumn.AddQueryStringValue("rep_field", "username");
            linkColumn.AddQueryStringValue("rep_name", "Username");
            theTable.AddLinkColumn(linkColumn);
        }

        if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.REMOVE))
        {
            DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_confirm.aspx", "Cancel", "STATUS=3;STATUS=4");
            linkColumn.AddQueryStringValue("id", "field=id");
            linkColumn.AddQueryStringValue("table", "message_announcements");
            linkColumn.AddQueryStringValue("db", "notifications_connection");
            linkColumn.AddQueryStringValue("confirm", "false");
            linkColumn.AddQueryStringValue("main_menu", "14");
            linkColumn.AddQueryStringValue("sub_menu", "2");
            linkColumn.AddQueryStringValue("rep_field", "username");
            linkColumn.AddQueryStringValue("rep_name", "Username");
            theTable.AddLinkColumn(linkColumn);
        }

    }

    private DataTable GetAllMessageAnnouncements(int nGroupID)
    {
        DataTable dt = null;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery.SetConnectionKey("notifications_connection");
        selectQuery += "select a.ID, a.recipients as 'recipientsCode', a.status, a.is_active, a.name, a.message, a.start_time, a.sent, a.updater_id, a.update_date,a.create_date, ";
        selectQuery += " a.group_id, a.timezone , ";
        selectQuery += " CASE  WHEN a.recipients = 0 THEN  'All'  WHEN a.recipients = 1 THEN 'LoggedIn'  when a.recipients = 2 then 'Guests' ";
        selectQuery += " when a.recipients = 3 then 'Other' end as 'recipients' , ";
        selectQuery += " CASE WHEN a.sent = 0 THEN  'Not Sent'  WHEN a.sent = 1 THEN 'Sending' when a.sent = 2 then 'Sent' when a.sent = 3 then 'Aborted' end as 'message status' ";
        selectQuery += "  from message_announcements a   ";
        selectQuery += "  where a.status <> 2  And a.recipients <> 3 And";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("a.group_id", "=", nGroupID);
        selectQuery += " order by id desc ";
        
        if (selectQuery.Execute("query", true) != null)
        {
            int nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
            {
                dt = selectQuery.Table("query");

                foreach (DataRow dr in dt.Rows)
                {
                    DateTime date_time = ODBCWrapper.Utils.GetDateSafeVal(dr["start_time"]);
                    string time_zone = ODBCWrapper.Utils.GetSafeStr(dr["timezone"]);
                    DateTime local_date_time = ODBCWrapper.Utils.ConvertFromUtc(date_time, time_zone);
                    dr["start_time"] = local_date_time;
                }
            }
        }
        selectQuery.Finish();
        selectQuery = null;


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
        Response.Write(PageUtils.GetPreHeader() + ": System Announcements");
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