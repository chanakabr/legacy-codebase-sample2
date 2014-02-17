using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace ElasticSearch.Common
{
    public static class Utils
    {
        public static string GetWSURL(string key)
        {
            return TVinciShared.WS_Utils.GetTcmConfigValue(key);
        }

        public static string EscapeValues(ref string values)
        {
            string sRes = string.Empty;

            if (!string.IsNullOrWhiteSpace(values))
                sRes = values.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r\n", " ").Replace('\n', ' ').ToLower();

            return sRes;
        }

        public static string EscapeValues(string values)
        {
            string sRes = string.Empty;

            if (!string.IsNullOrWhiteSpace(values))
                sRes = values.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r\n", " ").Replace('\n', ' ').ToLower();

            return sRes;
        }
    }
}
