using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using KLogMonitor;
using Newtonsoft.Json.Linq;

namespace ElasticSearch.Common
{
    public static class Utils
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

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
                log.Error("ElasticSearch.Common - Key=" + sKey + "," + ex.Message, ex);
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

        public static string GetLangCodeTokenizerKey(string languageCode)
        {
            return string.Concat(languageCode, "_tokenizer");
        }

        public static string GetGroupStatisticsIndex(int nParentGroupId)
        {
            return string.Concat(nParentGroupId, "_statistics");
        }

        public static List<string> GetDocumentIds(string originalString)
        {
            List<string> result = new List<string>();

            try
            {
                var jsonObj = JObject.Parse(originalString);

                if (jsonObj != null)
                {
                    JToken tempToken;
                    int totalItems = ((tempToken = jsonObj.SelectToken("hits.total")) == null ? 0 : (int)tempToken);

                    if (totalItems > 0)
                    {
                        foreach (var item in jsonObj.SelectToken("hits.hits"))
                        {
                            result.Add(((tempToken = item.SelectToken("_id")) == null ? string.Empty : (string)tempToken));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Error - " + string.Format("Json Deserialization failed for ElasticSearch search request. Execption={0}", ex.Message), ex);
            }

            return result;
        }
    }
}
