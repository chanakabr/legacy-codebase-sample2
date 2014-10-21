using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace ElasticSearch.Common
{
    public static class Utils
    {
        public static readonly string ES_STATS_TYPE = "stats";
        public static readonly string ES_DATE_FORMAT = "yyyyMMddHHmmss";

        public static string GetWSURL(string sKey)
        {
            string result = string.Empty;
            try
            {
                result = TCMClient.Settings.Instance.GetValue<string>(sKey);
            }
            catch (Exception ex)
            {
                result = string.Empty;
                Logger.Logger.Log("ElasticSearch.Common", "Key=" + sKey + "," + ex.Message, "Tcm");
            }
            return result;
        }

        private static readonly string[] EscapeChars = new[]
		                                               	{
		                                               		"\\", "\"",  "\r\n", "\n", "\r", "\t", "\f", "\b"
		                                               	};

        private static readonly string[] EscapeCharOutputs = new[]
		                                                     	{
		                                                     		"\\\\", "\\\"", " ", " ", " ", " ", " ", " "
		                                                     	};

        public static string EscapeValues(ref string values)
        {
            if (string.IsNullOrEmpty(values))
            {
                return values;
            }

            StringBuilder sb = new StringBuilder(values);
            for (int i = 0; i < EscapeChars.Length; i++)
            {
                sb = sb.Replace(EscapeChars[i], EscapeCharOutputs[i]);
            }

            return sb.ToString().ToLower();
        }

        public static string GetKeyNameWithPrefix(string sKey, string sPrefix)
        {
            string sRes;

            if (!string.IsNullOrEmpty(sPrefix))
            {
                sRes = string.Concat(sPrefix, ".", sKey);
            }
            else
            {
                sRes = sKey;
            }

            return sRes;
        }

        public static string GetLangCodeAnalyzerKey(string sLanguageCode)
        {
            return string.Concat(sLanguageCode, "_analyzer");
        }

        public static string GetLangCodeFilterKey(string sLanguageCode)
        {
            return string.Concat(sLanguageCode, "_filter");
        }

        public static string GetGroupStatisticsIndex(int nParentGroupId)
        {
            return string.Concat(nParentGroupId, "_statistics");
        }
    }
}
