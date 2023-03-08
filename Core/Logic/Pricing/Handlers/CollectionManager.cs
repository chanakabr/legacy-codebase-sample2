using ApiObjects;
using ApiObjects.Base;
using ApiObjects.Pricing;
using ApiObjects.Pricing.Dto;
using ApiObjects.Response;
using Core.Api;
using Core.Catalog.CatalogManagement;
using Core.GroupManagers;
using Core.Pricing;
using DAL;
using Phx.Lib.Log;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;
using Tvinci.Core.DAL;

namespace ApiLogic.Pricing.Handlers
{
    public class CollectionManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private static readonly Lazy<CollectionManager> lazy = new Lazy<CollectionManager>(() =>
                            new CollectionManager(PricingDAL.Instance,
                                                  GroupSettingsManager.Instance,
                                                  UsageModuleManager.Instance,
                                                  PriceDetailsManager.Instance,
                                                  CatalogDAL.Instance,
                                                  Core.Pricing.Module.Instance,
                                                  api.Instance,
                                                  Core.Catalog.CatalogManagement.FileManager.Instance),
                            LazyThreadSafetyMode.PublicationOnly);

        private readonly ICollectionRepository _repository;
        private readonly IGroupSettingsManager _groupSettingsManager;
        private readonly IUsageModuleManager _usageModuleManager;
        private readonly IPriceDetailsManager _priceDetailsManager;
        private readonly IChannelRepository _channelRepository;
        private readonly IPricingModule _pricingModule;
        private readonly IVirtualAssetManager _virtualAssetManager;
        private readonly IMediaFileTypeManager _fileManager;

        public static CollectionManager Instance => lazy.Value;

        public CollectionManager(ICollectionRepository repository,
                                 IGroupSettingsManager groupSettingsManager,
                                 IUsageModuleManager usageModuleManager,
                                 IPriceDetailsManager priceDetailsManager,
                                 IChannelRepository channelRepository,
                                 IPricingModule pricingModule,
                                 IVirtualAssetManager virtualAssetManager,
                                 IMediaFileTypeManager fileManager)
        {
            _repository = repository;
            _groupSettingsManager = groupSettingsManager;
            _usageModuleManager = usageModuleManager;
            _priceDetailsManager = priceDetailsManager;
            _channelRepository = channelRepository;
            _pricingModule = pricingModule;
            _virtualAssetManager = virtualAssetManager;
            _fileManager = fileManager;
        }

        public GenericResponse<CollectionInternal> Add(ContextData contextData, CollectionInternal collectionToInsert)
        {
            var response = new GenericResponse<CollectionInternal>();
            if (!_groupSettingsManager.IsOpc(contextData.GroupId))
            {
                response.SetStatus(eResponseStatus.AccountIsNotOpcSupported, "Account Is Not OPC Supported");
                return response;
            }

            Status status;

            // default active = true
            if (collectionToInsert.IsActive == null)
            {
                collectionToInsert.IsActive = true;
            }

            #region validate ExternalId - must be unique
            status = ValidateExternalId(contextData.GroupId, collectionToInsert.ExternalId);
            if (!status.IsOkStatusCode())
            {
                response.SetStatus(status);
                return response;
            }
            #endregion validate ExternalId

            #region validate usageModule
            status = ValidateUsageModule(contextData.GroupId, collectionToInsert.UsageModuleId.Value);
            if (!status.IsOkStatusCode())
            {
                response.SetStatus(status);
                return response;
            }
            #endregion validate usageModule

            #region validate PriceDetailsId
            status = ValidatePriceCode(contextData.GroupId, collectionToInsert.PriceDetailsId.Value);
            if (!status.IsOkStatusCode())
            {
                response.SetStatus(status);
                return response;
            }
            #endregion validate PriceDetailsId

            #region validate ChannelsIds
            if (collectionToInsert.ChannelsIds?.Count > 0)
            {
                status = ValidateChannels(contextData.GroupId, collectionToInsert.ChannelsIds);
                if (!status.IsOkStatusCode())
                {
                    response.SetStatus(status);
                    return response;
                }
            }
            #endregion validate ChannelsIds

            #region validate CouponGroups                
            if (collectionToInsert.CouponGroups?.Count > 0)
            {
                status = ValidateCouponGroups(contextData.GroupId, collectionToInsert.CouponGroups);
                if (!status.IsOkStatusCode())
                {
                    response.SetStatus(status);
                    return response;
                }
            }
            #endregion validate CouponGroups

            #region validate DiscountModuleId
            if (collectionToInsert.DiscountModuleId.HasValue)
            {
                status = ValidateDiscountModule(contextData.GroupId, collectionToInsert.DiscountModuleId.Value);
                if (!status.IsOkStatusCode())
                {
                    response.SetStatus(status);
                    return response;
                }
            }
            #endregion validate DiscountModuleId

            #region validate FileTypesIds
            if (collectionToInsert.FileTypesIds?.Count > 0)
            {
                status = ValidateFileTypesIds(contextData.GroupId, collectionToInsert.FileTypesIds);
                if (!status.IsOkStatusCode())
                {
                    response.SetStatus(status);
                    return response;
                }
            }
            #endregion validate FileTypesIds     

            bool isShopUser = false;
            if (collectionToInsert.AssetUserRuleId > 0)
            {
                var assetRuleResponse = Core.Api.Managers.AssetUserRuleManager.GetAssetUserRuleByRuleId(contextData.GroupId, collectionToInsert.AssetUserRuleId.Value);
                if (!assetRuleResponse.IsOkStatusCode())
                {
                    response.SetStatus(assetRuleResponse.Status);
                    return response;
                }
            }

            var shopId = Core.Api.Managers.AssetUserRuleManager.GetShopAssetUserRuleId(contextData.GroupId, contextData.UserId);
            if (shopId > 0)
            {
                isShopUser = true;
                collectionToInsert.AssetUserRuleId = shopId;
            }

            long id = _repository.AddCollection(contextData.GroupId, contextData.UserId.Value, collectionToInsert);
            if (id == 0)
            {
                log.Error($"Error while ADD Collection. contextData: {contextData.ToString()}.");
                return response;
            }

            _pricingModule.InvalidateCollection(contextData.GroupId);

            // Add VirtualAssetInfo for new collection 
            var virtualAssetInfo = new VirtualAssetInfo()
            {
                Type = ObjectVirtualAssetInfoType.Boxset,
                Id = id,
                Name = collectionToInsert.Names[0].m_sValue,
                UserId = contextData.UserId.Value,
                StartDate = collectionToInsert.StartDate,
                EndDate = collectionToInsert.EndDate,
                IsActive = collectionToInsert.IsActive,
                Description = collectionToInsert.Descriptions != null && collectionToInsert.Descriptions.Length > 0 ?
                                collectionToInsert.Descriptions[0].m_sValue : string.Empty
            };

            if (!isShopUser && collectionToInsert.AssetUserRuleId > 0)
            {
                virtualAssetInfo.AssetUserRuleId = collectionToInsert.AssetUserRuleId;
            }

            var virtualAssetInfoResponse = _virtualAssetManager.AddVirtualAsset(contextData.GroupId, virtualAssetInfo);
            if (virtualAssetInfoResponse.Status == VirtualAssetInfoStatus.Error)
            {
                log.Error($"Error while AddVirtualAsset - Collection (boxset) Id: {id} will delete ");
                //Delete(contextData, id);
                int Id = _repository.DeleteCollection(contextData.GroupId, id, contextData.UserId.Value);
                return response;
            }

            if (virtualAssetInfoResponse.Status == VirtualAssetInfoStatus.OK && virtualAssetInfoResponse.AssetId > 0)
            {
                _repository.UpdateCollectionVirtualAssetId(contextData.GroupId, id, virtualAssetInfoResponse.AssetId, contextData.UserId.Value);
            }

            collectionToInsert.Id = id;
            collectionToInsert.CouponGroups = null; // this empty object needed for mapping. don't remove it
            collectionToInsert.ExternalProductCodes = null; // this empty object needed for mapping. don't remove it
            response.Object = collectionToInsert;
            response.Status.Set(eResponseStatus.OK);

            return response;
        }

        public GenericResponse<Collection> GetCollection(int groupId, long collectionId, bool alsoUnActive = false)
        {
            GenericResponse<Collection> result = new GenericResponse<Collection>();

            result.Object = _pricingModule.GetCollectionData(groupId, collectionId.ToString(), string.Empty, string.Empty, string.Empty, alsoUnActive);
            if (result.Object != null)
            {
                result.SetStatus(eResponseStatus.OK);
            }
            else
            {
                result.SetStatus(eResponseStatus.CollectionNotExist);
            }

            return result;
        }

        public Status Delete(ContextData contextData, long id)
        {
            Status result = new Status();

            if (!_groupSettingsManager.IsOpc(contextData.GroupId))
            {
                result.Set(eResponseStatus.AccountIsNotOpcSupported, "Account Is Not OPC Supported");
                return result;
            }

            var collection = _pricingModule.GetCollectionData(contextData.GroupId, id.ToString(), string.Empty, string.Empty, string.Empty, false);
            if (collection == null)
            {
                result.Set(eResponseStatus.CollectionNotExist, $"Collection {id} does not exist");
                return result;
            }

            var shopId = Core.Api.Managers.AssetUserRuleManager.GetShopAssetUserRuleId(contextData.GroupId, contextData.UserId);
            if (shopId > 0)
            {
                long assetUserruleId = shopId;
                if (collection.AssetUserRuleId != assetUserruleId)
                {
                    result.Set(eResponseStatus.CollectionNotExist, $"Collection {id} does not exist");
                    return result;
                }
            }

            if (collection.VirtualAssetId > 0)
            {
                // Due to atomic action delete virtual asset before collection delete
                // Delete the virtual asset
                var vai = new VirtualAssetInfo()
                {
                    Type = ObjectVirtualAssetInfoType.Boxset,
                    Id = id,
                    UserId = contextData.UserId.Value
                };

                var response = _virtualAssetManager.DeleteVirtualAsset(contextData.GroupId, vai);
                if (response.Status == VirtualAssetInfoStatus.Error)
                {
                    log.Error($"Error while delete Collection virtual asset id {vai.ToString()}");
                    result.Set(eResponseStatus.Error, $"Failed to delete Collection {id}");
                    return result;
                }
            }

            int Id = _repository.DeleteCollection(contextData.GroupId, id, contextData.UserId.Value);
            if (Id == 0)
            {
                result.Set(eResponseStatus.Error);
            }
            else if (Id == -1)
            {
                result.Set(eResponseStatus.CollectionNotExist, $"Collection {id} does not exist");
            }
            else
            {
                _pricingModule.InvalidateCollection(contextData.GroupId, id);
                result.Set(eResponseStatus.OK);
            }

            return result;
        }

        public GenericListResponse<Collection> GetCollectionsData(ContextData contextData, string[] collCodes, string country, int pageIndex = 0, int pageSize = 30,
            bool shouldIgnorePaging = true, int? couponGroupIdEqual = null, bool inactiveAssets = false, CollectionOrderBy orderBy = CollectionOrderBy.None, long? assetUserRuleId = null)
        {
            GenericListResponse<Collection> response = new GenericListResponse<Collection>();
            List<long> collectionsIds = null;
            int totalResults = 0;
            int groupId = contextData.GroupId;

            if (!assetUserRuleId.HasValue)
            {
                var shopUserId = contextData.GetCallerUserId();
                var shopId = Core.Api.Managers.AssetUserRuleManager.GetShopAssetUserRuleId(contextData.GroupId, shopUserId);
                if (shopId > 0)
                {
                    assetUserRuleId = shopId;
                }
            }

            if (collCodes == null || collCodes.Length == 0)
            {
                collectionsIds = PricingCache.GetCollectionsIds(groupId, inactiveAssets, assetUserRuleId);
            }
            else
            {
                collectionsIds = collCodes.Select(x => long.Parse(x)).ToList();
                if (assetUserRuleId.HasValue && assetUserRuleId.Value > 0)
                {
                    collectionsIds = PricingCache.FilterCollectionsByAssetUserRuleId(groupId, collectionsIds, assetUserRuleId.Value);
                }
            }

            if (collectionsIds == null || collectionsIds.Count == 0)
            {
                response.SetStatus(eResponseStatus.OK, eResponseStatus.OK.ToString());
                return response;
            }

            if (orderBy == CollectionOrderBy.None)
            {
                if (!shouldIgnorePaging && collectionsIds.Count > 0)
                {
                    totalResults = collectionsIds.Count;
                    int startIndexOnList = pageIndex * pageSize;
                    int rangeToGetFromList = (startIndexOnList + pageSize) > collectionsIds.Count ? (collectionsIds.Count - startIndexOnList) > 0 ? (collectionsIds.Count - startIndexOnList) : 0 : pageSize;
                    if (rangeToGetFromList > 0)
                    {
                        collectionsIds = collectionsIds.Skip(startIndexOnList).Take(rangeToGetFromList).ToList();
                    }
                }

                response.Objects = _pricingModule.GetCollections(groupId, collectionsIds, country, contextData.Udid, contextData.Language, couponGroupIdEqual, inactiveAssets);

                if (response.Objects != null)
                {
                    response.TotalItems = totalResults >= pageSize && totalResults > response.Objects.Count ? totalResults : response.Objects.Count;
                }
            }
            else
            {
                // getting all collections
                response.Objects = _pricingModule.GetCollections(groupId, collectionsIds, country, contextData.Udid, contextData.Language, couponGroupIdEqual, inactiveAssets);
                if (response.Objects != null)
                {
                    // filter couponGroupIdEqual
                    if (couponGroupIdEqual.HasValue)
                    {
                        response.Objects = response.Objects?.Where(x => x.m_oCouponsGroup.m_sGroupCode == couponGroupIdEqual.Value.ToString()).ToList();
                    }

                    totalResults = response.Objects.Count;

                    switch (orderBy)
                    {
                        case CollectionOrderBy.NameAsc:
                            response.Objects = response.Objects.OrderBy(col => col.m_sObjectVirtualName).ToList();
                            break;
                        case CollectionOrderBy.NameDesc:
                            response.Objects = response.Objects.OrderByDescending(col => col.m_sObjectVirtualName).ToList();
                            break;
                        case CollectionOrderBy.UpdateDataAsc:
                            response.Objects = response.Objects.OrderBy(col => col.UpdateDate).ToList();
                            break;
                        case CollectionOrderBy.UpdateDataDesc:
                            response.Objects = response.Objects.OrderByDescending(col => col.UpdateDate).ToList();
                            break;
                    }

                    if (!shouldIgnorePaging && response.Objects.Count > 0)
                    {
                        int startIndexOnList = pageIndex * pageSize;
                        int rangeToGetFromList = (startIndexOnList + pageSize) > response.Objects.Count ? (response.Objects.Count - startIndexOnList) > 0 ? (response.Objects.Count - startIndexOnList) : 0 : pageSize;
                        if (rangeToGetFromList > 0)
                        {
                            response.Objects = response.Objects.Skip(startIndexOnList).Take(rangeToGetFromList).ToList();
                        }
                    }

                    response.TotalItems = totalResults > response.Objects.Count ? totalResults : response.Objects.Count;
                }
            }

            response.SetStatus(eResponseStatus.OK, eResponseStatus.OK.ToString());
            return response;
        }

        public GenericListResponse<Collection> GetCollectionsData(ContextData contextData, string country, int pageIndex, int pageSize, bool shouldIgnorePaging, int? couponGroupIdEqual = null,
            bool inactiveAssets = false, CollectionOrderBy orderBy = CollectionOrderBy.None)
        {
            return GetCollectionsData(contextData, null, country, pageIndex, pageSize, shouldIgnorePaging, couponGroupIdEqual, inactiveAssets, orderBy);
        }

        public IdsResponse GetCollectionIdsContainingMediaFile(int groupId, int mediaId, int mediaFileID)
        {
            BaseCollection t = null;
            Utils.GetBaseImpl(ref t, groupId);
            if (t != null)
            {
                return (new CollectionCacheWrapper(t)).GetCollectionIdsContainingMediaFile(mediaId, mediaFileID);
            }

            return null;
        }

        private Status ValidateExternalId(int groupId, string externalId, long collectionId = 0)
        {
            Status status = new Status(eResponseStatus.OK);

            long id = GetCollectionByExternalId(groupId, externalId);

            if (!string.IsNullOrEmpty(externalId) && id > 0 && id != collectionId)
            {
                status.Set(eResponseStatus.ExternalIdAlreadyExists, $"Collection external ID already exists {externalId}");
                return status;
            }

            return status;
        }

        private long GetCollectionByExternalId(int groupId, string externalId)
        {
            return _repository.GetCollectionByExternalId(groupId, externalId);
        }

        private Status ValidateUsageModule(int groupId, int usageModuleId)
        {
            Status status = new Status(eResponseStatus.OK);

            var usageModuleRes = _usageModuleManager.GetUsageModuleById(groupId, usageModuleId);

            if (!usageModuleRes.IsOkStatusCode() || usageModuleRes.Object == null)
            {
                status.Set(eResponseStatus.UsageModuleDoesNotExist, $"usageModule Code {usageModuleId} does not exist");
                return status;
            }

            return status;
        }

        private Status ValidatePriceCode(int groupId, long priceDetailsId)
        {
            Status status = new Status(eResponseStatus.OK);

            var priceDetailsRes = _priceDetailsManager.GetPriceDetailsById(groupId, priceDetailsId);

            if (!priceDetailsRes.IsOkStatusCode() || priceDetailsRes.Object == null)
            {
                status.Set(eResponseStatus.PriceDetailsDoesNotExist, $"PriceDetails {priceDetailsId} does not exist");
                return status;
            }

            return status;
        }

        private Status ValidateChannels(int groupId, List<long> channelsIds)
        {
            Status status = new Status(eResponseStatus.OK);

            foreach (var channelId in channelsIds)
            {
                if (!_channelRepository.IsChannelExists(groupId, channelId))
                {
                    status.Set(eResponseStatus.ChannelDoesNotExist, $"Could not find channel {channelId}");
                    return status;
                }
            }

            return status;
        }

        private Status ValidateCouponGroups(int groupId, List<SubscriptionCouponGroupDTO> couponGroups)
        {
            Status status = new Status(eResponseStatus.OK);

            foreach (var couponGroup in couponGroups)
            {
                if (long.TryParse(couponGroup.GroupCode, out long couponGroupId))
                {
                    var result = _pricingModule.GetCouponsGroup(groupId, couponGroupId);

                    if (!result.Status.IsOkStatusCode() || result.CouponsGroup == null)
                    {
                        status.Set(result.Status.Code, $"Could not find CouponGroup {couponGroupId}");
                        return status;
                    }
                }
            }

            return status;
        }

        private Status ValidateDiscountModule(int groupId, long discountModuleId)
        {
            Status status = new Status(eResponseStatus.OK);

            var discounts = _pricingModule.GetValidDiscounts(groupId);

            if (!discounts.HasObjects() || !discounts.Objects.Any(x => x.Id == discountModuleId))
            {
                status.Set(eResponseStatus.InvalidDiscountCode, $"DiscountModuleId is missing for group");
                return status;
            }

            return status;
        }

        public GenericResponse<CollectionInternal> Update(ContextData contextData, CollectionInternal collectionToUpdate)
        {
            GenericResponse<CollectionInternal> response = new GenericResponse<CollectionInternal>();
            VirtualAssetInfo virtualAssetInfo = null;

            if (!_groupSettingsManager.IsOpc(contextData.GroupId))
            {
                response.SetStatus(eResponseStatus.AccountIsNotOpcSupported, "Account Is Not OPC Supported");
                return response;
            }

            Status status;

            var collection = _pricingModule.GetCollectionData(contextData.GroupId, collectionToUpdate.Id.ToString(), string.Empty, string.Empty, string.Empty, false);
            if (collection == null)
            {
                response.SetStatus(eResponseStatus.CollectionNotExist, $"Collection {collectionToUpdate.Id} does not exist");
                return response;
            }

            var shopId = Core.Api.Managers.AssetUserRuleManager.GetShopAssetUserRuleId(contextData.GroupId, contextData.UserId);
            if (shopId > 0)
            {
                if (collection.AssetUserRuleId != shopId)
                {
                    response.SetStatus(eResponseStatus.CollectionNotExist, $"Collection {collectionToUpdate.Id} does not exist");
                    return response;
                }
            }

            #region validate ExternalId - must be unique
            if (collectionToUpdate.ExternalId != string.Empty && collectionToUpdate.ExternalId != collection.m_ProductCode)
            {
                status = ValidateExternalId(contextData.GroupId, collectionToUpdate.ExternalId, collectionToUpdate.Id);
                if (!status.IsOkStatusCode())
                {
                    response.SetStatus(status);
                    return response;
                }
            }
            #endregion validate ExternalId

            #region validate usageModule
            if (collectionToUpdate.UsageModuleId.HasValue)
            {
                if (collection.m_oCollectionUsageModule == null || collection.m_oCollectionUsageModule.m_nObjectID != collectionToUpdate.UsageModuleId.Value)
                {
                    status = ValidateUsageModule(contextData.GroupId, collectionToUpdate.UsageModuleId.Value);
                    if (!status.IsOkStatusCode())
                    {
                        response.SetStatus(status);
                        return response;
                    }
                }
            }
            #endregion validate usageModule

            #region validate PriceDetailsId
            if (collectionToUpdate.PriceDetailsId.HasValue)
            {
                if (collection.m_oCollectionPriceCode == null || collection.m_oCollectionPriceCode.m_nObjectID != collectionToUpdate.PriceDetailsId.Value)
                {
                    status = ValidatePriceCode(contextData.GroupId, collectionToUpdate.PriceDetailsId.Value);
                    if (!status.IsOkStatusCode())
                    {
                        response.SetStatus(status);
                        return response;
                    }
                }
            }
            #endregion validate PriceDetailsId

            #region validate ChannelsIds
            if (collectionToUpdate.ChannelsIds != null)
            {
                status = ValidateChannelsForUpdate(contextData.GroupId, collectionToUpdate.ChannelsIds, collection.m_sCodes);
                if (!status.IsOkStatusCode())
                {
                    response.SetStatus(status);
                    return response;
                }
            }
            #endregion validate ChannelsIds

            #region validate CouponGroups                

            if (collectionToUpdate.CouponGroups != null)
            {
                status = ValidateCouponGroupsForUpdate(contextData.GroupId, collectionToUpdate.CouponGroups, collection.CouponsGroups);
                if (!status.IsOkStatusCode())
                {
                    response.SetStatus(status);
                    return response;
                }
            }

            #endregion validate CouponGroups                

            #region validate DiscountModuleId
            if (collectionToUpdate.DiscountModuleId.HasValue)
            {
                if (collection.m_oDiscountModule == null || collection.m_oDiscountModule.m_nObjectID != collectionToUpdate.DiscountModuleId.Value)
                {
                    status = ValidateDiscountModule(contextData.GroupId, collectionToUpdate.DiscountModuleId.Value);
                    if (!status.IsOkStatusCode())
                    {
                        response.SetStatus(status);
                        return response;
                    }
                }
            }
            #endregion validate DiscountModuleId            

            if (collectionToUpdate.Names != null)
            {
                status = ValidateNamesForUpdate(collectionToUpdate.Names);
                if (!status.IsOkStatusCode())
                {
                    response.SetStatus(status);
                    return response;
                }

                if (collectionToUpdate.Names[0].m_sValue != collection.m_sObjectVirtualName)
                {
                    virtualAssetInfo = new VirtualAssetInfo()
                    {
                        Type = ObjectVirtualAssetInfoType.Boxset,
                        Id = collectionToUpdate.Id,
                        Name = collectionToUpdate.Names[0].m_sValue,
                        UserId = contextData.UserId.Value
                    };
                }
            }

            var nullableEndDate = new DAL.NullableObj<DateTime?>(collectionToUpdate.EndDate, collectionToUpdate.IsNullablePropertyExists("EndDate"));
            var nullableStartDate = new DAL.NullableObj<DateTime?>(collectionToUpdate.StartDate, collectionToUpdate.IsNullablePropertyExists("StartDate"));

            #region validate dates
            status = ValidateDates(nullableEndDate, nullableStartDate, collection);
            if (!status.IsOkStatusCode())
            {
                response.SetStatus(status);
                return response;
            }
            #endregion

            #region validate FileTypesIds

            if (collectionToUpdate.FileTypesIds != null)
            {
                status = ValidateFileTypesForUpdate(contextData.GroupId, collectionToUpdate.FileTypesIds, collection.m_sFileTypes);
                if (!status.IsOkStatusCode())
                {
                    response.SetStatus(status);
                    return response;
                }
            }

            #endregion validate FileTypesIds

            // Due to atomic action update virtual asset before collection update
            long? virtualAssetId = null;
            if (virtualAssetInfo != null)
            {
                var virtualAssetInfoResponse = _virtualAssetManager.UpdateVirtualAsset(contextData.GroupId, virtualAssetInfo);
                if (virtualAssetInfoResponse.Status == VirtualAssetInfoStatus.Error)
                {
                    log.Error($"Error while update collection's virtualAsset. groupId: {contextData.GroupId}, collectionId: {collectionToUpdate.Id}, Name: {collectionToUpdate.Names[0].m_sValue} ");
                    if (virtualAssetInfoResponse.ResponseStatus != null)
                    {
                        response.SetStatus(virtualAssetInfoResponse.ResponseStatus);
                    }
                    else
                    {
                        response.SetStatus(eResponseStatus.Error, "Error while updating collection.");
                    }

                    return response;
                }
                else
                {
                    if (virtualAssetInfoResponse.AssetId > 0 && virtualAssetInfoResponse.AssetId != collection.VirtualAssetId)
                    {
                        virtualAssetId = virtualAssetInfoResponse.AssetId;
                    }
                }
            }

            collectionToUpdate.IsActive = !collectionToUpdate.IsActive.HasValue ? collection.IsActive : collectionToUpdate.IsActive;

            bool success = _repository.UpdateCollection(contextData.GroupId, contextData.UserId.Value, collectionToUpdate, nullableStartDate, nullableEndDate, virtualAssetId);
            if (success)
            {
                _pricingModule.InvalidateCollection(contextData.GroupId, collectionToUpdate.Id);
                collectionToUpdate.CouponGroups = null; // this empty object needed for mapping. don't remove it
                collectionToUpdate.ExternalProductCodes = null; // this empty object needed for mapping. don't remove it
                response.Object = collectionToUpdate;
                response.Status.Set(eResponseStatus.OK);
            }

            return response;
        }

        private Status ValidateChannelsForUpdate(int groupId, List<long> channelsIds, BundleCodeContainer[] codes)
        {
            Status status = new Status(eResponseStatus.OK);

            if (channelsIds.Count == 0)
            {
                return status;
            }
            else
            {
                List<long> channelsToValidate = channelsIds;
                // compare current channelsIds with updated list
                if (codes != null && codes.Length > 0)
                {
                    var currChannelsIds = codes.Select(x => long.Parse(x.m_sCode)).ToList();

                    // check if both lists contain the same items in the same order. in case they ar not, need to update
                    if (!channelsIds.SequenceEqual(currChannelsIds))
                    {
                        // need to validate new channels in the list
                        channelsToValidate = channelsIds.Except(currChannelsIds).ToList();
                    }
                }

                status = ValidateChannels(groupId, channelsToValidate);
                if (!status.IsOkStatusCode())
                {
                    return status;
                }

            }

            return status;
        }

        private Status ValidateCouponGroupsForUpdate(int groupId, List<SubscriptionCouponGroupDTO> couponGroupsDTOs, List<SubscriptionCouponGroup> couponsGroups)
        {
            Status status = new Status(eResponseStatus.OK);

            if (couponGroupsDTOs.Count == 0 || couponsGroups == null)
            {
                return status;
            }
            else
            {
                // compare current couponsGroups with updated list
                if (couponsGroups?.Count > 0)
                {
                    var currCouponsGroups = couponsGroups.Select(s => new SubscriptionCouponGroupDTO(s.m_sGroupCode, s.startDate, s.endDate)).ToList();

                    // check if both lists contain the same items in the same order. in case they ar not, need to update
                    if (!couponGroupsDTOs.SequenceEqual(currCouponsGroups))
                    {
                        // need to validate new channels in the list
                        var newChennelsInList = couponGroupsDTOs.Except(currCouponsGroups).ToList();
                        status = ValidateCouponGroups(groupId, newChennelsInList);
                        if (!status.IsOkStatusCode())
                        {
                            return status;
                        }
                    }
                }
            }

            return status;
        }

        private Status ValidateNamesForUpdate(LanguageContainer[] names)
        {
            if (names.Length == 0 || string.IsNullOrEmpty(names[0].m_sValue))
            {
                return new Status(eResponseStatus.NameRequired, "name cannot be empty");
            }

            return new Status(eResponseStatus.OK);
        }

        private Status ValidateDates(DAL.NullableObj<DateTime?> endDateToUpdate, DAL.NullableObj<DateTime?> startDateToUpdate, Collection collection)
        {
            DateTime? startDate = startDateToUpdate.Obj.HasValue ? startDateToUpdate.Obj.Value : collection.m_dStartDate;
            DateTime? endDate = endDateToUpdate.Obj.HasValue ? endDateToUpdate.Obj.Value : collection.m_dEndDate;

            if (startDate.HasValue && endDate.HasValue && startDate > endDate)
            {
                return new Status(eResponseStatus.StartDateShouldBeLessThanEndDate, "StartDate should be less than EndDate");
            }

            return new Status(eResponseStatus.OK);
        }

        public void HandleChannelUpdate(int groupId, long userId, long channelId)
        {
            List<long> collectionIds = _repository.GetCollectionsByChannelId(groupId, channelId);

            //delete all Collections_Channels ByChannel id
            _repository.DeleteCollectionsChannelsByChannel(groupId, userId, channelId);

            if (collectionIds?.Count > 0)
            {
                foreach (var collectionId in collectionIds)
                {
                    _pricingModule.InvalidateCollection(groupId, collectionId);
                }
            }
        }

        private Status ValidateFileTypesIds(int groupId, List<long> fileTypesIds)
        {
            Status status = new Status(eResponseStatus.OK);

            var res = _fileManager.GetMediaFileTypes(groupId);
            if (res.Objects == null || res.Objects.Count < 0)
            {
                status.Set(eResponseStatus.InvalidFileTypes, $"FileTypes are missing for group");
                return status;
            }

            List<long> groupFileTypeIds = res.Objects.Select(x => x.Id).ToList();

            foreach (var fileTypesId in fileTypesIds)
            {
                if (!groupFileTypeIds.Contains(fileTypesId))
                {
                    status.Set(eResponseStatus.InvalidFileType, $"FileType not valid {fileTypesId}");
                    return status;
                }
            }

            return status;
        }

        private Status ValidateFileTypesForUpdate(int groupId, List<long> fileTypesIds, int[] fileTypes)
        {
            Status status = new Status(eResponseStatus.OK);

            if (fileTypesIds.Count == 0 || fileTypes == null)
            {
                return status;
            }
            else
            {
                // compare current fileTypes with updated list
                if (fileTypes != null && fileTypes.Length > 0)
                {
                    var currFileTypeIds = fileTypes.Select(i => (long)i).ToList();

                    // check if both lists contain the same items in the same order. in case they ar not, need to update
                    if (!fileTypesIds.SequenceEqual(currFileTypeIds))
                    {
                        // need to validate new channels in the list
                        var newFileTypesInList = fileTypesIds.Except(currFileTypeIds).ToList();
                        status = ValidateFileTypesIds(groupId, newFileTypesInList);
                        if (!status.IsOkStatusCode())
                        {
                            return status;
                        }
                    }
                }
            }

            return status;
        }

        private GenericListResponse<long> FilterByVirtualAsset(ContextData contextData, List<long> collectionsIds)
        {
            GenericListResponse<long> response = new GenericListResponse<long>(Status.Ok, collectionsIds);
            try
            {
                int groupId = contextData.GroupId;

                if (_groupSettingsManager.IsOpc(groupId) && contextData.UserId.HasValue && contextData.UserId.Value > 0)
                {
                    var shopId = Core.Api.Managers.AssetUserRuleManager.GetShopAssetUserRuleId(groupId, contextData.UserId);
                    if (shopId > 0)
                    {
                        response = new GenericListResponse<long>();

                        AssetSearchDefinition assetSearchDefinition = new AssetSearchDefinition()
                        {
                            UserId = contextData.UserId.Value,
                            IsAllowedToViewInactiveAssets = true,
                            NoSegmentsFilter = true,
                            FilterEmpty = true
                        };

                        HashSet<long> ids = new HashSet<long>();
                        foreach (var tId in collectionsIds)
                        {
                            ids.Add(tId);
                        }

                        var filter = _virtualAssetManager.GetObjectVirtualAssetObjectIds(groupId, assetSearchDefinition, ObjectVirtualAssetInfoType.Boxset, ids);

                        if (filter.ResultStatus == ObjectVirtualAssetFilterStatus.Error)
                        {
                            response.SetStatus(filter.Status);
                            return response;
                        }

                        response.SetStatus((int)eResponseStatus.OK, "OK");
                        if (filter.ResultStatus == ObjectVirtualAssetFilterStatus.Results && filter.ObjectIds?.Count > 0)
                        {
                            response.Objects = filter.ObjectIds;
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }

            response.SetStatus((int)eResponseStatus.OK, "OK");
            return response;
        }
    }
}