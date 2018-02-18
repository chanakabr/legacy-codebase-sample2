using APILogic.AmazonSnsAdapter;
using APILogic.DmsService;
using ApiObjects;
using ApiObjects.Notification;
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
    public class UserMessageFlow
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private static readonly List<eUserMessageAction> PUSH_ACTIONS = new List<eUserMessageAction>() {
            eUserMessageAction.Login,
            eUserMessageAction.IdentifyPushRegistration,
            eUserMessageAction.Logout,
            eUserMessageAction.AnonymousPushRegistration,
            eUserMessageAction.DeleteUser,
            eUserMessageAction.ChangeUsers,
            eUserMessageAction.EnableUserNotifications,
            eUserMessageAction.DisableUserNotifications,
            eUserMessageAction.DeleteDevice
        };

        private static readonly List<eUserMessageAction> MAIL_ACTIONS = new List<eUserMessageAction>() {
            eUserMessageAction.DeleteUser,
            eUserMessageAction.EnableUserMailNotifications,
            eUserMessageAction.DisableUserNotifications,
            eUserMessageAction.UpdateUser,
            eUserMessageAction.Signup
        };

        private static readonly List<eUserMessageAction> SMS_ACTIONS = new List<eUserMessageAction>() {
            eUserMessageAction.DeleteUser,
            eUserMessageAction.Signup,
            eUserMessageAction.UpdateUser,
            eUserMessageAction.DisableUserSmsNotifications,
            eUserMessageAction.EnableUserSmsNotifications
        };

        public static bool InitiateNotificationAction(int groupId, eUserMessageAction userAction, int userId, string udid, string pushToken)
        {
            bool result = true;
            UserNotification userNotificationData = null;

            if (userId > 0)
            {
                ApiObjects.Response.Status status = Utils.GetUserNotificationData(groupId, userId, out userNotificationData);
                if (status.Code != (int)ApiObjects.Response.eResponseStatus.OK)
                {
                    return false;
                }

                if (userNotificationData != null)
                {
                    // remove old user reminders
                    DeleteOldAnnouncements(groupId, userNotificationData);
                }
                else
                {
                    log.ErrorFormat("Failed to get userNotificationDate, userId = {0}", userId);
                    return false;
                }
            }

            if (PUSH_ACTIONS.Contains(userAction))
            {
                result = InitiatePushAction(groupId, userAction, userId, udid, pushToken, userNotificationData);
            }

            if (MAIL_ACTIONS.Contains(userAction))
            {
                result = result && InitiateMailAction(groupId, userAction, userId, userNotificationData);
            }

            if (SMS_ACTIONS.Contains(userAction))
            {
                result = result && InitiateSmsAction(groupId, userAction, userId, userNotificationData);
            }

            return result;
        }

        public static bool InitiatePushAction(int groupId, eUserMessageAction userAction, int userId, string udid, string pushToken, UserNotification userNotificationData)
        {
            bool result = false;
            bool docExists = false;
            PushData pushData = null;
            DeviceNotificationData deviceData = null;

            if (!NotificationSettings.IsPartnerPushEnabled(groupId))
            {
                log.DebugFormat("partner push is disabled. partner ID: {0}", groupId);
                return true;
            }

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
                    case eUserMessageAction.DeleteDevice:
                        result = HandlePushDeleteDevice(groupId, deviceData, udid);
                        if (result)
                            log.Debug("Successfully performed Delete Device");
                        else
                            log.Error("Error occurred while trying to perform Delete Device");
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
            // remove old reminders
            int numOfRemindersToRemove = 0;
            List<DbReminder> dbReminders = new List<DbReminder>();
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
            List<DbSeriesReminder> dbSeriesReminders = new List<DbSeriesReminder>();
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
            int numOfFollowSeriesToRemove = 0;
            List<DbAnnouncement> dbFollowSeries = null;
            NotificationCache.TryGetAnnouncements(groupId, ref dbFollowSeries);

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

                // get sms notification data
                bool docExists = false;
                SmsNotificationData smsData = DAL.NotificationDal.GetUserSmsNotificationData(groupId, userNotificationData.UserId, ref docExists);
                if (smsData != null)
                {

                    // remove reminders 
                    int numOfSMSRemindersToRemove = 0;
                    if (numOfRemindersToRemove > 0)
                    {
                        if (dbReminders == null || dbReminders.Count == 0)
                        {
                            // nothing came back from DB - remove all reminders
                            numOfSMSRemindersToRemove = smsData.SubscribedReminders.Count;
                            smsData.SubscribedReminders.Clear();
                        }
                        else
                        {
                            // some did not come back from DB - partial removal
                            numOfSMSRemindersToRemove = smsData.SubscribedReminders.RemoveAll(x => !dbReminders.Exists(y => y.ID == x.Id));
                        }
                    }

                    // remove follow series
                    int numOfSMSFollowSeriesToRemove = 0;
                    if (numOfFollowSeriesToRemove > 0)
                    {
                        if (dbFollowSeries == null || dbFollowSeries.Count == 0)
                        {
                            // nothing came back from DB - remove all announcements
                            numOfSMSFollowSeriesToRemove = smsData.SubscribedAnnouncements.Count;
                            smsData.SubscribedAnnouncements.Clear();
                        }
                        else
                        {
                            // some did not come back from DB - partial removal
                            numOfSMSFollowSeriesToRemove = smsData.SubscribedAnnouncements.RemoveAll(x => !dbFollowSeries.Exists(y => y.ID == x.Id));
                        }

                    }

                    if (numOfSMSRemindersToRemove > 0 || numOfSMSFollowSeriesToRemove > 0)
                    {
                        // update device data
                        if (!NotificationDal.SetUserSmsNotificationData(groupId, userNotificationData.UserId, smsData))
                        {
                            log.ErrorFormat("Error deleting old sms notifications. User ID: {0}", userNotificationData.UserId);
                        }
                        else
                        {
                            log.DebugFormat("old sms notifications removed. User ID: {0}. number of reminders removed: {1}, number of follow series removed: {2}",
                                userNotificationData.UserId,
                                numOfSMSRemindersToRemove,
                                numOfSMSFollowSeriesToRemove);
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
                log.ErrorFormat("Error while trying to updated user notification data. Logout flow. data: {0}",
                    userNotificationData != null ? JsonConvert.SerializeObject(userNotificationData) : string.Empty);
            }
            else
            {
                log.DebugFormat("Successfully updated user notification data. Logout flow. data: {0}",
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
                deviceData = new DeviceNotificationData(pushData.Udid)
                {
                    IsLoggedIn = true,
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
                deviceData = new DeviceNotificationData(pushData.Udid)
                {
                    IsLoggedIn = isLogin,
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
                                List<DbAnnouncement> notifications = null;
                                NotificationCache.TryGetAnnouncements(groupId, ref notifications);
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

        private static bool HandlePushDeleteDevice(int groupId, DeviceNotificationData deviceData, string udid)
        {
            bool result = true;
            PushData pushData = null;

            if (deviceData == null)
            {
                log.DebugFormat("device notification data is empty. GroupId: {0}, UDID: {1}", groupId, udid);
                return false;
            }

            pushData = PushAnnouncementsHelper.GetPushData(groupId, udid, string.Empty);
            if (pushData == null)
            {
                log.ErrorFormat("push data wasn't found. GID: {0}, UDID: {1}", groupId, udid);
                return false;
            }

            // Get user data for updating( remove) device from devices list 
            bool isDocexist = false;
            var userNotificationData = NotificationDal.GetUserNotificationData(groupId, deviceData.UserId, ref isDocexist);
            result &= LogoutPushNotification(groupId, deviceData.UserId, false, pushData, userNotificationData, deviceData, false);

            // remove device data
            if (!NotificationDal.RemoveDeviceNotificationData(groupId, udid, deviceData.cas))
            {
                log.ErrorFormat("Error while trying to delete device data. GID: {0}, UDID: {1}", groupId, udid);
                return false;
            }
            else
                log.DebugFormat("Successfully removed device notification data. GID: {0}, UDID: {1}", groupId, udid);

            // remove device data from user Data
            userNotificationData.devices.Remove(userNotificationData.devices.Where(x => x.Udid == udid).First());

            // update user data
            if (!NotificationDal.SetUserNotificationData(groupId, deviceData.UserId, userNotificationData))
            {
                log.ErrorFormat("Error while trying to remove device from user notification data. GroupId: {0}, UserId: {1}, UDID: {2}", groupId, deviceData.UserId, udid);
                return false;
            }
            else
                log.DebugFormat("Successfully removed device from user notification data. GroupId: {0}, UserId: {1}, UDID: {2}", groupId, deviceData.UserId, udid);

            return result;
        }

        public static bool InitiateMailAction(int groupId, eUserMessageAction userAction, int userId, UserNotification userNotificationData)
        {
            bool result = false;

            if (!NotificationSettings.IsPartnerMailNotificationEnabled(groupId))
            {
                log.DebugFormat("partner mail notifications is disabled. partner ID: {0}", groupId);
                return true;
            }

            try
            {
                switch (userAction)
                {
                    case eUserMessageAction.DeleteUser:
                        result = UnSubscribeUserMailNotification(groupId, userId, userNotificationData);
                        if (result)
                            log.Debug("Successfully performed delete User");
                        else
                            log.Error("Error occurred while trying to perform Delete User");
                        break;

                    case eUserMessageAction.EnableUserMailNotifications:
                        result = SubscribeUserMailNotification(groupId, userId, userNotificationData);
                        if (result)
                            log.Debug("Successfully enabled user mail notifications");
                        else
                            log.Error("Error enabling user mail notifications");
                        break;

                    case eUserMessageAction.DisableUserNotifications:
                        result = UnSubscribeUserMailNotification(groupId, userId, userNotificationData);
                        if (result)
                            log.Debug("Successfully disabled user mail notifications");
                        else
                            log.Error("Error disabling user mail notifications");
                        break;
                    case eUserMessageAction.UpdateUser:
                        result = HandleUpdateUserForMailNotification(groupId, userId, userNotificationData);
                        if (result)
                            log.Debug("Successfully updated user data for mail notifications");
                        else
                            log.Error("Error occurred while trying to update user data for mail notifications");
                        break;
                    case eUserMessageAction.Signup:
                        result = HandleUserSignUpForMailNotification(groupId, userId, userNotificationData);
                        if (result)
                            log.Debug("Successfully enabled user mail notifications");
                        else
                            log.Error("Error enabling user mail notifications");
                        break;
                    default:
                        log.ErrorFormat("Unidentified mail notification action requested. action: {0}", userAction);
                        break;
                }
            }
            catch (Exception ex)
            {
                log.Error("Notification Error", ex);
            }

            return result;
        }

        private static bool SubscribeUserMailNotification(int groupId, int userId, UserNotification userNotificationData)
        {
            bool result = true;
            if (userNotificationData.Settings.EnableMail.HasValue && userNotificationData.Settings.EnableMail.Value && !string.IsNullOrEmpty(userNotificationData.UserData.Email))
            {
                List<string> externalIds = MailAnnouncementsHelper.GetAllAnnouncementExternalIdsForUser(groupId, userNotificationData);
                if (externalIds == null || externalIds.Count == 0)
                {
                    log.ErrorFormat("Failed to get user announcements external Ids to subscribe. group: {0}, userId = {1}", groupId, userId);
                    return false;
                }

                if (!MailNotificationAdapterClient.SubscribeToAnnouncement(groupId, externalIds, userNotificationData.UserData, userId))
                {
                    log.ErrorFormat("Failed subscribing user to mail announcement. group: {0}, userId: {1}, email: {2}, externaiIds: {3}",
                        groupId, userId, userNotificationData.UserData.Email, JsonConvert.SerializeObject(externalIds));
                }
            }

            return result;
        }

        private static bool UnSubscribeUserMailNotification(int groupId, int userId, UserNotification userNotificationData)
        {
            bool result = true;
            if (userNotificationData.Settings.EnableMail.HasValue && userNotificationData.Settings.EnableMail.Value && !string.IsNullOrEmpty(userNotificationData.UserData.Email))
            {
                List<string> externalIds = MailAnnouncementsHelper.GetAllAnnouncementExternalIdsForUser(groupId, userNotificationData);
                if (externalIds == null || externalIds.Count == 0)
                {
                    log.ErrorFormat("Failed to get user announcements external Ids to unsubscribe. group: {0}, userId = {1}", groupId, userId);
                    return false;
                }

                if (!MailNotificationAdapterClient.UnSubscribeToAnnouncement(groupId, externalIds, userNotificationData.UserData, userId))
                {
                    log.ErrorFormat("Failed unsubscribing user to mail announcement. group: {0}, userId: {1}, email: {2}, externaiIds: {3}",
                        groupId, userId, userNotificationData.UserData.Email, JsonConvert.SerializeObject(externalIds));
                }
            }

            return result;
        }

        private static bool HandleUpdateUserForMailNotification(int groupId, int userId, UserNotification userNotificationData)
        {
            bool result = true;

            Users.UserResponseObject response = Core.Users.Module.GetUserData(groupId, userId.ToString(), string.Empty);
            if (response == null || response.m_RespStatus != ApiObjects.ResponseStatus.OK || response.m_user == null)
            {
                log.ErrorFormat("Failed to get user data for userId = {0}", userId);
            }
            else
            {
                if (userNotificationData.UserData.Email != response.m_user.m_oBasicData.m_sEmail || userNotificationData.UserData.FirstName != response.m_user.m_oBasicData.m_sFirstName || userNotificationData.UserData.LastName != response.m_user.m_oBasicData.m_sLastName)
                {
                    UserData newuserData = new UserData()
                    {
                        Email = response.m_user.m_oBasicData.m_sEmail,
                        FirstName = response.m_user.m_oBasicData.m_sLastName,
                        LastName = response.m_user.m_oBasicData.m_sLastName
                    };

                    List<string> externalIds = MailAnnouncementsHelper.GetAllAnnouncementExternalIdsForUser(groupId, userNotificationData);

                    if (!MailNotificationAdapterClient.UpdateUserData(groupId, userId, userNotificationData.UserData, newuserData, externalIds))
                    {
                        log.ErrorFormat("Failed to update User Data user to mail announcement. group: {0}, userId: {1}, email: {2}, externaiIds: {3}",
                            groupId, userId, userNotificationData.UserData.Email, JsonConvert.SerializeObject(externalIds));
                    }

                    userNotificationData.UserData.Email = response.m_user.m_oBasicData.m_sEmail;
                    userNotificationData.UserData.FirstName = response.m_user.m_oBasicData.m_sFirstName;
                    userNotificationData.UserData.LastName = response.m_user.m_oBasicData.m_sLastName;

                    if (!DAL.NotificationDal.SetUserNotificationData(groupId, userId, userNotificationData))
                    {
                        log.ErrorFormat("error setting user notification data on update user. group: {0}, user id: {1}", groupId, userId);
                        result = false;
                    }
                }
            }

            return result;
        }

        private static bool HandleUserSignUpForMailNotification(int groupId, int userId, UserNotification userNotificationData)
        {
            bool result = true;
            if (userNotificationData.Settings.EnableMail.HasValue && userNotificationData.Settings.EnableMail.Value && !string.IsNullOrEmpty(userNotificationData.UserData.Email))
            {
                List<DbAnnouncement> announcements = new List<DbAnnouncement>(); ;
                NotificationCache.TryGetAnnouncements(groupId, ref announcements);
                if (announcements != null)
                {
                    DbAnnouncement mailAnnouncement = announcements.Where(a => a.RecipientsType == eAnnouncementRecipientsType.Mail).FirstOrDefault();
                    if (mailAnnouncement == null)
                    {
                        log.ErrorFormat("Failed to get mail announcement.");
                        return false;
                    }

                    if (!MailNotificationAdapterClient.SubscribeToAnnouncement(groupId, new List<string>() { mailAnnouncement.MailExternalId }, userNotificationData.UserData, userId))
                    {
                        log.ErrorFormat("Failed subscribing user to mail announcement. group: {0}, userId: {1}, email: {2}, externaiId: {3}",
                            groupId, userId, userNotificationData.UserData.Email, mailAnnouncement.MailExternalId);
                    }

                    userNotificationData.Announcements.Add(new Announcement()
                    {
                        AddedDateSec = DateUtils.UnixTimeStampNow(),
                        AnnouncementId = mailAnnouncement.ID,
                        AnnouncementName = mailAnnouncement.Name
                    });

                    if (!DAL.NotificationDal.SetUserNotificationData(groupId, userId, userNotificationData))
                    {
                        log.ErrorFormat("error setting user notification data on update user. group: {0}, user id: {1}", groupId, userId);
                        result = false;
                    }
                }
            }

            return result;
        }

        public static bool InitiateSmsAction(int groupId, eUserMessageAction userAction, int userId, UserNotification userNotificationData)
        {
            bool result = false;

            if (!NotificationSettings.IsPartnerSmsNotificationEnabled(groupId))
            {
                log.DebugFormat("partner mail notifications is disabled. partner ID: {0}", groupId);
                return true;
            }

            try
            {
                switch (userAction)
                {
                    case eUserMessageAction.DeleteUser:
                        result = HandleSmsDeleteUser(groupId, userId, userNotificationData);
                        if (result)
                            log.Debug("Successfully performed delete User");
                        else
                            log.Error("Error occurred while trying to perform Delete User");
                        break;

                    case eUserMessageAction.EnableUserSmsNotifications:
                        result = EnableUserSmsNotification(groupId, userId, userNotificationData);
                        if (result)
                            log.Debug("Successfully enabled user mail notifications");
                        else
                            log.Error("Error enabling user mail notifications");
                        break;

                    case eUserMessageAction.DisableUserNotifications:
                        result = DisableUserSmsNotifications(groupId, userId, userNotificationData);
                        if (result)
                            log.Debug("Successfully disabled user mail notifications");
                        else
                            log.Error("Error disabling user mail notifications");
                        break;
                    case eUserMessageAction.UpdateUser:
                        result = HandleUpdateUserForSmsNotification(groupId, userId, userNotificationData);
                        if (result)
                            log.Debug("Successfully updated user data for mail notifications");
                        else
                            log.Error("Error occurred while trying to update user data for mail notifications");
                        break;
                    case eUserMessageAction.Signup:
                        result = HandleUserSignUpForSmsNotification(groupId, userId, userNotificationData);
                        if (result)
                            log.Debug("Successfully enabled user mail notifications");
                        else
                            log.Error("Error enabling user mail notifications");
                        break;
                    default:
                        log.ErrorFormat("Unidentified mail notification action requested. action: {0}", userAction);
                        break;
                }
            }
            catch (Exception ex)
            {
                log.Error("Notification Error", ex);
            }

            return result;
        }

        private static bool HandleUpdateUserForSmsNotification(int groupId, int userId, UserNotification userNotificationData)
        {
            Users.UserResponseObject response = Core.Users.Module.GetUserData(groupId, userId.ToString(), string.Empty);
            if (response == null || response.m_RespStatus != ApiObjects.ResponseStatus.OK || response.m_user == null)
            {
                log.ErrorFormat("Failed to get user data for userId = {0}", userId);
                return false;
            }
            else
            {
                if (userNotificationData.UserData.PhoneNumber != response.m_user.m_oBasicData.m_sPhone)
                {
                    userNotificationData.UserData.PhoneNumber = response.m_user.m_oBasicData.m_sPhone;

                    if (!NotificationDal.SetUserNotificationData(groupId, userId, userNotificationData))
                    {
                        log.ErrorFormat("Error while trying to update user notification data. GID: {0}, UID: {1}", groupId, userId);
                        return false;
                    }
                    
                    SmsNotificationData userSmsNotificationData = null;
                    ApiObjects.Response.Status status = Utils.GetUserSmsNotificationData(groupId, userId, userNotificationData, out userSmsNotificationData);
                    if (status.Code != (int)ApiObjects.Response.eResponseStatus.OK || userSmsNotificationData == null)
                    {
                        log.DebugFormat("Failed to get user SMS notification data. userId = {0}", userId);
                        return false;
                    }

                    if (!UnsubscribeSmsAnnouncements(groupId, userId, userSmsNotificationData))
                    {
                        log.DebugFormat("Failed to unsubscribe user SMS notifications. userId = {0}", userId);
                        return false;
                    }

                    if (!string.IsNullOrEmpty(userNotificationData.UserData.PhoneNumber))
                    {
                        log.DebugFormat("user does not have phone number for SMS", groupId, userId);
                        return true;
                    }

                    if (!SubscribeSmsAnnouncements(groupId, userId, userNotificationData, userSmsNotificationData))
                    {
                        log.DebugFormat("Failed to subscribe user SMS notifications. userId = {0}", userId);
                        return false;
                    }
                }
            }

            return true;
        }

        private static bool HandleUserSignUpForSmsNotification(int groupId, int userId, UserNotification userNotificationData)
        {
            if (!string.IsNullOrEmpty(userNotificationData.UserData.PhoneNumber))
            {
                SmsNotificationData userSmsNotificationData = null;
                ApiObjects.Response.Status status = Utils.GetUserSmsNotificationData(groupId, userId, userNotificationData, out userSmsNotificationData);
                if (status.Code != (int)ApiObjects.Response.eResponseStatus.OK || userSmsNotificationData == null)
                {
                    log.DebugFormat("Failed to get user SMS notification data. userId = {0}", userId);
                    return false;
                }

                if (!SubscribeSmsAnnouncements(groupId, userId, userNotificationData, userSmsNotificationData))
                {
                    log.DebugFormat("Failed to subscribe user SMS notifications. userId = {0}", userId);
                    return false;
                }
            }

            return true;
        }

        private static bool SubscribeSmsAnnouncements(int groupId, int userId, UserNotification userNotificationData, SmsNotificationData userSmsNotificationData)
        {
            long smsAnnouncementId = 0;
            List<AnnouncementSubscriptionData> subscribeList = SmsAnnouncementsHelper.InitAllAnnouncementToSubscribeForAdapter(groupId, userNotificationData, userSmsNotificationData, out smsAnnouncementId);
            if (subscribeList == null ||
                subscribeList.Count == 0 ||
                smsAnnouncementId == 0)
            {
                log.Error("Error retrieving login announcement to subscribe");
                return false;
            }

            // execute action
            List<AnnouncementSubscriptionData> subscribeListResult = NotificationAdapter.SubscribeToAnnouncement(groupId, subscribeList);
            if (subscribeListResult == null || subscribeListResult.Count == 0 || string.IsNullOrEmpty(subscribeListResult.First().SubscriptionArnResult))
            {
                log.ErrorFormat("error subscribing to Amazon for SMS. group: {0}, userId: {1}", groupId, userId);
                return false;
            }
            else
            {
                log.DebugFormat("Successfully subscribed to Amazon for SMS. group: {0}, userId: {1}", groupId, userId);
            }

            // update sms notification data
            if (!UpdateSmsDataAccordingToAdapterResult(groupId, userId, userSmsNotificationData, subscribeList))
            {
                log.ErrorFormat("Error updating SMS notification data. data: {0}",
                    userSmsNotificationData != null ? JsonConvert.SerializeObject(userSmsNotificationData) : string.Empty);
            }
            else
            {
                log.DebugFormat("Successfully updated SMS notification data. data: {0}",
                    userSmsNotificationData != null ? JsonConvert.SerializeObject(userSmsNotificationData) : string.Empty);
            }

            return true;
        }

        private static bool HandleSmsDeleteUser(int groupId, int userId, UserNotification userNotificationData)
        {
            bool result = false;

            if (userNotificationData == null)
            {
                log.DebugFormat("user notification data is empty {0}", userId);
                return true;
            }

            if (userNotificationData.UserData == null || string.IsNullOrEmpty(userNotificationData.UserData.PhoneNumber))
            {
                log.DebugFormat("user notification sms data is empty {0}", userId);
                return true;
            }

            // get sms notification data
            bool docExists = false;
            SmsNotificationData smsData = DAL.NotificationDal.GetUserSmsNotificationData(groupId, userNotificationData.UserId, ref docExists);
            if (smsData == null)
            {
                log.DebugFormat("user sms notification data is empty {0}", userId);
                return true;
            }

            long smsAnnouncementsId = 0;
            List<UnSubscribe> unsubscibeList = SmsAnnouncementsHelper.InitAllAnnouncementToUnSubscribeForAdapter(smsData, out smsAnnouncementsId);

            if (unsubscibeList.Count > 0)
            {
                unsubscibeList = NotificationAdapter.UnSubscribeToAnnouncement(groupId, unsubscibeList);
                if (unsubscibeList == null || unsubscibeList.Count == 0 || !unsubscibeList.First().Success)
                {
                    log.ErrorFormat("error removing reminders from Amazon subscribed. group: {0}, userId: {1}", groupId, userId);
                }
                else
                {
                    log.DebugFormat("Successfully unsubscribed reminders for device from Amazon. group: {0}, userId: {1}", groupId, userId);
                    result = true;
                }
            }
            else
            {
                log.DebugFormat("No messages were found to unsubscribe. group: {0}, userId: {1}", groupId, userId);
            }

            // remove sms data
            NotificationDal.RemoveSmsNotificationData(groupId, userId, smsData.cas);
            
            return result;
        }

        private static bool DisableUserSmsNotifications(int groupId, int userId, UserNotification userNotificationData)
        {
            bool result = true;

            if (userNotificationData == null)
            {
                log.DebugFormat("user notification data is empty {0}", userId);
                return false;
            }

            if (userNotificationData.UserData == null || string.IsNullOrEmpty(userNotificationData.UserData.PhoneNumber))
            {
                log.DebugFormat("user notification sms data is empty {0}", userId);
                return false;
            }

            // get sms notification data
            SmsNotificationData userSmsNotificationData = null;
            ApiObjects.Response.Status status = Utils.GetUserSmsNotificationData(groupId, userId, userNotificationData, out userSmsNotificationData);
            if (status.Code != (int)ApiObjects.Response.eResponseStatus.OK || userSmsNotificationData == null)
            {
                log.DebugFormat("Failed to get user SMS notification data. userId = {0}", userId);
                return false;
            }

            if (!UnsubscribeSmsAnnouncements(groupId, userId, userSmsNotificationData))
            {
                log.DebugFormat("Failed to unsubscribe user SMS notifications. userId = {0}", userId);
                return false;
            }

            return result;
        }

        private static bool UnsubscribeSmsAnnouncements(int groupId, int userId, SmsNotificationData userSmsNotificationData)
        {
            long smsAnnouncementsId = 0;
            List<UnSubscribe> announcementToUnsubscribe = SmsAnnouncementsHelper.InitAllAnnouncementToUnSubscribeForAdapter(userSmsNotificationData, out smsAnnouncementsId);
            List<UnSubscribe> announcementToUnsubscribeResult = null;

            if (announcementToUnsubscribe.Count > 0)
            {
                announcementToUnsubscribeResult = NotificationAdapter.UnSubscribeToAnnouncement(groupId, announcementToUnsubscribe);
                if (announcementToUnsubscribeResult == null || announcementToUnsubscribeResult.Count == 0 || !announcementToUnsubscribeResult.First().Success)
                {
                    log.ErrorFormat("error removing reminders from Amazon subscribed. group: {0}, userId: {1}", groupId, userId);
                    return false;
                }
                else
                {
                    log.DebugFormat("Successfully unsubscribed reminders for device from Amazon. group: {0}, userId: {1}", groupId, userId);
                }
            }
            else
            {
                log.DebugFormat("No messages were found to unsubscribe. group: {0}, userId: {1}", groupId, userId);
            }

            // update SMS notification data
            if (UpdateSmsDataAccordingToAdapterResult(groupId, userId, userSmsNotificationData, announcementToUnsubscribe, announcementToUnsubscribeResult, smsAnnouncementsId))
            {
                log.ErrorFormat("Error updating SMS notification data. Disable all notifications flow. data: {0}",
                    userSmsNotificationData != null ? JsonConvert.SerializeObject(userSmsNotificationData) : string.Empty);
            }
            else
            {
                log.DebugFormat("Successfully updated SMS notification data. Disable all notifications flow. data: {0}",
                    userSmsNotificationData != null ? JsonConvert.SerializeObject(userSmsNotificationData) : string.Empty);
            }

            return true;
        }

        private static bool EnableUserSmsNotification(int groupId, int userId, UserNotification userNotificationData)
        {
            if (userNotificationData == null)
            {
                log.DebugFormat("user notification data is empty", groupId, userId);
                return false;
            }

            if (!string.IsNullOrEmpty(userNotificationData.UserData.PhoneNumber))
            {
                log.DebugFormat("user does not have phone number for SMS", groupId, userId);
                return false;
            }

            SmsNotificationData userSmsNotificationData = null;
            ApiObjects.Response.Status status = Utils.GetUserSmsNotificationData(groupId, userId, userNotificationData, out userSmsNotificationData);
            if (status.Code != (int)ApiObjects.Response.eResponseStatus.OK || userSmsNotificationData == null)
            {
                log.ErrorFormat("Failed to get user SMS notification data. userId = {0}, groupId = {1}", userId, groupId);
                return false;
            }

            if (!SubscribeSmsAnnouncements(groupId, userId, userNotificationData, userSmsNotificationData))
            {
                log.DebugFormat("Failed to subscribe user SMS notifications. userId = {0}", userId);
                return false;
            }

            return true;
        }

        private static bool UpdateSmsDataAccordingToAdapterResult(int groupId, int userId, SmsNotificationData smsNotificationData, List<UnSubscribe> announcementToUnsubscribe, List<UnSubscribe> adapterResult, long smsAnnouncementId)
        {
            // update device document 
            if (smsNotificationData == null)
            {
                smsNotificationData = new SmsNotificationData(userId)
                {
                    UpdatedAt = DateUtils.DateTimeToUnixTimestamp(DateTime.UtcNow),
                    SubscribedAnnouncements = new List<NotificationSubscription>(),
                };
            }
            else
            {
                smsNotificationData.UserId = userId;
                smsNotificationData.UpdatedAt = DateUtils.DateTimeToUnixTimestamp(DateTime.UtcNow);
                if (smsNotificationData.SubscribedAnnouncements == null)
                    smsNotificationData.SubscribedAnnouncements = new List<NotificationSubscription>();
            }

            // remove canceled subscriptions
            if (announcementToUnsubscribe != null)
            {
                if (announcementToUnsubscribe.Count > 0 &&
                    (adapterResult == null ||
                    adapterResult.Count == 0))
                {
                    log.ErrorFormat("Error while trying to unsubscribe user SMS announcements. PID: {0}, UID: {1}", groupId, userId);
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
                        if (cancelSubResult.SubscriptionArn == smsNotificationData.SubscriptionExternalIdentifier)
                        {
                            // remove system announcement
                            smsNotificationData.SubscriptionExternalIdentifier = string.Empty;
                            canceledSystemAnnouncement = true;
                        }

                        // remove subscription from device list
                        if (smsNotificationData.SubscribedAnnouncements.Count > 0)
                        {
                            var removedSub = smsNotificationData.SubscribedAnnouncements.FirstOrDefault(x => x.ExternalId == cancelSubResult.SubscriptionArn);
                            if (removedSub != null)
                                smsNotificationData.SubscribedAnnouncements.Remove(removedSub);
                            else
                            {
                                // remove reminder from device list
                                var removedReminder = smsNotificationData.SubscribedReminders.FirstOrDefault(x => x.ExternalId == cancelSubResult.SubscriptionArn);
                                if (removedReminder != null)
                                    smsNotificationData.SubscribedReminders.Remove(removedReminder);
                            }
                        }
                    }

                    if (!canceledSystemAnnouncement)
                        log.Error("Error canceling system announcement");
                }
            }

            if (!NotificationDal.SetUserSmsNotificationData(groupId, userId, smsNotificationData))
            {
                log.ErrorFormat("Error while trying to update device data. GID: {0}, UID: {1}", groupId, userId);
                return false;
            }
            else
                return true;
        }

        private static bool UpdateSmsDataAccordingToAdapterResult(int groupId, int userId, SmsNotificationData smsNotificationData, List<AnnouncementSubscriptionData> announcementToSubscribe)
        {
            // add new subscriptions
            if (announcementToSubscribe != null)
            {

                // update device announcements
                foreach (var subscription in announcementToSubscribe)
                {
                    if (subscription == null || string.IsNullOrEmpty(subscription.SubscriptionArnResult))
                    {
                        log.ErrorFormat("Error subscribing announcement. announcement: {0}", subscription != null ? JsonConvert.SerializeObject(subscription) : string.Empty);
                        continue;
                    }

                    // add result to follow announcements (if its a follow push announcement)
                    List<DbAnnouncement> notifications = null;
                    NotificationCache.TryGetAnnouncements(groupId, ref notifications);
                    if (notifications != null && notifications.FirstOrDefault(x => x.ID == subscription.ExternalId) != null)
                    {
                        smsNotificationData.SubscribedAnnouncements.Add(new NotificationSubscription()
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
                            smsNotificationData.SubscribedReminders.Add(new NotificationSubscription()
                            {
                                SubscribedAtSec = DateUtils.DateTimeToUnixTimestamp(DateTime.UtcNow),
                                ExternalId = subscription.SubscriptionArnResult,
                                Id = subscription.ExternalId
                            });
                        }
                    }
                }
            }

            if (!NotificationDal.SetUserSmsNotificationData(groupId, userId, smsNotificationData))
            {
                log.ErrorFormat("Error while trying to update SMS data. groupId: {0}, userId: {1}", groupId, userId);
                return false;
            }
            else
                return true;
        }
    }
}