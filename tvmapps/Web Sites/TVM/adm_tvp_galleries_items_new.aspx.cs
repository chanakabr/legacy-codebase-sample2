using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Runtime.Caching;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using TVinciShared;

public partial class adm_tvp_galleries_items_new : System.Web.UI.Page
{
    protected string m_sMenu;
    protected string m_sSubMenu;
    protected string m_sLangMenu;

    protected Int32 GetGalleryItemTempalteID(Int32 nGalleryItemID , Int32 nGroupID)
    {
        Int32 nRet = 0;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery.SetConnectionKey("tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString());
        selectQuery += "select tg.CHANNEL_TEMPLATE_ID from tvp_galleries tg,tvp_galleries_items tgi where tgi.TVP_GALLERY_ID=tg.id and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("tgi.ID", "=", nGalleryItemID);
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
                nRet = int.Parse(selectQuery.Table("query").DefaultView[0].Row["CHANNEL_TEMPLATE_ID"].ToString());
        }
        selectQuery.Finish();
        selectQuery = null;
        return nRet;
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
            Int32 nGroupID = LoginManager.GetLoginGroupID();
            if (Request.QueryString["submited"] != null && Request.QueryString["submited"].ToString() == "1")
            {
                Int32 nGalleryItemID = DBManipulator.DoTheWork("tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString());
                CachingManager.CachingManager.RemoveFromCache("GetTVMAccountDetails_");
                
                if (nGalleryItemID != 0)
                {
                    Int32 nTemplateID = GetGalleryItemTempalteID(nGalleryItemID , nGroupID);
                    Session["template_id"] = nTemplateID;
                }
                Session["gallery_item_id"] = nGalleryItemID.ToString();
                Int32 nLangID = int.Parse(Session["lang_id"].ToString());
                Int32 nGalleryItemTextID = 0;
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select id from tvp_galleries_items_text where is_active=1 and status=1 and ";
                selectQuery.SetConnectionKey("tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString());
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("LANGUAGE_ID", "=", nLangID);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("tvp_gallery_item_ID", "=", nGalleryItemID);
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                        nGalleryItemTextID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["id"].ToString());
                }
                selectQuery.Finish();
                selectQuery = null;

                Int32 nIter = 1;
                string sLang = "";
                Int32 nNumOfItems = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("tvp_template_channels_gallery_types", "NUM_OF_ITEMS", int.Parse(Session["template_id"].ToString()), "tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString()).ToString());
                if (int.Parse(Session["lang_id"].ToString()) != GetMainLang(ref sLang) || nNumOfItems !=3)
                    nIter = 0;
                string sTitle = "";
                bool bWithLang = false;
                if (IsFieldAvailable("HAS_TITLE", "tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString()) == true)
                {
                    sTitle = Request.Form[nIter.ToString() + "_val"].ToString();
                    bWithLang = true;
                    nIter++;
                }
                string sMainDescription = "";
                if (IsFieldAvailable("HAS_MAIN_DESCRIPTION", "tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString()) == true)
                {
                    sMainDescription = Request.Form[nIter.ToString() + "_val"].ToString();
                    bWithLang = true;
                    nIter++;
                }

                string sSubDescription = "";
                if (IsFieldAvailable("HAS_SUB_DESCRIPTION", "tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString()) == true)
                {
                    sSubDescription = Request.Form[nIter.ToString() + "_val"].ToString();
                    bWithLang = true;
                    nIter++;
                }

                string sToolTip = "";
                if (IsFieldAvailable("HAS_TOOLTIP_TEXT", "tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString()) == true)
                {
                    sToolTip = Request.Form[nIter.ToString() + "_val"].ToString();
                    bWithLang = true;
                    nIter++;
                }

                if (nGalleryItemTextID != 0)
                {
                    ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("tvp_galleries_items_text");
                    updateQuery.SetConnectionKey("tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString());
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("tvp_gallery_item_ID", "=", nGalleryItemID);
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
                    //if (bWithLang == true)
                    //    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("LANGUAGE_ID", "=", nLangID);
                    //else
                    //    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("LANGUAGE_ID", "=", 0);

                    if (nLangID != 0)
                    {
                        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("LANGUAGE_ID", "=", nLangID);
                    }
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("TITLE", "=", sTitle);
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("MAIN_DESCRIPTION", "=", sMainDescription);
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("SUB_DESCRIPTION", "=", sSubDescription);
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("TOOLTIP_TEXT", "=", sToolTip);
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATER_ID", "=", LoginManager.GetLoginID());
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATE_DATE", "=", DateTime.UtcNow);
                    updateQuery += " where ";
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nGalleryItemTextID);
                    updateQuery.Execute();
                    updateQuery.Finish();
                    updateQuery = null;
                }
                else
                {
                    ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("tvp_galleries_items_text");
                    insertQuery.SetConnectionKey("tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString());
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("tvp_gallery_item_ID", "=", nGalleryItemID);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("LANGUAGE_ID", "=", nLangID);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("TITLE", "=", sTitle);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("MAIN_DESCRIPTION", "=", sMainDescription);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("SUB_DESCRIPTION", "=", sSubDescription);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("TOOLTIP_TEXT", "=", sToolTip);
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

            if (Request.QueryString["gallery_item_id"] != null &&
               Request.QueryString["gallery_item_id"].ToString() != "")
            {
                Session["gallery_item_id"] = int.Parse(Request.QueryString["gallery_item_id"].ToString());
            }
            else
            {
                if ((Request.QueryString["gallery_id"] != null &&
                   Request.QueryString["gallery_id"].ToString() != "") || (Session["gallery_id"] != null && Session["gallery_id"].ToString() != "0" &&
                   Request.QueryString["auth"] != null &&
                   Request.QueryString["auth"].ToString() == "1"))
                {
                    if (Request.QueryString["gallery_id"] != null && Request.QueryString["gallery_id"].ToString() != "")
                        Session["gallery_id"] = int.Parse(Request.QueryString["gallery_id"].ToString());
                    object oItemID = ODBCWrapper.Utils.GetTableSingleVal("tvp_galleries_items", "ID", "TVP_GALLERY_ID", "=", int.Parse(Session["gallery_id"].ToString()), "tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString());
                    if (oItemID == null || oItemID == DBNull.Value)
                    {
                        InsertNewRecord("tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString(), nGroupID);
                        oItemID = ODBCWrapper.Utils.GetTableSingleVal("tvp_galleries_items", "ID", "TVP_GALLERY_ID", "=", int.Parse(Session["gallery_id"].ToString()), "tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString());
                    }
                    Session["gallery_item_id"] = oItemID;
                }
                else
                    Session["gallery_item_id"] = 0;
            }
            m_sLangMenu = GetLangMenu(nGroupID);
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

    protected string GetLangMenu(Int32 nGroupID)
    {
        if (Session["gallery_item_id"] == null || Session["gallery_item_id"].ToString() == "0" || Session["gallery_item_id"].ToString() == "" ||
            (IsFieldAvailable("HAS_TITLE", "tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString()) == false &&
            IsFieldAvailable("HAS_MAIN_DESCRIPTION", "tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString()) == false &&
            IsFieldAvailable("HAS_SUB_DESCRIPTION", "tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString()) == false &&
            IsFieldAvailable("HAS_TOOLTIP_TEXT", "tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString()) == false))
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
                sTemp += "adm_tvp_galleries_items_new.aspx?gallery_item_id=" + Session["gallery_item_id"].ToString() + "&lang_id=" + nMainLangID.ToString();
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
                        sTemp += "adm_tvp_galleries_items_new.aspx?gallery_item_id=" + Session["gallery_item_id"].ToString() + "&lang_id=" + nLangID.ToString();
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

    static protected bool IsFieldAvailable(string sField, string sCollKey)
    {
        Int32 nTemplateID = 0;
        if (CachingManager.CachingManager.Exists("TemplateID" + HttpContext.Current.Session["gallery_id"].ToString()) == true)
        {
            nTemplateID = int.Parse(CachingManager.CachingManager.GetCachedData("TemplateID" + HttpContext.Current.Session["gallery_id"].ToString()).ToString());
        }
        else
        {
            nTemplateID = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("tvp_galleries", "CHANNEL_TEMPLATE_ID", int.Parse(HttpContext.Current.Session["gallery_id"].ToString()), 10000, sCollKey).ToString());
            CachingManager.CachingManager.SetCachedData("TemplateID" + HttpContext.Current.Session["gallery_id"].ToString(), nTemplateID, 3600, CacheItemPriority.Default, 0, false);
        }
        Int32 nOK = 0;
        DataTable dt = null;
        if (CachingManager.CachingManager.Exists("IsFieldAvailable_" + sCollKey + "_" + nTemplateID.ToString()) == true)
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
                CachingManager.CachingManager.SetCachedData("IsFieldAvailable_" + sCollKey + "_" + nTemplateID.ToString(), selectQuery.Table("query").Copy(), 3600, CacheItemPriority.Default, 0, false);
            }
            selectQuery.Finish();
            selectQuery = null;
        }
        nOK = int.Parse(dt.DefaultView[0].Row[sField].ToString());
        if (nOK == 0)
            return false;
        return true;
    }

    static protected object GetDefaultVal(Int32 nGalleryID , string sField, string sCollKey)
    {
        if (CachingManager.CachingManager.Exists("GetDefaultVal_" + sField + "_" + sCollKey + "_" + nGalleryID.ToString()) == true)
        {
            return CachingManager.CachingManager.GetCachedData("GetDefaultVal_" + sField + "_" + sCollKey + "_" + nGalleryID.ToString());
        }
        Int32 nTemplateID = 0;
        if (CachingManager.CachingManager.Exists("TemplateID" + nGalleryID.ToString()) == true)
        {
            nTemplateID = int.Parse(CachingManager.CachingManager.GetCachedData("TemplateID" + nGalleryID.ToString()).ToString());
        }
        else
        {
            nTemplateID = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("tvp_galleries", "CHANNEL_TEMPLATE_ID", nGalleryID, 10000, sCollKey).ToString());
            CachingManager.CachingManager.SetCachedData("TemplateID" + nGalleryID.ToString(), nTemplateID, 3600, CacheItemPriority.Default, 0, false);
        }
        //Int32 nTemplateID = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("tvp_galleries", "CHANNEL_TEMPLATE_ID", nGalleryID, 10000, sCollKey).ToString());
        object oRet = ODBCWrapper.Utils.GetTableSingleVal("tvp_template_channels_gallery_types", sField, nTemplateID, 10000, sCollKey);
        CachingManager.CachingManager.SetCachedData("GetDefaultVal_" + sField + "_" + sCollKey + "_" + nGalleryID.ToString(), oRet, 3600, CacheItemPriority.Default, 0, false);
        return oRet;
    }

    protected void InsertNewRecord(string sCollKey , Int32 nGroupID)
    {
        ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("tvp_galleries_items");
        insertQuery.SetConnectionKey(sCollKey);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("ORDER_NUM", "=", 1);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("VIRTUAL_NAME", "=", "---");
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("TVP_GALLERY_ID", "=", int.Parse(Session["gallery_id"].ToString()));
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("PAGE_SIZE", "=", GetDefaultVal(int.Parse(Session["gallery_id"].ToString()) , "DEFAULT_PAGE_SIZE" , sCollKey));
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("MAX_RESULT_NUM", "=", GetDefaultVal(int.Parse(Session["gallery_id"].ToString()) , "DEFAULT_MAX_RESULT_NUM" , sCollKey));
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("VIEW_TYPE", "=", GetDefaultVal(int.Parse(Session["gallery_id"].ToString()) , "DEFAULT_VIEW_TYPE" , sCollKey));
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

    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
    }

    public void GetHeader()
    {
        Int32 nGroupID = LoginManager.GetLoginGroupID();
        string sRet = PageUtils.GetPreHeader() + ": Gallery Component ";
        if (Session["gallery_item_id"] != null && Session["gallery_item_id"].ToString() != "" && Session["gallery_item_id"].ToString() != "0")
        {
            sRet += "(" + ODBCWrapper.Utils.GetTableSingleVal("tvp_galleries_items", "VIRTUAL_NAME", int.Parse(Session["gallery_item_id"].ToString()), "tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString()).ToString() + ")";
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

    protected string GetCurrentValue(string sField, string sTable, Int32 nGalleryItemID, Int32 nLangID, string sConnKey)
    {
        string sRet = "";
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery.SetConnectionKey(sConnKey);
        selectQuery += "select " + sField + " from " + sTable + " where ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("tvp_gallery_item_ID", "=", nGalleryItemID);
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
        if (Session["error_msg"] != null && Session["error_msg"].ToString() != "")
        {
            Session["error_msg"] = "";
            return Session["last_page_html"].ToString();
        }
        Int32 nGroupID = LoginManager.GetLoginGroupID();

        object t = null; ;
        if (Session["gallery_item_id"] != null && Session["gallery_item_id"].ToString() != "" && int.Parse(Session["gallery_item_id"].ToString()) != 0)
            t = Session["gallery_item_id"];
        string sCollKey = "tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString();
        string sBack = "";
        Int32 nNumOfItems = 1;
        object o = GetDefaultVal(int.Parse(Session["gallery_id"].ToString()) , "NUM_OF_ITEMS" , sCollKey);
        if (o != null && o != DBNull.Value)
            nNumOfItems = int.Parse(o.ToString());
        if (nNumOfItems == 2)
            sBack = "adm_tvp_galleries.aspx?search_save=1";
        if (nNumOfItems == 3)
            sBack = "adm_tvp_galleries_items.aspx?search_save=1&gallery_id=" + Session["gallery_id"].ToString();
        
        DBRecordWebEditor theRecord = new DBRecordWebEditor("tvp_galleries_items", "adm_table_pager", sBack, "", "ID", t, sBack, "");
        theRecord.SetConnectionKey("tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString());
        string sMainLang = "";
        bool bIsLangMain = (int.Parse(Session["lang_id"].ToString()) == GetMainLang(ref sMainLang));
        //Int32 nNumOfItems = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("tvp_template_channels_gallery_types", "NUM_OF_ITEMS", int.Parse(Session["template_id"].ToString()), "tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString()).ToString());
        if (bIsLangMain == true && nNumOfItems == 3)
        {
            DataRecordShortTextField dr_domain = new DataRecordShortTextField("ltr", true, 60, 128);
            dr_domain.Initialize("Virtual Name", "adm_table_header_nbg", "FormInput", "VIRTUAL_NAME", true);
            theRecord.AddRecord(dr_domain);
        }

        if (IsFieldAvailable("HAS_TITLE", "tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString()) == true)
        {
            DataRecordShortTextField dr_title = new DataRecordShortTextField("ltr", true, 60, 128);
            dr_title.Initialize("Title", "adm_table_header_nbg", "FormInput", "", false);
            if (Session["gallery_item_id"] != null && Session["gallery_item_id"].ToString() != "0")
            {
                dr_title.SetValue(GetCurrentValue("TITLE", "tvp_galleries_items_text", int.Parse(Session["gallery_item_id"].ToString()), int.Parse(Session["lang_id"].ToString()), "tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString()));
            }
            else
                dr_title.SetValue("");
            theRecord.AddRecord(dr_title);
        }

        if (IsFieldAvailable("HAS_MAIN_DESCRIPTION", "tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString()) == true)
        {
            DataRecordLongTextField dr_main_desc = new DataRecordLongTextField("ltr", true, 60, 3);
            dr_main_desc.Initialize("Description", "adm_table_header_nbg", "FormInput", "", false);
            if (Session["gallery_item_id"] != null && Session["gallery_item_id"].ToString() != "0")
            {
                dr_main_desc.SetValue(GetCurrentValue("MAIN_DESCRIPTION", "tvp_galleries_items_text", int.Parse(Session["gallery_item_id"].ToString()), int.Parse(Session["lang_id"].ToString()), "tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString()));
            }
            else
                dr_main_desc.SetValue("");
            theRecord.AddRecord(dr_main_desc);
        }

        if (IsFieldAvailable("HAS_SUB_DESCRIPTION", "tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString()) == true)
        {
            DataRecordLongTextField dr_sub_desc = new DataRecordLongTextField("ltr", true, 60, 3);
            dr_sub_desc.Initialize("Sub Description", "adm_table_header_nbg", "FormInput", "", false);
            if (Session["gallery_item_id"] != null && Session["gallery_item_id"].ToString() != "0")
            {
                dr_sub_desc.SetValue(GetCurrentValue("SUB_DESCRIPTION", "tvp_galleries_items_text", int.Parse(Session["gallery_item_id"].ToString()), int.Parse(Session["lang_id"].ToString()), "tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString()));
            }
            else
                dr_sub_desc.SetValue("");
            theRecord.AddRecord(dr_sub_desc);
        }

        if (IsFieldAvailable("HAS_TOOLTIP_TEXT", "tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString()) == true)
        {
            DataRecordLongTextField dr_tooltip = new DataRecordLongTextField("ltr", true, 60, 3);
            dr_tooltip.Initialize("Tooltip", "adm_table_header_nbg", "FormInput", "", false);
            if (Session["gallery_item_id"] != null && Session["gallery_item_id"].ToString() != "0")
            {
                dr_tooltip.SetValue(GetCurrentValue("TOOLTIP_TEXT", "tvp_galleries_items_text", int.Parse(Session["gallery_item_id"].ToString()), int.Parse(Session["lang_id"].ToString()), "tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString()));
            }
            else
                dr_tooltip.SetValue("");
            theRecord.AddRecord(dr_tooltip);
        }
        if (bIsLangMain == true)
        {
            if (IsFieldAvailable("HAS_LINK", sCollKey) == true)
            {
                DataRecordShortTextField dr_link = new DataRecordShortTextField("ltr", true, 60, 128);
                dr_link.Initialize("Link", "adm_table_header_nbg", "FormInput", "LINK", false);
                theRecord.AddRecord(dr_link);
            }

            if (IsFieldAvailable("HAS_PIC_MAIN", sCollKey) == true)
            {
                DataRecordOnePicBrowserField dr_PIC_MAIN = new DataRecordOnePicBrowserField();
                dr_PIC_MAIN.Initialize("Main Image", "adm_table_header_nbg", "FormInput", "PIC_MAIN", false);
                theRecord.AddRecord(dr_PIC_MAIN);
            }
            else
            {
                DataRecordShortIntField dr_PIC_MAIN = new DataRecordShortIntField(false, 9, 9);
                dr_PIC_MAIN.Initialize("Group", "adm_table_header_nbg", "FormInput", "PIC_MAIN", false);
                dr_PIC_MAIN.SetValue("0");
                theRecord.AddRecord(dr_PIC_MAIN);
            }

            if (IsFieldAvailable("HAS_PIC_SUB", sCollKey) == true)
            {
                DataRecordOnePicBrowserField dr_PIC_SUB = new DataRecordOnePicBrowserField();
                dr_PIC_SUB.Initialize("Sub Image", "adm_table_header_nbg", "FormInput", "PIC_SUB", false);
                theRecord.AddRecord(dr_PIC_SUB);
            }
            else
            {
                DataRecordShortIntField dr_PIC_SUB = new DataRecordShortIntField(false, 9, 9);
                dr_PIC_SUB.Initialize("Group", "adm_table_header_nbg", "FormInput", "PIC_SUB", false);
                dr_PIC_SUB.SetValue("0");
                theRecord.AddRecord(dr_PIC_SUB);
            }

            if (IsFieldAvailable("HAS_SWF", sCollKey) == true)
            {
                DataRecordUploadField dr_swf = new DataRecordUploadField(36, "skins", false);
                dr_swf.Initialize("Flash Object", "adm_table_header_nbg", "FormInput", "SWF", false);
                theRecord.AddRecord(dr_swf);
            }

            if (IsFieldAvailable("HAS_PAGE_SIZE", sCollKey) == true)
            {
                DataRecordShortIntField dr_page_size = new DataRecordShortIntField(true, 9, 9);
                dr_page_size.Initialize("Page Size", "adm_table_header_nbg", "FormInput", "PAGE_SIZE", true);
                theRecord.AddRecord(dr_page_size);
            }
            else
            {
                DataRecordShortIntField dr_page_size = new DataRecordShortIntField(false, 9, 9);
                dr_page_size.Initialize("Page Size", "adm_table_header_nbg", "FormInput", "PAGE_SIZE", false);
                dr_page_size.SetValue(GetDefaultVal(int.Parse(Session["gallery_id"].ToString()), "DEFAULT_PAGE_SIZE", sCollKey).ToString());
                theRecord.AddRecord(dr_page_size);
            }

            if (IsFieldAvailable("HAS_MAX_RESULT_NUM", sCollKey) == true)
            {
                DataRecordShortIntField dr_max_result_num = new DataRecordShortIntField(true, 9, 9);
                dr_max_result_num.Initialize("Maximum Results", "adm_table_header_nbg", "FormInput", "MAX_RESULT_NUM", true);
                theRecord.AddRecord(dr_max_result_num);
            }
            else
            {
                DataRecordShortIntField dr_max_result_num = new DataRecordShortIntField(false, 9, 9);
                dr_max_result_num.Initialize("Maximum Results", "adm_table_header_nbg", "FormInput", "MAX_RESULT_NUM", false);
                dr_max_result_num.SetValue(GetDefaultVal(int.Parse(Session["gallery_id"].ToString()), "DEFAULT_MAX_RESULT_NUM", sCollKey).ToString());
                theRecord.AddRecord(dr_max_result_num);
            }

            if (IsFieldAvailable("HAS_DEFAULT_VIEW_TYPE", sCollKey) == true)
            {
                DataRecordDropDownField dr_VIEW_TYPE = new DataRecordDropDownField("lu_gallery_view_types", "DESCRIPTION", "id", "", null, 60, false);
                dr_VIEW_TYPE.Initialize("Default View Type", "adm_table_header_nbg", "FormInput", "VIEW_TYPE", true);
                theRecord.AddRecord(dr_VIEW_TYPE);
            }
            else
            {
                DataRecordShortIntField dr_VIEW_TYPE = new DataRecordShortIntField(false, 9, 9);
                dr_VIEW_TYPE.Initialize("Default View Type", "adm_table_header_nbg", "FormInput", "VIEW_TYPE", false);
                dr_VIEW_TYPE.SetValue(GetDefaultVal(int.Parse(Session["gallery_id"].ToString()), "DEFAULT_VIEW_TYPE", sCollKey).ToString());
                theRecord.AddRecord(dr_VIEW_TYPE);
            }

            if (IsFieldAvailable("HAS_PIC_SIZE", sCollKey) == true)
            {
                DataRecordDropDownField dr_Pic_Size = new DataRecordDropDownField("lu_gallery_pic_sizes", "DESCRIPTION", "id", "", null, 60, false);
                dr_Pic_Size.Initialize("Pic Size", "adm_table_header_nbg", "FormInput", "PIC_SIZE", true);
                theRecord.AddRecord(dr_Pic_Size);
            }
            else
            {
                DataRecordShortIntField dr_Pic_Size = new DataRecordShortIntField(false, 9, 9);
                dr_Pic_Size.Initialize("Pic Size", "adm_table_header_nbg", "FormInput", "PIC_SIZE", false);
                dr_Pic_Size.SetValue(GetDefaultVal(int.Parse(Session["gallery_id"].ToString()), "DEFAULT_PIC_SIZE", sCollKey).ToString());
                theRecord.AddRecord(dr_Pic_Size);
            }

            if (IsFieldAvailable("HAS_SUB_PIC_SIZE", sCollKey) == true)
            {
                DataRecordDropDownField dr_SUB_PIC_SIZE = new DataRecordDropDownField("lu_gallery_pic_sizes", "DESCRIPTION", "id", "", null, 60, false);
                dr_SUB_PIC_SIZE.Initialize("Sub Pic Size", "adm_table_header_nbg", "FormInput", "SUB_PIC_SIZE", true);
                theRecord.AddRecord(dr_SUB_PIC_SIZE);
            }
            else
            {
                DataRecordShortIntField dr_SUB_PIC_SIZE = new DataRecordShortIntField(false, 9, 9);
                dr_SUB_PIC_SIZE.Initialize("Sub Pic Size", "adm_table_header_nbg", "FormInput", "SUB_PIC_SIZE", false);
                dr_SUB_PIC_SIZE.SetValue(GetDefaultVal(int.Parse(Session["gallery_id"].ToString()), "DEFAULT_SUB_PIC_SIZE", sCollKey).ToString());
                theRecord.AddRecord(dr_SUB_PIC_SIZE);
            }

            string sPUN = "";
            string sPPass = "";

            if (IsFieldAvailable("HAS_CHANNEL_MAIN", sCollKey) == true)
            {
                Int32 nTVAID = GetTVMAccountDetails(int.Parse(Session["gallery_item_id"].ToString()), true, ref sPUN, ref sPPass, nGroupID);
                DataRecordTVMChannelCategoryField dr_tvm_main = new DataRecordTVMChannelCategoryField(false, sPUN, sPPass);
                dr_tvm_main.Initialize("Channel", "adm_table_header_nbg", "FormInput", "CHANNEL_MAIN", false);
                dr_tvm_main.SetFrameName("main_chooser");
                dr_tvm_main.SetDefault(0);
                theRecord.AddRecord(dr_tvm_main);

                DataRecordDropDownField dr_MAIN_TVM_ACCOUNT_ID = new DataRecordDropDownField("tvp_tvm_accounts", "NAME", "id", "", null, 60, false);
                dr_MAIN_TVM_ACCOUNT_ID.SetWhereString("status<>2 and group_id=" + nGroupID.ToString());
                dr_MAIN_TVM_ACCOUNT_ID.Initialize("TVM Account", "adm_table_header_nbg", "FormInput", "MAIN_TVM_ACCOUNT_ID", true);
                dr_MAIN_TVM_ACCOUNT_ID.SetOnString(" onchange=changeMainChooser(this.options[this.selectedIndex].value) ");
                dr_MAIN_TVM_ACCOUNT_ID.SetValue(nTVAID.ToString());
                theRecord.AddRecord(dr_MAIN_TVM_ACCOUNT_ID);
            }
            else
            {
                DataRecordShortIntField dr_tvm_main = new DataRecordShortIntField(false, 9, 9);
                dr_tvm_main.Initialize("Group", "adm_table_header_nbg", "FormInput", "CHANNEL_MAIN", false);
                dr_tvm_main.SetValue("0");
                theRecord.AddRecord(dr_tvm_main);

                DataRecordShortIntField dr_MAIN_TVM_ACCOUNT_ID = new DataRecordShortIntField(false, 9, 9);
                dr_MAIN_TVM_ACCOUNT_ID.Initialize("Group", "adm_table_header_nbg", "FormInput", "MAIN_TVM_ACCOUNT_ID", false);
                dr_MAIN_TVM_ACCOUNT_ID.SetValue(GetDefaultVal(int.Parse(Session["gallery_id"].ToString()), "DEFAULT_TVM_FOR_CHANNEL_MAIN", sCollKey).ToString());
                theRecord.AddRecord(dr_MAIN_TVM_ACCOUNT_ID);
            }


            if (IsFieldAvailable("HAS_CHANNEL_SUB", sCollKey) == true)
            {
                Int32 nTVAID = GetTVMAccountDetails(int.Parse(Session["gallery_item_id"].ToString()), false, ref sPUN, ref sPPass, nGroupID);
                DataRecordTVMChannelCategoryField dr_tvm_sub = new DataRecordTVMChannelCategoryField(false, sPUN, sPPass);
                dr_tvm_sub.SetFrameName("sub_chooser");
                dr_tvm_sub.Initialize("Channel Sub", "adm_table_header_nbg", "FormInput", "CHANNEL_SUB", false);
                dr_tvm_sub.SetDefault(0);
                theRecord.AddRecord(dr_tvm_sub);

                DataRecordDropDownField dr_SUB_TVM_ACCOUNT_ID = new DataRecordDropDownField("tvp_tvm_accounts", "NAME", "id", "", null, 60, false);
                dr_SUB_TVM_ACCOUNT_ID.SetWhereString("status<>2 and group_id=" + nGroupID.ToString());
                dr_SUB_TVM_ACCOUNT_ID.Initialize("Sub TVM Account", "adm_table_header_nbg", "FormInput", "SUB_TVM_ACCOUNT_ID", true);
                dr_SUB_TVM_ACCOUNT_ID.SetOnString(" onchange=changeSubChooser(this.options[this.selectedIndex].value) ");
                dr_SUB_TVM_ACCOUNT_ID.SetValue(nTVAID.ToString());
                theRecord.AddRecord(dr_SUB_TVM_ACCOUNT_ID);
            }
            else
            {
                DataRecordShortIntField dr_tvm_sub = new DataRecordShortIntField(false, 9, 9);
                dr_tvm_sub.Initialize("Group", "adm_table_header_nbg", "FormInput", "CHANNEL_SUB", false);
                dr_tvm_sub.SetValue("0");
                theRecord.AddRecord(dr_tvm_sub);

                DataRecordShortIntField dr_SUB_TVM_ACCOUNT_ID = new DataRecordShortIntField(false, 9, 9);
                dr_SUB_TVM_ACCOUNT_ID.Initialize("Group", "adm_table_header_nbg", "FormInput", "SUB_TVM_ACCOUNT_ID", false);
                dr_SUB_TVM_ACCOUNT_ID.SetValue(GetDefaultVal(int.Parse(Session["gallery_id"].ToString()), "DEFAULT_TVM_FOR_CHANNEL_SUB", sCollKey).ToString());
                theRecord.AddRecord(dr_SUB_TVM_ACCOUNT_ID);
            }

            if (IsFieldAvailable("HAS_NUMERIC", sCollKey) == true)
            {
                DataRecordShortIntField dr_HAS_NUMERIC = new DataRecordShortIntField(true, 9, 9);
                dr_HAS_NUMERIC.Initialize(GetDefaultVal(int.Parse(Session["gallery_id"].ToString()), "NUMERIC_TITLE", sCollKey).ToString(), "adm_table_header_nbg", "FormInput", "NUMERIC", false);
                theRecord.AddRecord(dr_HAS_NUMERIC);
            }

            if (IsFieldAvailable("HAS_BOOLEAN", sCollKey) == true)
            {
                DataRecordBoolField dr_HAS_BOOLEAN = new DataRecordBoolField(true);
                dr_HAS_BOOLEAN.Initialize(GetDefaultVal(int.Parse(Session["gallery_id"].ToString()), "BOOLEAN_TITLE", sCollKey).ToString(), "adm_table_header_nbg", "FormInput", "BOOLEAN", false);
                theRecord.AddRecord(dr_HAS_BOOLEAN);
            }

            if (IsFieldAvailable("HAS_TIME", sCollKey) == true)
            {
                DataRecordTimeField dr_statrt_time = new DataRecordTimeField();
                dr_statrt_time.Initialize("Start time", "adm_table_header_nbg", "FormInput", "PLAY_START_TIME", false);
                theRecord.AddRecord(dr_statrt_time);
            }
        }

        DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
        dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", false);
        dr_groups.SetValue(LoginManager.GetLoginGroupID().ToString());
        theRecord.AddRecord(dr_groups);

        DataRecordShortIntField dr_gallery_id = new DataRecordShortIntField(false, 9, 9);
        dr_gallery_id.Initialize("Group", "adm_table_header_nbg", "FormInput", "TVP_GALLERY_ID", false);
        dr_gallery_id.SetValue(Session["gallery_id"].ToString());
        theRecord.AddRecord(dr_gallery_id);

        string sTable = theRecord.GetTableHTML("adm_tvp_galleries_items_new.aspx?submited=1");

        return sTable;
    }

    static protected Int32 GetTVMAccountDetails(Int32 nID , bool bIsMain , ref string sPUN , ref string sPPass , Int32 nGroupID)
    {
        if (CachingManager.CachingManager.Exists("GetTVMAccountDetails_" + nID.ToString() + "_" + bIsMain.ToString() + "_" + nGroupID.ToString()) == true)
        {
            string sAll = CachingManager.CachingManager.GetCachedData("GetTVMAccountDetails_" + nID.ToString() + "_" + bIsMain.ToString() + "_" + nGroupID.ToString()).ToString();
            string[] sSep = {"|"};
            string[] sAlls = sAll.Split(sSep, StringSplitOptions.RemoveEmptyEntries);
            sPUN = sAlls[0];
            sPPass = sAlls[1];
            return int.Parse(sAlls[2]);
        }
        Int32 nRet = 0;
        string sConnKey = "tvp_connection_" + nGroupID.ToString() + "_" + HttpContext.Current.Session["platform"].ToString();
        string sTVMAccountRefField = "tgi.";
        string sDefTVMAccountField = "";
        string sCache = "";
        if (bIsMain == true)
        {
            sTVMAccountRefField += "MAIN_TVM_ACCOUNT_ID";
            sDefTVMAccountField = "DEFAULT_TVM_FOR_CHANNEL_MAIN";
        }
        else
        {
            sTVMAccountRefField += "SUB_TVM_ACCOUNT_ID";
            sDefTVMAccountField = "DEFAULT_TVM_FOR_CHANNEL_SUB";
        }
        if (nID != 0)
        {
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetConnectionKey(sConnKey);
            selectQuery += " select tta.* from tvp_tvm_accounts tta,tvp_galleries_items tgi where tta.is_active=1 and tta.status=1 and tta.id=" + sTVMAccountRefField;
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("tgi.id", "=", nID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    nRet = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
                    sPUN = selectQuery.Table("query").DefaultView[0].Row["PLAYER_UN"].ToString();
                    sPPass = selectQuery.Table("query").DefaultView[0].Row["PLAYER_PASS"].ToString();
                    sCache = sPUN + "|";
                    sCache += sPPass + "|";
                    sCache += nRet.ToString();
                }
            }
            selectQuery.Finish();
            selectQuery = null;
        }
        else
        {
            object oDefTVMAccount = GetDefaultVal(int.Parse(HttpContext.Current.Session["gallery_id"].ToString()), sDefTVMAccountField, sConnKey);
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetConnectionKey(sConnKey);
            selectQuery += " select tta.* from tvp_tvm_accounts tta where tta.is_active=1 and tta.status=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("tta.id", "=", int.Parse(oDefTVMAccount.ToString()));
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    nRet = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
                    sPUN = selectQuery.Table("query").DefaultView[0].Row["PLAYER_UN"].ToString();
                    sPPass = selectQuery.Table("query").DefaultView[0].Row["PLAYER_PASS"].ToString();
                    sCache = sPUN + "|";
                    sCache += sPPass + "|";
                    sCache += nRet.ToString();
                }
            }
            selectQuery.Finish();
            selectQuery = null;
        }
        if (sCache != "")
            CachingManager.CachingManager.SetCachedData("GetTVMAccountDetails_" + nID.ToString() + "_" + bIsMain.ToString() + "_" + nGroupID.ToString(), sCache, 3600, CacheItemPriority.Default, 0, false);
        return nRet;
    }
}
