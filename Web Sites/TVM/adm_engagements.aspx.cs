using System;
using System.Collections;
using System.Reflection;
using KLogMonitor;
using TVinciShared;

public partial class adm_engagements : System.Web.UI.Page
{
    private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

    protected string m_sMenu;
    protected string m_sSubMenu;
    protected string m_sSubSubMenu;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        else if (LoginManager.IsPagePermitted("adm_engagements.aspx") == false)
            LoginManager.LogoutFromSite("login.html");
        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        if (!IsPostBack)
        {
            Int32 nMenuID = 0;
            m_sMenu = TVinciShared.Menu.GetMainMenu(23, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, false);
            if (Request.QueryString["search_save"] != null)
                Session["search_save"] = "1";
            else
                Session["search_save"] = null;

            System.Collections.SortedList sortedMenu = GetSubMenuList();
            m_sSubSubMenu = TVinciShared.Menu.GetSubMenu(sortedMenu, -1, false);
        }
    }

    private SortedList GetSubMenuList()
    {
        SortedList sortedMenu = new SortedList();
        string sButton = "Add A.UserList";
        sButton += "|";
        sButton += "adm_engagements_new.aspx?type=1";
        sortedMenu[0] = sButton;

        sButton = "Add M.UserList";
        sButton += "|";
        sButton += "adm_engagements_new.aspx?type=2";
        sortedMenu[1] = sButton;

        return sortedMenu;

    }

    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
    }

    protected void GetSubMenu()
    {
        Response.Write(m_sSubMenu);
    }

    protected void GetSubSubMenu()
    {
        Response.Write(m_sSubSubMenu);
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
        Int32 groupId = LoginManager.GetLoginGroupID();
        theTable.SetConnectionKey("notifications_connection");
        theTable += "SELECT ID ,group_id ,status, is_active, case engagement_type when 1 then 'Churn' else '' end as 'Type',";
        theTable += "case when adapter_id > 0 then 'External' else 'Manual' end as 'Source',";
        theTable += "case when adapter_id > 0 then 1 else 2 end as 'isAdapter',";
        theTable += "send_time as 'SendTime', total_number_of_recipients as '# Recipients',";
        theTable += "INTERVAL_SECONDS / CAST(3600 AS float)  as 'Interval (hours)'";
        theTable += "FROM engagements with (nolock)";
        theTable += string.Format(" Where status<>2 and group_id = {0} ", groupId);

        if (sOrderBy != "")
        {
            theTable += " order by ";
            theTable += sOrderBy;
        }
        else
        {
            theTable += " order by ID desc";
        }

        theTable.AddHiddenField("group_id");
        theTable.AddHiddenField("status");
        theTable.AddHiddenField("is_active");
        theTable.AddHiddenField("isAdapter");
        theTable.AddOrderByColumn("SendTime", "SendTime");
        theTable.AddOrderByColumn("ID", "ID");

        DataTableLinkColumn linkColumnKeParams = new DataTableLinkColumn("adm_engagements_new.aspx", "Expand", "");
        linkColumnKeParams.AddQueryStringValue("engagement_id", "field=id");
        linkColumnKeParams.AddQueryStringValue("type", "field=isAdapter");
        theTable.AddLinkColumn(linkColumnKeParams);

        /*
        if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.PUBLISH) && LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.EDIT))
        {
            theTable.AddActivationField("engagements", "adm_engagements.aspx");
        */
    }

    public string GetPageContent(string sOrderBy, string sPageNum)
    {
        if (sOrderBy != "")
            Session["order_by"] = sOrderBy;
        else if (Session["search_save"] == null)
            Session["order_by"] = "";
        else
            sOrderBy = Session["order_by"].ToString();

        string sOldOrderBy = "";
        if (Session["order_by"] != null)
            sOldOrderBy = Session["order_by"].ToString();

        DBTableWebEditor theTable = new DBTableWebEditor(true, true, false, "", "adm_table_header", "adm_table_cell", "adm_table_alt_cell", "adm_table_link", "adm_table_pager", "adm_table", sOldOrderBy, 50);
        theTable.SetConnectionKey("notifications_connection");
        FillTheTableEditor(ref theTable, sOrderBy);

        string sTable = theTable.GetPageHTML(int.Parse(sPageNum), sOrderBy);

        theTable.Finish();
        theTable = null;
        return sTable;
    }

    public void GetHeader()
    {
        Response.Write(PageUtils.GetPreHeader() + ": Engagements");
    }

    public void UpdateOnOffStatus(string theTableName, string sID, string sStatus)
    {

    }
}