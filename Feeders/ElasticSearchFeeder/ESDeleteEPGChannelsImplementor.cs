using ElasticSearch.Common;
using ElasticSearch.Searcher;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ApiObjects;
using KLogMonitor;
using System.Reflection;

namespace ElasticSearchFeeder
{
    public class ESDeleteEPGChannelsImplementor : ElasticSearchBaseImplementor
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private static readonly string ES_DELETE_CHANNELS_LOG_FILE = "ESDeleteEPGChannelsImplementor";
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
                ESTerms epgChannelIDsTerms = new ESTerms(true) { isNot = false, Key = "epg_channel_id" };
                epgChannelIDsTerms.Value.AddRange(epgChannelIDsToDelete.Select((item) => item.ToString()));
                boolQuery.AddChild(epgChannelIDsTerms, ApiObjects.SearchObjects.CutWith.AND);
                query = boolQuery.ToString();
                log.Debug("Status - " + String.Concat("Sending to ES the following delete query: ", query) + ES_DELETE_CHANNELS_LOG_FILE);

                bool deleteRes = es.DeleteDocsByQuery(m_sQueueName, eESFeeder.ToString().ToLower(), ref query);
                if (!deleteRes)
                {
                    StringBuilder sb = new StringBuilder("Failed to delete programs from ES of the following epg channel ids: ");
                    for (int i = 0; i < epgChannelIDsToDelete.Count; i++)
                    {
                        sb.Append(String.Concat(epgChannelIDsToDelete[i], ";"));
                    }
                    sb.Append(String.Concat(" ES Query: ", query));
                    sb.Append(String.Concat(" G ID: ", m_nGroupID));
                    sb.Append(String.Concat(" Index Name: ", m_sQueueName));
                    sb.Append(String.Concat(" ES Type: ", eESFeeder.ToString().ToLower()));
                    log.Error("Error - " + sb.ToString() + ES_DELETE_CHANNELS_LOG_FILE);
                }
                else
                {
                    // success. log and try remove from cb.
                    StringBuilder sb = new StringBuilder("Succeeded to delete programmes from ES of the following epg channel ids: ");
                    for (int i = 0; i < epgChannelIDsToDelete.Count; i++)
                    {
                        sb.Append(String.Concat(epgChannelIDsToDelete[i], ";"));
                    }
                    sb.Append(String.Concat(" ES Query: ", query));
                    sb.Append(String.Concat(" G ID: ", m_nGroupID));
                    sb.Append(String.Concat(" Index Name: ", m_sQueueName));
                    sb.Append(String.Concat(" ES Type: ", eESFeeder.ToString().ToLower()));
                    log.Debug("Status - " + sb.ToString());
                    bool cbRemovalSuccess = DeleteChannelProgramsFromCB(m_nGroupID);
                    if (cbRemovalSuccess)
                    {
                        log.Debug("Success - Succeeded removing programs from cb. " + ES_DELETE_CHANNELS_LOG_FILE);
                    }
                    else
                    {
                        log.Error("Error - Failed to removed data from CB. " + ES_DELETE_CHANNELS_LOG_FILE);
                    }
                }
            }
            else
            {
                // log
                log.Error("Error - " + String.Concat("Wrong ES Feeder Type: ", eESFeeder.ToString()) + ES_DELETE_CHANNELS_LOG_FILE);
            }
        }

        private bool DeleteChannelProgramsFromCB(int nGroupID)
        {
            bool res = true;
            try
            {
                log.Debug("Status - " + String.Concat("Entering DeleteChannelProgramsFromCB try block. Group ID: ", nGroupID) + ES_DELETE_CHANNELS_LOG_FILE);
                EpgBL.BaseEpgBL oEpgBL = EpgBL.Utils.GetInstance(nGroupID);
                List<DateTime> datesToDelete = new List<DateTime>();
                DateTime now = DateTime.UtcNow;
                DateTime pivot = new DateTime(now.Year, now.Month, now.Day);
                for (int i = -60; i < 61; i++)
                {
                    datesToDelete.Add(pivot.AddDays(i));
                }
                log.Debug("Status - " + String.Concat("Deleting programs of channels: ", epgChannelIDsToDelete.Aggregate<int, string>(string.Empty, (acc1, curr) => (String.Concat(acc1, curr, ";"))), " in dates: ", datesToDelete.Aggregate<DateTime, string>(string.Empty, (acc1, curr) => (String.Concat(acc1, curr.ToString("yyyy-MM-dd"), ";")))) + ES_DELETE_CHANNELS_LOG_FILE);
                for (int i = 0; i < epgChannelIDsToDelete.Count; i++)
                {
                    log.Debug("Status - " + String.Concat("Trying to delete programs of epg channel id: ", epgChannelIDsToDelete[i]) + ES_DELETE_CHANNELS_LOG_FILE);
                    oEpgBL.RemoveGroupPrograms(datesToDelete, epgChannelIDsToDelete[i]);
                    log.Debug("Status - " + String.Concat("Probably succeeded to delete programs of epg channel id: ", epgChannelIDsToDelete[i]) + ES_DELETE_CHANNELS_LOG_FILE);
                }
            }
            catch (Exception ex)
            {
                res = false;
                log.Error("Exception", ex);
            }

            return res;


        }
    }
}
