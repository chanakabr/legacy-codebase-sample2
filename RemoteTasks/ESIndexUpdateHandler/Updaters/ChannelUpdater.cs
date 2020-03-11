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

namespace ESIndexUpdateHandler.Updaters
{
    public class ChannelUpdater : IUpdateable
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        public static readonly string PERCOLATOR = "_percolator";

        private int m_nGroupID;
        private ElasticSearch.Common.ESSerializer m_oESSerializer;
        private ElasticSearch.Common.ElasticSearchApi m_oESApi;

        public List<int> IDs { get; set; }
        public ApiObjects.eAction Action { get; set; }

        public ChannelUpdater(int nGroupID)
        {
            m_nGroupID = nGroupID;
            m_oESSerializer = new ElasticSearch.Common.ESSerializer();
            m_oESApi = new ElasticSearch.Common.ElasticSearchApi();
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

            if (!m_oESApi.IndexExists(ElasticsearchTasksCommon.Utils.GetMediaGroupAliasStr(m_nGroupID)))
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

        private bool DeleteChannel(List<int> lIDs)
        {
            bool bRes = false;
            string sIndex = m_nGroupID.ToString();

            List<string> aliases = m_oESApi.GetAliases(sIndex);
            if (aliases != null && aliases.Count > 0)
            {
                bRes = true;

                ESDeleteResult deleteResult;
                foreach (int nChannelID in lIDs)
                {
                    foreach (string index in aliases)
                    {
                        deleteResult = m_oESApi.DeleteDoc(PERCOLATOR, index, nChannelID.ToString());
                        bRes &= deleteResult.Ok;

                        if (!deleteResult.Ok)
                        {
                            log.Error("Error - " + string.Concat("Could not delete channel from Elasticsearch. ID=", nChannelID));
                        }
                    }
                }
            }
            else
            {
                log.Error("Error - " + string.Concat("Could not find indices for alias ", sIndex));
            }

            return bRes;
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

            List<string> aliases = m_oESApi.GetAliases(m_nGroupID.ToString());

            Channel channel;
            MediaSearchObj searchObject;
            ESMediaQueryBuilder queryBuilder;
            string queryString;

            if (aliases != null && aliases.Count > 0)
            {
                foreach (int nChannelID in channelIds)
                {
                    channel = ChannelRepository.GetChannel(nChannelID, group);
                    if (channel != null && channel.m_nIsActive == 1)
                    {
                        queryBuilder = new ESMediaQueryBuilder()
                        {
                            QueryType = eQueryType.EXACT,
                            m_nGroupID = channel.m_nGroupID
                        };
                        searchObject = ElasticsearchTasksCommon.Utils.BuildBaseChannelSearchObject(channel, group.m_nSubGroup);
                        queryBuilder.oSearchObject = searchObject;
                        queryString = queryBuilder.BuildSearchQueryString(true);

                        foreach (string alias in aliases)
                        {
                            result = m_oESApi.AddQueryToPercolator(alias, channel.m_nChannelID.ToString(), ref queryString);
                        }
                    }
                }
            }

            return result;
        }
    }
}
