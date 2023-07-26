using AdapterControllers.cdvrAdap;
//using AdapterControllers.CdvrAdapterService;
using ApiObjects;
using ApiObjects.Response;
using CachingHelpers;
using Phx.Lib.Log;
using Synchronizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using TVinciShared;
using ApiObjects.TimeShiftedTv;
using System.Web;
using Phx.Lib.Appconfig;


namespace AdapterControllers.CDVR
{
    public class CdvrAdapterController
    {
        #region Consts
        public const int STATUS_OK = 0;
        public const int STATUS_NO_CONFIGURATION_FOUND = 3;

        public const string PARAMETER_GROUP_ID = "group_id";
        public const string PARAMETER_ADAPTER = "adapter";
        #endregion

        #region Static Data Members

        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        /// <summary>
        /// Locker for the entire class
        /// </summary>
        private static readonly object generalLocker = new object();

        #endregion

        #region Private Data Members

        private CouchbaseSynchronizer configurationSynchronizer;

        #endregion

        #region Singleton

        private static CdvrAdapterController instance;

        /// <summary>
        /// Gets the singleton instance of the adapter controller
        /// </summary>     
        /// <returns></returns>
        public static CdvrAdapterController GetInstance()
        {
            if (instance == null)
            {
                lock (generalLocker)
                {
                    if (instance == null)
                    {
                        instance = new CdvrAdapterController();
                    }
                }
            }

            return instance;
        }

        #endregion

        #region Ctor

        private CdvrAdapterController()
        {
            configurationSynchronizer = new CouchbaseSynchronizer(100);
            configurationSynchronizer.SynchronizedAct += configurationSynchronizer_SynchronizedAct;
        }

        #endregion

        #region Public Method

        public ServiceClient GetCDVRAdapterServiceClient(string adapterUrl)
        {
            log.Debug($"Constructing GetCDVRAdapterServiceClient Client with url:[{adapterUrl}]");
            var SSOAdapaterServiceEndpointConfiguration = ServiceClient.EndpointConfiguration.BasicHttpBinding_IService;
            var adapterClient = new ServiceClient(SSOAdapaterServiceEndpointConfiguration, adapterUrl);
            adapterClient.ConfigureServiceClient(ApplicationConfiguration.Current.AdaptersClientConfiguration.CdvrAdapter);

            return adapterClient;
        }

        public bool SetConfiguration(CDVRAdapter adapter, int partnerId)
        {
            bool result = false;
            try
            {
                string cdvrAdapterUrl = adapter.AdapterUrl;
                var client = GetCDVRAdapterServiceClient(cdvrAdapterUrl);

                //set unixTimestamp
                long timeStamp = TVinciShared.DateUtils.DateTimeToUtcUnixTimestampSeconds(DateTime.UtcNow);
                //set signature
                string signature = string.Concat(adapter.ID, adapter.Settings != null ? string.Concat(adapter.Settings.Select(kv => string.Concat(kv.key, kv.value))) : string.Empty,
                    partnerId, timeStamp);

                //call Adapter 
                List<AdapterControllers.cdvrAdap.KeyValue> keyValue = new List<AdapterControllers.cdvrAdap.KeyValue>();
                cdvrAdap.AdapterStatus adapterStatus = null;
                if (adapter.Settings != null)
                {
                    keyValue = adapter.Settings.Select(setting => new AdapterControllers.cdvrAdap.KeyValue()
                    {
                        Key = setting.key,
                        Value = setting.value
                    }).ToList();
                }
                adapterStatus = client
                    .SetConfigurationAsync(adapter.ID, keyValue, partnerId, timeStamp, Utils.GetSignature(adapter.SharedSecret, signature))
                    .ExecuteAndWait();

                if (adapterStatus != null)
                    log.DebugFormat("Cdvr Adapter Send Configuration Result = {0}", adapterStatus);
                else
                    log.Debug("Adapter status is null");

                if (adapterStatus != null && adapterStatus.Code == STATUS_OK)
                {
                    result = true;
                }
            }
            catch (Exception ex)
            {
                ReportCDVRAdapterError(adapter.ID, adapter.AdapterUrl, ex, "SetConfiguration", false);
                log.ErrorFormat("Failed ex = {0}, adapter id = {1}", ex, adapter != null ? adapter.ID : 0);
            }
            return result;
        }

        public RecordResult Record(int partnerId, long startTimeSeconds, long durationSeconds, string epgId, string channelId, int adapterId, List<long> domainIds)
        {
            RecordResult recordResult = new RecordResult();

            CDVRAdapter adapter = CdvrAdapterCache.Instance().GetCdvrAdapter(partnerId, adapterId);

            if (adapter == null)
            {
                throw new KalturaException(string.Format("Cdvr Adapter {0} doesn't exist", adapterId), (int)eResponseStatus.AdapterNotExists);
            }

            if (string.IsNullOrEmpty(adapter.AdapterUrl))
            {
                throw new KalturaException("Cdvr adapter has no URL", (int)eResponseStatus.AdapterUrlRequired);
            }

            string cdvrAdapterUrl = adapter.AdapterUrl;
            var client = GetCDVRAdapterServiceClient(cdvrAdapterUrl);

            //set unixTimestamp
            long timeStamp = TVinciShared.DateUtils.DateTimeToUtcUnixTimestampSeconds(DateTime.UtcNow);

            //TODO: verify that signature is correct
            string signature = string.Concat(startTimeSeconds, durationSeconds, channelId, adapterId, timeStamp);

            try
            {
                log.DebugFormat("Sending request to cdvr adapter. partnerId ID = {0}, adapterID = {1}, startTimeSeconds = {2}, durationSeconds = {3}, channelId = {4}",
                    partnerId, adapter.ID, startTimeSeconds, durationSeconds, channelId);

                cdvrAdap.RecordingResponse adapterResponse = new cdvrAdap.RecordingResponse();

                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    try
                    {
                        //call Adapter Record
                        adapterResponse = client
                            .RecordAsync(startTimeSeconds, durationSeconds, epgId, channelId, domainIds, adapter.ID, timeStamp, Utils.GetSignature(adapter.SharedSecret, signature))
                            .ExecuteAndWait();
                    }
                    catch (Exception ex)
                    {
                        ReportCDVRAdapterError(adapterId, cdvrAdapterUrl, ex, "Record");
                    }
                }

                LogAdapterResponse(adapterResponse, "Record", adapterId,
                    string.Format("startTimeSeconds = {0}, durationSeconds = {1}, channelId = {2}", startTimeSeconds, durationSeconds, channelId));

                if (adapterResponse != null && adapterResponse.Status != null &&
                    adapterResponse.Status.Code == STATUS_NO_CONFIGURATION_FOUND)
                {
                    #region Send Configuration if not found

                    string key = string.Format("Cdvr_Adapter_Locker_{0}", adapterId);

                    // Build dictionary for synchronized action
                    Dictionary<string, object> parameters = new Dictionary<string, object>()
                    {
                        {PARAMETER_ADAPTER, adapter},
                        {PARAMETER_GROUP_ID, partnerId}
                    };

                    configurationSynchronizer.DoAction(key, parameters);

                    using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                    {
                        try
                        {
                            //call Adapter Record - after it is configured
                            adapterResponse = client
                                .RecordAsync(startTimeSeconds, durationSeconds, epgId, channelId, domainIds, adapter.ID, timeStamp, Utils.GetSignature(adapter.SharedSecret, signature))
                                .ExecuteAndWait();
                        }
                        catch (Exception ex)
                        {
                            ReportCDVRAdapterError(adapterId, cdvrAdapterUrl, ex, "Record");
                        }
                    }

                    LogAdapterResponse(adapterResponse, "Record", adapterId,
                        string.Format("startTimeSeconds = {0}, durationSeconds = {1}, channelId = {2}", startTimeSeconds, durationSeconds, channelId));

                    #endregion
                }

                if (adapterResponse != null && adapterResponse.Status != null)
                {
                    // If something went wrong in the adapter, throw relevant exception
                    if (adapterResponse.Status.Code != (int)eResponseStatus.OK)
                    {
                        throw new KalturaException("Adapter failed completing request", (int)eResponseStatus.AdapterAppFailure);
                    }
                    else if (adapterResponse.Recording != null)
                    {
                        recordResult = CreateRecordResult(adapterResponse);
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error in Record (Cdvr): error = {0}, adapterID = {1}, adapter ExternalIdentifier = {2}, channelId = {3}",
                    ex, adapterId, adapter.ExternalIdentifier, channelId
                    );
                throw new KalturaException("Adapter failed completing request", (int)eResponseStatus.AdapterAppFailure);
            }
            return recordResult;
        }

        public RecordResult GetRecordingStatus(int partnerId, string channelId, string recordingId, int adapterId)
        {
            RecordResult recordResult = new RecordResult();

            CDVRAdapter adapter = CdvrAdapterCache.Instance().GetCdvrAdapter(partnerId, adapterId);

            if (adapter == null)
            {
                throw new KalturaException(string.Format("Cdvr Adapter {0} doesn't exist", adapter.ID), (int)eResponseStatus.AdapterNotExists);
            }

            if (string.IsNullOrEmpty(adapter.AdapterUrl))
            {
                throw new KalturaException("Cdvr adapter has no URL", (int)eResponseStatus.AdapterUrlRequired);
            }

            string cdvrAdapterUrl = adapter.AdapterUrl;
            var client = GetCDVRAdapterServiceClient(cdvrAdapterUrl);

            //set unixTimestamp
            long timeStamp = TVinciShared.DateUtils.DateTimeToUtcUnixTimestampSeconds(DateTime.UtcNow);

            //TODO: verify that signature is correct
            string signature = string.Concat(recordingId, channelId, adapterId, timeStamp);

            try
            {
                log.DebugFormat("Sending request to cdvr adapter. partnerId ID = {0}, adapterID = {1}, recordingId = {2}, channelId = {3}",
                    partnerId, adapter.ID, recordingId, channelId);

                var adapterResponse = new AdapterControllers.cdvrAdap.RecordingResponse();

                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    try
                    {
                        //call Adapter GetRecordingStatus
                        adapterResponse = client
                            .GetRecordingStatusAsync(recordingId, channelId, adapterId, timeStamp, Utils.GetSignature(adapter.SharedSecret, signature))
                            .ExecuteAndWait();
                    }
                    catch (Exception ex)
                    {
                        ReportCDVRAdapterError(adapterId, cdvrAdapterUrl, ex, "GetRecordingStatus");
                    }
                }

                LogAdapterResponse(adapterResponse, "GetRecordingStatus", adapterId,
                    string.Format("recordingId = {0}", recordingId));

                if (adapterResponse != null && adapterResponse.Status != null && adapterResponse.Status.Code == STATUS_NO_CONFIGURATION_FOUND)
                {
                    #region Send Configuration if not found

                    string key = string.Format("Cdvr_Adapter_Locker_{0}", adapterId);

                    // Build dictionary for synchronized action
                    Dictionary<string, object> parameters = new Dictionary<string, object>()
                    {
                        {PARAMETER_ADAPTER, adapter},
                        {PARAMETER_GROUP_ID, partnerId}
                    };

                    configurationSynchronizer.DoAction(key, parameters);

                    using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                    {
                        //call Adapter GetRecordingStatus - after it is configured
                        try
                        {
                            adapterResponse = client
                                .GetRecordingStatusAsync(recordingId, channelId, adapterId, timeStamp, Utils.GetSignature(adapter.SharedSecret, signature))
                                .ExecuteAndWait();
                        }
                        catch (Exception ex)
                        {
                            ReportCDVRAdapterError(adapterId, cdvrAdapterUrl, ex, "GetRecordingStatus");
                        }
                    }

                    LogAdapterResponse(adapterResponse, "GetRecordingStatus", adapterId,
                    string.Format("recordingId = {0}", recordingId));

                    #endregion
                }

                if (adapterResponse != null && adapterResponse.Status != null)
                {
                    // If something went wrong in the adapter, throw relevant exception
                    if (adapterResponse.Status.Code != (int)eResponseStatus.OK)
                    {
                        throw new KalturaException("Adapter failed completing request", (int)eResponseStatus.AdapterAppFailure);
                    }
                    else if (adapterResponse.Recording != null)
                    {
                        recordResult = CreateRecordResult(adapterResponse);
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error in GetRecordingStatus (Cdvr): error = {0}, adapterID = {1}, adapter ExternalIdentifier = {2}, RecordingId = {3}, channelId = {4}",
                    ex, adapterId, adapter.ExternalIdentifier, recordingId, channelId
                    );
                throw new KalturaException("Adapter failed completing request", (int)eResponseStatus.AdapterAppFailure);
            }
            return recordResult;
        }

        public RecordResult UpdateRecordingSchedule(int partnerId, string epgId, string channelId, string recordingId, int adapterId, long startDateSeconds, long durationSeconds)
        {
            RecordResult recordResult = new RecordResult();

            CDVRAdapter adapter = CdvrAdapterCache.Instance().GetCdvrAdapter(partnerId, adapterId);

            if (adapter == null)
            {
                throw new KalturaException(string.Format("Cdvr Adapter {0} doesn't exist", adapter.ID), (int)eResponseStatus.AdapterNotExists);
            }

            if (string.IsNullOrEmpty(adapter.AdapterUrl))
            {
                throw new KalturaException("Cdvr adapter has no URL", (int)eResponseStatus.AdapterUrlRequired);
            }
            string cdvrAdapterUrl = adapter.AdapterUrl;
            var client = GetCDVRAdapterServiceClient(cdvrAdapterUrl);

            //set unixTimestamp
            long timeStamp = TVinciShared.DateUtils.DateTimeToUtcUnixTimestampSeconds(DateTime.UtcNow);

            //TODO: verify that signature is correct
            string signature = string.Concat(recordingId, channelId, adapterId, startDateSeconds, durationSeconds, timeStamp);

            try
            {
                log.DebugFormat("Sending request to cdvr adapter. partnerId ID = {0}, adapterID = {1}, recordingId = {2}, channelId = {3}",
                    partnerId, adapter.ID, recordingId, channelId);

                var adapterResponse = new AdapterControllers.cdvrAdap.RecordingResponse();

                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    //call Adapter UpdateRecordingSchedule
                    try
                    {
                        adapterResponse =
                            client.UpdateRecordingScheduleAsync(recordingId, epgId, channelId, adapterId, startDateSeconds, durationSeconds,
                            timeStamp, Utils.GetSignature(adapter.SharedSecret, signature))
                            .ExecuteAndWait();
                    }
                    catch (Exception ex)
                    {
                        ReportCDVRAdapterError(adapterId, cdvrAdapterUrl, ex, "UpdateRecordingSchedule");
                    }
                }

                LogAdapterResponse(adapterResponse, "UpdateRecordingSchedule", adapterId,
                    string.Format("recordingId = {0}, startDateSeconds = {1}, durationSeconds = {2}", recordingId, startDateSeconds, durationSeconds));

                if (adapterResponse != null && adapterResponse.Status != null && adapterResponse.Status.Code == STATUS_NO_CONFIGURATION_FOUND)
                {
                    #region Send Configuration if not found

                    string key = string.Format("Cdvr_Adapter_Locker_{0}", adapterId);

                    // Build dictionary for synchronized action
                    Dictionary<string, object> parameters = new Dictionary<string, object>()
                    {
                        {PARAMETER_ADAPTER, adapter},
                        {PARAMETER_GROUP_ID, partnerId}
                    };

                    configurationSynchronizer.DoAction(key, parameters);

                    using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                    {
                        //call Adapter UpdateRecordingSchedule - after it is configured
                        try
                        {
                            adapterResponse =
                                client.UpdateRecordingScheduleAsync(recordingId, epgId, channelId, adapterId, startDateSeconds, durationSeconds,
                                timeStamp, Utils.GetSignature(adapter.SharedSecret, signature))
                                .ExecuteAndWait();
                        }
                        catch (Exception ex)
                        {
                            ReportCDVRAdapterError(adapterId, cdvrAdapterUrl, ex, "UpdateRecordingSchedule");
                        }
                    }

                    LogAdapterResponse(adapterResponse, "UpdateRecordingSchedule", adapterId,
                    string.Format("recordingId = {0}, startDateSeconds = {1}, durationSeconds = {2}, channelId = {3}", recordingId, startDateSeconds, durationSeconds, channelId));

                    #endregion
                }

                if (adapterResponse != null && adapterResponse.Status != null)
                {
                    // If something went wrong in the adapter, throw relevant exception
                    if (adapterResponse.Status.Code != (int)eResponseStatus.OK)
                    {
                        throw new KalturaException("Adapter failed completing request", (int)eResponseStatus.AdapterAppFailure);
                    }
                    else if (adapterResponse.Recording != null)
                    {
                        recordResult = CreateRecordResult(adapterResponse);
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error in UpdateRecordingSchedule (Cdvr): error = {0}, adapterID = {1}, adapter ExternalIdentifier = {2}, RecordingId = {3}, channelId = {4}",
                    ex, adapterId, adapter.ExternalIdentifier, recordingId, channelId
                    );
                throw new KalturaException("Adapter failed completing request", (int)eResponseStatus.AdapterAppFailure);
            }
            return recordResult;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="partnerId"></param>
        /// <param name="recordingId">External recording ID</param>
        /// <param name="adapterId"></param>
        /// <returns></returns>
        public RecordResult CancelRecording(int partnerId, string epgId, string channelId, string recordingId, int adapterId, long domainId)
        {
            RecordResult recordResult = new RecordResult();

            CDVRAdapter adapter = CdvrAdapterCache.Instance().GetCdvrAdapter(partnerId, adapterId);

            if (adapter == null)
            {
                throw new KalturaException(string.Format("Cdvr Adapter {0} doesn't exist", adapter.ID), (int)eResponseStatus.AdapterNotExists);
            }

            if (string.IsNullOrEmpty(adapter.AdapterUrl))
            {
                throw new KalturaException("Cdvr adapter has no URL", (int)eResponseStatus.AdapterUrlRequired);
            }

            string cdvrAdapterUrl = adapter.AdapterUrl;
            var client = GetCDVRAdapterServiceClient(cdvrAdapterUrl);

            //set unixTimestamp
            long timeStamp = TVinciShared.DateUtils.DateTimeToUtcUnixTimestampSeconds(DateTime.UtcNow);

            //TODO: verify that signature is correct
            string signature = string.Concat(recordingId, channelId, adapterId, timeStamp);

            try
            {
                log.DebugFormat("Sending request to cdvr adapter. partnerId ID = {0}, adapterID = {1}, recordingId = {2}, channelId = {3}",
                    partnerId, adapter.ID, recordingId, channelId);

                var adapterResponse = new AdapterControllers.cdvrAdap.RecordingResponse();

                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    //call Adapter CancelRecording
                    try
                    {
                        adapterResponse = client
                            .CancelRecordingAsync(recordingId, epgId, channelId, domainId, adapterId, timeStamp, Utils.GetSignature(adapter.SharedSecret, signature))
                            .ExecuteAndWait();
                    }
                    catch (Exception ex)
                    {
                        ReportCDVRAdapterError(adapterId, cdvrAdapterUrl, ex, "CancelRecording");
                    }
                }

                LogAdapterResponse(adapterResponse, "CancelRecording", adapterId,
                    string.Format("recordingId = {0}", recordingId));

                if (adapterResponse != null && adapterResponse.Status != null && adapterResponse.Status.Code == STATUS_NO_CONFIGURATION_FOUND)
                {
                    #region Send Configuration if not found

                    string key = string.Format("Cdvr_Adapter_Locker_{0}", adapterId);

                    // Build dictionary for synchronized action
                    Dictionary<string, object> parameters = new Dictionary<string, object>()
                    {
                        {PARAMETER_ADAPTER, adapter},
                        {PARAMETER_GROUP_ID, partnerId}
                    };

                    configurationSynchronizer.DoAction(key, parameters);

                    using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                    {
                        //call Adapter CancelRecording - after it is configured
                        try
                        {
                            adapterResponse = client
                                .CancelRecordingAsync(recordingId, epgId, channelId, domainId, adapterId, timeStamp, Utils.GetSignature(adapter.SharedSecret, signature))
                                .ExecuteAndWait();
                        }
                        catch (Exception ex)
                        {
                            ReportCDVRAdapterError(adapterId, cdvrAdapterUrl, ex, "CancelRecording");
                        }
                    }

                    LogAdapterResponse(adapterResponse, "CancelRecording", adapterId,
                    string.Format("recordingId = {0}", recordingId));

                    #endregion
                }

                if (adapterResponse != null && adapterResponse.Status != null)
                {
                    // If something went wrong in the adapter, throw relevant exception
                    if (adapterResponse.Status.Code != (int)eResponseStatus.OK)
                    {
                        throw new KalturaException("Adapter failed completing request", (int)eResponseStatus.AdapterAppFailure);
                    }
                    else if (adapterResponse.Recording != null)
                    {
                        recordResult = CreateRecordResult(adapterResponse);
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error in CancelRecording (Cdvr): error = {0}, adapterID = {1}, adapter ExternalIdentifier = {2}, RecordingId = {3}, channelId = {3}",
                    ex, adapterId, adapter.ExternalIdentifier, recordingId, channelId
                    );
                throw new KalturaException("Adapter failed completing request", (int)eResponseStatus.AdapterAppFailure);
            }
            return recordResult;
        }

        public RecordResult DeleteRecording(int partnerId, string epgId, string channelId, string recordingId, int adapterId, List<long> domainIds)
        {
            RecordResult recordResult = new RecordResult();

            CDVRAdapter adapter = CdvrAdapterCache.Instance().GetCdvrAdapter(partnerId, adapterId);

            if (adapter == null)
            {
                throw new KalturaException(string.Format("Cdvr Adapter {0} doesn't exist", adapter.ID), (int)eResponseStatus.AdapterNotExists);
            }

            if (string.IsNullOrEmpty(adapter.AdapterUrl))
            {
                throw new KalturaException("Cdvr adapter has no URL", (int)eResponseStatus.AdapterUrlRequired);
            }

            string cdvrAdapterUrl = adapter.AdapterUrl;
            var client = GetCDVRAdapterServiceClient(cdvrAdapterUrl);

            //set unixTimestamp
            long timeStamp = TVinciShared.DateUtils.DateTimeToUtcUnixTimestampSeconds(DateTime.UtcNow);

            //TODO: verify that signature is correct
            string signature = string.Concat(recordingId, channelId, adapterId, timeStamp);

            try
            {
                log.DebugFormat("Sending request to cdvr adapter. partnerId ID = {0}, adapterID = {1}, recordingId = {2}, channelId = {3}",
                    partnerId, adapter.ID, recordingId, channelId);

                var adapterResponse = new AdapterControllers.cdvrAdap.RecordingResponse();

                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    //call Adapter DeleteRecording
                    try
                    {
                        adapterResponse = client
                            .DeleteRecordingAsync(recordingId, epgId, channelId, domainIds, adapterId, timeStamp, Utils.GetSignature(adapter.SharedSecret, signature))
                            .ExecuteAndWait();
                    }
                    catch (Exception ex)
                    {
                        ReportCDVRAdapterError(adapterId, cdvrAdapterUrl, ex, "DeleteRecording");
                    }
                }

                LogAdapterResponse(adapterResponse, "DeleteRecording", adapterId,
                    string.Format("recordingId = {0}", recordingId));

                if (adapterResponse != null && adapterResponse.Status != null && adapterResponse.Status.Code == STATUS_NO_CONFIGURATION_FOUND)
                {
                    #region Send Configuration if not found

                    string key = string.Format("Cdvr_Adapter_Locker_{0}", adapterId);

                    // Build dictionary for synchronized action
                    Dictionary<string, object> parameters = new Dictionary<string, object>()
                    {
                        {PARAMETER_ADAPTER, adapter},
                        {PARAMETER_GROUP_ID, partnerId}
                    };

                    configurationSynchronizer.DoAction(key, parameters);

                    using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                    {
                        //call Adapter - DeleteRecording after it is configured
                        try
                        {
                            adapterResponse = client
                                .DeleteRecordingAsync(recordingId, epgId, channelId, domainIds, adapterId, timeStamp, Utils.GetSignature(adapter.SharedSecret, signature))
                                .ExecuteAndWait();
                        }
                        catch (Exception ex)
                        {
                            ReportCDVRAdapterError(adapterId, cdvrAdapterUrl, ex, "DeleteRecording");
                        }
                    }

                    LogAdapterResponse(adapterResponse, "DeleteRecording", adapterId,
                    string.Format("recordingId = {0}", recordingId));

                    #endregion
                }

                if (adapterResponse != null && adapterResponse.Status != null)
                {
                    // If something went wrong in the adapter, throw relevant exception
                    if (adapterResponse.Status.Code != (int)eResponseStatus.OK)
                    {
                        throw new KalturaException("Adapter failed completing request", (int)eResponseStatus.AdapterAppFailure);
                    }
                    else if (adapterResponse.Recording != null)
                    {
                        recordResult = CreateRecordResult(adapterResponse);
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error in DeleteRecording (Cdvr): error = {0}, adapterID = {1}, adapter ExternalIdentifier = {2}, RecordingId = {3}, channelId = {4}",
                    ex, adapterId, adapter.ExternalIdentifier, recordingId, channelId
                    );
                throw new KalturaException("Adapter failed completing request", (int)eResponseStatus.AdapterAppFailure);
            }
            return recordResult;
        }

        public RecordResult GetRecordingLinks(int partnerId, string channelId, string recordingId, int adapterId, long domainId)
        {
            RecordResult recordResult = new RecordResult();

            CDVRAdapter adapter = CdvrAdapterCache.Instance().GetCdvrAdapter(partnerId, adapterId);

            if (adapter == null)
            {
                throw new KalturaException(string.Format("Cdvr Adapter {0} doesn't exist", adapter.ID), (int)eResponseStatus.AdapterNotExists);
            }

            if (string.IsNullOrEmpty(adapter.AdapterUrl))
            {
                throw new KalturaException("Cdvr adapter has no URL", (int)eResponseStatus.AdapterUrlRequired);
            }

            string cdvrAdapterUrl = adapter.AdapterUrl;
            var client = GetCDVRAdapterServiceClient(cdvrAdapterUrl);

            //set unixTimestamp
            long timeStamp = TVinciShared.DateUtils.DateTimeToUtcUnixTimestampSeconds(DateTime.UtcNow);

            //TODO: verify that signature is correct
            string signature = string.Concat(recordingId, channelId, adapterId, timeStamp);

            try
            {
                log.DebugFormat("Sending request to cdvr adapter. partnerId ID = {0}, adapterID = {1}, recordingId = {2}, channelId = {3}",
                    partnerId, adapter.ID, recordingId, channelId);

                var adapterResponse = new AdapterControllers.cdvrAdap.RecordingResponse();

                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    //call Adapter GetRecordingLinks
                    try
                    {
                        adapterResponse = client
                            .GetRecordingLinksAsync(recordingId, channelId, domainId, adapterId, timeStamp, Utils.GetSignature(adapter.SharedSecret, signature))
                            .ExecuteAndWait();
                    }
                    catch (Exception ex)
                    {
                        ReportCDVRAdapterError(adapterId, cdvrAdapterUrl, ex, "GetRecordingLinks");
                    }
                }

                LogAdapterResponse(adapterResponse, "GetRecordingLinks", adapterId,
                    string.Format("recordingId = {0}", recordingId));

                if (adapterResponse != null && adapterResponse.Status != null && adapterResponse.Status.Code == STATUS_NO_CONFIGURATION_FOUND)
                {
                    #region Send Configuration if not found

                    string key = string.Format("Cdvr_Adapter_Locker_{0}", adapterId);

                    // Build dictionary for synchronized action
                    Dictionary<string, object> parameters = new Dictionary<string, object>()
                    {
                        {PARAMETER_ADAPTER, adapter},
                        {PARAMETER_GROUP_ID, partnerId}
                    };

                    configurationSynchronizer.DoAction(key, parameters);

                    using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                    {
                        //call Adapter GetRecordingLinks - after it is configured
                        try
                        {
                            adapterResponse = client
                                .GetRecordingLinksAsync(recordingId, channelId, domainId, adapterId, timeStamp, Utils.GetSignature(adapter.SharedSecret, signature))
                                .ExecuteAndWait();
                        }
                        catch (Exception ex)
                        {
                            ReportCDVRAdapterError(adapterId, cdvrAdapterUrl, ex, "GetRecordingLinks");
                        }
                    }

                    LogAdapterResponse(adapterResponse, "GetRecordingLinks", adapterId,
                        string.Format("recordingId = {0}", recordingId));

                    #endregion
                }

                if (adapterResponse != null && adapterResponse.Status != null)
                {
                    // If something went wrong in the adapter, throw relevant exception
                    if (adapterResponse.Status.Code != (int)eResponseStatus.OK)
                    {
                        throw new KalturaException("Adapter failed completing request", (int)eResponseStatus.AdapterAppFailure);
                    }
                    else if (adapterResponse.Recording != null)
                    {
                        recordResult = CreateRecordResult(adapterResponse);
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error in GetRecordingLinks (Cdvr): error = {0}, adapterID = {1}, adapter ExternalIdentifier = {2}, RecordingId = {3}, channelId = {4}",
                    ex, adapterId, adapter.ExternalIdentifier, recordingId, channelId
                    );
                throw new KalturaException("Adapter failed completing request", (int)eResponseStatus.AdapterAppFailure);
            }
            return recordResult;
        }

        public ApiObjects.TimeShiftedTv.RecordingResponse SearchCloudRecordings(int adapterId, int partnerId, string userId, long domainId, Dictionary<string, string> adapterData, List<TstvRecordingStatus> recordingStatuses, int pageIndex, int pageSize)
        {
            var recordingResult = new ApiObjects.TimeShiftedTv.RecordingResponse();

            CDVRAdapter adapter = CdvrAdapterCache.Instance().GetCdvrAdapter(partnerId, adapterId);

            if (adapter == null)
            {
                throw new KalturaException(string.Format("Cdvr Adapter {0} doesn't exist", adapter.ID), (int)eResponseStatus.AdapterNotExists);
            }

            if (string.IsNullOrEmpty(adapter.AdapterUrl))
            {
                throw new KalturaException("Cdvr adapter has no URL", (int)eResponseStatus.AdapterUrlRequired);
            }

            string cdvrAdapterUrl = adapter.AdapterUrl;
            var client = GetCDVRAdapterServiceClient(cdvrAdapterUrl);

            
            var adapterdDataToSend = adapterData != null ? adapterData.Select(x => new KeyValue()
                {
                    Key = x.Key,
                    Value = x.Value
                }).ToList() : null;
            
            //set unixTimestamp
            long timeStamp = TVinciShared.DateUtils.DateTimeToUtcUnixTimestampSeconds(DateTime.UtcNow);

            //TODO: verify that signature is correct
            string signature = string.Concat(userId, domainId, adapterdDataToSend != null ? string.Concat(adapterdDataToSend.Select(kv => string.Concat(kv.Key, kv.Value))) : string.Empty, adapterId, timeStamp);

            try
            {
                log.DebugFormat("Sending request to cdvr adapter. partnerId ID = {0}, adapterID = {1}, adapterData = {2}",
                    partnerId, adapter.ID, adapterData);

                var adapterResponse = new ExternalRecordingResponse();

                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    //call Adapter GetRecordingLinks
                    try
                    {
                        adapterResponse = client
                            .GetCloudRecordingAsync(userId, domainId, adapterdDataToSend, adapterId, timeStamp, Utils.GetSignature(adapter.SharedSecret, signature))
                            .ExecuteAndWait();
                    }
                    catch (Exception ex)
                    {
                        ReportCDVRAdapterError(adapterId, cdvrAdapterUrl, ex, "GetCloudRecording");
                    }
                }

                LogAdapterResponse(adapterResponse, "GetCloudRecording", adapterId);

                if (adapterResponse != null && adapterResponse.Status != null && adapterResponse.Status.Code == STATUS_NO_CONFIGURATION_FOUND)
                {
                    #region Send Configuration if not found

                    string key = string.Format("Cdvr_Adapter_Locker_{0}", adapterId);

                    // Build dictionary for synchronized action
                    Dictionary<string, object> parameters = new Dictionary<string, object>()
                    {
                        {PARAMETER_ADAPTER, adapter},
                        {PARAMETER_GROUP_ID, partnerId}
                    };

                    configurationSynchronizer.DoAction(key, parameters);


                    using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                    {
                        //call Adapter GetRecordingLinks
                        try
                        {
                            adapterResponse = client
                                .GetCloudRecordingAsync(userId, domainId, adapterdDataToSend, adapterId, timeStamp, Utils.GetSignature(adapter.SharedSecret, signature))
                                .ExecuteAndWait();
                        }
                        catch (Exception ex)
                        {
                            ReportCDVRAdapterError(adapterId, cdvrAdapterUrl, ex, "GetCloudRecording");
                        }
                    }

                    LogAdapterResponse(adapterResponse, "GetCloudRecording", adapterId);

                    #endregion
                }

                if (adapterResponse != null && adapterResponse.Status != null)
                {
                    // If something went wrong in the adapter, throw relevant exception
                    if (adapterResponse.Status.Code != (int)eResponseStatus.OK)
                    {
                        throw new KalturaException("Adapter failed completing request", (int)eResponseStatus.AdapterAppFailure);
                    }
                    else if (adapterResponse.ExternalRecordings != null)
                    {
                        recordingResult = CreateRecordingResult(adapterResponse.ExternalRecordings);
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error in GetCloudRecording (Cdvr): error = {0}, adapterID = {1}, adapter ExternalIdentifier = {2}, adapterData = {3}",
                    ex, adapterId, adapter.ExternalIdentifier, adapterData);
                throw new KalturaException("Adapter failed completing request", (int)eResponseStatus.AdapterAppFailure);
            }

            return recordingResult;
        }

        public ApiObjects.TimeShiftedTv.SeriesResponse SearchCloudSeriesRecordings(int adapterId, int partnerId, string userId, long domainId, Dictionary<string, string> adapterData)
        {
            var externalSeriesRecordingResult = new ApiObjects.TimeShiftedTv.SeriesResponse();

            CDVRAdapter adapter = CdvrAdapterCache.Instance().GetCdvrAdapter(partnerId, adapterId);

            if (adapter == null)
            {
                throw new KalturaException(string.Format("Cdvr Adapter {0} doesn't exist", adapter.ID), (int)eResponseStatus.AdapterNotExists);
            }

            if (string.IsNullOrEmpty(adapter.AdapterUrl))
            {
                throw new KalturaException("Cdvr adapter has no URL", (int)eResponseStatus.AdapterUrlRequired);
            }

            string cdvrAdapterUrl = adapter.AdapterUrl;
            var client = GetCDVRAdapterServiceClient(cdvrAdapterUrl);

            //set unixTimestamp
            long timeStamp = TVinciShared.DateUtils.DateTimeToUtcUnixTimestampSeconds(DateTime.UtcNow);

            var adapterdDataToSend = adapterData != null ? adapterData.Select(x => new KeyValue()
            {
                Key = x.Key,
                Value = x.Value
            }).ToList() : null;

            //TODO: verify that signature is correct
            string signature = string.Concat(userId, domainId, adapterdDataToSend != null ? string.Concat(adapterdDataToSend.Select(kv => string.Concat(kv.Key, kv.Value))) : string.Empty, adapterId, timeStamp);

            try
            {
                log.DebugFormat("Sending request to cdvr adapter. partnerId ID = {0}, adapterID = {1}",
                    partnerId, adapter.ID);

                var adapterResponse = new AdapterControllers.cdvrAdap.ExternalSeriesRecordingResponse();

                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    //call Adapter GetRecordingLinks
                    try
                    {
                        adapterResponse = client
                            .GetCloudSeriesRecordingAsync(userId, domainId, adapterdDataToSend, adapterId, timeStamp, Utils.GetSignature(adapter.SharedSecret, signature))
                            .ExecuteAndWait();
                    }
                    catch (Exception ex)
                    {
                        ReportCDVRAdapterError(adapterId, cdvrAdapterUrl, ex, "GetCloudRecording");
                    }
                }

                LogAdapterResponse(adapterResponse, "SearchCloudSeriesRecordings", adapterId);

                if (adapterResponse != null && adapterResponse.Status != null && adapterResponse.Status.Code == STATUS_NO_CONFIGURATION_FOUND)
                {
                    #region Send Configuration if not found

                    string key = string.Format("Cdvr_Adapter_Locker_{0}", adapterId);

                    // Build dictionary for synchronized action
                    Dictionary<string, object> parameters = new Dictionary<string, object>()
                    {
                        {PARAMETER_ADAPTER, adapter},
                        {PARAMETER_GROUP_ID, partnerId}
                    };

                    configurationSynchronizer.DoAction(key, parameters);

                    using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                    {
                        //call Adapter GetRecordingLinks
                        try
                        {
                            adapterResponse = client.GetCloudSeriesRecordingAsync(userId, domainId, adapterdDataToSend, adapterId, timeStamp, Utils.GetSignature(adapter.SharedSecret, signature))
                                .ExecuteAndWait();
                        }
                        catch (Exception ex)
                        {
                            ReportCDVRAdapterError(adapterId, cdvrAdapterUrl, ex, "SearchCloudSeriesRecordings");
                        }
                    }

                    LogAdapterResponse(adapterResponse, "SearchCloudSeriesRecordings", adapterId);

                    #endregion
                }

                if (adapterResponse != null && adapterResponse.Status != null)
                {
                    // If something went wrong in the adapter, throw relevant exception
                    if (adapterResponse.Status.Code != (int)eResponseStatus.OK)
                    {
                        throw new KalturaException("Adapter failed completing request", (int)eResponseStatus.AdapterAppFailure);
                    }
                    else if (adapterResponse.ExternalSeriesRecordings != null)
                    {
                        externalSeriesRecordingResult = CreateSeriesResponse(adapterResponse.ExternalSeriesRecordings);
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error in GetCloudRecording (Cdvr): error = {0}, adapterID = {1}, adapter ExternalIdentifier = {2}, adapterData = {3}",
                    ex, adapterId, adapter.ExternalIdentifier, adapterData);
                throw new KalturaException("Adapter failed completing request", (int)eResponseStatus.AdapterAppFailure);
            }

            return externalSeriesRecordingResult;
        }

        #endregion

        #region Private Method

        public static DateTime FromUnixTime(long unixTime)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return epoch.AddSeconds(unixTime);
        }

        private ApiObjects.TimeShiftedTv.RecordingResponse CreateRecordingResult(List<CloudRecording> externalRecordings)
        {
            var response = new ApiObjects.TimeShiftedTv.RecordingResponse
            {
                Recordings = new List<ApiObjects.TimeShiftedTv.Recording>()
            };

            long? futureDate = DateUtils.DateTimeToUtcUnixTimestampSeconds(DateTime.UtcNow.AddHours(1));
            foreach (var item in externalRecordings)
            {
                try
                {
                    response.Recordings.Add(new ExternalRecording
                    {
                        ChannelId = item.ChannelId,
                        CreateDate = FromUnixTime(item.CreateDate),
                        EpgId = item.EpgId,
                        ExternalDomainRecordingId = item.Id.ToString(),
                        IsProtected = item.IsProtected,
                        RecordingStatus = (TstvRecordingStatus)item.RecordingStatus,
                        Type = (RecordingType)item.Type,
                        UpdateDate = FromUnixTime(item.UpdateDate),
                        ViewableUntilDate = item.ViewableUntilDate,
                        MetaData = item.MetaData.ToDictionary(x => x.Key, y => y.Value),
                        ProtectedUntilDate = item.IsProtected ? futureDate : null //BEO-10901 
                    });
                }
                catch { }
            }

            return response;
        }

        private SeriesResponse CreateSeriesResponse(List<CloudSeriesRecording> externalSeriesRecordings)
        {
            var response = new ApiObjects.TimeShiftedTv.SeriesResponse
            {
                SeriesRecordings = new List<ApiObjects.TimeShiftedTv.SeriesRecording>()
            };

            foreach (var item in externalSeriesRecordings)
            {
                try
                {
                    response.SeriesRecordings.Add(new ExternalSeriesRecording
                    {
                        EpgChannelId = item.EpgChannelId,
                        CreateDate = FromUnixTime(item.CreateDate),
                        EpgId = item.EpgId,
                        Id = item.Id,
                        Type = (RecordingType)item.Type,
                        UpdateDate = FromUnixTime(item.UpdateDate),
                        SeasonNumber = item.SeasonNumber,
                        SeriesId = item.SeriesId,
                        MetaData = item.MetaData != null ? item.MetaData.ToDictionary(x => x.Key, y => y.Value) : null,
                        isExternalRecording = true
                    });
                }
                catch { }
            }

            return response;
        }

        private static RecordResult CreateRecordResult(cdvrAdap.RecordingResponse adapterResponse)
        {
            List<ApiObjects.TimeShiftedTv.RecordingLink> links = new List<ApiObjects.TimeShiftedTv.RecordingLink>();
            List<long> failedDomainIds = new List<long>();

            if (adapterResponse.Recording.Links != null)
            {
                links = adapterResponse.Recording.Links.Select(result =>
                        new ApiObjects.TimeShiftedTv.RecordingLink()
                        {
                            FileType = result.FileType,
                            Url = result.Url
                        }).ToList();
            }

            if (adapterResponse.Recording.FailedDomainIds != null)
            {
                failedDomainIds.AddRange(adapterResponse.Recording.FailedDomainIds);
            }

            bool actionSuccess = adapterResponse.Recording.RecordingState == 0;

            RecordResult recordResult = new RecordResult()
            {
                Links = links,
                RecordingId = adapterResponse.Recording.RecordingId,
                ActionSuccess = actionSuccess,
                FailReason = adapterResponse.Recording.FailReason,
                ProviderStatusCode = adapterResponse.Recording.ProviderStatusCode,
                ProviderStatusMessage = adapterResponse.Recording.ProviderStatusMessage,
                FailedDomainIds = failedDomainIds
            };

            return recordResult;
        }

        private bool configurationSynchronizer_SynchronizedAct(Dictionary<string, object> parameters)
        {
            bool result = false;

            if (parameters != null)
            {
                int partnerId = 0;
                CDVRAdapter adapter = null;

                if (parameters.ContainsKey(PARAMETER_GROUP_ID))
                {
                    partnerId = (int)parameters[PARAMETER_GROUP_ID];
                }

                if (parameters.ContainsKey(PARAMETER_ADAPTER))
                {
                    adapter = (CDVRAdapter)parameters[PARAMETER_ADAPTER];
                }

                // get the right 
                result = this.SetConfiguration(adapter, partnerId);
            }

            return result;
        }

        private static void LogAdapterResponse(AdapterControllers.cdvrAdap.RecordingResponse adapterResponse, string action,
            int adapterId, string inputParameters)
        {
            string logMessage = string.Empty;

            if (adapterResponse == null)
            {
                logMessage = string.Format("Cdvr Adapter {0} Result is null", action != null ? action : string.Empty);
            }
            else if (adapterResponse.Status == null)
            {
                logMessage = string.Format("Cdvr Adapter {0} Result's status is null", action != null ? action : string.Empty);
            }
            else
            {
                if (adapterResponse.Recording == null)
                {
                    logMessage = string.Format("Cdvr Adapter {0} Result Status: Message = {1}, Code = {2}. Recording is null",
                                     action != null ? action : string.Empty,                                                                                                                // {0}
                                     adapterResponse != null && adapterResponse.Status != null && adapterResponse.Status.Message != null ? adapterResponse.Status.Message : string.Empty,   // {1}
                                     adapterResponse != null && adapterResponse.Status != null ? adapterResponse.Status.Code : -1);                                                         // {2}
                }
                else
                {
                    logMessage = string.Format("Cdvr Adapter {0} Result Status: Message = {1}, Code = {2}. Recording: RecordingId = {3}, RecordingState = {4}",
                        // {0}
                        action != null ? action : string.Empty,
                        // {1}
                        adapterResponse != null && adapterResponse.Status != null && adapterResponse.Status.Message != null ? adapterResponse.Status.Message : string.Empty,
                        // {2}
                        adapterResponse != null && adapterResponse.Status != null ? adapterResponse.Status.Code : -1,
                        //{3}
                        adapterResponse.Recording.RecordingId,
                        // {4}
                        adapterResponse.Recording.RecordingState);

                    // if Status code is not 0 OR fail reason is not 0 OR
                    // provider status code is not 0 (and not empty...)
                    if ((adapterResponse.Status.Code != 0) ||
                        (adapterResponse.Recording.FailReason != 0) ||
                        (!string.IsNullOrEmpty(adapterResponse.Recording.ProviderStatusCode) &&
                        adapterResponse.Recording.ProviderStatusCode != "0"))
                    {
                        ReportCDVRProviderFailure(adapterId, action, inputParameters,
                            adapterResponse.Status.Code, adapterResponse.Recording.ProviderStatusCode, adapterResponse.Recording.ProviderStatusMessage,
                            adapterResponse.Recording.FailReason);
                    }
                }
            }

            log.Debug(logMessage);
        }

        private void LogAdapterResponse(ExternalRecordingResponse adapterResponse, string action, int adapterId)
        {
            string logMessage = string.Empty;

            if (adapterResponse == null)
            {
                logMessage = string.Format("Cdvr Adapter {0} Result is null", action != null ? action : string.Empty);
            }
            else if (adapterResponse.Status == null)
            {
                logMessage = string.Format("Cdvr Adapter {0} Result's status is null", action != null ? action : string.Empty);
            }
            else
            {
                if (adapterResponse.ExternalRecordings == null)
                {
                    logMessage = string.Format("Cdvr Adapter {0} Result Status: Message = {1}, Code = {2}. Recording is null",
                                     action != null ? action : string.Empty,                                                                                                                // {0}
                                     adapterResponse != null && adapterResponse.Status != null && adapterResponse.Status.Message != null ? adapterResponse.Status.Message : string.Empty,   // {1}
                                     adapterResponse != null && adapterResponse.Status != null ? adapterResponse.Status.Code : -1);                                                         // {2}
                }
                else
                {
                    logMessage = string.Format("Cdvr Adapter {0} Result Status: Message = {1}, Code = {2}. Recording: RecordingIds = {3}",
                        // {0}
                        action != null ? action : string.Empty,
                        // {1}
                        adapterResponse != null && adapterResponse.Status != null && adapterResponse.Status.Message != null ? adapterResponse.Status.Message : string.Empty,
                        // {2}
                        adapterResponse != null && adapterResponse.Status != null ? adapterResponse.Status.Code : -1,
                        //{3}
                        string.Join(",", adapterResponse.ExternalRecordings.Select(x => x.Id)));
                }
            }

            log.Debug(logMessage);
        }

        private void LogAdapterResponse(ExternalSeriesRecordingResponse adapterResponse, string action,
            int adapterId)
        {
            string logMessage = string.Empty;

            if (adapterResponse == null)
            {
                logMessage = string.Format("Cdvr Adapter {0} Result is null", action != null ? action : string.Empty);
            }
            else if (adapterResponse.Status == null)
            {
                logMessage = string.Format("Cdvr Adapter {0} Result's status is null", action != null ? action : string.Empty);
            }
            else
            {
                if (adapterResponse.ExternalSeriesRecordings == null)
                {
                    logMessage = string.Format("Cdvr Adapter {0} Result Status: Message = {1}, Code = {2}. Recording is null",
                                     action != null ? action : string.Empty,                                                                                                                // {0}
                                     adapterResponse != null && adapterResponse.Status != null && adapterResponse.Status.Message != null ? adapterResponse.Status.Message : string.Empty,   // {1}
                                     adapterResponse != null && adapterResponse.Status != null ? adapterResponse.Status.Code : -1);                                                         // {2}
                }
                else
                {
                    logMessage = string.Format("Cdvr Adapter {0} Result Status: Message = {1}, Code = {2}. Recording: RecordingIds = {3}",
                        // {0}
                        action != null ? action : string.Empty,
                        // {1}
                        adapterResponse != null && adapterResponse.Status != null && adapterResponse.Status.Message != null ? adapterResponse.Status.Message : string.Empty,
                        // {2}
                        adapterResponse != null && adapterResponse.Status != null ? adapterResponse.Status.Code : -1,
                        //{3}
                        string.Join(",", adapterResponse.ExternalSeriesRecordings.Select(x => x.Id)));
                }
            }

            log.Debug(logMessage);
        }

        private static void ReportCDVRAdapterError(int adapterId, string url, Exception ex, string action, bool throwException = true)
        {
            string cdvrAdapter = "C-DVR adapter";

            if (HttpContext.Current?.Items != null)
            {
                if (HttpContext.Current?.Items.Count > 0 && HttpContext.Current.Items.ContainsKey(Constants.TOPIC))
                {
                    var previousTopic = HttpContext.Current.Items[Constants.TOPIC];
                    HttpContext.Current.Items[Constants.TOPIC] = cdvrAdapter;

                    log.ErrorFormat("Failed communicating with adapter. Adapter identifier: {0}, Adapter URL: {1}, Adapter Api: {2}. Error: {3}",
                        adapterId,
                        url,
                        action,
                        ex);
                    HttpContext.Current.Items[Constants.TOPIC] = previousTopic;

                    if (throwException)
                    {
                        throw ex;
                    }
                }
                else
                {
                    HttpContext.Current.Items.Add(Constants.TOPIC, cdvrAdapter);
                    log.DebugFormat("ReportCDVRAdapterError added {0} to HttpContext.Current.Items. Adapter identifier: {1}, Adapter URL: {2}, Adapter Api: {3}",
                        Constants.TOPIC, adapterId, url, action, ex);
                }
            }
        }

        private static void ReportCDVRProviderFailure(int adapterId, string action, string inputParameters, int errorCode, string providerCode,
            string providerMessage, int failReason)
        {
            string cdvrProvider = "C-DVR provider";

            if (HttpContext.Current?.Items != null)
            {
                if (HttpContext.Current?.Items.Count > 0 && HttpContext.Current.Items.ContainsKey(Constants.TOPIC))
                {
                    var previousTopic = HttpContext.Current.Items[Constants.TOPIC];
                    HttpContext.Current.Items[Constants.TOPIC] = cdvrProvider;

                    log.ErrorFormat("Adapter was accessed successfully, but returned an error. " +
                        "Adapter identifier: {0}, Adapter Api: {1}. Input parameters: {2}. Error code: {3}, Provider Code: {4}, Provider Message: {5}, Fail Reason: {6}",
                        adapterId,
                        action,
                        inputParameters,
                        errorCode,
                        providerCode,
                        providerMessage,
                        failReason);
                    HttpContext.Current.Items[Constants.TOPIC] = previousTopic;
                }
                else
                {
                    HttpContext.Current.Items.Add(Constants.TOPIC, cdvrProvider);
                    log.DebugFormat("ReportCDVRProviderFailure added {0} to HttpContext.Current.Items. Adapter identifier: {1}, Adapter Api: {2},  Input parameters: {3}",
                       Constants.TOPIC, adapterId, action, inputParameters);
                }
            }
        }

        #endregion
    }
}