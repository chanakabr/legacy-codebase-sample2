using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TVinciShared;

public partial class adm_device_management : System.Web.UI.Page
{
    protected string m_sMenu;
    protected string m_sSubMenu;
    protected void Page_Load(object sender, EventArgs e)
    {
         if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        if (LoginManager.IsPagePermitted("adm_device_limitation_modules") == false)
            LoginManager.LogoutFromSite("login.html");
        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        if (!IsPostBack)
        {
            if (Request.QueryString["limit_module_id"] != null && !string.IsNullOrEmpty(Request.QueryString["limit_module_id"].ToString()))
            {
                Session["limit_module_id"] = Request.QueryString["limit_module_id"].ToString();
            }
            Int32 nMenuID = 0;
            m_sMenu = TVinciShared.Menu.GetMainMenu(2, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 4, false);
            if (Request.QueryString["search_save"] != null)
                Session["search_save"] = "1";
            else
                Session["search_save"] = null;


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
        string sBack = "adm_device_limitation_modules.aspx";
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
        theTable += ("select a.is_active,a.id as id,ac.id as family_id,ac.NAME as 'Name', a.max_limit as 'Limit', a.max_concurrent_limit as 'Concurrent Limit', a.status " + 
                     "from groups_device_families a with (nolock), lu_DeviceFamily ac with (nolock) where a.device_family_id = ac.id and ");
        theTable += ODBCWrapper.Parameter.NEW_PARAM("a.group_id", "=", nGroupID);
        theTable += " and (";
        theTable += ODBCWrapper.Parameter.NEW_PARAM("ac.Group_Id", "=", nGroupID);
        theTable += " or ";
        theTable += ODBCWrapper.Parameter.NEW_PARAM("ac.Group_Id", "=", 0);
        theTable += ") and ";
        theTable += ODBCWrapper.Parameter.NEW_PARAM("ac.Status", "=", 1);
        theTable += " and ";
        theTable += ODBCWrapper.Parameter.NEW_PARAM("a.limit_module_id", "=", int.Parse(Session["limit_module_id"].ToString()));
        if (sOrderBy != "")
        {
            theTable += " order by ";
            theTable += sOrderBy;
        }
        theTable.AddHiddenField("ID");
        theTable.AddHiddenField("family_id");
        theTable.AddHiddenField("status");
        theTable.AddActivationField("groups_device_families");
        theTable.AddHiddenField("is_active");

        DataTableLinkColumn linkColumn1 = new DataTableLinkColumn("adm_device_brands.aspx", "Devices", "");
        linkColumn1.AddQueryStringValue("family_id", "field=family_id");
        linkColumn1.AddQueryCounterValue("select count(*) as val from groups_device_brands where status=1 and is_active=1 and group_id=" + LoginManager.GetLoginGroupID() + " and device_family_id=", "field=family_id");
        theTable.AddLinkColumn(linkColumn1);


        DataTableLinkColumn linkColumn2 = new DataTableLinkColumn("adm_device_management_new.aspx", "Edit", "");
        linkColumn2.AddQueryStringValue("group_device_family_id", "field=id");
        theTable.AddLinkColumn(linkColumn2);

        {
            DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_remove.aspx", "Delete", "STATUS=1;STATUS=3");
            linkColumn.AddQueryStringValue("id", "field=id");
            linkColumn.AddQueryStringValue("table", "groups_device_families");
            linkColumn.AddQueryStringValue("confirm", "true");
            linkColumn.AddQueryStringValue("main_menu", "2");
            linkColumn.AddQueryStringValue("sub_menu", "3");
            linkColumn.AddQueryStringValue("rep_field", "username");
            linkColumn.AddQueryStringValue("rep_name", "Username");
            theTable.AddLinkColumn(linkColumn);

        }

        if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.PUBLISH))
        {
            DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_confirm.aspx", "Confirm", "STATUS=3;STATUS=4");
            linkColumn.AddQueryStringValue("id", "field=id");
            linkColumn.AddQueryStringValue("table", "groups_device_families");
            linkColumn.AddQueryStringValue("confirm", "true");
            linkColumn.AddQueryStringValue("main_menu", "2");
            linkColumn.AddQueryStringValue("sub_menu", "3");
            linkColumn.AddQueryStringValue("rep_field", "username");
            linkColumn.AddQueryStringValue("rep_name", "Username");
            theTable.AddLinkColumn(linkColumn);
        }

        
        {
            DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_confirm.aspx", "Cancel", "STATUS=3;STATUS=4");
            linkColumn.AddQueryStringValue("id", "field=id");
            linkColumn.AddQueryStringValue("table", "groups_devices");
            linkColumn.AddQueryStringValue("confirm", "false");
            linkColumn.AddQueryStringValue("main_menu", "2");
            linkColumn.AddQueryStringValue("sub_menu", "3");
            linkColumn.AddQueryStringValue("rep_field", "username");
            linkColumn.AddQueryStringValue("rep_name", "Username");
            theTable.AddLinkColumn(linkColumn);
        }
    }

    public string GetPageContent(string sOrderBy, string sPageNum)
    {
        string sOldOrderBy = "";
        if (Session["order_by"] != null)
            sOldOrderBy = Session["order_by"].ToString();
        DBTableWebEditor theTable = new DBTableWebEditor(true, true, true, "", "adm_table_header", "adm_table_cell", "adm_table_alt_cell", "adm_table_link", "adm_table_pager", "adm_table", sOldOrderBy, 50);
        FillTheTableEditor(ref theTable, sOrderBy);

        string sTable = theTable.GetPageHTML(int.Parse(sPageNum), sOrderBy, false);
        Session["ContentPage"] = "adm_device_limitation_modules.aspx";
        theTable.Finish();
        theTable = null;
        return sTable;
    }

    public void GetHeader()
    {
        Response.Write(PageUtils.GetPreHeader() + " : Device Families");
    }
}
