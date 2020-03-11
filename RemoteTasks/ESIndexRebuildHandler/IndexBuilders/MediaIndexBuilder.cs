using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ElasticSearch.Common;
using ElasticsearchTasksCommon;
using Catalog;
using ApiObjects.SearchObjects;
using ElasticSearch.Searcher;
using GroupsCacheManager;
using KLogMonitor;
using System.Reflection;

namespace ESIndexRebuildHandler.IndexBuilders
{
    public class MediaIndexBuilder : IIndexBuilder
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private static readonly string MEDIA = "media";

        private int m_nGroupID;
        private ESSerializer m_oESSerializer;
        private ElasticSearchApi m_oESApi;
        private Group m_oGroup;
        private GroupManager groupManager;

        public bool SwitchIndexAlias { get; set; }
        public bool DeleteOldIndices { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public MediaIndexBuilder(int nGroupID)
        {
            m_nGroupID = nGroupID;
            m_oESApi = new ElasticSearchApi();
            m_oESSerializer = new ESSerializer();
        }

        public bool Build()
        {
            bool bSuccess = false;
            log.Debug("Info - " + string.Concat("Starting media index build for group ", m_nGroupID));

            if (m_nGroupID == 0)
            {
                bSuccess = true;
                return bSuccess;
            }

            GroupManager groupManager = new GroupManager();
            m_oGroup = groupManager.GetGroup(m_nGroupID);

            if (m_oGroup == null)
            {
                log.Error("Error - Could not load group in media index builder");
                return bSuccess;
            }

            string sNewIndex;
            bSuccess = CreateIndex(out sNewIndex);

            if (!bSuccess)
            {
                log.Error("Error - " + string.Concat("Building index for group failed. group id=", m_nGroupID));
                return bSuccess;
            }


            bSuccess = CreateMapping(sNewIndex);

            if (!bSuccess)
            {
                log.Error("Error - " + string.Concat("Building mapping for group failed. group id=", m_nGroupID));
                return bSuccess;
            }

            IndexMedias(sNewIndex);
            IndexChannels(sNewIndex);

            if (SwitchIndexAlias)
                bSuccess = SwitchIndices(sNewIndex);


            return bSuccess;
        }

        private bool CreateIndex(out string sNewIndex)
        {
            string sNumOfShards = GetTcmConfigValue("ES_NUM_OF_SHARDS");
            string sNumOfReplicas = GetTcmConfigValue("ES_NUM_OF_REPLICAS");

            int nNumOfShards, nNumOfReplicas;

            int.TryParse(sNumOfReplicas, out nNumOfReplicas);
            int.TryParse(sNumOfShards, out nNumOfShards);

            List<string> lAnalyzers;
            List<string> lFilters;
            GetAnalyzers(m_oGroup.GetLangauges(), out lAnalyzers, out lFilters);

            sNewIndex = ElasticsearchTasksCommon.Utils.GetNewMediaIndexStr(m_nGroupID);
            bool bRes = m_oESApi.BuildIndex(sNewIndex, nNumOfShards, nNumOfReplicas, lAnalyzers, lFilters);

            return bRes;
        }

        private bool CreateMapping(string sIndex)
        {
            bool bRes = false;
            foreach (ApiObjects.LanguageObj language in m_oGroup.GetLangauges())
            {
                string indexAnalyzer, searchAnalyzer;

                if (ElasticSearchApi.AnalyzerExists(ElasticSearch.Common.Utils.GetLangCodeAnalyzerKey(language.Code)))
                {
                    indexAnalyzer = string.Concat(language.Code, "_index_", "analyzer");
                    searchAnalyzer = string.Concat(language.Code, "_search_", "analyzer");
                }
                else
                {
                    indexAnalyzer = "whitespace";
                    searchAnalyzer = "whitespace";
                    log.Error("Error - " + string.Format("could not find analyzer for language ({0}) for mapping. whitespace analyzer will be used instead", language.Code));
                }

                string sMapping = m_oESSerializer.CreateMediaMapping(m_oGroup.m_oMetasValuesByGroupId, m_oGroup.m_oGroupTags, indexAnalyzer, searchAnalyzer);
                string sType = (language.IsDefault) ? MEDIA : string.Concat(MEDIA, "_", language.Code);
                bool bMappingRes = m_oESApi.InsertMapping(sIndex, sType, sMapping.ToString());

                if (language.IsDefault && bMappingRes)
                    bRes = true;

                if (!bMappingRes)
                    log.Error("Error - " + string.Concat("Could not create mapping of type media for language ", language.Name));

            }

            return bRes;
        }

        private void IndexMedias(string sIndex)
        {
            Dictionary<int, Dictionary<int, Media>> dGroupMedias = ElasticsearchTasksCommon.Utils.GetGroupMedias(m_nGroupID, 0);

            if (dGroupMedias != null)
            {
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

                            string sType = ElasticsearchTasksCommon.Utils.GetTanslationType(MEDIA, m_oGroup.GetLanguage(nLangID));

                            lBulkObj.Add(new ESBulkRequestObj<int>() { docID = oMedia.m_nMediaID, index = sIndex, type = sType, document = sMediaObj });
                        }
                        if (lBulkObj.Count >= 50)
                        {
                            m_oESApi.CreateBulkIndexRequest(lBulkObj);
                            lBulkObj = new List<ESBulkRequestObj<int>>();
                        }
                    }
                }

                if (lBulkObj.Count > 0)
                {
                    m_oESApi.CreateBulkIndexRequest(lBulkObj);
                }
            }
        }

        private void IndexChannels(string sIndex)
        {

            if (m_oGroup.channelIDs != null)
            {
                MediaSearchObj oSearchObj;
                string sQueryStr;
                ESMediaQueryBuilder oQueryParser = new ESMediaQueryBuilder() { QueryType = eQueryType.EXACT };


                List<KeyValuePair<int, string>> lChannelRequests = new List<KeyValuePair<int, string>>();
                foreach (int channelId in m_oGroup.channelIDs)
                {
                    Channel oChannel = groupManager.GetChannel(channelId, ref m_oGroup);

                    if (oChannel == null || oChannel.m_nIsActive != 1)
                        continue;

                    oQueryParser.m_nGroupID = oChannel.m_nGroupID;
                    oSearchObj = ElasticsearchTasksCommon.Utils.BuildBaseChannelSearchObject(oChannel, m_oGroup.m_nSubGroup);
                    oQueryParser.oSearchObject = oSearchObj;
                    sQueryStr = oQueryParser.BuildSearchQueryString(true);

                    lChannelRequests.Add(new KeyValuePair<int, string>(oChannel.m_nChannelID, sQueryStr));

                    if (lChannelRequests.Count > 50)
                    {
                        m_oESApi.CreateBulkIndexRequest("_percolator", sIndex, lChannelRequests);
                        lChannelRequests.Clear();
                    }
                }

                if (lChannelRequests.Count > 0)
                {
                    m_oESApi.CreateBulkIndexRequest("_percolator", sIndex, lChannelRequests);
                }
            }

        }

        private bool SwitchIndices(string sIndex)
        {
            string sAlias = ElasticsearchTasksCommon.Utils.GetMediaGroupAliasStr(m_nGroupID);
            List<string> lOldIndices = m_oESApi.GetAliases(sAlias);

            bool bSwithcIndex = m_oESApi.SwitchIndex(sIndex, sAlias, lOldIndices);

            if (!bSwithcIndex)
            {
                log.Debug("Info - " + string.Concat("Unable to switch from old to new index. id=", sIndex));
            }
            else if (DeleteOldIndices)
            {
                m_oESApi.DeleteIndices(lOldIndices);
            }

            return bSwithcIndex;
        }

        private void GetAnalyzers(List<ApiObjects.LanguageObj> lLanguages, out List<string> lAnalyzers, out List<string> lFilters)
        {
            lAnalyzers = new List<string>();
            lFilters = new List<string>();

            if (lLanguages != null)
            {
                foreach (ApiObjects.LanguageObj language in lLanguages)
                {
                    string analyzer = ElasticSearchApi.GetAnalyzerDefinition(ElasticSearch.Common.Utils.GetLangCodeAnalyzerKey(language.Code));
                    string filter = ElasticSearchApi.GetFilterDefinition(ElasticSearch.Common.Utils.GetLangCodeFilterKey(language.Code));

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
                }
            }
        }

        public static string GetTcmConfigValue(string sKey)
        {
            string result = string.Empty;
            try
            {
                result = TCMClient.Settings.Instance.GetValue<string>(sKey);
            }
            catch (Exception ex)
            {
                result = string.Empty;
                log.Error("TvinciShared.Ws_Utils Key=" + sKey + "," + ex.Message, ex);
            }
            return result;
        }
    }
}
