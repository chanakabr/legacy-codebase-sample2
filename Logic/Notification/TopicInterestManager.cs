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


namespace APILogic.Notification
{
    public class TopicInterestManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private const string USER_INTEREST_NOT_EXIST = "User interest not exist";

        private const string ROUTING_KEY_INTEREST_MESSAGES = "PROCESS_MESSAGE_INTERESTS";

        public static ApiObjects.Response.Status AddUserInterest(int partnerId, int userId, UserInterest userInterest)
        {
            ApiObjects.Response.Status response = null;
            UserInterests userInterests = null;
            bool updateIsNeeded = false;
            try
            {
                // Validate data
                // 1. validate user

                // 2. validate with topic_interest make sure that parent and child valid
                List<Meta> groupsMeta = CatalogDAL.GetTopicInterests(partnerId);
                bool a = ValidateInterestNode(userInterest, groupsMeta);

                Meta groupTopicInterest = groupsMeta.Where(x => x.MetaId == userInterest.MetaId).FirstOrDefault();
                if (groupTopicInterest == null)
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
                            return new Status() { Code = (int)eResponseStatus.UserInterestAlreadyExist, Message = "User interest already exist" };
                        }
                    }

                    // UserInterest should ne added
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

                // Send notification incase feature is ENABLED_NOTIFICATION                
                if (groupTopicInterest.Features.Contains(MetaFeatureType.ENABLED_NOTIFICATION))
                {
                    // get interestNotification - according to userInterest
                    string topicNameValue;
                    InterestNotification interestNotification;
                    response = GetInterestNotification(partnerId, userInterest, groupTopicInterest, out topicNameValue, out interestNotification);

                    if (response.Code != (int)eResponseStatus.OK)
                        return response;

                    // create Amazon topic in case partner push is enabled 
                    if (NotificationSettings.IsPartnerPushEnabled(partnerId))
                    {
                        // create Amazon topic 
                        string externalId = string.Empty;
                        if (string.IsNullOrEmpty(interestNotification.ExternalId))
                        {
                            //TODO: check with Amishai metaNameValue or topicNameValue 
                            externalId = NotificationAdapter.CreateAnnouncement(partnerId, string.Format("Interest_{0}", topicNameValue));
                            if (string.IsNullOrEmpty(externalId))
                            {
                                log.DebugFormat("failed to create announcement groupID = {0}, topicNameValue = {1}", partnerId, topicNameValue);
                                return new Status((int)eResponseStatus.FailCreateAnnouncement, "fail create Amazon interest topic");
                            }

                            // update table with external id
                            interestNotification = InterestDal.UpdateTopicInterestNotification(partnerId, interestNotification.Id, externalId);
                            if (interestNotification == null)
                            {
                                log.DebugFormat("failed to update topic interest notification: {0} with externalId: {1}, groupId = {2}", interestNotification.Id, externalId, partnerId);
                            }
                        }

                        // register user to topic


                    }
                }

            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error inserting user interest  into CB. User interest {0}, exception {1} ", JsonConvert.SerializeObject(userInterests), ex);
            }

            return new Status() { Code = (int)eResponseStatus.OK, Message = eResponseStatus.OK.ToString() };
        }

        private static ApiObjects.Response.Status GetInterestNotification(int partnerId, UserInterest userInterest, Meta groupTopicInterest,
            out string topicNameValue, out InterestNotification interestNotification)
        {
            topicNameValue = string.Format("{0}_{1}", groupTopicInterest.Name, userInterest.Topic.Value);
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
                //TODO: talks with amishai topicInterestId (0)
                interestNotification = InterestDal.InsertTopicInterestNotification(partnerId, groupTopicInterest.Name, string.Empty, messageTemplateType, topicNameValue, 0, groupTopicInterest.AssetType);

                if (interestNotification == null)
                {
                    log.ErrorFormat("Error to create DB interestNotification. TopicInterest : {0}", JsonConvert.SerializeObject(groupTopicInterest));
                    return new Status() { Code = (int)eResponseStatus.Error, Message = eResponseStatus.Error.ToString() };
                }
            }

            return new Status() { Code = (int)eResponseStatus.OK, Message = eResponseStatus.OK.ToString() };
        }

        private static bool ValidateInterestNode(UserInterest userInterest, List<Meta> groupsMeta)
        {
            //var a = groupsMeta.Where(x => x.MetaId == userInterest.MetaId).FirstOrDefault();
            //if (a != null)
            //{
            //    var b = groupsMeta.Where(x => x.MetaId == userInterest.Topic.).FirstOrDefault();
            //}
            return true;
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

        internal static bool SendMessageInterest(int nGroupID, long startTime, int notificationInterestId)
        {
            throw new NotImplementedException();
        }
    }
}
