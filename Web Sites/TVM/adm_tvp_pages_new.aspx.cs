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

public partial class adm_tvp_pages_new : System.Web.UI.Page
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
                Int32 nPageID = DBManipulator.DoTheWork("tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString());
                Session["tvp_page_id"] = nPageID.ToString();
                Int32 nLangID = int.Parse(Session["lang_id"].ToString());
                Int32 nPageTextID = 0;
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select id from tvp_pages_texts where is_active=1 and status=1 and ";
                selectQuery.SetConnectionKey("tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString());
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("LANGUAGE_ID", "=", nLangID);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("PAGE_STRUCTURE_ID", "=", nPageID);
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                        nPageTextID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["id"].ToString());
                }
                selectQuery.Finish();
                selectQuery = null;

                Int32 nIter = 14;
                string sLang = "";
                if (int.Parse(Session["lang_id"].ToString()) != GetMainLang(ref sLang))
                    nIter = 0;
                string sTitle = "";
                sTitle = Request.Form[nIter.ToString() + "_val"].ToString();
                nIter++;
                string sDescription = "";
                sDescription = Request.Form[nIter.ToString() + "_val"].ToString();
                nIter++;
                string sBREADCRUMBTEXT = "";
                sBREADCRUMBTEXT = Request.Form[nIter.ToString() + "_val"].ToString();
                nIter++;
                string sKEYWORDS = "";
                sKEYWORDS = Request.Form[nIter.ToString() + "_val"].ToString();
                nIter++;

                if (nPageTextID != 0)
                {
                    ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("tvp_pages_texts");
                    updateQuery.SetConnectionKey("tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString());
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("PAGE_STRUCTURE_ID", "=", nPageID);
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("LANGUAGE_ID", "=", nLangID);
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("NAME", "=", sTitle);
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("DESCRIPTION", "=", sDescription);
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("BREADCRUMBTEXT", "=", sBREADCRUMBTEXT);
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("KEYWORDS", "=", sKEYWORDS);
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATER_ID", "=", LoginManager.GetLoginID());
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATE_DATE", "=", DateTime.UtcNow);
                    updateQuery += " where ";
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nPageTextID);
                    updateQuery.Execute();
                    updateQuery.Finish();
                    updateQuery = null;
                }
                else
                {
                    ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("tvp_pages_texts");
                    insertQuery.SetConnectionKey("tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString());
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("PAGE_STRUCTURE_ID", "=", nPageID);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("LANGUAGE_ID", "=", nLangID);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("NAME", "=", sTitle);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("DESCRIPTION", "=", sDescription);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("BREADCRUMBTEXT", "=", sBREADCRUMBTEXT);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("KEYWORDS", "=", sKEYWORDS);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATER_ID", "=", LoginManager.GetLoginID());
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATE_DATE", "=", DateTime.UtcNow);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CREATE_DATE", "=", DateTime.UtcNow);
                    insertQuery.Execute();
                    insertQuery.Finish();
                    insertQuery = null;
                }

                return;
            }

            m_sMenu = TVinciShared.Menu.GetMainMenu(14, true, ref nMenuID, "adm_tvp_pages.aspx?page_type=" + Session["page_type"].ToString() + "&platform=" + Session["platform"].ToString());
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, true);
            string sMainLang = "";
            if (Request.QueryString["lang_id"] != null &&
               Request.QueryString["lang_id"].ToString() != "")
                Session["lang_id"] = Request.QueryString["lang_id"].ToString();
            else
                Session["lang_id"] = GetMainLang(ref sMainLang);

            if (Request.QueryString["tvp_page_id"] != null &&
               Request.QueryString["tvp_page_id"].ToString() != "")
            {
                Session["tvp_page_id"] = int.Parse(Request.QueryString["tvp_page_id"].ToString());
            }
            else
                Session["tvp_page_id"] = 0;

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
        object oTypeName = ODBCWrapper.Utils.GetTableSingleVal("lu_page_types", "DESCRIPTION", int.Parse(Session["page_type"].ToString()), sConn);
        string sTypeName = "";
        if (oTypeName != DBNull.Value)
            sTypeName = oTypeName.ToString();
        string sRet = PageUtils.GetPreHeader() + ": Page (" + sTypeName + ")";
        if (Session["tvp_page_id"] != null && Session["tvp_page_id"].ToString() != "" && Session["tvp_page_id"].ToString() != "0")
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
        Int32 nGroupID = LoginManager.GetLoginGroupID();

        object t = null; ;
        if (Session["tvp_page_id"] != null && Session["tvp_page_id"].ToString() != "" && int.Parse(Session["tvp_page_id"].ToString()) != 0)
            t = Session["tvp_page_id"];

        string sBack = "adm_tvp_pages.aspx?search_save=1&page_type=" + Session["page_type"].ToString() + "&platform=" + Session["platform"].ToString();
        DBRecordWebEditor theRecord = new DBRecordWebEditor("tvp_pages_structure", "adm_table_pager", sBack, "", "ID", t, sBack, "");
        theRecord.SetConnectionKey("tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString());

        string sMainLang = "";
        if (int.Parse(Session["lang_id"].ToString()) == GetMainLang(ref sMainLang))
        {
            DataRecordShortTextField dr_domain = new DataRecordShortTextField("ltr", true, 60, 128);
            dr_domain.Initialize("Virtual Name", "adm_table_header_nbg", "FormInput", "VIRTUAL_NAME", true);
            theRecord.AddRecord(dr_domain);

            DataRecordRadioField dr_page_profile = new DataRecordRadioField("lu_pages_profiles_types", "description", "id", "", null);
            dr_page_profile.Initialize("Page World", "adm_table_header_nbg", "FormInput", "PAGE_PROFILE_ID", true);
            dr_page_profile.SetDefault(0);
            theRecord.AddRecord(dr_page_profile);

            DataRecordBoolField dr_IS_PROTECTED = new DataRecordBoolField(true);
            dr_IS_PROTECTED.Initialize("Is protected", "adm_table_header_nbg", "FormInput", "IS_PROTECTED", false);
            theRecord.AddRecord(dr_IS_PROTECTED);

            DataRecordBoolField dr_HAS_SIDE = new DataRecordBoolField(true);
            dr_HAS_SIDE.Initialize("Has Side Profile", "adm_table_header_nbg", "FormInput", "HAS_SIDE_PROFILE", true);
            dr_HAS_SIDE.SetDefault(1);
            theRecord.AddRecord(dr_HAS_SIDE);

            /*
            DataRecordDropDownField dr_Top_profile = new DataRecordDropDownField("tvp_profiles", "NAME", "id", "", null, 60, false);
            dr_Top_profile.SetWhereString("PROFILE_TYPE=1 and group_id=" + nGroupID.ToString() + " and is_active=1 and status=1");
            dr_Top_profile.Initialize("Top Profile", "adm_table_header_nbg", "FormInput", "TOP_PROFILE_ID", true);
            theRecord.AddRecord(dr_Top_profile);
            */

            DataRecordDropDownField dr_MENU = new DataRecordDropDownField("TVP_MENU", "NAME", "id", "", null, 60, false);
            dr_MENU.SetWhereString("MENU_TYPE=1 and group_id=" + nGroupID.ToString() + " and is_active=1 and status=1");
            dr_MENU.Initialize("Menu", "adm_table_header_nbg", "FormInput", "MENU_ID", true);
            theRecord.AddRecord(dr_MENU);

            DataRecordDropDownField dr_FOOTER = new DataRecordDropDownField("TVP_MENU", "NAME", "id", "", null, 60, false);
            dr_FOOTER.SetWhereString("MENU_TYPE=2 and group_id=" + nGroupID.ToString() + " and is_active=1 and status=1");
            dr_FOOTER.Initialize("Footer", "adm_table_header_nbg", "FormInput", "FOOTER_ID", true);
            theRecord.AddRecord(dr_FOOTER);

            DataRecordDropDownField dr_SUB_FOOTER = new DataRecordDropDownField("TVP_MENU", "NAME", "id", "", null, 60, false);
            dr_SUB_FOOTER.SetWhereString("MENU_TYPE=2 and group_id=" + nGroupID.ToString() + " and is_active=1 and status=1");
            dr_SUB_FOOTER.Initialize("Middle Footer", "adm_table_header_nbg", "FormInput", "MIDDLE_FOOTER_ID", true);
            theRecord.AddRecord(dr_SUB_FOOTER);

            DataRecordDropDownField dr_Side_profile = new DataRecordDropDownField("tvp_profiles", "NAME", "id", "", null, 60, false);
            dr_Side_profile.SetWhereString("PROFILE_TYPE=2 and group_id=" + nGroupID.ToString() + " and is_active=1 and status=1");
            dr_Side_profile.Initialize("Side Profile", "adm_table_header_nbg", "FormInput", "SIDE_PROFILE_ID", true);
            theRecord.AddRecord(dr_Side_profile);

            DataRecordDropDownField dr_Bot_profile = new DataRecordDropDownField("tvp_profiles", "NAME", "id", "", null, 60, false);
            dr_Bot_profile.SetWhereString("PROFILE_TYPE=3 and group_id=" + nGroupID.ToString() + " and is_active=1 and status=1");
            dr_Bot_profile.Initialize("Bottom Profile", "adm_table_header_nbg", "FormInput", "BOTTOM_PROFILE_ID", true);
            theRecord.AddRecord(dr_Bot_profile);

            DataRecordOnePicBrowserField dr_BRANDING_BIG_PIC_ID = new DataRecordOnePicBrowserField();
            dr_BRANDING_BIG_PIC_ID.Initialize("Branding Main Image", "adm_table_header_nbg", "FormInput", "BRANDING_BIG_PIC_ID", false);
            theRecord.AddRecord(dr_BRANDING_BIG_PIC_ID);

            DataRecordOnePicBrowserField dr_BRANDING_SMALL_PIC_ID = new DataRecordOnePicBrowserField();
            dr_BRANDING_SMALL_PIC_ID.Initialize("Branding Recurring Image", "adm_table_header_nbg", "FormInput", "BRANDING_SMALL_PIC_ID", false);
            theRecord.AddRecord(dr_BRANDING_SMALL_PIC_ID);

            DataRecordShortIntField dr_BRANDING_PIXEL_HEIGHT = new DataRecordShortIntField(true, 9, 9);
            dr_BRANDING_PIXEL_HEIGHT.Initialize("Branding Pixel Height", "adm_table_header_nbg", "FormInput", "BRANDING_PIXEL_HEIGHT", false);
            theRecord.AddRecord(dr_BRANDING_PIXEL_HEIGHT);

            DataRecordBoolField dr_IS_RECURRING_HORIZONTAL = new DataRecordBoolField(true);
            dr_IS_RECURRING_HORIZONTAL.Initialize("Is recurring horizontal", "adm_table_header_nbg", "FormInput", "IS_RECURRING_HORIZONTAL", false);
            theRecord.AddRecord(dr_IS_RECURRING_HORIZONTAL);

            DataRecordBoolField dr_IS_RECURRING_VERTICAL = new DataRecordBoolField(true);
            dr_IS_RECURRING_VERTICAL.Initialize("Is recurring vertical", "adm_table_header_nbg", "FormInput", "IS_RECURRING_VERTICAL", false);
            theRecord.AddRecord(dr_IS_RECURRING_VERTICAL);
        }

        DataRecordShortTextField dr_name = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_name.Initialize("Title", "adm_table_header_nbg", "FormInput", "", false);
        if (Session["tvp_page_id"] != null && Session["tvp_page_id"].ToString() != "0")
        {
            dr_name.SetValue(GetCurrentValue("NAME", "tvp_pages_texts", int.Parse(Session["tvp_page_id"].ToString()), int.Parse(Session["lang_id"].ToString()), "tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString()));
        }
        else
            dr_name.SetValue("");
        theRecord.AddRecord(dr_name);

        DataRecordShortTextField dr_DESCRIPTION = new DataRecordShortTextField("ltr", true, 60, 510);
        dr_DESCRIPTION.Initialize("Description", "adm_table_header_nbg", "FormInput", "", false);
        if (Session["tvp_page_id"] != null && Session["tvp_page_id"].ToString() != "0")
        {
            dr_DESCRIPTION.SetValue(GetCurrentValue("DESCRIPTION", "tvp_pages_texts", int.Parse(Session["tvp_page_id"].ToString()), int.Parse(Session["lang_id"].ToString()), "tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString()));
        }
        else
            dr_DESCRIPTION.SetValue("");
        theRecord.AddRecord(dr_DESCRIPTION);

        DataRecordShortTextField dr_BREADCRUMBTEXT = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_BREADCRUMBTEXT.Initialize("Breadcrumb", "adm_table_header_nbg", "FormInput", "", false);
        if (Session["tvp_page_id"] != null && Session["tvp_page_id"].ToString() != "0")
        {
            dr_BREADCRUMBTEXT.SetValue(GetCurrentValue("BREADCRUMBTEXT", "tvp_pages_texts", int.Parse(Session["tvp_page_id"].ToString()), int.Parse(Session["lang_id"].ToString()), "tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString()));
        }
        else
            dr_BREADCRUMBTEXT.SetValue("");
        theRecord.AddRecord(dr_BREADCRUMBTEXT);

        DataRecordShortTextField dr_KEYWORDS = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_KEYWORDS.Initialize("Key Words (; seperated)", "adm_table_header_nbg", "FormInput", "", false);
        if (Session["tvp_page_id"] != null && Session["tvp_page_id"].ToString() != "0")
        {
            dr_KEYWORDS.SetValue(GetCurrentValue("KEYWORDS", "tvp_pages_texts", int.Parse(Session["tvp_page_id"].ToString()), int.Parse(Session["lang_id"].ToString()), "tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString()));
        }
        else
            dr_KEYWORDS.SetValue("");
        theRecord.AddRecord(dr_KEYWORDS);
        
        DataRecordShortIntField dr_PAGE_TYPE = new DataRecordShortIntField(false, 9, 9);
        dr_PAGE_TYPE.Initialize("PAGE_TYPE", "adm_table_header_nbg", "FormInput", "PAGE_TYPE", false);
        dr_PAGE_TYPE.SetValue(Session["page_type"].ToString());
        theRecord.AddRecord(dr_PAGE_TYPE);

        DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
        dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", false);
        dr_groups.SetValue(LoginManager.GetLoginGroupID().ToString());
        theRecord.AddRecord(dr_groups);

        string sTable = theRecord.GetTableHTML("adm_tvp_pages_new.aspx?submited=1");
        Response.Expires = -1;
        return sTable;
    }

    protected string GetCurrentValue(string sField, string sTable, Int32 nGalleryID, Int32 nLangID, string sConnKey)
    {
        string sRet = "";
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery.SetConnectionKey(sConnKey);
        selectQuery += "select " + sField + " from " + sTable + " where ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("PAGE_STRUCTURE_ID", "=", nGalleryID);
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
                sTemp += "adm_tvp_pages_new.aspx?tvp_page_id=" + Session["tvp_page_id"].ToString() + "&lang_id=" + nMainLangID.ToString();
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
                        sTemp += "adm_tvp_pages_new.aspx?tvp_page_id=" + Session["tvp_page_id"].ToString() + "&lang_id=" + nLangID.ToString();
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
