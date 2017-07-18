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
    [RoutePrefix("_service/priceDetails/action")]
    public class PriceDetailsController : ApiController
    {
        /// <summary>
        /// Returns the list of available prices, can be filtered by price IDs
        /// </summary>
        /// <param name="filter">Filter</param>
        /// <returns></returns>
        [Route("list"), HttpPost]
        [ApiAuthorize]
        [Throws(eResponseStatus.InvalidCurrency)]
        public KalturaPriceDetailsListResponse List(KalturaPriceDetailsFilter filter = null)
        {
            int groupId = KS.GetFromRequest().GroupId;
            string currency = Utils.Utils.GetCurrencyFromRequest();
            List<KalturaPriceDetails> prices = null;

            try
            {
                List<long> priceIds = null;
                
                prices = ClientsManager.PricingClient().GetPrices(groupId, filter != null ? filter.GetIdIn() : null, currency);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return new KalturaPriceDetailsListResponse() { Prices = prices, TotalCount = prices != null ? prices.Count : 0 };
        }
    }
}