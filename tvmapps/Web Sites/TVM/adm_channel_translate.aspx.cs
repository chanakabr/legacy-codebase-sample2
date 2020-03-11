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

public partial class adm_channel_translate : System.Web.UI.Page
{
    protected string m_sMenu;
    protected string m_sSubMenu;
    protected string m_sLangMenu;
    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        if (LoginManager.IsPagePermitted("adm_channels.aspx") == false)
            LoginManager.LogoutFromSite("login.html");
        if (LoginManager.IsActionPermittedOnPage("adm_channels.aspx", LoginManager.PAGE_PERMISION_TYPE.EDIT) == false)
            LoginManager.LogoutFromSite("login.html");

        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        if (!IsPostBack)
        {
            if (Request.QueryString["submited"] != null && Request.QueryString["submited"].ToString() == "1")
            {
                DBManipulator.DoTheWork();
                return;
            }
            Int32 nMenuID = 0;
            m_sMenu = TVinciShared.Menu.GetMainMenu(6, true, ref nMenuID);
            Int32 nOwnerGroupID = LoginManager.GetLoginGroupID();
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 2, false);

            if (Request.QueryString["channel_id"] != null &&
                Request.QueryString["channel_id"].ToString() != "")
            {
                Session["channel_id"] = int.Parse(Request.QueryString["channel_id"].ToString());
                nOwnerGroupID = int.Parse(PageUtils.GetTableSingleVal("channels", "group_id", int.Parse(Session["channel_id"].ToString())).ToString());
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
            m_sLangMenu = GetLangMenu(nOwnerGroupID);
        }
    }

    protected void GetLangMenu()
    {
        Response.Write(m_sLangMenu);
    }

    protected void StartChannelType()
    {
        Session["channel_type"] = int.Parse(PageUtils.GetTableSingleVal("channels", "CHANNEL_TYPE", int.Parse(Session["channel_id"].ToString())).ToString());
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
            sTemp += "adm_channels_new.aspx?channel_id=" + Session["channel_id"].ToString();
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
                    sTemp += "adm_channel_translate.aspx?channel_id=" + Session["channel_id"].ToString() + "&lang_id=" + nLangID.ToString();
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
        Response.Write(PageUtils.GetPreHeader() + ": Channels translation (" + PageUtils.GetTableSingleVal("channels" , "name" , int.Parse(Session["channel_id"].ToString())) + " )");
    }

    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
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
        if (Session["channel_id"] != null && Session["channel_id"].ToString() != "" && int.Parse(Session["channel_id"].ToString()) != 0)
        {
            if (Session["lang_id"] != null && Session["lang_id"].ToString() != "" && int.Parse(Session["lang_id"].ToString()) != 0)
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select id from channel_translate where ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("CHANNEL_ID", "=", int.Parse(Session["channel_id"].ToString()));
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
        string sRet = "adm_channels_new.aspx?channel_id=" + Session["channel_id"].ToString();
        DBRecordWebEditor theRecord = new DBRecordWebEditor("channel_translate", "adm_table_pager", sRet, "", "ID", t, sRet, "channel_id");

        DataRecordShortTextField dr_name = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_name.Initialize("Name", "adm_table_header_nbg", "FormInput", "NAME", true);
        theRecord.AddRecord(dr_name);

        DataRecordLongTextField dr_bio = new DataRecordLongTextField("ltr", true, 60, 20);
        dr_bio.Initialize("Description", "adm_table_header_nbg", "FormInput", "DESCRIPTION", false);
        theRecord.AddRecord(dr_bio);

        DataRecordLongTextField dr_edit_data = new DataRecordLongTextField("rtl", true, 60, 10);
        dr_edit_data.Initialize("Editor remarks", "adm_table_header_nbg", "FormInput", "EDITOR_REMARKS", false);
        theRecord.AddRecord(dr_edit_data);

        
        DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
        dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", false);
        dr_groups.SetValue(LoginManager.GetLoginGroupID().ToString());
        theRecord.AddRecord(dr_groups);

        DataRecordShortIntField dr_channel_id = new DataRecordShortIntField(false, 9, 9);
        dr_channel_id.Initialize("Channel", "adm_table_header_nbg", "FormInput", "CHANNEL_ID", false);
        dr_channel_id.SetValue(Session["channel_id"].ToString());
        theRecord.AddRecord(dr_channel_id);

        DataRecordShortIntField dr_lang_id = new DataRecordShortIntField(false, 9, 9);
        dr_lang_id.Initialize("Lang", "adm_table_header_nbg", "FormInput", "LANGUAGE_ID", false);
        dr_lang_id.SetValue(Session["lang_id"].ToString());
        theRecord.AddRecord(dr_lang_id);

        string sTable = theRecord.GetTableHTML("adm_channel_translate.aspx?submited=1");

        return sTable;
    }
}
