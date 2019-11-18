using ElasticSearch.Common;
using KLogMonitor;
using Polly;
using Polly.Retry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ElasticsearchIndexCleaner
{
    public class IndexCleaner
    {

        private static readonly KLogger _Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public bool Clean(IEnumerable<int> groupIds, int oldIndicesToSaveCount)
        {
            var esClient = new ElasticSearchApi();

            foreach (var groupId in groupIds)
            {
                _Logger.Info($"Starting ElasticsearchIndexCleaner for gropup:[{groupId}], oldIndicesToSaveCount:[{oldIndicesToSaveCount}]");
                var indices = esClient.ListIndices($"{groupId}_epg_*");
                var indicesToDelete = indices.Where(i => !i.Aliases.Any()).Select(i => i.Name).ToList();

                if (oldIndicesToSaveCount < 0)
                {
                    _Logger.Error($"ElasticsearchIndexCleaner oldIndicesToSaveCount:[{oldIndicesToSaveCount}] shouold be greater to or equal to 0");
                    return false;
                }
                if (oldIndicesToSaveCount >= indicesToDelete.Count)
                {
                    _Logger.Info($"ElasticsearchIndexCleaner oldIndicesToSaveCount:[{oldIndicesToSaveCount}] is greater or equal to the amount of exsisting backups, existing without deletion. indicesToDelete.Count:[{indicesToDelete.Count}]");
                    return true;
                }

                var counter = indicesToDelete.ToDictionary(key => int.Parse(key.Split('_').Last()), value => value);
                var sorted = new SortedDictionary<int, string>(counter, Comparer<int>.Default);
                indicesToDelete = sorted.Take(indicesToDelete.Count - oldIndicesToSaveCount).Select(x => x.Value).ToList();

                var indicesToDeleteStr = string.Join(",", indicesToDelete);
                _Logger.Debug($"Deleting indices:[{indicesToDeleteStr}] ");
                using (var mon = new KMonitor(Events.eEvent.EVENT_ELASTIC, groupId.ToString(), "delete_old_epg_indices"))
                {
                    mon.Table = $"indices_to_delete:[{indicesToDeleteStr}]";
                    esClient.DeleteIndices(indicesToDelete);
                }
                _Logger.Debug($"Completed ElasticsearchIndexCleaner for gropu:[{groupId}], succcess.");
            }

            return true;
        }
    }
}
