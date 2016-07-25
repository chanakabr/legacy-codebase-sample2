using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Schema;
using WebAPI.Models.Catalog;
using WebAPI.Models.General;
using WebAPI.Models.Notification;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/personalFeed/action")]
    [OldStandardAction("listOldStandard", "list")]
    public class PersonalFeedController : ApiController
    {
        /// <summary>
        /// List user's feeds.
        /// <remarks>Possible status codes:</remarks>
        /// </summary>
        /// <param name="filter">Required sort option to apply for the identified assets. If omitted – will use relevancy.
        /// Possible values: relevancy, a_to_z, z_to_a, views, ratings, votes, newest.</param>        
        /// <param name="pager">Page size and index</param>
        /// <returns></returns>
        [Route("list"), HttpPost]
        [ApiAuthorize]
        public KalturaPersonalFeedListResponse List(KalturaPersonalFeedFilter filter = null, KalturaFilterPager pager = null)
        {
            KalturaPersonalFeedListResponse response = null;

            int groupId = KS.GetFromRequest().GroupId;
            string userID = KS.GetFromRequest().UserId;

            if (pager == null)
                pager = new KalturaFilterPager();

            try
            {
                response = ClientsManager.NotificationClient().GetUserFeedList(groupId, userID, pager.getPageSize(), pager.getPageIndex(), filter.OrderBy);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// List user's feeds.
        /// <remarks>Possible status codes:</remarks>
        /// </summary>
        /// <param name="order_by">Required sort option to apply for the identified assets. If omitted – will use relevancy.
        /// Possible values: relevancy, a_to_z, z_to_a, views, ratings, votes, newest.</param>        
        /// <param name="pager">Page size and index</param>
        /// <returns></returns>
        [Route("listOldStandard"), HttpPost]
        [ApiAuthorize]
        [Obsolete]
        public KalturaPersonalFollowFeedResponse ListOldStandard(KalturaOrder? order_by = null, KalturaFilterPager pager = null)
        {
            KalturaPersonalFollowFeedResponse response = null;

            int groupId = KS.GetFromRequest().GroupId;
            string userID = KS.GetFromRequest().UserId;

            if (pager == null)
                pager = new KalturaFilterPager();
            
            try
            {
                response = ClientsManager.NotificationClient().GetUserFeeder(groupId, userID, pager.getPageSize(), pager.getPageIndex(), order_by);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}