using ApiObjects;
using ApiObjects.Billing;
using ApiObjects.Response;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using CouchbaseManager;
using System.Threading;
using Synchronizer;
using Core.Api;

namespace ApiLogic
{
    /// <summary>
    /// Responsible for sending request to the differet adapters and managing their configuration
    /// </summary>
    public class AdaptersController
    {
        #region Consts

        protected const string PARAMETER_OSS_ADAPTER = "ossAdapter";
        protected const string PARAMETER_GROUP_ID = "groupId";

        #endregion

        #region Static Data Members

        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private static Dictionary<int, AdaptersController> instances;

        /// <summary>
        /// Locker for the entire class
        /// </summary>
        private static readonly object generalLocker = new object();

        private static readonly Random random;

        #endregion

        #region Normal Data Members

        private int ossAdapterId;

        private CouchbaseSynchronizer configurationSynchronizer;

        #endregion

        #region Getter

        /// <summary>
        /// Gets the singleton instance of the adapter controller which is relevant for the given oss adapter Id
        /// </summary>
        /// <param name="ossAdapterId"></param>
        /// <returns></returns>
        public static AdaptersController GetInstance(int ossAdapterId)
        {
            if (!instances.ContainsKey(ossAdapterId))
            {
                lock (generalLocker)
                {
                    if (!instances.ContainsKey(ossAdapterId))
                    {
                        instances[ossAdapterId] = new AdaptersController();
                    }
                }
            }

            return instances[ossAdapterId];
        }

        #endregion

        #region Ctors

        static AdaptersController()
        {
            instances = new Dictionary<int, AdaptersController>();
            random = new Random();
        }

        private AdaptersController()
        {
            configurationSynchronizer = new CouchbaseSynchronizer(100);
            configurationSynchronizer.SynchronizedAct += synchronizer_SynchronizedAct;
        }

        #endregion

        #region Methods

        public APILogic.OSSAdapterService.HouseholdPaymentGatewayResponse GetHouseholdPaymentGatewaySettings(HouseholdBillingRequest request)
        {
            APILogic.OSSAdapterService.HouseholdPaymentGatewayResponse adapterResponse = null;

            try
            {
                adapterResponse = ValidateRequest(request);

                // If it is not valid - stop and return
                if (adapterResponse.Status != null)
                {
                    return adapterResponse;
                }

                this.ossAdapterId = request.OSSAdapter.ID;

                APILogic.OSSAdapterService.ServiceClient adapterClient = new APILogic.OSSAdapterService.ServiceClient(string.Empty, request.OSSAdapter.AdapterUrl);

                if (!string.IsNullOrEmpty(request.OSSAdapter.AdapterUrl))
                {
                    adapterClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(request.OSSAdapter.AdapterUrl);
                }

                //set unixTimestamp
                long unixTimestamp = TVinciShared.DateUtils.DateTimeToUnixTimestamp(DateTime.UtcNow);

                //set signature
                string signature = string.Concat(request.HouseholdId, request.UserIP, unixTimestamp);

                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    //call Adapter Transact
                    adapterResponse = adapterClient.GetHouseholdPaymentGatewaySettings(request.HouseholdId.ToString(), request.UserIP, unixTimestamp,
                        System.Convert.ToBase64String(TVinciShared.EncryptUtils.AesEncrypt(request.OSSAdapter.SharedSecret, TVinciShared.EncryptUtils.HashSHA1(signature))));
                }

                LogAdapterResponse(adapterResponse, "GetHouseholdPaymentGatewaySettings");

                if (adapterResponse != null && adapterResponse.Status != null && adapterResponse.Status.Code == (int)OSSAdapterStatus.NoConfigurationFound)
                {
                    string key = string.Format("OSS_Adapter_Locker_{0}", ossAdapterId);

                    // Build dictionary for synchronized action
                    Dictionary<string, object> parameters = new Dictionary<string, object>()
                    {
                        {PARAMETER_OSS_ADAPTER, request.OSSAdapter},
                        {PARAMETER_GROUP_ID, request.GroupId}
                    };

                    configurationSynchronizer.DoAction(key, parameters);

                    using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                    {
                        //call Adapter Transact - after it is configured
                        adapterResponse = adapterClient.GetHouseholdPaymentGatewaySettings(request.HouseholdId.ToString(), request.UserIP, unixTimestamp,
                            System.Convert.ToBase64String(TVinciShared.EncryptUtils.AesEncrypt(request.OSSAdapter.SharedSecret, TVinciShared.EncryptUtils.HashSHA1(signature))));
                    }

                    LogAdapterResponse(adapterResponse, "GetHouseholdPaymentGatewaySettings");
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error in GetHouseholdPaymentGatewaySettings : error = {3}, household id = {0}, oss adapter ID = {1}, AdapterUrl = {2} ",
                    request.HouseholdId, request.OSSAdapter.ID, request.OSSAdapter.AdapterUrl, ex);
            }

            return adapterResponse;
        }

        /// <summary>
        /// The synchronized action is sending the configuration to the adapter
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        private bool synchronizer_SynchronizedAct(Dictionary<string, object> parameters)
        {
            bool result = false;

            if (parameters != null)
            {
                OSSAdapter ossAdapter = null;
                int groupId = 0;

                if (parameters.ContainsKey(PARAMETER_OSS_ADAPTER))
                {
                    ossAdapter = (OSSAdapter)parameters[PARAMETER_OSS_ADAPTER];
                }

                if (parameters.ContainsKey(PARAMETER_GROUP_ID))
                {
                    groupId = (int)parameters[PARAMETER_GROUP_ID];
                }

                result = this.SendConfiguration(ossAdapter, groupId);
            }

            return result;
        }

        /// <summary>
        /// Logs what came
        /// </summary>
        /// <param name="adapterResponse"></param>
        private static void LogAdapterResponse(APILogic.OSSAdapterService.HouseholdPaymentGatewayResponse adapterResponse, string action)
        {
            string logMessage = string.Empty;

            if (adapterResponse == null)
            {
                logMessage = string.Format("OSS adapter {0} Result is null", action);
            }
            else if (adapterResponse.Status == null)
            {
                logMessage = string.Format("OSS adapter {0} Result's status is null", action);
            }
            else if (adapterResponse.Configuration == null)
            {
                logMessage = string.Format("OSS adapter {0} Result Status: Message = {1}, Code = {2}",
                                action, adapterResponse.Status.Message, adapterResponse.Status.Code);
            }
            else
            {
                logMessage = string.Format("OSS adapter {0} Result Status: Message = {1}, Code = {2}, " +
                    "Configuration: ChargeId = {3}, PaymentGatewayId = {4}, StateCode = {5}",
                    // {0}
                    action,
                    // {1}                    
                    adapterResponse.Status.Message != null ? adapterResponse.Status.Message : string.Empty,
                    // {2}
                    adapterResponse.Status.Code,
                    // {3}                    
                    adapterResponse.Configuration.ChargeId != null ? adapterResponse.Configuration.ChargeId : string.Empty,
                    // {4}
                    adapterResponse.Configuration.PaymentGatewayId != null ? adapterResponse.Configuration.PaymentGatewayId : string.Empty,
                    // {5}
                    adapterResponse.Configuration.StateCode
                    );
            }

            log.Debug(logMessage);
        }

        private bool SendConfiguration(OSSAdapter ossAdapter, int groupId)
        {
            bool result = false;

            if (ossAdapter != null && !string.IsNullOrEmpty(ossAdapter.AdapterUrl))
            {
                APILogic.OSSAdapterService.ServiceClient client = new APILogic.OSSAdapterService.ServiceClient(string.Empty, ossAdapter.AdapterUrl);

                //set unixTimestamp
                long unixTimestamp = TVinciShared.DateUtils.DateTimeToUnixTimestamp(DateTime.UtcNow);

                //set signature
                string signature = string.Concat(ossAdapter.ID, ossAdapter.Settings != null ?
                        string.Concat(ossAdapter.Settings.Select(setting => string.Concat(setting.key, setting.value))) : string.Empty,
                        groupId, unixTimestamp);

                APILogic.OSSAdapterService.AdapterStatus adapterResponse = null;

                try
                {
                    using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                    {
                        //call Adapter SetConfiguration
                        adapterResponse =
                            client.SetConfiguration(this.ossAdapterId,
                            ossAdapter.Settings != null ? ossAdapter.Settings.Select(setting => new APILogic.OSSAdapterService.KeyValue()
                            {
                                Key = setting.key,
                                Value = setting.value
                            }).ToArray() : null,
                            groupId,
                            unixTimestamp,
                            System.Convert.ToBase64String(TVinciShared.EncryptUtils.AesEncrypt(ossAdapter.SharedSecret, TVinciShared.EncryptUtils.HashSHA1(signature))));
                    }

                    log.DebugFormat("OSS adapter Result AdapterStatus = {0}", adapterResponse);

                    if (adapterResponse == null && adapterResponse.Code == (int)OSSAdapterStatus.OK)
                    {
                        result = true;
                    }
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("Failed ex = {0}, oass adapter id = {1}", ex, ossAdapter.ID);
                }
            }

            return result;
        }

        private APILogic.OSSAdapterService.HouseholdPaymentGatewayResponse ValidateRequest(HouseholdBillingRequest request)
        {
            APILogic.OSSAdapterService.HouseholdPaymentGatewayResponse adapterResponse = new APILogic.OSSAdapterService.HouseholdPaymentGatewayResponse()
            {
                Status = null,
                Configuration = null
            };

            if (request.HouseholdId <= 0)
            {
                adapterResponse.Status = new APILogic.OSSAdapterService.AdapterStatus() { Code = (int)OSSAdapterStatus.Error, Message = "No household identifier sent" };
            }

            return adapterResponse;
        }

        public APILogic.OSSAdapterService.EntitlementsResponse GetEntitlements(int groupId, OSSAdapter ossAdapter, string userId)
        {
            APILogic.OSSAdapterService.EntitlementsResponse adapterResponse = null;

            try
            {
                this.ossAdapterId = ossAdapter.ID;

                APILogic.OSSAdapterService.ServiceClient adapterClient = new APILogic.OSSAdapterService.ServiceClient(string.Empty, ossAdapter.AdapterUrl);

                if (!string.IsNullOrEmpty(ossAdapter.AdapterUrl))
                {
                    adapterClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(ossAdapter.AdapterUrl);
                }

                //set unixTimestamp
                long unixTimestamp = TVinciShared.DateUtils.DateTimeToUnixTimestamp(DateTime.UtcNow);

                //set signature
                string signature = string.Concat(userId, unixTimestamp);

                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    //call adapter
                    adapterResponse = adapterClient.GetEntitlements(userId, unixTimestamp,
                        System.Convert.ToBase64String(TVinciShared.EncryptUtils.AesEncrypt(ossAdapter.SharedSecret, TVinciShared.EncryptUtils.HashSHA1(signature))));
                }

                if (adapterResponse != null && adapterResponse.Status != null && adapterResponse.Status.Code == (int)OSSAdapterStatus.NoConfigurationFound)
                {
                    string key = string.Format("OSS_Adapter_Locker_{0}", ossAdapterId);

                    // Build dictionary for synchronized action
                    Dictionary<string, object> parameters = new Dictionary<string, object>()
                    {
                        {PARAMETER_OSS_ADAPTER, ossAdapter},
                        {PARAMETER_GROUP_ID, groupId}
                    };

                    configurationSynchronizer.DoAction(key, parameters);

                    using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                    {
                        //call Adapter - after it is configured
                        adapterResponse = adapterClient.GetEntitlements(userId, unixTimestamp,
                            System.Convert.ToBase64String(TVinciShared.EncryptUtils.AesEncrypt(ossAdapter.SharedSecret, TVinciShared.EncryptUtils.HashSHA1(signature))));
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error in GetEntitlements : error = {3}, user id = {0}, oss adapter ID = {1}, AdapterUrl = {2} ",
                    userId, ossAdapter.ID, ossAdapter.AdapterUrl, ex);
            }

            return adapterResponse;
        }

        
        #endregion
    }
}
