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

public partial class adm_commercials_statistics : System.Web.UI.Page
{
    protected string m_sMenu;
    protected string m_sSubMenu;
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
            Int32 nMenuID = 0;

            m_sMenu = TVinciShared.Menu.GetMainMenu(13, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 2, true);
            if (Request.QueryString["commercial_id"] != null &&
                Request.QueryString["commercial_id"].ToString() != "")
            {
                Session["commercial_id"] = int.Parse(Request.QueryString["commercial_id"].ToString());
                Int32 nOwnerGroupID = int.Parse(PageUtils.GetTableSingleVal("commercial", "group_id", int.Parse(Session["commercial_id"].ToString())).ToString());
                Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();

                if (PageUtils.GetUpperGroupID(nOwnerGroupID) != PageUtils.GetUpperGroupID(nLogedInGroupID) && PageUtils.IsTvinciUser() == false)
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
        }
    }

    public void GetHeader()
    {
        string sName = PageUtils.GetTableSingleVal("commercial", "name", int.Parse(Session["commercial_id"].ToString())).ToString();
        Response.Write(PageUtils.GetPreHeader() + ": commercial Statistics: " + sName);
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
        selectQuery += ") as co from watchers_media_actions wmpc,countries c where wmpc.action_id=4 and ";
        if (nDaysAgo > 0)
        {
            selectQuery += " wmpc.create_date>";
            if (nDaysAgo == 31)
                selectQuery += "DATEADD(m, -1, getdate()) and ";
            else
                selectQuery += "DATEADD(d, " + (nDaysAgo * -1).ToString() + " , getdate()) and ";
        }
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("wmpc.media_id", "=", int.Parse(Session["commercial_id"].ToString()));
        selectQuery += "and c.id=wmpc.country_id group by c.country_name order by co desc";
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
                theCountries[sCountryName] += "<td ";
                if (nDaysAgo == 1)
                    theCountries[sCountryName] += " onmouseout=\"P1[" + i.ToString() + "].MoveTo('','',0);\" onmouseover=\"P1[" + i.ToString() + "].MoveTo('','',10);\" ";
                if (nDaysAgo == 31)
                    theCountries[sCountryName] += " onmouseout=\"P31[" + i.ToString() + "].MoveTo('','',0);\" onmouseover=\"P31[" + i.ToString() + "].MoveTo('','',10);\" ";
                theCountries[sCountryName] += " style=\"font-size: 12px;text-align: center;\">" + dPer.ToString() + "%</td>";
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
        selectQuery += "select c.domain,count(";
        if (bUnique == true)
            selectQuery += "distinct watcher_id";
        else
            selectQuery += "*";
        selectQuery += ") as co from watchers_media_actions wmpc,groups_passwords c where wmpc.action_id=4 and ";
        if (nDaysAgo > 0)
        {
            selectQuery += " wmpc.create_date>";
            if (nDaysAgo == 31)
                selectQuery += "DATEADD(m, -1, getdate()) and ";
            else
                selectQuery += "DATEADD(d, " + (nDaysAgo * -1).ToString() + " , getdate()) and ";
        }
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("wmpc.media_id", "=", int.Parse(Session["commercial_id"].ToString()));
        selectQuery += "and c.id=wmpc.player_id group by c.domain order by co desc";
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            for (int i = 0; i < nCount; i++)
            {
                string sCountryName = selectQuery.Table("query").DefaultView[i].Row["domain"].ToString();
                Int32 nCO = int.Parse(selectQuery.Table("query").DefaultView[i].Row["co"].ToString());
                nTotal += nCO;
            }
            for (int i = 0; i < nCount; i++)
            {
                string sSomainName = selectQuery.Table("query").DefaultView[i].Row["domain"].ToString();
                Int32 nCO = int.Parse(selectQuery.Table("query").DefaultView[i].Row["co"].ToString());
                double dPer = Math.Round(((double)nCO / (double)nTotal) * 100, 2);
                thePlayers[sSomainName] += "<td ";
                if (nDaysAgo == 1)
                    thePlayers[sSomainName] += " onmouseout=\"PP1[" + i.ToString() + "].MoveTo('','',0);\" onmouseover=\"PP1[" + i.ToString() + "].MoveTo('','',10);\" ";
                if (nDaysAgo == 31)
                    thePlayers[sSomainName] += " onmouseout=\"PP31[" + i.ToString() + "].MoveTo('','',0);\" onmouseover=\"PP31[" + i.ToString() + "].MoveTo('','',10);\" ";
                thePlayers[sSomainName] += " style=\"font-size: 12px;text-align: center;\">" + dPer.ToString() + "%</td>";
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
        string sName = PageUtils.GetTableSingleVal("commercial", "name", int.Parse(Session["commercial_id"].ToString())).ToString();
        string sRet = "<table width=100%><tr><td colspan=\"2\" width=\"100%\" align=\"center\" class=\"adm_table_header\">" + sName + " - Geo Summery</td></tr>";
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
        sRet += "<tr><th style=\"font-size: 12px;width: 28%;\"></th><th style=\"font-size: 12px;width: 18%;\">Total</th><th style=\"font-size: 12px;width: 18%;\">Last Mounth</th><th style=\"font-size: 12px;width: 18%;\">Last Week</th><th style=\"font-size: 12px;width: 18%;\">Last Day</th></tr>";
        sRet += GetGEOSummeryIntr(false);
        sRet += "</table>";
        sRet += "</td></tr>";
        sRet += "</table>";
        Response.Write(sRet);
    }

    public void GetPlayersSummery()
    {
        string sName = PageUtils.GetTableSingleVal("commercial", "name", int.Parse(Session["commercial_id"].ToString())).ToString();
        string sRet = "<table width=100%><tr><td colspan=\"2\" width=\"100%\" align=\"center\" class=\"adm_table_header\">" + sName + " - Players Summery</td></tr>";
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
        sRet += "<tr><th style=\"font-size: 12px;width: 28%;\"></th><th style=\"font-size: 12px;width: 18%;\">Total</th><th style=\"font-size: 12px;width: 18%;\">Last Mounth</th><th style=\"font-size: 12px;width: 18%;\">Last Week</th><th style=\"font-size: 12px;width: 18%;\">Last Day</th></tr>";
        sRet += GetPlayersSummeryIntr(false);
        sRet += "</table>";
        sRet += "</td></tr>";
        sRet += "</table>";
        Response.Write(sRet);
    }

    public void GetWatchesSummery()
    {
        string sName = PageUtils.GetTableSingleVal("commercial", "name", int.Parse(Session["commercial_id"].ToString())).ToString();
        string sRet = "<table width=100%><tr><td width=\"100%\" align=\"center\" class=\"adm_table_header\">" + sName + " - Views Summery</td></tr>";
        sRet += "<tr><td width=\"100%\" nowrap>";
        sRet += "<table width=\"100%\" style=\"border: solid 1px #cccccc;\">";
        sRet += "<tr><th style=\"font-size: 12px;width: 28%;\"></th><th style=\"font-size: 12px;width: 18%;\">Total</th><th style=\"font-size: 12px;width: 18%;\">Last Mounth</th><th style=\"font-size: 12px;width: 18%;\">Last Week</th><th style=\"font-size: 12px;width: 18%;\">Last Day</th></tr>";

        Int32 nWatchCountDay = GetNumberOfWatches(false, 1 , 4);
        Int32 nWatchCountWeek = GetNumberOfWatches(false, 7 , 4);
        Int32 nWatchCountMonth = GetNumberOfWatches(false, 31 , 4);
        Int32 nWatchCountTotal = GetNumberOfWatches(false, 0 , 4);

        Int32 nWatchCountUniqueDay = GetNumberOfWatches(true, 1 , 4);
        Int32 nWatchCountUniqueWeek = GetNumberOfWatches(true, 7 , 4);
        Int32 nWatchCountUniqueMonth = GetNumberOfWatches(true, 31 , 4);
        Int32 nWatchCountUniqueTotal = GetNumberOfWatches(true, 0 , 4);

        Int32 nClickCountDay = GetNumberOfWatches(false, 1, 5);
        Int32 nClickCountWeek = GetNumberOfWatches(false, 7, 5);
        Int32 nClickCountMonth = GetNumberOfWatches(false, 31, 5);
        Int32 nClickCountTotal = GetNumberOfWatches(false, 0, 5);

        Int32 nClickCountUniqueDay = GetNumberOfWatches(true, 1, 5);
        Int32 nClickCountUniqueWeek = GetNumberOfWatches(true, 7, 5);
        Int32 nClickCountUniqueMonth = GetNumberOfWatches(true, 31, 5);
        Int32 nClickCountUniqueTotal = GetNumberOfWatches(true, 0, 5);

        double dAvgDayWatchCount = Math.Round(nClickCountDay / (double)nWatchCountDay, 2) * 100;
        double dAvgWeekWatchCount = Math.Round(nClickCountWeek / (double)nWatchCountWeek, 2) * 100;
        double dAvgMonthWatchCount = Math.Round(nClickCountMonth / (double)nWatchCountMonth, 2) * 100;
        double dAvgTotalWatchCount = Math.Round(nClickCountTotal / (double)nWatchCountTotal, 2) * 100;

        double dAvgDayWatchCountUnique = Math.Round(nClickCountUniqueDay / (double)nWatchCountUniqueDay, 2) * 100;
        double dAvgWeekWatchCountUnique = Math.Round(nClickCountUniqueWeek / (double)nWatchCountUniqueWeek, 2) * 100;
        double dAvgMonthWatchCountUnique = Math.Round(nClickCountUniqueMonth / (double)nWatchCountUniqueMonth, 2) * 100;
        double dAvgTotalWatchCountUnique = Math.Round(nClickCountUniqueTotal / (double)nWatchCountUniqueTotal, 2) * 100;

        sRet += "<tr><th style=\"font-size: 12px;\">Views Count</th><td style=\"font-size: 12px;text-align: center;\">" + String.Format("{0:0.##}", nWatchCountTotal) + "</td><td style=\"font-size: 12px;text-align: center;\">" + String.Format("{0:0.##}", nWatchCountMonth) + "</td><td style=\"font-size: 12px;text-align: center;\">" + String.Format("{0:0.##}", nWatchCountWeek) + "</td><td style=\"font-size: 12px;text-align: center;\">" + String.Format("{0:0.##}", nWatchCountDay) + "</td></tr>";
        sRet += "<tr><th style=\"font-size: 12px;\">Unique Count</th><td style=\"font-size: 12px;text-align: center;\">" + String.Format("{0:0.##}", nWatchCountUniqueTotal) + "</td><td style=\"font-size: 12px;text-align: center;\">" + String.Format("{0:0.##}", nWatchCountUniqueMonth) + "</td><td style=\"font-size: 12px;text-align: center;\">" + String.Format("{0:0.##}", nWatchCountUniqueWeek) + "</td><td style=\"font-size: 12px;text-align: center;\">" + String.Format("{0:0.##}", nWatchCountUniqueDay) + "</td></tr>";
        sRet += "<tr><th style=\"font-size: 12px;\">Click Count</th><td style=\"font-size: 12px;text-align: center;\">" + String.Format("{0:0.##}", nClickCountTotal) + "</td><td style=\"font-size: 12px;text-align: center;\">" + String.Format("{0:0.##}", nClickCountMonth) + "</td><td style=\"font-size: 12px;text-align: center;\">" + String.Format("{0:0.##}", nClickCountWeek) + "</td><td style=\"font-size: 12px;text-align: center;\">" + String.Format("{0:0.##}", nClickCountDay) + "</td></tr>";
        sRet += "<tr><th style=\"font-size: 12px;\">Unique Click Count</th><td style=\"font-size: 12px;text-align: center;\">" + String.Format("{0:0.##}", nClickCountUniqueTotal) + "</td><td style=\"font-size: 12px;text-align: center;\">" + String.Format("{0:0.##}", nClickCountUniqueMonth) + "</td><td style=\"font-size: 12px;text-align: center;\">" + String.Format("{0:0.##}", nClickCountUniqueWeek) + "</td><td style=\"font-size: 12px;text-align: center;\">" + String.Format("{0:0.##}", nClickCountUniqueDay) + "</td></tr>";
        sRet += "<tr><th style=\"font-size: 12px;\">Click/Watch (%)</th><td style=\"font-size: 12px;text-align: center;\">" + String.Format("{0:0.##}", dAvgTotalWatchCount) + "</td><td style=\"font-size: 12px;text-align: center;\">" + String.Format("{0:0.##}", dAvgMonthWatchCount) + "</td><td style=\"font-size: 12px;text-align: center;\">" + String.Format("{0:0.##}", dAvgWeekWatchCount) + "</td><td style=\"font-size: 12px;text-align: center;\">" + String.Format("{0:0.##}", dAvgDayWatchCount) + "</td></tr>";
        sRet += "<tr><th style=\"font-size: 12px;\">Click/Watch Unique(%)</th><td style=\"font-size: 12px;text-align: center;\">" + String.Format("{0:0.##}", dAvgTotalWatchCountUnique) + "</td><td style=\"font-size: 12px;text-align: center;\">" + String.Format("{0:0.##}", dAvgMonthWatchCountUnique) + "</td><td style=\"font-size: 12px;text-align: center;\">" + String.Format("{0:0.##}", dAvgWeekWatchCountUnique) + "</td><td style=\"font-size: 12px;text-align: center;\">" + String.Format("{0:0.##}", dAvgDayWatchCountUnique) + "</td></tr>";
        //sRet += "</tr>";
        sRet += "</table>";
        sRet += "</td></tr>";
        sRet += "</table>";
        Response.Write(sRet);
    }

    protected Int32 GetNumberOfWatches(bool bUnique, Int32 nDaysAgo , Int32 nActionID)
    {
        Int32 nRet = 0;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select  count(";
        if (bUnique == true)
            selectQuery += "distinct watcher_id";
        else
            selectQuery += "*";
        selectQuery += ") as co from watchers_media_actions wma where ";
        if (nDaysAgo > 0)
        {
            selectQuery += " wma.create_date>";
            if (nDaysAgo == 31)
                selectQuery += "DATEADD(m, -1, getdate()) and ";
            else
                selectQuery += "DATEADD(d, " + (nDaysAgo * -1).ToString() + " , getdate()) and ";
        }
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("wma.media_id", "=", int.Parse(Session["commercial_id"].ToString()));
        selectQuery += " and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("action_id", "=", nActionID);
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
            selectQuery += " wma.create_date>";
            if (nDaysAgo == 31)
                selectQuery += "DATEADD(m, -1, getdate()) and ";
            else
                selectQuery += "DATEADD(d, " + (nDaysAgo * -1).ToString() + " , getdate()) and ";
        }
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("wma.media_id", "=", int.Parse(Session["commercial_id"].ToString()));
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

    public void GetMediaMounthViewsCountStatistics(Int32 nActionID)
    {
        if (Session["commercial_id"] == null)
            return;
        Int32 nMaxViews = 0;
        string sBar = "";
        string sDot = "";
        string sName = PageUtils.GetTableSingleVal("commercial", "name", int.Parse(Session["commercial_id"].ToString())).ToString();
        string sID = Session["commercial_id"].ToString();
        Int32 nCount = 0;

        ODBCWrapper.DataSetSelectQuery selectQuery1 = new ODBCWrapper.DataSetSelectQuery();
        selectQuery1 += "select ISNULL(q2.co,0) as co,q1.DD,q1.MM from ";
        selectQuery1 += "(select distinct DatePart(Dd,wma.create_date) as DD,DatePart(Mm,wma.create_date) as MM,DateDiff(dd,'1/1/2000',wma.create_date) as DY  from watchers_media_actions wma where wma.create_date>DATEADD(m, -1, getdate()))q1 left join ";
        selectQuery1 += "(select  ISNULL(count(distinct watcher_id),0) as co,DatePart(Dd,wma.create_date) as DD,DatePart(Mm,wma.create_date) as MM,DateDiff(dd,'1/1/2000',wma.create_date) as DY  from watchers_media_actions wma where ";
        selectQuery1 += ODBCWrapper.Parameter.NEW_PARAM("action_id", "=", nActionID);
        selectQuery1 += " and  wma.create_date>DATEADD(m, -1, getdate()) and ";
        selectQuery1 += ODBCWrapper.Parameter.NEW_PARAM("wma.media_id", "=", int.Parse(Session["commercial_id"].ToString()));
        selectQuery1 += " group by DatePart(Dd,wma.create_date),DatePart(Mm,wma.create_date),DateDiff(dd,'1/1/2000',wma.create_date))q2 on (q1.DD=q2.DD and q1.MM=q2.MM and q1.DY=q2.DY) ORDER BY q1.DY,q1.MM,q1.DD";
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
        selectQuery += "(select distinct DatePart(Dd,wma.create_date) as DD,DatePart(Mm,wma.create_date) as MM,DateDiff(dd,'1/1/2000',wma.create_date) as DY  from watchers_media_actions wma where wma.create_date>DATEADD(m, -1, getdate()))q1 left join ";
        selectQuery += "(select  ISNULL(count(*),0) as co,DatePart(Dd,wma.create_date) as DD,DatePart(Mm,wma.create_date) as MM,DateDiff(dd,'1/1/2000',wma.create_date) as DY  from watchers_media_actions wma where wma.create_date>DATEADD(m, -1, getdate()) and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("action_id", "=", nActionID);
        selectQuery += " and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("wma.media_id", "=", int.Parse(Session["commercial_id"].ToString()));
        selectQuery += " group by DatePart(Dd,wma.create_date),DatePart(Mm,wma.create_date),DateDiff(dd,'1/1/2000',wma.create_date))q2 on (q1.DD=q2.DD and q1.MM=q2.MM and q1.DY=q2.DY) ORDER BY q1.DY,q1.MM,q1.DD";
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
        if (nActionID == 5)
            sRet += "D.SetText(\"\" ,\"\" , \"" + sName + " - Clicks Last Month\");\r\n";
        if (nActionID == 4)
            sRet += "D.SetText(\"\" ,\"\" , \"" + sName + " - Views Last Month\");\r\n";
        sRet += "D.XScale=0;\r\n";
        sRet += "D.SetGridColor(\"#cccccc\");\r\n";
        Int32 nAddToMax = 1;
        if (nMaxViews > 10)
            nAddToMax = 3;
        if (nMaxViews > 100)
            nAddToMax = 10;
        sRet += "D.SetBorder(0, " + (nCount - 1).ToString() + ", 0, " + (nMaxViews + nAddToMax).ToString() + ");\r\n";
        sRet += "D.Draw(\"#FFFFFF\",\"#666666\",false);\r\n";
        if (nActionID == 4)
            sBar += "new Box(D.ScreenX(1),D.ScreenY(" + (nMaxViews + nAddToMax).ToString() + ")+5,D.ScreenX(1) + 80,D.ScreenY(" + (nMaxViews + nAddToMax).ToString() + ")+5,\"#66ccff\" , \"Views\" , \"\" , 2);";
        if (nActionID == 5)
            sBar += "new Box(D.ScreenX(1),D.ScreenY(" + (nMaxViews + nAddToMax).ToString() + ")+5,D.ScreenX(1) + 80,D.ScreenY(" + (nMaxViews + nAddToMax).ToString() + ")+5,\"#66ccff\" , \"Clicks\" , \"\" , 2);";
        sBar += "new Box(D.ScreenX(1),D.ScreenY(" + (nMaxViews + nAddToMax).ToString() + ")+25,D.ScreenX(1) + 80,D.ScreenY(" + (nMaxViews + nAddToMax).ToString() + ")+25,\"#cccccc\" , \"Unique\" , \"\" , 2);";

        sRet += sBar + sDot;
        Response.Write(sRet);
    }

    public void GetMediaDayViewCountStatistics(Int32 nActionID)
    {
        if (Session["commercial_id"] == null)
            return;
        Int32 nMaxViews = 0;
        string sBar = "";
        string sDot = "";
        string sName = PageUtils.GetTableSingleVal("commercial", "name", int.Parse(Session["commercial_id"].ToString())).ToString();
        string sID = Session["commercial_id"].ToString();

        Int32 nCount = 0;
        ODBCWrapper.DataSetSelectQuery selectQuery1 = new ODBCWrapper.DataSetSelectQuery();
        selectQuery1 += "select ISNULL(q2.co,0) as co,q1.HH from ";
        selectQuery1 += "(select distinct DatePart(HH,wma.create_date) as HH,DateDiff(hh,'1/1/2000',wma.create_date) as DY  from watchers_media_actions wma where wma.create_date>DATEADD(d, -1, getdate()))q1 left join ";
        selectQuery1 += "(select  ISNULL(count(distinct watcher_id),0) as co,DatePart(HH,wma.create_date) as HH,DateDiff(hh,'1/1/2000',wma.create_date) as DY  from watchers_media_actions wma where wma.create_date>DATEADD(d, -1, getdate()) and ";
        selectQuery1 += ODBCWrapper.Parameter.NEW_PARAM("wma.action_id", "=", nActionID);
        selectQuery1 += " and ";
        selectQuery1 += ODBCWrapper.Parameter.NEW_PARAM("wma.media_id", "=", int.Parse(Session["commercial_id"].ToString()));
        selectQuery1 += " group by DatePart(HH,wma.create_date),DateDiff(hh,'1/1/2000',wma.create_date))q2 on (q1.HH=q2.HH and q1.DY=q2.DY) ORDER BY q1.DY,q1.HH";
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
        selectQuery += "(select distinct DatePart(HH,wma.create_date) as HH,DateDiff(hh,'1/1/2000',wma.create_date) as DY  from watchers_media_actions wma where wma.create_date>DATEADD(d, -1, getdate()))q1 left join ";
        selectQuery += "(select  ISNULL(count(*),0) as co,DatePart(HH,wma.create_date) as HH,DateDiff(hh,'1/1/2000',wma.create_date) as DY  from watchers_media_actions wma where wma.create_date>DATEADD(d, -1, getdate()) and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("wma.action_id", "=", nActionID);
        selectQuery += "and";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("wma.media_id", "=", int.Parse(Session["commercial_id"].ToString()));
        selectQuery += " group by DatePart(HH,wma.create_date),DateDiff(hh,'1/1/2000',wma.create_date))q2 on (q1.HH=q2.HH and q1.DY=q2.DY) ORDER BY q1.DY,q1.HH";
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
        if (nActionID == 5)
            sRet += "D.SetText(\"\" ,\"\" , \"" + sName + " - Clicks Last Day\");\r\n";
        if (nActionID == 4)
            sRet += "D.SetText(\"\" ,\"\" , \"" + sName + " - Views Last Day\");\r\n";
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
        if (nActionID == 4)
            sBar += "new Box(D.ScreenX(1),D.ScreenY(" + (nMaxViews + nAddToMax).ToString() + ")+5,D.ScreenX(1) + 80,D.ScreenY(" + (nMaxViews + nAddToMax).ToString() + ")+5,\"#66ccff\" , \"Views\" , \"\" , 2);";
        if (nActionID == 5)
            sBar += "new Box(D.ScreenX(1),D.ScreenY(" + (nMaxViews + nAddToMax).ToString() + ")+5,D.ScreenX(1) + 80,D.ScreenY(" + (nMaxViews + nAddToMax).ToString() + ")+5,\"#66ccff\" , \"Clicks\" , \"\" , 2);";
        sBar += "new Box(D.ScreenX(1),D.ScreenY(" + (nMaxViews + nAddToMax).ToString() + ")+25,D.ScreenX(1) + 80,D.ScreenY(" + (nMaxViews + nAddToMax).ToString() + ")+25,\"#cccccc\" , \"Unique\" , \"\" , 2);";

        sRet += sBar + sDot;
        Response.Write(sRet);
    }

    public void GetMediaMounthViewsTimeStatistics()
    {
        if (Session["commercial_id"] == null)
            return;
        Int32 nMaxViews = 0;
        string sBar = "";
        string sDot = "";
        string sName = PageUtils.GetTableSingleVal("commercial", "name", int.Parse(Session["commercial_id"].ToString())).ToString();
        string sID = Session["commercial_id"].ToString();
        Int32 nCount = 0;

        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select ISNULL(q2.co,0) as co,q1.DD,q1.MM from ";
        selectQuery += "(select distinct DatePart(Dd,wma.create_date) as DD,DatePart(Mm,wma.create_date) as MM,DateDiff(dd,'1/1/2000',wma.create_date) as DY  from watchers_media_actions wma where wma.create_date>DATEADD(m, -1, getdate()))q1 left join ";
        selectQuery += "(select  ISNULL(sum(PLAY_TIME_COUNTER),0) as co,DatePart(Dd,wma.create_date) as DD,DatePart(Mm,wma.create_date) as MM,DateDiff(dd,'1/1/2000',wma.create_date) as DY  from watchers_media_play_counters wma where wma.create_date>DATEADD(m, -1, getdate()) and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("wma.media_id", "=", int.Parse(Session["commercial_id"].ToString()));
        selectQuery += " group by DatePart(Dd,wma.create_date),DatePart(Mm,wma.create_date),DateDiff(dd,'1/1/2000',wma.create_date))q2 on (q1.DD=q2.DD and q1.MM=q2.MM and q1.DY=q2.DY) ORDER BY q1.DY,q1.MM,q1.DD";
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
        if (Session["commercial_id"] == null)
            return;
        Int32 nMaxViews = 0;
        string sBar = "";
        string sDot = "";
        string sName = PageUtils.GetTableSingleVal("commercial", "name", int.Parse(Session["commercial_id"].ToString())).ToString();
        string sID = Session["commercial_id"].ToString();
        Int32 nCount = 0;

        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select ISNULL(q2.co,0) as co,q1.HH from ";
        selectQuery += "(select distinct DatePart(HH,wma.create_date) as HH,DateDiff(hh,'1/1/2000',wma.create_date) as DY  from watchers_media_actions wma where wma.create_date>DATEADD(d, -1, getdate()))q1 left join ";
        selectQuery += "(select  ISNULL(sum(PLAY_TIME_COUNTER),0) as co,DatePart(HH,wma.create_date) as HH,DateDiff(hh,'1/1/2000',wma.create_date) as DY  from watchers_media_play_counters wma where wma.create_date>DATEADD(d, -1, getdate()) and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("wma.media_id", "=", int.Parse(Session["commercial_id"].ToString()));
        selectQuery += " group by DatePart(HH,wma.create_date),DateDiff(hh,'1/1/2000',wma.create_date))q2 on (q1.HH=q2.HH and q1.DY=q2.DY) ORDER BY q1.DY,q1.HH";
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

    public void GetMediaMounthGeoStatistics()
    {
        Response.Write(GetMediaGeoStatisticsStr(31));
    }

    public string GetMediaGeoStatisticsStr(Int32 nDays)
    {
        if (Session["commercial_id"] == null)
            return "";
        string sRet = "var P" + nDays.ToString() + " = new Array();\r\n";
        Int32 nTotal = 0;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select c.id,c.country_name,count(*) as co from watchers_media_actions wmpc,countries c where wmpc.action_id=4 and ";
        if (nDays > 0)
        {
            if (nDays == 31)
                selectQuery += " wmpc.create_date>DATEADD(m, -1, getdate()) and";
            else
                selectQuery += " wmpc.create_date>DATEADD(d, " + (nDays * -1).ToString() + ", getdate()) and";
        }

        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("wmpc.media_id", "=", int.Parse(Session["commercial_id"].ToString()));
        selectQuery += "and c.id=wmpc.country_id group by c.id,c.country_name order by co desc";
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
        if (Session["commercial_id"] == null)
            return "";
        string sRet = "var PP" + nDays.ToString() + " = new Array();\r\n";
        Int32 nTotal = 0;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select c.id,c.domain,count(*) as co from watchers_media_actions wmpc,groups_passwords c where wmpc.action_id=4 and ";
        if (nDays > 0)
        {
            if (nDays == 31)
                selectQuery += " wmpc.create_date>DATEADD(m, -1, getdate()) and";
            else
                selectQuery += " wmpc.create_date>DATEADD(d, " + (nDays * -1).ToString() + ", getdate()) and";
        }

        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("wmpc.media_id", "=", int.Parse(Session["commercial_id"].ToString()));
        selectQuery += "and c.id=wmpc.player_id group by c.id,c.domain order by co desc";
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            for (int i = 0; i < nCount; i++)
            {
                string sDomainName = selectQuery.Table("query").DefaultView[i].Row["domain"].ToString();
                Int32 nCO = int.Parse(selectQuery.Table("query").DefaultView[i].Row["co"].ToString());
                nTotal += nCO;
            }
            double dAngle0 = 0;
            double dAngle1 = 0;
            Int32 nColor = 0;
            for (int i = 0; i < nCount; i++)
            {
                string sDomainName = selectQuery.Table("query").DefaultView[i].Row["domain"].ToString();
                Int32 nCO = int.Parse(selectQuery.Table("query").DefaultView[i].Row["co"].ToString());
                Int32 nID = int.Parse(selectQuery.Table("query").DefaultView[i].Row["id"].ToString());
                double dPer = Math.Round(((double)nCO / (double)nTotal) * 100, 2);

                dAngle1 += (((double)nCO / (double)nTotal) * 360);
                sRet += "PP" + nDays.ToString() + "[" + i.ToString() + "]=new Pie(220,110,0,100," + dAngle0.ToString() + "," + dAngle1.ToString() + ",\"" + System.Drawing.ColorTranslator.ToHtml(System.Drawing.ColorTranslator.FromWin32((255 - nID) * 700)) + "\",\"" + sDomainName + " - " + dPer.ToString() + "%\");\r\n";
                dAngle0 = dAngle1;
                nColor++;
            }
        }
        selectQuery.Finish();
        selectQuery = null;
        if (nDays == 31)
            sRet += "new Box(1,5,80,5,\"#FFFFFF\" , \"Players Last Mounth\" , \"\" , 2);";
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
