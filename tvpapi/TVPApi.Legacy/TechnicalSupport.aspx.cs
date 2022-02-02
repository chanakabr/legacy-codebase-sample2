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
using System.Collections.Generic;
using TVPPro.SiteManager.Manager;
using Tvinci.Helpers;
using KLogMonitor;
using System.Reflection;
using Phx.Lib.Log;

public partial class TechnicalSupport : System.Web.UI.Page
{
    private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

    protected void Page_Load(object sender, EventArgs e)
    {
        //string mode = System.Configuration.ConfigurationManager.AppSettings["Tvp.Demo.ExposeTechnicalAbilities"] as string;
        //if (string.IsNullOrEmpty(mode) || mode.ToLower() != "true")
        //{
        //    Response.Write("Permission denied (Tvp.Demo.ExposeTechnicalAbilities)");
        //    Response.End();
        //    return;
        //}

        string RequestorIP = GetRequestorIP();

        bool ClearCache = false;
        bool ClearCategories = false;
        bool ClearEPG = false;
        if (Request.QueryString["ClearCache"] != null)
            bool.TryParse(Request.QueryString["ClearCache"].ToString(), out ClearCache);
        if (Request.QueryString["ClearCategories"] != null)
            bool.TryParse(Request.QueryString["ClearCategories"].ToString(), out ClearCache);
        if (Request.QueryString["ClearEPG"] != null)
            bool.TryParse(Request.QueryString["ClearEPG"].ToString(), out ClearCache);

        if (RequestorIP.Contains("72.26.211") || RequestorIP.Equals("127.0.0.1") || ClearCache)
        {
            try
            {
                HttpRuntime.UnloadAppDomain();

                Response.Write("Site cache cleared");
            }
            catch (Exception ex)
            {
                logger.Error("", ex);
                Response.Write("failed to clear site cache");
            }
        }
        else if (ClearEPG)
        {
            var httpCache = HttpContext.Current.Cache;
            var toRemove = httpCache.Cast<DictionaryEntry>()
                .Select(de => (string)de.Key)
                .Where(key => key.Contains("EPG_Programs"))
                .ToArray();

            foreach (var keyToRemove in toRemove)
                httpCache.Remove(keyToRemove);

            Response.Write("EPG cache cleared");
        }
        else if (ClearCategories)
        {

            var httpCache = HttpContext.Current.Cache;
            var toRemove = httpCache.Cast<DictionaryEntry>()
                .Select(de => (string)de.Key)
                .Where(key => key.Contains("Category"))
                .ToArray();

            foreach (var keyToRemove in toRemove)
                httpCache.Remove(keyToRemove);

            Response.Write("Category cache cleared");
        }
        else
        {
            Response.Status = "404 Not Found";
            Response.StatusCode = 404;
            Response.Write("Permission denied");
        }
    }

    private string GetRequestorIP()
    {
        string ip = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
        string ClientIP = string.Empty;

        if (!string.IsNullOrEmpty(ip))
        {
            string[] ipRange = ip.Split(',');
            int le = ipRange.Length - 1;
            ClientIP = ipRange[le];
        }
        else
        {
            ClientIP = Request.ServerVariables["REMOTE_ADDR"];
        }

        return ClientIP;
    }
}
