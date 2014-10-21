using ElasticSearch.Common;
using ElasticSearch.Searcher;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ElasticSearchFeeder
{
    public class ESDeleteEPGChannelsImplementor : ElasticSearchBaseImplementor
    {
        private static readonly string ES_DELETE_CHANNELS_LOG_FILE = "esDeleteChannels";
        private List<int> epgChannelIDsToDelete;

        public ESDeleteEPGChannelsImplementor(List<int> epgChannelIDsToDelete, int nGroupID, string sQueueName, bool bReload)
            : base(nGroupID, sQueueName, bReload)
        {
            if (epgChannelIDsToDelete == null || epgChannelIDsToDelete.Count == 0)
            {
                throw new ArgumentException("EPG Channel IDs is null or empty.");
            }
            this.epgChannelIDsToDelete = epgChannelIDsToDelete;
        }

        public override void Update(eESFeederType eESFeeder)
        {
            if (eESFeeder == eESFeederType.EPG)
            {
                ElasticSearchApi es = new ElasticSearchApi();
                string query = string.Empty;
                BoolQuery boolQuery = new BoolQuery();
                ESTerms epgChannelIDsTerms = new ESTerms(true) { bNot = false, Key = "epg_channel_id" };
                epgChannelIDsTerms.Value.AddRange(epgChannelIDsToDelete.Select((item) => item.ToString()));
                boolQuery.AddChild(epgChannelIDsTerms, ApiObjects.SearchObjects.CutWith.AND);
                query = boolQuery.ToString();
                bool deleteRes = es.DeleteDocsByQuery(m_sQueueName, eESFeeder.ToString().ToLower(), ref query);
                if (!deleteRes)
                {
                    #region Logging
                    StringBuilder sb = new StringBuilder("Failed to delete programmes from ES of the following epg channel ids: ");
                    for (int i = 0; i < epgChannelIDsToDelete.Count; i++)
                    {
                        sb.Append(String.Concat(epgChannelIDsToDelete[i], ";"));
                    }
                    sb.Append(String.Concat(" ES Query: ", query));
                    sb.Append(String.Concat(" G ID: ", m_nGroupID));
                    sb.Append(String.Concat(" Index Name: ", m_sQueueName));
                    sb.Append(String.Concat(" ES Type: ", eESFeeder.ToString().ToLower()));
                    Logger.Logger.Log("Error", sb.ToString(), ES_DELETE_CHANNELS_LOG_FILE);
                    #endregion
                }
            }
            else
            {
                // log
                Logger.Logger.Log("Error", String.Concat("Wrong ES Feeder Type: ", eESFeeder.ToString()), ES_DELETE_CHANNELS_LOG_FILE);
            }
        }
    }
}
