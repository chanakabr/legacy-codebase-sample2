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
using System.Data;
using System.Threading.Tasks;
using KLogMonitor;
using System.Reflection;
using ApiObjects.Response;

namespace ElasticSearchHandler.IndexBuilders
{
    public class MediaIndexBuilder : AbstractIndexBuilder
    {
        private static readonly string MEDIA = "media";

        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public MediaIndexBuilder(int groupID)
            : base(groupID)
        {

        }

        #region Interface Methods

        public override bool BuildIndex()
        {
            string newIndexName = ElasticSearchTaskUtils.GetNewMediaIndexStr(groupId);

            #region Build new index and specify number of nodes/shards

            string numberOfShards = ElasticSearchTaskUtils.GetTcmConfigValue("ES_NUM_OF_SHARDS");
            string numberOfReplicas = ElasticSearchTaskUtils.GetTcmConfigValue("ES_NUM_OF_REPLICAS");
            string sizeOfBulkString = ElasticSearchTaskUtils.GetTcmConfigValue("ES_BULK_SIZE");

            int numOfShards;
            int numOfReplicas;
            int sizeOfBulk;

            int.TryParse(numberOfReplicas, out numOfReplicas);
            int.TryParse(numberOfShards, out numOfShards);
            int.TryParse(sizeOfBulkString, out sizeOfBulk);

            if (sizeOfBulk == 0)
            {
                sizeOfBulk = 50;
            }

            GroupManager groupManager = new GroupManager();
            groupManager.RemoveGroup(groupId);
            Group group = groupManager.GetGroup(groupId);

            if (group == null)
            {
                log.ErrorFormat("Could not load group {0} in media index builder", groupId);
                return false;
            }

            List<string> analyzers;
            List<string> filters;
            List<string> tokenizers;

            GetAnalyzers(group.GetLangauges(), out analyzers, out filters, out tokenizers);

            bool actionResult = api.BuildIndex(newIndexName, numOfShards, numOfReplicas, analyzers, filters, tokenizers);

            if (!actionResult)
            {
                log.Error(string.Format("Failed creating index for index:{0}", newIndexName));
                return actionResult;
            }

            #endregion

            #region create mapping
            foreach (ApiObjects.LanguageObj language in group.GetLangauges())
            {
                string indexAnalyzer, searchAnalyzer;
                string autocompleteIndexAnalyzer = null;
                string autocompleteSearchAnalyzer = null;

                string analyzerDefinitionName = ElasticSearch.Common.Utils.GetLangCodeAnalyzerKey(language.Code);

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
                    log.Error(string.Format("could not find analyzer for language ({0}) for mapping. whitespace analyzer will be used instead", language.Code));
                }

                string mapping = serializer.CreateMediaMapping(group.m_oMetasValuesByGroupId, group.m_oGroupTags, indexAnalyzer, searchAnalyzer, autocompleteIndexAnalyzer, autocompleteSearchAnalyzer);
                string type = (language.IsDefault) ? MEDIA : string.Concat(MEDIA, "_", language.Code);
                bool bMappingRes = api.InsertMapping(newIndexName, type, mapping.ToString());

                if (language.IsDefault && !bMappingRes)
                    actionResult = false;

                if (!bMappingRes)
                {
                    log.Error(string.Concat("Could not create mapping of type media for language ", language.Name));
                }

            }

            if (!actionResult)
                return actionResult;

            #endregion

            #region insert medias
            Dictionary<int, Dictionary<int, Media>> groupMedias = GetGroupMedias(groupId, 0);

            if (groupMedias != null)
            {
                log.Debug("Info - " + string.Format("Start indexing medias. total medias={0}", groupMedias.Count));
                List<ESBulkRequestObj<int>> lBulkObj = new List<ESBulkRequestObj<int>>();

                foreach (int mediaId in groupMedias.Keys)
                {
                    foreach (int languageId in groupMedias[mediaId].Keys)
                    {
                        Media media = groupMedias[mediaId][languageId];

                        if (media != null)
                        {
                            string serializedMedia;

                            serializedMedia = serializer.SerializeMediaObject(media);

                            string sType = ElasticSearchTaskUtils.GetTanslationType(MEDIA, group.GetLanguage(languageId));

                            lBulkObj.Add(new ESBulkRequestObj<int>()
                            {
                                docID = media.m_nMediaID,
                                index = newIndexName,
                                type = sType,
                                document = serializedMedia
                            });
                        }
                        if (lBulkObj.Count >= sizeOfBulk)
                        {
                            Task<List<ESBulkRequestObj<int>>> t = Task<List<ESBulkRequestObj<int>>>.Factory.StartNew(() => api.CreateBulkIndexRequest(lBulkObj));
                            t.Wait();
                            lBulkObj = new List<ESBulkRequestObj<int>>();
                        }
                    }
                }

                if (lBulkObj.Count > 0)
                {
                    Task<List<ESBulkRequestObj<int>>> t = Task<List<ESBulkRequestObj<int>>>.Factory.StartNew(() => api.CreateBulkIndexRequest(lBulkObj));
                    t.Wait();
                }
            }

            #endregion

            #region insert channel queries

            if (group.channelIDs != null)
            {
                log.Info(string.Format("Start indexing channels. total channels={0}", group.channelIDs.Count));

                
                List<KeyValuePair<int, string>> channelRequests = new List<KeyValuePair<int, string>>();
                try
                {
                    List<Channel> allChannels = groupManager.GetChannels(group.channelIDs.ToList(), groupId);

                    ESMediaQueryBuilder mediaQueryParser = new ESMediaQueryBuilder()
                        {
                            QueryType = eQueryType.EXACT
                        };
                    var unifiedQueryBuilder = new ESUnifiedQueryBuilder(null, groupId);

                    foreach (Channel currentChannel in allChannels)
                    {
                        if (currentChannel == null || currentChannel.m_nIsActive != 1)
                            continue;

                        string channelQuery = string.Empty;

                        if (currentChannel.m_nChannelTypeID == (int)ChannelType.KSQL)
                        {
                            try
                            {
                                // If there is at least 1 media type, build its definitions
                                if (currentChannel.m_nMediaType == null || currentChannel.m_nMediaType.Count == 0 || 
                                    (currentChannel.m_nMediaType != null && currentChannel.m_nMediaType.Count > 0 &&
                                    currentChannel.m_nMediaType.Count(type => type != Channel.EPG_ASSET_TYPE) > 0))
                                {
                                    UnifiedSearchDefinitions definitions = ElasticsearchTasksCommon.Utils.BuildSearchDefinitions(currentChannel, true);

                                    unifiedQueryBuilder.SearchDefinitions = definitions;
                                    channelQuery = unifiedQueryBuilder.BuildSearchQueryString(true);
                                }
                            }
                            catch (KalturaException ex)
                            {
                                log.ErrorFormat("Tried to index an invalid KSQL Channel. ID = {0}, message = {1}, st = {2}", currentChannel.m_nChannelID, ex.Message, ex.StackTrace);
                            }
                            catch (Exception ex)
                            {
                                log.ErrorFormat("Failed indexing KSQL channel. ID = {0}, message = {1}, message = {1}, st = {2}", currentChannel.m_nChannelID, ex.Message, ex.StackTrace);
                            }
                        }
                        else
                        {
                            try
                            {
                                mediaQueryParser.m_nGroupID = currentChannel.m_nGroupID;
                                MediaSearchObj mediaSearchObject = BuildBaseChannelSearchObject(currentChannel);

                                mediaQueryParser.oSearchObject = mediaSearchObject;
                                channelQuery = mediaQueryParser.BuildSearchQueryString(true);
                            }
                            catch (Exception ex)
                            {
                                log.ErrorFormat("Failed indexing automatic/manual channel. ID = {0}, message = {1}", currentChannel.m_nChannelID, ex.Message, ex);
                            }
                        }

                        if (!string.IsNullOrEmpty(channelQuery))
                        {
                            channelRequests.Add(new KeyValuePair<int, string>(currentChannel.m_nChannelID, channelQuery));

                            if (channelRequests.Count > 50)
                            {
                                try
                                {
                                    api.CreateBulkIndexRequest("_percolator", newIndexName, channelRequests);
                                }
                                catch (Exception ex)
                                {
                                    log.ErrorFormat("Failed indexing channels in bulk. ex = {0}", ex);
                                }

                                channelRequests.Clear();
                            }
                        }
                    }

                    if (channelRequests.Count > 0)
                    {
                        try
                        {
                            api.CreateBulkIndexRequest("_percolator", newIndexName, channelRequests);
                        }
                        catch (Exception ex)
                        {
                            log.ErrorFormat("Failed indexing channels in bulk. ex = {0}", ex);
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.Error(string.Format("Caught exception while indexing channels. Ex={0};Stack={1}", ex.Message, ex.StackTrace));
                }
            }

            #endregion

            string alias = ElasticSearchTaskUtils.GetMediaGroupAliasStr(groupId);
            bool indexExists = api.IndexExists(alias);

            if (this.SwitchIndexAlias || !indexExists)
            {
                List<string> oldIndices = api.GetAliases(alias);

                Task<bool> taskSwitchIndex = Task<bool>.Factory.StartNew(() => api.SwitchIndex(newIndexName, alias, oldIndices));
                taskSwitchIndex.Wait();

                if (!taskSwitchIndex.Result)
                {
                    log.ErrorFormat("Failed switching index for new index name = {0}, group alias = {1}", newIndexName, alias);
                    actionResult = false;
                }

                if (this.DeleteOldIndices && taskSwitchIndex.Result && oldIndices.Count > 0)
                {
                    Task t = Task.Factory.StartNew(() => api.DeleteIndices(oldIndices));
                    t.Wait();
                }
            }

            return true;
        }

        #endregion

        #region Public Methods

        public static Dictionary<int, Dictionary<int, Media>> GetGroupMedias(int nGroupID, int nMediaID)
        {
            //dictionary contains medias such that first key is media_id, which returns a dictionary with a key language_id and value Media object.
            //E.g. dMedias[123][2] --> will return media 123 of the hebrew language
            Dictionary<int, Dictionary<int, Media>> dMediaTrans = new Dictionary<int, Dictionary<int, Media>>();

            //temporary media dictionary
            Dictionary<int, Media> medias = new Dictionary<int, Media>();

            try
            {
                Group oGroup = GroupsCache.Instance().GetGroup(nGroupID);

                if (oGroup == null)
                {
                    log.Error("Could not load group from cache in GetGroupMedias");
                    return dMediaTrans;
                }

                ApiObjects.LanguageObj oDefaultLangauge = oGroup.GetGroupDefaultLanguage();

                if (oDefaultLangauge == null)
                {
                    log.Error("Could not get group default language from cache in GetGroupMedias");
                    return dMediaTrans;
                }

                ODBCWrapper.StoredProcedure GroupMedias = new ODBCWrapper.StoredProcedure("Get_GroupMedias_ml");
                GroupMedias.SetConnectionKey("MAIN_CONNECTION_STRING");

                GroupMedias.AddParameter("@GroupID", nGroupID);
                GroupMedias.AddParameter("@MediaID", nMediaID);

                Task<DataSet> tDS = Task<DataSet>.Factory.StartNew(() => GroupMedias.ExecuteDataSet());
                tDS.Wait();
                DataSet dataSet = tDS.Result;

                if (dataSet != null && dataSet.Tables.Count > 0)
                {
                    if (dataSet.Tables[0].Rows.Count > 0)
                    {
                        foreach (DataRow row in dataSet.Tables[0].Rows)
                        {
                            Media media = new Media();
                            if (dataSet.Tables[0].Columns != null && dataSet.Tables[0].Rows != null)
                            {
                                #region media info
                                media.m_nMediaID = ODBCWrapper.Utils.GetIntSafeVal(row, "ID");
                                media.m_nWPTypeID = ODBCWrapper.Utils.GetIntSafeVal(row, "watch_permission_type_id");
                                media.m_nMediaTypeID = ODBCWrapper.Utils.GetIntSafeVal(row, "media_type_id");
                                media.m_nGroupID = ODBCWrapper.Utils.GetIntSafeVal(row, "group_id");
                                media.m_nIsActive = ODBCWrapper.Utils.GetIntSafeVal(row, "is_active");
                                media.m_nDeviceRuleId = ODBCWrapper.Utils.GetIntSafeVal(row, "device_rule_id");
                                media.m_nLikeCounter = ODBCWrapper.Utils.GetIntSafeVal(row, "like_counter");
                                media.m_nViews = ODBCWrapper.Utils.GetIntSafeVal(row, "views");
                                media.m_sUserTypes = ODBCWrapper.Utils.GetSafeStr(row["user_types"]);

                                // by default - media is not free
                                media.isFree = false;

                                double dSum = ODBCWrapper.Utils.GetDoubleSafeVal(row, "votes_sum");
                                double dCount = ODBCWrapper.Utils.GetDoubleSafeVal(row, "votes_count");

                                if (dCount > 0)
                                {
                                    media.m_nVotes = (int)dCount;
                                    media.m_dRating = dSum / dCount;
                                }

                                media.m_sName = ODBCWrapper.Utils.GetSafeStr(row, "name");
                                media.m_sDescription = ODBCWrapper.Utils.GetSafeStr(row, "description");

                                if (!string.IsNullOrEmpty(ODBCWrapper.Utils.GetSafeStr(row, "create_date")))
                                {
                                    DateTime dt = ODBCWrapper.Utils.GetDateSafeVal(row, "create_date");
                                    media.m_sCreateDate = dt.ToString("yyyyMMddHHmmss");
                                }
                                if (!string.IsNullOrEmpty(ODBCWrapper.Utils.GetSafeStr(row, "update_date")))
                                {
                                    DateTime dt = ODBCWrapper.Utils.GetDateSafeVal(row, "update_date");
                                    media.m_sUpdateDate = dt.ToString("yyyyMMddHHmmss");
                                }
                                if (!string.IsNullOrEmpty(ODBCWrapper.Utils.GetSafeStr(row, "start_date")))
                                {
                                    DateTime dt = ODBCWrapper.Utils.GetDateSafeVal(row, "start_date");
                                    media.m_sStartDate = dt.ToString("yyyyMMddHHmmss");
                                }

                                if (!string.IsNullOrEmpty(ODBCWrapper.Utils.GetSafeStr(row, "end_date")))
                                {
                                    DateTime dt = ODBCWrapper.Utils.GetDateSafeVal(row, "end_date");
                                    media.m_sEndDate = dt.ToString("yyyyMMddHHmmss");

                                }

                                if (!string.IsNullOrEmpty(ODBCWrapper.Utils.GetSafeStr(row, "final_end_date")))
                                {
                                    DateTime dt = ODBCWrapper.Utils.GetDateSafeVal(row, "final_end_date");
                                    media.m_sFinalEndDate = dt.ToString("yyyyMMddHHmmss");

                                }

                                media.geoBlockRule = ODBCWrapper.Utils.ExtractInteger(row, "geo_block_rule_id");

                                string epgIdentifier = ODBCWrapper.Utils.ExtractString(row, "epg_identifier");

                                if (!string.IsNullOrEmpty(epgIdentifier))
                                {
                                    media.epgIdentifier = epgIdentifier;
                                }

                                #endregion

                                #region - get all metas by groupId
                                Dictionary<string, string> dMetas;
                                //Get Meta - MetaNames (e.g. will contain key/value <META1_STR, show>)
                                if (oGroup.m_oMetasValuesByGroupId.TryGetValue(media.m_nGroupID, out dMetas))
                                {
                                    foreach (string sMeta in dMetas.Keys)
                                    {
                                        //Retreive meta name and check that it is not null or empty so that it will not form an invalid field later on
                                        string sMetaName;
                                        dMetas.TryGetValue(sMeta, out sMetaName);

                                        if (!string.IsNullOrEmpty(sMetaName))
                                        {
                                            string sMetaValue = ODBCWrapper.Utils.GetSafeStr(row[sMeta]);
                                            media.m_dMeatsValues.Add(sMetaName, sMetaValue);
                                        }
                                    }
                                }
                            }
                            medias.Add(media.m_nMediaID, media);
                                #endregion
                        }

                        #region - get all the media files types for each mediaId that have been selected.
                        if (dataSet.Tables[1].Columns != null && dataSet.Tables[1].Rows != null && dataSet.Tables[1].Rows.Count > 0)
                        {
                            foreach (DataRow row in dataSet.Tables[1].Rows)
                            {
                                int mediaID = ODBCWrapper.Utils.GetIntSafeVal(row, "media_id");
                                string sMFT = ODBCWrapper.Utils.GetSafeStr(row, "media_type_id");
                                bool isTypeFree = ODBCWrapper.Utils.ExtractBoolean(row, "is_free");

                                Media theMedia = medias[mediaID];

                                theMedia.m_sMFTypes += string.Format("{0};", sMFT);

                                int mediaTypeId;

                                if (isTypeFree)
                                {
                                    // if at least one of the media types is free - this media is free
                                    theMedia.isFree = true;

                                    if (int.TryParse(sMFT, out mediaTypeId))
                                    {
                                        theMedia.freeFileTypes.Add(mediaTypeId);
                                    }
                                }
                            }
                        }


                        #endregion

                        #region - get regions of media

                        // Regions table should be 6h on stored procedure
                        if (dataSet.Tables.Count > 5 && dataSet.Tables[5].Columns != null && dataSet.Tables[5].Rows != null)
                        {
                            foreach (DataRow mediaRegionRow in dataSet.Tables[5].Rows)
                            {
                                int mediaId = ODBCWrapper.Utils.ExtractInteger(mediaRegionRow, "MEDIA_ID");
                                int regionId = ODBCWrapper.Utils.ExtractInteger(mediaRegionRow, "REGION_ID");

                                // Accumulate region ids in list
                                medias[mediaId].regions.Add(regionId);
                            }
                        }

                        // If no regions were found for this media - use 0, that indicates that the media is region-less
                        foreach (Media media in medias.Values)
                        {
                            if (media.regions.Count == 0)
                            {
                                media.regions.Add(0);
                            }
                        }


                        #endregion

                        #region - get all media tags
                        if (dataSet.Tables[2].Columns != null && dataSet.Tables[2].Rows != null && dataSet.Tables[2].Rows.Count > 0)
                        {
                            foreach (DataRow row in dataSet.Tables[2].Rows)
                            {
                                int nTagMediaID = ODBCWrapper.Utils.GetIntSafeVal(row, "media_id");
                                int mttn = ODBCWrapper.Utils.GetIntSafeVal(row, "tag_type_id");
                                string val = ODBCWrapper.Utils.GetSafeStr(row, "value");
                                long tagID = ODBCWrapper.Utils.GetLongSafeVal(row, "tag_id");

                                try
                                {
                                    if (oGroup.m_oGroupTags.ContainsKey(mttn))
                                    {
                                        string sTagName = oGroup.m_oGroupTags[mttn];

                                        if (!string.IsNullOrEmpty(sTagName))
                                        {
                                            if (!medias[nTagMediaID].m_dTagValues.ContainsKey(sTagName))
                                            {
                                                medias[nTagMediaID].m_dTagValues.Add(sTagName, new Dictionary<long, string>());
                                            }

                                            if (!medias[nTagMediaID].m_dTagValues[sTagName].ContainsKey(tagID))
                                            {
                                                medias[nTagMediaID].m_dTagValues[sTagName].Add(tagID, val);
                                            }
                                        }
                                    }
                                }
                                catch
                                {
                                    log.Error(string.Format("Caught exception when trying to add media to group tags. TagMediaId={0}; TagTypeID={1}; TagID={2}; TagValue={3}",
                                        nTagMediaID, mttn, tagID, val));
                                }
                            }
                        }
                        #endregion

                        #region Clone medias to all translated languages
                        foreach (int mediaID in medias.Keys)
                        {
                            Media media = medias[mediaID];

                            Dictionary<int, Media> tempMediaTrans = new Dictionary<int, Media>();
                            foreach (ApiObjects.LanguageObj oLanguage in oGroup.GetLangauges())
                            {
                                tempMediaTrans.Add(oLanguage.ID, media.Clone());
                            }

                            dMediaTrans.Add(mediaID, tempMediaTrans);

                        }
                        #endregion

                        #region get all translated metas and media info

                        if (dataSet.Tables[3].Columns != null && dataSet.Tables[3].Rows != null && dataSet.Tables[3].Rows.Count > 0)
                        {
                            Dictionary<string, string> dMetas;

                            foreach (DataRow row in dataSet.Tables[3].Rows)
                            {
                                int mediaID = ODBCWrapper.Utils.GetIntSafeVal(row, "MEDIA_ID");
                                int nLanguageID = ODBCWrapper.Utils.GetIntSafeVal(row, "LANGUAGE_ID");

                                if (dMediaTrans.ContainsKey(mediaID) && dMediaTrans[mediaID].ContainsKey(nLanguageID))
                                {
                                    Media oMedia = dMediaTrans[mediaID][nLanguageID];

                                    if (oGroup.m_oMetasValuesByGroupId.TryGetValue(oMedia.m_nGroupID, out dMetas))
                                    {
                                        #region get media translated name
                                        string sTransName = ODBCWrapper.Utils.GetSafeStr(row, "NAME");

                                        if (!string.IsNullOrEmpty(sTransName))
                                            oMedia.m_sName = sTransName;
                                        #endregion

                                        #region get media translated description
                                        string sTransDesc = ODBCWrapper.Utils.GetSafeStr(row, "DESCRIPTION");

                                        if (!string.IsNullOrEmpty(sTransDesc))
                                            oMedia.m_sDescription = sTransDesc;
                                        #endregion

                                        #region get media translated metas
                                        foreach (string sMeta in dMetas.Keys)
                                        {
                                            //if meta is a string, then get translated value from DB, for all other metas, we keep the same values as there's no translation
                                            if (sMeta.EndsWith("_STR"))
                                            {
                                                string sMetaName;
                                                dMetas.TryGetValue(sMeta, out sMetaName);

                                                if (!string.IsNullOrEmpty(sMetaName))
                                                {
                                                    string sMetaValue = ODBCWrapper.Utils.GetSafeStr(row, sMeta);

                                                    if (!string.IsNullOrEmpty(sMetaValue))
                                                    {
                                                        oMedia.m_dMeatsValues[sMetaName] = sMetaValue;
                                                    }
                                                }
                                            }
                                        }
                                        #endregion
                                    }
                                }
                            }
                        }
                        #endregion

                        #region - get all translated media tags
                        if (dataSet.Tables[4].Columns != null && dataSet.Tables[4].Rows != null && dataSet.Tables[4].Rows.Count > 0)
                        {
                            foreach (DataRow row in dataSet.Tables[4].Rows)
                            {
                                int nTagMediaID = ODBCWrapper.Utils.GetIntSafeVal(row, "media_id");
                                int mttn = ODBCWrapper.Utils.GetIntSafeVal(row, "tag_type_id");
                                string val = ODBCWrapper.Utils.GetSafeStr(row, "translated_value");
                                int nLangID = ODBCWrapper.Utils.GetIntSafeVal(row, "language_id");
                                long tagID = ODBCWrapper.Utils.GetLongSafeVal(row, "tag_id");

                                if (oGroup.m_oGroupTags.ContainsKey(mttn) && !string.IsNullOrEmpty(val))
                                {
                                    Media oMedia;

                                    if (dMediaTrans.ContainsKey(nTagMediaID) && dMediaTrans[nTagMediaID].ContainsKey(nLangID))
                                    {
                                        oMedia = dMediaTrans[nTagMediaID][nLangID];
                                        string sTagTypeName = oGroup.m_oGroupTags[mttn];

                                        if (oMedia.m_dTagValues.ContainsKey(sTagTypeName))
                                        {
                                            oMedia.m_dTagValues[sTagTypeName][tagID] = val;
                                        }
                                        else
                                        {
                                            Dictionary<long, string> dTemp = new Dictionary<long, string>();
                                            dTemp[tagID] = val;
                                            oMedia.m_dTagValues[sTagTypeName] = dTemp;
                                        }
                                    }
                                }
                            }
                        }

                        #endregion
                    }

                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("GetGroupMedias - {0}", ex.Message), ex);
            }

            return dMediaTrans;
        }

        #endregion

        #region Private Methods

        private void GetAnalyzers(List<ApiObjects.LanguageObj> lLanguages, out List<string> lAnalyzers, out List<string> lFilters, out List<string> tokenizers)
        {
            lAnalyzers = new List<string>();
            lFilters = new List<string>();
            tokenizers = new List<string>();

            if (lLanguages != null)
            {
                foreach (ApiObjects.LanguageObj language in lLanguages)
                {
                    string analyzer = ElasticSearchApi.GetAnalyzerDefinition(ElasticSearch.Common.Utils.GetLangCodeAnalyzerKey(language.Code));
                    string filter = ElasticSearchApi.GetFilterDefinition(ElasticSearch.Common.Utils.GetLangCodeFilterKey(language.Code));
                    string tokenizer = ElasticSearchApi.GetTokenizerDefinition(ElasticSearch.Common.Utils.GetLangCodeTokenizerKey(language.Code));

                    if (string.IsNullOrEmpty(analyzer))
                    {
                        log.Error(string.Format("analyzer for language {0} doesn't exist", language.Code));
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

        private static ApiObjects.SearchObjects.MediaSearchObj BuildBaseChannelSearchObject(Channel channel)
        {
            ApiObjects.SearchObjects.MediaSearchObj searchObject = new ApiObjects.SearchObjects.MediaSearchObj();
            searchObject.m_nGroupId = channel.m_nGroupID;
            searchObject.m_bExact = true;
            searchObject.m_eCutWith = channel.m_eCutWith;

            if (channel.m_nMediaType != null)
            {
                searchObject.m_sMediaTypes = string.Join(";", channel.m_nMediaType.Select(type => type.ToString()));
            }

            searchObject.m_sPermittedWatchRules = ElasticsearchTasksCommon.Utils.GetPermittedWatchRules(channel.m_nGroupID);
            searchObject.m_oOrder = new ApiObjects.SearchObjects.OrderObj();

            searchObject.m_bUseStartDate = false;
            searchObject.m_bUseFinalEndDate = false;

            CopySearchValuesToSearchObjects(ref searchObject, channel.m_eCutWith, channel.m_lChannelTags);
            return searchObject;
        }


        private static void CopySearchValuesToSearchObjects(ref ApiObjects.SearchObjects.MediaSearchObj searchObject,
            ApiObjects.SearchObjects.CutWith cutWith, List<ApiObjects.SearchObjects.SearchValue> channelSearchValues)
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

        #endregion
    }
}
