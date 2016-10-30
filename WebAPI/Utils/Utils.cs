using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebAPI.ClientManagers;
using WebAPI.Filters;
using WebAPI.Managers.Models;

namespace WebAPI.Utils
{
    public class Utils
    {
        internal static int GetLanguageId(int groupId, string language)
        {
            // get all group languages
            var languages = GroupsManager.GetGroup(groupId).Languages;

            // get default/specific language
            Language langModel = new Language();
            if (string.IsNullOrEmpty(language))
                langModel = languages.Where(l => l.IsDefault).FirstOrDefault();
            else
                langModel = languages.Where(l => l.Code == language).FirstOrDefault();

            if (langModel != null)
                return langModel.Id;
            else
                return 0;
        }

        public static string GetClientIP()
        {
            string ip = string.Empty;
            string retIp = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            string[] ipRange;
            if (!string.IsNullOrEmpty(retIp) && (ipRange = retIp.Split(',')) != null && ipRange.Length > 0)
            {
                ip = ipRange[0];
            }
            else
            {
                ip = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
            }

            if (ip.Equals("127.0.0.1") || ip.Equals("::1") || ip.StartsWith("192.168.")) ip = "81.218.199.175";

            if (ip.Contains(':'))
            {
                ip.Substring(0, ip.IndexOf(':'));
            }


            return ip.Trim();
        }

        public static string Generate32LengthGuid()
        {
            return Guid.NewGuid().ToString("N");
        }

        public static DateTime UnixTimeStampToDateTime(long unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToUniversalTime();
            return dtDateTime;
        }

        public static DateTime UnixTimeStampMillisecondsToDateTime(long unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            dtDateTime = dtDateTime.AddMilliseconds(unixTimeStamp).ToUniversalTime();
            return dtDateTime;
        }

        public static long DateTimeToUnixTimestamp(DateTime dateTime, bool isUniversal = true)
        {
            if (isUniversal)
            {
                return (long)(dateTime - new DateTime(1970, 1, 1).ToUniversalTime()).TotalSeconds;
            }
            else
            {
                return (long)(dateTime - new DateTime(1970, 1, 1)).TotalSeconds;
            }
        }

        public static long DateTimeToUnixTimestampMilliseconds(DateTime dateTime)
        {
            return (long)(dateTime - new DateTime(1970, 1, 1).ToUniversalTime()).TotalMilliseconds;
        }

        internal static string GetLanguageFromRequest()
        {
            var language = HttpContext.Current.Items[RequestParser.REQUEST_LANGUAGE];
            return language != null ? language.ToString() : null;
        }
    }
}