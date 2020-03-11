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

public partial class adm_statistics_players : System.Web.UI.Page
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
            if (Request.QueryString["media_id"] != null &&
                Request.QueryString["media_id"].ToString() != "")
            {
                m_sMenu = TVinciShared.Menu.GetMainMenu(7, true, ref nMenuID);
                m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 4, false);
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
                m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 3, false);
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
                m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 3, false);
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
                m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 4, false);
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
        TVinciShared.Channel c = new TVinciShared.Channel(int.Parse(Session["channel_id"].ToString()), false , 0 , true , 0 , 0);
        string s = c.GetChannelMediaIDs();
        if (s == "")
            s = "0";
        HttpContext.Current.Cache["GetMediaIDsForChannel" + Session["channel_id"].ToString()] = s;
        return s;
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

    public void GetPlayersSummery()
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

    public string GetMediaPlayersStatisticsStr(Int32 nDays)
    {
        string sRet = "var PP" + nDays.ToString() + " = new Array();\r\n";
        Int32 nTotal = 0;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        string sGroups = PageUtils.GetParentsGroupsStr(LoginManager.GetLoginGroupID());
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

    protected void GetPlayersSummeryForPeriod(bool bUnique, Int32 nDaysAgo, ref System.Collections.Specialized.NameValueCollection thePlayers)
    {
        //Hashtable theContries = new Hashtable();

        Int32 nTotal = 0;
        bool bAnd = false;
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
                selectQuery += "DATEADD(m, -1, CONVERT(datetime, LEFT(CONVERT(char(10), getdate(), 20) , 10))) ";
            else
                selectQuery += "DATEADD(d, " + (nDaysAgo * -1).ToString() + " , CONVERT(datetime, LEFT(CONVERT(char(10), getdate(), 20) , 10))) ";
            bAnd = true;
        }
        if (Session["media_id"] != null)
        {
            if (bAnd == true)
                selectQuery += "and";
            bAnd = true;
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("wmpc.media_id", "=", int.Parse(Session["media_id"].ToString()));
        }
        else if (Session["channel_id"] != null)
        {
            if (bAnd == true)
                selectQuery += "and";
            bAnd = true;
            selectQuery += " wmpc.media_id in (" + GetMediaIDsForChannel() + ")";
        }
        else if (Session["player_id"] != null)
        {
            if (bAnd == true)
                selectQuery += "and";
            bAnd = true;
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("wmpc.PLAYER_ID", "=", int.Parse(Session["player_id"].ToString()));
        }
        if (bAnd == true)
            selectQuery += "and";
        bAnd = true;
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("wmpc.group_id", "=", LoginManager.GetLoginGroupID());
        selectQuery += " and ";
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

    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
    }

    protected void GetSubMenu()
    {
        Response.Write(m_sSubMenu);
    }
}
