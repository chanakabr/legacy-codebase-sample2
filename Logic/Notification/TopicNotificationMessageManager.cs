using ApiObjects;
using ApiObjects.Notification;
using ApiObjects.Response;
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
                    //return response;
                }

                AddTopicNotificationMessageToQueue(topicNotificationMessage, topicNotification);

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

        public static bool Send(int groupId, long startTime, int messageAnnouncementId)
        {
            throw new NotImplementedException();
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

                if (currentTopicNotificationMessage.Trigger.Equals(topicNotificationMessageToUpdate.Trigger))
                {
                    log.ErrorFormat("Wrong topic notification trigger. {0}", logData);
                    response.SetStatus(eResponseStatus.WrongTopicNotificationTrigger);
                    return response;
                }

                // Save TopicNotificationAtCB                    
                if (!NotificationDal.SaveTopicNotificationMessageCB(currentTopicNotificationMessage.GroupId, currentTopicNotificationMessage))
                {
                    log.ErrorFormat("Error while saving topicNotificationMessage. groupId: {0}, topicNotificationMessageId:{1}", currentTopicNotificationMessage.GroupId, currentTopicNotificationMessage.Id);
                }
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

        //TODO anat add paging
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

                // get announcements                
                DataTable topicNotificationMessageDT = NotificationDal.GetTopicNotificationMessages(groupId, topicNotificationId);
                if (topicNotificationMessageDT?.Rows.Count > 0)
                {
                    List<long> topicNotificationMessageIds = new List<long>();
                    response.Objects = new List<TopicNotificationMessage>();
                    foreach (DataRow row in topicNotificationMessageDT.Rows)
                    {
                        topicNotificationMessageIds.Add(ODBCWrapper.Utils.GetLongSafeVal(row, "id"));
                    }

                    // paging
                    topicNotificationMessageIds = topicNotificationMessageIds.Skip(pageSize * pageIndex).Take(pageSize).ToList();
                    TopicNotificationMessage topicNotificationMessage = null;

                    foreach (long item in topicNotificationMessageIds)
                    {
                        topicNotificationMessage = NotificationDal.GetTopicNotificationMessageCB(item);
                        if(topicNotificationMessage !=null)
                        {
                            response.Objects.Add(topicNotificationMessage);

                        }
                        else
                        {
                            log.ErrorFormat("Failed to get topicNotificationMessage {0}", item);
                        }
                    }

                    response.TotalItems = topicNotificationMessageIds.Count;
                }

                response.Status.Set((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error in GetTopicNotificationMessages {0}", ex);
            }

            return response;
        }

        private static bool AddTopicNotificationMessageToQueue(TopicNotificationMessage topicNotificationMessage, TopicNotification topicNotification)
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
                            long subscriptionId = 0;
                            if (topicNotification.SubscribeReference != null && topicNotification.SubscribeReference.Type == SubscribeReferenceType.Subscription)
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