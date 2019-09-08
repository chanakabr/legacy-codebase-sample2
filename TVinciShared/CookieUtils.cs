using System;
using System.Data;
using System.Configuration;
using System.Web;

using KLogMonitor;
using System.Reflection;

#if NETFRAMEWORK
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
#endif

#if NETSTANDARD2_0
using Microsoft.AspNetCore.Http;
#endif

namespace TVinciShared
{
    /// <summary>
    /// Summary description for CookieUtils
    /// </summary>
    public class CookieUtils
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public CookieUtils()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        public static string GetSubDomain(Uri url)
        {
            if (url.HostNameType == UriHostNameType.Dns)
            {
                string host = url.Host;
                if (host.Split('.').Length > 2)
                {
                    int index = host.IndexOf(".");
                    return host.Substring(0, index);
                }
            }
            return "";
        }

        public static string GetBaseDomain(Uri url)
        {
            if (url.HostNameType == UriHostNameType.Dns)
            {
                string host = url.Host;
                if (host.Split('.').Length > 2)
                {
                    int index = host.IndexOf(".");
                    return host.Substring(index);
                }
                else
                    return host;
            }
            return "";
        }

        public static void ShareSession()
        {
            //string sBaseDomain = GetBaseDomain(HttpContext.Current.Request.Url);
            //string sSubDomail = GetSubDomain(HttpContext.Current.Request.Url);
            //string sDomain = "";
            //if (sBaseDomain.ToLower().Trim() == ".co.il" ||
            //sBaseDomain.ToLower().Trim() == ".com" ||
            //sBaseDomain.ToLower().Trim() == ".net.il" ||
            //sBaseDomain.ToLower().Trim() == ".net" ||
            //sBaseDomain.ToLower().Trim() == ".org")
            //sDomain = "." + sSubDomail + sBaseDomain;
            //else
            //{
            //if (sDomain.StartsWith(".") == false)
            //sDomain = "." + sBaseDomain;
            //else
            //sDomain = sBaseDomain;
            //}
            //HttpContext.Current.Response.Cookies["ASP.NET_SessionId"].Value = HttpContext.Current.Session.SessionID;
            //HttpContext.Current.Response.Cookies["ASP.NET_SessionId"].Domain = sDomain;
        }

#if NETFRAMEWORK
        public static bool SetCookie(string cookiename, string cookievalue, int iMinToExpire)
        {
            try
            {
                HttpCookie objCookie = new HttpCookie(cookiename, cookievalue);
                DateTime dtExpiry = DateTime.UtcNow.AddDays(iMinToExpire);
                objCookie.Expires = dtExpiry;
                objCookie.Domain = ".tvinci.com";
                HttpContext.Current.Response.Cookies.Add(objCookie);
                //Response.Cookies("UID").Domain = ".myserver.com"

            }
            catch (Exception e)
            {
                string s = e.Message;
                log.Error("exception - " + s, e);

                return false;
            }
            return true;
        }


        public static string GetCookie(string cookiename)
        {
            string cookyval = "";

            try
            {
                if (HttpContext.Current.Request.Cookies[cookiename] == null)
                {
                    return cookyval;
                }

                cookyval = HttpContext.Current.Request.Cookies[cookiename].Value;
                string sCN = cookyval.ToLower();
                if (sCN.IndexOf(cookiename.ToLower() + "=") == 0)
                {
                    cookyval = cookyval.Substring(cookiename.Length + 1);
                }
            }
            catch (Exception e)
            {
                string s = e.Message;
                cookyval = "";
            }

            return cookyval;
        }
#endif



#if NETSTANDARD2_0
        public static bool SetCookie(string cookiename, string cookievalue, int iMinToExpire)
        {
            try
            {
                var option = new CookieOptions();
                option.Expires = DateTime.Now.AddMinutes(iMinToExpire);
                option.Domain = ".tvinci.com";
                System.Web.HttpContext.Current.Response.Cookies.Append(cookiename, cookievalue, option);
            }
            catch (Exception e)
            {
                string s = e.Message;
                log.Error("exception - " + s, e);
                return false;
            }
            return true;
        }

        public static string GetCookie(string cookiename)
        {
            string cookyval = "";

            try
            {
                if (System.Web.HttpContext.Current.Request.Cookies[cookiename] == null)
                {
                    return cookyval;
                }

                cookyval = System.Web.HttpContext.Current.Request.Cookies[cookiename];
                string sCN = cookyval.ToLower();
                if (sCN.IndexOf(cookiename.ToLower() + "=") == 0)
                {
                    cookyval = cookyval.Substring(cookiename.Length + 1);
                }
            }
            catch (Exception e)
            {
                string s = e.Message;
                cookyval = "";
            }

            return cookyval;
        }
#endif
    }
}