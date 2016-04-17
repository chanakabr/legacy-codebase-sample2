using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.Notification;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/FollowTemplate/action")]
    public class FollowTemplateController : ApiController
    {
        /// <summary>
        /// TBD
        /// </summary>
        /// <param name="follow_template">TBD</param>
        /// <remarks>
        /// 
        /// </remarks>
        /// <returns></returns>
        [Route("set"), HttpPost]
        [ApiAuthorize]
        public KalturaFollowTemplate Set(KalturaFollowTemplate follow_template)
        {
            KalturaFollowTemplate response = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.NotificationClient().SetFollowTemplate(groupId, follow_template);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// TBD
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <returns></returns>
        [Route("Get"), HttpPost]
        [ApiAuthorize]
        public KalturaFollowTemplate Get()
        {
            KalturaFollowTemplate response = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.NotificationClient().GetFollowTemplate(groupId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

    }
}