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
using KLogMonitor;
using TVinciShared;

public partial class adm_tvp_pages : System.Web.UI.Page
{
    private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
    protected string m_sMenu;
    protected string m_sSubMenu;

    protected void Page_Load(object sender, EventArgs e)
    {
        Session["tvp_profile_id"] = null;
        if (Request.QueryString["page_type"] != null)
            Session["page_type"] = int.Parse(Request.QueryString["page_type"].ToString());
        if (Request.QueryString["platform"] != null)
            Session["platform"] = int.Parse(Request.QueryString["platform"].ToString());
        if (Session["page_type"] == null)
        {
            LoginManager.LogoutFromSite("login.html");
            return;
        }

        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        if (LoginManager.IsPagePermitted("adm_tvp_pages.aspx?page_type=" + Session["page_type"].ToString() + "&platform=" + Session["platform"].ToString()) == false)
            LoginManager.LogoutFromSite("login.html");
        if (AMS.Web.RemoteScripting.InvokeMethod(this))
        {
            // Response.Expires = -1;
            return;
        }
        if (!IsPostBack)
        {
            Int32 nMenuID = 0;
            m_sMenu = TVinciShared.Menu.GetMainMenu(14, true, ref nMenuID, "adm_tvp_pages.aspx?page_type=" + Session["page_type"].ToString() + "&platform=" + Session["platform"].ToString());
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 3, false);
            if (Request.QueryString["search_save"] != null)
                Session["search_save"] = "1";
            else
                Session["search_save"] = null;
        }
        // Response.Expires = -1;
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
        theTable.SetConnectionKey("tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString());
        log.Debug("TVP Connection is - tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString());
        theTable += "select lupt.IsViewPage,tps.is_active,tps.id as id,tps.VIRTUAL_NAME as 'Virtual Name',tp_side.NAME as 'Side Profile',tp_bot.NAME as 'Bottom Profile', tps.status,lcs.description as 'State' from lu_page_types lupt,tvp_pages_structure tps,lu_content_status lcs,tvp_profiles tp_side,tvp_profiles tp_bot where tp_side.id=tps.SIDE_PROFILE_ID and tp_bot.id=tps.BOTTOM_PROFILE_ID and lcs.id=tps.status and tps.status<>2 and lupt.id=tps.PAGE_TYPE and ";
        theTable += ODBCWrapper.Parameter.NEW_PARAM("tps.PAGE_TYPE", "=", int.Parse(Session["page_type"].ToString()));
        theTable += " and ";
        theTable += ODBCWrapper.Parameter.NEW_PARAM("tps.GROUP_ID", "=", nGroupID);
        if (Session["search_on_off"] != null && Session["search_on_off"].ToString() != "" && Session["search_on_off"].ToString() != "-1")
        {
            theTable += " and ";
            theTable += ODBCWrapper.Parameter.NEW_PARAM("tps.is_active", "=", int.Parse(Session["search_on_off"].ToString()));
        }
        if (Session["search_free"] != null && Session["search_free"].ToString() != "")
        {
            string sLike = "like(N'%" + Session["search_free"].ToString() + "%')";
            theTable += " and (";
            theTable += "tps.virtual_name " + sLike + " OR tps.ID in (select PAGE_STRUCTURE_ID from tvp_pages_texts where (NAME " + sLike + " OR DESCRIPTION " + sLike + ")) ";
            theTable += ")";
        }
        if (sOrderBy != "")
        {
            theTable += " order by ";
            theTable += sOrderBy;
        }
        else
            theTable += " order by tps.id desc";
        theTable.AddHiddenField("ID");
        theTable.AddHiddenField("status");
        theTable.AddHiddenField("IsViewPage");
        theTable.AddActivationField("tvp_pages_structure");
        theTable.AddHiddenField("is_active");
        theTable.AddTechDetails("tvp_pages_structure");

        {
            DataTableLinkColumn linkColumn1 = new DataTableLinkColumn("adm_tvp_galleries.aspx", "Main Galleries", "");
            linkColumn1.AddQueryCounterValue("select count(*) as val from tvp_galleries g where g.LOCATION_ID=4 and g.status=1 and g.is_active=1 and g.PAGE_STRUCTURE_ID=", "field=id");
            linkColumn1.AddQueryStringValue("tvp_page_id", "field=id");
            linkColumn1.AddQueryStringValue("page_location", "4");
            theTable.AddLinkColumn(linkColumn1);
        }

        {
            DataTableLinkColumn linkColumn1 = new DataTableLinkColumn("adm_tvp_galleries.aspx", "Top Galleries", "");
            linkColumn1.AddQueryCounterValue("select count(*) as val from tvp_galleries g where g.LOCATION_ID=1 and g.status=1 and g.is_active=1 and g.PAGE_STRUCTURE_ID=", "field=id");
            linkColumn1.AddQueryStringValue("tvp_page_id", "field=id");
            linkColumn1.AddQueryStringValue("page_location", "1");
            theTable.AddLinkColumn(linkColumn1);
        }

        if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.EDIT))
        {
            DataTableLinkColumn linkColumn1 = new DataTableLinkColumn("adm_tvp_pages_new.aspx", "Edit", "");
            linkColumn1.AddQueryStringValue("tvp_page_id", "field=id");
            theTable.AddLinkColumn(linkColumn1);
        }

        if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.EDIT))
        {
            DataTableLinkColumn linkColumn1 = new DataTableLinkColumn("javascript:openPage", "View Page", "IsViewPage=True");
            linkColumn1.AddQueryStringValue("tvp_page_id", "field=id");
            linkColumn1.AddQueryStringValue("platform", Session["platform"].ToString());
            theTable.AddLinkColumn(linkColumn1);
        }

        if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.EDIT))
        {
            DataTableLinkColumn linkColumn1 = new DataTableLinkColumn("javascript:copyToCB", "Link To ClipBoard", "IsViewPage=True");
            linkColumn1.AddQueryStringValue("tvp_page_id", "field=id");
            theTable.AddLinkColumn(linkColumn1);
        }

        if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.REMOVE))
        {
            DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_remove.aspx", "Delete", "STATUS=1;STATUS=3");
            linkColumn.AddQueryStringValue("id", "field=id");
            linkColumn.AddQueryStringValue("db", "tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString());
            linkColumn.AddQueryStringValue("table", "tvp_pages_structure");
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
            linkColumn.AddQueryStringValue("db", "tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString());
            linkColumn.AddQueryStringValue("table", "tvp_pages_structure");
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
            linkColumn.AddQueryStringValue("table", "tvp_pages_structure");
            linkColumn.AddQueryStringValue("db", "tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString());
            linkColumn.AddQueryStringValue("confirm", "false");
            linkColumn.AddQueryStringValue("main_menu", "14");
            linkColumn.AddQueryStringValue("sub_menu", "2");
            linkColumn.AddQueryStringValue("rep_field", "username");
            linkColumn.AddQueryStringValue("rep_name", "Username");
            theTable.AddLinkColumn(linkColumn);
        }
    }

    public string GetPageContent(string sOrderBy, string sPageNum, string search_free, string search_on_off)
    {
        try
        {
            if (search_on_off != "-1")
                Session["search_on_off"] = search_on_off;
            else if (Session["search_save"] == null)
                Session["search_on_off"] = "";

            if (search_free != "")
                Session["search_free"] = search_free.Replace("'", "''");
            else if (Session["search_save"] == null)
                Session["search_free"] = "";

            string sOldOrderBy = "";
            if (Session["order_by"] != null)
                sOldOrderBy = Session["order_by"].ToString();
            DBTableWebEditor theTable = new DBTableWebEditor(true, true, true, "", "adm_table_header", "adm_table_cell", "adm_table_alt_cell", "adm_table_link", "adm_table_pager", "adm_table", sOldOrderBy, 50);
            Int32 nGroupID = LoginManager.GetLoginGroupID();
            theTable.SetConnectionKey("tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString());
            FillTheTableEditor(ref theTable, sOrderBy);

            string sTable = theTable.GetPageHTML(int.Parse(sPageNum), sOrderBy);
            Session["ContentPage"] = "adm_tvp_pages.aspx";
            theTable.Finish();
            theTable = null;
            Response.Expires = -1;
            return sTable;
        }
        catch (Exception ex)
        {
            log.Error("sdfsdF - " + ex.Message + "||" + ex.StackTrace, ex);
            return "";
        }
    }


    public void GetHeader()
    {
        Int32 nGroupID = LoginManager.GetLoginGroupID();
        string sConn = "tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString();
        object oTypeName = ODBCWrapper.Utils.GetTableSingleVal("lu_page_types", "DESCRIPTION", int.Parse(Session["page_type"].ToString()), sConn);
        string sTypeName = "";
        if (oTypeName != DBNull.Value)
            sTypeName = oTypeName.ToString();
        Response.Write(PageUtils.GetPreHeader() + ": Pages (" + sTypeName + ")");
    }
}
