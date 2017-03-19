using apiWS;
using GroupsCacheManager;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TVinciShared;

public partial class adm_asset_life_cycle_rules_new : System.Web.UI.Page
{
    private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
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
                FriendlyAssetLifeCycleRule rule = null;
                if (!GetFriendlyAssetLifeCycleRule(ref rule) ||  !InsertOrUpdateFriendlyAssetLifeCycleRule(rule))
                {
                    log.ErrorFormat("Failed GetFriendlyAssetLifeCycleRule or InsertOrUpdateFriendlyAssetLifeCycleRule, rule_id: {0}, name: {1}", rule.Id, rule.Name);
                    HttpContext.Current.Session["error_msg"] = "incorrect values while updating / failed inserting new rule";
                }

                return;
            }

            m_sMenu = TVinciShared.Menu.GetMainMenu(14, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, true);
            int ruleId = 0;
            if (Request.QueryString["rule_id"] != null && !string.IsNullOrEmpty(Request.QueryString["rule_id"].ToString()) && int.TryParse(Request.QueryString["rule_id"].ToString(), out ruleId) && ruleId > 0)
            {
                Session["rule_id"] = ruleId;                
            }
            else
            {
                Session["rule_id"] = 0;
            }
        }

    }

    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
    }

    public void GetHeader()
    {
        string sRet = PageUtils.GetPreHeader() + ": Asset Scheduling Rules";
        if (Session["rule_id"] != null && Session["rule_id"].ToString() != "" && Session["rule_id"].ToString() != "0")
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
        FriendlyAssetLifeCycleRule friendlyAssetLifeCycleRule = null;
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
            if (!string.IsNullOrEmpty(sWSURL) && !string.IsNullOrEmpty(sWSUserName) && !string.IsNullOrEmpty(sWSPass))
            {
                apiWS.API client = new apiWS.API();
                client.Url = sWSURL;
                FriendlyAssetLifeCycleRuleResponse res = client.GetFriendlyAssetLifeCycleRule(sWSUserName, sWSPass, ruleId);
                if (res != null && res.Status != null && res.Status.Code == 0 && res.Rule != null)
                {
                    friendlyAssetLifeCycleRule = res.Rule;
                }
                else 
                {
                    Session["error_msg"] = "Failed to get asset life cycle rule";
                    return Session["last_page_html"].ToString();
                }
            }
        }

        string sBack = "adm_asset_life_cycle_rules.aspx?search_save=1";
        DBRecordWebEditor theRecord = new DBRecordWebEditor("asset_life_cycle_rules", "adm_table_pager", sBack, "", "ID", ruleId, sBack, "");
        theRecord.SetConnectionKey("MAIN_CONNECTION_STRING");

        DataRecordShortTextField dr_name = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_name.Initialize("Name", "adm_table_header_nbg", "FormInput", "", true);
        dr_name.setFiledName("Name");
        if (friendlyAssetLifeCycleRule != null)
        {
            dr_name.SetValue(friendlyAssetLifeCycleRule.Name);
        }
        theRecord.AddRecord(dr_name);

        DataRecordShortTextField dr_description = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_description.Initialize("Description", "adm_table_header_nbg", "FormInput", "", true);
        dr_description.setFiledName("Description");
        if (friendlyAssetLifeCycleRule != null)
        {
            dr_description.SetValue(friendlyAssetLifeCycleRule.Description);
        }
        theRecord.AddRecord(dr_description);

        DataRecordDropDownField dr_FilterTagType = new DataRecordDropDownField("", "name", "id", "", null, 60, false);
        string sQuery = "select distinct name as txt, id as id from Tvinci.dbo.media_tags_types where status=1 and group_id in (" + string.Join(",", subGroups) + ")";
        dr_FilterTagType.SetSelectsQuery(sQuery);        
        dr_FilterTagType.Initialize("Transition Filter Tag Type", "adm_table_header_nbg", "FormInput", "", true);
        dr_FilterTagType.setFiledName("FilterTagTypeId");
        if (friendlyAssetLifeCycleRule != null)
        {
            dr_FilterTagType.SetValue(friendlyAssetLifeCycleRule.FilterTagType.key);
        }
        theRecord.AddRecord(dr_FilterTagType);

        DataRecordRadioField dr_filterTagOperand = new DataRecordRadioField("", "NAME", "id", "", null);
        dr_filterTagOperand.SetSelectsDT(GetFilterTagOperandDT());        
        dr_filterTagOperand.Initialize("Filter Tag Operand", "adm_table_header_nbg", "FormInput", "", true);
        dr_filterTagOperand.SetDefault(0);
        dr_filterTagOperand.setFiledName("FilterTagOperand");
        if (friendlyAssetLifeCycleRule != null)
        {
            dr_filterTagOperand.SetValue(((int)friendlyAssetLifeCycleRule.FilterTagOperand).ToString());
        }
        theRecord.AddRecord(dr_filterTagOperand);

        DataRecordShortTextField dr_filterTagValues = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_filterTagValues.Initialize("Filter Tag Values", "adm_table_header_nbg", "FormInput", "", true);
        dr_filterTagValues.setFiledName("FilterTagValues");
        if (friendlyAssetLifeCycleRule != null)
        {
            dr_filterTagValues.SetValue(string.Join(";",friendlyAssetLifeCycleRule.FilterTagValues));
        }
        theRecord.AddRecord(dr_filterTagValues);

        DataRecordDropDownField dr_transitionIntervalUnits = new DataRecordDropDownField("lu_alcr_transition_interval_units", "name", "id", "", "", 60, false);
        dr_transitionIntervalUnits.Initialize("Transition Interval Unit", "adm_table_header_nbg", "FormInput", "", true);
        dr_transitionIntervalUnits.SetDefault(1);
        dr_transitionIntervalUnits.setFiledName("TransitionIntervalUnitsId");
        if (friendlyAssetLifeCycleRule != null)
        {
            dr_transitionIntervalUnits.SetValue(friendlyAssetLifeCycleRule.TransitionIntervalUnits.ToString());
        }
        theRecord.AddRecord(dr_transitionIntervalUnits);

        DataRecordDropDownField dr_metaDateName = new DataRecordDropDownField("", "name", "id", "", null, 60, true);
        sQuery = "select distinct name as txt, id as id from Tvinci.dbo.groups_date_metas where status=1 and group_id in (" + string.Join(",", subGroups) + ")";
        dr_metaDateName.SetSelectsQuery(sQuery);
        dr_metaDateName.Initialize("Meta Date Name", "adm_table_header_nbg", "FormInput", "", true);
        dr_metaDateName.SetNoSelectStr("Start Date");
        dr_metaDateName.setFiledName("MetaDateNameId");        
        if (friendlyAssetLifeCycleRule != null)
        {
            if (friendlyAssetLifeCycleRule.MetaDateName.ToLower() == "start_date")
            {
                dr_metaDateName.SetValue("0");
            }
            else
            {
                object metaDateNameId = ODBCWrapper.Utils.GetTableSingleVal("groups_date_metas", "id", "name", "=", friendlyAssetLifeCycleRule.MetaDateName, 0, "MAIN_CONNECTION_STRING");
                if (metaDateNameId != null && metaDateNameId != DBNull.Value && !string.IsNullOrEmpty(metaDateNameId.ToString()))
                {
                    dr_metaDateName.SetValue(metaDateNameId.ToString());
                }
            }
        }
        theRecord.AddRecord(dr_metaDateName);

        DataRecordShortIntField dr_metaDateValue = new DataRecordShortIntField(true, 9, 9, 0);
        dr_metaDateValue.Initialize("Meta Date Value", "adm_table_header_nbg", "FormInput", "", true);
        dr_metaDateValue.SetDefault(1);
        dr_metaDateValue.setFiledName("MetaDateValue");
        if (friendlyAssetLifeCycleRule != null)
        {
            dr_metaDateValue.SetValue(friendlyAssetLifeCycleRule.MetaDateValue.ToString());
        }
        theRecord.AddRecord(dr_metaDateValue);

        DataRecordShortTextField dr_transitionTagToAdd = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_transitionTagToAdd.Initialize("Add Tag Values", "adm_table_header_nbg", "FormInput", "", false);
        dr_transitionTagToAdd.setFiledName("TagNamesToAdd");
        if (friendlyAssetLifeCycleRule != null)
        {
            dr_transitionTagToAdd.SetValue(string.Join(";", friendlyAssetLifeCycleRule.TagNamesToAdd));
        }
        theRecord.AddRecord(dr_transitionTagToAdd);

        DataRecordShortTextField dr_transitionTagToRemove = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_transitionTagToRemove.Initialize("Remove Tag Values", "adm_table_header_nbg", "FormInput", "", false);
        dr_transitionTagToRemove.setFiledName("TagNamesToRemove");
        if (friendlyAssetLifeCycleRule != null)
        {
            dr_transitionTagToRemove.SetValue(string.Join(";", friendlyAssetLifeCycleRule.TagNamesToRemove));
        }
        theRecord.AddRecord(dr_transitionTagToRemove);

        string sTable = theRecord.GetTableHTML("adm_asset_life_cycle_rules_new.aspx?submited=1");

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

    private bool GetFriendlyAssetLifeCycleRule(ref apiWS.FriendlyAssetLifeCycleRule rule)
    {
        bool result = false;
        System.Collections.Specialized.NameValueCollection coll = HttpContext.Current.Request.Form;       
        int nCount = coll.Count;
        int nCounter = 0;
        string[] values = null;
        AssetLifeCycleRuleTransitionIntervalUnits transitionIntervalUnits = AssetLifeCycleRuleTransitionIntervalUnits.Unknown;
        eCutType operand = eCutType.And;
        try
        {
            rule = new FriendlyAssetLifeCycleRule();
            int ruleId = 0;
            if (!string.IsNullOrEmpty(Session["rule_id"].ToString()) && int.TryParse(Session["rule_id"].ToString(), out ruleId) && ruleId > 0)
            {
                rule.Id = ruleId;
            }

            result = true;
            while (nCounter < nCount && result)
            {
                try
                {
                    if (coll[nCounter.ToString() + "_fieldName"] != null)
                    {
                        string sFieldName = coll[nCounter.ToString() + "_fieldName"].ToString();
                        string sVal = "";
                        if (coll[nCounter.ToString() + "_val"] != null)
                        {
                            sVal = coll[nCounter.ToString() + "_val"].ToString();
                        }

                        if (!string.IsNullOrEmpty(sVal))
                        {
                            #region case
                            switch (sFieldName)
                            {
                                case "Name":
                                    rule.Name = sVal;
                                    break;
                                case "Description":
                                    rule.Description = sVal;
                                    break;
                                case "FilterTagTypeId":
                                    rule.FilterTagType = new KeyValuePair() { key = sVal, value = string.Empty };
                                    break;
                                case "FilterTagOperand":
                                    if (Enum.TryParse<eCutType>(sVal, out operand))
                                    {
                                        rule.FilterTagOperand = operand;
                                    }
                                    else
                                    {
                                        result = false;
                                    }                                    
                                    break;
                                case "FilterTagValues":
                                    values = sVal.Split(new char[1] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                                    if (values != null && values.Length > 0)
                                    {
                                        rule.FilterTagValues = values;
                                    }
                                    break;
                                case "TransitionIntervalUnitsId":
                                    if (Enum.TryParse<AssetLifeCycleRuleTransitionIntervalUnits>(sVal, out transitionIntervalUnits))
                                    {
                                        rule.TransitionIntervalUnits = transitionIntervalUnits;
                                    }
                                    else
                                    {
                                        result = false;
                                    }
                                    break;
                                case "MetaDateNameId":
                                    int metaDateNameId = 0;
                                    if (sVal == "0")
                                    {
                                        rule.MetaDateName = "start_date";
                                    }
                                    else if (int.TryParse(sVal, out metaDateNameId) && metaDateNameId > 0)
                                    {
                                        object obj = ODBCWrapper.Utils.GetTableSingleVal("groups_date_metas", "name", metaDateNameId);
                                        if (obj != null && obj != DBNull.Value && !string.IsNullOrEmpty(obj.ToString()))
                                        {
                                            rule.MetaDateName = obj.ToString();
                                        }
                                        else
                                        {
                                            result = false;
                                        }
                                    }
                                    else
                                    {
                                        result = false;
                                    }
                                    break;
                                case "MetaDateValue":
                                    long metaDate = 0;
                                    if (long.TryParse(sVal, out metaDate))
                                    {
                                        rule.MetaDateValue = metaDate;
                                    }
                                    else
                                    {
                                        result = false;
                                    }
                                    break;
                                case "TagNamesToAdd":
                                    values = sVal.Split(new char[1] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                                    if (values != null && values.Length > 0)
                                    {
                                        rule.TagNamesToAdd = values;
                                    }
                                    break;
                                case "TagNamesToRemove":
                                    values = sVal.Split(new char[1] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                                    if (values != null && values.Length > 0)
                                    {
                                        rule.TagNamesToRemove = values;
                                    }
                                    break;
                                default:
                                    break;

                            }
                            #endregion
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.Error("Failed in switch GetFriendlyAssetLifeCycleRule", ex);
                    result = false;
                    break;
                }

                nCounter++;
            }

        }
        catch (Exception ex)
        {
            log.Error("Failed GetFriendlyAssetLifeCycleRule", ex);
        }
        
        return result;
    }            

    private bool InsertOrUpdateFriendlyAssetLifeCycleRule(FriendlyAssetLifeCycleRule rule)
    {
        bool result = false;
        if (rule != null)
        {
            string sIP = "1.1.1.1";
            string sWSUserName = "";
            string sWSPass = "";

            TVinciShared.WS_Utils.GetWSUNPass(LoginManager.GetLoginGroupID(), "Asset", "api", sIP, ref sWSUserName, ref sWSPass);
            string sWSURL = TVinciShared.WS_Utils.GetTcmConfigValue("api_ws");
            if (!string.IsNullOrEmpty(sWSURL) && !string.IsNullOrEmpty(sWSUserName) && !string.IsNullOrEmpty(sWSPass))
            {
                apiWS.API client = new apiWS.API();
                client.Url = sWSURL;
                FriendlyAssetLifeCycleRuleResponse res = client.InsertOrUpdateFriendlyAssetLifeCycleRule(sWSUserName, sWSPass, rule);
                if (res != null && res.Status != null && res.Status.Code == 0 && res.Rule != null)
                {
                    result = true;
                }
                else
                {
                    Session["error_msg"] = "Failed to insert or update asset life cycle rule";
                }
            }
        }

        return result;
    }

}