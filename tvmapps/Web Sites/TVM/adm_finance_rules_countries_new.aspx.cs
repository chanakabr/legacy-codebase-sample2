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

public partial class adm_finance_rules_countries_new : System.Web.UI.Page
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
            if (Request.QueryString["submited"] != null && Request.QueryString["submited"].ToString() == "1")
            {
                DBManipulator.DoTheWork();
                return;
            }
            m_sMenu = TVinciShared.Menu.GetMainMenu(5, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, true);
            if (Request.QueryString["finance_block_rule_id"] != null &&
                Request.QueryString["finance_block_rule_id"].ToString() != "")
            {
                Session["finance_block_rule_id"] = int.Parse(Request.QueryString["finance_block_rule_id"].ToString());
                Int32 nOwnerGroupID = int.Parse(PageUtils.GetTableSingleVal("geo_block_types", "group_id", int.Parse(Session["finance_block_rule_id"].ToString())).ToString());
                Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();
                if (nLogedInGroupID != nOwnerGroupID && PageUtils.IsTvinciUser() == false)
                {
                    LoginManager.LogoutFromSite("login.html");
                    return;
                }
            }
            else
                Session["finance_block_rule_id"] = 0;
        }
    }

    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
    }

    public void GetHeader()
    {
        string sRet = PageUtils.GetPreHeader() + ": Geo block rules";
        if (Session["finance_block_rule_id"] != null && Session["finance_block_rule_id"].ToString() != "" && Session["finance_block_rule_id"].ToString() != "0")
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
        if (Session["finance_block_rule_id"] != null && Session["finance_block_rule_id"].ToString() != "" && int.Parse(Session["finance_block_rule_id"].ToString()) != 0)
            t = Session["finance_block_rule_id"];
        string sBack = "adm_finance_rules_countries.aspx?search_save=1";
        DBRecordWebEditor theRecord = new DBRecordWebEditor("geo_block_types", "adm_table_pager", sBack, "", "ID", t, sBack, "");

        DataRecordShortTextField dr_domain = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_domain.Initialize("Rule name", "adm_table_header_nbg", "FormInput", "NAME", true);
        theRecord.AddRecord(dr_domain);

        DataRecordRadioField dr_but_or_only = new DataRecordRadioField("lu_only_or_but", "description", "id", "", null);
        dr_but_or_only.Initialize("Rule type", "adm_table_header_nbg", "FormInput", "ONLY_OR_BUT", true);
        dr_but_or_only.SetDefault(0);
        theRecord.AddRecord(dr_but_or_only);

        DataRecordMultiField dr_countries = new DataRecordMultiField("countries", "id", "id", "geo_block_types_countries", "GEO_BLOCK_TYPE_ID", "COUNTRY_ID", false, "ltr", 60, "tags");
        dr_countries.Initialize("Rule type", "adm_table_header_nbg", "FormInput", "COUNTRY_NAME", false);
        dr_countries.SetExtraWhere("GROUP_ID is null");
        dr_countries.SetOrderCollectionBy("newid()");
        theRecord.AddRecord(dr_countries);

        bool bVisible = PageUtils.IsTvinciUser();
        DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
        dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", false);
        dr_groups.SetValue(LoginManager.GetLoginGroupID().ToString());
        theRecord.AddRecord(dr_groups);

        DataRecordShortIntField dr_groups1 = new DataRecordShortIntField(false, 9, 9);
        dr_groups1.Initialize("Group", "adm_table_header_nbg", "FormInput", "GEO_RULE_TYPE", false);
        dr_groups1.SetValue("2");
        theRecord.AddRecord(dr_groups1);

        string sTable = theRecord.GetTableHTML("adm_finance_rules_countries_new.aspx?submited=1");

        return sTable;
    }
}
