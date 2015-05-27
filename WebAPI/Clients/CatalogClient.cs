using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using AutoMapper;
using Enyim.Caching;
using WebAPI.Catalog;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Models;
using WebAPI.Models.Catalog;
using WebAPI.ObjectsConvertor;
using WebAPI.Utils;
using WebAPI.Models.General;

namespace WebAPI.Clients
{
    public class CatalogClient : BaseClient
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

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

        protected WebAPI.Catalog.IserviceClient CatalogClientModule
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

        public AssetInfoWrapper SearchAssets(int groupId, string siteGuid, string udid, string language, int pageIndex, int? pageSize,
            string filter, Order? orderBy, List<int> assetTypes, List<With> with)
        {
            AssetInfoWrapper result = new AssetInfoWrapper();

            // Create catalog order object
            OrderObj order = new OrderObj();
            if (orderBy == null)
            {
                order.m_eOrderBy = OrderBy.RELATED;
                order.m_eOrderDir = OrderDir.DESC;
            }
            else
            {
                order = CatalogConvertor.ConvertOrderToOrderObj(orderBy.Value);
            }

            // build request
            UnifiedSearchRequest request = new UnifiedSearchRequest()
            {
                m_sSignature = Signature,
                m_sSignString = SignString,
                m_oFilter = new Filter()
                {
                    m_sDeviceId = udid,
                    m_nLanguage = CatalogUtils.GetLanguageId(groupId, language),
                },
                m_nGroupID = groupId,
                m_nPageIndex = pageIndex,
                m_nPageSize = pageSize.Value,
                filterQuery = filter,
                order = order,
                assetTypes = assetTypes,
            };

            // build failover cache key
            StringBuilder key = new StringBuilder();
            key.AppendFormat("Unified_search_g={0}_ps={1}_pi={2}_ob={3}_od={4}_ov={5}_f={6}", groupId, pageSize, pageIndex, order.m_eOrderBy, order.m_eOrderDir, order.m_sOrderValue, filter);
            if (assetTypes != null && assetTypes.Count > 0)
                key.AppendFormat("_at={0}", string.Join(",", assetTypes.Select(at => at.ToString()).ToArray()));

            // fire unified search request
            UnifiedSearchResponse searchResponse = new UnifiedSearchResponse();
            if (!CatalogUtils.GetBaseResponse<UnifiedSearchResponse>(CatalogClientModule, request, out searchResponse))
            {
                // general error
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (searchResponse.status.Code != (int)StatusCode.OK)
            {
                // Bad response received from WS
                throw new ClientException(searchResponse.status.Code, searchResponse.status.Message);
            }

            if (searchResponse.searchResults != null && searchResponse.searchResults.Count > 0)
            {
                // get base objects list
                List<BaseObject> assetsBaseDataList = searchResponse.searchResults.Select(x => x as BaseObject).ToList();

                // get assets from catalog/cache
                List<IAssetable> assetsInfo = CatalogUtils.GetAssets(CatalogClientModule, assetsBaseDataList, request, CacheDuration, with, CatalogConvertor.ConvertBaseObjectsToAssetsInfo);
                
                // build AssetInfoWrapper response
                if (assetsInfo != null)
                {
                    result.Assets = assetsInfo.Select(a => (AssetInfo)a).ToList();
                }

                result.TotalItems = searchResponse.m_nTotalItems;
            }
            return result;
        }

        public SlimAssetInfoWrapper Autocomplete(int groupId, string siteGuid, string udid, string language, int? size, string query, Order? orderBy, List<int> assetTypes, List<With> with)
        {
            SlimAssetInfoWrapper result = new SlimAssetInfoWrapper();

            // Create our own filter - only search in title
            string filter = string.Format("(and name^'{0}')", query.Replace("'", "%27"));

            // Create catalog order object
            OrderObj order = new OrderObj();
            if (orderBy == null)
            {
                order.m_eOrderBy = OrderBy.RELATED;
                order.m_eOrderDir = OrderDir.DESC;
            }
            else
            {
                order = CatalogConvertor.ConvertOrderToOrderObj(orderBy.Value);
            }

            // build request
            UnifiedSearchRequest request = new UnifiedSearchRequest()
            {
                m_sSignature = Signature,
                m_sSignString = SignString,
                m_oFilter = new Filter()
                {
                    m_nLanguage = CatalogUtils.GetLanguageId(groupId, language),
                    m_sDeviceId = udid
                },
                m_nGroupID = groupId,
                m_nPageIndex = 0,
                m_nPageSize = size.Value,
                filterQuery = filter,
                order = order,
                assetTypes = assetTypes,
            };

            // build failover cache key
            StringBuilder key = new StringBuilder();
            key.AppendFormat("Autocomplete_g={0}_ps={1}_pi={2}_ob={3}_od={4}_ov={5}_f={6}", groupId, size, 0, order.m_eOrderBy, order.m_eOrderDir, order.m_sOrderValue, filter);
            if (assetTypes != null && assetTypes.Count > 0)
                key.AppendFormat("_at={0}", string.Join(",", assetTypes.Select(at => at.ToString()).ToArray()));

            // fire unified search request
            UnifiedSearchResponse searchResponse = new UnifiedSearchResponse();
            if (!CatalogUtils.GetBaseResponse<UnifiedSearchResponse>(CatalogClientModule, request, out searchResponse))
            {
                // general error
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (searchResponse.status.Code != (int)StatusCode.OK)
            {
                // Bad response received from WS
                throw new ClientException(searchResponse.status.Code, searchResponse.status.Message);
            }

            if (searchResponse.searchResults != null && searchResponse.searchResults.Count > 0)
            {
                // get base objects list
                List<BaseObject> assetsBaseDataList = searchResponse.searchResults.Select(x => x as BaseObject).ToList();

                // get assets from catalog/cache
                List<IAssetable> assetsInfo = CatalogUtils.GetAssets(CatalogClientModule, assetsBaseDataList, request, CacheDuration, with, CatalogConvertor.ConvertBaseObjectsToSlimAssetsInfo);

                // build AssetInfoWrapper response
                if (assetsInfo != null)
                {
                    result.Assets = assetsInfo.Select(a => (SlimAssetInfo)a).ToList();
                }

                result.TotalItems = searchResponse.m_nTotalItems;
            }

            return result;
        }

        public WatchHistoryAssetWrapper WatchHistory(int groupId, string siteGuid, string language, int pageIndex, int? pageSize, WatchStatus? filterStatus, int days, List<int> assetTypes, List<With> withList)
        {
            WatchHistoryAssetWrapper finalResults = new WatchHistoryAssetWrapper();

            // build request
            WatchHistoryRequest request = new WatchHistoryRequest()
            {
                m_sSiteGuid = siteGuid,
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

            // fire history watched request
            WatchHistoryResponse watchHistoryResponse = new WatchHistoryResponse();
            if (!CatalogUtils.GetBaseResponse<WatchHistoryResponse>(CatalogClientModule, request, out watchHistoryResponse))
            {
                // general error
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (watchHistoryResponse.status.Code != (int)StatusCode.OK)
            {
                // Bad response received from WS
                throw new ClientException(watchHistoryResponse.status.Code, watchHistoryResponse.status.Message);
            }

            if (watchHistoryResponse.result != null && watchHistoryResponse.result.Count > 0)
            {
                // get base objects list
                List<BaseObject> assetsBaseDataList = watchHistoryResponse.result.Select(x => x as BaseObject).ToList();

                // get assets from catalog/cache
                List<IAssetable> assetsInfo = CatalogUtils.GetAssets(CatalogClientModule, assetsBaseDataList, request, CacheDuration, withList, CatalogConvertor.ConvertBaseObjectsToAssetsInfo);

                // combine asset info and watch history info
                finalResults.TotalItems = watchHistoryResponse.m_nTotalItems;

                UserWatchHistory watchHistory = new UserWatchHistory();
                foreach (var assetInfo in assetsInfo)
                {
                    watchHistory = watchHistoryResponse.result.FirstOrDefault(x => x.AssetId == ((AssetInfo)assetInfo).Id.ToString());

                    if (watchHistory != null)
                    {
                        finalResults.WatchHistoryAssets.Add(new WatchHistoryAsset()
                        {
                            Asset = (AssetInfo)assetInfo,
                            Duration = watchHistory.Duration,
                            IsFinishedWatching = watchHistory.IsFinishedWatching,
                            LastWatched = watchHistory.LastWatch,
                            Position = watchHistory.Location
                        });
                    }
                }
            }

            return finalResults;
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

            var response = CatalogClientModule.GetResponse(request) as AssetStatsResponse;

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