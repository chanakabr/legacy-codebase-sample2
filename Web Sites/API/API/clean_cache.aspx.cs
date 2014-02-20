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

public partial class CleanCache : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        string sHost = "";
        string sRefferer = "";
        if (HttpContext.Current.Request.ServerVariables["REMOTE_HOST"] != null)
            sHost = HttpContext.Current.Request.ServerVariables["REMOTE_HOST"].ToLower();
        if (HttpContext.Current.Request.ServerVariables["HTTP_REFERER"] != null)
            sRefferer = HttpContext.Current.Request.ServerVariables["HTTP_REFERER"].ToLower();
        bool bAdmin = false;
        if (sHost.ToLower().IndexOf("admin.tvinci.com") != -1 ||
            sHost.ToLower().IndexOf("62.128.54.164") != -1 ||
            sHost.ToLower().IndexOf("62.128.54.165") != -1 ||
            sHost.ToLower().IndexOf("62.128.54.166") != -1 ||
            sHost.ToLower().IndexOf("62.128.54.167") != -1 ||
            sHost.ToLower().IndexOf("62.128.54.168") != -1 ||
            sHost.ToLower().IndexOf("80.179.194.132") != -1 ||
            sHost.ToLower().IndexOf("213.8.115.108") != -1 ||
            sHost.ToLower().StartsWith("72.26.211") == true ||
            sHost.ToLower().IndexOf("127.0.0.1") != -1 ||
            sRefferer.ToLower().IndexOf("tvinci.com") != -1)
            bAdmin = true;
        if (Request.QueryString["action"] != null &&
            Request.QueryString["action"].ToString().ToLower().Trim() == "clear_all")
        {
            Response.Clear();
            Response.Write("Clear cache request from host: " + sHost + " , Refferer: " + sRefferer + "<br/>");
            if (bAdmin == true)
            {
                CachingManager.CachingManager.RemoveFromCache("");
                Response.Write("Cache cleared");
            }
            else
            {
                Response.StatusCode = 404;
            }
        }
        else
            Response.StatusCode = 404;
    }
}
