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

public partial class clear_cache : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        string sHost = "";
        string sRefferer = "";
        if (HttpContext.Current.Request.ServerVariables["REMOTE_HOST"] != null)
            sHost = HttpContext.Current.Request.ServerVariables["REMOTE_HOST"].ToLower();
        if (HttpContext.Current.Request.ServerVariables["HTTP_REFERER"] != null)
            sRefferer = HttpContext.Current.Request.ServerVariables["HTTP_REFERER"].ToLower();

        if (Request.QueryString["action"] != null &&
            Request.QueryString["action"].ToString().ToLower().Trim() == "clear_all")
        {
            Response.Clear();
            Response.Write("Clear cache request from host: " + sHost + " , Refferer: " + sRefferer + "<br/>");

            try
            {
                CachingManager.CachingManager.RemoveFromCache("");
                TvinciCache.WSCache.ClearAll();
                Response.Write("Cache cleared");
            }
            catch (Exception ex)
            {
                Response.Write("Error : " + ex.Message);
                Response.StatusCode = 404;
            }

        }
        else
            Response.StatusCode = 404;
    }
}
