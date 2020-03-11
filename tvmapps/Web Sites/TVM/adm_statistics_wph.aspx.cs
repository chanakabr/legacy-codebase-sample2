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

public partial class adm_statistics_wph : System.Web.UI.Page
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
                m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, false);
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
                m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, false);
            }
            if (Request.QueryString["start_date"] != null)
            {
                DateTime tStart = DateUtils.GetDateFromStr(Request.QueryString["start_date"].ToString());
                Session["start_date"] = tStart.Date;
            }
            else
                Session["start_date"] = DateTime.Now.Date.AddMonths(-2);

            if (Request.QueryString["end_date"] != null)
            {
                DateTime tEnd = DateUtils.GetDateFromStr(Request.QueryString["end_date"].ToString());
                Session["end_date"] = tEnd.Date;
            }
            else
                Session["end_date"] = DateTime.Now.Date;

            if (Request.QueryString["search_save"] != null)
                Session["search_save"] = "1";
            else
                Session["search_save"] = null;
        }
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
    
    protected string GetMediaIDsForChannel()
    {
        if (HttpContext.Current.Cache["GetMediaIDsForChannel" + Session["channel_id"].ToString()] != null)
            return HttpContext.Current.Cache["GetMediaIDsForChannel" + Session["channel_id"].ToString()].ToString();
        TVinciShared.Channel c = new TVinciShared.Channel(int.Parse(Session["channel_id"].ToString()) , false , 0 , true , 0 , 0);
        string s = c.GetChannelMediaIDs();
        if (s == "")
            s = "0";
        HttpContext.Current.Cache["GetMediaIDsForChannel" + Session["channel_id"].ToString()] = s;
        return s;
    }

    public void GetWatchesSummery()
    {
        string sRet = "<table width=100%><tr><td width=\"100%\" align=\"center\" class=\"adm_table_header\">Views Summary for period: ";
        sRet += DateUtils.GetStrFromDate((DateTime)(Session["start_date"]));
        sRet += " - ";
        sRet += DateUtils.GetStrFromDate((DateTime)(Session["end_date"]));
        sRet += "</td></tr>";
        sRet += "<tr><td width=\"100%\" nowrap>";
        sRet += "<table width=\"100%\" style=\"border: solid 1px #cccccc;\">";
        sRet += "<tr><th style=\"font-size: 12px;width: 28%;\"></th><th style=\"font-size: 12px;width: 18%;\">Total For Period</th></tr>";
        Int32 nWatchCount = GetNumberOfWatches(false);

        Int32 nWatchCountUnique = GetNumberOfWatches(true);

        double dAvgTotalWatchPerUnique = Math.Round(nWatchCount / (double)nWatchCountUnique, 2);

        double nWatchTime = GetTimeOfWatches();

        double dAvgWatchCount = Math.Round(nWatchTime / (double)nWatchCount, 2);

        double dAvgTotalWatchCountUnique = Math.Round(nWatchTime / (double)nWatchCountUnique, 2);

        sRet += "<tr><th style=\"font-size: 12px;\">Views Count</th><td style=\"font-size: 12px;text-align: center;\">" + String.Format("{0:0,0.##}", nWatchCount) + "</td></tr>";
        sRet += "<tr><th style=\"font-size: 12px;\">Unique Count</th><td style=\"font-size: 12px;text-align: center;\">" + String.Format("{0:0,0.##}", nWatchCountUnique) + "</td></tr>";
        sRet += "<tr><th style=\"font-size: 12px;\">Watch Per Unique</th><td style=\"font-size: 12px;text-align: center;\">" + String.Format("{0:0,0.##}", dAvgTotalWatchPerUnique) + "</td></tr>";
        sRet += "<tr><th style=\"font-size: 12px;\">Watch Time (Min)</th><td style=\"font-size: 12px;text-align: center;\">" + String.Format("{0:0,0.##}", nWatchTime) + "</td></tr>";
        sRet += "<tr><th style=\"font-size: 12px;\">Avg Watch Time (Min)</th><td style=\"font-size: 12px;text-align: center;\">" + String.Format("{0:0,0.##}", dAvgWatchCount) + "</td></tr>";
        sRet += "<tr><th style=\"font-size: 12px;\">Avg Unique Watch Time (Min)</th><td style=\"font-size: 12px;text-align: center;\">" + String.Format("{0:0,0.##}", dAvgTotalWatchCountUnique) + "</td></tr>";
        //sRet += "</tr>";
        sRet += "</table>";
        sRet += "</td></tr>";
        sRet += "</table>";
        Response.Write(sRet);
    }

    public void GetSearchPannel()
    {
        DateTime dStart = (DateTime)(Session["start_date"]);
        DateTime dEnd = (DateTime)(Session["end_date"]);
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


    protected Int32 GetNumberOfWatches(bool bUnique)
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

        selectQuery += "and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("wma.create_date", ">=", (DateTime)(Session["start_date"]));
        selectQuery += "and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("wma.create_date", "<", (DateTime)(Session["end_date"]));
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

    protected double GetTimeOfWatches()
    {
        Int32 nRet = 0;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select  ISNULL(sum(PLAY_TIME_COUNTER),0) as co";
        selectQuery += " from watchers_media_play_counters wma where ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("wma.group_id", "=", LoginManager.GetLoginGroupID());
        string sStartDay = DateTime.Now.Date.ToString();
        selectQuery += " and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("wma.create_date", ">=", (DateTime)(Session["start_date"]));
        selectQuery += "and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("wma.create_date", "<", (DateTime)(Session["end_date"]));
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
    
    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
    }

    protected void GetSubMenu()
    {
        Response.Write(m_sSubMenu);
    }
}
