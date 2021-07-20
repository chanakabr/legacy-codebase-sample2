using ApiObjects;
using ApiObjects.SearchObjects;
using ConfigurationManager;
using Core.Catalog;
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
        
        public const string LOWERCASE_ANALYZER =
            "\"lowercase_analyzer\": {\"type\": \"custom\",\"tokenizer\": \"keyword\",\"filter\": [\"lowercase\"],\"char_filter\": [\"html_strip\"]}";

        public IPToCountryIndexBuilder()
        {
        }

        public bool BuildIndex()
        {
            bool result = false;

            try
            {
                var indexManager = IndexManagerFactory.GetInstance(0);
                string newIndexName = indexManager.SetupIPToCountryIndex();

                List<IPV6> ipv6List = new List<IPV6>();
                List<IPV4> ipv4List = new List<IPV4>();

                DataTable ipV6ToCountryMapping = DAL.ApiDAL.GetIpv6ToCountryTable();
                DataTable ipV4ToCountryMapping = DAL.ApiDAL.Get_IPToCountryTable();

                if (ipV6ToCountryMapping != null)
                {
                    foreach (DataRow row in ipV6ToCountryMapping.Rows)
                    {
                        string network = ODBCWrapper.Utils.ExtractValue<string>(row, "NETWORK");
                        int countryId = ODBCWrapper.Utils.ExtractInteger(row, "COUNTRY_ID");
                        string code = ODBCWrapper.Utils.ExtractString(row, "COUNTRY_CD2");
                        string name = ODBCWrapper.Utils.ExtractString(row, "COUNTRY_NAME");
                        string id = ODBCWrapper.Utils.ExtractString(row, "ID");

                        var tuple = IpToCountryHandler.GetIpRangesByNetworkStatic(network);
                        IPV6 ipv6 = new IPV6(tuple, countryId, code, name);

                        ipv6List.Add(ipv6);
                    }
                }

                if (ipV4ToCountryMapping != null)
                {
                    foreach (DataRow row in ipV4ToCountryMapping.Rows)
                    {
                        string id = ODBCWrapper.Utils.ExtractString(row, "ID");
                        long ipFrom = ODBCWrapper.Utils.ExtractValue<long>(row, "IP_FROM");
                        long ipTo = ODBCWrapper.Utils.ExtractValue<long>(row, "IP_TO");
                        int countryId = ODBCWrapper.Utils.ExtractInteger(row, "COUNTRY_ID");
                        string code = ODBCWrapper.Utils.ExtractString(row, "COUNTRY_CD2");
                        string name = ODBCWrapper.Utils.ExtractString(row, "COUNTRY_NAME");

                        IPV4 ipv4 = new IPV4(id, countryId, code, name, ipFrom, ipTo);
                        ipv4List.Add(ipv4);
                    }
                }

                indexManager.InsertDataToIPToCountryIndex(newIndexName, ipv4List, ipv6List);

                result = indexManager.PublishIPToCountryIndex(newIndexName);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Failed building ip to country index. reason = {0}", ex);

                result = false;
            }

            return result;
        }
    }
}
