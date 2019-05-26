using APILogic.AmazonSnsAdapter;
using APILogic.DmsService;
using ApiObjects;
using ApiObjects.Notification;
using ApiObjects.Response;
using CachingProvider.LayeredCache;
using Core.Notification.Adapters;
using DAL;
using KLogMonitor;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TVinciShared;

namespace Core.Notification
{
    public class TopicNotificationMessageManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        internal static GenericResponse<TopicNotificationMessage> Add(int groupId, TopicNotificationMessage topicNotificationMessage, long userId)
        {
            GenericResponse<TopicNotificationMessage> response = new GenericResponse<TopicNotificationMessage>();

            try
            {
                // create DB topicNotification
                topicNotificationMessage.Id = NotificationDal.InsertTopicNotificationMessage(topicNotificationMessage.GroupId, topicNotificationMessage.TopicNotificationId, userId);
                if (topicNotificationMessage.Id == 0)
                {
                    log.DebugFormat("failed to insert topicNotificationMessage to DB groupID = {0}, TopicNotificationId = {1}", groupId, topicNotificationMessage.TopicNotificationId);
                    response.SetStatus(eResponseStatus.Error, "fail insert TopicNotificationMessage to DB");
                    return response;
                }

                // Save TopicNotificationAtCB                    
                if (!NotificationDal.SaveTopicNotificationMessageCB(groupId, topicNotificationMessage))
                {
                    log.ErrorFormat("Error while saving topicNotificationMessage. groupId: {0}, topicNotificationMessageId:{1}", groupId, topicNotificationMessage.Id);
                    //return response;
                }

                //TODO anat/irena cache ? 

                //string invalidationKey = LayeredCacheKeys.GetTopicNotificationsInvalidationKey(groupId, (int)topicNotification.SubscribeReference.Type);
                //if (!LayeredCache.Instance.SetInvalidationKey(invalidationKey))
                //{
                //    log.ErrorFormat("Failed to set invalidation key on topic notifications key = {0}", invalidationKey);
                //}
                AddTopicNotificationMessageToQueue(topicNotificationMessage);

                response.Object = topicNotificationMessage;
                response.SetStatus(eResponseStatus.OK, eResponseStatus.OK.ToString());
            }
            catch (Exception ex)
            {
                log.ErrorFormat("CreateTopicNotificationMessage  failed groupId = {0}, ex = {1}", groupId, ex);
                return response;
            }

            return response;
        }

        public static bool Send(int groupId, long startTime, int messageId)
        {
            // get message announcements
            TopicNotificationMessage topicNotificationMessage = NotificationDal.GetTopicNotificationMessageCB(messageId);
            if (topicNotificationMessage == null)
            {
                log.ErrorFormat("topic notification message was not found. grogroupIdup: {0} messageId: {1}", groupId, messageId);
                return false;
            }

            // get topic notification
            TopicNotification topicNotification = TopicNotificationManager.GetTopicNotificationById(topicNotificationMessage.TopicNotificationId);
            if (topicNotification == null)
            {
                log.ErrorFormat("topic notification was not found. grogroupIdup: {0} topicNotificationId: {1}", groupId, topicNotificationMessage.TopicNotificationId);
                return false;
            }

            // validate start time - TODO: irena
            //
            //


            long currentTimeSec = DateUtils.DateTimeToUtcUnixTimestampSeconds(DateTime.UtcNow);
            string singleTopicExternalId = string.Empty;
            string singleQueueName = string.Empty;
            List<string> topicExternalIds = new List<string>();
            List<string> queueNames = new List<string>();
            bool includeMail = false;
            string mailTemplate = string.Empty;
            string mailSubject = string.Empty;
            bool includeSms = false;

            if (topicNotificationMessage.Dispatchers != null)
            {
                includeSms = topicNotificationMessage.Dispatchers.Count(d => d.Type == TopicNotificationDispatcherType.Sms) > 0;

                var dispatcher = topicNotificationMessage.Dispatchers.First(d => d.Type == TopicNotificationDispatcherType.Mail);
                if (dispatcher != null)
                {
                    TopicNotificationMailDispatcher mailDispatcher = (TopicNotificationMailDispatcher)dispatcher;
                    includeMail = true;
                    mailTemplate = mailDispatcher.BodyTemplate;
                    mailSubject = mailDispatcher.SubjectTemplate;
                }
            }

            // send inbox messages
            /*
            if (NotificationSettings.IsPartnerInboxEnabled(groupId))
            {
                InboxMessage inboxMessage = new InboxMessage()
                {
                    Category = eMessageCategory.SystemAnnouncement, // TODO: ??
                    CreatedAtSec = currentTimeSec,
                    Id = topicNotificationMessage.Id.ToString(), // TODO: ??
                    Message = topicNotificationMessage.Message,
                    State = eMessageState.Unread, // TODO: ??
                    UpdatedAtSec = currentTimeSec,
                    Url = url, // TODO: ??
                    ImageUrl = topicNotificationMessage.ImageUrl
                };

                if (!NotificationDal.SetSystemAnnouncementMessage(groupId, inboxMessage, NotificationSettings.GetInboxMessageTTLDays(groupId)))
                    log.ErrorFormat("Error while setting topic notification inbox message. GID: {0}, InboxMessage: {1}", groupId, JsonConvert.SerializeObject(inboxMessage));
            }
            */
            if (includeMail && NotificationSettings.IsPartnerMailNotificationEnabled(groupId))
            {
                if (!string.IsNullOrEmpty(topicNotification.MailExternalId))
                {
                    if (!MailNotificationAdapterClient.PublishToAnnouncement(groupId, topicNotification.MailExternalId, mailSubject, null, mailTemplate))
                    {
                        log.ErrorFormat("failed to send topic notification message to mail adapter. annoucementId = {0}", topicNotification.Id);
                    }
                    else
                    {
                        log.DebugFormat("Successfully sent topic notification message to mail. announcementId: {0}", topicNotification.Id);

                        // TODO: update system external result ?? 
                    }
                }
            }

            if (!string.IsNullOrEmpty(topicNotification.PushExternalId) &&
                (NotificationSettings.IsPartnerSmsNotificationEnabled(groupId) || NotificationSettings.IsPartnerPushEnabled(groupId)))
            {
                string url = string.Empty; // TODO: ??
                string sound = string.Empty; // TODO: ??
                string category = string.Empty; // TODO: ??

                var messageTemplate = NotificationCache.Instance().GetMessageTemplates(groupId).First(x => x.TemplateType == MessageTemplateType.Reminder);

                if (messageTemplate != null)
                {
                    url = messageTemplate.URL;
                    sound = messageTemplate.Sound;
                    category = messageTemplate.Action;
                }

                string resultMsgId = NotificationAdapter.PublishToAnnouncement(groupId, topicNotification.PushExternalId, string.Empty,
                    new MessageData()
                    {
                        Alert = topicNotificationMessage.Message,
                        Url = url,
                        Sound = sound,
                        Category = category,
                        ImageUrl = topicNotificationMessage.ImageUrl
                    });
                if (string.IsNullOrEmpty(resultMsgId))
                {
                    log.ErrorFormat("failed to send SMS/push with topic notification message to adapter. topicNotificationMessageId = {0}", topicNotificationMessage.Id);
                }
                else
                {
                    log.DebugFormat("Successfully sent SMS/push with topic notification message to adapter. topicNotificationMessageId = {0}", topicNotificationMessage.Id);
                }
            }

            log.DebugFormat("Successfully sent topic notification message: Id: {0}", messageId);

            // TODO:  - update message sent
            //DAL.NotificationDal.Update_MessageAnnouncementSent(messageId, groupId, (int)eAnnouncementStatus.Sent);
            //DAL.NotificationDal.Update_MessageAnnouncementResultMessageId(messageId, groupId, resultMsgIds);
            return true;
        }

        internal static GenericResponse<TopicNotificationMessage> Update(int groupId, TopicNotificationMessage topicNotificationMessageToUpdate, long userId)
        {
            GenericResponse<TopicNotificationMessage> response = new GenericResponse<TopicNotificationMessage>();
            string logData = string.Format("groupId: {0}, TopicNotificationMessageId: {1}", groupId, topicNotificationMessageToUpdate.Id);

            try
            {
                // get currnt TopicNotificationMessage
                TopicNotificationMessage currentTopicNotificationMessage = NotificationDal.GetTopicNotificationMessageCB(topicNotificationMessageToUpdate.Id);
                if (currentTopicNotificationMessage == null)
                {
                    log.ErrorFormat("TopicNotificationMessage wasn't found. {0}", logData);
                    response.SetStatus(eResponseStatus.TopicNotificationMessageNotFound);
                    return response;
                }

                //if (!currentTopicNotification.Name.Equals(topicNotification.Name) || !currentTopicNotification.Description.Equals(topicNotification.Description))
                //{
                // update DB topicNotification
                //if (!NotificationDal.UpdateTopicNotificationMessage(currentTopicNotificationMessage.Id, topicNotification.GroupId, topicNotification.Name, userId))
                //{
                //    log.DebugFormat("failed to update TopicNotification to DB groupID = {0}, topicName {1}", topicNotification.GroupId, topicNotification.Name);
                //    response.SetStatus(eResponseStatus.Error, "fail update TopicNotification to DB");
                //    return response;
                //}

                //    currentTopicNotification.Name = topicNotification.Name;
                //    currentTopicNotification.Description = topicNotification.Description;

                //    // Save TopicNotificationAtCB                    
                //    if (!NotificationDal.SaveTopicNotificationMessageCB(currentTopicNotificationMessage.GroupId, currentTopicNotificationMessage))
                //    {
                //        log.ErrorFormat("Error while saving topicNotificationMessage. groupId: {0}, topicNotificationMessageId:{1}", currentTopicNotificationMessage.GroupId, currentTopicNotificationMessage.Id);
                //    }

                //    //string invalidationKey = LayeredCacheKeys.GetTopicNotificationsInvalidationKey(groupId, (int)topicNotification.SubscribeReference.Type);
                //    //if (!LayeredCache.Instance.SetInvalidationKey(invalidationKey))
                //    //{
                //    //    log.ErrorFormat("Failed to set invalidation key on topic notifications key = {0}", invalidationKey);
                //    //}
                //}
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Update topicNotificationMessage failed groupId = {0}, ex = {1}", topicNotificationMessageToUpdate.GroupId, ex);
                return response;
            }

            return response;
        }

        internal static Status Delete(int groupId, long userId, long topicNotificationMessageId)
        {
            Status response = new Status() { Code = (int)eResponseStatus.Error, Message = eResponseStatus.Error.ToString() };
            string logData = string.Format("GID: {0}, topicNotificationMessageId: {1}", groupId, topicNotificationMessageId);

            try
            {
                // get topicNotificationMessage
                TopicNotificationMessage topicNotificationMessage = NotificationDal.GetTopicNotificationMessageCB(topicNotificationMessageId);
                if (topicNotificationMessage == null)
                {
                    log.ErrorFormat("TopicNotificationMessage wasn't found. {0}", logData);
                    response.Code = (int)eResponseStatus.TopicNotificationMessageNotFound;
                    return response;
                }

                // delete topicNotificationMessage from DB
                if (!NotificationDal.DeleteTopicNotificationMessage(groupId, userId, topicNotificationMessageId))
                {
                    log.ErrorFormat("Error while trying to delete DB topicNotificationMessage. {0}", logData);
                    return response;
                }
                else
                {
                    // remove from CB
                    if (!NotificationDal.DeleteTopicNotificationMessageCB(topicNotificationMessageId))
                    {
                        log.ErrorFormat("Error while delete topicNotificationMessage CB. groupId: {0}, topicNotificationMessage:{1}", groupId, topicNotificationMessageId);
                        return response;
                    }

                    //string invalidationKey = LayeredCacheKeys.GetTopicNotificationsInvalidationKey(groupId, (int)topicNotificationMessage.SubscribeReference.Type);
                    //if (!LayeredCache.Instance.SetInvalidationKey(invalidationKey))
                    //{
                    //    log.ErrorFormat("Failed to set invalidation key on topic notifications key = {0}", invalidationKey);
                    //}

                    response = new Status() { Code = (int)eResponseStatus.OK, Message = eResponseStatus.OK.ToString() };
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Delete topicNotificationMessage failed topicNotificationMessageId = {0}, ex = {1}", topicNotificationMessageId, ex);
                return response;
            }

            return response;
        }

        internal static GenericListResponse<TopicNotificationMessage> List(int groupId, long topicNotificationId)
        {
            throw new NotImplementedException();
        }

        private static bool AddTopicNotificationMessageToQueue(TopicNotificationMessage topicNotificationMessage)
        {
            bool result = false;
            DateTime? triggerTime = null;
            if (topicNotificationMessage.Trigger != null)
            {
                switch (topicNotificationMessage.Trigger.Type)
                {
                    case TopicNotificationTriggerType.Date:
                        triggerTime = ((TopicNotificationDateTrigger)topicNotificationMessage.Trigger).Date;
                        break;
                    case TopicNotificationTriggerType.Subscription:
                        {
                            TopicNotificationSubscriptionTrigger trigger = (TopicNotificationSubscriptionTrigger)topicNotificationMessage.Trigger;
                            TopicNotification topicNotification = TopicNotificationManager.GetTopicNotificationById(topicNotificationMessage.TopicNotificationId);
                            long subscriptionId = 0;
                            if (topicNotification != null && topicNotification.SubscribeReference != null && topicNotification.SubscribeReference.Type == SubscribeReferenceType.Subscription)
                            {
                                subscriptionId = ((SubscriptionSubscribeReference)topicNotification.SubscribeReference).SubscriptionId;
                            }
                            if (subscriptionId > 0)
                            {
                                var subscription = Pricing.Module.GetSubscriptionData(topicNotification.GroupId, subscriptionId.ToString(), string.Empty, string.Empty, string.Empty, false);
                                switch (trigger.TriggerType)
                                {
                                    case ApiObjects.TopicNotificationSubscriptionTriggerType.StartDate:
                                        triggerTime = subscription.m_dStartDate.AddSeconds(trigger.Offset);
                                        break;
                                    case ApiObjects.TopicNotificationSubscriptionTriggerType.EndDate:
                                        triggerTime = subscription.m_dEndDate.AddSeconds(trigger.Offset);
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                        break;
                    default:
                        break;
                }
            }

            if (triggerTime.HasValue)
            {
                QueueWrapper.Queues.QueueObjects.MessageAnnouncementQueue queue = new QueueWrapper.Queues.QueueObjects.MessageAnnouncementQueue();
                ApiObjects.QueueObjects.MessageAnnouncementData messageAnnouncementData = new ApiObjects.QueueObjects.MessageAnnouncementData(
                    topicNotificationMessage.GroupId, 
                    DateUtils.DateTimeToUtcUnixTimestampSeconds(triggerTime.Value), 
                    (int)topicNotificationMessage.Id, MessageAnnouncementRequestType.TopicNotificationMessage)
                {
                    ETA = triggerTime
                };

                if (queue.Enqueue(messageAnnouncementData, AnnouncementManager.ROUTING_KEY_PROCESS_MESSAGE_ANNOUNCEMENTS))
                {
                    log.DebugFormat("Successfully inserted a message to announcement queue: {0}", messageAnnouncementData);
                    result = true;
                }
                else
                {
                    log.ErrorFormat("Error while inserting announcement {0} to queue", messageAnnouncementData);
                }
            }

            return result;
        }
    }
}