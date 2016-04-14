using AdapterControllers.CdvrEngineAdapter;
using ApiObjects;
using ApiObjects.Response;
using CachingHelpers;
using KLogMonitor;
using Synchronizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TVinciShared;

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
        public bool SendConfiguration(CDVRAdapter adapter, int groupId)
        {
            bool result = false;
            try
            {
                CdvrEngineAdapter.IService client = new CdvrEngineAdapter.ServiceClient();
               
                //set unixTimestamp
                long unixTimestamp = TVinciShared.DateUtils.DateTimeToUnixTimestamp(DateTime.UtcNow);
                //set signature
                string signature = string.Empty;

                //call Adapter 
                List<AdapterControllers.CdvrEngineAdapter.KeyValue> keyValue = new List<KeyValue>();
                CdvrEngineAdapter.AdapterStatus adapterStatus = null;
                if (adapter.Settings != null)
                {
                    keyValue = adapter.Settings.Select(setting => new AdapterControllers.CdvrEngineAdapter.KeyValue()
                    {
                        Key = setting.key,
                        Value = setting.value
                    }).ToList();
                }
                    client.SetConfiguration(adapter.ID, keyValue,
                                        groupId, unixTimestamp, System.Convert.ToBase64String(EncryptUtils.AesEncrypt(adapter.SharedSecret, EncryptUtils.HashSHA1(signature))));

                if (adapterStatus != null)
                    log.DebugFormat("Cdvr Engine Adapter Send Configuration Result = {0}", adapterStatus);
                else
                    log.Debug("Adapter status is null");

                if (adapterStatus != null && adapterStatus.Code == STATUS_OK)
                {
                    result = true;
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Failed ex = {0}, engine id = {1}", ex, adapter != null ? adapter.ID : 0);
            }
            return result;
        }

        public RecordResult Record(int groupID, long startTimeSeconds, long durationSeconds, string channelId, int adapterId)
        {
            RecordResult recordResult = new RecordResult();

            CDVRAdapter adapter = CdvrEnginesCache.Instance().GetCdvrAdapter(groupID, adapterId);

            if (adapter == null)
            {
                throw new KalturaException(string.Format("Cdvr Adapter {0} doesn't exist", adapter.ID), (int)eResponseStatus.AdapterNotExists);
            }

            if (string.IsNullOrEmpty(adapter.AdapterUrl))
            {
                throw new KalturaException("Recommendation engine adapter has no URL", (int)eResponseStatus.AdapterUrlRequired);
            }

            CdvrEngineAdapter.ServiceClient client = new CdvrEngineAdapter.ServiceClient(string.Empty, adapter.AdapterUrl);            

            //set unixTimestamp
            long unixTimestamp = TVinciShared.DateUtils.DateTimeToUnixTimestamp(DateTime.UtcNow);

            //TODO: verify that signature is correct
            string signature = string.Concat(adapter.ExternalIdentifier, adapter.ID, unixTimestamp);
           
            try
            {
                log.DebugFormat("Sending request to cdvr engine adapter. groupID ID = {0}, adapterID = {1}, startTimeSeconds = {2}, durationSeconds = {3}, channelId = {4}",
                    groupID, adapter.ID, startTimeSeconds, durationSeconds, channelId);

                CdvrEngineAdapter.RecordingResponse adapterResponse = new CdvrEngineAdapter.RecordingResponse();

                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    //call Adapter get channel recommendations
                    adapterResponse = client.Record(startTimeSeconds, durationSeconds, channelId, adapter.ID, unixTimestamp,
                        System.Convert.ToBase64String(EncryptUtils.AesEncrypt(adapter.SharedSecret, EncryptUtils.HashSHA1(signature)))
                        );
                }               

                LogAdapterResponse(adapterResponse, "Record");

                if (adapterResponse != null && adapterResponse.Status != null &&
                    adapterResponse.Status.Code == STATUS_NO_CONFIGURATION_FOUND)
                {
                    #region Send Configuration if not found

                    string key = string.Format("CdvrEngine_Adapter_Locker_{0}", adapterId);

                    // Build dictionary for synchronized action
                    Dictionary<string, object> parameters = new Dictionary<string, object>()
                    {
                        {PARAMETER_ADAPTER, adapter},
                        {PARAMETER_GROUP_ID, groupID}
                    };

                    configurationSynchronizer.DoAction(key, parameters);

                    using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                    {
                        //call Adapter get recommendations - after it is configured
                        adapterResponse = client.Record(startTimeSeconds, durationSeconds, channelId, adapter.ID, unixTimestamp,
                        System.Convert.ToBase64String(EncryptUtils.AesEncrypt(adapter.SharedSecret, EncryptUtils.HashSHA1(signature)))
                        );
                    }

                    LogAdapterResponse(adapterResponse, "Record");

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
                        recordResult = new RecordResult()
                                {
                                    Links = adapterResponse.Recording.Links.Select(result =>
                                        new RecordingLink()
                                        {
                                            DeviceType = result.DeviceType,
                                            Url = result.Url
                                        }).ToList(),
                                    RecordingId = adapterResponse.Recording.RecordingId,
                                    RecordingState = adapterResponse.Recording.RecordingState,
                                    FailReason = adapterResponse.Recording.FailReason,
                                    ProviderStatusCode = adapterResponse.Recording.ProviderStatusCode,
                                    ProviderStatusMessage = adapterResponse.Recording.ProviderStatusMessage
                                };
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

        public RecordResult GetRecordingStatus(int groupID, string recordingId, int adapterId)
        {
            RecordResult recordResult = new RecordResult();

            CDVRAdapter adapter = CdvrEnginesCache.Instance().GetCdvrAdapter(groupID, adapterId);

            if (adapter == null)
            {
                throw new KalturaException(string.Format("Cdvr Adapter {0} doesn't exist", adapter.ID), (int)eResponseStatus.AdapterNotExists);
            }

            if (string.IsNullOrEmpty(adapter.AdapterUrl))
            {
                throw new KalturaException("Recommendation engine adapter has no URL", (int)eResponseStatus.AdapterUrlRequired);
            }

            CdvrEngineAdapter.ServiceClient client = new CdvrEngineAdapter.ServiceClient(string.Empty, adapter.AdapterUrl);

            //set unixTimestamp
            long unixTimestamp = TVinciShared.DateUtils.DateTimeToUnixTimestamp(DateTime.UtcNow);

            //TODO: verify that signature is correct
            string signature = string.Concat(adapter.ExternalIdentifier, adapter.ID, unixTimestamp);

            try
            {
                log.DebugFormat("Sending request to cdvr engine adapter. groupID ID = {0}, adapterID = {1}, recordingId = {2}",
                    groupID, adapter.ID, recordingId);

                var adapterResponse = new RecordingResponse();

                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    //call Adapter get channel recommendations
                    adapterResponse = client.GetRecordingStatus(recordingId,adapterId,unixTimestamp,
                        System.Convert.ToBase64String(EncryptUtils.AesEncrypt(adapter.SharedSecret, EncryptUtils.HashSHA1(signature)))
                        );
                }

                LogAdapterResponse(adapterResponse, "GetRecordingStatus");

                if (adapterResponse != null && adapterResponse.Status != null && adapterResponse.Status.Code == STATUS_NO_CONFIGURATION_FOUND)
                {
                    #region Send Configuration if not found

                    string key = string.Format("CdvrEngine_Adapter_Locker_{0}", adapterId);

                    // Build dictionary for synchronized action
                    Dictionary<string, object> parameters = new Dictionary<string, object>()
                    {
                        {PARAMETER_ADAPTER, adapter},
                        {PARAMETER_GROUP_ID, groupID}
                    };

                    configurationSynchronizer.DoAction(key, parameters);

                    using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                    {
                        //call Adapter get recommendations - after it is configured
                        adapterResponse = client.GetRecordingStatus(recordingId, adapterId, unixTimestamp,
                        System.Convert.ToBase64String(EncryptUtils.AesEncrypt(adapter.SharedSecret, EncryptUtils.HashSHA1(signature)))
                        );
                    }

                    LogAdapterResponse(adapterResponse, "GetRecordingStatus");

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
                        recordResult = new RecordResult()
                        {
                            Links = adapterResponse.Recording.Links.Select(result =>
                                new RecordingLink()
                                {
                                    DeviceType = result.DeviceType,
                                    Url = result.Url
                                }).ToList(),
                            RecordingId = adapterResponse.Recording.RecordingId,
                            RecordingState = adapterResponse.Recording.RecordingState,
                            FailReason = adapterResponse.Recording.FailReason,
                            ProviderStatusCode = adapterResponse.Recording.ProviderStatusCode,
                            ProviderStatusMessage = adapterResponse.Recording.ProviderStatusMessage 
                        };
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

        public RecordResult UpdateRecordingSchedule(int groupID, string recordingId, int adapterId, long startDateSeconds, long durationSeconds)
        {
            RecordResult recordResult = new RecordResult();

            CDVRAdapter adapter = CdvrEnginesCache.Instance().GetCdvrAdapter(groupID, adapterId);

            if (adapter == null)
            {
                throw new KalturaException(string.Format("Cdvr Adapter {0} doesn't exist", adapter.ID), (int)eResponseStatus.AdapterNotExists);
            }

            if (string.IsNullOrEmpty(adapter.AdapterUrl))
            {
                throw new KalturaException("Recommendation engine adapter has no URL", (int)eResponseStatus.AdapterUrlRequired);
            }

            CdvrEngineAdapter.ServiceClient client = new CdvrEngineAdapter.ServiceClient(string.Empty, adapter.AdapterUrl);

            //set unixTimestamp
            long unixTimestamp = TVinciShared.DateUtils.DateTimeToUnixTimestamp(DateTime.UtcNow);

            //TODO: verify that signature is correct
            string signature = string.Concat(adapter.ExternalIdentifier, adapter.ID, unixTimestamp);

            try
            {
                log.DebugFormat("Sending request to cdvr engine adapter. groupID ID = {0}, adapterID = {1}, recordingId = {2}",
                    groupID, adapter.ID, recordingId);

                var adapterResponse = new RecordingResponse();

                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    //call Adapter get channel recommendations
                    adapterResponse = client.UpdateRecordingSchedule(recordingId, adapterId, startDateSeconds, durationSeconds, unixTimestamp,
                        System.Convert.ToBase64String(EncryptUtils.AesEncrypt(adapter.SharedSecret, EncryptUtils.HashSHA1(signature)))
                        );
                }

                LogAdapterResponse(adapterResponse, "UpdateRecordingSchedule");

                if (adapterResponse != null && adapterResponse.Status != null && adapterResponse.Status.Code == STATUS_NO_CONFIGURATION_FOUND)
                {
                    #region Send Configuration if not found

                    string key = string.Format("CdvrEngine_Adapter_Locker_{0}", adapterId);

                    // Build dictionary for synchronized action
                    Dictionary<string, object> parameters = new Dictionary<string, object>()
                    {
                        {PARAMETER_ADAPTER, adapter},
                        {PARAMETER_GROUP_ID, groupID}
                    };

                    configurationSynchronizer.DoAction(key, parameters);

                    using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                    {
                        //call Adapter get recommendations - after it is configured
                        adapterResponse = client.UpdateRecordingSchedule(recordingId, adapterId, startDateSeconds, durationSeconds, unixTimestamp,
                            System.Convert.ToBase64String(EncryptUtils.AesEncrypt(adapter.SharedSecret, EncryptUtils.HashSHA1(signature)))
                            );
                    }

                    LogAdapterResponse(adapterResponse, "UpdateRecordingSchedule");

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
                        recordResult = new RecordResult()
                        {
                            Links = adapterResponse.Recording.Links.Select(result =>
                                new RecordingLink()
                                {
                                    DeviceType = result.DeviceType,
                                    Url = result.Url
                                }).ToList(),
                            RecordingId = adapterResponse.Recording.RecordingId,
                            RecordingState = adapterResponse.Recording.RecordingState,
                            FailReason = adapterResponse.Recording.FailReason,
                            ProviderStatusCode = adapterResponse.Recording.ProviderStatusCode,
                            ProviderStatusMessage = adapterResponse.Recording.ProviderStatusMessage
                        };
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

        public RecordResult CancelRecording(int groupID, string recordingId, int adapterId)
        {
            RecordResult recordResult = new RecordResult();

            CDVRAdapter adapter = CdvrEnginesCache.Instance().GetCdvrAdapter(groupID, adapterId);

            if (adapter == null)
            {
                throw new KalturaException(string.Format("Cdvr Adapter {0} doesn't exist", adapter.ID), (int)eResponseStatus.AdapterNotExists);
            }

            if (string.IsNullOrEmpty(adapter.AdapterUrl))
            {
                throw new KalturaException("Recommendation engine adapter has no URL", (int)eResponseStatus.AdapterUrlRequired);
            }

            CdvrEngineAdapter.ServiceClient client = new CdvrEngineAdapter.ServiceClient(string.Empty, adapter.AdapterUrl);

            //set unixTimestamp
            long unixTimestamp = TVinciShared.DateUtils.DateTimeToUnixTimestamp(DateTime.UtcNow);

            //TODO: verify that signature is correct
            string signature = string.Concat(adapter.ExternalIdentifier, adapter.ID, unixTimestamp);

            try
            {
                log.DebugFormat("Sending request to cdvr engine adapter. groupID ID = {0}, adapterID = {1}, recordingId = {2}",
                    groupID, adapter.ID, recordingId);

                var adapterResponse = new RecordingResponse();

                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    //call Adapter get channel recommendations
                    adapterResponse = client.CancelRecording(recordingId, adapterId, unixTimestamp,
                        System.Convert.ToBase64String(EncryptUtils.AesEncrypt(adapter.SharedSecret, EncryptUtils.HashSHA1(signature)))
                        );
                }

                LogAdapterResponse(adapterResponse, "CancelRecording");

                if (adapterResponse != null && adapterResponse.Status != null && adapterResponse.Status.Code == STATUS_NO_CONFIGURATION_FOUND)
                {
                    #region Send Configuration if not found

                    string key = string.Format("CdvrEngine_Adapter_Locker_{0}", adapterId);

                    // Build dictionary for synchronized action
                    Dictionary<string, object> parameters = new Dictionary<string, object>()
                    {
                        {PARAMETER_ADAPTER, adapter},
                        {PARAMETER_GROUP_ID, groupID}
                    };

                    configurationSynchronizer.DoAction(key, parameters);

                    using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                    {
                        //call Adapter get recommendations - after it is configured
                        adapterResponse = client.CancelRecording(recordingId, adapterId, unixTimestamp,
                         System.Convert.ToBase64String(EncryptUtils.AesEncrypt(adapter.SharedSecret, EncryptUtils.HashSHA1(signature)))
                         );
                    }

                    LogAdapterResponse(adapterResponse, "CancelRecording");

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
                        recordResult = new RecordResult()
                        {
                            Links = adapterResponse.Recording.Links.Select(result =>
                                new RecordingLink()
                                {
                                    DeviceType = result.DeviceType,
                                    Url = result.Url
                                }).ToList(),
                            RecordingId = adapterResponse.Recording.RecordingId,
                            RecordingState = adapterResponse.Recording.RecordingState,
                            FailReason = adapterResponse.Recording.FailReason,
                            ProviderStatusCode = adapterResponse.Recording.ProviderStatusCode,
                            ProviderStatusMessage = adapterResponse.Recording.ProviderStatusMessage
                        };
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


        public RecordResult DeleteRecording(int groupID, string recordingId, int adapterId)
        {
            RecordResult recordResult = new RecordResult();

            CDVRAdapter adapter = CdvrEnginesCache.Instance().GetCdvrAdapter(groupID, adapterId);

            if (adapter == null)
            {
                throw new KalturaException(string.Format("Cdvr Adapter {0} doesn't exist", adapter.ID), (int)eResponseStatus.AdapterNotExists);
            }

            if (string.IsNullOrEmpty(adapter.AdapterUrl))
            {
                throw new KalturaException("Recommendation engine adapter has no URL", (int)eResponseStatus.AdapterUrlRequired);
            }

            CdvrEngineAdapter.ServiceClient client = new CdvrEngineAdapter.ServiceClient(string.Empty, adapter.AdapterUrl);

            //set unixTimestamp
            long unixTimestamp = TVinciShared.DateUtils.DateTimeToUnixTimestamp(DateTime.UtcNow);

            //TODO: verify that signature is correct
            string signature = string.Concat(adapter.ExternalIdentifier, adapter.ID, unixTimestamp);

            try
            {
                log.DebugFormat("Sending request to cdvr engine adapter. groupID ID = {0}, adapterID = {1}, recordingId = {2}",
                    groupID, adapter.ID, recordingId);

                var adapterResponse = new RecordingResponse();

                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    //call Adapter get channel recommendations
                    adapterResponse = client.DeleteRecording(recordingId, adapterId, unixTimestamp,
                        System.Convert.ToBase64String(EncryptUtils.AesEncrypt(adapter.SharedSecret, EncryptUtils.HashSHA1(signature)))
                        );
                }

                LogAdapterResponse(adapterResponse, "DeleteRecording");

                if (adapterResponse != null && adapterResponse.Status != null && adapterResponse.Status.Code == STATUS_NO_CONFIGURATION_FOUND)
                {
                    #region Send Configuration if not found

                    string key = string.Format("CdvrEngine_Adapter_Locker_{0}", adapterId);

                    // Build dictionary for synchronized action
                    Dictionary<string, object> parameters = new Dictionary<string, object>()
                    {
                        {PARAMETER_ADAPTER, adapter},
                        {PARAMETER_GROUP_ID, groupID}
                    };

                    configurationSynchronizer.DoAction(key, parameters);

                    using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                    {
                        //call Adapter get recommendations - after it is configured
                        adapterResponse = client.DeleteRecording(recordingId, adapterId, unixTimestamp,
                        System.Convert.ToBase64String(EncryptUtils.AesEncrypt(adapter.SharedSecret, EncryptUtils.HashSHA1(signature)))
                        );
                    }

                    LogAdapterResponse(adapterResponse, "DeleteRecording");

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
                        recordResult = new RecordResult()
                        {
                            Links = adapterResponse.Recording.Links.Select(result =>
                                new RecordingLink()
                                {
                                    DeviceType = result.DeviceType,
                                    Url = result.Url
                                }).ToList(),
                            RecordingId = adapterResponse.Recording.RecordingId,
                            RecordingState = adapterResponse.Recording.RecordingState,
                            FailReason = adapterResponse.Recording.FailReason,
                            ProviderStatusCode = adapterResponse.Recording.ProviderStatusCode,
                            ProviderStatusMessage = adapterResponse.Recording.ProviderStatusMessage
                        };
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

        public RecordResult GetRecordingLinks(int groupID, string recordingId, int adapterId)
        {
            RecordResult recordResult = new RecordResult();

            CDVRAdapter adapter = CdvrEnginesCache.Instance().GetCdvrAdapter(groupID, adapterId);

            if (adapter == null)
            {
                throw new KalturaException(string.Format("Cdvr Adapter {0} doesn't exist", adapter.ID), (int)eResponseStatus.AdapterNotExists);
            }

            if (string.IsNullOrEmpty(adapter.AdapterUrl))
            {
                throw new KalturaException("Recommendation engine adapter has no URL", (int)eResponseStatus.AdapterUrlRequired);
            }

            CdvrEngineAdapter.ServiceClient client = new CdvrEngineAdapter.ServiceClient(string.Empty, adapter.AdapterUrl);

            //set unixTimestamp
            long unixTimestamp = TVinciShared.DateUtils.DateTimeToUnixTimestamp(DateTime.UtcNow);

            //TODO: verify that signature is correct
            string signature = string.Concat(adapter.ExternalIdentifier, adapter.ID, unixTimestamp);

            try
            {
                log.DebugFormat("Sending request to cdvr engine adapter. groupID ID = {0}, adapterID = {1}, recordingId = {2}",
                    groupID, adapter.ID, recordingId);

                var adapterResponse = new RecordingResponse();

                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    //call Adapter get channel recommendations
                    adapterResponse = client.GetRecordingLinks(recordingId, adapterId, unixTimestamp,
                        System.Convert.ToBase64String(EncryptUtils.AesEncrypt(adapter.SharedSecret, EncryptUtils.HashSHA1(signature)))
                        );
                }

                LogAdapterResponse(adapterResponse, "GetRecordingLinks");

                if (adapterResponse != null && adapterResponse.Status != null && adapterResponse.Status.Code == STATUS_NO_CONFIGURATION_FOUND)
                {
                    #region Send Configuration if not found

                    string key = string.Format("CdvrEngine_Adapter_Locker_{0}", adapterId);

                    // Build dictionary for synchronized action
                    Dictionary<string, object> parameters = new Dictionary<string, object>()
                    {
                        {PARAMETER_ADAPTER, adapter},
                        {PARAMETER_GROUP_ID, groupID}
                    };

                    configurationSynchronizer.DoAction(key, parameters);

                    using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                    {
                        //call Adapter get recommendations - after it is configured
                        adapterResponse = client.GetRecordingLinks(recordingId, adapterId, unixTimestamp,
                        System.Convert.ToBase64String(EncryptUtils.AesEncrypt(adapter.SharedSecret, EncryptUtils.HashSHA1(signature)))
                        );
                    }

                    LogAdapterResponse(adapterResponse, "GetRecordingLinks");

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
                        recordResult = new RecordResult()
                        {
                            Links = adapterResponse.Recording.Links.Select(result =>
                                new RecordingLink()
                                {
                                    DeviceType = result.DeviceType,
                                    Url = result.Url
                                }).ToList(),
                            RecordingId = adapterResponse.Recording.RecordingId,
                            RecordingState = adapterResponse.Recording.RecordingState,
                            FailReason = adapterResponse.Recording.FailReason,
                            ProviderStatusCode = adapterResponse.Recording.ProviderStatusCode,
                            ProviderStatusMessage = adapterResponse.Recording.ProviderStatusMessage
                        };
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
        private bool configurationSynchronizer_SynchronizedAct(Dictionary<string, object> parameters)
        {
            bool result = false;

            if (parameters != null)
            {                
                int groupId = 0;
                CDVRAdapter adapter = null;

                if (parameters.ContainsKey(PARAMETER_GROUP_ID))
                {
                    groupId = (int)parameters[PARAMETER_GROUP_ID];
                }

                if (parameters.ContainsKey(PARAMETER_ADAPTER))
                {
                    adapter = (CDVRAdapter)parameters[PARAMETER_ADAPTER];
                }

                // get the right 
                result = this.SendConfiguration(adapter, groupId);
            }

            return result;
        }

        private void LogAdapterResponse(RecordingResponse adapterResponse, string action)
        {
            string logMessage = string.Empty;

            if (adapterResponse == null)
            {
                logMessage = string.Format("Cdvr Engine Adapter {0} Result is null", action != null ? action : string.Empty);
            }
            else if (adapterResponse.Status == null)
            {
                logMessage = string.Format("Cdvr Engine Adapter {0} Result's status is null", action != null ? action : string.Empty);
            }
            else if (adapterResponse.Recording == null)
            {
                logMessage = string.Format("Cdvr Engine Adapter {0} Result Status: Message = {1}, Code = {2}",
                                 action != null ? action : string.Empty,                                                                                                                // {0}
                                 adapterResponse != null && adapterResponse.Status != null && adapterResponse.Status.Message != null ? adapterResponse.Status.Message : string.Empty,   // {1}
                                 adapterResponse != null && adapterResponse.Status != null ? adapterResponse.Status.Code : -1);                                                         // {2}
            }
            else
            {
                logMessage = string.Format("Cdvr Engine Adapter  RecordingId = {1}, RecordingState = {2}",                  
                  adapterResponse.Recording.RecordingId,
                    adapterResponse.Recording.RecordingState
                    );
            }

            log.Debug(logMessage);
        }
        
        #endregion
    }
}
