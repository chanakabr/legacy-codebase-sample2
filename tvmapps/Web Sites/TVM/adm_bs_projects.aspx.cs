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

public partial class adm_bs_projects : System.Web.UI.Page
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
            m_sMenu = TVinciShared.Menu.GetMainMenu(17, true, ref nMenuID);
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
        theTable += "select p.is_active,p.id as 'Project ID',p.id,p.name as 'Name',p.description as 'Description',p.status,lcs.description as 'State' from bs_projects p,lu_content_status lcs where lcs.id=p.status and p.status<>2 and ";
        theTable += ODBCWrapper.Parameter.NEW_PARAM("p.group_id", "=", nGroupID);
        if (sOrderBy != "")
        {
            theTable += " order by ";
            theTable += sOrderBy;
        }
        else
            theTable += " order by p.id";

        DataTableMultiValuesColumn multi_accounts = new DataTableMultiValuesColumn("Mailing List", "val", "pa.project_id", "ID");
        multi_accounts += "select a.username as val from accounts a,bs_projects_accounts pa where pa.account_id=a.id and pa.status=1 and ";
        theTable.AddMultiValuesColumn(multi_accounts);

        theTable.AddHiddenField("status");
        theTable.AddHiddenField("id");
        theTable.AddHiddenField("is_active");
        theTable.AddOrderByColumn("Name", "p.name");
        theTable.AddTechDetails("bs_projects");
        theTable.AddActivationField("bs_projects");

        {
            DataTableLinkColumn linkColumn1 = new DataTableLinkColumn("adm_bs_bugs.aspx", "Bugs", "");
            linkColumn1.AddQueryStringValue("project_id", "field=id");
            linkColumn1.AddQueryCounterValue("select count(*) as val from bs_project_bugs cc,bs_projects c where cc.BUG_TYPE_ID=1 and cc.CARE_STATUS<>4 and cc.PROJECT_ID=c.id and c.status=1 and c.is_active=1 and cc.status=1 and cc.project_id=", "field=id");
            theTable.AddLinkColumn(linkColumn1);
        }
        {
            DataTableLinkColumn linkColumn1 = new DataTableLinkColumn("adm_bs_features.aspx", "Features", "");
            linkColumn1.AddQueryStringValue("project_id", "field=id");
            linkColumn1.AddQueryCounterValue("select count(*) as val from bs_project_bugs cc,bs_projects c where cc.BUG_TYPE_ID=2 and cc.BUG_TYPE_ID<>4 and cc.CARE_STATUS<>4 and cc.PROJECT_ID=c.id and c.status=1 and c.is_active=1 and cc.status=1 and cc.project_id=", "field=id");
            theTable.AddLinkColumn(linkColumn1);
        }

        if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.EDIT))
        {
            DataTableLinkColumn linkColumn1 = new DataTableLinkColumn("adm_bs_projects_new.aspx", "Edit", "");
            linkColumn1.AddQueryStringValue("project_id", "field=id");
            theTable.AddLinkColumn(linkColumn1);
        }

        if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.REMOVE))
        {
            DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_remove.aspx", "Delete", "STATUS=1;STATUS=3");
            linkColumn.AddQueryStringValue("id", "field=id");
            linkColumn.AddQueryStringValue("table", "bs_projects");
            linkColumn.AddQueryStringValue("confirm", "true");
            linkColumn.AddQueryStringValue("main_menu", "17");
            linkColumn.AddQueryStringValue("sub_menu", "1");
            linkColumn.AddQueryStringValue("rep_field", "username");
            linkColumn.AddQueryStringValue("rep_name", "Username");
            theTable.AddLinkColumn(linkColumn);
        }

        if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.PUBLISH))
        {
            DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_confirm.aspx", "Confirm", "STATUS=3;STATUS=4");
            linkColumn.AddQueryStringValue("id", "field=id");
            linkColumn.AddQueryStringValue("table", "bs_projects");
            linkColumn.AddQueryStringValue("confirm", "true");
            linkColumn.AddQueryStringValue("main_menu", "17");
            linkColumn.AddQueryStringValue("sub_menu", "1");
            linkColumn.AddQueryStringValue("rep_field", "username");
            linkColumn.AddQueryStringValue("rep_name", "Username");
            theTable.AddLinkColumn(linkColumn);
        }

        if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.REMOVE))
        {
            DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_confirm.aspx", "Cancel", "STATUS=3;STATUS=4");
            linkColumn.AddQueryStringValue("id", "field=id");
            linkColumn.AddQueryStringValue("table", "bs_projects");
            linkColumn.AddQueryStringValue("confirm", "false");
            linkColumn.AddQueryStringValue("main_menu", "17");
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

        string sTable = theTable.GetPageHTML(int.Parse(sPageNum), sOrderBy, "adm_bs_projects.aspx");
        theTable.Finish();
        theTable = null;
        return sTable;
    }

    public void GetHeader()
    {
        Response.Write(PageUtils.GetPreHeader() + ": Bug System");
    }
}
