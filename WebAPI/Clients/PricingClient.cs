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
using WebAPI.Models.General;
using WebAPI.Models.Pricing;
using WebAPI.Utils;

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

        internal List<Subscription> GetSubscriptionsData(int groupId, List<string> subscriptionsIds, string udid, string languageCode)
        {
            WebAPI.Pricing.SubscriptionsResponse response = null;
            List<Subscription> subscriptions = new List<Subscription>();

            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Pricing.GetSubscriptionsData(group.PricingCredentials.Username, group.PricingCredentials.Password,
                        subscriptionsIds.ToArray(), string.Empty, languageCode, udid);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. ws address: {0}, exception: {1}", Pricing.Url, ex);
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

            subscriptions = AutoMapper.Mapper.Map<List<Subscription>>(response.Subscriptions);

            return subscriptions;
        }
    }
}