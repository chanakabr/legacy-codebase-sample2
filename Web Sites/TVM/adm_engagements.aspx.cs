using System;
using System.Collections;
using System.Data;
using TVinciShared;

public partial class adm_engagements : System.Web.UI.Page
{
    protected string m_sMenu;
    protected string m_sSubMenu;
    protected string m_sSubSubMenu;
      
    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        else if (LoginManager.IsPagePermitted("adm_engagements.aspx") == false)
            LoginManager.LogoutFromSite("login.html");
        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        if (!IsPostBack)
        {
            Int32 nMenuID = 0;
            m_sMenu = TVinciShared.Menu.GetMainMenu(23, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, false);
            if (Request.QueryString["search_save"] != null)
                Session["search_save"] = "1";
            else
                Session["search_save"] = null;

            System.Collections.SortedList sortedMenu = GetSubMenuList();
            m_sSubSubMenu = TVinciShared.Menu.GetSubMenu(sortedMenu, -1, false);
        }
    }

    private SortedList GetSubMenuList()
    {
        SortedList sortedMenu = new SortedList();
        string sButton = "Add A.UserList";
        sButton += "|";
        sButton += "adm_engagements_new.aspx?user_list=1";
        sortedMenu[0] = sButton;

        sButton = "Add M.UserList";
        sButton += "|";
        sButton += "adm_engagements_new.aspx?user_list=2";
        sortedMenu[1] = sButton;

        return sortedMenu;

    }

    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
    }

    protected void GetSubMenu()
    {
        Response.Write(m_sSubMenu);
    }

    protected void GetSubSubMenu()
    {
        Response.Write(m_sSubSubMenu);
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
        Int32 groupId = LoginManager.GetLoginGroupID();
        theTable.SetConnectionKey("notifications_connection");
        theTable += "SELECT ID ,group_id ,status, is_active, engagement_type as 'Type' ,send_time as 'Send Time',total_number_of_recipients as '# Recipients',interval ";
        theTable += "FROM engagements with (nolock)";
        theTable += string.Format(" Where status<>2 and  group_id = {0} ", groupId);

        if (sOrderBy != "")
        {
            theTable += " order by ";
            theTable += sOrderBy;
        }
      
        theTable.AddHiddenField("ID");
        theTable.AddHiddenField("group_id");
        theTable.AddHiddenField("status");
        theTable.AddHiddenField("is_active");
       
        //if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.PUBLISH) &&  LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.EDIT))
        //{
        //    theTable.AddActivationField("engagements", "adm_engagements.aspx");
        //}

        //if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.EDIT))
        //{
        //    DataTableLinkColumn linkColumn1 = new DataTableLinkColumn("adm_engagements_new.aspx", "Edit", "");
        //    linkColumn1.AddQueryStringValue("engagement_id", "field=ID");
        //    theTable.AddLinkColumn(linkColumn1);
        //}

        //if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.REMOVE))
        //{
        //    DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_remove.aspx", "Delete", "STATUS=1;STATUS=3");
        //    linkColumn.AddQueryStringValue("id", "field=id");
        //    linkColumn.AddQueryStringValue("db", "notifications_connection");
        //    linkColumn.AddQueryStringValue("table", "engagements");
        //    linkColumn.AddQueryStringValue("confirm", "true");
        //    linkColumn.AddQueryStringValue("main_menu", "14");
        //    linkColumn.AddQueryStringValue("sub_menu", "2");
        //    linkColumn.AddQueryStringValue("rep_field", "username");
        //    linkColumn.AddQueryStringValue("rep_name", "Username");
        //    theTable.AddLinkColumn(linkColumn);
        //}

        //if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.PUBLISH))
        //{
        //    DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_confirm.aspx", "Confirm", "STATUS=3;STATUS=4");
        //    linkColumn.AddQueryStringValue("id", "field=id");
        //    linkColumn.AddQueryStringValue("db", "notifications_connection");
        //    linkColumn.AddQueryStringValue("table", "engagements");
        //    linkColumn.AddQueryStringValue("confirm", "true");
        //    linkColumn.AddQueryStringValue("main_menu", "14");
        //    linkColumn.AddQueryStringValue("sub_menu", "2");
        //    linkColumn.AddQueryStringValue("rep_field", "username");
        //    linkColumn.AddQueryStringValue("rep_name", "Username");
        //    theTable.AddLinkColumn(linkColumn);
        //}

        //if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.REMOVE))
        //{
        //    DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_confirm.aspx", "Cancel", "STATUS=3;STATUS=4");
        //    linkColumn.AddQueryStringValue("id", "field=id");
        //    linkColumn.AddQueryStringValue("table", "engagements");
        //    linkColumn.AddQueryStringValue("db", "notifications_connection");
        //    linkColumn.AddQueryStringValue("confirm", "false");
        //    linkColumn.AddQueryStringValue("main_menu", "14");
        //    linkColumn.AddQueryStringValue("sub_menu", "2");
        //    linkColumn.AddQueryStringValue("rep_field", "username");
        //    linkColumn.AddQueryStringValue("rep_name", "Username");
        //    theTable.AddLinkColumn(linkColumn);
        //}

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
        Response.Write(PageUtils.GetPreHeader() + ": Engagements");
    }
}