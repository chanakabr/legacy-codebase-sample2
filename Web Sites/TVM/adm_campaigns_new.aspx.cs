using ConfigurationManager;
using System;
using System.Data;
using TVinciShared;

public partial class adm_campaigns_new : System.Web.UI.Page
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
            m_sMenu = TVinciShared.Menu.GetMainMenu(13, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, true);
            if (Request.QueryString["campaign_id"] != null &&
                Request.QueryString["campaign_id"].ToString() != "")
            {
                Session["campaign_id"] = int.Parse(Request.QueryString["campaign_id"].ToString());
                Int32 nOwnerGroupID = int.Parse(PageUtils.GetTableSingleVal("campaigns", "group_id", int.Parse(Session["campaign_id"].ToString())).ToString());
                Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();
                if (nLogedInGroupID != nOwnerGroupID && PageUtils.IsTvinciUser() == false)
                {
                    LoginManager.LogoutFromSite("login.html");
                    return;
                }
            }
            else
                Session["campaign_id"] = 0;

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
        string sRet = PageUtils.GetPreHeader() + ": Campaigns";
        if (Session["campaign_id"] != null && Session["campaign_id"].ToString() != "" && Session["campaign_id"].ToString() != "0")
            sRet += " - Edit";
        else
            sRet += " - New";

        Response.Write(sRet);
    }

    protected void GetSubMenu()
    {
        Response.Write(m_sSubMenu);
    }

    protected System.Data.DataTable GetBaseDT()
    {
        System.Data.DataTable dT = new System.Data.DataTable();
        Int32 n = 0;
        string s = "";
        dT.Columns.Add(PageUtils.GetColumn("ID", n));
        dT.Columns.Add(PageUtils.GetColumn("txt", s));
        return dT.Copy();
    }

    protected DataTable GetUsageModulesDT(TvinciPricing.mdoule m, string sWSUserName, string sWSPass)
    {
        System.Data.DataTable usageModuleCodesDT = GetBaseDT();

        TvinciPricing.UsageModule[] oUMCodes = m.GetUsageModuleList(sWSUserName, sWSPass, string.Empty, string.Empty, string.Empty);
        if (oUMCodes != null)
        {
            for (int i = 0; i < oUMCodes.Length; i++)
            {
                System.Data.DataRow tmpRow = null;
                tmpRow = usageModuleCodesDT.NewRow();
                tmpRow["ID"] = oUMCodes[i].m_nObjectID;
                tmpRow["txt"] = oUMCodes[i].m_sVirtualName;
                usageModuleCodesDT.Rows.InsertAt(tmpRow, 0);
                usageModuleCodesDT.AcceptChanges();
            }
        }
        return usageModuleCodesDT;
    }

    private System.Data.DataTable GetCouponsDT(TvinciPricing.mdoule m, string sWSUserName, string sWSPass)
    {
        System.Data.DataTable CouponsGroupDT = GetBaseDT();
        

        TVinciShared.WS_Utils.GetWSUNPass(LoginManager.GetLoginGroupID(), "GetCouponGroupListForAdmin", "pricing", "1.1.1.1", ref sWSUserName, ref sWSPass);

        TvinciPricing.CouponsGroup[] oCouponsGroup = m.GetCouponGroupListForAdmin(sWSUserName, sWSPass);
        if (oCouponsGroup != null)
        {
            for (int i = 0; i < oCouponsGroup.Length; i++)
            {
                System.Data.DataRow tmpRow = null;
                tmpRow = CouponsGroupDT.NewRow();
                tmpRow["ID"] = int.Parse(oCouponsGroup[i].m_sGroupCode);
                tmpRow["txt"] = oCouponsGroup[i].m_sGroupName;
                /*
                TvinciPricing.LanguageContainer[] lang = oCouponsGroup[i].m_sDescription;
                if (lang != null)
                {
                    string sMainLang = GetMainLang();
                    for (int j = 0; j < lang.Length; j++)
                    {
                        if (lang[j].m_sLanguageCode3 == sMainLang)
                            tmpRow["txt"] += "(" + lang[j].m_sValue + ")";
                    }
                }
                */
                CouponsGroupDT.Rows.InsertAt(tmpRow, 0);
                CouponsGroupDT.AcceptChanges();
            }
        }

        return CouponsGroupDT;
    }

    public string GetPageContent(string sOrderBy, string sPageNum)
    {
        if (Session["error_msg"] != null && Session["error_msg"].ToString() != "")
        {
            Session["error_msg"] = "";
            return Session["last_page_html"].ToString();
        }
        object t = null; ;
        if (Session["campaign_id"] != null && Session["campaign_id"].ToString() != "" && int.Parse(Session["campaign_id"].ToString()) != 0)
            t = Session["campaign_id"];
        string sBack = "adm_campaigns.aspx?search_save=1";
        DBRecordWebEditor theRecord = new DBRecordWebEditor("campaigns", "adm_table_pager", sBack, "", "ID", t, sBack, "");

        DataRecordShortTextField dr_group_name = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_group_name.Initialize("Campaign Name", "adm_table_header_nbg", "FormInput", "NAME", true);
        theRecord.AddRecord(dr_group_name);

        DataRecordShortTextField dr_desc = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_desc.Initialize("Short Description", "adm_table_header_nbg", "FormInput", "DESCRIPTION", false);
        theRecord.AddRecord(dr_desc);

        DataRecordDropDownField dr_trigger_type = new DataRecordDropDownField("lu_campaign_action_types", "NAME", "id", "Action_Type", 1, 60, true);
        dr_trigger_type.SetNoSelectStr("---");
        dr_trigger_type.Initialize("Campaign Trigger", "adm_table_header_nbg", "FormInput", "trigger_type", false);
        theRecord.AddRecord(dr_trigger_type);

        DataRecordDropDownField dr_result_type = new DataRecordDropDownField("lu_campaign_action_types", "NAME", "id", "Action_Type", 2, 60, true);
        dr_result_type.SetNoSelectStr("---");
        dr_result_type.Initialize("Campaign Results", "adm_table_header_nbg", "FormInput", "result_type", false);
        theRecord.AddRecord(dr_result_type);

        DataRecordDateTimeField dr_start_date = new DataRecordDateTimeField(true);
        dr_start_date.Initialize("Campaign Start Date", "adm_table_header_nbg", "FormInput", "START_DATE", false);
        dr_start_date.SetDefault(DateTime.Now);
        theRecord.AddRecord(dr_start_date);

        DataRecordDateTimeField dr_end_date = new DataRecordDateTimeField(true);
        dr_end_date.Initialize("Campaign End Date", "adm_table_header_nbg", "FormInput", "END_DATE", false);
        theRecord.AddRecord(dr_end_date);

        TvinciPricing.mdoule m = new TvinciPricing.mdoule();
        string sWSURL = ApplicationConfiguration.WebServicesConfiguration.Pricing.URL.Value;
        if (sWSURL != "")
            m.Url = sWSURL;
        string sWSUserName = "";
        string sWSPass = "";

        
        TVinciShared.WS_Utils.GetWSUNPass(LoginManager.GetLoginGroupID(), "GetPriceCodeList", "pricing", "1.1.1.1", ref sWSUserName, ref sWSPass);

        DataRecordDropDownField dr_camp_usage_module = new DataRecordDropDownField("discount_codes", "code", "id", "", null, 60, false);
        dr_camp_usage_module.SetFieldType("int");
        System.Data.DataTable usageModuleCodesDT = GetUsageModulesDT(m, sWSUserName, sWSPass);
        dr_camp_usage_module.SetSelectsDT(usageModuleCodesDT);
        dr_camp_usage_module.Initialize("Usage Module", "adm_table_header_nbg", "FormInput", "USAGE_MODULE_ID", true);
        dr_camp_usage_module.SetDefault(0);
        theRecord.AddRecord(dr_camp_usage_module);

        DataRecordDropDownField dr_coupons_group = new DataRecordDropDownField("discount_codes", "code", "id", "", null, 60, true);
        dr_coupons_group.SetFieldType("string");
        dr_coupons_group.SetNoSelectStr("---");
        System.Data.DataTable CouponsGroupDT = GetCouponsDT(m, sWSUserName, sWSPass);

        dr_coupons_group.SetSelectsDT(CouponsGroupDT);
        dr_coupons_group.Initialize("Voucher Group ", "adm_table_header_nbg", "FormInput", "coupon_group_id", false);
        dr_coupons_group.SetDefault(0);
        theRecord.AddRecord(dr_coupons_group);

        DataRecordShortIntField dr_max_views = new DataRecordShortIntField(true, 60, 128);
        dr_max_views.Initialize("Maximum Views", "adm_table_header_nbg", "FormInput", "MAX_VIEWS", true);
        theRecord.AddRecord(dr_max_views);

        

        if (Session["campaign_type"] != null && !string.IsNullOrEmpty(Session["campaign_type"].ToString()))
        {
            DataRecordShortIntField dr_type = new DataRecordShortIntField(false, 9, 9);
            dr_type.Initialize("Type", "adm_table_header_nbg", "FormInput", "campaign_type", false);
            dr_type.SetValue(Session["campaign_type"].ToString());
            theRecord.AddRecord(dr_type);
        }
        DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
        dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", false);
        dr_groups.SetValue(LoginManager.GetLoginGroupID().ToString());
        theRecord.AddRecord(dr_groups);

        string sTable = theRecord.GetTableHTML("adm_campaigns_new.aspx?submited=1");

        return sTable;
    }
}
