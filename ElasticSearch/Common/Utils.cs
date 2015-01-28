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

        private static readonly Dictionary<string, string> m_dicDocumentReservedCharacters = new Dictionary<string, string>()
        {
           {"\\", "\\\\"},
           {"&quot;", "\""},
           {"\"", "\\\""},
           {"\r\n", " "},
           {"\n", " "},
           {"\r", " "},
           {"\t", " "},
           {"\f", " "},
           {"\b", " "},
        };

        private static readonly Dictionary<string, string> m_dicQueryReservedCharacters = new Dictionary<string, string>()
        {
           {"\\", "\\\\"},
           {"\"", "\\\""},
           {"\r\n", " "},
           {"\n", " "},
           {"\r", " "},
           {"\t", " "},
           {"\f", " "}, 
           {"\b", " "},
           {"+", "\\+"},
           {"-", "\\\\-"},
           {"&&", "\\\\&&"},
           {"!", "\\\\!"},
           {"(", "\\\\("},
           {")", "\\\\)"},
           {"{", "\\\\{"},
           {"}", "\\\\}"},
           {"[", "\\\\["},
           {"]", "\\\\]"},
           {"^", "\\\\^"},
           {"~", "\\\\~"},
           {"*", "\\\\*"},
           {"?", "\\\\?"},
           {":", "\\\\:"},
           {"/", "\\\\/"}
        };

        /// <summary>
        /// Replaces special characters when inserting a new document
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static string ReplaceDocumentReservedCharacters(ref string values)
        {
            return Replace(ref values, m_dicDocumentReservedCharacters);
        }

        /// <summary>
        /// Replaces special characters when querying
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static string ReplaceQueryReservedCharacters(ref string values)
        {
            return Replace(ref values, m_dicQueryReservedCharacters);
        }

        private static string Replace(ref string p_sValue, Dictionary<string, string> p_dicReplacements)
        {
            if (string.IsNullOrEmpty(p_sValue))
            {
                return p_sValue;
            }

            StringBuilder sb = new StringBuilder(p_sValue, p_sValue.Length * 2);

            foreach (var kvpEscapeChar in p_dicReplacements)
            {
                sb.Replace(kvpEscapeChar.Key, kvpEscapeChar.Value);
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
