using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Filters;
using WebAPI.Managers.Models;
using WebAPI.Managers.Schema;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Models.General;
using WebAPI.Models.Pricing;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/productPrice/action")]
    public class ProductPriceController : ApiController
    {
        /// <summary>
        /// Returns a price and a purchase status for each subscription or/and media file, for a given user (if passed) and with the consideration of a coupon code (if passed). 
        /// </summary>
        /// <param name="filter">Request filter</param>
        /// <remarks></remarks>
        [Route("list"), HttpPost]
        [ApiAuthorize]
        public KalturaProductPriceListResponse List(KalturaProductPriceFilter filter)
        {
            List<KalturaProductPrice> productPrices = new List<KalturaProductPrice>();
            List<KalturaSubscriptionPrice> subscriptionPrices = new List<KalturaSubscriptionPrice>();
            List<KalturaPpvPrice> ppvPrices = new List<KalturaPpvPrice>();

            int groupId = KS.GetFromRequest().GroupId;
            string udid = KSUtils.ExtractKSPayload().UDID;
            string language = Utils.Utils.GetLanguageFromRequest();
            
            if (filter == null)
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "filter cannot be empty");
            }

            if ((filter.SubscriptionIdIn == null || filter.SubscriptionIdIn.Count() == 0) && (filter.FileIdIn == null || filter.FileIdIn.Count() == 0))
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "at least one of subscriptions_ids and files_ids must not be empty");
            }
            try
            {
                if (filter.SubscriptionIdIn != null && filter.SubscriptionIdIn.Count() > 0)
                {
                    // call client
                    subscriptionPrices = ClientsManager.ConditionalAccessClient().GetSubscriptionsPrices(groupId, filter.getSubscriptionIdIn(), KS.GetFromRequest().UserId, filter.CouponCodeEqual,
                        udid, language, filter.getShouldGetOnlyLowest());
                    productPrices.AddRange(subscriptionPrices);
                }

                if (filter.FileIdIn != null && filter.FileIdIn.Count() > 0)
                {
                    // call client
                    ppvPrices = ClientsManager.ConditionalAccessClient().GetPpvPrices(groupId, filter.getFileIdIn(), KS.GetFromRequest().UserId, filter.CouponCodeEqual,
                        udid, language, filter.getShouldGetOnlyLowest());
                    productPrices.AddRange(ppvPrices);
                }

                // order
                switch (filter.OrderBy)
                {
                    case KalturaProductPriceOrderBy.PRODUCT_ID_ASC:
                        productPrices = productPrices.OrderBy(p => p.ProductId).ToList();
                        break;
                    case KalturaProductPriceOrderBy.PRODUCT_ID_DESC:
                        productPrices = productPrices.OrderByDescending(p => p.ProductId).ToList();
                        break;
                    default:
                        break;
                }
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return new KalturaProductPriceListResponse() { ProductsPrices = productPrices, TotalCount = productPrices.Count() };
        }
    }
}