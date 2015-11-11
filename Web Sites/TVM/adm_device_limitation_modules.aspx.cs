using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TVinciShared;
using System.Reflection;
using KLogMonitor;

public partial class adm_device_limitation_modules : System.Web.UI.Page
{
    private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
    protected string m_sMenu;
    protected string m_sSubMenu;
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!LoginManager.CheckLogin())
            Response.Redirect("login.html");
        if (!LoginManager.IsPagePermitted())
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
        string sOldOrderBy = string.Empty;
        DBTableWebEditor theTable = null;
        string sCSVFile = string.Empty;
        try
        {
            if (Session["order_by"] != null)
                sOldOrderBy = Session["order_by"].ToString();
            theTable = new DBTableWebEditor(true, true, false, "", "adm_table_header", "adm_table_cell", "adm_table_alt_cell", "adm_table_link", "adm_table_pager", "adm_table", sOldOrderBy, 50);
            FillTheTableEditor(ref theTable, sOldOrderBy);

            sCSVFile = theTable.OpenCSV();
        }
        finally
        {
            if (theTable != null)
            {
                theTable.Finish();
                theTable = null;
            }
        }

        return sCSVFile;
    }

    protected void FillTheTableEditor(ref DBTableWebEditor theTable, string sOrderBy)
    {
        Int32 nGroupID = LoginManager.GetLoginGroupID();
        theTable += "select a.is_active,a.id as id,a.NAME as 'Name', a.max_limit as 'Limit',  q1.DESCRIPTION as 'Frequency', a.concurrent_max_limit as 'Concurrent Limit', a.status, a.Home_network_quantity as 'Home networks limit', q2.DESCRIPTION as 'Home network frequency', env.description as 'Environment Type'";
        theTable += "from groups_device_limitation_modules a with (nolock) ";
        theTable += "left join (select lmp1.ID, lmp1.description from lu_min_periods lmp1 with (nolock)) q1 on q1.ID=a.freq_period_id ";
        theTable += "left join (select lmp2.ID, lmp2.description from lu_min_periods lmp2 with (nolock)) q2 on q2.ID=a.Home_Network_Frequency ";
        theTable += "left join lu_domain_environment env with (nolock) on env.ID = a.environment_type where ";
        theTable += ODBCWrapper.Parameter.NEW_PARAM("a.group_id", "=", nGroupID);
        theTable += "and (";
        theTable += ODBCWrapper.Parameter.NEW_PARAM("a.STATUS", "=", 1);
        theTable += "or ";
        theTable += ODBCWrapper.Parameter.NEW_PARAM("a.STATUS", "=", 4);
        theTable += ")";
        if (sOrderBy.Length > 0)
        {
            theTable += " order by ";
            theTable += sOrderBy;
        }
        theTable.AddHiddenField("ID");
        theTable.AddHiddenField("status");
        theTable.AddActivationField("groups_device_limitation_modules", "adm_device_limitation_modules.aspx");
        theTable.AddHiddenField("is_active");
        theTable.AddHiddenField("environment_type");      

        DataTableLinkColumn linkColumn1 = new DataTableLinkColumn("adm_limitation_modules.aspx", "Limitation Modules", "");
        linkColumn1.AddQueryStringValue("limit_module_id", "field=id");
        //linkColumn1.AddQueryCounterValue("select count([id]) as val from groups_device_families_limitation_modules with (nolock) where status=1 and is_active=1 and group_id=" + LoginManager.GetLoginGroupID() + " and id=", "field=id");
        theTable.AddLinkColumn(linkColumn1);

        DataTableLinkColumn linkMCRule = new DataTableLinkColumn("adm_limitation_media_concurrency_rules.aspx", "Media Concurrency Rules", "");
        linkMCRule.AddQueryStringValue("limit_module_id", "field=id");
        linkMCRule.AddQueryCounterValue("select count(*) as val from groups_device_media_concurrency_rules with (nolock) where status=1 and is_active=1 and group_id=" + LoginManager.GetLoginGroupID() + " and DEVICE_LIMITATION_ID=", "field=id");
        theTable.AddLinkColumn(linkMCRule);

        if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.EDIT))
        {
            DataTableLinkColumn linkColumn2 = new DataTableLinkColumn("adm_device_limitation_modules_new.aspx", "Edit", "");
            linkColumn2.AddQueryStringValue("limit_id", "field=id");
            theTable.AddLinkColumn(linkColumn2);
        }
        if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.REMOVE))
        {
            DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_remove.aspx", "Delete", "STATUS=1;STATUS=3");
            linkColumn.AddQueryStringValue("id", "field=id");
            linkColumn.AddQueryStringValue("table", "groups_device_limitation_modules");
            linkColumn.AddQueryStringValue("confirm", "true");
            linkColumn.AddQueryStringValue("main_menu", "2");
            linkColumn.AddQueryStringValue("sub_menu", "3");
            linkColumn.AddQueryStringValue("rep_field", "username");
            linkColumn.AddQueryStringValue("rep_name", "Username");
            theTable.AddLinkColumn(linkColumn);
        }

        if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.PUBLISH))
        {
            DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_confirm.aspx", "Confirm", "STATUS=3;STATUS=4");
            linkColumn.AddQueryStringValue("id", "field=id");
            linkColumn.AddQueryStringValue("table", "groups_device_limitation_modules");
            linkColumn.AddQueryStringValue("confirm", "true");
            linkColumn.AddQueryStringValue("main_menu", "2");
            linkColumn.AddQueryStringValue("sub_menu", "3");
            linkColumn.AddQueryStringValue("rep_field", "username");
            linkColumn.AddQueryStringValue("rep_name", "Username");
            theTable.AddLinkColumn(linkColumn);
        }

        if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.REMOVE))
        {
            DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_confirm.aspx", "Cancel", "STATUS=3;STATUS=4");
            linkColumn.AddQueryStringValue("id", "field=id");
            linkColumn.AddQueryStringValue("table", "groups_device_limitation_modules");
            linkColumn.AddQueryStringValue("confirm", "false");
            linkColumn.AddQueryStringValue("main_menu", "2");
            linkColumn.AddQueryStringValue("sub_menu", "3");
            linkColumn.AddQueryStringValue("rep_field", "username");
            linkColumn.AddQueryStringValue("rep_name", "Username");
            theTable.AddLinkColumn(linkColumn);
        }
    }

    public string GetPageContent(string sOrderBy, string sPageNum)
    {
        string sOldOrderBy = string.Empty;
        DBTableWebEditor theTable = null;
        string sTable = string.Empty;
        try
        {
            if (Session["order_by"] != null)
                sOldOrderBy = Session["order_by"].ToString();
            theTable = new DBTableWebEditor(true, true, true, "", "adm_table_header", "adm_table_cell", "adm_table_alt_cell", "adm_table_link", "adm_table_pager", "adm_table", sOldOrderBy, 50);
            FillTheTableEditor(ref theTable, sOrderBy);

            sTable = theTable.GetPageHTML(int.Parse(sPageNum), sOrderBy);
        }
        finally
        {
            if (theTable != null)
            {
                theTable.Finish();
                theTable = null;
            }
        }
        return sTable;
    }

    public void GetHeader()
    {
        Response.Write(PageUtils.GetPreHeader() + " : Device Families");
    }

    public void UpdateOnOffStatus(string theTableName, string sID, string sStatus)
    {
        int nId = int.Parse(sID);

        DomainsWS.module p;
        string sIP = "1.1.1.1";
        string sWSUserName = "";
        string sWSPass = "";
        string sWSURL;

        // delete from cache this DLM object    
        if (sStatus == "0")
        {
            p = new DomainsWS.module();
            TVinciShared.WS_Utils.GetWSUNPass(LoginManager.GetLoginGroupID(), "DLM", "domains", sIP, ref sWSUserName, ref sWSPass);
            sWSURL = TVinciShared.WS_Utils.GetTcmConfigValue("domains_ws");
            if (sWSURL != "")
                p.Url = sWSURL;
            try
            {
                DomainsWS.Status resp = p.RemoveDLM(sWSUserName, sWSPass, nId);
                log.Debug("RemoveDLM - " + string.Format("Dlm:{0}, res:{1}", nId, resp.Code));
            }
            catch (Exception ex)
            {
                log.Error("Exception - " + string.Format("Dlm:{0}, msg:{1}, st:{2}", nId, ex.Message, ex.StackTrace), ex);
            }
        }
    }
}
