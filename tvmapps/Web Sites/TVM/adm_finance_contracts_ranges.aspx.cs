using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TVinciShared;

public partial class adm_finance_contracts_ranges : System.Web.UI.Page
{
    protected string m_sMenu;
    protected string m_sSubMenu;

    protected void Page_Load(object sender, EventArgs e)
    {
         if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        if (LoginManager.IsPagePermitted("adm_finance_contracts_ranges.aspx") == false)
            LoginManager.LogoutFromSite("login.html");
        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        if (!IsPostBack)
        {
            Int32 nMenuID = 0;
            m_sMenu = TVinciShared.Menu.GetMainMenu(7, true, ref nMenuID, "adm_finance_contracts_ranges.aspx");
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, false);

         
        }

        Session["fr_contract_range_id"] = null;
    }


    public void GetHeader()
    {
        Response.Write(PageUtils.GetPreHeader() + ":" + "Contracts Ranges");
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
        theTable += "select ID, MIN, MAX, RANGE_TYPE as RANGE_TYPE_CODE , START_FROM as START_FROM_CODE, NAME, STATUS, IS_ACTIVE, group_id, ";
        theTable += SelectRangeType();
        theTable += SelectStartFrom();        
        theTable += " from fr_financial_contracts_ranges fr ";
        theTable += "where status = 1 and is_active = 1 and ";
        theTable += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
        theTable += "order by id";

        //theTable.AddHiddenField("ID");
        theTable.AddHiddenField("STATUS");
        theTable.AddHiddenField("IS_ACTIVE");
        theTable.AddHiddenField("GROUP_ID");
        //theTable.AddOrderByColumn("Name", "NAME");

        //theTable.AddHTMLField("MIN");
        //theTable.AddHTMLField("MAX");

        theTable.AddHiddenField("START_FROM_CODE");
        theTable.AddHiddenField("RANGE_TYPE_CODE");



        if (LoginManager.IsActionPermittedOnPage("adm_finance_contracts_ranges.aspx", LoginManager.PAGE_PERMISION_TYPE.EDIT))
        {
            DataTableLinkColumn linkColumn1 = new DataTableLinkColumn("adm_finance_contracts_ranges_new.aspx", "Edit", "");
            linkColumn1.AddQueryStringValue("fr_contract_range_id", "field=id");
            theTable.AddLinkColumn(linkColumn1);
        }
        if (LoginManager.IsActionPermittedOnPage("adm_finance_contracts_ranges.aspx", LoginManager.PAGE_PERMISION_TYPE.REMOVE))
        {
            DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_remove.aspx", "Delete", "STATUS=1;STATUS=3");
            linkColumn.AddQueryStringValue("id", "field=id");
            linkColumn.AddQueryStringValue("table", "fr_financial_contracts_ranges");
            linkColumn.AddQueryStringValue("confirm", "true");
            linkColumn.AddQueryStringValue("main_menu", "9");
            linkColumn.AddQueryStringValue("sub_menu", "1");
            linkColumn.AddQueryStringValue("rep_field", "NAME");
            linkColumn.AddQueryStringValue("rep_name", "שם");
            theTable.AddLinkColumn(linkColumn);
        }

        if (LoginManager.IsActionPermittedOnPage("adm_finance_contracts_ranges.aspx", LoginManager.PAGE_PERMISION_TYPE.PUBLISH))
        {
            DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_confirm.aspx", "Confirm", "STATUS=3;STATUS=4");
            linkColumn.AddQueryStringValue("id", "field=id");
            linkColumn.AddQueryStringValue("table", "fr_financial_contracts_ranges");
            linkColumn.AddQueryStringValue("confirm", "true");
            linkColumn.AddQueryStringValue("main_menu", "9");
            linkColumn.AddQueryStringValue("sub_menu", "1");
            linkColumn.AddQueryStringValue("rep_field", "NAME");
            linkColumn.AddQueryStringValue("rep_name", "שם");
            theTable.AddLinkColumn(linkColumn);
        }

        if (LoginManager.IsActionPermittedOnPage("adm_finance_contracts_ranges.aspx", LoginManager.PAGE_PERMISION_TYPE.PUBLISH))
        {
            DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_confirm.aspx", "Cancel", "STATUS=3;STATUS=4");
            linkColumn.AddQueryStringValue("id", "field=id");
            linkColumn.AddQueryStringValue("table", "fr_financial_contracts_ranges");
            linkColumn.AddQueryStringValue("confirm", "false");
            linkColumn.AddQueryStringValue("main_menu", "9");
            linkColumn.AddQueryStringValue("sub_menu", "1");
            linkColumn.AddQueryStringValue("rep_field", "NAME");
            linkColumn.AddQueryStringValue("rep_name", "שם");
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
        //Session["ContentPage"] = "adm_financial_contracts_co.aspx";
        //Session["LastContentPage"] = "adm_financial_contracts_co_list.aspx?search_save=1";
        Session["order_by"] = sOldOrderBy;
        theTable.Finish();
        theTable = null;
        return sTable;
    }


    private string SelectRangeType()
    {
        string s = " (select lurt.name from lu_financial_range_type lurt where lurt.id = fr.RANGE_TYPE) as 'Range Type', ";
        return s;
    }

    private string SelectStartFrom()
    {
        string s = " (select lusf.name from lu_financial_ranges_start_from lusf where lusf.id = fr.START_FROM) as 'Start From' ";
        return s;
    }

  
}