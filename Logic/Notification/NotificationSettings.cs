using ApiObjects.Notification;
using ApiObjects.Response;
using DAL;
using KLogMonitor;
using System;
using System.Reflection;
using TVinciShared;
using System.Collections.Generic;

namespace Core.Notification
{
    public class NotificationSettings
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private const int MAX_MESSAGE_TTL_DAYS = 90;
        private const int MAX_REMINDERS_PREPADDING_SEC = 3600;
        private const int MIN_REMINDERS_PREPADDING_SEC = 0;
        private const string INVALID_MESSAGE_TTL = "Invalid message ttl";
        private const string INVALID_REMINDERS_PREPADDING_SEC = "Invalid pre-padding value";

        public static ApiObjects.Response.Status UpdateNotificationPartnerSettings(int groupID, ApiObjects.Notification.NotificationPartnerSettings settings)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            bool isSet = false;

            try
            {
                response = CheckNotificationPartnerSettings(groupID, settings);

                if (response.Code == (int)eResponseStatus.OK)
                {
                    isSet = DAL.NotificationDal.UpdateNotificationPartnerSettings(groupID, settings);

                    if (isSet)
                    {
                        response = new ApiObjects.Response.Status((int)eResponseStatus.OK, "notification partner settings set changes");

                        // remove cache
                        NotificationCache.Instance().RemovePartnerNotificationSettingsFromCache(groupID);
                    }
                    else
                    {
                        response = new ApiObjects.Response.Status((int)eResponseStatus.Error, "notification partner settings failed set changes");
                    }
                }
            }
            catch (Exception ex)
            {
                response = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.Error(string.Format("Failed groupID={0}"), ex);
            }
            return response;
        }

        public static ApiObjects.Notification.NotificationPartnerSettingsResponse GetPartnerNotificationSettings(int groupID)
        {
            ApiObjects.Notification.NotificationPartnerSettingsResponse response = new ApiObjects.Notification.NotificationPartnerSettingsResponse();

            try
            {
                // get partner notification settings
                List<NotificationPartnerSettings> partnersNotificationSettings = DAL.NotificationDal.GetNotificationPartnerSettings(groupID);
                if (partnersNotificationSettings == null || partnersNotificationSettings.Count == 0)
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, "no notification partner settings found");
                else
                {
                    response.settings = partnersNotificationSettings[0];
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("GetPartnerNotificationSettings Failed groupID: {0}, ex: {1}", groupID, ex);
            }

            return response;
        }

        public static Status UpdateUserNotificationSettings(int groupId, string userIdentifier, ApiObjects.Notification.UserNotificationSettings settings)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            bool isDocumentExist = true;
            int userId = 0;
            UserNotification userNotificationData = null;
            int.TryParse(userIdentifier, out userId);
            try
            {
                // validated given settings exists
                if (settings == null)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.NoNotificationSettingsSent, "no notification setting sent");
                    return response;
                }

                // update user settings
                UserNotificationSettings userSettings = DAL.NotificationDal.UpdateUserNotificationSettings(groupId, userId, settings, ref isDocumentExist);
                if (!isDocumentExist)
                {
                    // create new user Notification, update settings with setting's input parameters
                    userNotificationData = CreateUserNotification(groupId, userId, settings);
                    if (!NotificationDal.SetUserNotificationData(groupId, userId, userNotificationData))
                    {
                        log.ErrorFormat("Error while trying to set user notification data. GID: {0}, UID: {1}", groupId, userId);
                        response = new ApiObjects.Response.Status((int)eResponseStatus.Error, "notification settings failed set changes");
                        return response;
                    }
                    else
                        userSettings = userNotificationData.Settings;
                }

                if (userSettings == null)
                {
                    log.ErrorFormat("Error while trying to set user notification data. GID: {0}, UID: {1}", groupId, userId);
                    response = new ApiObjects.Response.Status((int)eResponseStatus.Error, "notification settings failed set changes");
                    return response;
                }
                else
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.OK, "notification settings set changes");

                    if (userSettings.EnablePush == true)
                    {
                        // add push registration to all user devices
                        UserMessageFlow.InitiatePushAction(groupId, ApiObjects.eUserMessageAction.EnableUserNotifications, userId, null, null);
                    }

                    if (userSettings.EnablePush == false)
                    {
                        // remove push registration from all user devices
                        UserMessageFlow.InitiatePushAction(groupId, ApiObjects.eUserMessageAction.DisableUserNotifications, userId, null, null);
                    }
                }
            }
            catch (Exception ex)
            {
                response = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.Error(string.Format("Failed groupID={0}"), ex);
            }
            return response;
        }

        private static UserNotification CreateUserNotification(int groupId, int userId, UserNotificationSettings settings)
        {
            UserNotification userNotificationData = new UserNotification(userId) { CreateDateSec = DateUtils.UnixTimeStampNow() };

            if (settings.EnableInbox.HasValue)
            {
                userNotificationData.Settings.EnableInbox = settings.EnableInbox.Value;
            }

            if (settings.EnableMail.HasValue)
            {
                userNotificationData.Settings.EnableMail = settings.EnableMail.Value;
            }

            if (settings.EnablePush.HasValue)
            {
                userNotificationData.Settings.EnablePush = settings.EnablePush.Value;
            }
            else
            {
                //update user settings according to partner settings configuration                          
                userNotificationData.Settings.EnablePush = NotificationSettings.IsPartnerPushEnabled(groupId, userId);
            }

            if (settings.FollowSettings.EnableMail.HasValue)
            {
                userNotificationData.Settings.FollowSettings.EnableMail = settings.FollowSettings.EnableMail.Value;
            }

            if (settings.FollowSettings.EnablePush.HasValue)
            {
                userNotificationData.Settings.FollowSettings.EnablePush = settings.FollowSettings.EnablePush.Value;
            }

            return userNotificationData;
        }

        public static ApiObjects.Notification.NotificationSettingsResponse GetUserNotificationSettings(int groupID, int userID)
        {
            ApiObjects.Notification.NotificationSettingsResponse response = new ApiObjects.Notification.NotificationSettingsResponse();

            try
            {
                // get partner notification settings   
                bool isDocExists = false;
                var userNotificationData = DAL.NotificationDal.GetUserNotificationData(groupID, userID, ref isDocExists);

                // no settings for user - default is enabled
                if (userNotificationData == null || userNotificationData.Settings == null)
                {
                    log.DebugFormat("user announcement data wasn't found. GID: {0}, UID: {1}", groupID, userID);
                    response.settings = new ApiObjects.Notification.UserNotificationSettings();
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, "no notification settings found  return default value");
                }
                else
                {
                    response.settings = userNotificationData.Settings;
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed groupID={0}, userID = {1}", groupID, userID), ex);
            }

            return response;
        }

        public static Status CheckNotificationPartnerSettings(int groupID, NotificationPartnerSettings settings)
        {
            Status response = new Status((int)eResponseStatus.OK, "OK");
            try
            {
                if (settings == null)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.NoNotificationSettingsSent, "no notification setting sent");
                    return response;
                }

                if ((!settings.IsPushNotificationEnabled.HasValue || (!settings.IsPushNotificationEnabled.Value))
                    && settings.IsPushSystemAnnouncementsEnabled.HasValue && settings.IsPushSystemAnnouncementsEnabled.Value)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.PushNotificationFalse, "push notification must be true with push system announcements true");
                    return response;
                }

                if (settings.MessageTTLDays.HasValue && settings.MessageTTLDays.Value > MAX_MESSAGE_TTL_DAYS)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.InvalidMessageTTL, INVALID_MESSAGE_TTL);
                    return response;
                }

                if (settings.RemindersPrePaddingSec.HasValue &&
                    (settings.RemindersPrePaddingSec.Value > MAX_REMINDERS_PREPADDING_SEC || settings.RemindersPrePaddingSec.Value < MIN_REMINDERS_PREPADDING_SEC))
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.InvalidReminderPrePaddingSec, INVALID_REMINDERS_PREPADDING_SEC);
                    return response;
                }
            }
            catch (Exception ex)
            {
                response = new Status((int)eResponseStatus.Error, "Error");
                log.Error("Error checking partner settings", ex);
            }
            return response;
        }

        public static bool IsUserFollowPushEnabled(UserNotificationSettings userSettings)
        {
            if (userSettings != null &&
                userSettings.EnablePush == true &&
                userSettings.FollowSettings != null &&
                userSettings.FollowSettings.EnablePush == true)
            {
                return true;
            }

            return false;
        }

        public static bool IsUserPushEnabled(UserNotificationSettings userSettings)
        {
            if (userSettings != null &&
                userSettings.EnablePush == true)
            {
                return true;
            }

            return false;
        }

        public static bool IsPartnerPushEnabled(int groupId)
        {
            var partnerSettingsResponse = NotificationCache.Instance().GetPartnerNotificationSettings(groupId);
            if (partnerSettingsResponse != null &&
                partnerSettingsResponse.settings != null &&
                partnerSettingsResponse.settings.IsPushNotificationEnabled.HasValue &&
                partnerSettingsResponse.settings.IsPushNotificationEnabled.Value)
            {
                return true;
            }

            return false;
        }

        public static bool IsPartnerPushEnabled(int groupId, int userId)
        {
            bool isPartnerPushEnabled = IsPartnerPushEnabled(groupId);

            if (!isPartnerPushEnabled)
            {
                log.ErrorFormat("partner push is disabled. PID: {0}, UID: {1}", groupId, userId);
            }

            return isPartnerPushEnabled;
        }

        public static bool IsPartnerSystemAnnouncementEnabled(int groupId)
        {
            var partnerSettingsResponse = NotificationCache.Instance().GetPartnerNotificationSettings(groupId);
            if (partnerSettingsResponse != null &&
                partnerSettingsResponse.settings != null &&
                partnerSettingsResponse.settings.IsPushSystemAnnouncementsEnabled.HasValue &&
                partnerSettingsResponse.settings.IsPushSystemAnnouncementsEnabled.Value)
            {
                return true;
            }

            return false;
        }

        public static bool IsPartnerInboxEnabled(int groupId)
        {
            var partnerSettingsResponse = NotificationCache.Instance().GetPartnerNotificationSettings(groupId);
            if (partnerSettingsResponse != null &&
                partnerSettingsResponse.settings != null &&
                partnerSettingsResponse.settings.IsInboxEnabled.HasValue &&
                partnerSettingsResponse.settings.IsInboxEnabled.Value)
            {
                return true;
            }

            return false;
        }

        public static bool IsPartnerRemindersEnabled(int groupId)
        {
            var partnerSettingsResponse = NotificationCache.Instance().GetPartnerNotificationSettings(groupId);
            if (partnerSettingsResponse != null &&
                partnerSettingsResponse.settings != null &&
                partnerSettingsResponse.settings.IsRemindersEnabled.HasValue &&
                partnerSettingsResponse.settings.IsRemindersEnabled.Value)
            {
                return true;
            }

            return false;
        }

        public static int GetInboxMessageTTLDays(int groupId)
        {
            var partnerSettingsResponse = NotificationCache.Instance().GetPartnerNotificationSettings(groupId);
            if (partnerSettingsResponse != null &&
                partnerSettingsResponse.settings != null &&
                partnerSettingsResponse.settings.MessageTTLDays.HasValue)
            {
                return partnerSettingsResponse.settings.MessageTTLDays.Value;
            }
            else
            {
                return MAX_MESSAGE_TTL_DAYS;
            }
        }

        public static bool ShouldIssueAutomaticFollowNotification(int groupId)
        {
            var partnerSettingsResponse = NotificationCache.Instance().GetPartnerNotificationSettings(groupId);
            if (partnerSettingsResponse != null &&
                partnerSettingsResponse.settings != null &&
                partnerSettingsResponse.settings.AutomaticIssueFollowNotifications.HasValue)
            {
                return (bool)partnerSettingsResponse.settings.AutomaticIssueFollowNotifications;
            }

            return false;
        }

        public static string GetPushAdapterUrl(int groupId)
        {
            var partnerSettingsResponse = NotificationCache.Instance().GetPartnerNotificationSettings(groupId);
            if (partnerSettingsResponse != null &&
                partnerSettingsResponse.settings != null)
            {
                return partnerSettingsResponse.settings.PushAdapterUrl;
            }
            return string.Empty;
        }

        public static bool IsWithinPushSendTimeWindow(int groupId, TimeSpan time)
        {
            var partnerSettingsResponse = NotificationCache.Instance().GetPartnerNotificationSettings(groupId);
            if (partnerSettingsResponse == null &&
                partnerSettingsResponse.settings != null &&
                partnerSettingsResponse.settings.PushStartHour.HasValue &&
                partnerSettingsResponse.settings.PushEndHour.HasValue)
            {
                DateTime startTime = DateUtils.UnixTimeStampToDateTime(partnerSettingsResponse.settings.PushStartHour.Value);
                DateTime endTime = DateUtils.UnixTimeStampToDateTime(partnerSettingsResponse.settings.PushEndHour.Value);

                TimeSpan allowedStart = startTime.TimeOfDay;
                TimeSpan allowedEnd = endTime.TimeOfDay;

                if ((time >= allowedStart && time <= allowedEnd) ||
                     allowedStart == allowedEnd)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
