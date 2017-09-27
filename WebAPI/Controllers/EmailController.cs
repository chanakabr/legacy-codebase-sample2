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
using WebAPI.Models.Notification;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/email/action")]
    public class EmailController : ApiController
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        /// <summary>
        /// Sends email notification
        /// </summary>
        /// <remarks>
        /// Possible status codes:       
        /// </remarks>        
        /// <param name="emailMessage">email details</param>     
        [Route("send"), HttpPost]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        public bool Send(KalturaEmailMessage emailMessage)
        {
            bool response = false;

            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                KS.GetFromRequest().ToString();
                
                if (emailMessage == null)
                       throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "sendMessage");

                if (string.IsNullOrEmpty(emailMessage.TemplateName))
                    throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "emailMessage.TemplateName");

                if (string.IsNullOrEmpty(emailMessage.Subject))
                    throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "emailMessage.Subject");

                if (string.IsNullOrEmpty(emailMessage.SenderTo))
                    throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "emailMessage.SenderTo");

                if (string.IsNullOrEmpty(emailMessage.SenderFrom))
                    throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "emailMessage.SenderFrom");


                // call client                
                response = ClientsManager.NotificationClient().SendEmail(groupId, emailMessage);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
            return response;
        }
    }
}