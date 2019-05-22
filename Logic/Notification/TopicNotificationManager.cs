using ApiObjects.Notification;
using ApiObjects.Response;
using Core.Notification.Adapters;
using DAL;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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
                    return response;
                }

                // Save TopicNotificationAtCB                    
                if (!NotificationDal.SaveTopicNotificationCB(topicNotification.GroupId, topicNotification))
                {
                    log.ErrorFormat("Error while saving topicNotification. groupId: {0}, topicNotificationId:{1}", topicNotification.GroupId, topicNotification.Id);
                    //return response;
                }

                //TODO anat cache
                //NotificationCache.Instance().RemoveAnnouncementsFromCache(topicNotification.GroupId);

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

        public static GenericResponse<TopicNotification> Update(int groupId, TopicNotification topicNotification)
        {
            return null;
        }

        public static Status Delete(int groupId, int userId, long topicNotificationId)
        {
            Status responseStatus = new Status() { Code = (int)eResponseStatus.Error, Message = eResponseStatus.Error.ToString() };
            string logData = string.Format("GID: {0}, topicNotificationId: {1}", groupId, topicNotificationId);

            // get announcement
            List<TopicNotification> topicNotifications = null;
            TopicNotification topicNotification = null;
            //TODO anat  NotificationCache.TryGetAnnouncements(groupId, ref announcements);

            if (topicNotifications != null)
                topicNotification = topicNotifications.FirstOrDefault(x => x.Id == topicNotificationId);
            if (topicNotification == null)
            {
                log.ErrorFormat("TopicNotification wasn't found. {0}", logData);
                return new Status() { Code = (int)eResponseStatus.TopicNotificationNotFound, Message = eResponseStatus.TopicNotificationNotFound.ToString() };                
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

            // delete announcement from DB
            if (!NotificationDal.DeleteTopicNotification(groupId, userId, topicNotificationId))
            {
                log.ErrorFormat("Error while trying to delete DB topicNotification. {0}", logData);
            }
            else
            {
                // remove from CB

                if (!NotificationDal.DeleteTopicNotificationCB(topicNotificationId))
                {
                    log.ErrorFormat("Error while delete TopicNotification CB. groupId: {0}, TopicNotification:{1}", groupId, topicNotificationId);
                }
                // TODO anat cache remove announcements from cache
                //NotificationCache.Instance().RemoveAnnouncementsFromCache(groupId);

                responseStatus = new Status() { Code = (int)eResponseStatus.OK, Message = eResponseStatus.OK.ToString() };
                log.DebugFormat("Successfully removed DB topicNotification. {0}", logData);
            }

            return responseStatus;
        }

        public static GenericListResponse<TopicNotification> List(int groupId, SubscribeReference subscribeReference)
        {
            return null;
        }

        public static Status Subscribe(int groupId, long topicNotificationId, int userId)
        {
            Status response = new Status();

            // get user notifications
            UserNotification userNotificationData = null;
            Status getUserNotificationDataStatus = Utils.GetUserNotificationData(groupId, userId, out userNotificationData);
            if (getUserNotificationDataStatus.Code != (int)eResponseStatus.OK || userNotificationData == null)
            {
                response.Set((eResponseStatus)getUserNotificationDataStatus.Code, getUserNotificationDataStatus.Message);
                return response;
            }

            //try
            //{
            //    // get user announcements from DB
            //    DbAnnouncement announcementToFollow = null;
            //    List<DbAnnouncement> dbAnnouncements = null;

            //    if (NotificationCache.TryGetAnnouncements(followItem.GroupId, ref dbAnnouncements))
            //        announcementToFollow = dbAnnouncements.FirstOrDefault(ann => ann.FollowPhrase == followItem.FollowPhrase);

            //    if (announcementToFollow == null)
            //    {
            //        // follow announcement doesn't exists - first time the series is being followed - create a new one
            //        GenericResponse<DbAnnouncement> announcementToFollowResponse = CreateFollowAnnouncement(followItem);

            //        if (announcementToFollowResponse.Status.Code != (int)eResponseStatus.OK)
            //        {
            //            log.ErrorFormat("user notification data not found group: {0}, user: {1}", followItem.GroupId, userId);
            //            response.SetStatus((eResponseStatus)announcementToFollowResponse.Status.Code, announcementToFollowResponse.Status.Message);
            //            return response;
            //        }

            //        announcementToFollow = announcementToFollowResponse.Object;
            //    }

            //    // validate existence of db follow announcement
            //    if (announcementToFollow == null)
            //    {
            //        log.ErrorFormat("announcement not found group: {0}, user: {1}, phrase: {2}",
            //                        followItem.GroupId, userId, followItem.FollowPhrase);
            //        return response;
            //    }

            //    followItem.AnnouncementId = announcementToFollow.ID;
            //    if (userNotificationData.Announcements.Count(x => x.AnnouncementId == followItem.AnnouncementId) > 0)
            //    {
            //        // user already follows the series
            //        log.DebugFormat("User is already following announcement. PID: {0}, UID: {1}, Announcement ID: {2}",
            //                        followItem.GroupId, userId, followItem.AnnouncementId);
            //        response.SetStatus(eResponseStatus.UserAlreadyFollowing, "User already following");
            //        return response;
            //    }

            //    // create added time
            //    long addedSecs = DateUtils.DateTimeToUtcUnixTimestampSeconds(DateTime.UtcNow);

            //    if (userNotificationData.Settings.EnableMail.HasValue &&
            //        userNotificationData.Settings.EnableMail.Value &&
            //        !string.IsNullOrEmpty(userNotificationData.UserData.Email))
            //    {
            //        if (!MailNotificationAdapterClient.SubscribeToAnnouncement(followItem.GroupId,
            //                                                                   new List<string>() { announcementToFollow.MailExternalId },
            //                                                                   userNotificationData.UserData,
            //                                                                   userId))
            //        {
            //            log.ErrorFormat("Failed subscribing user to email announcement. group: {0}, userId: {1}, email: {2}",
            //                            followItem.GroupId, userId, userNotificationData.UserData.Email);
            //        }
            //    }

            //    HandleFollowSms(userId, followItem, userNotificationData, announcementToFollow, addedSecs);
            //    HandleFollowPush(userId, followItem, userNotificationData, announcementToFollow, addedSecs);

            //    // update user notification object
            //    userNotificationData.Announcements.Add(new Announcement()
            //    {
            //        AnnouncementId = followItem.AnnouncementId,
            //        AnnouncementName = announcementToFollow.Name,
            //        AddedDateSec = addedSecs,
            //    });

            //    response.Object = new FollowDataBase(followItem.GroupId, announcementToFollow.FollowPhrase)
            //    {
            //        AnnouncementId = announcementToFollow.ID,
            //        Status = 1,                         // only enabled status in this phase
            //        Title = announcementToFollow.Name,
            //        //Type = FollowType.TV_Series_VOD,  // only TV series in this phase
            //        FollowReference = announcementToFollow.FollowReference,
            //        Timestamp = addedSecs,
            //    };

            //    if (!DAL.NotificationDal.SetUserNotificationData(followItem.GroupId, userId, userNotificationData))
            //        log.ErrorFormat("error setting user notification data. group: {0}, user id: {1}", followItem.GroupId, userId);
            //    else
            //    {
            //        // update user following items
            //        if (!NotificationDal.SetUserFollowNotificationData(followItem.GroupId, userId, (int)followItem.AnnouncementId))
            //            log.ErrorFormat("Error updating the user following notification data. GID :{0}, user ID: {1}, Announcement ID: {2}", followItem.GroupId, userId, followItem.AnnouncementId);
            //        else
            //            log.DebugFormat("successfully set notification announcements inbox mapping. group: {0}, user id: {1}, Announcements ID: {2}", followItem.GroupId, userId, (int)followItem.AnnouncementId);

            //        log.DebugFormat("successfully updated user notification data. group: {0}, user id: {1}", followItem.GroupId, userId);
            //    }

            //    response.SetStatus(eResponseStatus.OK, eResponseStatus.OK.ToString());
            //}
            //catch (Exception ex)
            //{
            //    log.Error("Error in follow", ex);
            //    response.SetStatus(eResponseStatus.Error, eResponseStatus.Error.ToString());
            //}

            return response;
        }

        public static Status Unsubscribe(int groupId, long topicNotificationId, long userId)
        {
            return null;
        }
    }
}  