using APILogic.AmazonSnsAdapter;
using ApiObjects;
using ApiObjects.Notification;
using ApiObjects.Response;
using Core.Notification.Adapters;
using Core.Pricing;
using DAL;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Data;
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
                // validate topicNotificationMessage.TopicNotificationId                
                TopicNotification topicNotification = NotificationDal.GetTopicNotificationCB(topicNotificationMessage.TopicNotificationId);
                if (topicNotification == null)
                {
                    log.DebugFormat("failed to insert topicNotificationMessage to DB groupID = {0}, TopicNotificationId does not exist= {1}", groupId, topicNotificationMessage.TopicNotificationId);
                    response.SetStatus(eResponseStatus.TopicNotificationNotFound);
                    return response;
                }

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
                    response.SetStatus(eResponseStatus.Error, "fail insert TopicNotificationMessage to CB");
                    return response;
                }
                long triggerTime = GetTopicNotificationTriggerTime(topicNotificationMessage, topicNotification);

                if (AddTopicNotificationMessageToQueue(groupId, topicNotificationMessage.Id, triggerTime))
                {
                    response.Object = topicNotificationMessage;
                    response.SetStatus(eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
                else
                {
                    log.ErrorFormat("Error while AddTopicNotificationMessageToQueue. groupId: {0}, topicNotificationMessageId:{1}", groupId, topicNotificationMessage.Id);
                    response.SetStatus(eResponseStatus.Error, "fail add topic notification message to queue");
                    return response;
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("CreateTopicNotificationMessage  failed groupId = {0}, ex = {1}", groupId, ex);
                return response;
            }

            return response;
        }

        public static bool Send(int groupId, long sendTime, int messageId)
        {
            // get message announcements
            TopicNotificationMessage topicNotificationMessage = NotificationDal.GetTopicNotificationMessageCB(messageId);
            if (topicNotificationMessage == null)
            {
                log.ErrorFormat("topic notification message was not found. groupId: {0} messageId: {1}", groupId, messageId);
                return true;
            }

            // get topic notification
            GenericResponse<TopicNotification> response = TopicNotificationManager.GetTopicNotificationById(topicNotificationMessage.TopicNotificationId);
            if (!response.HasObject())
            {
                log.ErrorFormat("topic notification was not found. grogroupIdup: {0} topicNotificationId: {1}", groupId, topicNotificationMessage.TopicNotificationId);
                return true;
            }

            TopicNotification topicNotification = response.Object;

            // validate send time 
            var triggerTime = GetTopicNotificationTriggerTime(topicNotificationMessage, topicNotification);
            if (triggerTime <= 0 || triggerTime != sendTime)
            {
                log.ErrorFormat("topic notification message was not sent due to wrong trigger time. groupId: {0} messageId: {1}, triggerTime: {2}, sent time: {3}",
                    groupId, topicNotificationMessage.TopicNotificationId, triggerTime, sendTime);

                return false;
            }

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
                string url = string.Empty;
                string sound = string.Empty;
                string category = string.Empty;

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

            topicNotificationMessage.Status = TopicNotificationMessageStatus.Sent;
            topicNotificationMessage.SendDate = DateUtils.GetUtcUnixTimestampNow();
            DAL.NotificationDal.SaveTopicNotificationMessageCB(groupId, topicNotificationMessage);
            DAL.NotificationDal.UpdateTopicNotificationMessage(topicNotificationMessage.Id, (int)topicNotificationMessage.Status, topicNotificationMessage.SendDate);

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

                if (currentTopicNotificationMessage.TopicNotificationId != topicNotificationMessageToUpdate.TopicNotificationId)
                {
                    log.ErrorFormat("Wrong topic notification identifier. {0}", logData);
                    response.SetStatus(eResponseStatus.WrongTopicNotification);
                    return response;
                }

                if (currentTopicNotificationMessage.Status == TopicNotificationMessageStatus.Sent)
                {
                    log.ErrorFormat("Cannot update sent message. {0}", logData);
                    response.SetStatus(eResponseStatus.Error, "Cannot update sent message");
                    return response;
                }

                if (!currentTopicNotificationMessage.Trigger.Equals(topicNotificationMessageToUpdate.Trigger))
                {
                    log.ErrorFormat("Wrong topic notification trigger. {0}", logData);
                    response.SetStatus(eResponseStatus.WrongTopicNotificationTrigger);
                    return response;
                }

                if (topicNotificationMessageToUpdate.Message != null)
                {
                    currentTopicNotificationMessage.Message = topicNotificationMessageToUpdate.Message;
                }

                if (topicNotificationMessageToUpdate.ImageUrl != null)
                {
                    currentTopicNotificationMessage.ImageUrl = topicNotificationMessageToUpdate.ImageUrl;
                }

                if (topicNotificationMessageToUpdate.Dispatchers != null)
                {
                    currentTopicNotificationMessage.Dispatchers = topicNotificationMessageToUpdate.Dispatchers;
                }

                // Save TopicNotificationAtCB                    
                if (!NotificationDal.SaveTopicNotificationMessageCB(currentTopicNotificationMessage.GroupId, currentTopicNotificationMessage))
                {
                    log.ErrorFormat("Error while saving SaveTopicNotificationMessageCB. groupId: {0}, topicNotificationMessageId:{1}", currentTopicNotificationMessage.GroupId, currentTopicNotificationMessage.Id);
                    response.SetStatus(eResponseStatus.Error);
                    return response;
                }

                response.Object = currentTopicNotificationMessage;
                response.SetStatus(eResponseStatus.OK);
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

        internal static GenericListResponse<TopicNotificationMessage> List(int groupId, long topicNotificationId, int pageSize, int pageIndex)
        {
            GenericListResponse<TopicNotificationMessage> response = new GenericListResponse<TopicNotificationMessage>();
            try
            {
                // validate topicNotificationMessage.TopicNotificationId                
                TopicNotification topicNotification = NotificationDal.GetTopicNotificationCB(topicNotificationId);
                if (topicNotification == null)
                {
                    log.DebugFormat("failed to get topicNotificationMessages for groupID = {0}, TopicNotificationId does not exist= {1}", groupId, topicNotificationId);
                    response.SetStatus(eResponseStatus.TopicNotificationNotFound);
                    return response;
                }

                // get topicNotificationMessages                
                DataTable topicNotificationMessageDT = NotificationDal.GetTopicNotificationMessages(groupId, topicNotificationId);
                if (topicNotificationMessageDT?.Rows.Count > 0)
                {
                    List<long> topicNotificationMessageIds = new List<long>();
                    response.Objects = new List<TopicNotificationMessage>();
                    foreach (DataRow row in topicNotificationMessageDT.Rows)
                    {
                        topicNotificationMessageIds.Add(ODBCWrapper.Utils.GetLongSafeVal(row, "id"));
                    }

                    topicNotificationMessageIds = topicNotificationMessageIds.OrderByDescending(x => x).ToList();
                    response.TotalItems = topicNotificationMessageIds.Count;

                    // paging
                    topicNotificationMessageIds = topicNotificationMessageIds.Skip(pageSize * pageIndex).Take(pageSize).ToList();
                    if (topicNotificationMessageIds.Count > 0)
                    {
                        response.Objects = NotificationDal.GetTopicNotificationMessagesCB(topicNotificationMessageIds);
                    }
                }

                response.Status.Set((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error in GetTopicNotificationMessages {0}", ex);
            }

            return response;
        }

        public static bool AddTopicNotificationMessageToQueue(int groupId, long topicNotificationMessageId, long sendDate)
        {
            bool result = false;

            QueueWrapper.Queues.QueueObjects.MessageAnnouncementQueue queue = new QueueWrapper.Queues.QueueObjects.MessageAnnouncementQueue();
            ApiObjects.QueueObjects.MessageAnnouncementData messageAnnouncementData = new ApiObjects.QueueObjects.MessageAnnouncementData(
                groupId,
                sendDate,
                (int)topicNotificationMessageId, MessageAnnouncementRequestType.TopicNotificationMessage)
            {
                ETA = DateUtils.UtcUnixTimestampSecondsToDateTime(sendDate)
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


            return result;
        }

        private static long GetTopicNotificationTriggerTime(TopicNotificationMessage topicNotificationMessage, TopicNotification topicNotification)
        {
            long triggerTime = 0;

            if (topicNotificationMessage.Trigger != null)
            {
                switch (topicNotificationMessage.Trigger.Type)
                {
                    case TopicNotificationTriggerType.Date:
                        triggerTime = DateUtils.DateTimeToUtcUnixTimestampSeconds(((TopicNotificationDateTrigger)topicNotificationMessage.Trigger).Date);
                        break;
                    case TopicNotificationTriggerType.Subscription:
                        {
                            TopicNotificationSubscriptionTrigger trigger = (TopicNotificationSubscriptionTrigger)topicNotificationMessage.Trigger;
                            long subscriptionId = 0;
                            if (topicNotification != null && topicNotification.SubscribeReference != null && topicNotification.SubscribeReference.Type == SubscribeReferenceType.Subscription)
                            {
                                subscriptionId = ((SubscriptionSubscribeReference)topicNotification.SubscribeReference).SubscriptionId;
                            }
                            if (subscriptionId > 0)
                            {
                                Subscription subscription = Pricing.Module.GetSubscriptionData(topicNotification.GroupId, subscriptionId.ToString(), string.Empty, string.Empty, string.Empty, false);                                
                                switch (trigger.TriggerType)
                                {
                                    case ApiObjects.TopicNotificationSubscriptionTriggerType.StartDate:
                                        triggerTime = DateUtils.DateTimeToUtcUnixTimestampSeconds(subscription.m_dStartDate.AddSeconds(trigger.Offset));
                                        break;
                                    case ApiObjects.TopicNotificationSubscriptionTriggerType.EndDate:
                                        triggerTime = DateUtils.DateTimeToUtcUnixTimestampSeconds(subscription.m_dEndDate.AddSeconds(trigger.Offset));
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

            return triggerTime;
        }

        public static GenericResponse<TopicNotificationMessage> GetTopicNotificationMessageById(long topicNotificationMessageId)
        {
            GenericResponse<TopicNotificationMessage> response = new GenericResponse<TopicNotificationMessage>();

            TopicNotificationMessage topicNotificationMessage = NotificationDal.GetTopicNotificationMessageCB(topicNotificationMessageId);
            if (topicNotificationMessage == null)
            {
                //log.ErrorFormat("TopicNotificationMessage wasn't found. {0}", logData);
                return response;
            }

            response.Object = topicNotificationMessage;
            response.SetStatus(eResponseStatus.OK, eResponseStatus.OK.ToString());

            return response;
        }
    }
}