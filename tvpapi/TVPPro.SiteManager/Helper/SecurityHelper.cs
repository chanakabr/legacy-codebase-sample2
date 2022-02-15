using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tvinci.Helpers;
using System.Web;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using Phx.Lib.Log;
using System.Reflection;
using TVinciShared;

namespace TVPPro.SiteManager.Helper
{
    public class SecurityHelper
    {
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public static void SetSecurityParams(string SessionId, string Ip)
        {
            //While the user has a SessionId we have to set the uniueid & his ip in order to check Cross-Site Request Forgery
            string UniqeId = Guid.NewGuid().ToString();

            SessionHelper.SessionId = SessionId;
            SessionHelper.UniqueId = UniqeId;
            SessionHelper.RequestIp = Ip;

            //if (string.IsNullOrEmpty(SessionHelper.SessionId) || string.IsNullOrEmpty(SessionHelper.RequestIp))
            //{
            //    HttpContext.Current.Session.Clear();
            //    HttpContext.Current.Session.Abandon();

            //    HttpContext.Current.Response.Redirect(LinkHelper.ParseURL("~/index.aspx"));
            //}
        }


        public static void CheckSessionValidity(out bool retVal)
        {
            //Check the user has a valid session id (the same as he got on Sessin_start) an this ip hasnt change, this function
            //should be used on web services

            string UserIp = TVPPro.SiteManager.Helper.SiteHelper.GetClientIP();
            string SessionId = HttpContext.Current.Session.Get("SessionId").ToString();
            retVal = true;

            //if (SessionHelper.SessionId != SessionId || SessionHelper.RequestIp != UserIp)
            //{
            //    logger.ErrorFormat("Invalid Request, SessionId or UserIp are not eqal on session: SessionId - {0}, RequestIp-{1}, On request: SessionId - {2}, RequestIp - {3}",
            //        SessionHelper.SessionId, SessionHelper.RequestIp, SessionId, UserIp);

            //    HttpContext.Current.Session.Clear();
            //    HttpContext.Current.Session.Abandon();

            //    retVal = false;
            //}
        }

        public static void CheckSessionValidity()
        {
            ////Check the user has a valid session id (the same as he got on Sessin_start) an this ip hasnt change, otherwise he will 
            ////redirect to homepage

            //string UserIp = TVPPro.SiteManager.Helper.SiteHelper.GetClientIP();
            //string SessionId = HttpContext.Current.Session["SessionId"].ToString();

            //if (SessionHelper.SessionId != SessionId || SessionHelper.RequestIp != UserIp)
            //{
            //    logger.ErrorFormat("Invalid Request, SessionId or UserIp are not eqal on session: SessionId - {0}, RequestIp-{1}, On request: SessionId - {2}, RequestIp - {3}",
            //        SessionHelper.SessionId, SessionHelper.RequestIp, SessionId, UserIp);

            //    HttpContext.Current.Session.Clear();
            //    HttpContext.Current.Session.Abandon();

            //    HttpContext.Current.Response.Redirect(LinkHelper.ParseURL("~/index.aspx"));
            //}
        }

        public static bool IsEmail(string InputEmail)
        {
            string strRegex = @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}" +
                  @"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\" +
                  @".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";
            Regex reg = new Regex(strRegex);
            if (reg.IsMatch(InputEmail))
                return true;
            else
                return false;
        }

        public static bool IsIncludeForbiddenChars(string InputUserName, char[] ForbiddenChars)
        {
            bool retVal = false;

            foreach (char item in ForbiddenChars)
            {
                if (InputUserName.Contains(item))
                {
                    retVal = true;
                    break;
                }
            }

            return retVal;
        }

        public static bool IsIncludeForbiddenChars(string[] Inputs, char[] ForbiddenChars)
        {
            bool retVal = false;

            foreach (string input in Inputs)
            {
                string DecodeInput = HttpUtility.UrlDecode(input);
                foreach (char item in ForbiddenChars)
                {
                    if (DecodeInput.Contains(item))
                    {
                        retVal = true;
                        break;
                    }
                }
                if (retVal)
                {
                    break;
                }

            }

            return retVal;
        }

        public static string EncryptSiteGuid(string key, string IV, string siteGuid)
        {
            string encrtyped = string.Empty;

            AesManaged aes = new AesManaged();
            aes.Key = Convert.FromBase64String(key);
            aes.IV = Convert.FromBase64String(IV);

            return Convert.ToBase64String(DataHelper.encryptStringToBytes_AES(siteGuid, aes.Key, aes.IV));
        }

        public static string DecryptSiteGuid(string key, string IV, string siteGuid)
        {
            string encrtyped = string.Empty;

            AesManaged aes = new AesManaged();
            aes.Key = Convert.FromBase64String(key);
            aes.IV = Convert.FromBase64String(IV);

            return DataHelper.decryptStringFromBytes_AES(Convert.FromBase64String(siteGuid), aes.Key, aes.IV);
        }

        //
        public static string EncryptData(string key, string IV, string data)
        {
            string encrtyped = string.Empty;

            AesManaged aes = new AesManaged();
            aes.Key = Convert.FromBase64String(key);
            aes.IV = Convert.FromBase64String(IV);

            return Convert.ToBase64String(DataHelper.encryptStringToBytes_AES(data, aes.Key, aes.IV));
        }

        public static string DecryptData(string key, string IV, string data)
        {
            string encrtyped = string.Empty;

            AesManaged aes = new AesManaged();
            aes.Key = Convert.FromBase64String(key);
            aes.IV = Convert.FromBase64String(IV);

            return DataHelper.decryptStringFromBytes_AES(Convert.FromBase64String(data), aes.Key, aes.IV);
        }

    }

}
