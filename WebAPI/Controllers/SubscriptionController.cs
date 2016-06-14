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
        /// Returns a list of subscriptions requested by Subscription ID or file ID
        /// </summary>
        /// <param name="filter">Filter request</param>
        /// <remarks>Possible status codes:      
        ///   </remarks>
        [Route("list"), HttpPost]
        [ApiAuthorize]
        public List<KalturaSubscription> List(KalturaSubscriptionsFilter filter)
        {
            List<KalturaSubscription> subscruptions = null;
            List<int> subscriptionsIds = null;

            int groupId = KS.GetFromRequest().GroupId;
            string udid = KSUtils.ExtractKSPayload().UDID;
            string language = Utils.Utils.GetLanguageFromRequest();

            if (filter == null)
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "filter cannot be empty");
            }

            if (filter.Ids == null || filter.Ids.Count() == 0)
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "filter ids cannot be empty");
            }

            try
            {
                if (filter.By == KalturaSubscriptionsFilterBy.media_file_id)
                {
                    // call client
                    subscriptionsIds = ClientsManager.PricingClient().GetSubscriptionIDsContainingMediaFile(groupId, filter.Ids[0].value);

                    // get subscriptions
                    if (subscriptionsIds != null && subscriptionsIds.Count > 0)
                    {
                        subscruptions = ClientsManager.PricingClient().GetSubscriptionsData(groupId, subscriptionsIds.Select(id => id.ToString()).ToList(), udid, language);
                    }
                }

                else if (filter.By == KalturaSubscriptionsFilterBy.subscriptions_ids)
                {
                    // call client
                    subscruptions = ClientsManager.PricingClient().GetSubscriptionsData(groupId, filter.Ids.Select(x => x.value.ToString()).ToList(), udid, language);
                }
                
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return subscruptions;
        }
    }
}