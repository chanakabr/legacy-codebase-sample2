using DAL;
using System;
using System.Collections.Generic;
using System.Data;
using TVinciShared;

public partial class adm_device_rules_new : System.Web.UI.Page
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
                int ruleId = DBManipulator.DoTheWork();
                UpdateDeviceRulesBrands(ruleId, LoginManager.GetLoginGroupID());

                return;
            }
            m_sMenu = TVinciShared.Menu.GetMainMenu(5, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, true);
            if (Request.QueryString["device_rule_id"] != null &&
                Request.QueryString["device_rule_id"].ToString() != "")
            {
                Session["device_rule_id"] = int.Parse(Request.QueryString["device_rule_id"].ToString());
                Int32 nOwnerGroupID = int.Parse(PageUtils.GetTableSingleVal("device_rules", "group_id", int.Parse(Session["device_rule_id"].ToString())).ToString());
                Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();
                if (nLogedInGroupID != nOwnerGroupID && PageUtils.IsTvinciUser() == false)
                {
                    LoginManager.LogoutFromSite("login.html");
                    return;
                }
            }
            else
                Session["device_rule_id"] = 0;
        }
    }

    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
    }

    public void GetHeader()
    {
        string sRet = PageUtils.GetPreHeader() + ": Block media by device rules";
        if (Session["device_rule_id"] != null && Session["device_rule_id"].ToString() != "" && Session["device_rule_id"].ToString() != "0")
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
        if (Session["device_rule_id"] != null && Session["device_rule_id"].ToString() != "" && int.Parse(Session["device_rule_id"].ToString()) != 0)
            t = Session["device_rule_id"];
        string sBack = "adm_device_rules.aspx?search_save=1";
        DBRecordWebEditor theRecord = new DBRecordWebEditor("device_rules", "adm_table_pager", sBack, "", "ID", t, sBack, "");

        DataRecordShortTextField dr_domain = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_domain.Initialize("Rule name", "adm_table_header_nbg", "FormInput", "NAME", true);
        theRecord.AddRecord(dr_domain);

        DataRecordRadioField dr_but_or_only = new DataRecordRadioField("lu_only_or_but", "description", "id", "", null);
        dr_but_or_only.Initialize("Rule type", "adm_table_header_nbg", "FormInput", "ONLY_OR_BUT", true);
        dr_but_or_only.SetDefault(0);
        theRecord.AddRecord(dr_but_or_only);

        bool bVisible = PageUtils.IsTvinciUser();
        if (bVisible == true)
        {
            DataRecordDropDownField dr_groups = new DataRecordDropDownField("groups", "GROUP_NAME", "id", "", null, 60, false);
            dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", true);

            dr_groups.SetWhereString("status<>2 and id " + PageUtils.GetAllChildGroupsStr());
            theRecord.AddRecord(dr_groups);
        }
        else
        {
            DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
            dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", false);
            dr_groups.SetValue(LoginManager.GetLoginGroupID().ToString());
            theRecord.AddRecord(dr_groups);
        }

        string sTable = theRecord.GetTableHTML("adm_device_rules_new.aspx?submited=1");

        return sTable;
    }

    //public string changeItemStatus(string sID, string dualListName)
    //{
    //    // userType dualList cChanged then call changeItemStatusUserTypes
    //    if (dualListName.ToUpper() == "DualListUserTypes".ToUpper())
    //    {
    //        changeItemStatusUserTypes(sID);
    //    }
    //    return "";
    //}
    public string initDualObj()
    {
        Dictionary<string, object> dualList = new Dictionary<string, object>();
        dualList.Add("FirstListTitle", "Device types");
        dualList.Add("SecondListTitle", "Available Device types");

        List<object> deviceBrandsList = new List<object>();
        DataTable devicetypesInRule = new DataTable();

        if (Session["device_rule_id"] != null && !string.IsNullOrEmpty(Session["device_rule_id"].ToString()) && int.TryParse(Session["device_rule_id"].ToString(), out int ruleId) && ruleId > 0)
        {
            devicetypesInRule = TvmDAL.GetDeviceRuleBrandsById(ruleId);
        }

        DataTable deviceBrandsFamilies = TvmDAL.GetDeviceBrandsFamilies();
        List<string> devicetypesInRuleHashSet = new List<string>();
        List<string> deviceBrandsFamiliesHashSet = new List<string>();

        if (devicetypesInRule != null && devicetypesInRule.Rows != null)
        {
            foreach (DataRow dr in devicetypesInRule.Rows)
            {
                devicetypesInRuleHashSet.Add(dr["DEVICE_BRANDS_ID"].ToString());
                var data = new
                {
                    ID = dr["DEVICE_BRANDS_ID"],
                    Title = $"{dr["DISPLAY_NAME"]}",
                    Description = $"{dr["DISPLAY_NAME"]}",
                    InList = true
                };
                deviceBrandsList.Add(data);
            }
        }

        if (deviceBrandsFamilies.Rows != null)
        {
            foreach (DataRow dr in deviceBrandsFamilies.Rows)
            {
                if (devicetypesInRuleHashSet.Count == 0 || !devicetypesInRuleHashSet.Contains(dr["id"].ToString()))
                {
                    deviceBrandsFamiliesHashSet.Add(dr["id"].ToString());

                    var data = new
                    {
                        ID = dr["Id"],
                        Title = $"{dr["DISPLAY_NAME"]}",
                        Description = $"{dr["DISPLAY_NAME"]}",
                        InList = false
                    };
                    deviceBrandsList.Add(data);
                }
            }
        }

        Session["devicetypesInRule"] = devicetypesInRuleHashSet;
        Session["deviceBrandsFamilies"] = deviceBrandsFamiliesHashSet;

        object[] resultData = deviceBrandsList.ToArray();

        dualList.Add("Data", resultData);
        dualList.Add("pageName", "adm_device_rules_new.aspx");
        dualList.Add("withCalendar", false);

        return dualList.ToJSON();
    }

    public string changeItemStatus(string id, string sAction)
    {

        List<string> devicetypesInRuleHashSet = new List<string>();
        List<string> deviceBrandsFamiliesHashSet = new List<string>();
        if (Session["devicetypesInRule"] != null && Session["deviceBrandsFamilies"] != null)
        {
            devicetypesInRuleHashSet = (List<string>)Session["devicetypesInRule"];
            deviceBrandsFamiliesHashSet = (List<string>)Session["deviceBrandsFamilies"];
        }
        if (int.TryParse(id, out int deviceBrandsId))
        {
            if (devicetypesInRuleHashSet.Contains(id))
            {
                devicetypesInRuleHashSet.Remove(id);
                deviceBrandsFamiliesHashSet.Add(id);
            }

            else if (deviceBrandsFamiliesHashSet.Contains(id))
            {
                deviceBrandsFamiliesHashSet.Remove(id);
                devicetypesInRuleHashSet.Add(id);
            }
        }

        Session["devicetypesInRule"] = devicetypesInRuleHashSet;
        Session["deviceBrandsFamilies"] = deviceBrandsFamiliesHashSet;

        return "";
    }

    private void UpdateDeviceRulesBrands(int ruleId, int groupId)
    {
        if (Session["devicetypesInRule"] != null && Session["devicetypesInRule"] is List<string>)
        {
            List<string> ids = Session["devicetypesInRule"] as List<string>;

            TvmDAL.UpsertDeviceRulesBrands(ids, ruleId, groupId, LoginManager.GetLoginID());
        }
    }
}