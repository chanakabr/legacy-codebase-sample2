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
using System.Collections;
using System.Net.Sockets;

namespace ElasticSearch.Utilities
{
    public class IpToCountry
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private static readonly Dictionary<AddressFamily, IpToCountryHandler> handlers = new Dictionary<AddressFamily, IpToCountryHandler>()
        {
            { AddressFamily.InterNetworkV6, new IpV6ToCountryHandler() },
            { AddressFamily.InterNetwork, new IpV4ToCountryHandler() },
        };

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
            if (string.IsNullOrEmpty(ip)) { return null; }
            
            try
            {
                if (IPAddress.TryParse(ip, out IPAddress address))
                {
                    IpToCountryHandler handler = null;
                    if (address.AddressFamily == AddressFamily.InterNetworkV6 && !address.IsIPv4MappedToIPv6)
                    {
                        handler = handlers[AddressFamily.InterNetworkV6];
                    }
                    else
                    {
                        handler = handlers[AddressFamily.InterNetwork];
                    }

                    var ipValue = handler.ConvertIpToValidString(address);
                    log.DebugFormat("GetCountryByIp: ip={0} was converted to ipValue={1}.", ip, ipValue);
                    var query = handler.BuildFilteredQueryForIp(ipValue);
                    var searchQuery = query.ToString();

                    // Perform search
                    ElasticSearchApi api = new ElasticSearchApi();
                    string searchResult = api.Search("utils", handler.IndexType, ref searchQuery);

                    // parse search reult to json object
                    var country = ParseSearchResultToCountry(searchResult);
                    return country;
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetCountryByIp for ip: {0}", ip), ex);
            }

            return null;
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
                QueryFilter filter = new QueryFilter();
                FilterCompositeType composite = new FilterCompositeType(CutWith.AND);
                ESTerm term = new ESTerm(false)
                {
                    Key = "name",
                    Value = countryName.ToLower()
                };
                composite.AddChild(term);
                
                var query = new FilteredQuery(true)
                {
                    PageIndex = 0,
                    PageSize = 1,
                    Filter = filter
                };

                query.ReturnFields.Clear();
                query.ReturnFields.AddRange(new List<string>()
                {
                    { string.Format("\"{0}\"", "country_id") },
                    { string.Format("\"{0}\"", "name") },
                    { string.Format("\"{0}\"", "code") },
                    { string.Format("\"{0}\"", "_id") }
                });

                string searchQuery = query.ToString();

                // Perform search
                ElasticSearchApi api = new ElasticSearchApi();
                string searchResult = api.Search("utils", "iptocountry", ref searchQuery);

                // parse search reult to json object
                country = ParseSearchResultToCountry(searchResult);
            }

            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetCountryByCountryName for countryName: {0}", countryName), ex);
            }

            return country;
        }
        
        public static Tuple<string, string> GetIpRangesByNetwork(string network)
        {
            return handlers[AddressFamily.InterNetworkV6].GetIpRangesByNetwork(network);
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
                string country_id = string.Empty, code = string.Empty, name = string.Empty, id = string.Empty;
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
                                        country_id = value;
                                        break;
                                    case "countryId":
                                        country_id = value;
                                        break;
                                    case "code":
                                        code = value;
                                        break;
                                    case "_id":
                                        id = value;
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                    }
                }

                if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(code) && int.TryParse(country_id, out int countryId))
                {
                    log.DebugFormat("ParseSearchResultToCountry - the result (network) ID is:{0}.", id);
                    var country = new Country() { Id = countryId, Code = code, Name = name };
                    return country;
                }
            }

            return null;
        }
    }

    internal abstract class IpToCountryHandler
    {
        protected static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        internal abstract string IndexType { get; }
        internal abstract string ConvertIpToValidString(IPAddress ipAddress);
        internal abstract FilteredQuery BuildFilteredQueryForIp(string ipValue);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="network"></param>
        /// <returns>item1=fromAddress; item2=toAddress</returns>
        internal abstract Tuple<string, string> GetIpRangesByNetwork(string network);
    }

    internal class IpV4ToCountryHandler : IpToCountryHandler
    {
        internal override string IndexType { get { return "iptocountry"; } }

        internal override FilteredQuery BuildFilteredQueryForIp(string ipValue)
        {
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
            
            var query = new FilteredQuery(true)
            {
                PageIndex = 0,
                PageSize = 1,
                Filter = new QueryFilter { FilterSettings = composite }
            };

            query.ReturnFields.Clear();
            query.ReturnFields.AddRange(new List<string>()
            {
                { string.Format("\"{0}\"", "country_id") },
                { string.Format("\"{0}\"", "name") },
                { string.Format("\"{0}\"", "code") },
                { string.Format("\"{0}\"", "_id") }
            });

            return query;
        }

        internal override string ConvertIpToValidString(IPAddress ipAddress)
        {
            string ipValue = "0";

            string[] splitted = null;
            string toSplit = ipAddress.ToString();

            try
            {
                // try to convert to IPv4 only if it was previously mapped to IPv6 and if this is indeed an IPv6
                if (ipAddress.IsIPv4MappedToIPv6 && ipAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
                {
                    toSplit = ipAddress.MapToIPv4().ToString();
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("ConvertIpv4ToInt64 - Failed mapping IPv6 to IPv4 for IP = {0}, ex = {1}", ipAddress, ex);
            }

            splitted = toSplit.Split('.');

            // validate that we have a good split result
            if (splitted.Length == 4)
            {
                ipValue = (Int64.Parse(splitted[3]) +
                           Int64.Parse(splitted[2]) * 256 +
                           Int64.Parse(splitted[1]) * 256 * 256 +
                           Int64.Parse(splitted[0]) * 256 * 256 * 256).ToString();
            }

            return ipValue;
        }

        internal override Tuple<string, string> GetIpRangesByNetwork(string network)
        {
            throw new NotImplementedException();
        }
    }

    internal class IpV6ToCountryHandler : IpToCountryHandler
    {
        internal override string IndexType { get { return "ipv6tocountry"; } }

        internal override FilteredQuery BuildFilteredQueryForIp(string ipValue)
        {
            ESRange ipRangeFromLTE = new ESRange(true) { Key = "ipv6_from" };
            ipRangeFromLTE.Value.Add(new KeyValuePair<eRangeComp, string>(eRangeComp.LTE, string.Format("\"{0}\"", ipValue)));

            ESRange ipRangeToGTE = new ESRange(true) { Key = "ipv6_to" };
            ipRangeToGTE.Value.Add(new KeyValuePair<eRangeComp, string>(eRangeComp.GTE, string.Format("\"{0}\"", ipValue)));

            // (ipFrom <= ipv6Value) && (ipv6Value <= ipTo)
            FilterCompositeType composite = new FilterCompositeType(CutWith.AND);
            composite.AddChild(ipRangeFromLTE);
            composite.AddChild(ipRangeToGTE);
            
            var query = new FilteredQuery(true)
            {
                PageIndex = 0,
                PageSize = 1,
                Filter = new QueryFilter { FilterSettings = composite }
            };

            query.ReturnFields.Clear();
            query.ReturnFields.AddRange(new List<string>()
            {
                { string.Format("\"{0}\"", "countryId") },
                { string.Format("\"{0}\"", "name") },
                { string.Format("\"{0}\"", "code") },
                { string.Format("\"{0}\"", "_id") }
            });

            return query;
        }

        internal override string ConvertIpToValidString(IPAddress ipAddress)
        {
            var addressBytes = ipAddress.GetAddressBytes();
            if (BitConverter.IsLittleEndian)
            {
                //little-endian machines store multi-byte integers with the
                //least significant byte first. this is a problem, as integer
                //values are sent over the network in big-endian mode. reversing
                //the order of the bytes is a quick way to get the BitConverter
                //methods to convert the byte arrays in big-endian mode.
                var byteList = new List<byte>(addressBytes);
                byteList.Reverse();
                addressBytes = byteList.ToArray();
            }

            var addrWords = ConvertAddressBytesToString(addressBytes);
            return addrWords;
        }

        internal override Tuple<string, string> GetIpRangesByNetwork(string network)
        {
            try
            {
                //Split the string in parts for address and prefix
                var endOfAddressIndex = network.IndexOf('/');
                var address = network.Substring(0, endOfAddressIndex);
                var networkBits = Int32.Parse(network.Substring(endOfAddressIndex + 1));

                if (IPAddress.TryParse(address, out IPAddress ipAddress))
                {
                    var fromAddressBytes = ipAddress.GetAddressBytes();
                    var toAddressBytes = new byte[fromAddressBytes.Length];

                    if (BitConverter.IsLittleEndian)
                    {
                        var addressBytesList = new List<byte>(fromAddressBytes);
                        addressBytesList.Reverse();
                        fromAddressBytes = addressBytesList.ToArray();
                        var addressBitArray = new BitArray(fromAddressBytes);

                        // run over bit array and set value to 1 from networkBits to the end.
                        for (int i = 0; i < (128 - networkBits) && i < addressBitArray.Length; i++)
                        {
                            addressBitArray[i] = true;
                        }

                        addressBitArray.CopyTo(toAddressBytes, 0);
                    }
                    else
                    {
                        var addressBitArray = new BitArray(fromAddressBytes);

                        // run over bit array and set value to 1 from networkBits to the end.
                        for (int i = networkBits; i < 128 && i < addressBitArray.Length; i++)
                        {
                            addressBitArray[i] = true;
                        }

                        addressBitArray.CopyTo(toAddressBytes, 0);
                    }

                    var fromAddress = ConvertAddressBytesToString(fromAddressBytes);
                    var toAddress = ConvertAddressBytesToString(toAddressBytes);

                    return new Tuple<string, string>(fromAddress, toAddress);
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("An Exception was occurred in GetIpRangesByNetwork. network:{0}.", network), ex);
            }

            return null;
        }

        private string ConvertAddressBytesToString(byte[] addressWords)
        {
            var address = new StringBuilder(48);

            if (addressWords.Length < 16)
            {
                for (int i = 0; i < 16 - addressWords.Length; i++)
                {
                    address.Append("000");
                }
            }

            for (int i = addressWords.Length - 1; i >= 0; i--)
            {
                var word = addressWords[i].ToString();
                if (word.Length < 3)
                {
                    for (int j = 0; j < 3 - word.Length; j++)
                    {
                        address.Append("0");
                    }
                }
                address.Append(word);
            }

            return address.ToString();
        }
    }
}
