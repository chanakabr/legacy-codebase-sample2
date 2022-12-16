using ApiObjects.Response;
using Phx.Lib.Log;
using System;
using System.Reflection;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Notification;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [Service("topicNotification")]
    public class TopicNotificationController : IKalturaController
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        /// <summary>
        /// Add a new topic notification 
        /// </summary>
        /// <param name="topicNotification">The topic notification to add</param>
        [Action("add")]
        [ApiAuthorize]
        [Throws(eResponseStatus.SubscriptionDoesNotExist)]
        [Throws(eResponseStatus.FailCreateTopicNotification)]
        static public KalturaTopicNotification Add(KalturaTopicNotification topicNotification)
        {
            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                string userId = KS.GetFromRequest().UserId;

                if (string.IsNullOrEmpty(topicNotification.Name))
                {
                    throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "KalturaTopicNotification.name");
                }

                if (topicNotification.SubscribeReference == null)
                {
                    throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "KalturaTopicNotification.subscribeReference");
                }

                return ClientsManager.NotificationClient().AddTopicNotification(groupId, topicNotification, userId);
            }

            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return null;
        }

        /// <summary>
        /// Update an existing topic notification
        /// </summary>
        /// <param name="id">The topic notification ID to update </param>
        /// <param name="topicNotification">The topic notification to update</param>
        [Action("update")]
        [ApiAuthorize]
        [Throws(eResponseStatus.TopicNotificationNotFound)]        
        static public KalturaTopicNotification Update(int id, KalturaTopicNotification topicNotification)
        {
            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                string userId = KS.GetFromRequest().UserId;

                if (topicNotification.Name == string.Empty)
                {
                    throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "KalturaTopicNotification.name");
                }

                topicNotification.Id = id;

                return ClientsManager.NotificationClient().UpdateTopicNotification(groupId, topicNotification, userId);
            }

            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return null;
        }

        /// <summary>
        /// Delete an existing topic notification
        /// </summary>
        /// <param name="id">ID of topic notification to delete</param>
        [Action("delete")]
        [ApiAuthorize]
        [Throws(eResponseStatus.TopicNotificationNotFound)]
        static public void Delete(long id)
        {
            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                string userId = KS.GetFromRequest().UserId;

                ClientsManager.NotificationClient().DeleteTopicNotification(groupId, id, userId);
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
        [Action("list")]
        [ApiAuthorize]
        static public KalturaTopicNotificationListResponse List(KalturaTopicNotificationFilter filter)
        {
            KalturaTopicNotificationListResponse response = null;

            if (filter == null)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "filter");
            }

            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                response = ClientsManager.NotificationClient().GetTopicNotifications(groupId, filter.SubscribeReference);
            }

            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }


        /// <summary>
        /// Subscribe a user to a topic notification
        /// </summary>
        /// <param name="topicNotificationId">ID of topic notification to subscribe to.</param>
        [Action("subscribe")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [Throws(eResponseStatus.TopicNotificationNotFound)]
        static public void Subscribe(long topicNotificationId)
        {
            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                string userId = KS.GetFromRequest().UserId;
                ClientsManager.NotificationClient().SubscribeUserToTopicNotification(groupId, userId, topicNotificationId);
            }

            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
        }

        /// <summary>
        /// Unubscribe a user from a topic notification
        /// </summary>
        /// <param name="topicNotificationId">ID of topic notification to unsubscribe from.</param>
        [Action("unsubscribe")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [Throws(eResponseStatus.TopicNotificationNotFound)]
        [Throws(eResponseStatus.UserNotFollowing)]
        static public void Unubscribe(long topicNotificationId)
        {
            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                string userId = KS.GetFromRequest().UserId;
                ClientsManager.NotificationClient().UnsubscribeUserFromTopicNotification(groupId, userId, topicNotificationId);
            }

            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
        }

    }
}