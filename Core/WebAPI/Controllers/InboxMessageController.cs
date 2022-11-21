using ApiObjects.Response;
using System;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;
using WebAPI.Models.Notification;
using WebAPI.ObjectsConvertor.Extensions;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [Service("inboxMessage")]
    public class InboxMessageController : IKalturaController
    {

        /// <summary>
        /// TBD
        /// </summary>
        /// <param name="id">message id</param>        
        [Action("get")]
        [ApiAuthorize]
        [Throws(eResponseStatus.UserInboxMessagesNotExist)]
        [Throws(eResponseStatus.FeatureDisabled)]
        [Throws(eResponseStatus.MessageIdentifierRequired)]
        [Throws(StatusCode.UserIDInvalid)]
        static public KalturaInboxMessage Get(string id)
        {
            KalturaInboxMessage response = null;

            // parameters validation
            if (string.IsNullOrEmpty(id))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "id");
            }

            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                string userId = KS.GetFromRequest().UserId;
                var domainId = HouseholdUtils.GetHouseholdIDByKS();
                // call client                
                response = ClientsManager.NotificationClient().GetInboxMessage(groupId, domainId, userId, id);
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
        /// <param name="filter">filter</param>   
        /// <param name="pager">Page size and index</param>        
        [Action("list")]
        [ApiAuthorize]
        [Throws(StatusCode.UserIDInvalid)]
        [Throws(eResponseStatus.FeatureDisabled)]
        static public KalturaInboxMessageListResponse List(KalturaInboxMessageFilter filter = null, KalturaFilterPager pager = null)
        {
            KalturaInboxMessageListResponse response = null;

            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                string userId = KS.GetFromRequest().UserId;
                var domainId = HouseholdUtils.GetHouseholdIDByKS();

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
                response = ClientsManager.NotificationClient().GetInboxMessageList(groupId, domainId, userId, pager.PageSize.Value, pager.GetRealPageIndex(), filter.getTypeIn(), filter.CreatedAtGreaterThanOrEqual.Value, filter.CreatedAtLessThanOrEqual.Value);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
            return response;
        }

        /// <summary>
        /// Updates the message status.
        /// </summary>
        /// <remarks>
        /// Possible status codes:       
        /// UserIDInvalid = 500009, FeatureDisabled = 8009, MessageIdentifierRequired = 8019, UserInboxMessagesNotExist = 8020
        /// </remarks>
        /// <param name="id">Message identifier</param>        
        /// <param name="status">Message status</param>        
        [Action("updateStatus")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [Throws(eResponseStatus.UserInboxMessagesNotExist)]
        [Throws(eResponseStatus.FeatureDisabled)]
        [Throws(eResponseStatus.MessageIdentifierRequired)]
        [Throws(StatusCode.UserIDInvalid)]
        static public bool UpdateStatus(string id, KalturaInboxMessageStatus status)
        {
            bool response = false;

            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                string userId = KS.GetFromRequest().UserId;
                var domainId = HouseholdUtils.GetHouseholdIDByKS();

                // call client                
                response = ClientsManager.NotificationClient().UpdateInboxMessage(groupId, domainId, userId, id, status);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
            return response;
        }
    }
}