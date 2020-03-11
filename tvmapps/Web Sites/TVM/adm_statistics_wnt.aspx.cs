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

public partial class adm_statistics_wnt : System.Web.UI.Page
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
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 2, false);
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

        theTable += "SELECT DATEADD(dd, 0, DATEDIFF(dd, 0, COUNT_DATE)) as 'date_to_sort' , CONVERT(varchar(11), COUNT_DATE, 103) AS 'Date' , case DATEPART(dw, COUNT_DATE) when 1 then 'Sun' when 2 then 'Mon' when 3 then 'Tue' when 4 then 'Wed' when 5 then 'Thu' when 6 then 'Fri' when 7 then 'Sat' END as 'DOW', COUNT(distinct WATCHER_ID) AS 'Unique Users', SUM(PLAY_TIME_COUNTER)/(count(distinct WATCHER_ID)+0.0)/60.0 as 'Watch Time Per User(MIN)',YEAR(COUNT_DATE) as d_y,MONTH(COUNT_DATE) as d_m,DAY(COUNT_DATE) as d_d, SUM(TIME_COUNTER)/60.0/60.0 AS 'On Site Time(Hours)', SUM(PLAY_TIME_COUNTER)/60.0/60.0 AS 'Watch Time(Hours)', CASE SUM(PLAY_TIME_COUNTER) when 0 then 0 else SUM(TIME_COUNTER)/(SUM(PLAY_TIME_COUNTER)+0.0) END as 'Site/Watch Time'";
        //theTable += " FROM  watchers_time_counters where PLAY_TIME_COUNTER>0 and ";
        theTable += " FROM  watchers_time_counters where ";
        theTable += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
        theTable += " GROUP BY YEAR(COUNT_DATE),MONTH(COUNT_DATE),DAY(COUNT_DATE),DATEPART(dw, COUNT_DATE),CONVERT(varchar(11), COUNT_DATE, 103),DATEADD(dd, 0, DATEDIFF(dd, 0, COUNT_DATE)) ";
        if (sOrderBy != "")
        {
            theTable += " order by ";
            theTable += sOrderBy;
        }
        else
            theTable += " order by YEAR(COUNT_DATE) desc,MONTH(COUNT_DATE) desc,DAY(COUNT_DATE) desc";


        theTable.AddHiddenField("d_y");
        theTable.AddHiddenField("d_m");
        theTable.AddHiddenField("d_d");
        theTable.AddHiddenField("date_to_sort");
        theTable.AddOrderByColumn("On Site Time(Hours)", "SUM(TIME_COUNTER)/60.0/60.0");
        theTable.AddOrderByColumn("Watch Time(Hours)", "SUM(PLAY_TIME_COUNTER)/60.0/60.0");
        theTable.AddOrderByColumn("DOW", "DATEPART(dw, COUNT_DATE)");
        theTable.AddOrderByColumn("Date", "DATEADD(dd, 0, DATEDIFF(dd, 0, COUNT_DATE))");
        //        theTable.AddOrderByColumn("Date", "COUNT_DATE");
    }

    public string GetPageContent(string sOrderBy, string sPageNum)
    {
        string sOldOrderBy = "";
        if (Session["order_by"] != null)
            sOldOrderBy = Session["order_by"].ToString();
        DBTableWebEditor theTable = new DBTableWebEditor(true, true, false, "", "adm_table_header", "adm_table_cell", "adm_table_alt_cell", "adm_table_link", "adm_table_pager", "adm_table", sOldOrderBy, 50);
        FillTheTableEditor(ref theTable, sOrderBy);

        string sTable = theTable.GetPageHTML(int.Parse(sPageNum), sOrderBy);

        theTable.Finish();
        theTable = null;
        return sTable;
    }

    public void GetHeader()
    {
        Response.Write(PageUtils.GetPreHeader() + ": Times Statistics");
    }
}
