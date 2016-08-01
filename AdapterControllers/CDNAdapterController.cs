using AdapterControllers.CdnAdapter;
using ApiObjects;
using ApiObjects.CDNAdapter;
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
using System.Web;
using TVinciShared;

namespace AdapterControllers
{
    public class CDNAdapterController
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private static readonly object generalLocker = new object();

        private const int STATUS_OK = 0;
        private const int STATUS_NO_CONFIGURATION_FOUND = 3;

        private const string PARAMETER_GROUP_ID = "group_id";
        private const string PARAMETER_ADAPTER = "adapter";

        private const string LOCKER_STRING_FORMAT = "CDN_Adapter_Locker_{0}";

        private static CDNAdapterController instance;

        // Gets the singleton instance of the adapter controller
        public static CDNAdapterController GetInstance()
        {
            if (instance == null)
            {
                lock (generalLocker)
                {
                    if (instance == null)
                    {
                        instance = new CDNAdapterController();
                    }
                }
            }

            return instance;
        }

        private CouchbaseSynchronizer configurationSynchronizer;

        private CDNAdapterController()
        {
            configurationSynchronizer = new CouchbaseSynchronizer(100);
            configurationSynchronizer.SynchronizedAct += configurationSynchronizer_SynchronizedAct;
        }

        public bool SendConfiguration(CDNAdapter adapter, int groupId)
        {
            bool result = false;

            if (adapter != null && !string.IsNullOrEmpty(adapter.AdapterUrl))
            {
                RecommendationEngineAdapter.ServiceClient client = new RecommendationEngineAdapter.ServiceClient(string.Empty, adapter.AdapterUrl);

                //set unixTimestamp
                long unixTimestamp = TVinciShared.DateUtils.DateTimeToUnixTimestamp(DateTime.UtcNow);

                //set signature
                string signature = string.Empty;

                try
                {
                    RecommendationEngineAdapter.AdapterStatus adapterResponse =
                        client.SetConfiguration(adapter.ID,
                        adapter.Settings != null ? adapter.Settings.Select(setting => new RecommendationEngineAdapter.KeyValue()
                        {
                            Key = setting.key,
                            Value = setting.value
                        }).ToArray() : null,
                        groupId,
                        unixTimestamp,
                        System.Convert.ToBase64String(EncryptUtils.AesEncrypt(adapter.SharedSecret, EncryptUtils.HashSHA1(signature))));

                    if (adapterResponse != null)
                        log.DebugFormat("CDN Adapter Send Configuration Result = {0}", adapterResponse);
                    else
                        log.Debug("Adapter response is null");

                    if (adapterResponse != null && adapterResponse.Code == STATUS_OK)
                    {
                        result = true;
                    }
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("Failed ex = {0}, adapter id = {1}", ex, adapter != null ? adapter.ID : 0);
                }
            }

            return result;
        }

        private bool configurationSynchronizer_SynchronizedAct(Dictionary<string, object> parameters)
        {
            bool result = false;

            if (parameters != null)
            {
                CDNAdapter adapter = null;
                int groupId = 0;

                if (parameters.ContainsKey(PARAMETER_ADAPTER))
                {
                    adapter = (CDNAdapter)parameters[PARAMETER_ADAPTER];
                }

                if (parameters.ContainsKey(PARAMETER_GROUP_ID))
                {
                    groupId = (int)parameters[PARAMETER_GROUP_ID];
                }

                result = this.SendConfiguration(adapter, groupId);
            }

            return result;
        }

        public LinkResult GetVodLink(int groupId, int adapterId, string userId, string url, string fileType, int assetId, int contentId, string ip)
        {
            LinkResult linkResult = null;

            CDNAdapter adapter = CdnAdapterCache.Instance().GetCdnAdapter(groupId, adapterId);

            if (adapter == null)
            {
                throw new KalturaException(string.Format("CDN adapter {0} doesn't exist", adapterId), (int)eResponseStatus.AdapterNotExists);
            }

            if (string.IsNullOrEmpty(adapter.AdapterUrl))
            {
                throw new KalturaException("CDN adapter has no URL", (int)eResponseStatus.AdapterUrlRequired);
            }

            CdnAdapter.ServiceClient adapterClient = new CdnAdapter.ServiceClient();
            adapterClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(adapter.AdapterUrl);

            //set unixTimestamp
            long unixTimestamp = TVinciShared.DateUtils.DateTimeToUnixTimestamp(DateTime.UtcNow);

            string signature =
                string.Concat(adapterId, userId, url, fileType, assetId, contentId, ip, unixTimestamp);

            try
            {
                LinkResponse adapterResponse = CallGetVodLink(adapterClient, adapter.SharedSecret, adapterId, userId, url, fileType, assetId, contentId, ip, unixTimestamp, signature);

                if (adapterResponse != null && adapterResponse.Status != null &&
                    adapterResponse.Status.Code == STATUS_NO_CONFIGURATION_FOUND)
                {
                    // Send Configuration if not found

                    string key = string.Format(LOCKER_STRING_FORMAT, adapter.ID);

                    // Build dictionary for synchronized action
                    Dictionary<string, object> parameters = new Dictionary<string, object>()
                    {
                        {PARAMETER_ADAPTER, adapter},
                        {PARAMETER_GROUP_ID, groupId}
                    };

                    configurationSynchronizer.DoAction(key, parameters);

                    // call adapter again after setting the configuration
                    adapterResponse = CallGetVodLink(adapterClient, adapter.SharedSecret, adapterId, userId, url, fileType, assetId, contentId, ip, unixTimestamp, signature);

                }

                linkResult = ParseLinkResponse(adapterResponse);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error in GetVodLink: adapterId = {0}",
                    adapterId, ex);
                throw new KalturaException("Adapter failed completing request", (int)eResponseStatus.AdapterAppFailure);
            }

            return linkResult;
        }

        public LinkResult GetRecordingLink(int groupId, int adapterId, string userId, string url, string fileType, string recordingId, string ip)
        {
            LinkResult linkResult = null;

            CDNAdapter adapter = CdnAdapterCache.Instance().GetCdnAdapter(groupId, adapterId);

            if (adapter == null)
            {
                throw new KalturaException(string.Format("CDN adapter {0} doesn't exist", adapterId), (int)eResponseStatus.AdapterNotExists);
            }

            if (string.IsNullOrEmpty(adapter.AdapterUrl))
            {
                throw new KalturaException("CDN adapter has no URL", (int)eResponseStatus.AdapterUrlRequired);
            }

            CdnAdapter.ServiceClient adapterClient = new CdnAdapter.ServiceClient();
            adapterClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(adapter.AdapterUrl);

            //set unixTimestamp
            long unixTimestamp = TVinciShared.DateUtils.DateTimeToUnixTimestamp(DateTime.UtcNow);

            string signature = string.Concat(adapterId, userId, url, fileType, recordingId, ip, unixTimestamp);

            try
            {
                LinkResponse adapterResponse = CallGetRecordingLink(adapterClient, adapter.SharedSecret, adapterId, userId, url, fileType, recordingId, ip, unixTimestamp, signature);

                if (adapterResponse != null && adapterResponse.Status != null &&
                    adapterResponse.Status.Code == STATUS_NO_CONFIGURATION_FOUND)
                {
                    // Send Configuration if not found

                    string key = string.Format(LOCKER_STRING_FORMAT, adapter.ID);

                    // Build dictionary for synchronized action
                    Dictionary<string, object> parameters = new Dictionary<string, object>()
                    {
                        {PARAMETER_ADAPTER, adapter},
                        {PARAMETER_GROUP_ID, groupId}
                    };

                    configurationSynchronizer.DoAction(key, parameters);

                    // call adapter again after setting the configuration
                    adapterResponse = CallGetRecordingLink(adapterClient, adapter.SharedSecret, adapterId, userId, url, fileType, recordingId, ip, unixTimestamp, signature);
                }

                linkResult = ParseLinkResponse(adapterResponse);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error in GetRecordingLink: adapterId = {0}",
                    adapterId, ex);
                throw new KalturaException("Adapter failed completing request", (int)eResponseStatus.AdapterAppFailure);
            }

            return linkResult;
        }

        public LinkResult GetEpgLink(int groupId, int adapterId, string userId, string url, string fileType, int programId, int assetId, int contentId, long startTimeSeconds, 
            int actionType, string ip)
        {
            LinkResult linkResult = null;

            CDNAdapter adapter = CdnAdapterCache.Instance().GetCdnAdapter(groupId, adapterId);

            if (adapter == null)
            {
                throw new KalturaException(string.Format("CDN adapter {0} doesn't exist", adapterId), (int)eResponseStatus.AdapterNotExists);
            }

            if (string.IsNullOrEmpty(adapter.AdapterUrl))
            {
                throw new KalturaException("CDN adapter has no URL", (int)eResponseStatus.AdapterUrlRequired);
            }

            CdnAdapter.ServiceClient adapterClient = new CdnAdapter.ServiceClient();
            adapterClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(adapter.AdapterUrl);

            //set unixTimestamp
            long unixTimestamp = TVinciShared.DateUtils.DateTimeToUnixTimestamp(DateTime.UtcNow);

            string signature =
                string.Concat(adapterId, userId, url, fileType, programId, assetId, contentId, ip, unixTimestamp);

            try
            {
                LinkResponse adapterResponse = CallGetEpgLink(adapterClient, adapter.SharedSecret, adapterId, userId, url, fileType, programId, assetId, contentId, startTimeSeconds, 
                    actionType, ip, unixTimestamp, signature);

                if (adapterResponse != null && adapterResponse.Status != null &&
                    adapterResponse.Status.Code == STATUS_NO_CONFIGURATION_FOUND)
                {
                    // Send Configuration if not found

                    string key = string.Format(LOCKER_STRING_FORMAT, adapter.ID);

                    // Build dictionary for synchronized action
                    Dictionary<string, object> parameters = new Dictionary<string, object>()
                    {
                        {PARAMETER_ADAPTER, adapter},
                        {PARAMETER_GROUP_ID, groupId}
                    };

                    configurationSynchronizer.DoAction(key, parameters);

                    // call adapter again after setting the configuration
                    adapterResponse = CallGetEpgLink(adapterClient, adapter.SharedSecret, adapterId, userId, url, fileType, programId, assetId, contentId, startTimeSeconds, actionType, 
                    ip, unixTimestamp, signature);
                }

                linkResult = ParseLinkResponse(adapterResponse);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error in GetRecordingLink: adapterId = {0}",
                    adapterId, ex);
                throw new KalturaException("Adapter failed completing request", (int)eResponseStatus.AdapterAppFailure);
            }

            return linkResult;
        }

        private LinkResponse CallGetEpgLink(ServiceClient adapterClient, string secret, int adapterId, string userId, string url, string fileType, int programId, int assetId, int contentId,
            long startTimeSeconds, int actionType, string ip, long unixTimestamp, string signature)
        {
            LinkResponse adapterResponse = null;

            string inputParameters = string.Format("adapterId = {0}, userId = {1}, url = {2}, fileType = {3}, programId = {4}, assetId = {5}, contentId = {6}, startTimeSeconds = {7}, actionType = {8}, ip = {9}",
                adapterId, userId, url, fileType, programId, assetId, contentId, startTimeSeconds, actionType, ip);
            log.DebugFormat("Sending request to CDN adapter. {0}", inputParameters);

            using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
            {
                //call adapter
                adapterResponse = adapterClient.GetEpgLink(adapterId, userId, url, fileType, programId, assetId, contentId, startTimeSeconds, actionType, ip, unixTimestamp,
                    System.Convert.ToBase64String(EncryptUtils.AesEncrypt(secret, EncryptUtils.HashSHA1(signature))));
            }

            LogAdapterResponse(adapterResponse, "GetEpgLink", adapterId, inputParameters);

            return adapterResponse;
        }

        private static LinkResult ParseLinkResponse(LinkResponse adapterResponse)
        {
            LinkResult linkResult = null; 

            if (adapterResponse != null && adapterResponse.Status != null)
            {
                // If something went wrong in the adapter, throw relevant exception
                if (adapterResponse.Status.Code != (int)eResponseStatus.OK)
                {
                    throw new KalturaException("Adapter failed completing request", (int)eResponseStatus.AdapterAppFailure);
                }
                else if (adapterResponse.Link != null)
                {
                    linkResult = new LinkResult()
                    {
                        ProviderStatusCode = adapterResponse.Link.ProviderStatusCode,
                        ProviderStatusMessage = adapterResponse.Link.ProviderStatusMessage,
                        Url = adapterResponse.Link.Url,
                    };
                }
            }
            return linkResult;
        }

        private LinkResponse CallGetRecordingLink(CdnAdapter.ServiceClient adapterClient, string secret,
            int adapterId, string userId, string url, string fileType, string recordingId, string ip, long unixTimestamp, string signature)
        {
            LinkResponse adapterResponse = null;

            string inputParameters = string.Format("adapterId = {0}, userId = {1}, url = {2}, fileType = {3}, recordingId = {4}, ip = {5}", 
                adapterId, userId, url, fileType, recordingId, ip);
            log.DebugFormat("Sending request to CDN adapter. {0}", inputParameters);

            using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
            {
                //call adapter
                adapterResponse = adapterClient.GetRecordingLink(adapterId, userId, url, fileType, recordingId, ip, unixTimestamp,
                    System.Convert.ToBase64String(EncryptUtils.AesEncrypt(secret, EncryptUtils.HashSHA1(signature))));
            }

            LogAdapterResponse(adapterResponse, "GetRecordingLink", adapterId, inputParameters);

            return adapterResponse;
        }

        private LinkResponse CallGetVodLink(CdnAdapter.ServiceClient adapterClient, string secret,
            int adapterId, string userId, string url, string fileType, int assetId, int contentId, string ip, long unixTimestamp, string signature)
        {
            LinkResponse adapterResponse = null;

            string inputParameters = string.Format("adapterId = {0}, userId = {1}, url = {2}, fileType = {3}, assetId = {4}, contentId = {5}, ip = {6}",
                adapterId, userId, url, fileType, assetId, contentId, ip);
            log.DebugFormat("Sending request to CDN adapter. {0}", inputParameters);

            using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
            {
                //call adapter
                adapterResponse = adapterClient.GetVodLink(adapterId, userId, url, fileType, assetId, contentId, ip, unixTimestamp,
                    System.Convert.ToBase64String(EncryptUtils.AesEncrypt(secret, EncryptUtils.HashSHA1(signature))));
            }

            LogAdapterResponse(adapterResponse, "GetVodLink", adapterId, inputParameters);

            return adapterResponse;
        }

        private void LogAdapterResponse(LinkResponse adapterResponse, string action, int adapterId, string inputParameters)
        {
            string logMessage = string.Empty;

            if (adapterResponse == null)
            {
                logMessage = string.Format("CDN Adapter {0} result is null", action != null ? action : string.Empty);
            }
            else if (adapterResponse.Status == null)
            {
                logMessage = string.Format("CDN Adapter {0} result's status is null", action != null ? action : string.Empty);
            }
            else
            {
                if (adapterResponse.Link == null)
                {
                    logMessage = string.Format("CDN Adapter {0} result status: message = {1}, code = {2}. Link is null",
                                     action != null ? action : string.Empty,                                                                                                                // {0}
                                     adapterResponse != null && adapterResponse.Status != null && adapterResponse.Status.Message != null ? adapterResponse.Status.Message : string.Empty,   // {1}
                                     adapterResponse != null && adapterResponse.Status != null ? adapterResponse.Status.Code : -1);                                                         // {2}
                }
                else
                {
                    logMessage = string.Format("CDN Adapter {0} result status: message = {1}, code = {2}. Link: Url = {3}",
                        // {0}
                        action != null ? action : string.Empty,
                        // {1}
                        adapterResponse != null && adapterResponse.Status != null && adapterResponse.Status.Message != null ? adapterResponse.Status.Message : string.Empty,
                        // {2}
                        adapterResponse != null && adapterResponse.Status != null ? adapterResponse.Status.Code : -1,
                        // {3}
                        adapterResponse.Link.Url);
                       

                    // if Status code is not 0
                    if ((adapterResponse.Status.Code != 0))
                    {
                        ReportCDVRProviderFailure(adapterId, action, inputParameters,
                            adapterResponse.Status.Code, adapterResponse.Link.ProviderStatusCode, adapterResponse.Link.ProviderStatusMessage);
                    }
                }
            }

            log.Debug(logMessage);
        }

        private static void ReportCDVRProviderFailure(int adapterId, string action, string inputParameters, int errorCode, string providerCode,
           string providerMessage)
        {
            var previousTopic = HttpContext.Current.Items[Constants.TOPIC];
            HttpContext.Current.Items[Constants.TOPIC] = "CDN provider";

            log.ErrorFormat("Adapter was accessed successfully, but returned an error. " +
                "Adapter identifier: {0}, Adapter Api: {1}. Input parameters: {2}. Error code: {3}, Provider Code: {4}, Provider Message: {5}",
                adapterId,
                action,
                inputParameters,
                errorCode,
                providerCode,
                providerMessage);

            HttpContext.Current.Items[Constants.TOPIC] = previousTopic;
        }
        
    }
}