using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.Pricing;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/price/action")]
    public class PriceController : ApiController
    {
        /// <summary>
        /// Returns the price details and purchase details for each file, for a given user (if passed) and with the consideration of a coupon code (if passed). 
        /// </summary>
        /// <param name="files_ids">Media files identifiers (separated by ',')</param>
        /// <param name="coupon_code">Discount coupon code</param>
        /// <param name="udid">Device UDID</param>
        /// <param name="language">Language code</param>
        /// <param name="should_get_only_lowest">A flag that indicates if only the lowest price of an item should return</param>
        /// <remarks>Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008 </remarks>
        [Route("get"), HttpPost]
        [ApiAuthorize(true)]
        public KalturaItemPriceListResponse Get(string files_ids, string coupon_code = null, string udid = null, string language = null, bool should_get_only_lowest = false)
        {
            List<KalturaItemPrice> ppvPrices = null;

            int groupId = KS.GetFromRequest().GroupId;

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
                ppvPrices = ClientsManager.ConditionalAccessClient().GetItemsPrices(groupId, filesIds, KS.GetFromRequest().UserId, coupon_code, udid, language, should_get_only_lowest);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return new KalturaItemPriceListResponse() { ItemPrice = ppvPrices, TotalCount = ppvPrices.Count };
        }
    }
}