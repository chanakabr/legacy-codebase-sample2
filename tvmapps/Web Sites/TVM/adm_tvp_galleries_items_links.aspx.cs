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

public partial class adm_tvp_galleries_items_links : System.Web.UI.Page
{
    protected string m_sMenu;
    protected string m_sSubMenu;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        if (Session["tvp_profile_id"] != null)
            if (LoginManager.IsPagePermitted("adm_tvp_profiles.aspx?profile_loc=" + Session["profile_loc"].ToString() + "&platform=" + Session["platform"].ToString()) == false)
                LoginManager.LogoutFromSite("login.html");
        if (Session["tvp_page_id"] != null)
            if (LoginManager.IsPagePermitted("adm_tvp_pages.aspx?tvp_page_id=" + Session["tvp_page_id"].ToString()) == false)
                LoginManager.LogoutFromSite("login.html");
        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        if (!IsPostBack)
        {
            if (Request.QueryString["link_type"] != null &&
               Request.QueryString["link_type"].ToString() != "")
            {
                Session["link_type"] = int.Parse(Request.QueryString["link_type"].ToString());
            }
            else
            {
                if (Session["link_type"] == null || Session["link_type"].ToString() == "" || Session["link_type"].ToString() == "0")
                {
                    LoginManager.LogoutFromSite("login.html");
                    return;
                }
            }

            if (Request.QueryString["gallery_id"] != null &&
               Request.QueryString["gallery_id"].ToString() != "")
            {
                Session["gallery_id"] = int.Parse(Request.QueryString["gallery_id"].ToString());
            }
            else
            {
                if (Session["gallery_id"] == null || Session["gallery_id"].ToString() == "" || Session["gallery_id"].ToString() == "0")
                {
                    LoginManager.LogoutFromSite("login.html");
                    return;
                }
            }
            Int32 nMenuID = 0;
            if (Session["tvp_profile_id"] != null)
                m_sMenu = TVinciShared.Menu.GetMainMenu(14, true, ref nMenuID, "adm_tvp_profiles.aspx?profile_loc=" + Session["profile_loc"].ToString() + "&platform=" + Session["platform"].ToString());
            if (Session["tvp_page_id"] != null)
                m_sMenu = TVinciShared.Menu.GetMainMenu(14, true, ref nMenuID, "adm_tvp_pages.aspx?page_type=" + Session["page_type"].ToString() + "&platform=" + Session["platform"].ToString());
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 3, false);
            if (Request.QueryString["search_save"] != null)
                Session["search_save"] = "1";
            else
                Session["search_save"] = null;
        }
    }

    public void GetBackLink()
    {
        string sBack = "adm_tvp_galleries.aspx?";
        if (Session["tvp_profile_id"] != null)
            sBack += "tvp_profile_id=" + Session["tvp_profile_id"].ToString();
        if (Session["tvp_page_id"] != null)
            sBack += "tvp_page_id=" + Session["tvp_page_id"].ToString();
        Response.Write(sBack);
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
        theTable += "select tgb.order_num,tgb.is_active,tgb.id as id,tgb.VIRTUAL_NAME as 'Virtual Name',tgb.status,lcs.description as 'State' from tvp_galleries_buttons tgb,lu_content_status lcs where lcs.id=tgb.status and tgb.status<>2 and ";
        theTable += ODBCWrapper.Parameter.NEW_PARAM("tgb.group_id", "=", nGroupID);
        theTable += " and ";
        theTable += ODBCWrapper.Parameter.NEW_PARAM("tgb.TVP_GALLERY_ID", "=", int.Parse(Session["gallery_id"].ToString()));
        theTable += " and ";
        theTable += ODBCWrapper.Parameter.NEW_PARAM("tgb.BUTTON_TYPE", "=", int.Parse(Session["link_type"].ToString()));
        if (sOrderBy != "")
        {
            theTable += " order by ";
            theTable += sOrderBy;
        }
        else
            theTable += " order by tgb.order_num";
        theTable.AddHiddenField("ID");
        theTable.AddHiddenField("status");
        theTable.AddActivationField("tvp_galleries_buttons");
        theTable.AddHiddenField("is_active");
        theTable.AddTechDetails("tvp_galleries_buttons");
        theTable.AddOrderNumField("tvp_galleries_buttons", "id", "order_num", "Order Number");
        theTable.AddHiddenField("order_num");
        string sPermited = "";
        if (Session["tvp_profile_id"] != null)
            sPermited = "adm_tvp_profiles.aspx?profile_loc=" + Session["profile_loc"].ToString() + "&platform=" + Session["platform"].ToString();
        if (Session["tvp_page_id"] != null)
            sPermited = "adm_tvp_pages.aspx?tvp_page_id=" + Session["tvp_page_id"].ToString();
        if (LoginManager.IsActionPermittedOnPage(sPermited, LoginManager.PAGE_PERMISION_TYPE.EDIT))
        {
            DataTableLinkColumn linkColumn1 = new DataTableLinkColumn("adm_tvp_galleries_items_links_new.aspx", "Edit", "");
            linkColumn1.AddQueryStringValue("link_id", "field=id");
            theTable.AddLinkColumn(linkColumn1);
        }

        if (LoginManager.IsActionPermittedOnPage(sPermited, LoginManager.PAGE_PERMISION_TYPE.REMOVE))
        {
            DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_remove.aspx", "Delete", "STATUS=1;STATUS=3");
            linkColumn.AddQueryStringValue("id", "field=id");
            linkColumn.AddQueryStringValue("db", "tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString());
            linkColumn.AddQueryStringValue("table", "tvp_galleries_buttons");
            linkColumn.AddQueryStringValue("confirm", "true");
            linkColumn.AddQueryStringValue("main_menu", "14");
            linkColumn.AddQueryStringValue("sub_menu", "2");
            linkColumn.AddQueryStringValue("rep_field", "username");
            linkColumn.AddQueryStringValue("rep_name", "Username");
            theTable.AddLinkColumn(linkColumn);
        }

        if (LoginManager.IsActionPermittedOnPage(sPermited, LoginManager.PAGE_PERMISION_TYPE.PUBLISH))
        {
            DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_confirm.aspx", "Confirm", "STATUS=3;STATUS=4");
            linkColumn.AddQueryStringValue("id", "field=id");
            linkColumn.AddQueryStringValue("db", "tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString());
            linkColumn.AddQueryStringValue("table", "tvp_galleries_buttons");
            linkColumn.AddQueryStringValue("confirm", "true");
            linkColumn.AddQueryStringValue("main_menu", "14");
            linkColumn.AddQueryStringValue("sub_menu", "2");
            linkColumn.AddQueryStringValue("rep_field", "username");
            linkColumn.AddQueryStringValue("rep_name", "Username");
            theTable.AddLinkColumn(linkColumn);
        }

        if (LoginManager.IsActionPermittedOnPage(sPermited, LoginManager.PAGE_PERMISION_TYPE.REMOVE))
        {
            DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_confirm.aspx", "Cancel", "STATUS=3;STATUS=4");
            linkColumn.AddQueryStringValue("id", "field=id");
            linkColumn.AddQueryStringValue("table", "tvp_galleries_buttons");
            linkColumn.AddQueryStringValue("db", "tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString());
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
        Int32 nGroupID = LoginManager.GetLoginGroupID();
        theTable.SetConnectionKey("tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString());
        FillTheTableEditor(ref theTable, sOrderBy);

        string sTable = theTable.GetPageHTML(int.Parse(sPageNum), sOrderBy);
        if (Session["tvp_profile_id"] != null)
            Session["ContentPage"] = "adm_tvp_profiles.aspx";
        if (Session["tvp_page_id"] != null)
            Session["ContentPage"] = "adm_tvp_pages.aspx";
        theTable.Finish();
        theTable = null;
        return sTable;
    }

    public void GetHeader()
    {
        Int32 nGroupID = LoginManager.GetLoginGroupID();
        string sHeader = ": Gallery ";
        if (Session["link_type"].ToString() == "1")
            sHeader += "Tabs (";
        if (Session["link_type"].ToString() == "2")
            sHeader += "Links (";
        sHeader += ODBCWrapper.Utils.GetTableSingleVal("tvp_galleries", "VIRTUAL_NAME", int.Parse(Session["gallery_id"].ToString()), "tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString()).ToString();
        sHeader += ") : ";
        Response.Write(PageUtils.GetPreHeader() + sHeader);
    }
}
