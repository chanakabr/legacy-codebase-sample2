using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TVinciShared;

public partial class adm_epg_channels_schedule : System.Web.UI.Page
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
            m_sMenu = TVinciShared.Menu.GetMainMenu(6, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 3, false);
            if (Request.QueryString["search_save"] != null)
                Session["search_save"] = "1";
            else
                Session["search_save"] = null;

            if (Request.QueryString["epg_channel_id"] != null &&
                Request.QueryString["epg_channel_id"].ToString() != "")
            {
                Session["epg_channel_id"] = int.Parse(Request.QueryString["epg_channel_id"].ToString());
            }
            else if (Session["epg_channel_id"] == null || Session["epg_channel_id"].ToString() == "" || Session["epg_channel_id"].ToString() == "0")
            {
                LoginManager.LogoutFromSite("index.html");
                return;
            }
        }
    }

    public void GetHeader()
    {
        Response.Write(PageUtils.GetPreHeader() + ":" + PageUtils.GetTableSingleVal("epg_channels", "NAME", int.Parse(Session["epg_channel_id"].ToString())).ToString() + ": EPG Channel schedule");
    }

    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
    }

    protected void GetSubMenu()
    {
        Response.Write(m_sSubMenu);
    }

    public string GetTableCSV(string startD, string startM, string startY, string endD, string endM, string endY)
    {
        string sOldOrderBy = "";
        if (Session["order_by"] != null)
            sOldOrderBy = Session["order_by"].ToString();
        DBTableWebEditor theTable = new DBTableWebEditor(true, true, false, "", "adm_table_header", "adm_table_cell", "adm_table_alt_cell", "adm_table_link", "adm_table_pager", "adm_table", sOldOrderBy, 50);
        FillTheTableEditor(ref theTable, sOldOrderBy, startD, startM, startY, endD, endM, endY);

        string sCSVFile = theTable.OpenCSV();
        theTable.Finish();
        theTable = null;
        return sCSVFile;
    }

    protected void FillTheTableEditor(ref DBTableWebEditor theTable, string sOrderBy, string startD, string startM, string startY, string endD, string endM, string endY)
    {
        DateTime tStart = DateUtils.GetDateFromStr(startD + "/" + startM + "/" + startY);
        DateTime tEnd = DateUtils.GetDateFromStr(endD + "/" + endM + "/" + endY);

        theTable += "select q.is_active,q.id,q.eci,q.m_id,q.DOW,q.name as 'EPG name',q.description as 'EPG Description', q.epg_identifier as 'EPG Identifier',m.description as 'Media description',q.START_DATE as 'Start Date',q.END_DATE as 'End Date',q.status,q.State from (select ecs.epg_identifier,ecs.id,ecs.EPG_CHANNEL_ID as 'eci',ecs.id as 'm_id',CASE DATEPART (dw,ecs.START_DATE) WHEN 3 THEN 'Tue' WHEN 1 THEN 'Sun' WHEN 2 THEN 'Mon' WHEN 4 THEN 'Wen' WHEN 5 THEN 'Thu' WHEN 6 THEN 'Fri' WHEN 7 THEN 'Sat' END as 'DOW',ecs.name,ecs.description,ecs.START_DATE,ecs.END_DATE,ecs.status,lcs.description as 'State',ecs.is_active ";
        theTable += " from ";
        theTable += " lu_content_status lcs,epg_channels_schedule ecs where lcs.id=ecs.status and ecs.status=1 ";
        theTable += " and ";
        theTable += ODBCWrapper.Parameter.NEW_PARAM("ecs.EPG_CHANNEL_ID", "=", int.Parse(Session["epg_channel_id"].ToString()));
        theTable += " and ";
        theTable += ODBCWrapper.Parameter.NEW_PARAM("ecs.END_DATE", ">=", tStart);
        theTable += " and ";
        theTable += ODBCWrapper.Parameter.NEW_PARAM("ecs.START_DATE", "<=", tEnd);
        theTable += " and ";
        theTable += ODBCWrapper.Parameter.NEW_PARAM("ecs.GROUP_ID", "=", LoginManager.GetLoginGroupID());
        theTable += " )q left join media m on m.epg_identifier=q.epg_identifier order by q.start_date";
        if (LoginManager.IsActionPermittedOnPage("adm_epg_channels.aspx", LoginManager.PAGE_PERMISION_TYPE.EDIT))
        {
            DataTableLinkColumn linkColumn1 = new DataTableLinkColumn("adm_epg_channels_schedule_new.aspx", "Edit", "");
            linkColumn1.AddQueryStringValue("epg_channels_schedule_id", "field=m_id");
            linkColumn1.AddQueryStringValue("epg_channels_id", "field=eci");
            theTable.AddLinkColumn(linkColumn1);
        }
        theTable.AddHiddenField("eci");
        theTable.AddHiddenField("m_id");
        theTable.AddHiddenField("id");
        theTable.AddHiddenField("status");
        theTable.AddActivationField("epg_channels_schedule");
        theTable.AddHiddenField("is_active");



        DataTableLinkColumn linkColumnComment = new DataTableLinkColumn("adm_epg_channels_schedule_comments.aspx", "Comments", "");
        linkColumnComment.AddQueryStringValue("program_id", "field=id");
        theTable.AddLinkColumn(linkColumnComment);



        if (LoginManager.IsActionPermittedOnPage("adm_epg_channels.aspx", LoginManager.PAGE_PERMISION_TYPE.REMOVE))
        {
            DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_remove.aspx", "Delete", "STATUS=1;STATUS=3");
            linkColumn.AddQueryStringValue("id", "field=m_id");
            linkColumn.AddQueryStringValue("table", "epg_channels_schedule");
            linkColumn.AddQueryStringValue("confirm", "true");
            linkColumn.AddQueryStringValue("main_menu", "6");
            linkColumn.AddQueryStringValue("sub_menu", "3");
            linkColumn.AddQueryStringValue("rep_field", "NAME");
            linkColumn.AddQueryStringValue("rep_name", "שם");
            theTable.AddLinkColumn(linkColumn);
        }

        if (LoginManager.IsActionPermittedOnPage("adm_epg_channels.aspx", LoginManager.PAGE_PERMISION_TYPE.PUBLISH))
        {
            DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_confirm.aspx", "Confirm", "STATUS=3;STATUS=4");
            linkColumn.AddQueryStringValue("id", "field=m_id");
            linkColumn.AddQueryStringValue("table", "epg_channels_schedule");
            linkColumn.AddQueryStringValue("confirm", "true");
            linkColumn.AddQueryStringValue("main_menu", "6");
            linkColumn.AddQueryStringValue("sub_menu", "3");
            linkColumn.AddQueryStringValue("rep_field", "NAME");
            linkColumn.AddQueryStringValue("rep_name", "שם");
            theTable.AddLinkColumn(linkColumn);
        }

        if (LoginManager.IsActionPermittedOnPage("adm_epg_channels.aspx", LoginManager.PAGE_PERMISION_TYPE.PUBLISH))
        {
            DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_confirm.aspx", "Cancel", "STATUS=3;STATUS=4");
            linkColumn.AddQueryStringValue("id", "field=m_id");
            linkColumn.AddQueryStringValue("table", "epg_channels_schedule");
            linkColumn.AddQueryStringValue("confirm", "false");
            linkColumn.AddQueryStringValue("main_menu", "6");
            linkColumn.AddQueryStringValue("sub_menu", "3");
            linkColumn.AddQueryStringValue("rep_field", "NAME");
            linkColumn.AddQueryStringValue("rep_name", "שם");
            theTable.AddLinkColumn(linkColumn);
        }
    }

    public string GetPageContent(string sOrderBy, string sPageNum, string startD, string startM, string startY, string endD, string endM, string endY)
    {
        string sOldOrderBy = "";
        if (Session["order_by"] != null)
            sOldOrderBy = Session["order_by"].ToString();
        DBTableWebEditor theTable = new DBTableWebEditor(true, true, true, "", "adm_table_header", "adm_table_cell", "adm_table_alt_cell", "adm_table_link", "adm_table_pager", "adm_table", sOldOrderBy, 50);
        FillTheTableEditor(ref theTable, sOrderBy, startD, startM, startY, endD, endM, endY);

        string sTable = theTable.GetPageHTML(int.Parse(sPageNum), sOrderBy);

        theTable.Finish();
        theTable = null;
        return sTable;
    }
    /*
    public string GetPageContent(string sOrderBy, string sPageNum)
    {
        string sOldOrderBy = "";
        if (Session["order_by"] != null)
            sOldOrderBy = Session["order_by"].ToString();

        //TVinciShared.Channel channel = new TVinciShared.Channel(int.Parse(Session["epg_channel_id"].ToString()), false);
        DBTableWebEditor theTable = new DBTableWebEditor(true, true, true, "", "adm_table_header", "adm_table_cell", "adm_table_alt_cell", "adm_table_link", "adm_table_pager", "adm_table", sOldOrderBy, 50);
        FillTheTableEditor(ref theTable, sOrderBy);

        string sTable = theTable.GetPageHTML(int.Parse(sPageNum), sOrderBy);
        Session["ContentPage"] = "adm_epg_channels.aspx?search_save=1";
        theTable.Finish();
        theTable = null;
        return sTable;
    }
    */
    public void GetSearchPannel()
    {
        DateTime dStart = DateTime.UtcNow;
        DateTime dEnd = DateTime.UtcNow.AddDays(1);
        string sRet = "<tr>\r\n";
        sRet += "<td colspan=\"2\">\r\n";
        sRet += "<table>\r\n";
        sRet += "<tr>\r\n";
        sRet += "<td>&nbsp;</td>\r\n";
        sRet += "</tr>\r\n";
        sRet += "<tr>\r\n";
        sRet += "<td class=\"FormError\"><div id=\"error_place\"></div></td>\r\n";
        sRet += "</tr>\r\n";
        sRet += "<tr>\r\n";
        sRet += "<td>&nbsp;</td>\r\n";
        sRet += "</tr>\r\n";
        sRet += "<tr>\r\n";
        sRet += "<td class=\"adm_table_header_nbg\" style=\"vertical-align: middle;\">From: </td>\r\n";
        sRet += "<td><input class=\"FormInput\" type=\"text\" size=\"2\" maxlength=\"2\" value=\"" + dStart.Day.ToString() + "\" id=\"s_day\" /> </td>\r\n";
        sRet += "<td>/</td>\r\n";
        sRet += "<td><input class=\"FormInput\" type=\"text\" size=\"2\" maxlength=\"2\" id=\"s_mounth\" value=\"" + dStart.Month.ToString() + "\" /> </td>\r\n";
        sRet += "<td>/</td>\r\n";
        sRet += "<td><input class=\"FormInput\" type=\"text\" size=\"4\" maxlength=\"4\" id=\"s_year\" value=\"" + dStart.Year.ToString() + "\" /> </td>\r\n";
        sRet += "<td>&nbsp;&nbsp;&nbsp;&nbsp;</td>\r\n";
        sRet += "<td class=\"adm_table_header_nbg\"  style=\"vertical-align: middle;\">To: </td>\r\n";
        sRet += "<td><input maxlength=\"2\" class=\"FormInput\" type=\"text\" size=\"2\" id=\"s_day_to\" value=\"" + dEnd.Day.ToString() + "\" /> </td>\r\n";
        sRet += "<td>/</td>\r\n";
        sRet += "<td><input maxlength=\"2\" class=\"FormInput\" type=\"text\" size=\"2\" id=\"s_mounth_to\" value=\"" + dEnd.Month.ToString() + "\" /> </td>\r\n";
        sRet += "<td>/</td>\r\n";
        sRet += "<td><input maxlength=\"4\" class=\"FormInput\" type=\"text\" size=\"4\" id=\"s_year_to\" value=\"" + dEnd.Year.ToString() + "\" /> </td>\r\n";
        sRet += "<td>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</td>\r\n";
        sRet += "<td><a href=\"javascript:reloadPage();\" class=\"btn\">Go</a></td>\r\n";
        sRet += "</tr>\r\n";
        sRet += "</table>\r\n";
        sRet += "</td>\r\n";
        sRet += "</tr>\r\n";
        Response.Write(sRet);
    }
}
