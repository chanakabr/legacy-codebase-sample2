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
using WebAPI.Models.Notification;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/inboxMessage/action")]
    public class InboxMessageController : ApiController
    {

        /// <summary>
        /// TBD
        /// </summary>
        /// <remarks>
        /// Possible status codes:       
        /// User inbox messages not exist = 8020
        /// </remarks>
        /// <param name="id">message id</param>        
        [Route("get"), HttpPost]
        [ApiAuthorize]
        public KalturaInboxMessage Get(string id)
        {
            KalturaInboxMessage response = null;

            // parameters validation
            if (string.IsNullOrEmpty(id))
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "id cannot be empty");
            }

            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                string userId = KS.GetFromRequest().UserId;
                // call client                
                response = ClientsManager.NotificationClient().GetInboxMessage(groupId, userId, id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
            return response;
        }

        /// <summary>
        /// List inbox messages
        /// </summary>
        /// <remarks>
        /// Possible status codes:       
        /// message identifier required = 8019, user inbox message not exist = 8020,
        /// </remarks>     
        /// <param name="filter">filter</param>   
        /// <param name="pager">Page size and index</param>        
        [Route("list"), HttpPost]
        [ApiAuthorize]
        public KalturaInboxMessageListResponse List(KalturaInboxMessageFilter filter = null, KalturaFilterPager pager = null)
        {
            KalturaInboxMessageListResponse response = null;

            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                string userId = KS.GetFromRequest().UserId;

                if (pager == null)
                    pager = new KalturaFilterPager();

                if (filter == null)
                    filter = new KalturaInboxMessageFilter();

                if (!filter.CreatedAtGreaterThanOrEqual.HasValue)
                {
                    filter.CreatedAtGreaterThanOrEqual = 0;
                }

                if (!filter.CreatedAtLessThanOrEqual.HasValue)
                {
                    filter.CreatedAtLessThanOrEqual = 0;
                }

                // call client                
                response = ClientsManager.NotificationClient().GetInboxMessageList(groupId, userId, pager.getPageSize(), pager.getPageIndex(), filter.getTypeIn(), filter.CreatedAtGreaterThanOrEqual.Value, filter.CreatedAtLessThanOrEqual.Value);
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
        /// Possible status codes:       
        /// 
        /// </remarks>
        /// <param name="id"></param>        
        /// <param name="status"></param>        
        [Route("updateStatus"), HttpPost]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        public bool UpdateStatus(string id, KalturaInboxMessageStatus status)
        {
            bool response = false;

            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                string userId = KS.GetFromRequest().UserId;

                // call client                
                response = ClientsManager.NotificationClient().UpdateInboxMessage(groupId, userId, id, status);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
            return response;
        }
    }
}