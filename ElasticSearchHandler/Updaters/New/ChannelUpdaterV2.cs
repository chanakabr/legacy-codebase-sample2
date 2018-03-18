using ApiObjects.SearchObjects;
using ElasticSearch.Common.DeleteResults;
using ElasticSearch.Searcher;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GroupsCacheManager;
using KLogMonitor;
using System.Reflection;
using Core.Catalog.CatalogManagement;

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

        private bool DeleteChannel(List<int> channelIds)
        {
            return IndexManager.DeleteChannelPercolator(m_nGroupID, channelIds);
        }

        private bool UpdateChannel(List<int> channelIds)
        {
            return IndexManager.UpdateChannelPercolator(m_nGroupID, channelIds, null);
        }
    }
}
