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

public partial class adm_commercials : System.Web.UI.Page
{
    protected string m_sMenu;
    protected string m_sSubMenu;
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
            m_sMenu = TVinciShared.Menu.GetMainMenu(13, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 2, false);
            if (Request.QueryString["search_save"] != null)
                Session["search_save"] = "1";
            else
                Session["search_save"] = null;

            if (Request.QueryString["campaign_id"] != null &&
                Request.QueryString["campaign_id"].ToString() != "")
                Session["campaign_id"] = int.Parse(Request.QueryString["campaign_id"].ToString());
            else
                //if (Session["campaign_id"] == null || Session["campaign_id"].ToString() == "" || Session["campaign_id"].ToString() == "0")
                    Session["campaign_id"] = null;
        }
    }

    public void GetBackButton()
    {
        if (Session["campaign_id"] == null)
            return;
        string sRet = "<tr>";
        sRet += "<td onclick='window.document.location.href=\"adm_campaigns.aspx\";'><a href=\"#\" class=\"btn\">Back</a></td>";
        sRet += "</tr>";
        Response.Write(sRet);
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

    protected void FillTheTableEditor(ref DBTableWebEditor theTable, string sOrderBy)
    {
        Int32 nGroupID = LoginManager.GetLoginGroupID();
        theTable += "select q.editor_remarks,q.is_active,q.status,q.s_id as 'CID',q.NAME as 'Name',q.start_date as 'Start Date',q.end_date as 'End Date',q.description as 'Description',q.TEXT_COMM as 'Text Add',q.Click_url as 'Link',q.views as 'Views',q.clicks as 'Clicks',q.s_id as 'id',q.s_desc as 'State' from ";
        theTable += "(select distinct m.editor_remarks,m.is_active,m.clicks,m.TEXT_COMM,m.is_active as 'q_ia',m.status,m.NAME as 'NAME',m.views,m.DESCRIPTION as 'Description',m.id as 's_id',lcs.description as 's_desc',CONVERT(VARCHAR(19),m.START_DATE, 120) as 'Start_Date',CONVERT(VARCHAR(19),m.End_DATE, 120) as 'End_Date',m.click_url  from ";
        if (Session["campaign_id"] != null)
            theTable += "campaigns_commercials cc,";
        theTable += "commercial m,lu_content_status lcs";
        if (Session["search_tag"] != null && Session["search_tag"].ToString() != "")
            theTable += ",tags t,commercial_tags mt ";
        theTable += "where m.status<>2 and lcs.id=m.status ";
        if (Session["campaign_id"] != null)
        {
            theTable += " and cc.status=1 and cc.commercial_id=m.id and ";
            theTable += ODBCWrapper.Parameter.NEW_PARAM("cc.campaign_id", "=", int.Parse(Session["campaign_id"].ToString()));
        }
            
        if (Session["search_tag"] != null && Session["search_tag"].ToString() != "")
        {
            string sL = "LTRIM(RTRIM(LOWER(t.value))) like ('%" + Session["search_tag"].ToString().ToLower().Trim() + "%')";
            theTable += " and t.id=mt.tag_id and mt.commercial_ID=m.id and " + sL;
        }
        if (Session["search_free"] != null && Session["search_free"].ToString() != "")
        {
            string sLike = "like('%" + Session["search_free"].ToString().Replace("'", "''") + "%')";
            theTable += " and (m.NAME " + sLike + " OR m.DESCRIPTION " + sLike + " OR m.TEXT_COMM " + sLike + ")";
        }
        theTable += "and ";
        theTable += ODBCWrapper.Parameter.NEW_PARAM("m.group_id", "=", nGroupID);
        theTable += " )q ";
        if (sOrderBy != "")
        {
            theTable += " order by ";
            theTable += sOrderBy;
        }
        else
            theTable += " order by q.s_id desc";
        theTable.AddHiddenField("ID");
        theTable.AddHiddenField("status");
        theTable.AddOrderByColumn("Name", "q.NAME");
        theTable.AddOrderByColumn("State", "q.s_desc");
        theTable.AddOrderByColumn("Media ID", "q.s_id");
        theTable.AddTechDetails("commercial");
        theTable.AddEditorRemarks("commercial");
        theTable.AddHiddenField("EDITOR_REMARKS");
        theTable.AddActivationField("commercial");
        theTable.AddHiddenField("is_active");

        {
            DataTableLinkColumn linkColumn1 = new DataTableLinkColumn("adm_commercials_files.aspx", "Files", "");
            linkColumn1.AddQueryStringValue("commercial_id", "field=id");
            linkColumn1.AddQueryCounterValue("select count(*) as val from commercial_files where status=1 and is_active=1 and commercial_ID=", "field=id");
            theTable.AddLinkColumn(linkColumn1);
        }

        {
            DataTableLinkColumn linkColumn1 = new DataTableLinkColumn("adm_commercials_statistics.aspx", "Statistics", "");
            linkColumn1.AddQueryStringValue("commercial_id", "field=id");
            theTable.AddLinkColumn(linkColumn1);
        }
        if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.EDIT))
        {
            DataTableLinkColumn linkColumn1 = new DataTableLinkColumn("adm_commercials_new.aspx", "Edit", "");
            linkColumn1.AddQueryStringValue("commercial_id", "field=id");
            theTable.AddLinkColumn(linkColumn1);
        }
        if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.REMOVE))
        {
            DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_remove.aspx", "Delete", "STATUS=1;STATUS=3");
            linkColumn.AddQueryStringValue("id", "field=id");
            linkColumn.AddQueryStringValue("table", "commercial");
            linkColumn.AddQueryStringValue("confirm", "true");
            linkColumn.AddQueryStringValue("main_menu", "13");
            linkColumn.AddQueryStringValue("sub_menu", "2");
            linkColumn.AddQueryStringValue("rep_field", "NAME");
            linkColumn.AddQueryStringValue("rep_name", "ων");
            theTable.AddLinkColumn(linkColumn);
        }

        if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.PUBLISH))
        {
            DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_confirm.aspx", "Confirm", "STATUS=3;STATUS=4");
            linkColumn.AddQueryStringValue("id", "field=id");
            linkColumn.AddQueryStringValue("table", "commercial");
            linkColumn.AddQueryStringValue("confirm", "true");
            linkColumn.AddQueryStringValue("main_menu", "13");
            linkColumn.AddQueryStringValue("sub_menu", "2");
            linkColumn.AddQueryStringValue("rep_field", "NAME");
            linkColumn.AddQueryStringValue("rep_name", "ων");
            theTable.AddLinkColumn(linkColumn);
        }

        if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.PUBLISH))
        {
            DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_confirm.aspx", "Cancel", "STATUS=3;STATUS=4");
            linkColumn.AddQueryStringValue("id", "field=id");
            linkColumn.AddQueryStringValue("table", "commercial");
            linkColumn.AddQueryStringValue("confirm", "false");
            linkColumn.AddQueryStringValue("main_menu", "13");
            linkColumn.AddQueryStringValue("sub_menu", "2");
            linkColumn.AddQueryStringValue("rep_field", "NAME");
            linkColumn.AddQueryStringValue("rep_name", "ων");
            theTable.AddLinkColumn(linkColumn);
        }
    }

    public string GetPageContent(string sOrderBy, string sPageNum, string search_tag, string search_free)
    {
        if (search_tag != "")
            Session["search_tag"] = search_tag.Replace("'", "''");
        else if (Session["search_save"] == null)
            Session["search_tag"] = "";

        if (search_free != "")
            Session["search_free"] = search_free.Replace("'", "''");
        else if (Session["search_save"] == null)
            Session["search_free"] = "";

        if (sOrderBy != "")
            Session["order_by"] = sOrderBy;
        else if (Session["search_save"] == null)
            Session["order_by"] = "";
        else
            sOrderBy = Session["order_by"].ToString();

        string sOldOrderBy = "";
        if (Session["order_by"] != null)
            sOldOrderBy = Session["order_by"].ToString();

        DBTableWebEditor theTable = new DBTableWebEditor(true, true, true, "", "adm_table_header", "adm_table_cell", "adm_table_alt_cell", "adm_table_link", "adm_table_pager", "adm_table", sOldOrderBy, 50);
        FillTheTableEditor(ref theTable, sOrderBy);

        string sTable = theTable.GetPageHTML(int.Parse(sPageNum), sOrderBy);
        theTable.Finish();
        theTable = null;
        return sTable;
    }
}
