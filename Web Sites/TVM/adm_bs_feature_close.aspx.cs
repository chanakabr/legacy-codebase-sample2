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

public partial class adm_bs_feature_close : System.Web.UI.Page
{
    private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
    protected string m_sMenu;
    protected string m_sSubMenu;
    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            LoginManager.LogoutFromSite("login.html");
        else if (LoginManager.IsPagePermitted("adm_bs_projects.aspx") == false)
            LoginManager.LogoutFromSite("login.html");
        else if (LoginManager.IsActionPermittedOnPage("adm_bs_projects.aspx", LoginManager.PAGE_PERMISION_TYPE.EDIT) == false)
            LoginManager.LogoutFromSite("login.html");

        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        Int32 nMenuID = 0;
        if (!IsPostBack)
        {
            m_sMenu = TVinciShared.Menu.GetMainMenu(17, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, true);
            if (Request.QueryString["submited"] != null && Request.QueryString["submited"].ToString() == "1")
            {
                Int32 nBugID = DBManipulator.DoTheWork();
                string sUser = ODBCWrapper.Utils.GetTableSingleVal("accounts", "username", LoginManager.GetLoginID()).ToString();
                ODBCWrapper.DirectQuery updateQuery = new ODBCWrapper.DirectQuery();
                updateQuery += "update bs_project_bugs set CLOSE_DESCRIPTION=CLOSE_DESCRIPTION+'";
                updateQuery += "\r\n(Closed on " + DateUtils.GetStrFromDate(DateTime.Now) + " by: " + sUser + ")',";
                updateQuery += "REOPEN_DESCRIPTION=REOPEN_DESCRIPTION+'";
                updateQuery += "\r\n(Closed on " + DateUtils.GetStrFromDate(DateTime.Now) + " by: " + sUser + ")' , close_date=getdate() ";
                updateQuery += " where ";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", int.Parse(Session["bs_bug_id"].ToString()));
                updateQuery.Execute();
                updateQuery.Finish();
                updateQuery = null;
                PageUtils.SendBugMail(nBugID, "Close", "FeatureReport.html", true);
                //PageUtils.SendBugMail(nBugID, "Reopen", "FeatureReportToClient.html", true);
                return;
            }
            if (Request.QueryString["project_id"] != null && Request.QueryString["project_id"].ToString() != "")
                Session["project_id"] = int.Parse(Request.QueryString["project_id"].ToString());
            if (Session["project_id"] == null || Session["project_id"].ToString() == "" || Session["project_id"].ToString() == "0")
            {
                LoginManager.LogoutFromSite("login.html");
                return;
            }

            if (Request.QueryString["bs_bug_id"] != null && Request.QueryString["bs_bug_id"].ToString() != "")
            {
                Session["bs_bug_id"] = int.Parse(Request.QueryString["bs_bug_id"].ToString());
            }
            else
            {
                Session["bs_bug_id"] = "0";
                LoginManager.LogoutFromSite("login.html");
            }

            if (Request.QueryString["action"] != null && Request.QueryString["action"].ToString() != "")
            {
                Session["action"] = Request.QueryString["action"].ToString();
            }
            else
            {
                Session["action"] = "watch";
            }
        }
    }

    protected string GetCurrentStatsus()
    {
        string sRet = "";
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select lcs.description from lu_care_status lcs,bs_project_bugs bpb where bpb.CARE_STATUS=lcs.id and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("bpb.id", "=", int.Parse(Session["bs_bug_id"].ToString()));
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 ncount = selectQuery.Table("query").DefaultView.Count;
            if (ncount > 0)
            {
                sRet = selectQuery.Table("query").DefaultView[0].Row["description"].ToString();
            }
        }
        selectQuery.Finish();
        selectQuery = null;
        return sRet;
    }

    public void GetHeader()
    {
        try
        {
            if (Session["action"] != null && Session["action"].ToString() == "watch")
                Response.Write(PageUtils.GetPreHeader() + ": Features System: Project: " + PageUtils.GetTableSingleVal("bs_projects", "NAME", int.Parse(Session["project_id"].ToString())).ToString() + ": Feature: " + PageUtils.GetTableSingleVal("bs_project_bugs", "NAME", int.Parse(Session["bs_bug_id"].ToString())).ToString() + " (Status: " + GetCurrentStatsus() + ") Close Details");
            else
                Response.Write(PageUtils.GetPreHeader() + ": Features System: Project: " + PageUtils.GetTableSingleVal("bs_projects", "NAME", int.Parse(Session["project_id"].ToString())).ToString() + ": Feature: " + PageUtils.GetTableSingleVal("bs_project_bugs", "NAME", int.Parse(Session["bs_bug_id"].ToString())).ToString() + " (Status: " + GetCurrentStatsus() + ") Close Form");
        }
        catch (Exception ex)
        {
            log.Error(string.Empty, ex);
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

    public string GetPageContent(string sOrderBy, string sPageNum)
    {
        if (Session["error_msg"] != null && Session["error_msg"].ToString() != "")
        {
            Session["error_msg"] = "";
            return Session["last_page_html"].ToString();
        }
        Int32 nGroupID = LoginManager.GetLoginGroupID();
        object t = null; ;
        if (Session["bs_bug_id"] != null && Session["bs_bug_id"].ToString() != "" && int.Parse(Session["bs_bug_id"].ToString()) != 0)
            t = Session["bs_bug_id"];
        string sBack = "adm_bs_features.aspx?search_save=1&project_id=" + Session["project_id"].ToString();
        DBRecordWebEditor theRecord = new DBRecordWebEditor("bs_project_bugs", "adm_table_pager", sBack, "", "ID", t, sBack, "project_id");

        DataRecordLongTextField dr_description = new DataRecordLongTextField("ltr", true, 120, 6);
        dr_description.Initialize("Remarks", "adm_table_header_nbg", "FormInput", "CLOSE_DESCRIPTION", true);
        theRecord.AddRecord(dr_description);

        DataRecordLongTextField dr_description_for_client = new DataRecordLongTextField("ltr", true, 120, 6);
        dr_description_for_client.Initialize("Client description", "adm_table_header_nbg", "FormInput", "DESCRIPTION_TO_CLIENT", true);
        theRecord.AddRecord(dr_description_for_client);

        DataRecordShortTextField dr_version = new DataRecordShortTextField("ltr", true, 50, 50);
        dr_version.Initialize("Version", "adm_table_header_nbg", "FormInput", "CLOSE_VERSION", false);
        theRecord.AddRecord(dr_version);

        DataRecordShortIntField dr_closer = new DataRecordShortIntField(false, 9, 9);
        dr_closer.Initialize("Closer", "adm_table_header_nbg", "FormInput", "CLOSER_ACCOUNT_ID", false);
        if (Session["action"].ToString() == "close")
            dr_closer.SetValue(LoginManager.GetLoginID().ToString());
        theRecord.AddRecord(dr_closer);

        DataRecordShortIntField dr_status = new DataRecordShortIntField(false, 9, 9);
        dr_status.Initialize("Closer", "adm_table_header_nbg", "FormInput", "CARE_STATUS", false);
        if (Session["action"].ToString() == "close")
            dr_status.SetValue("4");
        theRecord.AddRecord(dr_status);



        bool bRemoveConfirm = false;
        if (Session["action"].ToString() == "watch")
            bRemoveConfirm = true;
        string sTable = theRecord.GetTableHTML("adm_bs_feature_close.aspx?submited=1", bRemoveConfirm);
        return sTable;
    }
}
