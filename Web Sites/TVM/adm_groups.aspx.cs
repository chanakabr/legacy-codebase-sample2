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

public partial class adm_groups : System.Web.UI.Page
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
            m_sMenu = TVinciShared.Menu.GetMainMenu(2, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, false);
            if (Request.QueryString["search_save"] != null)
                Session["search_save"] = "1";
            else
                Session["search_save"] = null;

            if (Request.QueryString["parent_group_id"] != null)
                Session["parent_group_id"] = Request.QueryString["parent_group_id"].ToString();
            else
                Session["parent_group_id"] = null;
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
        if (Session["parent_group_id"] != null && Session["parent_group_id"].ToString() != "" && Session["parent_group_id"].ToString() != "0")
        {
            if (nGroupID != int.Parse(Session["parent_group_id"].ToString()))
            {
                sBack = "adm_groups.aspx?parent_group_id=" + PageUtils.GetTableSingleVal("groups", "parent_group_id", int.Parse(Session["parent_group_id"].ToString()));
            }
        }
        if (sBack != "")
        {
            string sRet = "<tr><td id=\"back_btn\" onclick='window.document.location.href=\"" + sBack + "\";'><a href=\"#back_btn\" class=\"btn_back\"></a></td></tr>";
            Response.Write(sRet);
        }
    }

    protected string GetWhereAmIStr()
    {
        string sGroupName = LoginManager.GetLoginGroupName();
        Int32 nGroupID = LoginManager.GetLoginGroupID();
        if (Session["parent_group_id"] != null && Session["parent_group_id"].ToString() != "" && Session["parent_group_id"].ToString() != "0")
            nGroupID = int.Parse(Session["parent_group_id"].ToString());
        
        string sRet = "";
        bool bFirst = true;
        Int32 nLast = 0;
        nLast = int.Parse(PageUtils.GetTableSingleVal("groups" , "parent_group_id" , LoginManager.GetLoginGroupID()).ToString());
        
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

    protected void FillTheTableEditor(ref DBTableWebEditor theTable, string sOrderBy)
    {
        Int32 nGroupID = LoginManager.GetLoginGroupID();
        if (Session["parent_group_id"] != null && Session["parent_group_id"].ToString() != "" && Session["parent_group_id"].ToString() != "0")
            nGroupID = int.Parse(Session["parent_group_id"].ToString());

        //theTable += "select q.status,q.s_id as 'Article ID',p.base_url as 'Pic',q.HEADER as 'Header',q.writer as 'Writer',q.s_id as id,q.s_desc as 'State' from (select s.LOGO_PIC_ID as pic_id,s.status,s.HEADER as 'HEADER',s.SUB_HEADER as 'SUB_HEADER',s.SHORT_DESCRIPTION as 'SHORT_DESCRIPTION' ,s.writer,s.id as s_id,lcs.description as s_desc from articles s,lu_content_status lcs where ";
        //theTable += "lcs.id=s.status and s.status<>2)q LEFT JOIN pics p ON p.id=q.pic_id and " + PageUtils.GetStatusQueryPart("p");

        theTable += "select q.order_num,q.is_active,q.id as id,p.BASE_URL as 'Logo',q.group_name as 'Group name',q.status,q.State as 'State', q.ADMIN_LOGO as 'pic_id', q.id as 'pic_group_id' from (select g.id as id,g.ADMIN_LOGO,g.group_name,g.status,lcs.description as 'State',order_num,g.is_active from groups g,lu_content_status lcs where lcs.id=g.status and g.status<>2 and (";
        theTable += ODBCWrapper.Parameter.NEW_PARAM("g.parent_group_id", "=", nGroupID);
        theTable += ") )q  LEFT JOIN pics p ON p.id=q.ADMIN_LOGO and " + PageUtils.GetStatusQueryPart("p");
        if (sOrderBy != "")
        {
            theTable += " order by ";
            theTable += sOrderBy;
        }
        else
            theTable += " order by q.order_num";
        theTable.AddHiddenField("ID");
        theTable.AddHiddenField("status");
        theTable.AddOrderByColumn("Group name", "g.group_name");
        theTable.AddOrderNumField("groups", "ID", "order_num", "Order number");
        theTable.AddHiddenField("order_num");
        theTable.AddTechDetails("groups");
        theTable.AddActivationField("groups");
        theTable.AddHiddenField("is_active");
        theTable.AddImageField("Logo");
        theTable.AddHiddenField("pic_id");
        theTable.AddHiddenField("pic_group_id");
        /*
        if (LoginManager.GetLoginGroupID() == 1)
        {
            DataTableLinkColumn linkColumn1 = new DataTableLinkColumn("adm_domains.aspx", "Domains", "");
            linkColumn1.AddQueryStringValue("group_id", "field=id");
            linkColumn1.AddQueryCounterValue("select count(*) as val from groups_domains where status=1 and is_active=1 and GROUP_ID=", "field=id");
            theTable.AddLinkColumn(linkColumn1);
        }
        */
        {
            DataTableLinkColumn linkColumn1 = new DataTableLinkColumn("adm_browse_as.aspx", "Browse as", "");
            linkColumn1.AddQueryStringValue("group_id", "field=id");
            theTable.AddLinkColumn(linkColumn1);
        }
        if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.EDIT))
        {
            DataTableLinkColumn linkColumn1 = new DataTableLinkColumn("adm_groups_new.aspx", "Edit", "");
            linkColumn1.AddQueryStringValue("group_id", "field=id");
            theTable.AddLinkColumn(linkColumn1);
        }

        {
            DataTableLinkColumn linkColumn1 = new DataTableLinkColumn("adm_groups.aspx", "Inner groups", "");
            linkColumn1.AddQueryStringValue("parent_group_id", "field=id");
            linkColumn1.AddQueryCounterValue("select count(*) as val from groups where status=1 and is_active=1 and parent_GROUP_ID=", "field=id");
            theTable.AddLinkColumn(linkColumn1);
        }

        if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.REMOVE))
        {
            DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_remove.aspx", "Delete", "STATUS=1;STATUS=3");
            linkColumn.AddQueryStringValue("id", "field=id");
            linkColumn.AddQueryStringValue("table", "groups");
            linkColumn.AddQueryStringValue("confirm", "true");
            linkColumn.AddQueryStringValue("main_menu", "2");
            linkColumn.AddQueryStringValue("sub_menu", "1");
            linkColumn.AddQueryStringValue("rep_field", "username");
            linkColumn.AddQueryStringValue("rep_name", "Username");
            theTable.AddLinkColumn(linkColumn);
        }

        if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.PUBLISH))
        {
            DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_confirm.aspx", "Confirm", "STATUS=3;STATUS=4");
            linkColumn.AddQueryStringValue("id", "field=id");
            linkColumn.AddQueryStringValue("table", "groups");
            linkColumn.AddQueryStringValue("confirm", "true");
            linkColumn.AddQueryStringValue("main_menu", "2");
            linkColumn.AddQueryStringValue("sub_menu", "1");
            linkColumn.AddQueryStringValue("rep_field", "username");
            linkColumn.AddQueryStringValue("rep_name", "Username");
            theTable.AddLinkColumn(linkColumn);
        }

        if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.REMOVE))
        {
            DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_confirm.aspx", "Cancel", "STATUS=3;STATUS=4");
            linkColumn.AddQueryStringValue("id", "field=id");
            linkColumn.AddQueryStringValue("table", "groups");
            linkColumn.AddQueryStringValue("confirm", "false");
            linkColumn.AddQueryStringValue("main_menu", "2");
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
        bool bAdd = false;
        if (LoginManager.GetLoginGroupID() == 1)
            bAdd = true;
        DBTableWebEditor theTable = new DBTableWebEditor(true, true, bAdd, "", "adm_table_header", "adm_table_cell", "adm_table_alt_cell", "adm_table_link", "adm_table_pager", "adm_table", sOldOrderBy, 50);
        FillTheTableEditor(ref theTable, sOrderBy);

        Int32 nGroupID = LoginManager.GetLoginGroupID();
        if (Session["parent_group_id"] != null && Session["parent_group_id"].ToString() != "" && Session["parent_group_id"].ToString() != "0")
            nGroupID = int.Parse(Session["parent_group_id"].ToString());
        string sTable = theTable.GetPageHTML(int.Parse(sPageNum), sOrderBy, "adm_groups.aspx?parent_group_id=" + nGroupID.ToString());
        Session["LastContentPage"] = "adm_groups.aspx?parent_group_id=" + nGroupID.ToString();
        theTable.Finish();
        theTable = null;
        return sTable;
    }

    public void GetHeader()
    {
        Response.Write(PageUtils.GetPreHeader() + ": " + GetWhereAmIStr());
    }
}
