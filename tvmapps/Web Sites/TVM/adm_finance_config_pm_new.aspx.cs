using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using TVinciShared;

public partial class adm_finance_config_pm_new : System.Web.UI.Page
{
    protected string m_sMenu;
    protected string m_sSubMenu;
    protected void Page_Load(object sender, EventArgs e)
    {
        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        Int32 nMenuID = 0;
        if (!IsPostBack)
        {
            m_sMenu = TVinciShared.Menu.GetMainMenu(2, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 3, true);
            if (Request.QueryString["fr_entity_id"] != null &&
                Request.QueryString["fr_entity_id"].ToString() != "")
            {
                Session["fr_entity_id"] = int.Parse(Request.QueryString["fr_entity_id"].ToString());

                Int32 nOwnerGroupID = int.Parse(PageUtils.GetTableSingleVal("fr_financial_entities", "group_id", int.Parse(Session["fr_entity_id"].ToString())).ToString());
                Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();
                if (nLogedInGroupID != nOwnerGroupID && PageUtils.IsTvinciUser() == false)
                {
                    LoginManager.LogoutFromSite("login.html");
                    return;
                }
            }
            else
                Session["fr_entity_id"] = 0;

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
        string sRet = PageUtils.GetPreHeader() + ": Payment methods";

        if (Session["fr_entity_id"] != null && Session["fr_entity_id"].ToString() != "" && Session["fr_entity_id"].ToString() != "0")
            sRet += " - Edit";
        else
            sRet += " - New";
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
        if (Session["fr_entity_id"] != null && Session["fr_entity_id"].ToString() != "" && int.Parse(Session["fr_entity_id"].ToString()) != 0)
            t = Session["fr_entity_id"];
        string sBack = "adm_finance_config_pm.aspx?search_save=1";
        DBRecordWebEditor theRecord = new DBRecordWebEditor("fr_financial_entities", "adm_table_pager", sBack, "", "ID", t, sBack, "");

        DataRecordShortTextField dr_d = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_d.Initialize("Name", "adm_table_header_nbg", "FormInput", "NAME", true);
        theRecord.AddRecord(dr_d);

        DataRecordLongTextField dr_description = new DataRecordLongTextField("ltr", true, 60, 4);
        dr_description.Initialize("Description", "adm_table_header_nbg", "FormInput", "DESCRIPTION", false);
        theRecord.AddRecord(dr_description);

        DataRecordDropDownField dr_billing_method = new DataRecordDropDownField("lu_billing_methods", "DESCRIPTION", "id", "", null, 60, false);
        dr_billing_method.Initialize("Billing method", "adm_table_header_nbg", "FormInput", "BILLING_METHOD_ID", true);
        theRecord.AddRecord(dr_billing_method);

        DataRecordDropDownField dr_billing_processor = new DataRecordDropDownField("lu_billing_processors", "DESCRIPTION", "value", "", null, 60, false);
        dr_billing_processor.Initialize("Billing processor", "adm_table_header_nbg", "FormInput", "BILLING_PROCESSOR_ID", true);
        theRecord.AddRecord(dr_billing_processor);

        DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
        dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", false);
        dr_groups.SetValue(LoginManager.GetLoginGroupID().ToString());
        theRecord.AddRecord(dr_groups);

        

        DataRecordShortIntField dr_et = new DataRecordShortIntField(false, 9, 9);
        dr_et.Initialize("Group", "adm_table_header_nbg", "FormInput", "ENTITY_TYPE", false);
        dr_et.SetValue("4");
        theRecord.AddRecord(dr_et);


        string sTable = theRecord.GetTableHTML("adm_finance_config_pm_new.aspx?submited=1");

        return sTable;
    }
}
