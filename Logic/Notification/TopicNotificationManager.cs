using ApiObjects.Notification;
using ApiObjects.Response;
using Core.Notification.Adapters;
using DAL;
using KLogMonitor;
using System;
using System.Reflection;

namespace Core.Notification
{
    public class TopicNotificationManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public static GenericResponse<TopicNotification> Add(int groupId, TopicNotification topicNotification)
        {
            return null;
        }

        public static GenericResponse<TopicNotification> Update(int groupId, TopicNotification topicNotification)
        {
            return null;
        }

        public static Status Delete(int groupId, long topicNotificationId)
        {
            return null;
        }

        public static GenericListResponse<TopicNotification> List(int groupId, SubscribeReference subscribeReference)
        {
            return null;
        }

        public static Status Subscribe(int groupId, long topicNotificationId, long userId)
        {
            return null;
        }

        public static Status Unsubscribe(int groupId, long topicNotificationId, long userId)
        {
            return null;
        }

        private static GenericResponse<TopicNotification> CreateTopicNotification(TopicNotification topicNotification, long userId)
        {
            GenericResponse<TopicNotification> response = new GenericResponse<TopicNotification>();

            try
            {
                string topicName = topicNotification.Name;

                // create Amazon topic 
                string pushExternalId = string.Empty;
                if (NotificationSettings.IsPartnerPushEnabled(topicNotification.GroupId))
                {
                    pushExternalId = NotificationAdapter.CreateAnnouncement(topicNotification.GroupId, topicName, true);
                    if (string.IsNullOrEmpty(pushExternalId))
                    {
                        log.DebugFormat("failed to create TopicNotification groupID = {0}, TopicNotification Name = {1}", topicNotification.GroupId, topicName);
                        response.SetStatus(eResponseStatus.FailCreateTopicNotification, "fail create TopicNotification");
                        return response;
                    }

                    topicNotification.PushExternalId = pushExternalId;
                }

                string mailExternalId = string.Empty;
                if (NotificationSettings.IsPartnerMailNotificationEnabled(topicNotification.GroupId))
                {
                    mailExternalId = MailNotificationAdapterClient.CreateAnnouncement(topicNotification.GroupId, topicName);
                    if (string.IsNullOrEmpty(pushExternalId))
                    {
                        log.DebugFormat("failed to create TopicNotification groupID = {0}, TopicNotification Name = {1}", topicNotification.GroupId, topicName);
                        response.SetStatus(eResponseStatus.FailCreateTopicNotification, "fail create TopicNotification");
                        return response;
                    }

                    topicNotification.MailExternalId = mailExternalId;
                }

                // create DB announcement
                topicNotification.Id = NotificationDal.Insert_TopicNotification(topicNotification.GroupId, topicName, topicNotification.SubscribeReference.Type, userId);
                if (topicNotification.Id == 0)
                {
                    log.DebugFormat("failed to insert TopicNotification to DB groupID = {0}, announcementName = {1}", topicNotification.GroupId, topicName);
                    response.SetStatus(eResponseStatus.Error, "fail insert TopicNotification to DB");
                }
                else
                {
                    //TODO anat
                    //NotificationCache.Instance().RemoveAnnouncementsFromCache(topicNotification.GroupId);

                    response.Object = topicNotification;
                    response.SetStatus(eResponseStatus.OK, eResponseStatus.OK.ToString());

                    // Save TopicNotificationAtCB                    
                    if (!NotificationDal.SaveTopicNotificationCB(topicNotification.GroupId, topicNotification))
                    {
                        log.ErrorFormat("Error while saving topicNotification. groupId: {0}, topicNotificationId:{1}", topicNotification.GroupId, topicNotification.Id);
                        //return response;
                    }
                }

            }
            catch (Exception ex)
            {
                log.ErrorFormat("CreateTopicNotification failed groupId = {0}, ex = {1}", topicNotification.GroupId, ex);
                return response;
            }

            return response;
        }
    }
}  