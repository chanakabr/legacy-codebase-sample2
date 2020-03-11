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

public partial class adm_financial_contracts_co_list : System.Web.UI.Page
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
            m_sMenu = TVinciShared.Menu.GetMainMenu(7, true, ref nMenuID, "adm_financial_contracts_co.aspx");
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, false);

            if (Request.QueryString["fr_entity_id"] != null &&
                Request.QueryString["fr_entity_id"].ToString() != "")
            {
                Session["fr_entity_id"] = int.Parse(Request.QueryString["fr_entity_id"].ToString());
                Int32 nOwnerGroupID = int.Parse(PageUtils.GetTableSingleVal("fr_financial_entities", "group_id", int.Parse(Session["fr_entity_id"].ToString())).ToString());
                Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();
                if (nLogedInGroupID != nOwnerGroupID && PageUtils.IsTvinciUser() == false)
                {
                    LoginManager.LogoutFromSite("login.html");
                    return;
                }
            }
            else if (Session["fr_entity_id"] == null || Session["fr_entity_id"].ToString() == "" || Session["fr_entity_id"].ToString() == "0")
            {
                LoginManager.LogoutFromSite("index.html");
                return;
            }
            Session["fr_contract_id"] = null;
        }
    }

    public void GetHeader()
    {
        Response.Write(PageUtils.GetPreHeader() + ":" + PageUtils.GetTableSingleVal("fr_financial_entities", "NAME", int.Parse(Session["fr_entity_id"].ToString())).ToString() + " Contracts ");
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
        theTable += "select q.Name,gbt.NAME as 'Countries rule',q.lic_or_sub as 'License/Subscription',q.code3 as 'Valid for Currency',q.OUT_OF_T as 'Calculated on',q.LEVEL_NUM as 'Level', q.status,q.id,q.Start_date as 'Start date',q.End_date as 'End date' from (select llos.description as 'lic_or_sub',t.LEVEL_NUM,t.Name,lc.Code3,t.status,t.id,CONVERT(VARCHAR(19),t.START_DATE, 120) as 'Start_date',CONVERT(VARCHAR(19),t.END_DATE, 120) as 'End_date',loo.description as 'OUT_OF_T',t.COUNTRIES_RULE_ID from fr_financial_entity_contracts t,lu_currency lc,lu_out_of_type loo,lu_license_or_sub llos ";
        theTable += "where llos.id=t.LICENSE_OR_SUB and loo.id=t.OUT_OF_TYPE and t.status<>2 and lc.id=t.CURRENCY_CD and ";
        theTable += ODBCWrapper.Parameter.NEW_PARAM("t.FINANCIAL_ENTITY_ID", "=", int.Parse(Session["fr_entity_id"].ToString()));
        theTable += "and";
        theTable += ODBCWrapper.Parameter.NEW_PARAM("t.group_id", "=", nGroupID);
        theTable += ")q left join geo_block_types gbt on gbt.id=q.COUNTRIES_RULE_ID";
        if (sOrderBy != "")
        {
            theTable += " order by ";
            theTable += sOrderBy;
        }
        else
            theTable += " order by q.LEVEL_NUM,q.Start_date";
        theTable.AddHiddenField("ID");
        theTable.AddHiddenField("status");
        theTable.AddOrderByColumn("Name", "q.Name");
        theTable.AddOrderByColumn("Start date", "q.Start_date");
        if (LoginManager.IsActionPermittedOnPage("adm_financial_contracts_co.aspx", LoginManager.PAGE_PERMISION_TYPE.EDIT))
        {
            DataTableLinkColumn linkColumn1 = new DataTableLinkColumn("adm_financial_contracts_co_list_new.aspx", "Edit", "");
            linkColumn1.AddQueryStringValue("fr_contract_id", "field=id");
            theTable.AddLinkColumn(linkColumn1);
        }
        if (LoginManager.IsActionPermittedOnPage("adm_financial_contracts_co.aspx", LoginManager.PAGE_PERMISION_TYPE.REMOVE))
        {
            DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_remove.aspx", "Delete", "STATUS=1;STATUS=3");
            linkColumn.AddQueryStringValue("id", "field=id");
            linkColumn.AddQueryStringValue("table", "fr_financial_entity_contracts");
            linkColumn.AddQueryStringValue("confirm", "true");
            linkColumn.AddQueryStringValue("main_menu", "9");
            linkColumn.AddQueryStringValue("sub_menu", "1");
            linkColumn.AddQueryStringValue("rep_field", "NAME");
            linkColumn.AddQueryStringValue("rep_name", "שם");
            theTable.AddLinkColumn(linkColumn);
        }

        if (LoginManager.IsActionPermittedOnPage("adm_financial_contracts_co.aspx", LoginManager.PAGE_PERMISION_TYPE.PUBLISH))
        {
            DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_confirm.aspx", "Confirm", "STATUS=3;STATUS=4");
            linkColumn.AddQueryStringValue("id", "field=id");
            linkColumn.AddQueryStringValue("table", "fr_financial_entity_contracts");
            linkColumn.AddQueryStringValue("confirm", "true");
            linkColumn.AddQueryStringValue("main_menu", "9");
            linkColumn.AddQueryStringValue("sub_menu", "1");
            linkColumn.AddQueryStringValue("rep_field", "NAME");
            linkColumn.AddQueryStringValue("rep_name", "שם");
            theTable.AddLinkColumn(linkColumn);
        }

        if (LoginManager.IsActionPermittedOnPage("adm_financial_contracts_co.aspx", LoginManager.PAGE_PERMISION_TYPE.PUBLISH))
        {
            DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_confirm.aspx", "Cancel", "STATUS=3;STATUS=4");
            linkColumn.AddQueryStringValue("id", "field=id");
            linkColumn.AddQueryStringValue("table", "fr_financial_entity_contracts");
            linkColumn.AddQueryStringValue("confirm", "false");
            linkColumn.AddQueryStringValue("main_menu", "9");
            linkColumn.AddQueryStringValue("sub_menu", "1");
            linkColumn.AddQueryStringValue("rep_field", "NAME");
            linkColumn.AddQueryStringValue("rep_name", "שם");
            theTable.AddLinkColumn(linkColumn);
        }
    }

    public void GetFamilyID()
    {
        string sRet = "";
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select fe.PARENT_ENTITY_ID from fr_financial_entities fe,fr_financial_entity_contracts c where c.FINANCIAL_ENTITY_ID=fe.id and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("fe.id", "=", int.Parse(Session["fr_entity_id"].ToString()));
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
                sRet = selectQuery.Table("query").DefaultView[0].Row["PARENT_ENTITY_ID"].ToString();
        }
            
        selectQuery.Finish();
        selectQuery = null;
        Response.Write(sRet);    
    }

    public string GetPageContent(string sOrderBy, string sPageNum)
    {
        string sOldOrderBy = "";
        if (Session["order_by"] != null)
            sOldOrderBy = Session["order_by"].ToString();

        DBTableWebEditor theTable = new DBTableWebEditor(true, true, true, "", "adm_table_header", "adm_table_cell", "adm_table_alt_cell", "adm_table_link", "adm_table_pager", "adm_table", sOldOrderBy, 50);
        FillTheTableEditor(ref theTable, sOrderBy);

        string sTable = theTable.GetPageHTML(int.Parse(sPageNum), sOrderBy, false);
        Session["ContentPage"] = "adm_financial_contracts_co.aspx";
        Session["LastContentPage"] = "adm_financial_contracts_co_list.aspx?search_save=1";
        Session["order_by"] = sOldOrderBy;
        theTable.Finish();
        theTable = null;
        return sTable;
    }
}
