using ElasticSearch.Common.DeleteResults;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using KLogMonitor;
using System.Reflection;
using ConfigurationManager;
using System.Collections.Specialized;
using Newtonsoft.Json;
using ElasticSearch.Searcher;
using System.Net.Http;
using TVinciShared;

namespace ElasticSearch.Common
{
    public class ElasticSearchApi
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public static readonly string ES_URL = ApplicationConfiguration.Current.ElasticSearchConfiguration.URL.Value;
        private const string ES_LOG_FILENAME = "Elasticsearch";

        private static readonly HttpClient httpClient = HttpClientUtil.GetHttpClient(ApplicationConfiguration.Current.ElasticSearchHttpClientConfiguration);
        public string baseUrl
        {
            get;
            set;
        }

        #region Ctor

        public ElasticSearchApi()
        {
            baseUrl = ES_URL;
        }

        #endregion

        #region Index creation

        public bool BuildIndex(string index, int shards, int replicas,
            List<string> analyzers, List<string> filters, List<string> tokenizers = null, int maxResultWindow = 0)
        {
            bool bRes = false;

            if (string.IsNullOrEmpty(index))
                return bRes;

            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append(@"{ ""settings"": {");

            bool bShards = false;
            if ((shards > 0 && replicas > 0) || maxResultWindow > 0)
            {
                bShards = true;
                stringBuilder.Append(@"""index"": {");

                if (shards > 0 && replicas > 0)
                {
                    stringBuilder.AppendFormat(" \"number_of_shards\": {0}, \"number_of_replicas\": {1}", shards, replicas);

                    if (maxResultWindow > 0)
                    {
                        stringBuilder.Append(",");
                    }
                }

                if (maxResultWindow > 0)
                {
                    stringBuilder.AppendFormat("\"max_result_window\" : {0}", maxResultWindow);
                }

                stringBuilder.Append("} ");
            }

            #region add analyzers/filters/tokenizers

            if (bShards)
                stringBuilder.Append(",");

            stringBuilder.Append("\"analysis\": {");
            bool bAnalyzer = false;
            if (analyzers != null && analyzers.Count > 0)
            {
                bAnalyzer = true;
                stringBuilder.Append("\"analyzer\":{");
                stringBuilder.Append(string.Join(",", analyzers));
                stringBuilder.Append("}");
            }

            bool hasFilter = false;

            if (filters != null && filters.Count > 0)
            {
                if (bAnalyzer)
                    stringBuilder.Append(",");

                stringBuilder.Append("\"filter\":{");
                stringBuilder.Append(string.Join(",", filters));
                stringBuilder.Append("}");

                hasFilter = true;
            }

            if (tokenizers != null && tokenizers.Count > 0)
            {
                if (bAnalyzer || hasFilter)
                {
                    stringBuilder.Append(",");
                }

                stringBuilder.Append("\"tokenizer\":{");
                stringBuilder.Append(string.Join(",", tokenizers));
                stringBuilder.Append("}");
            }

            stringBuilder.Append("}");
            #endregion

            stringBuilder.Append("} }");

            string sUrl = string.Format("{0}/{1}", baseUrl, index);
            int nStatus = 0;

            string sResponse = SendPostHttpReq(sUrl, ref nStatus, string.Empty, string.Empty, stringBuilder.ToString(), true);

            bRes = (nStatus == 200) ? true : false;

            if (!bRes)
            {
                log.ErrorFormat("Error when building index {0}. Response is: {1}", index, sResponse);
            }

            return bRes;
        }

        public bool IndexExists(string sIndex)
        {
            bool result = false;

            if (!string.IsNullOrEmpty(sIndex))
            {

                string sUrl = string.Format("{0}/{1}/_settings", baseUrl, sIndex);
                int nStatus = 0;
                string sResponse = SendGetHttpReq(sUrl, ref nStatus);

                result = nStatus == 200;
            }

            return result;
        }



        public bool Reindex(string source, string destination, string filterQuery = null)
        {
            bool result = false;
            /*
             POST /_reindex
            {
              "source": {
                "index": "twitter"
                "query": .... <Optional>
              },
              "dest": {
                "index": "new_twitter"
              }
            }
             */
            try
            {
                string url = $"{baseUrl}/_reindex";

                var jsonBody = new JObject();

                jsonBody["source"] = new JObject();
                jsonBody["source"]["index"] = source;

                if (!string.IsNullOrEmpty(filterQuery))
                {
                    var filterQueryObject = JObject.Parse(filterQuery);
                    jsonBody["source"]["query"] = new JObject();
                    jsonBody["source"]["query"]["filtered"] = filterQueryObject;
                }

                jsonBody["dest"] = new JObject();
                jsonBody["dest"]["index"] = destination;




                string body = jsonBody.ToString();
                int status = 0;
                string postResult = SendPostHttpReq(url, ref status, string.Empty, string.Empty, body, true);
                log.Debug($"Reindex > result:[{postResult}]");
                if (!string.IsNullOrEmpty(postResult))
                {
                    /*
                     {
                        "took": 235,
                        "timed_out": false,
                        "total": 42,
                        "updated": 0,
                        "created": 42,
                        "batches": 1,
                        "version_conflicts": 0,
                        "noops": 0,
                        "retries": 0,
                        "failures": []
                      }
                     */
                    var jsonResult = JObject.Parse(postResult);

                    log.Debug($"Reindex of {source} to {destination} result is {postResult}");

                    result = true;
                }
            }
            catch (Exception ex)
            {
                log.Error($"Failed reindex of {source} to {destination} with ex {ex}", ex);
                result = false;
            }

            return result;
        }

        #region Index definitions: Analyzers, filters, tokenizers

        protected static Dictionary<string, string> dESAnalyzers = new Dictionary<string, string>();
        protected static Dictionary<string, string> dESFilters = new Dictionary<string, string>();
        protected static Dictionary<string, string> tokenizers = new Dictionary<string, string>();

        public static string GetAnalyzerDefinition(string sAnalyzerName)
        {
            string analyzer;

            if (!dESAnalyzers.TryGetValue(sAnalyzerName, out analyzer))
            {

                analyzer = Utils.GetTcmValue(sAnalyzerName);
                if (!string.IsNullOrEmpty(analyzer))
                    dESAnalyzers[sAnalyzerName] = analyzer;
            }

            return analyzer;
        }

        public static string GetFilterDefinition(string sFilterName)
        {
            string filter;

            if (!dESFilters.TryGetValue(sFilterName, out filter))
            {
                filter = Utils.GetTcmValue(sFilterName);
                if (!string.IsNullOrEmpty(filter))
                    dESFilters[sFilterName] = filter;
            }

            return filter;
        }

        public static string GetTokenizerDefinition(string tokenizerName)
        {
            string tokenizer;

            if (!tokenizers.TryGetValue(tokenizerName, out tokenizer))
            {
                tokenizer = Utils.GetTcmValue(tokenizerName);

                if (!string.IsNullOrEmpty(tokenizer))
                {
                    tokenizers[tokenizerName] = tokenizer;
                }
            }

            return tokenizer;
        }

        public static bool AnalyzerExists(string sAnalyzerName)
        {
            bool bResult = string.IsNullOrEmpty(GetAnalyzerDefinition(sAnalyzerName)) ? false : true;

            return bResult;
        }

        public static bool FilterExists(string sFilterName)
        {
            bool bResult = string.IsNullOrEmpty(GetFilterDefinition(sFilterName)) ? false : true;

            return bResult;
        }

        public static bool TokenizerExists(string tokenizerName)
        {
            bool result = string.IsNullOrEmpty(GetTokenizerDefinition(tokenizerName)) ? false : true;

            return result;
        }

        #endregion

        #endregion

        #region Mapping

        public string GetAllMappings(string sIndex)
        {
            string sRes = string.Empty;

            if (!string.IsNullOrEmpty(sIndex))
            {
                string sUrl = string.Format("{0}/{1}/_mapping", baseUrl, sIndex);
                int nStatus = 0;
                string sResponse = SendGetHttpReq(sUrl, ref nStatus);

                sRes = (nStatus == 200) ? sResponse : sRes;
            }

            return sRes;
        }

        public bool InsertMapping(string sIndex, string sMapName, string sMappingObject)
        {
            bool bResult = false;

            if (!string.IsNullOrEmpty(sIndex) && !string.IsNullOrEmpty(sMappingObject) && !string.IsNullOrEmpty(sMapName))
            {
                string sUrl = string.Format("{0}/{1}/{2}/_mapping", baseUrl, sIndex, sMapName);

                int nStatus = 0;

                string sRetval = SendPostHttpReq(sUrl, ref nStatus, string.Empty, string.Empty, sMappingObject, true);

                bResult = (nStatus == 200) ? true : false;

                if (!bResult)
                    log.Error("Error - " + string.Format("Failed creating map. Explaination: {0}", sRetval));
            }

            return bResult;
        }

        public bool MappingExists(string sIndex, string sType)
        {
            bool bRes = false;

            if (!string.IsNullOrEmpty(sIndex) && !string.IsNullOrEmpty(sType))
            {

                string sUrl = string.Format("{0}/{1}/{2}/_mapping", baseUrl, sIndex, sType);
                int nStatus = 0;
                string sResponse = SendGetHttpReq(sUrl, ref nStatus);

                bRes = (nStatus == 200) ? true : false;
            }

            return bRes;
        }

        #endregion

        #region Aliases

        #region Switch index, delete old indices (For rebuildind index)

        public bool SwitchIndex(string index, string alias, List<string> indicesForRemoval, string sSearchRouting = null)
        {
            bool result = false;

            if (!string.IsNullOrEmpty(index) && !string.IsNullOrEmpty(alias))
            {
                StringBuilder httpRequestBody = new StringBuilder();
                httpRequestBody.Append("{ \"actions\": [");

                if (indicesForRemoval != null && indicesForRemoval.Count > 0)
                {
                    foreach (string sOldIndex in indicesForRemoval)
                    {
                        httpRequestBody.Append(@" { ""remove"": { ");
                        httpRequestBody.AppendFormat(" \"alias\": \"{0}\", \"index\": \"{1}\"", alias, sOldIndex);
                        httpRequestBody.Append(" } },");
                    }
                }
                httpRequestBody.Append(@" { ""add"": { ");
                httpRequestBody.AppendFormat(" \"alias\": \"{0}\", \"index\": \"{1}\"", alias, index);

                if (!string.IsNullOrEmpty(sSearchRouting))
                {
                    httpRequestBody.AppendFormat(", \"routing\":\"{0}\"", sSearchRouting);
                }

                httpRequestBody.Append(" } } ] }");

                string url = string.Format("{0}/_aliases", baseUrl);
                int httpStatus = 0;

                string postResultString = SendPostHttpReq(url, ref httpStatus, string.Empty, string.Empty, httpRequestBody.ToString(), true);

                result = (httpStatus == 200) ? true : false;

                if (result == false)
                    log.Error("Error - " + string.Format("error received when trying to switch indices. Message: {0}", postResultString));
            }

            return result;
        }

        public void DeleteIndices(List<string> lIndices)
        {
            if (lIndices != null)
            {
                string sUrl;
                int nStatus;
                foreach (string sIndex in lIndices)
                {
                    sUrl = string.Format("{0}/{1}", baseUrl, sIndex);
                    nStatus = 0;
                    string sRetval = SendDeleteHttpReq(sUrl, ref nStatus, string.Empty, string.Empty, string.Empty, true);

                    if (nStatus != 200)
                        log.Error("Error - " + string.Format("Unable to delete index. index={0}; Explanation{1}", sIndex, sRetval));
                }

            }
        }

        #endregion

        public List<string> GetAliases(string sIndex)
        {
            List<string> aliases = new List<string>();

            if (!string.IsNullOrEmpty(sIndex))
            {
                string url = string.Format("{0}/{1}/_aliases", baseUrl, sIndex);
                int status = 0;

                string httpResponse = SendGetHttpReq(url, ref status);

                if (status == 200 && !string.IsNullOrEmpty(httpResponse))
                {
                    try
                    {
                        var jsonObj =
                            JObject.Parse(httpResponse);

                        if (jsonObj != null)
                        {
                            foreach (var alias in jsonObj)
                            {
                                string indexName = alias.Key;

                                if (!string.IsNullOrEmpty(indexName))
                                    aliases.Add(indexName);
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        string s = ex.Message;
                    }
                }
            }

            return aliases;

        }

        /// <summary>
        /// Get a list of indcies using _cluser/state api
        /// </summary>
        /// <param name="pathQuery">
        /// query path always starts with metadata.indices. and continiues with whatever is sent in this param.
        /// i.e: if you like to seah for indices of group 203 use "203*" or "203_epg_*" for all epg indices.
        /// default values is set to * and will return all indices in elasticsearch
        /// </param>
        /// <returns></returns>
        public List<ESIndex> ListIndices(string indexQueryPattern = "*")
        {
            // cannot hold dots '.' because its part of the filter_path argument sent to elasticsearch
            if (indexQueryPattern.Contains('.')) { throw new ArgumentException("value cannot hold '.' charecters", "indexQueryPattern"); }

            var url = $"{baseUrl}/_cluster/state?filter_path=metadata.indices.{indexQueryPattern}.aliases";
            var status = 0;
            log.Debug($"Elasticsearch ListIndices > request GET:[{url}]");
            var ret = SendGetHttpReq(url, ref status);
            #region example response
            // Example Response (depending on the pathQuery for example pathQuery=metadata.indices.198_epg_*.aliases)
            //{
            //  "metadata": {
            //    "indices": {
            //      "198_epg_v2_20190610_11484": {
            //        "aliases": [
            //          "198_epg",
            //          "198_epg_v2_20190610"
            //        ]
            //      },
            //      "198_epg_v2_20190613_11484": {
            //        "aliases": []
            //      }
            //    }
            //  }
            //}
            #endregion

            log.Debug($"Elasticsearch ListIndices > response:[{ret}]");
            if (status != 200)
            {
                log.Error($"Error getting ListIndices, status:[{status}], msg:[{ret}]");
                throw new Exception($"Error getting ListIndices, status:[{status}]");
            }

            if (string.IsNullOrWhiteSpace(ret) || ret == "{}")
            {
                return new List<ESIndex>();
            }

            var clustureStats = JObject.Parse(ret);

            var indices = clustureStats.SelectToken("metadata.indices")
                .Children<JProperty>()
                .Select(p => new ESIndex
                {
                    Name = p.Name,
                    Aliases = p.Value["aliases"].ToObject<IEnumerable<string>>(),
                }).ToList();

            return indices;
        }


        public List<ESIndex> ListIndicesByAlias(string aliasQueryPattern = "*")
        {
            // cannot hold dots '.' because its part of the filter_path argument sent to elasticsearch
            if (aliasQueryPattern.Contains('.')) { throw new ArgumentException("value cannot hold '.' charecters", "aliasQueryPattern"); }


            //e.g: http://elasticsearch.service.consul:9200/_aliases?pretty&filter_path=*.aliases.*DELETION_CANDIDATE*
            var url = $"{baseUrl}/_aliases?filter_path=*.aliases.{aliasQueryPattern}*";
            var status = 0;
            log.Debug($"Elasticsearch ListIndicesByAlias > request GET:[{url}]");
            var ret = SendGetHttpReq(url, ref status);
            #region example response
            // Example Response (depending on the pathQuery for example pathQuery=metadata.indices.198_epg_*.aliases)
            //{
            //"1483_20200531124220" : {
            //    "aliases" : {
            //        "1483" : { },
            //     "deletion_candiadte" : { }
            //      }
            //  },
            //"203_recording_20200531124211" : {
            //    "aliases" : {
            //        "203_recording" : { }
            //    }
            //},
            //"1483_channel_20200531124410" : {
            //    "aliases" : {
            //        "1483_channel" : { }
            //    }
            //},
            //"203_epg_20200531124149" : {
            //    "aliases" : {
            //        "203_epg" : { }
            //    }
            //},
            //"1483_metadata_20200531124428" : {
            //    "aliases" : {
            //        "1483_metadata" : { }
            //    }
            //},
            //"utils_20191017083135" : {
            //    "aliases" : {
            //        "utils" : { }
            //    }
            //},
            //"1483_epg_20200531124323" : {
            //    "aliases" : {
            //        "1483_epg" : { }
            //    }
            //},
            //"203_20200531124132" : {
            //    "aliases" : {
            //        "203" : { }
            //    }
            //},
            //"1483_recording_20200531124429" : {
            //    "aliases" : {
            //        "1483_recording" : { }
            //    }
            //}
            //}
            #endregion

            log.Debug($"Elasticsearch ListIndicesByAlias > response:[{ret}]");
            if (status != 200)
            {
                log.Error($"Error getting ListIndicesByAlias, status:[{status}], msg:[{ret}]");
                throw new Exception($"Error getting ListIndicesByAlias, status:[{status}]");
            }

            var indicesByAlias = JObject.Parse(ret);
            var indices = indicesByAlias.Properties().Select(index => new ESIndex
            {
                Name = index.Name,
                Aliases = index.Value.SelectToken("aliases").
            ToObject<IDictionary<string, object>>()
            .Select(x => x.Key)
            }).ToList();

            return indices;
        }


        public bool AddAlias(string index, string alias)
        {
            bool result = false;

            if (string.IsNullOrEmpty(index) || string.IsNullOrEmpty(alias))
                return result;

            // /{index}/_alias/{name}
            string url = string.Format("{0}/{1}/_alias/{2}", baseUrl, index, alias);

            try
            {
                string putResult = SendPutHttpRequest(url, string.Empty);

                JObject jsonResult = JObject.Parse(putResult);

                if (jsonResult.ContainsKey("acknowledged") && jsonResult["acknowledged"].Value<bool>())
                {
                    result = true;
                }
                else
                {
                    log.ErrorFormat("error when adding alias {0} to index {1} putResult = {2}", alias, index, putResult);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("error when adding alias {0} to index {1} ex = {2}", alias, index, ex);
            }

            return result;
        }

        public bool RemoveAlias(string index, string alias)
        {
            bool result = false;

            if (string.IsNullOrEmpty(index) || string.IsNullOrEmpty(alias))
                return result;

            // /{index}/_alias/{name}
            string url = string.Format("{0}/{1}/_alias/{2}", baseUrl, index, alias);

            try
            {
                int status = 0;
                string deleteResult = SendDeleteHttpReq(url, ref status, string.Empty, string.Empty, string.Empty, true);

                var jsonResult = JObject.Parse(deleteResult);
                result = jsonResult["acknowledged"].Value<bool>();
                if (!result)
                {
                    log.ErrorFormat("error when removing alias {0} to index {1} putResult = {2}", alias, index, deleteResult);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("error when removing alias {0} to index {1} ex = {2}", alias, index, ex);
            }

            return result;
        }

        #endregion

        #region Insert, Update, Delete

        public bool InsertRecord(string sIndex, string sType, string sID, string sDoc)
        {
            bool bRes = false;
            if (string.IsNullOrEmpty(sIndex) || string.IsNullOrEmpty(sType) || string.IsNullOrEmpty(sDoc) || string.IsNullOrEmpty(sID))
                return bRes;

            string sUrl = string.Format("{0}/{1}/{2}/{3}", baseUrl, sIndex, sType, sID);
            int nStatus = 0;

            string sRes = SendPostHttpReq(sUrl, ref nStatus, string.Empty, string.Empty, sDoc, true);

            bRes = (nStatus == 200) ? true : false;
            if (!bRes)
                log.Error("Error - " + string.Format("Unable to insert record into elasticsearch. url={0};document={1};Explanation{2}", sUrl, sDoc, sRes));

            return bRes;
        }

        public ESDeleteResult DeleteDoc(string sIndex, string sType, string sId)
        {
            ESDeleteResult deleteResult = null;

            if (string.IsNullOrEmpty(sIndex) || string.IsNullOrEmpty(sType) || string.IsNullOrEmpty(sId))
            {
                deleteResult = new ESDeleteResult();
                return deleteResult;
            }

            string sUrl = string.Format("{0}/{1}/{2}/{3}", baseUrl, sIndex, sType, sId);
            int nStatus = 0;

            string sRetVal = SendDeleteHttpReq(sUrl, ref nStatus, string.Empty, string.Empty, string.Empty, true);

            log.DebugFormat("ElasticSearchApi DeleteDoc result: {0}", sRetVal);

            deleteResult = ESDeleteResult.GetDeleteResult(sRetVal);

            if (nStatus == 200 && deleteResult != null)
            {
                deleteResult.Ok = true;
            }

            return deleteResult;
        }

        public bool DeleteDocsByQuery(string sIndex, string sType, ref string sQuery)
        {
            bool bResult = true;
            try
            {
                string sUrl = string.Format("{0}/{1}/{2}/_query", baseUrl, sIndex, sType);
                int nStatus = 0;

                string sResult = SendDeleteHttpReq(sUrl, ref nStatus, string.Empty, string.Empty, sQuery, true);
                log.DebugFormat("Status - DeleteDocsByQuery. Returned JSON from ES: {0}, Query: {1}", sResult, sQuery);
                bResult = nStatus == 200;
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder("Exception at DeleteDocsByQuery.");
                sb.Append(String.Concat(" Ex Msg: ", ex.Message));
                sb.Append(String.Concat(" Index: ", sIndex));
                sb.Append(String.Concat(" Type: ", sType));
                sb.Append(String.Concat(" Query: ", sQuery));
                sb.Append(String.Concat(" Ex Type: ", ex.GetType().Name));
                sb.Append(String.Concat(" ST: ", ex.StackTrace));
                log.Error("Exception - " + sb.ToString(), ex);
                bResult = false;
            }

            return bResult;
        }

        public bool DeleteDocsByQuery(string sIndex, string sType, ref string sQuery, out int countDeleted)
        {
            countDeleted = 0;
            bool bResult = true;
            try
            {
                string sUrl = string.Format("{0}/{1}/{2}/_query", baseUrl, sIndex, sType);
                int nStatus = 0;

                string sResult = SendDeleteHttpReq(sUrl, ref nStatus, string.Empty, string.Empty, sQuery, true);
                log.DebugFormat("Status - DeleteDocsByQuery. Returned JSON from ES: {0}, Query: {1}", sResult, sQuery);
                bResult = nStatus == 200;
                var jsonResult = JObject.Parse(sResult);
                if (jsonResult != null)
                {
                    countDeleted = jsonResult["_indices"]["_all"]["deleted"].Value<int>();
                }
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder("Exception at DeleteDocsByQuery.");
                sb.Append(String.Concat(" Ex Msg: ", ex.Message));
                sb.Append(String.Concat(" Index: ", sIndex));
                sb.Append(String.Concat(" Type: ", sType));
                sb.Append(String.Concat(" Query: ", sQuery));
                sb.Append(String.Concat(" Ex Type: ", ex.GetType().Name));
                sb.Append(String.Concat(" ST: ", ex.StackTrace));
                log.Error("Exception - " + sb.ToString(), ex);
                bResult = false;
            }

            return bResult;
        }

        /// <summary>
        /// Performs a partial update on a document
        /// </summary>
        /// <param name="index"></param>
        /// <param name="type"></param>
        /// <param name="docId"></param>
        /// <param name="partialUpdate"></param>
        /// <returns></returns>
        public bool PartialUpdate(string index, string type, string docId, string partialUpdate)
        {
            bool result = false;

            // Validate parameters
            if (string.IsNullOrEmpty(index) || string.IsNullOrEmpty(type) || string.IsNullOrEmpty(docId) || string.IsNullOrEmpty(partialUpdate))
            {
                return result;
            }

            string url = string.Format("{0}/{1}/{2}/{3}/_update", baseUrl, index, type, docId);
            int httpStatus = 0;

            string postResult = SendPostHttpReq(url, ref httpStatus, string.Empty, string.Empty, partialUpdate, true);

            result = (httpStatus == 200) ? true : false;

            if (!result)
            {
                log.Error("Error - " + string.Format("Unable to partial update record into elasticsearch. url={0};id={1};Explanation{2}", baseUrl, docId, postResult));
            }

            return result;
        }

        /// <summary>
        /// Increases a single value of a document using a partial update. Useful for counting an additional view, for example
        /// </summary>
        /// <param name="index"></param>
        /// <param name="type"></param>
        /// <param name="docId"></param>
        /// <param name="field"></param>
        /// <returns></returns>
        public bool IncrementField(string index, string type, string docId, string field)
        {
            bool result = false;
            string partialUpdate = string.Concat("{ \"script\": \"ctx._source.", field, "+=1\" }");

            result = PartialUpdate(index, type, docId, partialUpdate);

            return result;
        }

        /// <summary>
        /// Decreases a single value of a document using a partial update. Useful for removing a like, for example
        /// </summary>
        /// <param name="index"></param>
        /// <param name="type"></param>
        /// <param name="docId"></param>
        /// <param name="field"></param>
        /// <returns></returns>
        public bool DecrementField(string index, string type, string docId, string field)
        {
            bool result = false;
            string partialUpdate = string.Concat("{ \"script\": \"ctx._source.", field, "-=1\" }");

            result = PartialUpdate(index, type, docId, partialUpdate);

            return result;
        }

        #endregion

        #region Bulk Requests

        /// <summary>
        /// Creates and sends an ElasticSearch bulk request bulk request. Returns the requests that failed and their errors.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bulkRequests"></param>
        /// <returns></returns>
        public List<KeyValuePair<string, string>> CreateBulkRequest<T>(List<ESBulkRequestObj<T>> bulkRequests)
        {
            List<ESBulkRequestObj<T>> invalidRecords;
            CreateBulkRequests(bulkRequests, out invalidRecords);

            List<KeyValuePair<string, string>> result = null;

            if (invalidRecords != null)
            {
                result = invalidRecords.Select(item => new KeyValuePair<string, string>(item.docID.ToString(), item.error)).ToList();
            }

            return result;
        }

        /// <summary>
        /// Creates and sends an ElasticSearch bulk request bulk request. Returns whether the request succeeded or not, outs the invalid requests
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bulkRequests"></param>
        /// <returns></returns>
        public bool CreateBulkRequests<T>(List<ESBulkRequestObj<T>> bulkRequests, out List<ESBulkRequestObj<T>> invalidRecords)
        {
            bool success = true;
            log.Debug("Start Elastic Search Bulk requests");
            StringBuilder requestString = new StringBuilder();
            invalidRecords = new List<ESBulkRequestObj<T>>();

            // Serialize/Build elastic search request body
            if (bulkRequests != null)
            {
                foreach (var bulk in bulkRequests)
                {
                    // remove any new line
                    bulk.document = bulk.document.Replace(Environment.NewLine, " ").Replace("\n", " ");

                    requestString.Append("{ \"");
                    requestString.Append(bulk.Operation.ToString());
                    requestString.Append("\": { ");

                    requestString.AppendFormat("\"_index\": \"{0}\"", bulk.index);
                    requestString.AppendFormat(", \"_type\": \"{0}\"", bulk.type);

                    if (!string.IsNullOrEmpty(bulk.routing))
                    {
                        requestString.AppendFormat(", \"_routing\": \"{0}\"", bulk.routing);
                    }

                    if (!string.IsNullOrEmpty(bulk.ttl))
                    {
                        requestString.AppendFormat(", \"_ttl\": \"{0}\"", bulk.ttl);
                    }

                    requestString.AppendFormat(",\"_id\" : \"{0}\"", bulk.docID);

                    requestString.Append(" } }\n");

                    if (!string.IsNullOrEmpty(bulk.document))
                    {
                        requestString.AppendFormat("{0}\n", bulk.document);
                    }
                }
            }

            // send request to ES server
            string url = string.Format("{0}/_bulk", baseUrl);
            int httpStatus = 0;
            string bodyRequest = requestString.ToString();

            string response = SendPostHttpReq(url, ref httpStatus, string.Empty, string.Empty, bodyRequest, true);

            // docId_documentType
            string keyFormat = "{0}_{1}";
            // Find out if there are errors
            try
            {
                if (!string.IsNullOrEmpty(response))
                {
                    var json = JObject.Parse(response);

                    if (json != null)
                    {
                        var errors = json["errors"];

                        //json["items"][0].First.First.ToString()
                        // If there are errors, report it
                        if (errors != null && Convert.ToBoolean(errors))
                        {
                            success = false;
                            var failedBulkRequests = bulkRequests.ToDictionary(item => GetDocumentUniqueKey(item.index, item.type, item.docID), item => item);

                            var items = json["items"];

                            foreach (var item in items)
                            {
                                if (item.First != null && item.First.First != null)
                                {
                                    var itemError = item.First.First["error"];
                                    var id = item.First.First["_id"].ToString();
                                    var type = item.First.First["_type"].ToString();
                                    var index = item.First.First["_index"].ToString();

                                    string key = GetDocumentUniqueKey(index, type, id);

                                    if (itemError != null)
                                    {
                                        var failedRequest = failedBulkRequests[key];
                                        failedRequest.error = itemError.ToString();

                                        invalidRecords.Add(failedRequest);
                                        log.ErrorFormat("Failed indexing percolator for channel {0} because of error {1}", key, itemError.ToString());
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                success = false;
                log.ErrorFormat("Failed parsing Elastic Search bulk request, error = {0}", ex);
                invalidRecords.AddRange(bulkRequests);
            }

            return success;
        }

        private string GetDocumentUniqueKey<T>(string index, string type, T documentId)
        {
            return $"{index}_{type}_{documentId}";
        }
        /// <summary>
        /// Creates and sends an ElasticSearch bulk request bulk request. Returns the requests that failed and their errors.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="index"></param>
        /// <param name="type"></param>
        /// <param name="objects"></param>
        /// <param name="routing"></param>
        /// <returns></returns>
        public List<KeyValuePair<string, string>> CreateBulkIndexRequest<T>(
            string index, string type, List<KeyValuePair<T, string>> objects, string routing = null)
        {
            log.Debug("Start ES Update - Start Bulk Update");

            StringBuilder sBulkRequest = new StringBuilder();

            List<ESBulkRequestObj<T>> bulkRequests = new List<ESBulkRequestObj<T>>();

            // Create a bulk request object from each of the objects in the parameter list
            foreach (var item in objects)
            {
                bulkRequests.Add(new ESBulkRequestObj<T>()
                {
                    docID = item.Key,
                    document = item.Value,
                    index = index,
                    type = type,
                    routing = routing,
                    Operation = eOperation.index
                });
            }

            List<KeyValuePair<T, string>> sInvalidRecords = new List<KeyValuePair<T, string>>();

            // Use other method to perform request
            var requestResult = CreateBulkRequest(bulkRequests);

            return requestResult;
        }

        #endregion

        #region Search

        public string Search(string index, string indexType, ref string searchQuery, List<string> routing = null, string preference = null)
        {
            string result = string.Empty;

            if (string.IsNullOrEmpty(index) || string.IsNullOrEmpty(searchQuery))
                return result;

            Search(index, indexType, searchQuery, routing, preference, out result, out string url, out int httpStatus);
            
            return result;
        }

        public Tuple<string, int> SearchWithStatus(string index, string indexType, ref string searchQuery, List<string> routing = null, string preference = null)
        {
            var result = string.Empty;

            if (string.IsNullOrEmpty(index) || string.IsNullOrEmpty(searchQuery))
                return Tuple.Create(result, 0);

            Search(index, indexType, searchQuery, routing, preference, out result, out string url, out int httpStatus);

            return Tuple.Create(result, httpStatus);
        }

        private void Search(string index, string indexType, string searchQuery, List<string> routing, string preference, out string result, out string url, out int httpStatus)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat("{0}/{1}/{2}/_search", baseUrl, index, indexType);

            List<string> queryParameters = new List<string>();

            if (routing != null && routing.Count > 0)
            {
                string combinedRouting = routing.Aggregate((current, next) => current + "," + next);
                queryParameters.Add(string.Format("routing={0}", combinedRouting));
            }

            if (!string.IsNullOrEmpty(preference))
            {
                queryParameters.Add(string.Format("preference={0}", preference));
            }

            if (queryParameters.Count > 0)
            {
                string queryString = string.Join("&", queryParameters);
                builder.AppendFormat("?{0}", queryString);
            }

            url = builder.ToString();
            httpStatus = 0;
            result = SendPostHttpReq(url, ref httpStatus, string.Empty, string.Empty, searchQuery, true);
        }

        public string MultiSearch(string sIndex, string sType, List<string> lSearchQueries, List<string> lRouting)
        {
            string sRes = string.Empty;

            if (string.IsNullOrEmpty(sIndex) || string.IsNullOrEmpty(sType) || lSearchQueries == null || lSearchQueries.Count <= 0)
                return sRes;

            StringBuilder sb = new StringBuilder();
            string routingStr = string.Empty;
            if (lRouting != null && lRouting.Count > 0)
            {
                routingStr = lRouting.Aggregate((current, next) => String.Concat(current, ",", next));
            }

            foreach (string query in lSearchQueries)
            {
                if (string.IsNullOrEmpty(query))
                    continue;

                sb.Append("{");
                if (routingStr.Length == 0)
                {
                    sb.AppendFormat("\"index\":\"{0}\", \"type\":\"{1}\"", sIndex, sType);
                }
                else
                {
                    sb.AppendFormat("\"index\":\"{0}\", \"type\":\"{1}\", \"routing\":\"{2}\"", sIndex, sType, routingStr);
                }
                sb.Append("}\n");
                sb.Append(query);
                sb.Append("\n");
            }

            string sUrl = string.Format("{0}/_msearch", baseUrl);
            int nStatus = 0;
            sRes = SendPostHttpReq(sUrl, ref nStatus, string.Empty, string.Empty, sb.ToString(), true);

            if (nStatus != 200)
            {
                log.Error("Error - " + string.Format("Search query failed. url={0};query={1}; Explanation={2}", sUrl, sb.ToString()));
                sRes = string.Empty;
            }

            return sRes;
        }

        #endregion

        #region Percolator

        public List<string> SearchPercolator(string sIndex, string sType, ref string sDoc)
        {
            List<string> lResult = new List<string>();

            if (string.IsNullOrEmpty(sIndex) || string.IsNullOrEmpty(sType) || string.IsNullOrEmpty(sDoc))
                return lResult;

            string sUrl = string.Format("{0}/{1}/{2}/_percolate ", baseUrl, sIndex, sType);
            int nStatus = 0;

            string retVal = SendPostHttpReq(sUrl, ref nStatus, string.Empty, string.Empty, sDoc, true);

            if (nStatus != 200)
            {
                log.Error("Error - " + string.Format("Search Percolator query failed. url={0};doc={1}; Explanation={2}", sUrl, sDoc, retVal));
            }
            else
            {
                try
                {
                    var jsonObj = JObject.Parse(retVal);

                    if (jsonObj != null)
                    {
                        JToken jToken = jsonObj.SelectToken("matches");
                        if (jToken != null)
                        {
                            lResult = jToken.Select(item =>
                            {
                                if (item is JValue)
                                {
                                    return item.ToString();
                                }

                                var itemId = item["_id"];

                                if (itemId != null)
                                {
                                    return itemId.ToString();
                                }
                                else
                                {
                                    return item.ToString();
                                }
                            }
                            ).ToList();
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.Error("Error - " + string.Format("SearchPercolator Could not parse response. Ex={0}", ex.Message));
                }
            }

            return lResult;
        }

        public bool AddQueryToPercolator(string sIndex, string sQueryName, ref string sQuery)
        {
            bool bResult = false;

            if (string.IsNullOrEmpty(sIndex) || string.IsNullOrEmpty(sQueryName) || string.IsNullOrEmpty(sQuery))
                return bResult;

            string sUrl = string.Format("{0}/_percolator/{1}/{2} ", baseUrl, sIndex, sQueryName);
            int nStatus = 0;

            string sRetVal = SendPostHttpReq(sUrl, ref nStatus, string.Empty, string.Empty, sQuery, true);

            if (nStatus == 200)
            {
                bResult = true;
            }
            else
            {
                log.Error("Error - " + string.Format("Adding Query to Percolator failed. url={0};query={1}; Explanation={2}", sUrl, sQuery, sRetVal));
            }

            return bResult;
        }

        public bool AddQueryToPercolatorV2(string index, string queryId, ref string queryBody)
        {
            bool result = false;

            if (string.IsNullOrEmpty(index) || string.IsNullOrEmpty(queryId) || string.IsNullOrEmpty(queryBody))
                return result;

            string sUrl = string.Format("{0}/{1}/.percolator/{2} ", baseUrl, index, queryId);
            int nStatus = 0;

            string httpResult = SendPostHttpReq(sUrl, ref nStatus, string.Empty, string.Empty, queryBody, true);

            if (nStatus == 200)
            {
                result = true;
            }
            else
            {
                log.Error("Error - " + string.Format("Adding Query to Percolator failed. url={0};query={1}; Explanation={2}", sUrl, queryBody, httpResult));
            }

            return result;
        }

        #endregion

        #region Get methods

        public string GetDoc(string sIndex, string sType, string sDocId)
        {
            string sRes = string.Empty;

            if (string.IsNullOrEmpty(sIndex) || string.IsNullOrEmpty(sType))
                return sRes;

            string sUrl = string.Format("{0}/{1}/{2}/{3}", baseUrl, sIndex, sType, sDocId);
            int nStatus = 0;

            sRes = SendGetHttpReq(sUrl, ref nStatus);

            if (nStatus != 200)
            {
                log.Error("Error - " + string.Format("Get record failed. url={0};docID={1}", sUrl, sDocId));
                sRes = string.Empty;
            }

            return sRes;
        }

        public string GetDoc(string sIndex, string sType, string sDocId, string routing)
        {
            string sRes = string.Empty;

            if (string.IsNullOrEmpty(sIndex) || string.IsNullOrEmpty(sType))
                return sRes;

            string sUrl = string.Format("{0}/{1}/{2}/{3}?routing={4}", baseUrl, sIndex, sType, sDocId, routing);
            int nStatus = 0;

            sRes = SendGetHttpReq(sUrl, ref nStatus);

            if (nStatus != 200)
            {
                log.Error("Error - " + string.Format("Get record failed. url={0};docID={1}", sUrl, sDocId));
                sRes = string.Empty;
            }

            return sRes;
        }

        public string MultiGetIDs<T>(string sIndex, string sType, List<T> oIDsList, int nNumOfResultsToReturn)
        {
            string res = string.Empty;
            string sQuery = string.Empty;
            if (!string.IsNullOrEmpty(sIndex) && !string.IsNullOrEmpty(sType) && oIDsList != null && oIDsList.Count > 0)
            {
                sQuery = BuildDirectIDsQuery(oIDsList, nNumOfResultsToReturn);
                if (sQuery.Length > 0)
                {
                    string sUrl = string.Format("{0}/{1}/{2}/_mget", baseUrl, sIndex, sType);
                    int nHttpStatusCode = 0;
                    res = SendPostHttpReq(sUrl, ref nHttpStatusCode, string.Empty, string.Empty, sQuery, true);
                    if (nHttpStatusCode != 200)
                    {
                        res = string.Empty;
                        #region Logging
                        log.Debug("MultiGetIDs - " + string.Format("Http status code: {0} , Index: {1} , Type: {2} , typeof(T) : {3} Query sent: {4}", nHttpStatusCode, sIndex, sType, oIDsList[0].GetType().Name, sQuery));
                        #endregion
                    }
                }
            }
            return res;
        }

        private string BuildDirectIDsQuery<T>(List<T> oIDsList, int nNumOfResultsToReturn)
        {
            string res = string.Empty;
            string sTypeName = oIDsList[0].GetType().Name.ToLower().Trim();
            int listLength = oIDsList.Count;
            StringBuilder sb = null;
            switch (sTypeName)
            {
                case "int16":
                case "int32":
                case "int64":
                    {
                        sb = new StringBuilder(String.Concat("{\"size\": ", nNumOfResultsToReturn, ", \"ids\": ["));
                        for (int i = 0; i < listLength; i++)
                        {
                            if (i != 0)
                                sb.Append(",");
                            sb.Append(String.Concat("\"", oIDsList[i].ToString(), "\""));
                        }
                        sb.Append("] }");
                        break;
                    }
                case "string":
                    {
                        bool isFirstIDWritten = false;
                        sb = new StringBuilder(String.Concat("{\"size\": ", nNumOfResultsToReturn, ", \"ids\": ["));
                        for (int i = 0; i < listLength; i++)
                        {
                            long temp = 0;
                            if (Int64.TryParse(oIDsList[i].ToString(), out temp))
                            {
                                if (isFirstIDWritten)
                                    sb.Append(",");
                                sb.Append(String.Concat("\"", oIDsList[i].ToString(), "\""));
                                isFirstIDWritten = true;
                            }
                        }
                        sb.Append("] }");
                        break;
                    }
                default:
                    {
                        sb = new StringBuilder();
                        break;
                    }

            }

            res = sb.ToString();
            return res;
        }

        #endregion

        #region HTTP requests

        public string SendPutHttpRequest(string url, string body)
        {
            int status = 0;
            return SendPostHttpReq(url, ref status, string.Empty, string.Empty, body, true, true);
        }

        public string SendPostHttpReq(string url, ref int status, string userName, string password, string parameters, bool isFirstTry, bool isPut = false)
        {
            StringContent strContent = new StringContent(parameters, Encoding.UTF8, "application/json");
            string result = string.Empty;
            string requestGuid = Guid.NewGuid().ToString();

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_ELASTIC, null, null, null, null)
                {
                    Database = url,
                    Table = requestGuid
                })
                {
                    HttpResponseMessage response = null;

                    {
                        if (!isPut)
                        {
                            response = httpClient.PostAsync(url, strContent).ExecuteAndWait();
                        }
                        else
                        {
                            response = httpClient.PutAsync(url, strContent).ExecuteAndWait();
                        }
                    }

                    status = GetResponseCode(response.StatusCode);
                    result = response.Content.ReadAsStringAsync().ExecuteAndWait();
                }

                log.Debug($"ElasticSearch API post request: guid = {requestGuid}, url = {url}, parameters = {parameters}, " +
                    $"body length = {parameters.Length}, response = {result}");
            }
            catch (WebException ex)
            {
                StreamReader errorStream = null;
                try
                {
                    errorStream = new StreamReader(ex.Response.GetResponseStream());
                    result = errorStream.ReadToEnd();
                }
                finally
                {
                    if (errorStream != null) errorStream.Close();
                }

                log.Error($"ElasticSearch API post request error: guid = {requestGuid}, url = {url}, parameters = " +
                    $"{parameters}, body length = {parameters.Length}, response = {result}\nex = {ex}");
            }
            catch (Exception ex)
            {
                log.Error("Error in SendPostHttpReq Exception", ex);
            }

            //retry alternative URL if this is the original (=first) call, the result was not OK and there is an alternative URL


            return result;
        }

        public string SendGetHttpReq(string url, ref int status, bool isFirstTry = true)
        {
            string result = string.Empty;
            string requestGuid = Guid.NewGuid().ToString();

            try
            {

                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_ELASTIC, null, null, null, null)
                {
                    Database = url,
                    Table = requestGuid
                })
                {
                    var response = httpClient.GetAsync(url).ExecuteAndWait();
                        
                    status = GetResponseCode(response.StatusCode);
                    result = response.Content.ReadAsStringAsync().ExecuteAndWait();
                }
            }
            catch (WebException ex)
            {
                StreamReader errorStream = null;
                try
                {
                    errorStream = new StreamReader(ex.Response.GetResponseStream());
                    result = errorStream.ReadToEnd();
                }
                finally
                {
                    if (errorStream != null) errorStream.Close();
                }
            }
            catch (Exception ex)
            {
                log.Error("Error in SendPostHttpReq Exception", ex);
            }

            return result;
        }

        public string SendDeleteHttpReq(string url, ref Int32 status, string userName, string password, string parameters, bool isFirstTry)
        {
            StringContent strContent = new StringContent(parameters, Encoding.UTF8, "application/json");
            string result = string.Empty;
            string requestGuid = Guid.NewGuid().ToString();

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_ELASTIC, null, null, null, null)
                {
                    Database = url,
                    Table = requestGuid
                })
                {
                    HttpRequestMessage request = new HttpRequestMessage
                    {
                        Content = strContent,
                        Method = HttpMethod.Delete,
                        RequestUri = new Uri(url)
                    };

                    HttpResponseMessage response = httpClient.SendAsync(request).ExecuteAndWait();
                        
                    status = GetResponseCode(response.StatusCode);
                    result = response.Content.ReadAsStringAsync().ExecuteAndWait();
                }

                log.Debug($"ElasticSearch API delete request: guid = {requestGuid}, url = {url}, parameters = {parameters}, " +
                    $"body length = {parameters.Length}, response = {result}");
            }
            catch (WebException ex)
            {
                StreamReader errorStream = null;
                try
                {
                    errorStream = new StreamReader(ex.Response.GetResponseStream());
                    result = errorStream.ReadToEnd();
                }
                finally
                {
                    if (errorStream != null) errorStream.Close();
                }

                log.Error($"ElasticSearch API post request error: guid = {requestGuid}, url = {url}, parameters = " +
                    $"{parameters}, body length = {parameters.Length}, response = {result}\nex = {ex}");
            }
            catch (Exception ex)
            {
                log.Error("Error in SendPostHttpReq Exception", ex);
            }


            return result;
        }

        public Int32 GetResponseCode(HttpStatusCode theCode)
        {
            if (theCode == HttpStatusCode.OK || theCode == HttpStatusCode.Created || theCode == HttpStatusCode.Accepted)
                return (int)HttpStatusCode.OK;
            if (theCode == HttpStatusCode.NotFound)
                return (int)HttpStatusCode.NotFound;
            return (int)HttpStatusCode.InternalServerError;
        }

        #endregion

        #region HealthCheck
        public bool HealthCheck()
        {
            bool result = false;
            try
            {
                var url = $"{baseUrl}/{"_cluster/health"}";
                var status = 0;
                var ret = SendGetHttpReq(url, ref status);
                log.Info($"Elasticsearch HealthCheck > response:[{ret}]");

                if (status != 200)
                {
                    return false;
                }

                var json = JObject.Parse(ret);

                if (json != null)
                {
                    var healthStatus = json["status"];

                    if (healthStatus != null)
                    {
                        var healthStatusValue = healthStatus.Value<string>().ToLower();

                        if (healthStatusValue == "yellow" || healthStatusValue == "green")
                        {
                            result = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error($"Error when checking ElasticSearch healthcheck. ex = {ex}");
                result = false;
            }

            return result;
        }
        #endregion

        public class ESAssetDocument
        {
            public string index
            {
                get;
                set;
            }
            public string type
            {
                get;
                set;
            }
            public string id
            {
                get;
                set;
            }
            public int asset_id
            {
                get;
                set;
            }
            public int group_id
            {
                get;
                set;
            }
            public double score
            {
                get;
                set;
            }
            public string name
            {
                get;
                set;
            }
            public DateTime cache_date
            {
                get;
                set;
            }
            public DateTime update_date
            {
                get;
                set;
            }
            public int epg_channel_id
            {
                get;
                set;
            }
            public DateTime start_date
            {
                get;
                set;
            }
            public int media_type_id
            {
                get;
                set;
            }

            public string epg_identifier
            {
                get;
                set;
            }
            public DateTime end_date
            {
                get;
                set;
            }
            public Dictionary<string, string> extraReturnFields
            {
                get;
                set;
            }
        }

        public class ESSearchResponse
        {
            public int totalNumOfItems { get; set; }
            public List<ElasticSearchApi.ESAssetDocument> documents { get; set; }
        }
    }
}
