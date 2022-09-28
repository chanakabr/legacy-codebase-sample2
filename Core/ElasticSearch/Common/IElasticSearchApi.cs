using ElasticSearch.Common.DeleteResults;
using ElasticSearch.Searcher;
using System;
using System.Collections.Generic;
using System.Net;

namespace ElasticSearch.Common
{
    public interface IElasticSearchApi
    {
        string baseUrl { get; set; }

        bool AddAlias(string index, string alias);
        bool AddQueryToPercolator(string sIndex, string sQueryName, ref string sQuery);
        bool AddQueryToPercolatorV2(string index, string queryId, ref string queryBody);
        bool BuildIndex(string index, int shards, int replicas, List<string> analyzers, List<string> filters, List<string> tokenizers = null, int maxResultWindow = 0, string refreshInterval = null);
        List<KeyValuePair<string, string>> CreateBulkIndexRequest<T>(string index, string type, List<KeyValuePair<T, string>> objects, string routing = null);
        List<KeyValuePair<string, string>> CreateBulkRequest<T>(List<ESBulkRequestObj<T>> bulkRequests);
        bool CreateBulkRequests<T>(List<ESBulkRequestObj<T>> bulkRequests, out List<ESBulkRequestObj<T>> invalidRecords);
        bool DecrementField(string index, string type, string docId, string field);
        ESDeleteResult DeleteDoc(string sIndex, string sType, string sId);
        bool DeleteDocsByQuery(string sIndex, string sType, ref string sQuery);
        bool DeleteDocsByQuery(string sIndex, string sType, ref string sQuery, out int countDeleted);
        bool DeleteIndices(List<string> lIndices);
        bool ForceRefresh(string index);
        List<string> GetAliases(string sIndex);
        string GetAllMappings(string sIndex);
        IEnumerable<string> GetMappingsNames(string indexName);
        string GetDoc(string sIndex, string sType, string sDocId);
        string GetDoc(string sIndex, string sType, string sDocId, string routing);
        int GetResponseCode(HttpStatusCode theCode);
        bool HealthCheck();
        bool IncrementField(string index, string type, string docId, string field);
        bool IndexExists(string sIndex);
        bool InsertMapping(string sIndex, string sMapName, string sMappingObject);
        bool InsertRecord(string sIndex, string sType, string sID, string sDoc, string sRouting = null);
        List<ESIndex> ListIndices(string indexQueryPattern = "*");
        List<ESIndex> ListIndicesByAlias(string aliasQueryPattern = "*");
        bool MappingExists(string sIndex, string sType);
        string MultiGetIDs<T>(string sIndex, string sType, List<T> oIDsList, int nNumOfResultsToReturn);
        string MultiSearch(string sIndex, string sType, List<string> lSearchQueries, List<string> lRouting);
        bool PartialUpdate(string index, string type, string docId, string partialUpdate);
        bool Reindex(string source, string destination, string filterQuery = null, string scriptFileName = null, int? batchSize = null);
        bool RemoveAlias(string index, string alias);
        string Search(string index, string indexType, ref string searchQuery, List<string> routing = null, string preference = null);
        List<string> SearchPercolator(string sIndex, string sType, ref string sDoc);
        Tuple<string, int> SearchWithStatus(string index, string indexType, ref string searchQuery, List<string> routing = null, string preference = null);
        string SendDeleteHttpReq(string url, ref int status, string userName, string password, string parameters, bool isFirstTry);
        string SendGetHttpReq(string url, ref int status, bool isFirstTry = true);
        string SendPostHttpReq(string url, ref int status, string userName, string password, string parameters, bool isFirstTry, bool isPut = false);
        string SendPutHttpRequest(string url, string body);
        bool SwitchIndex(string index, string alias, List<string> indicesForRemoval, string sSearchRouting = null);
        bool UpdateIndexRefreshInterval(string index, string refreshInterval);
    }
}