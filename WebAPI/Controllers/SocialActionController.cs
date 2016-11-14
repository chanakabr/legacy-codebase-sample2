using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;
using WebAPI.Models.Social;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/socialAction/action")]
    public class SocialActionController : ApiController
    {
        /// <summary>
        /// Insert new user social action
        /// </summary>
        /// <param name="socialAction">social Action Object</param>
        /// <remarks>
        /// Possible status codes:
        /// </remarks>       
        [Route("add"), HttpPost]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_RETURN_TYPE)]
        public KalturaUserSocialActionResponse Add(KalturaSocialAction socialAction)
        {
            KalturaUserSocialActionResponse response = null;

            int groupId = KS.GetFromRequest().GroupId;
            string userId = KS.GetFromRequest().UserId;
            string udid = KSUtils.ExtractKSPayload().UDID;

            try
            {
                // call client
                response = ClientsManager.SocialClient().AddSocialAction(groupId, userId, udid, socialAction);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

         /// <summary>
        /// Get list of user social actions
        /// </summary>
        /// <param name="filter">social action filter</param>
        ///<param name="pager">pager </param>
        /// <remarks>
        /// Possible status codes:
        /// </remarks>
        [Route("list"), HttpPost]
        [ApiAuthorize]
        public KalturaSocialActionListResponse List(KalturaSocialActionFilter filter = null, KalturaFilterPager pager = null)
        {
            KalturaSocialActionListResponse response = null;

            int groupId = KS.GetFromRequest().GroupId;
            string userId = KS.GetFromRequest().UserId;

             // parameters validation
            if (pager == null)
                pager = new KalturaFilterPager();
          
            try
            {          
                // call client
                response = ClientsManager.SocialClient().GetUserSocialActions(groupId, userId, filter.getAssetIdIn(), filter.AssetTypeEqual, filter.GetActionTypeIn(), pager.getPageIndex(), pager.PageSize, filter.OrderBy);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
        

    }
}