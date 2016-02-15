using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TVinciShared;

public partial class adm_payment_gateway_payment_method : System.Web.UI.Page
{
    protected string m_sMenu;
    protected string m_sSubMenu;

    protected void Page_Load(object sender, EventArgs e)
    {
        
        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        if (LoginManager.IsPagePermitted("adm_payment_gateway.aspx") == false)
            LoginManager.LogoutFromSite("login.html");
        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;

        if (string.IsNullOrEmpty(Request.QueryString["paymentGW_id"]))
        {   
            Session["paymentGW_id"] = null;            
        }
        if (!IsPostBack)
        {
            Int32 nMenuID = 0;
            m_sMenu = TVinciShared.Menu.GetMainMenu(7, true, ref nMenuID, "adm_payment_gateway.aspx");
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, false);

            if (Request.QueryString["paymentGW_id"] != null && Request.QueryString["paymentGW_id"].ToString() != "")
            {
                Session["paymentGW_id"] = int.Parse(Request.QueryString["paymentGW_id"].ToString());                                
            }           
        }
    }

    public void GetHeader()
    {
        Response.Write(PageUtils.GetPreHeader() + ":" + " Payment Method values");
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
        theTable.SetConnectionKey("billing_connection");
        theTable += "select ID, group_id, status, TYPE as 'Type', NAME as 'Name' ";
        theTable += " from payment_gateway_payment_method ";
        theTable += " where ( status = 1 or status = 4 ) AND " ;
        theTable += ODBCWrapper.Parameter.NEW_PARAM("payment_gateway_id", "=", int.Parse(Session["paymentGW_id"].ToString()));
        theTable += "AND";
        theTable += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", groupID);
        if (sOrderBy != "")
        {
            theTable += " order by ";
            theTable += sOrderBy;
        }
        else
        {
            theTable += " order by id ";
        }

        theTable.AddHiddenField("ID");
        theTable.AddHiddenField("group_id");
        theTable.AddHiddenField("status");

        if (LoginManager.IsActionPermittedOnPage("adm_payment_gateway.aspx", LoginManager.PAGE_PERMISION_TYPE.EDIT))
        {
            DataTableLinkColumn linkColumn1 = new DataTableLinkColumn("adm_payment_gateway_payment_method_new.aspx", "Edit", "");
            linkColumn1.AddQueryStringValue("payment_gateway_payment_method_id", "field=id");
            theTable.AddLinkColumn(linkColumn1);
        }
        if (LoginManager.IsActionPermittedOnPage("adm_payment_gateway.aspx", LoginManager.PAGE_PERMISION_TYPE.REMOVE))
        {
            DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_remove.aspx", "Delete", "STATUS=1;STATUS=3");
            linkColumn.AddQueryStringValue("id", "field=id");
            linkColumn.AddQueryStringValue("table", "payment_gateway_payment_method");
            linkColumn.AddQueryStringValue("db", "billing_connection");
            linkColumn.AddQueryStringValue("confirm", "true");
            linkColumn.AddQueryStringValue("main_menu", "7");
            linkColumn.AddQueryStringValue("sub_menu", "1");
            linkColumn.AddQueryStringValue("rep_field", "NAME");
            linkColumn.AddQueryStringValue("rep_name", "ùí");
            theTable.AddLinkColumn(linkColumn);
        }

        if (LoginManager.IsActionPermittedOnPage("adm_payment_gateway.aspx", LoginManager.PAGE_PERMISION_TYPE.PUBLISH))
        {
            DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_confirm.aspx", "Confirm", "STATUS=3;STATUS=4");
            linkColumn.AddQueryStringValue("id", "field=id");
            linkColumn.AddQueryStringValue("table", "payment_gateway_payment_method");
            linkColumn.AddQueryStringValue("db", "billing_connection");
            linkColumn.AddQueryStringValue("confirm", "true");
            linkColumn.AddQueryStringValue("main_menu", "7");
            linkColumn.AddQueryStringValue("sub_menu", "1");
            linkColumn.AddQueryStringValue("rep_field", "NAME");
            linkColumn.AddQueryStringValue("rep_name", "ùí");
            theTable.AddLinkColumn(linkColumn);
        }

        if (LoginManager.IsActionPermittedOnPage("adm_payment_gateway.aspx", LoginManager.PAGE_PERMISION_TYPE.PUBLISH))
        {
            DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_confirm.aspx", "Cancel", "STATUS=3;STATUS=4");
            linkColumn.AddQueryStringValue("id", "field=id");
            linkColumn.AddQueryStringValue("table", "payment_gateway_payment_method");
            linkColumn.AddQueryStringValue("db", "billing_connection");
            linkColumn.AddQueryStringValue("confirm", "false");
            linkColumn.AddQueryStringValue("main_menu", "7");
            linkColumn.AddQueryStringValue("sub_menu", "1");
            linkColumn.AddQueryStringValue("rep_field", "NAME");
            linkColumn.AddQueryStringValue("rep_name", "ùí");
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
        Session["ContentPage"] = "adm_payment_gateway.aspx";
        Session["LastContentPage"] = "adm_payment_gateway.aspx?search_save=1";
        Session["order_by"] = sOldOrderBy;
        theTable.Finish();
        theTable = null;
        return sTable;
    }
}