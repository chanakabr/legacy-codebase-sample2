using APILogic.AmazonSnsAdapter;
using APILogic.DmsService;
using ApiObjects.Notification;
using ApiObjects.Response;
using CachingProvider.LayeredCache;
using Core.Notification.Adapters;
using DAL;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TVinciShared;

namespace Core.Notification
{
    public class TopicNotificationManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public static GenericResponse<TopicNotification> Add(int groupId, TopicNotification topicNotification, long userId)
        {
            GenericResponse<TopicNotification> response = new GenericResponse<TopicNotification>();

            try
            {
                topicNotification.GroupId = groupId;
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

                // create DB topicNotification
                topicNotification.Id = NotificationDal.InsertTopicNotification(topicNotification.GroupId, topicName, topicNotification.SubscribeReference.Type, userId);
                if (topicNotification.Id == 0)
                {
                    log.DebugFormat("failed to insert TopicNotification to DB groupID = {0}, topicName = {1}", topicNotification.GroupId, topicName);
                    response.SetStatus(eResponseStatus.Error, "fail insert TopicNotification to DB");
                    return response;
                }

                // Save TopicNotificationAtCB                    
                if (!NotificationDal.SaveTopicNotificationCB(topicNotification.GroupId, topicNotification))
                {
                    log.ErrorFormat("Error while saving topicNotification. groupId: {0}, topicNotificationId:{1}", topicNotification.GroupId, topicNotification.Id);
                    //return response;
                }

                string invalidationKey = LayeredCacheKeys.GetTopicNotificationsInvalidationKey(groupId, (int)topicNotification.SubscribeReference.Type);
                if (!LayeredCache.Instance.SetInvalidationKey(invalidationKey))
                {
                    log.ErrorFormat("Failed to set invalidation key on topic notifications key = {0}", invalidationKey);
                }

                response.Object = topicNotification;
                response.SetStatus(eResponseStatus.OK, eResponseStatus.OK.ToString());
            }
            catch (Exception ex)
            {
                log.ErrorFormat("CreateTopicNotification failed groupId = {0}, ex = {1}", topicNotification.GroupId, ex);
                return response;
            }

            return response;
        }

        public static GenericResponse<TopicNotification> Update(int groupId, TopicNotification topicNotification, long userId)
        {
            GenericResponse<TopicNotification> response = new GenericResponse<TopicNotification>();
            string logData = string.Format("groupId: {0}, topicNotificationId: {1}", groupId, topicNotification.Id);

            try
            {
                // get currnt topicNotification
                TopicNotification currentTopicNotification = NotificationDal.GetTopicNotificationCB(topicNotification.Id);
                if (topicNotification == null)
                {
                    log.ErrorFormat("TopicNotification wasn't found. {0}", logData);
                    response.SetStatus(eResponseStatus.TopicNotificationNotFound);
                    return response;
                }

                if (!currentTopicNotification.Name.Equals(topicNotification.Name) || !currentTopicNotification.Description.Equals(topicNotification.Description))
                {
                    // update DB topicNotification
                    if (!NotificationDal.UpdateTopicNotification(topicNotification.Id, topicNotification.GroupId, topicNotification.Name, userId))
                    {
                        log.DebugFormat("failed to update TopicNotification to DB groupID = {0}, topicName {1}", topicNotification.GroupId, topicNotification.Name);
                        response.SetStatus(eResponseStatus.Error, "fail update TopicNotification to DB");
                        return response;
                    }

                    currentTopicNotification.Name = topicNotification.Name;
                    currentTopicNotification.Description = topicNotification.Description;

                    // Save TopicNotificationAtCB                    
                    if (!NotificationDal.SaveTopicNotificationCB(topicNotification.GroupId, currentTopicNotification))
                    {
                        log.ErrorFormat("Error while saving topicNotification. groupId: {0}, topicNotificationId:{1}", topicNotification.GroupId, topicNotification.Id);
                    }

                    string invalidationKey = LayeredCacheKeys.GetTopicNotificationsInvalidationKey(groupId, (int)topicNotification.SubscribeReference.Type);
                    if (!LayeredCache.Instance.SetInvalidationKey(invalidationKey))
                    {
                        log.ErrorFormat("Failed to set invalidation key on topic notifications key = {0}", invalidationKey);
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Update TopicNotification failed groupId = {0}, ex = {1}", topicNotification.GroupId, ex);
                return response;
            }

            return response;

        }

        public static Status Delete(int groupId, long userId, long topicNotificationId)
        {
            Status response = new Status() { Code = (int)eResponseStatus.Error, Message = eResponseStatus.Error.ToString() };
            string logData = string.Format("GID: {0}, topicNotificationId: {1}", groupId, topicNotificationId);

            try
            {
                // get topicNotification
                TopicNotification topicNotification = NotificationDal.GetTopicNotificationCB(topicNotificationId);
                if (topicNotification == null)
                {
                    log.ErrorFormat("TopicNotification wasn't found. {0}", logData);
                    response.Code = (int)eResponseStatus.TopicNotificationNotFound;
                    return response;
                }

                // delete Amazon topic
                if (topicNotification.PushExternalId != null &&
                    !NotificationAdapter.DeleteAnnouncement(groupId, topicNotification.PushExternalId))
                {
                    log.ErrorFormat("Error while trying to delete TopicNotification topic from external adapter. {0}, external topic ID: {1}",
                        logData, topicNotification.PushExternalId != null ? topicNotification.PushExternalId : string.Empty);
                }
                else
                {
                    log.DebugFormat("Successfully removed TopicNotification from external adapter. {0},  external topic ID: {1}",
                        logData, topicNotification.PushExternalId != null ? topicNotification.PushExternalId : string.Empty);
                }

                // delete topicNotification from DB
                if (!NotificationDal.DeleteTopicNotification(groupId, userId, topicNotificationId))
                {
                    log.ErrorFormat("Error while trying to delete DB topicNotification. {0}", logData);
                    return response;
                }
                else
                {
                    // remove from CB
                    if (!NotificationDal.DeleteTopicNotificationCB(topicNotificationId))
                    {
                        log.ErrorFormat("Error while delete TopicNotification CB. groupId: {0}, TopicNotification:{1}", groupId, topicNotificationId);
                        return response;
                    }

                    string invalidationKey = LayeredCacheKeys.GetTopicNotificationsInvalidationKey(groupId, (int)topicNotification.SubscribeReference.Type);
                    if (!LayeredCache.Instance.SetInvalidationKey(invalidationKey))
                    {
                        log.ErrorFormat("Failed to set invalidation key on topic notifications key = {0}", invalidationKey);
                    }

                    response = new Status() { Code = (int)eResponseStatus.OK, Message = eResponseStatus.OK.ToString() };
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Delete TopicNotification failed topicNotificationId = {0}, ex = {1}", topicNotificationId, ex);
                return response;
            }

            return response;
        }

        public static GenericListResponse<TopicNotification> List(int groupId, SubscribeReference subscribeReference, bool onlyType = false)
        {
            //TODO ANAT
            GenericListResponse<TopicNotification> response = new GenericListResponse<TopicNotification>();

            //1. get list of all topics by SubscribeReferenceType (layered-cache) 
            List<TopicNotification> topics = null;
            if (NotificationCache.TryGetTopicNotifications(groupId, subscribeReference, ref topics))
            {
                if (onlyType)
                {
                    response.Objects = topics;
                }
                else
                {
                    //2. filter by SubscribeReference (SubId) to new Object !!!
                    response.Objects.Add(topics.FirstOrDefault(x => x.SubscribeReference.GetSubscribtionReferenceId() == subscribeReference.GetSubscribtionReferenceId()));
                }

                response.SetStatus(eResponseStatus.OK);
            }

            return response;
        }

        public static Status Subscribe(int groupId, long topicNotificationId, int userId)
        {
            Status response = new Status();
            try
            {
                TopicNotification topicNotification = NotificationDal.GetTopicNotificationCB(topicNotificationId);

                if (topicNotification == null)
                {
                    response.Code = (int)eResponseStatus.TopicNotificationNotFound;
                    return response;
                }

                // get user notifications
                UserNotification userNotificationData = null;
                response = Utils.GetUserNotificationData(groupId, userId, out userNotificationData);
                if (response.Code != (int)eResponseStatus.OK || userNotificationData == null)
                {
                    return response;
                }

                // create added time
                long addedSecs = DateUtils.DateTimeToUtcUnixTimestampSeconds(DateTime.UtcNow);

                if (userNotificationData.Settings.EnableMail.HasValue
                    && userNotificationData.Settings.EnableMail.Value
                    && !string.IsNullOrEmpty(userNotificationData.UserData.Email))
                {
                    if (!MailNotificationAdapterClient.SubscribeToAnnouncement(topicNotification.GroupId,
                                                                               new List<string>() { topicNotification.MailExternalId },
                                                                               userNotificationData.UserData,
                                                                               userId))
                    {
                        log.ErrorFormat("Failed subscribing user to email topic. group: {0}, userId: {1}, email: {2}",
                                        topicNotification.GroupId, userId, userNotificationData.UserData.Email);
                    }
                }

                HandleSubscribePush(userId, userNotificationData, topicNotification, addedSecs);
                HandleSubscribeSms(userId, userNotificationData, topicNotification, addedSecs);

                // update user notification object
                userNotificationData.TopicNotifications.Add(new Announcement()
                {
                    AnnouncementId = topicNotification.Id,
                    AnnouncementName = topicNotification.Name,
                    AddedDateSec = addedSecs,
                });

                if (!DAL.NotificationDal.SetUserNotificationData(topicNotification.GroupId, userId, userNotificationData))
                {
                    log.ErrorFormat("error setting user notification data. group: {0}, user id: {1}", topicNotification.GroupId, userId);
                }
                /*
                else
                {
                    // update user following items
                    if (!NotificationDal.SetUserFollowNotificationData(topicNotification.GroupId, userId, (int)topicNotification.Id))
                        log.ErrorFormat("Error updating the user following notification data. GID :{0}, user ID: {1}, Announcement ID: {2}", followItem.GroupId, userId, followItem.AnnouncementId);
                    else
                        log.DebugFormat("successfully set notification announcements inbox mapping. group: {0}, user id: {1}, Announcements ID: {2}", followItem.GroupId, userId, (int)followItem.AnnouncementId);

                    log.DebugFormat("successfully updated user notification data. group: {0}, user id: {1}", followItem.GroupId, userId);
                }
                */
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error in Subscribe TopicNotificationId: {0}. ex : {1}", topicNotificationId, ex);
                response.Set(eResponseStatus.Error);
            }

            return response;
        }

        public static Status Unsubscribe(int groupId, long topicNotificationId, int userId)
        {
            Status status = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            try
            {
                TopicNotification topicNotification = NotificationDal.GetTopicNotificationCB(topicNotificationId);

                if (topicNotification == null)
                {
                    status.Code = (int)eResponseStatus.TopicNotificationNotFound;
                    return status;
                }

                // get user notification data
                bool docExists = false;
                UserNotification userNotificationData = NotificationDal.GetUserNotificationData(groupId, userId, ref docExists);

                if (userNotificationData == null || userNotificationData.TopicNotifications == null ||
                   userNotificationData.TopicNotifications?.Count(x => x.AnnouncementId == topicNotification.Id) == 0)
                {
                    log.DebugFormat("user notification data wasn't found. GID: {0}, UID: {1}", groupId, userId);
                    status = new Status((int)eResponseStatus.UserNotFollowing, "user is not subscibe  to topic");
                    return status;
                }

                if (!string.IsNullOrEmpty(userNotificationData.UserData.Email) &&
                userNotificationData.Settings.EnableMail.HasValue &&
                userNotificationData.Settings.EnableMail.Value)
                {
                    MailNotificationAdapterClient.UnSubscribeToAnnouncement(groupId, new List<string>() { topicNotification.MailExternalId }, userNotificationData.UserData, userId);
                }

                HandleUnsubscribeSms(groupId, userId, topicNotification.Id);
                HandleUnsubscribePush(groupId, userId, userNotificationData, topicNotification.Id);

                // remove announcement from user announcement list
                Announcement announcement = userNotificationData.TopicNotifications.First(x => x.AnnouncementId == topicNotification.Id);
                if (announcement != null)
                {
                    if (!userNotificationData.TopicNotifications.Remove(announcement) ||
                    !DAL.NotificationDal.SetUserNotificationData(groupId, userId, userNotificationData))
                    {
                        log.DebugFormat("an error while trying to remove topic. GID: {0}, UID: {1}, topicId: {2}", groupId, userId, topicNotification.Id);
                        status = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                    }
                    else
                    {
                        log.DebugFormat("Successfully removed topic from user topics object group: {0}, UID: {1}", groupId, userId);
                        status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error in UnSubscribe TopicNotificationId: {0}. ex : {1}", topicNotificationId, ex);
                status.Set(eResponseStatus.Error);
            }

            return status;
        }

        private static void HandleSubscribePush(int userId, UserNotification userNotificationData, TopicNotification topicNotification, long addedSecs)
        {
            if (userNotificationData.devices != null &&
                userNotificationData.devices.Count > 0 &&
                NotificationSettings.IsPartnerPushEnabled(topicNotification.GroupId) &&
                NotificationSettings.IsUserFollowPushEnabled(userNotificationData.Settings))
            {
                bool docExists = false;

                foreach (UserDevice device in userNotificationData.devices)
                {
                    string udid = device.Udid;
                    if (string.IsNullOrEmpty(udid))
                    {
                        log.Error("device UDID is empty: " + device.Udid);
                        continue;
                    }

                    log.DebugFormat("adding topicNotification to device group: {0}, user: {1}, UDID: {2}, topicNotificationId: {3}", topicNotification.GroupId, userId, udid, topicNotification.Id);

                    // get device notification data
                    docExists = false;
                    DeviceNotificationData deviceNotificationData = DAL.NotificationDal.GetDeviceNotificationData(topicNotification.GroupId, udid, ref docExists);
                    if (deviceNotificationData == null)
                    {
                        log.ErrorFormat("device notification data not found group: {0}, UDID: {1}", topicNotification.GroupId, device.Udid);
                        continue;
                    }

                    try
                    {
                        // validate device doesn't already have the announcement
                        var isSubscribedTopic = deviceNotificationData.SubscribedTopics.Count(x => x.Id == topicNotification.Id) > 0;
                        if (isSubscribedTopic)
                        {
                            log.ErrorFormat("user already following announcement on device. group: {0}, UDID: {1}", topicNotification.GroupId, device.Udid);
                            continue;
                        }

                        // get push data
                        PushData pushData = PushAnnouncementsHelper.GetPushData(topicNotification.GroupId, udid, string.Empty);
                        if (pushData == null)
                        {
                            log.ErrorFormat("push data not found. group: {0}, UDID: {1}", topicNotification.GroupId, device.Udid);
                            continue;
                        }

                        // subscribe device to announcement
                        AnnouncementSubscriptionData subData = new AnnouncementSubscriptionData()
                        {
                            EndPointArn = pushData.ExternalToken, // take from pushdata (with UDID)
                            Protocol = EnumseDeliveryProtocol.application,
                            TopicArn = topicNotification.PushExternalId,
                            ExternalId = topicNotification.Id
                        };

                        List<AnnouncementSubscriptionData> subs = new List<AnnouncementSubscriptionData>() { subData };
                        subs = NotificationAdapter.SubscribeToAnnouncement(topicNotification.GroupId, subs);
                        if (subs == null || subs.Count == 0 || string.IsNullOrEmpty(subs.First().SubscriptionArnResult))
                        {
                            log.ErrorFormat("Error registering device to announcement. group: {0}, UDID: {1}", topicNotification.GroupId, device.Udid);
                            continue;
                        }

                        // update device notification object
                        NotificationSubscription sub = new NotificationSubscription()
                        {
                            ExternalId = subs.First().SubscriptionArnResult,
                            Id = topicNotification.Id,
                            SubscribedAtSec = addedSecs
                        };
                        deviceNotificationData.SubscribedTopics.Add(sub);

                        if (!DAL.NotificationDal.SetDeviceNotificationData(topicNotification.GroupId, udid, deviceNotificationData))
                            log.ErrorFormat("error setting device notification data. group: {0}, UDID: {1}, topic: {2}", topicNotification.GroupId, device.Udid, subData.EndPointArn);
                        else
                        {
                            log.DebugFormat("Successfully registered device to announcement. group: {0}, UDID: {1}, topic: {2}", topicNotification.GroupId, device.Udid, subData.EndPointArn);
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Error("Error in follow for push", ex);
                    }
                }
            }
        }

        private static void HandleSubscribeSms(int userId, UserNotification userNotificationData, TopicNotification topicNotification, long addedSecs)
        {
            if (NotificationSettings.IsPartnerSmsNotificationEnabled(topicNotification.GroupId) &&
                userNotificationData.Settings.EnableSms.HasValue &&
                userNotificationData.Settings.EnableSms.Value &&
                !string.IsNullOrEmpty(userNotificationData.UserData.PhoneNumber))
            {
                try
                {
                    SmsNotificationData userSmsNotificationData = DAL.NotificationDal.GetUserSmsNotificationData(topicNotification.GroupId, userNotificationData.UserId);
                    if (userSmsNotificationData == null)
                    {
                        log.DebugFormat("user sms notification data is empty {0}", userId);
                        return;
                    }

                    bool isSubscribedTopic = userSmsNotificationData.SubscribedTopics.Count(x => x.Id == topicNotification.Id) > 0;
                    if (isSubscribedTopic)
                    {
                        log.ErrorFormat("user already following topic on sms. userId: {0}", userId);
                        return;
                    }

                    AnnouncementSubscriptionData subData = new AnnouncementSubscriptionData()
                    {
                        EndPointArn = userNotificationData.UserData.PhoneNumber,
                        Protocol = EnumseDeliveryProtocol.sms,
                        TopicArn = topicNotification.PushExternalId,
                        ExternalId = topicNotification.Id
                    };

                    List<AnnouncementSubscriptionData> subs = new List<AnnouncementSubscriptionData>() { subData };
                    subs = NotificationAdapter.SubscribeToAnnouncement(topicNotification.GroupId, subs);
                    if (subs == null || subs.Count == 0 || string.IsNullOrEmpty(subs.First().SubscriptionArnResult))
                    {
                        log.ErrorFormat("Error registering sms to announcement. userId: {0}, PhoneNumber: {1}", userId, userNotificationData.UserData.PhoneNumber);
                        return;
                    }

                    // update notification object
                    NotificationSubscription sub = new NotificationSubscription()
                    {
                        ExternalId = subs.First().SubscriptionArnResult,
                        Id = topicNotification.Id,
                        SubscribedAtSec = addedSecs
                    };
                    userSmsNotificationData.SubscribedTopics.Add(sub);

                    if (!DAL.NotificationDal.SetUserSmsNotificationData(topicNotification.GroupId, userId, userSmsNotificationData))
                    {
                        log.ErrorFormat("error setting sms notification data. group: {0}, userId: {1}, topic: {2}", topicNotification.GroupId, userId, subData.EndPointArn);
                    }
                    else
                    {
                        log.DebugFormat("Successfully registered device to announcement. group: {0}, userId: {1}, topic: {2}", topicNotification.GroupId, userId, subData.EndPointArn);
                    }
                }
                catch (Exception ex)
                {
                    log.Error("Error in follow for sms", ex);
                }
            }
        }

        internal static TopicNotification GetTopicNotificationById(long topicNotificationId)
        {
            TopicNotification topicNotification = null;

            var topics = NotificationDal.GetTopicsNotificationsCB(new List<long>() { topicNotificationId });
            if (topics != null & topics.Count > 0)
            {
                topicNotification = topics[0];
            }

            return topicNotification;
        }

        private static void HandleUnsubscribeSms(int groupId, int userId, long topicNotificationId)
        {
            SmsNotificationData smsNotificationData = NotificationDal.GetUserSmsNotificationData(groupId, userId);
            if (smsNotificationData != null)
            {
                var subscribedTopics = smsNotificationData.SubscribedTopics.First(x => x.Id == topicNotificationId);
                if (subscribedTopics == null)
                {
                    log.DebugFormat("sms notification data had no subscription to topic. group: {0}, userId: {1}", groupId, userId);
                    return;
                }

                // unsubscribe sms 
                List<UnSubscribe> unsubscibeList = new List<UnSubscribe>()
                    {
                        new UnSubscribe()
                        {
                            SubscriptionArn = subscribedTopics.ExternalId
                        }
                    };

                unsubscibeList = NotificationAdapter.UnSubscribeToAnnouncement(groupId, unsubscibeList);
                if (unsubscibeList == null ||
                    unsubscibeList.Count == 0 ||
                    !unsubscibeList.First().Success
                    || !smsNotificationData.SubscribedTopics.Remove(subscribedTopics)
                    || !DAL.NotificationDal.SetUserSmsNotificationData(groupId, userId, smsNotificationData))
                {
                    log.ErrorFormat("error removing topic from sms subscribed. group: {0}, userId: {1}", groupId, userId);
                }
                else
                    log.DebugFormat("Successfully unsubscribed device from topic group: {0}, userId: {1}", groupId, userId);
            }
        }

        private static void HandleUnsubscribePush(int groupId, int userId, UserNotification userNotificationData, long topicNotificationId)
        {
            if (userNotificationData.devices == null || userNotificationData.devices.Count == 0)
                log.DebugFormat("User doesn't have any devices. PID: {0}, UID: {1}", groupId, userId);
            else
            {
                bool docExists = false;
                foreach (UserDevice device in userNotificationData.devices)
                {
                    string udid = device.Udid;
                    if (string.IsNullOrEmpty(udid))
                    {
                        log.ErrorFormat("device UDID invalid: UDID: {0} PID: {1}, UID: {2}", device.Udid, groupId, userId);
                        continue;
                    }

                    // get device data
                    DeviceNotificationData deviceNotificationData = DAL.NotificationDal.GetDeviceNotificationData(groupId, udid, ref docExists);
                    if (deviceNotificationData == null)
                    {
                        log.ErrorFormat("device notification data not found group: {0}, UID: {1}, UDID: {2}", groupId, userId, device.Udid);
                        continue;
                    }

                    // get device subscription
                    var subscribedTopics = deviceNotificationData.SubscribedTopics.First(x => x.Id == topicNotificationId);
                    if (subscribedTopics == null)
                    {
                        log.ErrorFormat("device notification data had no subscription to topic. group: {0}, UID: {1}, UDID: {2}", groupId, userId, device.Udid);
                        continue;
                    }

                    // unsubscribe device 
                    List<UnSubscribe> unsubscibeList = new List<UnSubscribe>()
                    {
                        new UnSubscribe()
                        {
                            SubscriptionArn = subscribedTopics.ExternalId
                        }
                    };

                    unsubscibeList = NotificationAdapter.UnSubscribeToAnnouncement(groupId, unsubscibeList);
                    if (unsubscibeList == null ||
                        unsubscibeList.Count == 0 ||
                        !unsubscibeList.First().Success
                        || !deviceNotificationData.SubscribedTopics.Remove(subscribedTopics)
                        || !DAL.NotificationDal.SetDeviceNotificationData(groupId, udid, deviceNotificationData))
                    {
                        log.ErrorFormat("error removing topic from device subscribed. group: {0}, UID: {1}, UDID: {2}", groupId, userId, device.Udid);
                        continue;
                    }
                    else
                        log.DebugFormat("Successfully unsubscribed device from topic group: {0}, UID: {1}, UDID: {2}", groupId, userId, device.Udid);
                }
            }
        }

    }
}
