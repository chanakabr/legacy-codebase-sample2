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

public partial class adm_statistics_wpd : System.Web.UI.Page
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
            m_sMenu = TVinciShared.Menu.GetMainMenu(10, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 5, false);
            if (Request.QueryString["search_save"] != null)
                Session["search_save"] = "1";
            else
                Session["search_save"] = null;
        }
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
        FillTheTableEditor(ref theTable, sOldOrderBy , startD, startM, startY, endD, endM, endY);

        string sCSVFile = theTable.OpenCSV();
        theTable.Finish();
        theTable = null;
        return sCSVFile;
    }

    protected void GetRelevantDates(DateTime tStart, DateTime tEnd, ref Int32 nStart, ref Int32 nEnd)
    {
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select max(id) as max_id, min(id) as min_id from media_eoh_fact where ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("START_HOUR_DATE", ">=", tStart);
        selectQuery += " and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("START_HOUR_DATE", "<", tEnd);
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
            {
                object obj = selectQuery.Table("query").DefaultView[0].Row["max_id"];
                if (obj != null && obj != DBNull.Value)
                    nEnd = int.Parse(selectQuery.Table("query").DefaultView[0].Row["max_id"].ToString());

                object obj1 = selectQuery.Table("query").DefaultView[0].Row["min_id"];
                if (obj1 != null && obj1 != DBNull.Value)
                    nStart = int.Parse(selectQuery.Table("query").DefaultView[0].Row["min_id"].ToString());
            }
        }
        selectQuery.Finish();
        selectQuery = null;
    }

    protected void FillTheTableEditor(ref DBTableWebEditor theTable, string sOrderBy, string startD, string startM, string startY, string endD, string endM, string endY)
    {
        Int32 nGroupID = LoginManager.GetLoginGroupID();

        DateTime tStart = DateUtils.GetDateFromStr(startD + "/" + startM + "/" + startY);
        DateTime tEnd = DateUtils.GetDateFromStr(endD + "/" + endM + "/" + endY);
        Int32 nStart = 0;
        Int32 nEnd = 0;
        GetRelevantDates(tStart, tEnd, ref nStart, ref nEnd);

        theTable += "SELECT  CAST(YEAR( media_eoh_fact.CREATE_DATE) AS nvarchar(4)) + '/' + CAST(MONTH(media_eoh_fact.CREATE_DATE) AS nvarchar(2)) + '/' + CAST(DAY(media_eoh_fact.CREATE_DATE) AS nvarchar(2))as 'Day'  ";
        //theTable += ",lu_media_quality.description as 'Quality' ";
        //theTable += ",lu_media_types.description as 'Type' ";
        //theTable += ",case DATEPART(dw, CREATE_DATE) when 1 then 'Sun' when 2 then 'Mon' when 3 then 'Tue' when 4 then 'Wed' when 5 then 'Thu' when 6 then 'Fri' when 7 then 'Sat' END as 'DOW', COUNT(DISTINCT media_eoh.WATCHER_ID) AS 'distincts watcher', COUNT(DISTINCT media_eoh.SESSION_ID) AS 'distinct sessions' "; 
        theTable += ", SUM(media_eoh_fact.PLAY_TIME_COUNTER) AS 'total play time', SUM(media_eoh_fact.LOAD_COUNTER) AS 'loads', SUM(media_eoh_fact.FIRST_PLAY_COUNTER) AS 'first plays', SUM(media_eoh_fact.PLAY_COUNTER) AS 'plays' ";
        theTable += ", SUM(media_eoh_fact.PAUSE_COUNTER) AS 'pause', SUM(media_eoh_fact.STOP_COUNTER) AS 'stop', SUM(media_eoh_fact.FINISH_COUNTER) AS 'finish' ";
        theTable += ", SUM(media_eoh_fact.FULL_SCREEN_COUNTER) AS 'full', SUM(media_eoh_fact.EXIT_FULL_SCREEN_COUNTER) AS 'exit full' ";
        theTable += ", SUM(media_eoh_fact.SEND_TO_FRIEND_COUNTER) AS 'send to friend'";
        theTable += ", count(distinct media_eoh_fact.WATCHER_ID) AS 'distinct watchers'";
        theTable += ", count(distinct media_eoh_fact.SESSION_ID) AS 'distinct sessions'";

        //theTable += "  FROM media_eoh,lu_media_types,lu_media_quality WHERE lu_media_quality.id=FILE_QUALITY_ID and lu_media_types.id=FILE_FORMAT_ID and ";
        theTable += "  FROM media_eoh_fact WHERE  ";
        theTable += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
        theTable += " and ";
        theTable += "(";
        theTable += ODBCWrapper.Parameter.NEW_PARAM("media_eoh_fact.ID", ">=", nStart);
        theTable += " ) AND (";
        theTable += ODBCWrapper.Parameter.NEW_PARAM("media_eoh_fact.ID", "<", nEnd);
        theTable += ") ";
        theTable += "GROUP BY YEAR(media_eoh_fact.CREATE_DATE),MONTH(media_eoh_fact.CREATE_DATE),DAY(media_eoh_fact.CREATE_DATE)";
        
        //theTable += ",lu_media_quality.description,lu_media_types.description";
        if (sOrderBy != "")
        {
            theTable += " order by ";
            theTable += sOrderBy;
        }
        else
            theTable += " order by YEAR(media_eoh_fact.CREATE_DATE) desc,MONTH(media_eoh_fact.CREATE_DATE) desc,DAY(media_eoh_fact.CREATE_DATE) desc";
        
        //theTable.AddHiddenField("date_to_sort");
        //theTable.AddOrderByColumn("Num Of Watches", "COUNT(*)");
        //theTable.AddOrderByColumn("DOW", "DATEPART(dw, CREATE_DATE)");
        //theTable.AddOrderByColumn("Date", "DATEADD(dd, 0, DATEDIFF(dd, 0, CREATE_DATE))");
//        theTable.AddOrderByColumn("Date", "CREATE_DATE");
    }

    public void GetSearchPannel()
    {
        DateTime dStart = DateTime.Now.AddMonths(-1);
        DateTime dEnd = DateTime.Now.AddDays(1);
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

    public string GetPageContent(string sOrderBy, string sPageNum, string startD, string startM, string startY, string endD, string endM, string endY)
    {
        string sOldOrderBy = "";
        if (Session["order_by"] != null)
            sOldOrderBy = Session["order_by"].ToString();
        DBTableWebEditor theTable = new DBTableWebEditor(true, true, false, "", "adm_table_header", "adm_table_cell", "adm_table_alt_cell", "adm_table_link", "adm_table_pager", "adm_table", sOldOrderBy, 50);
        FillTheTableEditor(ref theTable, sOrderBy, startD, startM, startY, endD, endM, endY);

        string sTable = theTable.GetPageHTML(int.Parse(sPageNum), sOrderBy);

        theTable.Finish();
        theTable = null;
        return sTable;
    }

    public void GetHeader()
    {
        Response.Write(PageUtils.GetPreHeader() + ": Watch Per Day Statistics");
    }
}
