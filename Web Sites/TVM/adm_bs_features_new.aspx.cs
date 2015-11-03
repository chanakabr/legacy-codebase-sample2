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

public partial class adm_bs_features_new : System.Web.UI.Page
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
                Int32 nBugID = int.Parse(Session["bs_bug_id"].ToString());
                bool bSendMailNew = false;
                if (nBugID == 0)
                    bSendMailNew = true;
                nBugID = DBManipulator.DoTheWork();
                if (bSendMailNew == true)
                {
                    PageUtils.SendBugMail(nBugID, "New", "FeatureReport.html", true);
                    //PageUtils.SendBugMail(nBugID, "New", "FeatureReportToClient.html", true);
                }
                else
                {
                    PageUtils.SendBugMail(nBugID, "Update", "FeatureReport.html", true);
                    //PageUtils.SendBugMail(nBugID, "Update", "FeatureReportToClient.html", true);
                }
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
                Session["bs_bug_id"] = "0";
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
            if (Session["bs_bug_id"] != null && Session["bs_bug_id"].ToString() != "" && int.Parse(Session["bs_bug_id"].ToString()) != 0)
                Response.Write(PageUtils.GetPreHeader() + ": Features System: Project: " + PageUtils.GetTableSingleVal("bs_projects", "NAME", int.Parse(Session["project_id"].ToString())).ToString() + ": Feature: " + PageUtils.GetTableSingleVal("bs_project_bugs", "NAME", int.Parse(Session["bs_bug_id"].ToString())).ToString() + " (Status: " + GetCurrentStatsus() + ")");
            else
                Response.Write(PageUtils.GetPreHeader() + ": Features System: " + PageUtils.GetTableSingleVal("bs_projects", "NAME", int.Parse(Session["project_id"].ToString())).ToString() + ": New");
        }
        catch (Exception ex)
        {
            log.Error("", ex);
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

        DataRecordShortTextField dr_name = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_name.Initialize("Short Description", "adm_table_header_nbg", "FormInput", "Name", true);
        theRecord.AddRecord(dr_name);

        DataRecordLongTextField dr_description_for_client = new DataRecordLongTextField("ltr", true, 120, 6);
        dr_description_for_client.Initialize("Client description", "adm_table_header_nbg", "FormInput", "DESCRIPTION_TO_CLIENT", true);
        theRecord.AddRecord(dr_description_for_client);

        if (Session["project_id"].ToString() == "-1")
        {
            DataRecordDropDownField dr_project_id = new DataRecordDropDownField("bs_projects", "NAME", "id", "", null, 60, false);
            dr_project_id.Initialize("Project", "adm_table_header_nbg", "FormInput", "project_id", false);
            dr_project_id.SetOrderBy("ID");
            dr_project_id.SetWhereString("IS_ACTIVE=1 and status=1");
            theRecord.AddRecord(dr_project_id);
        }

        DataRecordDropDownField dr_department = new DataRecordDropDownField("lu_bugs_fields", "description", "id", "", null, 60, false);
        dr_department.Initialize("Department", "adm_table_header_nbg", "FormInput", "BUG_FIELD_ID", false);
        theRecord.AddRecord(dr_department);

        DataRecordDropDownField dr_savirity = new DataRecordDropDownField("lu_savirity", "description", "id", "", null, 60, false);
        dr_savirity.Initialize("Savirity", "adm_table_header_nbg", "FormInput", "SAVIRITY_ID", false);
        theRecord.AddRecord(dr_savirity);

        DataRecordLongTextField dr_description = new DataRecordLongTextField("ltr", true, 120, 6);
        dr_description.Initialize("Description", "adm_table_header_nbg", "FormInput", "Description", false);
        theRecord.AddRecord(dr_description);

        DataRecordLongTextField dr_recreate = new DataRecordLongTextField("ltr", true, 120, 6);
        dr_recreate.Initialize("Recreate", "adm_table_header_nbg", "FormInput", "RECREATE_DESC", false);
        theRecord.AddRecord(dr_recreate);

        DataRecordUploadField dr_file1 = new DataRecordUploadField(60, "bugs", false);
        dr_file1.Initialize("Attached File 1", "adm_table_header_nbg", "FormInput", "FILE1", false);
        theRecord.AddRecord(dr_file1);

        DataRecordUploadField dr_file2 = new DataRecordUploadField(60, "bugs", false);
        dr_file2.Initialize("Attached File 2", "adm_table_header_nbg", "FormInput", "FILE2", false);
        theRecord.AddRecord(dr_file2);

        DataRecordUploadField dr_file3 = new DataRecordUploadField(60, "bugs", false);
        dr_file3.Initialize("Attached File 3", "adm_table_header_nbg", "FormInput", "FILE3", false);
        theRecord.AddRecord(dr_file3);

        DataRecordShortIntField dr_reporter = new DataRecordShortIntField(false, 9, 9);
        dr_reporter.Initialize("Bug Reporter", "adm_table_header_nbg", "FormInput", "REPORTER_ACCOUNT_ID", false);
        if (Session["bs_bug_id"].ToString() == "0")
            dr_reporter.SetValue(LoginManager.GetLoginID().ToString());
        theRecord.AddRecord(dr_reporter);

        DataRecordDropDownField dr_resp = new DataRecordDropDownField("Accounts", "USERNAME", "id", "", null, 60, false);
        dr_resp.SetWhereString("group_id=1 and status=1");
        dr_resp.Initialize("Assigned To", "adm_table_header_nbg", "FormInput", "RESPONSIBLE_ACCOUNT_ID", false);
        dr_resp.SetOrderBy("USERNAME");
        theRecord.AddRecord(dr_resp);

        if (Session["project_id"].ToString() != "-1")
        {
            DataRecordShortIntField dr_project_id = new DataRecordShortIntField(false, 9, 9);
            dr_project_id.Initialize("Media ID", "adm_table_header_nbg", "FormInput", "project_id", false);
            dr_project_id.SetValue(Session["project_id"].ToString());
            theRecord.AddRecord(dr_project_id);
        }

        DataRecordShortIntField dr_bug_type_id = new DataRecordShortIntField(false, 9, 9);
        dr_bug_type_id.Initialize("Media ID", "adm_table_header_nbg", "FormInput", "BUG_TYPE_ID", false);
        dr_bug_type_id.SetValue("2");
        theRecord.AddRecord(dr_bug_type_id);

        string sTable = theRecord.GetTableHTML("adm_bs_features_new.aspx?submited=1");
        return sTable;
    }
}
