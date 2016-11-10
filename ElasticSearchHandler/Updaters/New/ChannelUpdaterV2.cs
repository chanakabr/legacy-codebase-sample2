using ApiObjects.SearchObjects;
using Catalog;
using ElasticSearch.Common.DeleteResults;
using ElasticSearch.Searcher;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Catalog.Cache;
using GroupsCacheManager;
using KLogMonitor;
using System.Reflection;

namespace ElasticSearchHandler.Updaters
{
    public class ChannelUpdaterV2 : IElasticSearchUpdater
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        public static readonly string PERCOLATOR = ".percolator";

        private int m_nGroupID;
        private ElasticSearch.Common.ESSerializerV2 m_oESSerializer;
        private ElasticSearch.Common.ElasticSearchApi esApi;

        public List<int> IDs { get; set; }
        public ApiObjects.eAction Action { get; set; }

        public string ElasticSearchUrl
        {
            get
            {
                if (esApi != null)
                {
                    return esApi.baseUrl;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (esApi != null)
                {
                    esApi.baseUrl = value;
                }
            }
        }

        public ChannelUpdaterV2(int nGroupID)
        {
            m_nGroupID = nGroupID;
            m_oESSerializer = new ElasticSearch.Common.ESSerializerV2();
            esApi = new ElasticSearch.Common.ElasticSearchApi();
        }

        public bool Start()
        {
            bool result = false;
            log.Debug("Info - Start Channel update");
            if (IDs == null || IDs.Count == 0)
            {
                log.Debug("Info - Channel Id list empty");
                result = true;

                return result;
            }

            if (!esApi.IndexExists(ElasticsearchTasksCommon.Utils.GetMediaGroupAliasStr(m_nGroupID)))
            {
                log.Error("Error - " + string.Format("Index of type media for group {0} does not exist", m_nGroupID));
                return result;
            }

            switch (Action)
            {
                case ApiObjects.eAction.Off:
                case ApiObjects.eAction.Delete:
                    result = DeleteChannel(IDs);
                    break;
                case ApiObjects.eAction.On:
                case ApiObjects.eAction.Update:
                    result = UpdateChannel(IDs);
                    break;
                default:
                    result = true;
                    break;
            }

            return result;
        }

        private bool DeleteChannel(List<int> ids)
        {
            bool result = false;
            string mediaIndex = m_nGroupID.ToString();
            string epgIndex = string.Format("{0}_epg", m_nGroupID);

            ESDeleteResult deleteResult;

            bool epgExists = esApi.IndexExists(epgIndex);

            List<string> mediaAliases = esApi.GetAliases(mediaIndex);
            List<string> epgAliases = null;

            if (epgExists)
            {
                epgAliases = esApi.GetAliases(epgIndex);
            }

            // If we found aliases to both, or if we don't have EPG at all
            if (mediaAliases != null && epgAliases != null &&
                (!epgExists || (mediaAliases.Count > 0 && epgAliases.Count > 0)))
            {
                result = true;
            }

            if (mediaAliases != null && mediaAliases.Count > 0)
            {
                foreach (int channelID in ids)
                {
                    foreach (string index in mediaAliases)
                    {
                        deleteResult = esApi.DeleteDoc(index, PERCOLATOR, channelID.ToString());
                        result &= deleteResult.Ok;

                        if (!deleteResult.Ok)
                        {
                            log.Error("Error - " + string.Concat("Could not delete channel from elasticsearch. ID=", channelID));
                        }
                    }
                }
            }
            else
            {
                log.Error("Error - " + string.Concat("Could not find indices for alias ", mediaIndex));
            }

            if (epgAliases != null && epgAliases.Count > 0)
            {
                foreach (int channelId in ids)
                {
                    foreach (string index in epgAliases)
                    {
                        deleteResult = esApi.DeleteDoc(index, PERCOLATOR, channelId.ToString());
                        result &= deleteResult.Ok;

                        if (!deleteResult.Ok)
                        {
                            log.Error("Error - " + string.Concat("Could not delete channel from elasticsearch. ID=", channelId));
                        }
                    }
                }
            }
            else
            {
                log.Error("Error - " + string.Concat("Could not find indices for alias ", epgIndex));
            }

            return result;
        }

        private bool UpdateChannel(List<int> channelIds)
        {
            bool result = false;
            GroupManager groupManager = new GroupManager();
            Group group = groupManager.GetGroup(m_nGroupID);

            if (group == null || group.channelIDs == null || group.channelIDs.Count == 0)
            {
                return result;
            }

            List<string> mediaAliases = esApi.GetAliases(m_nGroupID.ToString());
            List<string> epgAliases = esApi.GetAliases(string.Format("{0}_epg", m_nGroupID));

            if (mediaAliases != null && mediaAliases.Count > 0)
            {
                foreach (int channelId in channelIds)
                {
                    Channel channel = ChannelRepository.GetChannel(channelId, group);

                    if (channel != null && channel.m_nIsActive == 1)
                    {                        
                        bool isMedia = false;
                        bool isEpg = false;

                        string channelQuery = string.Empty;

                        if (channel.m_nChannelTypeID == (int)ChannelType.KSQL)
                        {
                            UnifiedSearchDefinitions definitions = ElasticsearchTasksCommon.Utils.BuildSearchDefinitions(channel, true);

                            isMedia = definitions.shouldSearchMedia;
                            isEpg = definitions.shouldSearchEpg;

                            var unifiedQueryBuilder = new ESUnifiedQueryBuilder(definitions);
                            channelQuery = unifiedQueryBuilder.BuildSearchQueryString(true);
                        }
                        else
                        {
                            isMedia = true;
                            ESMediaQueryBuilder mediaQueryParser = new ESMediaQueryBuilder()
                            {
                                QueryType = eQueryType.EXACT
                            };

                            mediaQueryParser.m_nGroupID = channel.m_nGroupID;
                            MediaSearchObj mediaSearchObject = ElasticsearchTasksCommon.Utils.BuildBaseChannelSearchObject(channel, group.m_nSubGroup);

                            mediaQueryParser.oSearchObject = mediaSearchObject;
                            channelQuery = mediaQueryParser.BuildSearchQueryString(true);
                        }

                        log.DebugFormat("Update channel with query: {0}", channelQuery);

                        if (isMedia)
                        {
                            foreach (string alias in mediaAliases)
                            {
                                result = esApi.AddQueryToPercolatorV2(alias, channel.m_nChannelID.ToString(), ref channelQuery);
                            }
                        }

                        if (isEpg)
                        {
                            foreach (string alias in epgAliases)
                            {
                                result = esApi.AddQueryToPercolatorV2(alias, channel.m_nChannelID.ToString(), ref channelQuery);
                            }
                        }

                    }
                }
            }

            return result;
        }
    }
}
