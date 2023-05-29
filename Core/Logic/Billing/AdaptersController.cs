using ApiObjects;
using ApiObjects.Billing;
using ApiObjects.Response;
using Phx.Lib.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using CouchbaseManager;
using System.Threading;
using Synchronizer;
using TVinciShared;
using PGWServiceClient = APILogic.PaymentGWAdapter.ServiceClient;
using APILogic.PaymentGWAdapter;
using Phx.Lib.Appconfig;

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
        private static readonly object generalLocker = new object();
        private static readonly Random random;
        #endregion

        #region Normal Data Members
        private int paymentGatewayId;
        private CouchbaseSynchronizer configurationSynchronizer;
        private readonly PGWServiceClient _PGWAdapterClient;
        #endregion

        #region Getter

        /// <summary>
        /// Gets the singleton instance of the adapter controller which is relevant for the given payment gateway ID
        /// </summary>
        /// <param name="paymentGatewayId"></param>
        /// <returns></returns>
        public static AdaptersController GetInstance(int paymentGatewayId, string adapterUrl)
        {
            if (!instances.ContainsKey(paymentGatewayId))
            {
                lock (generalLocker)
                {
                    if (!instances.ContainsKey(paymentGatewayId))
                    {
                        instances[paymentGatewayId] = new AdaptersController(adapterUrl);
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

        private AdaptersController(string adapterUrl)
        {
            configurationSynchronizer = new CouchbaseSynchronizer(100);
            configurationSynchronizer.SynchronizedAct += synchronizer_SynchronizedAct;
            _PGWAdapterClient = GetPGWServiceClient(adapterUrl);
        }

        public static PGWServiceClient GetPGWServiceClient(string adapterUrl)
        {
            var behvaiour = PGWServiceClient.EndpointConfiguration.BasicHttpBinding;
            var adapterClient = new PGWServiceClient(behvaiour, adapterUrl);
            adapterClient.ConfigureServiceClient(ApplicationConfiguration.Current.AdaptersClientConfiguration.PaymentGatewayAdapter);
            return adapterClient;
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


            if (!string.IsNullOrEmpty(request.paymentGateway.AdapterUrl))
            {
                _PGWAdapterClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(request.paymentGateway.AdapterUrl);
            }

            //set unixTimestamp
            long unixTimestamp = TVinciShared.DateUtils.DateTimeToUtcUnixTimestampSeconds(DateTime.UtcNow);

            //set signature
            string signature = string.Concat(this.paymentGatewayId, request.siteGuid, request.chargeId, request.price, request.currency,
                request.productId, request.productType, request.contentId, request.userIP, unixTimestamp, request.paymentMethodExternalId);

            try
            {
                //call Adapter Transact
                adapterResponse = _PGWAdapterClient.Transact(this.paymentGatewayId,
                        request.siteGuid, request.chargeId,
                        request.price, request.currency, request.productId.ToString(),
                        ConvertTransactionType(request.productType),
                        request.contentId.ToString(), request.userIP,
                        request.paymentMethodExternalId,
                        request.adapterData,
                        unixTimestamp,
                        Convert.ToBase64String(
                        EncryptUtils.AesEncrypt(request.paymentGateway.SharedSecret, TVinciShared.EncryptUtils.HashSHA1(signature))));

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
                    adapterResponse = _PGWAdapterClient.Transact(this.paymentGatewayId,
                            request.siteGuid, request.chargeId,
                            request.price, request.currency, request.productId.ToString(),
                            ConvertTransactionType(request.productType),
                            request.contentId.ToString(), request.userIP,
                            request.paymentMethodExternalId,
                            request.adapterData,
                            unixTimestamp,
                            Convert.ToBase64String(
                            EncryptUtils.AesEncrypt(request.paymentGateway.SharedSecret, TVinciShared.EncryptUtils.HashSHA1(signature))));

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

        public APILogic.PaymentGWAdapter.TransactionResponse ProcessRenewal(TransactionUnifiedRenewal request)
        {
            APILogic.PaymentGWAdapter.TransactionResponse adapterResponse = ValidateRequest(request);

            // If it is not valid - stop and return
            if (adapterResponse.Status != null)
                return adapterResponse;

            this.paymentGatewayId = request.paymentGateway.ID;

            if (!string.IsNullOrEmpty(request.paymentGateway.AdapterUrl))
            {
                _PGWAdapterClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(request.paymentGateway.AdapterUrl);
            }

            //set unixTimestamp
            long unixTimestamp = TVinciShared.DateUtils.DateTimeToUtcUnixTimestampSeconds(DateTime.UtcNow);

            //set signature
            string signature = string.Concat(this.paymentGatewayId, request.siteGuid, request.productId, request.productCode, request.ExternalTransactionId,
                request.GracePeriodMinutes, request.price, request.currency, request.chargeId, unixTimestamp, request.paymentMethodExternalId);

            try
            {
                //call Adapter Transact
                var adapterData = request.AdapterData?.Select(x => new KeyValue { Key = x.key, Value = x.value }).ToArray();
                adapterResponse = _PGWAdapterClient.ProcessRenewal(this.paymentGatewayId, request.siteGuid, request.productId.ToString(), request.productCode, request.ExternalTransactionId,
                                                               request.GracePeriodMinutes, request.price, request.currency, request.chargeId, request.paymentMethodExternalId, unixTimestamp,
                                                               Convert.ToBase64String(TVinciShared.EncryptUtils.AesEncrypt(request.paymentGateway.SharedSecret, TVinciShared.EncryptUtils.HashSHA1(signature))),
                                                            adapterData);

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
                    adapterResponse = _PGWAdapterClient.ProcessRenewal(this.paymentGatewayId, request.siteGuid, request.productId.ToString(), request.productCode, request.ExternalTransactionId,
                                                                request.GracePeriodMinutes, request.price, request.currency, request.chargeId, request.paymentMethodExternalId, unixTimestamp,
                                                                Convert.ToBase64String(TVinciShared.EncryptUtils.AesEncrypt(request.paymentGateway.SharedSecret, TVinciShared.EncryptUtils.HashSHA1(signature))), adapterData);

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

            if (!string.IsNullOrEmpty(paymentGateway.AdapterUrl))
            {
                _PGWAdapterClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(paymentGateway.AdapterUrl);
            }

            //set unixTimestamp
            long unixTimestamp = TVinciShared.DateUtils.DateTimeToUtcUnixTimestampSeconds(DateTime.UtcNow);

            //set signature
            string signature = string.Concat(this.paymentGatewayId, chargeId, unixTimestamp, paymentMethodExternalId);

            try
            {
                //call Adapter
                adapterResponse = _PGWAdapterClient.RemovePaymentMethodAsync(this.paymentGatewayId,
                    chargeId,
                    paymentMethodExternalId,
                    unixTimestamp,
                    Convert.ToBase64String(
                    EncryptUtils.AesEncrypt(paymentGateway.SharedSecret, TVinciShared.EncryptUtils.HashSHA1(signature)))
                    ).ExecuteAndWait();

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
                    adapterResponse = _PGWAdapterClient.RemovePaymentMethodAsync(this.paymentGatewayId,
                    chargeId.ToString(),
                    paymentMethodExternalId,
                    unixTimestamp,
                    Convert.ToBase64String(
                    EncryptUtils.AesEncrypt(paymentGateway.SharedSecret, TVinciShared.EncryptUtils.HashSHA1(signature)))
                    ).ExecuteAndWait();

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
        private APILogic.PaymentGWAdapter.eTransactionType ConvertTransactionType(ApiObjects.eTransactionType origin)
        {
            APILogic.PaymentGWAdapter.eTransactionType result = (APILogic.PaymentGWAdapter.eTransactionType)0;

            switch (origin)
            {
                case ApiObjects.eTransactionType.PPV:
                    {
                        result = APILogic.PaymentGWAdapter.eTransactionType.PPV;
                        break;
                    }
                case ApiObjects.eTransactionType.Subscription:
                    {
                        result = APILogic.PaymentGWAdapter.eTransactionType.Subscription;
                        break;
                    }
                case ApiObjects.eTransactionType.Collection:
                    {
                        result = APILogic.PaymentGWAdapter.eTransactionType.Collection;
                        break;
                    }
                case ApiObjects.eTransactionType.ProgramAssetGroupOffer:
                    {
                        result = APILogic.PaymentGWAdapter.eTransactionType.PAGO;
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

            if (!string.IsNullOrEmpty(request.paymentGateway.AdapterUrl))
            {
                _PGWAdapterClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(request.paymentGateway.AdapterUrl);
            }

            //set unixTimestamp
            long unixTimestamp = DateUtils.DateTimeToUtcUnixTimestampSeconds(DateTime.UtcNow);

            //set signature
            string signature = string.Concat(paymentGatewayId, request.pendingExternalTransactionId, unixTimestamp);

            try
            {
                //call Adapter verify
                adapterResponse = _PGWAdapterClient.VerifyPendingTransactionAsync(paymentGatewayId, request.pendingExternalTransactionId, unixTimestamp,
                    Convert.ToBase64String(EncryptUtils.AesEncrypt(request.paymentGateway.SharedSecret, TVinciShared.EncryptUtils.HashSHA1(signature)))
                    ).ExecuteAndWait();

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
                    adapterResponse = _PGWAdapterClient.VerifyPendingTransactionAsync(paymentGatewayId, request.pendingExternalTransactionId, unixTimestamp,
                        Convert.ToBase64String(EncryptUtils.AesEncrypt(request.paymentGateway.SharedSecret, EncryptUtils.HashSHA1(signature)))
                        ).ExecuteAndWait();

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
            var actionStr = action ?? string.Empty;

            if (adapterResponse == null)
            {
                logMessage = $"Payment Gateway Adapter {actionStr} Result is null";
            }
            else if (adapterResponse.Status == null)
            {
                logMessage = $"Payment Gateway Adapter {actionStr} Result's status is null";
            }
            else
            {
                var message = adapterResponse.Status.Message ?? string.Empty;

                if (adapterResponse.Transaction == null)
                {
                    logMessage = $"Payment Gateway Adapter {actionStr} Result Status: Message = {message}, Code = {adapterResponse.Status.Code}";
                }
                else
                {
                    var pGMessage = adapterResponse.Transaction.PGMessage ?? string.Empty;
                    var pGPayload = adapterResponse.Transaction.PGPayload ?? string.Empty;
                    var pGStatus = adapterResponse.Transaction.PGStatus ?? string.Empty;
                    var pGTransactionID = adapterResponse.Transaction.PGTransactionID ?? string.Empty;
                    var stateCode = adapterResponse.Transaction.StateCode;
                    var adapterData = adapterResponse.Transaction.AdapterData != null ? string.Join("; ", adapterResponse.Transaction.AdapterData.Select(x => $"key:{x.Key}, value:{x.Value}")) : string.Empty;

                    logMessage = $"Payment Gateway Adapter {actionStr} Result Status: Message = {message}, Code = {adapterResponse.Status.Code} Transaction: FailReasonCode = {adapterResponse.Transaction.FailReasonCode}, " +
                                 $" PGMessage = {pGMessage}, PGPayload = {pGPayload}, PGStatus = {pGStatus}, PGTransactionID = {pGTransactionID}, StateCode = {stateCode}, AdapterData={adapterData}.";
                }

            }

            log.Debug(logMessage);
        }

        public bool SendConfiguration(PaymentGateway paymentGateway, int groupId)
        {
            bool result = false;

            if (paymentGateway != null && !string.IsNullOrEmpty(paymentGateway.AdapterUrl))
            {
                //set unixTimestamp
                long unixTimestamp = TVinciShared.DateUtils.DateTimeToUtcUnixTimestampSeconds(DateTime.UtcNow);

                //set signature
                string signature = string.Concat(this.paymentGatewayId, paymentGateway.TransactUrl, paymentGateway.StatusUrl, paymentGateway.RenewUrl,
                    paymentGateway.Settings != null ?
                        string.Concat(paymentGateway.Settings.Select(setting => string.Concat(setting.key, setting.value))) : string.Empty,
                        groupId, unixTimestamp);

                try
                {

                    //call Adapter Transact
                    APILogic.PaymentGWAdapter.AdapterStatus adapterResponse =
                        _PGWAdapterClient.SetConfigurationAsync(this.paymentGatewayId, paymentGateway.TransactUrl, paymentGateway.StatusUrl, paymentGateway.RenewUrl,
                            paymentGateway.Settings != null ? paymentGateway.Settings.Select(setting => new APILogic.PaymentGWAdapter.KeyValue()
                            {
                                Key = setting.key,
                                Value = setting.value
                            }).ToArray() : null,
                            groupId,
                            unixTimestamp,
                            Convert.ToBase64String(EncryptUtils.AesEncrypt(paymentGateway.SharedSecret, EncryptUtils.HashSHA1(signature)))
                        ).ExecuteAndWait();

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

                    if (!string.IsNullOrEmpty(paymentGateway.AdapterUrl))
                    {
                        _PGWAdapterClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(paymentGateway.AdapterUrl);
                    }

                    //set unixTimestamp
                    long unixTimestamp = DateUtils.DateTimeToUtcUnixTimestampSeconds(DateTime.UtcNow);

                    //set signature
                    string signature = string.Concat(paymentGateway.ID, unixTimestamp, intent, paymentGateway.Settings != null ?
                        string.Concat(adapterKeyValue.Select(x => string.Concat(x.Key, x.Value))) : string.Empty);

                    //call Adapter
                    adapterResponse = _PGWAdapterClient.GetConfigurationAsync(paymentGateway.ID, intent, adapterKeyValue, unixTimestamp,
                        Convert.ToBase64String(EncryptUtils.AesEncrypt(paymentGateway.SharedSecret, EncryptUtils.HashSHA1(signature)))
                        ).ExecuteAndWait();

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
                        adapterResponse = _PGWAdapterClient.GetConfigurationAsync(paymentGateway.ID, intent, adapterKeyValue, unixTimestamp,
                        Convert.ToBase64String(EncryptUtils.AesEncrypt(paymentGateway.SharedSecret, EncryptUtils.HashSHA1(signature)))
                        ).ExecuteAndWait();
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

            if (!string.IsNullOrEmpty(request.paymentGateway.AdapterUrl))
            {
                _PGWAdapterClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(request.paymentGateway.AdapterUrl);
            }

            //set unixTimestamp
            long unixTimestamp = DateUtils.DateTimeToUtcUnixTimestampSeconds(DateTime.UtcNow);

            //set signature
            string signature = string.Concat(this.paymentGatewayId, request.siteGuid, request.userIP, request.productId.ToString(), request.productCode,
                ConvertTransactionType(request.productType), request.purchaseToken, unixTimestamp, request.contentId);

            try
            {
                //call Adapter VerifyTransaction
                adapterResponse = _PGWAdapterClient.VerifyTransactionAsync(this.paymentGatewayId,
                    request.siteGuid,
                    request.userIP,
                    request.productId.ToString(),
                    request.productCode,
                    ConvertTransactionType(request.productType),
                    request.purchaseToken,
                    unixTimestamp,
                    Convert.ToBase64String(EncryptUtils.AesEncrypt(request.paymentGateway.SharedSecret, EncryptUtils.HashSHA1(signature))),
                    request.contentId.ToString(),
                    request.adapterData).ExecuteAndWait();

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
                    adapterResponse = _PGWAdapterClient.VerifyTransactionAsync(this.paymentGatewayId,
                        request.siteGuid,
                        request.userIP,
                        request.productId.ToString(),
                        request.productCode,
                        ConvertTransactionType(request.productType),
                        request.purchaseToken,
                        unixTimestamp,
                        Convert.ToBase64String(EncryptUtils.AesEncrypt(request.paymentGateway.SharedSecret, EncryptUtils.HashSHA1(signature))),
                        request.contentId.ToString(),
                        request.adapterData).ExecuteAndWait();

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
            if (!string.IsNullOrEmpty(paymentGateway.AdapterUrl))
            {
                _PGWAdapterClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(paymentGateway.AdapterUrl);
            }

            //set unixTimestamp
            long unixTimestamp = TVinciShared.DateUtils.DateTimeToUtcUnixTimestampSeconds(DateTime.UtcNow);

            //set signature
            string signature = string.Concat(this.paymentGatewayId, chargeId, unixTimestamp, paymentMethodExternalIds != null ? string.Concat(paymentMethodExternalIds) : string.Empty);

            try
            {
                //call Adapter
                adapterResponse = _PGWAdapterClient.RemoveAccountAsync(this.paymentGatewayId,
                        chargeId,
                        paymentMethodExternalIds != null ? paymentMethodExternalIds.ToArray() : null,
                        unixTimestamp,
                        Convert.ToBase64String(
                        EncryptUtils.AesEncrypt(paymentGateway.SharedSecret, TVinciShared.EncryptUtils.HashSHA1(signature)))
                    ).ExecuteAndWait();

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
                    adapterResponse = _PGWAdapterClient.RemoveAccountAsync(this.paymentGatewayId,
                    chargeId.ToString(),
                    paymentMethodExternalIds != null ? paymentMethodExternalIds.ToArray() : null,
                    unixTimestamp,
                    Convert.ToBase64String(
                    EncryptUtils.AesEncrypt(paymentGateway.SharedSecret, TVinciShared.EncryptUtils.HashSHA1(signature)))
                    ).ExecuteAndWait();
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

        public APILogic.PaymentGWAdapter.TransactionResponse UnifiedProcessRenewal(TransactionUnifiedRenewalRequest request)
        {
            APILogic.PaymentGWAdapter.TransactionResponse adapterResponse = ValidateRequest(request);

            // If it is not valid - stop and return
            if (adapterResponse.Status != null)
                return adapterResponse;

            this.paymentGatewayId = request.paymentGateway.ID;
            if (!string.IsNullOrEmpty(request.paymentGateway.AdapterUrl))
            {
                _PGWAdapterClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(request.paymentGateway.AdapterUrl);
            }

            List<TransactionProductDetails> renewSubscription = new List<TransactionProductDetails>();
            renewSubscription = request.renewRequests.Select(x => new TransactionProductDetails()
            {
                gracePeriodMinutes = x.GracePeriodMinutes,
                price = x.price,
                productCode = x.productCode,
                productid = x.productId.ToString(),
                transactionId = x.ExternalTransactionId
            }).ToList();


            //set unixTimestamp
            long unixTimestamp = DateUtils.DateTimeToUtcUnixTimestampSeconds(DateTime.UtcNow);

            //set signature           

            string signature = string.Concat(this.paymentGatewayId, request.householdId, request.totalPrice, request.currency, request.chargeId, unixTimestamp, request.paymentMethodExternalId);


            try
            {
                var adapterData = request.AdapterData?.Select(x => new KeyValue { Key = x.key, Value = x.value }).ToArray();
                //call Adapter Transact
                adapterResponse = _PGWAdapterClient.UnifiedProcessRenewal(this.paymentGatewayId, request.householdId, request.chargeId, request.paymentMethodExternalId, request.currency,
                                                                               request.totalPrice, renewSubscription.ToArray(), unixTimestamp, Convert.ToBase64String(EncryptUtils.AesEncrypt(request.paymentGateway.SharedSecret,
                                                                               EncryptUtils.HashSHA1(signature))), adapterData);

                // log response
                LogAdapterResponse(adapterResponse, "Renewal");

                if (adapterResponse != null && adapterResponse.Status != null && adapterResponse.Status.Code == (int)PaymentGatewayAdapterStatus.NoConfigurationFound)
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
                    adapterResponse = _PGWAdapterClient.UnifiedProcessRenewal(this.paymentGatewayId, request.householdId, request.chargeId, request.paymentMethodExternalId,
                        request.currency, request.totalPrice, renewSubscription.ToArray(), unixTimestamp,
                        Convert.ToBase64String(EncryptUtils.AesEncrypt(request.paymentGateway.SharedSecret, EncryptUtils.HashSHA1(signature))), adapterData);

                    // log response
                    LogAdapterResponse(adapterResponse, "Renewal");
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error in transact: error = {4}, householdId = {0}, charge id = {1}, totalPrice = {2}, currency = {3}",
                    request != null ? request.householdId : 0, // {0}
                    request != null && request.chargeId != null ? request.chargeId : string.Empty, // {1}  
                    request != null ? request.totalPrice : 0,                                      // {2}
                    request != null && request.currency != null ? request.currency : string.Empty, // {3}                    
                    ex);                                                                           // {4}
            }

            return adapterResponse;
        }

        private APILogic.PaymentGWAdapter.TransactionResponse ValidateRequest(TransactionUnifiedRenewalRequest request)
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
    }
}
