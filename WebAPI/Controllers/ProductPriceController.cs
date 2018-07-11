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
    [Service("productPrice")]
    public class ProductPriceController : IKalturaController
    {
        /// <summary>
        /// Returns a price and a purchase status for each subscription or/and media file, for a given user (if passed) and with the consideration of a coupon code (if passed). 
        /// </summary>
        /// <param name="filter">Request filter</param>
        /// <remarks></remarks>
        [Action("list")]
        [ApiAuthorize]
        static public KalturaProductPriceListResponse List(KalturaProductPriceFilter filter)
        {
            List<KalturaProductPrice> productPrices = new List<KalturaProductPrice>();
            List<KalturaSubscriptionPrice> subscriptionPrices = new List<KalturaSubscriptionPrice>();
            List<KalturaPpvPrice> ppvPrices = new List<KalturaPpvPrice>();
            List<KalturaCollectionPrice> collectiontPrices = new List<KalturaCollectionPrice>();

            filter.Validate();

            int groupId = KS.GetFromRequest().GroupId;
            string udid = KSUtils.ExtractKSPayload().UDID;
            string language = Utils.Utils.GetLanguageFromRequest();
            string currency = Utils.Utils.GetCurrencyFromRequest();
            string userId = KS.GetFromRequest().UserId;

            try
            {
                if (filter.SubscriptionIdIn != null && filter.SubscriptionIdIn.Count() > 0)
                {
                    subscriptionPrices = ClientsManager.ConditionalAccessClient().GetSubscriptionsPrices(groupId, filter.getSubscriptionIdIn(), userId, filter.CouponCodeEqual,
                                                                                                            udid, language, filter.getShouldGetOnlyLowest(), currency);
                    productPrices.AddRange(subscriptionPrices);
                }

                if (filter.FileIdIn != null && filter.FileIdIn.Count() > 0)
                {
                    ppvPrices = ClientsManager.ConditionalAccessClient().GetPpvPrices(groupId, filter.getFileIdIn(), userId, filter.CouponCodeEqual, udid, language,
                                                                                        filter.getShouldGetOnlyLowest(), currency);
                    productPrices.AddRange(ppvPrices);
                }

                if (filter.CollectionIdIn != null && filter.CollectionIdIn.Count() > 0)
                {
                    collectiontPrices = ClientsManager.ConditionalAccessClient().GetCollectionPrices(groupId, filter.getCollectionIdIn(), userId, filter.CouponCodeEqual, udid, language,
                                                                                        filter.getShouldGetOnlyLowest(), currency);
                    productPrices.AddRange(collectiontPrices);
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