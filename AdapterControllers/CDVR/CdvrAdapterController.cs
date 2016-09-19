using AdapterControllers.cdvrAdap;
//using AdapterControllers.CdvrAdapterService;
using ApiObjects;
using ApiObjects.Response;
using CachingHelpers;
using KLogMonitor;
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

namespace AdapterControllers.CDVR
{
    public class CdvrAdapterController
    {
        #region Consts
        private const int STATUS_OK = 0;
        private const int STATUS_NO_CONFIGURATION_FOUND = 3;
                
        private const string PARAMETER_GROUP_ID = "group_id";
        private const string PARAMETER_ADAPTER = "adapter";
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
        public bool SetConfiguration(CDVRAdapter adapter, int partnerId)
        {
            bool result = false;
            try
            {
                string cdvrAdapterUrl = adapter.AdapterUrl;
                cdvrAdap.ServiceClient client = new cdvrAdap.ServiceClient();
                client.Endpoint.Address = new System.ServiceModel.EndpointAddress(cdvrAdapterUrl);            
                
                //set unixTimestamp
                long timeStamp = TVinciShared.DateUtils.DateTimeToUnixTimestamp(DateTime.UtcNow);
                //set signature
                string signature = string.Concat(adapter.Settings != null ? string.Concat(adapter.Settings.Select(kv => string.Concat(kv.key, kv.value))) : string.Empty, 
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
                adapterStatus = client.SetConfiguration(adapter.ID, keyValue, partnerId, timeStamp, Utils.GetSignature(adapter.SharedSecret, signature));

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

        public RecordResult Record(int partnerId, long startTimeSeconds, long durationSeconds, string channelId, int adapterId)
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
            cdvrAdap.ServiceClient client = new cdvrAdap.ServiceClient();
            client.Endpoint.Address = new System.ServiceModel.EndpointAddress(cdvrAdapterUrl);

            //set unixTimestamp
            long timeStamp = TVinciShared.DateUtils.DateTimeToUnixTimestamp(DateTime.UtcNow);

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
                        adapterResponse = client.Record(startTimeSeconds, durationSeconds, channelId, adapter.ID, timeStamp, Utils.GetSignature(adapter.SharedSecret, signature));
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
                            adapterResponse = client.Record(startTimeSeconds, durationSeconds, channelId, adapter.ID, timeStamp, Utils.GetSignature(adapter.SharedSecret, signature));
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
                log.ErrorFormat("Error in Record (Cdvr): error = {0}, adapterID = {1}, adapter ExternalIdentifier = {2}",
                    ex, adapterId, adapter.ExternalIdentifier
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
            cdvrAdap.ServiceClient client = new cdvrAdap.ServiceClient();
            client.Endpoint.Address = new System.ServiceModel.EndpointAddress(cdvrAdapterUrl);

            //set unixTimestamp
            long timeStamp = TVinciShared.DateUtils.DateTimeToUnixTimestamp(DateTime.UtcNow);

            //TODO: verify that signature is correct
            string signature = string.Concat(recordingId, adapterId, timeStamp);

            try
            {
                log.DebugFormat("Sending request to cdvr adapter. partnerId ID = {0}, adapterID = {1}, recordingId = {2}",
                    partnerId, adapter.ID, recordingId);

                var adapterResponse = new AdapterControllers.cdvrAdap.RecordingResponse();

                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    try
                    {
                        //call Adapter GetRecordingStatus
                        adapterResponse = client.GetRecordingStatus(recordingId, channelId, adapterId, timeStamp, Utils.GetSignature(adapter.SharedSecret, signature));
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
                            adapterResponse = client.GetRecordingStatus(recordingId, channelId, adapterId, timeStamp, Utils.GetSignature(adapter.SharedSecret, signature));
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
                log.ErrorFormat("Error in GetRecordingStatus (Cdvr): error = {0}, adapterID = {1}, adapter ExternalIdentifier = {2}, RecordingId = {3}",
                    ex, adapterId, adapter.ExternalIdentifier, recordingId
                    );
                throw new KalturaException("Adapter failed completing request", (int)eResponseStatus.AdapterAppFailure);
            }
            return recordResult;
        }

        public RecordResult UpdateRecordingSchedule(int partnerId, string channelId, string recordingId, int adapterId, long startDateSeconds, long durationSeconds)
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
            cdvrAdap.ServiceClient client = new cdvrAdap.ServiceClient();
            client.Endpoint.Address = new System.ServiceModel.EndpointAddress(cdvrAdapterUrl);

            //set unixTimestamp
            long timeStamp = TVinciShared.DateUtils.DateTimeToUnixTimestamp(DateTime.UtcNow);

            //TODO: verify that signature is correct
            string signature = string.Concat(recordingId, channelId, adapterId, startDateSeconds, durationSeconds, timeStamp);

            try
            {
                log.DebugFormat("Sending request to cdvr adapter. partnerId ID = {0}, adapterID = {1}, recordingId = {2}",
                    partnerId, adapter.ID, recordingId);

                var adapterResponse = new AdapterControllers.cdvrAdap.RecordingResponse();

                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    //call Adapter UpdateRecordingSchedule
                    try
                    {
                        adapterResponse =
                            client.UpdateRecordingSchedule(recordingId, channelId, adapterId, startDateSeconds, durationSeconds, 
                            timeStamp, Utils.GetSignature(adapter.SharedSecret, signature));
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
                                client.UpdateRecordingSchedule(recordingId, channelId, adapterId, startDateSeconds, durationSeconds,
                                timeStamp, Utils.GetSignature(adapter.SharedSecret, signature));
                        }
                        catch (Exception ex)
                        {
                            ReportCDVRAdapterError(adapterId, cdvrAdapterUrl, ex, "UpdateRecordingSchedule");
                        }
                    }

                    LogAdapterResponse(adapterResponse, "UpdateRecordingSchedule", adapterId,
                    string.Format("recordingId = {0}, startDateSeconds = {1}, durationSeconds = {2}", recordingId, startDateSeconds, durationSeconds));

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
                log.ErrorFormat("Error in UpdateRecordingSchedule (Cdvr): error = {0}, adapterID = {1}, adapter ExternalIdentifier = {2}, RecordingId = {3}",
                    ex, adapterId, adapter.ExternalIdentifier, recordingId
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
        public RecordResult CancelRecording(int partnerId, string channelId, string recordingId, int adapterId)
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
            cdvrAdap.ServiceClient client = new cdvrAdap.ServiceClient();
            client.Endpoint.Address = new System.ServiceModel.EndpointAddress(cdvrAdapterUrl);

            //set unixTimestamp
            long timeStamp = TVinciShared.DateUtils.DateTimeToUnixTimestamp(DateTime.UtcNow);

            //TODO: verify that signature is correct
            string signature = string.Concat(recordingId, channelId, adapterId, timeStamp);

            try
            {
                log.DebugFormat("Sending request to cdvr adapter. partnerId ID = {0}, adapterID = {1}, recordingId = {2}",
                    partnerId, adapter.ID, recordingId);

                var adapterResponse = new AdapterControllers.cdvrAdap.RecordingResponse();

                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    //call Adapter CancelRecording
                    try
                    {
                        adapterResponse = client.CancelRecording(recordingId, channelId, adapterId, timeStamp, Utils.GetSignature(adapter.SharedSecret, signature));
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
                            adapterResponse = client.CancelRecording(recordingId, channelId, adapterId, timeStamp, Utils.GetSignature(adapter.SharedSecret, signature));
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
                log.ErrorFormat("Error in CancelRecording (Cdvr): error = {0}, adapterID = {1}, adapter ExternalIdentifier = {2}, RecordingId = {3}",
                    ex, adapterId, adapter.ExternalIdentifier, recordingId
                    );
                throw new KalturaException("Adapter failed completing request", (int)eResponseStatus.AdapterAppFailure);
            }
            return recordResult;
        }

        public RecordResult DeleteRecording(int partnerId, string channelId, string recordingId, int adapterId)
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
            cdvrAdap.ServiceClient client = new cdvrAdap.ServiceClient();
            client.Endpoint.Address = new System.ServiceModel.EndpointAddress(cdvrAdapterUrl);

            //set unixTimestamp
            long timeStamp = TVinciShared.DateUtils.DateTimeToUnixTimestamp(DateTime.UtcNow);

            //TODO: verify that signature is correct
            string signature = string.Concat(recordingId, channelId, adapterId, timeStamp);

            try
            {
                log.DebugFormat("Sending request to cdvr adapter. partnerId ID = {0}, adapterID = {1}, recordingId = {2}",
                    partnerId, adapter.ID, recordingId);

                var adapterResponse = new AdapterControllers.cdvrAdap.RecordingResponse();

                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    //call Adapter DeleteRecording
                    try
                    {
                        adapterResponse = client.DeleteRecording(recordingId, channelId, adapterId, timeStamp, Utils.GetSignature(adapter.SharedSecret, signature));
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
                            adapterResponse = client.DeleteRecording(recordingId, channelId, adapterId, timeStamp, Utils.GetSignature(adapter.SharedSecret, signature));
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
                log.ErrorFormat("Error in DeleteRecording (Cdvr): error = {0}, adapterID = {1}, adapter ExternalIdentifier = {2}, RecordingId = {3}",
                    ex, adapterId, adapter.ExternalIdentifier, recordingId
                    );
                throw new KalturaException("Adapter failed completing request", (int)eResponseStatus.AdapterAppFailure);
            }
            return recordResult;
        }

        public RecordResult GetRecordingLinks(int partnerId, string channelId, string recordingId, int adapterId)
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
            cdvrAdap.ServiceClient client = new cdvrAdap.ServiceClient();
            client.Endpoint.Address = new System.ServiceModel.EndpointAddress(cdvrAdapterUrl);

            //set unixTimestamp
            long timeStamp = TVinciShared.DateUtils.DateTimeToUnixTimestamp(DateTime.UtcNow);

            //TODO: verify that signature is correct
            string signature = string.Concat(recordingId, channelId, adapterId, timeStamp);

            try
            {
                log.DebugFormat("Sending request to cdvr adapter. partnerId ID = {0}, adapterID = {1}, recordingId = {2}",
                    partnerId, adapter.ID, recordingId);

                var adapterResponse = new AdapterControllers.cdvrAdap.RecordingResponse();

                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    //call Adapter GetRecordingLinks
                    try
                    {
                        adapterResponse = client.GetRecordingLinks(recordingId, channelId, adapterId, timeStamp, Utils.GetSignature(adapter.SharedSecret, signature));
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
                            adapterResponse = client.GetRecordingLinks(recordingId, channelId, adapterId, timeStamp, Utils.GetSignature(adapter.SharedSecret, signature));
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
                log.ErrorFormat("Error in GetRecordingLinks (Cdvr): error = {0}, adapterID = {1}, adapter ExternalIdentifier = {2}, RecordingId = {3}",
                    ex, adapterId, adapter.ExternalIdentifier, recordingId
                    );
                throw new KalturaException("Adapter failed completing request", (int)eResponseStatus.AdapterAppFailure);
            }
            return recordResult;
        }

        #endregion

        #region Private Method
        
        private static RecordResult CreateRecordResult(cdvrAdap.RecordingResponse adapterResponse)
        {
            List<ApiObjects.TimeShiftedTv.RecordingLink> links = new List<ApiObjects.TimeShiftedTv.RecordingLink>();

            if (adapterResponse.Recording.Links != null)
            {
                links = adapterResponse.Recording.Links.Select(result =>
                        new ApiObjects.TimeShiftedTv.RecordingLink()
                        {
                            FileType = result.FileType,
                            Url = result.Url
                        }).ToList();
            }

            bool actionSuccess = adapterResponse.Recording.RecordingState == 0;

            RecordResult recordResult = new RecordResult()
            {
                Links = links,
                RecordingId = adapterResponse.Recording.RecordingId,
                ActionSuccess = actionSuccess,
                FailReason = adapterResponse.Recording.FailReason,
                ProviderStatusCode = adapterResponse.Recording.ProviderStatusCode,
                ProviderStatusMessage = adapterResponse.Recording.ProviderStatusMessage
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

        private void LogAdapterResponse(AdapterControllers.cdvrAdap.RecordingResponse adapterResponse, string action, 
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

        private static void ReportCDVRAdapterError(int adapterId, string url, Exception ex, string action, bool throwException = true)
        {
            var previousTopic = HttpContext.Current.Items[Constants.TOPIC];
            HttpContext.Current.Items[Constants.TOPIC] = "C-DVR adapter";

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

        private static void ReportCDVRProviderFailure(int adapterId, string action, string inputParameters, int errorCode, string providerCode,
            string providerMessage, int failReason)
        {
            var previousTopic = HttpContext.Current.Items[Constants.TOPIC];
            HttpContext.Current.Items[Constants.TOPIC] = "C-DVR provider";

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

        #endregion
    }
}
