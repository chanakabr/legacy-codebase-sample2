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
using WebAPI.Managers.Models;

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

        public KalturaAssetInfoWrapper SearchAssets(int groupId, string siteGuid, string udid, string language, int pageIndex, int? pageSize,
            string filter, KalturaOrder? orderBy, List<int> assetTypes, List<KalturaCatalogWith> with)
        {
            KalturaAssetInfoWrapper result = new KalturaAssetInfoWrapper();

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
                    m_nLanguage = Utils.Utils.GetLanguageId(groupId, language),
                },
                m_sUserIP = Utils.Utils.GetClientIP(),
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
            if (!CatalogUtils.GetBaseResponse<UnifiedSearchResponse>(CatalogClientModule, request, out searchResponse, true, key.ToString()))
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
                List<KalturaIAssetable> assetsInfo = CatalogUtils.GetAssets(CatalogClientModule, assetsBaseDataList, request, CacheDuration, with, CatalogConvertor.ConvertBaseObjectsToAssetsInfo);

                // build AssetInfoWrapper response
                if (assetsInfo != null)
                {
                    result.Assets = assetsInfo.Select(a => (KalturaAssetInfo)a).ToList();
                }

                result.TotalItems = searchResponse.m_nTotalItems;
            }
            return result;
        }

        public KalturaSlimAssetInfoWrapper Autocomplete(int groupId, string siteGuid, string udid, string language, int? size, string query, KalturaOrder? orderBy, List<int> assetTypes, List<KalturaCatalogWith> with)
        {
            KalturaSlimAssetInfoWrapper result = new KalturaSlimAssetInfoWrapper();

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
                    m_nLanguage = Utils.Utils.GetLanguageId(groupId, language),
                    m_sDeviceId = udid
                },
                m_sUserIP = Utils.Utils.GetClientIP(),
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
            if (!CatalogUtils.GetBaseResponse<UnifiedSearchResponse>(CatalogClientModule, request, out searchResponse, true, key.ToString()))
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
                List<KalturaIAssetable> assetsInfo = CatalogUtils.GetAssets(CatalogClientModule, assetsBaseDataList, request, CacheDuration, with, CatalogConvertor.ConvertBaseObjectsToSlimAssetsInfo);

                // build AssetInfoWrapper response
                if (assetsInfo != null)
                {
                    result.Assets = assetsInfo.Select(a => (KalturaSlimAssetInfo)a).ToList();
                }

                result.TotalItems = searchResponse.m_nTotalItems;
            }

            return result;
        }

        public KalturaWatchHistoryAssetWrapper WatchHistory(int groupId, string siteGuid, string language, int pageIndex, int? pageSize, KalturaWatchStatus? filterStatus, int days, List<int> assetTypes, List<KalturaCatalogWith> withList)
        {
            KalturaWatchHistoryAssetWrapper finalResults = new KalturaWatchHistoryAssetWrapper();

            // validate and convert filter status
            eWatchStatus filterStatusHelper = eWatchStatus.All;
            if (filterStatus != null)
                Enum.TryParse<eWatchStatus>(filterStatus.ToString(), out filterStatusHelper);

            // build request
            WatchHistoryRequest request = new WatchHistoryRequest()
            {
                m_sSiteGuid = siteGuid,
                m_sSignature = Signature,
                m_sSignString = SignString,
                m_oFilter = new Filter()
                {
                    m_nLanguage = Utils.Utils.GetLanguageId(groupId, language)
                },
                m_sUserIP = Utils.Utils.GetClientIP(),
                m_nGroupID = groupId,
                m_nPageIndex = pageIndex,
                m_nPageSize = pageSize.Value,
                AssetTypes = assetTypes,
                FilterStatus = filterStatusHelper,
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
                List<KalturaIAssetable> assetsInfo = CatalogUtils.GetAssets(CatalogClientModule, assetsBaseDataList, request, CacheDuration, withList, CatalogConvertor.ConvertBaseObjectsToAssetsInfo);

                // combine asset info and watch history info
                finalResults.TotalItems = watchHistoryResponse.m_nTotalItems;

                UserWatchHistory watchHistory = new UserWatchHistory();
                foreach (var assetInfo in assetsInfo)
                {
                    watchHistory = watchHistoryResponse.result.FirstOrDefault(x => x.AssetId == ((KalturaAssetInfo)assetInfo).Id.ToString());

                    if (watchHistory != null)
                    {
                        finalResults.WatchHistoryAssets.Add(new KalturaWatchHistoryAsset()
                        {
                            Asset = (KalturaAssetInfo)assetInfo,
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

        public List<KalturaAssetStats> GetAssetsStats(int groupID, string siteGuid, List<int> assetIds, StatsType assetType, long startTime = 0, long endTime = 0)
        {
            List<KalturaAssetStats> result = null;
            AssetStatsRequest request = new AssetStatsRequest()
            {
                m_sSignature = Signature,
                m_sSignString = SignString,
                m_sSiteGuid = siteGuid,
                m_nGroupID = groupID,
                m_sUserIP = Utils.Utils.GetClientIP(),
                m_nAssetIDs = assetIds,
                m_dStartDate = startTime != 0 ? SerializationUtils.ConvertFromUnixTimestamp(startTime) : DateTime.MinValue,
                m_dEndDate = endTime != 0 ? SerializationUtils.ConvertFromUnixTimestamp(endTime) : DateTime.MaxValue,
                m_type = Mapper.Map<WebAPI.Catalog.StatsType>(assetType)
            };

            AssetStatsResponse response = null;
            if (CatalogUtils.GetBaseResponse(CatalogClientModule, request, out response))
            {
                result = response.m_lAssetStat != null ?
                    Mapper.Map<List<KalturaAssetStats>>(response.m_lAssetStat) : null;
            }
            else
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            return result;
        }

        public KalturaAssetInfoWrapper GetRelatedMedia(int groupId, string siteGuid, int domainId, string udid, string language, int pageIndex, int? pageSize, int mediaId, List<int> mediaTypes, List<KalturaCatalogWith> with)
        {
            KalturaAssetInfoWrapper result = new KalturaAssetInfoWrapper();

            // build request
            MediaRelatedRequest request = new MediaRelatedRequest()
            {
                m_sSignature = Signature,
                m_sSignString = SignString,
                m_oFilter = new Filter()
                {
                    m_sDeviceId = udid,
                    m_nLanguage = Utils.Utils.GetLanguageId(groupId, language),
                },
                m_sUserIP = Utils.Utils.GetClientIP(),
                m_nGroupID = groupId,
                m_nPageIndex = pageIndex,
                m_nPageSize = pageSize.Value,
                m_nMediaID = mediaId,
                m_nMediaTypes = mediaTypes,
                m_sSiteGuid = siteGuid,
                domainId = domainId,
            };

            // build failover cache key
            StringBuilder key = new StringBuilder();
            key.AppendFormat("related_media_id={0}_pi={1}_pz={2}_g={3}_l={4}_mt={5}",
                mediaId, pageIndex, pageSize, groupId, language, mediaTypes != null ? string.Join(",", mediaTypes.ToArray()) : string.Empty);

            result = CatalogUtils.GetMedia(CatalogClientModule, request, key.ToString(), CacheDuration, with);

            return result;
        }

        public KalturaAssetInfoWrapper GetChannelMedia(int groupId, string siteGuid, int domainId, string udid, string language, int pageIndex, int? pageSize, int channelId, KalturaOrder? orderBy, List<KalturaCatalogWith> with)
        {
            KalturaAssetInfoWrapper result = new KalturaAssetInfoWrapper();

            // Create catalog order object
            OrderObj order = new OrderObj();
            if (orderBy == null)
            {
                order.m_eOrderBy = OrderBy.NONE;
            }
            else
            {
                order = CatalogConvertor.ConvertOrderToOrderObj(orderBy.Value);
            }

            // build request
            ChannelRequestMultiFiltering request = new ChannelRequestMultiFiltering()
            {
                m_sSignature = Signature,
                m_sSignString = SignString,
                m_oFilter = new Filter()
                {
                    m_sDeviceId = udid,
                    m_nLanguage = Utils.Utils.GetLanguageId(groupId, language),
                },
                m_sUserIP = Utils.Utils.GetClientIP(),
                m_nGroupID = groupId,
                m_nPageIndex = pageIndex,
                m_nPageSize = pageSize.Value,
                m_nChannelID = channelId,
                m_sSiteGuid = siteGuid,
                domainId = domainId,
                m_oOrderObj = order,
            };

            // build failover cache key
            StringBuilder key = new StringBuilder();
            key.AppendFormat("channel_id={0}_pi={1}_pz={2}_g={3}_l={4}_o_{5}",
                channelId, pageIndex, pageSize, groupId, siteGuid, language, orderBy);

            // fire request
            ChannelResponse channelResponse = new ChannelResponse();
            if (!CatalogUtils.GetBaseResponse<ChannelResponse>(CatalogClientModule, request, out channelResponse, true, key.ToString()))
            {
                // general error
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (channelResponse.m_nMedias != null && channelResponse.m_nMedias.Count > 0)
            {

                result.Assets = CatalogUtils.GetMediaByIds(CatalogClientModule, channelResponse.m_nMedias, request, CacheDuration, with);
                result.TotalItems = channelResponse.m_nTotalItems;
            }
            return result;
        }

        public KalturaAssetInfoWrapper GetMediaByIds(int groupId, string siteGuid, int domainId, string udid, string language, int pageIndex, int? pageSize, List<int> mediaIds, List<KalturaCatalogWith> with)
        {
            KalturaAssetInfoWrapper result = new KalturaAssetInfoWrapper();

            // build request
            MediaUpdateDateRequest request = new MediaUpdateDateRequest()
            {
                m_sSignature = Signature,
                m_sSignString = SignString,
                m_oFilter = new Filter()
                {
                    m_sDeviceId = udid,
                    m_nLanguage = Utils.Utils.GetLanguageId(groupId, language),
                },
                m_sUserIP = Utils.Utils.GetClientIP(),
                m_nGroupID = groupId,
                m_nPageIndex = pageIndex,
                m_nPageSize = pageSize.Value,
                m_sSiteGuid = siteGuid,
                domainId = domainId,
                m_lMediaIds = mediaIds,
            };

            MediaIdsResponse mediaIdsResponse = new MediaIdsResponse();
            if (!CatalogUtils.GetBaseResponse<MediaIdsResponse>(CatalogClientModule, request, out mediaIdsResponse))
            {
                // general error
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (mediaIdsResponse.m_nMediaIds != null && mediaIdsResponse.m_nMediaIds.Count > 0)
            {

                result.Assets = CatalogUtils.GetMediaByIds(CatalogClientModule, mediaIdsResponse.m_nMediaIds, request, CacheDuration, with);
                result.TotalItems = mediaIdsResponse.m_nTotalItems;
            }

            return result;
        }


        public WebAPI.Models.Catalog.KalturaChannel GetChannelInfo(int groupId, string siteGuid, int domainId, string language, int channelId)
        {
            WebAPI.Models.Catalog.KalturaChannel result = null;
            ChannelObjRequest request = new ChannelObjRequest()
            {
                m_sSignature = Signature,
                m_sSignString = SignString,
                m_oFilter = new Filter()
                {
                    m_nLanguage = Utils.Utils.GetLanguageId(groupId, language),
                },
                m_sSiteGuid = siteGuid,
                m_nGroupID = groupId,
                m_sUserIP = Utils.Utils.GetClientIP(),
                domainId = domainId,
                ChannelId = channelId,
            };

            ChannelObjResponse response = null;
            if (CatalogUtils.GetBaseResponse(CatalogClientModule, request, out response))
            {
                result = response.ChannelObj != null ?
                    Mapper.Map<WebAPI.Models.Catalog.KalturaChannel>(response.ChannelObj) : null;
            }
            else
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            return result;
        }

        public KalturaCategory GetCategory(int groupId, string siteGuid, int domainId, string language, int categoryId)
        {
            KalturaCategory result = null;
            CategoryRequest request = new CategoryRequest()
            {
                m_sSignature = Signature,
                m_sSignString = SignString,
                m_oFilter = new Filter()
                {
                    m_nLanguage = Utils.Utils.GetLanguageId(groupId, language),
                },
                m_sSiteGuid = siteGuid,
                m_nGroupID = groupId,
                m_sUserIP = Utils.Utils.GetClientIP(),
                domainId = domainId,
                m_nCategoryID = categoryId,
            };

            CategoryResponse response = null;
            if (CatalogUtils.GetBaseResponse(CatalogClientModule, request, out response) && response != null)
            {
                result = Mapper.Map<KalturaCategory>(response);
            }
            else
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            return result;
        }
    }
}