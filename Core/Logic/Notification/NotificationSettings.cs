using ApiObjects.Notification;
using ApiObjects.Response;
using DAL;
using KLogMonitor;
using System;
using System.Linq;
using System.Reflection;
using TVinciShared;
using System.Collections.Generic;
using Core.Catalog.CatalogManagement;
using ApiObjects;
using Core.Catalog;

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
        private const string MAIL_NOTIFICATION_ADAPTER_NOT_EXIST = "Mail notification adapter not exist";

        public static Status UpdateNotificationPartnerSettings(int groupId, NotificationPartnerSettings settings)
        {
            Status response;

            try
            {
                response = CheckNotificationPartnerSettings(groupId, settings);

                if (response.IsOkStatusCode())
                {                  
                    var success = NotificationDal.UpdateNotificationPartnerSettings(groupId, settings);
                    if (success)
                    {
                        response = new Status(eResponseStatus.OK, "notification partner settings set changes");
                        NotificationCache.Instance().RemovePartnerNotificationSettingsFromCache(groupId);
                    }
                    else
                    {
                        response = new Status(eResponseStatus.Error, "notification partner settings failed set changes");
                    }
                }
            }
            catch
            {
                response = Status.Error;
            }
            return response;
        }

        private static Status CheckNotificationPartnerSettings(int groupID, NotificationPartnerSettings settings)
        {
            try
            {
                if (settings == null) return new Status(eResponseStatus.NoNotificationSettingsSent, "no notification setting sent");

                if ((!settings.IsPushNotificationEnabled.HasValue || (!settings.IsPushNotificationEnabled.Value))
                    && settings.IsPushSystemAnnouncementsEnabled.HasValue && settings.IsPushSystemAnnouncementsEnabled.Value)
                    return new Status(eResponseStatus.PushNotificationFalse, "push notification must be true with push system announcements true");

                if (settings.MessageTTLDays.HasValue && settings.MessageTTLDays.Value > MAX_MESSAGE_TTL_DAYS)
                    return new Status(eResponseStatus.InvalidMessageTTL, INVALID_MESSAGE_TTL);

                if (settings.RemindersPrePaddingSec.HasValue &&
                    (settings.RemindersPrePaddingSec.Value > MAX_REMINDERS_PREPADDING_SEC || settings.RemindersPrePaddingSec.Value < MIN_REMINDERS_PREPADDING_SEC))
                    return new Status(eResponseStatus.InvalidReminderPrePaddingSec, INVALID_REMINDERS_PREPADDING_SEC);

                // make sure adapter exist
                if (settings.MailNotificationAdapterId.HasValue && settings.MailNotificationAdapterId > 0
                    && NotificationDal.GetMailNotificationAdapter(groupID, settings.MailNotificationAdapterId.Value) == null)
                    return new Status(eResponseStatus.MailNotificationAdapterNotExist, MAIL_NOTIFICATION_ADAPTER_NOT_EXIST);

                var epgStatus = CheckEpgNotificationSettings(settings, groupID);
                if (!epgStatus.IsOkStatusCode()) return epgStatus;
            }
            catch (Exception ex)
            {
                log.Error("Error checking partner settings", ex);
                return Status.Error;
            }
            return Status.Ok;
        }
        
        private static Status CheckEpgNotificationSettings(NotificationPartnerSettings settings, int groupId)
        {
            if (settings?.EpgNotification == null) return Status.Ok;

            if (!settings.IsIotEnabled.GetValueOrDefault() && settings.EpgNotification.Enabled) 
                return new Status(eResponseStatus.InvalidNotificationSettingsSetup, "iotEnabled should be `true` in order set epgNotification enabled");

            var existingDeviceFamilyIds = Api.Module.GetDeviceFamilyList(groupId).DeviceFamilies.Select(_ => _.Id);
            var unknownDeviceFamilyIds = settings.EpgNotification.DeviceFamilyIds.Except(existingDeviceFamilyIds).ToList();
            if (unknownDeviceFamilyIds.Count != 0) return new Status(eResponseStatus.NonExistingDeviceFamilyIds, "unknown device family:" + string.Join(",", unknownDeviceFamilyIds));

            var liveAssetsIds = settings.EpgNotification.LiveAssetIds;
            var epgChannels = liveAssetsIds.Select(liveAssetId => new KeyValuePair<eAssetTypes, long>(eAssetTypes.MEDIA, liveAssetId)).ToList();
            var existingAssetsIds = AssetManager.GetAssets(groupId, epgChannels, true)?.Where(_ => _ is LiveAsset).Select(_ => _.Id) ?? new List<long>();
            var unknownAssetsIds = liveAssetsIds.Except(existingAssetsIds).ToList();
            if (unknownAssetsIds.Count != 0) return new Status(eResponseStatus.AssetDoesNotExist, "unknown asset:" + string.Join(",", unknownAssetsIds));

            return Status.Ok;
        }

        public static NotificationPartnerSettingsResponse GetPartnerNotificationSettings(int groupID)
        {
            var response = new NotificationPartnerSettingsResponse();

            try
            {
                // get partner notification settings
                List<NotificationPartnerSettings> partnersNotificationSettings = NotificationDal.GetNotificationPartnerSettings(groupID);
                if (partnersNotificationSettings == null || partnersNotificationSettings.Count == 0)
                    response.Status = new Status(eResponseStatus.OK, "no notification partner settings found");
                else
                {
                    response.settings = partnersNotificationSettings[0];
                    response.Status = Status.Ok;
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
                        UserMessageFlow.InitiateNotificationAction(groupId, ApiObjects.eUserMessageAction.EnableUserNotifications, userId, null, null);
                    }

                    if (userSettings.EnablePush == false)
                    {
                        // remove push registration from all user devices
                        UserMessageFlow.InitiateNotificationAction(groupId, ApiObjects.eUserMessageAction.DisableUserNotifications, userId, null, null);
                    }

                    if (userSettings.EnableMail == true)
                    {
                        // add mail registration
                        UserMessageFlow.InitiateNotificationAction(groupId, ApiObjects.eUserMessageAction.EnableUserMailNotifications, userId, null, null);
                    }

                    if (userSettings.EnableMail == false)
                    {
                        // remove mail registration
                        UserMessageFlow.InitiateNotificationAction(groupId, ApiObjects.eUserMessageAction.DisableUserMailNotifications, userId, null, null);
                    }

                    if (userSettings.EnableSms == true)
                    {
                        // add SMS registration
                        UserMessageFlow.InitiateNotificationAction(groupId, ApiObjects.eUserMessageAction.EnableUserSmsNotifications, userId, null, null);
                    }

                    if (userSettings.EnableSms == false)
                    {
                        // remove SMS registration
                        UserMessageFlow.InitiateNotificationAction(groupId, ApiObjects.eUserMessageAction.DisableUserSmsNotifications, userId, null, null);
                    }
                }
            }
            catch (Exception ex)
            {
                response = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.Error($"Failed groupID={groupId}", ex);
            }
            return response;
        }

        private static UserNotification CreateUserNotification(int groupId, int userId, UserNotificationSettings settings)
        {
            UserNotification userNotificationData = new UserNotification(userId) { CreateDateSec = DateUtils.GetUtcUnixTimestampNow() };

            if (settings.EnableInbox.HasValue)
            {
                userNotificationData.Settings.EnableInbox = settings.EnableInbox.Value;
            }

            if (settings.EnableMail.HasValue)
            {
                userNotificationData.Settings.EnableMail = settings.EnableMail.Value;
                if (userNotificationData.Settings.EnableMail.Value)
                {

                    Users.UserResponseObject response = Core.Users.Module.GetUserData(groupId, userId.ToString(), string.Empty);
                    if (response != null && response.m_RespStatus == ApiObjects.ResponseStatus.OK && response.m_user != null)
                    {
                        userNotificationData.UserData.Email = response.m_user.m_oBasicData.m_sEmail;
                        userNotificationData.UserData.FirstName = response.m_user.m_oBasicData.m_sFirstName;
                        userNotificationData.UserData.LastName = response.m_user.m_oBasicData.m_sLastName;
                    }
                    else
                    {
                        log.ErrorFormat("Failed to get user data for userId = {0}", userId);
                    }
                }
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

        public static ApiObjects.IotProfile GetIotAdapter(int groupId)
        {
            var partnerSettingsResponse = NotificationCache.Instance().GetPartnerNotificationSettings(groupId);
            if (partnerSettingsResponse != null &&
                partnerSettingsResponse.settings != null)
            {
                return NotificationDal.GetIotProfile(groupId);
            }
            return null;
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
            if (partnerSettingsResponse != null &&
                partnerSettingsResponse.settings != null &&
                partnerSettingsResponse.settings.PushStartHour.HasValue &&
                partnerSettingsResponse.settings.PushEndHour.HasValue)
            {
                DateTime startTime = DateUtils.UtcUnixTimestampSecondsToDateTime(partnerSettingsResponse.settings.PushStartHour.Value);
                DateTime endTime = DateUtils.UtcUnixTimestampSecondsToDateTime(partnerSettingsResponse.settings.PushEndHour.Value);

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

        public static bool IsPartnerMailNotificationEnabled(int groupId)
        {
            var partnerSettingsResponse = NotificationCache.Instance().GetPartnerNotificationSettings(groupId);
            if (partnerSettingsResponse != null &&
                partnerSettingsResponse.settings != null &&
                partnerSettingsResponse.settings.MailNotificationAdapterId.HasValue &&
                partnerSettingsResponse.settings.MailNotificationAdapterId.Value > 0)
            {
                return true;
            }

            return false;
        }

        public static long GetPartnerMailNotificationAdapterId(int groupId)
        {
            long adapterId = 0;
            var partnerSettingsResponse = NotificationCache.Instance().GetPartnerNotificationSettings(groupId);
            if (partnerSettingsResponse != null &&
                partnerSettingsResponse.settings != null &&
                partnerSettingsResponse.settings.MailNotificationAdapterId.HasValue &&
                partnerSettingsResponse.settings.MailNotificationAdapterId.Value > 0)
            {
                adapterId = partnerSettingsResponse.settings.MailNotificationAdapterId.Value;
            }

            return adapterId;
        }

        public static bool IsPartnerSmsNotificationEnabled(int groupId)
        {
            var partnerSettingsResponse = NotificationCache.Instance().GetPartnerNotificationSettings(groupId);
            return (partnerSettingsResponse != null &&
                partnerSettingsResponse.settings != null &&
                partnerSettingsResponse.settings.IsSMSEnabled.HasValue &&
                partnerSettingsResponse.settings.IsSMSEnabled.Value);
        }

        public static bool IsPartnerIotNotificationEnabled(int groupId)
        {
            var partnerSettingsResponse = NotificationCache.Instance().GetPartnerNotificationSettings(groupId);
            return (partnerSettingsResponse != null &&
                partnerSettingsResponse.settings != null &&
                partnerSettingsResponse.settings.IsIotEnabled.HasValue &&
                partnerSettingsResponse.settings.IsIotEnabled.Value);
        }

        public static bool IsUserSmsEnabled(UserNotificationSettings userSettings)
        {
            return (userSettings != null &&
                userSettings.EnableSms == true);
        }

        public static bool IsNotificationSettingsExistsForPartner(int groupId)
        {            
            NotificationPartnerSettingsResponse partnerSettingsResponse = NotificationCache.Instance().GetPartnerNotificationSettings(groupId);
            return partnerSettingsResponse != null && partnerSettingsResponse.settings != null &&
                    ((partnerSettingsResponse.settings.IsPushNotificationEnabled.HasValue && partnerSettingsResponse.settings.IsPushNotificationEnabled.Value)
                    || (partnerSettingsResponse.settings.IsInboxEnabled.HasValue && partnerSettingsResponse.settings.IsInboxEnabled.Value)
                    || (partnerSettingsResponse.settings.IsPushSystemAnnouncementsEnabled.HasValue && partnerSettingsResponse.settings.IsPushSystemAnnouncementsEnabled.Value)
                    || (partnerSettingsResponse.settings.IsRemindersEnabled.HasValue && partnerSettingsResponse.settings.IsRemindersEnabled.Value)
                    || (partnerSettingsResponse.settings.IsSMSEnabled.HasValue && partnerSettingsResponse.settings.IsSMSEnabled.Value)
                    || (partnerSettingsResponse.settings.IsIotEnabled.HasValue && partnerSettingsResponse.settings.IsIotEnabled.Value));
        }
    }
}
