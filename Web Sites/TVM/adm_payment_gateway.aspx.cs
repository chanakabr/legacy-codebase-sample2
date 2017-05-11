using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TVinciShared;

public partial class adm_payment_gateway : System.Web.UI.Page
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
            m_sMenu = TVinciShared.Menu.GetMainMenu(6, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, false);
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
        Int32 groupID = LoginManager.GetLoginGroupID();
        

        theTable += "select pg.id, pg.name, pg.group_id, pg.is_active, pg.status, pg.adapter_url as 'adapter url'";
        theTable += ",case gp.[DEFAULT_PAYMENT_GATEWAY] when pg.id then 'YES' else 'NO' end as 'is default'";
        theTable += ",pg.external_identifier as 'external id'";        
        theTable += "from payment_gateway pg ";
        theTable += "left join groups_parameters gp on pg.group_id = gp.group_id ";      
        theTable += "where ";
        theTable += ODBCWrapper.Parameter.NEW_PARAM("pg.group_id", "=", groupID);
        theTable += " and ";
        theTable += ODBCWrapper.Parameter.NEW_PARAM("pg.status", "=", 1);
        theTable += " order by id ";
        theTable.SetConnectionKey("billing_connection");

        theTable.AddHiddenField("is_active");
        theTable.AddHiddenField("status");
        theTable.AddHiddenField("group_id");
        theTable.AddActivationField("is_active");
        theTable.AddActivationField("payment_gateway");

        DataTableLinkColumn linkColumnKeParams = new DataTableLinkColumn("adm_payment_gateway_config.aspx", "params", "");
        linkColumnKeParams.AddQueryStringValue("paymentGW_id", "field=id");
        linkColumnKeParams.AddQueryCounterValue("select count(*) as val from payment_gateway_config where ( status=1 or status = 4 )  and payment_gateway_id=", "field=id");
        theTable.AddLinkColumn(linkColumnKeParams);

        DataTableLinkColumn linkColumnKePaymentMethods = new DataTableLinkColumn("adm_payment_gateway_payment_method.aspx", "payment methods", "");
        linkColumnKePaymentMethods.AddQueryStringValue("paymentGW_id", "field=id");
        linkColumnKePaymentMethods.AddQueryCounterValue("select count(*) as val from payment_gateway_payment_method where status=1 and payment_gateway_id=", "field=id");
        theTable.AddLinkColumn(linkColumnKePaymentMethods);
        

        if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.EDIT))
        {
            DataTableLinkColumn linkColumn1 = new DataTableLinkColumn("adm_payment_gateway_new.aspx", "Edit", "");
            linkColumn1.AddQueryStringValue("paymentGW_id", "field=id");
            theTable.AddLinkColumn(linkColumn1);
        }

        if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.REMOVE))
        {
            DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_remove.aspx", "Delete", "STATUS=1;STATUS=3");
            linkColumn.AddQueryStringValue("id", "field=id");
            linkColumn.AddQueryStringValue("table", "payment_gateway");
            linkColumn.AddQueryStringValue("db", "billing_connection");
            linkColumn.AddQueryStringValue("confirm", "true");
            linkColumn.AddQueryStringValue("main_menu", "6");
            linkColumn.AddQueryStringValue("sub_menu", "1");
            linkColumn.AddQueryStringValue("rep_field", "username");
            linkColumn.AddQueryStringValue("rep_name", "Username");
            theTable.AddLinkColumn(linkColumn);
        }

        if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.PUBLISH))
        {
            DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_confirm.aspx", "Confirm", "STATUS=3;STATUS=4");
            linkColumn.AddQueryStringValue("id", "field=id");
            linkColumn.AddQueryStringValue("table", "payment_gateway");
            linkColumn.AddQueryStringValue("db", "billing_connection");
            linkColumn.AddQueryStringValue("confirm", "true");
            linkColumn.AddQueryStringValue("main_menu", "6");
            linkColumn.AddQueryStringValue("sub_menu", "1");
            linkColumn.AddQueryStringValue("rep_field", "username");
            linkColumn.AddQueryStringValue("rep_name", "Username");
            theTable.AddLinkColumn(linkColumn);
        }

        if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.REMOVE))
        {
            DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_confirm.aspx", "Cancel", "STATUS=3;STATUS=4");
            linkColumn.AddQueryStringValue("id", "field=id");
            linkColumn.AddQueryStringValue("table", "payment_gateway");
            linkColumn.AddQueryStringValue("db", "billing_connection");
            linkColumn.AddQueryStringValue("confirm", "false");
            linkColumn.AddQueryStringValue("main_menu", "6");
            linkColumn.AddQueryStringValue("sub_menu", "1");
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

        string sTable = theTable.GetPageHTML(int.Parse(sPageNum), sOrderBy);
        theTable.Finish();
        theTable = null;
        return sTable;
    }

    public void GetHeader()
    {
        Response.Write(PageUtils.GetPreHeader() + ": " + "Payment Gateway");
    }
}