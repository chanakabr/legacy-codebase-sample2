using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Filters;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Models.General;
using WebAPI.Models.Pricing;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/price/action")]
    [Obsolete]
    public class PriceController : ApiController
    {

        /// <summary>
        /// Returns a price and a purchase status for each subscription or/and media file, for a given user (if passed) and with the consideration of a coupon code (if passed). 
        /// </summary>
        /// <param name="filter">Request filter</param>
        /// <param name="coupon_code">Discount coupon code</param>
        /// <remarks></remarks>
        [Route("list"), HttpPost]
        [ApiAuthorize]
        [Obsolete]
        public KalturaProductsPriceListResponse List(KalturaPricesFilter filter, string coupon_code = null)
        {
            List<KalturaProductPrice> productPrices = new List<KalturaProductPrice>();
            List<KalturaSubscriptionPrice> subscriptionPrices = new List<KalturaSubscriptionPrice>();
            List<KalturaItemPrice> ppvPrices = new List<KalturaItemPrice>(); ;

            int groupId = KS.GetFromRequest().GroupId;
            string udid = KSUtils.ExtractKSPayload().UDID;
            string language = Utils.Utils.GetLanguageFromRequest();
            
            if ((filter.SubscriptionsIds == null || filter.SubscriptionsIds.Count() == 0) && (filter.FilesIds == null || filter.FilesIds.Count() == 0))
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_CANNOT_BE_EMPTY, "KalturaPricesFilter.subscriptionsIds, KalturaPricesFilter.filesIds");
            }
            try
            {
                if (filter.SubscriptionsIds != null && filter.SubscriptionsIds.Count() > 0)
                {
                    // call client
                    subscriptionPrices = ClientsManager.ConditionalAccessClient().GetSubscriptionsPrices(groupId, filter.SubscriptionsIds.Select(x => x.value), KS.GetFromRequest().UserId, coupon_code,
                        udid, language, filter.getShouldGetOnlyLowest());
                    productPrices.AddRange(subscriptionPrices);
                }

                if (filter.FilesIds != null && filter.FilesIds.Count() > 0)
                {
                    // call client
                    ppvPrices = ClientsManager.ConditionalAccessClient().GetItemsPrices(groupId, filter.FilesIds.Select(x => x.value).ToList(), KS.GetFromRequest().UserId, coupon_code,
                        udid, language, filter.getShouldGetOnlyLowest());
                    productPrices.AddRange(ppvPrices);
                }
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return new KalturaProductsPriceListResponse() { ProductsPrices = productPrices, TotalCount = productPrices.Count() };
        }
    }
}