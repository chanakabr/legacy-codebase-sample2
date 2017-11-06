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
    [RoutePrefix("_service/collection/action")]
    public class CollectionController : ApiController
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
        public KalturaCollectionListResponse List(KalturaCollectionFilter filter)
        {
            KalturaCollectionListResponse response = new KalturaCollectionListResponse();

            filter.Validate();

            int groupId = KS.GetFromRequest().GroupId;
            string udid = KSUtils.ExtractKSPayload().UDID;
            string language = Utils.Utils.GetLanguageFromRequest();

            try
            {
                if (!string.IsNullOrEmpty(filter.CollectionIdIn))
                {
                    response.Collections = ClientsManager.PricingClient().GetCollectionsData(groupId, filter.getCollectionIdIn(), udid, language, filter.OrderBy);
                }
                else if (filter.MediaFileIdEqual.HasValue)
                {
                    List<int> collectionsIds = ClientsManager.PricingClient().GetCollectionIdsContainingMediaFile(groupId, filter.MediaFileIdEqual.Value);
                    
                    // get collections
                    if (collectionsIds != null && collectionsIds.Count > 0)
                    {
                        response.Collections = ClientsManager.PricingClient().GetCollectionsData(groupId, collectionsIds.Select(id => id.ToString()).ToArray(), udid, language, filter.OrderBy);
                    }
                }

                response.TotalCount = response.Collections != null ? response.Collections.Count : 0;
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}