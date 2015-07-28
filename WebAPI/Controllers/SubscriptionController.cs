using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Filters;
using WebAPI.Models.API;
using WebAPI.Models.Pricing;
using WebAPI.Utils;
using WebAPI.Managers.Models;

namespace WebAPI.Controllers
{
    [RoutePrefix("subscription")]
    public class SubscriptionController : ApiController
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        /// <summary>
        /// Returns a price and a purchase status for each subscription, for a given user (if passed) and with the consideration of a coupon code (if passed). 
        /// </summary>
        /// <param name="partner_id">Partner identifier</param>
        /// <param name="subscriptions_ids">Subscription identifiers (separated by ',')</param>
        /// <param name="user_id">User identifier</param>
        /// <param name="coupon_code">Discount coupon code</param>
        /// <param name="udid">Device UDID</param>
        /// <param name="language">Language code</param>
        /// <param name="should_get_only_lowest">A flag that indicates if only the lowest price of a subscription should return</param>
        /// <remarks>Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008 </remarks>
        [Route("subscriptions/{subscriptions_ids}/prices"), HttpGet]
        public KalturaSubscriptionsPricesList GetSubscriptionsPrices([FromUri] string partner_id, [FromUri] string subscriptions_ids, [FromUri] string user_id = null,
            [FromUri] string coupon_code = null, [FromUri] string udid = null, [FromUri] string language = null, [FromUri] bool should_get_only_lowest = false)
        {
            List<KalturaSubscriptionPrice> subscriptionPrices = null;

            int groupId = int.Parse(partner_id);

            if (string.IsNullOrEmpty(subscriptions_ids))
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "subscriptions_ids cannot be empty");
            }

            List<string> subscriptionsIds = subscriptions_ids.Split(',').Distinct().ToList();

            try
            {
                // call client
                subscriptionPrices = ClientsManager.ConditionalAccessClient().GetSubscriptionsPrices(groupId, subscriptionsIds, user_id, coupon_code, udid, language, should_get_only_lowest);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return new KalturaSubscriptionsPricesList() { SubscriptionsPrices = subscriptionPrices };
        }

        /// <summary>
        /// Returns a list of subscriptions data.
        /// </summary>
        /// <param name="partner_id">Partner identifier</param>
        /// <param name="subscriptions_ids">Subscription identifiers (separated by ,)</param>
        /// <param name="udid">Device UDID</param>
        /// <param name="language">Language code</param>
        /// <remarks>Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008 </remarks>
        public KalturaSubscriptionsList Get(string partner_id, int[] subscriptions_ids, [FromUri] string udid = null, [FromUri] string language = null)
        {
            List<KalturaSubscription> subscruptions = null;

            int groupId = int.Parse(partner_id);

            if (subscriptions_ids.Count() == 0)
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "subscriptions_ids cannot be empty");
            }

            try
            {
                // call client
                subscruptions = ClientsManager.PricingClient().GetSubscriptionsData(groupId, subscriptions_ids.Select(x => x.ToString()).ToList(),
                    udid, language);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return new KalturaSubscriptionsList() { Subscriptions = subscruptions };
        }

        /// <summary>
        /// Returns a list of subscriptions that contain the supplied file
        /// </summary>
        /// <param name="partner_id">Partner identifier</param>
        /// <param name="media_id">Media identifier</param>
        /// <param name="file_id">Media file identifier</param>
        /// <param name="udid">Device UDID</param>
        /// <param name="language">Language code</param>
        /// <remarks>Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, 
        ///Configuration error = 500006, Not found = 500007, Partner is invalid = 500008 </remarks>
        [Route("files/{file_id}/subscriptions"), HttpGet]
        public List<KalturaSubscription> GetSubscriptionIDsContainingMediaFile([FromUri] string partner_id, [FromUri] int media_id, [FromUri] int file_id, [FromUri] string udid = null, [FromUri] string language = null)
        {
            List<KalturaSubscription> subscruptions = null;
            List<int> subscriptionsIds = null;

            int groupId = int.Parse(partner_id);

            if (media_id == 0)
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "media_id cannot be 0");
            }
            if (file_id == 0)
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "file_id cannot be 0");
            }

            try
            {
                // call client
                subscriptionsIds = ClientsManager.PricingClient().GetSubscriptionIDsContainingMediaFile(groupId, media_id, file_id);

                if (subscriptionsIds != null && subscriptionsIds.Count > 0)
                {
                    subscruptions = ClientsManager.PricingClient().GetSubscriptionsData(groupId, subscriptionsIds.Select(id => id.ToString()).ToList(), udid, language);
                }
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return subscruptions;
        }
    }
}