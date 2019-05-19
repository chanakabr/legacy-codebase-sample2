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

public partial class adm_tvp_menu_items : System.Web.UI.Page
{
    protected string m_sMenu;
    protected string m_sSubMenu;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        if (LoginManager.IsPagePermitted("adm_tvp_menu.aspx") == false)
            LoginManager.LogoutFromSite("login.html");
        if (AMS.Web.RemoteScripting.InvokeMethod(this)) 
            return;
        if (!IsPostBack)
        {
            Int32 nMenuID = 0;
            m_sMenu = TVinciShared.Menu.GetMainMenu(14, true, ref nMenuID, "adm_tvp_menu.aspx");
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 3, false);
            if (Request.QueryString["search_save"] != null)
                Session["search_save"] = "1";
            else
                Session["search_save"] = null;

            if (Request.QueryString["menu_item_parent_id"] != null &&
               Request.QueryString["menu_item_parent_id"].ToString() != "")
            {
                Session["menu_item_parent_id"] = int.Parse(Request.QueryString["menu_item_parent_id"].ToString());
            }
            else
                Session["menu_item_parent_id"] = 0;

            if (Request.QueryString["menu_id"] != null &&
               Request.QueryString["menu_id"].ToString() != "")
            {
                Session["menu_id"] = int.Parse(Request.QueryString["menu_id"].ToString());
            }
            else
            {
                LoginManager.LogoutFromSite("login.html");
                return;
            }
        }
    }
    /*
    protected string GetWhereAmIStr()
    {
        string sItem
        if (Session["menu_item_parent_id"] != null && Session["menu_item_parent_id"].ToString() != "" && Session["menu_item_parent_id"].ToString() != "0")
            nGroupID = int.Parse(Session["menu_item_parent_id"].ToString());

        string sRet = "";
        bool bFirst = true;
        Int32 nLast = 0;
        nLast = int.Parse(PageUtils.GetTableSingleVal("groups", "parent_group_id", LoginManager.GetLoginGroupID()).ToString());

        while (nGroupID != nLast)
        {
            Int32 nParentID = int.Parse(PageUtils.GetTableSingleVal("groups", "PARENT_GROUP_ID", nGroupID).ToString());
            string sHeader = PageUtils.GetTableSingleVal("groups", "group_name", nGroupID).ToString();
            if (bFirst == false)
                sRet = "<span style=\"cursor:pointer;\" onclick=\"document.location.href='adm_groups.aspx?parent_group_id=" + nParentID.ToString() + "';\">" + sHeader + " </span><span class=\"arrow\">&raquo; </span>" + sRet;
            else
                sRet = sHeader;
            bFirst = false;
            nGroupID = nParentID;
        }
        sRet = "Groups: " + sRet;
        return sRet;

    }
    */

    static protected Int32 GetMainLang(ref string sMainLang)
    {
        Int32 nLangID = 0;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select l.CODE3,l.id from groups g,lu_languages l where l.id=g.language_id and  ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("g.id", "=", LoginManager.GetLoginGroupID());
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
            {
                sMainLang = selectQuery.Table("query").DefaultView[0].Row["CODE3"].ToString();
                nLangID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
            }
        }
        selectQuery.Finish();
        selectQuery = null;
        return nLangID;
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

    public void GetBackLink()
    {
        Int32 nGroupID = LoginManager.GetLoginGroupID();
        string sConn = "tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString();
        string sBack = "adm_tvp_menu.aspx";

        string sParent = "0";
        object oParent = ODBCWrapper.Utils.GetTableSingleVal("tvp_menu_items", "PARENT_MENU_ITEM_ID", int.Parse(Session["menu_item_parent_id"].ToString()), sConn);
        if (oParent != DBNull.Value && oParent != null)
            sParent = oParent.ToString();
        if (Session["menu_item_parent_id"].ToString() != "0")
            sBack = "adm_tvp_menu_items.aspx?search_save=1&menu_id=" + Session["menu_id"].ToString() + "&menu_item_parent_id=" + sParent;

        Response.Write(sBack);
    }

    protected void FillTheTableEditor(ref DBTableWebEditor theTable, string sOrderBy)
    {
        string sLang = "";
        Int32 nMainLangID = GetMainLang(ref sLang);
        Int32 nGroupID = LoginManager.GetLoginGroupID();
        theTable.SetConnectionKey("tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString());
        theTable += "select q.is_active,mt.TITLE as 'Title(" + sLang + ")',q.order_num,q.id as id,q.status as status,q.State as State from ";
        theTable += "(select tmi.is_active,tmi.order_num,tmi.id as id,tmi.status,lcs.description as 'State' from tvp_menu_items tmi,lu_content_status lcs where lcs.id=tmi.status and tmi.status<>2 and ";
        theTable += ODBCWrapper.Parameter.NEW_PARAM("tmi.menu_id", "=", int.Parse(Session["menu_id"].ToString()));
        theTable += "and";
        theTable += ODBCWrapper.Parameter.NEW_PARAM("tmi.PARENT_MENU_ITEM_ID", "=", int.Parse(Session["menu_item_parent_id"].ToString()));
        theTable += "and";
        theTable += ODBCWrapper.Parameter.NEW_PARAM("tmi.group_id", "=", nGroupID);
        theTable += ")q left join tvp_menu_items_texts mt ON mt.MENU_ITEM_ID=q.id and mt.status=1 and mt.is_active=1 and ";
        theTable += ODBCWrapper.Parameter.NEW_PARAM("mt.LANGUAGE_ID", "=", nMainLangID);
        if (sOrderBy != "")
        {
            theTable += " order by ";
            theTable += sOrderBy;
        }
        else
        {
            theTable += " order by order_num";
        }

        theTable.AddHiddenField("ID");
        theTable.AddHiddenField("status");
        theTable.AddHiddenField("order_num");
        theTable.AddActivationField("tvp_menu_items");
        theTable.AddHiddenField("is_active");
        theTable.AddTechDetails("tvp_menu_items");
        theTable.AddOrderNumField("tvp_menu_items", "id", "order_num", "Order Number");
        //theTable.AddHiddenField("order_num");
        {
            DataTableLinkColumn linkColumn1 = new DataTableLinkColumn("adm_tvp_menu_items.aspx", "Items", "");
            linkColumn1.AddQueryStringValue("menu_item_parent_id", "field=id");
            linkColumn1.AddQueryStringValue("menu_id", Session["menu_id"].ToString());
            theTable.AddLinkColumn(linkColumn1);
        }

        if (LoginManager.IsActionPermittedOnPage("adm_tvp_menu.aspx" , LoginManager.PAGE_PERMISION_TYPE.EDIT))
        {
            DataTableLinkColumn linkColumn1 = new DataTableLinkColumn("adm_tvp_menu_items_new.aspx", "Edit", "");
            linkColumn1.AddQueryStringValue("tvp_menu_item_id", "field=id");
            theTable.AddLinkColumn(linkColumn1);
        }

        if (LoginManager.IsActionPermittedOnPage("adm_tvp_menu.aspx", LoginManager.PAGE_PERMISION_TYPE.REMOVE))
        {
            DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_remove.aspx", "Delete", "STATUS=1;STATUS=3");
            linkColumn.AddQueryStringValue("id", "field=id");
            linkColumn.AddQueryStringValue("db", "tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString());
            linkColumn.AddQueryStringValue("table", "tvp_menu_items");
            linkColumn.AddQueryStringValue("confirm", "true");
            linkColumn.AddQueryStringValue("main_menu", "14");
            linkColumn.AddQueryStringValue("sub_menu", "2");
            linkColumn.AddQueryStringValue("rep_field", "username");
            linkColumn.AddQueryStringValue("rep_name", "Username");
            theTable.AddLinkColumn(linkColumn);
        }

        if (LoginManager.IsActionPermittedOnPage("adm_tvp_menu.aspx" , LoginManager.PAGE_PERMISION_TYPE.PUBLISH))
        {
            DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_confirm.aspx", "Confirm", "STATUS=3;STATUS=4");
            linkColumn.AddQueryStringValue("id", "field=id");
            linkColumn.AddQueryStringValue("db", "tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString());
            linkColumn.AddQueryStringValue("table", "tvp_menu_items");
            linkColumn.AddQueryStringValue("confirm", "true");
            linkColumn.AddQueryStringValue("main_menu", "14");
            linkColumn.AddQueryStringValue("sub_menu", "2");
            linkColumn.AddQueryStringValue("rep_field", "username");
            linkColumn.AddQueryStringValue("rep_name", "Username");
            theTable.AddLinkColumn(linkColumn);
        }

        if (LoginManager.IsActionPermittedOnPage("adm_tvp_menu.aspx" , LoginManager.PAGE_PERMISION_TYPE.REMOVE))
        {
            DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_confirm.aspx", "Cancel", "STATUS=3;STATUS=4");
            linkColumn.AddQueryStringValue("id", "field=id");
            linkColumn.AddQueryStringValue("table", "tvp_menu_items");
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
        Session["ContentPage"] = "adm_tvp_menu.aspx";
        Session["LastContentPage"] = "adm_tvp_menu_items.aspx?search_save=1&menu_id=" + Session["menu_id"].ToString() + "&menu_item_parent_id=" + Session["menu_item_parent_id"].ToString();
        theTable.Finish();
        theTable = null;
        return sTable;
    }

    protected string GetWhereAmIStr(string sMenuName)
    {
        Int32 nParentItemID = 0;
        if (Session["menu_item_parent_id"] != null && Session["menu_item_parent_id"].ToString() != "" && Session["menu_item_parent_id"].ToString() != "0")
            nParentItemID = int.Parse(Session["menu_item_parent_id"].ToString());

        string sRet = "";
        bool bFirst = true;
        Int32 nLast = 0;
        nLast = 0;
        string sMainLang = "";
        Int32 nMainLang = GetMainLang(ref sMainLang);
        while (nParentItemID != nLast)
        {
            Int32 nGroupID = LoginManager.GetLoginGroupID();
            string sConn = "tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString();
            Int32 nParentID = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("tvp_menu_items", "PARENT_MENU_ITEM_ID", nParentItemID , sConn).ToString());
            string sHeader = "-";
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetConnectionKey(sConn);
            selectQuery += "select TITLE from tvp_menu_items_texts where is_active=1 and status=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("LANGUAGE_ID", "=", nMainLang);
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MENU_ITEM_ID", "=", nParentItemID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    object oHeader = selectQuery.Table("query").DefaultView[0].Row["TITLE"];
                    if (oHeader != null && oHeader != DBNull.Value)
                        sHeader = oHeader.ToString();
                }
            }
            selectQuery.Finish();
            selectQuery = null;

            //if (bFirst == false)
                sRet = "<span style=\"cursor:pointer;\" onclick=\"document.location.href='adm_tvp_menu_items.aspx?parent_group_id=" + nParentID.ToString() + "'menu_id=" + Session["menu_id"].ToString() + "&menu_item_parent_id=" + nParentID.ToString() + "';\">" + sHeader + " </span><span class=\"arrow\">&raquo; </span>" + sRet;
            //else
                //sRet = sHeader;
            bFirst = false;
            nParentItemID = nParentID;
        }
        
        sRet = "<span style=\"cursor:pointer;\" onclick=\"document.location.href='adm_tvp_menu.aspx?menu_id=" + Session["menu_id"].ToString() + "';\">Menu: " + sMenuName + " </span><span class=\"arrow\">&raquo; </span>" + sRet;
        return sRet;

    }

    public void GetHeader()
    {
        Int32 nGroupID = LoginManager.GetLoginGroupID();
        string sConn = "tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString();
        object oMenuName = ODBCWrapper.Utils.GetTableSingleVal("tvp_menu", "name", int.Parse(Session["menu_id"].ToString()), sConn);
        string sMenuName = "";
        if (oMenuName != DBNull.Value)
            sMenuName = oMenuName.ToString();
        Response.Write(PageUtils.GetPreHeader() + ": Menu Items : " + GetWhereAmIStr(sMenuName));   
    }
}
