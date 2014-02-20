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

public partial class whatismyip : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        GetIP();
    }

    public void GetIP()
    {
        string sOnlyIP = "0";
        if (Request.QueryString["onlyip"] != null)
            sOnlyIP = "1";
        string sIP = TVinciShared.PageUtils.GetCallerIP();
        if (sOnlyIP == "0")
        {

            string sBrowser = HttpContext.Current.Request.Browser.Type;
            string sPlatform = HttpContext.Current.Request.Browser.Platform;
            Response.Write("Browser=" + sBrowser + "<br/>");
            Response.Write("Platform=" + sPlatform + "<br/>");
            Response.Write("IP=" + sIP + "<br/>");
            Int32 nCountryID = TVinciShared.PageUtils.GetIPCountry2(sIP);
            object oCountry = ODBCWrapper.Utils.GetTableSingleVal("countries", "COUNTRY_NAME", nCountryID);
            string sCountry = "unknown";
            if (oCountry != null && oCountry != DBNull.Value)
                sCountry = oCountry.ToString();
            Response.Write("Country=" + sCountry + " (" + nCountryID.ToString() + ")<br/>");
        }
        else
        {
            Response.Clear();
            Response.ClearContent();
            Response.Write(sIP);
        }
    }
}
