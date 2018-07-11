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
using WebAPI.Managers.Scheme;
using ApiObjects.Response;

namespace WebAPI.Controllers
{
    [Service("pricePlan")]
    public class PricePlanController : IKalturaController
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
       
        /// <summary>
        /// Returns a list of price plans by IDs
        /// </summary>
        /// <param name="filter">Filter request</param>
        [Action("list")]
        [ApiAuthorize]
        static public KalturaPricePlanListResponse List(KalturaPricePlanFilter filter = null)
        {
            int groupId = KS.GetFromRequest().GroupId;
            
            List<KalturaPricePlan> pricePlans = null;

            try
            {
                List<long> priceIds = null;
                if (filter != null)
                {
                    priceIds = filter.GetIdIn();
                }

                pricePlans = ClientsManager.PricingClient().GetPricePlans(groupId, priceIds);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return new KalturaPricePlanListResponse() { PricePlans = pricePlans, TotalCount = pricePlans != null ? pricePlans.Count : 0 };
        }

        /// <summary>
        /// Updates a price plan
        /// </summary>
        /// <param name="pricePlan">Price plan to update</param>
        /// <param name="id">Price plan ID</param>
        [Action("update")]
        [ApiAuthorize]
        [Throws(eResponseStatus.PricePlanDoesNotExist)]
        [Throws(eResponseStatus.PriceDetailsDoesNotExist)]
        static public KalturaPricePlan Update(long id, KalturaPricePlan pricePlan)
        {
            int groupId = KS.GetFromRequest().GroupId;

            if (id == 0)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "id");
            }

            try
            {
                pricePlan = ClientsManager.PricingClient().UpdatePricePlan(groupId, id, pricePlan);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return pricePlan;
        }
    }
}