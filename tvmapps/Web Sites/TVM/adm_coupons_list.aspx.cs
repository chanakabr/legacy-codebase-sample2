using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using CachingProvider.LayeredCache;
using Phx.Lib.Log;
using TVinciShared;

public partial class adm_coupons_list : System.Web.UI.Page
{
    protected string m_sMenu;
    protected string m_sSubMenu;

    private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

    static public bool IsTvinciImpl()
    {
        Int32 nImplID = 0;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery.SetConnectionKey("pricing_connection");
        selectQuery += "select * from groups_modules_implementations where is_active=1 and status=1 and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", LoginManager.GetLoginGroupID());
        selectQuery += "and";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MODULE_ID", "=", 3);
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
        if (LoginManager.IsPagePermitted("adm_coupons_groups.aspx") == false)
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
            m_sMenu = TVinciShared.Menu.GetMainMenu(14, true, ref nMenuID, "adm_coupons_groups.aspx");
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 2, false);
            if (Request.QueryString["coupon_group_id"] != null &&
                Request.QueryString["coupon_group_id"].ToString() != "")
            {
                Session["coupon_group_id"] = int.Parse(Request.QueryString["coupon_group_id"].ToString());
                Int32 nOwnerGroupID = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("coupons_groups", "group_id", int.Parse(Session["coupon_group_id"].ToString()), "pricing_connection").ToString());
                Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();
                if (nLogedInGroupID != nOwnerGroupID && PageUtils.IsTvinciUser() == false)
                {
                    LoginManager.LogoutFromSite("login.html");
                    return;
                }
            }
            else
                Session["coupon_group_id"] = 0;
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
        theTable.SetConnectionKey("pricing_connection");
        theTable += "select c.is_active,c.id as id,c.status,c.code as 'Code',c.USE_COUNT as 'Used Count',lcs.description as 'State' from coupons c,lu_content_status lcs where lcs.id=c.status and c.status<>2 and ";
        theTable += ODBCWrapper.Parameter.NEW_PARAM("c.group_id", "=", nGroupID);
        theTable += " and ";
        theTable += ODBCWrapper.Parameter.NEW_PARAM("c.coupon_group_id", "=", int.Parse(Session["coupon_group_id"].ToString()));

        if (Session["search_free_cl"] != null && Session["search_free_cl"].ToString() != "")
        {
            string sLike = "like(N'%" + Session["search_free_cl"].ToString() + "%')";
            theTable += " and c.code " + sLike;
        }
        if (Session["search_on_off_cl"] != null && Session["search_on_off_cl"].ToString() != "" && Session["search_on_off_cl"].ToString() != "-1")
        {
            theTable += " and ";
            theTable += ODBCWrapper.Parameter.NEW_PARAM("c.is_active", "=", int.Parse(Session["search_on_off_cl"].ToString()));
        }

        if (sOrderBy != "")
        {
            theTable += " order by ";
            theTable += sOrderBy;
        }
        theTable.AddHiddenField("ID");
        theTable.AddHiddenField("status");
        theTable.AddOrderByColumn("Code", "c.CODE");
        theTable.AddActivationField("coupons");
        theTable.AddHiddenField("is_active");

        if (LoginManager.IsActionPermittedOnPage("adm_coupons_groups.aspx", LoginManager.PAGE_PERMISION_TYPE.EDIT))
        {
            DataTableLinkColumn linkColumn1 = new DataTableLinkColumn("adm_coupons_list_new.aspx", "Edit", "");
            linkColumn1.AddQueryStringValue("coupon_id", "field=id");
            theTable.AddLinkColumn(linkColumn1);
        }

        if (LoginManager.IsActionPermittedOnPage("adm_coupons_groups.aspx", LoginManager.PAGE_PERMISION_TYPE.REMOVE))
        {
            DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_remove.aspx", "Delete", "STATUS=1;STATUS=3");
            linkColumn.AddQueryStringValue("id", "field=id");
            linkColumn.AddQueryStringValue("db", "pricing_connection");
            linkColumn.AddQueryStringValue("table", "coupons");
            linkColumn.AddQueryStringValue("confirm", "true");
            linkColumn.AddQueryStringValue("main_menu", "14");
            linkColumn.AddQueryStringValue("sub_menu", "2");
            linkColumn.AddQueryStringValue("rep_field", "username");
            linkColumn.AddQueryStringValue("rep_name", "Username");
            theTable.AddLinkColumn(linkColumn);
        }

        if (LoginManager.IsActionPermittedOnPage("adm_coupons_groups.aspx", LoginManager.PAGE_PERMISION_TYPE.PUBLISH))
        {
            DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_confirm.aspx", "Confirm", "STATUS=3;STATUS=4");
            linkColumn.AddQueryStringValue("id", "field=id");
            linkColumn.AddQueryStringValue("db", "pricing_connection");
            linkColumn.AddQueryStringValue("table", "coupons");
            linkColumn.AddQueryStringValue("confirm", "true");
            linkColumn.AddQueryStringValue("main_menu", "14");
            linkColumn.AddQueryStringValue("sub_menu", "2");
            linkColumn.AddQueryStringValue("rep_field", "username");
            linkColumn.AddQueryStringValue("rep_name", "Username");
            theTable.AddLinkColumn(linkColumn);
        }

        if (LoginManager.IsActionPermittedOnPage("adm_coupons_groups.aspx", LoginManager.PAGE_PERMISION_TYPE.REMOVE))
        {
            DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_confirm.aspx", "Cancel", "STATUS=3;STATUS=4");
            linkColumn.AddQueryStringValue("id", "field=id");
            linkColumn.AddQueryStringValue("table", "coupons");
            linkColumn.AddQueryStringValue("db", "pricing_connection");
            linkColumn.AddQueryStringValue("confirm", "false");
            linkColumn.AddQueryStringValue("main_menu", "14");
            linkColumn.AddQueryStringValue("sub_menu", "2");
            linkColumn.AddQueryStringValue("rep_field", "username");
            linkColumn.AddQueryStringValue("rep_name", "Username");
            theTable.AddLinkColumn(linkColumn);
        }

    }

    public string GetPageContent(string sOrderBy, string sPageNum, string search_free_cl, string search_on_off_cl)
    {
        if (search_on_off_cl != "-1")
            Session["search_on_off_cl"] = search_on_off_cl;
        else if (Session["search_save"] == null)
            Session["search_on_off_cl"] = "";

        if (search_free_cl != "")
            Session["search_free_cl"] = search_free_cl.Replace("'", "''");
        else if (Session["search_save"] == null)
            Session["search_free_cl"] = "";

        string sOldOrderBy = "";
        if (Session["order_by"] != null)
            sOldOrderBy = Session["order_by"].ToString();
        DBTableWebEditor theTable = new DBTableWebEditor(true, true, true, "", "adm_table_header", "adm_table_cell", "adm_table_alt_cell", "adm_table_link", "adm_table_pager", "adm_table", sOldOrderBy, 50);
        theTable.SetConnectionKey("pricing_connection");
        FillTheTableEditor(ref theTable, sOrderBy);

        string sTable = theTable.GetPageHTML(int.Parse(sPageNum), sOrderBy);
        Session["ContentPage"] = "adm_coupons_groups.aspx";
        Session["LastContentPage"] = "adm_coupons_list.aspx?coupon_group_id=" + Session["coupon_group_id"].ToString();
        theTable.Finish();
        theTable = null;
        return sTable;
    }

    public void GetHeader()
    {
        string sPC = "";
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery.SetConnectionKey("pricing_connection");
        selectQuery += " select CODE from coupons_groups where ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", int.Parse(Session["coupon_group_id"].ToString()));
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
                sPC = selectQuery.Table("query").DefaultView[0].Row["CODE"].ToString();
        }
        selectQuery.Finish();
        selectQuery = null;
        Response.Write(PageUtils.GetPreHeader() + ": Coupon Group (" + sPC + ") - coupons list");
    }

    public void UpdateOnOffStatus(string theTableName, string sID, string sStatus)
    {
        int groupId = LoginManager.GetLoginGroupID();

        string invalidationKey = LayeredCacheKeys.GetPricingSettingsInvalidationKey(groupId);
        if (!CachingProvider.LayeredCache.LayeredCache.Instance.SetInvalidationKey(invalidationKey))
        {
            log.ErrorFormat("Failed to set pricing settings invalidation key after coupons list status change, {0}, key = {1}", invalidationKey);
        }
    }
}
