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
using System.Threading;

public partial class mtv_notifier : System.Web.UI.Page
{
    static protected Random RandomClass = new Random();
    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
            string sGroup = "";
            if (Request.QueryString["group_un"] != null)
                sGroup = Request.QueryString["group_un"].ToString();

            Int32 nMediaID = 0;
            if (Request.QueryString["media_id"] != null &&
                Request.QueryString["media_id"].ToString() != "")
            {
                try
                {
                    nMediaID = int.Parse(Request.QueryString["media_id"].ToString());
                    OuterVideoReport(nMediaID, sGroup);
                }
                catch (Exception ex)
                {
                    Logger.Logger.Log("exception", ex.Message, "mtv_notifier");
                }
            }
            Response.ClearHeaders();
            Response.Clear();
            Response.Expires = -1;
        }
        catch (Exception ex)
        {
            Logger.Logger.Log("exception", ex.Message, "mtv_notifier");
        }
    }

    protected string GetGroupCodeByUN(string sGroupUN)
    {
        string sCode = "";
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select g.GROUP_NOTIFY_CODE from groups g,groups_passwords gp where g.id=gp.GROUP_ID and gp.IS_ACTIVE=1 and gp.STATUS=1 and g.IS_ACTIVE=1 and g.STATUS=1 and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("gp.USERNAME" , "=" , sGroupUN);
        if (selectQuery.Execute("query" , true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
            {
                object oCode = selectQuery.Table("query").DefaultView[0].Row["GROUP_NOTIFY_CODE"];
                if (oCode != DBNull.Value && oCode != null)
                {
                    sCode = oCode.ToString();
                }
            }
        }
        selectQuery.Finish();
        selectQuery = null;
        return sCode;
    }

    protected string GetReportStringByMediaID(Int32 nMediaID)
    {
        string sCode = "";
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select META10_STR from media where ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id" , "=" , nMediaID);
        if (selectQuery.Execute("query" , true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
            {
                object oCode = selectQuery.Table("query").DefaultView[0].Row["META10_STR"];
                if (oCode != DBNull.Value && oCode != null)
                {
                    sCode = oCode.ToString();
                }
            }
        }
        selectQuery.Finish();
        selectQuery = null;
        return sCode;
    }

    protected void OuterVideoReport(Int32 nMediaID , string sGroupUN)
    {
        string sAkamaiReportURL = "http://viamtvnvideo.112.2o7.net/b/ss/viamtvnvideo/1/G.5--NS/";
        string sPageName = "pageName=" + GetGroupCodeByUN(sGroupUN);
        if (sPageName == "")
            return;
        string sReportString = GetReportStringByMediaID(nMediaID);
        string sURL = sAkamaiReportURL + RandomClass.Next(1000000, 9999999).ToString() + "?" + sPageName.ToString() + "&" + sReportString;
        Int32 nStatus = 0;
        Notifier.SendGetHttpReq(sURL, ref nStatus);
        Logger.Logger.Log("Notification", "Sent to " + sURL + " and got back status: " + nStatus.ToString(), "mtv_new_notifier");
    }
}