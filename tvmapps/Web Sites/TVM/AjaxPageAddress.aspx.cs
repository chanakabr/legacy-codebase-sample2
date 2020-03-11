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

public partial class AjaxPageAddress : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        string sRet = "FAIL";

        string sPageID = "0";
        string sPlatform = "0";
        if (Request.Form["page_id"] != null)
            sPageID = Request.Form["page_id"].ToString();
        if (Request.Form["platform"] != null)
            sPlatform = Request.Form["platform"].ToString();
        Int32 nGroupID = TVinciShared.LoginManager.GetLoginGroupID();

        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery.SetConnectionKey("tvp_connection_" + nGroupID.ToString() + "_" + sPlatform);
        selectQuery += "select lpt.url from lu_page_types lpt,tvp_pages_structure tps where tps.PAGE_TYPE=lpt.id and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("tps.id" , "=" , int.Parse(sPageID.ToString()));
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
            {
                sRet = "OK" + "~~|~~";
                object oBaseURL = ODBCWrapper.Utils.GetTableSingleVal("groups", "GROUP_BASE_URL", nGroupID);
                if (oBaseURL != null && oBaseURL != DBNull.Value)
                    sRet += oBaseURL.ToString();
                if (sRet.EndsWith("/") == false && sRet.EndsWith("\\") == false)
                    sRet += "/";
                sRet += selectQuery.Table("query").DefaultView[0].Row["url"].ToString();
                if (sRet.IndexOf("?") == -1)
                    sRet += "?";
                else
                    sRet += "&";
                sRet += "PageID=" + sPageID;
            }

        }
        selectQuery.Finish();
        selectQuery = null;

        Response.CacheControl = "no-cache";
        Response.AddHeader("Pragma", "no-cache");
        Response.Expires = -1;
        Response.Clear();
        Response.Write(sRet + "~~|~~");
    }
}
