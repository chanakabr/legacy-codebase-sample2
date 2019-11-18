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
                var indicesWithAliases = indices.Where(i => !i.Aliases.Any()).Select(i => i.Name).ToList();

                if (oldIndicesToSaveCount < 0)
                {
                    _Logger.Error($"ElasticsearchIndexCleaner oldIndicesToSaveCount:[{oldIndicesToSaveCount}] shouold be greater to or equal to 0");
                    return false;
                }
                if (oldIndicesToSaveCount >= indicesWithAliases.Count)
                {
                    _Logger.Info($"ElasticsearchIndexCleaner oldIndicesToSaveCount:[{oldIndicesToSaveCount}] is greater or equal to the amount of exsisting backups, existing without deletion. indicesToDelete.Count:[{indicesWithAliases.Count}]");
                    return true;
                }

                var indicesByDate = new Dictionary<DateTime, List<string>>();
                foreach (var item in indicesWithAliases)
                {
                    var date = GetDateTimeFromIndexName(item);
                    if (!indicesByDate.ContainsKey(date))
                    {
                        indicesByDate.Add(date, new List<string>());
                    }

                    indicesByDate[date].Add(item);
                }

                var indicesToDelete = new List<string>();
                foreach (var item in indicesByDate)
                {
                    if (item.Value.Count > oldIndicesToSaveCount)
                    {
                        var counter = item.Value.ToDictionary(key => int.Parse(key.Split('_').Last()), value => value);
                        var sorted = new SortedDictionary<int, string>(counter, Comparer<int>.Default);
                        indicesToDelete.AddRange(sorted.Take(oldIndicesToSaveCount).Select(x => x.Value));
                    }
                }

                var indicesToDeleteStr = string.Join(",", indicesToDelete);
                _Logger.Debug($"Deleting indices:[{indicesToDeleteStr}] ");
                using (var mon = new KMonitor(Events.eEvent.EVENT_ELASTIC, groupId.ToString(), "delete_old_epg_indices"))
                {
                    mon.Table = $"indices_to_delete:[{indicesToDeleteStr}]";
                    esClient.DeleteIndices(indicesWithAliases);
                }
                _Logger.Debug($"Completed ElasticsearchIndexCleaner for gropu:[{groupId}], succcess.");
            }

            return true;
        }

        private static DateTime GetDateTimeFromIndexName(string str)
        {
            var sDate = str.Split('_')[3];

            var year = int.Parse(sDate.Substring(0, 4));
            var month = int.Parse(sDate.Substring(4, 2));
            var day = int.Parse(sDate.Substring(6, 2));

            return new DateTime(year, month, day);
        }
    }
}
