using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;

#if NET452
using System.Web;
using System.Web.SessionState;
#endif

#if NETSTANDARD2_0
using Microsoft.AspNetCore.Http;
#endif

namespace TVinciShared
{
    public static class HttpContextUtils
    {
#if NET452
        public static object Get(this HttpSessionState session, string key)
        {
            return session[key];
        }

        public static void Set(this HttpSessionState session, string key, object value)
        {
            session[key] = value;
        }

        public static string GetSessionID(this HttpSessionState session)
        {
            return session.SessionID;
        }

        public static void Abandon(this HttpSessionState session)
        {
            session.Abandon();
        }

        public static Uri GetUrl(this HttpRequest req)
        {
            return req.Url;
        }

        public static string GetUserAgentString(this HttpRequest req)
        {
            return req.ServerVariables["HTTP_USER_AGENT"];
        }

        public static string GetHttpReferer(this HttpRequest req)
        {
            return req.ServerVariables["HTTP_REFERER"];
        }

        public static HttpBrowserCapabilities GetBrowser(this HttpRequest req)
        {
            return req.Browser;
        }

        public static NameValueCollection GetQueryString(this HttpRequest req)
        {
            return req.QueryString;
        }

        public static NameValueCollection GetForm(this HttpRequest req)
        {
            return req.Form;
        }

        public static NameValueCollection GetHeaders(this HttpRequest req)
        {
            return req.Headers;
        }

        public static string GetForwardedForHeader(this HttpRequest req)
        {
            return req.ServerVariables["HTTP_X_FORWARDED_FOR"];
        }

        public static string GetRemoteAddress(this HttpRequest req)
        {
            return req.ServerVariables["REMOTE_ADDR"];
        }

        public static string GetRemoteHost(this HttpRequest req)
        {
            return req.ServerVariables["REMOTE_HOST"];
        }


        public static HttpFileCollection GetFiles(this HttpRequest req)
        {
            return req.Files;
        }

        public static string GetFilePath(this HttpRequest req)
        {
            return req.FilePath;
        }

        public static string ServerMapPath(this HttpContext ctx, string path)
        {
            return ctx.Server.MapPath(path);
        }

#endif

#if NETSTANDARD2_0
        public static object Get(this ISession session, string key)
        {
            session.TryGetValue(key, out var value);

            return value;
        }

        public static void Set(this ISession session, string key, object value)
        {
            session.Set(key, value);
        }

        public static string GetSessionID(this ISession session)
        {
            return session.Id;
        }

        public static void RemoveAll(this ISession session)
        {
            foreach (var key in session.Keys)
            {
                session.Remove(key);
            }
        }

        public static void Abandon(this ISession session)
        {
            session.Clear();
        }


        public static void Write(this HttpResponse rsp, string content)
        {
            var byteContent = Encoding.UTF8.GetBytes(content);
            rsp.Body.Write(byteContent, 0, byteContent.Length);
        }

        public static Uri GetUrl(this HttpRequest req)
        {
            return new Uri($"{req.Scheme}://{req.Host}{req.Path}{req.QueryString}");
        }

        public static string GetUserAgentString(this HttpRequest req)
        {
            return req.Headers["User-Agent"].ToString();
        }

        public static string GetHttpReferer(this HttpRequest req)
        {
            return req.Headers["Referer"].ToString();
        }

        public static string GetForwardedForHeader(this HttpRequest req)
        {
            return req.Headers["x-forwarded-for"];
        }

        public static string GetRemoteAddress(this HttpRequest req)
        {
            return req.HttpContext.Connection.RemoteIpAddress.ToString();
        }

        public static string GetRemoteHost(this HttpRequest req)
        {
            return req.Host.Value;
        }

        public static NameValueCollection GetQueryString(this HttpRequest req)
        {
            var queryVals = req.Query.Select(f => new KeyValuePair<string, string>(f.Key, f.Value.ToString()));
            var retVal = new NameValueCollection();
            foreach (var qVal in queryVals)
            {
                retVal.Add(qVal.Key, qVal.Value);
            }
            return retVal;
        }

        public static NameValueCollection GetForm(this HttpRequest req)
        {
            var formVals = req.Form.Select(f => new KeyValuePair<string, string>(f.Key, f.Value.ToString()));
            var retVal = new NameValueCollection();
            foreach (var formVal in formVals)
            {
                retVal.Add(formVal.Key, formVal.Value);
            }
            return retVal;
        }

        public static NameValueCollection GetHeaders(this HttpRequest req)
        {
            var headerVals = req.Headers.Select(f => new KeyValuePair<string, string>(f.Key, f.Value.ToString()));
            var retVal = new NameValueCollection();
            foreach (var formVal in headerVals)
            {
                retVal.Add(formVal.Key, formVal.Value);
            }
            return retVal;
        }

        public static IFormFileCollection GetFiles(this HttpRequest req)
        {
            return req.Form.Files;
        }

        public static void SaveAs(this IFormFile file, string path)
        {
            using (var fileStream = new FileStream(path, FileMode.Create))
            {
                file.CopyTo(fileStream);
            }
        }

        /// <summary>
        /// Note! this is a shim for netstandard. this should not be used! always will return null
        /// </summary>
        public static dynamic GetBrowser(this HttpRequest req)
        {
            return null;
        }
        
        /// <summary>
        /// Note! this is a shim for netstandard. this should not be used! always will return empty string
        /// </summary>
        public static string GetFilePath(this HttpRequest req)
        {
            // TODO: find out what to return here in netstandard2.0
            return "";
        }

        /// <summary>
        /// Note! this is a shim for netstandard. this should not be used! always will return empty string
        /// </summary>
        public static string ServerMapPath(this HttpContext ctx, string path)
        {
            return Path.Combine(System.Web.HostingEnvironment.Current.WebRootPath, path);
        }

#endif
    }
}
