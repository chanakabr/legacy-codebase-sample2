using APILogic.AmazonSnsAdapter;
using ApiObjects;
using ApiObjects.Notification;
using ApiObjects.QueueObjects;
using ApiObjects.Response;
using Core.Notification;
using Core.Notification.Adapters;
using DAL;
using KLogMonitor;
using Newtonsoft.Json;
using QueueWrapper.Queues.QueueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Tvinci.Core.DAL;
using TVinciShared;
using Core.Notification;
using APILogic.AmazonSnsAdapter;
using Core.Notification.Adapters;
using ChannelsSchema;


namespace APILogic.Notification
{
    public class TopicInterestManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private const string USER_INTEREST_NOT_EXIST = "User interest not exist";
        private const string ROUTING_KEY_INTEREST_MESSAGES = "PROCESS_MESSAGE_INTERESTS";
        private const string NO_USER_INTEREST_TO_INSERT = "No user interest to insert";
        private const string META_ID_REQUIRED = "MetaId required";
        private const string META_VALUE_REQUIRED = "Meta value required";

        public static ApiObjects.Response.Status AddUserInterest(int partnerId, int userId, UserInterest userInterest)
        {
            Status response = null;
            UserInterests userInterests = null;
            bool updateIsNeeded = false;
            try
            {
                // 1. validate user                                
                response = ValidateUser(partnerId, userId);
                if (response == null || response.Code != (int)eResponseStatus.OK)
                {
                    log.ErrorFormat("Adding UserInterest failed: user: {0} not valid, partnerId: {1}, userInterest:{2}", userId, partnerId, JsonConvert.SerializeObject(userInterest));
                    return response;
                }

                // 2. validate with topic_interest make sure that parent and child valid
                List<Meta> groupTopics = CatalogDAL.GetTopicInterests(partnerId);
                response = ValidateUserInterest(userInterest, groupTopics);
                if (response == null || response.Code != (int)eResponseStatus.OK)
                {
                    log.ErrorFormat("Validate UserInterest failed: user: {0} not valid, partnerId: {1}, userInterest:{2}", userId, partnerId, JsonConvert.SerializeObject(userInterest));
                    return response;
                }

                Meta topicUserInterest = groupTopics.Where(x => x.MetaId == userInterest.MetaId).FirstOrDefault();
                if (topicUserInterest == null)
                {
                    log.ErrorFormat("UserInterest meta not recognize at group topic interest groupId = {0}, userInterest = {1}", partnerId, JsonConvert.SerializeObject(userInterest));
                    return new Status((int)eResponseStatus.NotaTopicInterestMeta, "Not a topic interest meta");
                }

                // 3. check that userInterest does not already exist 
                userInterests = InterestDal.GetUserInterest(partnerId, userId);
                if (userInterests == null)
                {
                    userInterests = new UserInterests() { PartnerId = partnerId, UserId = userId };
                    userInterest.Id = Guid.NewGuid().ToString();
                    userInterests.UserInterestList.Add(userInterest);
                    updateIsNeeded = true;
                }
                else
                {
                    foreach (var currentUserInterest in userInterests.UserInterestList)
                    {
                        if (currentUserInterest.Equals(userInterest))
                        {
                            return new ApiObjects.Response.Status() { Code = (int)eResponseStatus.UserInterestAlreadyExist, Message = "User interest already exist" };
                        }
                    }

                    // UserInterest should be added
                    userInterest.Id = Guid.NewGuid().ToString();
                    userInterests.UserInterestList.Add(userInterest);
                    updateIsNeeded = true;
                }

                // Set CB with new interest
                if (updateIsNeeded)
                {
                    if (!InterestDal.SetUserInterest(userInterests))
                        log.ErrorFormat("Error inserting user interest  into CB. User interest {0}", JsonConvert.SerializeObject(userInterests));
                }

                // 4. Send/Create Amazon Topic                
                if (NotificationSettings.IsPartnerPushEnabled(partnerId))
                {
                    // Send notification incase feature is ENABLED_NOTIFICATION                
                    if (topicUserInterest.Features.Contains(MetaFeatureType.ENABLED_NOTIFICATION))
                    {
                        // get interestNotification - according to userInterest
                        string topicNameValue = TopicInterestManager.GetInterestKeyValueName(topicUserInterest.Name, userInterest.Topic.Value);
                        InterestNotification interestNotification = InterestDal.GetTopicInterestNotificationsByTopicNameValue(partnerId, topicNameValue, topicUserInterest.AssetType);
                        if (interestNotification == null)
                            response = CreateInterestNotification(partnerId, userInterest, topicUserInterest, out interestNotification);

                        if (response.Code != (int)eResponseStatus.OK)
                            return response;

                        // create Amazon topic 
                        string externalId = string.Empty;
                        if (string.IsNullOrEmpty(interestNotification.ExternalPushId))
                        {
                            externalId = NotificationAdapter.CreateAnnouncement(partnerId, string.Format("Interest_{0}", topicNameValue));
                            if (string.IsNullOrEmpty(externalId))
                            {
                                log.DebugFormat("failed to create announcement groupID = {0}, topicNameValue = {1}", partnerId, topicNameValue);
                                return new ApiObjects.Response.Status((int)eResponseStatus.FailCreateAnnouncement, "fail create Amazon interest topic");
                            }

                            // update table with external id
                            interestNotification = InterestDal.UpdateTopicInterestNotification(partnerId, interestNotification.Id, externalId);
                            if (interestNotification == null)
                            {
                                log.DebugFormat("failed to update topic interest notification: {0} with externalId: {1}, groupId = {2}", interestNotification.Id, externalId, partnerId);
                                return new Status((int)eResponseStatus.FailCreateAnnouncement, "fail create Amazon interest topic"); //TODO: Anat change error code and message

                            }
                        }

                        // register user to topic
                        bool docExists = false;
                        UserNotification userNotificationData = NotificationDal.GetUserNotificationData(partnerId, userId, ref docExists);
                        if (userNotificationData == null)
                        {
                            if (docExists)
                            {
                                // error while getting user notification data
                                log.ErrorFormat("error retrieving user announcement data. GID: {0}, UID: {1}", partnerId, userId);
                                response = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                                return response;
                            }
                            else
                            {
                                log.DebugFormat("user announcement data wasn't found - going to create a new one. GID: {0}, UID: {1}", partnerId, userId);

                                // create user notification object
                                userNotificationData = new UserNotification(userId) { CreateDateSec = TVinciShared.DateUtils.UnixTimeStampNow() };

                                //update user settings according to partner settings configuration                    
                                userNotificationData.Settings.EnablePush = NotificationSettings.IsPartnerPushEnabled(partnerId, userId);
                            }
                        }

                        // create added time
                        long addedSecs = ODBCWrapper.Utils.DateTimeToUnixTimestampUtc(DateTime.UtcNow);

                        if (userNotificationData.devices != null && userNotificationData.devices.Count > 0)
                        {
                            foreach (UserDevice device in userNotificationData.devices)
                            {
                                string udid = device.Udid;
                                if (string.IsNullOrEmpty(udid))
                                {
                                    log.Error("device UDID is empty: " + device.Udid);
                                    continue;
                                }

                                log.DebugFormat("adding user interest to device group: {0}, user: {1}, UDID: {2}, interestNotificationId: {3}", partnerId, userId, udid, interestNotification.Id);

                                // get device notification data
                                docExists = false;
                                DeviceNotificationData deviceNotificationData = DAL.NotificationDal.GetDeviceNotificationData(partnerId, udid, ref docExists);
                                if (deviceNotificationData == null)
                                {
                                    log.ErrorFormat("device notification data not found group: {0}, UDID: {1}", partnerId, device.Udid);
                                    continue;
                                }

                                try
                                {
                                    // validate device doesn't already have the announcement
                                    var subscribedUserInterests = deviceNotificationData.SubscribedUserInterests.Where(x => x.Id == interestNotification.Id);
                                    if (subscribedUserInterests != null && subscribedUserInterests.Count() > 0)
                                    {
                                        log.ErrorFormat("user already subscribed on userInterests on device. group: {0}, UDID: {1}", partnerId, device.Udid);
                                        continue;
                                    }

                                    // get push data
                                    APILogic.DmsService.PushData pushData = PushAnnouncementsHelper.GetPushData(partnerId, udid, string.Empty);
                                    if (pushData == null)
                                    {
                                        log.ErrorFormat("push data not found. group: {0}, UDID: {1}", partnerId, device.Udid);
                                        continue;
                                    }

                                    // subscribe device to announcement
                                    AnnouncementSubscriptionData subData = new AnnouncementSubscriptionData()
                                    {
                                        EndPointArn = pushData.ExternalToken, // take from pushdata (with UDID)
                                        Protocol = EnumseDeliveryProtocol.application,
                                        TopicArn = interestNotification.ExternalPushId,
                                        ExternalId = interestNotification.Id
                                    };

                                    List<AnnouncementSubscriptionData> subs = new List<AnnouncementSubscriptionData>() { subData };
                                    subs = NotificationAdapter.SubscribeToAnnouncement(partnerId, subs);
                                    if (subs == null || subs.Count == 0 || string.IsNullOrEmpty(subs.First().SubscriptionArnResult))
                                    {
                                        log.ErrorFormat("Error registering device to announcement. group: {0}, UDID: {1}", partnerId, device.Udid);
                                        continue;
                                    }

                                    // update device notification object
                                    NotificationSubscription sub = new NotificationSubscription()
                                    {
                                        ExternalId = subs.First().SubscriptionArnResult,
                                        Id = interestNotification.Id,
                                        SubscribedAtSec = addedSecs
                                    };
                                    deviceNotificationData.SubscribedUserInterests.Add(sub);

                                    if (!DAL.NotificationDal.SetDeviceNotificationData(partnerId, udid, deviceNotificationData))
                                        log.ErrorFormat("error setting device notification data. group: {0}, UDID: {1}, topic: {2}", partnerId, device.Udid, subData.EndPointArn);
                                    else
                                    {
                                        log.DebugFormat("Successfully registered device to announcement. group: {0}, UDID: {1}, topic: {2}", partnerId, device.Udid, subData.EndPointArn);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    log.Error("Error in follow", ex);
                                }
                            }
                        }

                        // update user notification object
                        userNotificationData.UserInterests.Add(new Announcement()
                        {
                            AnnouncementId = interestNotification.Id,
                            AnnouncementName = topicNameValue,
                            AddedDateSec = addedSecs
                        });

                        if (!DAL.NotificationDal.SetUserNotificationData(partnerId, userId, userNotificationData))
                            log.ErrorFormat("error setting user notification data. group: {0}, user id: {1}", partnerId, userId);
                        else
                        {
                            log.DebugFormat("successfully updated user notification data. group: {0}, user id: {1}", partnerId, userId);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error inserting user interest  into CB. User interest {0}, exception {1} ", JsonConvert.SerializeObject(userInterests), ex);
            }

            return new ApiObjects.Response.Status() { Code = (int)eResponseStatus.OK, Message = eResponseStatus.OK.ToString() };
        }

        private static ApiObjects.Response.Status ValidateUser(int partnerId, int userId)
        {
            // validate user            

            try
            {
                Core.Users.UserResponseObject userResponseObject = Core.Users.Module.GetUserData(partnerId, userId.ToString(), string.Empty);

                // Make sure response is OK
                if (userResponseObject != null)
                {
                    if (userResponseObject.m_RespStatus == ResponseStatus.OK)
                    {
                        //check user suspend
                        if (userResponseObject.m_user.m_eSuspendState == DAL.DomainSuspentionStatus.Suspended)
                        {
                            return new ApiObjects.Response.Status() { Code = (int)eResponseStatus.UserSuspended, Message = eResponseStatus.UserSuspended.ToString() };
                        }
                    }
                    else
                        return new ApiObjects.Response.Status() { Code = (int)eResponseStatus.Error, Message = eResponseStatus.Error.ToString() };
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error when validating user {0} in group {1}. ex = {2}", userId, partnerId, ex);
                return new ApiObjects.Response.Status() { Code = (int)eResponseStatus.Error, Message = eResponseStatus.Error.ToString() };
            }

            return new ApiObjects.Response.Status() { Code = (int)eResponseStatus.OK, Message = eResponseStatus.OK.ToString() };
        }

        private static ApiObjects.Response.Status CreateInterestNotification(int partnerId, UserInterest userInterest, Meta groupTopicInterest, out InterestNotification interestNotification)
        {
            string topicNameValue = TopicInterestManager.GetInterestKeyValueName(groupTopicInterest.Name, userInterest.Topic.Value);
            interestNotification = InterestDal.GetTopicInterestNotificationsByTopicNameValue(partnerId, topicNameValue, groupTopicInterest.AssetType);

            if (interestNotification == null)
            {
                // select meesageTempalte
                MessageTemplateType messageTemplateType;
                switch (groupTopicInterest.AssetType)
                {
                    case eAssetTypes.EPG:
                        messageTemplateType = MessageTemplateType.InterestEPG;
                        break;
                    case eAssetTypes.MEDIA:
                        messageTemplateType = MessageTemplateType.InterestVod;
                        break;
                    case eAssetTypes.UNKNOWN:
                    case eAssetTypes.NPVR:
                    default:
                        log.ErrorFormat("Asset type is not recognized. TopicInterest : {0}", JsonConvert.SerializeObject(groupTopicInterest));
                        return new Status() { Code = (int)eResponseStatus.Error, Message = eResponseStatus.Error.ToString() };
                }

                interestNotification = InterestDal.InsertTopicInterestNotification(partnerId, groupTopicInterest.Name, string.Empty, messageTemplateType, topicNameValue, groupTopicInterest.MetaId
                    , groupTopicInterest.AssetType);

                if (interestNotification == null)
                {
                    log.ErrorFormat("Error to create DB interestNotification. TopicInterest : {0}", JsonConvert.SerializeObject(groupTopicInterest));
                    return new Status() { Code = (int)eResponseStatus.Error, Message = eResponseStatus.Error.ToString() };
                }
            }

            return new Status() { Code = (int)eResponseStatus.OK, Message = eResponseStatus.OK.ToString() };
        }

        private static Status ValidateUserInterest(UserInterest userInterest, List<Meta> groupsTopics)
        {
            if (userInterest == null)
            {
                return new Status() { Code = (int)eResponseStatus.NoUserInterestToInsert, Message = NO_USER_INTEREST_TO_INSERT };
            }

            if (!string.IsNullOrEmpty(userInterest.MetaId))
            {
                log.ErrorFormat("Error ValidateUserInterest. MetaIdRequired :{0}", JsonConvert.SerializeObject(userInterest));
                return new Status() { Code = (int)eResponseStatus.MetaIdRequired, Message = META_ID_REQUIRED };
            }

            if (userInterest.Topic == null || string.IsNullOrEmpty(userInterest.Topic.Value))
            {
                log.ErrorFormat("Error ValidateUserInterest. MetaValueRequired :{0}", JsonConvert.SerializeObject(userInterest));
                return new Status() { Code = (int)eResponseStatus.MetaValueRequired, Message = META_VALUE_REQUIRED };
            }

            if (userInterest.Topic.ParentTopic != null || string.IsNullOrEmpty(userInterest.Topic.ParentTopic.Value))
            {
                log.ErrorFormat("Error ValidateUserInterest. ParentTopic MetaValueRequired :{0}", JsonConvert.SerializeObject(userInterest));
                return new Status() { Code = (int)eResponseStatus.MetaValueRequired, Message = META_VALUE_REQUIRED };
            }

            Meta topicUserInterest = groupsTopics.Where(x => x.MetaId == userInterest.MetaId).FirstOrDefault();
            if (topicUserInterest == null)
            {
                log.ErrorFormat("Error topic not recognized at UserInterest. userInterest :{0}", JsonConvert.SerializeObject(userInterest));
                return new Status() { Code = (int)eResponseStatus.MetaValueRequired, Message = META_VALUE_REQUIRED };
            }

            return ValidateTopicUserInterest(userInterest.Topic, topicUserInterest, groupsTopics);
        }

        private static Status ValidateTopicUserInterest(UserInterestTopic userInterestTopic, Meta currentMeta, List<Meta> groupsTopics)
        {
            if (userInterestTopic.ParentTopic == null && currentMeta.ParentMetaId != null)
            {
                log.ErrorFormat("Error ParentTopic have no value but ParentMetaId does have value. userInterestTopic: {0}, currentMeta: {1}",
                    JsonConvert.SerializeObject(userInterestTopic), JsonConvert.SerializeObject(currentMeta));
                return new Status() { Code = (int)eResponseStatus.Error, Message = eResponseStatus.Error.ToString() };
            }

            if (userInterestTopic.ParentTopic != null && currentMeta.ParentMetaId == null)
            {
                log.ErrorFormat("Error ParentTopic have value but ParentMetaId does have no value. userInterestTopic: {0}, currentMeta: {1}",
                    JsonConvert.SerializeObject(userInterestTopic), JsonConvert.SerializeObject(currentMeta));
                return new Status() { Code = (int)eResponseStatus.Error, Message = eResponseStatus.Error.ToString() };
            }

            if (userInterestTopic.ParentTopic == null && currentMeta.ParentMetaId == null)
                return new Status() { Code = (int)eResponseStatus.OK, Message = eResponseStatus.OK.ToString() };

            Meta parentMeta = groupsTopics.Where(x => x.MetaId == currentMeta.ParentMetaId).FirstOrDefault();
            if (parentMeta == null)
            {
                log.ErrorFormat("Error ParentTopic have value but ParentMetaId not found. userInterestTopic: {0}, currentMeta: {1}",
                    JsonConvert.SerializeObject(userInterestTopic), JsonConvert.SerializeObject(currentMeta));
                return new Status() { Code = (int)eResponseStatus.Error, Message = eResponseStatus.Error.ToString() };
            }
            return ValidateTopicUserInterest(userInterestTopic.ParentTopic, parentMeta, groupsTopics);
        }

        internal static UserInterestResponseList GetUserInterests(int groupId, int userId)
        {
            UserInterestResponseList response = new UserInterestResponseList() { Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString()) };
            UserInterests userInterests = null;

            try
            {
                // insert user interest to CB
                userInterests = InterestDal.GetUserInterest(groupId, userId);
                if (userInterests == null)
                    log.ErrorFormat("Error inserting user interest  into CB. User interest {0}", JsonConvert.SerializeObject(userInterests));

                response.Status.Code = (int)eResponseStatus.OK;
                response.Status.Message = eResponseStatus.OK.ToString();
                response.UserInterests = userInterests.UserInterestList;
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error inserting user interest  into CB. User interest {0}, exception {1} ", JsonConvert.SerializeObject(userInterests), ex);
            }

            return response;
        }

        internal static Status DeleteUserInterest(int partnerId, int userId, string id)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status();

            UserInterests userInterests = null;
            bool updateIsNeeded = false;
            try
            {
                // Get user interests 
                userInterests = InterestDal.GetUserInterest(partnerId, userId);
                if (userInterests == null)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.UserInterestNotExist, USER_INTEREST_NOT_EXIST);
                    return response;
                }
                else
                {
                    foreach (var UserInterestItem in userInterests.UserInterestList)
                    {
                        if (UserInterestItem.Id == id)
                        {
                            userInterests.UserInterestList.Remove(UserInterestItem);
                            updateIsNeeded = true;
                            break;
                        }
                    }
                }

                // Set CB with new interest
                if (updateIsNeeded)
                {
                    if (!InterestDal.SetUserInterest(userInterests))
                        log.ErrorFormat("Error inserting user interest  into CB. User interest {0}", JsonConvert.SerializeObject(userInterests));
                }
                else
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.UserInterestNotExist, USER_INTEREST_NOT_EXIST);
                    return response;
                }

                response.Code = (int)eResponseStatus.OK;
                response.Message = eResponseStatus.OK.ToString();
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error inserting user interest  into CB. User interest {0}, exception {1} ", JsonConvert.SerializeObject(userInterests), ex);
            }

            return response;
        }

        public static string GetInterestKeyValueName(string key, string value)
        {
            return string.Format("{0}_{1}", key, value);
        }

        public static bool AddInterestToQueue(int groupId, InterestNotificationMessage interestNotificationMessage)
        {
            MessageInterestQueue que = new MessageInterestQueue();
            MessageInterestData messageReminderData = new MessageInterestData(groupId, DateUtils.DateTimeToUnixTimestamp(interestNotificationMessage.SendTime), interestNotificationMessage.Id)
            {
                ETA = interestNotificationMessage.SendTime
            };

            bool res = que.Enqueue(messageReminderData, ROUTING_KEY_INTEREST_MESSAGES);

            if (res)
                log.DebugFormat("Successfully inserted interest notification message to interest queue: {0}", messageReminderData);
            else
                log.ErrorFormat("Error while inserting interest notification message to queue. Message: {0}", messageReminderData);

            return res;
        }

        private static bool PushToWeb(int partnerId, int interestMessageId, string routeName, MessageData messageData, long sendTime)
        {
            bool isSent = false;
            if (string.IsNullOrEmpty(routeName))
            {
                log.DebugFormat("no queues found for push to web. interest message ID: {0}", interestMessageId);
                return isSent;
            }

            // enqueue message with small expiration date
            MessageAnnouncementFullData data = new MessageAnnouncementFullData(partnerId, messageData.Alert, messageData.Url, messageData.Sound, messageData.Category, sendTime);
            GeneralDynamicQueue q = new GeneralDynamicQueue(routeName, QueueWrapper.Enums.ConfigType.PushNotifications);
            if (!q.Enqueue(data, routeName, AnnouncementManager.PUSH_MESSAGE_EXPIRATION_MILLI_SEC))
            {
                log.ErrorFormat("Failed to insert push interest message to web push queue. reminder ID: {0}, route name: {1}",
                    interestMessageId,
                    routeName);
            }
            else
            {
                log.DebugFormat("Successfully inserted push interest message to web push queue data: {0}, reminder ID: {1}, route name: {2}",
                 JsonConvert.SerializeObject(data),
                 interestMessageId,
                 routeName);

                isSent = true;
            }

            return isSent;
        }

        internal static bool SendMessageInterest(int partnerId, long startTime, int notificationInterestMessageId)
        {
            // get partner notifications settings
            var partnerSettings = NotificationSettings.GetPartnerNotificationSettings(partnerId);
            if (partnerSettings == null && partnerSettings.settings != null)
            {
                log.ErrorFormat("Could not find partner notification settings. Partner ID: {0}", partnerId);
                return false;
            }

            // get interest message
            InterestNotificationMessage interestNotificationMessage = InterestDal.GetTopicInterestsNotificationMessageById(partnerId, notificationInterestMessageId);
            if (interestNotificationMessage == null)
            {
                log.ErrorFormat("could not find interest notification message. partner ID: {0}, start time: {1}, interest message ID: {2}", partnerId, startTime, notificationInterestMessageId);
                return false;
            }

            // get interest 
            InterestNotification interestNotification = InterestDal.GetTopicInterestNotificationsById(partnerId, interestNotificationMessage.TopicInterestsNotificationsId);
            if (interestNotification == null)
            {
                log.ErrorFormat("could not find interest notification. partner ID: {0}, start time: {1}, interest message ID: {2}", partnerId, startTime, notificationInterestMessageId);
                return false;
            }

            switch (interestNotification.AssetType)
            {
                case eAssetTypes.EPG:
                    return SendMessageInterestEpg(partnerId, startTime, interestNotification, interestNotificationMessage, partnerSettings);

                case eAssetTypes.MEDIA:
                    return SendMessageInterestVod(partnerId, startTime, interestNotification, interestNotificationMessage, partnerSettings);

                case eAssetTypes.UNKNOWN:
                case eAssetTypes.NPVR:
                default:

                    log.ErrorFormat("Interest message asset type was not implemented. partner ID: {0}, interest message ID: {1}, asset type: {2}", partnerId, startTime, notificationInterestMessageId, interestNotification.AssetType.ToString());
                    return true;
            }
        }

        internal static bool SendMessageInterestEpg(int partnerId, long startTime, InterestNotification interestNotification, InterestNotificationMessage interestNotificationMessage, NotificationPartnerSettingsResponse partnerSettings)
        {
            // get EPG program
            Core.Catalog.ProgramObj program = null;
            var status = AnnouncementManager.GetEpgProgram(partnerId, interestNotificationMessage.ReferenceAssetId, out program);
            if (status.Code != (int)eResponseStatus.OK)
            {
                log.ErrorFormat("program was not found. partner ID: {0}, start time: {1}, interest message ID: {2}, programId = ", partnerId, startTime, interestNotificationMessage.Id, interestNotificationMessage.ReferenceAssetId);
                return false;
            }

            // get media
            Core.Catalog.Response.MediaObj mediaChannel = null;
            string seriesName = string.Empty;
            status = AnnouncementManager.GetMedia(partnerId, (int)program.m_oProgram.LINEAR_MEDIA_ID, out mediaChannel, out seriesName);
            if (status.Code != (int)eResponseStatus.OK)
            {
                log.ErrorFormat("linear media for channel was not found. partner ID: {0}, start time: {1}, interest message ID: {2}, linear media Id= ", partnerId, startTime, interestNotificationMessage.Id, program.m_oProgram.LINEAR_MEDIA_ID);
                return false;
            }

            // Parse start date
            DateTime interestSendDate;
            if (!DateTime.TryParseExact(program.m_oProgram.START_DATE, AnnouncementManager.EPG_DATETIME_FORMAT, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out interestSendDate))
            {
                log.ErrorFormat("Failed parsing EPG start date for EPG notification event, epgID: {0}, startDate: {1}", program.m_oProgram.EPG_ID, program.m_oProgram.START_DATE);
                return false;
            }

            // get reminder pre-padding
            double prePadding = 0;
            if (partnerSettings.settings.RemindersPrePaddingSec.HasValue)
                prePadding = (double)partnerSettings.settings.RemindersPrePaddingSec;

            // validate program did not passed (10 min threshold)
            DateTime currentDate = DateTime.UtcNow;
            if (currentDate.AddMinutes(5) < interestSendDate.AddSeconds(-prePadding) ||
               (currentDate.AddMinutes(-5) > interestSendDate.AddSeconds(-prePadding)))
            {
                log.ErrorFormat("Program date passed. interest ID: {0}, current date: {1}, startDate: {2}", interestNotificationMessage.Id, currentDate, interestSendDate);
                return false;
            }

            // validate send time is same as send time in DB
            if (Math.Abs(DateUtils.DateTimeToUnixTimestamp(interestNotificationMessage.SendTime) - startTime) > 5)
            {
                log.ErrorFormat("Message sending time is not the same as DB reminder send date. program ID: {0}, message send date: {1}, DB send date: {2}",
                    interestNotificationMessage.Id,
                    DateUtils.UnixTimeStampToDateTime(Convert.ToInt64(startTime)),
                    interestNotificationMessage.SendTime);
                return false;
            }

            // send push messages
            if (NotificationSettings.IsPartnerPushEnabled(partnerId))
            {
                // get message templates
                MessageTemplate template = null;
                List<MessageTemplate> messageTemplates = NotificationCache.Instance().GetMessageTemplates(partnerId);
                if (messageTemplates == null)
                {
                    log.ErrorFormat("message templates were not found for partnerId = {0}", partnerId);
                    return false;
                }

                template = messageTemplates.FirstOrDefault(x => x.TemplateType == MessageTemplateType.InterestEPG);
                if (template == null)
                {
                    log.ErrorFormat("reminder message template was not found. group: {0}", partnerId);
                    return false;
                }

                // build message 
                MessageData messageData = new MessageData()
                {
                    Category = template.Action,
                    Sound = template.Sound,
                    Url = template.URL.Replace("{" + eReminderPlaceHolders.StartDate + "}", interestSendDate.ToString(template.DateFormat)).
                                                         Replace("{" + eReminderPlaceHolders.ProgramId + "}", program.m_oProgram.EPG_ID.ToString()).
                                                         Replace("{" + eReminderPlaceHolders.ProgramName + "}", program.m_oProgram.NAME).
                                                         Replace("{" + eReminderPlaceHolders.ChannelName + "}", mediaChannel != null && mediaChannel.m_sName != null ? mediaChannel.m_sName : string.Empty),
                    Alert = template.Message.Replace("{" + eReminderPlaceHolders.StartDate + "}", interestSendDate.ToString(template.DateFormat)).
                                                           Replace("{" + eReminderPlaceHolders.ProgramId + "}", program.m_oProgram.EPG_ID.ToString()).
                                                           Replace("{" + eReminderPlaceHolders.ProgramName + "}", program.m_oProgram.NAME).
                                                           Replace("{" + eReminderPlaceHolders.ChannelName + "}", mediaChannel != null && mediaChannel.m_sName != null ? mediaChannel.m_sName : string.Empty)
                };

                // send to Amazon
                if (string.IsNullOrEmpty(interestNotification.ExternalPushId))
                {
                    log.ErrorFormat("External push ID wasn't found. interest message: {0}", JsonConvert.SerializeObject(interestNotificationMessage));
                    return false;
                }

                // update message reminder
                interestNotificationMessage.Message = JsonConvert.SerializeObject(messageData);

                string resultMsgId = NotificationAdapter.PublishToAnnouncement(partnerId, interestNotification.ExternalPushId, string.Empty, messageData);
                if (string.IsNullOrEmpty(resultMsgId))
                    log.ErrorFormat("failed to publish interest message to push topic. result message id is empty for reminder {0}", interestNotificationMessage.Id);
                else
                {
                    log.DebugFormat("Successfully sent interest message. interest message Id: {0}", interestNotificationMessage.Id);

                    // update external push result
                    InterestNotificationMessage updatedInterestNotificationMessage = DAL.InterestDal.UpdateTopicInterestNotificationMessage(partnerId, interestNotificationMessage.Id, null, interestNotificationMessage.Message, true, resultMsgId, currentDate);
                    if (updatedInterestNotificationMessage == null)
                    {
                        log.ErrorFormat("Failed to update interest message. partner ID: {0}, reminder ID: {1} ", partnerId, interestNotificationMessage.Id);
                    }

                    // update interest notification 
                    InterestNotification updatedInterestNotification = DAL.InterestDal.UpdateTopicInterestNotification(partnerId, interestNotification.Id, null, currentDate);
                    if (updatedInterestNotification == null)
                    {
                        log.ErrorFormat("Failed to update interest notification last send date. partner ID: {0}, reminder ID: {1} ", partnerId, interestNotificationMessage.Id);
                    }
                }

                // send to push web - rabbit.                
                PushToWeb(partnerId, interestNotificationMessage.Id, interestNotification.QueueName, messageData, DateUtils.DateTimeToUnixTimestamp(interestNotificationMessage.SendTime));
               
            }
            return true;
        }

        internal static bool SendMessageInterestVod(int partnerId, long startTime, InterestNotification interestNotification, InterestNotificationMessage interestNotificationMessage, NotificationPartnerSettingsResponse partnerSettings)
        {
            // get media
            Core.Catalog.Response.MediaObj vodAsset = null;
            string seriesName = string.Empty;
            Status status = AnnouncementManager.GetMedia(partnerId, (int)interestNotificationMessage.ReferenceAssetId, out vodAsset, out seriesName);
            if (status.Code != (int)eResponseStatus.OK)
            {
                log.ErrorFormat("interest VOD asset was not found. partner ID: {0}, start time: {1}, interest message ID: {2}", partnerId, startTime, interestNotificationMessage.Id);
                return false;
            }

            DateTime interestSendDate = vodAsset.m_dCatalogStartDate;

            // validate program did not passed (10 min threshold)
            DateTime currentDate = DateTime.UtcNow;
            if (currentDate.AddMinutes(5) < interestSendDate ||
               (currentDate.AddMinutes(-5) > interestSendDate))
            {
                log.ErrorFormat("VOD date passed. interest ID: {0}, current date: {1}, startDate: {2}", interestNotificationMessage.Id, currentDate, interestSendDate);
                return false;
            }

            // validate send time is same as send time in DB
            if (Math.Abs(DateUtils.DateTimeToUnixTimestamp(interestNotificationMessage.SendTime) - startTime) > 5)
            {
                log.ErrorFormat("Message sending time is not the same as DB interest send date. asset ID: {0}, message send date: {1}, DB send date: {2}",
                    interestNotificationMessage.Id,
                    DateUtils.UnixTimeStampToDateTime(Convert.ToInt64(startTime)),
                    interestNotificationMessage.SendTime);
                return false;
            }

            // send push messages
            if (NotificationSettings.IsPartnerPushEnabled(partnerId))
            {
                // get message templates
                MessageTemplate template = null;
                List<MessageTemplate> messageTemplates = NotificationCache.Instance().GetMessageTemplates(partnerId);
                if (messageTemplates == null)
                {
                    log.ErrorFormat("message templates were not found for partnerId = {0}", partnerId);
                    return false;
                }

                template = messageTemplates.FirstOrDefault(x => x.TemplateType == MessageTemplateType.InterestVod);
                if (template == null)
                {
                    log.ErrorFormat("reminder message template was not found. group: {0}", partnerId);
                    return false;
                }

                // build message 
                MessageData messageData = new MessageData()
                {
                    Category = template.Action,
                    Sound = template.Sound,
                    Url = template.URL.Replace("{" + eFollowSeriesPlaceHolders.CatalaogStartDate + "}", vodAsset.m_dCatalogStartDate.ToString(template.DateFormat)).
                                                            Replace("{" + eFollowSeriesPlaceHolders.MediaId + "}", interestNotificationMessage.ReferenceAssetId.ToString()).
                                                            Replace("{" + eFollowSeriesPlaceHolders.MediaName + "}", vodAsset.m_sName).
                                                            Replace("{" + eFollowSeriesPlaceHolders.SeriesName + "}", seriesName).
                                                            Replace("{" + eFollowSeriesPlaceHolders.StartDate + "}", vodAsset.m_dStartDate.ToString(template.DateFormat)),
                    Alert = template.Message.Replace("{" + eFollowSeriesPlaceHolders.CatalaogStartDate + "}", vodAsset.m_dCatalogStartDate.ToString(template.DateFormat)).
                                                          Replace("{" + eFollowSeriesPlaceHolders.MediaId + "}", interestNotificationMessage.ReferenceAssetId.ToString()).
                                                          Replace("{" + eFollowSeriesPlaceHolders.MediaName + "}", vodAsset.m_sName).
                                                          Replace("{" + eFollowSeriesPlaceHolders.SeriesName + "}", seriesName).
                                                          Replace("{" + eFollowSeriesPlaceHolders.StartDate + "}", vodAsset.m_dStartDate.ToString(template.DateFormat))
                };

                // send to Amazon
                if (string.IsNullOrEmpty(interestNotification.ExternalPushId))
                {
                    log.ErrorFormat("External push ID wasn't found. interest message: {0}", JsonConvert.SerializeObject(interestNotificationMessage));
                    return false;
                }

                // update message reminder
                interestNotificationMessage.Message = JsonConvert.SerializeObject(messageData);

                string resultMsgId = NotificationAdapter.PublishToAnnouncement(partnerId, interestNotification.ExternalPushId, string.Empty, messageData);
                if (string.IsNullOrEmpty(resultMsgId))
                    log.ErrorFormat("failed to publish interest message to push topic. result message id is empty for reminder {0}", interestNotificationMessage.Id);
                else
                {
                    log.DebugFormat("Successfully sent interest message. interest message Id: {0}", interestNotificationMessage.Id);

                    // update external push result
                    InterestNotificationMessage updatedInterestNotificationMessage = DAL.InterestDal.UpdateTopicInterestNotificationMessage(partnerId, interestNotificationMessage.Id, null, interestNotificationMessage.Message, true, resultMsgId, DateTime.UtcNow);
                    if (updatedInterestNotificationMessage == null)
                    {
                        log.ErrorFormat("Failed to update interest message. partner ID: {0}, reminder ID: {1} ", partnerId, interestNotificationMessage.Id);
                    }
                }

                // send to push web - rabbit.                
                PushToWeb(partnerId, interestNotificationMessage.Id, interestNotification.QueueName, messageData, DateUtils.DateTimeToUnixTimestamp(interestNotificationMessage.SendTime));

                // send inbox messages
                if (NotificationSettings.IsPartnerInboxEnabled(partnerId))
                {
                    List<int> users = InterestDal.GetUsersListbyInterestId(partnerId, interestNotification.Id);
                    if (users != null)
                    {
                        foreach (var userId in users)
                        {
                            InboxMessage inboxMessage = new InboxMessage()
                            {
                                Category = eMessageCategory.Followed,
                                CreatedAtSec = DateUtils.DateTimeToUnixTimestamp(currentDate),
                                Id = Guid.NewGuid().ToString(),
                                Message = messageData.Alert,
                                State = eMessageState.Unread,
                                UpdatedAtSec = DateUtils.DateTimeToUnixTimestamp(currentDate),
                                Url = messageData.Url,
                                UserId = userId
                            };

                            if (!NotificationDal.SetUserInboxMessage(partnerId, inboxMessage, NotificationSettings.GetInboxMessageTTLDays(partnerId)))
                            {
                                log.ErrorFormat("Error while setting user interest inbox message. GID: {0}, InboxMessage: {1}",
                                    partnerId,
                                    JsonConvert.SerializeObject(inboxMessage));
                            }
                        }
                    }
                }
            }
            return true;
        }
    }
}
