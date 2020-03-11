using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TVinciShared;

public partial class adm_groups_mail_rules_new : System.Web.UI.Page
{
    protected string m_sMenu;
    protected string m_sSubMenu;
    protected string m_sLangMenu;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (AMS.Web.RemoteScripting.InvokeMethod(this))
        {
            Response.Expires = -1;
            return;
        }
        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");

        if (!IsPostBack)
        {
            if (Request.QueryString["ruleType"] != null && Request.QueryString["ruleType"].ToString() != "")
            {
                Session["ruleType"] = int.Parse(Request.QueryString["ruleType"].ToString());
            }
            else
            {
                Session["ruleType"] = 0;
            }
            if (Request.QueryString["rule_id"] != null && Request.QueryString["rule_id"].ToString() != "")
            {
                Session["rule_id"] = int.Parse(Request.QueryString["rule_id"].ToString());
            }
            else
            {
                Session["rule_id"] = 0;
            }
            if (Request.QueryString["submited"] != null && Request.QueryString["submited"].ToString() == "1")
                DBManipulator.DoTheWork();

        }
    }

    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
    }

    public void GetHeader()
    {
        string sRet = PageUtils.GetPreHeader() + ": Device Lmitation Module";
        if (Session["rule_id"] != null && Session["rule_id"].ToString() != "" && Session["rule_id"].ToString() != "0")
            sRet += " - Edit";
        else
            sRet += " - New";
        Response.Write(sRet);
    }

    public string GetPageContent(string sOrderBy, string sPageNum)
    {
        if (Session["error_msg"] != null && Session["error_msg"].ToString() != "")
        {
            Session["error_msg"] = "";
            return Session["last_page_html"].ToString();
        }
        object t = null; ;
        if (Session["rule_id"] != null && Session["rule_id"].ToString() != "" && int.Parse(Session["rule_id"].ToString()) != 0)
            t = Session["rule_id"];
        string sBack = "adm_groups_mail_rules.aspx?search_save=1";
        DBRecordWebEditor theRecord = new DBRecordWebEditor("groups_mail_rules", "adm_table_pager", sBack, "", "ID", t, sBack, "");

        DataRecordShortTextField dr_Name = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_Name.Initialize("Name", "adm_table_header_nbg", "FormInput", "Name", true);
        theRecord.AddRecord(dr_Name);

        DataRecordShortTextField dr_MailSubject = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_MailSubject.Initialize("Mail Subject", "adm_table_header_nbg", "FormInput", "Mail_Subject", true);
        theRecord.AddRecord(dr_MailSubject);

        DataRecordShortTextField dr_MailFrom = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_MailFrom.Initialize("Mail From", "adm_table_header_nbg", "FormInput", "Mail_From", true);
        theRecord.AddRecord(dr_MailFrom);

        DataRecordShortTextField dr_TemplateName = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_TemplateName.Initialize("Template Name", "adm_table_header_nbg", "FormInput", "Template_Name", true);
        theRecord.AddRecord(dr_TemplateName);

        int ruleType = 0;
        int.TryParse(Session["ruleType"].ToString(), out ruleType);

        if (ruleType != 0)
        {
            if (ruleType != 2 && ruleType != 6 && ruleType != 8)
            {
                DataRecordDropDownField dr_frequency = new DataRecordDropDownField("lu_min_periods", "Description", "ID", "ID>", " 1440", 60, false);
                dr_frequency.Initialize("Frequency", "adm_table_header_nbg", "FormInput", "min_limit_id", true);
                theRecord.AddRecord(dr_frequency);
            }

            if (ruleType == 4 || ruleType == 5)
            {
                DataRecordDropDownField dr_desc = new DataRecordDropDownField("Pricing.dbo.subscription_names", "Description", "subscription_id", "group_id", LoginManager.GetLoginGroupID(), 60, false);
                dr_desc.Initialize("Subscription type", "adm_table_header_nbg", "FormInput", "subscription_id", true);
                theRecord.AddRecord(dr_desc);
            }
        }

        if (ruleType != 8)
        {
            DataRecordDateTimeField dr_startDate = new DataRecordDateTimeField(true);
            dr_startDate.Initialize("Start", "adm_table_header_nbg", "FormInput", "start_date", true);
            theRecord.AddRecord(dr_startDate);

            DataRecordDateTimeField dr_endDate = new DataRecordDateTimeField(true);
            dr_endDate.Initialize("End", "adm_table_header_nbg", "FormInput", "end_date", false);
            theRecord.AddRecord(dr_endDate);

        }

        DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
        dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", false);
        dr_groups.SetValue(LoginManager.GetLoginGroupID().ToString());
        theRecord.AddRecord(dr_groups);

        DataRecordShortIntField dr_RuleType = new DataRecordShortIntField(false, 9, 9);
        dr_RuleType.Initialize("RuleType", "adm_table_header_nbg", "FormInput", "Type", false);
        dr_RuleType.SetValue(ruleType.ToString());
        theRecord.AddRecord(dr_RuleType);


        string sTable = theRecord.GetTableHTML("adm_groups_mail_rules_new.aspx?submited=1");

        return sTable;
    }

}