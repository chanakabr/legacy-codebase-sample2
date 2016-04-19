using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.General;
using WebAPI.Models.Notification;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/FollowTemplate/action")]
    public class FollowTemplateController : ApiController
    {
        /// <summary>
        /// Set follow template for follow notification 
        /// </summary>
        /// <param name="follow_template">The actual message with placeholders to be presented to the user</param>       
        /// <returns></returns>
        /// <remarks>
        /// Possible status codes: Invalid place-holders = 8014, Date time format is invalid  = 8015
        /// </remarks>
        [Route("set"), HttpPost]
        [ApiAuthorize]
        public KalturaFollowTemplate Set(KalturaFollowTemplate follow_template)
        {
            KalturaFollowTemplate response = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                if (string.IsNullOrEmpty(follow_template.Message))
                {
                    throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "message is empty");
                }

                if (string.IsNullOrEmpty(follow_template.DateFormat))
                {
                    throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "date_format is empty");
                }

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
        ///  Get follow template
        /// </summary>
        /// <param name="asset_Type">possible values: Asset type – Series</param>
        /// <returns></returns>     
        /// <remarks>
        /// Possible status codes: Follow template not found = 8016
        /// </remarks>
        [Route("get"), HttpPost]
        [ApiAuthorize]
        public KalturaFollowTemplate Get(KalturaOTTAssetType asset_Type)
        {
            KalturaFollowTemplate response = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.NotificationClient().GetFollowTemplate(groupId, asset_Type);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

    }
}