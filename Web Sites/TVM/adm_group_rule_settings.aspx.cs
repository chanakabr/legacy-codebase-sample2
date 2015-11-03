using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using KLogMonitor;
using TVinciShared;

public partial class adm_group_rule_settings : System.Web.UI.Page
{
    private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
    protected string m_sMenu;
    protected string m_sSubMenu;
    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
        {
            Response.Redirect("login.html");
        }
        else if (LoginManager.IsPagePermitted("adm_my_group.aspx") == false)
        {
            LoginManager.LogoutFromSite("login.html");
        }

        if (AMS.Web.RemoteScripting.InvokeMethod(this))
        {
            return;
        }

        Int32 nMenuID = 0;

        if (!IsPostBack)
        {
            m_sMenu = TVinciShared.Menu.GetMainMenu(2, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 2, true);
            if (Request.QueryString["submited"] != null && Request.QueryString["submited"].ToString() == "1")
            {
                DBManipulator.DoTheWork();
                Int32 nGroupID = LoginManager.GetLoginGroupID();
            }
        }
    }

    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
    }

    //protected string GetWhereAmIStr()
    //{
    //    string sGroupName = LoginManager.GetLoginGroupName();
    //    Int32 nGroupID = LoginManager.GetLoginGroupID();
    //    if (Session["parent_group_id"] != null && Session["parent_group_id"].ToString() != "" && Session["parent_group_id"].ToString() != "0")
    //        nGroupID = int.Parse(Session["parent_group_id"].ToString());

    //    string sRet = "";
    //    bool bFirst = true;
    //    Int32 nLast = 0;
    //    nLast = int.Parse(PageUtils.GetTableSingleVal("groups", "parent_group_id", LoginManager.GetLoginGroupID()).ToString());
    //    while (nGroupID != nLast)
    //    {
    //        Int32 nParentID = int.Parse(PageUtils.GetTableSingleVal("groups", "PARENT_GROUP_ID", nGroupID).ToString());
    //        string sHeader = PageUtils.GetTableSingleVal("groups", "group_name", nGroupID).ToString();
    //        if (bFirst == false)
    //            sRet = "<span style=\"cursor:pointer;\" onclick=\"document.location.href='adm_groups.aspx?parent_group_id=" + nParentID.ToString() + "';\">" + sHeader + " </span><span class=\"arrow\">&raquo; </span>" + sRet;
    //        else
    //            sRet = sHeader;
    //        bFirst = false;
    //        nGroupID = nParentID;
    //    }
    //    sRet = "Groups: " + sRet;
    //    return sRet;
    //}

    public void GetHeader()
    {
        Response.Write(PageUtils.GetPreHeader() + ": Group Rules Settings");
    }

    protected void GetSubMenu()
    {
        Response.Write(m_sSubMenu);
    }

    public string GetPageContent(string sOrderBy, string sPageNum)
    {
        string parentalRulesQuery = "select name as txt, id as id from parental_rules where status=1 and group_id= " + LoginManager.GetLoginGroupID().ToString();
        string purchaseSettingsQuery = "SELECT 0 as id, 'Block' as txt UNION ALL SELECT 1 as id, 'Ask' as txt UNION ALL SELECT 2 as id, 'Allow' as txt";

        if (Session["error_msg"] != null && Session["error_msg"].ToString() != string.Empty)
        {
            Session["error_msg"] = string.Empty;

            return Session["last_page_html"].ToString();
        }

        // Check if it's new recored or not
        object groupId = LoginManager.GetLoginGroupID();

        int tableID = GetGroupRuleID(ODBCWrapper.Utils.GetIntSafeVal(groupId));

        object t = null;

        string backPage = "adm_group_rule_settings.aspx?search_save=1";
        DBRecordWebEditor theRecord;
        if (tableID > 0) // a new record
        {
            t = tableID;
        }

        theRecord = new DBRecordWebEditor("group_rule_settings", "adm_table_pager", backPage, "", "ID", t, backPage, "");

        DataRecordDropDownField dr_default_parental_rule_movies = new DataRecordDropDownField("parental_rules", "NAME", "id", "", null, 60, true);
        dr_default_parental_rule_movies.SetSelectsQuery(parentalRulesQuery);
        dr_default_parental_rule_movies.Initialize("Force default parental rules (movies)", "adm_table_header_nbg", "FormInput", "DEFAULT_MOVIES_PARENTAL_RULE", false);
        theRecord.AddRecord(dr_default_parental_rule_movies);

        DataRecordDropDownField dr_default_parental_rule_tv_series = new DataRecordDropDownField("parental_rules", "NAME", "id", "", null, 60, true);
        dr_default_parental_rule_tv_series.SetSelectsQuery(parentalRulesQuery);
        dr_default_parental_rule_tv_series.Initialize("Force default parental rules (TV series)", "adm_table_header_nbg", "FormInput", "DEFAULT_TV_SERIES_PARENTAL_RULE", false);
        theRecord.AddRecord(dr_default_parental_rule_tv_series);

        DataRecordShortTextField dr_default_pin = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_default_pin.Initialize("Default parental rule PIN", "adm_table_header_nbg", "FormInput", "DEFAULT_PARENTAL_PIN", true);
        theRecord.AddRecord(dr_default_pin);

        DataRecordDropDownField dr_default_purchase_settings = new DataRecordDropDownField("parental_rules", "NAME", "id", "", null, 60, false);
        dr_default_purchase_settings.SetSelectsQuery(purchaseSettingsQuery);
        dr_default_purchase_settings.Initialize("Default purchase rules", "adm_table_header_nbg", "FormInput", "DEFAULT_PURCHASE_SETTINGS", true);
        theRecord.AddRecord(dr_default_purchase_settings);

        DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
        dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", false);
        dr_groups.SetValue(LoginManager.GetLoginGroupID().ToString());
        theRecord.AddRecord(dr_groups);

        string sTable = theRecord.GetTableHTML("adm_group_rule_settings.aspx?submited=1");

        return sTable;
    }

    private int GetGroupRuleID(int groupID)
    {
        int groupRuleId = 0;
        try
        {
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select ID from group_rule_settings where status=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", groupID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    groupRuleId = ODBCWrapper.Utils.GetIntSafeVal(selectQuery.Table("query").DefaultView[0].Row, "ID");
                }
            }
            selectQuery.Finish();
            selectQuery = null;
        }
        catch (Exception ex)
        {
            log.Error(string.Empty, ex);
            groupRuleId = 0;
        }
        return groupRuleId;
    }
}