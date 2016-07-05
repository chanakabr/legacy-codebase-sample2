using ElasticSearch.Common;
using ElasticSearch.Searcher;
using ElasticSearchHandler;
using KLogMonitor;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SetupTaskHandler
{
    public class StatisticsMigrationTool
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        protected const string STATS_TYPE = "stats";

        protected ESSerializerV1 oldSerializer;
        protected ESSerializerV2 newSerializer;
        protected ElasticSearchApi oldApi;
        protected ElasticSearchApi newApi;

        public StatisticsMigrationTool(string urlV1, string urlV2)
        {
            oldSerializer = new ESSerializerV1();
            newSerializer = new ESSerializerV2();

            oldApi = new ElasticSearchApi(urlV1);
            newApi = new ElasticSearchApi(urlV2);
        }

        internal bool Migrate(int groupId)
        {
            bool success = false;
            
            string index = ElasticSearch.Common.Utils.GetGroupStatisticsIndex(groupId);
            
            string numberOfShards = ElasticSearchTaskUtils.GetTcmConfigValue("ES_NUM_OF_SHARDS");
            string numberOfReplicas = ElasticSearchTaskUtils.GetTcmConfigValue("ES_NUM_OF_REPLICAS");
            string sizeOfBulkString = ElasticSearchTaskUtils.GetTcmConfigValue("ES_BULK_SIZE");

            int shards;
            int replicas;
            int sizeOfBulk;

            int.TryParse(numberOfReplicas, out replicas);
            int.TryParse(numberOfShards, out shards);
            int.TryParse(sizeOfBulkString, out sizeOfBulk);

            if (sizeOfBulk == 0)
            {
                sizeOfBulk = 1000;
            }

            if (!newApi.IndexExists(index))
            {
                newApi.BuildIndex(index, shards, replicas, new List<string>(), new List<string>());
            }

            int from = 0;

            ESMatchAllQuery matchAllQuery = new ESMatchAllQuery();
            ESQuery query = new ESQuery(matchAllQuery)
            {
                Size = sizeOfBulk
            };

            int totalItems = 0;

            // as long as we didn't cover all documents in old index
            while (from < totalItems || from == 0)
            {
                query.From = from;

                string searchQuery = query.ToString();

                // Peform current search
                var searchResult = oldApi.Search(index, STATS_TYPE, ref searchQuery);

                // Parse response to json object we can work with
                var json = JObject.Parse(searchResult);
                JToken tempToken = null;

                totalItems = ((tempToken = json.SelectToken("hits.total")) == null ? 0 : (int)tempToken);
                List<ESBulkRequestObj<string>> bulkRequests = new List<ESBulkRequestObj<string>>();

                if (totalItems > 0)
                {
                    var items = json.SelectToken("hits.hits");
                    from = from + items.Count();

                    foreach (var item in items)
                    {
                        string document = item["_source"].ToString(Newtonsoft.Json.Formatting.None).Replace("\n", "").Replace("\r","");
                        string id = item["_id"].ToString();

                        bulkRequests.Add(new ESBulkRequestObj<string>()
                        {
                            index = index,
                            type = STATS_TYPE,
                            Operation = eOperation.index,
                            document = document,
                            docID = id
                        });
                    }

                    newApi.CreateBulkRequest(bulkRequests);
                }
            }

            success = true;

            return success;
        }
    }
}
