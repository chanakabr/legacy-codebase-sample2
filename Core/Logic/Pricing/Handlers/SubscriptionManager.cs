using ApiObjects;
using ApiObjects.Base;
using ApiObjects.Pricing;
using ApiObjects.Response;
using Core.Api;
using Core.Pricing;
using DAL;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace ApiLogic.Pricing.Handlers
{
    public class SubscriptionManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private static readonly Lazy<SubscriptionManager> lazy = new Lazy<SubscriptionManager>(() =>
                                    new SubscriptionManager(PricingCache.Instance, PricingDAL.Instance, PricingDAL.Instance), LazyThreadSafetyMode.PublicationOnly);

        public static SubscriptionManager Instance => lazy.Value;

        private readonly ISubscriptionManagerRepository _repository;
        private readonly IPricingCache _pricingCache;
        private readonly IModuleManagerRepository _moduleManagerRepository;

        public SubscriptionManager(IPricingCache pricingCache, ISubscriptionManagerRepository repository, IModuleManagerRepository moduleManagerRepository)
        {
            _repository = repository;
            _pricingCache = pricingCache;
            _moduleManagerRepository = moduleManagerRepository;
        }

        public List<int> GetSubscriptionIDsContainingMediaFile(int groupId, int mediaFileIdEqual)
        {
            BaseSubscription t = null;
            Utils.GetBaseImpl(ref t, groupId);
            if (t != null)
            {
                IdsResponse result = (new SubscriptionCacheWrapper(t)).GetSubscriptionIDsContainingMediaFile(groupId, mediaFileIdEqual);
                return result?.Ids?.Count > 0 ? result.Ids : null;
            }
            else
            {
                return null;
            }
        }

        public GenericListResponse<Subscription> GetSubscriptionsData(int groupId, HashSet<long> subscriptionsIds, string udid, string languageCode, SubscriptionOrderBy orderBy,
            AssetSearchDefinition assetSearchDefinition, int pageIndex, int? pageSize = 30, int? couponGroupIdEqual = null)
        {

            SubscriptionsResponse response = GetSubscriptions(groupId, subscriptionsIds, string.Empty, languageCode, udid, assetSearchDefinition, orderBy,
                pageIndex, pageSize.Value, false, couponGroupIdEqual);

            GenericListResponse<Subscription> result = new GenericListResponse<Subscription>();
            if (response.Subscriptions != null)
            {
                result.TotalItems = response.Subscriptions.Length;
                result.Objects = response.Subscriptions.ToList();
            }
            result.SetStatus(response.Status);


            return result;
        }

        public SubscriptionsResponse GetSubscriptions(int groupId, HashSet<long> subscriptionIds, string sCountryCd2, string sLanguageCode3, string sDeviceName,
          AssetSearchDefinition assetSearchDefinition, SubscriptionOrderBy orderBy = SubscriptionOrderBy.StartDateAsc, int pageIndex = 0, int pageSize = 30,
          bool shouldIgnorePaging = true, int? couponGroupIdEqual = null)
        {
            SubscriptionsResponse response = new SubscriptionsResponse();
            BaseSubscription t = null;
            Utils.GetBaseImpl(ref t, groupId);
            if (t != null)
            {
                try
                {
                    var filter = api.Instance.GetObjectVirtualAssetObjectIds(groupId, assetSearchDefinition, ObjectVirtualAssetInfoType.Subscription, subscriptionIds, pageIndex, pageSize);
                    if (filter.ResultStatus == ObjectVirtualAssetFilterStatus.Error)
                    {
                        response.Status = filter.Status;
                        return response;
                    }

                    if (filter.ResultStatus == ObjectVirtualAssetFilterStatus.None)
                    {
                        response.Status = new Status((int)eResponseStatus.OK, "OK");
                        return response;
                    }

                    if (!shouldIgnorePaging && !couponGroupIdEqual.HasValue && filter.ObjectIds?.Count > 0)
                    {
                        int startIndexOnList = pageIndex * pageSize;
                        int rangeToGetFromList = (startIndexOnList + pageSize) > filter.ObjectIds.Count ? (filter.ObjectIds.Count - startIndexOnList) > 0 ? (filter.ObjectIds.Count - startIndexOnList) : 0 : pageSize;
                        if (rangeToGetFromList > 0)
                        {
                            filter.ObjectIds = filter.ObjectIds.Skip(startIndexOnList).Take(rangeToGetFromList).ToList();
                        }
                    }

                    if (filter.ObjectIds?.Count > 0)
                    {
                        response.Subscriptions = (new SubscriptionCacheWrapper(t)).GetSubscriptionsData(filter.ObjectIds.Select(x => x.ToString()).ToArray(), sCountryCd2, sLanguageCode3, sDeviceName, orderBy);

                        if (response.Subscriptions != null && response.Subscriptions.Length > 0 && response.Subscriptions.Any() && couponGroupIdEqual.HasValue)
                        {
                            FilterSubscriptionsByCoupon(pageIndex, pageSize, couponGroupIdEqual, response);
                        }
                    }

                    response.Status = new Status((int)eResponseStatus.OK, "OK");
                }
                catch (Exception)
                {
                    response.Status = new Status((int)eResponseStatus.Error, "Error");
                }
            }
            else
            {
                response.Status = new Status((int)eResponseStatus.Error, "Error");
            }
            return response;
        }

        private static void FilterSubscriptionsByCoupon(int pageIndex, int pageSize, int? couponGroupIdEqual, SubscriptionsResponse response)
        {
            var value = couponGroupIdEqual.Value.ToString();
            var subscriptions = new List<Subscription>();
            var index = 0;
            var startIndex = pageIndex * pageSize;
            foreach (var subscription in response.Subscriptions)
            {
                var couponGroups = subscription.CouponsGroups;

                if (couponGroups.Count > 0 || subscription.m_oCouponsGroup != null)
                {
                    var exists = couponGroups.Any
                        (coupon => coupon?.m_sGroupCode?.ToString() == value
                        && (!coupon.startDate.HasValue || coupon.startDate.Value <= DateTime.UtcNow)
                        && (!coupon.endDate.HasValue || coupon.endDate.Value >= DateTime.UtcNow));

                    if (exists || subscription.m_oCouponsGroup?.m_sGroupCode == value)
                    {
                        index++;
                        if (startIndex < index)
                        {
                            subscriptions.Add(subscription);

                            if (index > (pageIndex + 1) * pageSize)
                            {
                                break;
                            }
                        }
                    }
                }
            }

            response.Subscriptions = subscriptions?.ToArray();
        }


        public GenericListResponse<Subscription> GetSubscriptionsByProductCodeList(int groupId, List<string> productCodes, SubscriptionOrderBy orderBy = SubscriptionOrderBy.StartDateAsc)
        {
            SubscriptionsResponse response = GetSubscriptionsByProductCodes(groupId, productCodes, orderBy);

            GenericListResponse<Subscription> result = new GenericListResponse<Subscription>();
            if (response.Subscriptions != null)
            {
                result.TotalItems = response.Subscriptions.Length;
                result.Objects = response.Subscriptions.ToList();
            }
            result.SetStatus(response.Status);

            return result;
        }

        public SubscriptionsResponse GetSubscriptionsByProductCodes(int groupId, List<string> productCodes, SubscriptionOrderBy orderBy = SubscriptionOrderBy.StartDateAsc)
        {
            SubscriptionsResponse response = new SubscriptionsResponse()
            {
                Status = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString())
            };

            BaseSubscription t = null;
            Utils.GetBaseImpl(ref t, groupId);
            if (t != null)
            {
                response = (new SubscriptionCacheWrapper(t)).GetSubscriptionsDataByProductCodes(productCodes, false, orderBy);
            }

            return response;
        }

        public SubscriptionsResponse GetSubscriptions(int groupId, string language, string udid, SubscriptionOrderBy orderBy, int pageIndex, int pageSize, bool shouldIgnorePaging, int? couponGroupIdEqual = null)
        {
            // get group's subscriptionIds
            var subscriptionIds = _pricingCache.GetSubscriptionsIds(groupId);

            if (subscriptionIds == null)
            {
                return null;
            }

            return GetSubscriptions(groupId, subscriptionIds, string.Empty, language, udid, null, orderBy, pageIndex, pageSize, shouldIgnorePaging, couponGroupIdEqual);
        }

        public GenericListResponse<Subscription> GetSubscriptionsData(int groupId, string udid, string language, SubscriptionOrderBy orderBy, int pageIndex, int? pageSize, int? couponGroupIdEqual = null)
        {
            var response = GetSubscriptions(groupId, language, udid, orderBy, pageIndex, pageSize.Value, false, couponGroupIdEqual);

            GenericListResponse<Subscription> result = new GenericListResponse<Subscription>();
            if (response.Subscriptions != null)
            {
                result.TotalItems = response.Subscriptions.Length;
                result.Objects = response.Subscriptions.ToList();
            }
            result.SetStatus(response.Status);
            return result;
        }

        public GenericResponse<Subscription> GetSubscription(int groupId, long subscriptionId)
        {
            GenericResponse<Subscription> result = new GenericResponse<Subscription>();

            BaseSubscription t = null;
            Utils.GetBaseImpl(ref t, groupId);
            if (t != null)
            {
                var subscriptions = (new SubscriptionCacheWrapper(t)).GetSubscriptionsData(new string[] { subscriptionId.ToString() }, string.Empty, string.Empty, string.Empty, SubscriptionOrderBy.StartDateAsc);

                if (subscriptions?.Length > 0)
                {
                    result.Object = subscriptions[0];
                }

                result.SetStatus(eResponseStatus.OK);
            }

            return result;
        }

        public GenericResponse<SubscriptionInternal> Add(ContextData contextData, SubscriptionInternal subscriptionToInsert)
        {
            var response = new GenericResponse<SubscriptionInternal>();

            try
            {
                int? basePricePlanId = null;
                int? basePriceCodeId = null;
                bool isRecurring = false;

                if (subscriptionToInsert.PricePlanIds?.Count > 0)
                {
                    isRecurring = subscriptionToInsert.PricePlanIds.Count > 1;

                    var firstPricePlan = PricePlanManager.Instance.GetPricePlans(contextData.GroupId, new List<long>() { subscriptionToInsert.PricePlanIds[0] });

                    if (firstPricePlan.HasObjects())
                    {
                        basePricePlanId = firstPricePlan.Objects[0].m_nObjectID;
                        basePriceCodeId = firstPricePlan.Objects[0].m_pricing_id;

                        if (!isRecurring)
                        {
                            isRecurring = firstPricePlan.Objects[0].m_is_renew == 1 ? true : false;
                        }
                    }
                }

                int id = _repository.AddSubscription(contextData.GroupId, contextData.UserId.Value, subscriptionToInsert, basePricePlanId, basePriceCodeId, isRecurring);
                if (id == 0)
                {
                    log.Error($"Error while Insert Subscription. contextData: {contextData.ToString()}.");
                    return response;
                }

                subscriptionToInsert.Id = id;
                subscriptionToInsert.CouponGroups = null; // this empty object needed for mapping. don't remove it
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

            if (!_repository.IsSubscriptionExists(contextData.GroupId, id))
            {
                result.Set(eResponseStatus.SubscriptionDoesNotExist, $"Subscription {id} does not exist");
                return result;
            }

            int Id = _repository.DeleteSubscription(contextData.GroupId, id);
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
                result.Set(eResponseStatus.OK);
            }

            return result;
        }
    }
}