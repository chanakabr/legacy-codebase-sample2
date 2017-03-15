using GroupsCacheManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TVinciShared;

public partial class adm_asset_life_cycle_rules_new : System.Web.UI.Page
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
                System.Collections.Specialized.NameValueCollection coll = HttpContext.Current.Request.Form;
                //DBManipulator.DoTheWork("MAIN_CONNECTION_STRING");
                return;
            }

            m_sMenu = TVinciShared.Menu.GetMainMenu(14, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, true);
            int ruleId = 0;
            if (Request.QueryString["rule_id"] != null && !string.IsNullOrEmpty(Request.QueryString["rule_id"].ToString()) && int.TryParse(Request.QueryString["rule_id"].ToString(), out ruleId) && ruleId > 0)
            {
                Session["rule_id"] = ruleId;
                System.Collections.Specialized.NameValueCollection coll = HttpContext.Current.Request.Form;
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
            if (!string.IsNullOrEmpty(sWSURL))
            {
                apiWS.API client = new apiWS.API();
                client.Url = sWSURL;
                friendlyAssetLifeCycleRule = client.GetFriendlyAssetLifeCycleRule(sWSUserName, sWSPass, ruleId);
            }            
        }

        string sBack = "adm_asset_life_cycle_rules.aspx?search_save=1";
        DBRecordWebEditor theRecord = new DBRecordWebEditor("asset_life_cycle_rules", "adm_table_pager", sBack, "", "ID", ruleId, sBack, "");
        theRecord.SetConnectionKey("MAIN_CONNECTION_STRING");

        DataRecordShortTextField dr_name = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_name.Initialize("Name", "adm_table_header_nbg", "FormInput", "Name", true);
        dr_name.setFiledName("Name");
        theRecord.AddRecord(dr_name);

        DataRecordShortTextField dr_description = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_description.Initialize("Description", "adm_table_header_nbg", "FormInput", "Description", true);
        dr_description.setFiledName("Description");
        theRecord.AddRecord(dr_description);

        DataRecordDropDownField dr_FilterTagType = new DataRecordDropDownField("", "name", "id", "", null, 60, false);        
        string sQuery = "select distinct name as txt, id as id from Tvinci.dbo.media_tags_types where status=1 and group_id in (" + string.Join(",", subGroups) + ")";
        dr_FilterTagType.SetSelectsQuery(sQuery);        
        dr_FilterTagType.Initialize("Transition Filter Tag Type", "adm_table_header_nbg", "FormInput", "", true);
        dr_FilterTagType.setFiledName("FilterTagTypeName");
        theRecord.AddRecord(dr_FilterTagType);

        DataRecordRadioField dr_filterTagOperand = new DataRecordRadioField("", "NAME", "id", "", null);
        dr_filterTagOperand.SetSelectsDT(GetFilterTagOperandDT());        
        dr_filterTagOperand.Initialize("Filter Tag Operand", "adm_table_header_nbg", "FormInput", "", true);
        dr_filterTagOperand.SetDefault(0);
        dr_filterTagOperand.setFiledName("FilterTagOperand");
        theRecord.AddRecord(dr_filterTagOperand);

        DataRecordShortTextField dr_filterTagValues = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_filterTagValues.Initialize("Filter Tag Values", "adm_table_header_nbg", "FormInput", "Name", true);
        dr_filterTagValues.setFiledName("FilterTagValues");
        theRecord.AddRecord(dr_filterTagValues);

        DataRecordDropDownField dr_transitionIntervalUnits = new DataRecordDropDownField("lu_alcr_transition_interval_units", "name", "id", "", "", 60, false);
        dr_transitionIntervalUnits.Initialize("Transition Interval Unit", "adm_table_header_nbg", "FormInput", "TRANSITION_INTERVAL_UNITS_ID", true);
        dr_transitionIntervalUnits.SetDefault(1);
        dr_description.setFiledName("TransitionIntervalUnits");
        theRecord.AddRecord(dr_transitionIntervalUnits);

        DataRecordDropDownField dr_metaDateName = new DataRecordDropDownField("", "name", "id", "", null, 60, false);
        sQuery = "select name as txt, id as id from Tvinci.dbo.groups_date_metas where status=1 and group_id in (" + string.Join(",", subGroups) + ")";
        dr_metaDateName.SetSelectsQuery(sQuery);
        dr_metaDateName.Initialize("Meta Date Name", "adm_table_header_nbg", "FormInput", "", true);
        dr_metaDateName.setFiledName("MetaDateName");
        theRecord.AddRecord(dr_metaDateName);

        DataRecordShortIntField dr_metaDateValue = new DataRecordShortIntField(false, 9, 9, 0);
        dr_metaDateValue.Initialize("Meta Date Value", "adm_table_header_nbg", "FormInput", "", false);
        dr_metaDateValue.SetValue(LoginManager.GetLoginGroupID().ToString());
        dr_metaDateValue.setFiledName("MetaDateValueInSeconds");
        theRecord.AddRecord(dr_metaDateValue);

        DataRecordShortTextField dr_transitionTagToAdd = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_transitionTagToAdd.Initialize("Add Tag Values", "adm_table_header_nbg", "FormInput", "", false);
        dr_transitionTagToAdd.setFiledName("TagNamesToAdd");
        theRecord.AddRecord(dr_transitionTagToAdd);

        DataRecordShortTextField dr_transitionTagToRemove = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_transitionTagToRemove.Initialize("Remove Tag Values", "adm_table_header_nbg", "FormInput", "", false);
        dr_transitionTagToRemove.setFiledName("TagNamesToRemove");
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

    private apiWS.FriendlyAssetLifeCycleRule GetFriendlyAssetLifeCycleRule()
    {
        apiWS.FriendlyAssetLifeCycleRule result = null;
        System.Collections.Specialized.NameValueCollection coll = HttpContext.Current.Request.Form;
        if (coll["table_name"] == null)
        {
            HttpContext.Current.Session["error_msg"] = "missing table name - cannot update";
        }
        else
        {
            int nCount = coll.Count;
            int nCounter = 0;
            bool bValid = true;
            try
            {
                while (nCounter < nCount)
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
                            //#region case
                            //switch (sFieldName)
                            //{
                            //    //case "FilterTagTypeName":
                            //    //    if (sVal == "1")
                            //    //    break;
                            //    //case "FilterTagValues":
                            //    //    if (sVal == "1")
                            //    //    break;
                            //    //case "FilterTagOperand":
                            //    //    if (sVal == "1")
                            //    //    break;
                            //    //case "MetaDateName":
                            //    //    if (sVal == "1")
                            //    //    break;
                            //    //case "MetaDateValueInSeconds":
                            //    //    if (sVal == "1")
                            //    //    break;
                            //    //case "TagNamesToAdd":
                            //    //    if (sVal == "1")
                            //    //    break;
                            //    //case "TagNamesToRemove":
                            //    //    if (sVal == "1")
                            //    //    break;
                            //    //case "recipients":
                            //    //    if (!string.IsNullOrEmpty(sVal))
                            //    //    {
                            //    //        recipients = int.Parse(sVal);
                            //    //    }
                            //    //    break;
                            //    //case "Name":
                            //    //    name = sVal.Replace("\r\n", "<br\\>");
                            //    //    break;
                            //    //case "Message":
                            //    //    message = sVal.Replace("\r\n", "&lt;br\\&gt;");
                            //    //    break;
                            //    //case "StartDateTime":
                            //    //    string sValMin = coll[nCounter.ToString() + "_valMin"].ToString();
                            //    //    string sValHour = coll[nCounter.ToString() + "_valHour"].ToString();
                            //    //    bValid = validateParam("int", sValHour, 0, 23);
                            //    //    if (bValid == true)
                            //    //        bValid = validateParam("int", sValMin, 0, 59);
                            //    //    if (bValid == true)
                            //    //        bValid = validateParam("date", sVal, 0, 59);
                            //    //    DateTime tTime = DateUtils.GetDateFromStr(sVal);
                            //    //    if (sValHour == "")
                            //    //        sValHour = "0";
                            //    //    if (sValMin == "")
                            //    //        sValMin = "0";
                            //    //    tTime = tTime.AddHours(int.Parse(sValHour.ToString()));
                            //    //    tTime = tTime.AddMinutes(int.Parse(sValMin.ToString()));
                            //    //    date = tTime;
                            //    //    //getDateTime(sVal, nCounter, ref coll, ref bValid);
                            //    //    break;
                            //    //case "TimeZone":
                            //    //    try
                            //    //    {
                            //    //        timezone = sVal;
                            //    //    }
                            //    //    catch (Exception)
                            //    //    {

                            //    //    }
                            //    //    break;
                            //    default:
                            //        break;

                            //}
                            //#endregion
                        }

                    }
                    catch (Exception ex)
                    {
                        break;
                    }

                    nCounter++;
                }
                //convert datetime to UTC
                ////date = ODBCWrapper.Utils.ConvertToUtc(date, timezone);

            }
            catch (Exception)
            {
            }
        }

        return result;
    }

}
