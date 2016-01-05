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

public partial class adm_categories : System.Web.UI.Page
{
    protected string m_sMenu;
    protected string m_sSubMenu;
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
            Int32 nMenuID = 0;
            m_sMenu = TVinciShared.Menu.GetMainMenu(6, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, false);
            if (Request.QueryString["search_save"] != null)
                Session["search_save"] = "1";
            else
                Session["search_save"] = null;

            if (Request.QueryString["parent_category_id"] != null)
                Session["parent_category_id"] = Request.QueryString["parent_category_id"].ToString();
            else
                Session["parent_category_id"] = null;
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
        if (Session["parent_category_id"] != null && Session["parent_category_id"].ToString() != "" && Session["parent_category_id"].ToString() != "0")
        {
            sBack = "adm_categories.aspx?parent_category_id=" + PageUtils.GetTableSingleVal("categories", "parent_category_id", int.Parse(Session["parent_category_id"].ToString()));
        }
        if (sBack != "")
        {
            string sRet = "<tr><td id=\"back_btn\" onclick='window.document.location.href=\"" + sBack + "\";'><a href=\"#back_btn\" class=\"btn_back\"></a></td></tr>";
            Response.Write(sRet);
        }
    }

    protected void GetFirstCategoryValues(ref string sCategoryName, ref Int32 nCategoryID)
    {
        Int32 nGroupID = LoginManager.GetLoginGroupID();
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select id,CATEGORY_NAME from categories where ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
        selectQuery += "and status=1 and is_active=1";
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
            {
                sCategoryName = selectQuery.Table("query").DefaultView[0].Row["CATEGORY_NAME"].ToString();
                nCategoryID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
            }
        }
        selectQuery.Finish();
        selectQuery = null;
    }

    protected string GetWhereAmIStr()
    {
        Int32 nCategoryID = 0;
        //GetFirstCategoryValues(ref sCategoryName, ref nCategoryID);
        if (Session["parent_category_id"] != null && Session["parent_category_id"].ToString() != "" && Session["parent_category_id"].ToString() != "0")
            nCategoryID = int.Parse(Session["parent_category_id"].ToString());

        string sRet = "";
        bool bFirst = true;
        Int32 nLast = 0;
        nLast = 0;

        while (nCategoryID != nLast)
        {
            Int32 nParentID = int.Parse(PageUtils.GetTableSingleVal("categories", "parent_category_id", nCategoryID).ToString());
            string sHeader = PageUtils.GetTableSingleVal("categories", "CATEGORY_NAME", nCategoryID).ToString();
            if (bFirst == false)
                sRet = "<span style=\"cursor:pointer;\" onclick=\"document.location.href='adm_categories.aspx?parent_category_id=" + nParentID.ToString() + "';\">" + sHeader + " </span><span class=\"arrow\">&raquo; </span>" + sRet;
            else
                sRet = sHeader;
            bFirst = false;
            nCategoryID = nParentID;
        }
        if (sRet != "")
            sRet = "Categories: <span style=\"cursor:pointer;\" onclick=\"document.location.href='adm_categories.aspx?parent_category_id=0';\">Root </span><span class=\"arrow\">&raquo; </span>" + sRet;
        else
            sRet = "Categories: Root";
        return sRet;

    }

    protected void FillTheTableEditor(ref DBTableWebEditor theTable, string sOrderBy)
    {
        Int32 nGroupID = LoginManager.GetLoginGroupID();
        Int32 nCategoryID = 0;
        if (Session["parent_category_id"] != null && Session["parent_category_id"].ToString() != "" && Session["parent_category_id"].ToString() != "0")
            nCategoryID = int.Parse(Session["parent_category_id"].ToString());

        theTable += "select q.order_num,q.is_active,q.id as 'ID',q.id as 'CID',p.BASE_URL as 'Pic',q.category_name as 'Category name',q.admin_name as 'Unique Name',q.status,q.State as 'State', pic_Id from (select c.is_active,c.id as id,c.PIC_ID,c.category_name,c.admin_name,c.status,lcs.description as 'State',c.order_num from categories c,lu_content_status lcs where lcs.id=c.status and c.status<>2 and (";
        theTable += ODBCWrapper.Parameter.NEW_PARAM("c.parent_category_id", "=", nCategoryID);
        theTable += "and";
        theTable += ODBCWrapper.Parameter.NEW_PARAM("c.group_id", "=", nGroupID);
        theTable += ") )q  LEFT JOIN pics p ON p.id=q.PIC_ID and " + PageUtils.GetStatusQueryPart("p");
        if (sOrderBy != "")
        {
            theTable += " order by ";
            theTable += sOrderBy;
        }
        else
            theTable += " order by q.order_num";
        theTable.AddHiddenField("ID");
        theTable.AddHiddenField("order_num");
        theTable.AddHiddenField("status");
        theTable.AddOrderByColumn("Category name", "c.category_name");
        theTable.AddOrderByColumn("Unique Name", "c.admin_name");
        theTable.AddTechDetails("categories");
        theTable.AddActivationField("categories");
        theTable.AddImageField("Pic");
        theTable.AddOrderNumField("categories", "id", "order_num", "Order Number");
        theTable.AddHiddenField("pic_Id");
        theTable.AddHiddenField("is_active");
        if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.EDIT))
        {
            DataTableLinkColumn linkColumn1 = new DataTableLinkColumn("adm_categories_new.aspx", "Edit", "");
            linkColumn1.AddQueryStringValue("category_id", "field=id");
            theTable.AddLinkColumn(linkColumn1);
        }

        {
            DataTableLinkColumn linkColumn1 = new DataTableLinkColumn("adm_categories_channels.aspx", "Channels", "");
            linkColumn1.AddQueryStringValue("category_id", "field=id");
            linkColumn1.AddQueryCounterValue("select count(*) as val from categories_channels cc,channels c where c.status=1 and c.is_active=1 and cc.status=1 and c.id=cc.channel_id and cc.CATEGORY_ID=", "field=id");
            theTable.AddLinkColumn(linkColumn1);
        }

        {
            DataTableLinkColumn linkColumn1 = new DataTableLinkColumn("adm_categories.aspx", "Inner Categories", "");
            linkColumn1.AddQueryStringValue("parent_category_id", "field=id");
            linkColumn1.AddQueryCounterValue("select count(*) as val from categories where status=1 and parent_category_id=", "field=id");
            theTable.AddLinkColumn(linkColumn1);
        }

        if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.REMOVE))
        {
            DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_remove.aspx", "Delete", "STATUS=1;STATUS=3");
            linkColumn.AddQueryStringValue("id", "field=id");
            linkColumn.AddQueryStringValue("table", "categories");
            linkColumn.AddQueryStringValue("confirm", "true");
            linkColumn.AddQueryStringValue("main_menu", "6");
            linkColumn.AddQueryStringValue("sub_menu", "1");
            linkColumn.AddQueryStringValue("rep_field", "username");
            linkColumn.AddQueryStringValue("rep_name", "Username");
            theTable.AddLinkColumn(linkColumn);
        }

        if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.PUBLISH))
        {
            DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_confirm.aspx", "Confirm", "STATUS=3;STATUS=4");
            linkColumn.AddQueryStringValue("id", "field=id");
            linkColumn.AddQueryStringValue("table", "categories");
            linkColumn.AddQueryStringValue("confirm", "true");
            linkColumn.AddQueryStringValue("main_menu", "6");
            linkColumn.AddQueryStringValue("sub_menu", "1");
            linkColumn.AddQueryStringValue("rep_field", "username");
            linkColumn.AddQueryStringValue("rep_name", "Username");
            theTable.AddLinkColumn(linkColumn);
        }

        if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.REMOVE))
        {
            DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_confirm.aspx", "Cancel", "STATUS=3;STATUS=4");
            linkColumn.AddQueryStringValue("id", "field=id");
            linkColumn.AddQueryStringValue("table", "categories");
            linkColumn.AddQueryStringValue("confirm", "false");
            linkColumn.AddQueryStringValue("main_menu", "6");
            linkColumn.AddQueryStringValue("sub_menu", "1");
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
        FillTheTableEditor(ref theTable, sOrderBy);

        Int32 nCategoryID = 0;
        //GetFirstCategoryValues(ref sCatName, ref nCategoryID);
        if (Session["parent_category_id"] != null && Session["parent_category_id"].ToString() != "" && Session["parent_category_id"].ToString() != "0")
            nCategoryID = int.Parse(Session["parent_category_id"].ToString());
        string sTable = theTable.GetPageHTML(int.Parse(sPageNum), sOrderBy, "adm_groups.aspx?parent_category_id=" + nCategoryID.ToString());
        Session["LastContentPage"] = "adm_categories.aspx?parent_category_id=" + nCategoryID.ToString();
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
