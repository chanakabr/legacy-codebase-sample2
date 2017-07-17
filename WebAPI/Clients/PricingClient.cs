using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using WebAPI.ClientManagers;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Mapping.ObjectsConvertor;
using WebAPI.Models.General;
using WebAPI.Models.Pricing;
using WebAPI.Utils;
using System.Net;
using System.ServiceModel;
using ApiObjects.Response;
using WebAPI.ObjectsConvertor.Mapping;
using Core.Pricing;
using ApiObjects.Pricing;

namespace WebAPI.Clients
{
    public class PricingClient : BaseClient
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public PricingClient()
        {
        }

        internal List<KalturaSubscription> GetSubscriptionsData(int groupId, string[] subscriptionsIds, string udid, string languageCode, KalturaSubscriptionOrderBy orderBy)
        {
            SubscriptionsResponse response = null;
            List<KalturaSubscription> subscriptions = new List<KalturaSubscription>();

            
            SubscriptionOrderBy wsOrderBy = PricingMappings.ConvertSubscriptionOrderBy(orderBy);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Pricing.Module.GetSubscriptions(groupId, subscriptionsIds, string.Empty, languageCode, udid, wsOrderBy);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling pricing service. exception: {1}", ex);
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
                log.ErrorFormat("Exception received while calling pricing service. exception: {1}", ex);
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

        internal KalturaCoupon GetCouponStatus(int groupId, string couponCode)
        {
            CouponDataResponse response = null;
            KalturaCoupon coupon = new KalturaCoupon();

            

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Pricing.Module.GetCouponStatus(groupId, couponCode);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling pricing service. exception: {1}", ex);
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
                log.ErrorFormat("Exception received while calling pricing service. exception: {1}", ex);
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
              
        internal KalturaCoupon ValidateCouponForSubscription(int groupId, int subscriptionId, string couponCode)
        {
            CouponDataResponse response = null;
            KalturaCoupon coupon = new KalturaCoupon();
                       
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Pricing.Module.ValidateCouponForSubscription(groupId, subscriptionId, couponCode);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling pricing service. exception: {1}", ex);
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
                log.ErrorFormat("Exception received while calling pricing service. exception: {1}", ex);
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
                log.ErrorFormat("Exception received while calling pricing service. exception: {1}", ex);
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
                log.ErrorFormat("Exception received while calling service. exception: {1}", ex);
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
                log.ErrorFormat("Exception received while calling service. exception: {1}", ex);
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
                log.ErrorFormat("Exception received while calling service. exception: {1}", ex);
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
                log.ErrorFormat("Exception received while calling pricing service. exception: {1}", ex);
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
                log.ErrorFormat("Exception received while calling pricing service. exception: {1}", ex);
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
                log.ErrorFormat("Exception received while calling service. exception: {1}", ex);
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
                log.ErrorFormat("Exception received while calling service. exception: {1}", ex);
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

        internal List<KalturaPriceDetails> GetPrices(int groupId, List<long> priceIds)
        {
            throw new NotImplementedException();
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
                log.ErrorFormat("Exception received while calling pricing service. exception: {1}", ex);
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

        internal KalturaPricePlan UpdatePricePlan(int groupId, KalturaPricePlan pricePlan)
        {
            KalturaPricePlan pricePlanResponse = null;
            UsageModulesResponse response = null;

            UsageModule usageModule = AutoMapper.Mapper.Map<UsageModule>(pricePlan);
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Pricing.Module.UpdatePricePlan(groupId, usageModule);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling service. exception: {1}", ex);
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
                log.ErrorFormat("Exception received while calling pricing service. exception: {1}", ex);
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
    }
}