using ApiObjects;
using ApiObjects.Base;
using ApiObjects.Pricing;
using ApiObjects.Pricing.Dto;
using ApiObjects.Response;
using Core.Api;
using Core.Catalog.CatalogManagement;
using Core.Domains;
using Core.GroupManagers;
using Core.Pricing;
using DAL;
using Phx.Lib.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Tvinci.Core.DAL;

namespace ApiLogic.Pricing.Handlers
{
    public class SubscriptionManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private static readonly Lazy<SubscriptionManager> lazy = new Lazy<SubscriptionManager>(() =>
                            new SubscriptionManager(PricingDAL.Instance,
                                                    PricingDAL.Instance,
                                                    CatalogDAL.Instance,
                                                    Core.Pricing.Module.Instance,
                                                    Core.Catalog.CatalogManagement.FileManager.Instance,
                                                    Core.Domains.Module.Instance,
                                                    PricePlanManager.Instance,
                                                    api.Instance,
                                                    PartnerPremiumServicesManager.Instance,
                                                    GroupSettingsManager.Instance),
                            LazyThreadSafetyMode.PublicationOnly);

        public static SubscriptionManager Instance => lazy.Value;

        private readonly ISubscriptionManagerRepository _repository;
        private readonly IModuleManagerRepository _moduleManagerRepository;
        private readonly IChannelRepository _channelRepository;
        private readonly IPricingModule _pricingModule;
        private readonly IMediaFileTypeManager _fileManager;
        private readonly IDomainModule _domainModule;
        private readonly IPricePlanManager _pricePlanManager;
        private readonly IVirtualAssetManager _virtualAssetManager;
        private readonly IPartnerPremiumServicesManager _partnerPremiumServicesManager;
        private readonly IGroupSettingsManager _groupSettingsManager;

        public SubscriptionManager(ISubscriptionManagerRepository repository,
                                IModuleManagerRepository moduleManagerRepository,
                                IChannelRepository channelRepository,
                                IPricingModule pricingModule,
                                IMediaFileTypeManager fileManager,
                                IDomainModule domainModule,
                                IPricePlanManager pricePlanManager,
                                IVirtualAssetManager virtualAssetManager,
                                IPartnerPremiumServicesManager partnerPremiumServicesManager,
                                IGroupSettingsManager groupSettingsManager)
        {
            _repository = repository;
            _moduleManagerRepository = moduleManagerRepository;
            _channelRepository = channelRepository;
            _pricingModule = pricingModule;
            _fileManager = fileManager;
            _domainModule = domainModule;
            _pricePlanManager = pricePlanManager;
            _virtualAssetManager = virtualAssetManager;
            _partnerPremiumServicesManager = partnerPremiumServicesManager;
            _groupSettingsManager = groupSettingsManager;
        }

        public List<int> GetSubscriptionIDsContainingMediaFile(int groupId, int mediaFileIdEqual)
        {
            BaseSubscription t = null;
            Utils.GetBaseImpl(ref t, groupId);
            if (t != null)
            {
                IdsResponse result = (new SubscriptionCacheWrapper(t)).GetSubscriptionIDsContainingMediaFile(0, mediaFileIdEqual);
                return result?.Ids?.Count > 0 ? result.Ids : null;
            }
            else
            {
                return null;
            }
        }

        public GenericListResponse<Subscription> GetSubscriptionsData(int groupId, HashSet<long> subscriptionsIds, string udid, string languageCode, 
            SubscriptionOrderBy orderBy,
            AssetSearchDefinition assetSearchDefinition, int pageIndex, int? pageSize = 30, int? couponGroupIdEqual = null, bool getAlsoInActive = false, 
            HashSet<SubscriptionType> subscriptionTypes = null)
        {
            SubscriptionsResponse response = _pricingModule.GetSubscriptions(groupId, subscriptionsIds, string.Empty, languageCode, udid, assetSearchDefinition, orderBy,
                pageIndex, pageSize.Value, false, couponGroupIdEqual, getAlsoInActive, null, null, null, subscriptionTypes);

            GenericListResponse<Subscription> result = new GenericListResponse<Subscription>();
            if (response.Subscriptions != null)
            {
                result.TotalItems = response.TotalItems == 0 ? response.Subscriptions.Length : response.TotalItems;
                result.Objects = response.Subscriptions.ToList();
            }
            result.SetStatus(response.Status);


            return result;
        }

        public GenericListResponse<Subscription> GetSubscriptionsByProductCodeList(int groupId, List<string> productCodes, SubscriptionOrderBy orderBy = SubscriptionOrderBy.StartDateAsc)
        {
            SubscriptionsResponse response = _pricingModule.GetSubscriptionsByProductCodes(groupId, productCodes, orderBy);

            GenericListResponse<Subscription> result = new GenericListResponse<Subscription>();
            if (response.Subscriptions != null)
            {
                result.TotalItems = response.Subscriptions.Length;
                result.Objects = response.Subscriptions.ToList();
            }
            result.SetStatus(response.Status);

            return result;
        }

        public GenericListResponse<Subscription> GetSubscriptionsData(int groupId, string udid, string language, SubscriptionOrderBy orderBy, int pageIndex, int? pageSize, 
            int? couponGroupIdEqual = null, bool getAlsoInActive = false, long? previewModuleIdEqual = null, long? pricePlanIdEqual = null, long? channelIdEqual = null,
            HashSet<SubscriptionType> subscriptionTypes = null, string nameContains = null)
        {
            var response = _pricingModule.GetSubscriptions(groupId, language, udid, orderBy, pageIndex, pageSize.Value, false, getAlsoInActive, couponGroupIdEqual, 
                previewModuleIdEqual, pricePlanIdEqual, channelIdEqual, subscriptionTypes, nameContains);

            GenericListResponse<Subscription> result = new GenericListResponse<Subscription>();
            if (response.Subscriptions != null)
            {
                result.TotalItems = response.TotalItems;
                result.Objects = response.Subscriptions.ToList();
            }
            result.SetStatus(response.Status);
            return result;
        }

        public GenericResponse<Subscription> GetSubscription(int groupId, long subscriptionId)
        {
            GenericResponse<Subscription> result = new GenericResponse<Subscription>();

            result.Object = _pricingModule.GetSubscriptionData(groupId, subscriptionId.ToString(), string.Empty, string.Empty, string.Empty, false);
            if (result.Object != null)
            {
                result.SetStatus(eResponseStatus.OK);
            }

            return result;
        }

        public GenericResponse<SubscriptionInternal> Add(ContextData contextData, SubscriptionInternal subscriptionToInsert)
        {
            var response = new GenericResponse<SubscriptionInternal>();
            if (!_groupSettingsManager.IsOpc(contextData.GroupId))
            {
                response.SetStatus(eResponseStatus.AccountIsNotOpcSupported, "Account Is Not OPC Supported");
                return response;
            }

            Status status;
            try
            {
                #region validate ChannelsIds
                if (subscriptionToInsert.ChannelsIds?.Count > 0)
                {
                    status = ValidateChannels(contextData.GroupId, subscriptionToInsert.ChannelsIds);
                    if (!status.IsOkStatusCode())
                    {
                        response.SetStatus(status);
                        return response;
                    }
                }
                #endregion validate ChannelsIds

                #region validate CouponGroups                
                if (subscriptionToInsert.CouponGroups?.Count > 0)
                {
                    status = ValidateCouponGroups(contextData.GroupId, subscriptionToInsert.CouponGroups);
                    if (!status.IsOkStatusCode())
                    {
                        response.SetStatus(status);
                        return response;
                    }
                }
                #endregion validate CouponGroups

                #region validate ExternalId - must be unique
                status = ValidateExternalId(contextData.GroupId, subscriptionToInsert.ExternalId);
                if (!status.IsOkStatusCode())
                {
                    response.SetStatus(status);
                    return response;
                }
                #endregion validate ExternalId

                #region validate FileTypesIds
                if (subscriptionToInsert.FileTypesIds?.Count > 0)
                {
                    status = ValidateFileTypesIds(contextData.GroupId, subscriptionToInsert.FileTypesIds);
                    if (!status.IsOkStatusCode())
                    {
                        response.SetStatus(status);
                        return response;
                    }
                }
                #endregion validate FileTypesIds     

                #region validate HouseholdLimitationsId     
                if (subscriptionToInsert.HouseholdLimitationsId.HasValue)
                {
                    status = ValidateDLM(contextData.GroupId, subscriptionToInsert.HouseholdLimitationsId.Value);
                    if (!status.IsOkStatusCode())
                    {
                        response.SetStatus(status);
                        return response;
                    }
                }
                #endregion validate HouseholdLimitationsId     

                #region validate InternalDiscountModuleId
                if (subscriptionToInsert.InternalDiscountModuleId.HasValue)
                {
                    status = ValidateDiscountModule(contextData.GroupId, subscriptionToInsert.InternalDiscountModuleId.Value);
                    if (!status.IsOkStatusCode())
                    {
                        response.SetStatus(status);
                        return response;
                    }
                }
                #endregion validate InternalDiscountModuleId

                #region validate PremiumServices
                if (subscriptionToInsert.PremiumServices != null && subscriptionToInsert.PremiumServices.Length > 0)
                {
                    if (subscriptionToInsert.PremiumServices != null && subscriptionToInsert.PremiumServices.Length > 0)
                    {
                        status = ValidatePremiumServices(subscriptionToInsert.PremiumServices.Select(x => x.ID).ToList());
                        if (!status.IsOkStatusCode())
                        {
                            response.SetStatus(status);
                            return response;
                        }
                    }
                }
                #endregion validate PremiumServices

                long? basePricePlanId = null;
                long? basePriceCodeId = null;
                bool isRecurring = false;
                long? extDiscountId = null;

                #region PricePlanIds
                if (subscriptionToInsert.PricePlanIds?.Count > 0)
                {
                    Status pricePlanStatus = ValidatePricePlan(subscriptionToInsert.PricePlanIds, contextData.GroupId, out basePricePlanId, out basePriceCodeId, out isRecurring, out extDiscountId);
                    if (pricePlanStatus.Code != (int)eResponseStatus.OK)
                    {
                        response.SetStatus(pricePlanStatus);
                        return response;
                    }
                }
                #endregion PricePlanIds

                subscriptionToInsert.ProrityInOrder = subscriptionToInsert.ProrityInOrder.HasValue ? subscriptionToInsert.ProrityInOrder : 1;

                // BEO-12682 set default start and end dates for subscription
                if (!subscriptionToInsert.StartDate.HasValue)
                {
                    subscriptionToInsert.StartDate = new DateTime(2000, 1, 1);
                }

                if (!subscriptionToInsert.EndDate.HasValue)
                {
                    subscriptionToInsert.EndDate = new DateTime(2099, 1, 1);
                }

                int id = _repository.AddSubscription(contextData.GroupId, contextData.UserId.Value, subscriptionToInsert, basePricePlanId, basePriceCodeId, isRecurring, extDiscountId);
                if (id == 0)
                {
                    log.Error($"Error while Insert Subscription. contextData: {contextData.ToString()}.");
                    return response;
                }

                _pricingModule.InvalidateSubscription(contextData.GroupId);

                // Add VirtualAssetInfo for new subscription 
                var virtualAssetInfo = new VirtualAssetInfo()
                {
                    Type = ObjectVirtualAssetInfoType.Subscription,
                    Id = id,
                    Name = subscriptionToInsert.Names[0].m_sValue,
                    UserId = contextData.UserId.Value,
                    StartDate = subscriptionToInsert.StartDate,
                    EndDate = subscriptionToInsert.EndDate,
                    IsActive = subscriptionToInsert.IsActive,
                    Description = subscriptionToInsert.Descriptions != null && subscriptionToInsert.Descriptions.Length > 0 ?
                                    subscriptionToInsert.Descriptions[0].m_sValue : string.Empty
                };

                var virtualAssetInfoResponse = _virtualAssetManager.AddVirtualAsset(contextData.GroupId, virtualAssetInfo);
                if (virtualAssetInfoResponse.Status == VirtualAssetInfoStatus.Error)
                {
                    log.Error($"Error while AddVirtualAsset - SubscriptionId: {id} will delete ");
                    Delete(contextData, id);
                    return response;
                }

                if (virtualAssetInfoResponse.Status == VirtualAssetInfoStatus.OK && virtualAssetInfoResponse.AssetId > 0)
                {
                    _repository.UpdateSubscriptionVirtualAssetId(contextData.GroupId, id, virtualAssetInfoResponse.AssetId, contextData.UserId.Value);
                }

                subscriptionToInsert.Id = id;
                subscriptionToInsert.CouponGroups = null; // this empty object needed for mapping. don't remove it
                subscriptionToInsert.ExternalProductCodes = null; // this empty object needed for mapping. don't remove it
                response.Object = subscriptionToInsert;
                response.Status.Set(eResponseStatus.OK);
            }
            catch (Exception ex)
            {
                log.Error($"An Exception was occurred in Add Subscription. contextData:{contextData.ToString()}.", ex);
            }

            return response;
        }
        public Status Delete(ContextData contextData, long id)
        {
            Status result = new Status();

            if (!_groupSettingsManager.IsOpc(contextData.GroupId))
            {
                result.Set(eResponseStatus.AccountIsNotOpcSupported, "Account Is Not OPC Supported");
                return result;
            }

            if (!_repository.IsSubscriptionExists(contextData.GroupId, id))
            {
                result.Set(eResponseStatus.SubscriptionDoesNotExist, $"Subscription {id} does not exist");
                return result;
            }

            // Due to atomic action delete virtual asset before subscription delete
            // Delete the virtual asset
            var vai = new VirtualAssetInfo()
            {
                Type = ObjectVirtualAssetInfoType.Subscription,
                Id = id,
                UserId = contextData.UserId.Value
            };

            var response = _virtualAssetManager.DeleteVirtualAsset(contextData.GroupId, vai);
            if (response.Status == VirtualAssetInfoStatus.Error)
            {
                log.Error($"Error while delete subscription virtual asset id {vai.ToString()}");
                result.Set(eResponseStatus.Error, $"Failed to delete subscription {id}");
                return result;
            }

            int Id = _repository.DeleteSubscription(contextData.GroupId, id, contextData.UserId.Value);
            if (Id == 0)
            {
                result.Set(eResponseStatus.Error);
            }
            else if (Id == -1)
            {
                result.Set(eResponseStatus.SubscriptionDoesNotExist, $"The subscription {id} not exist");
            }
            else
            {
                _pricingModule.InvalidateSubscription(contextData.GroupId, (int)id);
                result.Set(eResponseStatus.OK);
            }

            return result;
        }

        private long GetSubscriptionByExternalId(int groupId, string externalId)
        {
            return _repository.GetSubscriptionByExternalId(groupId, externalId);
        }

        private Status ValidatePricePlan(List<long> pricePlanIds, int groupId, out long? basePricePlanId, out long? basePriceCodeId, out bool isRecurring, out long? extDiscountId)
        {
            basePricePlanId = null;
            basePriceCodeId = null;
            isRecurring = pricePlanIds.Count > 1;
            extDiscountId = null;


            GenericListResponse<PricePlan> pricePlan = null;
            for (int index = 0; index < pricePlanIds.Count; index++)
            {
                pricePlan = _pricePlanManager.GetPricePlans(groupId, new List<long>() { pricePlanIds[index] });
                if (pricePlan.HasObjects())
                {
                    if (index == 0)
                    {
                        basePricePlanId = pricePlan.Objects[index].Id;
                        basePriceCodeId = pricePlan.Objects[index].PriceDetailsId;
                        extDiscountId = pricePlan.Objects[index].DiscountId;

                        if (!isRecurring)
                        {
                            isRecurring = pricePlan.Objects[index].IsRenewable.HasValue ? pricePlan.Objects[index].IsRenewable.Value : false;
                        }
                    }
                }
                else
                {
                    return new Status() { Code = (int)eResponseStatus.PricePlanDoesNotExist, Message = $"PricePlan {pricePlanIds[index] } not exist" };
                }
            }

            return new Status() { Code = (int)eResponseStatus.OK };
        }

        public GenericResponse<SubscriptionInternal> Update(ContextData contextData, SubscriptionInternal subscriptionToUpdate)
        {
            GenericResponse<SubscriptionInternal> response = new GenericResponse<SubscriptionInternal>();
            if (!_groupSettingsManager.IsOpc(contextData.GroupId))
            {
                response.SetStatus(eResponseStatus.AccountIsNotOpcSupported, "Account Is Not OPC Supported");
                return response;
            }

            VirtualAssetInfo virtualAssetInfo = null;
            Status status;

            var subscription = _pricingModule.GetSubscriptionData(contextData.GroupId, subscriptionToUpdate.Id.ToString(), string.Empty, string.Empty, string.Empty, false);
            if (subscription == null)
            {
                response.SetStatus(eResponseStatus.SubscriptionDoesNotExist, $"Subscription {subscriptionToUpdate.Id} does not exist");
                return response;
            }

            if (subscriptionToUpdate.ChannelsIds != null)
            {
                status = ValidateChannelsForUpdate(contextData.GroupId, subscriptionToUpdate.ChannelsIds, subscription.m_sCodes);
                if (!status.IsOkStatusCode())
                {
                    response.SetStatus(status);
                    return response;
                }
            }

            if (subscriptionToUpdate.CouponGroups != null)
            {
                status = ValidateCouponGroupsForUpdate(contextData.GroupId, subscriptionToUpdate.CouponGroups, subscription.CouponsGroups);
                if (!status.IsOkStatusCode())
                {
                    response.SetStatus(status);
                    return response;
                }
            }

            var nullableEndDate = new DAL.NullableObj<DateTime?>(subscriptionToUpdate.EndDate, subscriptionToUpdate.IsNullablePropertyExists("EndDate"));

            if (subscriptionToUpdate.ExternalId != string.Empty && subscriptionToUpdate.ExternalId != subscription.m_ProductCode)
            {
                status = ValidateExternalId(contextData.GroupId, subscriptionToUpdate.ExternalId);
                if (!status.IsOkStatusCode())
                {
                    response.SetStatus(status);
                    return response;
                }
            }

            if (subscriptionToUpdate.FileTypesIds != null)
            {
                status = ValidateFileTypesForUpdate(contextData.GroupId, subscriptionToUpdate.FileTypesIds, subscription.m_sFileTypes);
                if (!status.IsOkStatusCode())
                {
                    response.SetStatus(status);
                    return response;
                }
            }

            if (subscriptionToUpdate.HouseholdLimitationsId.HasValue && subscriptionToUpdate.HouseholdLimitationsId.Value != subscription.m_nDomainLimitationModule)
            {
                status = ValidateDLM(contextData.GroupId, subscriptionToUpdate.HouseholdLimitationsId.Value);
                if (!status.IsOkStatusCode())
                {
                    response.SetStatus(status);
                    return response;
                }
            }

            if (subscriptionToUpdate.InternalDiscountModuleId.HasValue)
            {
                if (subscription.m_oDiscountModule != null && subscription.m_oDiscountModule.m_nObjectID != subscriptionToUpdate.InternalDiscountModuleId.Value)
                {
                    status = ValidateDiscountModule(contextData.GroupId, subscriptionToUpdate.InternalDiscountModuleId.Value);
                    if (!status.IsOkStatusCode())
                    {
                        response.SetStatus(status);
                        return response;
                    }
                }
            }

            if (subscriptionToUpdate.Names != null)
            {
                status = ValidateNamesForUpdate(subscriptionToUpdate.Names);
                if (!status.IsOkStatusCode())
                {
                    response.SetStatus(status);
                    return response;
                }

                if (subscriptionToUpdate.Names[0].m_sValue != subscription.m_sObjectVirtualName)
                {
                    virtualAssetInfo = new VirtualAssetInfo()
                    {
                        Type = ObjectVirtualAssetInfoType.Subscription,
                        Id = subscriptionToUpdate.Id,
                        Name = subscriptionToUpdate.Names[0].m_sValue,
                        UserId = contextData.UserId.Value
                    };
                }
            }

            if (subscriptionToUpdate.PremiumServices != null)
            {
                status = ValidatePremiumServicesForUpdate(subscriptionToUpdate.PremiumServices.Select(x => x.ID).ToList());
                if (!status.IsOkStatusCode())
                {
                    response.SetStatus(status);
                    return response;
                }
            }

            long? basePricePlanId = null;
            long? basePriceCodeId = null;
            bool? isRecurring = false;
            long? extDiscountId = null;

            if (subscriptionToUpdate.PricePlanIds != null)
            {
                status = ValidatePricePlanIdsForUpdate(contextData.GroupId, subscriptionToUpdate.PricePlanIds, subscription.m_MultiSubscriptionUsageModule);
                if (!status.IsOkStatusCode())
                {
                    response.SetStatus(status);
                    return response;
                }

                if (subscriptionToUpdate.PricePlanIds.Count > 0)
                {
                    isRecurring = subscriptionToUpdate.PricePlanIds.Count > 1;
                    var pricePlan = _pricePlanManager.GetPricePlans(contextData.GroupId, new List<long>() { subscriptionToUpdate.PricePlanIds[0] });
                    if (pricePlan.HasObjects())
                    {
                        basePricePlanId = pricePlan.Objects[0].Id;
                        basePriceCodeId = pricePlan.Objects[0].PriceDetailsId;
                        extDiscountId = pricePlan.Objects[0].DiscountId;

                        if (!isRecurring.Value)
                        {
                            isRecurring = pricePlan.Objects[0].IsRenewable.HasValue ? pricePlan.Objects[0].IsRenewable.Value : false;
                        }
                    }
                }
            }

            var nullableStartDate = new DAL.NullableObj<DateTime?>(subscriptionToUpdate.StartDate, subscriptionToUpdate.IsNullablePropertyExists("StartDate"));

            // Due to atomic action update virtual asset before subscription update
            if (virtualAssetInfo != null)
            {
                var virtualAssetInfoResponse = _virtualAssetManager.UpdateVirtualAsset(contextData.GroupId, virtualAssetInfo);
                if (virtualAssetInfoResponse.Status == VirtualAssetInfoStatus.Error)
                {
                    log.Error($"Error while update subscription's virtualAsset. groupId: {contextData.GroupId}, subscriptionId: {subscriptionToUpdate.Id}, Name: {subscriptionToUpdate.Names[0].m_sValue} ");
                    if (virtualAssetInfoResponse.ResponseStatus != null)
                    {
                        response.SetStatus(virtualAssetInfoResponse.ResponseStatus);
                    }
                    else
                    {
                        response.SetStatus(eResponseStatus.Error, "Error while updating subscription.");
                    }

                    return response;
                }
            }

            subscriptionToUpdate.ProrityInOrder = subscriptionToUpdate.ProrityInOrder.HasValue ? subscriptionToUpdate.ProrityInOrder : subscription.m_Priority;

            bool success = _repository.UpdateSubscription(contextData.GroupId, contextData.UserId.Value, subscriptionToUpdate, basePricePlanId, basePriceCodeId, isRecurring,
                nullableStartDate, nullableEndDate, extDiscountId);
            if (success)
            {
                _pricingModule.InvalidateSubscription(contextData.GroupId, (int)subscriptionToUpdate.Id);

                subscriptionToUpdate.CouponGroups = null; // this empty object needed for mapping. don't remove it
                subscriptionToUpdate.ExternalProductCodes = null; // this empty object needed for mapping. don't remove it
                response.Object = subscriptionToUpdate;
                response.SetStatus(eResponseStatus.OK);
            }

            return response;
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

        private Status ValidateExternalId(int groupId, string externalId)
        {
            Status status = new Status(eResponseStatus.OK);

            if (!string.IsNullOrEmpty(externalId) && GetSubscriptionByExternalId(groupId, externalId) > 0)
            {
                status.Set(eResponseStatus.ExternalIdAlreadyExists, $"subscription external ID already exists {externalId}");
                return status;
            }

            return status;
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

        private Status ValidateDLM(int groupId, int householdLimitationsId)
        {
            Status status = new Status(eResponseStatus.OK);

            var dlmResponse = _domainModule.GetDLM(groupId, householdLimitationsId);
            if (dlmResponse.dlm == null || dlmResponse.resp.Code != (int)eResponseStatus.OK)
            {
                status.Set(eResponseStatus.DlmNotExist, $"HouseholdLimitationsId not exist {householdLimitationsId}");
                return status;
            }

            return status;
        }

        private Status ValidateDiscountModule(int groupId, long internalDiscountModuleId)
        {
            Status status = new Status(eResponseStatus.OK);

            var discounts = _pricingModule.GetValidDiscounts(groupId);

            if (!discounts.HasObjects() || !discounts.Objects.Any(x => x.Id == internalDiscountModuleId))
            {
                status.Set(eResponseStatus.InvalidDiscountCode, $"InternalDiscountModuleId is missing for group");
                return status;
            }

            return status;
        }

        private Status ValidateChannelsForUpdate(int groupId, List<long> channelsIds, BundleCodeContainer[] codes)
        {
            Status status = new Status(eResponseStatus.OK);

            if (channelsIds.Count == 0 || codes == null)
            {
                return status;
            }
            else
            {
                // compare current channelsIds with updated list
                if (codes != null && codes.Length > 0)
                {
                    var currChannelsIds = codes.Select(x => long.Parse(x.m_sCode)).ToList();

                    // check if both lists contain the same items in the same order. in case they ar not, need to update
                    if (!channelsIds.SequenceEqual(currChannelsIds))
                    {
                        // need to validate new channels in the list
                        var newChennelsInList = channelsIds.Except(currChannelsIds).ToList();
                        status = ValidateChannels(groupId, newChennelsInList);
                        if (!status.IsOkStatusCode())
                        {
                            return status;
                        }
                    }
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

        private Status ValidateNamesForUpdate(LanguageContainer[] names)
        {
            if (names.Length == 0 || string.IsNullOrEmpty(names[0].m_sValue))
            {
                return new Status(eResponseStatus.NameRequired, "subscription name cannot be empty");
            }

            return new Status(eResponseStatus.OK);
        }

        private Status ValidatePricePlanIdsForUpdate(int groupId, List<long> pricePlanIds, UsageModule[] multiSubscriptionUsageModule)
        {
            Status status = null;

            if (pricePlanIds.Count == 0)
            {
                return new Status(eResponseStatus.OK);
            }
            else
            {
                if (multiSubscriptionUsageModule == null)
                {
                    status = ValidatePricePlan(pricePlanIds, groupId);
                    if (!status.IsOkStatusCode())
                    {
                        return status;
                    }
                }
                else
                {
                    // compare current fileTypes with updated list                
                    var currPricePlanIds = multiSubscriptionUsageModule.Select(s => (long)s.m_nObjectID).ToList();
                    // check if both lists contain the same items in the same order. in case they ar not, need to update                    

                    if (!pricePlanIds.SequenceEqual(currPricePlanIds))
                    {
                        // need to validate new pricePlanIds in the list
                        var newPricePlanIdsInList = pricePlanIds.Except(currPricePlanIds).ToList();
                        status = ValidatePricePlan(newPricePlanIdsInList, groupId);
                        if (!status.IsOkStatusCode())
                        {
                            return status;
                        }
                    }
                }
            }

            return new Status(eResponseStatus.OK);
        }

        private Status ValidatePricePlan(List<long> pricePlanIds, int groupId)
        {
            GenericListResponse<PricePlan> pricePlan = null;
            for (int index = 0; index < pricePlanIds.Count; index++)
            {
                pricePlan = _pricePlanManager.GetPricePlans(groupId, new List<long>() { pricePlanIds[index] });
                if (!pricePlan.IsOkStatusCode() || pricePlan.Objects?.Count == 0)
                {
                    return new Status() { Code = (int)eResponseStatus.PricePlanDoesNotExist, Message = $"PricePlan {pricePlanIds[index] } not exist" };
                }
            }

            return new Status() { Code = (int)eResponseStatus.OK };
        }

        private Status ValidatePremiumServices(List<long> premiumServices)
        {
            var allService = _partnerPremiumServicesManager.GetAllPremiumServices();
            List<long> allServiceIds = allService.Select(x => x.ID).ToList();

            if (allServiceIds.Count > 0 && premiumServices != null)
            {
                foreach (var service in premiumServices)
                {
                    if (!allServiceIds.Contains(service))
                    {
                        return new Status() { Code = (int)eResponseStatus.PremiumServiceDoesNotExist, Message = $"premiumService {service} not exist" };
                    }
                }
            }

            return new Status() { Code = (int)eResponseStatus.OK };
        }

        private Status ValidatePremiumServicesForUpdate(List<long> premiumServices)
        {
            if (premiumServices.Count == 0)
            {
                return new Status(eResponseStatus.OK);
            }
            else
            {
                var status = ValidatePremiumServices(premiumServices);
                if (!status.IsOkStatusCode())
                {
                    return status;
                }
            }

            return new Status(eResponseStatus.OK);
        }

        public void HandleChannelUpdate(int groupId, int channelId)
        {
            List<int> subscriptionsIds = _repository.GetSubscriptionsByChannelId(groupId, channelId);

            //delete all subscriptions_Channels ByChannel id
            _repository.DeleteSubscriptionsChannelsByChannel(groupId, channelId);

            if (subscriptionsIds?.Count > 0)
            {
                foreach (var id in subscriptionsIds)
                {
                    _pricingModule.InvalidateSubscription(groupId, id);
                }
            }
        }
    }
}