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

namespace ElasticSearchHandler.Updaters
{
    public class ChannelUpdater : IUpdateable
    {
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
            Logger.Logger.Log("Info", "Start Channel update", "ESUpdateHandler");
            if (IDs == null || IDs.Count == 0)
            {
                Logger.Logger.Log("Info", "Channel Id list empty", "ESUpdateHandler");
                result = true;

                return result;
            }

            if (!m_oESApi.IndexExists(ElasticsearchTasksCommon.Utils.GetMediaGroupAliasStr(m_nGroupID)))
            {
                Logger.Logger.Log("Error", string.Format("Index of type media for group {0} does not exist", m_nGroupID), "ESUpdateHandler");
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
                            Logger.Logger.Log("Error", string.Concat("Could not delete channel from elasticsearch. ID=", nChannelID), "ESUpdateHandler");
                        }
                    }
                }
            }
            else
            {
                Logger.Logger.Log("Error", string.Concat("Could not find indices for alias ", sIndex), "ESUpdateHandler");
            }

            return bRes;
        }

        private bool UpdateChannel(List<int> lChannelIds)
        {
            bool bRes = false;
            GroupManager groupManager = new GroupManager();
            Group oGroup = groupManager.GetGroup(m_nGroupID);

            if (oGroup == null || oGroup.channelIDs == null || oGroup.channelIDs.Count == 0)
                return bRes;

            List<string> aliases = m_oESApi.GetAliases(m_nGroupID.ToString());

            Channel oChannel;
            MediaSearchObj oSearchObj;
            ESMediaQueryBuilder oQueryParser;
            string sQueryStr;

            if (aliases != null && aliases.Count > 0)
            {
                foreach (int nChannelID in lChannelIds)
                {
                    oChannel = ChannelRepository.GetChannel(nChannelID, oGroup);
                    if (oChannel != null && oChannel.m_nIsActive == 1)
                    {
                        oQueryParser = new ESMediaQueryBuilder() { QueryType = eQueryType.EXACT, m_nGroupID = oChannel.m_nGroupID };
                        oSearchObj = ElasticsearchTasksCommon.Utils.BuildBaseChannelSearchObject(oChannel, oGroup.m_nSubGroup);
                        oQueryParser.oSearchObject = oSearchObj;
                        sQueryStr = oQueryParser.BuildSearchQueryString(false);

                        foreach (string sIndex in aliases)
                        {
                            bRes = m_oESApi.AddQueryToPercolator(sIndex, oChannel.m_nChannelID.ToString(), ref sQueryStr);
                        }
                    }
                }
            }

            return bRes;
        }

        
    }
}
