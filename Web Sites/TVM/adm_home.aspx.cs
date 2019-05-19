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

public partial class adm_home : System.Web.UI.Page
{
    protected string m_sMenu;
    protected string m_sSubMenu;
    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        if (LoginManager.IsPagePermitted("adm_media.aspx") == false)
            LoginManager.LogoutFromSite("login.html");
        if (LoginManager.IsActionPermittedOnPage("adm_media.aspx", LoginManager.PAGE_PERMISION_TYPE.EDIT) == false)
            LoginManager.LogoutFromSite("login.html");

        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        if (!IsPostBack)
        {
            Int32 nMenuID = 0;

            m_sMenu = TVinciShared.Menu.GetMainMenu(15, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, true);
            //Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();
        }
    }

    public void GetHeader()
    {
        Response.Write(PageUtils.GetPreHeader() + ": Home Page");
    }

    public void GetTotalMediaViews(Int32 nDays)
    {
        Int32 nMaxViews = 0;
        string sBar = "";
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select  TOP (10) count(*) as co,m.name,m.id from watchers_media_actions wma,media m where m.id=wma.media_ID and wma.action_id=1 and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("m.group_id", "=", LoginManager.GetLoginGroupID());
        if (nDays > 0)
        {
            selectQuery += "and";
            if (nDays == 31)
                selectQuery += "wma.create_date>DATEADD(m, -1, CONVERT(datetime, LEFT(CONVERT(char(10), getdate(), 20) , 10)))";
            else
                selectQuery += "wma.create_date>DATEADD(d, " + (nDays * -1).ToString() + ", CONVERT(datetime, LEFT(CONVERT(char(10), getdate(), 20) , 10)))";
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

    public void GetCampaignViews(Int32 nDays)
    {
        Int32 nMaxViews = 0;
        string sBar = "";
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select  TOP (10) count(*) as co,m.name,m.id from watchers_media_actions wma,commercial m where m.id=wma.media_ID and wma.action_id=4 and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("m.group_id", "=", LoginManager.GetLoginGroupID());
        if (nDays > 0)
        {
            selectQuery += "and";
            if (nDays == 31)
                selectQuery += "wma.create_date>DATEADD(m, -1, CONVERT(datetime, LEFT(CONVERT(char(10), getdate(), 20) , 10)))";
            else
                selectQuery += "wma.create_date>DATEADD(d, " + (nDays * -1).ToString() + ", CONVERT(datetime, LEFT(CONVERT(char(10), getdate(), 20) , 10)))";
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

    public void GetTotalCampaignsViews(Int32 nDays)
    {
        Int32 nMaxViews = 0;
        string sBar = "";
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select  TOP (10) c.id,c.name,c.views from campaigns c where ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("c.group_id", "=", LoginManager.GetLoginGroupID());
        if (nDays > 0)
        {
            selectQuery += "and";
            if (nDays == 31)
                selectQuery += "c.create_date>DATEADD(m, -1, CONVERT(datetime, LEFT(CONVERT(char(10), getdate(), 20) , 10)))";
            else
                selectQuery += "c.create_date>DATEADD(d, " + (nDays * -1).ToString() + ", CONVERT(datetime, LEFT(CONVERT(char(10), getdate(), 20) , 10)))";
        }
        selectQuery += " ORDER BY c.views DESC";
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            for (int i = 0; i < nCount; i++)
            {
                Int32 nViews = int.Parse(selectQuery.Table("query").DefaultView[i].Row["views"].ToString());
                string sName = selectQuery.Table("query").DefaultView[i].Row["name"].ToString();
                string sID = selectQuery.Table("query").DefaultView[i].Row["id"].ToString();
                if (i == 0)
                    nMaxViews = nViews;
                Int32 nXRight = (i * 3) + 3;
                Int32 nXLeft = (i * 3) + 1;
                sBar += "new Bar(D.ScreenX(" + nXLeft.ToString() + "),D.ScreenY(" + nViews.ToString() + "),D.ScreenX(" + nXRight.ToString() + "),D.ScreenY(0), \"#66ccff\" , \"\",\"#666666\",\"" + sName + " (" + nViews.ToString() + ")\",\"GetCampaignsStatistics(" + sID + ");\");";
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

    protected void GetGEOSummeryForPeriod(bool bUnique, Int32 nDaysAgo, ref System.Collections.Specialized.NameValueCollection theCountries)
    {
        //Hashtable theContries = new Hashtable();

        Int32 nTotal = 0;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select c.country_name,count(";
        if (bUnique == true)
            selectQuery += "distinct watcher_id";
        else
            selectQuery += "*";
        selectQuery += ") as co from watchers_media_play_counters wmpc,countries c where ";
        if (nDaysAgo > 0)
        {
            selectQuery += " wmpc.create_date>";
            if (nDaysAgo == 31)
                selectQuery += "DATEADD(m, -1, CONVERT(datetime, LEFT(CONVERT(char(10), getdate(), 20) , 10))) and ";
            else
                selectQuery += "DATEADD(d, " + (nDaysAgo * -1).ToString() + " , CONVERT(datetime, LEFT(CONVERT(char(10), getdate(), 20) , 10))) and ";
        }
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("wmpc.group_id", "=", LoginManager.GetLoginGroupID());
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

    protected void GetPlayersSummeryForPeriod(bool bUnique, Int32 nDaysAgo, ref System.Collections.Specialized.NameValueCollection thePlayers)
    {
        //Hashtable theContries = new Hashtable();

        Int32 nTotal = 0;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select c.DOMAIN,count(";
        if (bUnique == true)
            selectQuery += "distinct watcher_id";
        else
            selectQuery += "*";
        selectQuery += ") as co from watchers_media_play_counters wmpc,groups_passwords c where ";
        if (nDaysAgo > 0)
        {
            selectQuery += " wmpc.create_date>";
            if (nDaysAgo == 31)
                selectQuery += "DATEADD(m, -1, CONVERT(datetime, LEFT(CONVERT(char(10), getdate(), 20) , 10))) and ";
            else
                selectQuery += "DATEADD(d, " + (nDaysAgo * -1).ToString() + " , CONVERT(datetime, LEFT(CONVERT(char(10), getdate(), 20) , 10))) and ";
        }
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("wmpc.group_id", "=", LoginManager.GetLoginGroupID());
        selectQuery += "and";
        selectQuery += " c.id=wmpc.player_id group by c.domain order by co desc";
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            for (int i = 0; i < nCount; i++)
            {
                string sPlayerName = selectQuery.Table("query").DefaultView[i].Row["domain"].ToString();
                Int32 nCO = int.Parse(selectQuery.Table("query").DefaultView[i].Row["co"].ToString());
                nTotal += nCO;
            }
            for (int i = 0; i < nCount; i++)
            {
                string sPlayerName = selectQuery.Table("query").DefaultView[i].Row["domain"].ToString();
                Int32 nCO = int.Parse(selectQuery.Table("query").DefaultView[i].Row["co"].ToString());
                double dPer = Math.Round(((double)nCO / (double)nTotal) * 100, 2);
                thePlayers[sPlayerName] += "<td ";
                if (nDaysAgo == 1)
                    thePlayers[sPlayerName] += " onmouseout=\"PP1[" + i.ToString() + "].MoveTo('','',0);\" onmouseover=\"PP1[" + i.ToString() + "].MoveTo('','',10);\" ";
                if (nDaysAgo == 31)
                    thePlayers[sPlayerName] += " onmouseout=\"PP31[" + i.ToString() + "].MoveTo('','',0);\" onmouseover=\"PP31[" + i.ToString() + "].MoveTo('','',10);\" ";
                thePlayers[sPlayerName] += " style=\"font-size: 12px;text-align: center;\">" + dPer.ToString() + "%</td>";
            }
        }
        selectQuery.Finish();
        selectQuery = null;
    }

    protected string GetGEOSummeryIntr(bool bUnique)
    {
        System.Collections.Specialized.NameValueCollection theContries = new System.Collections.Specialized.NameValueCollection();
        GetGEOSummeryForPeriod(bUnique, 0, ref theContries);
        GetGEOSummeryForPeriod(bUnique, 31, ref theContries);
        GetGEOSummeryForPeriod(bUnique, 7, ref theContries);
        GetGEOSummeryForPeriod(bUnique, 1, ref theContries);

        string sRet = "";
        IEnumerator iter = theContries.GetEnumerator();
        Int32 nI = 0;
        while (iter.MoveNext())
        {
            sRet += "<tr onmouseout=\"this.style.backgroundColor='#FFFFFF';\" onmouseover=\"this.style.backgroundColor='#EEEEEE'\" style=\"cursor: pointer;\"><th style=\"font-size: 12px;cursor: pointer;\" >" + iter.Current.ToString() + "</th>";
            sRet += theContries[iter.Current.ToString()].ToString();
            sRet += "</tr>";
            nI++;
        }
        return sRet;
    }

    protected string GetPlayersSummeryIntr(bool bUnique)
    {
        System.Collections.Specialized.NameValueCollection thePlayers = new System.Collections.Specialized.NameValueCollection();
        GetPlayersSummeryForPeriod(bUnique, 0, ref thePlayers);
        GetPlayersSummeryForPeriod(bUnique, 31, ref thePlayers);
        GetPlayersSummeryForPeriod(bUnique, 7, ref thePlayers);
        GetPlayersSummeryForPeriod(bUnique, 1, ref thePlayers);



        string sRet = "";
        IEnumerator iter = thePlayers.GetEnumerator();
        Int32 nI = 0;
        while (iter.MoveNext())
        {
            sRet += "<tr onmouseout=\"this.style.backgroundColor='#FFFFFF';\" onmouseover=\"this.style.backgroundColor='#EEEEEE'\" style=\"cursor: pointer;\"><th style=\"font-size: 12px;cursor: pointer;\" >" + iter.Current.ToString() + "</th>";
            sRet += thePlayers[iter.Current.ToString()].ToString();
            sRet += "</tr>";
            nI++;
        }
        return sRet;
    }

    public void GetGeoSummery()
    {
        string sRet = "<table width=100%><tr><td colspan=\"2\" width=\"100%\" align=\"center\" class=\"adm_table_header\">Geo Summary</td></tr>";
        sRet += "<tr>";
        sRet += "<td>";
        sRet += "<div  style=\"position:relative; top:0px; height:220px;\">";
        sRet += "<script type=\"text/javascript\">";
        sRet += GetMediaGeoStatisticsStr(31);
        sRet += "</script>";
        sRet += "</div>";
        sRet += "</td>";
        sRet += "<td>";
        sRet += "<div  style=\"position:relative; top:0px; height:220px;\">";
        sRet += "<script type=\"text/javascript\">";
        sRet += GetMediaGeoStatisticsStr(1);
        sRet += "</script>";
        sRet += "</div>";
        sRet += "</td>";
        sRet += "</tr>";
        sRet += "<tr><td width=\"100%\" nowrap colspan=\"2\">";
        sRet += "<table width=\"100%\" style=\"border: solid 1px #cccccc;\">";
        sRet += "<tr><th style=\"font-size: 12px;width: 28%;\"></th><th style=\"font-size: 12px;width: 18%;\">Total</th><th style=\"font-size: 12px;width: 18%;\">Last Month</th><th style=\"font-size: 12px;width: 18%;\">Last Week</th><th style=\"font-size: 12px;width: 18%;\">Last Day</th></tr>";
        sRet += GetGEOSummeryIntr(false);
        sRet += "</table>";
        sRet += "</td></tr>";
        sRet += "</table>";
        Response.Write(sRet);
    }

    public void GetPlayersSummery()
    {
        string sRet = "<table width=100%><tr><td colspan=\"2\" width=\"100%\" align=\"center\" class=\"adm_table_header\">Players Summary</td></tr>";
        sRet += "<tr>";
        sRet += "<td>";
        sRet += "<div  style=\"position:relative; top:0px; height:220px;\">";
        sRet += "<script type=\"text/javascript\">";
        sRet += GetMediaPlayersStatisticsStr(31);
        sRet += "</script>";
        sRet += "</div>";
        sRet += "</td>";
        sRet += "<td>";
        sRet += "<div  style=\"position:relative; top:0px; height:220px;\">";
        sRet += "<script type=\"text/javascript\">";
        sRet += GetMediaPlayersStatisticsStr(1);
        sRet += "</script>";
        sRet += "</div>";
        sRet += "</td>";
        sRet += "</tr>";
        sRet += "<tr><td width=\"100%\" nowrap colspan=\"2\">";
        sRet += "<table width=\"100%\" style=\"border: solid 1px #cccccc;\">";
        sRet += "<tr><th style=\"font-size: 12px;width: 28%;\"></th><th style=\"font-size: 12px;width: 18%;\">Total</th><th style=\"font-size: 12px;width: 18%;\">Last Month</th><th style=\"font-size: 12px;width: 18%;\">Last Week</th><th style=\"font-size: 12px;width: 18%;\">Last Day</th></tr>";
        sRet += GetPlayersSummeryIntr(false);
        sRet += "</table>";
        sRet += "</td></tr>";
        sRet += "</table>";
        Response.Write(sRet);
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

    protected Int32 GetNumberOfMedia(Int32 nDays)
    {
        Int32 nRet = 0;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select count(*) as co from media where status=1 and ";
        if (nDays > 0)
        {
            if (nDays == 31)
                selectQuery += " start_Date>DATEADD(m, -1, CONVERT(datetime, LEFT(CONVERT(char(10), getdate(), 20) , 10))) and ";
            else
                selectQuery += " start_Date>DATEADD(d, " + (nDays * -1).ToString() + ", CONVERT(datetime, LEFT(CONVERT(char(10), getdate(), 20) , 10))) and ";
        }
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", LoginManager.GetLoginGroupID());
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

    protected Int32 GetNumberOfCampaigns(Int32 nDays)
    {
        Int32 nRet = 0;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select count(*) as co from commercial where status=1 and ";
        if (nDays > 0)
        {
            if (nDays == 31)
                selectQuery += " start_Date>DATEADD(m, -1, CONVERT(datetime, LEFT(CONVERT(char(10), getdate(), 20) , 10))) and ";
            else
                selectQuery += " start_Date>DATEADD(d, " + (nDays * -1).ToString() + ", CONVERT(datetime, LEFT(CONVERT(char(10), getdate(), 20) , 10))) and ";
        }
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", LoginManager.GetLoginGroupID());
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

    public void GetMediaSummery()
    {
        string sRet = "<table width=100%><tr><td width=\"100%\" align=\"center\" class=\"adm_table_header\">Views Summary</td></tr>";
        sRet += "<tr><td width=\"100%\" nowrap>";
        sRet += "<table width=\"100%\" style=\"border: solid 1px #cccccc;\">";
        sRet += "<tr><th style=\"font-size: 12px;width: 28%;\"></th><th style=\"font-size: 12px;width: 18%;\">Total</th><th style=\"font-size: 12px;width: 18%;\">Last Month</th><th style=\"font-size: 12px;width: 18%;\">Last Week</th><th style=\"font-size: 12px;width: 18%;\">Last Day</th></tr>";
        Int32 nNewMediaDay = GetNumberOfMedia(1);
        Int32 nNewMediaWeek = GetNumberOfMedia(7);
        Int32 nNewMediaMounth = GetNumberOfMedia(31);
        Int32 nNewMediaTotal = GetNumberOfMedia(0);

        Int32 nNewCommercialsDay = GetNumberOfCampaigns(1);
        Int32 nNewCommercialsWeek = GetNumberOfCampaigns(7);
        Int32 nNewCommercialsMounth = GetNumberOfCampaigns(31);
        Int32 nNewCommercialsTotal = GetNumberOfCampaigns(0);

        sRet += "<tr><th style=\"font-size: 12px;\">New Media</th><td style=\"font-size: 12px;text-align: center;\">" + String.Format("{0:0.##}", nNewCommercialsTotal) + "</td><td style=\"font-size: 12px;text-align: center;\">" + String.Format("{0:0.##}", nNewCommercialsMounth) + "</td><td style=\"font-size: 12px;text-align: center;\">" + String.Format("{0:0.##}", nNewCommercialsWeek) + "</td><td style=\"font-size: 12px;text-align: center;\">" + String.Format("{0:0.##}", nNewCommercialsDay) + "</td></tr>";
        sRet += "<tr><th style=\"font-size: 12px;\">New Commercials</th><td style=\"font-size: 12px;text-align: center;\">" + String.Format("{0:0.##}", nNewCommercialsTotal) + "</td><td style=\"font-size: 12px;text-align: center;\">" + String.Format("{0:0.##}", nNewCommercialsMounth) + "</td><td style=\"font-size: 12px;text-align: center;\">" + String.Format("{0:0.##}", nNewCommercialsWeek) + "</td><td style=\"font-size: 12px;text-align: center;\">" + String.Format("{0:0.##}", nNewCommercialsDay) + "</td></tr>";
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
        if (nDaysAgo > 0)
        {
            selectQuery += "and wma.create_date>=";
            if (nDaysAgo == 31)
                selectQuery += "DATEADD(m, -1, CONVERT(datetime, LEFT(CONVERT(char(10), getdate(), 20) , 10))) ";
            else
                selectQuery += "DATEADD(d, " + (nDaysAgo * -1).ToString() + " , CONVERT(datetime, LEFT(CONVERT(char(10), getdate(), 20) , 10))) ";

            selectQuery += "and wma.create_date<";
            selectQuery += "CONVERT(datetime, LEFT(CONVERT(char(10), getdate(), 20) , 10)) ";
        }
        if (nDaysAgo == -1)
        {
            selectQuery += "and wma.create_date>";
            selectQuery += "CONVERT(datetime, LEFT(CONVERT(char(10), getdate(), 20) , 10)) ";
        }
        selectQuery += " and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("wma.group_id", "=", LoginManager.GetLoginGroupID());
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
        if (nDaysAgo > 0)
        {
            selectQuery += "wma.create_date>";
            if (nDaysAgo == 31)
                selectQuery += "DATEADD(m, -1, CONVERT(datetime, LEFT(CONVERT(char(10), getdate(), 20) , 10))) ";
            else
                selectQuery += "DATEADD(d, " + (nDaysAgo * -1).ToString() + " , CONVERT(datetime, LEFT(CONVERT(char(10), getdate(), 20) , 10))) ";

            selectQuery += "and wma.create_date<";
            selectQuery += "CONVERT(datetime, LEFT(CONVERT(char(10), getdate(), 20) , 10)) ";
            selectQuery += " and";
        }
        if (nDaysAgo == -1)
        {
            selectQuery += "wma.create_date>";
            selectQuery += "CONVERT(datetime, LEFT(CONVERT(char(10), getdate(), 20) , 10)) and ";
        }
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

    public void GetMediaMounthViewsCountStatistics()
    {
        Int32 nMaxViews = 0;
        string sBar = "";
        string sDot = "";
        Int32 nCount = 0;

        ODBCWrapper.DataSetSelectQuery selectQuery1 = new ODBCWrapper.DataSetSelectQuery();
        selectQuery1 += "select ISNULL(q2.co,0) as co,q1.DD from ";
        selectQuery1 += "(select distinct CONVERT(datetime, LEFT(CONVERT(char(10), create_date, 20) , 10)) as DD  from watchers_media_actions wma where wma.action_id=1 and wma.create_date>DATEADD(m, -1, CONVERT(datetime, LEFT(CONVERT(char(10), getdate(), 20) , 10))))q1 left join ";
        selectQuery1 += "(select  ISNULL(count(distinct watcher_id),0) as co,CONVERT(datetime, LEFT(CONVERT(char(10), create_date, 20) , 10)) as DD  from watchers_media_actions wma where wma.action_id=1 and wma.create_date>DATEADD(m, -1, CONVERT(datetime, LEFT(CONVERT(char(10), getdate(), 20) , 10))) and ";
        selectQuery1 += ODBCWrapper.Parameter.NEW_PARAM("wma.group_id", "=", LoginManager.GetLoginGroupID());
        selectQuery1 += " group by CONVERT(datetime, LEFT(CONVERT(char(10), create_date, 20) , 10)))q2 on (q1.DD=q2.DD) ORDER BY q1.DD";
        if (selectQuery1.Execute("query", true) != null)
        {
            nCount = selectQuery1.Table("query").DefaultView.Count;
            Int32 nLastViews = 0;
            for (int i = 0; i < nCount; i++)
            {
                Int32 nViews = int.Parse(selectQuery1.Table("query").DefaultView[i].Row["co"].ToString());
                string sDate = selectQuery1.Table("query").DefaultView[i].Row["DD"].ToString();
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
        selectQuery += "select ISNULL(q2.co,0) as co,q1.DD from ";
        selectQuery += "(select distinct CONVERT(datetime, LEFT(CONVERT(char(10), create_date, 20) , 10)) as DD from watchers_media_actions wma where wma.create_date>DATEADD(m, -1, CONVERT(datetime, LEFT(CONVERT(char(10), getdate(), 20) , 10))))q1 left join ";
        selectQuery += "(select  ISNULL(count(*),0) as co,CONVERT(datetime, LEFT(CONVERT(char(10), create_date, 20) , 10)) as DD  from watchers_media_actions wma where wma.action_id=1 and wma.create_date>DATEADD(m, -1, CONVERT(datetime, LEFT(CONVERT(char(10), getdate(), 20) , 10))) and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("wma.group_id", "=", LoginManager.GetLoginGroupID());
        selectQuery += " group by CONVERT(datetime, LEFT(CONVERT(char(10), create_date, 20) , 10)))q2 on (q1.DD=q2.DD) ORDER BY q1.DD";
        if (selectQuery.Execute("query", true) != null)
        {
            nCount = selectQuery.Table("query").DefaultView.Count;
            Int32 nLastViews = 0;
            for (int i = 0; i < nCount; i++)
            {
                Int32 nViews = int.Parse(selectQuery.Table("query").DefaultView[i].Row["co"].ToString());
                string sDate = selectQuery.Table("query").DefaultView[i].Row["DD"].ToString();
                sDate = sDate.Split(' ')[0];
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
        sRet += "D.SetText(\"\" ,\"\" , \"Last Month\");\r\n";
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

        Int32 nCount = 0;
        ODBCWrapper.DataSetSelectQuery selectQuery1 = new ODBCWrapper.DataSetSelectQuery();
        selectQuery1 += "select ISNULL(q2.co,0) as co,q1.DD from ";
        selectQuery1 += "(select distinct CONVERT(datetime, CONVERT(char(13), create_date, 20)+':00:00') as DD  from watchers_media_actions wma where wma.create_date>DATEADD(d, -1, CONVERT(datetime, LEFT(CONVERT(char(10), getdate(), 20) , 10))))q1 left join ";
        selectQuery1 += "(select  ISNULL(count(distinct watcher_id),0) as co,CONVERT(datetime, CONVERT(char(13), create_date, 20)+':00:00') as DD  from watchers_media_actions wma where wma.action_id=1 and wma.create_date>DATEADD(d, -1, CONVERT(datetime, LEFT(CONVERT(char(10), getdate(), 20) , 10))) and ";
        selectQuery1 += ODBCWrapper.Parameter.NEW_PARAM("wma.group_id", "=", LoginManager.GetLoginGroupID());
        selectQuery1 += " group by CONVERT(datetime, CONVERT(char(13), create_date, 20)+':00:00'))q2 on (q1.DD=q2.DD) ORDER BY q1.DD";
        if (selectQuery1.Execute("query", true) != null)
        {
            nCount = selectQuery1.Table("query").DefaultView.Count;
            Int32 nLastViews = 0;
            for (int i = 0; i < nCount; i++)
            {
                Int32 nViews = int.Parse(selectQuery1.Table("query").DefaultView[i].Row["co"].ToString());
                string sDate = selectQuery1.Table("query").DefaultView[i].Row["DD"].ToString() + ":00";
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
        selectQuery += "select ISNULL(q2.co,0) as co,q1.DD from ";
        selectQuery += "(select distinct CONVERT(datetime, CONVERT(char(13), create_date, 20)+':00:00') as DD  from watchers_media_actions wma where wma.create_date>DATEADD(d, -1, CONVERT(datetime, LEFT(CONVERT(char(10), getdate(), 20) , 10))))q1 left join ";
        selectQuery += "(select  ISNULL(count(*),0) as co,CONVERT(datetime, CONVERT(char(13), create_date, 20)+':00:00') as DD  from watchers_media_actions wma where wma.action_id=1 and wma.create_date>DATEADD(d, -1, CONVERT(datetime, LEFT(CONVERT(char(10), getdate(), 20) , 10))) and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("wma.group_id", "=", LoginManager.GetLoginGroupID());
        selectQuery += " group by CONVERT(datetime, CONVERT(char(13), create_date, 20)+':00:00'))q2 on (q1.DD=q2.DD) ORDER BY q1.DD";
        if (selectQuery.Execute("query", true) != null)
        {
            nCount = selectQuery.Table("query").DefaultView.Count;
            Int32 nLastViews = 0;
            for (int i = 0; i < nCount; i++)
            {
                Int32 nViews = int.Parse(selectQuery.Table("query").DefaultView[i].Row["co"].ToString());
                string sDate = selectQuery.Table("query").DefaultView[i].Row["DD"].ToString() + ":00";
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
        sRet += "D.SetText(\"\" ,\"\" , \"Last Day\");\r\n";
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
        Int32 nCount = 0;

        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select ISNULL(q2.co,0) as co,q1.DD from ";
        selectQuery += "(select distinct CONVERT(datetime, LEFT(CONVERT(char(10), create_date, 20) , 10)) as DD  from watchers_media_actions wma where wma.create_date>DATEADD(m, -1, CONVERT(datetime, LEFT(CONVERT(char(10), getdate(), 20) , 10))))q1 left join ";
        selectQuery += "(select  ISNULL(sum(PLAY_TIME_COUNTER),0) as co,CONVERT(datetime, LEFT(CONVERT(char(10), create_date, 20) , 10)) as DD  from watchers_media_play_counters wma where wma.create_date>DATEADD(m, -1, CONVERT(datetime, LEFT(CONVERT(char(10), getdate(), 20) , 10))) and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("wma.group_id", "=", LoginManager.GetLoginGroupID());
        selectQuery += " group by CONVERT(datetime, LEFT(CONVERT(char(10), create_date, 20) , 10)))q2 on (q1.DD=q2.DD) ORDER BY q1.DD";
        if (selectQuery.Execute("query", true) != null)
        {
            nCount = selectQuery.Table("query").DefaultView.Count;
            Int32 nLastViews = 0;
            for (int i = 0; i < nCount; i++)
            {
                double dWatches = double.Parse(selectQuery.Table("query").DefaultView[i].Row["co"].ToString()) / 60;
                //Int32 nViews = int.Parse(selectQuery.Table("query").DefaultView[i].Row["co"].ToString());
                string sDate = selectQuery.Table("query").DefaultView[i].Row["DD"].ToString();
                sDate = sDate.Split(' ')[0];
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
        sRet += "D.SetText(\"\" ,\"\" , \"Minutes Last Month\");\r\n";
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
        Int32 nCount = 0;

        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select ISNULL(q2.co,0) as co,q1.DD from ";
        selectQuery += "(select distinct CONVERT(datetime, CONVERT(char(13), create_date, 20)+':00:00') as DD  from watchers_media_actions wma where wma.create_date>DATEADD(d, -1, CONVERT(datetime, LEFT(CONVERT(char(10), getdate(), 20) , 10))))q1 left join ";
        selectQuery += "(select  ISNULL(sum(PLAY_TIME_COUNTER),0) as co,CONVERT(datetime, CONVERT(char(13), create_date, 20)+':00:00') as DD  from watchers_media_play_counters wma where wma.create_date>DATEADD(d, -1, CONVERT(datetime, LEFT(CONVERT(char(10), getdate(), 20) , 10))) and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("wma.group_id", "=", LoginManager.GetLoginGroupID());
        selectQuery += " group by CONVERT(datetime, CONVERT(char(13), create_date, 20)+':00:00'))q2 on (q1.DD=q2.DD) ORDER BY q1.DD";
        if (selectQuery.Execute("query", true) != null)
        {
            nCount = selectQuery.Table("query").DefaultView.Count;
            double dLastViews = 0;
            for (int i = 0; i < nCount; i++)
            {
                double dWatches = double.Parse(selectQuery.Table("query").DefaultView[i].Row["co"].ToString()) / 60;
                //Int32 nViews = int.Parse(selectQuery.Table("query").DefaultView[i].Row["co"].ToString());
                string sDate = selectQuery.Table("query").DefaultView[i].Row["DD"].ToString() + ":00";
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
        sRet += "D.SetText(\"\" ,\"\" , \"Minutes Last Day\");\r\n";
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
        if (nDays > 0)
        {
            if (nDays == 31)
                selectQuery += " wmpc.create_date>DATEADD(m, -1, CONVERT(datetime, LEFT(CONVERT(char(10), getdate(), 20) , 10))) and";
            else
                selectQuery += " wmpc.create_date>DATEADD(d, " + (nDays * -1).ToString() + ", CONVERT(datetime, LEFT(CONVERT(char(10), getdate(), 20) , 10))) and";
        }
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("wmpc.group_id", "=", LoginManager.GetLoginGroupID());

        selectQuery += " and  c.id=wmpc.country_id group by c.id,c.country_name order by co desc";
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
                if (dPer > 0)
                {
                    dAngle1 += (((double)nCO / (double)nTotal) * 360);
                    sRet += "P" + nDays.ToString() + "[" + i.ToString() + "]=new Pie(220,110,0,100," + dAngle0.ToString() + "," + dAngle1.ToString() + ",\"" + System.Drawing.ColorTranslator.ToHtml(System.Drawing.ColorTranslator.FromWin32((255 - nID) * 700)) + "\",\"" + sCountryName + " - " + dPer.ToString() + "%\");\r\n";
                    dAngle0 = dAngle1;
                    nColor++;
                }
            }
        }
        selectQuery.Finish();
        selectQuery = null;
        if (nDays == 31)
            sRet += "new Box(1,5,80,5,\"#FFFFFF\" , \"Countries Last Month\" , \"\" , 2);";
        if (nDays == 1)
            sRet += "new Box(1,5,80,5,\"#FFFFFF\" , \"Countries Last Day\" , \"\" , 2);";
        return sRet;
    }

    public string GetMediaPlayersStatisticsStr(Int32 nDays)
    {
        string sRet = "var PP" + nDays.ToString() + " = new Array();\r\n";
        Int32 nTotal = 0;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        string sGroups = PageUtils.GetParentsGroupsStr(LoginManager.GetLoginGroupID());
        selectQuery += "select c.id,c.DOMAIN,count(*) as co from watchers_media_play_counters wmpc,groups_passwords c,media m where m.id=wmpc.media_id and c.group_id  ";
        selectQuery += sGroups += " and ";
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
        selectQuery += " c.id=wmpc.PLAYER_ID group by c.id,c.DOMAIN order by co desc";
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
