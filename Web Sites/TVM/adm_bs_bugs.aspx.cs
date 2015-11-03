using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using KLogMonitor;
using TVinciShared;

public partial class adm_bs_bugs : System.Web.UI.Page
{
    private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
    protected string m_sMenu;
    protected string m_sSubMenu;
    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        if (LoginManager.IsPagePermitted("adm_bs_projects.aspx") == false)
            LoginManager.LogoutFromSite("login.html");
        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        if (!IsPostBack)
        {
            Int32 nMenuID = 0;
            m_sMenu = TVinciShared.Menu.GetMainMenu(17, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, false);
            if (Request.QueryString["search_save"] != null)
                Session["search_save"] = "1";
            else
                Session["search_save"] = null;

            if (Request.QueryString["project_id"] != null &&
                Request.QueryString["project_id"].ToString() != "")
            {
                Session["project_id"] = int.Parse(Request.QueryString["project_id"].ToString());
            }
            else if (Session["project_id"] == null || Session["project_id"].ToString() == "" || Session["project_id"].ToString() == "0")
            {
                LoginManager.LogoutFromSite("index.html");
                return;
            }
        }
    }

    public void GetHeader()
    {
        Response.Write(PageUtils.GetPreHeader() + ": Bugs System: " + PageUtils.GetTableSingleVal("bs_projects", "NAME", int.Parse(Session["project_id"].ToString())).ToString());
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

    protected bool IsAccountPermittedToAction(string sAction, Int32 nAcoountID)
    {
        bool bRet = false;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select * from bs_permitted_accounts where ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("ACTION_DESC", "=", sAction);
        selectQuery += "and";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("ACCOUNT_ID", "=", nAcoountID);
        selectQuery += "and is_active=1";
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
                bRet = true;
        }
        selectQuery.Finish();
        selectQuery = null;
        return bRet;
    }

    protected void FillTheTableEditor(ref DBTableWebEditor theTable, string sOrderBy)
    {
        Int32 nGroupID = LoginManager.GetLoginGroupID();
        theTable += "select pb.CARE_STATUS,pb.id,pb.id as 'Bug ID',pb.NAME as 'Name' , pb.DESCRIPTION as 'Description' ,pb.REPORTER_ACCOUNT_ID,lbf.description as 'Department',pb.RESPONSIBLE_ACCOUNT_ID,pb.CLOSER_ACCOUNT_ID,lcs.description as 'Status',ls.description as 'Severity',pb.create_date as 'Create Date' from lu_bugs_fields lbf,lu_savirity ls,lu_care_status lcs,bs_project_bugs pb where ls.id=pb.savirity_id and lcs.id=pb.CARE_STATUS and " + PageUtils.GetStatusQueryPart("pb") + "and lbf.id=pb.BUG_FIELD_ID and pb.BUG_TYPE_ID=1 and ";
        theTable += ODBCWrapper.Parameter.NEW_PARAM("pb.project_id", "=", int.Parse(Session["project_id"].ToString()));
        if (Session["search_free"] != null && Session["search_free"].ToString() != "")
        {
            string sLike = "like('%" + Session["search_free"].ToString() + "%')";
            theTable += " and (pb.name " + sLike + " or pb.description " + sLike + " or pb.RECREATE_DESC " + sLike + " or pb.CLOSE_DESCRIPTION " + sLike + " or pb.REOPEN_DESCRIPTION " + sLike;
            try
            {
                Int32 nID = int.Parse(Session["search_free"].ToString());
                theTable += " or ";
                theTable += ODBCWrapper.Parameter.NEW_PARAM("pb.id", "=", nID);
            }
            catch (Exception ex)
            {
                log.Error("", ex);
            }
            theTable += ")";
        }
        if (Session["search_status"] != null && Session["search_status"].ToString() != "" && Session["search_status"].ToString() != "-1")
        {
            theTable += " and ";
            theTable += ODBCWrapper.Parameter.NEW_PARAM("pb.CARE_STATUS", "=", int.Parse(Session["search_status"].ToString()));
        }
        if (Session["search_assigned"] != null && Session["search_assigned"].ToString() != "" && Session["search_assigned"].ToString() != "-1")
        {
            theTable += " and ";
            theTable += ODBCWrapper.Parameter.NEW_PARAM("pb.RESPONSIBLE_ACCOUNT_ID", "=", int.Parse(Session["search_assigned"].ToString()));
        }
        if (Session["search_savirity"] != null && Session["search_savirity"].ToString() != "" && Session["search_savirity"].ToString() != "-1")
        {
            theTable += " and ";
            theTable += ODBCWrapper.Parameter.NEW_PARAM("pb.SAVIRITY_ID", "=", int.Parse(Session["search_savirity"].ToString()));
        }
        if (Session["search_department"] != null && Session["search_department"].ToString() != "" && Session["search_department"].ToString() != "-1")
        {
            theTable += " and ";
            theTable += ODBCWrapper.Parameter.NEW_PARAM("pb.BUG_FIELD_ID", "=", int.Parse(Session["search_department"].ToString()));
        }
        theTable.AddHiddenField("ID");
        //theTable.AddHiddenField("status");
        if (sOrderBy != "")
        {
            theTable += " order by ";
            theTable += sOrderBy;
        }
        else
            theTable += " order by id desc";

        theTable.AddHiddenField("REPORTER_ACCOUNT_ID");
        theTable.AddHiddenField("RESPONSIBLE_ACCOUNT_ID");
        theTable.AddHiddenField("CLOSER_ACCOUNT_ID");
        theTable.AddHiddenField("CARE_STATUS");

        DataTableMultiValuesColumn multi_rep_accounts = new DataTableMultiValuesColumn("Reporter", "val", "a.id", "REPORTER_ACCOUNT_ID");
        multi_rep_accounts += "select a.username as val from accounts a where ";
        theTable.AddMultiValuesColumn(multi_rep_accounts);

        DataTableMultiValuesColumn multi_resp_accounts = new DataTableMultiValuesColumn("Assignd To", "val", "a.id", "RESPONSIBLE_ACCOUNT_ID");
        multi_resp_accounts += "select a.username as val from accounts a where ";
        theTable.AddMultiValuesColumn(multi_resp_accounts);

        DataTableMultiValuesColumn multi_close_accounts = new DataTableMultiValuesColumn("Closed By", "val", "a.id", "CLOSER_ACCOUNT_ID");
        multi_close_accounts += "select a.username as val from accounts a where ";
        if (IsAccountPermittedToAction("Close", LoginManager.GetLoginID()) == true)
        {
            DataTableLinkColumn linkColumn1 = new DataTableLinkColumn("adm_bs_bug_close.aspx", "Close", "CARE_STATUS=3;CARE_STATUS=2;CARE_STATUS=1;CARE_STATUS=5;CARE_STATUS=8");
            linkColumn1.AddQueryStringValue("bs_bug_id", "field=id");
            linkColumn1.AddQueryStringValue("action", "close");
            theTable.AddLinkColumn(linkColumn1);
        }
        else
        {
            DataTableLinkColumn linkColumn1 = new DataTableLinkColumn("adm_bs_bug_close.aspx", "Fix", "CARE_STATUS=3;CARE_STATUS=2;CARE_STATUS=1;CARE_STATUS=5");
            linkColumn1.AddQueryStringValue("bs_bug_id", "field=id");
            linkColumn1.AddQueryStringValue("action", "fix");
            theTable.AddLinkColumn(linkColumn1);
        }
        {
            DataTableLinkColumn linkColumn1 = new DataTableLinkColumn("adm_bs_bug_close.aspx", "Close Details", "CARE_STATUS=4;CARE_STATUS=5");
            linkColumn1.AddQueryStringValue("bs_bug_id", "field=id");
            linkColumn1.AddQueryStringValue("action", "watch");
            theTable.AddLinkColumn(linkColumn1);
        }
        if (IsAccountPermittedToAction("ReOpen", LoginManager.GetLoginID()) == true)
        {
            DataTableLinkColumn linkColumn1 = new DataTableLinkColumn("adm_bs_bug_reopen.aspx", "Re Open", "CARE_STATUS=4");
            linkColumn1.AddQueryStringValue("bs_bug_id", "field=id");
            linkColumn1.AddQueryStringValue("action", "reopen");
            theTable.AddLinkColumn(linkColumn1);
        }
        {
            DataTableLinkColumn linkColumn1 = new DataTableLinkColumn("adm_bs_bug_reopen.aspx", "Re Open Details", "CARE_STATUS=5");
            linkColumn1.AddQueryStringValue("bs_bug_id", "field=id");
            linkColumn1.AddQueryStringValue("action", "watch");
            theTable.AddLinkColumn(linkColumn1);
        }

        if (LoginManager.IsActionPermittedOnPage("adm_bs_projects.aspx", LoginManager.PAGE_PERMISION_TYPE.EDIT))
        {
            DataTableLinkColumn linkColumn1 = new DataTableLinkColumn("adm_bs_bugs_new.aspx", "Edit", "");
            linkColumn1.AddQueryStringValue("bs_bug_id", "field=id");
            theTable.AddLinkColumn(linkColumn1);
        }

    }

    public string GetPageContent(string sOrderBy, string sPageNum, string search_status, string search_assigned,
        string search_savirity, string search_department, string search_free)
    {
        if (search_status != "")
            Session["search_status"] = search_status.Replace("'", "''");
        else if (Session["search_save"] == null)
            Session["search_status"] = "";

        if (search_assigned != "-1")
            Session["search_assigned"] = search_assigned;
        else if (Session["search_save"] == null)
            Session["search_assigned"] = "";

        if (search_savirity != "-1")
            Session["search_savirity"] = search_savirity;
        else if (Session["search_save"] == null)
            Session["search_savirity"] = "";

        if (search_department != "-1")
            Session["search_department"] = search_department;
        else if (Session["search_save"] == null)
            Session["search_department"] = "";

        if (search_free != "")
            Session["search_free"] = search_free.Replace("'", "''");
        else if (Session["search_save"] == null)
            Session["search_free"] = "";

        string sOldOrderBy = "";
        if (Session["order_by"] != null)
            sOldOrderBy = Session["order_by"].ToString();

        DBTableWebEditor theTable = new DBTableWebEditor(true, true, true, "", "adm_table_header", "adm_table_cell", "adm_table_alt_cell", "adm_table_link", "adm_table_pager", "adm_table", sOldOrderBy, 50);
        FillTheTableEditor(ref theTable, sOrderBy);

        string sTable = theTable.GetPageHTML(int.Parse(sPageNum), sOrderBy, false);
        Session["ContentPage"] = "adm_bs_projects.aspx";
        theTable.Finish();
        theTable = null;
        return sTable;
    }
}
