using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TVinciShared;
using System.Configuration;
using TvinciImporter;
using KLogMonitor;
using System.Reflection;

public partial class adm_subscriptions_new : System.Web.UI.Page
{
    private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
    protected string m_sMenu;
    protected string m_sSubMenu;
    protected string m_sLangMenu;
    private const string OLD_SUB_NAME_SESSION_KEY = "OldSubName";

    static public bool IsTvinciImpl()
    {
        Int32 nImplID = 0;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery.SetConnectionKey("pricing_connection");
        selectQuery += "select * from groups_modules_implementations with (nolock) where is_active=1 and status=1 and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", LoginManager.GetLoginGroupID());
        selectQuery += "and";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MODULE_ID", "=", 6);
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
        {
            return;
        }

        if (LoginManager.CheckLogin() == false)
        {
            Response.Redirect("login.html");
        }

        int nMenuID = 0;
        string sMainLang = "";
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
                bool update = (Session["subscription_id"] != null);

                int nSuscriptionID = DBManipulator.DoTheWork("pricing_connection");

                Session["subscription_id"] = nSuscriptionID.ToString();
                Int32 nLangID = int.Parse(Session["lang_id"].ToString());
                string sCode3 = Session["lang_code"].ToString();
                Int32 nSubscriptionDesc = 0;
                Int32 nSubscriptionName = 0;

                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select id from subscription_descriptions with (nolock) where is_active=1 and status=1 and ";
                selectQuery.SetConnectionKey("pricing_connection");
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("language_code3", "=", sCode3);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("subscription_id", "=", nSuscriptionID);
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                        nSubscriptionDesc = int.Parse(selectQuery.Table("query").DefaultView[0].Row["id"].ToString());
                }
                selectQuery.Finish();
                selectQuery = null;

                ODBCWrapper.DataSetSelectQuery selectQuery2 = new ODBCWrapper.DataSetSelectQuery();
                selectQuery2 += "select id from subscription_names with (nolock) where is_active=1 and status=1 and ";
                selectQuery2.SetConnectionKey("pricing_connection");
                selectQuery2 += ODBCWrapper.Parameter.NEW_PARAM("language_code3", "=", sCode3);
                selectQuery2 += "and";
                selectQuery2 += ODBCWrapper.Parameter.NEW_PARAM("subscription_id", "=", nSuscriptionID);
                if (selectQuery2.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery2.Table("query").DefaultView.Count;
                    if (nCount > 0)
                        nSubscriptionName = int.Parse(selectQuery2.Table("query").DefaultView[0].Row["id"].ToString());
                }
                selectQuery2.Finish();
                selectQuery2 = null;

                Int32 nIter = 11;
                string sLang = "";

                if (int.Parse(Session["lang_id"].ToString()) != GetMainLang(ref sLang, ref sMainCode3))
                    nIter = 0;
                string sName = "";
                sName = Request.Form[nIter.ToString() + "_val"].ToString();
                nIter++;
                string sDesc = "";
                sDesc = Request.Form[nIter.ToString() + "_val"].ToString();
                nIter++;


                if (nSubscriptionDesc != 0)
                {
                    ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("subscription_descriptions");
                    updateQuery.SetConnectionKey("pricing_connection");
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("subscription_id", "=", nSuscriptionID);
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("language_code3", "=", sCode3);
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("DESCRIPTION", "=", sDesc);
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", LoginManager.GetLoginGroupID());
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATER_ID", "=", LoginManager.GetLoginID());
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATE_DATE", "=", DateTime.UtcNow);
                    updateQuery += " where ";
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nSubscriptionDesc);
                    updateQuery.Execute();
                    updateQuery.Finish();
                    updateQuery = null;
                }
                else
                {
                    ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("subscription_descriptions");
                    insertQuery.SetConnectionKey("pricing_connection");
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("subscription_id", "=", nSuscriptionID);
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

                //bool update = true;
                if (nSubscriptionName != 0)
                {
                    ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("subscription_names");
                    updateQuery.SetConnectionKey("pricing_connection");
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("subscription_id", "=", nSuscriptionID);
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("language_code3", "=", sCode3);
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("DESCRIPTION", "=", sName);
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", LoginManager.GetLoginGroupID());
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATER_ID", "=", LoginManager.GetLoginID());
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATE_DATE", "=", DateTime.UtcNow);
                    updateQuery += " where ";
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nSubscriptionName);
                    updateQuery.Execute();
                    updateQuery.Finish();
                    updateQuery = null;
                }
                else
                {
                    update = false;
                    ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("subscription_names");
                    insertQuery.SetConnectionKey("pricing_connection");
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("subscription_id", "=", nSuscriptionID);
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

                try
                {
                    Notifiers.BaseSubscriptionNotifier t = null;
                    Notifiers.Utils.GetBaseSubscriptionsNotifierImpl(ref t, LoginManager.GetLoginGroupID(), "pricing_connection");

                    //bool update = (nSuscriptionID != 0);

                    if (t != null)
                    {
                        string errorMessage = "";
                        t.NotifyChange(nSuscriptionID.ToString(), ref errorMessage, Convert.ToInt32(update));

                        if (!string.IsNullOrEmpty(errorMessage))
                        {
                            HttpContext.Current.Session["error_msg_sub"] = "Error in Subscription ID " + nSuscriptionID + ":\r\n" + errorMessage;
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.Error("exception - " + nSuscriptionID.ToString() + " : " + ex.Message, ex);
                }

                if (nSuscriptionID != 0)
                {
                    string sSubName = "";
                    ODBCWrapper.DataSetSelectQuery selectQuery1 = new ODBCWrapper.DataSetSelectQuery();
                    selectQuery1.SetConnectionKey("pricing_connection");
                    selectQuery1 += "select * from subscriptions with (nolock) where ";
                    selectQuery1 += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nSuscriptionID);
                    if (selectQuery1.Execute("query", true) != null)
                    {
                        Int32 nCount = selectQuery1.Table("query").DefaultView.Count;
                        if (nCount > 0)
                        {
                            sSubName = selectQuery1.Table("query").DefaultView[0].Row["NAME"].ToString();
                        }
                    }
                    selectQuery1.Finish();
                    selectQuery1 = null;
                    if (sSubName.Length > 0)
                    {
                        int idToUpdateInLucene = DBManipulator.BuildOrUpdateFictivicMedia("Package", sSubName, nSuscriptionID, LoginManager.GetLoginGroupID(), Session[OLD_SUB_NAME_SESSION_KEY] != null ? Session[OLD_SUB_NAME_SESSION_KEY].ToString() : string.Empty);
                        if (Session[OLD_SUB_NAME_SESSION_KEY] != null && Session[OLD_SUB_NAME_SESSION_KEY].ToString().Length > 0) // when updating media need to update in lucene as well. when creating the lucene update occurs on adm_media_new.aspx.cs
                            ImporterImpl.UpdateRecordInLucene(LoginManager.GetLoginGroupID(), idToUpdateInLucene);
                    }
                }

                return;
            }

            m_sMenu = TVinciShared.Menu.GetMainMenu(14, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, true);

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
            if (Request.QueryString["subscription_id"] != null &&
                Request.QueryString["subscription_id"].ToString() != "")
            {
                Session["subscription_id"] = int.Parse(Request.QueryString["subscription_id"].ToString());
                Int32 nOwnerGroupID = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("subscriptions", "group_id", int.Parse(Session["subscription_id"].ToString()), "pricing_connection").ToString());
                Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();
                if (nLogedInGroupID != nOwnerGroupID && PageUtils.IsTvinciUser() == false)
                {
                    LoginManager.LogoutFromSite("login.html");
                    return;
                }
            }
            else
                Session["subscription_id"] = 0;
            Int32 nGroupID = LoginManager.GetLoginGroupID();
            m_sLangMenu = GetLangMenu(nGroupID);
        }

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
                sTemp += "adm_subscriptions_new.aspx?subscription_id=" + Session["subscription_id"].ToString() + "&lang_id=" + nMainLangID.ToString();
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
                        sTemp += "adm_subscriptions_new.aspx?subscription_id=" + Session["subscription_id"].ToString() + "&lang_id=" + nLangID.ToString();
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

    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
    }

    protected void GetLangMenu()
    {
        Response.Write(m_sLangMenu);
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

    static protected string GetMainLang()
    {
        string sMainLang = "";
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select l.CODE3,l.id from groups g with (nolock), lu_languages l with (nolock)  where l.id=g.language_id and  ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("g.id", "=", LoginManager.GetLoginGroupID());
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
            {
                sMainLang = selectQuery.Table("query").DefaultView[0].Row["CODE3"].ToString();
            }
        }
        selectQuery.Finish();
        selectQuery = null;
        return sMainLang;
    }

    public void GetHeader()
    {
        string sRet = PageUtils.GetPreHeader() + ": Subscription: ";
        if (Session["subscription_id"] != null && Session["subscription_id"].ToString() != "" && Session["subscription_id"].ToString() != "0")
        {
            object sSubName = ODBCWrapper.Utils.GetTableSingleVal("subscription_names", "description", "language_code3", "=", GetMainLang(), "pricing_connection");
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

    static protected string GetWSURL()
    {
        return TVinciShared.WS_Utils.GetTcmConfigValue("pricing_ws");
    }

    protected string GetCurrentValue(string sField, string sTable, Int32 nsubscription_idID, string sLangCode, string sConnKey)
    {
        string sRet = "";
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery.SetConnectionKey(sConnKey);
        selectQuery += "select " + sField + " from " + sTable + " with (nolock) where ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("subscription_id", "=", nsubscription_idID);
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

    protected System.Data.DataTable GetBaseDT()
    {
        System.Data.DataTable dT = new System.Data.DataTable();
        Int32 n = 0;
        string s = "";
        dT.Columns.Add(PageUtils.GetColumn("ID", n));
        dT.Columns.Add(PageUtils.GetColumn("txt", s));
        return dT.Copy();
    }



    protected System.Data.DataTable GetLimitsDT()
    {
        System.Data.DataTable retVal = GetBaseDT();
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select id as ID, Name as txt from groups_device_limitation_modules with (nolock) where ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", PageUtils.GetUpperGroupID(LoginManager.GetLoginGroupID()));
        selectQuery += " and is_active = 1 and status = 1";
        if (selectQuery.Execute("query", true) != null)
        {
            return selectQuery.Table("query");
        }
        selectQuery.Finish();
        selectQuery = null;
        return null;
    }

    public string GetPageContent(string sOrderBy, string sPageNum)
    {
        if (Session["error_msg"] != null && Session["error_msg"].ToString() != "")
        {
            Session["error_msg"] = "";
            return Session["last_page_html"].ToString();
        }

        object t = null;
        if (Session["subscription_id"] != null && Session["subscription_id"].ToString() != "" && int.Parse(Session["subscription_id"].ToString()) != 0)
            t = Session["subscription_id"];

        string sBack = "adm_subscriptions.aspx?search_save=1";
        DBRecordWebEditor theRecord = new DBRecordWebEditor("subscriptions", "adm_table_pager", sBack, "", "ID", t, sBack, "");
        theRecord.SetConnectionKey("pricing_connection");

        string sMainLang = "";
        string sMainCode = "";
        if (int.Parse(Session["lang_id"].ToString()) == GetMainLang(ref sMainLang, ref sMainCode))
        {
            DataRecordShortTextField dr_domain = new DataRecordShortTextField("ltr", true, 60, 128);
            dr_domain.Initialize("Code", "adm_table_header_nbg", "FormInput", "Name", true);
            theRecord.AddRecord(dr_domain);

            DataRecordDropDownField dr_sub_price_codes = new DataRecordDropDownField("discount_codes", "code", "id", "", null, 60, false);
            dr_sub_price_codes.SetFieldType("string");
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

            dr_sub_price_codes.SetSelectsDT(priceCodesDT);
            dr_sub_price_codes.Initialize("Subscription Price Code", "adm_table_header_nbg", "FormInput", "SUB_PRICE_CODE", true);
            dr_sub_price_codes.SetDefault(0);
            theRecord.AddRecord(dr_sub_price_codes);

            DataRecordDateTimeField dr_start_date = new DataRecordDateTimeField(true);
            dr_start_date.Initialize("Start Date", "adm_table_header_nbg", "FormInput", "START_DATE", false);
            dr_start_date.SetDefault(DateTime.Now);
            theRecord.AddRecord(dr_start_date);

            DataRecordDateTimeField dr_end_date = new DataRecordDateTimeField(true);
            dr_end_date.Initialize("End Date", "adm_table_header_nbg", "FormInput", "END_DATE", false);
            dr_end_date.SetDefault(new DateTime(2099, 1, 1));
            theRecord.AddRecord(dr_end_date);

            DataRecordDropDownField dr_sub_usage_module = new DataRecordDropDownField("discount_codes", "code", "id", "", null, 60, false);
            dr_sub_usage_module.SetFieldType("string");
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
            dr_sub_usage_module.SetSelectsDT(usageModuleCodesDT);
            dr_sub_usage_module.Initialize("Subscription Usage Module(period and counts)", "adm_table_header_nbg", "FormInput", "SUB_USAGE_MODULE_CODE", true);
            dr_sub_usage_module.SetDefault(0);
            theRecord.AddRecord(dr_sub_usage_module);


            DataRecordDropDownField dr_usage_module = new DataRecordDropDownField("discount_codes", "code", "id", "", null, 60, false);
            dr_usage_module.SetFieldType("string");
            dr_usage_module.SetSelectsDT(usageModuleCodesDT);
            dr_usage_module.Initialize("Usage Module (Internal Item period and counts)", "adm_table_header_nbg", "FormInput", "USAGE_MODULE_CODE", true);
            dr_usage_module.SetDefault(0);
            theRecord.AddRecord(dr_usage_module);

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
                    discCodesExternalDT.Rows.InsertAt(tmpRow, 0);
                    discCodesExternalDT.AcceptChanges();
                }
            }
            dr_discExternal.SetSelectsDT(discCodesExternalDT);
            dr_discExternal.Initialize("Discount", "adm_table_header_nbg", "FormInput", "Ext_discount_module", false);
            dr_discExternal.SetDefault(0);
            theRecord.AddRecord(dr_discExternal);

            DataRecordDropDownField dr_disc = new DataRecordDropDownField("discount_codes", "code", "id", "", null, 60, true);
            dr_disc.SetFieldType("string");
            dr_disc.SetNoSelectStr("---");
            System.Data.DataTable discCodesDT = GetBaseDT();
            sWSUserName = "";
            sWSPass = "";

            TVinciShared.WS_Utils.GetWSUNPass(LoginManager.GetLoginGroupID(), "GetDiscountsModuleListForAdmin", "pricing", sIP, ref sWSUserName, ref sWSPass);
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
            dr_disc.SetSelectsDT(discCodesDT);
            dr_disc.Initialize("Discounts (Internal Item)", "adm_table_header_nbg", "FormInput", "DISCOUNT_MODULE_CODE", false);
            dr_disc.SetDefault(0);
            theRecord.AddRecord(dr_disc);

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
            dr_coupons_group.SetSelectsDT(CouponsGroupDT);
            dr_coupons_group.Initialize("Coupon Group", "adm_table_header_nbg", "FormInput", "COUPON_GROUP_CODE", false);
            dr_coupons_group.SetDefault(0);
            theRecord.AddRecord(dr_coupons_group);

            DataRecordBoolField dr_is_recurring = new DataRecordBoolField(true);
            dr_is_recurring.Initialize("Is subscription renewed auto", "adm_table_header_nbg", "FormInput", "IS_RECURRING", false);
            theRecord.AddRecord(dr_is_recurring);

            DataRecordShortIntField dr_num_of_periods = new DataRecordShortIntField(true, 12, 12);
            dr_num_of_periods.Initialize("Num of automatic renewals (type 0 if unlimited)", "adm_table_header_nbg", "FormInput", "NUM_OF_REC_PERIODS", true);
            dr_num_of_periods.SetDefault(1);
            theRecord.AddRecord(dr_num_of_periods);
        }

        DataRecordLongTextField dr_Title = new DataRecordLongTextField("ltr", true, 60, 4);
        dr_Title.Initialize("Title", "adm_table_header_nbg", "FormInput", "", false);
        if (Session["subscription_id"] != null && Session["subscription_id"].ToString() != "0")
        {
            dr_Title.SetValue(GetCurrentValue("description", "subscription_names", int.Parse(Session["subscription_id"].ToString()), Session["lang_code"].ToString(), "pricing_connection"));
        }
        else
            dr_Title.SetValue("");
        theRecord.AddRecord(dr_Title);

        DataRecordLongTextField dr_Description = new DataRecordLongTextField("ltr", true, 60, 4);
        dr_Description.Initialize("Description", "adm_table_header_nbg", "FormInput", "", false);
        if (Session["subscription_id"] != null && Session["subscription_id"].ToString() != "0")
        {
            dr_Description.SetValue(GetCurrentValue("description", "subscription_descriptions", int.Parse(Session["subscription_id"].ToString()), Session["lang_code"].ToString(), "pricing_connection"));
        }
        else
            dr_Description.SetValue("");
        theRecord.AddRecord(dr_Description);


        DataRecordShortTextField dr_coguid = new DataRecordShortTextField("ltr", true, 60, 50);
        dr_coguid.Initialize("External ID", "adm_table_header_nbg", "FormInput", "CoGuid", false);
        theRecord.AddRecord(dr_coguid);


        DataRecordDropDownField dr_device_limits = new DataRecordDropDownField("groups_device_limitation_modules", "Name", "id", "group_id", PageUtils.GetUpperGroupID(LoginManager.GetLoginGroupID()), 60, false);
        dr_device_limits.SetSelectsDT(GetLimitsDT());
        dr_device_limits.Initialize("Device Limit", "adm_table_header_nbg", "FormInput", "device_limit_id", false);

        theRecord.AddRecord(dr_device_limits);


        DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
        dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", false);
        dr_groups.SetValue(LoginManager.GetLoginGroupID().ToString());
        theRecord.AddRecord(dr_groups);

        DataRecordDropDownField dr_block_rules = new DataRecordDropDownField("subscriptions", "NAME", "id", "", null, 60, true);
        string sQuery = "select gbt.name as txt, gbt.id as id from TVinci..geo_block_types gbt with (nolock), lu_content_status lcs with (nolock) " +
            "where gbt.status=lcs.id and gbt.geo_rule_type=3 and gbt.status=1 and gbt.is_active=1 and gbt.group_id = " +
            LoginManager.GetLoginGroupID().ToString();
        dr_block_rules.SetSelectsQuery(sQuery);
        dr_block_rules.Initialize("Countries rule", "adm_table_header_nbg", "FormInput", "geo_commerce_block_id", false);
        //dr_block_rules.SetDefaultVal(sDefBR);
        theRecord.AddRecord(dr_block_rules);

        DataRecordDropDownField dr_adsPolicy = new DataRecordDropDownField("", "NAME", "id", "", null, 60, true);
        dr_adsPolicy.SetSelectsDT(GetAdsPolicyDT());
        dr_adsPolicy.Initialize("Ads Policy", "adm_table_header_nbg", "FormInput", "ADS_POLICY", false);
        theRecord.AddRecord(dr_adsPolicy);

        DataRecordShortTextField dr_adsParam = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_adsParam.Initialize("Ads Param", "adm_table_header_nbg", "FormInput", "ADS_PARAM", false);
        theRecord.AddRecord(dr_adsParam);

        string sTable = theRecord.GetTableHTML("adm_subscriptions_new.aspx?submited=1");
        writeSubscriptionNameToSession(sTable);
        return sTable;
    }

    private void writeSubscriptionNameToSession(string sTable)
    {
        Session[OLD_SUB_NAME_SESSION_KEY] = null;
        if (Session["subscription_id"] != null && Session["subscription_id"].ToString().Length > 0)
        {
            int nSubscriptionID = int.Parse(Session["subscription_id"].ToString());
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetConnectionKey("pricing_connection");
            selectQuery += "select name from subscriptions with (nolock) where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nSubscriptionID);
            if (selectQuery.Execute("query", true) != null)
            {
                int count = selectQuery.Table("query").DefaultView.Count;
                if (count > 0)
                    Session[OLD_SUB_NAME_SESSION_KEY] = selectQuery.Table("query").DefaultView[0].Row["NAME"].ToString();
            }
            selectQuery.Finish();
            selectQuery = null;
        }
        if (Session[OLD_SUB_NAME_SESSION_KEY] == null)
            Session[OLD_SUB_NAME_SESSION_KEY] = string.Empty;

    }

    private System.Data.DataTable GetAdsPolicyDT()
    {
        System.Data.DataTable dt = new System.Data.DataTable();
        dt.Columns.Add("id", typeof(int));
        dt.Columns.Add("txt", typeof(string));
        foreach (ApiObjects.DrmType r in Enum.GetValues(typeof(ApiObjects.AdsPolicy)))
        {
            dt.Rows.Add((int)r, r);
        }
        return dt;
    }

}
