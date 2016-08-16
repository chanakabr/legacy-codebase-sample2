using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using AutoMapper;
using WebAPI.Catalog;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Models;
using WebAPI.Models.Catalog;
using WebAPI.ObjectsConvertor;
using WebAPI.Utils;
using WebAPI.Models.General;
using WebAPI.Managers.Models;
using WebAPI.Models.Users;
using KLogMonitor;
using WebAPI.ClientManagers;
using WebAPI.ObjectsConvertor.Mapping;
using System.Web;
using WebAPI.Filters;

namespace WebAPI.Clients
{
    public class CatalogClient : BaseClient
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

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

        private DateTime getServerTime()
        {
            return (DateTime)HttpContext.Current.Items[RequestParser.REQUEST_TIME];
        }

        [Obsolete]
        public KalturaAssetInfoListResponse SearchAssets(int groupId, string siteGuid, int domainId, string udid, string language, int pageIndex, int? pageSize,
            string filter, KalturaOrder? orderBy, List<int> assetTypes, string requestId, List<KalturaCatalogWith> with)
        {
            KalturaAssetInfoListResponse result = new KalturaAssetInfoListResponse();

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

            // get group configuration 
            Group group = GroupsManager.GetGroup(groupId);

            // build request
            UnifiedSearchRequest request = new UnifiedSearchRequest()
            {
                m_sSignature = Signature,
                m_sSignString = SignString,
                m_oFilter = new Filter()
                {
                    m_sDeviceId = udid,
                    m_nLanguage = Utils.Utils.GetLanguageId(groupId, language),
                    m_bUseStartDate = group.UseStartDate,
                    m_bOnlyActiveMedia = group.GetOnlyActiveAssets
                },
                m_sUserIP = Utils.Utils.GetClientIP(),
                m_nGroupID = groupId,
                m_nPageIndex = pageIndex,
                m_nPageSize = pageSize.Value,
                filterQuery = filter,
                m_dServerTime = getServerTime(),
                order = order,
                assetTypes = assetTypes,
                m_sSiteGuid = siteGuid,
                domainId = domainId,
                requestId = requestId
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
                    result.Objects = assetsInfo.Select(a => (KalturaAssetInfo)a).ToList();
                }

                result.TotalCount = searchResponse.m_nTotalItems;
            }

            result.RequestId = searchResponse.requestId;

            return result;
        }

        public KalturaAssetListResponse SearchAssets(int groupId, string siteGuid, int domainId, string udid, string language, int pageIndex, int? pageSize,
            string filter, KalturaAssetOrderBy orderBy, List<int> assetTypes, List<int> epgChannelIds)
        {
            KalturaAssetListResponse result = new KalturaAssetListResponse();

            OrderObj order = CatalogConvertor.ConvertOrderToOrderObj(orderBy);

            // get group configuration 
            Group group = GroupsManager.GetGroup(groupId);

            // build failover cache key
            StringBuilder key = new StringBuilder();
            key.AppendFormat("Unified_search_g={0}_ps={1}_pi={2}_ob={3}_od={4}_ov={5}_f={6}", groupId, pageSize, pageIndex, order.m_eOrderBy, order.m_eOrderDir, order.m_sOrderValue, filter);

            if (assetTypes != null && assetTypes.Count > 0)
            {
                key.AppendFormat("_at={0}", string.Join(",", assetTypes.Select(at => at.ToString()).ToArray()));
            }

            if (epgChannelIds != null && epgChannelIds.Count > 0)
            {
                string strEpgChannelIds = string.Join(",", epgChannelIds.Select(at => at.ToString()).ToArray());
                key.AppendFormat("_ec={0}", strEpgChannelIds);
                filter += string.Format(" epg_channel_id:'{0}'", strEpgChannelIds);
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
                    m_bUseStartDate = group.UseStartDate,
                    m_bOnlyActiveMedia = group.GetOnlyActiveAssets
                },
                m_sUserIP = Utils.Utils.GetClientIP(),
                m_nGroupID = groupId,
                m_nPageIndex = pageIndex,
                m_nPageSize = pageSize.Value,
                filterQuery = filter,
                m_dServerTime = getServerTime(),
                order = order,
                assetTypes = assetTypes,
                m_sSiteGuid = siteGuid,
                domainId = domainId
            };

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
                result.Objects = CatalogUtils.GetAssets(CatalogClientModule, assetsBaseDataList, request, CacheDuration);
                result.TotalCount = searchResponse.m_nTotalItems;
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

            // get group configuration 
            Group group = GroupsManager.GetGroup(groupId);

            // build request
            UnifiedSearchRequest request = new UnifiedSearchRequest()
            {
                m_sSignature = Signature,
                m_sSignString = SignString,
                m_oFilter = new Filter()
                {
                    m_nLanguage = Utils.Utils.GetLanguageId(groupId, language),
                    m_sDeviceId = udid,
                    m_bUseStartDate = group.UseStartDate,
                    m_bOnlyActiveMedia = group.GetOnlyActiveAssets
                },
                m_sUserIP = Utils.Utils.GetClientIP(),
                m_nGroupID = groupId,
                m_nPageIndex = 0,
                m_nPageSize = size.Value,
                filterQuery = filter,
                m_dServerTime = getServerTime(),
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
                    result.Objects = assetsInfo.Select(a => (KalturaBaseAssetInfo)a).ToList();
                }

                result.TotalCount = searchResponse.m_nTotalItems;
            }

            return result;
        }

        public KalturaAssetHistoryListResponse getAssetHistory(int groupId, string siteGuid, string udid, string language, int pageIndex, int? pageSize, KalturaWatchStatus watchStatus, int days, List<int> assetTypes, List<KalturaCatalogWith> withList)
        {
            KalturaAssetHistoryListResponse finalResults = new KalturaAssetHistoryListResponse();
            finalResults.Objects = new List<KalturaAssetHistory>();

            // get group configuration 
            Group group = GroupsManager.GetGroup(groupId);

            // build request
            WatchHistoryRequest request = new WatchHistoryRequest()
            {
                m_sSiteGuid = siteGuid,
                m_sSignature = Signature,
                m_sSignString = SignString,
                m_oFilter = new Filter()
                {
                    m_sDeviceId = udid,
                    m_nLanguage = Utils.Utils.GetLanguageId(groupId, language),
                    m_bUseStartDate = group.UseStartDate,
                    m_bOnlyActiveMedia = group.GetOnlyActiveAssets
                },
                m_sUserIP = Utils.Utils.GetClientIP(),
                m_nGroupID = groupId,
                m_nPageIndex = pageIndex,
                m_nPageSize = pageSize.Value,
                AssetTypes = assetTypes,
                FilterStatus = CatalogMappings.ConvertKalturaWatchStatus(watchStatus),
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
                finalResults.TotalCount = watchHistoryResponse.m_nTotalItems;

                UserWatchHistory watchHistory = new UserWatchHistory();
                foreach (KalturaIAssetable assetInfo in assetsInfo)
                {
                    watchHistory = watchHistoryResponse.result.FirstOrDefault(x => x.AssetId == ((KalturaAssetInfo)assetInfo).Id.ToString());

                    if (watchHistory != null)
                    {
                        finalResults.Objects.Add(new KalturaAssetHistory()
                        {
                            AssetId = ((KalturaAssetInfo)assetInfo).Id.Value,
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

        [Obsolete]
        public KalturaWatchHistoryAssetWrapper WatchHistory(int groupId, string siteGuid, string udid, string language, int pageIndex, int? pageSize, KalturaWatchStatus watchStatus, int days, List<int> assetTypes, List<string> assetIds, List<KalturaCatalogWith> withList)
        {
            KalturaWatchHistoryAssetWrapper finalResults = new KalturaWatchHistoryAssetWrapper();

            // get group configuration 
            Group group = GroupsManager.GetGroup(groupId);

            // build request
            WatchHistoryRequest request = new WatchHistoryRequest()
            {
                m_sSiteGuid = siteGuid,
                m_sSignature = Signature,
                m_sSignString = SignString,
                m_oFilter = new Filter()
                {
                    m_sDeviceId = udid,
                    m_nLanguage = Utils.Utils.GetLanguageId(groupId, language),
                    m_bUseStartDate = group.UseStartDate,
                    m_bOnlyActiveMedia = group.GetOnlyActiveAssets
                },
                m_sUserIP = Utils.Utils.GetClientIP(),
                m_nGroupID = groupId,
                m_nPageIndex = pageIndex,
                m_nPageSize = pageSize.Value,
                AssetTypes = assetTypes,
                AssetIds = assetIds,
                FilterStatus = CatalogMappings.ConvertKalturaWatchStatus(watchStatus),
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
                finalResults.TotalCount = watchHistoryResponse.m_nTotalItems;

                UserWatchHistory watchHistory = new UserWatchHistory();
                foreach (var assetInfo in assetsInfo)
                {
                    watchHistory = watchHistoryResponse.result.FirstOrDefault(x => x.AssetId == ((KalturaAssetInfo)assetInfo).Id.ToString());

                    if (watchHistory != null)
                    {
                        finalResults.Objects.Add(new KalturaWatchHistoryAsset()
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

        public List<KalturaAssetStatistics> GetAssetsStats(int groupID, string siteGuid, List<int> assetIds, KalturaAssetType assetType, long startTime = 0, long endTime = 0)
        {
            List<KalturaAssetStatistics> result = null;
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
                m_type = CatalogMappings.ConvertAssetType(assetType)
            };

            AssetStatsResponse response = null;
            if (CatalogUtils.GetBaseResponse(CatalogClientModule, request, out response))
            {
                result = response.m_lAssetStat != null ?
                    Mapper.Map<List<KalturaAssetStatistics>>(response.m_lAssetStat) : null;
            }
            else
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            return result;
        }

        [Obsolete]
        public KalturaAssetInfoListResponse GetRelatedMedia(int groupId, string siteGuid, int domainId, string udid, string language, int pageIndex, int? pageSize, int mediaId, string filter, List<int> mediaTypes, List<KalturaCatalogWith> with)
        {
            KalturaAssetInfoListResponse result = new KalturaAssetInfoListResponse();

            // get group configuration 
            Group group = GroupsManager.GetGroup(groupId);

            // build request
            MediaRelatedRequest request = new MediaRelatedRequest()
            {
                m_sSignature = Signature,
                m_sSignString = SignString,
                m_oFilter = new Filter()
                {
                    m_sDeviceId = udid,
                    m_nLanguage = Utils.Utils.GetLanguageId(groupId, language),
                    m_bUseStartDate = group.UseStartDate,
                    m_bOnlyActiveMedia = group.GetOnlyActiveAssets
                },
                m_sUserIP = Utils.Utils.GetClientIP(),
                m_nGroupID = groupId,
                m_nPageIndex = pageIndex,
                m_nPageSize = pageSize.Value,
                m_nMediaID = mediaId,
                m_nMediaTypes = mediaTypes,
                m_sSiteGuid = siteGuid,
                domainId = domainId,
                m_sFilter = filter
            };

            // build failover cache key
            StringBuilder key = new StringBuilder();
            key.AppendFormat("related_media_id={0}_pi={1}_pz={2}_g={3}_l={4}_mt={5}",
                mediaId, pageIndex, pageSize, groupId, language, mediaTypes != null ? string.Join(",", mediaTypes.ToArray()) : string.Empty);

            result = CatalogUtils.GetMedia(CatalogClientModule, request, key.ToString(), CacheDuration, with);

            return result;
        }

        public KalturaAssetListResponse GetRelatedMedia(int groupId, string siteGuid, int domainId, string udid, string language, int pageIndex, int? pageSize, int mediaId, string filter, List<int> mediaTypes, KalturaAssetOrderBy orderBy)
        {
            KalturaAssetListResponse result = new KalturaAssetListResponse();

            // get group configuration 
            Group group = GroupsManager.GetGroup(groupId);

            // convert order by
            OrderObj order = CatalogConvertor.ConvertOrderToOrderObj(orderBy);

            // build request
            MediaRelatedRequest request = new MediaRelatedRequest()
            {
                m_sSignature = Signature,
                m_sSignString = SignString,
                m_oFilter = new Filter()
                {
                    m_sDeviceId = udid,
                    m_nLanguage = Utils.Utils.GetLanguageId(groupId, language),
                    m_bUseStartDate = group.UseStartDate,
                    m_bOnlyActiveMedia = group.GetOnlyActiveAssets
                },
                m_sUserIP = Utils.Utils.GetClientIP(),
                m_nGroupID = groupId,
                m_nPageIndex = pageIndex,
                m_nPageSize = pageSize.Value,
                m_nMediaID = mediaId,
                m_nMediaTypes = mediaTypes,
                m_sSiteGuid = siteGuid,
                domainId = domainId,
                m_sFilter = filter, 
                OrderObj = order
            };

            // build failover cache key
            StringBuilder key = new StringBuilder();
            key.AppendFormat("related_media_id={0}_pi={1}_pz={2}_g={3}_l={4}_mt={5}",
                mediaId, pageIndex, pageSize, groupId, language, mediaTypes != null ? string.Join(",", mediaTypes.ToArray()) : string.Empty);

            result = CatalogUtils.GetMedia(CatalogClientModule, request, key.ToString(), CacheDuration);

            return result;
        }

        public KalturaAssetInfoListResponse GetRelatedMediaExternal(int groupId, string siteGuid, int domainId, string udid, string language, int pageIndex, int? pageSize, 
                                                                    int mediaId, List<int> mediaTypes, int utcOffset, List<KalturaCatalogWith> with, string freeParam)
        {
            KalturaAssetInfoListResponse result = new KalturaAssetInfoListResponse();

            // get group configuration 
            Group group = GroupsManager.GetGroup(groupId);

            // build request
            MediaRelatedExternalRequest request = new MediaRelatedExternalRequest()
            {
                m_sSignature = Signature,
                m_sSignString = SignString,
                m_oFilter = new Filter()
                {
                    m_sDeviceId = udid,
                    m_nLanguage = Utils.Utils.GetLanguageId(groupId, language),
                    m_bUseStartDate = group.UseStartDate,
                    m_bOnlyActiveMedia = group.GetOnlyActiveAssets
                },
                m_sLanguage = language,
                m_sUserIP = Utils.Utils.GetClientIP(),
                m_nGroupID = groupId,
                m_nPageIndex = pageIndex,
                m_nPageSize = pageSize.Value,
                m_nMediaID = mediaId,
                m_nMediaTypes = mediaTypes,
                m_sSiteGuid = siteGuid,
                domainId = domainId,
                m_nUtcOffset = utcOffset,
                m_sFreeParam = freeParam
            };

            // build failover cache key
            StringBuilder key = new StringBuilder();
            key.AppendFormat("related_media_id={0}_pi={1}_pz={2}_g={3}_l={4}_mt={5}",
                mediaId, pageIndex, pageSize, groupId, language, mediaTypes != null ? string.Join(",", mediaTypes.ToArray()) : string.Empty);

            result = CatalogUtils.GetMediaWithStatus(CatalogClientModule, request, key.ToString(), CacheDuration, with);

            return result;
        }

        public KalturaAssetInfoListResponse GetSearchMediaExternal(int groupId, string siteGuid, int domainId, string udid, string language, int pageIndex, int? pageSize, string query, List<int> mediaTypes, int utcOffset, List<KalturaCatalogWith> with)
        {
            KalturaAssetInfoListResponse result = new KalturaAssetInfoListResponse();

            // get group configuration 
            Group group = GroupsManager.GetGroup(groupId);

            // build request
            MediaSearchExternalRequest request = new MediaSearchExternalRequest()
            {
                m_sSignature = Signature,
                m_sSignString = SignString,
                m_oFilter = new Filter()
                {
                    m_sDeviceId = udid,
                    m_nLanguage = Utils.Utils.GetLanguageId(groupId, language),
                    m_bUseStartDate = group.UseStartDate,
                    m_bOnlyActiveMedia = group.GetOnlyActiveAssets
                },
                m_sLanguage = language,
                m_sUserIP = Utils.Utils.GetClientIP(),
                m_nGroupID = groupId,
                m_nPageIndex = pageIndex,
                m_nPageSize = pageSize.Value,
                m_sQuery = query,
                m_nMediaTypes = mediaTypes,
                m_sSiteGuid = siteGuid,
                domainId = domainId,
                m_nUtcOffset = utcOffset
            };

            // build failover cache key
            StringBuilder key = new StringBuilder();
            key.AppendFormat("search_q={0}_pi={1}_pz={2}_g={3}_l={4}_mt={5}",
                query, pageIndex, pageSize, groupId, language, mediaTypes != null ? string.Join(",", mediaTypes.ToArray()) : string.Empty);

            result = CatalogUtils.GetMediaWithStatus(CatalogClientModule, request, key.ToString(), CacheDuration, with);

            return result;
        }

        public KalturaAssetInfoListResponse GetChannelMedia(int groupId, string siteGuid, int domainId, string udid, string language, int pageIndex, int? pageSize,
            int channelId, KalturaOrder? orderBy, List<KalturaCatalogWith> with, List<KeyValue> filterTags,
            WebAPI.Models.Catalog.KalturaAssetInfoFilter.KalturaCutWith cutWith)
        {
            KalturaAssetInfoListResponse result = new KalturaAssetInfoListResponse();

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

            // get group configuration 
            Group group = GroupsManager.GetGroup(groupId);

            // build request
            ChannelRequestMultiFiltering request = new ChannelRequestMultiFiltering()
            {
                m_sSignature = Signature,
                m_sSignString = SignString,
                m_oFilter = new Filter()
                {
                    m_sDeviceId = udid,
                    m_nLanguage = Utils.Utils.GetLanguageId(groupId, language),
                    m_bUseStartDate = group.UseStartDate,
                    m_bOnlyActiveMedia = group.GetOnlyActiveAssets,
                },
                m_lFilterTags = filterTags,
                m_eFilterCutWith = CatalogConvertor.ConvertCutWith(cutWith),
                m_sUserIP = Utils.Utils.GetClientIP(),
                m_nGroupID = groupId,
                m_nPageIndex = pageIndex,
                m_nPageSize = pageSize.Value,
                m_nChannelID = channelId,
                m_sSiteGuid = siteGuid,
                domainId = domainId,
                m_oOrderObj = order,
                m_bIgnoreDeviceRuleID = false
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
                result.Objects = CatalogUtils.GetMediaByIds(CatalogClientModule, channelResponse.m_nMedias, request, CacheDuration, with);
                result.TotalCount = channelResponse.m_nTotalItems;
            }
            return result;
        }

        public KalturaAssetInfoListResponse GetChannelAssets(int groupId, string siteGuid, int domainId, string udid, string language, 
            int pageIndex, int? pageSize,
            List<KalturaCatalogWith> with,
            int channelId, KalturaOrder? orderBy, string filterQuery)
        {
            KalturaAssetInfoListResponse result = new KalturaAssetInfoListResponse();

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

            // get group configuration 
            Group group = GroupsManager.GetGroup(groupId);

            // build request
            InternalChannelRequest request = new InternalChannelRequest()
            {
                m_sSignature = Signature,
                m_sSignString = SignString,
                m_oFilter = new Filter()
                {
                    m_sDeviceId = udid,
                    m_nLanguage = Utils.Utils.GetLanguageId(groupId, language),
                    m_bUseStartDate = group.UseStartDate,
                    m_bOnlyActiveMedia = group.GetOnlyActiveAssets
                },
                m_sUserIP = Utils.Utils.GetClientIP(),
                m_nGroupID = groupId,
                m_nPageIndex = pageIndex,
                m_nPageSize = pageSize.Value,
                m_sSiteGuid = siteGuid,
                domainId = domainId,
                order = order,
                internalChannelID = channelId.ToString(),
                filterQuery = filterQuery,
                m_dServerTime = getServerTime(),
                m_bIgnoreDeviceRuleID = false
            };

            // build failover cache key
            StringBuilder key = new StringBuilder();
            key.AppendFormat("channel_id={0}_pi={1}_pz={2}_g={3}_l={4}_o_{5}",
                channelId, pageIndex, pageSize, groupId, siteGuid, language, orderBy);

            // fire request
            UnifiedSearchResponse channelResponse = new UnifiedSearchResponse();
            if (!CatalogUtils.GetBaseResponse<UnifiedSearchResponse>(CatalogClientModule, request, out channelResponse, true, key.ToString()))
            {
                // general error
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (channelResponse.status.Code != (int)StatusCode.OK)
            {
                // Bad response received from WS
                throw new ClientException(channelResponse.status.Code, channelResponse.status.Message);
            }

            if (channelResponse.searchResults != null && channelResponse.searchResults.Count > 0)
            {
                // get base objects list
                List<BaseObject> assetsBaseDataList = channelResponse.searchResults.Select(x => x as BaseObject).ToList();

                // get assets from catalog/cache
                List<KalturaIAssetable> assetsInfo = CatalogUtils.GetAssets(CatalogClientModule, assetsBaseDataList, request, CacheDuration, with, CatalogConvertor.ConvertBaseObjectsToAssetsInfo);

                // build AssetInfoWrapper response
                if (assetsInfo != null)
                {
                    result.Objects = assetsInfo.Select(a => (KalturaAssetInfo)a).ToList();
                }

                result.TotalCount = channelResponse.m_nTotalItems;
            }

            return result;
        }

        [Obsolete]
        public KalturaAssetInfoListResponse GetMediaByIds(int groupId, string siteGuid, int domainId, string udid, string language, int pageIndex, int? pageSize, List<int> mediaIds, List<KalturaCatalogWith> with)
        {
            KalturaAssetInfoListResponse result = new KalturaAssetInfoListResponse();

            // get group configuration 
            Group group = GroupsManager.GetGroup(groupId);

            // build request
            MediaUpdateDateRequest request = new MediaUpdateDateRequest()
            {
                m_sSignature = Signature,
                m_sSignString = SignString,
                m_oFilter = new Filter()
                {
                    m_sDeviceId = udid,
                    m_nLanguage = Utils.Utils.GetLanguageId(groupId, language),
                    m_bUseStartDate = group.UseStartDate,
                    m_bOnlyActiveMedia = group.GetOnlyActiveAssets
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
                result.Objects = CatalogUtils.GetMediaByIds(CatalogClientModule, mediaIdsResponse.m_nMediaIds, request, CacheDuration, with);
                result.TotalCount = mediaIdsResponse.m_nTotalItems;
            }

            return result;
        }

        public KalturaAssetListResponse GetMediaByIds(int groupId, string siteGuid, int domainId, string udid, string language, int pageIndex, int? pageSize, List<int> mediaIds, KalturaAssetOrderBy orderBy)
        {
            KalturaAssetListResponse result = new KalturaAssetListResponse();

            // get group configuration 
            Group group = GroupsManager.GetGroup(groupId);

            // build request
            MediaUpdateDateRequest request = new MediaUpdateDateRequest()
            {
                m_sSignature = Signature,
                m_sSignString = SignString,
                m_oFilter = new Filter()
                {
                    m_sDeviceId = udid,
                    m_nLanguage = Utils.Utils.GetLanguageId(groupId, language),
                    m_bUseStartDate = group.UseStartDate,
                    m_bOnlyActiveMedia = group.GetOnlyActiveAssets
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
                result.Objects = Mapper.Map<List<KalturaAsset>>(mediaIdsResponse.m_lObj);
                result.TotalCount = mediaIdsResponse.m_nTotalItems;
            }

            return result;
        }

        [Obsolete]
        public KalturaAssetInfoListResponse GetEPGByInternalIds(int groupId, string siteGuid, int domainId, string udid, string language, int pageIndex, int? pageSize, List<int> epgIds,
            List<KalturaCatalogWith> with)
        {
            KalturaAssetInfoListResponse result = new KalturaAssetInfoListResponse();

            // get group configuration 
            Group group = GroupsManager.GetGroup(groupId);

            // build request
            EpgProgramDetailsRequest request = new EpgProgramDetailsRequest()
            {
                m_sSignature = Signature,
                m_sSignString = SignString,
                m_oFilter = new Filter()
                {
                    m_sDeviceId = udid,
                    m_nLanguage = Utils.Utils.GetLanguageId(groupId, language),
                    m_bUseStartDate = group.UseStartDate,
                    m_bOnlyActiveMedia = group.GetOnlyActiveAssets
                },
                m_sUserIP = Utils.Utils.GetClientIP(),
                m_nGroupID = groupId,
                m_nPageIndex = pageIndex,
                m_nPageSize = pageSize.Value,
                m_sSiteGuid = siteGuid,
                domainId = domainId,
                m_lProgramsIds = epgIds,
            };

            EpgProgramResponse epgProgramResponse = null;

            if (CatalogUtils.GetBaseResponse(CatalogClientModule, request, out epgProgramResponse) && epgProgramResponse != null)
            {

                var list = CatalogConvertor.ConvertBaseObjectsToAssetsInfo(groupId, epgProgramResponse.m_lObj, with);

                // build AssetInfoWrapper response
                if (list != null)
                {
                    result.Objects = list.Select(a => (KalturaAssetInfo)a).ToList();
                    result.TotalCount = epgProgramResponse.m_nTotalItems;
                }
                else
                {
                    throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
                }
            }

            return result;
        }

        public KalturaAssetListResponse GetEPGByInternalIds(int groupId, string siteGuid, int domainId, string udid, string language, int pageIndex, int? pageSize, List<int> epgIds, KalturaAssetOrderBy orderBy)
        {
            KalturaAssetListResponse result = new KalturaAssetListResponse();

            // get group configuration 
            Group group = GroupsManager.GetGroup(groupId);

            // build request
            EpgProgramDetailsRequest request = new EpgProgramDetailsRequest()
            {
                m_sSignature = Signature,
                m_sSignString = SignString,
                m_oFilter = new Filter()
                {
                    m_sDeviceId = udid,
                    m_nLanguage = Utils.Utils.GetLanguageId(groupId, language),
                    m_bUseStartDate = group.UseStartDate,
                    m_bOnlyActiveMedia = group.GetOnlyActiveAssets
                },
                m_sUserIP = Utils.Utils.GetClientIP(),
                m_nGroupID = groupId,
                m_nPageIndex = pageIndex,
                m_nPageSize = pageSize.Value,
                m_sSiteGuid = siteGuid,
                domainId = domainId,
                m_lProgramsIds = epgIds,
            };

            EpgProgramResponse epgProgramResponse = null;

            if (CatalogUtils.GetBaseResponse(CatalogClientModule, request, out epgProgramResponse) && epgProgramResponse != null)
            {
                result.Objects = Mapper.Map<List<KalturaAsset>>(epgProgramResponse.m_lObj);

                if (result.Objects != null)
                {
                    result.TotalCount = epgProgramResponse.m_nTotalItems;
                }
                else
                {
                    throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
                }
            }

            return result;
        }

        [Obsolete]
        public KalturaAssetInfoListResponse GetEPGByExternalIds(int groupId, string siteGuid, int domainId, string udid, string language, int pageIndex, int? pageSize, List<string> epgIds,
            List<KalturaCatalogWith> with)
        {
            KalturaAssetInfoListResponse result = new KalturaAssetInfoListResponse();

            // get group configuration 
            Group group = GroupsManager.GetGroup(groupId);

            // build request
            EPGProgramsByProgramsIdentefierRequest request = new EPGProgramsByProgramsIdentefierRequest()
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
                pids = epgIds,
                eLang = Catalog.Language.English,
                duration = 0
            };

            EpgProgramsResponse epgProgramResponse = null;

            if (CatalogUtils.GetBaseResponse(CatalogClientModule, request, out epgProgramResponse) && epgProgramResponse != null)
            {

                var list = CatalogConvertor.ConvertEPGChannelProgrammeObjectToAssetsInfo(groupId, epgProgramResponse.lEpgList, with);

                // build AssetInfoWrapper response
                if (list != null)
                {
                    result.Objects = list.Select(a => (KalturaAssetInfo)a).ToList();
                    result.TotalCount = epgProgramResponse.m_nTotalItems;
                }
                else
                {
                    throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
                }
            }

            return result;
        }

        public KalturaAssetListResponse GetEPGByExternalIds(int groupId, string siteGuid, int domainId, string udid, string language, int pageIndex, int? pageSize, List<string> epgIds,
            KalturaAssetOrderBy orderBy)
        {
            KalturaAssetListResponse result = new KalturaAssetListResponse();

            // get group configuration 
            Group group = GroupsManager.GetGroup(groupId);

            // build request
            EPGProgramsByProgramsIdentefierRequest request = new EPGProgramsByProgramsIdentefierRequest()
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
                pids = epgIds,
                eLang = Catalog.Language.English,
                duration = 0
            };

            EpgProgramsResponse epgProgramResponse = null;

            if (CatalogUtils.GetBaseResponse(CatalogClientModule, request, out epgProgramResponse) && epgProgramResponse != null)
            {
                result.Objects = Mapper.Map<List<KalturaAsset>>(epgProgramResponse.lEpgList);

                if (result.Objects != null)
                {
                    result.TotalCount = epgProgramResponse.m_nTotalItems;
                }
                else
                {
                    throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
                }
            }

            return result;
        }

        internal List<KalturaEPGChannelAssets> GetEPGByChannelIds(int groupId, string userID, int domainId, string udid, string language, int pageIndex, int? pageSize, List<int> epgIds, DateTime startTime, DateTime endTime, List<KalturaCatalogWith> with)
        {
            List<KalturaEPGChannelAssets> result = new List<KalturaEPGChannelAssets>();

            // get group configuration 
            Group group = GroupsManager.GetGroup(groupId);

            // build request
            EpgRequest request = new EpgRequest()
            {
                m_sSignature = Signature,
                m_sSignString = SignString,
                m_oFilter = new Filter()
                {
                    m_sDeviceId = udid,
                    m_nLanguage = Utils.Utils.GetLanguageId(groupId, language),
                    m_bUseStartDate = group.UseStartDate,
                    m_bOnlyActiveMedia = group.GetOnlyActiveAssets
                },
                m_sUserIP = Utils.Utils.GetClientIP(),
                m_nGroupID = groupId,
                m_nPageIndex = pageIndex,
                m_nPageSize = pageSize.Value,
                m_sSiteGuid = userID,
                domainId = domainId,
                m_nChannelIDs = epgIds,
                m_dStartDate = startTime,
                m_dEndDate = endTime
            };

            EpgResponse epgProgramResponse = null;

            var isBaseResponse = CatalogUtils.GetBaseResponse <EpgResponse>(CatalogClientModule, request, out  epgProgramResponse);
            if (isBaseResponse && epgProgramResponse != null)
            {
                result = CatalogConvertor.ConvertEPGChannelAssets(groupId, epgProgramResponse.programsPerChannel, with);

                if (result == null)
                {
                    throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
                }
            }

            return result;

        }


        public WebAPI.Models.Catalog.KalturaChannel GetChannelInfo(int groupId, string siteGuid, int domainId, string udid, string language, int channelId)
        {
            WebAPI.Models.Catalog.KalturaChannel result = null;
            ChannelObjRequest request = new ChannelObjRequest()
            {
                m_sSignature = Signature,
                m_sSignString = SignString,
                m_oFilter = new Filter()
                {
                    m_sDeviceId = udid,
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

        public KalturaOTTCategory GetCategory(int groupId, string siteGuid, int domainId, string udid, string language, int categoryId)
        {
            KalturaOTTCategory result = null;
            CategoryRequest request = new CategoryRequest()
            {
                m_sSignature = Signature,
                m_sSignString = SignString,
                m_oFilter = new Filter()
                {
                    m_sDeviceId = udid,
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
                result = Mapper.Map<KalturaOTTCategory>(response);
            }
            else
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            return result;
        }

        [Obsolete]
        public KalturaAssetsBookmarksResponse GetAssetsBookmarksOldStandard(string siteGuid, int groupId, int domainId, string udid, List<KalturaSlimAsset> assets)
        {
            List<KalturaAssetBookmarks> result = null;
            List<AssetBookmarkRequest> assetsToRequestPositions = new List<AssetBookmarkRequest>();

            foreach (KalturaSlimAsset asset in assets)
            {
                AssetBookmarkRequest assetInfo = new AssetBookmarkRequest();
                assetInfo.AssetID = asset.Id;
                bool addToRequest = true;
                switch (asset.Type)
                {
                    case KalturaAssetType.media:
                        assetInfo.AssetType = eAssetTypes.MEDIA;
                        break;
                    case KalturaAssetType.recording:
                        assetInfo.AssetType = eAssetTypes.NPVR;
                        break;
                    case KalturaAssetType.epg:
                        assetInfo.AssetType = eAssetTypes.EPG;
                        break;
                    default:
                        assetInfo.AssetType = eAssetTypes.UNKNOWN;
                        addToRequest = false;
                        break;
                }
                if (addToRequest)
                {
                    assetsToRequestPositions.Add(assetInfo);
                }
            }

            AssetsBookmarksRequest request = new AssetsBookmarksRequest()
            {
                m_sSignature = Signature,
                m_sSignString = SignString,
                m_sSiteGuid = siteGuid,
                m_nGroupID = groupId,
                m_sUserIP = Utils.Utils.GetClientIP(),
                domainId = domainId,
                m_oFilter = new Filter()
                {
                    m_sDeviceId = udid
                },
                Data = new AssetsBookmarksRequestData()
                {
                    Assets = assetsToRequestPositions
                }
            };

            AssetsBookmarksResponse response = null;
            if (!CatalogUtils.GetBaseResponse(CatalogClientModule, request, out response) || response == null || response.Status == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }
            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            result = Mapper.Map<List<KalturaAssetBookmarks>>(response.AssetsBookmarks);

            return new KalturaAssetsBookmarksResponse() { AssetsBookmarks = result, TotalCount = response.m_nTotalItems };

        }

        public KalturaBookmarkListResponse GetAssetsBookmarks(string siteGuid, int groupId, int domainId, string udid, List<KalturaSlimAsset> assets, KalturaBookmarkOrderBy orderBy)
        {
            List<KalturaBookmark> result = null;
            List<AssetBookmarkRequest> assetsToRequestPositions = new List<AssetBookmarkRequest>();

            foreach (KalturaSlimAsset asset in assets)
            {
                AssetBookmarkRequest assetInfo = new AssetBookmarkRequest();
                assetInfo.AssetID = asset.Id;
                bool addToRequest = true;
                switch (asset.Type)
                {
                    case KalturaAssetType.media:
                        assetInfo.AssetType = eAssetTypes.MEDIA;
                        break;
                    case KalturaAssetType.recording:
                        assetInfo.AssetType = eAssetTypes.NPVR;
                        break;
                    case KalturaAssetType.epg:
                        assetInfo.AssetType = eAssetTypes.EPG;
                        break;
                    default:
                        assetInfo.AssetType = eAssetTypes.UNKNOWN;
                        addToRequest = false;
                        break;
                }
                if (addToRequest)
                {
                    assetsToRequestPositions.Add(assetInfo);
                }
            }

            AssetsBookmarksRequest request = new AssetsBookmarksRequest()
            {
                m_sSignature = Signature,
                m_sSignString = SignString,
                m_sSiteGuid = siteGuid,
                m_nGroupID = groupId,
                m_sUserIP = Utils.Utils.GetClientIP(),
                domainId = domainId,
                m_oFilter = new Filter()
                {
                    m_sDeviceId = udid
                },
                Data = new AssetsBookmarksRequestData()
                {
                    Assets = assetsToRequestPositions
                }
            };

            AssetsBookmarksResponse response = null;
            if (!CatalogUtils.GetBaseResponse(CatalogClientModule, request, out response) || response == null || response.Status == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }
            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            result = CatalogMappings.ConvertBookmarks(response.AssetsBookmarks, orderBy);

            return new KalturaBookmarkListResponse() { AssetsBookmarks = result, TotalCount = response.m_nTotalItems };

        }

        public KalturaAssetInfoListResponse GetExternalChannelAssets(int groupId, string channelId, 
            string siteGuid, int domainId, string udid, string language, int pageIndex, int? pageSize,
            KalturaOrder? orderBy, List<KalturaCatalogWith> with,
            string deviceType = null, string utcOffset = null, string freeParam = null)
        {
            KalturaAssetInfoListResponse result = new KalturaAssetInfoListResponse();

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

            // get group configuration 
            Group group = GroupsManager.GetGroup(groupId);

            // build request
            ExternalChannelRequest request = new ExternalChannelRequest()
            {
                m_sSignature = Signature,
                m_sSignString = SignString,
                deviceId = udid,
                deviceType = deviceType,
                domainId = domainId,
                internalChannelID = channelId,
                m_nGroupID = groupId,
                m_nPageIndex = pageIndex,
                m_nPageSize = pageSize.Value,
                m_oFilter = new Filter()
                {
                    m_sDeviceId = udid,
                    m_nLanguage = Utils.Utils.GetLanguageId(groupId, language),
                    m_bUseStartDate = group.UseStartDate,
                    m_bOnlyActiveMedia = group.GetOnlyActiveAssets
                },
                m_sSiteGuid = siteGuid,
                m_sUserIP = Utils.Utils.GetClientIP(),
                utcOffset = utcOffset,
                free = freeParam
            };

            // build failover cache key
            StringBuilder key = new StringBuilder();
            key.AppendFormat("external_channel_id={0}_pi={1}_pz={2}_g={3}_l={4}_o_{5}",
                channelId, pageIndex, pageSize, groupId, siteGuid, language, orderBy);

            // fire search request
            UnifiedSearchExternalResponse searchResponse = new UnifiedSearchExternalResponse();

            if (!CatalogUtils.GetBaseResponse<UnifiedSearchExternalResponse>(CatalogClientModule, request, out searchResponse, true, key.ToString()))
            {
                // general error
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (searchResponse == null || searchResponse.status == null)
            { 
                // Bad response received from WS
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
                List<KalturaIAssetable> assetsInfo = 
                    CatalogUtils.GetAssets(CatalogClientModule, assetsBaseDataList, request, CacheDuration, with, CatalogConvertor.ConvertBaseObjectsToAssetsInfo);

                // build AssetInfoWrapper response
                if (assetsInfo != null)
                {
                    result.Objects = assetsInfo.Select(a => (KalturaAssetInfo)a).ToList();
                }

                result.TotalCount = searchResponse.m_nTotalItems;
            }

            result.RequestId = searchResponse.requestId;

            return result;
        }

        internal bool AddBookmark(int groupId, string siteGuid, int householdId, string udid, string assetId, KalturaAssetType assetType, long fileId, int Position, string action, int averageBitRate, int totalBitRate, int currentBitRate)
        {
            int t;

            if (assetType != KalturaAssetType.recording)                
                if (string.IsNullOrEmpty(assetId) || !int.TryParse(assetId, out t))
                    throw new ClientException((int)StatusCode.BadRequest, "Invalid Asset id");

            eAssetTypes CatalogAssetType = eAssetTypes.UNKNOWN;
            switch (assetType)
            {
                case KalturaAssetType.epg:
                    CatalogAssetType = eAssetTypes.EPG;
                    break;
                case KalturaAssetType.media:
                    CatalogAssetType = eAssetTypes.MEDIA;
                    break;
                case KalturaAssetType.recording:
                    CatalogAssetType = eAssetTypes.NPVR;
                    break;
            }

            // build request
            MediaMarkRequest request = new MediaMarkRequest()
            {
                m_sSignature = Signature,
                m_sSignString = SignString,
                domainId = householdId,
                m_nGroupID = groupId,
                m_sSiteGuid = siteGuid,
                m_oMediaPlayRequestData = new MediaPlayRequestData()
                {
                    m_eAssetType = CatalogAssetType,
                    m_nLoc = Position,
                    m_nMediaFileID = (int)fileId,
                    m_sAssetID = assetId,
                    m_sAction = action,
                    m_sSiteGuid = siteGuid,
                    m_sUDID = udid,
                    m_nAvgBitRate = averageBitRate,
                    m_nCurrentBitRate = currentBitRate,
                    m_nTotalBitRate = totalBitRate
                }
            };
            
            // fire search request
            MediaMarkResponse response = new MediaMarkResponse();

            if (!CatalogUtils.GetBaseResponse<MediaMarkResponse>(CatalogClientModule, request, out response))
            {
                // general error
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.status.Code != (int)StatusCode.OK)
            {
                // Bad response received from WS
                throw new ClientException(response.status.Code, response.status.Message);
            }

            return true;
        }

        internal List<KalturaSlimAsset> GetAssetsFollowing(string userID, int groupId, List<KalturaPersonalAssetRequest> assets, List<string> followPhrases)
        {
            List<KalturaSlimAsset> result = new List<KalturaSlimAsset>();

            // Create our own filter - only search in title
            string filter = "(or";
            followPhrases.ForEach(x => filter += string.Format(" {0}", x));
            filter += ")";

            // get group configuration 
            Group group = GroupsManager.GetGroup(groupId);

            // build request
            UnifiedSearchRequest request = new UnifiedSearchRequest()
            {
                m_sSignature = Signature,
                m_sSignString = SignString,
                m_oFilter = new Filter()
                {
                    m_bOnlyActiveMedia = group.GetOnlyActiveAssets
                },
                m_sUserIP = Utils.Utils.GetClientIP(),
                m_nGroupID = groupId,
                filterQuery = filter,
                m_dServerTime = getServerTime(),
                specificAssets = assets.Select(asset => new KeyValuePairOfeAssetTypeslongHVR2FNfI(){ key = eAssetTypes.MEDIA, value = asset.getId() }).ToList()
                //assetTypes = assetTypes,
            };

            // fire unified search request
            UnifiedSearchResponse searchResponse = new UnifiedSearchResponse();
            if (!CatalogUtils.GetBaseResponse<UnifiedSearchResponse>(CatalogClientModule, request, out searchResponse, true, null))
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
                foreach (var searchRes in searchResponse.searchResults)
                {
                    result.Add(Mapper.Map<KalturaSlimAsset>(searchRes));
                }
            }

            return result;
        }

        internal KalturaCountry GetCountryByIp(int groupId, string ip)
        {
            KalturaCountry result = null;
            CountryRequest request = new CountryRequest()
            {
                m_sSignature = Signature,
                m_sSignString = SignString,
                m_oFilter = new Filter(),
                m_nGroupID = groupId,
                m_sUserIP = Utils.Utils.GetClientIP(),
                Ip = ip
            };

            CountryResponse response = null;
            if (CatalogUtils.GetBaseResponse(CatalogClientModule, request, out response) && response != null && response.Status != null)
            {
                if (response.Status.Code == (int)StatusCode.OK)
                {
                    result = Mapper.Map<KalturaCountry>(response.Country);
                }
                else
                {
                    throw new ClientException(response.Status.Code, response.Status.Message);
                }
            }
            else
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            return result;
        }

        internal KalturaAssetListResponse GetExternalChannelAssets(int groupId, string channelId, string userID, int domainId, string udid, string language, int pageIndex, int? pageSize, 
            KalturaAssetOrderBy orderBy, string deviceType, string utcOffset, string freeParam)
        {
            KalturaAssetListResponse result = new KalturaAssetListResponse();

            // convert order by
            OrderObj order = CatalogConvertor.ConvertOrderToOrderObj(orderBy);

            // get group configuration 
            Group group = GroupsManager.GetGroup(groupId);

            // build request
            ExternalChannelRequest request = new ExternalChannelRequest()
            {
                m_sSignature = Signature,
                m_sSignString = SignString,
                deviceId = udid,
                deviceType = deviceType,
                domainId = domainId,
                internalChannelID = channelId,
                m_nGroupID = groupId,
                m_nPageIndex = pageIndex,
                m_nPageSize = pageSize.Value,                
                m_oFilter = new Filter()
                {
                    m_sDeviceId = udid,
                    m_nLanguage = Utils.Utils.GetLanguageId(groupId, language),
                    m_bUseStartDate = group.UseStartDate,
                    m_bOnlyActiveMedia = group.GetOnlyActiveAssets
                },
                m_sSiteGuid = userID,
                m_sUserIP = Utils.Utils.GetClientIP(),
                utcOffset = utcOffset,
                free = freeParam
            };

            // build failover cache key
            StringBuilder key = new StringBuilder();
            key.AppendFormat("external_channel_id={0}_pi={1}_pz={2}_g={3}_l={4}_o_{5}",
                channelId, pageIndex, pageSize, groupId, userID, language, orderBy);

            // fire search request
            UnifiedSearchExternalResponse searchResponse = new UnifiedSearchExternalResponse();

            if (!CatalogUtils.GetBaseResponse<UnifiedSearchExternalResponse>(CatalogClientModule, request, out searchResponse, true, key.ToString()))
            {
                // general error
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (searchResponse == null || searchResponse.status == null)
            {
                // Bad response received from WS
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
                result.Objects = CatalogUtils.GetAssets(CatalogClientModule, assetsBaseDataList, request, CacheDuration);

                result.TotalCount = searchResponse.m_nTotalItems;
            }

            //result..RequestId = searchResponse.requestId;

            return result;
        }

        internal KalturaAssetListResponse GetRelatedMediaExternal(int groupId, string userID, int domainId, string udid, string language, int pageIndex, int? pageSize, int mediaId, 
            List<int> mediaTypes, int utcOffset, string freeParam)
        {
            KalturaAssetListResponse result = new KalturaAssetListResponse();

            // get group configuration 
            Group group = GroupsManager.GetGroup(groupId);

            // build request
            MediaRelatedExternalRequest request = new MediaRelatedExternalRequest()
            {
                m_sSignature = Signature,
                m_sSignString = SignString,
                m_oFilter = new Filter()
                {
                    m_sDeviceId = udid,
                    m_nLanguage = Utils.Utils.GetLanguageId(groupId, language),
                    m_bUseStartDate = group.UseStartDate,
                    m_bOnlyActiveMedia = group.GetOnlyActiveAssets
                },
                m_sLanguage = language,
                m_sUserIP = Utils.Utils.GetClientIP(),
                m_nGroupID = groupId,
                m_nPageIndex = pageIndex,
                m_nPageSize = pageSize.Value,
                m_nMediaID = mediaId,
                m_nMediaTypes = mediaTypes,
                m_sSiteGuid = userID,
                domainId = domainId,
                m_nUtcOffset = utcOffset,
                m_sFreeParam = freeParam
            };

            // build failover cache key
            StringBuilder key = new StringBuilder();
            key.AppendFormat("related_media_id={0}_pi={1}_pz={2}_g={3}_l={4}_mt={5}",
                mediaId, pageIndex, pageSize, groupId, language, mediaTypes != null ? string.Join(",", mediaTypes.ToArray()) : string.Empty);

            result = CatalogUtils.GetMediaWithStatus(CatalogClientModule, request, key.ToString(), CacheDuration);

            return result;
        }

        internal KalturaAssetListResponse GetSearchMediaExternal(int groupId, string userID, int domainId, string udid, string language, int pageIndex, int? pageSize, string query, 
            List<int> mediaTypes, int utcOffset)
        {
            KalturaAssetListResponse result = new KalturaAssetListResponse();

            // get group configuration 
            Group group = GroupsManager.GetGroup(groupId);

            // build request
            MediaSearchExternalRequest request = new MediaSearchExternalRequest()
            {
                m_sSignature = Signature,
                m_sSignString = SignString,
                m_oFilter = new Filter()
                {
                    m_sDeviceId = udid,
                    m_nLanguage = Utils.Utils.GetLanguageId(groupId, language),
                    m_bUseStartDate = group.UseStartDate,
                    m_bOnlyActiveMedia = group.GetOnlyActiveAssets
                },
                m_sLanguage = language,
                m_sUserIP = Utils.Utils.GetClientIP(),
                m_nGroupID = groupId,
                m_nPageIndex = pageIndex,
                m_nPageSize = pageSize.Value,
                m_sQuery = query,
                m_nMediaTypes = mediaTypes,
                m_sSiteGuid = userID,
                domainId = domainId,
                m_nUtcOffset = utcOffset
            };

            // build failover cache key
            StringBuilder key = new StringBuilder();
            key.AppendFormat("search_q={0}_pi={1}_pz={2}_g={3}_l={4}_mt={5}",
                query, pageIndex, pageSize, groupId, language, mediaTypes != null ? string.Join(",", mediaTypes.ToArray()) : string.Empty);

            result = CatalogUtils.GetMediaWithStatus(CatalogClientModule, request, key.ToString(), CacheDuration);

            return result;
        }

        internal KalturaAssetListResponse GetChannelAssets(int groupId, string userID, int domainId, string udid, string language, int pageIndex, int? pageSize, int id, 
            KalturaAssetOrderBy? orderBy, string filterQuery)
        {
            KalturaAssetListResponse result = new KalturaAssetListResponse();

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

            // get group configuration 
            Group group = GroupsManager.GetGroup(groupId);

            // build request
            InternalChannelRequest request = new InternalChannelRequest()
            {
                m_sSignature = Signature,
                m_sSignString = SignString,
                m_oFilter = new Filter()
                {
                    m_sDeviceId = udid,
                    m_nLanguage = Utils.Utils.GetLanguageId(groupId, language),
                    m_bUseStartDate = group.UseStartDate,
                    m_bOnlyActiveMedia = group.GetOnlyActiveAssets
                },
                m_sUserIP = Utils.Utils.GetClientIP(),
                m_nGroupID = groupId,
                m_nPageIndex = pageIndex,
                m_nPageSize = pageSize.Value,
                m_sSiteGuid = userID,
                domainId = domainId,
                order = order,
                internalChannelID = id.ToString(),
                filterQuery = filterQuery,
                m_dServerTime = getServerTime(),
                m_bIgnoreDeviceRuleID = false
            };

            // build failover cache key
            StringBuilder key = new StringBuilder();
            key.AppendFormat("channel_id={0}_pi={1}_pz={2}_g={3}_l={4}_o_{5}",
                id, pageIndex, pageSize, groupId, userID, language, orderBy);

            // fire request
            UnifiedSearchResponse channelResponse = new UnifiedSearchResponse();
            if (!CatalogUtils.GetBaseResponse<UnifiedSearchResponse>(CatalogClientModule, request, out channelResponse, true, key.ToString()))
            {
                // general error
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (channelResponse.status.Code != (int)StatusCode.OK)
            {
                // Bad response received from WS
                throw new ClientException(channelResponse.status.Code, channelResponse.status.Message);
            }

            if (channelResponse.searchResults != null && channelResponse.searchResults.Count > 0)
            {
                // get base objects list
                List<BaseObject> assetsBaseDataList = channelResponse.searchResults.Select(x => x as BaseObject).ToList();

                // get assets from catalog/cache
                result.Objects = CatalogUtils.GetAssets(CatalogClientModule, assetsBaseDataList, request, CacheDuration);
                result.TotalCount = channelResponse.m_nTotalItems;
            }

            return result;
        }

        internal KalturaAssetListResponse GetBundleAssets(int groupId, string userID, int domainId, string udid, string language, int pageIndex, int? pageSize, int id,
            KalturaAssetOrderBy? orderBy, List<int> mediaTypes, KalturaBundleType bundleType)
        {
            KalturaAssetListResponse result = new KalturaAssetListResponse();

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

            // get group configuration 
            Group group = GroupsManager.GetGroup(groupId);

            // build request
            BundleAssetsRequest request = new BundleAssetsRequest()
            {
                m_sSignature = Signature,
                m_sSignString = SignString,
                m_oFilter = new Filter()
                {
                    m_sDeviceId = udid,
                    m_nLanguage = Utils.Utils.GetLanguageId(groupId, language),
                    m_bUseStartDate = group.UseStartDate,
                    m_bOnlyActiveMedia = group.GetOnlyActiveAssets
                },
                m_sUserIP = Utils.Utils.GetClientIP(),
                m_nGroupID = groupId,
                m_nPageIndex = pageIndex,
                m_nPageSize = pageSize.Value,
                m_sSiteGuid = userID,
                domainId = domainId,
                m_oOrderObj = order,
                m_sMediaType = mediaTypes != null ? string.Join(";", mediaTypes.ToArray()) : null,
                m_dServerTime = getServerTime(),
                m_eBundleType = bundleType == KalturaBundleType.collection ? eBundleType.COLLECTION : eBundleType.SUBSCRIPTION,
                m_nBundleID = id
            };

              // build failover cache key
            StringBuilder key = new StringBuilder();
            key.AppendFormat("bundle_id={0}_pi={1}_pz={2}_g={3}_l={4}_mt={5}_type={6}",
                id, pageIndex, pageSize, groupId, language, mediaTypes != null ? string.Join(",", mediaTypes.ToArray()) : string.Empty, bundleType.ToString());

            result = CatalogUtils.GetMedia(CatalogClientModule, request, key.ToString(), CacheDuration);
           
            return result;
        }

        internal KalturaAssetCommentListResponse GetAssetCommentsList(int groupId, string language, int id, KalturaAssetType AssetType, string userId, int domainId, string udid,
            int pageIndex, int? pageSize, KalturaAssetCommentOrderBy? orderBy)
        {
            KalturaAssetCommentListResponse result = new KalturaAssetCommentListResponse();

            // get group configuration 
            Group group = GroupsManager.GetGroup(groupId);
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
            AssetCommentsRequest request = new AssetCommentsRequest()
            {
                m_sSignature = Signature,
                m_sSignString = SignString,
                m_oFilter = new Filter()
                {
                    m_sDeviceId = udid,
                    m_nLanguage = Utils.Utils.GetLanguageId(groupId, language),
                    m_bUseStartDate = group.UseStartDate,
                    m_bOnlyActiveMedia = group.GetOnlyActiveAssets
                },
                m_sUserIP = Utils.Utils.GetClientIP(),
                m_nGroupID = groupId,
                m_nPageIndex = pageIndex,
                m_nPageSize = pageSize.Value,
                m_sSiteGuid = userId,
                domainId = domainId,            
                m_dServerTime = getServerTime(),
                assetId = id,
                assetType = CatalogMappings.ConvertToAssetType(AssetType),
                orderObj = order,
            };

            // build failover cache key
            StringBuilder key = new StringBuilder();
            key.AppendFormat("asset_id={0}_pi={1}_pz={2}_g={3}_l={4}_type={5}",
                id, pageIndex, pageSize, groupId, language, eAssetType.PROGRAM.ToString());
            AssetCommentsListResponse commentResponse = new AssetCommentsListResponse();
            if (CatalogUtils.GetBaseResponse<AssetCommentsListResponse>(CatalogClientModule, request, out commentResponse))
            {
                if (commentResponse.status.Code != (int)StatusCode.OK)
                {
                    // Bad response received from WS
                    throw new ClientException(commentResponse.status.Code, commentResponse.status.Message);
                }
                else
                {
                    result.Objects = commentResponse.Comments != null ?
                        Mapper.Map<List<KalturaAssetComment>>(commentResponse.Comments) : null;
                    if (result.Objects != null)
                    {
                        result.TotalCount = commentResponse.m_nTotalItems;
                    }
                }
            }
            else
            {
                // general error
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }
            return result;
        }

        internal KalturaAssetComment AddAssetComment(int groupId, int assetId, KalturaAssetType assetType, string userId, int domainId, string writer, string header,
                                                     string subHeader, string contextText, string udid, string language, bool shouldAutoActive = true)
        {
            KalturaAssetComment result = new KalturaAssetComment();

            // get group configuration 
            Group group = GroupsManager.GetGroup(groupId);

            // build request
            AssetCommentAddRequest request = new AssetCommentAddRequest()
            {
                m_sSignature = Signature,
                m_sSignString = SignString,
                m_oFilter = new Filter()
                {
                    m_sDeviceId = udid,
                    m_nLanguage = Utils.Utils.GetLanguageId(groupId, language),
                    m_bUseStartDate = group.UseStartDate,
                    m_bOnlyActiveMedia = group.GetOnlyActiveAssets
                },
                m_sUserIP = Utils.Utils.GetClientIP(),
                m_nGroupID = groupId,
                m_sSiteGuid = userId,
                domainId = domainId,
                m_dServerTime = getServerTime(),
                assetId = assetId,
                assetType = CatalogMappings.ConvertToAssetType(assetType),
                writer = writer,
                header = header,
                subHeader = subHeader,
                contentText = contextText,
                udid = udid,
                shouldAutoActive = shouldAutoActive
            };

            AssetCommentResponse assetCommentResponse = null;
            if (CatalogUtils.GetBaseResponse<AssetCommentResponse>(CatalogClientModule, request, out assetCommentResponse))
            {
                if (assetCommentResponse.Status.Code != (int)StatusCode.OK)
                {
                    // Bad response received from WS
                    throw new ClientException(assetCommentResponse.Status.Code, assetCommentResponse.Status.Message);
                }
                else
                {
                    result = assetCommentResponse.AssetComment != null ? Mapper.Map<KalturaAssetComment>(assetCommentResponse.AssetComment) : null;
                }
            }
            else
            {
                // general error
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            return result;
        }
    }
}