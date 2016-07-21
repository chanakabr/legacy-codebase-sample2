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
using WebAPI.Pricing;
using WebAPI.Utils;
using System.Net;
using System.ServiceModel;

namespace WebAPI.Clients
{
    public class PricingClient : BaseClient
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public PricingClient()
        {
        }

        protected WebAPI.Pricing.mdoule Pricing
        {
            get
            {
                return (Module as WebAPI.Pricing.mdoule);
            }
        }

        internal List<KalturaSubscription> GetSubscriptionsData(int groupId, string[] subscriptionsIds, string udid, string languageCode, KalturaSubscriptionOrderBy orderBy)
        {
            WebAPI.Pricing.SubscriptionsResponse response = null;
            List<KalturaSubscription> subscriptions = new List<KalturaSubscription>();

            Group group = GroupsManager.GetGroup(groupId);
            SubscriptionOrderBy wsOrderBy = PricingMappings.ConvertSubscriptionOrderBy(orderBy);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Pricing.GetSubscriptions(group.PricingCredentials.Username, group.PricingCredentials.Password,
                        subscriptionsIds, string.Empty, languageCode, udid, wsOrderBy);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling pricing service. ws address: {0}, exception: {1}", Pricing.Url, ex);
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
            WebAPI.Pricing.IdsResponse response = null;
            List<int> subscriptions = new List<int>();

            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Pricing.GetSubscriptionIDsContainingMediaFile(group.PricingCredentials.Username, group.PricingCredentials.Password, 0, mediaFileID);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling pricing service. ws address: {0}, exception: {1}", Pricing.Url, ex);
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
            WebAPI.Pricing.CouponDataResponse response = null;
            KalturaCoupon coupon = new KalturaCoupon();

            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Pricing.GetCouponStatus(group.PricingCredentials.Username, group.PricingCredentials.Password, couponCode);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling pricing service. ws address: {0}, exception: {1}", Pricing.Url, ex);
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
            WebAPI.Pricing.PPVModuleDataResponse response = null;
            KalturaPpv result = new KalturaPpv();

            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Pricing.GetPPVModuleResponse(group.PricingCredentials.Username, group.PricingCredentials.Password, ppvCode.ToString(), string.Empty, string.Empty, string.Empty);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling pricing service. ws address: {0}, exception: {1}", Pricing.Url, ex);
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

namespace WebAPI.Pricing
{
    // adding request ID to header
    public partial class mdoule
    {
        protected override WebRequest GetWebRequest(Uri uri)
        {
            HttpWebRequest request = (HttpWebRequest)base.GetWebRequest(uri);
            KlogMonitorHelper.MonitorLogsHelper.AddHeaderToWebService(request);
            return request;
        }
    }
}