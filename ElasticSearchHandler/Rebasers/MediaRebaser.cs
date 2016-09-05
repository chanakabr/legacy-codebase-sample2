using ApiObjects.SearchObjects;
using ElasticSearch.Common;
using ElasticSearch.Searcher;
using ElasticSearchHandler.Updaters;
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
    public class MediaRebaser : AbstractIndexRebaser
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private static readonly string MEDIA = "media";

        public MediaRebaser(int groupId)
            : base(groupId)
        {
            updater = new MediaUpdaterV2(groupId);
        }

        public override bool Rebase()
        {
            log.DebugFormat("Started rebase index media for group {0}", this.groupId);

            bool result = false;

            GroupManager groupManager = new GroupManager();
            groupManager.RemoveGroup(groupId);
            Group group = groupManager.GetGroup(groupId);

            var languages = group.GetLangauges();

            // Without the group we cannot advance at all - there must be an error in CB or something
            if (group == null)
            {
                log.ErrorFormat("Could not load group {0} in media index rebaser", groupId);
                return false;
            }

            string indexName = ElasticSearchTaskUtils.GetMediaGroupAliasStr(groupId);

            // Get ALL media in group
            var groupMediasDictionary = ElasticsearchTasksCommon.Utils.GetRebaseMediaInformation(groupId);

            if (groupMediasDictionary == null)
            {
                log.ErrorFormat("Rebase index - index - Get_GroupMedias_Rebase for group {0} return null result", groupId);

                return false;
            }
            
            // Order all media by their ID
            var groupMedias = groupMediasDictionary.OrderBy(asset => asset.Key).ToList();

            log.DebugFormat("Rebase index - Get_GroupMedias_Rebase for group {0} return {1} media", groupId, groupMedias.Count);

            int updatedDocuments = 0;
            int deletedDocuments = 0;

            if (groupMedias != null && groupMedias.Count > 0)
            {
                // Media that exist in ES but not in DB - with IDs outside of DB's range
                DeleteEdgeDocuments(languages, indexName, groupMedias);
                
                bool isDone = false;
                int firstIndex = 0;
                int lastIndex = 0;

                // as long as there are more assets to cover
                while (!isDone)
                {
                    // If we reached the limit - this is the last bulk
                    if (firstIndex + sizeOfBulk >= groupMedias.Count)
                    {
                        // The current bulk will be until the end of the list/dictionary
                        lastIndex = groupMedias.Count;
                        isDone = true;

                        // mark this rebase as successful
                        result = true;
                    }
                    else
                    {
                        // the current bulk will be in the size of the predefined TCM value
                        lastIndex = firstIndex + sizeOfBulk;
                    }

                    try
                    {
                        HashSet<int> allIdsFromDB = new HashSet<int>();

                        bool isFirstRun = firstIndex == 0;

                        int skip = 1;

                        if (isFirstRun)
                        {
                            skip = 0;
                        }
                        
                        // Create a list with all the IDs that were found in Database
                        for (int i = firstIndex + skip; i < lastIndex; i++)
                        {
                            int currentId = groupMedias[i].Key;

                            allIdsFromDB.Add(currentId);
                        }

                        List<ESBulkRequestObj<int>> bulkRequests = new List<ESBulkRequestObj<int>>();
                        HashSet<int> assetsToDelete = new HashSet<int>();
                        HashSet<int> assetsToUpdate = new HashSet<int>();

                        // For each language
                        foreach (var language in languages)
                        {
                            int firstMediaId = groupMedias[firstIndex].Key;
                            int lastMediaId = groupMedias[lastIndex - 1].Key;
                            string documentType = ElasticSearchTaskUtils.GetTanslationType(MEDIA, language);

                            List<ElasticSearchApi.ESAssetDocument> searchResults =
                                GetRangedDocuments(indexName, firstMediaId.ToString(), lastMediaId.ToString(), "media_id", documentType, isFirstRun, allIdsFromDB.Count);

                            foreach (var currentAsset in searchResults)
                            {
                                int assetId = currentAsset.asset_id;
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

                                    if (groupMediasDictionary.ContainsKey(assetId))
                                    {
                                        DateTime updateDateDB = groupMediasDictionary[assetId].Value;
                                        bool isDBActive = groupMediasDictionary[assetId].Key;

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

                            // create a request for each deleted asset
                            foreach (var currentAsset in assetsToDelete)
                            {
                                ESBulkRequestObj<int> currentRequest = new ESBulkRequestObj<int>()
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
                                updatedDocuments++;
                            }
                        }

                        IssueUpdatesAndDeletes<int>(bulkRequests, assetsToUpdate.ToList());
                    }
                    catch (Exception ex)
                    {
                        log.ErrorFormat(
                            "Rebase media index of group {0} failed in bulk between INDEXES {1} and {2}. error = {3}", 
                            groupId, firstIndex, lastIndex, ex);
                    }

                    // move on to the new index
                    firstIndex = lastIndex - 1;
                }
            }

            log.DebugFormat("Rebase media index of group {0} finished. Updated documents = {1}, deleted documents = {2}", groupId, updatedDocuments, deletedDocuments);

            return result;
        }

        /// <summary>
        /// Deletes all documents in ES that their IDs are lower than the minimum ID in DB or higher than the maximum ID in DB
        /// </summary>
        /// <param name="languages"></param>
        /// <param name="indexName"></param>
        /// <param name="groupMedias"></param>
        private void DeleteEdgeDocuments(List<ApiObjects.LanguageObj> languages, string indexName, List<KeyValuePair<int, KeyValuePair<bool, DateTime>>> groupMedias)
        {
            int minimumId = 0;
            int maximumId = int.MaxValue;

            if (groupMedias.Count > 0)
            {
                minimumId = groupMedias[0].Key;
                maximumId = groupMedias[groupMedias.Count - 1].Key;
            }

            try
            {
                // For each language
                foreach (var language in languages)
                {
                    string documentType = ElasticSearchTaskUtils.GetTanslationType(MEDIA, language);

                    // Create a search range smaller than the first ID or larger than the last ID
                    ESRange range1 = new ESRange(true)
                    {
                        Key = "media_id"
                    };

                    ESRange range2 = new ESRange(true)
                    {
                        Key = "media_id"
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
                                "media_id",
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