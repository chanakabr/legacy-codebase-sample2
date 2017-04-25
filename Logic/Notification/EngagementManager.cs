using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using APILogic.Notification.Adapters;
using ApiObjects.Notification;
using ApiObjects.QueueObjects;
using ApiObjects.Response;
using DAL;
using KLogMonitor;
using Newtonsoft.Json;
using QueueWrapper.Queues.QueueObjects;
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

        internal static Status DeleteEngagementAdapterSettings(int groupId, int engagementAdapterId, System.Collections.Generic.List<EngagementAdapterSettings> settings)
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
                    response = new ApiObjects.Response.Status((int)eResponseStatus.EngagementAdapterParamsRequired, NO_PARAMS_TO_DELETE);
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

                bool isSet = EngagementDal.DeleteEngagementAdapterSettings(groupId, engagementAdapterId, settings);
                if (isSet)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.OK, "Successfully deleted engagement adapter configuration");

                    //Get engagement Adapter updated                        
                    engagementAdapter = EngagementDal.GetEngagementAdapter(groupId, engagementAdapterId);
                    if (!EngagementAdapterClient.SendConfigurationToAdapter(groupId, engagementAdapter))
                        log.ErrorFormat("DeleteEngagementAdapterSettings - SendConfigurationToAdapter failed: AdapterID = {0}", engagementAdapterId);
                }
                else
                    response = new ApiObjects.Response.Status((int)eResponseStatus.Error, "Failed to delete engagement adapter configuration");
            }
            catch (Exception ex)
            {
                response = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.Error(string.Format("Failed to delete engagement adapter settings. Group ID: {0}, engagement adapter ID: {1}", groupId, engagementAdapterId), ex);
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

        internal static Status InsertEngagementAdapterSettings(int groupId, int engagementAdapterId, System.Collections.Generic.List<EngagementAdapterSettings> settings)
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
                if (matchingKeyAmount > 0)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.ConflictedParams, CONFLICTED_PARAMS);
                    return response;
                }

                bool isInsert = EngagementDal.InsertEngagementAdapterSettings(groupId, engagementAdapterId, settings);
                if (isInsert)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.OK, "Successfully set engagement adapter configuration");

                    //Get engagement Adapter updated                        
                    engagementAdapter = EngagementDal.GetEngagementAdapter(groupId, engagementAdapterId);
                    if (!EngagementAdapterClient.SendConfigurationToAdapter(groupId, engagementAdapter))
                        log.ErrorFormat("InsertengagementAdapterSettings - SendConfigurationToAdapter failed : AdapterID = {0}", engagementAdapterId);
                }
                else
                    response = new ApiObjects.Response.Status((int)eResponseStatus.Error, "Failed to insert engagement adapter configuration");
            }
            catch (Exception ex)
            {

                response = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.Error(string.Format("Failed to insert engagement adapter settings. Group ID: {0}, engagement adapter ID: {1}", groupId, engagementAdapterId), ex);
            }
            return response;
        }

        internal static EngagementAdapterSettingsResponse GetEngagementAdapterSettings(int groupId)
        {
            EngagementAdapterSettingsResponse response = new EngagementAdapterSettingsResponse();
            try
            {
                response.EngagementAdapters = EngagementDal.GetEngagementAdapterSettingsList(groupId, 0);
                if (response.EngagementAdapters == null || response.EngagementAdapters.Count == 0)
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, "No engagement adapter configurations were found");
                else
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            }
            catch (Exception ex)
            {
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.Error(string.Format("Failed to get engagement adapter settings. Group ID: {0} ", groupId), ex);
            }

            return response;
        }

        internal static EngagementResponse AddEngagement(int groupId, Engagement engagement)
        {
            EngagementResponse response = new EngagementResponse();

            try
            {
                if (engagement == null)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.NoEngagementToInsert, NO_ENGAGEMENT_TO_INSERT);
                    return response;
                }



                // validate input

                //if (string.IsNullOrEmpty(engagementAdapter.Name))
                //{
                //    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.NameRequired, NAME_REQUIRED);
                //    return response;
                //}

                //if (string.IsNullOrEmpty(engagementAdapter.AdapterUrl))
                //{
                //    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.AdapterUrlRequired, ADAPTER_URL_REQUIRED);
                //    return response;
                //}


                response.Engagement = EngagementDal.InsertEngagement(groupId, engagement);
                if (response.Engagement != null && response.Engagement.Id > 0)
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, "new Engagement adapter insert");
                else
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "fail to insert new Engagement");
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed to add engagement. GroupID: {0}", groupId), ex);
            }
            return response;
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

        internal static EngagementResponseList GetEngagements(int groupId)
        {
            EngagementResponseList response = new EngagementResponseList();
            try
            {
                response.Engagements = EngagementDal.GetEngagementList(groupId);
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

            // if scheduler engagement - create next iteration
            if (engagementToBeSent.IntervalSeconds > 0)
            {
                if (!HandleSchedularEngagement(partnerId, lastHourAndFutureEngagement, engagementToBeSent))
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
                if (userList == null || userList.Count() == 0)
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
                if (userList == null || userList.Count() == 0)
                {
                    log.ErrorFormat("Error getting user list from engagement. Engagement: {0}", JsonConvert.SerializeObject(engagementToBeSent));

                    // return true - do not retry
                    return true;
                }
            }

            // get bulk from TCM
            int engagementBulkMessages = TCMClient.Settings.Instance.GetValue<int>("num_of_bulk_message_engagements");

            // calculate number of iterations
            int remainder = 0;
            long numOfBulkMessages = Math.DivRem(userList.Count, engagementBulkMessages, out remainder);
            if (remainder > 0)
                numOfBulkMessages++;

            Parallel.For(0, numOfBulkMessages, index =>
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
                EngagementBulkMessage insertedBulkMessage = EngagementDal.SetEngagementBulkMessage(partnerId, bulkMessage);
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

        internal static bool HandleSchedularEngagement(int partnerId, List<Engagement> lastHourAndFutureEngagement, Engagement engagementToBeSent)
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
            throw new NotImplementedException();
        }
    }
}
