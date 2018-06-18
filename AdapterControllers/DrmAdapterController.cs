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

namespace AdapterControllers
{
    public class DrmAdapterController
    {
        #region Consts
        private const int STATUS_OK = 0;
        private const int STATUS_NO_CONFIGURATION_FOUND = 3;

        private const string PARAMETER_GROUP_ID = "group_id";
        private const string PARAMETER_ADAPTER = "adapter";
        private const string LOCKER_STRING_FORMAT = "DRM_Adapter_Locker_{0}";

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

        private static DrmAdapterController instance;

        /// <summary>
        /// Gets the singleton instance of the adapter controller
        /// </summary>     
        /// <returns></returns>
        public static DrmAdapterController GetInstance()
        {
            if (instance == null)
            {
                lock (generalLocker)
                {
                    if (instance == null)
                    {
                        instance = new DrmAdapterController();
                    }
                }
            }

            return instance;
        }

        #endregion

        #region Ctor

        private DrmAdapterController()
        {
            configurationSynchronizer = new CouchbaseSynchronizer(100);
            configurationSynchronizer.SynchronizedAct += configurationSynchronizer_SynchronizedAct;
        }

        #endregion

        #region Public Method

        public bool SetConfiguration(DrmAdapter adapter, int partnerId)
        {
            bool result = false;
            try
            {
                string drmAdapterUrl = adapter.AdapterUrl;
                AdapterControllers.drmAdapter.ServiceClient client = new AdapterControllers.drmAdapter.ServiceClient();
                client.Endpoint.Address = new System.ServiceModel.EndpointAddress(drmAdapterUrl);

                //set unixTimestamp
                long timeStamp = TVinciShared.DateUtils.DateTimeToUnixTimestamp(DateTime.UtcNow);
                //set signature
                string signature = string.Concat(adapter.Settings, partnerId, timeStamp);

                //call Adapter 
                AdapterControllers.drmAdapter.AdapterStatus adapterStatus = null;

                adapterStatus = client.SetConfiguration(adapter.ID, adapter.Settings, partnerId, timeStamp, Utils.GetSignature(adapter.SharedSecret, signature));

                if (adapterStatus != null)
                    log.DebugFormat("DRM Adapter Send Configuration Result = {0}", adapterStatus);
                else
                    log.Debug("Adapter status is null");

                if (adapterStatus != null && adapterStatus.Code == STATUS_OK)
                {
                    result = true;
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Failed ex = {0}, adapter id = {1}", ex, adapter != null ? adapter.ID : 0);
            }
            return result;
        }

        public string GetDeviceLicenseData(int groupId, int adapterId, string userId, string udid, string deviceFamily, int deviceBrandId, string ip)
        {
            string licenseData = null;

            DrmAdapter adapter = DrmAdapterCache.Instance.GetDrmAdapter(groupId, adapterId);

            if (adapter == null)
            {
                throw new KalturaException(string.Format("DRM adapter {0} doesn't exist", adapterId), (int)eResponseStatus.AdapterNotExists);
            }

            if (string.IsNullOrEmpty(adapter.AdapterUrl))
            {
                throw new KalturaException("DRM adapter has no URL", (int)eResponseStatus.AdapterUrlRequired);
            }

            drmAdapter.ServiceClient adapterClient = new drmAdapter.ServiceClient();
            adapterClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(adapter.AdapterUrl);

            //set unixTimestamp
            long unixTimestamp = TVinciShared.DateUtils.DateTimeToUnixTimestamp(DateTime.UtcNow);

            string signature = string.Concat(adapterId, groupId, userId, udid, deviceFamily, deviceBrandId, ip, unixTimestamp);

            try
            {
                string inputParameters = string.Format("adapterId = {0}, groupId = {1}, userId = {2}, udid = {3}, deviceFamily = {4}, deviceBrandId = {5}, ip = {6}",
                adapterId, groupId, userId, deviceFamily, deviceBrandId, udid, ip);
                log.DebugFormat("Sending request to DRM adapter, GetDeviceLicenseData. {0}", inputParameters);

                drmAdapter.DrmAdapterResponse adapterResponse = CallGetDeviceLicenseData(adapterId, groupId, userId, udid, deviceFamily, deviceBrandId, ip, adapter, adapterClient,
                    unixTimestamp, signature);

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
                    adapterResponse = CallGetDeviceLicenseData(adapterId, groupId, userId, udid, deviceFamily, deviceBrandId, ip, adapter, adapterClient, unixTimestamp, signature);

                }

                licenseData = ParseDrmAdapterResponse(adapterResponse);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error in GetDeviceLicenseData: adapterId = {0}",
                    adapterId, ex);
                throw new KalturaException("Adapter failed completing request", (int)eResponseStatus.AdapterAppFailure);
            }

            return licenseData;
        }

        public string GetAssetLicenseData(int groupId, int adapterId, string userId, string assetId, eAssetTypes assetType, long fileId, string externalFileId, string ip,
            string udid, int deviceBrandId)
        {
            string licenseData = null;

            DrmAdapter adapter = DrmAdapterCache.Instance.GetDrmAdapter(groupId, adapterId);

            if (adapter == null)
            {
                throw new KalturaException(string.Format("DRM adapter {0} doesn't exist", adapterId), (int)eResponseStatus.AdapterNotExists);
            }

            if (string.IsNullOrEmpty(adapter.AdapterUrl))
            {
                throw new KalturaException("DRM adapter has no URL", (int)eResponseStatus.AdapterUrlRequired);
            }

            drmAdapter.ServiceClient adapterClient = new drmAdapter.ServiceClient();
            adapterClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(adapter.AdapterUrl);

            //set unixTimestamp
            long unixTimestamp = TVinciShared.DateUtils.DateTimeToUnixTimestamp(DateTime.UtcNow);

            string signature = string.Concat(adapterId, groupId, userId, assetId, assetType, fileId, externalFileId, ip, udid, unixTimestamp);

            try
            {
                string inputParameters = string.Format("adapterId = {0}, groupId = {1}, userId = {2}, assetId = {3}, assetType = {4}, fileId = {5}, externalFileId = {6}, ip = {7}, udid = {8}, deviceBrandId = {9}",
                adapterId, groupId, userId, assetId, assetType, fileId, externalFileId, ip, udid, deviceBrandId);
                log.DebugFormat("Sending request to DRM adapter. {0}", inputParameters);

                drmAdapter.DrmAdapterResponse adapterResponse = CallGetAssetLicenseData(adapterId, groupId, userId, assetId, assetType, fileId, externalFileId, ip, udid, deviceBrandId, adapter, adapterClient, unixTimestamp, signature);

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
                    adapterResponse = CallGetAssetLicenseData(adapterId, groupId, userId, assetId, assetType, fileId, externalFileId, ip, udid, deviceBrandId, adapter, adapterClient, 
                        unixTimestamp, signature);

                }

                licenseData = ParseDrmAdapterResponse(adapterResponse);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error in GetAssetLicenseData: adapterId = {0}",
                    adapterId, ex);
                throw new KalturaException("Adapter failed completing request", (int)eResponseStatus.AdapterAppFailure);
            }

            return licenseData;
        }

        #endregion

        #region Private Method

        private bool configurationSynchronizer_SynchronizedAct(Dictionary<string, object> parameters)
        {
            bool result = false;

            if (parameters != null)
            {
                int partnerId = 0;
                DrmAdapter adapter = null;

                if (parameters.ContainsKey(PARAMETER_GROUP_ID))
                {
                    partnerId = (int)parameters[PARAMETER_GROUP_ID];
                }

                if (parameters.ContainsKey(PARAMETER_ADAPTER))
                {
                    adapter = (DrmAdapter)parameters[PARAMETER_ADAPTER];
                }

                // get the right 
                result = this.SetConfiguration(adapter, partnerId);
            }

            return result;
        }

        private static string ParseDrmAdapterResponse(drmAdapter.DrmAdapterResponse adapterResponse)
        {
            if (adapterResponse != null && adapterResponse.Status != null)
            {
                // If something went wrong in the adapter, throw relevant exception
                if (adapterResponse.Status.Code != (int)eResponseStatus.OK)
                {
                    throw new KalturaException("Adapter failed completing request", (int)eResponseStatus.AdapterAppFailure);
                }
            }

            return adapterResponse.Data;
        }

        private static drmAdapter.DrmAdapterResponse CallGetAssetLicenseData(int adapterId, int partnerId, string userId, string assetId, eAssetTypes assetType, long fileId, string externalFileId,
            string ip, string udid, int deviceBrandId, DrmAdapter adapter, drmAdapter.ServiceClient adapterClient, long unixTimestamp, string signature)
        {
            drmAdapter.DrmAdapterResponse adapterResponse;

            using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
            {
                //call adapter
                adapterResponse = adapterClient.GetAssetLicenseData(adapterId, partnerId, userId, assetId, (drmAdapter.AssetType)assetType, fileId,
                    externalFileId, ip, udid, deviceBrandId, unixTimestamp, System.Convert.ToBase64String(EncryptUtils.AesEncrypt(adapter.SharedSecret, EncryptUtils.HashSHA1(signature))));
            }

            if (adapterResponse != null)
            {
                log.DebugFormat("DRM adapter response for GetLicenseUrl: status.code = {0}, licenseData = {1}, providerResponse = {2}",
                    adapterResponse.Status != null ? adapterResponse.Status.Code.ToString() : "null", adapterResponse.Data, adapterResponse.ProviderResponse);
            }
            else
            {
                log.Error("DRM adapter response for GetLicenseUrl is null");
            }

            return adapterResponse;
        }

        private static drmAdapter.DrmAdapterResponse CallGetDeviceLicenseData(int adapterId, int partnerId, string userId, string udid, string deviceFamily, int deviceBrandId, string ip, DrmAdapter adapter, drmAdapter.ServiceClient adapterClient, long unixTimestamp, string signature)
        {
            drmAdapter.DrmAdapterResponse adapterResponse;

            using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
            {
                //call adapter
                adapterResponse = adapterClient.GetDeviceLicenseData(adapterId, partnerId, userId, udid, deviceFamily, deviceBrandId, ip, unixTimestamp,
                    System.Convert.ToBase64String(EncryptUtils.AesEncrypt(adapter.SharedSecret, EncryptUtils.HashSHA1(signature))));
            }

            if (adapterResponse != null)
            {
                log.DebugFormat("DRM adapter response for GetLicenseUrl: status.code = {0}, licenseData = {1}, providerResponse = {2}",
                    adapterResponse.Status != null ? adapterResponse.Status.Code.ToString() : "null", adapterResponse.Data, adapterResponse.ProviderResponse);
            }
            else
            {
                log.Error("DRM adapter response for GetLicenseUrl is null");
            }

            return adapterResponse;
        }

        #endregion
    }
}
