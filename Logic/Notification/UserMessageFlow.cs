using APILogic.AmazonSnsAdapter;
using APILogic.DmsService;
using ApiObjects;
using ApiObjects.Notification;
using ApiObjects.Response;
using DAL;
using KLogMonitor;
using Newtonsoft.Json;
using Core.Notification.Adapters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TVinciShared;

namespace Core.Notification
{
    public class UserMessageFlow
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public static bool InitiatePushAction(int groupId, eUserMessageAction userAction, int userId, string udid, string pushToken)
        {
            bool result = false;
            bool docExists = false;
            PushData pushData = null;
            DeviceNotificationData deviceData = null;
            UserNotification userNotificationData = null;

            try
            {
                // get user notification data
                if (userId > 0)
                {
                    docExists = false;
                    userNotificationData = NotificationDal.GetUserNotificationData(groupId, userId, ref docExists);
                    if (userNotificationData == null)
                    {
                        if (docExists)
                        {
                            // error while getting user notification data
                            log.ErrorFormat("error retrieving user announcement data. GID: {0}, UID: {1}", groupId, userId);
                            return false;
                        }
                        else
                        {
                            log.DebugFormat("user announcement data wasn't found - going to create a new one. GID: {0}, UID: {1}", groupId, userId);

                            // create user notification object
                            userNotificationData = new UserNotification(userId) { CreateDateSec = DateUtils.UnixTimeStampNow() };

                            //update user settings according to partner settings configuration                    
                            userNotificationData.Settings.EnablePush = NotificationSettings.IsPartnerPushEnabled(groupId, userId);

                            // update user notification data
                            if (!NotificationDal.SetUserNotificationData(groupId, userId, userNotificationData))
                            {
                                log.ErrorFormat("Error while trying to create user notification document", JsonConvert.SerializeObject(userNotificationData));
                                return false;
                            }
                        }
                    }
                    else
                    {
                        // remove old user reminders
                        DeleteOldAnnouncements(groupId, userNotificationData);
                    }
                }

                // get push data
                if (!string.IsNullOrEmpty(udid))
                {
                    pushData = PushAnnouncementsHelper.GetPushData(groupId, udid, pushToken);
                    if (pushData == null)
                    {
                        log.ErrorFormat("push data wasn't found. GID: {0}, UDID: {1}", groupId, udid);
                        return false;
                    }
                    else
                    {
                        // get device notification data
                        docExists = false;
                        deviceData = NotificationDal.GetDeviceNotificationData(groupId, udid, ref docExists);
                        if (deviceData == null)
                            log.DebugFormat("device data wasn't found. GID: {0}, UDID: {1}", groupId, udid);
                    }
                }

                switch (userAction)
                {
                    case eUserMessageAction.Login:

                        result = LoginPushNotification(groupId, userId, false, pushData, userNotificationData, deviceData);
                        if (result)
                            log.Debug("Successfully performed login notification");
                        else
                            log.Error("Error occurred while trying to perform login notification");
                        break;

                    case eUserMessageAction.IdentifyPushRegistration:

                        result = LoginPushNotification(groupId, userId, true, pushData, userNotificationData, deviceData);
                        if (result)
                            log.Debug("Successfully performed Identified Push Registration");
                        else
                            log.Error("Error occurred while trying to perform Identified Push Registration");
                        break;

                    case eUserMessageAction.Logout:

                        result = LogoutPushNotification(groupId, userId, false, pushData, userNotificationData, deviceData);
                        if (result)
                            log.Debug("Successfully performed logout notification");
                        else
                            log.Error("Error occurred while trying to perform logout notification");
                        break;

                    case eUserMessageAction.AnonymousPushRegistration:

                        result = LogoutPushNotification(groupId, userId, true, pushData, userNotificationData, deviceData);
                        if (result)
                            log.Debug("Successfully performed Anonymous Push Registration");
                        else
                            log.Error("Error occurred while trying to perform Anonymous Push Registration");
                        break;

                    case eUserMessageAction.DeleteUser:
                        result = HandlePushDeleteUser(groupId, userId, userNotificationData);
                        if (result)
                            log.Debug("Successfully performed Delete User");
                        else
                            log.Error("Error occurred while trying to perform Delete User");
                        break;

                    case eUserMessageAction.ChangeUsers:

                        UserNotification originalUserNotificationData = NotificationDal.GetUserNotificationData(groupId, deviceData.UserId, ref docExists);
                        if (originalUserNotificationData != null)
                        {
                            result = LogoutPushNotification(groupId, originalUserNotificationData.UserId, true, pushData, originalUserNotificationData, deviceData);
                            if (result)
                                result = LoginPushNotification(groupId, userId, true, pushData, userNotificationData, deviceData);
                        }

                        if (result)
                            log.Debug("Successfully performed Change Users");
                        else
                            log.Error("Error occurred while trying to perform Change Users");
                        break;

                    case eUserMessageAction.EnableUserNotifications:
                        result = EnableUserPushNotification(groupId, userId, userNotificationData);
                        if (result)
                            log.Debug("Successfully enabled user push notifications");
                        else
                            log.Error("Error enabling user push notifications");
                        break;

                    case eUserMessageAction.DisableUserNotifications:
                        result = DisableUserPushNotification(groupId, userId, userNotificationData);
                        if (result)
                            log.Debug("Successfully disabled user push notifications");
                        else
                            log.Error("Error disabling user push notifications");
                        break;

                    default:

                        log.ErrorFormat("Unidentified push action requested. action: {0}", userAction);
                        break;
                }
            }
            catch (Exception ex)
            {
                log.Error("Notification Error", ex);
            }

            return result;
        }

        public static void DeleteOldAnnouncements(int groupId, UserNotification userNotificationData)
        {
            // take reminders 
            List<DbAnnouncement> dbFollowSeries = NotificationCache.Instance().GetAnnouncements(groupId);
            List<DbReminder> dbReminders = new List<DbReminder>();
            List<DbSeriesReminder> dbSeriesReminders = new List<DbSeriesReminder>();
            int numOfFollowSeriesToRemove = 0;
            int numOfRemindersToRemove = 0;

            // remove old reminders
            if (userNotificationData != null &&
                userNotificationData.Reminders != null &&
                userNotificationData.Reminders.Count > 0)
            {
                // get reminders from DB
                dbReminders = NotificationDal.GetReminders(groupId, userNotificationData.Reminders.Select(x => x.AnnouncementId).ToList());

                // remove reminders that did not came back from DB
                if (dbReminders == null || dbReminders.Count == 0)
                {
                    // nothing came back from DB - remove all reminders
                    log.DebugFormat("no reminders in DB - removing all user reminders. partner ID: {0}, user ID: {1}", groupId, userNotificationData.UserId);

                    numOfRemindersToRemove = userNotificationData.Reminders.Count;
                    userNotificationData.Reminders.Clear();
                }
                else
                {
                    // some did not come back from DB - partial removal
                    numOfRemindersToRemove = userNotificationData.Reminders.RemoveAll(x => !dbReminders.Exists(y => y.ID == x.AnnouncementId));
                }
            }

            // remove old series reminders
            if (userNotificationData != null &&
                userNotificationData.SeriesReminders != null &&
                userNotificationData.SeriesReminders.Count > 0)
            {
                // get series reminders from DB
                dbSeriesReminders = NotificationDal.GetSeriesReminders(groupId, userNotificationData.SeriesReminders.Select(x => x.AnnouncementId).ToList());

                // remove reminders that did not came back from DB
                if (dbSeriesReminders == null || dbSeriesReminders.Count == 0)
                {
                    // nothing came back from DB - remove all reminders
                    log.DebugFormat("no series reminders in DB - removing all user reminders. partner ID: {0}, user ID: {1}", groupId, userNotificationData.UserId);

                    numOfRemindersToRemove = userNotificationData.SeriesReminders.Count;
                    userNotificationData.SeriesReminders.Clear();
                }
                else
                {
                    // some did not come back from DB - partial removal
                    numOfRemindersToRemove = userNotificationData.SeriesReminders.RemoveAll(x => !dbSeriesReminders.Exists(y => y.ID == x.AnnouncementId));
                }
            }

            // remove old follow series
            if (userNotificationData != null &&
                userNotificationData.Announcements != null &&
                userNotificationData.Announcements.Count > 0)
            {
                IEnumerable<Announcement> followSeriesToRemove;
                if (dbFollowSeries == null || dbFollowSeries.Count == 0)
                {
                    // no announcements in DB - removing all user announcements
                    log.DebugFormat("no announcements in DB - removing all user announcements. partner ID: {0}, user ID: {1}", groupId, userNotificationData.UserId);

                    followSeriesToRemove = userNotificationData.Announcements;
                    numOfFollowSeriesToRemove = userNotificationData.Announcements.Count;

                    if (followSeriesToRemove != null)
                    {
                        // remove user follow series from user view
                        foreach (var item in followSeriesToRemove)
                        {
                            if (!NotificationDal.RemoveUserFollowNotification(groupId, userNotificationData.UserId, item.AnnouncementId))
                            {
                                log.ErrorFormat("Error while trying to remove user follow notification view. PID: {0}, user ID: {1}, follow series (announcement) ID: {2}",
                                    groupId,
                                    userNotificationData.UserId,
                                    item.AnnouncementId);
                            }
                        }
                    }

                    // remove user announcement from CB object
                    userNotificationData.Announcements.Clear();
                }
                else
                {
                    followSeriesToRemove = userNotificationData.Announcements.Where(x => !dbFollowSeries.Exists(y => y.ID == x.AnnouncementId));

                    if (followSeriesToRemove != null)
                    {
                        // remove user follow series from user view
                        foreach (var item in followSeriesToRemove)
                        {
                            if (!NotificationDal.RemoveUserFollowNotification(groupId, userNotificationData.UserId, item.AnnouncementId))
                            {
                                log.ErrorFormat("Error while trying to remove user follow notification view. PID: {0}, user ID: {1}, follow series (announcement) ID: {2}",
                                    groupId,
                                    userNotificationData.UserId,
                                    item.AnnouncementId);
                            }
                        }
                    }

                    // remove user announcement from CB object
                    numOfFollowSeriesToRemove = userNotificationData.Announcements.RemoveAll(x => !dbFollowSeries.Exists(y => y.ID == x.AnnouncementId));
                }
            }

            if (numOfRemindersToRemove > 0 || numOfFollowSeriesToRemove > 0)
            {
                // update user reminders 
                if (!NotificationDal.SetUserNotificationData(groupId, userNotificationData.UserId, userNotificationData))
                    log.ErrorFormat("Error deleting old user announcements. User ID: {0}", userNotificationData.UserId);
                else
                    log.DebugFormat("old user announcements removed. User ID: {0}", userNotificationData.UserId);

                // iterate through user devices and remove notifications
                if (userNotificationData.devices != null)
                {
                    foreach (var userDevice in userNotificationData.devices)
                    {
                        // get device notification data
                        bool isDocExists = false;
                        DeviceNotificationData deviceData = NotificationDal.GetDeviceNotificationData(groupId, userDevice.Udid, ref isDocExists);
                        if (deviceData == null)
                        {
                            log.DebugFormat("device data wasn't found for deletion of old notifications. GID: {0}, UDID: {1}", groupId, userDevice.Udid);
                            continue;
                        }

                        // remove reminders 
                        int numOfDeviceRemindersToRemove = 0;
                        if (numOfRemindersToRemove > 0)
                        {
                            if (dbReminders == null || dbReminders.Count == 0)
                            {
                                // nothing came back from DB - remove all reminders
                                numOfDeviceRemindersToRemove = deviceData.SubscribedReminders.Count;
                                deviceData.SubscribedReminders.Clear();
                            }
                            else
                            {
                                // some did not come back from DB - partial removal
                                numOfDeviceRemindersToRemove = deviceData.SubscribedReminders.RemoveAll(x => !dbReminders.Exists(y => y.ID == x.Id));
                            }
                        }

                        // remove follow series
                        int numOfDeviceFollowSeriesToRemove = 0;
                        if (numOfFollowSeriesToRemove > 0)
                        {
                            if (dbFollowSeries == null || dbFollowSeries.Count == 0)
                            {
                                // nothing came back from DB - remove all announcements
                                numOfDeviceFollowSeriesToRemove = deviceData.SubscribedAnnouncements.Count;
                                deviceData.SubscribedAnnouncements.Clear();
                            }
                            else
                            {
                                // some did not come back from DB - partial removal
                                numOfDeviceFollowSeriesToRemove = deviceData.SubscribedAnnouncements.RemoveAll(x => !dbFollowSeries.Exists(y => y.ID == x.Id));
                            }

                        }

                        if (numOfDeviceRemindersToRemove > 0 || numOfDeviceFollowSeriesToRemove > 0)
                        {
                            // update device data
                            if (!NotificationDal.SetDeviceNotificationData(groupId, deviceData.Udid, deviceData))
                            {
                                log.ErrorFormat("Error deleting old device notifications. User ID: {0}, UDID: {1}", userNotificationData.UserId, deviceData.Udid);
                                continue;
                            }
                            else
                            {
                                log.DebugFormat("old device notifications removed. User ID: {0}, UDID: {1}. number of reminders removed: {2}, number of follow series removed: {3}",
                                    userNotificationData.UserId,
                                    deviceData.Udid,
                                    numOfDeviceRemindersToRemove,
                                    numOfDeviceFollowSeriesToRemove);
                            }
                        }
                    }
                }
            }
        }

        private static bool HandlePushDeleteUser(int groupId, int userId, UserNotification userNotificationData)
        {
            bool result = true;
            PushData pushData = null;

            if (userNotificationData == null)
            {
                log.DebugFormat("user notification data is empty", groupId, userId);
                return false;
            }

            // get all user devices
            if (userNotificationData.devices != null)
            {
                List<string> udids = userNotificationData.devices.Select(x => x.Udid).ToList();

                foreach (var udid in udids)
                {
                    DeviceNotificationData deviceData = null;
                    if (!string.IsNullOrEmpty(udid))
                    {
                        bool isDocExists = false;
                        deviceData = NotificationDal.GetDeviceNotificationData(groupId, udid, ref isDocExists);
                    }

                    if (deviceData == null || string.IsNullOrEmpty(deviceData.Udid))
                    {
                        log.DebugFormat("device data wasn't found. GID: {0}, UDID: {1}", groupId, pushData.Udid);
                        continue;
                    }

                    pushData = PushAnnouncementsHelper.GetPushData(groupId, udid, string.Empty);
                    if (pushData == null)
                    {
                        log.ErrorFormat("push data wasn't found. GID: {0}, UDID: {1}", groupId, udid);
                        continue;
                    }

                    result &= LogoutPushNotification(groupId, userId, false, pushData, userNotificationData, deviceData, false);

                    // remove device data
                    if (!NotificationDal.RemoveDeviceNotificationData(groupId, udid, deviceData.cas))
                    {
                        log.ErrorFormat("Error while trying to delete device data. GID: {0}, UDID: {1}", groupId, udid);
                        continue;
                    }
                    else
                        log.DebugFormat("Successfully removed device notification data. GID: {0}, UDID: {1}", groupId, udid);
                }
            }

            // remove user data
            if (!NotificationDal.RemoveUserNotificationData(groupId, userId, userNotificationData.cas))
                log.ErrorFormat("Error while trying to remove user notification data. GID: {0}, UID: {1}", groupId, userId);
            else
                log.DebugFormat("Successfully removed user notification data. GID: {0}, UID: {1}", groupId, userId);

            return result;
        }

        private static bool LogoutPushNotification(int groupId, int userId, bool performRegistration, PushData pushData, UserNotification userNotificationData, DeviceNotificationData deviceData, bool allowTopicRegistration = true)
        {
            DeviceAppRegistration deviceRegistration = null;
            if (performRegistration)
            {
                // --> register device
                deviceRegistration = PushAnnouncementsHelper.InitDeviceRegistrationForAdapter(pushData);
                if (deviceRegistration == null)
                {
                    log.ErrorFormat("Error while trying to prepare push registration object in anonymous push data flow. push data: {0}",
                         JsonConvert.SerializeObject(pushData));
                    return false;
                }
            }

            // get old user ID if exists to remove 
            int oldUserIdToRemove = 0;
            if (deviceData != null &&
                deviceData.UserId != 0)
            {
                oldUserIdToRemove = deviceData.UserId;
            }

            // --> subscribe to guest announcement
            AnnouncementSubscriptionData announcementToSubscribe = null;
            List<AnnouncementSubscriptionData> announcementToSubscribeList = null;
            if (allowTopicRegistration)
            {
                announcementToSubscribe = PushAnnouncementsHelper.InitGuestAnnouncementToSubscribeForAdapter(groupId, pushData.ExternalToken, userNotificationData);
                if (announcementToSubscribe == null)
                {
                    log.Error("Error while trying to retrieve guest announcement");
                    return false;
                }
            }

            if (announcementToSubscribe != null)
                announcementToSubscribeList = new List<AnnouncementSubscriptionData>() { announcementToSubscribe };

            // --> cancel previous announcements
            List<UnSubscribe> announcementToUnsubscribe = PushAnnouncementsHelper.InitAllAnnouncementToUnSubscribeForAdapter(deviceData);

            // execute action
            DeviceAppRegistrationAnnouncementResult adapterResult = NotificationAdapter.RegisterDeviceToApplicationAndAnnouncement(groupId, deviceRegistration,
                announcementToSubscribeList, announcementToUnsubscribe);

            if (adapterResult == null)
            {
                log.ErrorFormat("Error received from notification adapter. Logout flow");
                return false;
            }

            // update device notification data
            if (!UpdateDeviceDataAccordingToAdapterResult(groupId, userId, pushData, deviceData, announcementToUnsubscribe, announcementToSubscribeList, adapterResult, 0, false))
            {
                log.ErrorFormat("Error updating device notification data. Logout flow. data: {0}",
                    deviceData != null ? JsonConvert.SerializeObject(deviceData) : string.Empty);
            }
            else
            {
                log.DebugFormat("Successfully updated device notification data. Logout flow. data: {0}",
                    deviceData != null ? JsonConvert.SerializeObject(deviceData) : string.Empty);
            }

            // update user notification data
            if (!UpdateUserDataAccordingToAdapterResult(groupId, userId, pushData, userNotificationData, false, oldUserIdToRemove))
            {
                log.ErrorFormat("Error while trying to updated user notification data. data: {0}",
                    userNotificationData != null ? JsonConvert.SerializeObject(userNotificationData) : string.Empty);
            }
            else
            {
                log.DebugFormat("Successfully updated user notification data. data: {0}",
                    userNotificationData != null ? JsonConvert.SerializeObject(userNotificationData) : string.Empty);
            }

            if (performRegistration)
            {
                // update device DMS data
                if (string.IsNullOrEmpty(adapterResult.EndPointArn))
                {
                    log.ErrorFormat("Error while trying to register device. deviceRegistration: {0}", deviceRegistration);
                    return false;
                }

                if (!DmsAdapter.SetPushData(groupId, new SetPushData() { Udid = pushData.Udid, Token = pushData.Token, ExternalToken = adapterResult.EndPointArn }))
                {
                    log.ErrorFormat("Error while trying update DMS push data");
                    return false;
                }
            }
            return true;
        }

        private static bool DisableUserPushNotifications(int groupId, int userId, PushData pushData, UserNotification userNotificationData, DeviceNotificationData deviceData)
        {
            // --> cancel previous announcements
            List<UnSubscribe> announcementToUnsubscribe = PushAnnouncementsHelper.InitAllAnnouncementToUnSubscribeForAdapter(deviceData);

            if (announcementToUnsubscribe == null || announcementToUnsubscribe.Count == 0)
            {
                log.DebugFormat("No announcement to unsubscribe. PID: {0}, UID: {1}", groupId, userId);
                return true;
            }

            // execute action
            List<UnSubscribe> announcementToUnsubscribeResult = NotificationAdapter.UnSubscribeToAnnouncement(groupId, announcementToUnsubscribe);
            if (announcementToUnsubscribe == null)
            {
                log.ErrorFormat("Error received from notification adapter. unsubscribe all announcements. PID: {0}, UID: {1}", groupId, userId);
                return false;
            }

            // update device notification data
            if (UpdateDeviceDataAccordingToAdapterResult(groupId, userId, pushData, deviceData, announcementToUnsubscribe, announcementToUnsubscribeResult))
            {
                log.ErrorFormat("Error updating device notification data. Disable all notifications flow. data: {0}",
                    deviceData != null ? JsonConvert.SerializeObject(deviceData) : string.Empty);
            }
            else
            {
                log.DebugFormat("Successfully updated device notification data. Disable all notifications flow. data: {0}",
                    deviceData != null ? JsonConvert.SerializeObject(deviceData) : string.Empty);
            }

            return true;
        }

        private static bool LoginPushNotification(int groupId, int userId, bool performRegistration, PushData pushData, UserNotification userNotificationData, DeviceNotificationData deviceData)
        {
            // validate user ID
            if (userId == 0)
            {
                log.Error("User ID wasn't not receive. cannot perform identified set push/login flow.");
                return false;
            }

            // validate push data
            if (pushData == null)
            {
                log.ErrorFormat("Error while trying to register device to notification. device is not registered.");
                return false;
            }

            DeviceAppRegistration deviceRegistration = null;
            if (performRegistration)
            {
                // --> register device
                deviceRegistration = PushAnnouncementsHelper.InitDeviceRegistrationForAdapter(pushData);
                if (deviceRegistration == null)
                {
                    log.ErrorFormat("Error while trying to prepare push registration object in identify push data flow. push data: {0}",
                        JsonConvert.SerializeObject(pushData));
                    return false;
                }
            }

            // --> subscribe to announcements
            long loginAnnouncementId = 0;
            List<AnnouncementSubscriptionData> announcementToSubscribe = PushAnnouncementsHelper.InitAllAnnouncementToSubscribeForAdapter(groupId, userNotificationData, deviceData, pushData.ExternalToken, out loginAnnouncementId);
            if (announcementToSubscribe == null ||
                announcementToSubscribe.Count == 0 ||
                loginAnnouncementId == 0)
            {
                log.Error("Error retrieving login announcement to subscribe");
                return false;
            }

            // --> cancel previous announcements
            List<UnSubscribe> announcementToUnsubscribe = PushAnnouncementsHelper.InitAllAnnouncementToUnSubscribeForAdapter(deviceData);

            // get old user ID if exists to remove 
            int oldUserIdToRemove = 0;
            if (deviceData != null &&
                deviceData.UserId != 0 &&
                deviceData.UserId != userId)
            {
                oldUserIdToRemove = deviceData.UserId;
            }

            // execute action
            DeviceAppRegistrationAnnouncementResult adapterResult = NotificationAdapter.RegisterDeviceToApplicationAndAnnouncement(groupId, deviceRegistration, announcementToSubscribe, announcementToUnsubscribe);

            if (adapterResult == null)
            {
                log.Error("Error while trying to register device + subscribe to notification + unsubscribe notification");
                return false;
            }

            // update device notification data
            if (!UpdateDeviceDataAccordingToAdapterResult(groupId, userId, pushData, deviceData, announcementToUnsubscribe, announcementToSubscribe, adapterResult, loginAnnouncementId, true))
            {
                log.ErrorFormat("Error updating device notification data. data: {0}",
                    deviceData != null ? JsonConvert.SerializeObject(deviceData) : string.Empty);
            }
            else
            {
                log.DebugFormat("Successfully updated device notification data. data: {0}",
                    deviceData != null ? JsonConvert.SerializeObject(deviceData) : string.Empty);
            }

            // update user notification data
            if (!UpdateUserDataAccordingToAdapterResult(groupId, userId, pushData, userNotificationData, true, oldUserIdToRemove))
            {
                log.ErrorFormat("Error while trying to updated user notification data on login flow. data: {0}",
                    userNotificationData != null ? JsonConvert.SerializeObject(userNotificationData) : string.Empty);
            }
            else
            {
                log.DebugFormat("Successfully updated user notification data. data: {0}",
                    userNotificationData != null ? JsonConvert.SerializeObject(userNotificationData) : string.Empty);
            }

            if (performRegistration)
            {
                // update device push registration data
                if (string.IsNullOrEmpty(adapterResult.EndPointArn))
                {
                    log.ErrorFormat("Error while trying to register device. deviceRegistration: {0}", deviceRegistration);
                    return false;
                }

                if (!DmsAdapter.SetPushData(groupId, new SetPushData() { Udid = pushData.Udid, Token = pushData.Token, ExternalToken = adapterResult.EndPointArn }))
                {
                    log.ErrorFormat("Error while trying update DMS push data");
                    return false;
                }
            }

            return true;
        }

        private static bool UpdateDeviceDataAccordingToAdapterResult(int groupId, int userId, PushData pushData,
                                                                    DeviceNotificationData deviceData,
                                                                    List<UnSubscribe> announcementToUnsubscribe,
                                                                    List<UnSubscribe> adapterResult)
        {
            // update device document 
            if (deviceData == null)
            {
                deviceData = new DeviceNotificationData()
                {
                    IsLoggedIn = true,
                    Udid = pushData.Udid,
                    UserId = userId,
                    UpdatedAt = DateUtils.DateTimeToUnixTimestamp(DateTime.UtcNow),
                    SubscribedAnnouncements = new List<NotificationSubscription>()
                };
            }
            else
            {
                deviceData.IsLoggedIn = true;
                deviceData.Udid = pushData.Udid;
                deviceData.UserId = userId;
                deviceData.UpdatedAt = DateUtils.DateTimeToUnixTimestamp(DateTime.UtcNow);
                if (deviceData.SubscribedAnnouncements == null)
                    deviceData.SubscribedAnnouncements = new List<NotificationSubscription>();
            }

            // remove canceled subscriptions
            if (announcementToUnsubscribe != null)
            {
                if (announcementToUnsubscribe.Count > 0 &&
                    (adapterResult == null ||
                    adapterResult.Count == 0))
                {
                    log.ErrorFormat("Error while trying to unsubscribe user announcements. PID: {0}, UID: {1}, UDID: {2}", groupId, userId, pushData.Udid);
                }
                else
                {
                    bool canceledSystemAnnouncement = false;
                    foreach (UnSubscribe cancelSubResult in adapterResult)
                    {
                        // check if cancel passed
                        if (!cancelSubResult.Success)
                        {
                            log.ErrorFormat("Error canceling subscription. cancelSubResult: {0}", JsonConvert.SerializeObject(cancelSubResult));
                            continue;
                        }

                        // check if canceled subscripting is the system announcement 
                        if (cancelSubResult.SubscriptionArn == deviceData.SubscriptionExternalIdentifier)
                        {
                            // remove system announcement
                            deviceData.SubscriptionExternalIdentifier = string.Empty;
                            canceledSystemAnnouncement = true;
                        }

                        // remove subscription from device list
                        if (deviceData.SubscribedAnnouncements.Count > 0)
                        {
                            var removedSub = deviceData.SubscribedAnnouncements.FirstOrDefault(x => x.ExternalId == cancelSubResult.SubscriptionArn);
                            if (removedSub != null)
                                deviceData.SubscribedAnnouncements.Remove(removedSub);
                            else
                            {
                                // remove reminder from device list
                                var removedReminder = deviceData.SubscribedReminders.FirstOrDefault(x => x.ExternalId == cancelSubResult.SubscriptionArn);
                                if (removedReminder != null)
                                    deviceData.SubscribedReminders.Remove(removedReminder);
                            }
                        }
                    }

                    if (!canceledSystemAnnouncement)
                        log.Error("Error canceling system announcement");
                }
            }

            if (!NotificationDal.SetDeviceNotificationData(groupId, pushData.Udid, deviceData))
            {
                log.ErrorFormat("Error while trying to update device data. GID: {0}, UDID: {1}", groupId, pushData.Udid);
                return false;
            }
            else
                return true;
        }

        private static bool UpdateDeviceDataAccordingToAdapterResult(int groupId, int userId, PushData pushData,
                                                                    DeviceNotificationData deviceData, List<UnSubscribe> announcementToUnsubscribe,
                                                                    List<AnnouncementSubscriptionData> announcementToSubscribe,
                                                                    DeviceAppRegistrationAnnouncementResult adapterResult,
                                                                    long loginAnnouncementId, bool isLogin)
        {
            // update device document 
            if (deviceData == null)
            {
                deviceData = new DeviceNotificationData()
                {
                    IsLoggedIn = isLogin,
                    Udid = pushData.Udid,
                    UserId = isLogin ? userId : 0,
                    UpdatedAt = DateUtils.DateTimeToUnixTimestamp(DateTime.UtcNow),
                    SubscribedAnnouncements = new List<NotificationSubscription>()
                };
            }
            else
            {
                deviceData.IsLoggedIn = isLogin;
                deviceData.Udid = pushData.Udid;
                deviceData.UserId = isLogin ? userId : 0;
                deviceData.UpdatedAt = DateUtils.DateTimeToUnixTimestamp(DateTime.UtcNow);
                if (deviceData.SubscribedAnnouncements == null)
                    deviceData.SubscribedAnnouncements = new List<NotificationSubscription>();
                if (deviceData.SubscribedReminders == null)
                    deviceData.SubscribedReminders = new List<NotificationSubscription>();
            }

            // remove canceled subscriptions
            if (announcementToUnsubscribe != null)
            {
                if (announcementToUnsubscribe.Count > 0 &&
                    (adapterResult.AnnounsmentsCancelledSubscriptions == null ||
                    adapterResult.AnnounsmentsCancelledSubscriptions.Count == 0))
                {
                    log.ErrorFormat("Error while trying to unsubscribe user announcements. deviceData: {0}", deviceData);
                }
                else
                {
                    bool canceledSystemAnnouncement = false;
                    foreach (UnSubscribe cancelSubResult in adapterResult.AnnounsmentsCancelledSubscriptions)
                    {
                        // check if cancel passed
                        if (cancelSubResult == null || !cancelSubResult.Success)
                        {
                            log.ErrorFormat("Error canceling subscription. cancelSubResult: {0}", cancelSubResult != null ? JsonConvert.SerializeObject(cancelSubResult) : string.Empty);
                            continue;
                        }

                        // check if canceled subscripting is the system announcement 
                        if (cancelSubResult.SubscriptionArn == deviceData.SubscriptionExternalIdentifier)
                        {
                            // remove system announcement
                            deviceData.SubscriptionExternalIdentifier = string.Empty;
                            canceledSystemAnnouncement = true;
                        }

                        // remove subscription from device list
                        if (deviceData.SubscribedAnnouncements.Count > 0)
                        {
                            var found = deviceData.SubscribedAnnouncements.FirstOrDefault(x => x.ExternalId == cancelSubResult.SubscriptionArn);
                            if (found != null)
                                deviceData.SubscribedAnnouncements.Remove(found);
                        }

                        // remove subscription from device list
                        if (deviceData.SubscribedReminders.Count > 0)
                        {
                            // remove reminder from device list
                            var removedReminder = deviceData.SubscribedReminders.FirstOrDefault(x => x.ExternalId == cancelSubResult.SubscriptionArn);
                            if (removedReminder != null)
                                deviceData.SubscribedReminders.Remove(removedReminder);
                        }
                    }

                    if (!canceledSystemAnnouncement)
                        log.Error("Error canceling system announcement");
                }
            }

            // add new subscriptions
            if (announcementToSubscribe != null)
            {
                if (announcementToSubscribe.Count > 0 &&
                    (adapterResult.AnnounsmentsSubscriptions == null ||
                    adapterResult.AnnounsmentsSubscriptions.Count == 0))
                {
                    log.ErrorFormat("Error while trying to unsubscribe user announcements in identified set push/login flow. deviceData: {0}", deviceData);
                }
                else
                {
                    if (isLogin)
                    {
                        bool loginAnnouncementUpdated = false;

                        // update device announcements
                        foreach (var subscription in adapterResult.AnnounsmentsSubscriptions)
                        {
                            if (subscription == null || string.IsNullOrEmpty(subscription.SubscriptionArnResult))
                            {
                                log.ErrorFormat("Error subscribing announcement. announcement: {0}", subscription != null ? JsonConvert.SerializeObject(subscription) : string.Empty);
                                continue;
                            }

                            // add device announcements except login announcements 
                            if (subscription.ExternalId != loginAnnouncementId)
                            {
                                // add result to follow announcements (if its a follow push announcement)
                                var notifications = NotificationCache.Instance().GetAnnouncements(groupId);
                                if (notifications != null && notifications.FirstOrDefault(x => x.ID == subscription.ExternalId) != null)
                                {
                                    deviceData.SubscribedAnnouncements.Add(new NotificationSubscription()
                                    {
                                        SubscribedAtSec = DateUtils.DateTimeToUnixTimestamp(DateTime.UtcNow),
                                        ExternalId = subscription.SubscriptionArnResult,
                                        Id = subscription.ExternalId
                                    });
                                }
                                else
                                {
                                    // add result to reminders (if its a reminder push announcement)
                                    var reminders = NotificationDal.GetReminders(groupId, subscription.ExternalId);
                                    if (reminders != null && reminders.Count > 0)
                                    {
                                        deviceData.SubscribedReminders.Add(new NotificationSubscription()
                                        {
                                            SubscribedAtSec = DateUtils.DateTimeToUnixTimestamp(DateTime.UtcNow),
                                            ExternalId = subscription.SubscriptionArnResult,
                                            Id = subscription.ExternalId
                                        });
                                    }
                                }
                            }
                            else
                            {
                                // update system announcement
                                deviceData.SubscriptionExternalIdentifier = subscription.SubscriptionArnResult;
                                loginAnnouncementUpdated = true;
                            }
                        }

                        if (!loginAnnouncementUpdated)
                            log.Error("subscribe to login failed");
                    }
                    else
                    {
                        // validate subscription to logout announcement
                        if (adapterResult.AnnounsmentsSubscriptions[0] == null || string.IsNullOrEmpty(adapterResult.AnnounsmentsSubscriptions[0].SubscriptionArnResult))
                            log.ErrorFormat("Error while trying to subscribe device to guest announcements. deviceData: {0}", deviceData);
                        else
                            deviceData.SubscriptionExternalIdentifier = adapterResult.AnnounsmentsSubscriptions[0].SubscriptionArnResult;
                    }
                }
            }

            if (!NotificationDal.SetDeviceNotificationData(groupId, pushData.Udid, deviceData))
            {
                log.ErrorFormat("Error while trying to update device data. GID: {0}, UDID: {1}", groupId, pushData.Udid);
                return false;
            }
            else
                return true;
        }

        private static bool UpdateUserDataAccordingToAdapterResult(int groupId, int userId, PushData pushData, UserNotification userNotificationData, bool isLogin, int userIdToRemove)
        {
            // update user notification object
            if (userNotificationData == null)
            {
                userNotificationData = new UserNotification(userId) { CreateDateSec = DateUtils.UnixTimeStampNow() };

                //update user settings according to partner settings configuration                    
                userNotificationData.Settings.EnablePush = NotificationSettings.IsPartnerPushEnabled(groupId, userId);
            }
            else
            {
                if (userNotificationData.devices == null)
                    userNotificationData.devices = new List<UserDevice>();

                if (userNotificationData.Announcements == null)
                    userNotificationData.Announcements = new List<Announcement>();
            }

            // remove device if exists 
            var deviceExists = userNotificationData.devices.Find(x => x.Udid.Trim().ToLower() == pushData.Udid.Trim().ToLower());
            if (deviceExists != null)
                userNotificationData.devices.Remove(deviceExists);

            // remove old UDID from user object 
            if (userIdToRemove > 0 && userIdToRemove != userId)
            {
                bool docExists = false;
                UserNotification oldUserNotificationData = NotificationDal.GetUserNotificationData(groupId, userIdToRemove, ref docExists);
                if (oldUserNotificationData == null)
                    log.DebugFormat("old user announcement data to remove wasn't found. GID: {0}, UID: {1}", groupId, userIdToRemove);
                else
                {
                    // remove device if exists 
                    var oldExist = oldUserNotificationData.devices.Find(x => x.Udid.Trim().ToLower() == pushData.Udid.Trim().ToLower());
                    if (oldExist != null)
                    {
                        oldUserNotificationData.devices.Remove(oldExist);

                        // update CB
                        if (!NotificationDal.SetUserNotificationData(groupId, userIdToRemove, oldUserNotificationData))
                            log.ErrorFormat("Error while trying to update old user notification data. GID: {0}, UID: {1}", groupId, userId);
                    }
                }
            }

            if (isLogin)
            {
                // add new device
                userNotificationData.devices.Add(new UserDevice()
                {
                    SignInAtSec = DateUtils.DateTimeToUnixTimestamp(DateTime.UtcNow),
                    Udid = pushData.Udid
                });
            }

            // update CB
            if (userId > 0)
            {
                if (!NotificationDal.SetUserNotificationData(groupId, userId, userNotificationData))
                {
                    log.ErrorFormat("Error while trying to update user notification data. GID: {0}, UID: {1}", groupId, userId);
                    return false;
                }
            }

            return true;
        }

        private static bool DisableUserPushNotification(int groupId, int userId, UserNotification userNotificationData)
        {
            bool result = true;
            PushData pushData = null;

            if (userNotificationData == null)
            {
                log.DebugFormat("user notification data is empty", groupId, userId);
                return false;
            }

            if (userNotificationData.devices != null)
            {
                // get user devices
                List<string> udids = userNotificationData.devices.Select(x => x.Udid).ToList();
                foreach (var udid in udids)
                {
                    bool docExists = false;
                    DeviceNotificationData deviceData = NotificationDal.GetDeviceNotificationData(groupId, udid, ref docExists);
                    if (deviceData == null)
                    {
                        log.DebugFormat("device data wasn't found. GID: {0}, UDID: {1}", groupId, pushData.Udid);
                        continue;
                    }

                    pushData = PushAnnouncementsHelper.GetPushData(groupId, udid, string.Empty);
                    if (pushData == null)
                    {
                        log.ErrorFormat("push data wasn't found. GID: {0}, UDID: {1}", groupId, udid);
                        continue;
                    }
                    result &= DisableUserPushNotifications(groupId, userId, pushData, userNotificationData, deviceData);
                }
            }

            return result;
        }

        private static bool EnableUserPushNotification(int groupId, int userId, UserNotification userNotificationData)
        {
            bool result = true;
            PushData pushData = null;

            if (userNotificationData == null)
            {
                log.DebugFormat("user notification data is empty", groupId, userId);
                return false;
            }

            if (userNotificationData.devices != null)
            {
                // get user devices
                List<string> udids = userNotificationData.devices.Select(x => x.Udid).ToList();
                bool docExists;
                foreach (var udid in udids)
                {
                    docExists = false;
                    DeviceNotificationData deviceData = NotificationDal.GetDeviceNotificationData(groupId, udid, ref docExists);
                    if (deviceData == null)
                    {
                        log.DebugFormat("device data wasn't found. GID: {0}, UDID: {1}", groupId, pushData.Udid);
                        continue;
                    }

                    pushData = PushAnnouncementsHelper.GetPushData(groupId, udid, string.Empty);
                    if (pushData == null)
                    {
                        log.ErrorFormat("push data wasn't found. GID: {0}, UDID: {1}", groupId, udid);
                        continue;
                    }

                    result &= LoginPushNotification(groupId, userId, false, pushData, userNotificationData, deviceData);
                }
            }

            return result;
        }
    }
}
