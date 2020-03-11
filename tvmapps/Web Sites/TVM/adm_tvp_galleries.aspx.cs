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

public partial class adm_tvp_galleries : System.Web.UI.Page
{
    protected string m_sMenu;
    protected string m_sSubMenu;
    protected string m_sSubSubMenu;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (AMS.Web.RemoteScripting.InvokeMethod(this))
        {
            Response.Expires = -1;
            return;
        }
        if (Request.QueryString["tvp_profile_id"] != null)
            Session["tvp_profile_id"] = int.Parse(Request.QueryString["tvp_profile_id"].ToString());

        if (Request.QueryString["tvp_page_id"] != null)
            Session["tvp_page_id"] = int.Parse(Request.QueryString["tvp_page_id"].ToString());

        if (Request.QueryString["page_location"] != null)
            Session["page_location"] = int.Parse(Request.QueryString["page_location"].ToString());

        if (Session["tvp_profile_id"] == null && Session["tvp_page_id"] == null)
        {
            LoginManager.LogoutFromSite("login.html");
            return;
        }
        if (Session["tvp_profile_id"] != null && Session["profile_loc"] == null)
        {
            LoginManager.LogoutFromSite("login.html");
            return;
        }

        if (Session["tvp_page_id"] != null && Session["page_location"] == null)
        {
            LoginManager.LogoutFromSite("login.html");
            return;
        }
        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        if (Session["tvp_profile_id"] != null)
            if (LoginManager.IsPagePermitted("adm_tvp_profiles.aspx") == false)
                LoginManager.LogoutFromSite("login.html");

        if (Session["tvp_page_id"] != null)
            if (LoginManager.IsPagePermitted("adm_tvp_pages.aspx") == false)
                LoginManager.LogoutFromSite("login.html");

        if (!IsPostBack)
        {
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

            //System.Collections.SortedList sortedMenu = GetSubMenuList();
            //m_sSubSubMenu = TVinciShared.Menu.GetSubMenu(sortedMenu, -1, false);
        }
        Response.Expires = -1;
    }

    protected System.Collections.SortedList GetSubMenuList()
    {
        Int32 nGroupID = LoginManager.GetLoginGroupID();
        System.Collections.SortedList sortedMenu = new SortedList();
        if (Session["profile_loc"] == null)
            return sortedMenu;

        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery.SetConnectionKey("tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString());
        selectQuery += "select * from tvp_template_channels_gallery_types where is_active=1 and status=1 and ";
        if (Session["profile_loc"].ToString() == "1")
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("IN_TOP", "=", 1);
        if (Session["profile_loc"].ToString() == "2")
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("IN_SIDE", "=", 1);
        if (Session["profile_loc"].ToString() == "3")
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("IN_BOTTOM", "=", 1);
        if (Session["profile_loc"].ToString() == "4")
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("IN_MAIN", "=", 1);
        if (selectQuery.Execute("query" , true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            for (int i = 0; i < nCount; i++)
            {
                string sButton = "+ " + selectQuery.Table("query").DefaultView[i].Row["VIRTUAL_NAME"].ToString();
                string sID = selectQuery.Table("query").DefaultView[i].Row["ID"].ToString();
                sButton += "|";
                sButton += "adm_tvp_galleries_new.aspx?template_id=" + sID;
                sortedMenu[i] = sButton;
            }
        }
        selectQuery.Finish();
        selectQuery = null;
        return sortedMenu;
    }

    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
    }

    protected void GetSubMenu()
    {
        Response.Write(m_sSubMenu);
    }
    protected void GetSubSubMenu()
    {
        Response.Write(m_sSubSubMenu);
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
        theTable += "select tg.family_num,tg.order_num,tg.is_active,tgt.HAS_BUTTONS*tgt.NUM_OF_ITEMS as 'hbnoi',tgt.HAS_TABS*tgt.NUM_OF_ITEMS as 'htnoi', tgt.NUM_OF_ITEMS,tg.id as id,tg.VIRTUAL_NAME as 'Name',tgt.VIRTUAL_NAME as 'Type',tg.status,lcs.description as 'State' from tvp_galleries tg,lu_content_status lcs,tvp_template_channels_gallery_types tgt where tgt.id=tg.CHANNEL_TEMPLATE_ID and lcs.id=tg.status and tg.status<>2 and ";
        theTable += ODBCWrapper.Parameter.NEW_PARAM("tg.group_id", "=", nGroupID);
        theTable += "and";
        if (Session["tvp_profile_id"] != null)
            theTable += ODBCWrapper.Parameter.NEW_PARAM("tg.PROFILE_ID", "=", int.Parse(Session["tvp_profile_id"].ToString()));
        if (Session["tvp_page_id"] != null)
        {
            theTable += ODBCWrapper.Parameter.NEW_PARAM("tg.PAGE_STRUCTURE_ID", "=", int.Parse(Session["tvp_page_id"].ToString()));
            theTable += " and ";
            theTable += ODBCWrapper.Parameter.NEW_PARAM("tg.LOCATION_ID", "=", int.Parse(Session["page_location"].ToString()));
        }

        if (sOrderBy != "")
        {
            theTable += " order by ";
            theTable += sOrderBy;
        }
        else
            theTable += " order by order_num";
        theTable.AddHiddenField("ID");
        theTable.AddHiddenField("status");
        theTable.AddHiddenField("NUM_OF_ITEMS");
        theTable.AddHiddenField("htnoi");
        theTable.AddHiddenField("hbnoi");
        theTable.AddActivationField("tvp_galleries");
        theTable.AddHiddenField("is_active");
        theTable.AddTechDetails("tvp_galleries");
        theTable.AddOrderNumField("tvp_galleries", "ID", "order_num", "Order");
        theTable.AddOrderNumField2("tvp_galleries", "ID", "family_num", "Family ID");
        theTable.AddHiddenField("order_num");
        theTable.AddHiddenField("family_num");
        
        {
            DataTableLinkColumn linkColumn1 = new DataTableLinkColumn("adm_tvp_galleries_items_links.aspx", "Links", "hbnoi=3");
            linkColumn1.AddQueryStringValue("gallery_id", "field=id");
            linkColumn1.AddQueryStringValue("link_type", "2");
            theTable.AddLinkColumn(linkColumn1);
        }
        {
            DataTableLinkColumn linkColumn1 = new DataTableLinkColumn("adm_tvp_galleries_items_links.aspx", "Buttons", "htnoi=3");
            linkColumn1.AddQueryStringValue("gallery_id", "field=id");
            linkColumn1.AddQueryStringValue("link_type", "1");
            theTable.AddLinkColumn(linkColumn1);
        }
        string sPermitedPage = "";
        if (Session["tvp_profile_id"] != null)
            sPermitedPage = "adm_tvp_profiles.aspx?profile_loc=" + Session["profile_loc"].ToString() + "&platform=" + Session["platform"].ToString();
        if (Session["tvp_page_id"] != null)
            sPermitedPage = "adm_tvp_pages.aspx?page_type=" + Session["page_type"].ToString() + "&platform=" + Session["platform"].ToString();

        if (LoginManager.IsActionPermittedOnPage(sPermitedPage, LoginManager.PAGE_PERMISION_TYPE.EDIT))
        {
            DataTableLinkColumn linkColumn1 = new DataTableLinkColumn("adm_tvp_galleries_items.aspx", "Items", "NUM_OF_ITEMS=3");
            linkColumn1.AddQueryStringValue("gallery_id", "field=id");
            theTable.AddLinkColumn(linkColumn1);
        }
        if (LoginManager.IsActionPermittedOnPage(sPermitedPage, LoginManager.PAGE_PERMISION_TYPE.EDIT))
        {
            DataTableLinkColumn linkColumn1 = new DataTableLinkColumn("adm_tvp_galleries_items_new.aspx", "Item", "NUM_OF_ITEMS=2");
            linkColumn1.AddQueryStringValue("gallery_id", "field=id");
            theTable.AddLinkColumn(linkColumn1);
        }

        if (LoginManager.IsActionPermittedOnPage(sPermitedPage, LoginManager.PAGE_PERMISION_TYPE.EDIT))
        {
            DataTableLinkColumn linkColumn1 = new DataTableLinkColumn("adm_tvp_galleries_new.aspx", "Edit", "");
            linkColumn1.AddQueryStringValue("gallery_id", "field=id");
            theTable.AddLinkColumn(linkColumn1);
        }

        if (LoginManager.IsActionPermittedOnPage(sPermitedPage, LoginManager.PAGE_PERMISION_TYPE.REMOVE))
        {
            DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_remove.aspx", "Delete", "STATUS=1;STATUS=3");
            linkColumn.AddQueryStringValue("id", "field=id");
            linkColumn.AddQueryStringValue("db", "tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString());
            linkColumn.AddQueryStringValue("table", "tvp_galleries");
            linkColumn.AddQueryStringValue("confirm", "true");
            linkColumn.AddQueryStringValue("main_menu", "14");
            linkColumn.AddQueryStringValue("sub_menu", "2");
            linkColumn.AddQueryStringValue("rep_field", "username");
            linkColumn.AddQueryStringValue("rep_name", "Username");
            theTable.AddLinkColumn(linkColumn);
        }

        if (LoginManager.IsActionPermittedOnPage(sPermitedPage, LoginManager.PAGE_PERMISION_TYPE.PUBLISH))
        {
            DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_confirm.aspx", "Confirm", "STATUS=3;STATUS=4");
            linkColumn.AddQueryStringValue("id", "field=id");
            linkColumn.AddQueryStringValue("db", "tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString());
            linkColumn.AddQueryStringValue("table", "tvp_galleries");
            linkColumn.AddQueryStringValue("confirm", "true");
            linkColumn.AddQueryStringValue("main_menu", "14");
            linkColumn.AddQueryStringValue("sub_menu", "2");
            linkColumn.AddQueryStringValue("rep_field", "username");
            linkColumn.AddQueryStringValue("rep_name", "Username");
            theTable.AddLinkColumn(linkColumn);
        }

        if (LoginManager.IsActionPermittedOnPage(sPermitedPage, LoginManager.PAGE_PERMISION_TYPE.REMOVE))
        {
            DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_confirm.aspx", "Cancel", "STATUS=3;STATUS=4");
            linkColumn.AddQueryStringValue("id", "field=id");
            linkColumn.AddQueryStringValue("table", "tvp_galleries");
            linkColumn.AddQueryStringValue("db", "tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString());
            linkColumn.AddQueryStringValue("confirm", "false");
            linkColumn.AddQueryStringValue("main_menu", "14");
            linkColumn.AddQueryStringValue("sub_menu", "2");
            linkColumn.AddQueryStringValue("rep_field", "username");
            linkColumn.AddQueryStringValue("rep_name", "Username");
            theTable.AddLinkColumn(linkColumn);
        }

    }

    public void GetBackLink()
    {
        string sBack = "";
        if (Session["tvp_profile_id"] != null)
            sBack = "adm_tvp_profiles.aspx?profile_loc=" + Session["profile_loc"].ToString() + "&platform=" + Session["platform"].ToString();
        if (Session["tvp_page_id"] != null)
            sBack = "adm_tvp_pages.aspx?page_type=" + Session["page_type"].ToString() + "&platform=" + Session["platform"].ToString();
        Response.Write(sBack);
    }

    public void GetAddList()
    {
        Int32 nGroupID = LoginManager.GetLoginGroupID();
        string sRet = "anylinkmenu1.items=[";
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery.SetConnectionKey("tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString());
        selectQuery += "select * from tvp_template_channels_gallery_types where is_active=1 and status=1 and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
        selectQuery += "and";
        if (Session["tvp_profile_id"] != null)
        {
            if (Session["profile_loc"].ToString() == "1")
            {
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("IN_TOP", "=", 1);
                selectQuery += " and id in (select tvp_gallery_template_id from tvp_galleries_templates_page_types where and status=1 and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("PAGE_TYPE", "=", int.Parse(Session["page_type"].ToString()));
                selectQuery += " ) ";
            }
            if (Session["profile_loc"].ToString() == "2")
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("IN_SIDE", "=", 1);
            if (Session["profile_loc"].ToString() == "3")
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("IN_BOTTOM", "=", 1);
        }

        if (Session["tvp_page_id"] != null)
        {
            if (Session["page_location"].ToString() == "4")
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("IN_MAIN", "=", 1);
            if (Session["page_location"].ToString() == "1")
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("IN_TOP", "=", 1);
            selectQuery += " and id in (select tvp_gallery_template_id from tvp_galleries_templates_page_types where status=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("PAGE_TYPE", "=", int.Parse(Session["page_type"].ToString()));
            selectQuery += " ) ";
        }
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            for (int i = 0; i < nCount; i++)
            {
                if (i != 0)
                    sRet += ",";
                string sButton = selectQuery.Table("query").DefaultView[i].Row["VIRTUAL_NAME"].ToString();
                string sID = selectQuery.Table("query").DefaultView[i].Row["ID"].ToString();
                string sLink = "adm_tvp_galleries_new.aspx?template_id=" + sID;
                sRet += "[\"" + sButton + "\", \"" + sLink + "\"]";
            }
        }
        selectQuery.Finish();
        selectQuery = null;
        sRet += "]";
        Response.Write(sRet);
    }

    public string GetPageContent(string sOrderBy, string sPageNum)
    {
        string sOldOrderBy = "";
        if (Session["order_by"] != null)
            sOldOrderBy = Session["order_by"].ToString();
        DBTableWebEditor theTable = new DBTableWebEditor(true, true, true, "", "adm_table_header", "adm_table_cell", "adm_table_alt_cell", "adm_table_link", "adm_table_pager", "adm_table", sOldOrderBy, 50);
        theTable.SetNewList(" ");
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
        string sRet = PageUtils.GetPreHeader();
        if (Session["tvp_profile_id"] != null)
        {
            sRet += ": Profile Items - ";
            sRet += ODBCWrapper.Utils.GetTableSingleVal("tvp_profiles", "NAME", int.Parse(Session["tvp_profile_id"].ToString()), "tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString()).ToString();
        }
        if (Session["tvp_page_id"] != null)
        {
            if (Session["page_location"].ToString() == "1")
                sRet += ": Page Top Items - ";
            if (Session["page_location"].ToString() == "4")
                sRet += ": Page Main Items - ";
            sRet += ODBCWrapper.Utils.GetTableSingleVal("tvp_pages_structure", "VIRTUAL_NAME", int.Parse(Session["tvp_page_id"].ToString()), "tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString()).ToString();
        }

        Response.Write(sRet);
    }
}
