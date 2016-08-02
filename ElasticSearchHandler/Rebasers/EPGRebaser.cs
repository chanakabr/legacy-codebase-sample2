using ApiObjects;
using ApiObjects.SearchObjects;
using ElasticSearch.Common;
using ElasticSearch.Searcher;
using ElasticSearchHandler.Updaters;
using EpgBL;
using GroupsCacheManager;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ElasticSearchHandler
{
    public class EPGRebaser : AbstractIndexRebaser
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private static readonly string EPG = "EPG";
        
        public EPGRebaser(int groupId)
            : base(groupId)
        {
            updater = new EpgUpdaterV2(groupId);
        }

        public override bool Rebase()
        {
            bool result = false;

            GroupManager groupManager = new GroupManager();
            groupManager.RemoveGroup(groupId);
            Group group = groupManager.GetGroup(groupId);

            var languages = group.GetLangauges();

            // Without the group we cannot advance at all - there must be an error in CB or something
            if (group == null)
            {
                log.ErrorFormat("Could not load group {0} in epg index rebaser", groupId);
                return false;
            }

            string indexName = ElasticSearchTaskUtils.GetEpgGroupAliasStr(groupId);

            TvinciEpgBL epgBL = new TvinciEpgBL(groupId);
            var groupEpgs = epgBL.GetGroupEpgs(maxResults, 0, null, null);

            if (groupEpgs != null)
            {
                Dictionary<ulong, EpgCB> dictionary = groupEpgs.ToDictionary<EpgCB, ulong>(epg => epg.EpgID);
                groupEpgs = groupEpgs.OrderBy(epg => epg.EpgID).ToList();

                bool isDone = false;
                int firstIndex = 0;
                int lastIndex = 0;

                // as long as there are more assets to cover
                while (!isDone)
                {
                    // If we reached the limit - this is the last bulk
                    if (firstIndex + sizeOfBulk > groupEpgs.Count)
                    {
                        // The current bulk will be until the end of the list/dictionary
                        lastIndex = groupEpgs.Count - 1;
                        isDone = true;

                        // mark this rebase as successful
                        result = true;
                    }
                    else
                    {
                        // the current bulk will be in the size of the predefined TCM value
                        lastIndex = firstIndex + sizeOfBulk;
                    }

                    HashSet<ulong> allIdsFromDB = new HashSet<ulong>();

                    // Create a list with all the IDs that were found in Database
                    for (int i = firstIndex; i < lastIndex; i++)
                    {
                        ulong currentId = groupEpgs[i].EpgID;

                        allIdsFromDB.Add(currentId);
                    }

                    List<ESBulkRequestObj<ulong>> bulkRequests = new List<ESBulkRequestObj<ulong>>();
                    HashSet<ulong> assetsToDelete = new HashSet<ulong>();
                    HashSet<ulong> assetsToUpdate = new HashSet<ulong>();

                    // For each language
                    foreach (var language in languages)
                    {
                        int languageId = language.ID;

                        // Create a search range from the first ID to the last ID
                        ESRange range = new ESRange(true)
                        {
                            Key = "epg_id"
                        };

                        ulong firstEpgId = groupEpgs[firstIndex].EpgID;
                        ulong lastEpgId = groupEpgs[lastIndex].EpgID;
                        string documentType = ElasticSearchTaskUtils.GetTanslationType(EPG, group.GetLanguage(languageId));

                        List<ElasticSearchApi.ESAssetDocument> searchResults =
                            GetRangedDocuments(indexName, firstEpgId.ToString(), lastEpgId.ToString(), "epg_id", documentType);

                        foreach (var currentAsset in searchResults)
                        {
                            ulong assetId = (ulong)currentAsset.asset_id;
                            DateTime updateDateES = currentAsset.update_date;
                            string isESActiveString;
                            currentAsset.extraReturnFields.TryGetValue("is_active", out isESActiveString);
                            bool isESActive = isESActiveString == "1";

                            // If it is not contained in list of IDs from DB - 
                            // this means this media is deleted and should be deleted from ES as well
                            if (!allIdsFromDB.Contains(assetId))
                            {
                                assetsToDelete.Add(assetId);
                            }
                            else
                            {
                                // Otherwise - remove it from this list, so eventually it will contain only the assets that are in DB but not in ES
                                // assets that will eventually remain in this list are assets that exist in DB and not in ES
                                allIdsFromDB.Remove(assetId);

                                if (dictionary.ContainsKey(assetId))
                                {
                                    DateTime updateDateDB = dictionary[assetId].UpdateDate;
                                    bool isDBActive = dictionary[assetId].isActive;

                                    // Compare the dates - if the DB was updated after the ES was updated, this means we need to update ES
                                    if ((updateDateDB.Subtract(updateDateES) > minimumTimeSpan) ||
                                        // or is the is_active is different between them
                                        (isESActive != isDBActive))
                                    {
                                        assetsToUpdate.Add(assetId);
                                    }
                                }
                            }
                        }

                        // create a request for each deleted asset
                        foreach (var currentAsset in assetsToDelete)
                        {
                            ESBulkRequestObj<ulong> currentRequest = new ESBulkRequestObj<ulong>()
                            {
                                docID = currentAsset,
                                document = string.Empty,
                                index = indexName,
                                Operation = eOperation.delete,
                                type = documentType
                            };

                            bulkRequests.Add(currentRequest);
                        }

                        // create a request for each new asset
                        // assets that were left in this list are assets that exist in DB and not in ES
                        foreach (var assetId in allIdsFromDB)
                        {
                            assetsToUpdate.Add(assetId);
                        }
                    }

                    var assetsToUpdateList = assetsToUpdate.Select(i => (int)i).ToList();

                    IssueUpdatesAndDeletes(bulkRequests, assetsToUpdateList);

                    // move on to the new index
                    firstIndex = lastIndex;
                }
            }

            return result;
        }
    }
}
