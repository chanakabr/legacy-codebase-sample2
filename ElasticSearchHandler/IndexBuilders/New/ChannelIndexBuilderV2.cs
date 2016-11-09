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
using KlogMonitorHelper;

namespace ElasticSearchHandler.IndexBuilders
{
    public class ChannelIndexBuilderV2 : AbstractIndexBuilder
    {

        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public ChannelIndexBuilderV2(int groupID)
            : base(groupID)
        {
            serializer = new ESSerializerV2();
        }

        #region Interface Methods

        public override bool BuildIndex()
        {
            bool result = false;

            ContextData cd = new ContextData();

            // get index name
            string indexName = ElasticSearchTaskUtils.GetMediaGroupAliasStr(groupId);

            // make sure the index exists - if not build the index
            if (api.IndexExists(indexName))
            {
                GroupManager groupManager = new GroupManager();
                Group group = groupManager.GetGroup(groupId);

                if (group == null)
                {
                    log.ErrorFormat("Could not load group {0} in media index builder", groupId);
                    return false;
                }
                try
                {
                    // get current indexed channels
                    ESMatchAllQuery matchAllQuery = new ESMatchAllQuery();
                    FilteredQuery filteredQuery = new FilteredQuery()
                    {
                        Query = matchAllQuery
                    };

                    string query = filteredQuery.ToString();

                    string searchResults = api.Search(indexName, ElasticSearch.Common.Utils.ES_PERCOLATOR_TYPE, ref query);

                    List<string> currentChannelIds = ElasticSearch.Common.Utils.GetDocumentIds(searchResults);

                    HashSet<string> channelsToRemove;

                    // insert / update new channels
                    result = BuildChannelQueries(groupId, api, group.channelIDs, indexName, out channelsToRemove);

                    // remove old deleted channels
                    List<ESBulkRequestObj<string>> bulkList = new List<ESBulkRequestObj<string>>();
                    int sizeOfBulk = 500;

                    int id = 0;
                    foreach (var channelId in currentChannelIds)
                    {
                        // channel is not in groups channel anymore / channel with empty query / channel id is not int - must be garbage
                        if ((int.TryParse(channelId, out id) && !group.channelIDs.Contains(id)) || id == 0 || channelsToRemove.Contains(channelId))
                        {
                            log.DebugFormat("Removing channel from percolator - channelId = {0}", channelId);

                            bulkList.Add(new ESBulkRequestObj<string>()
                            {
                                docID = channelId,
                                index = indexName,
                                type = ElasticSearch.Common.Utils.ES_PERCOLATOR_TYPE,
                                Operation = eOperation.delete
                            });

                            if (bulkList.Count >= sizeOfBulk)
                            {
                                Task t = Task.Factory.StartNew(() =>
                                {
                                    cd.Load();
                                    var invalidResults = api.CreateBulkRequest(bulkList);

                                    if (invalidResults != null && invalidResults.Count > 0)
                                    {
                                        foreach (var item in invalidResults)
                                        {
                                            log.ErrorFormat("Error - Could not add channel to ES index. GroupID={0};ID={1};error={2};",
                                                groupId, item.Key, item.Value);
                                        }
                                    }
                                });
                                t.Wait();
                                bulkList = new List<ESBulkRequestObj<string>>();
                            }
                        }
                    }

                    if (bulkList.Count > 0)
                    {
                        Task t = Task.Factory.StartNew(() =>
                        {
                            cd.Load();
                            var invalidResults = api.CreateBulkRequest(bulkList);

                            if (invalidResults != null && invalidResults.Count > 0)
                            {
                                foreach (var item in invalidResults)
                                {
                                    log.ErrorFormat("Error - Could not add channel to ES index. GroupID={0};ID={1};error={2};",
                                        groupId, item.Key, item.Value);
                                }
                            }
                        });
                        t.Wait();
                    }
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("Error while rebuilding channels in index for group = {0}", groupId, ex);
                    return false;
                }
            }
            else
            {
                // TODO: Ira decide
                return new MediaIndexBuilderV1(groupId).BuildIndex();
            }

            return true;
        }

        #endregion

        public static bool BuildChannelQueries(int groupId, ElasticSearchApi api, HashSet<int> channelIds, string newIndexName, out HashSet<string> channelsToRemove)
        {
            channelsToRemove = new HashSet<string>();

            if (channelIds != null)
            {
                log.Info(string.Format("Start indexing channels. total channels={0}", channelIds.Count));

                List<KeyValuePair<int, string>> channelRequests = new List<KeyValuePair<int, string>>();

                try
                {
                    GroupManager groupManager = new GroupManager();
                    groupManager.RemoveGroup(groupId);

                    List<Channel> allChannels = groupManager.GetChannels(channelIds.ToList(), groupId);

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
                                UnifiedSearchDefinitions definitions = ElasticsearchTasksCommon.Utils.BuildSearchDefinitions(currentChannel, true);

                                unifiedQueryBuilder.SearchDefinitions = definitions;
                                channelQuery = unifiedQueryBuilder.BuildSearchQueryString(true);
                            }
                            catch (KalturaException ex)
                            {
                                log.ErrorFormat("Tried to index an invalid KSQL Channel. ID = {0}, message = {1}", currentChannel.m_nChannelID, ex.Message, ex);
                            }
                            catch (Exception ex)
                            {
                                throw ex;
                            }
                        }
                        else
                        {

                            mediaQueryParser.m_nGroupID = currentChannel.m_nGroupID;
                            MediaSearchObj mediaSearchObject = BuildBaseChannelSearchObject(currentChannel);

                            mediaQueryParser.oSearchObject = mediaSearchObject;
                            channelQuery = mediaQueryParser.BuildSearchQueryString(true);
                        }

                        if (!string.IsNullOrEmpty(channelQuery))
                        {
                            log.DebugFormat("Adding channel to percolator - channelId = {0}", currentChannel.m_nChannelID);

                            channelRequests.Add(new KeyValuePair<int, string>(currentChannel.m_nChannelID, channelQuery));

                            if (channelRequests.Count > 50)
                            {
                                api.CreateBulkIndexRequest(newIndexName, ElasticSearch.Common.Utils.ES_PERCOLATOR_TYPE, channelRequests);
                                channelRequests.Clear();
                            }
                        }
                        else
                        {
                            log.DebugFormat("channel with empty query will be removed from percolator - channelId = {0}", currentChannel.m_nChannelID);
                            channelsToRemove.Add(currentChannel.m_nChannelID.ToString());
                        }
                    }

                    if (channelRequests.Count > 0)
                    {
                        api.CreateBulkIndexRequest(newIndexName, ElasticSearch.Common.Utils.ES_PERCOLATOR_TYPE, channelRequests);
                    }
                }
                catch (Exception ex)
                {
                    log.Error(string.Format("Caught exception while indexing channels. Ex={0};Stack={1}", ex.Message, ex.StackTrace));
                    return false;
                }
            }

            return true;
        }

        private static ApiObjects.SearchObjects.MediaSearchObj BuildBaseChannelSearchObject(Channel channel)
        {
            ApiObjects.SearchObjects.MediaSearchObj searchObject = new ApiObjects.SearchObjects.MediaSearchObj();
            searchObject.m_nGroupId = channel.m_nGroupID;
            searchObject.m_bExact = true;
            searchObject.m_eCutWith = channel.m_eCutWith;
            searchObject.m_sMediaTypes = string.Join(";", channel.m_nMediaType.Select(type => type.ToString()));
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
    }
}
