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
            log.DebugFormat("Started rebase index EPG for group {0}", this.groupId);

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

            log.DebugFormat("Rebase index - GetGroupEpgs return {0} epg", groupEpgs.Count);

            int updatedDocuments = 0;
            int deletedDocuments = 0;

            if (groupEpgs != null && groupEpgs.Count > 0)
            {
                Dictionary<ulong, Dictionary<string, EpgCB>> epgDictionary = new Dictionary<ulong, Dictionary<string, EpgCB>>();

                string defaultLanguageCode = group.GetGroupDefaultLanguage().Code;

                foreach (var epg in groupEpgs)
                {
                    if (!epgDictionary.ContainsKey(epg.EpgID))
                    {
                        epgDictionary.Add(epg.EpgID, new Dictionary<string, EpgCB>());
                    }

                    string languageCode = epg.Language;

                    if (string.IsNullOrEmpty(languageCode))
                    {
                        languageCode = defaultLanguageCode;
                    }

                    epgDictionary[epg.EpgID][languageCode] = epg;
                }
                
                //groupEpgs.ToDictionary<EpgCB, ulong>(epg => epg.EpgID);
                groupEpgs = groupEpgs.OrderBy(epg => epg.EpgID).ToList();

                DeleteEdgeDocuments(languages, indexName, groupEpgs);

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
                        lastIndex = groupEpgs.Count;
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

                    bool isFirstRun = firstIndex == 0;

                    int skip = 1;

                    if (isFirstRun)
                    {
                        skip = 0;
                    }
                        
                    // Create a list with all the IDs that were found in Database
                    for (int i = firstIndex + skip; i < lastIndex; i++)
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
                        ulong lastEpgId = groupEpgs[lastIndex - 1].EpgID;
                        string documentType = ElasticSearchTaskUtils.GetTanslationType(EPG, group.GetLanguage(languageId)).ToLower();

                        List<ElasticSearchApi.ESAssetDocument> searchResults =
                            GetRangedDocuments(indexName, firstEpgId.ToString(), lastEpgId.ToString(), "epg_id", documentType, isFirstRun, allIdsFromDB.Count);

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
                                deletedDocuments++;
                            }
                            else
                            {
                                // Otherwise - remove it from this list, so eventually it will contain only the assets that are in DB but not in ES
                                // assets that will eventually remain in this list are assets that exist in DB and not in ES
                                allIdsFromDB.Remove(assetId);

                                if (epgDictionary.ContainsKey(assetId))
                                {
                                    DateTime updateDateDB = epgDictionary[assetId][language.Code].UpdateDate;
                                    bool isDBActive = epgDictionary[assetId][language.Code].isActive;

                                    // Compare the dates - if the DB was updated after the ES was updated, this means we need to update ES
                                    if ((updateDateDB.Subtract(updateDateES) > minimumTimeSpan) ||
                                        // or is the is_active is different between them
                                        (isESActive != isDBActive))
                                    {
                                        assetsToUpdate.Add(assetId);
                                        updatedDocuments++;
                                    }
                                }
                            }
                        }

                        // create a request for each new asset
                        // assets that were left in this list are assets that exist in DB and not in ES
                        foreach (var assetId in allIdsFromDB)
                        {
                            assetsToUpdate.Add(assetId);
                            updatedDocuments++;
                        }
                    }

                    var assetsToUpdateList = assetsToUpdate.Select(i => (int)i).ToList();
                    var assetsToDeleteList = assetsToDelete.Select(i => (int)i).ToList();

                    IssueUpdatesAndDeletes(assetsToDeleteList, assetsToUpdateList);

                    // move on to the new index
                    firstIndex = lastIndex - 1;
                }
            }

            log.DebugFormat("Rebase EPG index of group {0} finished. Updated documents = {1}, deleted documents = {2}", groupId, updatedDocuments, deletedDocuments);

            return result;
        }


        /// <summary>
        /// Deletes all documents in ES that their IDs are lower than the minimum ID in DB or higher than the maximum ID in DB
        /// </summary>
        /// <param name="languages"></param>
        /// <param name="indexName"></param>
        /// <param name="groupEpgs"></param>
        private void DeleteEdgeDocuments(List<ApiObjects.LanguageObj> languages, string indexName, List<EpgCB> groupEpgs)
        {
            ulong minimumId = 0;
            ulong maximumId = int.MaxValue;

            if (groupEpgs.Count > 0)
            {
                minimumId = groupEpgs[0].EpgID;
                maximumId = groupEpgs[groupEpgs.Count - 1].EpgID;
            }

            try
            {
                // For each language
                foreach (var language in languages)
                {
                    string documentType = ElasticSearchTaskUtils.GetTanslationType(EPG, language);

                    // Create a search range smaller than the first ID or larger than the last ID
                    ESRange range1 = new ESRange(true)
                    {
                        Key = "epg_id"
                    };

                    ESRange range2 = new ESRange(true)
                    {
                        Key = "epg_id"
                    };

                    range1.Value.Add(new KeyValuePair<eRangeComp, string>(eRangeComp.LT, minimumId.ToString()));
                    range2.Value.Add(new KeyValuePair<eRangeComp, string>(eRangeComp.GT, maximumId.ToString()));

                    BoolQuery boolQuery = new BoolQuery();

                    boolQuery.AddChild(range1, CutWith.OR);
                    boolQuery.AddChild(range2, CutWith.OR);

                    ESQuery query = new ESQuery(boolQuery)
                    {
                        Size = maxResults,
                        Fields = new List<string>()
                            {
                                "epg_id",
                                "update_date",
                                "is_active"
                            }
                    };

                    string queryString = query.ToString();

                    // Perform delete by query : id ≤ first_id OR id  ≥ last_id
                    bool deleteResult = api.DeleteDocsByQuery(indexName, documentType, ref queryString);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Rebase index for group {0}. Failed when deleting documents bigger than max ID or smaller than min ID", groupId);
            }
        }
    }
}
