using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TVinciShared;
using System.Configuration;
using KLogMonitor;
using System.Reflection;

public partial class adm_usage_modules_new : System.Web.UI.Page
{
    private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
    protected string m_sMenu;
    protected string m_sSubMenu;

    static public bool IsTvinciImpl()
    {
        Int32 nImplID = 0;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery.SetConnectionKey("pricing_connection");
        selectQuery += "select IMPLEMENTATION_ID from groups_modules_implementations where is_active=1 and status=1 and ";
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
        if (nImplID > 0)
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

            if (Request.QueryString["submited"] != null && Request.QueryString["submited"].ToString() == "1")
            {
                int usageModuleID = DBManipulator.DoTheWork("pricing_connection");
                if (!string.IsNullOrEmpty(Session["usage_module_type"].ToString()) && Session["usage_module_type"].ToString() == "2")
                {
                    ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                    selectQuery.SetConnectionKey("pricing_connection");
                    selectQuery += @"select subscription_id from 
                                    (
                                    select sum.subscription_id, sum.order_num, sum.usage_module_id, dense_rank() over (partition by sum.subscription_id order by sum.order_num asc) rn 
                                    from subscriptions_usage_modules sum
                                    join usage_modules um on (sum.usage_module_id=um.id and um.status=1 and um.IS_ACTIVE=1 and ";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("um.group_id", "=", LoginManager.GetLoginGroupID());
                    selectQuery += @")
                                    where subscription_id in (
						                                        select subscription_id
						                                        from subscriptions_usage_modules
						                                        where is_active=1 and status=1
						                                        and ";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("usage_module_id", "=", usageModuleID);
                    selectQuery += @"                           )
                                    and sum.status=1 and sum.is_Active=1
                                    ) a
                                    where rn=1 and ";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("usage_module_id", "=", usageModuleID);
                    if (selectQuery.Execute("query", true) != null)
                    {
                        int count = selectQuery.Table("query").DefaultView.Count;
                        if (count > 0)
                        {
                            string priceCode = string.Empty;
                            int nExtDiscountID = 0;
                            ODBCWrapper.DataSetSelectQuery umSelectQuery = new ODBCWrapper.DataSetSelectQuery();
                            umSelectQuery.SetConnectionKey("pricing_connection");
                            umSelectQuery += "select id, pricing_id, ext_discount_id from usage_modules where ";
                            umSelectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", usageModuleID);
                            if (umSelectQuery.Execute("query", true) != null)
                            {
                                int umCount = umSelectQuery.Table("query").DefaultView.Count;
                                if (umCount > 0)
                                {
                                    priceCode = umSelectQuery.Table("query").DefaultView[0].Row["pricing_id"].ToString();
                                    log.Debug("MultiUM - Found price code " + priceCode.ToString());

                                    nExtDiscountID = int.Parse(umSelectQuery.Table("query").DefaultView[0].Row["ext_discount_id"].ToString());

                                }
                            }
                            umSelectQuery.Finish();
                            umSelectQuery = null;
                            for (int i = 0; i < count; i++)
                            {

                                int subID = int.Parse(selectQuery.Table("query").DefaultView[i].Row["subscription_id"].ToString());
                                log.Debug("MultiUM - Updating subscription " + subID.ToString());
                                ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("subscriptions");
                                updateQuery.SetConnectionKey("pricing_connection");
                                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("SUB_PRICE_CODE", "=", priceCode);
                                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("Ext_discount_module", "=", nExtDiscountID);
                                updateQuery += " where ";
                                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", subID);
                                updateQuery.Execute();
                                updateQuery.Finish();
                                updateQuery = null;
                            }
                        }
                    }
                    selectQuery.Finish();
                    selectQuery = null;
                }
                return;
            }
            m_sMenu = TVinciShared.Menu.GetMainMenu(14, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, true);
            if (Request.QueryString["usage_module_id"] != null &&
                Request.QueryString["usage_module_id"].ToString() != "")
            {
                Session["usage_module_id"] = int.Parse(Request.QueryString["usage_module_id"].ToString());
                Int32 nOwnerGroupID = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("usage_modules", "group_id", int.Parse(Session["usage_module_id"].ToString()), "pricing_connection").ToString());
                Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();
                if (nLogedInGroupID != nOwnerGroupID && PageUtils.IsTvinciUser() == false)
                {
                    LoginManager.LogoutFromSite("login.html");
                    return;
                }
            }
            else
                Session["usage_module_id"] = 0;

        }

    }

    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
    }

    public void GetHeader()
    {
        string sRet = string.Empty;
        if (Session["usage_module_type"] != null)
        {
            if (Session["usage_module_type"].ToString() == "1")
            {
                sRet = PageUtils.GetPreHeader() + ": PPV Usage Modules";
            }
            else if (Session["usage_module_type"].ToString() == "2")
            {
                sRet = PageUtils.GetPreHeader() + ": Subscription Usage Modules";
            }
        }
        if (Session["usage_module_id"] != null && Session["usage_module_id"].ToString() != "" && Session["usage_module_id"].ToString() != "0")
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

    static protected string GetPricingWSURL()
    {
        return TVinciShared.WS_Utils.GetTcmConfigValue("pricing_ws");
    }


    public string GetPPVPageContent(string sOrderBy, string sPageNum)
    {
        if (Session["error_msg"] != null && Session["error_msg"].ToString() != "")
        {
            Session["error_msg"] = "";
            return Session["last_page_html"].ToString();
        }
        object t = null; ;
        if (Session["usage_module_id"] != null && Session["usage_module_id"].ToString() != "" && int.Parse(Session["usage_module_id"].ToString()) != 0)
            t = Session["usage_module_id"];
        string sBack = "adm_usage_modules.aspx?search_save=1&type=1";
        DBRecordWebEditor theRecord = new DBRecordWebEditor("usage_modules", "adm_table_pager", sBack, "", "ID", t, sBack, "");
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




        string sTable = theRecord.GetTableHTML("adm_usage_modules_new.aspx?submited=1");

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
        if (Session["usage_module_id"] != null && Session["usage_module_id"].ToString() != "" && int.Parse(Session["usage_module_id"].ToString()) != 0)
            t = Session["usage_module_id"];
        string sBack = "adm_usage_modules.aspx?search_save=1&type=2";
        DBRecordWebEditor theRecord = new DBRecordWebEditor("usage_modules", "adm_table_pager", sBack, "", "ID", t, sBack, "");
        theRecord.SetConnectionKey("pricing_connection");

        DataRecordShortTextField dr_domain = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_domain.Initialize("Code", "adm_table_header_nbg", "FormInput", "Name", true);
        theRecord.AddRecord(dr_domain);

        DataRecordDropDownField dr_full_lc = new DataRecordDropDownField("lu_min_periods", "DESCRIPTION", "id", "", null, 60, false);
        dr_full_lc.Initialize("Full Life Cycle (Subscription Period length)", "adm_table_header_nbg", "FormInput", "FULL_LIFE_CYCLE_MIN", true);
        dr_full_lc.SetDefaultVal("1440");
        theRecord.AddRecord(dr_full_lc);
        /*
        DataRecordShortDoubleField dr_full_lc = new DataRecordShortDoubleField(true, 12, 12);
        dr_full_lc.Initialize("Full Life Cycle (Min)", "adm_table_header_nbg", "FormInput", "FULL_LIFE_CYCLE_MIN", true);
        dr_full_lc.SetDefault(44640);
        theRecord.AddRecord(dr_full_lc);
        */

        DataRecordDropDownField dr_view_lc = new DataRecordDropDownField("lu_min_periods", "DESCRIPTION", "id", "", null, 60, false);
        dr_view_lc.Initialize("View Life Cycle", "adm_table_header_nbg", "FormInput", "VIEW_LIFE_CYCLE_MIN", true);
        dr_view_lc.SetDefaultVal("1440");
        theRecord.AddRecord(dr_view_lc);

        DataRecordShortDoubleField dr_max_views = new DataRecordShortDoubleField(true, 12, 12);
        dr_max_views.Initialize("Maximum Views", "adm_table_header_nbg", "FormInput", "MAX_VIEWS_NUMBER", true);
        dr_max_views.SetDefault(0);
        theRecord.AddRecord(dr_max_views);

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
        //dr_coupons_group.SetDefault(0);
        theRecord.AddRecord(dr_coupons_group);
        /*
        DataRecordShortDoubleField dr_view_lc = new DataRecordShortDoubleField(true, 12, 12);
        dr_view_lc.Initialize("View Life Cycle (Min)", "adm_table_header_nbg", "FormInput", "VIEW_LIFE_CYCLE_MIN", true);
        dr_view_lc.SetDefault(1440);
        theRecord.AddRecord(dr_view_lc);
        */


        DataRecordBoolField dr_sub_only = new DataRecordBoolField(true);
        dr_sub_only.Initialize("Is Renewable", "adm_table_header_nbg", "FormInput", "is_renew", false);
        theRecord.AddRecord(dr_sub_only);

        DataRecordShortDoubleField dr_num_of_rec_periods = new DataRecordShortDoubleField(true, 12, 12);
        dr_num_of_rec_periods.Initialize("Recurring Periods(0 for unlimited)", "adm_table_header_nbg", "FormInput", "num_of_rec_periods", true);
        dr_num_of_rec_periods.SetDefault(0);
        theRecord.AddRecord(dr_num_of_rec_periods);


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

        if (Session["usage_module_type"] != null)
        {
            if (Session["usage_module_type"].ToString() == "1")
            {
                return GetPPVPageContent(sOrderBy, sPageNum);
            }
            else
            {
                if (Session["usage_module_type"].ToString() == "2")
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
        if (Session["usage_module_id"] != null && Session["usage_module_id"].ToString() != "" && int.Parse(Session["usage_module_id"].ToString()) != 0)
            t = Session["usage_module_id"];
        string sBack = "adm_usage_modules.aspx?search_save=1";
        DBRecordWebEditor theRecord = new DBRecordWebEditor("usage_modules", "adm_table_pager", sBack, "", "ID", t, sBack, "");
        theRecord.SetConnectionKey("pricing_connection");

        DataRecordShortTextField dr_domain = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_domain.Initialize("Code", "adm_table_header_nbg", "FormInput", "Name", true);
        theRecord.AddRecord(dr_domain);

        DataRecordDropDownField dr_full_lc = new DataRecordDropDownField("lu_min_periods", "DESCRIPTION", "id", "", null, 60, false);
        dr_full_lc.Initialize("Full Life Cycle", "adm_table_header_nbg", "FormInput", "FULL_LIFE_CYCLE_MIN", true);
        dr_full_lc.SetDefaultVal("1440");
        theRecord.AddRecord(dr_full_lc);

        DataRecordDropDownField dr_view_lc = new DataRecordDropDownField("lu_min_periods", "DESCRIPTION", "id", "", null, 60, false);
        dr_view_lc.Initialize("View Life Cycle", "adm_table_header_nbg", "FormInput", "VIEW_LIFE_CYCLE_MIN", true);
        dr_view_lc.SetDefaultVal("1440");
        theRecord.AddRecord(dr_view_lc);

        DataRecordShortDoubleField dr_max_views = new DataRecordShortDoubleField(true, 12, 12);
        dr_max_views.Initialize("Maximum Views", "adm_table_header_nbg", "FormInput", "MAX_VIEWS_NUMBER", true);
        dr_max_views.SetDefault(0);
        theRecord.AddRecord(dr_max_views);

        DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
        dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", false);
        dr_groups.SetValue(LoginManager.GetLoginGroupID().ToString());
        theRecord.AddRecord(dr_groups);


        // get default value from configuration for waiver_period by group id

        object oWaiverPeriod = ODBCWrapper.Utils.GetTableSingleVal("groups", "WAIVER_PERIOD", LoginManager.GetLoginGroupID());
        object oWaiver = 0;
        if (t != null)
        {
            try
            {
                int usageModuleID = int.Parse(t.ToString());
                oWaiver = ODBCWrapper.Utils.GetTableSingleVal("usage_modules", "waiver", usageModuleID, 0, "pricing_connection");
            }
            catch
            {
            }
        }

        int nWaiverPeriod = ODBCWrapper.Utils.GetIntSafeVal(oWaiverPeriod);
        int nWaiver = ODBCWrapper.Utils.GetIntSafeVal(oWaiver);
        // get the value for  WAIVE from usage_module table - if it's an edit item

        DataRecordCheckBoxField dr_waiver = new DataRecordCheckBoxField(true);
        dr_waiver.Initialize("Waiver", "adm_table_header_nbg", "FormInput", "waiver", false);

        if (nWaiverPeriod > 0 || nWaiver > 0)
        {
            dr_waiver.SetValue("1");
        }
        else
        {
            dr_waiver.SetValue("0");
        }

        theRecord.AddRecord(dr_waiver);

        //cancellation regulation
        DataRecordDropDownField dr_waiver_period = new DataRecordDropDownField("lu_min_periods", "DESCRIPTION", "id", "", null, 60, true);
        dr_waiver_period.Initialize("Waiver Period", "adm_table_header_nbg", "FormInput", "waiver_period", false);
        //dr_waiver_period.SetDefaultVal(nWaiverPeriod.ToString());// ("20160");
        theRecord.AddRecord(dr_waiver_period);


        DataRecordCheckBoxField dr_OfflinePlayback = new DataRecordCheckBoxField(true);
        dr_OfflinePlayback.Initialize("Offline Playback", "adm_table_header_nbg", "FormInput", "offline_playback", false);

        theRecord.AddRecord(dr_OfflinePlayback);
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
        string sWSURL = GetPricingWSURL();
        if (sWSURL != "")
            m.Url = sWSURL;

        TvinciPricing.PriceCode[] oModules = m.GetPriceCodeList(sWSUserName, sWSPass, string.Empty, string.Empty, string.Empty);
        if (oModules != null)
        {
            for (int i = 0; i < oModules.Length; i++)
            {
                System.Data.DataRow tmpRow = null;
                tmpRow = priceCodesDT.NewRow();
                tmpRow["ID"] = oModules[i].m_nObjectID;
                tmpRow["txt"] = oModules[i].m_sCode;
                /*
                TvinciPricing.LanguageContainer[] lang = oModules[i].m_sDescription;
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
        string sWSURL = GetPricingWSURL();
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
                /*
                TvinciPricing.LanguageContainer[] lang = oDiscCodes[i].m_sDescription;
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
        string sWSURL = GetPricingWSURL();
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
}
