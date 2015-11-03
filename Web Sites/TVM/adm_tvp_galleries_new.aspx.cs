using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using KLogMonitor;
using TVinciShared;

public partial class adm_tvp_galleries_new : System.Web.UI.Page
{
    private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
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
                Int32 nGalleryID = DBManipulator.DoTheWork("tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString());
                Session["gallery_id"] = nGalleryID.ToString();
                Int32 nLangID = int.Parse(Session["lang_id"].ToString());
                Int32 nGalleryTextID = 0;
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select id from tvp_galleries_text where is_active=1 and status=1 and ";
                selectQuery.SetConnectionKey("tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString());
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("LANGUAGE_ID", "=", nLangID);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("tvp_gallery_ID", "=", nGalleryID);
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                        nGalleryTextID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["id"].ToString());
                }
                selectQuery.Finish();
                selectQuery = null;

                Int32 nIter = 1;
                string sLang = "";
                if (int.Parse(Session["lang_id"].ToString()) != GetMainLang(ref sLang))
                    nIter = 0;
                string sTitle = "";
                if (IsFieldAvailable("GROUP_HAS_TITLE", "tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString()) == true)
                {
                    sTitle = Request.Form[nIter.ToString() + "_val"].ToString();
                    nIter++;
                }
                string sMainDescription = "";
                if (IsFieldAvailable("GROUP_HAS_MAIN_DESCRIPTION", "tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString()) == true)
                {
                    sMainDescription = Request.Form[nIter.ToString() + "_val"].ToString();
                    nIter++;
                }

                string sSubDescription = "";
                if (IsFieldAvailable("GROUP_HAS_SUB_DESCRIPTION", "tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString()) == true)
                {
                    sSubDescription = Request.Form[nIter.ToString() + "_val"].ToString();
                    nIter++;
                }
                string sLinksHeader = "";
                if (IsFieldAvailable("HAS_BUTTONS", "tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString()) == true)
                {
                    sLinksHeader = Request.Form[nIter.ToString() + "_val"].ToString();
                    nIter++;
                }

                if (nGalleryTextID != 0)
                {
                    ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("tvp_galleries_text");
                    updateQuery.SetConnectionKey("tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString());
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("tvp_gallery_ID", "=", nGalleryID);
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("LANGUAGE_ID", "=", nLangID);
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("TITLE", "=", sTitle);
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("LINKS_HEADER", "=", sLinksHeader);
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("MAIN_DESCRIPTION", "=", sMainDescription);
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("SUB_DESCRIPTION", "=", sSubDescription);
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATER_ID", "=", LoginManager.GetLoginID());
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATE_DATE", "=", DateTime.UtcNow);
                    updateQuery += " where ";
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nGalleryTextID);
                    updateQuery.Execute();
                    updateQuery.Finish();
                    updateQuery = null;
                }
                else
                {
                    ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("tvp_galleries_text");
                    insertQuery.SetConnectionKey("tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString());
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("tvp_gallery_ID", "=", nGalleryID);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("LANGUAGE_ID", "=", nLangID);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("TITLE", "=", sTitle);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("MAIN_DESCRIPTION", "=", sMainDescription);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("SUB_DESCRIPTION", "=", sSubDescription);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("LINKS_HEADER", "=", sLinksHeader);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATER_ID", "=", LoginManager.GetLoginID());
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATE_DATE", "=", DateTime.UtcNow);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CREATE_DATE", "=", DateTime.UtcNow);
                    insertQuery.Execute();
                    insertQuery.Finish();
                    insertQuery = null;
                }
                Int32 nNumOfItems = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("tvp_template_channels_gallery_types", "NUM_OF_ITEMS", int.Parse(Session["template_id"].ToString()), "tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString()).ToString());
                if (nNumOfItems == 1)
                {
                    object oID = ODBCWrapper.Utils.GetTableSingleVal("tvp_galleries_items", "ID", "TVP_GALLERY_ID", "=", int.Parse(Session["gallery_id"].ToString()), "tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString());
                    if (oID == null || oID == DBNull.Value)
                        InsertNewRecord("tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString(), nGroupID);
                }
                return;
            }
            string sMainLang = "";
            if (Session["tvp_profile_id"] != null)
                m_sMenu = TVinciShared.Menu.GetMainMenu(14, true, ref nMenuID, "adm_tvp_profiles.aspx?profile_loc=" + Session["profile_loc"].ToString() + "&platform=" + Session["platform"].ToString());
            if (Session["tvp_page_id"] != null)
                m_sMenu = TVinciShared.Menu.GetMainMenu(14, true, ref nMenuID, "adm_tvp_pages.aspx?page_type=" + Session["page_type"].ToString() + "&platform=" + Session["platform"].ToString());
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, true);

            if (Request.QueryString["lang_id"] != null &&
               Request.QueryString["lang_id"].ToString() != "")
                Session["lang_id"] = Request.QueryString["lang_id"].ToString();
            else
                Session["lang_id"] = GetMainLang(ref sMainLang);

            if (Request.QueryString["gallery_id"] != null &&
               Request.QueryString["gallery_id"].ToString() != "")
            {
                Session["gallery_id"] = int.Parse(Request.QueryString["gallery_id"].ToString());
            }
            else
                Session["gallery_id"] = 0;

            if (Request.QueryString["template_id"] != null &&
               Request.QueryString["template_id"].ToString() != "")
            {
                Session["template_id"] = int.Parse(Request.QueryString["template_id"].ToString());
            }
            else
            {
                if (Session["gallery_id"] != null && Session["gallery_id"].ToString() != "0")
                {
                    Session["template_id"] = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("tvp_galleries", "CHANNEL_TEMPLATE_ID", int.Parse(Session["gallery_id"].ToString()), "tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString()).ToString());
                }
                else
                {
                    LoginManager.LogoutFromSite("login.html");
                    return;
                }
            }
            m_sLangMenu = GetLangMenu(nGroupID);
        }
    }

    static protected object GetDefaultVal(Int32 nGalleryID, string sField, string sCollKey)
    {
        if (CachingManager.CachingManager.Exist("GetDefaultVal_" + sField + "_" + sCollKey + "_" + nGalleryID.ToString()) == true)
        {
            return CachingManager.CachingManager.GetCachedData("GetDefaultVal_" + sField + "_" + sCollKey + "_" + nGalleryID.ToString());
        }
        Int32 nTemplateID = 0;
        if (CachingManager.CachingManager.Exist("TemplateID" + nGalleryID.ToString()) == true)
        {
            nTemplateID = int.Parse(CachingManager.CachingManager.GetCachedData("TemplateID" + nGalleryID.ToString()).ToString());
        }
        else
        {
            if (nGalleryID != 0)
                nTemplateID = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("tvp_galleries", "CHANNEL_TEMPLATE_ID", nGalleryID, 10000, sCollKey).ToString());
            else
                nTemplateID = int.Parse(HttpContext.Current.Session["template_id"].ToString());
            CachingManager.CachingManager.SetCachedData("TemplateID" + nGalleryID.ToString(), nTemplateID, 3600, System.Web.Caching.CacheItemPriority.Normal, 0, false);
        }
        //Int32 nTemplateID = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("tvp_galleries", "CHANNEL_TEMPLATE_ID", nGalleryID, 10000, sCollKey).ToString());
        object oRet = ODBCWrapper.Utils.GetTableSingleVal("tvp_template_channels_gallery_types", sField, nTemplateID, 10000, sCollKey);
        CachingManager.CachingManager.SetCachedData("GetDefaultVal_" + sField + "_" + sCollKey + "_" + nGalleryID.ToString(), oRet, 3600, System.Web.Caching.CacheItemPriority.Normal, 0, false);
        return oRet;
    }

    protected void InsertNewRecord(string sCollKey, Int32 nGroupID)
    {
        ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("tvp_galleries_items");
        insertQuery.SetConnectionKey(sCollKey);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("ORDER_NUM", "=", 1);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("VIRTUAL_NAME", "=", "---");
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("TVP_GALLERY_ID", "=", int.Parse(Session["gallery_id"].ToString()));
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("PAGE_SIZE", "=", GetDefaultVal(int.Parse(Session["gallery_id"].ToString()), "DEFAULT_PAGE_SIZE", sCollKey));
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("MAX_RESULT_NUM", "=", GetDefaultVal(int.Parse(Session["gallery_id"].ToString()), "DEFAULT_MAX_RESULT_NUM", sCollKey));
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("VIEW_TYPE", "=", GetDefaultVal(int.Parse(Session["gallery_id"].ToString()), "DEFAULT_VIEW_TYPE", sCollKey));
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("PIC_SIZE", "=", GetDefaultVal(int.Parse(Session["gallery_id"].ToString()), "DEFAULT_PIC_SIZE", sCollKey));
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("SUB_PIC_SIZE", "=", GetDefaultVal(int.Parse(Session["gallery_id"].ToString()), "DEFAULT_SUB_PIC_SIZE", sCollKey));
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("LINK", "=", "");
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CHANNEL_MAIN", "=", 0);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CHANNEL_SUB", "=", 0);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("PIC_MAIN", "=", 0);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("PIC_SUB", "=", 0);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("SWF", "=", "");
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("MAIN_TVM_ACCOUNT_ID", "=", GetDefaultVal(int.Parse(Session["gallery_id"].ToString()), "DEFAULT_TVM_FOR_CHANNEL_MAIN", sCollKey));
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("SUB_TVM_ACCOUNT_ID", "=", GetDefaultVal(int.Parse(Session["gallery_id"].ToString()), "DEFAULT_TVM_FOR_CHANNEL_SUB", sCollKey));
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATER_ID", "=", LoginManager.GetLoginID());
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATE_DATE", "=", DateTime.UtcNow);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CREATE_DATE", "=", DateTime.UtcNow);
        insertQuery.Execute();
        insertQuery.Finish();
        insertQuery = null;
    }

    static protected bool IsFieldAvailable(string sField, string sCollKey)
    {
        Int32 nOK = 0;
        Int32 nTemplateID = int.Parse(HttpContext.Current.Session["template_id"].ToString());

        DataTable dt = null;
        if (CachingManager.CachingManager.Exist("IsFieldAvailable_" + sCollKey + "_" + nTemplateID.ToString()) == true)
        {
            dt = (DataTable)(CachingManager.CachingManager.GetCachedData("IsFieldAvailable_" + sCollKey + "_" + nTemplateID.ToString()));
        }
        else
        {
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetCachedSec(10000);
            selectQuery.SetConnectionKey(sCollKey);
            selectQuery += " select * from tvp_template_channels_gallery_types where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nTemplateID);
            if (selectQuery.Execute("query", true) != null)
            {
                dt = selectQuery.Table("query");
                CachingManager.CachingManager.SetCachedData("IsFieldAvailable_" + sCollKey + "_" + nTemplateID.ToString(), selectQuery.Table("query").Copy(), 3600, System.Web.Caching.CacheItemPriority.Normal, 0, false);
            }
            selectQuery.Finish();
            selectQuery = null;
        }
        nOK = int.Parse(dt.DefaultView[0].Row[sField].ToString());
        if (nOK == 0)
            return false;
        return true;
        /*
        if (CachingManager.CachingManager.Exist("IsFieldAvailable_" + sField + "_" + sCollKey + "_" + nTemplateID.ToString()) == true)
        {
            nOK = int.Parse(CachingManager.CachingManager.GetCachedData("IsFieldAvailable_" + sField + "_" + sCollKey + "_" + nTemplateID.ToString()).ToString());
        }
        else
        {
            nOK = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("tvp_template_channels_gallery_types", sField, nTemplateID, 10000, sCollKey).ToString());
            CachingManager.CachingManager.SetCachedData("IsFieldAvailable_" + sField + "_" + sCollKey + "_" + nTemplateID.ToString(), nOK, 3600, System.Web.Caching.CacheItemPriority.Normal, 0, false);
        }
        if (nOK == 0)
            return false;
        return true;
        */
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

    protected string GetLangMenu(Int32 nGroupID)
    {
        if (Session["gallery_id"] == null || Session["gallery_id"].ToString() == "0" || Session["gallery_id"].ToString() == "" ||
            (IsFieldAvailable("GROUP_HAS_TITLE", "tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString()) == false &&
            IsFieldAvailable("GROUP_HAS_MAIN_DESCRIPTION", "tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString()) == false &&
            IsFieldAvailable("GROUP_HAS_SUB_DESCRIPTION", "tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString()) == false) &&
            IsFieldAvailable("HAS_BUTTONS", "tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString()) == false)
            return "";
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
                sTemp += "adm_tvp_galleries_new.aspx?gallery_id=" + Session["gallery_id"].ToString() + "&template_id=" + Session["template_id"].ToString() + "&lang_id=" + nMainLangID.ToString();
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
                        sTemp += "adm_tvp_galleries_new.aspx?gallery_id=" + Session["gallery_id"].ToString() + "&template_id=" + Session["template_id"].ToString() + "&lang_id=" + nLangID.ToString();
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

    public void GetHeader()
    {
        try
        {
            string sRet = PageUtils.GetPreHeader() + ": Galleries ";
            Int32 nGroupID = LoginManager.GetLoginGroupID();
            if (Session["gallery_id"] != null && Session["gallery_id"].ToString() != "" && Session["gallery_id"].ToString() != "0")
            {
                sRet += "(" + ODBCWrapper.Utils.GetTableSingleVal("tvp_galleries", "VIRTUAL_NAME", int.Parse(Session["gallery_id"].ToString()), "tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString()) + ")";
                sRet += " - Edit";
            }
            else
                sRet += " - New";
            Response.Write(sRet);
        }
        catch { }
    }

    protected void GetLangMenu()
    {
        Response.Write(m_sLangMenu);
    }

    protected void GetSubMenu()
    {
        Response.Write(m_sSubMenu);
    }

    protected string GetCurrentValue(string sField, string sTable, Int32 nGalleryID, Int32 nLangID, string sConnKey)
    {
        string sRet = "";
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery.SetConnectionKey(sConnKey);
        selectQuery += "select " + sField + " from " + sTable + " where ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("tvp_gallery_ID", "=", nGalleryID);
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

    public string GetPageContent(string sOrderBy, string sPageNum)
    {
        try
        {
            if (Session["error_msg"] != null && Session["error_msg"].ToString() != "")
            {
                Session["error_msg"] = "";
                return Session["last_page_html"].ToString();
            }
            Int32 nGroupID = LoginManager.GetLoginGroupID();
            string sBack = "";
            object t = null; ;
            if (Session["gallery_id"] != null && Session["gallery_id"].ToString() != "" && Session["gallery_id"].ToString() != "0")
                t = Session["gallery_id"];
            Int32 nNumOfItems = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("tvp_template_channels_gallery_types", "NUM_OF_ITEMS", int.Parse(Session["template_id"].ToString()), "tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString()).ToString());

            if (nNumOfItems == 3 || nNumOfItems == 1 || t != null)
            {
                if (Session["tvp_profile_id"] != null)
                    sBack = "adm_tvp_galleries.aspx?search_save=1&tvp_profile_id=" + Session["tvp_profile_id"].ToString();
                if (Session["tvp_page_id"] != null)
                    sBack = "adm_tvp_galleries.aspx?search_save=1&tvp_page_id=" + Session["tvp_page_id"].ToString();
            }
            else
                sBack = "adm_tvp_galleries_items_new.aspx?auth=1";


            string sCancel = "adm_tvp_galleries.aspx?search_save=1&";
            if (Session["tvp_profile_id"] != null)
                sCancel += "tvp_profile_id=" + Session["tvp_profile_id"].ToString();
            if (Session["tvp_page_id"] != null)
                sCancel += "tvp_page_id=" + Session["tvp_page_id"].ToString();
            DBRecordWebEditor theRecord = new DBRecordWebEditor("tvp_galleries", "adm_table_pager", sBack, "", "ID", t, sCancel, "");
            theRecord.SetConnectionKey("tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString());
            string sLang = "";
            if (int.Parse(Session["lang_id"].ToString()) == GetMainLang(ref sLang))
            {
                DataRecordShortTextField dr_domain = new DataRecordShortTextField("ltr", true, 60, 128);
                dr_domain.Initialize("Gallery Virtual Name", "adm_table_header_nbg", "FormInput", "VIRTUAL_NAME", true);
                theRecord.AddRecord(dr_domain);
            }

            if (IsFieldAvailable("GROUP_HAS_TITLE", "tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString()) == true)
            {
                DataRecordShortTextField dr_title = new DataRecordShortTextField("ltr", true, 60, 128);
                dr_title.Initialize("Title", "adm_table_header_nbg", "FormInput", "", false);
                if (Session["gallery_id"] != null && Session["gallery_id"].ToString() != "0")
                {
                    dr_title.SetValue(GetCurrentValue("TITLE", "tvp_galleries_text", int.Parse(Session["gallery_id"].ToString()), int.Parse(Session["lang_id"].ToString()), "tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString()));
                }
                else
                    dr_title.SetValue("");
                theRecord.AddRecord(dr_title);
            }
            if (IsFieldAvailable("GROUP_HAS_MAIN_DESCRIPTION", "tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString()) == true)
            {
                DataRecordShortTextField dr_desc = new DataRecordShortTextField("ltr", true, 60, 128);
                dr_desc.Initialize("Main Description", "adm_table_header_nbg", "FormInput", "", false);
                if (Session["gallery_id"] != null && Session["gallery_id"].ToString() != "0")
                {
                    dr_desc.SetValue(GetCurrentValue("MAIN_DESCRIPTION", "tvp_galleries_text", int.Parse(Session["gallery_id"].ToString()), int.Parse(Session["lang_id"].ToString()), "tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString()));
                }
                else
                    dr_desc.SetValue("");
                theRecord.AddRecord(dr_desc);
            }
            if (IsFieldAvailable("GROUP_HAS_SUB_DESCRIPTION", "tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString()) == true)
            {
                DataRecordShortTextField dr_sub_desc = new DataRecordShortTextField("ltr", true, 60, 128);
                dr_sub_desc.Initialize("Sub Description", "adm_table_header_nbg", "FormInput", "", false);
                if (Session["gallery_id"] != null && Session["gallery_id"].ToString() != "0")
                {
                    dr_sub_desc.SetValue(GetCurrentValue("SUB_DESCRIPTION", "tvp_galleries_text", int.Parse(Session["gallery_id"].ToString()), int.Parse(Session["lang_id"].ToString()), "tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString()));
                }
                else
                    dr_sub_desc.SetValue("");
                theRecord.AddRecord(dr_sub_desc);
            }
            if (IsFieldAvailable("HAS_BUTTONS", "tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString()) == true)
            {
                DataRecordShortTextField dr_links_header = new DataRecordShortTextField("ltr", true, 60, 128);
                dr_links_header.Initialize("Links Header", "adm_table_header_nbg", "FormInput", "", false);
                if (Session["gallery_id"] != null && Session["gallery_id"].ToString() != "0")
                {
                    dr_links_header.SetValue(GetCurrentValue("LINKS_HEADER", "tvp_galleries_text", int.Parse(Session["gallery_id"].ToString()), int.Parse(Session["lang_id"].ToString()), "tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString()));
                }
                else
                    dr_links_header.SetValue("");
                theRecord.AddRecord(dr_links_header);
            }

            if (int.Parse(Session["lang_id"].ToString()) == GetMainLang(ref sLang))
            {
                if (IsFieldAvailable("HAS_TVM_ACCOUNT", "tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString()) == true)
                {
                    DataRecordDropDownField dr_TVM_ACCOUNT_ID = new DataRecordDropDownField("tvp_tvm_accounts", "NAME", "id", "", null, 60, false);
                    dr_TVM_ACCOUNT_ID.SetWhereString("status<>2 and id<>0 and group_id=" + nGroupID.ToString());
                    dr_TVM_ACCOUNT_ID.Initialize("TVM Account", "adm_table_header_nbg", "FormInput", "TVM_ACCOUNT_ID", true);
                    theRecord.AddRecord(dr_TVM_ACCOUNT_ID);
                }
                else
                {
                    DataRecordShortIntField dr_TVM_ACCOUNT_ID = new DataRecordShortIntField(false, 9, 9);
                    dr_TVM_ACCOUNT_ID.Initialize("Group", "adm_table_header_nbg", "FormInput", "TVM_ACCOUNT_ID", false);
                    try
                    {
                        dr_TVM_ACCOUNT_ID.SetValue(GetDefaultVal(int.Parse(Session["gallery_id"].ToString()), "DEFAULT_TVM_ACCOUNT", "tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString()).ToString());
                    }
                    catch
                    {

                    }
                    theRecord.AddRecord(dr_TVM_ACCOUNT_ID);
                }

                DataRecordMultiField dr_devices = new DataRecordMultiField("lu_devices", "id", "id", "tvp_galleries_devices", "tvp_gallery_id", "DEVICE_ID", false, "ltr", 60, "tags");
                dr_devices.Initialize("Applications(None for All)", "adm_table_header_nbg", "FormInput", "DESCRIPTION", false);
                dr_devices.SetOrderCollectionBy("newid()");
                theRecord.AddRecord(dr_devices);

                DataRecordMultiField dr_countries = new DataRecordMultiField("lu_countries", "id", "id", "tvp_galleries_countries", "tvp_gallery_id", "COUNTRY_ID", false, "ltr", 60, "tags");
                dr_countries.Initialize("Countries(None for All)", "adm_table_header_nbg", "FormInput", "COUNTRY_NAME", false);
                dr_countries.SetOrderCollectionBy("newid()");
                theRecord.AddRecord(dr_countries);

                DataRecordMultiField dr_lang = new DataRecordMultiField("lu_languages", "id", "id", "tvp_galleries_langs", "tvp_gallery_id", "LANGUAGE_ID", false, "ltr", 60, "tags");
                dr_lang.Initialize("Languages(None for All)", "adm_table_header_nbg", "FormInput", "NAME", false);
                dr_lang.SetOrderCollectionBy("newid()");
                theRecord.AddRecord(dr_lang);

                DataRecordMultiField dr_us = new DataRecordMultiField("lu_user_states", "id", "id", "tvp_galleries_user_states", "tvp_gallery_id", "USER_STATE_ID", false, "ltr", 60, "tags");
                dr_us.Initialize("User Profiles(None for All)", "adm_table_header_nbg", "FormInput", "DESCRIPTION", false);
                dr_us.SetOrderCollectionBy("newid()");
                theRecord.AddRecord(dr_us);
            }

            DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
            dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", false);
            dr_groups.SetValue(LoginManager.GetLoginGroupID().ToString());
            theRecord.AddRecord(dr_groups);
            if (Session["tvp_profile_id"] != null)
            {
                DataRecordShortIntField dr_PROFILE_ID = new DataRecordShortIntField(false, 9, 9);
                dr_PROFILE_ID.Initialize("Group", "adm_table_header_nbg", "FormInput", "PROFILE_ID", false);
                dr_PROFILE_ID.SetValue(Session["tvp_profile_id"].ToString());
                theRecord.AddRecord(dr_PROFILE_ID);
            }
            if (Session["tvp_page_id"] != null)
            {
                DataRecordShortIntField dr_PAGE_STRUCTURE_ID = new DataRecordShortIntField(false, 9, 9);
                dr_PAGE_STRUCTURE_ID.Initialize("Group", "adm_table_header_nbg", "FormInput", "PAGE_STRUCTURE_ID", false);
                dr_PAGE_STRUCTURE_ID.SetValue(Session["tvp_page_id"].ToString());
                theRecord.AddRecord(dr_PAGE_STRUCTURE_ID);

                DataRecordShortIntField dr_loc_ID = new DataRecordShortIntField(false, 9, 9);
                dr_loc_ID.Initialize("Group", "adm_table_header_nbg", "FormInput", "LOCATION_ID", false);
                dr_loc_ID.SetValue(Session["page_location"].ToString());
                theRecord.AddRecord(dr_loc_ID);
            }

            DataRecordShortIntField dr_CHANNEL_TEMPLATE_ID = new DataRecordShortIntField(false, 9, 9);
            dr_CHANNEL_TEMPLATE_ID.Initialize("Group", "adm_table_header_nbg", "FormInput", "CHANNEL_TEMPLATE_ID", false);
            dr_CHANNEL_TEMPLATE_ID.SetValue(Session["template_id"].ToString());
            theRecord.AddRecord(dr_CHANNEL_TEMPLATE_ID);

            DataRecordShortIntField dr_MAIN_NUM = new DataRecordShortIntField(false, 9, 9);
            dr_MAIN_NUM.Initialize("Group", "adm_table_header_nbg", "FormInput", "MAIN_NUM", false);
            dr_MAIN_NUM.SetValue("1");
            theRecord.AddRecord(dr_MAIN_NUM);

            string sTable = theRecord.GetTableHTML("adm_tvp_galleries_new.aspx?submited=1");

            return sTable;
        }
        catch (Exception ex)
        {
            log.Error("sdfsdF " + ex.Message + "||" + ex.StackTrace, ex);
            return "";
        }
    }
}
