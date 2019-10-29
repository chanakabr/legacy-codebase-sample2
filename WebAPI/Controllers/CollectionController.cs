using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Models.Pricing;
using WebAPI.Utils;
using WebAPI.Managers.Models;
using WebAPI.Models.General;
using WebAPI.Managers.Scheme;

namespace WebAPI.Controllers
{
    [Service("collection")]
    public class CollectionController : IKalturaController
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        /// <summary>
        /// Returns a list of subscriptions requested by Subscription ID or file ID
        /// </summary>
        /// <param name="filter">Filter request</param>
        /// <param name="pager">Page size and index</param>
        /// <remarks>Possible status codes:      
        ///   </remarks>
        [Action("list")]
        [ApiAuthorize]
        static public KalturaCollectionListResponse List(KalturaCollectionFilter filter = null, KalturaFilterPager pager = null)
        {
            KalturaCollectionListResponse response = new KalturaCollectionListResponse();

            if (pager == null)
            {
                pager = new KalturaFilterPager();
            }

            if (filter == null)
            {
                filter = new KalturaCollectionFilter();
            }
            else
            {
                filter.Validate();
            }
            

            int groupId = KS.GetFromRequest().GroupId;
            string udid = KSUtils.ExtractKSPayload().UDID;
            string language = Utils.Utils.GetLanguageFromRequest();

            try
            {
                if (!string.IsNullOrEmpty(filter.CollectionIdIn))
                {
                    response.Collections = ClientsManager.PricingClient().GetCollectionsData(groupId, filter.getCollectionIdIn(), udid, language, filter.OrderBy, pager.getPageIndex(), pager.PageSize, filter.CouponGroupIdEqual);
                }
                else if (filter.MediaFileIdEqual.HasValue)
                {
                    List<int> collectionsIds = ClientsManager.PricingClient().GetCollectionIdsContainingMediaFile(groupId, filter.MediaFileIdEqual.Value);
                    
                    // get collections
                    if (collectionsIds != null && collectionsIds.Count > 0)
                    {
                        response.Collections = ClientsManager.PricingClient().GetCollectionsData(groupId, collectionsIds.Select(id => id.ToString()).ToArray(), udid, language, filter.OrderBy, pager.getPageIndex(), pager.PageSize, filter.CouponGroupIdEqual);
                    }
                }
                else
                {
                    response.Collections = ClientsManager.PricingClient().GetCollectionsData(groupId, udid, language, filter.OrderBy, pager.getPageIndex(), pager.PageSize, filter.CouponGroupIdEqual);
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