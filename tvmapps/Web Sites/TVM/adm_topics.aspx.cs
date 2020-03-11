using System;
using System.Data;
using System.Web;
using TvinciImporter;
using TVinciShared;

public partial class adm_topics : System.Web.UI.Page
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
        else if (LoginManager.IsPagePermitted("adm_topics.aspx") == false)
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
        Session["announcement_id"] = null;
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

        // GetAllAnnouncements - return DataTable with column format manipulation (subscribers)        
        DataTable dt = GetAllAnnouncements(nGroupID);
        if (dt != null)
        {
            if (!string.IsNullOrEmpty(sOrderBy))
            {
                theTable += " order by ";
                theTable += sOrderBy;
                dt.DefaultView.Sort = sOrderBy;
                dt = dt.DefaultView.ToTable();
            }

            theTable.FillDataTable(dt);
        }
        else
        {
            // in case Data Table return null ( no rows found) 
            // add Select query for empty grid layout
            theTable += "select ID, name , group_id, status, last_message_sent_date_sec as 'last sent date sec' ";
            theTable += ",CASE WHEN automatic_sending is null THEN  'Inherit' WHEN automatic_sending = 1 THEN  'Yes' WHEN automatic_sending = 0 THEN  'No'  end as 'automatic sending'";
            theTable += ",  name as  subscribers"; // add column subscribers only for page empty layout
            theTable += "  from announcements  ";
            theTable += "  where status <> 2  And recipient_type = 3 And";
            theTable += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
        }

        theTable.AddOrderByColumn("ID", "ID");
        theTable.AddHiddenField("group_id");
        theTable.AddHiddenField("status");
        theTable.AddHiddenField("announcementId");
        theTable.AddOrderByColumn("name", "name");
        theTable.AddOrderByColumn("automatic sending", "automatic sending");
        theTable.AddOrderByColumn("subscribers", "subscribers");
        theTable.AddOrderByColumn("last sent date sec", "last sent date sec");

        if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.EDIT))
        {
            DataTableLinkColumn linkColumn1 = new DataTableLinkColumn("adm_topics_new.aspx", "Edit", "");
            linkColumn1.AddQueryStringValue("announcement_id", "field=id");
            theTable.AddLinkColumn(linkColumn1);
        }

        if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.REMOVE))
        {
            DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_remove.aspx", "Delete", "STATUS=1;STATUS=3");
            linkColumn.AddQueryStringValue("id", "field=id");
            linkColumn.AddQueryStringValue("db", "notifications_connection");
            linkColumn.AddQueryStringValue("table", "announcements");
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
            linkColumn.AddQueryStringValue("table", "announcements");
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
            linkColumn.AddQueryStringValue("table", "announcements");
            linkColumn.AddQueryStringValue("db", "notifications_connection");
            linkColumn.AddQueryStringValue("confirm", "false");
            linkColumn.AddQueryStringValue("main_menu", "14");
            linkColumn.AddQueryStringValue("sub_menu", "2");
            linkColumn.AddQueryStringValue("rep_field", "username");
            linkColumn.AddQueryStringValue("rep_name", "Username");
            theTable.AddLinkColumn(linkColumn);
        }

    }

    private DataTable GetAllAnnouncements(int groupId)
    {
        DataTable dt = null;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery.SetConnectionKey("notifications_connection");
        selectQuery += "select ID, name , group_id, status, last_message_sent_date_sec as 'last sent date sec' ";
        selectQuery += ",CASE WHEN automatic_sending is null THEN  'Inherit' WHEN automatic_sending = 1 THEN  'Yes' WHEN automatic_sending = 0 THEN  'No'  end as 'automatic sending'";
        selectQuery += "  from announcements  ";
        selectQuery += "  where status <> 2  And recipient_type = 3 And";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", groupId);

        if (selectQuery.Execute("query", true) != null)
        {
            int nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
            {
                dt = selectQuery.Table("query");
            }
        }
        selectQuery.Finish();
        selectQuery = null;

        if (dt != null)
        {
            var dictAmountOfSubscribersPerAnnouncement = ImporterImpl.GetAmountOfSubscribersPerAnnouncement(groupId);
            if (dictAmountOfSubscribersPerAnnouncement != null && dictAmountOfSubscribersPerAnnouncement.Count > 0)
            {
                string announcementId = string.Empty; // dictAmountOfSubscribersPerAnnouncement key
                int amountOfSubscribers = 0; // dictAmountOfSubscribersPerAnnouncement value

                dt.Columns.Add("subscribers", typeof(int)); // amountOfSubscribers

                //going over result rows. 
                foreach (DataRow row in dt.Rows)
                {
                    announcementId = row["ID"].ToString();
                    //in case announcementId exist at Dic, add the subscribers amount
                    if (dictAmountOfSubscribersPerAnnouncement.ContainsKey(announcementId))
                    {
                        dictAmountOfSubscribersPerAnnouncement.TryGetValue(announcementId, out amountOfSubscribers);
                        row["subscribers"] = amountOfSubscribers;
                    }
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
        DBTableWebEditor theTable = new DBTableWebEditor(true, true, false, "", "adm_table_header", "adm_table_cell", "adm_table_alt_cell", "adm_table_link", "adm_table_pager", "adm_table", sOldOrderBy, 50);
        theTable.SetConnectionKey("notifications_connection");
        FillTheTableEditor(ref theTable, sOrderBy);

        string sTable = theTable.GetPageHTML(int.Parse(sPageNum), sOrderBy);

        theTable.Finish();
        theTable = null;
        return sTable;
    }

    public void GetHeader()
    {
        Response.Write(PageUtils.GetPreHeader() + ": Topics");
    }
}