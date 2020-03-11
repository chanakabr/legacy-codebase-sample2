using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TVinciShared;

public partial class adm_alias_mapping : System.Web.UI.Page
{
    protected string m_sMenu;
    protected string m_sSubMenu;
    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        if (LoginManager.IsPagePermitted("adm_epg_metas.aspx") == false)
            LoginManager.LogoutFromSite("login.html");
        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        if (!IsPostBack)
        {
            Int32 nMenuID = 0;
            m_sMenu = TVinciShared.Menu.GetMainMenu(5, true, ref nMenuID);
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

        theTable += " SELECT am.id, am.group_id, am.status, am.is_active, lu.name as 'alias name',  am.alias_id, am.field_id, am.type , mt.name as 'name'";
        theTable += " FROM [TVinci].[dbo].[alias_mapping] am  inner join Tvinci.dbo.EPG_metas_types mt  on	am.type = 2  and	am.field_id = mt.id and am.status <> 2 and";
        theTable +=  ODBCWrapper.Parameter.NEW_PARAM("am.group_id", "=", nGroupID);
        theTable +=  " inner join TVinci.dbo.lu_alias lu  on	lu.id = am.alias_id UNION ";
        theTable += " SELECT am.id, am.group_id, am.status, am.is_active, lu.name as 'alias name',  am.alias_id, am.field_id, am.type, tt.name as 'name' ";
        theTable += " FROM [TVinci].[dbo].[alias_mapping] am  inner join Tvinci.dbo.EPG_tags_types tt  on	am.type =3  and	am.field_id = tt.id  and am.status <> 2 and  ";
        theTable += ODBCWrapper.Parameter.NEW_PARAM("am.group_id", "=", nGroupID);
        theTable += " inner join TVinci.dbo.lu_alias lu  on	lu.id = am.alias_id  ";
       
        theTable.AddHiddenField("id");
        theTable.AddHiddenField("status");
        theTable.AddHiddenField("is_active");
        theTable.AddHiddenField("alias_id");
        theTable.AddHiddenField("field_id");
        theTable.AddHiddenField("type");
        theTable.AddHiddenField("group_id");
        
        if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.EDIT))
        {
            DataTableLinkColumn linkColumn1 = new DataTableLinkColumn("adm_alias_mapping_new.aspx", "Edit", "");
            linkColumn1.AddQueryStringValue("alias_mapping_id", "field=id");
            theTable.AddLinkColumn(linkColumn1);       
        }

        if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.REMOVE))
        {
            DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_remove.aspx", "Delete", "STATUS=1;STATUS=3");
            linkColumn.AddQueryStringValue("id", "field=id");
            linkColumn.AddQueryStringValue("table", "alias_mapping");
            linkColumn.AddQueryStringValue("confirm", "true");
            linkColumn.AddQueryStringValue("main_menu", "5");
            linkColumn.AddQueryStringValue("sub_menu", "1");
            linkColumn.AddQueryStringValue("rep_field", "username");
            linkColumn.AddQueryStringValue("rep_name", "Username");
            theTable.AddLinkColumn(linkColumn);
        }

        if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.PUBLISH))
        {
            DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_confirm.aspx", "Confirm", "STATUS=3;STATUS=4");
            linkColumn.AddQueryStringValue("id", "field=id");
            linkColumn.AddQueryStringValue("table", "alias_mapping");
            linkColumn.AddQueryStringValue("confirm", "true");
            linkColumn.AddQueryStringValue("main_menu", "5");
            linkColumn.AddQueryStringValue("sub_menu", "1");
            linkColumn.AddQueryStringValue("rep_field", "username");
            linkColumn.AddQueryStringValue("rep_name", "Username");
            theTable.AddLinkColumn(linkColumn);
        }

        if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.REMOVE))
        {
            DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_confirm.aspx", "Cancel", "STATUS=3;STATUS=4");
            linkColumn.AddQueryStringValue("id", "field=id");
            linkColumn.AddQueryStringValue("table", "alias_mapping");
            linkColumn.AddQueryStringValue("confirm", "false");
            linkColumn.AddQueryStringValue("main_menu", "5");
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

        string sTable = theTable.GetPageHTML(int.Parse(sPageNum), sOrderBy);

        theTable.Finish();
        theTable = null;
        return sTable;
    }

    public void GetHeader()
    {
        Response.Write(PageUtils.GetPreHeader() + ": Alias Mapping");
    }
}