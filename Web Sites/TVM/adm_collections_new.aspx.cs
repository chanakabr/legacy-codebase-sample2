using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using TVinciShared;
using TvinciImporter;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;

public partial class adm_collections_new : System.Web.UI.Page
{
    protected string m_sMenu;
    protected string m_sSubMenu;
    protected string m_sLangMenu;
    private const string OLD_COL_NAME_SESSION_KEY = "OldColName";

    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        if (LoginManager.IsPagePermitted("adm_collections.aspx") == false)
            LoginManager.LogoutFromSite("login.html");
        if (LoginManager.IsActionPermittedOnPage("adm_collections.aspx", LoginManager.PAGE_PERMISION_TYPE.EDIT) == false)
            LoginManager.LogoutFromSite("login.html");

        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;

        int nMenuID = 0;
        string sMainLang = "";
        string sMainCode3 = "";

        if (!IsPostBack)
        {
            if (Request.QueryString["submited"] != null && Request.QueryString["submited"].ToString().Trim() == "1")
            {
                int nCollectionID = 0;
                nCollectionID = DBManipulator.DoTheWork("pricing_connection");
                Session["collection_id"] = nCollectionID.ToString();
                Int32 nLangID = int.Parse(Session["lang_id"].ToString());
                string sCode3 = Session["lang_code"].ToString();
                Int32 nCollectionDesc = 0;
                Int32 nCollectionName = 0;

                UpdateCouponsGroup(nCollectionID, LoginManager.GetLoginGroupID());

                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select id from collection_descriptions with (nolock) where is_active=1 and status=1 and ";
                selectQuery.SetConnectionKey("pricing_connection");
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("language_code3", "=", sCode3);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("collection_id", "=", nCollectionID);
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                        nCollectionDesc = int.Parse(selectQuery.Table("query").DefaultView[0].Row["id"].ToString());
                }
                selectQuery.Finish();
                selectQuery = null;

                ODBCWrapper.DataSetSelectQuery selectQuery2 = new ODBCWrapper.DataSetSelectQuery();
                selectQuery2 += "select id from collection_names with (nolock) where is_active=1 and status=1 and ";
                selectQuery2.SetConnectionKey("pricing_connection");
                selectQuery2 += ODBCWrapper.Parameter.NEW_PARAM("language_code3", "=", sCode3);
                selectQuery2 += "and";
                selectQuery2 += ODBCWrapper.Parameter.NEW_PARAM("collection_id", "=", nCollectionID);
                if (selectQuery2.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery2.Table("query").DefaultView.Count;
                    if (nCount > 0)
                        nCollectionName = int.Parse(selectQuery2.Table("query").DefaultView[0].Row["id"].ToString());
                }
                selectQuery2.Finish();
                selectQuery2 = null;

                Int32 nIter = 8;
                string sLang = "";

                if (int.Parse(Session["lang_id"].ToString()) != GetMainLang(ref sLang, ref sMainCode3))
                    nIter = 0;
                string sName = "";
                sName = Request.Form[nIter.ToString() + "_val"].ToString();
                nIter++;
                string sDesc = "";
                sDesc = Request.Form[nIter.ToString() + "_val"].ToString();
                nIter++;

                if (nCollectionDesc != 0)
                {
                    ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("collection_descriptions");
                    updateQuery.SetConnectionKey("pricing_connection");
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("collection_id", "=", nCollectionID);
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("language_code3", "=", sCode3);
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("DESCRIPTION", "=", sDesc);
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", LoginManager.GetLoginGroupID());
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATER_ID", "=", LoginManager.GetLoginID());
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATE_DATE", "=", DateTime.UtcNow);
                    updateQuery += " where ";
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nCollectionDesc);
                    updateQuery.Execute();
                    updateQuery.Finish();
                    updateQuery = null;
                }
                else
                {
                    ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("collection_descriptions");
                    insertQuery.SetConnectionKey("pricing_connection");
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("collection_id", "=", nCollectionID);
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

                if (nCollectionName != 0)
                {
                    ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("collection_names");
                    updateQuery.SetConnectionKey("pricing_connection");
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("collection_id", "=", nCollectionID);
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("language_code3", "=", sCode3);
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("DESCRIPTION", "=", sName);
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", LoginManager.GetLoginGroupID());
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATER_ID", "=", LoginManager.GetLoginID());
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATE_DATE", "=", DateTime.UtcNow);
                    updateQuery += " where ";
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nCollectionName);
                    updateQuery.Execute();
                    updateQuery.Finish();
                    updateQuery = null;
                }
                else
                {
                    ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("collection_names");
                    insertQuery.SetConnectionKey("pricing_connection");
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("collection_id", "=", nCollectionID);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("language_code3", "=", sCode3);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("DESCRIPTION", "=", sName);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", LoginManager.GetLoginGroupID());
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATER_ID", "=", LoginManager.GetLoginID());
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATE_DATE", "=", DateTime.UtcNow);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CREATE_DATE", "=", DateTime.UtcNow);
                    insertQuery.Execute();
                    insertQuery.Finish();
                    insertQuery = null;
                }

                if (nCollectionID != 0)
                {
                    string sColName = "";
                    ODBCWrapper.DataSetSelectQuery selectQuery1 = new ODBCWrapper.DataSetSelectQuery();
                    selectQuery1.SetConnectionKey("pricing_connection");
                    selectQuery1 += "select * from collections with (nolock) where ";
                    selectQuery1 += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nCollectionID);
                    if (selectQuery1.Execute("query", true) != null)
                    {
                        Int32 nCount = selectQuery1.Table("query").DefaultView.Count;
                        if (nCount > 0)
                        {
                            sColName = selectQuery1.Table("query").DefaultView[0].Row["NAME"].ToString();
                        }
                    }
                    selectQuery1.Finish();
                    selectQuery1 = null;

                    if (sColName.Length > 0)
                    {
                        DBManipulator.BuildOrUpdateFictivicMedia("Collection", sColName, nCollectionID, LoginManager.GetLoginGroupID(), Session[OLD_COL_NAME_SESSION_KEY] != null ? Session[OLD_COL_NAME_SESSION_KEY].ToString() : string.Empty);
                    }
                }

                return;
            }
                m_sMenu = TVinciShared.Menu.GetMainMenu(7, true, ref nMenuID);
                m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, true);

                if (Request.QueryString["lang_id"] != null &&
                   Request.QueryString["lang_id"].ToString() != "")
                {
                    Session["lang_id"]   = Request.QueryString["lang_id"].ToString();
                    Session["lang_code"] = ODBCWrapper.Utils.GetTableSingleVal("lu_languages", "code3", int.Parse(Session["lang_id"].ToString()));
                }
                else
                {
                    Session["lang_id"]  = GetMainLang(ref sMainLang, ref sMainCode3);
                    Session["lang_code"] = sMainCode3;
                }

                if (Request.QueryString["collection_id"] != null &&
                    Request.QueryString["collection_id"].ToString() != "")
                {
                    Session["collection_id"] = int.Parse(Request.QueryString["collection_id"].ToString());
                }
                else
                    Session["collection_id"] = 0;

                Int32 nGroupID = LoginManager.GetLoginGroupID();
                m_sLangMenu = GetLangMenu(nGroupID);
        }
    }

    static protected Int32 GetMainLang(ref string sMainLang, ref string sCode)
    {
        Int32 nLangID = 0;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select l.CODE3,l.NAME,l.id from groups g with (nolock), lu_languages l with (nolock) where l.id=g.language_id and  ";
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

    protected string GetLangMenu(Int32 nGroupID)
    {
        try
        {
            string sTemp = "";
            string sMainLang = "";
            string sCode3 = "";
            Int32 nMainLangID = GetMainLang(ref sMainLang, ref sCode3);

            string sOnOff = "on";
            if (nMainLangID != int.Parse(Session["lang_id"].ToString()))
                sOnOff = "off";
            sTemp += "<li><a class=\"" + sOnOff + "\" href=\"";
            if (nMainLangID != int.Parse(Session["lang_id"].ToString()))
                sTemp += "adm_collections_new.aspx?collection_id=" + Session["collection_id"].ToString() + "&lang_id=" + nMainLangID.ToString();
            else
                sTemp += "javascript:void(0);";
            sTemp += "\"><span>";
            sTemp += sMainLang;
            sTemp += "</span></a></li>";

            ODBCWrapper.DataSetSelectQuery selectQuery1 = new ODBCWrapper.DataSetSelectQuery();
            selectQuery1 += "select l.name,l.id from group_extra_languages gel with (nolock) ,lu_languages l with (nolock) " +
                            "where gel.language_id=l.id and l.status=1 and gel.status=1 and  ";
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
                        sTemp += "adm_collections_new.aspx?collection_id=" + Session["collection_id"].ToString() + "&lang_id=" + nLangID.ToString();
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

    public void GetHeader()
    {
        Response.Write(PageUtils.GetPreHeader() + ": Collection management");
    }

    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
    }

    protected void GetSubMenu()
    {
        Response.Write(m_sSubMenu);
    }

    protected void GetLangMenu()
    {
        Response.Write(m_sLangMenu);
    }

    public string GetPageContent(string sOrderBy, string sPageNum)
    {
        if (Session["error_msg"] != null && Session["error_msg"].ToString() != "")
        {
            Session["error_msg"] = "";
            return Session["last_page_html"].ToString();
        }
        object t = null; ;
        var f = Session["collection_id"];
        if (Session["collection_id"] != null && Session["collection_id"].ToString() != "" && int.Parse(Session["collection_id"].ToString()) != 0)
            t = Session["collection_id"];

        string sBack = "adm_collections.aspx?search_save=1";
        DBRecordWebEditor theRecord = new DBRecordWebEditor("collections", "adm_table_pager", sBack, "", "ID", t, sBack, "");
        theRecord.SetConnectionKey("pricing_connection");

        DataRecordShortTextField dr_domain = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_domain.Initialize("External ID", "adm_table_header_nbg", "FormInput", "CODE", true);
        theRecord.AddRecord(dr_domain);

        DataRecordShortTextField dr_name = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_name.Initialize("Collection Name", "adm_table_header_nbg", "FormInput", "NAME", true);
        theRecord.AddRecord(dr_name);

        DataRecordDropDownField dr_col_price_codes = new DataRecordDropDownField("discount_codes", "code", "id", "", null, 60, false);
        dr_col_price_codes.SetFieldType("string");
        System.Data.DataTable priceCodesDT = GetBaseDT();
        string sWSUserName = "";
        string sWSPass = "";

        string sIP = "1.1.1.1";
        TVinciShared.WS_Utils.GetWSUNPass(LoginManager.GetLoginGroupID(), "GetPriceCodeList", "pricing", sIP, ref sWSUserName, ref sWSPass);
        TvinciPricing.mdoule m = new TvinciPricing.mdoule();
        string sWSURL = GetWSURL();
        if (sWSURL != "")
            m.Url = sWSURL;

        TvinciPricing.PriceCode[] oModules = m.GetPriceCodeList(sWSUserName, sWSPass, string.Empty, string.Empty, string.Empty);
        if (oModules != null)
        {
            for (int i = 0; i < oModules.Length; i++)
            {
                System.Data.DataRow tmpRow = null;
                tmpRow = priceCodesDT.NewRow();
                tmpRow["ID"]  = oModules[i].m_nObjectID;
                tmpRow["txt"] = oModules[i].m_sCode;

                priceCodesDT.Rows.InsertAt(tmpRow, 0);
                priceCodesDT.AcceptChanges();
            }
        }

        dr_col_price_codes.SetSelectsDT(priceCodesDT);
        dr_col_price_codes.Initialize("Collection Price Code", "adm_table_header_nbg", "FormInput", "PRICE_ID", true);
        dr_col_price_codes.SetDefault(0);
        theRecord.AddRecord(dr_col_price_codes);

        DataRecordDateTimeField dr_start_date = new DataRecordDateTimeField(true);
        dr_start_date.Initialize("Start Date", "adm_table_header_nbg", "FormInput", "START_DATE", false);
        dr_start_date.SetDefault(DateTime.Now);
        theRecord.AddRecord(dr_start_date);

        DataRecordDateTimeField dr_end_date = new DataRecordDateTimeField(true);
        dr_end_date.Initialize("End Date", "adm_table_header_nbg", "FormInput", "END_DATE", false);
        dr_end_date.SetDefault(new DateTime(2099, 1, 1));
        theRecord.AddRecord(dr_end_date);

        DataRecordDropDownField dr_col_usage_module = new DataRecordDropDownField("discount_codes", "code", "id", "", null, 60, false);
        dr_col_usage_module.SetFieldType("string");
        System.Data.DataTable usageModuleCodesDT = GetBaseDT();
        sWSUserName = "";
        sWSPass = "";

        TVinciShared.WS_Utils.GetWSUNPass(LoginManager.GetLoginGroupID(), "GetUsageModuleList", "pricing", sIP, ref sWSUserName, ref sWSPass);
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
        dr_col_usage_module.SetSelectsDT(usageModuleCodesDT);
        dr_col_usage_module.Initialize("Collection Usage Module(period and counts)", "adm_table_header_nbg", "FormInput", "USAGE_MODULE_ID", true);
        dr_col_usage_module.SetDefault(0);
        theRecord.AddRecord(dr_col_usage_module);

        DataRecordDropDownField dr_discExternal = new DataRecordDropDownField("discount_codes", "code", "id", "", null, 60, true);
        dr_discExternal.SetFieldType("string");
        dr_discExternal.SetNoSelectStr("---");

        System.Data.DataTable discCodesExternalDT = GetBaseDT();
        sWSUserName = "";
        sWSPass = "";

        TVinciShared.WS_Utils.GetWSUNPass(LoginManager.GetLoginGroupID(), "GetDiscountsModuleListForAdmin", "pricing", sIP, ref sWSUserName, ref sWSPass);
        TvinciPricing.DiscountModule[] oDiscCodesExternal = m.GetDiscountsModuleListForAdmin(sWSUserName, sWSPass);
        if (oDiscCodesExternal != null)
        {
            for (int i = 0; i < oDiscCodesExternal.Length; i++)
            {
                System.Data.DataRow tmpRow = null;
                tmpRow = discCodesExternalDT.NewRow();
                tmpRow["ID"] = oDiscCodesExternal[i].m_nObjectID;
                tmpRow["txt"] = oDiscCodesExternal[i].m_sCode;

                discCodesExternalDT.Rows.InsertAt(tmpRow, 0);
                discCodesExternalDT.AcceptChanges();
            }
        }
        dr_discExternal.SetSelectsDT(discCodesExternalDT);
        dr_discExternal.Initialize("Discounts (Internal Item)", "adm_table_header_nbg", "FormInput", "DISCOUNT_ID", false);
        dr_discExternal.SetDefault(0);
        theRecord.AddRecord(dr_discExternal);





        DataRecordDropDownField dr_coupons_group = new DataRecordDropDownField("discount_codes", "code", "id", "", null, 60, true);
        dr_coupons_group.SetFieldType("string");
        dr_coupons_group.SetNoSelectStr("---");
        System.Data.DataTable CouponsGroupDT = GetBaseDT();
        sWSUserName = "";
        sWSPass = "";

        TVinciShared.WS_Utils.GetWSUNPass(LoginManager.GetLoginGroupID(), "GetCouponGroupListForAdmin", "pricing", sIP, ref sWSUserName, ref sWSPass);
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
        dr_coupons_group.SetSelectsDT(CouponsGroupDT);
        dr_coupons_group.Initialize("Coupon Group", "adm_table_header_nbg", "FormInput", "COUPON_GROUP_CODE", false);
        dr_coupons_group.SetDefault(0);
        theRecord.AddRecord(dr_coupons_group);


        DataRecordLongTextField dr_Title = new DataRecordLongTextField("ltr", true, 60, 4);
        dr_Title.Initialize("Title", "adm_table_header_nbg", "FormInput", "", false);
        if (Session["collection_id"] != null && Session["collection_id"].ToString() != "0")
        {
            dr_Title.SetValue(GetCurrentValue("description", "collection_names", int.Parse(Session["collection_id"].ToString()), Session["lang_code"].ToString(), "pricing_connection"));
        }
        else
            dr_Title.SetValue("");
        theRecord.AddRecord(dr_Title);

        DataRecordLongTextField dr_Description = new DataRecordLongTextField("ltr", true, 60, 4);
        dr_Description.Initialize("Description", "adm_table_header_nbg", "FormInput", "", false);
        if (Session["collection_id"] != null && Session["collection_id"].ToString() != "0")
        {
            dr_Description.SetValue(GetCurrentValue("description", "collection_descriptions", int.Parse(Session["collection_id"].ToString()), Session["lang_code"].ToString(), "pricing_connection"));
        }
        else
            dr_Description.SetValue("");
        theRecord.AddRecord(dr_Description);

        object parent_group_id = LoginManager.GetLoginGroupID();
        if (parent_group_id == null)
        {
            parent_group_id = 156;
        }

        DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
        dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "group_id", false);
        dr_groups.SetValue(parent_group_id.ToString());
        theRecord.AddRecord(dr_groups);

        string sTable = theRecord.GetTableHTML("adm_collections_new.aspx?submited=1");
        writeCollectionNameToSession(sTable);
        return sTable;
    }

    protected string GetCurrentValue(string sField, string sTable, Int32 nCollection_ID, string sLangCode, string sConnKey)
    {
        string sRet = "";
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery.SetConnectionKey(sConnKey);
        selectQuery += "select " + sField + " from " + sTable + " with (nolock) where ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("collection_id", "=", nCollection_ID);
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

    private void writeCollectionNameToSession(string sTable)
    {
        Session[OLD_COL_NAME_SESSION_KEY] = null;
        if (Session["collection_id"] != null && Session["collection_id"].ToString().Length > 0)
        {
            int nCollectionID = int.Parse(Session["collection_id"].ToString());
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetConnectionKey("pricing_connection");
            selectQuery += "select name from collections with (nolock) where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nCollectionID);
            if (selectQuery.Execute("query", true) != null)
            {
                int count = selectQuery.Table("query").DefaultView.Count;
                if (count > 0)
                    Session[OLD_COL_NAME_SESSION_KEY] = selectQuery.Table("query").DefaultView[0].Row["NAME"].ToString();
            }
            selectQuery.Finish();
            selectQuery = null;
        }
        if (Session[OLD_COL_NAME_SESSION_KEY] == null)
            Session[OLD_COL_NAME_SESSION_KEY] = string.Empty;

    }

    static protected string GetWSURL()
    {
        return TVinciShared.WS_Utils.GetTcmConfigValue("pricing_ws");
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

    private void changeItemStatusCouponGroup(string sID)
    {
        if (Session["col_coupons_group"] != null && Session["col_coupons_group"] is List<CouponGroup>)
        {
            List<CouponGroup> cgObjList = Session["col_coupons_group"] as List<CouponGroup>;
            CouponGroup data = null;
            bool addToList = false;

            for (int i = 0; i < cgObjList.Count; i++)
            {
                CouponGroup obj = cgObjList[i];
                if (obj.id.Equals(sID))
                {
                    addToList = true;
                    data = cgObjList[i];
                    cgObjList.Remove(cgObjList[i]);
                    break;
                }
            }

            if (addToList && data != null)
            {
                data.isBelongToCollection = !data.isBelongToCollection;
                cgObjList.Add(data);
            }

            Session["col_coupons_group"] = cgObjList;
        }
    }

    public string changeItemStatus(string sID, string dualListName)
    {
        changeItemStatusCouponGroup(sID);
        return "";
    }

    public string changeItemDates(string sID, string sStartDate, string sEndDate)
    {
        if (Session["col_coupons_group"] != null && Session["col_coupons_group"] is List<CouponGroup>)
        {
            List<CouponGroup> cgObjList = Session["col_coupons_group"] as List<CouponGroup>;
            CouponGroup obj = cgObjList.Where(x => x.id == sID).Select(x => x).FirstOrDefault();
            if (obj != null)
            {
                cgObjList.Remove(obj);

                obj.startDate = sStartDate;
                obj.endDate = sEndDate;

                cgObjList.Add(obj);

            }
            Session["col_coupons_group"] = cgObjList;
        }

        return "";
    }

    public string initDualObj()
    {
        Dictionary<string, object> dualLists = new Dictionary<string, object>();
        Dictionary<string, object> couponGroups = new Dictionary<string, object>();

        couponGroups.Add("name", "DualListCouponGroup");
        couponGroups.Add("FirstListTitle", "Coupon Groups");
        couponGroups.Add("SecondListTitle", "Available Coupon Groups");
        couponGroups.Add("pageName", "adm_collections_new.aspx");
        couponGroups.Add("withCalendar", true);
        object[] couponGroupsData = null;
        initCouponsGroup(ref couponGroupsData);
        couponGroups.Add("Data", couponGroupsData);

        dualLists.Add("0", couponGroups);
        dualLists.Add("size", dualLists.Count);
        dualLists.Add("multiple", true);

        return dualLists.ToJSON();
    }

    private void initCouponsGroup(ref object[] couponGroupsData)
    {
        List<object> couponsGroup = new List<object>();
        List<CouponGroup> cgList = new List<CouponGroup>();
        Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();
        int collectionId = 0;
        if (Session["collection_id"] != null && int.TryParse(Session["collection_id"].ToString(), out collectionId))
        {
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetConnectionKey("pricing_connection");
            selectQuery += " select pcg.COUPON_GROUP_ID, cg.CODE, cg.id, pcg.START_DATE, pcg.END_DATE , cg.group_id ";
            selectQuery += " from coupons_groups cg (nolock)  left join products_coupons_groups pcg(nolock)  on	pcg.coupon_group_id = cg.id  and pcg.product_type = 2 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("pcg.product_id", "=", collectionId);
            selectQuery += " and pcg.is_active = 1 and pcg.status = 1  where  ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("cg.group_id", "=", nLogedInGroupID);
            DataTable dt = selectQuery.Execute("query", true);

            if (dt != null && dt.Rows != null)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    int couponGroupId = ODBCWrapper.Utils.GetIntSafeVal(dr, "COUPON_GROUP_ID");
                    int id = ODBCWrapper.Utils.GetIntSafeVal(dr, "ID");
                    string groupID = ODBCWrapper.Utils.GetSafeStr(dr, "group_ID");
                    string code = ODBCWrapper.Utils.GetSafeStr(dr, "code");
                    string description = "";
                    DateTime? date = ODBCWrapper.Utils.GetNullableDateSafeVal(dr, "START_DATE");
                    string startDate = date.HasValue ? date.Value.ToString("dd/MM/yyyy") : string.Empty;
                    date = ODBCWrapper.Utils.GetNullableDateSafeVal(dr, "END_DATE");
                    string endDate = date.HasValue ? date.Value.ToString("dd/MM/yyyy") : string.Empty;
                    var data = new
                    {
                        ID = id.ToString(),
                        Title = code,
                        Description = description,
                        InList = couponGroupId == 0 ? false : true,
                        StartDate = startDate,
                        EndDate = endDate
                    };

                    CouponGroup cg = new CouponGroup(id.ToString(), code, description, couponGroupId == 0 ? false : true, startDate, endDate);
                    cgList.Add(cg);
                    couponsGroup.Add(data);
                }
            }
            selectQuery.Finish();
            selectQuery = null;
        }

        couponGroupsData = new object[couponsGroup.Count];
        couponGroupsData = couponsGroup.ToArray();
        Session["col_coupons_group"] = cgList;
    }

    private void UpdateCouponsGroup(int collectionId, int groupID)
    {
        if (Session["col_coupons_group"] != null && Session["col_coupons_group"] is List<CouponGroup>)
        {
            List<int> tempIDs = new List<int>();
            List<CouponGroup> newCg = Session["col_coupons_group"] as List<CouponGroup>;
            List<int> oldCg = BuildCollectionCouponGroup(collectionId, groupID, true);

            foreach (CouponGroup newObj in newCg)
            {
                int newID = int.Parse(newObj.id);
                tempIDs.Add(newID);
                if (oldCg.Contains(newID))
                {
                    if (newObj.isBelongToCollection)
                    {
                        UpdateCouponsGroupDB(newID, collectionId, newObj.startDate, newObj.endDate);
                    }
                    else
                    {
                        RemoveCouponsGroupDB(newID, collectionId);
                    }
                }
                else
                {
                    if (newObj.isBelongToCollection)
                    {
                        InsertCouponsGroupDB(newID, collectionId, groupID, newObj.startDate, newObj.endDate);
                    }
                }

            }
            foreach (int oldID in oldCg)
            {
                if (!tempIDs.Contains(oldID))
                {
                    RemoveCouponsGroupDB(oldID, collectionId);
                }
            }
        }
    }

    private void RemoveCouponsGroupDB(int oldID, int collectionId)
    {
        ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("products_coupons_groups");
        updateQuery.SetConnectionKey("pricing_connection");
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("is_active", "=", 0);
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("status", "=", 2);
        updateQuery += " where ";
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("product_id", "=", collectionId);
        updateQuery += " and product_type = 2 and ";
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("COUPON_GROUP_ID", "=", oldID);
        updateQuery.Execute();
        updateQuery.Finish();
        updateQuery = null;
    }

    private void InsertCouponsGroupDB(int newID, int collectionId, int groupID, string startDate, string endDate)
    {
        ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("products_coupons_groups");
        insertQuery.SetConnectionKey("pricing_connection");
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("product_id", collectionId);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("COUPON_GROUP_ID", "=", newID);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", groupID);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("product_type", "=", 2);

        DateTime? dStartDate = string.IsNullOrEmpty(startDate) ? null : (DateTime?)(DateTime.ParseExact(startDate, "dd/MM/yyyy", CultureInfo.InvariantCulture));
        DateTime? dEndDate = string.IsNullOrEmpty(endDate) ? null : (DateTime?)(DateTime.ParseExact(endDate, "dd/MM/yyyy", CultureInfo.InvariantCulture));
        if (dStartDate.HasValue)
        {
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("START_DATE", "=", dStartDate);
        }
        else
        {
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("START_DATE", "=", DBNull.Value);
        }
        if (dEndDate.HasValue)
        {
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("END_DATE", "=", dEndDate);
        }
        else
        {
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("END_DATE", "=", DBNull.Value);
        }

        insertQuery.Execute();
        insertQuery.Finish();
        insertQuery = null;
    }

    private void UpdateCouponsGroupDB(int newID, int collectionId, string startDate, string endDate)
    {
        ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("products_coupons_groups");
        updateQuery.SetConnectionKey("pricing_connection");
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("is_active", "=", 1);
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("status", "=", 1);

        DateTime? dStartDate = string.IsNullOrEmpty(startDate) ? null : (DateTime?)(DateTime.ParseExact(startDate, "dd/MM/yyyy", CultureInfo.InvariantCulture));
        DateTime? dEndDate = string.IsNullOrEmpty(endDate) ? null : (DateTime?)(DateTime.ParseExact(endDate, "dd/MM/yyyy", CultureInfo.InvariantCulture));
        if (dStartDate.HasValue)
        {
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("START_DATE", "=", dStartDate);
        }
        else
        {
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("START_DATE", "=", DBNull.Value);
        }
        if (dEndDate.HasValue)
        {
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("END_DATE", "=", dEndDate);
        }
        else
        {
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("END_DATE", "=", DBNull.Value);
        }
        updateQuery += " where ";
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("product_id", "=", collectionId);
        updateQuery += " and product_type = 2 and ";
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("COUPON_GROUP_ID", "=", newID);
        updateQuery.Execute();
        updateQuery.Finish();
        updateQuery = null;
    }

    private List<int> BuildCollectionCouponGroup(int collectionId, int collectionGroupID, bool alsoUnActive)
    {
        List<int> retVal = new List<int>();
        //List<CGObj> cgList = new List<CGObj>();
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery.SetConnectionKey("pricing_connection");
        selectQuery += " select pcg.COUPON_GROUP_ID, cg.CODE, cg.id, pcg.START_DATE, pcg.END_DATE , cg.group_id ";
        selectQuery += " from coupons_groups cg (nolock)  inner join products_coupons_groups pcg(nolock)  on	pcg.coupon_group_id = cg.id  and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("pcg.product_id", "=", collectionId);
        if (!alsoUnActive)
        {
            selectQuery += " and pcg.is_active = 1 and pcg.status = 1 and pcg.product_type = 2 and ";
        }
        selectQuery += " where ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("cg.group_id", "=", collectionGroupID);
        DataTable dt = selectQuery.Execute("query", true);

        selectQuery.Finish();
        selectQuery = null;

        if (dt != null && dt.Rows != null)
        {
            foreach (DataRow dr in dt.Rows)
            {
                int couponGroupId = ODBCWrapper.Utils.GetIntSafeVal(dr, "COUPON_GROUP_ID"); // id from subscriptions_coupons_groups table                   
                int id = ODBCWrapper.Utils.GetIntSafeVal(dr, "ID"); // id from subscriptions_coupons_groups table                   
               
                if (couponGroupId != 0)
                {
                    retVal.Add(id);
                }
            }
        }

        return retVal;
    }
    public class CouponGroup
    {
        public string id;
        public string title;
        public string description;
        public bool isBelongToCollection;
        public string startDate;
        public string endDate;

        public CouponGroup(string id, string title, string desc, bool isBelong, string startDate, string endDate)
        {
            this.id = id;
            this.title = title;
            this.description = desc;
            this.isBelongToCollection = isBelong;
            this.startDate = startDate;
            this.endDate = endDate;
        }
    }
}
