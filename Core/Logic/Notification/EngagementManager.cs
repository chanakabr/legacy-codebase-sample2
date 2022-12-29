using ApiLogic.Notification;
using ApiLogic.Notification.Managers;
using APILogic.AmazonSnsAdapter;
using APILogic.Notification.Adapters;
using ApiObjects;
using ApiObjects.Notification;
using ApiObjects.QueueObjects;
using ApiObjects.Response;
using Phx.Lib.Appconfig;
using Core.Notification.Adapters;
using Core.Pricing;
using DAL;
using Phx.Lib.Log;
using Newtonsoft.Json;
using QueueWrapper.Queues.QueueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Core.Users;
using Core.Users.Cache;
using iot;
using TVinciShared;
using Iot = ApiObjects.Iot;
using Status = ApiObjects.Response.Status;

namespace Core.Notification
{
    public class EngagementManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private static readonly Status ENGAGEMENT_ADAPTER_ID_REQUIRED = new Status(eResponseStatus.EngagementAdapterIdentifierRequired, "Engagement adapter identifier is required");
        private static readonly Status ENGAGEMENT_ADAPTER_NOT_EXIST = new Status(eResponseStatus.EngagementAdapterNotExist, "Engagement adapter doesn't exist");
        private static readonly Status ENGAGEMENT_ADAPTER_GENERAL_ERROR = new Status(eResponseStatus.Error, eResponseStatus.Error.ToString());

        private static readonly Status ENGAGEMENT_ADAPTER_FAILED_TO_INSERT = new Status(eResponseStatus.Error, "failed to insert new engagement Adapter");
        private static readonly Status ENGAGEMENT_ADAPTER_FAILED_SET_CHANGES = new Status(eResponseStatus.Error, "Engagement adapter failed set changes");

        private static readonly Status NO_ENGAGEMENT_ADAPTER_TO_INSERT = new Status(eResponseStatus.NoEngagementAdapterToInsert, "Engagement adapter wasn't found");
        private static readonly Status NO_ENGAGEMENT_ADAPTER_TO_UPDATE = new Status(eResponseStatus.NoEngagementAdapterToUpdate, "No Engagement adapter to update");

        private static readonly Status NAME_REQUIRED = new Status(eResponseStatus.NameRequired, "Name must have a value");
        private static readonly Status ADAPTER_URL_REQUIRED = new Status(eResponseStatus.AdapterUrlRequired, "ADAPTER URL must have a value");
        private static readonly Status PROVIDER_URL_REQUIRED = new Status(eResponseStatus.ProviderUrlRequired, "Provider URL must have a value");
        
        private const string NO_ENGAGEMENT_TO_INSERT = "No Engagement to insert";
        private const string ENGAGEMENT_NOT_EXIST = "Engagement not exist";
        private const string EMPTY_SOURCE_USER_LIST = "User list and adapter ID are empty";
        private const string DUPLICATE_SOURCE_USER_LIST = "Duplicate source list";
        private const string FUTURE_SEND_TIME = "Send time must be in the future";
        private const string ILLEGAL_USER_LIST = "Illegal user list";
        private const string ILLEGAL_ENGAGEMENT_INTERVAL = "Illegal interval inserted";
        private const string ENGAGEMENT_TIME_DIFFERENCE = "The difference send time between identical engagements must be higher that 1 hour";
        private const string ENGAGEMENT_SEND_WINDOW_FRAME = "Send time is not between the allowed time window";
        private const string FUTURE_SCHEDULE_ENGAGEMENT_DETECTED = "Future engagement scheduler detected";
        private const string ENGAGEMENT_TEMPLATE_NOT_FOUND = "Engagement template wasn't found";
        private const string ENGAGEMENT_SUCCESSFULLY_INSERTED = "Engagement was successfully inserted";
        private const string ERROR_INSERTING_ENGAGEMENT = "Error occurred while inserting engagement";
        private const string COUPON_GROUP_NOT_FOUND = "Coupon group ID wasn't found";
        private const string SCHEDULE_ENGAGEMENT_WITHOUT_ADAPTER = "Scheduler engagement must contain an adapter ID";
        private const string MAX_NUMBER_OF_PUSH_MSG_EXCEEDED = "Maximum number of push messages to user have reached its limit";
        private const string FAILED_TO_UPDATE_THING_SHADOW = "Failed to update thing shadow";
        private const string DEVICE_NOT_IN_DOMAIN = "Device not in domain";

        private static int NUM_OF_BULK_MESSAGE_ENGAGEMENTS = 500;
        private static int NUM_OF_ENGAGEMENT_THREADS = 10;
        private static int MAX_PUSH_MSG_PER_SECONDS = 3;

        private const string ROUTING_KEY_ENGAGEMENTS = "PROCESS_ENGAGEMENTS";

        # region Engagement Adapter

        internal static EngagementAdapterResponseList GetEngagementAdapters(int groupId)
        {
            try
            {
                var engagementAdapters = EngagementDal.GetEngagementAdapterList(groupId);

                return new EngagementAdapterResponseList
                {
                    EngagementAdapters = engagementAdapters,
                    Status = new Status(eResponseStatus.OK,
                        engagementAdapters?.Count > 0
                            ? eResponseStatus.OK.ToString()
                            : "No engagement adapter related to group")
                };
            }
            catch (Exception ex)
            {
                log.Error($"Failed to get engagement adapters. GroupID: {groupId}", ex);
                return new EngagementAdapterResponseList {Status = ENGAGEMENT_ADAPTER_GENERAL_ERROR};
            }
        }

        internal static EngagementAdapterResponse GetEngagementAdapter(int groupId, int engagementAdapterId)
        {
            try
            {
                var engagementAdapter = EngagementDal.GetEngagementAdapter(groupId, engagementAdapterId);

                return engagementAdapter == null 
                    ? EngagementAdapterResponse.Error(ENGAGEMENT_ADAPTER_NOT_EXIST)
                    : EngagementAdapterResponse.Ok(engagementAdapter, "OK");
            }
            catch (Exception ex)
            {
                log.Error($"Failed to get engagement adapter. Group ID: {groupId}, engagementAdapterId: {engagementAdapterId}", ex);
                return EngagementAdapterResponse.Error(ENGAGEMENT_ADAPTER_GENERAL_ERROR);
            }
        }

        internal static Status DeleteEngagementAdapter(int groupId, int id)
        {
            try
            {
                if (id == 0)
                {
                    return ENGAGEMENT_ADAPTER_ID_REQUIRED;
                }

                var adapter = EngagementDal.GetEngagementAdapter(groupId, id);
                if (adapter == null || adapter.ID <= 0)
                {
                    return ENGAGEMENT_ADAPTER_NOT_EXIST;
                }

                var isSet = EngagementDal.DeleteEngagementAdapter(groupId, id);
                return isSet 
                    ? new Status(eResponseStatus.OK, "Engagement adapter deleted")
                    : ENGAGEMENT_ADAPTER_NOT_EXIST;
            }
            catch (Exception ex)
            {
                log.Error($"Failed to delete engagement adapter. Group ID: {groupId}, engagementAdapterId: {id}", ex);
                return ENGAGEMENT_ADAPTER_GENERAL_ERROR;
            }
        }

        internal static EngagementAdapterResponse InsertEngagementAdapter(int groupId, EngagementAdapter newEngagementAdapter)
        {
            if (newEngagementAdapter == null)
            {
                return EngagementAdapterResponse.Error(NO_ENGAGEMENT_ADAPTER_TO_INSERT);
            }

            var response = ValidateCommonFields(newEngagementAdapter);
            if (response != null)
            {
                return response;
            }

            try
            {
                newEngagementAdapter.SharedSecret = GenerateSharedSecret();

                var engagementAdapter = EngagementDal.InsertEngagementAdapter(groupId, newEngagementAdapter);

                if (engagementAdapter == null || engagementAdapter.ID <= 0)
                {
                    return EngagementAdapterResponse.Error(ENGAGEMENT_ADAPTER_FAILED_TO_INSERT);
                }

                if (!EngagementAdapterClient.SendConfigurationToAdapter(groupId, engagementAdapter))
                    log.Debug($"InsertEngagementAdapter - SendConfigurationToAdapter failed : AdapterID = {engagementAdapter.ID}");

                return EngagementAdapterResponse.Ok(engagementAdapter, "New engagement adapter was successfully inserted");
                
            }
            catch (Exception ex)
            {
                log.Error($"Failed to insert engagement adapter. Group ID: {groupId}", ex);
                return EngagementAdapterResponse.Error(ENGAGEMENT_ADAPTER_GENERAL_ERROR);
            }
        }

        internal static EngagementAdapterResponse SetEngagementAdapter(int groupId, EngagementAdapter newEngagementAdapter)
        {
            if (newEngagementAdapter == null)
            {
                return EngagementAdapterResponse.Error(NO_ENGAGEMENT_ADAPTER_TO_UPDATE);
            }

            if (newEngagementAdapter.ID == 0)
            {
                return EngagementAdapterResponse.Error(ENGAGEMENT_ADAPTER_ID_REQUIRED);
            }

            var response = ValidateCommonFields(newEngagementAdapter);
            if (response != null)
            {
                return response;
            }

            try
            {
                var existingEngagementAdapter = EngagementDal.GetEngagementAdapter(groupId, newEngagementAdapter.ID);
                if (existingEngagementAdapter == null)
                {
                    return EngagementAdapterResponse.Error(ENGAGEMENT_ADAPTER_NOT_EXIST);
                }

                // SharedSecret generated only at insert 
                // this value not relevant at update and should be ignored
                newEngagementAdapter.SharedSecret = null;

                var engagementAdapter = newEngagementAdapter.SkipSettings
                    ? EngagementDal.SetEngagementAdapter(groupId, newEngagementAdapter)
                    : EngagementDal.SetEngagementAdapterWithSettings(groupId, newEngagementAdapter);

                if (engagementAdapter == null || engagementAdapter.ID <= 0 || (!newEngagementAdapter.SkipSettings && engagementAdapter.Settings == null))
                {
                    return EngagementAdapterResponse.Error(ENGAGEMENT_ADAPTER_FAILED_SET_CHANGES);
                }

                if (!EngagementAdapterClient.SendConfigurationToAdapter(groupId, engagementAdapter))
                    log.Debug($"SetEngagementAdapter - SendConfigurationToAdapter failed : AdapterID = {newEngagementAdapter.ID}");

                return EngagementAdapterResponse.Ok(engagementAdapter, "Engagement adapter was successfully set");
            }
            catch (Exception ex)
            {
                log.Error($"Failed to set engagement adapter. Group ID: {groupId}, engagement adapter ID: {newEngagementAdapter.ID}, " +
                          $"name: {newEngagementAdapter.Name}, adapterUrl: {newEngagementAdapter.AdapterUrl}, isActive: {newEngagementAdapter.IsActive}", ex);
                return EngagementAdapterResponse.Error(ENGAGEMENT_ADAPTER_GENERAL_ERROR);
            }
        }

        internal static EngagementAdapterResponse GenerateEngagementSharedSecret(int groupId, int engagementAdapterId)
        {
            if (engagementAdapterId <= 0)
            {
                return EngagementAdapterResponse.Error(ENGAGEMENT_ADAPTER_ID_REQUIRED);
            }

            try
            {
                var engagementAdapter = EngagementDal.GetEngagementAdapter(groupId, engagementAdapterId);
                if (engagementAdapter == null || engagementAdapter.ID <= 0)
                {
                    return EngagementAdapterResponse.Error(ENGAGEMENT_ADAPTER_NOT_EXIST);
                }

                var sharedSecret = GenerateSharedSecret();

                var adapterWithSecret = EngagementDal.SetEngagementAdapterSharedSecret(groupId, engagementAdapterId, sharedSecret);
                if (adapterWithSecret == null || adapterWithSecret.ID <= 0)
                {
                    return EngagementAdapterResponse.Error(ENGAGEMENT_ADAPTER_FAILED_SET_CHANGES);
                }

                engagementAdapter.SharedSecret = adapterWithSecret.SharedSecret;

                return EngagementAdapterResponse.Ok(engagementAdapter, "Engagement adapter generate shared secret");
            }
            catch (Exception ex)
            {
                log.Error($"Failed to generate shared secret. GroupID={groupId}, engagementAdapterId={engagementAdapterId}", ex);
                return EngagementAdapterResponse.Error(ENGAGEMENT_ADAPTER_GENERAL_ERROR);
            }
        }

        private static EngagementAdapterResponse ValidateCommonFields(EngagementAdapter engagementAdapter)
        {
            if (string.IsNullOrEmpty(engagementAdapter.Name))
            {
                return EngagementAdapterResponse.Error(NAME_REQUIRED);
            }

            if (string.IsNullOrEmpty(engagementAdapter.AdapterUrl))
            {
                return EngagementAdapterResponse.Error(ADAPTER_URL_REQUIRED);
            }

            if (string.IsNullOrEmpty(engagementAdapter.ProviderUrl))
            {
                return EngagementAdapterResponse.Error(PROVIDER_URL_REQUIRED);
            }

            return null;
        }

        private static string GenerateSharedSecret()
        {
            return Guid.NewGuid().ToString("N").Substring(0, 16);
        }

        # endregion Engagement Adapter

        internal static EngagementResponse AddEngagement(int partnerId, Engagement engagement)
        {
            EngagementResponse response = new EngagementResponse();

            try
            {
                // validate input
                if (!ValidateInputEngagement(partnerId, engagement, ref response))
                    return response;

                // insert engagement to DB
                response.Engagement = EngagementDal.InsertEngagement(partnerId, engagement);
                if (response.Engagement != null && response.Engagement.Id > 0)
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, ENGAGEMENT_SUCCESSFULLY_INSERTED);
                else
                {
                    log.ErrorFormat("Error occurred while inserting engagement to DB");
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, ERROR_INSERTING_ENGAGEMENT);
                    return response;
                }

                // create rabbit message
                if (!AddEngagementToQueue(partnerId, DateUtils.DateTimeToUtcUnixTimestampSeconds(engagement.SendTime), response.Engagement.Id))
                {
                    log.ErrorFormat("Error while trying to create engagement rabbit message. Engagement data: {0}", JsonConvert.SerializeObject(engagement));
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, ERROR_INSERTING_ENGAGEMENT);

                    // remove newly created engagement from DB
                    if (!EngagementDal.DeleteEngagement(partnerId, response.Engagement.Id))
                        log.ErrorFormat("Error while trying to delete engagement from DB. Engagement: {0}", JsonConvert.SerializeObject(engagement));

                    return response;
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed to add engagement. GroupID: {0}", partnerId), ex);
            }
            return response;
        }

        private static bool ValidateInputEngagement(int partnerId, Engagement engagement, ref EngagementResponse response)
        {
            DateTime utcNow = DateTime.UtcNow;
            response = new EngagementResponse();

            // validate engagement exists
            if (engagement == null)
            {
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.NoEngagementToInsert, NO_ENGAGEMENT_TO_INSERT);
                log.ErrorFormat("Empty engagement received. Partner ID: {0}", partnerId);
                return false;
            }

            // validate send time is in the future
            if (engagement.SendTime < DateTime.UtcNow.AddMinutes(-2))
            {
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.IllegalPostData, FUTURE_SEND_TIME);
                log.ErrorFormat("Send time must be in the future. Partner ID: {0}, Engagement: {1}", partnerId, JsonConvert.SerializeObject(engagement));
                return false;
            }

            // validate user list is legal (if sent)
            if (!string.IsNullOrEmpty(engagement.UserList))
            {
                try
                {
                    List<int> userList = engagement.UserList.Split(';', ',', '|').Select(p => Convert.ToInt32(p.Trim())).ToList();
                    if (userList == null || userList.Count() == 0)
                    {
                        response.Status = new ApiObjects.Response.Status((int)eResponseStatus.IllegalPostData, ILLEGAL_USER_LIST);
                        log.ErrorFormat("Illegal user list were inserted. Partner ID: {0}, Engagement: {1}", partnerId, JsonConvert.SerializeObject(engagement));
                        return false;
                    }
                }
                catch (Exception)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.IllegalPostData, ILLEGAL_USER_LIST);
                    log.ErrorFormat("Illegal user list were inserted. Partner ID: {0}, Engagement: {1}", partnerId, JsonConvert.SerializeObject(engagement));
                    return false;
                }
            }

            // validate user list or adapter ID exists
            if (string.IsNullOrEmpty(engagement.UserList) && engagement.AdapterId == 0)
            {
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.IllegalPostData, EMPTY_SOURCE_USER_LIST);
                log.ErrorFormat("User list and adapter ID are empty. Partner ID: {0}, Engagement: {1}", partnerId, JsonConvert.SerializeObject(engagement));
                return false;
            }

            // validate user list or adapter ID exists
            if (!string.IsNullOrEmpty(engagement.UserList) && engagement.AdapterId > 0)
            {
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.IllegalPostData, DUPLICATE_SOURCE_USER_LIST);
                log.ErrorFormat("Duplicate source list. User list and adapter ID are both with values. Partner ID: {0}, Engagement: {1}", partnerId, JsonConvert.SerializeObject(engagement));
                return false;
            }

            // validate adapter ID exists
            if (engagement.AdapterId > 0)
            {
                if (EngagementDal.GetEngagementAdapter(partnerId, engagement.AdapterId) == null)
                {
                    response.Status = ENGAGEMENT_ADAPTER_NOT_EXIST;
                    log.ErrorFormat("Duplicate source list. User list and adapter ID are both with values. Partner ID: {0}, Engagement: {1}", partnerId, JsonConvert.SerializeObject(engagement));
                    return false;
                }
            }

            // validate interval is legal
            if (engagement.IntervalSeconds < 0)
            {
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.IllegalPostData, ILLEGAL_ENGAGEMENT_INTERVAL);
                log.ErrorFormat("Illegal interval was inserted. Partner ID: {0}, Engagement: {1}", partnerId, JsonConvert.SerializeObject(engagement));
                return false;
            }

            // Get all engagements from the last hour and forward
            List<Engagement> lastHourAndFutureEngagement = EngagementDal.GetEngagementList(partnerId, utcNow.AddHours(-1));
            if (lastHourAndFutureEngagement != null)
            {
                // validate same engagement was not already sent in the last hour
                Engagement engagementAlreadySent = lastHourAndFutureEngagement.FirstOrDefault(x => x.EngagementType == engagement.EngagementType &&
                                                                                                   x.AdapterId == engagement.AdapterId &&
                                                                                                   x.AdapterDynamicData == engagement.AdapterDynamicData &&
                                                                                                   x.UserList == engagement.UserList &&
                                                                                                  Math.Abs((x.SendTime - engagement.SendTime).TotalHours) < 1);
                if (engagementAlreadySent != null)
                {
                    log.ErrorFormat("Engagement was already sent in the last hour. Sent engagement: {0}, my (canceled) engagement: {1}",
                     JsonConvert.SerializeObject(engagementAlreadySent),
                     JsonConvert.SerializeObject(engagement));

                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.EngagementTimeDifference, ENGAGEMENT_TIME_DIFFERENCE);
                    return false;
                }
            }

            // if push enabled - validate time window
            if (NotificationSettings.IsPartnerPushEnabled(partnerId))
            {
                if (!NotificationSettings.IsWithinPushSendTimeWindow(partnerId, engagement.SendTime.TimeOfDay))
                {
                    log.ErrorFormat("Engagement send time is not between the allowed time window. Engagement: {0}", JsonConvert.SerializeObject(engagement));
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.EngagementIllegalSendTime, ENGAGEMENT_SEND_WINDOW_FRAME);
                    return false;
                }
            }

            // check if engagement is a scheduled type
            if (engagement.IntervalSeconds > 0)
            {
                // validate there isn't another one in the future 
                Engagement futureEngagement = lastHourAndFutureEngagement.FirstOrDefault(x => x.EngagementType == engagement.EngagementType &&
                                                                                              x.AdapterId == engagement.AdapterId &&
                                                                                              x.AdapterDynamicData == engagement.AdapterDynamicData &&
                                                                                              x.IntervalSeconds > 0 &&
                                                                                              x.SendTime > engagement.SendTime);
                if (futureEngagement != null)
                {
                    log.ErrorFormat("Future scheduled engagement detected. Future Engagement: {0}", JsonConvert.SerializeObject(futureEngagement));
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.FutureScheduledEngagementDetected, FUTURE_SCHEDULE_ENGAGEMENT_DETECTED);
                    return false;
                }

                // validate adapter exist (must for scheduler)
                if (engagement.AdapterId == 0)
                {
                    log.ErrorFormat("Scheduled engagement cannot be created without an adapter. Engagement: {0}", JsonConvert.SerializeObject(engagement));
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.EngagementScheduleWithoutAdapter, SCHEDULE_ENGAGEMENT_WITHOUT_ADAPTER);
                    return false;
                }
            }

            // validate template exists
            var templates = NotificationDal.GetMessageTemplate(partnerId, ApiObjects.MessageTemplateType.Churn);
            if (templates == null || templates.Count == 0)
            {
                log.ErrorFormat("Engagement template wasn't found: {0}", JsonConvert.SerializeObject(engagement));
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.EngagementTemplateNotFound, ENGAGEMENT_TEMPLATE_NOT_FOUND);
                return false;
            }

            // validate coupon group exists
            if (engagement.CouponGroupId == 0 || !PricingDAL.IsCouponGroupExsits(partnerId, engagement.CouponGroupId))
            {
                log.ErrorFormat("Coupon group ID wasn't found: {0}", JsonConvert.SerializeObject(engagement));
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.InvalidCouponGroup, COUPON_GROUP_NOT_FOUND);
                return false;
            }

            return true;
        }

        public static bool AddEngagementToQueue(int groupId, long startTime, int engagementId, int engagementBulkId = 0)
        {
            bool res = true;

            var queue = new EngagementQueue();
            var queueData = new EngagementData(groupId, startTime, engagementId, engagementBulkId)
            {
                ETA = DateUtils.UtcUnixTimestampSecondsToDateTime(startTime)
            };

            bool enqueueResult = queue.Enqueue(queueData, ROUTING_KEY_ENGAGEMENTS);

            if (enqueueResult)
                log.DebugFormat("Successfully inserted engagement message to queue: {0}", queueData);
            else
                log.ErrorFormat("Error while inserting engagement {0} to queue", queueData);

            return res;
        }

        internal static Status DeleteEngagement(int groupId, int id)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            try
            {
                //check Engagement exist
                Engagement engagement = EngagementDal.GetEngagement(groupId, id);
                if (engagement == null || engagement.Id <= 0)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.EngagementNotExist, ENGAGEMENT_NOT_EXIST);
                    return response;
                }

                bool isSet = EngagementDal.DeleteEngagement(groupId, id);
                if (isSet)
                    response = new ApiObjects.Response.Status((int)eResponseStatus.OK, "Engagement deleted");
                else
                    response = new ApiObjects.Response.Status((int)eResponseStatus.EngagementNotExist, ENGAGEMENT_NOT_EXIST);
            }
            catch (Exception ex)
            {
                response = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.Error(string.Format("Failed to delete engagement. Group ID: {0}, engagement ID: {1}", groupId, id), ex);
            }
            return response;
        }

        internal static EngagementResponseList GetEngagements(int groupId, List<eEngagementType> engagementTypes, DateTime? sendTimeLessThanOrEqual)
        {
            EngagementResponseList response = new EngagementResponseList();

            if (engagementTypes == null || engagementTypes.Count == 0)
            {
                engagementTypes = new List<eEngagementType>();
                engagementTypes = Enum.GetValues(typeof(eEngagementType)).Cast<eEngagementType>().ToList();
            }

            try
            {
                response.Engagements = EngagementDal.GetEngagementList(groupId, sendTimeLessThanOrEqual, engagementTypes);
                if (response.Engagements == null || response.Engagements.Count == 0)
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, "No engagement were found");
                else
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            }
            catch (Exception ex)
            {
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.Error(string.Format("Failed to get engagements. groupID: {0}", groupId), ex);
            }

            return response;
        }

        internal static EngagementResponse GetEngagement(int groupId, int id)
        {
            EngagementResponse response = new EngagementResponse();
            try
            {
                response.Engagement = EngagementDal.GetEngagement(groupId, id);
                if (response.Engagement == null)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.EngagementAdapterNotExist, "Engagement was not found");
                }
                else
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
            }
            catch (Exception ex)
            {
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.Error(string.Format("Failed groupID: {0}, engagement ID: {1}", groupId, id), ex);
            }

            return response;
        }

        internal static bool SendEngagement(int partnerId, int engagementId, long startTime)
        {
            DateTime utcNow = DateTime.UtcNow;

            // Get all engagements from the last hour forward
            List<Engagement> lastHourAndFutureEngagement = EngagementDal.GetEngagementList(partnerId, utcNow.AddHours(-1));
            if (lastHourAndFutureEngagement == null || lastHourAndFutureEngagement.Count == 0)
            {
                log.ErrorFormat("No Engagements were found in DB. Engagement ID: {0}", engagementId);
                return false;
            }

            // get relevant engagement 
            Engagement engagementToBeSent = lastHourAndFutureEngagement.FirstOrDefault(x => x.Id == engagementId);
            if (engagementToBeSent == null)
            {
                log.ErrorFormat("Engagement was not found in DB. Engagement ID: {0}", engagementId);
                return false;
            }

            // validate engagement time is the same as message time 
            if (Math.Abs(DateUtils.DateTimeToUtcUnixTimestampSeconds(engagementToBeSent.SendTime) - startTime) > 30)
            {
                log.ErrorFormat("Engagement time was changed (to more than 30 seconds). Engagement ID: {0}, engagement time: {1}, message time: {2}",
                    engagementId,
                    engagementToBeSent.SendTime,
                    DateUtils.UtcUnixTimestampSecondsToDateTime(startTime));

                // return true - do not retry
                return true;
            }

            // validate same engagement was not already sent in the last hour
            Engagement engagementAlreadySent = lastHourAndFutureEngagement.FirstOrDefault(x => x.Id != engagementId &&
                                                                                               x.EngagementType == engagementToBeSent.EngagementType &&
                                                                                               x.AdapterId == engagementToBeSent.AdapterId &&
                                                                                               x.AdapterDynamicData == engagementToBeSent.AdapterDynamicData &&
                                                                                               x.UserList == engagementToBeSent.UserList &&
                                                                                               x.SendTime > engagementToBeSent.SendTime.AddHours(-1) &&
                                                                                               x.SendTime < engagementToBeSent.SendTime);
            if (engagementAlreadySent != null)
            {
                log.ErrorFormat("Engagement was already sent in the last hour. Sent engagement: {0}, my (canceled) engagement: {1}",
                 JsonConvert.SerializeObject(engagementAlreadySent),
                 JsonConvert.SerializeObject(engagementToBeSent));

                // return true - do not retry
                return true;
            }

            // check if user list exist or get them from external adapter
            List<int> userList = new List<int>();
            if (engagementToBeSent.AdapterId > 0)
            {
                // get adapter 
                EngagementAdapter engagementAdapter = EngagementDal.GetEngagementAdapter(partnerId, engagementToBeSent.AdapterId);
                if (engagementAdapter == null)
                {
                    log.ErrorFormat("Engagement adapter wasn't found. Engagement: {0}", JsonConvert.SerializeObject(engagementToBeSent));

                    // return true - do not retry
                    return true;
                }

                // user list exists
                userList = EngagementAdapterClient.GetAdapterList(partnerId, engagementAdapter, engagementToBeSent.AdapterDynamicData);
                if (userList == null || userList.Count == 0)
                {
                    log.ErrorFormat("No users were received from adapter. Engagement: {0}, Adapter: {1}", JsonConvert.SerializeObject(engagementToBeSent), JsonConvert.SerializeObject(engagementAdapter));

                    // return true - do not retry
                    return true;
                }
            }
            else
            {
                // get user list from adapter
                userList = engagementToBeSent.UserList.Split(';', ',', '|').Select(p => Convert.ToInt32(p.Trim())).ToList();
                if (userList == null || userList.Count == 0)
                {
                    log.ErrorFormat("Error getting user list from engagement. Engagement: {0}", JsonConvert.SerializeObject(engagementToBeSent));

                    // return true - do not retry
                    return true;
                }
            }

            // if scheduler engagement - create next iteration
            if (engagementToBeSent.IntervalSeconds > 0)
            {
                if (!HandleSchedularEngagement(partnerId, lastHourAndFutureEngagement, engagementToBeSent))
                    return true;
            }

            // update engagement table with number of users
            engagementToBeSent.TotalNumberOfRecipients = userList.Count;
            if (EngagementDal.SetEngagement(partnerId, engagementToBeSent) == null)
            {
                log.ErrorFormat("Error update engagement with number of users. Engagement: {0}", JsonConvert.SerializeObject(engagementToBeSent));

                // return true - do not retry
                return true;
            }

            // get bulk from TCM

            int engagementBulkMessages = ApplicationConfiguration.Current.EngagementsConfiguration.NumberOfBulkMessageEngagements.Value;
            if (engagementBulkMessages == 0)
                engagementBulkMessages = NUM_OF_BULK_MESSAGE_ENGAGEMENTS;

            // calculate number of iterations
            int remainder = 0;
            long numOfBulkMessages = Math.DivRem(userList.Count, engagementBulkMessages, out remainder);
            if (remainder > 0)
                numOfBulkMessages++;

            // get number of allowed threads
            int numberOfEngagementThread = ApplicationConfiguration.Current.EngagementsConfiguration.NumberOfEngagementThreads.Value;
            if (numberOfEngagementThread == 0)
                numberOfEngagementThread = NUM_OF_ENGAGEMENT_THREADS;

            Parallel.For(0, numOfBulkMessages, new ParallelOptions() { MaxDegreeOfParallelism = numberOfEngagementThread }, index =>
            {
                // create bulk message
                EngagementBulkMessage bulkMessage = new EngagementBulkMessage()
                {
                    EngagementId = engagementToBeSent.Id,
                    IsSent = false,
                    IterationSize = engagementBulkMessages,
                    IterationOffset = (int)index * engagementBulkMessages
                };

                // insert into DB
                EngagementBulkMessage insertedBulkMessage = EngagementDal.InsertEngagementBulkMessage(partnerId, bulkMessage);
                if (insertedBulkMessage == null || insertedBulkMessage.Id == 0)
                    log.ErrorFormat("Error inserting bulk message in DB. Engagement: {0}, bulk message: {1}", JsonConvert.SerializeObject(engagementToBeSent), JsonConvert.SerializeObject(bulkMessage));
                else
                {
                    // insert to CB
                    for (long i = index * engagementBulkMessages; i < index * engagementBulkMessages + engagementBulkMessages; i++)
                    {
                        if (i == userList.Count)
                            break;

                        // insert create user engagement message to CB
                        UserEngagement userEngagement = new UserEngagement(partnerId, userList[(int)i], engagementToBeSent.Id, insertedBulkMessage.Id);
                        if (!EngagementDal.SetUserEngagement(userEngagement))
                            log.ErrorFormat("Error inserting user engagement message into CB. User message: {0}", JsonConvert.SerializeObject(userEngagement));
                    }

                    // create rabbit message
                    if (!AddEngagementToQueue(partnerId, DateUtils.DateTimeToUtcUnixTimestampSeconds(utcNow), engagementToBeSent.Id, insertedBulkMessage.Id))
                        log.ErrorFormat("Error while trying to create bulk engagement rabbit message. engagement data: {0}", JsonConvert.SerializeObject(insertedBulkMessage));
                }
            });

            return true;
        }

        private static bool HandleSchedularEngagement(int partnerId, List<Engagement> lastHourAndFutureEngagement, Engagement engagementToBeSent)
        {
            // validate there isn't another one in the future 
            Engagement futureEngagement = lastHourAndFutureEngagement.FirstOrDefault(x => x.Id != engagementToBeSent.Id &&
                                                                                    x.EngagementType == engagementToBeSent.EngagementType &&
                                                                                    x.AdapterId == engagementToBeSent.AdapterId &&
                                                                                    x.AdapterDynamicData == engagementToBeSent.AdapterDynamicData &&
                                                                                    x.IntervalSeconds > 0 &&
                                                                                    x.SendTime > engagementToBeSent.SendTime);

            if (futureEngagement != null)
            {
                log.ErrorFormat("Scheduler engagement was canceled - future scheduler was detected. cancelled engagement: {0}, future engagement: {1}",
                    JsonConvert.SerializeObject(engagementToBeSent),
                    JsonConvert.SerializeObject(futureEngagement));

                return false;
            }

            // validate adapter ID exists
            if (engagementToBeSent.AdapterId == 0)
            {
                log.ErrorFormat("Scheduled adapter must use an engagement adapter. engagement entered: {0}", JsonConvert.SerializeObject(engagementToBeSent));
                return false;
            }

            // create next iteration
            futureEngagement = new Engagement()
            {
                AdapterDynamicData = engagementToBeSent.AdapterDynamicData,
                AdapterId = engagementToBeSent.AdapterId,
                EngagementType = engagementToBeSent.EngagementType,
                IntervalSeconds = engagementToBeSent.IntervalSeconds,
                SendTime = engagementToBeSent.SendTime.AddSeconds(engagementToBeSent.IntervalSeconds),
                CouponGroupId = engagementToBeSent.CouponGroupId,
                UserList = engagementToBeSent.UserList
            };

            // insert to DB
            Engagement tempInsertedFutureEngagement = EngagementDal.InsertEngagement(partnerId, futureEngagement);
            if (tempInsertedFutureEngagement == null)
            {
                log.ErrorFormat("Error while trying to create next engagement iteration in DB. engagement data: {0}", JsonConvert.SerializeObject(futureEngagement));
                return false;
            }

            futureEngagement = tempInsertedFutureEngagement;

            // create engagement Rabbit message
            if (!AddEngagementToQueue(partnerId, DateUtils.DateTimeToUtcUnixTimestampSeconds(futureEngagement.SendTime), futureEngagement.Id))
            {
                log.ErrorFormat("Error while trying to create next engagement iteration in DB. engagement data: {0}", JsonConvert.SerializeObject(futureEngagement));

                // remove new engagement 
                if (!EngagementDal.DeleteEngagement(partnerId, futureEngagement.Id))
                    log.ErrorFormat("Error while trying to delete engagement after failed to create a rabbit message. engagement data: {0}", JsonConvert.SerializeObject(futureEngagement));

                return false;
            }
            return true;
        }

        internal static bool SendEngagementBulk(int partnerId, int engagementId, int engagementBulkId, long startTime)
        {
            // get relevant engagement  
            Engagement engagement = EngagementDal.GetEngagement(partnerId, engagementId);
            if (engagement == null)
            {
                log.ErrorFormat("Engagement was not found in DB. Engagement ID: {0}", engagementId);
                return false;
            }

            // get relevant engagement bulk message   
            EngagementBulkMessage engagementBulkMessage = EngagementDal.GetEngagementBulkMessage(partnerId, engagementBulkId);
            if (engagementBulkMessage == null)
            {
                log.ErrorFormat("EngagementBulkMessage was not found in DB. engagementBulkId: {0}", engagementBulkId);
                return false;
            }

            // get relevant usersengagementBulkMessage  
            List<UserEngagement> userEngagements = EngagementDal.GetBulkUserEngagementView(engagementId, engagementBulkId);
            if (userEngagements == null || userEngagements.Count == 0)
            {
                log.ErrorFormat("No User engagement found. engagementBulkId: {0}", engagementBulkId);
                return false;
            }

            // remove already engaged users 
            userEngagements.RemoveAll(x => x.IsEngagementSent);

            // generate coupon according to user count 
            Status status = null;
            List<Coupon> coupons = Core.Pricing.Module.GenerateCoupons(partnerId, userEngagements.Count, engagement.CouponGroupId, out status);
            if (coupons == null || coupons.Count != userEngagements.Count)
            {
                log.ErrorFormat("Number of coupons not equal to users number. engagementBulkId: {0}, requested coupons: {1}, received: {2} ",
                    engagementBulkId,
                    userEngagements.Count,
                    coupons != null ? coupons.Count.ToString() : "null");
                return false;
            }

            // get partner notifications settings  
            var partnerSettings = NotificationSettings.GetPartnerNotificationSettings(partnerId);
            if (partnerSettings == null && partnerSettings.settings != null)
            {
                log.ErrorFormat("Could not find partner notification settings. Partner ID: {0}", partnerId);
                return false;
            }

            // get message templates  
            MessageTemplate messageTemplate = GetMessageTemplate(partnerId, engagement.EngagementType);

            if (messageTemplate == null)
                log.ErrorFormat("churn message template was not found. group: {0}", partnerId);

            // stop process  if push = true and template empty 
            if (NotificationSettings.IsPartnerPushEnabled(partnerId) && (messageTemplate == null || string.IsNullOrEmpty(messageTemplate.Message)))
            {
                log.ErrorFormat("Stop process! partentSettings push is enabled but template empty. group: {0}", partnerId);
                return true;
            }

            // get number of engagements threads
            int numberOfEngagementThread = ApplicationConfiguration.Current.EngagementsConfiguration.NumberOfEngagementThreads.Value;
            if (numberOfEngagementThread == 0)
                numberOfEngagementThread = NUM_OF_ENGAGEMENT_THREADS;

            // send mail and inbox message in parallel
            List<UserEngagement> successfullySentEngagementUsers = new List<UserEngagement>();

            log.DebugFormat("Start parallel SendMailInboxEngagement for groupId: {0}, bulkId:{1}, userEngagements.Count:{2}",
                partnerId, engagementBulkId, userEngagements.Count);

            Parallel.For(0, userEngagements.Count, new ParallelOptions() { MaxDegreeOfParallelism = numberOfEngagementThread }, userIndex =>
            {
                if (SendMailInboxEngagement(partnerId, userEngagements[userIndex], coupons[userIndex].code, partnerSettings.settings, messageTemplate, engagement.EngagementType))
                    successfullySentEngagementUsers.Add(userEngagements[userIndex]);
            });

            log.DebugFormat("End parallel SendMailInboxEngagement for groupId: {0}, bulkId:{1}, userEngagements.Count:{2}",
              partnerId, engagementBulkId, userEngagements.Count);

            // update bulk engagement
            engagementBulkMessage.IsSent = true;
            if (EngagementDal.SetEngagementBulkMessage(partnerId, engagementBulkMessage) == null)
                log.ErrorFormat("Error occurred while updating bulk engagement message. GID: {0}, engagementId: {1}, engagementBulkId: {2}", partnerId, engagementId, engagementBulkId);

            // 3. send push
            if (!SendPushEngagement(partnerId, successfullySentEngagementUsers, messageTemplate, engagement.EngagementType, engagementId))
            {
                log.ErrorFormat("Error occurred while trying to send push engagement. GID: {0}, engagementId: {1}, engagementBulkId: {2}", partnerId, engagementId, engagementBulkId);
                return true;
            }
            else
                log.DebugFormat("Engagement bulk push message was finished. Partner ID: {0}, engagementBulkId: {1}", partnerId, engagementBulkId);

            return true;
        }

        private static MessageTemplate GetMessageTemplate(int partnerId, eEngagementType eEngagementType)
        {
            MessageTemplate engagementTemplate = null;

            List<MessageTemplate> messageTemplates = NotificationCache.Instance().GetMessageTemplates(partnerId);
            if (messageTemplates != null)
                engagementTemplate = messageTemplates.FirstOrDefault(x => x.TemplateType == GetEngagementMessageTemplateType(eEngagementType));

            return engagementTemplate;
        }

        private static MessageTemplateType GetEngagementMessageTemplateType(eEngagementType engagementType)
        {
            switch (engagementType)
            {
                case eEngagementType.Churn:
                    return MessageTemplateType.Churn;
                default:
                    throw new Exception("Unknown EngagementType Type");
            }
        }

        private static bool SendPushEngagement(int partnerId, List<UserEngagement> engagementUsers, MessageTemplate messageTemplate, eEngagementType engagementType, int engagementId)
        {
            // 3. send push              
            bool docExists = false;
            List<EndPointData> usersEndPointDatas = new List<EndPointData>();
            // get user notifications 
            UserNotification userNotificationData = null;
            EndPointData userEndPointData = null;
            foreach (UserEngagement userEngagement in engagementUsers)
            {
                userNotificationData = DAL.NotificationDal.GetUserNotificationData(partnerId, userEngagement.UserId, ref docExists);
                if (userNotificationData == null)
                {
                    log.ErrorFormat("Error retrieving User notification data. partnerId: {0}, UserId: {1}", partnerId, userEngagement.UserId);
                    continue;
                }

                if (userNotificationData.devices == null || userNotificationData.devices.Count == 0)
                {
                    log.ErrorFormat("No devices for user. partnerId: {0}, UserId: {1}", partnerId, userEngagement.UserId);
                    continue;
                }

                foreach (var device in userNotificationData.devices)
                {
                    DeviceNotificationData deviceData = NotificationDal.GetDeviceNotificationData(partnerId, device.Udid, ref docExists);
                    if (deviceData == null)
                    {
                        log.DebugFormat("device data wasn't found. GID: {0}, UDID: {1}, userId: {2}", partnerId, device.Udid, userEngagement.UserId);
                        continue;
                    }

                    var pushData = PushAnnouncementsHelper.GetPushData(partnerId, device.Udid, string.Empty);
                    if (pushData == null)
                    {
                        log.ErrorFormat("push data wasn't found. GID: {0}, UDID: {1}, userId: {2}", partnerId, device.Udid, userEngagement.UserId);
                        continue;
                    }

                    userEndPointData = new EndPointData()
                    {
                        Attributes = GetUserAttributes(partnerId, userEngagement.UserId, engagementType),
                        EndPointArn = pushData.ExternalToken,
                        Category = messageTemplate.Action,
                        Sound = messageTemplate.Sound,
                        Url = messageTemplate.URL,
                        ExtraData = userEngagement.UserId.ToString()
                    };

                    usersEndPointDatas.Add(userEndPointData);
                }
            }

            WSEndPointPublishData publishData = new WSEndPointPublishData();
            publishData.EndPoints = usersEndPointDatas;
            publishData.Message = new MessageData()
            {
                Alert = messageTemplate.Message,
                Url = messageTemplate.URL,
                Category = messageTemplate.Action,
                Sound = messageTemplate.Sound
            };


            List<WSEndPointPublishDataResult> pushPublishResults = NotificationAdapter.PublishToEndPoint(partnerId, publishData);
            if (pushPublishResults == null)
            {
                log.ErrorFormat("Error at PublishToEndPoint. GID: {0}, engagementId: {1}", partnerId, engagementId);
                return false;
            }

            foreach (var pushPublishResult in pushPublishResults)
            {
                // connect user document to result document
                UserEngagement userEngagement = engagementUsers.FirstOrDefault(x => x.UserId.ToString() == pushPublishResult.ExtraData);
                int userID = 0;
                if (userEngagement != null)
                    userID = userEngagement.UserId;

                if (string.IsNullOrEmpty(pushPublishResult.ResultMessageId))
                {
                    log.ErrorFormat("Error occur at PublishToEndPoint. GID: {0}, user ID: {1}, EndPointArn: {2}",
                        partnerId,
                        userEngagement != null ? userEngagement.UserId : 0,
                        pushPublishResult.EndPointArn);
                }
                else
                {
                    // update user document with result push ID
                    if (userEngagement != null)
                    {
                        userEngagement.ResultPushIds.Add(pushPublishResult.ResultMessageId);
                        if (!EngagementDal.SetUserEngagement(userEngagement))
                        {
                            log.ErrorFormat("Error occurred while updating user engagement data with result push token. GID: {0}, user ID: {1}, EndPointArn: {2}",
                                partnerId,
                                userEngagement.UserId,
                                pushPublishResult.EndPointArn);
                        }
                    }
                }
            }

            return true;
        }

        public static Status SendPushToUser(int partnerId, int userId, PushMessage pushMessage)
        {
            var result = new Status() { Code = (int)eResponseStatus.Error };
            int allowedPushMsg = ApplicationConfiguration.Current.PushMessagesConfiguration.NumberOfMessagesPerSecond.Value;
            if (ExceededMaximumAllowedPush(partnerId, userId, allowedPushMsg))
            {
                log.ErrorFormat("Cannot send user push notification. maximum number of push allowed per hour: {0}, partner ID: {1}, user ID: {2}", allowedPushMsg, partnerId, userId);
                result = new Status() { Code = (int)eResponseStatus.Error, Message = MAX_NUMBER_OF_PUSH_MSG_EXCEEDED };
                return result;
            }

            if (!string.IsNullOrEmpty(pushMessage.Udid))//send to a specific device
            {
                result = SendToSingleDevice(partnerId, userId, pushMessage);
            }
            else
            {
                result = SendAllConnectedDevices(partnerId, userId, pushMessage);
            }

            return result;
        }

        private static Status SendSingleSnsDevice(int partnerId, int userId, PushMessage pushMessage, string udid)
        {
            var result = new Status() { Code = (int)eResponseStatus.Error };
            var docExists = false;
            var usersEndPointDatas = new List<EndPointData>();

            // get device data
            DeviceNotificationData deviceData = NotificationDal.GetDeviceNotificationData(partnerId, udid, ref docExists);
            if (deviceData == null)
            {
                log.DebugFormat($"device data wasn't found. GID: {partnerId}, UDID: {udid}, userId: {userId}");
                result.Message = "device data wasn't found";
                return result;
            }

            // get push data
            var pushData = PushAnnouncementsHelper.GetPushData(partnerId, udid, string.Empty);
            if (pushData == null)
            {
                log.ErrorFormat("push data wasn't found. GID: {0}, UDID: {1}, userId: {2}", partnerId, udid, userId);
                result.Message = "push data wasn't found";
                return result;
            }

            // prepare push
            usersEndPointDatas.Add(new EndPointData()
            {
                EndPointArn = pushData.ExternalToken,
                Category = pushMessage.Action,
                Sound = pushMessage.Sound,
                Url = pushMessage.Url,
                ExtraData = userId.ToString()
            });

            result = PushSns(partnerId, userId, pushMessage, usersEndPointDatas, result);

            return result;
        }

        private static Status SendAllConnectedDevices(int partnerId, int userId, PushMessage pushMessage)
        {
            var result = new Status() { Code = (int)eResponseStatus.Error };
            var usersEndPointDatas = new List<EndPointData>();

            // get user notification data
            bool docExists = false;
            UserNotification userNotificationData = DAL.NotificationDal.GetUserNotificationData(partnerId, userId, ref docExists);
            if (userNotificationData == null)
            {
                log.ErrorFormat("Error retrieving User notification data. partnerId: {0}, UserId: {1}, message: {2}", partnerId, userId, JsonConvert.SerializeObject(pushMessage));
                result = new Status() { Code = (int)eResponseStatus.Error };
                return result;
            }

            // validate user has devices
            if (userNotificationData.devices == null || userNotificationData.devices.Count == 0)
            {
                log.ErrorFormat("No devices for user. partnerId: {0}, UserId: {1}, message: {2}", partnerId, userId, JsonConvert.SerializeObject(pushMessage));
                result = new Status() { Code = (int)eResponseStatus.Error };
                return result;
            }

            foreach (var device in userNotificationData.devices)
            {
                if (pushMessage.PushChannels.Contains(PushChannel.Iot) || device.PushChannel == PushChannel.Iot)
                {
                    var iotDeviceConfiguration = GetIotUserConfiguration(partnerId, pushMessage, device, userId);
                    if (iotDeviceConfiguration != null && iotDeviceConfiguration.Status == iot.Status.Success)
                    {
                        PublishToIot(partnerId, pushMessage, iotDeviceConfiguration);
                    }
                }
                else if (IsToSns(pushMessage, device))
                {
                    // get device data
                    DeviceNotificationData deviceData = NotificationDal.GetDeviceNotificationData(partnerId, device.Udid, ref docExists);
                    if (deviceData == null)
                    {
                        log.DebugFormat($"device data wasn't found. GID: {partnerId}, UDID: {device.Udid}, userId: {userId}");
                        continue;
                    }

                    // get push data
                    var pushData = PushAnnouncementsHelper.GetPushData(partnerId, device.Udid, string.Empty);
                    if (pushData == null)
                    {
                        log.ErrorFormat("push data wasn't found. GID: {0}, UDID: {1}, userId: {2}", partnerId, device.Udid, userId);
                        continue;
                    }

                    // prepare push
                    usersEndPointDatas.Add(new EndPointData()
                    {
                        EndPointArn = pushData.ExternalToken,
                        Category = pushMessage.Action,
                        Sound = pushMessage.Sound,
                        Url = pushMessage.Url,
                        ExtraData = userId.ToString()
                    });
                }
            }

            if (usersEndPointDatas.Count > 0)
            {
                return PushSns(partnerId, userId, pushMessage, usersEndPointDatas, result);
            }

            result = new Status() { Code = (int)eResponseStatus.OK };

            return result;
        }

        private static Status PushSns(int partnerId, int userId, PushMessage pushMessage, List<EndPointData> usersEndPointDatas, Status result)
        {
            // prepare push 
            WSEndPointPublishData publishData = new WSEndPointPublishData();
            publishData.EndPoints = usersEndPointDatas;
            publishData.Message = new MessageData()
            {
                Alert = pushMessage.Message,
                Url = pushMessage.Url,
                Category = pushMessage.Action,
                Sound = pushMessage.Sound
            };

            // send push
            List<WSEndPointPublishDataResult> pushPublishResults = NotificationAdapter.PublishToEndPoint(partnerId, publishData);
            if (pushPublishResults == null)
            {
                log.ErrorFormat("Error at PublishToEndPoint. GID: {0}, user ID: {1}, message: {2}", partnerId, userId, JsonConvert.SerializeObject(pushMessage));
                result = new Status() { Code = (int)eResponseStatus.Error };
                return result;
            }

            // go over push results
            foreach (var pushPublishResult in pushPublishResults)
            {
                if (string.IsNullOrEmpty(pushPublishResult.ResultMessageId))
                    log.ErrorFormat("Error occur at PublishToEndPoint. GID: {0}, user ID: {1}, EndPointArn: {2}, message: {3}", partnerId, userId, pushPublishResult.EndPointArn, JsonConvert.SerializeObject(pushMessage));
                else
                    log.DebugFormat("Successfully sent push message. GID: {0}, user ID: {1}, EndPointArn: {2}, message: {3}", partnerId, userId, pushPublishResult.EndPointArn, JsonConvert.SerializeObject(pushMessage));
            }

            return new Status() { Code = (int)eResponseStatus.OK };
        }

        private static bool IsToSns(PushMessage pushMessage, UserDevice device)
        {
            return (pushMessage.PushChannels == null || pushMessage.PushChannels.Contains(PushChannel.Push))
                                && (device.PushChannel == default || device.PushChannel == PushChannel.Push);
        }

        private static GetClientConfigurationResponse GetIotUserConfiguration(int groupId, PushMessage pushMessage, UserDevice device, int userId)
        {
            long domainId = UsersCache.Instance().GetDomainIdByUser(userId, groupId);
            var regionId = DomainsCache.Instance().GetDomain((int) domainId, groupId).m_nRegion;
            return IotGrpcClientWrapper.IotClient.Instance.GetClientConfiguration(groupId, domainId, regionId, device.Udid);
        }

        private static Status SendToSingleDevice(int partnerId, int userId, PushMessage pushMessage)
        {
            var status = new Status() { Code = (int)eResponseStatus.Error };

            var response = Core.Users.Module.GetUserData(partnerId, userId.ToString(), string.Empty);
            if (response == null || response.m_RespStatus != ResponseStatus.OK || response.m_user == null || response.m_user.m_oBasicData == null)
            {
                var _response = response != null ? JsonConvert.SerializeObject(response) : "null";
                log.Error($"User not found or is corrupted, partner: {partnerId}," +
                    $"User: {userId}, response: {_response}");
                return null;
            }

            var devices = Api.api.Instance.GetDomainDevices(response.m_user.m_domianID, partnerId);
            if (devices == null || !devices.ContainsKey(pushMessage.Udid))
            {
                log.Error($"{DEVICE_NOT_IN_DOMAIN}, device: {pushMessage.Udid} Domain: {response.m_user.m_domianID}");
                status = new Status() { Code = (int)eResponseStatus.Error, Message = DEVICE_NOT_IN_DOMAIN };
                return status;
            }

            var domain = DomainsCache.Instance().GetDomain((int) response.m_user.m_domianID, partnerId);
            var device = IotGrpcClientWrapper.IotClient.Instance.GetClientConfiguration(partnerId, domain.m_nDomainID, domain.m_nRegion, pushMessage.Udid);
            if (device != null)
            {
                status = PublishToIot(partnerId, pushMessage, device);
            }
            else
            {
                status = SendSingleSnsDevice(partnerId, userId, pushMessage, pushMessage.Udid);
            }

            return status;
        }

        private static bool ExceededMaximumAllowedPush(int partnerId, int userId, int allowedPushMsg)
        {
            // get maximum allowed push 
            if (allowedPushMsg == 0)
                allowedPushMsg = MAX_PUSH_MSG_PER_SECONDS;

            int counter;

            // check if user document exists
            if (NotificationDal.IsUserPushDocExists(partnerId, userId))
                counter = (int)NotificationDal.IncreasePushCounter(partnerId, userId, false);
            else
                counter = (int)NotificationDal.IncreasePushCounter(partnerId, userId, true);

            // validate did not reach maximum of allowed user push messages
            return counter > allowedPushMsg;
        }

        private static Status PublishToIot(int partnerId, PushMessage pushMessage, GetClientConfigurationResponse iotDevice)
        {
            if (!NotificationAdapter.AddPrivateMessageToShadowIot(partnerId, pushMessage.Message, iotDevice.ThingArn, iotDevice.ThingName))
            {
                log.Error($"Can't update thing's shadow. thing: {iotDevice.ThingArn}");
                return new Status() { Code = (int)eResponseStatus.Error, Message = FAILED_TO_UPDATE_THING_SHADOW }; ;
            }

            return new Status() { Code = (int)eResponseStatus.OK };
        }

        private static List<KeyValue> GetUserAttributes(int partnerId, int userId, eEngagementType engagementType)
        {
            List<KeyValue> userAttributes = new List<KeyValue>();

            Core.Users.UserResponseObject response = Core.Users.Module.GetUserData(partnerId, userId.ToString(), string.Empty);
            if (response == null || response.m_RespStatus != ResponseStatus.OK || response.m_user == null || response.m_user.m_oBasicData == null)
            {
                log.ErrorFormat("Could not send engagement - User invalid. partnerId: {0}, userId: {1}, userObj: {2}", partnerId, userId, response != null ? JsonConvert.SerializeObject(response) : "null");
                return null;
            }

            switch (engagementType)
            {
                case eEngagementType.Churn:
                    userAttributes.Add(new KeyValue() { Key = eChurnPlaceHolders.FirstName.ToString(), Value = response.m_user.m_oBasicData.m_sFirstName });
                    userAttributes.Add(new KeyValue() { Key = eChurnPlaceHolders.LastName.ToString(), Value = response.m_user.m_oBasicData.m_sLastName });
                    break;
                default:
                    throw new Exception("Unknown Engagement Type");
            }

            return userAttributes;
        }

        internal static bool SendMailInboxEngagement(int partnerId, UserEngagement userEngagement, string couponCode,
            NotificationPartnerSettings notificationPartnerSettings, MessageTemplate messageTemplate, eEngagementType engagementType)
        {
            Core.Users.UserResponseObject dbUserData = Core.Users.Module.GetUserData(partnerId, userEngagement.UserId.ToString(), string.Empty);
            if (dbUserData == null || dbUserData.m_RespStatus != ResponseStatus.OK || dbUserData.m_user == null || dbUserData.m_user.m_oBasicData == null)
            {
                log.ErrorFormat("Could not send engagement - User invalid. partnerId: {0}, userId: {1}, userObj: {2}", partnerId, userEngagement.UserId, dbUserData != null ? JsonConvert.SerializeObject(dbUserData) : "null");
                return false;
            }

            if (string.IsNullOrEmpty(dbUserData.m_user.m_oBasicData.m_sEmail))
            {
                log.ErrorFormat("Error: user email is empty. partnerId: {0}, userId: {1}", partnerId, userEngagement.UserId);
                return false;
            }

            MailRequestObj mailRequest = null;

            switch (engagementType)
            {
                case eEngagementType.Churn:
                    ChurnMailRequest churnMailRequest = new ChurnMailRequest()
                    {
                        CouponCode = couponCode,
                        m_sTemplateName = notificationPartnerSettings.ChurnMailTemplateName,
                        m_sSubject = notificationPartnerSettings.ChurnMailSubject,
                        m_sSenderFrom = notificationPartnerSettings.SenderEmail,
                        m_sSenderName = notificationPartnerSettings.MailSenderName,
                        m_sFirstName = dbUserData.m_user.m_oBasicData.m_sFirstName,
                        m_sLastName = dbUserData.m_user.m_oBasicData.m_sLastName,
                        m_sSenderTo = dbUserData.m_user.m_oBasicData.m_sEmail
                    };
                    mailRequest = churnMailRequest;
                    break;
                default:
                    throw new Exception("Unknown Engagement Type");
            }

            // send mail  
            if (!Core.Api.Module.SendMailTemplate(partnerId, mailRequest))
            {
                log.ErrorFormat("Error while sending mail engagement. partnerId: {0}, mail request: {1}", partnerId, JsonConvert.SerializeObject(mailRequest));
                return false;
            }
            else
                log.DebugFormat("Engagement mail was successfully sent. partner ID: {0}, user ID: {1}", partnerId, userEngagement.UserId);

            // update userEngagement && doc 
            userEngagement.IsEngagementSent = true;
            userEngagement.CouponId = couponCode.ToString();
            if (!EngagementDal.SetUserEngagement(userEngagement))
            {
                log.ErrorFormat("Could not SetUserEngagement. partnerId: {0}, userId: {1}", partnerId, userEngagement.UserId);
                return true;
            }

            // validate partner inbox configuration is enabled  
            if (!NotificationSettings.Instance.IsPartnerInboxEnabled(partnerId))
            {
                log.ErrorFormat("Partner inbox feature is off. GID: {0}, UID: {1}", partnerId, userEngagement.UserId);
                return true;
            }

            if (messageTemplate == null)
            {
                log.ErrorFormat("messageTemplate is empty.GID: {0}, UID: {1}, ", partnerId, userEngagement.UserId);
                return true;
            }
            // build message  
            MessageData messageData = null;

            switch (engagementType)
            {
                case eEngagementType.Churn:
                    messageData = new MessageData()
                    {
                        Category = messageTemplate.Action,
                        Sound = messageTemplate.Sound,
                        Url = messageTemplate.URL.Replace("{" + eChurnPlaceHolders.FirstName + "}", dbUserData.m_user.m_oBasicData.m_sFirstName).
                                                    Replace("{" + eChurnPlaceHolders.LastName + "}", dbUserData.m_user.m_oBasicData.m_sLastName),
                        Alert = messageTemplate.Message.Replace("{" + eChurnPlaceHolders.FirstName + "}", dbUserData.m_user.m_oBasicData.m_sFirstName).
                                                    Replace("{" + eChurnPlaceHolders.LastName + "}", dbUserData.m_user.m_oBasicData.m_sLastName)
                    };
                    break;
                default:
                    throw new Exception("Unknown Engagement Type");
            }

            long currentTimeSec = DateUtils.DateTimeToUtcUnixTimestampSeconds(DateTime.UtcNow);

            InboxMessage inboxMessage = new InboxMessage()
            {
                Category = eMessageCategory.Engagement,
                CreatedAtSec = currentTimeSec,
                Id = Guid.NewGuid().ToString(),
                Message = messageData.Alert,
                State = eMessageState.Unread,
                UpdatedAtSec = currentTimeSec,
                Url = messageData.Url,
                UserId = userEngagement.UserId
            };

            if (!NotificationDal.Instance.SetUserInboxMessage(partnerId, inboxMessage, NotificationSettings.Instance.GetInboxMessageTTLDays(partnerId)))
                log.ErrorFormat("Error while setting churn inbox message. GID: {0}, InboxMessage: {1}", partnerId, JsonConvert.SerializeObject(inboxMessage));
            else
                log.DebugFormat("Engagement inbox message was successfully sent. partner ID: {0}, user ID: {1}", partnerId, userEngagement.UserId);

            return true;
        }

        internal static ApiObjects.Response.Status SetEngagementAdapterConfiguration(int groupId, int engagementAdapterId)
        {
            ApiObjects.Response.Status status = new ApiObjects.Response.Status();

            try
            {
                if (engagementAdapterId == 0)
                {
                    status = ENGAGEMENT_ADAPTER_ID_REQUIRED;
                    return status;
                }

                //get engagementAdapter
                EngagementAdapter engagementAdapter = EngagementDal.GetEngagementAdapter(groupId, engagementAdapterId);

                if (engagementAdapter == null || engagementAdapter.ID <= 0)
                {
                    return ENGAGEMENT_ADAPTER_NOT_EXIST;
                }

                if (EngagementAdapterClient.SendConfigurationToAdapter(groupId, engagementAdapter))
                {
                    status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
                else
                {
                    status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                    log.ErrorFormat("SetEngagementAdapterConfiguration - SendConfigurationToAdapter failed : AdapterID = {0}", engagementAdapter.ID);
                }
            }
            catch (Exception ex)
            {
                status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.ErrorFormat("Failed ex={0}", ex);
            }

            return status;
        }

        internal static bool ReSendEngagement(int partnerId, int engagementId)
        {
            //check Engagement exist
            Engagement engagement = DAL.EngagementDal.GetEngagement(partnerId, engagementId);
            if (engagement == null || engagement.Id <= 0)
            {
                log.ErrorFormat("Engagement wasn't found. ID: {0}", engagementId);
                return false;
            }

            // get all corresponding bulk engagements
            List<EngagementBulkMessage> bulkEngagements = EngagementDal.GetEngagementBulkMessages(partnerId, engagement.Id);
            if (bulkEngagements == null || bulkEngagements.Count == 0)
            {
                log.ErrorFormat("Engagement bulk messages were not found. ID: {0}", engagementId);
                return false;
            }

            // create rabbit message for each bulk message
            foreach (var bulk in bulkEngagements)
            {
                // create rabbit message
                if (!AddEngagementToQueue(partnerId, DateUtils.DateTimeToUtcUnixTimestampSeconds(DateTime.UtcNow), engagement.Id, bulk.Id))
                    log.ErrorFormat("Error while trying to create bulk engagement rabbit message. engagement data: {0}", JsonConvert.SerializeObject(bulk));
            }

            return true;
        }

        public static GenericResponse<string> GetPhoneNumberFromUser(int groupId, int userId)
        {
            var result = new GenericResponse<string>();
            bool docExists = false;
            UserNotification userNotificationData = DAL.NotificationDal.GetUserNotificationData(groupId, userId, ref docExists);
            if (userNotificationData == null)
            {
                log.DebugFormat("user notification data wasn't found. sending SMS by user data GID: {0}, userId: {2}", groupId, userId);
                Users.UserResponseObject response = Core.Users.Module.GetUserData(groupId, userId.ToString(), string.Empty);
                if (response == null || response.m_RespStatus != ApiObjects.ResponseStatus.OK || response.m_user == null)
                {
                    log.ErrorFormat("Failed to get user data for userId = {0}", userId);
                    result.SetStatus(eResponseStatus.Error, "Failed to get user data");
                    return result;
                }
                else
                {
                    result.Object = response.m_user.m_oBasicData.m_sPhone;
                }
            }
            else if (userNotificationData.Settings.EnableSms == true)
            {
                result.Object = userNotificationData.UserData.PhoneNumber;
            }

            return result;
        }

        internal static Status SendSmsToUser(int groupId, int userId, string message, string phoneNumber)
        {
            Status result = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            if (!NotificationSettings.IsPartnerSmsNotificationEnabled(groupId))
            {
                result.Message = "SMS is disabled for partner";
                return result;
            }

            // get maximum allowed push 
            int allowedPushMsg = ApplicationConfiguration.Current.PushMessagesConfiguration.NumberOfMessagesPerSecond.Value;
            if (allowedPushMsg == 0)
                allowedPushMsg = MAX_PUSH_MSG_PER_SECONDS;

            int counter = 0;

            // check if user document exists
            if (NotificationDal.IsUserPushDocExists(groupId, userId))
                counter = (int)NotificationDal.IncreasePushCounter(groupId, userId, false);
            else
                counter = (int)NotificationDal.IncreasePushCounter(groupId, userId, true);

            // validate did not reach maximum of allowed user push messages
            if (counter > allowedPushMsg)
            {
                log.ErrorFormat("Cannot send user SMS notification. maximum number of SMS allowed per hour: {0}, partner ID: {1}, user ID: {2}", allowedPushMsg, groupId, userId);
                result = new Status((int)eResponseStatus.Error, MAX_NUMBER_OF_PUSH_MSG_EXCEEDED);
                return result;
            }

            // get user notification data
            if (string.IsNullOrEmpty(phoneNumber))
            {
                var _phoneNumber = GetPhoneNumberFromUser(groupId, userId);
                if (!_phoneNumber.IsOkStatusCode())
                {
                    result.Message = "Failed to get user data";
                    return result;
                }
                phoneNumber = _phoneNumber.Object;
            }

            if (!string.IsNullOrEmpty(phoneNumber))
            {
                // send SMS                
                var success = NotificationAdapter.SendSms(groupId, message, phoneNumber);

                if (!success)
                {
                    log.ErrorFormat("Error at SendSMS. GID: {0}, user ID: {1}, message: {2}", groupId, userId, message);
                    result = new Status() { Code = (int)eResponseStatus.Error, Message = "Failed Sending Sms" };
                    return result;
                }

                result = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            }

            return result;
        }

    }
}
