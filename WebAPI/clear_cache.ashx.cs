using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebAPI.ClientManagers.Client;

namespace WebAPI
{
    /// <summary>
    /// Summary description for clear_cache
    /// </summary>
    public class clear_cache : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";            
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
                if (context.Request.QueryString["action"] != null)
                {
                    context.Response.Clear();
                    context.Response.Write("Clear cache request from host: " + sHost + " , Refferer: " + sRefferer + "<br/>");
                    if (context.Request.QueryString["action"].ToString().ToLower().Trim() == "clear_all")
                    {
                        if (bAdmin == true)
                        {
                            try
                            {
                                CachingManager.CachingManager.RemoveFromCache("");
                                TvinciCache.WSCache.ClearAll();
                                //bool layeredCacheCleared = false;
                                //bool addGroupIdInResponse = false;
                                //int groupId;
                                //if (context.Request.QueryString["groupId"] != null)
                                //{                                    
                                //    if (int.TryParse(context.Request.QueryString["groupId"].ToString().Trim(), out groupId))
                                //    {
                                //        addGroupIdInResponse = true;
                                //        WS_API.API api = new WS_API.API();
                                //        layeredCacheCleared = ClientsManager.ApiClient().ClearLayeredCache(groupId);
                                //    }
                                //}
                                //else
                                //{
                                //    layeredCacheCleared = true;
                                //}
                                //string addGroupIdResult = addGroupIdInResponse ? string.Format("for groupId: {0}", groupId) : string.Empty;
                                //string clearCacheResult = layeredCacheCleared ? string.Format("Cleared all cache {0}", addGroupIdResult) : string.Format("Cache cleared but LayeredCache failed {0}", addGroupIdResult);
                                //context.Response.Write(clearCacheResult);
                                context.Response.Write("Cache cleared");
                            }
                            catch (Exception ex)
                            {
                                context.Response.Write("Error : " + ex.Message);
                                context.Response.StatusCode = 404;
                            }
                        }
                        else
                        {
                            context.Response.StatusCode = 404;
                        }
                    }
                    if (context.Request.QueryString["action"].ToString().ToLower().Trim() == "keys")
                    {
                        System.Collections.Generic.List<string> keys = CachingManager.CachingManager.GetCachedKeys();
                        TvinciCache.WSCache cache = TvinciCache.WSCache.Instance;
                        if (cache != null)
                        {
                            keys.AddRange(cache.GetKeys());
                        }
                        context.Response.Write(string.Join(",", keys));
                    }
                }
                else if (context.Request.QueryString["key"] != null)
                {
                    context.Response.Clear();
                    if (bAdmin == true)
                    {
                        try
                        {
                            string key = context.Request.QueryString["key"].ToString().Trim();
                            object o = CachingManager.CachingManager.GetCacheObject(key);
                            if (o != null)
                            {
                                context.Response.Write(string.Format("{0} value is: {1}", key, o.ToString()));
                            }
                            else
                            {
                                TvinciCache.WSCache cache = TvinciCache.WSCache.Instance;
                                if (cache != null)
                                {
                                    object value;
                                    bool isExists = cache.TryGet<object>(key, out value);
                                    if (isExists)
                                    {
                                        context.Response.Write(string.Format("{0} value is: {1}", key, value));
                                    }
                                    else
                                    {
                                        context.Response.Write(string.Format("Key {0} not found", key));
                                    }
                                }
                                else
                                {
                                    context.Response.Write(string.Format("Key {0} not found", key));
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            context.Response.Write("Error : " + ex.Message);
                            context.Response.StatusCode = 404;
                        }
                    }
                    else
                    {
                        context.Response.StatusCode = 404;
                    }
                }
                else
                    context.Response.StatusCode = 404;
            }
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

    }
}