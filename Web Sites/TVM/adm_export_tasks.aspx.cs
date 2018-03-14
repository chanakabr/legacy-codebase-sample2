using ConfigurationManager;
using KLogMonitor;
using System;
using System.Reflection;
using TVinciShared;

public partial class adm_export_tasks : System.Web.UI.Page
{
    private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
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
        Int32 groupID = LoginManager.GetLoginGroupID();

        theTable += "select	bet.id as 'ID', bet.name as 'Name', bet.external_key as 'Alias ', lbedt.description as 'Data Type', bet.filter as 'Filter', lbeet.description as 'Export Type', bet.frequency as 'Frequency', bet.Notification_url as 'Notification URL', bet.group_id, bet.status, bet.is_active ";
        theTable += "FROM bulk_export_tasks bet inner join lu_bulk_export_export_types lbeet on bet.EXPORT_TYPE = lbeet.id inner join lu_bulk_export_data_types lbedt on bet.DATA_TYPE = lbedt.ID ";
        theTable += "where";
        theTable += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", groupID);
        theTable += "and";
        theTable += ODBCWrapper.Parameter.NEW_PARAM("status", "<>", 2);
        theTable += " order by id";

        theTable.AddHiddenField("is_active");
        theTable.AddActivationField("bulk_export_tasks", "adm_export_tasks.aspx");
        theTable.AddHiddenField("status");
        theTable.AddHiddenField("group_id");

        if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.EDIT))
        {
            DataTableLinkColumn linkColumn1 = new DataTableLinkColumn("adm_export_tasks_vod_types.aspx", "VOD Types", "Data Type=VOD");
            linkColumn1.AddQueryStringValue("task_id", "field=id");
            theTable.AddLinkColumn(linkColumn1);
        }

        if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.EDIT))
        {
            DataTableLinkColumn linkColumn1 = new DataTableLinkColumn("adm_export_tasks_new.aspx", "Edit", "");
            linkColumn1.AddQueryStringValue("export_task_id", "field=id");
            theTable.AddLinkColumn(linkColumn1);
        }

        if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.REMOVE))
        {
            DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_remove.aspx", "Delete", "STATUS=1;STATUS=3");
            linkColumn.AddQueryStringValue("id", "field=id");
            linkColumn.AddQueryStringValue("table", "bulk_export_tasks");
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
            linkColumn.AddQueryStringValue("table", "bulk_export_tasks");
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
            linkColumn.AddQueryStringValue("table", "bulk_export_tasks");
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

        string sTable = theTable.GetPageHTML(int.Parse(sPageNum), sOrderBy);
        theTable.Finish();
        theTable = null;
        return sTable;
    }

    public void GetHeader()
    {
        Response.Write(PageUtils.GetPreHeader() + ": " + "Export Tasks");
    }

    public void UpdateOnOffStatus(string theTableName, string sID, string sStatus)
    {
        int nAction = int.Parse(sStatus);
        
        int nId = int.Parse(sID);
        
        string sWSUserName = "";
        string sWSPass = "";
        string sWSURL;

        if (nAction != 0)
        {
            //send message to rabbit
            try
            {
                // insert new message to tasks queue (for celery)
                apiWS.API m = new apiWS.API();
                sWSURL = ApplicationConfiguration.WebServicesConfiguration.Api.URL.Value;

                if (sWSURL != "")
                    m.Url = sWSURL;
                sWSUserName = "";
                sWSPass = "";

                TVinciShared.WS_Utils.GetWSUNPass(LoginManager.GetLoginGroupID(), "EnqueueExportTask", "api", "1.1.1.1", ref sWSUserName, ref sWSPass);

                m.EnqueueExportTask(sWSUserName, sWSPass, nId);
            }
            catch (Exception ex)
            {
                log.Error("Error - " + string.Format("EnqueueExportTask in ws_api failed, taskId = {0}, ex = {1}", nId, ex), ex);
            }
        }         
    }
}