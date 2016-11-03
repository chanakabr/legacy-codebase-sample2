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
    [RoutePrefix("_service/socialComment/action")]
    public class SocialCommentController : ApiController
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        /// <summary>
        /// Get a list of all social comments filtered by asset ID and social platform
        /// </summary>
        /// <param name="filter">Country filter</param>
        /// <remarks></remarks>
        [Route("list"), HttpPost]
        [ApiAuthorize]
        public KalturaSocialCommentListResponse List(KalturaSocialCommentFilter filter = null, KalturaFilterPager pager = null)
        {
            KalturaSocialCommentListResponse response = null;

            if (filter == null)
            {
                filter = new KalturaSocialCommentFilter();
            }

            if (pager == null)
            {
                pager = new KalturaFilterPager();
            }

            filter.validate();
            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                //response = ClientsManager.SocialClient().GetFriendsActions(groupId, KS.GetFromRequest().UserId, filter.AssetIdEqual.HasValue ? filter.AssetIdEqual.Value : 0,
                //    filter.AssetTypeEqual.HasValue ? filter.AssetTypeEqual.Value : 0, filter.GetActionTypeIn(), pager.getPageSize(), pager.getPageIndex());
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}