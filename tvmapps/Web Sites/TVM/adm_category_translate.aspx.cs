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

public partial class adm_category_translate : System.Web.UI.Page
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
        Int32 nOwnerGroupID = 0;
        if (!IsPostBack)
        {
            if (Request.QueryString["submited"] != null && Request.QueryString["submited"].ToString() == "1")
            {
                DBManipulator.DoTheWork();
                return;
            }
            m_sMenu = TVinciShared.Menu.GetMainMenu(6, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, true);
            
            if (Request.QueryString["category_id"] != null &&
                Request.QueryString["category_id"].ToString() != "")
            {
                Session["category_id"] = int.Parse(Request.QueryString["category_id"].ToString());
                nOwnerGroupID = int.Parse(PageUtils.GetTableSingleVal("categories", "group_id", int.Parse(Session["category_id"].ToString())).ToString());
                Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();
                if (nLogedInGroupID != nOwnerGroupID && PageUtils.IsTvinciUser() == false)
                {
                    LoginManager.LogoutFromSite("login.html");
                    return;
                }
            }
            else
            {
                LoginManager.LogoutFromSite("login.html");
                return;
            }

            if (Request.QueryString["lang_id"] != null &&
                Request.QueryString["lang_id"].ToString() != "")
            {
                Session["lang_id"] = int.Parse(Request.QueryString["lang_id"].ToString());
                //nOwnerGroupID = LoginManager.GetLoginGroupID();
                Int32 nCO = 0;
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select count(*) as co from group_extra_languages where status=1 and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nOwnerGroupID);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("LANGUAGE_ID", "=", int.Parse(Session["lang_id"].ToString()));
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                        nCO = int.Parse(selectQuery.Table("query").DefaultView[0].Row["co"].ToString());
                }
                selectQuery.Finish();
                selectQuery = null;
                if (nCO == 0)
                {
                    LoginManager.LogoutFromSite("login.html");
                    return;
                }
                m_sLangMenu = GetLangMenu(nOwnerGroupID);
            }
            else
            {
                LoginManager.LogoutFromSite("login.html");
                return;
            }
            nOwnerGroupID = LoginManager.GetLoginGroupID();
            m_sLangMenu = GetLangMenu(nOwnerGroupID);
            

            //m_sLangMenu = GetLangMenu(nOwnerGroupID);
        }
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
            Int32 nMainLangID = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select l.name,l.id from groups g,lu_languages l where l.id=g.language_id and  ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("g.id", "=", nGroupID);
            if (selectQuery.Execute("query", true) != null)
            {
                nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    sMainLang = selectQuery.Table("query").DefaultView[0].Row["name"].ToString();
                    nMainLangID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            sTemp += "<li><a href=\"";
            sTemp += "adm_categories_new.aspx?category_id=" + Session["category_id"].ToString();
            sTemp += "\"><span>";
            sTemp += sMainLang;
            sTemp += "</span></a></li>";

            //Int32 nOwnerGroupID = int.Parse(PageUtils.GetTableSingleVal("media", "group_id", int.Parse(Session["media_id"].ToString())).ToString());
            ODBCWrapper.DataSetSelectQuery selectQuery1 = new ODBCWrapper.DataSetSelectQuery();
            selectQuery1 += "select l.name,l.id from group_extra_languages gel,lu_languages l where gel.language_id=l.id and l.status=1 and gel.status=1 and  ";
            selectQuery1 += ODBCWrapper.Parameter.NEW_PARAM("l.id", "<>", nMainLangID);
            selectQuery1 += "and";
            selectQuery1 += ODBCWrapper.Parameter.NEW_PARAM("gel.group_id", "=", nGroupID);
            selectQuery1 += " order by l.name";
            if (selectQuery1.Execute("query", true) != null)
            {
                Int32 nCount1 = selectQuery1.Table("query").DefaultView.Count;
                for (int i = 0; i < nCount1; i++)
                {
                    Int32 nLangID = int.Parse(selectQuery1.Table("query").DefaultView[i].Row["id"].ToString());
                    string nLangName = selectQuery1.Table("query").DefaultView[i].Row["name"].ToString();
                    sTemp += "<li><a ";
                    if (nLangID == int.Parse(Session["lang_id"].ToString()))
                        sTemp += " class=\"on\" ";
                    sTemp += " href=\"";
                    sTemp += "adm_category_translate.aspx?category_id=" + Session["category_id"].ToString() + "&lang_id=" + nLangID.ToString();
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
    
    protected void GetFirstCategoryValues(ref string sCategoryName, ref Int32 nCategoryID)
    {
        Int32 nGroupID = LoginManager.GetLoginGroupID();
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select id,CATEGORY_NAME from categories where ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
        selectQuery += "and status=1 and is_active=1";
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
            {
                sCategoryName = selectQuery.Table("query").DefaultView[0].Row["CATEGORY_NAME"].ToString();
                nCategoryID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
            }
        }
        selectQuery.Finish();
        selectQuery = null;
    }

    protected string GetWhereAmIStr()
    {
        Int32 nCategoryID = 0;
        if (Session["parent_category_id"] != null && Session["parent_category_id"].ToString() != "" && Session["parent_category_id"].ToString() != "0")
            nCategoryID = int.Parse(Session["parent_category_id"].ToString());

        string sRet = "";
        bool bFirst = true;
        Int32 nLast = 0;
        nLast = 0;

        while (nCategoryID != nLast)
        {
            Int32 nParentID = int.Parse(PageUtils.GetTableSingleVal("categories", "parent_category_id", nCategoryID).ToString());
            string sHeader = PageUtils.GetTableSingleVal("categories", "CATEGORY_NAME", nCategoryID).ToString();
            if (bFirst == false)
                sRet = "<span style=\"cursor:pointer;\" onclick=\"document.location.href='adm_categories.aspx?parent_category_id=" + nParentID.ToString() + "';\">" + sHeader + " </span><span class=\"arrow\">&raquo; </span>" + sRet;
            else
                sRet = sHeader;
            bFirst = false;
            nCategoryID = nParentID;
        }
        if (sRet != "")
            sRet = "Categories: <span style=\"cursor:pointer;\" onclick=\"document.location.href='adm_categories.aspx?parent_category_id=0';\">Root </span><span class=\"arrow\">&raquo; </span>" + sRet;
        else
            sRet = "Categories: Root";
        return sRet;

    }

    public void GetHeader()
    {
        string sRet = PageUtils.GetPreHeader() + ": " + GetWhereAmIStr();
        sRet += " - Translate (";
        sRet += PageUtils.GetTableSingleVal("categories", "category_name", int.Parse(Session["category_id"].ToString())).ToString();
        sRet += " )";
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
        object t = null; ;
        if (Session["category_id"] != null && Session["category_id"].ToString() != "" && int.Parse(Session["category_id"].ToString()) != 0)
        {
            if (Session["lang_id"] != null && Session["lang_id"].ToString() != "" && int.Parse(Session["lang_id"].ToString()) != 0)
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select id from categories_translate where ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("CATEGORY_ID", "=", int.Parse(Session["category_id"].ToString()));
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("LANGUAGE_ID", "=", int.Parse(Session["lang_id"].ToString()));
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                        t = int.Parse(selectQuery.Table("query").DefaultView[0].Row["id"].ToString());
                }
                selectQuery.Finish();
                selectQuery = null;
            }
        }
        string sRet = "adm_categories_new.aspx?category_id=" + Session["category_id"].ToString();
        if (Session["parent_category_id"] != null)
            sRet += "&parent_category_id=" + Session["parent_category_id"].ToString();
        DBRecordWebEditor theRecord = new DBRecordWebEditor("categories_translate", "adm_table_pager", sRet, "", "ID", t, sRet, "");

        DataRecordShortTextField dr_group_name = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_group_name.Initialize("Category name", "adm_table_header_nbg", "FormInput", "NAME", true);
        theRecord.AddRecord(dr_group_name);

        DataRecordLongTextField dr_edit_data = new DataRecordLongTextField("rtl", true, 60, 10);
        dr_edit_data.Initialize("Editor remarks", "adm_table_header_nbg", "FormInput", "EDITOR_REMARKS", false);
        theRecord.AddRecord(dr_edit_data);

        DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
        dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", false);
        dr_groups.SetValue(LoginManager.GetLoginGroupID().ToString());
        theRecord.AddRecord(dr_groups);

        DataRecordShortIntField dr_channel_id = new DataRecordShortIntField(false, 9, 9);
        dr_channel_id.Initialize("Category", "adm_table_header_nbg", "FormInput", "CATEGORY_ID", false);
        dr_channel_id.SetValue(Session["category_id"].ToString());
        theRecord.AddRecord(dr_channel_id);

        DataRecordShortIntField dr_lang_id = new DataRecordShortIntField(false, 9, 9);
        dr_lang_id.Initialize("Lang", "adm_table_header_nbg", "FormInput", "LANGUAGE_ID", false);
        dr_lang_id.SetValue(Session["lang_id"].ToString());
        theRecord.AddRecord(dr_lang_id);

        string sTable = theRecord.GetTableHTML("adm_category_translate.aspx?submited=1");

        return sTable;
    }
}
