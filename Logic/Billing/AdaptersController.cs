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
using Enyim.Caching.Memcached;
using System.Threading;
using Synchronizer;

namespace Core.Billing
{
    /// <summary>
    /// Responsible for sending request to the different adapters and managing their configuration
    /// </summary>
    public class AdaptersController
    {
        #region Consts

        protected const string PARAMETER_PAYMENT_GATEWAY = "paymentGateway";
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

        private int paymentGatewayId;

        private CouchbaseSynchronizer configurationSynchronizer;

        #endregion

        #region Getter

        /// <summary>
        /// Gets the singleton instance of the adapter controller which is relevant for the given payment gateway ID
        /// </summary>
        /// <param name="paymentGatewayId"></param>
        /// <returns></returns>
        public static AdaptersController GetInstance(int paymentGatewayId)
        {
            if (!instances.ContainsKey(paymentGatewayId))
            {
                lock (generalLocker)
                {
                    if (!instances.ContainsKey(paymentGatewayId))
                    {
                        instances[paymentGatewayId] = new AdaptersController();
                    }
                }
            }

            return instances[paymentGatewayId];
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

        public APILogic.PaymentGWAdapter.TransactionResponse Transact(TransactionRequest request)
        {
            APILogic.PaymentGWAdapter.TransactionResponse adapterResponse = ValidateRequest(request);

            // If it is not valid - stop and return
            if (adapterResponse.Status != null)
            {
                return adapterResponse;
            }

            this.paymentGatewayId = request.paymentGateway.ID;

            APILogic.PaymentGWAdapter.ServiceClient adapterClient = new APILogic.PaymentGWAdapter.ServiceClient(string.Empty, request.paymentGateway.AdapterUrl);

            if (!string.IsNullOrEmpty(request.paymentGateway.AdapterUrl))
            {
                adapterClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(request.paymentGateway.AdapterUrl);
            }

            //set unixTimestamp
            long unixTimestamp = TVinciShared.DateUtils.DateTimeToUnixTimestamp(DateTime.UtcNow);

            //set signature
            string signature = string.Concat(this.paymentGatewayId, request.siteGuid, request.chargeId, request.price, request.currency,
                request.productId, request.productType, request.contentId, request.userIP, unixTimestamp, request.paymentMethodExternalId);

            try
            {
                //call Adapter Transact
                adapterResponse = adapterClient.Transact(this.paymentGatewayId,
                    request.siteGuid, request.chargeId,
                    request.price, request.currency, request.productId.ToString(),
                    ConvertTransactionType(request.productType),
                    request.contentId.ToString(), request.userIP,
                    request.paymentMethodExternalId,
                    request.adapterData,
                    unixTimestamp,
                    System.Convert.ToBase64String(
                        TVinciShared.EncryptUtils.AesEncrypt(request.paymentGateway.SharedSecret, TVinciShared.EncryptUtils.HashSHA1(signature))));

                LogAdapterResponse(adapterResponse, "Transact");

                if (adapterResponse != null && adapterResponse.Status != null &&
                    adapterResponse.Status.Code == (int)PaymentGatewayAdapterStatus.NoConfigurationFound)
                {
                    string key = string.Format("PaymentGateway_Adapter_Locker_{0}", paymentGatewayId);

                    // Build dictionary for synchronized action
                    Dictionary<string, object> parameters = new Dictionary<string, object>()
                    {
                        {PARAMETER_PAYMENT_GATEWAY, request.paymentGateway},
                        {PARAMETER_GROUP_ID, request.groupId}
                    };

                    configurationSynchronizer.DoAction(key, parameters);

                    //call Adapter Transact - after it is configured
                    adapterResponse = adapterClient.Transact(this.paymentGatewayId,
                        request.siteGuid, request.chargeId,
                        request.price, request.currency, request.productId.ToString(),
                        ConvertTransactionType(request.productType),
                        request.contentId.ToString(), request.userIP,
                        request.paymentMethodExternalId,
                        request.adapterData,
                        unixTimestamp,
                        System.Convert.ToBase64String(
                            TVinciShared.EncryptUtils.AesEncrypt(request.paymentGateway.SharedSecret, TVinciShared.EncryptUtils.HashSHA1(signature))));

                    LogAdapterResponse(adapterResponse, "Transact After NoConfigurationFound");
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error in transact: error = {7}, siteguid = {0}, charge id = {1}, price = {2}, currency = {3}, product id = {4}, product type = {5}, content id = {6}",
                    request != null && request.siteGuid != null ? request.siteGuid : string.Empty, // {0}
                    request != null && request.chargeId != null ? request.chargeId : string.Empty, // {1}  
                    request != null ? request.price : 0,                                           // {2}
                    request != null && request.currency != null ? request.currency : string.Empty, // {3}
                    request != null ? request.productId : 0,                                       // {4}
                    request != null ? request.productType.ToString() : string.Empty,               // {5}
                    request != null ? request.contentId : 0,                                       // {6}
                    ex);                                                                           // {7}
            }

            return adapterResponse;
        }

        public APILogic.PaymentGWAdapter.TransactionResponse ProcessRenewal(TransactionRenewalRequest request)
        {
            APILogic.PaymentGWAdapter.TransactionResponse adapterResponse = ValidateRequest(request);

            // If it is not valid - stop and return
            if (adapterResponse.Status != null)
                return adapterResponse;

            this.paymentGatewayId = request.paymentGateway.ID;

            APILogic.PaymentGWAdapter.ServiceClient adapterClient = new APILogic.PaymentGWAdapter.ServiceClient(string.Empty, request.paymentGateway.AdapterUrl);

            if (!string.IsNullOrEmpty(request.paymentGateway.AdapterUrl))
            {
                adapterClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(request.paymentGateway.AdapterUrl);
            }

            //set unixTimestamp
            long unixTimestamp = TVinciShared.DateUtils.DateTimeToUnixTimestamp(DateTime.UtcNow);

            //set signature
            string signature = string.Concat(this.paymentGatewayId, request.siteGuid, request.productId, request.productCode, request.ExternalTransactionId,
                request.GracePeriodMinutes, request.price, request.currency, request.chargeId, unixTimestamp, request.paymentMethodExternalId);

            try
            {
                //call Adapter Transact
                adapterResponse = adapterClient.ProcessRenewal(this.paymentGatewayId, request.siteGuid, request.productId.ToString(), request.productCode, request.ExternalTransactionId,
                                                               request.GracePeriodMinutes, request.price, request.currency, request.chargeId, request.paymentMethodExternalId, unixTimestamp,
                                                               Convert.ToBase64String(TVinciShared.EncryptUtils.AesEncrypt(request.paymentGateway.SharedSecret, TVinciShared.EncryptUtils.HashSHA1(signature))));

                // log response
                LogAdapterResponse(adapterResponse, "Renewal");

                if (adapterResponse != null && adapterResponse.Status != null &&
                    adapterResponse.Status.Code == (int)PaymentGatewayAdapterStatus.NoConfigurationFound)
                {
                    string key = string.Format("PaymentGateway_Adapter_Locker_{0}", paymentGatewayId);

                    // Build dictionary for synchronized action
                    Dictionary<string, object> parameters = new Dictionary<string, object>()
                    {
                        {PARAMETER_PAYMENT_GATEWAY, request.paymentGateway},
                        {PARAMETER_GROUP_ID, request.groupId}
                    };

                    configurationSynchronizer.DoAction(key, parameters);

                    //call Adapter Transact - after it is configured
                    adapterResponse = adapterClient.ProcessRenewal(this.paymentGatewayId, request.siteGuid, request.productId.ToString(), request.productCode, request.ExternalTransactionId,
                                                                request.GracePeriodMinutes, request.price, request.currency, request.chargeId, request.paymentMethodExternalId, unixTimestamp,
                                                                Convert.ToBase64String(TVinciShared.EncryptUtils.AesEncrypt(request.paymentGateway.SharedSecret, TVinciShared.EncryptUtils.HashSHA1(signature))));

                    // log response
                    LogAdapterResponse(adapterResponse, "Renewal");
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error in transact: error = {7}, siteguid = {0}, charge id = {1}, price = {2}, currency = {3}, product id = {4}, product type = {5}, content id = {6}",
                    request != null && request.siteGuid != null ? request.siteGuid : string.Empty, // {0}
                    request != null && request.chargeId != null ? request.chargeId : string.Empty, // {1}  
                    request != null ? request.price : 0,                                           // {2}
                    request != null && request.currency != null ? request.currency : string.Empty, // {3}
                    request != null ? request.productId : 0,                                       // {4}
                    request != null ? request.productType.ToString() : string.Empty,               // {5}
                    request != null ? request.contentId : 0,                                       // {6}
                    ex);                                                                           // {7}
            }

            return adapterResponse;
        }

        public APILogic.PaymentGWAdapter.PaymentMethodResponse RemoveHouseholdPaymentMethod(PaymentGateway paymentGateway, int groupId, string chargeId, string paymentMethodExternalId)
        {
            APILogic.PaymentGWAdapter.PaymentMethodResponse adapterResponse = null;

            this.paymentGatewayId = paymentGateway.ID;

            APILogic.PaymentGWAdapter.ServiceClient adapterClient = new APILogic.PaymentGWAdapter.ServiceClient(string.Empty, paymentGateway.AdapterUrl);

            if (!string.IsNullOrEmpty(paymentGateway.AdapterUrl))
            {
                adapterClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(paymentGateway.AdapterUrl);
            }

            //set unixTimestamp
            long unixTimestamp = TVinciShared.DateUtils.DateTimeToUnixTimestamp(DateTime.UtcNow);

            //set signature
            string signature = string.Concat(this.paymentGatewayId, chargeId, unixTimestamp, paymentMethodExternalId);

            try
            {
                //call Adapter
                adapterResponse = adapterClient.RemovePaymentMethod(this.paymentGatewayId,
                    chargeId,
                    paymentMethodExternalId,
                    unixTimestamp,
                    System.Convert.ToBase64String(
                        TVinciShared.EncryptUtils.AesEncrypt(paymentGateway.SharedSecret, TVinciShared.EncryptUtils.HashSHA1(signature))));

                //LogAdapterResponse(adapterResponse, "Transact");

                if (adapterResponse != null && adapterResponse.Status != null &&
                    adapterResponse.Status.Code == (int)PaymentGatewayAdapterStatus.NoConfigurationFound)
                {
                    string key = string.Format("PaymentGateway_Adapter_Locker_{0}", paymentGatewayId);

                    // Build dictionary for synchronized action
                    Dictionary<string, object> parameters = new Dictionary<string, object>()
                    {
                        {PARAMETER_PAYMENT_GATEWAY, paymentGateway},
                        {PARAMETER_GROUP_ID, groupId}
                    };

                    configurationSynchronizer.DoAction(key, parameters);

                    //call Adapter - after it is configured
                    adapterResponse = adapterClient.RemovePaymentMethod(this.paymentGatewayId,
                    chargeId.ToString(),
                    paymentMethodExternalId,
                    unixTimestamp,
                    System.Convert.ToBase64String(
                        TVinciShared.EncryptUtils.AesEncrypt(paymentGateway.SharedSecret, TVinciShared.EncryptUtils.HashSHA1(signature))));

                    //LogAdapterResponse(adapterResponse, "Transact After NoConfigurationFound");
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error in transact: error = {1}, charge id = {0}",                   
                    chargeId,
                    ex);  
            }

            return adapterResponse;
        }

        /// <summary>
        /// Convert ApiObject enum to PaymentGWAdapter enum
        /// </summary>
        /// <param name="origin"></param>
        /// <returns></returns>
        private APILogic.PaymentGWAdapter.eTransactionType ConvertTransactionType(eTransactionType origin)
        {
            APILogic.PaymentGWAdapter.eTransactionType result = (APILogic.PaymentGWAdapter.eTransactionType)0;

            switch (origin)
            {
                case eTransactionType.PPV:
                    {
                        result = APILogic.PaymentGWAdapter.eTransactionType.PPV;
                        break;
                    }
                case eTransactionType.Subscription:
                    {
                        result = APILogic.PaymentGWAdapter.eTransactionType.Subscription;
                        break;
                    }
                case eTransactionType.Collection:
                    {
                        result = APILogic.PaymentGWAdapter.eTransactionType.Collection;
                        break;
                    }
                default:
                    {
                        break;
                    }
            }

            return result;
        }

        public APILogic.PaymentGWAdapter.TransactionResponse CheckPendingTransaction(PendingTransactionRequest request)
        {
            APILogic.PaymentGWAdapter.TransactionResponse adapterResponse = null;

            this.paymentGatewayId = request.paymentGateway.ID;

            APILogic.PaymentGWAdapter.ServiceClient adapterClient = new APILogic.PaymentGWAdapter.ServiceClient(string.Empty, request.paymentGateway.AdapterUrl);

            if (!string.IsNullOrEmpty(request.paymentGateway.AdapterUrl))
            {
                adapterClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(request.paymentGateway.AdapterUrl);
            }

            //set unixTimestamp
            long unixTimestamp = TVinciShared.DateUtils.DateTimeToUnixTimestamp(DateTime.UtcNow);

            //set signature
            string signature = string.Concat(paymentGatewayId, request.pendingExternalTransactionId, unixTimestamp);

            try
            {
                //call Adapter verify
                adapterResponse = adapterClient.VerifyPendingTransaction(paymentGatewayId, request.pendingExternalTransactionId, unixTimestamp,
                    System.Convert.ToBase64String(TVinciShared.EncryptUtils.AesEncrypt(request.paymentGateway.SharedSecret, TVinciShared.EncryptUtils.HashSHA1(signature))));

                LogAdapterResponse(adapterResponse, "Verify");

                if (adapterResponse != null && adapterResponse.Status != null &&
                    adapterResponse.Status.Code == (int)PaymentGatewayAdapterStatus.NoConfigurationFound)
                {
                    string key = string.Format("PaymentGateway_Adapter_Locker_{0}", paymentGatewayId);

                    // Build dictionary for synchronized action
                    Dictionary<string, object> parameters = new Dictionary<string, object>()
                    {
                        {PARAMETER_PAYMENT_GATEWAY, request.paymentGateway},
                        {PARAMETER_GROUP_ID, request.groupId}
                    };

                    configurationSynchronizer.DoAction(key, parameters);

                    //call Adapter verify - after it is configured
                    adapterResponse = adapterClient.VerifyPendingTransaction(paymentGatewayId, request.pendingExternalTransactionId, unixTimestamp,
                        System.Convert.ToBase64String(TVinciShared.EncryptUtils.AesEncrypt(request.paymentGateway.SharedSecret, TVinciShared.EncryptUtils.HashSHA1(signature))));

                    LogAdapterResponse(adapterResponse, "Verify after NoConfigurationFound");
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error in verify: error = {0}, siteguid = {1}, product id = {2}, product type = {3}",
                            ex,                                                                             // {0}
                            request != null && request.siteGuid != null ? request.siteGuid : string.Empty,  // {1}
                            request != null ? request.productId : 0,                                        // {2}
                            request != null ? request.productType.ToString() : string.Empty);               // {3}
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
                PaymentGateway paymentGateway = null;
                int groupId = 0;

                if (parameters.ContainsKey(PARAMETER_PAYMENT_GATEWAY))
                {
                    paymentGateway = (PaymentGateway)parameters[PARAMETER_PAYMENT_GATEWAY];
                }

                if (parameters.ContainsKey(PARAMETER_GROUP_ID))
                {
                    groupId = (int)parameters[PARAMETER_GROUP_ID];
                }

                result = this.SendConfiguration(paymentGateway, groupId);
            }

            return result;
        }

        /// <summary>
        /// Logs what came
        /// </summary>
        /// <param name="adapterResponse"></param>
        private static void LogAdapterResponse(APILogic.PaymentGWAdapter.TransactionResponse adapterResponse, string action)
        {
            string logMessage = string.Empty;

            if (adapterResponse == null)
            {
                logMessage = string.Format("Payment Gateway Adapter {0} Result is null", action != null ? action : string.Empty);
            }
            else if (adapterResponse.Status == null)
            {
                logMessage = string.Format("Payment Gateway Adapter {0} Result's status is null", action != null ? action : string.Empty);
            }
            else if (adapterResponse.Transaction == null)
            {
                logMessage = string.Format("Payment Gateway Adapter {0} Result Status: Message = {1}, Code = {2}",
                                 action != null ? action : string.Empty,                                                                                                                // {0}
                                 adapterResponse != null && adapterResponse.Status != null && adapterResponse.Status.Message != null ? adapterResponse.Status.Message : string.Empty,   // {1}
                                 adapterResponse != null && adapterResponse.Status != null ? adapterResponse.Status.Code : -1);                                                         // {2}
            }
            else
            {
                logMessage = string.Format("Payment Gateway Adapter {0} Result Status: Message = {1}, Code = {2} Transaction: FailReasonCode = {3}, PGMessage = {4}, PGPayload = {5}, PGStatus = {6}, PGTransactionID = {7}, StateCode = {8}",
                    action != null ? action : string.Empty,                                                                                                                                              // {0}
                    adapterResponse != null && adapterResponse.Status != null && adapterResponse.Status.Message != null ? adapterResponse.Status.Message : string.Empty,                                 // {1}
                    adapterResponse != null && adapterResponse.Status != null ? adapterResponse.Status.Code : -1,                                                                                        // {2}
                    adapterResponse != null && adapterResponse.Transaction != null ? adapterResponse.Transaction.FailReasonCode : -1,                                                                    // {3}
                    adapterResponse != null && adapterResponse.Transaction != null && adapterResponse.Transaction.PGMessage != null ? adapterResponse.Transaction.PGMessage : string.Empty,              // {4}
                    adapterResponse != null && adapterResponse.Transaction != null && adapterResponse.Transaction.PGPayload != null ? adapterResponse.Transaction.PGPayload : string.Empty,              // {5}
                    adapterResponse != null && adapterResponse.Transaction != null && adapterResponse.Transaction.PGStatus != null ? adapterResponse.Transaction.PGStatus : string.Empty,                // {6}
                    adapterResponse != null && adapterResponse.Transaction != null && adapterResponse.Transaction.PGTransactionID != null ? adapterResponse.Transaction.PGTransactionID : string.Empty,  // {7}
                    adapterResponse != null && adapterResponse.Transaction != null ? adapterResponse.Transaction.StateCode : 0);                                                                         // {8}
            }

            log.Debug(logMessage);
        }

        public bool SendConfiguration(PaymentGateway paymentGateway, int groupId)
        {
            bool result = false;

            if (paymentGateway != null && !string.IsNullOrEmpty(paymentGateway.AdapterUrl))
            {
                //set unixTimestamp
                long unixTimestamp = TVinciShared.DateUtils.DateTimeToUnixTimestamp(DateTime.UtcNow);

                //set signature
                string signature = string.Concat(this.paymentGatewayId, paymentGateway.TransactUrl, paymentGateway.StatusUrl, paymentGateway.RenewUrl,
                    paymentGateway.Settings != null ?
                        string.Concat(paymentGateway.Settings.Select(setting => string.Concat(setting.key, setting.value))) : string.Empty,
                        groupId, unixTimestamp);

                try
                {
                    APILogic.PaymentGWAdapter.ServiceClient client = new APILogic.PaymentGWAdapter.ServiceClient(string.Empty, paymentGateway.AdapterUrl);

                    //call Adapter Transact
                    APILogic.PaymentGWAdapter.AdapterStatus adapterResponse =
                        client.SetConfiguration(this.paymentGatewayId, paymentGateway.TransactUrl, paymentGateway.StatusUrl, paymentGateway.RenewUrl,
                        paymentGateway.Settings != null ? paymentGateway.Settings.Select(setting => new APILogic.PaymentGWAdapter.KeyValue()
                        {
                            Key = setting.key,
                            Value = setting.value
                        }).ToArray() : null,
                        groupId,
                        unixTimestamp,
                        System.Convert.ToBase64String(TVinciShared.EncryptUtils.AesEncrypt(paymentGateway.SharedSecret, TVinciShared.EncryptUtils.HashSHA1(signature))));

                    if (adapterResponse != null)
                    {
                        log.DebugFormat("Payment Gateway Adapter Transaction Result AdapterStatus = {0}", adapterResponse);
                        result = adapterResponse.Code == (int)PaymentGatewayAdapterStatus.OK;
                    }
                    else
                        log.Error("Adapter response is null");
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("Failed ex = {0}, payment gateway id = {1}", ex, paymentGateway != null ? paymentGateway.ID : 0);
                }
            }

            return result;
        }

        public APILogic.PaymentGWAdapter.ConfigurationResponse GetAdapterConfiguration(PaymentGateway paymentGateway, int groupId, string intent, List<ApiObjects.KeyValuePair> extraParams)
        {
            APILogic.PaymentGWAdapter.ConfigurationResponse adapterResponse = new APILogic.PaymentGWAdapter.ConfigurationResponse();
            adapterResponse.Status = new APILogic.PaymentGWAdapter.AdapterStatus() { Code = (int)PaymentGatewayAdapterStatus.Error };
            APILogic.PaymentGWAdapter.KeyValue[] adapterKeyValue = extraParams.Select(x => new APILogic.PaymentGWAdapter.KeyValue() { Key = x.key, Value = x.value }).ToArray();

            try
            {
                if (paymentGateway != null && !string.IsNullOrEmpty(paymentGateway.AdapterUrl))
                {
                    this.paymentGatewayId = paymentGateway.ID;

                    APILogic.PaymentGWAdapter.ServiceClient client = new APILogic.PaymentGWAdapter.ServiceClient(string.Empty, paymentGateway.AdapterUrl);
                    if (!string.IsNullOrEmpty(paymentGateway.AdapterUrl))
                    {
                        client.Endpoint.Address = new System.ServiceModel.EndpointAddress(paymentGateway.AdapterUrl);
                    }

                    //set unixTimestamp
                    long unixTimestamp = ODBCWrapper.Utils.DateTimeToUnixTimestamp(DateTime.UtcNow);

                    //set signature
                    string signature = string.Concat(paymentGateway.ID, unixTimestamp, intent, paymentGateway.Settings != null ?
                        string.Concat(adapterKeyValue.Select(x => string.Concat(x.Key, x.Value))) : string.Empty);

                    //call Adapter
                    adapterResponse = client.GetConfiguration(paymentGateway.ID, intent, adapterKeyValue, unixTimestamp,
                        System.Convert.ToBase64String(TVinciShared.EncryptUtils.AesEncrypt(paymentGateway.SharedSecret, TVinciShared.EncryptUtils.HashSHA1(signature))));

                    log.Debug(string.Format("GetAdapterConfiguration - paymentGateway id = {0}, groupId = {1} intent = {2}", paymentGateway.ID, groupId, intent));

                    #region config not found
                    if (adapterResponse != null && adapterResponse.Status != null &&
                        adapterResponse.Status.Code == (int)PaymentGatewayAdapterStatus.NoConfigurationFound)
                    {
                        string key = string.Format("PaymentGateway_Adapter_Locker_{0}", paymentGatewayId);

                        // Build dictionary for synchronized action
                        Dictionary<string, object> parameters = new Dictionary<string, object>()
                        {
                            {PARAMETER_PAYMENT_GATEWAY, paymentGateway},
                            {PARAMETER_GROUP_ID, groupId}
                        };

                        configurationSynchronizer.DoAction(key, parameters);

                        log.Debug(string.Format("GetAdapterConfiguration - no configuration, sending again - paymentGateway id = {0}, groupId = {1} intent = {2}", paymentGateway.ID, groupId, intent));

                        //call Adapter after it is configured
                        adapterResponse = client.GetConfiguration(paymentGateway.ID, intent, adapterKeyValue, unixTimestamp,
                        System.Convert.ToBase64String(TVinciShared.EncryptUtils.AesEncrypt(paymentGateway.SharedSecret, TVinciShared.EncryptUtils.HashSHA1(signature))));
                    }
                    #endregion

                    if (adapterResponse != null && adapterResponse.Status != null && adapterResponse.Status.Code == (int)PaymentGatewayAdapterStatus.OK)
                    {
                        string vals = string.Empty;
                        if (adapterResponse.Configuration != null && adapterResponse.Configuration.Length > 0)
                            vals = string.Join(", ", adapterResponse.Configuration.Select(x => x.Key + ":" + x.Value));

                        log.DebugFormat("Payment Gateway Adapter GetConfiguration Result: AdapterID = {0}, Configuration = {1}", paymentGateway.ID, vals);
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error in GetAdapterConfiguration: error = {0}", ex);
            }

            return adapterResponse;
        }

        private APILogic.PaymentGWAdapter.TransactionResponse ValidateRequest(TransactionRequest request)
        {
            APILogic.PaymentGWAdapter.TransactionResponse adapterResponse = new APILogic.PaymentGWAdapter.TransactionResponse()
            {
                Status = null,
                Transaction = null
            };

            if (request == null)
            {
                adapterResponse.Status = new APILogic.PaymentGWAdapter.AdapterStatus()
                {
                    Code = (int)PaymentGatewayAdapterStatus.Error,
                    Message = "No request sent"
                };
            }
            else if (request.paymentGateway == null)
            {
                adapterResponse.Status = new APILogic.PaymentGWAdapter.AdapterStatus()
                {
                    Code = (int)PaymentGatewayAdapterStatus.Error,
                    Message = "No payment gateway sent"
                };
            }
            else if (string.IsNullOrEmpty(request.paymentGateway.AdapterUrl))
            {
                adapterResponse.Status = new APILogic.PaymentGWAdapter.AdapterStatus()
                {
                    Code = (int)PaymentGatewayAdapterStatus.Error,
                    Message = "Payment gateway has no adapter URL"
                };
            }

            return adapterResponse;
        }

        public APILogic.PaymentGWAdapter.TransactionResponse VerifyReceipt(VerifyReceiptRequest request)
        {
            APILogic.PaymentGWAdapter.TransactionResponse adapterResponse = ValidateRequest(request);

            // If it is not valid - stop and return
            if (adapterResponse.Status != null)
            {
                return adapterResponse;
            }

            this.paymentGatewayId = request.paymentGateway.ID;

            APILogic.PaymentGWAdapter.ServiceClient adapterClient = new APILogic.PaymentGWAdapter.ServiceClient(string.Empty, request.paymentGateway.AdapterUrl);

            if (!string.IsNullOrEmpty(request.paymentGateway.AdapterUrl))
            {
                adapterClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(request.paymentGateway.AdapterUrl);
            }

            //set unixTimestamp
            long unixTimestamp = TVinciShared.DateUtils.DateTimeToUnixTimestamp(DateTime.UtcNow);

            //set signature
            string signature = string.Concat(this.paymentGatewayId, request.siteGuid, request.userIP, request.productId.ToString(), request.productCode,
                ConvertTransactionType(request.productType), request.purchaseToken, unixTimestamp, request.contentId);

            try
            {
                //call Adapter VerifyTransaction
                adapterResponse = adapterClient.VerifyTransaction(this.paymentGatewayId,
                    request.siteGuid,
                    request.userIP,
                    request.productId.ToString(),
                    request.productCode,
                    ConvertTransactionType(request.productType),
                    request.purchaseToken,
                    unixTimestamp,
                    System.Convert.ToBase64String(
                    TVinciShared.EncryptUtils.AesEncrypt(request.paymentGateway.SharedSecret, TVinciShared.EncryptUtils.HashSHA1(signature)))
                    ,
                    request.contentId.ToString()
                    );

                LogAdapterResponse(adapterResponse, "VerifyTransaction");

                if (adapterResponse != null && adapterResponse.Status != null &&
                    adapterResponse.Status.Code == (int)PaymentGatewayAdapterStatus.NoConfigurationFound)
                {
                    string key = string.Format("PaymentGateway_Adapter_Locker_{0}", paymentGatewayId);

                    // Build dictionary for synchronized action
                    Dictionary<string, object> parameters = new Dictionary<string, object>()
                    {
                        {PARAMETER_PAYMENT_GATEWAY, request.paymentGateway},
                        {PARAMETER_GROUP_ID, request.groupId}
                    };

                    configurationSynchronizer.DoAction(key, parameters);

                    //call Adapter Verify Transaction - after it is configured
                    adapterResponse = adapterClient.VerifyTransaction(this.paymentGatewayId,
                        request.siteGuid,
                        request.userIP,
                        request.productId.ToString(),
                        request.productCode,
                        ConvertTransactionType(request.productType),
                        request.purchaseToken,
                        unixTimestamp,
                        System.Convert.ToBase64String(TVinciShared.EncryptUtils.AesEncrypt(request.paymentGateway.SharedSecret, TVinciShared.EncryptUtils.HashSHA1(signature)))
                        ,
                         request.contentId.ToString()
                        );

                    LogAdapterResponse(adapterResponse, "VerifyTransaction");
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error in transact: error = {7}, siteguid = {0}, charge id = {1}, price = {2}, currency = {3}, product id = {4}, product type = {5}, content id = {6}",
                 request != null && request.siteGuid != null ? request.siteGuid : string.Empty, // {0}
                 request != null && request.chargeId != null ? request.chargeId : string.Empty, // {1}  
                 request != null ? request.price : 0,                                           // {2}
                 request != null && request.currency != null ? request.currency : string.Empty, // {3}
                 request != null ? request.productId : 0,                                       // {4}
                 request != null ? request.productType.ToString() : string.Empty,               // {5}
                 request != null ? request.contentId : 0,                                       // {6}
                 ex);                                                                           // {7}
            }

            return adapterResponse;
        }

        #endregion

        internal APILogic.PaymentGWAdapter.PaymentMethodResponse RemoveAccount(PaymentGateway paymentGateway, int groupId, string chargeId, List<string> paymentMethodExternalIds)
        {
            APILogic.PaymentGWAdapter.PaymentMethodResponse adapterResponse = null;

            this.paymentGatewayId = paymentGateway.ID;

            APILogic.PaymentGWAdapter.ServiceClient adapterClient = new APILogic.PaymentGWAdapter.ServiceClient(string.Empty, paymentGateway.AdapterUrl);

            if (!string.IsNullOrEmpty(paymentGateway.AdapterUrl))
            {
                adapterClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(paymentGateway.AdapterUrl);
            }

            //set unixTimestamp
            long unixTimestamp = TVinciShared.DateUtils.DateTimeToUnixTimestamp(DateTime.UtcNow);

            //set signature
            string signature = string.Concat(this.paymentGatewayId, chargeId, unixTimestamp, paymentMethodExternalIds != null ? string.Concat(paymentMethodExternalIds) : string.Empty);

            try
            {
                //call Adapter
                adapterResponse = adapterClient.RemoveAccount(this.paymentGatewayId,
                    chargeId,
                    paymentMethodExternalIds != null ? paymentMethodExternalIds.ToArray() : null,
                    unixTimestamp,
                    System.Convert.ToBase64String(
                        TVinciShared.EncryptUtils.AesEncrypt(paymentGateway.SharedSecret, TVinciShared.EncryptUtils.HashSHA1(signature))));

                if (adapterResponse != null && adapterResponse.Status != null &&
                    adapterResponse.Status.Code == (int)PaymentGatewayAdapterStatus.NoConfigurationFound)
                {
                    string key = string.Format("PaymentGateway_Adapter_Locker_{0}", paymentGatewayId);

                    // Build dictionary for synchronized action
                    Dictionary<string, object> parameters = new Dictionary<string, object>()
                    {
                        {PARAMETER_PAYMENT_GATEWAY, paymentGateway},
                        {PARAMETER_GROUP_ID, groupId}
                    };

                    configurationSynchronizer.DoAction(key, parameters);

                    //call Adapter - after it is configured
                    adapterResponse = adapterClient.RemoveAccount(this.paymentGatewayId,
                    chargeId.ToString(),
                    paymentMethodExternalIds != null ? paymentMethodExternalIds.ToArray() : null,
                    unixTimestamp,
                    System.Convert.ToBase64String(
                        TVinciShared.EncryptUtils.AesEncrypt(paymentGateway.SharedSecret, TVinciShared.EncryptUtils.HashSHA1(signature))));
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error in RemoveAccount: error = {1}, charge id = {0}",
                    chargeId,
                    ex);
            }

            return adapterResponse;
        }
    }
}
