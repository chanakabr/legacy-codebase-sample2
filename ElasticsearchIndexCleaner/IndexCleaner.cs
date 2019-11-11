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
                _Logger.Info($"Starting ElasticsearchIndexCleaner for gropu:[{groupId}]");
                var indices = esClient.ListIndices($"{groupId}_epg_*");
                var indicesToDelete = indices.Where(i => !i.Aliases.Any()).Select(i => i.Name).ToList();

                if (oldIndicesToSaveCount > 0 && oldIndicesToSaveCount < indicesToDelete.Count)
                {
                    var counter = indicesToDelete.ToDictionary(x => int.Parse(x.Split('_').Last()), y => y);
                    var sorted = new SortedDictionary<int, string>(counter, Comparer<int>.Default);
                    indicesToDelete = sorted.Take(indicesToDelete.Count - oldIndicesToSaveCount).Select(x => x.Value).ToList();
                }

                var indicesToDeleteStr = string.Join(",", indicesToDelete);
                _Logger.Debug($"Deleting indices:[{indicesToDeleteStr}] ");
                using (var mon = new KMonitor(Events.eEvent.EVENT_ELASTIC, groupId.ToString(), "delete_old_epg_indices"))
                {
                    mon.Table = $"indices_to_delete:[{indicesToDeleteStr}]";
                    esClient.DeleteIndices(indicesToDelete);
                }
                _Logger.Debug($"Deleted!");
            }

            return true;
        }
    }
}
