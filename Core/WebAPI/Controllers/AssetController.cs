using ApiObjects.Response;
using ApiObjects.SearchObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ApiLogic.Api.Managers.Rule;
using ApiObjects;
using ApiObjects.SearchObjects.GroupRepresentatives;
using ApiObjects.User;
using AssetSelectionGrpcClientWrapper;
using TVinciShared;
using WebAPI.ClientManagers;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.API;
using WebAPI.Models.Catalog;
using WebAPI.Models.Catalog.GroupRepresentatives;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Models.General;
using WebAPI.Models.Upload;
using WebAPI.ModelsValidators;
using WebAPI.ObjectsConvertor.Extensions;
using WebAPI.ObjectsConvertor.GroupRepresentatives;
using WebAPI.ObjectsConvertor.Ordering;
using WebAPI.Utils;
using SearchAssetsFilter = WebAPI.InternalModels.SearchAssetsFilter;
using Core.Api.Managers;
using ApiObjects.Base;

namespace WebAPI.Controllers
{
    [Service("asset")]
    public class AssetController : IKalturaController
    {
        private static readonly IMediaFileFilter _mediaFileFilter = MediaFileFilter.Instance;
        
        private static readonly KalturaAssetListResponse EmptyList = new KalturaAssetListResponse();
        private static readonly KalturaAssetOrder[] DefaultOrder = { new KalturaAssetOrder { OrderBy = KalturaAssetOrderByType.RELEVANCY_DESC } };
        private static readonly BadRequestException InvalidSlotNumber = new BadRequestException(BadRequestException.ARGUMENT_MIN_VALUE_CROSSED, nameof(KalturaPersonalAssetSelectionFilter.SlotNumberEqual), 1);

        /// <summary>
        /// Returns media or EPG assets. Filters by media identifiers or by EPG internal or external identifier.
        /// </summary>
        /// <param name="filter">Filtering the assets request</param>
        /// <param name="order_by">Ordering the assets</param>
        /// <param name="pager">Paging the request</param>
        /// <param name="with">Additional data to return per asset, formatted as a comma-separated array. 
        /// Possible values: stats – add the AssetStats model to each asset. files – add the AssetFile model to each asset. images - add the Image model to each asset.</param>
        /// <remarks></remarks>
        [Action("listOldStandard")]
        [OldStandardAction("list")]
        [ApiAuthorize]
        [Obsolete]
        [Throws(WebAPI.Managers.Models.StatusCode.NotFound)]
        static public KalturaAssetInfoListResponse ListOldStandard(KalturaAssetInfoFilter filter, List<KalturaCatalogWithHolder> with = null, KalturaOrder? order_by = null,
            KalturaFilterPager pager = null)
        {
            KalturaAssetInfoListResponse response = null;

            int groupId = KS.GetFromRequest().GroupId;
            string udid = KSUtils.ExtractKSPayload().UDID;

            if (pager == null)
                pager = new KalturaFilterPager();

            if (with == null)
                with = new List<KalturaCatalogWithHolder>();

            if (filter.IDs == null || filter.IDs.Count == 0)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "KalturaAssetInfoFilter.IDs");
            }

            try
            {
                string userID = KS.GetFromRequest().UserId;
                string language = Utils.Utils.GetLanguageFromRequest();
                List<int> ids = null;

                switch (filter.ReferenceType)
                {
                    case KalturaCatalogReferenceBy.MEDIA:
                        {
                            try
                            {
                                ids = filter.IDs.Select(x => int.Parse(x.value)).ToList();
                            }
                            catch (Exception)
                            {
                                throw new BadRequestException(BadRequestException.MEDIA_IDS_MUST_BE_NUMERIC);
                            }


                            response = ClientsManager.CatalogClient().GetMediaByIds(groupId, userID, udid, language,
                                pager.GetRealPageIndex(), pager.PageSize, ids, with.Select(x => x.type).ToList());
                        }
                        break;
                    case KalturaCatalogReferenceBy.EPG_INTERNAL:
                        {
                            try
                            {
                                ids = filter.IDs.Select(x => int.Parse(x.value)).ToList();
                            }
                            catch (Exception)
                            {
                                throw new BadRequestException(BadRequestException.EPG_INTERNAL_IDS_MUST_BE_NUMERIC);
                            }

                            response = ClientsManager.CatalogClient().GetEPGByInternalIds(groupId, userID, (int)HouseholdUtils.GetHouseholdIDByKS(), udid, language,
                               pager.GetRealPageIndex(), pager.PageSize, ids, with.Select(x => x.type).ToList());

                            // if no response - return not found status 
                            if (response == null || response.Objects == null || response.Objects.Count == 0)
                            {
                                throw new NotFoundException(NotFoundException.OBJECT_NOT_FOUND, "Asset");
                            }

                        }
                        break;
                    case KalturaCatalogReferenceBy.EPG_EXTERNAL:
                        {
                            response = ClientsManager.CatalogClient().GetEPGByExternalIds(groupId, userID, (int)HouseholdUtils.GetHouseholdIDByKS(), udid, language,
                                  pager.GetRealPageIndex(), pager.PageSize, filter.IDs.Select(id => id.value).ToList(), with.Select(x => x.type).ToList());

                            // if no response - return not found status 
                            if (response == null || response.Objects == null || response.Objects.Count == 0)
                            {
                                throw new NotFoundException(NotFoundException.OBJECT_NOT_FOUND, "Asset");
                            }
                        }
                        break;
                    case KalturaCatalogReferenceBy.channel:
                        {
                            int channelID;
                            if (!int.TryParse(filter.IDs.First().value, out channelID))
                            {
                                throw new BadRequestException(BadRequestException.INVALID_ACTION_PARAMETER, "filter.ids");
                            }

                            var withList = with.Select(x => x.type).ToList();
                            response = ClientsManager.CatalogClient().GetChannelAssets(groupId, userID, (int)HouseholdUtils.GetHouseholdIDByKS(), udid, language,
                            pager.GetRealPageIndex(), pager.PageSize, withList, channelID, order_by, string.Empty, false);
                        }
                        break;
                }
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Returns media or EPG assets. Filters by media identifiers or by EPG internal or external identifier.
        /// </summary>
        /// <param name="filter">Filtering the assets request</param>
        /// <param name="pager">Paging the request</param>
        /// <remarks></remarks>
        [Action("list")]
        [ApiAuthorize(eKSValidation.Expiration)]
        [Throws(eResponseStatus.SyntaxError)]
        [Throws(eResponseStatus.BadSearchRequest)]
        [Throws(eResponseStatus.IndexMissing)]
        static public KalturaAssetListResponse List(KalturaAssetFilter filter = null, KalturaFilterPager pager = null)
        {
            KalturaAssetListResponse response = null;
            KalturaBaseResponseProfile responseProfile = Utils.Utils.GetResponseProfileFromRequest();
            var contextData = KS.GetContextData();

            // parameters validation
            if (pager == null)
                pager = new KalturaFilterPager();

            if (filter == null)
            {
                filter = new KalturaSearchAssetFilter();
            }
            else
            {
                filter.Validate();
            }

            try
            {
                response = filter.GetAssets(contextData, responseProfile, pager);

                if (response?.Objects?.Count > 0)
                {
                    _mediaFileFilter.FilterAssetFiles(response.Objects, contextData.GroupId, contextData.SessionCharacteristicKey);

                    var clientTag = OldStandardAttribute.getCurrentClientTag();
                    response.Objects.ForEach(asset => asset.Metas = ModifyAlias(contextData.GroupId, clientTag, asset));
                }

                CatalogUtils.HandleResponseProfile(responseProfile, response.Objects);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Returns assets deduplicated by asset metadata (or supported asset's property).
        /// </summary>
        /// <param name="filter">Filtering the assets request</param>
        /// <param name="groupBy">A metadata (or supported asset's property) to group by the assets</param>
        /// <param name="unmatchedItemsPolicy">Defines the policy to handle assets that don't have groupBy property</param>
        /// <param name="orderBy">A metadata or supported asset's property to sort by</param>
        /// <param name="selectionPolicy">A policy that implements a well defined parametric process to select an asset out of group</param>
        /// <param name="pager">Paging the request</param>
        [Action("groupRepresentativeList")]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        public static KalturaAssetListResponse GroupRepresentativeList(
            KalturaAssetGroupBy groupBy,
            KalturaUnmatchedItemsPolicy? unmatchedItemsPolicy,
            KalturaBaseAssetOrder orderBy = null,
            KalturaListGroupsRepresentativesFilter filter = null,
            KalturaRepresentativeSelectionPolicy selectionPolicy = null,
            KalturaFilterPager pager = null)
        {
            var contextData = KS.GetContextData();
            pager = pager ?? new KalturaFilterPager();
            var orderingParameters = orderBy != null
                ? new List<KalturaBaseAssetOrder> { orderBy }
                : null;
            var userId = contextData.UserId ?? default;
            var partnerId = contextData.GroupId;
            var group = GroupsManager.Instance.GetGroup(partnerId);
            var isAllowedToViewInactiveAssets = Utils.Utils.IsAllowedToViewInactiveAssets(
                partnerId,
                userId.ToString(),
                true);
            var ksqlFilter = FilterAsset.Instance.UpdateKsql(filter?.KSql, contextData.GroupId, contextData.SessionCharacteristicKey);
            var request = new GroupRepresentativesRequest
            {
                DomainId = contextData.DomainId ?? default,
                UserId = contextData.UserId ?? default,
                Udid = contextData.Udid,
                PartnerId = partnerId,
                PageIndex = pager.GetRealPageIndex(),
                PageSize = pager.PageSize.Value,
                LanguageId = Utils.Utils.GetLanguageId(partnerId, contextData.Language),
                Filter = ksqlFilter,
                UnmatchedItemsPolicy = GroupRepresentativesSelectionMapper.Instance.MapToUnmatchedItemsPolicy(unmatchedItemsPolicy),
                SelectionPolicy = GroupRepresentativesSelectionMapper.Instance.MapToRepresentativeSelectionPolicy(selectionPolicy),
                OrderingParameters = KalturaOrderMapper.Instance.MapParameters(orderingParameters, OrderBy.CREATE_DATE),
                GroupByValue = groupBy.GetValue(),
                IsAllowedToViewInactiveAssets = isAllowedToViewInactiveAssets,
                UseStartDate = group.UseStartDate,
                GetOnlyActiveAssets = group.GetOnlyActiveAssets,
                UserIp = Utils.Utils.GetClientIP()
            };

            var response = ClientsManager.CatalogClient().GroupRepresentativeList(request);
            if (response?.Objects?.Count > 0)
            {
                _mediaFileFilter.FilterAssetFiles(response.Objects, contextData.GroupId, contextData.SessionCharacteristicKey);

                var clientTag = OldStandardAttribute.getCurrentClientTag();
                response.Objects.ForEach(asset => asset.Metas = ModifyAlias(contextData.GroupId, clientTag, asset));
            }

            return response;
        }

        /// <summary>
        /// Returns recent selected assets
        /// </summary>
        /// <param name="filter">Filtering the assets request</param>
        /// <remarks></remarks>
        [Action("listPersonalSelection")]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [ApiAuthorize(eKSValidation.Expiration)]
        [Throws(eResponseStatus.SyntaxError)]
        [Throws(eResponseStatus.BadSearchRequest)]
        [Throws(eResponseStatus.IndexMissing)]
        static public KalturaAssetListResponse ListPersonalSelection(KalturaPersonalAssetSelectionFilter filter)
        {
            if (filter.SlotNumberEqual <= 0) throw InvalidSlotNumber;
            
            var contextData = KS.GetContextData();

            if (contextData.UserId == null || contextData.UserId.Value.IsAnonymous()) return EmptyList;
            
            var userAssets = AssetSelectionClientInstance.Get()
                .GetUserAssetSelections(contextData.GroupId, contextData.UserId.Value, filter.SlotNumberEqual);
            if (userAssets.Count == 0) return EmptyList;
            
            // copy-paste from KalturaSearchAssetFilter, but with SpecificAssets
            var userId = contextData.UserId.ToString();
            var specificAssets = userAssets.Select(_ => new KeyValuePair<eAssetTypes, long>(_.AssetType, _.AssetId)).ToList();
            var pageSize = userAssets.Count;
            var assetTypes = userAssets.Select(_ => _.AssetType).Distinct();
            var assetTypesKsql = KsqlBuilderOld.Or(assetTypes.Select(KsqlBuilderOld.AssetType));
            
            var searchAssetFilter = new SearchAssetsFilter
            {
                GroupId = contextData.GroupId,
                SiteGuid = userId,
                DomainId = (int)(contextData.DomainId ?? 0),
                Udid = contextData.Udid,
                Language = contextData.Language,
                PageIndex = 0,
                PageSize = pageSize,
                Filter = FilterAsset.Instance.UpdateKsql(assetTypesKsql, contextData.GroupId, contextData.SessionCharacteristicKey),
                AssetTypes = null,
                EpgChannelIds = null,
                ManagementData = contextData.ManagementData,
                GroupBy = null,
                IsAllowedToViewInactiveAssets = Utils.Utils.IsAllowedToViewInactiveAssets(contextData.GroupId, userId, true),
                IgnoreEndDate = false,
                GroupByType = GroupingOption.Omit,
                IsPersonalListSearch = false,
                UseFinal = false,
                ShouldApplyPriorityGroups = false,
                ResponseProfile = null,
                OrderingParameters = DefaultOrder,
                GroupByOrder = null,
                SpecificAssets = specificAssets
            };
            
            var response = ClientsManager.CatalogClient().SearchAssets(searchAssetFilter);
            
            if (response.Objects == null || response.Objects.Count == 0) return EmptyList;

            // asset-selection service returns assets in a specific order, so rearrange assets by this order  
            var idToAsset = response.Objects.ToDictionary(
                key => (key.Id.Value, AssetTypeMapper.ToEAssetType(key.Type)),
                value => value);

            var assetList = new List<KalturaAsset>();
            foreach (var userAsset in userAssets)
            {
                if (idToAsset.TryGetValue((userAsset.AssetId, userAsset.AssetType), out var asset))
                {
                    assetList.Add(asset);
                }
            }

            response.Objects = assetList;
            response.TotalCount = assetList.Count;
            
            //copy-paste from asset.list
            _mediaFileFilter.FilterAssetFiles(response.Objects, contextData.GroupId, contextData.SessionCharacteristicKey);

            var clientTag = OldStandardAttribute.getCurrentClientTag();
            response.Objects.ForEach(asset => asset.Metas = ModifyAlias(contextData.GroupId, clientTag, asset));

            return response;
        }

        /// <summary>
        /// Returns media or EPG asset by media / EPG internal or external identifier.
        /// Note: OPC accounts asset.get for internal identifier doesn't take under consideration personalized aspects neither shop limitations.
        /// </summary>
        /// <param name="id">Asset identifier</param>                
        /// <param name="assetReferenceType">Asset type</param>
        /// <remarks></remarks>
        [Action("get")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_ARGUMENTS)]
        [Throws(StatusCode.NotFound)]
        [Throws(StatusCode.ArgumentCannotBeEmpty)]
        [Throws(StatusCode.ArgumentMustBeNumeric)]
        [Throws(eResponseStatus.SyntaxError)]
        [Throws(eResponseStatus.BadSearchRequest)]
        [Throws(eResponseStatus.IndexMissing)]
        [Throws(eResponseStatus.AssetDoesNotExist)]
        [Throws(StatusCode.EnumValueNotSupported)]
        static public KalturaAsset Get(string id, KalturaAssetReferenceType assetReferenceType)
        {
            KalturaAsset asset = null;

            if (string.IsNullOrEmpty(id))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "id");
            }

            var contextData = KS.GetContextData();

            try
            {
                bool isAllowedToViewInactiveAssets = Utils.Utils.IsAllowedToViewInactiveAssets(contextData.GroupId, contextData.UserId.ToString(), true);

                switch (assetReferenceType)
                {
                    case KalturaAssetReferenceType.media:
                        long mediaId;
                        if (!long.TryParse(id, out mediaId))
                        {
                            throw new BadRequestException(BadRequestException.ARGUMENT_MUST_BE_NUMERIC, "id");
                        }

                        if (contextData.UserId.Value > 0)
                        {                            
                            var shopUserId = contextData.GetCallerUserId();
                            var shopId = AssetUserRuleManager.Instance.GetShopAssetUserRuleId(contextData.GroupId, shopUserId);
                            if (shopId > 0)
                            {
                                var searchAssetsFilter = new SearchAssetsFilter
                                {
                                    GroupId = contextData.GroupId,
                                    SiteGuid = contextData.UserId.ToString(),
                                    DomainId = (int)contextData.DomainId,
                                    Udid = contextData.Udid,
                                    Language = contextData.Language,
                                    PageIndex = 0,
                                    PageSize = 1,
                                    Filter = $"(and asset_type='media' media_id = '{id}')",
                                    AssetTypes = null,
                                    EpgChannelIds = null,
                                    ManagementData = false,
                                    GroupBy = null,
                                    IsAllowedToViewInactiveAssets = isAllowedToViewInactiveAssets,
                                    IgnoreEndDate = true,
                                    GroupByType = GroupingOption.Omit,
                                    IsPersonalListSearch = false,
                                    UseFinal = false,
                                    OrderingParameters = KalturaOrderAdapter.Instance.MapToOrderingList(KalturaAssetOrderBy.RELEVANCY_DESC),
                                    OriginalUserId = contextData.OriginalUserId
                                };

                                KalturaAssetListResponse assetListResponse = ClientsManager.CatalogClient().SearchAssets(searchAssetsFilter);
                                if (assetListResponse != null && assetListResponse.TotalCount == 1 && assetListResponse.Objects.Count == 1)
                                {
                                    return assetListResponse.Objects[0];
                                }
                                else
                                {
                                    throw new NotFoundException(NotFoundException.OBJECT_NOT_FOUND, "Asset");
                                }
                            }
                        }

                        asset = ClientsManager.CatalogClient().GetAsset
                            (contextData.GroupId, mediaId, assetReferenceType, contextData.UserId.ToString(), (int)contextData.DomainId, contextData.Udid, contextData.Language, isAllowedToViewInactiveAssets, true);
                        break;
                    case KalturaAssetReferenceType.epg_internal:
                        int epgId;
                        if (!int.TryParse(id, out epgId))
                        {
                            throw new BadRequestException(BadRequestException.ARGUMENT_MUST_BE_NUMERIC, "id");
                        }

                        if (Utils.Utils.DoesGroupUsesTemplates(contextData.GroupId))
                        {
                            asset = ClientsManager.CatalogClient().GetEpgAsset(contextData.GroupId, epgId, isAllowedToViewInactiveAssets);
                        }
                        else
                        {
                            var epgRes = ClientsManager.CatalogClient().GetEPGByInternalIds
                                (contextData, 0, 1, new List<int> { epgId }, KalturaAssetOrderBy.START_DATE_DESC);

                            // if no response - return not found status 
                            if (epgRes == null || epgRes.Objects == null || epgRes.Objects.Count == 0)
                            {
                                throw new NotFoundException(NotFoundException.OBJECT_NOT_FOUND, "Asset");
                            }

                            asset = epgRes.Objects.First();
                        }
                        break;

                    case KalturaAssetReferenceType.epg_external:
                        var epgExRes = ClientsManager.CatalogClient().GetEPGByExternalIds(contextData, 0, 1, new List<string> { id });

                        // if no response - return not found status 
                        if (epgExRes == null || epgExRes.Objects == null || epgExRes.Objects.Count == 0)
                        {
                            throw new NotFoundException(NotFoundException.OBJECT_NOT_FOUND, "Asset");
                        }

                        asset = epgExRes.Objects.First();
                        break;

                    case KalturaAssetReferenceType.npvr:
                        asset = GetRecordingAsset(long.Parse(id), contextData, isAllowedToViewInactiveAssets);
                        break;

                    default:
                        throw new BadRequestException(BadRequestException.ARGUMENT_ENUM_VALUE_NOT_SUPPORTED, "assetReferenceType");
                }
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            if (asset != null)
            {
                _mediaFileFilter.FilterAssetFiles(asset, contextData.GroupId, contextData.SessionCharacteristicKey);
                var clientTag = OldStandardAttribute.getCurrentClientTag();
                asset.Metas = ModifyAlias(contextData.GroupId, clientTag, asset);
            }

            return asset;
        }

        private static KalturaRecordingAsset GetRecordingAsset(long id, ContextData contextData, bool isAllowedToViewInactiveAssets)
        {
            var recording = RecordingController.Get(id);
            if (recording == null)
            {
                throw new NotFoundException(NotFoundException.OBJECT_NOT_FOUND, "Recoring");
            }
            else
            {
                int epgId = (int)recording.AssetId;

                if (Utils.Utils.DoesGroupUsesTemplates(contextData.GroupId))
                {
                    var epgAsset = ClientsManager.CatalogClient().GetEpgAsset(contextData.GroupId, epgId, isAllowedToViewInactiveAssets);
                    return new KalturaRecordingAsset(epgAsset)
                    {
                        RecordingId = id.ToString(),
                        RecordingType = recording.Type, 
                        ViewableUntilDate = recording.ViewableUntilDate.HasValue ? recording.ViewableUntilDate.Value : 0
                    };
                }
                else
                {
                    var epgRes = ClientsManager.CatalogClient().GetEPGByInternalIds
                        (contextData, 0, 1, new List<int> { epgId }, KalturaAssetOrderBy.START_DATE_DESC);

                    // if no response - return not found status 
                    if (epgRes == null || epgRes.Objects == null || epgRes.Objects.Count == 0)
                    {
                        throw new NotFoundException(NotFoundException.OBJECT_NOT_FOUND, "Recording");
                    }

                    var epgAsset = epgRes.Objects.First();
                    return new KalturaRecordingAsset((KalturaProgramAsset)epgAsset) { RecordingId = id.ToString(), RecordingType = recording.Type };
                }
            }
        }

        /// <summary>
        /// Returns media or EPG asset by media / EPG internal or external identifier
        /// </summary>
        /// <param name="id">Asset identifier</param>                
        /// <param name="type">Asset type</param>                
        /// <param name="with">Additional data to return per asset, formatted as a comma-separated array. 
        /// Possible values: stats – add the AssetStats model to each asset. files – add the AssetFile model to each asset. images - add the Image model to each asset.</param>
        /// <remarks></remarks>
        [Action("getOldStandard")]
        [OldStandardAction("get")]
        [ApiAuthorize]
        [Obsolete]
        [Throws(WebAPI.Managers.Models.StatusCode.NotFound)]
        static public KalturaAssetInfo GetOldStandard(string id, KalturaAssetReferenceType type, List<KalturaCatalogWithHolder> with = null)
        {
            KalturaAssetInfo response = null;

            int groupId = KS.GetFromRequest().GroupId;

            if (string.IsNullOrEmpty(id))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "id");
            }

            if (with == null)
                with = new List<KalturaCatalogWithHolder>();

            try
            {
                string userID = KS.GetFromRequest().UserId;
                string udid = KSUtils.ExtractKSPayload().UDID;
                string language = Utils.Utils.GetLanguageFromRequest();

                switch (type)
                {
                    case KalturaAssetReferenceType.media:
                        {
                            int mediaId;
                            if (!int.TryParse(id, out mediaId))
                            {
                                throw new BadRequestException(BadRequestException.ARGUMENT_MUST_BE_NUMERIC, "id");
                            }

                            var mediaRes = ClientsManager.CatalogClient().GetMediaByIds(groupId, userID, udid, language,
                                0, 1, new List<int>() { mediaId }, with.Select(x => x.type).ToList());

                            response = mediaRes.Objects.First();
                        }
                        break;
                    case KalturaAssetReferenceType.epg_internal:
                        {
                            int epgId;
                            if (!int.TryParse(id, out epgId))
                            {
                                throw new BadRequestException(BadRequestException.ARGUMENT_MUST_BE_NUMERIC, "id");
                            }

                            var epgRes = ClientsManager.CatalogClient().GetEPGByInternalIds(groupId, userID, (int)HouseholdUtils.GetHouseholdIDByKS(), udid, language,
                               0, 1, new List<int> { epgId }, with.Select(x => x.type).ToList());

                            // if no response - return not found status 
                            if (epgRes == null || epgRes.Objects == null || epgRes.Objects.Count == 0)
                            {
                                throw new NotFoundException(NotFoundException.OBJECT_NOT_FOUND, "Asset");
                            }

                            response = epgRes.Objects.First();
                        }
                        break;
                    case KalturaAssetReferenceType.epg_external:
                        {
                            var epgRes = ClientsManager.CatalogClient().GetEPGByExternalIds(groupId, userID, (int)HouseholdUtils.GetHouseholdIDByKS(), udid, language,
                              0, 1, new List<string> { id }, with.Select(x => x.type).ToList());

                            // if no response - return not found status 
                            if (epgRes == null || epgRes.Objects == null || epgRes.Objects.Count == 0)
                            {
                                throw new NotFoundException(NotFoundException.OBJECT_NOT_FOUND, "Asset");
                            }

                            response = epgRes.Objects.First();
                        }
                        break;
                    default:
                        break;
                }
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Unified search across – VOD: Movies, TV Series/episodes, EPG content.        
        /// </summary>
        /// <param name="filter_types">List of asset types to search within. 
        /// Possible values: 0 – EPG linear programs entries, any media type ID (according to media type IDs defined dynamically in the system).
        /// If omitted – all types should be included.</param>
        /// <param name="filter">      
        /// <![CDATA[
        /// Search assets using dynamic criteria. Provided collection of nested expressions with key, comparison operators, value, and logical conjunction.
        /// Possible keys: any Tag or Meta defined in the system and the following reserved keys: start_date, end_date. 
        /// epg_id, media_id - for specific asset IDs.
        /// geo_block - only valid value is "true": When enabled, only assets that are not restricted to the user by geo-block rules will return.
        /// parental_rules - only valid value is "true": When enabled, only assets that the user doesn't need to provide PIN code will return.
        /// user_interests - only valid value is "true". When enabled, only assets that the user defined as his interests (by tags and metas) will return.
        /// epg_channel_id – the channel identifier of the EPG program.
        /// entitled_assets - valid values: "free", "entitled", "not_entitled", "both". free - gets only free to watch assets. entitled - only those that the user is implicitly entitled to watch.
        /// asset_type - valid values: "media", "epg", "recording" or any number that represents media type in group.
        /// aufo_fill - only valid value is "true": When enabled, auto fill assets will also return.
        /// Comparison operators: for numerical fields =, >, >=, <, <=, : (in). 
        /// For alpha-numerical fields =, != (not), ~ (like), !~, ^ (any word starts with), ^= (phrase starts with), + (exists), !+ (not exists).
        /// Logical conjunction: and, or. 
        /// Search values are limited to 20 characters each for the next operators: ~, !~, ^, ^=
        /// (maximum length of entire filter is 4096 characters)]]></param>
        /// <param name="order_by">Required sort option to apply for the identified assets. If omitted – will use relevancy.
        /// Possible values: relevancy, a_to_z, z_to_a, views, ratings, votes, newest.</param>
        /// <param name="with"> Additional data to return per asset, formatted as a comma-separated array. 
        /// Possible values: stats – add the AssetStats model to each asset. files – add the AssetFile model to each asset. images - add the Image model to each asset.</param>
        /// <param name="pager">Page size and index</param>
        /// <param name="request_id">Current request identifier (used for paging)</param>
        /// <remarks>Possible status codes: Bad search request = 4002, Missing index = 4003, SyntaxError = 4004, InvalidSearchField = 4005</remarks>
        [Action("search")]
        [ApiAuthorize]
        [Obsolete]
        [SchemeArgument("filter", MaxLength = 4096)]
        [Throws(eResponseStatus.BadSearchRequest)]
        [Throws(eResponseStatus.IndexMissing)]
        [Throws(eResponseStatus.SyntaxError)]
        [Throws(eResponseStatus.InvalidSearchField)]
        static public KalturaAssetInfoListResponse Search(KalturaOrder? order_by, List<KalturaIntegerValue> filter_types = null, string filter = null,
            List<KalturaCatalogWithHolder> with = null, KalturaFilterPager pager = null, string request_id = null)
        {
            KalturaAssetInfoListResponse response = null;

            int groupId = KS.GetFromRequest().GroupId;
            string userID = KS.GetFromRequest().UserId;
            int domainId = (int)HouseholdUtils.GetHouseholdIDByKS();
            string udid = KSUtils.ExtractKSPayload().UDID;
            string language = Utils.Utils.GetLanguageFromRequest();

            if (pager == null)
                pager = new KalturaFilterPager();

            if (with == null)
                with = new List<KalturaCatalogWithHolder>();

            if (filter_types == null)
                filter_types = new List<KalturaIntegerValue>();

            try
            {
                // call client
                response = ClientsManager.CatalogClient().SearchAssets(groupId, userID, domainId, udid, language,
                pager.GetRealPageIndex(), pager.PageSize, filter, order_by, filter_types.Select(x => x.value).ToList(),
                request_id,
                with.Select(x => x.type).ToList(), false);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Cross asset types search optimized for autocomplete search use. Search is within the title only, “starts with”, consider white spaces. Maximum number of returned assets – 10, no paging.
        /// </summary>
        /// <param name="query">Search string to look for within the assets’ title only. Search is starts with. White spaces are not ignored. Limited to 20 characters</param>
        /// <param name="with">Additional data to return per asset, formatted as a comma-separated array</param>
        /// <param name="filter_types">List of asset types to search within.
        /// Possible values: 0 – EPG linear programs entries, any media type ID (according to media type IDs defined dynamically in the system). 
        /// If omitted – all types should be included. </param>
        /// <param name="order_by"> Required sort option to apply for the identified assets. If omitted – will use newest.</param>
        /// <param name="size"><![CDATA[Maximum number of assets to return.  Possible range 1 ≤ size ≥ 10. If omitted or not in range – default to 5]]></param>
        /// <remarks>Possible status codes: Missing index = 4003</remarks>
        [Action("autocomplete")]
        [ApiAuthorize]
        [Obsolete]
        [Throws(eResponseStatus.IndexMissing)]
        static public KalturaSlimAssetInfoWrapper Autocomplete(string query, List<KalturaCatalogWithHolder> with = null, List<KalturaIntegerValue> filter_types = null,
            KalturaOrder? order_by = null, int? size = null)
        {
            KalturaSlimAssetInfoWrapper response = null;

            int groupId = KS.GetFromRequest().GroupId;
            string userID = KS.GetFromRequest().UserId;
            string udid = KSUtils.ExtractKSPayload().UDID;

            // Size rules - according to spec.  10>=size>=1 is valid. default is 5.
            if (size == null || size > 10 || size < 1)
            {
                size = 5;
            }

            if (with == null)
                with = new List<KalturaCatalogWithHolder>();

            if (filter_types == null)
                filter_types = new List<KalturaIntegerValue>();

            string language = Utils.Utils.GetLanguageFromRequest();
            try
            {
                response = ClientsManager.CatalogClient().Autocomplete(groupId, userID, udid, language, size, query, order_by, filter_types.Select(x => x.value).ToList(), with.Select(x => x.type).ToList());
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Return list of media assets that are related to a provided asset ID (of type VOD). Returned assets can be within multi VOD asset types or be of same type as the provided asset. Response is ordered by relevancy. On-demand, per asset enrichment is supported. Maximum number of returned assets – 20, using paging <br />        
        /// </summary>        
        /// <param name="media_id">The ID of the asset for which to return related assets</param>
        /// <param name="filter_types">List of type of related assets to return. Possible values: 0 - for EPG ; any media type ID (according to media type IDs defined dynamically in the system). If omitted – return assets of same asset type as the provided asset type. </param>        
        /// <param name="pager">Paging filter - Page number to return. If omitted returns first page. Number of assets to return per page. Possible range 5 ≤ size ≥ 50. If omitted – 5 is used. Value greater than 50 will set to 50</param>
        /// <param name="filter">Valid KSQL expression. If provided – the filter is applied on the result set and further reduce it</param>
        /// <param name="with">Additional data to return per asset, formatted as a comma-separated array. 
        /// Possible values: stats – add the AssetStats model to each asset. files – add the AssetFile model to each asset. images - add the Image model to each asset.</param>
        /// <remarks></remarks>
        [Action("related")]
        [ApiAuthorize]
        [Obsolete]
        [SchemeArgument("id", MinInteger = 1)]
        static public KalturaAssetInfoListResponse Related(int media_id, string filter = null, KalturaFilterPager pager = null, List<KalturaIntegerValue> filter_types = null,
            List<KalturaCatalogWithHolder> with = null)
        {
            KalturaAssetInfoListResponse response = null;

            int groupId = KS.GetFromRequest().GroupId;

            if (pager == null)
                pager = new KalturaFilterPager();

            if (with == null)
                with = new List<KalturaCatalogWithHolder>();

            if (filter_types == null)
                filter_types = new List<KalturaIntegerValue>();

            try
            {
                string userID = KS.GetFromRequest().UserId;
                string udid = KSUtils.ExtractKSPayload().UDID;
                string language = Utils.Utils.GetLanguageFromRequest();

                response = ClientsManager.CatalogClient().GetRelatedMedia(groupId, userID, (int)HouseholdUtils.GetHouseholdIDByKS(), udid,
                    language, pager.GetRealPageIndex(), pager.PageSize, media_id, filter, filter_types.Select(x => x.value).ToList(), with.Select(x => x.type).ToList());
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Return list of assets that are related to a provided asset ID. Returned assets can be within multi asset types or be of same type as the provided asset. Support on-demand, per asset enrichment. Related assets are provided from the external source (e.g. external recommendation engine). Maximum number of returned assets – 20, using paging <br />        
        /// </summary>        
        /// <param name="asset_id">The ID of the asset for which to return related assets</param>
        /// <param name="filter_type_ids">The type of related assets to return. Possible values: ALL – include all asset types ; any media type ID (according to media type IDs defined dynamically in the system). If ommited = ALL.</param>        
        /// <param name="with">Additional data to return per asset, formatted as a comma-separated array. 
        /// Possible values: stats – add the AssetStats model to each asset. files – add the AssetFile model to each asset. images - add the Image model to each asset.</param>
        /// <param name="pager">Paging filter - Page number to return. If omitted returns first page. Number of assets to return per page. Possible range 5 ≤ size ≥ 20. If omitted – 5 is used. Value greater than 20 will set to 20</param>
        /// <param name="utc_offset">Client’s offset from UTC. Format: +/-HH:MM. Example (client located at NY - EST): “-05:00”. If provided – may be used to further fine tune the returned collection</param>        
        /// <param name="free_param">Suplimentry data that the client can provide the external recommnedation engine</param>        
        /// <remarks></remarks>
        [Action("relatedExternal")]
        [ApiAuthorize]
        [Obsolete]
        [SchemeArgument("asset_id", MinInteger = 1)]
        static public KalturaAssetInfoListResponse RelatedExternal(int asset_id, KalturaFilterPager pager = null, List<KalturaIntegerValue> filter_type_ids = null, int utc_offset = 0,
            List<KalturaCatalogWithHolder> with = null, string free_param = null)
        {
            KalturaAssetInfoListResponse response = null;

            int groupId = KS.GetFromRequest().GroupId;
            string language = Utils.Utils.GetLanguageFromRequest();

            if (with == null)
                with = new List<KalturaCatalogWithHolder>();

            if (filter_type_ids == null)
                filter_type_ids = new List<KalturaIntegerValue>();

            if (pager == null)
                pager = new KalturaFilterPager() { PageIndex = 1, PageSize = 5 };

            string udid = KSUtils.ExtractKSPayload(KS.GetFromRequest()).UDID;

            try
            {
                string userID = KS.GetFromRequest().UserId;

                response = ClientsManager.CatalogClient().GetRelatedMediaExternal(groupId, userID, (int)HouseholdUtils.GetHouseholdIDByKS(), udid,
                    language, pager.GetRealPageIndex(), pager.PageSize, asset_id, filter_type_ids.Select(x => x.value).ToList(), utc_offset, with.Select(x => x.type).ToList(), free_param);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Search for assets via external service (e.g. external recommendation engine). Search can return multi asset types. Support on-demand, per asset enrichment. Maximum number of returned assets – 100, using paging <br />        
        /// </summary>        
        /// <param name="query">Search string </param>
        /// <param name="filter_type_ids">Related media types list - possible values:
        /// any media type ID (according to media type IDs defined dynamically in the system).
        /// If omitted – all types should be included.</param>        
        /// <param name="with">Additional data to return per asset, formatted as a comma-separated array. 
        /// Possible values: stats – add the AssetStats model to each asset. files – add the AssetFile model to each asset. images - add the Image model to each asset.</param>
        /// <param name="pager">Paging filter - Page number to return. If omitted returns first page. Number of assets to return per page. Possible range 5 ≤ size ≥ 20. If omitted – 10 is used. Value greater than 20 will set to 20.</param>
        /// <param name="utc_offset">Client’s offset from UTC. Format: +/-HH:MM. Example (client located at NY - EST): “-05:00”. If provided – may be used to further fine tune the returned collection</param>  
        /// <remarks></remarks>
        [Action("searchExternal")]
        [ApiAuthorize]
        [Obsolete]
        static public KalturaAssetInfoListResponse searchExternal(string query, KalturaFilterPager pager = null, List<KalturaIntegerValue> filter_type_ids = null, int utc_offset = 0,
            List<KalturaCatalogWithHolder> with = null)
        {
            KalturaAssetInfoListResponse response = null;

            int groupId = KS.GetFromRequest().GroupId;

            if (with == null)
                with = new List<KalturaCatalogWithHolder>();

            if (filter_type_ids == null)
                filter_type_ids = new List<KalturaIntegerValue>();

            if (pager == null)
                pager = new KalturaFilterPager() { PageIndex = 1, PageSize = 5 };

            string udid = KSUtils.ExtractKSPayload(KS.GetFromRequest()).UDID;

            try
            {
                string userID = KS.GetFromRequest().UserId;
                string language = Utils.Utils.GetLanguageFromRequest();

                response = ClientsManager.CatalogClient().GetSearchMediaExternal(groupId, userID, (int)HouseholdUtils.GetHouseholdIDByKS(), udid,
                    language, pager.GetRealPageIndex(), pager.PageSize, query, filter_type_ids.Select(x => x.value).ToList(), utc_offset, with.Select(x => x.type).ToList());
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Returns assets that belong to a channel
        /// </summary>
        /// <param name="id">Channel identifier</param>
        /// <param name="order_by">Ordering the channel</param>
        /// <param name="pager">Paging the request</param>
        /// <param name="with">Additional data to return per asset, formatted as a comma-separated array. 
        /// Possible values: stats – add the AssetStats model to each asset. files – add the AssetFile model to each asset. images - add the Image model to each asset.</param>
        /// <param name="filter_query"> /// <![CDATA[
        /// Search assets using dynamic criteria. Provided collection of nested expressions with key, comparison operators, value, and logical conjunction.
        /// Possible keys: any Tag or Meta defined in the system and the following reserved keys: start_date, end_date. 
        /// epg_id, media_id - for specific asset IDs.
        /// geo_block - only valid value is "true": When enabled, only assets that are not restricted to the user by geo-block rules will return.
        /// parental_rules - only valid value is "true": When enabled, only assets that the user doesn't need to provide PIN code will return.
        /// user_interests - only valid value is "true". When enabled, only assets that the user defined as his interests (by tags and metas) will return.
        /// epg_channel_id – the channel identifier of the EPG program.
        /// entitled_assets - valid values: "free", "entitled", "not_entitled", "both", "entitledSubscriptions". free - gets only free to watch assets. entitled - only those that the user is implicitly entitled to watch. entitledSubscriptions - only those that the user is implicitly has subscription to watch.
        /// asset_type - valid values: "media", "epg", "recording" or any number that represents media type in group.
        /// Comparison operators: for numerical fields =, >, >=, <, <=, : (in). 
        /// For alpha-numerical fields =, != (not), ~ (like), !~, ^ (any word starts with), ^= (phrase starts with), + (exists), !+ (not exists).
        /// Logical conjunction: and, or. 
        /// Search values are limited to 20 characters each for the next operators: ~, !~, ^, ^=
        /// (maximum length of entire filter is 4096 characters)]]></param>
        /// <remarks>Possible status codes: 
        /// BadSearchRequest = 4002, IndexMissing = 4003, SyntaxError = 4004, InvalidSearchField = 4005, Channel does not exist = 4018
        /// </remarks>
        [Action("channel")]
        [ApiAuthorize]
        [Obsolete]
        [SchemeArgument("id", MinInteger = 1)]
        [Throws(eResponseStatus.BadSearchRequest)]
        [Throws(eResponseStatus.IndexMissing)]
        [Throws(eResponseStatus.SyntaxError)]
        [Throws(eResponseStatus.InvalidSearchField)]
        [Throws(eResponseStatus.ObjectNotExist)]
        static public KalturaAssetInfoListResponse Channel(int id, List<KalturaCatalogWithHolder> with = null, KalturaOrder? order_by = null,
            KalturaFilterPager pager = null, string filter_query = null)
        {
            KalturaAssetInfoListResponse response = null;

            int groupId = KS.GetFromRequest().GroupId;
            string udid = KSUtils.ExtractKSPayload().UDID;
            string language = Utils.Utils.GetLanguageFromRequest();

            if (pager == null)
                pager = new KalturaFilterPager();

            if (with == null)
                with = new List<KalturaCatalogWithHolder>();

            try
            {
                string userID = KS.GetFromRequest().UserId;

                var withList = with.Select(x => x.type).ToList();
                response = ClientsManager.CatalogClient().GetChannelAssets(groupId, userID, (int)HouseholdUtils.GetHouseholdIDByKS(), udid, language,
                    pager.GetRealPageIndex(), pager.PageSize, withList, id, order_by, filter_query, false);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }


        /// <summary>
        /// Returns assets as defined by an external channel (3rd party recommendations)
        /// </summary>
        /// <param name="id">External channel's identifier</param>
        /// <param name="order_by">Ordering the assets</param>
        /// <param name="pager">Paging the request</param>
        /// <param name="with">Additional data to return per asset, formatted as a comma-separated array. 
        /// Possible values: stats – add the AssetStats model to each asset. files – add the AssetFile model to each asset. images - add the Image model to each asset.</param>
        /// <param name="utc_offset">UTC offset for request's enrichment</param>
        /// <param name="free_param">Suplimentry data that the client can provide the external recommnedation engine</param>
        /// <remarks>Possible status codes: 
        /// External Channel reference type: ExternalChannelHasNoRecommendationEngine = 4014, AdapterAppFailure = 6012, AdapterUrlRequired = 5013,
        /// BadSearchRequest = 4002, IndexMissing = 4003, SyntaxError = 4004, InvalidSearchField = 4005, 
        /// RecommendationEngineNotExist = 4007, ExternalChannelNotExist = 4011</remarks>
        [Action("externalChannel")]
        [ApiAuthorize]
        [Obsolete]
        [SchemeArgument("id", MinInteger = 1)]
        [SchemeArgument("utc_offset", MinFloat = -12, MaxFloat = 12)]
        [Throws(eResponseStatus.ExternalChannelHasNoRecommendationEngine)]
        [Throws(eResponseStatus.AdapterAppFailure)]
        [Throws(eResponseStatus.AdapterUrlRequired)]
        [Throws(eResponseStatus.BadSearchRequest)]
        [Throws(eResponseStatus.IndexMissing)]
        [Throws(eResponseStatus.SyntaxError)]
        [Throws(eResponseStatus.InvalidSearchField)]
        [Throws(eResponseStatus.RecommendationEngineNotExist)]
        [Throws(eResponseStatus.ExternalChannelNotExist)]
        static public KalturaAssetInfoListResponse ExternalChannel(int id, List<KalturaCatalogWithHolder> with = null, KalturaOrder? order_by = null,
            KalturaFilterPager pager = null, float? utc_offset = null, string free_param = null)
        {
            KalturaAssetInfoListResponse response = null;

            int groupId = KS.GetFromRequest().GroupId;
            string udid = KSUtils.ExtractKSPayload().UDID;
            string language = Utils.Utils.GetLanguageFromRequest();

            if (pager == null)
                pager = new KalturaFilterPager();

            if (with == null)
                with = new List<KalturaCatalogWithHolder>();

            try
            {
                string userID = KS.GetFromRequest().UserId;

                var convertedWith = with.Select(x => x.type).ToList();

                string deviceType = System.Web.HttpContext.Current.Request.GetUserAgentString();
                string str_utc_offset = utc_offset.HasValue ? utc_offset.Value.ToString() : null;
                response = ClientsManager.CatalogClient().GetExternalChannelAssets(groupId, id.ToString(), userID, (int)HouseholdUtils.GetHouseholdIDByKS(), udid,
                    language, pager.GetRealPageIndex(), pager.PageSize, order_by, convertedWith, deviceType, str_utc_offset, free_param);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// This action delivers all data relevant for player
        /// </summary>
        /// <param name="assetId">Asset identifier</param>
        /// <param name="assetType">Asset type</param>
        /// <param name="contextDataParams">Parameters for the request</param>
        /// <param name="sourceType">Filter sources by type</param>
        [Action("getPlaybackContext")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [Throws(eResponseStatus.RecordingNotFound)]
        [Throws(eResponseStatus.ProgramDoesntExist)]
        [Throws(eResponseStatus.DeviceNotInDomain)]
        [Throws(eResponseStatus.RecordingStatusNotValid)]
        [Throws(eResponseStatus.AdapterAppFailure)]
        [Throws(eResponseStatus.AdapterNotExists)]
        [Throws(eResponseStatus.AdapterUrlRequired)]
        [Throws(eResponseStatus.UserSuspended)]
        [Throws(eResponseStatus.CatchUpBufferLimitation)]
        [Throws(eResponseStatus.ProgramCatchUpNotEnabled)]
        [Throws(eResponseStatus.AccountCatchUpNotEnabled)]
        [Throws(eResponseStatus.ServiceNotAllowed)]
        [Throws(eResponseStatus.NotAllowed)]
        [Throws(eResponseStatus.ProgramStartOverNotEnabled)]
        static public KalturaPlaybackContext GetPlaybackContext(string assetId, KalturaAssetType assetType, KalturaPlaybackContextOptions contextDataParams, string sourceType = null)
        {
            KalturaPlaybackContext response = null;

            KS ks = KS.GetFromRequest();

            contextDataParams.Validate(assetType, assetId);

            try
            {
                response = ClientsManager.ConditionalAccessClient().GetPlaybackContext(ks.GroupId, ks.UserId, KSUtils.ExtractKSPayload().UDID, assetId, assetType, contextDataParams, sourceType);

                if (response.Sources?.Count > 0)
                {
                    response.Sources = _mediaFileFilter.GetFilteredAssetFiles(
                        response.Sources,
                        ks.GroupId,
                        long.Parse(assetId),
                        assetType,
                        KSUtils.ExtractKSPayload(ks).SessionCharacteristicKey).ToList();
                    if (response.Sources == null || response.Sources.Count == 0)
                    {
                        response.Messages.Add(new KalturaAccessControlMessage
                        {
                            Code = eResponseStatus.NoFilesFound.ToString(),
                            Message = "No files found"
                        });
                        return response;
                    }
                    DrmUtils.BuildSourcesDrmData(assetId, assetType, contextDataParams, ks, ref response);

                    // Check and get PlaybackAdapter in case asset set rule and action.
                    KalturaPlaybackContext adapterResponse = PlaybackAdapterManager.GetPlaybackAdapterContext(ks.GroupId, ks.UserId, assetId, assetType,
                        KSUtils.ExtractKSPayload().UDID, Utils.Utils.GetClientIP(), response, contextDataParams);
                    if (adapterResponse != null)
                    {
                        response = adapterResponse;
                    }

                    //if sources left after building DRM data, build the manifest URL
                    if (response.Sources.Count > 0)
                    {
                        if (contextDataParams.UrlType != KalturaUrlType.DIRECT) // no need to built Url in case of urlType us direct
                        {
                            string baseUrl = WebAPI.Utils.Utils.GetCurrentBaseUrl();

                            StringBuilder url = null;
                            foreach (var source in response.Sources)
                            {
                                // check if is tokenized . if yes add base64 url
                                source.Url = ExtractUrl(source, baseUrl);
                                source.AltUrl = ExtractUrl(source, baseUrl, true);
                            }
                        }

                        if (response.Messages == null)
                        {
                            response.Messages = new List<KalturaAccessControlMessage>();
                        }

                        if (response.Messages.Count == 0)
                        {
                            response.Messages.Add(new KalturaAccessControlMessage()
                            {
                                Code = WebAPI.Managers.Models.StatusCode.OK.ToString(),
                                Message = WebAPI.Managers.Models.StatusCode.OK.ToString(),
                            });
                        }
                    }
                }

                string ExtractUrl(KalturaPlaybackSource source, string baseUrl, bool isAltUrl = false)
                {
                    StringBuilder url;
                    if (source.IsTokenized == true && !string.IsNullOrEmpty(!isAltUrl ? source.Url : source.AltUrl))
                    {
                        url = new StringBuilder(string.Format(
                            "{0}/api_v3/service/assetFile/action/playManifest/partnerId/{1}/assetId/{2}/assetType/{3}/assetFileId/{4}/contextType/{5}/tokenizedUrl/{6}",
                            baseUrl,
                            ks.GroupId,
                            assetId,
                            assetType,
                            source.Id,
                            contextDataParams.Context,
                            Utils.Utils.RemoveSlashesFromBase64Str(Convert.ToBase64String(Encoding.UTF8.GetBytes(source.Url)))));
                    }
                    else
                    {
                        url = new StringBuilder(string.Format(
                            "{0}/api_v3/service/assetFile/action/playManifest/partnerId/{1}/assetId/{2}/assetType/{3}/assetFileId/{4}/contextType/{5}/isAltUrl/{6}",
                            baseUrl,
                            ks.GroupId,
                            assetId,
                            assetType,
                            source.Id,
                            contextDataParams.Context,
                            isAltUrl));
                    }

                    if (!string.IsNullOrEmpty(ks.UserId) && ks.UserId != "0")
                    {
                        url.AppendFormat("/ks/{0}", ks);
                    }

                    return url.AppendFormat("/a{0}", source.FileExtention).ToString();
                }
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Returns a group-by result for media or EPG according to given filter. Lists values of each field and their respective count.
        /// </summary>
        /// <param name="filter">Filtering the assets request</param>
        /// <returns></returns>
        [Action("count")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [Throws(StatusCode.ArgumentCannotBeEmpty)]
        [Throws(eResponseStatus.SyntaxError)]
        [Throws(eResponseStatus.BadSearchRequest)]
        public static KalturaAssetCount Count(KalturaSearchAssetFilter filter = null)
        {
            KalturaAssetCount response = null;

            int groupId = KS.GetFromRequest().GroupId;
            string userID = KS.GetFromRequest().UserId;
            int domainId = (int)HouseholdUtils.GetHouseholdIDByKS();
            string udid = KSUtils.ExtractKSPayload().UDID;
            string language = Utils.Utils.GetLanguageFromRequest();

            if (filter == null)
            {
                filter = new KalturaSearchAssetFilter();
            }
            else
            {
                filter.Validate();
            }

            if (filter.GroupBy == null)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "groupBy");
            }

            var groupByValuesList = filter.GroupBy.Select(group => group.GetValue()).ToList();
            if (groupByValuesList == null || groupByValuesList.Count == 0)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "groupBy");
            }

            try
            {
                bool isAllowedToViewInactiveAssets = Utils.Utils.IsAllowedToViewInactiveAssets(groupId, userID, true);

                var searchAssetFilter = new SearchAssetsFilter()
                {
                    GroupId = groupId,
                    SiteGuid = userID,
                    DomainId = domainId,
                    Udid = udid,
                    Language = language,
                    Filter = filter.Ksql,
                    AssetTypes = filter.getTypeIn(),
                    EpgChannelIds = filter.getEpgChannelIdIn(),
                    GroupBy = groupByValuesList,
                    GroupByType = GenericExtensionMethods.ConvertEnumsById<KalturaGroupingOption, GroupingOption>(filter.GroupingOptionEqual, GroupingOption.Omit).Value,
                    IsAllowedToViewInactiveAssets = isAllowedToViewInactiveAssets,
                    GroupByOrder = filter.GroupByOrder,
                    OrderingParameters = filter.Orderings
                };

                response = ClientsManager.CatalogClient().GetAssetCount(searchAssetFilter);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Returns the data for ads control
        /// </summary>
        /// <param name="assetId">Asset identifier</param>
        /// <param name="assetType">Asset type</param>
        /// <param name="contextDataParams">Parameters for the request</param>
        [Action("getAdsContext")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [Throws(eResponseStatus.RecordingNotFound)]
        [Throws(eResponseStatus.ProgramDoesntExist)]
        [Throws(eResponseStatus.DeviceNotInDomain)]
        [Throws(eResponseStatus.RecordingStatusNotValid)]
        [Throws(eResponseStatus.ServiceNotAllowed)]
        [Throws(eResponseStatus.RecordingPlaybackNotAllowedForNonExistingEpgChannel)]
        [Throws(eResponseStatus.UserSuspended)]
        [Throws(eResponseStatus.UserWithNoDomain)]
        [Throws(eResponseStatus.UserNotInDomain)]
        [Throws(eResponseStatus.UserWithNoDomain)]
        [Throws(eResponseStatus.UserDoesNotExist)]
        static public KalturaAdsContext GetAdsContext(string assetId, KalturaAssetType assetType, KalturaPlaybackContextOptions contextDataParams)
        {
            KalturaAdsContext response = null;

            KS ks = KS.GetFromRequest();
            string userId = ks.UserId;

            contextDataParams.Validate(assetType, assetId);

            try
            {
                response = ClientsManager.ConditionalAccessClient().GetAdsContext(ks.GroupId, userId, KSUtils.ExtractKSPayload().UDID, assetId, assetType, contextDataParams);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Add a new asset.
        /// For metas of type bool-> use kalturaBoolValue, type number-> KalturaDoubleValue, type date -> KalturaLongValue, type string -> KalturaStringValue
        /// </summary>
        /// <param name="asset">Asset object</param>
        /// <returns></returns>
        [Action("add")]
        [ApiAuthorize]
        [Throws(eResponseStatus.AssetStructDoesNotExist)]
        [Throws(eResponseStatus.AssetExternalIdMustBeUnique)]
        [Throws(eResponseStatus.InvalidMetaType)]
        [Throws(eResponseStatus.InvalidValueSentForMeta)]
        [Throws(eResponseStatus.DeviceRuleDoesNotExistForGroup)]
        [Throws(eResponseStatus.GeoBlockRuleDoesNotExistForGroup)]
        [Throws(StatusCode.StartDateShouldBeLessThanEndDate)]
        [Throws(eResponseStatus.EPGSProgramDatesError)]
        [Throws(eResponseStatus.AssetDoesNotExist)]
        [Throws(eResponseStatus.ChannelDoesNotExist)]
        [Throws(eResponseStatus.NameRequired)]
        [Throws(eResponseStatus.MetaDoesNotExist)]
        [Throws(eResponseStatus.GroupDoesNotContainLanguage)]
        static public KalturaAsset Add(KalturaAsset asset)
        {
            KalturaAsset response = null;
            int groupId = KS.GetFromRequest().GroupId;
            long userId = Utils.Utils.GetUserIdFromKs();

            try
            {
                asset.ValidateForInsert();
                response = ClientsManager.CatalogClient().AddAsset(groupId, asset, userId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Delete an existing asset
        /// </summary>
        /// <param name="id">Asset Identifier</param>
        /// <param name="assetReferenceType">Type of asset</param>
        /// <returns></returns>
        [Action("delete")]
        [ApiAuthorize]
        [Throws(eResponseStatus.AssetDoesNotExist)]
        [Throws(eResponseStatus.ActionIsNotAllowed)]
        [Throws(eResponseStatus.CategoryVersionIsNotDraft)]
        [SchemeArgument("id", MinLong = 1)]
        [ValidationException(SchemeValidationType.ACTION_ARGUMENTS)]
        static public bool Delete(long id, KalturaAssetReferenceType assetReferenceType)
        {
            bool result = false;
            int groupId = KS.GetFromRequest().GroupId;
            long userId = Utils.Utils.GetUserIdFromKs();

            try
            {
                result = ClientsManager.CatalogClient().DeleteAsset(groupId, id, assetReferenceType, userId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return result;
        }

        /// <summary>
        /// update an existing asset.
        /// For metas of type bool-> use kalturaBoolValue, type number-> KalturaDoubleValue, type date -> KalturaLongValue, type string -> KalturaStringValue
        /// </summary>
        /// <param name="id">Asset Identifier</param>
        /// <param name="assetReferenceType">Type of asset</param>
        /// <param name="asset">Asset object</param>
        /// <returns></returns>
        [Action("update")]
        [ApiAuthorize]
        [Throws(eResponseStatus.AssetDoesNotExist)]
        [Throws(eResponseStatus.AssetExternalIdMustBeUnique)]
        [Throws(eResponseStatus.InvalidMetaType)]
        [Throws(eResponseStatus.InvalidValueSentForMeta)]
        [Throws(eResponseStatus.ActionIsNotAllowed)]
        [Throws(eResponseStatus.RelatedEntitiesExceedLimitation)]
        [Throws(eResponseStatus.StartDateShouldBeLessThanEndDate)]
        [Throws(eResponseStatus.MetaDoesNotExist)]
        [Throws(eResponseStatus.GroupDoesNotContainLanguage)]
        [Throws(eResponseStatus.AssetStructDoesNotExist)]
        [Throws(eResponseStatus.CategoryVersionIsNotDraft)]
        [SchemeArgument("id", MinLong = 1)]
        static public KalturaAsset Update(long id, KalturaAsset asset)
        {
            KalturaAsset response = null;
            int groupId = KS.GetFromRequest().GroupId;
            long userId = Utils.Utils.GetUserIdFromKs();

            try
            {
                asset.ValidateForUpdate();
                response = ClientsManager.CatalogClient().UpdateAsset(groupId, id, asset, userId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// remove metas and tags from asset
        /// </summary>
        /// <param name="id">Asset Identifier</param>
        /// <param name="assetReferenceType">Type of asset</param>
        /// <param name="idIn">comma separated ids of metas and tags</param>
        /// <returns></returns>
        [Action("removeMetasAndTags")]
        [ApiAuthorize]
        [Throws(eResponseStatus.AssetDoesNotExist)]
        [Throws(eResponseStatus.CanNotRemoveBasicMetaIds)]
        [Throws(StatusCode.NotFound)]
        [SchemeArgument("id", MinLong = 1)]
        [SchemeArgument("idIn", DynamicMinInt = 1)]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [ValidationException(SchemeValidationType.ACTION_ARGUMENTS)]
        static public bool RemoveMetasAndTags(long id, KalturaAssetReferenceType assetReferenceType, string idIn)
        {
            bool result = false;
            var contextData = KS.GetContextData();

            HashSet<long> topicIds = new HashSet<long>();
            if (string.IsNullOrEmpty(idIn))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "topicIds");
            }
            else
            {
                string[] stringValues = idIn.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string stringValue in stringValues)
                {
                    long value;
                    if (long.TryParse(stringValue, out value))
                    {
                        topicIds.Add(value);
                    }
                    else
                    {
                        throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "topicIdIn");
                    }
                }
            }

            try
            {
                switch (assetReferenceType)
                {
                    case KalturaAssetReferenceType.epg_external:
                        KalturaAssetListResponse epgRes = ClientsManager.CatalogClient().GetEPGByExternalIds(contextData,0, 1, new List<string> { id.ToString() });

                        // if no response - return not found status 
                        if (epgRes == null || epgRes.Objects == null || epgRes.Objects.Count == 0)
                        {
                            throw new NotFoundException(NotFoundException.OBJECT_NOT_FOUND, "Asset");
                        }

                        id = epgRes.Objects.First().Id.Value;
                        break;
                    case KalturaAssetReferenceType.media:
                    case KalturaAssetReferenceType.epg_internal:
                    default:
                        break;
                }

                result = ClientsManager.CatalogClient().RemoveTopicsFromAsset(contextData.GroupId, id, assetReferenceType, topicIds, contextData.UserId.Value);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return result;
        }

        /// <summary>
        /// Add new bulk upload batch job Conversion profile id can be specified in the API.
        /// </summary>
        /// <param name="fileData">fileData</param>
        /// <param name="bulkUploadJobData">bulkUploadJobData</param>
        /// <param name="bulkUploadAssetData">bulkUploadAssetData</param>
        /// <returns></returns>
        [Action("addFromBulkUpload")]
        [ApiAuthorize]
        [Throws(StatusCode.ArgumentCannotBeEmpty)]
        [Throws(eResponseStatus.FileDoesNotExists)]
        [Throws(eResponseStatus.FileAlreadyExists)]
        [Throws(eResponseStatus.ErrorSavingFile)]
        [Throws(eResponseStatus.FileIdNotInCorrectLength)]
        [Throws(eResponseStatus.InvalidFileType)]
        [Throws(eResponseStatus.EnqueueFailed)]
        [Throws(eResponseStatus.InvalidArgumentValue)]
        [Throws(eResponseStatus.BulkUploadDoesNotExist)]
        [Throws(eResponseStatus.AccountEpgIngestVersionDoesNotSupportBulk)]
        [Throws(eResponseStatus.FileExceededMaxSize)]
        [Throws(eResponseStatus.FileExtensionNotSupported)]
        [Throws(eResponseStatus.FileMimeDifferentThanExpected)]
        [Throws(eResponseStatus.DynamicListDoesNotExist)]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [ValidationException(SchemeValidationType.ACTION_ARGUMENTS)]

        public static KalturaBulkUpload AddFromBulkUpload(KalturaOTTFile fileData, KalturaBulkUploadJobData bulkUploadJobData, KalturaBulkUploadAssetData bulkUploadAssetData)
        {
            KalturaBulkUpload bulkUpload = null;

            int groupId = KS.GetFromRequest().GroupId;
            long userId = Utils.Utils.GetUserIdFromKs();

            try
            {
                if (fileData == null || (fileData.File == null && string.IsNullOrEmpty(fileData.path)))
                {
                    throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "fileData");
                }

                if (bulkUploadJobData == null)
                {
                    throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "bulkUploadJobData");
                }

                if (bulkUploadAssetData == null)
                {
                    throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "bulkUploadAssetData");
                }

                bulkUploadJobData.Validate(fileData);
                bulkUploadAssetData.Validate(groupId);

                var assetType = bulkUploadAssetData.GetBulkUploadObjectType();

                bulkUpload =
                    ClientsManager.CatalogClient().AddBulkUpload(groupId, userId, assetType, bulkUploadJobData, bulkUploadAssetData, fileData);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return bulkUpload;
        }

        /// <summary>
        /// This action delivers all data relevant for player
        /// </summary>
        /// <param name="assetId">Asset identifier</param>
        /// <param name="assetType">Asset type</param>
        /// <param name="contextDataParams">Parameters for the request</param>
        /// <param name="sourceType">Filter sources by type</param>
        [Action("getPlaybackManifest")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [Throws(eResponseStatus.RecordingNotFound)]
        [Throws(eResponseStatus.RecordingStatusNotValid)]
        [Throws(eResponseStatus.RecordingPlaybackNotAllowedForNonExistingEpgChannel)]
        [Throws(eResponseStatus.ProgramDoesntExist)]
        [Throws(eResponseStatus.AdapterAppFailure)]
        [Throws(eResponseStatus.AdapterNotExists)]
        [Throws(eResponseStatus.AdapterUrlRequired)]
        [Throws(eResponseStatus.DeviceNotInDomain)]
        [Throws(eResponseStatus.NoFilesFound)]
        static public KalturaPlaybackContext GetPlaybackManifest(string assetId, KalturaAssetType assetType, KalturaPlaybackContextOptions contextDataParams, string sourceType = null)
        {
            KalturaPlaybackContext response = null;

            KS ks = KS.GetFromRequest();

            contextDataParams.Validate(assetType, assetId);

            try
            {
                string udid = KSUtils.ExtractKSPayload().UDID;

                response = ClientsManager.ConditionalAccessClient().GetPlaybackContext(ks.GroupId, ks.UserId, udid, assetId, assetType, contextDataParams, sourceType, true);

                if (response.Sources?.Count > 0)
                {
                    response.Sources = _mediaFileFilter.GetFilteredAssetFiles(
                        response.Sources,
                        ks.GroupId,
                        long.Parse(assetId),
                        assetType,
                        KSUtils.ExtractKSPayload(ks).SessionCharacteristicKey).ToList();
                    if (response.Sources == null || response.Sources.Count == 0)
                    {
                        response.Messages.Add(new KalturaAccessControlMessage
                        {
                            Code = eResponseStatus.NoFilesFound.ToString(),
                            Message = "No files found"
                        });
                        return response;
                    }
                    
                    KalturaPlaybackContext adapterResponse = PlaybackAdapterManager.GetPlaybackAdapterManifest(ks.GroupId, assetId, assetType, response, contextDataParams, ks.UserId, udid, Utils.Utils.GetClientIP());
                    if (adapterResponse != null)
                    {
                        response = adapterResponse;
                    }

                    if (response.Messages == null)
                    {
                        response.Messages = new List<KalturaAccessControlMessage>();
                    }

                    if (response.Messages.Count == 0)
                    {
                        response.Messages.Add(new KalturaAccessControlMessage()
                        {
                            Code = StatusCode.OK.ToString(),
                            Message = StatusCode.OK.ToString(),
                        });
                    }
                }
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        private static SerializableDictionary<string, KalturaValue> ModifyAlias(int groupId, string clientTag, KalturaAsset asset)
        {
            if (asset.Metas == null || !asset.Metas.Any())
                return asset.Metas;

            var manager = Core.Catalog.CatalogManagement.CatalogManager.Instance;

            if (!manager.IsGroupUsingAliases(groupId))
                return asset.Metas;

            var aliasHelper = ApiLogic.Api.Managers.CustomFieldsPartnerConfigManager.Instance;
            var modifier = ApiLogic.Catalog.CatalogManagement.Managers.AssetMetaModifier.Instance;

            var _metas = modifier.ReplaceWithAlias(groupId, clientTag, asset.Type.Value, asset.Metas, aliasHelper);
            var _t = new SerializableDictionary<string, KalturaValue>();
            _t.TryAddRange(_metas);
            return _t;
        }
    }
}
