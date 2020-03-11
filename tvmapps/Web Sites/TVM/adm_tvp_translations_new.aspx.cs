using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using TVinciShared;

public partial class adm_tvp_translations_new : System.Web.UI.Page
{
    protected string m_sMenu;
    protected string m_sSubMenu;
    protected string m_sLangMenu;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (AMS.Web.RemoteScripting.InvokeMethod(this))
        {
            Response.Expires = -1;
            return;
        }
        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        Int32 nMenuID = 0;
        if (!IsPostBack)
        {
            Int32 nGroupID = LoginManager.GetLoginGroupID();
            if (Request.QueryString["submited"] != null && Request.QueryString["submited"].ToString() == "1")
            {
                bool bNew = true;
                if (Session["translation_id"] != null && Session["translation_id"].ToString() != "0" && Session["translation_id"].ToString() != "")
                {
                    bNew = false;
                }
                Int32 nTranslationID = DBManipulator.DoTheWork("tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString());
                if (bNew == false)
                {
                    Session["translation_id"] = nTranslationID.ToString();
                    Int32 nLangID = int.Parse(Session["lang_id"].ToString());
                    Int32 nTranslationTextID = 0;
                    ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                    selectQuery += "select id from TranslationMetadata where is_active=1 and status=1 and ";
                    selectQuery.SetConnectionKey("tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString());
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("LANGUAGE_ID", "=", nLangID);
                    selectQuery += "and";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("TranslationID", "=", nTranslationID);
                    if (selectQuery.Execute("query", true) != null)
                    {
                        Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                        if (nCount > 0)
                            nTranslationTextID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["id"].ToString());
                    }
                    selectQuery.Finish();
                    selectQuery = null;

                    Int32 nIter = 2;
                    string sLang = "";
                    if (int.Parse(Session["lang_id"].ToString()) != GetMainLang(ref sLang) || PageUtils.IsTvinciUser() == false)
                        nIter = 0;
                    string sText = "";
                    sText = Request.Form[nIter.ToString() + "_val"].ToString();
                    if (!string.IsNullOrEmpty(sText) && !string.IsNullOrEmpty(sText.Trim()))
                    {
                        if (nTranslationTextID != 0)
                        {
                            ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("TranslationMetadata");
                            updateQuery.SetConnectionKey("tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString());
                            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("TranslationID", "=", nTranslationID);
                            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
                            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("LANGUAGE_ID", "=", nLangID);
                            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
                            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("Text", "=", sText);
                            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("OriginalText", "=", sText);
                            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("Culture", "=", ODBCWrapper.Utils.GetTableSingleVal("lu_languages", "CULTURE", nLangID, "tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString()));
                            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
                            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATER_ID", "=", LoginManager.GetLoginID());
                            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATE_DATE", "=", DateTime.UtcNow);
                            updateQuery += " where ";
                            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nTranslationTextID);
                            updateQuery.Execute();
                            updateQuery.Finish();
                            updateQuery = null;
                        }
                        else
                        {
                            ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("TranslationMetadata");
                            insertQuery.SetConnectionKey("tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString());
                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("TranslationID", "=", nTranslationID);
                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("LANGUAGE_ID", "=", nLangID);
                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("Text", "=", sText);
                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("OriginalText", "=", sText);
                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("Culture", "=", ODBCWrapper.Utils.GetTableSingleVal("lu_languages", "CULTURE", nLangID, "tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString()));
                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATER_ID", "=", LoginManager.GetLoginID());
                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATE_DATE", "=", DateTime.UtcNow);
                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CREATE_DATE", "=", DateTime.UtcNow);
                            insertQuery.Execute();
                            insertQuery.Finish();
                            insertQuery = null;
                        }
                    }
                }

                return;
            }

            m_sMenu = TVinciShared.Menu.GetMainMenu(14, true, ref nMenuID, "adm_tvp_translations.aspx");
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, true);
            string sMainLang = "";
            if (Request.QueryString["lang_id"] != null &&
               Request.QueryString["lang_id"].ToString() != "")
                Session["lang_id"] = Request.QueryString["lang_id"].ToString();
            else
                Session["lang_id"] = GetMainLang(ref sMainLang);

            if (Request.QueryString["translation_id"] != null &&
               Request.QueryString["translation_id"].ToString() != "")
            {
                Session["translation_id"] = int.Parse(Request.QueryString["translation_id"].ToString());
            }
            else
                Session["translation_id"] = 0;

            m_sLangMenu = GetLangMenu(nGroupID);
        }
        Response.Expires = -1;
    }

    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
    }

    public void GetHeader()
    {
        Int32 nGroupID = LoginManager.GetLoginGroupID();
        string sConn = "tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString();
        object oTitleName = ODBCWrapper.Utils.GetTableSingleVal("Translation", "TitleID", int.Parse(Session["translation_id"].ToString()), sConn);
        object oCategoryToken = ODBCWrapper.Utils.GetTableSingleVal("Translation", "CategoryToken", int.Parse(Session["translation_id"].ToString()), sConn);
        string sTypeName = "";
        if (oTitleName != DBNull.Value && oTitleName != null)
            sTypeName = oTitleName.ToString();
        if (oCategoryToken != DBNull.Value && oCategoryToken != null)
        {
            if (sTypeName != "")
                sTypeName += " - ";
            sTypeName += oCategoryToken.ToString();
        }
        string sRet = PageUtils.GetPreHeader() + ": Translation ";
        if (Session["translation_id"] != null && Session["translation_id"].ToString() != "" && Session["translation_id"].ToString() != "0")
            sRet += "(" + sTypeName + ")";
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
        Int32 nGroupID = LoginManager.GetLoginGroupID();

        object t = null; ;
        if (Session["translation_id"] != null && Session["translation_id"].ToString() != "" && int.Parse(Session["translation_id"].ToString()) != 0)
            t = Session["translation_id"];

        string sBack = "adm_tvp_translations.aspx?search_save=1";
        DBRecordWebEditor theRecord = new DBRecordWebEditor("Translation", "adm_table_pager", sBack, "", "ID", t, sBack, "");
        theRecord.SetConnectionKey("tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString());

        string sMainLang = "";
        if (int.Parse(Session["lang_id"].ToString()) == GetMainLang(ref sMainLang) &&  PageUtils.IsTvinciUser() == true)
        {
            DataRecordShortTextField dr_domain = new DataRecordShortTextField("ltr", true, 60, 128);
            dr_domain.Initialize("Token", "adm_table_header_nbg", "FormInput", "TitleID", true);
            theRecord.AddRecord(dr_domain);

            DataRecordShortTextField dr_token = new DataRecordShortTextField("ltr", true, 60, 128);
            dr_token.Initialize("Location Token", "adm_table_header_nbg", "FormInput", "CategoryToken", false);
            theRecord.AddRecord(dr_token);
        }
        if (Session["translation_id"] != null && Session["translation_id"].ToString() != "0" && Session["translation_id"].ToString() != "")
        {
            DataRecordLongTextField dr_text = new DataRecordLongTextField("ltr", true, 60, 5);
            dr_text.Initialize("Translation", "adm_table_header_nbg", "FormInput", "", false);
            if (Session["translation_id"] != null && Session["translation_id"].ToString() != "0")
            {
                dr_text.SetValue(GetCurrentValue("OriginalText", "TranslationMetadata", int.Parse(Session["translation_id"].ToString()), int.Parse(Session["lang_id"].ToString()), "tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString()));
            }
            else
                dr_text.SetValue("");
            theRecord.AddRecord(dr_text);
        }

        DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
        dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", false);
        dr_groups.SetValue(LoginManager.GetLoginGroupID().ToString());
        theRecord.AddRecord(dr_groups);

        string sTable = theRecord.GetTableHTML("adm_tvp_translations_new.aspx?submited=1");
        Response.Expires = -1;
        return sTable;
    }

    protected string GetCurrentValue(string sField, string sTable, Int32 nTransalationID, Int32 nLangID, string sConnKey)
    {
        string sRet = "";
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery.SetConnectionKey(sConnKey);
        selectQuery += "select " + sField + " from " + sTable + " where ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("TranslationID", "=", nTransalationID);
        selectQuery += " and is_active=1 and status=1 and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("LANGUAGE_ID", "=", nLangID);
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

    protected string GetLangMenu(Int32 nGroupID)
    {
        try
        {
            string sTemp = "";
            Int32 nCount = 0;
            string sMainLang = "";
            Int32 nMainLangID = GetMainLang(ref sMainLang);

            string sOnOff = "on";
            if (nMainLangID != int.Parse(Session["lang_id"].ToString()))
                sOnOff = "off";
            sTemp += "<li><a class=\"" + sOnOff + "\" href=\"";
            if (nMainLangID != int.Parse(Session["lang_id"].ToString()))
                sTemp += "adm_tvp_translations_new.aspx?translation_id=" + Session["translation_id"].ToString() + "&lang_id=" + nMainLangID.ToString();
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
                        sTemp += "adm_tvp_translations_new.aspx?translation_id=" + Session["translation_id"].ToString() + "&lang_id=" + nLangID.ToString();
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
        if (Session["translation_id"] != null && Session["translation_id"].ToString() != "0" && Session["translation_id"].ToString() != "")
            Response.Write(m_sLangMenu);
    }

    static protected Int32 GetMainLang(ref string sMainLang)
    {
        Int32 nLangID = 0;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select l.NAME,l.id from groups g,lu_languages l where l.id=g.language_id and  ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("g.id", "=", LoginManager.GetLoginGroupID());
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
            {
                sMainLang = selectQuery.Table("query").DefaultView[0].Row["NAME"].ToString();
                nLangID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
            }
        }
        selectQuery.Finish();
        selectQuery = null;
        return nLangID;
    }
}
