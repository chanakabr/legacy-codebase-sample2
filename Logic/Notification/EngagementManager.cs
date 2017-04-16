using ApiObjects.Notification;
using ApiObjects.Response;
using DAL;
using KLogMonitor;
using System;
using System.Reflection;

namespace Core.Notification
{
    public class EngagementManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private const string ENGAGEMENT_ADAPTER_ID_REQUIRED = "Engagement adapter identifier is required";
        private const string ENGAGEMENT_ADAPTER_NOT_EXIST = "Engagement adapter not exist";
        private const string NO_ENGAGEMENT_ADAPTER_TO_INSERT = "No Engagement adapter to insert";
        private const string NAME_REQUIRED = "Name must have a value";
        private const string ADAPTER_URL_REQUIRED = "Adapter URL must have a value";
        private const string NO_ENGAGEMENT_ADAPTER_TO_UPDATE = "No Engagement adapter to update";

        internal static EngagementAdapterResponseList GetEngagementAdapters(int groupId)
        {
            EngagementAdapterResponseList response = new EngagementAdapterResponseList();
            try
            {
                response.EngagementAdapters = NotificationDal.GetEngagementAdapterList(groupId);
                if (response.EngagementAdapters == null || response.EngagementAdapters.Count == 0)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, "no engagement adapter related to group");
                }
                else
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
            }
            catch (Exception ex)
            {
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.Error(string.Format("Failed groupID={0}", groupId), ex);
            }

            return response;
        }

        internal static EngagementAdapterResponse GetEngagementAdapter(int groupId, int engagementAdapterId)
        {
            EngagementAdapterResponse response = new EngagementAdapterResponse();
            try
            {
                response.EngagementAdapter = NotificationDal.GetEngagementAdapter(groupId, engagementAdapterId);
                if (response.EngagementAdapter == null)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.EngagementAdapterNotExist, "Engagement adapter not exist");
                }
                else
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
            }
            catch (Exception ex)
            {
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.Error(string.Format("Failed groupID={0}, engagementAdapterId={1}", groupId, engagementAdapterId), ex);
            }

            return response;
        }

        internal static Status DeleteEngagementAdapter(int groupId, int engagementAdapterId)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            try
            {
                if (engagementAdapterId == 0)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.EngagementAdapterIdentifierRequired, ENGAGEMENT_ADAPTER_ID_REQUIRED);
                    return response;
                }

                //check Engagement Adapter exist
                EngagementAdapter adapter = NotificationDal.GetEngagementAdapter(groupId, engagementAdapterId);
                if (adapter == null || adapter.ID <= 0)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.EngagementAdapterNotExist, ENGAGEMENT_ADAPTER_NOT_EXIST);
                    return response;
                }

                bool isSet = NotificationDal.DeleteEngagementAdapter(groupId, engagementAdapterId);
                if (isSet)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.OK, "Engagement adapter deleted");
                }
                else
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.EngagementAdapterNotExist, ENGAGEMENT_ADAPTER_NOT_EXIST);
                }

            }
            catch (Exception ex)
            {
                response = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.Error(string.Format("Failed groupID={0}, engagementAdapterId={1}", groupId, engagementAdapterId), ex);
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
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.AdapterUrlRequired, ADAPTER_URL_REQUIRED);
                    return response;
                }

                // Create Shared secret 
                engagementAdapter.SharedSecret = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 16);

                response.EngagementAdapter = NotificationDal.InsertEngagementAdapter(groupId, engagementAdapter);
                if (response.EngagementAdapter != null && response.EngagementAdapter.ID > 0)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, "new Engagement adapter insert");

                    if (!SendConfigurationToAdapter(groupId, response.EngagementAdapter))
                    {
                        log.ErrorFormat("InsertEngagementAdapter  - SendConfigurationToAdapter failed : AdapterID = {0}", response.EngagementAdapter.ID);
                    }
                }
                else
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "fail to insert new Engagement Adapter");
                }
            }
            catch (Exception ex)
            {
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.Error(string.Format("Failed groupID={0}", groupId), ex);
            }
            return response;
        }


        private static bool SendConfigurationToAdapter(int groupId, EngagementAdapter engagementAdapter)
        {
            //try
            //{
            //    if (engagementAdapter  != null && !string.IsNullOrEmpty(engagementAdapter .AdapterUrl))
            //    {
            //        //set unixTimestamp
            //        long unixTimestamp = ODBCWrapper.Utils.DateTimeToUnixTimestamp(DateTime.UtcNow);

            //        //set signature
            //        string signature = string.Concat(engagementAdapter .ID, engagementAdapter .Settings != null ? string.Concat(engagementAdapter.Settings.Select(s => string.Concat(s.key, s.value))) : string.Empty,
            //            groupId, unixTimestamp);

            //        using (APILogic.EngagementAdapterService.ServiceClient client = new APILogic.EngagementAdapterService.ServiceClient(string.Empty, engagementAdapter.AdapterUrl))
            //        {
            //            if (!string.IsNullOrEmpty(engagementAdapter.AdapterUrl))
            //            {
            //                client.Endpoint.Address = new System.ServiceModel.EndpointAddress(engagementAdapter.AdapterUrl);
            //            }

            //            APILogic.EngagementAdapterService.AdapterStatus adapterResponse = client.SetConfiguration(
            //                engagementAdapter.ID,
            //                engagementAdapter.Settings != null ? engagementAdapter.Settings.Select(s => new APILogic.engagementAdapterService.KeyValue() { Key = s.key, Value = s.value }).ToArray() : null,
            //                groupId,
            //                unixTimestamp,
            //                System.Convert.ToBase64String(TVinciShared.EncryptUtils.AesEncrypt(engagementAdapter.SharedSecret, TVinciShared.EncryptUtils.HashSHA1(signature))));

            //            if (adapterResponse != null && adapterResponse.Code == (int)engagementAdapterStatus.OK)
            //            {
            //                log.DebugFormat("Engagement Adapter SetConfiguration Result: AdapterID = {0}, AdapterStatus = {1}", engagementAdapter.ID, adapterResponse.Code);
            //                return true;
            //            }
            //            else
            //            {
            //                log.ErrorFormat("Engagement Adapter SetConfiguration Result: AdapterID = {0}, AdapterStatus = {1}",
            //                    engagementAdapter.ID, adapterResponse != null ? adapterResponse.Code.ToString() : "ERROR");
            //                return false;
            //            }
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    log.ErrorFormat("SendConfigurationToAdapter Failed: AdapterID = {0}, ex = {1}", engagementAdapter.ID, ex);
            //}
            return false; //TODO: Anat
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
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.AdapterUrlRequired, ADAPTER_URL_REQUIRED);
                    return response;
                }

                // SharedSecret generated only at insert 
                // this value not relevant at update and should be ignored
                //--------------------------------------------------------
                engagementAdapter.SharedSecret = null;

                // check engagementAdapter with this ID exists
                EngagementAdapter existingEngagementAdapter = NotificationDal.GetEngagementAdapter(groupId, engagementAdapter.ID);
                if (existingEngagementAdapter == null)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.EngagementAdapterNotExist, ENGAGEMENT_ADAPTER_NOT_EXIST);
                    return response;
                }

                response.EngagementAdapter = NotificationDal.SetEngagementAdapter(groupId, engagementAdapter);

                if (response.EngagementAdapter != null && response.EngagementAdapter.ID > 0)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, "Engagement adapter set changes");

                    if (!engagementAdapter.SkipSettings)
                    {
                        bool isSet = NotificationDal.SetEngagementAdapterSettings(groupId, engagementAdapter.ID, engagementAdapter.Settings);
                        if (isSet)
                        {
                            response.EngagementAdapter = NotificationDal.GetEngagementAdapter(groupId, engagementAdapter.ID);
                        }
                        else
                        {
                            response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "Engagement adapter failed set changes, check your params");
                        }
                    }

                    bool isSendSucceeded = SendConfigurationToAdapter(groupId, response.EngagementAdapter);
                    if (!isSendSucceeded)
                    {
                        log.DebugFormat("SetEngagementAdapter - SendConfigurationToAdapter failed : AdapterID = {0}", engagementAdapter.ID);
                    }
                }
                else
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "Engagement adapter failed set changes");
                }

            }
            catch (Exception ex)
            {
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.Error(string.Format("Failed groupID={0}, engagementAdapterId={1}, name={2}, adapterUrl={3}, isActive={4}",
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
                EngagementAdapter engagementAdapter = NotificationDal.GetEngagementAdapter(groupId, engagementAdapterId);
                if (engagementAdapter == null || engagementAdapter.ID <= 0)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.EngagementAdapterNotExist, ENGAGEMENT_ADAPTER_NOT_EXIST);
                    return response;
                }

                // Create Shared secret 
                string sharedSecret = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 16);

                response.EngagementAdapter = NotificationDal.SetEngagementAdapterSharedSecret(groupId, engagementAdapterId, sharedSecret);

                if (response.EngagementAdapter != null && response.EngagementAdapter.ID > 0)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, "engagement adapter generate shared secret");
                }
                else
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "engagement adapter failed set changes");
                }

            }
            catch (Exception ex)
            {
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.Error(string.Format("Failed groupID={0}, engagementAdapterId={1}", groupId, engagementAdapterId), ex);
            }
            return response;
        }
    }
}
