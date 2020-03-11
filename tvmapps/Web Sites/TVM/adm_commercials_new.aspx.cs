using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using TVinciShared;

public partial class adm_commercials_new : System.Web.UI.Page
{
    protected string m_sMenu;
    protected string m_sSubMenu;
    protected string m_sLangMenu;
    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        if (LoginManager.IsPagePermitted("adm_commercials.aspx") == false)
            LoginManager.LogoutFromSite("login.html");
        if (LoginManager.IsActionPermittedOnPage("adm_commercials.aspx", LoginManager.PAGE_PERMISION_TYPE.EDIT) == false)
            LoginManager.LogoutFromSite("login.html");

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

            m_sMenu = TVinciShared.Menu.GetMainMenu(13, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 2, true);
            if (Request.QueryString["commercial_id"] != null &&
                Request.QueryString["commercial_id"].ToString() != "")
            {
                Session["commercial_id"] = int.Parse(Request.QueryString["commercial_id"].ToString());
                Int32 nOwnerGroupID = int.Parse(PageUtils.GetTableSingleVal("commercial", "group_id", int.Parse(Session["commercial_id"].ToString())).ToString());
                Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();
                if (nLogedInGroupID != nOwnerGroupID && PageUtils.IsTvinciUser() == false)
                {
                    LoginManager.LogoutFromSite("login.html");
                    return;
                }
                m_sLangMenu = GetLangMenu(nOwnerGroupID);
            }
            else
                Session["commercial_id"] = 0;
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
            sTemp += "<li><a class=\"on\" href=\"";
            sTemp += "adm_commercials_new.aspx?commercial_id=" + Session["commercial_id"].ToString();
            sTemp += "\"><span>";
            sTemp += sMainLang;
            sTemp += "</span></a></li>";

            Int32 nOwnerGroupID = int.Parse(PageUtils.GetTableSingleVal("commercial", "group_id", int.Parse(Session["commercial_id"].ToString())).ToString());
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
                    sTemp += "<li><a href=\"";
                    sTemp += "adm_commercial_translate.aspx?commercial_id=" + Session["commercial_id"].ToString() + "&lang_id=" + nLangID.ToString();
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
        Response.Write(PageUtils.GetPreHeader() + ": Commercials Management");
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

    protected void AddTagsFields(ref DBRecordWebEditor theRecord)
    {
        DataRecordMultiField dr_tags = new DataRecordMultiField("tags", "id", "id", "commercial_tags", "commercial_id", "TAG_ID", true, "ltr", 60, "tags");
        dr_tags.Initialize("Free Tags", "adm_table_header_nbg", "FormInput", "VALUE", false);
        dr_tags.SetCollectionLength(8);
        dr_tags.SetExtraWhere("TAG_TYPE_ID=0");
        theRecord.AddRecord(dr_tags);
    }

    public string GetPageContent(string sOrderBy, string sPageNum)
    {
        if (Session["error_msg"] != null && Session["error_msg"].ToString() != "")
        {
            Session["error_msg"] = "";
            return Session["last_page_html"].ToString();
        }
        object t = null;
        string sRet = "adm_commercials.aspx";
        if (Session["commercial_id"] != null && Session["commercial_id"].ToString() != "" && int.Parse(Session["commercial_id"].ToString()) != 0)
        {
            t = Session["commercial_id"];
            if (Session["campaign_id"] != null && Session["campaign_id"].ToString() != "" && int.Parse(Session["campaign_id"].ToString()) != 0)
                sRet += "?campaign_id=" + Session["campaign_id"].ToString();
        }

        DBRecordWebEditor theRecord = new DBRecordWebEditor("commercial", "adm_table_pager", sRet, "", "ID", t, sRet, "commercial_id");

        DataRecordShortTextField dr_name = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_name.Initialize("Name", "adm_table_header_nbg", "FormInput", "NAME", true);
        theRecord.AddRecord(dr_name);

        DataRecordLongTextField dr_description = new DataRecordLongTextField("ltr", true, 60, 4);
        dr_description.Initialize("Description", "adm_table_header_nbg", "FormInput", "DESCRIPTION", false);
        theRecord.AddRecord(dr_description);

        DataRecordShortTextField dr_text = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_text.Initialize("Text Commercial", "adm_table_header_nbg", "FormInput", "TEXT_COMM", false);
        theRecord.AddRecord(dr_text);

        DataRecordShortTextField dr_url = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_url.Initialize("On Click URL", "adm_table_header_nbg", "FormInput", "CLICK_URL", false);
        theRecord.AddRecord(dr_url);

        DataRecordCheckBoxField dr_main_stop = new DataRecordCheckBoxField(true);
        dr_main_stop.Initialize("Main Video Stop", "adm_table_header_nbg", "FormInput", "MAIN_STOP", false);
        theRecord.AddRecord(dr_main_stop);

        DataRecordCheckBoxField dr_controlls = new DataRecordCheckBoxField(true);
        dr_controlls.Initialize("Controlls Enabled", "adm_table_header_nbg", "FormInput", "CONTROLS_ENABLE", false);
        theRecord.AddRecord(dr_controlls);

        DataRecordCheckBoxField dr_close_butt = new DataRecordCheckBoxField(true);
        dr_close_butt.Initialize("With Close Button", "adm_table_header_nbg", "FormInput", "CLOSE_BUTTON", false);
        theRecord.AddRecord(dr_close_butt);

        DataRecordShortIntField dr_vis_time = new DataRecordShortIntField(true , 60 , 60);
        dr_vis_time.Initialize("Visibility Time", "adm_table_header_nbg", "FormInput", "VISIBLE_TIME_SEC", false);
        theRecord.AddRecord(dr_vis_time);

        DataRecordDateTimeField dr_start_date = new DataRecordDateTimeField(true);
        dr_start_date.Initialize("Start Date", "adm_table_header_nbg", "FormInput", "START_DATE", false);
        dr_start_date.SetDefault(DateTime.Now);
        theRecord.AddRecord(dr_start_date);

        DataRecordDateTimeField dr_end_date = new DataRecordDateTimeField(true);
        dr_end_date.Initialize("End Date", "adm_table_header_nbg", "FormInput", "END_DATE", false);
        theRecord.AddRecord(dr_end_date);

        DataRecordShortIntField dr_unique_views = new DataRecordShortIntField(true, 60, 128);
        dr_unique_views.Initialize("Maximum Unique Views", "adm_table_header_nbg", "FormInput", "MAX_UNIQUE_VIEWS", false);
        theRecord.AddRecord(dr_unique_views);

        DataRecordShortIntField dr_unique_views_day = new DataRecordShortIntField(true, 60, 128);
        dr_unique_views_day.Initialize("Maximum Unique Views Per Day", "adm_table_header_nbg", "FormInput", "MAX_UNIQUE_VIEWS_DAY", false);
        theRecord.AddRecord(dr_unique_views_day);

        DataRecordShortIntField dr_unique_delta = new DataRecordShortIntField(true, 60, 128);
        dr_unique_delta.Initialize("Unique Frequency(Minutes)", "adm_table_header_nbg", "FormInput", "UNIQUE_TIME_DIFF", false);
        theRecord.AddRecord(dr_unique_delta);

        DataRecordDropDownField dr_watch_permissions = new DataRecordDropDownField("watch_permissions_types", "NAME", "id", "", null, 60, true);
        string sQuery = "select name as txt,id as id from watch_permissions_types where status=1 and group_id= " + LoginManager.GetLoginGroupID().ToString();
        dr_watch_permissions.SetSelectsQuery(sQuery);
        dr_watch_permissions.Initialize("Watch Permission Rule", "adm_table_header_nbg", "FormInput", "WATCH_PERMISSION_TYPE_ID", false);
        theRecord.AddRecord(dr_watch_permissions);

        DataRecordDropDownField dr_block_rules = new DataRecordDropDownField("geo_block_types", "NAME", "id", "", null, 60, true);
        sQuery = "select name as txt,id as id from geo_block_types where status=1 and group_id= " + LoginManager.GetLoginGroupID().ToString();
        dr_block_rules.SetSelectsQuery(sQuery);
        dr_block_rules.Initialize("Geo block Rule", "adm_table_header_nbg", "FormInput", "BLOCK_TEMPLATE_ID", false);
        theRecord.AddRecord(dr_block_rules);

        DataRecordDropDownField dr_players_rules = new DataRecordDropDownField("geo_block_types", "NAME", "id", "", null, 60, true);
        sQuery = "select name as txt,id as id from players_groups_types where status=1 and group_id= " + LoginManager.GetLoginGroupID().ToString();
        dr_players_rules.SetSelectsQuery(sQuery);
        dr_players_rules.Initialize("Players Rule", "adm_table_header_nbg", "FormInput", "PLAYERS_RULES", false);
        theRecord.AddRecord(dr_players_rules);

        DataRecordMultiField dr_campaigns = new DataRecordMultiField("campaigns", "id", "id", "campaigns_commercials", "COMMERCIAL_ID", "campaign_id", false, "ltr", 60, "tags");
        dr_campaigns.Initialize("Campaigns", "adm_table_header_nbg", "FormInput", "NAME", false);
        sQuery = "select NAME as txt,id as val from campaigns where status=1 and group_id=" + LoginManager.GetLoginGroupID().ToString();
        sQuery += " order by NAME";
        dr_campaigns.SetCollectionQuery(sQuery);
        theRecord.AddRecord(dr_campaigns);

        AddTagsFields(ref theRecord);

        DataRecordLongTextField dr_remarks = new DataRecordLongTextField("ltr", true, 60, 4);
        dr_remarks.Initialize("Remarks", "adm_table_header_nbg", "FormInput", "EDITOR_REMARKS", false);
        theRecord.AddRecord(dr_remarks);

        DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
        dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", false);
        dr_groups.SetValue(LoginManager.GetLoginGroupID().ToString());
        theRecord.AddRecord(dr_groups);

        string sTable = theRecord.GetTableHTML("adm_commercials_new.aspx?submited=1");

        return sTable;
    }
}
