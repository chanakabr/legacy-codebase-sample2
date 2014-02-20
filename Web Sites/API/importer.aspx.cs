using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TVinciShared;

public partial class importer : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        Int32 nGroupID = 0;
        string sCallerIP = PageUtils.GetCallerIP();
        
        if (Request.Form["group_id"] != null)
        {
            Int32 nFormGroupID = int.Parse(Request.Form["group_id"].ToString());
            bool bIPOK = IsIpValid(ref nGroupID , nFormGroupID);
            if (bIPOK == false || (Request.IsSecureConnection == false && sCallerIP != "127.0.0.1"))
            {
                Response.StatusCode = 404;
                Response.End();
                return;
            }
            try
            {
                string sImportURL = Request.Form["xml_url"].ToString();
                string sNotifyURL = Request.Form["notify_url"].ToString();


                ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("importer_alerts");
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("IMPORT_URL", "=", sImportURL);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("NOTIFY_URL", "=", sNotifyURL);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("RUN_STATUS", "=", 0);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATER_ID", "=", 43);
                insertQuery.Execute();
                insertQuery.Finish();
                insertQuery = null;

                Response.Write("OK");
            }
            catch
            {
                Response.StatusCode = 500;
                Response.End();
            }
        }
        else
        {
            Response.StatusCode = 404;
            Response.End();
            return;
        }
    }

    static protected bool IsIpValid(ref Int32 nGroupID , Int32 nFormGroupID)
    {
        bool bOK = false;
        string sCallerIP = PageUtils.GetCallerIP();
        if (sCallerIP == "127.0.0.1")
        {
            nGroupID = 1;
            return true;
        }
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select group_id from groups_ips where IMPORTER_OPEN=1 and is_active=1 and (end_date is null or end_date>getdate()) and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("LTRIM(RTRIM(IP))", "=", sCallerIP);
        selectQuery += " and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nFormGroupID);
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount == 1)
            {
                bOK = true;
                nGroupID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["group_id"].ToString());
            }
        }
        selectQuery.Finish();
        selectQuery = null;
        return bOK;
    }
}
