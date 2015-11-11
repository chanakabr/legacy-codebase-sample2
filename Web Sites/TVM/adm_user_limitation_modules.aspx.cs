using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using KLogMonitor;
using TVinciShared;
using System.Reflection;

public partial class adm_user_limitation_modules : System.Web.UI.Page
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
        theTable += "select a.is_active, a.id as id, a.NAME as 'Name', a.user_max_limit as 'Limit',  lmp.DESCRIPTION as 'Frequency', a.status from groups_device_limitation_modules a WITH (NOLOCK) " + 
                    "left join lu_min_periods lmp WITH (NOLOCK) on a.user_freq_period_id = lmp.ID where ";

        theTable += ODBCWrapper.Parameter.NEW_PARAM("a.group_id", "=", nGroupID);
        theTable += "and (";
        theTable += ODBCWrapper.Parameter.NEW_PARAM("a.STATUS", "=", 1);
        theTable += "or ";
        theTable += ODBCWrapper.Parameter.NEW_PARAM("a.STATUS", "=", 4);
        theTable += ")";
        if (sOrderBy != "")
        {
            theTable += " order by ";
            theTable += sOrderBy;
        }
        theTable.AddHiddenField("ID");
        theTable.AddHiddenField("status");
        theTable.AddActivationField("groups_device_limitation_modules", "adm_user_limitation_modules.aspx");
        theTable.AddHiddenField("is_active");

        //DataTableLinkColumn linkColumn1 = new DataTableLinkColumn("adm_user_management.aspx", "User Limitations", "");
        //linkColumn1.AddQueryStringValue("limit_module_id", "field=id");
        //linkColumn1.AddQueryCounterValue("select count(*) as val from groups_device_families WITH (NOLOCK) where status=1 and is_active=1 and group_id=" + 
        //                                LoginManager.GetLoginGroupID() + " and device_family_id=", "field=id");
        //theTable.AddLinkColumn(linkColumn1);

        if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.EDIT))
        {
            DataTableLinkColumn linkColumn2 = new DataTableLinkColumn("adm_user_limitation_modules_new.aspx", "Edit", "");
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
        Response.Write(PageUtils.GetPreHeader() + " : User Limitations");
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
