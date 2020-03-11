using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TVinciShared;

public partial class adm_epg_channels_schedule_comments : System.Web.UI.Page
{
    protected string m_sMenu;
    protected string m_sSubMenu;
    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        if (LoginManager.IsPagePermitted("adm_epg_channels.aspx") == false)
            LoginManager.LogoutFromSite("login.html");
        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        if (!IsPostBack)
        {
            Int32 nMenuID = 0;

            if (Request.QueryString["search_save"] != null)
                Session["search_save"] = "1";
            else
                Session["search_save"] = null;

            if (Request.QueryString["comment_type_id"] != null &&
                Request.QueryString["comment_type_id"].ToString() != "")
                Session["comment_type_id"] = int.Parse(Request.QueryString["comment_type_id"].ToString());
            else
                Session["comment_type_id"] = 0;

            if (Request.QueryString["program_id"] != null && Request.QueryString["program_id"].ToString() != "")
            {
                Session["program_id"] = int.Parse(Request.QueryString["program_id"].ToString());
                if (Session["program_id"].ToString() != "0")
                {
                    Int32 nOwnerGroupID = int.Parse(PageUtils.GetTableSingleVal("epg_channels_schedule", "group_id", int.Parse(Session["program_id"].ToString())).ToString());
                    Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();
                    if (nLogedInGroupID != nOwnerGroupID && PageUtils.IsTvinciUser() == false)
                    {
                        LoginManager.LogoutFromSite("login.html");
                        return;
                    }
                }
            }
            else
                Session["program_id"] = 0;
            if (Session["program_id"].ToString() != "0")
                m_sMenu = TVinciShared.Menu.GetMainMenu(6, true, ref nMenuID, "adm_epg_channels.aspx");
            else
                m_sMenu = TVinciShared.Menu.GetMainMenu(6, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, false);
        }
    }

    public void GetHeader()
    {
        string sCommentType = "Watchers Comments";
        if (Session["comment_type_id"] != null && Session["comment_type_id"].ToString() != "" && Session["comment_type_id"].ToString() != "0")
            sCommentType = ODBCWrapper.Utils.GetTableSingleVal("comment_types", "NAME", int.Parse(Session["comment_type_id"].ToString())).ToString();
        if (Session["program_id"] != null && Session["program_id"].ToString() != "" && Session["program_id"].ToString() != "0")
            Response.Write(PageUtils.GetPreHeader() + ":" + PageUtils.GetTableSingleVal("epg_channels_schedule", "NAME", int.Parse(Session["program_id"].ToString())).ToString() + " " + sCommentType);
        else
            Response.Write(PageUtils.GetPreHeader() + ": All epg program " + sCommentType);
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
        theTable += "select q.*,CASE ll.name when '---' then 'All' ELSE ll.name END as 'Valid for languages' ";
        theTable += " from (select ec.language_id,	ec.EPG_PROGRAM_ID,	ec.comment_type_id,	ec.is_active,	ec.id,	ec.status,	e.NAME as 'Program', ";
        theTable += " ec.HEADER as 'Header',	ec.SUB_HEADER as 'Sub Header',	ec.CONTENT_TEXT as 'Comment',   	ec.COMMENT_IP as 'IP',	lcs.description as 'State' ";                                                                                
        theTable += " from epg_channels_schedule e, 	lu_content_status lcs, 		epg_comments ec ";
        theTable += " where lcs.id = ec.status 		and e.id = ec.EPG_PROGRAM_ID		and e.status=1 		and ec.status=1  and " + PageUtils.GetStatusQueryPart("ec") + "and";
			if (Session["program_id"].ToString() != "0")
        {
            theTable += ODBCWrapper.Parameter.NEW_PARAM("ec.EPG_PROGRAM_ID", "=", int.Parse(Session["program_id"].ToString()));
            theTable += " and ";
        }
        theTable += ODBCWrapper.Parameter.NEW_PARAM("ec.COMMENT_TYPE_ID", "=", int.Parse(Session["comment_type_id"].ToString()));
        theTable += " and ";
        theTable += ODBCWrapper.Parameter.NEW_PARAM("ec.GROUP_ID", "=", nGroupID);
        if (Session["search_on_off"] != null && Session["search_on_off"].ToString() != "" && Session["search_on_off"].ToString() != "-1")
        {
            theTable += " and ";
            theTable += ODBCWrapper.Parameter.NEW_PARAM("ec.is_active", "=", int.Parse(Session["search_on_off"].ToString()));
        }
        if (Session["search_mc_free"] != null && Session["search_mc_free"].ToString() != "")
        {
            string sLike = " like ('%" + Session["search_mc_free"].ToString().ToLower().Trim() + "%')";
            string sL = "LTRIM(RTRIM(LOWER(ec.header)))" + sLike + " OR LTRIM(RTRIM(LOWER(ec.sub_header)))" + sLike + " OR ec.CONTENT_TEXT" + sLike;
            theTable += " and (" + sL + ")";
        }
        theTable += ")q left join lu_languages ll on ll.id=q.language_id";

        theTable.AddHiddenField("ID");
        theTable.AddHiddenField("status");
        theTable.AddHiddenField("language_id");
        theTable.AddHiddenField("media_id");
        theTable.AddHiddenField("comment_type_id");
        theTable.AddOrderByColumn("State", "ec.status");
        theTable.AddActivationField("program_comments");
        theTable.AddHiddenField("is_active");
        theTable.AddHTMLField("ec.CONTENT_TEXT");
        if (sOrderBy != "")
        {
            theTable += " order by ";
            theTable += sOrderBy;
        }
        else
            theTable += " order by q.is_active,q.id desc";
        theTable.AddTechDetails("program_comments");

        if (LoginManager.IsActionPermittedOnPage("adm_epg_channels.aspx", LoginManager.PAGE_PERMISION_TYPE.EDIT))
        {
            DataTableLinkColumn linkColumn1 = new DataTableLinkColumn("adm_epg_channels_schedule_comments_new.aspx", "Edit", "");
            linkColumn1.AddQueryStringValue("program_comment_id", "field=id");
            linkColumn1.AddQueryStringValue("program_id", "field=EPG_PROGRAM_ID");
            linkColumn1.AddQueryStringValue("comment_type_id", "field=comment_type_id");
            theTable.AddLinkColumn(linkColumn1);
        }

        if (Session["comment_type_id"] != null && Session["comment_type_id"].ToString() != "" && Session["comment_type_id"].ToString() != "0")
        {
            /*
            if (LoginManager.IsActionPermittedOnPage("adm_media.aspx", LoginManager.PAGE_PERMISION_TYPE.EDIT))
            {
                DataTableLinkColumn linkColumn1 = new DataTableLinkColumn("adm_media_comments_new.aspx", "Edit", "");
                linkColumn1.AddQueryStringValue("media_comment_id", "field=id");
                linkColumn1.AddQueryStringValue("media_id", "field=media_id");
                linkColumn1.AddQueryStringValue("comment_type_id", "field=comment_type_id");
                theTable.AddLinkColumn(linkColumn1);
            }
            */

            if (LoginManager.IsActionPermittedOnPage("adm_epg_channels.aspx", LoginManager.PAGE_PERMISION_TYPE.REMOVE))
            {
                DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_remove.aspx", "Delete", "STATUS=1;STATUS=3");
                linkColumn.AddQueryStringValue("id", "field=id");
                linkColumn.AddQueryStringValue("table", "epg_comments");
                linkColumn.AddQueryStringValue("confirm", "true");
                linkColumn.AddQueryStringValue("main_menu", "7");
                linkColumn.AddQueryStringValue("sub_menu", "1");
                linkColumn.AddQueryStringValue("rep_field", "NAME");
                linkColumn.AddQueryStringValue("rep_name", "שם");
                theTable.AddLinkColumn(linkColumn);
            }

            if (LoginManager.IsActionPermittedOnPage("adm_epg_channels.aspx", LoginManager.PAGE_PERMISION_TYPE.PUBLISH))
            {
                DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_confirm.aspx", "Confirm", "STATUS=3;STATUS=4");
                linkColumn.AddQueryStringValue("id", "field=id");
                linkColumn.AddQueryStringValue("table", "epg_comments");
                linkColumn.AddQueryStringValue("confirm", "true");
                linkColumn.AddQueryStringValue("main_menu", "7");
                linkColumn.AddQueryStringValue("sub_menu", "1");
                linkColumn.AddQueryStringValue("rep_field", "NAME");
                linkColumn.AddQueryStringValue("rep_name", "שם");
                theTable.AddLinkColumn(linkColumn);
            }

            if (LoginManager.IsActionPermittedOnPage("adm_epg_channels.aspx", LoginManager.PAGE_PERMISION_TYPE.PUBLISH))
            {
                DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_confirm.aspx", "Cancel", "STATUS=3;STATUS=4");
                linkColumn.AddQueryStringValue("id", "field=id");
                linkColumn.AddQueryStringValue("table", "epg_comments");
                linkColumn.AddQueryStringValue("confirm", "false");
                linkColumn.AddQueryStringValue("main_menu", "7");
                linkColumn.AddQueryStringValue("sub_menu", "1");
                linkColumn.AddQueryStringValue("rep_field", "NAME");
                linkColumn.AddQueryStringValue("rep_name", "שם");
                theTable.AddLinkColumn(linkColumn);
            }
        }
    }

    public string GetPageContent(string sOrderBy, string sPageNum, string search_on_off, string search_mc_free)
    {
        if (search_on_off != "-1")
            Session["search_on_off"] = search_on_off;
        else if (Session["search_save"] == null)
            Session["search_on_off"] = "";

        if (search_mc_free != "-1")
            Session["search_mc_free"] = search_mc_free;
        else if (Session["search_save"] == null)
            Session["search_mc_free"] = "";

        if (sOrderBy != "")
            Session["order_by"] = sOrderBy;
        else if (Session["search_save"] == null)
            Session["order_by"] = "";
        else
            sOrderBy = Session["order_by"].ToString();

        string sOldOrderBy = "";
        if (Session["order_by"] != null)
            sOldOrderBy = Session["order_by"].ToString();

        bool bNew = false;
        if (Session["comment_type_id"] != null && Session["comment_type_id"] != "" && int.Parse(Session["comment_type_id"].ToString()) != 0)
            bNew = true;
        DBTableWebEditor theTable = new DBTableWebEditor(true, true, bNew, "EPG_PROGRAM_ID=" + Session["program_id"].ToString() + "&comment_type_id=" + Session["comment_type_id"].ToString(), "adm_table_header", "adm_table_cell", "adm_table_alt_cell", "adm_table_link", "adm_table_pager", "adm_table", sOldOrderBy, 50);
        FillTheTableEditor(ref theTable, sOrderBy);

        string sTable = theTable.GetPageHTML(int.Parse(sPageNum), sOrderBy, false);
        Session["ContentPage"] = "adm_epg_channels.aspx";
        theTable.Finish();
        theTable = null;
        return sTable;
    }
}