using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class AjaxTokenApprove : System.Web.UI.Page
{
    private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

    protected void Page_Load(object sender, EventArgs e)
    {
        string sRet = "FAIL";
        if (Request.Url.Host != "localhost" && Request.Url.Host != "127.0.0.1" && Request.Url.Scheme.ToUpper().Trim() != "HTTPS")
            sRet = "HTTPS_REQUIERED";
        else
        {
            log.Debug("AjaxTokenApprove page load");
            string sGuid = "";
            if (Request.Form["email"] != null)
                sGuid = Request.Form["email"].ToString();
            string sIpAddress = TVinciShared.PageUtils.GetCallerIP();
            Int32 nID = GetIPID(sIpAddress, sGuid);

            log.DebugFormat("AjaxTokenApprove sGuid={0}, sIpAddress={1}, nID={2}", sGuid, sIpAddress, nID);


            if (nID > 0)
            {
                ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("groups_ips");
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
                updateQuery += "where";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nID);
                updateQuery.Execute();
                updateQuery.Finish();
                updateQuery = null;
                sRet = "login.html";
            }
            else
                sRet = "WRONG_USERNAME_PASS";
        

            log.DebugFormat("AjaxTokenApprove sRet={0}", sRet);

        Response.CacheControl = "no-cache";
        Response.AddHeader("Pragma", "no-cache");
        Response.Expires = -1;
        Response.Clear();
        Response.Write(sRet + "~~|~~");
    }

    static public Int32 GetIPID(string sIP, string sGuid)
    {
        Int32 nID = 0;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select * from groups_ips where ADMIN_OPEN=1 and IS_ACTIVE=1 and end_date>getdate() and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("IP", "=", sIP);
        selectQuery += "and";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GUID", "=", sGuid);
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
        return nID;
    }
}
