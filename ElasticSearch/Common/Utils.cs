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
        public static readonly string ES_PERCOLATOR_TYPE = ".percolator";

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

        public static string GetLangCodeAnalyzerKey(string languageCode, string version)
        {
            if (string.IsNullOrEmpty(version))
            {
                return string.Concat(languageCode, "_analyzer");
            }
            else
            {
                return string.Concat(languageCode, "_analyzer_v", version);
            }
        }

        public static string GetLangCodeFilterKey(string languageCode, string version)
        {
            if (string.IsNullOrEmpty(version))
            {
                return string.Concat(languageCode, "_filter");
            }
            else
            {
                return string.Concat(languageCode, "_filter_v", version);
            }
        }

        public static string GetLangCodeTokenizerKey(string languageCode, string version)
        {
            if (string.IsNullOrEmpty(version))
            {
                return string.Concat(languageCode, "_tokenizer");
            }
            else
            {
                return string.Concat(languageCode, "_tokenizer_v", version);
            }
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

        public static T ExtractValueFromToken<T>(JToken item, string fieldName)
        {
            T result = default(T);

            JToken tempToken = null;

            try
            {
                tempToken = item[fieldName];
            }
            catch
            {
            }

            if (tempToken == null)
            {
                tempToken = item.SelectToken(fieldName);
            }

            result = ExtractValueFromToken<T>(tempToken);

            return result;
        }

        public static T ExtractValueFromToken<T>(JToken tempToken)
        {
            T result = default(T);

            JArray tempArray = null;

            if (tempToken != null)
            {
                tempArray = tempToken as JArray;

                if (tempArray != null && tempArray.Count > 0)
                {
                    result = tempArray[0].ToObject<T>();
                }
                else
                {
                    result = tempToken.ToObject<T>();
                }
            }

            return result;
        }

        public static DateTime ExtractDateFromToken(JToken item, string fieldName)
        {
            DateTime result = new DateTime(1970, 1, 1, 0, 0, 0);
            string dateString = ExtractValueFromToken<string>(item, fieldName);

            if (!string.IsNullOrEmpty(dateString))
            {
                result = DateTime.ParseExact(dateString, ES_DATE_FORMAT, null);
            }

            return result;
        }

    }
}
