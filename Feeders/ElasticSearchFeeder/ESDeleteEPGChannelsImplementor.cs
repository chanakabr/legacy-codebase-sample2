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
                es.DeleteDocsByQuery(m_sQueueName, eESFeeder.ToString().ToLower(), ref query);
            }
            else
            {
                // log
            }
        }
    }
}
