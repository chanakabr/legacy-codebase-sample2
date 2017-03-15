using GroupsCacheManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TVinciShared;

public partial class adm_asset_life_cycle_rule_new : System.Web.UI.Page
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
                DBManipulator.DoTheWork("MAIN_CONNECTION_STRING");
                return;
            }

            m_sMenu = TVinciShared.Menu.GetMainMenu(14, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, true);
            if (Request.QueryString["rule_id"] != null && !string.IsNullOrEmpty(Request.QueryString["rule_id"].ToString()))
            {
                Session["rule_id"] = int.Parse(Request.QueryString["rule_id"].ToString());
            }
            else
                Session["rule_id"] = 0;
        }

    }

    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
    }

    public void GetHeader()
    {
        string sRet = PageUtils.GetPreHeader() + ": Discount Codes";
        if (Session["discount_code_id"] != null && Session["discount_code_id"].ToString() != "" && Session["discount_code_id"].ToString() != "0")
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
        int ruleId = 0;
        apiWS.FriendlyAssetLifeCycleRule friendlyAssetLifeCycleRule = null;
        int currentGroup = LoginManager.GetLoginGroupID();
        GroupManager groupManager = new GroupManager();
        List<int> subGroups = groupManager.GetSubGroup(currentGroup);
        if (Session["rule_id"] != null && !string.IsNullOrEmpty(Session["rule_id"].ToString()) && int.TryParse(Session["rule_id"].ToString(), out ruleId) && ruleId > 0)
        {            
            string sIP = "1.1.1.1";
            string sWSUserName = "";
            string sWSPass = "";
            
            TVinciShared.WS_Utils.GetWSUNPass(LoginManager.GetLoginGroupID(), "Asset", "api", sIP, ref sWSUserName, ref sWSPass);
            string sWSURL = TVinciShared.WS_Utils.GetTcmConfigValue("api_ws");

            //if (string.IsNullOrEmpty(sWSURL) || string.IsNullOrEmpty(sWSUserName) || string.IsNullOrEmpty(sWSPass))
            //{
            //    return string.Empty;
            //}

            apiWS.API client = new apiWS.API();
            client.Url = sWSURL;
            friendlyAssetLifeCycleRule = client.GetFriendlyAssetLifeCycleRule(sWSUserName, sWSPass, ruleId);
        }

        string sBack = "adm_asset_life_cycle_rules.aspx?search_save=1";
        DBRecordWebEditor theRecord = new DBRecordWebEditor("asset_life_cycle_rules", "adm_table_pager", sBack, "", "ID", ruleId, sBack, "");
        theRecord.SetConnectionKey("MAIN_CONNECTION_STRING");

        DataRecordShortTextField dr_name = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_name.Initialize("Name", "adm_table_header_nbg", "FormInput", "Name", true);
        theRecord.AddRecord(dr_name);

        DataRecordShortTextField dr_description = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_description.Initialize("Description", "adm_table_header_nbg", "FormInput", "Description", true);        
        theRecord.AddRecord(dr_description);

        DataRecordDropDownField dr_FilterTagType = new DataRecordDropDownField("", "name", "id", "", null, 60, false);        
        string sQuery = "select distinct name as txt, id as id from Tvinci.dbo.media_tags_types where status=1 and group_id in (" + string.Join(",", subGroups) + ")";
        dr_FilterTagType.SetSelectsQuery(sQuery);
        dr_FilterTagType.Initialize("Transition Filter Tag Type", "adm_table_header_nbg", "FormInput", "", false);        
        theRecord.AddRecord(dr_FilterTagType);

        DataRecordRadioField dr_filterTagOperand = new DataRecordRadioField("", "NAME", "id", "", null);
        dr_filterTagOperand.SetSelectsDT(GetFilterTagOperandDT());        
        dr_filterTagOperand.Initialize("Filter Tag Operand", "adm_table_header_nbg", "FormInput", "", true);
        theRecord.AddRecord(dr_filterTagOperand);

        DataRecordShortTextField dr_filterTagValues = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_filterTagValues.Initialize("Filter Tag Values", "adm_table_header_nbg", "FormInput", "Name", true);
        theRecord.AddRecord(dr_filterTagValues);

        DataRecordDropDownField dr_transitionIntervalUnits = new DataRecordDropDownField("lu_alcr_transition_interval_units", "name", "id", "", "", 60, false);
        dr_transitionIntervalUnits.Initialize("Transition Interval Unit", "adm_table_header_nbg", "FormInput", "TRANSITION_INTERVAL_UNITS_ID", true);
        dr_transitionIntervalUnits.SetDefault(1);
        theRecord.AddRecord(dr_transitionIntervalUnits);

        DataRecordDropDownField dr_metaDateName = new DataRecordDropDownField("", "name", "id", "", null, 60, false);
        sQuery = "select name as txt, id as id from Tvinci.dbo.groups_date_metas where status=1 and group_id in (" + string.Join(",", subGroups) + ")";
        dr_metaDateName.SetSelectsQuery(sQuery);
        dr_metaDateName.Initialize("Meta Date Name", "adm_table_header_nbg", "FormInput", "", false);
        theRecord.AddRecord(dr_metaDateName);

        DataRecordShortIntField dr_metaDateValue = new DataRecordShortIntField(false, 9, 9, 0);
        dr_metaDateValue.Initialize("Meta Date Value", "adm_table_header_nbg", "FormInput", "", false);
        dr_metaDateValue.SetValue(LoginManager.GetLoginGroupID().ToString());
        theRecord.AddRecord(dr_metaDateValue);

        DataRecordShortTextField dr_transitionTagToAdd = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_transitionTagToAdd.Initialize("Add Tag Values", "adm_table_header_nbg", "FormInput", "", true);
        theRecord.AddRecord(dr_transitionTagToAdd);

        DataRecordShortTextField dr_transitionTagToRemove = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_transitionTagToRemove.Initialize("Remove Tag Values", "adm_table_header_nbg", "FormInput", "", true);
        theRecord.AddRecord(dr_transitionTagToRemove);

        DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
        dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", false);
        dr_groups.SetValue(LoginManager.GetLoginGroupID().ToString());
        theRecord.AddRecord(dr_groups);

        string sTable = theRecord.GetTableHTML("adm_asset_life_cycle_rule_new.aspx?submited=1");

        return sTable;
    }

    private System.Data.DataTable GetFilterTagOperandDT()
    {
        System.Data.DataTable dt = new System.Data.DataTable();
        dt.Columns.Add("id", typeof(int));
        dt.Columns.Add("txt", typeof(string));
        foreach (ApiObjects.eCutType r in Enum.GetValues(typeof(ApiObjects.eCutType)))
        {
            dt.Rows.Add((int)r, r);
        }

        return dt;
    }

}
