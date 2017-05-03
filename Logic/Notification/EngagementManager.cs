using APILogic.AmazonSnsAdapter;
using APILogic.Notification.Adapters;
using ApiObjects;
using ApiObjects.Notification;
using ApiObjects.QueueObjects;
using ApiObjects.Response;
using Core.Notification.Adapters;
using Core.Pricing;
using DAL;
using KLogMonitor;
using Newtonsoft.Json;
using QueueWrapper.Queues.QueueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using TVinciShared;

namespace Core.Notification
{
    public class EngagementManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private const string ENGAGEMENT_ADAPTER_ID_REQUIRED = "Engagement adapter identifier is required";
        private const string ENGAGEMENT_ADAPTER_NOT_EXIST = "Engagement adapter doesn't exist";
        private const string NO_ENGAGEMENT_ADAPTER_TO_INSERT = "Engagement adapter wasn't found";
        private const string NAME_REQUIRED = "Name must have a value";
        private const string PROVIDER_URL_REQUIRED = "Provider URL must have a value";
        private const string NO_ENGAGEMENT_ADAPTER_TO_UPDATE = "No Engagement adapter to update";
        private const string NO_PARAMS_TO_INSERT = "No parameters to insert";
        private const string CONFLICTED_PARAMS = "Conflicted params";
        private const string NO_PARAMS_TO_DELETE = "No parameters to delete";
        private const string NO_ENGAGEMENT_TO_INSERT = "No Engagement to insert";
        private const string ENGAGEMENT_NOT_EXIST = "Engagement not exist";
        private const string EMPTY_SOURCE_USER_LIST = "User list and adapter ID are empty";
        private const string FUTURE_SEND_TIME = "Send time must be in the future";
        private const string ILLEGAL_USER_LIST = "Illegal user list";
        private const string ILLEGAL_ENGAGEMENT_INTERVAL = "Illegal interval inserted";
        private const string ENGAGEMENT_RECENTLY_SENT = "Other engagement was recently sent";
        private const string ENGAGEMENT_SEND_WINDOW_FRAME = "Send time is not between the allowed time window";
        private const string FUTURE_SCHEDULE_ENGAGEMENT_DETECTED = "Future engagement scheduler detected";
        private const string ENGAGEMENT_TEMPLATE_NOT_DOUND = "Engagement template wasn't found";
        private const string ENGAGEMENT_SUCCESSFULLY_INSERTED = "Engagement was successfully inserted";
        private const string ERROR_INSERTING_ENGAGEMENT = "Error occurred while inserting engagement";
        private const string COUPON_GROUP_NOT_FOUND = "Coupon group ID wasn't found";

        private static int NUM_OF_BULK_MESSAGE_ENGAGEMENTS = 500;
        private static int NUM_OF_ENGAGEMENT_THREADS = 10;

        private const string ROUTING_KEY_ENGAGEMENTS = "PROCESS_ENGAGEMENTS";


        internal static EngagementAdapterResponseList GetEngagementAdapters(int groupId)
        {
            EngagementAdapterResponseList response = new EngagementAdapterResponseList();
            try
            {
                response.EngagementAdapters = EngagementDal.GetEngagementAdapterList(groupId);
                if (response.EngagementAdapters == null || response.EngagementAdapters.Count == 0)
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, "No engagement adapter related to group");
                else
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            }
            catch (Exception ex)
            {
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.Error(string.Format("Failed to get engagement adapter groupID: {0}", groupId), ex);
            }

            return response;
        }

        internal static EngagementAdapterResponse GetEngagementAdapter(int groupId, int engagementAdapterId)
        {
            EngagementAdapterResponse response = new EngagementAdapterResponse();
            try
            {
                response.EngagementAdapter = EngagementDal.GetEngagementAdapter(groupId, engagementAdapterId);
                if (response.EngagementAdapter == null)
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.EngagementAdapterNotExist, "Engagement adapter not exist");
                else
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            }
            catch (Exception ex)
            {
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.Error(string.Format("Failed to get engagement adapter. Group ID: {0}, engagementAdapterId: {1}", groupId, engagementAdapterId), ex);
            }

            return response;
        }

        internal static Status DeleteEngagementAdapter(int groupId, int id)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            try
            {
                if (id == 0)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.EngagementAdapterIdentifierRequired, ENGAGEMENT_ADAPTER_ID_REQUIRED);
                    return response;
                }

                //check Engagement Adapter exist
                EngagementAdapter adapter = EngagementDal.GetEngagementAdapter(groupId, id);
                if (adapter == null || adapter.ID <= 0)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.EngagementAdapterNotExist, ENGAGEMENT_ADAPTER_NOT_EXIST);
                    return response;
                }

                bool isSet = EngagementDal.DeleteEngagementAdapter(groupId, id);
                if (isSet)
                    response = new ApiObjects.Response.Status((int)eResponseStatus.OK, "Engagement adapter deleted");
                else
                    response = new ApiObjects.Response.Status((int)eResponseStatus.EngagementAdapterNotExist, ENGAGEMENT_ADAPTER_NOT_EXIST);
            }
            catch (Exception ex)
            {
                response = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.Error(string.Format("Failed to delete engagement adapter. Group ID: {0}, engagementAdapterId: {1}", groupId, id), ex);
            }
            return response;
        }

        internal static EngagementAdapterResponse InsertEngagementAdapter(int groupId, EngagementAdapter engagementAdapter)
        {
            EngagementAdapterResponse response = new EngagementAdapterResponse();

            try
            {
                if (engagementAdapter == null)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.NoEngagementAdapterToInsert, NO_ENGAGEMENT_ADAPTER_TO_INSERT);
                    return response;
                }

                if (string.IsNullOrEmpty(engagementAdapter.Name))
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.NameRequired, NAME_REQUIRED);
                    return response;
                }

                if (string.IsNullOrEmpty(engagementAdapter.AdapterUrl))
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.ProviderUrlRequired, PROVIDER_URL_REQUIRED);
                    return response;
                }

                // Create Shared secret 
                engagementAdapter.SharedSecret = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 16);

                response.EngagementAdapter = EngagementDal.InsertEngagementAdapter(groupId, engagementAdapter);
                if (response.EngagementAdapter != null && response.EngagementAdapter.ID > 0)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, "New engagement adapter was successfully inserted");

                    if (!EngagementAdapterClient.SendConfigurationToAdapter(groupId, response.EngagementAdapter))
                        log.ErrorFormat("InsertEngagementAdapter - SendConfigurationToAdapter failed : AdapterID = {0}", response.EngagementAdapter.ID);
                }
                else
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "failed to insert new engagement Adapter");
            }
            catch (Exception ex)
            {
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.Error(string.Format("Failed to insert engagement adapter. Group ID: {0}", groupId), ex);
            }
            return response;
        }

        internal static EngagementAdapterResponse SetEngagementAdapter(int groupId, EngagementAdapter engagementAdapter)
        {
            EngagementAdapterResponse response = new EngagementAdapterResponse();

            try
            {
                if (engagementAdapter == null)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.NoEngagementAdapterToUpdate, NO_ENGAGEMENT_ADAPTER_TO_UPDATE);
                    return response;
                }

                if (engagementAdapter.ID == 0)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.EngagementAdapterIdentifierRequired, ENGAGEMENT_ADAPTER_ID_REQUIRED);
                    return response;
                }
                if (string.IsNullOrEmpty(engagementAdapter.Name))
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.NameRequired, NAME_REQUIRED);
                    return response;
                }

                if (string.IsNullOrEmpty(engagementAdapter.AdapterUrl))
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.ProviderUrlRequired, PROVIDER_URL_REQUIRED);
                    return response;
                }

                // SharedSecret generated only at insert 
                // this value not relevant at update and should be ignored
                //--------------------------------------------------------
                engagementAdapter.SharedSecret = null;

                // check engagementAdapter with this ID exists
                EngagementAdapter existingEngagementAdapter = EngagementDal.GetEngagementAdapter(groupId, engagementAdapter.ID);
                if (existingEngagementAdapter == null)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.EngagementAdapterNotExist, ENGAGEMENT_ADAPTER_NOT_EXIST);
                    return response;
                }

                response.EngagementAdapter = EngagementDal.SetEngagementAdapter(groupId, engagementAdapter);

                if (response.EngagementAdapter != null && response.EngagementAdapter.ID > 0)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, "Engagement adapter was successfully set");

                    if (!engagementAdapter.SkipSettings)
                    {
                        bool isSet = EngagementDal.SetEngagementAdapterSettings(groupId, engagementAdapter.ID, engagementAdapter.Settings);
                        if (isSet)
                            response.EngagementAdapter = EngagementDal.GetEngagementAdapter(groupId, engagementAdapter.ID);
                        else
                            response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "Engagement adapter failed set changes, check your params");
                    }

                    bool isSendSucceeded = EngagementAdapterClient.SendConfigurationToAdapter(groupId, response.EngagementAdapter);
                    if (!isSendSucceeded)
                        log.DebugFormat("SetEngagementAdapter - SendConfigurationToAdapter failed : AdapterID = {0}", engagementAdapter.ID);
                }
                else
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "Engagement adapter failed set changes");
            }
            catch (Exception ex)
            {
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.Error(string.Format("Failed to set engagement adapter. Group ID: {0}, engagement adapter ID: {1}, name: {2}, adapterUrl: {3}, isActive: {4}",
                    groupId, engagementAdapter.ID, engagementAdapter.Name, engagementAdapter.AdapterUrl, engagementAdapter.IsActive), ex);
            }
            return response;
        }

        internal static EngagementAdapterResponse GenerateEngagementSharedSecret(int groupId, int engagementAdapterId)
        {
            EngagementAdapterResponse response = new EngagementAdapterResponse();

            try
            {
                if (engagementAdapterId <= 0)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.EngagementAdapterIdentifierRequired, ENGAGEMENT_ADAPTER_ID_REQUIRED);
                    return response;
                }

                //check Engagement Adapter exist
                EngagementAdapter engagementAdapter = EngagementDal.GetEngagementAdapter(groupId, engagementAdapterId);
                if (engagementAdapter == null || engagementAdapter.ID <= 0)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.EngagementAdapterNotExist, ENGAGEMENT_ADAPTER_NOT_EXIST);
                    return response;
                }

                // Create Shared secret 
                string sharedSecret = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 16);

                response.EngagementAdapter = EngagementDal.SetEngagementAdapterSharedSecret(groupId, engagementAdapterId, sharedSecret);

                if (response.EngagementAdapter != null && response.EngagementAdapter.ID > 0)
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, "Engagement adapter generate shared secret");
                else
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "Engagement adapter failed set changes");
            }
            catch (Exception ex)
            {
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.Error(string.Format("Failed groupID={0}, engagementAdapterId={1}", groupId, engagementAdapterId), ex);
            }

            return response;
        }

        internal static Status SetEngagementAdapterSettings(int groupId, int engagementAdapterId, System.Collections.Generic.List<EngagementAdapterSettings> settings)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());

            try
            {
                if (engagementAdapterId == 0)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.EngagementAdapterIdentifierRequired, ENGAGEMENT_ADAPTER_ID_REQUIRED);
                    return response;
                }

                if (settings == null || settings.Count == 0)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.EngagementAdapterParamsRequired, NO_PARAMS_TO_INSERT);
                    return response;
                }

                //check engagement Adapter exist
                EngagementAdapter engagementAdapter = EngagementDal.GetEngagementAdapter(groupId, engagementAdapterId);
                if (engagementAdapter == null || engagementAdapter.ID <= 0)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.EngagementAdapterNotExist, ENGAGEMENT_ADAPTER_NOT_EXIST);
                    return response;
                }

                int matchingKeyAmount = GetMatchingKeyAmount(engagementAdapter.Settings, settings);
                if (matchingKeyAmount != settings.Count)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.ConflictedParams, CONFLICTED_PARAMS);
                    return response;
                }

                bool isSet = EngagementDal.SetEngagementAdapterSettings(groupId, engagementAdapterId, settings);
                if (isSet)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.OK, "successfully set engagement adapter changes");
                    //Get engagement Adapter updated                        
                    engagementAdapter = EngagementDal.GetEngagementAdapter(groupId, engagementAdapterId);
                    if (!EngagementAdapterClient.SendConfigurationToAdapter(groupId, engagementAdapter))
                        log.ErrorFormat("SetengagementAdapterSettings - SendConfigurationToAdapter failed : AdapterID = {0}", engagementAdapterId);
                }
                else
                    response = new ApiObjects.Response.Status((int)eResponseStatus.Error, "Failed to set engagement adapter settings, please check your input parameters");
            }
            catch (Exception ex)
            {
                response = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.Error(string.Format("Failed to set engagement adapter settings. Group ID: {0}, engagement adapter ID: {1}", groupId, engagementAdapterId), ex);
            }
            return response;
        }

        private static int GetMatchingKeyAmount(System.Collections.Generic.List<EngagementAdapterSettings> originalList, System.Collections.Generic.List<EngagementAdapterSettings> settings)
        {
            int matchingKeyAmount = 0;
            EngagementAdapterSettings result;
            foreach (EngagementAdapterSettings originalSettings in originalList)
            {
                result = settings.Find(x => x.Key == originalSettings.Key);
                if (result != null)
                    matchingKeyAmount++; ;
            }

            return matchingKeyAmount;
        }

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
                if (!AddEngagementToQueue(partnerId, DateUtils.DateTimeToUnixTimestamp(engagement.SendTime), response.Engagement.Id))
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
                    List<int> userList = engagement.UserList.Split(';').Select(p => Convert.ToInt32(p.Trim())).ToList();
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

            // validate interval is legal
            if (engagement.IntervalSeconds < 0)
            {
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.IllegalPostData, ILLEGAL_ENGAGEMENT_INTERVAL);
                log.ErrorFormat("Illegal interval was inserted. Partner ID: {0}, Engagement: {1}", partnerId, JsonConvert.SerializeObject(engagement));
                return false;
            }

            // Get all engagements from the last hour and forward
            List<Engagement> lastHourAndFutureEngagement = EngagementDal.GetEngagementList(partnerId, utcNow.AddHours(-1), true);
            if (lastHourAndFutureEngagement != null)
            {
                // validate same engagement was not already sent in the last hour
                Engagement engagementAlreadySent = lastHourAndFutureEngagement.FirstOrDefault(x => x.EngagementType == engagement.EngagementType &&
                                                                                                   x.AdapterId == engagement.AdapterId &&
                                                                                                   x.AdapterDynamicData == engagement.AdapterDynamicData &&
                                                                                                   x.UserList == engagement.UserList &&
                                                                                                   x.SendTime > engagement.SendTime.AddHours(-1) &&
                                                                                                   x.SendTime < engagement.SendTime);
                if (engagementAlreadySent != null)
                {
                    log.ErrorFormat("Engagement was already sent in the last hour. Sent engagement: {0}, my (canceled) engagement: {1}",
                     JsonConvert.SerializeObject(engagementAlreadySent),
                     JsonConvert.SerializeObject(engagement));

                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.EngagementRecentlySent, ENGAGEMENT_RECENTLY_SENT);
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
            }

            // validate template exists
            var templates = NotificationDal.GetMessageTemplate(partnerId, ApiObjects.MessageTemplateType.Churn);
            if (templates == null || templates.Count == 0)
            {
                log.ErrorFormat("Engagement template wasn't found: {0}", JsonConvert.SerializeObject(engagement));
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.EngagementTemplateNotFound, ENGAGEMENT_TEMPLATE_NOT_DOUND);
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
            EngagementQueue queue = new EngagementQueue();
            EngagementData queueData = new EngagementData(groupId, startTime, engagementId, engagementBulkId)
            {
                ETA = ODBCWrapper.Utils.UnixTimestampToDateTime(startTime)
            };

            bool res = queue.Enqueue(queueData, ROUTING_KEY_ENGAGEMENTS);

            if (res)
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
                response.Engagements = EngagementDal.GetEngagementList(groupId, sendTimeLessThanOrEqual, false, engagementTypes);
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

        internal static bool SendEngagement(int partnerId, int engagementId, int startTime)
        {
            DateTime utcNow = DateTime.UtcNow;

            // Get all engagements from the last hour forward
            List<Engagement> lastHourAndFutureEngagement = EngagementDal.GetEngagementList(partnerId, utcNow.AddHours(-1), true);
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
            if (Math.Abs(DateUtils.DateTimeToUnixTimestamp(engagementToBeSent.SendTime) - startTime) > 30)
            {
                log.ErrorFormat("Engagement time was changed (to more than 30 seconds). Engagement ID: {0}, engagement time: {1}, message time: {2}",
                    engagementId,
                    engagementToBeSent.SendTime,
                    DateUtils.UnixTimeStampToDateTime(startTime));

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
                userList = engagementToBeSent.UserList.Split(';').Select(p => Convert.ToInt32(p.Trim())).ToList();
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
            int engagementBulkMessages = TCMClient.Settings.Instance.GetValue<int>("num_of_bulk_message_engagements");
            if (engagementBulkMessages == 0)
                engagementBulkMessages = NUM_OF_BULK_MESSAGE_ENGAGEMENTS;

            // calculate number of iterations
            int remainder = 0;
            long numOfBulkMessages = Math.DivRem(userList.Count, engagementBulkMessages, out remainder);
            if (remainder > 0)
                numOfBulkMessages++;

            // get number of allowed threads
            int numberOfEngagementThread = TCMClient.Settings.Instance.GetValue<int>("num_of_engagement_threads");
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
                    if (!AddEngagementToQueue(partnerId, DateUtils.DateTimeToUnixTimestamp(utcNow), engagementToBeSent.Id, insertedBulkMessage.Id))
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

            // create next iteration
            futureEngagement = new Engagement()
            {
                AdapterDynamicData = engagementToBeSent.AdapterDynamicData,
                AdapterId = engagementToBeSent.AdapterId,
                EngagementType = engagementToBeSent.EngagementType,
                IntervalSeconds = engagementToBeSent.IntervalSeconds,
                SendTime = engagementToBeSent.SendTime.AddSeconds(engagementToBeSent.IntervalSeconds)
            };

            // insert to DB
            if (EngagementDal.InsertEngagement(partnerId, futureEngagement) == null)
            {
                log.ErrorFormat("Error while trying to create next engagement iteration in DB. engagement data: {0}", JsonConvert.SerializeObject(futureEngagement));
                return false;
            }

            // create engagement Rabbit message
            if (!AddEngagementToQueue(partnerId, DateUtils.DateTimeToUnixTimestamp(futureEngagement.SendTime), futureEngagement.Id))
            {
                log.ErrorFormat("Error while trying to create next engagement iteration in DB. engagement data: {0}", JsonConvert.SerializeObject(futureEngagement));

                // remove new engagement 
                if (!EngagementDal.DeleteEngagement(partnerId, futureEngagement.Id))
                    log.ErrorFormat("Error while trying to delete engagement after failed to create a rabbit message. engagement data: {0}", JsonConvert.SerializeObject(futureEngagement));

                return false;
            }
            return true;
        }

        internal static bool SendEngagementBulk(int partnerId, int engagementId, int engagementBulkId, int startTime)
        {
            // get relevant engagement  
            Engagement engagement = EngagementDal.GetEngagement(partnerId, engagementId);
            if (engagement == null)
            {
                log.ErrorFormat("Engagement was not found in DB. Engagement ID: {0}", engagementId);
                return false;
            }

            // validate engagement 
            if (engagement.IsActive == false)
            {
                log.ErrorFormat("Engagement is active false. Engagement ID: {0}", engagementId);
                return true;
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
            List<Coupon> coupons = Core.Pricing.Module.GenerateCoupons(partnerId, userEngagements.Count, engagement.CouponGroupId);
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
            int numberOfEngagementThread = TCMClient.Settings.Instance.GetValue<int>("num_of_engagement_threads");
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
            if (!NotificationSettings.IsPartnerInboxEnabled(partnerId))
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

            long currentTimeSec = ODBCWrapper.Utils.DateTimeToUnixTimestamp(DateTime.UtcNow);

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

            if (!NotificationDal.SetUserInboxMessage(partnerId, inboxMessage, NotificationSettings.GetInboxMessageTTLDays(partnerId)))
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
                    status = new ApiObjects.Response.Status((int)eResponseStatus.EngagementAdapterIdentifierRequired, ENGAGEMENT_ADAPTER_ID_REQUIRED);
                    return status;
                }

                //get engagementAdapter
                EngagementAdapter engagementAdapter = EngagementDal.GetEngagementAdapter(groupId, engagementAdapterId);

                if (engagementAdapter == null || engagementAdapter.ID <= 0)
                {
                    status = new ApiObjects.Response.Status((int)eResponseStatus.EngagementAdapterNotExist, ENGAGEMENT_ADAPTER_NOT_EXIST);
                    return status;
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
                if (!AddEngagementToQueue(partnerId, DateUtils.DateTimeToUnixTimestamp(DateTime.UtcNow), engagement.Id, bulk.Id))
                    log.ErrorFormat("Error while trying to create bulk engagement rabbit message. engagement data: {0}", JsonConvert.SerializeObject(bulk));
            }

            return true;
        }
    }
}
