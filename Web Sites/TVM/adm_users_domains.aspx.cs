using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TVinciShared;

public partial class adm_users_domains : System.Web.UI.Page
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
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 4, false);
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
        theTable.SetConnectionKey("users_connection");
        theTable += "select d.id, d.name, d.description, d.status, d.is_active from domains d";
        if (Session["search_user_name"] != null && Session["search_user_name"].ToString() != "")
        {
            theTable += " join users_domains ud on ud.domain_id=d.id and ud.status=1 and ud.Is_Active=1 and ud.Is_Master=1";
            theTable += " join users u on ud.user_id=u.id and u.status=1 and u.Is_Active=1 and u.username like (N'%"+Session["search_user_name"].ToString().ToLower().Trim() + "%')";
        }

        theTable += "where d.status<>2 and ";
        theTable += ODBCWrapper.Parameter.NEW_PARAM("d.group_id", "=", nGroupID);
        if (Session["search_domain_id"] != null && Session["search_domain_id"].ToString() != "")
        {
            int domainId = 0;
            if (int.TryParse(Session["search_domain_id"].ToString(), out domainId) && domainId > 0)
            {
                theTable += " and d.id=" + domainId;
            }
        }

        if (sOrderBy != "")
        {
            theTable += " order by ";
            theTable += sOrderBy;
        }

        theTable.AddActivationField("domains");
        theTable.AddHiddenField("status");
        theTable.AddHiddenField("is_active");

        DataTableLinkColumn linkColumn1 = new DataTableLinkColumn("adm_users_list.aspx", "Users", "");
        linkColumn1.AddQueryStringValue("domain_id", "field=id");
        theTable.AddLinkColumn(linkColumn1);

        if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.EDIT))
        {
            DataTableLinkColumn linkColumn2 = new DataTableLinkColumn("adm_users_domains_new.aspx", "Edit", "");
            linkColumn2.AddQueryStringValue("domain_id", "field=id");
            theTable.AddLinkColumn(linkColumn2);
        }
        if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.REMOVE))
        {
            DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_remove.aspx", "Delete", "STATUS=1;STATUS=3");
            linkColumn.AddQueryStringValue("id", "field=id");
            linkColumn.AddQueryStringValue("table", "domains");
            linkColumn.AddQueryStringValue("confirm", "true");
            linkColumn.AddQueryStringValue("main_menu", "2");
            linkColumn.AddQueryStringValue("sub_menu", "3");
            linkColumn.AddQueryStringValue("rep_field", "username");
            linkColumn.AddQueryStringValue("rep_name", "Username");
            linkColumn.AddQueryStringValue("db", "users_connection");
            theTable.AddLinkColumn(linkColumn);
        }
    }

    public string GetPageContent(string sOrderBy, string sPageNum, string search_user_name, string search_domain_id)
    {
        if (search_user_name != "")
            Session["search_user_name"] = search_user_name.Replace("'", "''");
        else if (Session["search_save"] == null)
            Session["search_user_name"] = "";

        if (search_domain_id != "")
            Session["search_domain_id"] = search_domain_id.Replace("'", "''");
        else if (Session["search_save"] == null)
            Session["search_domain_id"] = "";
        string sOldOrderBy = "";
        if (Session["order_by"] != null)
            sOldOrderBy = Session["order_by"].ToString();
        DBTableWebEditor theTable = new DBTableWebEditor(true, true, false, "", "adm_table_header", "adm_table_cell", "adm_table_alt_cell", "adm_table_link", "adm_table_pager", "adm_table", sOldOrderBy, 50);
        FillTheTableEditor(ref theTable, sOrderBy);

        string sTable = theTable.GetPageHTML(int.Parse(sPageNum), sOrderBy);
        theTable.Finish();
        theTable = null;
        return sTable;
    }

    public void GetHeader()
    {
        Response.Write(PageUtils.GetPreHeader() + " : Domains");
    }
}
