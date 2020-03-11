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

public partial class adm_ext_media : System.Web.UI.Page
{
    protected string m_sMenu;
    protected string m_sSubMenu;
    protected string m_sLangMenu;
    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        if (LoginManager.IsPagePermitted() == false)
            LoginManager.LogoutFromSite("login.html");
        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        if (!IsPostBack)
        {
            Int32 nMenuID = 0;
            m_sMenu = TVinciShared.Menu.GetMainMenu(7, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 2, false);
            Int32 nGroupID = LoginManager.GetLoginGroupID();
            m_sLangMenu = GetLangMenu(nGroupID);
            if (Request.QueryString["search_save"] != null)
                Session["search_save"] = "1";
            else
                Session["search_save"] = null;
        }
    }

    protected void GetLangMenu()
    {
        Response.Write(m_sLangMenu);
    }

    public void GetHeader()
    {
        Response.Write(PageUtils.GetPreHeader() + ": Media management");
    }

    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
    }

    protected void GetSubMenu()
    {
        Response.Write(m_sSubMenu);
    }

    public string GetTableCSV()
    {
        string sOldOrderBy = "";
        if (Session["order_by"] != null)
            sOldOrderBy = Session["order_by"].ToString();
        DBTableWebEditor theTable = new DBTableWebEditor(true, true, false, "", "adm_table_header", "adm_table_cell", "adm_table_alt_cell", "adm_table_link", "adm_table_pager", "adm_table", sOldOrderBy, 50);
        FillTheTableEditor(ref theTable, sOldOrderBy);

        string sCSVFile = theTable.OpenCSV();
        theTable.Finish();
        theTable = null;
        return sCSVFile;
    }

    protected string GetLangMenu(Int32 nGroupID)
    {
        try
        {
            string sTemp = "";

            sTemp += "<li><a class=\"on\" href=\"";
            sTemp += "adm_ext_media.aspx";
            sTemp += "\"><span>Source Language";
            sTemp += "</span></a></li>";

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
            sTemp += "adm_ext_media_translate.aspx?lang_id=" + nMainLangID.ToString();
            sTemp += "\"><span>";
            sTemp += sMainLang;
            sTemp += "</span></a></li>";

            Int32 nOwnerGroupID = LoginManager.GetLoginGroupID();
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
                    sTemp += "adm_ext_media_translate.aspx?lang_id=" + nLangID.ToString();
                    sTemp += "\"><span>";
                    sTemp += nLangName;
                    sTemp += "</span></a></li>";
                }
                //if (nCount1 == 0)
                    //sTemp = "";
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

    protected void FillTheTableEditor(ref DBTableWebEditor theTable, string sOrderBy)
    {
        Int32 nGroupID = LoginManager.GetLoginGroupID();
        string sWPGID = PageUtils.GetPermittedWatchRulesID(nGroupID);
        theTable += "select q.editor_remarks,q.group_id,q.status,q.s_id as 'MID',q.group_name as 'Group',p.base_url as 'Pic',q.NAME as 'Name',q.start_date as 'Start Date',q.end_date as 'End Date',q.description as 'Description',q.s_id as 'id',q.s_desc as 'State',q.q_ia as 'On/Off' from (select distinct m.group_id,m.is_active as 'q_ia',m.MEDIA_PIC_ID as 'pic_id',m.status,m.NAME as 'NAME',m.DESCRIPTION as 'Description',m.id as 's_id',lcs.description as 's_desc',CONVERT(VARCHAR(19),m.START_DATE, 120) as 'Start_Date',CONVERT(VARCHAR(19),m.End_DATE, 120) as 'End_Date',g.GROUP_NAME,m.editor_remarks  from media m,lu_content_status lcs,groups g ";
        if (Session["search_tag"] != null && Session["search_tag"].ToString() != "")
            theTable += ",tags t,media_tags mt ";
        theTable += "where g.id=m.group_id and m.status<>2 and lcs.id=m.status ";
        if (Session["search_tag"] != null && Session["search_tag"].ToString() != "")
        {
            string sL = "LTRIM(RTRIM(LOWER(t.value))) like ('%" + Session["search_tag"].ToString().ToLower().Trim() + "%')";
            theTable += " and t.id=mt.tag_id and mt.media_ID=m.id and " + sL;
        }
        if (Session["search_free"] != null && Session["search_free"].ToString() != "")
        {
            string sLike = "like('%" + Session["search_free"].ToString().Replace("'", "''") + "%')";
            theTable += " and (m.NAME " + sLike + " OR m.DESCRIPTION " + sLike + " OR m.META1_STR " + sLike + " OR m.META2_STR " + sLike + " OR m.META3_STR " + sLike + " OR m.META4_STR " + sLike + " OR m.META5_STR " + sLike + " OR m.META6_STR " + sLike + " OR m.META7_STR " + sLike + " OR m.META8_STR " + sLike + " OR m.META9_STR " + sLike + " OR m.META10_STR " + sLike + ")";
        }
        if (Session["search_on_off"] != null && Session["search_on_off"].ToString() != "" && Session["search_on_off"].ToString() != "-1")
        {
            theTable += " and ";
            theTable += ODBCWrapper.Parameter.NEW_PARAM("m.is_active", "=", int.Parse(Session["search_on_off"].ToString()));
        }
        /*if (Session["translated_yes_no"] != null && Session["translated_yes_no"].ToString() != "" && Session["translated_yes_no"].ToString() != "-1")
        {
            theTable += " and m.id in(";
            theTable += GetTranslatedMedia(
            theTable += ")";
        }*/
        theTable += "and (";
        theTable += ODBCWrapper.Parameter.NEW_PARAM("m.group_id", "<>", nGroupID);
        if (sWPGID != "")
        {
            theTable += " and m.WATCH_PERMISSION_TYPE_ID in (";
            theTable += sWPGID;
            theTable += ")";
        }
        else
            theTable += " and m.WATCH_PERMISSION_TYPE_ID in (-1) ";
        theTable += ") ";
        theTable += " )q LEFT JOIN pics p ON p.id=q.pic_id and " + PageUtils.GetStatusQueryPart("p");
        if (sOrderBy != "")
        {
            theTable += " order by ";
            theTable += sOrderBy;
        }
        else
            theTable += " order by q.s_id desc";
        theTable.AddHiddenField("ID");
        theTable.AddHiddenField("group_id");
        theTable.AddHiddenField("status");
        theTable.AddOrderByColumn("Name", "q.NAME");
        theTable.AddOrderByColumn("Group", "q.group_name");
        theTable.AddOrderByColumn("State", "q.s_desc");
        theTable.AddOrderByColumn("Media ID", "q.s_id");
        theTable.AddImageField("Pic");
        theTable.AddTechDetails("media");
        theTable.AddEditorRemarks("media");
        theTable.AddHiddenField("EDITOR_REMARKS");
        //theTable.AddActivationField("media");

        {
            DataTableLinkColumn linkColumn1 = new DataTableLinkColumn("adm_ext_media_files.aspx", "Files", "");
            linkColumn1.AddQueryStringValue("ext_media_id", "field=id");
            linkColumn1.AddQueryCounterValue("select count(*) as val from media_files where status=1 and is_active=1 and media_ID=", "field=id");
            theTable.AddLinkColumn(linkColumn1);
        }
        {
            DataTableLinkColumn linkColumn1 = new DataTableLinkColumn("adm_media_statistics.aspx", "Statistics", "");
            linkColumn1.AddQueryStringValue("media_id", "field=id");
            theTable.AddLinkColumn(linkColumn1);
        }
        if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.EDIT))
        {
            DataTableLinkColumn linkColumn1 = new DataTableLinkColumn("adm_ext_media_translate.aspx", "Edit", "");
            linkColumn1.AddQueryStringValue("media_id", "field=id");
            theTable.AddLinkColumn(linkColumn1);
        }
    }

    public string GetPageContent(string sOrderBy, string sPageNum, string search_tag, string search_free, string search_on_off)
    {
        if (search_tag != "")
            Session["search_tag"] = search_tag.Replace("'", "''");
        else if (Session["search_save"] == null)
            Session["search_tag"] = "";

        if (search_free != "")
            Session["search_free"] = search_free.Replace("'", "''");
        else if (Session["search_save"] == null)
            Session["search_free"] = "";

        if (search_on_off != "-1")
            Session["search_on_off"] = search_on_off;
        else if (Session["search_save"] == null)
            Session["search_on_off"] = "";
        /*
        if (translated_yes_no != "-1")
            Session["translated_yes_no"] = translated_yes_no;
        else if (Session["search_save"] == null)
            Session["translated_yes_no"] = "";
        */

        if (sOrderBy != "")
            Session["order_by"] = sOrderBy;
        else if (Session["search_save"] == null)
            Session["order_by"] = "";
        else
            sOrderBy = Session["order_by"].ToString();

        string sOldOrderBy = "";
        if (Session["order_by"] != null)
            sOldOrderBy = Session["order_by"].ToString();

        DBTableWebEditor theTable = new DBTableWebEditor(true, true, false, "", "adm_table_header", "adm_table_cell", "adm_table_alt_cell", "adm_table_link", "adm_table_pager", "adm_table", sOldOrderBy, 20);
        FillTheTableEditor(ref theTable, sOrderBy);

        string sTable = theTable.GetPageHTML(int.Parse(sPageNum), sOrderBy);
        theTable.Finish();
        theTable = null;
        return sTable;
    }
}
