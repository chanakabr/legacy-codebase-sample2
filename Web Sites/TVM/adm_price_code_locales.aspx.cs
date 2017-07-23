using CachingProvider.LayeredCache;
using KLogMonitor;
using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using TVinciShared;

public partial class adm_price_code_locales : System.Web.UI.Page
{
    private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

    protected string m_sMenu;
    protected string m_sSubMenu;
    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        if (LoginManager.IsPagePermitted("adm_price_codes.aspx") == false)
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
            m_sMenu = TVinciShared.Menu.GetMainMenu(7, true, ref nMenuID , "adm_price_codes.aspx");
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, false);
            if (Request.QueryString["price_code_id"] != null &&
                Request.QueryString["price_code_id"].ToString() != "")
            {
                Session["price_code_id"] = int.Parse(Request.QueryString["price_code_id"].ToString());
                Int32 nOwnerGroupID = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("price_codes", "group_id", int.Parse(Session["price_code_id"].ToString()) , "pricing_connection").ToString());
                Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();
                if (nLogedInGroupID != nOwnerGroupID && PageUtils.IsTvinciUser() == false)
                {
                    LoginManager.LogoutFromSite("login.html");
                    return;
                }
            }
            else if (Session["price_code_id"] == null || Session["price_code_id"].ToString() == "" || Session["price_code_id"].ToString() == "0")
            {
                LoginManager.LogoutFromSite("index.html");
                return;
            }
        }
    }

    static public bool IsTvinciImpl()
    {
        Int32 nImplID = 0;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery.SetConnectionKey("pricing_connection");
        selectQuery += "select * from groups_modules_implementations where is_active=1 and status=1 and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", LoginManager.GetLoginGroupID());
        selectQuery += "and";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MODULE_ID", "=", 1);
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

    public void GetHeader()
    {
        Response.Write(PageUtils.GetPreHeader() + ":" + ODBCWrapper.Utils.GetTableSingleVal("price_codes", "NAME", int.Parse(Session["price_code_id"].ToString()), "pricing_connection").ToString() + " Locale Parameters ");
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
        theTable.SetConnectionKey("pricing_connection");
        theTable += "select q.is_active,q.DEVICE_NAME as 'Application',q.Country,q.Language,q.PRICE as 'Price',q.CODE3 as 'Currency',q.id,q.status,q.State from (select ml.COUNTRY_CODE as 'Country', ml.LANGUAGE_CODE as 'Language',ml.PRICE,lc.CODE3,ml.DEVICE_NAME,ml.id,ml.status,lcs.description as 'State',ml.is_active from lu_currency lc,lu_content_status lcs,price_codes_locales ml where lc.id=ml.CURRENCY_CD and lcs.id=ml.status and " + PageUtils.GetStatusQueryPart("ml") + "and";
        theTable += ODBCWrapper.Parameter.NEW_PARAM("ml.price_code_id", "=", int.Parse(Session["price_code_id"].ToString()));
        theTable += ")q";
        theTable.AddHiddenField("ID");
        theTable.AddHiddenField("status");
        theTable.AddActivationField("price_codes_locales");
        theTable.AddHiddenField("is_active");
        if (sOrderBy != "")
        {
            theTable += " order by ";
            theTable += sOrderBy;
        }
        theTable.AddTechDetails("price_codes_locales");

        if (LoginManager.IsActionPermittedOnPage("adm_price_codes.aspx", LoginManager.PAGE_PERMISION_TYPE.EDIT))
        {
            DataTableLinkColumn linkColumn1 = new DataTableLinkColumn("adm_price_code_locales_new.aspx", "Edit", "");
            linkColumn1.AddQueryStringValue("price_codes_locale_id", "field=id");
            theTable.AddLinkColumn(linkColumn1);
        }
        if (LoginManager.IsActionPermittedOnPage("adm_price_codes.aspx", LoginManager.PAGE_PERMISION_TYPE.REMOVE))
        {
            DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_remove.aspx", "Delete", "STATUS=1;STATUS=3");
            linkColumn.AddQueryStringValue("id", "field=id");
            linkColumn.AddQueryStringValue("table", "price_codes_locales");
            linkColumn.AddQueryStringValue("db", "pricing_connection");
            linkColumn.AddQueryStringValue("confirm", "true");
            linkColumn.AddQueryStringValue("main_menu", "7");
            linkColumn.AddQueryStringValue("sub_menu", "1");
            linkColumn.AddQueryStringValue("rep_field", "NAME");
            linkColumn.AddQueryStringValue("rep_name", "שם");
            theTable.AddLinkColumn(linkColumn);
        }

        if (LoginManager.IsActionPermittedOnPage("adm_price_codes.aspx", LoginManager.PAGE_PERMISION_TYPE.PUBLISH))
        {
            DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_confirm.aspx", "Confirm", "STATUS=3;STATUS=4");
            linkColumn.AddQueryStringValue("id", "field=id");
            linkColumn.AddQueryStringValue("table", "price_codes_locales");
            linkColumn.AddQueryStringValue("db", "pricing_connection");
            linkColumn.AddQueryStringValue("confirm", "true");
            linkColumn.AddQueryStringValue("main_menu", "7");
            linkColumn.AddQueryStringValue("sub_menu", "1");
            linkColumn.AddQueryStringValue("rep_field", "NAME");
            linkColumn.AddQueryStringValue("rep_name", "שם");
            theTable.AddLinkColumn(linkColumn);
        }

        if (LoginManager.IsActionPermittedOnPage("adm_price_codes.aspx", LoginManager.PAGE_PERMISION_TYPE.PUBLISH))
        {
            DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_confirm.aspx", "Cancel", "STATUS=3;STATUS=4");
            linkColumn.AddQueryStringValue("id", "field=id");
            linkColumn.AddQueryStringValue("table", "price_codes_locales");
            linkColumn.AddQueryStringValue("db", "pricing_connection");
            linkColumn.AddQueryStringValue("confirm", "false");
            linkColumn.AddQueryStringValue("main_menu", "7");
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
        theTable.SetConnectionKey("pricing_connection");
        FillTheTableEditor(ref theTable, sOrderBy);

        string sTable = theTable.GetPageHTML(int.Parse(sPageNum), sOrderBy, false);
        Session["ContentPage"] = "adm_price_codes.aspx";
        Session["LastContentPage"] = "adm_price_code_locales.aspx?search_save=1";
        Session["order_by"] = sOldOrderBy;
        theTable.Finish();
        theTable = null;
        return sTable;
    }

    public void UpdateOnOffStatus(string theTableName, string sID, string sStatus)
    {
        int groupId = LoginManager.GetLoginGroupID();
        // invalidation keys
        string invalidationKey = LayeredCacheKeys.GetGroupPriceCodesInvalidationKey(groupId);
        if (!CachingProvider.LayeredCache.LayeredCache.Instance.SetInvalidationKey(invalidationKey))
        {
            log.ErrorFormat("Failed to set invalidation key for group price codes. key = {0}", invalidationKey);
        }

        invalidationKey = LayeredCacheKeys.GetPriceCodeInvalidationKey(groupId, int.Parse(sID));
        if (!CachingProvider.LayeredCache.LayeredCache.Instance.SetInvalidationKey(invalidationKey))
        {
            log.ErrorFormat("Failed to set invalidation key for Price code. key = {0}", invalidationKey);
        }
    }
}
