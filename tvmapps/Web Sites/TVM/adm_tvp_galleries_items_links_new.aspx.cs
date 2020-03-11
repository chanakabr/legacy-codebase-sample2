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

public partial class adm_tvp_galleries_items_links_new : System.Web.UI.Page
{
    protected string m_sMenu;
    protected string m_sSubMenu;
    protected string m_sLangMenu;

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
            string sConnKey = "tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString();
            if (Request.QueryString["submited"] != null && Request.QueryString["submited"].ToString() == "1")
            {
                Int32 nID = DBManipulator.DoTheWork(sConnKey);
                Int32 nLangID = int.Parse(Session["lang_id"].ToString());
                Int32 nButtonTextID = GetButtonTextID(nID , nLangID , nGroupID);
                string sValue = "";
                string sLang = "";
                bool bIsMainLang = int.Parse(Session["lang_id"].ToString()) == GetMainLang(ref sLang);
                if (bIsMainLang == true)
                    sValue = Request.Form["2_val"].ToString();
                else
                    sValue = Request.Form["0_val"].ToString();
                if (nButtonTextID == 0)
                {
                    ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("tvp_galleries_buttons_text");
                    insertQuery.SetConnectionKey("tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString());
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("tvp_gallery_button_ID", "=", nID);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("VALUE", "=", sValue);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("LANGUAGE_ID", "=", nLangID);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATER_ID", "=", LoginManager.GetLoginID());
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATE_DATE", "=", DateTime.UtcNow);
                    insertQuery.Execute();
                    insertQuery.Finish();
                    insertQuery = null;
                }
                else
                {
                    ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("tvp_galleries_buttons_text");
                    updateQuery.SetConnectionKey("tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString());
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("VALUE", "=", sValue);
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATER_ID", "=", LoginManager.GetLoginID());
                    updateQuery += " where ";
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nButtonTextID);
                    updateQuery.Execute();
                    updateQuery.Finish();
                    updateQuery = null;
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

            if (Request.QueryString["link_id"] != null &&
               Request.QueryString["link_id"].ToString() != "")
                Session["link_id"] = int.Parse(Request.QueryString["link_id"].ToString());
            else
                Session["link_id"] = 0;

            if (Request.QueryString["link_type"] != null &&
               Request.QueryString["link_type"].ToString() != "")
            {
                Session["link_type"] = int.Parse(Request.QueryString["link_type"].ToString());
                Int32 nID = int.Parse(Session["link_type"].ToString());
            }
            else
            {
                if (Session["link_type"] == null || Session["link_type"].ToString() == "" || Session["link_type"].ToString() == "0")
                {
                    LoginManager.LogoutFromSite("login.html");
                    return;
                }
            }

            m_sLangMenu = GetLangMenu(nGroupID);
        }

    }

    static protected Int32 GetButtonTextID(Int32 nButtonID , Int32 nLangID , Int32 nGroupID)
    {
        Int32 nButtonTextID = 0;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery.SetConnectionKey("tvp_connection_" + nGroupID.ToString() + "_" + HttpContext.Current.Session["platform"].ToString());
        selectQuery += "select id from tvp_galleries_buttons_text where ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("tvp_gallery_button_ID", "=", nButtonID);
        selectQuery += "and";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("LANGUAGE_ID", "=", nLangID);
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
            {
                nButtonTextID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
            }
        }
        selectQuery.Finish();
        selectQuery = null;
        return nButtonTextID;
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

    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
    }

    public void GetHeader()
    {
        Int32 nGroupID = LoginManager.GetLoginGroupID();
        string sConn = "tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString();
        
        string sRet = PageUtils.GetPreHeader() + ": Gallery ";
        if (Session["link_type"].ToString() == "1")
            sRet += "Tabs ";
        if (Session["link_type"].ToString() == "2")
            sRet += "Links ";
        if (Session["link_id"] != null && Session["link_id"].ToString() != "" && Session["link_id"].ToString() != "0")
        {
            object oName = ODBCWrapper.Utils.GetTableSingleVal("tvp_galleries_buttons", "VIRTUAL_NAME", int.Parse(Session["link_id"].ToString()), sConn);
            string sName = "";
            if (oName != DBNull.Value)
                sName = oName.ToString();
            sRet += "(" + sName + ") - Edit";
        }
        else
            sRet += " - New";
        Response.Write(sRet);
    }

    protected void GetSubMenu()
    {
        Response.Write(m_sSubMenu);
    }

    protected void GetLangMenu()
    {
        Response.Write(m_sLangMenu);
    }

    protected string GetLangMenu(Int32 nGroupID)
    {
        try
        {
            string sTemp = "";
            string sMainLang = "";
            Int32 nMainLangID = GetMainLang(ref sMainLang);

            string sOnOff = "on";
            if (nMainLangID != int.Parse(Session["lang_id"].ToString()))
                sOnOff = "off";
            sTemp += "<li><a class=\"" + sOnOff + "\" href=\"";
            if (nMainLangID != int.Parse(Session["lang_id"].ToString()))
                sTemp += "adm_tvp_galleries_items_links_new.aspx?link_id=" + Session["link_id"].ToString() + "&lang_id=" + nMainLangID.ToString() + "&link_type=" + Session["link_type"].ToString();
            else
                sTemp += "void(0);";
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
                        sTemp += "adm_tvp_galleries_items_links_new.aspx?link_id=" + Session["link_id"].ToString() + "&link_type=" + Session["link_type"].ToString() + "&lang_id=" + nLangID.ToString();
                    else
                        sTemp += "void(0);";
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

    protected string GetCurrentValue(string sField, string sTable, Int32 nGalleryID, Int32 nLangID, string sConnKey)
    {
        string sRet = "";
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery.SetConnectionKey(sConnKey);
        selectQuery += "select " + sField + " from " + sTable + " where ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("tvp_gallery_button_ID", "=", nGalleryID);
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
        if (Session["link_id"] != null && Session["link_id"].ToString() != "" && int.Parse(Session["link_id"].ToString()) != 0 )
            t = int.Parse(Session["link_id"].ToString());

        string sBack = "adm_tvp_galleries_items_links.aspx?search_save=1";
        DBRecordWebEditor theRecord = new DBRecordWebEditor("tvp_galleries_buttons", "adm_table_pager", sBack, "", "ID", t, sBack, "");
        theRecord.SetConnectionKey("tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString());
        string sLang = "";
        bool bIsMainLang = int.Parse(Session["lang_id"].ToString()) == GetMainLang(ref sLang);
        if (bIsMainLang == true)
        {
            DataRecordShortTextField dr_vn = new DataRecordShortTextField("ltr", true, 60, 128);
            dr_vn.Initialize("Virtual Name", "adm_table_header_nbg", "FormInput", "VIRTUAL_NAME", true);
            theRecord.AddRecord(dr_vn);

            DataRecordShortTextField dr_link = new DataRecordShortTextField("ltr", true, 60, 128);
            dr_link.Initialize("Link", "adm_table_header_nbg", "FormInput", "LINK", true);
            theRecord.AddRecord(dr_link);
        }

        DataRecordShortTextField dr_title = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_title.Initialize("Button Text", "adm_table_header_nbg", "FormInput", "", false);
        if (Session["gallery_id"] != null && Session["gallery_id"].ToString() != "0")
        {
            dr_title.SetValue(GetCurrentValue("VALUE", "tvp_galleries_buttons_text", int.Parse(Session["link_id"].ToString()), int.Parse(Session["lang_id"].ToString()), "tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString()));
        }
        else
            dr_title.SetValue("");
        theRecord.AddRecord(dr_title);

        DataRecordShortIntField dr_gallery_id = new DataRecordShortIntField(false, 9, 9);
        dr_gallery_id.Initialize("Gallery ID", "adm_table_header_nbg", "FormInput", "TVP_GALLERY_ID", false);
        dr_gallery_id.SetValue(Session["gallery_id"].ToString());
        theRecord.AddRecord(dr_gallery_id);

        DataRecordShortIntField dr_button_type = new DataRecordShortIntField(false, 9, 9);
        dr_button_type.Initialize("Gallery ID", "adm_table_header_nbg", "FormInput", "BUTTON_TYPE", false);
        dr_button_type.SetValue(Session["link_type"].ToString());
        theRecord.AddRecord(dr_button_type);

        DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
        dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", false);
        dr_groups.SetValue(LoginManager.GetLoginGroupID().ToString());
        theRecord.AddRecord(dr_groups);

        string sTable = theRecord.GetTableHTML("adm_tvp_galleries_items_links_new.aspx?submited=1");

        return sTable;
    }
}
