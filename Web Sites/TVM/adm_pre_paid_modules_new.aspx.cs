using ConfigurationManager;
using System;
using System.Data;
using System.Web;
using TVinciShared;

public partial class adm_pre_paid_modules_new : System.Web.UI.Page
{
    protected string m_sMenu;
    protected string m_sSubMenu;
    protected string m_sLangMenu;

    static public bool IsTvinciImpl()
    {
        Int32 nImplID = 0;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery.SetConnectionKey("pricing_connection");
        selectQuery += "select * from groups_modules_implementations where is_active=1 and status=1 and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", LoginManager.GetLoginGroupID());
        selectQuery += "and";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MODULE_ID", "=", 5);
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
        string sMainCode3 = "";
        if (!IsPostBack)
        {
            if (IsTvinciImpl() == false)
            {
                Server.Transfer("adm_module_not_implemented.aspx");
                return;
            }

            if (Request.QueryString["submited"] != null && Request.QueryString["submited"].ToString() == "1")
            {
                Int32 nPrePaidModuleID = DBManipulator.DoTheWork("pricing_connection");
                Session["pre_paid_module_id"] = nPrePaidModuleID.ToString();
                Int32 nLangID = int.Parse(Session["lang_id"].ToString());
                string sCode3 = Session["lang_code"].ToString();
                int prePaidDescID = 0;
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select id from pre_paid_descriptions where is_active=1 and status=1 and ";
                selectQuery.SetConnectionKey("pricing_connection");
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("language_code3", "=", sCode3);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("pre_paid_module_id", "=", nPrePaidModuleID);
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                        prePaidDescID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["id"].ToString());
                }
                selectQuery.Finish();
                selectQuery = null;

                Int32 nIter = 7;
                string sLang = "";

                if (int.Parse(Session["lang_id"].ToString()) != GetMainLang(ref sLang, ref sMainCode3))
                    nIter = 0;
                string sDesc = "";
                sDesc = Request.Form[nIter.ToString() + "_val"].ToString();
                nIter++;

                if (prePaidDescID != 0)
                {
                    ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("pre_paid_descriptions");
                    updateQuery.SetConnectionKey("pricing_connection");
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("pre_paid_module_id", "=", nPrePaidModuleID);
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("language_code3", "=", sCode3);
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("DESCRIPTION", "=", sDesc);
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", LoginManager.GetLoginGroupID());
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATER_ID", "=", LoginManager.GetLoginID());
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATE_DATE", "=", DateTime.UtcNow);
                    updateQuery += " where ";
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", prePaidDescID);
                    updateQuery.Execute();
                    updateQuery.Finish();
                    updateQuery = null;
                }
                else
                {
                    ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("pre_paid_descriptions");
                    insertQuery.SetConnectionKey("pricing_connection");
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("pre_paid_module_id", "=", nPrePaidModuleID);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("language_code3", "=", sCode3);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("DESCRIPTION", "=", sDesc);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", LoginManager.GetLoginGroupID());
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATER_ID", "=", LoginManager.GetLoginID());
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATE_DATE", "=", DateTime.UtcNow);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CREATE_DATE", "=", DateTime.UtcNow);
                    insertQuery.Execute();
                    insertQuery.Finish();
                    insertQuery = null;
                }
                if (nPrePaidModuleID != 0)
                {
                    string name = Request.Form["0_val"].ToString();
                    DBManipulator.BuildFictivicMedia("Pre Paid", name, nPrePaidModuleID, LoginManager.GetLoginGroupID());
                }
                return;
            }
            m_sMenu = TVinciShared.Menu.GetMainMenu(14, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, true);
            string sMainLang = "";
            if (Request.QueryString["lang_id"] != null &&
               Request.QueryString["lang_id"].ToString() != "")
            {
                Session["lang_id"] = Request.QueryString["lang_id"].ToString();
                Session["lang_code"] = ODBCWrapper.Utils.GetTableSingleVal("lu_languages", "code3", int.Parse(Session["lang_id"].ToString()));
            }
            else
            {
                Session["lang_id"] = GetMainLang(ref sMainLang, ref sMainCode3);
                Session["lang_code"] = sMainCode3;
            }
            if (Request.QueryString["pp_module_id"] != null &&
                Request.QueryString["pp_module_id"].ToString() != "")
            {
                Session["pp_module_id"] = int.Parse(Request.QueryString["pp_module_id"].ToString());
                Int32 nOwnerGroupID = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("pre_paid_modules", "group_id", int.Parse(Session["pp_module_id"].ToString()), "pricing_connection").ToString());
                Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();
                if (nLogedInGroupID != nOwnerGroupID && PageUtils.IsTvinciUser() == false)
                {
                    LoginManager.LogoutFromSite("login.html");
                    return;
                }
            }
            else
                Session["pp_module_id"] = 0;

            
        }
        Int32 nGroupID = LoginManager.GetLoginGroupID();
        m_sLangMenu = GetLangMenu(nGroupID);
    }

    protected string GetLangMenu(Int32 nGroupID)
    {
        try
        {
            string sTemp = "";
            Int32 nCount = 0;
            string sMainLang = "";
            string sCode3 = "";
            Int32 nMainLangID = GetMainLang(ref sMainLang, ref sCode3);

            string sOnOff = "on";
            if (nMainLangID != int.Parse(Session["lang_id"].ToString()))
                sOnOff = "off";
            sTemp += "<li><a class=\"" + sOnOff + "\" href=\"";
            if (nMainLangID != int.Parse(Session["lang_id"].ToString()))
                sTemp += "adm_pre_paid_modules_new.aspx?pp_module_id=" + Session["pp_module_id"].ToString() + "&lang_id=" + nMainLangID.ToString();
            else
                sTemp += "javascript:void(0);";
            sTemp += "\"><span>";
            sTemp += sMainLang;
            sTemp += "</span></a></li>";

            ODBCWrapper.DataSetSelectQuery selectQuery1 = new ODBCWrapper.DataSetSelectQuery();
            selectQuery1 += "select l.name,l.id from group_extra_languages gel,lu_languages l where gel.language_id=l.id and l.status=1 and gel.status=1 and  ";
            selectQuery1 += ODBCWrapper.Parameter.NEW_PARAM("gel.group_id", "=", nGroupID);
            selectQuery1 += " order by l.name";
            if (selectQuery1.Execute("query", true) != null)
            {
                Int32 nCount1 = selectQuery1.Table("query").DefaultView.Count;
                for (int i = 0; i < nCount1; i++)
                {
                    Int32 nLangID = int.Parse(selectQuery1.Table("query").DefaultView[i].Row["id"].ToString());
                    string nLangName = selectQuery1.Table("query").DefaultView[i].Row["name"].ToString();
                    sOnOff = "on";
                    if (nLangID != int.Parse(Session["lang_id"].ToString()))
                        sOnOff = "off";
                    sTemp += "<li><a class=\"" + sOnOff + "\" href=\"";
                    if (nLangID != int.Parse(Session["lang_id"].ToString()))
                    {
                        sTemp += "adm_pre_paid_modules_new.aspx?pp_module_id=" + Session["pp_module_id"].ToString() + "&lang_id=" + nLangID.ToString();
                    }
                    else
                        sTemp += "javascript:void(0);";
                    sTemp += "\"><span>";
                    sTemp += nLangName;
                    sTemp += "</span></a></li>";
                }
                if (nCount1 == 0)
                    sTemp = "";
            }
            selectQuery1.Finish();
            selectQuery1 = null;

            return sTemp;
        }
        catch
        {
            HttpContext.Current.Response.Redirect("login.html");
            return "";
        }
    }

    protected void GetLangMenu()
    {
        Response.Write(m_sLangMenu);
    }

    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
    }

    static protected Int32 GetMainLang(ref string sMainLang, ref string sCode)
    {
        Int32 nLangID = 0;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select l.CODE3,l.NAME,l.id from groups g,lu_languages l where l.id=g.language_id and  ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("g.id", "=", LoginManager.GetLoginGroupID());
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
            {
                sMainLang = selectQuery.Table("query").DefaultView[0].Row["NAME"].ToString();
                nLangID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
                sCode = selectQuery.Table("query").DefaultView[0].Row["CODE3"].ToString();
            }
        }
        selectQuery.Finish();
        selectQuery = null;
        return nLangID;
    }

    public void GetHeader()
    {
        string sRet = PageUtils.GetPreHeader() + ": Pre Paid Module: ";
        if (Session["pp_module_id"] != null && Session["pp_module_id"].ToString() != "" && Session["pp_module_id"].ToString() != "0")
        {
            string sMainLang = "";
            string sCode3 = "";
            Int32 nLangID = GetMainLang(ref sMainLang, ref sCode3);
            object sSubName = ODBCWrapper.Utils.GetTableSingleVal("pre_paid_descriptions", "description", "language_code3", "=", sMainLang, "pricing_connection");
            if (sSubName != null && sSubName != DBNull.Value)
                sRet += "(" + sSubName.ToString() + ")";
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

    protected string GetCurrentValue(string sField, string sTable, Int32 npp_module_idID, string sLangCode, string sConnKey)
    {
        string sRet = "";
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery.SetConnectionKey(sConnKey);
        selectQuery += "select " + sField + " from " + sTable + " where ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("pre_paid_module_id", "=", npp_module_idID);
        selectQuery += " and is_active=1 and status=1 and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("language_code3", "=", sLangCode);
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
            {
                object oRet = selectQuery.Table("query").DefaultView[0].Row[sField];
                if (oRet != null && oRet != DBNull.Value)
                    sRet = oRet.ToString();
            }
        }
        selectQuery.Finish();
        selectQuery = null;
        return sRet;
    }

    protected DataTable GetPriceCodesDT(TvinciPricing.mdoule m, string sWSUserName, string sWSPass)
    {
        System.Data.DataTable priceCodesDT = GetBaseDT();

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

    protected DataTable GetDiscountsDT(TvinciPricing.mdoule m, string sWSUserName, string sWSPass)
    {
        System.Data.DataTable discCodesDT = GetBaseDT();
        
        
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

    protected DataTable GetCouponsDT(TvinciPricing.mdoule m, string sWSUserName, string sWSPass, ref string sMainLang)
    {
        System.Data.DataTable CouponsGroupDT = GetBaseDT();
        TvinciPricing.CouponsGroup[] oCouponsGroup = m.GetCouponGroupListForAdmin(sWSUserName, sWSPass);
        if (oCouponsGroup != null)
        {
            for (int i = 0; i < oCouponsGroup.Length; i++)
            {
                System.Data.DataRow tmpRow = null;
                tmpRow = CouponsGroupDT.NewRow();
                tmpRow["ID"] = int.Parse(oCouponsGroup[i].m_sGroupCode);
                tmpRow["txt"] = oCouponsGroup[i].m_sGroupName;
                TvinciPricing.LanguageContainer[] lang = oCouponsGroup[i].m_sDescription;
                if (lang != null)
                {
                    string sCode3 = "";
                    Int32 nLangID = GetMainLang(ref sMainLang, ref sCode3);
                    for (int j = 0; j < lang.Length; j++)
                    {
                        if (lang[j].m_sLanguageCode3 == sMainLang)
                            tmpRow["txt"] += "(" + lang[j].m_sValue + ")";
                    }
                }
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
        if (Session["pp_module_id"] != null && Session["pp_module_id"].ToString() != "" && int.Parse(Session["pp_module_id"].ToString()) != 0)
            t = Session["pp_module_id"];
        string sBack = "adm_pre_paid_modules.aspx?search_save=1";
        DBRecordWebEditor theRecord = new DBRecordWebEditor("pre_paid_modules", "adm_table_pager", sBack, "", "ID", t, sBack, "");
        theRecord.SetConnectionKey("pricing_connection");
        string sMainLang = "";
        string sMainCode = "";
        TvinciPricing.mdoule m = new TvinciPricing.mdoule();
        string sWSURL = ApplicationConfiguration.WebServicesConfiguration.Pricing.URL.Value;
        if (sWSURL != "")
            m.Url = sWSURL;
        string sWSUserName = "";
        string sWSPass = "";

        string sIP = "1.1.1.1";
        TVinciShared.WS_Utils.GetWSUNPass(LoginManager.GetLoginGroupID(), "GetPriceCodeList", "pricing", "1.1.1.1", ref sWSUserName, ref sWSPass);
        if (int.Parse(Session["lang_id"].ToString()) == GetMainLang(ref sMainLang, ref sMainCode))
        {
            DataRecordDropDownField dr_sub_price_codes = new DataRecordDropDownField("discount_codes", "code", "id", "", null, 60, false);
            dr_sub_price_codes.SetFieldType("int");
            System.Data.DataTable priceCodesDT = GetPriceCodesDT(m, sWSUserName, sWSPass);
            

            DataRecordShortTextField dr_domain = new DataRecordShortTextField("ltr", true, 60, 128);
            dr_domain.Initialize("Code", "adm_table_header_nbg", "FormInput", "Name", true);
            theRecord.AddRecord(dr_domain);

            dr_sub_price_codes.SetSelectsDT(priceCodesDT);
            dr_sub_price_codes.Initialize("Price Code", "adm_table_header_nbg", "FormInput", "PRICE_CODE", true);
            dr_sub_price_codes.SetDefault(0);
            theRecord.AddRecord(dr_sub_price_codes);

            DataRecordDropDownField dr_credit_price_codes = new DataRecordDropDownField("discount_codes", "code", "id", "", null, 60, false);
            dr_credit_price_codes.SetFieldType("int");
            System.Data.DataTable creditCodesDT = GetPriceCodesDT(m, sWSUserName, sWSPass);

            dr_credit_price_codes.SetSelectsDT(priceCodesDT);
            dr_credit_price_codes.Initialize("Credit Value", "adm_table_header_nbg", "FormInput", "VALUE_PRICE_CODE", true);
            dr_credit_price_codes.SetDefault(0);
            theRecord.AddRecord(dr_credit_price_codes);

            DataRecordDropDownField dr_sub_usage_module = new DataRecordDropDownField("discount_codes", "code", "id", "", null, 60, false);
            dr_sub_usage_module.SetFieldType("int");
            System.Data.DataTable usageModuleCodesDT = GetUsageModulesDT(m, sWSUserName, sWSPass);
            dr_sub_usage_module.SetSelectsDT(usageModuleCodesDT);
            dr_sub_usage_module.Initialize("Usage Module", "adm_table_header_nbg", "FormInput", "USAGE_MODULE_CODE", true);
            dr_sub_usage_module.SetDefault(0);
            theRecord.AddRecord(dr_sub_usage_module);

            DataRecordDropDownField dr_disc = new DataRecordDropDownField("discount_codes", "code", "id", "", null, 60, true);
            dr_disc.SetFieldType("int");
            System.Data.DataTable discCodesDT = GetDiscountsDT(m, sWSUserName, sWSPass);
            
            dr_disc.SetSelectsDT(discCodesDT);
            dr_disc.Initialize("Discounts", "adm_table_header_nbg", "FormInput", "DISCOUNT_MODULE_CODE", false);
            dr_disc.SetNoSelectStr("---");
            dr_disc.SetDefault(0);
            theRecord.AddRecord(dr_disc);

            DataRecordDropDownField dr_coupons_group = new DataRecordDropDownField("discount_codes", "code", "id", "", null, 60, true);
            dr_coupons_group.SetFieldType("int");
            System.Data.DataTable CouponsGroupDT = GetCouponsDT(m, sWSUserName, sWSPass, ref sMainLang);
           
            dr_coupons_group.SetSelectsDT(CouponsGroupDT);
            dr_coupons_group.Initialize("Coupon Group", "adm_table_header_nbg", "FormInput", "COUPON_GROUP_CODE", false);
            dr_coupons_group.SetNoSelectStr("---");
            //dr_coupons_group.SetDefault(0);
            theRecord.AddRecord(dr_coupons_group);

            DataRecordBoolField dr_is_fixed = new DataRecordBoolField(true);
            dr_is_fixed.Initialize("Is Fixed Credit", "adm_table_header_nbg", "FormInput", "IS_FIXED_PRICE", false);
            theRecord.AddRecord(dr_is_fixed);
        }

        DataRecordLongTextField dr_Description = new DataRecordLongTextField("ltr", true, 60, 4);
        dr_Description.Initialize("Title", "adm_table_header_nbg", "FormInput", "", false);
        if (Session["pp_module_id"] != null && Session["pp_module_id"].ToString() != "0")
        {
            dr_Description.SetValue(GetCurrentValue("description", "pre_paid_descriptions", int.Parse(Session["pp_module_id"].ToString()), Session["lang_code"].ToString(), "pricing_connection"));
        }
        else
            dr_Description.SetValue("");
        theRecord.AddRecord(dr_Description);

        DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
        dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", false);
        dr_groups.SetValue(LoginManager.GetLoginGroupID().ToString());
        theRecord.AddRecord(dr_groups);

        string sTable = theRecord.GetTableHTML("adm_pre_paid_modules_new.aspx?submited=1");

        return sTable;
    }
}