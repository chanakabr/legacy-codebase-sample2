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
using WebAPI.Models.ConditionalAccess;
using WebAPI.Models.General;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/subscription/action")]
    public class SubscriptionController : ApiController
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        /// <summary>
        /// Returns a price and a purchase status for each subscription, for a given user (if passed) and with the consideration of a coupon code (if passed). 
        /// </summary>
        /// <param name="subscriptions_ids">Subscription identifiers (separated by ',')</param>
        /// <param name="coupon_code">Discount coupon code</param>
        /// <param name="udid">Device UDID</param>
        /// <param name="language">Language code</param>
        /// <param name="should_get_only_lowest">A flag that indicates if only the lowest price of a subscription should return</param>
        /// <remarks>Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008 </remarks>
        [Route("getPrices"), HttpPost]
        [ApiAuthorize]
        public KalturaSubscriptionsPriceListResponse GetSubscriptionsPrices(string subscriptions_ids, string coupon_code = null, string udid = null, string language = null, 
            bool should_get_only_lowest = false)
        {
            List<KalturaSubscriptionPrice> subscriptionPrices = null;

            int groupId = KS.GetFromRequest().GroupId;

            if (string.IsNullOrEmpty(subscriptions_ids))
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "subscriptions_ids cannot be empty");
            }

            List<string> subscriptionsIds = subscriptions_ids.Split(',').Distinct().ToList();

            try
            {
                // call client
                subscriptionPrices = ClientsManager.ConditionalAccessClient().GetSubscriptionsPrices(groupId, subscriptionsIds, KS.GetFromRequest().UserId, coupon_code, udid, language, should_get_only_lowest);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return new KalturaSubscriptionsPriceListResponse() { SubscriptionsPrices = subscriptionPrices, TotalCount = subscriptionPrices.Count };
        }

        /// <summary>
        /// Returns a list of subscriptions data.
        /// </summary>
        /// <param name="subscriptions_ids">Subscription identifiers</param>
        /// <param name="udid">Device UDID</param>
        /// <param name="language">Language code</param>
        /// <remarks>Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008 </remarks>
        [Route("get"), HttpPost]
        [ApiAuthorize(true)]
        public KalturaSubscriptionListResponse Get(KalturaIntegerValue[] subscriptions_ids, string udid = null, string language = null)
        {
            List<KalturaSubscription> subscriptions = null;

            int groupId = KS.GetFromRequest().GroupId;

            if (subscriptions_ids.Count() == 0)
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "subscriptions_ids cannot be empty");
            }

            try
            {
                // call client
                subscriptions = ClientsManager.PricingClient().GetSubscriptionsData(groupId, subscriptions_ids.Select(x => x.value.ToString()).ToList(),
                    udid, language);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return new KalturaSubscriptionListResponse() { Subscriptions = subscriptions, TotalCount = subscriptions.Count };
        }

        /// <summary>
        /// Returns a list of subscriptions that contain the supplied file
        /// </summary>
        /// <param name="media_id">Media identifier</param>
        /// <param name="file_id">Media file identifier</param>
        /// <param name="udid">Device UDID</param>
        /// <param name="language">Language code</param>
        /// <remarks>Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, 
        ///Configuration error = 500006, Not found = 500007, Partner is invalid = 500008 </remarks>
        [Route("getSubscriptionsContainingMediaFile"), HttpPost]
        [ApiAuthorize(true)]
        public List<KalturaSubscription> GetSubscriptionsContainingMediaFile(int media_id, int file_id, string udid = null, string language = null)
        {
            List<KalturaSubscription> subscruptions = null;
            List<int> subscriptionsIds = null;

            int groupId = KS.GetFromRequest().GroupId;

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

        /// <summary>
        /// Charges a user for subscription        
        /// </summary>
        /// <remarks>
        /// Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008,
        /// Price not correct = 6000, Unknown PPV module = 6001, Expired credit card = 6002, Cellular permissions error (for cellular charge) = 6003, Unknown billing provider = 6004
        /// </remarks>
        /// <param name="udid">Device UDID</param>
        /// <param name="sub_id">Subscription identifier</param>
        /// <param name="request">Charge request parameters</param>
        [Route("buy"), HttpPost]
        [Obsolete]
        [ApiAuthorize]
        public KalturaBillingResponse Buy(string sub_id, KalturaCharge request, [FromUri]string udid = null)
        {
            KalturaBillingResponse response = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.ConditionalAccessClient().ChargeUserForSubscription(groupId, request.UserId, request.Price, request.Currency, sub_id, request.CouponCode,
                    request.ExtraParams, udid, request.EncryptedCvv);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}