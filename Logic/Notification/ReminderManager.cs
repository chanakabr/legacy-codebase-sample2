using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using APILogic.AmazonSnsAdapter;
using APILogic.DmsService;
using ApiObjects;
using ApiObjects.Notification;
using ApiObjects.QueueObjects;
using ApiObjects.Response;
using ApiObjects.SearchObjects;
using Core.Catalog;
using Core.Catalog.Request;
using Core.Catalog.Response;
using Core.Notification.Adapters;
using DAL;
using KLogMonitor;
using Newtonsoft.Json;
using QueueWrapper.Queues.QueueObjects;
using ScheduledTasks;
using TVinciShared;
using System.Threading.Tasks;
using CachingProvider.LayeredCache;
using APILogic.Notification;

namespace Core.Notification
{
    public class ReminderManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private const string REMINDER_QUEUE_NAME_FORMAT = @"Reminder_{0}_{1}"; // Reminder_GID_ReminderID
        private static string outerPushServerSecret = ODBCWrapper.Utils.GetTcmConfigValue("PushServerKey");
        private static string outerPushServerIV = ODBCWrapper.Utils.GetTcmConfigValue("PushServerIV");
        private static string outerPushDomainName = ODBCWrapper.Utils.GetTcmConfigValue("PushDomainName");


        private static string CatalogSignString = Guid.NewGuid().ToString();
        private static string CatalogSignatureKey = ODBCWrapper.Utils.GetTcmConfigValue("CatalogSignatureKey");

        public const double REMINDER_CLEANUP_INTERVAL_SEC = 21600; // 6 hours 
        private const string ROUTING_KEY_REMINDERS_MESSAGES = "PROCESS_MESSAGE_REMINDERS";

        public static RemindersResponse AddUserReminder(int userId, DbReminder clientReminder, ProgramObj epgProgram = null)
        {
            RemindersResponse response = new RemindersResponse();
            int reminderId = 0;
            DbReminder dbReminder = null;
            Status status;

            try
            {
                // validate reminder is enabled
                if (!NotificationSettings.IsPartnerRemindersEnabled(clientReminder.GroupId))
                {
                    log.ErrorFormat("AddUserReminder - partner reminder is disabled. groupID = {0}", clientReminder.GroupId);
                    response.Status = new Status((int)eResponseStatus.FeatureDisabled, "Feature Disabled");
                    return response;
                }

                // get user reminder from DB                
                dbReminder = NotificationDal.GetReminderByReferenceId(clientReminder.GroupId, clientReminder.Reference);
                if (dbReminder == null)
                {
                    dbReminder = clientReminder;
                }
                else if (userId == 0)
                {
                    response.Reminders = new List<DbReminder>();
                    response.Reminders.Add(dbReminder);
                    return response;
                }

                // get user notifications
                UserNotification userNotificationData = null;
                response.Status = Utils.GetUserNotificationData(dbReminder.GroupId, userId, out userNotificationData);
                if (response.Status.Code != (int)eResponseStatus.OK || userNotificationData == null)
                {
                    return response;
                }

                if (epgProgram == null)
                {
                    // update reminder name and startDate according to epgAssetId
                    status = AnnouncementManager.GetEpgProgram(dbReminder.GroupId, (int)dbReminder.Reference, out epgProgram);
                    if (status.Code != (int)eResponseStatus.OK)
                    {
                        response.Status = status;
                        return response;
                    }
                }

                if (userNotificationData.Reminders.FirstOrDefault(x => x.AnnouncementId == dbReminder.ID) != null ||
                    IsAlreadyFollowedAsSeries(dbReminder.GroupId, epgProgram, userNotificationData.SeriesReminders))
                {
                    // user already set the reminder
                    log.DebugFormat("User is already set a reminder. PID: {0}, UID: {1}, ReminderID: {2}", dbReminder.GroupId, userId, dbReminder.ID);
                    response.Status = new Status((int)eResponseStatus.UserAlreadySetReminder, "User already set a reminder");
                    return response;
                }

                // update reminder with epg program data                
                if (epgProgram != null && epgProgram.AssetType == eAssetTypes.EPG && epgProgram.m_oProgram != null && epgProgram.m_oProgram.EPG_ID > 0)
                {
                    // update date
                    DateTime newEpgSendDate;
                    if (!DateTime.TryParseExact(epgProgram.m_oProgram.START_DATE, AnnouncementManager.EPG_DATETIME_FORMAT, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out newEpgSendDate))
                    {
                        log.ErrorFormat("Failed parsing EPG start date form EPG program setting, epgID: {0}, startDate: {1}", epgProgram.m_oProgram.EPG_ID, epgProgram.m_oProgram.START_DATE);
                        response.Status = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                        return response;
                    }
                    dbReminder.SendTime = DateUtils.DateTimeToUnixTimestamp(newEpgSendDate);

                    // update name
                    if (string.IsNullOrEmpty(epgProgram.m_oProgram.NAME))
                    {
                        log.ErrorFormat("Failed get EPG program name form EPG program setting, epgID: {0}, name: {1}", epgProgram.m_oProgram.EPG_ID, epgProgram.m_oProgram.NAME);
                        response.Status = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                        return response;
                    }
                    dbReminder.Name = epgProgram.m_oProgram.NAME;
                }
                else
                {
                    log.ErrorFormat("Failed get EPG program data form EPG program setting, GID: {0}, assetId: {1}", dbReminder.GroupId, dbReminder.Reference);
                    response.Status = new Status((int)eResponseStatus.ProgramDoesntExist, "program does not exists");
                    return response;
                }

                // validate future asset
                var utcNow = DateUtils.UnixTimeStampNow();
                if (dbReminder.SendTime < utcNow)
                {
                    log.ErrorFormat("Program passed. AssetId: {0}, SendTime: {1}, UtcNow: {2}", dbReminder.Reference, DateUtils.UnixTimeStampToDateTime(dbReminder.SendTime), DateUtils.UnixTimeStampToDateTime(utcNow));
                    response.Status = new Status((int)eResponseStatus.PassedAsset, "Program passed");
                    return response;
                }

                // Need to save in database in 2 cases:
                // 1. A new reminder 
                // 2. An exist reminder that the ExternalPushId is empty, which can happened if the the partner push was disabled in the first time the reminder was inserted 
                bool setReminderDBNeeded = false;
                if (userId > 0)
                {
                    status = TryCreateTopic(dbReminder, out setReminderDBNeeded);
                    if (status.Code != (int)eResponseStatus.OK)
                    {
                        response.Status = status;
                        return response;
                    }
                }

                // update db incase of update (ExternalPushId) or insert (new reminder)
                if (setReminderDBNeeded || dbReminder.ID == 0)
                {
                    log.DebugFormat("Going to set reminder in db for program id: {0}", dbReminder.Reference);

                    // get partner notifications settings
                    var partnerSettings = NotificationSettings.GetPartnerNotificationSettings(dbReminder.GroupId);

                    // update reminder date with pre-pad seconds
                    dbReminder.SendTime -= Convert.ToInt64(partnerSettings.settings.RemindersPrePaddingSec);

                    // insert to DB                    
                    reminderId = DAL.NotificationDal.SetReminder(dbReminder);
                    if (reminderId == 0)
                    {
                        log.ErrorFormat("AddUserReminder failed insert reminder to DB groupID = {0}, Reminder = {1}", dbReminder.GroupId, JsonConvert.SerializeObject(dbReminder));
                        response.Status = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                        return response;
                    }

                    // update reminder id with returned id
                    dbReminder.ID = reminderId;

                    // send rabbit message if needed
                    if (!AddReminderToQueue(dbReminder.GroupId, dbReminder))
                    {
                        log.ErrorFormat("Add reminder: Error while creating reminder message queue. Partner: {0}, Reminder ID: {1}, program ID: {2}, Send date: {3}",
                        dbReminder.GroupId,
                        dbReminder.ID,
                        dbReminder.Reference,
                        dbReminder.SendTime);
                    }
                    else
                        log.DebugFormat("Add reminder: successfully created new message reminder in queue. group ID: {0}, message: {1}", dbReminder.GroupId, JsonConvert.SerializeObject(dbReminder));
                }

                if (userId == 0)
                {
                    response.Reminders = new List<DbReminder>();
                    response.Reminders.Add(dbReminder);
                    return response;
                }

                // update user notification object
                long addedSecs = ODBCWrapper.Utils.DateTimeToUnixTimestampUtc(DateTime.UtcNow);
                userNotificationData.Reminders.Add(new Announcement()
                {
                    AnnouncementId = dbReminder.ID,
                    AnnouncementName = dbReminder.Name,
                    AddedDateSec = addedSecs
                });

                // update CB userNotificationData
                if (!DAL.NotificationDal.SetUserNotificationData(dbReminder.GroupId, userId, userNotificationData))
                {
                    log.ErrorFormat("error setting user reminder notification data. group: {0}, user id: {1}", dbReminder.GroupId, userId);
                    response.Status = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                    return response;
                }

                if (NotificationSettings.IsPartnerMailNotificationEnabled(dbReminder.GroupId) &&
                    userNotificationData.Settings.EnableMail.HasValue && userNotificationData.Settings.EnableMail.Value &&
                    !string.IsNullOrEmpty(userNotificationData.UserData.Email))
                {
                    if (!MailNotificationAdapterClient.SubscribeToAnnouncement(dbReminder.GroupId, new List<string>() { dbReminder.MailExternalId }, userNotificationData.UserData, userId))
                    {
                        log.ErrorFormat("Failed subscribing user reminder to email announcement. group: {0}, userId: {1}, email: {2}", dbReminder.GroupId, userId, userNotificationData.UserData.Email);
                    }
                }

                // update user devices
                if (userNotificationData.devices != null &&
                   userNotificationData.devices.Count > 0 &&
                   NotificationSettings.IsPartnerPushEnabled(dbReminder.GroupId))
                {
                    foreach (UserDevice device in userNotificationData.devices)
                    {
                        log.DebugFormat("adding reminder to device group: {0}, user: {1}, UDID: {2}, reminderId: {3}", dbReminder.GroupId, userId, device.Udid, dbReminder.ID);

                        // get device notification data
                        bool docExists = false;
                        DeviceNotificationData deviceNotificationData = DAL.NotificationDal.GetDeviceNotificationData(dbReminder.GroupId, device.Udid, ref docExists);
                        if (deviceNotificationData == null)
                        {
                            log.ErrorFormat("device notification data not found group: {0}, UDID: {1}, reminderId: {2}", dbReminder.GroupId, device.Udid, dbReminder.ID);
                            continue;
                        }

                        try
                        {
                            // validate device doesn't already have the reminder
                            var subscribedReminder = deviceNotificationData.SubscribedReminders.FirstOrDefault(x => x.Id == dbReminder.ID);
                            if (subscribedReminder != null)
                            {
                                log.ErrorFormat("user already set reminder on device. group: {0}, UDID: {1}", dbReminder.GroupId, device.Udid);
                                continue;
                            }

                            // get push data
                            PushData pushData = PushAnnouncementsHelper.GetPushData(dbReminder.GroupId, device.Udid, string.Empty);
                            if (pushData == null)
                            {
                                log.ErrorFormat("push data not found. group: {0}, UDID: {1}, reminderId: {2}", dbReminder.GroupId, device.Udid, dbReminder.ID);
                                continue;
                            }

                            // subscribe device to reminder
                            AnnouncementSubscriptionData subData = new AnnouncementSubscriptionData()
                            {
                                EndPointArn = pushData.ExternalToken, // take from pushdata (with UDID)
                                Protocol = EnumseDeliveryProtocol.application,
                                TopicArn = dbReminder.ExternalPushId,
                                ExternalId = dbReminder.ID
                            };

                            List<AnnouncementSubscriptionData> subs = new List<AnnouncementSubscriptionData>() { subData };
                            subs = NotificationAdapter.SubscribeToAnnouncement(dbReminder.GroupId, subs);
                            if (subs == null || subs.Count == 0 || string.IsNullOrEmpty(subs.First().SubscriptionArnResult))
                            {
                                log.ErrorFormat("Error registering device to reminder. group: {0}, UDID: {1}, reminderId: {2}", dbReminder.GroupId, device.Udid, dbReminder.ID);
                                continue;
                            }

                            // update device notification object                           
                            deviceNotificationData.SubscribedReminders.Add(new NotificationSubscription()
                            {
                                ExternalId = subs.First().SubscriptionArnResult,
                                Id = dbReminder.ID,
                                SubscribedAtSec = addedSecs
                            });

                            if (!DAL.NotificationDal.SetDeviceNotificationData(dbReminder.GroupId, device.Udid, deviceNotificationData))
                                log.ErrorFormat("error setting device notification data. group: {0}, UDID: {1}, topic: {2}, reminderId: {3}", dbReminder.GroupId, device.Udid, subData.EndPointArn, dbReminder.ID);
                            else
                            {
                                log.DebugFormat("Successfully registered device to reminder. group: {0}, UDID: {1}, topic: {2}, reminderId: {3}", dbReminder.GroupId, device.Udid, subData.EndPointArn, dbReminder.ID);
                            }
                        }
                        catch (Exception ex)
                        {
                            log.ErrorFormat("Error while adding reminder to device. GID: {0}, UDID: {1}, reminderId: {2}, ex: {3}", dbReminder.GroupId, device.Udid, dbReminder.ID, ex);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while adding reminder. GID: {0}, reminderId: {1}, ex: {2}", dbReminder.GroupId, dbReminder.ID, ex);
            }
            response.Reminders = new List<DbReminder>();
            response.Reminders.Add(dbReminder);
            return response;
        }

        public static RemindersResponse AddUserSeriesReminder(int userId, DbSeriesReminder clientReminder)
        {
            RemindersResponse response = new RemindersResponse();

            int reminderId = 0;
            string externalTopicId = string.Empty;
            DbSeriesReminder dbSeriesReminder = null;
            List<Task> tasks = null;
            try
            {
                // validate reminder is enabled
                if (!NotificationSettings.IsPartnerRemindersEnabled(clientReminder.GroupId))
                {
                    log.ErrorFormat("AddUserSeriesReminder - partner reminder is disabled. groupID = {0}", clientReminder.GroupId);
                    response.Status = new Status((int)eResponseStatus.FeatureDisabled, "Feature Disabled");
                    return response;
                }

                // get user reminder from DB                
                dbSeriesReminder = NotificationDal.GetSeriesReminder(clientReminder.GroupId, clientReminder.SeriesId, clientReminder.SeasonNumber, clientReminder.EpgChannelId);
                if (dbSeriesReminder == null)
                {
                    dbSeriesReminder = clientReminder;
                }

                // get user notifications
                UserNotification userNotificationData = null;
                response.Status = Utils.GetUserNotificationData(dbSeriesReminder.GroupId, userId, out userNotificationData);
                if (response.Status.Code != (int)eResponseStatus.OK || userNotificationData == null)
                {
                    return response;
                }

                if (userNotificationData.SeriesReminders == null)
                {
                    userNotificationData.SeriesReminders = new List<Announcement>();
                }

                if (userNotificationData.SeriesReminders.FirstOrDefault(x => x.AnnouncementId == dbSeriesReminder.ID) != null || IsSeriesAlreadyFollowed(dbSeriesReminder.GroupId, dbSeriesReminder.SeriesId, dbSeriesReminder.SeasonNumber, dbSeriesReminder.EpgChannelId, userNotificationData))
                {
                    // user already set the reminder
                    log.DebugFormat("User is already set a series reminder. PID: {0}, UID: {1}, ReminderID: {2}", dbSeriesReminder.GroupId, userId, dbSeriesReminder.ID);
                    response.Status = new Status((int)eResponseStatus.UserAlreadySetReminder, "User already set a reminder");
                    return response;
                }

                dbSeriesReminder.Name = clientReminder.SeasonNumber != 0 ? string.Format("{0}, season {1}", clientReminder.SeriesId, clientReminder.SeasonNumber) : clientReminder.SeriesId.ToString();

                // Need to save in database in 2 cases:
                // 1. A new reminder 
                // 2. An exist reminder that the ExternalPushId is empty, which can happened if the the partner push was disabled in the first time the reminder was inserted 
                bool setReminderDBNeeded;
                Status status = TryCreateTopic(dbSeriesReminder, out setReminderDBNeeded);
                if (status.Code != (int)eResponseStatus.OK)
                {
                    response.Status = status;
                    return response;
                }

                // update db incase of update (ExternalPushId) or insert (new reminder)
                if (setReminderDBNeeded || dbSeriesReminder.ID == 0)
                {
                    log.DebugFormat("Going to set series reminder in db");

                    // insert to DB                    
                    reminderId = DAL.NotificationDal.SetSeriesReminder(dbSeriesReminder);

                    if (reminderId == 0)
                    {
                        log.ErrorFormat("AddUserReminder failed insert reminder to DB groupID = {0}, Reminder = {1}", dbSeriesReminder.GroupId, JsonConvert.SerializeObject(dbSeriesReminder));
                        response.Status = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                        return response;
                    }

                    LayeredCache.Instance.SetInvalidationKey(LayeredCacheKeys.GetSeriesRemindersInvalidationKey(dbSeriesReminder.GroupId, reminderId));
                    dbSeriesReminder.ID = reminderId;

                    tasks = new List<Task>();
                    tasks.Add(Task.Run(() => SetRemindersForSerieEpisodes(dbSeriesReminder.GroupId, dbSeriesReminder.SeriesId, dbSeriesReminder.SeasonNumber, dbSeriesReminder.EpgChannelId)));
                }

                // update user notification object
                long addedSecs = ODBCWrapper.Utils.DateTimeToUnixTimestampUtc(DateTime.UtcNow);
                userNotificationData.SeriesReminders.Add(new Announcement()
                {
                    AnnouncementId = dbSeriesReminder.ID,
                    AnnouncementName = dbSeriesReminder.Name,
                    AddedDateSec = addedSecs
                });

                List<long> remindersToRemove = null;
                List<long> seriesRemindersToRemove = null;
                FilterRedundendReminders(clientReminder.GroupId, clientReminder.SeriesId, clientReminder.SeasonNumber, clientReminder.EpgChannelId, ref userNotificationData, out remindersToRemove, out seriesRemindersToRemove);

                // update CB userNotificationData
                if (!DAL.NotificationDal.SetUserNotificationData(dbSeriesReminder.GroupId, userId, userNotificationData))
                {
                    log.ErrorFormat("error setting user reminder notification data. group: {0}, user id: {1}", dbSeriesReminder.GroupId, userId);
                    response.Status = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                    Utils.WaitForAllTasksToFinish(tasks);
                    return response;
                }

                if (NotificationSettings.IsPartnerMailNotificationEnabled(dbSeriesReminder.GroupId) &&
                    userNotificationData.Settings.EnableMail.HasValue && userNotificationData.Settings.EnableMail.Value &&
                    !string.IsNullOrEmpty(userNotificationData.UserData.Email))
                {
                    if (!MailNotificationAdapterClient.SubscribeToAnnouncement(dbSeriesReminder.GroupId, new List<string>() { dbSeriesReminder.MailExternalId }, userNotificationData.UserData, userId))
                    {
                        log.ErrorFormat("Failed subscribing user series reminder to email announcement. group: {0}, userId: {1}, email: {2}", dbSeriesReminder.GroupId, userId, userNotificationData.UserData.Email);
                    }
                }

                // update user devices
                if (userNotificationData.devices != null &&
                   userNotificationData.devices.Count > 0 &&
                   NotificationSettings.IsPartnerPushEnabled(dbSeriesReminder.GroupId))
                {
                    foreach (UserDevice device in userNotificationData.devices)
                    {
                        log.DebugFormat("adding reminder to device group: {0}, user: {1}, UDID: {2}, reminderId: {3}", dbSeriesReminder.GroupId, userId, device.Udid, dbSeriesReminder.ID);

                        // get device notification data
                        bool docExists = false;
                        DeviceNotificationData deviceNotificationData = DAL.NotificationDal.GetDeviceNotificationData(dbSeriesReminder.GroupId, device.Udid, ref docExists);
                        if (deviceNotificationData == null)
                        {
                            log.ErrorFormat("device notification data not found group: {0}, UDID: {1}, reminderId: {2}", dbSeriesReminder.GroupId, device.Udid, dbSeriesReminder.ID);
                            continue;
                        }

                        try
                        {
                            UnsubscribeDevicesRedundendReminders(clientReminder.GroupId, ref deviceNotificationData, remindersToRemove, seriesRemindersToRemove);

                            // validate device doesn't already have the reminder
                            var subscribedReminder = deviceNotificationData.SubscribedSeriesReminders.FirstOrDefault(x => x.Id == dbSeriesReminder.ID);
                            if (subscribedReminder != null)
                            {
                                log.ErrorFormat("user already set reminder on device. group: {0}, UDID: {1}", dbSeriesReminder.GroupId, device.Udid);
                                continue;
                            }

                            // get push data
                            PushData pushData = PushAnnouncementsHelper.GetPushData(dbSeriesReminder.GroupId, device.Udid, string.Empty);
                            if (pushData == null)
                            {
                                log.ErrorFormat("push data not found. group: {0}, UDID: {1}, reminderId: {2}", dbSeriesReminder.GroupId, device.Udid, dbSeriesReminder.ID);
                                continue;
                            }



                            // subscribe device to reminder
                            AnnouncementSubscriptionData subData = new AnnouncementSubscriptionData()
                            {
                                EndPointArn = pushData.ExternalToken, // take from pushdata (with UDID)
                                Protocol = EnumseDeliveryProtocol.application,
                                TopicArn = dbSeriesReminder.ExternalPushId,
                                ExternalId = dbSeriesReminder.ID
                            };

                            List<AnnouncementSubscriptionData> subs = new List<AnnouncementSubscriptionData>() { subData };
                            subs = NotificationAdapter.SubscribeToAnnouncement(dbSeriesReminder.GroupId, subs);
                            if (subs == null || subs.Count == 0 || string.IsNullOrEmpty(subs.First().SubscriptionArnResult))
                            {
                                log.ErrorFormat("Error registering device to reminder. group: {0}, UDID: {1}, reminderId: {2}", dbSeriesReminder.GroupId, device.Udid, dbSeriesReminder.ID);
                                continue;
                            }

                            // update device notification object                           
                            deviceNotificationData.SubscribedSeriesReminders.Add(new NotificationSubscription()
                            {
                                ExternalId = subs.First().SubscriptionArnResult,
                                Id = dbSeriesReminder.ID,
                                SubscribedAtSec = addedSecs
                            });

                            if (!DAL.NotificationDal.SetDeviceNotificationData(dbSeriesReminder.GroupId, device.Udid, deviceNotificationData))
                                log.ErrorFormat("error setting device notification data. group: {0}, UDID: {1}, topic: {2}, reminderId: {3}", dbSeriesReminder.GroupId, device.Udid, subData.EndPointArn, dbSeriesReminder.ID);
                            else
                            {
                                log.DebugFormat("Successfully registered device to reminder. group: {0}, UDID: {1}, topic: {2}, reminderId: {3}", dbSeriesReminder.GroupId, device.Udid, subData.EndPointArn, dbSeriesReminder.ID);
                            }
                        }
                        catch (Exception ex)
                        {
                            log.ErrorFormat("Error while adding reminder to device. GID: {0}, UDID: {1}, reminderId: {2}, ex: {3}", dbSeriesReminder.GroupId, device.Udid, dbSeriesReminder.ID, ex);
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while adding series reminder. GID: {0}, reminderId: {1}, ex: {2}", dbSeriesReminder.GroupId, dbSeriesReminder.ID, ex);
                response.Status = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            }
            response.Reminders = new List<DbReminder>();
            response.Reminders.Add(dbSeriesReminder);
            Utils.WaitForAllTasksToFinish(tasks);
            return response;
        }

        private static bool IsSeriesAlreadyFollowed(int groupId, string seriesId, long seasonNumber, long epgChannelId, UserNotification userNotificationData)
        {
            List<DbSeriesReminder> dbSeriesReminders = Utils.GetSeriesReminders(groupId, userNotificationData.SeriesReminders.Select(userAnn => userAnn.AnnouncementId).ToList());
            return dbSeriesReminders != null &&
                dbSeriesReminders.Where(sr => sr.SeriesId == seriesId && (sr.SeasonNumber == 0) && sr.EpgChannelId == epgChannelId).FirstOrDefault() != null;
        }

        private static void FilterRedundendReminders(int groupId, string seriesId, long seasonNumber, long epgChannelId, ref UserNotification userNotificationData, out List<long> remindersToRemove, out List<long> seriesRemindersToRemove)
        {
            remindersToRemove = null;
            seriesRemindersToRemove = null;

            Tuple<string, FieldTypes> seriesIdNameType, seasonNumberNameType, episodeNumberNameType;
            if (!Utils.GetSeriesMetaTagsFieldsNamesAndTypes(groupId, out seriesIdNameType, out seasonNumberNameType, out episodeNumberNameType))
            {
                log.ErrorFormat("failed to 'GetSeriesMetaTagsFieldsNamesAndTypes' for groupId = {0} ", groupId);
                return;
            }

            List<long> remindersIdsToRemove = null;
            List<long> seriesReminderIdsToRemove = null;

            if (userNotificationData.Reminders != null && userNotificationData.Reminders.Count > 0)
            {
                List<DbReminder> reminders = Utils.GetReminders(groupId, userNotificationData.Reminders.Select(r => r.AnnouncementId).ToList());
                if (reminders != null && reminders.Count > 0)
                {
                    List<ProgramObj> programs = GetEpgPrograms(groupId, reminders.Select(r => (int)r.Reference).ToList());
                    if (programs != null && programs.Count > 0)
                    {
                        List<ProgramObj> episodesToRemove = new List<ProgramObj>();
                        foreach (ProgramObj program in programs)
                        {
                            if (((seriesIdNameType.Item2 == FieldTypes.Meta && program.m_oProgram.EPG_Meta.Where(pm => pm.Key == seriesIdNameType.Item1).FirstOrDefault().Value == seriesId) ||
                                (seriesIdNameType.Item2 == FieldTypes.Tag && program.m_oProgram.EPG_TAGS.Where(pm => pm.Key == seriesIdNameType.Item1).FirstOrDefault().Value == seriesId)) &&
                                (seasonNumber != 0 && seasonNumberNameType != null ?
                                ((seasonNumberNameType.Item2 == FieldTypes.Meta && program.m_oProgram.EPG_Meta.Where(pm => pm.Key == seasonNumberNameType.Item1).FirstOrDefault().Value == seasonNumber.ToString()) ||
                                (seasonNumberNameType.Item2 == FieldTypes.Tag && program.m_oProgram.EPG_TAGS.Where(pm => pm.Key == seasonNumberNameType.Item1).FirstOrDefault().Value == seasonNumber.ToString()))
                                : true))
                            {
                                episodesToRemove.Add(program);
                            }
                        }

                        if (episodesToRemove != null && episodesToRemove.Count > 0)
                        {
                            var episodesIdsToRemove = episodesToRemove.Select(e => long.Parse(e.AssetId));
                            remindersIdsToRemove = remindersToRemove = reminders.Where(r => episodesIdsToRemove.Contains(r.Reference)).Select(r => (long)r.ID).ToList();
                            userNotificationData.Reminders = userNotificationData.Reminders.Where(r => !remindersIdsToRemove.Contains(r.AnnouncementId)).ToList();
                        }
                    }
                }
            }

            if (seasonNumber == 0 && userNotificationData.SeriesReminders != null && userNotificationData.SeriesReminders.Count > 0)
            {
                List<DbSeriesReminder> dbSeriesReminders = Utils.GetSeriesReminders(groupId, userNotificationData.SeriesReminders.Select(userAnn => userAnn.AnnouncementId).ToList());
                if (dbSeriesReminders != null && dbSeriesReminders.Count > 0)
                {
                    seriesReminderIdsToRemove = seriesRemindersToRemove = dbSeriesReminders.Where(sr => sr.EpgChannelId == epgChannelId && sr.SeriesId == seriesId && sr.SeasonNumber != 0).Select(sr => (long)sr.ID).ToList();
                    userNotificationData.SeriesReminders = userNotificationData.SeriesReminders.Where(sr => !seriesReminderIdsToRemove.Contains(sr.AnnouncementId)).ToList();
                }
            }
        }

        private static void UnsubscribeDevicesRedundendReminders(int groupId, ref DeviceNotificationData deviceData, List<long> remindersToRemove, List<long> seriesRemindersToRemove)
        {
            if (deviceData == null)
                log.Debug("user device data is empty");
            else
            {
                // prepare unsubscribe guest/login announcement object
                List<UnSubscribe> unsubscibeList = new List<UnSubscribe>();

                // prepare reminder subscription to cancel list
                if (deviceData.SubscribedReminders != null)
                {
                    foreach (var reminderSubscription in deviceData.SubscribedReminders)
                    {
                        if (remindersToRemove.Contains(reminderSubscription.Id))
                        {
                            unsubscibeList.Add(new UnSubscribe() { SubscriptionArn = reminderSubscription.ExternalId, ExternalId = reminderSubscription.Id });
                        }
                    }
                    deviceData.SubscribedReminders.Where(r => !remindersToRemove.Contains(r.Id)).ToList();
                }

                // prepare series reminder subscription to cancel list
                if (deviceData.SubscribedSeriesReminders != null)
                {
                    foreach (var reminderSubscription in deviceData.SubscribedSeriesReminders)
                    {
                        if (seriesRemindersToRemove.Contains(reminderSubscription.Id))
                        {
                            unsubscibeList.Add(new UnSubscribe() { SubscriptionArn = reminderSubscription.ExternalId, ExternalId = reminderSubscription.Id });
                        }
                    }
                    deviceData.SubscribedSeriesReminders.Where(r => !seriesRemindersToRemove.Contains(r.Id)).ToList();
                }

                if (unsubscibeList.Count > 0)
                {
                    unsubscibeList = NotificationAdapter.UnSubscribeToAnnouncement(groupId, unsubscibeList);
                    if (unsubscibeList == null || unsubscibeList.Count == 0 || !unsubscibeList.First().Success)
                    {
                        log.ErrorFormat("error removing reminders from Amazon subscribed. group: {0}, UID: {1}, UDID: {2}", groupId, deviceData.UserId, deviceData.Udid);
                    }
                    else
                        log.DebugFormat("Successfully unsubscribed reminders for device from Amazon group: {0}, UID: {1}, UDID: {2}", groupId, deviceData.UserId, deviceData.Udid);
                }
            }

        }

        private static bool IsAlreadyFollowedAsSeries(int groupId, ProgramObj epgProgram, List<Announcement> userSeriesReminders)
        {
            Dictionary<string, string> aliases = Core.ConditionalAccess.Utils.GetEpgFieldTypeEntitys(groupId, epgProgram.m_oProgram);
            if (aliases == null || aliases.Count == 0)
            {
                log.ErrorFormat("failed to alias mappings for groupId = {0}, programId = {1} ", groupId, epgProgram.AssetId);
                return false;
            }

            string seriesId = aliases[Core.ConditionalAccess.Utils.SERIES_ID];
            long seasonNumber = aliases.ContainsKey(Core.ConditionalAccess.Utils.SEASON_NUMBER) ? long.Parse(aliases[Core.ConditionalAccess.Utils.SEASON_NUMBER]) : 0;

            DbSeriesReminder seriesSeasonReminder = NotificationDal.GetSeriesReminder(groupId, seriesId, seasonNumber, int.Parse(epgProgram.m_oProgram.EPG_CHANNEL_ID));
            if (seriesSeasonReminder != null && userSeriesReminders.Where(usr => usr.AnnouncementId == seriesSeasonReminder.ID).FirstOrDefault() != null)
                return true;

            DbSeriesReminder seriesReminder = NotificationDal.GetSeriesReminder(groupId, seriesId, null, int.Parse(epgProgram.m_oProgram.EPG_CHANNEL_ID));
            if (seriesReminder != null && userSeriesReminders.Where(usr => usr.AnnouncementId == seriesReminder.ID).FirstOrDefault() != null)
                return true;

            return false;
        }

        public static void SetRemindersForSerieEpisodes(int groupId, string seriesId, long seasonNumber, long epgChannelId)
        {
            List<UnifiedSearchResult> episodeResults = Utils.SearchSeriesEpisodes(groupId, seriesId, seasonNumber, epgChannelId);
            if (episodeResults != null && episodeResults.Count > 0)
            {
                List<ProgramObj> programs = GetEpgPrograms(groupId, episodeResults.Select(p => Convert.ToInt32(p.AssetId)).ToList());

                if (programs != null && programs.Count > 0)
                {
                    DbReminder episodeDbReminder;
                    foreach (var program in programs)
                    {
                        episodeDbReminder = new DbReminder()
                        {
                            GroupId = groupId,
                            Reference = long.Parse(program.AssetId),
                        };

                        AddUserReminder(0, episodeDbReminder, program);
                    }
                }
                else
                {
                    log.ErrorFormat("failed to get series episodes for setting reminder. seriesId = {0}, seasonNum = {1}, epgChannelId = {2}",
                        seriesId, seasonNumber, epgChannelId);

                }
            }
            else
            {
                log.DebugFormat("failed to get series episodes IDs for setting reminder. seriesId = {0}, seasonNum = {1}, epgChannelId = {2}",
                    seriesId, seasonNumber, epgChannelId);
            }
        }

        private static Status TryCreateTopic(DbReminder dbReminder, out bool setReminderDBNeeded)
        {
            Status response = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            setReminderDBNeeded = false;

            // validate partner push is enabled
            if (!NotificationSettings.IsPartnerPushEnabled(dbReminder.GroupId))
            {
                log.DebugFormat("AddUserReminder - partner push is disabled. groupID = {0}", dbReminder.GroupId);
            }
            else
            {
                // push enabled - check if topic creation is needed
                if (string.IsNullOrEmpty(dbReminder.ExternalPushId))
                {
                    // Create topic
                    string externalTopicId = NotificationAdapter.CreateAnnouncement(dbReminder.GroupId, dbReminder.Name);
                    if (string.IsNullOrEmpty(externalTopicId))
                    {
                        log.DebugFormat("failed to create topic groupID = {0}, reminderName = {1}", dbReminder.GroupId, dbReminder.Name);
                        response = new Status((int)eResponseStatus.FailCreateAnnouncement, "fail create Guest announcement");
                        return response;
                    }

                    dbReminder.ExternalPushId = externalTopicId;
                    setReminderDBNeeded = true;
                }
            }

            if (!NotificationSettings.IsPartnerMailNotificationEnabled(dbReminder.GroupId))
            {
                log.DebugFormat("AddUserReminder - partner mail notifications is disabled. groupID = {0}", dbReminder.GroupId);
            }
            else
            {
                // push enabled - check if topic creation is needed
                if (string.IsNullOrEmpty(dbReminder.MailExternalId))
                {
                    // Create topic
                    string externalId = MailNotificationAdapterClient.CreateAnnouncement(dbReminder.GroupId, dbReminder.Name);
                    if (string.IsNullOrEmpty(externalId))
                    {
                        log.DebugFormat("failed to create mail announcement groupID = {0}, reminderName = {1}", dbReminder.GroupId, dbReminder.Name);
                        response = new Status((int)eResponseStatus.FailCreateAnnouncement, "fail create mail announcement");
                        return response;
                    }

                    dbReminder.MailExternalId = externalId;
                    setReminderDBNeeded = true;
                }
            }

            return response;
        }

        private static List<ProgramObj> GetEpgPrograms(int groupId, List<int> assetIds)
        {
            List<ProgramObj> programs = null;
            EpgProgramResponse epgProgramResponse = null;
            EpgProgramDetailsRequest epgRequest = new EpgProgramDetailsRequest();

            try
            {
                // get EPG information
                epgRequest = new EpgProgramDetailsRequest()
                {
                    m_lProgramsIds = assetIds,
                    m_nGroupID = groupId,
                    m_sSignature = NotificationUtils.GetSignature(CatalogSignString, CatalogSignatureKey),
                    m_sSignString = CatalogSignString
                };

                epgProgramResponse = epgRequest.GetProgramsByIDs(epgRequest);

                if (epgProgramResponse != null && epgProgramResponse.m_lObj != null && epgProgramResponse.m_lObj.Count > 0)
                {
                    programs = epgProgramResponse.m_lObj.Select(p => p as ProgramObj).ToList();
                }
                else
                {
                    log.ErrorFormat("Error when getting EPG information. request: {0}", JsonConvert.SerializeObject(epgRequest));
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error when calling catalog to get EPG information. request: {0}, ex: {1}", JsonConvert.SerializeObject(epgRequest), ex);
            }

            return programs;
        }

        public static Status DeleteUserReminder(int groupId, int userId, long reminderId)
        {
            Status statusResult = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            // get user notification data
            bool docExists = false;
            UserNotification userNotificationData = DAL.NotificationDal.GetUserNotificationData(groupId, userId, ref docExists);

            if (userNotificationData == null ||
                userNotificationData.Reminders == null ||
                userNotificationData.Reminders.Where(x => x.AnnouncementId == reminderId).Count() == 0)
            {
                log.DebugFormat("user notification data wasn't found. GID: {0}, UID: {1}, reminderID: {2}", groupId, userId, reminderId);
                statusResult = new Status((int)eResponseStatus.ReminderNotFound, "reminder not found");
                return statusResult;
            }

            // remove reminder from user notification object
            userNotificationData.Reminders.Remove(userNotificationData.Reminders.Where(x => x.AnnouncementId == reminderId).First());

            // update CB userNotificationData
            if (!DAL.NotificationDal.SetUserNotificationData(groupId, userId, userNotificationData))
                log.ErrorFormat("error while removing user reminder from notification data. group: {0}, userId: {1}, reminderId: {2}", groupId, userId, reminderId);
            else
            {
                log.DebugFormat("Successfully removed user reminder from user notification data. group: {0}, userId: {1}, reminderId: {2}", groupId, userId, reminderId);
                statusResult = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            }

            if (NotificationSettings.IsPartnerMailNotificationEnabled(groupId) &&
                   userNotificationData.Settings.EnableMail.HasValue && userNotificationData.Settings.EnableMail.Value &&
                   !string.IsNullOrEmpty(userNotificationData.UserData.Email))
            {
                List<DbReminder> reminders = NotificationDal.GetReminders(groupId, reminderId);
                if (reminders != null && reminders.Count > 0)
                {
                    if (!MailNotificationAdapterClient.UnSubscribeToAnnouncement(groupId, new List<string>() { reminders[0].MailExternalId }, userNotificationData.UserData, userId))
                    {
                        log.ErrorFormat("Failed subscribing user reminder to email announcement. group: {0}, userId: {1}, email: {2}", groupId, userId, userNotificationData.UserData.Email);
                    }
                }
                else
                {
                    log.ErrorFormat("Reminder not found. group: {0}, reminderId: {1}", groupId, reminderId);
                }
            }

            // iterate through user devices and remove reminders
            if (userNotificationData.devices == null || userNotificationData.devices.Count == 0)
                log.DebugFormat("User doesn't have any devices. no reminders were removed. PID: {0}, UID: {1}, reminderId: {2}", groupId, userId, reminderId);
            else
            {
                foreach (UserDevice device in userNotificationData.devices)
                {
                    // get device data
                    docExists = false;
                    DeviceNotificationData deviceNotificationData = DAL.NotificationDal.GetDeviceNotificationData(groupId, device.Udid, ref docExists);
                    if (deviceNotificationData == null)
                    {
                        log.ErrorFormat("device notification data not found group: {0}, UID: {1}, UDID: {2}, reminderId: {3}", groupId, userId, device.Udid, reminderId);
                        continue;
                    }

                    // get device subscription
                    var subscribedReminder = deviceNotificationData.SubscribedReminders.FirstOrDefault(x => x.Id == reminderId);
                    if (subscribedReminder == null)
                    {
                        log.ErrorFormat("device notification data had no subscription to reminder. group: {0}, UID: {1}, UDID: {2}, reminderId: {3}", groupId, userId, device.Udid, reminderId);
                        continue;
                    }

                    // unsubscribe device 
                    List<UnSubscribe> unsubscibeList = new List<UnSubscribe>()
                    {
                        new UnSubscribe()
                        {
                            SubscriptionArn = subscribedReminder.ExternalId
                        }
                    };

                    // unsubscribe reminder from Amazon
                    unsubscibeList = NotificationAdapter.UnSubscribeToAnnouncement(groupId, unsubscibeList);
                    if (unsubscibeList == null || unsubscibeList.Count == 0 || !unsubscibeList.First().Success)
                    {
                        log.ErrorFormat("error removing reminder from Amazon subscribed. group: {0}, UID: {1}, UDID: {2}, reminderId: {3}", groupId, userId, device.Udid, reminderId);
                    }
                    else
                        log.DebugFormat("Successfully unsubscribed device from Amazon group: {0}, UID: {1}, UDID: {2}, reminderId: {3}", groupId, userId, device.Udid, reminderId);

                    // remove reminder from user device data                    
                    deviceNotificationData.SubscribedReminders.Remove(subscribedReminder);
                    if (!DAL.NotificationDal.SetDeviceNotificationData(groupId, device.Udid, deviceNotificationData))
                    {
                        log.ErrorFormat("error update device subscribed reminder. group: {0}, UID: {1}, UDID: {2}, reminderId: {3}", groupId, userId, device.Udid, reminderId);
                        continue;
                    }
                    else
                        log.DebugFormat("Successfully updated subscribed device reminder. group: {0}, UID: {1}, UDID: {2}, reminderId: {3}", groupId, userId, device.Udid, reminderId);
                }
            }

            return statusResult;
        }

        public static Status DeleteUserSeriesReminder(int groupId, int userId, long reminderId)
        {
            Status statusResult = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            // get user notification data
            bool docExists = false;
            UserNotification userNotificationData = DAL.NotificationDal.GetUserNotificationData(groupId, userId, ref docExists);

            if (userNotificationData == null ||
                userNotificationData.SeriesReminders == null ||
                userNotificationData.SeriesReminders.Where(x => x.AnnouncementId == reminderId).Count() == 0)
            {
                log.DebugFormat("user notification data wasn't found. GID: {0}, UID: {1}, reminderID: {2}", groupId, userId, reminderId);
                statusResult = new Status((int)eResponseStatus.ReminderNotFound, "reminder not found");
                return statusResult;
            }

            // remove reminder from user notification object
            userNotificationData.SeriesReminders.Remove(userNotificationData.SeriesReminders.Where(x => x.AnnouncementId == reminderId).First());

            // update CB userNotificationData
            if (!DAL.NotificationDal.SetUserNotificationData(groupId, userId, userNotificationData))
                log.ErrorFormat("error while removing user series reminder from notification data. group: {0}, userId: {1}, reminderId: {2}", groupId, userId, reminderId);
            else
            {
                log.DebugFormat("Successfully removed user series reminder from user notification data. group: {0}, userId: {1}, reminderId: {2}", groupId, userId, reminderId);
                statusResult = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            }

            if (NotificationSettings.IsPartnerMailNotificationEnabled(groupId) &&
                  userNotificationData.Settings.EnableMail.HasValue && userNotificationData.Settings.EnableMail.Value &&
                  !string.IsNullOrEmpty(userNotificationData.UserData.Email))
            {
                List<DbSeriesReminder> reminders = NotificationDal.GetSeriesReminders(groupId, new List<long> () { reminderId });
                if (reminders != null && reminders.Count > 0)
                {
                    if (!MailNotificationAdapterClient.UnSubscribeToAnnouncement(groupId, new List<string>() { reminders[0].MailExternalId }, userNotificationData.UserData, userId))
                    {
                        log.ErrorFormat("Failed subscribing user reminder to email announcement. group: {0}, userId: {1}, email: {2}", groupId, userId, userNotificationData.UserData.Email);
                    }
                }
                else
                {
                    log.ErrorFormat("Series reminder not found. group: {0}, reminderId: {1}", groupId, reminderId);
                }
            }

            // iterate through user devices and remove reminders
            if (userNotificationData.devices == null || userNotificationData.devices.Count == 0)
                log.DebugFormat("User doesn't have any devices. no reminders were removed. PID: {0}, UID: {1}, reminderId: {2}", groupId, userId, reminderId);
            else
            {
                foreach (UserDevice device in userNotificationData.devices)
                {
                    // get device data
                    docExists = false;
                    DeviceNotificationData deviceNotificationData = DAL.NotificationDal.GetDeviceNotificationData(groupId, device.Udid, ref docExists);
                    if (deviceNotificationData == null)
                    {
                        log.ErrorFormat("device notification data not found group: {0}, UID: {1}, UDID: {2}, reminderId: {3}", groupId, userId, device.Udid, reminderId);
                        continue;
                    }

                    // get device subscription
                    var subscribedReminder = deviceNotificationData.SubscribedSeriesReminders.FirstOrDefault(x => x.Id == reminderId);
                    if (subscribedReminder == null)
                    {
                        log.ErrorFormat("device notification data had no subscription to series reminder. group: {0}, UID: {1}, UDID: {2}, reminderId: {3}", groupId, userId, device.Udid, reminderId);
                        continue;
                    }

                    // unsubscribe device 
                    List<UnSubscribe> unsubscibeList = new List<UnSubscribe>()
                    {
                        new UnSubscribe()
                        {
                            SubscriptionArn = subscribedReminder.ExternalId
                        }
                    };

                    // unsubscribe reminder from Amazon
                    unsubscibeList = NotificationAdapter.UnSubscribeToAnnouncement(groupId, unsubscibeList);
                    if (unsubscibeList == null || unsubscibeList.Count == 0 || !unsubscibeList.First().Success)
                    {
                        log.ErrorFormat("error removing reminder from Amazon subscribed. group: {0}, UID: {1}, UDID: {2}, reminderId: {3}", groupId, userId, device.Udid, reminderId);
                    }
                    else
                        log.DebugFormat("Successfully unsubscribed device from Amazon group: {0}, UID: {1}, UDID: {2}, reminderId: {3}", groupId, userId, device.Udid, reminderId);

                    // remove reminder from user device data                    
                    deviceNotificationData.SubscribedReminders.Remove(subscribedReminder);
                    if (!DAL.NotificationDal.SetDeviceNotificationData(groupId, device.Udid, deviceNotificationData))
                    {
                        log.ErrorFormat("error update device subscribed reminder. group: {0}, UID: {1}, UDID: {2}, reminderId: {3}", groupId, userId, device.Udid, reminderId);
                        continue;
                    }
                    else
                        log.DebugFormat("Successfully updated subscribed device reminder. group: {0}, UID: {1}, UDID: {2}, reminderId: {3}", groupId, userId, device.Udid, reminderId);
                }
            }

            return statusResult;
        }

        public static RegistryResponse RegisterPushReminderParameters(int groupId, long reminderId, string hash, string ip)
        {
            RegistryResponse response = new RegistryResponse() { Status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString()) };

            // get all reminders
            var reminders = NotificationDal.GetReminders(groupId, reminderId);
            if (reminders == null)
            {
                log.Error("GetPushWebParams: reminders were not found.");
                response.Status = new Status((int)eResponseStatus.ReminderNotFound, "reminders were not found");
                return response;
            }

            // build queue name
            string queueName = string.Format(REMINDER_QUEUE_NAME_FORMAT, groupId, reminderId);

            // get relevant reminder
            var reminder = reminders.Where(x => x.ID == reminderId).FirstOrDefault();
            if (reminder == null)
            {
                log.ErrorFormat("GetPushWebParams: reminder not found. id: {0}.", reminderId);
                response.Status = new Status((int)eResponseStatus.ItemNotFound, eResponseStatus.ItemNotFound.ToString());
            }
            else
            {
                // update web push queue name on DB (if necessary)
                if (string.IsNullOrEmpty(reminder.RouteName))
                {
                    reminder.RouteName = queueName;

                    if (DAL.NotificationDal.SetReminder(reminder) == 0)
                    {
                        log.ErrorFormat("Error while trying to update reminder with web push queue name. GID: {0}, reminder ID: {1}, queue name: {2}",
                            groupId,
                            reminder.ID,
                            queueName);
                    }
                    else
                    {
                        log.DebugFormat("Successfully update reminder with web push queue name. GID: {0}, reminder ID: {1}, queue name: {2}",
                            groupId,
                            reminder.ID,
                            queueName);

                        // remove reminders from cache
                        //NotificationCache.Instance().RemoveRemindersFromCache(groupId);
                    }
                }

                // create key
                string keyToEncrypt = string.Format("{0}:{1}", queueName, hash);
                string encryptedKey = EncryptUtils.EncryptRJ128(outerPushServerSecret, outerPushServerIV, keyToEncrypt);

                // create URL
                Random rand = new Random();
                string tokenPart = string.Format("{0}:{1}:{2}:{3}", outerPushServerSecret, ip, hash, rand.Next());
                string encryptedtokenPart = EncryptUtils.EncryptRJ128(outerPushServerSecret, outerPushServerIV, tokenPart);
                string token = HttpUtility.UrlEncode(Convert.ToBase64String(Encoding.UTF8.GetBytes(groupId + ":" + encryptedtokenPart)));
                string url = string.Format(@"http://{0}/?p={1}&x={2}", outerPushDomainName, groupId, token);

                log.DebugFormat("GetPushWebParams: Create URL and key for queue: {0}. URL: {1}, KEY: {2}", queueName, url, encryptedKey);
                response.Url = url;
                response.Key = encryptedKey;
                response.NotificationId = reminderId;
            }
            return response;
        }

        public static bool SendMessageReminder(int partnerId, long startTime, int reminderId)
        {
            // validate reminder is enabled
            if (!NotificationSettings.IsPartnerRemindersEnabled(partnerId))
            {
                log.DebugFormat("Reminder feature is disabled to partner ID: {0}, reminder ID: {1}", partnerId, reminderId);
                return false;
            }

            // get partner notifications settings
            var partnerSettings = NotificationSettings.GetPartnerNotificationSettings(partnerId);
            if (partnerSettings == null && partnerSettings.settings != null)
            {
                log.ErrorFormat("Could not find partner notification settings. Partner ID: {0}", partnerId);
                return false;
            }

            // get reminders 
            List<DbReminder> reminders = NotificationDal.GetReminders(partnerId, reminderId);
            DbReminder reminder = null;
            if (reminders != null)
                reminder = reminders.FirstOrDefault(x => x.ID == reminderId);
            if (reminder == null)
            {
                log.ErrorFormat("reminder was not found. partner ID: {0}, start time: {1}, reminder ID: {2}", partnerId, startTime, reminderId);
                return false;
            }

            // get EPG program
            ProgramObj program = null;
            var status = AnnouncementManager.GetEpgProgram(partnerId, (int)reminder.Reference, out program);
            if (status.Code != (int)eResponseStatus.OK)
            {
                log.ErrorFormat("program was not found. partner ID: {0}, start time: {1}, reminder ID: {2}, programId = ", partnerId, startTime, reminderId, reminder.Reference);
                return false;
            }

            // get media
            MediaObj mediaChannel = null;
            string seriesName = string.Empty;
            status = AnnouncementManager.GetMedia(partnerId, (int)program.m_oProgram.LINEAR_MEDIA_ID, out mediaChannel, out seriesName);
            if (status.Code != (int)eResponseStatus.OK)
            {
                log.ErrorFormat("linear media for channel was not found. partner ID: {0}, start time: {1}, reminder ID: {2}, linear media Id= ", partnerId, startTime, reminderId, program.m_oProgram.LINEAR_MEDIA_ID);
                return false;
            }

            // Parse start date
            DateTime dbReminderSendDate;
            if (!DateTime.TryParseExact(program.m_oProgram.START_DATE, AnnouncementManager.EPG_DATETIME_FORMAT, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out dbReminderSendDate))
            {
                log.ErrorFormat("Failed parsing EPG start date for EPG notification event, epgID: {0}, startDate: {1}", program.m_oProgram.EPG_ID, program.m_oProgram.START_DATE);
                return false;
            }

            // get reminder pre-padding
            double prePadding = 0;
            if (partnerSettings.settings.RemindersPrePaddingSec.HasValue)
            {
                prePadding = (double)partnerSettings.settings.RemindersPrePaddingSec;
            }

            // validate program did not passed (10 min threshold)
            DateTime currentDate = DateTime.UtcNow;
            if (currentDate.AddMinutes(5) < dbReminderSendDate.AddSeconds(-prePadding) ||
               (currentDate.AddMinutes(-5) > dbReminderSendDate.AddSeconds(-prePadding)))
            {
                log.ErrorFormat("Program date passed. reminder ID: {0}, current date: {1}, startDate: {2}", reminderId, currentDate, dbReminderSendDate);
                return false;
            }

            // validate send time is same as send time in DB
            if (reminder.SendTime != startTime)
            {
                log.ErrorFormat("Message sending time is not the same as DB reminder send date. program ID: {0}, message send date: {1}, DB send date: {2}",
                    reminderId,
                    DateUtils.UnixTimeStampToDateTime(Convert.ToInt64(startTime)),
                    DateUtils.UnixTimeStampToDateTime(reminder.SendTime));
                return false;
            }

            // send push / mail messages
            if (NotificationSettings.IsPartnerPushEnabled(partnerId) || NotificationSettings.IsPartnerMailNotificationEnabled(partnerId))
            {
                // get message templates
                MessageTemplate reminderTemplate = null;
                MessageTemplate seriesReminderTemplate = null;
                List<MessageTemplate> messageTemplates = NotificationCache.Instance().GetMessageTemplates(partnerId);
                if (messageTemplates != null)
                {
                    reminderTemplate = messageTemplates.FirstOrDefault(x => x.TemplateType == MessageTemplateType.Reminder);
                    if (reminderTemplate != null)
                    {
                        SendSingleMessageReminder(partnerId, reminder, program, mediaChannel, dbReminderSendDate, reminderTemplate);
                        log.DebugFormat("sent single reminder, reminderId = {0}", reminder.ID);
                    }
                    else
                    {
                        log.ErrorFormat("reminder message template was not found. group: {0}", partnerId);
                    }

                    seriesReminderTemplate = messageTemplates.FirstOrDefault(x => x.TemplateType == MessageTemplateType.SeriesReminder);
                    if (seriesReminderTemplate != null)
                    {
                        SendSeriesMessageReminder(partnerId, program, dbReminderSendDate, seriesReminderTemplate, reminderId, mediaChannel);
                        log.DebugFormat("sent series reminder, reminderId = {0}", reminder.ID);
                    }
                    else
                    {
                        log.ErrorFormat("series reminder message template was not found. group: {0}", partnerId);
                    }
                }
                else
                {
                    log.ErrorFormat("message templates were not found for partnerId = {0}", partnerId);
                    return false;
                }
            }

            return true;
        }

        private static void SendSingleMessageReminder(int partnerId, DbReminder reminder, ProgramObj program, MediaObj mediaChannel, DateTime dbReminderSendDate, MessageTemplate reminderTemplate)
        {
            // push - send to Amazon
            if (NotificationSettings.IsPartnerPushEnabled(partnerId))
            {
                if (string.IsNullOrEmpty(reminder.ExternalPushId))
                {
                    log.ErrorFormat("External push ID wasn't found. reminder: {0}", JsonConvert.SerializeObject(reminder));
                }
                else
                {
                    // build message 
                    MessageData messageData = new MessageData()
                    {
                        Category = reminderTemplate.Action,
                        Sound = reminderTemplate.Sound,
                        Url = reminderTemplate.URL.Replace("{" + eReminderPlaceHolders.StartDate + "}", dbReminderSendDate.ToString(reminderTemplate.DateFormat)).
                                                             Replace("{" + eReminderPlaceHolders.ProgramId + "}", program.m_oProgram.EPG_ID.ToString()).
                                                             Replace("{" + eReminderPlaceHolders.ProgramName + "}", program.m_oProgram.NAME).
                                                             Replace("{" + eReminderPlaceHolders.ChannelName + "}", mediaChannel != null && mediaChannel.m_sName != null ? mediaChannel.m_sName : string.Empty),
                        Alert = reminderTemplate.Message.Replace("{" + eReminderPlaceHolders.StartDate + "}", dbReminderSendDate.ToString(reminderTemplate.DateFormat)).
                                                               Replace("{" + eReminderPlaceHolders.ProgramId + "}", program.m_oProgram.EPG_ID.ToString()).
                                                               Replace("{" + eReminderPlaceHolders.ProgramName + "}", program.m_oProgram.NAME).
                                                               Replace("{" + eReminderPlaceHolders.ChannelName + "}", mediaChannel != null && mediaChannel.m_sName != null ? mediaChannel.m_sName : string.Empty)
                    };

                    // update message reminder
                    reminder.Message = JsonConvert.SerializeObject(messageData);

                    string resultMsgId = NotificationAdapter.PublishToAnnouncement(partnerId, reminder.ExternalPushId, string.Empty, messageData);
                    if (string.IsNullOrEmpty(resultMsgId))
                        log.ErrorFormat("failed to publish remind message to push topic. result message id is empty for reminder {0}", reminder.ID);
                    else
                    {
                        log.DebugFormat("Successfully sent push reminder. reminder Id: {0}", reminder.ID);
                        // update external push result
                        reminder.ExternalResult = resultMsgId;
                        reminder.IsSent = true;
                    }

                    // send to push web - rabbit.                
                    PushToWeb(partnerId, reminder, messageData);
                }
            }

            // mail
            if (NotificationSettings.IsPartnerMailNotificationEnabled(partnerId))
            {
                if (string.IsNullOrEmpty(reminder.MailExternalId))
                {
                    log.ErrorFormat("External mail ID wasn't found. reminder: {0}", JsonConvert.SerializeObject(reminder));
                }
                else
                {
                    string imageUrl = Utils.GetProgramImageUrlByRatio(program.m_oProgram.EPG_PICTURES, reminderTemplate.RatioId);
                    string subject = reminderTemplate.MailSubject.Replace("{" + eReminderPlaceHolders.StartDate + "}", dbReminderSendDate.ToString(reminderTemplate.DateFormat)).
                                                             Replace("{" + eReminderPlaceHolders.ProgramId + "}", program.m_oProgram.EPG_ID.ToString()).
                                                             Replace("{" + eReminderPlaceHolders.ProgramName + "}", program.m_oProgram.NAME).
                                                             Replace("{" + eReminderPlaceHolders.ChannelName + "}", mediaChannel != null && mediaChannel.m_sName != null ? mediaChannel.m_sName : string.Empty);

                    List<KeyValuePair<string, string>> mergeVars = new List<KeyValuePair<string, string>>() {
                        new KeyValuePair<string, string>(eReminderPlaceHolders.StartDate.ToString(), dbReminderSendDate.ToString(reminderTemplate.DateFormat)),
                        new KeyValuePair<string, string>(eReminderPlaceHolders.ProgramId.ToString(), program.m_oProgram.EPG_ID.ToString()),
                        new KeyValuePair<string, string>(eReminderPlaceHolders.ProgramName.ToString(), program.m_oProgram.NAME),
                        new KeyValuePair<string, string>(eReminderPlaceHolders.ChannelName.ToString(), mediaChannel != null && mediaChannel.m_sName != null ? mediaChannel.m_sName : string.Empty),
                        new KeyValuePair<string, string>(eReminderPlaceHolders.Image.ToString(), imageUrl)
                    };

                    if (!MailNotificationAdapterClient.PublishToAnnouncement(partnerId, reminder.MailExternalId, subject, mergeVars, reminderTemplate.MailTemplate))
                        log.ErrorFormat("failed to send remind message to mail adapter. result message id is empty for reminder {0}", reminder.ID);
                    else
                    {
                        reminder.IsSent = true;
                        log.DebugFormat("Successfully sent reminder to mail. reminder Id: {0}", reminder.ID);
                        // IRA: what about updating the reminders table with is sent and external result?????
                    }
                }
            }

            // update reminder 
            if (DAL.NotificationDal.SetReminder(reminder) == 0)
            {
                log.ErrorFormat("Failed to update reminder. partner ID: {0}, reminder ID: {1} ", partnerId, reminder.ID);
            }
        }

        private static void SendSeriesMessageReminder(int partnerId, ProgramObj program, DateTime dbReminderSendDate, MessageTemplate seriesReminderTemplate, long reminderId, MediaObj mediaChannel)
        {
            log.DebugFormat("SendSeriesMessageReminder started");

            if (seriesReminderTemplate == null)
            {
                log.ErrorFormat("series reminder message template was not found. group: {0}", partnerId);
            }

            Dictionary<string, string> aliases = Core.ConditionalAccess.Utils.GetEpgFieldTypeEntitys(partnerId, program.m_oProgram);
            if (aliases == null || aliases.Count == 0)
            {
                log.ErrorFormat("failed to alias mappings for groupId = {0}, programId = {1} ", partnerId, program.AssetId);
                return;
            }
            else
            {
                string seriesId = aliases[Core.ConditionalAccess.Utils.SERIES_ID];
                long seasonNumber = aliases.ContainsKey(Core.ConditionalAccess.Utils.SEASON_NUMBER) ? long.Parse(aliases[Core.ConditionalAccess.Utils.SEASON_NUMBER]) : 0;

                List<DbSeriesReminder> seriesReminders = NotificationDal.GetSeriesReminderBySeries(partnerId, seriesId, null, program.m_oProgram.EPG_CHANNEL_ID);

                seriesReminders = seriesReminders != null && seriesReminders.Count > 0 ? seriesReminders.Where(sr => sr.SeasonNumber == seasonNumber || sr.SeasonNumber == 0).ToList() : null;

                if (seriesReminders == null || seriesReminders.Count == 0)
                {
                    log.ErrorFormat("failed to get series reminders for programId = {0}, seriesId = {1}, seasonNumber = {2}, epgChannelId = {3}",
                        program.AssetId, seriesId, seasonNumber, program.m_oProgram.EPG_CHANNEL_ID);
                }
                else
                {
                    log.DebugFormat("found series reminders for the program");

                    // PUSH
                    if (NotificationSettings.IsPartnerPushEnabled(partnerId))
                    {
                        MessageData seriesMessageData = new MessageData()
                        {
                            Category = seriesReminderTemplate.Action,
                            Sound = seriesReminderTemplate.Sound,
                            Url = seriesReminderTemplate.URL.Replace("{" + eSeriesReminderPlaceHolders.StartDate + "}", dbReminderSendDate.ToString(seriesReminderTemplate.DateFormat)).
                                                                 Replace("{" + eSeriesReminderPlaceHolders.ChannelName + "}", mediaChannel.m_sName != null ? mediaChannel.m_sName : string.Empty).
                                                                 Replace("{" + eSeriesReminderPlaceHolders.SeriesName + "}", seriesId).
                                                                 Replace("{" + eSeriesReminderPlaceHolders.EpisodeName + "}", program.m_oProgram.NAME != null ? program.m_oProgram.NAME : string.Empty),
                            Alert = seriesReminderTemplate.Message.Replace("{" + eSeriesReminderPlaceHolders.StartDate + "}", dbReminderSendDate.ToString(seriesReminderTemplate.DateFormat)).
                                                                 Replace("{" + eSeriesReminderPlaceHolders.ChannelName + "}", mediaChannel.m_sName != null ? mediaChannel.m_sName : string.Empty).
                                                                 Replace("{" + eSeriesReminderPlaceHolders.SeriesName + "}", seriesId).
                                                                 Replace("{" + eSeriesReminderPlaceHolders.EpisodeName + "}", program.m_oProgram.NAME != null ? program.m_oProgram.NAME : string.Empty)
                        };

                        string serializedMessage = JsonConvert.SerializeObject(seriesMessageData);

                        foreach (DbSeriesReminder seriesReminder in seriesReminders)
                        {
                            if (seriesReminder != null && !string.IsNullOrEmpty(seriesReminder.ExternalPushId))
                            {
                                // update message reminder
                                seriesReminder.Message = serializedMessage;

                                // send to Amazon
                                string resultMsgId = NotificationAdapter.PublishToAnnouncement(partnerId, seriesReminder.ExternalPushId, string.Empty, seriesMessageData);
                                if (string.IsNullOrEmpty(resultMsgId))
                                    log.ErrorFormat("failed to publish remind message to push topic. result message id is empty for series reminder {0}", seriesReminder.ID);
                                else
                                {
                                    // update last send date
                                    seriesReminder.LastSendDate = DateTime.UtcNow;
                                    if (NotificationDal.SetSeriesReminder(seriesReminder) == 0)
                                    {
                                        log.ErrorFormat("Failed to set series reminder send date. seriesReminder.ID = {0}", seriesReminder.ID);
                                    }

                                    // update series reminder external result
                                    if (NotificationDal.AddSeriesReminderExternalResult(partnerId, seriesReminder.ID, reminderId, resultMsgId) == 0)
                                    {
                                        log.ErrorFormat("Failed to update series reminder external result. seriesReminder.ID = {0}, reminderId = {1}, resultMsgId = {2}",
                                            seriesReminder.ID, reminderId, resultMsgId);
                                    }
                                }

                                // send to push web - rabbit.                
                                PushToWeb(partnerId, seriesReminder, seriesMessageData);
                            }
                        }
                    }

                    // MAIL
                    if (NotificationSettings.IsPartnerMailNotificationEnabled(partnerId))
                    {
                        string imageUrl = Utils.GetProgramImageUrlByRatio(program.m_oProgram.EPG_PICTURES, seriesReminderTemplate.RatioId);
                        string subject = seriesReminderTemplate.MailSubject.Replace("{" + eSeriesReminderPlaceHolders.StartDate + "}", dbReminderSendDate.ToString(seriesReminderTemplate.DateFormat)).
                                                                 Replace("{" + eSeriesReminderPlaceHolders.ChannelName + "}", mediaChannel.m_sName != null ? mediaChannel.m_sName : string.Empty).
                                                                 Replace("{" + eSeriesReminderPlaceHolders.SeriesName + "}", seriesId).
                                                                 Replace("{" + eSeriesReminderPlaceHolders.EpisodeName + "}", program.m_oProgram.NAME != null ? program.m_oProgram.NAME : string.Empty);

                        List<KeyValuePair<string, string>> mergeVars = new List<KeyValuePair<string, string>>()
                        {
                            new KeyValuePair<string, string>(eSeriesReminderPlaceHolders.StartDate.ToString(), dbReminderSendDate.ToString(seriesReminderTemplate.DateFormat)),
                            new KeyValuePair<string, string>(eSeriesReminderPlaceHolders.ChannelName.ToString(), mediaChannel.m_sName != null ? mediaChannel.m_sName : string.Empty),
                            new KeyValuePair<string, string>(eSeriesReminderPlaceHolders.SeriesName.ToString(), seriesId),
                            new KeyValuePair<string, string>(eSeriesReminderPlaceHolders.EpisodeName.ToString(), program.m_oProgram.NAME != null ? program.m_oProgram.NAME : string.Empty),
                            new KeyValuePair<string, string>(eSeriesReminderPlaceHolders.Image.ToString(), imageUrl)
                        };

                        foreach (DbSeriesReminder seriesReminder in seriesReminders)
                        {
                            if (seriesReminder != null && !string.IsNullOrEmpty(seriesReminder.MailExternalId))
                            {
                                // send to mail adapter
                                if (!MailNotificationAdapterClient.PublishToAnnouncement(partnerId, seriesReminder.MailExternalId, subject, mergeVars, seriesReminderTemplate.MailTemplate))
                                    log.ErrorFormat("failed to publish remind message to push topic. result message id is empty for series reminder {0}", seriesReminder.ID);
                                else
                                {
                                    // update last send date
                                    seriesReminder.LastSendDate = DateTime.UtcNow;
                                    if (NotificationDal.SetSeriesReminder(seriesReminder) == 0)
                                    {
                                        log.ErrorFormat("Failed to set series reminder send date. seriesReminder.ID = {0}", seriesReminder.ID);
                                    }

                                    // update series reminder external result
                                    if (NotificationDal.AddSeriesReminderExternalResult(partnerId, seriesReminder.ID, reminderId, string.Empty) == 0)
                                    {
                                        log.ErrorFormat("Failed to update series reminder external result. seriesReminder.ID = {0}, reminderId = {1}",
                                            seriesReminder.ID, reminderId);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private static void PushToWeb(int partnerId, DbReminder reminder, MessageData messageData)
        {
            if (reminder.RouteName != null)
            {
                // enqueue message with small expiration date
                MessageAnnouncementFullData data = new MessageAnnouncementFullData(partnerId, messageData.Alert, messageData.Url, messageData.Sound, messageData.Category, reminder.SendTime, messageData.ImageUrl);
                GeneralDynamicQueue q = new GeneralDynamicQueue(reminder.RouteName, QueueWrapper.Enums.ConfigType.PushNotifications);
                if (!q.Enqueue(data, reminder.RouteName, AnnouncementManager.PUSH_MESSAGE_EXPIRATION_MILLI_SEC))
                {
                    log.ErrorFormat("Failed to insert push reminder message to web push queue. reminder ID: {0}, route name: {1}",
                        reminder.ID,
                        reminder.RouteName);
                }
                else
                {
                    log.DebugFormat("Successfully inserted push reminder message to web push queue data: {0}, reminder ID: {1}, route name: {2}",
                     JsonConvert.SerializeObject(data),
                     reminder.ID,
                     reminder.RouteName);
                }
            }
            else
                log.DebugFormat("no queues found for push to web. reminder ID: {0}", reminder.ID);
        }

        public static Status DeleteOldReminders(ref bool createNextRunningIteration, ref double nextIntervalSec)
        {
            createNextRunningIteration = true;
            Status responseStatus = new Status() { Code = (int)eResponseStatus.Error, Message = eResponseStatus.Error.ToString() };
            List<NotificationPartnerSettings> partnerNotificationSettings = new List<NotificationPartnerSettings>();
            nextIntervalSec = REMINDER_CLEANUP_INTERVAL_SEC;
            DateTime currentTime = DateTime.UtcNow;
            log.DebugFormat("Starting DeleteOldReminders iteration");
            BaseScheduledTaskLastRunDetails reminderCleanupTask = new BaseScheduledTaskLastRunDetails(ScheduledTaskType.reminderCleanup);
            ScheduledTaskLastRunDetails lastRunDetails = reminderCleanupTask.GetLastRunDetails();
            reminderCleanupTask = lastRunDetails != null ? (BaseScheduledTaskLastRunDetails)lastRunDetails : null;

            if (reminderCleanupTask != null && reminderCleanupTask.Status.Code == (int)eResponseStatus.OK && reminderCleanupTask.NextRunIntervalInSeconds > 0)
            {

                nextIntervalSec = reminderCleanupTask.NextRunIntervalInSeconds;
                if (reminderCleanupTask.LastRunDate.AddSeconds(nextIntervalSec) > currentTime)
                {
                    log.ErrorFormat("Cannot start reminder cleanup iteration - minimum time haven't passed. current time: {0}, last running date: {1}, minimum date: {2}",
                                     currentTime,
                                     reminderCleanupTask.LastRunDate,
                                     reminderCleanupTask.LastRunDate.AddSeconds(nextIntervalSec));
                    createNextRunningIteration = false;
                    return responseStatus;
                }
            }

            // get partner/s notification settings
            partnerNotificationSettings.AddRange(NotificationDal.GetNotificationPartnerSettings(0));
            if (partnerNotificationSettings == null || partnerNotificationSettings.Count == 0)
            {
                log.Error("Error getting partners notification settings.");
                return responseStatus;
            }

            int totalAnnouncementsDeleted = 0;
            foreach (var partnerSettings in partnerNotificationSettings)
            {
                // get all announcements
                var reminders = NotificationDal.GetReminders(partnerSettings.PartnerId);
                if (reminders == null)
                {
                    log.ErrorFormat("Error getting reminders. GID {0}", partnerSettings.PartnerId);
                    continue;
                }

                foreach (var reminder in reminders)
                {
                    // check if reminder expiration passed (over 24 hours ago)
                    if (reminder.SendTime < TVinciShared.DateUtils.DateTimeToUnixTimestamp(currentTime.AddDays(-1)))
                    {
                        Status deleteAnnouncementResp = DeletePartnerReminder(partnerSettings.PartnerId, reminder);
                        if (deleteAnnouncementResp != null && deleteAnnouncementResp.Code != (int)eResponseStatus.OK)
                        {
                            log.ErrorFormat("Error while trying to delete old reminder topic. GID: {0}, reminder ID: {1}, reminder send date: {2}",
                                partnerSettings.PartnerId,
                                reminder.ID,
                                TVinciShared.DateUtils.UnixTimeStampToDateTime(reminder.SendTime).ToString());
                        }
                        else
                        {
                            totalAnnouncementsDeleted++;
                            log.DebugFormat("successfully deleted old reminder. GID: {0}, Reminder ID: {1}, topic expiration duration in days: {2}",
                                partnerSettings.PartnerId,
                                reminder.ID,
                                TVinciShared.DateUtils.UnixTimeStampToDateTime(reminder.SendTime).ToString());
                        }
                    }
                }
            }

            // update last run details
            reminderCleanupTask = new BaseScheduledTaskLastRunDetails(currentTime, totalAnnouncementsDeleted, nextIntervalSec, ScheduledTaskType.reminderCleanup);
            if (!reminderCleanupTask.SetLastRunDetails())
            {
                log.ErrorFormat("Error while trying to update reminder cleanup last run details, ReminderCleanupResponse: {0}", reminderCleanupTask.ToString());
                return responseStatus;
            }

            log.DebugFormat("Finished DeleteOldReminders iteration");

            responseStatus = new Status() { Code = (int)eResponseStatus.OK, Message = eResponseStatus.OK.ToString() };
            return responseStatus;
        }

        public static Status DeletePartnerReminder(int partnerId, DbReminder reminder)
        {
            Status responseStatus = new Status() { Code = (int)eResponseStatus.Error, Message = eResponseStatus.Error.ToString() };
            string logData = string.Format("Partner ID: {0}, reminderId: {1}", partnerId, reminder.ID);

            if (string.IsNullOrEmpty(reminder.ExternalPushId))
                log.DebugFormat("Reminder was not created in Amazon. partner ID: {0}, reminder ID: {1}", partnerId, reminder.ID);
            else
            {
                // delete Amazon topic
                if (!NotificationAdapter.DeleteAnnouncement(partnerId, reminder.ExternalPushId))
                {
                    log.ErrorFormat("Error while trying to delete reminder topic from external adapter. {0}, external topic ID: {1}",
                        logData,
                        reminder.ExternalPushId != null ? reminder.ExternalPushId : string.Empty);
                }
                else
                {
                    log.DebugFormat("Successfully removed reminder from external adapter. {0},  external topic ID: {1}",
                        logData,
                        reminder.ExternalPushId != null ? reminder.ExternalPushId : string.Empty);
                }
            }

            // delete announcement from DB
            if (!NotificationDal.DeleteReminder(partnerId, reminder.ID))
                log.ErrorFormat("Error while trying to delete DB reminder. {0}", logData);
            else
            {
                responseStatus = new Status() { Code = (int)eResponseStatus.OK, Message = eResponseStatus.OK.ToString() };
                log.DebugFormat("Successfully removed DB reminder. {0}", logData);
            }

            return responseStatus;
        }

        public static bool AddReminderToQueue(int groupId, DbReminder reminder)
        {
            MessageReminderQueue que = new MessageReminderQueue();
            MessageReminderData messageReminderData = new MessageReminderData(groupId, reminder.SendTime, reminder.ID)
            {
                ETA = ODBCWrapper.Utils.UnixTimestampToDateTime(reminder.SendTime)
            };

            bool res = que.Enqueue(messageReminderData, ROUTING_KEY_REMINDERS_MESSAGES);

            if (res)
                log.DebugFormat("Successfully inserted a reminder message to reminder queue: {0}", messageReminderData);
            else
                log.ErrorFormat("Error while inserting reminder {0} to queue", messageReminderData);

            return res;
        }

        public static bool HandleEpgEvent(int partnerId, List<int> programIds)
        {
            // get partner notifications settings
            var partnerSettings = NotificationSettings.GetPartnerNotificationSettings(partnerId);
            if (partnerSettings == null && partnerSettings.settings != null)
            {
                log.ErrorFormat("Could not find partner notification settings. Partner ID: {0}", partnerId);
                return false;
            }

            // get EPG programs
            List<ProgramObj> epgs = GetEpgPrograms(partnerId, programIds);
            if (epgs == null || epgs.Count == 0)
            {
                log.ErrorFormat("Programs were not found. request: {0}", JsonConvert.SerializeObject(programIds));
                return false;
            }

            // validate all programs returned
            if (epgs.Count != programIds.Count)
                log.ErrorFormat("EPG reminder event: Not all EPG returned from catalog: asked: {0}, returned: {1}", string.Join(",", programIds.ToArray()), JsonConvert.SerializeObject(epgs));

            try
            {
                // handle reminders 
                HandleEpgEventForReminders(partnerId, partnerSettings, epgs);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception while sending reminders. Partner ID: {0}, program IDs: {1}, ex: {2}", partnerId, string.Join(",", programIds), ex);
            }

            try
            {
                // handle interests
                TopicInterestManager.HandleEpgEventForInterests(partnerId, partnerSettings, epgs);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception while sending user interests. Partner ID: {0}, program IDs: {1}, ex: {2}", partnerId, string.Join(",", programIds), ex);
            }

            return true;
        }

        public static bool HandleEpgEventForReminders(int partnerId, NotificationPartnerSettingsResponse partnerSettings, List<ProgramObj> programs)
        {
            // validate reminder is enabled
            if (!NotificationSettings.IsPartnerRemindersEnabled(partnerId))
            {
                log.DebugFormat("Reminder feature is disabled to partner: {0}", partnerId);
                return false;
            }

            foreach (ProgramObj program in programs)
            {
                // check EPG validity
                if (program == null || program.AssetType != eAssetTypes.EPG || program.m_oProgram == null || program.m_oProgram.EPG_ID < 1)
                {
                    log.ErrorFormat("Error with received EPG program: {0}", JsonConvert.SerializeObject(program));
                    continue;
                }

                // get dbReminders
                var dbReminders = NotificationDal.GetReminderByReferenceId(partnerId, programs.Select(x => x.m_oProgram.EPG_ID).ToList());

                // Parse start date
                DateTime newEpgSendDate;
                if (!DateTime.TryParseExact(program.m_oProgram.START_DATE, AnnouncementManager.EPG_DATETIME_FORMAT, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out newEpgSendDate))
                {
                    log.ErrorFormat("Failed parsing EPG start date for EPG notification event, epgID: {0}, startDate: {1}", program.m_oProgram.EPG_ID, program.m_oProgram.START_DATE);
                    continue;
                }
                newEpgSendDate = newEpgSendDate.AddSeconds((double)partnerSettings.settings.RemindersPrePaddingSec * -1);

                // check if reminder already exists
                DbReminder dbReminder = null;
                if (dbReminders != null)
                {
                    dbReminder = dbReminders.FirstOrDefault(x => x.Reference == program.m_oProgram.EPG_ID);
                }
                if (dbReminder == null)
                {
                    log.DebugFormat("reminder was not found for ingested EPG: group {0}, program ID: {1}", partnerId, program.m_oProgram.EPG_ID);
                    Dictionary<string, string> epgFieldMappings = ConditionalAccess.Utils.GetEpgFieldTypeEntitys(partnerId, program.m_oProgram);
                    if (epgFieldMappings != null && epgFieldMappings.Count > 0)
                    {
                        long epgChannelId;
                        if (!long.TryParse(program.m_oProgram.EPG_CHANNEL_ID, out epgChannelId))
                        {
                            log.ErrorFormat("failed parsing EPG_CHANNEL_ID, epgId = {0}, epgChannelId: {1}", program.m_oProgram.EPG_ID, program.m_oProgram.EPG_CHANNEL_ID);
                            continue;
                        }

                        string seriesId = epgFieldMappings[ConditionalAccess.Utils.SERIES_ID];
                        int seasonNum = epgFieldMappings.ContainsKey(ConditionalAccess.Utils.SEASON_NUMBER) ? int.Parse(epgFieldMappings[ConditionalAccess.Utils.SEASON_NUMBER]) : 0;
                        if (NotificationDal.IsReminderRequired(partnerId, seriesId, seasonNum, epgChannelId))
                        {
                            dbReminder = new DbReminder() { GroupId = partnerId, Reference = program.m_oProgram.EPG_ID };
                            RemindersResponse reminderResponse = AddUserReminder(0, dbReminder, program);
                            if (reminderResponse == null || reminderResponse.Status == null || reminderResponse.Status.Code != (int)eResponseStatus.OK)
                            {
                                log.ErrorFormat("Failed adding reminder for groupId: {0}, epgId: {1}", partnerId, program.m_oProgram.EPG_ID);
                                if (reminderResponse != null && reminderResponse.Status != null)
                                    log.ErrorFormat("Failed adding reminder. code = {0}, msg = {1}", reminderResponse.Status.Code, reminderResponse.Status.Message);
                                continue;
                            }
                            else
                            {
                                log.DebugFormat("reminder added for groupId: {0}, epgId: {1}", partnerId, program.m_oProgram.EPG_ID);
                            }
                        }
                        else
                        {
                            log.DebugFormat("reminder not required for ingested EPG: group {0}, program ID: {1}, seriesId = {2}, season = {3}",
                                partnerId, program.m_oProgram.EPG_ID, seriesId, seasonNum);
                        }
                    }
                    else
                    {
                        log.DebugFormat("Alias mapping were not found");
                    }
                }
                else
                {
                    // reminder found
                    DateTime oldEpgSendDate = TVinciShared.DateUtils.UnixTimeStampToDateTime(dbReminder.SendTime);

                    // check if should update time
                    bool shouldUpdateReminder = false;
                    if (oldEpgSendDate != newEpgSendDate)
                        shouldUpdateReminder = true;

                    log.DebugFormat("reminder found for ingested EPG: GID: {0}, program ID: {1}, reminder ID: {2}, reminder name: {3}, old send date: {4}, new send date: {5}. should update: {6}",
                        partnerId,                          // {0}
                        program.m_oProgram.EPG_ID,              // {1}
                        dbReminder.ID,                      // {2}
                        dbReminder.Name,                    // {3}
                        oldEpgSendDate,                     // {4}
                        newEpgSendDate,                     // {5}
                        shouldUpdateReminder.ToString());   // {6}

                    if (shouldUpdateReminder)
                    {
                        // update DB
                        dbReminder.SendTime = TVinciShared.DateUtils.DateTimeToUnixTimestamp(newEpgSendDate);
                        if (NotificationDal.SetReminder(dbReminder) == 0)
                        {
                            log.ErrorFormat("Error while trying to update reminder time in table. reminder: {0}", JsonConvert.SerializeObject(dbReminder));
                            continue;
                        }
                        else
                        {
                            log.DebugFormat("reminder date updated. program ID: {0}, reminder ID: {1}, old date: {2}, new date: {3}",
                                            program.m_oProgram.EPG_ID, dbReminder.ID, oldEpgSendDate, newEpgSendDate);
                        }

                        DateTime nowDate = DateTime.UtcNow;
                        if (newEpgSendDate < nowDate)
                        {
                            // new EPG date has passed - not creating a new message
                            log.DebugFormat("Program have passed - not creating a new message. program ID: {0}, reminder ID: {1}, current date: {2}, new date: {3}",
                                            program.m_oProgram.EPG_ID, dbReminder.ID, nowDate, newEpgSendDate);
                            continue;
                        }

                        // create new message
                        log.DebugFormat("Sending a new message reminder to queue. Partner: {0}, Reminder ID: {1}, program ID: {2}, Send date: {3}",
                                            partnerId, dbReminder.ID, program.m_oProgram.EPG_ID, newEpgSendDate);

                        // sending a new rabbit reminder message
                        if (!AddReminderToQueue(partnerId, dbReminder))
                        {
                            log.ErrorFormat("Error while creating reminder message queue. Partner: {0}, Reminder ID: {1}, program ID: {2}, Send date: {3}",
                                            partnerId, dbReminder.ID, program.m_oProgram.EPG_ID, newEpgSendDate);
                        }
                        else
                            log.DebugFormat("successfully created new message reminder in queue. group ID: {0}, message: {1}", partnerId, JsonConvert.SerializeObject(dbReminder));
                    }
                }
            }

            return true;
        }

        public static RemindersResponse GetUserReminders(int groupId, int userId, string filter, int pageSize, int pageIndex, OrderObj orderObj)
        {
            RemindersResponse response = new RemindersResponse();

            // validate reminder is enabled
            if (!NotificationSettings.IsPartnerRemindersEnabled(groupId))
            {
                log.ErrorFormat("GetUserReminders - partner reminder is disabled. groupID = {0}", groupId);
                response.Status = new Status((int)eResponseStatus.FeatureDisabled, "Feature Disabled");
                return response;
            }

            // Get user notification data
            bool isDocExist = false;
            UserNotification userNotificationData = NotificationDal.GetUserNotificationData(groupId, userId, ref isDocExist);
            if (userNotificationData == null ||
                userNotificationData.Reminders == null ||
                userNotificationData.Reminders.Count == 0)
            {
                log.DebugFormat("User doesn't have any reminders. GID: {0}, user ID: {1}", groupId, userId);
                response.Status = new Status() { Code = (int)eResponseStatus.OK, Message = eResponseStatus.OK.ToString() };
                return response;
            }

            // get reminder from DB
            List<DbReminder> dbReminders = NotificationDal.GetReminders(groupId, userNotificationData.Reminders.Select(userAnn => userAnn.AnnouncementId).ToList());
            if (dbReminders == null || dbReminders.Count == 0)
            {
                log.DebugFormat("user reminders were not found on DB. GID: {0}, user ID: {1}", groupId, userId);
                response.Status = new Status() { Code = (int)eResponseStatus.OK, Message = eResponseStatus.OK.ToString() };
                return response;
            }

            string newFilter = string.Empty;
            // validate original KSql filter
            ApiObjects.SearchObjects.BooleanPhraseNode temporaryNode = null;
            var parseStatus = ApiObjects.SearchObjects.BooleanPhraseNode.ParseSearchExpression(filter, ref temporaryNode);

            if (parseStatus == null)
            {
                log.DebugFormat("Failed parsing filter query. GID: {0}, user ID: {1}, filter: {2}", groupId, userId, filter);
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.SyntaxError, "Failed parsing filter query");
                return response;
            }

            List<long> dbProgramsIds = dbReminders.Select(x => x.Reference).ToList();
            if (dbProgramsIds != null && dbProgramsIds.Count > 0)
            {
                string ids = string.Join(",", dbProgramsIds.Select(at => at.ToString()).ToArray());
                newFilter = string.Format("(and epg_id:'{0}' {1})", ids, filter);
            }

            // build unified search request
            UnifiedSearchRequest request = new UnifiedSearchRequest()
            {
                m_sSignature = NotificationUtils.GetSignature(CatalogSignString, CatalogSignatureKey),
                m_sSignString = CatalogSignString,
                filterQuery = newFilter,
                m_nGroupID = groupId,
                m_nPageIndex = pageIndex,
                m_nPageSize = pageSize,
                m_oFilter = new Filter()
                {
                    m_bOnlyActiveMedia = true
                },
                shouldDateSearchesApplyToAllTypes = true,
                order = orderObj
            };

            // perform the search
            UnifiedSearchResponse unifiedSearchResponse = NotificationUtils.GetUnifiedSearchResponse(request);
            if (unifiedSearchResponse == null || unifiedSearchResponse.searchResults == null)
            {
                log.ErrorFormat("elastic search did not find any results (error occurred). GID: {0}, user ID: {1}, search object: {2}", groupId, userId, JsonConvert.SerializeObject(request));
                response.Status = new Status() { Code = (int)eResponseStatus.Error, Message = eResponseStatus.Error.ToString() };
                return response;
            }

            if (unifiedSearchResponse.searchResults.Count == 0)
            {
                log.DebugFormat("elastic search did not find any results. GID: {0}, user ID: {1}, search Query: {2}", groupId, userId, newFilter.ToString());
                response.Status = new Status() { Code = (int)eResponseStatus.OK, Message = eResponseStatus.OK.ToString() };
                return response;
            }

            // build final result 
            List<int> resultProgramsIds = new List<int>();
            resultProgramsIds.AddRange(unifiedSearchResponse.searchResults.Select((item => int.Parse(item.AssetId))));
            response.Reminders = dbReminders.Where(reminder => resultProgramsIds.Exists(x => x == reminder.Reference)).ToList();
            response.TotalCount = unifiedSearchResponse.m_nTotalItems;
            response.Status = new Status() { Code = (int)eResponseStatus.OK, Message = eResponseStatus.OK.ToString() };

            // Clear Announcements
            UserMessageFlow.DeleteOldAnnouncements(groupId, userNotificationData);

            return response;
        }

        public static SeriesRemindersResponse GetUserSeriesReminders(int groupId, int userId, List<string> seriesIds, List<long> seasonNumbers, long? epgChannelId,
            int pageSize, int pageIndex)
        {
            SeriesRemindersResponse response = new SeriesRemindersResponse();

            // validate reminder is enabled
            if (!NotificationSettings.IsPartnerRemindersEnabled(groupId))
            {
                log.ErrorFormat("GetUserReminders - partner reminder is disabled. groupID = {0}", groupId);
                response.Status = new Status((int)eResponseStatus.FeatureDisabled, "Feature Disabled");
                return response;
            }

            // Get user notification data
            bool isDocExist = false;
            UserNotification userNotificationData = NotificationDal.GetUserNotificationData(groupId, userId, ref isDocExist);
            if (userNotificationData == null ||
                userNotificationData.SeriesReminders == null ||
                userNotificationData.SeriesReminders.Count == 0)
            {
                log.DebugFormat("User doesn't have any series reminders. GID: {0}, user ID: {1}", groupId, userId);
                response.Status = new Status() { Code = (int)eResponseStatus.OK, Message = eResponseStatus.OK.ToString() };
                return response;
            }

            // get reminder from DB
            List<DbSeriesReminder> dbSeriesReminders = Utils.GetSeriesReminders(groupId, userNotificationData.SeriesReminders.Select(userAnn => userAnn.AnnouncementId).ToList());

            if (dbSeriesReminders == null || dbSeriesReminders.Count == 0)
            {
                log.DebugFormat("user reminders were not found on DB. GID: {0}, user ID: {1}", groupId, userId);
                response.Status = new Status() { Code = (int)eResponseStatus.OK, Message = eResponseStatus.OK.ToString() };
                return response;
            }

            // seasons filtering
            if (seriesIds != null && seriesIds.Count == 1 && seasonNumbers != null && seasonNumbers.Count > 0)
            {
                dbSeriesReminders = dbSeriesReminders.Where(sr => seriesIds.Contains(sr.SeriesId) && seasonNumbers.Contains(sr.SeasonNumber)).ToList();
            }
            // series filter
            else if (seriesIds != null && seriesIds.Count > 0 && (seasonNumbers == null || seasonNumbers.Count == 0))
            {
                dbSeriesReminders = dbSeriesReminders.Where(sr => seriesIds.Contains(sr.SeriesId) && sr.SeasonNumber == 0).ToList();
            }

            if (epgChannelId.HasValue && epgChannelId.Value != 0)
            {
                dbSeriesReminders = dbSeriesReminders.Where(sr => sr.EpgChannelId == epgChannelId.Value).ToList();
            }

            if (dbSeriesReminders == null || dbSeriesReminders.Count == 0)
            {
                log.DebugFormat("user reminders were not found on DB. GID: {0}, user ID: {1}", groupId, userId);
                response.Status = new Status() { Code = (int)eResponseStatus.OK, Message = eResponseStatus.OK.ToString() };
                return response;
            }

            response.TotalCount = dbSeriesReminders.Count;
            response.Reminders = dbSeriesReminders.Skip(pageIndex * pageSize).Take(pageSize).ToList();
            response.Status = new Status() { Code = (int)eResponseStatus.OK, Message = eResponseStatus.OK.ToString() };


            // Clear Announcements
            UserMessageFlow.DeleteOldAnnouncements(groupId, userNotificationData);

            return response;
        }

        internal static RegistryResponse RegisterPushSeriesReminderParameters(int groupId, long seriesReminderId, string hash, string ip)
        {
            RegistryResponse response = new RegistryResponse() { Status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString()) };

            // get all reminders
            var reminders = NotificationDal.GetSeriesReminders(groupId, new List<long> { seriesReminderId });
            if (reminders == null || reminders.Count == 0)
            {
                log.Error("GetPushWebParams: series reminders were not found.");
                response.Status = new Status((int)eResponseStatus.ReminderNotFound, "reminders were not found");
                return response;
            }

            // build queue name
            string queueName = string.Format(REMINDER_QUEUE_NAME_FORMAT, groupId, seriesReminderId);

            // get relevant reminder
            var reminder = reminders.Where(x => x.ID == seriesReminderId).FirstOrDefault();
            if (reminder == null)
            {
                log.ErrorFormat("GetPushWebParams: reminder not found. id: {0}.", seriesReminderId);
                response.Status = new Status((int)eResponseStatus.ItemNotFound, eResponseStatus.ItemNotFound.ToString());
            }
            else
            {
                // update web push queue name on DB (if necessary)
                if (string.IsNullOrEmpty(reminder.RouteName))
                {
                    reminder.RouteName = queueName;

                    if (DAL.NotificationDal.SetSeriesReminder(reminder) == 0)
                    {
                        log.ErrorFormat("Error while trying to update reminder with web push queue name. GID: {0}, reminder ID: {1}, queue name: {2}",
                            groupId,
                            reminder.ID,
                            queueName);
                    }
                    else
                    {
                        log.DebugFormat("Successfully update reminder with web push queue name. GID: {0}, reminder ID: {1}, queue name: {2}",
                            groupId,
                            reminder.ID,
                            queueName);
                    }
                }

                // create key
                string keyToEncrypt = string.Format("{0}:{1}", queueName, hash);
                string encryptedKey = EncryptUtils.EncryptRJ128(outerPushServerSecret, outerPushServerIV, keyToEncrypt);

                // create URL
                Random rand = new Random();
                string tokenPart = string.Format("{0}:{1}:{2}:{3}", outerPushServerSecret, ip, hash, rand.Next());
                string encryptedtokenPart = EncryptUtils.EncryptRJ128(outerPushServerSecret, outerPushServerIV, tokenPart);
                string token = HttpUtility.UrlEncode(Convert.ToBase64String(Encoding.UTF8.GetBytes(groupId + ":" + encryptedtokenPart)));
                string url = string.Format(@"http://{0}/?p={1}&x={2}", outerPushDomainName, groupId, token);

                log.DebugFormat("GetPushWebParams: Create URL and key for queue: {0}. URL: {1}, KEY: {2}", queueName, url, encryptedKey);
                response.Url = url;
                response.Key = encryptedKey;
                response.NotificationId = seriesReminderId;
            }
            return response;
        }
    }
}
