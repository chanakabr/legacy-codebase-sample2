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

        public bool Clean(IEnumerable<int> groupIds, int oldIndicesToSaveCount)
        {
            var esClient = new ElasticSearchApi();

            foreach (var groupId in groupIds)
            {
                _Logger.Info($"Starting ElasticsearchIndexCleaner for group:[{groupId}], oldIndicesToSaveCount:[{oldIndicesToSaveCount}]");
                var deleteCandidateIndices = esClient.ListIndicesByAlias(ESUtils.DELETE_CANDIDATE_ALIAS)
                    .Select(x => x.Name).ToList();
                    
                if (oldIndicesToSaveCount < 0)
                {
                    _Logger.Error($"ElasticsearchIndexCleaner oldIndicesToSaveCount:[{oldIndicesToSaveCount}] should be greater to or equal to 0");
                    return false;
                }
                if (oldIndicesToSaveCount >= deleteCandidateIndices.Count)
                {
                    _Logger.Info($"ElasticsearchIndexCleaner oldIndicesToSaveCount:[{oldIndicesToSaveCount}] is greater or equal to the amount of exsisting backups, existing without deletion. indicesToDelete.Count:[{deleteCandidateIndices.Count}]");
                    return true;
                }

                var indicesByDate = new Dictionary<DateTime, List<string>>();
                foreach (var item in deleteCandidateIndices)
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
                    if (item.Value.Count > oldIndicesToSaveCount)
                    {
                        var counter = item.Value.ToDictionary(key => int.Parse(key.Split('_').Last()), value => value);
                        var sorted = new SortedDictionary<int, string>(counter, Comparer<int>.Default);
                        indicesToDelete.AddRange(sorted.Take(sorted.Count - oldIndicesToSaveCount).Select(x => x.Value));
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
