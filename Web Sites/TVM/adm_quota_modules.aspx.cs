using ApiObjects;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TVinciShared;

public partial class adm_quota_modules : System.Web.UI.Page
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
        theTable += "select id, is_active, name , quota_in_minutes , status ";
        theTable += "from quota_modules  with (nolock) ";
        theTable += " where ";
        theTable += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
        theTable += " and ";
        theTable += ODBCWrapper.Parameter.NEW_PARAM("status", "<>", 2);
        if (sOrderBy.Length > 0)
        {
            theTable += " order by ";
            theTable += sOrderBy;
        }
        theTable.AddHiddenField("ID");
        theTable.AddHiddenField("status");
        //theTable.AddActivationField("is_active");
        theTable.AddHiddenField("is_active");

        theTable.AddActivationField("quota_modules", "adm_quota_modules.aspx");
       
        if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.EDIT))
        {
            DataTableLinkColumn linkColumn2 = new DataTableLinkColumn("adm_quota_modules_new.aspx", "Edit", "");
            linkColumn2.AddQueryStringValue("quota_id", "field=id");
            theTable.AddLinkColumn(linkColumn2);
        }

        if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.REMOVE))
        {
            DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_remove.aspx", "Delete", "STATUS=1;STATUS=3");
            linkColumn.AddQueryStringValue("id", "field=id");
            linkColumn.AddQueryStringValue("table", "quota_modules");
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
            linkColumn.AddQueryStringValue("table", "quota_modules");
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
            linkColumn.AddQueryStringValue("table", "quota_modules");
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
        Response.Write(PageUtils.GetPreHeader() + " : Quota Modules");
    }


    public void UpdateOnOffStatus(string theTableName, string sID, string sStatus)
    {
        Int32 nGroupID = LoginManager.GetLoginGroupID();
        eAction eAction;

        int nAction = int.Parse(sStatus);
        int nId = int.Parse(sID);
        List<int> idsToUpdate = new List<int>();
        if (nId != 0)
        {
            idsToUpdate.Add(nId);
        }

        if (nAction == 0)
        {
            eAction = eAction.Delete;
        }
        else // status sent is 1
        {
            eAction = eAction.Update;
        }     
    }
}