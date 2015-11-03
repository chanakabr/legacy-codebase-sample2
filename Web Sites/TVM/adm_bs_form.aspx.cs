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

public partial class adm_bs_form : System.Web.UI.Page
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
            m_sMenu = TVinciShared.Menu.GetMainMenu(18, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, true);
            if (Request.QueryString["submited"] != null && Request.QueryString["submited"].ToString() == "1")
            {
                Int32 nBugID = DBManipulator.DoTheWork();
                PageUtils.SendGroupBugMail(nBugID, "New", "GroupBagReport.html");
                return;
            }
        }
    }

    public void GetHeader()
    {
        try
        {
            Response.Write(PageUtils.GetPreHeader() + ": Bugs System: New");
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
        string sBack = "adm_bs_form.aspx?search_save=1";
        DBRecordWebEditor theRecord = new DBRecordWebEditor("bs_project_bugs", "adm_table_pager", sBack, "", "ID", t, sBack, "project_id");

        DataRecordShortTextField dr_name = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_name.Initialize("Short Description", "adm_table_header_nbg", "FormInput", "Name", true);
        theRecord.AddRecord(dr_name);

        DataRecordDropDownField dr_savirity = new DataRecordDropDownField("lu_savirity", "description", "id", "", null, 60, false);
        dr_savirity.Initialize("Savirity", "adm_table_header_nbg", "FormInput", "SAVIRITY_ID", false);
        theRecord.AddRecord(dr_savirity);

        DataRecordLongTextField dr_description = new DataRecordLongTextField("ltr", true, 120, 6);
        dr_description.Initialize("Description", "adm_table_header_nbg", "FormInput", "Description", true);
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
        dr_reporter.SetValue(LoginManager.GetLoginID().ToString());
        theRecord.AddRecord(dr_reporter);

        DataRecordShortIntField dr_project_id = new DataRecordShortIntField(false, 9, 9);
        dr_project_id.Initialize("Group ID", "adm_table_header_nbg", "FormInput", "group_id", false);
        dr_project_id.SetValue(nGroupID.ToString());
        theRecord.AddRecord(dr_project_id);

        DataRecordRadioField dr_bug_type_id = new DataRecordRadioField("lu_bug_type", "description", "id", "", null);
        dr_bug_type_id.Initialize("Bug Or Feature", "adm_table_header_nbg", "FormInput", "BUG_TYPE_ID", false);
        dr_bug_type_id.SetDefault(0);
        theRecord.AddRecord(dr_bug_type_id);

        string sTable = theRecord.GetTableHTML("adm_bs_form.aspx?submited=1");
        return sTable;
    }
}
