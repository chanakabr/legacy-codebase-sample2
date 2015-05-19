using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using WebAPI.Catalog;
using WebAPI.Clients.Exceptions;
using WebAPI.Clients.Utils;
using WebAPI.Models;
using WebAPI.Utils;

namespace WebAPI.Clients
{
    public class CatalogClient : BaseClient
    {
        private const string MEDIA_CACHE_KEY_PREFIX = "media";
        private const string EPG_CACHE_KEY_PREFIX = "epg";
        private const string CACHE_KEY_FORMAT = "{0}_lng{1}";

        public string Signature { get; set; }
        public string SignString { get; set; }
        public string SignatureKey
        {
            set
            {
                SignString = Guid.NewGuid().ToString();
                Signature = GetSignature(SignString, value);
            }
        }

        public int CacheDuration { get; set; }

        public CatalogClient()
        {

        }

        protected WebAPI.Catalog.IserviceClient Catalog
        {
            get
            {
                return (Module as WebAPI.Catalog.IserviceClient);
            }
        }


        private string GetSignature(string signString, string signatureKey)
        {
            string retVal;
            //Get key from DB
            string hmacSecret = signatureKey;
            // The HMAC secret as configured in the skin
            // Values are always transferred using UTF-8 encoding
            System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();

            // Calculate the HMAC
            // signingString is the SignString from the request
            HMACSHA1 myhmacsha1 = new HMACSHA1(encoding.GetBytes(hmacSecret));
            retVal = System.Convert.ToBase64String(myhmacsha1.ComputeHash(encoding.GetBytes(signString)));
            myhmacsha1.Clear();
            return retVal;
        }

        public AssetInfoWrapper SearchAssets(int groupID, string siteGuid, string udid, int language, int pageIndex, int? pageSize,
            string filter, Order? orderBy, List<int> assetTypes, List<With> with)
        {
            AssetInfoWrapper result = new AssetInfoWrapper();

            if (!string.IsNullOrEmpty(filter) && filter.Length > 500 * 1024)
            {
                throw new ClientException((int)StatusCode.BadRequest, "too long filter");
            }
            // page size - 5 <= size <= 50
            if (pageSize == null)
            {
                pageSize = 25;
            }
            else if (pageSize > 50)
            {
                pageSize = 50;
            }
            else if (pageSize < 5)
            {
                throw new ClientException((int)StatusCode.BadRequest, "page_size range can be between 5 and 50");
            }

            // Create catalog order object
            OrderObj order = new OrderObj();
            if (orderBy == null)
            {
                order.m_eOrderBy = OrderBy.RELATED;
                order.m_eOrderDir = OrderDir.DESC;
            }
            else
            {
                switch (orderBy)
                {
                    case Order.a_to_z:
                        order.m_eOrderBy = OrderBy.NAME;
                        order.m_eOrderDir = OrderDir.ASC;
                        break;
                    case Order.z_to_a:
                        order.m_eOrderBy = OrderBy.NAME;
                        order.m_eOrderDir = OrderDir.DESC;
                        break;
                    case Order.views:
                        order.m_eOrderBy = OrderBy.VIEWS;
                        order.m_eOrderDir = OrderDir.DESC;
                        break;
                    case Order.ratings:
                        order.m_eOrderBy = OrderBy.RATING;
                        order.m_eOrderDir = OrderDir.DESC;
                        break;
                    case Order.votes:
                        order.m_eOrderBy = OrderBy.VOTES_COUNT;
                        order.m_eOrderDir = OrderDir.DESC;
                        break;
                    case Order.newest:
                        order.m_eOrderBy = OrderBy.CREATE_DATE;
                        order.m_eOrderDir = OrderDir.DESC;
                        break;
                    case null:
                    case Order.relevancy:
                        order.m_eOrderBy = OrderBy.RELATED;
                        order.m_eOrderDir = OrderDir.DESC;
                        break;
                    default:
                        throw new ClientException((int)StatusCode.BadRequest, "Unknown order_by value");
                }
            }

            // build request
            UnifiedSearchRequest request = new UnifiedSearchRequest()
            {
                m_sSignature = Signature,
                m_sSignString = SignString,
                m_oFilter = new Filter()
                {
                    m_sDeviceId = udid,
                    m_nLanguage = language,
                },
                m_nGroupID = groupID,
                m_nPageIndex = pageIndex,
                m_nPageSize = pageSize.Value,
                filterQuery = filter,
                order = order,
                assetTypes = assetTypes,
            };

            // build failover cahce key
            StringBuilder key = new StringBuilder();
            key.AppendFormat("Unified_search_g={0}_ps={1}_pi={2}_ob={3}_od={4}_ov={5}_f={6}", groupID, pageSize, pageIndex, order.m_eOrderBy, order.m_eOrderDir, order.m_sOrderValue, filter);
            if (assetTypes != null && assetTypes.Count > 0)
                key.AppendFormat("_at={0}", string.Join(",", assetTypes.Select(at => at.ToString()).ToArray()));

            result = CatalogUtils.SearchAssets<AssetInfoWrapper>(Catalog, SignString, Signature, CacheDuration, request, key.ToString(), with);

            return result;
        }

        public SlimAssetInfoWrapper Autocomplete(int groupID, string siteGuid, string udid, int language, int? size, string query, Order? orderBy, List<int> assetTypes, List<With> with)
        {
            SlimAssetInfoWrapper result = new SlimAssetInfoWrapper();

            // Size rules - according to spec.  10>=size>=1 is valid. default is 5.
            if (size == null || size > 10 || size < 1)
            {
                size = 5;
            }

            // Create our own filter - only search in title
            string filter = string.Format("(and name^'{0}')", query.Replace("'", "%27"));

            // Create catalog order object
            OrderObj order = new OrderObj();

            if (orderBy == null)
            {
                order.m_eOrderBy = OrderBy.CREATE_DATE;
                order.m_eOrderDir = OrderDir.DESC;
            }
            else
            {
                switch (orderBy)
                {
                    case Order.a_to_z:
                        order.m_eOrderBy = OrderBy.NAME;
                        order.m_eOrderDir = OrderDir.ASC;
                        break;
                    case Order.z_to_a:
                        order.m_eOrderBy = OrderBy.NAME;
                        order.m_eOrderDir = OrderDir.DESC;
                        break;
                    case Order.views:
                        order.m_eOrderBy = OrderBy.VIEWS;
                        order.m_eOrderDir = OrderDir.DESC;
                        break;
                    case Order.ratings:
                        order.m_eOrderBy = OrderBy.RATING;
                        order.m_eOrderDir = OrderDir.DESC;
                        break;
                    case Order.votes:
                        order.m_eOrderBy = OrderBy.VOTES_COUNT;
                        order.m_eOrderDir = OrderDir.DESC;
                        break;
                    case Order.newest:
                        order.m_eOrderBy = OrderBy.CREATE_DATE;
                        order.m_eOrderDir = OrderDir.DESC;
                        break;
                    case null:
                    case Order.relevancy:
                        order.m_eOrderBy = OrderBy.RELATED;
                        order.m_eOrderDir = OrderDir.DESC;
                        break;
                    default:
                        throw new ClientException((int)StatusCode.BadRequest, "Unknown order_by value");
                }
            }

            // build request
            UnifiedSearchRequest request = new UnifiedSearchRequest()
            {
                m_sSignature = Signature,
                m_sSignString = SignString,
                m_oFilter = new Filter()
                {
                    m_nLanguage = language,
                    m_sDeviceId = udid
                },
                m_nGroupID = groupID,
                m_nPageIndex = 0,
                m_nPageSize = size.Value,
                filterQuery = filter,
                order = order,
                assetTypes = assetTypes,
            };

            // build failover cache key
            StringBuilder key = new StringBuilder();
            key.AppendFormat("Autocomplete_g={0}_ps={1}_pi={2}_ob={3}_od={4}_ov={5}_f={6}", groupID, size, 0, order.m_eOrderBy, order.m_eOrderDir, order.m_sOrderValue, filter);
            if (assetTypes != null && assetTypes.Count > 0)
                key.AppendFormat("_at={0}", string.Join(",", assetTypes.Select(at => at.ToString()).ToArray()));

            result = CatalogUtils.SearchAssets<SlimAssetInfoWrapper>(Catalog, SignString, Signature, CacheDuration, request, key.ToString(), with);

            return result;
        }


        public WatchHistoryAssetWrapper WatchHistory(int groupId, string siteGuid, string language, int pageIndex, int? pageSize, WatchStatus? filterStatus, int days, List<int> assetTypes, List<With> with)
        {
            WatchHistoryAssetWrapper result = new WatchHistoryAssetWrapper();

            // page size - 5 <= size <= 50
            if (pageSize == null || pageSize == 0)
            {
                pageSize = 25;
            }
            else if (pageSize > 50)
            {
                pageSize = 50;
            }
            else if (pageSize < 5)
            {
                throw new ClientException((int)StatusCode.BadRequest, "page_size range can be between 5 and 50");
            }

            // days - default value 7
            if (days == 0)
                days = 7;

            // build request
            WatchHistoryRequest request = new WatchHistoryRequest()
            {
                m_sSignature = Signature,
                m_sSignString = SignString,
                m_oFilter = new Filter()
                {
                    m_nLanguage = CatalogUtils.GetLanguageId(groupId, language)
                },
                m_nGroupID = groupId,
                m_nPageIndex = pageIndex,
                m_nPageSize = pageSize.Value,
                AssetTypes = assetTypes,
                FilterStatus = (eWatchStatus)Enum.Parse(typeof(eWatchStatus), filterStatus.ToString()),
                NumOfDays = days,
                OrderDir = OrderDir.DESC
            };

            result = CatalogUtils.WatchHistory(Catalog, SignString, Signature, CacheDuration, request, with);

            return result;
        }

        public List<AssetStats> GetAssetsStats(int groupID, string siteGuid, List<int> assetIds, long startTime, long endTime, StatsType assetType)
        {
            List<AssetStats> result = null;
            AssetStatsRequest request = new AssetStatsRequest()
            {
                m_sSignature = Signature,
                m_sSignString = SignString,
                m_sSiteGuid = siteGuid,
                m_nGroupID = groupID,
                m_nAssetIDs = assetIds,
                m_dStartDate = SerializationUtils.ConvertFromUnixTimestamp(startTime),
                m_dEndDate = SerializationUtils.ConvertFromUnixTimestamp(endTime),
                m_type = Mapper.Map<WebAPI.Catalog.StatsType>(assetType)
            };

            var response = Catalog.GetResponse(request) as AssetStatsResponse;

            if (response != null)
            {
                result = response.m_lAssetStat != null ?
                    Mapper.Map<List<AssetStats>>(response.m_lAssetStat) : null;
            }
            else
            {
                throw new ClientException((int)StatusCode.Error, "Failed to receive stats from catalog");
            }

            return result;
        }




        //public string MediaMark(int groupID, PlatformType platform, string siteGuid, string udid, int language, int mediaId, int mediaFileId, int location,
        //    string mediaCdn, string errorMessage, string errorCode, string mediaDuration, string action, int totalBitRate, int currentBitRate, int avgBitRate, string npvrId = null)
        //{
        //    string res = null;

        //    MediaMarkRequest request = new MediaMarkRequest()
        //    {
        //        m_sSignature = Signature,
        //        m_sSignString = SignString,
        //        m_oMediaPlayRequestData = new MediaPlayRequestData()
        //        {
        //            m_nAvgBitRate = avgBitRate,
        //            m_nCurrentBitRate = currentBitRate,
        //            m_nLoc = location,
        //            m_nMediaFileID = mediaFileId,
        //            m_nMediaID = mediaId,
        //            m_nTotalBitRate = totalBitRate,
        //            m_sAction = action,
        //            m_sMediaDuration = mediaDuration,
        //            m_sSiteGuid = siteGuid,
        //            m_sUDID = udid,
        //            m_sNpvrID = npvrId
        //        },
        //        m_sErrorCode = errorCode,
        //        m_sErrorMessage = errorMessage,
        //        m_sMediaCDN = mediaCdn,
        //        m_sSiteGuid = siteGuid,
        //        m_nGroupID = groupID,
        //        m_oFilter = new Filter()
        //        {
        //            m_sDeviceId = udid,
        //            m_nLanguage = language,
        //            m_sPlatform = platform.ToString(),
        //        }
        //    };

        //    var response = Catalog.GetResponse(request) as MediaMarkResponse;

        //    if (response != null)
        //    {
        //        res = response.m_sStatus;
        //    }
        //    else
        //    {
        //        throw new Exception("Failed to receive stats from catalog");
        //    }

        //    return res;
        //}
    }
}