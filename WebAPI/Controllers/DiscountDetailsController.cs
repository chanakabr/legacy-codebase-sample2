using ApiObjects.Response;
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
    [Service("discountDetails")]
    public class DiscountDetailsController : IKalturaController
    {
        /// <summary>
        /// Returns the list of available discounts details, can be filtered by discount codes
        /// </summary>
        /// <param name="filter">Filter</param>
        /// <returns></returns>
        [Action("list")]
        [ApiAuthorize]
        [Throws(eResponseStatus.InvalidCurrency)]
        static public KalturaDiscountDetailsListResponse List(KalturaDiscountDetailsFilter filter = null)
        {
            int groupId = KS.GetFromRequest().GroupId;
            string currency = Utils.Utils.GetCurrencyFromRequest();
            List<KalturaDiscountDetails> discounts = null;

            try
            {
                List<long> priceIds = null;
                
                discounts = ClientsManager.PricingClient().GetDiscounts(groupId, filter != null ? filter.GetIdIn() : null, currency);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return new KalturaDiscountDetailsListResponse() { Discounts = discounts, TotalCount = discounts != null ? discounts.Count : 0 };
        }
    }
}