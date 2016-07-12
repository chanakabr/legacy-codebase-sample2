using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class AjaxToken : System.Web.UI.Page
{
    private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
    protected void Page_Load(object sender, EventArgs e)
    {
        string sRet = "FAIL";
        if (Request.Url.Host != "localhost" && Request.Url.Host != "127.0.0.1" && Request.Url.Scheme.ToUpper().Trim() != "HTTPS")
            sRet = "HTTPS_REQUIERED";
        else
        {
            Int32 nAccountID = 0;
            string sEmail = "";
            string sEmailAdd = "";
            Int32 nGroupID = 0;
            if (Request.Form["email"] != null)
                sEmail = Request.Form["email"].ToString();
            log.DebugFormat("AjaxToken pageLoad sEmail = {0}", sEmail.Trim().ToLower());
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select id,email_add,group_id from accounts where status=1 and is_active=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("LOWER(LTRIM(RTRIM(USERNAME)))", "=", sEmail.Trim().ToLower());
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    nAccountID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
                    nGroupID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["GROUP_ID"].ToString());
                    if (selectQuery.Table("query").DefaultView[0].Row["EMAIL_ADD"] != DBNull.Value &&
                        selectQuery.Table("query").DefaultView[0].Row["EMAIL_ADD"] != null)
                        sEmailAdd = selectQuery.Table("query").DefaultView[0].Row["EMAIL_ADD"].ToString();
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            log.DebugFormat("AjaxToken pageLoad sEmailAdd = {0}, nAccountID = {1}, nGroupID = {2}", sEmailAdd, nAccountID, nGroupID);
            if (nAccountID == 0)
                sRet = "WRONG_USERNAME_PASS";
            else
            {
                string sGuid = System.Guid.NewGuid().ToString();
                string sIpAddress = TVinciShared.PageUtils.GetCallerIP();
                GetIPID(sIpAddress, nGroupID , sGuid);
                log.DebugFormat("AjaxToken pageLoad sIpAddress = {0}, nGroupID = {1}, sGuid = {2}", sIpAddress, nGroupID, sGuid);
                string sMail = "<h1>IP Token </h1><br/>Enter the following token to the <a href='https://tvp.tvinci.com/token_enter.html'>TVM token approval page</a><br/><b>Token datails</b><br/>IP: " + sIpAddress + "<br/>Duration: 12 hours <br/>GUID: " + sGuid;
                TVinciShared.Mailer mailer = new TVinciShared.Mailer(1);
                log.DebugFormat("call mailer.SendMail sMail={0}", sMail);
                mailer.SendMail(sEmailAdd, "", sMail, "Token from Tvinci");
                sRet = "token_enter.html";
                log.DebugFormat("sRet = {0} ", sRet);
            }
        }

        Response.CacheControl = "no-cache";
        Response.AddHeader("Pragma", "no-cache");
        Response.Expires = -1;
        Response.Clear();
        Response.Write(sRet + "~~|~~");
    }

    static public void GetIPID(string sIP , Int32 nGroupID , string sGuid)
    {
        Int32 nID = 0;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select * from groups_ips where IS_ACTIVE=1 and end_date is not null and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("IP", "=", sIP);
        selectQuery += "and";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
            {
                nID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
            }
        }
        selectQuery.Finish();
        selectQuery = null;

        if (nID != 0)
        {
            ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("groups_ips");
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("END_DATE", "=", DateTime.UtcNow.AddHours(12));
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("GUID", "=", sGuid);
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 0);
            updateQuery += " where END_DATE is not null  and ";
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nID);
            updateQuery.Execute();
            updateQuery.Finish();
            updateQuery = null;
        }
        else
        {
            ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("groups_ips");
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID" , "=" , nGroupID);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("IP" , "=" , sIP);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE" , "=" , 1);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 0);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("END_DATE" , "=" , DateTime.UtcNow.AddHours(12));
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GUID", "=", sGuid);
            insertQuery.Execute();
            insertQuery.Finish();
            insertQuery = null;
        }
    }
}
