using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TVinciShared;
using Financial;
using System.Text;
using System.IO;
using System.Globalization;

public partial class adm_fr_reports_new : System.Web.UI.Page
{

    protected string m_sMenu;
    protected string m_sSubMenu;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        int nMenuID = 0;
        if (!Page.IsPostBack)
        {
            m_sMenu = TVinciShared.Menu.GetMainMenu(14, true, ref nMenuID, "adm_fr_reports.aspx");
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, true);
            if (!string.IsNullOrEmpty(Request.QueryString["submitted"]) && Request.QueryString["submitted"].Equals("1"))
            {
                string startDateStr = Request.Form["0_val"];
                string endDateStr = Request.Form["1_val"];
                string groupIDStr = Request.Form["2_val"];

                Int32 nGroupID = int.Parse(groupIDStr);

                DateTime startDate = DateUtils.GetDateFromStr(startDateStr);
                DateTime endDate = DateUtils.GetDateFromStr(endDateStr);

                //Call Report Generator!               
                ReportGenerator( startDate, endDate, nGroupID);

                Response.Redirect("adm_fr_reports.aspx");
            }
        }
    }

    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
    }

    public void GetHeader()
    {
        string sRet = PageUtils.GetPreHeader() + ": Generate Financial Report";
       
        Response.Write(sRet);
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
        object t = null; ;
        
        string sBack = string.Empty;
        DBRecordWebEditor theRecord = new DBRecordWebEditor("", "adm_table_pager", "adm_fr_reports.aspx", "", "ID", t, sBack, "");

        

        DataRecordDateTimeField dr_start_date = new DataRecordDateTimeField(true);
        dr_start_date.Initialize("Report Start Date", "adm_table_header_nbg", "FormInput", "START_DATE", false);
        DateTime startDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        dr_start_date.SetDefault(startDate);
        theRecord.AddRecord(dr_start_date);

        DataRecordDateTimeField dr_end_date = new DataRecordDateTimeField(true);
        dr_end_date.Initialize("Report End Date", "adm_table_header_nbg", "FormInput", "END_DATE", false);
        DateTime endDate = new DateTime(DateTime.Now.Year,DateTime.Now.Month, DateTime.Now.Day );
        dr_end_date.SetDefault(endDate);
        theRecord.AddRecord(dr_end_date);

        

        DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
        dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", false);
        dr_groups.SetValue(LoginManager.GetLoginGroupID().ToString());
        theRecord.AddRecord(dr_groups);

        string sTable = theRecord.GetTableHTML("adm_fr_report_new.aspx?submited=1", true);

        return sTable;
    }

    private void ReportGenerator(DateTime dStartDate, DateTime dEndDate, Int32 nGroupID)
    {
        Int32 nRHEntityID = 0;

        if (Session["RightHolder"] != null)
        {
            nRHEntityID = int.Parse(Session["RightHolder"].ToString());
        }

        FilmoFinancialReport ffr = new FilmoFinancialReport(dStartDate, dEndDate, nGroupID, nRHEntityID);
        ffr.CreateReport(Server.MapPath(string.Empty));
    }
}