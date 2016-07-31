using ApiObjects.SearchObjects;
using ElasticSearch.Common;
using ElasticSearch.Searcher;
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

        }

        public override bool Rebase()
        {
            bool result = false;

            int sizeOfBulk = TVinciShared.WS_Utils.GetTcmIntValue("ES_BULK_SIZE");
            int maxResults = TVinciShared.WS_Utils.GetTcmIntValue("MAX_RESULTS");

            // Default for size of bulk should be 50, if not stated otherwise in TCM
            if (sizeOfBulk == 0)
            {
                sizeOfBulk = 50;
            }

            // Default size of max results should be 100,000
            if (maxResults == 0)
            {
                maxResults = 100000;
            }

            GroupManager groupManager = new GroupManager();
            groupManager.RemoveGroup(groupId);
            Group group = groupManager.GetGroup(groupId);

            // Without the group we cannot advance at all - there must be an error in CB or something
            if (group == null)
            {
                log.ErrorFormat("Could not load group {0} in media index builder", groupId);
                return false;
            }

            string indexName = ElasticSearchTaskUtils.GetMediaGroupAliasStr(groupId);

            // Get ALL media in group
            Dictionary<int, Dictionary<int, Media>> groupMediasDictionary = ElasticsearchTasksCommon.Utils.GetGroupMediasTotal(groupId, 0);

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

                    int firstMediaId = groupMedias[firstIndex].Key;

                    HashSet<int> allIdsFromDB = new HashSet<int>();

                    // Create a list with all the IDs that were found in Database
                    for (int i = firstIndex; i < lastIndex; i++)
                    {
                        int currentId = groupMedias[i].Key;

                        allIdsFromDB.Add(currentId);
                    }

                    int lastMediaId = groupMedias[lastIndex].Key;

                    var groupMedia = groupMedias[firstIndex];

                    // For each language
                    foreach (int languageId in groupMedia.Value.Keys)
                    {
                        // Create a search range from the first ID to the last ID
                        ESRange range = new ESRange(true)
                        {
                            Key = "media_id"
                        };

                        range.Value.Add(new KeyValuePair<eRangeComp, string>(eRangeComp.GTE, firstMediaId.ToString()));
                        range.Value.Add(new KeyValuePair<eRangeComp, string>(eRangeComp.LTE, lastMediaId.ToString()));

                        ESQuery query = new ESQuery(range)
                        {
                            Size = maxResults
                        };

                        string queryString = query.ToString();
                        string documentType = ElasticSearchTaskUtils.GetTanslationType(MEDIA, group.GetLanguage(languageId));

                        // Perform search: id > first_id AND id < last_id                            
                        string searchResultString = api.Search(indexName, documentType, ref queryString);

                        // Parse results to workable list
                        int totalItems = 0;
                        var searchResults = Catalog.ElasticsearchWrapper.DecodeAssetSearchJsonObject(searchResultString, ref totalItems, null, "_source");

                        List<int> assetsToDelete = new List<int>();
                        List<int> assetsToUpdate = new List<int>();

                        foreach (var currentAsset in searchResults)
                        {
                            int assetId = currentAsset.asset_id;
                            DateTime updateDateES = currentAsset.update_date;

                            // If it is not contained in list of IDs from DB - this means this media is deleted and should be deleted from ES as well
                            if (!allIdsFromDB.Contains(assetId))
                            {
                                assetsToDelete.Add(assetId);
                            }
                            else
                            {
                                // Otherwise - remove it from this list, so eventually it will contain only the assets that are in DB but not in ES
                                allIdsFromDB.Remove(assetId);

                                string updateDateDBString = groupMediasDictionary[assetId][languageId].m_sUpdateDate;

                                DateTime updateDateDB = DateTime.ParseExact(updateDateDBString, "yyyyMMddHHmmss", null);

                                // Compare the dates - if the DB was updated after the ES was updated, this means we need to update ES
                                if (updateDateDB > updateDateES)
                                {
                                    assetsToUpdate.Add(assetId);
                                }
                            }
                        }

                        List<ESBulkRequestObj<int>> bulkRequests = new List<ESBulkRequestObj<int>>();

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

                        // create a request for each updated asset
                        foreach (var currentAsset in assetsToUpdate)
                        {
                            var media = groupMediasDictionary[currentAsset][languageId];
                            string serializedMedia = serializer.SerializeMediaObject(media);

                            ESBulkRequestObj<int> currentRequest = new ESBulkRequestObj<int>()
                            {
                                docID = currentAsset,
                                document = serializedMedia,
                                index = indexName,
                                Operation = eOperation.update,
                                type = documentType
                            };

                            bulkRequests.Add(currentRequest);
                        }

                        // create a request for each new asset
                        foreach (var assetId in allIdsFromDB)
                        {
                            var media = groupMediasDictionary[assetId][languageId];
                            string serializedMedia = serializer.SerializeMediaObject(media);

                            ESBulkRequestObj<int> currentRequest = new ESBulkRequestObj<int>()
                            {
                                docID = assetId,
                                document = serializedMedia,
                                index = indexName,
                                Operation = eOperation.index,
                                type = documentType
                            };

                            bulkRequests.Add(currentRequest);
                        }

                        // Perform bulk requests if there are any
                        if (bulkRequests.Count > 0)
                        {
                            var bulkResults = api.CreateBulkRequest<int>(bulkRequests);
                        }
                    }

                    // move on to the new index
                    firstIndex = lastIndex;
                }
            }

            return result;
        }
    }
}
