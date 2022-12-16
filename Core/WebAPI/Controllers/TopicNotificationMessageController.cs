using ApiObjects.Response;
using Phx.Lib.Log;
using System;
using System.Reflection;
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
    [Service("topicNotificationMessage")]
    public class TopicNotificationMessageController : IKalturaController
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        /// <summary>
        /// Add a new topic notification message
        /// </summary>
        /// <param name="topicNotificationMessage">The topic notification message to add</param>
        [Action("add")]
        [ApiAuthorize]
        [Throws(eResponseStatus.TopicNotificationNotFound)]
        static public KalturaTopicNotificationMessage Add(KalturaTopicNotificationMessage topicNotificationMessage)
        {
            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                string userId = KS.GetFromRequest().UserId;

                if (string.IsNullOrEmpty(topicNotificationMessage.Message))
                {
                    throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "KalturaTopicNotificationMessage.message");
                }
                
                return ClientsManager.NotificationClient().AddTopicNotificationMessage(groupId, topicNotificationMessage, userId);
            }

            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return null;
        }

        /// <summary>
        /// Update an existing topic notification message
        /// </summary>
        /// <param name="id">The topic notification message ID to update </param>
        /// <param name="topicNotificationMessage">The topic notification message to update</param>
        [Action("update")]
        [ApiAuthorize]
        [Throws(eResponseStatus.TopicNotificationMessageNotFound)]
        [Throws(eResponseStatus.WrongTopicNotification)]
        [Throws(eResponseStatus.WrongTopicNotificationTrigger)]

        static public KalturaTopicNotificationMessage Update(int id, KalturaTopicNotificationMessage topicNotificationMessage)
        {
            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                string userId = KS.GetFromRequest().UserId;

                if (topicNotificationMessage.Message == string.Empty)
                {
                    throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "KalturaTopicNotificationMessage.Message");
                }

                topicNotificationMessage.Id = id;
                return ClientsManager.NotificationClient().UpdateTopicNotificationMessage(groupId, id, topicNotificationMessage, userId);
            }

            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return null;
        }

        /// <summary>
        /// Delete an existing topic notification message
        /// </summary>
        /// <param name="id">ID of topic notification message to delete</param>
        [Action("delete")]
        [ApiAuthorize]
        [Throws(eResponseStatus.TopicNotificationMessageNotFound)]
        static public void Delete(long id)
        {
            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                string userId = KS.GetFromRequest().UserId;

                ClientsManager.NotificationClient().DeleteTopicNotificationMessage(groupId, id, userId);
            }

            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

        }

        /// <summary>
        /// Lists all topic notifications in the system.
        /// </summary>
        /// <param name="filter">Filter options</param>
        /// <param name="pager">Paging the request</param>
        [Action("list")]
        [ApiAuthorize]
        [Throws(eResponseStatus.TopicNotificationNotFound)]
        static public KalturaTopicNotificationMessageListResponse List(KalturaTopicNotificationMessageFilter filter = null, KalturaFilterPager pager = null)
        {
            KalturaTopicNotificationMessageListResponse response = null;

            if (filter == null)
                filter = new KalturaTopicNotificationMessageFilter();

            if (pager == null)
                pager = new KalturaFilterPager();

            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                response = ClientsManager.NotificationClient().GetTopicNotificationMessages(groupId, filter.TopicNotificationIdEqual, pager.PageSize.Value, pager.GetRealPageIndex());
            }

            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}