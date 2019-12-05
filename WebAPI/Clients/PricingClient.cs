using ApiObjects;
using ApiObjects.Pricing;
using ApiObjects.Response;
using Core.Pricing;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TVinciShared;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.General;
using WebAPI.Models.Pricing;
using WebAPI.ObjectsConvertor.Mapping;
using WebAPI.Utils;

namespace WebAPI.Clients
{
    public class PricingClient : BaseClient
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public PricingClient()
        {
        }

        internal List<KalturaSubscription> GetSubscriptionsData(int groupId, string[] subscriptionsIds, string udid, string languageCode, KalturaSubscriptionOrderBy orderBy,
            AssetSearchDefinition assetSearchDefinition, int pageIndex = 0, int? pageSize = 30, int? couponGroupIdEqual = null)
        {
            SubscriptionsResponse response = null;
            List<KalturaSubscription> subscriptions = new List<KalturaSubscription>();


            SubscriptionOrderBy wsOrderBy = PricingMappings.ConvertSubscriptionOrderBy(orderBy);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Pricing.Module.GetSubscriptions(groupId, subscriptionsIds, string.Empty, languageCode, udid, assetSearchDefinition, wsOrderBy, pageIndex, pageSize.Value, false, couponGroupIdEqual);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling pricing service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            subscriptions = AutoMapper.Mapper.Map<List<KalturaSubscription>>(response.Subscriptions);

            return subscriptions;
        }

        internal List<KalturaSubscription> GetSubscriptionsData(int groupId, string udid, string language, KalturaSubscriptionOrderBy orderBy, int pageIndex, int? pageSize, int? couponGroupIdEqual = null)
        {
            SubscriptionsResponse response = null;
            List<KalturaSubscription> subscriptions = new List<KalturaSubscription>();


            SubscriptionOrderBy wsOrderBy = PricingMappings.ConvertSubscriptionOrderBy(orderBy);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Pricing.Module.GetSubscriptions(groupId, string.Empty, language, udid, wsOrderBy, pageIndex, pageSize.Value, false, couponGroupIdEqual);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling pricing service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            subscriptions = AutoMapper.Mapper.Map<List<KalturaSubscription>>(response.Subscriptions);

            return subscriptions;
        }

        internal List<int> GetSubscriptionIDsContainingMediaFile(int groupId, int mediaFileID)
        {
            IdsResponse response = null;
            List<int> subscriptions = new List<int>();

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Pricing.Module.GetSubscriptionIDsContainingMediaFile(groupId, 0, mediaFileID);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling pricing service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            subscriptions = PricingMappings.ConvertToIntList(response.Ids);

            return subscriptions;
        }

        internal KalturaCoupon GetCouponStatus(int groupId, string couponCode, long householdId)
        {
            CouponDataResponse response = null;
            KalturaCoupon coupon = new KalturaCoupon();

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Pricing.Module.GetCouponStatus(groupId, couponCode, householdId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling pricing service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            coupon = AutoMapper.Mapper.Map<KalturaCoupon>(response.Coupon);

            return coupon;
        }

        internal KalturaPpv GetPPVModuleData(int groupId, long ppvCode)
        {
            PPVModuleDataResponse response = null;
            KalturaPpv result = new KalturaPpv();

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Pricing.Module.GetPPVModuleResponse(groupId, ppvCode.ToString(), string.Empty, string.Empty, string.Empty);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling pricing service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                // general exception
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                // internal web service exception
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            result = AutoMapper.Mapper.Map<KalturaPpv>(response.PPVModule);

            return result;
        }

        internal KalturaCoupon ValidateCouponForSubscription(int groupId, int subscriptionId, string couponCode, long householdId)
        {
            CouponDataResponse response = null;
            KalturaCoupon coupon = new KalturaCoupon();

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Pricing.Module.ValidateCouponForSubscription(groupId, subscriptionId, couponCode, householdId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling pricing service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            coupon = AutoMapper.Mapper.Map<KalturaCoupon>(response.Coupon);

            return coupon;
        }

        internal KalturaSubscriptionSetListResponse GetSubscriptionSets(int groupId, List<long> ids, KalturaSubscriptionSetOrderBy? orderBy, KalturaSubscriptionSetType? type)
        {
            KalturaSubscriptionSetListResponse result = new KalturaSubscriptionSetListResponse() { TotalCount = 0 };
            SubscriptionSetsResponse response = null;

            try
            {
                SubscriptionSetType? setType = PricingMappings.ConvertSubscriptionSetType(type);

                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {

                    response = Core.Pricing.Module.GetSubscriptionSets(groupId, ids, setType);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling pricing service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            if (response.SubscriptionSets != null && response.SubscriptionSets.Count > 0)
            {
                result.TotalCount = response.SubscriptionSets.Count;
                result.SubscriptionSets = new List<KalturaSubscriptionSet>();
                foreach (SubscriptionSet subscriptionSet in response.SubscriptionSets)
                {
                    if (subscriptionSet.Type == SubscriptionSetType.Dependency)
                    {
                        result.SubscriptionSets.Add(AutoMapper.Mapper.Map<KalturaSubscriptionDependencySet>(subscriptionSet));
                    }
                    else
                    {
                        result.SubscriptionSets.Add(AutoMapper.Mapper.Map<KalturaSubscriptionSwitchSet>(subscriptionSet));
                    }
                }
            }

            if (result.TotalCount > 0 && orderBy.HasValue)
            {
                switch (orderBy.Value)
                {
                    case KalturaSubscriptionSetOrderBy.NAME_ASC:
                        result.SubscriptionSets = result.SubscriptionSets.OrderBy(x => x.Name).ToList();
                        break;
                    case KalturaSubscriptionSetOrderBy.NAME_DESC:
                        result.SubscriptionSets = result.SubscriptionSets.OrderByDescending(x => x.Name).ToList();
                        break;
                    default:
                        break;
                }
            }

            return result;
        }

        internal KalturaSubscriptionSetListResponse GetSubscriptionSetsBySubscriptionIds(int groupId, List<long> subscriptionIds, KalturaSubscriptionSetOrderBy? orderBy, KalturaSubscriptionSetType? type)
        {
            KalturaSubscriptionSetListResponse result = new KalturaSubscriptionSetListResponse() { TotalCount = 0 };
            SubscriptionSetsResponse response = null;

            try
            {
                SubscriptionSetType? setType = PricingMappings.ConvertSubscriptionSetType(type);

                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Pricing.Module.GetSubscriptionSetsBySubscriptionIds(groupId, subscriptionIds, setType);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling pricing service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            if (response.SubscriptionSets != null && response.SubscriptionSets.Count > 0)
            {
                result.TotalCount = response.SubscriptionSets.Count;
                result.SubscriptionSets = new List<KalturaSubscriptionSet>();
                foreach (SubscriptionSet subscriptionSet in response.SubscriptionSets)
                {
                    result.SubscriptionSets.Add(AutoMapper.Mapper.Map<KalturaSubscriptionSwitchSet>(subscriptionSet));
                }
            }

            if (result.TotalCount > 0 && orderBy.HasValue)
            {
                switch (orderBy.Value)
                {
                    case KalturaSubscriptionSetOrderBy.NAME_ASC:
                        result.SubscriptionSets = result.SubscriptionSets.OrderBy(x => x.Name).ToList();
                        break;
                    case KalturaSubscriptionSetOrderBy.NAME_DESC:
                        result.SubscriptionSets = result.SubscriptionSets.OrderByDescending(x => x.Name).ToList();
                        break;
                    default:
                        break;
                }
            }

            return result;
        }

        internal KalturaSubscriptionSet AddSubscriptionSet(int groupId, string name, List<long> subscriptionIds)
        {
            KalturaSubscriptionSet subscriptionSet = null;
            SubscriptionSetsResponse response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    // fire request
                    response = Core.Pricing.Module.AddSubscriptionSet(groupId, name, subscriptionIds);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                // general exception
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                // internal web service exception
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            if (response.SubscriptionSets != null && response.SubscriptionSets.Count == 1)
            {
                // convert response
                subscriptionSet = AutoMapper.Mapper.Map<KalturaSubscriptionSwitchSet>(response.SubscriptionSets.First());
            }

            return subscriptionSet;
        }

        internal KalturaSubscriptionSet UpdateSubscriptionSet(int groupId, long setId, string name, List<long> subscriptionIds, bool shouldUpdateSubscriptionIds)
        {
            KalturaSubscriptionSet subscriptionSet = null;
            SubscriptionSetsResponse response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    // fire request
                    response = Core.Pricing.Module.UpdateSubscriptionSet(groupId, setId, name, subscriptionIds, shouldUpdateSubscriptionIds);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                // general exception
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                // internal web service exception
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            if (response.SubscriptionSets != null && response.SubscriptionSets.Count == 1)
            {
                // convert response
                subscriptionSet = AutoMapper.Mapper.Map<KalturaSubscriptionSwitchSet>(response.SubscriptionSets.First());
            }

            return subscriptionSet;
        }

        internal bool DeleteSubscriptionSet(int groupId, long setId)
        {
            Status response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    // fire request
                    response = Core.Pricing.Module.DeleteSubscriptionSet(groupId, setId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                // general exception
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Code != (int)StatusCode.OK)
            {
                // internal web service exception
                throw new ClientException(response.Code, response.Message);
            }

            return true;
        }

        internal KalturaSubscriptionSet GetSubscriptionSet(int groupId, long setId)
        {
            KalturaSubscriptionSet result = null;
            SubscriptionSetsResponse response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Pricing.Module.GetSubscriptionSet(groupId, setId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling pricing service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            if (response.SubscriptionSets != null && response.SubscriptionSets.Count == 1)
            {
                if (response.SubscriptionSets[0].Type == SubscriptionSetType.Dependency)
                {
                    result = AutoMapper.Mapper.Map<KalturaSubscriptionDependencySet>(response.SubscriptionSets[0]);
                }
                else
                {
                    result = AutoMapper.Mapper.Map<KalturaSubscriptionSwitchSet>(response.SubscriptionSets[0]);
                }
            }

            return result;
        }


        internal KalturaSubscriptionSetListResponse GetSubscriptionSetsBySBaseSubscriptionIds(int groupId, List<long> subscriptionIds, KalturaSubscriptionSetOrderBy? orderBy, KalturaSubscriptionSetType? type)
        {
            KalturaSubscriptionSetListResponse result = new KalturaSubscriptionSetListResponse() { TotalCount = 0 };
            SubscriptionSetsResponse response = null;

            try
            {
                SubscriptionSetType? setType = PricingMappings.ConvertSubscriptionSetType(type);

                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Pricing.Module.GetSubscriptionSetsByBaseSubscriptionIds(groupId, subscriptionIds, setType);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling pricing service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            if (response.SubscriptionSets != null && response.SubscriptionSets.Count > 0)
            {
                result.TotalCount = response.SubscriptionSets.Count;
                result.SubscriptionSets = new List<KalturaSubscriptionSet>();
                foreach (SubscriptionSet subscriptionSet in response.SubscriptionSets)
                {
                    if (subscriptionSet.Type == SubscriptionSetType.Dependency)
                    {
                        result.SubscriptionSets.Add(AutoMapper.Mapper.Map<KalturaSubscriptionDependencySet>(subscriptionSet));
                    }
                    else
                    {
                        result.SubscriptionSets.Add(AutoMapper.Mapper.Map<KalturaSubscriptionSwitchSet>(subscriptionSet));
                    }
                }
            }

            if (result.TotalCount > 0 && orderBy.HasValue)
            {
                switch (orderBy.Value)
                {
                    case KalturaSubscriptionSetOrderBy.NAME_ASC:
                        result.SubscriptionSets = result.SubscriptionSets.OrderBy(x => x.Name).ToList();
                        break;
                    case KalturaSubscriptionSetOrderBy.NAME_DESC:
                        result.SubscriptionSets = result.SubscriptionSets.OrderByDescending(x => x.Name).ToList();
                        break;
                    default:
                        break;
                }
            }

            return result;
        }

        internal KalturaSubscriptionSet AddSubscriptionDependencySet(int groupId, string name, long baseSubscriptionId, List<long> subscriptionIds)
        {
            KalturaSubscriptionSet subscriptionSet = null;
            SubscriptionSetsResponse response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    // fire request
                    response = Core.Pricing.Module.AddSubscriptionDependencySet(groupId, name, baseSubscriptionId, subscriptionIds, SubscriptionSetType.Dependency);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                // general exception
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                // internal web service exception
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            if (response.SubscriptionSets != null && response.SubscriptionSets.Count == 1)
            {
                // convert response
                subscriptionSet = AutoMapper.Mapper.Map<KalturaSubscriptionDependencySet>(response.SubscriptionSets.First());
            }

            return subscriptionSet;
        }

        internal KalturaSubscriptionSet UpdateSubscriptionDependencySet(int groupId, long setId, string name, long? baseSubscriptionId, List<long> subscriptionIds, bool shouldUpdateSubscriptionIds)
        {
            KalturaSubscriptionSet subscriptionSet = null;
            SubscriptionSetsResponse response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    // fire request
                    response = Core.Pricing.Module.UpdateSubscriptionDependencySet(groupId, setId, name, baseSubscriptionId, subscriptionIds, shouldUpdateSubscriptionIds);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                // general exception
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                // internal web service exception
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            if (response.SubscriptionSets != null && response.SubscriptionSets.Count == 1)
            {
                // convert response
                subscriptionSet = AutoMapper.Mapper.Map<KalturaSubscriptionDependencySet>(response.SubscriptionSets.First());
            }

            return subscriptionSet;
        }

        internal List<KalturaPriceDetails> GetPrices(int groupId, List<long> priceIds, string currency)
        {
            PriceDetailsResponse response = null;
            List<KalturaPriceDetails> prices = new List<KalturaPriceDetails>();

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Pricing.Module.GetPriceCodesDataByCurrency(groupId, priceIds, currency);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling pricing service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            prices = AutoMapper.Mapper.Map<List<KalturaPriceDetails>>(response.PriceCodes);

            return prices;
        }

        internal List<KalturaPricePlan> GetPricePlans(int groupId, List<long> pricePlanIds)
        {
            UsageModulesResponse response = null;
            List<KalturaPricePlan> pricePlans = new List<KalturaPricePlan>();

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Pricing.Module.GetPricePlans(groupId, pricePlanIds);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling pricing service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            pricePlans = AutoMapper.Mapper.Map<List<KalturaPricePlan>>(response.UsageModules);

            return pricePlans;
        }

        internal KalturaPricePlan UpdatePricePlan(int groupId, long id, KalturaPricePlan pricePlan)
        {
            KalturaPricePlan pricePlanResponse = null;
            UsageModulesResponse response = null;

            UsageModule usageModule = AutoMapper.Mapper.Map<UsageModule>(pricePlan);
            usageModule.m_nObjectID = (int)id;
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Pricing.Module.UpdatePricePlan(groupId, usageModule);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                // general exception
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                // internal web service exception
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            if (response.UsageModules != null && response.UsageModules.Count > 0)
            {
                // convert response
                pricePlanResponse = AutoMapper.Mapper.Map<KalturaPricePlan>(response.UsageModules[0]);
            }
            return pricePlanResponse;
        }

        internal List<KalturaSubscription> GetSubscriptionsDataByProductCodes(int groupId, List<string> productCodes, KalturaSubscriptionOrderBy orderBy)
        {
            SubscriptionsResponse response = null;
            List<KalturaSubscription> subscriptions = new List<KalturaSubscription>();
            SubscriptionOrderBy wsOrderBy = AutoMapper.Mapper.Map<SubscriptionOrderBy>(orderBy);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Pricing.Module.GetSubscriptionsByProductCodes(groupId, productCodes, wsOrderBy);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling pricing service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            subscriptions = AutoMapper.Mapper.Map<List<KalturaSubscription>>(response.Subscriptions);

            return subscriptions;
        }

        internal List<KalturaCollection> GetCollectionsData(int groupId, string[] collectionIds, string udid, string language, KalturaCollectionOrderBy orderBy, int pageIndex = 0, int? pageSize = 30, int? couponGroupIdEqual = null)
        {
            CollectionsResponse response = null;
            List<KalturaCollection> collections = new List<KalturaCollection>();

            // TODO: add order by

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Pricing.Module.GetCollectionsData(groupId, collectionIds, string.Empty, language, udid, pageIndex, pageSize.Value, false, couponGroupIdEqual);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling pricing service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            collections = AutoMapper.Mapper.Map<List<KalturaCollection>>(response.Collections);

            return collections;
        }

        internal List<KalturaCollection> GetCollectionsData(int groupId, string udid, string language, KalturaCollectionOrderBy orderBy, int pageIndex, int? pageSize, int? couponGroupIdEqual = null)
        {
            CollectionsResponse response = null;
            List<KalturaCollection> collections = new List<KalturaCollection>();

            // TODO: add order by

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Pricing.Module.GetCollectionsData(groupId, string.Empty, language, udid, pageIndex, pageSize.Value, false, couponGroupIdEqual);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling pricing service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            collections = AutoMapper.Mapper.Map<List<KalturaCollection>>(response.Collections);

            return collections;
        }

        internal List<int> GetCollectionIdsContainingMediaFile(int groupId, int mediaFileID)
        {
            IdsResponse response = null;
            List<int> subscriptions = new List<int>();

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Pricing.Module.GetCollectionIdsContainingMediaFile(groupId, 0, mediaFileID);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling pricing service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            subscriptions = PricingMappings.ConvertToIntList(response.Ids);

            return subscriptions;
        }

        internal KalturaStringValueArray GenerateCode(int groupId, long couponGroupId, int numberOfCoupons, bool useLetters, bool useNumbers, bool useSpecialCharacters)
        {
            KalturaStringValueArray stringValueArray = null;
            CouponGroupGenerationResponse response = new CouponGroupGenerationResponse() { Status = new Status() { Code = (int)eResponseStatus.Error, Message = eResponseStatus.Error.ToString() } };
            Status status = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    var coupons = Core.Pricing.Module.GenerateCoupons(groupId, numberOfCoupons, couponGroupId, out status, useLetters, useNumbers, useSpecialCharacters);
                    response.Status = status;
                    if (coupons != null && coupons.Count > 0)
                    {
                        response.Codes = coupons.Select(x => x.code).ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling pricing service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            stringValueArray = PricingMappings.BuildCouponCodeList(response.Codes);

            return stringValueArray;
        }

        internal KalturaStringValueArray GeneratePublicCode(int groupId, long couponGroupId, string code)
        {
            KalturaStringValueArray stringValueArray = null;
            CouponGroupGenerationResponse response = new CouponGroupGenerationResponse() { Status = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString()) };
            Status status = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    var coupons = Core.Pricing.Module.GeneratePublicCode(groupId, couponGroupId, code, out status);
                    response.Status = status;

                    if (coupons != null && coupons.Count > 0)
                    {
                        response.Codes = coupons.Select(x => x.code).ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling pricing service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            stringValueArray = PricingMappings.BuildCouponCodeList(response.Codes);

            return stringValueArray;
        }

        internal KalturaCouponsGroup GetCouponsGroup(int groupId, long id)
        {
            CouponsGroupResponse response = null;
            KalturaCouponsGroup couponsGroup = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Pricing.Module.GetCouponsGroup(groupId, id);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling pricing service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            couponsGroup = AutoMapper.Mapper.Map<KalturaCouponsGroup>(response.CouponsGroup);

            return couponsGroup;
        }

        internal KalturaCouponsGroupListResponse GetCouponsGroups(int groupId)
        {
            CouponsGroupsResponse response = null;
            KalturaCouponsGroupListResponse couponsGroups = new KalturaCouponsGroupListResponse();

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Pricing.Module.GetCouponsGroups(groupId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling pricing service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }
            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }
            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            couponsGroups.couponsGroups = AutoMapper.Mapper.Map<List<KalturaCouponsGroup>>(response.CouponsGroups);
            couponsGroups.TotalCount = couponsGroups.couponsGroups != null ? couponsGroups.couponsGroups.Count : 0;

            return couponsGroups;
        }

        internal KalturaCouponsGroup UpdateCouponsGroup(int groupId, long id, KalturaCouponsGroup kCouponsGroup)
        {
            CouponsGroupResponse response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    // fire request                        
                    response = Core.Pricing.Module.UpdateCouponsGroup(groupId, id, kCouponsGroup.Name,
                        kCouponsGroup.StartDate.HasValue ? DateUtils.UtcUnixTimestampSecondsToDateTime(kCouponsGroup.StartDate.Value) : new DateTime?(),
                        kCouponsGroup.EndDate.HasValue ? DateUtils.UtcUnixTimestampSecondsToDateTime(kCouponsGroup.EndDate.Value) : new DateTime?(),
                        kCouponsGroup.MaxUsesNumber, kCouponsGroup.MaxUsesNumberOnRenewableSub, kCouponsGroup.MaxHouseholdUses,
                        PricingMappings.ConvertCouponGroupType(kCouponsGroup.CouponGroupType),
                        kCouponsGroup.DiscountId.HasValue ? kCouponsGroup.DiscountId : kCouponsGroup.DiscountCode);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling pricing service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }
            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }
            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            KalturaCouponsGroup kalturaCouponsGroup = AutoMapper.Mapper.Map<KalturaCouponsGroup>(response.CouponsGroup);

            return kalturaCouponsGroup;
        }

        internal bool DeleteCouponsGroups(int groupId, long id)
        {
            Status response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    // fire request
                    response = Core.Pricing.Module.DeleteCouponsGroups(groupId, id);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                // general exception
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Code != (int)StatusCode.OK)
            {
                // internal web service exception
                throw new ClientException(response.Code, response.Message);
            }

            return true;
        }

        internal KalturaCouponsGroup AddCouponsGroup(int groupId, KalturaCouponsGroup kCouponsGroup)
        {
            CouponsGroupResponse response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    //kCouponsGroup.descriptions TODO: 
                    // fire request                 
                    DateTime startDate = new DateTime(1970, 1, 1);
                    DateTime endDate = DateTime.MaxValue;

                    if (kCouponsGroup.StartDate.HasValue)
                    {
                        startDate = DateUtils.UtcUnixTimestampSecondsToDateTime(kCouponsGroup.StartDate.Value);
                    }

                    if (kCouponsGroup.EndDate.HasValue)
                    {
                        endDate = DateUtils.UtcUnixTimestampSecondsToDateTime(kCouponsGroup.EndDate.Value);
                    }

                    response = Core.Pricing.Module.AddCouponsGroup(groupId, kCouponsGroup.Name, startDate, endDate,
                        kCouponsGroup.MaxUsesNumber, kCouponsGroup.MaxUsesNumberOnRenewableSub, kCouponsGroup.MaxHouseholdUses,
                        PricingMappings.ConvertCouponGroupType(kCouponsGroup.CouponGroupType),
                        kCouponsGroup.DiscountId.HasValue ? kCouponsGroup.DiscountId.Value : kCouponsGroup.DiscountCode.Value);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling pricing service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }
            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }
            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            KalturaCouponsGroup kalturaCouponsGroup = AutoMapper.Mapper.Map<KalturaCouponsGroup>(response.CouponsGroup);

            return kalturaCouponsGroup;
        }

        internal List<KalturaDiscountDetails> GetDiscounts(int groupId, List<long> discountIds, string currency)
        {
            GenericListResponse<DiscountDetails> response = null;
            List<KalturaDiscountDetails> discounts = new List<KalturaDiscountDetails>();

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Pricing.Module.GetDiscountsByCurrency(groupId, discountIds, currency);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling pricing service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            discounts = AutoMapper.Mapper.Map<List<KalturaDiscountDetails>>(response.Objects);

            return discounts;
        }

        internal KalturaPpvListResponse GetPPVModulesData(int groupId, KalturaPpvOrderBy orderBy = KalturaPpvOrderBy.NAME_ASC, int? couponGroupIdEqual = null)
        {
            KalturaPpvListResponse result = new KalturaPpvListResponse();

            Func<GenericListResponse<PPVModule>> getPPVModulesDataFunc = () =>
                Core.Pricing.Module.GetPPVModuleList(groupId, couponGroupIdEqual);

            KalturaGenericListResponse<KalturaPpv> response =
                ClientUtils.GetResponseListFromWS<KalturaPpv, PPVModule>(getPPVModulesDataFunc);

            result.Ppvs = response.Objects;
            result.TotalCount = response.TotalCount;

            // order results
            switch (orderBy)
            {
                case KalturaPpvOrderBy.NAME_ASC:
                    result.Ppvs = result.Ppvs.OrderBy(r => r.Name).ToList();
                    break;
                case KalturaPpvOrderBy.NAME_DESC:
                    result.Ppvs = result.Ppvs.OrderByDescending(r => r.Name).ToList();
                    break;
                default:
                    break;
            }

            return result;
        }

        internal KalturaPpvListResponse GetPPVModulesData(int groupId, List<long> list, KalturaPpvOrderBy orderBy = KalturaPpvOrderBy.NAME_ASC, int? CouponGroupIdEqual = null)
        {
            KalturaPpvListResponse result = GetPPVModulesData(groupId, orderBy);
            if (result != null && result.Ppvs != null && result.Ppvs.Count > 0)
            {
                List<string> ppvIds = list.ConvertAll<string>(i => i.ToString());
                result.Ppvs = result.Ppvs.Where(x => ppvIds.Contains(x.Id)).ToList();
                result.TotalCount = result.Ppvs.Count;

            }
            return result;
        }

        internal KalturaAssetFilePpvListResponse GetAssetFilePPVList(int groupId, long? assetIdEqual, long? assetFileIdEqual)
        {
            KalturaAssetFilePpvListResponse result = new KalturaAssetFilePpvListResponse() { TotalCount = 0 };

            Func<GenericListResponse<AssetFilePpv>> getAssetFilePPVListFunc = () =>
               Core.Pricing.PriceManager.GetAssetFilePPVList(groupId, assetIdEqual.Value, assetFileIdEqual.Value);

            KalturaGenericListResponse<KalturaAssetFilePpv> response =
                ClientUtils.GetResponseListFromWS<KalturaAssetFilePpv, AssetFilePpv>(getAssetFilePPVListFunc);

            result.AssetFilesPpvs = response.Objects;
            result.TotalCount = response.TotalCount;

            return result;
        }

        internal KalturaAssetFilePpv AddAssetFilePpv(int groupId, KalturaAssetFilePpv kAssetFilePpv)
        {
            // fire request                 
            Func<GenericResponse<AssetFilePpv>> addAssetFilePpvFunc = () => Core.Pricing.PriceManager.AddAssetFilePPV(groupId, kAssetFilePpv.AssetFileId,
                kAssetFilePpv.PpvModuleId, DateUtils.UtcUnixTimestampSecondsToDateTime(kAssetFilePpv.StartDate), DateUtils.UtcUnixTimestampSecondsToDateTime(kAssetFilePpv.EndDate));
            return ClientUtils.GetResponseFromWS<KalturaAssetFilePpv, AssetFilePpv>(addAssetFilePpvFunc);

        }

        internal bool DeleteAssetFilePpv(int groupId, long assetFileId, long ppvModuleId)
        {
            Status response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    // fire request
                    response = Core.Pricing.PriceManager.DeleteAssetFilePPV(groupId, assetFileId, ppvModuleId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                // general exception
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Code != (int)StatusCode.OK)
            {
                // internal web service exception
                throw new ClientException(response.Code, response.Message);
            }

            return true;
        }

        internal KalturaAssetFilePpv UpdateAssetFilePpv(int groupId, KalturaAssetFilePpv kAssetFilePpv)
        {
            var request = AutoMapper.Mapper.Map<AssetFilePpv>(kAssetFilePpv);
            Func<GenericResponse<AssetFilePpv>> updateAssetFilePpvFunc = () => Core.Pricing.PriceManager.UpdateAssetFilePPV(groupId, request);
            return ClientUtils.GetResponseFromWS<KalturaAssetFilePpv, AssetFilePpv>(updateAssetFilePpvFunc);
        }

        internal KalturaCouponListResponse GetCoupons(int groupId, List<string> couponCodes, long householdId)
        {
            KalturaCouponListResponse result = new KalturaCouponListResponse();

            Func<GenericListResponse<CouponData>> getGetCouponsFunc = () =>
               Core.Pricing.Module.GetCoupons(groupId, couponCodes, householdId);

            KalturaGenericListResponse<KalturaCoupon> response =
                ClientUtils.GetResponseListFromWS<KalturaCoupon, CouponData>(getGetCouponsFunc);

            result.Objects = response.Objects;
            result.TotalCount = response.TotalCount;

            return result;
        }
    }
}