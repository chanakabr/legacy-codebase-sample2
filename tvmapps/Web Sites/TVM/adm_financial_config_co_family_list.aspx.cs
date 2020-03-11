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

public partial class adm_financial_config_co_family_list : System.Web.UI.Page
{
    protected string m_sMenu;
    protected string m_sSubMenu;
    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        if (LoginManager.IsPagePermitted("adm_finance_config_co.aspx") == false)
            LoginManager.LogoutFromSite("login.html");
        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        if (!IsPostBack)
        {
            Int32 nMenuID = 0;
            m_sMenu = TVinciShared.Menu.GetMainMenu(9, true, ref nMenuID, "adm_finance_config_co.aspx");
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, false);

            if (Request.QueryString["fr_parent_entity_id"] != null &&
                Request.QueryString["fr_parent_entity_id"].ToString() != "")
            {
                Session["fr_parent_entity_id"] = int.Parse(Request.QueryString["fr_parent_entity_id"].ToString());
                Int32 nOwnerGroupID = int.Parse(PageUtils.GetTableSingleVal("fr_financial_entities", "group_id", int.Parse(Session["fr_parent_entity_id"].ToString())).ToString());
                Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();
                if (nLogedInGroupID != nOwnerGroupID && PageUtils.IsTvinciUser() == false)
                {
                    LoginManager.LogoutFromSite("login.html");
                    return;
                }
            }
            else if (Session["fr_parent_entity_id"] == null || Session["fr_parent_entity_id"].ToString() == "" || Session["fr_parent_entity_id"].ToString() == "0")
            {
                LoginManager.LogoutFromSite("index.html");
                return;
            }

            Int32 nGroupID = LoginManager.GetLoginGroupID();
            if (Request.QueryString["search_save"] != null)
                Session["search_save"] = "1";
            else
                Session["search_save"] = null;
        }
    }

    public void GetHeader()
    {
        Response.Write(PageUtils.GetPreHeader() + ": Content owners family management");
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
        theTable += "select t.status,t.id,t.name as 'Name',t.description as 'Description' from fr_financial_entities t";
        theTable += "where status<>2 and ";
        theTable += ODBCWrapper.Parameter.NEW_PARAM("PARENT_ENTITY_ID" , "=" , int.Parse(Session["fr_parent_entity_id"].ToString()));
        theTable +=" and ";
        theTable += ODBCWrapper.Parameter.NEW_PARAM("t.entity_type", "=", 1);
        if (Session["search_free"] != null && Session["search_free"].ToString() != "")
        {
            string sLike = "like('%" + Session["search_free"].ToString().Replace("'", "''") + "%')";
            theTable += " and (t.DESCRIPTION " + sLike + ")";
        }
        theTable += "and";
        theTable += ODBCWrapper.Parameter.NEW_PARAM("t.group_id", "=", nGroupID);
        if (sOrderBy != "")
        {
            theTable += " order by ";
            theTable += sOrderBy;
        }
        else
            theTable += " order by t.description";
        theTable.AddHiddenField("ID");
        theTable.AddHiddenField("status");
        theTable.AddTechDetails("fr_financial_entities");
        theTable.AddOrderByColumn("Name", "t.Name");
        theTable.AddOrderByColumn("Description", "t.Description");
        if (LoginManager.IsActionPermittedOnPage("adm_finance_config_co.aspx" , LoginManager.PAGE_PERMISION_TYPE.EDIT))
        {
            DataTableLinkColumn linkColumn1 = new DataTableLinkColumn("adm_financial_config_co_family_list_new.aspx", "Edit", "");
            linkColumn1.AddQueryStringValue("fr_entity_id", "field=id");
            theTable.AddLinkColumn(linkColumn1);
        }
        if (LoginManager.IsActionPermittedOnPage("adm_finance_config_co.aspx" , LoginManager.PAGE_PERMISION_TYPE.REMOVE))
        {
            DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_remove.aspx", "Delete", "STATUS=1;STATUS=3");
            linkColumn.AddQueryStringValue("id", "field=id");
            linkColumn.AddQueryStringValue("table", "fr_financial_entities");
            linkColumn.AddQueryStringValue("confirm", "true");
            linkColumn.AddQueryStringValue("main_menu", "9");
            linkColumn.AddQueryStringValue("sub_menu", "1");
            linkColumn.AddQueryStringValue("rep_field", "NAME");
            linkColumn.AddQueryStringValue("rep_name", "שם");
            theTable.AddLinkColumn(linkColumn);
        }

        if (LoginManager.IsActionPermittedOnPage("adm_finance_config_co.aspx" , LoginManager.PAGE_PERMISION_TYPE.PUBLISH))
        {
            DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_confirm.aspx", "Confirm", "STATUS=3;STATUS=4");
            linkColumn.AddQueryStringValue("id", "field=id");
            linkColumn.AddQueryStringValue("table", "fr_financial_entities");
            linkColumn.AddQueryStringValue("confirm", "true");
            linkColumn.AddQueryStringValue("main_menu", "9");
            linkColumn.AddQueryStringValue("sub_menu", "1");
            linkColumn.AddQueryStringValue("rep_field", "NAME");
            linkColumn.AddQueryStringValue("rep_name", "שם");
            theTable.AddLinkColumn(linkColumn);
        }

        if (LoginManager.IsActionPermittedOnPage("adm_finance_config_co.aspx" , LoginManager.PAGE_PERMISION_TYPE.PUBLISH))
        {
            DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_confirm.aspx", "Cancel", "STATUS=3;STATUS=4");
            linkColumn.AddQueryStringValue("id", "field=id");
            linkColumn.AddQueryStringValue("table", "fr_financial_entities");
            linkColumn.AddQueryStringValue("confirm", "false");
            linkColumn.AddQueryStringValue("main_menu", "9");
            linkColumn.AddQueryStringValue("sub_menu", "1");
            linkColumn.AddQueryStringValue("rep_field", "NAME");
            linkColumn.AddQueryStringValue("rep_name", "שם");
            theTable.AddLinkColumn(linkColumn);
        }
    }

    public string GetPageContent(string sOrderBy, string sPageNum, string search_free)
    {
        if (search_free != "")
            Session["search_free"] = search_free;
        else if (Session["search_save"] == null)
            Session["search_free"] = "";

        if (sOrderBy != "")
            Session["order_by"] = sOrderBy;
        else if (Session["search_save"] == null)
            Session["order_by"] = "";
        else
            sOrderBy = Session["order_by"].ToString();

        string sOldOrderBy = "";
        if (Session["order_by"] != null)
            sOldOrderBy = Session["order_by"].ToString();

        DBTableWebEditor theTable = new DBTableWebEditor(true, true, true, "", "adm_table_header", "adm_table_cell", "adm_table_alt_cell", "adm_table_link", "adm_table_pager", "adm_table", sOldOrderBy, 50);
        FillTheTableEditor(ref theTable, sOrderBy);

        string sTable = theTable.GetPageHTML(int.Parse(sPageNum), sOrderBy);
        Session["ContentPage"] = "adm_financial_contracts_co.aspx";
        Session["LastContentPage"] = string.Format("adm_financial_config_co_family_list.aspx?fr_parent_entity_id={0}", int.Parse(Session["fr_parent_entity_id"].ToString()));
        theTable.Finish();
        theTable = null;
        return sTable;
    }
}
