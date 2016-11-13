using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.API;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Models.General;
using WebAPI.Models.Social;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/socialFriendActivity/action")]
    public class SocialFriendActivityController : ApiController
    {
         private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        /// <summary>
         /// Get a list of the social friends activity for a user
        /// </summary>
        /// <param name="filter">Social friend activity filter</param>
        /// <remarks></remarks>
         [Route("list"), HttpPost]
         [ApiAuthorize]
         public KalturaSocialFriendActivityListResponse List(KalturaSocialFriendActivityFilter filter = null, KalturaFilterPager pager = null)
         {
             KalturaSocialFriendActivityListResponse response = null;

             if (filter == null)
             {
                 filter = new KalturaSocialFriendActivityFilter();
             }

             if (pager == null)
             {
                 pager = new KalturaFilterPager();
             }

             filter.validate();
             int groupId = KS.GetFromRequest().GroupId;

             try
             {
                 response = ClientsManager.SocialClient().GetFriendsActions(groupId, KS.GetFromRequest().UserId, filter.AssetIdEqual.HasValue ? filter.AssetIdEqual.Value : 0, 
                     filter.AssetTypeEqual.HasValue ? filter.AssetTypeEqual.Value : 0, filter.GetActionTypeIn(), pager.getPageSize(), pager.getPageIndex());
             }
             catch (ClientException ex)
             {
                 ErrorUtils.HandleClientException(ex);
             }

             return response;
         }
    }
}