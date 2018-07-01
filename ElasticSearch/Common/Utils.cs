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

        public static string GetTcmValue(string sKey)
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
        public static string ReplaceDocumentReservedCharacters(string values, bool toLower = true)
        {
            return Replace(values, m_dicDocumentReservedCharacters, toLower);
        }

        /// <summary>
        /// Replaces special characters when querying
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static string ReplaceQueryReservedCharacters(string values)
        {
            return Replace(values, m_dicQueryReservedCharacters);
        }

        private static string Replace(string value, Dictionary<string, string> replacements, bool toLower = true)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            StringBuilder stringBuilder = new StringBuilder(value, value.Length * 2);

            foreach (var escapeChar in replacements)
            {
                stringBuilder.Replace(escapeChar.Key, escapeChar.Value);
            }

            string result = stringBuilder.ToString();

            if (toLower)
            {
                result = result.ToLower();
            }

            return result;
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

        public static string GetGroupMetadataIndex(int groupId)
        {
            return string.Concat(groupId, "_metadata");
        }

        public static string GetGroupChannelIndex(int groupId)
        {
            return string.Concat(groupId, "_channel");
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

        /// <summary>
        /// Parses a string to an enum, regardless of upper/lowercase issues
        /// </summary>
        /// <param name="typeString"></param>
        /// <returns></returns>
        public static ApiObjects.eAssetTypes ParseAssetType(string typeString)
        {
            ApiObjects.eAssetTypes typeEnum = ApiObjects.eAssetTypes.UNKNOWN;

            if (typeString.ToLower().StartsWith("media"))
            {
                typeEnum = ApiObjects.eAssetTypes.MEDIA;
            }
            else if (typeString.ToLower().StartsWith("epg"))
            {
                typeEnum = ApiObjects.eAssetTypes.EPG;
            }
            else if (typeString.ToLower().StartsWith("recording"))
            {
                typeEnum = ApiObjects.eAssetTypes.NPVR;
            }

            return typeEnum;
        }

        public static List<ElasticSearchApi.ESAssetDocument> DecodeAssetSearchJsonObject(string sObj, ref int totalItems,
            List<string> extraReturnFields = null, string prefix = "fields")
        {
            List<ElasticSearchApi.ESAssetDocument> documents = null;
            try
            {
                var jsonObj = JObject.Parse(sObj);

                if (jsonObj != null)
                {
                    JToken tempToken;
                    totalItems = ((tempToken = jsonObj.SelectToken("hits.total")) == null ? 0 : (int)tempToken);
                    if (totalItems > 0)
                    {
                        documents = new List<ElasticSearchApi.ESAssetDocument>();

                        foreach (var item in jsonObj.SelectToken("hits.hits"))
                        {
                            var newDocument = DecodeSingleAssetJsonObject(item, prefix, extraReturnFields);

                            documents.Add(newDocument);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Error - " + string.Format("Json Deserialization failed for ElasticSearch search request. Execption={0}", ex.Message), ex);
            }

            return documents;
        }

        public static ElasticSearchApi.ESAssetDocument DecodeSingleAssetJsonObject(JToken item, string fieldNamePrefix, List<string> extraReturnFields = null)
        {
            JToken tempToken = null;
            string typeString = ((tempToken = item.SelectToken("_type")) == null ? string.Empty : (string)tempToken);
            ApiObjects.eAssetTypes assetType = ParseAssetType(typeString);

            string assetIdField = string.Empty;

            switch (assetType)
            {
                case ApiObjects.eAssetTypes.MEDIA:
                {
                    assetIdField = AddPrefixToFieldName("media_id", fieldNamePrefix);
                    break;
                }
                case ApiObjects.eAssetTypes.EPG:
                {
                    assetIdField = AddPrefixToFieldName("epg_id", fieldNamePrefix);
                    break;
                }
                case ApiObjects.eAssetTypes.NPVR:
                {
                    assetIdField = AddPrefixToFieldName("recording_id", fieldNamePrefix);
                    break;
                }
                default:
                {
                    break;
                }
            }

            string id = ((tempToken = item.SelectToken("_id")) == null ? string.Empty : (string)tempToken);
            string index = ((tempToken = item.SelectToken("_index")) == null ? string.Empty : (string)tempToken);
            int assetId = 0;
            int groupId = 0;
            string name = string.Empty;
            DateTime cacheDate = new DateTime(1970, 1, 1, 0, 0, 0);
            DateTime updateDate = new DateTime(1970, 1, 1, 0, 0, 0);
            DateTime startDate = new DateTime(1970, 1, 1, 0, 0, 0);
            DateTime endDate = new DateTime(1970, 1, 1, 0, 0, 0);

            int mediaTypeId = 0;
            string epgIdentifier = string.Empty;

            assetId = ElasticSearch.Common.Utils.ExtractValueFromToken<int>(item, assetIdField);
            groupId = ElasticSearch.Common.Utils.ExtractValueFromToken<int>(item, AddPrefixToFieldName("group_id", fieldNamePrefix));

            var subItem = item;

            if (!string.IsNullOrEmpty(fieldNamePrefix))
            {
                subItem = item.SelectToken(fieldNamePrefix);
            }

            foreach (var subSubItem in subItem)
            {
                JProperty property = subSubItem as JProperty;

                if (property != null && property.Name.Contains("name"))
                {
                    name = ElasticSearch.Common.Utils.ExtractValueFromToken<string>(subItem, property.Name);

                    break;
                }
            }

            cacheDate = ElasticSearch.Common.Utils.ExtractDateFromToken(item, AddPrefixToFieldName("cache_date", fieldNamePrefix));
            updateDate = ElasticSearch.Common.Utils.ExtractDateFromToken(item, AddPrefixToFieldName("update_date", fieldNamePrefix));
            startDate = ElasticSearch.Common.Utils.ExtractDateFromToken(item, AddPrefixToFieldName("start_date", fieldNamePrefix));
            mediaTypeId = ElasticSearch.Common.Utils.ExtractValueFromToken<int>(item, AddPrefixToFieldName("media_type_id", fieldNamePrefix));
            epgIdentifier = ElasticSearch.Common.Utils.ExtractValueFromToken<string>(item, AddPrefixToFieldName("epg_identifier", fieldNamePrefix));
            endDate = ElasticSearch.Common.Utils.ExtractDateFromToken(item, AddPrefixToFieldName("end_date", fieldNamePrefix));

            var newDocument = new ElasticSearchApi.ESAssetDocument()
            {
                id = id,
                index = index,
                type = typeString,
                asset_id = assetId,
                group_id = groupId,
                name = name,
                cache_date = cacheDate,
                update_date = updateDate,
                start_date = startDate,
                end_date = endDate,
                media_type_id = mediaTypeId,
                epg_identifier = epgIdentifier,
            };

            if (extraReturnFields != null)
            {
                newDocument.extraReturnFields = new Dictionary<string, string>();

                foreach (var fieldName in extraReturnFields)
                {
                    string fieldValue = null;

                    if (fieldName.Contains("."))
                    {
                        var fieldsToken = item["fields"];

                        if (fieldsToken != null)
                        {
                            var specificFieldToken = fieldsToken[fieldName];

                            if (specificFieldToken != null)
                            {
                                fieldValue = ElasticSearch.Common.Utils.ExtractValueFromToken<string>(specificFieldToken);
                            }
                        }
                    }
                    else
                    {
                        fieldValue = ElasticSearch.Common.Utils.ExtractValueFromToken<string>(item, AddPrefixToFieldName(fieldName, fieldNamePrefix));
                    }

                    if (!string.IsNullOrEmpty(fieldValue))
                    {
                        newDocument.extraReturnFields.Add(fieldName, fieldValue);
                    }
                }
            }

            return newDocument;
        }

        public static ElasticSearchApi.ESAssetDocument DecodeSingleJsonObject(string sObj)
        {
            ElasticSearchApi.ESAssetDocument doc = null;

            try
            {
                var jsonObj = JObject.Parse(sObj);

                if (jsonObj != null)
                {
                    doc = DecodeSingleAssetJsonObject(jsonObj, "_source");
                }
            }
            catch (Exception ex)
            {
                log.Error("Error - " + string.Format("Json Deserialization failed for ElasticSearch request. Exception={0}", ex.Message), ex);
                doc = null;
            }

            return doc;
        }

        public static string AddPrefixToFieldName(string fieldName, string prefix)
        {
            string result = fieldName;

            if (!string.IsNullOrEmpty(prefix))
            {
                result = string.Format("{0}.{1}", prefix, fieldName);
            }

            return result;
        }

    }
}
