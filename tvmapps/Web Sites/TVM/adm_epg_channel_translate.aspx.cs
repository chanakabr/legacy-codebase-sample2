using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TVinciShared;

public partial class adm_epg_channel_translate : System.Web.UI.Page
{
    protected string m_sMenu;
    protected string m_sSubMenu;
    protected string m_sLangMenu;
    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        if (LoginManager.IsPagePermitted("adm_epg_channels.aspx") == false)
        {
            LoginManager.LogoutFromSite("login.html");
            return;
        }
        if (LoginManager.IsActionPermittedOnPage("adm_epg_channels.aspx", LoginManager.PAGE_PERMISION_TYPE.EDIT) == false)
        {
            LoginManager.LogoutFromSite("login.html");
            return;
        }

        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        if (!IsPostBack)
        {
            if (Request.QueryString["submited"] != null && Request.QueryString["submited"].ToString().Trim() == "1")
            {
                Int32 nID = DBManipulator.DoTheWork();
                return;
            }
            Int32 nMenuID = 0;

            m_sMenu = TVinciShared.Menu.GetMainMenu(6, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 3, true);
            if (Request.QueryString["epg_channel_id"] != null &&
                Request.QueryString["epg_channel_id"].ToString() != "")
            {
                Session["epg_channel_id"] = int.Parse(Request.QueryString["epg_channel_id"].ToString());
                Int32 nOwnerGroupID = int.Parse(PageUtils.GetTableSingleVal("epg_channels", "group_id", int.Parse(Session["epg_channel_id"].ToString())).ToString());
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
                Int32 nOwnerGroupID = int.Parse(PageUtils.GetTableSingleVal("epg_channels", "group_id", int.Parse(Session["epg_channel_id"].ToString())).ToString());
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
        }
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
            sTemp += "epg_channels_new.aspx?epg_channel_id=" + Session["epg_channel_id"].ToString();
            sTemp += "\"><span>";
            sTemp += sMainLang;
            sTemp += "</span></a></li>";

            Int32 nOwnerGroupID = int.Parse(PageUtils.GetTableSingleVal("media", "group_id", int.Parse(Session["epg_channel_id"].ToString())).ToString());
            ODBCWrapper.DataSetSelectQuery selectQuery1 = new ODBCWrapper.DataSetSelectQuery();
            selectQuery1 += "select l.name,l.id from group_extra_languages gel,lu_languages l where gel.language_id=l.id and l.status=1 and gel.status=1 and  ";
            selectQuery1 += ODBCWrapper.Parameter.NEW_PARAM("l.id", "<>", nMainLangID);
            selectQuery1 += "and";
            selectQuery1 += ODBCWrapper.Parameter.NEW_PARAM("gel.group_id", "=", nOwnerGroupID);
            selectQuery1 += " order by l.name";
            if (selectQuery1.Execute("query", true) != null)
            {
                Int32 nCount1 = selectQuery1.Table("query").DefaultView.Count;
                for (int i = 0; i < nCount1; i++)
                {
                    Int32 nLangID = int.Parse(selectQuery1.Table("query").DefaultView[i].Row["id"].ToString());
                    string nLangName = selectQuery1.Table("query").DefaultView[i].Row["name"].ToString();
                    if (int.Parse(Session["lang_id"].ToString()) == nLangID)
                        sTemp += "<li><a class=\"on\" href=\"";
                    else
                        sTemp += "<li><a href=\"";
                    sTemp += "epg_channel_translate.aspx?epg_channel_id=" + Session["epg_channel_id"].ToString() + "&lang_id=" + nLangID.ToString();
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
        Response.Write(PageUtils.GetPreHeader() + ": EPG Channels translation (" + PageUtils.GetTableSingleVal("epg_channels", "name", int.Parse(Session["epg_channel_id"].ToString())).ToString());
        //Response.Write(PageUtils.GetPreHeader() + ": Media management translation");
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
        object t = null;
        if (Session["epg_channel_id"] != null && Session["epg_channel_id"].ToString() != "" && int.Parse(Session["epg_channel_id"].ToString()) != 0)
        {
            if (Session["lang_id"] != null && Session["lang_id"].ToString() != "" && int.Parse(Session["lang_id"].ToString()) != 0)
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select id from epg_channels_translate where ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("epg_channel_id", "=", int.Parse(Session["epg_channel_id"].ToString()));
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
        string sRet = "adm_epg_channels.aspx?search_save=1";
        DBRecordWebEditor theRecord = new DBRecordWebEditor("epg_channels_translate", "adm_table_pager", sRet, "", "ID", t, sRet, "epg_channel_id");

        DataRecordShortTextField dr_name = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_name.Initialize("Name", "adm_table_header_nbg", "FormInput", "NAME", true);
        theRecord.AddRecord(dr_name);

        DataRecordLongTextField dr_description = new DataRecordLongTextField("ltr", true, 60, 4);
        dr_description.Initialize("Description", "adm_table_header_nbg", "FormInput", "DESCRIPTION", false);
        theRecord.AddRecord(dr_description);

        DataRecordLongTextField dr_remarks = new DataRecordLongTextField("ltr", true, 60, 4);
        dr_remarks.Initialize("Remarks", "adm_table_header_nbg", "FormInput", "EDITOR_REMARKS", false);
        theRecord.AddRecord(dr_remarks);

        DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
        dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", false);
        dr_groups.SetValue(LoginManager.GetLoginGroupID().ToString());
        theRecord.AddRecord(dr_groups);

        DataRecordShortIntField dr_epg_channel_id = new DataRecordShortIntField(false, 9, 9);
        dr_epg_channel_id.Initialize("epg_channels", "adm_table_header_nbg", "FormInput", "epg_channel_id", false);
        dr_epg_channel_id.SetValue(Session["epg_channel_id"].ToString());
        theRecord.AddRecord(dr_epg_channel_id);

        DataRecordShortIntField dr_lang_id = new DataRecordShortIntField(false, 9, 9);
        dr_lang_id.Initialize("Lang", "adm_table_header_nbg", "FormInput", "LANGUAGE_ID", false);
        dr_lang_id.SetValue(Session["lang_id"].ToString());
        theRecord.AddRecord(dr_lang_id);

        string sTable = theRecord.GetTableHTML("adm_epg_channel_translate.aspx?submited=1");

        return sTable;
    }
}
