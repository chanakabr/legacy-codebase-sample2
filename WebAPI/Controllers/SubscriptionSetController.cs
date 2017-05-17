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

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/subscriptionSet/action")]
    public class SubscriptionSetController : ApiController
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
       
        /// <summary>
        /// Returns a list of subscriptionSets requested by ids or subscription ids
        /// </summary>
        /// <param name="filter">SubscriptionSet filter</param>
        /// <remarks></remarks>
        [Route("list"), HttpPost]
        [ApiAuthorize]
        public KalturaSubscriptionSetListResponse List(KalturaSubscriptionSetFilter filter = null)
        {
            if (filter == null)
            {
                filter = new KalturaSubscriptionSetFilter();
            }
            else
            {
                filter.Validate();
            }

            KalturaSubscriptionSetListResponse response = new KalturaSubscriptionSetListResponse();            
            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                if (!string.IsNullOrEmpty(filter.SubscriptionIdContains))
                {
                    // call client
                    response = ClientsManager.PricingClient().GetSubscriptionSetsBySubscriptionIds(groupId, filter.GetSubscriptionIdContains(), filter.OrderBy);  
                }
                else
                {
                    // call client
                    response = ClientsManager.PricingClient().GetSubscriptionSets(groupId, filter.GetIdIn(), filter.OrderBy);
                }
                
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }       

    }
}