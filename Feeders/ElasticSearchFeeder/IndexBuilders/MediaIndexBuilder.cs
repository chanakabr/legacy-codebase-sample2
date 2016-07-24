using ApiObjects.SearchObjects;
using Catalog;
using ElasticSearch.Common;
using ElasticSearch.Searcher;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catalog.Cache;
using GroupsCacheManager;
using KLogMonitor;
using System.Reflection;

namespace ElasticSearchFeeder.IndexBuilders
{
    public class MediaIndexBuilder : AbstractIndexBuilder
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        protected ElasticSearchApi m_oESApi;
        protected int m_nGroupID;
        protected ESSerializer m_oESSerializer;

        public MediaIndexBuilder(int nGroupID)
        {
            m_nGroupID = nGroupID;
            bSwitchIndex = false;
            bSwitchIndex = false;
            m_oESApi = new ElasticSearchApi();
            m_oESSerializer = new ESSerializer();
        }

        public override async Task<bool> BuildIndex()
        {
            string sNewIndex = Utils.GetNewMediaIndexStr(m_nGroupID);

            #region Build new index and specify number of nodes/shards

            string sNumOfShards = Utils.GetWSURL("ES_NUM_OF_SHARDS");
            string sNumOfReplicas = Utils.GetWSURL("ES_NUM_OF_REPLICAS");

            int nNumOfShards, nNumOfReplicas;

            int.TryParse(sNumOfReplicas, out nNumOfReplicas);
            int.TryParse(sNumOfShards, out nNumOfShards);

            GroupManager groupManager = new GroupManager();
            bool bres = groupManager.RemoveGroup(m_nGroupID);
            Group oGroup = groupManager.GetGroup(m_nGroupID);

            if (oGroup == null)
            {
                log.Error("Error - Could not load group in media index builder. ESFeeder");
                return false;
            }

            List<string> lAnalyzers;
            List<string> lFilters;
            List<string> tokenizers;
            GetAnalyzers(oGroup.GetLangauges(), out lAnalyzers, out lFilters, out tokenizers);

            bool bRes = m_oESApi.BuildIndex(sNewIndex, nNumOfShards, nNumOfReplicas, lAnalyzers, lFilters, tokenizers);

            #endregion

            #region create mapping
            foreach (ApiObjects.LanguageObj language in oGroup.GetLangauges())
            {
                string indexAnalyzer, searchAnalyzer;
                string autocompleteIndexAnalyzer = null;
                string autocompleteSearchAnalyzer = null;

                string analyzerDefinitionName = ElasticSearch.Common.Utils.GetLangCodeAnalyzerKey(language.Code, string.Empty);

                if (ElasticSearchApi.AnalyzerExists(analyzerDefinitionName))
                {
                    indexAnalyzer = string.Concat(language.Code, "_index_", "analyzer");
                    searchAnalyzer = string.Concat(language.Code, "_search_", "analyzer");

                    if (ElasticSearchApi.GetAnalyzerDefinition(analyzerDefinitionName).Contains("autocomplete"))
                    {
                        autocompleteIndexAnalyzer = string.Concat(language.Code, "_autocomplete_analyzer");
                        autocompleteSearchAnalyzer = string.Concat(language.Code, "_autocomplete_search_analyzer");
                    }
                }
                else
                {
                    indexAnalyzer = "whitespace";
                    searchAnalyzer = "whitespace";
                    log.Error("Error - " + string.Format("could not find analyzer for language ({0}) for mapping. whitespace analyzer will be used instead", language.Code) + " ElasticSearch");
                }

                string sMapping = m_oESSerializer.CreateMediaMapping(oGroup.m_oMetasValuesByGroupId, oGroup.m_oGroupTags, indexAnalyzer, searchAnalyzer, autocompleteIndexAnalyzer, autocompleteSearchAnalyzer);
                string sType = (language.IsDefault) ? MEDIA : string.Concat(MEDIA, "_", language.Code);
                bool bMappingRes = m_oESApi.InsertMapping(sNewIndex, sType, sMapping.ToString());

                if (language.IsDefault && !bMappingRes)
                    bRes = false;

                if (!bMappingRes)
                    log.Error("Error - " + string.Concat("Could not create mapping of type media for language ", language.Name) + " ESFeeder");

            }

            if (!bRes)
                return bRes;
            #endregion

            #region insert medias
            Dictionary<int, Dictionary<int, Media>> dGroupMedias = await GetGroupMedias(m_nGroupID, 0);

            if (dGroupMedias != null)
            {
                log.Debug("Info - " + string.Format("Start indexing medias. total medias={0}", dGroupMedias.Count) + " ESFeeder");
                List<ESBulkRequestObj<int>> lBulkObj = new List<ESBulkRequestObj<int>>();

                foreach (int nMediaID in dGroupMedias.Keys)
                {
                    foreach (int nLangID in dGroupMedias[nMediaID].Keys)
                    {
                        Media oMedia = dGroupMedias[nMediaID][nLangID];

                        if (oMedia != null)
                        {
                            string sMediaObj;

                            sMediaObj = m_oESSerializer.SerializeMediaObject(oMedia);

                            string sType = Utils.GetTanslationType(MEDIA, oGroup.GetLanguage(nLangID));

                            lBulkObj.Add(new ESBulkRequestObj<int>() { docID = oMedia.m_nMediaID, index = sNewIndex, type = sType, document = sMediaObj });
                        }
                        if (lBulkObj.Count >= 50)
                        {
                            Task t = Task.Factory.StartNew(() =>  m_oESApi.CreateBulkRequest(lBulkObj));
                            t.Wait();
                            lBulkObj = new List<ESBulkRequestObj<int>>();
                        }
                    }
                }

                if (lBulkObj.Count > 0)
                {
                    Task t = Task.Factory.StartNew(() => m_oESApi.CreateBulkRequest(lBulkObj));
                    t.Wait();
                }
            }
            #endregion

            #region insert channel queries

            if (oGroup.channelIDs != null)
            {
                log.Debug("Info - " + string.Format("Start indexing channels. total channels={0}", oGroup.channelIDs.Count) + " ESFeeder");

                MediaSearchObj oSearchObj;
                string sQueryStr;
                ESMediaQueryBuilder oQueryParser = new ESMediaQueryBuilder() { QueryType = eQueryType.EXACT };

                List<KeyValuePair<int, string>> lChannelRequests = new List<KeyValuePair<int, string>>();
                try
                {
                    List<Channel> allChannels = groupManager.GetChannels(oGroup.channelIDs.ToList(), m_nGroupID);

                    foreach (Channel currentChannel in allChannels)
                    {
                        if (currentChannel == null || currentChannel.m_nIsActive != 1)
                            continue;

                        oQueryParser.m_nGroupID = currentChannel.m_nGroupID;
                        oSearchObj = BuildBaseChannelSearchObject(currentChannel);
                        oQueryParser.oSearchObject = oSearchObj;
                        sQueryStr = oQueryParser.BuildSearchQueryString(false);

                        lChannelRequests.Add(new KeyValuePair<int, string>(currentChannel.m_nChannelID, sQueryStr));

                        if (lChannelRequests.Count > 50)
                        {
                            m_oESApi.CreateBulkIndexRequest("_percolator", sNewIndex, lChannelRequests);
                            lChannelRequests.Clear();
                        }
                    }

                    if (lChannelRequests.Count > 0)
                    {
                        m_oESApi.CreateBulkIndexRequest("_percolator", sNewIndex, lChannelRequests);
                    }
                }
                catch (Exception ex)
                {
                    log.Error("Error " + string.Format("Caught exception while indexing channels. Ex={0};Stack={1}", ex.Message, ex.StackTrace) + " ESFeeder", ex);
                }
            }

            #endregion

            string sAlias = Utils.GetMediaGroupAliasStr(m_nGroupID);
            bool indexExists = m_oESApi.IndexExists(sAlias);

            if (bSwitchIndex || !indexExists)
            {
                List<string> lOldIndices = m_oESApi.GetAliases(sAlias);

                Task<bool> tSwitchIndex = Task<bool>.Factory.StartNew(() => m_oESApi.SwitchIndex(sNewIndex, sAlias, lOldIndices));
                tSwitchIndex.Wait();

                if (tSwitchIndex.Result && lOldIndices.Count > 0)
                {
                    Task t = Task.Factory.StartNew(() => m_oESApi.DeleteIndices(lOldIndices));
                    t.Wait();
                }
            }

            return true;
        }

        private void GetAnalyzers(List<ApiObjects.LanguageObj> lLanguages, out List<string> lAnalyzers, out List<string> lFilters, out List<string> tokenizers)
        {
            lAnalyzers = new List<string>();
            lFilters = new List<string>();
            tokenizers = new List<string>();

            if (lLanguages != null)
            {
                foreach (ApiObjects.LanguageObj language in lLanguages)
                {
                    string analyzer = ElasticSearchApi.GetAnalyzerDefinition(ElasticSearch.Common.Utils.GetLangCodeAnalyzerKey(language.Code, string.Empty));
                    string filter = ElasticSearchApi.GetFilterDefinition(ElasticSearch.Common.Utils.GetLangCodeFilterKey(language.Code, string.Empty));
                    string tokenizer = ElasticSearchApi.GetTokenizerDefinition(ElasticSearch.Common.Utils.GetLangCodeTokenizerKey(language.Code, string.Empty));

                    if (string.IsNullOrEmpty(analyzer))
                    {
                        log.Error("Error - " + string.Format("analyzer for language {0} doesn't exist", language.Code));
                    }
                    else
                    {
                        lAnalyzers.Add(analyzer);
                    }

                    if (!string.IsNullOrEmpty(filter))
                    {
                        lFilters.Add(filter);
                    }

                    if (!string.IsNullOrEmpty(tokenizer))
                    {
                        tokenizers.Add(tokenizer);
                    }
                }
            }
        }

        public static async Task<Dictionary<int, Dictionary<int, Media>>> GetGroupMedias(int nGroupID, int nMediaID)
        {

            //dictionary contains medias such that first key is media_id, which returns a dictionary with a key language_id and value Media object.
            //E.g. dMedias[123][2] --> will return media 123 of the Hebrew language
            Dictionary<int, Dictionary<int, Media>> dMediaTrans = new Dictionary<int, Dictionary<int, Media>>();

            //temporary media dictionary
            Dictionary<int, Media> medias = new Dictionary<int, Media>();

            try
            {
                Group oGroup = GroupsCache.Instance().GetGroup(nGroupID);

                if (oGroup == null)
                {
                    log.Error("Error - Could not load group from cache in GetGroupMedias. ESFeeder");
                    return dMediaTrans;
                }

                ApiObjects.LanguageObj oDefaultLangauge = oGroup.GetGroupDefaultLanguage();

                if (oDefaultLangauge == null)
                {
                    log.Error("Error - Could not get group default language from cache in GetGroupMedias. ESFeeder");
                    return dMediaTrans;
                }

                ODBCWrapper.StoredProcedure GroupMedias = new ODBCWrapper.StoredProcedure("Get_GroupMedias_ml");
                GroupMedias.SetConnectionKey("MAIN_CONNECTION_STRING");

                GroupMedias.AddParameter("@GroupID", nGroupID);
                GroupMedias.AddParameter("@MediaID", nMediaID);

                Task<DataSet> tDS = Task<DataSet>.Factory.StartNew(() => GroupMedias.ExecuteDataSet());
                tDS.Wait();
                DataSet ds = tDS.Result;

                Catalog.Utils.BuildMediaFromDataSet(ref dMediaTrans, ref medias, oGroup, ds);
            }
            catch (Exception ex)
            {
                log.Error("Media Exception - " + ex.Message + " ESFeeder", ex);
            }

            return dMediaTrans;
        }

        public static ApiObjects.SearchObjects.MediaSearchObj BuildBaseChannelSearchObject(Channel channel)
        {
            ApiObjects.SearchObjects.MediaSearchObj searchObject = new ApiObjects.SearchObjects.MediaSearchObj();
            searchObject.m_nGroupId = channel.m_nGroupID;
            searchObject.m_bExact = true;
            searchObject.m_eCutWith = channel.m_eCutWith;
            searchObject.m_sMediaTypes = string.Join(";", channel.m_nMediaType.Select(type => type.ToString()));
            searchObject.m_sPermittedWatchRules = GetPermittedWatchRules(channel.m_nGroupID);
            searchObject.m_oOrder = new ApiObjects.SearchObjects.OrderObj();

            searchObject.m_bUseStartDate = false;
            searchObject.m_bUseFinalEndDate = false;

            CopySearchValuesToSearchObjects(ref searchObject, channel.m_eCutWith, channel.m_lChannelTags);
            return searchObject;
        }

        private static void CopySearchValuesToSearchObjects(ref ApiObjects.SearchObjects.MediaSearchObj searchObject, ApiObjects.SearchObjects.CutWith cutWith, List<ApiObjects.SearchObjects.SearchValue> channelSearchValues)
        {
            List<ApiObjects.SearchObjects.SearchValue> m_dAnd = new List<ApiObjects.SearchObjects.SearchValue>();
            List<ApiObjects.SearchObjects.SearchValue> m_dOr = new List<ApiObjects.SearchObjects.SearchValue>();

            ApiObjects.SearchObjects.SearchValue search = new ApiObjects.SearchObjects.SearchValue();
            if (channelSearchValues != null && channelSearchValues.Count > 0)
            {
                foreach (ApiObjects.SearchObjects.SearchValue searchValue in channelSearchValues)
                {
                    if (!string.IsNullOrEmpty(searchValue.m_sKey))
                    {
                        search = new ApiObjects.SearchObjects.SearchValue();
                        search.m_sKey = searchValue.m_sKey;
                        search.m_lValue = searchValue.m_lValue;
                        search.m_sKeyPrefix = searchValue.m_sKeyPrefix;
                        search.m_eInnerCutWith = searchValue.m_eInnerCutWith;

                        switch (cutWith)
                        {
                            case ApiObjects.SearchObjects.CutWith.OR:
                                {
                                    m_dOr.Add(search);
                                    break;
                                }
                            case ApiObjects.SearchObjects.CutWith.AND:
                                {
                                    m_dAnd.Add(search);
                                    break;
                                }
                            default:
                                break;
                        }
                    }
                }
            }

            if (m_dOr.Count > 0)
            {
                searchObject.m_dOr = m_dOr;
            }

            if (m_dAnd.Count > 0)
            {
                searchObject.m_dAnd = m_dAnd;
            }
        }

        private static string GetPermittedWatchRules(int nGroupId)
        {
            DataTable permittedWathRulesDt = Tvinci.Core.DAL.CatalogDAL.GetPermittedWatchRulesByGroupId(nGroupId, null);
            List<string> lWatchRulesIds = null;
            if (permittedWathRulesDt != null && permittedWathRulesDt.Rows.Count > 0)
            {
                lWatchRulesIds = new List<string>();
                foreach (DataRow permittedWatchRuleRow in permittedWathRulesDt.Rows)
                {
                    lWatchRulesIds.Add(ODBCWrapper.Utils.GetSafeStr(permittedWatchRuleRow["RuleID"]));
                }
            }

            string sRules = string.Empty;

            if (lWatchRulesIds != null && lWatchRulesIds.Count > 0)
            {
                sRules = string.Join(" ", lWatchRulesIds);
            }

            return sRules;
        }
    }
}
