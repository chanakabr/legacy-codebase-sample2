using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ElasticSearch;
using ElasticSearch.Searcher;
using ElasticSearch.Common;
using ApiObjects.SearchObjects;
using Newtonsoft.Json.Linq;

namespace ElasticSearch.Utilities
{
    public class IpToCountry
    {
        /// <summary>
        /// Finds the country id of a given ip, using special elastic search index
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public static int GetCountryByIp(string ip)
        {
            int result = 0;

            object id = GetCountryByIp(ip, "country_id");

            result = Convert.ToInt32(id);

            return result;
        }

        public static string GetCountryNameByIp(string ip)
        {
            string result = string.Empty;

            object name = GetCountryByIp(ip, "name");

            result = Convert.ToString(name);

            return result;
        }

        public static string GetCountryCodeByIp(string ip)
        {
            string result = string.Empty;

            object name = GetCountryByIp(ip, "code");

            result = Convert.ToString(name);

            return result;
        }

        public static object GetCountryByIp(string ip, string fieldName)
        {
            object result = null;

            // Build query for getting coutnry
            FilteredQuery query = new FilteredQuery(true);

            // basic initialization
            query.PageIndex = 0;
            query.PageSize = 1;
            query.ReturnFields.Clear();
            query.ReturnFields.Add(string.Format("\"{0}\"", fieldName));

            QueryFilter filter = new QueryFilter();

            string ipValue = "0";

            if (!string.IsNullOrEmpty(ip))
            {
                string[] splitted = ip.Split('.');
                ipValue =
                    (Int64.Parse(splitted[3]) + Int64.Parse(splitted[2]) * 256 + Int64.Parse(splitted[1]) * 256 * 256 +
                        Int64.Parse(splitted[0]) * 256 * 256 * 256).ToString();
            }
            FilterCompositeType composite = new FilterCompositeType(CutWith.AND);

            // Build range term: the country id will be the closest to these
            ESRange rangeTo = new ESRange(true)
            {
                Key = "ip_to",
            };

            rangeTo.Value.Add(new KeyValuePair<eRangeComp, string>(eRangeComp.GTE, ipValue));

            ESRange rangeFrom = new ESRange(true)
            {
                Key = "ip_from",
            };

            rangeFrom.Value.Add(new KeyValuePair<eRangeComp, string>(eRangeComp.LTE, ipValue));

            // ip value is between ip_to and ip_from
            composite.AddChild(rangeTo);
            composite.AddChild(rangeFrom);

            filter.FilterSettings = composite;
            query.Filter = filter;

            string searchQuery = query.ToString();

            // Perform search
            ElasticSearchApi api = new ElasticSearchApi();
            string searchResult = api.Search("utils", "iptocountry", ref searchQuery);

            // parse search reult to json object
            var jsonObj = JObject.Parse(searchResult);

            if (jsonObj != null)
            {
                JToken tempToken;

                // check total items
                int totalItems = ((tempToken = jsonObj.SelectToken("hits.total")) == null ? 0 : (int)tempToken);

                if (totalItems > 0)
                {
                    // get country from first (and hopefully only) result
                    result = jsonObj.SelectToken("hits.hits").First().SelectToken(string.Format("fields.{0}", fieldName));

                    JArray tempArray = result as JArray;

                    if (tempArray != null && tempArray.Count > 0)
                    {
                        result = tempArray[0];
                    }
                }
            }

            return result;
        }
    }
}
