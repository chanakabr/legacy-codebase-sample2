using ApiObjects.Response;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Http;
using TVinciShared;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;
using WebAPI.Models.Notification;
using WebAPI.Models.Notifications;
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
        static public KalturaTopicNotification Add(KalturaTopicNotification topicNotification)
        {
            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                string userId = KS.GetFromRequest().UserId;
                
                if (string.IsNullOrEmpty(topicNotification.Name))
                {
                    throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "name");
                }

                if (topicNotification.SubscribeReference == null)
                {
                    throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "subscribeReference");
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