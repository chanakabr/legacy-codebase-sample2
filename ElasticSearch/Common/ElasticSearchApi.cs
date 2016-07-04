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

namespace ElasticSearch.Common
{
    public class ElasticSearchApi
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public static readonly string ES_URL = Common.Utils.GetWSURL("ES_URL");
        public static readonly string ALT_ES_URL = Common.Utils.GetWSURL("ALT_ES_URL");
        private const string ES_LOG_FILENAME = "Elasticsearch";

        public string baseUrl
        {
            get;
            set;
        }

        public ElasticSearchApi(string elaticSearchUrl = null)
        {
            if (string.IsNullOrEmpty(elaticSearchUrl))
            {
                baseUrl = ES_URL;
            }
            else
            {
                baseUrl = elaticSearchUrl;
            }
        }
        
        public string GetDoc(string sIndex, string sType, string sDocId)
        {
            string sRes = string.Empty;

            if (string.IsNullOrEmpty(sIndex) || string.IsNullOrEmpty(sType))
                return sRes;

            string sUrl = string.Format("{0}/{1}/{2}/{3}", baseUrl, sIndex, sType, sDocId);
            int nStatus = 0;

            sRes = SendGetHttpReq(sUrl, ref nStatus, string.Empty, string.Empty, true);

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

            sRes = SendGetHttpReq(sUrl, ref nStatus, string.Empty, string.Empty, true);

            if (nStatus != 200)
            {
                log.Error("Error - " + string.Format("Get record failed. url={0};docID={1}", sUrl, sDocId));
                sRes = string.Empty;
            }

            return sRes;
        }

        public bool BuildIndex(string sIndex, int nShards, int nReplicas,
            List<string> lAnalyzers, List<string> lFilters, List<string> tokenizers = null)
        {
            bool bRes = false;

            if (string.IsNullOrEmpty(sIndex))
                return bRes;

            StringBuilder sBuildIndex = new StringBuilder();

            sBuildIndex.Append(@"{ ""settings"": {");

            bool bShards = false;
            if (nShards > 0 && nReplicas > 0)
            {
                bShards = true;
                sBuildIndex.Append(@"""index"": {");
                sBuildIndex.AppendFormat(" \"number_of_shards\": {0}, \"number_of_replicas\": {1}", nShards, nReplicas);
                sBuildIndex.Append("} ");
            }

            #region add analyzers/filters/tokenizers

            if (bShards)
                sBuildIndex.Append(",");

            sBuildIndex.Append("\"analysis\": {");
            bool bAnalyzer = false;
            if (lAnalyzers != null && lAnalyzers.Count > 0)
            {
                bAnalyzer = true;
                sBuildIndex.Append("\"analyzer\":{");
                sBuildIndex.Append(string.Join(",", lAnalyzers));
                sBuildIndex.Append("}");
            }

            bool hasFilter = false;

            if (lFilters != null && lFilters.Count > 0)
            {
                if (bAnalyzer)
                    sBuildIndex.Append(",");

                sBuildIndex.Append("\"filter\":{");
                sBuildIndex.Append(string.Join(",", lFilters));
                sBuildIndex.Append("}");

                hasFilter = true;
            }

            if (tokenizers != null && tokenizers.Count > 0)
            {
                if (bAnalyzer || hasFilter)
                {
                    sBuildIndex.Append(",");
                }

                sBuildIndex.Append("\"tokenizer\":{");
                sBuildIndex.Append(string.Join(",", tokenizers));
                sBuildIndex.Append("}");
            }

            sBuildIndex.Append("}");
            #endregion

            sBuildIndex.Append("} }");

            string sUrl = string.Format("{0}/{1}", baseUrl, sIndex);
            int nStatus = 0;

            string sResponse = SendPostHttpReq(sUrl, ref nStatus, string.Empty, string.Empty, sBuildIndex.ToString(), true);

            bRes = (nStatus == 200) ? true : false;

            if (!bRes)
            {
                log.ErrorFormat("Error when building index {0}. Response is: {1}", sIndex, sResponse);
            }

            return bRes;
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

        public bool SwitchIndex(string sIndex, string sAlias, List<string> lIndicesForRemoval, string sSearchRouting = null)
        {
            bool bResult = false;

            if (!string.IsNullOrEmpty(sIndex) && !string.IsNullOrEmpty(sAlias))
            {
                StringBuilder sActionRequest = new StringBuilder();
                sActionRequest.Append("{ \"actions\": [");

                if (lIndicesForRemoval != null)
                {
                    foreach (string sOldIndex in lIndicesForRemoval)
                    {
                        sActionRequest.Append(@" { ""remove"": { ");
                        sActionRequest.AppendFormat(" \"alias\": \"{0}\", \"index\": \"{1}\"", sAlias, sOldIndex);
                        sActionRequest.Append(" } },");
                    }
                }
                sActionRequest.Append(@" { ""add"": { ");
                sActionRequest.AppendFormat(" \"alias\": \"{0}\", \"index\": \"{1}\"", sAlias, sIndex);

                if (!string.IsNullOrEmpty(sSearchRouting))
                {
                    sActionRequest.AppendFormat(", \"routing\":\"{0}\"", sSearchRouting);
                }

                sActionRequest.Append(" } } ] }");

                string sUrl = string.Format("{0}/_aliases", baseUrl);
                int nStatus = 0;

                string sRetVal = SendPostHttpReq(sUrl, ref nStatus, string.Empty, string.Empty, sActionRequest.ToString(), true);

                bResult = (nStatus == 200) ? true : false;

                if (bResult == false)
                    log.Error("Error - " + string.Format("error received when trying to switch indices. Message: {0}", sRetVal));
            }

            return bResult;
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

            deleteResult = ESDeleteResult.GetDeleteResult(sRetVal);

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
                log.Debug("Status - " + string.Format("DeleteDocsByQuery. Returned JSON from ES: ", sResult, " Query: ", sQuery));
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

        public List<string> GetAliases(string sIndex)
        {
            List<string> aliases = new List<string>();

            if (!string.IsNullOrEmpty(sIndex))
            {
                string url = string.Format("{0}/{1}/_aliases", baseUrl, sIndex);
                int status = 0;

                string httpResponse = SendGetHttpReq(url, ref status, string.Empty, string.Empty, true);

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

        public bool IndexExists(string sIndex)
        {
            bool bRes = false;

            if (!string.IsNullOrEmpty(sIndex))
            {

                string sUrl = string.Format("{0}/{1}/_settings", baseUrl, sIndex);
                int nStatus = 0;
                string sResponse = SendGetHttpReq(sUrl, ref nStatus, string.Empty, string.Empty, true);

                bRes = nStatus == 200;
            }

            return bRes;
        }

        public bool MappingExists(string sIndex, string sType)
        {
            bool bRes = false;

            if (!string.IsNullOrEmpty(sIndex) && !string.IsNullOrEmpty(sType))
            {

                string sUrl = string.Format("{0}/{1}/{2}/_mapping", baseUrl, sIndex, sType);
                int nStatus = 0;
                string sResponse = SendGetHttpReq(sUrl, ref nStatus, string.Empty, string.Empty, true);

                bRes = (nStatus == 200) ? true : false;
            }

            return bRes;
        }

        public string GetAllMappings(string sIndex)
        {
            string sRes = string.Empty;

            if (!string.IsNullOrEmpty(sIndex))
            {
                string sUrl = string.Format("{0}/{1}/_mapping", baseUrl, sIndex);
                int nStatus = 0;
                string sResponse = SendGetHttpReq(sUrl, ref nStatus, string.Empty, string.Empty, true);

                sRes = (nStatus == 200) ? sResponse : sRes;
            }

            return sRes;
        }

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

        public List<ESBulkRequestObj<T>> CreateBulkIndexRequest<T>(List<ESBulkRequestObj<T>> lBulkRequest)
        {
            log.Debug("STart ES Update - Start Bulk Update ");
            StringBuilder sBulkRequest = new StringBuilder();
            List<ESBulkRequestObj<T>> sInvalidRecords = new List<ESBulkRequestObj<T>>();


            if (lBulkRequest != null)
            {
                foreach (var bulkObj in lBulkRequest)
                {
                    sBulkRequest.Append("{ \"");
                    sBulkRequest.Append(bulkObj.Operation.ToString());
                    sBulkRequest.Append("\": { ");


                    sBulkRequest.AppendFormat("\"_index\": \"{0}\"", bulkObj.index);
                    sBulkRequest.AppendFormat(", \"_type\": \"{0}\"", bulkObj.type);

                    if (!string.IsNullOrEmpty(bulkObj.routing))
                    {
                        sBulkRequest.AppendFormat(", \"_routing\": \"{0}\"", bulkObj.routing);
                    }
                    sBulkRequest.AppendFormat(",\"_id\" : \"{0}\"", bulkObj.docID);

                    sBulkRequest.Append(" } }\n");

                    if (!string.IsNullOrEmpty(bulkObj.document))
                    {
                        sBulkRequest.AppendFormat("{0}\n", bulkObj.document);
                    }
                }
            }

            string sUrl = string.Format("{0}/_bulk", baseUrl);
            int nStatus = 0;
            string sParams = sBulkRequest.ToString();
            string sRetVal = SendPostHttpReq(sUrl, ref nStatus, string.Empty, string.Empty, sParams, true);
            log.Debug("Finish ES Update - " + sRetVal);
            //Will need to add treatment on objects that returned with an "ok": false

            return sInvalidRecords;
        }

        public List<KeyValuePair<T, string>> CreateBulkIndexRequest<T>(string sIndex, string sType, List<KeyValuePair<T, string>> lObjects, string sRouting = null)
        {
            log.Debug("Start ES Update - Start Bulk Update");

            StringBuilder sBulkRequest = new StringBuilder();
            List<KeyValuePair<T, string>> sInvalidRecords = new List<KeyValuePair<T, string>>();

            if (lObjects != null)
            {
                foreach (KeyValuePair<T, string> sObj in lObjects)
                {
                    sBulkRequest.Append("{ \"index\": { ");

                    sBulkRequest.AppendFormat("\"_index\": \"{0}\"", sIndex);
                    sBulkRequest.AppendFormat(", \"_type\": \"{0}\"", sType);

                    if (!string.IsNullOrEmpty(sRouting))
                    {
                        sBulkRequest.AppendFormat(", \"_routing\": \"{0}\"", sRouting);
                    }

                    sBulkRequest.AppendFormat(",\"_id\" : \"{0}\"", sObj.Key);

                    sBulkRequest.Append(" } }\n");
                    sBulkRequest.AppendFormat("{0}\n", sObj.Value);
                }
            }

            string sUrl = string.Format("{0}/_bulk", baseUrl);
            int nStatus = 0;
            string sParams = sBulkRequest.ToString();
            string sRetVal = SendPostHttpReq(sUrl, ref nStatus, string.Empty, string.Empty, sParams, true);
            log.Debug("Finish ElasticSearch Bulk Request - " + sRetVal);
            //Will need to add treatment on objects that returned with an "ok": false

            return sInvalidRecords;
        }

        public string Search(string sIndex, string sType, ref string sSearchQuery, List<string> routing = null)
        {
            string sRes = string.Empty;

            if (string.IsNullOrEmpty(sIndex) || string.IsNullOrEmpty(sType) || string.IsNullOrEmpty(sSearchQuery))
                return sRes;

            string sUrl;
            if (routing != null && routing.Count > 0)
            {
                string sRouting = routing.Aggregate((current, next) => current + "," + next);
                sUrl = string.Format("{0}/{1}/{2}/_search?routing={3}", baseUrl, sIndex, sType, sRouting);
            }
            else
            {
                sUrl = string.Format("{0}/{1}/{2}/_search", baseUrl, sIndex, sType);
            }
            int nStatus = 0;

            sRes = SendPostHttpReq(sUrl, ref nStatus, string.Empty, string.Empty, sSearchQuery, true);

            log.DebugFormat("ES API request: URL = {0}, body = {1}, result = {2}", sUrl, sSearchQuery, sRes);

            if (nStatus != 200)
            {
                log.Error("Error - " + string.Format("Search query failed. url={0};query={1}; explanation={2}", sUrl, sSearchQuery, sRes));
                sRes = string.Empty;
            }

            return sRes;
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
            log.DebugFormat("ES request: URL = {0}, body = {1}, result = {2}", sUrl, sb.ToString(), sRes);
            

            if (nStatus != 200)
            {
                log.Error("Error - " + string.Format("Search query failed. url={0};query={1}; Explanation={2}", sUrl, sb.ToString()));
                sRes = string.Empty;
            }

            return sRes;
        }

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

        protected static Dictionary<string, string> dESAnalyzers = new Dictionary<string, string>();
        protected static Dictionary<string, string> dESFilters = new Dictionary<string, string>();
        protected static Dictionary<string, string> tokenizers = new Dictionary<string, string>();

        public static string GetAnalyzerDefinition(string sAnalyzerName)
        {
            string analyzer;

            if (!dESAnalyzers.TryGetValue(sAnalyzerName, out analyzer))
            {

                analyzer = Utils.GetWSURL(sAnalyzerName);
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
                filter = Utils.GetWSURL(sFilterName);
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
                tokenizer = Utils.GetWSURL(tokenizerName);

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
        
        #region HTTP requests
        public string SendPostHttpReq(string sUrl, ref int nStatus, string sUserName, string sPassword, string sParams, bool isFirstTry)
        {
            Int32 nStatusCode = -1;

            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(sUrl);
            webRequest.ContentType = "application/x-www-form-urlencoded";
            webRequest.Method = "POST";
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(sParams);
            webRequest.ContentLength = bytes.Length;
            using (System.IO.Stream os = webRequest.GetRequestStream())
            {
                os.Write(bytes, 0, bytes.Length);
            }

            string res = string.Empty;
            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_ELASTIC, null, null, null, null) { Database = sUrl })
                {
                HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse();
                HttpStatusCode sCode = webResponse.StatusCode;
                nStatusCode = GetResponseCode(sCode);
                StreamReader sr = null;
                try
                {
                    sr = new StreamReader(webResponse.GetResponseStream());
                    res = sr.ReadToEnd();
                }
                finally
                {
                    if (sr != null)
                        sr.Close();
                }
                }
            }
            catch (WebException ex)
            {
                log.Error("Error in SendPostHttpReq WebException", ex);
                StreamReader errorStream = null;
                try
                {
                    errorStream = new StreamReader(ex.Response.GetResponseStream());
                    res = errorStream.ReadToEnd();
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

            //retry alternative URL if this is the original (=first) call, the result was not OK and there is an alternative URL
            if (isFirstTry && nStatusCode != 200 && !string.IsNullOrEmpty(ALT_ES_URL))
            {
                string sAlternativeURL = sUrl.Replace(ES_URL, ALT_ES_URL);
                res = SendPostHttpReq(sAlternativeURL, ref nStatus, sUserName, sPassword, sParams, false);
            }

            nStatus = nStatusCode;
            return res;
        }

        public string SendGetHttpReq(string sUrl, ref Int32 nStatus, string sUserName, string sPassword, bool isFirstTry)
        {
            HttpWebRequest oWebRequest = (HttpWebRequest)WebRequest.Create(sUrl);
            HttpWebResponse oWebResponse = null;
            Stream receiveStream = null;
            Int32 nStatusCode = -1;
            Encoding enc = new UTF8Encoding(false);
            try
            {
                oWebRequest.Credentials = new NetworkCredential(sUserName, sPassword);
                oWebRequest.Timeout = 1000000;
                oWebResponse = (HttpWebResponse)oWebRequest.GetResponse();
                HttpStatusCode sCode = oWebResponse.StatusCode;
                nStatusCode = GetResponseCode(sCode);
                receiveStream = oWebResponse.GetResponseStream();

                StreamReader sr = new StreamReader(receiveStream, enc);
                string resultString = sr.ReadToEnd();

                sr.Close();

                oWebResponse.Close();
                oWebRequest = null;
                oWebResponse = null;

                //retry alternative URL if this is the original (=first) call, the result was not OK and there is an alternative URL
                if (isFirstTry && nStatusCode != 200 && !string.IsNullOrEmpty(ALT_ES_URL))
                {
                    string sAlternativeURL = sUrl.Replace(ES_URL, ALT_ES_URL);
                    resultString = SendGetHttpReq(sAlternativeURL, ref nStatus, sUserName, sPassword, false);
                }

                nStatus = nStatusCode;
                return resultString;
            }
            catch (Exception ex)
            {
                log.Debug("Notifier - SendGetHttpReq exception:" + ex.Message + " to: " + sUrl);
                if (oWebResponse != null)
                    oWebResponse.Close();
                if (receiveStream != null)
                    receiveStream.Close();
                nStatus = 404;
                return ex.Message;
            }
        }

        public string SendDeleteHttpReq(string sUrl, ref Int32 nStatus, string sUserName, string sPassword, string sParams, bool isFirstTry)
        {
            Int32 nStatusCode = -1;

            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(sUrl);
            webRequest.ContentType = "application/x-www-form-urlencoded";
            webRequest.Method = "DELETE";
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(sParams);
            webRequest.ContentLength = bytes.Length;
            System.IO.Stream os = webRequest.GetRequestStream();
            os.Write(bytes, 0, bytes.Length);
            os.Close();

            string res = string.Empty;
            try
            {
                HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse();
                HttpStatusCode sCode = webResponse.StatusCode;
                nStatusCode = GetResponseCode(sCode);
                StreamReader sr = null;
                try
                {
                    sr = new StreamReader(webResponse.GetResponseStream());
                    res = sr.ReadToEnd();
                }
                finally
                {
                    if (sr != null)
                        sr.Close();
                }

            }
            catch (WebException ex)
            {
                StreamReader errorStream = null;
                try
                {
                    errorStream = new StreamReader(ex.Response.GetResponseStream());
                    res = errorStream.ReadToEnd();
                }
                finally
                {
                    if (errorStream != null) errorStream.Close();
                }
            }

            //retry alternative URL if this is the original (=first) call, the result was not OK and there is an alternative URL
            if (isFirstTry && nStatusCode != 200 && !string.IsNullOrEmpty(ALT_ES_URL))
            {
                string sAlternativeURL = sUrl.Replace(ES_URL, ALT_ES_URL);
                res = SendDeleteHttpReq(sAlternativeURL, ref nStatus, sUserName, sPassword, sParams, false);
            }

            nStatus = nStatusCode;
            return res;
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
            //public double score { get; set; }
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
        }

        public class ESSearchResponse
        {
            public int totalNumOfItems { get; set; }
            public List<ElasticSearchApi.ESAssetDocument> documents { get; set; }
        }
    }
}
