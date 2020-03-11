using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections;
using KLogMonitor;
using System.Reflection;

public partial class ClearCache : System.Web.UI.Page
{
    private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
            List<string> keys = new List<string>();

            foreach (DictionaryEntry entry in Context.Cache)
            {
                keys.Add(entry.Key.ToString());
            }

            foreach (string key in keys)
            {
                Context.Cache.Remove(key);
            }

            //TVPApi.SiteMapManager.GetInstance.Clear();
            //TVPApi.PageData.GetInstance.Clear();
            TVPApi.MenuBuilder.Clear();
            TVPApi.UsersXMLParser.Clear();
            if (Request.UrlReferrer != null && !string.IsNullOrEmpty(Request.UrlReferrer.ToString()))
            {
                Response.Redirect(Request.UrlReferrer.ToString());
            }
            else
            {
                Response.Write("Site cache cleared");
            }

            //Response.Write("Site cache cleared");            
        }
        catch (Exception ex)
        {
            logger.Error("", ex);
            Response.Write("failed to clear site cache");
        }
    }
}
