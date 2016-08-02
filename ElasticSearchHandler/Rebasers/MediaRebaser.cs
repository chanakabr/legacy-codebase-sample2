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
            //Dictionary<int, Dictionary<int, Media>> groupMediasDictionary = ElasticsearchTasksCommon.Utils.GetGroupMediasTotal(groupId, 0);
            var groupMediasDictionary = ElasticsearchTasksCommon.Utils.GetRebaseMediaInformation(groupId);

            // Order all media by their ID
            var groupMedias = groupMediasDictionary.OrderBy(asset => asset.Key).ToList();

            if (groupMedias != null)
            {
                bool isDone = false;
                int firstIndex = 0;
                int lastIndex = 0;

                // as long as there are more assets to cover
                while (!isDone)
                {
                    // If we reached the limit - this is the last bulk
                    if (firstIndex + sizeOfBulk > groupMedias.Count)
                    {
                        // The current bulk will be until the end of the list/dictionary
                        lastIndex = groupMedias.Count - 1;
                        isDone = true;

                        // mark this rebase as successful
                        result = true;
                    }
                    else
                    {
                        // the current bulk will be in the size of the predefined TCM value
                        lastIndex = firstIndex + sizeOfBulk;
                    }

                    HashSet<int> allIdsFromDB = new HashSet<int>();

                    // Create a list with all the IDs that were found in Database
                    for (int i = firstIndex; i < lastIndex; i++)
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
                        int languageId = language.ID;

                        int firstMediaId = groupMedias[firstIndex].Key;
                        int lastMediaId = groupMedias[lastIndex].Key;
                        string documentType = ElasticSearchTaskUtils.GetTanslationType(MEDIA, group.GetLanguage(languageId));

                        List<ElasticSearchApi.ESAssetDocument> searchResults =
                            GetRangedDocuments(indexName, firstMediaId.ToString(), lastMediaId.ToString(), "media_id", documentType);

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
                        }
                    }

                    IssueUpdatesAndDeletes<int>(bulkRequests, assetsToUpdate.ToList());

                    // move on to the new index
                    firstIndex = lastIndex;
                }
            }

            return result;
        }
    }
}