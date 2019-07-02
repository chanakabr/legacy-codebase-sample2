using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ElasticSearch;
using ElasticSearch.Searcher;
using ElasticSearch.Common;
using ApiObjects.SearchObjects;
using Newtonsoft.Json.Linq;
using ApiObjects;
using KLogMonitor;
using System.Reflection;
using System.Net;

namespace ElasticSearch.Utilities
{
    public class IpToCountry
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        ///// <summary>
        ///// Finds the country id of a given ip, using special elastic search index
        ///// </summary>
        ///// <param name="ip"></param>
        ///// <returns></returns>
        //public static int GetCountryIdByIp(string ip)
        //{
        //    int result = 0;

        //    object id = GetCountryPropertyByIp(ip, "country_id");

        //    result = Convert.ToInt32(id);

        //    return result;
        //}

        //public static string GetCountryNameByIp(string ip)
        //{
        //    string result = string.Empty;

        //    object name = GetCountryPropertyByIp(ip, "name");

        //    result = Convert.ToString(name);

        //    return result;
        //}

        //public static string GetCountryCodeByIp(string ip)
        //{
        //    string result = string.Empty;

        //    object name = GetCountryPropertyByIp(ip, "code");

        //    result = Convert.ToString(name);

        //    return result;
        //}

        //private static object GetCountryPropertyByIp(string ip, string fieldName)
        //{
        //    object result = null;

        //    // Build query for getting coutnry
        //    FilteredQuery query = new FilteredQuery(true);

        //    // basic initialization
        //    query.PageIndex = 0;
        //    query.PageSize = 1;
        //    query.ReturnFields.Clear();
        //    query.ReturnFields.Add(string.Format("\"{0}\"", fieldName));

        //    QueryFilter filter = new QueryFilter();

        //    string ipValue = "0";

        //    if (!string.IsNullOrEmpty(ip))
        //    {
        //        string[] splitted = ip.Split('.');
        //        ipValue =
        //            (Int64.Parse(splitted[3]) + Int64.Parse(splitted[2]) * 256 + Int64.Parse(splitted[1]) * 256 * 256 +
        //                Int64.Parse(splitted[0]) * 256 * 256 * 256).ToString();
        //    }
        //    FilterCompositeType composite = new FilterCompositeType(CutWith.AND);

        //    // Build range term: the country id will be the closest to these
        //    ESRange rangeTo = new ESRange(true)
        //    {
        //        Key = "ip_to",
        //    };

        //    rangeTo.Value.Add(new KeyValuePair<eRangeComp, string>(eRangeComp.GTE, ipValue));

        //    ESRange rangeFrom = new ESRange(true)
        //    {
        //        Key = "ip_from",
        //    };

        //    rangeFrom.Value.Add(new KeyValuePair<eRangeComp, string>(eRangeComp.LTE, ipValue));

        //    // ip value is between ip_to and ip_from
        //    composite.AddChild(rangeTo);
        //    composite.AddChild(rangeFrom);

        //    filter.FilterSettings = composite;
        //    query.Filter = filter;

        //    string searchQuery = query.ToString();

        //    // Perform search
        //    ElasticSearchApi api = new ElasticSearchApi();
        //    string searchResult = api.Search("utils", "iptocountry", ref searchQuery);

        //    // parse search reult to json object
        //    var jsonObj = JObject.Parse(searchResult);

        //    if (jsonObj != null)
        //    {
        //        JToken tempToken;

        //        // check total items
        //        int totalItems = ((tempToken = jsonObj.SelectToken("hits.total")) == null ? 0 : (int)tempToken);

        //        if (totalItems > 0)
        //        {
        //            // get country from first (and hopefully only) result
        //            result = jsonObj.SelectToken("hits.hits").First().SelectToken(string.Format("fields.{0}", fieldName));

        //            JArray tempArray = result as JArray;

        //            if (tempArray != null && tempArray.Count > 0)
        //            {
        //                result = tempArray[0];
        //            }
        //        }
        //    }

        //    return result;
        //}

        public static Country GetCountryByIp(string ip)
        {
            Country country = null;

            try
            {
                FilteredQuery query = BuildFilteredQueryForGetCountry();
                QueryFilter filter = new QueryFilter();

                string ipValue = ConvertIpv4ToInt64(ip);

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
                country = ParseSearchResultToCountry(searchResult);
            }

            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetCountryByIp for ip: {0}", ip), ex);
            }

            return country;
        }

        public static Country GetCountryByCountryName(string countryName)
        {
            Country country = null;
            try
            {
                if (string.IsNullOrEmpty(countryName))
                {
                    return country;
                }

                // Build query for getting country
                FilteredQuery query = new FilteredQuery(true);

                // basic initialization
                query.PageIndex = 0;
                query.PageSize = 1;
                query.ReturnFields.Clear();
                query.ReturnFields.AddRange(new List<string>() { { string.Format("\"{0}\"", "country_id") }, { string.Format("\"{0}\"", "name") }, { string.Format("\"{0}\"", "code") } });

                QueryFilter filter = new QueryFilter();
                FilterCompositeType composite = new FilterCompositeType(CutWith.AND);
                ESTerm term = new ESTerm(false)
                {
                    Key = "name",
                    Value = countryName.ToLower()
                };

                composite.AddChild(term);
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
                        JObject jObj = jsonObj.SelectToken("hits.hits").First().SelectToken("fields") as JObject;
                        if (jObj != null && jObj.HasValues)
                        {
                            string name = string.Empty;
                            string code = string.Empty;
                            string countryId = string.Empty;
                            int id;
                            foreach (JProperty jProp in jObj.Properties())
                            {
                                if (jProp != null && jProp.HasValues)
                                {
                                    string key = jProp.Name;
                                    JArray jArray = jProp.Value as JArray;
                                    if (jArray != null && jArray.Count > 0)
                                    {
                                        string value = jArray[0].ToString();
                                        if (!string.IsNullOrEmpty(key))
                                        {
                                            switch (key)
                                            {
                                                case "name":
                                                    name = value;
                                                    break;
                                                case "country_id":
                                                    countryId = value;
                                                    break;
                                                case "code":
                                                    code = value;
                                                    break;
                                                default:
                                                    break;
                                            }
                                        }
                                    }
                                }
                            }

                            if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(code) && int.TryParse(countryId, out id))
                            {
                                country = new Country() { Code = code, Id = id, Name = name };
                            }
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetCountryByCountryName for countryName: {0}", countryName), ex);
            }

            return country;
        }

        // TODO SHIR - FINISH GetCountryByIpv6FromES
        public static Tuple<Country, bool> GetCountryByIpv6FromES(Dictionary<string, object> funcParams)
        {
            Country country = null;

            try
            {
                if (funcParams != null && funcParams.Count == 1 && funcParams.ContainsKey("ipv6"))
                {
                    string ipv6 = funcParams["ipv6"].ToString();
                    if (!string.IsNullOrEmpty(ipv6))
                    {
                        var query = BuildFilteredQueryForGetCountry();
                        QueryFilter filter = new QueryFilter();

                        var ipv6Value = ConvertIpv6ToUInt64Array(ipv6);

                        // 2001:bf8:0000:0000:0000:0000:0000:0000 - 2001:bf8:ffff:ffff:ffff:ffff:ffff:ffff
                        // [300, 042] -> 300042
                        //  300 < x2 && 042 > x1
                        //  300041 -> x1=041, x2=300

                        // Build range term: the country id will be the closest to these
                        ESRange rangeTo = new ESRange(true) { Key = "ip_to" };
                        rangeTo.Value.Add(new KeyValuePair<eRangeComp, string>(eRangeComp.GTE, ipv6Value[0].ToString()));

                        ESRange rangeFrom = new ESRange(true) { Key = "ip_from" };
                        rangeFrom.Value.Add(new KeyValuePair<eRangeComp, string>(eRangeComp.LTE, ipv6Value[1].ToString()));

                        // ip value is between ip_to and ip_from
                        FilterCompositeType composite = new FilterCompositeType(CutWith.AND);
                        composite.AddChild(rangeTo);
                        composite.AddChild(rangeFrom);

                        filter.FilterSettings = composite;
                        query.Filter = filter;

                        string searchQuery = query.ToString();

                        // Perform search
                        ElasticSearchApi api = new ElasticSearchApi();
                        string searchResult = api.Search("utils", "iptocountry", ref searchQuery);

                        // parse search reult to json object
                        country = ParseSearchResultToCountry(searchResult);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("GetCountryByIpFromES failed, parameters : {0}", string.Join(";", funcParams.Keys)), ex);
            }

            return new Tuple<ApiObjects.Country, bool>(country, country != null);
        }

        private static FilteredQuery BuildFilteredQueryForGetCountry()
        {
            // basic initialization
            var query = new FilteredQuery(true)
            {
                PageIndex = 0,
                PageSize = 1,
            };

            query.ReturnFields.Clear();
            query.ReturnFields.AddRange(new List<string>()
            {
                { string.Format("\"{0}\"", "country_id") },
                { string.Format("\"{0}\"", "name") },
                { string.Format("\"{0}\"", "code") }
            });

            return query;
        }

        private static string ConvertIpv4ToInt64(string ipv4)
        {
            string ipValue = "0";

            if (!string.IsNullOrEmpty(ipv4))
            {
                string[] splitted = null;
                string toSplit = ipv4;

                IPAddress address;

                try
                {
                    // try to convert to IPv4 only if it was previously mapped to IPv6 and if this is indeed an IPv6
                    if (IPAddress.TryParse(ipv4, out address))
                    {
                        if (address.IsIPv4MappedToIPv6 && address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
                        {
                            toSplit = address.MapToIPv4().ToString();
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("ConvertIpv4ToInt64 - Failed mapping IPv6 to IPv4 for IP = {0}, ex = {1}", ipv4, ex);
                }

                splitted = toSplit.Split('.');

                // validate that we have a good split result
                if (splitted.Length == 4)
                {
                    ipValue =
                        (Int64.Parse(splitted[3]) + Int64.Parse(splitted[2]) * 256 + Int64.Parse(splitted[1]) * 256 * 256 +
                            Int64.Parse(splitted[0]) * 256 * 256 * 256).ToString();
                }
            }

            return ipValue;
        }

        private static ulong[] ConvertIpv6ToUInt64Array(string ipv6)
        {
            if (IPAddress.TryParse(ipv6, out IPAddress address))
            {
                var addrBytes = address.GetAddressBytes();
                if (BitConverter.IsLittleEndian)
                {
                    //little-endian machines store multi-byte integers with the
                    //least significant byte first. this is a problem, as integer
                    //values are sent over the network in big-endian mode. reversing
                    //the order of the bytes is a quick way to get the BitConverter
                    //methods to convert the byte arrays in big-endian mode.
                    var byteList = new List<byte>(addrBytes);
                    byteList.Reverse();
                    addrBytes = byteList.ToArray();
                }

                var addrWords = new ulong[2];
                if (addrBytes.Length > 8)
                {
                    addrWords[0] = BitConverter.ToUInt64(addrBytes, 8);
                    addrWords[1] = BitConverter.ToUInt64(addrBytes, 0);
                }
                else
                {
                    addrWords[0] = 0;
                    addrWords[1] = BitConverter.ToUInt32(addrBytes, 0);
                }

                return addrWords;
            }

            return null;
        }

        private static Country ParseSearchResultToCountry(string searchResult)
        {
            var jsonObj = JObject.Parse(searchResult);
            if (jsonObj == null) { return null; }

            var tempToken = jsonObj.SelectToken("hits.total");

            // check total items
            int totalItems = tempToken == null ? 0 : (int)tempToken;
            if (totalItems <= 0) { return null; }

            // get country from first (and hopefully only) result 
            if (jsonObj.SelectToken("hits.hits").First().SelectToken("fields") is JObject jObj && jObj.HasValues)
            {
                string countryId = string.Empty, code = string.Empty, name = string.Empty;
                foreach (JProperty jProp in jObj.Properties())
                {
                    if (jProp != null && jProp.HasValues)
                    {
                        string key = jProp.Name;
                        if (jProp.Value is JArray jArray && jArray.Count > 0)
                        {
                            string value = jArray[0].ToString();
                            if (!string.IsNullOrEmpty(key))
                            {
                                switch (key)
                                {
                                    case "name":
                                        name = value;
                                        break;
                                    case "country_id":
                                        countryId = value;
                                        break;
                                    case "code":
                                        code = value;
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                    }
                }

                if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(code) && int.TryParse(countryId, out int id))
                {
                    var country = new Country() { Id = id, Code = code, Name = name };
                    return country;
                }
            }

            return null;
        }
    }
}
