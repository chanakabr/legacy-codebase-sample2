using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ApiObjects;
using ApiObjects.Notification;
using ApiObjects.QueueObjects;
using ApiObjects.Response;
using DAL;
using KLogMonitor;
using Newtonsoft.Json;
using Core.Notification.Adapters;
using QueueWrapper.Queues.QueueObjects;
using APILogic.AmazonSnsAdapter;
using APILogic.DmsService;

namespace Core.Notification
{
    public class PushAnnouncementsHelper
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private const string ROUTING_KEY_INITIATE_NOTIFICATION_ACTION = "PROCESS_INITIATE_NOTIFICATION_ACTION";

        /// <summary>
        /// initialize subscription of user announcements + login announcement for notification adapter
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="userNotificationData"></param>
        /// <param name="pushExternalToken"></param>
        /// <returns></returns>
        public static List<AnnouncementSubscriptionData> InitAllAnnouncementToSubscribeForAdapter(int groupId, UserNotification userNotificationData, DeviceNotificationData deviceData, string pushExternalToken, out long loginAnnouncementId)
        {
            List<AnnouncementSubscriptionData> result = new List<AnnouncementSubscriptionData>();
            loginAnnouncementId = 0;

            // validate user enabled push notifications
            if (!NotificationSettings.IsUserPushEnabled(userNotificationData.Settings))
            {
                log.ErrorFormat("User push notification is disabled. PID: {0}, UID: {1}", groupId, userNotificationData.UserId);
                return result;
            }

            // Get list of announcements to subscribe (VOD)
            List<AnnouncementSubscriptionData> temp = PrepareUserAnnouncementsSubscriptions(groupId, userNotificationData, deviceData, pushExternalToken, out loginAnnouncementId);
            if (temp != null)
                result.AddRange(temp);
            temp = null;

            // Get list of reminders to subscribe (EGP)
            temp = PrepareUserRemindersSubscriptions(groupId, userNotificationData, deviceData, pushExternalToken);
            result.AddRange(temp);
            temp = null;

            // Get list of series reminders to subscribe (EPG)
            temp = PrepareUserSeriesRemindersSubscriptions(groupId, userNotificationData, deviceData, pushExternalToken);
            result.AddRange(temp);
            temp = null;

            return result;
        }

        /// <summary>
        /// initialize the cancel subscription list taken from the device object
        /// </summary>
        /// <param name="deviceData"></param>
        /// <returns></returns>
        public static List<UnSubscribe> InitAllAnnouncementToUnSubscribeForAdapter(DeviceNotificationData deviceData)
        {
            List<UnSubscribe> result = null;

            if (deviceData == null)
                log.Debug("user device data is empty");
            else
            {
                // prepare unsubscribe guest/login announcement object
                result = new List<UnSubscribe>();
                if (!string.IsNullOrEmpty(deviceData.SubscriptionExternalIdentifier))
                    result.Add(new UnSubscribe() { SubscriptionArn = deviceData.SubscriptionExternalIdentifier });

                // prepare announcement subscription to cancel list
                if (deviceData.SubscribedAnnouncements != null)
                {
                    UnSubscribe subscriptionToRemove;
                    foreach (var subscription in deviceData.SubscribedAnnouncements)
                    {
                        subscriptionToRemove = new UnSubscribe() { SubscriptionArn = subscription.ExternalId, ExternalId = subscription.Id };
                        result.Add(subscriptionToRemove);
                    }
                }

                // prepare reminder subscription to cancel list
                if (deviceData.SubscribedReminders != null)
                {
                    UnSubscribe subscriptionToRemove;
                    foreach (var reminderSubscription in deviceData.SubscribedReminders)
                    {
                        subscriptionToRemove = new UnSubscribe() { SubscriptionArn = reminderSubscription.ExternalId, ExternalId = reminderSubscription.Id };
                        result.Add(subscriptionToRemove);
                    }
                }

                // prepare series reminder subscription to cancel list
                if (deviceData.SubscribedSeriesReminders != null)
                {
                    UnSubscribe subscriptionToRemove;
                    foreach (var reminderSubscription in deviceData.SubscribedSeriesReminders)
                    {
                        subscriptionToRemove = new UnSubscribe() { SubscriptionArn = reminderSubscription.ExternalId, ExternalId = reminderSubscription.Id };
                        result.Add(subscriptionToRemove);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// initialize guest subscription for notification adapter
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="pushExternalToken"></param>
        /// <returns></returns>
        public static AnnouncementSubscriptionData InitGuestAnnouncementToSubscribeForAdapter(int groupId, string pushExternalToken, UserNotification userNotificationData)
        {
            AnnouncementSubscriptionData result = null;

            // validate user enabled push notifications
            if (userNotificationData != null &&
                userNotificationData.Settings != null &&
                !NotificationSettings.IsUserPushEnabled(userNotificationData.Settings))
            {
                log.ErrorFormat("User push notification is disabled. PID: {0}, UID: {1}", groupId, userNotificationData.UserId);
                return result;
            }

            // get guest announcement external token from DB
            var guestAnnouncement = NotificationCache.Instance().GetAnnouncements(groupId).Where(x => x.RecipientsType == eAnnouncementRecipientsType.Guests).FirstOrDefault();

            // build announcements adapter object
            if (guestAnnouncement == null)
                log.Error("Error while trying to fetch guest announcement from DB.");
            else
            {
                result = new AnnouncementSubscriptionData()
                {
                    Protocol = EnumseDeliveryProtocol.application,
                    TopicArn = guestAnnouncement.ExternalId,
                    EndPointArn = pushExternalToken,
                    ExternalId = guestAnnouncement.ID
                };
            }

            return result;
        }

        /// <summary>
        /// initialize device registration for notification adapter
        /// </summary>
        /// <param name="PushData"></param>
        /// <returns></returns>
        public static DeviceAppRegistration InitDeviceRegistrationForAdapter(PushData PushData)
        {
            DeviceAppRegistration deviceRegistration = null;

            if (string.IsNullOrEmpty(PushData.ApplicationExternalToken))
                log.ErrorFormat("Error while trying to prepare push registration object. ApplicationExternalToken wasn't found. push data: {0}", JsonConvert.SerializeObject(PushData));
            else
            {
                if (string.IsNullOrEmpty(PushData.Token))
                    log.ErrorFormat("Error while trying to prepare push registration object. token wasn't found. push data: {0}", JsonConvert.SerializeObject(PushData));
                else
                {
                    deviceRegistration = new DeviceAppRegistration()
                    {
                        EndpointArn = PushData.ExternalToken != null ? PushData.ExternalToken : string.Empty,
                        PlatformApplicationArn = PushData.ApplicationExternalToken,
                        Token = PushData.Token
                    };
                }
            }

            return deviceRegistration;
        }

        public static PushData GetPushData(int groupId, string udid, string pushToken)
        {
            PushData pushData = null;

            pushData = DmsAdapter.GetPushData(groupId, udid);
            if (pushData == null)
                log.ErrorFormat("Error while trying to get push data from DMS. GID: {0}, UDID: {1}", groupId, udid);
            else
            {
                // validate device token exists/given
                if (string.IsNullOrEmpty(pushToken) && string.IsNullOrEmpty(pushData.Token))
                {
                    pushData = null;
                    log.ErrorFormat("device doesn't have push token. GID: {0}, UDID: {1}", groupId, udid);
                }
                else
                {
                    // update token
                    if (!string.IsNullOrEmpty(pushToken))
                        pushData.Token = pushToken;
                }
            }
            return pushData;
        }

        public static List<AnnouncementSubscriptionData> PrepareUserAnnouncementsSubscriptions(int groupId, UserNotification userNotificationData, DeviceNotificationData deviceData, string pushExternalToken, out long loginAnnouncementId)
        {
            List<AnnouncementSubscriptionData> result = new List<AnnouncementSubscriptionData>();
            loginAnnouncementId = 0;

            // get notification list from user object
            List<long> notificationIds = new List<long>();
            if (userNotificationData != null && userNotificationData.Announcements != null)
                notificationIds = userNotificationData.Announcements.Select(x => x.AnnouncementId).ToList();
            else
                log.DebugFormat("User doesn't follow anything. PID: {0}, UID: {1}", groupId, userNotificationData.UserId);

            // get announcements
            result = new List<AnnouncementSubscriptionData>();
            var announcements = NotificationCache.Instance().GetAnnouncements(groupId).Where(x => x.RecipientsType == eAnnouncementRecipientsType.LoggedIn || notificationIds.Contains(x.ID)).ToList();

            // build announcements adapter object
            if (announcements == null || announcements.Count == 0)
                log.ErrorFormat("Error while trying to fetch announcements from DB. login announcement + announcements ID: {0}", JsonConvert.SerializeObject(notificationIds));
            else
            {
                AnnouncementSubscriptionData announcement;
                foreach (var ann in announcements)
                {
                    // validate external announcement ID
                    string topicArn = ann.ExternalId;
                    if (string.IsNullOrEmpty(topicArn))
                    {
                        log.ErrorFormat("announcement doesn't have external token ID. rowAnnouncement: {0}", JsonConvert.SerializeObject(ann));
                        continue;
                    }

                    // find logged in announcement
                    eAnnouncementRecipientsType announcementType = ann.RecipientsType;

                    // update logged in announcement ID
                    if (announcementType == eAnnouncementRecipientsType.LoggedIn)
                        loginAnnouncementId = ann.ID;

                    // create subscription announcement
                    announcement = new AnnouncementSubscriptionData()
                    {
                        Protocol = EnumseDeliveryProtocol.application,
                        TopicArn = topicArn,
                        EndPointArn = pushExternalToken,
                        ExternalId = ann.ID
                    };

                    // validate user enabled follow push notifications
                    if (!NotificationSettings.IsUserFollowPushEnabled(userNotificationData.Settings) &&
                        announcementType != eAnnouncementRecipientsType.LoggedIn)
                    {
                        log.ErrorFormat("User follow push notification is disabled. PID: {0}, UID: {1}", groupId, userNotificationData.UserId);
                        continue;
                    }

                    // validate device doesn't already has the follow push
                    if (deviceData != null &&
                        deviceData.SubscribedAnnouncements != null &&
                        deviceData.SubscribedAnnouncements.FirstOrDefault(x => x.Id == ann.ID) != null)
                    {
                        log.DebugFormat("Device already has the follow announcement. PID: {0}, UID: {1}, UDID: {2}, announcement ID: {3}",
                            groupId,
                            userNotificationData.UserId,
                            deviceData.Udid,
                            ann.ID);
                        continue;
                    }

                    result.Add(announcement);
                }

                // validate login announcement was fetched 
                if (loginAnnouncementId == 0)
                {
                    log.Error("Error getting the login announcement ID");
                    result = null;
                }
            }

            return result;
        }

        public static List<AnnouncementSubscriptionData> PrepareUserRemindersSubscriptions(int groupId, UserNotification userNotificationData, DeviceNotificationData deviceData, string pushExternalToken)
        {
            List<AnnouncementSubscriptionData> result = new List<AnnouncementSubscriptionData>();

            // get notification list from user object
            List<long> remindersIds = new List<long>();
            if (userNotificationData != null && userNotificationData.Reminders != null)
                remindersIds = userNotificationData.Reminders.Select(x => x.AnnouncementId).ToList();
            else
                log.DebugFormat("User doesn't have any reminders. PID: {0}, UID: {1}", groupId, userNotificationData.UserId);


            // get reminders from DB
            var reminders = NotificationDal.GetReminders(groupId, remindersIds);

            AnnouncementSubscriptionData reminderAnnouncement;
            foreach (var reminderId in remindersIds)
            {
                if (reminders == null || reminders.Count() == 0)
                {
                    log.ErrorFormat("device reminder wasn't found. partner ID: {0}, user ID: {1}, UDID: {2}, reminder ID: {3}",
                        groupId, userNotificationData.UserId,
                        deviceData != null && deviceData.Udid != null ? deviceData.Udid : string.Empty,
                        reminderId);
                    continue;
                }

                var reminder = reminders.FirstOrDefault(x => x.ID == reminderId);
                if (reminder == null)
                {
                    log.ErrorFormat("device reminder wasn't found. partner ID: {0}, user ID: {1}, UDID: {2}, reminder ID: {3}",
                        groupId, userNotificationData.UserId,
                        deviceData != null && deviceData.Udid != null ? deviceData.Udid : string.Empty,
                        reminderId);
                    continue;
                }

                // validate external announcement ID
                string topicArn = reminder.ExternalPushId;
                if (string.IsNullOrEmpty(topicArn))
                {
                    log.ErrorFormat("reminder doesn't have external push ID. reminder: {0}", JsonConvert.SerializeObject(reminder));
                    continue;
                }

                // create subscription announcement
                reminderAnnouncement = new AnnouncementSubscriptionData()
                {
                    Protocol = EnumseDeliveryProtocol.application,
                    TopicArn = topicArn,
                    EndPointArn = pushExternalToken,
                    ExternalId = reminder.ID
                };

                // validate partner enabled push notifications
                if (!NotificationSettings.IsPartnerPushEnabled(groupId))
                {
                    log.ErrorFormat("Partner push notification is disabled. PID: {0}, UID: {1}", groupId, userNotificationData.UserId);
                    break;
                }

                // validate device doesn't already has the follow push
                if (deviceData != null &&
                    deviceData.SubscribedReminders != null &&
                    deviceData.SubscribedReminders.FirstOrDefault(x => x.Id == reminder.ID) != null)
                {
                    log.DebugFormat("Device already has the reminder. PID: {0}, UID: {1}, UDID: {2}, reminder ID: {3}",
                        groupId,
                        userNotificationData.UserId,
                        deviceData.Udid,
                        reminder.ID);
                    continue;
                }

                result.Add(reminderAnnouncement);
            }

            return result;
        }

        public static List<AnnouncementSubscriptionData> PrepareUserSeriesRemindersSubscriptions(int groupId, UserNotification userNotificationData, DeviceNotificationData deviceData, string pushExternalToken)
        {
            List<AnnouncementSubscriptionData> result = new List<AnnouncementSubscriptionData>();

            // get notification list from user object
            List<long> remindersIds = new List<long>();
            if (userNotificationData != null && userNotificationData.SeriesReminders != null)
                remindersIds = userNotificationData.SeriesReminders.Select(x => x.AnnouncementId).ToList();
            else
                log.DebugFormat("User doesn't have any series reminders. PID: {0}, UID: {1}", groupId, userNotificationData.UserId);


            // get reminders from DB
            var reminders = NotificationDal.GetSeriesReminders(groupId, remindersIds);

            AnnouncementSubscriptionData reminderAnnouncement;
            foreach (var reminderId in remindersIds)
            {
                if (reminders == null || reminders.Count() == 0)
                {
                    log.ErrorFormat("device reminder wasn't found. partner ID: {0}, user ID: {1}, UDID: {2}, reminder ID: {3}",
                        groupId, userNotificationData.UserId,
                        deviceData != null && deviceData.Udid != null ? deviceData.Udid : string.Empty,
                        reminderId);
                    continue;
                }

                var reminder = reminders.FirstOrDefault(x => x.ID == reminderId);
                if (reminder == null)
                {
                    log.ErrorFormat("device reminder wasn't found. partner ID: {0}, user ID: {1}, UDID: {2}, reminder ID: {3}",
                        groupId, userNotificationData.UserId,
                        deviceData != null && deviceData.Udid != null ? deviceData.Udid : string.Empty,
                        reminderId);
                    continue;
                }

                // validate external announcement ID
                string topicArn = reminder.ExternalPushId;
                if (string.IsNullOrEmpty(topicArn))
                {
                    log.ErrorFormat("reminder doesn't have external push ID. reminder: {0}", JsonConvert.SerializeObject(reminder));
                    continue;
                }

                // create subscription announcement
                reminderAnnouncement = new AnnouncementSubscriptionData()
                {
                    Protocol = EnumseDeliveryProtocol.application,
                    TopicArn = topicArn,
                    EndPointArn = pushExternalToken,
                    ExternalId = reminder.ID
                };

                // validate partner enabled push notifications
                if (!NotificationSettings.IsPartnerPushEnabled(groupId))
                {
                    log.ErrorFormat("Partner push notification is disabled. PID: {0}, UID: {1}", groupId, userNotificationData.UserId);
                    break;
                }

                // validate device doesn't already has the follow push
                if (deviceData != null &&
                    deviceData.SubscribedReminders != null &&
                    deviceData.SubscribedReminders.FirstOrDefault(x => x.Id == reminder.ID) != null)
                {
                    log.DebugFormat("Device already has the reminder. PID: {0}, UID: {1}, UDID: {2}, reminder ID: {3}",
                        groupId,
                        userNotificationData.UserId,
                        deviceData.Udid,
                        reminder.ID);
                    continue;
                }

                result.Add(reminderAnnouncement);
            }

            return result;
        }
    }
}
