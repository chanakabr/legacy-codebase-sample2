using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using TVinciShared;

public partial class adm_categories_channels : System.Web.UI.Page
{
    protected string m_sMenu;
    protected string m_sSubMenu;
    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        if (LoginManager.IsPagePermitted("adm_categories.aspx") == false)
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

            if (Request.QueryString["category_id"] != null)
                Session["category_id"] = Request.QueryString["category_id"].ToString();
            else
                Session["category_id"] = null;
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

    public void GetBackButton()
    {
        string sBack = "";
        Int32 nGroupID = LoginManager.GetLoginGroupID();
        if (Session["category_id"] != null && Session["category_id"].ToString() != "" && Session["category_id"].ToString() != "0")
        {
            sBack = "adm_categories.aspx?category_id=" + PageUtils.GetTableSingleVal("categories", "parent_category_id", int.Parse(Session["category_id"].ToString()));
        }
        if (sBack != "")
        {
            string sRet = "<tr><td id=\"back_btn\" onclick='window.document.location.href=\"" + sBack + "\";'><a href=\"#back_btn\" class=\"btn_back\"></a></td></tr>";
            Response.Write(sRet);
        }
    }

    protected string GetWhereAmIStr()
    {
        Int32 nCategoryID = 0;
        if (Session["category_id"] != null && Session["category_id"].ToString() != "" && Session["category_id"].ToString() != "0")
            nCategoryID = int.Parse(Session["category_id"].ToString());

        string sRet = "";
        bool bFirst = true;
        Int32 nLast = 0;
        nLast = 0;

        while (nCategoryID != nLast)
        {
            Int32 nParentID = int.Parse(PageUtils.GetTableSingleVal("categories", "parent_category_id", nCategoryID).ToString());
            string sHeader = PageUtils.GetTableSingleVal("categories", "CATEGORY_NAME", nCategoryID).ToString();
            if (bFirst == false)
                sRet = "<span style=\"cursor:pointer;\" onclick=\"document.location.href='adm_categories.aspx?category_id=" + nParentID.ToString() + "';\">" + sHeader + " </span><span class=\"arrow\">&raquo; </span>" + sRet;
            else
                sRet = sHeader;
            bFirst = false;
            nCategoryID = nParentID;
        }
        if (sRet != "")
            sRet = "Categories Channels: <span style=\"cursor:pointer;\" onclick=\"document.location.href='adm_categories.aspx?category_id=0';\">Root </span><span class=\"arrow\">&raquo; </span>" + sRet;
        else
            sRet = "Categories Channels: Root";
        return sRet;

    }

    protected void FillTheTableEditor(ref DBTableWebEditor theTable, string sOrderBy)
    {
        Int32 nGroupID = LoginManager.GetLoginGroupID();
        string sGroups = PageUtils.GetAllGroupTreeStr(nGroupID);

        Int32 nCategoryID = 0;
        if (Session["category_id"] != null && Session["category_id"].ToString() != "" && Session["category_id"].ToString() != "0")
            nCategoryID = int.Parse(Session["category_id"].ToString());

        theTable += "select q.order_num,q.status,q.c_id as 'Channel ID',q.s_id as 'ID',p.base_url as 'Pic',q.NAME as 'Name',q.description as 'Description',q.channel_type as 'Channel Type',q.s_desc as 'State' from (select lct.description as 'channel_type',cc.order_num as 'order_num',c.PIC_ID as 'pic_id',cc.status,c.ADMIN_NAME as 'NAME',c.DESCRIPTION as 'Description',c.id as 'c_id',cc.id as 's_id',lcs.description as 's_desc' from lu_channel_type lct,channels c,categories_channels cc,lu_content_status lcs";
        theTable += "where c.is_active=1 and cc.channel_id=c.id and lct.id=c.CHANNEL_TYPE and cc.status=1 and c.status=1 and lcs.id=c.status ";
        theTable += "and ";
        theTable += ODBCWrapper.Parameter.NEW_PARAM("cc.category_id", "=", int.Parse(Session["category_id"].ToString()));
        theTable += "and c.group_id " + sGroups;
        theTable += " )q LEFT JOIN pics p ON p.id=q.pic_id and " + PageUtils.GetStatusQueryPart("p");
        if (sOrderBy != "")
        {
            theTable += " order by ";
            theTable += sOrderBy;
        }
        else
            theTable += " order by q.order_num,q.s_id desc";
        theTable.AddHiddenField("status");
        theTable.AddOrderByColumn("Name", "q.NAME");
        //theTable.AddOrderByColumn("ID", "q.s_id");
        theTable.AddHiddenField("ID");
        theTable.AddOrderByColumn("Channel ID", "q.c_id");
        theTable.AddImageField("Pic");
        theTable.AddTechDetails("categories_channels");
        theTable.AddOrderNumField("categories_channels", "id", "order_num", "Order Number");
        theTable.AddHiddenField("order_num");

        if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.REMOVE))
        {
            DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_remove.aspx", "Delete", "STATUS=1;STATUS=3");
            linkColumn.AddQueryStringValue("id", "field=id");
            linkColumn.AddQueryStringValue("table", "categories_channels");
            linkColumn.AddQueryStringValue("confirm", "true");
            linkColumn.AddQueryStringValue("main_menu", "6");
            linkColumn.AddQueryStringValue("sub_menu", "2");
            linkColumn.AddQueryStringValue("rep_field", "NAME");
            linkColumn.AddQueryStringValue("rep_name", "ων");
            theTable.AddLinkColumn(linkColumn);
        }

        if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.PUBLISH))
        {
            DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_confirm.aspx", "Confirm", "STATUS=3;STATUS=4");
            linkColumn.AddQueryStringValue("id", "field=id");
            linkColumn.AddQueryStringValue("table", "categories_channels");
            linkColumn.AddQueryStringValue("confirm", "true");
            linkColumn.AddQueryStringValue("main_menu", "6");
            linkColumn.AddQueryStringValue("sub_menu", "2");
            linkColumn.AddQueryStringValue("rep_field", "NAME");
            linkColumn.AddQueryStringValue("rep_name", "ων");
            theTable.AddLinkColumn(linkColumn);
        }

        if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.PUBLISH))
        {
            DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_confirm.aspx", "Cancel", "STATUS=3;STATUS=4");
            linkColumn.AddQueryStringValue("id", "field=id");
            linkColumn.AddQueryStringValue("table", "categories_channels");
            linkColumn.AddQueryStringValue("confirm", "false");
            linkColumn.AddQueryStringValue("main_menu", "6");
            linkColumn.AddQueryStringValue("sub_menu", "2");
            linkColumn.AddQueryStringValue("rep_field", "NAME");
            linkColumn.AddQueryStringValue("rep_name", "ων");
            theTable.AddLinkColumn(linkColumn);
        }
    }

    public string GetPageContent(string sOrderBy, string sPageNum)
    {
        string sOldOrderBy = "";
        if (Session["order_by"] != null)
            sOldOrderBy = Session["order_by"].ToString();
        DBTableWebEditor theTable = new DBTableWebEditor(true, true, false, "", "adm_table_header", "adm_table_cell", "adm_table_alt_cell", "adm_table_link", "adm_table_pager", "adm_table", sOldOrderBy, 50);
        FillTheTableEditor(ref theTable, sOrderBy);

        Int32 nCategoryID = 0;
        //GetFirstCategoryValues(ref sCatName, ref nCategoryID);
        if (Session["category_id"] != null && Session["category_id"].ToString() != "" && Session["category_id"].ToString() != "0")
            nCategoryID = int.Parse(Session["category_id"].ToString());
        string sTable = theTable.GetPageHTML(int.Parse(sPageNum), sOrderBy, "adm_groups.aspx?category_id=" + nCategoryID.ToString());
        Session["LastContentPage"] = "adm_categories.aspx?category_id=" + nCategoryID.ToString();
        theTable.Finish();
        theTable = null;
        return sTable;
    }

    public void GetHeader()
    {
        Response.Write(PageUtils.GetPreHeader() + ": " + GetWhereAmIStr());
    }

    public void GetCahannelsIFrame()
    {
        string sRet = "<IFRAME SRC=\"admin_tree_player.aspx";
        sRet += "\" WIDTH=\"800px\" HEIGHT=\"300px\" FRAMEBORDER=\"0\"></IFRAME>";
        Response.Write(sRet);
    }
}
