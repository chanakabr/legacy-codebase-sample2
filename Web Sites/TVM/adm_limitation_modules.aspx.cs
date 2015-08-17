using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TVinciShared;

public partial class adm_limitation_modules : System.Web.UI.Page
{
    protected string m_sMenu;
    protected string m_sSubMenu;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!LoginManager.CheckLogin())
            Response.Redirect("login.html");
        if (!LoginManager.IsPagePermitted("adm_domain_limitation_modules.aspx"))
            LoginManager.LogoutFromSite("login.html");
        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        if (Request.QueryString["limit_module_id"] != null && Request.QueryString["limit_module_id"].ToString().Length > 0)
        {
            Session["limit_module_id"] = Int32.Parse(Request.QueryString["limit_module_id"].ToString());
        }
        if (!IsPostBack)
        {
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
        int parentLimitModuleID = 0;
        if (Session["limit_module_id"] != null && Session["limit_module_id"].ToString().Length > 0)
        {
            parentLimitModuleID = Int32.Parse(Session["limit_module_id"].ToString());
        }
        theTable += "select gdflm.is_active, gdflm.id, gdflm.status, gdflm.description as 'Name', gdflm.value as 'Value', ludlm.description as 'Type' from groups_device_families_limitation_modules gdflm with (nolock) ";
        theTable += "inner join lu_device_limitation_modules ludlm with (nolock) on ludlm.ID=gdflm.type where ";
        theTable += "gdflm.status=1 and ludlm.status=1 and ";
        theTable += ODBCWrapper.Parameter.NEW_PARAM("gdflm.group_id", "=", nGroupID);
        theTable += " and ";
        theTable += ODBCWrapper.Parameter.NEW_PARAM("gdflm.parent_limit_module_id", "=", parentLimitModuleID);
        if (sOrderBy.Length > 0)
        {
            theTable += " order by ";
            theTable += sOrderBy;
        }
        theTable.AddHiddenField("ID");
        theTable.AddHiddenField("status");
        theTable.AddActivationField("groups_device_families_limitation_modules");
        theTable.AddHiddenField("is_active");

        //DataTableLinkColumn linkColumn1 = new DataTableLinkColumn("adm_device_management.aspx", "Device Families", "");
        //linkColumn1.AddQueryStringValue("limit_module_id", "field=id");
        //linkColumn1.AddQueryCounterValue("select count(*) as val from groups_device_families with (nolock) where status=1 and is_active=1 and group_id=" + LoginManager.GetLoginGroupID() + " and device_family_id=", "field=id");
        //theTable.AddLinkColumn(linkColumn1);
        DataTableLinkColumn linkColumn1 = new DataTableLinkColumn("adm_limitation_modules_new.aspx", "Edit", "");
        linkColumn1.AddQueryStringValue("limit_id", "field=id");
        linkColumn1.AddQueryStringValue("parent_limit_id", parentLimitModuleID + "");
        theTable.AddLinkColumn(linkColumn1);

        DataTableLinkColumn linkColumn2 = new DataTableLinkColumn("adm_generic_remove.aspx", "Delete", "STATUS=1;STATUS=3");
        linkColumn2.AddQueryStringValue("id", "field=id");
        linkColumn2.AddQueryStringValue("table", "groups_device_families_limitation_modules");
        linkColumn2.AddQueryStringValue("confirm", "true");
        linkColumn2.AddQueryStringValue("main_menu", "2");
        linkColumn2.AddQueryStringValue("sub_menu", "3");
        linkColumn2.AddQueryStringValue("rep_field", "username");
        linkColumn2.AddQueryStringValue("rep_name", "Username");
        theTable.AddLinkColumn(linkColumn2);


        DataTableLinkColumn linkColumn3 = new DataTableLinkColumn("adm_generic_confirm.aspx", "Confirm", "STATUS=3;STATUS=4");
        linkColumn3.AddQueryStringValue("id", "field=id");
        linkColumn3.AddQueryStringValue("table", "groups_device_families_limitation_modules");
        linkColumn3.AddQueryStringValue("confirm", "true");
        linkColumn3.AddQueryStringValue("main_menu", "2");
        linkColumn3.AddQueryStringValue("sub_menu", "3");
        linkColumn3.AddQueryStringValue("rep_field", "username");
        linkColumn3.AddQueryStringValue("rep_name", "Username");
        theTable.AddLinkColumn(linkColumn3);


        DataTableLinkColumn linkColumn4 = new DataTableLinkColumn("adm_generic_confirm.aspx", "Cancel", "STATUS=3;STATUS=4");
        linkColumn4.AddQueryStringValue("id", "field=id");
        linkColumn4.AddQueryStringValue("table", "groups_device_families_limitation_modules");
        linkColumn4.AddQueryStringValue("confirm", "false");
        linkColumn4.AddQueryStringValue("main_menu", "2");
        linkColumn4.AddQueryStringValue("sub_menu", "3");
        linkColumn4.AddQueryStringValue("rep_field", "username");
        linkColumn4.AddQueryStringValue("rep_name", "Username");
        theTable.AddLinkColumn(linkColumn4);
    }

    public string GetPageContent(string sOrderBy, string sPageNum)
    {
        string sOldOrderBy = string.Empty;
        if (Session["order_by"] != null)
            sOldOrderBy = Session["order_by"].ToString();
        DBTableWebEditor theTable = new DBTableWebEditor(true, true, true, "parent_limit_id=" + Session["limit_module_id"].ToString(), "adm_table_header", "adm_table_cell", "adm_table_alt_cell", "adm_table_link", "adm_table_pager", "adm_table", sOldOrderBy, 50);
        FillTheTableEditor(ref theTable, sOrderBy);

        string sTable = theTable.GetPageHTML(int.Parse(sPageNum), sOrderBy);
        theTable.Finish();
        Session["ContentPage"] = "adm_domain_limitation_modules.aspx";

        theTable = null;
        return sTable;
    }

    public void GetHeader()
    {
        Response.Write(PageUtils.GetPreHeader() + " : Device Families");
    }
}
