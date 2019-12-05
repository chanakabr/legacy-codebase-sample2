using ConfigurationManager;
using ElasticSearch.Common;
using ElasticSearch.Utilities;
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
        
        public const string LOWERCASE_ANALYZER =
            "\"lowercase_analyzer\": {\"type\": \"custom\",\"tokenizer\": \"keyword\",\"filter\": [\"lowercase\"],\"char_filter\": [\"html_strip\"]}";

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

            string newIndexName = ElasticSearchTaskUtils.GetNewUtilsIndexString();
            string ipToCountryType = "iptocountry";
            string ipV6ToCountryType = "ipv6tocountry";

            int numOfShards = ApplicationConfiguration.Current.ElasticSearchHandlerConfiguration.NumberOfShards.Value;
            int numOfReplicas = ApplicationConfiguration.Current.ElasticSearchHandlerConfiguration.NumberOfReplicas.Value;
            
            try
            {
                bool indexExists = api.IndexExists(newIndexName);

                if (!indexExists)
                {
                    List<string> analyzers = new List<string>()
                    {
                        LOWERCASE_ANALYZER
                    };

                    indexExists = api.BuildIndex(newIndexName, numOfShards, numOfReplicas, analyzers, new List<string>());
                }

                #region Ip

                // Insert mapping for name field - default mapping isn't good for us
                ESMappingObj ipIndexMapping = new ESMappingObj(ipToCountryType);

                ipIndexMapping.AddProperty(new BasicMappingPropertyV2()
                {
                    name = "name",
                    type = eESFieldType.STRING,
                    index = eMappingIndex.analyzed,
                    analyzer = "lowercase_analyzer"
                });

                api.InsertMapping(newIndexName, ipToCountryType, ipIndexMapping.ToString());

                DataTable ipToCountryMapping = DAL.ApiDAL.Get_IPToCountryTable();

                if (ipToCountryMapping != null)
                {
                    List<ESBulkRequestObj<int>> bulkObjects = new List<ESBulkRequestObj<int>>();

                    foreach (DataRow row in ipToCountryMapping.Rows)
                    {

                        string serializedMapping = SerializeIPMapping(row);
                        int id = ODBCWrapper.Utils.ExtractInteger(row, "ID");

                        bulkObjects.Add(new ESBulkRequestObj<int>()
                        {
                            docID = id,
                            index = newIndexName,
                            type = ipToCountryType,
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

                #endregion

                #region IpV6

                // Insert mapping for name field - default mapping isn't good for us
                ESMappingObj ipV6IndexMapping = new ESMappingObj(ipV6ToCountryType);

                ipV6IndexMapping.AddProperty(new BasicMappingPropertyV2()
                {
                    name = "name",
                    type = eESFieldType.STRING,
                    index = eMappingIndex.analyzed,
                    analyzer = "lowercase_analyzer"
                });

                api.InsertMapping(newIndexName, ipV6ToCountryType, ipV6IndexMapping.ToString());

                DataTable ipV6ToCountryMapping = DAL.ApiDAL.GetIpv6ToCountryTable();

                if (ipV6ToCountryMapping != null)
                {
                    List<ESBulkRequestObj<int>> bulkObjects = new List<ESBulkRequestObj<int>>();

                    foreach (DataRow row in ipV6ToCountryMapping.Rows)
                    {

                        string serializedMapping = SerializeIPV6Mapping(row);
                        int id = ODBCWrapper.Utils.ExtractInteger(row, "ID");

                        bulkObjects.Add(new ESBulkRequestObj<int>()
                        {
                            docID = id,
                            index = newIndexName,
                            type = ipV6ToCountryType,
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

                #endregion

                // Switch index alias + Delete old indices handling
                string alias = "utils";
                bool currentIndexExists = api.IndexExists(alias);

                List<string> oldIndices = null;

                try
                {
                    oldIndices = api.GetAliases(alias);
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("Error when getting aliases of {0}, ex={1}", alias, ex);
                }

                Task<bool> taskSwitchIndex = Task<bool>.Factory.StartNew(() =>
                {
                    return api.SwitchIndex(newIndexName, alias, oldIndices);
                });

                taskSwitchIndex.Wait();

                if (!taskSwitchIndex.Result)
                {
                    log.ErrorFormat("Failed switching index for new index name = {0}, index alias = {1}", newIndexName, alias);
                    result = false;
                }

                if (taskSwitchIndex.Result && oldIndices != null && oldIndices.Count > 0)
                {
                    Task t = Task.Factory.StartNew(() =>
                    {
                        api.DeleteIndices(oldIndices);
                    });

                    t.Wait();
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

        private string SerializeIPMapping(DataRow row)
        {
            string result = string.Empty;

            long ipFrom = ODBCWrapper.Utils.ExtractValue<long>(row, "IP_FROM");
            long ipTo = ODBCWrapper.Utils.ExtractValue<long>(row, "IP_TO");
            int countryId = ODBCWrapper.Utils.ExtractInteger(row, "COUNTRY_ID");
            string code = ODBCWrapper.Utils.ExtractString(row, "COUNTRY_CD2");
            string name = ODBCWrapper.Utils.ExtractString(row, "COUNTRY_NAME");
            name = ElasticSearch.Common.Utils.ReplaceDocumentReservedCharacters(name, false);

            result = string.Concat("{",
                string.Format("\"ip_from\": {0}, \"ip_to\": {1}, \"country_id\": {2}, \"code\": \"{3}\", \"name\": \"{4}\" ", 
                                ipFrom,             ipTo,           countryId,          code,           name),
                "}");

            return result;
        }

        private string SerializeIPV6Mapping(DataRow row)
        {
            string result = string.Empty;

            string network = ODBCWrapper.Utils.ExtractValue<string>(row, "NETWORK");
            int countryId = ODBCWrapper.Utils.ExtractInteger(row, "COUNTRY_ID");
            string code = ODBCWrapper.Utils.ExtractString(row, "COUNTRY_CD2");
            string name = ODBCWrapper.Utils.ExtractString(row, "COUNTRY_NAME");
            name = ElasticSearch.Common.Utils.ReplaceDocumentReservedCharacters(name, false);

            var tuple = IpToCountry.GetIpRangesByNetwork(network);
            IPV6 ipv6 = new IPV6(tuple, countryId, code, name);

            return Newtonsoft.Json.JsonConvert.SerializeObject(ipv6);
        }

        public class IPV6
        {
            public int countryId;
            public string code;
            public string name;
            public string ipv6_to;
            public string ipv6_from;


            public IPV6(Tuple<string, string> tuple, int countryId, string code, string name)
            {
                ipv6_from = tuple.Item1;
                ipv6_to = tuple.Item2;

                this.countryId = countryId;
                this.code = code;
                this.name = name;
            }
        }
    }
}
