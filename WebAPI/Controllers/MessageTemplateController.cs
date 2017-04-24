using System;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;
using WebAPI.Models.Notification;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/messageTemplate/action")]
    public class MessageTemplateController : ApiController
    {
        /// <summary>
        /// Set the account’s push notifications and inbox messages templates
        /// </summary>
        /// <param name="message_template">The actual message with placeholders to be presented to the user</param>       
        /// <returns></returns>
        /// <remarks>
        /// Possible status codes: message place holders invalid = 8014, url placeholders invalid = 8017
        /// </remarks>
        [Route("set"), HttpPost]
        [ApiAuthorize]
        [Obsolete]
        public KalturaMessageTemplate Set(KalturaMessageTemplate message_template)
        {
            KalturaMessageTemplate response = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                if (string.IsNullOrEmpty(message_template.Message))
                {
                    throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "KalturaMessageTemplate.message");
                }

                if (string.IsNullOrEmpty(message_template.DateFormat))
                {
                    throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "KalturaMessageTemplate.dateFormat");
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
        /// Set the account’s push notifications and inbox messages templates
        /// </summary>
        /// <param name="assetType">The asset type to update its template</param>  
        /// <param name="template">The actual message with placeholders to be presented to the user</param>       
        /// <returns></returns>
        /// <remarks>
        /// Possible status codes: message place holders invalid = 8014, url placeholders invalid = 8017
        /// </remarks>
        [Route("update"), HttpPost]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_ARGUMENTS)]
        public KalturaMessageTemplate Update(KalturaOTTAssetType assetType, KalturaMessageTemplate template)
        {
            KalturaMessageTemplate response = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                template.AssetType = assetType;

                if (string.IsNullOrEmpty(template.Message))
                {
                    throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "KalturaMessageTemplate.message");
                }

                if (string.IsNullOrEmpty(template.DateFormat))
                {
                    throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "KalturaMessageTemplate.dateFormat");
                }

                // call client
                response = ClientsManager.NotificationClient().SetMessageTemplate(groupId, template);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Retrieve a message template used in push notifications and inbox
        /// </summary>
        /// <param name="messageType">possible values: Asset type – Series</param>
        /// <returns></returns>     
        /// <remarks>
        /// Possible status codes: message template not found = 8016
        /// </remarks>
        [Route("get"), HttpPost]
        [ApiAuthorize]
        [OldStandardArgument("messageType", "asset_Type")]
        [OldStandardArgument("messageType", "assetType", "3.6.2094.15157")]
        [ValidationException(SchemeValidationType.ACTION_ARGUMENTS)]
        public KalturaMessageTemplate Get(KalturaOTTAssetType messageType)
        {
            KalturaMessageTemplate response = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.NotificationClient().GetMessageTemplate(groupId, messageType);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

    }
}