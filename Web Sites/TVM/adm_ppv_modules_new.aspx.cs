using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TVinciShared;
using System.Configuration;

public partial class adm_ppv_module_new : System.Web.UI.Page
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
                Int32 nPPVModuleID = DBManipulator.DoTheWork("pricing_connection");
                UpdateUsageModules(nPPVModuleID, LoginManager.GetLoginGroupID());
                Session["ppv_module_id"] = nPPVModuleID.ToString();
                Int32 nLangID = int.Parse(Session["lang_id"].ToString());
                string sCode3 = Session["lang_code"].ToString();
                Int32 nPPVModuleDesc = 0;
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select id from ppv_descriptions where is_active=1 and status=1 and ";
                selectQuery.SetConnectionKey("pricing_connection");
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("language_code3", "=", sCode3);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("ppv_module_id", "=", nPPVModuleID);
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                        nPPVModuleDesc = int.Parse(selectQuery.Table("query").DefaultView[0].Row["id"].ToString());
                }
                selectQuery.Finish();
                selectQuery = null;

                Int32 nIter = 1;
                string sLang = "";
                
                if (int.Parse(Session["lang_id"].ToString()) != GetMainLang(ref sLang , ref sMainCode3))
                    nIter = 0;
                string sDesc = "";
                sDesc = Request.Form[nIter.ToString() + "_val"].ToString();
                nIter++;

                if (nPPVModuleDesc != 0) 
                {
                    ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("ppv_descriptions");
                    updateQuery.SetConnectionKey("pricing_connection");
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ppv_module_id", "=", nPPVModuleID);
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("language_code3", "=", sCode3);
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("DESCRIPTION", "=", sDesc);
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", LoginManager.GetLoginGroupID());
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATER_ID", "=", LoginManager.GetLoginID());
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATE_DATE", "=", DateTime.UtcNow);
                    updateQuery += " where ";
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nPPVModuleDesc);
                    updateQuery.Execute();
                    updateQuery.Finish();
                    updateQuery = null;
                }
                else
                {
                    ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("ppv_descriptions");
                    insertQuery.SetConnectionKey("pricing_connection");
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("ppv_module_id", "=", nPPVModuleID);
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
            if (Request.QueryString["ppv_module_id"] != null &&
                Request.QueryString["ppv_module_id"].ToString() != "")
            {
                Session["ppv_module_id"] = int.Parse(Request.QueryString["ppv_module_id"].ToString());
                Int32 nOwnerGroupID = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("ppv_modules", "group_id", int.Parse(Session["ppv_module_id"].ToString()), "pricing_connection").ToString());
                Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();
                if (nLogedInGroupID != nOwnerGroupID && PageUtils.IsTvinciUser() == false)
                {
                    LoginManager.LogoutFromSite("login.html");
                    return;
                }
            }
            else
                Session["ppv_module_id"] = 0;
        }
        Int32 nGroupID = LoginManager.GetLoginGroupID();
        m_sLangMenu = GetLangMenu(nGroupID);
    }

    protected void UpdateUsageModules(int ppvID, int groupID)
    {
        if (Session["ppv_usage_modules"] != null && Session["ppv_usage_modules"] is List<UMObj>)
        {
            List<int> tempIDs = new List<int>();
            List<UMObj> newUMs = Session["ppv_usage_modules"] as List<UMObj>;
            List<int> oldUMs = BuildPPVUMs(ppvID, groupID, true);
            foreach (UMObj newObj in newUMs)
            {
                int newID = int.Parse(newObj.m_id);
                tempIDs.Add(newID);
                if (oldUMs.Contains(newID))
                {
                    UpdateUsageModule(newID, ppvID, newObj.m_orderNum);
                }
                else
                {
                    InsertUsageModule(newID, ppvID, newObj.m_orderNum, groupID);
                }

            }
            foreach (int oldID in oldUMs)
            {
                if (!tempIDs.Contains(oldID))
                {
                    RemoveUsageModule(oldID, ppvID);
                }
            }
        }
    }

    protected void RemoveUsageModule(int umID, int ppvID)
    {
        ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("ppv_usage_modules");
        updateQuery.SetConnectionKey("pricing_connection");
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("is_active", "=", 0);
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("status", "=", 2);
        updateQuery += " where ";
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ppv_id", "=", ppvID);
        updateQuery += " and ";
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("usage_module_id", "=", umID);
        updateQuery.Execute();
        updateQuery.Finish();
        updateQuery = null;
    }

    protected void UpdateUsageModule(int umID, int ppvID, int order_num)
    {
        ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("ppv_usage_modules");
        updateQuery.SetConnectionKey("pricing_connection");
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("order_num", "=", order_num);
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("is_active", "=", 1);
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("status", "=", 1);
        updateQuery += " where ";
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ppv_id", "=", ppvID);
        updateQuery += " and ";
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("usage_module_id", "=", umID);
        updateQuery.Execute();
        updateQuery.Finish();
        updateQuery = null;
    }

    protected void InsertUsageModule(int umID, int subID, int order_num, int groupID)
    {
        ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("ppv_usage_modules");
        insertQuery.SetConnectionKey("pricing_connection");
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("ppv_id", subID);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("usage_module_id", "=", umID);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("order_num", "=", order_num);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", groupID);
        insertQuery.Execute();
        insertQuery.Finish();
        insertQuery = null;
    }

    protected string GetLangMenu(Int32 nGroupID)
    {
        try
        {
            string sTemp = "";
            Int32 nCount = 0;
            string sMainLang = "";
            string sCode3 = "";
            Int32 nMainLangID = GetMainLang(ref sMainLang , ref sCode3);

            string sOnOff = "on";
            if (nMainLangID != int.Parse(Session["lang_id"].ToString()))
                sOnOff = "off";
            sTemp += "<li><a class=\"" + sOnOff + "\" href=\"";
            if (nMainLangID != int.Parse(Session["lang_id"].ToString()))
                sTemp += "adm_ppv_modules_new.aspx?ppv_module_id=" + Session["ppv_module_id"].ToString() + "&lang_id=" + nMainLangID.ToString();
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
                        sTemp += "adm_ppv_modules_new.aspx?ppv_module_id=" + Session["ppv_module_id"].ToString() + "&lang_id=" + nLangID.ToString();
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
    
    static protected Int32 GetMainLang(ref string sMainLang , ref string sCode)
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
        string sRet = PageUtils.GetPreHeader() + ": PPV Module: ";
        if (Session["ppv_module_id"] != null && Session["ppv_module_id"].ToString() != "" && Session["ppv_module_id"].ToString() != "0")
        {
            string sMainLang = "";
            string sCode3 = "";
            Int32 nLangID = GetMainLang(ref sMainLang , ref sCode3);
            object sSubName = ODBCWrapper.Utils.GetTableSingleVal("ppv_descriptions", "description", "language_code3", "=", sMainLang, "pricing_connection");
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
        if (ConfigurationManager.AppSettings["pricing_ws"] != null &&
            ConfigurationManager.AppSettings["pricing_ws"].ToString() != "")
            return ConfigurationManager.AppSettings["pricing_ws"].ToString();
        return "";
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

    protected string GetCurrentValue(string sField, string sTable, Int32 nppv_module_idID, string sLangCode, string sConnKey)
    {
        string sRet = "";
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery.SetConnectionKey(sConnKey);
        selectQuery += "select " + sField + " from " + sTable + " where ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("ppv_module_id", "=", nppv_module_idID);
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

    protected List<int> BuildPPVUMs(int ppvID, int groupID, bool alsoUnActive)
    {
        List<int> retVal = new List<int>();
        List<UMObj> umList = new List<UMObj>();
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery.SetConnectionKey("pricing_connection");
        selectQuery += "select um.id, um.name, pum.order_num from usage_modules um, ppv_usage_modules pum where pum.usage_module_id = um.id and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("pum.ppv_id", "=", ppvID);
        selectQuery += " and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("um.group_id", "=", groupID);
        if (!alsoUnActive)
        {
            selectQuery += " and pum.is_active = 1 and pum.status = 1";
        }
        selectQuery += "order by pum.order_num";
        if (selectQuery.Execute("query", true) != null)
        {
            int count = selectQuery.Table("query").DefaultView.Count;
            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    string title = selectQuery.Table("query").DefaultView[i].Row["Name"].ToString();
                    string description = string.Empty;
                    string uID = selectQuery.Table("query").DefaultView[i].Row["id"].ToString();
                    int orderNum = int.Parse(selectQuery.Table("query").DefaultView[i].Row["order_num"].ToString());
                    UMObj umObj = new UMObj(uID, title, description, true, orderNum);
                    umList.Add(umObj);
                    retVal.Add(int.Parse(uID));
                }
            }
        }
        selectQuery.Finish();
        selectQuery = null;
        Session["ppv_usage_modules"] = umList;
        return retVal;
    }

    public string changeItemStatus(string sID, string sAction, string index)
    {
        string retVal = string.Empty;
        if (Session["ppv_usage_modules"] != null && Session["ppv_usage_modules"] is List<UMObj>)
        {
            List<UMObj> umObjList = Session["ppv_usage_modules"] as List<UMObj>;
            switch (sAction.ToLower())
            {
                case "remove":
                    {
                        for (int i = 0; i < umObjList.Count; i++)
                        {
                            UMObj obj = umObjList[i];
                            if (obj.m_id.Equals(sID))
                            {
                                umObjList.Remove(obj);
                                break;
                            }
                        }
                        Session["ppv_usage_modules"] = umObjList;
                        break;
                    }
                case "add":
                    {
                        UMObj obj = new UMObj(sID, string.Empty, string.Empty, true, int.Parse(index));
                        int newOrder = int.Parse(index);

                        foreach (UMObj umObj in umObjList)
                        {
                            int oldOrder = umObj.m_orderNum;

                            if (oldOrder >= newOrder)
                            {
                                umObj.m_orderNum++;
                            }

                        }
                        umObjList.Insert(int.Parse(index), obj);
                        umObjList.Sort();
                        Session["ppv_usage_modules"] = umObjList;
                        break;
                    }
            }
        }
        return retVal;
    }

    public string initDualObj()
    {
        Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();

        string sRet = "";
        sRet += "Uage Modules included in PPV Module";
        sRet += "~~|~~";
        sRet += "Available Usage Modules";
        sRet += "~~|~~";
        sRet += "<root>";
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery.SetConnectionKey("pricing_connection");
        selectQuery += "select * from usage_modules where is_active=1 and status=1 and ";
        Int32 nCommerceGroupID = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("groups", "COMMERCE_GROUP_ID", LoginManager.GetLoginGroupID()).ToString());
        if (nCommerceGroupID == 0)
            nCommerceGroupID = nLogedInGroupID;
        selectQuery += "group_id " + PageUtils.GetFullChildGroupsStr(nCommerceGroupID, "");
        selectQuery += " and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("type", "=", 1);
        List<int> ppvUMsIDs = null;
        if (Session["ppv_module_id"] != null && !string.IsNullOrEmpty(Session["ppv_module_id"].ToString()))
        {
            ppvUMsIDs = BuildPPVUMs(int.Parse(Session["ppv_module_id"].ToString()), nCommerceGroupID, false);
        }
        //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", LoginManager.GetLoginGroupID());
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            for (int i = 0; i < nCount; i++)
            {
                string sID = selectQuery.Table("query").DefaultView[i].Row["ID"].ToString();
                string sGroupID = selectQuery.Table("query").DefaultView[i].Row["group_ID"].ToString();
                string sTitle = "";
                if (selectQuery.Table("query").DefaultView[i].Row["NAME"] != null &&
                    selectQuery.Table("query").DefaultView[i].Row["NAME"] != DBNull.Value)
                    sTitle = selectQuery.Table("query").DefaultView[i].Row["NAME"].ToString();
                string sDescription = "";
                /*
                if (selectQuery.Table("query").DefaultView[i].Row["DESCRIPTION"] != null &&
                    selectQuery.Table("query").DefaultView[i].Row["DESCRIPTION"] != DBNull.Value)
                    sDescription = selectQuery.Table("query").DefaultView[i].Row["DESCRIPTION"].ToString();
                 */
                if (ppvUMsIDs == null || (ppvUMsIDs != null && !ppvUMsIDs.Contains(int.Parse(sID))))
                {
                    sRet += "<item id=\"" + sID + "\"  title=\"" + TVinciShared.ProtocolsFuncs.XMLEncode(sTitle, true) + "\" description=\"" + TVinciShared.ProtocolsFuncs.XMLEncode(sDescription, true) + "\" inList=\"false\" />";
                }
            }
            if (Session["ppv_usage_modules"] != null && Session["ppv_usage_modules"] != null)
            {
                int subID = int.Parse(Session["ppv_module_id"].ToString());
                List<UMObj> umObjList = Session["ppv_usage_modules"] as List<UMObj>;
                foreach (UMObj obj in umObjList)
                {
                    sRet += "<item id=\"" + obj.m_id + "\"  title=\"" + TVinciShared.ProtocolsFuncs.XMLEncode(obj.m_title, true) + "\" description=\"" + TVinciShared.ProtocolsFuncs.XMLEncode(obj.m_description, true) + "\" inList=\"true\" />";
                }
            }
        }
        selectQuery.Finish();
        selectQuery = null;


        sRet += "</root>";
        return sRet;
    }

    public string GetPageContent(string sOrderBy, string sPageNum)
    {
        if (Session["error_msg"] != null && Session["error_msg"].ToString() != "")
        {
            Session["error_msg"] = "";
            return Session["last_page_html"].ToString();
        }
        object t = null; ;
        if (Session["ppv_module_id"] != null && Session["ppv_module_id"].ToString() != "" && int.Parse(Session["ppv_module_id"].ToString()) != 0)
            t = Session["ppv_module_id"];
        string sBack = "adm_ppv_modules.aspx?search_save=1";
        DBRecordWebEditor theRecord = new DBRecordWebEditor("ppv_modules", "adm_table_pager", sBack, "", "ID", t, string.Empty, "");
        theRecord.SetConnectionKey("pricing_connection");
        string sMainLang = "";
        string sMainCode = "";
        if (int.Parse(Session["lang_id"].ToString()) == GetMainLang(ref sMainLang, ref sMainCode))
        {
            //DataRecordDropDownField dr_sub_price_codes = new DataRecordDropDownField("discount_codes", "code", "id", "", null, 60, false);
            //dr_sub_price_codes.SetFieldType("string");
            //System.Data.DataTable priceCodesDT = GetBaseDT();
            //string sWSUserName = "";
            //string sWSPass = "";

            //string sIP = "1.1.1.1";
            //TVinciShared.WS_Utils.GetWSUNPass(LoginManager.GetLoginGroupID(), "GetPriceCodeList", "pricing", sIP, ref sWSUserName, ref sWSPass);
            //TvinciPricing.mdoule m = new TvinciPricing.mdoule();
            //string sWSURL = GetWSURL();
            //if (sWSURL != "")
            //    m.Url = sWSURL;

            //TvinciPricing.PriceCode[] oModules = m.GetPriceCodeList(sWSUserName, sWSPass);
            //if (oModules != null)
            //{
            //    for (int i = 0; i < oModules.Length; i++)
            //    {
            //        System.Data.DataRow tmpRow = null;
            //        tmpRow = priceCodesDT.NewRow();
            //        tmpRow["ID"] = oModules[i].m_nObjectID;
            //        tmpRow["txt"] = oModules[i].m_sCode;
            //        /*
            //        TvinciPricing.LanguageContainer[] lang = oModules[i].m_sDescription;
            //        if (lang != null)
            //        {
            //            string sMainLang = GetMainLang();
            //            for (int j = 0; j < lang.Length; j++)
            //            {
            //                if (lang[j].m_sLanguageCode3 == sMainLang)
            //                    tmpRow["txt"] += "(" + lang[j].m_sValue + ")";
            //            }
            //        }
            //         */
            //        priceCodesDT.Rows.InsertAt(tmpRow, 0);
            //        priceCodesDT.AcceptChanges();
            //    }
            //}

            DataRecordShortTextField dr_domain = new DataRecordShortTextField("ltr", true, 60, 128);
            dr_domain.Initialize("Code", "adm_table_header_nbg", "FormInput", "Name", true);
            theRecord.AddRecord(dr_domain);

            //    dr_sub_price_codes.SetSelectsDT(priceCodesDT);
            //    dr_sub_price_codes.Initialize("Price Code", "adm_table_header_nbg", "FormInput", "PRICE_CODE", true);
            //    dr_sub_price_codes.SetDefault(0);
            //    theRecord.AddRecord(dr_sub_price_codes);


           

            //    DataRecordDropDownField dr_disc = new DataRecordDropDownField("discount_codes", "code", "id", "", null, 60, true);
            //    dr_disc.SetFieldType("string");
            //    System.Data.DataTable discCodesDT = GetBaseDT();
            //    sWSUserName = "";
            //    sWSPass = "";

            //    TVinciShared.WS_Utils.GetWSUNPass(LoginManager.GetLoginGroupID(), "GetDiscountsModuleListForAdmin", "pricing", sIP, ref sWSUserName, ref sWSPass);
            //    TvinciPricing.DiscountModule[] oDiscCodes = m.GetDiscountsModuleListForAdmin(sWSUserName, sWSPass);
            //    if (oDiscCodes != null)
            //    {
            //        for (int i = 0; i < oDiscCodes.Length; i++)
            //        {
            //            System.Data.DataRow tmpRow = null;
            //            tmpRow = discCodesDT.NewRow();
            //            tmpRow["ID"] = oDiscCodes[i].m_nObjectID;
            //            tmpRow["txt"] = oDiscCodes[i].m_sCode;
            //            discCodesDT.Rows.InsertAt(tmpRow, 0);
            //            discCodesDT.AcceptChanges();
            //        }
            //    }
            //    dr_disc.SetSelectsDT(discCodesDT);
            //    dr_disc.Initialize("Discounts", "adm_table_header_nbg", "FormInput", "DISCOUNT_MODULE_CODE", false);
            //    dr_disc.SetNoSelectStr("---");
            //    dr_disc.SetDefault(0);
            //    theRecord.AddRecord(dr_disc);

            //    DataRecordDropDownField dr_coupons_group = new DataRecordDropDownField("discount_codes", "code", "id", "", null, 60, true);
            //    dr_coupons_group.SetFieldType("string");
            //    System.Data.DataTable CouponsGroupDT = GetBaseDT();
            //    sWSUserName = "";
            //    sWSPass = "";

            //    TVinciShared.WS_Utils.GetWSUNPass(LoginManager.GetLoginGroupID(), "GetCouponGroupListForAdmin", "pricing", sIP, ref sWSUserName, ref sWSPass);
            //    TvinciPricing.CouponsGroup[] oCouponsGroup = m.GetCouponGroupListForAdmin(sWSUserName, sWSPass);
            //    if (oCouponsGroup != null)
            //    {
            //        for (int i = 0; i < oCouponsGroup.Length; i++)
            //        {
            //            System.Data.DataRow tmpRow = null;
            //            tmpRow = CouponsGroupDT.NewRow();
            //            tmpRow["ID"] = int.Parse(oCouponsGroup[i].m_sGroupCode);
            //            tmpRow["txt"] = oCouponsGroup[i].m_sGroupName;
            //            TvinciPricing.LanguageContainer[] lang = oCouponsGroup[i].m_sDescription;
            //            if (lang != null)
            //            {
            //                string sCode3 = "";
            //                Int32 nLangID = GetMainLang(ref sMainLang, ref sCode3);
            //                for (int j = 0; j < lang.Length; j++)
            //                {
            //                    if (lang[j].m_sLanguageCode3 == sMainLang)
            //                        tmpRow["txt"] += "(" + lang[j].m_sValue + ")";
            //                }
            //            }
            //            CouponsGroupDT.Rows.InsertAt(tmpRow, 0);
            //            CouponsGroupDT.AcceptChanges();
            //        }
            //    }
            //    dr_coupons_group.SetSelectsDT(CouponsGroupDT);
            //    dr_coupons_group.Initialize("Coupon Group", "adm_table_header_nbg", "FormInput", "COUPON_GROUP_CODE", false);
            //    dr_coupons_group.SetNoSelectStr("---");
            //    //dr_coupons_group.SetDefault(0);
            //    theRecord.AddRecord(dr_coupons_group);

            //    DataRecordBoolField dr_is_recurring = new DataRecordBoolField(true);
            //    dr_is_recurring.Initialize("Subscription Only", "adm_table_header_nbg", "FormInput", "SUBSCRIPTION_ONLY", false);
            //    theRecord.AddRecord(dr_is_recurring);
            //}
        }
        DataRecordLongTextField dr_Description = new DataRecordLongTextField("ltr", true, 60, 4);
        dr_Description.Initialize("Title", "adm_table_header_nbg", "FormInput", "", false);
        if (Session["ppv_module_id"] != null && Session["ppv_module_id"].ToString() != "0")
        {
            dr_Description.SetValue(GetCurrentValue("description", "ppv_descriptions", int.Parse(Session["ppv_module_id"].ToString()), Session["lang_code"].ToString(), "pricing_connection"));
        }
        else
            dr_Description.SetValue("");
        theRecord.AddRecord(dr_Description);

        DataRecordDropDownField dr_sub_usage_module = new DataRecordDropDownField("discount_codes", "code", "id", "", null, 60, false);
        dr_sub_usage_module.SetFieldType("string");
        System.Data.DataTable usageModuleCodesDT = GetBaseDT();

        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery.SetConnectionKey("pricing_connection");
        selectQuery += "select * from usage_modules where is_active=1 and status=1 and ";
        Int32 nCommerceGroupID = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("groups", "COMMERCE_GROUP_ID", LoginManager.GetLoginGroupID()).ToString());
        if (nCommerceGroupID == 0)
            nCommerceGroupID = LoginManager.GetLoginGroupID();
        selectQuery += "group_id " + PageUtils.GetFullChildGroupsStr(nCommerceGroupID, "");
        selectQuery += " and (";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("type", "=", 1);
        selectQuery += " or ";
        selectQuery += " type is null)";

        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            for (int i = 0; i < nCount; i++)
            {
                string sID = selectQuery.Table("query").DefaultView[i].Row["ID"].ToString();

                string sTitle = "";
                if (selectQuery.Table("query").DefaultView[i].Row["NAME"] != null &&
                    selectQuery.Table("query").DefaultView[i].Row["NAME"] != DBNull.Value)
                    sTitle = selectQuery.Table("query").DefaultView[i].Row["NAME"].ToString();
                System.Data.DataRow tmpRow = null;
                tmpRow = usageModuleCodesDT.NewRow();
                tmpRow["ID"] = sID;
                tmpRow["txt"] = sTitle;
                usageModuleCodesDT.Rows.InsertAt(tmpRow, 0);
                usageModuleCodesDT.AcceptChanges();
                /*
                if (selectQuery.Table("query").DefaultView[i].Row["DESCRIPTION"] != null &&
                    selectQuery.Table("query").DefaultView[i].Row["DESCRIPTION"] != DBNull.Value)
                    sDescription = selectQuery.Table("query").DefaultView[i].Row["DESCRIPTION"].ToString();
                 */

            }
        }
        //TVinciShared.WS_Utils.GetWSUNPass(LoginManager.GetLoginGroupID(), "GetUsageModuleList", "pricing", "1.1.1.1", ref sWSUserName, ref sWSPass);
        //TvinciPricing.UsageModule[] oUMCodes = m.GetUsageModuleList(sWSUserName, sWSPass);
        //if (oUMCodes != null)
        //{
        //    for (int i = 0; i < oUMCodes.Length; i++)
        //    {
        //        System.Data.DataRow tmpRow = null;
        //        tmpRow = usageModuleCodesDT.NewRow();
        //        tmpRow["ID"] = oUMCodes[i].m_nObjectID;
        //        tmpRow["txt"] = oUMCodes[i].m_sVirtualName;
        //        usageModuleCodesDT.Rows.InsertAt(tmpRow, 0);
        //        usageModuleCodesDT.AcceptChanges();
        //    }
        //}
        dr_sub_usage_module.SetSelectsDT(usageModuleCodesDT);
        dr_sub_usage_module.Initialize("Usage Module", "adm_table_header_nbg", "FormInput", "USAGE_MODULE_CODE", true);
        dr_sub_usage_module.SetDefault(0);
        theRecord.AddRecord(dr_sub_usage_module);

        DataRecordCheckBoxField dr_FirstDeviceLimitation = new DataRecordCheckBoxField(true);
        dr_FirstDeviceLimitation.Initialize("First Device Limitation", "adm_table_header_nbg", "FormInput", "FirstDeviceLimitation", false);
        theRecord.AddRecord(dr_FirstDeviceLimitation);


        DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
        dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", false);
        dr_groups.SetValue(LoginManager.GetLoginGroupID().ToString());
        theRecord.AddRecord(dr_groups);

        string sTable = theRecord.GetTableHTML("adm_ppv_modules_new.aspx?submited=1", false);

        return sTable;
    }
}
