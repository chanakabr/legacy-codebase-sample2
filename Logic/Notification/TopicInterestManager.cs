using APILogic.AmazonSnsAdapter;
using ApiObjects;
using ApiObjects.Notification;
using ApiObjects.QueueObjects;
using ApiObjects.Response;
using ChannelsSchema;
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
using TVinciShared;


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
        private const string TOPIC_NOT_FOUND = "Topic not found";
        private static string PARTNER_TOPIC_INTEREST_IS_MISSING = "Partner topic interest is missing";
        private static string PARENT_TOPIC_IS_REQUIRED = "Meta defined with parentMeta but ParentTopic have no value";
        private static string PARENT_TOPIC_SHOULD_NOT_HAVE_VALUE = " ParentTopic have value but Meta defined without parentMeta";
        private static string PARENT_TOPIC_META_ID_NOT_EQUAL_TO_META_PARENT_META_ID = "ParentTopic metaId not equal to Meta parentMeta";
        private static string PARENT_TOPIC_VALUE_IS_MISSING = "Parent topic value is missing";
        private static string PARENT_ID_NOT_A_USER_INTERSET = "Parent meta id should be recognized as user interest";

        public static ApiObjects.Response.Status AddUserInterest(int partnerId, int userId, UserInterest newUserInterest)
        {
            List<KeyValuePair> interestsToCancel = new List<KeyValuePair>();
            Status response = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            try
            {
                // validate user                                
                response = ValidateUser(partnerId, userId);
                if (response == null || response.Code != (int)eResponseStatus.OK)
                {
                    log.ErrorFormat("Adding user interest failed. User not valid. User ID: {0}, partnerId: {1}", userId, partnerId);
                    return response;
                }

                // get partner topics
                var partnerTopicInterests = NotificationCache.Instance().GetPartnerTopicInterests(partnerId);
                if (partnerTopicInterests == null || partnerTopicInterests.Count == 0)
                {
                    log.ErrorFormat("Error getting partner topic interests. User ID: {0}, Partner ID: {1}", userId, partnerId);
                    return new ApiObjects.Response.Status((int)eResponseStatus.PartnerTopicInterestIsMissing, PARTNER_TOPIC_INTEREST_IS_MISSING);
                }

                // get user interests
                UserInterests userInterests = InterestDal.GetUserInterest(partnerId, userId);

                // validate new topic is legal
                response = ValidateNewUserInterest(newUserInterest, userInterests, partnerTopicInterests);
                if (response == null || response.Code != (int)eResponseStatus.OK)
                {
                    log.ErrorFormat("Validation of new user interest failed. User ID: {0}, Partner ID: {1}, User Interest: {2}", userId, partnerId, JsonConvert.SerializeObject(newUserInterest));
                    return response;
                }

                bool isRegistrationNeeded = true;
                if (userInterests == null)
                {
                    // first user interest
                    userInterests = new UserInterests() { PartnerId = partnerId, UserId = userId };
                    newUserInterest.UserInterestId = Guid.NewGuid().ToString();
                    userInterests.UserInterestList.Add(newUserInterest);
                }
                else
                {
                    // checking whether user already register to an upper topic level notification or if he should be removed from lower topic level notification
                    isRegistrationNeeded = IsNotificationRegistrationNeeded(newUserInterest, userInterests, partnerTopicInterests, out interestsToCancel);

                    // User interest should be added
                    newUserInterest.UserInterestId = Guid.NewGuid().ToString();
                    userInterests.UserInterestList.Add(newUserInterest);
                }

                // Set CB with new interest
                if (!InterestDal.SetUserInterest(userInterests))
                {
                    log.ErrorFormat("Error inserting user interest into CB. User interest {0}", JsonConvert.SerializeObject(userInterests));
                    return new ApiObjects.Response.Status() { Code = (int)eResponseStatus.Error };
                }

                if (!isRegistrationNeeded)
                {
                    log.DebugFormat("Notification registration is not needed. group: {0}, user id: {1}", partnerId, userId);
                    return new ApiObjects.Response.Status() { Code = (int)eResponseStatus.OK, Message = eResponseStatus.OK.ToString() };
                }

                // get relevant topic 
                Meta topicInterest = partnerTopicInterests.Where(x => x.Id == newUserInterest.Topic.MetaId).FirstOrDefault();

                // get interestNotification
                string topicNameValue = TopicInterestManager.GetInterestKeyValueName(topicInterest.Name, newUserInterest.Topic.Value);
                InterestNotification interestNotification = InterestDal.GetTopicInterestNotificationsByTopicNameValue(partnerId, topicNameValue, topicInterest.AssetType);
                if (interestNotification == null)
                {
                    // create notification in DB
                    response = CreateInterestNotification(partnerId, newUserInterest, topicInterest, out interestNotification);
                    if (response.Code != (int)eResponseStatus.OK)
                        return response;
                }

                // update user-interest mapping (for inbox purposes)
                if (topicInterest.AssetType == eAssetTypes.MEDIA)
                {
                    if (!InterestDal.SetUserInterestMapping(partnerId, userId, interestNotification.Id))
                    {
                        log.DebugFormat("Error. set userInterst mapping. group: {0}, user id: {1}", partnerId, userId); 
                        return new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                    }
                }

                // validate push is enabled
                if (!NotificationSettings.IsPartnerPushEnabled(partnerId))
                {
                    log.DebugFormat("Success. adding userInterst PartnerPushEnabled = false. group: {0}, user id: {1}", partnerId, userId);
                    return new ApiObjects.Response.Status() { Code = (int)eResponseStatus.OK, Message = eResponseStatus.OK.ToString() };
                }

                // register user (devices) to Amazon topic interests
                response = RegisterUserToInterestNotification(partnerId, userId, interestNotification);
                if (response == null || response.Code != (int)eResponseStatus.OK)
                {
                    log.ErrorFormat("Registering user to notifications failed. User ID: {0}, Partner ID: {1}, User Interest: {2}", userId, partnerId, JsonConvert.SerializeObject(newUserInterest));
                    return response;
                }

                // get all notification interest to cancel
                List<InterestNotification> interestsNotificationToCancel = new List<InterestNotification>();
                foreach (var interestToCancel in interestsToCancel)
                {
                    string notificationKeyValue = GetInterestKeyValueName(interestToCancel.key, interestToCancel.value);
                    InterestNotification interestNotificationToCancel = InterestDal.GetTopicInterestNotificationsByTopicNameValue(partnerId, notificationKeyValue, topicInterest.AssetType);
                    if (interestNotificationToCancel == null)
                        log.ErrorFormat("Could not find topic notification to cancel. notificationKeyValue: {0}, type: {1}", notificationKeyValue, topicInterest.AssetType.ToString());
                    else
                        interestsNotificationToCancel.Add(interestNotificationToCancel);
                }

                if (interestsNotificationToCancel.Count == 0)
                {
                    // remove user mapping
                    if (topicInterest.AssetType == eAssetTypes.MEDIA)
                    {
                        foreach (var interestNotificationToCancel in interestsNotificationToCancel)
                        {
                            if (!InterestDal.RemoveUserInterestMapping(partnerId, userId, interestNotificationToCancel.Id))
                                log.ErrorFormat("Error un-mapping interest to user. User ID: {0}, interest ID: {1}", userId, interestNotificationToCancel.Id);
                        }
                    }

                    // un-register user (devices) to Amazon topic interests
                    response = UnRegisterUserToInterestNotifications(partnerId, userId, interestsNotificationToCancel);
                    if (response == null || response.Code != (int)eResponseStatus.OK)
                    {
                        log.ErrorFormat("Unregistering user to notifications failed. User ID: {0}, Partner ID: {1}, User Interest to cancel: {2}", userId, partnerId, JsonConvert.SerializeObject(interestsNotificationToCancel));
                        return response;
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error inserting user interest  into CB. New user interest: {0}, exception {1}", JsonConvert.SerializeObject(newUserInterest), ex);
            }

            return new ApiObjects.Response.Status() { Code = (int)eResponseStatus.OK, Message = eResponseStatus.OK.ToString() };
        }

        private static Status UnRegisterUserToInterestNotifications(int partnerId, int userId, List<InterestNotification> interestsNotificationToCancel)
        {
            Status response = new ApiObjects.Response.Status() { Code = (int)eResponseStatus.Error, Message = eResponseStatus.Error.ToString() };
            List<UnSubscribe> unsubscribeList = new List<AmazonSnsAdapter.UnSubscribe>();

            // register user to topic
            bool docExists = false;
            UserNotification userNotificationData = NotificationDal.GetUserNotificationData(partnerId, userId, ref docExists);
            if (userNotificationData == null)
            {
                // error while getting user notification data
                log.ErrorFormat("error retrieving user announcement data. GID: {0}, UID: {1}", partnerId, userId);
                response = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                return response;
            }

            foreach (var interestNotificationToCancel in interestsNotificationToCancel)
                userNotificationData.UserInterests.RemoveAll(x => x.AnnouncementId == interestNotificationToCancel.Id);

            // update user notification object
            if (!DAL.NotificationDal.SetUserNotificationData(partnerId, userId, userNotificationData))
            {
                log.ErrorFormat("error setting user notification data. group: {0}, user id: {1}", partnerId, userId);
                response = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            }
            else
                log.DebugFormat("successfully updated user notification data. group: {0}, user id: {1}, user data: {2}", partnerId, userId, JsonConvert.SerializeObject(userNotificationData));

            // update user's devices 
            if (userNotificationData.devices == null || userNotificationData.devices.Count == 0)
            {
                log.DebugFormat("User doesn't have any notification devices. User notification object: {0}", JsonConvert.SerializeObject(userNotificationData));
                response = new Status((int)eResponseStatus.OK);
                return response;
            }

            foreach (UserDevice device in userNotificationData.devices)
            {
                // get device notification data
                docExists = false;
                DeviceNotificationData deviceNotificationData = DAL.NotificationDal.GetDeviceNotificationData(partnerId, device.Udid, ref docExists);
                if (deviceNotificationData == null)
                {
                    log.ErrorFormat("device notification data not found group: {0}, UDID: {1}", partnerId, device.Udid);
                    continue;
                }

                // get push data
                APILogic.DmsService.PushData pushData = PushAnnouncementsHelper.GetPushData(partnerId, device.Udid, string.Empty);
                if (pushData == null)
                {
                    log.ErrorFormat("push data not found. group: {0}, UDID: {1}", partnerId, device.Udid);
                    continue;
                }

                foreach (var interestNotificationToCancel in interestsNotificationToCancel)
                {
                    if (string.IsNullOrEmpty(device.Udid))
                    {
                        log.Error("device UDID is empty: " + device.Udid);
                        continue;
                    }

                    try
                    {
                        // get device interest
                        var deviceInterest = deviceNotificationData.SubscribedUserInterests.FirstOrDefault(x => x.Id == interestNotificationToCancel.Id);
                        if (deviceInterest == null)
                        {
                            log.ErrorFormat("User interest to remove was not found on user device. group: {0}, UDID: {1}, interest to remove: {2}, device notification doc: {3}",
                                partnerId,
                                device.Udid,
                                interestNotificationToCancel.Id,
                                JsonConvert.SerializeObject(device));
                            continue;
                        }

                        // add to cancel list
                        unsubscribeList.Add(new UnSubscribe()
                        {
                            SubscriptionArn = deviceInterest.ExternalId,
                            ExternalId = interestNotificationToCancel.Id
                        });

                        deviceNotificationData.SubscribedUserInterests.RemoveAll(x => x.Id == interestNotificationToCancel.Id);
                    }
                    catch (Exception ex)
                    {
                        log.Error("Error in follow", ex);
                    }
                }

                // update device data
                if (!DAL.NotificationDal.SetDeviceNotificationData(partnerId, device.Udid, deviceNotificationData))
                {
                    log.ErrorFormat("error setting device notification data. group: {0}, UDID: {1}", partnerId, device.Udid);
                    response = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                }
                else
                    log.DebugFormat("Successfully updated device data. group: {0}, UDID: {1}, topic: {2}", partnerId, device.Udid, JsonConvert.SerializeObject(deviceNotificationData));
            }

            if (unsubscribeList != null && unsubscribeList.Count > 0)
            {
                List<UnSubscribe> unregisterResults = NotificationAdapter.UnSubscribeToAnnouncement(partnerId, unsubscribeList);
                if (unregisterResults == null)
                {
                    log.ErrorFormat("Error while trying to unregister devices. unsubscribeList: {0}", JsonConvert.SerializeObject(unsubscribeList));
                }

                foreach (var unregisterResult in unregisterResults)
                {
                    if (unregisterResult.Success)
                        log.DebugFormat("Successfully unregistered device from interest. user ID: {0}, interest ID", userId, unregisterResult.ExternalId);
                    else
                        log.ErrorFormat("Error unregistering device from interest. user ID: {0}, interest ID", userId, unregisterResult.ExternalId);
                }
            }
            response = new ApiObjects.Response.Status() { Code = (int)eResponseStatus.OK };
            return response;
        }

        private static Status RegisterUserToInterestNotification(int partnerId, int userId, InterestNotification interestNotification)
        {
            Status response = new ApiObjects.Response.Status() { Code = (int)eResponseStatus.Error, Message = eResponseStatus.Error.ToString() };

            // create Amazon topic 
            if (string.IsNullOrEmpty(interestNotification.ExternalPushId))
            {
                string externalId = NotificationAdapter.CreateAnnouncement(partnerId, string.Format("Interest_{0}_{1}", interestNotification.AssetType.ToString(), interestNotification.TopicNameValue));
                if (string.IsNullOrEmpty(externalId))
                {
                    log.DebugFormat("failed to create announcement groupID = {0}, topicNameValue = {1}", partnerId, interestNotification.TopicNameValue);
                    return new ApiObjects.Response.Status((int)eResponseStatus.FailCreateAnnouncement, "fail create Amazon interest topic");
                }

                // update table with external id
                interestNotification = InterestDal.UpdateTopicInterestNotification(partnerId, interestNotification.Id, externalId);
                if (interestNotification == null)
                {
                    log.DebugFormat("failed to update topic interest notification: {0} with externalId: {1}, groupId = {2}", interestNotification.Id, externalId, partnerId);
                    return new Status((int)eResponseStatus.Error, "Error"); 
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

            // update user notification object
            long addedSecs = ODBCWrapper.Utils.DateTimeToUnixTimestampUtc(DateTime.UtcNow);

            if (userNotificationData.UserInterests.FirstOrDefault(x => x.AnnouncementId == interestNotification.Id) != null)
                log.DebugFormat("User is already registered to topic.group: {0}, user id: {1}, interest ID: {2}", partnerId, userId, interestNotification.Id);
            else
            {
                userNotificationData.UserInterests.Add(new Announcement()
                {
                    AnnouncementId = interestNotification.Id,
                    AnnouncementName = interestNotification.Name,
                    AddedDateSec = addedSecs
                });

                if (!DAL.NotificationDal.SetUserNotificationData(partnerId, userId, userNotificationData))
                {
                    log.ErrorFormat("error setting user notification data. group: {0}, user id: {1}", partnerId, userId);
                    response = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                }
                else
                    log.DebugFormat("successfully updated user notification data. group: {0}, user id: {1}", partnerId, userId);
            }


            // update user's devices 
            if (userNotificationData.devices != null && userNotificationData.devices.Count > 0)
            {
                foreach (UserDevice device in userNotificationData.devices)
                {
                    if (string.IsNullOrEmpty(device.Udid))
                    {
                        log.Error("device UDID is empty: " + device.Udid);
                        continue;
                    }

                    log.DebugFormat("adding user interest to device group: {0}, user: {1}, UDID: {2}, interestNotificationId: {3}", partnerId, userId, device.Udid, interestNotification.Id);

                    // get device notification data
                    docExists = false;
                    DeviceNotificationData deviceNotificationData = DAL.NotificationDal.GetDeviceNotificationData(partnerId, device.Udid, ref docExists);
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
                        APILogic.DmsService.PushData pushData = PushAnnouncementsHelper.GetPushData(partnerId, device.Udid, string.Empty);
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

                        if (!DAL.NotificationDal.SetDeviceNotificationData(partnerId, device.Udid, deviceNotificationData))
                            log.ErrorFormat("error setting device notification data. group: {0}, UDID: {1}, topic: {2}", partnerId, device.Udid, subData.EndPointArn);
                        else
                            log.DebugFormat("Successfully registered device to announcement. group: {0}, UDID: {1}, topic: {2}", partnerId, device.Udid, subData.EndPointArn);
                    }
                    catch (Exception ex)
                    {
                        log.Error("Error in follow", ex);
                    }
                }
            }
            response = new ApiObjects.Response.Status() { Code = (int)eResponseStatus.OK };
            return response;
        }

        private static bool IsNotificationRegistrationNeeded(UserInterest newUserInterest, UserInterests userInterests, List<Meta> groupsTopics, out List<KeyValuePair> interestNotificationToCancel)
        {
            interestNotificationToCancel = new List<KeyValuePair>();

            // validate feature is ENABLED_NOTIFICATION   
            Meta topicInterest = groupsTopics.Where(x => x.Id == newUserInterest.Topic.MetaId).FirstOrDefault();
            if (!topicInterest.Features.Contains(MetaFeatureType.ENABLED_NOTIFICATION))
            {
                log.DebugFormat("Interest is not notification enabled - not adding notification");
                return false;
            }

            // get partner notification topics
            List<string> partnerMetasWithNotification = groupsTopics.Where(x => x.Features != null && x.Features.Contains(MetaFeatureType.ENABLED_NOTIFICATION)).Select(y => y.Id).ToList();
            if (partnerMetasWithNotification.Count == 0)
                return false;

            // get user interests leafs
            var userInterestsLeafs = userInterests.UserInterestList.Select(y => new KeyValuePair<string, string>(y.Topic.MetaId, y.Topic.Value)).ToList();

            // get new user interest values
            List<string> newUserInterestValues = new List<string>();
            GetNewUserInterestMetaIds(newUserInterest.Topic, ref newUserInterestValues);

            foreach (string userMetaValue in newUserInterestValues)
            {
                if (userInterestsLeafs.Exists(x => x.Value == userMetaValue))
                {
                    string metaId = userInterestsLeafs.First(x => x.Value == userMetaValue).Key;
                    if (partnerMetasWithNotification.Contains(metaId))
                    {
                        log.DebugFormat("Registration of notification is not needed since the user is already registered to a parent notification node. Parent node value: {0}, requested node: {1}",
                            userMetaValue,
                            JsonConvert.SerializeObject(newUserInterest));
                        return false;
                    }
                }
            }

            // iterate through all tree
            foreach (var interestLeaf in userInterests.UserInterestList)
            {
                // iterate through branch
                UserInterestTopic node = interestLeaf.Topic;

                Meta groupsTopic = groupsTopics.FirstOrDefault(x => x.Features != null && x.Features.Contains(MetaFeatureType.ENABLED_NOTIFICATION) && x.Id == node.MetaId);
                // validate leaf is notification enabled
                if (groupsTopic != null)
                {
                    KeyValuePair branchInterestToCancel = new KeyValuePair() { key = groupsTopic.Name, value = node.Value };

                    while (node != null)
                    {
                        if (node.Value.ToLower() == newUserInterest.Topic.Value.ToLower())
                        {
                            interestNotificationToCancel.Add(branchInterestToCancel);
                            break;
                        }

                        // go to parent node
                        node = node.ParentTopic;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Returns user interest values
        /// </summary>
        private static bool GetNewUserInterestMetaIds(UserInterestTopic userInterestTopic, ref List<string> newUserInterestValues)
        {
            if (userInterestTopic == null || string.IsNullOrEmpty(userInterestTopic.Value))
                return false;

            newUserInterestValues.Add(userInterestTopic.Value);

            return GetNewUserInterestMetaIds(userInterestTopic.ParentTopic, ref newUserInterestValues);
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
            interestNotification = null;
            string topicNameValue = TopicInterestManager.GetInterestKeyValueName(groupTopicInterest.Name, userInterest.Topic.Value);

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

            log.DebugFormat("Creating new notification interest. partner ID: {0}, name: {1}, messageTemplateType: {2}, topicNameValue: {3}",
                partnerId,
                groupTopicInterest.Name,
                messageTemplateType.ToString(),
                topicNameValue);

            interestNotification = InterestDal.InsertTopicInterestNotification(partnerId, groupTopicInterest.Name, string.Empty, messageTemplateType, topicNameValue,
                groupTopicInterest.Id,
                groupTopicInterest.AssetType);

            if (interestNotification == null)
            {
                log.ErrorFormat("Error to create DB interestNotification. TopicInterest : {0}", JsonConvert.SerializeObject(groupTopicInterest));
                return new Status() { Code = (int)eResponseStatus.Error, Message = eResponseStatus.Error.ToString() };
            }

            return new Status() { Code = (int)eResponseStatus.OK, Message = eResponseStatus.OK.ToString() };
        }

        private static Status ValidateNewUserInterest(UserInterest newUserInterest, UserInterests userInterests, List<Meta> groupsTopics)
        {
            if (newUserInterest == null)
            {
                return new Status() { Code = (int)eResponseStatus.NoUserInterestToInsert, Message = NO_USER_INTEREST_TO_INSERT };
            }

            // validate user interest doesn't exist 
            if (userInterests != null)
            {
                foreach (var currentUserInterest in userInterests.UserInterestList)
                {
                    if (currentUserInterest.Equals(newUserInterest))
                    {
                        log.ErrorFormat("User interest already exists. New user Interest: {0}, current Interests: {1}", JsonConvert.SerializeObject(newUserInterest), JsonConvert.SerializeObject(userInterests));
                        return new ApiObjects.Response.Status() { Code = (int)eResponseStatus.UserInterestAlreadyExist, Message = "User interest already exist" };
                    }
                }
            }

            // metaId must have a value
            if (newUserInterest.Topic == null || string.IsNullOrEmpty(newUserInterest.Topic.MetaId))
            {
                log.ErrorFormat("Error ValidateUserInterest. MetaIdRequired :{0}", JsonConvert.SerializeObject(newUserInterest));
                return new Status() { Code = (int)eResponseStatus.MetaIdRequired, Message = META_ID_REQUIRED };
            }

            //  Meta must be recognized as partner topic
            Meta topicUserInterest = groupsTopics.Where(x => x.Id == newUserInterest.Topic.MetaId).FirstOrDefault();
            if (topicUserInterest == null)
            {
                log.ErrorFormat("Error partner topic not configured as UserInterest. userInterest :{0}", JsonConvert.SerializeObject(newUserInterest));
                return new Status() { Code = (int)eResponseStatus.TopicNotFound, Message = TOPIC_NOT_FOUND };
            }

            // Value cannot be empty
            if (string.IsNullOrEmpty(newUserInterest.Topic.Value))
            {
                log.ErrorFormat("Error ValidateUserInterest. MetaValueRequired :{0}", JsonConvert.SerializeObject(newUserInterest));
                return new Status() { Code = (int)eResponseStatus.MetaValueRequired, Message = META_VALUE_REQUIRED };
            }

            //Check recursively for each parent level there is a topic meta configured, have a value
            return ValidateTopicUserInterest(newUserInterest.Topic, topicUserInterest, groupsTopics);
        }

        /// <summary>
        /// Check recursively for each parent level there is a topic meta.        
        /// </summary>
        private static Status ValidateTopicUserInterest(UserInterestTopic userInterestTopic, Meta partnerMeta, List<Meta> groupsTopics)
        {
            string errorMessageData = string.Format(" userInterestTopic: {0}, currentMeta: {1}", JsonConvert.SerializeObject(userInterestTopic), JsonConvert.SerializeObject(partnerMeta));

            // Meta defined with parentMeta but ParentTopic have no value 
            if (userInterestTopic.ParentTopic == null && !string.IsNullOrEmpty(partnerMeta.ParentId))
            {
                log.ErrorFormat("Error ParentTopic have no value but partnerMetaId does have value. {0}", errorMessageData);
                return new Status((int)eResponseStatus.ParentTopicIsRequired, PARENT_TOPIC_IS_REQUIRED);
            }

            // ParentTopic have value but Meta defined without parentMeta
            if (userInterestTopic.ParentTopic != null && !string.IsNullOrEmpty(userInterestTopic.ParentTopic.MetaId) && string.IsNullOrEmpty(partnerMeta.ParentId))
            {
                log.ErrorFormat("Error ParentTopic have value but partnerMetaId does have no value. {0}", errorMessageData);
                return new Status((int)eResponseStatus.ParentTopicShouldNotHaveValue, PARENT_TOPIC_SHOULD_NOT_HAVE_VALUE);
            }

            // ParentTopic metaId not equal to Meta parentMeta
            if (userInterestTopic.ParentTopic != null && !string.IsNullOrEmpty(userInterestTopic.ParentTopic.MetaId) && !string.IsNullOrEmpty(partnerMeta.ParentId)
                && userInterestTopic.ParentTopic.MetaId != partnerMeta.ParentId)
            {
                log.ErrorFormat("Error ParentTopic MetaId not Equals to partnerMetaId. {0}", errorMessageData);
                return new Status((int)eResponseStatus.ParentTopicMetaIdNotEqualToMetaParentMetaID, PARENT_TOPIC_META_ID_NOT_EQUAL_TO_META_PARENT_META_ID);
            }

            // ParentTopic value is missing
            if (userInterestTopic.ParentTopic != null && !string.IsNullOrEmpty(userInterestTopic.ParentTopic.MetaId) && !string.IsNullOrEmpty(partnerMeta.ParentId)
               && userInterestTopic.ParentTopic.MetaId == partnerMeta.ParentId && string.IsNullOrEmpty(userInterestTopic.ParentTopic.Value))
            {
                log.ErrorFormat("Error ParentTopic value is missing. {0}", errorMessageData);
                return new Status((int)eResponseStatus.ParentTopicValueIsMissing, PARENT_TOPIC_VALUE_IS_MISSING);
            }

            if (userInterestTopic.ParentTopic == null && string.IsNullOrEmpty(partnerMeta.ParentId))
                return new Status() { Code = (int)eResponseStatus.OK, Message = eResponseStatus.OK.ToString() };

            partnerMeta = groupsTopics.Where(x => x.Id == partnerMeta.ParentId).FirstOrDefault();
            if (partnerMeta == null)
            {
                log.ErrorFormat("Error ParentTopic have value but partnerMetaId not found. {0}", errorMessageData);
                return new Status((int)eResponseStatus.ParentIdNotAUserInterest, PARENT_ID_NOT_A_USER_INTERSET);
            }

            return ValidateTopicUserInterest(userInterestTopic.ParentTopic, partnerMeta, groupsTopics);
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
            Status response = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            string logData = string.Format("partnerId: {0}, userId: {1}", partnerId, userId);

            // validate user                                
            response = ValidateUser(partnerId, userId);
            if (response == null || response.Code != (int)eResponseStatus.OK)
            {
                log.ErrorFormat("Delete user interest failed. User not valid. {0}", logData);
                return response;
            }

            UserInterests userInterests = null;
            try
            {
                // Get user interests 
                userInterests = InterestDal.GetUserInterest(partnerId, userId);
                if (userInterests == null)
                {
                    log.ErrorFormat("No userInterests for user. {0}", logData);
                    return new ApiObjects.Response.Status((int)eResponseStatus.UserInterestNotExist, USER_INTEREST_NOT_EXIST);
                }

                var userInterestToRemove = userInterests.UserInterestList.FirstOrDefault(x => x.UserInterestId == id);
                if (userInterestToRemove == null)
                {
                    log.ErrorFormat("No such userInterestToRemove. {0}", logData);
                    return new ApiObjects.Response.Status((int)eResponseStatus.UserInterestNotExist, USER_INTEREST_NOT_EXIST);
                }

                userInterests.UserInterestList.Remove(userInterestToRemove);

                // Set CB with new interest
                if (!InterestDal.SetUserInterest(userInterests))
                {
                    log.ErrorFormat("Error delete user interest into CB. User interest {0}", JsonConvert.SerializeObject(userInterests));
                    return new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                }

                // UnRegister User ToInterestNotifications In case Topic is enable notification
                var partnerTopics = NotificationCache.Instance().GetPartnerTopicInterests(partnerId);
                if (partnerTopics == null || partnerTopics.Count == 0)
                {
                    log.ErrorFormat("Error getting partner topic interests. User ID: {0}, Partner ID: {1}", userId, partnerId);
                    return new ApiObjects.Response.Status((int)eResponseStatus.PartnerTopicInterestIsMissing, PARTNER_TOPIC_INTEREST_IS_MISSING);
                }

                var userInterestToRemoveTopic = partnerTopics.FirstOrDefault(x => x.Id == userInterestToRemove.Topic.MetaId);
                if (userInterestToRemoveTopic == null)
                {
                    log.ErrorFormat("userInterestToRemoveTopic is missing . {0}", logData);
                    return new ApiObjects.Response.Status((int)eResponseStatus.Error, "Error");
                }

                // Get all topic upper level for notification registration. 
                List<UserInterest> userInterestsForRegisterNotifications = GetUserInterestsForRegisterNotifications(userInterestToRemove, userInterests.UserInterestList);

                if (userInterestsForRegisterNotifications != null)
                {
                    // Register Notification
                    RegisterUserInterestsForNotifications(partnerId, userId, partnerTopics, userInterestsForRegisterNotifications);
                }

                // remove notification 
                // get interestNotification
                string topicNameValue = TopicInterestManager.GetInterestKeyValueName(userInterestToRemoveTopic.Name, userInterestToRemove.Topic.Value);
                InterestNotification interestNotificationToCancel = InterestDal.GetTopicInterestNotificationsByTopicNameValue(partnerId, topicNameValue, userInterestToRemoveTopic.AssetType);
                if (interestNotificationToCancel == null)
                {
                    log.ErrorFormat("Error .InterestNotification to cancel not found. {0}", logData);                    
                }

                // remove user mapping
                if (userInterestToRemoveTopic.AssetType == eAssetTypes.MEDIA)
                {
                    if (!InterestDal.RemoveUserInterestMapping(partnerId, userId, interestNotificationToCancel.Id))
                    {
                        log.ErrorFormat("Error un-mapping interest to user. User ID: {0}, interest ID: {1}", userId, interestNotificationToCancel.Id);                        
                    }
                }

                // un-register user (devices) to Amazon topic interests
                response = UnRegisterUserToInterestNotifications(partnerId, userId, new List<InterestNotification> { interestNotificationToCancel });
                if (response == null || response.Code != (int)eResponseStatus.OK)
                {
                    log.ErrorFormat("Unregistering user to notifications failed. User ID: {0}, Partner ID: {1}, User Interest to cancel: {2}", userId, partnerId, JsonConvert.SerializeObject(interestNotificationToCancel));
                    return response;
                }



                response = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error delete user interest  into CB. User interest {0}, exception {1} ", JsonConvert.SerializeObject(userInterests), ex);
            }

            return response;
        }

        private static void RegisterUserInterestsForNotifications(int partnerId, int userId, List<Meta> partnerTopics, List<UserInterest> userInterestsForRegisterNotifications)
        {
            foreach (var userInterestsForRegisterNotification in userInterestsForRegisterNotifications)
            {
                // get relevant topic 
                Meta topicInterest = partnerTopics.FirstOrDefault(x => x.Id == userInterestsForRegisterNotification.Topic.MetaId);
                if (topicInterest == null)
                {
                    log.ErrorFormat("Enable to find  topicInterest for userInterest User ID: {0}, Partner ID: {1}", userId, partnerId);
                    continue;
                }

                // get interestNotification
                string topicNameValue = TopicInterestManager.GetInterestKeyValueName(topicInterest.Name, userInterestsForRegisterNotification.Topic.Value);
                InterestNotification interestNotification = InterestDal.GetTopicInterestNotificationsByTopicNameValue(partnerId, topicNameValue, topicInterest.AssetType);
                if (interestNotification == null)
                {
                    // create notification in DB
                    CreateInterestNotification(partnerId, userInterestsForRegisterNotification, topicInterest, out interestNotification);
                    continue;
                }

                // update user-interest mapping (for inbox purposes)
                if (topicInterest.AssetType == eAssetTypes.MEDIA)
                {
                    if (!InterestDal.SetUserInterestMapping(partnerId, userId, interestNotification.Id))
                    {
                        log.ErrorFormat("Error . SetUserInterestMapping . group: {0}, user id: {1}", partnerId, userId);
                    }
                }

                // validate push is enabled
                if (!NotificationSettings.IsPartnerPushEnabled(partnerId))
                {
                    log.DebugFormat("Success. adding userInterst PartnerPushEnabled = false. group: {0}, user id: {1}", partnerId, userId);
                    continue;
                }

                // register user (devices) to Amazon topic interests
                var response = RegisterUserToInterestNotification(partnerId, userId, interestNotification);
                if (response == null || response.Code != (int)eResponseStatus.OK)
                {
                    log.ErrorFormat("Registering user to notifications failed. User ID: {0}, Partner ID: {1}, User Interest: {2}", userId, partnerId, JsonConvert.SerializeObject(userInterestsForRegisterNotification));
                    continue;
                }
            }

        }
        
        private static List<UserInterest> GetUserInterestsForRegisterNotifications(UserInterest userInterestToRemove, List<UserInterest> userInterestList)
        {
            List<UserInterest> userInterestsForRegisterNotifications = new List<UserInterest>();
            var userInterestNodeDepth = new Dictionary<string, int>();

            // iterate through UserInterestList
            foreach (var userInterestItem in userInterestList)
            {
                // iterate through topic
                UserInterestTopic topic = userInterestItem.Topic;
                int nodeDeep = 1;
                bool shouldAdd = false;

                while (topic != null)
                {
                    if (topic.Value.ToLower() == userInterestToRemove.Topic.Value.ToLower())
                    {
                        userInterestsForRegisterNotifications.Add(userInterestItem);
                        shouldAdd = true;
                    }

                    // go to parent node
                    topic = topic.ParentTopic;
                    nodeDeep++;
                }

                if (shouldAdd)
                {
                    userInterestNodeDepth.Add(userInterestItem.UserInterestId, nodeDeep);
                }
            }

            List<UserInterest> finalListForRegisterNotifications = null;

            if (userInterestNodeDepth.Count > 0)
            {
                var shorterBranch = userInterestNodeDepth.Min(x => x.Value);
                var userInterestIdsForRegistration = userInterestNodeDepth.Where(x => x.Value == shorterBranch).ToList();

                finalListForRegisterNotifications = userInterestsForRegisterNotifications.Where(x => userInterestIdsForRegistration.All(d => x.UserInterestId == d.Key)).ToList();
            }

            return finalListForRegisterNotifications;
        }

        public static string GetInterestKeyValueName(string key, string value)
        {
            return string.Format("{0}_{1}", key, value).ToLower();
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
                log.DebugFormat("linear media for channel was not found. partner ID: {0}, start time: {1}, interest message ID: {2}, linear media Id= ", partnerId, startTime, interestNotificationMessage.Id, program.m_oProgram.LINEAR_MEDIA_ID);
                // return false;
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
                //PushToWeb(partnerId, interestNotificationMessage.Id, interestNotification.QueueName, messageData, DateUtils.DateTimeToUnixTimestamp(interestNotificationMessage.SendTime));
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
            DateTime currentDate = DateTime.UtcNow;

            // validate send time is same as send time in DB
            if (Math.Abs(DateUtils.DateTimeToUnixTimestamp(interestNotificationMessage.SendTime) - startTime) > 5)
            {
                log.ErrorFormat("Message sending time is not the same as DB interest send date. asset ID: {0}, message send date: {1}, DB send date: {2}",
                    interestNotificationMessage.Id,
                    DateUtils.UnixTimeStampToDateTime(Convert.ToInt64(startTime)),
                    interestNotificationMessage.SendTime);
                return false;
            }

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

            // send push messages
            if (NotificationSettings.IsPartnerPushEnabled(partnerId))
            {
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
                    log.ErrorFormat("failed to publish interest message to push topic. response message id is empty for interests notification ID: {0}", interestNotificationMessage.Id);
                else
                {
                    log.DebugFormat("Successfully sent interest push message to topic. interest message Id: {0}", interestNotificationMessage.Id);

                    // update external push result
                    InterestNotificationMessage updatedInterestNotificationMessage = DAL.InterestDal.UpdateTopicInterestNotificationMessage(partnerId, interestNotificationMessage.Id, null, interestNotificationMessage.Message, true, resultMsgId, currentDate);
                    if (updatedInterestNotificationMessage == null)
                        log.ErrorFormat("Failed to update interest message. partner ID: {0}, interests notification ID: {1} ", partnerId, interestNotificationMessage.Id);
                    else
                    {
                        // update interest notification 
                        if (DAL.InterestDal.UpdateTopicInterestNotification(partnerId, interestNotification.Id, null, currentDate, null) == null)
                            log.ErrorFormat("Failed to update interest notification last message send date. partner ID: {0}, interests notification ID: {1} ", partnerId, interestNotificationMessage.Id);
                    }
                }

                // send to push web - rabbit.                
                //PushToWeb(partnerId, interestNotificationMessage.Id, interestNotification.QueueName, messageData, DateUtils.DateTimeToUnixTimestamp(interestNotificationMessage.SendTime));
            }

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
                            Category = eMessageCategory.Interest,
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

            return true;
        }

        internal static void HandleVodEventForInterest(int partnerId, int assetId)
        {
            // get interests topic
            List<ApiObjects.Meta> availableTopics = NotificationCache.Instance().GetPartnerTopicInterests(partnerId);
            if (availableTopics == null || availableTopics.Count == 0)
            {
                log.ErrorFormat("Available partner topics were not found. Partner ID: {0}", partnerId);
                return;
            }

            // remove irrelevant topics - relevant topics are EGPs and ENABLED_NOTIFICATION
            availableTopics.RemoveAll(x => x.AssetType != eAssetTypes.MEDIA ||
                                         x.Features == null ||
                                         !x.Features.Contains(MetaFeatureType.ENABLED_NOTIFICATION));

            if (availableTopics.Count == 0)
            {
                log.DebugFormat("Available partner VOD notifications topics were not found. Partner ID: {0}", partnerId);
                return;
            }

            // get VOD
            Core.Catalog.Response.MediaObj assetVod = null;
            string seriesName = string.Empty;
            Status status = AnnouncementManager.GetMedia(partnerId, assetId, out assetVod, out seriesName);
            if (status.Code != (int)eResponseStatus.OK)
            {
                log.ErrorFormat("interest VOD asset was not found. partner ID: {0}, assetId: {1}", partnerId, assetId);
                return;
            }

            // Iterate through all configured "should notified" partner topics
            List<KeyValuePair> relevantMediaTopics = new List<KeyValuePair>();
            foreach (var availableTopic in availableTopics)
            {
                // search for VOD Meta to notify
                if (!availableTopic.IsTag && assetVod.m_lMetas != null)
                {
                    var assetMetasForNotification = assetVod.m_lMetas.Where(x => x.m_oTagMeta != null && x.m_oTagMeta.m_sName.ToLower() == availableTopic.Name.ToLower());
                    if (assetMetasForNotification.Count() > 0)
                    {
                        foreach (var meta in assetMetasForNotification)
                            relevantMediaTopics.Add(new KeyValuePair() { key = meta.m_oTagMeta.m_sName.ToLower(), value = meta.m_sValue.ToLower() });
                    }
                }

                // search for VOD Tag to notify
                if (availableTopic.IsTag && assetVod.m_lTags != null)
                {
                    var assetTagsForNotification = assetVod.m_lTags.Where(x => x.m_oTagMeta != null && x.m_oTagMeta.m_sName.ToLower() == availableTopic.Name.ToLower());
                    if (assetTagsForNotification.Count() > 0)
                    {
                        foreach (var tag in assetTagsForNotification)
                        {
                            if (tag.Values != null)
                            {
                                foreach (var tagValue in tag.m_lValues)
                                    relevantMediaTopics.Add(new KeyValuePair() { key = tag.m_oTagMeta.m_sName.ToLower(), value = tagValue.ToLower() });
                            }
                        }
                    }
                }

                InterestNotificationMessage newInterestMessage;
                foreach (var programNotificationTopic in relevantMediaTopics)
                {
                    // check if topic interest exists
                    string keyValueTopic = TopicInterestManager.GetInterestKeyValueName(programNotificationTopic.key, programNotificationTopic.value);
                    InterestNotification interestNotification = InterestDal.GetTopicInterestNotificationsByTopicNameValue(partnerId, keyValueTopic, eAssetTypes.MEDIA);
                    if (interestNotification == null)
                    {
                        log.DebugFormat("No interest notification for topic key-value: {0}, partner ID: {1}", keyValueTopic, partnerId);
                        continue;
                    }
                    else
                        log.DebugFormat("VOD topic was found for notification. Asset ID: {0}, key-value topic: {1}", assetId, keyValueTopic);


                    // check if future message was not already sent and we only need to update
                    InterestNotificationMessage oldInterestMessage = InterestDal.GetTopicInterestNotificationMessageByInterestNotificationId(partnerId, interestNotification.Id, assetId);
                    if (oldInterestMessage == null)
                    {
                        // interest message wasn't found - create a new one
                        newInterestMessage = new InterestNotificationMessage()
                        {
                            Name = string.Format("Interest_{0}_{1}", keyValueTopic, assetId),
                            ReferenceAssetId = assetId,
                            SendTime = assetVod.m_dCatalogStartDate,
                            TopicInterestsNotificationsId = interestNotification.Id
                        };

                        // insert to DB
                        newInterestMessage = InterestDal.InsertTopicInterestNotificationMessage(partnerId, newInterestMessage.Name, newInterestMessage.Message, newInterestMessage.SendTime, newInterestMessage.TopicInterestsNotificationsId, newInterestMessage.ReferenceAssetId);
                        if (newInterestMessage == null || newInterestMessage.Id == 0)
                        {
                            log.ErrorFormat("Error while trying to insert new interest message. topic: {0}, asset VOD: {1}", JsonConvert.SerializeObject(programNotificationTopic), JsonConvert.SerializeObject(assetVod));
                            continue;
                        }

                        // send rabbit
                        TopicInterestManager.AddInterestToQueue(partnerId, newInterestMessage);
                    }
                    else
                    {
                        // interest found - check if send date changed
                        if ((oldInterestMessage.SendTime - assetVod.m_dCatalogStartDate).Duration() > TimeSpan.FromMinutes(1))
                        {
                            log.DebugFormat("Asset VOD changed it catalog start date - updating in interest message. partner ID: {0}, asset ID: {1}, original send time: {2}, new send time: {3}",
                                partnerId,
                                assetId,
                                oldInterestMessage.SendTime.ToString(),
                                assetVod.m_dCatalogStartDate);

                            // epg program date changed - update DB
                            newInterestMessage = InterestDal.UpdateTopicInterestNotificationMessage(partnerId, oldInterestMessage.Id, assetVod.m_dCatalogStartDate);
                            if (newInterestMessage == null || newInterestMessage.Id == 0)
                            {
                                log.ErrorFormat("Error while trying to update interest message time. topic: {0}, assetVod: {1}", JsonConvert.SerializeObject(programNotificationTopic), JsonConvert.SerializeObject(assetVod));
                                continue;
                            }

                            // send rabbit
                            TopicInterestManager.AddInterestToQueue(partnerId, newInterestMessage);
                        }
                    }
                }
            }
        }

        public static void HandleEpgEventForInterests(int partnerId, NotificationPartnerSettingsResponse partnerSettings, List<Core.Catalog.ProgramObj> programs)
        {
            // get interests topic
            List<ApiObjects.Meta> availableTopics = NotificationCache.Instance().GetPartnerTopicInterests(partnerId);
            if (availableTopics == null || availableTopics.Count == 0)
            {
                log.ErrorFormat("Available partner topics were not found. Partner ID: {0}", partnerId);
                return;
            }

            // remove irrelevant topics - relevant topics are EGPs and ENABLED_NOTIFICATION
            availableTopics.RemoveAll(x => x.AssetType != eAssetTypes.EPG ||
                                         x.Features == null ||
                                         !x.Features.Contains(MetaFeatureType.ENABLED_NOTIFICATION));

            if (availableTopics.Count == 0)
            {
                log.ErrorFormat("Available partner EPG notifications topics were not found. Partner ID: {0}", partnerId);
                return;
            }

            foreach (Core.Catalog.ProgramObj program in programs)
            {
                // check EPG validity
                if (program == null || program.AssetType != eAssetTypes.EPG || program.m_oProgram == null || program.m_oProgram.EPG_ID < 1)
                {
                    log.ErrorFormat("Error with received EPG program: {0}", JsonConvert.SerializeObject(program));
                    continue;
                }

                // Iterate through all configured "should notified" partner topics
                List<EPGDictionary> programNotificationTopics;
                foreach (var availableTopic in availableTopics)
                {
                    programNotificationTopics = new List<EPGDictionary>();

                    // check if program contains an allowed meta notification

                    if (!availableTopic.IsTag && program.m_oProgram.EPG_Meta != null)
                    {
                        if (program.m_oProgram.EPG_Meta.Exists(x => x.Key.ToLower() == availableTopic.Name.ToLower()))
                            programNotificationTopics = program.m_oProgram.EPG_Meta.Where(x => x.Key.ToLower() == availableTopic.Name.ToLower()).ToList();
                    }

                    // check if program contains an allowed tag notification
                    if (availableTopic.IsTag && program.m_oProgram.EPG_TAGS != null)
                    {
                        if (program.m_oProgram.EPG_TAGS.Exists(x => x.Key.ToLower() == availableTopic.Name.ToLower()))
                        {
                            if (programNotificationTopics.Count > 0)
                                programNotificationTopics.AddRange(program.m_oProgram.EPG_TAGS.Where(x => x.Key.ToLower() == availableTopic.Name.ToLower()).ToList());
                            else
                                programNotificationTopics = program.m_oProgram.EPG_TAGS.Where(x => x.Key.ToLower() == availableTopic.Name.ToLower()).ToList();
                        }
                    }

                    // Parse program start date (with reminder pre-padding)
                    DateTime newEpgSendDate;
                    if (!DateTime.TryParseExact(program.m_oProgram.START_DATE, AnnouncementManager.EPG_DATETIME_FORMAT, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out newEpgSendDate))
                    {
                        log.ErrorFormat("Failed parsing EPG start date for EPG notification event, epgID: {0}, startDate: {1}", program.m_oProgram.EPG_ID, program.m_oProgram.START_DATE);
                        continue;
                    }
                    newEpgSendDate = newEpgSendDate.AddSeconds((double)partnerSettings.settings.RemindersPrePaddingSec * -1);

                    InterestNotificationMessage newInterestMessage;
                    foreach (var programNotificationTopic in programNotificationTopics)
                    {
                        // check if topic interest exists
                        string keyValueTopic = TopicInterestManager.GetInterestKeyValueName(programNotificationTopic.Key, programNotificationTopic.Value);
                        InterestNotification interestNotification = InterestDal.GetTopicInterestNotificationsByTopicNameValue(partnerId, keyValueTopic, eAssetTypes.EPG);
                        if (interestNotification == null)
                        {
                            log.DebugFormat("No interest notification for topic key-value: {0}, partner ID: {1}", keyValueTopic, partnerId);
                            continue;
                        }
                        else
                            log.DebugFormat("Program topic was found for notification. Program ID: {0}, key-value topic: {1}", program.AssetId, keyValueTopic);

                        // check if future message was not already sent and we only need to update
                        InterestNotificationMessage oldInterestMessage = InterestDal.GetTopicInterestNotificationMessageByInterestNotificationId(partnerId, interestNotification.Id, (int)program.m_oProgram.EPG_ID);
                        if (oldInterestMessage == null)
                        {
                            // interest message wasn't found - create a new one
                            newInterestMessage = new InterestNotificationMessage()
                            {
                                Name = string.Format("Interest_{0}_{1}", keyValueTopic, program.m_oProgram.NAME),
                                ReferenceAssetId = (int)program.m_oProgram.EPG_ID,
                                SendTime = newEpgSendDate,
                                TopicInterestsNotificationsId = interestNotification.Id
                            };

                            // insert to DB
                            newInterestMessage = InterestDal.InsertTopicInterestNotificationMessage(partnerId, newInterestMessage.Name, newInterestMessage.Message, newInterestMessage.SendTime, newInterestMessage.TopicInterestsNotificationsId, newInterestMessage.ReferenceAssetId);
                            if (newInterestMessage == null || newInterestMessage.Id == 0)
                            {
                                log.ErrorFormat("Error while trying to insert new interest message. topic: {0}, program: {1}", JsonConvert.SerializeObject(programNotificationTopic), JsonConvert.SerializeObject(program));
                                continue;
                            }

                            // send rabbit
                            TopicInterestManager.AddInterestToQueue(partnerId, newInterestMessage);
                        }
                        else
                        {
                            // interest found - check if send date changed
                            if ((oldInterestMessage.SendTime - newEpgSendDate).Duration() > TimeSpan.FromMinutes(1))
                            {
                                log.DebugFormat("Asset EPG changed it start date - updating in interest message. partner ID: {0}, asset ID: {1}, original send time: {2}, new send time: {3}",
                                         partnerId,
                                         program.AssetId,
                                         oldInterestMessage.SendTime.ToString(),
                                         newEpgSendDate.ToString());

                                // EPG program date changed - update DB
                                newInterestMessage = InterestDal.UpdateTopicInterestNotificationMessage(partnerId, oldInterestMessage.Id, newEpgSendDate);
                                if (newInterestMessage == null || newInterestMessage.Id == 0)
                                {
                                    log.ErrorFormat("Error while trying to update interest message time. topic: {0}, program: {1}", JsonConvert.SerializeObject(programNotificationTopic), JsonConvert.SerializeObject(program));
                                    continue;
                                }

                                // send rabbit
                                TopicInterestManager.AddInterestToQueue(partnerId, newInterestMessage);
                            }
                        }
                    }
                }
            }
        }
    }

}
