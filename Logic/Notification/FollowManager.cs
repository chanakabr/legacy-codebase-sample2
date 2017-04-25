using APILogic.AmazonSnsAdapter;
using APILogic.DmsService;
using ApiObjects;
using ApiObjects.Notification;
using ApiObjects.Response;
using ApiObjects.SearchObjects;
using Core.Catalog;
using Core.Catalog.Request;
using Core.Catalog.Response;
using DAL;
using KLogMonitor;
using Newtonsoft.Json;
using Core.Notification.Adapters;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Core.Notification
{
    public class FollowManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private const string MESSAGE_PLACEHOLDERS_ARE_INVALID = "Message placeholders are not valid";
        private const string URL_PLACEHOLDERS_ARE_INVALID = "URL placeholders are not valid";
        private const string DATETIME_FORMAT_IS_INVALID = "Date time format is invalid";
        private const string FOLLOW_TEMPLATE_NOT_FOUND = "Message template not found";
        private static string CatalogSignString = Guid.NewGuid().ToString();
        private static string CatalogSignatureKey = ODBCWrapper.Utils.GetTcmConfigValue("CatalogSignatureKey");


        /// <summary>
        /// Add/Update group's Message template
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="messageTemplate"></param>
        /// <returns></returns>
        public static MessageTemplateResponse SetMessageTemplate(int groupId, MessageTemplate messageTemplate)
        {
            MessageTemplateResponse response = new MessageTemplateResponse();

            response.Status = ValidateMessageTemplate(groupId, messageTemplate);

            if (response.Status.Code != (int)eResponseStatus.OK)
            {
                return response;
            }

            response.MessageTemplate = SetMessageTemplateAtDB(groupId, messageTemplate);

            if (response.MessageTemplate == null || response.MessageTemplate.Id <= 0)
            {
                log.ErrorFormat("Error while inserting messageTemplate {0} to DB", messageTemplate.ToString());
                response.Status = new Status() { Code = (int)eResponseStatus.Error, Message = eResponseStatus.Error.ToString() };
            }

            // clear message templates cache
            NotificationCache.Instance().RemoveMessageTemplateFromCache(groupId);

            return response;
        }

        private static MessageTemplate SetMessageTemplateAtDB(int groupId, MessageTemplate messageTemplate)
        {
            return NotificationDal.SetMessageTemplate(groupId, messageTemplate);
        }

        private static Status ValidateMessageTemplate(int groupId, MessageTemplate messageTemplate)
        {
            Status validationStatus = new Status() { Code = (int)eResponseStatus.OK, Message = eResponseStatus.OK.ToString() };

            // ValidateMessage - 
            // 1. check if eFollowSeriesPlaceHolders
            // 2. replace placeHolder case Sensitive   (formattedMessage)         
            string formattedMessage = messageTemplate.Message;
            validationStatus = ValidateMessage(ref formattedMessage, messageTemplate.TemplateType);

            if (validationStatus.Code != (int)eResponseStatus.OK)
            {
                log.ErrorFormat("ValidateMessageTemplate: invalid message: {0}, GID: {1}", messageTemplate.Message, groupId);
                return validationStatus;
            }

            messageTemplate.Message = formattedMessage;

            // ValidateDatFormat
            validationStatus = ValidateDateFormat(messageTemplate.DateFormat);
            if (validationStatus.Code != (int)eResponseStatus.OK)
            {
                log.ErrorFormat("ValidateMessageTemplate: invalid date format: {0}, GID: {1}", messageTemplate.DateFormat, groupId);
                return validationStatus;
            }

            // ValidateMessage - 
            // 1. check if eFollowSeriesPlaceHolders
            // 2. replace placeHolder case Sensitive   (formattedMessage)         
            string formattedUrl = messageTemplate.URL;
            validationStatus = ValidateUrl(ref formattedUrl, messageTemplate.TemplateType);

            if (validationStatus.Code != (int)eResponseStatus.OK)
            {
                log.ErrorFormat("ValidateMessageTemplate: invalid url: {0}, GID: {1}", messageTemplate.URL, groupId);
                return validationStatus;
            }

            messageTemplate.URL = formattedUrl;

            return validationStatus;
        }

        public static MessageTemplateResponse GetMessageTemplate(int groupId, MessageTemplateType assetType)
        {
            MessageTemplateResponse response = new MessageTemplateResponse();
            response.Status = new Status() { Code = (int)eResponseStatus.OK, Message = eResponseStatus.OK.ToString() };

            response.MessageTemplate = NotificationCache.Instance().GetMessageTemplates(groupId).First(x => x.TemplateType == assetType);

            if (response.MessageTemplate == null || response.MessageTemplate.Id <= 0)
            {
                response.Status = new Status() { Code = (int)eResponseStatus.MessageTemplateNotFound, Message = FOLLOW_TEMPLATE_NOT_FOUND };
                log.ErrorFormat("GetMessageTemplate: message template not found: GID: {0}, assetType: {1}", groupId, assetType.ToString());
            }

            return response;
        }

        private static Status ValidateDateFormat(string dateTimeFormat)
        {
            Status validationStatus = new Status() { Code = (int)eResponseStatus.OK, Message = eResponseStatus.OK.ToString() };

            DateTime tmpDateTime; // only for date format check
            bool isValid = DateTime.TryParseExact(DateTime.UtcNow.ToString(dateTimeFormat), dateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out tmpDateTime);
            if (!isValid)
            {
                log.ErrorFormat("ValidateDateFormat - Wrong DateTime format {0}", dateTimeFormat);
                validationStatus.Code = (int)eResponseStatus.DatetimeFormatIsInvalid;
                validationStatus.Message = DATETIME_FORMAT_IS_INVALID;
            }

            return validationStatus;
        }

        private static Status ValidateMessage(ref string message, MessageTemplateType assetTypes)
        {
            Status validationStatus = new Status() { Code = (int)eResponseStatus.Error, Message = eResponseStatus.Error.ToString() };

            switch (assetTypes)
            {
                case ApiObjects.MessageTemplateType.Series:
                    validationStatus = ValidatePlaceholders<eFollowSeriesPlaceHolders>(ref message);
                    break;
                case ApiObjects.MessageTemplateType.Reminder:
                    validationStatus = ValidatePlaceholders<eReminderPlaceHolders>(ref message);
                    break;
                case ApiObjects.MessageTemplateType.Churn:
                    validationStatus = ValidatePlaceholders<eChurnPlaceHolders>(ref message);
                    break;
                default:
                    break;
            }

            if (validationStatus.Code != (int)eResponseStatus.OK)
            {
                validationStatus.Code = (int)eResponseStatus.MessagePlaceholdersInvalid; ;
                validationStatus.Message = MESSAGE_PLACEHOLDERS_ARE_INVALID;
            }
            return validationStatus;
        }

        private static Status ValidateUrl(ref string url, MessageTemplateType assetTypes)
        {
            Status validationStatus = new Status() { Code = (int)eResponseStatus.Error, Message = eResponseStatus.Error.ToString() };

            switch (assetTypes)
            {
                case ApiObjects.MessageTemplateType.Series:
                    validationStatus = ValidatePlaceholders<eFollowSeriesPlaceHolders>(ref url);
                    break;
                case ApiObjects.MessageTemplateType.Reminder:
                    validationStatus = ValidatePlaceholders<eReminderPlaceHolders>(ref url);
                    break;
                case ApiObjects.MessageTemplateType.Churn:
                    validationStatus = ValidatePlaceholders<eChurnPlaceHolders>(ref url);
                    break;
                default:
                    break;
            }

            if (validationStatus.Code != (int)eResponseStatus.OK)
            {
                validationStatus.Code = (int)eResponseStatus.URLPlaceholdersInvalid; ;
                validationStatus.Message = URL_PLACEHOLDERS_ARE_INVALID;
            }
            return validationStatus;
        }

        public static Status ValidatePlaceholders<TEnum>(ref string message) where TEnum : struct
        {
            Status validationStatus = new Status() { Code = (int)eResponseStatus.OK, Message = eResponseStatus.OK.ToString() };

            if (!string.IsNullOrEmpty(message))
            {
                //check for letters only inside {}
                //--------------------------------
                Regex expression = new Regex(@"\{([^}]*)\}");
                // get message place-holders 
                var messagePlaceHolders = expression.Matches(message).Cast<Match>().Select(m => m.Value.Replace("{", "").Replace("}", "")).ToArray();
                //get enum values
                var followSeriesPlaceHoldersEnumValues = Enum.GetNames(typeof(TEnum));
                //compare string [message place-holders] not in [enum values]
                var notInEnum = messagePlaceHolders.Select(m => m.ToLower()).Except(followSeriesPlaceHoldersEnumValues.Select(x => x.ToLower())).ToList();
                // 
                if (notInEnum.Count > 0)
                {
                    log.ErrorFormat("ValidateSeriesPlaceholders - Wrong place-holders {0}, message: {1}", string.Join(", ", notInEnum), message);
                    validationStatus.Code = (int)eResponseStatus.Error;
                }

                // replace placeHoldaer to the enums exact values 
                TEnum tmpeFollowSeriesPlaceHolders;
                foreach (var s in messagePlaceHolders)
                {
                    if (Enum.TryParse<TEnum>(s, true, out tmpeFollowSeriesPlaceHolders))
                    {
                        message = message.Replace(s, tmpeFollowSeriesPlaceHolders.ToString());
                    }
                }
            }

            return validationStatus;
        }

        public static Status CreateFollowAnnouncement(int groupId, FollowDataBase followData, ref DbAnnouncement newAnnouncement)
        {
            try
            {
                int announcementId = 0;
                string announcementName = followData.Title;

                // create Amazon topic 
                string externalAnnouncementId = string.Empty;
                if (NotificationSettings.IsPartnerPushEnabled(groupId))
                {
                    externalAnnouncementId = NotificationAdapter.CreateAnnouncement(groupId, announcementName);
                    if (string.IsNullOrEmpty(externalAnnouncementId))
                    {
                        log.DebugFormat("failed to create announcement groupID = {0}, announcementName = {1}", groupId, announcementName);
                        return new Status((int)eResponseStatus.FailCreateAnnouncement, "fail create Guest announcement");
                    }
                }

                // create DB announcement
                announcementId = DAL.NotificationDal.Insert_Announcement(groupId, announcementName, externalAnnouncementId, (int)eMessageType.Push, (int)eAnnouncementRecipientsType.Other, followData.FollowPhrase, followData.FollowReference);
                if (announcementId == 0)
                {
                    log.DebugFormat("failed to insert announcement to DB groupID = {0}, announcementName = {1}", groupId, announcementName);
                    return new Status((int)eResponseStatus.Error, "fail insert guest announcement to DB");
                }
                else
                {
                    NotificationCache.Instance().RemoveAnnouncementsFromCache(groupId);

                    newAnnouncement = new DbAnnouncement()
                    {
                        ExternalId = externalAnnouncementId,
                        FollowPhrase = followData.FollowPhrase,
                        FollowReference = followData.FollowReference,
                        ID = announcementId,
                        Name = announcementName,
                        RecipientsType = eAnnouncementRecipientsType.Other
                    };
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("CreateFollowAnnouncement failed groupId = {0}, ex = {1}", groupId, ex.Message), ex);
                return new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            }
            return new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString()); ;
        }

        public static GetUserFollowsResponse Get_UserFollows(int groupId, int userId, int pageSize, int pageIndex, OrderDir order)
        {
            GetUserFollowsResponse userFollowResponse = new GetUserFollowsResponse();
            userFollowResponse.Status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            userFollowResponse.Follows = new List<FollowDataBase>();

            // get user notifications
            bool docExists = false;
            UserNotification userNotificationData = DAL.NotificationDal.GetUserNotificationData(groupId, userId, ref docExists);

            if (userNotificationData == null && docExists)
            {
                userFollowResponse.Status = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.ErrorFormat("Error retrieving User notification data. PID: {0}, UID: {1}", groupId, userId);
                return userFollowResponse;
            }

            if (userNotificationData != null &&
                userNotificationData.Announcements != null &&
                userNotificationData.Announcements.Count > 0)
            {
                // get announcement from DB
                List<DbAnnouncement> dbAnnouncements = NotificationCache.Instance().GetAnnouncements(groupId).Where(ann => userNotificationData.Announcements.Select(userAnn => userAnn.AnnouncementId).Contains(ann.ID)).ToList();

                // create response object
                userFollowResponse.Follows = dbAnnouncements.Select(x => new FollowDataBase(groupId, x.FollowPhrase)
                {
                    AnnouncementId = x.ID,
                    Status = 1,                         // only enabled status in this phase
                    Title = x.Name,
                    FollowReference = x.FollowReference,
                    Timestamp = userNotificationData.Announcements.Where(userAnn => userAnn.AnnouncementId == x.ID).First().AddedDateSec
                })
                .OrderBy(x => x.Timestamp).ToList();
            }
            else
                log.DebugFormat("User doesn't have any follow notifications. PID: {0}, UID: {1}", groupId, userId);

            if (userFollowResponse.Follows != null &&
                userFollowResponse.Follows.Count > 0)
            {
                // set result order
                if (order == OrderDir.DESC)
                    userFollowResponse.Follows.Reverse();

                // update total results
                userFollowResponse.TotalCount = userFollowResponse.Follows.Count;

                // paging
                userFollowResponse.Follows = userFollowResponse.Follows.Skip(pageSize * pageIndex).Take(pageSize).ToList();
            }

            return userFollowResponse;
        }

        public static Status Unfollow(int groupId, int userId, FollowDataBase followData)
        {
            Status statusResult = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());

            // populate follow phrase
            followData.FollowPhrase = GetSeriesFollowPhrase(groupId, followData.Title);

            // get user notification data
            bool docExists = false;
            UserNotification userNotificationData = DAL.NotificationDal.GetUserNotificationData(groupId, userId, ref docExists);

            // get announcement from DB
            List<DbAnnouncement> dbAnnouncements = NotificationCache.Instance().GetAnnouncements(groupId).Where(ann => ann.FollowPhrase == followData.FollowPhrase).ToList();
            if (dbAnnouncements.Count == 0)
            {
                log.ErrorFormat("user is not following any asset. group: {0}, user: {1}, phrase: {2}", groupId, userId, followData.FollowPhrase);
                statusResult = new Status((int)eResponseStatus.UserNotFollowing, "user is not following asset");
                return statusResult;
            }

            // get user announcement
            long announcementId = dbAnnouncements.First().ID;
            if (userNotificationData == null ||
                userNotificationData.Announcements == null ||
                userNotificationData.Announcements.Where(x => x.AnnouncementId == announcementId).Count() == 0)
            {
                log.DebugFormat("user notification data wasn't found. GID: {0}, UID: {1}", groupId, userId);
                statusResult = new Status((int)eResponseStatus.UserNotFollowing, "user is not following asset");
                return statusResult;
            }

            // iterate through user devices and unfollow series
            if (userNotificationData.devices == null || userNotificationData.devices.Count == 0)
                log.DebugFormat("User doesn't have any devices. PID: {0}, UID: {1}", groupId, userId);
            else
            {
                foreach (UserDevice device in userNotificationData.devices)
                {
                    string udid = device.Udid;
                    if (string.IsNullOrEmpty(udid))
                    {
                        log.ErrorFormat("device UDID invalid: UDID: {0} PID: {1}, UID: {2}", device.Udid, groupId, userId);
                        continue;
                    }

                    // get device data
                    docExists = false;
                    DeviceNotificationData deviceNotificationData = DAL.NotificationDal.GetDeviceNotificationData(groupId, udid, ref docExists);
                    if (deviceNotificationData == null)
                    {
                        log.ErrorFormat("device notification data not found group: {0}, UID: {1}, UDID: {2}", groupId, userId, device.Udid);
                        continue;
                    }

                    // get device subscription
                    var subscribedAnnouncements = deviceNotificationData.SubscribedAnnouncements.Where(x => x.Id == announcementId);
                    if (subscribedAnnouncements == null || subscribedAnnouncements.Count() == 0)
                    {
                        log.ErrorFormat("device notification data had no subscription to announcement. group: {0}, UID: {1}, UDID: {2}", groupId, userId, device.Udid);
                        continue;
                    }

                    // unsubscribe device 
                    List<UnSubscribe> unsubscibeList = new List<UnSubscribe>() 
                    { 
                        new UnSubscribe() 
                        { 
                            SubscriptionArn = subscribedAnnouncements.First().ExternalId 
                        } 
                    };

                    unsubscibeList = NotificationAdapter.UnSubscribeToAnnouncement(groupId, unsubscibeList);
                    if (unsubscibeList == null ||
                        unsubscibeList.Count == 0 ||
                        !unsubscibeList.First().Success
                        || !deviceNotificationData.SubscribedAnnouncements.Remove(subscribedAnnouncements.First())
                        || !DAL.NotificationDal.SetDeviceNotificationData(groupId, udid, deviceNotificationData))
                    {
                        log.ErrorFormat("error removing announcement from device subscribed. group: {0}, UID: {1}, UDID: {2}", groupId, userId, device.Udid);
                        continue;
                    }
                    else
                        log.DebugFormat("Successfully unsubscribed device from announcement group: {0}, UID: {1}, UDID: {2}", groupId, userId, device.Udid);
                }
            }

            // remove announcement from user announcement list
            Announcement announcement = userNotificationData.Announcements.Where(x => x.AnnouncementId == announcementId).First();
            if (!userNotificationData.Announcements.Remove(announcement) ||
                !NotificationDal.RemoveUserFollowNotification(groupId, userId, announcement.AnnouncementId) ||
                !DAL.NotificationDal.SetUserNotificationData(groupId, userId, userNotificationData))
            {
                log.DebugFormat("an error while trying to remove announcement. GID: {0}, UID: {1}, announcementId: {2}", groupId, userId, announcementId);
                statusResult = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                return statusResult;
            }
            else
                log.DebugFormat("Successfully removed announcement from user announcements object group: {0}, UID: {1}", groupId, userId);

            return statusResult;
        }

        public static FollowResponse Follow(int groupId, int userId, FollowDataBase followData)
        {
            FollowResponse response = new FollowResponse()
            {
                Status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString())
            };

            // populate follow phrase
            followData.FollowPhrase = GetSeriesFollowPhrase(groupId, followData.Title);

            // validate asset type
            if (!ValidateFollowData(groupId, userId, followData))
            {
                response.Status = new Status((int)eResponseStatus.InvalidAssetId, "invalid asset");
                return response;
            }

            // get user notifications
            bool docExists = false;
            UserNotification userNotificationData = DAL.NotificationDal.GetUserNotificationData(groupId, userId, ref docExists);
            if (userNotificationData == null)
            {
                if (docExists)
                {
                    // error while getting user notification data
                    log.ErrorFormat("error retrieving user announcement data. GID: {0}, UID: {1}", groupId, userId);
                    response.Status = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                    return response;
                }
                else
                {
                    log.DebugFormat("user announcement data wasn't found - going to create a new one. GID: {0}, UID: {1}", groupId, userId);

                    // create user notification object
                    userNotificationData = new UserNotification(userId) { CreateDateSec = TVinciShared.DateUtils.UnixTimeStampNow() };

                    //update user settings according to partner settings configuration                    
                    userNotificationData.Settings.EnablePush = NotificationSettings.IsPartnerPushEnabled(groupId, userId);
                }
            }

            try
            {
                // get user announcements from DB
                DbAnnouncement announcementToFollow = null;
                List<DbAnnouncement> dbAnnouncements = NotificationCache.Instance().GetAnnouncements(groupId);
                if (dbAnnouncements != null)
                    announcementToFollow = dbAnnouncements.FirstOrDefault(ann => ann.FollowPhrase == followData.FollowPhrase);

                if (announcementToFollow == null)
                {
                    // follow announcement doesn't exists - first time the series is being followed - create a new one
                    Status createAnnStatus = CreateFollowAnnouncement(groupId, followData, ref announcementToFollow);
                    if (createAnnStatus.Code != (int)eResponseStatus.OK)
                    {
                        log.ErrorFormat("user notification data not found group: {0}, user: {1}", groupId, userId);
                        response.Status = createAnnStatus;
                        return response;
                    }
                }

                // validate existence of db follow announcement
                if (announcementToFollow == null)
                {
                    log.ErrorFormat("announcement not found group: {0}, user: {1}, phrase: {2}", groupId, userId, followData.FollowPhrase);
                    response.Status = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                    return response;
                }

                followData.AnnouncementId = announcementToFollow.ID;
                if (userNotificationData.Announcements.Where(x => x.AnnouncementId == followData.AnnouncementId).Count() > 0)
                {
                    // user already follows the series
                    log.DebugFormat("User is already following announcement. PID: {0}, UID: {1}, Announcement ID: {2}", groupId, userId, followData.AnnouncementId);
                    response.Status = new Status((int)eResponseStatus.UserAlreadyFollowing, "User already following");
                    return response;
                }

                // create added time
                long addedSecs = ODBCWrapper.Utils.DateTimeToUnixTimestampUtc(DateTime.UtcNow);

                if (userNotificationData.devices != null &&
                    userNotificationData.devices.Count > 0 &&
                    NotificationSettings.IsPartnerPushEnabled(groupId) &&
                    NotificationSettings.IsUserFollowPushEnabled(userNotificationData.Settings))
                {
                    foreach (UserDevice device in userNotificationData.devices)
                    {
                        string udid = device.Udid;
                        if (string.IsNullOrEmpty(udid))
                        {
                            log.Error("device UDID is empty: " + device.Udid);
                            continue;
                        }

                        log.DebugFormat("adding announcement to device group: {0}, user: {1}, UDID: {2}, announcementId: {3}", groupId, userId, udid, followData.AnnouncementId);

                        // get device notification data
                        docExists = false;
                        DeviceNotificationData deviceNotificationData = DAL.NotificationDal.GetDeviceNotificationData(groupId, udid, ref docExists);
                        if (deviceNotificationData == null)
                        {
                            log.ErrorFormat("device notification data not found group: {0}, UDID: {1}", groupId, device.Udid);
                            continue;
                        }

                        try
                        {
                            // validate device doesn't already have the announcement
                            var subscribedAnnouncements = deviceNotificationData.SubscribedAnnouncements.Where(x => x.Id == followData.AnnouncementId);
                            if (subscribedAnnouncements != null && subscribedAnnouncements.Count() > 0)
                            {
                                log.ErrorFormat("user already following announcement on device. group: {0}, UDID: {1}", groupId, device.Udid);
                                continue;
                            }

                            // get push data
                            PushData pushData = PushAnnouncementsHelper.GetPushData(groupId, udid, string.Empty);
                            if (pushData == null)
                            {
                                log.ErrorFormat("push data not found. group: {0}, UDID: {1}", groupId, device.Udid);
                                continue;
                            }

                            // subscribe device to announcement
                            AnnouncementSubscriptionData subData = new AnnouncementSubscriptionData()
                            {
                                EndPointArn = pushData.ExternalToken, // take from pushdata (with UDID)
                                Protocol = EnumseDeliveryProtocol.application,
                                TopicArn = announcementToFollow.ExternalId,
                                ExternalId = announcementToFollow.ID
                            };

                            List<AnnouncementSubscriptionData> subs = new List<AnnouncementSubscriptionData>() { subData };
                            subs = NotificationAdapter.SubscribeToAnnouncement(groupId, subs);
                            if (subs == null || subs.Count == 0 || string.IsNullOrEmpty(subs.First().SubscriptionArnResult))
                            {
                                log.ErrorFormat("Error registering device to announcement. group: {0}, UDID: {1}", groupId, device.Udid);
                                continue;
                            }

                            // update device notification object
                            NotificationSubscription sub = new NotificationSubscription()
                            {
                                ExternalId = subs.First().SubscriptionArnResult,
                                Id = followData.AnnouncementId,
                                SubscribedAtSec = addedSecs
                            };
                            deviceNotificationData.SubscribedAnnouncements.Add(sub);

                            if (!DAL.NotificationDal.SetDeviceNotificationData(groupId, udid, deviceNotificationData))
                                log.ErrorFormat("error setting device notification data. group: {0}, UDID: {1}, topic: {2}", groupId, device.Udid, subData.EndPointArn);
                            else
                            {
                                log.DebugFormat("Successfully registered device to announcement. group: {0}, UDID: {1}, topic: {2}", groupId, device.Udid, subData.EndPointArn);
                            }
                        }
                        catch (Exception ex)
                        {
                            log.Error("Error in follow", ex);
                        }
                    }
                }

                // update user notification object
                userNotificationData.Announcements.Add(new Announcement()
                {
                    AnnouncementId = followData.AnnouncementId,
                    AnnouncementName = announcementToFollow.Name,
                    AddedDateSec = addedSecs
                });

                response.Follow = new FollowDataBase(groupId, announcementToFollow.FollowPhrase)
                {
                    AnnouncementId = announcementToFollow.ID,
                    Status = 1,                         // only enabled status in this phase
                    Title = announcementToFollow.Name,
                    //Type = FollowType.TV_Series_VOD,  // only TV series in this phase
                    FollowReference = announcementToFollow.FollowReference,
                    Timestamp = userNotificationData.Announcements.Where(userAnn => userAnn.AnnouncementId == announcementToFollow.ID).First().AddedDateSec
                };

                if (!DAL.NotificationDal.SetUserNotificationData(groupId, userId, userNotificationData))
                    log.ErrorFormat("error setting user notification data. group: {0}, user id: {1}", groupId, userId);
                else
                {
                    // update user following items
                    if (!NotificationDal.SetUserFollowNotificationData(groupId, userId, (int)followData.AnnouncementId))
                        log.ErrorFormat("Error updating the user following notification data. GID :{0}, user ID: {1}, Announcement ID: {2}", groupId, userId, followData.AnnouncementId);

                    log.DebugFormat("successfully updated user notification data. group: {0}, user id: {1}", groupId, userId);
                }
            }
            catch (Exception ex)
            {
                log.Error("Error in follow", ex);
            }

            return response;
        }

        private static bool ValidateFollowData(int groupId, int userId, FollowDataBase followData)
        {
            if (followData is FollowDataTvSeries)
            {
                bool res = NotificationCache.Instance().GetOTTAssetTypeByMediaTypeId(groupId, followData.Type) == eOTTAssetTypes.Series;
                if (!res)
                    log.DebugFormat("Invalid asset: group: {0}, user: {1}, asset: {2}", groupId, userId, (followData as FollowDataTvSeries).AssetId);

                return res;
            }

            return false;
        }

        public static void AddFollowRequest(int groupId, string userId, int mediaID)
        {
            AddTvSeriesFollowRequest(groupId, userId, mediaID);
        }

        public static void AddTvSeriesFollowRequest(int groupId, string userId, int mediaID)
        {
            MediaResponse response = null;

            // get media information
            var request = new MediasProtocolRequest()
            {
                m_sSignature = NotificationUtils.GetSignature(CatalogSignString, CatalogSignatureKey),
                m_sSignString = CatalogSignString,
                m_lMediasIds = new List<int> { mediaID },
                m_nGroupID = groupId,
                m_oFilter = new Filter(),
                m_sSiteGuid = userId
            };

            try
            {
                response = request.GetMediasByIDs(request);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error when calling catalog to get the ingested media information. request: {0}, ex: {1}", JsonConvert.SerializeObject(request), ex);
                return;
            }

            // Get catalog start date & series name
            string[] seriesNames = null;
            long catalogStartDate = 0;
            if (response != null &&
                response.m_lObj != null &&
                response.m_lObj.Count > 0 &&
                response.m_lObj.First() != null &&
                response.m_lObj.First() is MediaObj)
            {
                MediaObj mediaObj = response.m_lObj.First() as MediaObj;
                if (mediaObj.m_lTags != null &&
                    mediaObj.m_lTags.Count > 0 &&
                    mediaObj.m_lTags.First() != null)
                {
                    catalogStartDate = ODBCWrapper.Utils.DateTimeToUnixTimestampUtc(mediaObj.m_dCatalogStartDate);

                    // validate the media type ID is a series
                    if (mediaObj.m_oMediaType != null &&
                        NotificationCache.Instance().GetEpisodeMediaTypeId(groupId) == mediaObj.m_oMediaType.m_nTypeID)
                    {
                        // check if media is an episode
                        foreach (var tag in mediaObj.m_lTags)
                        {
                            if (tag.m_oTagMeta != null && tag.m_oTagMeta.m_sName.ToLower().Trim() == GetEpisodeAssociationTag(groupId).ToLower().Trim())
                                seriesNames = tag.m_lValues.ToArray();
                        }
                    }
                }
            }

            // check series name was found
            if (seriesNames == null || seriesNames.Length == 0)
            {
                log.DebugFormat("ingested media is not a series episode (series name is empty). group {0}, media: {1}", groupId, mediaID);
                return;
            }
            else
                log.DebugFormat("ingested media is an episode of a series. group {0}, media: {1}, series name: {2}", groupId, mediaID, string.Join(",", seriesNames));

            // build of series phrases
            List<string> phrases = seriesNames.Select(x => GetSeriesFollowPhrase(groupId, x)).ToList();

            // get announcement of message
            List<DbAnnouncement> dbAnnouncements = NotificationCache.Instance().GetAnnouncements(groupId);
            if (dbAnnouncements != null)
                dbAnnouncements = dbAnnouncements.Where(dbAnn => phrases.Contains(dbAnn.FollowPhrase)).ToList();

            if (dbAnnouncements == null || dbAnnouncements.Count == 0)
            {
                log.DebugFormat("no announcements found for ingested media: group {0}, media: {1}, search phrase phrases: {2}", groupId, mediaID, string.Join(",", phrases));
                return;
            }
            else
            {
                log.DebugFormat("announcement found for ingested media: GID: {0}, media ID: {1}, Announcement ID: {2}, announcement name: {3}",
                    groupId,
                    mediaID,
                    dbAnnouncements[0].ID,
                    dbAnnouncements[0].Name);
            }

            // validate if announcement should be automatically sent
            DbAnnouncement announcement = dbAnnouncements[0];
            if ((announcement.AutomaticIssueFollowNotification.HasValue &&
               !announcement.AutomaticIssueFollowNotification.Value)
               || (!announcement.AutomaticIssueFollowNotification.HasValue &&
               !NotificationSettings.ShouldIssueAutomaticFollowNotification(groupId)))
            {
                log.DebugFormat("Notification wasn't sent due to 'ShouldIssueAutomaticFollowNotification' parameter is false. group {0}, media: {1}, Announcement id: {2}, Announcement name: {3}, Announcement phrase: {4}, Announcement ref: {5}",
                    groupId, mediaID, announcement.ID, announcement.Name, announcement.FollowPhrase, announcement.FollowReference);
                return;
            }

            // get message template and build message with it
            MessageTemplateResponse msgTemplateResponse = GetMessageTemplate(groupId, MessageTemplateType.Series);
            if (msgTemplateResponse.Status.Code != (int)eResponseStatus.OK)
            {
                log.ErrorFormat("message template not found. group: {0}, asset type: {1}, error: {2}-{3}",
                    groupId,
                    MessageTemplateType.Series.ToString(),
                    msgTemplateResponse.Status.Code,
                    msgTemplateResponse.Status.Message);
                return;
            }

            MessageAnnouncement message = new MessageAnnouncement()
            {
                AnnouncementId = announcement.ID,
                Enabled = true,
                Message = msgTemplateResponse.MessageTemplate.Message,
                MessageReference = mediaID.ToString(),
                Name = string.Format("announcement_{0}_{1}", eOTTAssetTypes.Series.ToString(), mediaID),
                Recipients = eAnnouncementRecipientsType.Other,
                StartTime = catalogStartDate,
                Timezone = "UTC",
                Status = eAnnouncementStatus.NotSent
            };

            // check if previous unsent messages were queued for this announcement and asset (series).
            bool shouldSend = true;
            DataRowCollection previousAnnouncementMessageRows = NotificationDal.Get_MessageAnnouncementByAnnouncementAndReference(announcement.ID, mediaID.ToString());
            if (previousAnnouncementMessageRows != null)
            {
                // check for each message (should be a single message) if not sent and needs to update
                foreach (DataRow row in previousAnnouncementMessageRows)
                {
                    MessageAnnouncement msgFromQ = Core.Notification.Utils.GetMessageAnnouncementFromDataRow(row);
                    if (msgFromQ.Status == eAnnouncementStatus.NotSent)
                    {
                        if (msgFromQ.StartTime == catalogStartDate)
                        {
                            // message not changed.
                            log.DebugFormat("found previous message announcement that wasn't sent with the same start time. canceling follow notification. group {0}, previous message: {1}",
                                groupId, JsonConvert.SerializeObject(msgFromQ));

                            shouldSend = false;
                            break;
                        }
                        else
                        {
                            // update message & start time
                            log.DebugFormat("found previous message announcement that wasn't sent with the different start time. updating old message and canceling new one. group {0}, previous message: {1}, new message: {2}, new start time: {3}",
                                groupId,
                                JsonConvert.SerializeObject(msgFromQ),
                                message.Message,
                                message.StartTime);

                            // updating message
                            msgFromQ.StartTime = message.StartTime;
                            msgFromQ.Message = message.Message;
                            if (AnnouncementManager.UpdateMessageAnnouncement(groupId, msgFromQ.MessageAnnouncementId, msgFromQ, true, false).Status.Code == (int)eResponseStatus.OK)
                            {
                                log.DebugFormat("successfully updated previous message announcement. group ID: {0}, previous message: {1}, new message: {2}, new start time: {3}",
                                    groupId,
                                    JsonConvert.SerializeObject(msgFromQ),
                                    message.Message,
                                    message.StartTime);

                                // update announcement message sent date
                                if (!NotificationDal.UpdateAnnouncement(groupId, msgFromQ.AnnouncementId, announcement.AutomaticIssueFollowNotification, DateTime.UtcNow))
                                    log.ErrorFormat("Error while trying to update last announcement message sent date. GID: {0}, message: {1}", groupId, JsonConvert.SerializeObject(msgFromQ));

                                shouldSend = false;
                                break;
                            }
                            else
                            {
                                log.ErrorFormat("error while trying to update previous message announcement that wasn't sent with the different start time. group {0}, previous message: {1}, new message: {2}, new start time: {3}",
                                    groupId,
                                    JsonConvert.SerializeObject(msgFromQ),
                                    message.Message,
                                    message.StartTime);
                            }
                        }
                    }
                }
            }

            if (shouldSend)
            {
                // sending message to queue
                log.DebugFormat("about to add message announcement for: group {0}, media: {1}, start date: {2}", groupId, mediaID, catalogStartDate);
                var addMsgAnnResponse = AnnouncementManager.AddMessageAnnouncement(groupId, message, true, false);
                if (addMsgAnnResponse.Status.Code != (int)eResponseStatus.OK)
                {
                    log.ErrorFormat("add message announcement failed. group: {0}, asset type: {1}, error: {2}-{3}",
                        groupId, eOTTAssetTypes.Series.ToString(),
                        addMsgAnnResponse.Status.Code,
                        addMsgAnnResponse.Status.Message);
                }
                else
                {
                    log.DebugFormat("successfully created new message announcement in queue. group ID: {0}, message: {1}", groupId, JsonConvert.SerializeObject(message));

                    // update announcement message sent date
                    if (!NotificationDal.UpdateAnnouncement(groupId, message.AnnouncementId, announcement.AutomaticIssueFollowNotification, DateTime.UtcNow))
                        log.ErrorFormat("Error while trying to update last announcement message sent date. GID: {0}, message: {1}", groupId, JsonConvert.SerializeObject(message));
                }
            }
        }

        public static IdsResponse Get_FollowedAssetIdsFromAssets(int groupId, int userId, List<int> assets)
        {
            IdsResponse response = new IdsResponse(new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString()), new List<int>());

            var userFollows = Get_UserFollows(groupId, userId, 1000, 0, OrderDir.ASC);
            if (userFollows == null ||
                userFollows.Status == null ||
                userFollows.Status.Code != (int)eResponseStatus.OK ||
                userFollows.Follows == null ||
                userFollows.Follows.Count == 0)
            {
                // error is printed in Get_UserFollows
                response.Status = userFollows.Status;
                return response;
            }

            List<int> followedAssets = Get_FollowedAssets(groupId, userFollows.Follows);

            followedAssets = followedAssets.Where(x => assets.Contains(x)).ToList();

            response.Ids = followedAssets.ToList();

            return response;
        }

        private static List<int> Get_FollowedAssets(int groupId, List<FollowDataBase> follows)
        {
            List<int> ret = new List<int>();

            foreach (var follow in follows)
            {
                // if tv series take series asset id from follow reference field
                if (follow.FollowPhrase.ToLower().Trim().StartsWith(GetEpisodeAssociationTag(groupId).ToLower().Trim()))
                {
                    int id = 0;
                    if (int.TryParse(follow.FollowReference, out id))
                        ret.Add(id);
                }
            }

            return ret;
        }

        public static IdListResponse GetUserFeeder(int groupId, int userId, int pageSize, int pageIndex, OrderObj orderObj)
        {
            IdListResponse response = new IdListResponse() { Status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString()), Ids = new List<int>() };
            List<int> mediaIds = null;

            bool isDocExist = false;
            UserNotification userNotificationData = NotificationDal.GetUserNotificationData(groupId, userId, ref isDocExist);

            if (userNotificationData == null ||
                userNotificationData.Announcements == null ||
                userNotificationData.Announcements.Count == 0)
            {
                log.DebugFormat("User doesn't have any follow announcements. GID: {0}, user ID: {1}", groupId, userId);
                return response;
            }

            // get announcement from DB
            List<DbAnnouncement> dbAnnouncements = NotificationCache.Instance().GetAnnouncements(groupId).Where(ann => userNotificationData.Announcements.Select(userAnn => userAnn.AnnouncementId).Contains(ann.ID)).ToList();
            if (dbAnnouncements == null || dbAnnouncements.Count == 0)
            {
                log.ErrorFormat("user announcements were not found on the DB. GID: {0}, user ID: {1}", groupId, userId);
                response.Status = new Status() { Code = (int)eResponseStatus.Error, Message = eResponseStatus.Error.ToString() };
                return response;
            }

            // build ElasticSearch filter
            StringBuilder fullFilter = new StringBuilder();
            fullFilter.Append("(or ");
            foreach (DbAnnouncement announcement in dbAnnouncements)
            {
                //Elastic search should receive phrase + start date.
                fullFilter.Append(GetAnnouncementFilter(announcement, userNotificationData.Announcements.Where(userAnn => userAnn.AnnouncementId == announcement.ID).First().AddedDateSec));
            }
            fullFilter.Append(" )");

            // build unified search request
            UnifiedSearchRequest request = new UnifiedSearchRequest()
            {
                m_sSignature = NotificationUtils.GetSignature(CatalogSignString, CatalogSignatureKey),
                m_sSignString = CatalogSignString,
                filterQuery = fullFilter.ToString(),
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
                log.ErrorFormat("elastic search did not find any results. GID: {0}, user ID: {1}, search object: {2}", groupId, userId, JsonConvert.SerializeObject(request));
                response.Status = new Status() { Code = (int)eResponseStatus.Error, Message = eResponseStatus.Error.ToString() };
                return response;
            }

            if (unifiedSearchResponse.searchResults.Count == 0)
            {
                log.DebugFormat("elastic search did not find any results. GID: {0}, user ID: {1}, search Query: {2}", groupId, userId, fullFilter.ToString());
                response.Status = new Status() { Code = (int)eResponseStatus.OK, Message = eResponseStatus.OK.ToString() };
                return response;
            }

            mediaIds = new List<int>();
            mediaIds.AddRange(unifiedSearchResponse.searchResults.Select((item => int.Parse(item.AssetId))));
            response.Ids = mediaIds;
            response.TotalCount = unifiedSearchResponse.m_nTotalItems;

            return response;
        }

        private static string GetAnnouncementFilter(DbAnnouncement announcement, long addedDateSec)
        {
            long startDate = GetStartDate(addedDateSec);
            string filter = string.Format(" (and {0} start_date >= '{1}') ", announcement.FollowPhrase.ToLower(), startDate);
            return filter;
        }

        private static long GetStartDate(long addedDateSec)
        {
            long startDate = 0;
            long nowMinusTtl = GetPersonalizedFeedTtlDaysInSec();
            //Start date is determined as followed:
            //SF - this is the date the user started to follow the series.
            //SF < Now- TTL -> phrase + SF
            //SF > Now - TTL ->phrase + (Now - TTL) 
            startDate = (addedDateSec) > nowMinusTtl ? addedDateSec : nowMinusTtl;
            return startDate;

        }
        private static long GetPersonalizedFeedTtlDaysInSec()
        {
            int personalizedFeedTtlDay = 0;
            string res = TVinciShared.WS_Utils.GetTcmConfigValue("PersonalizedFeedTTLDays");

            if (!int.TryParse(res, out personalizedFeedTtlDay) || personalizedFeedTtlDay == 0)
            {
                throw new Exception("TCM value [PersonalizedFeedTTLDays] wasn't found");
            }

            return TVinciShared.DateUtils.DateTimeToUnixTimestamp(DateTime.UtcNow.AddDays(-personalizedFeedTtlDay));
        }

        public static string GetSeriesFollowPhrase(int groupId, string title)
        {
            // validate association tag
            string associationTag = Core.Notification.NotificationCache.Instance().GetEpisodeAssociationTagName(groupId);
            if (string.IsNullOrEmpty(associationTag))
            {
                log.ErrorFormat("Error getting follow series phrase - Association tag wasn't found. groupId: {0}, title: {1}", groupId, title);
                return null;
            }

            return string.Format("{0}='{1}'", associationTag, title);
        }

        public static string GetEpisodeAssociationTag(int groupId)
        {
            // validate association tag
            string associationTag = Core.Notification.NotificationCache.Instance().GetEpisodeAssociationTagName(groupId);
            if (string.IsNullOrEmpty(associationTag))
            {
                log.ErrorFormat("Error getting episode association tag - Association tag wasn't found. groupId: {0}", groupId);
            }

            return associationTag;
        }

        public static DateTime oldEpgSendDate { get; set; }
    }
}
