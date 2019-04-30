using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TVinciShared;

public partial class adm_ads_cdns_config : System.Web.UI.Page
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
            m_sMenu = TVinciShared.Menu.GetMainMenu(4, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 2, false);
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
        theTable += "select ac.status,ac.id as 'CDN ID' ,";
        if (LoginManager.GetLoginGroupID() == 1)
            theTable += "g.group_name as 'Group',";
        theTable += " ac.ADS_COMPANY_NAME as 'Name' ,ac.id, ac.COMMERCIAL_URL as 'Base URL',oct.DESCRIPTION as 'Action Code',lcs.description as 'State' from ads_companies ac,lu_outer_comm_types oct,lu_content_status lcs";
        if (LoginManager.GetLoginGroupID() == 1)
            theTable += ",groups g";
        theTable += "where oct.id=ac.COMMERCIAL_TYPE_ID and lcs.id=ac.status and ac.status<>2 ";
        if (LoginManager.GetLoginGroupID() == 1)
            theTable += " and g.id=ac.group_id";
        else
        {
            theTable += " and ";
            theTable += ODBCWrapper.Parameter.NEW_PARAM("ac.group_id", "=", nGroupID);
        }
        if (sOrderBy != "")
        {
            theTable += " order by ";
            theTable += sOrderBy;
        }
        else if (LoginManager.GetLoginGroupID() == 1)
            theTable += " order by g.group_name";
        theTable.AddHiddenField("ID");
        theTable.AddHiddenField("status");
        if (LoginManager.GetLoginGroupID() == 1)
            theTable.AddOrderByColumn("Group", "g.GROUP_NAME");
        theTable.AddOrderByColumn("Name", "ac.ADS_COMPANY_NAME");
        theTable.AddOrderByColumn("State", "lcs.description");

        if (PageUtils.IsTvinciUser() == true)
        {
            if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.EDIT))
            {
                DataTableLinkColumn linkColumn1 = new DataTableLinkColumn("adm_ads_cdns_config_new.aspx", "Edit", "");
                linkColumn1.AddQueryStringValue("ads_company_id", "field=id");
                theTable.AddLinkColumn(linkColumn1);
            }

            if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.REMOVE))
            {
                DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_remove.aspx", "Delete", "STATUS=1;STATUS=3");
                linkColumn.AddQueryStringValue("id", "field=id");
                linkColumn.AddQueryStringValue("table", "ads_companies");
                linkColumn.AddQueryStringValue("confirm", "true");
                linkColumn.AddQueryStringValue("main_menu", "4");
                linkColumn.AddQueryStringValue("sub_menu", "2");
                linkColumn.AddQueryStringValue("rep_field", "ADS_COMPANY_NAME");
                linkColumn.AddQueryStringValue("rep_name", "שם");
                theTable.AddLinkColumn(linkColumn);
            }

            if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.PUBLISH))
            {
                DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_confirm.aspx", "Confirm", "STATUS=3;STATUS=4");
                linkColumn.AddQueryStringValue("id", "field=id");
                linkColumn.AddQueryStringValue("table", "ads_companies");
                linkColumn.AddQueryStringValue("confirm", "true");
                linkColumn.AddQueryStringValue("main_menu", "4");
                linkColumn.AddQueryStringValue("sub_menu", "2");
                linkColumn.AddQueryStringValue("rep_field", "ADS_COMPANY_NAME");
                linkColumn.AddQueryStringValue("rep_name", "שם");
                theTable.AddLinkColumn(linkColumn);
            }

            if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.PUBLISH))
            {
                DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_confirm.aspx", "Cancel", "STATUS=3;STATUS=4");
                linkColumn.AddQueryStringValue("id", "field=id");
                linkColumn.AddQueryStringValue("table", "ads_companies");
                linkColumn.AddQueryStringValue("confirm", "false");
                linkColumn.AddQueryStringValue("main_menu", "4");
                linkColumn.AddQueryStringValue("sub_menu", "2");
                linkColumn.AddQueryStringValue("rep_field", "ADS_COMPANY_NAME");
                linkColumn.AddQueryStringValue("rep_name", "שם");
                theTable.AddLinkColumn(linkColumn);
            }
        }
    }

    public void GetHeader()
    {
        Response.Write(PageUtils.GetPreHeader() + ": Ads CDNs");
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
}
