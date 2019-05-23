using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Http;
using KLogMonitor;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;
using WebAPI.Models.Notification;
using WebAPI.Models.Notifications;
using WebAPI.Utils;
using ApiObjects.Response;
using TVinciShared;

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
        static public KalturaTopicNotificationMessage Add(KalturaTopicNotificationMessage topicNotificationMessage)
        {
            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                string userId = KS.GetFromRequest().UserId;

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
        static public KalturaTopicNotificationMessage Update(int id, KalturaTopicNotificationMessage topicNotificationMessage)
        {
            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                string userId = KS.GetFromRequest().UserId;

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
        [Action("list")]
        [ApiAuthorize]
        static public KalturaTopicNotificationMessageListResponse List(KalturaTopicNotificationMessageFilter filter = null)
        {
            KalturaTopicNotificationMessageListResponse response = null;

            if (filter == null)
                filter = new KalturaTopicNotificationMessageFilter();

            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                response = ClientsManager.NotificationClient().GetTopicNotificationMessages(groupId, filter.TopicNotificationIdEqual);
            }

            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}