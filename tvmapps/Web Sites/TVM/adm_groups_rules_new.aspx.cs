using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TVinciShared;

public partial class adm_groups_rules_new : System.Web.UI.Page
{
    protected string m_sMenu;
    protected string m_sSubMenu;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        if (LoginManager.IsPagePermitted("adm_groups_rules.aspx") == false)
            LoginManager.LogoutFromSite("login.html");
        if (LoginManager.IsActionPermittedOnPage("adm_groups_rules.aspx", LoginManager.PAGE_PERMISION_TYPE.EDIT) == false)
            LoginManager.LogoutFromSite("login.html");

        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        if (!IsPostBack)
        {
            if (Request.QueryString["submited"] != null && Request.QueryString["submited"].ToString().Trim() == "1")
            {

                DBManipulator.DoTheWork();
                return;
            }
            Int32 nMenuID = 0;

            m_sMenu = TVinciShared.Menu.GetMainMenu(7, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, true);
            
            if (Request.QueryString["rule_id"] != null &&
                Request.QueryString["rule_id"].ToString() != "")
            {
                Session["rule_id"] = int.Parse(Request.QueryString["rule_id"].ToString());
            }
            else
                Session["rule_id"] = 0;
        }
    }

    public void GetHeader()
    {
        Response.Write(PageUtils.GetPreHeader() + ": Group rule values");
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
        object t = null; ;
        if (Session["rule_id"] != null && Session["rule_id"].ToString() != "" && int.Parse(Session["rule_id"].ToString()) != 0)
            t = Session["rule_id"];
        string sBack = "adm_groups_rules.aspx?search_save=1";
        DBRecordWebEditor theRecord = new DBRecordWebEditor("groups_rules", "adm_table_pager", sBack, "", "ID", t, sBack, "");

        DataRecordShortTextField dr_name = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_name.Initialize("Name", "adm_table_header_nbg", "FormInput", "NAME", true);
        theRecord.AddRecord(dr_name);

        DataRecordDropDownField dr_tag_type = new DataRecordDropDownField("media_tags_types", "NAME", "id", "", null, 60, true);
        string sQuery = "select name as txt,id as id from media_tags_types where status=1 and group_id= " + LoginManager.GetLoginGroupID().ToString();
        dr_tag_type.SetSelectsQuery(sQuery);
        dr_tag_type.Initialize("Tag Type", "adm_table_header_nbg", "FormInput", "tag_type_id", false);
        //dr_tag_type.SetDefaultVal(sDefWP);
        theRecord.AddRecord(dr_tag_type);

        DataRecordDropDownField dr_group_rule_type = new DataRecordDropDownField("lu_group_rule_types", "NAME", "id", "", null, 60, false);
        string sGroupRulesTypesQuery = "select description as txt,id as id from lu_group_rule_types";
        dr_group_rule_type.SetSelectsQuery(sGroupRulesTypesQuery);
        dr_group_rule_type.Initialize("Group Rule Type", "adm_table_header_nbg", "FormInput", "group_rule_type_id", true);
        theRecord.AddRecord(dr_group_rule_type);


        DataRecordShortTextField dr_ddk = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_ddk.Initialize("Dynamic Data Key", "adm_table_header_nbg", "FormInput", "dynamic_data_key", true);
        theRecord.AddRecord(dr_ddk);

        DataRecordShortTextField dr_defaultCode = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_defaultCode.Initialize("Default Code", "adm_table_header_nbg", "FormInput", "default_val", true);
        theRecord.AddRecord(dr_defaultCode);

        DataRecordBoolField dr_defaultEnabled = new DataRecordBoolField(false);
        dr_defaultEnabled.Initialize("Default Enabled", "adm_table_header_nbg", "FormInput", "default_enabled", false);
        theRecord.AddRecord(dr_defaultEnabled);

        DataRecordBoolField dr_block_anonymous = new DataRecordBoolField(true);
        dr_block_anonymous.Initialize("Block Anonymous Access", "adm_table_header_nbg", "FormInput", "block_anonymous", false);
        theRecord.AddRecord(dr_block_anonymous);

        DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
        dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", false);
        dr_groups.SetValue(LoginManager.GetLoginGroupID().ToString());
        theRecord.AddRecord(dr_groups);

        string sTable = theRecord.GetTableHTML("adm_groups_rules_new.aspx?submited=1");

        return sTable;
    }
    
}