using ConfigurationManager;
using System;
using TVinciShared;

public partial class adm_preview_modules_new : System.Web.UI.Page
{
    protected string m_sMenu;
    protected string m_sSubMenu;

    static public bool IsTvinciImpl()
    {
        Int32 nImplID = 0;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery.SetConnectionKey("pricing_connection");
        selectQuery += "select * from groups_modules_implementations where is_active=1 and status=1 and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", LoginManager.GetLoginGroupID());
        selectQuery += "and";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MODULE_ID", "=", 4);
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
            {
                nImplID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["IMPLEMENTATION_ID"].ToString());
            }
        }
        selectQuery.Finish();
        selectQuery = null;
        if (nImplID == 1)
            return true;
        return false;
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        Int32 nMenuID = 0;
        if (!IsPostBack)
        {
            if (IsTvinciImpl() == false)
            {
                Server.Transfer("adm_module_not_implemented.aspx");
                return;
            }

            m_sMenu = TVinciShared.Menu.GetMainMenu(14, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, true);
            if (Request.QueryString["preview_module_id"] != null &&
                Request.QueryString["preview_module_id"].ToString().Length > 0)
            {
                Session["preview_module_id"] = int.Parse(Request.QueryString["preview_module_id"].ToString());
                Int32 nOwnerGroupID = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("preview_modules", "group_id", int.Parse(Session["preview_module_id"].ToString()), "pricing_connection").ToString());
                Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();
                if (nLogedInGroupID != nOwnerGroupID && PageUtils.IsTvinciUser() == false)
                {
                    LoginManager.LogoutFromSite("login.html");
                    return;
                }
            }
            else
                Session["preview_module_id"] = 0;

        }

    }

    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
    }

    public void GetHeader()
    {
        string sRet = PageUtils.GetPreHeader() + ": Preview Module: ";
        if (Session["preview_module_id"] != null && Session["preview_module_id"].ToString().Length > 0 && Session["preview_module_id"].ToString() != "0")
        {
            int nPreviewModuleID = 0;
            Int32.TryParse(Session["preview_module_id"].ToString(), out nPreviewModuleID);
            object sPreviewModuleName = ODBCWrapper.Utils.GetTableSingleVal("preview_modules", "name", nPreviewModuleID ,"pricing_connection");
            if (sPreviewModuleName != null && sPreviewModuleName != DBNull.Value)
                sRet += "(" + sPreviewModuleName.ToString() + ")";
            sRet += " - Edit";
        }
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

    public string GetPPVPageContent(string sOrderBy, string sPageNum)
    {
        if (Session["error_msg"] != null && Session["error_msg"].ToString() != "")
        {
            Session["error_msg"] = "";
            return Session["last_page_html"].ToString();
        }
        object t = null; ;
        if (Session["preview_module_id"] != null && Session["preview_module_id"].ToString() != "" && int.Parse(Session["preview_module_id"].ToString()) != 0)
            t = Session["preview_module_id"];
        string sBack = "adm_preview_modules.aspx?search_save=1&type=1";
        DBRecordWebEditor theRecord = new DBRecordWebEditor("preview_modules", "adm_table_pager", sBack, "", "ID", t, sBack, "");
        theRecord.SetConnectionKey("pricing_connection");

        DataRecordShortTextField dr_domain = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_domain.Initialize("Code", "adm_table_header_nbg", "FormInput", "Name", true);
        theRecord.AddRecord(dr_domain);

       
        DataRecordDropDownField dr_view_lc = new DataRecordDropDownField("lu_min_periods", "DESCRIPTION", "id", "", null, 60, false);
        dr_view_lc.Initialize("View Life Cycle", "adm_table_header_nbg", "FormInput", "VIEW_LIFE_CYCLE_MIN", true);
        dr_view_lc.SetDefaultVal("1440");
        theRecord.AddRecord(dr_view_lc);

        DataRecordDropDownField dr_sub_price_codes = new DataRecordDropDownField("discount_codes", "code", "id", "", null, 60, false);
        dr_sub_price_codes.SetFieldType("string");
        System.Data.DataTable priceCodesDT = GetPriceCodeDT();
       

        dr_sub_price_codes.SetSelectsDT(priceCodesDT);
        dr_sub_price_codes.Initialize("Price Code", "adm_table_header_nbg", "FormInput", "pricing_id", true);
        dr_sub_price_codes.SetDefault(0);
        theRecord.AddRecord(dr_sub_price_codes);

        DataRecordDropDownField dr_disc = new DataRecordDropDownField("discount_codes", "code", "id", "", null, 60, true);
        dr_disc.SetFieldType("string");
        dr_disc.SetNoSelectStr("---");
        System.Data.DataTable discCodesDT = GetDiscountsDT();
      
        dr_disc.SetSelectsDT(discCodesDT);
        dr_disc.Initialize("Discount", "adm_table_header_nbg", "FormInput", "ext_discount_id", false);
        dr_disc.SetDefault(0);
        theRecord.AddRecord(dr_disc);

        DataRecordDropDownField dr_coupons_group = new DataRecordDropDownField("discount_codes", "code", "id", "", null, 60, true);
        dr_coupons_group.SetFieldType("string");
        dr_coupons_group.SetNoSelectStr("---");
        System.Data.DataTable CouponsGroupDT = GetCouponsDT();
        
        dr_coupons_group.SetSelectsDT(CouponsGroupDT);
        dr_coupons_group.Initialize("Coupon Group ", "adm_table_header_nbg", "FormInput", "coupon_id", false);
        dr_coupons_group.SetDefault(0);
        theRecord.AddRecord(dr_coupons_group);

        DataRecordShortDoubleField dr_max_views = new DataRecordShortDoubleField(true, 12, 12);
        dr_max_views.Initialize("Maximum Views", "adm_table_header_nbg", "FormInput", "MAX_VIEWS_NUMBER", true);
        dr_max_views.SetDefault(0);
        theRecord.AddRecord(dr_max_views);

        DataRecordBoolField dr_sub_only = new DataRecordBoolField(true);
        dr_sub_only.Initialize("Subscription Only", "adm_table_header_nbg", "FormInput", "SUBSCRIPTION_ONLY", false);
        theRecord.AddRecord(dr_sub_only);

        DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
        dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", false);
        dr_groups.SetValue(LoginManager.GetLoginGroupID().ToString());
        theRecord.AddRecord(dr_groups);

        DataRecordShortIntField dr_type = new DataRecordShortIntField(false, 9, 9);
        dr_type.Initialize("Type", "adm_table_header_nbg", "FormInput", "type", false);
        dr_type.SetValue("1");
        theRecord.AddRecord(dr_type);


        string sTable = theRecord.GetTableHTML("adm_preview_modules_new.aspx?submited=1");

        return sTable;
    }

   

    public string GetSubsPageContent(string sOrderBy, string sPageNum)
    {
        if (Session["error_msg"] != null && Session["error_msg"].ToString() != "")
        {
            Session["error_msg"] = "";
            return Session["last_page_html"].ToString();
        }
        object t = null; ;
        if (Session["preview_module_id"] != null && Session["preview_module_id"].ToString() != "" && int.Parse(Session["preview_module_id"].ToString()) != 0)
            t = Session["preview_module_id"];
        string sBack = "adm_preview_modules.aspx?search_save=1&type=2";
        DBRecordWebEditor theRecord = new DBRecordWebEditor("preview_modules", "adm_table_pager", sBack, "", "ID", t, sBack, "");
        theRecord.SetConnectionKey("pricing_connection");

        DataRecordShortTextField dr_domain = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_domain.Initialize("Code", "adm_table_header_nbg", "FormInput", "Name", true);
        theRecord.AddRecord(dr_domain);

        DataRecordDropDownField dr_full_lc = new DataRecordDropDownField("lu_min_periods", "DESCRIPTION", "id", "", null, 60, false);
        dr_full_lc.Initialize("Full Life Cycle (Subscription Period length)", "adm_table_header_nbg", "FormInput", "FULL_LIFE_CYCLE_ID", true);
        dr_full_lc.SetDefaultVal("1440");
        theRecord.AddRecord(dr_full_lc);

        DataRecordDropDownField dr_sub_price_codes = new DataRecordDropDownField("discount_codes", "code", "id", "", null, 60, false);
        dr_sub_price_codes.SetFieldType("string");
        System.Data.DataTable priceCodesDT = GetPriceCodeDT();

        dr_sub_price_codes.SetSelectsDT(priceCodesDT);
        dr_sub_price_codes.Initialize("Price Code", "adm_table_header_nbg", "FormInput", "pricing_id", true);
        dr_sub_price_codes.SetDefault(0);
        theRecord.AddRecord(dr_sub_price_codes);

        DataRecordDropDownField dr_disc_ext = new DataRecordDropDownField("discount_codes", "code", "id", "", null, 60, true);
        dr_disc_ext.SetFieldType("string");
        dr_disc_ext.SetNoSelectStr("---");
        System.Data.DataTable discCodesDT = GetDiscountsDT();
        dr_disc_ext.SetSelectsDT(discCodesDT);
        dr_disc_ext.Initialize("Subscription Discount", "adm_table_header_nbg", "FormInput", "ext_discount_id", false);
        dr_disc_ext.SetDefault(0);
        theRecord.AddRecord(dr_disc_ext);

        DataRecordDropDownField dr_disc_int = new DataRecordDropDownField("discount_codes", "code", "id", "", null, 60, true);
        dr_disc_int.SetFieldType("string");
        dr_disc_int.SetNoSelectStr("---");
        dr_disc_int.SetSelectsDT(discCodesDT);
        dr_disc_int.Initialize("Internal Discount", "adm_table_header_nbg", "FormInput", "internal_discount_id", false);
        dr_disc_int.SetDefault(0);
        theRecord.AddRecord(dr_disc_int);

        DataRecordDropDownField dr_coupons_group = new DataRecordDropDownField("discount_codes", "code", "id", "", null, 60, true);
        dr_coupons_group.SetFieldType("string");
        dr_coupons_group.SetNoSelectStr("---");
        System.Data.DataTable CouponsGroupDT = GetCouponsDT();
        
        dr_coupons_group.SetSelectsDT(CouponsGroupDT);
        dr_coupons_group.Initialize("Coupon Group ", "adm_table_header_nbg", "FormInput", "coupon_id", false);
        dr_coupons_group.SetDefault(0);
        theRecord.AddRecord(dr_coupons_group);
       

        DataRecordBoolField dr_sub_only = new DataRecordBoolField(true);
        dr_sub_only.Initialize("Is Renewable", "adm_table_header_nbg", "FormInput", "is_renew", false);
        theRecord.AddRecord(dr_sub_only);

        DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
        dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", false);
        dr_groups.SetValue(LoginManager.GetLoginGroupID().ToString());
        theRecord.AddRecord(dr_groups);

        DataRecordShortIntField dr_type = new DataRecordShortIntField(false, 9, 9);
        dr_type.Initialize("Type", "adm_table_header_nbg", "FormInput", "type", false);
        dr_type.SetValue("2");
        theRecord.AddRecord(dr_type);


        string sTable = theRecord.GetTableHTML("adm_usage_modules_new.aspx?submited=1");

        return sTable;
    }

    public string GetPageContent(string sOrderBy, string sPageNum)
    {

        if (Session["preview_module_type"] != null)
        {
            if (Session["preview_module_type"].ToString() == "1")
            {
                return GetPPVPageContent(sOrderBy, sPageNum);
            }
            else
            {
                if (Session["preview_module_type"].ToString() == "2")
                {
                    return GetSubsPageContent(sOrderBy, sPageNum);
                }
            }
        }
        if (Session["error_msg"] != null && Session["error_msg"].ToString() != "")
        {
            Session["error_msg"] = "";
            return Session["last_page_html"].ToString();
        }
        object t = null; ;
        if (Session["preview_module_id"] != null && Session["preview_module_id"].ToString() != "" && int.Parse(Session["preview_module_id"].ToString()) != 0)
            t = Session["preview_module_id"];
        string sBack = "adm_preview_modules.aspx?search_save=1";
        DBRecordWebEditor theRecord = new DBRecordWebEditor("preview_modules", "adm_table_pager", sBack, "", "ID", t, sBack, "");
        theRecord.SetConnectionKey("pricing_connection");

        DataRecordShortTextField dr_domain = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_domain.Initialize("Name", "adm_table_header_nbg", "FormInput", "Name", true);
        theRecord.AddRecord(dr_domain);

        DataRecordDropDownField dr_full_lc = new DataRecordDropDownField("lu_min_periods", "DESCRIPTION", "id", "", null, 60, false);
        dr_full_lc.Initialize("Full Life Cycle", "adm_table_header_nbg", "FormInput", "FULL_LIFE_CYCLE_ID", true);
        dr_full_lc.SetDefaultVal("1440");
        theRecord.AddRecord(dr_full_lc);

        DataRecordDropDownField dr_non_renewing_lc = new DataRecordDropDownField("lu_min_periods", "DESCRIPTION", "id", "", null, 60, false);
        dr_non_renewing_lc.Initialize("Non Renewing Period", "adm_table_header_nbg", "FormInput", "NON_RENEWING_PERIOD_ID", true);
        dr_non_renewing_lc.SetDefaultVal("1440");
        theRecord.AddRecord(dr_non_renewing_lc);

        DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
        dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", false);
        dr_groups.SetValue(LoginManager.GetLoginGroupID().ToString());
        theRecord.AddRecord(dr_groups);

        string sTable = theRecord.GetTableHTML("adm_discounts_new.aspx?submited=1");

        return sTable;
    }

    private System.Data.DataTable GetPriceCodeDT()
    {
        System.Data.DataTable priceCodesDT = GetBaseDT();
        string sWSUserName = "";
        string sWSPass = "";

        string sIP = "1.1.1.1";
        TVinciShared.WS_Utils.GetWSUNPass(LoginManager.GetLoginGroupID(), "GetPriceCodeList", "pricing", sIP, ref sWSUserName, ref sWSPass);
        TvinciPricing.mdoule m = new TvinciPricing.mdoule();
        string sWSURL = ApplicationConfiguration.WebServicesConfiguration.Pricing.URL.Value;
        if (sWSURL != "")
            m.Url = sWSURL;

        TvinciPricing.PriceCode[] oModules = m.GetPriceCodeList(sWSUserName, sWSPass,string.Empty,string.Empty,string.Empty);
        if (oModules != null)
        {
            for (int i = 0; i < oModules.Length; i++)
            {
                System.Data.DataRow tmpRow = null;
                tmpRow = priceCodesDT.NewRow();
                tmpRow["ID"] = oModules[i].m_nObjectID;
                tmpRow["txt"] = oModules[i].m_sCode;
                priceCodesDT.Rows.InsertAt(tmpRow, 0);
                priceCodesDT.AcceptChanges();
            }
        }

        return priceCodesDT;
    }

    private System.Data.DataTable GetDiscountsDT()
    {
        System.Data.DataTable discCodesDT = GetBaseDT();
        string sWSUserName = "";
        string sWSPass = "";
        TvinciPricing.mdoule m = new TvinciPricing.mdoule();
        string sWSURL = ApplicationConfiguration.WebServicesConfiguration.Pricing.URL.Value;
        if (sWSURL != "")
            m.Url = sWSURL;
        TVinciShared.WS_Utils.GetWSUNPass(LoginManager.GetLoginGroupID(), "GetDiscountsModuleListForAdmin", "pricing", "1.1.1.1", ref sWSUserName, ref sWSPass);
        TvinciPricing.DiscountModule[] oDiscCodes = m.GetDiscountsModuleListForAdmin(sWSUserName, sWSPass);
        if (oDiscCodes != null)
        {
            for (int i = 0; i < oDiscCodes.Length; i++)
            {
                System.Data.DataRow tmpRow = null;
                tmpRow = discCodesDT.NewRow();
                tmpRow["ID"] = oDiscCodes[i].m_nObjectID;
                tmpRow["txt"] = oDiscCodes[i].m_sCode;
                discCodesDT.Rows.InsertAt(tmpRow, 0);
                discCodesDT.AcceptChanges();
            }
        }

        return discCodesDT;
    }

    private System.Data.DataTable GetCouponsDT()
    {
        System.Data.DataTable CouponsGroupDT = GetBaseDT();
        string sWSUserName = "";
        string sWSPass = "";

        TVinciShared.WS_Utils.GetWSUNPass(LoginManager.GetLoginGroupID(), "GetCouponGroupListForAdmin", "pricing", "1.1.1.1", ref sWSUserName, ref sWSPass);
        TvinciPricing.mdoule m = new TvinciPricing.mdoule();
        string sWSURL = ApplicationConfiguration.WebServicesConfiguration.Pricing.URL.Value;
        if (sWSURL != "")
            m.Url = sWSURL;
        TvinciPricing.CouponsGroup[] oCouponsGroup = m.GetCouponGroupListForAdmin(sWSUserName, sWSPass);
        if (oCouponsGroup != null)
        {
            for (int i = 0; i < oCouponsGroup.Length; i++)
            {
                System.Data.DataRow tmpRow = null;
                tmpRow = CouponsGroupDT.NewRow();
                tmpRow["ID"] = int.Parse(oCouponsGroup[i].m_sGroupCode);
                tmpRow["txt"] = oCouponsGroup[i].m_sGroupName;
                CouponsGroupDT.Rows.InsertAt(tmpRow, 0);
                CouponsGroupDT.AcceptChanges();
            }
        }

        return CouponsGroupDT;
    }
}