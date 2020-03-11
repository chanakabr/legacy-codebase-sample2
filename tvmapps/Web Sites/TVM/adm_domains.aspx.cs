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

public partial class adm_domains : System.Web.UI.Page
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
            m_sMenu = TVinciShared.Menu.GetMainMenu(3, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, false);
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

    protected void FillTheTableEditor(ref DBTableWebEditor theTable, string sOrderBy)
    {
        Int32 nGroupID = LoginManager.GetLoginGroupID();

        //theTable += "select q.status,q.s_id as 'Article ID',p.base_url as 'Pic',q.HEADER as 'Header',q.writer as 'Writer',q.s_id as id,q.s_desc as 'State' from (select s.LOGO_PIC_ID as pic_id,s.status,s.HEADER as 'HEADER',s.SUB_HEADER as 'SUB_HEADER',s.SHORT_DESCRIPTION as 'SHORT_DESCRIPTION' ,s.writer,s.id as s_id,lcs.description as s_desc from articles s,lu_content_status lcs where ";
        //theTable += "lcs.id=s.status and s.status<>2)q LEFT JOIN pics p ON p.id=q.pic_id and " + PageUtils.GetStatusQueryPart("p");

        theTable += "select gd.is_active,gd.id as id,g.group_name as 'Group',gd.domain as 'Domain',gd.USERNAME as 'Players Username',gd.status,lcs.description as 'State' from groups g,groups_passwords gd,lu_content_status lcs where lcs.id=gd.status and gd.status<>2 and g.status<>2 and gd.group_id=g.id";
        //if (PageUtils.IsTvinciUser() == false)
        //{
            theTable += "and";
            theTable += ODBCWrapper.Parameter.NEW_PARAM("g.id", "=", nGroupID);
        //}
        if (sOrderBy != "")
        {
            theTable += " order by ";
            theTable += sOrderBy;
        }
        else
            theTable += " order by g.group_name";
        theTable.AddHiddenField("ID");
        theTable.AddHiddenField("status");
        theTable.AddOrderByColumn("Group", "g.group_name");
        theTable.AddTechDetails("groups_passwords");
        theTable.AddActivationField("groups_passwords");
        theTable.AddHiddenField("is_active");

        if (PageUtils.IsTvinciUser() == true)
        {
            if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.EDIT))
            {
                DataTableLinkColumn linkColumn1 = new DataTableLinkColumn("adm_domains_new.aspx", "Edit", "");
                linkColumn1.AddQueryStringValue("group_domain_id", "field=id");
                theTable.AddLinkColumn(linkColumn1);
            }
        }

        if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.REMOVE))
        {
            DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_remove.aspx", "Delete", "STATUS=1;STATUS=3");
            linkColumn.AddQueryStringValue("id", "field=id");
            linkColumn.AddQueryStringValue("table", "groups_passwords");
            linkColumn.AddQueryStringValue("confirm", "true");
            linkColumn.AddQueryStringValue("main_menu", "3");
            linkColumn.AddQueryStringValue("sub_menu", "1");
            linkColumn.AddQueryStringValue("rep_field", "username");
            linkColumn.AddQueryStringValue("rep_name", "Username");
            theTable.AddLinkColumn(linkColumn);
        }

        if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.PUBLISH))
        {
            DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_confirm.aspx", "Confirm", "STATUS=3;STATUS=4");
            linkColumn.AddQueryStringValue("id", "field=id");
            linkColumn.AddQueryStringValue("table", "groups_passwords");
            linkColumn.AddQueryStringValue("confirm", "true");
            linkColumn.AddQueryStringValue("main_menu", "3");
            linkColumn.AddQueryStringValue("sub_menu", "1");
            linkColumn.AddQueryStringValue("rep_field", "username");
            linkColumn.AddQueryStringValue("rep_name", "Username");
            theTable.AddLinkColumn(linkColumn);
        }

        if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.REMOVE))
        {
            DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_confirm.aspx", "Cancel", "STATUS=3;STATUS=4");
            linkColumn.AddQueryStringValue("id", "field=id");
            linkColumn.AddQueryStringValue("table", "groups_passwords");
            linkColumn.AddQueryStringValue("confirm", "false");
            linkColumn.AddQueryStringValue("main_menu", "3");
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
        if (PageUtils.IsTvinciUser() == true)
            bAdd = true;
        DBTableWebEditor theTable = new DBTableWebEditor(true, true, bAdd, "", "adm_table_header", "adm_table_cell", "adm_table_alt_cell", "adm_table_link", "adm_table_pager", "adm_table", sOldOrderBy, 50);
        FillTheTableEditor(ref theTable, sOrderBy);

        string sTable = theTable.GetPageHTML(int.Parse(sPageNum), sOrderBy);

        theTable.Finish();
        theTable = null;
        return sTable;
    }

    public void GetHeader()
    {
        Response.Write(PageUtils.GetPreHeader() + ": Players Login Details");
    }
}
