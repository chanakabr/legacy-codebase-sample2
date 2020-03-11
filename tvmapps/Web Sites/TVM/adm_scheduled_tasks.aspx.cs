using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using TVinciShared;

public partial class adm_scheduled_tasks : System.Web.UI.Page
{
    protected string m_sMenu;
    protected string m_sSubMenu;
    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        if (LoginManager.IsPagePermitted() == false)
            LoginManager.LogoutFromSite("login.html");
        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        if (!IsPostBack)
        {
            Int32 nMenuID = 0;
            m_sMenu = TVinciShared.Menu.GetMainMenu(16, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 2, false);
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
        Int32 nGroupID = LoginManager.GetLoginGroupID();
        theTable += "select st.id as id,st.DLL as 'Task DLL',st.RUN_DATE as 'Run Date',st.PARAMETERS as 'Parameters',lrs.description as 'Run Status',st.INTERVAL as 'Interval(Sec)' from lu_run_status lrs,scheduled_tasks st where lrs.id=st.RUN_STATUS";
        if (sOrderBy != "")
        {
            theTable += " order by ";
            theTable += sOrderBy;
        }
        else
            theTable += "order by run_date desc";
        theTable.AddHiddenField("ID");
        //theTable.AddHiddenField("status");

        /*
        if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.REMOVE))
        {
            DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_remove.aspx", "Delete", "STATUS=1;STATUS=3");
            linkColumn.AddQueryStringValue("id", "field=id");
            linkColumn.AddQueryStringValue("table", "windows_services");
            linkColumn.AddQueryStringValue("confirm", "true");
            linkColumn.AddQueryStringValue("main_menu", "17");
            linkColumn.AddQueryStringValue("sub_menu", "1");
            linkColumn.AddQueryStringValue("rep_field", "username");
            linkColumn.AddQueryStringValue("rep_name", "Username");
            theTable.AddLinkColumn(linkColumn);
        }

        if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.PUBLISH))
        {
            DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_confirm.aspx", "Confirm", "STATUS=3;STATUS=4");
            linkColumn.AddQueryStringValue("id", "field=id");
            linkColumn.AddQueryStringValue("table", "windows_services");
            linkColumn.AddQueryStringValue("confirm", "true");
            linkColumn.AddQueryStringValue("main_menu", "17");
            linkColumn.AddQueryStringValue("sub_menu", "1");
            linkColumn.AddQueryStringValue("rep_field", "username");
            linkColumn.AddQueryStringValue("rep_name", "Username");
            theTable.AddLinkColumn(linkColumn);
        }

        if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.REMOVE))
        {
            DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_confirm.aspx", "Cancel", "STATUS=3;STATUS=4");
            linkColumn.AddQueryStringValue("id", "field=id");
            linkColumn.AddQueryStringValue("table", "windows_services");
            linkColumn.AddQueryStringValue("confirm", "false");
            linkColumn.AddQueryStringValue("main_menu", "17");
            linkColumn.AddQueryStringValue("sub_menu", "1");
            linkColumn.AddQueryStringValue("rep_field", "username");
            linkColumn.AddQueryStringValue("rep_name", "Username");
            theTable.AddLinkColumn(linkColumn);
        }
         */
    }

    public void GetHeader()
    {
        Response.Write(PageUtils.GetPreHeader() + ": Scheduled Tasks");
    }

    public string ChangeServiceStatus(string sAction, string sID)
    {
        if ((sID == "3" || sID == "6") && sAction == "Stop")
            return "Services are not allowed to be terminated!!!";
        ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("windows_services");
        if (sAction == "Stop")
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("REQUEST_TYPE", "=", 1);
        if (sAction == "Start")
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("REQUEST_TYPE", "=", 2);
        updateQuery += "where";
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", int.Parse(sID));
        updateQuery.Execute();
        updateQuery.Finish();
        updateQuery = null;
        return "OK - The service status will be changed in the next few seconds.";
    }

    public string GetPageContent(string sOrderBy, string sPageNum)
    {
        string sOldOrderBy = "";
        if (Session["order_by"] != null)
            sOldOrderBy = Session["order_by"].ToString();
        DBTableWebEditor theTable = new DBTableWebEditor(true, true, false, "", "adm_table_header", "adm_table_cell", "adm_table_alt_cell", "adm_table_link", "adm_table_pager", "adm_table", sOldOrderBy, 50);
        FillTheTableEditor(ref theTable, sOrderBy);

        string sTable = theTable.GetPageHTML(int.Parse(sPageNum), sOrderBy);
        theTable.Finish();
        theTable = null;
        return sTable;
    }
}
