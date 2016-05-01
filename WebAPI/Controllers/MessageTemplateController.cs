using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.General;
using WebAPI.Models.Notification;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/messageTemplate/action")]
    public class MessageTemplateController : ApiController
    {
        /// <summary>
        /// Set message template for message notification 
        /// </summary>
        /// <param name="message_template">The actual message with placeholders to be presented to the user</param>       
        /// <returns></returns>
        /// <remarks>
        /// Possible status codes: message place holders invalid = 8014, url placeholders invalid = 8017
        /// </remarks>
        [Route("set"), HttpPost]
        [ApiAuthorize]
        public KalturaMessageTemplate Set(KalturaMessageTemplate message_template)
        {
            KalturaMessageTemplate response = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                if (string.IsNullOrEmpty(message_template.Message))
                {
                    throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "message is empty");
                }

                if (string.IsNullOrEmpty(message_template.DateFormat))
                {
                    throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "date_format is empty");
                }

                // call client
                response = ClientsManager.NotificationClient().SetMessageTemplate(groupId, message_template);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        ///  Get message template
        /// </summary>
        /// <param name="asset_Type">possible values: Asset type – Series</param>
        /// <returns></returns>     
        /// <remarks>
        /// Possible status codes: message template not found = 8016
        /// </remarks>
        [Route("get"), HttpPost]
        [ApiAuthorize]
        public KalturaMessageTemplate Get(KalturaOTTAssetType asset_Type)
        {
            KalturaMessageTemplate response = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.NotificationClient().GetMessageTemplate(groupId, asset_Type);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

    }
}