using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Ingest
{
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
            bool bAdmin = true;
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
                sRefferer.ToLower().IndexOf("tvinci.com") != -1 ||
            sHost.ToLower().IndexOf("173.231.146.34") != -1)
                bAdmin = true;
            if (Request.QueryString["action"] != null &&
                Request.QueryString["action"].ToString().ToLower().Trim() == "clear_all")
            {
                Response.Clear();
                Response.Write("Clear cache request from host: " + sHost + " , Refferer: " + sRefferer + "<br/>");
                if (bAdmin == true)
                {
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
                {
                    Response.StatusCode = 404;
                }
            }
            else if (Request.QueryString["key"] != null)
            {
                Response.Clear();
                if (bAdmin == true)
                {
                    try
                    {
                        string key = Request.QueryString["key"].ToString().Trim();
                        TvinciCache.WSCache cache = TvinciCache.WSCache.Instance;
                        if (cache != null)
                        {
                            object value;
                            bool isExists = cache.TryGet<object>(key, out value);
                            if (isExists)
                            {
                                Response.Write(string.Format("{0} value is: {1}", key, value));
                            }
                            else
                            {
                                Response.Write(string.Format("Key {0} not found", key));
                            }
                        }
                        else
                        {
                            Response.Write(string.Format("Key {0} not found", key));
                        }
                    }
                    catch (Exception ex)
                    {
                        Response.Write("Error : " + ex.Message);
                        Response.StatusCode = 404;
                    }
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
}