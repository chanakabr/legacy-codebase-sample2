using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Models.Pricing;
using WebAPI.Utils;
using WebAPI.Managers.Models;
using System.Web.Http.Description;

namespace WebAPI.Controllers
{
    [RoutePrefix("pricing")]
    public class PricingController : ApiController
    {
        /// <summary>
        /// Returns the price details and purchase details for each file, for a given user (if passed) and with the consideration of a coupon code (if passed). 
        /// </summary>
        /// <param name="partner_id">Partner identifier</param>
        /// <param name="files_ids">Media files identifiers (separated by ',')</param>
        /// <param name="user_id">User identifier</param>
        /// <param name="coupon_code">Discount coupon code</param>
        /// <param name="udid">Device UDID</param>
        /// <param name="language">Language code</param>
        /// <param name="should_get_only_lowest">A flag that indicates if only the lowest price of an item should return</param>
        /// <remarks>Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008 </remarks>
        [Route("files/{files_ids}/prices"), HttpGet]
        public KalturaItemPricesList GetItemsPrices([FromUri] string partner_id, [FromUri] string files_ids, [FromUri] string user_id = null,
            [FromUri] string coupon_code = null, [FromUri] string udid = null, [FromUri] string language = null, [FromUri] bool should_get_only_lowest = false)
        {
            List<KalturaItemPrice> ppvPrices = null;

            int groupId = int.Parse(partner_id);

            if (string.IsNullOrEmpty(files_ids))
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "files_ids cannot be empty");
            }

            List<int> filesIds;
            try
            {
                filesIds = files_ids.Split(',').Distinct().Select(f => int.Parse(f)).ToList();
            }
            catch (Exception)
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "each file id must be integer");
            }

            try
            {
                // call client
                ppvPrices = ClientsManager.ConditionalAccessClient().GetItemsPrices(groupId, filesIds, user_id, coupon_code, udid, language, should_get_only_lowest);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return new KalturaItemPricesList() { ItemPrice = ppvPrices };
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

        [Route("files/{file_id}/subscriptions"), HttpPost]
        [ApiExplorerSettings(IgnoreApi = true)]
        public List<KalturaSubscription> _GetSubscriptionIDsContainingMediaFile([FromUri] string partner_id, [FromUri] int media_id, [FromUri] int file_id, [FromUri] string udid = null, [FromUri] string language = null)
        {
           return GetSubscriptionIDsContainingMediaFile(partner_id, media_id, file_id, udid, language);
        }      


    }
}