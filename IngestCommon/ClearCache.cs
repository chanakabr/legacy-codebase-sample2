using KLogMonitor;
using System;
using System.Reflection;
using System.Web;
using TVinciShared;

namespace IngestCommon
{
    public class ClearCache
    {
        private static readonly KLogger _Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public static void Clear()
        {
            var Request = HttpContext.Current.Request;
            string sHost = "";
            string sRefferer = "";
            if (HttpContext.Current.Request.GetRemoteHost() != null)
                sHost = HttpContext.Current.Request.GetRemoteHost().ToLower();
            if (HttpContext.Current.Request.GetHttpReferer() != null)
                sRefferer = HttpContext.Current.Request.GetHttpReferer().ToLower();
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
            if (Request.GetQueryString()["action"] != null &&
                Request.GetQueryString()["action"].ToString().ToLower().Trim() == "clear_all")
            {
                //Response.Clear();
                _Logger.Info("Clear cache request from host: " + sHost + " , Refferer: " + sRefferer + "<br/>");
                if (bAdmin == true)
                {
                    try
                    {
                        CachingManager.CachingManager.RemoveFromCache("");
                        TvinciCache.WSCache.ClearAll();
                        _Logger.Info("Cache cleared");
                    }
                    catch (Exception ex)
                    {
                        _Logger.Error("Error while clearing cache : ", ex);
                        //Response.StatusCode = 404;
                    }
                }
                else
                {
                    _Logger.Error("Not Found");
                    //Response.StatusCode = 404;
                }
            }
            else if (Request.GetQueryString()["key"] != null)
            {
                //Response.Clear();
                if (bAdmin == true)
                {
                    try
                    {
                        string key = Request.GetQueryString()["key"].ToString().Trim();
                        TvinciCache.WSCache cache = TvinciCache.WSCache.Instance;
                        if (cache != null)
                        {
                            object value;
                            bool isExists = cache.TryGet<object>(key, out value);
                            if (isExists)
                            {
                                _Logger.Info(string.Format("{0} value is: {1}", key, value));
                            }
                            else
                            {
                                _Logger.Info(string.Format("Key {0} not found", key));
                            }
                        }
                        else
                        {
                            _Logger.Info(string.Format("Key {0} not found", key));
                        }
                    }
                    catch (Exception ex)
                    {
                        _Logger.Error("Error while clear cache", ex);
                        //Response.StatusCode = 404;
                    }
                }
                else
                {
                    _Logger.Error("Not Found");
                    //Response.StatusCode = 404;
                }
            }
            else
            {
                _Logger.Error("Not Found");
                //Response.StatusCode = 404;
            }
        }
    }
}
