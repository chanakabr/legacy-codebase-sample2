using ApiObjects;
using ApiObjects.Billing;
using ApiObjects.Response;
using KLogMonitor;
using QueueWrapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Net;
using System.Web;
using ApiLogic;


namespace Core.Billing
{
    public class BasePaymentGateway
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        #region Data Member

        protected int groupID;

        #endregion

        #region Consts

        private const int BILLING_PROVIDER = (int)eBillingProvider.PaymentGateway;
        private const int BILLING_TRANSACTION_FAIL_STATUS = 1;
        private const int BILLING_TRANSACTION_SUCCESS_STATUS = 0;
        private const int MIN_RENEWAL_INTERVAL_MINUTES = 15;
        private const int MIN_OSS_CACHE_TIME = 60;

        private const string ACTION_IS_NOT_ALLOWED = "Action is not allowed";
        private const string ERROR_PAYMENT_GATEWAY_NOT_EXIST = "Payment gateway not exist";
        private const string EXTERNAL_IDENTIFIER_REQUIRED = "External identifier is required";
        private const string ERROR_EXT_ID_ALREADY_IN_USE = "external Identifier must be unique";
        private const string ERROR_NO_PGW_RELATED_TO_HOUSEHOLD = "payment gateway not related to household";
        private const string ERROR_CHARGE_ID_MISSING = "Payment gateway charge id required";
        private const string ERROR_SAVING_PAYMENT_GATEWAY_TRANSACTION = "Error while saving payment gateway transaction";
        private const string ERROR_SAVING_PAYMENT_GATEWAY_PENDING = "Error while saving payment gateway pending";
        private const string ERROR_SAVING_PAYMENT_GATEWAY_HOUSEHOLD = "Error while saving payment gateway household";
        private const string NO_PAYMENT_GATEWAY = "No payment gateway sent";
        private const string NO_PAYMENT_GATEWAY_TO_INSERT = "No payment gateway to insert";
        private const string INVALID_PAYMENT_GATEWAY_ID = "New payment gateway id must be 0";
        private const string INVALID_PAYMENT_GATEWAY_NAME = "Payment gateway name must have a value";
        private const string INVALID_PAYMENT_GATEWAY_SHARED_SECRET = "Payment gateway shared secret must have a value";
        private const string PAYMENT_GATEWAY_ALREADY_EXIST = "Payment gateway already exist";
        private const string SIGNATURE_DOES_NOT_MATCH = "Signature doesn't match";
        private const string ERROR_UPDATING_PENDING_TRANSACTION = "Error while updating pending transaction status";
        private const string PAYMENT_GATEWAY_TRANSACTION_NOT_FOUND = "Payment gateway transaction was not found";
        private const string PAYMENT_GATEWAY_TRANSACTION_IS_NOT_PENDING = "Payment gateway transaction is not pending";
        private const string ERROR_HOUSEHOLD_ID_MISSING = "Household Identifier must have a value";
        private const string HOUSEHOLD_ALREADY_SET_TO_PAYMENT_GATEWAY = "Payment gateway already set to household";
        private const string CHARGE_ID_ALREADY_SET_TO_HOUSEHOLD = "Chargeid already set to household";
        private const string CHARGE_ID_NOT_SET = "Charge Identifier not set to household";
        private const string PAYMENT_GATEWAY_SELECTION_IS_DISABLED = "Payment gateway selection is disabled";
        private const string UNKNOWN_TRANSACTION_STATE = "Unknown transaction state";
        private const string PAYMENT_GATEWAY_ID_REQUIRED = "Payment Gateway Id is Required";
        private const string PAYMENT_GATEWAY_NOT_VALID = "Payment Gateway Id is not valid";
        private const string HOUSEHOLD_NOT_SET_TO_PAYMENT_GATEWAY = "Household not set to payment gateway";
        private const string ADAPTER_URL_REQUIRED = "Adapter url must have a value";
        private const string NO_PARAMS_TO_INSERT = "no params to insert";
        private const string NO_PARAMS_TO_DELETE = "no params to delete";
        private const string ERROR_PAYMENT_METHOD_NOT_EXIST = "payment method not exist";
        private const string ERROR_NO_PG_PM_RELATED_TO_HOUSEHOLD = "payment method not related to household";
        private const string ERROR_PG_PM_HOUSEHOLD_USED = "Payment Method Is Used By Household";
        private const string PAYMENT_METHOD_EXTERNAL_ID_REQUIRED = "payment method external id required";
        private const string PAYMENT_METHOD_ID_REQUIRED = "payment method id required";
        private const string PAYMENT_METHOD_NAME_REQUIRED = "payment method name required";
        private const string PAYMENT_METHOD_TYPE_REQUIRED = "Payment method type is required";
        private const string PAYMENT_METHOD_NOT_EXIST = "payment method not exist";
        private const string ERROR_SAVING_PAYMENT_GATEWAY_HOUSEHOLD_PAYMENT_METHOD = "Error saving paymentGateway household paymentMethod";
        private const string ERROR_REMOVING_PAYMENT_GATEWAY_HOUSEHOLD_PAYMENT_METHOD = "Error removing paymentGateway household paymentMethod";
        private const string PAYMENT_METHOD_ALREADY_SET_TO_HOUSEHOLD_PAYMENTGATEWAY = "Payment method already set to household paymentgateway";
        private const string PAYMENT_GATEWAY_NOT_SUPPORT_PAYMENT_METHOD = "Payment gateway not support payment method";
        private const string PAYMENT_GATEWAY_NOT_SET_TO_HOUSEHOLD = "Payment gateway not set to household";

        protected const int FAIL_REASON_EXCEEDED_RETRY_LIMIT_CODE = 26;

        protected const string ROUTING_KEY_CHECK_PENDING_TRANSACTION = "PROCESS_CHECK_PENDING_TRANSACTION\\{0}";

        #endregion

        #region Ctors

        protected BasePaymentGateway()
        {
        }

        public BasePaymentGateway(Int32 groupID)
        {
            this.groupID = groupID;
        }

        #endregion

        #region Virtual Methods
        public virtual PaymentGatewaySettingsResponse GetPaymentGatewateSettings()
        {
            PaymentGatewaySettingsResponse response = new PaymentGatewaySettingsResponse();
            try
            {
                response.pgw = DAL.BillingDAL.GetPaymentGatewaySettingsList(groupID, 0);
                if (response.pgw == null || response.pgw.Count == 0)
                {
                    response.resp = new ApiObjects.Response.Status((int)eResponseStatus.OK, "no payment gateway related to group");
                }
                else
                {
                    response.resp = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
            }
            catch (Exception ex)
            {
                response = new PaymentGatewaySettingsResponse();
                response.resp = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.Error(string.Format("Failed groupID = {0} ", groupID), ex);
            }

            return response;
        }

        public virtual ApiObjects.Response.Status SetPaymentGWSettings(int paymentGWID, List<PaymentGatewaySettings> settings)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            try
            {
                if (paymentGWID == 0)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.PaymentGatewayIdRequired, PAYMENT_GATEWAY_ID_REQUIRED);
                }
                else
                {
                    bool isSet = DAL.BillingDAL.SetPaymentGWSettings(groupID, paymentGWID, settings);
                    if (isSet)
                    {
                        response = new ApiObjects.Response.Status((int)eResponseStatus.OK, "payment gateway set changes");
                    }
                    else
                    {
                        response = new ApiObjects.Response.Status((int)eResponseStatus.Error, "payment gateway failed set changes, check your params");
                    }
                }
            }
            catch (Exception ex)
            {
                response = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.Error(string.Format("Failed groupID = {0} , paymentGWID={1}", groupID, paymentGWID), ex);
            }
            return response;
        }

        public PaymentGatewayItemResponse SetPaymentGateway(int paymentGatewayId, PaymentGateway paymentGateway)
        {
            PaymentGatewayItemResponse response = new PaymentGatewayItemResponse();
            response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            try
            {
                if (paymentGatewayId == 0) // , name, URL, isDefault, isActive, configs)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.PaymentGatewayIdRequired, PAYMENT_GATEWAY_ID_REQUIRED);
                    return response;
                }
                if (string.IsNullOrEmpty(paymentGateway.Name))
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.PaymentGatewayNameRequired, INVALID_PAYMENT_GATEWAY_NAME);
                    return response;
                }

                if (string.IsNullOrEmpty(paymentGateway.SharedSecret))
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.PaymentGatewaySharedSecretRequired, INVALID_PAYMENT_GATEWAY_SHARED_SECRET);
                    return response;
                }

                if (paymentGateway.RenewalIntervalMinutes < MIN_RENEWAL_INTERVAL_MINUTES)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error,
                        string.Format("Renewal interval must be larger then {0} minutes", MIN_RENEWAL_INTERVAL_MINUTES));
                    return response;
                }

                if (string.IsNullOrEmpty(paymentGateway.ExternalIdentifier))
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.ExternalIdentifierRequired, EXTERNAL_IDENTIFIER_REQUIRED);
                    return response;
                }

                //check External Identifier uniqueness 
                int returnPaymentGatewayId = DAL.BillingDAL.GetPaymentGWInternalID(groupID, paymentGateway.ExternalIdentifier);

                if (returnPaymentGatewayId > 0 && paymentGatewayId != returnPaymentGatewayId)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.ExternalIdentifierMustBeUnique, ERROR_EXT_ID_ALREADY_IN_USE);
                    return response;
                }

                paymentGateway.ID = paymentGatewayId;

                // in case IsActive = false and the PaymentGateway is the group default: update is not allowed
                if (paymentGateway.IsActive == 0)
                {
                    // in case paymentGatewayId is the group selected paymentGateway  - delete isn’t allowed
                    //-------------------------------------------------------------------------------
                    object defaultPaymentGateway = ODBCWrapper.Utils.GetTableSingleVal("groups_parameters", "DEFAULT_PAYMENT_GATEWAY", "GROUP_ID", "=", groupID, "BILLING_CONNECTION_STRING");
                    int paymentGatewayIdentifier = 0;
                    if (defaultPaymentGateway != null && int.TryParse(defaultPaymentGateway.ToString(), out paymentGatewayIdentifier) && paymentGatewayIdentifier > 0)
                    {
                        if (paymentGatewayIdentifier == paymentGatewayId)
                        {
                            response.Status = new ApiObjects.Response.Status((int)eResponseStatus.ActionIsNotAllowed, ACTION_IS_NOT_ALLOWED);
                            return response;
                        }
                    }
                }

                bool isSet = DAL.BillingDAL.SetPaymentGateway(groupID, paymentGateway);
                if (isSet)
                {
                    if (!paymentGateway.SkipSettings)
                    {
                        isSet = DAL.BillingDAL.SetPaymentGWSettings(groupID, paymentGateway.ID, paymentGateway.Settings);
                        if (!isSet)
                        {
                            response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "payment gateway failed set changes, check your params");
                        }
                    }

                    if (!AdaptersController.GetInstance(paymentGateway.ID).SendConfiguration(paymentGateway, groupID))
                    {
                        log.DebugFormat("SetPaymentGateway - SendConfigurationToAdapter failed : AdapterID = {0}", paymentGatewayId);
                    }

                    paymentGateway = DAL.BillingDAL.GetPaymentGateway(groupID, paymentGatewayId, null);
                    if (paymentGateway == null || paymentGateway.ID <= 0)
                    {
                        response.Status = new ApiObjects.Response.Status((int)eResponseStatus.PaymentGatewayNotExist, ERROR_PAYMENT_GATEWAY_NOT_EXIST);
                        return response;
                    }
                    else
                    {
                        response.PaymentGateway = paymentGateway;
                        response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, "payment gateway set changes");
                    }
                }
                else
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "payment gateway failed set changes");
                }

            }
            catch (Exception ex)
            {
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.Error(string.Format("Failed groupID={0}, paymentGWID={1}, name={2}, adapterUrl={3}, transactUrl={4}, statusUrl={5}, renewUrl={6}, isDefault={7}, isActive={8}",
                    groupID, paymentGatewayId, paymentGateway.Name, paymentGateway.AdapterUrl, paymentGateway.TransactUrl, paymentGateway.StatusUrl, paymentGateway.RenewUrl, paymentGateway.IsDefault,
                    paymentGateway.IsActive), ex);
            }
            return response;
        }

        public virtual ApiObjects.Response.Status DeletePaymentGateway(int paymentGatewayId)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            try
            {
                if (paymentGatewayId == 0)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.PaymentGatewayIdRequired, PAYMENT_GATEWAY_ID_REQUIRED);
                    return response;
                }

                //check paymentGateway exist
                PaymentGateway paymentGateway = DAL.BillingDAL.GetPaymentGateway(groupID, paymentGatewayId, null);
                if (paymentGateway == null || paymentGateway.ID <= 0)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.PaymentGatewayNotExist, ERROR_PAYMENT_GATEWAY_NOT_EXIST);
                    return response;
                }

                bool isSet = DAL.BillingDAL.DeletePaymentGateway(groupID, paymentGatewayId);
                if (isSet)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.OK, "payment gateway deleted");
                }
                else
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.PaymentGatewayNotExist, ERROR_PAYMENT_GATEWAY_NOT_EXIST);
                }
            }
            catch (Exception ex)
            {
                response = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.Error(string.Format("Failed groupID={0}, paymentGWID={1}", groupID, paymentGatewayId), ex);
            }
            return response;
        }

        public virtual ApiObjects.Response.Status DeletePaymentGatewaySettings(int paymentGatewayId, List<PaymentGatewaySettings> settings)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            try
            {
                if (paymentGatewayId == 0)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.PaymentGatewayIdRequired, PAYMENT_GATEWAY_ID_REQUIRED);
                    return response;
                }

                if (settings == null || settings.Count == 0)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.PaymentGatewayParamsRequired, NO_PARAMS_TO_DELETE);
                    return response;
                }

                //check Payment Gateway exist
                PaymentGateway paymentGateway = DAL.BillingDAL.GetPaymentGateway(groupID, paymentGatewayId);
                if (paymentGateway == null || paymentGateway.ID <= 0)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.PaymentGatewayNotExist, ERROR_PAYMENT_GATEWAY_NOT_EXIST);
                    return response;
                }

                bool isSet = DAL.BillingDAL.DeletePaymentGW(groupID, paymentGatewayId, settings);
                if (isSet)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.OK, "payment gateway configs delete");
                }
                else
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.Error, "payment gateway configs faild delete");
                }
            }
            catch (Exception ex)
            {
                response = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.Error(string.Format("Failed groupID={0}, paymentGWID={1}", groupID, paymentGatewayId), ex);
            }
            return response;
        }

        public virtual PaymentGatewayResponse GetPaymentGateway()
        {
            PaymentGatewayResponse response = new PaymentGatewayResponse();
            try
            {
                response.pgw = DAL.BillingDAL.GetPaymentGWList(groupID);
                if (response.pgw == null || response.pgw.Count == 0)
                {
                    response.resp = new ApiObjects.Response.Status((int)eResponseStatus.OK, "no payment gateway related to group");
                }
                else
                {
                    response.resp = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
            }
            catch (Exception ex)
            {
                response = new PaymentGatewayResponse();
                response.resp = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.Error(string.Format("Failed groupID={0}", groupID), ex);
            }

            return response;
        }

        public virtual ApiObjects.Response.Status SetHouseholdPaymentGateway(int paymentGatewayId, string siteGuid, int householdId)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            try
            {
                // paymentGatewayId validation: not empty
                if (paymentGatewayId == 0)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.PaymentGatewayIdRequired, PAYMENT_GATEWAY_ID_REQUIRED);
                    return response;
                }

                // paymentGatewayId validation: paymentGateway exist and active

                PaymentGateway paymentGateway = DAL.BillingDAL.GetPaymentGateway(groupID, paymentGatewayId);
                if (paymentGateway == null || paymentGateway.Status != 1 || paymentGateway.IsActive != 1)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.PaymentGatewayNotValid, PAYMENT_GATEWAY_NOT_VALID);
                    return response;
                }
                // check for ENABLE_PAYMENT_GATEWAY_SELECTION- in case in disable return error
                object pgsettings = ODBCWrapper.Utils.GetTableSingleVal("groups_parameters", "ENABLE_PAYMENT_GATEWAY_SELECTION", "GROUP_ID", "=", groupID, "BILLING_CONNECTION_STRING");

                int enablePaymentGatewaySelection = 0;
                if (pgsettings != null && pgsettings != DBNull.Value)
                {
                    int.TryParse(pgsettings.ToString(), out enablePaymentGatewaySelection);
                }

                if (enablePaymentGatewaySelection == 0)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.PaymentGatewaySelectionIsDisabled, PAYMENT_GATEWAY_SELECTION_IS_DISABLED);
                    return response;
                }

                // check user
                ApiObjects.Response.Status userStatus = Utils.ValidateUserAndDomain(groupID, siteGuid, ref householdId);

                if (userStatus.Code == (int)ResponseStatus.OK && householdId > 0)
                {
                    HouseholdPaymentGateway householdPaymentGateway = DAL.BillingDAL.GetHouseholdPaymentGateway(groupID, paymentGatewayId, householdId, 1);

                    if (householdPaymentGateway != null && householdPaymentGateway.Selected == 1)
                    {
                        response = new ApiObjects.Response.Status((int)eResponseStatus.HouseholdAlreadySetToPaymentGateway, HOUSEHOLD_ALREADY_SET_TO_PAYMENT_GATEWAY);
                        return response;
                    }

                    bool isSet = DAL.BillingDAL.SetPaymentGatewayHousehold(groupID, paymentGatewayId, householdId, 1, null, 1);
                    if (isSet)
                    {
                        response = new ApiObjects.Response.Status((int)eResponseStatus.OK, "payment gateway set to household");
                    }
                    else
                    {
                        response = new ApiObjects.Response.Status((int)eResponseStatus.ErrorSavingPaymentGatewayHousehold, "payment gateway failed set to household");
                    }
                }
                else if (householdId == 0)
                {
                    response = userStatus;
                }
                else
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.Error, userStatus.Message.ToString());
                }

            }
            catch (Exception ex)
            {
                response = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.Error(string.Format("Failed groupID={0}, paymentGWID={1}, siteGuid= {2}", groupID, paymentGatewayId, siteGuid), ex);
            }
            return response;
        }

        public virtual PaymentGatewayListResponse GetHouseholdPaymentGateways(string siteGuid, int householdId)
        {
            PaymentGatewayListResponse response = new PaymentGatewayListResponse();
            try
            {
                // check user
                ApiObjects.Response.Status userStatus = Utils.ValidateUserAndDomain(groupID, siteGuid, ref householdId);

                if (userStatus.Code == (int)eResponseStatus.OK && householdId > 0)
                {
                    DataSet dsAllPaymentGateways = DAL.BillingDAL.GetAllPaymentGatewaysWithPaymentMethods(groupID, householdId);

                    if (dsAllPaymentGateways == null || dsAllPaymentGateways.Tables.Count == 0)
                    {
                        response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "No Payment Gateway found"); // TO DO : right message
                    }

                    if (dsAllPaymentGateways.Tables.Count > 0)
                    {
                        DataTable dtPaymentGateways = dsAllPaymentGateways.Tables[0];

                        Dictionary<int, List<PaymentMethod>> pms = new Dictionary<int, List<PaymentMethod>>();
                        Dictionary<int, List<HouseholdPaymentMethod>> hpms = new Dictionary<int, List<HouseholdPaymentMethod>>();

                        if (dsAllPaymentGateways.Tables.Count > 1)
                        {
                            DataTable dtPaymentGatewaysPaymentMethods = dsAllPaymentGateways.Tables[1];

                            foreach (DataRow dr in dtPaymentGatewaysPaymentMethods.Rows)
                            {
                                PaymentMethod pm = new PaymentMethod();

                                int pgid = ODBCWrapper.Utils.GetIntSafeVal(dr, "pg_id");

                                pm.ID = ODBCWrapper.Utils.GetIntSafeVal(dr, "ID");
                                pm.PaymentGatewayId = pgid;
                                pm.Name = ODBCWrapper.Utils.GetSafeStr(dr, "name");
                                pm.AllowMultiInstance = ODBCWrapper.Utils.GetIntSafeVal(dr, "allow_multi_instance") == 1;

                                if (!pms.ContainsKey(pgid))
                                {
                                    pms.Add(pgid, new List<PaymentMethod>());
                                }

                                pms[pgid].Add(pm);

                            }
                        }

                        if (dsAllPaymentGateways.Tables.Count > 2)
                        {
                            DataTable dtHouseholdPaymentMethods = dsAllPaymentGateways.Tables[2];

                            foreach (DataRow dr in dtHouseholdPaymentMethods.Rows)
                            {
                                HouseholdPaymentMethod hpm = new HouseholdPaymentMethod();

                                hpm.ID = ODBCWrapper.Utils.GetIntSafeVal(dr, "ID");
                                hpm.Details = ODBCWrapper.Utils.GetSafeStr(dr, "PAYMENT_DETAILS");
                                hpm.Selected = ODBCWrapper.Utils.GetIntSafeVal(dr, "SELECTED") == 1;
                                hpm.ExternalId = ODBCWrapper.Utils.GetSafeStr(dr, "PAYMENT_METHOD_EXTERNAL_ID");

                                int pmid = ODBCWrapper.Utils.GetIntSafeVal(dr, "PAYMENT_METHOD_ID");

                                if (!hpms.ContainsKey(pmid))
                                {
                                    hpms.Add(pmid, new List<HouseholdPaymentMethod>());
                                }

                                hpms[pmid].Add(hpm);
                            }
                        }

                        if (dtPaymentGateways != null && dtPaymentGateways.Rows.Count > 0)
                        {
                            PaymentGatewaySelectedBy pgsby = null;
                            foreach (DataRow dr in dtPaymentGateways.Rows)
                            {
                                pgsby = new PaymentGatewaySelectedBy();
                                pgsby.ID = ODBCWrapper.Utils.GetIntSafeVal(dr, "ID");
                                pgsby.Name = ODBCWrapper.Utils.GetSafeStr(dr, "name");
                                int household = ODBCWrapper.Utils.GetIntSafeVal(dr, "house_hold_id");
                                pgsby.By = ApiObjects.eHouseholdPaymentGatewaySelectedBy.None;

                                if (ODBCWrapper.Utils.GetIntSafeVal(dr, "is_default") == 1)
                                {
                                    pgsby.IsDefault = true;
                                    pgsby.By = ApiObjects.eHouseholdPaymentGatewaySelectedBy.Account;
                                }

                                if (household > 0)
                                {
                                    pgsby.IsDefault = ODBCWrapper.Utils.GetIntSafeVal(dr, "selected") == 1;
                                    pgsby.By = ApiObjects.eHouseholdPaymentGatewaySelectedBy.Household;
                                }

                                if (pms != null && pms.ContainsKey(pgsby.ID))
                                {
                                    pgsby.PaymentMethods = new List<PaymentGatwayPaymentMethods>();
                                    List<PaymentMethod> pmList = pms.Where(kvp => kvp.Key == pgsby.ID).SelectMany(kvp => kvp.Value).ToList<PaymentMethod>();
                                    pgsby.SupportPaymentMethod = true;

                                    foreach (PaymentMethod pm in pmList)
                                    {
                                        PaymentGatwayPaymentMethods pgpm = new PaymentGatwayPaymentMethods();
                                        pgpm.PaymentMethod = pm;

                                        List<HouseholdPaymentMethod> hpList = hpms.Where(kvp => kvp.Key == pm.ID).SelectMany(kvp => kvp.Value).ToList<HouseholdPaymentMethod>();
                                        if (hpList != null && hpList.Count > 0)
                                        {
                                            pgpm.HouseHoldPaymentMethods = new List<HouseholdPaymentMethod>();
                                            pgpm.HouseHoldPaymentMethods.AddRange(hpList);
                                        }
                                        pgsby.PaymentMethods.Add(pgpm);
                                    }
                                }

                                response.PaymentGateways.Add(pgsby);
                            }

                        }
                    }

                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());

                }
                else
                {
                    response.Status = userStatus;
                }

            }
            catch (Exception ex)
            {
                response = new PaymentGatewayListResponse();
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.Error(string.Format("Failed groupID={0}, siteGuid= {1}", groupID, siteGuid), ex);
            }

            return response;
        }

        public virtual PaymentGatewayItemResponse InsertPaymentGateway(PaymentGateway paymentGateway)
        {
            PaymentGatewayItemResponse response = new PaymentGatewayItemResponse();
            response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            try
            {
                if (paymentGateway == null)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.NoPaymentGatewayToInsert, NO_PAYMENT_GATEWAY_TO_INSERT);
                    return response;
                }

                if (string.IsNullOrEmpty(paymentGateway.Name))
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.PaymentGatewayNameRequired, INVALID_PAYMENT_GATEWAY_NAME);
                    return response;
                }

                if (string.IsNullOrEmpty(paymentGateway.SharedSecret))
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.PaymentGatewaySharedSecretRequired, INVALID_PAYMENT_GATEWAY_SHARED_SECRET);
                    return response;
                }

                if (paymentGateway.RenewalIntervalMinutes < MIN_RENEWAL_INTERVAL_MINUTES)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error,
                        string.Format("Renewal interval must be larger then {0} minutes", MIN_RENEWAL_INTERVAL_MINUTES));
                    return response;
                }

                if (string.IsNullOrEmpty(paymentGateway.ExternalIdentifier))
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.ExternalIdentifierRequired, EXTERNAL_IDENTIFIER_REQUIRED);
                    return response;
                }

                //check External Identiifer uniqueness 
                int paymentGWID = DAL.BillingDAL.GetPaymentGWInternalID(groupID, paymentGateway.ExternalIdentifier);

                if (paymentGWID > 0)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.ExternalIdentifierMustBeUnique, ERROR_EXT_ID_ALREADY_IN_USE);
                    return response;
                }

                PaymentGateway createdPaymentGateway = DAL.BillingDAL.InsertPaymentGW(groupID, paymentGateway);
                if (createdPaymentGateway != null)
                {
                    response.PaymentGateway = createdPaymentGateway;
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, "new payment gateway insert");
                }
                else
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "fail to insert new payment gateway");
                }
            }
            catch (Exception ex)
            {
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.Error(string.Format("Failed groupID={0}", groupID), ex);
            }
            return response;
        }

        public virtual PaymentGatewayItemResponse InsertPaymentGatewaySettings(int paymentGatewayId, List<PaymentGatewaySettings> settings)
        {
            PaymentGatewayItemResponse response = new PaymentGatewayItemResponse();
            response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            try
            {
                if (paymentGatewayId == 0)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.PaymentGatewayIdRequired, PAYMENT_GATEWAY_ID_REQUIRED);
                    return response;
                }
                if (settings == null || settings.Count == 0)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.PaymentGatewayParamsRequired, NO_PARAMS_TO_INSERT);
                    return response;
                }

                //check Payment Gateway exist
                PaymentGateway paymentGateway = DAL.BillingDAL.GetPaymentGateway(groupID, paymentGatewayId);
                if (paymentGateway == null || paymentGateway.ID <= 0)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.PaymentGatewayNotExist, ERROR_PAYMENT_GATEWAY_NOT_EXIST);
                    return response;
                }

                bool isInsert = DAL.BillingDAL.InsertPaymentGatewaySettings(groupID, paymentGatewayId, settings);
                if (isInsert)
                {
                    paymentGateway = DAL.BillingDAL.GetPaymentGateway(groupID, paymentGatewayId);
                    if (paymentGateway == null || paymentGateway.ID <= 0)
                    {
                        response.Status = new ApiObjects.Response.Status((int)eResponseStatus.PaymentGatewayNotExist, ERROR_PAYMENT_GATEWAY_NOT_EXIST);
                        return response;
                    }
                    else
                    {
                        response.PaymentGateway = paymentGateway;
                        response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, "payment gateway configs insert");
                    }
                }
                else
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "fail to insert payment gateway configs");
                }

            }
            catch (Exception ex)
            {
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.Error(string.Format("Failed groupID={0}, paymentGWID={1}", groupID, paymentGatewayId), ex);
            }
            return response;
        }

        public ApiObjects.Response.Status DeleteHouseholdPaymentGateway(int paymentGatewayId, string siteGuid, int householdId)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            try
            {
                if (paymentGatewayId == 0)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.PaymentGatewayIdRequired, PAYMENT_GATEWAY_ID_REQUIRED);
                    return response;
                }

                // check user
                ApiObjects.Response.Status userStatus = Utils.ValidateUserAndDomain(groupID, siteGuid, ref householdId);

                if (userStatus.Code != (int)ResponseStatus.OK)
                {
                    return userStatus;
                }

                //check if paymentGatewayId exist
                PaymentGateway paymentGateway = DAL.BillingDAL.GetPaymentGateway(groupID, paymentGatewayId);
                if (paymentGateway == null)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.PaymentGatewayNotExist, ERROR_PAYMENT_GATEWAY_NOT_EXIST);
                    return response;
                }
                //check if householed reltaed to PaymentGateway
                HouseholdPaymentGateway householdPaymentGateway = DAL.BillingDAL.GetHouseholdPaymentGateway(groupID, paymentGatewayId, householdId);
                if (householdPaymentGateway == null)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.HouseholdNotSetToPaymentGateway, HOUSEHOLD_NOT_SET_TO_PAYMENT_GATEWAY);
                    return response;
                }

                object pgsettings = ODBCWrapper.Utils.GetTableSingleVal("groups_parameters", "ENABLE_PAYMENT_GATEWAY_SELECTION", "GROUP_ID", "=", groupID, "BILLING_CONNECTION_STRING");

                int enablePaymentGatewaySelection = 0;
                if (pgsettings != null && pgsettings != DBNull.Value)
                {
                    int.TryParse(pgsettings.ToString(), out enablePaymentGatewaySelection);
                }

                if (enablePaymentGatewaySelection == 0)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.PaymentGatewaySelectionIsDisabled, PAYMENT_GATEWAY_SELECTION_IS_DISABLED);
                    return response;
                }

                if (userStatus.Code == (int)ResponseStatus.OK && householdId > 0)
                {
                    bool isSet = DAL.BillingDAL.DeletePaymentGatewayHousehold(groupID, paymentGatewayId, householdId);
                    if (isSet)
                    {
                        response = new ApiObjects.Response.Status((int)eResponseStatus.OK, "payment gateway delete from household");
                    }
                    else
                    {
                        response = new ApiObjects.Response.Status((int)eResponseStatus.Error, "payment gateway faild to delete from household");
                    }
                }
                else
                {
                    response = userStatus;
                }

            }
            catch (Exception ex)
            {
                response = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.Error(string.Format("Failed groupID={0}, paymentGWID={1}, siteGuid={2}", groupID, paymentGatewayId, siteGuid), ex);
            }
            return response;
        }

        public virtual ApiObjects.Response.Status SetHouseholdChargeID(string externalIdentifier, int householdId, string chargeID)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());

            try
            {
                if (string.IsNullOrEmpty(externalIdentifier))
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.ExternalIdentifierRequired, EXTERNAL_IDENTIFIER_REQUIRED);
                    return response;
                }

                if (string.IsNullOrEmpty(chargeID))
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.PaymentGatewayChargeIdRequired, ERROR_CHARGE_ID_MISSING);
                    return response;
                }

                // check domain
                response = Utils.ValidateDomain(groupID, householdId);

                if (response.Code == (int)ResponseStatus.OK)
                {
                    int paymentGatewayId = DAL.BillingDAL.GetPaymentGWInternalID(groupID, externalIdentifier);

                    if (paymentGatewayId > 0)
                    {
                        HouseholdPaymentGateway householdPaymentGateway = DAL.BillingDAL.GetHouseholdPaymentGateway(groupID, paymentGatewayId, householdId);

                        if (householdPaymentGateway != null && householdPaymentGateway.ChargeId == chargeID)
                        {
                            response = new ApiObjects.Response.Status((int)eResponseStatus.ChargeIdAlreadySetToHouseholdPaymentGateway, CHARGE_ID_ALREADY_SET_TO_HOUSEHOLD);
                            return response;

                        }
                        bool isSet = DAL.BillingDAL.SetPaymentGatewayHousehold(groupID, paymentGatewayId, householdId, null, chargeID);

                        if (isSet)
                        {
                            response = new ApiObjects.Response.Status((int)eResponseStatus.OK, "OK");
                        }
                        else
                        {
                            response = new ApiObjects.Response.Status((int)eResponseStatus.ErrorSavingPaymentGatewayHousehold, ERROR_SAVING_PAYMENT_GATEWAY_HOUSEHOLD);
                        }

                    }
                    else
                    {
                        response = new ApiObjects.Response.Status((int)eResponseStatus.PaymentGatewayNotExist, ERROR_PAYMENT_GATEWAY_NOT_EXIST);

                    }
                }
            }
            catch (Exception ex)
            {
                response = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.Error(string.Format("Failed groupID={0}, externalIdentifier={1}, householdID={2}", groupID, externalIdentifier, householdId), ex);
            }
            return response;
        }

        public virtual PaymentGatewayChargeIDResponse GetHouseholdChargeID(string externalIdentifier, int householdID)
        {

            PaymentGatewayChargeIDResponse response = new PaymentGatewayChargeIDResponse();
            try
            {
                if (string.IsNullOrEmpty(externalIdentifier))
                {
                    response.ResponseStatus = new ApiObjects.Response.Status((int)eResponseStatus.ExternalIdentifierRequired, EXTERNAL_IDENTIFIER_REQUIRED);
                    return response;
                }
                // check domain
                response.ResponseStatus = Utils.ValidateDomain(groupID, householdID);

                if (response.ResponseStatus.Code == (int)ResponseStatus.OK)
                {
                    int paymentGWID = DAL.BillingDAL.GetPaymentGWInternalID(groupID, externalIdentifier);

                    if (paymentGWID > 0)
                    {
                        response.ChargeID = DAL.BillingDAL.GetPaymentGWChargeID(paymentGWID, householdID);

                        if (string.IsNullOrEmpty(response.ChargeID))
                        {
                            response.ResponseStatus = new ApiObjects.Response.Status((int)eResponseStatus.ChargeIdNotSetToHousehold, CHARGE_ID_NOT_SET);
                        }
                        else
                        {
                            response.ResponseStatus = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                        }
                    }
                    else
                    {
                        response.ResponseStatus = new ApiObjects.Response.Status((int)eResponseStatus.PaymentGatewayNotExist, ERROR_PAYMENT_GATEWAY_NOT_EXIST);
                    }
                }

            }
            catch (Exception ex)
            {
                response = new PaymentGatewayChargeIDResponse();
                response.ResponseStatus = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.Error(string.Format("Failed groupID={0}, externalIdentifier={1}, householdID={2}", groupID, externalIdentifier, householdID), ex);
            }

            return response;
        }

        #endregion


        public virtual TransactResult Transact(string siteGuid, long householdID, double price, string currency, string userIP, string customData,
            int productID, eTransactionType productType, int contentID, string billingGuid, int paymentGatewayId, int paymentGatewayHHPaymentMethodId, string adapterData)
        {
            TransactResult response = new TransactResult();
            PaymentGateway paymentGateway = null;
            PaymentGatewayHouseholdPaymentMethod pghpm = null;
            string chargeId = string.Empty;
            string paymentMethodExternalId = string.Empty;
            int paymentNumber = 1;

            try
            {
                // Get Oss Adapter default payment gateway
                paymentGateway = GetOSSAdapterPaymentGateway(groupID, householdID, userIP, out chargeId);

                // in case OSS Adapter not set.
                if (paymentGateway == null)
                {
                    if (paymentGatewayId == 0)
                    {
                        // get selected household payment gateway
                        paymentGateway = DAL.BillingDAL.GetSelectedHouseholdPaymentGateway(groupID, householdID, ref chargeId);
                        if (paymentGateway == null)
                        {
                            response.Status = new ApiObjects.Response.Status((int)eResponseStatus.PaymentGatewayNotSetForHousehold, ERROR_NO_PGW_RELATED_TO_HOUSEHOLD);
                            return response;
                        }
                        else
                        {
                            paymentGatewayId = paymentGateway.ID;
                            log.DebugFormat("Transact using Selected HH PG Id: {0}", paymentGatewayId);
                        }
                    }
                    else
                    {
                        //get paymentGateway according to input parameter
                        paymentGateway = DAL.BillingDAL.GetPaymentGateway(groupID, paymentGatewayId, 1, 1);

                        //in case paymentGateway not valid
                        if (paymentGateway == null)
                        {
                            response.Status = new ApiObjects.Response.Status((int)eResponseStatus.PaymentGatewayNotExist, ERROR_PAYMENT_GATEWAY_NOT_EXIST);
                            return response;
                        }

                        bool isPaymentGWHouseholdExist = false;
                        chargeId = DAL.BillingDAL.GetPaymentGWChargeID(paymentGatewayId, householdID, ref isPaymentGWHouseholdExist);

                        if (!isPaymentGWHouseholdExist)
                        {
                            response.Status = new ApiObjects.Response.Status((int)eResponseStatus.PaymentGatewayNotSetForHousehold, ERROR_NO_PGW_RELATED_TO_HOUSEHOLD);
                            return response;
                        }
                    }
                }
                else
                {
                    paymentGatewayId = paymentGateway.ID;
                    log.DebugFormat("Transact using oss adapter payment gateway Id: {0}", paymentGatewayId);
                }

                if (string.IsNullOrEmpty(chargeId))
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.PaymentGatewayChargeIdRequired, ERROR_CHARGE_ID_MISSING);
                    return response;
                }

                // Handle  Payment Method
                //------------------------
                ApiObjects.Response.Status pghpmStatus = null;
                if (paymentGateway.SupportPaymentMethod)
                {
                    pghpmStatus = GetHouseholdPaymentGatewayPaymentMethod(householdID, paymentGatewayId, paymentGatewayHHPaymentMethodId, out pghpm);

                    if (pghpmStatus.Code != (int)eResponseStatus.OK)
                    {
                        response.Status = pghpmStatus;
                        return response;
                    }
                    if (pghpm != null)
                    {
                        paymentGatewayHHPaymentMethodId = pghpm.Id;
                        paymentMethodExternalId = pghpm.PaymentMethodExternalId;
                    }
                }

                response = SendPaymentToAdapter(chargeId, price, currency, userIP, productID, productType, contentID, siteGuid, householdID, billingGuid,
                    paymentGateway, customData, paymentNumber, paymentMethodExternalId, paymentGatewayHHPaymentMethodId, adapterData);

                bool purchaseMail = true;
                bool failPurchaseMail = true;
                DAL.BillingDAL.GetPurchaseMailTriggerAccountSettings(groupID, ref purchaseMail, ref failPurchaseMail);
                SendMail(siteGuid, price, currency, customData, productID, productType, contentID, response, paymentGatewayId, purchaseMail, failPurchaseMail);

                log.DebugFormat("Account purchase mail settings purchaseMail = {0} failPurchaseMail = {1}, groupID = {2}", purchaseMail, failPurchaseMail, groupID);
            }
            catch (Exception ex)
            {
                response = new TransactResult();
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.ErrorFormat("Failed ex={0}, siteGUID={1}, price={2}, currency={3}, customData={4}, productID={5}, transactionType={6}, billingGuid={7}", ex, siteGuid,
                    price, currency, customData, productID, productType, billingGuid);
            }

            return response;
        }

        public void SendMail(string siteGuid, double price, string currency, string customData, int productID, eTransactionType productType, int contentID, TransactResult response, int paymentGatewayId,
            bool successMail = true, bool failMail = true)
        {
            try
            {
                if (response != null && response.Status.Code == (int)eResponseStatus.OK)
                {
                    string ItemName = string.Empty;
                    string PreivewEnd = string.Empty;

                    int SendMail = ODBCWrapper.Utils.GetIntSafeVal(ODBCWrapper.Utils.GetTableSingleVal("payment_gateway", "send_mail", paymentGatewayId, "BILLING_CONNECTION_STRING").ToString());

                    if (SendMail == 0)
                    {
                        log.DebugFormat("SendMail for payment_gateway id {0} is false - can't send mail", paymentGatewayId);
                        return;
                    }
                    GetDetailsFromCustomData(customData, ref PreivewEnd, ref price, ref currency);
                    ItemName = GetItemName(productType, contentID, productID);

                    switch (response.State)
                    {
                        case eTransactionState.OK:
                        case eTransactionState.Pending:
                            if (successMail)
                            {
                                Utils.SendMail(response.PaymentMethod, ItemName, siteGuid, response.TransactionID, price.ToString(), currency, response.PGReferenceID, groupID, response.PaymentDetails, PreivewEnd, eMailTemplateType.Purchase);
                            }
                            break;
                        case eTransactionState.Failed:
                            if (failMail)
                            {
                                Utils.SendMail(response.PaymentMethod, ItemName, siteGuid, response.TransactionID, price.ToString(), currency, response.PGReferenceID, groupID, response.PaymentDetails, PreivewEnd, eMailTemplateType.PaymentFail);
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Failed SendMail ex={0}, siteGUID={1}, price={2}, currency={3}, customData={4}, productID={5}, transactionType={6}", ex, siteGuid,
                    price, currency, customData, productID, productType);
            }
        }

        private void GetDetailsFromCustomData(string customData, ref string PreivewEnd, ref double price, ref string currency)
        {
            try
            {
                System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
                doc.LoadXml(customData);
                System.Xml.XmlNode theRequest = doc.FirstChild;
                PreivewEnd = TVinciShared.XmlUtils.GetSafeValue("prevlc", ref theRequest);
                if (price == 0)
                {
                    string spri = Utils.GetSafeValue("pri", ref theRequest);
                    if (string.IsNullOrEmpty(spri))
                    {
                        price = double.Parse(spri);
                    }
                }
                if (string.IsNullOrEmpty(currency))
                {
                    currency = Utils.GetSafeValue("cu", ref theRequest);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("GetDetailsFromCustomData ex = {0}, customData = {1}, PreivewEnd = {2}, price = {2}, currency = {3}", ex.Message, customData, PreivewEnd, price, currency);
            }
        }

        private string GetItemName(eTransactionType productType, int contentID, int productID)
        {
            string ItemName = string.Empty;
            try
            {
                switch (productType)
                {
                    case eTransactionType.PPV:
                        ItemName = ODBCWrapper.Utils.GetTableSingleVal("media", "name", contentID, "MAIN_CONNECTION_STRING").ToString();
                        break;
                    case eTransactionType.Subscription:
                        ItemName = ODBCWrapper.Utils.GetTableSingleVal("subscriptions", "name", productID, "pricing_connection").ToString();

                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("GetItemName ex = {0}, productType = {1}, contentID = {2}, productID = {2}", ex.Message, productType.ToString(), contentID, productID);
            }
            return ItemName;
        }

        private ApiObjects.Response.Status GetHouseholdPaymentGatewayPaymentMethod(long householdID, int paymentGatewayId, int paymentGatewayHHPaymentMethodId, out PaymentGatewayHouseholdPaymentMethod pghpm)
        {
            ApiObjects.Response.Status status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            pghpm = null;

            if (paymentGatewayHHPaymentMethodId == 0)
            {
                pghpm = DAL.BillingDAL.GetSelectedHouseholdPaymentGatewayPaymentMethod(groupID, householdID, paymentGatewayId);
                if (pghpm == null || pghpm.Id <= 0)
                {
                    log.ErrorFormat("GetSelectedHouseholdPaymentGatewayPaymentMethod no payment method related to payment gateway {0} ", paymentGatewayId);
                    return new ApiObjects.Response.Status() { Code = (int)eResponseStatus.PaymentMethodNotSetForHousehold, Message = ERROR_NO_PG_PM_RELATED_TO_HOUSEHOLD };
                }
            }
            else
            {
                pghpm = DAL.BillingDAL.GetPaymentGatewayHouseholdPaymentMethod(groupID, paymentGatewayId, householdID, paymentGatewayHHPaymentMethodId);
                if (pghpm == null || pghpm.Id <= 0)
                {
                    log.ErrorFormat("GetPaymentGatewayHouseholdPaymentMethod Payment Method Not Exist {0} ", paymentGatewayHHPaymentMethodId);
                    return new ApiObjects.Response.Status() { Code = (int)eResponseStatus.PaymentMethodNotExist, Message = PAYMENT_METHOD_NOT_EXIST };
                }
            }

            return status;
        }

        private PaymentGateway GetOSSAdapterPaymentGateway(int groupID, long householdID, string userIP, out string chargeId)
        {
            PaymentGateway ossPaymentGateway = null;
            int ossAdapterIdentifier = 0;
            int paymentGatewayId = 0;
            chargeId = string.Empty;

            //if configured : get Oss Adapter
            if (!TvinciCache.WSCache.Instance.TryGet<int>("OSS_ADAPTER_" + groupID, out ossAdapterIdentifier))
            {
                var ossAdapterId = ODBCWrapper.Utils.GetTableSingleVal("groups_parameters", "OSS_ADAPTER", "GROUP_ID", "=", groupID, "BILLING_CONNECTION_STRING");

                if (ossAdapterId != null && ossAdapterId != DBNull.Value && int.TryParse(ossAdapterId.ToString(), out ossAdapterIdentifier))
                {
                    TvinciCache.WSCache.Instance.Add("OSS_ADAPTER_" + groupID, ossAdapterIdentifier, MIN_OSS_CACHE_TIME);
                }
            }

            //in case ossAdapter exist: Get billing details            
            if (ossAdapterIdentifier > 0)
            {
                log.DebugFormat("GetOSSAdapterPaymentGateway  ossAdapterIdentifier= {0}", ossAdapterIdentifier);
                OSSAdapterBillingDetailsResponse ossAdapterResponse = GetOSSAdapterBillingDetails(householdID, ossAdapterIdentifier, userIP);

                if (ossAdapterResponse != null && ossAdapterResponse.Status.Code == (int)eResponseStatus.OK
                      && !string.IsNullOrEmpty(ossAdapterResponse.PaymentGatewayId)
                      && !string.IsNullOrEmpty(ossAdapterResponse.ChargeId))
                {
                    // Get paymentGateway
                    log.DebugFormat("GetOSSAdapterPaymentGateway  ossAdapter PaymentGatewayId= {0}", ossAdapterResponse.PaymentGatewayId);
                    paymentGatewayId = DAL.BillingDAL.GetPaymentGWInternalID(groupID, ossAdapterResponse.PaymentGatewayId);
                    log.DebugFormat("GetOSSAdapterPaymentGateway  PaymentGatewayId= {0}", paymentGatewayId);

                    if (paymentGatewayId > 0)
                    {
                        ossPaymentGateway = DAL.BillingDAL.GetPaymentGateway(groupID, paymentGatewayId, 1, 1);
                        if (ossPaymentGateway != null && ossPaymentGateway.ID > 0)
                        {
                            chargeId = ossAdapterResponse.ChargeId;
                            log.DebugFormat("GetOSSAdapterPaymentGateway  ChargeId= {0}", ossAdapterResponse.ChargeId);
                        }
                        else
                        {
                            log.DebugFormat("GetOSSAdapterPaymentGateway  ossPaymentGateway not valid {0}", paymentGatewayId);
                        }
                    }
                }
            }

            return ossPaymentGateway;
        }

        private OSSAdapterBillingDetailsResponse GetOSSAdapterBillingDetails(long householdID, int ossAdapterIdentifier, string userIP)
        {
            OSSAdapterBillingDetailsResponse ossAdapterResponse = null;
            try
            {
                // call new billing method for charge adapter
                ossAdapterResponse = Api.Module.GetUserBillingDetails(groupID, householdID, ossAdapterIdentifier, userIP);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Failed GetOSSAdapterBillingDetails ex={0}, householdID={1}, ossAdapterIdentifier={2}", ex, householdID, ossAdapterIdentifier);
            }

            return ossAdapterResponse;
        }


        public virtual TransactResult ProcessRenewal(string siteguid, long householdId, double price, string currency, string customData, int productId, string productCode,
                                                     int paymentNumber, int numberOfPayments, string billingGuid, int gracePeriodMinutes)
        {
            TransactResult transactionResponse = new TransactResult();
            PaymentGateway paymentGateway = null;
            PaymentGateway ossPaymentGateway = null;
            PaymentGatewayHouseholdPaymentMethod pghpm = null;
            bool isPaymentGatewayHouseholdExist = false;
            string chargeId = string.Empty;
            bool isOssValid = false;
            string paymentMethodExternalId = string.Empty;

            try
            {
                // get payment GW ID and external transaction ID
                List<PaymentDetails> PaymentDetails = GetPaymentDetails(new List<string>() { billingGuid });
                
                PaymentDetails pd = PaymentDetails != null ? PaymentDetails.Where(x => x.BillingGuid == billingGuid).FirstOrDefault() : null;

                if (pd == null)
                {
                    // error while trying to get payment GW ID and external transaction ID
                    log.ErrorFormat("error while getting payment GW ID and external transaction ID. groupID: {0}, householdId: {1), billingGuid: {2}", groupID, householdId, billingGuid);
                    transactionResponse.Status = new ApiObjects.Response.Status((int)eResponseStatus.PaymentGatewayNotExist, ERROR_PAYMENT_GATEWAY_NOT_EXIST);
                    return transactionResponse;
                }
                
                string externalTransactionId = pd.TransactionId;
                int paymentGatewayId = pd.PaymentGatewayId;
                int paymentMethodId = pd.PaymentMethodId;

                log.DebugFormat("successfully received payment GW ID and external transaction ID. paymentGatewayId: {0}, externalTransactionId: {1}", paymentGatewayId, externalTransactionId);

                // there are 2 types of payment Gateway
                // external ( verification) payment Gateway and "normal"
                // incase of verification payment Gateway there is no need to look for the most update and relevant payment gateway
                // the renewal will use only the verification payment Gateway                
                paymentGateway = DAL.BillingDAL.GetPaymentGateway(groupID, paymentGatewayId, 1, 1);
                if (paymentGateway == null || paymentGateway.ID <= 0)
                {
                    ossPaymentGateway = GetOSSAdapterPaymentGateway(groupID, householdId, string.Empty, out chargeId);

                    if (ossPaymentGateway == null || ossPaymentGateway.ID <= 0)
                    {
                        transactionResponse.Status = new ApiObjects.Response.Status((int)eResponseStatus.PaymentGatewayNotExist, ERROR_PAYMENT_GATEWAY_NOT_EXIST);
                        return transactionResponse;
                    }
                    else
                    {
                        isOssValid = true;
                        paymentGateway = ossPaymentGateway;
                        paymentGatewayId = ossPaymentGateway.ID;
                    }
                }

                bool isVerification = IsVerificationPaymentGateway(paymentGateway);

                if (!isVerification && !isOssValid)
                {
                    // get charge ID
                    chargeId = DAL.BillingDAL.GetPaymentGWChargeID(paymentGatewayId, householdId, ref isPaymentGatewayHouseholdExist);

                    // Handle  Payment Method
                    //------------------------
                    if (paymentGateway.SupportPaymentMethod)
                    {
                        ApiObjects.Response.Status pghpmStatus = null;
                        pghpmStatus = GetHouseholdPaymentGatewayPaymentMethod(householdId, paymentGatewayId, paymentMethodId, out pghpm);

                        if (pghpmStatus.Code != (int)eResponseStatus.OK)
                        {
                            transactionResponse.Status = pghpmStatus;
                            return transactionResponse;
                        }

                        paymentMethodExternalId = pghpm.PaymentMethodExternalId;
                    }
                }

                log.DebugFormat("paymentGatewayId: {0}, householdId: {1}, isPaymentGWHouseholdExist: {2}, chargeID: {3}",
                    paymentGatewayId,                                               //{0}
                    householdId,                                                    //{1}
                    isPaymentGatewayHouseholdExist,                                 //{2}    
                    !string.IsNullOrEmpty(chargeId) ? chargeId : string.Empty);    //{3}

                transactionResponse = SendRenewalRequestToAdapter(chargeId, price, currency, productId, productCode, paymentNumber, numberOfPayments, siteguid,
                    householdId, billingGuid, paymentGateway, customData, externalTransactionId, gracePeriodMinutes, paymentMethodExternalId, paymentMethodId);

                // check if account trigger settings for send purchase mail is true 
                bool renewMail = true;
                bool failRenewMail = true;
                DAL.BillingDAL.GetRenewMailTriggerAccountSettings(groupID, ref renewMail, ref failRenewMail);
                SendMail(siteguid, price, currency, customData, productId, eTransactionType.Subscription, 0, transactionResponse, paymentGatewayId, renewMail, failRenewMail);
                log.DebugFormat("Account Renew mail settings renewMail = {0} failRenewMail = {1}, groupID = {2}", renewMail, failRenewMail, groupID);
            }
            catch (Exception ex)
            {
                transactionResponse = new TransactResult();
                transactionResponse.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.ErrorFormat("Failed ex={0}, siteGUID={1}, price={2}, currency={3}, customData={4}, householdId={5}, productId={6}, billingGuid={7}," +
                    " productCode={8}, paymentNumber={9}, numberOfPayments={10}, gracePeriodMinutes={11}",
                    ex,                                                                       // {0}
                    !string.IsNullOrEmpty(siteguid) ? siteguid : string.Empty,                // {1}
                    price,                                                                    // {2}
                    !string.IsNullOrEmpty(currency) ? currency : string.Empty,                // {3}
                    !string.IsNullOrEmpty(customData) ? customData : string.Empty,            // {4}
                    householdId,                                                              // {5}
                    productId,                                                                // {6}
                    !string.IsNullOrEmpty(billingGuid) ? billingGuid : string.Empty,          // {7}
                    !string.IsNullOrEmpty(productCode) ? productCode : string.Empty,          // {8}
                    paymentNumber,                                                            // {9}
                    numberOfPayments,                                                         // {10}
                    gracePeriodMinutes);                                                      // {11}
            }
            return transactionResponse;
        }

        private bool IsVerificationPaymentGateway(PaymentGateway paymentGateway)
        {
            bool isVerification = false;

            object verificationPaymentGateway = ODBCWrapper.Utils.GetTableSingleVal("verification_payment_gateway", "DESCRIPTION", "DESCRIPTION", "=", paymentGateway.Name, "BILLING_CONNECTION_STRING");
            isVerification = (verificationPaymentGateway != null && !string.IsNullOrEmpty(verificationPaymentGateway.ToString()));

            return isVerification;
        }

        private TransactResult SendRenewalRequestToAdapter(string chargeId, double price, string currency, int productId, string productCode, int paymentNumber,
                                                           int numberOfPayments, string siteguid, long householdId, string billingGuid, PaymentGateway paymentGateway,
                                                           string customData, string externalTransactionId, int gracePeriodMinutes, string paymentMethodExternalId, int paymenMethodId)
        {
            TransactResult response = new TransactResult();

            string logString = string.Format("chargeId: {0}, price: {1}, currency: {2}, productId: {3}, productCode: {4}, paymentNumber: {5}, numberOfPayments: {6}, siteguid: {7}, household: {8} " +
                "billingGuid {9}, customData: {10}, externalTransactionId={11}, gracePeriodMinutes={12}",
                chargeId != null ? chargeId : string.Empty,                             // {0}
                price,                                                                  // {1}
                currency != null ? currency : string.Empty,                             // {2}
                productId,                                                              // {3}
                productCode != null ? productCode : string.Empty,                       // {4}
                paymentNumber,                                                          // {5}
                numberOfPayments,                                                       // {6}
                siteguid != null ? siteguid : string.Empty,                             // {7}
                householdId,                                                            // {8}
                billingGuid != null ? billingGuid : string.Empty,                       // {9}
                customData != null ? customData : string.Empty,                         // {10}
                externalTransactionId != null ? externalTransactionId : string.Empty,   // {11}
                gracePeriodMinutes,
                paymentMethodExternalId != null ? paymentMethodExternalId : string.Empty);                                                    // {12,13}

            // validate user
            long userId = 0;
            if (!long.TryParse(siteguid, out userId))
            {
                // User validation failed
                log.ErrorFormat("User validation failed. data: {0}", logString);
                response.Status = new ApiObjects.Response.Status() { Code = (int)eResponseStatus.InvalidUser, Message = "Invalid User" };
                return response;
            }

            // build request
            if (!string.IsNullOrEmpty(paymentGateway.AdapterUrl))
            {
                TransactionRenewalRequest request = new TransactionRenewalRequest()
                {
                    groupId = groupID,
                    billingGuid = billingGuid,
                    chargeId = chargeId,
                    paymentMethodExternalId = paymentMethodExternalId,
                    currency = currency,
                    customData = customData,
                    householdID = householdId,
                    paymentGateway = paymentGateway,
                    price = price,
                    productId = productId,
                    productType = eTransactionType.Subscription,
                    siteGuid = siteguid,
                    productCode = productCode,
                    ExternalTransactionId = externalTransactionId,
                    GracePeriodMinutes = gracePeriodMinutes
                };

                // fire request
                APILogic.PaymentGWAdapter.TransactionResponse adapterResponse = AdaptersController.GetInstance(paymentGateway.ID).ProcessRenewal(request);
                response = ValidateAdapterResponse(adapterResponse, logString);
                if (response == null)
                {
                    log.Error("Error received while trying to process renewal");
                    response = new TransactResult()
                    {
                        Status = new ApiObjects.Response.Status()
                        {
                            Code = (int)eResponseStatus.Error,
                            Message = "Error validating adapter response"
                        }
                    };
                }
                // validation OK -> continue
                // validation NOT OK -> validation response is final response
                else if (response.Status.Code == (int)eResponseStatus.OK)
                {
                    if (adapterResponse.Status.Code == (int)PaymentGatewayAdapterStatus.OK)
                    {
                        switch (adapterResponse.Transaction.StateCode)
                        {
                            case (int)eTransactionState.OK:

                                // renewal passed - create transaction
                                log.Debug("process renewal passed - create transaction");

                                int paymentGatewayTransactionId = CreateTransaction(ref response, adapterResponse, productId, eTransactionType.Subscription, billingGuid,
                                    0, paymentGateway.ID, householdId, userId, customData, BILLING_TRANSACTION_SUCCESS_STATUS, paymentNumber, paymenMethodId);

                                if (paymentGatewayTransactionId > 0 &&
                                    response != null &&
                                    response.Status.Code != (int)eResponseStatus.OK)
                                {
                                    // error saving transaction
                                    log.ErrorFormat("Error creating transaction (adapter code success). log string: {0}", logString);
                                }
                                else
                                {
                                    // renewal passed
                                    log.Debug("process renewal passed - transaction created");
                                }

                                break;

                            case (int)eTransactionState.Pending:

                                // process renewal returned "PENDING"
                                log.InfoFormat("PG return pending renewal request. data: {0}", logString);

                                //set return response values
                                response.PGResponseID = !string.IsNullOrEmpty(adapterResponse.Transaction.PGStatus) ? adapterResponse.Transaction.PGStatus : string.Empty;
                                response.PGReferenceID = !string.IsNullOrEmpty(adapterResponse.Transaction.PGTransactionID) ? adapterResponse.Transaction.PGTransactionID : string.Empty;
                                response.State = (eTransactionState)adapterResponse.Transaction.StateCode;
                                response.FailReasonCode = adapterResponse.Transaction.FailReasonCode;
                                response.PaymentDetails = !string.IsNullOrEmpty(adapterResponse.Transaction.PaymentDetails) ? adapterResponse.Transaction.PaymentDetails : string.Empty;
                                response.PaymentMethod = !string.IsNullOrEmpty(adapterResponse.Transaction.PaymentMethod) ? adapterResponse.Transaction.PaymentMethod : string.Empty;
                                response.StartDateSeconds = adapterResponse.Transaction.StartDateSeconds;
                                response.EndDateSeconds = adapterResponse.Transaction.EndDateSeconds;
                                response.AutoRenewing = adapterResponse.Transaction.AutoRenewing;

                                break;

                            case (int)eTransactionState.Failed:

                                // process renewal failed
                                log.Error("process renewal failed");
                                HandleAdapterTransactionFailed(ref response, adapterResponse, productId, eTransactionType.Subscription, billingGuid, 0, paymentGateway.ID, householdId, userId, customData, paymentNumber);
                                break;

                            default:

                                // process renewal returned an unhandled status
                                response.Status = new ApiObjects.Response.Status()
                                {
                                    Code = (int)eResponseStatus.UnknownTransactionState,
                                    Message = "Unknown transaction state"
                                };
                                log.ErrorFormat("Could not parse adapter result ENUM. Received: {0}, log string: {1}", adapterResponse.Transaction.StateCode, logString);
                                break;
                        }
                    }
                    else
                    {
                        // general response code is not OK
                        ApiObjects.Response.Status status = new ApiObjects.Response.Status();
                        switch (adapterResponse.Status.Code)
                        {
                            case (int)PaymentGatewayAdapterStatus.NoConfigurationFound:

                                // no configuration found
                                status.Code = (int)eResponseStatus.NoConfigurationFound;
                                status.Message = "Payment Gateway Adapter : No Configuration Found";
                                break;

                            case (int)PaymentGatewayAdapterStatus.SignatureMismatch:

                                // signature mismatch
                                status.Code = (int)eResponseStatus.SignatureMismatch;
                                status.Message = "Payment Gateway Adapter : Signature Mismatch";
                                break;

                            case (int)PaymentGatewayAdapterStatus.Error:
                            default:

                                // general error
                                status.Code = (int)eResponseStatus.Error;
                                status.Message = "Unknown Gateway adapter transaction error";
                                break;
                        }

                        log.ErrorFormat("process renewal returned the following status: {0}, message: {1}", ((eResponseStatus)status.Code).ToString(), status.Message);
                        response.Status = status;
                    }
                }
            }
            return response;

        }

        private TransactResult SendPaymentToAdapter(string chargeId, double price, string currency, string userIP, int productId, eTransactionType productType, int contentId,
            string siteGuid, long householdID, string billingGuid, PaymentGateway paymentGateway, string customData, int paymentNumber, string paymentMethodExternalId, int pghpmId, string adapterData)
        {
            TransactResult response = new TransactResult();

            string logString = string.Format("chargeId: {0}, price: {1}, currency: {2}, userIP: {3}, productId: {4}, productType: {5}, contentId: {6}, siteGuid: {7}, householdID: {8}, billingGuid {9}, customData: {10}, paymentMethodExternalId: {11}, pghpmId: {12}, adapterData: {13}",
                chargeId != null ? chargeId : string.Empty,         // {0}
                price,                                              // {1}
                currency != null ? currency : string.Empty,         // {2}
                userIP != null ? userIP : string.Empty,             // {3}
                productId,                                          // {4}
                productType.ToString(),                             // {5}
                contentId,                                          // {6}
                siteGuid != null ? siteGuid : string.Empty,         // {7}
                householdID,                                        // {8}
                billingGuid != null ? billingGuid : string.Empty,   // {9}
                customData != null ? customData : string.Empty,    // {10}
                !string.IsNullOrEmpty(paymentMethodExternalId) ? paymentMethodExternalId : string.Empty,
                pghpmId, adapterData);

            log.DebugFormat("SendPaymentToAdapter {0}", logString);

            long userId = 0;
            if (!long.TryParse(siteGuid, out userId))
            {
                response.Status = new ApiObjects.Response.Status() { Code = (int)eResponseStatus.InvalidUser, Message = "Invalid User" };
                return response;
            }

            if (string.IsNullOrEmpty(paymentGateway.AdapterUrl))
            {
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.AdapterUrlRequired, ADAPTER_URL_REQUIRED);
                return response;
            }

            TransactionRequest request = new TransactionRequest()
            {
                groupId = groupID,
                billingGuid = billingGuid,
                chargeId = chargeId,
                paymentMethodExternalId = paymentMethodExternalId,
                contentId = contentId,
                currency = currency,
                customData = customData,
                householdID = householdID,
                paymentGateway = paymentGateway,
                price = price,
                productId = productId,
                productType = productType,
                siteGuid = siteGuid,
                userIP = userIP,
                adapterData = adapterData
            };

            APILogic.PaymentGWAdapter.TransactionResponse adapterResponse = AdaptersController.GetInstance(paymentGateway.ID).Transact(request);

            response = ValidateAdapterResponse(adapterResponse, logString);

            if (response == null)
            {
                response = new TransactResult()
                {
                    Status = new ApiObjects.Response.Status()
                    {
                        Code = (int)eResponseStatus.Error,
                        Message = "Error validating adapter response"
                    }
                };
            }
            // if response from validation is ok, continue
            // if it is not ok, use the response from validation as final response
            else if (response.Status.Code == (int)eResponseStatus.OK)
            {
                if (adapterResponse.Status.Code == (int)PaymentGatewayAdapterStatus.OK)
                {
                    switch (adapterResponse.Transaction.StateCode)
                    {
                        case (int)eTransactionState.OK:
                            {
                                CreateTransaction(ref response, adapterResponse, productId, productType, billingGuid,
                                    contentId, paymentGateway.ID, householdID, userId, customData, BILLING_TRANSACTION_SUCCESS_STATUS, paymentNumber, pghpmId);

                                if (response != null && response.Status.Code != (int)eResponseStatus.OK)
                                {
                                    // error saving transaction
                                    log.ErrorFormat("Error creating transaction (adapter code success). log string: {0}", logString);
                                }

                                break;
                            }
                        case (int)eTransactionState.Pending:
                            {
                                int paymentGatewayTransactionId = CreateTransaction(ref response, adapterResponse, productId, productType,
                                    billingGuid, contentId, paymentGateway.ID, householdID, userId, customData, BILLING_TRANSACTION_SUCCESS_STATUS, paymentNumber, pghpmId);

                                if (response != null && response.Status.Code == (int)eResponseStatus.OK)
                                {
                                    //set return response values
                                    response.State = eTransactionState.Pending;

                                    // set PaymentGWPending for saving
                                    PaymentGatewayPending paymentGWPending = new PaymentGatewayPending()
                                    {
                                        PaymentGatewayTransactionId = paymentGatewayTransactionId,
                                        NextRetryDate = DateTime.UtcNow.AddMinutes(paymentGateway.PendingInterval),
                                        BillingGuid = billingGuid,
                                        AdapterRetryCount = 0
                                    };

                                    // Insert PaymentGateway pending return new paymentGateway pendingId
                                    paymentGWPending.ID = DAL.BillingDAL.InsertPaymentGWPending(groupID, paymentGWPending);

                                    if (paymentGWPending.ID == 0)
                                    {
                                        response.Status = new ApiObjects.Response.Status()
                                        {
                                            Code = (int)eResponseStatus.Error,
                                            Message = ERROR_SAVING_PAYMENT_GATEWAY_PENDING
                                        };
                                        log.ErrorFormat("{0}. log string: {1}", ERROR_SAVING_PAYMENT_GATEWAY_PENDING, logString);
                                    }
                                    else
                                    {
                                        // Retry only if gateway has defined pending retires amount
                                        if (paymentGateway.PendingRetries > 0)
                                        {
                                            // enqueue pending transaction
                                            PendingTransactionsQueue queue = new PendingTransactionsQueue();
                                            PendingTransactionData data = new PendingTransactionData(groupID,
                                                paymentGWPending,
                                                siteGuid,
                                                productId,
                                                (int)productType);

                                            bool enqueueSuccessful = queue.Enqueue(data, string.Format(ROUTING_KEY_CHECK_PENDING_TRANSACTION, groupID));

                                            if (!enqueueSuccessful)
                                            {
                                                log.ErrorFormat("Failed enqueue of pending transaction {0}", data);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    // error saving transaction
                                    log.ErrorFormat("Error creating transaction (adapter code pending). log string: {0}", logString);
                                }
                                break;
                            }
                        case (int)eTransactionState.Failed:
                            {
                                HandleAdapterTransactionFailed(ref response, adapterResponse, productId, productType,
                                    billingGuid, contentId, paymentGateway.ID, householdID, userId, customData, paymentNumber);
                                break;
                            }
                        default:
                            {
                                response.Status = new ApiObjects.Response.Status()
                                {
                                    Code = (int)eResponseStatus.UnknownTransactionState,
                                    Message = "Unknown transaction state"
                                };
                                log.ErrorFormat("Could not parse adapter result ENUM. Received: {0}, log string: {1}", adapterResponse.Transaction.StateCode, logString);
                                break;
                            }
                    }
                }
                else
                {
                    ApiObjects.Response.Status status = new ApiObjects.Response.Status();
                    switch (adapterResponse.Status.Code)
                    {
                        case (int)PaymentGatewayAdapterStatus.NoConfigurationFound:
                            status.Code = (int)eResponseStatus.NoConfigurationFound;
                            status.Message = "Payment Gateway Adapter : No Configuration Found";
                            break;
                        case (int)PaymentGatewayAdapterStatus.SignatureMismatch:
                            status.Code = (int)eResponseStatus.SignatureMismatch;
                            status.Message = "Payment Gateway Adapter : Signature Mismatch";
                            break;
                        case (int)PaymentGatewayAdapterStatus.Error:
                        default:
                            status.Code = (int)eResponseStatus.Error;
                            status.Message = "Unknown Gateway adapter transaction error";
                            break;
                    }

                    response.Status = status;
                }
            }


            return response;
        }

        private ApiObjects.Response.Status SendRemoveHouseholdPaymentmethodToAdapter(string chargeId, long householdID, PaymentGateway paymentGateway, string paymentMethodExternalId)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status();

            string logString = string.Format("chargeId: {0}, householdID: {1}, paymentMethodExternalId: {2}",
                chargeId != null ? chargeId : string.Empty,
                householdID,
                !string.IsNullOrEmpty(paymentMethodExternalId) ? paymentMethodExternalId : string.Empty);

            log.DebugFormat("RemoveHouseholdPaymentmethod {0}", logString);

            if (string.IsNullOrEmpty(paymentGateway.AdapterUrl))
            {
                response = new ApiObjects.Response.Status((int)eResponseStatus.AdapterUrlRequired, ADAPTER_URL_REQUIRED);
                return response;
            }

            APILogic.PaymentGWAdapter.PaymentMethodResponse adapterResponse = AdaptersController.GetInstance(paymentGateway.ID).RemoveHouseholdPaymentMethod(paymentGateway, groupID, chargeId, paymentMethodExternalId);

            if (adapterResponse == null || adapterResponse.Status == null)
            {
                response = new ApiObjects.Response.Status()
                {
                    Code = (int)eResponseStatus.Error,
                    Message = "Error validating adapter response"
                };
            }

            if (adapterResponse.Status.Code == (int)PaymentGatewayAdapterStatus.OK)
            {
                if (adapterResponse.IsSuccess)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
                else
                {
                    log.ErrorFormat("RemoveHouseholdPaymentMethod: remove payment method did not succeed for charge id: {0}, payment method: {1}, adapter Status: {2}, adapter Message: {3}",
                        chargeId, paymentMethodExternalId, adapterResponse.PGStatus, adapterResponse.PGMessage);
                    response = new ApiObjects.Response.Status((int)eResponseStatus.Error, adapterResponse.PGMessage);
                }
            }
            else
            {
                switch (adapterResponse.Status.Code)
                {
                    case (int)PaymentGatewayAdapterStatus.NoConfigurationFound:
                        response.Code = (int)eResponseStatus.NoConfigurationFound;
                        response.Message = "Payment Gateway Adapter : No Configuration Found";
                        break;
                    case (int)PaymentGatewayAdapterStatus.SignatureMismatch:
                        response.Code = (int)eResponseStatus.SignatureMismatch;
                        response.Message = "Payment Gateway Adapter : Signature Mismatch";
                        break;
                    case (int)PaymentGatewayAdapterStatus.Error:
                    default:
                        response.Code = (int)eResponseStatus.Error;
                        response.Message = "Unknown Gateway adapter transaction error";
                        break;
                }
            }

            return response;
        }

        private TransactResult HandleAdapterTransactionFailed(ref TransactResult response, APILogic.PaymentGWAdapter.TransactionResponse adapterResponse, int productId, eTransactionType productType,
            string billingGuid, int contentId, int paymentGatewayId, long householdID, long userId, string customData, int paymentNumber)
        {
            //incase "CREATE_TRANSACTION true
            bool failReasonCodeExist;

            if (adapterResponse != null &&
                adapterResponse.Transaction != null)
            {
                bool createTransaction = DAL.BillingDAL.GetPaymentGatewayFailReason(adapterResponse.Transaction.FailReasonCode, out failReasonCodeExist);

                if (!failReasonCodeExist)
                {
                    response.Status = new ApiObjects.Response.Status() { Code = (int)eResponseStatus.PaymentGatewayAdapterFailReasonUnknown, Message = "Payment gateway adapter fail reason unknown" };
                    return response;
                }

                if (createTransaction)
                {
                    // save transaction
                    CreateTransaction(ref response, adapterResponse, productId, productType, billingGuid, contentId, paymentGatewayId, householdID,
                        userId, customData, BILLING_TRANSACTION_FAIL_STATUS, paymentNumber, 0);
                }
                else
                {
                    //set return response values
                    response.PGResponseID = !string.IsNullOrEmpty(adapterResponse.Transaction.PGStatus) ? adapterResponse.Transaction.PGStatus : string.Empty;
                    response.PGReferenceID = !string.IsNullOrEmpty(adapterResponse.Transaction.PGTransactionID) ? adapterResponse.Transaction.PGTransactionID : string.Empty;
                    response.State = eTransactionState.Failed;
                    response.FailReasonCode = adapterResponse.Transaction.FailReasonCode;
                    response.PaymentDetails = !string.IsNullOrEmpty(adapterResponse.Transaction.PaymentDetails) ? adapterResponse.Transaction.PaymentDetails : string.Empty;
                    response.PaymentMethod = !string.IsNullOrEmpty(adapterResponse.Transaction.PaymentMethod) ? adapterResponse.Transaction.PaymentMethod : string.Empty;
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                    response.StartDateSeconds = adapterResponse.Transaction.StartDateSeconds;
                    response.EndDateSeconds = adapterResponse.Transaction.EndDateSeconds;
                    response.AutoRenewing = adapterResponse.Transaction.AutoRenewing;
                }
            }

            return response;
        }

        private ApiObjects.Response.Status CreateResponseStatus(int adapterResponseStatusCode)
        {
            ApiObjects.Response.Status status = new ApiObjects.Response.Status();
            switch (adapterResponseStatusCode)
            {
                case (int)PaymentGatewayAdapterStatus.NoConfigurationFound:
                    status.Code = (int)eResponseStatus.NoConfigurationFound;
                    status.Message = "Payment Gateway Adapter : No Configuration Found";
                    break;
                case (int)PaymentGatewayAdapterStatus.SignatureMismatch:
                    status.Code = (int)eResponseStatus.SignatureMismatch;
                    status.Message = "Payment Gateway Adapter : Signature Mismatch";
                    break;
                case (int)PaymentGatewayAdapterStatus.Error:
                default:
                    status.Code = (int)eResponseStatus.Error;
                    status.Message = "Unknown Gateway adapter transaction error";
                    break;
            }

            return status;
        }

        private int CreateTransaction(ref TransactResult response, APILogic.PaymentGWAdapter.TransactionResponse adapterResponse, int productId, eTransactionType productType, string billingGuid,
           int contentId, int paymentGatewayId, long householdID, long userId, string customData, int billingTransactionStatus, int paymentNumber, int paymenMethodId)
        {
            PaymentGatewayTransaction paymentGWTransaction = new PaymentGatewayTransaction();

            string logString = string.Format("{0}: productId: {1}, productType: {2}, billingGuid: {3}, contentId: {4}, paymentGatewayId: {5}, householdID: {6}, userId: {7}, customData: {8}, billingTransactionStatus: {9}",
                    "CreateTransaction",                               // {0}
                    productId,                                         // {1}             
                    productType.ToString(),                            // {2}
                    billingGuid != null ? billingGuid : string.Empty,  // {3}
                    contentId,                                         // {4}
                    paymentGatewayId,                                  // {5}
                    householdID,                                       // {6}
                    userId,                                            // {7}
                    customData != null ? customData : string.Empty,    // {8}
                    billingTransactionStatus);                         // {9}


            if (adapterResponse == null ||
                adapterResponse.Transaction == null)
            {
                response.Status = new ApiObjects.Response.Status() { Code = (int)eResponseStatus.Error, Message = ERROR_SAVING_PAYMENT_GATEWAY_TRANSACTION };
                log.ErrorFormat("{0}. adapterResponse or adapterResponse.Transaction are null. log string: {1}",
                    ERROR_SAVING_PAYMENT_GATEWAY_TRANSACTION,  // {0}
                    logString);                                // {1}  
            }
            else
            {
                paymentGWTransaction = SaveTransaction(ref response, adapterResponse.Transaction.PGTransactionID, adapterResponse.Transaction.PGStatus, productId, (int)productType, billingGuid, contentId,
                    adapterResponse.Transaction.PGMessage, adapterResponse.Transaction.StateCode, paymentGatewayId, adapterResponse.Transaction.FailReasonCode, adapterResponse.Transaction.PaymentMethod,
                    adapterResponse.Transaction.PaymentDetails, paymenMethodId, householdID, userId, customData, billingTransactionStatus, paymentNumber,
                    adapterResponse.Transaction.StartDateSeconds, adapterResponse.Transaction.EndDateSeconds, adapterResponse.Transaction.AutoRenewing);
            }

            return paymentGWTransaction.ID;
        }

        private PaymentGatewayTransaction SaveTransaction(ref TransactResult response, string externalTransactionId, string externalStatus, int productId, int productType, string billingGuid, int contentId, string message,
            int state, int paymentGatewayId, int failReason, string paymentMethod, string paymentDetails, int paymenMethodId, long householdID, long userId, string customData, int billingTransactionStatus = BILLING_TRANSACTION_SUCCESS_STATUS,
            int paymentNumber = 1, long startDateSeconds = 0, long endDateSeconds = 0, bool autoRenewing = false)
        {
            string logString = string.Format("{0}: productId: {1}, productType: {2}, billingGuid: {3}, contentId: {4}, paymentGatewayId: {5}, householdID: {6}, userId: {7}, customData: {8}, billingTransactionStatus: {9}",
                    "CreateTransaction",                               // {0}
                    productId,                                         // {1}             
                    productType.ToString(),                            // {2}
                    billingGuid != null ? billingGuid : string.Empty,  // {3}
                    contentId,                                         // {4}
                    paymentGatewayId,                                  // {5}
                    householdID,                                       // {6}
                    userId,                                            // {7}
                    customData != null ? customData : string.Empty,    // {8}
                    billingTransactionStatus);                         // {9}

            // set PaymentGWTransaction for saving
            PaymentGatewayTransaction paymentGWTransaction = new PaymentGatewayTransaction()
            {
                ExternalTransactionId = !string.IsNullOrEmpty(externalTransactionId) ? externalTransactionId : string.Empty,
                ExternalStatus = !string.IsNullOrEmpty(externalStatus) ? externalStatus : string.Empty,
                ProductId = productId,
                ProductType = productType,
                BillingGuid = !string.IsNullOrEmpty(billingGuid) ? billingGuid : string.Empty,
                ContentId = contentId,
                Message = !string.IsNullOrEmpty(message) ? message : string.Empty,
                State = state,
                PaymentGatewayID = paymentGatewayId,
                FailReason = failReason,
                PaymentMethod = !string.IsNullOrEmpty(paymentMethod) ? paymentMethod : string.Empty,
                PaymentDetails = !string.IsNullOrEmpty(paymentDetails) ? paymentDetails : string.Empty,
                PaymentMethodId = paymenMethodId
            };

            // Insert PaymentGateway Transaction return new paymentGateway TransactionId
            paymentGWTransaction.ID = DAL.BillingDAL.InsertPaymentGatewayTransaction(groupID, householdID, userId, paymentGWTransaction);

            //success
            if (paymentGWTransaction.ID > 0)
            {
                //set return response values
                response.PGResponseID = !string.IsNullOrEmpty(externalStatus) ? externalStatus : string.Empty;
                response.PGReferenceID = !string.IsNullOrEmpty(externalTransactionId) ? externalTransactionId : string.Empty;
                response.State = (eTransactionState)state;
                response.FailReasonCode = failReason;
                response.PaymentDetails = !string.IsNullOrEmpty(paymentDetails) ? paymentDetails : string.Empty;
                response.PaymentMethod = !string.IsNullOrEmpty(paymentMethod) ? paymentMethod : string.Empty;
                response.StartDateSeconds = startDateSeconds;
                response.EndDateSeconds = endDateSeconds;
                response.AutoRenewing = autoRenewing;
                // create billing transaction
                long billingTranactionId = InsertBillingTransaction(BILLING_PROVIDER, paymentGatewayId, customData, paymentGWTransaction, billingTransactionStatus, paymentNumber);

                if (billingTranactionId < 1)
                {
                    // create billing transaction failed
                    response.Status = new ApiObjects.Response.Status() { Code = (int)eResponseStatus.Error, Message = ERROR_SAVING_PAYMENT_GATEWAY_TRANSACTION };
                    log.ErrorFormat("Error creating billing transaction. log string: {0}", logString);
                }
                else
                {
                    // create billing transaction passed
                    response.Status = new ApiObjects.Response.Status() { Code = (int)eResponseStatus.OK };
                    response.TransactionID = billingTranactionId;
                }
            }
            else
            {
                response.Status = new ApiObjects.Response.Status() { Code = (int)eResponseStatus.Error, Message = ERROR_SAVING_PAYMENT_GATEWAY_TRANSACTION };
                log.ErrorFormat("{0} log string: {1}",
                    ERROR_SAVING_PAYMENT_GATEWAY_TRANSACTION,  // {0}
                    logString);                                // {1}             
            }

            return paymentGWTransaction;
        }

        private long InsertBillingTransaction(int nBillingProvider, int nBillingMethod, string customData, PaymentGatewayTransaction paymentGWTransaction,
                                                int status, int paymentNumber)
        {
            int nNumberOfPayments = 0;

            int nMediaFileID = 0;
            int nMediaID = 0;
            string sSubscriptionCode = string.Empty;
            string sPPVCode = string.Empty;
            string sPriceCode = string.Empty;
            string sPPVModuleCode = string.Empty;
            bool bIsRecurring = false;
            string sCurrencyCode = string.Empty;
            double dChargePrice = 0.0;

            string sRelevantSub = string.Empty;
            string sUserGUID = string.Empty;
            int nMaxNumberOfUses = 0;
            int nMaxUsageModuleLifeCycle = 0;
            int nViewLifeCycleSecs = 0;
            string sPurchaseType = string.Empty;
            string sPreviewModuleID = string.Empty;

            string sCountryCd = string.Empty;
            string sLanguageCode = string.Empty;
            string sDeviceName = string.Empty;
            string sPrePaidCode = string.Empty;
            string sCollectionCode = string.Empty;

            Core.Billing.Utils.SplitRefference(customData, ref nMediaFileID, ref nMediaID, ref sSubscriptionCode, ref sPPVCode, ref sPrePaidCode, ref sPriceCode,
                    ref dChargePrice, ref sCurrencyCode, ref bIsRecurring, ref sPPVModuleCode, ref nNumberOfPayments, ref sUserGUID,
                                ref sRelevantSub, ref nMaxNumberOfUses, ref nMaxUsageModuleLifeCycle, ref nViewLifeCycleSecs, ref sPurchaseType,
                                ref sCountryCd, ref sLanguageCode, ref sDeviceName, ref sPreviewModuleID, ref sCollectionCode);

            long lBillingTransactionID = Core.Billing.Utils.InsertBillingTransaction(sUserGUID, string.Empty, dChargePrice, sPriceCode,
                    sCurrencyCode, customData, status, string.Empty, bIsRecurring, nMediaFileID, nMediaID, sPPVModuleCode,
                    sSubscriptionCode, "", groupID, nBillingProvider, paymentGWTransaction.ID, 0.0, dChargePrice, paymentNumber, nNumberOfPayments, "",
                    sCountryCd, sLanguageCode, sDeviceName, nBillingMethod, nBillingMethod, sPrePaidCode, sPreviewModuleID, sCollectionCode, paymentGWTransaction.BillingGuid);

            return lBillingTransactionID;
        }


        public ApiObjects.Response.Status SetPaymentGatewayConfiguration(int paymentGatewayId)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status();

            if (paymentGatewayId == 0)
            {
                return new ApiObjects.Response.Status((int)eResponseStatus.PaymentGatewayNotExist, ERROR_PAYMENT_GATEWAY_NOT_EXIST);
            }

            try
            {
                if (paymentGatewayId > 0)
                {
                    List<PaymentGateway> paymentpaymentGatewayList = DAL.BillingDAL.GetPaymentGatewaySettingsList(groupID, paymentGatewayId);

                    PaymentGateway paymentGateway = null;

                    if (paymentpaymentGatewayList != null && paymentpaymentGatewayList.Count > 0)
                    {
                        paymentGateway = paymentpaymentGatewayList[0];
                    }

                    if (SendConfigurationToAdapter(paymentGateway))
                    {
                        response = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                    }
                    else
                    {
                        response = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                    }
                }
            }

            catch (Exception ex)
            {
                response = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.ErrorFormat("Failed ex={0}", ex);
            }

            return response;
        }

        public PaymentGatewayConfigurationResponse GetPaymentGatewayConfiguration(string paymentGWExternalId, string intent, List<ApiObjects.KeyValuePair> extraParams)
        {
            PaymentGatewayConfigurationResponse res = new PaymentGatewayConfigurationResponse();

            if (string.IsNullOrEmpty(paymentGWExternalId))
            {
                res.Status = new ApiObjects.Response.Status((int)eResponseStatus.PaymentGatewayNotExist, ERROR_PAYMENT_GATEWAY_NOT_EXIST);
                return res;
            }

            int paymentGWId = DAL.BillingDAL.GetPaymentGWInternalID(groupID, paymentGWExternalId);

            if (paymentGWId <= 0)
            {
                res.Status = new ApiObjects.Response.Status((int)eResponseStatus.PaymentGatewayNotExist, ERROR_PAYMENT_GATEWAY_NOT_EXIST);
                return res;
            }

            try
            {
                List<PaymentGateway> paymentpaymentGatewayList = DAL.BillingDAL.GetPaymentGatewaySettingsList(groupID, paymentGWId);

                PaymentGateway paymentGateway = null;

                if (paymentpaymentGatewayList == null || paymentpaymentGatewayList.Count == 0)
                {
                    res.Status = new ApiObjects.Response.Status((int)eResponseStatus.PaymentGatewayNotExist, ERROR_PAYMENT_GATEWAY_NOT_EXIST);
                    return res;
                }

                paymentGateway = paymentpaymentGatewayList[0];

                APILogic.PaymentGWAdapter.ConfigurationResponse adapterResponse = AdaptersController.GetInstance(paymentGateway.ID).GetAdapterConfiguration(paymentGateway, groupID, intent, extraParams);

                if (adapterResponse == null || adapterResponse.Status == null)
                {
                    log.Error("Error received while trying to get configuration");
                    res = new PaymentGatewayConfigurationResponse()
                    {
                        Status = new ApiObjects.Response.Status()
                        {
                            Code = (int)eResponseStatus.Error,
                            Message = "Error validating adapter response"
                        }
                    };
                }
                // validation OK -> continue
                // validation NOT OK -> validation response is final response
                else if (adapterResponse.Status.Code == (int)PaymentGatewayAdapterStatus.OK)
                {
                    if (adapterResponse.Configuration == null || adapterResponse.Configuration.Length == 0)
                        res.Configuration = new List<ApiObjects.KeyValuePair>();
                    else
                        res.Configuration = adapterResponse.Configuration.Select(x => new ApiObjects.KeyValuePair(x.Key, x.Value)).ToList();
                }

                res.Status = new ApiObjects.Response.Status();

                switch (adapterResponse.Status.Code)
                {
                    case (int)PaymentGatewayAdapterStatus.OK:
                        res.Status.Code = (int)eResponseStatus.OK;
                        break;
                    case (int)PaymentGatewayAdapterStatus.SignatureMismatch:
                        res.Status.Code = (int)eResponseStatus.SignatureMismatch;
                        break;
                    case (int)PaymentGatewayAdapterStatus.NoConfigurationFound:
                        res.Status.Code = (int)eResponseStatus.Error;
                        break;
                    default:
                        res.Status.Code = (int)eResponseStatus.Error;
                        break;
                }

                res.Status.Message = adapterResponse.Status.Message;
            }

            catch (Exception ex)
            {
                res.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.ErrorFormat("GetPaymentGatewayConfiguration Failed ex={0}", ex);
            }

            return res;
        }

        private bool SendConfigurationToAdapter(PaymentGateway paymentGateway)
        {
            if (paymentGateway != null && !string.IsNullOrEmpty(paymentGateway.AdapterUrl))
            {
                APILogic.PaymentGWAdapter.ServiceClient client = new APILogic.PaymentGWAdapter.ServiceClient(string.Empty, paymentGateway.AdapterUrl);
                if (!string.IsNullOrEmpty(paymentGateway.AdapterUrl))
                {
                    client.Endpoint.Address = new System.ServiceModel.EndpointAddress(paymentGateway.AdapterUrl);
                }

                //set unixTimestamp
                long unixTimestamp = ODBCWrapper.Utils.DateTimeToUnixTimestamp(DateTime.UtcNow);

                //set signature
                string signature = string.Concat(paymentGateway.ID, paymentGateway.TransactUrl, paymentGateway.StatusUrl, paymentGateway.RenewUrl,
                    paymentGateway.Settings != null ? string.Concat(paymentGateway.Settings.Select(s => string.Concat(s.key, s.value))) : string.Empty,
                    this.groupID, unixTimestamp);

                //call Adapter Transact
                APILogic.PaymentGWAdapter.AdapterStatus adapterResponse =
                    client.SetConfiguration(paymentGateway.ID, paymentGateway.TransactUrl, paymentGateway.StatusUrl, paymentGateway.RenewUrl,
                    paymentGateway.Settings != null ?
                        paymentGateway.Settings.Select(s => new APILogic.PaymentGWAdapter.KeyValue() { Key = s.key, Value = s.value }).ToArray() : null,
                    this.groupID,
                    unixTimestamp,
                    System.Convert.ToBase64String(TVinciShared.EncryptUtils.AesEncrypt(paymentGateway.SharedSecret, TVinciShared.EncryptUtils.HashSHA1(signature))));

                if (adapterResponse != null && adapterResponse.Code == (int)PaymentGatewayAdapterStatus.OK)
                {
                    log.DebugFormat("Payment Gateway Adapter SetConfiguration Result: AdapterID = {0}, AdapterStatus = {1}", paymentGateway.ID, adapterResponse.Code);
                    return true;
                }
                else
                {
                    log.DebugFormat("Payment Gateway Adapter SetConfiguration Result: AdapterID = {0}, AdapterStatus = {1}",
                        paymentGateway.ID, adapterResponse != null ? adapterResponse.Code.ToString() : "ERROR");
                    return false;
                }
            }
            return false;
        }

        public UpdatePendingResponse UpdatePendingTransaction(string paymentGatewayId, int adapterTransactionState, string externalTransactionId, string externalStatus,
            string externalMessage, int failReason, string signature)
        {
            UpdatePendingResponse response = new UpdatePendingResponse();

            // validate paymentGatewayId
            int pgId = 0;
            if (string.IsNullOrEmpty(paymentGatewayId) || !int.TryParse(paymentGatewayId, out pgId))
            {
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.NoPaymentGateway, NO_PAYMENT_GATEWAY);
                return response;
            }

            try
            {
                // parse and validate the transaction state is a known state
                eTransactionState transactionStatus;
                if (!TryConvertTransactionState(adapterTransactionState, out transactionStatus))
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.UnknownTransactionState, UNKNOWN_TRANSACTION_STATE);
                    return response;
                }

                if (transactionStatus == eTransactionState.Failed)
                {
                    bool failReasonCodeExist = false;
                    bool createTransaction = DAL.BillingDAL.GetPaymentGatewayFailReason(failReason, out failReasonCodeExist);

                    if (!failReasonCodeExist)
                    {
                        response.Status = new ApiObjects.Response.Status() { Code = (int)eResponseStatus.PaymentGatewayAdapterFailReasonUnknown, Message = "Payment gateway adapter fail reason unknown" };
                        return response;
                    }
                }

                // if status is pending - nothing to do
                if (transactionStatus == eTransactionState.Pending)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                    return response;
                }

                // get paymentGateway
                List<PaymentGateway> paymentpaymentGatewayList = DAL.BillingDAL.GetPaymentGatewaySettingsList(groupID, pgId);

                PaymentGateway paymentGateway = null;

                if (paymentpaymentGatewayList != null && paymentpaymentGatewayList.Count > 0)
                {
                    paymentGateway = paymentpaymentGatewayList[0];
                }
                // validate paymentGateway was found
                if (paymentGateway == null)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.PaymentGatewayNotExist, ERROR_PAYMENT_GATEWAY_NOT_EXIST);
                    return response;
                }

                // validate signature
                if (!TVinciShared.EncryptUtils.IsSignatureValid(string.Concat(paymentGatewayId, adapterTransactionState, externalTransactionId, externalStatus, externalMessage, failReason), signature, paymentGateway.SharedSecret))
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.SignatureDoesNotMatch, SIGNATURE_DOES_NOT_MATCH);
                    return response;
                }

                // get relevant transaction data
                string billingGuid;
                int productType, transactionState, pendingTransactionState, domainId;
                if (!DAL.BillingDAL.GetPendingPaymentGatewayTransactionDetails(pgId, externalTransactionId, out billingGuid, out productType, out transactionState, out pendingTransactionState, out domainId) || string.IsNullOrEmpty(billingGuid))
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.PaymentGatewayTransactionNotFound, PAYMENT_GATEWAY_TRANSACTION_NOT_FOUND);
                    log.DebugFormat("payment gateway transaction was not found. paymentGatewayId = {0}, adapterTransactionState = {1}, failReason = {2}, externalTransactionId = {3}, externalStatus = {4}, externalMessage = {5}, failReason = {6},",
                        paymentGatewayId, adapterTransactionState, failReason, externalTransactionId, externalStatus, externalMessage, failReason);
                }

                // check if transaction is pending
                if (transactionState != (int)eTransactionState.Pending || pendingTransactionState != (int)eTransactionState.Pending)
                {
                    log.DebugFormat("The transaction was not pending. BillingGuid = {0}", billingGuid);
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.PaymentGatewayTransactionIsNotPending, PAYMENT_GATEWAY_TRANSACTION_IS_NOT_PENDING);
                    return response;
                }

                // update pending transaction in payment_gateway_transactions and payment_gateway_pending
                if (DAL.BillingDAL.UpdatePaymentGatewayPendingTransaction(billingGuid, adapterTransactionState, externalStatus, externalMessage, failReason))
                {
                    response.BillingGuid = billingGuid;
                    response.ProductType = (eTransactionType)productType;
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                    response.TransactionState = (eTransactionState)adapterTransactionState;
                    response.DomainId = (long)domainId;
                }
                // update failed
                else
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.ErrorUpdatingPendingTransaction, ERROR_UPDATING_PENDING_TRANSACTION);
                    log.DebugFormat("Failed to update pending transaction. paymentGatewayId = {0}, adapterTransactionState = {1}, failReason = {2}, externalTransactionId = {3}, externalStatus = {4}, externalMessage = {5}",
                        paymentGatewayId, adapterTransactionState, failReason, externalTransactionId, externalStatus, externalMessage);
                }
            }

            catch (Exception ex)
            {
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.ErrorFormat("Failed. ex = {0}", ex);
            }

            return response;
        }

        private static bool TryConvertTransactionState(int state, out eTransactionState transactionState)
        {
            transactionState = eTransactionState.Failed;

            if (Enum.IsDefined(typeof(eTransactionState), state))
            {
                transactionState = (eTransactionState)state;
                return true;
            }
            return false;
        }

        public HouseholdPaymentGatewayResponse GetSelectedHouseholdPaymentGateway(int householdId)
        {
            HouseholdPaymentGatewayResponse response = new HouseholdPaymentGatewayResponse();
            try
            {
                // check Domain
                ApiObjects.Response.Status domainStatus = Utils.ValidateDomain(groupID, householdId);
                if (domainStatus.Code == (int)ResponseStatus.OK)
                {
                    string chargeId = string.Empty;
                    PaymentGateway paymentGateway = DAL.BillingDAL.GetSelectedHouseholdPaymentGateway(groupID, householdId, ref chargeId);
                    if (paymentGateway != null)
                    {
                        response.PaymentGateway = new PaymentGatewayBase() { ID = paymentGateway.ID, Name = paymentGateway.Name, IsDefault = paymentGateway.IsDefault };
                        response.SelectedBy = paymentGateway.Selected == 1 ? eHouseholdPaymentGatewaySelectedBy.Household : eHouseholdPaymentGatewaySelectedBy.Account;
                        response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                    }
                    else
                    {
                        response.Status = new ApiObjects.Response.Status((int)eResponseStatus.HouseholdNotSetToPaymentGateway, "Household Not Set To Payment Gateway");

                    }
                }
                else
                {
                    response.Status = domainStatus;
                }

            }
            catch (Exception ex)
            {
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.Error(string.Format("Failed groupID={0}", groupID), ex);
            }

            return response;
        }

        public TransactResult CheckPendingTransaction(long paymentGatewayPendingId,
            int numberOfRetries, string billingGuid, long paymentGatewayTransactionId, string siteGuid)
        {
            string logString = string.Format("paymentGatewayPendingId = {0}, paymentGatewayTransactionId = {1}, " +
                "number of retries = {2}, billing guid = {3}, site guid = {4}",
                paymentGatewayPendingId, paymentGatewayTransactionId, numberOfRetries, billingGuid, siteGuid);

            log.Debug("CheckPendingTransaction: " + logString);

            TransactResult result = null;

            PaymentGatewayTransaction paymentGatewayTransaction = DAL.BillingDAL.GetPaymentGatewayTransactionByID(paymentGatewayTransactionId);

            // If it is OK or failed, we are done here. Someone else must have updated this already
            if (paymentGatewayTransaction.State == (int)(eTransactionState.OK) ||
                paymentGatewayTransaction.State == (int)(eTransactionState.Failed))
            {
                result = new TransactResult()
                {
                    State = (eTransactionState)paymentGatewayTransaction.State,
                    TransactionID = paymentGatewayTransaction.ID,
                    PaymentDetails = paymentGatewayTransaction.PaymentDetails,
                    PaymentMethod = paymentGatewayTransaction.PaymentMethod,
                    Status = new ApiObjects.Response.Status()
                    {
                        Code = (int)eResponseStatus.OK,
                        Message = string.Empty
                    }
                };
            }
            // If it is still pending
            else if (paymentGatewayTransaction.State == (int)eTransactionState.Pending)
            {
                PaymentGateway paymentGateway = null;

                #region Get PaymentGateway object
                int paymentGatewayId = paymentGatewayTransaction.PaymentGatewayID;

                // Get PaymentGateway object by ID
                List<PaymentGateway> paymentGatewaysList = DAL.BillingDAL.GetPaymentGatewaySettingsList(groupID, paymentGatewayId);

                if (paymentGatewaysList == null || paymentGatewaysList.Count == 0)
                {
                    result = new TransactResult();
                    result.Status = new ApiObjects.Response.Status((int)eResponseStatus.PaymentGatewayNotExist, ERROR_PAYMENT_GATEWAY_NOT_EXIST);
                    return result;
                }

                paymentGateway = paymentGatewaysList[0];

                #endregion

                // Create request to adapter and check its status
                PendingTransactionRequest request = new PendingTransactionRequest()
                {
                    groupId = groupID,
                    paymentGateway = paymentGateway,
                    productId = paymentGatewayTransaction.ProductId,
                    productType = (eTransactionType)paymentGatewayTransaction.ProductType,
                    siteGuid = siteGuid,
                    pendingTransactionId = paymentGatewayTransactionId,
                    pendingExternalTransactionId = paymentGatewayTransaction.ExternalTransactionId
                };

                var adapterResponse = AdaptersController.GetInstance(paymentGatewayTransaction.PaymentGatewayID).CheckPendingTransaction(request);

                // Basic validation of response
                result = ValidateAdapterResponse(adapterResponse, logString);

                if (result == null)
                {
                    result = new TransactResult()
                    {
                        Status = new ApiObjects.Response.Status()
                        {
                            Code = (int)eResponseStatus.Error,
                            Message = "Error validating adapter response"
                        }
                    };
                }
                // if response from validation is ok, continue
                // if it is not ok, use the response from validation as final response
                else if (result.Status.Code == (int)eResponseStatus.OK)
                {
                    // If the adapter failed for any reason - return
                    if (adapterResponse.Status.Code != (int)(ResponseStatus.OK))
                    {
                        #region Adapter is not OK

                        ApiObjects.Response.Status status = new ApiObjects.Response.Status();
                        switch (adapterResponse.Status.Code)
                        {
                            case (int)PaymentGatewayAdapterStatus.NoConfigurationFound:
                                status.Code = (int)eResponseStatus.NoConfigurationFound;
                                status.Message = "Payment Gateway Adapter : No Configuration Found";
                                break;
                            case (int)PaymentGatewayAdapterStatus.SignatureMismatch:
                                status.Code = (int)eResponseStatus.SignatureMismatch;
                                status.Message = "Payment Gateway Adapter : Signature Mismatch";
                                break;
                            case (int)PaymentGatewayAdapterStatus.Error:
                            default:
                                status.Code = (int)eResponseStatus.Error;
                                status.Message = "Unknown Gateway adapter transaction error";
                                break;
                        }

                        PaymentGatewayPending pending = new PaymentGatewayPending()
                        {
                            // Increase counter by one
                            AdapterRetryCount = numberOfRetries,
                            BillingGuid = billingGuid,
                            PaymentGatewayTransactionId = (int)paymentGatewayTransactionId,
                            NextRetryDate = DateTime.UtcNow.AddMinutes(paymentGateway.PendingInterval),

                        };

                        // enqueue pending transaction
                        PendingTransactionsQueue queue = new PendingTransactionsQueue();
                        PendingTransactionData data = new PendingTransactionData(groupID,
                            pending,
                            siteGuid,
                            paymentGatewayTransaction.ProductId,
                            paymentGatewayTransaction.ProductType);

                        bool enqueueSuccessful = queue.Enqueue(data, string.Format(ROUTING_KEY_CHECK_PENDING_TRANSACTION, groupID));

                        if (!enqueueSuccessful)
                        {
                            log.ErrorFormat("Failed enqueue of pending transaction {0}", data);
                        }

                        result.Status = status;

                        #endregion
                    }
                    else
                    {
                        // If the transaction is pending
                        if (adapterResponse.Transaction.StateCode == (int)(eTransactionState.Pending))
                        {
                            #region Pending

                            // If we exceeded the number of allowed retries - bye bye. This transaction is done
                            if (numberOfRetries + 1 >= paymentGateway.PendingRetries)
                            {
                                // return a result inidicating we failed
                                result = new TransactResult()
                                {
                                    State = eTransactionState.Failed,
                                    TransactionID = paymentGatewayTransaction.ID,
                                    PaymentDetails = paymentGatewayTransaction.PaymentDetails,
                                    PaymentMethod = paymentGatewayTransaction.PaymentMethod,
                                    FailReasonCode = BasePaymentGateway.FAIL_REASON_EXCEEDED_RETRY_LIMIT_CODE,
                                    Status = new ApiObjects.Response.Status()
                                    {
                                        Code = (int)eResponseStatus.OK,
                                        Message = string.Empty
                                    }
                                };

                                // update pending transaction in payment_gateway_transactions and payment_gateway_pending
                                if (!DAL.BillingDAL.UpdatePaymentGatewayPendingTransaction(billingGuid, (int)eTransactionState.Failed,
                                    adapterResponse.Transaction.PGStatus, adapterResponse.Transaction.PGMessage,
                                    (int)BasePaymentGateway.FAIL_REASON_EXCEEDED_RETRY_LIMIT_CODE))
                                {
                                    result.Status = new ApiObjects.Response.Status((int)eResponseStatus.ErrorUpdatingPendingTransaction,
                                        ERROR_UPDATING_PENDING_TRANSACTION);
                                    log.DebugFormat("Failed to update pending transaction. paymentGatewayId = {0}," +
                                    "adapterTransactionState = {1}, failReason = {2}, externalStatus = {3}, externalMessage = {4}",
                                        paymentGatewayId, adapterResponse.Transaction.StateCode,
                                        BasePaymentGateway.FAIL_REASON_EXCEEDED_RETRY_LIMIT_CODE,
                                        adapterResponse.Transaction.PGStatus, adapterResponse.Transaction.PGMessage);
                                }
                            }
                            else
                            {
                                // If not exceeded, 

                                // update paymentgateway pending

                                PaymentGatewayPending pending = new PaymentGatewayPending()
                                {
                                    // Increase counter by one
                                    AdapterRetryCount = numberOfRetries + 1,
                                    BillingGuid = billingGuid,
                                    PaymentGatewayTransactionId = (int)paymentGatewayTransactionId,
                                    NextRetryDate = DateTime.UtcNow.AddMinutes(paymentGateway.PendingInterval),

                                };

                                // Update PaymentGateway pending, return its paymentGateway pendingId (search by billing guid)
                                pending.ID = DAL.BillingDAL.UpdatePaymentGatewayPending(groupID, pending);

                                // If not succeeded, return error
                                if (pending.ID == 0)
                                {
                                    result.Status = new ApiObjects.Response.Status()
                                    {
                                        Code = (int)eResponseStatus.Error,
                                        Message = ERROR_SAVING_PAYMENT_GATEWAY_PENDING
                                    };

                                    log.ErrorFormat(logString,
                                        ERROR_SAVING_PAYMENT_GATEWAY_PENDING,
                                        paymentGatewayPendingId, numberOfRetries, billingGuid, paymentGatewayId);
                                }
                                else
                                {
                                    // If updated successfully, enqueue new message

                                    // enqueue pending transaction
                                    PendingTransactionsQueue queue = new PendingTransactionsQueue();
                                    PendingTransactionData data = new PendingTransactionData(groupID,
                                        pending,
                                        siteGuid,
                                        paymentGatewayTransaction.ProductId,
                                        paymentGatewayTransaction.ProductType);

                                    bool enqueueSuccessful = queue.Enqueue(data, string.Format(ROUTING_KEY_CHECK_PENDING_TRANSACTION, groupID));

                                    if (!enqueueSuccessful)
                                    {
                                        log.ErrorFormat("Failed enqueue of pending transaction {0}", data);
                                    }
                                }
                            }

                            #endregion
                        }
                        else if (adapterResponse.Transaction.StateCode == (int)(eTransactionState.OK) ||
                            (adapterResponse.Transaction.StateCode == (int)(eTransactionState.Failed)))
                        {
                            #region OK And Failed

                            // return a result inidicating we failed
                            result = new TransactResult()
                            {
                                State = (eTransactionState)adapterResponse.Transaction.StateCode,
                                TransactionID = paymentGatewayTransaction.ID,
                                PaymentDetails = paymentGatewayTransaction.PaymentDetails,
                                PaymentMethod = paymentGatewayTransaction.PaymentMethod,
                                FailReasonCode = adapterResponse.Transaction.FailReasonCode,
                                Status = new ApiObjects.Response.Status()
                                {
                                    Code = (int)eResponseStatus.OK,
                                    Message = string.Empty
                                }
                            };

                            // update pending transaction in payment_gateway_transactions and payment_gateway_pending
                            if (!DAL.BillingDAL.UpdatePaymentGatewayPendingTransaction(billingGuid, adapterResponse.Transaction.StateCode,
                                adapterResponse.Transaction.PGStatus, adapterResponse.Transaction.PGMessage, adapterResponse.Transaction.FailReasonCode))
                            {
                                result.Status = new ApiObjects.Response.Status((int)eResponseStatus.ErrorUpdatingPendingTransaction,
                                    ERROR_UPDATING_PENDING_TRANSACTION);

                                log.DebugFormat("Failed to update pending transaction. paymentGatewayId = {0}," +
                                    "adapterTransactionState = {1}, failReason = {2}, externalStatus = {3}, externalMessage = {4}",
                                        paymentGatewayId, adapterResponse.Transaction.StateCode,
                                        adapterResponse.Transaction.FailReasonCode,
                                        adapterResponse.Transaction.PGStatus, adapterResponse.Transaction.PGMessage);
                            }

                            #endregion

                            #region Failed Specific Part

                            // If the transaction failed - update the billing status
                            if (adapterResponse.Transaction.StateCode == (int)eTransactionState.Failed)
                            {
                                if (!DAL.ApiDAL.Update_BillingStatusAndReason_ByBillingGuid(billingGuid, 1, adapterResponse.Transaction.FailReasonCode.ToString()))
                                {
                                    result.Status = new ApiObjects.Response.Status((int)eResponseStatus.ErrorUpdatingPendingTransaction,
                                        ERROR_UPDATING_PENDING_TRANSACTION);

                                    log.DebugFormat("Failed to update pending transaction. paymentGatewayId = {0}," +
                                        "adapterTransactionState = {1}, failReason = {2}, externalStatus = {3}, externalMessage = {4}",
                                            paymentGatewayId, adapterResponse.Transaction.StateCode,
                                            adapterResponse.Transaction.FailReasonCode,
                                            adapterResponse.Transaction.PGStatus, adapterResponse.Transaction.PGMessage);
                                }
                            }

                            #endregion
                        }
                    }
                }
            }

            return result;
        }

        public TransactResult ValidateAdapterResponse(APILogic.PaymentGWAdapter.TransactionResponse adapterResponse, string logString)
        {
            // response is valid until told otherwise
            TransactResult response = new TransactResult()
                {
                    Status = new ApiObjects.Response.Status()
                    {
                        Code = (int)eResponseStatus.OK
                    }
                };

            if (adapterResponse == null || adapterResponse.Status == null)
            {
                // Adapter response is null
                response.Status = new ApiObjects.Response.Status()
                {
                    Code = (int)eResponseStatus.Error,
                    Message = "Adapter response is null"
                };
                log.ErrorFormat("Adapter response is null. log string: {0}", logString);
                return response;
            }

            if (adapterResponse.Status.Code == (int)PaymentGatewayAdapterStatus.OK && adapterResponse.Transaction == null)
            {
                log.DebugFormat(@"Payment Gateway Adapter Transaction Result Status: Message = {0}, Code = {1}",
                    adapterResponse.Status.Message != null ? adapterResponse.Status.Message : string.Empty    // {0}
                   , adapterResponse.Status.Code                                                               // {1}                       
                   );

                // Adapter transact response is null
                response.Status = new ApiObjects.Response.Status()
                {
                    Code = (int)eResponseStatus.Error,
                    Message = "Adapter transact response is null"
                };
                log.ErrorFormat("Adapter transact response is null. log string: {0}", logString);
                return response;
            }

            return response;
        }

        public virtual TransactResult VerifyReceipt(string siteGUID, long householdID, double price, string currency, string userIP, string customData,
            int productID, string productCode, eTransactionType productType, int contentID, string purchaseToken, string paymentGatewayTypeName, string billingGuid)
        {
            TransactResult response = new TransactResult();

            try
            {
                // validate type name
                if (string.IsNullOrEmpty(paymentGatewayTypeName))
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.PaymentGatewayNotExist, ERROR_PAYMENT_GATEWAY_NOT_EXIST);
                    return response;
                }

                // get payment gateway
                List<PaymentGateway> paymentGWList = DAL.BillingDAL.GetPaymentGatewaySettingsList(groupID, paymentGatewayTypeName);
                if (paymentGWList == null ||
                    paymentGWList.Count == 0 ||
                    paymentGWList[0].ID < 1)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.PaymentGatewayNotExist, ERROR_PAYMENT_GATEWAY_NOT_EXIST);
                    return response;
                }

                int paymentNumber = 1;

                response = SendVerifyReceiptToAdapter(price, currency, userIP, productID, productCode, productType, contentID, siteGUID,
                                                      householdID, billingGuid, paymentGWList[0], customData, purchaseToken, paymentNumber);
            }
            catch (Exception ex)
            {
                response = new TransactResult();
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.ErrorFormat("Failed ex={0}, siteGUID={1}, price={2}, currency={3}, customData={4}, productID={5}, productCode={6}, transactionType={7}, billingGuid={8}",
                    ex,                                                               // {0}
                    !string.IsNullOrEmpty(siteGUID) ? siteGUID : string.Empty,        // {1}
                    price,                                                            // {2}
                    !string.IsNullOrEmpty(currency) ? currency : string.Empty,        // {3}
                    !string.IsNullOrEmpty(customData) ? customData : string.Empty,    // {4}
                    productID,                                                        // {5}
                    productCode,                                                      // {6}
                    productType.ToString(),                                           // {7}
                    !string.IsNullOrEmpty(billingGuid) ? billingGuid : string.Empty); // {8})
            }

            return response;
        }

        private TransactResult SendVerifyReceiptToAdapter(double price, string currency, string userIP, int productId, string productCode, eTransactionType productType, int contentId,
            string siteGuid, long householdID, string billingGuid, PaymentGateway paymentGateway, string customData, string purchaseToken, int paymentNumber)
        {
            TransactResult response = new TransactResult();

            string logString = string.Format("price = {0}, currency = {1}, userIP = {2}, productId = {3}, productCode = {4}, productType = {5},contentId = {6}, siteGuid = {7}, householdID = {8}, billingGuid = {9}, paymentGateway = {10}, customData = {11}, purchaseToken = {12}",
                    price,                                                               // {0}
                    !string.IsNullOrEmpty(currency) ? currency : string.Empty,           // {1}
                    !string.IsNullOrEmpty(userIP) ? userIP : string.Empty,               // {2}
                    productId,                                                           // {3}
                    productCode,                                                         // {4}
                    productType.ToString(),                                              // {5} 
                    contentId,                                                           // {6} 
                    !string.IsNullOrEmpty(siteGuid) ? siteGuid : string.Empty,           // {7} 
                    householdID,                                                         // {8} 
                    !string.IsNullOrEmpty(billingGuid) ? billingGuid : string.Empty,     // {9} 
                    paymentGateway,                                                      // {10} 
                    !string.IsNullOrEmpty(customData) ? customData : string.Empty,       // {11} 
                    !string.IsNullOrEmpty(purchaseToken) ? purchaseToken : string.Empty);// {12} 


            // validate and convert userId
            long userId = 0;
            if (!long.TryParse(siteGuid, out userId))
            {
                response.Status = new ApiObjects.Response.Status() { Code = (int)eResponseStatus.InvalidUser, Message = "Invalid User" };
                return response;
            }

            // validate adapter URL exists
            if (string.IsNullOrEmpty(paymentGateway.AdapterUrl))
            {
                response.Status = new ApiObjects.Response.Status() { Code = (int)eResponseStatus.NoConfigurationFound, Message = "Adapter URL wasn't found" };
                return response;
            }
            else
            {
                // create verify request
                VerifyReceiptRequest request = new VerifyReceiptRequest()
                {
                    groupId = groupID,
                    billingGuid = billingGuid,
                    contentId = contentId,
                    currency = currency,
                    customData = customData,
                    householdID = householdID,
                    paymentGateway = paymentGateway,
                    price = price,
                    productId = productId,
                    productType = productType,
                    siteGuid = siteGuid,
                    userIP = userIP,
                    purchaseToken = purchaseToken,
                    productCode = productCode
                };

                // verify receipt
                APILogic.PaymentGWAdapter.TransactionResponse adapterResponse = AdaptersController.GetInstance(paymentGateway.ID).VerifyReceipt(request);

                // validate response
                response = ValidateAdapterResponse(adapterResponse, logString);

                if (response == null)
                {
                    // error validating adapter response
                    response = new TransactResult()
                    {
                        Status = new ApiObjects.Response.Status()
                        {
                            Code = (int)eResponseStatus.Error,
                            Message = "Error validating adapter response"
                        }
                    };
                }
                else if (response.Status.Code == (int)eResponseStatus.OK)
                {
                    // adapter response validation passed
                    if (adapterResponse.Status.Code == (int)PaymentGatewayAdapterStatus.OK)
                    {
                        // check adapter response value
                        switch (adapterResponse.Transaction.StateCode)
                        {
                            case (int)eTransactionState.OK:

                                // verification passed - create transaction
                                CreateTransaction(ref response, adapterResponse, productId, productType, billingGuid,
                                    contentId, paymentGateway.ID, householdID, userId, customData, BILLING_TRANSACTION_SUCCESS_STATUS, paymentNumber, 0);

                                if (response != null &&
                                    response.Status.Code != (int)eResponseStatus.OK)
                                {
                                    // error saving transaction
                                    log.ErrorFormat("Error creating transaction (adapter code success). log string: {0}", logString);
                                }
                                break;

                            case (int)eTransactionState.Failed:

                                // received failed from adapter
                                HandleAdapterTransactionFailed(ref response, adapterResponse, productId, productType, billingGuid, contentId, paymentGateway.ID,
                                    householdID, userId, customData, paymentNumber);
                                log.ErrorFormat("Failed to verify transaction. log string: {0}", logString);
                                break;

                            default:

                                // verify returned state - unknown
                                response.Status = new ApiObjects.Response.Status()
                                {
                                    Code = (int)eResponseStatus.UnknownTransactionState,
                                    Message = "Unknown transaction state"
                                };
                                log.ErrorFormat("Could not parse adapter result ENUM. Received: {0}, log string: {1}", adapterResponse.Transaction.StateCode, logString);
                                break;
                        }
                    }
                    else
                    {
                        // failed to verify response (received error from external status)
                        ApiObjects.Response.Status status = new ApiObjects.Response.Status();
                        switch (adapterResponse.Status.Code)
                        {
                            case (int)PaymentGatewayAdapterStatus.NoConfigurationFound:

                                status.Code = (int)eResponseStatus.NoConfigurationFound;
                                status.Message = "Payment Gateway Adapter : No Configuration Found";
                                break;

                            case (int)PaymentGatewayAdapterStatus.SignatureMismatch:

                                status.Code = (int)eResponseStatus.SignatureMismatch;
                                status.Message = "Payment Gateway Adapter : Signature Mismatch";
                                break;

                            case (int)PaymentGatewayAdapterStatus.Error:
                            default:

                                status.Code = (int)eResponseStatus.Error;
                                status.Message = "Unknown Gateway adapter transaction error";
                                break;
                        }

                        log.ErrorFormat("{0}, log string: {1}", status.Message, logString);
                        response.Status = status;
                    }
                }
            }

            return response;
        }

        public virtual PaymentGateway GetPaymentGatewayByBillingGuid(long householdId, string billingGuid)
        {
            PaymentGateway paymentGatewayResponse = null;

            try
            {
                List<PaymentDetails> paymentDetails = GetPaymentDetails(new List<string>() { billingGuid });
                
                if (paymentDetails == null || paymentDetails.Count == 0)
                {
                    log.ErrorFormat("error while getting payment GW ID. groupID: {0}, householdId: {1), billingGuid: {2}", groupID, householdId, billingGuid);
                    return paymentGatewayResponse;
                }
               
                // get payment gateway 

                PaymentDetails pd = paymentDetails.Where(x => x.BillingGuid == billingGuid).FirstOrDefault();
                paymentGatewayResponse = DAL.BillingDAL.GetPaymentGateway(groupID, pd != null ? pd.PaymentGatewayId : 0);
                if (paymentGatewayResponse == null)
                {
                    log.ErrorFormat("payment gateway was not found billingGuid ={0}", billingGuid);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Failed billingGuid ={0}, ex={1}", billingGuid, ex);
            }

            return paymentGatewayResponse;
        }
        public PaymentGatewayItemResponse GeneratePaymentGatewaySharedSecret(int paymentGatewayId)
        {
            PaymentGatewayItemResponse response = new PaymentGatewayItemResponse();

            try
            {
                if (paymentGatewayId <= 0)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.PaymentGatewayIdRequired, PAYMENT_GATEWAY_ID_REQUIRED);
                    return response;
                }

                //check payment gateway exist
                PaymentGateway paymentGateway = DAL.BillingDAL.GetPaymentGateway(groupID, paymentGatewayId, 1, 1);
                if (paymentGateway == null || paymentGateway.ID <= 0)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.PaymentGatewayNotExist, ERROR_PAYMENT_GATEWAY_NOT_EXIST);
                    return response;
                }

                // Create Shared secret 
                string sharedSecret = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 16);

                response.PaymentGateway = DAL.BillingDAL.SetPaymentGatewaySharedSecret(groupID, paymentGatewayId, sharedSecret);

                if (response.PaymentGateway != null && response.PaymentGateway.ID > 0)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, "payment gateway generate shared secret");
                }
                else
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "payment gateway failed set changes");
                }

            }
            catch (Exception ex)
            {
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.Error(string.Format("Failed groupID={0}, paymentGatewayId={1}", groupID, paymentGatewayId), ex);
            }
            return response;
        }

        public TransactResult RecordTransaction(string userId, int householdID, string externalTransactionId, string externalStatus, int productId, int productType,
            string billingGuid, int contentId, string message, int state, int paymentGatewayId, int failReason, string paymentMethod, string paymentDetails,
            string customData, string paymentMethodExternalId)
        {
            TransactResult response = new TransactResult()
            {
                Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString())
            };

            long siteGuid = 0;
            int paymentMethodId = 0;

            string logData = string.Format("userId: {0}, householdId: {1}, externalTransactionId: {2}, paymentGaytwayId: {3}, paymentMethodExternalId: {4}",
                userId, householdID, externalTransactionId, paymentGatewayId, paymentMethodExternalId);

            if (!long.TryParse(userId, out siteGuid))
            {
                log.DebugFormat("RecordTransaction: failed to parse userId = {0}", userId);
                return response;
            }

            try
            {
                if (paymentGatewayId == 0)
                {
                    log.DebugFormat("RecordTransaction: payment gateway id = 0");
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.PaymentGatewayIdRequired, PAYMENT_GATEWAY_ID_REQUIRED);
                    return response;
                }
                // get pg 
                PaymentGateway paymentGateway = DAL.BillingDAL.GetPaymentGateway(groupID, paymentGatewayId);

                if (paymentGateway == null)
                {
                    log.DebugFormat("RecordTransaction: payment gateway not found. id = {0}", paymentGatewayId);
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.PaymentGatewayNotExist, ERROR_PAYMENT_GATEWAY_NOT_EXIST);
                    return response;
                }

                if (paymentGateway.SupportPaymentMethod)
                {
                    if (!string.IsNullOrEmpty(paymentMethodExternalId))
                    {
                        PaymentGatewayHouseholdPaymentMethod pghhpm = DAL.BillingDAL.GetPaymentGatewayHouseholdPaymentMethod(groupID, paymentGatewayId, householdID, paymentMethodExternalId);

                        if (pghhpm == null || pghhpm.Id <= 0)
                        {
                            log.ErrorFormat("GetPaymentMethodExternalId payment method not exist {0}, log Data {1} ", paymentMethodExternalId, logData);
                            response.Status = new ApiObjects.Response.Status() { Code = (int)eResponseStatus.PaymentMethodNotExist, Message = PAYMENT_METHOD_NOT_EXIST };
                            return response;
                        }
                        else
                        {
                            paymentMethodId = pghhpm.Id;
                        }
                    }
                }

                // save transaction
                PaymentGatewayTransaction pgt = SaveTransaction(ref response, externalTransactionId, externalStatus, productId, productType, billingGuid, contentId,
                    message, state, paymentGatewayId, failReason, paymentMethod, paymentDetails, paymentMethodId, householdID, siteGuid, customData);

                if (pgt != null && pgt.State == (int)eTransactionState.Pending)
                {
                    //set return response values
                    response.State = eTransactionState.Pending;

                    // set PaymentGWPending for saving
                    PaymentGatewayPending paymentGWPending = new PaymentGatewayPending()
                    {
                        PaymentGatewayTransactionId = pgt.ID,
                        NextRetryDate = DateTime.UtcNow.AddMinutes(paymentGateway.PendingInterval),
                        BillingGuid = billingGuid,
                        AdapterRetryCount = 0
                    };

                    // Insert PaymentGateway pending return new paymentGateway pendingId
                    paymentGWPending.ID = DAL.BillingDAL.InsertPaymentGWPending(groupID, paymentGWPending);

                    if (paymentGWPending.ID == 0)
                    {
                        response.Status = new ApiObjects.Response.Status()
                        {
                            Code = (int)eResponseStatus.Error,
                            Message = ERROR_SAVING_PAYMENT_GATEWAY_PENDING
                        };
                        //log.ErrorFormat("{0}. log string: {1}", ERROR_SAVING_PAYMENT_GATEWAY_PENDING, logString);
                    }
                    else
                    {
                        // Retry only if gateway has defined pending retires amount
                        if (paymentGateway.PendingRetries > 0)
                        {
                            // enqueue pending transaction
                            PendingTransactionsQueue queue = new PendingTransactionsQueue();
                            PendingTransactionData data = new PendingTransactionData(groupID,
                                paymentGWPending,
                                userId,
                                productId,
                                (int)productType);

                            bool enqueueSuccessful = queue.Enqueue(data, string.Format(ROUTING_KEY_CHECK_PENDING_TRANSACTION, groupID));

                            if (!enqueueSuccessful)
                            {
                                log.ErrorFormat("Failed enqueue of pending transaction {0}", data);
                            }
                        }
                    }
                }

                if (response == null || response.Status == null || response.Status.Code != (int)eResponseStatus.OK)
                {
                    log.DebugFormat("RecordTransaction: failed to insert transaction for: paymentGaytwayId = {0}, userId = {1}, householdId = {2}", paymentGatewayId, userId, householdID);
                }
                else
                {
                    //send mail
                    eTransactionType TransactionType = eTransactionType.PPV;
                    if (typeof(eTransactionType).IsEnumDefined(productType))
                    {
                        TransactionType = (eTransactionType)productType;
                    }
                    SendMail(userId, 0, string.Empty, customData, productId, TransactionType, contentId, response, paymentGatewayId);
                }

            }
            catch (Exception ex)
            {
                log.Error(string.Format("RecordTransaction: failed with error for: paymentGaytwayId = {0}, userId = {1}, householdId = {2}", paymentGatewayId, userId, householdID), ex);
            }

            return response;
        }

        public ApiObjects.Response.Status SetPaymentGatewayHouseholdPaymentMethod(string externalIdentifier, int householdId,
            string paymentMethodName, string paymentDetails, string paymentMethodExternalId, out int pghhpmId)
        {
            ApiObjects.Response.Status status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            pghhpmId = 0;

            try
            {
                if (string.IsNullOrEmpty(paymentMethodExternalId))
                {
                    return new ApiObjects.Response.Status((int)eResponseStatus.PaymentMethodExternalIdRequired, PAYMENT_METHOD_EXTERNAL_ID_REQUIRED);
                }

                if (string.IsNullOrEmpty(externalIdentifier))
                {
                    return new ApiObjects.Response.Status((int)eResponseStatus.ExternalIdentifierRequired, EXTERNAL_IDENTIFIER_REQUIRED);
                }

                if (string.IsNullOrEmpty(paymentMethodName))
                {
                    return new ApiObjects.Response.Status((int)eResponseStatus.PaymentMethodNameRequired, PAYMENT_METHOD_NAME_REQUIRED);
                }

                // check domain
                status = Utils.ValidateDomain(groupID, householdId);

                if (status.Code != (int)ResponseStatus.OK)
                {
                    return status;
                }

                // get paymentGatewayId according to paymentGateway external id.  
                //--------------------------------------------------------------
                //PaymentGateway paymentGateway = GetPaymentGatewayHousehold(groupID, externalIdentifier, householdId, paymentMethodName, paymentMethodExternalId, out paymentGatewayStatus);

                PaymentGatewayHouseholdPaymentMethod pghpm = DAL.BillingDAL.GetPaymentGatewayHouseholdPaymentMethod(groupID, externalIdentifier, householdId,
                                                                                                                        paymentMethodName, paymentMethodExternalId);
                if (pghpm == null || pghpm.PaymentGatewayId == 0)
                {
                    return new ApiObjects.Response.Status((int)eResponseStatus.PaymentGatewayNotExist, ERROR_PAYMENT_GATEWAY_NOT_EXIST);
                }

                if (pghpm.HouseholdId == 0)
                {
                    return new ApiObjects.Response.Status((int)eResponseStatus.PaymentGatewayNotSetForHousehold, ERROR_NO_PGW_RELATED_TO_HOUSEHOLD);
                }

                if (pghpm.PaymentMethodId == 0)
                {
                    return new ApiObjects.Response.Status((int)eResponseStatus.PaymentMethodNotExist, ERROR_PAYMENT_METHOD_NOT_EXIST);
                }

                if (!string.IsNullOrEmpty(pghpm.PaymentMethodExternalId))
                {
                    return new ApiObjects.Response.Status((int)eResponseStatus.PaymentMethodAlreadySetToHouseholdPaymentGateway, PAYMENT_METHOD_ALREADY_SET_TO_HOUSEHOLD_PAYMENTGATEWAY);
                }

                pghhpmId = DAL.BillingDAL.SetPaymentGatewayHouseholdPaymentMethod(groupID, pghpm.PaymentGatewayId, householdId, pghpm.PaymentMethodId, paymentDetails,
                    null, paymentMethodExternalId);

                if (pghhpmId > 0)
                {
                    status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
                else
                {
                    status = new ApiObjects.Response.Status((int)eResponseStatus.ErrorSavingPaymentGatewayHouseholdPaymentMethod, ERROR_SAVING_PAYMENT_GATEWAY_HOUSEHOLD_PAYMENT_METHOD);
                }

            }
            catch (Exception ex)
            {
                status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.Error(string.Format("Failed groupID={0}, externalIdentifier={1}, householdID={2}", groupID, externalIdentifier, householdId), ex);
            }

            return status;
        }

        private ApiObjects.Response.Status GetPaymentGatewayHousehold(string externalIdentifier, int householdId, string paymentMethodName, string paymentMethodExternalId,
            out int paymentGatewayId)
        {
            paymentGatewayId = 0;

            PaymentGatewayHouseholdPaymentMethod pghpm = DAL.BillingDAL.GetPaymentGatewayHouseholdPaymentMethod(groupID, externalIdentifier, householdId,
                paymentMethodName, paymentMethodExternalId);

            if (pghpm == null || pghpm.PaymentGatewayId == 0)
            {
                return new ApiObjects.Response.Status((int)eResponseStatus.PaymentGatewayNotExist, ERROR_PAYMENT_GATEWAY_NOT_EXIST);
            }

            if (pghpm.HouseholdId == 0)
            {
                return new ApiObjects.Response.Status((int)eResponseStatus.PaymentGatewayNotSetForHousehold, ERROR_NO_PGW_RELATED_TO_HOUSEHOLD);
            }

            if (pghpm.PaymentMethodId == 0)
            {
                return new ApiObjects.Response.Status((int)eResponseStatus.PaymentMethodNotExist, ERROR_PAYMENT_METHOD_NOT_EXIST);
            }

            if (!string.IsNullOrEmpty(pghpm.PaymentMethodExternalId))
            {
                return new ApiObjects.Response.Status((int)eResponseStatus.PaymentMethodNotExist, ERROR_PAYMENT_METHOD_NOT_EXIST);
            }

            paymentGatewayId = pghpm.PaymentGatewayId;
            return new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
        }

        public PaymentMethodsResponse AddPaymentMethodToPaymentGateway(int paymentGatewayId, string name, bool allowMultiInstance)
        {
            PaymentMethodsResponse response = new PaymentMethodsResponse()
            {
                Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString())
            };

            // validate parameters
            if (paymentGatewayId <= 0)
            {
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.PaymentGatewayIdRequired, PAYMENT_GATEWAY_ID_REQUIRED);
                return response;
            }

            if (string.IsNullOrEmpty(name))
            {
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.PaymentMethodNameRequired, PAYMENT_METHOD_NAME_REQUIRED);
                return response;
            }

            //check payment gateway exists
            PaymentGateway paymentGateway = DAL.BillingDAL.GetPaymentGateway(groupID, paymentGatewayId, 1, 1);
            if (paymentGateway == null || paymentGateway.ID <= 0)
            {
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.PaymentGatewayNotExist, ERROR_PAYMENT_GATEWAY_NOT_EXIST);
                return response;
            }

            PaymentMethod paymentMethod = DAL.BillingDAL.Insert_PaymentGatewayPaymentMethod(groupID, paymentGatewayId, name, allowMultiInstance);

            if (paymentMethod != null)
            {
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                response.PaymentMethods = new List<PaymentMethod>() { paymentMethod };
            }

            return response;
        }

        public ApiObjects.Response.Status UpdatePaymentGatewayPaymentMethod(int paymentGatewayId, int paymentMethodId, string name, bool allowMultiInstance)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            // validate parameters
            if (paymentGatewayId <= 0)
            {
                response = new ApiObjects.Response.Status((int)eResponseStatus.PaymentGatewayIdRequired, PAYMENT_GATEWAY_ID_REQUIRED);
                return response;
            }

            if (paymentMethodId <= 0)
            {
                response = new ApiObjects.Response.Status((int)eResponseStatus.PaymentMethodIdRequired, PAYMENT_METHOD_ID_REQUIRED);
                return response;
            }

            if (string.IsNullOrEmpty(name))
            {
                return new ApiObjects.Response.Status((int)eResponseStatus.PaymentMethodNameRequired, PAYMENT_METHOD_NAME_REQUIRED);
            }

            //check payment gateway exists
            PaymentGateway paymentGateway = DAL.BillingDAL.GetPaymentGateway(groupID, paymentGatewayId, 1, 1);
            if (paymentGateway == null || paymentGateway.ID <= 0)
            {
                response = new ApiObjects.Response.Status((int)eResponseStatus.PaymentGatewayNotExist, ERROR_PAYMENT_GATEWAY_NOT_EXIST);
                return response;
            }

            //check payment method exists
            if (!DAL.BillingDAL.GetPaymentMethod(groupID, paymentGatewayId, paymentMethodId))
            {
                response = new ApiObjects.Response.Status((int)eResponseStatus.PaymentMethodNotExist, ERROR_PAYMENT_METHOD_NOT_EXIST);
                return response;
            }

            if (DAL.BillingDAL.Update_PaymentMethod(paymentMethodId, name, allowMultiInstance) != null)
            {
                response = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            }

            return response;
        }

        public PaymentMethodResponse UpdatePaymentMethod(int paymentMethodId, string name, bool allowMultiInstance)
        {
            PaymentMethodResponse response = new PaymentMethodResponse()
            {
                Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString())
            };

            if (string.IsNullOrEmpty(name))
            {
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.PaymentMethodNameRequired, PAYMENT_METHOD_NAME_REQUIRED);
                return response;
            }

            response.PaymentMethod = DAL.BillingDAL.Update_PaymentMethod(paymentMethodId, name, allowMultiInstance);
            if (response.PaymentMethod != null)
            {
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            }
            else
            {
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.PaymentMethodNotExist, eResponseStatus.PaymentMethodNotExist.ToString());
            }

            return response;
        }

        public ApiObjects.Response.Status DeletePaymentGatewayPaymentMethod(int paymentGatewayId, int paymentMethodId)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            // validate parameters
            if (paymentGatewayId <= 0)
            {
                response = new ApiObjects.Response.Status((int)eResponseStatus.PaymentGatewayIdRequired, PAYMENT_GATEWAY_ID_REQUIRED);
                return response;
            }

            if (paymentMethodId <= 0)
            {
                response = new ApiObjects.Response.Status((int)eResponseStatus.PaymentMethodIdRequired, PAYMENT_METHOD_ID_REQUIRED);
                return response;
            }

            //check payment gateway exists
            PaymentGateway paymentGateway = DAL.BillingDAL.GetPaymentGateway(groupID, paymentGatewayId, 1, 1);
            if (paymentGateway == null || paymentGateway.ID <= 0)
            {
                response = new ApiObjects.Response.Status((int)eResponseStatus.PaymentGatewayNotExist, ERROR_PAYMENT_GATEWAY_NOT_EXIST);
                return response;
            }

            //check payment method exists
            if (!DAL.BillingDAL.GetPaymentMethod(groupID, paymentGatewayId, paymentMethodId))
            {
                response = new ApiObjects.Response.Status((int)eResponseStatus.PaymentMethodNotExist, ERROR_PAYMENT_METHOD_NOT_EXIST);
                return response;
            }

            if (DAL.BillingDAL.Delete_PaymentGatewayPaymentMethod(paymentMethodId))
            {
                response = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            }

            return response;
        }

        public ApiObjects.Response.Status DeletePaymentMethod(int paymentMethodId)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            if (paymentMethodId <= 0)
            {
                response = new ApiObjects.Response.Status((int)eResponseStatus.PaymentMethodIdRequired, PAYMENT_METHOD_ID_REQUIRED);
                return response;
            }

            if (DAL.BillingDAL.Delete_PaymentGatewayPaymentMethod(paymentMethodId))
            {
                response = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            }
            else
            {
                response = new ApiObjects.Response.Status((int)eResponseStatus.PaymentMethodNotExist, eResponseStatus.PaymentMethodNotExist.ToString());
            }

            return response;
        }

        public PaymentMethodsResponse GetPaymentGatewayPaymentMethods(int paymentGatewayId)
        {
            PaymentMethodsResponse response = new PaymentMethodsResponse()
            {
                Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString())
            };

            // validate parameters
            if (paymentGatewayId <= 0)
            {
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.PaymentGatewayIdRequired, PAYMENT_GATEWAY_ID_REQUIRED);
                return response;
            }

            //check payment gateway exists
            PaymentGateway paymentGateway = DAL.BillingDAL.GetPaymentGateway(groupID, paymentGatewayId, 1, 1);
            if (paymentGateway == null || paymentGateway.ID <= 0)
            {
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.PaymentGatewayNotExist, ERROR_PAYMENT_GATEWAY_NOT_EXIST);
                return response;
            }

            response.PaymentMethods = DAL.BillingDAL.Get_PaymentGatewayPaymentMethods(groupID, paymentGatewayId);
            if (response.PaymentMethods != null)
            {
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            }

            return response;
        }

        public ApiObjects.Response.Status SetPaymentMethodHouseholdPaymentGateway(int paymentGatewayId, string siteGuid, int householdId, int paymentMethodId)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            try
            {
                // paymentGatewayId validation: not empty
                if (paymentGatewayId == 0)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.PaymentGatewayIdRequired, PAYMENT_GATEWAY_ID_REQUIRED);
                    return response;
                }

                // paymentmethodId validation: not empty
                if (paymentMethodId == 0)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.PaymentMethodIdRequired, PAYMENT_METHOD_ID_REQUIRED);
                    return response;
                }

                // check user
                ApiObjects.Response.Status userStatus = Utils.ValidateUserAndDomain(groupID, siteGuid, ref householdId);

                if (userStatus.Code == (int)ResponseStatus.OK && householdId > 0)
                {
                    // paymentGatewayId validation: paymentGateway exist and active
                    PaymentGateway paymentGateway = DAL.BillingDAL.GetPaymentGateway(groupID, paymentGatewayId, 1, 1);
                    if (paymentGateway == null)
                    {
                        return new ApiObjects.Response.Status((int)eResponseStatus.PaymentGatewayNotValid, PAYMENT_GATEWAY_NOT_VALID);
                    }

                    if (!paymentGateway.SupportPaymentMethod)
                    {
                        return new ApiObjects.Response.Status((int)eResponseStatus.PaymentGatewayNotSupportPaymentMethod, PAYMENT_GATEWAY_NOT_SUPPORT_PAYMENT_METHOD);
                    }

                    HouseholdPaymentGateway householdPaymentGateway = DAL.BillingDAL.GetHouseholdPaymentGateway(groupID, paymentGatewayId, householdId, 1);
                    if (householdPaymentGateway == null)
                    {
                        return new ApiObjects.Response.Status((int)eResponseStatus.PaymentGatewayNotSetForHousehold, HOUSEHOLD_NOT_SET_TO_PAYMENT_GATEWAY);
                    }

                    PaymentGatewayHouseholdPaymentMethod pghpm = DAL.BillingDAL.GetPaymentGatewayHouseholdPaymentMethod(groupID, paymentGatewayId, householdId, paymentMethodId);
                    if (pghpm == null || pghpm.Id <= 0)
                    {
                        return new ApiObjects.Response.Status((int)eResponseStatus.PaymentMethodNotSetForHousehold, ERROR_NO_PG_PM_RELATED_TO_HOUSEHOLD);
                    }

                    int pghhpmId = DAL.BillingDAL.SetPaymentGatewayHouseholdPaymentMethod(groupID, paymentGatewayId, householdId, pghpm.PaymentMethodId, null, 1, pghpm.PaymentMethodExternalId);
                    if (pghhpmId > 0)
                    {
                        return new ApiObjects.Response.Status((int)eResponseStatus.OK, pghhpmId.ToString());
                    }
                    else
                    {
                        return new ApiObjects.Response.Status((int)eResponseStatus.ErrorSavingPaymentGatewayHouseholdPaymentMethod, ERROR_SAVING_PAYMENT_GATEWAY_HOUSEHOLD_PAYMENT_METHOD);
                    }
                }
                else if (householdId == 0)
                {
                    return userStatus;
                }
                else
                {
                    return new ApiObjects.Response.Status((int)eResponseStatus.Error, userStatus.Message.ToString());
                }

            }
            catch (Exception ex)
            {
                response = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.ErrorFormat("Failed groupID={0}, paymentGatewayId={1}, siteGuid= {2}, paymentMethodId {3}", groupID, paymentGatewayId, siteGuid, paymentMethodId, ex);
            }
            return response;
        }

        public ApiObjects.Response.Status RemovePaymentMethodHouseholdPaymentGateway(int paymentGatewayId, string siteGuid, int householdId, int paymentMethodId, bool force)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            try
            {
                // paymentGatewayId validation: not empty
                if (paymentGatewayId == 0)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.PaymentGatewayIdRequired, PAYMENT_GATEWAY_ID_REQUIRED);
                    return response;
                }

                // paymentmethodId validation: not empty
                if (paymentMethodId == 0)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.PaymentMethodIdRequired, PAYMENT_METHOD_ID_REQUIRED);
                    return response;
                }

                // check user
                ApiObjects.Response.Status userStatus = Utils.ValidateUserAndDomain(groupID, siteGuid, ref householdId);

                if (userStatus.Code == (int)ResponseStatus.OK && householdId > 0)
                {
                    // paymentGatewayId validation: paymentGateway exist and active
                    List<PaymentGateway> paymentpaymentGatewayList = DAL.BillingDAL.GetPaymentGatewaySettingsList(groupID, paymentGatewayId);

                    if (paymentpaymentGatewayList == null || paymentpaymentGatewayList.Count == 0 || paymentpaymentGatewayList[0] == null)
                    {
                        response = new ApiObjects.Response.Status((int)eResponseStatus.PaymentGatewayNotExist, ERROR_PAYMENT_GATEWAY_NOT_EXIST);
                        return response;
                    }

                    PaymentGateway paymentGateway = paymentpaymentGatewayList[0];

                    HouseholdPaymentGateway householdPaymentGateway = DAL.BillingDAL.GetHouseholdPaymentGateway(groupID, paymentGatewayId, householdId, 1);
                    if (householdPaymentGateway == null)
                    {
                        return new ApiObjects.Response.Status((int)eResponseStatus.PaymentGatewayNotSetForHousehold, HOUSEHOLD_NOT_SET_TO_PAYMENT_GATEWAY);
                    }

                    PaymentGatewayHouseholdPaymentMethod pghpm = DAL.BillingDAL.GetPaymentGatewayHouseholdPaymentMethod(groupID, paymentGatewayId, householdId, paymentMethodId);
                    if (pghpm == null || pghpm.Id <= 0)
                    {
                        return new ApiObjects.Response.Status((int)eResponseStatus.PaymentMethodNotSetForHousehold, ERROR_NO_PG_PM_RELATED_TO_HOUSEHOLD);
                    }

                    ApiObjects.Response.Status sendToAdapterStatus = SendRemoveHouseholdPaymentmethodToAdapter(householdPaymentGateway.ChargeId, householdId, paymentGateway, pghpm.PaymentMethodExternalId);

                    if (sendToAdapterStatus.Code != (int)eResponseStatus.OK)
                    {
                        return sendToAdapterStatus;
                    }

                    bool isSet = DAL.BillingDAL.RemovePaymentGatewayHouseholdPaymentMethod(paymentMethodId);
                    if (isSet)
                    {
                        return new ApiObjects.Response.Status((int)eResponseStatus.OK, "OK");
                    }
                    else
                    {
                        return new ApiObjects.Response.Status((int)eResponseStatus.Error, "Internal Error");
                    }
                }
                else if (householdId == 0)
                {
                    return userStatus;
                }
                else
                {
                    return new ApiObjects.Response.Status((int)eResponseStatus.Error, userStatus.Message.ToString());
                }

            }
            catch (Exception ex)
            {
                response = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.ErrorFormat("Failed groupID={0}, paymentGatewayId={1}, siteGuid= {2}, paymentMethodId {3}, ex: {4}", groupID, paymentGatewayId, siteGuid, paymentMethodId, ex);
            }
            return response;
        }

        public ApiObjects.Response.Status UpdateRecordedTransaction(int householdId, string externalTransactionId, string paymentDetails, string paymentMethod, int paymentGatewayId,
            string paymentMethodExternalId)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());

            string logData = string.Format("externalTransactionId: {0}, paymentDetails: {1}, paymentMethod: {2}, paymentGatewayId: {3}, paymentMethodExternalId: {4}",
               externalTransactionId, paymentDetails, paymentMethod, paymentGatewayId, paymentMethodExternalId);

            // get pg 
            PaymentGateway paymentGateway = DAL.BillingDAL.GetPaymentGateway(groupID, paymentGatewayId, 1, 1);

            if (paymentGateway == null)
            {
                log.DebugFormat("UpdateRecordedTransaction: payment gateway not found. id = {0}", paymentGatewayId);
                return new ApiObjects.Response.Status((int)eResponseStatus.PaymentGatewayNotExist, ERROR_PAYMENT_GATEWAY_NOT_EXIST);
            }

            if (paymentGateway.SupportPaymentMethod)
            {
                if (string.IsNullOrEmpty(paymentMethodExternalId))
                {
                    log.ErrorFormat("GetPaymentMethodExternalId PaymentMethodExternalIdRequired {0} ", logData);
                    return new ApiObjects.Response.Status() { Code = (int)eResponseStatus.PaymentMethodExternalIdRequired, Message = PAYMENT_METHOD_EXTERNAL_ID_REQUIRED };
                }

                PaymentGatewayHouseholdPaymentMethod pghhpm = DAL.BillingDAL.GetPaymentGatewayHouseholdPaymentMethod(groupID, paymentGatewayId, householdId, paymentMethodExternalId);

                if (pghhpm == null || pghhpm.Id <= 0)
                {
                    log.ErrorFormat("GetPaymentMethodExternalId payment method not exist {0}, log Data {1} ", paymentMethodExternalId, logData);
                    return new ApiObjects.Response.Status() { Code = (int)eResponseStatus.PaymentMethodNotExist, Message = PAYMENT_METHOD_NOT_EXIST };
                }

                if (DAL.BillingDAL.UpdatePaymentGatewayTransaction(groupID, paymentGatewayId, externalTransactionId, paymentDetails, paymentMethod, pghhpm.Id))
                {
                    return new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
            }

            return response;
        }

        public PaymentGatewayConfigurationResponse PaymentGatewayInvoke(int paymentGatewayId, string intent, List<ApiObjects.KeyValuePair> extraParams)
        {
            PaymentGatewayConfigurationResponse res = new PaymentGatewayConfigurationResponse();

            if (paymentGatewayId <= 0)
            {
                res.Status = new ApiObjects.Response.Status((int)eResponseStatus.PaymentGatewayNotExist, ERROR_PAYMENT_GATEWAY_NOT_EXIST);
                return res;
            }

            try
            {
                List<PaymentGateway> paymentpaymentGatewayList = DAL.BillingDAL.GetPaymentGatewaySettingsList(groupID, paymentGatewayId);

                PaymentGateway paymentGateway = null;

                if (paymentpaymentGatewayList == null || paymentpaymentGatewayList.Count == 0)
                {
                    res.Status = new ApiObjects.Response.Status((int)eResponseStatus.PaymentGatewayNotExist, ERROR_PAYMENT_GATEWAY_NOT_EXIST);
                    return res;
                }

                paymentGateway = paymentpaymentGatewayList[0];

                APILogic.PaymentGWAdapter.ConfigurationResponse adapterResponse = AdaptersController.GetInstance(paymentGateway.ID).GetAdapterConfiguration(paymentGateway, groupID, intent, extraParams);

                if (adapterResponse == null || adapterResponse.Status == null)
                {
                    log.Error("Error received while trying to get configuration");
                    res = new PaymentGatewayConfigurationResponse()
                    {
                        Status = new ApiObjects.Response.Status()
                        {
                            Code = (int)eResponseStatus.Error,
                            Message = "Error validating adapter response"
                        }
                    };
                }
                // validation OK -> continue
                // validation NOT OK -> validation response is final response
                else if (adapterResponse.Status.Code == (int)PaymentGatewayAdapterStatus.OK)
                {
                    if (adapterResponse.Configuration == null || adapterResponse.Configuration.Length == 0)
                        res.Configuration = new List<ApiObjects.KeyValuePair>();
                    else
                        res.Configuration = adapterResponse.Configuration.Select(x => new ApiObjects.KeyValuePair(x.Key, x.Value)).ToList();
                }

                res.Status = new ApiObjects.Response.Status();

                switch (adapterResponse.Status.Code)
                {
                    case (int)PaymentGatewayAdapterStatus.OK:
                        res.Status.Code = (int)eResponseStatus.OK;
                        break;
                    case (int)PaymentGatewayAdapterStatus.SignatureMismatch:
                        res.Status.Code = (int)eResponseStatus.SignatureMismatch;
                        break;
                    case (int)PaymentGatewayAdapterStatus.NoConfigurationFound:
                        res.Status.Code = (int)eResponseStatus.Error;
                        break;
                    default:
                        res.Status.Code = (int)eResponseStatus.Error;
                        break;
                }

                res.Status.Message = adapterResponse.Status.Message;
            }

            catch (Exception ex)
            {
                res.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.ErrorFormat("GetPaymentGatewayConfiguration Failed ex={0}", ex);
            }

            return res;
        }

        public HouseholdPaymentMethodResponse AddPaymentGatewayPaymentMethodToHousehold(HouseholdPaymentMethod paymentMethod, int householdId)
        {
            HouseholdPaymentMethodResponse response = new HouseholdPaymentMethodResponse() { Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString()) };

            try
            {
                if (paymentMethod == null)
                {
                    return response;
                }

                if (paymentMethod.PaymentMethodId == 0)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.PaymentMethodIdRequired, PAYMENT_METHOD_ID_REQUIRED);
                    return response;
                }

                if (paymentMethod.PaymentGatewayId == 0)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.PaymentGatewayIdRequired, PAYMENT_GATEWAY_ID_REQUIRED);
                    return response;
                }

                if (string.IsNullOrEmpty(paymentMethod.ExternalId))
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.PaymentMethodExternalIdRequired, PAYMENT_METHOD_EXTERNAL_ID_REQUIRED);
                    return response;
                }

                // check domain
                response.Status = Utils.ValidateDomain(groupID, householdId);

                if (response.Status.Code != (int)ResponseStatus.OK)
                {
                    return response;
                }

                // get paymentGatewayId according to paymentGateway external id.  
                //--------------------------------------------------------------
                //PaymentGateway paymentGateway = GetPaymentGatewayHousehold(groupID, externalIdentifier, householdId, paymentMethodName, paymentMethodExternalId, out paymentGatewayStatus);

                PaymentGatewayHouseholdPaymentMethod pghpm = DAL.BillingDAL.GetPaymentGatewayHouseholdByPaymentGatewayId(groupID, paymentMethod.PaymentGatewayId, householdId,
                    paymentMethod.PaymentMethodId, paymentMethod.ExternalId);
                if (pghpm == null || pghpm.PaymentGatewayId == 0)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.PaymentGatewayNotExist, ERROR_PAYMENT_GATEWAY_NOT_EXIST);
                    return response;
                }

                if (pghpm.HouseholdId == 0)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.PaymentGatewayNotSetForHousehold, ERROR_NO_PGW_RELATED_TO_HOUSEHOLD);
                    return response;
                }

                if (pghpm.PaymentMethodId == 0)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.PaymentMethodNotExist, ERROR_PAYMENT_METHOD_NOT_EXIST);
                    return response;
                }

                if (!string.IsNullOrEmpty(pghpm.PaymentMethodExternalId))
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.PaymentMethodAlreadySetToHouseholdPaymentGateway, PAYMENT_METHOD_ALREADY_SET_TO_HOUSEHOLD_PAYMENTGATEWAY);
                    return response;
                }

                response.PaymentMethod = DAL.BillingDAL.AddPaymentGatewayHouseholdPaymentMethod(groupID, householdId, paymentMethod.PaymentGatewayId, paymentMethod.PaymentMethodId, paymentMethod.ExternalId, paymentMethod.Details);

                if (response.PaymentMethod != null)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
                else
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.ErrorSavingPaymentGatewayHouseholdPaymentMethod, ERROR_SAVING_PAYMENT_GATEWAY_HOUSEHOLD_PAYMENT_METHOD);
                }
            }
            catch (Exception ex)
            {
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.Error(string.Format("Failed groupID={0}, paymentGaytewayId={1}, householdID={2}", groupID, paymentMethod.PaymentGatewayId, householdId), ex);
            }

            return response;
        }

        public ApiObjects.Response.Status RemoveAccount(int householdId)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());

            DataTable dt = DAL.BillingDAL.GetHouseholdPaymentMethods(householdId);

            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
            {
                PaymentGateway paymentGateway = null;
                int paymentGatewayId = 0;
                int householdPaymentMethodId = 0;
                string householdChargeId = null;
                string PaymentMethodExternalId;
                List<long> allPayentMethodsToDelete = new List<long>();

                Dictionary<int, KeyValuePair<PaymentGateway, List<string>>> householdPaymentgatewayPaymentMethodExternalIds = new Dictionary<int, KeyValuePair<PaymentGateway, List<string>>>();
                ApiObjects.Response.Status sendToAdapterStatus;

                foreach (DataRow row in dt.Rows)
                {
                    paymentGatewayId = ODBCWrapper.Utils.GetIntSafeVal(row, "paymentGatewayId");
                    householdPaymentMethodId = ODBCWrapper.Utils.GetIntSafeVal(row, "paymentMethodId");
                    householdChargeId = ODBCWrapper.Utils.GetSafeStr(row, "householdChargeId");
                    PaymentMethodExternalId = ODBCWrapper.Utils.GetSafeStr(row, "paymentMethodExternalId");

                    if (!householdPaymentgatewayPaymentMethodExternalIds.ContainsKey(paymentGatewayId))
                    {
                        // get the next payment gateway
                        paymentGateway = DAL.BillingDAL.GetPaymentGateway(groupID, paymentGatewayId);
                        householdPaymentgatewayPaymentMethodExternalIds.Add(paymentGatewayId, new KeyValuePair<PaymentGateway, List<string>>(paymentGateway, new List<string>()));
                    }

                    householdPaymentgatewayPaymentMethodExternalIds[paymentGatewayId].Value.Add(PaymentMethodExternalId);
                    allPayentMethodsToDelete.Add(householdPaymentMethodId);
                }

                foreach (var pg in householdPaymentgatewayPaymentMethodExternalIds.Values)
                {
                    // send remove account for the previous payment gateway
                    sendToAdapterStatus = SendRemoveAccountToAdapter(householdChargeId, householdId, pg.Key, pg.Value);
                }
                if (allPayentMethodsToDelete.Count > 0)
                {
                    int deleted = DAL.BillingDAL.DeleteHouseholdPaymentMethods(allPayentMethodsToDelete);
                    if (deleted >= 0)
                    {
                        log.DebugFormat("Successfully removed {0} payment methods for household = {1} from DB", deleted, householdId);
                    }
                    else
                    {
                        log.DebugFormat("Failed to remove payment methods for household = {0} from DB", householdId);
                    }
                }
            }

            return response;
        }

        private ApiObjects.Response.Status SendRemoveAccountToAdapter(string chargeId, long householdID, PaymentGateway paymentGateway, List<string> paymentMethodExternalIds)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status();

            string logString = string.Format("chargeId: {0}, householdID: {1}, paymentMethodExternalIds: {2}",
                chargeId != null ? chargeId : string.Empty,
                householdID,
                paymentMethodExternalIds != null ? string.Join(", ", paymentMethodExternalIds) : string.Empty);

            log.DebugFormat("RemoveAccount {0}", logString);

            if (string.IsNullOrEmpty(paymentGateway.AdapterUrl))
            {
                response = new ApiObjects.Response.Status((int)eResponseStatus.AdapterUrlRequired, ADAPTER_URL_REQUIRED);
                return response;
            }

            APILogic.PaymentGWAdapter.PaymentMethodResponse adapterResponse = AdaptersController.GetInstance(paymentGateway.ID).RemoveAccount(paymentGateway, groupID, chargeId, paymentMethodExternalIds);

            if (adapterResponse == null || adapterResponse.Status == null)
            {
                response = new ApiObjects.Response.Status()
                {
                    Code = (int)eResponseStatus.Error,
                    Message = "Error validating adapter response"
                };
            }

            if (adapterResponse.Status.Code == (int)PaymentGatewayAdapterStatus.OK)
            {
                if (adapterResponse.IsSuccess)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
                else
                {
                    log.ErrorFormat("RemoveAccount failed for charge id: {0}, payment methods: {1}, adapter Status: {2}, adapter Message: {3}",
                        chargeId, paymentMethodExternalIds != null ? string.Join(", ", paymentMethodExternalIds) : string.Empty, adapterResponse.PGStatus, adapterResponse.PGMessage);
                    response = new ApiObjects.Response.Status((int)eResponseStatus.Error, adapterResponse.PGMessage);
                }
            }
            else
            {
                switch (adapterResponse.Status.Code)
                {
                    case (int)PaymentGatewayAdapterStatus.NoConfigurationFound:
                        response.Code = (int)eResponseStatus.NoConfigurationFound;
                        response.Message = "Payment Gateway Adapter : No Configuration Found";
                        break;
                    case (int)PaymentGatewayAdapterStatus.SignatureMismatch:
                        response.Code = (int)eResponseStatus.SignatureMismatch;
                        response.Message = "Payment Gateway Adapter : Signature Mismatch";
                        break;
                    case (int)PaymentGatewayAdapterStatus.Error:
                    default:
                        response.Code = (int)eResponseStatus.Error;
                        response.Message = "Unknown Gateway adapter transaction error";
                        break;
                }
            }

            return response;
        }

        public Status ChangePaymentDetails(string billingGuid, long householdId, int newPaymentGatewayId, int newPaymentMethodId)
        {
            Status response = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            try
            {
                if (string.IsNullOrEmpty(billingGuid))
                {
                    log.ErrorFormat("error - billingGuid is null or empty");
                    response = new ApiObjects.Response.Status((int)eResponseStatus.Error, "error while getting payment details");
                    return response;
                }

                // check about payment gateway
                PaymentGateway newPaymentGateway = DAL.BillingDAL.GetPaymentGateway(groupID, newPaymentGatewayId);
                if (newPaymentGateway == null)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.PaymentGatewayNotExist, ERROR_PAYMENT_GATEWAY_NOT_EXIST);
                    return response;
                }

                bool isPaymentGWHouseholdExist = false;
                string chargeId = DAL.BillingDAL.GetPaymentGWChargeID(newPaymentGatewayId, householdId, ref isPaymentGWHouseholdExist);

                if (!isPaymentGWHouseholdExist)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.PaymentGatewayNotSetForHousehold, ERROR_NO_PGW_RELATED_TO_HOUSEHOLD);
                    return response;
                }

                if (newPaymentGateway.SupportPaymentMethod)
                {
                    if (newPaymentMethodId == 0)
                    {
                        response = new ApiObjects.Response.Status((int)eResponseStatus.PaymentMethodIdRequired, PAYMENT_METHOD_ID_REQUIRED);
                        return response;
                    }
                    // check if payment method valid for household
                    PaymentGatewayHouseholdPaymentMethod pghpm = null;
                    ApiObjects.Response.Status pghpmStatus = GetHouseholdPaymentGatewayPaymentMethod(householdId, newPaymentGatewayId, newPaymentMethodId, out pghpm);

                    if (pghpmStatus.Code != (int)eResponseStatus.OK)
                    {
                        response = pghpmStatus;
                        return response;
                    }
                }
                else if (newPaymentMethodId > 0)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.PaymentGatewayNotSupportPaymentMethod, PAYMENT_GATEWAY_NOT_SUPPORT_PAYMENT_METHOD);
                    return response;
                }

                // check if IsVerificationPaymentGateway
                if (IsVerificationPaymentGateway(newPaymentGateway))
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.PaymentGatewayNotValid, PAYMENT_GATEWAY_NOT_VALID);
                    return response;
                }

                if (string.IsNullOrEmpty(chargeId))
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.PaymentGatewayChargeIdRequired, ERROR_CHARGE_ID_MISSING);
                    return response;
                }

                List<PaymentDetails> paymentDetails = GetPaymentDetails(new List<string>() { billingGuid });               

                int currentPaymentGatewayId, currentPaymentMethodId;

                PaymentDetails pd = paymentDetails != null ? paymentDetails.Where(x => x.BillingGuid == billingGuid).FirstOrDefault() : null;

                bool SetPaymentDetails = false;
                if (pd == null)
                {
                    SetPaymentDetails = true;
                }
                else
                {
                    currentPaymentGatewayId = pd.PaymentGatewayId;
                    currentPaymentMethodId = pd.PaymentMethodId;

                    PaymentGateway currentPaymentGateway = DAL.BillingDAL.GetPaymentGateway(groupID, currentPaymentGatewayId);

                    // check if IsVerificationPaymentGateway
                    if (IsVerificationPaymentGateway(currentPaymentGateway))
                    {
                        response = new ApiObjects.Response.Status((int)eResponseStatus.PaymentGatewayNotValid, PAYMENT_GATEWAY_NOT_VALID);
                        return response;
                    }

                    // if diffrent - check if the new related to household payment gateway
                    if (newPaymentGatewayId != currentPaymentGatewayId || newPaymentMethodId != currentPaymentMethodId)
                    {
                        SetPaymentDetails = true;
                    }
                }
                if (SetPaymentDetails)
                {
                    if (DAL.BillingDAL.SetTransactionPaymentDetails(groupID, billingGuid, newPaymentGatewayId, newPaymentMethodId) == 0)
                    {
                        response = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                        log.ErrorFormat("error fail save new payment details groupID={0}, billingGuid={1}, newPaymentGatewayId={2}, newPaymentMethodId={3}",
                            groupID, billingGuid, newPaymentGatewayId, newPaymentMethodId);
                        return response;
                    }
                }

                response = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            }
            catch (Exception ex)
            {
                log.ErrorFormat("error - ChangePaymentDetails groupID={0}, billingGuid={1}, householdId={2}, newPaymentGatewayId={3}, newPaymentMethodId={4}, ex={5}",
                   groupID, billingGuid, householdId, newPaymentGatewayId, newPaymentMethodId, ex.Message);
                response = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            }

            return response;
        }


        public List<PaymentDetails> GetPaymentDetails(List<string> billingGuids)
        {
            List<PaymentDetails> response = new List<PaymentDetails>();
            try
            {
                // get payment GW ID               
                DataSet ds = DAL.BillingDAL.GetTransactionPaymentDetails(billingGuids);
                if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
                {
                    PaymentDetails paymentDetails = new PaymentDetails();
                    
                    List<DataRow> rpd = new List<DataRow>();
                    List<DataRow> pgt = new List<DataRow>();
                    if (ds.Tables.Count > 1 && Utils.DataTableExsits(ds.Tables[1]))
                    {
                        rpd = ds.Tables[1].AsEnumerable().ToList();
                    }
                    if (ds.Tables.Count > 0 && Utils.DataTableExsits(ds.Tables[0]))
                    {
                        pgt = ds.Tables[0].AsEnumerable().ToList();
                    }

                    if (rpd.Count > 0)
                    {
                        foreach (DataRow dr in rpd)
                        {
                            paymentDetails = new PaymentDetails();

                            paymentDetails.PaymentGatewayId = ODBCWrapper.Utils.GetIntSafeVal(dr, "payment_gateway_id");
                            paymentDetails.PaymentMethodId = ODBCWrapper.Utils.GetIntSafeVal(dr, "payment_method_id");
                            paymentDetails.BillingGuid = ODBCWrapper.Utils.GetSafeStr(dr, "billing_guid");
                            //get external transaction from pgt                             

                            DataRow dataRow = pgt.Where(y => y.Field<string>("billing_guid") == paymentDetails.BillingGuid).FirstOrDefault();
                            if (dataRow != null)
                            {
                                paymentDetails.TransactionId = dataRow != null ? ODBCWrapper.Utils.GetSafeStr(dataRow, "external_transaction_id") : string.Empty;
                                pgt.Remove(dataRow);
                            }

                            response.Add(paymentDetails);
                        }
                    }

                    if (pgt.Count > 0)
                    {
                        foreach (DataRow dr in pgt)
                        {
                            paymentDetails = new PaymentDetails();

                            paymentDetails.PaymentGatewayId = ODBCWrapper.Utils.GetIntSafeVal(dr, "payment_gateway_id");
                            paymentDetails.PaymentMethodId = ODBCWrapper.Utils.GetIntSafeVal(dr, "payment_method_id");
                            paymentDetails.TransactionId = ODBCWrapper.Utils.GetSafeStr(dr, "external_transaction_id");
                            paymentDetails.BillingGuid = ODBCWrapper.Utils.GetSafeStr(dr, "billing_guid");

                            response.Add(paymentDetails);

                            // to do create thid one to get multi 
                            DAL.BillingDAL.SetTransactionPaymentDetails(groupID, paymentDetails.BillingGuid, paymentDetails.PaymentGatewayId, paymentDetails.PaymentMethodId);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("GetPaymentDetails groupId={0} , billingGuids={1}", groupID, string.Join(",", billingGuids)), ex);
                response = new List<PaymentDetails>();
            }
            return response;
        }

        public Status GetPaymentGatewayVerificationStatus(string billingGuid)
        {
            Status response = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());

            List<PaymentDetails> paymentDetails = GetPaymentDetails(new List<string>() { billingGuid });               

            int currentPaymentGatewayId, currentPaymentMethodId;

            PaymentDetails pd = paymentDetails != null ? paymentDetails.Where(x => x.BillingGuid == billingGuid).FirstOrDefault() : null;

            if (pd != null)
            {
                currentPaymentGatewayId = pd.PaymentGatewayId;
                currentPaymentMethodId = pd.PaymentMethodId;

                PaymentGateway currentPaymentGateway = DAL.BillingDAL.GetPaymentGateway(groupID, currentPaymentGatewayId);

                // check if IsVerificationPaymentGateway
                if (IsVerificationPaymentGateway(currentPaymentGateway))
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.PaymentGatewayNotValid, "Payment gateway is not valid for action");
                }
            }
            return response;
        }
    }
}
