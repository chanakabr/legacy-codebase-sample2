using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TVinciShared;
using System.Configuration;

public partial class adm_multi_pricing_plans : System.Web.UI.Page
{
    protected string m_sMenu;
    protected string m_sSubMenu;

    static public bool IsTvinciImpl()
    {
        Int32 nImplID = 0;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery.SetConnectionKey("pricing_connection");
        selectQuery += "select * from groups_modules_implementations where is_active=1 and status=1 and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", LoginManager.GetLoginGroupID());
        selectQuery += "and";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MODULE_ID", "=", 6);
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
            {
                nImplID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["IMPLEMENTATION_ID"].ToString());
            }
        }
        selectQuery.Finish();
        selectQuery = null;
        if (nImplID == 1)
            return true;
        return false;
    }

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
            if (IsTvinciImpl() == false)
            {
                Server.Transfer("adm_module_not_implemented.aspx");
                return;
            }
            Int32 nMenuID = 0;
            Session["mpp"] = null;
            Session["subscription_id"] = null;
            m_sMenu = TVinciShared.Menu.GetMainMenu(14, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 3, false);
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
        theTable.SetConnectionKey("pricing_connection");
        theTable += @"select s.is_active, s.id, s.order_num, s.Name as 'Code', s.id as 'Subscription ID', sn.description as 'Title', sd.description as 'Description', 
	                CONVERT(VARCHAR(10),s.START_DATE, 104) as 'Start date', CONVERT(VARCHAR(10),s.End_DATE, 104) as 'End date', s.CoGuid as 'External ID',
                    case s.type when 1 then 'Base' when 2 then ' AddOn' else 'N/A' end as 'Type', s.status, lcs.description as 'State'
	                from subscriptions s with (nolock)
	                left join lu_yes_no lyn1 with (nolock) on (lyn1.id=s.IS_RECURRING)
	                left join lu_content_status lcs with (nolock) on (lcs.id=s.status)
	                left join subscription_names sn with (nolock) ON (sn.language_code3='" + sGroupLang + @"' and sn.subscription_id=s.id and sn.IS_ACTIVE=1 and sn.STATUS=1)
	                LEFT JOIN subscription_descriptions sd with (nolock) ON (sd.language_code3='" + sGroupLang + @"' and sd.subscription_id=s.id and sd.IS_ACTIVE=1 and sd.STATUS=1)
	                where s.status<>2 and ";
        theTable += ODBCWrapper.Parameter.NEW_PARAM("s.group_id", "=", nGroupID);
        if (sOrderBy != "")
        {
            theTable += " order by ";
            theTable += sOrderBy;
        }
        theTable.AddHiddenField("ID");
        theTable.AddHiddenField("status");
        theTable.AddActivationField("subscriptions", "adm_subscriptions.aspx");
        theTable.AddHiddenField("is_active");
        theTable.AddOrderNumField("subscriptions", "id", "order_num", "Priority");
        /*
        if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.EDIT))
        {
            DataTableLinkColumn linkColumn1 = new DataTableLinkColumn("adm_subscription_names.aspx", "Names", "");
            string sQuery = "select description as 'txt' from subscription_names where group_id=" + LoginManager.GetLoginGroupID().ToString() + " and status=1 and is_active=1 and language_code3='" + sGroupLang + "' and subscription_id=";
            linkColumn1.SetColumnTextDynamicQuery(sQuery);
            linkColumn1.AddQueryStringValue("subscription_id", "field=id");
            theTable.AddLinkColumn(linkColumn1);
        }

        if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.EDIT))
        {
            DataTableLinkColumn linkColumn1 = new DataTableLinkColumn("adm_subscription_descriptions.aspx", "Descriptions", "");
            string sQuery = "select description as 'txt' from subscription_descriptions where group_id=" + LoginManager.GetLoginGroupID().ToString() + " and status=1 and is_active=1 and language_code3='" + sGroupLang + "' and subscription_id=";
            linkColumn1.SetColumnTextDynamicQuery(sQuery);
            linkColumn1.AddQueryStringValue("subscription_id", "field=id");
            theTable.AddLinkColumn(linkColumn1);
        }
        */
        {
            DataTableLinkColumn linkColumn1 = new DataTableLinkColumn("adm_subscription_channels.aspx", "Channels", "");
            linkColumn1.AddQueryStringValue("subscription_id", "field=id");
            linkColumn1.AddQueryStringValue("mpp", "1");
            theTable.AddLinkColumn(linkColumn1);
        }

        {
            DataTableLinkColumn linkColumn1 = new DataTableLinkColumn("adm_subscription_operator.aspx", "Operators", "");
            linkColumn1.AddQueryStringValue("subscription_id", "field=id");
            theTable.AddLinkColumn(linkColumn1);
        }

        {
            DataTableLinkColumn linkColumn1 = new DataTableLinkColumn("adm_subscription_file_types.aspx", "File Types", "");
            linkColumn1.AddQueryStringValue("subscription_id", "field=id");
            linkColumn1.AddQueryStringValue("mpp", "1");
            theTable.AddLinkColumn(linkColumn1);
        }

        {
            DataTableLinkColumn linkColumn1 = new DataTableLinkColumn("adm_subscription_services.aspx", "Services", "");
            linkColumn1.AddQueryStringValue("subscription_id", "field=id");
            linkColumn1.AddQueryStringValue("mpp", "1");
            theTable.AddLinkColumn(linkColumn1);
        }
        {
            DataTableLinkColumn linkColumn1 = new DataTableLinkColumn("adm_subscription_product_codes.aspx", "Product Codes", "");
            linkColumn1.AddQueryStringValue("subscription_id", "field=id");
            linkColumn1.AddQueryCounterValue("select count(*) as val from Pricing.dbo.subscriptions_product_codes where status=1 and is_active=1 and SUBSCRIPTION_ID=", "field=id");
            linkColumn1.AddQueryStringValue("mpp", "1");
            theTable.AddLinkColumn(linkColumn1);
        }

        if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.EDIT))
        {
            DataTableLinkColumn linkColumn1 = new DataTableLinkColumn("adm_multi_pricing_plans_new.aspx", "Edit", "");
            linkColumn1.AddQueryStringValue("subscription_id", "field=id");
            theTable.AddLinkColumn(linkColumn1);
        }

        if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.REMOVE))
        {
            DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_remove.aspx", "Delete", "STATUS=1;STATUS=3");
            linkColumn.AddQueryStringValue("id", "field=id");
            linkColumn.AddQueryStringValue("db", "pricing_connection");
            linkColumn.AddQueryStringValue("table", "subscriptions");
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
            linkColumn.AddQueryStringValue("db", "pricing_connection");
            linkColumn.AddQueryStringValue("table", "subscriptions");
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
            linkColumn.AddQueryStringValue("table", "subscriptions");
            linkColumn.AddQueryStringValue("db", "pricing_connection");
            linkColumn.AddQueryStringValue("confirm", "false");
            linkColumn.AddQueryStringValue("main_menu", "14");
            linkColumn.AddQueryStringValue("sub_menu", "2");
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
        theTable.SetConnectionKey("pricing_connection");
        FillTheTableEditor(ref theTable, sOrderBy);

        string sTable = theTable.GetPageHTML(int.Parse(sPageNum), sOrderBy);

        theTable.Finish();
        theTable = null;
        return sTable;
    }

    public void GetHeader()
    {
        Response.Write(PageUtils.GetPreHeader() + ": Subscriptions");
    }
}