using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using Nest;

namespace Core.Catalog
{
    public abstract class IpToCountryHandler
    {
        protected static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private static readonly Dictionary<AddressFamily, IpToCountryHandler> handlers = new Dictionary<AddressFamily, IpToCountryHandler>()
        {
            { AddressFamily.InterNetworkV6, new IpV6ToCountryHandler() },
            { AddressFamily.InterNetwork, new IpV4ToCountryHandler() },
        };

        internal static Dictionary<AddressFamily, IpToCountryHandler> Handlers
        {
            get
            {
                return handlers;
            }
        }

        public static Tuple<string, string> GetIpRangesByNetworkStatic(string network)
        {
            return handlers[AddressFamily.InterNetworkV6].GetIpRangesByNetwork(network);
        }

        internal abstract string IndexType { get; }
        internal abstract string ConvertIpToValidString(IPAddress ipAddress);
        internal abstract FilteredQuery BuildFilteredQueryForIp(string ipValue);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="network"></param>
        /// <returns>item1=fromAddress; item2=toAddress</returns>
        internal abstract Tuple<string, string> GetIpRangesByNetwork(string network);

        protected abstract string FromField { get; }
        protected abstract string ToField { get; }

        internal virtual QueryContainer BuildNestQueryForIp(QueryContainerDescriptor<ApiLogic.IndexManager.NestData.Country> q, string ipValue)
        {
            return q.TermRange(range => range.Field(this.ToField).GreaterThanOrEquals(ipValue)) &&
                    q.TermRange(range => range.Field(this.FromField).LessThanOrEquals(ipValue))
                ;
        }
    }

    internal class IpV4ToCountryHandler : IpToCountryHandler
    {
        protected override string FromField { get { return "ip_from"; } }

        protected override string ToField { get { return "ip_to"; } }


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
        protected override string FromField { get { return "ipv6_from"; } }

        protected override string ToField { get { return "ipv6_to"; } }


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
