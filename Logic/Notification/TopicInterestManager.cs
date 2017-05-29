using ApiObjects;
using ApiObjects.Response;
using DAL;
using KLogMonitor;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ApiObjects.Notification;
using QueueWrapper.Queues.QueueObjects;
using ApiObjects.QueueObjects;
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
            ApiObjects.Response.Status response = new ApiObjects.Response.Status();

            UserInterests userInterests = null;
            bool updateIsNeeded = false;
            try
            {
                // Validate data
                // 1. validate user
                // 2. validate with topic_interenst make sure that parent and child valid

                // 3. check that userInterest not already exist


                // Get user interests 
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
                    string newUserInterestString = JsonConvert.SerializeObject(userInterest);

                    var userInterestItem = userInterests.UserInterestList.Where(x => JsonConvert.SerializeObject(x) == newUserInterestString).FirstOrDefault();

                    if (userInterestItem == null)
                    {
                        userInterest.Id = Guid.NewGuid().ToString();
                        userInterests.UserInterestList.Add(userInterest);
                        updateIsNeeded = true;
                    }
                }

                // Set CB with new interest
                if (updateIsNeeded)
                {
                    if (!InterestDal.SetUserInterest(userInterests))
                        log.ErrorFormat("Error inserting user interest  into CB. User interest {0}", JsonConvert.SerializeObject(userInterests));
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
