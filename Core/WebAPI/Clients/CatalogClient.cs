using ApiLogic.Catalog;
using ApiLogic.Catalog.CatalogManagement.Managers;
using ApiLogic.Catalog.CatalogManagement.Services;
using ApiLogic.IndexManager.QueryBuilders;
using APILogic.CRUD;
using ApiObjects;
using ApiObjects.BulkUpload;
using ApiObjects.Catalog;
using ApiObjects.Response;
using ApiObjects.SearchObjects;
using ApiObjects.SearchPriorityGroups;
using AutoMapper;
using Catalog.Response;
using Core.Catalog;
using Core.Catalog.CatalogManagement;
using Core.Catalog.Request;
using Core.Catalog.Response;
using Core.GroupManagers;
using GroupsCacheManager;
using KalturaRequestContext;
using Phx.Lib.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using ApiLogic.Catalog.CatalogManagement.Services.GroupRepresentatives;
using ApiObjects.SearchObjects.GroupRepresentatives;
using GroupsCacheManager.Mappers;
using TVinciShared;
using WebAPI.ClientManagers;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.InternalModels;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Api;
using WebAPI.Models.API;
using WebAPI.Models.Catalog;
using WebAPI.Models.Catalog.SearchPriority;
using WebAPI.Models.Catalog.SearchPriorityGroup;
using WebAPI.Models.General;
using WebAPI.Models.Upload;
using WebAPI.Models.Users;
using WebAPI.ObjectsConvertor;
using WebAPI.ObjectsConvertor.Extensions;
using WebAPI.ObjectsConvertor.GroupRepresentatives;
using WebAPI.ObjectsConvertor.Mapping;
using WebAPI.ObjectsConvertor.Ordering;
using WebAPI.Utils;
using Channel = GroupsCacheManager.Channel;
using CountryResponse = Core.Catalog.Response.CountryResponse;
using Group = WebAPI.Managers.Models.Group;
using Language = ApiObjects.Language;
using MetaType = ApiObjects.MetaType;
using OrderDir = ApiObjects.SearchObjects.OrderDir;
using Ratio = Core.Catalog.Ratio;
using SearchAssetsFilter = WebAPI.InternalModels.SearchAssetsFilter;
using WebAPI.ObjectsConvertor.Extensions;
using ApiObjects.Base;

namespace WebAPI.Clients
{
    public class CatalogClient : BaseClient
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private const string EPG_DATETIME_FORMAT = "dd/MM/yyyy HH:mm:ss";
        private const string OPC_MERGE_VERSION = "5.0.0.0";
        private readonly Version opcMergeVersion = new Version(OPC_MERGE_VERSION);

        private readonly ISearchPriorityGroupManager _searchPriorityGroupManager = SearchPriorityGroupManager.Instance;

        public string Signature { get; set; }
        public string SignString { get; set; }
        public int CacheDuration { get; set; }

        public string SignatureKey
        {
            set
            {
                SignString = Guid.NewGuid().ToString();
                Signature = GetSignature(SignString, value);
            }
        }

        #region New Catalog Management

        public KalturaAssetStructListResponse GetAssetStructs(int groupId, KalturaBaseAssetStructFilter filter)
        {
            GenericListResponse<AssetStruct> GetAssetStructsListFunc() => filter.GetResponse(groupId);
            var response = ClientUtils.GetResponseListFromWS<KalturaAssetStruct, AssetStruct>(GetAssetStructsListFunc);
            if (response.TotalCount > 0)
            {
                response.Objects = AssetStructUtils.GetSortedAssetStructs(response.Objects, filter.OrderBy).ToList();
            }

            return new KalturaAssetStructListResponse
            {
                TotalCount = response.TotalCount,
                AssetStructs = response.Objects
            };
        }

        public KalturaAssetStruct AddAssetStruct(int groupId, KalturaAssetStruct assetStrcut, long userId)
        {
            Func<AssetStruct, GenericResponse<AssetStruct>> addAssetStructFunc = (AssetStruct assetStructToAdd) =>
                CatalogManager.Instance.AddAssetStruct(groupId, assetStructToAdd, userId);

            KalturaAssetStruct result =
                ClientUtils.GetResponseFromWS<KalturaAssetStruct, AssetStruct>(assetStrcut, addAssetStructFunc);

            return result;
        }

        public KalturaAssetStruct UpdateAssetStruct(int groupId, long id, KalturaAssetStruct assetStrcut, long userId)
        {
            bool shouldUpdateMetaIds = assetStrcut.MetaIds != null;
            Func<AssetStruct, GenericResponse<AssetStruct>> updateAssetStructFunc = (AssetStruct assetStructToUpdate) =>
                CatalogManager.Instance.UpdateAssetStruct(groupId, id, assetStructToUpdate, shouldUpdateMetaIds, userId);

            KalturaAssetStruct result =
                ClientUtils.GetResponseFromWS<KalturaAssetStruct, AssetStruct>(assetStrcut, updateAssetStructFunc);

            return result;
        }

        public bool DeleteAssetStruct(int groupId, long id, long userId)
        {
            Func<Status> deleteAssetStructFunc = () => CatalogManager.Instance.DeleteAssetStruct(groupId, id, userId);
            return ClientUtils.GetResponseStatusFromWS(deleteAssetStructFunc);
        }

        public KalturaAssetStruct GetAssetStruct(int groupId, long id)
        {
            Func<GenericResponse<AssetStruct>> getAssetStructFunc = () =>
               CatalogManager.Instance.GetAssetStruct(groupId, id);

            KalturaAssetStruct response =
                ClientUtils.GetResponseFromWS<KalturaAssetStruct, AssetStruct>(getAssetStructFunc);

            return response;
        }

        public KalturaMetaListResponse GetMetas(int groupId, List<long> ids, KalturaMetaDataType? type, KalturaMetaOrderBy? orderBy,
                                                bool? multipleValue = null, long? assetStructId = null)
        {
            KalturaMetaListResponse result = new KalturaMetaListResponse() { TotalCount = 0 };

            Func<GenericListResponse<Topic>> getTopicListFunc = delegate ()
            {
                GenericListResponse<Topic> topicList = null;
                MetaType metaType = MetaType.All;
                if (type.HasValue)
                {
                    metaType = CatalogMappings.ConvertToMetaType(type, multipleValue);
                }

                if (assetStructId.HasValue)
                {
                    topicList = TopicManager.Instance.GetTopicsByAssetStructId(groupId, assetStructId.Value, metaType);
                }
                else
                {
                    topicList = TopicManager.Instance.GetTopicsByIds(groupId, ids, metaType);
                }

                return topicList;
            };

            KalturaGenericListResponse<KalturaMeta> response =
                ClientUtils.GetResponseListFromWS<KalturaMeta, Topic>(getTopicListFunc);

            result.Metas = response.Objects;
            result.TotalCount = response.TotalCount;

            return result;

            if (result.TotalCount > 0 && orderBy.HasValue)
            {
                switch (orderBy.Value)
                {
                    case KalturaMetaOrderBy.NAME_ASC:
                        result.Metas = result.Metas.OrderBy(x => x.Name.ToString()).ToList();
                        break;
                    case KalturaMetaOrderBy.NAME_DESC:
                        result.Metas = result.Metas.OrderByDescending(x => x.Name.ToString()).ToList();
                        break;
                    case KalturaMetaOrderBy.SYSTEM_NAME_ASC:
                        result.Metas = result.Metas.OrderBy(x => x.SystemName).ToList();
                        break;
                    case KalturaMetaOrderBy.SYSTEM_NAME_DESC:
                        result.Metas = result.Metas.OrderByDescending(x => x.SystemName).ToList();
                        break;
                    case KalturaMetaOrderBy.CREATE_DATE_ASC:
                        result.Metas = result.Metas.OrderBy(x => x.CreateDate).ToList();
                        break;
                    case KalturaMetaOrderBy.CREATE_DATE_DESC:
                        result.Metas = result.Metas.OrderByDescending(x => x.CreateDate).ToList();
                        break;
                    case KalturaMetaOrderBy.UPDATE_DATE_ASC:
                        result.Metas = result.Metas.OrderBy(x => x.UpdateDate).ToList();
                        break;
                    case KalturaMetaOrderBy.UPDATE_DATE_DESC:
                        result.Metas = result.Metas.OrderByDescending(x => x.UpdateDate).ToList();
                        break;
                    default:
                        break;
                }
            }

            return result;
        }

        public KalturaMeta AddMeta(int groupId, KalturaMeta meta, long userId)
        {
            Func<Topic, GenericResponse<Topic>> addTopicFunc = (Topic topicToAdd) =>
                TopicManager.Instance.AddTopic(groupId, topicToAdd, userId);

            KalturaMeta result =
                ClientUtils.GetResponseFromWS<KalturaMeta, Topic>(meta, addTopicFunc);

            return result;
        }

        public KalturaMeta UpdateMeta(int groupId, long id, KalturaMeta meta, long userId)
        {
            Func<Topic, GenericResponse<Topic>> updateTopicFunc = (Topic topicToUpdate) =>
                TopicManager.Instance.UpdateTopic(groupId, id, topicToUpdate, userId);

            KalturaMeta result =
                ClientUtils.GetResponseFromWS<KalturaMeta, Topic>(meta, updateTopicFunc);

            return result;
        }

        public bool DeleteMeta(int groupId, long id, long userId)
        {
            Func<Status> deleteTopicFunc = () => TopicManager.Instance.DeleteTopic(groupId, id, userId);
            return ClientUtils.GetResponseStatusFromWS(deleteTopicFunc);
        }

        public KalturaAsset AddAsset(int groupId, KalturaAsset asset, long userId)
        {
            Func<Asset, GenericResponse<Asset>> addAssetFunc = (Asset assetToAdd) =>
                AssetManager.Instance.AddAsset(groupId, assetToAdd, userId);

            KalturaAsset result =
                ClientUtils.GetResponseFromWS<KalturaAsset, Asset>(asset, addAssetFunc);

            return result;
        }

        public bool DeleteAsset(int groupId, long id, KalturaAssetReferenceType assetReferenceType, long userId)
        {
            Func<Status> deleteAssetFunc = delegate ()
            {
                eAssetTypes assetType = CatalogMappings.ConvertToAssetTypes(assetReferenceType);
                return AssetManager.Instance.DeleteAsset(groupId, id, assetType, userId);
            };

            return ClientUtils.GetResponseStatusFromWS(deleteAssetFunc);
        }

        public KalturaAsset GetAsset(int groupId, long id, KalturaAssetReferenceType assetReferenceType, string siteGuid, int domainId, string udid,
            string language, bool isAllowedToViewInactiveAssets, bool ignoreEndDate = false)
        {
            KalturaAsset result = null;
            GenericResponse<Asset> response = null;
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    KalturaAssetListResponse assetListResponse = null;
                    bool opcAccount = Utils.Utils.DoesGroupUsesTemplates(groupId);
                    if (!isAllowedToViewInactiveAssets)
                    {
                        Version requestVersion = OldStandardAttribute.getCurrentRequestVersion();
                        isAllowedToViewInactiveAssets = requestVersion.CompareTo(opcMergeVersion) < 0;
                    }

                    if (isAllowedToViewInactiveAssets)
                    {
                        if (opcAccount)
                        {
                            eAssetTypes assetType = CatalogMappings.ConvertToAssetTypes(assetReferenceType);
                            response = AssetManager.Instance.GetAsset(groupId, id, assetType, isAllowedToViewInactiveAssets);
                        }
                        else
                        {
                            assetListResponse = GetMediaByIdForOperator(groupId, language, id);
                        }
                    }
                    else
                    {
                        var useFinal = false;
                        if (!opcAccount)
                        {
                            var groupManager = new GroupManager();
                            var group = groupManager.GetGroup(groupId);

                            useFinal = !group.isGeoAvailabilityWindowingEnabled;
                        }

                        var searchAssetsFilter = new SearchAssetsFilter
                        {
                            GroupId = groupId,
                            SiteGuid = siteGuid,
                            DomainId = domainId,
                            Udid = udid,
                            Language = language,
                            PageIndex = 0,
                            PageSize = 1,
                            Filter = $"(and asset_type='media' media_id = '{id}')",
                            AssetTypes = null,
                            EpgChannelIds = null,
                            ManagementData = false,
                            GroupBy = null,
                            IsAllowedToViewInactiveAssets = false,
                            IgnoreEndDate = true,
                            GroupByType = GroupingOption.Omit,
                            IsPersonalListSearch = false,
                            UseFinal = useFinal,
                            OrderingParameters = KalturaOrderAdapter.Instance.MapToOrderingList(KalturaAssetOrderBy.RELEVANCY_DESC)
                        };

                        assetListResponse = SearchAssets(searchAssetsFilter);
                    }

                    if (assetListResponse != null && assetListResponse.TotalCount == 1 && assetListResponse.Objects.Count == 1)
                    {
                        return assetListResponse.Objects[0];
                    }
                    else if (response == null)
                    {
                        throw new NotFoundException(NotFoundException.OBJECT_NOT_FOUND, "Asset");
                    }
                }
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status);
            }

            result = Mapper.Map<KalturaAsset>(response.Object);
            result.Images = CatalogMappings.ConvertImageListToKalturaMediaImageList(response.Object.Images, groupId);

            return result;
        }

        public KalturaAsset UpdateAsset(int groupId, long id, KalturaAsset asset, long userId)
        {
            Func<Asset, GenericResponse<Asset>> updateAssetFunc = (Asset assetToUpdate) =>
                AssetManager.Instance.UpdateAsset(groupId, id, assetToUpdate, userId);

            KalturaAsset result =
                ClientUtils.GetResponseFromWS<KalturaAsset, Asset>(asset, updateAssetFunc);

            return result;
        }

        public KalturaProgramAsset GetEpgAsset(int groupId, long epgId, bool isAllowedToViewInactiveAssets)
        {
            GenericResponse<Asset> response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = AssetManager.Instance.GetAsset(groupId, epgId, eAssetTypes.EPG, isAllowedToViewInactiveAssets);
                }
            }
            catch (Exception ex)
            {
                log.Error("Exception received while calling catalog service.", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status);
            }

            KalturaProgramAsset result = null;
            if (response.Object != null)
            {
                result = Mapper.Map<KalturaProgramAsset>(response.Object);
                result.Images = CatalogMappings.ConvertImageListToKalturaMediaImageList(response.Object.Images, groupId);
            }

            return result;
        }

        public bool RemoveTopicsFromAsset(int groupId, long id, KalturaAssetReferenceType assetReferenceType, HashSet<long> topicIds, long userId)
        {
            Func<Status> removeTopicsFromAssetFunc = delegate ()
            {
                eAssetTypes assetType = CatalogMappings.ConvertToAssetTypes(assetReferenceType);
                return AssetManager.RemoveTopicsFromAsset(groupId, id, assetType, topicIds, userId);
            };

            return ClientUtils.GetResponseStatusFromWS(removeTopicsFromAssetFunc);
        }

        public KalturaAssetListResponse GetAssetsForOPCAccount(int groupId, long domainId, List<BaseObject> assetsBaseDataList, bool isAllowedToViewInactiveAssets,
            KalturaBaseResponseProfile responseProfile = null,
            IReadOnlyDictionary<double, SearchPriorityGroup> priorityGroupsMapping = null)
        {
            KalturaAssetListResponse result = new KalturaAssetListResponse();
            if (assetsBaseDataList != null && assetsBaseDataList.Count > 0)
            {
                Version requestVersion = OldStandardAttribute.getCurrentRequestVersion();
                bool shouldReturnOldMediaObj = requestVersion.CompareTo(opcMergeVersion) < 0;
                GenericListResponse<AssetPriority> assetListResponse = AssetManager.GetOrderedAssets(groupId, assetsBaseDataList, isAllowedToViewInactiveAssets, priorityGroupsMapping);
                if (assetListResponse != null && assetListResponse.Status != null && assetListResponse.Status.Code == (int)eResponseStatus.OK)
                {
                    result.Objects = new List<KalturaAsset>();
                    var assets = new List<KalturaAssetPriority>();
                    // convert assets
                    foreach (AssetPriority assetToConvert in assetListResponse.Objects)
                    {
                        KalturaAsset asset = null;
                        if (assetToConvert.Asset.AssetType == eAssetTypes.MEDIA && shouldReturnOldMediaObj)
                        {
                            MediaObj oldMediaObj = new MediaObj(groupId, assetToConvert.Asset as MediaAsset);
                            asset = Mapper.Map<KalturaMediaAsset>(oldMediaObj);
                        }
                        else
                        {
                            if (assetToConvert.Asset.AssetType == eAssetTypes.NPVR)
                            {
                                long.TryParse((assetToConvert.Asset as RecordingAsset).RecordingId, out var domainRecordingId);
                                (assetToConvert.Asset as RecordingAsset).ViewableUntilDate = Core.Recordings.RecordingsManager
                                    .Instance.GetRecordingViewableUntilDate(groupId, domainId, domainRecordingId);
                            }

                            asset = Mapper.Map<KalturaAsset>(assetToConvert.Asset);
                            asset.Images = CatalogMappings.ConvertImageListToKalturaMediaImageList(assetToConvert.Asset.Images, groupId);
                        }

                        assets.Add(new KalturaAssetPriority(asset, assetToConvert.PriorityGroupId));
                    }

                    result.Objects = assets.Select(x => x.Asset).ToList();

                    SearchPriorityProfileProcessor.ProcessSearchPriorityResponseProfile(responseProfile, assets);
                }
                else if (assetListResponse != null && assetListResponse.Status != null)
                {
                    throw new ClientException(assetListResponse.Status);
                }
            }

            return result;
        }

        public KalturaAssetListResponse GetAssetFromUnifiedSearchResponse(int groupId, UnifiedSearchResponse searchResponse, BaseRequest request, bool isAllowedToViewInactiveAssets,
                                                                            bool managementData = false, KalturaBaseResponseProfile responseProfile = null, bool isPersonalListSearch = false,
                                                                            IReadOnlyDictionary<double, SearchPriorityGroup> priorityGroupsMapping = null)
        {
            KalturaAssetListResponse result = new KalturaAssetListResponse();
            bool doesGroupUsesTemplates = Utils.Utils.DoesGroupUsesTemplates(groupId);
            // check if aggregation result have values
            if (searchResponse.aggregationResults != null && searchResponse.aggregationResults.Count > 0 &&
                searchResponse.aggregationResults[0].results != null && responseProfile != null)
            {

                var assetsBaseDataList = new List<BaseObject>();
                foreach (AggregationResult aggregationResult in searchResponse.aggregationResults[0].results)
                {
                    if (aggregationResult.topHits != null && aggregationResult.topHits.Count > 0)
                    {
                        if (aggregationResult.value == ESUnifiedQueryBuilder.MissedHitBucketKeyString)
                        {
                            //take all hits from 'missing' bucket
                            assetsBaseDataList.AddRange(aggregationResult.topHits);
                        }
                        else
                        {
                            assetsBaseDataList.Add(aggregationResult.topHits[0]);
                        }
                    }
                }

                assetsBaseDataList = assetsBaseDataList
                    .Skip(request.m_nPageIndex * request.m_nPageSize)
                    .Take(request.m_nPageSize)
                    .ToList();

                var aggregationResults = searchResponse.aggregationResults[0].results;
                if (doesGroupUsesTemplates)
                {
                    result = GetAssetsForOPCAccount(groupId, request.domainId, assetsBaseDataList, isAllowedToViewInactiveAssets, responseProfile, priorityGroupsMapping);

                    CatalogUtils.SetTopHitCount(responseProfile, aggregationResults, result.Objects);
                }
                else
                {
                    // build the assetsBaseDataList from the hit array
                    result.Objects = CatalogUtils.GetAssets(aggregationResults, assetsBaseDataList, request, managementData, responseProfile);
                }

                result.TotalCount = searchResponse.aggregationResults[0].totalItems;
            }
            else
            {
                if (searchResponse.searchResults != null && searchResponse.searchResults.Count > 0)
                {
                    List<BaseObject> assetsBaseDataList = searchResponse.searchResults.Select(x => x as BaseObject).ToList();
                    if (doesGroupUsesTemplates)
                    {
                        result = GetAssetsForOPCAccount(groupId, request.domainId, assetsBaseDataList, isAllowedToViewInactiveAssets, responseProfile, priorityGroupsMapping);
                    }
                    else
                    {
                        // get base objects list
                        result.Objects = CatalogUtils.GetAssets(assetsBaseDataList, request, managementData);
                    }
                }

                result.TotalCount = searchResponse.m_nTotalItems;
            }

            return result;
        }

        public KalturaAssetStructMeta UpdateAssetStructMeta(long assetStructId, long MetaId, KalturaAssetStructMeta assetStructMeta, int groupId, long userId)
        {
            Func<AssetStructMeta, GenericResponse<AssetStructMeta>> updateAssetStructMetaFunc = (AssetStructMeta assetStructMetaToUpdate) =>
                CatalogManager.Instance.UpdateAssetStructMeta
                        (assetStructId, MetaId, assetStructMetaToUpdate, groupId, userId);

            KalturaAssetStructMeta result =
                ClientUtils.GetResponseFromWS(assetStructMeta, updateAssetStructMetaFunc);

            return result;
        }

        public KalturaAssetStructMetaListResponse GetAssetStructMetaList(int groupId, long? assetStructId, long? metaId)
        {
            KalturaAssetStructMetaListResponse result = new KalturaAssetStructMetaListResponse() { TotalCount = 0 };

            Func<GenericListResponse<AssetStructMeta>> getAssetStructMetaListFunc = () =>
               CatalogManager.Instance.GetAssetStructMetaList(groupId, assetStructId, metaId);

            KalturaGenericListResponse<KalturaAssetStructMeta> response =
                ClientUtils.GetResponseListFromWS<KalturaAssetStructMeta, AssetStructMeta>(getAssetStructMetaListFunc);

            result.AssetStructMetas = response.Objects;
            result.TotalCount = response.TotalCount;

            return result;
        }

        #endregion

        private string GetSignature(string signString, string signatureKey)
        {
            string retVal;
            //Get key from DB
            string hmacSecret = signatureKey;
            // The HMAC secret as configured in the skin
            // Values are always transferred using UTF-8 encoding
            UTF8Encoding encoding = new UTF8Encoding();

            // Calculate the HMAC
            // signingString is the SignString from the request
            HMACSHA1 myhmacsha1 = new HMACSHA1(encoding.GetBytes(hmacSecret));
            retVal = Convert.ToBase64String(myhmacsha1.ComputeHash(encoding.GetBytes(signString)));
            myhmacsha1.Clear();
            return retVal;
        }

        private DateTime getServerTime()
        {
            return (DateTime)HttpContext.Current.Items[RequestContextConstants.REQUEST_TIME];
        }

        [Obsolete]
        public KalturaAssetInfoListResponse SearchAssets(int groupId, string siteGuid, int domainId, string udid, string language, int pageIndex, int? pageSize,
                                                            string filter, KalturaOrder? orderBy, List<int> assetTypes, string requestId,
                                                            List<KalturaCatalogWith> with, bool excludeWatched)

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
            Group group = GroupsManager.Instance.GetGroup(groupId);

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
            if (!CatalogUtils.GetBaseResponse<UnifiedSearchResponse>(request, out searchResponse, true, key.ToString()))
            {
                // general error
                throw new ClientException(StatusCode.Error);
            }

            if (searchResponse.status.Code != (int)StatusCode.OK)
            {
                // Bad response received from WS
                throw new ClientException(searchResponse.status);
            }

            if (searchResponse.searchResults != null && searchResponse.searchResults.Count > 0)
            {
                // get base objects list
                List<BaseObject> assetsBaseDataList = searchResponse.searchResults.Select(x => x as BaseObject).ToList();

                // get assets from catalog/cache
                List<KalturaIAssetable> assetsInfo = CatalogUtils.GetAssets(assetsBaseDataList, request, with, CatalogConvertor.ConvertBaseObjectsToAssetsInfo);

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

        public KalturaAssetListResponse SearchAssetsExcludeWatched(SearchAssetsFilter searchAssetFilter, bool managementData)
        {
            var userId = int.Parse(searchAssetFilter.SiteGuid);
            var filter = searchAssetFilter.Filter;
            var pageIndex = searchAssetFilter.PageIndex;
            var orderingParameters = KalturaOrderMapper.Instance.MapParameters(searchAssetFilter.OrderingParameters);

            // get group configuration
            var group = GroupsManager.Instance.GetGroup(searchAssetFilter.GroupId);

            if (searchAssetFilter.EpgChannelIds?.Count > 0)
            {
                var strEpgChannelIds = string.Join(",", searchAssetFilter.EpgChannelIds.Select(at => at.ToString()).ToArray());
                filter = $"(and {filter} epg_channel_id:'{strEpgChannelIds}')";
            }

            UnifiedSearchRequest request = null;
            List<BaseObject> totalAssetsBaseDataList = new List<BaseObject>();
            int totalCount = 0;
            List<long> userWatchedMediaIds = new List<long>();

            while (pageIndex < 10)
            {
                var searchResponse = CatalogUtils.SearchAssets(
                    searchAssetFilter.GroupId,
                    userId,
                    searchAssetFilter.DomainId,
                    searchAssetFilter.Udid,
                    searchAssetFilter.Language,
                    pageIndex,
                    searchAssetFilter.PageSize,
                    filter,
                    searchAssetFilter.AssetTypes,
                    getServerTime(),
                    orderingParameters,
                    group,
                    Signature,
                    SignString,
                    ref request,
                    searchAssetFilter.GroupByType,
                    searchAssetFilter.ShouldApplyPriorityGroups,
                    searchAssetFilter.OriginalUserId);

                if (searchResponse.searchResults != null && searchResponse.searchResults.Count > 0)
                {
                    // get base objects list
                    var assetsBaseDataList = searchResponse.searchResults.Select(x => x as BaseObject).ToList();
                    var totalResults = assetsBaseDataList.Count;

                    // Get user user's watched media and exclude them from searchResults
                    if (pageIndex == 0)
                    {
                        userWatchedMediaIds = CatalogUtils.GetUserWatchedMediaIds(searchAssetFilter.GroupId, userId);
                    }

                    // filter out watched media
                    if (userWatchedMediaIds != null && userWatchedMediaIds.Count > 0)
                    {
                        var excludedMediaIds = assetsBaseDataList.Where(x => userWatchedMediaIds.Select(y => y.ToString()).ToList().Contains(x.AssetId) && x.AssetType != eAssetTypes.EPG && x.AssetType != eAssetTypes.NPVR);
                        if (excludedMediaIds != null)
                        {
                            var res = assetsBaseDataList.Except(excludedMediaIds);
                            if (res != null)
                            {
                                assetsBaseDataList = res.ToList();
                            }
                        }
                    }

                    if (totalCount + assetsBaseDataList.Count > searchAssetFilter.PageSize)
                    {
                        totalAssetsBaseDataList.AddRange(assetsBaseDataList.Take(assetsBaseDataList.Count - totalCount));
                        totalCount = searchAssetFilter.PageSize.Value;
                    }
                    else
                    {
                        totalAssetsBaseDataList.AddRange(assetsBaseDataList);
                        totalCount += assetsBaseDataList.Count;
                    }

                    if (totalResults < searchAssetFilter.PageSize || totalCount == searchAssetFilter.PageSize)
                    {
                        break;
                    }

                    pageIndex++;
                }
                else
                {
                    break;
                }
            }

            var modifiedSearchResponse = new UnifiedSearchResponse
            {
                searchResults = totalAssetsBaseDataList.Select(x => x as UnifiedSearchResult).ToList(),
                m_nTotalItems = totalCount
            };

            return GetAssetFromUnifiedSearchResponse(
                searchAssetFilter.GroupId,
                modifiedSearchResponse,
                request,
                false,
                managementData,
                searchAssetFilter.ResponseProfile,
                false,
                request.PriorityGroupsMappings);
        }

        public KalturaAssetListResponse SearchAssets(SearchAssetsFilter searchAssetsFilter)
        {
            // get group configuration
            var group = GroupsManager.Instance.GetGroup(searchAssetsFilter.GroupId);
            if (searchAssetsFilter.EpgChannelIds != null && searchAssetsFilter.EpgChannelIds.Count > 0)
            {
                var strEpgChannelIds = string.Join(",", searchAssetsFilter.EpgChannelIds.Select(at => at.ToString()).ToArray());
                searchAssetsFilter.Filter = $"(and {searchAssetsFilter.Filter} epg_channel_id:'{strEpgChannelIds}')";
            }

            var aggregationOrder = CatalogConvertor.ConvertToAggregationOrder(searchAssetsFilter.GroupByOrder);

            // build request
            var request = new UnifiedSearchRequest
            {
                m_sSignature = Signature,
                m_sSignString = SignString,
                m_oFilter = new Filter()
                {
                    m_sDeviceId = searchAssetsFilter.Udid,
                    m_nLanguage = Utils.Utils.GetLanguageId(searchAssetsFilter.GroupId, searchAssetsFilter.Language),
                    m_bUseStartDate = group.UseStartDate,
                    m_bOnlyActiveMedia = group.GetOnlyActiveAssets,
                    m_bUseFinalDate = searchAssetsFilter.UseFinal
                },
                m_sUserIP = Utils.Utils.GetClientIP(),
                m_nGroupID = searchAssetsFilter.GroupId,
                m_nPageIndex = searchAssetsFilter.PageIndex,
                m_nPageSize = searchAssetsFilter.PageSize.Value,
                filterQuery = searchAssetsFilter.Filter,
                m_dServerTime = getServerTime(),
                assetTypes = searchAssetsFilter.AssetTypes,
                m_sSiteGuid = searchAssetsFilter.SiteGuid,
                domainId = searchAssetsFilter.DomainId,
                isAllowedToViewInactiveAssets = searchAssetsFilter.IsAllowedToViewInactiveAssets,
                shouldIgnoreEndDate = searchAssetsFilter.IgnoreEndDate && !searchAssetsFilter.UseFinal,
                GroupByOption = searchAssetsFilter.GroupByType,
                orderingParameters = KalturaOrderMapper.Instance.MapParameters(searchAssetsFilter.OrderingParameters),
                specificAssets = searchAssetsFilter.SpecificAssets,
                OriginalUserId = searchAssetsFilter.OriginalUserId
            };

            // for testing purposes
            if (searchAssetsFilter.ShouldApplyPriorityGroups)
            {
                request.PriorityGroupsMappings = _searchPriorityGroupManager.ListSearchPriorityGroupMappings(searchAssetsFilter.GroupId);
            }

            if (searchAssetsFilter.GroupBy != null && searchAssetsFilter.GroupBy.Count > 0)
            {
                request.searchGroupBy = new SearchAggregationGroupBy()
                {
                    groupBy = searchAssetsFilter.GroupBy,
                    distinctGroup = searchAssetsFilter.GroupBy[0], // maybe will send string.empty - and Backend will fill it if necessary
                    topHitsCount = 1,
                    groupByOrder = aggregationOrder
                };
            }

            // fire unified search request
            if (!CatalogUtils.GetBaseResponse(request, out UnifiedSearchResponse searchResponse))
            {
                // general error
                throw new ClientException(StatusCode.Error);
            }

            if (searchResponse.status.Code != (int)StatusCode.OK)
            {
                // Bad response received from WS
                throw new ClientException(searchResponse.status);
            }

            return GetAssetFromUnifiedSearchResponse(
                searchAssetsFilter.GroupId,
                searchResponse, request,
                searchAssetsFilter.IsAllowedToViewInactiveAssets,
                searchAssetsFilter.ManagementData,
                searchAssetsFilter.ResponseProfile,
                searchAssetsFilter.IsPersonalListSearch,
                request.PriorityGroupsMappings);
        }

        internal KalturaAssetCount GetAssetCount(SearchAssetsFilter searchAssetFilter)
        {
            KalturaAssetCount result = new KalturaAssetCount();

            // get group configuration
            Group group = GroupsManager.Instance.GetGroup(searchAssetFilter.GroupId);
            var filter = searchAssetFilter.Filter;

            if (searchAssetFilter.EpgChannelIds?.Count > 0)
            {
                var strEpgChannelIds = string.Join(",", searchAssetFilter.EpgChannelIds.Select(at => at.ToString()).ToArray());
                filter += $" epg_channel_id:'{strEpgChannelIds}'";
            }

            if (searchAssetFilter.GroupBy == null)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "groupBy");
            }

            var aggregationOrder = CatalogConvertor.ConvertToAggregationOrder(
                    searchAssetFilter.GroupByOrder) ?? AggregationOrder.Value_Asc;

            // build request
            var request = new UnifiedSearchRequest
            {
                m_sSignature = Signature,
                m_sSignString = SignString,
                m_oFilter = new Filter
                {
                    m_sDeviceId = searchAssetFilter.Udid,
                    m_nLanguage = Utils.Utils.GetLanguageId(searchAssetFilter.GroupId, searchAssetFilter.Language),
                    m_bUseStartDate = group.UseStartDate,
                    m_bOnlyActiveMedia = group.GetOnlyActiveAssets
                },
                m_sUserIP = Utils.Utils.GetClientIP(),
                m_nGroupID = searchAssetFilter.GroupId,
                m_nPageIndex = 0,
                m_nPageSize = 0,
                filterQuery = filter,
                m_dServerTime = getServerTime(),
                assetTypes = searchAssetFilter.AssetTypes,
                m_sSiteGuid = searchAssetFilter.SiteGuid,
                domainId = searchAssetFilter.DomainId,
                searchGroupBy = new SearchAggregationGroupBy()
                {
                    groupBy = searchAssetFilter.GroupBy,
                    groupByOrder = aggregationOrder
                },
                isAllowedToViewInactiveAssets = searchAssetFilter.IsAllowedToViewInactiveAssets,
                GroupByOption = searchAssetFilter.GroupByType,
                orderingParameters = KalturaOrderMapper.Instance.MapParameters(searchAssetFilter.OrderingParameters),
                specificAssets = searchAssetFilter.SpecificAssets,
                OriginalUserId = searchAssetFilter.OriginalUserId
            };

            // fire unified search request
            if (!CatalogUtils.GetBaseResponse(request, out UnifiedSearchResponse searchResponse))
            {
                // general error
                throw new ClientException(StatusCode.Error);
            }

            if (searchResponse.status.Code != (int)StatusCode.OK)
            {
                // Bad response received from WS
                throw new ClientException(searchResponse.status);
            }

            if (searchResponse.aggregationResults != null && searchResponse.aggregationResults.Count > 0)
            {
                // map counts
                result.SubCounts = Mapper.Map<List<KalturaAssetsCount>>(searchResponse.aggregationResults);
                result.Count = searchResponse.m_nTotalItems;
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
            Group group = GroupsManager.Instance.GetGroup(groupId);

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
            if (!CatalogUtils.GetBaseResponse<UnifiedSearchResponse>(request, out searchResponse, true, key.ToString()))
            {
                // general error
                throw new ClientException(StatusCode.Error);
            }

            if (searchResponse.status.Code != (int)StatusCode.OK)
            {
                // Bad response received from WS
                throw new ClientException(searchResponse.status);
            }

            if (searchResponse.searchResults != null && searchResponse.searchResults.Count > 0)
            {
                // get base objects list
                List<BaseObject> assetsBaseDataList = searchResponse.searchResults.Select(x => x as BaseObject).ToList();

                // get assets from catalog/cache
                List<KalturaIAssetable> assetsInfo = CatalogUtils.GetAssets(assetsBaseDataList, request, with, CatalogConvertor.ConvertBaseObjectsToSlimAssetsInfo);

                // build AssetInfoWrapper response
                if (assetsInfo != null)
                {
                    result.Objects = assetsInfo.Select(a => (KalturaBaseAssetInfo)a).ToList();
                }

                result.TotalCount = searchResponse.m_nTotalItems;
            }

            return result;
        }

        public KalturaAssetHistoryListResponse getAssetHistory(int groupId, string siteGuid, string udid, string language, int pageIndex, int? pageSize,
            KalturaWatchStatus watchStatus, int days, List<int> assetTypes, List<string> assetIds, bool suppress, string ksql, List<KalturaCatalogWith> withList = null)
        {
            KalturaAssetHistoryListResponse finalResults = new KalturaAssetHistoryListResponse();
            finalResults.Objects = new List<KalturaAssetHistory>();

            // get group configuration
            Group group = GroupsManager.Instance.GetGroup(groupId);

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
                OrderDir = OrderDir.DESC,
                AssetIds = assetIds,
                Suppress = suppress,
                FilterQuery = ksql
            };

            // fire history watched request
            WatchHistoryResponse watchHistoryResponse = new WatchHistoryResponse();
            if (!CatalogUtils.GetBaseResponse<WatchHistoryResponse>(request, out watchHistoryResponse))
            {
                // general error
                throw new ClientException(StatusCode.Error);
            }

            if (watchHistoryResponse.status.Code != (int)StatusCode.OK)
            {
                // Bad response received from WS
                throw new ClientException(watchHistoryResponse.status);
            }

            if (watchHistoryResponse.result != null && watchHistoryResponse.result.Count > 0)
            {
                // combine asset info and watch history info
                finalResults.TotalCount = watchHistoryResponse.m_nTotalItems;
                var assetHistories = Mapper.Map<List<KalturaAssetHistory>>(watchHistoryResponse.result.Where(x => x != null));
                finalResults.Objects.AddRange(assetHistories);
            }

            return finalResults;
        }

        [Obsolete]
        public KalturaWatchHistoryAssetWrapper WatchHistory(int groupId, string siteGuid, string udid, string language, int pageIndex, int? pageSize, KalturaWatchStatus watchStatus, int days, List<int> assetTypes, List<string> assetIds, List<KalturaCatalogWith> withList)
        {
            KalturaWatchHistoryAssetWrapper finalResults = new KalturaWatchHistoryAssetWrapper();

            // get group configuration
            Group group = GroupsManager.Instance.GetGroup(groupId);

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
            if (!CatalogUtils.GetBaseResponse<WatchHistoryResponse>(request, out watchHistoryResponse))
            {
                // general error
                throw new ClientException(StatusCode.Error);
            }

            if (watchHistoryResponse.status.Code != (int)StatusCode.OK)
            {
                // Bad response received from WS
                throw new ClientException(watchHistoryResponse.status);
            }

            if (watchHistoryResponse.result != null && watchHistoryResponse.result.Count > 0)
            {
                // get base objects list
                List<BaseObject> assetsBaseDataList = watchHistoryResponse.result.Select(x => x as BaseObject).ToList();

                // get assets from catalog/cache
                List<KalturaIAssetable> assetsInfo = CatalogUtils.GetAssets(assetsBaseDataList, request, withList, CatalogConvertor.ConvertBaseObjectsToAssetsInfo);

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
                m_dStartDate = startTime != 0 ? DateUtils.UtcUnixTimestampSecondsToDateTime(startTime) : DateTime.MinValue,
                m_dEndDate = endTime != 0 ? DateUtils.UtcUnixTimestampSecondsToDateTime(endTime) : DateTime.MaxValue,
                m_type = Mapper.Map<StatsType>(assetType)
            };

            AssetStatsResponse response = null;
            if (CatalogUtils.GetBaseResponse(request, out response))
            {
                result = response.m_lAssetStat != null ?
                    Mapper.Map<List<KalturaAssetStatistics>>(response.m_lAssetStat) : null;
            }
            else
            {
                throw new ClientException(StatusCode.Error);
            }

            return result;
        }

        [Obsolete]
        public KalturaAssetInfoListResponse GetRelatedMedia(int groupId, string siteGuid, int domainId, string udid, string language,
            int pageIndex, int? pageSize, int mediaId, string filter, List<int> mediaTypes, List<KalturaCatalogWith> with)
        {
            KalturaAssetInfoListResponse result = new KalturaAssetInfoListResponse();

            // get group configuration
            Group group = GroupsManager.Instance.GetGroup(groupId);

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

            result = CatalogUtils.GetMedia(request, key.ToString(), with);

            return result;
        }

        public KalturaAssetListResponse GetRelatedMedia(
            ContextData contextData,
            int pageIndex,
            int? pageSize,
            int mediaId,
            string filter,
            List<int> mediaTypes,
            IEnumerable<KalturaBaseAssetOrder> orderings,
            List<string> groupBy = null,
            KalturaBaseResponseProfile responseProfile = null,
            bool shouldApplyPriorityGroups = false)
        {
            // get group configuration
            Group group = GroupsManager.Instance.GetGroup(contextData.GroupId);

            // build request
            var request = new MediaRelatedRequest
            {
                m_sSignature = Signature,
                m_sSignString = SignString,
                m_oFilter = new Filter()
                {
                    m_sDeviceId = contextData.Udid,
                    m_nLanguage = Utils.Utils.GetLanguageId(contextData.GroupId, contextData.Language),
                    m_bUseStartDate = group.UseStartDate,
                    m_bOnlyActiveMedia = group.GetOnlyActiveAssets
                },
                m_sUserIP = Utils.Utils.GetClientIP(),
                m_nGroupID = contextData.GroupId,
                m_nPageIndex = pageIndex,
                m_nPageSize = pageSize.Value,
                m_nMediaID = mediaId,
                m_nMediaTypes = mediaTypes,
                m_sSiteGuid = contextData.UserId.ToString(),
                domainId = (int)(contextData.DomainId ?? 0),
                m_sFilter = filter,
                OrderingParameters = KalturaOrderMapper.Instance.MapParameters(orderings),
                OriginalUserId = contextData.OriginalUserId
            };
            if (shouldApplyPriorityGroups)
            {
                request.PriorityGroupsMappings = _searchPriorityGroupManager.ListSearchPriorityGroupMappings(contextData.GroupId);
            }

            if (groupBy != null && groupBy.Count > 0)
            {
                request.searchGroupBy = new SearchAggregationGroupBy()
                {
                    groupBy = groupBy,
                    distinctGroup = groupBy[0], // mabye will send string.empty - and Backend will fill it if nessecery
                    topHitsCount = 1
                };
            }

            return CatalogUtils.GetMedia(request, false, responseProfile);
        }

        public KalturaAssetListResponse GetRelatedMediaExcludeWatched(
            ContextData contextData,
            int pageIndex,
            int? pageSize,
            int mediaId,
            string filter,
            List<int> mediaTypes,
            IReadOnlyCollection<KalturaBaseAssetOrder> orderings,
            KalturaBaseResponseProfile responseProfile = null,
            bool shouldApplyPriorityGroups = false)
        {
            // get group configuration
            Group group = GroupsManager.Instance.GetGroup(contextData.GroupId);

            MediaRelatedRequest request = null;
            List<BaseObject> totalAssetsBaseDataList = new List<BaseObject>();
            int totalCount = 0;
            List<long> userWatchedMediaIds = new List<long>();
            var orderingParameters = KalturaOrderMapper.Instance.MapParameters(orderings);

            while (pageIndex < 10)
            {
                var searchResponse = CatalogUtils.GetMediaExcludeWatched(
                    contextData,
                    pageIndex,
                    pageSize,
                    mediaId,
                    filter,
                    mediaTypes,
                    orderingParameters,
                    group,
                    Signature,
                    SignString,
                    ref request,
                    shouldApplyPriorityGroups);

                if (searchResponse.searchResults != null && searchResponse.searchResults.Count > 0)
                {
                    // get base objects list
                    var assetsBaseDataList = searchResponse.searchResults.Select(x => x as BaseObject).ToList();
                    int totalResults = assetsBaseDataList.Count;

                    // Get user user's watched media and exclude them from searchResults
                    if (pageIndex == 0)
                    {
                        userWatchedMediaIds = CatalogUtils.GetUserWatchedMediaIds(contextData.GroupId, (int)contextData.UserId);
                    }
                    // filter out watched media
                    if (userWatchedMediaIds != null && userWatchedMediaIds.Count > 0)
                    {
                        var excludedMediaIds = assetsBaseDataList.Where(x => userWatchedMediaIds.Select(y => y.ToString()).ToList().Contains(x.AssetId) && x.AssetType != eAssetTypes.EPG && x.AssetType != eAssetTypes.NPVR);
                        if (excludedMediaIds != null)
                        {
                            var res = assetsBaseDataList.Except(excludedMediaIds);
                            if (res != null)
                            {
                                assetsBaseDataList = res.ToList();
                            }
                        }
                    }

                    if (totalCount + assetsBaseDataList.Count > pageSize)
                    {
                        totalAssetsBaseDataList.AddRange(assetsBaseDataList.Take(assetsBaseDataList.Count - totalCount));
                        totalCount = pageSize.Value;
                    }
                    else
                    {
                        totalAssetsBaseDataList.AddRange(assetsBaseDataList);
                        totalCount += assetsBaseDataList.Count;
                    }

                    if (totalResults < pageSize || totalCount == pageSize)
                    {
                        break;
                    }

                    pageIndex++;
                }
                else
                {
                    break;
                }
            }

            var mediaIdsResponse = new UnifiedSearchResponse
            {
                searchResults = totalAssetsBaseDataList.Select(x => x as UnifiedSearchResult).ToList(),
                m_nTotalItems = totalCount
            };

            return ClientsManager.CatalogClient().GetAssetFromUnifiedSearchResponse(
                    request?.m_nGroupID ?? contextData.GroupId,
                    mediaIdsResponse,
                    request,
                    false,
                    responseProfile: responseProfile,
                    priorityGroupsMapping: request?.PriorityGroupsMappings);
        }

        public KalturaAssetInfoListResponse GetRelatedMediaExternal(int groupId, string siteGuid, int domainId, string udid, string language, int pageIndex, int? pageSize,
                                                                    int mediaId, List<int> mediaTypes, int utcOffset, List<KalturaCatalogWith> with, string freeParam)
        {
            KalturaAssetInfoListResponse result = new KalturaAssetInfoListResponse();

            // get group configuration
            Group group = GroupsManager.Instance.GetGroup(groupId);

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

            result = CatalogUtils.GetMediaWithStatus(request, key.ToString(), with);

            return result;
        }

        public KalturaAssetInfoListResponse GetSearchMediaExternal(int groupId, string siteGuid, int domainId, string udid, string language, int pageIndex, int? pageSize, string query, List<int> mediaTypes, int utcOffset, List<KalturaCatalogWith> with)
        {
            KalturaAssetInfoListResponse result = new KalturaAssetInfoListResponse();

            // get group configuration
            Group group = GroupsManager.Instance.GetGroup(groupId);

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
                m_nUtcOffset = utcOffset,
                m_sDeviceID = udid
            };

            // build failover cache key
            StringBuilder key = new StringBuilder();
            key.AppendFormat("search_q={0}_pi={1}_pz={2}_g={3}_l={4}_mt={5}",
                query, pageIndex, pageSize, groupId, language, mediaTypes != null ? string.Join(",", mediaTypes.ToArray()) : string.Empty);

            result = CatalogUtils.GetMediaWithStatus(request, key.ToString(), with);

            return result;
        }

        public KalturaAssetInfoListResponse GetChannelMedia(int groupId, string siteGuid, int domainId, string udid, string language, int pageIndex, int? pageSize,
            int channelId, KalturaOrder? orderBy, List<KalturaCatalogWith> with, List<KeyValue> filterTags,
            KalturaCutWith cutWith)
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
            Group group = GroupsManager.Instance.GetGroup(groupId);

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
            if (!CatalogUtils.GetBaseResponse<ChannelResponse>(request, out channelResponse, true, key.ToString()))
            {
                // general error
                throw new ClientException(StatusCode.Error);
            }

            if (channelResponse.m_nMedias != null && channelResponse.m_nMedias.Count > 0)
            {
                result.Objects = CatalogUtils.GetMediaByIds(channelResponse.m_nMedias, request, with);
                result.TotalCount = channelResponse.m_nTotalItems;
            }
            return result;
        }

        public KalturaAssetInfoListResponse GetChannelAssets(int groupId, string siteGuid, int domainId, string udid, string language,
            int pageIndex, int? pageSize, List<KalturaCatalogWith> with, int channelId, KalturaOrder? orderBy, string filterQuery, bool excludeWatched)
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
            Group group = GroupsManager.Instance.GetGroup(groupId);

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
            if (!CatalogUtils.GetBaseResponse<UnifiedSearchResponse>(request, out channelResponse, true, key.ToString()))
            {
                // general error
                throw new ClientException(StatusCode.Error);
            }

            if (channelResponse.status.Code != (int)StatusCode.OK)
            {
                // Bad response received from WS
                throw new ClientException(channelResponse.status);
            }

            if (channelResponse.searchResults != null && channelResponse.searchResults.Count > 0)
            {
                // get base objects list
                List<BaseObject> assetsBaseDataList = channelResponse.searchResults.Select(x => x as BaseObject).ToList();
                result.TotalCount = channelResponse.m_nTotalItems;

                // get assets from catalog/cache
                List<KalturaIAssetable> assetsInfo = CatalogUtils.GetAssets(assetsBaseDataList, request, with, CatalogConvertor.ConvertBaseObjectsToAssetsInfo);

                // build AssetInfoWrapper response
                if (assetsInfo != null)
                {
                    result.Objects = assetsInfo.Select(a => (KalturaAssetInfo)a).ToList();
                }

            }

            return result;
        }

        [Obsolete]
        public KalturaAssetInfoListResponse GetMediaByIds(int groupId, string siteGuid, string udid, string language, int pageIndex, int? pageSize, List<int> mediaIds, List<KalturaCatalogWith> with)
        {
            KalturaAssetInfoListResponse result = new KalturaAssetInfoListResponse();
            int domainId = (int)HouseholdUtils.GetHouseholdIDByKS();

            // get group configuration
            Group group = GroupsManager.Instance.GetGroup(groupId);

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
            if (!CatalogUtils.GetBaseResponse<MediaIdsResponse>(request, out mediaIdsResponse))
            {
                // general error
                throw new ClientException(StatusCode.Error);
            }

            if (mediaIdsResponse.m_nMediaIds != null && mediaIdsResponse.m_nMediaIds.Count > 0)
            {
                result.Objects = CatalogUtils.GetMediaByIds(mediaIdsResponse.m_nMediaIds, request, with);
                result.TotalCount = mediaIdsResponse.m_nTotalItems;
            }

            if (result == null || result.Objects == null || result.Objects.Count == 0)
            {
                throw new ClientException((int)StatusCode.NotFound, "asset not found");
            }

            return result;
        }

        public KalturaAssetListResponse GetMediaByIds(int groupId, string siteGuid, int domainId, string udid, string language, int pageIndex, int? pageSize, List<int> mediaIds, KalturaAssetOrderBy orderBy)
        {
            KalturaAssetListResponse result = new KalturaAssetListResponse();

            // get group configuration
            Group group = GroupsManager.Instance.GetGroup(groupId);

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
            if (!CatalogUtils.GetBaseResponse<MediaIdsResponse>(request, out mediaIdsResponse))
            {
                // general error
                throw new ClientException(StatusCode.Error);
            }

            if (mediaIdsResponse.m_nMediaIds != null && mediaIdsResponse.m_nMediaIds.Count > 0)
            {
                result.Objects = CatalogUtils.GetMediaByIds(mediaIdsResponse.m_nMediaIds, request);
                result.TotalCount = mediaIdsResponse.m_nTotalItems;
            }

            return result;
        }

        public KalturaAssetListResponse GetMediaByIdForOperator(int groupId, string language, long assetId)
        {
            log.Debug($"BEO-9511 GetMediaByIdForOperator");

            KalturaAssetListResponse result = new KalturaAssetListResponse();

            BaseObject asset = new BaseObject()
            {
                AssetId = assetId.ToString(),
                AssetType = eAssetTypes.MEDIA
            };

            MediasProtocolRequest request = new MediasProtocolRequest()
            {
                m_oFilter = new Filter()
                {
                    m_bUseStartDate = false,
                    m_bOnlyActiveMedia = false,
                    m_bUseFinalDate = false,
                    m_nLanguage = Utils.Utils.GetLanguageId(groupId, language),
                },
                m_nGroupID = groupId
            };

            var response = CatalogUtils.GetAssets(new List<BaseObject>() { asset }, request);

            if (response?.Count > 0)
            {
                result.Objects = response;
                result.TotalCount = result.Objects.Count;
            }

            return result;
        }

        [Obsolete]
        public KalturaAssetInfoListResponse GetEPGByInternalIds(int groupId, string siteGuid, int domainId, string udid, string language, int pageIndex, int? pageSize, List<int> epgIds,
            List<KalturaCatalogWith> with)
        {
            KalturaAssetInfoListResponse result = new KalturaAssetInfoListResponse();

            // get group configuration
            Group group = GroupsManager.Instance.GetGroup(groupId);

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

            if (CatalogUtils.GetBaseResponse(request, out epgProgramResponse) && epgProgramResponse != null)
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
                    throw new ClientException(StatusCode.Error);
                }
            }

            return result;
        }

        public KalturaAssetListResponse GetEPGByInternalIds(ContextData contextData, int pageIndex, int? pageSize, List<int> epgIds, KalturaAssetOrderBy orderBy)
        {
            KalturaAssetListResponse result = new KalturaAssetListResponse();

            // get group configuration
            Group group = GroupsManager.Instance.GetGroup(contextData.GroupId);

            // build request
            EpgProgramDetailsRequest request = new EpgProgramDetailsRequest()
            {
                m_sSignature = Signature,
                m_sSignString = SignString,
                m_oFilter = new Filter()
                {
                    m_sDeviceId = contextData.Udid,
                    m_nLanguage = Utils.Utils.GetLanguageId(contextData.GroupId, contextData.Language),
                    m_bUseStartDate = group.UseStartDate,
                    m_bOnlyActiveMedia = group.GetOnlyActiveAssets
                },
                m_sUserIP = Utils.Utils.GetClientIP(),
                m_nGroupID = contextData.GroupId,
                m_nPageIndex = pageIndex,
                m_nPageSize = pageSize.Value,
                m_sSiteGuid = contextData.UserId.ToString(),
                domainId = (int)(contextData.DomainId ?? 0),
                m_lProgramsIds = epgIds,
                OriginalUserId = contextData.OriginalUserId
            };

            EpgProgramResponse epgProgramResponse = null;

            if (CatalogUtils.GetBaseResponse(request, out epgProgramResponse) && epgProgramResponse != null)
            {
                result.Objects = Mapper.Map<List<KalturaAsset>>(epgProgramResponse.m_lObj);

                if (result.Objects != null)
                {
                    result.TotalCount = epgProgramResponse.m_nTotalItems;
                }
                else
                {
                    throw new ClientException(StatusCode.Error);
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
            Group group = GroupsManager.Instance.GetGroup(groupId);

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
                eLang = Language.English,
                duration = 0
            };

            EpgProgramsResponse epgProgramResponse = null;

            if (CatalogUtils.GetBaseResponse(request, out epgProgramResponse) && epgProgramResponse != null)
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
                    throw new ClientException(StatusCode.Error);
                }
            }

            return result;
        }

        public KalturaAssetListResponse GetEPGByExternalIds(ContextData contextData, int pageIndex, int? pageSize, List<string> epgIds)
        {
            KalturaAssetListResponse result = new KalturaAssetListResponse();

            // get group configuration
            Group group = GroupsManager.Instance.GetGroup(contextData.GroupId);

            // build request
            EPGProgramsByProgramsIdentefierRequest request = new EPGProgramsByProgramsIdentefierRequest()
            {
                m_sSignature = Signature,
                m_sSignString = SignString,
                m_oFilter = new Filter()
                {
                    m_sDeviceId = contextData.Udid,
                    m_nLanguage = Utils.Utils.GetLanguageId(contextData.GroupId, contextData.Language),
                },
                m_sUserIP = Utils.Utils.GetClientIP(),
                m_nGroupID = contextData.GroupId,
                m_nPageIndex = pageIndex,
                m_nPageSize = pageSize.Value,
                m_sSiteGuid = contextData.UserId.ToString(),
                domainId = (int)(contextData.DomainId ?? 0),
                pids = epgIds,
                eLang = Language.English,
                duration = 0,
                OriginalUserId = contextData.OriginalUserId
            };

            if (CatalogUtils.GetBaseResponse(request, out EpgProgramsResponse epgProgramResponse)
                && epgProgramResponse?.lEpgList != null)
            {
                // get base objects list
                var assetsBaseDataList = epgProgramResponse.lEpgList.Select(x => new BaseObject
                {
                    AssetId = x.EPG_ID.ToString(),
                    AssetType = eAssetTypes.EPG,
                    m_dUpdateDate = Core.Catalog.Utils.ConvertStringToDateTimeByFormat(
                        x.UPDATE_DATE,
                        EPGChannelProgrammeObject.DATE_FORMAT,
                        out var updateDate) ? updateDate : DateTime.MinValue
                }).ToList();

                // get assets from catalog/cache

                if (Utils.Utils.DoesGroupUsesTemplates(contextData.GroupId))
                {
                    KalturaAssetListResponse getAssetRes = GetAssetsForOPCAccount(contextData.GroupId, request.domainId, assetsBaseDataList, Utils.Utils.IsAllowedToViewInactiveAssets(contextData.GroupId, request.m_sSiteGuid, true));
                    if (getAssetRes != null)
                    {
                        result.Objects = getAssetRes.Objects;
                    }
                }
                else
                {
                    result.Objects = CatalogUtils.GetAssets(assetsBaseDataList, request);
                }

                result.TotalCount = epgProgramResponse.m_nTotalItems;

                if (result.Objects != null)
                {
                    result.TotalCount = epgProgramResponse.m_nTotalItems;
                }
                else
                {
                    throw new ClientException(StatusCode.Error);
                }
            }

            return result;
        }

        internal List<KalturaEPGChannelAssets> GetEPGByChannelIds(int groupId, string userID, int domainId, string udid, string language, int pageIndex, int? pageSize, List<int> epgIds, DateTime startTime, DateTime endTime, List<KalturaCatalogWith> with)
        {
            List<KalturaEPGChannelAssets> result = new List<KalturaEPGChannelAssets>();

            // get group configuration
            Group group = GroupsManager.Instance.GetGroup(groupId);

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

            var isBaseResponse = CatalogUtils.GetBaseResponse<EpgResponse>(request, out epgProgramResponse);
            if (isBaseResponse && epgProgramResponse != null)
            {
                result = CatalogConvertor.ConvertEPGChannelAssets(groupId, epgProgramResponse.programsPerChannel, with);

                if (result == null)
                {
                    throw new ClientException(StatusCode.Error);
                }
            }

            return result;

        }

        public KalturaChannel GetChannelInfo(ContextData contextData, int channelId)
        {
            KalturaChannel result = null;
            ChannelObjRequest request = new ChannelObjRequest()
            {
                m_sSignature = Signature,
                m_sSignString = SignString,
                m_oFilter = new Filter()
                {
                    m_sDeviceId = contextData.Udid,
                    m_nLanguage = Utils.Utils.GetLanguageId(contextData.GroupId, contextData.Language),
                },
                m_sSiteGuid = contextData.UserId.ToString(),
                m_nGroupID = contextData.GroupId,
                m_sUserIP = Utils.Utils.GetClientIP(),
                domainId = (int)contextData.DomainId,
                ChannelId = channelId,
                OriginalUserId = contextData.OriginalUserId
            };

            ChannelObjResponse response = null;
            if (CatalogUtils.GetBaseResponse(request, out response))
            {
                Version requestVersion = OldStandardAttribute.getCurrentRequestVersion();
                if (requestVersion.CompareTo(opcMergeVersion) > 0)
                {
                    result = response.ChannelObj != null ? Mapper.Map<KalturaDynamicChannel>(response.ChannelObj) : null;
                }
                else
                {
                    result = response.ChannelObj != null ? Mapper.Map<KalturaChannel>(response.ChannelObj) : null;
                }
            }
            else
            {
                throw new ClientException(StatusCode.Error);
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
            if (CatalogUtils.GetBaseResponse(request, out response) && response != null)
            {
                result = Mapper.Map<KalturaOTTCategory>(response);
            }
            else
            {
                throw new ClientException(StatusCode.Error);
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
            if (!CatalogUtils.GetBaseResponse(request, out response) || response == null || response.Status == null)
            {
                throw new ClientException(StatusCode.Error);
            }
            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status);
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
            if (!CatalogUtils.GetBaseResponse(request, out response) || response == null || response.Status == null)
            {
                throw new ClientException(StatusCode.Error);
            }
            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status);
            }

            result = CatalogMappings.ConvertBookmarks(response.AssetsBookmarks, orderBy);

            return new KalturaBookmarkListResponse() { AssetsBookmarks = result, TotalCount = response.m_nTotalItems };

        }

        public KalturaAssetInfoListResponse GetExternalChannelAssets(int groupId, string channelId,
            string siteGuid, int domainId, string udid, string language, int pageIndex, int? pageSize,
            KalturaOrder? orderBy, List<KalturaCatalogWith> with,
            string deviceType = null, string utcOffset = null, string freeParam = null, int? trendingDays = null)
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

            if (trendingDays.HasValue)
                order.trendingAssetWindow = DateTime.UtcNow.AddDays(-trendingDays.Value);

            // get group configuration
            Group group = GroupsManager.Instance.GetGroup(groupId);

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

            if (!CatalogUtils.GetBaseResponse<UnifiedSearchExternalResponse>(request, out searchResponse, true, key.ToString()))
            {
                // general error
                throw new ClientException(StatusCode.Error);
            }

            if (searchResponse == null || searchResponse.status == null)
            {
                // Bad response received from WS
                throw new ClientException(StatusCode.Error);
            }

            if (searchResponse.status.Code != (int)StatusCode.OK)
            {
                // Bad response received from WS
                throw new ClientException(searchResponse.status);
            }

            if (searchResponse.searchResults != null && searchResponse.searchResults.Count > 0)
            {
                // get base objects list
                List<BaseObject> assetsBaseDataList = searchResponse.searchResults.Select(x => x as BaseObject).ToList();

                // get assets from catalog/cache
                List<KalturaIAssetable> assetsInfo =
                    CatalogUtils.GetAssets(assetsBaseDataList, request, with, CatalogConvertor.ConvertBaseObjectsToAssetsInfo);

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

        public bool AddBookmark(int groupId, string siteGuid, int householdId, string udid, string assetId, KalturaAssetType assetType, long fileId,
                                  int Position, string action, int averageBitRate, int totalBitRate, int currentBitRate, long programId = 0, bool isReportingMode = false, string userIp = null)
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
                m_sUserIP = userIp ?? Utils.Utils.GetClientIP(),
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
                    m_nTotalBitRate = totalBitRate,
                    ProgramId = programId,
                    IsReportingMode = isReportingMode
                }
            };

            log.DebugFormat("BookmarkAdd - MediaMarkRequest details: userId {0}, assetId {1}, action {2}, udid {3}, programId {4}, assetType {5}, isReportingMode {6}",
                            request.m_sSiteGuid,
                            request.m_oMediaPlayRequestData.m_sAssetID,
                            request.m_oMediaPlayRequestData.m_sAction,
                            request.m_oMediaPlayRequestData.m_sUDID,
                            request.m_oMediaPlayRequestData.ProgramId,
                            request.m_oMediaPlayRequestData.m_eAssetType,
                            request.m_oMediaPlayRequestData.IsReportingMode);

            // fire search request
            MediaMarkResponse response = new MediaMarkResponse();

            if (!CatalogUtils.GetBaseResponse<MediaMarkResponse>(request, out response))
            {
                // general error
                throw new ClientException(StatusCode.Error);
            }

            if (response.status.Code != (int)StatusCode.OK)
            {
                // Bad response received from WS
                throw new ClientException(response.status);
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
            Group group = GroupsManager.Instance.GetGroup(groupId);

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
                specificAssets = assets.Select(asset => new KeyValuePair<eAssetTypes, long>(eAssetTypes.MEDIA, asset.getId())).ToList()
                //assetTypes = assetTypes,
            };

            // fire unified search request
            UnifiedSearchResponse searchResponse = new UnifiedSearchResponse();
            if (!CatalogUtils.GetBaseResponse<UnifiedSearchResponse>(request, out searchResponse, true, null))
            {
                // general error
                throw new ClientException(StatusCode.Error);
            }

            if (searchResponse.status.Code != (int)StatusCode.OK)
            {
                // Bad response received from WS
                throw new ClientException(searchResponse.status);
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
            if (CatalogUtils.GetBaseResponse(request, out response) && response != null && response.Status != null)
            {
                if (response.Status.Code == (int)StatusCode.OK)
                {
                    result = Mapper.Map<KalturaCountry>(response.Country);
                }
                else
                {
                    throw new ClientException(response.Status);
                }
            }
            else
            {
                throw new ClientException(StatusCode.Error);
            }

            return result;
        }

        internal KalturaAssetListResponse GetExternalChannelAssets
            (ContextData contextData, string channelId, int pageIndex, int? pageSize, string deviceType, string utcOffset, string freeParam, string alias)
        {
            KalturaAssetListResponse result = new KalturaAssetListResponse();

            // get group configuration
            Group group = GroupsManager.Instance.GetGroup(contextData.GroupId);

            // build request
            ExternalChannelRequest request = new ExternalChannelRequest()
            {
                m_sSignature = Signature,
                m_sSignString = SignString,
                deviceId = contextData.Udid,
                deviceType = deviceType,
                domainId = (int)(contextData.DomainId ?? 0),
                internalChannelID = channelId,
                externalChannelID = alias,
                m_nGroupID = contextData.GroupId,
                m_nPageIndex = pageIndex,
                m_nPageSize = pageSize.Value,
                m_oFilter = new Filter()
                {
                    m_sDeviceId = contextData.Udid,
                    m_nLanguage = Utils.Utils.GetLanguageId(contextData.GroupId, contextData.Language),
                    m_bUseStartDate = group.UseStartDate,
                    m_bOnlyActiveMedia = group.GetOnlyActiveAssets
                },
                m_sSiteGuid = contextData.UserId.ToString(),
                m_sUserIP = Utils.Utils.GetClientIP(),
                utcOffset = utcOffset,
                free = freeParam,
                OriginalUserId = contextData.OriginalUserId
            };

            // fire search request
            UnifiedSearchExternalResponse searchResponse = new UnifiedSearchExternalResponse();

            if (!CatalogUtils.GetBaseResponse(request, out searchResponse))
            {
                // general error
                throw new ClientException(StatusCode.Error);
            }

            if (searchResponse == null || searchResponse.status == null)
            {
                // Bad response received from WS
                throw new ClientException(StatusCode.Error);
            }

            if (searchResponse.status.Code != (int)StatusCode.OK)
            {
                // Bad response received from WS
                throw new ClientException(searchResponse.status);
            }
            if (searchResponse.searchResults != null && searchResponse.searchResults.Count > 0)
            {
                // get base objects list
                List<BaseObject> assetsBaseDataList = searchResponse.searchResults.Select(x => x as BaseObject).ToList();

                // get assets from catalog/cache
                result.Objects = CatalogUtils.GetAssets(assetsBaseDataList, request);

                CatalogUtils.UpdateEpgTags(result.Objects, assetsBaseDataList);

                result.TotalCount = searchResponse.m_nTotalItems;
            }

            //result..RequestId = searchResponse.requestId;

            return result;
        }

        internal KalturaAssetListResponse GetRelatedMediaExternal
            (ContextData contextData, int pageIndex, int? pageSize, int mediaId, List<int> mediaTypes, int utcOffset, string freeParam)
        {
            KalturaAssetListResponse result = new KalturaAssetListResponse();

            // get group configuration
            Group group = GroupsManager.Instance.GetGroup(contextData.GroupId);

            // build request
            MediaRelatedExternalRequest request = new MediaRelatedExternalRequest()
            {
                m_sSignature = Signature,
                m_sSignString = SignString,
                m_oFilter = new Filter()
                {
                    m_sDeviceId = contextData.Udid,
                    m_nLanguage = Utils.Utils.GetLanguageId(contextData.GroupId, contextData.Language),
                    m_bUseStartDate = group.UseStartDate,
                    m_bOnlyActiveMedia = group.GetOnlyActiveAssets
                },
                m_sLanguage = contextData.Language,
                m_sUserIP = Utils.Utils.GetClientIP(),
                m_nGroupID = contextData.GroupId,
                m_nPageIndex = pageIndex,
                m_nPageSize = pageSize.Value,
                m_nMediaID = mediaId,
                m_nMediaTypes = mediaTypes,
                m_sSiteGuid = contextData.UserId.ToString(),
                domainId = (int)(contextData.DomainId ?? 0),
                m_nUtcOffset = utcOffset,
                m_sFreeParam = freeParam,
                OriginalUserId = contextData.OriginalUserId
            };

            // build failover cache key
            StringBuilder key = new StringBuilder();
            key.AppendFormat("related_media_id={0}_pi={1}_pz={2}_g={3}_l={4}_mt={5}",
                mediaId, pageIndex, pageSize, contextData.GroupId, contextData.Language, mediaTypes != null ? string.Join(",", mediaTypes.ToArray()) : string.Empty);

            result = CatalogUtils.GetMediaWithStatus(request, key.ToString());

            return result;
        }

        internal KalturaAssetListResponse GetSearchMediaExternal(ContextData contextData, int pageIndex, int? pageSize, string query, List<int> mediaTypes, int utcOffset)
        {
            KalturaAssetListResponse result = new KalturaAssetListResponse();

            // get group configuration
            Group group = GroupsManager.Instance.GetGroup(contextData.GroupId);

            // build request
            MediaSearchExternalRequest request = new MediaSearchExternalRequest()
            {
                m_sSignature = Signature,
                m_sSignString = SignString,
                m_oFilter = new Filter()
                {
                    m_sDeviceId = contextData.Udid,
                    m_nLanguage = Utils.Utils.GetLanguageId(contextData.GroupId, contextData.Language),
                    m_bUseStartDate = group.UseStartDate,
                    m_bOnlyActiveMedia = group.GetOnlyActiveAssets
                },
                m_sLanguage = contextData.Language,
                m_sUserIP = Utils.Utils.GetClientIP(),
                m_nGroupID = contextData.GroupId,
                m_nPageIndex = pageIndex,
                m_nPageSize = pageSize.Value,
                m_sQuery = query,
                m_nMediaTypes = mediaTypes,
                m_sSiteGuid = contextData.UserId.ToString(),
                domainId = (int)(contextData.DomainId ?? 0),
                m_nUtcOffset = utcOffset,
                m_sDeviceID = contextData.Udid,
                OriginalUserId = contextData.OriginalUserId
            };

            // build failover cache key
            StringBuilder key = new StringBuilder();
            key.AppendFormat("search_q={0}_pi={1}_pz={2}_g={3}_l={4}_mt={5}",
                query, pageIndex, pageSize, contextData.GroupId, contextData.Language, mediaTypes != null ? string.Join(",", mediaTypes.ToArray()) : string.Empty);

            UnifiedSearchResponse response = new UnifiedSearchResponse();
            if (!CatalogUtils.GetBaseResponse<UnifiedSearchResponse>(request, out response, true, key.ToString())
                || response == null || response.status.Code != (int)StatusCode.OK)
            {
                if (response == null)
                    throw new ClientException(StatusCode.Error);

                // general error
                throw new ClientException(response.status);
            }

            if (response.searchResults != null && response.searchResults.Count > 0)
            {
                result.Objects = CatalogUtils.GetAssets(response.searchResults.Select(a => (BaseObject)a).ToList(), request);
                result.TotalCount = response.m_nTotalItems;
            }
            return result;
        }

        internal KalturaAssetListResponse GetChannelAssets(ContextData contextData,
            int pageIndex,
            int? pageSize,
            int id,
            IReadOnlyCollection<KalturaBaseAssetOrder> orderings,
            string filterQuery,
            KalturaBaseResponseProfile responseProfile = null,
            bool isAllowedToViewInactiveAssets = false,
            List<string> groupByValues = null,
            bool allowIncludedGroupBy = false,
            bool shouldApplyPriorityGroups = false)
        {
            // Create catalog order object
            var orderingParameters = orderings != null ? KalturaOrderMapper.Instance.MapParameters(orderings) : null;

            // get group configuration
            Group group = GroupsManager.Instance.GetGroup(contextData.GroupId);

            // build request
            var request = new InternalChannelRequest
            {
                m_sSignature = Signature,
                m_sSignString = SignString,
                m_oFilter = new Filter
                {
                    m_sDeviceId = contextData.Udid,
                    m_nLanguage = Utils.Utils.GetLanguageId(contextData.GroupId, contextData.Language),
                    m_bUseStartDate = group.UseStartDate,
                    m_bOnlyActiveMedia = group.GetOnlyActiveAssets
                },
                m_sUserIP = Utils.Utils.GetClientIP(),
                m_nGroupID = contextData.GroupId,
                m_nPageIndex = pageIndex,
                m_nPageSize = pageSize.Value,
                m_sSiteGuid = contextData.UserId.ToString(),
                domainId = (int)(contextData.DomainId ?? 0),
                orderingParameters = orderingParameters,
                internalChannelID = id.ToString(),
                filterQuery = filterQuery,
                m_dServerTime = getServerTime(),
                m_bIgnoreDeviceRuleID = false,
                isAllowedToViewInactiveAssets = isAllowedToViewInactiveAssets,
                OriginalUserId = contextData.OriginalUserId
            };

            if (shouldApplyPriorityGroups)
            {
                request.PriorityGroupsMappings = _searchPriorityGroupManager.ListSearchPriorityGroupMappings(contextData.GroupId);
            }

            if (groupByValues != null && groupByValues.Count > 0)
            {
                request.searchGroupBy = new SearchAggregationGroupBy()
                {
                    groupBy = groupByValues,
                    distinctGroup = groupByValues[0], // mabye will send string.empty - and Backend will fill it if nessecery
                    topHitsCount = 1,
                    isGroupingOptionInclude = allowIncludedGroupBy
                };
            }

            // fire request
            if (!CatalogUtils.GetBaseResponse(request, out UnifiedSearchResponse channelResponse))
            {
                // general error
                throw new ClientException(StatusCode.Error);
            }

            if (channelResponse.status.Code != (int)StatusCode.OK)
            {
                // Bad response received from WS
                throw new ClientException(channelResponse.status);
            }

            return GetAssetFromUnifiedSearchResponse(
                contextData.GroupId,
                channelResponse,
                request,
                isAllowedToViewInactiveAssets,
                false,
                responseProfile,
                priorityGroupsMapping: request.PriorityGroupsMappings);
        }

        internal KalturaAssetListResponse GetChannelAssetsExcludeWatched(
            ContextData contextData,
            int pageIndex,
            int? pageSize,
            int id,
            IReadOnlyCollection<KalturaBaseAssetOrder> orderings,
            string filterQuery,
            bool isAllowedToViewInactiveAssets,
            KalturaBaseResponseProfile responseProfile = null,
            bool shouldApplyPriorityGroups = false)
        {
            // Create catalog order object
            var orderingParameters = orderings != null ? KalturaOrderMapper.Instance.MapParameters(orderings) : null;

            // get group configuration
            Group group = GroupsManager.Instance.GetGroup(contextData.GroupId);

            InternalChannelRequest request = null;
            List<BaseObject> totalAssetsBaseDataList = new List<BaseObject>();
            int totalCount = 0;
            List<long> userWatchedMediaIds = new List<long>();

            while (pageIndex < 10)
            {
                var searchResponse = CatalogUtils.GetChannelAssets(
                    contextData,
                    pageIndex,
                    pageSize,
                    id,
                    filterQuery,
                    getServerTime(),
                    orderingParameters,
                    @group,
                    Signature,
                    SignString,
                    ref request,
                    shouldApplyPriorityGroups);

                if (searchResponse.searchResults?.Count > 0)
                {
                    // get base objects list
                    var assetsBaseDataList = searchResponse.searchResults.Select(x => x as BaseObject).ToList();
                    int totalResults = assetsBaseDataList.Count;

                    // Get user user's watched media and exclude them from searchResults
                    if (pageIndex == 0)
                    {
                        userWatchedMediaIds = CatalogUtils.GetUserWatchedMediaIds(contextData.GroupId, (int)(contextData.UserId ?? 0));
                    }

                    // filter out watched media
                    if (userWatchedMediaIds?.Count > 0)
                    {
                        var excludedMediaIds = assetsBaseDataList.Where(x => userWatchedMediaIds.Select(y => y.ToString()).ToList().Contains(x.AssetId) && x.AssetType != eAssetTypes.EPG && x.AssetType != eAssetTypes.NPVR);
                        if (excludedMediaIds != null)
                        {
                            var res = assetsBaseDataList.Except(excludedMediaIds);
                            if (res != null)
                            {
                                assetsBaseDataList = res.ToList();
                            }
                        }
                    }

                    if (totalCount + assetsBaseDataList.Count > pageSize)
                    {
                        totalAssetsBaseDataList.AddRange(assetsBaseDataList.Take(assetsBaseDataList.Count - totalCount));
                        totalCount = pageSize.Value;
                    }
                    else
                    {
                        totalAssetsBaseDataList.AddRange(assetsBaseDataList);
                        totalCount += assetsBaseDataList.Count;
                    }

                    if (totalResults < pageSize || totalCount == pageSize)
                    {
                        break;
                    }

                    pageIndex++;
                }
                else
                {
                    break;
                }
            }

            var channelResponse = new UnifiedSearchResponse
            {
                searchResults = totalAssetsBaseDataList.Select(x => x as UnifiedSearchResult).ToList(),
                m_nTotalItems = totalCount
            };

            return GetAssetFromUnifiedSearchResponse(
                contextData.GroupId,
                channelResponse,
                request,
                isAllowedToViewInactiveAssets,
                responseProfile: responseProfile,
                priorityGroupsMapping: request.PriorityGroupsMappings);
        }

        public KalturaAssetListResponse  GetBundleAssets(SearchAssetsFilter searchAssetsFilter, int id, KalturaBundleType bundleType)
        {
            // get group configuration
            Group group = GroupsManager.Instance.GetGroup(searchAssetsFilter.GroupId);

            eBundleType bType;
            switch (bundleType)
            {
                case KalturaBundleType.subscription:
                    bType = eBundleType.SUBSCRIPTION;
                    break;
                case KalturaBundleType.collection:
                    bType = eBundleType.COLLECTION;
                    break;
                case KalturaBundleType.pago:
                    bType = eBundleType.PAGO;
                    break;
                default:
                    throw new Exception($"Unknown bundle type {bundleType}");
            }

            BaseRequest request;

            if (bType == eBundleType.PAGO)
            {
                request = new PagoBundleAssetRequest
                {
                    m_sSignature = Signature,
                    m_sSignString = SignString,
                    m_oFilter = new Filter
                    {
                        m_sDeviceId = searchAssetsFilter.Udid,
                        m_nLanguage =
                            Utils.Utils.GetLanguageId(searchAssetsFilter.GroupId, searchAssetsFilter.Language),
                        m_bUseStartDate = group.UseStartDate,
                        m_bOnlyActiveMedia = group.GetOnlyActiveAssets
                    },
                    m_sUserIP = Utils.Utils.GetClientIP(),
                    m_nGroupID = searchAssetsFilter.GroupId,
                    m_nPageIndex = searchAssetsFilter.PageIndex,
                    m_nPageSize = searchAssetsFilter.PageSize.Value,
                    m_sSiteGuid = searchAssetsFilter.SiteGuid,
                    domainId = searchAssetsFilter.DomainId,
                    PagoId = id,
                    m_dServerTime = getServerTime(),
                    isGroupingOptionInclude = searchAssetsFilter.GroupByType == GroupingOption.Include,
                    AssetFilterKsql = searchAssetsFilter.Filter,
                    AssetTypes = searchAssetsFilter.AssetTypes,
                    OriginalUserId = searchAssetsFilter.OriginalUserId
                };
            }
            else
            {
                // build request
                request = new BundleAssetsRequest
                {
                    m_sSignature = Signature,
                    m_sSignString = SignString,
                    m_oFilter = new Filter
                    {
                        m_sDeviceId = searchAssetsFilter.Udid,
                        m_nLanguage = Utils.Utils.GetLanguageId(searchAssetsFilter.GroupId, searchAssetsFilter.Language),
                        m_bUseStartDate = group.UseStartDate,
                        m_bOnlyActiveMedia = group.GetOnlyActiveAssets
                    },
                    m_sUserIP = Utils.Utils.GetClientIP(),
                    m_nGroupID = searchAssetsFilter.GroupId,
                    m_nPageIndex = searchAssetsFilter.PageIndex,
                    m_nPageSize = searchAssetsFilter.PageSize.Value,
                    m_sSiteGuid = searchAssetsFilter.SiteGuid,
                    domainId = searchAssetsFilter.DomainId,
                    m_oOrderObj =  ChannelDataRowMapper.BuildOrderObj(KalturaOrderMapper.Instance.MapParameters(searchAssetsFilter.OrderingParameters, OrderBy.NONE).First()),
                    m_sMediaType = searchAssetsFilter.AssetTypes != null ? string.Join(";", searchAssetsFilter.AssetTypes) : null,
                    m_dServerTime = getServerTime(),
                    m_eBundleType = bType,
                    m_nBundleID = id,
                    isAllowedToViewInactiveAssets = searchAssetsFilter.IsAllowedToViewInactiveAssets,
                    isGroupingOptionInclude = searchAssetsFilter.GroupByType == GroupingOption.Include,
                    AssetFilterKsql = searchAssetsFilter.Filter,
                    OriginalUserId = searchAssetsFilter.OriginalUserId
                };
            }

            return CatalogUtils.GetMedia(request, searchAssetsFilter.IsAllowedToViewInactiveAssets);
        }

        internal KalturaAssetCommentListResponse GetAssetCommentsList(int groupId, string language, int id, KalturaAssetType AssetType, string userId, int domainId, string udid,
            int pageIndex, int? pageSize, KalturaAssetCommentOrderBy? orderBy)
        {
            KalturaAssetCommentListResponse result = new KalturaAssetCommentListResponse();

            // get group configuration
            Group group = GroupsManager.Instance.GetGroup(groupId);
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
                assetType = Mapper.Map<eAssetType>(AssetType),
                orderObj = order,
            };

            // build failover cache key
            StringBuilder key = new StringBuilder();
            key.AppendFormat("asset_id={0}_pi={1}_pz={2}_g={3}_l={4}_type={5}",
                id, pageIndex, pageSize, groupId, language, eAssetType.PROGRAM.ToString());
            AssetCommentsListResponse commentResponse = new AssetCommentsListResponse();
            if (CatalogUtils.GetBaseResponse<AssetCommentsListResponse>(request, out commentResponse))
            {
                if (commentResponse.status.Code != (int)StatusCode.OK)
                {
                    // Bad response received from WS
                    throw new ClientException(commentResponse.status);
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
                throw new ClientException(StatusCode.Error);
            }
            return result;
        }

        internal KalturaAssetComment AddAssetComment(int groupId, int assetId, KalturaAssetType assetType, string userId, int domainId, string writer, string header,
                                                     string subHeader, string contextText, string udid, string language)
        {
            KalturaAssetComment result = new KalturaAssetComment();

            // get group configuration
            Group group = GroupsManager.Instance.GetGroup(groupId);

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
                assetType = Mapper.Map<eAssetType>(assetType),
                writer = writer,
                header = header,
                subHeader = subHeader,
                contentText = contextText,
                udid = udid
            };

            AssetCommentResponse assetCommentResponse = null;
            if (CatalogUtils.GetBaseResponse<AssetCommentResponse>(request, out assetCommentResponse))
            {
                if (assetCommentResponse.Status.Code != (int)StatusCode.OK)
                {
                    // Bad response received from WS
                    throw new ClientException(assetCommentResponse.Status);
                }
                else
                {
                    result = assetCommentResponse.AssetComment != null ? Mapper.Map<KalturaAssetComment>(assetCommentResponse.AssetComment) : null;
                }
            }
            else
            {
                // general error
                throw new ClientException(StatusCode.Error);
            }

            return result;
        }

        internal KalturaAssetListResponse GetScheduledRecordingAssets(
            ContextData contextData,
            List<long> channelIdsToFilter,
            int pageIndex,
            int? pageSize,
            long? startDateToFilter,
            long? endDateToFilter,
            IReadOnlyCollection<KalturaBaseAssetOrder> orderings,
            KalturaScheduledRecordingAssetType scheduledRecordingType,
            List<string> seriesIdsToFilter = null)
        {
            KalturaAssetListResponse result = new KalturaAssetListResponse();

            // get group configuration
            Group group = GroupsManager.Instance.GetGroup(contextData.GroupId);

            // build request
            ScheduledRecordingsRequest request = new ScheduledRecordingsRequest
            {
                m_sSignature = Signature,
                m_sSignString = SignString,
                m_oFilter = new Filter()
                {
                    m_sDeviceId = contextData.Udid,
                    m_nLanguage = Utils.Utils.GetLanguageId(contextData.GroupId, contextData.Language),
                    m_bUseStartDate = group.UseStartDate,
                    m_bOnlyActiveMedia = group.GetOnlyActiveAssets
                },
                m_sUserIP = Utils.Utils.GetClientIP(),
                m_nGroupID = contextData.GroupId,
                m_nPageIndex = pageIndex,
                m_nPageSize = pageSize.Value,
                m_sSiteGuid = contextData.UserId.ToString(),
                domainId = (int)(contextData.DomainId ?? 0),
                orderingParameters = KalturaOrderMapper.Instance.MapParameters(orderings, OrderBy.NONE),
                m_dServerTime = getServerTime(),
                channelIds = channelIdsToFilter,
                seriesIds = seriesIdsToFilter,
                scheduledRecordingAssetType = CatalogMappings.ConvertKalturaScheduledRecordingAssetType(scheduledRecordingType),
                startDate = startDateToFilter.HasValue ? DateUtils.UtcUnixTimestampSecondsToDateTime(startDateToFilter.Value) : new DateTime?(),
                endDate = endDateToFilter.HasValue ? DateUtils.UtcUnixTimestampSecondsToDateTime(endDateToFilter.Value) : new DateTime?(),
                OriginalUserId = contextData.OriginalUserId
            };

            // fire request
            UnifiedSearchResponse scheduledRecordingResponse = new UnifiedSearchResponse();
            if (!CatalogUtils.GetBaseResponse<UnifiedSearchResponse>(request, out scheduledRecordingResponse))
            {
                // general error
                throw new ClientException(StatusCode.Error);
            }

            if (scheduledRecordingResponse.status.Code != (int)StatusCode.OK)
            {
                // Bad response received from WS
                throw new ClientException(scheduledRecordingResponse.status);
            }

            if (scheduledRecordingResponse.searchResults != null && scheduledRecordingResponse.searchResults.Count > 0)
            {
                // get base objects list
                List<BaseObject> assetsBaseDataList = scheduledRecordingResponse.searchResults.Select(x => x as BaseObject).ToList();

                //BEO-9994, pagination if from aggs and has pagination
                if (pageSize.HasValue && scheduledRecordingResponse.aggregationResults != null
                    && scheduledRecordingResponse.aggregationResults.Any() && assetsBaseDataList != null && assetsBaseDataList.Count != 0)
                {
                    assetsBaseDataList = assetsBaseDataList.Skip(pageSize.Value * pageIndex)?.Take(pageSize.Value).ToList();
                }

                // get assets from catalog/cache
                result.Objects = CatalogUtils.GetAssets(assetsBaseDataList, request);

                if (scheduledRecordingResponse.aggregationResults != null && scheduledRecordingResponse.aggregationResults.Any())
                {
                    //BEO-9982
                    var bucketList = scheduledRecordingResponse.aggregationResults.Where(x => x.totalItems > 0).ToList();
                    if (bucketList != null && bucketList.Any())
                    {
                        result.TotalCount = bucketList.Sum(x => x.totalItems);
                    }
                    else
                    {
                        result.TotalCount = 0;
                    }
                }
                else
                {
                    result.TotalCount = scheduledRecordingResponse.m_nTotalItems; //BEO-9440
                }
            }

            return result;
        }

        public KalturaLastPositionListResponse GetAssetsLastPositionBookmarks(string siteGuid, int groupId, int domainId, string udid, List<KalturaSlimAsset> assets)
        {
            List<KalturaLastPosition> result = null;
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
            if (!CatalogUtils.GetBaseResponse(request, out response) || response == null || response.Status == null)
            {
                throw new ClientException(StatusCode.Error);
            }
            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status);
            }

            result = CatalogMappings.ConvertBookmarks(response.AssetsBookmarks);

            return new KalturaLastPositionListResponse() { LastPositions = result, TotalCount = response.m_nTotalItems };

        }

        internal KalturaMeta UpdateGroupMeta(int groupId, KalturaMeta meta)
        {
            MetaResponse response = null;
            KalturaMeta result = null;

            try
            {
                Meta apiMeta = null;
                apiMeta = Mapper.Map<Meta>(meta);

                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Catalog.Module.UpdateGroupMeta(groupId, apiMeta);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while UpdateGroupMeta.  groupID: {0}, exception: {1}", groupId, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status);
            }

            if (response.MetaList != null && response.MetaList.Count > 0)
            {
                result = Mapper.Map<KalturaMeta>(response.MetaList[0]);
            }

            return result;
        }

        internal KalturaTagListResponse SearchTags(int groupId, bool isExcatValue, string value, int topicId, string searchLanguage, int pageIndex, int pageSize)
        {
            KalturaTagListResponse result = new KalturaTagListResponse();

            Func<GenericListResponse<TagValue>> searchTagsFunc = delegate ()
            {
                int searchLanguageId = Utils.Utils.GetLanguageId(groupId, searchLanguage);
                return CatalogManager.SearchTags(groupId, isExcatValue, value, topicId, searchLanguageId, pageIndex, pageSize);
            };

            KalturaGenericListResponse<KalturaTag> response =
                ClientUtils.GetResponseListFromWS<KalturaTag, TagValue>(searchTagsFunc);

            result.Tags = response.Objects;
            result.TotalCount = response.TotalCount;

            return result;
        }

        internal KalturaTagListResponse GetTags(int groupId, List<long> idIn, int pageIndex, int pageSize)
        {
            KalturaTagListResponse result = new KalturaTagListResponse();

            Func<GenericListResponse<TagValue>> searchTagsFunc = delegate ()
            {
                return CatalogManager.Instance.GetTags(groupId, idIn, pageIndex, pageSize);
            };

            KalturaGenericListResponse<KalturaTag> response =
                ClientUtils.GetResponseListFromWS<KalturaTag, TagValue>(searchTagsFunc);

            result.Tags = response.Objects;
            result.TotalCount = response.TotalCount;

            return result;
        }

        internal KalturaTag AddTag(int groupId, KalturaTag tag, long userId)
        {
            Func<TagValue, GenericResponse<TagValue>> addTagFunc = (TagValue requestTag) =>
                CatalogManager.Instance.AddTag(groupId, requestTag, userId);

            KalturaTag result =
                ClientUtils.GetResponseFromWS<KalturaTag, TagValue>(tag, addTagFunc);

            return result;
        }

        internal KalturaTag UpdateTag(int groupId, long id, KalturaTag tag, long userId)
        {
            tag.Id = id;
            Func<TagValue, GenericResponse<TagValue>> updateTagFunc = (TagValue tagToUpdate) =>
                CatalogManager.UpdateTag(groupId, tagToUpdate, userId);

            KalturaTag result =
                ClientUtils.GetResponseFromWS<KalturaTag, TagValue>(tag, updateTagFunc);

            return result;
        }

        internal bool DeleteTag(int groupId, long id, long userId)
        {
            Func<Status> deleteTagFunc = () => CatalogManager.DeleteTag(groupId, id, userId);
            return ClientUtils.GetResponseStatusFromWS(deleteTagFunc);
        }

        internal KalturaImageTypeListResponse GetImageTypes(int groupId, bool isSearchByIds, List<long> ids)
        {
            KalturaImageTypeListResponse result = new KalturaImageTypeListResponse() { TotalCount = 0 };

            Func<GenericListResponse<ImageType>> getImageTypesFunc = () =>
               Core.Catalog.CatalogManagement.ImageManager.GetImageTypes(groupId, isSearchByIds, ids);

            KalturaGenericListResponse<KalturaImageType> response =
                ClientUtils.GetResponseListFromWS<KalturaImageType, ImageType>(getImageTypesFunc);

            result.ImageTypes = response.Objects;
            result.TotalCount = response.TotalCount;

            return result;
        }

        internal KalturaImageType AddImageType(int groupId, long userId, KalturaImageType imageType)
        {
            Func<ImageType, GenericResponse<ImageType>> addImageTypeFunc = (ImageType requestImageType) =>
                Core.Catalog.CatalogManagement.ImageManager.AddImageType(groupId, requestImageType, userId);

            KalturaImageType result =
                ClientUtils.GetResponseFromWS<KalturaImageType, ImageType>(imageType, addImageTypeFunc);

            return result;
        }

        internal KalturaImageType UpdateImageType(int groupId, long userId, long id, KalturaImageType imageType)
        {
            Func<ImageType, GenericResponse<ImageType>> updateImageTypeFunc = (ImageType requestImageType) =>
                Core.Catalog.CatalogManagement.ImageManager.UpdateImageType(groupId, id, requestImageType, userId);

            KalturaImageType result =
                ClientUtils.GetResponseFromWS<KalturaImageType, ImageType>(imageType, updateImageTypeFunc);

            return result;
        }

        internal bool DeleteImageType(int groupId, long userId, long id)
        {
            Func<Status> deleteImageTypeFunc = () => Core.Catalog.CatalogManagement.ImageManager.DeleteImageType(groupId, id, userId);
            return ClientUtils.GetResponseStatusFromWS(deleteImageTypeFunc);
        }

        internal KalturaRatioListResponse GetRatios(int groupId)
        {
            KalturaRatioListResponse result = new KalturaRatioListResponse();

            Func<GenericListResponse<Ratio>> getRatiosFunc = () =>
               Core.Catalog.CatalogManagement.ImageManager.GetRatios(groupId);

            KalturaGenericListResponse<KalturaRatio> response =
                ClientUtils.GetResponseListFromWS<KalturaRatio, Ratio>(getRatiosFunc);

            result.Ratios = response.Objects;
            result.TotalCount = response.TotalCount;

            return result;
        }

        internal KalturaImageListResponse GetImagesByIds(int groupId, List<long> imagesIds, bool? isDefault = null)
        {
            KalturaImageListResponse result = new KalturaImageListResponse() { TotalCount = 0 };

            Func<GenericListResponse<Image>> getImagesByIdsFunc = () =>
               Core.Catalog.CatalogManagement.ImageManager.GetImagesByIds(groupId, imagesIds, isDefault);

            KalturaGenericListResponse<KalturaImage> response =
                ClientUtils.GetResponseListFromWS<KalturaImage, Image>(getImagesByIdsFunc);

            result.Images = response.Objects;
            result.TotalCount = response.TotalCount;

            return result;
        }

        internal KalturaImageListResponse GetImagesByObject(int groupId, long imageObjectId, KalturaImageObjectType imageObjectType, bool? isDefault = null)
        {
            KalturaImageListResponse result = new KalturaImageListResponse() { TotalCount = 0 };

            Func<GenericListResponse<Image>> getImagesByObjectFunc = () =>
               Core.Catalog.CatalogManagement.ImageManager.Instance.GetImagesByObject(groupId, imageObjectId, CatalogMappings.ConvertImageObjectType(imageObjectType), isDefault);

            KalturaGenericListResponse<KalturaImage> response =
                ClientUtils.GetResponseListFromWS<KalturaImage, Image>(getImagesByObjectFunc);

            result.Images = response.Objects;
            result.TotalCount = response.TotalCount;

            return result;
        }

        internal bool DeleteImage(int groupId, long userId, long id)
        {
            Func<Status> deleteImageFunc = () => Core.Catalog.CatalogManagement.ImageManager.Instance.DeleteImage(groupId, id, userId, isStandaloneOperation: true);
            return ClientUtils.GetResponseStatusFromWS(deleteImageFunc);
        }

        internal KalturaImage AddImage(int groupId, long userId, KalturaImage image)
        {
            Func<Image, GenericResponse<Image>> addImageFunc = (Image requestImage) =>
               Core.Catalog.CatalogManagement.ImageManager.Instance.AddImage(groupId, requestImage, userId);

            KalturaImage result =
                ClientUtils.GetResponseFromWS<KalturaImage, Image>(image, addImageFunc);

            Dictionary<long, string> imageTypeIdToNameMap = Core.Catalog.CatalogManagement.ImageManager.GetImageTypeIdToNameMap(groupId);
            result.ImageTypeName = imageTypeIdToNameMap != null && imageTypeIdToNameMap.ContainsKey(result.ImageTypeId) ?
                imageTypeIdToNameMap[result.ImageTypeId] : string.Empty;

            return result;
        }

        internal void SetContent(int groupId, long userId, long id, string url)
        {
            Status response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Catalog.CatalogManagement.ImageManager.Instance.SetContent(groupId, userId, id, url, isStandaloneOperation: true);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling catalog service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (response.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response);
            }
        }

        internal KalturaRatio AddRatio(int groupId, long userId, KalturaRatio ratio)
        {
            KalturaRatio responseRatio = new KalturaRatio();
            RatioResponse response = null;

            try
            {
                var requestRatio = Mapper.Map<Ratio>(ratio);
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Catalog.CatalogManagement.ImageManager.AddRatio(groupId, userId, requestRatio);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling api service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null || response.Status == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status);
            }

            if (response.Ratio != null)
            {
                responseRatio = Mapper.Map<KalturaRatio>(response.Ratio);
            }

            return responseRatio;
        }

        internal KalturaRatio UpdateRatio(int groupId, long userId, KalturaRatio ratio, long ratioId)
        {
            KalturaRatio responseRatio = new KalturaRatio();
            RatioResponse response = null;

            try
            {
                var requestRatio = Mapper.Map<Ratio>(ratio);
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Catalog.CatalogManagement.ImageManager.UpdateRatio(groupId, userId, requestRatio, ratioId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling api service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null || response.Status == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status);
            }

            if (response.Ratio != null)
            {
                responseRatio = Mapper.Map<KalturaRatio>(response.Ratio);
            }

            return responseRatio;
        }

        public KalturaMediaFileTypeListResponse GetMediaFileTypes(int groupId)
        {
            KalturaMediaFileTypeListResponse result = new KalturaMediaFileTypeListResponse() { TotalCount = 0 };

            Func<GenericListResponse<MediaFileType>> getMediaFileTypesFunc = () =>
               Core.Catalog.CatalogManagement.FileManager.Instance.GetMediaFileTypes(groupId);

            KalturaGenericListResponse<KalturaMediaFileType> response =
                ClientUtils.GetResponseListFromWS<KalturaMediaFileType, MediaFileType>(getMediaFileTypesFunc);

            result.Types = response.Objects;
            result.TotalCount = response.TotalCount;

            return result;
        }

        public KalturaMediaFileType AddMediaFileType(int groupId, KalturaMediaFileType mediaFileType, long userId)
        {
            Func<MediaFileType, GenericResponse<MediaFileType>> addMediaFileTypeFunc = (MediaFileType mediaFileTypeToAdd) =>
               FileManager.AddMediaFileType(groupId, mediaFileTypeToAdd, userId);

            KalturaMediaFileType result =
                ClientUtils.GetResponseFromWS<KalturaMediaFileType, MediaFileType>(mediaFileType, addMediaFileTypeFunc);

            return result;
        }

        public KalturaMediaFileType UpdateMediaFileType(int groupId, long id, KalturaMediaFileType mediaFileType, long userId)
        {
            Func<MediaFileType, GenericResponse<MediaFileType>> updateMediaFileTypeFunc = (MediaFileType mediaFileTypeToUpdate) =>
                FileManager.UpdateMediaFileType(groupId, id, mediaFileTypeToUpdate, userId);

            KalturaMediaFileType result =
                ClientUtils.GetResponseFromWS<KalturaMediaFileType, MediaFileType>(mediaFileType, updateMediaFileTypeFunc);

            return result;
        }

        public bool DeleteMediaFileType(int groupId, long id, long userId)
        {
            Func<Status> deleteMediaFileTypeFunc = () => FileManager.DeleteMediaFileType(groupId, id, userId);
            return ClientUtils.GetResponseStatusFromWS(deleteMediaFileTypeFunc);
        }

        internal KalturaMediaFile AddMediaFile(int groupId, KalturaMediaFile assetFile, long userId)
        {
            Func<AssetFile, GenericResponse<AssetFile>> insertMediaFileFunc = (AssetFile assetFileToAdd) =>
                FileManager.Instance.InsertMediaFile(groupId, userId, assetFileToAdd);

            KalturaMediaFile result =
                ClientUtils.GetResponseFromWS<KalturaMediaFile, AssetFile>(assetFile, insertMediaFileFunc);

            return result;
        }

        internal bool DeleteMediaFile(int groupId, long userId, long id)
        {
            Func<Status> deleteMediaFileFunc = () => FileManager.Instance.DeleteMediaFile(groupId, userId, id);
            return ClientUtils.GetResponseStatusFromWS(deleteMediaFileFunc);
        }

        internal KalturaMediaFile UpdateMediaFile(int groupId, long id, KalturaMediaFile assetFile, long userId)
        {
            assetFile.Id = (int)id;

            Func<AssetFile, GenericResponse<AssetFile>> updateMediaFileFunc = (AssetFile assetFileToUpdate) =>
                FileManager.Instance.UpdateMediaFile(groupId, assetFileToUpdate, userId);

            KalturaMediaFile result =
                ClientUtils.GetResponseFromWS<KalturaMediaFile, AssetFile>(assetFile, updateMediaFileFunc);

            return result;
        }

        internal KalturaMediaFileListResponse GetMediaFiles(int groupId, long id, long assetId)
        {
            KalturaMediaFileListResponse result = new KalturaMediaFileListResponse() { TotalCount = 0 };

            Func<GenericListResponse<AssetFile>> getMediaFilesFunc = () =>
               FileManager.Instance.GetMediaFiles(groupId, id, assetId);

            KalturaGenericListResponse<KalturaMediaFile> response =
                ClientUtils.GetResponseListFromWS<KalturaMediaFile, AssetFile>(getMediaFilesFunc);

            result.Files = response.Objects;
            result.TotalCount = response.TotalCount;

            return result;
        }

        internal KalturaChannel GetChannel(ContextData contextData, int channelId, bool isAllowedToViewInactiveAssets)
        {
            GenericResponse<Channel> response = null;
            KalturaChannel result = null;
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = ChannelManager.Instance.GetChannel(contextData, channelId, isAllowedToViewInactiveAssets, true);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while calling GetChannel. groupId: {0}, exception: {1}", contextData.GroupId, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status);
            }

            // DynamicChannel
            if (response.Object.m_nChannelTypeID == 4)
            {
                result = Mapper.Map<KalturaDynamicChannel>(response.Object);
            }
            // Should only be manual channel
            else
            {
                result = Mapper.Map<KalturaManualChannel>(response.Object);
            }

            return result;
        }

        internal KalturaChannel InsertKSQLChannel(int groupId, KalturaChannel channel, long userId)
        {
            KSQLChannelResponse response = null;
            KalturaChannel result = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    KSQLChannel request = Mapper.Map<KSQLChannel>(channel);
                    response = KSQLChannelsManager.Insert(groupId, request);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while calling InsertKSQLChannel. groupId: {0}, exception: {1}", groupId, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status);
            }

            result = Mapper.Map<KalturaChannel>(response.Channel);
            return result;
        }

        internal KalturaChannel InsertChannel(int groupId, KalturaChannel channel, UserSearchContext searchContext)
        {
            KalturaChannel result = null;
            GenericResponse<Channel> response = null;

            try
            {
                Channel channelToAdd = Mapper.Map<Channel>(channel);
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = ChannelManager.AddChannel(groupId, channelToAdd, searchContext);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling catalog service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status);
            }

            // DynamicChannel
            if (response.Object.m_nChannelTypeID == 4)
            {
                result = Mapper.Map<KalturaDynamicChannel>(response.Object);
            }
            // Should only be manual channel
            else
            {
                result = Mapper.Map<KalturaManualChannel>(response.Object);
            }

            return result;
        }

        internal KalturaChannel UpdateChannel(int groupId, int id, KalturaChannel channel, UserSearchContext searchContext)
        {
            KalturaChannel result = null;
            GenericResponse<Channel> response = null;

            try
            {
                Type manualChannelType = typeof(KalturaManualChannel);
                Channel channelToUpdate = Mapper.Map<Channel>(channel);
                if (manualChannelType.IsAssignableFrom(channel.GetType()) && ((KalturaManualChannel)channel).MediaIds == null)
                {
                    channelToUpdate.m_lManualMedias = null;
                }

                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = ChannelManager.Instance.UpdateChannel(groupId, id, channelToUpdate, searchContext);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling catalog service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status);
            }

            // DynamicChannel
            if (response.Object.m_nChannelTypeID == 4)
            {
                result = Mapper.Map<KalturaDynamicChannel>(response.Object);
            }
            // Should only be manual channel
            else
            {
                result = Mapper.Map<KalturaManualChannel>(response.Object);
            }

            return result;
        }

        internal KalturaChannel SetKSQLChannel(int groupId, KalturaChannel channel, long userId)
        {
            KSQLChannelResponse response = null;
            KalturaChannel profile = null;



            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    KSQLChannel request = Mapper.Map<KSQLChannel>(channel);
                    response = Core.Api.Module.SetKSQLChannel(groupId, request, userId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while SetKSQLChannel. groupID: {0}, exception: {1}", groupId, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status);
            }

            profile = Mapper.Map<KalturaChannel>(response.Channel);
            return profile;
        }


        [Obsolete]
        internal KalturaChannelProfile InsertKSQLChannelProfile(int groupId, KalturaChannelProfile channel, long userId)
        {
            KSQLChannelResponse response = null;
            KalturaChannelProfile profile = null;



            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    KSQLChannel request = Mapper.Map<KSQLChannel>(channel);
                    response = KSQLChannelsManager.Insert(groupId, request);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while InsertKSQLChannel.  groupID: {0}, exception: {1}", groupId, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status);
            }

            profile = Mapper.Map<KalturaChannelProfile>(response.Channel);
            return profile;
        }

        [Obsolete]
        internal KalturaChannelProfile SetKSQLChannelProfile(int groupId, KalturaChannelProfile channel, long userId)
        {
            KSQLChannelResponse response = null;
            KalturaChannelProfile profile = null;



            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    KSQLChannel request = Mapper.Map<KSQLChannel>(channel);
                    response = Core.Api.Module.SetKSQLChannel(groupId, request, userId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while SetKSQLChannel. groupID: {0}, exception: {1}", groupId, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status);
            }

            profile = Mapper.Map<KalturaChannelProfile>(response.Channel);
            return profile;
        }

        internal bool DeleteChannel(int groupId, int channelId, long userId)
        {
            Func<Status> deleteChannelFunc = () => ChannelManager.Instance.DeleteChannel(groupId, channelId, userId);
            return ClientUtils.GetResponseStatusFromWS(deleteChannelFunc); ;
        }

        internal KalturaAssetListResponse GetPersonalListAssets(
            ContextData contextData,
            string kSql,
            IReadOnlyCollection<KalturaBaseAssetOrder> orderingParameters,
            List<string> groupBy,
            int pageIndex,
            int pageSize,
            HashSet<int> partnerListTypes,
            KalturaBaseResponseProfile responseProfile,
            bool shouldApplyPriorityGroups)
        {
            var personalListResponse = ClientsManager.ApiClient().GetPersonalListItems(
                contextData.GroupId,
                (int)contextData.UserId,
                0,
                0,
                KalturaPersonalListOrderBy.CREATE_DATE_ASC,
                partnerListTypes);

            if (personalListResponse.PersonalListList?.Count > 0)
            {
                StringBuilder ksqlBuilder = new StringBuilder();

                ksqlBuilder.AppendFormat("(or");
                foreach (var personalList in personalListResponse.PersonalListList)
                {
                    ksqlBuilder.AppendFormat(" {0}", personalList.Ksql);
                }
                ksqlBuilder.AppendFormat(")");

                string ksqlFilter = ksqlBuilder.ToString();

                if (!string.IsNullOrEmpty(kSql))
                {
                    ksqlFilter = $"(and {kSql} {ksqlFilter})";
                }

                var searchAssetsFilter = new SearchAssetsFilter
                {
                    GroupId = contextData.GroupId,
                    SiteGuid = contextData.UserId.ToString(),
                    DomainId = (int)(contextData.DomainId ?? 0),
                    Udid = contextData.Udid,
                    Language = contextData.Language,
                    PageIndex = pageIndex,
                    PageSize = pageSize,
                    Filter = ksqlFilter.ToString(),
                    AssetTypes = null,
                    EpgChannelIds = null,
                    ManagementData = false,
                    GroupBy = groupBy,
                    IsAllowedToViewInactiveAssets = false,
                    IgnoreEndDate = false,
                    GroupByType = GroupingOption.Omit,
                    IsPersonalListSearch = true,
                    UseFinal = false,
                    OrderingParameters = orderingParameters,
                    ShouldApplyPriorityGroups = shouldApplyPriorityGroups,
                    ResponseProfile = responseProfile,
                    OriginalUserId = contextData.OriginalUserId
                };

                return ClientsManager.CatalogClient().SearchAssets(searchAssetsFilter);
            }

            return new KalturaAssetListResponse();
        }

        internal KalturaBulkUpload AddBulkUpload(int groupId, long userId, string objectTypeName, KalturaBulkUploadJobData jobData, KalturaBulkUploadObjectData objectData, KalturaOTTFile fileData)
        {
            var bulkUploadJobData = Mapper.Map<BulkUploadJobData>(jobData);
            var bulkUploadObjectData = Mapper.Map<BulkUploadObjectData>(objectData);
            OTTBasicFile file = fileData.ConvertToOttFileType();
            Func<GenericResponse<BulkUpload>> addBulkUploadFunc = () => BulkUploadManager.AddBulkUpload(groupId, userId, objectTypeName, BulkUploadJobAction.Upsert, bulkUploadJobData, bulkUploadObjectData, file);
            KalturaBulkUpload result = ClientUtils.GetResponseFromWS<KalturaBulkUpload, BulkUpload>(addBulkUploadFunc);
            return result;
        }

        public KalturaBulkUpload GetBulkUpload(int groupId, long id)
        {
            Func<GenericResponse<BulkUpload>> getBulkUploadFunc = () => BulkUploadManager.GetBulkUpload(groupId, id);

            KalturaBulkUpload response =
                ClientUtils.GetResponseFromWS<KalturaBulkUpload, BulkUpload>(getBulkUploadFunc);

            return response;
        }

        internal Models.Upload.KalturaBulkUploadStatistics GetBulkUploadStatusSummary(long groupId, string bulkObjectType, long CreateDateGreaterThanOrEqual)
        {
            Func<GenericResponse<ApiObjects.BulkUpload.BulkUploadStatistics>> getBulkUploadsFunc = () =>
              BulkUploadManager.GetBulkUploadSummary(groupId, bulkObjectType, CreateDateGreaterThanOrEqual);

            var response = ClientUtils.GetResponseFromWS<Models.Upload.KalturaBulkUploadStatistics, ApiObjects.BulkUpload.BulkUploadStatistics>(getBulkUploadsFunc);

            return response;
        }

        internal KalturaBulkUploadListResponse GetBulkUploadList(int groupId, string bulkObjectType, List<KalturaBulkUploadJobStatus> statuses, DateTime createDate, long? userId, KalturaBulkUploadOrderBy orderBy, KalturaFilterPager pager)
        {
            var statusesIn = Mapper.Map<List<BulkUploadJobStatus>>(statuses);

            Func<GenericListResponse<BulkUpload>> getBulkUploadsFunc = () =>
              BulkUploadManager.GetBulkUploads(groupId, bulkObjectType, createDate, statusesIn, userId);

            KalturaGenericListResponse<KalturaBulkUpload> response =
                ClientUtils.GetResponseListFromWS<KalturaBulkUpload, BulkUpload>(getBulkUploadsFunc);

            switch (orderBy)
            {
                case KalturaBulkUploadOrderBy.UPDATE_DATE_ASC:
                    response.Objects = response.Objects.OrderBy(x => x.UpdateDate).ToList();
                    break;
                case KalturaBulkUploadOrderBy.UPDATE_DATE_DESC:
                    response.Objects = response.Objects.OrderByDescending(x => x.UpdateDate).ToList();
                    break;
                default:
                    break;
            }

            KalturaBulkUploadListResponse result = new KalturaBulkUploadListResponse() { TotalCount = 0 };

            if (pager == null)
            {
                result.Objects = response.Objects;
            }
            else
            {
                bool illegalRequest;
                var pagedObjects = response.Objects.Page(pager.PageSize.Value, pager.GetRealPageIndex(), out illegalRequest);

                if (illegalRequest)
                {
                    result.Objects = response.Objects;
                }
                else
                {
                    result.Objects = new List<KalturaBulkUpload>(pagedObjects);
                }
            }

            result.TotalCount = response.TotalCount;
            return result;
        }

        internal KalturaCategoryTree Duplicate(int groupId, long userId, long categoryItemId, string name)
        {
            Func<GenericResponse<CategoryTree>> duplicateFunc = () => CategoryItemHandler.Instance.Duplicate(groupId, userId, categoryItemId, name);

            KalturaCategoryTree response =
                ClientUtils.GetResponseFromWS<KalturaCategoryTree, CategoryTree>(duplicateFunc);

            return response;
        }

        internal KalturaCategoryTree GetCategoryTree(int groupId, long categoryItemId, bool filter, bool isAllowedToViewInactiveAssets)
        {
            Func<GenericResponse<CategoryTree>> treeFunc = () => CategoryItemHandler.Instance.GetCategoryTree(groupId, categoryItemId, filter, !isAllowedToViewInactiveAssets);

            KalturaCategoryTree response =
                ClientUtils.GetResponseFromWS<KalturaCategoryTree, CategoryTree>(treeFunc);

            return response;
        }

        internal KalturaImageListResponse GetImagesByObjects(int groupId, List<long> imageObjectIds, KalturaImageObjectType imageObjectType, bool? isDefault = null)
        {
            KalturaImageListResponse result = new KalturaImageListResponse() { TotalCount = 0 };

            Func<GenericListResponse<Image>> getImagesByObjectFunc = () =>
               Core.Catalog.CatalogManagement.ImageManager.GetImagesByObjects(groupId, imageObjectIds, CatalogMappings.ConvertImageObjectType(imageObjectType), isDefault);

            KalturaGenericListResponse<KalturaImage> response =
                ClientUtils.GetResponseListFromWS<KalturaImage, Image>(getImagesByObjectFunc);

            result.Images = response.Objects;
            result.TotalCount = response.TotalCount;

            return result;
        }

        internal KalturaLabel AddLabel(int groupId, KalturaLabel label, long userId)
        {
            Func<LabelValue, GenericResponse<LabelValue>> addFunc = requestLabel => CatalogManager.Instance.AddLabel(groupId, requestLabel, userId);
            var result = ClientUtils.GetResponseFromWS(label, addFunc);

            return result;
        }

        internal KalturaLabel UpdateLabel(int groupId, KalturaLabel label, long userId)
        {
            Func<LabelValue, GenericResponse<LabelValue>> updateFunc = requestLabel => CatalogManager.Instance.UpdateLabel(groupId, requestLabel, userId);
            var result = ClientUtils.GetResponseFromWS(label, updateFunc);

            return result;
        }

        internal bool DeleteLabel(int groupId, long labelId, long userId)
        {
            Func<Status> deleteFunc = () => CatalogManager.Instance.DeleteLabel(groupId, labelId, userId);
            var result = ClientUtils.GetResponseStatusFromWS(deleteFunc);

            return result;
        }

        internal KalturaLabelListResponse SearchLabels(int groupId, IEnumerable<long> idIn, string labelEqual, string labelStartWith, KalturaEntityAttribute entityAttribute, int pageIndex, int pageSize)
        {
            Func<GenericListResponse<LabelValue>> searchFunc = () => CatalogManager.Instance.SearchLabels(groupId, idIn.ToArray(), labelEqual, labelStartWith, (EntityAttribute)entityAttribute, pageIndex, pageSize);
            var labels = ClientUtils.GetResponseListFromWS<KalturaLabel, LabelValue>(searchFunc);

            var result = new KalturaLabelListResponse
            {
                Labels = labels.Objects,
                TotalCount = labels.TotalCount
            };

            return result;
        }

        internal KalturaLineupChannelAssetListResponse GetLineup(long groupId, long regionId, UserSearchContext searchContext, int pageIndex, int pageSize)
        {
            Func<GenericResponse<LineupChannelAssetResponse>> getLineupAssets = () => LineupService.Instance.GetLineupChannelAssets(groupId, regionId, searchContext, pageIndex, pageSize);

            var response = ClientUtils.GetResponseFromWS<KalturaLineupChannelAssetListResponse, LineupChannelAssetResponse>(getLineupAssets);

            var result = new KalturaLineupChannelAssetListResponse(response);

            return result;
        }

        internal bool SendUpdatedNotification(long groupId, string userId, List<long> regionIds)
        {
            Status SendUpdatedNotificationCallback() => LineupService.Instance.SendUpdatedNotification(groupId, userId, regionIds);

            return ClientUtils.GetResponseStatusFromWS(SendUpdatedNotificationCallback);
        }

        internal KalturaSearchPriorityGroup AddSearchPriorityGroup(long groupId, KalturaSearchPriorityGroup searchPriorityGroup, long userId)
        {
            Func<SearchPriorityGroup, GenericResponse<SearchPriorityGroup>> addFunc = request => _searchPriorityGroupManager.AddSearchPriorityGroup(groupId, request, userId);
            var result = ClientUtils.GetResponseFromWS(searchPriorityGroup, addFunc);

            return result;
        }

        internal KalturaSearchPriorityGroup UpdateSearchPriorityGroup(long groupId, KalturaSearchPriorityGroup searchPriorityGroup)
        {
            Func<SearchPriorityGroup, GenericResponse<SearchPriorityGroup>> updateFunc = request => _searchPriorityGroupManager.UpdateSearchPriorityGroup(groupId, request);
            var result = ClientUtils.GetResponseFromWS(searchPriorityGroup, updateFunc);

            return result;
        }

        internal bool DeleteSearchPriorityGroup(long groupId, long searchPriorityGroupId, long updaterId)
        {
            Func<Status> deleteFunc = () => _searchPriorityGroupManager.DeleteSearchPriorityGroup(groupId, searchPriorityGroupId, updaterId);
            var result = ClientUtils.GetResponseStatusFromWS(deleteFunc);

            return result;
        }

        internal KalturaSearchPriorityGroupListResponse ListSearchPriorityGroups(long groupId, SearchPriorityGroupQuery query)
        {
            Func<GenericListResponse<SearchPriorityGroup>> listFunc = () => _searchPriorityGroupManager.ListSearchPriorityGroups(groupId, query);
            var response = ClientUtils.GetResponseListFromWS<KalturaSearchPriorityGroup, SearchPriorityGroup>(listFunc);

            var result = new KalturaSearchPriorityGroupListResponse
            {
                Objects = response.Objects,
                TotalCount = response.TotalCount
            };

            return result;
        }

        internal KalturaSearchPriorityGroupOrderedIdsSet SetKalturaSearchPriorityGroupOrderedList(long groupId, KalturaSearchPriorityGroupOrderedIdsSet orderedList)
        {
            Func<SearchPriorityGroupOrderedIdsSet, GenericResponse<SearchPriorityGroupOrderedIdsSet>> setFunc = request => _searchPriorityGroupManager.SetKalturaSearchPriorityGroupOrderedList(groupId, request);
            var result = ClientUtils.GetResponseFromWS(orderedList, setFunc);

            return result;
        }

        internal KalturaSearchPriorityGroupOrderedIdsSet GetKalturaSearchPriorityGroupOrderedList(long groupId)
        {
            Func<GenericResponse<SearchPriorityGroupOrderedIdsSet>> getFunc = () => _searchPriorityGroupManager.GetKalturaSearchPriorityGroupOrderedList(groupId);
            var result = ClientUtils.GetResponseFromWS<KalturaSearchPriorityGroupOrderedIdsSet, SearchPriorityGroupOrderedIdsSet>(getFunc);

            return result;
        }

        internal KalturaAssetListResponse GroupRepresentativeList(GroupRepresentativesRequest request)
        {
            var contextData = KS.GetContextData();
            var userId = contextData.UserId ?? default;
            var partnerId = contextData.GroupId;
            var isAllowedToViewInactiveAssets = Utils.Utils.IsAllowedToViewInactiveAssets(
                partnerId,
                userId.ToString(),
                true);
            var response = GroupRepresentativesService.Instance.GetGroupRepresentativeList(request, ClientData);
            if (!response.IsOkStatusCode())
            {
                throw new ClientException(response.Status);
            }

            return GetAssetFromUnifiedSearchResponse(
                partnerId,
                response.Object.SearchResponse,
                response.Object.OriginalRequest,
                isAllowedToViewInactiveAssets);
        }

        private CatalogClientData ClientData => new CatalogClientData
        {
            Signature = Signature,
            SignString = SignString,
            ServerTime = getServerTime()
        };
    }
}
