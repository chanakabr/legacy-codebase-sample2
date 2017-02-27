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
                    response = Core.Pricing.Module.GetSubscriptions(groupId,
                        subscriptionsIds, string.Empty, languageCode, udid, wsOrderBy);
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
    }
}