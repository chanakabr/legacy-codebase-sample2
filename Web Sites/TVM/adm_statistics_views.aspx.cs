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

public partial class adm_statistics_views : System.Web.UI.Page
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
            string sQueryString = "";
            string[] allKeys = Request.QueryString.AllKeys;
            for (int i = 0; i < allKeys.Length; i++)
            {
                if (i > 0)
                    sQueryString += "&";
                else
                    sQueryString += "?";
                sQueryString += Request.QueryString.AllKeys[i].ToString() + "=" + Request.QueryString[Request.QueryString.AllKeys[i].ToString()];
            }
            Session["query_string"] = sQueryString;
            Int32 nMenuID = 0;
            Session["media_id"] = null;
            Session["channel_id"] = null;
            Session["player_id"] = null;
            if (Request.QueryString["media_id"] != null &&
                Request.QueryString["media_id"].ToString() != "")
            {
                m_sMenu = TVinciShared.Menu.GetMainMenu(7, true, ref nMenuID);
                m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 2, false);
                Session["media_id"] = int.Parse(Request.QueryString["media_id"].ToString());
                Int32 nOwnerGroupID = int.Parse(PageUtils.GetTableSingleVal("media", "group_id", int.Parse(Session["media_id"].ToString())).ToString());
                Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();

                if (PageUtils.GetUpperGroupID(nOwnerGroupID) != PageUtils.GetUpperGroupID(nLogedInGroupID) && PageUtils.IsTvinciUser() == false)
                {
                    LoginManager.LogoutFromSite("login.html");
                    return;
                }
            }
            else if (Request.QueryString["channel_id"] != null &&
                Request.QueryString["channel_id"].ToString() != "")
            {
                m_sMenu = TVinciShared.Menu.GetMainMenu(6, true, ref nMenuID);
                m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, false);
                Session["channel_id"] = int.Parse(Request.QueryString["channel_id"].ToString());
                Int32 nOwnerGroupID = int.Parse(PageUtils.GetTableSingleVal("channels", "group_id", int.Parse(Session["channel_id"].ToString())).ToString());
                Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();

                if (PageUtils.GetUpperGroupID(nOwnerGroupID) != PageUtils.GetUpperGroupID(nLogedInGroupID) && PageUtils.IsTvinciUser() == false)
                {
                    LoginManager.LogoutFromSite("login.html");
                    return;
                }
            }
            else if (Request.QueryString["player_id"] != null &&
                Request.QueryString["player_id"].ToString() != "")
            {
                m_sMenu = TVinciShared.Menu.GetMainMenu(3, true, ref nMenuID);
                m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, false);
                Session["player_id"] = int.Parse(Request.QueryString["player_id"].ToString());
                Int32 nOwnerGroupID = int.Parse(PageUtils.GetTableSingleVal("groups_passwords", "group_id", int.Parse(Session["player_id"].ToString())).ToString());
                Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();

                if (PageUtils.GetUpperGroupID(nOwnerGroupID) != PageUtils.GetUpperGroupID(nLogedInGroupID) && PageUtils.IsTvinciUser() == false)
                {
                    LoginManager.LogoutFromSite("login.html");
                    return;
                }
            }
            else
            {
                m_sMenu = TVinciShared.Menu.GetMainMenu(10, true, ref nMenuID);
                m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 2, false);
            }
            if (Request.QueryString["start_date"] != null)
            {
                DateTime tStart = DateUtils.GetDateFromStr(Request.QueryString["start_date"].ToString());
                Session["start_date"] = tStart;
            }
            else
                Session["start_date"] = null;

            if (Request.QueryString["end_date"] != null)
            {
                DateTime tEnd = DateUtils.GetDateFromStr(Request.QueryString["end_date"].ToString());
                Session["end_date"] = tEnd;
            }
            else
                Session["end_date"] = null;

            if (Request.QueryString["search_save"] != null)
                Session["search_save"] = "1";
            else
                Session["search_save"] = null;
        }
    }

    public void GetTotalMediaViews(Int32 nDays)
    {
        if (Session["media_id"] != null || Session["channel_id"] != null || Session["player_id"] != null)
            return ;
        Int32 nMaxViews = 0;
        string sBar = "";
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select  TOP (10) count(*) as co,m.name,m.id from watchers_media_actions wma,media m where m.id=wma.media_ID and wma.action_id=1 and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("m.group_id", "=", LoginManager.GetLoginGroupID());
        if (nDays > 0)
        {
            selectQuery += "and";
            string sStartDay = DateTime.Now.Date.ToString();
            if (nDays == 31)
                selectQuery += "wma.create_date>DATEADD(m, -1, CONVERT(datetime, '" + sStartDay + "'))";
            else
                selectQuery += "wma.create_date>DATEADD(d, " + (nDays * -1).ToString() + ", CONVERT(datetime, '" + sStartDay + "'))";
        }
        selectQuery += " group by m.id,m.name  ORDER BY co DESC";
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            for (int i = 0; i < nCount; i++)
            {
                Int32 nViews = int.Parse(selectQuery.Table("query").DefaultView[i].Row["co"].ToString());
                string sName = selectQuery.Table("query").DefaultView[i].Row["name"].ToString();
                string sID = selectQuery.Table("query").DefaultView[i].Row["id"].ToString();
                if (i == 0)
                    nMaxViews = nViews;
                Int32 nXRight = (i * 3) + 3;
                Int32 nXLeft = (i * 3) + 1;
                sBar += "new Bar(D.ScreenX(" + nXLeft.ToString() + "),D.ScreenY(" + nViews.ToString() + "),D.ScreenX(" + nXRight.ToString() + "),D.ScreenY(0), \"#66ccff\" , \"\",\"#666666\",\"" + sName + " (" + nViews.ToString() + ")\",\"GetMediaStatistics(" + sID + ");\");";
            }
        }
        selectQuery.Finish();
        selectQuery = null;
        string sRet = "";
        sRet += "var D=new Diagram();\r\n";
        sRet += "D.SetFrame(70, 30, 380, 200);\r\n";
        if (nDays == 31)
            sRet += "D.SetText(\"\" ,\"\" , \"Top 10 Media - Last Month\");\r\n";
        if (nDays == 7)
            sRet += "D.SetText(\"\" ,\"\" , \"Top 10 Media - Last Week\");\r\n";
        if (nDays == 1)
            sRet += "D.SetText(\"\" ,\"\" , \"Top 10 Media - Last Day\");\r\n";
        if (nDays == 0)
            sRet += "D.SetText(\"\" ,\"\" , \"Top 10 Media\");\r\n";
        sRet += "D.XScale=0;\r\n";
        sRet += "D.SetGridColor(\"#cccccc\");\r\n";
        Int32 nAddToMax = 1;
        if (nMaxViews > 10)
            nAddToMax = 3;
        if (nMaxViews > 100)
            nAddToMax = 10;
        sRet += "D.SetBorder(0, 31, 0, " + (nMaxViews + nAddToMax).ToString() + ");\r\n";
        sRet += "D.Draw(\"#FFFFFF\",\"#666666\",false);\r\n";
        //sRet += "for (var i=15; i<50; i+=5)\r\n";
        sRet += sBar;
        Response.Write(sRet);
    }

    public void GetPageParameters()
    {
        if (Session["query_string"] != null)
            Response.Write(Session["query_string"].ToString());
    }

    public void GetHeader()
    {
        string sName = "";
        if (Session["media_id"] != null)
            sName = ": Media Statistics: " + PageUtils.GetTableSingleVal("media", "name", int.Parse(Session["media_id"].ToString())).ToString();
        else if (Session["channel_id"] != null)
            sName = ": Channel Statistics: " + PageUtils.GetTableSingleVal("channels", "name", int.Parse(Session["channel_id"].ToString())).ToString();
        else if (Session["player_id"] != null)
            sName = ": Player Statistics: " + PageUtils.GetTableSingleVal("groups_passwords", "DOMAIN", int.Parse(Session["player_id"].ToString())).ToString();
        else
            sName = ": Statistics for all";
        Response.Write(PageUtils.GetPreHeader() + sName);
    }

    protected void GetGEOSummeryForPeriod(bool bUnique, Int32 nDaysAgo, ref System.Collections.Specialized.NameValueCollection theCountries)
    {
        Int32 nTotal = 0;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select c.country_name,count(";
        if (bUnique == true)
            selectQuery += "distinct watcher_id";
        else
            selectQuery += "*";
        selectQuery += ") as co from watchers_media_play_counters wmpc,countries c ";
        selectQuery += " where ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("wmpc.group_id", "=", LoginManager.GetLoginGroupID());
        if (nDaysAgo > 0)
        {
            selectQuery += " and wmpc.create_date>";
            string sStartDay = DateTime.Now.Date.ToString();
            if (nDaysAgo == 31)
                selectQuery += "DATEADD(m, -1, '" + sStartDay + "') ";
            else
                selectQuery += "DATEADD(d, " + (nDaysAgo * -1).ToString() + " , CONVERT(datetime, '" + sStartDay + "')) ";
        }
        if (Session["media_id"] != null)
        {
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("wmpc.media_id", "=", int.Parse(Session["media_id"].ToString()));
        }
        else if (Session["channel_id"] != null)
        {
            selectQuery += "and";
            selectQuery += " wmpc.media_id in (" + GetMediaIDsForChannel() + ")";
        }
        else if (Session["player_id"] != null)
        {
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("wmpc.PLAYER_ID", "=", int.Parse(Session["player_id"].ToString()));
        }
        selectQuery += "and";
        selectQuery += " c.id=wmpc.country_id group by c.country_name order by co desc";
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            for (int i = 0; i < nCount; i++)
            {
                string sCountryName = selectQuery.Table("query").DefaultView[i].Row["country_name"].ToString();
                Int32 nCO = int.Parse(selectQuery.Table("query").DefaultView[i].Row["co"].ToString());
                nTotal += nCO;
            }
            for (int i = 0; i < nCount; i++)
            {
                string sCountryName = selectQuery.Table("query").DefaultView[i].Row["country_name"].ToString();
                Int32 nCO = int.Parse(selectQuery.Table("query").DefaultView[i].Row["co"].ToString());
                double dPer = Math.Round(((double)nCO / (double)nTotal) * 100, 2);
                if (dPer > 0)
                {
                    theCountries[sCountryName] += "<td ";
                    if (nDaysAgo == 1)
                        theCountries[sCountryName] += " onmouseout=\"P1[" + i.ToString() + "].MoveTo('','',0);\" onmouseover=\"P1[" + i.ToString() + "].MoveTo('','',10);\" ";
                    if (nDaysAgo == 31)
                        theCountries[sCountryName] += " onmouseout=\"P31[" + i.ToString() + "].MoveTo('','',0);\" onmouseover=\"P31[" + i.ToString() + "].MoveTo('','',10);\" ";
                    theCountries[sCountryName] += " style=\"font-size: 12px;text-align: center;\">" + dPer.ToString() + "%</td>";
                }
            }
        }
        selectQuery.Finish();
        selectQuery = null;
    }

    protected string GetMediaIDsForChannel()
    {
        if (HttpContext.Current.Cache["GetMediaIDsForChannel" + Session["channel_id"].ToString()] != null)
            return HttpContext.Current.Cache["GetMediaIDsForChannel" + Session["channel_id"].ToString()].ToString();
        TVinciShared.Channel c = new TVinciShared.Channel(int.Parse(Session["channel_id"].ToString()), false, 0, true, 0, 0);
        string s = c.GetChannelMediaIDs();
        if (s == "")
            s = "0";
        HttpContext.Current.Cache["GetMediaIDsForChannel" + Session["channel_id"].ToString()] = s;
        return s;
    }

    public void GetWatchesSummery()
    {
        string sRet = "<table width=100%><tr><td width=\"100%\" align=\"center\" class=\"adm_table_header\">Views Summary</td></tr>";
        sRet += "<tr><td width=\"100%\" nowrap>";
        sRet += "<table width=\"100%\" style=\"border: solid 1px #cccccc;\">";
        sRet += "<tr><th style=\"font-size: 12px;width: 28%;\"></th><th style=\"font-size: 12px;width: 18%;\">Total</th><th style=\"font-size: 12px;width: 18%;\">Last Month</th><th style=\"font-size: 12px;width: 18%;\">Last Week</th><th style=\"font-size: 12px;width: 18%;\">Yesterday</th><th style=\"font-size: 12px;width: 18%;\">Today</th></tr>";
        Int32 nWatchCountToday = GetNumberOfWatches(false, -1);
        Int32 nWatchCountDay = GetNumberOfWatches(false, 1);
        Int32 nWatchCountWeek = GetNumberOfWatches(false, 7);
        Int32 nWatchCountMonth = GetNumberOfWatches(false, 31);
        Int32 nWatchCountTotal = GetNumberOfWatches(false, 0);

        Int32 nWatchCountUniqueToday = GetNumberOfWatches(true, -1);
        Int32 nWatchCountUniqueDay = GetNumberOfWatches(true, 1);
        Int32 nWatchCountUniqueWeek = GetNumberOfWatches(true, 7);
        Int32 nWatchCountUniqueMonth = GetNumberOfWatches(true, 31);
        Int32 nWatchCountUniqueTotal = GetNumberOfWatches(true, 0);

        double dAvgTodayWatchPerUnique = Math.Round(nWatchCountToday / (double)nWatchCountUniqueToday, 2);
        double dAvgDayWatchPerUnique = Math.Round(nWatchCountDay / (double)nWatchCountUniqueDay, 2);
        double dAvgWeekWatchPerUnique = Math.Round(nWatchCountWeek / (double)nWatchCountUniqueWeek, 2);
        double dAvgMonthWatchPerUnique = Math.Round(nWatchCountMonth / (double)nWatchCountUniqueMonth, 2);
        double dAvgTotalWatchPerUnique = Math.Round(nWatchCountTotal / (double)nWatchCountUniqueTotal, 2);

        double nWatchTimeToday = GetTimeOfWatches(-1);
        double nWatchTimeDay = GetTimeOfWatches(1);
        double nWatchTimeWeek = GetTimeOfWatches(7);
        double nWatchTimeMonth = GetTimeOfWatches(31);
        double nWatchTimeTotal = GetTimeOfWatches(0);

        double dAvgTodayWatchCount = Math.Round(nWatchTimeToday / (double)nWatchCountToday, 2);
        double dAvgDayWatchCount = Math.Round(nWatchTimeDay / (double)nWatchCountDay, 2);
        double dAvgWeekWatchCount = Math.Round(nWatchTimeWeek / (double)nWatchCountWeek, 2);
        double dAvgMonthWatchCount = Math.Round(nWatchTimeMonth / (double)nWatchCountMonth, 2);
        double dAvgTotalWatchCount = Math.Round(nWatchTimeTotal / (double)nWatchCountTotal, 2);

        double dAvgTodayWatchCountUnique = Math.Round(nWatchTimeToday / (double)nWatchCountUniqueToday, 2);
        double dAvgDayWatchCountUnique = Math.Round(nWatchTimeDay / (double)nWatchCountUniqueDay, 2);
        double dAvgWeekWatchCountUnique = Math.Round(nWatchTimeWeek / (double)nWatchCountUniqueWeek, 2);
        double dAvgMonthWatchCountUnique = Math.Round(nWatchTimeMonth / (double)nWatchCountUniqueMonth, 2);
        double dAvgTotalWatchCountUnique = Math.Round(nWatchTimeTotal / (double)nWatchCountUniqueTotal, 2);

        sRet += "<tr><th style=\"font-size: 12px;\">Views Count</th><td style=\"font-size: 12px;text-align: center;\">" + String.Format("{0:0.##}", nWatchCountTotal) + "</td><td style=\"font-size: 12px;text-align: center;\">" + String.Format("{0:0.##}", nWatchCountMonth) + "</td><td style=\"font-size: 12px;text-align: center;\">" + String.Format("{0:0.##}", nWatchCountWeek) + "</td><td style=\"font-size: 12px;text-align: center;\">" + String.Format("{0:0.##}", nWatchCountDay) + "</td><td style=\"font-size: 12px;text-align: center;\">" + String.Format("{0:0.##}", nWatchCountToday) + "</td></tr>";
        sRet += "<tr><th style=\"font-size: 12px;\">Unique Count</th><td style=\"font-size: 12px;text-align: center;\">" + String.Format("{0:0.##}", nWatchCountUniqueTotal) + "</td><td style=\"font-size: 12px;text-align: center;\">" + String.Format("{0:0.##}", nWatchCountUniqueMonth) + "</td><td style=\"font-size: 12px;text-align: center;\">" + String.Format("{0:0.##}", nWatchCountUniqueWeek) + "</td><td style=\"font-size: 12px;text-align: center;\">" + String.Format("{0:0.##}", nWatchCountUniqueDay) + "</td><td style=\"font-size: 12px;text-align: center;\">" + String.Format("{0:0.##}", nWatchCountUniqueToday) + "</td></tr>";
        sRet += "<tr><th style=\"font-size: 12px;\">Watch Per Unique</th><td style=\"font-size: 12px;text-align: center;\">" + String.Format("{0:0.##}", dAvgTotalWatchPerUnique) + "</td><td style=\"font-size: 12px;text-align: center;\">" + String.Format("{0:0.##}", dAvgMonthWatchPerUnique) + "</td><td style=\"font-size: 12px;text-align: center;\">" + String.Format("{0:0.##}", dAvgWeekWatchPerUnique) + "</td><td style=\"font-size: 12px;text-align: center;\">" + String.Format("{0:0.##}", dAvgDayWatchPerUnique) + "</td><td style=\"font-size: 12px;text-align: center;\">" + String.Format("{0:0.##}", dAvgTodayWatchPerUnique) + "</td></tr>";
        sRet += "<tr><th style=\"font-size: 12px;\">Watch Time (Min)</th><td style=\"font-size: 12px;text-align: center;\">" + String.Format("{0:0.##}", nWatchTimeTotal) + "</td><td style=\"font-size: 12px;text-align: center;\">" + String.Format("{0:0.##}", nWatchTimeMonth) + "</td><td style=\"font-size: 12px;text-align: center;\">" + String.Format("{0:0.##}", nWatchTimeWeek) + "</td><td style=\"font-size: 12px;text-align: center;\">" + String.Format("{0:0.##}", nWatchTimeDay) + "</td><td style=\"font-size: 12px;text-align: center;\">" + String.Format("{0:0.##}", nWatchTimeToday) + "</td></tr>";
        sRet += "<tr><th style=\"font-size: 12px;\">Avg Watch Time (Min)</th><td style=\"font-size: 12px;text-align: center;\">" + String.Format("{0:0.##}", dAvgTotalWatchCount) + "</td><td style=\"font-size: 12px;text-align: center;\">" + String.Format("{0:0.##}", dAvgMonthWatchCount) + "</td><td style=\"font-size: 12px;text-align: center;\">" + String.Format("{0:0.##}", dAvgWeekWatchCount) + "</td><td style=\"font-size: 12px;text-align: center;\">" + String.Format("{0:0.##}", dAvgDayWatchCount) + "</td><td style=\"font-size: 12px;text-align: center;\">" + String.Format("{0:0.##}", dAvgTodayWatchCount) + "</td></tr>";
        sRet += "<tr><th style=\"font-size: 12px;\">Avg Unique Watch Time (Min)</th><td style=\"font-size: 12px;text-align: center;\">" + String.Format("{0:0.##}", dAvgTotalWatchCountUnique) + "</td><td style=\"font-size: 12px;text-align: center;\">" + String.Format("{0:0.##}", dAvgMonthWatchCountUnique) + "</td><td style=\"font-size: 12px;text-align: center;\">" + String.Format("{0:0.##}", dAvgWeekWatchCountUnique) + "</td><td style=\"font-size: 12px;text-align: center;\">" + String.Format("{0:0.##}", dAvgDayWatchCountUnique) + "</td><td style=\"font-size: 12px;text-align: center;\">" + String.Format("{0:0.##}", dAvgTodayWatchCountUnique) + "</td></tr>";
        //sRet += "</tr>";
        sRet += "</table>";
        sRet += "</td></tr>";
        sRet += "</table>";
        Response.Write(sRet);
    }

    protected Int32 GetNumberOfWatches(bool bUnique, Int32 nDaysAgo)
    {
        Int32 nRet = 0;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select  count(";
        if (bUnique == true)
            selectQuery += "distinct watcher_id";
        else
            selectQuery += "*";
        selectQuery += ") as co from watchers_media_actions wma where wma.action_id=1  ";
        string sStartDay = DateTime.Now.Date.ToString();
        if (nDaysAgo > 0)
        {
            selectQuery += "and wma.create_date>=";
            
            if (nDaysAgo == 31)
                selectQuery += "DATEADD(m, -1, CONVERT(datetime, '" + sStartDay + "')) ";
            else
                selectQuery += "DATEADD(d, " + (nDaysAgo * -1).ToString() + " , CONVERT(datetime, '" + sStartDay + "')) ";

            selectQuery += "and wma.create_date<";
            selectQuery += "CONVERT(datetime, '" + sStartDay + "') ";
        }
        if (nDaysAgo == -1)
        {
            selectQuery += "and wma.create_date>";
            selectQuery += "CONVERT(datetime, '" + sStartDay + "') ";
        }
        selectQuery += " and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("wma.group_id", "=", LoginManager.GetLoginGroupID());
        if (Session["media_id"] != null)
        {
            selectQuery += "and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("wma.media_id", "=", int.Parse(Session["media_id"].ToString()));
        }
        else if (Session["channel_id"] != null)
        {
            selectQuery += "and ";
            selectQuery += " wma.media_id in (" + GetMediaIDsForChannel() + ") ";
        }
        else if (Session["player_id"] != null)
        {
            selectQuery += "and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("wma.PLAYER_ID", "=", int.Parse(Session["player_id"].ToString()));
        }
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
                nRet = int.Parse(selectQuery.Table("query").DefaultView[0].Row["co"].ToString());
        }
        selectQuery.Finish();
        selectQuery = null;
        return nRet;
    }

    protected double GetTimeOfWatches(Int32 nDaysAgo)
    {
        Int32 nRet = 0;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select  ISNULL(sum(PLAY_TIME_COUNTER),0) as co";
        selectQuery += " from watchers_media_play_counters wma where ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("wma.group_id", "=", LoginManager.GetLoginGroupID());
        string sStartDay = DateTime.Now.Date.ToString();
        if (nDaysAgo > 0)
        {
            selectQuery += " and wma.create_date>";
            if (nDaysAgo == 31)
                selectQuery += "DATEADD(m, -1, CONVERT(datetime, '" + sStartDay + "')) ";
            else
                selectQuery += "DATEADD(d, " + (nDaysAgo * -1).ToString() + " , CONVERT(datetime, '" + sStartDay + "')) ";

            selectQuery += "and wma.create_date<";
            selectQuery += "CONVERT(datetime, '" + sStartDay + "') ";
        }
        if (nDaysAgo == -1)
        {
            selectQuery += " and wma.create_date>";
            selectQuery += "CONVERT(datetime, '" + sStartDay + "') ";
        }
        if (nDaysAgo == 0)
        {
            selectQuery += " and wma.create_date>getdate()-1000 ";
        }
        if (Session["media_id"] != null)
        {
            selectQuery += "and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("wma.media_id", "=", int.Parse(Session["media_id"].ToString()));
        }
        else if (Session["channel_id"] != null)
        {
            selectQuery += "and ";
            selectQuery += " wma.media_id in (" + GetMediaIDsForChannel() + ") ";
        }
        else if (Session["player_id"] != null)
        {
            selectQuery += "and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("wma.PLAYER_ID", "=", int.Parse(Session["player_id"].ToString()));
        }
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
                nRet = int.Parse(selectQuery.Table("query").DefaultView[0].Row["co"].ToString());
        }
        selectQuery.Finish();
        selectQuery = null;
        return ((double)nRet) / 60;
    }
    /*
    protected double GetTimeOfWatches(Int32 nDaysAgo)
    {
        Int32 nRet = 0;
        bool bWhere = true;
        bool bAnd = false;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select  ISNULL(sum(PLAY_TIME_COUNTER),0) as co";
        selectQuery += " from watchers_media_play_counters wma ";
        if (nDaysAgo > 0)
        {
            if (bWhere == true)
                selectQuery += "where";
            bWhere = false;
            selectQuery += " wma.create_date>";
            if (nDaysAgo == 31)
                selectQuery += "DATEADD(m, -1, CONVERT(datetime, LEFT(CONVERT(char(10), getdate(), 20) , 10))) ";
            else
                selectQuery += "DATEADD(d, " + (nDaysAgo * -1).ToString() + " , CONVERT(datetime, LEFT(CONVERT(char(10), getdate(), 20) , 10))) ";
            bAnd = true;
        }
        if (Session["media_id"] != null)
        {
            if (bWhere == true)
                selectQuery += "where";
            bWhere = false;
            if (bAnd == true)
                selectQuery += "and ";
            bAnd = true;
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("wma.media_id", "=", int.Parse(Session["media_id"].ToString()));
        }
        else if (Session["channel_id"] != null)
        {
            if (bWhere == true)
                selectQuery += "where";
            bWhere = false;

            if (bAnd == true)
                selectQuery += "and ";
            bAnd = true;
            selectQuery += " wma.media_id in (" + GetMediaIDsForChannel() + ") ";
        }
        else if (Session["player_id"] != null)
        {
            if (bWhere == true)
                selectQuery += "where";
            bWhere = false;
            if (bAnd == true)
                selectQuery += "and ";
            bAnd = true;
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("wma.PLAYER_ID", "=", int.Parse(Session["player_id"].ToString()));
        }
        if (bWhere == true)
            selectQuery += "where";
        bWhere = false;
        if (bAnd == true)
            selectQuery += "and ";
        bAnd = true;
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("wma.group_id", "=", LoginManager.GetLoginGroupID());
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
                nRet = int.Parse(selectQuery.Table("query").DefaultView[0].Row["co"].ToString());
        }
        selectQuery.Finish();
        selectQuery = null;
        return ((double)nRet) / 60;
    }
    */
    public void GetMediaMounthViewsCountStatistics()
    {
        Int32 nMaxViews = 0;
        string sBar = "";
        string sDot = "";
        string sName = "";
        if (Session["media_id"] != null)
            sName = ": Media Statistics: " + PageUtils.GetTableSingleVal("media", "name", int.Parse(Session["media_id"].ToString())).ToString();
        else if (Session["channel_id"] != null)
            sName = ": Channel Statistics: " + PageUtils.GetTableSingleVal("channels", "name", int.Parse(Session["channel_id"].ToString())).ToString();
        else if (Session["player_id"] != null)
            sName = ": Player Statistics: " + PageUtils.GetTableSingleVal("groups_passwords", "DOMAIN", int.Parse(Session["player_id"].ToString())).ToString();
        else
            sName = ": Statistics for all";
        //string sID = Session["media_id"].ToString();
        Int32 nCount = 0;
        string sStartDay = DateTime.Now.Date.ToString();
        ODBCWrapper.DataSetSelectQuery selectQuery1 = new ODBCWrapper.DataSetSelectQuery();
        selectQuery1 += "select ISNULL(q2.co,0) as co,q1.DD,q1.MM from ";
        selectQuery1 += "(select distinct DatePart(Dd,wma.create_date) as DD,DatePart(Mm,wma.create_date) as MM,DatePart(YYYY,wma.create_date) as YYYY  from watchers_media_actions wma where wma.action_id=1 and wma.create_date>DATEADD(m, -1, CONVERT(datetime, '" + sStartDay + "')))q1 left join ";
        selectQuery1 += "(select  ISNULL(count(distinct watcher_id),0) as co,DatePart(Dd,wma.create_date) as DD,DatePart(Mm,wma.create_date) as MM,DatePart(YYYY,wma.create_date) as YYYY  from watchers_media_actions wma where wma.action_id=1 and wma.create_date>DATEADD(m, -1, CONVERT(datetime, '" + sStartDay + "')) and ";
        selectQuery1 += ODBCWrapper.Parameter.NEW_PARAM("wma.group_id", "=", LoginManager.GetLoginGroupID());
        if (Session["media_id"] != null)
        {
            selectQuery1 += "and";
            selectQuery1 += ODBCWrapper.Parameter.NEW_PARAM("wma.media_id", "=", int.Parse(Session["media_id"].ToString()));
        }
        else if (Session["channel_id"] != null)
        {
            selectQuery1 += "and";
            selectQuery1 += " wma.media_id in (" + GetMediaIDsForChannel() + ") ";
        }
        else if (Session["player_id"] != null)
        {
            //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("wma.PLAYER_ID", "=", int.Parse(Session["player_id"].ToString()));
        }

        selectQuery1 += " group by DatePart(Dd,wma.create_date),DatePart(Mm,wma.create_date),DatePart(YYYY,wma.create_date))q2 on (q1.DD=q2.DD and q1.YYYY=q2.YYYY and q1.MM=q2.MM) ORDER BY q1.YYYY,q1.MM,q1.DD";
        if (selectQuery1.Execute("query", true) != null)
        {
            nCount = selectQuery1.Table("query").DefaultView.Count;
            Int32 nLastViews = 0;
            for (int i = 0; i < nCount; i++)
            {
                Int32 nViews = int.Parse(selectQuery1.Table("query").DefaultView[i].Row["co"].ToString());
                string sDate = selectQuery1.Table("query").DefaultView[i].Row["DD"].ToString() + "/" + selectQuery1.Table("query").DefaultView[i].Row["MM"].ToString();
                if (nViews > nMaxViews)
                    nMaxViews = nViews;
                Int32 nX0 = i - 1;
                Int32 nX1 = i;
                Int32 nY0 = nLastViews;
                Int32 nY1 = nViews;

                if (i > 0)
                    sBar += "new Line(D.ScreenX(" + nX0.ToString() + "),D.ScreenY(" + nY0.ToString() + "),D.ScreenX(" + nX1.ToString() + "),D.ScreenY(" + nY1.ToString() + "), \"#cccccc\",1,\"" + sDate + " (" + nViews.ToString() + ")\");";
                sDot += "new Dot(D.ScreenX(" + nX1.ToString() + "),D.ScreenY(" + nY1.ToString() + "),6,6,\"#cccccc\",\"" + sDate + " (" + nViews.ToString() + ")\");";
                nLastViews = nViews;
            }
        }
        selectQuery1.Finish();
        selectQuery1 = null;

        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select ISNULL(q2.co,0) as co,q1.DD,q1.MM from ";
        selectQuery += "(select distinct DatePart(YYYY,wma.create_date) as YYYY,DatePart(Dd,wma.create_date) as DD,DatePart(Mm,wma.create_date) as MM  from watchers_media_actions wma where wma.create_date>DATEADD(m, -1, CONVERT(datetime, '" + sStartDay + "')))q1 left join ";
        selectQuery += "(select  ISNULL(count(*),0) as co,DatePart(YYYY,wma.create_date) as YYYY,DatePart(Dd,wma.create_date) as DD,DatePart(Mm,wma.create_date) as MM  from watchers_media_actions wma where wma.action_id=1 and wma.create_date>DATEADD(m, -1, CONVERT(datetime, '" + sStartDay + "')) and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("wma.group_id", "=", LoginManager.GetLoginGroupID());
        if (Session["media_id"] != null)
        {
            selectQuery += "and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("wma.media_id", "=", int.Parse(Session["media_id"].ToString()));
        }
        else if (Session["channel_id"] != null)
        {
            selectQuery += "and ";
            selectQuery += " wma.media_id in (" + GetMediaIDsForChannel() + ") ";
        }
        else if (Session["player_id"] != null)
        {
            //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("wma.PLAYER_ID", "=", int.Parse(Session["player_id"].ToString()));
        }
        selectQuery += " group by DatePart(Dd,wma.create_date),DatePart(Mm,wma.create_date),DatePart(YYYY,wma.create_date))q2 on (q1.DD=q2.DD and q1.MM=q2.MM and q1.YYYY=q2.YYYY) ORDER BY q1.YYYY,q1.MM,q1.DD";
        if (selectQuery.Execute("query", true) != null)
        {
            nCount = selectQuery.Table("query").DefaultView.Count;
            Int32 nLastViews = 0;
            for (int i = 0; i < nCount; i++)
            {
                Int32 nViews = int.Parse(selectQuery.Table("query").DefaultView[i].Row["co"].ToString());
                string sDate = selectQuery.Table("query").DefaultView[i].Row["DD"].ToString() + "/" + selectQuery.Table("query").DefaultView[i].Row["MM"].ToString();
                if (nViews > nMaxViews)
                    nMaxViews = nViews;
                Int32 nX0 = i - 1;
                Int32 nX1 = i;
                Int32 nY0 = nLastViews;
                Int32 nY1 = nViews;

                if (i > 0)
                    sBar += "new Line(D.ScreenX(" + nX0.ToString() + "),D.ScreenY(" + nY0.ToString() + "),D.ScreenX(" + nX1.ToString() + "),D.ScreenY(" + nY1.ToString() + "), \"#66ccff\",1,\"" + sDate + " (" + nViews.ToString() + ")\");";
                sDot += "new Dot(D.ScreenX(" + nX1.ToString() + "),D.ScreenY(" + nY1.ToString() + "),6,6,\"#006699\",\"" + sDate + " (" + nViews.ToString() + ")\");";
                nLastViews = nViews;
            }
        }
        selectQuery.Finish();
        selectQuery = null;

        string sRet = "";
        sRet += "var D=new Diagram();\r\n";
        sRet += "D.SetFrame(70, 30, 380, 200);\r\n";
        sRet += "D.SetText(\"\" ,\"\" , \"" + sName + " - Last Month\");\r\n";
        sRet += "D.XScale=0;\r\n";
        sRet += "D.SetGridColor(\"#cccccc\");\r\n";
        Int32 nAddToMax = 1;
        if (nMaxViews > 10)
            nAddToMax = 3;
        if (nMaxViews > 100)
            nAddToMax = 10;
        sRet += "D.SetBorder(0, " + (nCount - 1).ToString() + ", 0, " + (nMaxViews + nAddToMax).ToString() + ");\r\n";
        sRet += "D.Draw(\"#FFFFFF\",\"#666666\",false);\r\n";

        sBar += "new Box(D.ScreenX(1),D.ScreenY(" + (nMaxViews + nAddToMax).ToString() + ")+5,D.ScreenX(1) + 80,D.ScreenY(" + (nMaxViews + nAddToMax).ToString() + ")+5,\"#66ccff\" , \"Views\" , \"\" , 2);";
        sBar += "new Box(D.ScreenX(1),D.ScreenY(" + (nMaxViews + nAddToMax).ToString() + ")+25,D.ScreenX(1) + 80,D.ScreenY(" + (nMaxViews + nAddToMax).ToString() + ")+25,\"#cccccc\" , \"Unique\" , \"\" , 2);";

        sRet += sBar + sDot;
        Response.Write(sRet);
    }

    public void GetMediaDayViewCountStatistics()
    {
        Int32 nMaxViews = 0;
        string sBar = "";
        string sDot = "";
        string sName = "";
        if (Session["media_id"] != null)
            sName = ": Media Statistics: " + PageUtils.GetTableSingleVal("media", "name", int.Parse(Session["media_id"].ToString())).ToString();
        else if (Session["channel_id"] != null)
            sName = ": Channel Statistics: " + PageUtils.GetTableSingleVal("channels", "name", int.Parse(Session["channel_id"].ToString())).ToString();
        else if (Session["player_id"] != null)
            sName = ": Player Statistics: " + PageUtils.GetTableSingleVal("groups_passwords", "DOMAIN", int.Parse(Session["player_id"].ToString())).ToString();
        else
            sName = ": Statistics for all";
        //string sID = Session["media_id"].ToString();

        Int32 nCount = 0;
        ODBCWrapper.DataSetSelectQuery selectQuery1 = new ODBCWrapper.DataSetSelectQuery();
        selectQuery1 += "select ISNULL(q2.co,0) as co,q1.HH from ";
        selectQuery1 += "(select distinct DatePart(DD,wma.create_date) as DD,DatePart(MM,wma.create_date) as MM,DatePart(YYYY,wma.create_date) as YYYY,DatePart(HH,wma.create_date) as HH  from watchers_media_actions wma where wma.create_date>DATEADD(d, -1, getdate()))q1 left join ";
        selectQuery1 += "(select  ISNULL(count(distinct watcher_id),0) as co,DatePart(DD,wma.create_date) as DD,DatePart(MM,wma.create_date) as MM,DatePart(YYYY,wma.create_date) as YYYY,DatePart(HH,wma.create_date) as HH  from watchers_media_actions wma where wma.action_id=1 and wma.create_date>DATEADD(d, -1, getdate()) and ";
        selectQuery1 += ODBCWrapper.Parameter.NEW_PARAM("wma.group_id", "=", LoginManager.GetLoginGroupID());
        if (Session["media_id"] != null)
        {
            selectQuery1 += "and";
            selectQuery1 += ODBCWrapper.Parameter.NEW_PARAM("wma.media_id", "=", int.Parse(Session["media_id"].ToString()));
        }
        else if (Session["channel_id"] != null)
        {
            selectQuery1 += "and";
            selectQuery1 += " wma.media_id in (" + GetMediaIDsForChannel() + ") ";
        }
        else if (Session["player_id"] != null)
        {
            //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("wma.PLAYER_ID", "=", int.Parse(Session["player_id"].ToString()));
        }
        selectQuery1 += " group by DatePart(DD,wma.create_date),DatePart(MM,wma.create_date),DatePart(YYYY,wma.create_date),DatePart(HH,wma.create_date))q2 on (q1.HH=q2.HH and q1.YYYY=q2.YYYY and q1.MM=q2.MM and q1.DD=q2.DD) ORDER BY q1.YYYY,q1.MM,q1.DD,q1.HH";
        if (selectQuery1.Execute("query", true) != null)
        {
            nCount = selectQuery1.Table("query").DefaultView.Count;
            Int32 nLastViews = 0;
            for (int i = 0; i < nCount; i++)
            {
                Int32 nViews = int.Parse(selectQuery1.Table("query").DefaultView[i].Row["co"].ToString());
                string sDate = selectQuery1.Table("query").DefaultView[i].Row["HH"].ToString() + ":00";
                if (nViews > nMaxViews)
                    nMaxViews = nViews;
                Int32 nX0 = i - 1;
                Int32 nX1 = i;
                Int32 nY0 = nLastViews;
                Int32 nY1 = nViews;

                if (i > 0)
                    sBar += "new Line(D.ScreenX(" + nX0.ToString() + "),D.ScreenY(" + nY0.ToString() + "),D.ScreenX(" + nX1.ToString() + "),D.ScreenY(" + nY1.ToString() + "), \"#cccccc\",1,\"" + sDate + " (" + nViews.ToString() + ")\");";
                sDot += "new Dot(D.ScreenX(" + nX1.ToString() + "),D.ScreenY(" + nY1.ToString() + "),6,6,\"#cccccc\",\"" + sDate + " (" + nViews.ToString() + ")\");";
                nLastViews = nViews;
            }
        }
        selectQuery1.Finish();
        selectQuery1 = null;


        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select ISNULL(q2.co,0) as co,q1.HH from ";
        selectQuery += "(select distinct DatePart(YYYY,wma.create_date) as YYYY,DatePart(MM,wma.create_date) as MM,DatePart(DD,wma.create_date) as DD,DatePart(HH,wma.create_date) as HH  from watchers_media_actions wma where wma.create_date>DATEADD(d, -1, getdate()))q1 left join ";
        selectQuery += "(select  ISNULL(count(*),0) as co,DatePart(YYYY,wma.create_date) as YYYY,DatePart(MM,wma.create_date) as MM,DatePart(DD,wma.create_date) as DD,DatePart(HH,wma.create_date) as HH  from watchers_media_actions wma where wma.action_id=1 and wma.create_date>DATEADD(d, -1, getdate()) and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("wma.group_id", "=", LoginManager.GetLoginGroupID());
        if (Session["media_id"] != null)
        {
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("wma.media_id", "=", int.Parse(Session["media_id"].ToString()));
        }
        else if (Session["channel_id"] != null)
        {
            selectQuery += "and";
            selectQuery += " wma.media_id in (" + GetMediaIDsForChannel() + ") ";
        }
        else if (Session["player_id"] != null)
        {
            //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("wma.PLAYER_ID", "=", int.Parse(Session["player_id"].ToString()));
        }
        selectQuery += " group by DatePart(YYYY,wma.create_date),DatePart(MM,wma.create_date),DatePart(DD,wma.create_date),DatePart(HH,wma.create_date))q2 on (q1.HH=q2.HH and q1.YYYY=q2.YYYY and q1.MM=q2.MM and q1.DD=q2.DD) ORDER BY q1.YYYY,q1.MM,q1.DD,q1.HH";
        if (selectQuery.Execute("query", true) != null)
        {
            nCount = selectQuery.Table("query").DefaultView.Count;
            Int32 nLastViews = 0;
            for (int i = 0; i < nCount; i++)
            {
                Int32 nViews = int.Parse(selectQuery.Table("query").DefaultView[i].Row["co"].ToString());
                string sDate = selectQuery.Table("query").DefaultView[i].Row["HH"].ToString() + ":00";
                if (nViews > nMaxViews)
                    nMaxViews = nViews;
                Int32 nX0 = i - 1;
                Int32 nX1 = i;
                Int32 nY0 = nLastViews;
                Int32 nY1 = nViews;

                if (i > 0)
                    sBar += "new Line(D.ScreenX(" + nX0.ToString() + "),D.ScreenY(" + nY0.ToString() + "),D.ScreenX(" + nX1.ToString() + "),D.ScreenY(" + nY1.ToString() + "), \"#66ccff\",1,\"" + sDate + " (" + nViews.ToString() + ")\");";
                sDot += "new Dot(D.ScreenX(" + nX1.ToString() + "),D.ScreenY(" + nY1.ToString() + "),6,6,\"#006699\",\"" + sDate + " (" + nViews.ToString() + ")\");";
                nLastViews = nViews;
            }
        }
        selectQuery.Finish();
        selectQuery = null;

        string sRet = "";
        sRet += "var D=new Diagram();\r\n";
        sRet += "D.SetFrame(70, 30, 380, 200);\r\n";
        sRet += "D.SetText(\"\" ,\"\" , \"" + sName + " - Last Day\");\r\n";
        sRet += "D.XScale=0;\r\n";
        sRet += "D.SetGridColor(\"#cccccc\");\r\n";
        Int32 nAddToMax = 1;
        if (nMaxViews > 10)
            nAddToMax = 3;
        if (nMaxViews > 100)
            nAddToMax = 10;
        sRet += "D.SetBorder(0, " + (nCount - 1).ToString() + ", 0, " + (nMaxViews + nAddToMax).ToString() + ");\r\n";
        sRet += "D.Draw(\"#FFFFFF\",\"#666666\",false);\r\n";
        //sRet += "for (var i=15; i<50; i+=5)\r\n";

        sBar += "new Box(D.ScreenX(1),D.ScreenY(" + (nMaxViews + nAddToMax).ToString() + ")+5,D.ScreenX(1) + 80,D.ScreenY(" + (nMaxViews + nAddToMax).ToString() + ")+5,\"#66ccff\" , \"Views\" , \"\" , 2);";
        sBar += "new Box(D.ScreenX(1),D.ScreenY(" + (nMaxViews + nAddToMax).ToString() + ")+25,D.ScreenX(1) + 80,D.ScreenY(" + (nMaxViews + nAddToMax).ToString() + ")+25,\"#cccccc\" , \"Unique\" , \"\" , 2);";

        sRet += sBar + sDot;
        Response.Write(sRet);
    }

    public void GetMediaMounthViewsTimeStatistics()
    {
        Int32 nMaxViews = 0;
        string sBar = "";
        string sDot = "";
        string sName = "";
        if (Session["media_id"] != null)
            sName = ": Media Statistics: " + PageUtils.GetTableSingleVal("media", "name", int.Parse(Session["media_id"].ToString())).ToString();
        else if (Session["channel_id"] != null)
            sName = ": Channel Statistics: " + PageUtils.GetTableSingleVal("channels", "name", int.Parse(Session["channel_id"].ToString())).ToString();
        else if (Session["player_id"] != null)
            sName = ": Player Statistics: " + PageUtils.GetTableSingleVal("groups_passwords", "DOMAIN", int.Parse(Session["player_id"].ToString())).ToString();
        else
            sName = ": Statistics for all";
        //string sID = Session["media_id"].ToString();
        Int32 nCount = 0;

        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select ISNULL(q2.co,0) as co,q1.DD,q1.MM,q1.YYYY from ";
        selectQuery += "(select distinct DatePart(Dd,wma.create_date) as DD,DatePart(Mm,wma.create_date) as MM,DatePart(YYYY,wma.create_date) as YYYY  from watchers_media_actions wma where wma.create_date>DATEADD(m, -1, getdate()))q1 left join ";
        selectQuery += "(select  ISNULL(sum(PLAY_TIME_COUNTER),0) as co,DatePart(Dd,wma.create_date) as DD,DatePart(Mm,wma.create_date) as MM,DatePart(YYYY,wma.create_date) as YYYY  from watchers_media_play_counters wma where wma.create_date>DATEADD(m, -1, getdate()) and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("wma.group_id", "=", LoginManager.GetLoginGroupID());
        if (Session["media_id"] != null)
        {
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("wma.media_id", "=", int.Parse(Session["media_id"].ToString()));
        }
        else if (Session["channel_id"] != null)
        {
            selectQuery += "and";
            selectQuery += " wma.media_id in (" + GetMediaIDsForChannel() + ") ";
        }
        else if (Session["player_id"] != null)
        {
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("wma.PLAYER_ID", "=", int.Parse(Session["player_id"].ToString()));
        }
        selectQuery += " group by DatePart(Dd,wma.create_date),DatePart(Mm,wma.create_date),DatePart(YYYY,wma.create_date))q2 on (q1.DD=q2.DD and q1.MM=q2.MM and q1.YYYY=q2.YYYY) ORDER BY q1.YYYY,q1.MM,q1.DD";
        if (selectQuery.Execute("query", true) != null)
        {
            nCount = selectQuery.Table("query").DefaultView.Count;
            Int32 nLastViews = 0;
            for (int i = 0; i < nCount; i++)
            {
                double dWatches = double.Parse(selectQuery.Table("query").DefaultView[i].Row["co"].ToString()) / 60;
                //Int32 nViews = int.Parse(selectQuery.Table("query").DefaultView[i].Row["co"].ToString());
                string sDate = selectQuery.Table("query").DefaultView[i].Row["DD"].ToString() + "/" + selectQuery.Table("query").DefaultView[i].Row["MM"].ToString();
                if (dWatches > nMaxViews)
                    nMaxViews = (Int32)dWatches;
                Int32 nX0 = i - 1;
                Int32 nX1 = i;
                Int32 nY0 = nLastViews;
                double nY1 = dWatches;

                if (i > 0)
                    sBar += "new Line(D.ScreenX(" + nX0.ToString() + "),D.ScreenY(" + nY0.ToString() + "),D.ScreenX(" + nX1.ToString() + "),D.ScreenY(" + nY1.ToString() + "), \"#66ccff\",1,\"" + sDate + " (" + dWatches.ToString() + ")\");";
                sDot += "new Dot(D.ScreenX(" + nX1.ToString() + "),D.ScreenY(" + nY1.ToString() + "),6,6,\"#006699\",\"" + sDate + " (" + dWatches.ToString() + ")\");";
                nLastViews = (Int32)dWatches;
            }
        }
        selectQuery.Finish();
        selectQuery = null;

        string sRet = "";
        sRet += "var D=new Diagram();\r\n";
        sRet += "D.SetFrame(70, 30, 380, 200);\r\n";
        sRet += "D.SetText(\"\" ,\"\" , \"" + sName + " - Minutes Last Month\");\r\n";
        sRet += "D.XScale=0;\r\n";
        sRet += "D.SetGridColor(\"#cccccc\");\r\n";
        Int32 nAddToMax = 1;
        if (nMaxViews > 10)
            nAddToMax = 3;
        if (nMaxViews > 100)
            nAddToMax = 10;
        sRet += "D.SetBorder(0, " + (nCount - 1).ToString() + ", 0, " + (nMaxViews + nAddToMax).ToString() + ");\r\n";
        sRet += "D.Draw(\"#FFFFFF\",\"#666666\",false);\r\n";

        sBar += "new Box(D.ScreenX(1),D.ScreenY(" + (nMaxViews + nAddToMax).ToString() + ")+5,D.ScreenX(1) + 80,D.ScreenY(" + (nMaxViews + nAddToMax).ToString() + ")+5,\"#66ccff\" , \"Minutes\" , \"\" , 2);";
        //sDot += "new Box(D.ScreenX(1),D.ScreenY(" + (nMaxViews + nAddToMax).ToString() + ")+25,D.ScreenX(1) + 80,D.ScreenY(" + (nMaxViews + nAddToMax).ToString() + ")+25,\"#cccccc\" , \"Unique\");";

        sRet += sBar + sDot;
        Response.Write(sRet);
    }

    public void GetMediaDayViewsTimeStatistics()
    {
        Int32 nMaxViews = 0;
        string sBar = "";
        string sDot = "";
        string sName = "";
        if (Session["media_id"] != null)
            sName = ": Media Statistics: " + PageUtils.GetTableSingleVal("media", "name", int.Parse(Session["media_id"].ToString())).ToString();
        else if (Session["channel_id"] != null)
            sName = ": Channel Statistics: " + PageUtils.GetTableSingleVal("channels", "name", int.Parse(Session["channel_id"].ToString())).ToString();
        else if (Session["player_id"] != null)
            sName = ": Player Statistics: " + PageUtils.GetTableSingleVal("groups_passwords", "DOMAIN", int.Parse(Session["player_id"].ToString())).ToString();
        else
            sName = ": Statistics for all";
        //string sID = Session["media_id"].ToString();
        Int32 nCount = 0;

        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select ISNULL(q2.co,0) as co,q1.HH,q1.YYYY from ";
        selectQuery += "(select distinct DatePart(Dd,wma.create_date) as DD,DatePart(Mm,wma.create_date) as MM,DatePart(HH,wma.create_date) as HH,DatePart(YYYY,wma.create_date) as YYYY  from watchers_media_actions wma where wma.create_date>DATEADD(d, -1, getdate()))q1 left join ";
        selectQuery += "(select  ISNULL(sum(PLAY_TIME_COUNTER),0) as co,DatePart(Dd,wma.create_date) as DD,DatePart(Mm,wma.create_date) as MM,DatePart(HH,wma.create_date) as HH,DatePart(YYYY,wma.create_date) as YYYY  from watchers_media_play_counters wma where wma.create_date>DATEADD(d, -1, getdate()) and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("wma.group_id", "=", LoginManager.GetLoginGroupID());
        if (Session["media_id"] != null)
        {
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("wma.media_id", "=", int.Parse(Session["media_id"].ToString()));
        }
        else if (Session["channel_id"] != null)
        {
            selectQuery += "and";
            selectQuery += " wma.media_id in (" + GetMediaIDsForChannel() + ") ";
        }
        else if (Session["player_id"] != null)
        {
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("wma.PLAYER_ID", "=", int.Parse(Session["player_id"].ToString()));
        }
        selectQuery += " group by DatePart(Dd,wma.create_date),DatePart(Mm,wma.create_date),DatePart(HH,wma.create_date),DatePart(YYYY,wma.create_date))q2 on (q1.HH=q2.HH and q1.YYYY=q2.YYYY and q1.DD=q2.DD and q1.MM=q2.MM) ORDER BY q1.YYYY,q1.MM,q1.DD,q1.HH";
        if (selectQuery.Execute("query", true) != null)
        {
            nCount = selectQuery.Table("query").DefaultView.Count;
            double dLastViews = 0;
            for (int i = 0; i < nCount; i++)
            {
                double dWatches = double.Parse(selectQuery.Table("query").DefaultView[i].Row["co"].ToString()) / 60;
                //Int32 nViews = int.Parse(selectQuery.Table("query").DefaultView[i].Row["co"].ToString());
                string sDate = selectQuery.Table("query").DefaultView[i].Row["HH"].ToString() + ":00";
                if (dWatches > nMaxViews)
                    nMaxViews = (Int32)dWatches;
                Int32 nX0 = i - 1;
                Int32 nX1 = i;
                double nY0 = dLastViews;
                double nY1 = dWatches;

                if (i > 0)
                    sBar += "new Line(D.ScreenX(" + nX0.ToString() + "),D.ScreenY(" + nY0.ToString() + "),D.ScreenX(" + nX1.ToString() + "),D.ScreenY(" + nY1.ToString() + "), \"#66ccff\",1,\"" + sDate + " (" + dWatches.ToString() + ")\");";
                sDot += "new Dot(D.ScreenX(" + nX1.ToString() + "),D.ScreenY(" + nY1.ToString() + "),6,6,\"#006699\",\"" + sDate + " (" + dWatches.ToString() + ")\");";
                dLastViews = dWatches;
            }
        }
        selectQuery.Finish();
        selectQuery = null;

        string sRet = "";
        sRet += "var D=new Diagram();\r\n";
        sRet += "D.SetFrame(70, 30, 380, 200);\r\n";
        sRet += "D.SetText(\"\" ,\"\" , \"" + sName + " - Minutes Last Day\");\r\n";
        sRet += "D.XScale=0;\r\n";
        sRet += "D.SetGridColor(\"#cccccc\");\r\n";
        Int32 nAddToMax = 1;
        if (nMaxViews > 10)
            nAddToMax = 3;
        if (nMaxViews > 100)
            nAddToMax = 10;
        sRet += "D.SetBorder(0, " + (nCount - 1).ToString() + ", 0, " + (nMaxViews + nAddToMax).ToString() + ");\r\n";
        sRet += "D.Draw(\"#FFFFFF\",\"#666666\",false);\r\n";

        sBar += "new Box(D.ScreenX(1),D.ScreenY(" + (nMaxViews + nAddToMax).ToString() + ")+5,D.ScreenX(1) + 80,D.ScreenY(" + (nMaxViews + nAddToMax).ToString() + ")+5,\"#66ccff\" , \"Minutes\" , \"\" , 2);";
        //sDot += "new Box(D.ScreenX(1),D.ScreenY(" + (nMaxViews + nAddToMax).ToString() + ")+25,D.ScreenX(1) + 80,D.ScreenY(" + (nMaxViews + nAddToMax).ToString() + ")+25,\"#cccccc\" , \"Unique\");";

        sRet += sBar + sDot;
        Response.Write(sRet);
    }

    public void GetMediaMounthPlayersStatistics()
    {
    }

    public void GetMediaDayPlayersStatistics()
    {
    }
    /*
    public void GetMediaMounthGeoStatistics()
    {
        Response.Write(GetMediaGeoStatisticsStr(31));
    }
    
    public string GetMediaGeoStatisticsStr(Int32 nDays)
    {
        string sRet = "var P" + nDays.ToString() + " = new Array();\r\n";
        Int32 nTotal = 0;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select c.id,c.country_name,count(*) as co from watchers_media_play_counters wmpc,countries c where ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("wmpc.group_id", "=", LoginManager.GetLoginGroupID());
        if (nDays > 0)
        {
            if (nDays == 31)
                selectQuery += " and wmpc.create_date>DATEADD(m, -1, CONVERT(datetime, LEFT(CONVERT(char(10), getdate(), 20) , 10))) ";
            else
                selectQuery += " and wmpc.create_date>DATEADD(d, " + (nDays * -1).ToString() + ", CONVERT(datetime, LEFT(CONVERT(char(10), getdate(), 20) , 10))) ";
        }
        if (Session["media_id"] != null)
        {
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("wmpc.media_id", "=", int.Parse(Session["media_id"].ToString()));
        }
        else if (Session["channel_id"] != null)
        {
            selectQuery += "and";
            selectQuery += " wmpc.media_id in (" + GetMediaIDsForChannel() + ") ";
        }
        else if (Session["player_id"] != null)
        {
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("wmpc.PLAYER_ID", "=", int.Parse(Session["player_id"].ToString()));
        }
        selectQuery += "and";
        selectQuery += "c.id=wmpc.country_id group by c.id,c.country_name order by co desc";
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            for (int i = 0; i < nCount; i++)
            {
                string sCountryName = selectQuery.Table("query").DefaultView[i].Row["country_name"].ToString();
                Int32 nCO = int.Parse(selectQuery.Table("query").DefaultView[i].Row["co"].ToString());
                nTotal += nCO;
            }
            double dAngle0 = 0;
            double dAngle1 = 0;
            Int32 nColor = 0;
            for (int i = 0; i < nCount; i++)
            {
                string sCountryName = selectQuery.Table("query").DefaultView[i].Row["country_name"].ToString();
                Int32 nCO = int.Parse(selectQuery.Table("query").DefaultView[i].Row["co"].ToString());
                Int32 nID = int.Parse(selectQuery.Table("query").DefaultView[i].Row["id"].ToString());
                double dPer = Math.Round(((double)nCO / (double)nTotal) * 100, 2);

                dAngle1 += (((double)nCO / (double)nTotal) * 360);
                sRet += "P" + nDays.ToString() + "[" + i.ToString() + "]=new Pie(220,110,0,100," + dAngle0.ToString() + "," + dAngle1.ToString() + ",\"" + System.Drawing.ColorTranslator.ToHtml(System.Drawing.ColorTranslator.FromWin32((255 - nID) * 700)) + "\",\"" + sCountryName + " - " + dPer.ToString() + "%\");\r\n";
                dAngle0 = dAngle1;
                nColor++;
            }
        }
        selectQuery.Finish();
        selectQuery = null;
        if (nDays == 31)
            sRet += "new Box(1,5,80,5,\"#FFFFFF\" , \"Countries Last Mounth\" , \"\" , 2);";
        if (nDays == 1)
            sRet += "new Box(1,5,80,5,\"#FFFFFF\" , \"Countries Last Day\" , \"\" , 2);";
        return sRet;
    }
    
    public string GetMediaPlayersStatisticsStr(Int32 nDays)
    {
        string sRet = "var PP" + nDays.ToString() + " = new Array();\r\n";
        Int32 nTotal = 0;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        string sGroups = PageUtils.GetAllGroupTreeStr(LoginManager.GetLoginGroupID());
        selectQuery += "select c.id,c.DOMAIN,count(*) as co from watchers_media_play_counters wmpc,groups_passwords c,media m where m.id=wmpc.media_id and ";// c.group_id  ";
        //selectQuery += sGroups += " and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("m.group_id", "=", LoginManager.GetLoginGroupID());
        selectQuery += " and ";
        //Int32 nOwnerGroupID = int.Parse(PageUtils.GetTableSingleVal("media", "group_id", int.Parse(Session["media_id"].ToString())).ToString());
        //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", LoginManager.GetLoginGroupID());
        if (nDays > 0)
        {
            if (nDays == 31)
                selectQuery += " wmpc.create_date>DATEADD(m, -1, CONVERT(datetime, LEFT(CONVERT(char(10), getdate(), 20) , 10))) and";
            else
                selectQuery += " wmpc.create_date>DATEADD(d, " + (nDays * -1).ToString() + ", CONVERT(datetime, LEFT(CONVERT(char(10), getdate(), 20) , 10))) and";
        }

        //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("wmpc.media_id", "=", int.Parse(Session["media_id"].ToString()));
        selectQuery += " c.id=wmpc.PLAYER_ID group by c.id,c.DOMAIN order by count(*) desc";
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            for (int i = 0; i < nCount; i++)
            {
                string sPlayerName = selectQuery.Table("query").DefaultView[i].Row["DOMAIN"].ToString();
                Int32 nCO = int.Parse(selectQuery.Table("query").DefaultView[i].Row["co"].ToString());
                nTotal += nCO;
            }
            double dAngle0 = 0;
            double dAngle1 = 0;
            Int32 nColor = 0;
            for (int i = 0; i < nCount; i++)
            {
                string sPlayerName = selectQuery.Table("query").DefaultView[i].Row["DOMAIN"].ToString();
                Int32 nCO = int.Parse(selectQuery.Table("query").DefaultView[i].Row["co"].ToString());
                Int32 nID = int.Parse(selectQuery.Table("query").DefaultView[i].Row["id"].ToString());
                double dPer = Math.Round(((double)nCO / (double)nTotal) * 100, 2);
                if (dPer > 0)
                {
                    dAngle1 += (((double)nCO / (double)nTotal) * 360);
                    sRet += "PP" + nDays.ToString() + "[" + i.ToString() + "]=new Pie(220,110,0,100," + dAngle0.ToString() + "," + dAngle1.ToString() + ",\"" + System.Drawing.ColorTranslator.ToHtml(System.Drawing.ColorTranslator.FromWin32((255 - nID) * 700)) + "\",\"" + sPlayerName + " - " + dPer.ToString() + "%\");\r\n";
                    dAngle0 = dAngle1;
                    nColor++;
                }
            }
        }
        selectQuery.Finish();
        selectQuery = null;
        if (nDays == 31)
            sRet += "new Box(1,5,80,5,\"#FFFFFF\" , \"Players Last Month\" , \"\" , 2);";
        if (nDays == 1)
            sRet += "new Box(1,5,80,5,\"#FFFFFF\" , \"Players Last Day\" , \"\" , 2);";
        return sRet;
    }
     */
    public void GetCampaignViews(Int32 nDays)
    {
        Int32 nMaxViews = 0;
        string sBar = "";
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select  TOP (10) count(*) as co,m.name,m.id from watchers_media_actions wma,commercial m where m.id=wma.media_ID and wma.action_id in (4,41) and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("m.group_id", "=", LoginManager.GetLoginGroupID());
        if (nDays > 0)
        {
            selectQuery += "and";
            if (nDays == 31)
                selectQuery += "wma.create_date>DATEADD(m, -1, getdate())";
            else
                selectQuery += "wma.create_date>DATEADD(d, " + (nDays * -1).ToString() + ", getdate())";
        }
        selectQuery += " group by m.id,m.name  ORDER BY co DESC";
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            for (int i = 0; i < nCount; i++)
            {
                Int32 nViews = int.Parse(selectQuery.Table("query").DefaultView[i].Row["co"].ToString());
                string sName = selectQuery.Table("query").DefaultView[i].Row["name"].ToString();
                string sID = selectQuery.Table("query").DefaultView[i].Row["id"].ToString();
                if (i == 0)
                    nMaxViews = nViews;
                Int32 nXRight = (i * 3) + 3;
                Int32 nXLeft = (i * 3) + 1;
                sBar += "new Bar(D.ScreenX(" + nXLeft.ToString() + "),D.ScreenY(" + nViews.ToString() + "),D.ScreenX(" + nXRight.ToString() + "),D.ScreenY(0), \"#66ccff\" , \"\",\"#666666\",\"" + sName + " (" + nViews.ToString() + ")\",\"GetMediaStatistics(" + sID + ");\");";
            }
        }
        selectQuery.Finish();
        selectQuery = null;
        string sRet = "";
        sRet += "var D=new Diagram();\r\n";
        sRet += "D.SetFrame(70, 30, 380, 200);\r\n";
        if (nDays == 31)
            sRet += "D.SetText(\"\" ,\"\" , \"Top 10 Commercials - Last Month\");\r\n";
        if (nDays == 7)
            sRet += "D.SetText(\"\" ,\"\" , \"Top 10 Commercials - Last Week\");\r\n";
        if (nDays == 1)
            sRet += "D.SetText(\"\" ,\"\" , \"Top 10 Commercials - Last Day\");\r\n";
        if (nDays == 0)
            sRet += "D.SetText(\"\" ,\"\" , \"Top 10 Commercials\");\r\n";
        sRet += "D.XScale=0;\r\n";
        sRet += "D.SetGridColor(\"#cccccc\");\r\n";
        Int32 nAddToMax = 1;
        if (nMaxViews > 10)
            nAddToMax = 3;
        if (nMaxViews > 100)
            nAddToMax = 10;
        sRet += "D.SetBorder(0, 31, 0, " + (nMaxViews + nAddToMax).ToString() + ");\r\n";
        sRet += "D.Draw(\"#FFFFFF\",\"#666666\",false);\r\n";
        //sRet += "for (var i=15; i<50; i+=5)\r\n";
        sRet += sBar;
        Response.Write(sRet);
    }

    public void GetMediaDayGeoStatistics()
    {
    }


    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
    }

    protected void GetSubMenu()
    {
        Response.Write(m_sSubMenu);
    }
}
