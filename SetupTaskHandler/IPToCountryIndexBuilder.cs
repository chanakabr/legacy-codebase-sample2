using ElasticSearch.Common;
using ElasticSearchHandler;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SetupTaskHandler
{
    public class IPToCountryIndexBuilder
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        
        protected BaseESSeralizer serializer;
        protected ElasticSearchApi api;

        public IPToCountryIndexBuilder()
        {
            serializer = null;
            api = new ElasticSearchApi();
        }

        public bool BuildIndex(string elasticSearchUrl = "", int version = 1)
        {
            bool result = false;

            if (!string.IsNullOrEmpty(elasticSearchUrl))
            {
                api.baseUrl = elasticSearchUrl;
            }

            switch (version)
            {
                case 1:
                {
                    serializer = new ESSerializerV1();
                    break;
                }
                case 2:
                {
                    serializer = new ESSerializerV2();
                    break;
                }
                default:
                {
                    serializer = new ESSerializerV1();
                    break;
                }
            }

            string newIndex = "utils";
            string type = "iptocountry";

            string numberOfShards = ElasticSearchTaskUtils.GetTcmConfigValue("ES_NUM_OF_SHARDS");
            string numberOfReplicas = ElasticSearchTaskUtils.GetTcmConfigValue("ES_NUM_OF_REPLICAS");

            int numOfShards;
            int numOfReplicas;

            int.TryParse(numberOfReplicas, out numOfReplicas);
            int.TryParse(numberOfShards, out numOfShards);

            try
            {
                bool indexExists = api.IndexExists(newIndex);

                if (!indexExists)
                {
                    indexExists = api.BuildIndex(newIndex, numOfShards, numOfReplicas, new List<string>(), new List<string>());
                }

                DataTable mappingTable = DAL.ApiDAL.Get_IPToCountryTable();

                if (mappingTable != null)
                {
                    List<ESBulkRequestObj<int>> bulkObjects = new List<ESBulkRequestObj<int>>();

                    foreach (DataRow row in mappingTable.Rows)
                    {

                        string serializedMapping = SerializeMapping(row);
                        int id = ODBCWrapper.Utils.ExtractInteger(row, "ID");

                        bulkObjects.Add(new ESBulkRequestObj<int>()
                        {
                            docID = id,
                            index = newIndex,
                            type = type,
                            document = serializedMapping
                        });

                        if (bulkObjects.Count >= 5000)
                        {
                            Task<object> t = Task<object>.Factory.StartNew(() => api.CreateBulkRequest(bulkObjects));
                            t.Wait();
                            bulkObjects = new List<ESBulkRequestObj<int>>();
                        }
                    }

                    if (bulkObjects.Count > 0)
                    {
                        Task<object> t = Task<object>.Factory.StartNew(() => api.CreateBulkRequest(bulkObjects));
                        t.Wait();
                    }
                }

                result = true;
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Failed building ip to country index. reason = {0}", ex);

                result = false;
            }


            return result;
        }

        private string SerializeMapping(DataRow row)
        {
            string result = string.Empty;

            long ipFrom = ODBCWrapper.Utils.ExtractValue<long>(row, "IP_FROM");
            long ipTo = ODBCWrapper.Utils.ExtractValue<long>(row, "IP_TO");
            int countryId = ODBCWrapper.Utils.ExtractInteger(row, "COUNTRY_ID");
            string code = ODBCWrapper.Utils.ExtractString(row, "COUNTRY_CD2");
            string name = ODBCWrapper.Utils.ExtractString(row, "COUNTRY_NAME");

            result = string.Concat("{",
                string.Format("\"ip_from\": {0}, \"ip_to\": {1}, \"country_id\": {2}, \"code\": \"{3}\", \"name\": \"{4}\" ", 
                                ipFrom,             ipTo,           countryId,          code,           name),
                "}");

            return result;
        }
    }
}
