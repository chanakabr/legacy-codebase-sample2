using ElasticSearch.Common;
using ElasticSearch.Searcher;
using ElasticSearchHandler.Updaters;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ElasticSearchHandler
{
    public class AbstractIndexRebaser
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        
        protected int groupId;
        protected ElasticSearchApi api;
        protected BaseESSeralizer serializer;
        protected IElasticSearchUpdater updater;
        protected int maxResults;
        protected int sizeOfBulk;
        /// <summary>
        ///  Minimum time span to consider that there was a real change
        ///  (because ES update date has no milliseconds)
        /// </summary>
        protected readonly TimeSpan minimumTimeSpan = new TimeSpan(0, 0, 0, 10, 62);

        public string Url
        {
            set
            {
                if (api != null)
                {
                    api.baseUrl = value;
                }
            }
        }

        public AbstractIndexRebaser(int groupId)
        {
            this.groupId = groupId;
            api = new ElasticSearchApi();
            serializer = new ESSerializerV2();
            updater = null;

            sizeOfBulk = TVinciShared.WS_Utils.GetTcmIntValue("ES_BULK_SIZE");
            maxResults = TVinciShared.WS_Utils.GetTcmIntValue("MAX_RESULTS");

            // Default for size of bulk should be 1000, if not stated otherwise in TCM
            if (sizeOfBulk == 0)
            {
                sizeOfBulk = 1000;
            }

            // Default size of max results should be 100,000
            if (maxResults == 0)
            {
                maxResults = 100000;
            }
        }

        public virtual bool Rebase()
        {
            bool result = false;
            
            return result;
        }

        protected List<ElasticSearchApi.ESAssetDocument> GetRangedDocuments(string indexName, 
            string firstId, string lastId, string idField, string documentType, bool isFirstRun, int countOfOriginalIds)
        {
            // Create a search range from the first ID to the last ID
            ESRange range = new ESRange(true)
            {
                Key = idField
            };

            eRangeComp comparison =eRangeComp.GT;

            if (isFirstRun)
            {
                comparison = eRangeComp.GTE;
            }

            range.Value.Add(new KeyValuePair<eRangeComp, string>(comparison, firstId));
            range.Value.Add(new KeyValuePair<eRangeComp, string>(eRangeComp.LTE, lastId));

            ESQuery query = new ESQuery(range)
            {
                Size = maxResults,
                Fields = new List<string>()
                            {
                                idField,
                                "update_date",
                                "is_active"
                            }
            };

            string queryString = query.ToString();

            // Perform search: id ≥ first_id AND id ≤ last_id
            string searchResultString = api.Search(indexName, documentType, ref queryString);

            // Parse results to workable list
            int totalItems = 0;
            List<string> extraField = new List<string>() { "is_active" };

            List<ElasticSearchApi.ESAssetDocument> searchResults =
                Catalog.ElasticsearchWrapper.DecodeAssetSearchJsonObject(searchResultString, ref totalItems, extraField);

            int count = 0;

            if (searchResults != null)
            {
                count = searchResults.Count;
            }

            log.DebugFormat("Get ranged documents for index {0}, first ID = {1}, last ID = {2}, bulk size = {3}, search result count = {4}",
                indexName, firstId, lastId, countOfOriginalIds, count);

            return searchResults;
        }

        protected void IssueUpdatesAndDeletes<T>(List<ESBulkRequestObj<T>> bulkRequests, List<int> assetsToUpdate)
        {
            // Perform bulk requests if there are any
            if (bulkRequests.Count > 0)
            {
                var bulkResults = api.CreateBulkRequest<T>(bulkRequests);
            }

            // Call media updater for the media that needs an update
            if (assetsToUpdate.Count > 0)
            {
                updater.Action = ApiObjects.eAction.Update;
                updater.IDs = assetsToUpdate;
                updater.Start();
            }
        }
    }
}
