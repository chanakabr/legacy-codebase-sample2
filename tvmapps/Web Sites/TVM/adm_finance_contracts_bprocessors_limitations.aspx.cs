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

public partial class adm_finance_contracts_bprocessors_limitations : System.Web.UI.Page
{
    protected string m_sMenu;
    protected string m_sSubMenu;
    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        if (LoginManager.IsPagePermitted("adm_media.aspx") == false)
            LoginManager.LogoutFromSite("login.html");
        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        if (!IsPostBack)
        {
            Int32 nMenuID = 0;
            m_sMenu = TVinciShared.Menu.GetMainMenu(7, true, ref nMenuID, "adm_finance_contracts_bprocessors.aspx");
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, false);

            if (Request.QueryString["p_id"] != null &&
                Request.QueryString["p_id"].ToString() != "")
                Session["p_id"] = int.Parse(Request.QueryString["p_id"].ToString());
            else if (Session["p_id"] == null || Session["p_id"].ToString() == "" || Session["p_id"].ToString() == "0")
            {
                LoginManager.LogoutFromSite("index.html");
                return;
            }
        }
    }

    public void GetHeader()
    {
        Response.Write(PageUtils.GetPreHeader() + ":" + PageUtils.GetTableSingleVal("lu_billing_processors", "description", int.Parse(Session["p_id"].ToString())).ToString() + " Processors limitations ");
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
        theTable += "select t.Name,t.MAX_FIX_PRICE as 'Maximum monthly amount',t.MIN_FIX_PRICE as 'Minimum monthly amount',lc.Code3 as 'Currency',t.status,t.id,CONVERT(VARCHAR(19),t.START_DATE, 120) as 'Start date',CONVERT(VARCHAR(19),t.END_DATE, 120) as 'End date' from fr_processors_limitations t,lu_currency lc ";
        theTable += "where t.status<>2 and lc.id=t.CURRENCY_CD and ";
        theTable += ODBCWrapper.Parameter.NEW_PARAM("t.PROCESSOR_ID", "=", int.Parse(Session["p_id"].ToString()));
        theTable += "and";
        theTable += ODBCWrapper.Parameter.NEW_PARAM("t.group_id", "=", nGroupID);
        if (sOrderBy != "")
        {
            theTable += " order by ";
            theTable += sOrderBy;
        }
        else
            theTable += " order by t.Start_date";
        theTable.AddHiddenField("ID");
        theTable.AddHiddenField("status");
        theTable.AddOrderByColumn("Name", "t.Name");
        theTable.AddOrderByColumn("Start date", "t.Start_date");
        if (LoginManager.IsActionPermittedOnPage("adm_finance_contracts_bprocessors.aspx", LoginManager.PAGE_PERMISION_TYPE.EDIT))
        {
            DataTableLinkColumn linkColumn1 = new DataTableLinkColumn("adm_finance_contracts_processors_limitations_new.aspx", "Edit", "");
            linkColumn1.AddQueryStringValue("fr_processor_id", "field=id");
            theTable.AddLinkColumn(linkColumn1);
        }
        if (LoginManager.IsActionPermittedOnPage("adm_finance_contracts_bprocessors.aspx", LoginManager.PAGE_PERMISION_TYPE.REMOVE))
        {
            DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_remove.aspx", "Delete", "STATUS=1;STATUS=3");
            linkColumn.AddQueryStringValue("id", "field=id");
            linkColumn.AddQueryStringValue("table", "fr_processors_limitations");
            linkColumn.AddQueryStringValue("confirm", "true");
            linkColumn.AddQueryStringValue("main_menu", "9");
            linkColumn.AddQueryStringValue("sub_menu", "1");
            linkColumn.AddQueryStringValue("rep_field", "NAME");
            linkColumn.AddQueryStringValue("rep_name", "שם");
            theTable.AddLinkColumn(linkColumn);
        }

        if (LoginManager.IsActionPermittedOnPage("adm_finance_contracts_bprocessors.aspx", LoginManager.PAGE_PERMISION_TYPE.PUBLISH))
        {
            DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_confirm.aspx", "Confirm", "STATUS=3;STATUS=4");
            linkColumn.AddQueryStringValue("id", "field=id");
            linkColumn.AddQueryStringValue("table", "fr_processors_limitations");
            linkColumn.AddQueryStringValue("confirm", "true");
            linkColumn.AddQueryStringValue("main_menu", "9");
            linkColumn.AddQueryStringValue("sub_menu", "1");
            linkColumn.AddQueryStringValue("rep_field", "NAME");
            linkColumn.AddQueryStringValue("rep_name", "שם");
            theTable.AddLinkColumn(linkColumn);
        }

        if (LoginManager.IsActionPermittedOnPage("adm_finance_contracts_bprocessors.aspx", LoginManager.PAGE_PERMISION_TYPE.PUBLISH))
        {
            DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_confirm.aspx", "Cancel", "STATUS=3;STATUS=4");
            linkColumn.AddQueryStringValue("id", "field=id");
            linkColumn.AddQueryStringValue("table", "fr_processors_limitations");
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
        Session["ContentPage"] = "adm_finance_contracts_bprocessors.aspx";
        Session["LastContentPage"] = "adm_finance_contracts_bprocessors_limitations.aspxx?search_save=1";
        Session["order_by"] = sOldOrderBy;
        theTable.Finish();
        theTable = null;
        return sTable;
    }
}
