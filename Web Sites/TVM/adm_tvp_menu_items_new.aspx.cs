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

public partial class adm_tvp_menu_items_new : System.Web.UI.Page
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
                string sLink = Request.Form["4_val"].ToString();
                sLink = sLink.Replace("''", "\"");

                string sType = Request.Form["4_type"].ToString();
                int nNoFollow = int.Parse(Request.Form["5_val"].ToString());
                Int32 nID = DBManipulator.DoTheWork(sConnKey);
                Int32 nPARENT_MENU_ITEM_ID = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("tvp_menu_items_texts", "MENU_ITEM_ID", nID, sConnKey).ToString());
                if (nPARENT_MENU_ITEM_ID == 0)
                {
                    ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("tvp_menu_items");
                    insertQuery.SetConnectionKey(sConnKey);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("MENU_ID", "=", int.Parse(Session["menu_id"].ToString()));
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("PARENT_MENU_ITEM_ID", "=", int.Parse(Session["menu_item_parent_id"].ToString()));
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
                    insertQuery.Execute();
                    insertQuery.Finish();
                    insertQuery = null;

                    ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                    selectQuery.SetConnectionKey(sConnKey);
                    selectQuery += "select id from tvp_menu_items where ";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MENU_ID", "=", int.Parse(Session["menu_id"].ToString()));
                    selectQuery += " and";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("PARENT_MENU_ITEM_ID", "=", int.Parse(Session["menu_item_parent_id"].ToString()));
                    selectQuery += " and";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
                    selectQuery += " order by id desc ";
                    if (selectQuery.Execute("query", true) != null)
                    {
                        Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                        if (nCount > 0)
                            nPARENT_MENU_ITEM_ID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
                    }
                    selectQuery.Finish();
                    selectQuery = null;
                }
                if (nPARENT_MENU_ITEM_ID != 0)
                {
                    ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("tvp_menu_items_texts");
                    updateQuery.SetConnectionKey(sConnKey);
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("MENU_ITEM_ID", "=", nPARENT_MENU_ITEM_ID);
                    //updateQuery += ODBCWrapper.Parameter.NEW_PARAM("LINK", "=", sLink);
                    updateQuery += " where ";
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nID);
                    updateQuery.Execute();
                    updateQuery.Finish();
                    updateQuery = null;

                    ODBCWrapper.UpdateQuery updateQuery1 = new ODBCWrapper.UpdateQuery("tvp_menu_items");
                    updateQuery1.SetConnectionKey(sConnKey);
                    updateQuery1 += ODBCWrapper.Parameter.NEW_PARAM("LINK", "=", sLink);
                    updateQuery1 += ODBCWrapper.Parameter.NEW_PARAM("HAS_NO_FOLLOW", "=", nNoFollow);
                    updateQuery1 += " where ";
                    updateQuery1 += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nPARENT_MENU_ITEM_ID);
                    updateQuery1.Execute();
                    updateQuery1.Finish();
                    updateQuery1 = null;

                }
                return;
            }
            string sMainLang = ""; 
            m_sMenu = TVinciShared.Menu.GetMainMenu(14, true, ref nMenuID, "adm_tvp_menu.aspx");
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, true);
            
            if (Request.QueryString["lang_id"] != null &&
               Request.QueryString["lang_id"].ToString() != "")
                Session["lang_id"] = Request.QueryString["lang_id"].ToString();
            else
                Session["lang_id"] = GetMainLang(ref sMainLang);

            if (Request.QueryString["tvp_menu_item_id"] != null &&
               Request.QueryString["tvp_menu_item_id"].ToString() != "")
            {
                Session["tvp_menu_item_id"] = int.Parse(Request.QueryString["tvp_menu_item_id"].ToString());
                Int32 nID = int.Parse(Session["tvp_menu_item_id"].ToString());
                Session["menu_id"] = ODBCWrapper.Utils.GetTableSingleVal("tvp_menu_items", "MENU_ID", nID, sConnKey).ToString();
                Session["menu_item_parent_id"] = ODBCWrapper.Utils.GetTableSingleVal("tvp_menu_items", "PARENT_MENU_ITEM_ID", nID, sConnKey).ToString();
            }
            else
            {
                if (Session["menu_id"] == null || Session["menu_id"].ToString() == "" || Session["menu_id"].ToString() == "0")
                {
                    LoginManager.LogoutFromSite("login.html");
                    return;
                }

                if (Session["menu_item_parent_id"] == null)
                    Session["menu_item_parent_id"] = 0;
                Session["tvp_menu_item_id"] = "0";
            }

            m_sLangMenu = GetLangMenu(nGroupID);

            
        }

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
        object oMenuName = ODBCWrapper.Utils.GetTableSingleVal("tvp_menu", "name", int.Parse(Session["menu_id"].ToString()), sConn);
        string sMenuName = "";
        if (oMenuName != DBNull.Value)
            sMenuName = oMenuName.ToString();
        string sRet = PageUtils.GetPreHeader() + ": Menu item ";
        if (Session["tvp_menu_item_id"] != null && Session["tvp_menu_item_id"].ToString() != "" && Session["tvp_menu_item_id"].ToString() != "0")
            sRet += " - Edit";
        else
            sRet += " - New";
        sRet += " : " + GetWhereAmIStr(sMenuName);
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
            Int32 nCount = 0;
            string sMainLang = "";
            Int32 nMainLangID = GetMainLang(ref sMainLang);

            string sOnOff = "on";
            if (nMainLangID != int.Parse(Session["lang_id"].ToString()))
                sOnOff = "off";
            sTemp += "<li><a class=\"" + sOnOff + "\" href=\"";
            if (nMainLangID != int.Parse(Session["lang_id"].ToString()))
                sTemp += "adm_tvp_menu_items_new.aspx?tvp_menu_item_id=" + Session["tvp_menu_item_id"].ToString() + "&lang_id=" + nMainLangID.ToString();
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
                        sTemp += "adm_tvp_menu_items_new.aspx?tvp_menu_item_id=" + Session["tvp_menu_item_id"].ToString() + "&lang_id=" + nLangID.ToString();
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

    public string GetPageContent(string sOrderBy, string sPageNum)
    {
        if (Session["error_msg"] != null && Session["error_msg"].ToString() != "")
        {
            Session["error_msg"] = "";
            return Session["last_page_html"].ToString();
        }
        Int32 nGroupID = LoginManager.GetLoginGroupID();

        object t = null; ;
        if (Session["tvp_menu_item_id"] != null && Session["tvp_menu_item_id"].ToString() != "" && int.Parse(Session["tvp_menu_item_id"].ToString()) != 0 &&
            Session["lang_id"] != null && Session["lang_id"].ToString() != "" && int.Parse(Session["lang_id"].ToString()) != 0)
        {
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetConnectionKey("tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString());
            selectQuery += "select id from tvp_menu_items_texts where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("language_id", "=", int.Parse(Session["lang_id"].ToString()));
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MENU_ITEM_ID", "=", int.Parse(Session["tvp_menu_item_id"].ToString()));
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCO = selectQuery.Table("query").DefaultView.Count;
                if (nCO > 0)
                {
                    t = selectQuery.Table("query").DefaultView[0].Row["id"];
                }
            }
            selectQuery.Finish();
            selectQuery = null;
        }

        string sBack = "adm_tvp_menu_items.aspx?search_save=1&menu_item_parent_id=" + Session["menu_item_parent_id"].ToString() + "&menu_id=" + Session["menu_id"].ToString();
        DBRecordWebEditor theRecord = new DBRecordWebEditor("tvp_menu_items_texts", "adm_table_pager", sBack, "", "ID", t, sBack, "");
        theRecord.SetConnectionKey("tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString());

        DataRecordShortTextField dr_text = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_text.Initialize("Title", "adm_table_header_nbg", "FormInput", "TITLE", true);
        theRecord.AddRecord(dr_text);

        DataRecordShortIntField dr_lang_id = new DataRecordShortIntField(false, 9, 9);
        dr_lang_id.Initialize("Lang ID", "adm_table_header_nbg", "FormInput", "LANGUAGE_ID", false);
        dr_lang_id.SetValue(Session["lang_id"].ToString());
        theRecord.AddRecord(dr_lang_id);

        DataRecordShortIntField dr_parent_menu_item_id = new DataRecordShortIntField(false, 9, 9);
        dr_parent_menu_item_id.Initialize("Menu Item ID", "adm_table_header_nbg", "FormInput", "MENU_ITEM_ID", false);
        dr_parent_menu_item_id.SetValue(Session["tvp_menu_item_id"].ToString());
        theRecord.AddRecord(dr_parent_menu_item_id);

        DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
        dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", false);
        dr_groups.SetValue(LoginManager.GetLoginGroupID().ToString());
        theRecord.AddRecord(dr_groups);
         
        DataRecordShortTextField dr_link = new DataRecordShortTextField("ltr", true, 100, 512);
        dr_link.Initialize("Link", "adm_table_header_nbg", "FormInput", "", true);
        if (Session["tvp_menu_item_id"] != null && Session["tvp_menu_item_id"].ToString() != "0")
        {
            dr_link.SetValue(ODBCWrapper.Utils.GetTableSingleVal("tvp_menu_items", "link", int.Parse(Session["tvp_menu_item_id"].ToString()), "tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString()).ToString());
        }
        theRecord.AddRecord(dr_link);

        DataRecordBoolField dr_IS_NO_FOLLOW = new DataRecordBoolField(true);
        dr_IS_NO_FOLLOW.Initialize("No Follow", "adm_table_header_nbg", "FormInput", "", false);
        if (Session["tvp_menu_item_id"] != null && Session["tvp_menu_item_id"].ToString() != "0")
        {
            dr_IS_NO_FOLLOW.SetValue(ODBCWrapper.Utils.GetTableSingleVal("tvp_menu_items", "HAS_NO_FOLLOW", int.Parse(Session["tvp_menu_item_id"].ToString()), "tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString()).ToString());
        }
        theRecord.AddRecord(dr_IS_NO_FOLLOW);

        string sTable = theRecord.GetTableHTML("adm_tvp_menu_items_new.aspx?submited=1");

        return sTable;
    }

    protected string GetWhereAmIStr(string sMenuName)
    {
        Int32 nParentItemID = 0;
        if (Session["menu_item_parent_id"] != null && Session["menu_item_parent_id"].ToString() != "" && Session["menu_item_parent_id"].ToString() != "0")
            nParentItemID = int.Parse(Session["menu_item_parent_id"].ToString());

        string sRet = "";
        bool bFirst = true;
        Int32 nLast = 0;
        nLast = 0;
        string sMainLang = "";
        Int32 nMainLang = GetMainLang(ref sMainLang);
        while (nParentItemID != nLast)
        {
            Int32 nGroupID = LoginManager.GetLoginGroupID();
            string sConn = "tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString();
            Int32 nParentID = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("tvp_menu_items", "PARENT_MENU_ITEM_ID", nParentItemID, sConn).ToString());
            string sHeader = "-";
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetConnectionKey(sConn);
            selectQuery += "select TITLE from tvp_menu_items_texts where is_active=1 and status=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("LANGUAGE_ID", "=", nMainLang);
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MENU_ITEM_ID", "=", nParentItemID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    object oHeader = selectQuery.Table("query").DefaultView[0].Row["TITLE"];
                    if (oHeader != null && oHeader != DBNull.Value)
                        sHeader = oHeader.ToString();
                }
            }
            selectQuery.Finish();
            selectQuery = null;

            sRet = "<span style=\"cursor:pointer;\" onclick=\"document.location.href='adm_tvp_menu_items.aspx?parent_group_id=" + nParentID.ToString() + "'menu_id=" + Session["menu_id"].ToString() + "&menu_item_parent_id=" + nParentID.ToString() + "';\">" + sHeader + " </span><span class=\"arrow\">&raquo; </span>" + sRet;
            bFirst = false;
            nParentItemID = nParentID;
        }

        sRet = "<span style=\"cursor:pointer;\" onclick=\"document.location.href='adm_tvp_menu.aspx?menu_id=" + Session["menu_id"].ToString() + "';\">Menu: " + sMenuName + " </span><span class=\"arrow\">&raquo; </span>" + sRet;
        return sRet;

    }
}
