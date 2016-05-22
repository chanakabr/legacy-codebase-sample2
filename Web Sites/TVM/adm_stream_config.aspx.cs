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

public partial class adm_stream_config : System.Web.UI.Page
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
        theTable += "select sc.status, sc.id as 'CDN ID', sc.alias as 'SystemName', sc.STREAMING_COMPANY_NAME as 'Name' ,sc.id, case when sc.is_active is null then 1 else is_active end 'is_active', case when len(adapter_url)>0 then 'Yes' else 'No' end 'Is_Adapter' from streaming_companies sc where sc.status<>2 and";
        theTable += ODBCWrapper.Parameter.NEW_PARAM("sc.group_id", "=", nGroupID);
        /*
        if (LoginManager.GetLoginGroupID() == 1)
            theTable += "g.group_name as 'Group',";
        theTable += " sc.STREAMING_COMPANY_NAME as 'Name' ,sc.id,  from streaming_companies sc";
        if (LoginManager.GetLoginGroupID() == 1)
            theTable += ",groups g";
        theTable += "where sc.status<>2 ";
        if (LoginManager.GetLoginGroupID() == 1)
            theTable += " and g.id=sc.group_id";
        else
        {
            theTable += " and ";
            theTable += ODBCWrapper.Parameter.NEW_PARAM("sc.group_id", "=", nGroupID);
        }
        */
        if (sOrderBy != "")
        {
            theTable += " order by ";
            theTable += sOrderBy;
        }
        //else if (LoginManager.GetLoginGroupID() == 1)
        //    theTable += " order by g.group_name";
        theTable.AddHiddenField("ID");
        theTable.AddHiddenField("status");
        theTable.AddHiddenField("is_active");
        //if (LoginManager.GetLoginGroupID() == 1)
        //    theTable.AddOrderByColumn("Group", "g.GROUP_NAME");
        theTable.AddOrderByColumn("Name", "sc.STREAMING_COMPANY_NAME");
        theTable.AddOrderByColumn("Is_Adapter", "Is_Adapter");        
        theTable.AddActivationField("streaming_companies");

        DataTableLinkColumn linkColumnKeParams = new DataTableLinkColumn("adm_stream_config_settings.aspx", "Settings", "");
        linkColumnKeParams.AddQueryStringValue("cdn_adapter_id", "field=id");
        linkColumnKeParams.AddQueryCounterValue("select count(*) as val from streaming_companies_settings where ( status=1 or status = 4 )  and adapter_id=", "field=id");
        theTable.AddLinkColumn(linkColumnKeParams);

        if (PageUtils.IsTvinciUser() == true)
        {
            if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.EDIT))
            {
                DataTableLinkColumn linkColumn1 = new DataTableLinkColumn("adm_stream_config_new.aspx", "Edit", "");
                linkColumn1.AddQueryStringValue("stream_company_id", "field=id");
                theTable.AddLinkColumn(linkColumn1);
            }

            if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.REMOVE))
            {
                DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_remove.aspx", "Delete", "STATUS=1;STATUS=3");
                linkColumn.AddQueryStringValue("id", "field=id");
                linkColumn.AddQueryStringValue("table", "streaming_companies");
                linkColumn.AddQueryStringValue("confirm", "true");
                linkColumn.AddQueryStringValue("main_menu", "4");
                linkColumn.AddQueryStringValue("sub_menu", "1");
                linkColumn.AddQueryStringValue("rep_field", "STREAMING_COMPANY_NAME");
                linkColumn.AddQueryStringValue("rep_name", "ων");
                theTable.AddLinkColumn(linkColumn);
            }

            if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.PUBLISH))
            {
                DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_confirm.aspx", "Confirm", "STATUS=3;STATUS=4");
                linkColumn.AddQueryStringValue("id", "field=id");
                linkColumn.AddQueryStringValue("table", "streaming_companies");
                linkColumn.AddQueryStringValue("confirm", "true");
                linkColumn.AddQueryStringValue("main_menu", "4");
                linkColumn.AddQueryStringValue("sub_menu", "1");
                linkColumn.AddQueryStringValue("rep_field", "STREAMING_COMPANY_NAME");
                linkColumn.AddQueryStringValue("rep_name", "ων");
                theTable.AddLinkColumn(linkColumn);
            }

            if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.PUBLISH))
            {
                DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_confirm.aspx", "Cancel", "STATUS=3;STATUS=4");
                linkColumn.AddQueryStringValue("id", "field=id");
                linkColumn.AddQueryStringValue("table", "streaming_companies");
                linkColumn.AddQueryStringValue("confirm", "false");
                linkColumn.AddQueryStringValue("main_menu", "4");
                linkColumn.AddQueryStringValue("sub_menu", "1");
                linkColumn.AddQueryStringValue("rep_field", "STREAMING_COMPANY_NAME");
                linkColumn.AddQueryStringValue("rep_name", "ων");
                theTable.AddLinkColumn(linkColumn);
            }
        }
    }

    public void GetHeader()
    {
        Response.Write(PageUtils.GetPreHeader() + ": CDNs");
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
