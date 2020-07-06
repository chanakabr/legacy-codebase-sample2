using ApiObjects.Ingest;
using ElasticSearch.Common;
using KLogMonitor;
using Polly;
using Polly.Retry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ESUtils = ElasticSearch.Common.Utils;

namespace ElasticsearchIndexCleaner
{
    public class IndexCleaner
    {

        private static readonly KLogger _Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public bool Clean(IEnumerable<int> groupIds, int lastIndexBackupCount)
        {
            var esClient = new ElasticSearchApi();
            var purgeIndices = esClient.ListIndicesByAlias(IngestConsts.PURGE_INDEX_ALIAS)
                   .Select(x => x.Name).ToList();

            if (purgeIndices.Any())
            {
                _Logger.Info($"Found [{purgeIndices.Count}] purge indexes to delete, removing now");
                esClient.DeleteIndices(purgeIndices);
            }

            var lastIndexBackupCandidates = esClient.ListIndicesByAlias(IngestConsts.LAST_BACKUP_ALIAS)
                    .Select(x => x.Name).ToList();

            foreach (var groupId in groupIds)
            {
                _Logger.Info($"Starting ElasticsearchIndexCleaner for group:[{groupId}], lastIndexBackupCount:[{lastIndexBackupCount}]");
                
                    
                if (lastIndexBackupCount < 0)
                {
                    _Logger.Error($"ElasticsearchIndexCleaner lastIndexBackupCount:[{lastIndexBackupCount}] should be greater to or equal to 0");
                    return false;
                }
                if (lastIndexBackupCount >= lastIndexBackupCandidates.Count)
                {
                    _Logger.Info($"ElasticsearchIndexCleaner lastIndexBackupCount:[{lastIndexBackupCount}] is greater or equal to the amount of exsisting backups, existing without deletion. indicesToDelete.Count:[{lastIndexBackupCandidates.Count}]");
                    return true;
                }

                var indicesByDate = new Dictionary<DateTime, List<string>>();
                foreach (var item in lastIndexBackupCandidates)
                {
                    // There migth be some old indices that are not in the new naming convention we need to be fault tolerent
                    var dateResult = GetDateTimeFromIndexName(item);
                    if (!dateResult.HasValue) { continue; }
                    var date = dateResult.Value;

                    if (!indicesByDate.ContainsKey(date))
                    {
                        indicesByDate.Add(date, new List<string>());
                    }

                    indicesByDate[date].Add(item);
                }

                var indicesToDelete = new List<string>();
                foreach (var item in indicesByDate)
                {
                    if (item.Value.Count > lastIndexBackupCount)
                    {
                        var counter = item.Value.ToDictionary(key => int.Parse(key.Split('_').Last()), value => value);
                        var sorted = new SortedDictionary<int, string>(counter, Comparer<int>.Default);
                        indicesToDelete.AddRange(sorted.Take(sorted.Count - lastIndexBackupCount).Select(x => x.Value));
                    }
                }

                if (indicesToDelete.Any())
                {
                    var indicesToDeleteStr = string.Join(",", indicesToDelete);
                    _Logger.Debug($"Deleting indices:[{indicesToDeleteStr}] ");
                    using (var mon = new KMonitor(Events.eEvent.EVENT_ELASTIC, groupId.ToString(), "delete_old_epg_indices"))
                    {
                        mon.Table = $"indices_to_delete:[{indicesToDeleteStr}]";
                        esClient.DeleteIndices(indicesToDelete);
                    }
                }
                else
                {
                    _Logger.Debug($"No indices to delete");
                }

                _Logger.Debug($"Completed ElasticsearchIndexCleaner for group:[{groupId}], succcess.");
            }

            return true;
        }

        private static DateTime? GetDateTimeFromIndexName(string str)
        {
            var indexNameParts = str.Split('_');
            if (indexNameParts.Length < 4) { return null; }

            var sDate = indexNameParts[3];

            var year = int.Parse(sDate.Substring(0, 4));
            var month = int.Parse(sDate.Substring(4, 2));
            var day = int.Parse(sDate.Substring(6, 2));

            return new DateTime(year, month, day);
        }
    }
}
