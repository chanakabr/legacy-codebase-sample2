using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.Notification;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/FollowTemplate/action")]
    public class FollowTemplate : ApiController
    {
        /// <summary>
        /// TBD
        /// </summary>
        /// <param name="follow_template">TBD</param>
        /// <remarks>
        /// 
        /// </remarks>
        /// <returns></returns>
        [Route("add"), HttpPost]
        [ApiAuthorize]
        public KalturaFollowTemplate Add(KalturaFollowTemplate follow_template)
        {
            KalturaFollowTemplate response = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.NotificationClient().InsertFollowTemplate(groupId, follow_template);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}